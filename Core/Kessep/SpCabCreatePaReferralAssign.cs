// Program: SP_CAB_CREATE_PA_REFERRAL_ASSIGN, ID: 372318280, model: 746.
// Short name: SWE01827
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_PA_REFERRAL_ASSIGN.
/// </summary>
[Serializable]
public partial class SpCabCreatePaReferralAssign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_PA_REFERRAL_ASSIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreatePaReferralAssign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreatePaReferralAssign.
  /// </summary>
  public SpCabCreatePaReferralAssign(IContext context, Import import,
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
    if (ReadOfficeServiceProvider())
    {
      if (ReadPaReferral())
      {
        try
        {
          CreatePaReferralAssignment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_PA_REFERRAL_ASSIGN_AE";

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
        ExitState = "PA_REFERRAL_NF";
      }
    }
    else
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";
    }
  }

  private void CreatePaReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var reasonCode = import.PaReferralAssignment.ReasonCode;
    var overrideInd = import.PaReferralAssignment.OverrideInd;
    var effectiveDate = import.PaReferralAssignment.EffectiveDate;
    var discontinueDate = import.PaReferralAssignment.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = import.PaReferralAssignment.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.PaReferralAssignment.LastUpdatedTimestamp;
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var pafNo = entities.PaReferral.Number;
    var pafType = entities.PaReferral.Type1;
    var pafTstamp = entities.PaReferral.CreatedTimestamp;

    entities.PaReferralAssignment.Populated = false;
    Update("CreatePaReferralAssignment",
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
        db.SetString(command, "pafNo", pafNo);
        db.SetString(command, "pafType", pafType);
        db.SetDateTime(command, "pafTstamp", pafTstamp);
      });

    entities.PaReferralAssignment.ReasonCode = reasonCode;
    entities.PaReferralAssignment.OverrideInd = overrideInd;
    entities.PaReferralAssignment.EffectiveDate = effectiveDate;
    entities.PaReferralAssignment.DiscontinueDate = discontinueDate;
    entities.PaReferralAssignment.CreatedBy = createdBy;
    entities.PaReferralAssignment.CreatedTimestamp = createdTimestamp;
    entities.PaReferralAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.PaReferralAssignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.PaReferralAssignment.SpdId = spdId;
    entities.PaReferralAssignment.OffId = offId;
    entities.PaReferralAssignment.OspCode = ospCode;
    entities.PaReferralAssignment.OspDate = ospDate;
    entities.PaReferralAssignment.PafNo = pafNo;
    entities.PaReferralAssignment.PafType = pafType;
    entities.PaReferralAssignment.PafTstamp = pafTstamp;
    entities.PaReferralAssignment.Populated = true;
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

  private bool ReadPaReferral()
  {
    entities.PaReferral.Populated = false;

    return Read("ReadPaReferral",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTstamp",
          import.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "numb", import.PaReferral.Number);
        db.SetString(command, "type", import.PaReferral.Type1);
      },
      (db, reader) =>
      {
        entities.PaReferral.Number = db.GetString(reader, 0);
        entities.PaReferral.Type1 = db.GetString(reader, 1);
        entities.PaReferral.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PaReferral.Populated = true;
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
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

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

    private PaReferral paReferral;
    private PaReferralAssignment paReferralAssignment;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
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
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

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

    private PaReferral paReferral;
    private PaReferralAssignment paReferralAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
  }
#endregion
}
