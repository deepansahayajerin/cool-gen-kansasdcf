// Program: SP_CAB_UPDATE_PA_REF_ASSGMT, ID: 372318287, model: 746.
// Short name: SWE01905
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_PA_REF_ASSGMT.
/// </para>
/// <para>
/// RESP: SERVPLAN
/// This acblk updates the Monitored Activity Assignment entity
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdatePaRefAssgmt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_PA_REF_ASSGMT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdatePaRefAssgmt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdatePaRefAssgmt.
  /// </summary>
  public SpCabUpdatePaRefAssgmt(IContext context, Import import, Export export):
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
    //  			Remove seperate READ on PA Referral
    // *********************************************
    if (ReadPaReferralAssignment())
    {
      try
      {
        UpdatePaReferralAssignment();
        MovePaReferralAssignment(entities.PaReferralAssignment,
          export.PaReferralAssignment);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PA_REFERRAL_ASSIGNMENT_NU";

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
      ExitState = "PA_REFERRAL_ASSIGNMENT_NF";
    }
  }

  private static void MovePaReferralAssignment(PaReferralAssignment source,
    PaReferralAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private bool ReadPaReferralAssignment()
  {
    entities.PaReferralAssignment.Populated = false;

    return Read("ReadPaReferralAssignment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "pafNo", import.PaReferral.Number);
        db.SetString(command, "pafType", import.PaReferral.Type1);
        db.SetString(command, "ospCode", import.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(command, "spdId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offId", import.Office.SystemGeneratedId);
        db.SetDateTime(
          command, "createdTimestamp",
          import.PaReferralAssignment.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.PaReferralAssignment.OverrideInd = db.GetString(reader, 1);
        entities.PaReferralAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.PaReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PaReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.PaReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.PaReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.PaReferralAssignment.SpdId = db.GetInt32(reader, 7);
        entities.PaReferralAssignment.OffId = db.GetInt32(reader, 8);
        entities.PaReferralAssignment.OspCode = db.GetString(reader, 9);
        entities.PaReferralAssignment.OspDate = db.GetDate(reader, 10);
        entities.PaReferralAssignment.PafNo = db.GetString(reader, 11);
        entities.PaReferralAssignment.PafType = db.GetString(reader, 12);
        entities.PaReferralAssignment.PafTstamp = db.GetDateTime(reader, 13);
        entities.PaReferralAssignment.Populated = true;
      });
  }

  private void UpdatePaReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.PaReferralAssignment.Populated);

    var reasonCode = import.PaReferralAssignment.ReasonCode;
    var overrideInd = import.PaReferralAssignment.OverrideInd;
    var discontinueDate = import.PaReferralAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.PaReferralAssignment.Populated = false;
    Update("UpdatePaReferralAssignment",
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
          entities.PaReferralAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.PaReferralAssignment.SpdId);
        db.SetInt32(command, "offId", entities.PaReferralAssignment.OffId);
        db.SetString(command, "ospCode", entities.PaReferralAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.PaReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "pafNo", entities.PaReferralAssignment.PafNo);
        db.SetString(command, "pafType", entities.PaReferralAssignment.PafType);
        db.SetDateTime(
          command, "pafTstamp",
          entities.PaReferralAssignment.PafTstamp.GetValueOrDefault());
      });

    entities.PaReferralAssignment.ReasonCode = reasonCode;
    entities.PaReferralAssignment.OverrideInd = overrideInd;
    entities.PaReferralAssignment.DiscontinueDate = discontinueDate;
    entities.PaReferralAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.PaReferralAssignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.PaReferralAssignment.Populated = true;
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
    /// A value of PaReferralAssignment.
    /// </summary>
    [JsonPropertyName("paReferralAssignment")]
    public PaReferralAssignment PaReferralAssignment
    {
      get => paReferralAssignment ??= new();
      set => paReferralAssignment = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
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

    private PaReferralAssignment paReferralAssignment;
    private PaReferral paReferral;
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
    /// A value of PaReferralAssignment.
    /// </summary>
    [JsonPropertyName("paReferralAssignment")]
    public PaReferralAssignment PaReferralAssignment
    {
      get => paReferralAssignment ??= new();
      set => paReferralAssignment = value;
    }

    private PaReferralAssignment paReferralAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaReferralAssignment.
    /// </summary>
    [JsonPropertyName("paReferralAssignment")]
    public PaReferralAssignment PaReferralAssignment
    {
      get => paReferralAssignment ??= new();
      set => paReferralAssignment = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
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

    private PaReferralAssignment paReferralAssignment;
    private PaReferral paReferral;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
