using BeaconBridge.Config;
using BeaconBridge.Models.Submission.Tes;
using Microsoft.Extensions.Options;
using Minio.Exceptions;

namespace BeaconBridge.Services;

public class CrateSubmissionService(
  MinioStoreServiceFactory minioStoreServiceFactory,
  ILogger<CrateSubmissionService> logger,
  IOptions<SubmissionOptions> submissionOptions,
  TesSubmissionService submissionService)
{
  private readonly SubmissionOptions _submissionOptions = submissionOptions.Value;

  /// <summary>
  /// Upload ROCrate to minio store.
  /// Get Download url, create TesTask & submit to submission layer.
  /// </summary>
  /// <param name="bagItPath">Path to BagIt directory</param>
  /// <param name="zip">Zip file byte array.</param>
  /// <param name="beaconTaskId">ID for beacon task</param>
  public async Task<Models.TesTask> SubmitCrate(string bagItPath, byte[] zip, string beaconTaskId)
  {
    var fileName = bagItPath + ".zip";
    var store = await minioStoreServiceFactory.Create();
    // Add the workflow crate to MinIO
    if (await store.StoreExists())
    {
      try
      {
        if (store.ObjectIsInStore(fileName))
        {
          logger.LogInformation("{Object} already exists",
            Path.GetFileName(bagItPath));
        }
        else
        {
          logger.LogInformation("Saving Beacon workflow to object store");
          await store.WriteToStore(fileName, zip);
        }
      }
      catch (Exception e) when (e is MinioException or FileNotFoundException)
      {
        logger.LogError("Unable to write {Object} to store", bagItPath);
      }
      catch (NullReferenceException)
      {
        logger.LogError("Unable to read objects in the store");
      }
      catch (Exception)
      {
        logger.LogCritical("An unknown error occurred when trying to up load the workflow to the object store");
      }
    }
    else
    {
      logger.LogError("Cannot save Beacon workflow. Object store does not exist");
    }


    // Get the workflow URL
    var downloadUrl = await store.GetObjectDownloadUrl(fileName);
    logger.LogInformation("Download URL found:{url}", downloadUrl);
    // Build the TES task
    var tesTask = new TesTask
    {
      Id = null,
      Name = beaconTaskId,
      Executors = new List<TesExecutor>
      {
        new()
        {
          Image = downloadUrl
        }
      },
      Tags = new Dictionary<string, string>()
      {
        { "project", _submissionOptions.ProjectName },
        { "tres", string.Join('|', _submissionOptions.Tres) }
      },
    };
    logger.LogInformation("TesTask ready for submission:{task}", tesTask.ToJson());
    // Submit to submission layer
    var task = await submissionService.SubmitTesTask(tesTask);
    return task;
  }
}
