using BeaconBridge.Constants.Submission;
using BeaconBridge.Data;
using BeaconBridge.Data.Entities.Submission;
using BeaconBridge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace BeaconBridge.Controllers;

[ApiController, Route("api/[controller]")]
public class SubmissionController(SubmissionContext submissionContext, SubmissionStatusService statusService,
  UserHelper userHelper, ILogger logger) : ControllerBase
{
  /// <summary>
  /// Update the status of a submission to a TRE.
  /// </summary>
  /// <param name="subId">The ID of the submission to update.</param>
  /// <param name="statusType">The new status for the submission.</param>
  /// <param name="description">A description to accompany the update.</param>
  /// <returns></returns>
  [Authorize(Roles = "dare-control-admin,dare-tre-admin")]
  [HttpGet("UpdateStatusForTre")]
  [SwaggerResponse(statusCode: 200, description: "The submission was updated successfully.")]
  [SwaggerResponse(statusCode: 400, description: "The submission is either closed or non-existent")]
  public async Task<IActionResult> UpdateStatusForTre(string subId, StatusType statusType, string? description)
  {
    try
    {
      await UpdateSubmissionStatus(subId, statusType, description);
      await submissionContext.SaveChangesAsync();
      return Ok();
    }
    catch (Exception e) when (e is DbUpdateException or DbUpdateConcurrencyException or OperationCanceledException)
    {
      logger.LogCritical("Unable to save updates for submission {SubId}", subId);
      return StatusCode(500);
    }
    catch (Exception e) when (e is InvalidDataException or InvalidOperationException)
    {
      logger.LogError("{Message}", e.Message);
      return BadRequest();
    }
  }

  private async Task<Submission> UpdateSubmissionStatus(string subId, StatusType statusType, string? description)
  {
    var tre = await userHelper.GetUserTre(User);
    var sub = submissionContext.Submissions.FirstOrDefault(x => x.Id == int.Parse(subId) && x.Tre == tre);
    if (sub == null)
    {
      throw new InvalidDataException("Invalid subid or tre not valid for tes");
    }

    if (statusService.SubCompleteTypes.Contains(sub.Status))
    {
      throw new InvalidOperationException("Submission already closed. Can't change status");
    }

    statusService.UpdateStatusNoSave(sub, statusType, description);
    return sub;
  }
}
