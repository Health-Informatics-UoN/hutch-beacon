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
      .Configure<ServiceOptions>(b.Configuration.GetSection("ServiceInfo"))
      .Configure<OpenIdOptions>(b.Configuration.GetSection("IdentityProvider"))
      .Configure<MinioOptions>(b.Configuration.GetSection("Minio"))
      .Configure<WorkflowOptions>(b.Configuration.GetSection("Workflow"))
      .Configure<CrateAgentOptions>(b.Configuration.GetSection("Crate:Agent"))
      .Configure<CrateProjectOptions>(b.Configuration.GetSection("Crate:Project"))
      .Configure<CrateOrganizationOptions>(b.Configuration.GetSection("Crate:Organisation"))
      .Configure<AgreementPolicyOptions>(b.Configuration.GetSection("AgreementPolicy"))
      .Configure<AssessActionsOptions>(b.Configuration.GetSection("AssessActions"));
    // Add HttpClients

    // Add Services
    b.Services
      .AddTransient<UserHelper>()
      .AddTransient<OpenIdIdentityService>()
      .AddTransient<MinioService>()
      .AddSingleton<SubmissionStatusService>();

    return b;
  }
}
