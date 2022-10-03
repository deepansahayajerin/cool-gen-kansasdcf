// Program: SP_CAB_CREATE_ADMIN_APPEAL_ASSGN, ID: 372318277, model: 746.
// Short name: SWE01875
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_ADMIN_APPEAL_ASSGN.
/// </summary>
[Serializable]
public partial class SpCabCreateAdminAppealAssgn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_ADMIN_APPEAL_ASSGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateAdminAppealAssgn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateAdminAppealAssgn.
  /// </summary>
  public SpCabCreateAdminAppealAssgn(IContext context, Import import,
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
    if (ReadAdministrativeAppeal())
    {
      if (ReadOfficeServiceProvider())
      {
        try
        {
          CreateAdministrativeAppealAssignment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_ADMIN_APPEAL_ASSGN_AE";

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
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";
    }
  }

  private void CreateAdministrativeAppealAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var reasonCode = import.AdministrativeAppealAssignment.ReasonCode;
    var overrideInd = import.AdministrativeAppealAssignment.OverrideInd;
    var effectiveDate = import.AdministrativeAppealAssignment.EffectiveDate;
    var discontinueDate = import.AdministrativeAppealAssignment.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = import.AdministrativeAppealAssignment.LastUpdatedBy ?? ""
      ;
    var lastUpdatedTimestamp =
      import.AdministrativeAppealAssignment.LastUpdatedTimestamp;
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var aapId = entities.AdministrativeAppeal.Identifier;

    entities.AdministrativeAppealAssignment.Populated = false;
    Update("CreateAdministrativeAppealAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "aapId", aapId);
      });

    entities.AdministrativeAppealAssignment.ReasonCode = reasonCode;
    entities.AdministrativeAppealAssignment.OverrideInd = overrideInd;
    entities.AdministrativeAppealAssignment.EffectiveDate = effectiveDate;
    entities.AdministrativeAppealAssignment.DiscontinueDate = discontinueDate;
    entities.AdministrativeAppealAssignment.CreatedBy = createdBy;
    entities.AdministrativeAppealAssignment.CreatedTimestamp = createdTimestamp;
    entities.AdministrativeAppealAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.AdministrativeAppealAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.AdministrativeAppealAssignment.SpdId = spdId;
    entities.AdministrativeAppealAssignment.OffId = offId;
    entities.AdministrativeAppealAssignment.OspCode = ospCode;
    entities.AdministrativeAppealAssignment.OspDate = ospDate;
    entities.AdministrativeAppealAssignment.AapId = aapId;
    entities.AdministrativeAppealAssignment.Populated = true;
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", import.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Populated = true;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of AdministrativeAppealAssignment.
    /// </summary>
    [JsonPropertyName("administrativeAppealAssignment")]
    public AdministrativeAppealAssignment AdministrativeAppealAssignment
    {
      get => administrativeAppealAssignment ??= new();
      set => administrativeAppealAssignment = value;
    }

    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private AdministrativeAppeal administrativeAppeal;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of AdministrativeAppealAssignment.
    /// </summary>
    [JsonPropertyName("administrativeAppealAssignment")]
    public AdministrativeAppealAssignment AdministrativeAppealAssignment
    {
      get => administrativeAppealAssignment ??= new();
      set => administrativeAppealAssignment = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private AdministrativeAppeal administrativeAppeal;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
  }
#endregion
}
