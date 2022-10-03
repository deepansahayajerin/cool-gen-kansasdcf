// Program: SP_CAB_CREATE_CU_FUNCTION_ASSIGN, ID: 372318281, model: 746.
// Short name: SWE01821
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_CU_FUNCTION_ASSIGN.
/// </summary>
[Serializable]
public partial class SpCabCreateCuFunctionAssign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_CU_FUNCTION_ASSIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateCuFunctionAssign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateCuFunctionAssign.
  /// </summary>
  public SpCabCreateCuFunctionAssign(IContext context, Import import,
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
    if (ReadOfficeServiceProvider())
    {
      if (ReadCaseUnit())
      {
        try
        {
          CreateCaseUnitFunctionAssignmt();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_AE";

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
        ExitState = "CASE_UNIT_NF";
      }
    }
    else
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";
    }
  }

  private void CreateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);

    var reasonCode = import.CaseUnitFunctionAssignmt.ReasonCode;
    var overrideInd = import.CaseUnitFunctionAssignmt.OverrideInd;
    var effectiveDate = import.CaseUnitFunctionAssignmt.EffectiveDate;
    var discontinueDate = import.CaseUnitFunctionAssignmt.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedBy = import.CaseUnitFunctionAssignmt.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp =
      import.CaseUnitFunctionAssignmt.LastUpdatedTimestamp;
    var spdId = entities.Existing.SpdGeneratedId;
    var offId = entities.Existing.OffGeneratedId;
    var ospCode = entities.Existing.RoleCode;
    var ospDate = entities.Existing.EffectiveDate;
    var csuNo = entities.CaseUnit.CuNumber;
    var casNo = entities.CaseUnit.CasNo;
    var function = import.CaseUnitFunctionAssignmt.Function;

    entities.CaseUnitFunctionAssignmt.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt",
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
        db.SetInt32(command, "csuNo", csuNo);
        db.SetString(command, "casNo", casNo);
        db.SetString(command, "function", function);
      });

    entities.CaseUnitFunctionAssignmt.ReasonCode = reasonCode;
    entities.CaseUnitFunctionAssignmt.OverrideInd = overrideInd;
    entities.CaseUnitFunctionAssignmt.EffectiveDate = effectiveDate;
    entities.CaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.CaseUnitFunctionAssignmt.CreatedBy = createdBy;
    entities.CaseUnitFunctionAssignmt.CreatedTimestamp = createdTimestamp;
    entities.CaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CaseUnitFunctionAssignmt.SpdId = spdId;
    entities.CaseUnitFunctionAssignmt.OffId = offId;
    entities.CaseUnitFunctionAssignmt.OspCode = ospCode;
    entities.CaseUnitFunctionAssignmt.OspDate = ospDate;
    entities.CaseUnitFunctionAssignmt.CsuNo = csuNo;
    entities.CaseUnitFunctionAssignmt.CasNo = casNo;
    entities.CaseUnitFunctionAssignmt.Function = function;
    entities.CaseUnitFunctionAssignmt.Populated = true;
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetInt32(command, "cuNumber", import.CaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.Existing.Populated = false;

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
        entities.Existing.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.Existing.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Existing.RoleCode = db.GetString(reader, 2);
        entities.Existing.EffectiveDate = db.GetDate(reader, 3);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Existing.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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

    private Case1 case1;
    private ServiceProvider serviceProvider;
    private Office office;
    private CaseUnit caseUnit;
    private OfficeServiceProvider officeServiceProvider;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public OfficeServiceProvider Existing
    {
      get => existing ??= new();
      set => existing = value;
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

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private CaseUnit caseUnit;
    private Case1 case1;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider existing;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
