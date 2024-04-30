using System.Text.Json.Serialization;
using BeaconBridge.Config;

namespace BeaconBridge.Models;

public class BaseResponse
{
  [JsonPropertyName("id")] 
  public string Id { get; set; } = string.Empty;
  
  [JsonPropertyName("name")] 
  public string Name { get; set; } = string.Empty;
  
  [JsonPropertyName("apiVersion")] 
  public string ApiVersion { get; set; } = string.Empty;
  
  [JsonPropertyName("environment")] 
  public string Environment { get; set; } = string.Empty;

  [JsonPropertyName("organization")] 
  public OrganisationOptions Organisation { get; set; } = new();
  
  [JsonPropertyName("description")] 
  public string Description { get; set; } = string.Empty;
  
  [JsonPropertyName("version")] 
  public string Version { get; set; } = string.Empty;
  
  [JsonPropertyName("welcomeUrl")] 
  public string WelcomeUrl { get; set; } = string.Empty;
  
  [JsonPropertyName("alternativeUrl")] 
  public string AlternativeUrl { get; set; } = string.Empty;
  [JsonPropertyName("createDateTime")] 
  public DateTime CreateDateTime { get; set; } = new DateTime(2024, 2, 23, 15, 24, 16);
  
  [JsonPropertyName("updateDateTime")] 
  public DateTime UpdateDateTime { get; set; } = new DateTime(2024, 2, 26, 11, 30, 12);

  [JsonPropertyName("datasets")] 
  public List<string> Datasets { get; set; } = new();
  
}
