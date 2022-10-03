// Program: SP_CAB_CREATE_LEGAL_ACTION_ASSGN, ID: 372318278, model: 746.
// Short name: SWE01824
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_LEGAL_ACTION_ASSGN.
/// </summary>
[Serializable]
public partial class SpCabCreateLegalActionAssgn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_LEGAL_ACTION_ASSGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateLegalActionAssgn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateLegalActionAssgn.
  /// </summary>
  public SpCabCreateLegalActionAssgn(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 10/24/96 Rick Delgado               Initial Development
    // 10/27/97  Siraj Konkader            Performance tuning. Replaced 
    // persistent views, moved edits to PRAD.
    // ------------------------------------------------------------
    if (ReadLegalAction())
    {
      if (ReadOfficeServiceProvider())
      {
        try
        {
          CreateLegalActionAssigment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_LEGAL_ACTION_ASSIGN_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";
    }
  }

  private void CreateLegalActionAssigment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var lgaIdentifier = entities.LegalAction.Identifier;
    var ospEffectiveDate = entities.OfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.OfficeServiceProvider.RoleCode;
    var offGeneratedId = entities.OfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId = entities.OfficeServiceProvider.SpdGeneratedId;
    var effectiveDate = import.LegalActionAssigment.EffectiveDate;
    var discontinueDate = import.LegalActionAssigment.DiscontinueDate;
    var reasonCode = import.LegalActionAssigment.ReasonCode;
    var overrideInd = import.LegalActionAssigment.OverrideInd;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = import.LegalActionAssigment.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.LegalActionAssigment.LastUpdatedTimestamp;

    entities.LegalActionAssigment.Populated = false;
    Update("CreateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
      });

    entities.LegalActionAssigment.LgaIdentifier = lgaIdentifier;
    entities.LegalActionAssigment.OspEffectiveDate = ospEffectiveDate;
    entities.LegalActionAssigment.OspRoleCode = ospRoleCode;
    entities.LegalActionAssigment.OffGeneratedId = offGeneratedId;
    entities.LegalActionAssigment.SpdGeneratedId = spdGeneratedId;
    entities.LegalActionAssigment.EffectiveDate = effectiveDate;
    entities.LegalActionAssigment.DiscontinueDate = discontinueDate;
    entities.LegalActionAssigment.ReasonCode = reasonCode;
    entities.LegalActionAssigment.OverrideInd = overrideInd;
    entities.LegalActionAssigment.CreatedBy = createdBy;
    entities.LegalActionAssigment.CreatedTimestamp = createdTimestamp;
    entities.LegalActionAssigment.LastUpdatedBy = lastUpdatedBy;
    entities.LegalActionAssigment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LegalActionAssigment.Populated = true;
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private LegalAction legalAction;
    private LegalActionAssigment legalActionAssigment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private LegalAction legalAction;
    private LegalActionAssigment legalActionAssigment;
  }
#endregion
}
