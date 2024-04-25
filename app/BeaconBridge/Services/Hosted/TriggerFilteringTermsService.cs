using BeaconBridge.Config;
using BeaconBridge.Models.Submission.Tes;
using Microsoft.Extensions.Options;
using Minio.Exceptions;

namespace BeaconBridge.Services.Hosted;

/// <summary>
/// Hosted service for sending requests to the submission to trigger a workflow run that gets the filtering
/// terms for an OMOP database.
/// </summary>
public class TriggerFilteringTermsService(IOptions<FilteringTermsUpdateOptions> filteringTermsOptions,
  ILogger<TriggerFilteringTermsService> logger, MinioService minio, IOptions<SubmissionOptions> submissionOptions,
  TesSubmissionService submissionService) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Triggering filtering terms cache update");
    var objectName = Path.GetFileName(filteringTermsOptions.Value.PathToWorkflow);
    while (!stoppingToken.IsCancellationRequested)
    {
      var delay = Task.Delay(TimeSpan.FromSeconds(filteringTermsOptions.Value.DelaySeconds), stoppingToken);

      // Add the workflow crate to MinIO
      if (await minio.StoreExists())
      {
        try
        {
          if (minio.ObjectIsInStore(objectName))
          {
            logger.LogInformation("{Object} already exists",
              Path.GetFileName(filteringTermsOptions.Value.PathToWorkflow));
          }
          else
          {
            logger.LogInformation("Saving Filtering Terms workflow to object store");
            await minio.WriteToStore(filteringTermsOptions.Value.PathToWorkflow);
          }
        }
        catch (Exception e) when (e is MinioException or FileNotFoundException)
        {
          logger.LogError("Unable to write {Object} to store", filteringTermsOptions.Value.PathToWorkflow);
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
        logger.LogError("Cannot save Filtering Terms workflow. Object store does not exist");
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
    logger.LogInformation("Stopping triggering filtering terms cache updates");
    return base.StopAsync(cancellationToken);
  }
}
