// Program: FN_EAB_SWEXFR17_COLL_POT_READ, ID: 373457551, model: 746.
// Short name: SWEXFR17
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EAB_SWEXFR17_COLL_POT_READ.
/// </summary>
[Serializable]
public partial class FnEabSwexfr17CollPotRead: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EAB_SWEXFR17_COLL_POT_READ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEabSwexfr17CollPotRead(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEabSwexfr17CollPotRead.
  /// </summary>
  public FnEabSwexfr17CollPotRead(IContext context, Import import, Export export)
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
      "SWEXFR17", context, import, export, EabOptions.Hpvp);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of EtotalOrdersReferred.
      /// </summary>
      [JsonPropertyName("etotalOrdersReferred")]
      [Member(Index = 1, AccessFields = false, Members = new[] { "Count" })]
      public Common EtotalOrdersReferred
      {
        get => etotalOrdersReferred ??= new();
        set => etotalOrdersReferred = value;
      }

      /// <summary>
      /// A value of EnumberOfPayingOrders.
      /// </summary>
      [JsonPropertyName("enumberOfPayingOrders")]
      [Member(Index = 2, AccessFields = false, Members = new[] { "Count" })]
      public Common EnumberOfPayingOrders
      {
        get => enumberOfPayingOrders ??= new();
        set => enumberOfPayingOrders = value;
      }

      /// <summary>
      /// A value of EordersInLocate.
      /// </summary>
      [JsonPropertyName("eordersInLocate")]
      [Member(Index = 3, AccessFields = false, Members = new[] { "Count" })]
      public Common EordersInLocate
      {
        get => eordersInLocate ??= new();
        set => eordersInLocate = value;
      }

      /// <summary>
      /// A value of Fips.
      /// </summary>
      [JsonPropertyName("fips")]
      [Member(Index = 4, AccessFields = false, Members = new[]
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
      [Member(Index = 5, AccessFields = false, Members
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
      [Member(Index = 6, AccessFields = false, Members
        = new[] { "StandardNumber" })]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>
      /// A value of EreferralType.
      /// </summary>
      [JsonPropertyName("ereferralType")]
      [Member(Index = 7, AccessFields = false, Members = new[] { "Text2" })]
      public TextWorkArea EreferralType
      {
        get => ereferralType ??= new();
        set => ereferralType = value;
      }

      /// <summary>
      /// A value of EcollCurrentState.
      /// </summary>
      [JsonPropertyName("ecollCurrentState")]
      [Member(Index = 8, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollCurrentState
      {
        get => ecollCurrentState ??= new();
        set => ecollCurrentState = value;
      }

      /// <summary>
      /// A value of EcollCurrentFamily.
      /// </summary>
      [JsonPropertyName("ecollCurrentFamily")]
      [Member(Index = 9, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollCurrentFamily
      {
        get => ecollCurrentFamily ??= new();
        set => ecollCurrentFamily = value;
      }

      /// <summary>
      /// A value of EcollCurrentIState.
      /// </summary>
      [JsonPropertyName("ecollCurrentIState")]
      [Member(Index = 10, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollCurrentIState
      {
        get => ecollCurrentIState ??= new();
        set => ecollCurrentIState = value;
      }

      /// <summary>
      /// A value of EcollCurrentIFamily.
      /// </summary>
      [JsonPropertyName("ecollCurrentIFamily")]
      [Member(Index = 11, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollCurrentIFamily
      {
        get => ecollCurrentIFamily ??= new();
        set => ecollCurrentIFamily = value;
      }

      /// <summary>
      /// A value of EcollArrearsState.
      /// </summary>
      [JsonPropertyName("ecollArrearsState")]
      [Member(Index = 12, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollArrearsState
      {
        get => ecollArrearsState ??= new();
        set => ecollArrearsState = value;
      }

      /// <summary>
      /// A value of EcollArrearsFamily.
      /// </summary>
      [JsonPropertyName("ecollArrearsFamily")]
      [Member(Index = 13, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollArrearsFamily
      {
        get => ecollArrearsFamily ??= new();
        set => ecollArrearsFamily = value;
      }

      /// <summary>
      /// A value of EcollArrearsIState.
      /// </summary>
      [JsonPropertyName("ecollArrearsIState")]
      [Member(Index = 14, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollArrearsIState
      {
        get => ecollArrearsIState ??= new();
        set => ecollArrearsIState = value;
      }

      /// <summary>
      /// A value of EcollArrearsIFamily.
      /// </summary>
      [JsonPropertyName("ecollArrearsIFamily")]
      [Member(Index = 15, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollArrearsIFamily
      {
        get => ecollArrearsIFamily ??= new();
        set => ecollArrearsIFamily = value;
      }

      /// <summary>
      /// A value of EowedCurrentState.
      /// </summary>
      [JsonPropertyName("eowedCurrentState")]
      [Member(Index = 16, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedCurrentState
      {
        get => eowedCurrentState ??= new();
        set => eowedCurrentState = value;
      }

      /// <summary>
      /// A value of EowedCurrentFamily.
      /// </summary>
      [JsonPropertyName("eowedCurrentFamily")]
      [Member(Index = 17, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedCurrentFamily
      {
        get => eowedCurrentFamily ??= new();
        set => eowedCurrentFamily = value;
      }

      /// <summary>
      /// A value of EowedCurrentIState.
      /// </summary>
      [JsonPropertyName("eowedCurrentIState")]
      [Member(Index = 18, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedCurrentIState
      {
        get => eowedCurrentIState ??= new();
        set => eowedCurrentIState = value;
      }

      /// <summary>
      /// A value of EowedCurrentIFamily.
      /// </summary>
      [JsonPropertyName("eowedCurrentIFamily")]
      [Member(Index = 19, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedCurrentIFamily
      {
        get => eowedCurrentIFamily ??= new();
        set => eowedCurrentIFamily = value;
      }

      /// <summary>
      /// A value of EowedArrearsState.
      /// </summary>
      [JsonPropertyName("eowedArrearsState")]
      [Member(Index = 20, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedArrearsState
      {
        get => eowedArrearsState ??= new();
        set => eowedArrearsState = value;
      }

      /// <summary>
      /// A value of EowedArrearsFamily.
      /// </summary>
      [JsonPropertyName("eowedArrearsFamily")]
      [Member(Index = 21, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedArrearsFamily
      {
        get => eowedArrearsFamily ??= new();
        set => eowedArrearsFamily = value;
      }

      /// <summary>
      /// A value of EowedArrearsIState.
      /// </summary>
      [JsonPropertyName("eowedArrearsIState")]
      [Member(Index = 22, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedArrearsIState
      {
        get => eowedArrearsIState ??= new();
        set => eowedArrearsIState = value;
      }

      /// <summary>
      /// A value of EowedArrearsIFamily.
      /// </summary>
      [JsonPropertyName("eowedArrearsIFamily")]
      [Member(Index = 23, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedArrearsIFamily
      {
        get => eowedArrearsIFamily ??= new();
        set => eowedArrearsIFamily = value;
      }

      /// <summary>
      /// A value of EowedCurrTotal.
      /// </summary>
      [JsonPropertyName("eowedCurrTotal")]
      [Member(Index = 24, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedCurrTotal
      {
        get => eowedCurrTotal ??= new();
        set => eowedCurrTotal = value;
      }

      /// <summary>
      /// A value of EowedArrearsTotal.
      /// </summary>
      [JsonPropertyName("eowedArrearsTotal")]
      [Member(Index = 25, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedArrearsTotal
      {
        get => eowedArrearsTotal ??= new();
        set => eowedArrearsTotal = value;
      }

      /// <summary>
      /// A value of EowedTotal.
      /// </summary>
      [JsonPropertyName("eowedTotal")]
      [Member(Index = 26, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EowedTotal
      {
        get => eowedTotal ??= new();
        set => eowedTotal = value;
      }

      /// <summary>
      /// A value of EcollCurrTotal.
      /// </summary>
      [JsonPropertyName("ecollCurrTotal")]
      [Member(Index = 27, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollCurrTotal
      {
        get => ecollCurrTotal ??= new();
        set => ecollCurrTotal = value;
      }

      /// <summary>
      /// A value of EcollArrearsTotal.
      /// </summary>
      [JsonPropertyName("ecollArrearsTotal")]
      [Member(Index = 28, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollArrearsTotal
      {
        get => ecollArrearsTotal ??= new();
        set => ecollArrearsTotal = value;
      }

      /// <summary>
      /// A value of EcollectedTotal.
      /// </summary>
      [JsonPropertyName("ecollectedTotal")]
      [Member(Index = 29, AccessFields = false, Members
        = new[] { "TotalCurrency" })]
      public Common EcollectedTotal
      {
        get => ecollectedTotal ??= new();
        set => ecollectedTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common etotalOrdersReferred;
      private Common enumberOfPayingOrders;
      private Common eordersInLocate;
      private Fips fips;
      private Tribunal tribunal;
      private LegalAction legalAction;
      private TextWorkArea ereferralType;
      private Common ecollCurrentState;
      private Common ecollCurrentFamily;
      private Common ecollCurrentIState;
      private Common ecollCurrentIFamily;
      private Common ecollArrearsState;
      private Common ecollArrearsFamily;
      private Common ecollArrearsIState;
      private Common ecollArrearsIFamily;
      private Common eowedCurrentState;
      private Common eowedCurrentFamily;
      private Common eowedCurrentIState;
      private Common eowedCurrentIFamily;
      private Common eowedArrearsState;
      private Common eowedArrearsFamily;
      private Common eowedArrearsIState;
      private Common eowedArrearsIFamily;
      private Common eowedCurrTotal;
      private Common eowedArrearsTotal;
      private Common eowedTotal;
      private Common ecollCurrTotal;
      private Common ecollArrearsTotal;
      private Common ecollectedTotal;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 1)]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Array<GroupGroup> group;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
