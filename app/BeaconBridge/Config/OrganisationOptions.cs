using System.Text.Json.Serialization;

namespace BeaconBridge.Config;

public class OrganisationOptions
{
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? Id { get; set; } = string.Empty;

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? Name { get; set; } = string.Empty;

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
  public string? Description { get; set; } = string.Empty;
  
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? Address { get; set; } = string.Empty;
  
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? WelcomeUrl { get; set; } = string.Empty;

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? ContactUrl { get; set; } = string.Empty;
  
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]  
  public string? LogoUrl { get; set; } = string.Empty;
}
