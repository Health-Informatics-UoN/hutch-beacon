namespace BeaconBridge.Models;
using System.Text.Json.Serialization;

public class ResponseSummary
{
  [JsonPropertyName("exists")] public bool Exists { get; set; } = false;
}
