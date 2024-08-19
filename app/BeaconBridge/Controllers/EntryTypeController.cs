using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using BeaconBridge.Config;
using BeaconBridge.Constants;
using BeaconBridge.Constants.Submission;
using BeaconBridge.Models;
using BeaconBridge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace BeaconBridge.Controllers;

[ApiController]
[Route("api/")]
public class EntryTypeController(
  IOptions<BeaconInfoOptions> beaconInfoOptions,
  IOptions<BridgeOptions> bridgeOptions,
  CrateGenerationService crateGenerationService,
  IFeatureManager featureFlags,
  CrateSubmissionService crateSubmissionService,
  TesSubmissionService tesSubmissionService)
{
  private readonly BeaconInfoOptions _beaconInfoOptions = beaconInfoOptions.Value;
  private readonly BridgeOptions _bridgeOptions = bridgeOptions.Value;

  [HttpGet("individuals")]
  public async Task<ActionResult<EntryTypeResponse>> GetIndividuals([FromQuery] string? filters,
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
      }
    };
    individualsResponse.Meta.ReturnedSchemas.Add(new ReturnedSchema()
      { EntityType = EntityTypes.Individuals, Schema = Schemas.Individuals });
    
    if (filters is not null)
    {
      var beaconTaskId = Guid.NewGuid().ToString();
      var bagItPath = Path.Combine(_bridgeOptions.WorkingDirectoryBase, beaconTaskId);
      // Build RO-Crate
      var archive = await crateGenerationService.BuildCrate(filters, bagItPath);
      // Assess RO-Crate
      if (await featureFlags.IsEnabledAsync(FeatureFlags.MakeAssessActions))
        await crateGenerationService.AssessBagIt(archive);
      // Zip the BagIt package
      if (!Directory.Exists(bagItPath))
        Directory.CreateDirectory(bagItPath);
      var fileName = bagItPath + ".zip";
      ZipFile.CreateFromDirectory(bagItPath, fileName);

      //Submit Crate
      var tesTask = await crateSubmissionService.SubmitCrate(bagItPath, beaconTaskId);
      // await tesTaskService.Create(tesTask);

      // Poll for results
      // Start 5-minute timer
      Stopwatch timer = new Stopwatch();
      timer.Start();
      while (timer.Elapsed.TotalSeconds < 300)
      {
        var submissionStatus = await tesSubmissionService.CheckStatus(tesTask);
        Thread.Sleep(1000);
        if (submissionStatus == StatusType.Completed)
        {
          var responseSummary = await tesSubmissionService.DownloadResults(tesTask);
          individualsResponse.ResponseSummary = responseSummary;
          break;
        }
      }

      timer.Stop();

      // split filters
      Regex regex = new Regex(",");
      string[] filterList = regex.Split(filters);

      foreach (var match in filterList) individualsResponse.Meta.ReceivedRequestSummary.Filters.Add(match);
    }

    return individualsResponse;
  }
}
