using System.Net.Http.Headers;
using BeaconBridge.Config;
using BeaconBridge.Services.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeaconBridge.Services;

public class KeycloakMinioUserService(KeyCloakOptions submissionKeyCloakSettings, ILogger logger) : IKeycloakMinioUserService
{
  public KeyCloakOptions _submissionKeyCloakSettings = submissionKeyCloakSettings;

  public async Task<bool> SetMinioUserAttribute(string accessToken, string userName, string attributeName, string attributeValueToAdd)
  {
    var baseUrl = _submissionKeyCloakSettings.Server;
    var realm = _submissionKeyCloakSettings.Realm;
    var attributeKey = "policy";
    var userId = await GetUserIdAsync(accessToken, userName);
    var userAttributesJson = await GetUserAttributesAsync(baseUrl, realm, accessToken, userId);

    if (userAttributesJson != null)
    {

      JObject user = JObject.Parse(userAttributesJson);

      if (user["attributes"] == null)
      {
        JObject attributes = new JObject();

        // Add the "attributes" object to the user object
        user["attributes"] = attributes;
      }
      if (user["attributes"][attributeKey] != null)
      {
        var existingValues = user["attributes"][attributeKey].ToObject<JArray>();
        existingValues.Add(attributeValueToAdd);
        user["attributes"][attributeKey] = existingValues;
      }
      else
      {
        user["attributes"][attributeKey] = new JArray(attributeValueToAdd);
      }


      string updatedUserData = user.ToString();


      bool updateResult = await UpdateUserAttributes(baseUrl, realm, userId, accessToken, updatedUserData);

      if (updateResult)
      {
        logger.LogInformation("{Function} attributes added successfully", "SetMinioUserAttribute");
        return true;
      }

      logger.LogError("{Function} Failed to update user attributes", "SetMinioUserAttribute");
      return true;
    }

    logger.LogError("{Function} Failed to retrieve user attributes", "SetMinioUserAttribute");
    return true;
  }
  
  public async Task<bool> RemoveMinioUserAttribute(string accessToken, string userName, string attributeName, string attributeValueToRemove)
  {
    var baseUrl = _submissionKeyCloakSettings.Server;
    var realm = _submissionKeyCloakSettings.Realm;
    var attributeKey = "policy";
    var userId = await GetUserIdAsync(accessToken, userName);
    var userAttributesJson = await GetUserAttributesAsync(baseUrl, realm, accessToken, userId);

    {

      JObject user = JObject.Parse(userAttributesJson);

      if (user["attributes"][attributeKey] != null)
      {

        var existingValues = user["attributes"][attributeKey].ToObject<JArray>();


        var updatedValues = new JArray();


        foreach (var value in existingValues)
        {
          if (value.ToString() != attributeValueToRemove)
          {
            updatedValues.Add(value);
          }
        }

        user["attributes"][attributeKey] = updatedValues;
      }

      string updatedUserData = user.ToString();

      bool updateResult = await UpdateUserAttributes(baseUrl, realm, userId, accessToken, updatedUserData);

      if (updateResult)
      {
        logger.LogInformation("{Function} attributes added successfully.", "RemoveMinioUserAttribute");
        return true;
      }

      logger.LogError("{Function} Failed to update user attributes.", "RemoveMinioUserAttribute");
      return false;
    }
  }
  public async Task<string> GetUserIdAsync(string accessToken, string userName)
  {
    var baseUrl = _submissionKeyCloakSettings.Server;
    var realm = _submissionKeyCloakSettings.Realm;
    HttpClient httpClient = new HttpClient(_submissionKeyCloakSettings.getProxyHandler);
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var apiUrl = $"https://{baseUrl}/admin/realms/{realm}/users?username={userName}";

    var response = await httpClient.GetAsync(apiUrl);

    var jsonString = await response.Content.ReadAsStringAsync();
    JArray jsonObject = JsonConvert.DeserializeObject<JArray>(jsonString);
    foreach (var item in jsonObject)
    {
      if (item["username"].ToString().ToLower() == userName.ToLower())
      {
        return item["id"].ToString();
      }
    }

    return string.Empty;
    //throw new Exception("User not found");
  }
  
  public async Task<string> GetUserAttributesAsync(string baseUrl, string realm, string accessToken, string userID)
  {
    using (var httpClient = new HttpClient(_submissionKeyCloakSettings.getProxyHandler))
    {
      httpClient.BaseAddress = new Uri($"https://{baseUrl}/admin/realms/{realm}/users/{userID}");
      httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      var response = await httpClient.GetAsync("");
      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }
  }
  
  public async Task<bool> UpdateUserAttributes(string keycloakBaseUrl, string realm, string userId, string accessToken, string updatedUserData)
  {
    using (var httpClient = new HttpClient(_submissionKeyCloakSettings.getProxyHandler))
    {
      httpClient.BaseAddress = new Uri($"https://{keycloakBaseUrl}/admin/realms/{realm}/users/{userId}");
      httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      var content = new StringContent(updatedUserData, System.Text.Encoding.UTF8, "application/json");
      var response = await httpClient.PutAsync("", content);
      var stream = response.Content.ReadAsStream();
      string content2 = "";
      using (StreamReader reader = new StreamReader(stream))
      {
        // Read the stream content into a string
        content2 = reader.ReadToEnd();

        // Output the string content

      }
      return response.IsSuccessStatusCode;
    }
  }
}
