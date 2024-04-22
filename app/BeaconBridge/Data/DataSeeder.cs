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
        new()
        {
          Type = "ontology",
          Id = "Gender:M",
          Label = "MALE"
        },
        new()
        {
          Type = "ontology",
          Id = "Gender:F",
          Label = "FEMALE"
        },
        new()
        {
          Type = "ontology",
          Id = "Race:2",
          Label = "Asian"
        },
        new()
        {
          Type = "ontology",
          Id = "Race:5",
          Label = "White"
        },
        new()
        {
          Type = "ontology",
          Id = "Race:3",
          Label = "Black or African American"
        },
        new()
        {
          Type = "ontology",
          Id = "None:No matching concept",
          Label = "No matching concept"
        }
      };

      foreach (var seed in seedData)
      {
        db.Add(seed);
      }

      await db.SaveChangesAsync();
    }
  }
}
