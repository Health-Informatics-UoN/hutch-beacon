using BeaconBridge.Constants.Submission;

namespace BeaconBridge.Models.Submission;

public class StageInfo
{
  public List<StatusType> statusTypeList { get; set; }
  public string stageName { get; set; }
  public int stageNumber { get; set; }
  public Dictionary<int, List<StatusType>> stagesDict { get; set; }
}
