// Program: SP_B308_HOUSEKEEPING, ID: 374476677, model: 746.
// Short name: SWE02712
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B308_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB308Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B308_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB308Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB308Housekeeping.
  /// </summary>
  public SpB308Housekeeping(IContext context, Import import, Export export):
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
    local.ProgramProcessingInfo.Name = "SWEPB308";
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

    // --------------------------------------------------------------------
    // SET RUNTIME PARAMETERS TO DEFAULTS
    // --------------------------------------------------------------------
    export.DebugOn.Flag = "N";
    local.ArchiveDaysCommon.Count = 60;

    // --------------------------------------------------------------------
    // EXTRACT RUNTIME PARAMETERS FROM PPI
    // --------------------------------------------------------------------
    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // Extract Archive days from Prarameter list.
      // If DAYS parameter is non numeric or less than 30 then default to 60.
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DAYS=");

      if (local.Position.Count > 0)
      {
        local.ArchiveDaysExternal.TextLine8 = "00000" + Substring
          (local.ProgramProcessingInfo.ParameterList, local.Position.Count +
          5, 3);

        if (Verify(local.ArchiveDaysExternal.TextLine8, "0123456789") == 0)
        {
          local.ArchiveDaysCommon.Count =
            (int)StringToNumber(local.ArchiveDaysExternal.TextLine8);

          if (local.ArchiveDaysCommon.Count < 30)
          {
            local.ArchiveDaysCommon.Count = 60;
          }
        }
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

      if (local.Position.Count > 0)
      {
        export.DebugOn.Flag = "Y";
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

    // --------------------------------------------------------------------
    // SET RESTART PARAMETERS TO DEFAULTS
    // --------------------------------------------------------------------
    export.Restart.SystemGeneratedIdentifier = 999999999;
    export.RecsWrittenToArchive.Count = 0;
    export.DocsRead.Count = 0;
    export.FieldValuesRead.Count = 0;
    export.DocsArchvdAndUpdated.Count = 0;
    export.DocsOnlyUpdated.Count = 0;
    export.FieldValuesArchivedDel.Count = 0;
    export.FieldValuesOnlyDeleted.Count = 0;

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // --------------------------------------------------------------------
      // EXTRACT RESTART PARAMETERS FROM RESTART_INFO
      // --------------------------------------------------------------------
      if (IsEmpty(export.ProgramCheckpointRestart.RestartInfo))
      {
        ExitState = "RESTART_INFO_BLANK";

        return;
      }

      export.ProgramProcessingInfo.ProcessDate =
        StringToDate(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 1, 10));
      export.Restart.SystemGeneratedIdentifier =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 11, 15));
      export.RecsWrittenToArchive.Count =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 26, 15));
      export.DocsRead.Count =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 41, 15));
      export.FieldValuesRead.Count =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 56, 15));
      export.DocsArchvdAndUpdated.Count =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 71, 15));
      export.DocsOnlyUpdated.Count =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 86, 15));
      export.FieldValuesArchivedDel.Count =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 101, 15));
      export.FieldValuesOnlyDeleted.Count =
        (int)StringToNumber(Substring(
          export.ProgramCheckpointRestart.RestartInfo, 250, 116, 15));
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
    // OPEN ARCHIVE FILE
    // ------------------------------------------------------------
    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.External.FileInstruction = "OPENRS";
    }
    else
    {
      local.External.FileInstruction = "OPENNM";
    }

    UseSpEabArchiveFieldValuesRs();

    if (!IsEmpty(local.External.TextReturnCode))
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
    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "R U N   T I M E   P A R A M E T E R S";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "Parameter List : " + (
      local.ProgramProcessingInfo.ParameterList ?? "");
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // -----------------------------------------------------------
    // Set Archive Date to Process date - archive days
    // -----------------------------------------------------------
    export.Archive.Date =
      AddDays(export.ProgramProcessingInfo.ProcessDate, -
      local.ArchiveDaysCommon.Count);
    local.Temp.TextLine80 = NumberToString(Year(export.Archive.Date), 12, 4) + "-"
      + NumberToString(Month(export.Archive.Date), 14, 2) + "-";
    local.Temp.TextLine80 =
      Substring(local.Temp.TextLine80, External.TextLine80_MaxLength, 1, 8) + NumberToString
      (Day(export.Archive.Date), 14, 2);
    local.EabReportSend.RptDetail = "Archive Date : " + Substring
      (local.Temp.TextLine80, External.TextLine80_MaxLength, 1, 10);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.Temp.TextLine80 =
      NumberToString(Year(export.ProgramProcessingInfo.ProcessDate), 12, 4) + "-"
      + NumberToString
      (Month(export.ProgramProcessingInfo.ProcessDate), 14, 2) + "-";
    local.Temp.TextLine80 =
      Substring(local.Temp.TextLine80, External.TextLine80_MaxLength, 1, 8) + NumberToString
      (Day(export.ProgramProcessingInfo.ProcessDate), 14, 2);
    local.EabReportSend.RptDetail = "Process Date : " + Substring
      (local.Temp.TextLine80, External.TextLine80_MaxLength, 1, 10);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "Archive Days : " + NumberToString
      (local.ArchiveDaysCommon.Count, 13, 3);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.DebugOn.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "     DEBUG:  ON";
    }
    else
    {
      local.EabReportSend.RptDetail = "     DEBUG:  OFF";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.EabReportSend.RptDetail = "     RESTART:  YES";
    }
    else
    {
      local.EabReportSend.RptDetail = "     RESTART:  NO";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
    // -----------------------------------------------------------
    // Format Archive Timestamp from Archive Date
    // YYYY-MM-DD-00:00:00:000000
    // -----------------------------------------------------------
    local.Temp.TextLine80 = NumberToString(Year(export.Archive.Date), 12, 4) + "-"
      + NumberToString(Month(export.Archive.Date), 14, 2) + "-";
    local.Temp.TextLine80 =
      Substring(local.Temp.TextLine80, External.TextLine80_MaxLength, 1, 8) + NumberToString
      (Day(export.Archive.Date), 14, 2);
    export.Archive.Timestamp =
      Timestamp(Substring(
        local.Temp.TextLine80, External.TextLine80_MaxLength, 1, 10));
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

  private void UseSpEabArchiveFieldValuesRs()
  {
    var useImport = new SpEabArchiveFieldValuesRs.Import();
    var useExport = new SpEabArchiveFieldValuesRs.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useImport.LocalRecCount.Count = export.RecsWrittenToArchive.Count;
    MoveExternal(local.External, useExport.External);

    Call(SpEabArchiveFieldValuesRs.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
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
    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Infrastructure Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of RecsWrittenToArchive.
    /// </summary>
    [JsonPropertyName("recsWrittenToArchive")]
    public Common RecsWrittenToArchive
    {
      get => recsWrittenToArchive ??= new();
      set => recsWrittenToArchive = value;
    }

    /// <summary>
    /// A value of DocsRead.
    /// </summary>
    [JsonPropertyName("docsRead")]
    public Common DocsRead
    {
      get => docsRead ??= new();
      set => docsRead = value;
    }

    /// <summary>
    /// A value of FieldValuesRead.
    /// </summary>
    [JsonPropertyName("fieldValuesRead")]
    public Common FieldValuesRead
    {
      get => fieldValuesRead ??= new();
      set => fieldValuesRead = value;
    }

    /// <summary>
    /// A value of DocsArchvdAndUpdated.
    /// </summary>
    [JsonPropertyName("docsArchvdAndUpdated")]
    public Common DocsArchvdAndUpdated
    {
      get => docsArchvdAndUpdated ??= new();
      set => docsArchvdAndUpdated = value;
    }

    /// <summary>
    /// A value of DocsOnlyUpdated.
    /// </summary>
    [JsonPropertyName("docsOnlyUpdated")]
    public Common DocsOnlyUpdated
    {
      get => docsOnlyUpdated ??= new();
      set => docsOnlyUpdated = value;
    }

    /// <summary>
    /// A value of FieldValuesArchivedDel.
    /// </summary>
    [JsonPropertyName("fieldValuesArchivedDel")]
    public Common FieldValuesArchivedDel
    {
      get => fieldValuesArchivedDel ??= new();
      set => fieldValuesArchivedDel = value;
    }

    /// <summary>
    /// A value of FieldValuesOnlyDeleted.
    /// </summary>
    [JsonPropertyName("fieldValuesOnlyDeleted")]
    public Common FieldValuesOnlyDeleted
    {
      get => fieldValuesOnlyDeleted ??= new();
      set => fieldValuesOnlyDeleted = value;
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
    /// A value of Archive.
    /// </summary>
    [JsonPropertyName("archive")]
    public DateWorkArea Archive
    {
      get => archive ??= new();
      set => archive = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private Infrastructure restart;
    private Common recsWrittenToArchive;
    private Common docsRead;
    private Common fieldValuesRead;
    private Common docsArchvdAndUpdated;
    private Common docsOnlyUpdated;
    private Common fieldValuesArchivedDel;
    private Common fieldValuesOnlyDeleted;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea archive;
    private Common debugOn;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ArchiveDaysCommon.
    /// </summary>
    [JsonPropertyName("archiveDaysCommon")]
    public Common ArchiveDaysCommon
    {
      get => archiveDaysCommon ??= new();
      set => archiveDaysCommon = value;
    }

    /// <summary>
    /// A value of ArchiveDaysExternal.
    /// </summary>
    [JsonPropertyName("archiveDaysExternal")]
    public External ArchiveDaysExternal
    {
      get => archiveDaysExternal ??= new();
      set => archiveDaysExternal = value;
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

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public External Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private Common archiveDaysCommon;
    private External archiveDaysExternal;
    private External external;
    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private External temp;
  }
#endregion
}
