// Program: FN_RELEASE_SUPPRESSED_DISBURSEMT, ID: 372544596, model: 746.
// Short name: SWE02577
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_RELEASE_SUPPRESSED_DISBURSEMT.
/// </para>
/// <para>
/// Check on the end dates of the suppressed disbursements. If they are ending 
/// that evening then give them a new status of Released.
/// </para>
/// </summary>
[Serializable]
public partial class FnReleaseSuppressedDisbursemt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RELEASE_SUPPRESSED_DISBURSEMT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReleaseSuppressedDisbursemt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReleaseSuppressedDisbursemt.
  /// </summary>
  public FnReleaseSuppressedDisbursemt(IContext context, Import import,
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
    // ****************************************************************
    // Created: 5/27/99     RK
    // This action block will release all suppressed disbursments that have the 
    // suppression ending today or earlier and haven't been processed.  Except
    // when a new suppression with a higher end date than what is on the current
    // suppression is found.
    // ****************************************************************
    // ****************************************************************
    // 2000-02-14  PR 86861  Fangman  Cleanup for restructuring for the CR FEE 
    // changes.
    // 2000-02-28  PR 86768  Fangman  Adjust code to ensure that suppressed 
    // disbursements are released on the suppression discontinue date.
    // 2000-03-06  PR 86768  Fangman  Adjusted code to release a suppressed 
    // disbursement if the suppression rules have been "turned off".  This means
    // that every disbursement in a suppressed status must have its release
    // date recalculated every night based upon the suppression rules (person/
    // collection) currently in effect.
    // 2000-03-23  PR 86768  Fangman  Per JE - automatic should be checked 1st &
    // if found then the other suppression rules should be skipped, if not
    // found then check for an FDSO Collection type suppression & if found then
    // skip, if not found then use the greater of the collection or person level
    // suppression date.
    // 2000-04-07  PRWORA  Fangman  Add code for URA suppression.
    // 2000-09-18  PRWORA Seg ID A4 Fangman  Added suppression counts.
    // 2000-09-27  PR 98039  Fangman   Added code to set the Suppression Reason 
    // on the Disbursement Status History.
    // Also added suppression counts by type.  Also added "D" suppression report
    // detail info to the control report.
    // 2000-12-14  NA  Fangman  Added information about not changing the 
    // suppression end date for FDSO.  No code was changed so this can go in
    // with the next code change.
    // 2002-01-16  WR 000235  Fangman  PSUM project.  Changed code that updated 
    // the monthly sum table.
    // 2002-06-04  PR 144630  Changed code to use the suppression reason of the 
    // original disbursement for the backed out disbursement for X URA
    // disbursements being suppressed.
    // 2002-10-21  PR 159535  Change rules for FDSO Collection suppression to 
    // not override all other suppression rules.  From this point on
    // disbursements will not be released as long as there is any suppression
    // rule in effect.
    // ****************************************************************
    // ****************************************************************
    // 2002-11-25  PR 162249  Added a count and amount line to the report to 
    // break out the suppressed disbursements that are suppressed due to an FDSO
    // suppression rule.  The suppressed disbursements may also be suppressed
    // by other suppression reason as well.
    // ****************************************************************
    // ******************************************************************
    // 01/13/05   WR 040796  Fangman    Added changes for suppression by court 
    // order number.
    // *******************************************************************
    // ******************************************************************
    // 07/02/19  GVandy  CQ65423	Apply new system suppressions for Deceased
    // 				Payees and Payees without addresses.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.CurrentTimestamp.Timestamp = Now();
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "Duplicate Payment Disbursements Reporting.";
    UseCabControlReport();
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "AR Person    Reference    Disbursement   Process   Collection";
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "  Number       Number         Amount      Date        Date";
    UseCabControlReport();

    foreach(var item in ReadDisbursementStatusHistoryDisbursementTransaction())
    {
      if (!IsEmpty(import.TestFirst.Number) || !IsEmpty(import.TestLast.Number))
      {
        if (Lt(entities.CsePerson.Number, import.TestFirst.Number) || Lt
          (import.TestLast.Number, entities.CsePerson.Number))
        {
          continue;
        }
      }

      ++export.SupprCounts.DtlSupprRead.Count;

      if (AsChar(entities.DisbursementStatusHistory.SuppressionReason) == 'D')
      {
        local.UnformattedAmt.Number112 = entities.Debit.Amount;
        UseCabFormat112AmtFieldTo8();
        local.Unformatted.Date = entities.Debit.DisbursementDate;
        UseCabFormatDate1();
        local.Unformatted.Date = entities.Debit.CollectionDate;
        UseCabFormatDate2();
        local.EabReportSend.RptDetail = entities.CsePerson.Number + "  " + entities
          .Debit.ReferenceNumber + "  " + local.FormattedAmt.Text9 + "  " + local
          .FormattedDisbursementDate.Text10 + "  " + local
          .FormattedCollectionDate.Text10;
        UseCabControlReport();
        ++export.SupprCounts.DtlDSupprCnt.Count;
        export.SupprCounts.DtlDSupprAmt.TotalCurrency += entities.Debit.Amount;
        ++export.SupprCounts.DtlSupprNoChange.Count;

        continue;
      }

      local.Fdso.Flag = "N";
      local.HighestSupprEndDate.Date = local.InitializedDate.Date;

      // 07/02/19  GVandy  CQ65423	Apply new system suppressions for Deceased
      // 				Payees and Payees without addresses.
      // --Once a disbursement is in a system suppression status (Y - AR is 
      // deceased, Z - No active
      //   address for the AR) do not allow it to revert back to a non system 
      // suppression status.
      //   If the system suppression is satisfied (e.g. the date of death is 
      // removed or an address
      //   is added) then the disbursement should proceed to the warrant 
      // processing.
      if (AsChar(entities.DisbursementStatusHistory.SuppressionReason) != 'Y'
        && AsChar(entities.DisbursementStatusHistory.SuppressionReason) != 'Z')
      {
        if (entities.DisbursementType.SystemGeneratedIdentifier == 71 || entities
          .DisbursementType.SystemGeneratedIdentifier == 348 || entities
          .DisbursementType.SystemGeneratedIdentifier == 349)
        {
          // Passthrus and XURAs are not associated to a collection so automatic
          // & collection type suppression would never apply to a passthru.
        }
        else if (ReadCollectionTypeCollectionDisbursementTransaction())
        {
          // ---------- Collection Level Check  ----------
          UseFnCheckForCollDisbSup();

          // This section of code should be replaced by the section below to 
          // ensure the FDSO suppression dates are not changed by reapplying
          // suppression rules.
          local.HighestSupprEndDate.Date =
            local.CollectionSupprEndDate.DiscontinueDate;

          if (Lt(local.InitializedDate.Date,
            local.CollectionSupprEndDate.DiscontinueDate))
          {
            local.ForUpdate.SuppressionReason = "C";

            if (entities.CollectionType.SequentialIdentifier == 3)
            {
              ++local.CfdsoSupprCnt.Count;
              local.CfdsoSupprAmt.TotalCurrency += entities.Debit.Amount;
            }

            // PR 159535  Took out the following IF statement (and the stmts 
            // inside it) to implement Brian's request to NOT have FDSO release
            // dates override other suppression rules
          }

          // This code should replace the section above to ensure the FDSO 
          // suppression dates are not changed by reapplying suppression rules.
          if (AsChar(entities.Collection.AppliedToCode) == 'C' && Equal
            (entities.Collection.ProgramAppliedTo, "NA") && AsChar
            (entities.Collection.AppliedToFuture) == 'Y')
          {
            if (ReadDebtDetail())
            {
              // ---------- Automatic Level Check  ----------
              UseFnCheckForAutomaticDisbSupp();

              if (Lt(local.HighestSupprEndDate.Date,
                local.AutomaticSupprEndDate.DiscontinueDate))
              {
                local.ForUpdate.SuppressionReason = "A";
                local.HighestSupprEndDate.Date =
                  local.AutomaticSupprEndDate.DiscontinueDate;
              }
            }
            else
            {
              ExitState = "FN0211_DEBT_DETAIL_NF";

              break;
            }
          }
        }
        else
        {
          ExitState = "FN0000_COLLECTION_TYPE_NF";

          break;
        }

        // PR 159535  Took out the following IF statement (the stmts inside the 
        // IF stmt were moved below) to implement Brian's request to NOT have
        // FDSO release dates override other suppression rules
        // ---------- Person Level Check  ----------
        UseFnCheckForPerDisbSup();

        if (Lt(local.HighestSupprEndDate.Date,
          local.PersonSupprEndDate.DiscontinueDate))
        {
          local.ForUpdate.SuppressionReason = "P";
          local.HighestSupprEndDate.Date =
            local.PersonSupprEndDate.DiscontinueDate;
        }

        // ---------- X URA Level Check  ----------
        if (entities.Collection.Populated && AsChar
          (entities.Debit.ExcessUraInd) == 'Y')
        {
          if (entities.Debit.Amount < 0)
          {
            local.UraSupprEndDate.DiscontinueDate = local.InitializedDate.Date;

            if (ReadDisbursementTransaction3())
            {
              if (ReadDisbursementTransaction4())
              {
                if (ReadDisbursementStatusHistoryDisbursementStatus())
                {
                  if (entities.OriginalDisbursementStatus.
                    SystemGeneratedIdentifier == 3)
                  {
                    local.ForUpdate.SuppressionReason =
                      entities.OriginalDisbursementStatusHistory.
                        SuppressionReason;
                    local.UraSupprEndDate.DiscontinueDate =
                      entities.OriginalDisbursementStatusHistory.
                        DiscontinueDate;
                  }
                }

                if (!entities.OriginalDisbursementStatusHistory.Populated)
                {
                  ExitState = "FN0000_DISB_STAT_HIST_NF";

                  break;
                }
                else if (Equal(local.UraSupprEndDate.DiscontinueDate,
                  local.InitializedDate.Date))
                {
                  // The original disb is no longer suppressed (3) so the 
                  // adjusted disb should be released.
                  local.HighestSupprEndDate.Date = local.InitializedDate.Date;
                }
              }
              else
              {
                ExitState = "FN0000_ORIGINAL_DISB_NF";

                break;
              }
            }
            else
            {
              ExitState = "FN0000_DISB_CREDIT_NF_FOR_ADJUST";

              break;
            }
          }
          else
          {
            // ****  Get the process date from the credit disbursement  ****
            local.UraSupprEndDate.DiscontinueDate =
              AddDays(entities.Credit.ProcessDate,
              import.UraSuppressionLength.LastUsedNumber);
          }

          if (Lt(local.HighestSupprEndDate.Date,
            local.UraSupprEndDate.DiscontinueDate))
          {
            local.ForUpdate.SuppressionReason = "X";
            local.HighestSupprEndDate.Date =
              local.UraSupprEndDate.DiscontinueDate;
          }
        }

        // *****  changes for WR 040796
        // ---------- Court Order Level Check  ----------
        if (!ReadDisbursementTransaction2())
        {
          ExitState = "FN0000_DISB_TRANS_NF_RB";

          return;
        }

        UseFnCheckForCourtOrderSuppr();

        if (Lt(local.HighestSupprEndDate.Date,
          local.CourtOrderSupprEndDate.DiscontinueDate))
        {
          local.ForUpdate.SuppressionReason = "O";
          local.HighestSupprEndDate.Date =
            local.CourtOrderSupprEndDate.DiscontinueDate;
        }

        // *****  changes for WR 040796
      }

      // ---------- System Suppression Check - Y (Deceased) and Z (No active 
      // address)  ----------
      if (Lt(import.ProgramProcessingInfo.ProcessDate,
        local.HighestSupprEndDate.Date))
      {
        // This disbursement will be suppressed for a reason other than one of 
        // the two system suppression reasons.
        if (AsChar(entities.DisbursementStatusHistory.SuppressionReason) == 'Y'
          || AsChar(entities.DisbursementStatusHistory.SuppressionReason) == 'Z'
          )
        {
          // --Determine if any other of the ARs disbursements are suppressed 
          // for this system suppression reason.
          if (ReadDisbursementTransaction1())
          {
            // --Other disbursements for the AR are still suppressed for this 
            // system suppression reason.
            //   Do not end date the suppression rule.
          }
          else
          {
            // --End date the system suppression rule for the ar.
            if (ReadDisbSuppressionStatusHistory())
            {
              try
              {
                UpdateDisbSuppressionStatusHistory();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_DISB_SUPP_STAT_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_DISB_SUPP_STAT_PV";

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
              // -- Continue.
            }
          }
        }
      }
      else
      {
        UseFnCheckForSystemSuppression();

        if (Lt(local.HighestSupprEndDate.Date,
          local.SystemSupprEndDate.DiscontinueDate))
        {
          local.ForUpdate.SuppressionReason = local.SystemSupprEndDate.Type1;
          local.ForUpdate.ReasonText = local.SystemSupprEndDate.ReasonText ?? ""
            ;
          local.HighestSupprEndDate.Date =
            local.SystemSupprEndDate.DiscontinueDate;

          // --Determine if this is a new suppression.
          //   If so, it will be written to the Daily Suppression report and an 
          // event will be created.
          local.NewSystemSuppression.Flag = "N";

          if (Equal(entities.DisbursementStatusHistory.EffectiveDate,
            import.ProgramProcessingInfo.ProcessDate))
          {
            // --The suppression was created on todays processing date, so it is
            // a new system suppression.
            local.NewSystemSuppression.Flag = "Y";
          }

          if (AsChar(entities.DisbursementStatusHistory.SuppressionReason) != AsChar
            (local.SystemSupprEndDate.Type1))
          {
            // --The suppression type changed to a system suppression, so it is 
            // considered a new system suppression.
            local.NewSystemSuppression.Flag = "Y";
          }

          if (Equal(Date(entities.DisbursementStatusHistory.CreatedTimestamp),
            import.ProgramProcessingInfo.ProcessDate) || Equal
            (Date(entities.DisbursementStatusHistory.CreatedTimestamp),
            Now().Date))
          {
            // --The suppression was created on todays (processing or current) 
            // date, so it is a new system suppression.  (This is mainly to
            // support testing.  Production suppressions would have been flagged
            // as new in the check for effective_date = processing_date above).
            local.NewSystemSuppression.Flag = "Y";
          }

          if (AsChar(local.NewSystemSuppression.Flag) == 'Y')
          {
            // --Raise the system suppression event.
            UseFnRaiseSystemSupprEvent();
          }

          // --Log to reports and create tab delimited file.
          UseFnB651PrintErrorLine();
        }
      }

      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        if (entities.Debit.Amount < 0)
        {
          local.SignField.Text2 = " -";
        }
        else
        {
          local.SignField.Text2 = "";
        }

        local.UnformattedAmt.Number112 = entities.Debit.Amount;
        UseCabFormat112AmtFieldTo8();
        UseCabFormatDate3();
      }

      if (!Lt(import.ProgramProcessingInfo.ProcessDate,
        local.HighestSupprEndDate.Date))
      {
        // ****  The suppression has discontinued so set the status to released
        // ****
        try
        {
          UpdateDisbursementStatusHistory2();

          try
          {
            CreateDisbursementStatusHistory();
            ++export.SupprCounts.DtlSupprReleased.Count;
            local.MonthlyObligeeSummary.Assign(local.Initialized);
            local.MonthlyObligeeSummary.Year =
              Year(entities.Debit.CollectionDate);
            local.MonthlyObligeeSummary.Month =
              Month(entities.Debit.CollectionDate);
            local.MonthlyObligeeSummary.DisbursementsSuppressed =
              -entities.Debit.Amount;

            if (entities.DisbursementType.SystemGeneratedIdentifier == 71)
            {
              local.MonthlyObligeeSummary.PassthruAmount =
                entities.Debit.Amount;
            }
            else if (AsChar(entities.Debit.ExcessUraInd) == 'Y')
            {
              local.MonthlyObligeeSummary.TotExcessUraAmt =
                entities.Debit.Amount;
            }
            else
            {
              local.MonthlyObligeeSummary.CollectionsDisbursedToAr =
                entities.Debit.Amount;
            }

            UseFnUpdateObligeeMonthlyTotals();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DISB_STAT_HIST_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_STATUS_HISTORY_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (AsChar(import.TestDisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "AR " + entities.CsePerson.Number + "  Disb ID " +
            NumberToString(entities.Debit.SystemGeneratedIdentifier, 7, 9) + local
            .SignField.Text2 + local.FormattedAmt.Text9 + "  Release dt " + local
            .FormattedDate.Text10 + "  " + entities
            .DisbursementStatusHistory.SuppressionReason + "  RELEASED";
        }
      }
      else if (Equal(local.HighestSupprEndDate.Date,
        entities.DisbursementStatusHistory.DiscontinueDate))
      {
        // ****  No changes needed  ****
        ++export.SupprCounts.DtlSupprNoChange.Count;

        if (AsChar(import.TestDisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "AR " + entities.CsePerson.Number + "  Disb ID " +
            NumberToString(entities.Debit.SystemGeneratedIdentifier, 7, 9) + local
            .SignField.Text2 + local.FormattedAmt.Text9 + "  Release dt " + local
            .FormattedDate.Text10 + "  " + entities
            .DisbursementStatusHistory.SuppressionReason + "  NO CHANGE";
        }

        if (AsChar(entities.DisbursementStatusHistory.SuppressionReason) != AsChar
          (local.ForUpdate.SuppressionReason))
        {
          try
          {
            UpdateDisbursementStatusHistory3();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DISB_STAT_HIST_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else
      {
        if (Lt(entities.DisbursementStatusHistory.DiscontinueDate,
          local.HighestSupprEndDate.Date))
        {
          ++export.SupprCounts.DtlSupprExtended.Count;
        }
        else
        {
          ++export.SupprCounts.DtlSupprReduced.Count;
        }

        if (AsChar(import.TestDisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "AR " + entities.CsePerson.Number + "  Disb ID " +
            NumberToString(entities.Debit.SystemGeneratedIdentifier, 7, 9) + local
            .SignField.Text2 + local.FormattedAmt.Text9 + "  " + entities
            .DisbursementStatusHistory.SuppressionReason + "  changed from " + NumberToString
            (DateToInt(entities.DisbursementStatusHistory.DiscontinueDate), 8, 8)
            + " to " + NumberToString
            (DateToInt(local.HighestSupprEndDate.Date), 8, 8);
        }

        // **** The suppression has been extended so update the discontinue 
        // date. ****
        try
        {
          UpdateDisbursementStatusHistory1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_STAT_HIST_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        UseCabControlReport();
      }

      if (!Lt(import.ProgramProcessingInfo.ProcessDate,
        local.HighestSupprEndDate.Date))
      {
        // **** Don't count the disbursements being released. ****
      }
      else
      {
        switch(AsChar(local.ForUpdate.SuppressionReason))
        {
          case 'X':
            ++export.SupprCounts.DtlXSupprCnt.Count;
            export.SupprCounts.DtlXSupprAmt.TotalCurrency += entities.Debit.
              Amount;

            break;
          case 'A':
            ++export.SupprCounts.DtlASupprCnt.Count;
            export.SupprCounts.DtlASupprAmt.TotalCurrency += entities.Debit.
              Amount;

            break;
          case 'P':
            ++export.SupprCounts.DtlPSupprCnt.Count;
            export.SupprCounts.DtlPSupprAmt.TotalCurrency += entities.Debit.
              Amount;

            break;
          case 'C':
            ++export.SupprCounts.DtlCSupprCnt.Count;
            export.SupprCounts.DtlCSupprAmt.TotalCurrency += entities.Debit.
              Amount;

            // *****  changes for WR 040796
            break;
          case 'O':
            ++local.CourtOrderSupprCnt.Count;
            local.CourtOrderSupprAmt.TotalCurrency += entities.Debit.Amount;

            // *****  changes for WR 040796
            break;
          case 'Y':
            ++export.SupprCounts.DtlYSupprCnt.Count;
            export.SupprCounts.DtlYSupprAmt.TotalCurrency += entities.Debit.
              Amount;

            break;
          case 'Z':
            ++export.SupprCounts.DtlZSupprCnt.Count;
            export.SupprCounts.DtlZSupprAmt.TotalCurrency += entities.Debit.
              Amount;

            break;
          default:
            break;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Error in Suppr Release AB for AR " + entities
        .CsePerson.Number + "  Disb ID " + NumberToString
        (entities.Debit.SystemGeneratedIdentifier, 7, 9) + "  " + local
        .ExitStateMessag.Message;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      "End of Duplicate Payment Disbursements Reporting.";
    UseCabControlReport();
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();
    local.EabReportSend.RptDetail =
      NumberToString(local.CfdsoSupprCnt.Count, 10, 6) + " disbursements for " +
      NumberToString((long)local.CfdsoSupprAmt.TotalCurrency, 9, 7) + "." + NumberToString
      ((long)(local.CfdsoSupprAmt.TotalCurrency * 100), 14, 2) + " are FDSO suppressed (and may have other suppr rules in effect as well.)";
      
    UseCabControlReport();
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();
    export.SupprCounts.DtlCSupprCnt.Count -= local.CfdsoSupprCnt.Count;
    export.SupprCounts.DtlCSupprAmt.TotalCurrency -= local.CfdsoSupprAmt.
      TotalCurrency;

    // *****  changes for WR 040796
    local.EabReportSend.RptDetail =
      NumberToString(local.CourtOrderSupprCnt.Count, 10, 6) + " disbursements for " +
      NumberToString((long)local.CourtOrderSupprAmt.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(local.CourtOrderSupprAmt.TotalCurrency * 100), 14, 2) + " are Court Order suppressed.  ***  ";
      
    UseCabControlReport();
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    // *****  changes for WR 040796
  }

  private static void MoveCsePersonAccount(CsePersonAccount source,
    CsePersonAccount target)
  {
    target.Type1 = source.Type1;
    target.CspNumber = source.CspNumber;
  }

  private static void MoveDisbursementStatusHistory(
    DisbursementStatusHistory source, DisbursementStatusHistory target)
  {
    target.ReasonText = source.ReasonText;
    target.SuppressionReason = source.SuppressionReason;
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.CollectionDate = source.CollectionDate;
    target.InterstateInd = source.InterstateInd;
    target.ExcessUraInd = source.ExcessUraInd;
    target.CollectionProcessDate = source.CollectionProcessDate;
  }

  private static void MoveMonthlyObligeeSummary(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.DisbursementsSuppressed = source.DisbursementsSuppressed;
    target.PassthruAmount = source.PassthruAmount;
    target.CollectionsDisbursedToAr = source.CollectionsDisbursedToAr;
    target.TotExcessUraAmt = source.TotExcessUraAmt;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFormat112AmtFieldTo8()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.UnformattedAmt.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedAmt.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormatDate1()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.Unformatted.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedDisbursementDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDate2()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.Unformatted.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedCollectionDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDate3()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.HighestSupprEndDate.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateMessag.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateMessag.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnB651PrintErrorLine()
  {
    var useImport = new FnB651PrintErrorLine.Import();
    var useExport = new FnB651PrintErrorLine.Export();

    useImport.Ar.Number = entities.CsePerson.Number;
    useImport.DisbursementTransaction.Assign(entities.Debit);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.NewSystemSuppression.Flag = local.NewSystemSuppression.Flag;
    MoveDisbursementStatusHistory(local.ForUpdate,
      useImport.DisbursementStatusHistory);

    Call(FnB651PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnCheckForAutomaticDisbSupp()
  {
    var useImport = new FnCheckForAutomaticDisbSupp.Import();
    var useExport = new FnCheckForAutomaticDisbSupp.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(FnCheckForAutomaticDisbSupp.Execute, useImport, useExport);

    local.AutomaticSupprEndDate.DiscontinueDate =
      useExport.DisbSuppressionStatusHistory.DiscontinueDate;
  }

  private void UseFnCheckForCollDisbSup()
  {
    var useImport = new FnCheckForCollDisbSup.Import();
    var useExport = new FnCheckForCollDisbSup.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.DisbursementTransaction.CollectionDate =
      entities.Debit.CollectionDate;
    useImport.CollectionType.SequentialIdentifier =
      entities.CollectionType.SequentialIdentifier;
    useImport.HardcodeCollectionType.Type1 =
      import.DisbSuppressionStatusHistory.Type1;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(FnCheckForCollDisbSup.Execute, useImport, useExport);

    local.CollectionSupprEndDate.DiscontinueDate =
      useExport.DisbSuppressionStatusHistory.DiscontinueDate;
  }

  private void UseFnCheckForCourtOrderSuppr()
  {
    var useImport = new FnCheckForCourtOrderSuppr.Import();
    var useExport = new FnCheckForCourtOrderSuppr.Export();

    useImport.Per.Assign(entities.DisbursementTransaction);
    useImport.PerObligee.Assign(entities.CsePersonAccount);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.TestDisplayInd.Flag = import.TestDisplayInd.Flag;

    Call(FnCheckForCourtOrderSuppr.Execute, useImport, useExport);

    MoveDisbursementTransaction(useImport.Per, entities.DisbursementTransaction);
      
    entities.CsePersonAccount.Type1 = useImport.PerObligee.Type1;
    local.CourtOrderSupprEndDate.DiscontinueDate =
      useExport.DisbSuppressionStatusHistory.DiscontinueDate;
  }

  private void UseFnCheckForPerDisbSup()
  {
    var useImport = new FnCheckForPerDisbSup.Import();
    var useExport = new FnCheckForPerDisbSup.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(FnCheckForPerDisbSup.Execute, useImport, useExport);

    local.PersonSupprEndDate.DiscontinueDate =
      useExport.DisbSuppressionStatusHistory.DiscontinueDate;
  }

  private void UseFnCheckForSystemSuppression()
  {
    var useImport = new FnCheckForSystemSuppression.Import();
    var useExport = new FnCheckForSystemSuppression.Export();

    useImport.Ar.Number = entities.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Persistent.Assign(entities.Debit);

    Call(FnCheckForSystemSuppression.Execute, useImport, useExport);

    local.SystemSupprEndDate.Assign(useExport.DisbSuppressionStatusHistory);
  }

  private void UseFnRaiseSystemSupprEvent()
  {
    var useImport = new FnRaiseSystemSupprEvent.Import();
    var useExport = new FnRaiseSystemSupprEvent.Export();

    useImport.Ar.Number = entities.CsePerson.Number;
    useImport.DisbursementTransaction.Assign(entities.Debit);
    MoveDisbursementStatusHistory(local.ForUpdate,
      useImport.DisbursementStatusHistory);

    Call(FnRaiseSystemSupprEvent.Execute, useImport, useExport);
  }

  private void UseFnUpdateObligeeMonthlyTotals()
  {
    var useImport = new FnUpdateObligeeMonthlyTotals.Import();
    var useExport = new FnUpdateObligeeMonthlyTotals.Export();

    useImport.Per.Assign(entities.CsePersonAccount);
    MoveMonthlyObligeeSummary(local.MonthlyObligeeSummary,
      useImport.MonthlyObligeeSummary);

    Call(FnUpdateObligeeMonthlyTotals.Execute, useImport, useExport);

    MoveCsePersonAccount(useImport.Per, entities.CsePersonAccount);
  }

  private void CreateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);

    var dbsGeneratedId = import.PerReleased.SystemGeneratedIdentifier;
    var dtrGeneratedId = entities.Debit.SystemGeneratedIdentifier;
    var cspNumber = entities.Debit.CspNumber;
    var cpaType = entities.Debit.CpaType;
    var systemGeneratedIdentifier =
      entities.DisbursementStatusHistory.SystemGeneratedIdentifier + 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = import.MaxDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.CurrentTimestamp.Timestamp;

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.DisbursementStatusHistory.Populated = false;
    Update("CreateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "dbsGeneratedId", dbsGeneratedId);
        db.SetInt32(command, "dtrGeneratedId", dtrGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "disbStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
        db.SetNullableString(command, "suppressionReason", "");
      });

    entities.DisbursementStatusHistory.DbsGeneratedId = dbsGeneratedId;
    entities.DisbursementStatusHistory.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementStatusHistory.CspNumber = cspNumber;
    entities.DisbursementStatusHistory.CpaType = cpaType;
    entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.CreatedBy = createdBy;
    entities.DisbursementStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbursementStatusHistory.ReasonText = "";
    entities.DisbursementStatusHistory.SuppressionReason = "";
    entities.DisbursementStatusHistory.Populated = true;
  }

  private bool ReadCollectionTypeCollectionDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.CollectionType.Populated = false;
    entities.Collection.Populated = false;
    entities.Credit.Populated = false;

    return Read("ReadCollectionTypeCollectionDisbursementTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId", entities.Debit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debit.CpaType);
        db.SetString(command, "cspNumber", entities.Debit.CspNumber);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Credit.ColId = db.GetNullableInt32(reader, 1);
        entities.Collection.AppliedToCode = db.GetString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Credit.CrtId = db.GetNullableInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Credit.CstId = db.GetNullableInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Credit.CrvId = db.GetNullableInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Credit.CrdId = db.GetNullableInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Credit.ObgId = db.GetNullableInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Credit.CspNumberDisb = db.GetNullableString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Credit.OtrId = db.GetNullableInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Credit.OtyId = db.GetNullableInt32(reader, 12);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 13);
        entities.Collection.AppliedToFuture = db.GetString(reader, 14);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 15);
        entities.Credit.CpaType = db.GetString(reader, 16);
        entities.Credit.CspNumber = db.GetString(reader, 17);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 18);
        entities.Credit.ProcessDate = db.GetNullableDate(reader, 19);
        entities.CollectionType.Populated = true;
        entities.Collection.Populated = true;
        entities.Credit.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.Credit.CpaTypeDisb);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.Credit.OtrTypeDisb);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otrGeneratedId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
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
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetString(
          command, "type",
          entities.DisbursementStatusHistory.SuppressionReason ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbursementStatusHistoryDisbursementStatus()
  {
    System.Diagnostics.Debug.Assert(entities.OriginalDb.Populated);
    entities.OriginalDisbursementStatusHistory.Populated = false;
    entities.OriginalDisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatusHistoryDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.OriginalDb.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.OriginalDb.CspNumber);
        db.SetString(command, "cpaType", entities.OriginalDb.CpaType);
      },
      (db, reader) =>
      {
        entities.OriginalDisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.OriginalDisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OriginalDisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.OriginalDisbursementStatusHistory.CspNumber =
          db.GetString(reader, 2);
        entities.OriginalDisbursementStatusHistory.CpaType =
          db.GetString(reader, 3);
        entities.OriginalDisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.OriginalDisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OriginalDisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 6);
        entities.OriginalDisbursementStatusHistory.Populated = true;
        entities.OriginalDisbursementStatus.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.OriginalDisbursementStatusHistory.CpaType);
      });
  }

  private IEnumerable<bool>
    ReadDisbursementStatusHistoryDisbursementTransaction()
  {
    entities.CsePerson.Populated = false;
    entities.CsePersonAccount.Populated = false;
    entities.Debit.Populated = false;
    entities.DisbursementType.Populated = false;
    entities.DisbursementStatusHistory.Populated = false;

    return ReadEach("ReadDisbursementStatusHistoryDisbursementTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbsGeneratedId",
          import.PerSuppressed.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.Debit.CspNumber = db.GetString(reader, 2);
        entities.CsePerson.Number = db.GetString(reader, 2);
        entities.CsePerson.Number = db.GetString(reader, 2);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 2);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 2);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.Debit.CpaType = db.GetString(reader, 3);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 3);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.CreatedBy = db.GetString(reader, 7);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 10);
        entities.Debit.Type1 = db.GetString(reader, 11);
        entities.Debit.Amount = db.GetDecimal(reader, 12);
        entities.Debit.DisbursementDate = db.GetNullableDate(reader, 13);
        entities.Debit.CollectionDate = db.GetNullableDate(reader, 14);
        entities.Debit.DbtGeneratedId = db.GetNullableInt32(reader, 15);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.Debit.InterstateInd = db.GetNullableString(reader, 16);
        entities.Debit.ReferenceNumber = db.GetNullableString(reader, 17);
        entities.Debit.ExcessUraInd = db.GetNullableString(reader, 18);
        entities.CsePerson.Populated = true;
        entities.CsePersonAccount.Populated = true;
        entities.Debit.Populated = true;
        entities.DisbursementType.Populated = true;
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<DisbursementTransaction>("Type1", entities.Debit.Type1);

        return true;
      });
  }

  private bool ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.OtherDebit.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetInt32(
          command, "disbTranId", entities.Debit.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "suppressionReason",
          entities.DisbursementStatusHistory.SuppressionReason ?? "");
        db.SetInt32(
          command, "dbsGeneratedId",
          import.PerSuppressed.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OtherDebit.CpaType = db.GetString(reader, 0);
        entities.OtherDebit.CspNumber = db.GetString(reader, 1);
        entities.OtherDebit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.OtherDebit.Type1 = db.GetString(reader, 3);
        entities.OtherDebit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.OtherDebit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.OtherDebit.Type1);
      });
  }

  private bool ReadDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId", entities.Debit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debit.CpaType);
        db.SetString(command, "cspNumber", entities.Debit.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransaction.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 9);
        entities.DisbursementTransaction.CollectionProcessDate =
          db.GetDate(reader, 10);
        entities.DisbursementTransaction.InterstateInd =
          db.GetNullableString(reader, 11);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 12);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 13);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
      });
  }

  private bool ReadDisbursementTransaction3()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.OriginalCr.Populated = false;

    return Read("ReadDisbursementTransaction3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId", entities.Collection.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "otyId", entities.Collection.OtyId);
        db.SetNullableInt32(command, "obgId", entities.Collection.ObgId);
        db.SetNullableString(
          command, "cspNumberDisb", entities.Collection.CspNumber);
        db.
          SetNullableString(command, "cpaTypeDisb", entities.Collection.CpaType);
          
        db.SetNullableInt32(command, "otrId", entities.Collection.OtrId);
        db.
          SetNullableString(command, "otrTypeDisb", entities.Collection.OtrType);
          
        db.SetNullableInt32(command, "crtId", entities.Collection.CrtType);
        db.SetNullableInt32(command, "cstId", entities.Collection.CstId);
        db.SetNullableInt32(command, "crvId", entities.Collection.CrvId);
        db.SetNullableInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(
          command, "disbTranId", entities.Credit.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OriginalCr.CpaType = db.GetString(reader, 0);
        entities.OriginalCr.CspNumber = db.GetString(reader, 1);
        entities.OriginalCr.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.OriginalCr.OtyId = db.GetNullableInt32(reader, 3);
        entities.OriginalCr.OtrTypeDisb = db.GetNullableString(reader, 4);
        entities.OriginalCr.OtrId = db.GetNullableInt32(reader, 5);
        entities.OriginalCr.CpaTypeDisb = db.GetNullableString(reader, 6);
        entities.OriginalCr.CspNumberDisb = db.GetNullableString(reader, 7);
        entities.OriginalCr.ObgId = db.GetNullableInt32(reader, 8);
        entities.OriginalCr.CrdId = db.GetNullableInt32(reader, 9);
        entities.OriginalCr.CrvId = db.GetNullableInt32(reader, 10);
        entities.OriginalCr.CstId = db.GetNullableInt32(reader, 11);
        entities.OriginalCr.CrtId = db.GetNullableInt32(reader, 12);
        entities.OriginalCr.ColId = db.GetNullableInt32(reader, 13);
        entities.OriginalCr.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.OriginalCr.CpaType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.OriginalCr.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.OriginalCr.CpaTypeDisb);
      });
  }

  private bool ReadDisbursementTransaction4()
  {
    System.Diagnostics.Debug.Assert(entities.OriginalCr.Populated);
    entities.OriginalDb.Populated = false;

    return Read("ReadDisbursementTransaction4",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.OriginalCr.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.OriginalCr.CpaType);
        db.SetString(command, "cspPNumber", entities.OriginalCr.CspNumber);
      },
      (db, reader) =>
      {
        entities.OriginalDb.CpaType = db.GetString(reader, 0);
        entities.OriginalDb.CspNumber = db.GetString(reader, 1);
        entities.OriginalDb.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.OriginalDb.DbtGeneratedId = db.GetNullableInt32(reader, 3);
        entities.OriginalDb.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.OriginalDb.CpaType);
      });
  }

  private void UpdateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);

    var discontinueDate = import.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("UpdateDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.LastUpdatedBy = lastUpdatedBy;
    entities.DisbSuppressionStatusHistory.LastUpdatedTmst = lastUpdatedTmst;
    entities.DisbSuppressionStatusHistory.Populated = true;
  }

  private void UpdateDisbursementStatusHistory1()
  {
    System.Diagnostics.Debug.
      Assert(entities.DisbursementStatusHistory.Populated);

    var discontinueDate = local.HighestSupprEndDate.Date;
    var suppressionReason = local.ForUpdate.SuppressionReason ?? "";

    entities.DisbursementStatusHistory.Populated = false;
    Update("UpdateDisbursementStatusHistory1",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "suppressionReason", suppressionReason);
        db.SetInt32(
          command, "dbsGeneratedId",
          entities.DisbursementStatusHistory.DbsGeneratedId);
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementStatusHistory.DtrGeneratedId);
        db.SetString(
          command, "cspNumber", entities.DisbursementStatusHistory.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementStatusHistory.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.SuppressionReason = suppressionReason;
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void UpdateDisbursementStatusHistory2()
  {
    System.Diagnostics.Debug.
      Assert(entities.DisbursementStatusHistory.Populated);

    var discontinueDate = import.ProgramProcessingInfo.ProcessDate;
    var reasonText = "PROCESSED";

    entities.DisbursementStatusHistory.Populated = false;
    Update("UpdateDisbursementStatusHistory2",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetInt32(
          command, "dbsGeneratedId",
          entities.DisbursementStatusHistory.DbsGeneratedId);
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementStatusHistory.DtrGeneratedId);
        db.SetString(
          command, "cspNumber", entities.DisbursementStatusHistory.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementStatusHistory.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.ReasonText = reasonText;
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void UpdateDisbursementStatusHistory3()
  {
    System.Diagnostics.Debug.
      Assert(entities.DisbursementStatusHistory.Populated);

    var suppressionReason = local.ForUpdate.SuppressionReason ?? "";

    entities.DisbursementStatusHistory.Populated = false;
    Update("UpdateDisbursementStatusHistory3",
      (db, command) =>
      {
        db.SetNullableString(command, "suppressionReason", suppressionReason);
        db.SetInt32(
          command, "dbsGeneratedId",
          entities.DisbursementStatusHistory.DbsGeneratedId);
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementStatusHistory.DtrGeneratedId);
        db.SetString(
          command, "cspNumber", entities.DisbursementStatusHistory.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementStatusHistory.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbursementStatusHistory.SuppressionReason = suppressionReason;
    entities.DisbursementStatusHistory.Populated = true;
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
    /// A value of PerSuppressed.
    /// </summary>
    [JsonPropertyName("perSuppressed")]
    public DisbursementStatus PerSuppressed
    {
      get => perSuppressed ??= new();
      set => perSuppressed = value;
    }

    /// <summary>
    /// A value of PerReleased.
    /// </summary>
    [JsonPropertyName("perReleased")]
    public DisbursementStatus PerReleased
    {
      get => perReleased ??= new();
      set => perReleased = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of HardcodedPerson.
    /// </summary>
    [JsonPropertyName("hardcodedPerson")]
    public DisbSuppressionStatusHistory HardcodedPerson
    {
      get => hardcodedPerson ??= new();
      set => hardcodedPerson = value;
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
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    /// <summary>
    /// A value of TestFirst.
    /// </summary>
    [JsonPropertyName("testFirst")]
    public CsePerson TestFirst
    {
      get => testFirst ??= new();
      set => testFirst = value;
    }

    /// <summary>
    /// A value of TestLast.
    /// </summary>
    [JsonPropertyName("testLast")]
    public CsePerson TestLast
    {
      get => testLast ??= new();
      set => testLast = value;
    }

    /// <summary>
    /// A value of UraSuppressionLength.
    /// </summary>
    [JsonPropertyName("uraSuppressionLength")]
    public ControlTable UraSuppressionLength
    {
      get => uraSuppressionLength ??= new();
      set => uraSuppressionLength = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private DisbursementStatus perSuppressed;
    private DisbursementStatus perReleased;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DisbSuppressionStatusHistory hardcodedPerson;
    private ProgramProcessingInfo programProcessingInfo;
    private Common testDisplayInd;
    private CsePerson testFirst;
    private CsePerson testLast;
    private ControlTable uraSuppressionLength;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A SupprCountsGroup group.</summary>
    [Serializable]
    public class SupprCountsGroup
    {
      /// <summary>
      /// A value of DtlSupprRead.
      /// </summary>
      [JsonPropertyName("dtlSupprRead")]
      public Common DtlSupprRead
      {
        get => dtlSupprRead ??= new();
        set => dtlSupprRead = value;
      }

      /// <summary>
      /// A value of DtlSupprReleased.
      /// </summary>
      [JsonPropertyName("dtlSupprReleased")]
      public Common DtlSupprReleased
      {
        get => dtlSupprReleased ??= new();
        set => dtlSupprReleased = value;
      }

      /// <summary>
      /// A value of DtlSupprExtended.
      /// </summary>
      [JsonPropertyName("dtlSupprExtended")]
      public Common DtlSupprExtended
      {
        get => dtlSupprExtended ??= new();
        set => dtlSupprExtended = value;
      }

      /// <summary>
      /// A value of DtlSupprReduced.
      /// </summary>
      [JsonPropertyName("dtlSupprReduced")]
      public Common DtlSupprReduced
      {
        get => dtlSupprReduced ??= new();
        set => dtlSupprReduced = value;
      }

      /// <summary>
      /// A value of DtlSupprNoChange.
      /// </summary>
      [JsonPropertyName("dtlSupprNoChange")]
      public Common DtlSupprNoChange
      {
        get => dtlSupprNoChange ??= new();
        set => dtlSupprNoChange = value;
      }

      /// <summary>
      /// A value of DtlPSupprCnt.
      /// </summary>
      [JsonPropertyName("dtlPSupprCnt")]
      public Common DtlPSupprCnt
      {
        get => dtlPSupprCnt ??= new();
        set => dtlPSupprCnt = value;
      }

      /// <summary>
      /// A value of DtlCSupprCnt.
      /// </summary>
      [JsonPropertyName("dtlCSupprCnt")]
      public Common DtlCSupprCnt
      {
        get => dtlCSupprCnt ??= new();
        set => dtlCSupprCnt = value;
      }

      /// <summary>
      /// A value of DtlASupprCnt.
      /// </summary>
      [JsonPropertyName("dtlASupprCnt")]
      public Common DtlASupprCnt
      {
        get => dtlASupprCnt ??= new();
        set => dtlASupprCnt = value;
      }

      /// <summary>
      /// A value of DtlXSupprCnt.
      /// </summary>
      [JsonPropertyName("dtlXSupprCnt")]
      public Common DtlXSupprCnt
      {
        get => dtlXSupprCnt ??= new();
        set => dtlXSupprCnt = value;
      }

      /// <summary>
      /// A value of DtlDSupprCnt.
      /// </summary>
      [JsonPropertyName("dtlDSupprCnt")]
      public Common DtlDSupprCnt
      {
        get => dtlDSupprCnt ??= new();
        set => dtlDSupprCnt = value;
      }

      /// <summary>
      /// A value of DtlPSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlPSupprAmt")]
      public Common DtlPSupprAmt
      {
        get => dtlPSupprAmt ??= new();
        set => dtlPSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlCSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlCSupprAmt")]
      public Common DtlCSupprAmt
      {
        get => dtlCSupprAmt ??= new();
        set => dtlCSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlASupprAmt.
      /// </summary>
      [JsonPropertyName("dtlASupprAmt")]
      public Common DtlASupprAmt
      {
        get => dtlASupprAmt ??= new();
        set => dtlASupprAmt = value;
      }

      /// <summary>
      /// A value of DtlXSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlXSupprAmt")]
      public Common DtlXSupprAmt
      {
        get => dtlXSupprAmt ??= new();
        set => dtlXSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlDSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlDSupprAmt")]
      public Common DtlDSupprAmt
      {
        get => dtlDSupprAmt ??= new();
        set => dtlDSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlYSupprCnt.
      /// </summary>
      [JsonPropertyName("dtlYSupprCnt")]
      public Common DtlYSupprCnt
      {
        get => dtlYSupprCnt ??= new();
        set => dtlYSupprCnt = value;
      }

      /// <summary>
      /// A value of DtlZSupprCnt.
      /// </summary>
      [JsonPropertyName("dtlZSupprCnt")]
      public Common DtlZSupprCnt
      {
        get => dtlZSupprCnt ??= new();
        set => dtlZSupprCnt = value;
      }

      /// <summary>
      /// A value of DtlYSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlYSupprAmt")]
      public Common DtlYSupprAmt
      {
        get => dtlYSupprAmt ??= new();
        set => dtlYSupprAmt = value;
      }

      /// <summary>
      /// A value of DtlZSupprAmt.
      /// </summary>
      [JsonPropertyName("dtlZSupprAmt")]
      public Common DtlZSupprAmt
      {
        get => dtlZSupprAmt ??= new();
        set => dtlZSupprAmt = value;
      }

      private Common dtlSupprRead;
      private Common dtlSupprReleased;
      private Common dtlSupprExtended;
      private Common dtlSupprReduced;
      private Common dtlSupprNoChange;
      private Common dtlPSupprCnt;
      private Common dtlCSupprCnt;
      private Common dtlASupprCnt;
      private Common dtlXSupprCnt;
      private Common dtlDSupprCnt;
      private Common dtlPSupprAmt;
      private Common dtlCSupprAmt;
      private Common dtlASupprAmt;
      private Common dtlXSupprAmt;
      private Common dtlDSupprAmt;
      private Common dtlYSupprCnt;
      private Common dtlZSupprCnt;
      private Common dtlYSupprAmt;
      private Common dtlZSupprAmt;
    }

    /// <summary>
    /// Gets a value of SupprCounts.
    /// </summary>
    [JsonPropertyName("supprCounts")]
    public SupprCountsGroup SupprCounts
    {
      get => supprCounts ?? (supprCounts = new());
      set => supprCounts = value;
    }

    private SupprCountsGroup supprCounts;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NewSystemSuppression.
    /// </summary>
    [JsonPropertyName("newSystemSuppression")]
    public Common NewSystemSuppression
    {
      get => newSystemSuppression ??= new();
      set => newSystemSuppression = value;
    }

    /// <summary>
    /// A value of SystemSupprEndDate.
    /// </summary>
    [JsonPropertyName("systemSupprEndDate")]
    public DisbSuppressionStatusHistory SystemSupprEndDate
    {
      get => systemSupprEndDate ??= new();
      set => systemSupprEndDate = value;
    }

    /// <summary>
    /// A value of CourtOrderSupprCnt.
    /// </summary>
    [JsonPropertyName("courtOrderSupprCnt")]
    public Common CourtOrderSupprCnt
    {
      get => courtOrderSupprCnt ??= new();
      set => courtOrderSupprCnt = value;
    }

    /// <summary>
    /// A value of CourtOrderSupprAmt.
    /// </summary>
    [JsonPropertyName("courtOrderSupprAmt")]
    public Common CourtOrderSupprAmt
    {
      get => courtOrderSupprAmt ??= new();
      set => courtOrderSupprAmt = value;
    }

    /// <summary>
    /// A value of CfdsoSupprCnt.
    /// </summary>
    [JsonPropertyName("cfdsoSupprCnt")]
    public Common CfdsoSupprCnt
    {
      get => cfdsoSupprCnt ??= new();
      set => cfdsoSupprCnt = value;
    }

    /// <summary>
    /// A value of CfdsoSupprAmt.
    /// </summary>
    [JsonPropertyName("cfdsoSupprAmt")]
    public Common CfdsoSupprAmt
    {
      get => cfdsoSupprAmt ??= new();
      set => cfdsoSupprAmt = value;
    }

    /// <summary>
    /// A value of SignField.
    /// </summary>
    [JsonPropertyName("signField")]
    public TextWorkArea SignField
    {
      get => signField ??= new();
      set => signField = value;
    }

    /// <summary>
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public DisbursementStatusHistory ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of FormattedDisbursementDate.
    /// </summary>
    [JsonPropertyName("formattedDisbursementDate")]
    public WorkArea FormattedDisbursementDate
    {
      get => formattedDisbursementDate ??= new();
      set => formattedDisbursementDate = value;
    }

    /// <summary>
    /// A value of FormattedCollectionDate.
    /// </summary>
    [JsonPropertyName("formattedCollectionDate")]
    public WorkArea FormattedCollectionDate
    {
      get => formattedCollectionDate ??= new();
      set => formattedCollectionDate = value;
    }

    /// <summary>
    /// A value of Unformatted.
    /// </summary>
    [JsonPropertyName("unformatted")]
    public DateWorkArea Unformatted
    {
      get => unformatted ??= new();
      set => unformatted = value;
    }

    /// <summary>
    /// A value of FormattedAmt.
    /// </summary>
    [JsonPropertyName("formattedAmt")]
    public WorkArea FormattedAmt
    {
      get => formattedAmt ??= new();
      set => formattedAmt = value;
    }

    /// <summary>
    /// A value of UnformattedAmt.
    /// </summary>
    [JsonPropertyName("unformattedAmt")]
    public NumericWorkSet UnformattedAmt
    {
      get => unformattedAmt ??= new();
      set => unformattedAmt = value;
    }

    /// <summary>
    /// A value of Fdso.
    /// </summary>
    [JsonPropertyName("fdso")]
    public Common Fdso
    {
      get => fdso ??= new();
      set => fdso = value;
    }

    /// <summary>
    /// A value of HighestSupprEndDate.
    /// </summary>
    [JsonPropertyName("highestSupprEndDate")]
    public DateWorkArea HighestSupprEndDate
    {
      get => highestSupprEndDate ??= new();
      set => highestSupprEndDate = value;
    }

    /// <summary>
    /// A value of PersonSupprEndDate.
    /// </summary>
    [JsonPropertyName("personSupprEndDate")]
    public DisbSuppressionStatusHistory PersonSupprEndDate
    {
      get => personSupprEndDate ??= new();
      set => personSupprEndDate = value;
    }

    /// <summary>
    /// A value of CollectionSupprEndDate.
    /// </summary>
    [JsonPropertyName("collectionSupprEndDate")]
    public DisbSuppressionStatusHistory CollectionSupprEndDate
    {
      get => collectionSupprEndDate ??= new();
      set => collectionSupprEndDate = value;
    }

    /// <summary>
    /// A value of AutomaticSupprEndDate.
    /// </summary>
    [JsonPropertyName("automaticSupprEndDate")]
    public DisbSuppressionStatusHistory AutomaticSupprEndDate
    {
      get => automaticSupprEndDate ??= new();
      set => automaticSupprEndDate = value;
    }

    /// <summary>
    /// A value of UraSupprEndDate.
    /// </summary>
    [JsonPropertyName("uraSupprEndDate")]
    public DisbSuppressionStatusHistory UraSupprEndDate
    {
      get => uraSupprEndDate ??= new();
      set => uraSupprEndDate = value;
    }

    /// <summary>
    /// A value of CourtOrderSupprEndDate.
    /// </summary>
    [JsonPropertyName("courtOrderSupprEndDate")]
    public DisbSuppressionStatusHistory CourtOrderSupprEndDate
    {
      get => courtOrderSupprEndDate ??= new();
      set => courtOrderSupprEndDate = value;
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
    /// A value of InitializedDate.
    /// </summary>
    [JsonPropertyName("initializedDate")]
    public DateWorkArea InitializedDate
    {
      get => initializedDate ??= new();
      set => initializedDate = value;
    }

    /// <summary>
    /// A value of ExitStateMessag.
    /// </summary>
    [JsonPropertyName("exitStateMessag")]
    public ExitStateWorkArea ExitStateMessag
    {
      get => exitStateMessag ??= new();
      set => exitStateMessag = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public MonthlyObligeeSummary Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    private Common newSystemSuppression;
    private DisbSuppressionStatusHistory systemSupprEndDate;
    private Common courtOrderSupprCnt;
    private Common courtOrderSupprAmt;
    private Common cfdsoSupprCnt;
    private Common cfdsoSupprAmt;
    private TextWorkArea signField;
    private WorkArea formattedDate;
    private DisbursementStatusHistory forUpdate;
    private WorkArea formattedDisbursementDate;
    private WorkArea formattedCollectionDate;
    private DateWorkArea unformatted;
    private WorkArea formattedAmt;
    private NumericWorkSet unformattedAmt;
    private Common fdso;
    private DateWorkArea highestSupprEndDate;
    private DisbSuppressionStatusHistory personSupprEndDate;
    private DisbSuppressionStatusHistory collectionSupprEndDate;
    private DisbSuppressionStatusHistory automaticSupprEndDate;
    private DisbSuppressionStatusHistory uraSupprEndDate;
    private DisbSuppressionStatusHistory courtOrderSupprEndDate;
    private DateWorkArea currentTimestamp;
    private DateWorkArea initializedDate;
    private ExitStateWorkArea exitStateMessag;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private MonthlyObligeeSummary initialized;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public DisbursementStatusHistory Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of OtherDebit.
    /// </summary>
    [JsonPropertyName("otherDebit")]
    public DisbursementTransaction OtherDebit
    {
      get => otherDebit ??= new();
      set => otherDebit = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of OriginalCr.
    /// </summary>
    [JsonPropertyName("originalCr")]
    public DisbursementTransaction OriginalCr
    {
      get => originalCr ??= new();
      set => originalCr = value;
    }

    /// <summary>
    /// A value of OriginalDisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("originalDisbursementTransactionRln")]
    public DisbursementTransactionRln OriginalDisbursementTransactionRln
    {
      get => originalDisbursementTransactionRln ??= new();
      set => originalDisbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of OriginalDb.
    /// </summary>
    [JsonPropertyName("originalDb")]
    public DisbursementTransaction OriginalDb
    {
      get => originalDb ??= new();
      set => originalDb = value;
    }

    /// <summary>
    /// A value of OriginalDisbursementType.
    /// </summary>
    [JsonPropertyName("originalDisbursementType")]
    public DisbursementType OriginalDisbursementType
    {
      get => originalDisbursementType ??= new();
      set => originalDisbursementType = value;
    }

    /// <summary>
    /// A value of OriginalDisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("originalDisbursementStatusHistory")]
    public DisbursementStatusHistory OriginalDisbursementStatusHistory
    {
      get => originalDisbursementStatusHistory ??= new();
      set => originalDisbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of OriginalDisbursementStatus.
    /// </summary>
    [JsonPropertyName("originalDisbursementStatus")]
    public DisbursementStatus OriginalDisbursementStatus
    {
      get => originalDisbursementStatus ??= new();
      set => originalDisbursementStatus = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DisbursementStatusHistory other;
    private DisbursementTransaction otherDebit;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private DisbursementTransaction debit;
    private DisbursementType disbursementType;
    private DisbursementStatusHistory disbursementStatusHistory;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction credit;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DisbursementTransaction originalCr;
    private DisbursementTransactionRln originalDisbursementTransactionRln;
    private DisbursementTransaction originalDb;
    private DisbursementType originalDisbursementType;
    private DisbursementStatusHistory originalDisbursementStatusHistory;
    private DisbursementStatus originalDisbursementStatus;
  }
#endregion
}
