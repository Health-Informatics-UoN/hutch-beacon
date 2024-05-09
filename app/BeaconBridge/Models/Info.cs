using System.Text.Json.Serialization;

namespace BeaconBridge.Models;

public class Info
{
  [JsonPropertyName("meta")] public BaseMeta BaseMeta { get; set; } = new();

  [JsonPropertyName("response")] public BaseResponse BaseResponse { get; set; } = new();
}
