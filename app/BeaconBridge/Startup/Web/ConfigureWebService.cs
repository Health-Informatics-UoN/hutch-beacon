using BeaconBridge.Config;
using BeaconBridge.Services;

namespace BeaconBridge.Startup.Web;

public static class ConfigureWebService
{
  public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder b)
  {
    b.Services.AddRouting(options => options.LowercaseUrls = true);
    b.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    b.Services.AddEndpointsApiExplorer();
    b.Services.AddSwaggerGen();

    // Add Options
    b.Services
      .Configure<BeaconInfoOptions>(b.Configuration.GetSection("BeaconInfo"))
      .Configure<MinioOptions>(b.Configuration.GetSection("Minio"))
      .Configure<KeyCloakOptions>(b.Configuration.GetSection("Keycloak"))
      .Configure<OrganisationOptions>(b.Configuration.GetSection("Organisation"));

    // Add HttpClients

    // Add Services
    b.Services.AddTransient<MinioHelper>();
    b.Services.AddTransient<KeycloakMinioUserService>();

    return b;
  }
}
