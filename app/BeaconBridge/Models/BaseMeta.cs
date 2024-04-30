

namespace BeaconBridge.Models;

public class BaseMeta
{
  public string BeaconId { get; set; } = string.Empty;
  
  public string ApiVersion { get; set; } = string.Empty;
  
  public List<ReturnedSchema> ReturnedSchemas { get; set; } = new();
}
