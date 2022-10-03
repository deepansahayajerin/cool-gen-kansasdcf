// Program: SI_B273_CLOSE, ID: 371058296, model: 746.
// Short name: SWE01270
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_CLOSE.
/// </summary>
[Serializable]
public partial class SiB273Close: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_CLOSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273Close(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273Close.
  /// </summary>
  public SiB273Close(IContext context, Import import, Export export):
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
    // CLOSE ADABAS
    // **********************************************************
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    // **********************************************************
    // CLOSE INPUT DHR FILE
    // **********************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseEabReadFederalNewHireFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "ERROR CLOSING FEDERAL NEW HIRE INPUT FILE";
      UseCabErrorReport2();
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
            "RECORDS READ                       " + "   " + NumberToString
            (import.RecordsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "INCOME SOURCES CREATED             " + "   " + NumberToString
            (import.IncomeSourcesCreated.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "DATE OF HIRE UPDATES               " + "   " + NumberToString
            (import.DateOfHireUpdates.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "RECORDS SKIPPED - NEW HIRE REPORT  " + "   " + NumberToString
            (import.RecordsSkippedNuhireRp.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "RECORDS SKIPPED - DATE OF HIRE     " + "   " + NumberToString
            (import.RecordsSkippedDateHire.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "RECORDS SKIPPED - SSN MISMATCH     " + "   " + NumberToString
            (import.RecordsSkippedSsn.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "RECORDS SKIPPED - NOT TYPE ONE     " + "   " + NumberToString
            (import.RecordsSkippedNotOne.Count, 15);

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "RECORDS PERSON NOT FOUND           " + "   " + NumberToString
            (import.RecordsPersonNotFound.Count, 15);

          break;
        case 10:
          local.EabReportSend.RptDetail =
            "RECORDS ALREADY PROCESSED          " + "   " + NumberToString
            (import.RecordsAlreadyProcessed.Count, 15);

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "                                      " + "---------------";

          break;
        case 12:
          local.TotalProcessed.Count = import.IncomeSourcesCreated.Count + import
            .DateOfHireUpdates.Count + import.RecordsSkippedNuhireRp.Count + import
            .RecordsSkippedDateHire.Count + import.RecordsSkippedSsn.Count + import
            .RecordsSkippedNotOne.Count + import.RecordsPersonNotFound.Count + import
            .RecordsAlreadyProcessed.Count;
          local.EabReportSend.RptDetail =
            "                    TOTAL:         " + "   " + NumberToString
            (local.TotalProcessed.Count, 15);

          break;
        case 13:
          local.EabReportSend.RptDetail = "";

          break;
        case 14:
          local.EabReportSend.RptDetail = "";

          break;
        case 15:
          local.EabReportSend.RptDetail = "";

          break;
        case 16:
          local.EabReportSend.RptDetail = "";

          break;
        case 17:
          local.EabReportSend.RptDetail =
            "EMPLOYERS CREATED                  " + "   " + NumberToString
            (import.EmployersCreated.Count, 15);

          break;
        case 18:
          local.EabReportSend.RptDetail =
            "EMPLOYER INFO MISMATCHED           " + "   " + NumberToString
            (import.EmployerInfoMismatch.Count, 15);

          break;
        case 19:
          local.EabReportSend.RptDetail =
            "EMPLOYEE NAME MISMATCHED           " + "   " + NumberToString
            (import.EmployeeNameMismatch.Count, 15);

          break;
        case 20:
          local.EabFileHandling.Action = "CLOSE";

          break;
        default:
          break;
      }

      UseCabControlReport();
      ++local.Subscript.Count;
    }
    while(local.Subscript.Count <= 20);

    // **********************************************************
    // CLOSE OUTPUT ERROR REPORT
    // **********************************************************
    UseCabErrorReport1();

    // **********************************************************
    // CLOSE OUTPUT BUSINESS REPORTS
    // **********************************************************
    UseCabBusinessReport01();
    UseCabBusinessReport02();
    UseCabBusinessReport03();
    UseCabBusinessReport04();
    UseCabBusinessReport05();
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);
  }

  private void UseCabBusinessReport02()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport02.Execute, useImport, useExport);
  }

  private void UseCabBusinessReport03()
  {
    var useImport = new CabBusinessReport03.Import();
    var useExport = new CabBusinessReport03.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport03.Execute, useImport, useExport);
  }

  private void UseCabBusinessReport04()
  {
    var useImport = new CabBusinessReport04.Import();
    var useExport = new CabBusinessReport04.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport04.Execute, useImport, useExport);
  }

  private void UseCabBusinessReport05()
  {
    var useImport = new CabBusinessReport05.Import();
    var useExport = new CabBusinessReport05.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport05.Execute, useImport, useExport);
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseEabReadFederalNewHireFile()
  {
    var useImport = new EabReadFederalNewHireFile.Import();
    var useExport = new EabReadFederalNewHireFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFederalNewHireFile.Execute, useImport, useExport);

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
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of EmployersCreated.
    /// </summary>
    [JsonPropertyName("employersCreated")]
    public Common EmployersCreated
    {
      get => employersCreated ??= new();
      set => employersCreated = value;
    }

    /// <summary>
    /// A value of IncomeSourcesCreated.
    /// </summary>
    [JsonPropertyName("incomeSourcesCreated")]
    public Common IncomeSourcesCreated
    {
      get => incomeSourcesCreated ??= new();
      set => incomeSourcesCreated = value;
    }

    /// <summary>
    /// A value of EmployerInfoMismatch.
    /// </summary>
    [JsonPropertyName("employerInfoMismatch")]
    public Common EmployerInfoMismatch
    {
      get => employerInfoMismatch ??= new();
      set => employerInfoMismatch = value;
    }

    /// <summary>
    /// A value of EmployeeNameMismatch.
    /// </summary>
    [JsonPropertyName("employeeNameMismatch")]
    public Common EmployeeNameMismatch
    {
      get => employeeNameMismatch ??= new();
      set => employeeNameMismatch = value;
    }

    /// <summary>
    /// A value of DateOfHireUpdates.
    /// </summary>
    [JsonPropertyName("dateOfHireUpdates")]
    public Common DateOfHireUpdates
    {
      get => dateOfHireUpdates ??= new();
      set => dateOfHireUpdates = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNuhireRp.
    /// </summary>
    [JsonPropertyName("recordsSkippedNuhireRp")]
    public Common RecordsSkippedNuhireRp
    {
      get => recordsSkippedNuhireRp ??= new();
      set => recordsSkippedNuhireRp = value;
    }

    /// <summary>
    /// A value of RecordsSkippedDateHire.
    /// </summary>
    [JsonPropertyName("recordsSkippedDateHire")]
    public Common RecordsSkippedDateHire
    {
      get => recordsSkippedDateHire ??= new();
      set => recordsSkippedDateHire = value;
    }

    /// <summary>
    /// A value of RecordsSkippedSsn.
    /// </summary>
    [JsonPropertyName("recordsSkippedSsn")]
    public Common RecordsSkippedSsn
    {
      get => recordsSkippedSsn ??= new();
      set => recordsSkippedSsn = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNotOne.
    /// </summary>
    [JsonPropertyName("recordsSkippedNotOne")]
    public Common RecordsSkippedNotOne
    {
      get => recordsSkippedNotOne ??= new();
      set => recordsSkippedNotOne = value;
    }

    /// <summary>
    /// A value of RecordsPersonNotFound.
    /// </summary>
    [JsonPropertyName("recordsPersonNotFound")]
    public Common RecordsPersonNotFound
    {
      get => recordsPersonNotFound ??= new();
      set => recordsPersonNotFound = value;
    }

    /// <summary>
    /// A value of RecordsAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("recordsAlreadyProcessed")]
    public Common RecordsAlreadyProcessed
    {
      get => recordsAlreadyProcessed ??= new();
      set => recordsAlreadyProcessed = value;
    }

    private Common recordsRead;
    private Common employersCreated;
    private Common incomeSourcesCreated;
    private Common employerInfoMismatch;
    private Common employeeNameMismatch;
    private Common dateOfHireUpdates;
    private Common recordsSkippedNuhireRp;
    private Common recordsSkippedDateHire;
    private Common recordsSkippedSsn;
    private Common recordsSkippedNotOne;
    private Common recordsPersonNotFound;
    private Common recordsAlreadyProcessed;
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
    /// A value of TotalProcessed.
    /// </summary>
    [JsonPropertyName("totalProcessed")]
    public Common TotalProcessed
    {
      get => totalProcessed ??= new();
      set => totalProcessed = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private Common totalProcessed;
    private CsePersonsWorkSet csePersonsWorkSet;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common subscript;
  }
#endregion
}
