using System.IO.Compression;
using System.Text.Json;
using BeaconBridge.Config;
using BeaconBridge.Constants;
using BeaconBridge.Constants.Submission;
using BeaconBridge.Data.Entities.Submission;
using BeaconBridge.Models;
using BeaconBridge.Models.Egress;
using BeaconBridge.Models.Submission;
using BeaconBridge.Models.Submission.Tes;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using TesTask = BeaconBridge.Models.Submission.Tes.TesTask;

namespace BeaconBridge.Services;

public class TesSubmissionService
{
  private readonly ILogger<TesSubmissionService> _logger;
  private readonly OpenIdIdentityService _openIdIdentity;
  private readonly OpenIdOptions _openIdOptions;
  private readonly OpenIdOptions _egressOpenIdOptions;
  private readonly SubmissionOptions _submissionOptions;
  private readonly EgressOptions _egressOptions;
  private readonly string _identityToken;
  private readonly string _egressIdentityToken;
  private readonly TesTaskOptions _tesTaskOptions;
  private readonly MinioStoreServiceFactory _minioStore;
  private readonly IFeatureManager _featureFlags;
  private readonly MinioOptions _minioOptions;

  public TesSubmissionService(IOptions<SubmissionOptions> submissionOptions, OpenIdIdentityService openIdIdentity,
    IOptionsSnapshot<OpenIdOptions> openIdOptions, ILogger<TesSubmissionService> logger,
    IOptions<EgressOptions> egressOptions, IOptions<TesTaskOptions> tesTaskOptions, IFeatureManager featureFlags,
    MinioStoreServiceFactory minioStore, IOptions<MinioOptions> minioOptions)

  {
    _openIdIdentity = openIdIdentity;
    _openIdOptions = openIdOptions.Get(OpenIdOptions.Submission);
    _submissionOptions = submissionOptions.Value;
    _logger = logger;
    _featureFlags = featureFlags;
    _minioStore = minioStore;
    _minioOptions = minioOptions.Value;
    _tesTaskOptions = tesTaskOptions.Value;
    _egressOpenIdOptions = openIdOptions.Get(OpenIdOptions.Egress);
    _egressOptions = egressOptions.Value;
    _identityToken = _openIdIdentity.GetAuthorised(_openIdOptions).Result;
    _egressIdentityToken = _openIdIdentity.GetAuthorised(_egressOpenIdOptions).Result;
  }

  public TesTask CreateTesTask(string beaconTaskId, string filters)
  {
    var tesTask = new TesTask()
    {
      Name = $"beacon-{beaconTaskId}",
      Outputs = new List<TesOutput>()
      {
        new()
        {
          Name = _tesTaskOptions.Outputs.Name,
          Url = _tesTaskOptions.Outputs.Url,
          Path = _tesTaskOptions.Outputs.Path,
          Type = TesFileType.DIRECTORYEnum,
          Description = "Results for beacon query"
        }
      },
      Executors = new List<TesExecutor>()
      {
        new()
        {
          Image = $"{_tesTaskOptions.BeaconImage.Image}:{_tesTaskOptions.BeaconImage.Version}",

          Command = new List<string>()
          {
            "beacon-omop-worker",
            "individuals",
            "-f",
            filters
          },
          Stdout = $"{_tesTaskOptions.Outputs.Path}/stdout",
          Env = _tesTaskOptions.Env,
          Workdir = "/outputs"
        }
      },
      Tags = new Dictionary<string, string>()
      {
        { "project", _submissionOptions.ProjectName },
        { "tres", string.Join('|', _submissionOptions.Tres) }
      },
    };
    return tesTask;
  }

  /// <summary>
  /// Get a Submitted TES Task from the Sub. Layer API
  /// </summary>
  /// <param name="tesTask"></param>
  /// <returns></returns>
  public async Task<Submission> GetTesTask(Models.TesTask tesTask)
  {
    var subId = Int32.Parse(tesTask.SubId) + 1;

    var req = _submissionOptions.SubmissionLayerHost
      .AppendPathSegments("api")
      .AppendPathSegments("Submission", "GetASubmission", subId)
      .WithOAuthBearerToken(_identityToken);
    try
    {
      var task = await req.GetAsync().ReceiveJson<Submission>();
      return task;
    }
    catch (FlurlHttpException e)
    {
      var error = await e.GetResponseStringAsync();
      _logger.LogError("Could not get task. {Message}", error);
      throw;
    }
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

  public async Task<ResponseSummary> DownloadResults(int taskId, string outputBucket)
  {
    var responseSummary = new ResponseSummary();
    if (await _featureFlags.IsEnabledAsync(FeatureFlags.UseRoCrate))
    {
      var req = _submissionOptions.SubmissionLayerHost
        .AppendPathSegments("api")
        .AppendPathSegments("Submission", "DownloadFile")
        .SetQueryParam("submissionId", taskId)
        .WithOAuthBearerToken(_identityToken);
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
    }
    else
    {
      var store = await _minioStore.Create(new MinioOptions()
        { Host = _minioOptions.Host, Bucket = outputBucket, Secure = _minioOptions.Secure });
      // Get results from output bucket
      var file = await store.GetFromStore($"{taskId}/output.json");
      
      responseSummary = JsonSerializer.Deserialize<ResponseSummary>(file);
    }

    return responseSummary ?? throw new NullReferenceException("Could not unpack response summary from output.json");
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

  public async Task ApproveEgress(Models.TesTask tesTask)
  {
    // account for sub layer having different sub ids
    var subId = Int32.Parse(tesTask.SubId) + 1;

    var reqAllEgress = _egressOptions.EgressLayerHost
      .AppendPathSegments("api")
      .AppendPathSegments("DataEgress")
      .AppendPathSegments("GetAllEgresses")
      .AppendQueryParam("unprocessedonly", true)
      .WithOAuthBearerToken(_egressIdentityToken);

    try
    {
      // Get all unprocessed Egress requests
      var allEgressResponse = await reqAllEgress.GetAsync().ReceiveJson<List<EgressSubmission>>();
      var egressRequest = allEgressResponse.First(submission => submission.SubmissionId == subId.ToString());
      _logger.LogInformation("Submission {id} is waiting for Egress approval", egressRequest.SubmissionId);

      // Approve
      var reqApproveEgress = _egressOptions.EgressLayerHost
        .AppendPathSegments("api")
        .AppendPathSegments("DataEgress")
        .AppendPathSegments("CompleteEgress")
        .AppendQueryParam("id", egressRequest.Id)
        .WithOAuthBearerToken(_egressIdentityToken);

      await reqApproveEgress.PostJsonAsync(ApproveEgressSubmission(egressRequest));
      _logger.LogInformation("Successfully Approved Egress for tes submission: {egressResponse}",
        egressRequest.SubmissionId);
    }
    catch (FlurlHttpException e)
    {
      var error = await e.GetResponseStringAsync();
      _logger.LogError("Could not get available egress information. {Message}", error);
      throw;
    }
  }

  private EgressSubmission ApproveEgressSubmission(EgressSubmission egressSubmission)
  {
    egressSubmission.Status = EgressStatus.FullyApproved;
    egressSubmission.Completed = DateTime.Now;
    egressSubmission.Reviewer = _egressOpenIdOptions.Username;
    foreach (var file in egressSubmission.Files)
    {
      file.Status = FileStatus.Approved;
      file.Reviewer = _egressOpenIdOptions.Username;
    }

    return egressSubmission;
  }
}
