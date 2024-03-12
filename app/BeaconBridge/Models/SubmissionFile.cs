using BeaconBridge.Constants;

namespace BeaconBridge.Models;

public class SubmissionFile
{
  public int Id { get; set; }
  public string Name { get; set; }
  public string TreBucketFullPath { get; set; }
  public string SubmisionBucketFullPath { get; set; }
  public FileStatus Status { get; set; }
  public string Description { get; set; }
  public virtual BL.Models.Submission Submission { get; set; }

}
