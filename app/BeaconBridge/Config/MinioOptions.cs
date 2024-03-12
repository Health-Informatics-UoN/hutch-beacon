namespace BeaconBridge.Config;

public class MinioOptions
{
  public string Url { get; set; }
  public string AccessKey { get; set; }
  public string SecretKey { get; set; }
  public string BucketName { get; set; }
  public string AWSRegion { get; set; }
  public string HutchURLOverride { get; set; }
  public string AWSService { get; set; }
  public string AttributeName { get; set; }
  public string AdminConsole { get; set; }
  public bool UesProxy { get; set; }
  public string ProxyAddresURL { get; set; }
  public string BypassProxy { get; set; }
}
