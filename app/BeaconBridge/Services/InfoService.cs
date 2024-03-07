using BeaconBridge.Config;
using BeaconBridge.Constants;
using BeaconBridge.Models;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Services;

public class InfoService(IOptions<BeaconInfoOptions> options)
{
  private readonly BeaconInfoOptions _options = options.Value;

  public RequestSummary SetSummary(string queryParameters)
  {
    var requestSummary = new RequestSummary()
    {
      ApiVersion = _options.ApiVersion,
      Granularity = Granularity.Boolean,
      Schemas = queryParameters
    };
    return requestSummary;
  }
}
