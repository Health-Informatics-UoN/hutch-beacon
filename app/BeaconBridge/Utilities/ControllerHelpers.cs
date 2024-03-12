using System.Security.Claims;
using BeaconBridge.Constants;
using BeaconBridge.Data;
using BeaconBridge.Services.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using BeaconBridge.Models;

namespace BeaconBridge.Utilities;

public class ControllerHelpers
{
  public static async Task AddUserToMinioBucket(User user, Project project,
            IHttpContextAccessor httpContextAccessor, string attributeName,
            IKeycloakMinioUserService keycloakMinioUserService, ClaimsPrincipal loggedInUser,
            BeaconContext dbContext)
        {
            var accessToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");


            await keycloakMinioUserService.SetMinioUserAttribute(accessToken, user.Name.ToString(), attributeName,
                project.SubmissionBucket.ToLower() + "_policy");

            await keycloakMinioUserService.SetMinioUserAttribute(accessToken, user.Name.ToString(), attributeName,
                project.OutputBucket.ToLower() + "_policy");
            



        }


        public static async Task<Tre?> GetUserTre(ClaimsPrincipal loggedInUser, BeaconContext dbContext)
        {
            var usersName = (from x in loggedInUser.Claims where x.Type == "preferred_username" select x.Value).First();
            var tre = await dbContext.Tres.FirstAsync(x => string.Equals(x.AdminUsername, usersName, StringComparison.CurrentCultureIgnoreCase));

            return tre;
        }


        public static async Task RemoveUserFromMinioBucket(User user, Project project,
            IHttpContextAccessor httpContextAccessor, string attributeName,
            IKeycloakMinioUserService keycloakMinioUserService, ClaimsPrincipal loggedInUser,
            BeaconContext dbContext)
        {
            var accessToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");



            await keycloakMinioUserService.RemoveMinioUserAttribute(accessToken, user.Name.ToString(), attributeName,
                project.SubmissionBucket.ToLower() + "_policy");
            await keycloakMinioUserService.RemoveMinioUserAttribute(accessToken, user.Name.ToString(), attributeName,
                project.OutputBucket.ToLower() + "_policy");

            


        }

        public static async Task AddAuditLog(LogType logType, User? user, Project? project, Tre? tre, Submission? submission, string? formData,
            IHttpContextAccessor httpContextAccessor,
            ClaimsPrincipal loggedInUser, BeaconContext dbContext)
        {
            var audit = new AuditLog()
            {
                HistoricFormData = formData,
                Submission = submission,
                IPaddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                LoggedInUserName = (from x in loggedInUser.Claims where x.Type == "preferred_username" select x.Value).First(),
                Project = project,
                User = user,
                Tre = tre,
                LogType = logType,
                Date = DateTime.Now.ToUniversalTime()
            };
            dbContext.AuditLogs.Add(audit);
            await dbContext.SaveChangesAsync();
        }
}
