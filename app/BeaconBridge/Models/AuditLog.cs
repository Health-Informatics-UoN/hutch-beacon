using BeaconBridge.Constants;

namespace BeaconBridge.Models;

public class AuditLog
{
  public int Id { get; set; }

  public virtual Project? Project { get; set; }
  public virtual User? User { get; set; }
  public virtual Tre? Tre { get; set; }
  public virtual Submission? Submission { get; set; }
       
  public string? LoggedInUserName { get; set; }
  public string? HistoricFormData { get; set; }
  public string? IPaddress { get; set; }

  public LogType LogType { get; set; }
  public DateTime Date { get; set; }

}
