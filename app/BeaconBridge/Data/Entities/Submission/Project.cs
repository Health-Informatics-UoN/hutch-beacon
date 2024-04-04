namespace BeaconBridge.Data.Entities.Submission;

public class Project
{
  public int Id { get; set; }
  public List<User> Users { get; set; }
  public List<Tre> Tres { get; set; }
  public string FormData { get; set; }
  public string Name { get; set; }
  public string Display { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  public string ProjectDescription { get; set; }
  public string? ProjectOwner { get; set; }
  public string? ProjectContact { get; set; }
  public bool MarkAsEmbargoed { get; set; }
  public string? SubmissionBucket { get; set; }
  public string? OutputBucket { get; set; }
  public List<Submission> Submissions { get; set; }
  public List<AuditLog>? AuditLogs { get; set; }
  public List<ProjectTreDecision> ProjectTreDecisions { get; set; }
  public List<MembershipTreDecision> MembershipTreDecision { get; set; }
}
