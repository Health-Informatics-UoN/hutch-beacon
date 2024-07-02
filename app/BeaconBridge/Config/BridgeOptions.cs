namespace BeaconBridge.Config;

public class BridgeOptions
{
  /// <summary>
  /// A path for BeaconBridge to use as a working directory.
  /// </summary>
  public string WorkingDirectoryBase { get; set; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "bridge-workdir");
}
