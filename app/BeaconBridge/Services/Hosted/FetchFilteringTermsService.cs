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
      var objectNameOnDisk = string.Empty;

      // Download RO-Crate from MinIO
      if (await minio.StoreExists())
      {
        try
        {
          logger.LogInformation("Looking for new workflow run results");
          // Get the most recent object in the bucket
          var mostRecentUpload = minio.GetObjectsInBucket().First();
          objectNameOnDisk = Path.Combine(filteringTermsOptions.Value.PathToResults, mostRecentUpload.Key);
          logger.LogInformation("Downloading the results RO-Crate of the workflow");
          await minio.GetFromStore(mostRecentUpload.Key, objectNameOnDisk);
          logger.LogInformation("Successfully downloaded the results RO-Crate of the workflow");
        }
        catch (BucketNotFoundException)
        {
          logger.LogError("Unable to download results RO-Crate from the store");
          await delay;
          continue;
        }
        catch (Exception)
        {
          logger.LogError("Unable to find the most recently uploaded workflow run results");
          await delay;
          continue;
        }
      }
      else
      {
        logger.LogError("Minio bucket does not exist");
        await delay;
        continue;
      }

      // Unzip RO-Crate
      var dirInfo = new DirectoryInfo(filteringTermsOptions.Value.PathToResults);
      if (!dirInfo.Exists) dirInfo.Create();
      try
      {
        ZipFile.ExtractToDirectory(objectNameOnDisk, filteringTermsOptions.Value.PathToResults);
      }
      catch (Exception e) when (e is NullReferenceException or IOException or ArgumentException)
      {
        logger.LogError("Unable to unzip results RO-Crate");
        await delay;
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
        logger.LogInformation("Saved filtering terms to cache");
      }
      catch (InvalidOperationException)
      {
        logger.LogError("Unable to locate filtering terms RO-Crate or results file");
        await delay;
        continue;
      }
      catch (Exception e) when (e is NullReferenceException or JsonException)
      {
        logger.LogError("Unable to deserialise filtering terms file");
        await delay;
        continue;
      }
      catch (OperationCanceledException)
      {
        logger.LogError("Unable to read filtering terms JSON file");
        await delay;
        continue;
      }

      // Clean up
      if (dirInfo.Exists)
      {
        foreach (var directory in dirInfo.EnumerateDirectories())
        {
          directory.Delete(recursive: true);
        }

        foreach (var file in dirInfo.EnumerateFiles())
        {
          file.Delete();
        }
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
