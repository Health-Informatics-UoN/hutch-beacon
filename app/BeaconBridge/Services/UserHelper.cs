using System.Security.Claims;
using BeaconBridge.Data;
using BeaconBridge.Data.Entities.Submission;
using Microsoft.EntityFrameworkCore;

namespace BeaconBridge.Services;

public class UserHelper(SubmissionContext context)
{
  /// <summary>
  /// Get a TRE associated with a user.
  /// </summary>
  /// <param name="user">The user to from whom to find the TRE.</param>
  /// <returns>The TRE a user is connected to, or null if the user isn't connected to a TRE</returns>
  public async Task<Tre?> GetUserTre(ClaimsPrincipal user)
  {
    var usersName = (from x in user.Claims where x.Type == "preferred_username" select x.Value).First();
    var tre = await context.Tres.FirstOrDefaultAsync(x => string.Equals(x.AdminUsername, usersName, StringComparison.CurrentCultureIgnoreCase));

    return tre;
  }
}
