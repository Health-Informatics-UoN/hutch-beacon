using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using BeaconBridge.Config;
using BeaconBridge.Constants;
using BeaconBridge.Utilities;
using FiveSafes.Net;
using FiveSafes.Net.Constants;
using FiveSafes.Net.Utilities;
using Flurl;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using ROCrates;
using ROCrates.Models;
using Scriban;
using UoN.ZipBuilder;
using File = System.IO.File;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BeaconBridge.Services;

public class CrateGenerationService(
  ILogger<CrateGenerationService> logger,
  IOptions<CratePublishingOptions> publishingOptions,
  IOptions<CrateAgentOptions> agentOptions,
  IOptions<CrateProjectOptions> projectOptions,
  IOptions<CrateOrganizationOptions> organizationOptions,
  IOptions<WorkflowOptions> workflowOptions,
  IOptions<AssessActionsOptions> assessActions,
  IOptions<AgreementPolicyOptions> agreementPolicy,
  IFeatureManager featureFlags
)
{
  private readonly AgreementPolicyOptions _agreementPolicyOptions = agreementPolicy.Value;
  private readonly AssessActionsOptions _assessActionsOptions = assessActions.Value;
  private readonly CrateAgentOptions _crateAgentOptions = agentOptions.Value;
  private readonly CrateOrganizationOptions _crateOrganizationOptions = organizationOptions.Value;
  private readonly CrateProjectOptions _crateProjectOptions = projectOptions.Value;
  private readonly CratePublishingOptions _publishingOptions = publishingOptions.Value;
  private readonly WorkflowOptions _workflowOptions = workflowOptions.Value;

  private readonly string[] _tagFiles =
    { BagItConstants.BagitTxtPath, BagItConstants.BagInfoTxtPath, BagItConstants.ManifestPath };

  /// <summary>
  /// Build an RO-Crate.
  /// </summary>
  /// <param name="filters">The input filters for the crate.</param>
  /// <param name="bagItPath">The BagItArchive path to save the crate to.</param>
  /// <returns>Zip file as byte array</returns>
  /// <exception cref="NotImplementedException">Query type is unavailable.</exception>
  public async Task<byte[]> BuildCrate(string filters, string bagItPath)
  {
    // Generate ROCrate metadata
    logger.LogInformation("Building Five Safes ROCrate...");

    var builder = new RoCrateBuilder(_workflowOptions, _publishingOptions, _crateAgentOptions, _crateProjectOptions,
      _crateOrganizationOptions, _agreementPolicyOptions, _assessActionsOptions);

    var crate = await BuildFiveSafesCrate(builder, filters);

    // Metadata
    var jsonMetadata = ParseMetadata(crate);
    var crateMetaPath = Path.Combine("data", "ro-crate-metadata.json");

    // Preview
    var contents = ParsePreview(crate);
    var cratePreviewPath = Path.Combine("data", "ro-crate-preview.html");

    var crateFiles = new Dictionary<string, byte[]>();
    crateFiles.Add(crateMetaPath, JsonSerializer.SerializeToUtf8Bytes(jsonMetadata));
    crateFiles.Add(cratePreviewPath, JsonSerializer.SerializeToUtf8Bytes(contents));

    var zipBuilder = new ZipBuilder()
      .CreateZipStream() // initialise the archive
      .AddBytes(crateFiles[crateMetaPath], crateMetaPath)
      .AddBytes(crateFiles[cratePreviewPath], cratePreviewPath);

    logger.LogInformation("Created Five Safes ROCrate");
    var bagitManifest = WriteManifestSha512(crateFiles);
    var bagIt = WriteBagitTxt();
    var bagItInfo = WriteBagInfoTxt();

    var bagitFiles = new Dictionary<string, byte[]>();
    bagitFiles.Add(BagItConstants.BagitTxtPath, bagIt);
    bagitFiles.Add(BagItConstants.BagInfoTxtPath, bagItInfo);
    bagitFiles.Add(BagItConstants.ManifestPath, bagitManifest);

    zipBuilder.AddBytes(bagIt, BagItConstants.BagitTxtPath);
    zipBuilder.AddBytes(bagItInfo, BagItConstants.BagInfoTxtPath);

    zipBuilder.AddBytes(bagitManifest, BagItConstants.ManifestPath);
    zipBuilder.AddBytes(WriteTagManifestSha512(bagitFiles),BagItConstants.TagManifestPath);

    return zipBuilder.AsByteArray();
  }

  /// <summary>
  /// Build BagIt archive
  /// </summary>
  /// <param name="destination"></param>
  /// <param name="workflowUri"></param>
  /// <returns></returns>
  public async Task<BagItArchive> BuildBagIt(string destination, string workflowUri)
  {
    var builder = new FiveSafesBagItBuilder();
    builder.BuildCrate(workflowUri);
    await builder.BuildChecksums();
    await builder.BuildTagFiles();
    return builder.GetArchive();
  }

  public async Task<ROCrate> BuildFiveSafesCrate(RoCrateBuilder builder, string filters)
  {
    builder.AddLicense();
    builder.AddCreateAction(filters);
    builder.AddAgent();
    builder.UpdateMainEntity();
    if (await featureFlags.IsEnabledAsync(FeatureFlags.MakeAssessActions))
      builder.AssessBagIt();
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

  private JsonObject ParseMetadata(ROCrate crate)
  {
    // Parse Metadata
    JsonObject jsonObject = new JsonObject()
    {
      {
        "@context",
        (JsonNode)"https://w3id.org/ro/crate/1.1/context"
      }
    };
    JsonArray jsonArray = new JsonArray(new JsonNodeOptions?());
    foreach (KeyValuePair<string, Entity> entity in crate.Entities)
      jsonArray.Add(JsonNode.Parse(entity.Value.Serialize()));
    jsonObject.Add("@graph", (JsonNode)jsonArray);
    return jsonObject;
  }

  private string ParsePreview(ROCrate crate)
  {
    string contents;

    using StreamReader streamReader =
      new StreamReader(
        typeof(Preview).Assembly.GetManifestResourceStream("ROCrates.Templates.ro-crate-preview.html")!,
        Encoding.UTF8);
    contents = Template.Parse(streamReader.ReadToEnd()).Render((object)new
    {
      data =
        crate.Entities.Values.Select<Entity, JsonObject>((Func<Entity, JsonObject>)(entity => entity.Properties)),
      root_dataset = crate?.RootDataset
    });

    return contents;
  }

  public byte[] WriteManifestSha512(Dictionary<string, byte[]> entries)
  {
    var manifestStream = new MemoryStream();
    foreach (var entry in entries)
    {
      using var stream = new MemoryStream(entry.Value);
      var checksum = ChecksumUtility.ComputeSha512(stream);
      // Note there should be 2 spaces between the checksum and the file path
      // The path should be relative to bagitDir
      manifestStream.Write(Encoding.UTF8.GetBytes($"{checksum}  {entry.Key}"));
      manifestStream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
    }

    return manifestStream.ToArray();
  }

  /// <summary>
  /// Compute the SHA512 for the <c>bagit.txt</c>, <c>bag-info.txt</c> and <c>manifest-sha512.txt</c> and
  /// write a <c>tagmanifest-sha512.txt</c> to the archive.
  /// </summary>
  public byte[] WriteTagManifestSha512(Dictionary<string, byte[]> bagitFiles)
  {
    var tagManifestStream = new MemoryStream();
    foreach (var tagFile in _tagFiles)
    {
      using var stream = new MemoryStream(bagitFiles[tagFile]);
      var checksum = ChecksumUtility.ComputeSha512(stream);
      // // Note there should be 2 spaces between the checksum and the file path
      tagManifestStream.Write(Encoding.UTF8.GetBytes($"{checksum}  {tagFile}"));
      tagManifestStream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
    }

    return tagManifestStream.ToArray();
  }

  public byte[] WriteBagitTxt()
  {
    string[] contents = { "BagIt-Version: 1.0", "Tag-File-Character-Encoding: UTF-8" };
    using var stream = new MemoryStream();
    foreach (var line in contents)
    {
      stream.Write(Encoding.UTF8.GetBytes(line));
      stream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
    }

    return stream.ToArray();
  }

  public byte[] WriteBagInfoTxt()
  {
    var contents = "External-Identifier: urn:uuid:{0}";
    using var stream = new MemoryStream();
    stream.Write(Encoding.UTF8.GetBytes(string.Format(contents, Guid.NewGuid().ToString())));
    stream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
    return stream.ToArray();
  }
}
