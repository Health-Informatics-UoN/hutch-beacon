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

  public Pagination Pagination { get; set; } = new();
  
  public string? Granularity { get; set; } = Constants.Granularity.Boolean;
  
  public bool TestMode { get; set; } = false;

}

public class Pagination
{
  public int Skip { get; set; }

  public int Limit { get; set; }
}
