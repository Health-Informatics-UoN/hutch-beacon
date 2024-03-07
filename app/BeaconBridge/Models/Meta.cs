using System.Text.Json.Serialization;

namespace BeaconBridge.Models;

public class Meta
{
  [JsonPropertyName("beaconId")] 
  public string BeaconId { get; set; } = string.Empty;
  
  [JsonPropertyName("apiVersion")] 
  public string ApiVersion { get; set; } = string.Empty;

  [JsonPropertyName("returnedGranularity")]
  public string Granularity { get; set; } = string.Empty;

  [JsonPropertyName("receivedRequestSummary")]
  public RequestSummary RequestSummary { get; set; } = new();

  [JsonPropertyName("returnedSchemas")]
  public string ReturnedSchemas { get; set; } = string.Empty;
}
