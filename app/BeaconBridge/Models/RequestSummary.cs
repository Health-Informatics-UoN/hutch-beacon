using System.Text.Json.Serialization;
using BeaconBridge.Constants;

namespace BeaconBridge.Models;

public class RequestSummary
{
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]  
  public string? ApiVersion { get; set; }
  
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]  
  public string? Schemas { get; set; }
  
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]  
  public List<string>? Filters { get; set; }
  
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]  
  public string? Parameters { get; set; }
  
  public string? IncludeResultSetResponses { get; set; } = ResultsetResponses.Hit;

  public Dictionary<string, int>? Pagination { get; set; } = new()
  {
    { "skip", 0 },
    { "limit", 10 }
  };
  
  public string? Granularity { get; set; } = Constants.Granularity.Boolean;
  
  public bool TestMode { get; set; } = false;

}
