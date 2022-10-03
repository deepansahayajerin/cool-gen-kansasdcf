// Program: FN_B643_WRITE_CONTROLS_AND_CLOSE, ID: 372683788, model: 746.
// Short name: SWE02384
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_WRITE_CONTROLS_AND_CLOSE.
/// </summary>
[Serializable]
public partial class FnB643WriteControlsAndClose: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_WRITE_CONTROLS_AND_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643WriteControlsAndClose(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643WriteControlsAndClose.
  /// </summary>
  public FnB643WriteControlsAndClose(IContext context, Import import,
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
    export.VendorFileRecordCount.Count = import.VendorFileRecordCount.Count;
    export.SortSequenceNumber.Count = import.SortSequenceNumber.Count;
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;

    // **********************************************************
    // WRITE TRAILER RECORD (REC TYPE = 99)
    // Note:  using a currency field in eab to store a stmt and cpn count.
    // **********************************************************
    local.RecordType.ActionEntry = "99";
    local.EabFileHandling.Action = "WRITE";
    local.NumberOfStmts.AverageCurrency = import.StmtsCreated.Count;
    local.NumberOfCoupons.AverageCurrency = import.CpnsCreated.Count;
    UseEabCreateVendorFile1();

    // **********************************************************
    // CLOSE OUTPUT STATEMENT EXTRACT FILE TO BE SENT TO VENDOR
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseEabCreateVendorFile2();

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
            "OBLIGORS READ                      " + "   " + NumberToString
            (import.ObligorsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "STATEMENTS CREATED                 " + "   " + NumberToString
            (import.StmtsCreated.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "COUPONS CREATED                    " + "   " + NumberToString
            (import.CpnsCreated.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "STATEMENTS SUPPRESSED BY OBLIGOR   " + "   " + NumberToString
            (import.StmtsSuppressByObligor.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "COUPONS SUPPRESSED BY OBLIGOR      " + "   " + NumberToString
            (import.CpnsSuppressByObligor.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "STATEMENTS SUPPRESSED BY OBLIGATION" + "   " + NumberToString
            (import.StmtsSuppressByDebt.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "COUPONS SUPPRESSED BY OBLIGATION   " + "   " + NumberToString
            (import.CpnsSuppressByDebt.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "VENDOR FILE HANDOFF RECORDS        " + "   " + NumberToString
            (import.VendorFileRecordCount.Count, 15);

          break;
        case 9:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 9);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    UseCabErrorReport();
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile1()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.AmountOne.AverageCurrency = local.NumberOfStmts.AverageCurrency;
    useImport.AmountTwo.AverageCurrency = local.NumberOfCoupons.AverageCurrency;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile2()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

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
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of ObligorsRead.
    /// </summary>
    [JsonPropertyName("obligorsRead")]
    public Common ObligorsRead
    {
      get => obligorsRead ??= new();
      set => obligorsRead = value;
    }

    /// <summary>
    /// A value of StmtsCreated.
    /// </summary>
    [JsonPropertyName("stmtsCreated")]
    public Common StmtsCreated
    {
      get => stmtsCreated ??= new();
      set => stmtsCreated = value;
    }

    /// <summary>
    /// A value of CpnsCreated.
    /// </summary>
    [JsonPropertyName("cpnsCreated")]
    public Common CpnsCreated
    {
      get => cpnsCreated ??= new();
      set => cpnsCreated = value;
    }

    /// <summary>
    /// A value of StmtsSuppressByObligor.
    /// </summary>
    [JsonPropertyName("stmtsSuppressByObligor")]
    public Common StmtsSuppressByObligor
    {
      get => stmtsSuppressByObligor ??= new();
      set => stmtsSuppressByObligor = value;
    }

    /// <summary>
    /// A value of CpnsSuppressByObligor.
    /// </summary>
    [JsonPropertyName("cpnsSuppressByObligor")]
    public Common CpnsSuppressByObligor
    {
      get => cpnsSuppressByObligor ??= new();
      set => cpnsSuppressByObligor = value;
    }

    /// <summary>
    /// A value of StmtsSuppressByDebt.
    /// </summary>
    [JsonPropertyName("stmtsSuppressByDebt")]
    public Common StmtsSuppressByDebt
    {
      get => stmtsSuppressByDebt ??= new();
      set => stmtsSuppressByDebt = value;
    }

    /// <summary>
    /// A value of CpnsSuppressByDebt.
    /// </summary>
    [JsonPropertyName("cpnsSuppressByDebt")]
    public Common CpnsSuppressByDebt
    {
      get => cpnsSuppressByDebt ??= new();
      set => cpnsSuppressByDebt = value;
    }

    private Common sortSequenceNumber;
    private Common vendorFileRecordCount;
    private Common obligorsRead;
    private Common stmtsCreated;
    private Common cpnsCreated;
    private Common stmtsSuppressByObligor;
    private Common cpnsSuppressByObligor;
    private Common stmtsSuppressByDebt;
    private Common cpnsSuppressByDebt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
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

    private Common sortSequenceNumber;
    private Common vendorFileRecordCount;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NumberOfCoupons.
    /// </summary>
    [JsonPropertyName("numberOfCoupons")]
    public Common NumberOfCoupons
    {
      get => numberOfCoupons ??= new();
      set => numberOfCoupons = value;
    }

    /// <summary>
    /// A value of NumberOfStmts.
    /// </summary>
    [JsonPropertyName("numberOfStmts")]
    public Common NumberOfStmts
    {
      get => numberOfStmts ??= new();
      set => numberOfStmts = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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

    private Common numberOfCoupons;
    private Common numberOfStmts;
    private Common recordType;
    private Common subscript;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
