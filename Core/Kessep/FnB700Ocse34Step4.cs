// Program: FN_B700_OCSE34_STEP_4, ID: 373315423, model: 746.
// Short name: SWE02989
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_OCSE34_STEP_4.
/// </summary>
[Serializable]
public partial class FnB700Ocse34Step4: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_OCSE34_STEP_4 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700Ocse34Step4(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700Ocse34Step4.
  /// </summary>
  public FnB700Ocse34Step4(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // -------------------------------------------------------------
    // 02/??/02  K Doshi	????????	Initial Development
    // 12/05/03  E Shirk	040134		Federally mandated OCSE34 report changes.
    // 06/15/04  CM Johnson	PR207423	Modifications for OCSE34 report.
    // 12/03/07  GVandy	CQ295		Federally mandated changes to OCSE34 report.
    // 01/06/09  GVandy	CQ486,		1) Enhance audit trail to determine why part 1 
    // and part 2 of the
    // 			CQ971		   OCSE34 report do not balance.
    // 					2) Re-write to improve performance
    // 					3) Correct a multitude of balancing issues
    // 10/14/12  GVandy			Emergency fix to expand foreign group view size
    // -----------------------------------------------------------------------------------------------------
    // -- Initialize process variables.
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ForCreate.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreate.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.TbdLocalCollAppliedInd.Flag = "N";
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.EabFileHandling.Action = "WRITE";

    // ???  last business day timestamp is no longer needed...
    // -- Build the last business day beginning timestamp.
    UseFnBuildTimestampFrmDateTime();

    // Checkpoint layout is as follows:
    // Position  1 - 2   Restart Step (this cab is step 04)
    // Position  3       Blank
    // Position  4       Which section in this step to restart (from 1 to 5)
    // Position  5       Blank
    // Position  6 - 15  CSE Person Number
    // Position  16      Blank
    // Position  17 - 25 Collection System Generated Id
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 2, "04"))
    {
      local.RestartInSection.Count =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 4, 1));
      UseFnB700BuildGvForRestart();
    }
    else
    {
      local.RestartInSection.Count = 1;
    }

    for(local.Counter.Count = local.RestartInSection.Count; local
      .Counter.Count <= 5; ++local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 1:
          // --------------------------------------------------------------------------------------------
          // ---   Read FEES, RECOVERIES and other collections which do not 
          // result in a disbursement.
          // ---   For the purposes of OCSE34, these collections are considered
          // "disbursed" when created.
          // ---   (Skip non-cash cash receipt and collection types)
          // --------------------------------------------------------------------------------------------
          if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
            (import.ProgramCheckpointRestart.RestartInfo, 1, 4, "04 1"))
          {
            local.RestartCsePerson.Number =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 6, 10);
            local.RestartCollection.SystemGeneratedIdentifier =
              (int)StringToNumber(Substring(
                import.ProgramCheckpointRestart.RestartInfo, 250, 17, 9));
          }
          else
          {
            local.RestartCsePerson.Number = "";
            local.RestartCollection.SystemGeneratedIdentifier = 0;
          }

          foreach(var item in ReadCollectionCsePersonCashReceiptCashReceiptDetail())
            
          {
            // @@@  Removed restart logic from the where predicate to correct a 
            // performance problem.
            if (Lt(entities.Ap.Number, local.RestartCsePerson.Number) || Equal
              (entities.Ap.Number, local.RestartCsePerson.Number) && entities
              .Collection.SystemGeneratedIdentifier <= local
              .RestartCollection.SystemGeneratedIdentifier)
            {
              continue;
            }

            // -- Add support for "unadjusted collections".  Also changes in the
            // above read each...
            if (!Lt(entities.Collection.CreatedTmst,
              import.RptPrdBegin.Timestamp) && !
              Lt(import.RptPrdEnd.Timestamp, entities.Collection.CreatedTmst) &&
              AsChar(entities.Collection.AdjustedInd) == 'N' && Lt
              (import.RptPrdEnd.Date, entities.Collection.UnadjustedDate) && !
              Lt(entities.Collection.PreviousCollectionAdjDate,
              import.RptPrdBegin.Date) && !
              Lt(import.RptPrdEnd.Date,
              entities.Collection.PreviousCollectionAdjDate))
            {
              // -- Skip collection.  It was created during the quarter and was 
              // adjusted at the end of the quarter, so the net during the
              // quarter was zero.
              continue;
            }

            MoveOcse157Verification2(local.NullOcse157Verification,
              local.ForCreate);

            if (Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
              (entities.Collection.ProgramAppliedTo, "FC") || Equal
              (entities.Collection.ProgramAppliedTo, "NF") || Equal
              (entities.Collection.ProgramAppliedTo, "NC"))
            {
              // --  Non FEES and RECOVERIES
              if (ReadCsePerson())
              {
                local.Supp.Number = entities.Supp.Number;
              }
              else
              {
                local.Supp.Number = "";
              }

              if (!Lt(entities.Collection.CollectionAdjustmentDt,
                import.RptPrdBegin.Date) && !
                Lt(import.RptPrdEnd.Date,
                entities.Collection.CollectionAdjustmentDt))
              {
                // -- Adjusted collections are subtracted from the line 4 or 7 
                // amounts.
                local.Common.TotalCurrency = -entities.Collection.Amount;
              }
              else
              {
                // -- Non-adjusted collections are added to the line 4 or 7 
                // amounts.
                local.Common.TotalCurrency = entities.Collection.Amount;
              }

              UseFnB700Ocse34MaintainLine2();

              if (IsEmpty(local.ForCreate.LineNumber))
              {
                // --  Log to error report
                local.EabReportSend.RptDetail =
                  "Unclassified collection - Step 4, Line 4/7, Section " + NumberToString
                  (local.Counter.Count, 15, 1) + ", AP # = " + entities
                  .Ap.Number + ", Collection Sys Gen Id = " + NumberToString
                  (entities.Collection.SystemGeneratedIdentifier, 7, 9) + ", OB Type = " +
                  NumberToString
                  (entities.ObligationType.SystemGeneratedIdentifier, 14, 2) + ", Amount =" +
                  NumberToString((long)(local.Common.TotalCurrency * 100), 15);

                if (local.Common.TotalCurrency < 0)
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "-";
                }

                UseCabErrorReport();

                continue;
              }

              local.ForCreate.Comment = "B700 Step 4 (1a)";
            }
            else
            {
              // --  FEES and RECOVERIES
              if (!Lt(entities.Collection.CollectionAdjustmentDt,
                import.RptPrdBegin.Date) && !
                Lt(import.RptPrdEnd.Date,
                entities.Collection.CollectionAdjustmentDt))
              {
                // -- Adjusted fees/recoveries are added back to the line 2 
                // amounts.
                local.Common.TotalCurrency = entities.Collection.Amount;
              }
              else
              {
                // -- Non-adjusted fees/recoveries are subtracted from the line 
                // 2 amounts.
                local.Common.TotalCurrency = -entities.Collection.Amount;
              }

              if (!ReadCashReceiptDetail())
              {
                continue;
              }

              UseFnB700MaintainLine2Totals();

              if (IsEmpty(local.ForCreate.LineNumber))
              {
                // --  Log to error report
                local.EabReportSend.RptDetail =
                  "Unclassified collection - Step 4, Line 2, Section " + NumberToString
                  (local.Counter.Count, 15, 1) + ", AP # = " + entities
                  .Ap.Number + ", Collection Sys Gen Id = " + NumberToString
                  (entities.Collection.SystemGeneratedIdentifier, 7, 9) + ", OB Type = " +
                  NumberToString
                  (entities.ObligationType.SystemGeneratedIdentifier, 14, 2) + ", Amount =" +
                  NumberToString((long)(local.Common.TotalCurrency * 100), 15);

                if (local.Common.TotalCurrency < 0)
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "-";
                }

                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + ", CRD ID = " + NumberToString
                  (entities.CashReceipt.SequentialNumber, 9, 7) + "-" + NumberToString
                  (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
                UseCabErrorReport();

                continue;
              }

              local.ForCreate.Comment = "B700 Step 4 (1b)";
              local.Supp.Number = "";
            }

            // -- Record audit detail.
            if (AsChar(import.WriteAuditDtl.Flag) == 'Y')
            {
              local.ForCreate.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreate.CollectionDte = entities.Collection.CollectionDt;

              // *** Build CRD #
              local.ForCreate.CaseWorkerName =
                NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-"
                ;
              local.ForCreate.CaseWorkerName =
                TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
                (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
              local.ForCreate.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreate.CollApplToCode =
                entities.Collection.AppliedToCode;
              local.ForCreate.SuppPersonNumber = local.Supp.Number;
              local.ForCreate.ObligorPersonNbr = entities.Ap.Number;
              local.ForCreate.CourtOrderNumber =
                entities.Collection.CourtOrderAppliedTo;
              local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
              UseFnCreateOcse157Verification();
              ++local.Update.Count;
            }

            if (local.Update.Count >= import
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              // -- Checkpoint
              UseFnB700ApplyUpdates();
              local.Update.Count = 0;
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo = "04 " + NumberToString
                (local.Counter.Count, 15, 1) + " " + entities.Ap.Number + " " +
                NumberToString
                (entities.Collection.SystemGeneratedIdentifier, 7, 9);
              UseUpdateCheckpointRstAndCommit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.ForError.LineNumber = "04";
                UseOcse157WriteError();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Abort.Flag = "Y";

                  return;
                }
              }
            }
          }

          break;
        case 2:
          // --------------------------------------------------------------------------------------------
          // ---   Read disbursements processed during the quarter that did not 
          // result in a payment request (i.e. they netted to zero).
          // ---   For the purposes of OCSE34, these collections are considered
          // "disbursed" on the disbursement process date.
          // ---   (Skip cost recovery fees and non-cash disbursements)
          // --------------------------------------------------------------------------------------------
          if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
            (import.ProgramCheckpointRestart.RestartInfo, 1, 4, "04 2"))
          {
            local.RestartCsePerson.Number =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 6, 10);
            local.RestartCollection.SystemGeneratedIdentifier =
              (int)StringToNumber(Substring(
                import.ProgramCheckpointRestart.RestartInfo, 250, 17, 9));
          }
          else
          {
            local.RestartCsePerson.Number = "";
            local.RestartCollection.SystemGeneratedIdentifier = 0;
          }

          foreach(var item in ReadDisbursementCollectionObligationTypeCsePerson())
            
          {
            // @@@  Removed restart logic from the where predicate to correct a 
            // performance problem.
            if (Lt(entities.Ap.Number, local.RestartCsePerson.Number) || Equal
              (entities.Ap.Number, local.RestartCsePerson.Number) && entities
              .Collection.SystemGeneratedIdentifier <= local
              .RestartCollection.SystemGeneratedIdentifier)
            {
              continue;
            }

            if (ReadPaymentRequest2())
            {
              // -- Skip this disbursement since it resulted in a payment 
              // request.
              //    We're only looking for those disbursements which did NOT 
              // result in a paymet request.
              continue;
            }

            // --  Determine if cost recovery fee needs to be included.
            local.Common.TotalCurrency = 0;

            foreach(var item1 in ReadDisbursement3())
            {
              local.Common.TotalCurrency += entities.CostRecoveryFee.Amount;
            }

            if (local.Common.TotalCurrency > 0)
            {
              // -- If there is no payment request tied to the disbursement 
              // collection then the cost recovery
              // -- fee should be added to the this disbursement amount.
              if (ReadPaymentRequest1())
              {
                // -- A payment request exists.  The cost recovery fee would 
                // have been included with the warrant/recapture/potential
                // recovery amount.
                local.Common.TotalCurrency = 0;
              }
              else
              {
                // -- A payment request does not exist.  The cost recovery fee 
                // will be included with the disbursement amount.
              }
            }

            local.Common.TotalCurrency += entities.Disbursement.Amount;

            // --  <END> Determine if cost recovery fee needs to be included.
            MoveOcse157Verification2(local.NullOcse157Verification,
              local.ForCreate);
            UseFnB700Ocse34MaintainLine1();

            if (IsEmpty(local.ForCreate.LineNumber))
            {
              // --  Log to error report
              local.EabReportSend.RptDetail =
                "Unclassified collection - Step 4, Line 4/7, Section " + NumberToString
                (local.Counter.Count, 15, 1) + ", AP # = " + entities
                .Ap.Number + ", Collection Sys Gen Id = " + NumberToString
                (entities.Collection.SystemGeneratedIdentifier, 7, 9) + ", OB Type = " +
                NumberToString
                (entities.ObligationType.SystemGeneratedIdentifier, 14, 2) + ", Amount =" +
                NumberToString((long)(local.Common.TotalCurrency * 100), 15);

              if (local.Common.TotalCurrency < 0)
              {
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "-";
              }

              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + ", CRD ID = " + entities
                .Disbursement.ReferenceNumber;
              UseCabErrorReport();

              continue;
            }

            // -- Record audit detail.
            if (AsChar(import.WriteAuditDtl.Flag) == 'Y')
            {
              local.ForCreate.Comment = "B700 Step 4 (2)";
              local.ForCreate.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreate.CollectionDte = entities.Collection.CollectionDt;

              // *** Build CRD #
              local.ForCreate.CaseWorkerName =
                entities.Disbursement.ReferenceNumber;
              local.ForCreate.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreate.CollApplToCode =
                entities.Collection.AppliedToCode;
              local.ForCreate.SuppPersonNumber = entities.Supp.Number;
              local.ForCreate.ObligorPersonNbr = entities.Ap.Number;
              local.ForCreate.CourtOrderNumber =
                entities.Collection.CourtOrderAppliedTo;
              local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
              UseFnCreateOcse157Verification();
              ++local.Update.Count;
            }

            if (local.Update.Count >= import
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              // -- Checkpoint
              UseFnB700ApplyUpdates();
              local.Update.Count = 0;
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo = "04 " + NumberToString
                (local.Counter.Count, 15, 1) + " " + entities.Ap.Number + " " +
                NumberToString
                (entities.Collection.SystemGeneratedIdentifier, 7, 9);
              UseUpdateCheckpointRstAndCommit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.ForError.LineNumber = "04";
                UseOcse157WriteError();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Abort.Flag = "Y";

                  return;
                }
              }
            }
          }

          break;
        case 3:
          // --------------------------------------------------------------------------------------------
          // ---   Read payment requests processed during the quarter that were 
          // cancelled (i.e. the warrant amount was less than $1).
          // ---   For the purposes of OCSE34, these collections are considered
          // "disbursed" on the effective date of the cancelled status record.
          // --------------------------------------------------------------------------------------------
          if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
            (import.ProgramCheckpointRestart.RestartInfo, 1, 4, "04 3"))
          {
            local.RestartCsePerson.Number =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 6, 10);
            local.RestartCollection.SystemGeneratedIdentifier =
              (int)StringToNumber(Substring(
                import.ProgramCheckpointRestart.RestartInfo, 250, 17, 9));
          }
          else
          {
            local.RestartCsePerson.Number = "";
            local.RestartCollection.SystemGeneratedIdentifier = 0;
          }

          foreach(var item in ReadDisbursementCsePersonCollectionObligationType1())
            
          {
            // @@@  Removed restart logic from the where predicate to correct a 
            // performance problem.
            if (Lt(entities.Ap.Number, local.RestartCsePerson.Number) || Equal
              (entities.Ap.Number, local.RestartCsePerson.Number) && entities
              .Collection.SystemGeneratedIdentifier <= local
              .RestartCollection.SystemGeneratedIdentifier)
            {
              continue;
            }

            MoveOcse157Verification2(local.NullOcse157Verification,
              local.ForCreate);

            // --  Add cost recovery fee to disbursement amount.
            local.Common.TotalCurrency = 0;

            foreach(var item1 in ReadDisbursement3())
            {
              local.Common.TotalCurrency += entities.CostRecoveryFee.Amount;
            }

            if (local.Common.TotalCurrency > 0)
            {
              // -- More CRF changes... for the .01 CRF problem on CRD 1596728-
              // 2327
              if (ReadDisbursement2())
              {
                // --  If the disbursement credit split into multiple 
                // disbursement debits then we only
                //     include the cost recovery fee with one of the debits.  
                // We'll include it on the debit with the
                //     smallest disbursement system generated id.
                local.Common.TotalCurrency = 0;
              }
            }

            local.Common.TotalCurrency += entities.Disbursement.Amount;
            UseFnB700Ocse34MaintainLine1();

            if (IsEmpty(local.ForCreate.LineNumber))
            {
              // --  Log to error report
              local.EabReportSend.RptDetail =
                "Unclassified collection - Step 4, Line 4/7, Section " + NumberToString
                (local.Counter.Count, 15, 1) + ", AP # = " + entities
                .Ap.Number + ", Collection Sys Gen Id = " + NumberToString
                (entities.Collection.SystemGeneratedIdentifier, 7, 9) + ", OB Type = " +
                NumberToString
                (entities.ObligationType.SystemGeneratedIdentifier, 14, 2) + ", Amount =" +
                NumberToString((long)(local.Common.TotalCurrency * 100), 15);

              if (local.Common.TotalCurrency < 0)
              {
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "-";
              }

              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + ", CRD ID = " + entities
                .Disbursement.ReferenceNumber;
              UseCabErrorReport();

              continue;
            }

            // -- Record audit detail.
            if (AsChar(import.WriteAuditDtl.Flag) == 'Y')
            {
              local.ForCreate.Comment = "B700 Step 4 (3)";
              local.ForCreate.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreate.CollectionDte = entities.Collection.CollectionDt;

              // *** Build CRD #
              local.ForCreate.CaseWorkerName =
                entities.Disbursement.ReferenceNumber;
              local.ForCreate.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreate.CollApplToCode =
                entities.Collection.AppliedToCode;
              local.ForCreate.SuppPersonNumber = entities.Supp.Number;
              local.ForCreate.ObligorPersonNbr = entities.Ap.Number;
              local.ForCreate.CourtOrderNumber =
                entities.Collection.CourtOrderAppliedTo;
              local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
              UseFnCreateOcse157Verification();
              ++local.Update.Count;
            }

            if (local.Update.Count >= import
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              // -- Checkpoint
              UseFnB700ApplyUpdates();
              local.Update.Count = 0;
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo = "04 " + NumberToString
                (local.Counter.Count, 15, 1) + " " + entities.Ap.Number + " " +
                NumberToString
                (entities.Collection.SystemGeneratedIdentifier, 7, 9);
              UseUpdateCheckpointRstAndCommit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.ForError.LineNumber = "04";
                UseOcse157WriteError();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Abort.Flag = "Y";

                  return;
                }
              }
            }
          }

          break;
        case 4:
          // --------------------------------------------------------------------------------------------
          // ---   Read disbursements processed during the quarter that resulted
          // in Potential Recoveries and Recaptures.
          // ---   For the purposes of OCSE34, these collections are considered
          // "disbursed" on the created timestamp of the payment request.
          // --------------------------------------------------------------------------------------------
          if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
            (import.ProgramCheckpointRestart.RestartInfo, 1, 4, "04 4"))
          {
            local.RestartCsePerson.Number =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 6, 10);
            local.RestartCollection.SystemGeneratedIdentifier =
              (int)StringToNumber(Substring(
                import.ProgramCheckpointRestart.RestartInfo, 250, 17, 9));
          }
          else
          {
            local.RestartCsePerson.Number = "";
            local.RestartCollection.SystemGeneratedIdentifier = 0;
          }

          foreach(var item in ReadDisbursementCsePersonCollectionObligationType2())
            
          {
            // @@@  Removed restart logic from the where predicate to correct a 
            // performance problem.
            if (Lt(entities.Ap.Number, local.RestartCsePerson.Number) || Equal
              (entities.Ap.Number, local.RestartCsePerson.Number) && entities
              .Collection.SystemGeneratedIdentifier <= local
              .RestartCollection.SystemGeneratedIdentifier)
            {
              continue;
            }

            // --  Skip if collection is adjusted and the disbursement 
            // adjustment has not yet processed.
            if (AsChar(entities.Collection.AdjustedInd) == 'Y' && !
              Lt(import.RptPrdEnd.Date,
              entities.Collection.CollectionAdjustmentDt) && (
                Equal(entities.Collection.DisbursementAdjProcessDate,
              local.NullDateWorkArea.Date) || Lt
              (import.RptPrdEnd.Date,
              entities.Collection.DisbursementAdjProcessDate)))
            {
              // -- The disbursement adjustment processing has not yet occurred,
              // so there is no negative disbursement amount to offset the
              // positive disbursement amount.  Since the collection is adjusted
              // the collection amount will be included in Suspense, do not
              // include it with successfully processed warrants.
              continue;
            }

            MoveOcse157Verification2(local.NullOcse157Verification,
              local.ForCreate);
            local.Common.TotalCurrency = 0;

            foreach(var item1 in ReadDisbursement3())
            {
              local.Common.TotalCurrency += entities.CostRecoveryFee.Amount;
            }

            if (local.Common.TotalCurrency > 0)
            {
              // -- If there is no warrant tied to the disbursement collection 
              // then the cost recovery
              // -- fee should be added to the recapture/potential recovery 
              // amount.
              if (ReadWarrant())
              {
                // -- A warrant exists.  The cost recovery fee would have been 
                // included with the warrant amount.
                local.Common.TotalCurrency = 0;
              }
              else
              {
                // -- A warrant does not exist.  The cost recovery fee will be 
                // included with the recapture/potential recovery amount.
              }
            }

            local.Common.TotalCurrency += entities.Disbursement.Amount;
            UseFnB700Ocse34MaintainLine1();

            if (IsEmpty(local.ForCreate.LineNumber))
            {
              // --  Log to error report
              local.EabReportSend.RptDetail =
                "Unclassified collection - Step 4, Line 4/7, Section " + NumberToString
                (local.Counter.Count, 15, 1) + ", AP # = " + entities
                .Ap.Number + ", Collection Sys Gen Id = " + NumberToString
                (entities.Collection.SystemGeneratedIdentifier, 7, 9) + ", OB Type = " +
                NumberToString
                (entities.ObligationType.SystemGeneratedIdentifier, 14, 2) + ", Amount =" +
                NumberToString((long)(local.Common.TotalCurrency * 100), 15);

              if (local.Common.TotalCurrency < 0)
              {
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "-";
              }

              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + ", CRD ID = " + entities
                .Disbursement.ReferenceNumber;
              UseCabErrorReport();

              continue;
            }

            // -- Record audit detail.
            if (AsChar(import.WriteAuditDtl.Flag) == 'Y')
            {
              local.ForCreate.Comment = "B700 Step 4 (4)";
              local.ForCreate.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreate.CollectionDte = entities.Collection.CollectionDt;

              // *** Build CRD #
              local.ForCreate.CaseWorkerName =
                entities.Disbursement.ReferenceNumber;
              local.ForCreate.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreate.CollApplToCode =
                entities.Collection.AppliedToCode;
              local.ForCreate.SuppPersonNumber = entities.Supp.Number;
              local.ForCreate.ObligorPersonNbr = entities.Ap.Number;
              local.ForCreate.CourtOrderNumber =
                entities.Collection.CourtOrderAppliedTo;
              local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
              UseFnCreateOcse157Verification();
              ++local.Update.Count;
            }

            if (local.Update.Count >= import
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              // -- Checkpoint
              UseFnB700ApplyUpdates();
              local.Update.Count = 0;
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo = "04 " + NumberToString
                (local.Counter.Count, 15, 1) + " " + entities.Ap.Number + " " +
                NumberToString
                (entities.Collection.SystemGeneratedIdentifier, 7, 9);
              UseUpdateCheckpointRstAndCommit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.ForError.LineNumber = "04";
                UseOcse157WriteError();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Abort.Flag = "Y";

                  return;
                }
              }
            }
          }

          break;
        case 5:
          // --------------------------------------------------------------------------------------------
          // ---   Read Warrants paid during the quarter.
          // ---   For the purposes of OCSE34, these collections are considered
          // "disbursed" on the warrant print date.
          // --------------------------------------------------------------------------------------------
          if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
            (import.ProgramCheckpointRestart.RestartInfo, 1, 4, "04 5"))
          {
            local.RestartCsePerson.Number =
              Substring(import.ProgramCheckpointRestart.RestartInfo, 6, 10);
            local.RestartCollection.SystemGeneratedIdentifier =
              (int)StringToNumber(Substring(
                import.ProgramCheckpointRestart.RestartInfo, 250, 17, 9));
          }
          else
          {
            local.RestartCsePerson.Number = "";
            local.RestartCollection.SystemGeneratedIdentifier = 0;
          }

          foreach(var item in ReadDisbursementCsePersonCollectionObligationType3())
            
          {
            // @@@  Removed restart logic from the where predicate to correct a 
            // performance problem.
            if (Lt(entities.Ap.Number, local.RestartCsePerson.Number) || Equal
              (entities.Ap.Number, local.RestartCsePerson.Number) && entities
              .Collection.SystemGeneratedIdentifier <= local
              .RestartCollection.SystemGeneratedIdentifier)
            {
              continue;
            }

            // --  Skip if collection is adjusted and the disbursement 
            // adjustment has not yet processed.
            if (AsChar(entities.Collection.AdjustedInd) == 'Y' && !
              Lt(import.RptPrdEnd.Date,
              entities.Collection.CollectionAdjustmentDt) && (
                Equal(entities.Collection.DisbursementAdjProcessDate,
              local.NullDateWorkArea.Date) || Lt
              (import.RptPrdEnd.Date,
              entities.Collection.DisbursementAdjProcessDate)))
            {
              // -- The disbursement adjustment processing has not yet occurred,
              // so there is no negative disbursement amount to offset the
              // positive disbursement amount.  Since the collection is adjusted
              // the collection amount will be included in Suspense, do not
              // include it with successfully processed warrants.
              continue;
            }

            MoveOcse157Verification2(local.NullOcse157Verification,
              local.ForCreate);
            local.Common.TotalCurrency = 0;

            foreach(var item1 in ReadDisbursement3())
            {
              local.Common.TotalCurrency += entities.CostRecoveryFee.Amount;
            }

            if (local.Common.TotalCurrency > 0)
            {
              if (ReadDisbursement1())
              {
                // --  If the disbursement credit split into multiple 
                // disbursement debits then we only
                //     include the cost recovery fee with one of the debits.  
                // We'll include it on the debit with the
                //     smallest disbursement system generated id.
                local.Common.TotalCurrency = 0;
              }
            }

            local.Common.TotalCurrency += entities.Disbursement.Amount;
            UseFnB700Ocse34MaintainLine1();

            if (IsEmpty(local.ForCreate.LineNumber))
            {
              // --  Log to error report
              local.EabReportSend.RptDetail =
                "Unclassified collection - Step 4, Line 4/7, Section " + NumberToString
                (local.Counter.Count, 15, 1) + ", AP # = " + entities
                .Ap.Number + ", Collection Sys Gen Id = " + NumberToString
                (entities.Collection.SystemGeneratedIdentifier, 7, 9) + ", OB Type = " +
                NumberToString
                (entities.ObligationType.SystemGeneratedIdentifier, 14, 2) + ", Amount =" +
                NumberToString((long)(local.Common.TotalCurrency * 100), 15);

              if (local.Common.TotalCurrency < 0)
              {
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "-";
              }

              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + ", CRD ID = " + entities
                .Disbursement.ReferenceNumber;
              UseCabErrorReport();

              continue;
            }

            // -- Record audit detail.
            if (AsChar(import.WriteAuditDtl.Flag) == 'Y')
            {
              local.ForCreate.Comment = "B700 Step 4 (5)";
              local.ForCreate.CollectionSgi =
                entities.Collection.SystemGeneratedIdentifier;
              local.ForCreate.CollectionDte = entities.Collection.CollectionDt;

              // *** Build CRD #
              local.ForCreate.CaseWorkerName =
                entities.Disbursement.ReferenceNumber;
              local.ForCreate.CollCreatedDte =
                Date(entities.Collection.CreatedTmst);
              local.ForCreate.CollApplToCode =
                entities.Collection.AppliedToCode;
              local.ForCreate.SuppPersonNumber = entities.Supp.Number;
              local.ForCreate.ObligorPersonNbr = entities.Ap.Number;
              local.ForCreate.CourtOrderNumber =
                entities.Collection.CourtOrderAppliedTo;
              local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
              UseFnCreateOcse157Verification();
              ++local.Update.Count;
            }

            if (local.Update.Count >= import
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              // -- Checkpoint
              UseFnB700ApplyUpdates();
              local.Update.Count = 0;
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo = "04 " + NumberToString
                (local.Counter.Count, 15, 1) + " " + entities.Ap.Number + " " +
                NumberToString
                (entities.Collection.SystemGeneratedIdentifier, 7, 9);
              UseUpdateCheckpointRstAndCommit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                local.ForError.LineNumber = "04";
                UseOcse157WriteError();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Abort.Flag = "Y";

                  return;
                }
              }
            }
          }

          break;
        default:
          break;
      }

      // -- Checkpoint to start at the next local counter value.
      UseFnB700ApplyUpdates();
      local.Update.Count = 0;
      local.ProgramCheckpointRestart.RestartInd = "Y";
      local.ProgramCheckpointRestart.RestartInfo = "04 " + NumberToString
        ((long)local.Counter.Count + 1, 15, 1) + " 0000000000 000000000";
      UseUpdateCheckpointRstAndCommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ForError.LineNumber = "04";
        UseOcse157WriteError();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Abort.Flag = "Y";

          return;
        }
      }
    }

    // **************************************************************************
    // **  Apply counts to database.
    // **************************************************************************
    UseFnB700ApplyUpdates();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "05" + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "04";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.CollectionDt = source.CollectionDt;
    target.AdjustedInd = source.AdjustedInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CreatedTmst = source.CreatedTmst;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
  }

  private static void MoveGroup1(FnB700BuildGvForRestart.Export.
    GroupGroup source, Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup2(FnB700MaintainLine2Totals.Import.
    GroupGroup source, Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup3(FnB700Ocse34MaintainLine47.Import.
    GroupGroup source, Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup4(Import.GroupGroup source,
    FnB700MaintainLine2Totals.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup5(Import.GroupGroup source,
    FnB700ApplyUpdates.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup6(Import.GroupGroup source,
    FnB700Ocse34MaintainLine47.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveIncomingForeign(Import.IncomingForeignGroup source,
    FnB700Ocse34MaintainLine47.Import.IncomingForeignGroup target)
  {
    target.GimportIncomingForeign.StandardNumber =
      source.GimportIncomingForeign.StandardNumber;
  }

  private static void MoveOcse157Verification1(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseWorkerName = source.CaseWorkerName;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseWorkerName = source.CaseWorkerName;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification3(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
  }

  private static void MoveOcse34(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveOutgoingForeign(Import.OutgoingForeignGroup source,
    FnB700MaintainLine2Totals.Import.OutgoingForeignGroup target)
  {
    target.GimportOutgoingForeign.StandardNumber =
      source.GimportOutgoingForeign.StandardNumber;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ParameterList = source.ParameterList;
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

  private void UseFnB700ApplyUpdates()
  {
    var useImport = new FnB700ApplyUpdates.Import();
    var useExport = new FnB700ApplyUpdates.Export();

    import.Group.CopyTo(useImport.Group, MoveGroup5);
    MoveOcse34(import.Ocse34, useImport.Ocse34);

    Call(FnB700ApplyUpdates.Execute, useImport, useExport);
  }

  private void UseFnB700BuildGvForRestart()
  {
    var useImport = new FnB700BuildGvForRestart.Import();
    var useExport = new FnB700BuildGvForRestart.Export();

    MoveOcse34(import.Ocse34, useImport.Ocse34);

    Call(FnB700BuildGvForRestart.Execute, useImport, useExport);

    useExport.Group.CopyTo(import.Group, MoveGroup1);
  }

  private void UseFnB700MaintainLine2Totals()
  {
    var useImport = new FnB700MaintainLine2Totals.Import();
    var useExport = new FnB700MaintainLine2Totals.Export();

    useImport.CashReceiptDetail.CourtOrderNumber =
      entities.CoNumber.CourtOrderNumber;
    useImport.CollectionType.SequentialIdentifier =
      entities.CollectionType.SequentialIdentifier;
    MoveCashReceiptSourceType(entities.CashReceiptSourceType,
      useImport.CashReceiptSourceType);
    import.Group.CopyTo(useImport.Group, MoveGroup4);
    import.OutgoingForeign.
      CopyTo(useImport.OutgoingForeign, MoveOutgoingForeign);
    useImport.Common.TotalCurrency = local.Common.TotalCurrency;

    Call(FnB700MaintainLine2Totals.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup2);
    local.Common.TotalCurrency = useImport.Common.TotalCurrency;
    local.ForCreate.LineNumber = useExport.Ocse157Verification.LineNumber;
  }

  private void UseFnB700Ocse34MaintainLine1()
  {
    var useImport = new FnB700Ocse34MaintainLine47.Import();
    var useExport = new FnB700Ocse34MaintainLine47.Export();

    useImport.Supp.Number = entities.Supp.Number;
    MoveCollection(entities.Collection, useImport.Collection);
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    import.Group.CopyTo(useImport.Group, MoveGroup6);
    import.IncomingForeign.
      CopyTo(useImport.IncomingForeign, MoveIncomingForeign);
    useImport.Common.TotalCurrency = local.Common.TotalCurrency;

    Call(FnB700Ocse34MaintainLine47.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup3);
    local.Common.TotalCurrency = useImport.Common.TotalCurrency;
    local.ForCreate.LineNumber = useExport.Ocse157Verification.LineNumber;
  }

  private void UseFnB700Ocse34MaintainLine2()
  {
    var useImport = new FnB700Ocse34MaintainLine47.Import();
    var useExport = new FnB700Ocse34MaintainLine47.Export();

    MoveCollection(entities.Collection, useImport.Collection);
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    import.Group.CopyTo(useImport.Group, MoveGroup6);
    import.IncomingForeign.
      CopyTo(useImport.IncomingForeign, MoveIncomingForeign);
    useImport.Supp.Number = local.Supp.Number;
    useImport.Common.TotalCurrency = local.Common.TotalCurrency;

    Call(FnB700Ocse34MaintainLine47.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup3);
    local.Common.TotalCurrency = useImport.Common.TotalCurrency;
    local.ForCreate.LineNumber = useExport.Ocse157Verification.LineNumber;
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = import.RptPrdEnd.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    local.LastDayBusStartTime.Timestamp = useExport.DateWorkArea.Timestamp;
  }

  private void UseFnCreateOcse157Verification()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification1(local.ForCreate, useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    MoveOcse157Verification3(local.ForError, useImport.Ocse157Verification);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CoNumber.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CoNumber.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CoNumber.CstIdentifier = db.GetInt32(reader, 1);
        entities.CoNumber.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CoNumber.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.CoNumber.CourtOrderNumber = db.GetNullableString(reader, 4);
        entities.CoNumber.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionCsePersonCashReceiptCashReceiptDetail()
  {
    entities.Collection.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Ap.Populated = false;
    entities.CollectionType.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCollectionCsePersonCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          import.RptPrdBegin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.RptPrdEnd.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt", import.RptPrdEnd.Date.GetValueOrDefault());
        db.
          SetDate(command, "date", import.RptPrdBegin.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 5);
        entities.Collection.CrtType = db.GetInt32(reader, 6);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 6);
        entities.Collection.CstId = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 7);
        entities.Collection.CrvId = db.GetInt32(reader, 8);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 8);
        entities.Collection.CrdId = db.GetInt32(reader, 9);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 9);
        entities.Collection.ObgId = db.GetInt32(reader, 10);
        entities.Collection.CspNumber = db.GetString(reader, 11);
        entities.Ap.Number = db.GetString(reader, 11);
        entities.Collection.CpaType = db.GetString(reader, 12);
        entities.Collection.OtrId = db.GetInt32(reader, 13);
        entities.Collection.OtrType = db.GetString(reader, 14);
        entities.Collection.PreviousCollectionAdjDate =
          db.GetNullableDate(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 18);
        entities.Collection.Amount = db.GetDecimal(reader, 19);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 20);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 21);
        entities.Collection.UnadjustedDate = db.GetNullableDate(reader, 22);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 23);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 23);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 24);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 24);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 25);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 25);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 26);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 27);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 27);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 28);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 29);
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Ap.Populated = true;
        entities.CollectionType.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supp.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supp.Populated = true;
      });
  }

  private bool ReadDisbursement1()
  {
    System.Diagnostics.Debug.Assert(entities.DisbCollection.Populated);
    entities.Other.Populated = false;

    return Read("ReadDisbursement1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.DisbCollection.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.DisbCollection.CpaType);
        db.SetString(command, "cspPNumber", entities.DisbCollection.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.Disbursement.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Other.CpaType = db.GetString(reader, 0);
        entities.Other.CspNumber = db.GetString(reader, 1);
        entities.Other.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Other.DbtGeneratedId = db.GetNullableInt32(reader, 3);
        entities.Other.PrqGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Other.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Other.CpaType);
      });
  }

  private bool ReadDisbursement2()
  {
    System.Diagnostics.Debug.Assert(entities.DisbCollection.Populated);
    entities.Other.Populated = false;

    return Read("ReadDisbursement2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.DisbCollection.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.DisbCollection.CpaType);
        db.SetString(command, "cspPNumber", entities.DisbCollection.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.Disbursement.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Other.CpaType = db.GetString(reader, 0);
        entities.Other.CspNumber = db.GetString(reader, 1);
        entities.Other.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Other.DbtGeneratedId = db.GetNullableInt32(reader, 3);
        entities.Other.PrqGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Other.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Other.CpaType);
      });
  }

  private IEnumerable<bool> ReadDisbursement3()
  {
    System.Diagnostics.Debug.Assert(entities.DisbCollection.Populated);
    entities.CostRecoveryFee.Populated = false;

    return ReadEach("ReadDisbursement3",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.DisbCollection.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.DisbCollection.CpaType);
        db.SetString(command, "cspPNumber", entities.DisbCollection.CspNumber);
      },
      (db, reader) =>
      {
        entities.CostRecoveryFee.CpaType = db.GetString(reader, 0);
        entities.CostRecoveryFee.CspNumber = db.GetString(reader, 1);
        entities.CostRecoveryFee.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CostRecoveryFee.Amount = db.GetDecimal(reader, 3);
        entities.CostRecoveryFee.DbtGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.CostRecoveryFee.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.CostRecoveryFee.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementCollectionObligationTypeCsePerson()
  {
    entities.Supp.Populated = false;
    entities.Collection.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Ap.Populated = false;
    entities.Disbursement.Populated = false;
    entities.DisbCollection.Populated = false;

    return ReadEach("ReadDisbursementCollectionObligationTypeCsePerson",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.RptPrdBegin.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.RptPrdEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Disbursement.CpaType = db.GetString(reader, 0);
        entities.Disbursement.CspNumber = db.GetString(reader, 1);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.Disbursement.Amount = db.GetDecimal(reader, 3);
        entities.Disbursement.ProcessDate = db.GetNullableDate(reader, 4);
        entities.Disbursement.CashNonCashInd = db.GetString(reader, 5);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 6);
        entities.Disbursement.PrqGeneratedId = db.GetNullableInt32(reader, 7);
        entities.Disbursement.ReferenceNumber = db.GetNullableString(reader, 8);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 9);
        entities.DisbCollection.ColId = db.GetNullableInt32(reader, 9);
        entities.Collection.AppliedToCode = db.GetString(reader, 10);
        entities.Collection.CollectionDt = db.GetDate(reader, 11);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 12);
        entities.Collection.ConcurrentInd = db.GetString(reader, 13);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 14);
        entities.Collection.CrtType = db.GetInt32(reader, 15);
        entities.DisbCollection.CrtId = db.GetNullableInt32(reader, 15);
        entities.Collection.CstId = db.GetInt32(reader, 16);
        entities.DisbCollection.CstId = db.GetNullableInt32(reader, 16);
        entities.Collection.CrvId = db.GetInt32(reader, 17);
        entities.DisbCollection.CrvId = db.GetNullableInt32(reader, 17);
        entities.Collection.CrdId = db.GetInt32(reader, 18);
        entities.DisbCollection.CrdId = db.GetNullableInt32(reader, 18);
        entities.Collection.ObgId = db.GetInt32(reader, 19);
        entities.DisbCollection.ObgId = db.GetNullableInt32(reader, 19);
        entities.Collection.CspNumber = db.GetString(reader, 20);
        entities.Ap.Number = db.GetString(reader, 20);
        entities.DisbCollection.CspNumberDisb =
          db.GetNullableString(reader, 20);
        entities.Collection.CpaType = db.GetString(reader, 21);
        entities.DisbCollection.CpaTypeDisb = db.GetNullableString(reader, 21);
        entities.Collection.OtrId = db.GetInt32(reader, 22);
        entities.DisbCollection.OtrId = db.GetNullableInt32(reader, 22);
        entities.Collection.OtrType = db.GetString(reader, 23);
        entities.DisbCollection.OtrTypeDisb = db.GetNullableString(reader, 23);
        entities.Collection.PreviousCollectionAdjDate =
          db.GetNullableDate(reader, 24);
        entities.Collection.OtyId = db.GetInt32(reader, 25);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 25);
        entities.DisbCollection.OtyId = db.GetNullableInt32(reader, 25);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 26);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 27);
        entities.Collection.Amount = db.GetDecimal(reader, 28);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 29);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 30);
        entities.Collection.UnadjustedDate = db.GetNullableDate(reader, 31);
        entities.Supp.Number = db.GetString(reader, 32);
        entities.DisbCollection.CpaType = db.GetString(reader, 33);
        entities.DisbCollection.CspNumber = db.GetString(reader, 34);
        entities.DisbCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 35);
        entities.Supp.Populated = true;
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Ap.Populated = true;
        entities.Disbursement.Populated = true;
        entities.DisbCollection.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Disbursement.CpaType);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Disbursement.CashNonCashInd);
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbCollection.CpaTypeDisb);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbCollection.OtrTypeDisb);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbCollection.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementCsePersonCollectionObligationType1()
  {
    entities.Supp.Populated = false;
    entities.Collection.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Ap.Populated = false;
    entities.Disbursement.Populated = false;
    entities.DisbCollection.Populated = false;

    return ReadEach("ReadDisbursementCsePersonCollectionObligationType1",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.RptPrdBegin.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.RptPrdEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Disbursement.CpaType = db.GetString(reader, 0);
        entities.Disbursement.CspNumber = db.GetString(reader, 1);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.Disbursement.Amount = db.GetDecimal(reader, 3);
        entities.Disbursement.ProcessDate = db.GetNullableDate(reader, 4);
        entities.Disbursement.CashNonCashInd = db.GetString(reader, 5);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 6);
        entities.Disbursement.PrqGeneratedId = db.GetNullableInt32(reader, 7);
        entities.Disbursement.ReferenceNumber = db.GetNullableString(reader, 8);
        entities.Ap.Number = db.GetString(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.DisbCollection.CspNumberDisb = db.GetNullableString(reader, 9);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.DisbCollection.ColId = db.GetNullableInt32(reader, 10);
        entities.Collection.AppliedToCode = db.GetString(reader, 11);
        entities.Collection.CollectionDt = db.GetDate(reader, 12);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 13);
        entities.Collection.ConcurrentInd = db.GetString(reader, 14);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 15);
        entities.Collection.CrtType = db.GetInt32(reader, 16);
        entities.DisbCollection.CrtId = db.GetNullableInt32(reader, 16);
        entities.Collection.CstId = db.GetInt32(reader, 17);
        entities.DisbCollection.CstId = db.GetNullableInt32(reader, 17);
        entities.Collection.CrvId = db.GetInt32(reader, 18);
        entities.DisbCollection.CrvId = db.GetNullableInt32(reader, 18);
        entities.Collection.CrdId = db.GetInt32(reader, 19);
        entities.DisbCollection.CrdId = db.GetNullableInt32(reader, 19);
        entities.Collection.ObgId = db.GetInt32(reader, 20);
        entities.DisbCollection.ObgId = db.GetNullableInt32(reader, 20);
        entities.Collection.CpaType = db.GetString(reader, 21);
        entities.DisbCollection.CpaTypeDisb = db.GetNullableString(reader, 21);
        entities.Collection.OtrId = db.GetInt32(reader, 22);
        entities.DisbCollection.OtrId = db.GetNullableInt32(reader, 22);
        entities.Collection.OtrType = db.GetString(reader, 23);
        entities.DisbCollection.OtrTypeDisb = db.GetNullableString(reader, 23);
        entities.Collection.PreviousCollectionAdjDate =
          db.GetNullableDate(reader, 24);
        entities.Collection.OtyId = db.GetInt32(reader, 25);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 25);
        entities.DisbCollection.OtyId = db.GetNullableInt32(reader, 25);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 26);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 27);
        entities.Collection.Amount = db.GetDecimal(reader, 28);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 29);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 30);
        entities.Collection.UnadjustedDate = db.GetNullableDate(reader, 31);
        entities.Supp.Number = db.GetString(reader, 32);
        entities.DisbCollection.CpaType = db.GetString(reader, 33);
        entities.DisbCollection.CspNumber = db.GetString(reader, 34);
        entities.DisbCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 35);
        entities.Supp.Populated = true;
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Ap.Populated = true;
        entities.Disbursement.Populated = true;
        entities.DisbCollection.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Disbursement.CpaType);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Disbursement.CashNonCashInd);
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbCollection.CpaTypeDisb);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbCollection.OtrTypeDisb);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbCollection.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementCsePersonCollectionObligationType2()
  {
    entities.Supp.Populated = false;
    entities.Collection.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Ap.Populated = false;
    entities.Disbursement.Populated = false;
    entities.DisbCollection.Populated = false;

    return ReadEach("ReadDisbursementCsePersonCollectionObligationType2",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.RptPrdBegin.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.RptPrdEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Disbursement.CpaType = db.GetString(reader, 0);
        entities.Disbursement.CspNumber = db.GetString(reader, 1);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.Disbursement.Amount = db.GetDecimal(reader, 3);
        entities.Disbursement.ProcessDate = db.GetNullableDate(reader, 4);
        entities.Disbursement.CashNonCashInd = db.GetString(reader, 5);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 6);
        entities.Disbursement.PrqGeneratedId = db.GetNullableInt32(reader, 7);
        entities.Disbursement.ReferenceNumber = db.GetNullableString(reader, 8);
        entities.Ap.Number = db.GetString(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.DisbCollection.CspNumberDisb = db.GetNullableString(reader, 9);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.DisbCollection.ColId = db.GetNullableInt32(reader, 10);
        entities.Collection.AppliedToCode = db.GetString(reader, 11);
        entities.Collection.CollectionDt = db.GetDate(reader, 12);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 13);
        entities.Collection.ConcurrentInd = db.GetString(reader, 14);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 15);
        entities.Collection.CrtType = db.GetInt32(reader, 16);
        entities.DisbCollection.CrtId = db.GetNullableInt32(reader, 16);
        entities.Collection.CstId = db.GetInt32(reader, 17);
        entities.DisbCollection.CstId = db.GetNullableInt32(reader, 17);
        entities.Collection.CrvId = db.GetInt32(reader, 18);
        entities.DisbCollection.CrvId = db.GetNullableInt32(reader, 18);
        entities.Collection.CrdId = db.GetInt32(reader, 19);
        entities.DisbCollection.CrdId = db.GetNullableInt32(reader, 19);
        entities.Collection.ObgId = db.GetInt32(reader, 20);
        entities.DisbCollection.ObgId = db.GetNullableInt32(reader, 20);
        entities.Collection.CpaType = db.GetString(reader, 21);
        entities.DisbCollection.CpaTypeDisb = db.GetNullableString(reader, 21);
        entities.Collection.OtrId = db.GetInt32(reader, 22);
        entities.DisbCollection.OtrId = db.GetNullableInt32(reader, 22);
        entities.Collection.OtrType = db.GetString(reader, 23);
        entities.DisbCollection.OtrTypeDisb = db.GetNullableString(reader, 23);
        entities.Collection.PreviousCollectionAdjDate =
          db.GetNullableDate(reader, 24);
        entities.Collection.OtyId = db.GetInt32(reader, 25);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 25);
        entities.DisbCollection.OtyId = db.GetNullableInt32(reader, 25);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 26);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 27);
        entities.Collection.Amount = db.GetDecimal(reader, 28);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 29);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 30);
        entities.Collection.UnadjustedDate = db.GetNullableDate(reader, 31);
        entities.Supp.Number = db.GetString(reader, 32);
        entities.DisbCollection.CpaType = db.GetString(reader, 33);
        entities.DisbCollection.CspNumber = db.GetString(reader, 34);
        entities.DisbCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 35);
        entities.Supp.Populated = true;
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Ap.Populated = true;
        entities.Disbursement.Populated = true;
        entities.DisbCollection.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Disbursement.CpaType);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Disbursement.CashNonCashInd);
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbCollection.CpaTypeDisb);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbCollection.OtrTypeDisb);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbCollection.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementCsePersonCollectionObligationType3()
  {
    entities.Supp.Populated = false;
    entities.Collection.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Ap.Populated = false;
    entities.Disbursement.Populated = false;
    entities.DisbCollection.Populated = false;

    return ReadEach("ReadDisbursementCsePersonCollectionObligationType3",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.RptPrdBegin.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.RptPrdEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Disbursement.CpaType = db.GetString(reader, 0);
        entities.Disbursement.CspNumber = db.GetString(reader, 1);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.Disbursement.Amount = db.GetDecimal(reader, 3);
        entities.Disbursement.ProcessDate = db.GetNullableDate(reader, 4);
        entities.Disbursement.CashNonCashInd = db.GetString(reader, 5);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 6);
        entities.Disbursement.PrqGeneratedId = db.GetNullableInt32(reader, 7);
        entities.Disbursement.ReferenceNumber = db.GetNullableString(reader, 8);
        entities.Ap.Number = db.GetString(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.DisbCollection.CspNumberDisb = db.GetNullableString(reader, 9);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.DisbCollection.ColId = db.GetNullableInt32(reader, 10);
        entities.Collection.AppliedToCode = db.GetString(reader, 11);
        entities.Collection.CollectionDt = db.GetDate(reader, 12);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 13);
        entities.Collection.ConcurrentInd = db.GetString(reader, 14);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 15);
        entities.Collection.CrtType = db.GetInt32(reader, 16);
        entities.DisbCollection.CrtId = db.GetNullableInt32(reader, 16);
        entities.Collection.CstId = db.GetInt32(reader, 17);
        entities.DisbCollection.CstId = db.GetNullableInt32(reader, 17);
        entities.Collection.CrvId = db.GetInt32(reader, 18);
        entities.DisbCollection.CrvId = db.GetNullableInt32(reader, 18);
        entities.Collection.CrdId = db.GetInt32(reader, 19);
        entities.DisbCollection.CrdId = db.GetNullableInt32(reader, 19);
        entities.Collection.ObgId = db.GetInt32(reader, 20);
        entities.DisbCollection.ObgId = db.GetNullableInt32(reader, 20);
        entities.Collection.CpaType = db.GetString(reader, 21);
        entities.DisbCollection.CpaTypeDisb = db.GetNullableString(reader, 21);
        entities.Collection.OtrId = db.GetInt32(reader, 22);
        entities.DisbCollection.OtrId = db.GetNullableInt32(reader, 22);
        entities.Collection.OtrType = db.GetString(reader, 23);
        entities.DisbCollection.OtrTypeDisb = db.GetNullableString(reader, 23);
        entities.Collection.PreviousCollectionAdjDate =
          db.GetNullableDate(reader, 24);
        entities.Collection.OtyId = db.GetInt32(reader, 25);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 25);
        entities.DisbCollection.OtyId = db.GetNullableInt32(reader, 25);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 26);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 27);
        entities.Collection.Amount = db.GetDecimal(reader, 28);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 29);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 30);
        entities.Collection.UnadjustedDate = db.GetNullableDate(reader, 31);
        entities.Supp.Number = db.GetString(reader, 32);
        entities.DisbCollection.CpaType = db.GetString(reader, 33);
        entities.DisbCollection.CspNumber = db.GetString(reader, 34);
        entities.DisbCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 35);
        entities.Supp.Populated = true;
        entities.Collection.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Ap.Populated = true;
        entities.Disbursement.Populated = true;
        entities.DisbCollection.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Disbursement.CpaType);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Disbursement.CashNonCashInd);
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbCollection.CpaTypeDisb);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbCollection.OtrTypeDisb);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbCollection.CpaType);

        return true;
      });
  }

  private bool ReadPaymentRequest1()
  {
    System.Diagnostics.Debug.Assert(entities.DisbCollection.Populated);
    entities.KeyOnly.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.DisbCollection.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.DisbCollection.CpaType);
        db.SetString(command, "cspPNumber", entities.DisbCollection.CspNumber);
      },
      (db, reader) =>
      {
        entities.KeyOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.KeyOnly.PrqRGeneratedId = db.GetNullableInt32(reader, 1);
        entities.KeyOnly.Populated = true;
      });
  }

  private bool ReadPaymentRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.Disbursement.Populated);
    entities.KeyOnly.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.Disbursement.PrqGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.KeyOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.KeyOnly.PrqRGeneratedId = db.GetNullableInt32(reader, 1);
        entities.KeyOnly.Populated = true;
      });
  }

  private bool ReadWarrant()
  {
    System.Diagnostics.Debug.Assert(entities.DisbCollection.Populated);
    entities.Warrant.Populated = false;

    return Read("ReadWarrant",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.DisbCollection.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.DisbCollection.CpaType);
        db.SetString(command, "cspPNumber", entities.DisbCollection.CspNumber);
      },
      (db, reader) =>
      {
        entities.Warrant.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Warrant.Type1 = db.GetString(reader, 1);
        entities.Warrant.PrqRGeneratedId = db.GetNullableInt32(reader, 2);
        entities.Warrant.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.Warrant.Type1);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 74;

      private Common common;
    }

    /// <summary>A IncomingForeignGroup group.</summary>
    [Serializable]
    public class IncomingForeignGroup
    {
      /// <summary>
      /// A value of GimportIncomingForeign.
      /// </summary>
      [JsonPropertyName("gimportIncomingForeign")]
      public LegalAction GimportIncomingForeign
      {
        get => gimportIncomingForeign ??= new();
        set => gimportIncomingForeign = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private LegalAction gimportIncomingForeign;
    }

    /// <summary>A OutgoingForeignGroup group.</summary>
    [Serializable]
    public class OutgoingForeignGroup
    {
      /// <summary>
      /// A value of GimportOutgoingForeign.
      /// </summary>
      [JsonPropertyName("gimportOutgoingForeign")]
      public LegalAction GimportOutgoingForeign
      {
        get => gimportOutgoingForeign ??= new();
        set => gimportOutgoingForeign = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private LegalAction gimportOutgoingForeign;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
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
    /// A value of RptPrdEnd.
    /// </summary>
    [JsonPropertyName("rptPrdEnd")]
    public DateWorkArea RptPrdEnd
    {
      get => rptPrdEnd ??= new();
      set => rptPrdEnd = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    /// <summary>
    /// A value of RptPrdBegin.
    /// </summary>
    [JsonPropertyName("rptPrdBegin")]
    public DateWorkArea RptPrdBegin
    {
      get => rptPrdBegin ??= new();
      set => rptPrdBegin = value;
    }

    /// <summary>
    /// Gets a value of IncomingForeign.
    /// </summary>
    [JsonIgnore]
    public Array<IncomingForeignGroup> IncomingForeign =>
      incomingForeign ??= new(IncomingForeignGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IncomingForeign for json serialization.
    /// </summary>
    [JsonPropertyName("incomingForeign")]
    [Computed]
    public IList<IncomingForeignGroup> IncomingForeign_Json
    {
      get => incomingForeign;
      set => IncomingForeign.Assign(value);
    }

    /// <summary>
    /// Gets a value of OutgoingForeign.
    /// </summary>
    [JsonIgnore]
    public Array<OutgoingForeignGroup> OutgoingForeign =>
      outgoingForeign ??= new(OutgoingForeignGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OutgoingForeign for json serialization.
    /// </summary>
    [JsonPropertyName("outgoingForeign")]
    [Computed]
    public IList<OutgoingForeignGroup> OutgoingForeign_Json
    {
      get => outgoingForeign;
      set => OutgoingForeign.Assign(value);
    }

    /// <summary>
    /// A value of WriteAuditDtl.
    /// </summary>
    [JsonPropertyName("writeAuditDtl")]
    public Common WriteAuditDtl
    {
      get => writeAuditDtl ??= new();
      set => writeAuditDtl = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification ocse157Verification;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea rptPrdEnd;
    private Array<GroupGroup> group;
    private Ocse34 ocse34;
    private DateWorkArea rptPrdBegin;
    private Array<IncomingForeignGroup> incomingForeign;
    private Array<OutgoingForeignGroup> outgoingForeign;
    private Common writeAuditDtl;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public Ocse157Verification ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    /// <summary>
    /// A value of TbdLocalCollAppliedInd.
    /// </summary>
    [JsonPropertyName("tbdLocalCollAppliedInd")]
    public Common TbdLocalCollAppliedInd
    {
      get => tbdLocalCollAppliedInd ??= new();
      set => tbdLocalCollAppliedInd = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of LastDayBusStartTime.
    /// </summary>
    [JsonPropertyName("lastDayBusStartTime")]
    public DateWorkArea LastDayBusStartTime
    {
      get => lastDayBusStartTime ??= new();
      set => lastDayBusStartTime = value;
    }

    /// <summary>
    /// A value of RestartInSection.
    /// </summary>
    [JsonPropertyName("restartInSection")]
    public Common RestartInSection
    {
      get => restartInSection ??= new();
      set => restartInSection = value;
    }

    /// <summary>
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of RestartCollection.
    /// </summary>
    [JsonPropertyName("restartCollection")]
    public Collection RestartCollection
    {
      get => restartCollection ??= new();
      set => restartCollection = value;
    }

    /// <summary>
    /// A value of NullOcse157Verification.
    /// </summary>
    [JsonPropertyName("nullOcse157Verification")]
    public Ocse157Verification NullOcse157Verification
    {
      get => nullOcse157Verification ??= new();
      set => nullOcse157Verification = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
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
    /// A value of EvaluationDate.
    /// </summary>
    [JsonPropertyName("evaluationDate")]
    public DateWorkArea EvaluationDate
    {
      get => evaluationDate ??= new();
      set => evaluationDate = value;
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
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
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
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    private DateWorkArea nullDateWorkArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forCreate;
    private Common tbdLocalCollAppliedInd;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private DateWorkArea lastDayBusStartTime;
    private Common restartInSection;
    private CsePerson restartCsePerson;
    private Collection restartCollection;
    private Ocse157Verification nullOcse157Verification;
    private CsePerson supp;
    private Common common;
    private DateWorkArea evaluationDate;
    private EabReportSend eabReportSend;
    private Common counter;
    private Common update;
    private Ocse157Verification forError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public DisbursementTransaction Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of Warrant.
    /// </summary>
    [JsonPropertyName("warrant")]
    public PaymentRequest Warrant
    {
      get => warrant ??= new();
      set => warrant = value;
    }

    /// <summary>
    /// A value of CostRecoveryFee.
    /// </summary>
    [JsonPropertyName("costRecoveryFee")]
    public DisbursementTransaction CostRecoveryFee
    {
      get => costRecoveryFee ??= new();
      set => costRecoveryFee = value;
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
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of CoNumber.
    /// </summary>
    [JsonPropertyName("coNumber")]
    public CashReceiptDetail CoNumber
    {
      get => coNumber ??= new();
      set => coNumber = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public PaymentRequest KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
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
    /// A value of DisbCollection.
    /// </summary>
    [JsonPropertyName("disbCollection")]
    public DisbursementTransaction DisbCollection
    {
      get => disbCollection ??= new();
      set => disbCollection = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private DisbursementTransaction other;
    private PaymentRequest warrant;
    private DisbursementTransaction costRecoveryFee;
    private DisbursementType disbursementType;
    private CsePerson supp;
    private CsePersonAccount supported;
    private ObligationTransaction debt;
    private Collection collection;
    private ObligationType obligationType;
    private CsePerson ap;
    private CashReceiptDetail coNumber;
    private CollectionType collectionType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private PaymentRequest keyOnly;
    private DisbursementTransaction disbursement;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction disbCollection;
    private PaymentRequest paymentRequest;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
  }
#endregion
}
