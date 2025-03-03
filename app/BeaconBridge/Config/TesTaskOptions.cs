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
  public string? Name { get; set; }

  /// <summary>
  /// URL in long term storage, for example: s3://my-object-store/file1 gs://my-bucket/file2 file:///path/to/my/file /path/to/my/file etc...
  /// </summary>
  /// <value>URL in long term storage, for example: s3://my-object-store/file1 gs://my-bucket/file2 file:///path/to/my/file /path/to/my/file etc...</value>
  public string? Url { get; set; }

  /// <summary>
  /// Path of the file inside the container. Must be an absolute path.
  /// </summary>
  /// <value>Path of the file inside the container. Must be an absolute path.</value>
  public string? Path { get; set; }
}

public class BeaconImage
{
  public string? Image { get; set; }

  public string? Version { get; set; }
}
