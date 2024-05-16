using System.IO.Compression;
using System.Text.Json;
using BeaconBridge.Config;
using Microsoft.Extensions.Options;
using Minio.Exceptions;

namespace BeaconBridge.Services.Hosted;

public class FetchCrateService(
  IOptions<WorkflowCrateOptions> crateOptions,
  ILogger<FetchCrateService> logger,
  MinioService minio,
  IServiceProvider serviceProvider) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Fetching Crate...");

    while (!stoppingToken.IsCancellationRequested)
    {
      var delay = Task.Delay(TimeSpan.FromSeconds(crateOptions.Value.DelaySeconds), stoppingToken);

      // Download RO-Crate from MinIO
      string objectNameOnDisk;
      if (await minio.StoreExists())
      {
        try
        {
          logger.LogInformation("Looking for new workflow run results");
          // Get the most recent object in the bucket
          var mostRecentUpload = minio.GetObjectsInBucket().First();
          objectNameOnDisk = Path.Combine(crateOptions.Value.PathToResults, mostRecentUpload.Key);
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
      var dirInfo = new DirectoryInfo(crateOptions.Value.PathToResults);
      if (!dirInfo.Exists) dirInfo.Create();
      try
      {
        ZipFile.ExtractToDirectory(objectNameOnDisk, crateOptions.Value.PathToResults);
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
          .EnumerateFiles(crateOptions.Value.ExpectedOutputFileName, SearchOption.AllDirectories).First();

        // Deserialise the file
        var resultsJson = await File.ReadAllTextAsync(file.FullName, stoppingToken);
        var result = JsonSerializer.Deserialize<Models.ResponseSummary>(resultsJson);
        logger.LogInformation(result.ToString());
      }
      catch (InvalidOperationException)
      {
        logger.LogError("Unable to locate results file");
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
        logger.LogError("Unable to read results JSON file");
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
    logger.LogInformation("Stopping fetching of workflow crate");
    return base.StopAsync(cancellationToken);
  }
}
