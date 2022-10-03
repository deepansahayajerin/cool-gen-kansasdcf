// Program: FN_B680_AR_GDG_EXTRACT, ID: 374555301, model: 746.
// Short name: SWEF680B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B680_AR_GDG_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB680ArGdgExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B680_AR_GDG_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB680ArGdgExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB680ArGdgExtract.
  /// </summary>
  public FnB680ArGdgExtract(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 03/25/10  DDupree	CQ12861	Initial Development.  New business rules for AR
    // statements.
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  This program extracts collection and payment request information 
    // needed to be sent to EES.
    // --
    // --  If an AR number and reporting timeframe is not specified on the PPI 
    // record then the program extracts TAF and
    // --  NA collection info for the preceeding calendar month for all ARs with
    // an assigned obligation.
    // --
    // --  The extract file created by this program is then sorted/summed 
    // externally in a separate job step and the results
    // --  provided as input to SWEFB664 which actually creates the AR 
    // statements.
    // --
    // --  Note that on a restart we simply start at the last checkpointed 
    // person number.  Any duplicated collections
    // --  that may result in the extract file will be eliminated during the 
    // sort step.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  General Housekeeping and Initializations.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB680BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (!IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Initialization Cab Error..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // -- Set hardcoded values.
    UseFnHardcodedDebtDistribution();

    // -- Load cash/non-cash indicators to a group view for later processing.
    foreach(var item in ReadCashReceiptType())
    {
      local.GlocalCashIndicator.Index =
        entities.CashIndicatorCashReceiptType.SystemGeneratedIdentifier - 1;
      local.GlocalCashIndicator.CheckSize();

      local.GlocalCashIndicator.Update.GcashReceiptType.CategoryIndicator =
        entities.CashIndicatorCashReceiptType.CategoryIndicator;
    }

    foreach(var item in ReadCollectionType2())
    {
      local.GlocalCashIndicator.Index =
        entities.CashIndicatorCollectionType.SequentialIdentifier - 1;
      local.GlocalCashIndicator.CheckSize();

      local.GlocalCashIndicator.Update.GcollectionType.CashNonCashInd =
        entities.CashIndicatorCollectionType.CashNonCashInd;
    }

    // -- The following IF is for control purposes only.  We need to  escape out
    // of the IF in certain circumstances.
    local.Common.Flag = "Y";

    if (AsChar(local.Common.Flag) == 'Y')
    {
      if (!IsEmpty(local.ReportingAr.Number))
      {
        // --  Accomodate report generation for a single specified AR number.
        // --  Since TAF collections no longer have a disbursement collection to
        // the AR we need to read each possible
        // --  obligor who might have made a payment to the AR.  Then read for a
        // collection for each of these obligors.
        if (!ReadCsePerson2())
        {
          local.EabReportSend.RptDetail = "Specified AR not found.  AR # " + local
            .ReportingAr.Number;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.Group.Index = -1;

        foreach(var item in ReadCsePerson3())
        {
          if (local.Group.Index + 1 < Local.GroupGroup.Capacity)
          {
            ++local.Group.Index;
            local.Group.CheckSize();

            local.Group.Update.GlocalObligor.Number = entities.Obligor1.Number;
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Exceeded maximum number of obligors for AR.  Maximum number of obligors is " +
              NumberToString(Local.GroupGroup.Capacity, 12, 4) + ".";
            UseCabErrorReport2();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        if (local.Group.Index == -1)
        {
          // --  No obligors found.  Skip read for TAF collections.
          goto Test;
        }
      }

      local.Group.Index = -1;

      do
      {
        if (!IsEmpty(local.ReportingAr.Number))
        {
          ++local.Group.Index;
          local.Group.CheckSize();

          local.ObligorCsePerson.Number = local.Group.Item.GlocalObligor.Number;
        }

        // -------------------------------------------------------------------------------------------------------------------------
        // --  Read all Collections created during the reporting timeframe.
        // -------------------------------------------------------------------------------------------------------------------------
        foreach(var item in ReadCollectionCashReceiptDetailCashReceiptEvent())
        {
          // --  Eliminate non-cash collections.
          local.GlocalCashIndicator.Index =
            entities.CashReceiptType.SystemGeneratedIdentifier - 1;
          local.GlocalCashIndicator.CheckSize();

          if (AsChar(local.GlocalCashIndicator.Item.GcashReceiptType.
            CategoryIndicator) != 'C')
          {
            continue;
          }

          if (ReadCollectionType1())
          {
            local.GlocalCashIndicator.Index =
              entities.CollectionType.SequentialIdentifier - 1;
            local.GlocalCashIndicator.CheckSize();

            if (AsChar(local.GlocalCashIndicator.Item.GcollectionType.
              CashNonCashInd) != 'C')
            {
              continue;
            }
          }
          else
          {
            // --  Increment the error count.
            ++local.NumberOfErrors.Count;

            // --  write to error file...
            local.EabReportSend.RptDetail =
              "Collection type not found for cash receipt detail...Obligor = " +
              entities.Obligor1.Number + "  Cash Receipt Detail Number = " + NumberToString
              (entities.CashReceiptDetail.SequentialIdentifier, 12, 4) + " Obligation Number = " +
              NumberToString
              (entities.Obligation.SystemGeneratedIdentifier, 13, 3);
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // -- Set Abort exit state and escape...
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            // -- Skip the collection.
            continue;
          }

          ++local.ReadCount.Count;

          if (!Equal(entities.Obligor1.Number, local.PreviousObligor.Number))
          {
            local.PreviousObligor.Number = entities.Obligor1.Number;

            // -- Retrieve the obligor last name.
            local.ObligorCsePersonsWorkSet.Number = entities.Obligor1.Number;
            UseEabReadCsePersonBatch1();

            if (IsEmpty(local.AbendData.Type1))
            {
              // -- Successful Adabas read occurred.
            }
            else
            {
              // --  Increment the error count.
              ++local.NumberOfErrors.Count;

              switch(AsChar(local.AbendData.Type1))
              {
                case 'A':
                  // -- Unsuccessful Adabas read occurred.
                  switch(TrimEnd(local.AbendData.AdabasResponseCd))
                  {
                    case "0113":
                      local.EabReportSend.RptDetail =
                        "Adabas response code 113, Obligor cse person number " +
                        entities.Obligor1.Number + " not found in Adabas.";

                      break;
                    case "0148":
                      local.EabReportSend.RptDetail =
                        "Adabas response code 148, Adabas unavailable.  Obligor cse person number " +
                        entities.Obligor1.Number + ".";

                      break;
                    case "0000":
                      local.EabReportSend.RptDetail =
                        "Adabas response code 0000, Obligor cse person number " +
                        entities.Obligor1.Number + " expected record not returned from Adabas.";
                        

                      break;
                    default:
                      local.EabReportSend.RptDetail =
                        "Adabas error, response code = " + local
                        .AbendData.AdabasResponseCd + ", type = " + local
                        .AbendData.Type1 + ", Obligor cse person number = " + entities
                        .Obligor1.Number;

                      break;
                  }

                  break;
                case 'C':
                  // -- CICS action failed.
                  local.EabReportSend.RptDetail =
                    "CICS error, response code = " + local
                    .AbendData.CicsResponseCd + ", for Obligor cse person number = " +
                    entities.Obligor1.Number;

                  break;
                default:
                  // -- Action failed.
                  local.EabReportSend.RptDetail =
                    "Unknown Adabas error, type = " + local.AbendData.Type1 + ", for Obligor cse person number = " +
                    entities.Obligor1.Number;

                  break;
              }

              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

                return;
              }

              if (AsChar(local.AbendData.Type1) == 'A' && (
                Equal(local.AbendData.AdabasResponseCd, "0113") || Equal
                (local.AbendData.AdabasResponseCd, "0000")))
              {
                // -- No need to abend if the AR is not found on Adabas, just 
                // log to the error file.
                continue;
              }
              else
              {
                // -- Any errors beside the AR not being found on Adabas should 
                // abend.
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }

            // -- Only checkpointing after all collections for any particular 
            // obligor are read.
            if (local.ReadCount.Count > local
              .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
            {
              local.ReadCount.Count = 0;
              local.ProgramCheckpointRestart.RestartInd = "Y";
              local.ProgramCheckpointRestart.RestartInfo =
                entities.Obligor1.Number;
              UseUpdatePgmCheckpointRestart();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                // -- Extract the exit state message and write to the error 
                // report.
                UseEabExtractExitStateMessage();
                local.EabReportSend.RptDetail = "Checkpoint Error..." + local
                  .ExitStateWorkArea.Message;
                UseCabErrorReport2();

                // -- Set Abort exit state and escape...
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              UseExtToDoACommit();

              if (local.External.NumericReturnCode != 0)
              {
                local.EabReportSend.RptDetail =
                  "(01) Error in External Commit Routine.  Return Code = " + NumberToString
                  (local.External.NumericReturnCode, 14, 2);
                UseCabErrorReport2();

                // -- Set Abort exit state and escape...
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }
          }

          // -------------------------------------------------------------------------------------------------------------------------
          // --  If the collection date is before the report period start date 
          // then insure that the cash receipt detail has never
          // --  been adjusted and that there are no adjusted collections for 
          // the cash receipt detail.
          // -------------------------------------------------------------------------------------------------------------------------
          if (Lt(entities.Collection.CollectionDt,
            local.ReportingPeriodStarting.Date))
          {
            // --  Check for prior adjusted collections for the cash receipt 
            // detail.
            if (ReadCollection())
            {
              continue;
            }
            else
            {
              // -- Continue.
            }

            // --  Check for prior adjustment to the cash receipt detail.
            if (ReadCashReceiptDetailBalanceAdj1())
            {
              continue;
            }
            else
            {
              // -- Continue.
            }

            if (ReadCashReceiptDetailBalanceAdj2())
            {
              continue;
            }
            else
            {
              // -- Continue.
            }
          }

          // -------------------------------------------------------------------------------------------------------------------------
          // --  Determine the AR for the collection.
          // -------------------------------------------------------------------------------------------------------------------------
          UseFnB663DetermineAr();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -- Error message(s) for AR not found, closed case, etc....
            // --  Increment the error count.
            ++local.NumberOfErrors.Count;

            // --  Write identifying info to error file...
            if (IsEmpty(local.ArCsePerson.Number))
            {
              local.EabReportSend.RptDetail = "Error determining AR for " + entities
                .Collection.ProgramAppliedTo;
            }
            else
            {
              local.EabReportSend.RptDetail = "AR=" + local.ArCsePerson.Number;
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + " derived for " + entities
                .Collection.ProgramAppliedTo;
            }

            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " collection.  Obligor=";
              
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + entities
              .Obligor1.Number;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ", obligation=";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              (entities.Obligation.SystemGeneratedIdentifier, 13, 3);

            if (!Equal(local.DebtDetail.DueDt, local.Null1.Date))
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + ", debt detail due date=";
                
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + NumberToString
                (DateToInt(local.DebtDetail.DueDt), 8, 8);
            }

            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ", coll date=";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              (DateToInt(entities.Collection.CollectionDt), 8, 8);
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // -- Set Abort exit state and escape...
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            // -- Extract the exit state message and write to the error report.
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = "        " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // -- Set Abort exit state and escape...
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            // -- Reset exit state.
            ExitState = "ACO_NN0000_ALL_OK";

            // -- Skip the collection.
            continue;
          }

          // --  Exclude the collection if the AR is an organization.
          if (AsChar(local.ArCsePerson.Type1) == 'O')
          {
            continue;
          }

          local.ArCsePersonsWorkSet.Number = local.ArCsePerson.Number;
          UseEabReadCsePersonBatch3();

          if (IsEmpty(local.AbendData.Type1))
          {
            // -- Successful Adabas read occurred.
          }
          else
          {
            // --  Increment the error count.
            ++local.NumberOfErrors.Count;

            switch(AsChar(local.AbendData.Type1))
            {
              case 'A':
                // -- Unsuccessful Adabas read occurred.
                switch(TrimEnd(local.AbendData.AdabasResponseCd))
                {
                  case "0113":
                    local.EabReportSend.RptDetail =
                      "Adabas response code 113, Obligor cse person number " + entities
                      .Obligor1.Number + " not found in Adabas.";

                    break;
                  case "0148":
                    local.EabReportSend.RptDetail =
                      "Adabas response code 148, Adabas unavailable.  Obligor cse person number " +
                      entities.Obligor1.Number + ".";

                    break;
                  case "0000":
                    local.EabReportSend.RptDetail =
                      "Adabas response code 0000, Obligor cse person number " +
                      entities.Obligor1.Number + " expected record not returned from Adabas.";
                      

                    break;
                  default:
                    local.EabReportSend.RptDetail =
                      "Adabas error, response code = " + local
                      .AbendData.AdabasResponseCd + ", type = " + local
                      .AbendData.Type1 + ", Obligor cse person number = " + entities
                      .Obligor1.Number;

                    break;
                }

                break;
              case 'C':
                // -- CICS action failed.
                local.EabReportSend.RptDetail =
                  "CICS error, response code = " + local
                  .AbendData.CicsResponseCd + ", for Obligor cse person number = " +
                  entities.Obligor1.Number;

                break;
              default:
                // -- Action failed.
                local.EabReportSend.RptDetail =
                  "Unknown Adabas error, type = " + local.AbendData.Type1 + ", for Obligor cse person number = " +
                  entities.Obligor1.Number;

                break;
            }

            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }

            if (AsChar(local.AbendData.Type1) == 'A' && (
              Equal(local.AbendData.AdabasResponseCd, "0113") || Equal
              (local.AbendData.AdabasResponseCd, "0000")))
            {
              // -- No need to abend if the AR is not found on Adabas, just log 
              // to the error file.
              continue;
            }
            else
            {
              // -- Any errors beside the AR not being found on Adabas should 
              // abend.
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }

          // --  If we're processing for a single AR, insure that this 
          // collection is for that AR.
          if (!IsEmpty(local.ReportingAr.Number) && !
            Equal(local.ReportingAr.Number, local.ArCsePerson.Number))
          {
            // --  The collection was not for the specified AR.  Skip this 
            // collection.
            continue;
          }

          // --  At this point, the collection has passed all our edits.  
          // Prepare to write the collection info the extract file.
          switch(TrimEnd(entities.Collection.ProgramAppliedTo))
          {
            case "AF":
              ++local.NumberOfTafCollections.Count;
              local.ForwardedToFamily.Amount = 0;
              local.Retained.Amount = entities.Collection.Amount;

              break;
            case "NA":
              ++local.NumberOfNaCollections.Count;
              local.ForwardedToFamily.Amount = entities.Collection.Amount;
              local.Retained.Amount = 0;

              break;
            default:
              break;
          }

          // 05/02/05  GVandy PR242288  Do not send statement if the only 
          // collection activity is for 718B judgements.
          // --  Write obligation type of 0 to the extract file for 718B 
          // collections.  The sort/sum step will then sum
          // --  the obligation types on the ARs collections and B664 will not 
          // print a statement if the summed obligation
          // --  types for the AR equal 0 (meaning only 718B collections were 
          // found).
          if (entities.ObligationType.SystemGeneratedIdentifier == local
            .Local718B.SystemGeneratedIdentifier)
          {
            local.ObligationType.SystemGeneratedIdentifier = 0;
          }
          else
          {
            local.ObligationType.SystemGeneratedIdentifier = 1;
          }

          if (Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
            (entities.Collection.ProgramAppliedTo, "NA") && Equal
            (entities.Collection.DistPgmStateAppldTo, "CA"))
          {
            local.Taf.SystemGeneratedIdentifier = 1;
          }
          else
          {
            local.Taf.SystemGeneratedIdentifier = 0;
          }

          if (ReadCsePerson1())
          {
            local.Ch.Number = entities.Ch.Number;
            UseEabReadCsePersonBatch2();

            if (IsEmpty(local.AbendData.Type1))
            {
              // -- Successful Adabas read occurred.
            }
            else
            {
              // --  Increment the error count.
              ++local.NumberOfErrors.Count;

              switch(AsChar(local.AbendData.Type1))
              {
                case 'A':
                  // -- Unsuccessful Adabas read occurred.
                  switch(TrimEnd(local.AbendData.AdabasResponseCd))
                  {
                    case "0113":
                      local.EabReportSend.RptDetail =
                        "Adabas response code 113, Obligor cse person number " +
                        entities.Obligor1.Number + " not found in Adabas.";

                      break;
                    case "0148":
                      local.EabReportSend.RptDetail =
                        "Adabas response code 148, Adabas unavailable.  Obligor cse person number " +
                        entities.Obligor1.Number + ".";

                      break;
                    case "0000":
                      local.EabReportSend.RptDetail =
                        "Adabas response code 0000, Obligor cse person number " +
                        entities.Obligor1.Number + " expected record not returned from Adabas.";
                        

                      break;
                    default:
                      local.EabReportSend.RptDetail =
                        "Adabas error, response code = " + local
                        .AbendData.AdabasResponseCd + ", type = " + local
                        .AbendData.Type1 + ", Obligor cse person number = " + entities
                        .Obligor1.Number;

                      break;
                  }

                  break;
                case 'C':
                  // -- CICS action failed.
                  local.EabReportSend.RptDetail =
                    "CICS error, response code = " + local
                    .AbendData.CicsResponseCd + ", for Obligor cse person number = " +
                    entities.Obligor1.Number;

                  break;
                default:
                  // -- Action failed.
                  local.EabReportSend.RptDetail =
                    "Unknown Adabas error, type = " + local.AbendData.Type1 + ", for Obligor cse person number = " +
                    entities.Obligor1.Number;

                  break;
              }

              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

                return;
              }

              if (AsChar(local.AbendData.Type1) == 'A' && (
                Equal(local.AbendData.AdabasResponseCd, "0113") || Equal
                (local.AbendData.AdabasResponseCd, "0000")))
              {
                // -- No need to abend if the AR is not found on Adabas, just 
                // log to the error file.
                continue;
              }
              else
              {
                // -- Any errors beside the AR not being found on Adabas should 
                // abend.
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }
          }

          local.EabFileHandling.Action = "WRITE";

          // --  Write the collection info to the extract file.
          UseFnB680ArExtractData2();

          if (!Equal(local.External.TextReturnCode, "OK"))
          {
            // --  write to error file...
            local.EabReportSend.RptDetail =
              "(01) Error writing collection info to extract file...  Returned Status = " +
              local.EabFileHandling.Status;
            UseCabErrorReport2();
            ExitState = "ERROR_WRITING_TO_FILE_AB";

            return;
          }
        }
      }
      while(local.Group.Index + 1 < local.Group.Count && !
        IsEmpty(local.ReportingAr.Number));
    }

Test:

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Take a final Checkpoint.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Final Checkpoint Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      local.EabReportSend.RptDetail =
        "(04) Error in External Commit Routine.  Return Code = " + NumberToString
        (local.External.NumericReturnCode, 14, 2);
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseFnB680ArExtractData3();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Extract File...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private static void MoveProgramCheckpointRestart1(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.RestartInd = source.RestartInd;
  }

  private static void MoveProgramCheckpointRestart2(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonBatch1()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ObligorCsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(local.ObligorCsePersonsWorkSet,
      useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.ObligorCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabReadCsePersonBatch2()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ch.Number;
    MoveCsePersonsWorkSet(local.Ch, useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.Ch.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabReadCsePersonBatch3()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ArCsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(local.ArCsePersonsWorkSet, useExport.CsePersonsWorkSet);
      
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.ArCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB663DetermineAr()
  {
    var useImport = new FnB663DetermineAr.Import();
    var useExport = new FnB663DetermineAr.Export();

    useImport.Obligor.Number = entities.Obligor1.Number;
    useImport.PersistentCollection.Assign(entities.Collection);
    useImport.PersistentDebt.Assign(entities.Debt);
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.SpousalSupport.SystemGeneratedIdentifier =
      local.SpousalSupport.SystemGeneratedIdentifier;
    useImport.SpousalArrearsJudgement.SystemGeneratedIdentifier =
      local.SpousalArrearsJudgement.SystemGeneratedIdentifier;
    useImport.Voluntary.SystemGeneratedIdentifier =
      local.Voluntary.SystemGeneratedIdentifier;

    Call(FnB663DetermineAr.Execute, useImport, useExport);

    local.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    MoveCsePerson(useExport.Ar, local.ArCsePerson);
  }

  private void UseFnB680ArExtractData2()
  {
    var useImport = new FnB680ArExtractData1.Import();
    var useExport = new FnB680ArExtractData1.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ArCsePerson.Number = local.ArCsePerson.Number;
    useImport.ObligorCsePersonsWorkSet.LastName =
      local.ObligorCsePersonsWorkSet.LastName;
    useImport.ObligorCsePerson.Number = entities.Obligor1.Number;
    MoveCollection(entities.Collection, useImport.Collection);
    useImport.Retained.Amount = local.Retained.Amount;
    useImport.ForwardedToFamily.Amount = local.ForwardedToFamily.Amount;
    useImport.Obligor.Type1 = entities.Obligor2.Type1;
    useImport.ObligationType.SystemGeneratedIdentifier =
      local.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    MoveObligationTransaction(entities.Debt, useImport.ObligationTransaction);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.ChPerson.Assign(local.Ch);
    useImport.ArCsePersonsWorkSet.Assign(local.ArCsePersonsWorkSet);
    useImport.Taf.SystemGeneratedIdentifier =
      local.Taf.SystemGeneratedIdentifier;
    MoveExternal(local.External, useExport.External);

    Call(FnB680ArExtractData1.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnB680ArExtractData3()
  {
    var useImport = new FnB680ArExtractData1.Import();
    var useExport = new FnB680ArExtractData1.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveExternal(local.External, useExport.External);

    Call(FnB680ArExtractData1.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnB680BatchInitialization()
  {
    var useImport = new FnB680BatchInitialization.Import();
    var useExport = new FnB680BatchInitialization.Export();

    Call(FnB680BatchInitialization.Execute, useImport, useExport);

    local.ReportingAr.Number = useExport.Ar.Number;
    MoveDateWorkArea(useExport.ReportingPeriodEnding,
      local.ReportingPeriodEnding);
    MoveDateWorkArea(useExport.ReportingPeriodStarting,
      local.ReportingPeriodStarting);
    MoveProgramCheckpointRestart1(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.Restart.Number = useExport.Restart.Number;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.Local718B.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.SpousalSupport.SystemGeneratedIdentifier =
      useExport.OtSpousalSupport.SystemGeneratedIdentifier;
    local.SpousalArrearsJudgement.SystemGeneratedIdentifier =
      useExport.OtSpousalArrearsJudgement.SystemGeneratedIdentifier;
    local.Voluntary.SystemGeneratedIdentifier =
      useExport.OtVoluntary.SystemGeneratedIdentifier;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    MoveProgramCheckpointRestart2(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadCashReceiptDetailBalanceAdj1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailBalanceAdj.Populated = false;

    return Read("ReadCashReceiptDetailBalanceAdj1",
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
        entities.CashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CashReceiptDetailBalanceAdj.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailBalanceAdj2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailBalanceAdj.Populated = false;

    return Read("ReadCashReceiptDetailBalanceAdj2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdSIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvSIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstSIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtSIdentifier", entities.CashReceiptDetail.CrtIdentifier);
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
        entities.CashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CashReceiptDetailBalanceAdj.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptType()
  {
    entities.CashIndicatorCashReceiptType.Populated = false;

    return ReadEach("ReadCashReceiptType",
      null,
      (db, reader) =>
      {
        entities.CashIndicatorCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashIndicatorCashReceiptType.CategoryIndicator =
          db.GetString(reader, 1);
        entities.CashIndicatorCashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashIndicatorCashReceiptType.CategoryIndicator);

        return true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Adjusted.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Adjusted.CrtType = db.GetInt32(reader, 2);
        entities.Adjusted.CstId = db.GetInt32(reader, 3);
        entities.Adjusted.CrvId = db.GetInt32(reader, 4);
        entities.Adjusted.CrdId = db.GetInt32(reader, 5);
        entities.Adjusted.ObgId = db.GetInt32(reader, 6);
        entities.Adjusted.CspNumber = db.GetString(reader, 7);
        entities.Adjusted.CpaType = db.GetString(reader, 8);
        entities.Adjusted.OtrId = db.GetInt32(reader, 9);
        entities.Adjusted.OtrType = db.GetString(reader, 10);
        entities.Adjusted.OtyId = db.GetInt32(reader, 11);
        entities.Adjusted.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Adjusted.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Adjusted.CpaType);
        CheckValid<Collection>("OtrType", entities.Adjusted.OtrType);
      });
  }

  private IEnumerable<bool> ReadCollectionCashReceiptDetailCashReceiptEvent()
  {
    entities.Obligor1.Populated = false;
    entities.Obligor2.Populated = false;
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.Obligation.Populated = false;
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;
    entities.ObligationType.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;

    return ReadEach("ReadCollectionCashReceiptDetailCashReceiptEvent",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.ReportingPeriodStarting.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.ReportingPeriodEnding.Timestamp.GetValueOrDefault());
        db.SetString(command, "number", local.ObligorCsePerson.Number);
        db.SetString(command, "cspNumber", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 6);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 8);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Obligor1.Number = db.GetString(reader, 9);
        entities.Debt.CspNumber = db.GetString(reader, 9);
        entities.Debt.CspNumber = db.GetString(reader, 9);
        entities.Obligor2.CspNumber = db.GetString(reader, 9);
        entities.Obligor2.CspNumber = db.GetString(reader, 9);
        entities.Obligor2.CspNumber = db.GetString(reader, 9);
        entities.Obligation.CspNumber = db.GetString(reader, 9);
        entities.Obligation.CspNumber = db.GetString(reader, 9);
        entities.Obligation.CspNumber = db.GetString(reader, 9);
        entities.Obligation.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Debt.CpaType = db.GetString(reader, 10);
        entities.Obligor2.Type1 = db.GetString(reader, 10);
        entities.Obligor2.Type1 = db.GetString(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Debt.Type1 = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Debt.OtyType = db.GetInt32(reader, 13);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 13);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 13);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 17);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 19);
        entities.Collection.ArNumber = db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 21);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 22);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 23);
        entities.Obligor1.Populated = true;
        entities.Obligor2.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.Obligation.Populated = true;
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;
        entities.ObligationType.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor2.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor2.Type1);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadCollectionType1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollectionType2()
  {
    entities.CashIndicatorCollectionType.Populated = false;

    return ReadEach("ReadCollectionType2",
      null,
      (db, reader) =>
      {
        entities.CashIndicatorCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CashIndicatorCollectionType.CashNonCashInd =
          db.GetString(reader, 1);
        entities.CashIndicatorCollectionType.Populated = true;
        CheckValid<CollectionType>("CashNonCashInd",
          entities.CashIndicatorCollectionType.CashNonCashInd);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Ch.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Ch.Number = db.GetString(reader, 0);
        entities.Ch.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.ReportingAr.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.Obligor1.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligor1.Populated = true;

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
    /// <summary>A GlocalCashIndicatorGroup group.</summary>
    [Serializable]
    public class GlocalCashIndicatorGroup
    {
      /// <summary>
      /// A value of GcollectionType.
      /// </summary>
      [JsonPropertyName("gcollectionType")]
      public CollectionType GcollectionType
      {
        get => gcollectionType ??= new();
        set => gcollectionType = value;
      }

      /// <summary>
      /// A value of GcashReceiptType.
      /// </summary>
      [JsonPropertyName("gcashReceiptType")]
      public CashReceiptType GcashReceiptType
      {
        get => gcashReceiptType ??= new();
        set => gcashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CollectionType gcollectionType;
      private CashReceiptType gcashReceiptType;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GlocalObligor.
      /// </summary>
      [JsonPropertyName("glocalObligor")]
      public CsePerson GlocalObligor
      {
        get => glocalObligor ??= new();
        set => glocalObligor = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson glocalObligor;
    }

    /// <summary>
    /// A value of Taf.
    /// </summary>
    [JsonPropertyName("taf")]
    public ObligationType Taf
    {
      get => taf ??= new();
      set => taf = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePersonsWorkSet Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of Local718B.
    /// </summary>
    [JsonPropertyName("local718B")]
    public ObligationType Local718B
    {
      get => local718B ??= new();
      set => local718B = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of ReportingAr.
    /// </summary>
    [JsonPropertyName("reportingAr")]
    public CsePerson ReportingAr
    {
      get => reportingAr ??= new();
      set => reportingAr = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEnding.
    /// </summary>
    [JsonPropertyName("reportingPeriodEnding")]
    public DateWorkArea ReportingPeriodEnding
    {
      get => reportingPeriodEnding ??= new();
      set => reportingPeriodEnding = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStarting.
    /// </summary>
    [JsonPropertyName("reportingPeriodStarting")]
    public DateWorkArea ReportingPeriodStarting
    {
      get => reportingPeriodStarting ??= new();
      set => reportingPeriodStarting = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of SpousalSupport.
    /// </summary>
    [JsonPropertyName("spousalSupport")]
    public ObligationType SpousalSupport
    {
      get => spousalSupport ??= new();
      set => spousalSupport = value;
    }

    /// <summary>
    /// A value of SpousalArrearsJudgement.
    /// </summary>
    [JsonPropertyName("spousalArrearsJudgement")]
    public ObligationType SpousalArrearsJudgement
    {
      get => spousalArrearsJudgement ??= new();
      set => spousalArrearsJudgement = value;
    }

    /// <summary>
    /// A value of Voluntary.
    /// </summary>
    [JsonPropertyName("voluntary")]
    public ObligationType Voluntary
    {
      get => voluntary ??= new();
      set => voluntary = value;
    }

    /// <summary>
    /// Gets a value of GlocalCashIndicator.
    /// </summary>
    [JsonIgnore]
    public Array<GlocalCashIndicatorGroup> GlocalCashIndicator =>
      glocalCashIndicator ??= new(GlocalCashIndicatorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of GlocalCashIndicator for json serialization.
    /// </summary>
    [JsonPropertyName("glocalCashIndicator")]
    [Computed]
    public IList<GlocalCashIndicatorGroup> GlocalCashIndicator_Json
    {
      get => glocalCashIndicator;
      set => GlocalCashIndicator.Assign(value);
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of NumberOfErrors.
    /// </summary>
    [JsonPropertyName("numberOfErrors")]
    public Common NumberOfErrors
    {
      get => numberOfErrors ??= new();
      set => numberOfErrors = value;
    }

    /// <summary>
    /// A value of ReadCount.
    /// </summary>
    [JsonPropertyName("readCount")]
    public Common ReadCount
    {
      get => readCount ??= new();
      set => readCount = value;
    }

    /// <summary>
    /// A value of PreviousObligor.
    /// </summary>
    [JsonPropertyName("previousObligor")]
    public CsePerson PreviousObligor
    {
      get => previousObligor ??= new();
      set => previousObligor = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of NumberOfTafCollections.
    /// </summary>
    [JsonPropertyName("numberOfTafCollections")]
    public Common NumberOfTafCollections
    {
      get => numberOfTafCollections ??= new();
      set => numberOfTafCollections = value;
    }

    /// <summary>
    /// A value of ForwardedToFamily.
    /// </summary>
    [JsonPropertyName("forwardedToFamily")]
    public Collection ForwardedToFamily
    {
      get => forwardedToFamily ??= new();
      set => forwardedToFamily = value;
    }

    /// <summary>
    /// A value of Retained.
    /// </summary>
    [JsonPropertyName("retained")]
    public Collection Retained
    {
      get => retained ??= new();
      set => retained = value;
    }

    /// <summary>
    /// A value of NumberOfNaCollections.
    /// </summary>
    [JsonPropertyName("numberOfNaCollections")]
    public Common NumberOfNaCollections
    {
      get => numberOfNaCollections ??= new();
      set => numberOfNaCollections = value;
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

    private ObligationType taf;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePersonsWorkSet ch;
    private ObligationType local718B;
    private ObligationType obligationType;
    private EabFileHandling eabFileHandling;
    private CsePerson reportingAr;
    private DateWorkArea reportingPeriodEnding;
    private DateWorkArea reportingPeriodStarting;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restart;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private ObligationType spousalSupport;
    private ObligationType spousalArrearsJudgement;
    private ObligationType voluntary;
    private Array<GlocalCashIndicatorGroup> glocalCashIndicator;
    private Common common;
    private Array<GroupGroup> group;
    private CsePerson obligorCsePerson;
    private Common numberOfErrors;
    private Common readCount;
    private CsePerson previousObligor;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private AbendData abendData;
    private External external;
    private DebtDetail debtDetail;
    private CsePerson arCsePerson;
    private DateWorkArea null1;
    private Common numberOfTafCollections;
    private Collection forwardedToFamily;
    private Collection retained;
    private Common numberOfNaCollections;
    private Common counter;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of CashIndicatorCashReceiptType.
    /// </summary>
    [JsonPropertyName("cashIndicatorCashReceiptType")]
    public CashReceiptType CashIndicatorCashReceiptType
    {
      get => cashIndicatorCashReceiptType ??= new();
      set => cashIndicatorCashReceiptType = value;
    }

    /// <summary>
    /// A value of CashIndicatorCollectionType.
    /// </summary>
    [JsonPropertyName("cashIndicatorCollectionType")]
    public CollectionType CashIndicatorCollectionType
    {
      get => cashIndicatorCollectionType ??= new();
      set => cashIndicatorCollectionType = value;
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
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public Collection Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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

    private CsePerson ch;
    private CsePersonAccount supported;
    private CashReceiptType cashIndicatorCashReceiptType;
    private CollectionType cashIndicatorCollectionType;
    private CsePerson ar;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private CaseRole caseRole;
    private Case1 case1;
    private CaseRole applicantRecipient;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private Obligation obligation;
    private Collection adjusted;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
    private Collection collection;
    private ObligationTransaction debt;
    private ObligationType obligationType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
  }
#endregion
}
