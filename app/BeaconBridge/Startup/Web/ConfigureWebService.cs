using BeaconBridge.Config;

namespace BeaconBridge.Startup.Web;

public static class ConfigureWebService
{
  public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder b)
  {
    b.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    b.Services.AddEndpointsApiExplorer();
    b.Services.AddSwaggerGen();

    // Add Options
    b.Services
      .Configure<BeaconInfoOptions>(b.Configuration.GetSection("BeaconInfo"))
      .Configure<OrganisationOptions>(b.Configuration.GetSection("Organisation"))
      .Configure<ServiceOptions>(b.Configuration.GetSection("ServiceInfo"));
    // Add HttpClients

    // Add Services

    return b;
  }
}
