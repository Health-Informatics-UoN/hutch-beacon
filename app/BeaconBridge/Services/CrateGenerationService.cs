using System.Text.RegularExpressions;
using BeaconBridge.Config;
using BeaconBridge.Constants;
using BeaconBridge.Utilities;
using FiveSafes.Net;
using FiveSafes.Net.Constants;
using FiveSafes.Net.Utilities;
using Flurl;
using Microsoft.Extensions.Options;
using ROCrates;
using ROCrates.Models;
using File = System.IO.File;

namespace BeaconBridge.Services;

public class CrateGenerationService(
  ILogger<CrateGenerationService> logger,
  IOptions<CratePublishingOptions> publishingOptions,
  IOptions<CrateAgentOptions> agentOptions,
  IOptions<CrateProjectOptions> projectOptions,
  IOptions<CrateOrganizationOptions> organizationOptions,
  IOptions<WorkflowOptions> workflowOptions,
  IOptions<AssessActionsOptions> assessActions,
  IOptions<AgreementPolicyOptions> agreementPolicy)
{
  private readonly AgreementPolicyOptions _agreementPolicyOptions = agreementPolicy.Value;
  private readonly AssessActionsOptions _assessActionsOptions = assessActions.Value;
  private readonly CrateAgentOptions _crateAgentOptions = agentOptions.Value;
  private readonly CrateOrganizationOptions _crateOrganizationOptions = organizationOptions.Value;
  private readonly CrateProjectOptions _crateProjectOptions = projectOptions.Value;
  private readonly CratePublishingOptions _publishingOptions = publishingOptions.Value;
  private readonly WorkflowOptions _workflowOptions = workflowOptions.Value;

  /// <summary>
  /// Build an RO-Crate.
  /// </summary>
  /// <param name="bagItPath">The BagItArchive path to save the crate to.</param>
  /// <param name="input">The inputs for the workflow.</param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException">Query type is unavailable.</exception>
  public async Task<BagItArchive> BuildCrate(string input, string bagItPath)
  {
    var workflowUri = GetWorkflowUrl();
    var archive = await BuildBagIt(bagItPath, workflowUri);

    // Generate ROCrate metadata
    logger.LogInformation("Building Five Safes ROCrate...");
    var builder = new RoCrateBuilder(_workflowOptions, _publishingOptions, _crateAgentOptions,
      _crateProjectOptions, _crateOrganizationOptions, archive.PayloadDirectoryPath, _agreementPolicyOptions);
    var crate = BuildFiveSafesCrate(builder);
    crate.Save(archive.PayloadDirectoryPath);
    logger.LogInformation("Saved Five Safes ROCrate to {ArchivePayloadDirectoryPath}", archive.PayloadDirectoryPath);
    await archive.WriteManifestSha512();
    await archive.WriteTagManifestSha512();

    return archive;
  }

  /// <summary>
  /// Build BagIt archive
  /// </summary>
  /// <param name="destination"></param>
  /// <param name="workflowUri"></param>
  /// <returns></returns>
  public async Task<BagItArchive> BuildBagIt(string destination, string workflowUri)
  {
    var builder = new FiveSafesBagItBuilder(destination);
    builder.BuildCrate(workflowUri);
    await builder.BuildChecksums();
    await builder.BuildTagFiles();
    return builder.GetArchive();
  }

  public ROCrate BuildFiveSafesCrate(RoCrateBuilder builder)
  {
    builder.AddLicense();
    builder.AddCreateAction();
    builder.AddAgent();
    builder.UpdateMainEntity();
    return builder.GetROCrate();
  }

  /// <summary>
  /// Construct the Workflow URL from WorkflowOptions.
  /// </summary>
  /// <returns>Workflow URL</returns>
  public string GetWorkflowUrl()
  {
    return Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString())
      .SetQueryParam("version", _workflowOptions.Version.ToString());
  }

  public async Task AssessBagIt(BagItArchive archive)
  {
    var builder = new RoCrateBuilder(_workflowOptions, _publishingOptions, _crateAgentOptions,
      _crateProjectOptions, _crateOrganizationOptions, archive.PayloadDirectoryPath, _agreementPolicyOptions);
    var validator = new Part() { Id = $"validator-{Guid.NewGuid()}" };
    if (_assessActionsOptions.CheckValue)
    {
      var manifestPath = Path.Combine(archive.ArchiveRootPath, BagItConstants.ManifestPath);
      var tagManifestPath = Path.Combine(archive.ArchiveRootPath, BagItConstants.TagManifestPath);

      var bothFilesExist = File.Exists(manifestPath) && File.Exists(tagManifestPath);
      var checkSumsMatch = await ChecksumsMatch(manifestPath, archive.ArchiveRootPath) &&
                           await ChecksumsMatch(tagManifestPath, archive.ArchiveRootPath);

      if (bothFilesExist && checkSumsMatch)
      {
        builder.AddCheckValueAssessAction(ActionStatus.CompletedActionStatus, DateTime.Now, validator);
      }
      else
      {
        builder.AddCheckValueAssessAction(ActionStatus.FailedActionStatus, DateTime.Now, validator);
      }
    }

    if (_assessActionsOptions.Validate)
    {
      builder.AddValidateCheck(ActionStatus.CompletedActionStatus, validator);
    }

    if (_assessActionsOptions.SignOff)
    {
      builder.AddSignOff();
    }

    var crate = builder.GetROCrate();
    crate.Save(archive.PayloadDirectoryPath);
    await archive.WriteTagManifestSha512();
    await archive.WriteManifestSha512();
  }

  /// <summary>
  /// Check that the actual checksums of the files match the recorded checksums.
  /// </summary>
  /// <param name="checksumFilePath">The path to the checksum file containing records that need validating.</param>
  /// <param name="archiveRoot">Path to the root of the archive.</param>
  /// <returns></returns>
  private async Task<bool> ChecksumsMatch(string checksumFilePath, string archiveRoot)
  {
    var lines = await File.ReadAllLinesAsync(checksumFilePath);
    foreach (var line in lines)
    {
      var checksumAndFile = Regex.Split(line, @"\s+");
      var expectedChecksum = checksumAndFile.First();
      var fileName = checksumAndFile.Last();

      await using var fileStream = File.OpenRead(Path.Combine(archiveRoot, fileName));
      var fileChecksum = ChecksumUtility.ComputeSha512(fileStream);
      if (fileChecksum != expectedChecksum) return false;
    }

    return true;
  }
}
