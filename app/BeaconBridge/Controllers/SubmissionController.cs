using BeaconBridge.Constants;
using BeaconBridge.Data;
using BeaconBridge.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace BeaconBridge.Controllers;

[ApiController, Route("api/[controller]/")]
public class SubmissionController(BeaconContext db, ControllerHelpers helpers) : ControllerBase
{
  [HttpGet]
  [Route("get-waiting-submissions-for-tre")]
  public async Task<IActionResult> GetWaitingSubmissionsForTre()
  {
    var tre = await helpers.GetUserTre(User, db);
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
    var tre = await helpers.GetUserTre(User, db);
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
