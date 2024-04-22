using BeaconBridge.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeaconBridge.Data;

public class DataSeeder(BeaconContext db)
{
  public async Task SeedFilteringTerms()
  {
    if (!await db.FilteringTerms.AsNoTracking().AnyAsync())
    {
      var seedData = new List<FilteringTerm>
      {
        new FilteringTerm
        {
          Type = "ontology",
          Id = "Gender:M",
          Label = "MALE",
          Scope = string.Empty
        },
        new FilteringTerm
        {
          Type = "ontology",
          Id = "Gender:F",
          Label = "FEMALE",
          Scope = string.Empty
        },
        new FilteringTerm
        {
          Type = "ontology",
          Id = "Race:2",
          Label = "Asian",
          Scope = string.Empty
        },
        new FilteringTerm
        {
          Type = "ontology",
          Id = "Race:5",
          Label = "White",
          Scope = string.Empty
        },
        new FilteringTerm
        {
          Type = "ontology",
          Id = "Race:3",
          Label = "Black or African American",
          Scope = string.Empty
        },
        new FilteringTerm
        {
          Type = "ontology",
          Id = "None:No matching concept",
          Label = "No matching concept",
          Scope = string.Empty
        }
      };
    }
  }
}
