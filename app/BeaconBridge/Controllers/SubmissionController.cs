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
  [HttpGet]
  [Route("get-waiting-submissions-for-tre")]
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
  
  [HttpGet, Route("get-request-cancel-subs-for-tre")]
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
  
  [HttpGet, Route("update-status-for-tre")]
  public async Task<IActionResult> UpdateStatusForTre(string subId, StatusType statusType, string? description)
  {
    await UpdateStatusForTreGuts(subId, statusType, description);
    await db.SaveChangesAsync();
    
    return NoContent();
  }
  
  [HttpGet, Route("close-submission-for-tre")]
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
  
  [HttpGet("download-file")]
  public async Task<IActionResult> DownloadFileAsync(int submissionId)
  {
    try
    {
      logger.LogDebug("DownloadFileAsync submissionId > {SubmissionId}", submissionId);
      var submission = db.Submissions.First(x => x.Id == submissionId);


      logger.LogDebug("DownloadFileAsync submission.Project.OutputBucket > {ProjectOutputBucket} submission.FinalOutputFile > {SubmissionFinalOutputFile} ", submission.Project.OutputBucket, submission.FinalOutputFile);
      var response = await minioHelper.GetCopyObject(submission.Project.OutputBucket, submission.FinalOutputFile);

            
      var responseStream = response.ResponseStream;
      return File(responseStream, GetContentType(submission.FinalOutputFile), submission.FinalOutputFile);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Function} Crashed", "DownloadFiles");
      throw;
    }
  }
  
  [HttpPost("save-submission-files")]
  public async Task<IActionResult> SaveSubmissionFiles(int submissionId, List<SubmissionFile> submissionFiles)
  {
    try { 
      var existingSubmission = db.Submissions
        .Include(d => d.SubmissionFiles)
        .FirstOrDefault(d => d.Id == submissionId);

      if (existingSubmission != null)
      {
        foreach (var file in submissionFiles)
        {
          var existingFile =
            existingSubmission.SubmissionFiles.FirstOrDefault(f =>
              f.TreBucketFullPath == file.TreBucketFullPath);
          if (existingFile != null)
          {
            existingFile.Name = file.Name;
            existingFile.TreBucketFullPath = file.TreBucketFullPath;
            existingFile.SubmisionBucketFullPath = file.SubmisionBucketFullPath;
            existingFile.Status = file.Status;
            existingFile.Description = file.Description;
          }
          else
          {
            existingSubmission.SubmissionFiles.Add(file);
          }
        }

        await db.SaveChangesAsync();
        return Ok(existingSubmission);
      }
      return BadRequest();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Function} Crashed", "SaveSubmissionFiles");
      throw;
    }
  }
  
  private static string GetContentType(string fileName)
  {
    // Create a new FileExtensionContentTypeProvider
    var provider = new FileExtensionContentTypeProvider();

    // Try to get the content type based on the file name's extension
    if (provider.TryGetContentType(fileName, out var contentType))
    {
      return contentType;
    }

    // If the content type cannot be determined, provide a default value
    return "application/octet-stream"; // This is a common default for unknown file types
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
