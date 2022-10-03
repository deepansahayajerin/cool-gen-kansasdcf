// Program: FN_B634_CLOSE, ID: 372264647, model: 746.
// Short name: SWE02342
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B634_CLOSE.
/// </summary>
[Serializable]
public partial class FnB634Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B634_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB634Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB634Close.
  /// </summary>
  public FnB634Close(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
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
            "PROGRAMS CHANGED                   " + "   " + NumberToString
            (import.NoProgramsChanged.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "URA ADJUSTMENTS                    " + "   " + NumberToString
            (import.NoUraAdjustments.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "COURT ORDER NUMBER CHANGES         " + "   " + NumberToString
            (import.NoConChanges.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "DEACTIVATED COLL PROT CHANGES      " + "   " + NumberToString
            (import.NoDeactivatedCollProt.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "CASH RECEIPT DETAILS UPDATED       " + "   " + NumberToString
            (import.NoCashRcptDtlsUpdated.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "COLLECTIONS REVERSED               " + "   " + NumberToString
            (import.NoCollectionsReversed.Count, 15);

          break;
        case 7:
          local.EabFileHandling.Action = "CLOSE";

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
    while(local.Subscript.Count <= 7);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    UseCabErrorReport();

    if (AsChar(import.ReportNeeded.Flag) == 'Y')
    {
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
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
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
    /// A value of NoCashRcptDtlsUpdated.
    /// </summary>
    [JsonPropertyName("noCashRcptDtlsUpdated")]
    public Common NoCashRcptDtlsUpdated
    {
      get => noCashRcptDtlsUpdated ??= new();
      set => noCashRcptDtlsUpdated = value;
    }

    /// <summary>
    /// A value of NoCollectionsReversed.
    /// </summary>
    [JsonPropertyName("noCollectionsReversed")]
    public Common NoCollectionsReversed
    {
      get => noCollectionsReversed ??= new();
      set => noCollectionsReversed = value;
    }

    /// <summary>
    /// A value of NoProgramsChanged.
    /// </summary>
    [JsonPropertyName("noProgramsChanged")]
    public Common NoProgramsChanged
    {
      get => noProgramsChanged ??= new();
      set => noProgramsChanged = value;
    }

    /// <summary>
    /// A value of NoUraAdjustments.
    /// </summary>
    [JsonPropertyName("noUraAdjustments")]
    public Common NoUraAdjustments
    {
      get => noUraAdjustments ??= new();
      set => noUraAdjustments = value;
    }

    /// <summary>
    /// A value of NoConChanges.
    /// </summary>
    [JsonPropertyName("noConChanges")]
    public Common NoConChanges
    {
      get => noConChanges ??= new();
      set => noConChanges = value;
    }

    /// <summary>
    /// A value of NoDeactivatedCollProt.
    /// </summary>
    [JsonPropertyName("noDeactivatedCollProt")]
    public Common NoDeactivatedCollProt
    {
      get => noDeactivatedCollProt ??= new();
      set => noDeactivatedCollProt = value;
    }

    private Common reportNeeded;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common noCashRcptDtlsUpdated;
    private Common noCollectionsReversed;
    private Common noProgramsChanged;
    private Common noUraAdjustments;
    private Common noConChanges;
    private Common noDeactivatedCollProt;
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
  public class Local
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

    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
    private EabConvertNumeric2 eabConvertNumeric;
  }
#endregion
}
