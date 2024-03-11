using BeaconBridge.Data;
using BeaconBridge.Services;
using BL.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BeaconBridge.Controllers;

[ApiController, Route("api/[controller]/")]
public class SubmissionController : ControllerBase
{
  private readonly BeaconContext _db;
  public SubmissionController(BeaconContext db)
  {
    _db = db;
  }

  [HttpGet]
  [Route("GetWaitingSubmissionsForTre")]
  public async Task<IActionResult> GetWaitingSubmissionsForTre()
  {
    var usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
    var tre = ControllerHelpers.GetUserTre(User, _db);

    tre.LastHeartBeatReceived = DateTime.Now.ToUniversalTime();
    await _db.SaveChangesAsync();
    var results = tre.Submissions.Where(x => x.Status == StatusType.WaitingForAgentToTransfer).ToList();


    return StatusCode(200, results);
  }
}
