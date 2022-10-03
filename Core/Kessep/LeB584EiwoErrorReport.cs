// Program: LE_B584_EIWO_ERROR_REPORT, ID: 1902478062, model: 746.
// Short name: SWEL584B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B584_EIWO_ERROR_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB584EiwoErrorReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B584_EIWO_ERROR_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB584EiwoErrorReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB584EiwoErrorReport.
  /// </summary>
  public LeB584EiwoErrorReport(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 07/27/15  GVandy	CQ22212		Initial Code.
    // -------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Overview
    // Write all eIWO records in Error status to a report.
    // Business Rules
    // After all records with a corresponding document in "D" status have been 
    // processed, find
    // each e-IWO record in "E" (error) status with Red severity and write them 
    // to an error
    // report.
    // 	a. For each record in the error report, include:
    // 		i.   Transaction Number- The e-IWO transaction number
    // 		ii.  Office- current legal action service provider office
    // 		iii. Worker- current legal action service provider
    // 		iv.  Legal Action- legal action taken tied to the transaction (IWO,
    // 		     IWOTERM, etc)
    // 		v.   Error Event- e-IWO Failed Document Field Retrieval or e-IWO
    // 		     Portal Error
    // 		vi.  Error Event Date- The date the transaction went into E status
    // 	b. Sort the report by Error Event date (descending), then by Transaction
    // Number.
    // --------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // ------------------------------------------------------------------------------
    // -- Read for restart info.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // ------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      //  Checkpoint Info...
      // 	Position  Description
      // 	--------  
      // -----------------------------------------
      // 	001-010   Status Date n (10)  (YYYY-MM-DD)
      // 	011-011   Blank
      // 	012-023   Transaction Number (12)
      //         024-024   Blank
      //         025-033   Total Number of eIWOs in 'E' Status
      // -------------------------------------------------------------------------------------
      local.RestartIwoAction.StatusDate =
        StringToDate(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 10));
      local.RestartIwoTransaction.TransactionNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 12, 12);
      local.TotalRecordCount.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 25, 9));

      // -- Log restart data to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 6; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "Program is restarting...";

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "    Restarting at Error Date................." + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 1, 10);

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "    Restarting at Transaction Number........." + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 12, 12);

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "    Number of eIWOs Previously Processed....." + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 25, 9);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error Writing Restart Info to the Control Report...  Returned Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    else
    {
      local.RestartIwoAction.StatusDate = new DateTime(2099, 12, 31);
      local.RestartIwoTransaction.TransactionNumber = "";
      local.TotalRecordCount.Count = 0;

      // -- Log info to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail =
            "Program is starting from the beginning.";
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error Writing Restart Info to the Control Report...  Returned Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Report File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening report file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // -- Write the report header.
    // -------------------------------------------------------------------------------------
    local.EabReportSend.RptDetail =
      "ERROR DATE  TRANSACTION # OFFICE  WORKER    ACTION    ERROR DESCRIPTION";
      
    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Writing Report Header...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NumbOfIwosSinceChckpnt.Count = 0;

    // -------------------------------------------------------------------------------------
    // -- Find each eIWO in 'E' status and severity_cleared_ind = 'N'.
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadIwoActionIwoTransaction())
    {
      if (ReadLegalAction())
      {
        local.ActionTaken.Text8 =
          Substring(entities.LegalAction.ActionTaken, 1, 8);
      }
      else
      {
        local.ActionTaken.Text8 = "";
      }

      if (ReadOfficeServiceProvider())
      {
        local.Office.Text3 =
          NumberToString(entities.Office.SystemGeneratedId, 13, 3);
        local.ServiceProvider.UserId = entities.ServiceProvider.UserId;
      }
      else
      {
        local.Office.Text3 = "";
        local.ServiceProvider.UserId = "";
      }

      local.ErrorDescription.Text80 = "";

      if (ReadIwoActionHistory())
      {
        if (Equal(entities.IwoActionHistory.CreatedBy, "SWELB588"))
        {
          local.ErrorDescription.Text80 = "e-IWO Portal Error";
          local.ErrorDescription.Text80 =
            TrimEnd(local.ErrorDescription.Text80) + " - " + entities
            .IwoAction.ErrorRecordType;

          if (Equal(entities.IwoAction.ErrorRecordType, "DTL"))
          {
            local.ErrorDescription.Text80 =
              TrimEnd(local.ErrorDescription.Text80) + " - " + TrimEnd
              (entities.IwoAction.ErrorField1);
          }
          else
          {
            switch(TrimEnd(entities.IwoAction.StatusReasonCode))
            {
              case "CDT":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " -  Creation Date";

                break;
              case "CNM":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " - Control Number";

                break;
              case "CTM":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " -  Creation Time";

                break;
              case "DOC":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " - Document Code";

                break;
              case "DUP":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " - File Already Received";
                  

                break;
              case "EIN":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " - EIN Text";

                break;
              case "FPS":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " - State FIPS Code";
                  

                break;
              case "PPE":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " - Payroll Processor EIN Text";
                  

                break;
              case "BCT":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " - Batch Count";

                break;
              case "RCT":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " - Record Count";

                break;
              case "REC":
                local.ErrorDescription.Text80 =
                  TrimEnd(local.ErrorDescription.Text80) + " - Invalid File Structure";
                  

                break;
              default:
                break;
            }
          }
        }
        else
        {
          local.ErrorDescription.Text80 =
            "e-IWO Failed Document Field Retrieval";
        }
      }

      local.DateWorkArea.Date = entities.IwoAction.StatusDate;
      UseCabFormatDate();

      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = local.FormattedDate.Text10 + "  " + entities
        .IwoTransaction.TransactionNumber + "    " + local.Office.Text3 + "   " +
        local.ServiceProvider.UserId + "  " + local.ActionTaken.Text8 + "  " + local
        .ErrorDescription.Text80;
      UseCabBusinessReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error Writing Report Detail...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.TotalRecordCount.Count;
      ++local.NumbOfIwosSinceChckpnt.Count;

      // -- Checkpoint processing.
      if (local.NumbOfIwosSinceChckpnt.Count > local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -- Checkpoint.
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	Position  Description
        // 	--------  
        // -----------------------------------------
        // 	001-010   Status Date n (10)  (YYYY-MM-DD)
        // 	011-011   Blank
        // 	012-023   Transaction Number (12)
        //         024-024   Blank
        //         025-033   Total Number of eIWOs in 'E' Status
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.DateWorkArea.Date = entities.IwoAction.StatusDate;
        UseCabDate2TextWithHyphens();
        local.ProgramCheckpointRestart.RestartInfo = local.TextDate.Text10 + " " +
          entities.IwoTransaction.TransactionNumber + " " + NumberToString
          (local.TotalRecordCount.Count, 7, 9);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking checkpoint.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.NumbOfIwosSinceChckpnt.Count = 0;
      }
    }

    // -------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      if (local.Common.Count == 1)
      {
        local.EabReportSend.RptDetail =
          "Number of eIWO Errors Written to the Error Report.................." +
          NumberToString(local.TotalRecordCount.Count, 9, 7);
      }
      else
      {
        local.EabReportSend.RptDetail = "";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error Writing Control Report Totals...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking final checkpoint.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Report File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing report file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

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

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabDate2TextWithHyphens()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextDate.Text10 = useExport.TextWorkArea.Text10;
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

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadIwoActionHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    entities.IwoActionHistory.Populated = false;

    return Read("ReadIwoActionHistory",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
        db.SetInt32(command, "iwaIdentifier", entities.IwoAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IwoActionHistory.Identifier = db.GetInt32(reader, 0);
        entities.IwoActionHistory.CreatedBy = db.GetString(reader, 1);
        entities.IwoActionHistory.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.IwoActionHistory.CspNumber = db.GetString(reader, 3);
        entities.IwoActionHistory.LgaIdentifier = db.GetInt32(reader, 4);
        entities.IwoActionHistory.IwtIdentifier = db.GetInt32(reader, 5);
        entities.IwoActionHistory.IwaIdentifier = db.GetInt32(reader, 6);
        entities.IwoActionHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIwoActionIwoTransaction()
  {
    entities.IwoTransaction.Populated = false;
    entities.IwoAction.Populated = false;

    return ReadEach("ReadIwoActionIwoTransaction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "statusDate",
          local.RestartIwoAction.StatusDate.GetValueOrDefault());
        db.SetNullableString(
          command, "transactionNumber",
          local.RestartIwoTransaction.TransactionNumber ?? "");
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoAction.StatusReasonCode = db.GetNullableString(reader, 4);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 5);
        entities.IwoAction.SeverityClearedInd = db.GetNullableString(reader, 6);
        entities.IwoAction.ErrorRecordType = db.GetNullableString(reader, 7);
        entities.IwoAction.ErrorField1 = db.GetNullableString(reader, 8);
        entities.IwoAction.ErrorField2 = db.GetNullableString(reader, 9);
        entities.IwoAction.CspNumber = db.GetString(reader, 10);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 10);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 11);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 11);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 12);
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 12);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 13);
        entities.IwoTransaction.Populated = true;
        entities.IwoAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", entities.IwoTransaction.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.ActionTaken = db.GetString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.ServiceProvider.UserId = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
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
    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public WorkArea ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of ErrorDescription.
    /// </summary>
    [JsonPropertyName("errorDescription")]
    public WorkArea ErrorDescription
    {
      get => errorDescription ??= new();
      set => errorDescription = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public WorkArea Office
    {
      get => office ??= new();
      set => office = value;
    }

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
    /// A value of RestartIwoTransaction.
    /// </summary>
    [JsonPropertyName("restartIwoTransaction")]
    public IwoTransaction RestartIwoTransaction
    {
      get => restartIwoTransaction ??= new();
      set => restartIwoTransaction = value;
    }

    /// <summary>
    /// A value of RestartIwoAction.
    /// </summary>
    [JsonPropertyName("restartIwoAction")]
    public IwoAction RestartIwoAction
    {
      get => restartIwoAction ??= new();
      set => restartIwoAction = value;
    }

    /// <summary>
    /// A value of TotalRecordCount.
    /// </summary>
    [JsonPropertyName("totalRecordCount")]
    public Common TotalRecordCount
    {
      get => totalRecordCount ??= new();
      set => totalRecordCount = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of NumbOfIwosSinceChckpnt.
    /// </summary>
    [JsonPropertyName("numbOfIwosSinceChckpnt")]
    public Common NumbOfIwosSinceChckpnt
    {
      get => numbOfIwosSinceChckpnt ??= new();
      set => numbOfIwosSinceChckpnt = value;
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
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public TextWorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    private WorkArea actionTaken;
    private WorkArea errorDescription;
    private WorkArea formattedDate;
    private WorkArea office;
    private ServiceProvider serviceProvider;
    private IwoTransaction restartIwoTransaction;
    private IwoAction restartIwoAction;
    private Common totalRecordCount;
    private DateWorkArea dateWorkArea;
    private Common numbOfIwosSinceChckpnt;
    private ProgramCheckpointRestart programCheckpointRestart;
    private TextWorkArea textDate;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IwoActionHistory.
    /// </summary>
    [JsonPropertyName("iwoActionHistory")]
    public IwoActionHistory IwoActionHistory
    {
      get => iwoActionHistory ??= new();
      set => iwoActionHistory = value;
    }

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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    private IwoActionHistory iwoActionHistory;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private LegalActionAssigment legalActionAssigment;
    private LegalAction legalAction;
    private IwoTransaction iwoTransaction;
    private IwoAction iwoAction;
  }
#endregion
}
