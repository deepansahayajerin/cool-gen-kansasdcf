// Program: FN_B672_HOUSEKEEPING, ID: 372402818, model: 746.
// Short name: SWE02423
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B672_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB672Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B672_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB672Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB672Housekeeping.
  /// </summary>
  public FnB672Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Process.Date = Now().Date;
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.Process.Date;
    local.EabReportSend.ProgramName = global.UserId;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN INBOUND EFT FILE AND READ THE HEADER RECORD
    // **********************************************************
    UseEabAccessInboundEftFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // Write out the error line returned from the external.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 30) + local
        .EabFileHandling.Status;
      UseCabErrorReport1();
      local.EabReportSend.RptDetail =
        "Abend error occurred.  Contact system support.";
      UseCabErrorReport1();
      ExitState = "OE0000_ERROR_READING_EXT_FILE";

      return;
    }

    // **********************************************************
    // WRITE INBOUND EFT FILE CREATION DATE AND TIME TO CONTROL REPORT 98
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "Bank File Information:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.WorkArea.Text15 =
      NumberToString(DateToInt(export.EftHeaderRecord.Date), 15);
    local.EabReportSend.RptDetail = "Inbound EFT file creation date..." + Substring
      (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 8, 4) + "-" + Substring
      (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 12, 2) + "-" + Substring
      (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.WorkArea.Text15 =
      NumberToString(TimeToInt(export.EftHeaderRecord.Time), 15);
    local.EabReportSend.RptDetail = "Inbound EFT file creation time..." + Substring
      (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 10, 2) + ":" + Substring
      (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 12, 2);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
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

  private void UseEabAccessInboundEftFile()
  {
    var useImport = new EabAccessInboundEftFile.Import();
    var useExport = new EabAccessInboundEftFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.Error.RptDetail = local.EabReportSend.RptDetail;
    MoveDateWorkArea(export.EftHeaderRecord, useExport.EftHeaderRecord);

    Call(EabAccessInboundEftFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.EabReportSend.RptDetail = useExport.Error.RptDetail;
    MoveDateWorkArea(useExport.EftHeaderRecord, export.EftHeaderRecord);
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of EftHeaderRecord.
    /// </summary>
    [JsonPropertyName("eftHeaderRecord")]
    public DateWorkArea EftHeaderRecord
    {
      get => eftHeaderRecord ??= new();
      set => eftHeaderRecord = value;
    }

    private DateWorkArea process;
    private DateWorkArea eftHeaderRecord;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private WorkArea workArea;
  }
#endregion
}
