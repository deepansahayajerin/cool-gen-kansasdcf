// Program: EAB_READ_AP_STMTS_VENDOR_FILE, ID: 372994995, model: 746.
// Short name: SWEXFE47
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_AP_STMTS_VENDOR_FILE.
/// </summary>
[Serializable]
public partial class EabReadApStmtsVendorFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_AP_STMTS_VENDOR_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadApStmtsVendorFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadApStmtsVendorFile.
  /// </summary>
  public EabReadApStmtsVendorFile(IContext context, Import import, Export export)
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
    GetService<IEabStub>().Execute(
      "SWEXFE47", context, import, export, EabOptions.Hpvp);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of StatementNumber.
    /// </summary>
    [JsonPropertyName("statementNumber")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text4" })]
    public TextWorkArea StatementNumber
    {
      get => statementNumber ??= new();
      set => statementNumber = value;
    }

    private EabFileHandling eabFileHandling;
    private CsePerson csePerson;
    private TextWorkArea statementNumber;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of StatementNumber.
    /// </summary>
    [JsonPropertyName("statementNumber")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text4" })]
    public TextWorkArea StatementNumber
    {
      get => statementNumber ??= new();
      set => statementNumber = value;
    }

    /// <summary>
    /// A value of StmtMonth.
    /// </summary>
    [JsonPropertyName("stmtMonth")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text10" })]
    public TextWorkArea StmtMonth
    {
      get => stmtMonth ??= new();
      set => stmtMonth = value;
    }

    /// <summary>
    /// A value of StmtYear.
    /// </summary>
    [JsonPropertyName("stmtYear")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text4" })]
    public TextWorkArea StmtYear
    {
      get => stmtYear ??= new();
      set => stmtYear = value;
    }

    /// <summary>
    /// A value of Coupons.
    /// </summary>
    [JsonPropertyName("coupons")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Flag" })]
    public Common Coupons
    {
      get => coupons ??= new();
      set => coupons = value;
    }

    /// <summary>
    /// A value of CourtCase.
    /// </summary>
    [JsonPropertyName("courtCase")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea CourtCase
    {
      get => courtCase ??= new();
      set => courtCase = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private CsePerson csePerson;
    private TextWorkArea statementNumber;
    private TextWorkArea stmtMonth;
    private TextWorkArea stmtYear;
    private Common coupons;
    private TextWorkArea courtCase;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
