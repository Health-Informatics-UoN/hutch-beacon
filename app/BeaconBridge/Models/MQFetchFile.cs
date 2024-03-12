namespace BeaconBridge.Models;

public class MQFetchFile
{
  public string Url { get; set; }
  public string BucketName { get; set; }
  public string? Key { get; set; }
}
