// Program: SP_CAB_UPDATE_ADM_APP_ASSIGNMENT, ID: 372318283, model: 746.
// Short name: SWE01901
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_ADM_APP_ASSIGNMENT.
/// </para>
/// <para>
/// RESP: SERVPLAN
/// This acblk updates the Monitored Activity Assignment entity
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateAdmAppAssignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_ADM_APP_ASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateAdmAppAssignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateAdmAppAssignment.
  /// </summary>
  public SpCabUpdateAdmAppAssignment(IContext context, Import import,
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
    // *********************************************
    // Date	By	IDCR #	Description
    // xxxxxx	??????		Initial creation.
    // 110397  Siraj		Move edit for overlapping assignment to PRAD
    //  			Replaced extended read on Admin Appeal and Admin appeal Assignment w/
    // single read.
    // *********************************************
    if (ReadAdministrativeAppealAssignment())
    {
      try
      {
        UpdateAdministrativeAppealAssignment();
        export.AdministrativeAppealAssignment.Assign(
          entities.AdministrativeAppealAssignment);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "ADMIN_APPEAL_ASSIGNMENT_NU";

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
      ExitState = "ADMIN_APPEAL_ASSIGNMENT_NF";
    }
  }

  private bool ReadAdministrativeAppealAssignment()
  {
    entities.AdministrativeAppealAssignment.Populated = false;

    return Read("ReadAdministrativeAppealAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "aapId", import.AdministrativeAppeal.Identifier);
        db.SetString(command, "ospCode", import.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(command, "spdId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offId", import.Office.SystemGeneratedId);
        db.SetDateTime(
          command, "createdTimestamp",
          import.AdministrativeAppealAssignment.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeAppealAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.AdministrativeAppealAssignment.OverrideInd =
          db.GetString(reader, 1);
        entities.AdministrativeAppealAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.AdministrativeAppealAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.AdministrativeAppealAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.AdministrativeAppealAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.AdministrativeAppealAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.AdministrativeAppealAssignment.SpdId = db.GetInt32(reader, 7);
        entities.AdministrativeAppealAssignment.OffId = db.GetInt32(reader, 8);
        entities.AdministrativeAppealAssignment.OspCode =
          db.GetString(reader, 9);
        entities.AdministrativeAppealAssignment.OspDate =
          db.GetDate(reader, 10);
        entities.AdministrativeAppealAssignment.AapId = db.GetInt32(reader, 11);
        entities.AdministrativeAppealAssignment.Populated = true;
      });
  }

  private void UpdateAdministrativeAppealAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdministrativeAppealAssignment.Populated);

    var reasonCode = import.AdministrativeAppealAssignment.ReasonCode;
    var overrideInd = import.AdministrativeAppealAssignment.OverrideInd;
    var discontinueDate = import.AdministrativeAppealAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.AdministrativeAppealAssignment.Populated = false;
    Update("UpdateAdministrativeAppealAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.AdministrativeAppealAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.AdministrativeAppealAssignment.SpdId);
        db.SetInt32(
          command, "offId", entities.AdministrativeAppealAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.AdministrativeAppealAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.AdministrativeAppealAssignment.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "aapId", entities.AdministrativeAppealAssignment.AapId);
      });

    entities.AdministrativeAppealAssignment.ReasonCode = reasonCode;
    entities.AdministrativeAppealAssignment.OverrideInd = overrideInd;
    entities.AdministrativeAppealAssignment.DiscontinueDate = discontinueDate;
    entities.AdministrativeAppealAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.AdministrativeAppealAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.AdministrativeAppealAssignment.Populated = true;
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
    /// A value of AdministrativeAppealAssignment.
    /// </summary>
    [JsonPropertyName("administrativeAppealAssignment")]
    public AdministrativeAppealAssignment AdministrativeAppealAssignment
    {
      get => administrativeAppealAssignment ??= new();
      set => administrativeAppealAssignment = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private AdministrativeAppealAssignment administrativeAppealAssignment;
    private AdministrativeAppeal administrativeAppeal;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AdministrativeAppealAssignment.
    /// </summary>
    [JsonPropertyName("administrativeAppealAssignment")]
    public AdministrativeAppealAssignment AdministrativeAppealAssignment
    {
      get => administrativeAppealAssignment ??= new();
      set => administrativeAppealAssignment = value;
    }

    private AdministrativeAppealAssignment administrativeAppealAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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

    private AdministrativeAppeal administrativeAppeal;
    private AdministrativeAppealAssignment administrativeAppealAssignment;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
