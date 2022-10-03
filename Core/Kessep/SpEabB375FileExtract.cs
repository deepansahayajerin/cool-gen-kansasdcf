// Program: SP_EAB_B375_FILE_EXTRACT, ID: 373029337, model: 746.
// Short name: SWEX375W
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_EAB_B375_FILE_EXTRACT.
/// </summary>
[Serializable]
public partial class SpEabB375FileExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EAB_B375_FILE_EXTRACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEabB375FileExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEabB375FileExtract.
  /// </summary>
  public SpEabB375FileExtract(IContext context, Import import, Export export):
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
      "SWEX375W", context, import, export, EabOptions.Hpvp);
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

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Number" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Co.
    /// </summary>
    [JsonPropertyName("co")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "FormattedName" })
      ]
    public CsePersonsWorkSet Co
    {
      get => co ??= new();
      set => co = value;
    }

    /// <summary>
    /// A value of Ss.
    /// </summary>
    [JsonPropertyName("ss")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "FormattedName" })
      ]
    public CsePersonsWorkSet Ss
    {
      get => ss ??= new();
      set => ss = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Name" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "FormattedName" })
      ]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Review.
    /// </summary>
    [JsonPropertyName("review")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "TextDate" })]
    public DateWorkArea Review
    {
      get => review ??= new();
      set => review = value;
    }

    private Common common;
    private Case1 case1;
    private CsePersonsWorkSet co;
    private CsePersonsWorkSet ss;
    private Office office;
    private CsePersonsWorkSet ap;
    private DateWorkArea review;
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
