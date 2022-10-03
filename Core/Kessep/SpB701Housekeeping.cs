// Program: SP_B701_HOUSEKEEPING, ID: 372446811, model: 746.
// Short name: SWE02304
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B701_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB701Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B701_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB701Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB701Housekeeping.
  /// </summary>
  public SpB701Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------
    //  Date		Developer	Request #      Description
    // -------------------------------------------------------------------------------------------------------------------
    // 07/17/2020	Raj	        CQ66150         Modified to retrive and set the 
    // below listed values from Program
    //                                                 
    // Processing Info table.
    //                                                 
    // 1. Notice Days (Notice generateion in how many
    // days in advance)
    //                                                 
    // 2. Last Processing Date, this will updated with
    // current batch
    //                                                    
    // Process Date for next run in order include
    // Emancipation date
    //                                                    
    // falls during holidays and weekends.
    // -------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // --------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB701";
    export.Current.Timestamp = Now();
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    // mjr---> If ppi process_date is null, set it to current date
    if (!Lt(local.Null1.Date, export.ProgramProcessingInfo.ProcessDate))
    {
      export.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    // --------------------------------------------------------------------
    // SET RUNTIME PARAMETERS TO DEFAULTS
    // --------------------------------------------------------------------
    export.DebugOn.Flag = "N";
    export.RunType.Flag = "F";
    local.Start.Count = 2;
    local.Length.Count = 1;

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      export.ProgramProcessingInfo.ParameterList =
        Spaces(ProgramProcessingInfo.ParameterList_MaxLength);

      // ----------------------------------------------------------------------------------------------------------------
      // Extract notice days from program processing info parameter field.  
      // Value starts from 13th position and 3 digits
      // ----------------------------------------------------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "NOTICE DAYS:");

      if (local.Position.Count > 0)
      {
        local.WorkArea.Text15 = "Notice Days>0.";
        local.WorkArea.Text3 =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 12, 3);

        if (Lt("000", local.WorkArea.Text3))
        {
          local.NoOfDays.Count = (int)StringToNumber(local.WorkArea.Text3);
        }
        else
        {
          local.NoOfDays.Count = 60;
          local.WorkArea.Text3 = "060";
        }
      }
      else
      {
        local.WorkArea.Text15 = "Notice Days=0.";
        local.NoOfDays.Count = 60;
      }

      // ----------------------------------------------------------------------------------------------------------------
      // Extract Last Processing Date(YYYYMMDD) from program processing info 
      // parameter field.  Value starts from 16th
      // position & 10 characters. This value was updated by previous batch run 
      // jobs upon successful completion, this
      // value is required to cover if the children emancipation date falls 
      // during weekends, holidays, job holding etc.
      // ----------------------------------------------------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "LAST PROCESSING DATE:");
        

      if (local.Position.Count > 0)
      {
        local.WorkArea.Text20 = "Last Date > SPACES";
        local.WorkArea.Text8 =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 21, 8);

        if (Lt("00000000", local.WorkArea.Text8))
        {
          local.WorkArea.Text10 =
            Substring(local.WorkArea.Text8, WorkArea.Text8_MaxLength, 1, 4) + "-"
            + Substring
            (local.WorkArea.Text8, WorkArea.Text8_MaxLength, 5, 2) + "-" + Substring
            (local.WorkArea.Text8, WorkArea.Text8_MaxLength, 7, 2);
          local.LastProcessingDate.Date = StringToDate(local.WorkArea.Text10);
          local.LastProcessingDate.Date =
            AddDays(local.LastProcessingDate.Date, 1);
        }
        else
        {
          local.LastProcessingDate.Date =
            local.ProgramProcessingInfo.ProcessDate;
          local.WorkArea.Text8 =
            NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate),
            8, 8);
        }
      }
      else
      {
        local.WorkArea.Text20 = "Last Date = SPACES";
        local.LastProcessingDate.Date = local.ProgramProcessingInfo.ProcessDate;
        local.WorkArea.Text8 =
          NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8,
          8);
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

      if (local.Position.Count > 0)
      {
        export.DebugOn.Flag = "Y";
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "RUNTYPE:");

      if (local.Position.Count > 0)
      {
        export.RunType.Flag =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 8, 1);
      }
    }
    else
    {
      local.WorkArea.Text15 = "Notice Days=0.";
      local.NoOfDays.Count = 60;
      local.WorkArea.Text20 = "Last Date = SPACES";
      local.LastProcessingDate.Date = local.ProgramProcessingInfo.ProcessDate;
      local.WorkArea.Text8 =
        NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8, 8);
        
      local.WorkArea.Text3 = "060";
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

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // --------------------------------------------------------------------
      // EXTRACT RESTART PARAMETERS FROM RESTART_INFO
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(export.ProgramCheckpointRestart.RestartInfo, "CASE:");

      if (local.Position.Count > 0)
      {
        export.RestartCase.Number =
          Substring(export.ProgramCheckpointRestart.RestartInfo,
          local.Position.Count + 5, 10);
      }

      local.Position.Count =
        Find(export.ProgramCheckpointRestart.RestartInfo, "CHILD:");

      if (local.Position.Count > 0)
      {
        export.RestartCsePerson.Number =
          Substring(export.ProgramCheckpointRestart.RestartInfo,
          local.Position.Count + 6, 10);
      }

      local.Position.Count =
        Find(export.ProgramCheckpointRestart.RestartInfo, "ROLE:");

      if (local.Position.Count > 0)
      {
        export.RestartCaseRole.Identifier =
          (int)StringToNumber(Substring(
            export.ProgramCheckpointRestart.RestartInfo, 250,
          local.Position.Count + 5, 3));
      }
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

    if (AsChar(export.RunType.Flag) == 'D')
    {
      local.EabReportSend.RptDetail = "     RUN TYPE:  DOCUMENT TRIGGERS ONLY";
    }
    else if (AsChar(export.RunType.Flag) == 'U')
    {
      local.EabReportSend.RptDetail = "     RUN TYPE:  UPDATE CHILDREN ONLY";
    }
    else
    {
      local.EabReportSend.RptDetail = "     RUN TYPE:  FULL";
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

    local.EabReportSend.RptDetail = "     CHILDREN EMANCIPATING...";
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

    local.EabReportSend.RptDetail = "      NO OF DAYS IN THE FUTURE:  " + local
      .WorkArea.Text3;
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

    local.EabReportSend.RptDetail = "EMANCIPATION LETTER START DATE:  " + NumberToString
      (DateToInt(AddDays(local.LastProcessingDate.Date, local.NoOfDays.Count)),
      8, 8);
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

    local.EabReportSend.RptDetail = "  EMANCIPATION LETTER END DATE:  " + NumberToString
      (DateToInt(
        AddDays(local.ProgramProcessingInfo.ProcessDate, local.NoOfDays.Count)),
      8, 8);
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

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
    export.StartMonth.Date =
      AddDays(local.LastProcessingDate.Date, local.NoOfDays.Count);
    export.EndMonth.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate, local.NoOfDays.Count);
    local.WorkArea.Text8 =
      NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8, 8);
    export.ProgramProcessingInfo.ParameterList = "NOTICE DAYS:" + local
      .WorkArea.Text3 + " LAST PROCESSING DATE:" + local.WorkArea.Text8;

    if (AsChar(export.RunType.Flag) != 'F')
    {
      export.ProgramProcessingInfo.ParameterList =
        (export.ProgramProcessingInfo.ParameterList ?? "") + " RUNTYPE:" + export
        .RunType.Flag;
    }

    if (AsChar(export.DebugOn.Flag) != 'N')
    {
      export.ProgramProcessingInfo.ParameterList =
        (export.ProgramProcessingInfo.ParameterList ?? "") + " DEBUG";
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
    /// A value of RunType.
    /// </summary>
    [JsonPropertyName("runType")]
    public Common RunType
    {
      get => runType ??= new();
      set => runType = value;
    }

    /// <summary>
    /// A value of RestartCaseRole.
    /// </summary>
    [JsonPropertyName("restartCaseRole")]
    public CaseRole RestartCaseRole
    {
      get => restartCaseRole ??= new();
      set => restartCaseRole = value;
    }

    /// <summary>
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of RestartCase.
    /// </summary>
    [JsonPropertyName("restartCase")]
    public Case1 RestartCase
    {
      get => restartCase ??= new();
      set => restartCase = value;
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
    /// A value of StartMonth.
    /// </summary>
    [JsonPropertyName("startMonth")]
    public DateWorkArea StartMonth
    {
      get => startMonth ??= new();
      set => startMonth = value;
    }

    /// <summary>
    /// A value of EndMonth.
    /// </summary>
    [JsonPropertyName("endMonth")]
    public DateWorkArea EndMonth
    {
      get => endMonth ??= new();
      set => endMonth = value;
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

    private Common runType;
    private CaseRole restartCaseRole;
    private CsePerson restartCsePerson;
    private Case1 restartCase;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea startMonth;
    private DateWorkArea endMonth;
    private Common debugOn;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
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
    /// A value of NoOfDays.
    /// </summary>
    [JsonPropertyName("noOfDays")]
    public Common NoOfDays
    {
      get => noOfDays ??= new();
      set => noOfDays = value;
    }

    /// <summary>
    /// A value of LastProcessingDate.
    /// </summary>
    [JsonPropertyName("lastProcessingDate")]
    public DateWorkArea LastProcessingDate
    {
      get => lastProcessingDate ??= new();
      set => lastProcessingDate = value;
    }

    private WorkArea workArea;
    private Common length;
    private Common start;
    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private Common noOfDays;
    private DateWorkArea lastProcessingDate;
  }
#endregion
}
