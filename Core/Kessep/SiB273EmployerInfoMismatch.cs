// Program: SI_B273_EMPLOYER_INFO_MISMATCH, ID: 371058726, model: 746.
// Short name: SWE01280
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_EMPLOYER_INFO_MISMATCH.
/// </summary>
[Serializable]
public partial class SiB273EmployerInfoMismatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_EMPLOYER_INFO_MISMATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273EmployerInfoMismatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273EmployerInfoMismatch.
  /// </summary>
  public SiB273EmployerInfoMismatch(IContext context, Import import,
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

    do
    {
      ++local.Counter.Count;

      switch(local.Counter.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "    EIN: " + (
            import.FcrEmployer.Ein ?? "");

          break;
        case 2:
          if (!Equal(import.FcrEmployer.Name, import.KaecsesEmployer.Name))
          {
            local.EabReportSend.RptDetail = "   NAME: " + (
              import.FcrEmployer.Name ?? "") + " ** " + (
                import.KaecsesEmployer.Name ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "   NAME: " + (
              import.FcrEmployer.Name ?? "") + "    " + (
                import.KaecsesEmployer.Name ?? "");
          }

          break;
        case 3:
          if (!Equal(import.FcrEmployerAddress.Street1,
            import.KaecsesDomesticEmployerAddress.Street1))
          {
            local.EabReportSend.RptDetail = "STREET1: " + (
              import.FcrEmployerAddress.Street1 ?? "") + "         ** " + (
                import.KaecsesDomesticEmployerAddress.Street1 ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "STREET1: " + (
              import.FcrEmployerAddress.Street1 ?? "") + "            " + (
                import.KaecsesDomesticEmployerAddress.Street1 ?? "");
          }

          break;
        case 4:
          if (!Equal(import.FcrEmployerAddress.Street2,
            import.KaecsesDomesticEmployerAddress.Street2))
          {
            local.EabReportSend.RptDetail = "STREET2: " + (
              import.FcrEmployerAddress.Street2 ?? "") + "         ** " + (
                import.KaecsesDomesticEmployerAddress.Street2 ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "STREET2: " + (
              import.FcrEmployerAddress.Street2 ?? "") + "            " + (
                import.KaecsesDomesticEmployerAddress.Street2 ?? "");
          }

          break;
        case 5:
          if (!Equal(import.FcrEmployerAddress.City,
            import.KaecsesDomesticEmployerAddress.City))
          {
            local.EabReportSend.RptDetail = "   CITY: " + (
              import.FcrEmployerAddress.City ?? "") + "                   ** " +
              (import.KaecsesDomesticEmployerAddress.City ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "   CITY: " + (
              import.FcrEmployerAddress.City ?? "") + "                      " +
              (import.KaecsesDomesticEmployerAddress.City ?? "");
          }

          break;
        case 6:
          if (!Equal(import.FcrEmployerAddress.State,
            import.KaecsesDomesticEmployerAddress.State))
          {
            local.EabReportSend.RptDetail = "  STATE: " + (
              import.FcrEmployerAddress.State ?? "") + "                                ** " +
              (import.KaecsesDomesticEmployerAddress.State ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "  STATE: " + (
              import.FcrEmployerAddress.State ?? "") + "                                   " +
              (import.KaecsesDomesticEmployerAddress.State ?? "");
          }

          break;
        case 7:
          if (!Equal(import.FcrEmployerAddress.ZipCode,
            import.KaecsesDomesticEmployerAddress.ZipCode))
          {
            local.EabReportSend.RptDetail = "ZIPCODE: " + (
              import.FcrEmployerAddress.ZipCode ?? "") + "                             ** " +
              (import.KaecsesDomesticEmployerAddress.ZipCode ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "ZIPCODE: " + (
              import.FcrEmployerAddress.ZipCode ?? "") + "                                " +
              (import.KaecsesDomesticEmployerAddress.ZipCode ?? "");
          }

          break;
        case 8:
          if (!Equal(import.FcrEmployerAddress.Zip4,
            import.KaecsesDomesticEmployerAddress.Zip4))
          {
            local.EabReportSend.RptDetail = "   ZIP4: " + (
              import.FcrEmployerAddress.Zip4 ?? "") + "                              ** " +
              (import.KaecsesDomesticEmployerAddress.Zip4 ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "   ZIP4: " + (
              import.FcrEmployerAddress.Zip4 ?? "") + "                                 " +
              (import.KaecsesDomesticEmployerAddress.Zip4 ?? "");
          }

          break;
        case 9:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabBusinessReport04();
    }
    while(local.Counter.Count <= 9);
  }

  private void UseCabBusinessReport04()
  {
    var useImport = new CabBusinessReport04.Import();
    var useExport = new CabBusinessReport04.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport04.Execute, useImport, useExport);

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
    /// A value of KaecsesEmployer.
    /// </summary>
    [JsonPropertyName("kaecsesEmployer")]
    public Employer KaecsesEmployer
    {
      get => kaecsesEmployer ??= new();
      set => kaecsesEmployer = value;
    }

    /// <summary>
    /// A value of KaecsesDomesticEmployerAddress.
    /// </summary>
    [JsonPropertyName("kaecsesDomesticEmployerAddress")]
    public EmployerAddress KaecsesDomesticEmployerAddress
    {
      get => kaecsesDomesticEmployerAddress ??= new();
      set => kaecsesDomesticEmployerAddress = value;
    }

    /// <summary>
    /// A value of FcrEmployer.
    /// </summary>
    [JsonPropertyName("fcrEmployer")]
    public Employer FcrEmployer
    {
      get => fcrEmployer ??= new();
      set => fcrEmployer = value;
    }

    /// <summary>
    /// A value of FcrEmployerAddress.
    /// </summary>
    [JsonPropertyName("fcrEmployerAddress")]
    public EmployerAddress FcrEmployerAddress
    {
      get => fcrEmployerAddress ??= new();
      set => fcrEmployerAddress = value;
    }

    private Employer kaecsesEmployer;
    private EmployerAddress kaecsesDomesticEmployerAddress;
    private Employer fcrEmployer;
    private EmployerAddress fcrEmployerAddress;
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

    private Common counter;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
