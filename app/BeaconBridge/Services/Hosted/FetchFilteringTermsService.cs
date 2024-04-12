using System.IO.Compression;
using System.Text.Json;
using BeaconBridge.Config;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Services.Hosted;

public class FetchFilteringTermsService(IOptions<FilteringTermsUpdateOptions> filteringTermsOptions,
  ILogger<FetchFilteringTermsService> logger, MinioService minio,
  FilteringTermsService filteringTermsService) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Triggering filtering terms cache update");
    var objectName = Path.GetFileName(filteringTermsOptions.Value.PathToResults);
    while (!stoppingToken.IsCancellationRequested)
    {
      var delay = Task.Delay(TimeSpan.FromSeconds(filteringTermsOptions.Value.DelaySeconds), stoppingToken);

      // Download RO-Crate from MinIO
      if (await minio.StoreExists() && minio.ObjectIsInStore(objectName))
      {
        try
        {
          logger.LogInformation("Downloading the results RO-Crate of the workflow");
          await minio.GetFromStore(objectName, filteringTermsOptions.Value.PathToResults);
        }
        catch (Exception)
        {
          logger.LogCritical("Unable to download results RO-Crate from the store");
          continue;
        }
      }

      // Unzip RO-Crate
      var dirInfo = new DirectoryInfo(Path.GetDirectoryName(filteringTermsOptions.Value.PathToResults)!);
      ZipFile.ExtractToDirectory(filteringTermsOptions.Value.PathToResults, dirInfo.FullName);

      try
      {
        // Get the file containing the results
        var file = dirInfo.EnumerateFiles("data/outputs/*/output.json").First();

        // Deserialise the file
        var termsJson = await File.ReadAllTextAsync(file.FullName, stoppingToken);
        var filteringTerms = JsonSerializer.Deserialize<List<Models.FilteringTerm>>(termsJson);

        // Save results to the cache
        await filteringTermsService.SaveRangeAsync(filteringTerms ?? throw new InvalidOperationException());
      }
      catch (InvalidOperationException)
      {
        logger.LogCritical("Unable to locate filtering terms RO-Crate or results file");
        continue;
      }
      catch (NullReferenceException)
      {
        logger.LogCritical("Unable to deserialise filtering terms file");
        continue;
      }

      await delay;
    }
  }

  public override Task StopAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Stopping fetching filtering terms");
    return base.StopAsync(cancellationToken);
  }
}
