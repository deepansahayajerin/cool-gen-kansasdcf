// Program: FN_B793_WRITE_EXTRACT_FILE, ID: 1902420746, model: 746.
// Short name: SWEXFE42
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B793_WRITE_EXTRACT_FILE.
/// </summary>
[Serializable]
public partial class FnB793WriteExtractFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B793_WRITE_EXTRACT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB793WriteExtractFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB793WriteExtractFile.
  /// </summary>
  public FnB793WriteExtractFile(IContext context, Import import, Export export):
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
      "SWEXFE42", context, import, export, EabOptions.Hpvp);
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
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "ProcessDate" })]
    public ProgramProcessingInfo Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of DetailCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("detailCashReceiptDetail")]
    [Member(Index = 4, AccessFields = false, Members
      = new[] { "CollectionAmount", "CollectionDate" })]
    public CashReceiptDetail DetailCashReceiptDetail
    {
      get => detailCashReceiptDetail ??= new();
      set => detailCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of DetailCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("detailCsePersonsWorkSet")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "Number",
      "Dob",
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet DetailCsePersonsWorkSet
    {
      get => detailCsePersonsWorkSet ??= new();
      set => detailCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Footer.
    /// </summary>
    [JsonPropertyName("footer")]
    [Member(Index = 6, AccessFields = false, Members
      = new[] { "Count", "TotalCurrency" })]
    public Common Footer
    {
      get => footer ??= new();
      set => footer = value;
    }

    private EabFileHandling eabFileHandling;
    private TextWorkArea recordType;
    private ProgramProcessingInfo header;
    private CashReceiptDetail detailCashReceiptDetail;
    private CsePersonsWorkSet detailCsePersonsWorkSet;
    private Common footer;
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
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }
#endregion
}
