using System.Diagnostics;
using System.Text.RegularExpressions;
using BeaconBridge.Config;
using BeaconBridge.Constants;
using BeaconBridge.Constants.Submission;
using BeaconBridge.Models;
using BeaconBridge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace BeaconBridge.Controllers;

[ApiController]
[Route("api/")]
public class EntryTypeController(
  IOptions<BeaconInfoOptions> beaconInfoOptions,
  CrateGenerationService crateGenerationService,
  CrateSubmissionService crateSubmissionService,
  TesSubmissionService tesSubmissionService,
  IFeatureManager featureFlags)
{
  private readonly BeaconInfoOptions _beaconInfoOptions = beaconInfoOptions.Value;

  [HttpGet("individuals")]
  public async Task<ActionResult<EntryTypeResponse>> GetIndividuals([FromQuery] string? filters,
    [FromQuery] string? requestedSchema,
    [FromQuery] int skip = 0, [FromQuery] int limit = 10)
  {
    var individualsResponse = new EntryTypeResponse()
    {
      Meta =
      {
        BeaconId = _beaconInfoOptions.BeaconId,
        ApiVersion = _beaconInfoOptions.ApiVersion,
        ReceivedRequestSummary = new RequestSummary()
        {
          ApiVersion = _beaconInfoOptions.ApiVersion,
          Pagination = new Pagination() { Limit = limit, Skip = skip }
        }
      }
    };
    individualsResponse.Meta.ReturnedSchemas.Add(new ReturnedSchema()
      { EntityType = EntityTypes.Individuals, Schema = Schemas.Individuals });
    if (filters is not null)
    {
      TesTask tesTask;
      var taskId = Guid.NewGuid().ToString();
      if (await featureFlags.IsEnabledAsync(FeatureFlags.UseRoCrate))
      {
        // Build RO-Crate
        var zipBytes = await crateGenerationService.BuildCrate(filters, taskId);

        // Submit Crate
        tesTask = await crateSubmissionService.SubmitCrate(taskId, zipBytes, taskId);
      }
      else
      {
        var task = tesSubmissionService.CreateTesTask(taskId, filters);
        tesTask = await tesSubmissionService.SubmitTesTask(task);
      }

      // Poll for results
      // Start 5-minute timer
      Stopwatch timer = new Stopwatch();
      timer.Start();
      // Wait for task to be created
      await Task.Delay(5000);
      while (timer.Elapsed.TotalSeconds < 480)
      {
        var submissionStatus = await tesSubmissionService.CheckStatus(tesTask);
        
        // Poll Submission Layer API for task status every 15 seconds
        await Task.Delay(20000);
        if (submissionStatus.Equals(StatusType.DataOutRequested) &&
            await featureFlags.IsEnabledAsync(FeatureFlags.ApproveEgress)
           )
        {
          await tesSubmissionService.ApproveEgress(tesTask);
        }

        if (submissionStatus.Equals(StatusType.Failed)) break;
        if (submissionStatus.Equals(StatusType.Completed))
        {
          var responseSummary = await tesSubmissionService.DownloadResults(tesTask);
          individualsResponse.ResponseSummary = responseSummary;
          break;
        }
      }

      timer.Stop();

      // split filters
      Regex regex = new Regex(",");
      string[] filterList = regex.Split(filters);
      foreach (var match in filterList) individualsResponse.Meta.ReceivedRequestSummary.Filters.Add(match);
    }

    return individualsResponse;
  }
}
