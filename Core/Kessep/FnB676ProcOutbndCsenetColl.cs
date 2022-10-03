// Program: FN_B676_PROC_OUTBND_CSENET_COLL, ID: 372449381, model: 746.
// Short name: SWEF676B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B676_PROC_OUTBND_CSENET_COLL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB676ProcOutbndCsenetColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B676_PROC_OUTBND_CSENET_COLL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB676ProcOutbndCsenetColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB676ProcOutbndCsenetColl.
  /// </summary>
  public FnB676ProcOutbndCsenetColl(IContext context, Import import,
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
    // * *   A Change in this TRAN - MAY - require a similar change
    // * *   in A/B FN_PROCESS_DISTRIBUTION_REQUEST (SWE02365)
    // * *     * This is NO LONGER true after implementation of 159319
    // ****************************************************************
    // I00122005      01/23/01    P.PHINNEY  - REMOVE BLOCK ON  
    // Interstate_Request_history
    // Which was Limited to "ERALL", "ERARR", and "EREXO"
    // I00117075      04/04/01    P.PHINNEY  - Pass Date of Collection
    // I00116873      04/10/01    P.PHINNEY  - Display COL on IREQ Screen
    // I00116873      04/10/01    P.PHINNEY  - Display COL on IREQ Screen
    // I00122438      08/13/2001  P.Phinney  - Verify that State is CSEnet ready
    // PR 137528      02/07/02    M.Ashworth - Changed action reason code from 
    // existing Int Req History to 'CITAX' so it will display on IREQ Screen.
    // I00141745      03/18/2002  P.Phinney  - Verify that State is CSEnet ready
    // - Check BOTH Flags
    // I00159319      11/15/02    P.Phinney  - COMPLETELY redid logic because 
    // program was not working correctly
    // --------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.UserId.Text8 = global.UserId;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // * * CseNet was stopped on 10/18/2002 for this major change to processing 
    // logic
    // * *  Anything before this should have already been processed
    local.RestartCsenetProcessing.CollectionDate = new DateTime(2002, 10, 17);
    local.CheckYesCollection.AdjustedInd = "Y";
    local.CheckYesCashReceiptDetail.CollectionAmtFullyAppliedInd = "Y";
    local.CheckOutgoingOpen.KsCaseInd = "Y";
    local.CheckOutgoingOpen.OtherStateCaseStatus = "O";
    local.ProcessCount.TotalInteger = 0;
    local.No.Flag = "N";
    local.Yes.Flag = "Y";
    UseFnB676Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    local.ForUpdate.LastUpdatedTmst = Now();
    local.ForUpdate.LastUpdatedBy = global.UserId;
    local.CheckStateInterstateRequest.OtherStateFips = 0;

    // * * Read OUTGOING/Open Interstate Request for this AP
    foreach(var item in ReadInterstateRequestCaseCsePersonLegalAction())
    {
      // * * Only ONE Notification per Obligor/State/Case
      if (entities.ExistingInterstateRequest.OtherStateFips == local
        .CheckStateInterstateRequest.OtherStateFips && Equal
        (entities.ExistingObligor1.Number, local.CheckStateCsePerson.Number) &&
        Equal(entities.ExistingCase.Number, local.CheckStateCase.Number))
      {
        continue;
      }

      // --------------------------------------------
      // 08/13/01 I00122438 - check Interstate Request State against CSEnet 
      // ready states
      // --------------------------------------------
      // IF the State is NOT Valid
      // OR the State Does NOT Recieve CSENET from us
      // BYPASS this Interstate Request
      // --------------------------------------------
      if (ReadFips())
      {
        local.CsenetStateTable.StateCode = entities.Fips.StateAbbreviation;
        UseSiReadCsenetStateTable();

        if (IsExitState("CO0000_CSENET_STATE_NF"))
        {
          // --------------------------------------------
          // The State is NOT Valid
          // BYPASS this Interstate Request
          // --------------------------------------------
          local.ValidCsenetState.Flag = local.No.Flag;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }

        // I00141745      03/18/2002  P.Phinney  - Verify that State is CSEnet 
        // ready
        // -- Check BOTH Flags
        if (AsChar(local.CsenetStateTable.RecStateInd) != AsChar
          (local.Yes.Flag) || AsChar(local.CsenetStateTable.CsenetReadyInd) != AsChar
          (local.Yes.Flag))
        {
          // --------------------------------------------
          // The State Does NOT Recieve CSENET from us
          // BYPASS this Interstate Request
          // --------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          local.ValidCsenetState.Flag = local.No.Flag;

          continue;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        local.ValidCsenetState.Flag = local.Yes.Flag;
      }
      else
      {
        // --------------------------------------------
        // The State is NOT Valid
        // BYPASS this Interstate Request
        // --------------------------------------------
        local.ValidCsenetState.Flag = local.No.Flag;

        continue;
      }

      // * * Initialize Starting Date to TOMORROW
      local.Starting.CollectionDt = AddDays(local.Current.Date, 1);
      local.HistoryRecFndInd.Flag = local.No.Flag;

      // I00122005      01/23/01    P.PHINNEY  - REMOVE BLOCK ON  
      // Interstate_Request_history
      // Which was Limited to "ERALL", "ERARR", and "EREXO"
      foreach(var item1 in ReadInterstateRequestHistory())
      {
        if (Equal(entities.ExistingInterstateRequestHistory.ActionReasonCode,
          "OICNV"))
        {
          // * * Set Starting Date to ENFORCEMENT Starting Date
          local.Starting.CollectionDt =
            entities.ExistingInterstateRequestHistory.TransactionDate;
          local.HistoryRecFndInd.Flag = local.Yes.Flag;

          break;
        }

        // * * If "C"ancelled - will NOT Accumulate Totals or Create CITAX
        // * * If "R"equest - will Accumulate Totals and Create CITAX
        // * * If "Anything Else" - will NOT Accumulate Totals or Create CITAX
        switch(AsChar(entities.ExistingInterstateRequestHistory.ActionCode))
        {
          case 'C':
            // "C"ancelled
            // * * Get NEXT Interstate Request
            goto ReadEach2;
          case 'R':
            // "R"equest - Continue Processing
            // * * Set Starting Date to ENFORCEMENT Starting Date
            local.Starting.CollectionDt =
              entities.ExistingInterstateRequestHistory.TransactionDate;
            local.HistoryRecFndInd.Flag = local.Yes.Flag;

            goto ReadEach1;
          default:
            // ANYTHING ELSE - Get NEXT Interstate Request History
            // * * TRY to find a "C" or "R"
            continue;
        }
      }

ReadEach1:

      // * * NO Valid History Record - Get NEXT Interstate Request
      if (AsChar(local.HistoryRecFndInd.Flag) == AsChar(local.No.Flag))
      {
        continue;
      }

      local.CheckAdjustedLiteral.AdjustedInd = local.Yes.Flag;

      // * * Read for a  CRD that can be processed
      foreach(var item1 in ReadCashReceiptDetail())
      {
        // * * A FULLY applied CRD was found - Check Collection Type
        if (ReadCollectionType())
        {
          // I00122005      01/23/01    P.PHINNEY  - Change to Validate correct 
          // Tax Intercept Values
          // ONLY  Federal Offset and State Offset records are processed
          switch(entities.ExistingCollectionType.SequentialIdentifier)
          {
            case 3:
              // FEDERAL Offset
              // I00122005      01/23/01    P.PHINNEY  - Use  "F" for Federal
              // This is then changed to "I" in SWEIB290  Format Outgoing
              // I00122005      01/23/01    P.PHINNEY  - Set payment_menthod to
              // "O"   (OTHER)
              local.PassInterstateCollection.InterstatePaymentMethod = "O";
              local.PassInterstateCollection.PaymentSource = "F";

              break;
            case 19:
              // FEDERAL Offset     (Treasury Offset - SALARY)
              // I00122005      01/23/01    P.PHINNEY  - Use  "F" for Federal
              // This is then changed to "I" in SWEIB290  Format Outgoing
              // I00122005      01/23/01    P.PHINNEY  - Set payment_menthod to
              // "O"   (OTHER)
              local.PassInterstateCollection.InterstatePaymentMethod = "O";
              local.PassInterstateCollection.PaymentSource = "F";

              break;
            case 25:
              // FEDERAL Offset     (Treasury Offset - RETIREMENT)
              // I00122005      01/23/01    P.PHINNEY  - Use  "F" for Federal
              // This is then changed to "I" in SWEIB290  Format Outgoing
              // I00122005      01/23/01    P.PHINNEY  - Set payment_menthod to
              // "O"   (OTHER)
              local.PassInterstateCollection.InterstatePaymentMethod = "O";
              local.PassInterstateCollection.PaymentSource = "F";

              break;
            case 26:
              // FEDERAL Offset     (Treasury Offset - VENDOR)
              // I00122005      01/23/01    P.PHINNEY  - Use  "F" for Federal
              // This is then changed to "I" in SWEIB290  Format Outgoing
              // I00122005      01/23/01    P.PHINNEY  - Set payment_menthod to
              // "O"   (OTHER)
              local.PassInterstateCollection.InterstatePaymentMethod = "O";
              local.PassInterstateCollection.PaymentSource = "F";

              break;
            case 4:
              // STATE Offset       (MISC)
              // I00122005      01/23/01    P.PHINNEY  - Set payment_menthod to
              // "O"   (OTHER)
              local.PassInterstateCollection.InterstatePaymentMethod = "O";
              local.PassInterstateCollection.PaymentSource = "S";

              break;
            case 5:
              // STATE Offset       (UNEMPLOYMENT)
              // I00122005      01/23/01    P.PHINNEY  - Set payment_menthod to
              // "O"   (OTHER)
              local.PassInterstateCollection.InterstatePaymentMethod = "O";
              local.PassInterstateCollection.PaymentSource = "S";

              break;
            case 10:
              // STATE Offset       (KPERS)
              // I00122005      01/23/01    P.PHINNEY  - Set payment_menthod to
              // "O"   (OTHER)
              local.PassInterstateCollection.InterstatePaymentMethod = "O";
              local.PassInterstateCollection.PaymentSource = "S";

              break;
            case 11:
              // STATE Offset       (RECOVERY)
              // PR 159319      BYPASS Recovery
              continue;
            default:
              // ONLY  Federal Offset and State Offset records are processed
              continue;
          }
        }
        else
        {
          // ONLY  Federal Offset and State Offset records are processed
          continue;
        }

        // * * Initialize the CRD Level - COLLECTION Counters
        local.CrdTotalCsenetColl.Count = 0;
        local.CrdTotalCsenetColl.TotalCurrency = 0;
        local.CrdTotalCsenetAdj.Count = 0;
        local.CrdTotalCsenetAdj.TotalCurrency = 0;

        // * * Mark the Collections as PROCESSED
        foreach(var item2 in ReadCollection())
        {
          // I00117075      04/04/01    P.PHINNEY  - Pass Date of Collection
          // * * IF Multiples - will use Most Current Date
          local.PassInterstateCollection.DateOfCollection =
            entities.ExistingCollection.CollectionDt;
          local.ForUpdate.CsenetOutboundReqInd = local.No.Flag;

          // Set DATES for COLLECTION Update
          if (Equal(entities.ExistingCollection.CsenetOutboundProcDt,
            local.Null1.Date))
          {
            local.ForUpdate.CsenetOutboundProcDt = local.Current.Date;
          }
          else
          {
            local.ForUpdate.CsenetOutboundProcDt =
              entities.ExistingCollection.CsenetOutboundProcDt;
          }

          if (Equal(entities.ExistingCollection.CsenetOutboundAdjProjDt,
            local.Null1.Date) && AsChar
            (entities.ExistingCollection.AdjustedInd) == AsChar
            (local.CheckAdjustedLiteral.AdjustedInd))
          {
            local.ForUpdate.CsenetOutboundAdjProjDt = local.Current.Date;
          }
          else
          {
            local.ForUpdate.CsenetOutboundAdjProjDt =
              entities.ExistingCollection.CsenetOutboundAdjProjDt;
          }

          // Accumulate TOTALs for CITAX Tran
          ++local.PrintLocalCollRead.TotalInteger;
          local.PrintLocalCollAmtRead.TotalCurrency += entities.
            ExistingCollection.Amount;

          // * * This will occur ONLY if an ENF/R or OINCV is found
          if (!Lt(entities.ExistingCollection.CollectionDt,
            local.Starting.CollectionDt))
          {
            // * * Accumulate CseNet Totals
            // * * Summarize COLLECTIONS
            if (Equal(entities.ExistingCollection.CsenetOutboundProcDt,
              local.Null1.Date))
            {
              ++local.PrintLocalCsenetProcessed.TotalCurrency;
              local.PrintLocalCsenetAmtProcessed.TotalCurrency += entities.
                ExistingCollection.Amount;
              local.CrdTotalCsenetColl.TotalCurrency += entities.
                ExistingCollection.Amount;
              ++local.CrdTotalCsenetColl.Count;
            }

            // * * Summarize ADJUSTMENTS
            if (Equal(entities.ExistingCollection.CsenetOutboundAdjProjDt,
              local.Null1.Date) && AsChar
              (entities.ExistingCollection.AdjustedInd) == AsChar
              (local.CheckAdjustedLiteral.AdjustedInd))
            {
              ++local.PrintLocalCsenetAdjProcessed.TotalCurrency;
              local.PrintLCsenetAdjAmtProcessed.TotalCurrency += entities.
                ExistingCollection.Amount;
              local.CrdTotalCsenetAdj.TotalCurrency += entities.
                ExistingCollection.Amount;
              local.CrdTotalCsenetAdj.Count = local.CrdTotalCsenetColl.Count + 1
                ;
            }
          }
          else
          {
            // * * Accumulate NON CseNet Totals
            ++local.PrintLocalNonCsenetProcessed.TotalInteger;
            local.PrintLNonCsenetAmtProcessed.TotalCurrency += entities.
              ExistingCollection.Amount;
          }

          // Update the COLLECTION so it will NOT be Processed in the Future
          try
          {
            UpdateCollection();
            ++local.ProcessCount.TotalInteger;

            continue;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_COLLECTION_NU";

                goto ReadEach3;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV";

                goto ReadEach3;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        // * * Calculate TOTAL Collection Amount for CITAX Tran
        local.PassInterstateCollection.PaymentAmount =
          local.CrdTotalCsenetColl.TotalCurrency - local
          .CrdTotalCsenetAdj.TotalCurrency;

        // * * NO Amount for CITAX Record - Get NEXT Interstate Request
        if (local.PassInterstateCollection.PaymentAmount.GetValueOrDefault() ==
          0)
        {
          continue;
        }

        // * * Create the CITAX Tran  VVVVVVVV
        // : Set the Interstate Payment Method & Payment Source.
        //   Only report collections from Fed or State Tax Intercepts.
        // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        local.PassInterstateCase.KsCaseId = entities.ExistingCase.Number;
        local.PassInterstateCase.InterstateCaseId =
          entities.ExistingInterstateRequest.OtherStateCaseId;
        local.PassInterstateCase.LocalFipsState = 20;
        local.PassInterstateCase.OtherFipsCounty = 0;
        local.PassInterstateCase.OtherFipsCounty = 0;
        local.PassInterstateCase.OtherFipsState =
          entities.ExistingInterstateRequest.OtherStateFips;
        local.PassInterstateCase.CaseType =
          entities.ExistingInterstateRequest.CaseType ?? Spaces(3);

        // I00116873      04/10/01    P.PHINNEY  - Display COL on IREQ Screen
        // Pass Interstate Request ID
        local.PassInterstateRequest.IntHGeneratedId =
          entities.ExistingInterstateRequest.IntHGeneratedId;
        UseFnCreateOutbndCsenetColl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto ReadEach3;
        }

        // * * Only ONE Notification per Obligor/State/Case
        local.CheckStateInterstateRequest.OtherStateFips =
          entities.ExistingInterstateRequest.OtherStateFips;
        local.CheckStateCsePerson.Number = entities.ExistingObligor1.Number;
        local.CheckStateCase.Number = entities.ExistingCase.Number;

        // * *  CheckPoint Processing
        if (local.ProcessCount.TotalInteger >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          UseExtToDoACommit();

          if (local.External.NumericReturnCode != 0)
          {
            ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

            goto ReadEach3;
          }

          local.ProcessCount.TotalInteger = 0;
        }
      }

ReadEach2:
      ;
    }

ReadEach3:

    // VVVVVVVV   OLD LOGIC -  EOJ REPORT's  VVVVVVV
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseFnB676ReportError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ++local.PrintLocalCollErrored.TotalInteger;
      local.PrintLocalCollAmtErrored.TotalCurrency += entities.
        ExistingCollection.Amount;
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "RUN RESULTS AS FOLLOWS:";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail = "";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Total Collections Read . . . . . . . . . . : " + NumberToString
      (local.PrintLocalCollRead.TotalInteger, 15) + "  -  $ " + NumberToString
      ((long)local.PrintLocalCollAmtRead.TotalCurrency, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Total CseNet Collections Processed . . . . : " + NumberToString
      ((long)local.PrintLocalCsenetProcessed.TotalCurrency, 15) + "  -  $ " + NumberToString
      ((long)local.PrintLocalCsenetAmtProcessed.TotalCurrency, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Total CseNet Adjustments Processed. . . . .: " + NumberToString
      ((long)local.PrintLocalCsenetAdjProcessed.TotalCurrency, 15) + "  -  $ " +
      NumberToString
      ((long)local.PrintLCsenetAdjAmtProcessed.TotalCurrency, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Total CseNet Collections In Error. . . . . : " + NumberToString
      (local.PrintLocalCollErrored.TotalInteger, 15) + "  -  $ " + NumberToString
      ((long)local.PrintLocalCollAmtErrored.TotalCurrency, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NeededToWrite.RptDetail =
      "Total Non-CseNet Collections Processed . . : " + NumberToString
      (local.PrintLocalNonCsenetProcessed.TotalInteger, 15) + "  -  $ " + NumberToString
      ((long)local.PrintLNonCsenetAmtProcessed.TotalCurrency, 15);
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.CollectionDt = source.CollectionDt;
    target.AdjustedInd = source.AdjustedInd;
    target.ConcurrentInd = source.ConcurrentInd;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.CsenetOutboundReqInd = source.CsenetOutboundReqInd;
    target.CsenetOutboundProcDt = source.CsenetOutboundProcDt;
    target.CsenetOutboundAdjProjDt = source.CsenetOutboundAdjProjDt;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB676Housekeeping()
  {
    var useImport = new FnB676Housekeeping.Import();
    var useExport = new FnB676Housekeeping.Export();

    Call(FnB676Housekeeping.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseFnB676ReportError()
  {
    var useImport = new FnB676ReportError.Import();
    var useExport = new FnB676ReportError.Export();

    useImport.Obligor.Number = entities.ExistingObligor1.Number;
    useImport.SuppPrsn.Number = entities.DelMeExistingSuppPrsn.Number;
    MoveCollection(entities.ExistingCollection, useImport.Persistant);

    Call(FnB676ReportError.Execute, useImport, useExport);
  }

  private void UseFnCreateOutbndCsenetColl()
  {
    var useImport = new FnCreateOutbndCsenetColl.Import();
    var useExport = new FnCreateOutbndCsenetColl.Export();

    useImport.Case1.Number = entities.ExistingCase.Number;
    useImport.InterstateCollection.Assign(local.PassInterstateCollection);
    useImport.InterstateRequest.IntHGeneratedId =
      local.PassInterstateRequest.IntHGeneratedId;
    useImport.InterstateRequestHistory.ActionReasonCode =
      local.PassInterstateRequestHistory.ActionReasonCode;
    useImport.InterstateCase.Assign(local.PassInterstateCase);
    useImport.UserId.Text8 = local.UserId.Text8;
    MoveDateWorkArea(local.Current, useImport.Current);

    Call(FnCreateOutbndCsenetColl.Execute, useImport, useExport);
  }

  private void UseSiReadCsenetStateTable()
  {
    var useImport = new SiReadCsenetStateTable.Import();
    var useExport = new SiReadCsenetStateTable.Export();

    useImport.CsenetStateTable.StateCode = local.CsenetStateTable.StateCode;

    Call(SiReadCsenetStateTable.Execute, useImport, useExport);

    local.CsenetStateTable.Assign(useExport.CsenetStateTable);
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.ExistingCashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingObligor1.Number);
        db.SetNullableString(
          command, "adjInd", local.CheckYesCollection.AdjustedInd ?? "");
        db.SetNullableDate(
          command, "csenetObAdjPDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaId", entities.ExistingLegalAction.Identifier);
        db.SetDate(
          command, "collectionDate",
          local.RestartCsenetProcessing.CollectionDate.GetValueOrDefault());
        db.SetNullableString(
          command, "collamtApplInd",
          local.CheckYesCashReceiptDetail.CollectionAmtFullyAppliedInd ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 4);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvId", entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstId", entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtType", entities.ExistingCashReceiptDetail.CrtIdentifier);
          
        db.SetNullableString(
          command, "adjInd", local.CheckAdjustedLiteral.AdjustedInd ?? "");
        db.SetNullableDate(
          command, "csenetObAdjPDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.CollectionDt = db.GetDate(reader, 1);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 2);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 3);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 4);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 6);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 7);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 8);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 9);
        entities.ExistingCollection.CpaType = db.GetString(reader, 10);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 11);
        entities.ExistingCollection.OtrType = db.GetString(reader, 12);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 13);
        entities.ExistingCollection.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingCollection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 16);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 17);
        entities.ExistingCollection.CsenetOutboundReqInd =
          db.GetString(reader, 18);
        entities.ExistingCollection.CsenetOutboundProcDt =
          db.GetNullableDate(reader, 19);
        entities.ExistingCollection.CsenetOutboundAdjProjDt =
          db.GetNullableDate(reader, 20);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);
        CheckValid<Collection>("CsenetOutboundReqInd",
          entities.ExistingCollection.CsenetOutboundReqInd);

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.ExistingCashReceiptDetail.CltIdentifier.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollectionType.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "state", entities.ExistingInterstateRequest.OtherStateFips);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestCaseCsePersonLegalAction()
  {
    entities.ExistingInterstateRequest.Populated = false;
    entities.ExistingObligor1.Populated = false;
    entities.ExistingCase.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadInterstateRequestCaseCsePersonLegalAction",
      (db, command) =>
      {
        db.SetString(command, "ksCaseInd", local.CheckOutgoingOpen.KsCaseInd);
        db.SetString(
          command, "othStCaseStatus",
          local.CheckOutgoingOpen.OtherStateCaseStatus);
      },
      (db, reader) =>
      {
        entities.ExistingInterstateRequest.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingInterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.ExistingInterstateRequest.OtherStateFips =
          db.GetInt32(reader, 2);
        entities.ExistingInterstateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingInterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 4);
        entities.ExistingInterstateRequest.CaseType =
          db.GetNullableString(reader, 5);
        entities.ExistingInterstateRequest.KsCaseInd = db.GetString(reader, 6);
        entities.ExistingInterstateRequest.CasINumber =
          db.GetNullableString(reader, 7);
        entities.ExistingCase.Number = db.GetString(reader, 7);
        entities.ExistingInterstateRequest.CasNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingInterstateRequest.CspNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingObligor1.Number = db.GetString(reader, 9);
        entities.ExistingInterstateRequest.CroType =
          db.GetNullableString(reader, 10);
        entities.ExistingInterstateRequest.CroId =
          db.GetNullableInt32(reader, 11);
        entities.ExistingCase.KsFipsCode = db.GetNullableString(reader, 12);
        entities.ExistingCase.InterstateCaseId =
          db.GetNullableString(reader, 13);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 14);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingInterstateRequest.Populated = true;
        entities.ExistingObligor1.Populated = true;
        entities.ExistingCase.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.ExistingInterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistory()
  {
    entities.ExistingInterstateRequestHistory.Populated = false;

    return ReadEach("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.ExistingInterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingInterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingInterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingInterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 2);
        entities.ExistingInterstateRequestHistory.ActionCode =
          db.GetString(reader, 3);
        entities.ExistingInterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 4);
        entities.ExistingInterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 5);
        entities.ExistingInterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 6);
        entities.ExistingInterstateRequestHistory.Populated = true;

        return true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);

    var lastUpdatedBy = local.ForUpdate.LastUpdatedBy ?? "";
    var lastUpdatedTmst = local.ForUpdate.LastUpdatedTmst;
    var csenetOutboundReqInd = local.ForUpdate.CsenetOutboundReqInd;
    var csenetOutboundProcDt = local.ForUpdate.CsenetOutboundProcDt;
    var csenetOutboundAdjProjDt = local.ForUpdate.CsenetOutboundAdjProjDt;

    CheckValid<Collection>("CsenetOutboundReqInd", csenetOutboundReqInd);
    entities.ExistingCollection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "csenetObReqInd", csenetOutboundReqInd);
        db.SetNullableDate(command, "csenetObPDt", csenetOutboundProcDt);
        db.SetNullableDate(command, "csenetObAdjPDt", csenetOutboundAdjProjDt);
        db.SetInt32(
          command, "collId",
          entities.ExistingCollection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.ExistingCollection.CrtType);
        db.SetInt32(command, "cstId", entities.ExistingCollection.CstId);
        db.SetInt32(command, "crvId", entities.ExistingCollection.CrvId);
        db.SetInt32(command, "crdId", entities.ExistingCollection.CrdId);
        db.SetInt32(command, "obgId", entities.ExistingCollection.ObgId);
        db.
          SetString(command, "cspNumber", entities.ExistingCollection.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingCollection.CpaType);
        db.SetInt32(command, "otrId", entities.ExistingCollection.OtrId);
        db.SetString(command, "otrType", entities.ExistingCollection.OtrType);
        db.SetInt32(command, "otyId", entities.ExistingCollection.OtyId);
      });

    entities.ExistingCollection.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCollection.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingCollection.CsenetOutboundReqInd = csenetOutboundReqInd;
    entities.ExistingCollection.CsenetOutboundProcDt = csenetOutboundProcDt;
    entities.ExistingCollection.CsenetOutboundAdjProjDt =
      csenetOutboundAdjProjDt;
    entities.ExistingCollection.Populated = true;
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
    /// <summary>A ZdelGroup group.</summary>
    [Serializable]
    public class ZdelGroup
    {
      /// <summary>
      /// A value of ZdelInterstateCase.
      /// </summary>
      [JsonPropertyName("zdelInterstateCase")]
      public InterstateCase ZdelInterstateCase
      {
        get => zdelInterstateCase ??= new();
        set => zdelInterstateCase = value;
      }

      /// <summary>
      /// A value of ZdelInterstateCollection.
      /// </summary>
      [JsonPropertyName("zdelInterstateCollection")]
      public InterstateCollection ZdelInterstateCollection
      {
        get => zdelInterstateCollection ??= new();
        set => zdelInterstateCollection = value;
      }

      /// <summary>
      /// A value of ZdelInterstateRequest.
      /// </summary>
      [JsonPropertyName("zdelInterstateRequest")]
      public InterstateRequest ZdelInterstateRequest
      {
        get => zdelInterstateRequest ??= new();
        set => zdelInterstateRequest = value;
      }

      /// <summary>
      /// A value of ZdelInterstateRequestHistory.
      /// </summary>
      [JsonPropertyName("zdelInterstateRequestHistory")]
      public InterstateRequestHistory ZdelInterstateRequestHistory
      {
        get => zdelInterstateRequestHistory ??= new();
        set => zdelInterstateRequestHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private InterstateCase zdelInterstateCase;
      private InterstateCollection zdelInterstateCollection;
      private InterstateRequest zdelInterstateRequest;
      private InterstateRequestHistory zdelInterstateRequestHistory;
    }

    /// <summary>
    /// A value of CheckStateCase.
    /// </summary>
    [JsonPropertyName("checkStateCase")]
    public Case1 CheckStateCase
    {
      get => checkStateCase ??= new();
      set => checkStateCase = value;
    }

    /// <summary>
    /// A value of CheckStateCsePerson.
    /// </summary>
    [JsonPropertyName("checkStateCsePerson")]
    public CsePerson CheckStateCsePerson
    {
      get => checkStateCsePerson ??= new();
      set => checkStateCsePerson = value;
    }

    /// <summary>
    /// A value of RestartCsenetProcessing.
    /// </summary>
    [JsonPropertyName("restartCsenetProcessing")]
    public CashReceiptDetail RestartCsenetProcessing
    {
      get => restartCsenetProcessing ??= new();
      set => restartCsenetProcessing = value;
    }

    /// <summary>
    /// A value of No.
    /// </summary>
    [JsonPropertyName("no")]
    public Common No
    {
      get => no ??= new();
      set => no = value;
    }

    /// <summary>
    /// A value of Yes.
    /// </summary>
    [JsonPropertyName("yes")]
    public Common Yes
    {
      get => yes ??= new();
      set => yes = value;
    }

    /// <summary>
    /// A value of CheckOutgoingOpen.
    /// </summary>
    [JsonPropertyName("checkOutgoingOpen")]
    public InterstateRequest CheckOutgoingOpen
    {
      get => checkOutgoingOpen ??= new();
      set => checkOutgoingOpen = value;
    }

    /// <summary>
    /// A value of PassInterstateCollection.
    /// </summary>
    [JsonPropertyName("passInterstateCollection")]
    public InterstateCollection PassInterstateCollection
    {
      get => passInterstateCollection ??= new();
      set => passInterstateCollection = value;
    }

    /// <summary>
    /// A value of PassInterstateRequest.
    /// </summary>
    [JsonPropertyName("passInterstateRequest")]
    public InterstateRequest PassInterstateRequest
    {
      get => passInterstateRequest ??= new();
      set => passInterstateRequest = value;
    }

    /// <summary>
    /// A value of PassInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("passInterstateRequestHistory")]
    public InterstateRequestHistory PassInterstateRequestHistory
    {
      get => passInterstateRequestHistory ??= new();
      set => passInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of PassInterstateCase.
    /// </summary>
    [JsonPropertyName("passInterstateCase")]
    public InterstateCase PassInterstateCase
    {
      get => passInterstateCase ??= new();
      set => passInterstateCase = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public Collection ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of CheckYesCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("checkYesCashReceiptDetail")]
    public CashReceiptDetail CheckYesCashReceiptDetail
    {
      get => checkYesCashReceiptDetail ??= new();
      set => checkYesCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CheckYesCollection.
    /// </summary>
    [JsonPropertyName("checkYesCollection")]
    public Collection CheckYesCollection
    {
      get => checkYesCollection ??= new();
      set => checkYesCollection = value;
    }

    /// <summary>
    /// A value of HistoryRecFndInd.
    /// </summary>
    [JsonPropertyName("historyRecFndInd")]
    public Common HistoryRecFndInd
    {
      get => historyRecFndInd ??= new();
      set => historyRecFndInd = value;
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
    /// A value of ValidCsenetState.
    /// </summary>
    [JsonPropertyName("validCsenetState")]
    public Common ValidCsenetState
    {
      get => validCsenetState ??= new();
      set => validCsenetState = value;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of ProcessCount.
    /// </summary>
    [JsonPropertyName("processCount")]
    public Common ProcessCount
    {
      get => processCount ??= new();
      set => processCount = value;
    }

    /// <summary>
    /// A value of PrintLocalCollRead.
    /// </summary>
    [JsonPropertyName("printLocalCollRead")]
    public Common PrintLocalCollRead
    {
      get => printLocalCollRead ??= new();
      set => printLocalCollRead = value;
    }

    /// <summary>
    /// A value of PrintLocalCollAmtRead.
    /// </summary>
    [JsonPropertyName("printLocalCollAmtRead")]
    public Common PrintLocalCollAmtRead
    {
      get => printLocalCollAmtRead ??= new();
      set => printLocalCollAmtRead = value;
    }

    /// <summary>
    /// A value of PrintLocalCsenetProcessed.
    /// </summary>
    [JsonPropertyName("printLocalCsenetProcessed")]
    public Common PrintLocalCsenetProcessed
    {
      get => printLocalCsenetProcessed ??= new();
      set => printLocalCsenetProcessed = value;
    }

    /// <summary>
    /// A value of PrintLocalCsenetAmtProcessed.
    /// </summary>
    [JsonPropertyName("printLocalCsenetAmtProcessed")]
    public Common PrintLocalCsenetAmtProcessed
    {
      get => printLocalCsenetAmtProcessed ??= new();
      set => printLocalCsenetAmtProcessed = value;
    }

    /// <summary>
    /// A value of PrintLocalCsenetAdjProcessed.
    /// </summary>
    [JsonPropertyName("printLocalCsenetAdjProcessed")]
    public Common PrintLocalCsenetAdjProcessed
    {
      get => printLocalCsenetAdjProcessed ??= new();
      set => printLocalCsenetAdjProcessed = value;
    }

    /// <summary>
    /// A value of PrintLCsenetAdjAmtProcessed.
    /// </summary>
    [JsonPropertyName("printLCsenetAdjAmtProcessed")]
    public Common PrintLCsenetAdjAmtProcessed
    {
      get => printLCsenetAdjAmtProcessed ??= new();
      set => printLCsenetAdjAmtProcessed = value;
    }

    /// <summary>
    /// A value of PrintLocalCollErrored.
    /// </summary>
    [JsonPropertyName("printLocalCollErrored")]
    public Common PrintLocalCollErrored
    {
      get => printLocalCollErrored ??= new();
      set => printLocalCollErrored = value;
    }

    /// <summary>
    /// A value of PrintLocalCollAmtErrored.
    /// </summary>
    [JsonPropertyName("printLocalCollAmtErrored")]
    public Common PrintLocalCollAmtErrored
    {
      get => printLocalCollAmtErrored ??= new();
      set => printLocalCollAmtErrored = value;
    }

    /// <summary>
    /// A value of PrintLocalNonCsenetProcessed.
    /// </summary>
    [JsonPropertyName("printLocalNonCsenetProcessed")]
    public Common PrintLocalNonCsenetProcessed
    {
      get => printLocalNonCsenetProcessed ??= new();
      set => printLocalNonCsenetProcessed = value;
    }

    /// <summary>
    /// A value of PrintLNonCsenetAmtProcessed.
    /// </summary>
    [JsonPropertyName("printLNonCsenetAmtProcessed")]
    public Common PrintLNonCsenetAmtProcessed
    {
      get => printLNonCsenetAmtProcessed ??= new();
      set => printLNonCsenetAmtProcessed = value;
    }

    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
    }

    /// <summary>
    /// A value of CheckStateInterstateRequest.
    /// </summary>
    [JsonPropertyName("checkStateInterstateRequest")]
    public InterstateRequest CheckStateInterstateRequest
    {
      get => checkStateInterstateRequest ??= new();
      set => checkStateInterstateRequest = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Collection Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of CrdTotalCsenetColl.
    /// </summary>
    [JsonPropertyName("crdTotalCsenetColl")]
    public Common CrdTotalCsenetColl
    {
      get => crdTotalCsenetColl ??= new();
      set => crdTotalCsenetColl = value;
    }

    /// <summary>
    /// A value of CrdTotalCsenetAdj.
    /// </summary>
    [JsonPropertyName("crdTotalCsenetAdj")]
    public Common CrdTotalCsenetAdj
    {
      get => crdTotalCsenetAdj ??= new();
      set => crdTotalCsenetAdj = value;
    }

    /// <summary>
    /// A value of CheckAdjustedLiteral.
    /// </summary>
    [JsonPropertyName("checkAdjustedLiteral")]
    public Collection CheckAdjustedLiteral
    {
      get => checkAdjustedLiteral ??= new();
      set => checkAdjustedLiteral = value;
    }

    /// <summary>
    /// A value of ZdelLocalCrdFound.
    /// </summary>
    [JsonPropertyName("zdelLocalCrdFound")]
    public Common ZdelLocalCrdFound
    {
      get => zdelLocalCrdFound ??= new();
      set => zdelLocalCrdFound = value;
    }

    /// <summary>
    /// A value of ZdelLocalInterstateRequestCl.
    /// </summary>
    [JsonPropertyName("zdelLocalInterstateRequestCl")]
    public Common ZdelLocalInterstateRequestCl
    {
      get => zdelLocalInterstateRequestCl ??= new();
      set => zdelLocalInterstateRequestCl = value;
    }

    /// <summary>
    /// A value of ZdelLocalInterstRqstFndInd.
    /// </summary>
    [JsonPropertyName("zdelLocalInterstRqstFndInd")]
    public Common ZdelLocalInterstRqstFndInd
    {
      get => zdelLocalInterstRqstFndInd ??= new();
      set => zdelLocalInterstRqstFndInd = value;
    }

    /// <summary>
    /// A value of Zdel1.
    /// </summary>
    [JsonPropertyName("zdel1")]
    public Common Zdel1
    {
      get => zdel1 ??= new();
      set => zdel1 = value;
    }

    /// <summary>
    /// A value of ZdelLocalSkipToNext.
    /// </summary>
    [JsonPropertyName("zdelLocalSkipToNext")]
    public Common ZdelLocalSkipToNext
    {
      get => zdelLocalSkipToNext ??= new();
      set => zdelLocalSkipToNext = value;
    }

    /// <summary>
    /// Gets a value of Zdel.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroup> Zdel => zdel ??= new(ZdelGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Zdel for json serialization.
    /// </summary>
    [JsonPropertyName("zdel")]
    [Computed]
    public IList<ZdelGroup> Zdel_Json
    {
      get => zdel;
      set => Zdel.Assign(value);
    }

    /// <summary>
    /// A value of ZdelLocalHardcodedFType.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodedFType")]
    public CollectionType ZdelLocalHardcodedFType
    {
      get => zdelLocalHardcodedFType ??= new();
      set => zdelLocalHardcodedFType = value;
    }

    /// <summary>
    /// A value of ZdelLocalHardcodedFTypeSal.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodedFTypeSal")]
    public CollectionType ZdelLocalHardcodedFTypeSal
    {
      get => zdelLocalHardcodedFTypeSal ??= new();
      set => zdelLocalHardcodedFTypeSal = value;
    }

    /// <summary>
    /// A value of ZdelLocalHardcodedFTypeVen.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodedFTypeVen")]
    public CollectionType ZdelLocalHardcodedFTypeVen
    {
      get => zdelLocalHardcodedFTypeVen ??= new();
      set => zdelLocalHardcodedFTypeVen = value;
    }

    /// <summary>
    /// A value of ZdelLocalHardcodedFTypeRet.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodedFTypeRet")]
    public CollectionType ZdelLocalHardcodedFTypeRet
    {
      get => zdelLocalHardcodedFTypeRet ??= new();
      set => zdelLocalHardcodedFTypeRet = value;
    }

    /// <summary>
    /// A value of ZdelLocalHardcodedSType.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodedSType")]
    public CollectionType ZdelLocalHardcodedSType
    {
      get => zdelLocalHardcodedSType ??= new();
      set => zdelLocalHardcodedSType = value;
    }

    /// <summary>
    /// A value of ZdelLocalHardcodedSpType.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodedSpType")]
    public ObligationType ZdelLocalHardcodedSpType
    {
      get => zdelLocalHardcodedSpType ??= new();
      set => zdelLocalHardcodedSpType = value;
    }

    /// <summary>
    /// A value of ZdelLocalHardcodeSajType.
    /// </summary>
    [JsonPropertyName("zdelLocalHardcodeSajType")]
    public ObligationType ZdelLocalHardcodeSajType
    {
      get => zdelLocalHardcodeSajType ??= new();
      set => zdelLocalHardcodeSajType = value;
    }

    private Case1 checkStateCase;
    private CsePerson checkStateCsePerson;
    private CashReceiptDetail restartCsenetProcessing;
    private Common no;
    private Common yes;
    private InterstateRequest checkOutgoingOpen;
    private InterstateCollection passInterstateCollection;
    private InterstateRequest passInterstateRequest;
    private InterstateRequestHistory passInterstateRequestHistory;
    private InterstateCase passInterstateCase;
    private Collection forUpdate;
    private CashReceiptDetail checkYesCashReceiptDetail;
    private Collection checkYesCollection;
    private Common historyRecFndInd;
    private External external;
    private Common validCsenetState;
    private TextWorkArea userId;
    private DateWorkArea current;
    private DateWorkArea null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private ExitStateWorkArea exitStateWorkArea;
    private Common processCount;
    private Common printLocalCollRead;
    private Common printLocalCollAmtRead;
    private Common printLocalCsenetProcessed;
    private Common printLocalCsenetAmtProcessed;
    private Common printLocalCsenetAdjProcessed;
    private Common printLCsenetAdjAmtProcessed;
    private Common printLocalCollErrored;
    private Common printLocalCollAmtErrored;
    private Common printLocalNonCsenetProcessed;
    private Common printLNonCsenetAmtProcessed;
    private CsenetStateTable csenetStateTable;
    private InterstateRequest checkStateInterstateRequest;
    private Collection starting;
    private Common crdTotalCsenetColl;
    private Common crdTotalCsenetAdj;
    private Collection checkAdjustedLiteral;
    private Common zdelLocalCrdFound;
    private Common zdelLocalInterstateRequestCl;
    private Common zdelLocalInterstRqstFndInd;
    private Common zdel1;
    private Common zdelLocalSkipToNext;
    private Array<ZdelGroup> zdel;
    private CollectionType zdelLocalHardcodedFType;
    private CollectionType zdelLocalHardcodedFTypeSal;
    private CollectionType zdelLocalHardcodedFTypeVen;
    private CollectionType zdelLocalHardcodedFTypeRet;
    private CollectionType zdelLocalHardcodedSType;
    private ObligationType zdelLocalHardcodedSpType;
    private ObligationType zdelLocalHardcodeSajType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingInterstateRequest.
    /// </summary>
    [JsonPropertyName("existingInterstateRequest")]
    public InterstateRequest ExistingInterstateRequest
    {
      get => existingInterstateRequest ??= new();
      set => existingInterstateRequest = value;
    }

    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("existingInterstateRequestHistory")]
    public InterstateRequestHistory ExistingInterstateRequestHistory
    {
      get => existingInterstateRequestHistory ??= new();
      set => existingInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExistingCollectionType.
    /// </summary>
    [JsonPropertyName("existingCollectionType")]
    public CollectionType ExistingCollectionType
    {
      get => existingCollectionType ??= new();
      set => existingCollectionType = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
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
    /// A value of ExistingLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("existingLegalActionCaseRole")]
    public LegalActionCaseRole ExistingLegalActionCaseRole
    {
      get => existingLegalActionCaseRole ??= new();
      set => existingLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of DelMeExistingSuppPrsn.
    /// </summary>
    [JsonPropertyName("delMeExistingSuppPrsn")]
    public CsePerson DelMeExistingSuppPrsn
    {
      get => delMeExistingSuppPrsn ??= new();
      set => delMeExistingSuppPrsn = value;
    }

    /// <summary>
    /// A value of DelMeExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("delMeExistingCashReceipt")]
    public CashReceipt DelMeExistingCashReceipt
    {
      get => delMeExistingCashReceipt ??= new();
      set => delMeExistingCashReceipt = value;
    }

    /// <summary>
    /// A value of DelMeExistingSupported.
    /// </summary>
    [JsonPropertyName("delMeExistingSupported")]
    public CsePersonAccount DelMeExistingSupported
    {
      get => delMeExistingSupported ??= new();
      set => delMeExistingSupported = value;
    }

    /// <summary>
    /// A value of DelMeApplicantRecipient.
    /// </summary>
    [JsonPropertyName("delMeApplicantRecipient")]
    public CaseRole DelMeApplicantRecipient
    {
      get => delMeApplicantRecipient ??= new();
      set => delMeApplicantRecipient = value;
    }

    /// <summary>
    /// A value of DelMeChild.
    /// </summary>
    [JsonPropertyName("delMeChild")]
    public CaseRole DelMeChild
    {
      get => delMeChild ??= new();
      set => delMeChild = value;
    }

    /// <summary>
    /// A value of DelMeInterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("delMeInterstateRequestObligation")]
    public InterstateRequestObligation DelMeInterstateRequestObligation
    {
      get => delMeInterstateRequestObligation ??= new();
      set => delMeInterstateRequestObligation = value;
    }

    /// <summary>
    /// A value of DelMeCaseUnit.
    /// </summary>
    [JsonPropertyName("delMeCaseUnit")]
    public CaseUnit DelMeCaseUnit
    {
      get => delMeCaseUnit ??= new();
      set => delMeCaseUnit = value;
    }

    /// <summary>
    /// A value of DelMeInterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("delMeInterstatePaymentAddress")]
    public InterstatePaymentAddress DelMeInterstatePaymentAddress
    {
      get => delMeInterstatePaymentAddress ??= new();
      set => delMeInterstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of DelMeInterstateCase.
    /// </summary>
    [JsonPropertyName("delMeInterstateCase")]
    public InterstateCase DelMeInterstateCase
    {
      get => delMeInterstateCase ??= new();
      set => delMeInterstateCase = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private InterstateRequest existingInterstateRequest;
    private CsePerson existingObligor1;
    private Case1 existingCase;
    private LegalAction existingLegalAction;
    private InterstateRequestHistory existingInterstateRequestHistory;
    private CashReceiptDetail existingCashReceiptDetail;
    private CollectionType existingCollectionType;
    private Collection existingCollection;
    private CaseRole caseRole;
    private LegalActionCaseRole existingLegalActionCaseRole;
    private CaseRole existingAbsentParent;
    private CsePersonAccount existingObligor2;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private CsePerson delMeExistingSuppPrsn;
    private CashReceipt delMeExistingCashReceipt;
    private CsePersonAccount delMeExistingSupported;
    private CaseRole delMeApplicantRecipient;
    private CaseRole delMeChild;
    private InterstateRequestObligation delMeInterstateRequestObligation;
    private CaseUnit delMeCaseUnit;
    private InterstatePaymentAddress delMeInterstatePaymentAddress;
    private InterstateCase delMeInterstateCase;
    private Fips fips;
  }
#endregion
}
