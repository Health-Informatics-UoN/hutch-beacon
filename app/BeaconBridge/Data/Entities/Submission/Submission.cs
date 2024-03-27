using System.ComponentModel.DataAnnotations.Schema;
using BeaconBridge.Constants.Submission;
using BeaconBridge.Models.Submission;

namespace BeaconBridge.Data.Entities.Submission;

public class Submission
{
  public int Id { get; set; }
  public int? ParentId { get; set; }
  public string? TesId { get; set; }
  public string SourceCrate { get; set; }
  public string TesName { get; set; }
  public string? TesJson { get; set; }
  public string? FinalOutputFile { get; set; }
  public string DockerInputLocation { get; set; }
  public Project Project { get; set; }
  [ForeignKey("ParentID")]
  public Submission? Parent { get; set; }
  public List<Submission> Children { get; set; }
  public List<HistoricStatus> HistoricStatuses { get; set; }
  [NotMapped]
  public List<StageInfo> StageInfo { get; set; }
  public List<SubmissionFile> SubmissionFiles { get; set; }
  public List<AuditLog>? AuditLogs { get; set; }
  public Tre? Tre { get; set; }
  public User SubmittedBy { get; set; }
  public DateTime LastStatusUpdate { get; set; }
  public DateTime StartTime { get; set; }
  public DateTime EndTime { get; set; }
  public StatusType Status { get; set; }
  public string? StatusDescription { get; set; }
}
