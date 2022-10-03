// Program: FN_B650_INITIALIZATION, ID: 372896157, model: 746.
// Short name: SWE02490
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B650_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnB650Initialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B650_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB650Initialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB650Initialization.
  /// </summary>
  public FnB650Initialization(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // 10/99 Fangman           Initial code
    // 11/18/99  Fangman  PR 78745  Added code to check for generation/
    // installation error that causes the user id to not be populated.  (Have
    // had this happen twice in production and once in Acceptance.)
    // 09/12/00  Fangman PR 103323  Added code to run with a range of APs.  This
    // was put in with the changes to fix the disb suppr with past discontinue
    // dates.
    // 06/18/03  Fangman  WR 030255  Added code to get parameter to determine 
    // how many collections w/ AR errors will be applied to potential recovery
    // obligations.
    // ---------------------------------------------
    // ***** Get the run parameters for this program.
    export.ProgramProcessingInfo.Name = "SWEFB650";
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.TestRunInd.Flag =
        Substring(export.ProgramProcessingInfo.ParameterList, 1, 1);
      export.DisplayInd.Flag =
        Substring(export.ProgramProcessingInfo.ParameterList, 3, 1);

      if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 5, 10)))
        
      {
        export.TestFirstObligor.Number =
          TrimEnd(Substring(export.ProgramProcessingInfo.ParameterList, 5, 10));
          
      }

      if (!IsEmpty(Substring(export.ProgramProcessingInfo.ParameterList, 16, 10)))
        
      {
        export.TestLastObligor.Number =
          TrimEnd(Substring(export.ProgramProcessingInfo.ParameterList, 16, 10));
          
      }
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(export.TestRunInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "Test run - no updates will be applied to the database.";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.DisplayInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail =
        "Display indicator is on - diagnostic information will be displayed.";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (!IsEmpty(export.TestFirstObligor.Number) || !
      IsEmpty(export.TestLastObligor.Number))
    {
      local.EabReportSend.RptDetail =
        "Processing collections for Obligors between: " + export
        .TestFirstObligor.Number + " and " + export.TestLastObligor.Number;
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.TestRunInd.Flag) == 'Y' || AsChar
      (export.DisplayInd.Flag) == 'Y' || !
      IsEmpty(export.TestFirstObligor.Number) || !
      IsEmpty(export.TestLastObligor.Number))
    {
      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (!Equal(global.UserId, export.ProgramProcessingInfo.Name))
    {
      local.EabReportSend.RptDetail =
        "Severe Error:  User_ID should be set to SWEFB650 instead of " + global
        .UserId + ".  This is usually due to an error in the generation/installation.";
        
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // ***** Get the DB2 commit frequency counts.
    export.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Fatal error occurred, must abort.  " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }

      return;
    }

    if (ReadControlTable())
    {
      export.ApsWithColToApplyToRcv.LastUsedNumber =
        entities.ControlTable.LastUsedNumber;
      local.EabReportSend.RptDetail = "NBR AP COLLS TO APPLY TO RCV:" + NumberToString
        (export.ApsWithColToApplyToRcv.LastUsedNumber, 15);
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Error: Not found reading Control table w/ ID of 'COLLECTIONS TO APPLY TO RCV'.";
        
      UseCabControlReport1();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = export.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadControlTable()
  {
    entities.ControlTable.Populated = false;

    return Read("ReadControlTable",
      null,
      (db, reader) =>
      {
        entities.ControlTable.Identifier = db.GetString(reader, 0);
        entities.ControlTable.LastUsedNumber = db.GetInt32(reader, 1);
        entities.ControlTable.Populated = true;
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
    /// A value of TestRunInd.
    /// </summary>
    [JsonPropertyName("testRunInd")]
    public Common TestRunInd
    {
      get => testRunInd ??= new();
      set => testRunInd = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of TestFirstObligor.
    /// </summary>
    [JsonPropertyName("testFirstObligor")]
    public CsePerson TestFirstObligor
    {
      get => testFirstObligor ??= new();
      set => testFirstObligor = value;
    }

    /// <summary>
    /// A value of TestLastObligor.
    /// </summary>
    [JsonPropertyName("testLastObligor")]
    public CsePerson TestLastObligor
    {
      get => testLastObligor ??= new();
      set => testLastObligor = value;
    }

    /// <summary>
    /// A value of ApsWithColToApplyToRcv.
    /// </summary>
    [JsonPropertyName("apsWithColToApplyToRcv")]
    public ControlTable ApsWithColToApplyToRcv
    {
      get => apsWithColToApplyToRcv ??= new();
      set => apsWithColToApplyToRcv = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common testRunInd;
    private Common displayInd;
    private CsePerson testFirstObligor;
    private CsePerson testLastObligor;
    private ControlTable apsWithColToApplyToRcv;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private ControlTable controlTable;
  }
#endregion
}
