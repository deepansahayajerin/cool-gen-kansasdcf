// Program: FN_OCSE157_GET_RESTART_LINE_NBR, ID: 371092707, model: 746.
// Short name: SWE02916
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_OCSE157_GET_RESTART_LINE_NBR.
/// </summary>
[Serializable]
public partial class FnOcse157GetRestartLineNbr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCSE157_GET_RESTART_LINE_NBR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcse157GetRestartLineNbr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcse157GetRestartLineNbr.
  /// </summary>
  public FnOcse157GetRestartLineNbr(IContext context, Import import,
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
    // --------------------------------------------------------------
    // This CAB will export restart line # only.  Restart parms such as
    // Case #, CSE Person #, etc will be set inside individual CABs.
    // --------------------------------------------------------------
    // --------------------------------------------------------------
    // 8/1/2001
    // - Write blank line to control report for easier reading.
    // --------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) != 'Y')
    {
      local.EabReportSend.RptDetail =
        "Error calling fn_ocse157_get_restart_line_nbr.";
      UseCabErrorReport();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    export.Restart.LineNumber =
      Substring(import.ProgramCheckpointRestart.RestartInfo, 1, 2);
    local.EabReportSend.RptDetail = "Program is being restarted at line # " + (
      export.Restart.LineNumber ?? "");
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "RESTART PARAMETERS....................................................................",
      1, 50);
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring("Position # 4-53...........................", 1, 23) + Substring
      (import.ProgramCheckpointRestart.RestartInfo, 250, 4, 50);
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    if (!IsEmpty(Substring(import.ProgramCheckpointRestart.RestartInfo, 54, 50)))
      
    {
      local.EabReportSend.RptDetail =
        Substring("Position # 54-103...........................", 1, 23) + Substring
        (import.ProgramCheckpointRestart.RestartInfo, 250, 54, 50);
      UseCabControlReport();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (!IsEmpty(Substring(import.ProgramCheckpointRestart.RestartInfo, 104, 50)))
      
    {
      local.EabReportSend.RptDetail =
        Substring("Position # 104-153...........................", 1, 23) + Substring
        (import.ProgramCheckpointRestart.RestartInfo, 250, 104, 50);
      UseCabControlReport();

      if (!Equal(local.Status.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
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
    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
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
    public Ocse157Verification Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    private Ocse157Verification restart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
