namespace BeaconBridge.Models;

public class EntryTypeResponse
{
  public Meta Meta { get; set; } = new();
  
  public ResponseSummary? ResponseSummary { get; set; }

  public List<BeaconHandover>? BeaconHandovers { get; set; }
}

public class BeaconHandover
{
  public HandoverType HandoverType { get; set; } = new();

  public string Url { get; set; } = string.Empty;
}

public class HandoverType
{
  public string Id { get; set; } = string.Empty;

  public string Label { get; set; } = string.Empty;
}


