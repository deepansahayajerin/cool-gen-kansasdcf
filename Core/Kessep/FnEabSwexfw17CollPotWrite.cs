// Program: FN_EAB_SWEXFW17_COLL_POT_WRITE, ID: 373454864, model: 746.
// Short name: SWEXFW17
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EAB_SWEXFW17_COLL_POT_WRITE.
/// </summary>
[Serializable]
public partial class FnEabSwexfw17CollPotWrite: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EAB_SWEXFW17_COLL_POT_WRITE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEabSwexfw17CollPotWrite(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEabSwexfw17CollPotWrite.
  /// </summary>
  public FnEabSwexfw17CollPotWrite(IContext context, Import import,
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
      "SWEXFW17", context, import, export, EabOptions.Hpvp);
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
    /// A value of TotalOrdersReferred.
    /// </summary>
    [JsonPropertyName("totalOrdersReferred")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Count" })]
    public Common TotalOrdersReferred
    {
      get => totalOrdersReferred ??= new();
      set => totalOrdersReferred = value;
    }

    /// <summary>
    /// A value of NumberOfPayingOrders.
    /// </summary>
    [JsonPropertyName("numberOfPayingOrders")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Count" })]
    public Common NumberOfPayingOrders
    {
      get => numberOfPayingOrders ??= new();
      set => numberOfPayingOrders = value;
    }

    /// <summary>
    /// A value of OrdersInLocate.
    /// </summary>
    [JsonPropertyName("ordersInLocate")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Count" })]
    public Common OrdersInLocate
    {
      get => ordersInLocate ??= new();
      set => ordersInLocate = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "StateAbbreviation",
      "State",
      "CountyAbbreviation",
      "County"
    })]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    [Member(Index = 6, AccessFields = false, Members
      = new[] { "JudicialDistrict", "Name" })]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 7, AccessFields = false, Members
      = new[] { "StandardNumber" })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ReferralType.
    /// </summary>
    [JsonPropertyName("referralType")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Text2" })]
    public TextWorkArea ReferralType
    {
      get => referralType ??= new();
      set => referralType = value;
    }

    /// <summary>
    /// A value of CollCurrentState.
    /// </summary>
    [JsonPropertyName("collCurrentState")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "TotalCurrency" })
      ]
    public Common CollCurrentState
    {
      get => collCurrentState ??= new();
      set => collCurrentState = value;
    }

    /// <summary>
    /// A value of CollCurrentFamily.
    /// </summary>
    [JsonPropertyName("collCurrentFamily")]
    [Member(Index = 10, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollCurrentFamily
    {
      get => collCurrentFamily ??= new();
      set => collCurrentFamily = value;
    }

    /// <summary>
    /// A value of CollCurrentIState.
    /// </summary>
    [JsonPropertyName("collCurrentIState")]
    [Member(Index = 11, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollCurrentIState
    {
      get => collCurrentIState ??= new();
      set => collCurrentIState = value;
    }

    /// <summary>
    /// A value of CollCurrentIFamily.
    /// </summary>
    [JsonPropertyName("collCurrentIFamily")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollCurrentIFamily
    {
      get => collCurrentIFamily ??= new();
      set => collCurrentIFamily = value;
    }

    /// <summary>
    /// A value of CollArrearsState.
    /// </summary>
    [JsonPropertyName("collArrearsState")]
    [Member(Index = 13, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollArrearsState
    {
      get => collArrearsState ??= new();
      set => collArrearsState = value;
    }

    /// <summary>
    /// A value of CollArrearsFamily.
    /// </summary>
    [JsonPropertyName("collArrearsFamily")]
    [Member(Index = 14, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollArrearsFamily
    {
      get => collArrearsFamily ??= new();
      set => collArrearsFamily = value;
    }

    /// <summary>
    /// A value of CollArrearsIState.
    /// </summary>
    [JsonPropertyName("collArrearsIState")]
    [Member(Index = 15, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollArrearsIState
    {
      get => collArrearsIState ??= new();
      set => collArrearsIState = value;
    }

    /// <summary>
    /// A value of CollArrearsIFamily.
    /// </summary>
    [JsonPropertyName("collArrearsIFamily")]
    [Member(Index = 16, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollArrearsIFamily
    {
      get => collArrearsIFamily ??= new();
      set => collArrearsIFamily = value;
    }

    /// <summary>
    /// A value of OwedCurrentState.
    /// </summary>
    [JsonPropertyName("owedCurrentState")]
    [Member(Index = 17, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedCurrentState
    {
      get => owedCurrentState ??= new();
      set => owedCurrentState = value;
    }

    /// <summary>
    /// A value of OwedCurrentFamily.
    /// </summary>
    [JsonPropertyName("owedCurrentFamily")]
    [Member(Index = 18, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedCurrentFamily
    {
      get => owedCurrentFamily ??= new();
      set => owedCurrentFamily = value;
    }

    /// <summary>
    /// A value of OwedCurrentIState.
    /// </summary>
    [JsonPropertyName("owedCurrentIState")]
    [Member(Index = 19, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedCurrentIState
    {
      get => owedCurrentIState ??= new();
      set => owedCurrentIState = value;
    }

    /// <summary>
    /// A value of OwedCurrentIFamily.
    /// </summary>
    [JsonPropertyName("owedCurrentIFamily")]
    [Member(Index = 20, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedCurrentIFamily
    {
      get => owedCurrentIFamily ??= new();
      set => owedCurrentIFamily = value;
    }

    /// <summary>
    /// A value of OwedArrearsState.
    /// </summary>
    [JsonPropertyName("owedArrearsState")]
    [Member(Index = 21, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedArrearsState
    {
      get => owedArrearsState ??= new();
      set => owedArrearsState = value;
    }

    /// <summary>
    /// A value of OwedArrearsFamily.
    /// </summary>
    [JsonPropertyName("owedArrearsFamily")]
    [Member(Index = 22, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedArrearsFamily
    {
      get => owedArrearsFamily ??= new();
      set => owedArrearsFamily = value;
    }

    /// <summary>
    /// A value of OwedArrearsIState.
    /// </summary>
    [JsonPropertyName("owedArrearsIState")]
    [Member(Index = 23, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedArrearsIState
    {
      get => owedArrearsIState ??= new();
      set => owedArrearsIState = value;
    }

    /// <summary>
    /// A value of OwedArrearsIFamily.
    /// </summary>
    [JsonPropertyName("owedArrearsIFamily")]
    [Member(Index = 24, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedArrearsIFamily
    {
      get => owedArrearsIFamily ??= new();
      set => owedArrearsIFamily = value;
    }

    /// <summary>
    /// A value of OwedCurrTotal.
    /// </summary>
    [JsonPropertyName("owedCurrTotal")]
    [Member(Index = 25, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedCurrTotal
    {
      get => owedCurrTotal ??= new();
      set => owedCurrTotal = value;
    }

    /// <summary>
    /// A value of OwedArrearsTotal.
    /// </summary>
    [JsonPropertyName("owedArrearsTotal")]
    [Member(Index = 26, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedArrearsTotal
    {
      get => owedArrearsTotal ??= new();
      set => owedArrearsTotal = value;
    }

    /// <summary>
    /// A value of OwedTotal.
    /// </summary>
    [JsonPropertyName("owedTotal")]
    [Member(Index = 27, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common OwedTotal
    {
      get => owedTotal ??= new();
      set => owedTotal = value;
    }

    /// <summary>
    /// A value of CollCurrTotal.
    /// </summary>
    [JsonPropertyName("collCurrTotal")]
    [Member(Index = 28, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollCurrTotal
    {
      get => collCurrTotal ??= new();
      set => collCurrTotal = value;
    }

    /// <summary>
    /// A value of CollArrearsTotal.
    /// </summary>
    [JsonPropertyName("collArrearsTotal")]
    [Member(Index = 29, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollArrearsTotal
    {
      get => collArrearsTotal ??= new();
      set => collArrearsTotal = value;
    }

    /// <summary>
    /// A value of CollectedTotal.
    /// </summary>
    [JsonPropertyName("collectedTotal")]
    [Member(Index = 30, AccessFields = false, Members
      = new[] { "TotalCurrency" })]
    public Common CollectedTotal
    {
      get => collectedTotal ??= new();
      set => collectedTotal = value;
    }

    private EabFileHandling eabFileHandling;
    private Common totalOrdersReferred;
    private Common numberOfPayingOrders;
    private Common ordersInLocate;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private TextWorkArea referralType;
    private Common collCurrentState;
    private Common collCurrentFamily;
    private Common collCurrentIState;
    private Common collCurrentIFamily;
    private Common collArrearsState;
    private Common collArrearsFamily;
    private Common collArrearsIState;
    private Common collArrearsIFamily;
    private Common owedCurrentState;
    private Common owedCurrentFamily;
    private Common owedCurrentIState;
    private Common owedCurrentIFamily;
    private Common owedArrearsState;
    private Common owedArrearsFamily;
    private Common owedArrearsIState;
    private Common owedArrearsIFamily;
    private Common owedCurrTotal;
    private Common owedArrearsTotal;
    private Common owedTotal;
    private Common collCurrTotal;
    private Common collArrearsTotal;
    private Common collectedTotal;
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
