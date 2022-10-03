// Program: SI_B292_HOUSEKEEPING, ID: 373440606, model: 746.
// Short name: SWE02746
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B292_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SiB292Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B292_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB292Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB292Housekeeping.
  /// </summary>
  public SiB292Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------
    // GET PROCESS DATE AND OPTIONAL PARAMETERS
    // ----------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEIB292";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      export.ProgramProcessingInfo);

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // ----------------------------------------------------------------------
      // SET RUNTIME PARAMETERS TO DEFAULTS
      // ----------------------------------------------------------------------
      export.DebugOn.Flag = "N";
      local.CutoffDaysCommon.Count = 180;
      local.CutoffDaysWorkArea.Text3 = "180";

      export.ErrorCodes.Index = 0;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.G.ErrorCode = "IREQ";
    }
    else
    {
      // ----------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // ----------------------------------------------------------------------
      // ----------------------------------------------------------------------
      // Set Debug parameter
      // ----------------------------------------------------------------------
      if (Find(local.ProgramProcessingInfo.ParameterList, "DEBUG") > 0)
      {
        export.DebugOn.Flag = "Y";
      }
      else
      {
        export.DebugOn.Flag = "N";
      }

      // ----------------------------------------------------------------------
      // Set Cutoff Days parameter
      // ----------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "CUTOFF_DAYS:");

      if (local.Position.Count > 0)
      {
        local.CutoffDaysWorkArea.Text3 =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 12, 3);

        if (!Lt(local.CutoffDaysWorkArea.Text3, "000") && !
          Lt("999", local.CutoffDaysWorkArea.Text3))
        {
          local.CutoffDaysCommon.Count =
            (int)StringToNumber(local.CutoffDaysWorkArea.Text3);
        }
        else
        {
          local.CutoffDaysCommon.Count = 180;
        }
      }
      else
      {
        local.CutoffDaysCommon.Count = 180;
        local.CutoffDaysWorkArea.Text3 = "180";
      }

      // ----------------------------------------------------------------------
      // Set Error Codes parameters
      // ----------------------------------------------------------------------
      export.ErrorCodes.Index = 0;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.G.ErrorCode = "IREQ";
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "ERROR_CODES:");

      if (local.Position.Count > 0)
      {
        local.Common.Count = local.Position.Count + 12;

        for(var limit =
          Length(TrimEnd(local.ProgramProcessingInfo.ParameterList)); local
          .Common.Count <= limit; local.Common.Count += 5)
        {
          local.ErrorCode.ErrorCode =
            Substring(local.ProgramProcessingInfo.ParameterList,
            local.Common.Count, 4);

          // ----------------------------------------------------------------
          // Eliminate duplicates in the group
          // ----------------------------------------------------------------
          for(export.ErrorCodes.Index = 0; export.ErrorCodes.Index < export
            .ErrorCodes.Count; ++export.ErrorCodes.Index)
          {
            if (!export.ErrorCodes.CheckSize())
            {
              break;
            }

            if (Equal(local.ErrorCode.ErrorCode,
              export.ErrorCodes.Item.G.ErrorCode))
            {
              goto Next;
            }
          }

          export.ErrorCodes.CheckIndex();

          // ----------------------------------------------------------------
          // Error code should be in the format Ennn.
          // Currently we only verify no other characters are in the code
          // (IREQ is eliminated in the duplicate check)
          // ----------------------------------------------------------------
          if (Verify(local.ErrorCode.ErrorCode, "E0123456789") > 0)
          {
            continue;
          }

          // ----------------------------------------------------------------
          // No check for group view overflow since we want it to ABEND so
          // the user removes the excess codes and re-runs
          // ----------------------------------------------------------------
          export.ErrorCodes.Index = export.ErrorCodes.Count;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.G.ErrorCode = local.ErrorCode.ErrorCode ?? ""
            ;

Next:
          ;
        }
      }
    }

    // ----------------------------------------------------------------------
    // DETERMINE IF THIS IS A RESTART SITUATION
    // ----------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveProgramCheckpointRestart1(local.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // ----------------------------------------------------------------------
      // EXTRACT RESTART PARAMETERS FROM RESTART_INFO
      // ----------------------------------------------------------------------
    }

    // ----------------------------------------------------------------------
    // OPEN ERROR REPORT (99)
    // ----------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ----------------------------------------------------------------------
    // OPEN CONTROL REPORT (98)
    // ----------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ----------------------------------------------------------------------
    // WRITE INITIAL LINES TO ERROR REPORT
    // ----------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "Transaction                   Error";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------------------------------------------------------------------
    // WRITE INITIAL LINES TO CONTROL REPORT
    // ----------------------------------------------------------------------
    local.EabReportSend.RptDetail = "DEBUG: " + export.DebugOn.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "CUTOFF DAYS: " + local
      .CutoffDaysWorkArea.Text3;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    export.Cutoff.Date =
      AddDays(export.ProgramProcessingInfo.ProcessDate, -
      local.CutoffDaysCommon.Count);
    local.Cutoff.TextDate = NumberToString(DateToInt(export.Cutoff.Date), 8, 8);
    local.EabReportSend.RptDetail = "CUTOFF DATE: " + local.Cutoff.TextDate;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "ERROR CODES:";

    for(export.ErrorCodes.Index = 0; export.ErrorCodes.Index < export
      .ErrorCodes.Count; ++export.ErrorCodes.Index)
    {
      if (!export.ErrorCodes.CheckSize())
      {
        break;
      }

      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + " " +
        (export.ErrorCodes.Item.G.ErrorCode ?? "");
    }

    export.ErrorCodes.CheckIndex();
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------------------------------------------------------------------
    // SET LITERALS
    // ----------------------------------------------------------------------
    export.Current.Date = export.ProgramProcessingInfo.ProcessDate;
    export.Current.Timestamp = Now();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart1(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
  }

  private static void MoveProgramCheckpointRestart2(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
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

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

    MoveProgramCheckpointRestart2(useExport.ProgramCheckpointRestart,
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
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsenetTransactionEnvelop G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsenetTransactionEnvelop g;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of Cutoff.
    /// </summary>
    [JsonPropertyName("cutoff")]
    public DateWorkArea Cutoff
    {
      get => cutoff ??= new();
      set => cutoff = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    private Common debugOn;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea cutoff;
    private Array<ErrorCodesGroup> errorCodes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ErrorCode.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public CsenetTransactionEnvelop ErrorCode
    {
      get => errorCode ??= new();
      set => errorCode = value;
    }

    /// <summary>
    /// A value of Cutoff.
    /// </summary>
    [JsonPropertyName("cutoff")]
    public DateWorkArea Cutoff
    {
      get => cutoff ??= new();
      set => cutoff = value;
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
    /// A value of CutoffDaysWorkArea.
    /// </summary>
    [JsonPropertyName("cutoffDaysWorkArea")]
    public WorkArea CutoffDaysWorkArea
    {
      get => cutoffDaysWorkArea ??= new();
      set => cutoffDaysWorkArea = value;
    }

    /// <summary>
    /// A value of CutoffDaysCommon.
    /// </summary>
    [JsonPropertyName("cutoffDaysCommon")]
    public Common CutoffDaysCommon
    {
      get => cutoffDaysCommon ??= new();
      set => cutoffDaysCommon = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    private CsenetTransactionEnvelop errorCode;
    private DateWorkArea cutoff;
    private Common common;
    private WorkArea cutoffDaysWorkArea;
    private Common cutoffDaysCommon;
    private Common position;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}
