using System.Text.Json.Serialization;

namespace BeaconBridge.Models;

public class BaseMeta
{
  [JsonPropertyName("beaconId")] 
  public string BeaconId { get; set; } = string.Empty;
  
  [JsonPropertyName("apiVersion")] 
  public string ApiVersion { get; set; } = string.Empty;

  [JsonPropertyName("returnedSchemas")] 
  public List<string> ReturnedSchemas { get; set; } = new();
}
