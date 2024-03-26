using BeaconBridge.Constants.Submission;

namespace BeaconBridge.Data.Entities;

public class MembershipTreDecision
{
  public int Id { get; set; }
  public Project? SubmissionProj { get; set; }
  public User? User { get; set; }
  public Tre? Tre { get; set; }
  public Decision Decision { get; set; }
}
