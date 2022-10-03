// Program: SP_CAB_UPDATE_LEG_REF_ASSGMT, ID: 372318286, model: 746.
// Short name: SWE01931
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_LEG_REF_ASSGMT.
/// </para>
/// <para>
/// RESP: SERVPLAN
/// This acblk updates the Monitored Activity Assignment entity
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateLegRefAssgmt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_LEG_REF_ASSGMT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateLegRefAssgmt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateLegRefAssgmt.
  /// </summary>
  public SpCabUpdateLegRefAssgmt(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------------------
    // Date	By	IDCR #	Description
    // 02/01/97 govind		Initial creation.
    // 11/03/97  Siraj		Move edit for overlapping assignment to PRAD
    //  			Remove seperate READ on Legal Referral
    // 04/26/01 GVandy	WR251	Override indicator is now the only updatable 
    // attribute.
    // -----------------------------------------------------------------------------------------------
    if (!ReadLegalReferralAssignment())
    {
      ExitState = "LEGAL_REFERRAL_ASSIGNMENT_NF";

      return;
    }

    try
    {
      UpdateLegalReferralAssignment();
      MoveLegalReferralAssignment(entities.LegalReferralAssignment,
        export.LegalReferralAssignment);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "LEGAL_REFERRAL_ASSIGNMENT_NU";

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

  private static void MoveLegalReferralAssignment(
    LegalReferralAssignment source, LegalReferralAssignment target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private bool ReadLegalReferralAssignment()
  {
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", import.LegalReferral.Identifier);
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetString(command, "ospCode", import.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(command, "spdId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offId", import.Office.SystemGeneratedId);
        db.SetDateTime(
          command, "createdTimestamp",
          import.LegalReferralAssignment.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.OverrideInd = db.GetString(reader, 0);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.LegalReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.LegalReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 6);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 7);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 8);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.Populated = true;
      });
  }

  private void UpdateLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);

    var overrideInd = import.LegalReferralAssignment.OverrideInd;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LegalReferralAssignment.Populated = false;
    Update("UpdateLegalReferralAssignment",
      (db, command) =>
      {
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.LegalReferralAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.LegalReferralAssignment.SpdId);
        db.SetInt32(command, "offId", entities.LegalReferralAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.LegalReferralAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.LegalReferralAssignment.CasNo);
        db.SetInt32(command, "lgrId", entities.LegalReferralAssignment.LgrId);
      });

    entities.LegalReferralAssignment.OverrideInd = overrideInd;
    entities.LegalReferralAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.LegalReferralAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.LegalReferralAssignment.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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

    private Case1 case1;
    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
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
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    private LegalReferralAssignment legalReferralAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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

    private Case1 case1;
    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
