using System.IO.Abstractions;
using BeaconBridge.Config;
using BeaconBridge.Services;
using Flurl.Http.Configuration;

namespace BeaconBridge.Startup.Web;

public static class ConfigureWebService
{
  public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder b)
  {
    b.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    b.Services.AddEndpointsApiExplorer();
    b.Services.AddSwaggerGen();
    b.Services
      .AddAutoMapper(typeof(Program).Assembly)
      .AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>()
      .AddHttpClient() // We prefer Flurl for most use cases, but IdentityModel has extensions for vanilla HttpClient
      .AddTransient<IFileSystem, FileSystem>();

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
      // .AddTransient<UserHelper>()  // Not used at the moment
      .AddTransient<OpenIdIdentityService>()
      .AddTransient<MinioService>()
      .AddTransient<CrateGenerationService>()
      .AddTransient<FilteringTermsService>()
      .AddSingleton<SubmissionStatusService>();

    return b;
  }
}
