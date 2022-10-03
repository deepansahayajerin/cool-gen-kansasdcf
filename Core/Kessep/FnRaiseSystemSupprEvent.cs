// Program: FN_RAISE_SYSTEM_SUPPR_EVENT, ID: 1625347652, model: 746.
// Short name: SWE02651
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RAISE_SYSTEM_SUPPR_EVENT.
/// </summary>
[Serializable]
public partial class FnRaiseSystemSupprEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RAISE_SYSTEM_SUPPR_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRaiseSystemSupprEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRaiseSystemSupprEvent.
  /// </summary>
  public FnRaiseSystemSupprEvent(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 07/02/19  GVandy	CQ65423		Raise event for system suppressions.  The event
    // 					will trigger an alert to the worker about the
    // 					suppression.
    // 08/20/20  GVandy	CQ66660		Modifying changes added for CQ65423 to now 
    // apply only
    // 					to cases where an AP on the case made the payment
    // 					that resulted in the suppressed disbursement.
    // --------------------------------------------------------------------------------------------------
    local.CommasRequired.Flag = "Y";

    if (!ReadDisbursementTransaction())
    {
      ExitState = "FN0000_DISB_TRANSACTION_NF";

      return;
    }

    foreach(var item in ReadCaseCaseRole())
    {
      if (!Lt(entities.CaseRole.EndDate, Now().Date))
      {
        local.ActiveRole.Flag = "Y";
      }

      if (AsChar(local.ActiveRole.Flag) == 'Y' && Lt
        (entities.CaseRole.EndDate, Now().Date))
      {
        return;
      }

      local.Reason.NarrativeText = "REASON: " + (
        import.DisbursementStatusHistory.SuppressionReason ?? "") + " " + (
          import.DisbursementStatusHistory.ReasonText ?? "");

      // --Convert the disbursement amount to a text value.
      if (import.DisbursementTransaction.Amount < 0)
      {
        local.ForConverting.Amount = -import.DisbursementTransaction.Amount;
        local.CurrencyConversion.Text1 = "-";
      }
      else
      {
        local.ForConverting.Amount = import.DisbursementTransaction.Amount;
        local.CurrencyConversion.Text1 = "";
      }

      local.CurrencyConversion.Text7 =
        NumberToString((long)local.ForConverting.Amount, 9, 7);
      local.Verify.Count = Verify(local.CurrencyConversion.Text7, "0");

      if (local.Verify.Count == 0)
      {
        local.CurrencyConversion.Text7 = "0";
      }
      else
      {
        local.CurrencyConversion.Text7 =
          Substring(local.CurrencyConversion.Text7, local.Verify.Count, 8 -
          local.Verify.Count);
      }

      local.CurrencyConversion.Text2 =
        NumberToString((long)(local.ForConverting.Amount * 100), 14, 2);
      local.CurrencyValueTextWorkArea.Text10 =
        local.CurrencyConversion.Text1 + TrimEnd
        (local.CurrencyConversion.Text7) + "." + local
        .CurrencyConversion.Text2;
      local.Amount.NarrativeText = "Amount: " + local
        .CurrencyValueTextWorkArea.Text10;
      local.Amount.NarrativeText = TrimEnd(local.Amount.NarrativeText) + "  Disb Date: " +
        NumberToString
        (Month(entities.DisbursementTransaction.DisbursementDate), 14, 2) + "-"
        + NumberToString
        (Day(entities.DisbursementTransaction.DisbursementDate), 14, 2) + "-"
        + NumberToString
        (Year(entities.DisbursementTransaction.DisbursementDate), 12, 4);
      local.Amount.NarrativeText = TrimEnd(local.Amount.NarrativeText) + " Reference #: " +
        (import.DisbursementTransaction.ReferenceNumber ?? "");
      local.CsePersonsWorkSet.Number = import.Ar.Number;
      local.Payee.NarrativeText = "Payee Number: " + local
        .CsePersonsWorkSet.Number;
      local.CsePersonsWorkSet.FormattedName = "";
      UseSiReadCsePerson();
      local.Payee.NarrativeText = TrimEnd(local.Payee.NarrativeText) + "  Name: " +
        local.CsePersonsWorkSet.FormattedName;

      // --Raise Event to create HIST record and alert.
      local.Infrastructure.EventId = 9;
      local.Infrastructure.ReasonCode = "DISBCANTPROCESS";
      local.Infrastructure.Detail =
        import.DisbursementStatusHistory.ReasonText ?? "";
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.CsePersonNumber = import.Ar.Number;
      local.Infrastructure.CaseNumber = entities.Case1.Number;

      if (!ReadCaseUnit())
      {
        continue;
      }

      local.Infrastructure.SituationNumber = 0;
      local.Infrastructure.CsenetInOutCode = "";
      local.Infrastructure.InitiatingStateCode = "KS";
      local.Infrastructure.ReferenceDate = Now().Date;
      local.Infrastructure.UserId = global.UserId;
      UseSpCabCreateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // --Create CSLN entry.
      local.NarrativeDetail.InfrastructureId =
        local.Infrastructure.SystemGeneratedIdentifier;
      local.NarrativeDetail.CaseNumber = entities.Case1.Number;
      local.NarrativeDetail.CreatedBy = global.UserId;
      local.NarrativeDetail.CreatedTimestamp =
        local.Infrastructure.CreatedTimestamp;

      for(local.NarrativeDetail.LineNumber = 1; local
        .NarrativeDetail.LineNumber <= 4; ++local.NarrativeDetail.LineNumber)
      {
        // --Set the narrative detail values
        switch(local.NarrativeDetail.LineNumber)
        {
          case 1:
            local.NarrativeDetail.NarrativeText =
              "SYSTEM SUPPRESSED DISBURSEMENT";

            break;
          case 2:
            local.NarrativeDetail.NarrativeText =
              local.Reason.NarrativeText ?? "";

            break;
          case 3:
            local.NarrativeDetail.NarrativeText =
              local.Amount.NarrativeText ?? "";

            break;
          case 4:
            local.NarrativeDetail.NarrativeText = local.Payee.NarrativeText ?? ""
              ;

            break;
          default:
            break;
        }

        UseSpCabCreateNarrativeDetail();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      if (Lt(entities.CaseRole.EndDate, Now().Date))
      {
        return;
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabCreateNarrativeDetail()
  {
    var useImport = new SpCabCreateNarrativeDetail.Import();
    var useExport = new SpCabCreateNarrativeDetail.Export();

    useImport.NarrativeDetail.Assign(local.NarrativeDetail);

    Call(SpCabCreateNarrativeDetail.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "startDate", date);
        db.SetString(command, "cspNumber", import.Ar.Number);
        db.SetInt32(
          command, "disbTranId",
          import.DisbursementTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction()
  {
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Ar.Number);
        db.SetInt32(
          command, "disbTranId",
          import.DisbursementTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
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
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson ar;
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
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public Common Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    /// <summary>
    /// A value of CurrencyConversion.
    /// </summary>
    [JsonPropertyName("currencyConversion")]
    public WorkArea CurrencyConversion
    {
      get => currencyConversion ??= new();
      set => currencyConversion = value;
    }

    /// <summary>
    /// A value of ForConverting.
    /// </summary>
    [JsonPropertyName("forConverting")]
    public DisbursementTransaction ForConverting
    {
      get => forConverting ??= new();
      set => forConverting = value;
    }

    /// <summary>
    /// A value of CommasRequired.
    /// </summary>
    [JsonPropertyName("commasRequired")]
    public Common CommasRequired
    {
      get => commasRequired ??= new();
      set => commasRequired = value;
    }

    /// <summary>
    /// A value of ActiveRole.
    /// </summary>
    [JsonPropertyName("activeRole")]
    public Common ActiveRole
    {
      get => activeRole ??= new();
      set => activeRole = value;
    }

    /// <summary>
    /// A value of Reason.
    /// </summary>
    [JsonPropertyName("reason")]
    public NarrativeDetail Reason
    {
      get => reason ??= new();
      set => reason = value;
    }

    /// <summary>
    /// A value of CurrencyValueCommon.
    /// </summary>
    [JsonPropertyName("currencyValueCommon")]
    public Common CurrencyValueCommon
    {
      get => currencyValueCommon ??= new();
      set => currencyValueCommon = value;
    }

    /// <summary>
    /// A value of CurrencyValueTextWorkArea.
    /// </summary>
    [JsonPropertyName("currencyValueTextWorkArea")]
    public TextWorkArea CurrencyValueTextWorkArea
    {
      get => currencyValueTextWorkArea ??= new();
      set => currencyValueTextWorkArea = value;
    }

    /// <summary>
    /// A value of Amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public NarrativeDetail Amount
    {
      get => amount ??= new();
      set => amount = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public NarrativeDetail Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    /// <summary>
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    private Common verify;
    private WorkArea currencyConversion;
    private DisbursementTransaction forConverting;
    private Common commasRequired;
    private Common activeRole;
    private NarrativeDetail reason;
    private Common currencyValueCommon;
    private TextWorkArea currencyValueTextWorkArea;
    private NarrativeDetail amount;
    private CsePersonsWorkSet csePersonsWorkSet;
    private NarrativeDetail payee;
    private Common common;
    private Infrastructure infrastructure;
    private NarrativeDetail narrativeDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonAccount obligee;
    private DisbursementTransaction credit;
    private DisbursementTransaction debit;
    private DisbursementTransactionRln disbursementTransactionRln;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson apCsePerson;
    private CaseRole apCaseRole;
    private CsePersonAccount csePersonAccount;
    private DisbursementTransaction disbursementTransaction;
    private CaseRole caseRole;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CsePerson csePerson;
  }
#endregion
}
