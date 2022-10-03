// Program: SI_B281_WRITE_DHR_FILE, ID: 373005245, model: 746.
// Short name: SWEI281B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B281_WRITE_DHR_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB281WriteDhrFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B281_WRITE_DHR_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB281WriteDhrFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB281WriteDhrFile.
  /// </summary>
  public SiB281WriteDhrFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //                M A I N T E N A N C E   L O G
    // Date		Developer	Request		Description
    // ------------------------------------------------------------
    // 07/17/2001	M Ramirez			Initial Development
    // ------------------------------------------------------------
    // ------------------------------------------------------------
    // PROCESSING DESCRIPTION
    // ------------------------------------------------------------
    // READs CSENet Trans Envelopes that have Processing_Status = D
    // and Last_Updated_Timestamp > the last time this program
    // was ran
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = "SWEIB281";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------------
    // Open the ERROR Report
    // -----------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.Open.ProgramName = local.ProgramProcessingInfo.Name;
    local.Open.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -----------------------------------------
    // Open the CONTROL Report
    // -----------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered opening the Control Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------
    // Open the DHR file
    // -----------------------------------------
    local.External.FileInstruction = "OPEN";
    UseSiEabWritePersonInfo2();

    if (!IsEmpty(local.External.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered opening the DHR file.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------
    // Get Processing Parameters
    // -----------------------------------------
    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      if (Find(local.ProgramProcessingInfo.ParameterList, "DEBUG") > 0)
      {
        local.Debug.Flag = "Y";
      }
      else
      {
        local.Debug.Flag = "N";
      }
    }
    else
    {
      local.Debug.Flag = "N";
    }

    // -----------------------------------------
    // Get Checkpoint restart information
    // -----------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "Error encountered reading the Checkpoint Restart Information.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------
    // If this is the first run set the last run to two days ago
    // Since si_b280 and si_b281 are in the same job and that job is weekly, 
    // this should get any LO1-R txns that are waiting on si_b281
    // -----------------------------------------------------------------------------
    if (Equal(local.ProgramCheckpointRestart.LastCheckpointTimestamp,
      local.LastRun.Timestamp))
    {
      local.LastRun.Timestamp = Now().AddDays(-2);
    }
    else
    {
      local.LastRun.Timestamp =
        local.ProgramCheckpointRestart.LastCheckpointTimestamp;
    }

    local.External.FileInstruction = "WRITE";
    local.SiWageIncomeSourceRec.CseIndicator = "C";
    local.EabFileHandling.Action = "WRITE";

    foreach(var item in ReadCsenetTransactionEnvelopInterstateCaseInterstateApIdentification())
      
    {
      if (AsChar(local.Debug.Flag) == 'Y')
      {
        local.Write.RptDetail = "DEBUG:  Read Interstate Case; Serial # = " + NumberToString
          (entities.InterstateCase.TransSerialNumber, 4, 12) + ", Date = " + NumberToString
          (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      ++local.LcontrolTotalReads.Count;

      if (!IsEmpty(entities.InterstateApIdentification.Ssn) && !
        Equal(entities.InterstateApIdentification.Ssn, "000000000"))
      {
        // -------------------------------------------
        // Write record to file
        // -------------------------------------------
        local.SiWageIncomeSourceRec.PersonSsn =
          entities.InterstateApIdentification.Ssn ?? Spaces(9);
        UseSiEabWritePersonInfo1();

        if (!IsEmpty(local.External.TextReturnCode))
        {
          local.Write.RptDetail =
            "Error writing record to DHR Locate Request file for IS Case Serial # " +
            NumberToString(entities.InterstateCase.TransSerialNumber, 4, 12) + ", " +
            NumberToString
            (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        ++local.LcontrolTotalWrittenToFile.Count;

        if (AsChar(local.Debug.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.Write.RptDetail = "DEBUG:  Wrote SSN to DHR File;  SSN = " + local
            .SiWageIncomeSourceRec.PersonSsn;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }

      if (!IsEmpty(entities.InterstateApIdentification.AliasSsn1) && !
        Equal(entities.InterstateApIdentification.AliasSsn1, "000000000"))
      {
        // -------------------------------------------
        // Write record to file
        // -------------------------------------------
        local.SiWageIncomeSourceRec.PersonSsn =
          entities.InterstateApIdentification.AliasSsn1 ?? Spaces(9);
        UseSiEabWritePersonInfo1();

        if (!IsEmpty(local.External.TextReturnCode))
        {
          local.Write.RptDetail =
            "Error writing record to DHR Locate Request file for IS Case Serial # " +
            NumberToString(entities.InterstateCase.TransSerialNumber, 4, 12) + ", " +
            NumberToString
            (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        ++local.LcontrolTotalWrittenToFile.Count;

        if (AsChar(local.Debug.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.Write.RptDetail = "DEBUG:  Wrote SSN to DHR File;  SSN = " + local
            .SiWageIncomeSourceRec.PersonSsn;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }

      if (!IsEmpty(entities.InterstateApIdentification.AliasSsn2) && !
        Equal(entities.InterstateApIdentification.AliasSsn2, "000000000"))
      {
        // -------------------------------------------
        // Write record to file
        // -------------------------------------------
        local.SiWageIncomeSourceRec.PersonSsn =
          entities.InterstateApIdentification.AliasSsn2 ?? Spaces(9);
        UseSiEabWritePersonInfo1();

        if (!IsEmpty(local.External.TextReturnCode))
        {
          local.Write.RptDetail =
            "Error writing record to DHR Locate Request file for IS Case Serial # " +
            NumberToString(entities.InterstateCase.TransSerialNumber, 4, 12) + ", " +
            NumberToString
            (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          return;
        }

        ++local.LcontrolTotalWrittenToFile.Count;

        if (AsChar(local.Debug.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.Write.RptDetail = "DEBUG:  Wrote SSN to DHR File;  SSN = " + local
            .SiWageIncomeSourceRec.PersonSsn;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }

      if (AsChar(local.Debug.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.Write.RptDetail = "DEBUG:  Processed successfully";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    // -----------------------------------------
    // Update Checkpoint restart
    // -----------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.CheckpointCount = 1;
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "Error encountered updating the Program Checkpoint Restart information.";
        
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -----------------------------------------
    // Close DHR file
    // -----------------------------------------
    local.External.FileInstruction = "CLOSE";
    UseSiEabWritePersonInfo2();

    if (!IsEmpty(local.External.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered closing the DHR Locate File.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -----------------------------------------
    // Write the CONTROL Report
    // -----------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail = "Number of Quick Locate Requests read      " + NumberToString
      (local.LcontrolTotalReads.Count, 15);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "Error encountered writing to the Control Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.Write.RptDetail = "Records written to DHR Interface file     " + NumberToString
      (local.LcontrolTotalWrittenToFile.Count, 15);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "Error encountered writing to the Control Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -----------------------------------------
    // Close the CONTROL Report
    // -----------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered closing the Control Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -----------------------------------------
    // Close the ERROR Report
    // -----------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered closing the Error Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
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
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseSiEabWritePersonInfo1()
  {
    var useImport = new SiEabWritePersonInfo.Import();
    var useExport = new SiEabWritePersonInfo.Export();

    useImport.SiWageIncomeSourceRec.Assign(local.SiWageIncomeSourceRec);
    useImport.External.Assign(local.External);
    useExport.External.Assign(local.External);

    Call(SiEabWritePersonInfo.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseSiEabWritePersonInfo2()
  {
    var useImport = new SiEabWritePersonInfo.Import();
    var useExport = new SiEabWritePersonInfo.Export();

    useImport.External.Assign(local.External);
    useExport.External.Assign(local.External);

    Call(SiEabWritePersonInfo.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool>
    ReadCsenetTransactionEnvelopInterstateCaseInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;
    entities.InterstateCase.Populated = false;
    entities.CsenetTransactionEnvelop.Populated = false;

    return ReadEach(
      "ReadCsenetTransactionEnvelopInterstateCaseInterstateApIdentification",
      (db, command) =>
      {
        db.SetDateTime(
          command, "lastUpdatedTimes",
          local.LastRun.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 3);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 4);
        entities.InterstateCase.ActionCode = db.GetString(reader, 5);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 6);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 7);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 8);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 9);
        entities.InterstateApIdentification.Populated = true;
        entities.InterstateCase.Populated = true;
        entities.CsenetTransactionEnvelop.Populated = true;

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
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public Common Debug
    {
      get => debug ??= new();
      set => debug = value;
    }

    /// <summary>
    /// A value of LastRun.
    /// </summary>
    [JsonPropertyName("lastRun")]
    public DateWorkArea LastRun
    {
      get => lastRun ??= new();
      set => lastRun = value;
    }

    /// <summary>
    /// A value of SiWageIncomeSourceRec.
    /// </summary>
    [JsonPropertyName("siWageIncomeSourceRec")]
    public SiWageIncomeSourceRec SiWageIncomeSourceRec
    {
      get => siWageIncomeSourceRec ??= new();
      set => siWageIncomeSourceRec = value;
    }

    /// <summary>
    /// A value of LcontrolTotalWrittenToFile.
    /// </summary>
    [JsonPropertyName("lcontrolTotalWrittenToFile")]
    public Common LcontrolTotalWrittenToFile
    {
      get => lcontrolTotalWrittenToFile ??= new();
      set => lcontrolTotalWrittenToFile = value;
    }

    /// <summary>
    /// A value of LcontrolTotalReads.
    /// </summary>
    [JsonPropertyName("lcontrolTotalReads")]
    public Common LcontrolTotalReads
    {
      get => lcontrolTotalReads ??= new();
      set => lcontrolTotalReads = value;
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
    /// A value of CheckpointReads.
    /// </summary>
    [JsonPropertyName("checkpointReads")]
    public Common CheckpointReads
    {
      get => checkpointReads ??= new();
      set => checkpointReads = value;
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
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabReportSend Write
    {
      get => write ??= new();
      set => write = value;
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

    private Common debug;
    private DateWorkArea lastRun;
    private SiWageIncomeSourceRec siWageIncomeSourceRec;
    private Common lcontrolTotalWrittenToFile;
    private Common lcontrolTotalReads;
    private External external;
    private Common checkpointReads;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend open;
    private EabReportSend write;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
  }
#endregion
}
