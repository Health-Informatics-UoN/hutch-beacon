using BeaconBridge.Config;
using BeaconBridge.Models.Submission.Tes;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;

namespace BeaconBridge.Services;

public class TesSubmissionService
{
  private readonly ILogger<TesSubmissionService> _logger;
  private readonly OpenIdIdentityService _openIdIdentity;
  private readonly OpenIdOptions _openIdOptions;
  private readonly SubmissionOptions _submissionOptions;
  private string _identityToken;

  public TesSubmissionService(IOptions<SubmissionOptions> submissionOptions, OpenIdIdentityService openIdIdentity,
    IOptions<OpenIdOptions> openIdOptions, ILogger<TesSubmissionService> logger)
  {
    _openIdIdentity = openIdIdentity;
    _openIdOptions = openIdOptions.Value;
    _submissionOptions = submissionOptions.Value;
    _logger = logger;
    _identityToken = GetAuthorised().Result;
  }

  /// <summary>
  /// Submit a TES task to the Submission Layer.
  /// </summary>
  /// <param name="task">The TES task to submit.</param>
  public async Task SubmitTesTask(TesTask task)
  {
    var req = _submissionOptions.SubmissionLayerHost
      .AppendPathSegments("v1", "tasks")
      .WithOAuthBearerToken(_identityToken);

    try
    {
      await req.PostJsonAsync(task);
      _logger.LogInformation("TES task submitted successfully");
    }
    catch (FlurlHttpException e)
    {
      var error = await e.GetResponseStringAsync();
      _logger.LogCritical("Could not submit TES Task to Submission Layer. Reason: {Message}", error);
    }
  }

  /// <summary>
  /// Authorise this service with the Submission Layer.
  /// </summary>
  /// <returns>The identity token for the authorised user.</returns>
  private async Task<string> GetAuthorised()
  {
    try
    {
      var (identity, _, _) = await _openIdIdentity.RequestUserTokens(_openIdOptions);
      return identity;
    }
    catch (InvalidOperationException)
    {
      _logger.LogCritical("Could not get authorised with the Identity Provider");
      return string.Empty;
    }
  }
}
