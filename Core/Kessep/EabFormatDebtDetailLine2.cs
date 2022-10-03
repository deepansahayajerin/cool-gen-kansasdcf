// Program: EAB_FORMAT_DEBT_DETAIL_LINE_2, ID: 372117070, model: 746.
// Short name: SWEXFW11
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_FORMAT_DEBT_DETAIL_LINE_2.
/// </summary>
[Serializable]
public partial class EabFormatDebtDetailLine2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_FORMAT_DEBT_DETAIL_LINE_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabFormatDebtDetailLine2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabFormatDebtDetailLine2.
  /// </summary>
  public EabFormatDebtDetailLine2(IContext context, Import import, Export export)
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
      "SWEXFW11", context, import, export, EabOptions.NoIefParams);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 1, Members = new[] { "Number", "FormattedName" })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    [Member(Index = 2, Members = new[] { "UserId" })]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    [Member(Index = 3, Members = new[] { "Number" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    [Member(Index = 4, Members
      = new[] { "SystemGeneratedIdentifier", "OrderTypeCode" })]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 5, Members = new[]
    {
      "Identifier",
      "CourtCaseNumber",
      "StandardNumber"
    })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private Obligation obligation;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ListScreenWorkArea.
    /// </summary>
    [JsonPropertyName("listScreenWorkArea")]
    [Member(Index = 1, Members = new[] { "TextLine76" })]
    public ListScreenWorkArea ListScreenWorkArea
    {
      get => listScreenWorkArea ??= new();
      set => listScreenWorkArea = value;
    }

    private ListScreenWorkArea listScreenWorkArea;
  }
#endregion
}
