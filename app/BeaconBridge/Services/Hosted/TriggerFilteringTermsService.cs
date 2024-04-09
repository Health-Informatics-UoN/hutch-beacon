using BeaconBridge.Config;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Services.Hosted;

/// <summary>
/// Hosted service for sending requests to the submission to trigger a workflow run that gets the filtering
/// terms for an OMOP database.
/// </summary>
public class TriggerFilteringTermsService(IOptions<FilteringTermsUpdateOptions> options,
  ILogger<TriggerFilteringTermsService> logger, MinioService minio) : BackgroundService
{
  private readonly FilteringTermsUpdateOptions _options = options.Value;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Triggering filtering terms cache update");
    while (!stoppingToken.IsCancellationRequested)
    {
      var delay = Task.Delay(TimeSpan.FromSeconds(_options.DelaySeconds), stoppingToken);

      // Add the workflow crate to MinIO
      if (await minio.StoreExists())
      {
        logger.LogInformation("Saving Filtering Terms workflow to object store");
        // Todo: save workflow to Minio
      }
      else
      {
        logger.LogCritical("Cannot save Filtering Terms workflow. Object store does not exist");
      }

      // Build the TES task

      // Submit to submission layer

      await delay;
    }
  }

  public override Task StopAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Stopping triggering filtering terms cache updates");
    return base.StopAsync(cancellationToken);
  }
}
