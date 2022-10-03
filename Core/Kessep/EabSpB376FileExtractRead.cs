// Program: EAB_SP_B376_FILE_EXTRACT_READ, ID: 371318336, model: 746.
// Short name: SWEXPE10
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_SP_B376_FILE_EXTRACT_READ.
/// </para>
/// <para>
/// This EAB reads data from the flat file and passes it back to the p-step .
/// </para>
/// </summary>
[Serializable]
public partial class EabSpB376FileExtractRead: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_SP_B376_FILE_EXTRACT_READ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabSpB376FileExtractRead(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabSpB376FileExtractRead.
  /// </summary>
  public EabSpB376FileExtractRead(IContext context, Import import, Export export)
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
      "SWEXPE10", context, import, export, EabOptions.Hpvp);
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "ActionEntry" })]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
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

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "SystemGeneratedId", "Name" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Ss.
    /// </summary>
    [JsonPropertyName("ss")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public CsePersonsWorkSet Ss
    {
      get => ss ??= new();
      set => ss = value;
    }

    /// <summary>
    /// A value of Co.
    /// </summary>
    [JsonPropertyName("co")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public CsePersonsWorkSet Co
    {
      get => co ??= new();
      set => co = value;
    }

    /// <summary>
    /// A value of Review.
    /// </summary>
    [JsonPropertyName("review")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea Review
    {
      get => review ??= new();
      set => review = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    [Member(Index = 7, AccessFields = false, Members
      = new[] { "Number", "CseOpenDate" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of DelinquentOrNever.
    /// </summary>
    [JsonPropertyName("delinquentOrNever")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Flag" })]
    public Common DelinquentOrNever
    {
      get => delinquentOrNever ??= new();
      set => delinquentOrNever = value;
    }

    /// <summary>
    /// A value of OfficeNumber.
    /// </summary>
    [JsonPropertyName("officeNumber")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Text4" })]
    public TextWorkArea OfficeNumber
    {
      get => officeNumber ??= new();
      set => officeNumber = value;
    }

    /// <summary>
    /// A value of DelinDateText.
    /// </summary>
    [JsonPropertyName("delinDateText")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "TextDate" })]
    public DateWorkArea DelinDateText
    {
      get => delinDateText ??= new();
      set => delinDateText = value;
    }

    private EabFileHandling eabFileHandling;
    private Office office;
    private CsePersonsWorkSet ss;
    private CsePersonsWorkSet co;
    private DateWorkArea review;
    private CsePersonsWorkSet ap;
    private Case1 case1;
    private Common delinquentOrNever;
    private TextWorkArea officeNumber;
    private DateWorkArea delinDateText;
  }
#endregion
}
