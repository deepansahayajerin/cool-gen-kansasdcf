// Program: FN_B602_CALC_BY_WORKER_ID, ID: 372707139, model: 746.
// Short name: SWE02579
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B602_CALC_BY_WORKER_ID.
/// </summary>
[Serializable]
public partial class FnB602CalcByWorkerId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B602_CALC_BY_WORKER_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB602CalcByWorkerId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB602CalcByWorkerId.
  /// </summary>
  public FnB602CalcByWorkerId(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // April, 2001, pr#118493, Maureen Brown: Removed the qualifiers for date 
    // ranges.
    local.Current.Date = Now().Date;
    local.CurrentTimestamp.Timestamp = Now();
    local.CanamSend.Parm2 = "";
    local.HardcodedRecovery.Classification = "R";
    local.CanamSend.Parm1 = "OF";
    UseFnB602EabProduceReport2();

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

    // April, 2001, pr#118493, Maureen Brown: Removed the qualifiers for date 
    // ranges.
    foreach(var item in ReadServiceProvider())
    {
      if (Equal(entities.ServiceProvider.UserId, local.Prev.UserId))
      {
        continue;
      }
      else
      {
        local.Prev.UserId = entities.ServiceProvider.UserId;
      }

      // April, 2001, pr#118493, Maureen Brown: Removed the qualifiers for date 
      // ranges.
      foreach(var item1 in ReadObligationType())
      {
        local.PriorRecoveryDue.TotalCurrency = 0;
        local.TotalOsBalance.TotalCurrency = 0;
        local.TotalCollections.TotalCurrency = 0;
        local.BalanceOwed.TotalCurrency = 0;
        local.InterestOwed.TotalCurrency = 0;
        local.Obligation.Count = 0;
        local.CourtOrder.Count = 0;

        foreach(var item2 in ReadObligation())
        {
          if (ReadLegalAction())
          {
            local.LegalActionFound.Flag = "Y";
          }
          else
          {
            local.LegalActionFound.Flag = "N";
          }

          foreach(var item3 in ReadDebtDetail())
          {
            if (!Lt(import.High.Date, entities.DebtDetail.DueDt))
            {
              // : Recovery Due and Recovery Interest Due
              ++local.Obligation.Count;

              if (AsChar(local.LegalActionFound.Flag) == 'Y')
              {
                ++local.CourtOrder.Count;
              }

              local.BalanceOwed.TotalCurrency += entities.DebtDetail.
                BalanceDueAmt;
              local.InterestOwed.TotalCurrency += entities.DebtDetail.
                InterestBalanceDueAmt.GetValueOrDefault();
            }

            if (Lt(entities.DebtDetail.DueDt, import.Low.Date))
            {
              // : Add the amount to Prior Recovery Due
              local.PriorRecoveryDue.TotalCurrency =
                entities.DebtDetail.BalanceDueAmt + entities
                .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() + local
                .PriorRecoveryDue.TotalCurrency;
            }
          }

          // :  Reverse adjustments to calculate the prior balance and the 
          // balance
          //    as of the high date of the date range.
          //    - If the adjustment occurred after the low date of the report 
          // date
          //    range, reverse the prior balance.
          //    - If it occurred after the high date of the report date range, 
          // reverse
          //      both the prior balance owed and the balance owed for the 
          // current report
          foreach(var item3 in ReadDebtAdjustment())
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

            // : Our totals do not include debt details with due date > high 
            // date
            //   of report date range.
            if (Lt(import.High.Date, entities.DebtDetail.DueDt))
            {
              goto ReadEach;
            }
            else if (Lt(entities.DebtDetail.DueDt, import.Low.Date) && !
              Lt(Date(entities.DebtAdjustment.CreatedTmst), import.Low.Date))
            {
              // : Reverse this adjustment for prior balance, since the debt 
              // detail due date was due in the prior report period.
              if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
              {
                // : Adjustment was an increase, so to reverse it, subtract from
                // the prior balance due.
                local.PriorRecoveryDue.TotalCurrency -= entities.DebtAdjustment.
                  Amount;
              }
              else
              {
                // : Adjustment was a decrease, so to reverse it, add to the 
                // prior balance due.
                local.PriorRecoveryDue.TotalCurrency += entities.DebtAdjustment.
                  Amount;
              }
            }

            // : Reverse this adjustment for current balance and current 
            // interest
            //   balance (if due date <= high date of report period, and 
            // adjustment
            //   was created after the high date of the report period).
            if (Lt(import.High.Date, Date(entities.DebtAdjustment.CreatedTmst)))
            {
              if (Equal(entities.ObligationTransactionRlnRsn.Code, "DA IOA"))
              {
                // : Interest Adjustment
                if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
                {
                  // : Adjustment was an increase, so to reverse it, subtract 
                  // from the interest due balance.
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
                  // : Adjustment was an increase, so to reverse it, subtract 
                  // from the recovery due balance.
                  local.BalanceOwed.TotalCurrency -= entities.DebtAdjustment.
                    Amount;
                }
                else
                {
                  // : Adjustment was a decrease, so to reverse it, add to the 
                  // recovery due balance.
                  local.BalanceOwed.TotalCurrency += entities.DebtAdjustment.
                    Amount;
                }
              }
            }
          }

          // : Reverse any collections and collection adjustments as per 
          // requirements.
          foreach(var item3 in ReadCollectionDebtDetail())
          {
            if (Lt(import.High.Date, entities.DebtDetail.DueDt))
            {
              continue;
            }

            local.CollectionCreated.Date =
              Date(entities.Collection.CreatedTmst);

            if (AsChar(entities.Collection.AdjustedInd) == 'Y')
            {
              // : Collection adjustments are reversed on the balance owed if
              //  the collection was created before the high date of the report 
              // date
              //  range, but the adjustment was created after that.
              if (Lt(import.High.Date,
                entities.Collection.CollectionAdjustmentDt) && !
                Lt(import.High.Date, local.CollectionCreated.Date))
              {
                if (AsChar(entities.Collection.AppliedToCode) == 'I')
                {
                  // : Adjust collection activity for the interest balance owed.
                  local.InterestOwed.TotalCurrency -= entities.Collection.
                    Amount;
                }
                else
                {
                  // : Adjust collection activity for the debt balance owed.
                  local.BalanceOwed.TotalCurrency -= entities.Collection.Amount;
                }
              }

              // : Collection adjustments are reversed on the prior balance if
              //  the adjustment occurred after the low date of the report
              //  range, and the collection occurred before this date.
              if (!Lt(entities.Collection.CollectionAdjustmentDt,
                import.Low.Date) && Lt
                (local.CollectionCreated.Date, import.Low.Date))
              {
                local.PriorRecoveryDue.TotalCurrency -= entities.Collection.
                  Amount;
              }
            }
            else
            {
              // : This is an unadjusted collection.
              // : Add the collection to total collections for the report period
              // if
              //   it was created within that date range.
              if (!Lt(local.CollectionCreated.Date, import.Low.Date) && !
                Lt(import.High.Date, local.CollectionCreated.Date))
              {
                local.TotalCollections.TotalCurrency += entities.Collection.
                  Amount;
              }

              // : Add it back to the balance owed if collection date is
              //   greater than the high date of the report period.
              if (Lt(import.High.Date, local.CollectionCreated.Date) && !
                Lt(import.High.Date, entities.DebtDetail.DueDt))
              {
                if (AsChar(entities.Collection.AppliedToCode) == 'I')
                {
                  // : The collection was applied to interest.
                  local.InterestOwed.TotalCurrency += entities.Collection.
                    Amount;
                }
                else
                {
                  local.BalanceOwed.TotalCurrency += entities.Collection.Amount;
                }
              }

              // : Reverse any collections on the prior balance amount if they
              //  were created after the low date of the current  report period.
              if (!Lt(local.CollectionCreated.Date, import.Low.Date) && Lt
                (entities.DebtDetail.DueDt, import.Low.Date))
              {
                local.PriorRecoveryDue.TotalCurrency += entities.Collection.
                  Amount;
              }
            }
          }

ReadEach:
          ;
        }

        local.TotalOsBalance.TotalCurrency = local.BalanceOwed.TotalCurrency + local
          .InterestOwed.TotalCurrency;

        if (local.TotalCollections.TotalCurrency == 0 && local
          .TotalOsBalance.TotalCurrency == 0)
        {
          // : Go to next Obligation type if totals are zero.
          continue;
        }

        // : Calculate percentage paid.
        if (local.TotalCollections.TotalCurrency <= 0)
        {
          local.PrcntOfTotCollected.TotalCurrency = 0;
        }
        else
        {
          // ###mfb - find out if they want this done the same as 237.  if so, 
          // we need the average and the prior balance.  If not, get rid of all
          // the code for prior balance.  Also note that prior recovery due can
          // bring down the average amount and then the percentage is out of
          // whack.  Since prior isn't on the report, it may be confusing.
          local.Average.TotalCurrency =
            Math.Round((
              local.TotalOsBalance.TotalCurrency + local
            .PriorRecoveryDue.TotalCurrency) /
            2, 2, MidpointRounding.AwayFromZero);

          if (local.Average.TotalCurrency == 0)
          {
            local.PrcntOfTotCollected.TotalCurrency = 0;
          }
          else
          {
            local.PrcntOfTotCollected.TotalCurrency =
              Math.Round(
                local.TotalCollections.TotalCurrency / local
              .Average.TotalCurrency * 100, 2, MidpointRounding.AwayFromZero);
          }
        }

        // : Calculate the percentage of obligations that were court ordered.
        local.PercentCourtOrd.TotalCurrency =
          Math.Round((decimal)local.CourtOrder.Count / local
          .Obligation.Count * 100, 2, MidpointRounding.AwayFromZero);
        UseFnB602EabProduceReport1();

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
    }

    local.CanamSend.Parm1 = "CF";
    UseFnB602EabProduceReport3();

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

  private void UseFnB602EabProduceReport1()
  {
    var useImport = new FnB602EabProduceReport.Import();
    var useExport = new FnB602EabProduceReport.Export();

    useImport.ServiceProvider.Assign(entities.ServiceProvider);
    useImport.ObligationType.Code = entities.ObligationType.Code;
    useImport.LowDate.Date = import.Low.Date;
    useImport.HighDate.Date = import.High.Date;
    MoveReportParms(local.CanamSend, useImport.ReportParms);
    useImport.TotalColl.TotalCurrency = local.TotalCollections.TotalCurrency;
    useImport.BalOwed.TotalCurrency = local.BalanceOwed.TotalCurrency;
    useImport.IntOwed.TotalCurrency = local.InterestOwed.TotalCurrency;
    useImport.TotOsBal.TotalCurrency = local.TotalOsBalance.TotalCurrency;
    useImport.PrcntCollected.TotalCurrency =
      local.PrcntOfTotCollected.TotalCurrency;
    useImport.PriorBalOwed.TotalCurrency = local.PriorRecoveryDue.TotalCurrency;
    useImport.Obligation.Count = local.Obligation.Count;
    useImport.PrcntCourtOrd.TotalCurrency = local.PercentCourtOrd.TotalCurrency;
    useImport.CourtOrder.Count = local.CourtOrder.Count;
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnB602EabProduceReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private void UseFnB602EabProduceReport2()
  {
    var useImport = new FnB602EabProduceReport.Import();
    var useExport = new FnB602EabProduceReport.Export();

    useImport.LowDate.Date = import.Low.Date;
    useImport.HighDate.Date = import.High.Date;
    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnB602EabProduceReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private void UseFnB602EabProduceReport3()
  {
    var useImport = new FnB602EabProduceReport.Import();
    var useExport = new FnB602EabProduceReport.Export();

    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnB602EabProduceReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private IEnumerable<bool> ReadCollectionDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Collection.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadCollectionDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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

  private IEnumerable<bool> ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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

  private IEnumerable<bool> ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "spdId", entities.ServiceProvider.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CreatedBy = db.GetString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);

        return true;
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

  private IEnumerable<bool> ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadServiceProvider",
      null,
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;

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
    /// A value of LegalActionFound.
    /// </summary>
    [JsonPropertyName("legalActionFound")]
    public Common LegalActionFound
    {
      get => legalActionFound ??= new();
      set => legalActionFound = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public ServiceProvider Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of HardcodedRecovery.
    /// </summary>
    [JsonPropertyName("hardcodedRecovery")]
    public ObligationType HardcodedRecovery
    {
      get => hardcodedRecovery ??= new();
      set => hardcodedRecovery = value;
    }

    /// <summary>
    /// A value of LowTimestamp.
    /// </summary>
    [JsonPropertyName("lowTimestamp")]
    public DateWorkArea LowTimestamp
    {
      get => lowTimestamp ??= new();
      set => lowTimestamp = value;
    }

    /// <summary>
    /// A value of HighTimestamp.
    /// </summary>
    [JsonPropertyName("highTimestamp")]
    public DateWorkArea HighTimestamp
    {
      get => highTimestamp ??= new();
      set => highTimestamp = value;
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
    /// A value of WorkCollections.
    /// </summary>
    [JsonPropertyName("workCollections")]
    public Common WorkCollections
    {
      get => workCollections ??= new();
      set => workCollections = value;
    }

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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Common Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of PercentCourtOrd.
    /// </summary>
    [JsonPropertyName("percentCourtOrd")]
    public Common PercentCourtOrd
    {
      get => percentCourtOrd ??= new();
      set => percentCourtOrd = value;
    }

    /// <summary>
    /// A value of CourtOrder.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    public Common CourtOrder
    {
      get => courtOrder ??= new();
      set => courtOrder = value;
    }

    private Common legalActionFound;
    private ServiceProvider prev;
    private ReportParms canamReturn;
    private ReportParms canamSend;
    private DateWorkArea collectionCreated;
    private DateWorkArea currentTimestamp;
    private DateWorkArea current;
    private ObligationType hardcodedRecovery;
    private DateWorkArea lowTimestamp;
    private DateWorkArea highTimestamp;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private Common average;
    private Common workCollections;
    private Common totalCollections;
    private Common balanceOwed;
    private Common interestOwed;
    private Common totalOsBalance;
    private Common prcntOfTotCollected;
    private Common priorRecoveryDue;
    private Common obligation;
    private Common percentCourtOrd;
    private Common courtOrder;
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
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    /// <summary>
    /// A value of GroupBy.
    /// </summary>
    [JsonPropertyName("groupBy")]
    public Obligation GroupBy
    {
      get => groupBy ??= new();
      set => groupBy = value;
    }

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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private ObligationAssignment obligationAssignment;
    private Obligation groupBy;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransaction debtAdjustment;
    private ObligationTransaction debt;
    private Collection collection;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private ObligationType obligationType;
    private LegalAction legalAction;
  }
#endregion
}
