// Program: FN_B636_AFCARS_SUMMARY, ID: 1902602461, model: 746.
// Short name: SWEF636B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B636_AFCARS_SUMMARY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB636AfcarsSummary: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B636_AFCARS_SUMMARY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB636AfcarsSummary(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB636AfcarsSummary.
  /// </summary>
  public FnB636AfcarsSummary(IContext context, Import import, Export export):
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
    // 08/25/17  GVandy	CQ59085		Initial Development.
    // 10/20/17  GVandy	CQ60200		Correct report period when processing date is 
    // 1st
    // 					day of the month.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Purpose:
    // 	Provide collection amount totals for each supported person for the prior
    // month.
    // 	This information is used by FACTS for the AFCARS report.
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

    local.TotalNumbSupportedRead.Count = 0;
    local.NumbSuppSinceChckpnt.Count = 0;
    local.RestartSupported.Number = "";

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // ------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      //  Checkpoint Info...
      // 	Position  Description
      // 	--------  
      // ---------------------------------------------------------
      //         001-009   Number of Supported Persons Previously Processed
      //         010-010   Blank
      // 	011-020   Last Supported Person Number Processed
      // -------------------------------------------------------------------------------------
      local.TotalNumbSupportedRead.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 9));
      local.RestartSupported.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 11, 10);

      // -- Log restart information to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Program is restarting after Supported CSP Number " + local
              .RestartSupported.Number;

            break;
          case 2:
            local.EabReportSend.RptDetail =
              NumberToString(local.TotalNumbSupportedRead.Count, 10, 6) + " Supported Persons Previously Processed";
              

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
            "(02) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // ------------------------------------------------------------------------------
    // -- Determine report period.
    // ------------------------------------------------------------------------------
    local.CollectionStartDate.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate,
      -(Day(local.ProgramProcessingInfo.ProcessDate) - 1));
    local.CollectionStartDate.Date =
      AddMonths(local.CollectionStartDate.Date, -1);
    local.CollectionStartDate.Time = TimeSpan.Zero;
    UseFnBuildTimestampFrmDateTime2();
    local.CollectionEndDate.Date =
      AddDays(AddMonths(local.CollectionStartDate.Date, 1), -1);
    local.CollectionEndDate.Time = new TimeSpan(23, 59, 59);
    UseFnBuildTimestampFrmDateTime1();
    local.AfcarsSummary.ReportMonth = Year(local.CollectionStartDate.Date) * 100
      + Month(local.CollectionStartDate.Date);

    // -- Log report period information to the control report.
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "Report Month............." + NumberToString
            (local.AfcarsSummary.ReportMonth, 10, 6);

          break;
        case 2:
          UseCabDate2TextWithHyphens2();
          local.EabReportSend.RptDetail = "Starting Report Date....." + local
            .TextWorkArea.Text10;

          break;
        case 3:
          UseCabDate2TextWithHyphens1();
          local.EabReportSend.RptDetail = "Ending Report Date......." + local
            .TextWorkArea.Text10;

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
          "(03) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.PreviousSupported.Number = "";
    local.AfcarsSummary.CollectionAmount = 0;

    // -------------------------------------------------------------------------------------
    // -- Process each supported person with a payment in the processing month.
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadCsePersonCollection())
    {
      if (!Equal(entities.Supported1.Number, local.PreviousSupported.Number) &&
        !IsEmpty(local.PreviousSupported.Number))
      {
        ++local.TotalNumbSupportedRead.Count;

        try
        {
          CreateAfcarsSummary();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "AFCARS_SUMMARY_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "AFCARS_SUMMARY_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Extract the exit state message and write to the error report.
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error creating AFCARS Summary record..." + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        ++local.NumbSuppSinceChckpnt.Count;

        // -- Commit processing.
        if (local.NumbSuppSinceChckpnt.Count > local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
        {
          // -- Checkpoint.
          // -------------------------------------------------------------------------------------
          //  Checkpoint Info...
          // 	Position  Description
          // 	--------  
          // ---------------------------------------------------------
          //         001-009   Number of Supported Persons Previously Processed
          //         010-010   Blank
          // 	011-020   Last Supported Person Number Processed
          // -------------------------------------------------------------------------------------
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo =
            NumberToString(local.TotalNumbSupportedRead.Count, 7, 9) + " " + local
            .PreviousSupported.Number;
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Error committing.";
            UseCabErrorReport2();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }

          local.NumbSuppSinceChckpnt.Count = 0;
        }

        local.AfcarsSummary.CollectionAmount = 0;
      }

      local.PreviousSupported.Number = entities.Supported1.Number;
      local.AfcarsSummary.CollectionAmount =
        local.AfcarsSummary.CollectionAmount.GetValueOrDefault() + entities
        .Collection.Amount;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -------------------------------------------------------------------------------------
      // --  Create AFCARS Summary record for the final supported person 
      // processed.
      // -------------------------------------------------------------------------------------
      if (local.AfcarsSummary.CollectionAmount.GetValueOrDefault() > 0)
      {
        ++local.TotalNumbSupportedRead.Count;

        try
        {
          CreateAfcarsSummary();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "AFCARS_SUMMARY_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "AFCARS_SUMMARY_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Extract the exit state message and write to the error report.
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error creating AFCARS Summary record..." + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto Test;
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
            "Total Number of Supported Persons Processed.........." + NumberToString
            (local.TotalNumbSupportedRead.Count, 9, 7);
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
            "(02) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto Test;
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
      }
    }

Test:

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
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveEabReportSend(EabReportSend source,
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
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.CollectionEndDate.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.CollectionStartDate.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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

  private void UseFnBuildTimestampFrmDateTime1()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea(local.CollectionEndDate, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    local.CollectionEndDate.Assign(useExport.DateWorkArea);
  }

  private void UseFnBuildTimestampFrmDateTime2()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea(local.CollectionStartDate, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    local.CollectionStartDate.Assign(useExport.DateWorkArea);
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

  private void CreateAfcarsSummary()
  {
    var cspNumber = local.PreviousSupported.Number;
    var reportMonth = local.AfcarsSummary.ReportMonth;
    var collectionAmount =
      local.AfcarsSummary.CollectionAmount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.AfcarsSummary.Populated = false;
    Update("CreateAfcarsSummary",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetNullableDecimal(command, "collectionAmt", collectionAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
      });

    entities.AfcarsSummary.CspNumber = cspNumber;
    entities.AfcarsSummary.ReportMonth = reportMonth;
    entities.AfcarsSummary.CollectionAmount = collectionAmount;
    entities.AfcarsSummary.CreatedBy = createdBy;
    entities.AfcarsSummary.CreatedTimestamp = createdTimestamp;
    entities.AfcarsSummary.Populated = true;
  }

  private IEnumerable<bool> ReadCsePersonCollection()
  {
    entities.Collection.Populated = false;
    entities.Supported1.Populated = false;

    return ReadEach("ReadCsePersonCollection",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          local.CollectionStartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          local.CollectionEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableString(
          command, "cspSupNumber", local.RestartSupported.Number);
      },
      (db, reader) =>
      {
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Collection.AppliedToCode = db.GetString(reader, 2);
        entities.Collection.CollectionDt = db.GetDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.CrtType = db.GetInt32(reader, 6);
        entities.Collection.CstId = db.GetInt32(reader, 7);
        entities.Collection.CrvId = db.GetInt32(reader, 8);
        entities.Collection.CrdId = db.GetInt32(reader, 9);
        entities.Collection.ObgId = db.GetInt32(reader, 10);
        entities.Collection.CspNumber = db.GetString(reader, 11);
        entities.Collection.CpaType = db.GetString(reader, 12);
        entities.Collection.OtrId = db.GetInt32(reader, 13);
        entities.Collection.OtrType = db.GetString(reader, 14);
        entities.Collection.OtyId = db.GetInt32(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        entities.Supported1.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);

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
    /// <summary>
    /// A value of PreviousSupported.
    /// </summary>
    [JsonPropertyName("previousSupported")]
    public CsePerson PreviousSupported
    {
      get => previousSupported ??= new();
      set => previousSupported = value;
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
    /// A value of AfcarsSummary.
    /// </summary>
    [JsonPropertyName("afcarsSummary")]
    public AfcarsSummary AfcarsSummary
    {
      get => afcarsSummary ??= new();
      set => afcarsSummary = value;
    }

    /// <summary>
    /// A value of CollectionEndDate.
    /// </summary>
    [JsonPropertyName("collectionEndDate")]
    public DateWorkArea CollectionEndDate
    {
      get => collectionEndDate ??= new();
      set => collectionEndDate = value;
    }

    /// <summary>
    /// A value of CollectionStartDate.
    /// </summary>
    [JsonPropertyName("collectionStartDate")]
    public DateWorkArea CollectionStartDate
    {
      get => collectionStartDate ??= new();
      set => collectionStartDate = value;
    }

    /// <summary>
    /// A value of RestartSupported.
    /// </summary>
    [JsonPropertyName("restartSupported")]
    public CsePerson RestartSupported
    {
      get => restartSupported ??= new();
      set => restartSupported = value;
    }

    /// <summary>
    /// A value of NumbSuppSinceChckpnt.
    /// </summary>
    [JsonPropertyName("numbSuppSinceChckpnt")]
    public Common NumbSuppSinceChckpnt
    {
      get => numbSuppSinceChckpnt ??= new();
      set => numbSuppSinceChckpnt = value;
    }

    /// <summary>
    /// A value of TotalNumbSupportedRead.
    /// </summary>
    [JsonPropertyName("totalNumbSupportedRead")]
    public Common TotalNumbSupportedRead
    {
      get => totalNumbSupportedRead ??= new();
      set => totalNumbSupportedRead = value;
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

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private CsePerson previousSupported;
    private DateWorkArea dateWorkArea;
    private AfcarsSummary afcarsSummary;
    private DateWorkArea collectionEndDate;
    private DateWorkArea collectionStartDate;
    private CsePerson restartSupported;
    private Common numbSuppSinceChckpnt;
    private Common totalNumbSupportedRead;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AfcarsSummary.
    /// </summary>
    [JsonPropertyName("afcarsSummary")]
    public AfcarsSummary AfcarsSummary
    {
      get => afcarsSummary ??= new();
      set => afcarsSummary = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    private AfcarsSummary afcarsSummary;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private Collection collection;
    private ObligationTransaction debt;
    private CsePerson supported1;
    private CsePersonAccount supported2;
  }
#endregion
}
