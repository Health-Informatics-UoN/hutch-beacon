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
}
