using System.Globalization;
using System.Text.Json;
using BeaconBridge.Config;
using BeaconBridge.Constants;
using FiveSafes.Net.Constants;
using Flurl;
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
  private readonly AssessActionsOptions _assessActionsOptions;
  private readonly FiveSafesProfile _profile = new();
  private ROCrate _crate = new();

  public RoCrateBuilder(WorkflowOptions workflowOptions, CratePublishingOptions publishingOptions,
    CrateAgentOptions crateAgentOptions, CrateProjectOptions crateProjectOptions,
    CrateOrganizationOptions crateOrganizationOptions,
    string archivePayloadDirectoryPath, AgreementPolicyOptions agreementPolicy,
    AssessActionsOptions assessActionsOptions)
  {
    _workflowOptions = workflowOptions;
    _publishingOptions = publishingOptions;
    _crateAgentOptions = crateAgentOptions;
    _crateProjectOptions = crateProjectOptions;
    _crateOrganizationOptions = crateOrganizationOptions;
    _agreementPolicy = agreementPolicy;
    _assessActionsOptions = assessActionsOptions;

    _crate.Initialise(archivePayloadDirectoryPath);
    AddProject();
    AddOrganisation();
  }

  public RoCrateBuilder(WorkflowOptions workflowOptions, CratePublishingOptions publishingOptions,
    CrateAgentOptions crateAgentOptions, CrateProjectOptions crateProjectOptions,
    CrateOrganizationOptions crateOrganizationOptions, AgreementPolicyOptions agreementPolicy,
    AssessActionsOptions assessActionsOptions)
  {
    _workflowOptions = workflowOptions;
    _publishingOptions = publishingOptions;
    _crateAgentOptions = crateAgentOptions;
    _crateProjectOptions = crateProjectOptions;
    _crateOrganizationOptions = crateOrganizationOptions;
    _agreementPolicy = agreementPolicy;
    _assessActionsOptions = assessActionsOptions;
    AddRootDataset();
    AddProfile();
    AddMainEntity();
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
  /// Adds Agent Entity and links to relevant organisation and project.
  /// </summary>
  public void AddAgent()
  {
    var organisation = _crate.Entities.Values.First(x => x.Id == _crateOrganizationOptions.Id);
    var project = _crate.Entities.Values.First(x => x.Id.StartsWith("#project-"));
    var agentEntity = new Entity(identifier: _crateAgentOptions.Id);
    agentEntity.SetProperty("@type", _crateAgentOptions.Type);
    agentEntity.SetProperty("name", _crateAgentOptions.Name);
    agentEntity.SetProperty("affiliation", new Part() { Id = organisation.Id });
    agentEntity.AppendTo("memberOf", project);
    _crate.Add(agentEntity);
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

  /// <summary>
  /// Adds Licence Entity to Five Safes RO-Crate.
  /// </summary>
  public void AddLicense()
  {
    if (string.IsNullOrEmpty(_publishingOptions.License?.Uri)) return;

    var licenseProps = _publishingOptions.License.Properties;
    var licenseEntity = new CreativeWork(
      identifier: _publishingOptions.License.Uri,
      properties: JsonSerializer.SerializeToNode(licenseProps)?.AsObject());

    // Bug in ROCrates.Net: CreativeWork class uses the base constructor so @type is Thing by default
    licenseEntity.SetProperty("@type", "CreativeWork");

    _crate.Add(licenseEntity);

    _crate.RootDataset.SetProperty("license", new Part { Id = licenseEntity.Id });
  }
  
  /// <summary>
  /// Add RootDataset entity to Five Safes RO-Crate
  /// </summary>
  private void AddRootDataset()
  {
    _crate.RootDataset.SetProperty("conformsTo", new Part
    {
      Id = _profile.Id,
    });
    _crate.RootDataset.SetProperty("datePublished", DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture));
  }
  
  /// <summary>
  /// Add Profile entity to Five Safes RO-Crate
  /// </summary>
  private void AddProfile()
  {
    var profileEntity = new Entity(identifier: _profile.Id);
    profileEntity.SetProperty("@type", _profile.Type);
    profileEntity.SetProperty("name", _profile.Name);
    _crate.Add(profileEntity);
  }

  /// <summary>
  /// Add mainEntity to Five Safes RO-Crate.
  /// </summary>
  /// <exception cref="InvalidDataException">mainEntity not found in RO-Crate.</exception>
  public void AddMainEntity()
  {
    var workflowUri = GetWorkflowUrl();
    var mainEntity = new Dataset(identifier: workflowUri);

    mainEntity.SetProperty("name", _workflowOptions.Name);

    mainEntity.SetProperty("distribution", new Part
    {
      Id = Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString(), "ro_crate")
        .SetQueryParam("version", _workflowOptions.Version.ToString())
    });
    _crate.Add(mainEntity);
    _crate.RootDataset.SetProperty("mainEntity", new Part { Id = mainEntity.Id });
  }

  /// <summary>
  /// Update mainEntity to Five Safes RO-Crate.
  /// </summary>
  /// <exception cref="InvalidDataException">mainEntity not found in RO-Crate.</exception>
  public void UpdateMainEntity()
  {
    var workflowUri = GetWorkflowUrl();
    if (_crate.Entities.TryGetValue(workflowUri, out var mainEntity))
    {
      mainEntity.SetProperty("name", _workflowOptions.Name);

      mainEntity.SetProperty("distribution", new Part
      {
        Id = Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString(), "ro_crate")
          .SetQueryParam("version", _workflowOptions.Version.ToString())
      });
      _crate.Add(mainEntity);
      _crate.RootDataset.SetProperty("mainEntity", new Part { Id = mainEntity.Id });
    }
    else
    {
      throw new InvalidDataException("Could not find mainEntity in RO-Crate.");
    }
  }

  /// <summary>
  /// <para>Add the <c>CreateAction</c> to the RO-Crate.</para>
  /// </summary>
  public void AddCreateAction(string filters)
  {
    var createActionId = $"#query-{Guid.NewGuid()}";
    var createAction = new ContextEntity(_crate, createActionId);
    createAction.SetProperty("@type", "CreateAction");
    createAction.SetProperty("actionStatus", ActionStatus.PotentialActionStatus);

    _crate.Entities.TryGetValue(GetWorkflowUrl(), out var workflow);
    if (workflow is not null) createAction.SetProperty("instrument", new Part { Id = workflow.Id });
    createAction.SetProperty("name", "Beacon Request");
    // input filters
    var inputFiltersEntity = AddQueryTypeMetadata(filters);
    createAction.AppendTo("object", inputFiltersEntity);

    _crate.Add(createAction);
    _crate.RootDataset.AppendTo("mentions", createAction);
  }

  /// <summary>
  /// Add the metadata for the input query filters
  /// </summary>
  /// <param name="filters">The input query filters</param>
  /// <returns>The entity detailing the input query filters.</returns>
  private ContextEntity AddQueryTypeMetadata(string filters)
  {
    var paramId = "#{0}-inputs-{1}";
    var entityId = "#input_{0}";

    var inputFiltersParam =
      new ContextEntity(null,
        string.Format(paramId, _workflowOptions.Name, "filters"));
    inputFiltersParam.SetProperty("@type", "FormalParameter");
    inputFiltersParam.SetProperty("name", "filters");
    inputFiltersParam.SetProperty("dct:conformsTo", "https://bioschemas.org/profiles/FormalParameter/1.0-RELEASE/");
    var inputFiltersEntity = new ContextEntity(null,
      string.Format(entityId, "filters"));
    inputFiltersEntity.SetProperty("@type", "PropertyValue");
    inputFiltersEntity.SetProperty("name", "filters");
    inputFiltersEntity.SetProperty("value", filters);
    inputFiltersEntity.SetProperty("exampleOfWork", new Part { Id = inputFiltersParam.Id });

    _crate.Add(inputFiltersParam, inputFiltersEntity);
    return inputFiltersEntity;
  }

  public void AddCheckValueAssessAction(string status, DateTime startTime, Part validator)
  {
    var checkActionId = $"#check-{Guid.NewGuid()}";
    var checkAction = new ContextEntity(_crate, checkActionId);
    checkAction.SetProperty("startTime", startTime);
    checkAction.SetProperty("@type", "AssessAction");
    checkAction.SetProperty("additionalType", new Part() { Id = "https://w3id.org/shp#CheckValue" });
    var statusMsg = GetStatus(status);
    checkAction.SetProperty("name", $"BagIt checksum of Crate: {statusMsg}");
    checkAction.SetProperty("actionStatus", status);
    checkAction.SetProperty("object", new Part { Id = _crate.RootDataset.Id });

    var instrument = new Entity { Id = "https://www.iana.org/assignments/named-information#sha-512" };
    instrument.SetProperty("@type", "DefinedTerm");
    instrument.SetProperty("name", "sha-512 algorithm");
    checkAction.SetProperty("instrument", new Part() { Id = instrument.Id });
    checkAction.SetProperty("agent", validator);
    checkAction.SetProperty("endTime", DateTime.Now);
    _crate.RootDataset.AppendTo("mentions", checkAction);
    _crate.Add(checkAction, instrument);
  }

  public void AddValidateCheck(string status, Part validator)
  {
    var profile = _crate.RootDataset.GetProperty<Part>("conformsTo") ??
                  throw new NullReferenceException("No profile found in RootDataset");

    var validateId = $"#validate - {Guid.NewGuid()}";
    var validateAction = new ContextEntity(_crate, validateId);
    validateAction.SetProperty("startTime", DateTime.Now);
    validateAction.SetProperty("@type", "AssessAction");
    validateAction.SetProperty("additionalType", new Part() { Id = "https://w3id.org/shp#ValidationCheck" });

    validateAction.SetProperty("name", $"Validation against Five Safes RO-Crate profile: approved");
    validateAction.SetProperty("actionStatus", status);
    validateAction.SetProperty("object", new Part { Id = _crate.RootDataset.Id });
    validateAction.SetProperty("instrument", new Part() { Id = profile.Id });
    validateAction.SetProperty("agent", validator);
    validateAction.SetProperty("endTime", DateTime.Now);
    _crate.RootDataset.AppendTo("mentions", validateAction);

    _crate.Add(validateAction);
  }

  public void AddSignOff()
  {
    var signOffEntity = new ContextEntity(identifier: $"#signoff-{Guid.NewGuid()}");
    signOffEntity.SetProperty("@type", "AssessAction");
    signOffEntity.SetProperty("additionalType", new Part { Id = "https://w3id.org/shp#SignOff" });
    signOffEntity.SetProperty("name", "Sign-off of execution according to Agreement policy");
    signOffEntity.SetProperty("endTime", DateTime.Now);
    _crate.Entities.TryGetValue(_crateAgentOptions.Id, out var agent);
    signOffEntity.SetProperty("agent", new Part() { Id = agent!.Id });
    var projectId = _crate.Entities.Keys.First(x => x.StartsWith("#project-"));
    signOffEntity.SetProperty("object", new Part[]
    {
      new() { Id = _crate.RootDataset.Id },
      new() { Id = GetWorkflowUrl() },
      new() { Id = projectId },
    });
    signOffEntity.SetProperty("actionStatus", ActionStatus.CompletedActionStatus);
    var agreementPolicyEntity = new CreativeWork(identifier: _agreementPolicy.Id);
    signOffEntity.SetProperty("instrument", new Part { Id = _agreementPolicy.Id });
    // Manually set type due to bug in ROCrates.Net
    agreementPolicyEntity.SetProperty("@type", "CreativeWork");
    agreementPolicyEntity.SetProperty("name", _agreementPolicy.Name);

    _crate.RootDataset.AppendTo("mentions", signOffEntity);
    _crate.Add(signOffEntity, agreementPolicyEntity);
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

  private string GetStatus(string status)
  {
    return status switch
    {
      ActionStatus.CompletedActionStatus => "completed",
      ActionStatus.ActiveActionStatus => "active",
      ActionStatus.FailedActionStatus => "failed",
      ActionStatus.PotentialActionStatus => "potential",
      _ => ""
    };
  }

  /// <summary>
  /// Add AssessActions to RO-Crate if configured
  /// </summary>
  public void AssessBagIt()
  {
    var validator = new Part() { Id = $"validator-{Guid.NewGuid()}" };
    if (_assessActionsOptions.CheckValue)
    {
      AddCheckValueAssessAction(ActionStatus.CompletedActionStatus, DateTime.Now, validator);
    }

    if (_assessActionsOptions.Validate)
    {
      AddValidateCheck(ActionStatus.CompletedActionStatus, validator);
    }

    if (_assessActionsOptions.SignOff)
    {
      AddSignOff();
    }
  }
}
