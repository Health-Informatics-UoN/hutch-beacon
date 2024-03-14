using BeaconBridge.Config;
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
      Response =
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
}
