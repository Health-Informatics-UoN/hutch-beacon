using BeaconBridge.Constants.Submission;

namespace BeaconBridge.Data.Entities.Submission;

public class ProjectTreDecision
{
  public int Id { get; set; }
  public Project? SubmissionProj { get; set; }
  public Tre? Tre { get; set; }
  public Decision Decision { get; set; }
}
