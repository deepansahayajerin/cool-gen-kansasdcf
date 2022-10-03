// Program: FN_B622_ROLL_UP_TOTALS, ID: 373457552, model: 746.
// Short name: SWE00220
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B622_ROLL_UP_TOTALS.
/// </summary>
[Serializable]
public partial class FnB622RollUpTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B622_ROLL_UP_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB622RollUpTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB622RollUpTotals.
  /// </summary>
  public FnB622RollUpTotals(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    for(import.Group1.Index = 0; import.Group1.Index < import.Group1.Count; ++
      import.Group1.Index)
    {
      if (!import.Group1.CheckSize())
      {
        break;
      }

      import.Group2.Index = import.Group1.Index;
      import.Group2.CheckSize();

      export.Group.Index = import.Group1.Index;
      export.Group.CheckSize();

      export.Group.Update.EcollArrearsFamily.TotalCurrency =
        import.Group1.Item.IcollArrearsFamily.TotalCurrency + import
        .Group2.Item.I2ollArrearsFamily.TotalCurrency;
      export.Group.Update.EcollArrearsIFamily.TotalCurrency =
        import.Group1.Item.IcollArrearsIFamily.TotalCurrency + import
        .Group2.Item.I2ollArrearsIFamily.TotalCurrency;
      export.Group.Update.EcollArrearsIState.TotalCurrency =
        import.Group1.Item.IcollArrearsIState.TotalCurrency + import
        .Group2.Item.I2ollArrearsIState.TotalCurrency;
      export.Group.Update.EcollArrearsState.TotalCurrency =
        import.Group1.Item.IcollArrearsState.TotalCurrency + import
        .Group2.Item.I2ollArrearsState.TotalCurrency;
      export.Group.Update.EcollArrearsTotal.TotalCurrency =
        import.Group1.Item.IcollArrearsTotal.TotalCurrency + import
        .Group2.Item.I2ollArrearsTotal.TotalCurrency;
      export.Group.Update.EcollCurrTotal.TotalCurrency =
        import.Group1.Item.IcollCurrTotal.TotalCurrency + import
        .Group2.Item.I2ollCurrTotal.TotalCurrency;
      export.Group.Update.EcollCurrentFamily.TotalCurrency =
        import.Group1.Item.IcollCurrentFamily.TotalCurrency + import
        .Group2.Item.I2ollCurrentFamily.TotalCurrency;
      export.Group.Update.EcollCurrentIFamily.TotalCurrency =
        import.Group1.Item.IcollCurrentIFamily.TotalCurrency + import
        .Group2.Item.I2ollCurrentIFamily.TotalCurrency;
      export.Group.Update.EcollCurrentIState.TotalCurrency =
        import.Group1.Item.IcollCurrentIState.TotalCurrency + import
        .Group2.Item.I2ollCurrentIState.TotalCurrency;
      export.Group.Update.EcollCurrentState.TotalCurrency =
        import.Group1.Item.IcollCurrentState.TotalCurrency + import
        .Group2.Item.I2ollCurrentState.TotalCurrency;
      export.Group.Update.EcollectedTotal.TotalCurrency =
        import.Group1.Item.IcollectedTotal.TotalCurrency + import
        .Group2.Item.I2ollectedTotal.TotalCurrency;
      export.Group.Update.EnumberOfPayingOrders.Count =
        import.Group1.Item.InumberOfPayingOrders.Count + import
        .Group2.Item.I2umberOfPayingOrders.Count;
      export.Group.Update.EordersInLocate.Count =
        import.Group1.Item.IordersInLocate.Count + import
        .Group2.Item.I2rdersInLocate.Count;
      export.Group.Update.EowedArrearsFamily.TotalCurrency =
        import.Group1.Item.IowedArrearsFamily.TotalCurrency + import
        .Group2.Item.I2wedArrearsFamily.TotalCurrency;
      export.Group.Update.EowedArrearsIFamily.TotalCurrency =
        import.Group1.Item.IowedArrearsIFamily.TotalCurrency + import
        .Group2.Item.I2wedArrearsIFamily.TotalCurrency;
      export.Group.Update.EowedArrearsIState.TotalCurrency =
        import.Group1.Item.IowedArrearsIState.TotalCurrency + import
        .Group2.Item.I2wedArrearsIState.TotalCurrency;
      export.Group.Update.EowedArrearsState.TotalCurrency =
        import.Group1.Item.IowedArrearsState.TotalCurrency + import
        .Group2.Item.I2wedArrearsState.TotalCurrency;
      export.Group.Update.EowedArrearsTotal.TotalCurrency =
        import.Group1.Item.IowedArrearsTotal.TotalCurrency + import
        .Group2.Item.I2wedArrearsTotal.TotalCurrency;
      export.Group.Update.EowedCurrTotal.TotalCurrency =
        import.Group1.Item.IowedCurrTotal.TotalCurrency + import
        .Group2.Item.I2wedCurrTotal.TotalCurrency;
      export.Group.Update.EowedCurrentFamily.TotalCurrency =
        import.Group1.Item.IowedCurrentFamily.TotalCurrency + import
        .Group2.Item.I2wedCurrentFamily.TotalCurrency;
      export.Group.Update.EowedCurrentIFamily.TotalCurrency =
        import.Group1.Item.IowedCurrentIFamily.TotalCurrency + import
        .Group2.Item.I2wedCurrentIFamily.TotalCurrency;
      export.Group.Update.EowedCurrentIState.TotalCurrency =
        import.Group1.Item.IowedCurrentIState.TotalCurrency + import
        .Group2.Item.I2wedCurrentIState.TotalCurrency;
      export.Group.Update.EowedCurrentState.TotalCurrency =
        import.Group1.Item.IowedCurrentState.TotalCurrency + import
        .Group2.Item.I2wedCurrentState.TotalCurrency;
      export.Group.Update.EowedTotal.TotalCurrency =
        import.Group1.Item.IowedTotal.TotalCurrency + import
        .Group2.Item.I2wedTotal.TotalCurrency;
      export.Group.Update.EtotalOrdersReferred.Count =
        import.Group1.Item.ItotalOrdersReferred.Count + import
        .Group2.Item.I2otalOrdersReferred.Count;
    }

    import.Group1.CheckIndex();
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
    /// <summary>A Group1Group group.</summary>
    [Serializable]
    public class Group1Group
    {
      /// <summary>
      /// A value of ItotalOrdersReferred.
      /// </summary>
      [JsonPropertyName("itotalOrdersReferred")]
      public Common ItotalOrdersReferred
      {
        get => itotalOrdersReferred ??= new();
        set => itotalOrdersReferred = value;
      }

      /// <summary>
      /// A value of InumberOfPayingOrders.
      /// </summary>
      [JsonPropertyName("inumberOfPayingOrders")]
      public Common InumberOfPayingOrders
      {
        get => inumberOfPayingOrders ??= new();
        set => inumberOfPayingOrders = value;
      }

      /// <summary>
      /// A value of IordersInLocate.
      /// </summary>
      [JsonPropertyName("iordersInLocate")]
      public Common IordersInLocate
      {
        get => iordersInLocate ??= new();
        set => iordersInLocate = value;
      }

      /// <summary>
      /// A value of Ifips.
      /// </summary>
      [JsonPropertyName("ifips")]
      public Fips Ifips
      {
        get => ifips ??= new();
        set => ifips = value;
      }

      /// <summary>
      /// A value of Itribunal.
      /// </summary>
      [JsonPropertyName("itribunal")]
      public Tribunal Itribunal
      {
        get => itribunal ??= new();
        set => itribunal = value;
      }

      /// <summary>
      /// A value of IlegalAction.
      /// </summary>
      [JsonPropertyName("ilegalAction")]
      public LegalAction IlegalAction
      {
        get => ilegalAction ??= new();
        set => ilegalAction = value;
      }

      /// <summary>
      /// A value of ItextWorkArea.
      /// </summary>
      [JsonPropertyName("itextWorkArea")]
      public TextWorkArea ItextWorkArea
      {
        get => itextWorkArea ??= new();
        set => itextWorkArea = value;
      }

      /// <summary>
      /// A value of IcollCurrentState.
      /// </summary>
      [JsonPropertyName("icollCurrentState")]
      public Common IcollCurrentState
      {
        get => icollCurrentState ??= new();
        set => icollCurrentState = value;
      }

      /// <summary>
      /// A value of IcollCurrentFamily.
      /// </summary>
      [JsonPropertyName("icollCurrentFamily")]
      public Common IcollCurrentFamily
      {
        get => icollCurrentFamily ??= new();
        set => icollCurrentFamily = value;
      }

      /// <summary>
      /// A value of IcollCurrentIState.
      /// </summary>
      [JsonPropertyName("icollCurrentIState")]
      public Common IcollCurrentIState
      {
        get => icollCurrentIState ??= new();
        set => icollCurrentIState = value;
      }

      /// <summary>
      /// A value of IcollCurrentIFamily.
      /// </summary>
      [JsonPropertyName("icollCurrentIFamily")]
      public Common IcollCurrentIFamily
      {
        get => icollCurrentIFamily ??= new();
        set => icollCurrentIFamily = value;
      }

      /// <summary>
      /// A value of IcollArrearsState.
      /// </summary>
      [JsonPropertyName("icollArrearsState")]
      public Common IcollArrearsState
      {
        get => icollArrearsState ??= new();
        set => icollArrearsState = value;
      }

      /// <summary>
      /// A value of IcollArrearsFamily.
      /// </summary>
      [JsonPropertyName("icollArrearsFamily")]
      public Common IcollArrearsFamily
      {
        get => icollArrearsFamily ??= new();
        set => icollArrearsFamily = value;
      }

      /// <summary>
      /// A value of IcollArrearsIState.
      /// </summary>
      [JsonPropertyName("icollArrearsIState")]
      public Common IcollArrearsIState
      {
        get => icollArrearsIState ??= new();
        set => icollArrearsIState = value;
      }

      /// <summary>
      /// A value of IcollArrearsIFamily.
      /// </summary>
      [JsonPropertyName("icollArrearsIFamily")]
      public Common IcollArrearsIFamily
      {
        get => icollArrearsIFamily ??= new();
        set => icollArrearsIFamily = value;
      }

      /// <summary>
      /// A value of IowedCurrentState.
      /// </summary>
      [JsonPropertyName("iowedCurrentState")]
      public Common IowedCurrentState
      {
        get => iowedCurrentState ??= new();
        set => iowedCurrentState = value;
      }

      /// <summary>
      /// A value of IowedCurrentFamily.
      /// </summary>
      [JsonPropertyName("iowedCurrentFamily")]
      public Common IowedCurrentFamily
      {
        get => iowedCurrentFamily ??= new();
        set => iowedCurrentFamily = value;
      }

      /// <summary>
      /// A value of IowedCurrentIState.
      /// </summary>
      [JsonPropertyName("iowedCurrentIState")]
      public Common IowedCurrentIState
      {
        get => iowedCurrentIState ??= new();
        set => iowedCurrentIState = value;
      }

      /// <summary>
      /// A value of IowedCurrentIFamily.
      /// </summary>
      [JsonPropertyName("iowedCurrentIFamily")]
      public Common IowedCurrentIFamily
      {
        get => iowedCurrentIFamily ??= new();
        set => iowedCurrentIFamily = value;
      }

      /// <summary>
      /// A value of IowedArrearsState.
      /// </summary>
      [JsonPropertyName("iowedArrearsState")]
      public Common IowedArrearsState
      {
        get => iowedArrearsState ??= new();
        set => iowedArrearsState = value;
      }

      /// <summary>
      /// A value of IowedArrearsFamily.
      /// </summary>
      [JsonPropertyName("iowedArrearsFamily")]
      public Common IowedArrearsFamily
      {
        get => iowedArrearsFamily ??= new();
        set => iowedArrearsFamily = value;
      }

      /// <summary>
      /// A value of IowedArrearsIState.
      /// </summary>
      [JsonPropertyName("iowedArrearsIState")]
      public Common IowedArrearsIState
      {
        get => iowedArrearsIState ??= new();
        set => iowedArrearsIState = value;
      }

      /// <summary>
      /// A value of IowedArrearsIFamily.
      /// </summary>
      [JsonPropertyName("iowedArrearsIFamily")]
      public Common IowedArrearsIFamily
      {
        get => iowedArrearsIFamily ??= new();
        set => iowedArrearsIFamily = value;
      }

      /// <summary>
      /// A value of IowedCurrTotal.
      /// </summary>
      [JsonPropertyName("iowedCurrTotal")]
      public Common IowedCurrTotal
      {
        get => iowedCurrTotal ??= new();
        set => iowedCurrTotal = value;
      }

      /// <summary>
      /// A value of IowedArrearsTotal.
      /// </summary>
      [JsonPropertyName("iowedArrearsTotal")]
      public Common IowedArrearsTotal
      {
        get => iowedArrearsTotal ??= new();
        set => iowedArrearsTotal = value;
      }

      /// <summary>
      /// A value of IowedTotal.
      /// </summary>
      [JsonPropertyName("iowedTotal")]
      public Common IowedTotal
      {
        get => iowedTotal ??= new();
        set => iowedTotal = value;
      }

      /// <summary>
      /// A value of IcollCurrTotal.
      /// </summary>
      [JsonPropertyName("icollCurrTotal")]
      public Common IcollCurrTotal
      {
        get => icollCurrTotal ??= new();
        set => icollCurrTotal = value;
      }

      /// <summary>
      /// A value of IcollArrearsTotal.
      /// </summary>
      [JsonPropertyName("icollArrearsTotal")]
      public Common IcollArrearsTotal
      {
        get => icollArrearsTotal ??= new();
        set => icollArrearsTotal = value;
      }

      /// <summary>
      /// A value of IcollectedTotal.
      /// </summary>
      [JsonPropertyName("icollectedTotal")]
      public Common IcollectedTotal
      {
        get => icollectedTotal ??= new();
        set => icollectedTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common itotalOrdersReferred;
      private Common inumberOfPayingOrders;
      private Common iordersInLocate;
      private Fips ifips;
      private Tribunal itribunal;
      private LegalAction ilegalAction;
      private TextWorkArea itextWorkArea;
      private Common icollCurrentState;
      private Common icollCurrentFamily;
      private Common icollCurrentIState;
      private Common icollCurrentIFamily;
      private Common icollArrearsState;
      private Common icollArrearsFamily;
      private Common icollArrearsIState;
      private Common icollArrearsIFamily;
      private Common iowedCurrentState;
      private Common iowedCurrentFamily;
      private Common iowedCurrentIState;
      private Common iowedCurrentIFamily;
      private Common iowedArrearsState;
      private Common iowedArrearsFamily;
      private Common iowedArrearsIState;
      private Common iowedArrearsIFamily;
      private Common iowedCurrTotal;
      private Common iowedArrearsTotal;
      private Common iowedTotal;
      private Common icollCurrTotal;
      private Common icollArrearsTotal;
      private Common icollectedTotal;
    }

    /// <summary>A Group2Group group.</summary>
    [Serializable]
    public class Group2Group
    {
      /// <summary>
      /// A value of I2otalOrdersReferred.
      /// </summary>
      [JsonPropertyName("i2otalOrdersReferred")]
      public Common I2otalOrdersReferred
      {
        get => i2otalOrdersReferred ??= new();
        set => i2otalOrdersReferred = value;
      }

      /// <summary>
      /// A value of I2umberOfPayingOrders.
      /// </summary>
      [JsonPropertyName("i2umberOfPayingOrders")]
      public Common I2umberOfPayingOrders
      {
        get => i2umberOfPayingOrders ??= new();
        set => i2umberOfPayingOrders = value;
      }

      /// <summary>
      /// A value of I2rdersInLocate.
      /// </summary>
      [JsonPropertyName("i2rdersInLocate")]
      public Common I2rdersInLocate
      {
        get => i2rdersInLocate ??= new();
        set => i2rdersInLocate = value;
      }

      /// <summary>
      /// A value of I2ips.
      /// </summary>
      [JsonPropertyName("i2ips")]
      public Fips I2ips
      {
        get => i2ips ??= new();
        set => i2ips = value;
      }

      /// <summary>
      /// A value of I2ribunal.
      /// </summary>
      [JsonPropertyName("i2ribunal")]
      public Tribunal I2ribunal
      {
        get => i2ribunal ??= new();
        set => i2ribunal = value;
      }

      /// <summary>
      /// A value of I2egalAction.
      /// </summary>
      [JsonPropertyName("i2egalAction")]
      public LegalAction I2egalAction
      {
        get => i2egalAction ??= new();
        set => i2egalAction = value;
      }

      /// <summary>
      /// A value of I2extWorkArea.
      /// </summary>
      [JsonPropertyName("i2extWorkArea")]
      public TextWorkArea I2extWorkArea
      {
        get => i2extWorkArea ??= new();
        set => i2extWorkArea = value;
      }

      /// <summary>
      /// A value of I2ollCurrentState.
      /// </summary>
      [JsonPropertyName("i2ollCurrentState")]
      public Common I2ollCurrentState
      {
        get => i2ollCurrentState ??= new();
        set => i2ollCurrentState = value;
      }

      /// <summary>
      /// A value of I2ollCurrentFamily.
      /// </summary>
      [JsonPropertyName("i2ollCurrentFamily")]
      public Common I2ollCurrentFamily
      {
        get => i2ollCurrentFamily ??= new();
        set => i2ollCurrentFamily = value;
      }

      /// <summary>
      /// A value of I2ollCurrentIState.
      /// </summary>
      [JsonPropertyName("i2ollCurrentIState")]
      public Common I2ollCurrentIState
      {
        get => i2ollCurrentIState ??= new();
        set => i2ollCurrentIState = value;
      }

      /// <summary>
      /// A value of I2ollCurrentIFamily.
      /// </summary>
      [JsonPropertyName("i2ollCurrentIFamily")]
      public Common I2ollCurrentIFamily
      {
        get => i2ollCurrentIFamily ??= new();
        set => i2ollCurrentIFamily = value;
      }

      /// <summary>
      /// A value of I2ollArrearsState.
      /// </summary>
      [JsonPropertyName("i2ollArrearsState")]
      public Common I2ollArrearsState
      {
        get => i2ollArrearsState ??= new();
        set => i2ollArrearsState = value;
      }

      /// <summary>
      /// A value of I2ollArrearsFamily.
      /// </summary>
      [JsonPropertyName("i2ollArrearsFamily")]
      public Common I2ollArrearsFamily
      {
        get => i2ollArrearsFamily ??= new();
        set => i2ollArrearsFamily = value;
      }

      /// <summary>
      /// A value of I2ollArrearsIState.
      /// </summary>
      [JsonPropertyName("i2ollArrearsIState")]
      public Common I2ollArrearsIState
      {
        get => i2ollArrearsIState ??= new();
        set => i2ollArrearsIState = value;
      }

      /// <summary>
      /// A value of I2ollArrearsIFamily.
      /// </summary>
      [JsonPropertyName("i2ollArrearsIFamily")]
      public Common I2ollArrearsIFamily
      {
        get => i2ollArrearsIFamily ??= new();
        set => i2ollArrearsIFamily = value;
      }

      /// <summary>
      /// A value of I2wedCurrentState.
      /// </summary>
      [JsonPropertyName("i2wedCurrentState")]
      public Common I2wedCurrentState
      {
        get => i2wedCurrentState ??= new();
        set => i2wedCurrentState = value;
      }

      /// <summary>
      /// A value of I2wedCurrentFamily.
      /// </summary>
      [JsonPropertyName("i2wedCurrentFamily")]
      public Common I2wedCurrentFamily
      {
        get => i2wedCurrentFamily ??= new();
        set => i2wedCurrentFamily = value;
      }

      /// <summary>
      /// A value of I2wedCurrentIState.
      /// </summary>
      [JsonPropertyName("i2wedCurrentIState")]
      public Common I2wedCurrentIState
      {
        get => i2wedCurrentIState ??= new();
        set => i2wedCurrentIState = value;
      }

      /// <summary>
      /// A value of I2wedCurrentIFamily.
      /// </summary>
      [JsonPropertyName("i2wedCurrentIFamily")]
      public Common I2wedCurrentIFamily
      {
        get => i2wedCurrentIFamily ??= new();
        set => i2wedCurrentIFamily = value;
      }

      /// <summary>
      /// A value of I2wedArrearsState.
      /// </summary>
      [JsonPropertyName("i2wedArrearsState")]
      public Common I2wedArrearsState
      {
        get => i2wedArrearsState ??= new();
        set => i2wedArrearsState = value;
      }

      /// <summary>
      /// A value of I2wedArrearsFamily.
      /// </summary>
      [JsonPropertyName("i2wedArrearsFamily")]
      public Common I2wedArrearsFamily
      {
        get => i2wedArrearsFamily ??= new();
        set => i2wedArrearsFamily = value;
      }

      /// <summary>
      /// A value of I2wedArrearsIState.
      /// </summary>
      [JsonPropertyName("i2wedArrearsIState")]
      public Common I2wedArrearsIState
      {
        get => i2wedArrearsIState ??= new();
        set => i2wedArrearsIState = value;
      }

      /// <summary>
      /// A value of I2wedArrearsIFamily.
      /// </summary>
      [JsonPropertyName("i2wedArrearsIFamily")]
      public Common I2wedArrearsIFamily
      {
        get => i2wedArrearsIFamily ??= new();
        set => i2wedArrearsIFamily = value;
      }

      /// <summary>
      /// A value of I2wedCurrTotal.
      /// </summary>
      [JsonPropertyName("i2wedCurrTotal")]
      public Common I2wedCurrTotal
      {
        get => i2wedCurrTotal ??= new();
        set => i2wedCurrTotal = value;
      }

      /// <summary>
      /// A value of I2wedArrearsTotal.
      /// </summary>
      [JsonPropertyName("i2wedArrearsTotal")]
      public Common I2wedArrearsTotal
      {
        get => i2wedArrearsTotal ??= new();
        set => i2wedArrearsTotal = value;
      }

      /// <summary>
      /// A value of I2wedTotal.
      /// </summary>
      [JsonPropertyName("i2wedTotal")]
      public Common I2wedTotal
      {
        get => i2wedTotal ??= new();
        set => i2wedTotal = value;
      }

      /// <summary>
      /// A value of I2ollCurrTotal.
      /// </summary>
      [JsonPropertyName("i2ollCurrTotal")]
      public Common I2ollCurrTotal
      {
        get => i2ollCurrTotal ??= new();
        set => i2ollCurrTotal = value;
      }

      /// <summary>
      /// A value of I2ollArrearsTotal.
      /// </summary>
      [JsonPropertyName("i2ollArrearsTotal")]
      public Common I2ollArrearsTotal
      {
        get => i2ollArrearsTotal ??= new();
        set => i2ollArrearsTotal = value;
      }

      /// <summary>
      /// A value of I2ollectedTotal.
      /// </summary>
      [JsonPropertyName("i2ollectedTotal")]
      public Common I2ollectedTotal
      {
        get => i2ollectedTotal ??= new();
        set => i2ollectedTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common i2otalOrdersReferred;
      private Common i2umberOfPayingOrders;
      private Common i2rdersInLocate;
      private Fips i2ips;
      private Tribunal i2ribunal;
      private LegalAction i2egalAction;
      private TextWorkArea i2extWorkArea;
      private Common i2ollCurrentState;
      private Common i2ollCurrentFamily;
      private Common i2ollCurrentIState;
      private Common i2ollCurrentIFamily;
      private Common i2ollArrearsState;
      private Common i2ollArrearsFamily;
      private Common i2ollArrearsIState;
      private Common i2ollArrearsIFamily;
      private Common i2wedCurrentState;
      private Common i2wedCurrentFamily;
      private Common i2wedCurrentIState;
      private Common i2wedCurrentIFamily;
      private Common i2wedArrearsState;
      private Common i2wedArrearsFamily;
      private Common i2wedArrearsIState;
      private Common i2wedArrearsIFamily;
      private Common i2wedCurrTotal;
      private Common i2wedArrearsTotal;
      private Common i2wedTotal;
      private Common i2ollCurrTotal;
      private Common i2ollArrearsTotal;
      private Common i2ollectedTotal;
    }

    /// <summary>
    /// Gets a value of Group1.
    /// </summary>
    [JsonIgnore]
    public Array<Group1Group> Group1 => group1 ??= new(Group1Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Group1 for json serialization.
    /// </summary>
    [JsonPropertyName("group1")]
    [Computed]
    public IList<Group1Group> Group1_Json
    {
      get => group1;
      set => Group1.Assign(value);
    }

    /// <summary>
    /// Gets a value of Group2.
    /// </summary>
    [JsonIgnore]
    public Array<Group2Group> Group2 => group2 ??= new(Group2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Group2 for json serialization.
    /// </summary>
    [JsonPropertyName("group2")]
    [Computed]
    public IList<Group2Group> Group2_Json
    {
      get => group2;
      set => Group2.Assign(value);
    }

    private Array<Group1Group> group1;
    private Array<Group2Group> group2;
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
      public Common EtotalOrdersReferred
      {
        get => etotalOrdersReferred ??= new();
        set => etotalOrdersReferred = value;
      }

      /// <summary>
      /// A value of EnumberOfPayingOrders.
      /// </summary>
      [JsonPropertyName("enumberOfPayingOrders")]
      public Common EnumberOfPayingOrders
      {
        get => enumberOfPayingOrders ??= new();
        set => enumberOfPayingOrders = value;
      }

      /// <summary>
      /// A value of EordersInLocate.
      /// </summary>
      [JsonPropertyName("eordersInLocate")]
      public Common EordersInLocate
      {
        get => eordersInLocate ??= new();
        set => eordersInLocate = value;
      }

      /// <summary>
      /// A value of Fips.
      /// </summary>
      [JsonPropertyName("fips")]
      public Fips Fips
      {
        get => fips ??= new();
        set => fips = value;
      }

      /// <summary>
      /// A value of Tribunal.
      /// </summary>
      [JsonPropertyName("tribunal")]
      public Tribunal Tribunal
      {
        get => tribunal ??= new();
        set => tribunal = value;
      }

      /// <summary>
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>
      /// A value of EreferralType.
      /// </summary>
      [JsonPropertyName("ereferralType")]
      public TextWorkArea EreferralType
      {
        get => ereferralType ??= new();
        set => ereferralType = value;
      }

      /// <summary>
      /// A value of EcollCurrentState.
      /// </summary>
      [JsonPropertyName("ecollCurrentState")]
      public Common EcollCurrentState
      {
        get => ecollCurrentState ??= new();
        set => ecollCurrentState = value;
      }

      /// <summary>
      /// A value of EcollCurrentFamily.
      /// </summary>
      [JsonPropertyName("ecollCurrentFamily")]
      public Common EcollCurrentFamily
      {
        get => ecollCurrentFamily ??= new();
        set => ecollCurrentFamily = value;
      }

      /// <summary>
      /// A value of EcollCurrentIState.
      /// </summary>
      [JsonPropertyName("ecollCurrentIState")]
      public Common EcollCurrentIState
      {
        get => ecollCurrentIState ??= new();
        set => ecollCurrentIState = value;
      }

      /// <summary>
      /// A value of EcollCurrentIFamily.
      /// </summary>
      [JsonPropertyName("ecollCurrentIFamily")]
      public Common EcollCurrentIFamily
      {
        get => ecollCurrentIFamily ??= new();
        set => ecollCurrentIFamily = value;
      }

      /// <summary>
      /// A value of EcollArrearsState.
      /// </summary>
      [JsonPropertyName("ecollArrearsState")]
      public Common EcollArrearsState
      {
        get => ecollArrearsState ??= new();
        set => ecollArrearsState = value;
      }

      /// <summary>
      /// A value of EcollArrearsFamily.
      /// </summary>
      [JsonPropertyName("ecollArrearsFamily")]
      public Common EcollArrearsFamily
      {
        get => ecollArrearsFamily ??= new();
        set => ecollArrearsFamily = value;
      }

      /// <summary>
      /// A value of EcollArrearsIState.
      /// </summary>
      [JsonPropertyName("ecollArrearsIState")]
      public Common EcollArrearsIState
      {
        get => ecollArrearsIState ??= new();
        set => ecollArrearsIState = value;
      }

      /// <summary>
      /// A value of EcollArrearsIFamily.
      /// </summary>
      [JsonPropertyName("ecollArrearsIFamily")]
      public Common EcollArrearsIFamily
      {
        get => ecollArrearsIFamily ??= new();
        set => ecollArrearsIFamily = value;
      }

      /// <summary>
      /// A value of EowedCurrentState.
      /// </summary>
      [JsonPropertyName("eowedCurrentState")]
      public Common EowedCurrentState
      {
        get => eowedCurrentState ??= new();
        set => eowedCurrentState = value;
      }

      /// <summary>
      /// A value of EowedCurrentFamily.
      /// </summary>
      [JsonPropertyName("eowedCurrentFamily")]
      public Common EowedCurrentFamily
      {
        get => eowedCurrentFamily ??= new();
        set => eowedCurrentFamily = value;
      }

      /// <summary>
      /// A value of EowedCurrentIState.
      /// </summary>
      [JsonPropertyName("eowedCurrentIState")]
      public Common EowedCurrentIState
      {
        get => eowedCurrentIState ??= new();
        set => eowedCurrentIState = value;
      }

      /// <summary>
      /// A value of EowedCurrentIFamily.
      /// </summary>
      [JsonPropertyName("eowedCurrentIFamily")]
      public Common EowedCurrentIFamily
      {
        get => eowedCurrentIFamily ??= new();
        set => eowedCurrentIFamily = value;
      }

      /// <summary>
      /// A value of EowedArrearsState.
      /// </summary>
      [JsonPropertyName("eowedArrearsState")]
      public Common EowedArrearsState
      {
        get => eowedArrearsState ??= new();
        set => eowedArrearsState = value;
      }

      /// <summary>
      /// A value of EowedArrearsFamily.
      /// </summary>
      [JsonPropertyName("eowedArrearsFamily")]
      public Common EowedArrearsFamily
      {
        get => eowedArrearsFamily ??= new();
        set => eowedArrearsFamily = value;
      }

      /// <summary>
      /// A value of EowedArrearsIState.
      /// </summary>
      [JsonPropertyName("eowedArrearsIState")]
      public Common EowedArrearsIState
      {
        get => eowedArrearsIState ??= new();
        set => eowedArrearsIState = value;
      }

      /// <summary>
      /// A value of EowedArrearsIFamily.
      /// </summary>
      [JsonPropertyName("eowedArrearsIFamily")]
      public Common EowedArrearsIFamily
      {
        get => eowedArrearsIFamily ??= new();
        set => eowedArrearsIFamily = value;
      }

      /// <summary>
      /// A value of EowedCurrTotal.
      /// </summary>
      [JsonPropertyName("eowedCurrTotal")]
      public Common EowedCurrTotal
      {
        get => eowedCurrTotal ??= new();
        set => eowedCurrTotal = value;
      }

      /// <summary>
      /// A value of EowedArrearsTotal.
      /// </summary>
      [JsonPropertyName("eowedArrearsTotal")]
      public Common EowedArrearsTotal
      {
        get => eowedArrearsTotal ??= new();
        set => eowedArrearsTotal = value;
      }

      /// <summary>
      /// A value of EowedTotal.
      /// </summary>
      [JsonPropertyName("eowedTotal")]
      public Common EowedTotal
      {
        get => eowedTotal ??= new();
        set => eowedTotal = value;
      }

      /// <summary>
      /// A value of EcollCurrTotal.
      /// </summary>
      [JsonPropertyName("ecollCurrTotal")]
      public Common EcollCurrTotal
      {
        get => ecollCurrTotal ??= new();
        set => ecollCurrTotal = value;
      }

      /// <summary>
      /// A value of EcollArrearsTotal.
      /// </summary>
      [JsonPropertyName("ecollArrearsTotal")]
      public Common EcollArrearsTotal
      {
        get => ecollArrearsTotal ??= new();
        set => ecollArrearsTotal = value;
      }

      /// <summary>
      /// A value of EcollectedTotal.
      /// </summary>
      [JsonPropertyName("ecollectedTotal")]
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

    private Array<GroupGroup> group;
  }
#endregion
}
