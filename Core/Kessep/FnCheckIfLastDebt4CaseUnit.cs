// Program: FN_CHECK_IF_LAST_DEBT_4CASE_UNIT, ID: 371358134, model: 746.
// Short name: SWE02025
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CHECK_IF_LAST_DEBT_4CASE_UNIT.
/// </summary>
[Serializable]
public partial class FnCheckIfLastDebt4CaseUnit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_IF_LAST_DEBT_4CASE_UNIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckIfLastDebt4CaseUnit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckIfLastDebt4CaseUnit.
  /// </summary>
  public FnCheckIfLastDebt4CaseUnit(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	  Request #	Description
    // --------  ---------	  ---------	
    // -------------------------------------------------------------------------------------------
    // 04/05/06  GVandy	  PR#261670	Initial Development
    // -----------------------------------------------------------------------------------------------------------------------------------
    if (Equal(import.ObligationType.Code, "SP") || Equal
      (import.ObligationType.Code, "SAJ"))
    {
      // -- Don't need to create events for Spousal debts.
      return;
    }

    // -- Determine if any additional debts exists for the obligor/child 
    // combination.
    foreach(var item in ReadSupportedCsePerson())
    {
      foreach(var item1 in ReadAccrualInstructions())
      {
        goto ReadEach;
      }

      foreach(var item1 in ReadDebtDetail())
      {
        goto ReadEach;
      }

      // -- There are no additional active debts for this Obligor/Child 
      // combination.
      // -- Find ALL case units the AP/Child have ever been on and raise event 7
      // /240.
      //    Event 7/240 causes a lifecycle transformation on the "Is Obligated" 
      // flag of the case state.
      //    Even though a case unit may be ended (even the entire case may be 
      // ended), the thinking is that
      //    if the case ever re-opens they want the state of the case unit to be
      // correct.
      foreach(var item1 in ReadCaseCaseUnit())
      {
        // -- Check if case is intertstate or not.
        local.Infrastructure.InitiatingStateCode = "KS";

        if (ReadInterstateRequest())
        {
          local.Infrastructure.InitiatingStateCode = "OS";
        }

        local.Infrastructure.ReasonCode = "FNALLOBGPD";
        local.Infrastructure.Detail =
          "All Obligations on this CASE are satisfied for Child " + entities
          .Supported1.Number;
        local.Infrastructure.EventId = 7;
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CaseNumber = entities.Case1.Number;
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.Infrastructure.CsePersonNumber = import.Obligor.Number;
        local.Infrastructure.UserId = "OPAY";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.ReferenceDate = import.Process.Date;
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++export.NoOfInfraRecsCreated.Count;
        }
        else if (IsExitState("SP0000_EVENT_DETAIL_NF"))
        {
          export.ErrorFound.ActionEntry = "03";
          export.ErrorFound.Flag = "Y";

          return;
        }
        else
        {
          export.ErrorFound.Flag = "Y";
          export.ErrorFound.ActionEntry = "99";

          return;
        }
      }

ReadEach:
      ;
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Supported2.Populated);
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt", import.Process.Date.GetValueOrDefault());
        db.SetNullableString(command, "cpaSupType", entities.Supported2.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported2.CspNumber);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnit()
  {
    entities.CaseUnit.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoChild", entities.Supported1.Number);
        db.SetNullableString(command, "cspNoAp", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 5);
        entities.CaseUnit.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Supported2.Populated);
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableString(command, "cpaSupType", entities.Supported2.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported2.CspNumber);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadSupportedCsePerson()
  {
    entities.Supported1.Populated = false;
    entities.Supported2.Populated = false;

    return ReadEach("ReadSupportedCsePerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetDate(
          command, "effectiveDt", import.LastRunDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Supported2.CspNumber = db.GetString(reader, 0);
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.Supported2.Type1 = db.GetString(reader, 1);
        entities.Supported1.Populated = true;
        entities.Supported2.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported2.Type1);

        return true;
      });
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of LastRunDate.
    /// </summary>
    [JsonPropertyName("lastRunDate")]
    public DateWorkArea LastRunDate
    {
      get => lastRunDate ??= new();
      set => lastRunDate = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private DateWorkArea process;
    private ObligationType obligationType;
    private CsePerson obligor;
    private DateWorkArea lastRunDate;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NoOfInfraRecsCreated.
    /// </summary>
    [JsonPropertyName("noOfInfraRecsCreated")]
    public Common NoOfInfraRecsCreated
    {
      get => noOfInfraRecsCreated ??= new();
      set => noOfInfraRecsCreated = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    private Common noOfInfraRecsCreated;
    private Common errorFound;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private DateWorkArea null1;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private CsePerson supported1;
    private CaseUnit caseUnit;
    private Case1 case1;
    private AccrualInstructions accrualInstructions;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAccount supported2;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private InterstateRequest interstateRequest;
  }
#endregion
}
