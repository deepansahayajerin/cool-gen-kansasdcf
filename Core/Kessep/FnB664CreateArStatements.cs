// Program: FN_B664_CREATE_AR_STATEMENTS, ID: 372549128, model: 746.
// Short name: SWEF664B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B664_CREATE_AR_STATEMENTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB664CreateArStatements: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B664_CREATE_AR_STATEMENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB664CreateArStatements(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB664CreateArStatements.
  /// </summary>
  public FnB664CreateArStatements(IContext context, Import import, Export export)
    :
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
    // 02/17/05  GVandy	PR233867	Initial Development.  New business rules for AR
    // statements.
    // 05/02/05  GVandy	PR242288	Do not send statement if the only collection 
    // activity is for 718B judgements.
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  This program creates the AR statements using an extract file from a 
    // previous job step which has been sorted
    // --  and summed for each ar, obligor, court order number, process year & 
    // month, applied to code, and adjustment indicator.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Read.Action = "READ";
    local.Write.Action = "WRITE";
    local.Close.Action = "CLOSE";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  General Housekeeping and Initializations.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB664BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (!IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Initialization Cab Error..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // -- Convert Reporting Period Starting Date to text.
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      NumberToString(Month(local.ReportingPeriodStartingDateWorkArea.Date), 14,
      2) + "/";
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodStartingTextWorkArea.Text10) + NumberToString
      (Day(local.ReportingPeriodStartingDateWorkArea.Date), 14, 2);
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodStartingTextWorkArea.Text10) + "/";
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodStartingTextWorkArea.Text10) + NumberToString
      (Year(local.ReportingPeriodStartingDateWorkArea.Date), 12, 4);

    // -- Convert Reporting Period Ending Date to text.
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      NumberToString(Month(local.ReportingPeriodEndingDateWorkArea.Date), 14, 2) +
      "/";
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodEndingTextWorkArea.Text10) + NumberToString
      (Day(local.ReportingPeriodEndingDateWorkArea.Date), 14, 2);
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodEndingTextWorkArea.Text10) + "/";
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodEndingTextWorkArea.Text10) + NumberToString
      (Year(local.ReportingPeriodEndingDateWorkArea.Date), 12, 4);
    UseFnHardcodedDebtDistribution();
    local.Local1.Index = -1;

    do
    {
      // -------------------------------------------------------------------------------------------------------------------------
      // --  Get a record from the sorted/summed extract file.
      // --
      // --  Note that the external can return more data than what we actually 
      // need.  Not all views need to be returned for what we're doing here.
      // -------------------------------------------------------------------------------------------------------------------------
      UseFnB664ReadExtractDataFile3();

      if (!Equal(local.Read.Status, "OK") && !Equal(local.Read.Status, "EF"))
      {
        // --  write to error file...
        local.EabReportSend.RptDetail =
          "(01) Error reading extract file...  Returned Status = " + local
          .Read.Status;
        UseCabErrorReport1();
        ExitState = "ERROR_READING_FILE_AB";

        return;
      }

      // -------------------------------------------------------------------------------------------------------------------------
      // -- If restarting then skip any ARs previously processed.
      // -------------------------------------------------------------------------------------------------------------------------
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y' && !
        IsEmpty(local.Ar.Number) && !Lt(local.Restart.Number, local.Ar.Number))
      {
        continue;
      }

      ++local.ReadCount.Count;

      if (!Equal(local.Ar.Number, local.PreviousAr.Number) && !
        IsEmpty(local.PreviousAr.Number))
      {
        // -------------------------------------------------------------------------------------------------------------------------
        // --  Our most recent read of the extract file produced a new AR 
        // number.  We need to write the AR statement for the
        // --  previous AR using all the collection information stored in the 
        // local group.
        // -------------------------------------------------------------------------------------------------------------------------
        // -- Write the AR statement.
        UseFnB664WriteArStatement1();

        switch(TrimEnd(local.ArStatementStatus.Text8))
        {
          case "PRINTED":
            // -- The AR statement processed successfully.  Increment the 
            // printed count.
            ++local.NumOfStatementsPrinted.Count;

            break;
          case "NOASSOB":
            // -- The statement was not printed.  There was no assigned 
            // obligation for the AR.  Increment the no assigned obligation
            // count.
            ++local.NumOfStatementsNoassob.Count;

            break;
          case "NOACTADD":
            // -- The statement was not printed.  There was no active verified 
            // address for the AR.  Increment the no active address count.
            ++local.NumOfStatementsNoactadd.Count;

            break;
          case "DECEASED":
            // -- The statement was not printed.  The AR is deceased.  Increment
            // the deceased count.
            ++local.NumOfStatementsDeceased.Count;

            break;
          case "718BONLY":
            // -- The statement was not printed.  The only activity for the AR 
            // was 718B collections.  Increment the 718B count.
            ++local.NumOfStatements718B.Count;

            break;
          case "ERRORED":
            // -- An error occurred while processing the AR statement.  
            // Increment the error count.
            ++local.NumOfStatementsErrored.Count;

            break;
          default:
            break;
        }

        // -- An exit state is set in fn_b664_write_ar_statement for errors that
        // should cause an abend.
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Extract the exit state message and write to the error report.
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail =
            "FN_B664_WRITE_AR_STATEMENT Error..." + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (local.ReadCount.Count > local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
        {
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Checkpoint.
          // -------------------------------------------------------------------------------------------------------------------------
          local.ReadCount.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = local.PreviousAr.Number;
          UseUpdatePgmCheckpointRestart();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -- Extract the exit state message and write to the error report.
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = "Checkpoint Error..." + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          UseExtToDoACommit();

          if (local.External.NumericReturnCode != 0)
          {
            local.EabReportSend.RptDetail =
              "(03) Error in External Commit Routine.  Return Code = " + NumberToString
              (local.External.NumericReturnCode, 14, 2);
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        // -- Note:  The local group was re-initialized by the export group view
        // matching on the call to write AR statement cab.
        local.Local1.Index = -1;

        // -- Reset the running obligation type count for the next AR.
        local.Non718BCollection.Count = 0;
      }

      if (local.Local1.Index + 1 >= Local.LocalGroup.Capacity)
      {
        // --  Increment the error count.
        ++local.NumOfStatementsErrored.Count;

        // -- Write to error file...
        local.EabReportSend.RptDetail =
          "Statement exceeded maximum number of lines for AR " + local
          .Ar.Number + ".  Maximum number of lines allowed is " + NumberToString
          (Local.LocalGroup.Capacity, 12, 4) + ".";
        UseCabErrorReport1();

        if (!Equal(local.Write.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

          return;
        }

        // -- Continue reading the extract file until we advance past this AR.
        while(Equal(local.Ar.Number, local.PreviousAr.Number))
        {
          UseFnB664ReadExtractDataFile2();

          if (!Equal(local.Read.Status, "OK") && !
            Equal(local.Read.Status, "EF"))
          {
            // --  write to error file...
            local.EabReportSend.RptDetail =
              "(02) Error reading extract file...  Returned Status = " + local
              .Read.Status;
            UseCabErrorReport1();
            ExitState = "ERROR_READING_FILE_AB";

            return;
          }
        }

        // -- Use the write cab to re-initialize the local group.
        UseFnB664WriteArStatement2();
        local.Local1.Index = -1;
      }

      // -- Move extract file info to the group view.
      local.PreviousAr.Number = local.Ar.Number;

      ++local.Local1.Index;
      local.Local1.CheckSize();

      local.Local1.Update.GlocalObligor.Number = local.Obligor.Number;
      local.Local1.Update.G.Assign(local.Collection);
      local.Local1.Update.GlocalRetained.Amount = local.Retained.Amount;
      local.Local1.Update.GlocalForwardedToFamily.Amount =
        local.ForwardedToFamily.Amount;

      // -- Increment the running obligation type count to determine if any 
      // collections other than for 718B judgements were received during the
      // reporting period.
      local.Non718BCollection.Count += local.ObligationType.
        SystemGeneratedIdentifier;
    }
    while(Equal(local.Read.Status, "OK"));

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    for(local.Counter.Count = 0; local.Counter.Count <= 17; ++
      local.Counter.Count)
    {
      switch(local.Counter.Count)
      {
        case 0:
          local.EabReportSend.RptDetail =
            "Number of AR Statements Printed with this Number of Pages in the Statement";
            

          break;
        case 10:
          local.EabReportSend.RptDetail = "";

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "Number of Statements with Errors    " + NumberToString
            (local.NumOfStatementsErrored.Count, 10, 6);

          break;
        case 12:
          local.EabReportSend.RptDetail = "";

          break;
        case 13:
          local.EabReportSend.RptDetail = "Number of AR Statements Skipped:";

          break;
        case 14:
          local.EabReportSend.RptDetail =
            "  -- 718B Collections Only          " + NumberToString
            (local.NumOfStatements718B.Count, 10, 6);

          break;
        case 15:
          local.EabReportSend.RptDetail =
            "  -- AR is Deceased                 " + NumberToString
            (local.NumOfStatementsDeceased.Count, 10, 6);

          break;
        case 16:
          local.EabReportSend.RptDetail =
            "  -- No Assigned Obligations        " + NumberToString
            (local.NumOfStatementsNoassob.Count, 10, 6);

          break;
        case 17:
          local.EabReportSend.RptDetail =
            "  -- No Active Verified Address     " + NumberToString
            (local.NumOfStatementsNoactadd.Count, 10, 6);

          break;
        case 18:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          local.StatementCount.Index = local.Counter.Count - 1;
          local.StatementCount.CheckSize();

          if (local.Counter.Count == 1)
          {
            local.EabReportSend.RptDetail = "                         " + NumberToString
              (local.Counter.Count, 15, 1) + " Page     " + NumberToString
              (local.StatementCount.Item.GlocalCount.Count, 10, 6);
          }
          else
          {
            local.EabReportSend.RptDetail = "                         " + NumberToString
              (local.Counter.Count, 15, 1) + " Pages    " + NumberToString
              (local.StatementCount.Item.GlocalCount.Count, 10, 6);
          }

          break;
      }

      UseCabControlReport1();

      if (!Equal(local.Write.Status, "OK"))
      {
        // -- Write to the error report.
        local.Write.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(02) Error Writing Control Report...  Returned Status = " + local
          .Write.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

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
      UseCabErrorReport1();

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
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB664ReadExtractDataFile1();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Extract File...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the AR Statement Report files.
    // -------------------------------------------------------------------------------------------------------------------------
    UseSpEabWriteDocument();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Report Files...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabErrorReport2();

    if (!Equal(local.Close.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveExport1ToLocal1(FnB664WriteArStatement.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.GlocalObligor.Number = source.GexportObligor.Number;
    target.G.Assign(source.G);
    target.GlocalRetained.Amount = source.GexportRetained.Amount;
    target.GlocalForwardedToFamily.Amount =
      source.GexportForwardedToFamily.Amount;
  }

  private static void MoveGimportExportStatementCount(FnB664WriteArStatement.
    Import.GimportExportStatementCountGroup source,
    Local.StatementCountGroup target)
  {
    target.GlocalCount.Count = source.GimportExportCount.Count;
  }

  private static void MoveLocal1ToImport1(Local.LocalGroup source,
    FnB664WriteArStatement.Import.ImportGroup target)
  {
    target.GimportObligor.Number = source.GlocalObligor.Number;
    target.G.Assign(source.G);
    target.GimportRetained.Amount = source.GlocalRetained.Amount;
    target.GimportForwardedToFamily.Amount =
      source.GlocalForwardedToFamily.Amount;
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

  private static void MoveStatementCount(Local.StatementCountGroup source,
    FnB664WriteArStatement.Import.GimportExportStatementCountGroup target)
  {
    target.GimportExportCount.Count = source.GlocalCount.Count;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Write.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Write.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB664BatchInitialization()
  {
    var useImport = new FnB664BatchInitialization.Import();
    var useExport = new FnB664BatchInitialization.Export();

    Call(FnB664BatchInitialization.Execute, useImport, useExport);

    local.CreateEvents.Flag = useExport.CreateEvents.Flag;
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      useExport.ReportingPeriodStartingTextWorkArea.Text10;
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      useExport.ReportingPeriodEndingTextWorkArea.Text10;
    local.ProgramProcessingInfo.ProcessDate =
      useExport.ProgramProcessingInfo.ProcessDate;
    local.Restart.Number = useExport.Restart.Number;
    MoveProgramCheckpointRestart1(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveDateWorkArea(useExport.ReportingPeriodEndingDateWorkArea,
      local.ReportingPeriodEndingDateWorkArea);
    MoveDateWorkArea(useExport.ReportingPeriodStartingDateWorkArea,
      local.ReportingPeriodStartingDateWorkArea);
  }

  private void UseFnB664ReadExtractDataFile1()
  {
    var useImport = new FnB664ReadExtractDataFile.Import();
    var useExport = new FnB664ReadExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.Close.Action;
    useExport.EabFileHandling.Status = local.Close.Status;

    Call(FnB664ReadExtractDataFile.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB664ReadExtractDataFile2()
  {
    var useImport = new FnB664ReadExtractDataFile.Import();
    var useExport = new FnB664ReadExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.Read.Action;
    useExport.EabFileHandling.Status = local.Read.Status;
    useExport.Ar.Number = local.Ar.Number;
    useExport.ForwardedToFamily.Amount = local.ForwardedToFamily.Amount;
    useExport.Retained.Amount = local.Retained.Amount;
    useExport.Collection.Assign(local.Collection);
    useExport.Obligor.Number = local.Obligor.Number;

    Call(FnB664ReadExtractDataFile.Execute, useImport, useExport);

    local.Read.Status = useExport.EabFileHandling.Status;
    local.Ar.Number = useExport.Ar.Number;
    local.ForwardedToFamily.Amount = useExport.ForwardedToFamily.Amount;
    local.Retained.Amount = useExport.Retained.Amount;
    local.Collection.Assign(useExport.Collection);
    local.Obligor.Number = useExport.Obligor.Number;
  }

  private void UseFnB664ReadExtractDataFile3()
  {
    var useImport = new FnB664ReadExtractDataFile.Import();
    var useExport = new FnB664ReadExtractDataFile.Export();

    useImport.EabFileHandling.Action = local.Read.Action;
    useExport.EabFileHandling.Status = local.Read.Status;
    useExport.Ar.Number = local.Ar.Number;
    useExport.ForwardedToFamily.Amount = local.ForwardedToFamily.Amount;
    useExport.Retained.Amount = local.Retained.Amount;
    useExport.Collection.Assign(local.Collection);
    useExport.Obligor.Number = local.Obligor.Number;
    useExport.ObligationType.SystemGeneratedIdentifier =
      local.ObligationType.SystemGeneratedIdentifier;

    Call(FnB664ReadExtractDataFile.Execute, useImport, useExport);

    local.Read.Status = useExport.EabFileHandling.Status;
    local.Ar.Number = useExport.Ar.Number;
    local.ForwardedToFamily.Amount = useExport.ForwardedToFamily.Amount;
    local.Retained.Amount = useExport.Retained.Amount;
    local.Collection.Assign(useExport.Collection);
    local.Obligor.Number = useExport.Obligor.Number;
    local.ObligationType.SystemGeneratedIdentifier =
      useExport.ObligationType.SystemGeneratedIdentifier;
  }

  private void UseFnB664WriteArStatement1()
  {
    var useImport = new FnB664WriteArStatement.Import();
    var useExport = new FnB664WriteArStatement.Export();

    useImport.CreateEvents.Flag = local.CreateEvents.Flag;
    local.StatementCount.CopyTo(
      useImport.GimportExportStatementCount, MoveStatementCount);
    useImport.ReportingPeriodStarting.Text10 =
      local.ReportingPeriodStartingTextWorkArea.Text10;
    useImport.ReportingPeriodEndingTextWorkArea.Text10 =
      local.ReportingPeriodEndingTextWorkArea.Text10;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Ar.Number = local.PreviousAr.Number;
    local.Local1.CopyTo(useImport.Import1, MoveLocal1ToImport1);
    useImport.ReportingPeriodEndingDateWorkArea.Date =
      local.ReportingPeriodEndingDateWorkArea.Date;
    useImport.Voluntary.SystemGeneratedIdentifier =
      local.Voluntary.SystemGeneratedIdentifier;
    useImport.SpousalArrearsJudgement.SystemGeneratedIdentifier =
      local.SpousalArrearsJudgement.SystemGeneratedIdentifier;
    useImport.SpousalSupport.SystemGeneratedIdentifier =
      local.SpousalSupport.SystemGeneratedIdentifier;
    useImport.Non718BCollection.Count = local.Non718BCollection.Count;

    Call(FnB664WriteArStatement.Execute, useImport, useExport);

    useImport.GimportExportStatementCount.CopyTo(
      local.StatementCount, MoveGimportExportStatementCount);
    local.ArStatementStatus.Text8 = useExport.ArStatementStatus.Text8;
    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
  }

  private void UseFnB664WriteArStatement2()
  {
    var useImport = new FnB664WriteArStatement.Import();
    var useExport = new FnB664WriteArStatement.Export();

    Call(FnB664WriteArStatement.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.Voluntary.SystemGeneratedIdentifier =
      useExport.OtVoluntary.SystemGeneratedIdentifier;
    local.SpousalArrearsJudgement.SystemGeneratedIdentifier =
      useExport.OtSpousalArrearsJudgement.SystemGeneratedIdentifier;
    local.SpousalSupport.SystemGeneratedIdentifier =
      useExport.OtSpousalSupport.SystemGeneratedIdentifier;
  }

  private void UseSpEabWriteDocument()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabFileHandling.Action = local.Close.Action;
    useExport.EabFileHandling.Status = local.Close.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// <summary>A StatementCountGroup group.</summary>
    [Serializable]
    public class StatementCountGroup
    {
      /// <summary>
      /// A value of GlocalCount.
      /// </summary>
      [JsonPropertyName("glocalCount")]
      public Common GlocalCount
      {
        get => glocalCount ??= new();
        set => glocalCount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Common glocalCount;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
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

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Collection G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GlocalRetained.
      /// </summary>
      [JsonPropertyName("glocalRetained")]
      public Collection GlocalRetained
      {
        get => glocalRetained ??= new();
        set => glocalRetained = value;
      }

      /// <summary>
      /// A value of GlocalForwardedToFamily.
      /// </summary>
      [JsonPropertyName("glocalForwardedToFamily")]
      public Collection GlocalForwardedToFamily
      {
        get => glocalForwardedToFamily ??= new();
        set => glocalForwardedToFamily = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson glocalObligor;
      private Collection g;
      private Collection glocalRetained;
      private Collection glocalForwardedToFamily;
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
    /// A value of CreateEvents.
    /// </summary>
    [JsonPropertyName("createEvents")]
    public Common CreateEvents
    {
      get => createEvents ??= new();
      set => createEvents = value;
    }

    /// <summary>
    /// A value of ArStatementStatus.
    /// </summary>
    [JsonPropertyName("arStatementStatus")]
    public TextWorkArea ArStatementStatus
    {
      get => arStatementStatus ??= new();
      set => arStatementStatus = value;
    }

    /// <summary>
    /// Gets a value of StatementCount.
    /// </summary>
    [JsonIgnore]
    public Array<StatementCountGroup> StatementCount => statementCount ??= new(
      StatementCountGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of StatementCount for json serialization.
    /// </summary>
    [JsonPropertyName("statementCount")]
    [Computed]
    public IList<StatementCountGroup> StatementCount_Json
    {
      get => statementCount;
      set => StatementCount.Assign(value);
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
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabFileHandling Write
    {
      get => write ??= new();
      set => write = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public EabFileHandling Close
    {
      get => close ??= new();
      set => close = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public EabFileHandling Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStartingTextWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodStartingTextWorkArea")]
    public TextWorkArea ReportingPeriodStartingTextWorkArea
    {
      get => reportingPeriodStartingTextWorkArea ??= new();
      set => reportingPeriodStartingTextWorkArea = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingTextWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingTextWorkArea")]
    public TextWorkArea ReportingPeriodEndingTextWorkArea
    {
      get => reportingPeriodEndingTextWorkArea ??= new();
      set => reportingPeriodEndingTextWorkArea = value;
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
    /// A value of NumOfStatementsErrored.
    /// </summary>
    [JsonPropertyName("numOfStatementsErrored")]
    public Common NumOfStatementsErrored
    {
      get => numOfStatementsErrored ??= new();
      set => numOfStatementsErrored = value;
    }

    /// <summary>
    /// A value of NumOfStatementsPrinted.
    /// </summary>
    [JsonPropertyName("numOfStatementsPrinted")]
    public Common NumOfStatementsPrinted
    {
      get => numOfStatementsPrinted ??= new();
      set => numOfStatementsPrinted = value;
    }

    /// <summary>
    /// A value of NumOfStatementsNoassob.
    /// </summary>
    [JsonPropertyName("numOfStatementsNoassob")]
    public Common NumOfStatementsNoassob
    {
      get => numOfStatementsNoassob ??= new();
      set => numOfStatementsNoassob = value;
    }

    /// <summary>
    /// A value of NumOfStatementsDeceased.
    /// </summary>
    [JsonPropertyName("numOfStatementsDeceased")]
    public Common NumOfStatementsDeceased
    {
      get => numOfStatementsDeceased ??= new();
      set => numOfStatementsDeceased = value;
    }

    /// <summary>
    /// A value of NumOfStatements718B.
    /// </summary>
    [JsonPropertyName("numOfStatements718B")]
    public Common NumOfStatements718B
    {
      get => numOfStatements718B ??= new();
      set => numOfStatements718B = value;
    }

    /// <summary>
    /// A value of Non718BCollection.
    /// </summary>
    [JsonPropertyName("non718BCollection")]
    public Common Non718BCollection
    {
      get => non718BCollection ??= new();
      set => non718BCollection = value;
    }

    /// <summary>
    /// A value of NumOfStatementsNoactadd.
    /// </summary>
    [JsonPropertyName("numOfStatementsNoactadd")]
    public Common NumOfStatementsNoactadd
    {
      get => numOfStatementsNoactadd ??= new();
      set => numOfStatementsNoactadd = value;
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
    /// A value of PreviousAr.
    /// </summary>
    [JsonPropertyName("previousAr")]
    public CsePerson PreviousAr
    {
      get => previousAr ??= new();
      set => previousAr = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of ReadCount.
    /// </summary>
    [JsonPropertyName("readCount")]
    public Common ReadCount
    {
      get => readCount ??= new();
      set => readCount = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingDateWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingDateWorkArea")]
    public DateWorkArea ReportingPeriodEndingDateWorkArea
    {
      get => reportingPeriodEndingDateWorkArea ??= new();
      set => reportingPeriodEndingDateWorkArea = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStartingDateWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodStartingDateWorkArea")]
    public DateWorkArea ReportingPeriodStartingDateWorkArea
    {
      get => reportingPeriodStartingDateWorkArea ??= new();
      set => reportingPeriodStartingDateWorkArea = value;
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
    /// A value of SpousalArrearsJudgement.
    /// </summary>
    [JsonPropertyName("spousalArrearsJudgement")]
    public ObligationType SpousalArrearsJudgement
    {
      get => spousalArrearsJudgement ??= new();
      set => spousalArrearsJudgement = value;
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

    private ObligationType obligationType;
    private Common createEvents;
    private TextWorkArea arStatementStatus;
    private Array<StatementCountGroup> statementCount;
    private EabReportSend eabReportSend;
    private EabFileHandling write;
    private EabFileHandling close;
    private EabFileHandling read;
    private TextWorkArea reportingPeriodStartingTextWorkArea;
    private TextWorkArea reportingPeriodEndingTextWorkArea;
    private ProgramProcessingInfo programProcessingInfo;
    private Common numOfStatementsErrored;
    private Common numOfStatementsPrinted;
    private Common numOfStatementsNoassob;
    private Common numOfStatementsDeceased;
    private Common numOfStatements718B;
    private Common non718BCollection;
    private Common numOfStatementsNoactadd;
    private CsePerson ar;
    private CsePerson previousAr;
    private Collection forwardedToFamily;
    private Collection retained;
    private Collection collection;
    private CsePerson obligor;
    private Array<LocalGroup> local1;
    private Common counter;
    private External external;
    private CsePerson restart;
    private Common readCount;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea reportingPeriodEndingDateWorkArea;
    private DateWorkArea reportingPeriodStartingDateWorkArea;
    private ObligationType voluntary;
    private ObligationType spousalArrearsJudgement;
    private ObligationType spousalSupport;
  }
#endregion
}
