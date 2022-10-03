// Program: SP_B730_HOUSEKEEPING, ID: 373509480, model: 746.
// Short name: SWE02800
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B730_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB730Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B730_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB730Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB730Housekeeping.
  /// </summary>
  public SpB730Housekeeping(IContext context, Import import, Export export):
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
    local.ProgramProcessingInfo.Name = global.UserId;
    export.Current.Timestamp = Now();
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.ProgramProcessingInfo.Assign(local.ProgramProcessingInfo);

    if (!Lt(local.Null1.Date, export.ProgramProcessingInfo.ProcessDate))
    {
      export.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    // --------------------------------------------------------------------
    // SET RUNTIME PARAMETERS TO DEFAULTS
    // --------------------------------------------------------------------
    export.DebugOn.Flag = "N";

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

      if (local.Position.Count > 0)
      {
        export.DebugOn.Flag = "Y";
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DOC NAME:");

      if (local.Position.Count > 0)
      {
        export.Document.Name =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 10, 8);

        if (IsEmpty(export.Document.Name))
        {
          ExitState = "SP0000_PARM_DOC_NAME_NOT_VLD_AB";

          return;
        }

        if (!ReadDocument())
        {
          ExitState = "SP0000_PARM_DOC_NAME_NOT_VLD_AB";

          return;
        }
      }
      else
      {
        local.EabReportSend.RptDetail =
          "No Document Name Found in the Parm Information.";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "SP0000_PARM_DOC_NAME_NOT_VLD_AB";

        return;
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

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (IsEmpty(export.ProgramCheckpointRestart.RestartInfo))
      {
        ExitState = "RESTART_INFO_BLANK";

        return;
      }

      local.Position.Count =
        Find(String(
          export.ProgramCheckpointRestart.RestartInfo,
        ProgramCheckpointRestart.RestartInfo_MaxLength), "PRSN: ");

      if (local.Position.Count > 0)
      {
        export.RestartCsePerson.Number =
          Substring(export.ProgramCheckpointRestart.RestartInfo,
          local.Position.Count + 6, 10);
      }

      local.Position.Count =
        Find(String(
          export.ProgramCheckpointRestart.RestartInfo,
        ProgramCheckpointRestart.RestartInfo_MaxLength), "CASE_NO: ");

      if (local.Position.Count > 0)
      {
        export.RestartCase.Number =
          Substring(export.ProgramCheckpointRestart.RestartInfo,
          local.Position.Count + 9, 10);
      }

      local.Position.Count =
        Find(String(
          export.ProgramCheckpointRestart.RestartInfo,
        ProgramCheckpointRestart.RestartInfo_MaxLength), "CR_TYPE: ");

      if (local.Position.Count > 0)
      {
        export.RestartCaseRole.Type1 =
          Substring(export.ProgramCheckpointRestart.RestartInfo,
          local.Position.Count + 9, 2);
      }

      if (IsEmpty(export.RestartCsePerson.Number))
      {
        if (IsEmpty(export.RestartCase.Number))
        {
          if (IsEmpty(export.RestartCaseRole.Type1))
          {
            local.EabReportSend.RptDetail =
              "ERROR: Restart information does not contain a person number, case number or case role.";
              
            UseCabErrorReport1();

            if (Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO0000_INVALID_RESTART_PARM";

            return;
          }
        }
        else if (IsEmpty(export.RestartCaseRole.Type1))
        {
          local.EabReportSend.RptDetail =
            "ERROR: Restart information does not contain a person number or case role, Case Number: " +
            export.RestartCase.Number;
          UseCabErrorReport1();

          if (!IsEmpty(local.EabFileHandling.Status))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO0000_INVALID_RESTART_PARM";

          return;
        }
      }
      else if (IsEmpty(export.RestartCase.Number))
      {
        if (IsEmpty(export.RestartCaseRole.Type1))
        {
          local.EabReportSend.RptDetail =
            "ERROR: Restart information does not contain a case number or case role, Person Number: " +
            export.RestartCsePerson.Number;
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO0000_INVALID_RESTART_PARM";

          return;
        }
      }
    }

    // -------------------------------------------------------
    // OPEN OUTPUT ERROR REPORT 99
    // -------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

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
    UseCabErrorReport2();

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
      UseCabErrorReport2();

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
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.DebugOn.Flag) == 'F')
    {
      local.EabReportSend.RptDetail = "DEBUG:  FULL";
    }
    else if (AsChar(export.DebugOn.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "DEBUG:  ON";
    }
    else
    {
      local.EabReportSend.RptDetail = "DEBUG:  OFF";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "DOCUMENT NAME:  " + export.Document.Name;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.EabReportSend.RptDetail = "RESTART:  YES";
    }
    else
    {
      local.EabReportSend.RptDetail = "RESTART:  NO";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (IsEmpty(export.RestartCsePerson.Number))
      {
        local.EabReportSend.RptDetail = "RESTART CSE PERSON: N/A";
      }
      else
      {
        local.EabReportSend.RptDetail = "RESTART CSE PERSON: " + export
          .RestartCsePerson.Number;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (IsEmpty(export.RestartCase.Number))
      {
        local.EabReportSend.RptDetail = "RESTART CASE NUMBER: N/A";
      }
      else
      {
        local.EabReportSend.RptDetail = "RESTART CASE NUMBER: " + export
          .RestartCase.Number;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (IsEmpty(export.RestartCaseRole.Type1))
      {
        local.EabReportSend.RptDetail = "RESTART CASE ROLE: N/A";
      }
      else
      {
        local.EabReportSend.RptDetail = "RESTART CASE ROLE: " + export
          .RestartCaseRole.Type1;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
    export.Infrastructure.EventId = 8;
    export.Infrastructure.ReasonCode = "MODFNRVWDT";
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

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
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

  private bool ReadDocument()
  {
    entities.ExistingDocument.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "name", export.Document.Name);
      },
      (db, reader) =>
      {
        entities.ExistingDocument.Name = db.GetString(reader, 0);
        entities.ExistingDocument.EffectiveDate = db.GetDate(reader, 1);
        entities.ExistingDocument.Populated = true;
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
    /// A value of RestartCaseRole.
    /// </summary>
    [JsonPropertyName("restartCaseRole")]
    public CaseRole RestartCaseRole
    {
      get => restartCaseRole ??= new();
      set => restartCaseRole = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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

    private Case1 restartCase;
    private CaseRole restartCaseRole;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea current;
    private Common debugOn;
    private Document document;
    private Infrastructure infrastructure;
    private CsePerson restartCsePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
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
    /// A value of Calc.
    /// </summary>
    [JsonPropertyName("calc")]
    public Common Calc
    {
      get => calc ??= new();
      set => calc = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea null1;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common position;
    private WorkArea workArea;
    private Common calc;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingDocument.
    /// </summary>
    [JsonPropertyName("existingDocument")]
    public Document ExistingDocument
    {
      get => existingDocument ??= new();
      set => existingDocument = value;
    }

    private Case1 existingCase;
    private CaseRole existingCaseRole;
    private CsePerson existingCsePerson;
    private Document existingDocument;
  }
#endregion
}
