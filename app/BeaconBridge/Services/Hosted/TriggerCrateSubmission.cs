using BeaconBridge.Config;
using BeaconBridge.Models.Submission.Tes;
using Microsoft.Extensions.Options;
using Minio.Exceptions;
namespace BeaconBridge.Services.Hosted;

public class TriggerCrateSubmission(IOptions<WorkflowCrateOptions> crateOptions,
  ILogger<TriggerCrateSubmission> logger, MinioService minio, IOptions<SubmissionOptions> submissionOptions,
  TesSubmissionService submissionService) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Triggering crate update");
    var objectName = Path.GetFileName(crateOptions.Value.PathToWorkflow);
    while (!stoppingToken.IsCancellationRequested)
    {
      var delay = Task.Delay(TimeSpan.FromSeconds(crateOptions.Value.DelaySeconds), stoppingToken);

      // Add the workflow crate to MinIO
      if (await minio.StoreExists())
      {
        try
        {
          if (minio.ObjectIsInStore(objectName))
          {
            logger.LogInformation("{Object} already exists",
              Path.GetFileName(crateOptions.Value.PathToWorkflow));
          }
          else
          {
            logger.LogInformation("Saving Beacon workflow to object store");
            await minio.WriteToStore(crateOptions.Value.PathToWorkflow);
          }
        }
        catch (Exception e) when (e is MinioException or FileNotFoundException)
        {
          logger.LogError("Unable to write {Object} to store", crateOptions.Value.PathToWorkflow);
          await delay;
          continue;
        }
        catch (NullReferenceException)
        {
          logger.LogError("Unable to read objects in the store");
          await delay;
          continue;
        }
        catch (Exception)
        {
          logger.LogCritical("An unknown error occurred when trying to up load the workflow to the object store");
          await delay;
          continue;
        }
      }
      else
      {
        logger.LogError("Cannot save Beacon workflow. Object store does not exist");
        await delay;
        continue;
      }

      // Get the workflow URL
      var downloadUrl = minio.GetObjectDownloadUrl(objectName);

      // Build the TES task
      var tesTask = new TesTask
      {
        Name = Guid.NewGuid().ToString(),
        Executors = new List<TesExecutor>
        {
          new()
          {
            Image = downloadUrl,
          }
        },
        Tags = new Dictionary<string, string>()
        {
          { "project", submissionOptions.Value.ProjectName },
          { "tres", string.Join('|', submissionOptions.Value.Tres) }
        },
      };

      // Submit to submission layer
      await submissionService.SubmitTesTask(tesTask);

      await delay;
    }
  }

  public override Task StopAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Stopping crate triggering");
    return base.StopAsync(cancellationToken);
  }
  
}
