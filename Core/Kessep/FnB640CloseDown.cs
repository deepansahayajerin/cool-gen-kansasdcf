// Program: FN_B640_CLOSE_DOWN, ID: 371021194, model: 746.
// Short name: SWE02810
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B640_CLOSE_DOWN.
/// </summary>
[Serializable]
public partial class FnB640CloseDown: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B640_CLOSE_DOWN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB640CloseDown(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB640CloseDown.
  /// </summary>
  public FnB640CloseDown(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "RUN RESULTS AS FOLLOWS:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "TOTAL NUMBER OF COLLECTIONS READ..........................................................           ",
      1, 50) + "    " + NumberToString(import.NbrOfRecordsRead.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "TOTAL NUMBER OF COLLECTIONS UPDATED...................................................        ",
      1, 50) + "    " + NumberToString(import.NbrOfRecordsUpdated.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring(
        "TOTAL NUMBER OF INTERFACE RECORDS CREATED..............................................  ",
      1, 50) + "    " + NumberToString(import.NbrOfRecordsCreated.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    /// A value of NbrOfRecordsRead.
    /// </summary>
    [JsonPropertyName("nbrOfRecordsRead")]
    public Common NbrOfRecordsRead
    {
      get => nbrOfRecordsRead ??= new();
      set => nbrOfRecordsRead = value;
    }

    /// <summary>
    /// A value of NbrOfRecordsUpdated.
    /// </summary>
    [JsonPropertyName("nbrOfRecordsUpdated")]
    public Common NbrOfRecordsUpdated
    {
      get => nbrOfRecordsUpdated ??= new();
      set => nbrOfRecordsUpdated = value;
    }

    /// <summary>
    /// A value of NbrOfRecordsCreated.
    /// </summary>
    [JsonPropertyName("nbrOfRecordsCreated")]
    public Common NbrOfRecordsCreated
    {
      get => nbrOfRecordsCreated ??= new();
      set => nbrOfRecordsCreated = value;
    }

    private Common nbrOfRecordsRead;
    private Common nbrOfRecordsUpdated;
    private Common nbrOfRecordsCreated;
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

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
