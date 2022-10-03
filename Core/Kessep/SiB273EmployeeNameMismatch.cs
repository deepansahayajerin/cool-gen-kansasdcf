// Program: SI_B273_EMPLOYEE_NAME_MISMATCH, ID: 371059587, model: 746.
// Short name: SWE01276
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_EMPLOYEE_NAME_MISMATCH.
/// </summary>
[Serializable]
public partial class SiB273EmployeeNameMismatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_EMPLOYEE_NAME_MISMATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273EmployeeNameMismatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273EmployeeNameMismatch.
  /// </summary>
  public SiB273EmployeeNameMismatch(IContext context, Import import,
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

    // ************ format fcr info *****************
    if (!IsEmpty(import.FederalCaseRegistry.MiddleInitial))
    {
      local.Fcr.FormattedName =
        TrimEnd(import.FederalCaseRegistry.FirstName) + " " + import
        .FederalCaseRegistry.MiddleInitial + " " + TrimEnd
        (import.FederalCaseRegistry.LastName);
    }
    else
    {
      local.Fcr.FormattedName =
        TrimEnd(import.FederalCaseRegistry.FirstName) + " " + "" + "" + TrimEnd
        (import.FederalCaseRegistry.LastName);
    }

    // ************ format kaecses info *****************
    if (!IsEmpty(import.Kaecses.MiddleInitial))
    {
      local.Kaecses.FormattedName = TrimEnd(import.Kaecses.FirstName) + " " + import
        .Kaecses.MiddleInitial + " " + TrimEnd(import.Kaecses.LastName);
    }
    else
    {
      local.Kaecses.FormattedName = TrimEnd(import.Kaecses.FirstName) + " " + ""
        + "" + TrimEnd(import.Kaecses.LastName);
    }

    local.EabReportSend.RptDetail = import.FederalCaseRegistry.Number + "  " + local
      .Fcr.FormattedName + "     " + local.Kaecses.FormattedName;
    UseCabBusinessReport03();
  }

  private void UseCabBusinessReport03()
  {
    var useImport = new CabBusinessReport03.Import();
    var useExport = new CabBusinessReport03.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport03.Execute, useImport, useExport);

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
    /// A value of Fcr.
    /// </summary>
    [JsonPropertyName("fcr")]
    public CsePersonsWorkSet Fcr
    {
      get => fcr ??= new();
      set => fcr = value;
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

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private CsePersonsWorkSet fcr;
    private CsePersonsWorkSet kaecses;
  }
#endregion
}
