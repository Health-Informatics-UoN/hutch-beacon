namespace BeaconBridge.Data.Entities.Submission;

public class User
{
  public int Id { get; set; }
  public string? FullName { get; set; }   
  public string Name { get; set; }
  public string Email { get; set; }
  public List<Project> Projects { get; set; }
  public List<Submission> Submissions { get; set; }
  public string FormData { get; set; }

  public string? Biography { get; set; }
  public string? Organisation {get;set;}
  public List<MembershipTreDecision> MembershipTreDecision { get; set; }
  public List<AuditLog>? AuditLogs { get; set; }
}
