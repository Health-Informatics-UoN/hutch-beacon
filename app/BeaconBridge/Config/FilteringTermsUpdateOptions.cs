namespace BeaconBridge.Config;

/// <summary>
/// Options for updating the filtering terms cache.
/// </summary>
public class FilteringTermsUpdateOptions
{
  /// <summary>
  /// <para>The time in seconds between each update attempt.</para>
  /// <para>Default: 86,400s (1 day)</para>
  /// </summary>
  public int DelaySeconds { get; set; } = 60 * 60 * 24;

  /// <summary>
  /// The path to the Filtering Terms workflow. It could be a link to it on WorkflowHub or a file on disk.
  /// </summary>
  public string PathToWorkflow { get; set; } = string.Empty;

  /// <summary>
  /// Save results from Filtering Terms workflow to this destination.
  /// </summary>
  public string PathToResults { get; set; } = string.Empty;

  /// <summary>
  /// The expected file name for the filtering terms workflow output.
  /// </summary>
  public string ExpectedOutputFileName { get; set; } = "output.json";
}
