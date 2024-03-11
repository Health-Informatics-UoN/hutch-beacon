namespace BeaconBridge.Services.Contracts;

public interface IKeycloakMinioUserService
{ 
  Task<bool> SetMinioUserAttribute(string accessToken, string userName, string attributeName, string NewAttribute);
  Task<bool> RemoveMinioUserAttribute(string accessToken, string userName, string attributeName, string NewAttribute);
  Task<string> GetUserIdAsync(string accessToken, string userName);
}
