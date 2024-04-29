using BeaconBridge.Models;

namespace BeaconBridge.Constants;

public class DefaultSchemas
{
  public ReturnedSchema Analyses = new()
    { EntityType = EntityTypes.Analyses, Schema = Schemas.Analyses };

  public ReturnedSchema BioSamples = new()
    { EntityType = EntityTypes.BioSamples, Schema = Schemas.BioSamples };

  public ReturnedSchema Cohorts = new()
    { EntityType = EntityTypes.Cohorts, Schema = Schemas.Cohorts };

  public ReturnedSchema Datasets = new()
    { EntityType = EntityTypes.Datasets, Schema = Schemas.Datasets };

  public ReturnedSchema FilteringTerms = new()
    { EntityType = EntityTypes.FilteringTerms, Schema = Schemas.FilteringTerms };

  public ReturnedSchema GenomicVariations = new()
    { EntityType = EntityTypes.GenomicVariations, Schema = Schemas.GenomicVariations };

  public ReturnedSchema Individuals = new()
    { EntityType = EntityTypes.Individuals, Schema = Schemas.Individuals };

  public ReturnedSchema Runs = new()
    { EntityType = EntityTypes.Runs, Schema = Schemas.Runs };
}
