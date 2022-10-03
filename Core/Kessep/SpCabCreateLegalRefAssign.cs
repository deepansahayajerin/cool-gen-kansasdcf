// Program: SP_CAB_CREATE_LEGAL_REF_ASSIGN, ID: 372318279, model: 746.
// Short name: SWE01825
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_LEGAL_REF_ASSIGN.
/// </summary>
[Serializable]
public partial class SpCabCreateLegalRefAssign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_LEGAL_REF_ASSIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateLegalRefAssign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateLegalRefAssign.
  /// </summary>
  public SpCabCreateLegalRefAssign(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 10/24/96 Rick Delgado			Initial Development
    // 10/27/97 Siraj Konkader			Performance tuning. Replaced persistent views, 
    // moved edits to PRAD.
    // 04/26/01 GVandy		WR251		End date any existing assignments.
    // -----------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    if (!ReadLegalReferral())
    {
      ExitState = "LEGAL_REFERRAL_NF";

      return;
    }

    if (!ReadOfficeServiceProvider())
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    // 04/26/01 GVandy WR251 - End date any existing assignments.
    foreach(var item in ReadLegalReferralAssignment())
    {
      try
      {
        UpdateLegalReferralAssignment();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_REFERRAL_ASSIGNMENT_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "LEGAL_REFERRAL_ASSIGNMENT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    try
    {
      CreateLegalReferralAssignment();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_LEGAL_REFFERAL_ASSIGN_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "LEGAL_REFERRAL_ASSIGNMENT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var reasonCode = import.LegalReferralAssignment.ReasonCode;
    var overrideInd = import.LegalReferralAssignment.OverrideInd;
    var effectiveDate = import.LegalReferralAssignment.EffectiveDate;
    var discontinueDate = import.LegalReferralAssignment.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var casNo = entities.LegalReferral.CasNumber;
    var lgrId = entities.LegalReferral.Identifier;

    entities.LegalReferralAssignment.Populated = false;
    Update("CreateLegalReferralAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetString(command, "casNo", casNo);
        db.SetInt32(command, "lgrId", lgrId);
      });

    entities.LegalReferralAssignment.ReasonCode = reasonCode;
    entities.LegalReferralAssignment.OverrideInd = overrideInd;
    entities.LegalReferralAssignment.EffectiveDate = effectiveDate;
    entities.LegalReferralAssignment.DiscontinueDate = discontinueDate;
    entities.LegalReferralAssignment.CreatedBy = createdBy;
    entities.LegalReferralAssignment.CreatedTimestamp = createdTimestamp;
    entities.LegalReferralAssignment.LastUpdatedBy = "";
    entities.LegalReferralAssignment.LastUpdatedTimestamp = null;
    entities.LegalReferralAssignment.SpdId = spdId;
    entities.LegalReferralAssignment.OffId = offId;
    entities.LegalReferralAssignment.OspCode = ospCode;
    entities.LegalReferralAssignment.OspDate = ospDate;
    entities.LegalReferralAssignment.CasNo = casNo;
    entities.LegalReferralAssignment.LgrId = lgrId;
    entities.LegalReferralAssignment.Populated = true;
  }

  private bool ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralAssignment.Populated = false;

    return ReadEach("ReadLegalReferralAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.OverrideInd = db.GetString(reader, 1);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.LegalReferralAssignment.CreatedBy = db.GetString(reader, 4);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.LegalReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.LegalReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 8);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 10);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 11);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 12);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 13);
        entities.LegalReferralAssignment.Populated = true;

        return true;
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

  private void UpdateLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);

    var discontinueDate =
      AddDays(import.LegalReferralAssignment.EffectiveDate, -1);
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LegalReferralAssignment.Populated = false;
    Update("UpdateLegalReferralAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
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

    entities.LegalReferralAssignment.DiscontinueDate = discontinueDate;
    entities.LegalReferralAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.LegalReferralAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.LegalReferralAssignment.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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

    private Case1 case1;
    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private LegalReferral legalReferral;
    private Case1 case1;
    private LegalReferralAssignment legalReferralAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
  }
#endregion
}
