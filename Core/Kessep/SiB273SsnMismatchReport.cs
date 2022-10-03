// Program: SI_B273_SSN_MISMATCH_REPORT, ID: 371060451, model: 746.
// Short name: SWE01278
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_SSN_MISMATCH_REPORT.
/// </summary>
[Serializable]
public partial class SiB273SsnMismatchReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_SSN_MISMATCH_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273SsnMismatchReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273SsnMismatchReport.
  /// </summary>
  public SiB273SsnMismatchReport(IContext context, Import import, Export export):
    
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

    // ************ format fcr info *****************
    local.FcrCsePersonsWorkSet.FormattedName =
      TrimEnd(import.FederalCaseRegistry.FirstName) + " " + import
      .FederalCaseRegistry.MiddleInitial + " " + TrimEnd
      (import.FederalCaseRegistry.LastName);
    local.FcrWorkArea.Text11 =
      Substring(import.FederalCaseRegistry.Ssn, CsePersonsWorkSet.Ssn_MaxLength,
      1, 3) + "-" + Substring
      (import.FederalCaseRegistry.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 4, 2) +
      "-" + Substring
      (import.FederalCaseRegistry.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4);

    // ************ format kaecses info *****************
    local.KaecsesCsePersonsWorkSet.FormattedName =
      TrimEnd(import.Kaecses.FirstName) + " " + import.Kaecses.MiddleInitial + " " +
      TrimEnd(import.Kaecses.LastName);
    local.KaecsesWorkArea.Text11 =
      Substring(import.Kaecses.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 1, 3) + "-"
      + Substring(import.Kaecses.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 4, 2) + "-"
      + Substring(import.Kaecses.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4);
    local.EabReportSend.RptDetail = local.FcrCsePersonsWorkSet.FormattedName + " " +
      local.FcrWorkArea.Text11 + " " + local.KaecsesWorkArea.Text11 + " " + local
      .KaecsesCsePersonsWorkSet.FormattedName;
    UseCabBusinessReport02();
  }

  private void UseCabBusinessReport02()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport02.Execute, useImport, useExport);

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
    /// A value of Kaecses.
    /// </summary>
    [JsonPropertyName("kaecses")]
    public CsePersonsWorkSet Kaecses
    {
      get => kaecses ??= new();
      set => kaecses = value;
    }

    /// <summary>
    /// A value of FederalCaseRegistry.
    /// </summary>
    [JsonPropertyName("federalCaseRegistry")]
    public CsePersonsWorkSet FederalCaseRegistry
    {
      get => federalCaseRegistry ??= new();
      set => federalCaseRegistry = value;
    }

    private CsePersonsWorkSet kaecses;
    private CsePersonsWorkSet federalCaseRegistry;
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
    /// A value of KaecsesWorkArea.
    /// </summary>
    [JsonPropertyName("kaecsesWorkArea")]
    public WorkArea KaecsesWorkArea
    {
      get => kaecsesWorkArea ??= new();
      set => kaecsesWorkArea = value;
    }

    /// <summary>
    /// A value of FcrWorkArea.
    /// </summary>
    [JsonPropertyName("fcrWorkArea")]
    public WorkArea FcrWorkArea
    {
      get => fcrWorkArea ??= new();
      set => fcrWorkArea = value;
    }

    /// <summary>
    /// A value of KaecsesCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("kaecsesCsePersonsWorkSet")]
    public CsePersonsWorkSet KaecsesCsePersonsWorkSet
    {
      get => kaecsesCsePersonsWorkSet ??= new();
      set => kaecsesCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FcrCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fcrCsePersonsWorkSet")]
    public CsePersonsWorkSet FcrCsePersonsWorkSet
    {
      get => fcrCsePersonsWorkSet ??= new();
      set => fcrCsePersonsWorkSet = value;
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

    private WorkArea kaecsesWorkArea;
    private WorkArea fcrWorkArea;
    private CsePersonsWorkSet kaecsesCsePersonsWorkSet;
    private CsePersonsWorkSet fcrCsePersonsWorkSet;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }
#endregion
}
