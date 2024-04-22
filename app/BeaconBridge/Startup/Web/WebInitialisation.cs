using BeaconBridge.Data;

namespace BeaconBridge.Startup.Web;

public static class WebInitialisation
{
  public static async Task Initialise(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BeaconContext>();
    var seeder = new DataSeeder(db);

    await seeder.SeedFilteringTerms();
  }
}
