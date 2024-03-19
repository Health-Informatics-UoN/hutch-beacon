using BeaconBridge.Config;
using Type = BeaconBridge.Config.Type;

namespace BeaconBridge.Models;

public class ServiceInfo
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public Type Type { get; set; } = new();
  public string Description { get; set; } = string.Empty;
  public OrganisationOptions Organisation { get; set; } = new();
  public string? ContactUrl { get; set; } = string.Empty;
  public string DocumentationUrl { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; } = new();
  public DateTime UpdatedAt { get; set; } = new();
  public string Environment { get; set; } = string.Empty;
  public string Version { get; set; } = string.Empty;
}

