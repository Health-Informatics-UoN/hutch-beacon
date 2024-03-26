using BeaconBridge.Constants.Submission;

namespace BeaconBridge.Data.Entities.Submission;

public class AuditLog
{
  public int Id { get; set; }
  public Project? Project { get; set; }
  public User? User { get; set; }
  public Tre? Tre { get; set; }
  public Submission? Submission { get; set; }
  public string? LoggedInUserName { get; set; }
  public string? HistoricFormData { get; set; }
  public string? IPaddress { get; set; }
  public LogType LogType { get; set; }
  public DateTime Date { get; set; }
}
