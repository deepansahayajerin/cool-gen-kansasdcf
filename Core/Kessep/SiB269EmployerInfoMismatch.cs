// Program: SI_B269_EMPLOYER_INFO_MISMATCH, ID: 373411375, model: 746.
// Short name: SWE02627
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B269_EMPLOYER_INFO_MISMATCH.
/// </summary>
[Serializable]
public partial class SiB269EmployerInfoMismatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B269_EMPLOYER_INFO_MISMATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB269EmployerInfoMismatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB269EmployerInfoMismatch.
  /// </summary>
  public SiB269EmployerInfoMismatch(IContext context, Import import,
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
      // ******************** DO REPORT 04 ********************
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
            import.KaecsesEmployerAddress.Street1))
          {
            local.EabReportSend.RptDetail = "STREET1: " + (
              import.FcrEmployerAddress.Street1 ?? "") + "         ** " + (
                import.KaecsesEmployerAddress.Street1 ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "STREET1: " + (
              import.FcrEmployerAddress.Street1 ?? "") + "            " + (
                import.KaecsesEmployerAddress.Street1 ?? "");
          }

          break;
        case 4:
          if (!Equal(import.FcrEmployerAddress.Street2,
            import.KaecsesEmployerAddress.Street2))
          {
            local.EabReportSend.RptDetail = "STREET2: " + (
              import.FcrEmployerAddress.Street2 ?? "") + "         ** " + (
                import.KaecsesEmployerAddress.Street2 ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "STREET2: " + (
              import.FcrEmployerAddress.Street2 ?? "") + "            " + (
                import.KaecsesEmployerAddress.Street2 ?? "");
          }

          break;
        case 5:
          if (!Equal(import.FcrEmployerAddress.City,
            import.KaecsesEmployerAddress.City))
          {
            local.EabReportSend.RptDetail = "   CITY: " + (
              import.FcrEmployerAddress.City ?? "") + "                   ** " +
              (import.KaecsesEmployerAddress.City ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "   CITY: " + (
              import.FcrEmployerAddress.City ?? "") + "                      " +
              (import.KaecsesEmployerAddress.City ?? "");
          }

          break;
        case 6:
          if (!Equal(import.FcrEmployerAddress.State,
            import.KaecsesEmployerAddress.State))
          {
            local.EabReportSend.RptDetail = "  STATE: " + (
              import.FcrEmployerAddress.State ?? "") + "                                ** " +
              (import.KaecsesEmployerAddress.State ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "  STATE: " + (
              import.FcrEmployerAddress.State ?? "") + "                                   " +
              (import.KaecsesEmployerAddress.State ?? "");
          }

          break;
        case 7:
          if (!Equal(import.FcrEmployerAddress.ZipCode,
            import.KaecsesEmployerAddress.ZipCode))
          {
            local.EabReportSend.RptDetail = "ZIPCODE: " + (
              import.FcrEmployerAddress.ZipCode ?? "") + "                             ** " +
              (import.KaecsesEmployerAddress.ZipCode ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "ZIPCODE: " + (
              import.FcrEmployerAddress.ZipCode ?? "") + "                                " +
              (import.KaecsesEmployerAddress.ZipCode ?? "");
          }

          break;
        case 8:
          if (!Equal(import.FcrEmployerAddress.Zip4,
            import.KaecsesEmployerAddress.Zip4))
          {
            local.EabReportSend.RptDetail = "   ZIP4: " + (
              import.FcrEmployerAddress.Zip4 ?? "") + "                              ** " +
              (import.KaecsesEmployerAddress.Zip4 ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "   ZIP4: " + (
              import.FcrEmployerAddress.Zip4 ?? "") + "                                 " +
              (import.KaecsesEmployerAddress.Zip4 ?? "");
          }

          break;
        case 9:
          local.EabReportSend.RptDetail = "   NOTE: " + "    " + "                                 " +
            (import.KaecsesEmployerAddress.Note ?? "");

          break;
        case 10:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabBusinessReport04();
    }
    while(local.Counter.Count <= 9);

    // *******************************************************************************************
    // Check to see if address 1 or 2 contains a number.  If no number then 
    // write to report5.
    // Either way, the data base will be updated with the new address.
    // *******************************************************************************************
    local.Counter.Count = Verify("0", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("1", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("2", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("3", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("4", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("5", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("6", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("7", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("8", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("9", import.FcrEmployerAddress.Street1);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("0", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("1", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("2", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("3", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("4", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("5", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("6", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("7", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("8", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = Verify("9", import.FcrEmployerAddress.Street2);

    if (local.Counter.Count == 0)
    {
      return;
    }

    local.Counter.Count = 0;

    do
    {
      // ******************** DO REPORT 05 ********************
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
            import.KaecsesEmployerAddress.Street1))
          {
            local.EabReportSend.RptDetail = "STREET1: " + (
              import.FcrEmployerAddress.Street1 ?? "") + "         ** " + (
                import.KaecsesEmployerAddress.Street1 ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "STREET1: " + (
              import.FcrEmployerAddress.Street1 ?? "") + "            " + (
                import.KaecsesEmployerAddress.Street1 ?? "");
          }

          break;
        case 4:
          if (!Equal(import.FcrEmployerAddress.Street2,
            import.KaecsesEmployerAddress.Street2))
          {
            local.EabReportSend.RptDetail = "STREET2: " + (
              import.FcrEmployerAddress.Street2 ?? "") + "         ** " + (
                import.KaecsesEmployerAddress.Street2 ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "STREET2: " + (
              import.FcrEmployerAddress.Street2 ?? "") + "            " + (
                import.KaecsesEmployerAddress.Street2 ?? "");
          }

          break;
        case 5:
          if (!Equal(import.FcrEmployerAddress.City,
            import.KaecsesEmployerAddress.City))
          {
            local.EabReportSend.RptDetail = "   CITY: " + (
              import.FcrEmployerAddress.City ?? "") + "                   ** " +
              (import.KaecsesEmployerAddress.City ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "   CITY: " + (
              import.FcrEmployerAddress.City ?? "") + "                      " +
              (import.KaecsesEmployerAddress.City ?? "");
          }

          break;
        case 6:
          if (!Equal(import.FcrEmployerAddress.State,
            import.KaecsesEmployerAddress.State))
          {
            local.EabReportSend.RptDetail = "  STATE: " + (
              import.FcrEmployerAddress.State ?? "") + "                                ** " +
              (import.KaecsesEmployerAddress.State ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "  STATE: " + (
              import.FcrEmployerAddress.State ?? "") + "                                   " +
              (import.KaecsesEmployerAddress.State ?? "");
          }

          break;
        case 7:
          if (!Equal(import.FcrEmployerAddress.ZipCode,
            import.KaecsesEmployerAddress.ZipCode))
          {
            local.EabReportSend.RptDetail = "ZIPCODE: " + (
              import.FcrEmployerAddress.ZipCode ?? "") + "                             ** " +
              (import.KaecsesEmployerAddress.ZipCode ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "ZIPCODE: " + (
              import.FcrEmployerAddress.ZipCode ?? "") + "                                " +
              (import.KaecsesEmployerAddress.ZipCode ?? "");
          }

          break;
        case 8:
          if (!Equal(import.FcrEmployerAddress.Zip4,
            import.KaecsesEmployerAddress.Zip4))
          {
            local.EabReportSend.RptDetail = "   ZIP4: " + (
              import.FcrEmployerAddress.Zip4 ?? "") + "                              ** " +
              (import.KaecsesEmployerAddress.Zip4 ?? "");
          }
          else
          {
            local.EabReportSend.RptDetail = "   ZIP4: " + (
              import.FcrEmployerAddress.Zip4 ?? "") + "                                 " +
              (import.KaecsesEmployerAddress.Zip4 ?? "");
          }

          break;
        case 9:
          local.EabReportSend.RptDetail = "";

          break;
        case 10:
          local.EabReportSend.RptDetail = "";

          break;
        default:
          break;
      }

      UseCabBusinessReport05();
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

  private void UseCabBusinessReport05()
  {
    var useImport = new CabBusinessReport05.Import();
    var useExport = new CabBusinessReport05.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport05.Execute, useImport, useExport);

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
    /// A value of KaecsesEmployerAddress.
    /// </summary>
    [JsonPropertyName("kaecsesEmployerAddress")]
    public EmployerAddress KaecsesEmployerAddress
    {
      get => kaecsesEmployerAddress ??= new();
      set => kaecsesEmployerAddress = value;
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

    /// <summary>
    /// A value of KaecsesEmployer.
    /// </summary>
    [JsonPropertyName("kaecsesEmployer")]
    public Employer KaecsesEmployer
    {
      get => kaecsesEmployer ??= new();
      set => kaecsesEmployer = value;
    }

    private EmployerAddress kaecsesEmployerAddress;
    private Employer fcrEmployer;
    private EmployerAddress fcrEmployerAddress;
    private Employer kaecsesEmployer;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private EabFileHandling eabFileHandling;
    private Common counter;
    private EabReportSend eabReportSend;
  }
#endregion
}
