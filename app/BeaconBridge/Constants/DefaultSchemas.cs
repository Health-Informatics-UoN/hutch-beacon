namespace BeaconBridge.Constants;

public class DefaultSchemas
{
  public Dictionary<string, string> Analyses = new()
    { { "entityType", EntityTypes.Analyses }, { "schema", Schemas.Analyses } };

  public Dictionary<string, string> BioSamples = new()
    { { "entityType", EntityTypes.BioSamples }, { "schema", Schemas.BioSamples } };
  
  public Dictionary<string, string> Cohorts = new()
    { { "entityType", EntityTypes.Cohorts }, { "schema", Schemas.Cohorts } };
  
  public Dictionary<string, string> Datasets = new()
    { { "entityType", EntityTypes.Datasets }, { "schema", Schemas.Datasets } };
  
  public Dictionary<string, string> FilteringTerms = new()
    { { "entityType", EntityTypes.FilteringTerms }, { "schema", Schemas.FilteringTerms } };
  
  public Dictionary<string, string> GenomicVariations = new()
    { { "entityType", EntityTypes.GenomicVariations }, { "schema", Schemas.GenomicVariations } };
  
  public Dictionary<string, string> Individuals = new()
    { { "entityType", EntityTypes.Individuals }, { "schema", Schemas.Individuals } };
  
  public Dictionary<string, string> Runs = new()
    { { "entityType", EntityTypes.Runs }, { "schema", Schemas.Runs } };
}
