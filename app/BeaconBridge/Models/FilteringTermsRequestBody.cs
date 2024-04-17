using System.Text.Json.Serialization;

namespace BeaconBridge.Models;

public class FilteringTermsRequestBody
{
  [JsonPropertyName("skip")] public int Skip { get; set; } = 0;

  [JsonPropertyName("limit")] public int Limit { get; set; } = 10;
}
