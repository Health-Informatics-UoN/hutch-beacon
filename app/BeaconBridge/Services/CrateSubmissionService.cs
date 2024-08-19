using BeaconBridge.Config;
using BeaconBridge.Models.Submission.Tes;
using Microsoft.Extensions.Options;
using Minio.Exceptions;

namespace BeaconBridge.Services;

public class CrateSubmissionService(MinioService minioService, ILogger<CrateSubmissionService> logger, IOptions<SubmissionOptions> submissionOptions, TesSubmissionService submissionService)
{
  private readonly SubmissionOptions _submissionOptions = submissionOptions.Value;

  public async Task<Models.TesTask> SubmitCrate(string bagItPath, string beaconTaskId)
  {
      var fileName = bagItPath + ".zip";
      var objectName = Path.GetFileName(fileName);
      logger.LogInformation("Name:{url}",objectName);

      // Add the workflow crate to MinIO
      if (await minioService.StoreExists())
      {
        try
        {
          if (minioService.ObjectIsInStore(fileName))
          {
            logger.LogInformation("{Object} already exists",
              Path.GetFileName(bagItPath));
          }
          else
          {
            logger.LogInformation("Saving Beacon workflow to object store");
            await minioService.WriteToStore(fileName);
          }
        }
        catch (Exception e) when (e is MinioException or FileNotFoundException)
        {
          logger.LogError("Unable to write {Object} to store",bagItPath);
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
      var downloadUrl = minioService.GetObjectDownloadUrl(objectName);
      logger.LogInformation("Download URL found:{url}",downloadUrl);
      // Build the TES task
      var tesTask = new TesTask
      {
        Id = null,
        Name = beaconTaskId,
        Executors = new List<TesExecutor>
        {
          new()
          {
            Image = downloadUrl,
          }
        },
        Tags = new Dictionary<string, string>()
        {
          { "project", _submissionOptions.ProjectName },
          { "tres", string.Join('|', _submissionOptions.Tres) }
        },
      };
      logger.LogInformation("TesTask ready for submission:{task}",tesTask.ToJson());
      // Submit to submission layer
      var task = await submissionService.SubmitTesTask(tesTask);
      return task;
  }
}
