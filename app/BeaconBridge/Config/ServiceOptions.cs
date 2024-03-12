namespace BeaconBridge.Config;

public class ServiceOptions
{
  public Type Type { get; set; } = new();
  public string Description { get; set; } = string.Empty;
  public string ContactUrl { get; set; } = string.Empty;
  public string DocumentationUrl { get; set; } = string.Empty;
  public string Version { get; set; } = string.Empty;

}
public class Type
{
  public string Group { get; set; } = string.Empty;
  public string Artifact { get; set; } = string.Empty;
  public string Version { get; set; } = string.Empty;

}
