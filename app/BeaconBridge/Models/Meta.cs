using BeaconBridge.Constants;

namespace BeaconBridge.Models;

public class Meta
{
  public string BeaconId { get; set; } = string.Empty;
  
  public string ApiVersion { get; set; } = string.Empty;
  
  public string ReturnedGranularity { get; set; } = Granularity.Boolean;
  
  public RequestSummary ReceivedRequestSummary { get; set; } = new();
  
  public List<Dictionary<string,string>> ReturnedSchemas { get; set; } = new();
}
