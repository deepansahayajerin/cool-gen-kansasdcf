// Program: SI_B272_EXTRACT_AP_FOR_INS, ID: 370991964, model: 746.
// Short name: SWEI272B
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
/// A program: SI_B272_EXTRACT_AP_FOR_INS.
/// </para>
/// <para>
/// This Procedure is for extracting Person numbers and SSN's of all active AR's
/// and AP's
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB272ExtractApForIns: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B272_EXTRACT_AP_FOR_INS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB272ExtractApForIns(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB272ExtractApForIns.
  /// </summary>
  public SiB272ExtractApForIns(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------
    // Date        Developer     Description
    // 10/02/2000  P.Phinney     WR000224 - New Development
    // ------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
      // ***************************************************
      // *Open the ERROR RPT.  DDNAME=RPT99.
      // ***************************************************
      local.EabFileHandling.Action = "OPEN";
      local.NeededToOpen.ProgramName = "SWEIB272";
      local.NeededToOpen.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      // ***************************************************
      // *Open the ERROR RPT.  DDNAME=RPT99.
      // ***************************************************
      local.EabFileHandling.Action = "OPEN";
      local.NeededToOpen.ProgramName = "SWEIB272";
      local.NeededToOpen.ProcessDate = Now().Date;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program Processing ERROR = " + TrimEnd
        (local.ExitStateWorkArea.Message);
      UseCabErrorReport();

      // ***************************************************
      // *  ABEND the Job.
      // ***************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ****************************
    // CHECK IF ADABAS IS AVAILABLE
    // ****************************
    UseCabReadAdabasPersonBatch3();

    if (Equal(local.AbendData.AdabasResponseCd, "0148"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "ADABAS is NOT Availiable";
      UseCabErrorReport();

      // ***************************************************
      // *  ABEND the Job.
      // ***************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // ***************************************************
    // *Get Checkpoint restart information
    // ***************************************************
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
      // ***************************************************
      // * RESTART is NOT Used at this time
      // ***************************************************
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        local.RestartCsePersonsWorkSet.Number =
          Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
        local.RestartCommon.Count =
          (int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 17, 9));
        local.RestartCsePersonsWorkSet.Number = "";
      }
      else
      {
        local.RestartCsePersonsWorkSet.Number = "";
      }
    }
    else
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "CheckPoint restart read ERROR = " + TrimEnd
        (local.ExitStateWorkArea.Message);
      UseCabErrorReport();

      // ***************************************************
      // *  ABEND the Job.
      // ***************************************************
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***************************************************
    // *  SET the literal values.
    // ***************************************************
    local.ApType.Type1 = "AP";
    local.ChType.Type1 = "CH";
    local.Dates.EndDate = AddDays(local.ProgramProcessingInfo.ProcessDate, -1);
    local.Dates.StartDate = AddDays(local.ProgramProcessingInfo.ProcessDate, 1);
    local.Abend.Flag = "N";
    local.FileOpened.Flag = "N";
    local.ClosedStatus.Status = "C";

    // ***************************************************
    // * Process all the selected records
    // ***************************************************
    // ***************************************************
    // * Get the AP Information -- LOOP
    // ***************************************************
    foreach(var item in ReadCsePersonCaseRoleCase())
    {
      ExitState = "ACO_NN0000_ALL_OK";

      if (Equal(entities.ApCsePerson.Number, local.SaveApCsePerson.Number) && Equal
        (entities.ApCase.Number, local.SaveApCase.Number))
      {
        continue;
      }

      if (Equal(entities.ApCsePerson.Number, local.SaveApCsePerson.Number))
      {
        if (IsEmpty(local.Ap.Ssn))
        {
          continue;
        }
      }
      else
      {
        local.Ap.Number = entities.ApCsePerson.Number;
        ExitState = "ACO_NN0000_ALL_OK";
        UseCabReadAdabasPersonBatch1();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ok, continue processing
          if (IsEmpty(local.Ap.Ssn) || Equal(local.Ap.Ssn, "000000000"))
          {
            continue;
          }

          if (IsEmpty(local.Ap.Number))
          {
            continue;
          }

          if (IsEmpty(local.Ap.FormattedName))
          {
            local.Ap.FormattedName = TrimEnd(local.Ap.FirstName) + " " + TrimEnd
              (local.Ap.MiddleInitial);
            local.Ap.FormattedName = TrimEnd(local.Ap.FormattedName) + " " + TrimEnd
              (local.Ap.LastName);
          }
        }
        else
        {
          // ***************************************************
          // *This is the ADABAS not found condition.
          // ***************************************************
          ++local.ApNotOnAdabas.Count;
          local.SaveApCase.Number = entities.ApCase.Number;
          local.SaveApCsePerson.Number = entities.ApCsePerson.Number;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
      }

      // ***************************************************
      // * Get the Information for EACH "CH"ild -- LOOP
      // ***************************************************
      foreach(var item1 in ReadCaseRoleCsePerson())
      {
        if (Equal(entities.ChCsePerson.Number, local.SaveCh.Number) && Equal
          (entities.ApCase.Number, local.SaveApCase.Number))
        {
          continue;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        local.Ch.Number = entities.ChCsePerson.Number;
        UseCabReadAdabasPersonBatch2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ok, continue processing
          ++local.NoOfChRecsRead.Count;

          if (IsEmpty(local.Ch.FormattedName))
          {
            local.Ch.FormattedName = TrimEnd(local.Ch.FirstName) + " " + TrimEnd
              (local.Ch.MiddleInitial);
            local.Ch.FormattedName = TrimEnd(local.Ch.FormattedName) + " " + TrimEnd
              (local.Ch.LastName);
          }
        }
        else
        {
          // ***************************************************
          // *This is the ADABAS not found condition.
          // ***************************************************
          ++local.ChNotOnAdabas.Count;
          ExitState = "ACO_NN0000_ALL_OK";
          local.SaveCh.Number = entities.ChCsePerson.Number;
          local.SaveApCase.Number = entities.ApCase.Number;

          continue;
        }

        if (AsChar(local.FileOpened.Flag) != 'Y')
        {
          // A/B  External here to OPEN OUTPUT File
          local.WriteExtractPersonInfo.FileInstruction = "OPEN";
          UseSiEabWritePcgMedicaidMatch3();

          if (!IsEmpty(local.WriteExtractPersonInfo.TextReturnCode))
          {
            // ***************************************************
            // *Write a line to the ERROR RPT.
            // ***************************************************
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered opening the Person Information extract file";
            local.NeededToWrite.RptDetail =
              TrimEnd(local.NeededToWrite.RptDetail) + " = " + local
              .WriteExtractPersonInfo.TextReturnCode;
            UseCabErrorReport();
            local.Abend.Flag = "Y";

            goto ReadEach;
          }

          local.FileOpened.Flag = "Y";
        }

        if (!Equal(local.SaveApCsePerson.Number, entities.ApCsePerson.Number))
        {
          ++local.NoOfApFileRecsRead.Count;
          local.SaveApCsePerson.Number = entities.ApCsePerson.Number;
        }

        if (!Equal(local.SaveApCase.Number, entities.ApCase.Number))
        {
          ++local.NoOfCaseRecsRead.Count;
        }

        // A/B  External here to WRITE OUTPUT File
        local.SaveApCase.Number = entities.ApCase.Number;
        local.WriteExtractPersonInfo.FileInstruction = "WRITE";
        UseSiEabWritePcgMedicaidMatch1();

        if (!IsEmpty(local.WriteExtractPersonInfo.TextReturnCode))
        {
          // ***************************************************
          // *Write a line to the ERROR RPT.
          // ***************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered writing the Person Information extract file";
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " = " + local
            .WriteExtractPersonInfo.TextReturnCode;
          UseCabErrorReport();
          ++local.AbendErrors.Count;
        }
        else
        {
          ++local.NoOfOutputRecsWritten.Count;
        }

        local.SaveCh.Number = entities.ChCsePerson.Number;

        // ***************************************************
        // * Bottom of Read Each "CH"ild cse_person loop.
        // ***************************************************
      }

      local.SaveApCase.Number = entities.ApCase.Number;
      local.SaveApCsePerson.Number = entities.ApCsePerson.Number;

      // ***************************************************
      // * Bottom of Read Each "AP" cse_person loop.
      // ***************************************************
    }

ReadEach:

    local.WriteExtractPersonInfo.FileInstruction = "CLOSE";
    UseSiEabWritePcgMedicaidMatch2();

    if (!IsEmpty(local.WriteExtractPersonInfo.TextReturnCode))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Person Information extract file";
      local.NeededToWrite.RptDetail = TrimEnd(local.NeededToWrite.RptDetail) + " = " +
        local.WriteExtractPersonInfo.TextReturnCode;
      UseCabErrorReport();
      local.Abend.Flag = "Y";
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // ***************************************************
    // *Open the CONTROL RPT. DDNAME=RPT98.
    // ***************************************************
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Control Report";
      local.NeededToWrite.RptDetail = TrimEnd(local.NeededToWrite.RptDetail) + " = " +
        local.EabFileHandling.Status;
      UseCabErrorReport();
      local.Abend.Flag = "Y";
    }
    else
    {
      // ***************************************************
      // * Write Summary Totals to the CONTROL REPORT
      // ***************************************************
      // ***************************************************
      // *   SPACING
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***************************************************
        // *Write a line to the ERROR RPT.
        // ***************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing the Control Report file. Spacing1";
        UseCabErrorReport();
        local.Abend.Flag = "Y";

        goto Test;
      }

      // ***************************************************
      // *   Total APs Processed
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "No of AP file records processed    : " + NumberToString
        (local.NoOfApFileRecsRead.Count, 15);
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***************************************************
        // *Write a line to the ERROR RPT.
        // ***************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing the Control Report file.  APs";
        UseCabErrorReport();
        local.Abend.Flag = "Y";

        goto Test;
      }

      // ***************************************************
      // *   Total AP CASES Processed
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "No of CASE file records processed  : " + NumberToString
        (local.NoOfCaseRecsRead.Count, 15);
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***************************************************
        // *Write a line to the ERROR RPT.
        // ***************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing the Control Report file. AP Cases";
        UseCabErrorReport();
        local.Abend.Flag = "Y";

        goto Test;
      }

      // ***************************************************
      // *   Total CH records Processed
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "No of CH file records processed    : " + NumberToString
        (local.NoOfChRecsRead.Count, 15);
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***************************************************
        // *Write a line to the ERROR RPT.
        // ***************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing the Control Report file. CHs";
        UseCabErrorReport();
        local.Abend.Flag = "Y";

        goto Test;
      }

      // ***************************************************
      // *   SPACING
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***************************************************
        // *Write a line to the ERROR RPT.
        // ***************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing the Control Report file. Spacing 2";
        UseCabErrorReport();
        local.Abend.Flag = "Y";

        goto Test;
      }

      // ***************************************************
      // *   Total Output records Processed
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "No of Output file records processed: " + NumberToString
        (local.NoOfOutputRecsWritten.Count, 15);
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***************************************************
        // *Write a line to the ERROR RPT.
        // ***************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing the Control Report file. Output";
        UseCabErrorReport();
        local.Abend.Flag = "Y";

        goto Test;
      }

      // ***************************************************
      // *   Spacing
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***************************************************
        // *Write a line to the ERROR RPT.
        // ***************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered writing the Control Report file. Spacing 3";
        UseCabErrorReport();
        local.Abend.Flag = "Y";

        goto Test;
      }

      // ***************************************************
      // *Close the CONTROL REPORT.
      // ***************************************************
      local.EabFileHandling.Action = "CLOSE";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // ***************************************************
        // *Write a line to the ERROR RPT.
        // ***************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered closing the Control Report file.";
        UseCabErrorReport();
        local.Abend.Flag = "Y";
      }
    }

Test:

    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "CheckPoint restart update ERROR = " + TrimEnd
        (local.ExitStateWorkArea.Message);
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.Abend.Flag = "Y";
      }
    }

    // ***************************************************
    // *   SPACING
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "";
    UseCabErrorReport();

    // ***************************************************
    // *   Total CH records Processed
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "No of AP records NOT on ADABAS   : " + NumberToString
      (local.ApNotOnAdabas.Count, 15);
    UseCabErrorReport();

    // ***************************************************
    // *   Total CH records Processed
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "No of CH records NOT on ADABAS   : " + NumberToString
      (local.ChNotOnAdabas.Count, 15);
    UseCabErrorReport();

    // ***************************************************
    // *   SPACING
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "";
    UseCabErrorReport();

    // ***************************************************
    // *Close the ERROR RPT.
    // ***************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.Abend.Flag = "Y";
    }

    // 05/18/99 Added per e-mail from Terri Struder
    // "batch jobs that call adabas" on 05/15/99 at 10:50 AM
    local.CsePersonsWorkSet.Ssn = "close";
    UseEabReadCsePersonUsingSsn();

    // Do NOT need to verify return
    if (AsChar(local.Abend.Flag) == 'Y')
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextReturnCode = source.TextReturnCode;
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

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabReadAdabasPersonBatch1()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ap.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.Ap.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabReadAdabasPersonBatch2()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ch.Number;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.Ch.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabReadAdabasPersonBatch3()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);
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

  private void UseSiEabWritePcgMedicaidMatch1()
  {
    var useImport = new SiEabWritePcgMedicaidMatch.Import();
    var useExport = new SiEabWritePcgMedicaidMatch.Export();

    useImport.ApCsePersonsWorkSet.Assign(local.Ap);
    useImport.Ch.Assign(local.Ch);
    useImport.ApCase.Number = local.SaveApCase.Number;
    useImport.External.FileInstruction =
      local.WriteExtractPersonInfo.FileInstruction;
    MoveExternal(local.WriteExtractPersonInfo, useExport.External);

    Call(SiEabWritePcgMedicaidMatch.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.WriteExtractPersonInfo);
  }

  private void UseSiEabWritePcgMedicaidMatch2()
  {
    var useImport = new SiEabWritePcgMedicaidMatch.Import();
    var useExport = new SiEabWritePcgMedicaidMatch.Export();

    useImport.ApCase.Number = local.SaveApCase.Number;
    useImport.External.FileInstruction =
      local.WriteExtractPersonInfo.FileInstruction;
    MoveExternal(local.WriteExtractPersonInfo, useExport.External);

    Call(SiEabWritePcgMedicaidMatch.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.WriteExtractPersonInfo);
  }

  private void UseSiEabWritePcgMedicaidMatch3()
  {
    var useImport = new SiEabWritePcgMedicaidMatch.Import();
    var useExport = new SiEabWritePcgMedicaidMatch.Export();

    useImport.External.FileInstruction =
      local.WriteExtractPersonInfo.FileInstruction;
    MoveExternal(local.WriteExtractPersonInfo, useExport.External);

    Call(SiEabWritePcgMedicaidMatch.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.WriteExtractPersonInfo);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.ChCsePerson.Populated = false;
    entities.ChCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", local.RestartCsePersonsWorkSet.Number);
          
        db.SetString(command, "casNumber", entities.ApCase.Number);
        db.SetString(command, "type", local.ChType.Type1);
        db.SetNullableDate(
          command, "startDate", local.Dates.StartDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Dates.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ChCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChCsePerson.Number = db.GetString(reader, 1);
        entities.ChCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCsePerson.Populated = true;
        entities.ChCaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRoleCase()
  {
    entities.ApCase.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ApCaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.RestartCsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "dateOfDeath",
          local.BlankCsePerson.DateOfDeath.GetValueOrDefault());
        db.SetString(command, "type", local.ApType.Type1);
        db.SetNullableDate(
          command, "endDate", local.Dates.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.Dates.StartDate.GetValueOrDefault());
        db.
          SetNullableString(command, "status", local.ClosedStatus.Status ?? "");
          
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 3);
        entities.ApCase.Number = db.GetString(reader, 3);
        entities.ApCaseRole.Type1 = db.GetString(reader, 4);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 5);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.ApCase.Status = db.GetNullableString(reader, 8);
        entities.ApCase.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ApCaseRole.Populated = true;

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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of SaveApCsePerson.
    /// </summary>
    [JsonPropertyName("saveApCsePerson")]
    public CsePerson SaveApCsePerson
    {
      get => saveApCsePerson ??= new();
      set => saveApCsePerson = value;
    }

    /// <summary>
    /// A value of SaveApCase.
    /// </summary>
    [JsonPropertyName("saveApCase")]
    public Case1 SaveApCase
    {
      get => saveApCase ??= new();
      set => saveApCase = value;
    }

    /// <summary>
    /// A value of SaveCh.
    /// </summary>
    [JsonPropertyName("saveCh")]
    public CsePerson SaveCh
    {
      get => saveCh ??= new();
      set => saveCh = value;
    }

    /// <summary>
    /// A value of NoOfApFileRecsRead.
    /// </summary>
    [JsonPropertyName("noOfApFileRecsRead")]
    public Common NoOfApFileRecsRead
    {
      get => noOfApFileRecsRead ??= new();
      set => noOfApFileRecsRead = value;
    }

    /// <summary>
    /// A value of NoOfCaseRecsRead.
    /// </summary>
    [JsonPropertyName("noOfCaseRecsRead")]
    public Common NoOfCaseRecsRead
    {
      get => noOfCaseRecsRead ??= new();
      set => noOfCaseRecsRead = value;
    }

    /// <summary>
    /// A value of NoOfChRecsRead.
    /// </summary>
    [JsonPropertyName("noOfChRecsRead")]
    public Common NoOfChRecsRead
    {
      get => noOfChRecsRead ??= new();
      set => noOfChRecsRead = value;
    }

    /// <summary>
    /// A value of NoOfOutputRecsWritten.
    /// </summary>
    [JsonPropertyName("noOfOutputRecsWritten")]
    public Common NoOfOutputRecsWritten
    {
      get => noOfOutputRecsWritten ??= new();
      set => noOfOutputRecsWritten = value;
    }

    /// <summary>
    /// A value of ChNotOnAdabas.
    /// </summary>
    [JsonPropertyName("chNotOnAdabas")]
    public Common ChNotOnAdabas
    {
      get => chNotOnAdabas ??= new();
      set => chNotOnAdabas = value;
    }

    /// <summary>
    /// A value of ApNotOnAdabas.
    /// </summary>
    [JsonPropertyName("apNotOnAdabas")]
    public Common ApNotOnAdabas
    {
      get => apNotOnAdabas ??= new();
      set => apNotOnAdabas = value;
    }

    /// <summary>
    /// A value of AbendErrors.
    /// </summary>
    [JsonPropertyName("abendErrors")]
    public Common AbendErrors
    {
      get => abendErrors ??= new();
      set => abendErrors = value;
    }

    /// <summary>
    /// A value of ClosedStatus.
    /// </summary>
    [JsonPropertyName("closedStatus")]
    public Case1 ClosedStatus
    {
      get => closedStatus ??= new();
      set => closedStatus = value;
    }

    /// <summary>
    /// A value of Dates.
    /// </summary>
    [JsonPropertyName("dates")]
    public CaseRole Dates
    {
      get => dates ??= new();
      set => dates = value;
    }

    /// <summary>
    /// A value of ChType.
    /// </summary>
    [JsonPropertyName("chType")]
    public CaseRole ChType
    {
      get => chType ??= new();
      set => chType = value;
    }

    /// <summary>
    /// A value of ApType.
    /// </summary>
    [JsonPropertyName("apType")]
    public CaseRole ApType
    {
      get => apType ??= new();
      set => apType = value;
    }

    /// <summary>
    /// A value of BlankCaseRole.
    /// </summary>
    [JsonPropertyName("blankCaseRole")]
    public CaseRole BlankCaseRole
    {
      get => blankCaseRole ??= new();
      set => blankCaseRole = value;
    }

    /// <summary>
    /// A value of BlankCsePerson.
    /// </summary>
    [JsonPropertyName("blankCsePerson")]
    public CsePerson BlankCsePerson
    {
      get => blankCsePerson ??= new();
      set => blankCsePerson = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Tbd.
    /// </summary>
    [JsonPropertyName("tbd")]
    public SiWageIncomeSourceRec Tbd
    {
      get => tbd ??= new();
      set => tbd = value;
    }

    /// <summary>
    /// A value of WriteExtractPersonInfo.
    /// </summary>
    [JsonPropertyName("writeExtractPersonInfo")]
    public External WriteExtractPersonInfo
    {
      get => writeExtractPersonInfo ??= new();
      set => writeExtractPersonInfo = value;
    }

    /// <summary>
    /// A value of RestartCommon.
    /// </summary>
    [JsonPropertyName("restartCommon")]
    public Common RestartCommon
    {
      get => restartCommon ??= new();
      set => restartCommon = value;
    }

    /// <summary>
    /// A value of RestartCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("restartCsePersonsWorkSet")]
    public CsePersonsWorkSet RestartCsePersonsWorkSet
    {
      get => restartCsePersonsWorkSet ??= new();
      set => restartCsePersonsWorkSet = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
    }

    /// <summary>
    /// A value of Abend.
    /// </summary>
    [JsonPropertyName("abend")]
    public Common Abend
    {
      get => abend ??= new();
      set => abend = value;
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

    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ch;
    private CsePerson saveApCsePerson;
    private Case1 saveApCase;
    private CsePerson saveCh;
    private Common noOfApFileRecsRead;
    private Common noOfCaseRecsRead;
    private Common noOfChRecsRead;
    private Common noOfOutputRecsWritten;
    private Common chNotOnAdabas;
    private Common apNotOnAdabas;
    private Common abendErrors;
    private Case1 closedStatus;
    private CaseRole dates;
    private CaseRole chType;
    private CaseRole apType;
    private CaseRole blankCaseRole;
    private CsePerson blankCsePerson;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private SiWageIncomeSourceRec tbd;
    private External writeExtractPersonInfo;
    private Common restartCommon;
    private CsePersonsWorkSet restartCsePersonsWorkSet;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common fileOpened;
    private Common abend;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ApCase.
    /// </summary>
    [JsonPropertyName("apCase")]
    public Case1 ApCase
    {
      get => apCase ??= new();
      set => apCase = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ChCase.
    /// </summary>
    [JsonPropertyName("chCase")]
    public Case1 ChCase
    {
      get => chCase ??= new();
      set => chCase = value;
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
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    private Case1 apCase;
    private CsePerson apCsePerson;
    private CaseRole apCaseRole;
    private Case1 chCase;
    private CsePerson chCsePerson;
    private CaseRole chCaseRole;
  }
#endregion
}
