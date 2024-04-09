using BeaconBridge.Config;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Services.Hosted;

/// <summary>
/// Hosted service for sending requests to the submission to trigger a workflow run that gets the filtering
/// terms for an OMOP database.
/// </summary>
/// <param name="options"></param>
/// <param name="logger"></param>
public class TriggerFilteringTermsService(IOptions<FilteringTermsUpdateOptions> options,
  ILogger<TriggerFilteringTermsService> logger) : BackgroundService
{
  private readonly FilteringTermsUpdateOptions _options = options.Value;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Triggering filtering terms cache update");
    while (!stoppingToken.IsCancellationRequested)
    {
      var delay = Task.Delay(TimeSpan.FromSeconds(_options.DelaySeconds), stoppingToken);
      await delay;
    }
  }

  public override Task StopAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Stopping triggering filtering terms cache updates");
    return base.StopAsync(cancellationToken);
  }
}
