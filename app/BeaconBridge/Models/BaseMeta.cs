

namespace BeaconBridge.Models;

public class BaseMeta
{
  public string BeaconId { get; set; } = string.Empty;
  
  public string ApiVersion { get; set; } = string.Empty;
  
  public List<Dictionary<string,string>> ReturnedSchemas { get; set; } = new();
}
