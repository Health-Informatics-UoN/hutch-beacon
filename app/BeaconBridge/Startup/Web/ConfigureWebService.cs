using BeaconBridge.Config;
using BeaconBridge.Data;
using BeaconBridge.Services;
using Microsoft.EntityFrameworkCore;

namespace BeaconBridge.Startup.Web;

public static class ConfigureWebService
{
  public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder b)
  {
    b.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    b.Services.AddEndpointsApiExplorer();
    b.Services.AddSwaggerGen();
    b.Services.AddDbContext<SubmissionContext>(o =>
    {
      var connectionString = b.Configuration.GetConnectionString("BeaconBridgeDb");
      o.UseSqlite(connectionString ?? "Data Source=BeaconBridge.db");
    });

    // Add Options
    b.Services
      .Configure<BeaconInfoOptions>(b.Configuration.GetSection("BeaconInfo"))
      .Configure<OrganisationOptions>(b.Configuration.GetSection("Organisation"))
      .Configure<ServiceOptions>(b.Configuration.GetSection("ServiceInfo"));
    // Add HttpClients

    // Add Services
    b.Services.AddTransient<UserHelper>();

    return b;
  }
}
