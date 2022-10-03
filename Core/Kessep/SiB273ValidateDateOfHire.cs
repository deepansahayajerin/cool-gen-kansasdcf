// Program: SI_B273_VALIDATE_DATE_OF_HIRE, ID: 373496036, model: 746.
// Short name: SWE01269
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_VALIDATE_DATE_OF_HIRE.
/// </summary>
[Serializable]
public partial class SiB273ValidateDateOfHire: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_VALIDATE_DATE_OF_HIRE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273ValidateDateOfHire(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273ValidateDateOfHire.
  /// </summary>
  public SiB273ValidateDateOfHire(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    UseEabDateValidationRoutine();

    switch(TrimEnd(local.Validity.ActionEntry))
    {
      case "":
        // Valid Date
        if (Lt(import.Process.Date, export.Hire.Date))
        {
          export.Hire.Date = import.Process.Date;
          local.After.Text10 =
            NumberToString(Month(export.Hire.Date), 14, 2) + "/" + NumberToString
            (Day(export.Hire.Date), 14, 2) + "/" + NumberToString
            (Year(export.Hire.Date), 12, 4);
          local.NeededToWrite.RptDetail = "Hire Date corrected   for " + import
            .Kaecses.Number;
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date " + import
            .Hire.TextDate + " changed to: " + local.After.Text10;

          break;
        }

        if (Lt(export.Hire.Date, AddMonths(import.Process.Date, -6)))
        {
          local.After.Text10 =
            NumberToString(Month(export.Hire.Date), 14, 2) + "/" + NumberToString
            (Day(export.Hire.Date), 14, 2) + "/" + NumberToString
            (Year(export.Hire.Date), 12, 4);
          local.NeededToWrite.RptDetail = "Record Skipped # " + import
            .Kaecses.Number;
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date " + local
            .After.Text10 + " is > 6 months old";
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " EIN:" + (
              import.Employer.Ein ?? "") + " NAME: " + (
              import.Employer.Name ?? "");

          break;
        }

        return;
      case "ND":
        // No Date
        export.Hire.Date = import.Process.Date;

        return;
      case "NN":
        // Not Numeric
        export.Hire.Date = import.Process.Date;
        local.NeededToWrite.RptDetail = "Hire Date not numeric for " + import
          .Kaecses.Number;
        local.After.Text10 = NumberToString(Month(export.Hire.Date), 14, 2) + "/"
          + NumberToString(Day(export.Hire.Date), 14, 2) + "/" + NumberToString
          (Year(export.Hire.Date), 12, 4);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date " + import
          .Hire.TextDate + " changed to: " + local.After.Text10;

        break;
      case "BM":
        // Bad Month
        export.Hire.Date = import.Process.Date;
        local.NeededToWrite.RptDetail = "Hire Date invalid mm  for " + import
          .Kaecses.Number;
        local.After.Text10 = NumberToString(Month(export.Hire.Date), 14, 2) + "/"
          + NumberToString(Day(export.Hire.Date), 14, 2) + "/" + NumberToString
          (Year(export.Hire.Date), 12, 4);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date " + import
          .Hire.TextDate + " changed to: " + local.After.Text10;

        break;
      case "BD":
        // Bad Day
        export.Hire.Date = import.Process.Date;
        local.NeededToWrite.RptDetail = "Hire Date invalid dd  for " + import
          .Kaecses.Number;
        local.After.Text10 = NumberToString(Month(export.Hire.Date), 14, 2) + "/"
          + NumberToString(Day(export.Hire.Date), 14, 2) + "/" + NumberToString
          (Year(export.Hire.Date), 12, 4);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date " + import
          .Hire.TextDate + " changed to: " + local.After.Text10;

        break;
      case "BC":
        // Bad Century
        export.Hire.Date = import.Process.Date;
        local.NeededToWrite.RptDetail = "Hire Date invalid cc  for " + import
          .Kaecses.Number;
        local.After.Text10 = NumberToString(Month(export.Hire.Date), 14, 2) + "/"
          + NumberToString(Day(export.Hire.Date), 14, 2) + "/" + NumberToString
          (Year(export.Hire.Date), 12, 4);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date " + import
          .Hire.TextDate + " changed to: " + local.After.Text10;

        break;
      case "CD":
        // Corrected Date
        if (Lt(export.Hire.Date, AddMonths(import.Process.Date, -6)))
        {
          local.After.Text10 =
            NumberToString(Month(export.Hire.Date), 14, 2) + "/" + NumberToString
            (Day(export.Hire.Date), 14, 2) + "/" + NumberToString
            (Year(export.Hire.Date), 12, 4);
          local.NeededToWrite.RptDetail = "Record Skipped - # " + import
            .Kaecses.Number;
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date " + local
            .After.Text10 + " is more than 6 months old.";
        }
        else
        {
          local.NeededToWrite.RptDetail = "Hire Date corrected   for " + import
            .Kaecses.Number;
          local.After.Text10 =
            NumberToString(Month(export.Hire.Date), 14, 2) + "/" + NumberToString
            (Day(export.Hire.Date), 14, 2) + "/" + NumberToString
            (Year(export.Hire.Date), 12, 4);
          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date " + import
            .Hire.TextDate + " changed to: " + local.After.Text10;
        }

        break;
      default:
        // Date Routine Error
        export.Hire.Date = import.Process.Date;
        local.NeededToWrite.RptDetail = "Hire Date Routine Error for " + import
          .Kaecses.Number;
        local.After.Text10 =
          NumberToString(Month(import.Process.Date), 14, 2) + "/" + NumberToString
          (Day(import.Process.Date), 14, 2) + "/" + NumberToString
          (Year(import.Process.Date), 12, 4);
        local.NeededToWrite.RptDetail =
          TrimEnd(local.NeededToWrite.RptDetail) + " - Hire Date " + import
          .Hire.TextDate + " received validity code: " + local
          .Validity.ActionEntry;

        break;
    }

    local.EabFileHandling.Action = "WRITE";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabDateValidationRoutine()
  {
    var useImport = new EabDateValidationRoutine.Import();
    var useExport = new EabDateValidationRoutine.Export();

    useImport.DateWorkArea.TextDate = import.Hire.TextDate;
    useExport.DateWorkArea.Date = export.Hire.Date;
    useExport.Validity.ActionEntry = local.Validity.ActionEntry;

    Call(EabDateValidationRoutine.Execute, useImport, useExport);

    export.Hire.Date = useExport.DateWorkArea.Date;
    local.Validity.ActionEntry = useExport.Validity.ActionEntry;
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
    /// A value of Hire.
    /// </summary>
    [JsonPropertyName("hire")]
    public DateWorkArea Hire
    {
      get => hire ??= new();
      set => hire = value;
    }

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
    /// A value of Kaecses.
    /// </summary>
    [JsonPropertyName("kaecses")]
    public CsePersonsWorkSet Kaecses
    {
      get => kaecses ??= new();
      set => kaecses = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private DateWorkArea hire;
    private DateWorkArea process;
    private CsePersonsWorkSet kaecses;
    private Employer employer;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Hire.
    /// </summary>
    [JsonPropertyName("hire")]
    public DateWorkArea Hire
    {
      get => hire ??= new();
      set => hire = value;
    }

    private DateWorkArea hire;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Before.
    /// </summary>
    [JsonPropertyName("before")]
    public TextWorkArea Before
    {
      get => before ??= new();
      set => before = value;
    }

    /// <summary>
    /// A value of After.
    /// </summary>
    [JsonPropertyName("after")]
    public TextWorkArea After
    {
      get => after ??= new();
      set => after = value;
    }

    /// <summary>
    /// A value of Validity.
    /// </summary>
    [JsonPropertyName("validity")]
    public Common Validity
    {
      get => validity ??= new();
      set => validity = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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

    private TextWorkArea before;
    private TextWorkArea after;
    private Common validity;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
