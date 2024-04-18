using System.IO.Compression;
using System.Text.Json;
using BeaconBridge.Config;
using Microsoft.Extensions.Options;
using Minio.Exceptions;

namespace BeaconBridge.Services.Hosted;

public class FetchFilteringTermsService(IOptions<FilteringTermsUpdateOptions> filteringTermsOptions,
  ILogger<FetchFilteringTermsService> logger, MinioService minio,
  IServiceProvider serviceProvider) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Triggering filtering terms cache update");

    while (!stoppingToken.IsCancellationRequested)
    {
      var delay = Task.Delay(TimeSpan.FromSeconds(filteringTermsOptions.Value.DelaySeconds), stoppingToken);
      string objectFileName;

      // Download RO-Crate from MinIO
      if (await minio.StoreExists())
      {
        try
        {
          logger.LogInformation("Looking for new workflow run results");
          // Get the most recent object in the bucket
          var mostRecentUpload = minio.GetObjectsInBucket().First();
          objectFileName = mostRecentUpload.Key;
          var destination = Path.Combine(filteringTermsOptions.Value.PathToResults, mostRecentUpload.Key);
          logger.LogInformation("Downloading the results RO-Crate of the workflow");
          await minio.GetFromStore(objectFileName, destination);
          logger.LogInformation("Successfully downloaded the results RO-Crate of the workflow");
        }
        catch (BucketNotFoundException)
        {
          logger.LogError("Unable to download results RO-Crate from the store");
          continue;
        }
        catch (Exception)
        {
          logger.LogError("Unable to find the most recently uploaded workflow run results");
          continue;
        }
      }
      else
      {
        logger.LogError("Minio bucket does not exist");
        continue;
      }

      // Unzip RO-Crate
      var dirInfo = new DirectoryInfo(filteringTermsOptions.Value.PathToResults);
      try
      {
        ZipFile.ExtractToDirectory(objectFileName, filteringTermsOptions.Value.PathToResults);
      }
      catch (Exception e) when (e is NullReferenceException or IOException)
      {
        logger.LogError("Unable to unzip results RO-Crate");
        continue;
      }

      try
      {
        // Get the file containing the results
        var file = dirInfo
          .EnumerateFiles(filteringTermsOptions.Value.ExpectedOutputFileName, SearchOption.AllDirectories).First();

        // Deserialise the file
        var termsJson = await File.ReadAllTextAsync(file.FullName, stoppingToken);
        var filteringTerms = JsonSerializer.Deserialize<List<Models.FilteringTerm>>(termsJson);

        // Save results to the cache
        await using var scope = serviceProvider.CreateAsyncScope();
        var filteringTermsService = scope.ServiceProvider.GetRequiredService<FilteringTermsService>();
        await filteringTermsService.AddOrUpdateRangeAsync(filteringTerms ?? throw new InvalidOperationException());
      }
      catch (InvalidOperationException)
      {
        logger.LogError("Unable to locate filtering terms RO-Crate or results file");
        continue;
      }
      catch (Exception e) when (e is NullReferenceException or JsonException)
      {
        logger.LogError("Unable to deserialise filtering terms file");
        continue;
      }
      catch (OperationCanceledException)
      {
        logger.LogError("Unable to read filtering terms JSON file");
        continue;
      }
      finally
      {
        if (dirInfo.Exists) dirInfo.Delete(recursive: true);
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
