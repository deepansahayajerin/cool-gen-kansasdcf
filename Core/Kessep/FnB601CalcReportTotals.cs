// Program: FN_B601_CALC_REPORT_TOTALS, ID: 372706230, model: 746.
// Short name: SWE02570
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B601_CALC_REPORT_TOTALS.
/// </summary>
[Serializable]
public partial class FnB601CalcReportTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B601_CALC_REPORT_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB601CalcReportTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB601CalcReportTotals.
  /// </summary>
  public FnB601CalcReportTotals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // MAINTENANCE LOG
    // : Jan 27, 2000, mbrown, pr# 85644 - Add count and total amount of
    //   number of obligations created in the report period.
    // : Once the Balance Due and Interest Due totals are derived from
    //   debt details, collection activity for the report period is added
    //   back to the debt balance.  This is so that collection totals do not
    //   appear to exceed balance owed totals on the report.
    local.Current.Date = Now().Date;
    local.CurrentTimestamp.Timestamp = Now();
    local.CanamSend.Parm2 = "";
    local.HardcodedRecovery.Classification = "R";
    local.CanamSend.Parm1 = "OF";
    UseFnB601EabProduceReport2();

    if (!IsEmpty(local.CanamReturn.Parm1))
    {
      // Parm 2 contains the file status
      // :output error here
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Problem opening report file.  File status is  " + local
        .CanamReturn.Parm2;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.CanamSend.Parm1 = "GR";

    foreach(var item in ReadObligationType())
    {
      local.PriorRecoveryDue.TotalCurrency = 0;
      local.TotalCollections.TotalCurrency = 0;
      local.NewDebts.TotalCurrency = 0;
      local.NewDebts.Count = 0;
      local.ObligationType.Text30 = entities.ObligationType.Name;

      // : Collections within the time period.
      // :   Recovery Due and Recovery Interest Due
      ReadDebtDetail1();

      // : Jan 27, 2000, mbrown, pr# 85644 - Add count and total amount of
      //   number of obligations created in the report period.
      foreach(var item1 in ReadDebt())
      {
        if (ReadDebtAdjustment1())
        {
          if (entities.DebtAdjustment.Amount == entities.Debt.Amount)
          {
            continue;
          }
          else
          {
            local.NewDebts.TotalCurrency -= entities.DebtAdjustment.Amount;
          }
        }
        else
        {
          // OK
        }

        local.NewDebts.TotalCurrency += entities.Debt.Amount;
        ++local.NewDebts.Count;
      }

      // : Prior Recovery Due
      ReadDebtDetail2();
      local.PriorRecoveryDue.TotalCurrency = local.WorkBalance.TotalCurrency + local
        .WorkInterest.TotalCurrency;

      // :  Reverse adjustments to calculate the prior balance and the balance
      //    as of the high date of the date range.
      //    - If the adjustment occurred after the low date of the report date
      //    range, reverse the prior balance.
      //    - If it occurred after the high date of the report date range, 
      // reverse
      //      both the prior balance owed and the balance owed for the current 
      // report
      foreach(var item1 in ReadDebtAdjustment2())
      {
        if (!Lt(import.Low.Date, Date(entities.DebtAdjustment.CreatedTmst)))
        {
          break;
        }

        // : Find the debt detail that the adjustment relates to.
        if (!ReadObligationTransactionRlnRsnDebtDetail2())
        {
          if (!ReadObligationTransactionRlnRsnDebtDetail1())
          {
            // : Shouldn't get this.
            continue;
          }
        }

        if (Lt(import.High.Date, entities.DebtDetail.DueDt))
        {
          // : IF the adjustment is for a debt detail with a due date greater
          //   than the report range, go to the next one.
          continue;
        }
        else if (Lt(entities.DebtDetail.DueDt, import.Low.Date) && !
          Lt(Date(entities.DebtAdjustment.CreatedTmst), import.Low.Date))
        {
          // : Reverse this adjustment for prior balance, since the debt detail 
          // due date was due in the prior report period.
          if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
          {
            // : Adjustment was an increase, so to reverse it, subtract from the
            // prior balance due.
            local.PriorRecoveryDue.TotalCurrency -= entities.DebtAdjustment.
              Amount;
          }
          else
          {
            // : Adjustment was a decrease, so to reverse it, add to the prior 
            // balance due.
            local.PriorRecoveryDue.TotalCurrency += entities.DebtAdjustment.
              Amount;
          }
        }

        // : Reverse this adjustment for current balance and current interest
        //   balance (if due date <= high date of report period, and adjustment
        //   was created after the high date of the report period).
        if (Lt(import.High.Date, Date(entities.DebtAdjustment.CreatedTmst)))
        {
          if (Equal(entities.ObligationTransactionRlnRsn.Code, "DA IOA"))
          {
            // : Interest Adjustment
            if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
            {
              // : Adjustment was an increase, so to reverse it, subtract from 
              // the interest due balance.
              local.InterestOwed.TotalCurrency -= entities.DebtAdjustment.
                Amount;
            }
            else
            {
              // : Adjustment was a decrease, so to reverse it, add to the 
              // interest due balance.
              local.InterestOwed.TotalCurrency += entities.DebtAdjustment.
                Amount;
            }
          }
          else
          {
            // : Debt Adjustment
            if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
            {
              // : Adjustment was an increase, so to reverse it, subtract from 
              // the recovery due balance.
              local.BalanceOwed.TotalCurrency -= entities.DebtAdjustment.Amount;
            }
            else
            {
              // : Adjustment was a decrease, so to reverse it, add to the 
              // recovery due balance.
              local.BalanceOwed.TotalCurrency += entities.DebtAdjustment.Amount;
            }
          }
        }
      }

      // : Reverse any collections and collection adjustments as per 
      // requirements.
      foreach(var item1 in ReadCollectionDebtDetail())
      {
        if (Lt(import.High.Date, entities.DebtDetail.DueDt))
        {
          continue;
        }

        local.CollectionCreated.Date = Date(entities.Collection.CreatedTmst);

        if (AsChar(entities.Collection.AdjustedInd) == 'Y')
        {
          // : Collection adjustments are reversed on the balance owed if
          //  the collection was created before the low date of the report date
          //  range, but the adjustment was created after that.
          if (!Lt(entities.Collection.CollectionAdjustmentDt, import.High.Date) &&
            Lt(local.CollectionCreated.Date, import.High.Date))
          {
            if (AsChar(entities.Collection.AppliedToCode) == 'I')
            {
              // : Adjust collection activity for the interest balance owed.
              local.InterestOwed.TotalCurrency -= entities.Collection.Amount;
            }
            else
            {
              // : Adjust collection activity for the debt balance owed.
              local.BalanceOwed.TotalCurrency -= entities.Collection.Amount;
            }
          }

          // : Collection adjustments are reversed on the prior balance if
          //  the adjustment occurred after the low date of the
          //  report range, and the collection occurred before this date.
          if (!Lt(entities.Collection.CollectionAdjustmentDt, import.Low.Date) &&
            Lt(local.CollectionCreated.Date, import.Low.Date))
          {
            local.PriorRecoveryDue.TotalCurrency -= entities.Collection.Amount;
          }
        }
        else
        {
          // : This is an unadjusted collection.  Add it back to the balance due
          //  if collection date is within the report period.  This is so that 
          // it
          //  is included in the debt totals.  Otherwise, collections may
          //  exceed debts on the report, since amount owed on debt detail
          //  will have the collection subtracted.
          // : Add the collection to total collections for the report period if
          //   it was created within that date range.
          if (!Lt(local.CollectionCreated.Date, import.Low.Date) && !
            Lt(import.High.Date, local.CollectionCreated.Date))
          {
            local.TotalCollections.TotalCurrency += entities.Collection.Amount;
          }

          if (!Lt(local.CollectionCreated.Date, import.High.Date) && !
            Lt(import.High.Date, entities.DebtDetail.DueDt))
          {
            if (AsChar(entities.Collection.AppliedToCode) == 'I')
            {
              // : The collection was applied to interest.
              local.InterestOwed.TotalCurrency += entities.Collection.Amount;
            }
            else
            {
              local.BalanceOwed.TotalCurrency += entities.Collection.Amount;
            }
          }

          // : Reverse any collections on the prior balance amount if they
          //  were created after the low date of the current report period.
          if (!Lt(local.CollectionCreated.Date, import.Low.Date) && Lt
            (entities.DebtDetail.DueDt, import.Low.Date))
          {
            local.PriorRecoveryDue.TotalCurrency += entities.Collection.Amount;
          }
        }
      }

      local.TotalOsBalance.TotalCurrency = local.BalanceOwed.TotalCurrency + local
        .InterestOwed.TotalCurrency;

      // : Calculate percentage paid.
      if (local.TotalCollections.TotalCurrency <= 0)
      {
        local.PrcntOfTotCollected.AverageCurrency = 0;
      }
      else
      {
        local.Average.TotalCurrency =
          Math.Round((
            local.TotalOsBalance.TotalCurrency + local
          .PriorRecoveryDue.TotalCurrency) /
          2, 2, MidpointRounding.AwayFromZero);

        if (local.Average.TotalCurrency == 0)
        {
          local.PrcntOfTotCollected.AverageCurrency = 0;
        }
        else
        {
          local.PrcntOfTotCollected.AverageCurrency =
            Math.Round(
              local.TotalCollections.TotalCurrency / local
            .Average.TotalCurrency * 100, 2, MidpointRounding.AwayFromZero);
        }
      }

      UseFnB601EabProduceReport1();

      if (!IsEmpty(local.CanamReturn.Parm1))
      {
        // : Error
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Problem writing report file.  File status is  " + local
          .CanamReturn.Parm2;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.CanamSend.Parm1 = "CF";
    UseFnB601EabProduceReport3();

    if (!IsEmpty(local.CanamReturn.Parm1))
    {
      // : Error
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Problem closing report file.  File status is  " + local
        .CanamReturn.Parm2;
      UseCabErrorReport();
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseFnB601EabProduceReport1()
  {
    var useImport = new FnB601EabProduceReport.Import();
    var useExport = new FnB601EabProduceReport.Export();

    useImport.ReportLiteral.Text10 = import.ReportLiteral.Text10;
    useImport.LowDate.Date = import.Low.Date;
    useImport.HighDate.Date = import.High.Date;
    useImport.TotalColl.TotalCurrency = local.TotalCollections.TotalCurrency;
    useImport.BalOwed.TotalCurrency = local.BalanceOwed.TotalCurrency;
    useImport.IntOwed.TotalCurrency = local.InterestOwed.TotalCurrency;
    useImport.TotOsBal.TotalCurrency = local.TotalOsBalance.TotalCurrency;
    useImport.Prcnt.AverageCurrency = local.PrcntOfTotCollected.AverageCurrency;
    useImport.PriorRecoveryDue.TotalCurrency =
      local.PriorRecoveryDue.TotalCurrency;
    MoveReportParms(local.CanamSend, useImport.ReportParms);
    useImport.ObligationType.Code = entities.ObligationType.Code;
    MoveCommon(local.NewDebts, useImport.NewDebts);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnB601EabProduceReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private void UseFnB601EabProduceReport2()
  {
    var useImport = new FnB601EabProduceReport.Import();
    var useExport = new FnB601EabProduceReport.Export();

    useImport.ReportLiteral.Text10 = import.ReportLiteral.Text10;
    useImport.LowDate.Date = import.Low.Date;
    useImport.HighDate.Date = import.High.Date;
    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnB601EabProduceReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private void UseFnB601EabProduceReport3()
  {
    var useImport = new FnB601EabProduceReport.Import();
    var useExport = new FnB601EabProduceReport.Export();

    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnB601EabProduceReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private IEnumerable<bool> ReadCollectionDebtDetail()
  {
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadCollectionDebtDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyId", entities.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 14);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.BalForIntCompBefColl =
          db.GetNullableDecimal(reader, 18);
        entities.Collection.CumIntChargedUptoColl =
          db.GetNullableDecimal(reader, 19);
        entities.Collection.CumIntCollAfterThisColl =
          db.GetNullableDecimal(reader, 20);
        entities.Collection.IntBalAftThisColl =
          db.GetNullableDecimal(reader, 21);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 22);
        entities.Collection.AppliedToFuture = db.GetString(reader, 23);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 24);
        entities.DebtDetail.CspNumber = db.GetString(reader, 25);
        entities.DebtDetail.CpaType = db.GetString(reader, 26);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 27);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 28);
        entities.DebtDetail.OtrType = db.GetString(reader, 29);
        entities.DebtDetail.DueDt = db.GetDate(reader, 30);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 31);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 32);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 33);
        entities.Collection.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebt()
  {
    entities.Debt.Populated = false;

    return ReadEach("ReadDebt",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "timestamp1", import.Low.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.High.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadDebtAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment1",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetDateTime(
          command, "timestamp1", import.Low.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.High.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 12);
        entities.DebtAdjustment.DebtAdjCollAdjProcDt =
          db.GetNullableDate(reader, 13);
        entities.DebtAdjustment.ReasonCode = db.GetString(reader, 14);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
      });
  }

  private IEnumerable<bool> ReadDebtAdjustment2()
  {
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 12);
        entities.DebtAdjustment.DebtAdjCollAdjProcDt =
          db.GetNullableDate(reader, 13);
        entities.DebtAdjustment.ReasonCode = db.GetString(reader, 14);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);

        return true;
      });
  }

  private bool ReadDebtDetail1()
  {
    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetDate(command, "dueDt", import.High.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.BalanceOwed.TotalCurrency = db.GetDecimal(reader, 0);
        local.InterestOwed.TotalCurrency = db.GetDecimal(reader, 1);
      });
  }

  private bool ReadDebtDetail2()
  {
    return Read("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetDate(command, "dueDt", import.Low.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.WorkBalance.TotalCurrency = db.GetDecimal(reader, 0);
        local.WorkInterest.TotalCurrency = db.GetDecimal(reader, 1);
      });
  }

  private bool ReadObligationTransactionRlnRsnDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.DebtAdjustment.Populated);
    entities.ObligationTransactionRlnRsn.Populated = false;
    entities.DebtDetail.Populated = false;

    return Read("ReadObligationTransactionRlnRsnDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.DebtAdjustment.OtyType);
        db.SetString(command, "otrPType", entities.DebtAdjustment.Type1);
        db.SetInt32(
          command, "otrPGeneratedId",
          entities.DebtAdjustment.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.DebtAdjustment.CpaType);
        db.SetString(command, "cspPNumber", entities.DebtAdjustment.CspNumber);
        db.SetInt32(
          command, "obgPGeneratedId", entities.DebtAdjustment.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Code = db.GetString(reader, 1);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.DebtDetail.CspNumber = db.GetString(reader, 3);
        entities.DebtDetail.CpaType = db.GetString(reader, 4);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 6);
        entities.DebtDetail.OtrType = db.GetString(reader, 7);
        entities.DebtDetail.DueDt = db.GetDate(reader, 8);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 9);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 10);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 11);
        entities.ObligationTransactionRlnRsn.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadObligationTransactionRlnRsnDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.DebtAdjustment.Populated);
    entities.ObligationTransactionRlnRsn.Populated = false;
    entities.DebtDetail.Populated = false;

    return Read("ReadObligationTransactionRlnRsnDebtDetail2",
      (db, command) =>
      {
        db.
          SetInt32(command, "otyTypeSecondary", entities.DebtAdjustment.OtyType);
          
        db.SetString(command, "otrType", entities.DebtAdjustment.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.DebtAdjustment.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.DebtAdjustment.CpaType);
        db.SetString(command, "cspNumber", entities.DebtAdjustment.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtAdjustment.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Code = db.GetString(reader, 1);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.DebtDetail.CspNumber = db.GetString(reader, 3);
        entities.DebtDetail.CpaType = db.GetString(reader, 4);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 6);
        entities.DebtDetail.OtrType = db.GetString(reader, 7);
        entities.DebtDetail.DueDt = db.GetDate(reader, 8);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 9);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 10);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 11);
        entities.ObligationTransactionRlnRsn.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private IEnumerable<bool> ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationType",
      (db, command) =>
      {
        db.SetString(
          command, "debtTypClass", local.HardcodedRecovery.Classification);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

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
    /// A value of ReportLiteral.
    /// </summary>
    [JsonPropertyName("reportLiteral")]
    public TextWorkArea ReportLiteral
    {
      get => reportLiteral ??= new();
      set => reportLiteral = value;
    }

    /// <summary>
    /// A value of Low.
    /// </summary>
    [JsonPropertyName("low")]
    public DateWorkArea Low
    {
      get => low ??= new();
      set => low = value;
    }

    /// <summary>
    /// A value of High.
    /// </summary>
    [JsonPropertyName("high")]
    public DateWorkArea High
    {
      get => high ??= new();
      set => high = value;
    }

    private TextWorkArea reportLiteral;
    private DateWorkArea low;
    private DateWorkArea high;
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
    /// A value of TotalCollections.
    /// </summary>
    [JsonPropertyName("totalCollections")]
    public Common TotalCollections
    {
      get => totalCollections ??= new();
      set => totalCollections = value;
    }

    /// <summary>
    /// A value of BalanceOwed.
    /// </summary>
    [JsonPropertyName("balanceOwed")]
    public Common BalanceOwed
    {
      get => balanceOwed ??= new();
      set => balanceOwed = value;
    }

    /// <summary>
    /// A value of InterestOwed.
    /// </summary>
    [JsonPropertyName("interestOwed")]
    public Common InterestOwed
    {
      get => interestOwed ??= new();
      set => interestOwed = value;
    }

    /// <summary>
    /// A value of TotalOsBalance.
    /// </summary>
    [JsonPropertyName("totalOsBalance")]
    public Common TotalOsBalance
    {
      get => totalOsBalance ??= new();
      set => totalOsBalance = value;
    }

    /// <summary>
    /// A value of PrcntOfTotCollected.
    /// </summary>
    [JsonPropertyName("prcntOfTotCollected")]
    public Common PrcntOfTotCollected
    {
      get => prcntOfTotCollected ??= new();
      set => prcntOfTotCollected = value;
    }

    /// <summary>
    /// A value of PriorRecoveryDue.
    /// </summary>
    [JsonPropertyName("priorRecoveryDue")]
    public Common PriorRecoveryDue
    {
      get => priorRecoveryDue ??= new();
      set => priorRecoveryDue = value;
    }

    /// <summary>
    /// A value of WorkInterest.
    /// </summary>
    [JsonPropertyName("workInterest")]
    public Common WorkInterest
    {
      get => workInterest ??= new();
      set => workInterest = value;
    }

    /// <summary>
    /// A value of WorkBalance.
    /// </summary>
    [JsonPropertyName("workBalance")]
    public Common WorkBalance
    {
      get => workBalance ??= new();
      set => workBalance = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public TextWorkArea ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of TotalLiteral.
    /// </summary>
    [JsonPropertyName("totalLiteral")]
    public TextWorkArea TotalLiteral
    {
      get => totalLiteral ??= new();
      set => totalLiteral = value;
    }

    /// <summary>
    /// A value of CanamReturn.
    /// </summary>
    [JsonPropertyName("canamReturn")]
    public ReportParms CanamReturn
    {
      get => canamReturn ??= new();
      set => canamReturn = value;
    }

    /// <summary>
    /// A value of CanamSend.
    /// </summary>
    [JsonPropertyName("canamSend")]
    public ReportParms CanamSend
    {
      get => canamSend ??= new();
      set => canamSend = value;
    }

    /// <summary>
    /// A value of CollectionCreated.
    /// </summary>
    [JsonPropertyName("collectionCreated")]
    public DateWorkArea CollectionCreated
    {
      get => collectionCreated ??= new();
      set => collectionCreated = value;
    }

    /// <summary>
    /// A value of CurrentTimestamp.
    /// </summary>
    [JsonPropertyName("currentTimestamp")]
    public DateWorkArea CurrentTimestamp
    {
      get => currentTimestamp ??= new();
      set => currentTimestamp = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of TotalCollectionAdjustments.
    /// </summary>
    [JsonPropertyName("totalCollectionAdjustments")]
    public Common TotalCollectionAdjustments
    {
      get => totalCollectionAdjustments ??= new();
      set => totalCollectionAdjustments = value;
    }

    /// <summary>
    /// A value of HardcodedRecovery.
    /// </summary>
    [JsonPropertyName("hardcodedRecovery")]
    public ObligationType HardcodedRecovery
    {
      get => hardcodedRecovery ??= new();
      set => hardcodedRecovery = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of ReportLiteral.
    /// </summary>
    [JsonPropertyName("reportLiteral")]
    public TextWorkArea ReportLiteral
    {
      get => reportLiteral ??= new();
      set => reportLiteral = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of Average.
    /// </summary>
    [JsonPropertyName("average")]
    public Common Average
    {
      get => average ??= new();
      set => average = value;
    }

    /// <summary>
    /// A value of NewDebts.
    /// </summary>
    [JsonPropertyName("newDebts")]
    public Common NewDebts
    {
      get => newDebts ??= new();
      set => newDebts = value;
    }

    private Common totalCollections;
    private Common balanceOwed;
    private Common interestOwed;
    private Common totalOsBalance;
    private Common prcntOfTotCollected;
    private Common priorRecoveryDue;
    private Common workInterest;
    private Common workBalance;
    private TextWorkArea obligationType;
    private TextWorkArea totalLiteral;
    private ReportParms canamReturn;
    private ReportParms canamSend;
    private DateWorkArea collectionCreated;
    private DateWorkArea currentTimestamp;
    private DateWorkArea current;
    private Common totalCollectionAdjustments;
    private ObligationType hardcodedRecovery;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private TextWorkArea reportLiteral;
    private EabReportSend neededToOpen;
    private Common average;
    private Common newDebts;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public Obligation KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
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

    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransaction debtAdjustment;
    private ObligationTransaction keyOnlyDebt;
    private ObligationTransaction debt;
    private Collection collection;
    private DebtDetail debtDetail;
    private Obligation keyOnly;
    private ObligationType obligationType;
  }
#endregion
}
