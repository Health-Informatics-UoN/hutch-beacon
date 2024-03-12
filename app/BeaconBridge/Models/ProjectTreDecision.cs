using BeaconBridge.Constants;

namespace BeaconBridge.Models;

public class ProjectTreDecision
{
  public int Id { get; set; }
  public virtual Project? SubmissionProj { get; set; }
  public virtual Tre? Tre { get; set; }
  public Decision Decision { get; set; }
}
