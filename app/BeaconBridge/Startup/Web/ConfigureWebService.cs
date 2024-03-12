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
    b.Services.Configure<MinioOptions>(b.Configuration.GetSection("Minio"));

    // Add HttpClients

    // Add Services
    b.Services.AddTransient<MinioHelper>();

    return b;
  }
}
