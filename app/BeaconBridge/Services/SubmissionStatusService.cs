using BeaconBridge.Constants.Submission;
using BeaconBridge.Data.Entities.Submission;

namespace BeaconBridge.Services;

public class SubmissionStatusService
{
  public List<StatusType> SubCompleteTypes =>
    new()
    {
      StatusType.Completed,
      StatusType.Cancelled,
      StatusType.Failed,
      StatusType.PartialResult,
    };

  public void UpdateStatusNoSave(Submission sub, StatusType type, string? description)
  {
    sub.HistoricStatuses.Add(new HistoricStatus()
    {
      Start = sub.LastStatusUpdate.ToUniversalTime(),
      End = DateTime.Now.ToUniversalTime(),
      Status = sub.Status,
      StatusDescription = sub.StatusDescription
    });
    if (type == StatusType.Cancelled || type == StatusType.Completed)
    {
      sub.EndTime = DateTime.Now.ToUniversalTime();
    }

    sub.Status = type;
    sub.LastStatusUpdate = DateTime.Now.ToUniversalTime();
    sub.StatusDescription = description;
    if (sub.Parent != null)
    {
      UpdateParentStatusNoSave(sub.Parent);
    }
  }

  private void UpdateParentStatusNoSave(Submission parent)
  {
    if (!parent.Children.All(x => SubCompleteTypes.Contains(x.Status))) return;
    if (parent.Children.All(x => x.Status == StatusType.Failed))
    {
      UpdateStatusNoSave(parent, StatusType.Failed, "");
    }
    else if (parent.Children.All(x => x.Status == StatusType.Completed))
    {
      UpdateStatusNoSave(parent, StatusType.Completed, "");
    }
    else if (parent.Children.All(x => x.Status == StatusType.Cancelled))
    {
      UpdateStatusNoSave(parent, StatusType.Cancelled, "");
    }
    else
    {
      UpdateStatusNoSave(parent, StatusType.PartialResult, "");
    }

    parent.EndTime = DateTime.Now.ToUniversalTime();
  }
}
