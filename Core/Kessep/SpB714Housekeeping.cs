// Program: SP_B714_HOUSEKEEPING, ID: 373370929, model: 746.
// Short name: SWE02761
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B714_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB714Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B714_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB714Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB714Housekeeping.
  /// </summary>
  public SpB714Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // --------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB714";
    export.Current.Timestamp = Now();
    local.Max.IefTimestamp =
      AddMicroseconds(new DateTime(2099, 12, 31, 23, 59, 59), 999999);
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      export.ProgramProcessingInfo);

    // mjr---> If ppi process_date is null, set it to current date
    if (!Lt(local.Null1.Date, export.ProgramProcessingInfo.ProcessDate))
    {
      export.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // SET RUNTIME PARAMETERS TO DEFAULTS
      // --------------------------------------------------------------------
      export.DebugOn.Flag = "N";
      local.BatchTimestampWorkArea.IefTimestamp = local.Max.IefTimestamp;
      local.BatchTimestampWorkArea.TestTimeHh = "hh";
      local.BatchTimestampWorkArea.TextTimeMm = "mm";
    }
    else
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

      if (local.Position.Count <= 0)
      {
        export.DebugOn.Flag = "N";
      }
      else
      {
        export.DebugOn.Flag =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 6, 1);
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "TIME:");

      if (local.Position.Count <= 0)
      {
        local.BatchTimestampWorkArea.IefTimestamp = local.Max.IefTimestamp;
        local.BatchTimestampWorkArea.TestTimeHh = "hh";
        local.BatchTimestampWorkArea.TextTimeMm = "mm";
      }
      else
      {
        local.BatchTimestampWorkArea.TestTimeHh =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 5, 2);
        local.BatchTimestampWorkArea.TextTimeMm =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 8, 2);

        if (!Lt(local.BatchTimestampWorkArea.TestTimeHh, "00") && !
          Lt(local.BatchTimestampWorkArea.TextTimeMm, "00") && !
          Lt("99", local.BatchTimestampWorkArea.TestTimeHh) && !
          Lt("99", local.BatchTimestampWorkArea.TextTimeMm))
        {
        }
        else
        {
          local.BatchTimestampWorkArea.IefTimestamp = local.Max.IefTimestamp;
          local.BatchTimestampWorkArea.TestTimeHh = "hh";
          local.BatchTimestampWorkArea.TextTimeMm = "mm";
        }
      }
    }

    // --------------------------------------------------------------------
    // DETERMINE IF THIS IS A RESTART SITUATION
    // --------------------------------------------------------------------
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --------------------------------------------------------------------------------
    // SET START AND STOP TIME FOR PROCESSING DOCUMENTS
    // ---------------------------------------------------------------------------------
    if (Lt(export.ProgramProcessingInfo.ProcessDate,
      Date(export.ProgramCheckpointRestart.LastCheckpointTimestamp)) && AsChar
      (export.DebugOn.Flag) == 'P')
    {
      // P in the debug flag means we are processing for a previous day.  The 
      // debug flag must be set to P in order to process a previous timeframe.
      export.Parm.LastUpdatdTstamp =
        Add(local.Null1.Timestamp,
        Year(export.ProgramProcessingInfo.ProcessDate),
        Month(export.ProgramProcessingInfo.ProcessDate),
        Day(export.ProgramProcessingInfo.ProcessDate));

      if (!Equal(local.BatchTimestampWorkArea.IefTimestamp,
        local.Max.IefTimestamp))
      {
        export.TimeLimit.IefTimestamp =
          AddMinutes(AddHours(
            export.Parm.LastUpdatdTstamp,
          (int)StringToNumber(local.BatchTimestampWorkArea.TestTimeHh)),
          (int)StringToNumber(local.BatchTimestampWorkArea.TextTimeMm));
      }
      else
      {
        export.TimeLimit.IefTimestamp =
          local.BatchTimestampWorkArea.IefTimestamp;
      }
    }
    else
    {
      export.Parm.LastUpdatdTstamp =
        export.ProgramCheckpointRestart.LastCheckpointTimestamp;
      export.TimeLimit.IefTimestamp = local.Max.IefTimestamp;
    }

    // -------------------------------------------------------
    // OPEN OUTPUT ERROR REPORT 99
    // -------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ------------------------------------------------------------
    // OPEN OUTPUT CONTROL REPORT 98
    // ------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ------------------------------------------------------------
    // OPEN DOCUMENTS OUTPUT FILES
    // ------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseSpEabWriteDocument();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO ERROR REPORT 99
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO CONTROL REPORT 98
    // -----------------------------------------------------------
    local.EabReportSend.RptDetail = "TIME PARAMETER = " + local
      .BatchTimestampWorkArea.TestTimeHh + ":" + local
      .BatchTimestampWorkArea.TextTimeMm;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.BatchTimestampWorkArea.IefTimestamp = export.Parm.LastUpdatdTstamp;
    local.BatchTimestampWorkArea.TextTimestamp = "";
    UseLeCabConvertTimestamp();
    local.EabReportSend.RptDetail = "START TIMESTAMP = " + local
      .BatchTimestampWorkArea.TextTimestamp;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.BatchTimestampWorkArea.IefTimestamp = export.TimeLimit.IefTimestamp;
    local.BatchTimestampWorkArea.TextTimestamp = "";
    UseLeCabConvertTimestamp();
    local.EabReportSend.RptDetail = "STOP TIMESTAMP = " + local
      .BatchTimestampWorkArea.TextTimestamp;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (export.Exceptions.Index < 0)
    {
      local.EabReportSend.RptDetail = "No exception documents";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else
    {
      local.EabReportSend.RptDetail = "Document Exceptions";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "-------------------";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      export.Exceptions.Index = 0;

      for(var limit = export.Exceptions.Count; export.Exceptions.Index < limit; ++
        export.Exceptions.Index)
      {
        if (!export.Exceptions.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Exceptions.Item.G.Name))
        {
          local.EabReportSend.RptDetail = export.Exceptions.Item.G.Name;
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }

      export.Exceptions.CheckIndex();
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ------------------------------------------------------------
    // Set Literals
    // ------------------------------------------------------------
    export.Max.IefTimestamp = local.Max.IefTimestamp;
    export.Parm.PrintSucessfulIndicator = "Y";
    export.Parm.LastUpdatedBy = "SWEPB709";
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
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
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    local.BatchTimestampWorkArea.Assign(useExport.BatchTimestampWorkArea);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSpEabWriteDocument()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// <summary>A ExceptionsGroup group.</summary>
    [Serializable]
    public class ExceptionsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Document G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document g;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public BatchTimestampWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of TimeLimit.
    /// </summary>
    [JsonPropertyName("timeLimit")]
    public BatchTimestampWorkArea TimeLimit
    {
      get => timeLimit ??= new();
      set => timeLimit = value;
    }

    /// <summary>
    /// A value of Parm.
    /// </summary>
    [JsonPropertyName("parm")]
    public OutgoingDocument Parm
    {
      get => parm ??= new();
      set => parm = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// Gets a value of Exceptions.
    /// </summary>
    [JsonIgnore]
    public Array<ExceptionsGroup> Exceptions => exceptions ??= new(
      ExceptionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Exceptions for json serialization.
    /// </summary>
    [JsonPropertyName("exceptions")]
    [Computed]
    public IList<ExceptionsGroup> Exceptions_Json
    {
      get => exceptions;
      set => Exceptions.Assign(value);
    }

    private BatchTimestampWorkArea max;
    private BatchTimestampWorkArea timeLimit;
    private OutgoingDocument parm;
    private Common debugOn;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Array<ExceptionsGroup> exceptions;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public BatchTimestampWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of DividerPosition.
    /// </summary>
    [JsonPropertyName("dividerPosition")]
    public Common DividerPosition
    {
      get => dividerPosition ??= new();
      set => dividerPosition = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private BatchTimestampWorkArea max;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common dividerPosition;
    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    private Alert alert;
    private Document document;
  }
#endregion
}
