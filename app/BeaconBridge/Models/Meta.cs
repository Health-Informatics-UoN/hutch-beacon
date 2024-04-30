using BeaconBridge.Constants;

namespace BeaconBridge.Models;

public class Meta: BaseMeta
{
  
  public string ReturnedGranularity { get; set; } = Granularity.Boolean;
  
  public RequestSummary ReceivedRequestSummary { get; set; } = new();
  
}
