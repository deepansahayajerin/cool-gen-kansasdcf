// Program: SP_CAB_UPDATE_CU_FUNC_ASSGN, ID: 372318289, model: 746.
// Short name: SWE01874
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_CU_FUNC_ASSGN.
/// </summary>
[Serializable]
public partial class SpCabUpdateCuFuncAssgn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_CU_FUNC_ASSGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateCuFuncAssgn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateCuFuncAssgn.
  /// </summary>
  public SpCabUpdateCuFuncAssgn(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadCaseUnitFunctionAssignmt())
    {
      try
      {
        UpdateCaseUnitFunctionAssignmt();
        export.CaseUnitFunctionAssignmt.
          Assign(entities.CaseUnitFunctionAssignmt);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_UNIT_FUNCTION_ASSIGNMT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASE_UNIT_FUNCTION_ASSIGNMT_PV";

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
      ExitState = "CASE_UNIT_FUNCTION_ASSIGNMT_NF";
    }
  }

  private bool ReadCaseUnitFunctionAssignmt()
  {
    entities.CaseUnitFunctionAssignmt.Populated = false;

    return Read("ReadCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.CaseUnitFunctionAssignmt.CreatedTimestamp.GetValueOrDefault());
          
        db.SetInt32(command, "csuNo", import.CaseUnit.CuNumber);
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetDate(
          command, "ospDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "ospCode", import.OfficeServiceProvider.RoleCode);
        db.SetInt32(command, "spdId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CaseUnitFunctionAssignmt.ReasonCode = db.GetString(reader, 0);
        entities.CaseUnitFunctionAssignmt.OverrideInd = db.GetString(reader, 1);
        entities.CaseUnitFunctionAssignmt.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CaseUnitFunctionAssignmt.CreatedBy = db.GetString(reader, 4);
        entities.CaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 8);
        entities.CaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 9);
        entities.CaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 10);
        entities.CaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 11);
        entities.CaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 12);
        entities.CaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 13);
        entities.CaseUnitFunctionAssignmt.Function = db.GetString(reader, 14);
        entities.CaseUnitFunctionAssignmt.Populated = true;
      });
  }

  private void UpdateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.
      Assert(entities.CaseUnitFunctionAssignmt.Populated);

    var reasonCode = import.CaseUnitFunctionAssignmt.ReasonCode;
    var overrideInd = import.CaseUnitFunctionAssignmt.OverrideInd;
    var discontinueDate = import.CaseUnitFunctionAssignmt.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt",
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
          entities.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.CaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(command, "offId", entities.CaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(command, "csuNo", entities.CaseUnitFunctionAssignmt.CsuNo);
        db.SetString(command, "casNo", entities.CaseUnitFunctionAssignmt.CasNo);
      });

    entities.CaseUnitFunctionAssignmt.ReasonCode = reasonCode;
    entities.CaseUnitFunctionAssignmt.OverrideInd = overrideInd;
    entities.CaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.CaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CaseUnitFunctionAssignmt.Populated = true;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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

    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
    }

    private CaseUnit caseUnit;
    private Case1 case1;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
    }

    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Overlapping.
    /// </summary>
    [JsonPropertyName("overlapping")]
    public CaseUnitFunctionAssignmt Overlapping
    {
      get => overlapping ??= new();
      set => overlapping = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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

    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
    }

    private CaseUnitFunctionAssignmt overlapping;
    private Case1 case1;
    private CaseUnit caseUnit;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
  }
#endregion
}
