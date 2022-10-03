// Program: FN_B718_REPORT_FOR_AUDITORS, ID: 373372980, model: 746.
// Short name: SWEF718B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B718_REPORT_FOR_AUDITORS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB718ReportForAuditors: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B718_REPORT_FOR_AUDITORS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB718ReportForAuditors(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB718ReportForAuditors.
  /// </summary>
  public FnB718ReportForAuditors(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------
    // Initial Version -  07/2002.
    // -----------------------------------------------------------
    // ------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ---------- 	------------	
    // --------------------------------------------------------------
    // 07/??/02  ???????	????????	Initial Development.
    // 06/28/04  GVandy	PR 212464	1) Add fiscal year range PPI parameters.
    // 					2) Change to use Kansas fiscal year verses federal fiscal year.
    // 					3) Add checkpointing by fiscal year.
    // 					4) Change all reads to with UR and remove cursor holds.
    // ------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = global.UserId;
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // WRITE DISPLAY STATEMENTS.
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      Substring(
        "RUN PARAMETERS....................................................................",
      1, 50);
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // ***  Get the run parameters for this program.
    // **********************************************************
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Fatal error occurred during reading program processing info.  " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // *******************************************************************
    // Parameter List:
    // Starting Fiscal Year: Positions 1-4
    // Ending Fiscal Year: Positions 6-9
    // *******************************************************************
    local.FromFiscalYearDateWorkAttributes.TextYear =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 4);

    if (Verify(local.FromFiscalYearDateWorkAttributes.TextYear, "0123456789") ==
      0)
    {
      local.FromFiscalYearDateWorkAttributes.NumericalYear =
        (int)StringToNumber(local.FromFiscalYearDateWorkAttributes.TextYear);
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "    STARTING FISCAL YEAR...." + local
        .FromFiscalYearDateWorkAttributes.TextYear;
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Invalid Starting Fiscal Year on PPI Record (Position 1-4)...  " + local
        .FromFiscalYearDateWorkAttributes.TextYear;
      UseCabErrorReport2();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.ToFiscalYearDateWorkAttributes.TextYear =
      Substring(local.ProgramProcessingInfo.ParameterList, 6, 4);

    if (Verify(local.ToFiscalYearDateWorkAttributes.TextYear, "0123456789") == 0
      )
    {
      local.ToFiscalYearDateWorkAttributes.NumericalYear =
        (int)StringToNumber(local.ToFiscalYearDateWorkAttributes.TextYear);
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "    ENDING FISCAL YEAR......" + local
        .ToFiscalYearDateWorkAttributes.TextYear;
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Invalid Ending Fiscal Year on PPI Record (Position 6-9)...  " + local
        .ToFiscalYearDateWorkAttributes.TextYear;
      UseCabErrorReport2();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // Determine if restarting.
    // **********************************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        local.FromFiscalYearDateWorkAttributes.NumericalYear =
          (int)(
            StringToNumber(local.FromFiscalYearDateWorkAttributes.TextYear) + 1
          );
        local.FromFiscalYearDateWorkAttributes.TextYear =
          NumberToString(local.FromFiscalYearDateWorkAttributes.NumericalYear,
          12, 4);
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "RESTARTING AT FISCAL YEAR......" + local
          .FromFiscalYearDateWorkAttributes.TextYear;
        UseCabControlReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabControlReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Fatal error occurred, must abort.  " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.Fy.Count = local.FromFiscalYearDateWorkAttributes.NumericalYear;

    for(var limit = local.ToFiscalYearDateWorkAttributes.NumericalYear; local
      .Fy.Count <= limit; ++local.Fy.Count)
    {
      if (local.Fy.Count == 2000)
      {
        local.StartDate.Date = new DateTime(1999, 9, 1);
        local.EndDate.Date = new DateTime(2000, 6, 30);
        local.EabReportSend.RptDetail = "Period : 09/01/1999 thru 06/30/2000";
      }
      else
      {
        local.StartDate.Date = IntToDate((int)((local.Fy.Count - 1) * (
          long)10000 + 701));
        local.EndDate.Date =
          IntToDate((int)((long)local.Fy.Count * 10000 + 630));
        local.EabReportSend.RptDetail = "Period : 07/01/" + NumberToString
          ((long)local.Fy.Count - 1, 12, 4) + " thru 06/30/" + NumberToString
          (local.Fy.Count, 12, 4);
      }

      UseFnBuildTimestampFrmDateTime2();
      UseFnBuildTimestampFrmDateTime1();
      local.EndDate.Timestamp =
        AddSeconds(AddDays(local.EndDate.Timestamp, 1), -1);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      foreach(var item in ReadCase2())
      {
        if (!ReadLegalReferralOffice())
        {
          continue;
        }

        foreach(var item1 in ReadCsePersonCsePerson())
        {
          if (ReadCollection2())
          {
            if (entities.Office.SystemGeneratedId >= 300)
            {
              ++local.PayingCasesContr.Count;
            }
            else
            {
              ++local.PayingCasesSrs.Count;
            }

            goto ReadEach;
          }

          if (ReadCollection1())
          {
            if (entities.Office.SystemGeneratedId >= 300)
            {
              ++local.PayingCasesContr.Count;
            }
            else
            {
              ++local.PayingCasesSrs.Count;
            }

            goto ReadEach;
          }

          if (ReadDebt2())
          {
            if (entities.Office.SystemGeneratedId >= 300)
            {
              ++local.TotalCasesContr.Count;
            }
            else
            {
              ++local.TotalCasesSrs.Count;
            }

            goto ReadEach;
          }

          if (ReadDebt1())
          {
            if (entities.Office.SystemGeneratedId >= 300)
            {
              ++local.TotalCasesContr.Count;
            }
            else
            {
              ++local.TotalCasesSrs.Count;
            }
          }
        }

ReadEach:
        ;
      }

      foreach(var item in ReadCollectionCsePersonCashReceiptType2())
      {
        if (!ReadCsePerson())
        {
          continue;
        }

        if (entities.CashReceiptType.SystemGeneratedIdentifier == 2 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 7)
        {
          if (!ReadCollectionType())
          {
            continue;
          }

          if (entities.CollectionType.SequentialIdentifier == 14 || entities
            .CollectionType.SequentialIdentifier == 20 || entities
            .CollectionType.SequentialIdentifier == 21)
          {
          }
          else
          {
            continue;
          }
        }

        if (!ReadCase1())
        {
          continue;
        }

        if (!ReadLegalReferralOffice())
        {
          continue;
        }

        if (entities.Office.SystemGeneratedId >= 300)
        {
          local.CsCollContr.Count =
            (int)Math.Round(
              local.CsCollContr.Count +
            entities.Collection.Amount, MidpointRounding.AwayFromZero);
        }
        else
        {
          local.CsCollSrs.Count =
            (int)Math.Round(
              local.CsCollSrs.Count +
            entities.Collection.Amount, MidpointRounding.AwayFromZero);
        }
      }

      // --------------------------------------------------------------
      // 3. Collection per Case - Total Collections / Paying Cases
      // --------------------------------------------------------------
      foreach(var item in ReadCollectionCsePersonCashReceiptType1())
      {
        if (!ReadCsePerson())
        {
          continue;
        }

        if (!ReadCollectionType())
        {
          continue;
        }

        // ***  Exclude CSE Net
        if (entities.CollectionType.SequentialIdentifier == 7 || entities
          .CollectionType.SequentialIdentifier == 8 || entities
          .CollectionType.SequentialIdentifier == 9)
        {
          continue;
        }

        if (entities.CashReceiptType.SystemGeneratedIdentifier == 2 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 7)
        {
          // ***  Include Direct payments. Exclude other non-cash pmnts.
          if (entities.CollectionType.SequentialIdentifier == 14 || entities
            .CollectionType.SequentialIdentifier == 20 || entities
            .CollectionType.SequentialIdentifier == 21)
          {
          }
          else
          {
            continue;
          }
        }

        if (!ReadCase1())
        {
          continue;
        }

        if (!ReadLegalReferralOffice())
        {
          continue;
        }

        if (entities.Office.SystemGeneratedId >= 300)
        {
          local.TotalCont.Count =
            (int)Math.Round(
              local.TotalCont.Count +
            entities.Collection.Amount, MidpointRounding.AwayFromZero);
        }
        else
        {
          local.TotalSrs.Count =
            (int)Math.Round(
              local.TotalSrs.Count +
            entities.Collection.Amount, MidpointRounding.AwayFromZero);
        }

        // --------------------------------------------------------------
        // 4. TAF Colls - Pgm applied to is AF, FC, AFI or FCI.
        // --------------------------------------------------------------
        if (Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
          (entities.Collection.ProgramAppliedTo, "FC") || Equal
          (entities.Collection.ProgramAppliedTo, "AFI") || Equal
          (entities.Collection.ProgramAppliedTo, "FCI"))
        {
          if (entities.Office.SystemGeneratedId >= 300)
          {
            local.TafCont.Count =
              (int)Math.Round(
                local.TafCont.Count +
              entities.Collection.Amount, MidpointRounding.AwayFromZero);
          }
          else
          {
            local.TafSrs.Count =
              (int)Math.Round(
                local.TafSrs.Count +
              entities.Collection.Amount, MidpointRounding.AwayFromZero);
          }
        }
      }

      local.TotalCasesContr.Count = (int)((long)local.TotalCasesContr.Count + local
        .PayingCasesContr.Count);
      local.TotalCasesSrs.Count = (int)((long)local.TotalCasesSrs.Count + local
        .PayingCasesSrs.Count);
      local.NonTafCont.Count = (int)((long)local.TotalCont.Count - local
        .TafCont.Count);
      local.NonTafSrs.Count = (int)((long)local.TotalSrs.Count - local
        .TafSrs.Count);

      if (local.PayingCasesSrs.Count > 0)
      {
        local.CollPerCaseSrs.Count =
          (int)Math.Round((decimal)local.TotalSrs.Count /
          local.PayingCasesSrs.Count, MidpointRounding.AwayFromZero);
      }
      else
      {
        local.CollPerCaseSrs.Count = 0;
      }

      if (local.PayingCasesContr.Count > 0)
      {
        local.CollPerCaseCont.Count =
          (int)Math.Round((decimal)local.TotalCont.Count /
          local.PayingCasesContr.Count, MidpointRounding.AwayFromZero);
      }
      else
      {
        local.CollPerCaseCont.Count = 0;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring(Substring(
          "Number of cases with orders.................................", 1,
        35) + "SRS-" + NumberToString(local.TotalCasesSrs.Count, 7, 9), 1, 50);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "   Contractor-" +
        NumberToString(local.TotalCasesContr.Count, 7, 9);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring(Substring(
          "Number of paying cases....................................................",
        1, 35) + "SRS-"
        + NumberToString(local.PayingCasesSrs.Count, 7, 9), 1, 50);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "   Contractor-" +
        NumberToString(local.PayingCasesContr.Count, 7, 9);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring(Substring(
          "Current Support Collected....................................................",
        1, 35) + "SRS-" + NumberToString(local.CsCollSrs.Count, 7, 9), 1, 50);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "   Contractor-" +
        NumberToString(local.CsCollContr.Count, 7, 9);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring(Substring(
          "Collection per case...................................................",
        1, 35) + "SRS-"
        + NumberToString(local.CollPerCaseSrs.Count / 1, 7, 9), 1, 50);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "   Contractor-" +
        NumberToString(local.CollPerCaseCont.Count, 7, 9);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring(Substring(
          "TAF $ collected...........................................................",
        1, 35) + "SRS-" + NumberToString(local.TafSrs.Count, 7, 9), 1, 50);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "   Contractor-" +
        NumberToString(local.TafCont.Count, 7, 9);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring(Substring(
          "Non-TAF $ collected.....................................................",
        1, 35) + "SRS-" + NumberToString(local.NonTafSrs.Count, 7, 9), 1, 50);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "   Contractor-" +
        NumberToString(local.NonTafCont.Count, 7, 9);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        Substring(
          "************************************************************************************************************"
        , 1, 75);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // **********************************************************
      // Checkpoint
      // **********************************************************
      local.ProgramCheckpointRestart.RestartInd = "Y";
      local.ProgramCheckpointRestart.RestartInfo =
        NumberToString(local.Fy.Count, 12, 4);
      UseUpdateCheckpointRstAndCommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Fatal error occurred during checkpoint, must abort.  " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        if (!Equal(local.Status.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        return;
      }
    }

    // **********************************************************
    // Take a final checkpoint
    // **********************************************************
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Fatal error occurred during final checkpoint, must abort.  " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport3();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // CLOSE OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
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

    useImport.DateWorkArea.Date = local.EndDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, local.EndDate);
  }

  private void UseFnBuildTimestampFrmDateTime2()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    useImport.DateWorkArea.Date = local.StartDate.Date;

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea(useExport.DateWorkArea, local.StartDate);
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

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.EndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.StartDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCollection1()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
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
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
      });
  }

  private bool ReadCollection2()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
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
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollectionCsePersonCashReceiptType1()
  {
    entities.CashReceiptType.Populated = false;
    entities.Collection.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCollectionCsePersonCashReceiptType1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst1",
          local.StartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2", local.EndDate.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.ApCsePerson.Number = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.CashReceiptType.Populated = true;
        entities.Collection.Populated = true;
        entities.ApCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionCsePersonCashReceiptType2()
  {
    entities.CashReceiptType.Populated = false;
    entities.Collection.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCollectionCsePersonCashReceiptType2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst1",
          local.StartDate.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2", local.EndDate.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.ApCsePerson.Number = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.CashReceiptType.Populated = true;
        entities.Collection.Populated = true;
        entities.ApCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ChCsePerson.Populated = false;

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
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCsePerson()
  {
    entities.ChCsePerson.Populated = false;
    entities.ApCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Number = db.GetString(reader, 1);
        entities.ChCsePerson.Populated = true;
        entities.ApCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadDebt1()
  {
    entities.Debt.Populated = false;

    return Read("ReadDebt1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.Debt.Populated = true;
      });
  }

  private bool ReadDebt2()
  {
    entities.Debt.Populated = false;

    return Read("ReadDebt2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspSupNumber", entities.ChCsePerson.Number);
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.Debt.Populated = true;
      });
  }

  private bool ReadLegalReferralOffice()
  {
    entities.LegalReferral.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadLegalReferralOffice",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 4);
        entities.LegalReferral.Populated = true;
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
    /// A value of ToFiscalYearDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("toFiscalYearDateWorkAttributes")]
    public DateWorkAttributes ToFiscalYearDateWorkAttributes
    {
      get => toFiscalYearDateWorkAttributes ??= new();
      set => toFiscalYearDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of FromFiscalYearDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("fromFiscalYearDateWorkAttributes")]
    public DateWorkAttributes FromFiscalYearDateWorkAttributes
    {
      get => fromFiscalYearDateWorkAttributes ??= new();
      set => fromFiscalYearDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of ToFiscalYearDateWorkArea.
    /// </summary>
    [JsonPropertyName("toFiscalYearDateWorkArea")]
    public DateWorkArea ToFiscalYearDateWorkArea
    {
      get => toFiscalYearDateWorkArea ??= new();
      set => toFiscalYearDateWorkArea = value;
    }

    /// <summary>
    /// A value of FromFiscalYearDateWorkArea.
    /// </summary>
    [JsonPropertyName("fromFiscalYearDateWorkArea")]
    public DateWorkArea FromFiscalYearDateWorkArea
    {
      get => fromFiscalYearDateWorkArea ??= new();
      set => fromFiscalYearDateWorkArea = value;
    }

    /// <summary>
    /// A value of CollPerCaseCont.
    /// </summary>
    [JsonPropertyName("collPerCaseCont")]
    public Common CollPerCaseCont
    {
      get => collPerCaseCont ??= new();
      set => collPerCaseCont = value;
    }

    /// <summary>
    /// A value of CollPerCaseSrs.
    /// </summary>
    [JsonPropertyName("collPerCaseSrs")]
    public Common CollPerCaseSrs
    {
      get => collPerCaseSrs ??= new();
      set => collPerCaseSrs = value;
    }

    /// <summary>
    /// A value of NonTafCont.
    /// </summary>
    [JsonPropertyName("nonTafCont")]
    public Common NonTafCont
    {
      get => nonTafCont ??= new();
      set => nonTafCont = value;
    }

    /// <summary>
    /// A value of NonTafSrs.
    /// </summary>
    [JsonPropertyName("nonTafSrs")]
    public Common NonTafSrs
    {
      get => nonTafSrs ??= new();
      set => nonTafSrs = value;
    }

    /// <summary>
    /// A value of TotalCont.
    /// </summary>
    [JsonPropertyName("totalCont")]
    public Common TotalCont
    {
      get => totalCont ??= new();
      set => totalCont = value;
    }

    /// <summary>
    /// A value of TafCont.
    /// </summary>
    [JsonPropertyName("tafCont")]
    public Common TafCont
    {
      get => tafCont ??= new();
      set => tafCont = value;
    }

    /// <summary>
    /// A value of TotalSrs.
    /// </summary>
    [JsonPropertyName("totalSrs")]
    public Common TotalSrs
    {
      get => totalSrs ??= new();
      set => totalSrs = value;
    }

    /// <summary>
    /// A value of TafSrs.
    /// </summary>
    [JsonPropertyName("tafSrs")]
    public Common TafSrs
    {
      get => tafSrs ??= new();
      set => tafSrs = value;
    }

    /// <summary>
    /// A value of CsCollSrs.
    /// </summary>
    [JsonPropertyName("csCollSrs")]
    public Common CsCollSrs
    {
      get => csCollSrs ??= new();
      set => csCollSrs = value;
    }

    /// <summary>
    /// A value of CsCollContr.
    /// </summary>
    [JsonPropertyName("csCollContr")]
    public Common CsCollContr
    {
      get => csCollContr ??= new();
      set => csCollContr = value;
    }

    /// <summary>
    /// A value of PayingCasesSrs.
    /// </summary>
    [JsonPropertyName("payingCasesSrs")]
    public Common PayingCasesSrs
    {
      get => payingCasesSrs ??= new();
      set => payingCasesSrs = value;
    }

    /// <summary>
    /// A value of TotalCasesContr.
    /// </summary>
    [JsonPropertyName("totalCasesContr")]
    public Common TotalCasesContr
    {
      get => totalCasesContr ??= new();
      set => totalCasesContr = value;
    }

    /// <summary>
    /// A value of Fy.
    /// </summary>
    [JsonPropertyName("fy")]
    public Common Fy
    {
      get => fy ??= new();
      set => fy = value;
    }

    /// <summary>
    /// A value of CommitCount.
    /// </summary>
    [JsonPropertyName("commitCount")]
    public Common CommitCount
    {
      get => commitCount ??= new();
      set => commitCount = value;
    }

    /// <summary>
    /// A value of TotalCasesSrs.
    /// </summary>
    [JsonPropertyName("totalCasesSrs")]
    public Common TotalCasesSrs
    {
      get => totalCasesSrs ??= new();
      set => totalCasesSrs = value;
    }

    /// <summary>
    /// A value of PayingCasesContr.
    /// </summary>
    [JsonPropertyName("payingCasesContr")]
    public Common PayingCasesContr
    {
      get => payingCasesContr ??= new();
      set => payingCasesContr = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
    }

    private DateWorkAttributes toFiscalYearDateWorkAttributes;
    private DateWorkAttributes fromFiscalYearDateWorkAttributes;
    private DateWorkArea toFiscalYearDateWorkArea;
    private DateWorkArea fromFiscalYearDateWorkArea;
    private Common collPerCaseCont;
    private Common collPerCaseSrs;
    private Common nonTafCont;
    private Common nonTafSrs;
    private Common totalCont;
    private Common tafCont;
    private Common totalSrs;
    private Common tafSrs;
    private Common csCollSrs;
    private Common csCollContr;
    private Common payingCasesSrs;
    private Common totalCasesContr;
    private Common fy;
    private Common commitCount;
    private Common totalCasesSrs;
    private Common payingCasesContr;
    private DateWorkArea endDate;
    private DateWorkArea startDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private ExitStateWorkArea exitStateWorkArea;
    private External forCommit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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

    private CaseAssignment caseAssignment;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private Collection collection;
    private ObligationType obligationType;
    private CsePersonAccount supported;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private ObligationTransaction debt;
    private CaseRole chCaseRole;
    private CaseRole apCaseRole;
    private CsePerson chCsePerson;
    private CsePerson apCsePerson;
    private OfficeServiceProvider officeServiceProvider;
    private LegalReferral legalReferral;
    private Office office;
    private LegalReferralAssignment legalReferralAssignment;
    private Case1 case1;
  }
#endregion
}
