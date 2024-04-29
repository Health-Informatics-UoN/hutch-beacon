using System.Text.RegularExpressions;
using BeaconBridge.Config;
using BeaconBridge.Constants;
using BeaconBridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Controllers;

[ApiController]
[Route("api/")]
public class EntryTypeController(IOptions<BeaconInfoOptions> beaconInfoOptions, IMemoryCache memoryCache
)
{
  private readonly BeaconInfoOptions _beaconInfoOptions = beaconInfoOptions.Value;

  [HttpGet("individuals")]
  public ActionResult<EntryTypeResponse> GetIndividuals([FromQuery] string? filters,
    [FromQuery] string? requestedSchema,
    [FromQuery] int skip = 0, [FromQuery] int limit = 10)
  {
    var individualsResponse = new EntryTypeResponse()
    {
      Meta =
      {
        BeaconId = _beaconInfoOptions.BeaconId,
        ApiVersion = _beaconInfoOptions.ApiVersion,
        ReceivedRequestSummary = new RequestSummary()
        {
          ApiVersion = _beaconInfoOptions.ApiVersion,
          Pagination = new Pagination() { Limit = limit, Skip = skip }
        }
      },
      ReturnedSchemas =
      {
        EntityType = EntityTypes.Individuals,
        Schema = Schemas.Individuals
      }
    };
    if (filters is not null)
    {
      // split filters
      Regex regex = new Regex(",");
      string[] filterList = regex.Split(filters);

      foreach (var match in filterList) individualsResponse.Meta.ReceivedRequestSummary.Filters.Add(match);

      if (filters.Contains("Gender:F") && filters.Contains("SNOMED:386661006") && filters.Contains("SNOMED:271825005"))
      {
        individualsResponse.ResponseSummary.Exists = true;
      }
      else
      {
        var random = new Random();
        // check if key in cache
        if (!memoryCache.TryGetValue(filters, out var  randomBool))
        {
          // set value
          randomBool = random.Next(2) == 1;
          // set data in cache
          memoryCache.Set(filters, randomBool);
        }
        Boolean.TryParse(memoryCache.Get(filters)?.ToString(), out var exists);
        individualsResponse.ResponseSummary.Exists = exists;
      }
    }
    return individualsResponse;
  }
}
