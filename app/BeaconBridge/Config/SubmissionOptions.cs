namespace BeaconBridge.Config;

public class SubmissionOptions
{
  /// <summary>
  /// The name of the project to submit requests against on the Submission Layer.
  /// </summary>
  public string ProjectName { get; set; } = string.Empty;

  /// <summary>
  /// The list of TREs to submit the requests against on the Submission Layer.
  /// </summary>
  public List<string> Tres { get; set; } = new();
}
