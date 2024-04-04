using BeaconBridge.Config;
using BeaconBridge.Constants;
using BeaconBridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Controllers;

[ApiController]
[Route("api/")]
public class InfoController(IOptions<BeaconInfoOptions> beaconInfoOptions,
    IOptions<OrganisationOptions> organisationOptions, IOptions<ServiceOptions> serviceOptions)
  : ControllerBase
{
  private readonly BeaconInfoOptions _beaconInfoOptions = beaconInfoOptions.Value;
  private readonly OrganisationOptions _organisationOptions = organisationOptions.Value;
  private readonly ServiceOptions _serviceOptions = serviceOptions.Value;

  [HttpGet, Route(""), Route("info")]
  public ActionResult<Info> Get([FromQuery] string? requestedSchema)
  {
    var info = new Info
    {
      BaseMeta =
      {
        ApiVersion = _beaconInfoOptions.ApiVersion,
        BeaconId = _beaconInfoOptions.BeaconId
      },
      BaseResponse =
      {
        Id = _beaconInfoOptions.BeaconId,
        Name = _beaconInfoOptions.Name,
        ApiVersion = _beaconInfoOptions.ApiVersion,
        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!,
        Organisation = _organisationOptions,
        Description = _beaconInfoOptions.Description,
        Version = _beaconInfoOptions.Version,
        WelcomeUrl = _beaconInfoOptions.WelcomeUrl,
        AlternativeUrl = _beaconInfoOptions.AlternativeUrl,
      }
    };
    return info;
  }

  [HttpGet("service-info")]
  public ActionResult<ServiceInfo> GetServiceInfo()
  {
    var serviceInfo = new ServiceInfo()
    {
      Id = _beaconInfoOptions.BeaconId,
      Name = _beaconInfoOptions.Name,
      Type = _serviceOptions.Type,
      Description = _beaconInfoOptions.Description,
      Organisation =
      {
        Id = null,
        Name = _organisationOptions.Name,
        WelcomeUrl = _organisationOptions.WelcomeUrl,
        Address = null,
        ContactUrl = null,
        Description = null,
        LogoUrl = null
      },
      ContactUrl = _organisationOptions.ContactUrl,
      DocumentationUrl = _serviceOptions.DocumentationUrl,
      Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!,
      Version = _beaconInfoOptions.Version
    };
    return serviceInfo;
  }

  /// <summary>
  /// Get the list of filtering terms handled by this beacon implementation.
  /// </summary>
  /// <param name="skip">Number of pages to skip</param>
  /// <param name="limit">Size of the page. Use 0 to return all the results or the maximum allowed by the Beacon.
  /// </param>
  /// <returns></returns>
  [HttpGet("filtering_terms")]
  public ActionResult<FilterResponse> GetFilteringTerms([FromQuery] int skip = 0, [FromQuery] int limit = 10)
  {
    var filterResponse = new FilterResponse()
    {
      Meta = new()
      {
        BeaconId = _beaconInfoOptions.BeaconId,
        ApiVersion = _beaconInfoOptions.ApiVersion,
        ReceivedRequestSummary = new RequestSummary()
        {
          ApiVersion = _beaconInfoOptions.ApiVersion,
          Filters = null,
          Pagination = new Pagination() { Limit = limit, Skip = skip }
        }
      },
      Response = new List<FilteringTerm>
      {
        // set to static value for now
        new()
        {
          Type = "type",
          Id = "id",
          Label = "1",
          Scope = "individuals"
        }
      }
    };
    filterResponse.Meta.ReturnedSchemas.Add(new DefaultSchemas().FilteringTerms);
    return filterResponse;
  }
}
