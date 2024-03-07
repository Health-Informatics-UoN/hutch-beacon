using BeaconBridge.Config;
using BeaconBridge.Models;
using BeaconBridge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Controllers;

[ApiController]
[Route("api/")]
public class InfoController(IOptions<BeaconInfoOptions> beaconInfoOptions,
    IOptions<OrganisationOptions> organisationOptions,InfoService infoService)
  : ControllerBase
{
  private readonly BeaconInfoOptions _beaconInfoOptions = beaconInfoOptions.Value;
  private readonly OrganisationOptions _organisationOptions = organisationOptions.Value;
  private readonly InfoService _infoService = infoService;

  [HttpGet,Route("")]
  public ActionResult<Info> Get([FromQuery] string requestedSchema)
  {
    var info = new Info()
    {
      Meta =
      {
        ApiVersion = _beaconInfoOptions.ApiVersion,
        BeaconId = _beaconInfoOptions.BeaconId,
        Granularity = _beaconInfoOptions.Granularity,
        RequestSummary = _infoService.SetSummary(requestedSchema)
      },
      Response =
      {
        Id = _beaconInfoOptions.BeaconId,
        Name = _beaconInfoOptions.Name,
        ApiVersion = _beaconInfoOptions.ApiVersion,
        Environment = _beaconInfoOptions.Environment,
        Organisation = _organisationOptions,
        Description = _beaconInfoOptions.Description,
        Version = _beaconInfoOptions.Version,
        WelcomeUrl = _beaconInfoOptions.WelcomeUrl,
        AlternativeUrl = _beaconInfoOptions.AlternativeUrl,
      }
    };
    return info;
  }
  
  [HttpGet,Route("[controller]")]
  public ActionResult<Info> GetInfo([FromQuery] string requestedSchema)
  {
    var info = new Info()
    {
      Meta =
      {
        ApiVersion = _beaconInfoOptions.ApiVersion,
        BeaconId = _beaconInfoOptions.BeaconId,
        Granularity = _beaconInfoOptions.Granularity,
        RequestSummary = _infoService.SetSummary(requestedSchema)
      },
      Response =
      {
        Id = _beaconInfoOptions.BeaconId,
        Name = _beaconInfoOptions.Name,
        ApiVersion = _beaconInfoOptions.ApiVersion,
        Environment = _beaconInfoOptions.Environment,
        Organisation = _organisationOptions,
        Description = _beaconInfoOptions.Description,
        Version = _beaconInfoOptions.Version,
        WelcomeUrl = _beaconInfoOptions.WelcomeUrl,
        AlternativeUrl = _beaconInfoOptions.AlternativeUrl
      }
    };
    return info;
  }
}
