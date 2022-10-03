// Program: FN_BFXN_BUDGET_QUERIES, ID: 1625329194, model: 746.
// Short name: SWEBFXNP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXN_BUDGET_QUERIES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxnBudgetQueries: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXN_BUDGET_QUERIES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxnBudgetQueries(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxnBudgetQueries.
  /// </summary>
  public FnBfxnBudgetQueries(IContext context, Import import, Export export):
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
    // 04/09/19  GVandy	CQ65619		Initial Development.
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
      // ---------------------------------------------------------
      // 	001-010   Last Person Number Processed
      //         010-010   blank
      //         011-014   Year being processed
      // 	
      // -------------------------------------------------------------------------------------
      local.Restart.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.FirstSfy.Year =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 12, 4));

      // -------------------------------------------------------------------------------------
      // --  Log restart info to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Restarting after person number " + local.Restart.Number;

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "Restarting in SFY beginning in " + NumberToString
              (local.FirstSfy.Year, 12, 4);

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
            "(01) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // --Load the last saved partial counts for the year
      foreach(var item in ReadOcse157Data1())
      {
        local.Local1.Index =
          (int)(StringToNumber(entities.Ocse157Data.LineNumber) - 1);
        local.Local1.CheckSize();

        local.Local1.Update.G.TotalCurrency =
          entities.Ocse157Data.Number.GetValueOrDefault();
        local.EabReportSend.RptDetail = "Re-loaded OB type " + entities
          .Ocse157Data.LineNumber + " value " + NumberToString
          (entities.Ocse157Data.Number.GetValueOrDefault(), 15);
        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
      {
        local.EabReportSend.RptDetail = "";
        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
    else if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 4)))
      
    {
      local.FirstSfy.Year = 2007;
    }
    else
    {
      local.FirstSfy.Year =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 4));
    }

    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 6, 4)))
    {
      local.LastSfy.Year = 2018;
    }
    else
    {
      local.LastSfy.Year =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 6, 4));
    }

    // -------------------------------------------------------------------------------------
    // --  Log timeframe info to the Control Report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      if (local.Common.Count == 1)
      {
        local.EabReportSend.RptDetail = "Processing SFY from 07-01-" + NumberToString
          (local.FirstSfy.Year, 12, 4) + " to 07-01-" + NumberToString
          (local.LastSfy.Year, 12, 4);
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
          "(01) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
    }
    else
    {
      // -------------------------------------------------------------------------------------
      // --  Delete any data from previous runs.
      // -------------------------------------------------------------------------------------
      local.Common.Count = local.FirstSfy.Year;

      for(var limit = local.LastSfy.Year; local.Common.Count <= limit; ++
        local.Common.Count)
      {
        foreach(var item in ReadOcse157Data2())
        {
          DeleteOcse157Data();
        }
      }
    }

    local.SfyStart.Year = local.FirstSfy.Year;

    for(var limit = local.LastSfy.Year; local.SfyStart.Year <= limit; ++
      local.SfyStart.Year)
    {
      local.SfyStart.Date =
        StringToDate("07/01/" + NumberToString(local.SfyStart.Year, 12, 4));
      local.SfyEnd.Date = AddDays(AddYears(local.SfyStart.Date, 1), -1);
      local.SfyEnd.Year = Year(local.SfyEnd.Date);
      UseFnBfxnCalculateSfyTotals();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      // -------------------------------------------------------------------------------------
      // --  Log totals to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail = "Completed SFY Starting 07-01-" + NumberToString
            (local.SfyStart.Year, 12, 4);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " and ending 06-30-" + NumberToString
            (local.SfyEnd.Year, 12, 4);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
            (Time(Now()).Hours, 14, 2) + ":" + NumberToString
            (Time(Now()).Minutes, 14, 2);
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
            "(01) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // --Initialize group for next SFY
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        local.Local1.Update.G.TotalCurrency = 0;
      }

      local.Local1.CheckIndex();
      local.Restart.Number = "";
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
    target.Year = source.Year;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExport1ToLocal1(FnBfxnCalculateSfyTotals.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.G.TotalCurrency = source.G.TotalCurrency;
  }

  private static void MoveLocal1ToExport1(Local.LocalGroup source,
    FnBfxnCalculateSfyTotals.Export.ExportGroup target)
  {
    target.G.TotalCurrency = source.G.TotalCurrency;
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

  private void UseFnBfxnCalculateSfyTotals()
  {
    var useImport = new FnBfxnCalculateSfyTotals.Import();
    var useExport = new FnBfxnCalculateSfyTotals.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);
    useImport.Restart.Number = local.Restart.Number;
    MoveDateWorkArea(local.SfyEnd, useImport.SfyEnd);
    MoveDateWorkArea(local.SfyStart, useImport.SfyStart);
    local.Local1.CopyTo(useExport.Export1, MoveLocal1ToExport1);

    Call(FnBfxnCalculateSfyTotals.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
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

  private void DeleteOcse157Data()
  {
    Update("DeleteOcse157Data",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse157Data.CreatedTimestamp.GetValueOrDefault());
      });
  }

  private IEnumerable<bool> ReadOcse157Data1()
  {
    entities.Ocse157Data.Populated = false;

    return ReadEach("ReadOcse157Data1",
      (db, command) =>
      {
        db.SetInt32(command, "year", local.FirstSfy.Year);
      },
      (db, reader) =>
      {
        entities.Ocse157Data.FiscalYear = db.GetNullableInt32(reader, 0);
        entities.Ocse157Data.RunNumber = db.GetNullableInt32(reader, 1);
        entities.Ocse157Data.LineNumber = db.GetNullableString(reader, 2);
        entities.Ocse157Data.Column = db.GetNullableString(reader, 3);
        entities.Ocse157Data.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Ocse157Data.Number = db.GetNullableInt64(reader, 5);
        entities.Ocse157Data.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOcse157Data2()
  {
    entities.Ocse157Data.Populated = false;

    return ReadEach("ReadOcse157Data2",
      (db, command) =>
      {
        db.SetInt32(command, "count", local.Common.Count);
      },
      (db, reader) =>
      {
        entities.Ocse157Data.FiscalYear = db.GetNullableInt32(reader, 0);
        entities.Ocse157Data.RunNumber = db.GetNullableInt32(reader, 1);
        entities.Ocse157Data.LineNumber = db.GetNullableString(reader, 2);
        entities.Ocse157Data.Column = db.GetNullableString(reader, 3);
        entities.Ocse157Data.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.Ocse157Data.Number = db.GetNullableInt64(reader, 5);
        entities.Ocse157Data.Populated = true;

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
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Common G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 23;

      private Common g;
    }

    /// <summary>
    /// A value of SfyStart.
    /// </summary>
    [JsonPropertyName("sfyStart")]
    public DateWorkArea SfyStart
    {
      get => sfyStart ??= new();
      set => sfyStart = value;
    }

    /// <summary>
    /// A value of SfyEnd.
    /// </summary>
    [JsonPropertyName("sfyEnd")]
    public DateWorkArea SfyEnd
    {
      get => sfyEnd ??= new();
      set => sfyEnd = value;
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
    /// A value of LastSfy.
    /// </summary>
    [JsonPropertyName("lastSfy")]
    public DateWorkArea LastSfy
    {
      get => lastSfy ??= new();
      set => lastSfy = value;
    }

    /// <summary>
    /// A value of FirstSfy.
    /// </summary>
    [JsonPropertyName("firstSfy")]
    public DateWorkArea FirstSfy
    {
      get => firstSfy ??= new();
      set => firstSfy = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    private DateWorkArea sfyStart;
    private DateWorkArea sfyEnd;
    private Array<LocalGroup> local1;
    private DateWorkArea lastSfy;
    private DateWorkArea firstSfy;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private CsePerson restart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ocse157Data.
    /// </summary>
    [JsonPropertyName("ocse157Data")]
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
    }

    private Ocse157Data ocse157Data;
  }
#endregion
}
