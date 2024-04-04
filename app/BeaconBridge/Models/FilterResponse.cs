namespace BeaconBridge.Models;

public class FilterResponse
{
  public Meta Meta { get; set; } = new();
  public List<FilteringTerm> Response { get; set; } = new();
}

public class FilteringTerm
{
  public string Type { get; set; } = string.Empty;
  public string Id { get; set; } = string.Empty;
  public string Label { get; set; } = string.Empty;
  public string Scope { get; set; } = string.Empty;
}
