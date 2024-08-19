using System.IO.Compression;
using System.Text.Json;
using BeaconBridge.Config;
using BeaconBridge.Constants.Submission;
using BeaconBridge.Data.Entities.Submission;
using BeaconBridge.Models;
using BeaconBridge.Models.Submission;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using TesTask = BeaconBridge.Models.Submission.Tes.TesTask;

namespace BeaconBridge.Services;

public class TesSubmissionService
{
  private readonly ILogger<TesSubmissionService> _logger;
  private readonly OpenIdIdentityService _openIdIdentity;
  private readonly OpenIdOptions _openIdOptions;
  private readonly SubmissionOptions _submissionOptions;
  private readonly string _identityToken;

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
  public async Task<Models.TesTask> SubmitTesTask(TesTask task)
  {
    var req = _submissionOptions.SubmissionLayerHost
      .AppendPathSegments("v1", "tasks")
      .WithOAuthBearerToken(_identityToken);

    try
    {
      var response = await req.PostJsonAsync(task).ReceiveJson<TesTaskResponse>();
      _logger.LogInformation("TES task submitted successfully");
      var tesTask = new Models.TesTask() { SubId = response.Id, Id = task.Name };
      return tesTask;
    }
    catch (FlurlHttpException e)
    {
      var error = await e.GetResponseStringAsync();
      _logger.LogError("Could not submit TES Task to Submission Layer. Reason: {Message}", error);
      throw;
    }
  }

  public async Task<ResponseSummary> DownloadResults(Models.TesTask tesTask)
  {
    // Account for submission layer tre branching ids
    var subId = Int32.Parse(tesTask.SubId) + 1;

    var req = _submissionOptions.SubmissionLayerHost
      .AppendPathSegments("api")
      .AppendPathSegments("Submission", "DownloadFile")
      .SetQueryParam("submissionId", subId)
      .WithOAuthBearerToken(_identityToken);

    var responseSummary = new ResponseSummary();
    try
    {
      var response = await req.GetBytesAsync();
      _logger.LogInformation("Successfully downloaded results crate.");

      // Load stream into ZipArchive
      var memoryStream = new MemoryStream(response);
      var zipArchive = new ZipArchive(memoryStream);

      foreach (var entry in zipArchive.Entries)
      {
        // Look for output file
        if (entry.Name.EndsWith("output.json"))
        {
          responseSummary = JsonSerializer.Deserialize<ResponseSummary>(entry.Open());
        }
      }
    }
    catch (FlurlHttpException e)
    {
      var error = await e.GetResponseStringAsync();
      _logger.LogError("Could not download results crate {Message}", error);
      throw;
    }

    return responseSummary ?? throw new NullReferenceException();
  }

  public async Task<StatusType> CheckStatus(Models.TesTask tesTask)
  {
    // account for sub layer having different sub ids
    var subId = Int32.Parse(tesTask.SubId) + 1;

    var req = _submissionOptions.SubmissionLayerHost
      .AppendPathSegments("api")
      .AppendPathSegments("Submission", "GetASubmission", subId)
      .WithOAuthBearerToken(_identityToken);
    try
    {
      var submissionResponse = await req.GetAsync().ReceiveJson<Submission>();
      _logger.LogInformation("Current submission status is: {status}", submissionResponse.Status);
      return submissionResponse.Status;
    }
    catch (FlurlHttpException e)
    {
      var error = await e.GetResponseStringAsync();
      _logger.LogError("Could not get submission status update. {Message}", error);
      throw;
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
