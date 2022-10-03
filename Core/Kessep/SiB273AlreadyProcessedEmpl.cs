// Program: SI_B273_ALREADY_PROCESSED_EMPL, ID: 1902631046, model: 746.
// Short name: SWE01242
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_ALREADY_PROCESSED_EMPL.
/// </summary>
[Serializable]
public partial class SiB273AlreadyProcessedEmpl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_ALREADY_PROCESSED_EMPL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273AlreadyProcessedEmpl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273AlreadyProcessedEmpl.
  /// </summary>
  public SiB273AlreadyProcessedEmpl(IContext context, Import import,
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
    local.EabFileHandling.Action = "WRITE";
    local.Counter.Count = 0;
    UseCabDate2TextWithHyphens2();

    do
    {
      ++local.Counter.Count;

      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "EIN:      " + (
            import.Kaecses.Ein ?? "");

          break;
        case 2:
          local.EabReportSend.RptDetail = "NAME:     " + (
            import.Kaecses.Name ?? "") + "    " + "                  ";

          break;
        case 3:
          local.EabReportSend.RptDetail = "PERSON #: " + import
            .CsePerson.Number;

          break;
        case 4:
          local.InConvert.Date = Date(import.Cse.CreatedTimestamp);
          UseCabDate2TextWithHyphens2();
          local.EabReportSend.RptDetail = "CREATED BY:        " + import
            .Cse.CreatedBy + "     CREATED DATE:      " + local.Date1.Text10;

          break;
        case 5:
          local.InConvert.Date = Date(import.Cse.LastUpdatedTimestamp);
          UseCabDate2TextWithHyphens2();
          local.EabReportSend.RptDetail = "LAST UPDATED BY:   " + (
            import.Cse.LastUpdatedBy ?? "") + "     LAST UPDATED:      " + local
            .Date1.Text10;

          break;
        case 6:
          local.InConvert.Date = import.Cse.ReturnDt;
          UseCabDate2TextWithHyphens2();
          local.EabReportSend.RptDetail = "EMPLOYMENT STATUS: " + (
            import.Cse.ReturnCd ?? "") + "            RETURN DATE :      " + local
            .Date1.Text10;

          break;
        case 7:
          local.InConvert.Date = import.Cse.StartDt;
          UseCabDate2TextWithHyphens2();
          local.InConvert.Date = import.NewHireIncomeSource.StartDt;
          UseCabDate2TextWithHyphens1();
          local.EabReportSend.RptDetail = "CSE START DATE:    " + local
            .Date1.Text10 + "   NEWHIRE HIRE DATE: " + local.Date2.Text10;

          break;
        case 8:
          local.InConvert.Date = import.Cse.EndDt;
          UseCabDate2TextWithHyphens2();
          local.EabReportSend.RptDetail = "CSE END DATE:      " + local
            .Date1.Text10;

          break;
        case 9:
          local.EabReportSend.RptDetail = "WORKER ID:         " + (
            import.Cse.WorkerId ?? "");

          break;
        case 10:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport05();
    }
    while(local.Counter.Count <= 10);
  }

  private void UseCabBusinessReport05()
  {
    var useImport = new CabBusinessReport05.Import();
    var useExport = new CabBusinessReport05.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport05.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.InConvert.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.Date2.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.InConvert.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.Date1.Text10 = useExport.TextWorkArea.Text10;
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
    /// A value of NewHireIncomeSource.
    /// </summary>
    [JsonPropertyName("newHireIncomeSource")]
    public IncomeSource NewHireIncomeSource
    {
      get => newHireIncomeSource ??= new();
      set => newHireIncomeSource = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public IncomeSource Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Kaecses.
    /// </summary>
    [JsonPropertyName("kaecses")]
    public Employer Kaecses
    {
      get => kaecses ??= new();
      set => kaecses = value;
    }

    /// <summary>
    /// A value of NewHireEmployer.
    /// </summary>
    [JsonPropertyName("newHireEmployer")]
    public Employer NewHireEmployer
    {
      get => newHireEmployer ??= new();
      set => newHireEmployer = value;
    }

    private IncomeSource newHireIncomeSource;
    private IncomeSource cse;
    private CsePerson csePerson;
    private Employer kaecses;
    private Employer newHireEmployer;
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
    /// A value of Date2.
    /// </summary>
    [JsonPropertyName("date2")]
    public TextWorkArea Date2
    {
      get => date2 ??= new();
      set => date2 = value;
    }

    /// <summary>
    /// A value of Date1.
    /// </summary>
    [JsonPropertyName("date1")]
    public TextWorkArea Date1
    {
      get => date1 ??= new();
      set => date1 = value;
    }

    /// <summary>
    /// A value of InConvert.
    /// </summary>
    [JsonPropertyName("inConvert")]
    public DateWorkArea InConvert
    {
      get => inConvert ??= new();
      set => inConvert = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
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

    private TextWorkArea date2;
    private TextWorkArea date1;
    private DateWorkArea inConvert;
    private Common counter;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
