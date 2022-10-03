// Program: FN_COMPUTE_SUMMARY_TOTALS, ID: 372289058, model: 746.
// Short name: SWE02321
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_COMPUTE_SUMMARY_TOTALS.
/// </summary>
[Serializable]
public partial class FnComputeSummaryTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COMPUTE_SUMMARY_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnComputeSummaryTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnComputeSummaryTotals.
  /// </summary>
  public FnComputeSummaryTotals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -- 09/23/2010 GVandy CQ22148  Emergency fix to correct inefficient index 
    // path chosen after DB2 V9 upgrade.
    //    See note in code for additional details.
    if (IsEmpty(import.Obligor.Number))
    {
      ExitState = "FN0000_CSE_PERSON_NOT_PASSED";

      return;
    }

    UseFnHardcodedCashReceipting();
    UseFnHardcodedDebtDistribution();
    local.Process.Date = Now().Date;
    local.MaxDiscontinue.Date = UseCabSetMaximumDiscontinueDate();
    UseCabFirstAndLastDateOfMonth();

    if (import.Filter.SystemGeneratedIdentifier == 0)
    {
      local.LowLimit.SystemGeneratedIdentifier = 0;
      local.HighLimit.SystemGeneratedIdentifier = 999;
      local.OmitSecondaryObligInd.Flag = "Y";
    }
    else
    {
      local.LowLimit.SystemGeneratedIdentifier =
        import.Filter.SystemGeneratedIdentifier;
      local.HighLimit.SystemGeneratedIdentifier =
        import.Filter.SystemGeneratedIdentifier;
      local.OmitSecondaryObligInd.Flag = "N";
    }

    foreach(var item in ReadObligationObligationTypeDebtDetail())
    {
      if (AsChar(local.OmitSecondaryObligInd.Flag) == 'Y')
      {
        if (AsChar(entities.Obligation.PrimarySecondaryCode) == AsChar
          (local.HardcodedSecondary.PrimarySecondaryCode))
        {
          continue;
        }
      }

      if (import.FilterByIdObligationType.SystemGeneratedIdentifier != 0)
      {
        if (import.FilterByIdObligationType.SystemGeneratedIdentifier != entities
          .KeyOnlyObligationType.SystemGeneratedIdentifier)
        {
          continue;
        }
      }

      if (import.FilterByIdLegalAction.Identifier != 0)
      {
        if (!ReadLegalAction1())
        {
          continue;
        }
      }
      else if (!IsEmpty(import.FilterByStdNo.StandardNumber))
      {
        if (!ReadLegalAction2())
        {
          continue;
        }
      }

      if (entities.KeyOnlyObligationType.SystemGeneratedIdentifier != local
        .Hold.SystemGeneratedIdentifier)
      {
        local.Hold.SystemGeneratedIdentifier =
          entities.KeyOnlyObligationType.SystemGeneratedIdentifier;

        if (!ReadObligationType())
        {
          ExitState = "FN0000_OBLIG_TYPE_NF_RB";

          return;
        }
      }

      if (!IsEmpty(import.FilterByClass.Classification))
      {
        if (AsChar(import.FilterByClass.Classification) != AsChar
          (entities.ObligationType.Classification))
        {
          continue;
        }
      }

      if (AsChar(entities.ObligationType.Classification) == AsChar
        (local.HardcodedAccruingClass.Classification))
      {
        if (!Lt(entities.DebtDetail.DueDt, local.ProcessMonthBegin.Date))
        {
          export.ScreenOwedAmounts.CurrentAmountOwed += entities.DebtDetail.
            BalanceDueAmt;
        }
        else
        {
          export.ScreenOwedAmounts.ArrearsAmountOwed += entities.DebtDetail.
            BalanceDueAmt;
        }
      }
      else
      {
        export.ScreenOwedAmounts.ArrearsAmountOwed += entities.DebtDetail.
          BalanceDueAmt;
      }

      export.ScreenOwedAmounts.InterestAmountOwed += entities.DebtDetail.
        InterestBalanceDueAmt.GetValueOrDefault();
    }

    export.ScreenOwedAmounts.TotalAmountOwed =
      export.ScreenOwedAmounts.ArrearsAmountOwed + export
      .ScreenOwedAmounts.CurrentAmountOwed + export
      .ScreenOwedAmounts.InterestAmountOwed;

    if (AsChar(import.OmitUndistAmtInd.Flag) != 'Y')
    {
      // ----------------------------------------------------------------------------------------------------
      //  09/23/2010 GVandy CQ22148  Part of Emergency fix to correct 
      // inefficient index path chosen after DB2 V9 upgrade.
      //  The following change will alter the index from CKI04107 to CKI08107. 
      // This does not appear to be due to the
      //  upgrade but will be more efficient using CKI08107.
      //    Modifying
      //      "desired cash_receipt_detail collection_amt_fully_applied_ind is 
      // equal to spaces"
      //    to
      //      "desired cash_receipt_detail collection_amt_fully_applied_ind is 
      // greater or equal to spaces AND
      //       desired cash_receipt_detail collection_amt_fully_applied_ind is 
      // less or equal to spaces"
      // ----------------------------------------------------------------------------------------------------
      foreach(var item in ReadCashReceiptDetailCashReceiptDetailStatus2())
      {
        if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .HardcodedAdjusted.SystemGeneratedIdentifier)
        {
          continue;
        }

        export.UndistAmt.TotalCurrency += entities.CashReceiptDetail.
          CollectionAmount - entities
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault() - entities
          .CashReceiptDetail.DistributedAmount.GetValueOrDefault();

        foreach(var item1 in ReadCashReceiptDetailBalanceAdjCashReceiptDetail())
        {
          export.UndistAmt.TotalCurrency -= entities.Adjustment.
            CollectionAmount;
        }
      }

      local.CsePersonsWorkSet.Number = import.Obligor.Number;
      UseSiReadCsePerson();

      // ----------------------------------------------------------------------------------------------------
      //  09/23/2010 GVandy CQ22148  Emergency fix to correct inefficient index 
      // path chosen after DB2 V9 upgrade.
      //  The following change will alter the index selected from CKI04107 back 
      // to the original CKI05107.
      //    Modifying
      //      "desired cash_receipt_detail collection_amt_fully_applied_ind is 
      // equal to spaces"
      //    to
      //      "desired cash_receipt_detail collection_amt_fully_applied_ind is 
      // greater or equal to spaces AND
      //       desired cash_receipt_detail collection_amt_fully_applied_ind is 
      // less or equal to spaces"
      // ----------------------------------------------------------------------------------------------------
      foreach(var item in ReadCashReceiptDetailCashReceiptDetailStatus1())
      {
        if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .HardcodedAdjusted.SystemGeneratedIdentifier)
        {
          continue;
        }

        export.UndistAmt.TotalCurrency += entities.CashReceiptDetail.
          CollectionAmount - entities
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault() - entities
          .CashReceiptDetail.DistributedAmount.GetValueOrDefault();

        foreach(var item1 in ReadCashReceiptDetailBalanceAdjCashReceiptDetail())
        {
          export.UndistAmt.TotalCurrency -= entities.Adjustment.
            CollectionAmount;
        }
      }
    }

    if (AsChar(import.OmitUnprocTrnCheckInd.Flag) != 'Y')
    {
      UseFnCheckForUnprocessedTrans();
    }
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Process.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.ProcessMonthBegin.Date = useExport.First.Date;
    local.ProcessMonthEnd.Date = useExport.Last.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCheckForUnprocessedTrans()
  {
    var useImport = new FnCheckForUnprocessedTrans.Import();
    var useExport = new FnCheckForUnprocessedTrans.Export();

    useImport.Obligor.Number = import.Obligor.Number;
    useImport.FilterObligation.SystemGeneratedIdentifier =
      import.Filter.SystemGeneratedIdentifier;
    useImport.FilterObligationType.SystemGeneratedIdentifier =
      import.FilterByIdObligationType.SystemGeneratedIdentifier;
    useImport.OmitCrdInd.Flag = import.OmitCrdInd.Flag;

    Call(FnCheckForUnprocessedTrans.Execute, useImport, useExport);

    export.ScreenOwedAmounts.ErrorInformationLine =
      useExport.ScreenOwedAmounts.ErrorInformationLine;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedAccruingClass.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodedSecondary.PrimarySecondaryCode =
      useExport.ObligSecondaryConcurrent.PrimarySecondaryCode;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCashReceiptDetailBalanceAdjCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Adjustment.Populated = false;
    entities.CashReceiptDetailBalanceAdj.Populated = false;

    return ReadEach("ReadCashReceiptDetailBalanceAdjCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 4);
        entities.Adjustment.SequentialIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.Adjustment.CrvIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.Adjustment.CstIdentifier = db.GetInt32(reader, 6);
        entities.CashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.Adjustment.CrtIdentifier = db.GetInt32(reader, 7);
        entities.CashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CashReceiptDetailBalanceAdj.Description =
          db.GetNullableString(reader, 10);
        entities.Adjustment.AdjustmentInd = db.GetNullableString(reader, 11);
        entities.Adjustment.CollectionAmount = db.GetDecimal(reader, 12);
        entities.Adjustment.Populated = true;
        entities.CashReceiptDetailBalanceAdj.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailStatus1()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailStatus1",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorSsn", local.CsePersonsWorkSet.Ssn);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaxDiscontinue.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.MultiPayor = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailStatus2()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailStatus2",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.Obligor.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaxDiscontinue.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.MultiPayor = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId1",
          entities.Obligation.LgaId.GetValueOrDefault());
        db.SetInt32(
          command, "legalActionId2", import.FilterByIdLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.FilterByStdNo.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationTypeDebtDetail()
  {
    entities.KeyOnlyObligationType.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadObligationObligationTypeDebtDetail",
      (db, command) =>
      {
        db.SetDate(
          command, "dueDt", local.ProcessMonthEnd.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.LowLimit.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HighLimit.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.KeyOnlyObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 6);
        entities.DebtDetail.CspNumber = db.GetString(reader, 7);
        entities.DebtDetail.CpaType = db.GetString(reader, 8);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 10);
        entities.DebtDetail.OtrType = db.GetString(reader, 11);
        entities.DebtDetail.DueDt = db.GetDate(reader, 12);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 13);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 14);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 15);
        entities.KeyOnlyObligationType.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Obligation Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of FilterByIdObligationType.
    /// </summary>
    [JsonPropertyName("filterByIdObligationType")]
    public ObligationType FilterByIdObligationType
    {
      get => filterByIdObligationType ??= new();
      set => filterByIdObligationType = value;
    }

    /// <summary>
    /// A value of FilterByClass.
    /// </summary>
    [JsonPropertyName("filterByClass")]
    public ObligationType FilterByClass
    {
      get => filterByClass ??= new();
      set => filterByClass = value;
    }

    /// <summary>
    /// A value of FilterByIdLegalAction.
    /// </summary>
    [JsonPropertyName("filterByIdLegalAction")]
    public LegalAction FilterByIdLegalAction
    {
      get => filterByIdLegalAction ??= new();
      set => filterByIdLegalAction = value;
    }

    /// <summary>
    /// A value of FilterByStdNo.
    /// </summary>
    [JsonPropertyName("filterByStdNo")]
    public LegalAction FilterByStdNo
    {
      get => filterByStdNo ??= new();
      set => filterByStdNo = value;
    }

    /// <summary>
    /// A value of OmitCrdInd.
    /// </summary>
    [JsonPropertyName("omitCrdInd")]
    public Common OmitCrdInd
    {
      get => omitCrdInd ??= new();
      set => omitCrdInd = value;
    }

    /// <summary>
    /// A value of OmitUndistAmtInd.
    /// </summary>
    [JsonPropertyName("omitUndistAmtInd")]
    public Common OmitUndistAmtInd
    {
      get => omitUndistAmtInd ??= new();
      set => omitUndistAmtInd = value;
    }

    /// <summary>
    /// A value of OmitUnprocTrnCheckInd.
    /// </summary>
    [JsonPropertyName("omitUnprocTrnCheckInd")]
    public Common OmitUnprocTrnCheckInd
    {
      get => omitUnprocTrnCheckInd ??= new();
      set => omitUnprocTrnCheckInd = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public DateWorkArea DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private CsePerson obligor;
    private Obligation filter;
    private ObligationType filterByIdObligationType;
    private ObligationType filterByClass;
    private LegalAction filterByIdLegalAction;
    private LegalAction filterByStdNo;
    private Common omitCrdInd;
    private Common omitUndistAmtInd;
    private Common omitUnprocTrnCheckInd;
    private DateWorkArea delMe;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of UndistAmt.
    /// </summary>
    [JsonPropertyName("undistAmt")]
    public Common UndistAmt
    {
      get => undistAmt ??= new();
      set => undistAmt = value;
    }

    private ScreenOwedAmounts screenOwedAmounts;
    private Common undistAmt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public ObligationType Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

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
    /// A value of ProcessMonthBegin.
    /// </summary>
    [JsonPropertyName("processMonthBegin")]
    public DateWorkArea ProcessMonthBegin
    {
      get => processMonthBegin ??= new();
      set => processMonthBegin = value;
    }

    /// <summary>
    /// A value of ProcessMonthEnd.
    /// </summary>
    [JsonPropertyName("processMonthEnd")]
    public DateWorkArea ProcessMonthEnd
    {
      get => processMonthEnd ??= new();
      set => processMonthEnd = value;
    }

    /// <summary>
    /// A value of LowLimit.
    /// </summary>
    [JsonPropertyName("lowLimit")]
    public Obligation LowLimit
    {
      get => lowLimit ??= new();
      set => lowLimit = value;
    }

    /// <summary>
    /// A value of HighLimit.
    /// </summary>
    [JsonPropertyName("highLimit")]
    public Obligation HighLimit
    {
      get => highLimit ??= new();
      set => highLimit = value;
    }

    /// <summary>
    /// A value of MaxDiscontinue.
    /// </summary>
    [JsonPropertyName("maxDiscontinue")]
    public DateWorkArea MaxDiscontinue
    {
      get => maxDiscontinue ??= new();
      set => maxDiscontinue = value;
    }

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
    /// A value of OmitSecondaryObligInd.
    /// </summary>
    [JsonPropertyName("omitSecondaryObligInd")]
    public Common OmitSecondaryObligInd
    {
      get => omitSecondaryObligInd ??= new();
      set => omitSecondaryObligInd = value;
    }

    /// <summary>
    /// A value of HardcodedAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodedAccruingClass")]
    public ObligationType HardcodedAccruingClass
    {
      get => hardcodedAccruingClass ??= new();
      set => hardcodedAccruingClass = value;
    }

    /// <summary>
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
    }

    /// <summary>
    /// A value of HardcodedAdjusted.
    /// </summary>
    [JsonPropertyName("hardcodedAdjusted")]
    public CashReceiptDetailStatus HardcodedAdjusted
    {
      get => hardcodedAdjusted ??= new();
      set => hardcodedAdjusted = value;
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

    private ObligationType hold;
    private DateWorkArea process;
    private DateWorkArea processMonthBegin;
    private DateWorkArea processMonthEnd;
    private Obligation lowLimit;
    private Obligation highLimit;
    private DateWorkArea maxDiscontinue;
    private DateWorkArea null1;
    private Common omitSecondaryObligInd;
    private ObligationType hardcodedAccruingClass;
    private Obligation hardcodedSecondary;
    private CashReceiptDetailStatus hardcodedAdjusted;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KeyOnlyObligor1.
    /// </summary>
    [JsonPropertyName("keyOnlyObligor1")]
    public CsePerson KeyOnlyObligor1
    {
      get => keyOnlyObligor1 ??= new();
      set => keyOnlyObligor1 = value;
    }

    /// <summary>
    /// A value of KeyOnlyObligor2.
    /// </summary>
    [JsonPropertyName("keyOnlyObligor2")]
    public CsePersonAccount KeyOnlyObligor2
    {
      get => keyOnlyObligor2 ??= new();
      set => keyOnlyObligor2 = value;
    }

    /// <summary>
    /// A value of KeyOnlyObligationType.
    /// </summary>
    [JsonPropertyName("keyOnlyObligationType")]
    public ObligationType KeyOnlyObligationType
    {
      get => keyOnlyObligationType ??= new();
      set => keyOnlyObligationType = value;
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
    /// A value of KeyOnlyDebt.
    /// </summary>
    [JsonPropertyName("keyOnlyDebt")]
    public ObligationTransaction KeyOnlyDebt
    {
      get => keyOnlyDebt ??= new();
      set => keyOnlyDebt = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public CashReceiptDetail Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public CashReceiptDetail DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private CsePerson keyOnlyObligor1;
    private CsePersonAccount keyOnlyObligor2;
    private ObligationType keyOnlyObligationType;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction keyOnlyDebt;
    private DebtDetail debtDetail;
    private LegalAction legalAction;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetail adjustment;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetail delMe;
  }
#endregion
}
