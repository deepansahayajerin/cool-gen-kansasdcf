// Program: FN_B669_CLOSE, ID: 372277427, model: 746.
// Short name: SWE02344
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B669_CLOSE.
/// </summary>
[Serializable]
public partial class FnB669Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B669_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB669Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB669Close.
  /// </summary>
  public FnB669Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***** Set restart indicator to no because we successfully finished this 
    // program.
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error trying to update checkpoint restart during closing.";
      UseCabErrorReport();
    }

    // **********************************************************
    // WRITE OUTPUT CONTROL REPORT AND CLOSE
    // **********************************************************
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "NEW DEBTS PROCESSED                " + "   " + NumberToString
            (import.NewDebtsProcessed.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "CASH RECEIPT DETAILS UPDATED       " + "   " + NumberToString
            (import.NoCashRcptDtlReversed.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "COLLECTIONS REVERSED               " + "   " + NumberToString
            (import.NoCollectiosReversed.Count, 15);

          break;
        case 4:
          local.EabFileHandling.Action = "CLOSE";

          break;
        case 5:
          break;
        case 6:
          break;
        case 7:
          break;
        case 8:
          break;
        case 9:
          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 4);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    UseCabErrorReport();

    // **********************************************************
    // CLOSE OUTPUT BUSINESS REPORT 01
    // **********************************************************
    local.Subscript.Count = 1;

    do
    {
      switch(local.Subscript.Count)
      {
        case 1:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabFileHandling.Action = "CLOSE";

          break;
        case 3:
          break;
        case 4:
          break;
        case 5:
          break;
        case 6:
          break;
        case 7:
          break;
        case 8:
          break;
        case 9:
          break;
        default:
          break;
      }

      UseCabBusinessReport01();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 2);
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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

    /// <summary>
    /// A value of NoCashRcptDtlReversed.
    /// </summary>
    [JsonPropertyName("noCashRcptDtlReversed")]
    public Common NoCashRcptDtlReversed
    {
      get => noCashRcptDtlReversed ??= new();
      set => noCashRcptDtlReversed = value;
    }

    /// <summary>
    /// A value of NoCollectiosReversed.
    /// </summary>
    [JsonPropertyName("noCollectiosReversed")]
    public Common NoCollectiosReversed
    {
      get => noCollectiosReversed ??= new();
      set => noCollectiosReversed = value;
    }

    /// <summary>
    /// A value of NewDebtsProcessed.
    /// </summary>
    [JsonPropertyName("newDebtsProcessed")]
    public Common NewDebtsProcessed
    {
      get => newDebtsProcessed ??= new();
      set => newDebtsProcessed = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Common noCashRcptDtlReversed;
    private Common noCollectiosReversed;
    private Common newDebtsProcessed;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
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

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      programCheckpointRestart = null;
      eabFileHandling = null;
      subscript = null;
      eabConvertNumeric = null;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
    private EabConvertNumeric2 eabConvertNumeric;
  }
#endregion
}
