using System.ComponentModel.DataAnnotations.Schema;
using BeaconBridge.Constants;
using BeaconBridge.Utilities;

namespace BeaconBridge.Models;

public class HistoricStatus
{
        
  public int Id { get; set; }
  public DateTime Start { get; set; }
  public DateTime End { get; set; }
  public virtual Submission Submission { get; set; }
  public StatusType Status { get; set; }
  public string? StatusDescription { get; set; }

  [NotMapped]
  public bool IsCurrent { get; set; }
  [NotMapped]
  public bool IsStillRunning { get; set; }


  public string GetDisplayRunTime()
  {
    return TimeUtility.GetDisplayTime(Start, End);
  }
}
