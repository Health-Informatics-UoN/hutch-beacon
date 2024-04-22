using System.Text;
using System.Text.Json;
using BeaconBridge.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeaconBridge.Data;

public class DataSeeder(BeaconContext db)
{
  public async Task SeedFilteringTerms()
  {
    if (!await db.FilteringTerms.AsNoTracking().AnyAsync())
    {
      var seedData = await ReadSampleData();

      foreach (var seed in seedData)
      {
        db.Add(seed);
      }

      await db.SaveChangesAsync();
    }
  }

  private async Task<List<FilteringTerm>> ReadSampleData()
  {
    var assembly = typeof(DataSeeder).Assembly;
    var resource = assembly.GetManifestResourceStream("BeaconBridge.SampleData.filtering_terms.json");

    using var sr = new StreamReader(resource!, Encoding.UTF8);
    var jsonText = await sr.ReadToEndAsync();

    var results = JsonSerializer.Deserialize<List<FilteringTerm>>(jsonText);
    if (results is null) throw new NullReferenceException("Could not parse the sample data");

    return results;
  }
}
