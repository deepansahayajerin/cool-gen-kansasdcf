// Program: EAB_READ_FEDERAL_DEBT_SETOFF_LST, ID: 371264486, model: 746.
// Short name: SWEXFE80
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_FEDERAL_DEBT_SETOFF_LST.
/// </summary>
[Serializable]
public partial class EabReadFederalDebtSetoffLst: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_FEDERAL_DEBT_SETOFF_LST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadFederalDebtSetoffLst(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadFederalDebtSetoffLst.
  /// </summary>
  public EabReadFederalDebtSetoffLst(IContext context, Import import,
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
    GetService<IEabStub>().Execute(
      "SWEXFE80", context, import, export, EabOptions.Hpvp);
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

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InjuredSpouseIndicator.
    /// </summary>
    [JsonPropertyName("injuredSpouseIndicator")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea InjuredSpouseIndicator
    {
      get => injuredSpouseIndicator ??= new();
      set => injuredSpouseIndicator = value;
    }

    /// <summary>
    /// A value of ReturnIndicator.
    /// </summary>
    [JsonPropertyName("returnIndicator")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea ReturnIndicator
    {
      get => returnIndicator ??= new();
      set => returnIndicator = value;
    }

    /// <summary>
    /// A value of CollectionAmount.
    /// </summary>
    [JsonPropertyName("collectionAmount")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "AverageCurrency" })]
    public Common CollectionAmount
    {
      get => collectionAmount ??= new();
      set => collectionAmount = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 4, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private TextWorkArea injuredSpouseIndicator;
    private TextWorkArea returnIndicator;
    private Common collectionAmount;
    private CsePerson csePerson;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
