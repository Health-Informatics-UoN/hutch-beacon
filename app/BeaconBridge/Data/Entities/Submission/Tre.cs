namespace BeaconBridge.Data.Entities.Submission;

public class Tre
{
  public int Id { get; set; }        
  public List<Project> Projects { get; set; }
  public string Name { get; set; }
  public DateTime LastHeartBeatReceived { get; set; }
  public string AdminUsername { get; set; }
  public string About {  get; set; }
  public string FormData { get; set; }
  public List<Submission> Submissions { get; set; }
  public List<ProjectTreDecision> ProjectTreDecisions { get; set; }
  public List<MembershipTreDecision> MembershipTreDecision { get; set; }
  public List<AuditLog>? AuditLogs { get; set; }
}
