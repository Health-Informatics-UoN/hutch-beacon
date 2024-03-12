using BeaconBridge.Constants;

namespace BeaconBridge.Models;

public class MembershipTreDecision
{
  public int Id { get; set; }
  public virtual Project? SubmissionProj { get; set; }

  public virtual User? User { get; set; }
  public virtual Tre? Tre { get; set; }
  public Decision Decision { get; set; }
}
