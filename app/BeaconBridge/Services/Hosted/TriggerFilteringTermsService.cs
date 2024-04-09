using BeaconBridge.Config;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Services.Hosted;

public class TriggerFilteringTermsService(IOptions<FilteringTermsUpdateOptions> options,
  ILogger<TriggerFilteringTermsService> logger) : BackgroundService
{
  private readonly FilteringTermsUpdateOptions _options = options.Value;

  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    throw new NotImplementedException();
  }

  public override Task StopAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Stopping triggering filtering terms cache updates");
    return base.StopAsync(cancellationToken);
  }
}
