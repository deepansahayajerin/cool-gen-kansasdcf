// Program: SP_B311_PURGE_PROCESSED_TRIGGERS, ID: 374360041, model: 746.
// Short name: SWEP311B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B311_PURGE_PROCESSED_TRIGGERS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB311PurgeProcessedTriggers: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B311_PURGE_PROCESSED_TRIGGERS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB311PurgeProcessedTriggers(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB311PurgeProcessedTriggers.
  /// </summary>
  public SpB311PurgeProcessedTriggers(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 03/09/2000	M Ramirez	WR 163		Initial Development
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB311Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.RowLock.Count = 2;

    foreach(var item in ReadCodeValue())
    {
      local.Parm.ProgramName = entities.ProcessorCodeValue.Cdvalue;
      UseReadPgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
        local.EabReportSend.RptDetail =
          "WARNING:  Program has no Checkpoint_Restart_Information; Program  = " +
          local.Parm.ProgramName;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";

        continue;
      }

      local.Trigger.CreatedTimestamp = local.Parm.LastCheckpointTimestamp;

      foreach(var item1 in ReadCodeValueCodeValueCombination2())
      {
        local.Trigger.Type1 = entities.TypeCodeValue.Cdvalue;

        foreach(var item2 in ReadCodeValueCodeValueCombination1())
        {
          if (Equal(entities.StatusCodeValue.Cdvalue, "BLANK"))
          {
            local.Trigger.Status = "";
          }
          else
          {
            local.Trigger.Status = entities.StatusCodeValue.Cdvalue;
          }

          foreach(var item3 in ReadTrigger())
          {
            ++local.RowLock.Count;
            ++local.TriggersRead.Count;
            DeleteTrigger();
            ++local.TriggersProcessed.Count;

            // mjr
            // -----------------------------------------------------
            // check for commit frequency
            // --------------------------------------------------------
            if (local.RowLock.Count >= local
              .ProgramCheckpointRestart.ReadFrequencyCount.
                GetValueOrDefault() || local.RowLock.Count >= local
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              UseExtToDoACommit();
              local.RowLock.Count = 0;
            }

            // mjr
            // -------------------------------------------------
            // End READ EACH Trigger
            // ----------------------------------------------------
          }

          // mjr
          // -------------------------------------------------
          // End READ EACH Status
          // ----------------------------------------------------
        }

        // mjr
        // -------------------------------------------------
        // End READ EACH Type
        // ----------------------------------------------------
      }

      // mjr
      // -------------------------------------------------
      // End READ EACH Processor
      // ----------------------------------------------------
    }

    // mjr
    // ---------------------------------------------------------
    // Do commit here, so errors with closing files will
    // not require the job to be re-run.
    // ------------------------------------------------------------
    UseExtToDoACommit();

    // ---------------------------------------------------------------
    // WRITE CONTROL TOTALS AND CLOSE REPORTS
    // ---------------------------------------------------------------
    UseSpB311WriteControlsAndClose();
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName = local.Parm.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    local.Parm.LastCheckpointTimestamp =
      useExport.ProgramCheckpointRestart.LastCheckpointTimestamp;
  }

  private void UseSpB311Housekeeping()
  {
    var useImport = new SpB311Housekeeping.Import();
    var useExport = new SpB311Housekeeping.Export();

    Call(SpB311Housekeeping.Execute, useImport, useExport);

    local.Type1.Id = useExport.Type1.Id;
    local.Status.Id = useExport.Status.Id;
    local.Processor.Id = useExport.Processor.Id;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
  }

  private void UseSpB311WriteControlsAndClose()
  {
    var useImport = new SpB311WriteControlsAndClose.Import();
    var useExport = new SpB311WriteControlsAndClose.Export();

    useImport.TriggersRead.Count = local.TriggersRead.Count;
    useImport.TriggersErred.Count = local.TriggersErred.Count;
    useImport.TriggersProcessed.Count = local.TriggersProcessed.Count;

    Call(SpB311WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void DeleteTrigger()
  {
    Update("DeleteTrigger",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.Trigger.Identifier);
      });
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    entities.ProcessorCodeValue.Populated = false;

    return ReadEach("ReadCodeValue",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableInt32(command, "codId", local.Processor.Id);
      },
      (db, reader) =>
      {
        entities.ProcessorCodeValue.Id = db.GetInt32(reader, 0);
        entities.ProcessorCodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.ProcessorCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ProcessorCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.ProcessorCodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.ProcessorCodeValue.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCodeValueCodeValueCombination1()
  {
    entities.StatusCodeValueCombination.Populated = false;
    entities.StatusCodeValue.Populated = false;

    return ReadEach("ReadCodeValueCodeValueCombination1",
      (db, command) =>
      {
        db.SetInt32(command, "covSId", entities.ProcessorCodeValue.Id);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableInt32(command, "codId", local.Status.Id);
      },
      (db, reader) =>
      {
        entities.StatusCodeValue.Id = db.GetInt32(reader, 0);
        entities.StatusCodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.StatusCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.StatusCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.StatusCodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.StatusCodeValueCombination.Id = db.GetInt32(reader, 5);
        entities.StatusCodeValueCombination.CovId = db.GetInt32(reader, 6);
        entities.StatusCodeValueCombination.CovSId = db.GetInt32(reader, 7);
        entities.StatusCodeValueCombination.EffectiveDate =
          db.GetDate(reader, 8);
        entities.StatusCodeValueCombination.ExpirationDate =
          db.GetDate(reader, 9);
        entities.StatusCodeValueCombination.Populated = true;
        entities.StatusCodeValue.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCodeValueCodeValueCombination2()
  {
    entities.TypeCodeValueCombination.Populated = false;
    entities.TypeCodeValue.Populated = false;

    return ReadEach("ReadCodeValueCodeValueCombination2",
      (db, command) =>
      {
        db.SetInt32(command, "covSId", entities.ProcessorCodeValue.Id);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableInt32(command, "codId", local.Type1.Id);
      },
      (db, reader) =>
      {
        entities.TypeCodeValue.Id = db.GetInt32(reader, 0);
        entities.TypeCodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.TypeCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.TypeCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.TypeCodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.TypeCodeValueCombination.Id = db.GetInt32(reader, 5);
        entities.TypeCodeValueCombination.CovId = db.GetInt32(reader, 6);
        entities.TypeCodeValueCombination.CovSId = db.GetInt32(reader, 7);
        entities.TypeCodeValueCombination.EffectiveDate = db.GetDate(reader, 8);
        entities.TypeCodeValueCombination.ExpirationDate =
          db.GetDate(reader, 9);
        entities.TypeCodeValueCombination.Populated = true;
        entities.TypeCodeValue.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTrigger()
  {
    entities.Trigger.Populated = false;

    return ReadEach("ReadTrigger",
      (db, command) =>
      {
        db.SetString(command, "type", local.Trigger.Type1);
        db.SetNullableString(command, "status", local.Trigger.Status ?? "");
        db.SetNullableDateTime(
          command, "createdTimestamp",
          local.Trigger.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Trigger.Identifier = db.GetInt32(reader, 0);
        entities.Trigger.Type1 = db.GetString(reader, 1);
        entities.Trigger.Status = db.GetNullableString(reader, 2);
        entities.Trigger.CreatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Trigger.Populated = true;

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
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    /// <summary>
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public Code Type1
    {
      get => type1 ??= new();
      set => type1 = value;
    }

    /// <summary>
    /// A value of Parm.
    /// </summary>
    [JsonPropertyName("parm")]
    public ProgramCheckpointRestart Parm
    {
      get => parm ??= new();
      set => parm = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public Code Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of Processor.
    /// </summary>
    [JsonPropertyName("processor")]
    public Code Processor
    {
      get => processor ??= new();
      set => processor = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of RowLock.
    /// </summary>
    [JsonPropertyName("rowLock")]
    public Common RowLock
    {
      get => rowLock ??= new();
      set => rowLock = value;
    }

    /// <summary>
    /// A value of TriggersRead.
    /// </summary>
    [JsonPropertyName("triggersRead")]
    public Common TriggersRead
    {
      get => triggersRead ??= new();
      set => triggersRead = value;
    }

    /// <summary>
    /// A value of TriggersErred.
    /// </summary>
    [JsonPropertyName("triggersErred")]
    public Common TriggersErred
    {
      get => triggersErred ??= new();
      set => triggersErred = value;
    }

    /// <summary>
    /// A value of TriggersProcessed.
    /// </summary>
    [JsonPropertyName("triggersProcessed")]
    public Common TriggersProcessed
    {
      get => triggersProcessed ??= new();
      set => triggersProcessed = value;
    }

    private Trigger trigger;
    private Code type1;
    private ProgramCheckpointRestart parm;
    private Code status;
    private Code processor;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common debugOn;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External external;
    private Common rowLock;
    private Common triggersRead;
    private Common triggersErred;
    private Common triggersProcessed;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of TypeCodeValueCombination.
    /// </summary>
    [JsonPropertyName("typeCodeValueCombination")]
    public CodeValueCombination TypeCodeValueCombination
    {
      get => typeCodeValueCombination ??= new();
      set => typeCodeValueCombination = value;
    }

    /// <summary>
    /// A value of TypeCode.
    /// </summary>
    [JsonPropertyName("typeCode")]
    public Code TypeCode
    {
      get => typeCode ??= new();
      set => typeCode = value;
    }

    /// <summary>
    /// A value of TypeCodeValue.
    /// </summary>
    [JsonPropertyName("typeCodeValue")]
    public CodeValue TypeCodeValue
    {
      get => typeCodeValue ??= new();
      set => typeCodeValue = value;
    }

    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    /// <summary>
    /// A value of StatusCodeValueCombination.
    /// </summary>
    [JsonPropertyName("statusCodeValueCombination")]
    public CodeValueCombination StatusCodeValueCombination
    {
      get => statusCodeValueCombination ??= new();
      set => statusCodeValueCombination = value;
    }

    /// <summary>
    /// A value of StatusCodeValue.
    /// </summary>
    [JsonPropertyName("statusCodeValue")]
    public CodeValue StatusCodeValue
    {
      get => statusCodeValue ??= new();
      set => statusCodeValue = value;
    }

    /// <summary>
    /// A value of ProcessorCodeValue.
    /// </summary>
    [JsonPropertyName("processorCodeValue")]
    public CodeValue ProcessorCodeValue
    {
      get => processorCodeValue ??= new();
      set => processorCodeValue = value;
    }

    /// <summary>
    /// A value of StatusCode.
    /// </summary>
    [JsonPropertyName("statusCode")]
    public Code StatusCode
    {
      get => statusCode ??= new();
      set => statusCode = value;
    }

    /// <summary>
    /// A value of ProcessorCode.
    /// </summary>
    [JsonPropertyName("processorCode")]
    public Code ProcessorCode
    {
      get => processorCode ??= new();
      set => processorCode = value;
    }

    private CodeValueCombination typeCodeValueCombination;
    private Code typeCode;
    private CodeValue typeCodeValue;
    private Trigger trigger;
    private CodeValueCombination statusCodeValueCombination;
    private CodeValue statusCodeValue;
    private CodeValue processorCodeValue;
    private Code statusCode;
    private Code processorCode;
  }
#endregion
}
