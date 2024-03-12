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
}
