using BeaconBridge.Data;
using Microsoft.EntityFrameworkCore;

namespace BeaconBridge.Startup.Web;

public static class WebEntrypoint
{
  public static async Task Run(string[] args)
  {
    var b = WebApplication.CreateBuilder(args);

    // Configure DI Services
    b.ConfigureServices();

    // Build the app
    var app = b.Build();

    // Configure the HTTP Request Pipeline
    app.UseWebPipeline();

    // Automatic Migrations

    using (var scope = app.Services.CreateScope())
    {
      var dbContext = scope.ServiceProvider.GetRequiredService<BeaconContext>();

      await dbContext.Database.MigrateAsync();
    }

    // Run the app!
    await app.RunAsync();
  }
}
