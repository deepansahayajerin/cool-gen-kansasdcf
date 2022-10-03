// Program: SI_B462_PURGE_INVALID_SSN, ID: 371161396, model: 746.
// Short name: SWEI462B
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
/// A program: SI_B462_PURGE_INVALID_SSN.
/// </para>
/// <para>
/// This program will purge invalid ssn records from the table when all the 
/// cases have been closed and the last one has been closed minium amoun to time
/// (24 months).
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB462PurgeInvalidSsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B462_PURGE_INVALID_SSN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB462PurgeInvalidSsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB462PurgeInvalidSsn.
  /// </summary>
  public SiB462PurgeInvalidSsn(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************************
    // DATE		Developer	Description
    // 05/19/2009      DDupree   	Initial Creation - CQ7189
    // ***********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiB462Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.StartCommon.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;

    do
    {
      local.Postion.Text1 =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.CurrentPosition.Count, 1);

      if (AsChar(local.Postion.Text1) == ';')
      {
        ++local.FieldNumber.Count;
        local.WorkArea.Text15 = "";

        if (local.FieldNumber.Count == 1)
        {
          local.NumOfMonthsWorkArea.Text4 = "";

          if (local.Current.Count == 1)
          {
            local.NumOfMonthsWorkArea.Text4 = "24";
          }
          else
          {
            local.NumOfMonthsWorkArea.Text4 =
              Substring(local.ProgramProcessingInfo.ParameterList,
              local.StartCommon.Count, local.Current.Count - 1);
          }

          local.NumOfMonthsCommon.Count =
            (int)StringToNumber(local.NumOfMonthsWorkArea.Text4);
          local.CompareDate.Date =
            AddMonths(local.ProgramProcessingInfo.ProcessDate, -
            local.NumOfMonthsCommon.Count);
          local.StartCommon.Count = local.CurrentPosition.Count + 1;
          local.Current.Count = 0;

          break;
        }
        else
        {
        }
      }

      ++local.CurrentPosition.Count;
      ++local.Current.Count;

      if (local.CurrentPosition.Count >= 17)
      {
        break;

        // we do not want to get into an endless loop, if for some reason there 
        // is no
        // delimiter in the parameter then when we will get to the last 
        // character we will escape.
      }
    }
    while(!Equal(global.Command, "COMMAND"));

    local.NullDate.Date = new DateTime(1, 1, 1);
    local.StartDate.Date = local.ProgramProcessingInfo.ProcessDate;
    local.StartBatchTimestampWorkArea.IefTimestamp = Now();
    local.TotalNumberRecords.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 15);
    local.NumErrorRecordsWorkArea.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 16, 15);
    local.NumOfRecordsDeleted.Text15 =
      Substring(local.ProgramCheckpointRestart.RestartInfo, 32, 15);
    local.RestartCount.Count =
      local.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault();
    local.NumRecordsDeleted.Count =
      (int)StringToNumber(local.NumOfRecordsDeleted.Text15);
    local.NumErrorRecordsCommon.Count =
      (int)StringToNumber(local.NumErrorRecordsWorkArea.Text15);
    local.TotalNumberProcessed.Count =
      (int)StringToNumber(local.TotalNumberRecords.Text15);
    local.RecordCanBeDeleted.Flag = "";
    ExitState = "ACO_NN0000_ALL_OK";

    foreach(var item in ReadInvalidSsnCsePerson())
    {
      ++local.TotalNumberProcessed.Count;

      if (!Equal(entities.CsePerson.Number, local.PreviousProcessed.Number))
      {
        if (AsChar(local.RecordCanBeDeleted.Flag) != 'N' && !
          IsEmpty(local.PreviousProcessed.Number))
        {
          foreach(var item1 in ReadInvalidSsn())
          {
            DeleteInvalidSsn();
            local.EabFileHandling.Action = "WRITE";
            local.SsnWorkArea.SsnNum9 = entities.ForDeletion.Ssn;
            local.SsnWorkArea.ConvertOption = "1";
            UseCabSsnConvertNumToText();
            local.EabReportSend.RptDetail =
              "Invalid SSN record deleted for CSE Person #: " + TrimEnd
              (local.PreviousProcessed.Number) + " Invalid SSN of  " + local
              .SsnWorkArea.SsnText9;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            ++local.NumRecordsDeleted.Count;
          }
        }

        local.RecordCanBeDeleted.Flag = "";
        local.PreviousProcessed.Number = entities.CsePerson.Number;

        foreach(var item1 in ReadCase())
        {
          if (AsChar(entities.Case1.Status) == 'C')
          {
            if (Lt(entities.Case1.StatusDate, local.CompareDate.Date))
            {
              // this record is ready to be aged off, but all records have to be
              // cases assicoated to the person has to be checked
            }
            else
            {
              // not ready to be purged yet, has not aged enough
              local.NextStatusDate.Date =
                AddMonths(entities.Case1.StatusDate,
                local.NumOfMonthsCommon.Count);
              local.RecordCanBeDeleted.Flag = "N";

              goto Test;
            }
          }
          else
          {
            local.RecordCanBeDeleted.Flag = "N";
            local.NextStatusDate.Date =
              AddMonths(local.StartDate.Date, local.NumOfMonthsCommon.Count);

            goto Test;
          }
        }
      }

Test:

      if (AsChar(local.RecordCanBeDeleted.Flag) == 'N')
      {
        // need to updated the next checked date with today's process date so we
        // will not
        // look at it until the requird time the case most be closed (24 months)
        // has passed
        try
        {
          UpdateInvalidSsn();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.Error.Number = entities.CsePerson.Number;
              ExitState = "INVALID_SSN_NU";
              ++local.NumErrorRecordsCommon.Count;

              break;
            case ErrorCode.PermittedValueViolation:
              local.Error.Number = entities.CsePerson.Number;
              ExitState = "INVALID_SSN_PV";
              ++local.NumErrorRecordsCommon.Count;

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
          (local.ExitStateWorkArea.Message) + " CSE Person # " + local
          .Error.Number;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.Error.Number = "";
        ExitState = "ACO_NN0000_ALL_OK";
      }

      local.RestartCount.Count = local.TotalNumberProcessed.Count;
      ++local.RecordCount.Count;

      if (local.RecordCount.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ProgramCheckpointRestart.CheckpointCount =
          local.TotalNumberProcessed.Count;
        local.TotalNumberRecords.Text15 =
          NumberToString(local.TotalNumberProcessed.Count, 15);
        local.NumErrorRecordsWorkArea.Text15 =
          NumberToString(local.NumErrorRecordsCommon.Count, 15);
        local.NumOfRecordsDeleted.Text15 =
          NumberToString(local.NumRecordsDeleted.Count, 15);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.TotalNumberRecords.Text15) + local
          .NumErrorRecordsWorkArea.Text15 + local.NumOfRecordsDeleted.Text15;
        UseUpdatePgmCheckpointRestart();
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.RecordCount.Count = 0;
      }
    }

    if (AsChar(local.RecordCanBeDeleted.Flag) != 'N' && !
      IsEmpty(local.PreviousProcessed.Number))
    {
      // if the last record is eligilbe for deletion then it will be deleted 
      // here
      //  the do not delete flag has not been set and previous person number has
      // been populated
      foreach(var item in ReadInvalidSsn())
      {
        DeleteInvalidSsn();
        local.EabFileHandling.Action = "WRITE";
        local.SsnWorkArea.SsnNum9 = entities.ForDeletion.Ssn;
        local.SsnWorkArea.ConvertOption = "1";
        UseCabSsnConvertNumToText();
        local.EabReportSend.RptDetail =
          "Invalid SSN record deleted for CSE Person #: " + TrimEnd
          (local.PreviousProcessed.Number) + " Invalid SSN of  " + local
          .SsnWorkArea.SsnText9;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++local.NumRecordsDeleted.Count;
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // Now wrap things up,
      UseSiB462Close();
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.ProgramName =
        local.ProgramProcessingInfo.Name;
      local.ProgramCheckpointRestart.CheckpointCount = -1;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
        (local.ExitStateWorkArea.Message) + " CSE Person # " + local
        .Error.Number;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseSiB462Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveSsnWorkArea(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNum9 = source.SsnNum9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
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

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea(useExport.SsnWorkArea, local.SsnWorkArea);
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

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiB462Close()
  {
    var useImport = new SiB462Close.Import();
    var useExport = new SiB462Close.Export();

    useImport.TotalNumProcessed.Count = local.TotalNumberProcessed.Count;
    useImport.NumRecordsDeleted.Count = local.NumRecordsDeleted.Count;
    useImport.NumberOfErrorRecords.Count = local.NumErrorRecordsCommon.Count;

    Call(SiB462Close.Execute, useImport, useExport);
  }

  private void UseSiB462Housekeeping()
  {
    var useImport = new SiB462Housekeeping.Import();
    var useExport = new SiB462Housekeeping.Export();

    Call(SiB462Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void DeleteInvalidSsn()
  {
    Update("DeleteInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ForDeletion.CspNumber);
        db.SetInt32(command, "ssn", entities.ForDeletion.Ssn);
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInvalidSsn()
  {
    entities.ForDeletion.Populated = false;

    return ReadEach("ReadInvalidSsn",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.PreviousProcessed.Number);
      },
      (db, reader) =>
      {
        entities.ForDeletion.CspNumber = db.GetString(reader, 0);
        entities.ForDeletion.Ssn = db.GetInt32(reader, 1);
        entities.ForDeletion.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInvalidSsnCsePerson()
  {
    entities.InvalidSsn.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadInvalidSsnCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "nextCheckDate", local.CompareDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.NextCheckDate = db.GetNullableDate(reader, 2);
        entities.CsePerson.Type1 = db.GetString(reader, 3);
        entities.InvalidSsn.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private void UpdateInvalidSsn()
  {
    System.Diagnostics.Debug.Assert(entities.InvalidSsn.Populated);

    var nextCheckDate = local.NextStatusDate.Date;

    entities.InvalidSsn.Populated = false;
    Update("UpdateInvalidSsn",
      (db, command) =>
      {
        db.SetNullableDate(command, "nextCheckDate", nextCheckDate);
        db.SetString(command, "cspNumber", entities.InvalidSsn.CspNumber);
        db.SetInt32(command, "ssn", entities.InvalidSsn.Ssn);
      });

    entities.InvalidSsn.NextCheckDate = nextCheckDate;
    entities.InvalidSsn.Populated = true;
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
    /// A value of NextStatusDate.
    /// </summary>
    [JsonPropertyName("nextStatusDate")]
    public DateWorkArea NextStatusDate
    {
      get => nextStatusDate ??= new();
      set => nextStatusDate = value;
    }

    /// <summary>
    /// A value of LoopCount.
    /// </summary>
    [JsonPropertyName("loopCount")]
    public Common LoopCount
    {
      get => loopCount ??= new();
      set => loopCount = value;
    }

    /// <summary>
    /// A value of NumOfMonthsCommon.
    /// </summary>
    [JsonPropertyName("numOfMonthsCommon")]
    public Common NumOfMonthsCommon
    {
      get => numOfMonthsCommon ??= new();
      set => numOfMonthsCommon = value;
    }

    /// <summary>
    /// A value of NumRecordsDeleted.
    /// </summary>
    [JsonPropertyName("numRecordsDeleted")]
    public Common NumRecordsDeleted
    {
      get => numRecordsDeleted ??= new();
      set => numRecordsDeleted = value;
    }

    /// <summary>
    /// A value of NumOfRecordsDeleted.
    /// </summary>
    [JsonPropertyName("numOfRecordsDeleted")]
    public WorkArea NumOfRecordsDeleted
    {
      get => numOfRecordsDeleted ??= new();
      set => numOfRecordsDeleted = value;
    }

    /// <summary>
    /// A value of NumOfMonthsWorkArea.
    /// </summary>
    [JsonPropertyName("numOfMonthsWorkArea")]
    public WorkArea NumOfMonthsWorkArea
    {
      get => numOfMonthsWorkArea ??= new();
      set => numOfMonthsWorkArea = value;
    }

    /// <summary>
    /// A value of RecordCanBeDeleted.
    /// </summary>
    [JsonPropertyName("recordCanBeDeleted")]
    public Common RecordCanBeDeleted
    {
      get => recordCanBeDeleted ??= new();
      set => recordCanBeDeleted = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Common Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of DateFound.
    /// </summary>
    [JsonPropertyName("dateFound")]
    public Common DateFound
    {
      get => dateFound ??= new();
      set => dateFound = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of CompareDate.
    /// </summary>
    [JsonPropertyName("compareDate")]
    public DateWorkArea CompareDate
    {
      get => compareDate ??= new();
      set => compareDate = value;
    }

    /// <summary>
    /// A value of NumberOfCtOrders.
    /// </summary>
    [JsonPropertyName("numberOfCtOrders")]
    public Common NumberOfCtOrders
    {
      get => numberOfCtOrders ??= new();
      set => numberOfCtOrders = value;
    }

    /// <summary>
    /// A value of TotalNumberProcessed.
    /// </summary>
    [JsonPropertyName("totalNumberProcessed")]
    public Common TotalNumberProcessed
    {
      get => totalNumberProcessed ??= new();
      set => totalNumberProcessed = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of PreviousProcessed.
    /// </summary>
    [JsonPropertyName("previousProcessed")]
    public CsePerson PreviousProcessed
    {
      get => previousProcessed ??= new();
      set => previousProcessed = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    /// <summary>
    /// A value of StartBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("startBatchTimestampWorkArea")]
    public BatchTimestampWorkArea StartBatchTimestampWorkArea
    {
      get => startBatchTimestampWorkArea ??= new();
      set => startBatchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    public Common RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
    }

    /// <summary>
    /// A value of NumErrorRecordsWorkArea.
    /// </summary>
    [JsonPropertyName("numErrorRecordsWorkArea")]
    public WorkArea NumErrorRecordsWorkArea
    {
      get => numErrorRecordsWorkArea ??= new();
      set => numErrorRecordsWorkArea = value;
    }

    /// <summary>
    /// A value of TotalNumberRecords.
    /// </summary>
    [JsonPropertyName("totalNumberRecords")]
    public WorkArea TotalNumberRecords
    {
      get => totalNumberRecords ??= new();
      set => totalNumberRecords = value;
    }

    /// <summary>
    /// A value of NumErrorRecordsCommon.
    /// </summary>
    [JsonPropertyName("numErrorRecordsCommon")]
    public Common NumErrorRecordsCommon
    {
      get => numErrorRecordsCommon ??= new();
      set => numErrorRecordsCommon = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public CsePerson Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of RestartCount.
    /// </summary>
    [JsonPropertyName("restartCount")]
    public Common RestartCount
    {
      get => restartCount ??= new();
      set => restartCount = value;
    }

    /// <summary>
    /// A value of StartCommon.
    /// </summary>
    [JsonPropertyName("startCommon")]
    public Common StartCommon
    {
      get => startCommon ??= new();
      set => startCommon = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    private DateWorkArea nextStatusDate;
    private Common loopCount;
    private Common numOfMonthsCommon;
    private Common numRecordsDeleted;
    private WorkArea numOfRecordsDeleted;
    private WorkArea numOfMonthsWorkArea;
    private Common recordCanBeDeleted;
    private Common restart;
    private DateWorkArea nullDate;
    private Common dateFound;
    private TextWorkArea date;
    private DateWorkArea compareDate;
    private Common numberOfCtOrders;
    private Common totalNumberProcessed;
    private DateWorkArea startDate;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private CsePerson previousProcessed;
    private DateWorkArea zeroDate;
    private BatchTimestampWorkArea startBatchTimestampWorkArea;
    private Common recordCount;
    private WorkArea numErrorRecordsWorkArea;
    private WorkArea totalNumberRecords;
    private Common numErrorRecordsCommon;
    private CsePerson error;
    private Common restartCount;
    private Common startCommon;
    private Common current;
    private Common currentPosition;
    private TextWorkArea postion;
    private Common fieldNumber;
    private WorkArea workArea;
    private SsnWorkArea ssnWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ForDeletion.
    /// </summary>
    [JsonPropertyName("forDeletion")]
    public InvalidSsn ForDeletion
    {
      get => forDeletion ??= new();
      set => forDeletion = value;
    }

    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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

    private InvalidSsn forDeletion;
    private InvalidSsn invalidSsn;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}
