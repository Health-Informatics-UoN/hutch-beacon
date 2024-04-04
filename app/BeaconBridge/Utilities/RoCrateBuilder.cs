using BeaconBridge.Config;
using ROCrates;
using ROCrates.Models;

namespace BeaconBridge.Utilities;

public class RoCrateBuilder
{
  private readonly AgreementPolicyOptions _agreementPolicy;
  private readonly CrateAgentOptions _crateAgentOptions;
  private readonly CrateOrganizationOptions _crateOrganizationOptions;
  private readonly CrateProjectOptions _crateProjectOptions;
  private readonly CratePublishingOptions _publishingOptions;
  private readonly WorkflowOptions _workflowOptions;
  private ROCrate _crate = new();

  public RoCrateBuilder(WorkflowOptions workflowOptions, CratePublishingOptions publishingOptions,
    CrateAgentOptions crateAgentOptions, CrateProjectOptions crateProjectOptions,
    CrateOrganizationOptions crateOrganizationOptions,
    string archivePayloadDirectoryPath, AgreementPolicyOptions agreementPolicy)
  {
    _workflowOptions = workflowOptions;
    _publishingOptions = publishingOptions;
    _crateAgentOptions = crateAgentOptions;
    _crateProjectOptions = crateProjectOptions;
    _crateOrganizationOptions = crateOrganizationOptions;
    _agreementPolicy = agreementPolicy;

    _crate.Initialise(archivePayloadDirectoryPath);
    AddProject();
    AddOrganisation();
  }

  public RoCrateBuilder(WorkflowOptions workflowOptions, CratePublishingOptions publishingOptions,
    CrateAgentOptions crateAgentOptions, CrateProjectOptions crateProjectOptions,
    CrateOrganizationOptions crateOrganizationOptions, AgreementPolicyOptions agreementPolicy)
  {
    _workflowOptions = workflowOptions;
    _publishingOptions = publishingOptions;
    _crateAgentOptions = crateAgentOptions;
    _crateProjectOptions = crateProjectOptions;
    _crateOrganizationOptions = crateOrganizationOptions;
    _agreementPolicy = agreementPolicy;

    AddProject();
    AddOrganisation();
  }

  /// <summary>
  /// Returns the <c>ROCrate</c> that has been built.
  /// </summary>
  /// <returns>The <c>ROCrate</c> that has been built.</returns>
  public ROCrate GetROCrate()
  {
    var result = _crate;
    ResetCrate();
    return result;
  }

  /// <summary>
  /// Resets the <c>ROCrate</c> object in the builder.
  /// </summary>
  private void ResetCrate()
  {
    _crate = new ROCrate();
  }

  /// <summary>
  /// Adds Project Entity as configured
  /// </summary>
  /// <returns></returns>
  private void AddProject()
  {
    var projectEntity = new Entity(identifier: $"#project-{Guid.NewGuid()}");
    projectEntity.SetProperty("@type", _crateProjectOptions.Type);
    projectEntity.SetProperty("name", _crateProjectOptions.Name);
    projectEntity.SetProperty("identifier", _crateProjectOptions.Identifiers);
    projectEntity.SetProperty("funding", _crateProjectOptions.Funding);
    projectEntity.SetProperty("member", _crateProjectOptions.Member);
    _crate.Add(projectEntity);
  }

  /// <summary>
  /// Adds Organisation Entity as configured.
  /// </summary>
  /// <returns></returns>
  private void AddOrganisation()
  {
    var orgEntity = new Entity(identifier: _crateOrganizationOptions.Id);
    orgEntity.SetProperty("@type", _crateOrganizationOptions.Type);
    orgEntity.SetProperty("name", _crateOrganizationOptions.Name);
    _crate.Add(orgEntity);
  }
}
