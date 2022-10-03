// Program: SI_B274_REVISED_WAGE_REPORT, ID: 371073603, model: 746.
// Short name: SWE01289
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B274_REVISED_WAGE_REPORT.
/// </summary>
[Serializable]
public partial class SiB274RevisedWageReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B274_REVISED_WAGE_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB274RevisedWageReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB274RevisedWageReport.
  /// </summary>
  public SiB274RevisedWageReport(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Year.Text4 = NumberToString(import.Year.Year, 12, 4);

    switch(AsChar(import.Quarter.Text1))
    {
      case '1':
        local.Send.SendAmount =
          NumberToString((long)(import.Existing.LastQtrIncome.
            GetValueOrDefault() * 100), 15);

        break;
      case '2':
        local.Send.SendAmount =
          NumberToString((long)(import.Existing.Attribute2NdQtrIncome.
            GetValueOrDefault() * 100), 15);

        break;
      case '3':
        local.Send.SendAmount =
          NumberToString((long)(import.Existing.Attribute3RdQtrIncome.
            GetValueOrDefault() * 100), 15);

        break;
      case '4':
        local.Send.SendAmount =
          NumberToString((long)(import.Existing.Attribute4ThQtrIncome.
            GetValueOrDefault() * 100), 15);

        break;
      default:
        break;
    }

    local.Send.SendNonSuppressPos = 2;
    UseEabConvertNumeric1();
    local.PreviousWage.Text10 =
      Substring(local.Return1.ReturnCurrencySigned, 11, 10);
    local.Send.SendAmount =
      NumberToString((long)(import.Wage.AverageCurrency * 100), 15);
    local.Send.SendNonSuppressPos = 2;
    UseEabConvertNumeric1();
    local.RevisedWage.Text10 =
      Substring(local.Return1.ReturnCurrencySigned, 11, 10);
    local.EabReportSend.RptDetail = import.CsePerson.Number + " " + (
      import.Employer.Name ?? "") + " " + local.Year.Text4 + "  " + import
      .Quarter.Text1 + "  " + local.PreviousWage.Text10 + " " + local
      .RevisedWage.Text10;
    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport05();
  }

  private void UseCabBusinessReport05()
  {
    var useImport = new CabBusinessReport05.Import();
    var useExport = new CabBusinessReport05.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport05.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.Send);
    useExport.EabConvertNumeric.Assign(local.Return1);

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.Return1.Assign(useExport.EabConvertNumeric);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public IncomeSource Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of Quarter.
    /// </summary>
    [JsonPropertyName("quarter")]
    public TextWorkArea Quarter
    {
      get => quarter ??= new();
      set => quarter = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public DateWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of Wage.
    /// </summary>
    [JsonPropertyName("wage")]
    public Common Wage
    {
      get => wage ??= new();
      set => wage = value;
    }

    private CsePerson csePerson;
    private Employer employer;
    private IncomeSource existing;
    private TextWorkArea quarter;
    private DateWorkArea year;
    private Common wage;
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
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public EabConvertNumeric2 Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of Send.
    /// </summary>
    [JsonPropertyName("send")]
    public EabConvertNumeric2 Send
    {
      get => send ??= new();
      set => send = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public TextWorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    /// <summary>
    /// A value of RevisedWage.
    /// </summary>
    [JsonPropertyName("revisedWage")]
    public TextWorkArea RevisedWage
    {
      get => revisedWage ??= new();
      set => revisedWage = value;
    }

    /// <summary>
    /// A value of PreviousWage.
    /// </summary>
    [JsonPropertyName("previousWage")]
    public TextWorkArea PreviousWage
    {
      get => previousWage ??= new();
      set => previousWage = value;
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

    private EabConvertNumeric2 return1;
    private EabConvertNumeric2 send;
    private TextWorkArea year;
    private TextWorkArea revisedWage;
    private TextWorkArea previousWage;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
