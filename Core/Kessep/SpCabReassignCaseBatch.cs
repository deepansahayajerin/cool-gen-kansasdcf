// Program: SP_CAB_REASSIGN_CASE_BATCH, ID: 372572055, model: 746.
// Short name: SWE00021
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_REASSIGN_CASE_BATCH.
/// </summary>
[Serializable]
public partial class SpCabReassignCaseBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_REASSIGN_CASE_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReassignCaseBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReassignCaseBatch.
  /// </summary>
  public SpCabReassignCaseBatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!import.Office.Populated)
    {
      ExitState = "OFFICE_ADDRESS_NF";

      return;
    }

    if (!import.Persistent.Populated)
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!import.Current.Populated)
    {
      ExitState = "CASE_ASSIGNMENT_NF";

      return;
    }

    if (!import.New1.Populated)
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    // **** End Date current case assignment ****
    try
    {
      UpdateCaseAssignment();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CASE_ASSIGNMENT_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CASE_ASSIGNMENT_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // **** Assign new SP ****
    try
    {
      CreateCaseAssignment();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_CASE_ASSIGNMENT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CASE_ASSIGNMENT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(import.New1.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = import.CurrentDatePlus1.Date;
    var discontinueDate = import.Max.Date;
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTimestamp = Now();
    var spdId = import.New1.SpdGeneratedId;
    var offId = import.New1.OffGeneratedId;
    var ospCode = import.New1.RoleCode;
    var ospDate = import.New1.EffectiveDate;
    var casNo = import.Persistent.Number;

    entities.CaseAssignment.Populated = false;
    Update("CreateCaseAssignment",
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
      });

    entities.CaseAssignment.ReasonCode = reasonCode;
    entities.CaseAssignment.OverrideInd = overrideInd;
    entities.CaseAssignment.EffectiveDate = effectiveDate;
    entities.CaseAssignment.DiscontinueDate = discontinueDate;
    entities.CaseAssignment.CreatedBy = createdBy;
    entities.CaseAssignment.CreatedTimestamp = createdTimestamp;
    entities.CaseAssignment.LastUpdatedBy = "";
    entities.CaseAssignment.LastUpdatedTimestamp = null;
    entities.CaseAssignment.SpdId = spdId;
    entities.CaseAssignment.OffId = offId;
    entities.CaseAssignment.OspCode = ospCode;
    entities.CaseAssignment.OspDate = ospDate;
    entities.CaseAssignment.CasNo = casNo;
    entities.CaseAssignment.Populated = true;
  }

  private void UpdateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(import.Current.Populated);

    var discontinueDate = import.CurrentDate.Date;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();

    import.Current.Populated = false;
    Update("UpdateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          import.Current.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", import.Current.SpdId);
        db.SetInt32(command, "offId", import.Current.OffId);
        db.SetString(command, "ospCode", import.Current.OspCode);
        db.SetDate(
          command, "ospDate", import.Current.OspDate.GetValueOrDefault());
        db.SetString(command, "casNo", import.Current.CasNo);
      });

    import.Current.DiscontinueDate = discontinueDate;
    import.Current.LastUpdatedBy = lastUpdatedBy;
    import.Current.LastUpdatedTimestamp = lastUpdatedTimestamp;
    import.Current.Populated = true;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of CurrentDatePlus1.
    /// </summary>
    [JsonPropertyName("currentDatePlus1")]
    public DateWorkArea CurrentDatePlus1
    {
      get => currentDatePlus1 ??= new();
      set => currentDatePlus1 = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public OfficeServiceProvider New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public CaseAssignment Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public Case1 Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    private DateWorkArea max;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea currentDatePlus1;
    private DateWorkArea currentDate;
    private Office office;
    private OfficeServiceProvider new1;
    private CaseAssignment current;
    private Case1 persistent;
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

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
  }
#endregion
}
