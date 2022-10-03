// Program: SP_CAB_REASSIGN_CASE_UNIT_BATCH, ID: 372572054, model: 746.
// Short name: SWE00022
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_REASSIGN_CASE_UNIT_BATCH.
/// </summary>
[Serializable]
public partial class SpCabReassignCaseUnitBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_REASSIGN_CASE_UNIT_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReassignCaseUnitBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReassignCaseUnitBatch.
  /// </summary>
  public SpCabReassignCaseUnitBatch(IContext context, Import import,
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
    // ************************    M A I N T E N A N C E    L O G    
    // ************************
    //   Date    Developer   PR#/WR#   Description
    //   ----    ---------   -------   -----------
    // 08/29/00  SWSRCHF     00100345  Check for discontinue date equal MAX date
    // (2099-12-31)
    // **************************************************************************************
    if (!import.Case1.Populated)
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!import.CurrentServiceProvider.Populated)
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (!import.New1.Populated)
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    export.Create.Count = import.Create.Count;
    export.Update.Count = import.Update.Count;

    // *** Problem report 00100345
    // *** 08/29/00 SWSRCHF
    // *** check for Discontinue_Date = '2099-12-31'
    foreach(var item in ReadCaseUnitCaseUnitFunctionAssignmt())
    {
      if (AsChar(entities.CaseUnitFunctionAssignmt.OverrideInd) == 'Y')
      {
        continue;
      }

      try
      {
        UpdateCaseUnitFunctionAssignmt();
        ++export.Update.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      try
      {
        CreateCaseUnitFunctionAssignmt();
        ++export.Create.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_CASE_UNIT_FUNC_ASSGN_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void CreateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(import.New1.Populated);
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var reasonCode = entities.CaseUnitFunctionAssignmt.ReasonCode;
    var overrideInd = "N";
    var effectiveDate = import.CurrentDatePlus1.Date;
    var discontinueDate = import.Max.Date;
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTimestamp = Now();
    var spdId = import.New1.SpdGeneratedId;
    var offId = import.New1.OffGeneratedId;
    var ospCode = import.New1.RoleCode;
    var ospDate = import.New1.EffectiveDate;
    var csuNo = entities.CaseUnit.CuNumber;
    var casNo = entities.CaseUnit.CasNo;
    var function = entities.CaseUnitFunctionAssignmt.Function;

    entities.New1.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt",
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
        db.SetInt32(command, "csuNo", csuNo);
        db.SetString(command, "casNo", casNo);
        db.SetString(command, "function", function);
      });

    entities.New1.ReasonCode = reasonCode;
    entities.New1.OverrideInd = overrideInd;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTimestamp = null;
    entities.New1.SpdId = spdId;
    entities.New1.OffId = offId;
    entities.New1.OspCode = ospCode;
    entities.New1.OspDate = ospDate;
    entities.New1.CsuNo = csuNo;
    entities.New1.CasNo = casNo;
    entities.New1.Function = function;
    entities.New1.Populated = true;
  }

  private IEnumerable<bool> ReadCaseUnitCaseUnitFunctionAssignmt()
  {
    entities.CaseUnitFunctionAssignmt.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnitCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetInt32(
          command, "spdId", import.CurrentServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "startDate",
          import.CurrentDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CasNo = db.GetString(reader, 5);
        entities.CaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 5);
        entities.CaseUnitFunctionAssignmt.ReasonCode = db.GetString(reader, 6);
        entities.CaseUnitFunctionAssignmt.OverrideInd = db.GetString(reader, 7);
        entities.CaseUnitFunctionAssignmt.EffectiveDate = db.GetDate(reader, 8);
        entities.CaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.CaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.CaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 13);
        entities.CaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 14);
        entities.CaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 15);
        entities.CaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 16);
        entities.CaseUnitFunctionAssignmt.Function = db.GetString(reader, 17);
        entities.CaseUnitFunctionAssignmt.Populated = true;
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private void UpdateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.
      Assert(entities.CaseUnitFunctionAssignmt.Populated);

    var discontinueDate = import.CurrentDateWorkArea.Date;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt",
      (db, command) =>
      {
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
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public Common Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CurrentServiceProvider.
    /// </summary>
    [JsonPropertyName("currentServiceProvider")]
    public ServiceProvider CurrentServiceProvider
    {
      get => currentServiceProvider ??= new();
      set => currentServiceProvider = value;
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

    private Common create;
    private Common update;
    private DateWorkArea currentDateWorkArea;
    private DateWorkArea currentDatePlus1;
    private DateWorkArea max;
    private ProgramProcessingInfo programProcessingInfo;
    private Case1 case1;
    private ServiceProvider currentServiceProvider;
    private OfficeServiceProvider new1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public Common Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    private Common create;
    private Common update;
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

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CaseUnitFunctionAssignmt New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private Case1 case1;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private OfficeServiceProvider officeServiceProvider;
    private CaseUnitFunctionAssignmt new1;
    private CaseUnit caseUnit;
  }
#endregion
}
