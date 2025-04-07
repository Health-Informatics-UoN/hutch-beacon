namespace BeaconBridge.Config;

public class TesTaskOptions
{
  public Outputs Outputs { get; set; } = new();
  public BeaconImage BeaconImage { get; set; } = new();
  public Dictionary<string, string> Env { get; set; } = new();
}

public class Outputs
{
  /// <summary>
  /// Gets or Sets Name
  /// </summary>
  public string Name { get; set; } = string.Empty;
  
  /// <summary>
  /// URL in long term storage, for example: s3://my-object-store/file1 file:///path/to/my/file etc.
  /// </summary>
  public string Url { get; set; } = string.Empty;
  
  /// <summary>
  /// Path of the file inside the container. Must be an absolute path.
  /// </summary>
  public string Path { get; set; } = string.Empty;
}

public class BeaconImage
{
  /// <summary>
  /// Beacon Image Name
  /// </summary>
  public string? Image { get; set; }

  /// <summary>
  /// Beacon Image Version tag
  /// </summary>
  public string? Version { get; set; }
}
