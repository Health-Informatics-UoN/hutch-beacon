namespace BeaconBridge.Config;

public class WorkflowCrateOptions
{
  /// <summary>
  /// <para>The time in seconds between each update attempt.</para>
  /// <para>Default: 86,400s (1 day)</para>
  /// </summary>
  public int DelaySeconds { get; set; } = 60 * 60 * 24;

  /// <summary>
  /// The path to the workflow crate. It could be a link to it on WorkflowHub or a file on disk.
  /// </summary>
  public string PathToWorkflow { get; set; } = string.Empty;

  public string PathToResults { get; set; } = string.Empty;
  /// <summary>
  /// The expected file name for the workflow output.
  /// </summary>
  public string ExpectedOutputFileName { get; set; } = "output.json";
}
