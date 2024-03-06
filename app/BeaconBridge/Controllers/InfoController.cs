using BeaconBridge.Config;
using BeaconBridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Controllers;

[ApiController]
[Route("api/")]
public class InfoController: ControllerBase
{
  private readonly BeaconInfoOptions _beaconInfoOptions;
  private readonly OrganisationOptions _organisationOptions;
  

  public InfoController(IOptions<BeaconInfoOptions> beaconInfoOptions, 
    IOptions<OrganisationOptions> organisationOptions)
  {
    _organisationOptions = organisationOptions.Value;
    _beaconInfoOptions = beaconInfoOptions.Value;
  }

  [HttpGet,Route("")]
  public ActionResult<Info> Get()
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
        AlternativeUrl = _beaconInfoOptions.AlternativeUrl
      }
    };
    return info;
  }
  
  [HttpGet,Route("[controller]")]
  public ActionResult<Info> GetInfo()
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
        AlternativeUrl = _beaconInfoOptions.AlternativeUrl
      }
    };
    return info;
  }
}
