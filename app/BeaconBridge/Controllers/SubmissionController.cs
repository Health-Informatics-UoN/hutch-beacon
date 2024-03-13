using BeaconBridge.Constants;
using BeaconBridge.Data;
using BeaconBridge.Models;
using BeaconBridge.Services;
using BeaconBridge.Services.Contracts;
using BeaconBridge.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace BeaconBridge.Controllers;

[ApiController, Route("api/[controller]/")]
public class SubmissionController(BeaconContext db, ILogger logger, IMinioHelper minioHelper) : ControllerBase
{
  [HttpGet("get-waiting-submissions-for-tre")]
  public async Task<IActionResult> GetWaitingSubmissionsForTre()
  {
    var tre = await ControllerHelpers.GetUserTre(User, db);
    if (tre == null)
    {
      return NotFound();
    }

    tre.LastHeartBeatReceived = DateTime.Now.ToUniversalTime();
    await db.SaveChangesAsync();
    var results = tre.Submissions.Where(x => x.Status == StatusType.WaitingForAgentToTransfer).ToList();

    return Ok(results);
  }
  
  [HttpGet("get-request-cancel-subs-for-tre")]
  public async Task<IActionResult> GetRequestCancelSubsForTre()
  {
    var tre = await ControllerHelpers.GetUserTre(User, db);
    if (tre == null)
    {
      return NotFound();
    }

    tre.LastHeartBeatReceived = DateTime.Now.ToUniversalTime();
    await db.SaveChangesAsync();
    var results = tre.Submissions.Where(x => x.Status == StatusType.RequestCancellation).ToList();


    return StatusCode(200, results);
  }
  
  [HttpGet("update-status-for-tre")]
  public async Task<IActionResult> UpdateStatusForTre(string subId, StatusType statusType, string? description)
  {
    await UpdateStatusForTreGuts(subId, statusType, description);
    await db.SaveChangesAsync();
    
    return NoContent();
  }
  
  [HttpGet("close-submission-for-tre")]
  public async Task<IActionResult> CloseSubmissionForTre(string subId, StatusType statusType, string? finalFile, string? description)
  {
    if (!UpdateSubmissionStatus.SubCompleteTypes.Contains(statusType) && statusType != StatusType.Failure)
    {
      throw new Exception("Invalid completion type");
    }

    if (statusType == StatusType.Failure)
    {
      await UpdateStatusForTreGuts(subId, statusType, description);
      await db.SaveChangesAsync();
      statusType = StatusType.Failed;
    }
    var sub = await UpdateStatusForTreGuts(subId, statusType, description);
    sub.FinalOutputFile = finalFile;
    await db.SaveChangesAsync();
            
    return NoContent();
  }
  
  [HttpGet("get-submission/{submissionId}")]
  public Submission GetSubmission(int submissionId)
  {
    try
    {
      var submission = db.Submissions.First(x => x.Id == submissionId);

      logger.LogInformation("{Function} Submission retrieved successfully", "GetSubmission");
      return submission;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Function} Crashed", "GetSubmission");
      throw;
    }
  }
  
  private async Task<Submission> UpdateStatusForTreGuts(string subId, StatusType statusType, string? description)
  {
    var tre = await ControllerHelpers.GetUserTre(User, db);


    var sub = db.Submissions.FirstOrDefault(x => x.Id == int.Parse(subId) && x.Tre == tre);
    if (sub == null)
    {
      throw new Exception("Invalid subid or tre not valid for tes");
    }

    if (UpdateSubmissionStatus.SubCompleteTypes.Contains(sub.Status))
    {
      throw new Exception("Submission already closed. Can't change status");
    }

    UpdateSubmissionStatus.UpdateStatusNoSave(sub, statusType, description);
    return sub;
  }
}
