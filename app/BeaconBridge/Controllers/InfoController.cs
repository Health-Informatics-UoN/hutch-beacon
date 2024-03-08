using BeaconBridge.Config;
using BeaconBridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Controllers;

[ApiController]
[Route("api/")]
public class InfoController(IOptions<BeaconInfoOptions> beaconInfoOptions,
    IOptions<OrganisationOptions> organisationOptions)
  : ControllerBase
{
  private readonly BeaconInfoOptions _beaconInfoOptions = beaconInfoOptions.Value;
  private readonly OrganisationOptions _organisationOptions = organisationOptions.Value;
  [HttpGet,Route(""),Route("info")]
  public ActionResult<Info> Get([FromQuery] string requestedSchema)
  {
    var info = new Info()
    {
      Meta =
      {
        ApiVersion = _beaconInfoOptions.ApiVersion,
        BeaconId = _beaconInfoOptions.BeaconId,
        Granularity = _beaconInfoOptions.Granularity
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
}
