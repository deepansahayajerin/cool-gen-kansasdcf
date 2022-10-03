// Program: SI_CADS_MOVE_NEXT_GOOD_CAUSE, ID: 371731797, model: 746.
// Short name: SWE01191
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CADS_MOVE_NEXT_GOOD_CAUSE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCadsMoveNextGoodCause: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CADS_MOVE_NEXT_GOOD_CAUSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCadsMoveNextGoodCause(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCadsMoveNextGoodCause.
  /// </summary>
  public SiCadsMoveNextGoodCause(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //    Date     Developer             Description
    // ??/??/?? ?????????             Initial Development
    // ---------------------------------------------------------
    // 06/22/99 W.Campbell            Replaced ZD exit state.
    // ---------------------------------------------------------
    export.CurrItmNoGc.Count = import.CurrItmNoGc.Count;
    export.Gc1.PageNumber = import.Gc1.PageNumber;
    export.GcMinus.OneChar = import.GcMinus.OneChar;
    export.GcPlus.OneChar = import.GcPlus.OneChar;
    export.HidNoItemsFndGc.Count = import.HidNoItemsFndGc.Count;

    export.Gc.Index = 0;
    export.Gc.CheckSize();

    export.Gc.Count = 1;
    export.NoItemsGc.Count = 0;

    do
    {
      ++export.NoItemsGc.Count;
      ++export.CurrItmNoGc.Count;

      import.HiddenGrpGc.Index = export.CurrItmNoGc.Count - 1;
      import.HiddenGrpGc.CheckSize();

      ++export.Gc.Index;
      export.Gc.CheckSize();

      export.Gc.Update.GgcApCsePersonsWorkSet.Number =
        import.HiddenGrpGc.Item.HiddenGGcApCsePersonsWorkSet.Number;
      MoveCaseRole(import.HiddenGrpGc.Item.HiddenGGcApCaseRole,
        export.Gc.Update.GgcApCaseRole);
      export.Gc.Update.GgcGoodCause.Assign(
        import.HiddenGrpGc.Item.HiddenGGcGoodCause);
      export.Gc.Update.GgcCommon.SelectChar =
        import.HiddenGrpGc.Item.HiddenGGcCommon.SelectChar;
    }
    while(export.Gc.Index != 3 && import.HiddenGrpGc.Index + 1 != export
      .HidNoItemsFndGc.Count);

    if (export.CurrItmNoGc.Count == export.HidNoItemsFndGc.Count)
    {
      export.GcPlus.OneChar = "";

      if (export.CurrItmNoGc.Count > 3)
      {
        export.GcMinus.OneChar = "-";
      }
    }
    else if (export.CurrItmNoGc.Count < export.HidNoItemsFndGc.Count)
    {
      export.GcPlus.OneChar = "+";

      if (export.Gc1.PageNumber > 1)
      {
        export.GcMinus.OneChar = "-";
      }
      else
      {
        export.GcMinus.OneChar = "";
      }
    }
    else
    {
      export.GcPlus.OneChar = "+";

      // ---------------------------------------------------------
      // 06/22/99 W.Campbell - Replaced ZD exit state.
      // ---------------------------------------------------------
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
    }
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
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
    /// <summary>A HiddenGrpGcGroup group.</summary>
    [Serializable]
    public class HiddenGrpGcGroup
    {
      /// <summary>
      /// A value of HiddenGGcGoodCause.
      /// </summary>
      [JsonPropertyName("hiddenGGcGoodCause")]
      public GoodCause HiddenGGcGoodCause
      {
        get => hiddenGGcGoodCause ??= new();
        set => hiddenGGcGoodCause = value;
      }

      /// <summary>
      /// A value of HiddenGGcCommon.
      /// </summary>
      [JsonPropertyName("hiddenGGcCommon")]
      public Common HiddenGGcCommon
      {
        get => hiddenGGcCommon ??= new();
        set => hiddenGGcCommon = value;
      }

      /// <summary>
      /// A value of HiddenGGcApCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenGGcApCaseRole")]
      public CaseRole HiddenGGcApCaseRole
      {
        get => hiddenGGcApCaseRole ??= new();
        set => hiddenGGcApCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenGGcApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGGcApCsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGGcApCsePersonsWorkSet
      {
        get => hiddenGGcApCsePersonsWorkSet ??= new();
        set => hiddenGGcApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private GoodCause hiddenGGcGoodCause;
      private Common hiddenGGcCommon;
      private CaseRole hiddenGGcApCaseRole;
      private CsePersonsWorkSet hiddenGGcApCsePersonsWorkSet;
    }

    /// <summary>
    /// A value of CurrItmNoGc.
    /// </summary>
    [JsonPropertyName("currItmNoGc")]
    public Common CurrItmNoGc
    {
      get => currItmNoGc ??= new();
      set => currItmNoGc = value;
    }

    /// <summary>
    /// Gets a value of HiddenGrpGc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpGcGroup> HiddenGrpGc => hiddenGrpGc ??= new(
      HiddenGrpGcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpGc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpGc")]
    [Computed]
    public IList<HiddenGrpGcGroup> HiddenGrpGc_Json
    {
      get => hiddenGrpGc;
      set => HiddenGrpGc.Assign(value);
    }

    /// <summary>
    /// A value of GcPlus.
    /// </summary>
    [JsonPropertyName("gcPlus")]
    public Standard GcPlus
    {
      get => gcPlus ??= new();
      set => gcPlus = value;
    }

    /// <summary>
    /// A value of GcMinus.
    /// </summary>
    [JsonPropertyName("gcMinus")]
    public Standard GcMinus
    {
      get => gcMinus ??= new();
      set => gcMinus = value;
    }

    /// <summary>
    /// A value of HidNoItemsFndGc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndGc")]
    public Common HidNoItemsFndGc
    {
      get => hidNoItemsFndGc ??= new();
      set => hidNoItemsFndGc = value;
    }

    /// <summary>
    /// A value of Gc1.
    /// </summary>
    [JsonPropertyName("gc1")]
    public Standard Gc1
    {
      get => gc1 ??= new();
      set => gc1 = value;
    }

    private Common currItmNoGc;
    private Array<HiddenGrpGcGroup> hiddenGrpGc;
    private Standard gcPlus;
    private Standard gcMinus;
    private Common hidNoItemsFndGc;
    private Standard gc1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GcGroup group.</summary>
    [Serializable]
    public class GcGroup
    {
      /// <summary>
      /// A value of GgcCdPrmpt.
      /// </summary>
      [JsonPropertyName("ggcCdPrmpt")]
      public Common GgcCdPrmpt
      {
        get => ggcCdPrmpt ??= new();
        set => ggcCdPrmpt = value;
      }

      /// <summary>
      /// A value of GgcCommon.
      /// </summary>
      [JsonPropertyName("ggcCommon")]
      public Common GgcCommon
      {
        get => ggcCommon ??= new();
        set => ggcCommon = value;
      }

      /// <summary>
      /// A value of GgcApCaseRole.
      /// </summary>
      [JsonPropertyName("ggcApCaseRole")]
      public CaseRole GgcApCaseRole
      {
        get => ggcApCaseRole ??= new();
        set => ggcApCaseRole = value;
      }

      /// <summary>
      /// A value of GgcApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("ggcApCsePersonsWorkSet")]
      public CsePersonsWorkSet GgcApCsePersonsWorkSet
      {
        get => ggcApCsePersonsWorkSet ??= new();
        set => ggcApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GgcGoodCause.
      /// </summary>
      [JsonPropertyName("ggcGoodCause")]
      public GoodCause GgcGoodCause
      {
        get => ggcGoodCause ??= new();
        set => ggcGoodCause = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common ggcCdPrmpt;
      private Common ggcCommon;
      private CaseRole ggcApCaseRole;
      private CsePersonsWorkSet ggcApCsePersonsWorkSet;
      private GoodCause ggcGoodCause;
    }

    /// <summary>A HiddenGrpGcGroup group.</summary>
    [Serializable]
    public class HiddenGrpGcGroup
    {
      /// <summary>
      /// A value of HiddenGGcGoodCause.
      /// </summary>
      [JsonPropertyName("hiddenGGcGoodCause")]
      public GoodCause HiddenGGcGoodCause
      {
        get => hiddenGGcGoodCause ??= new();
        set => hiddenGGcGoodCause = value;
      }

      /// <summary>
      /// A value of HiddenGGcCommon.
      /// </summary>
      [JsonPropertyName("hiddenGGcCommon")]
      public Common HiddenGGcCommon
      {
        get => hiddenGGcCommon ??= new();
        set => hiddenGGcCommon = value;
      }

      /// <summary>
      /// A value of HiddenGGcApCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenGGcApCaseRole")]
      public CaseRole HiddenGGcApCaseRole
      {
        get => hiddenGGcApCaseRole ??= new();
        set => hiddenGGcApCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenGGcApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGGcApCsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGGcApCsePersonsWorkSet
      {
        get => hiddenGGcApCsePersonsWorkSet ??= new();
        set => hiddenGGcApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private GoodCause hiddenGGcGoodCause;
      private Common hiddenGGcCommon;
      private CaseRole hiddenGGcApCaseRole;
      private CsePersonsWorkSet hiddenGGcApCsePersonsWorkSet;
    }

    /// <summary>
    /// Gets a value of Gc.
    /// </summary>
    [JsonIgnore]
    public Array<GcGroup> Gc => gc ??= new(GcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Gc for json serialization.
    /// </summary>
    [JsonPropertyName("gc")]
    [Computed]
    public IList<GcGroup> Gc_Json
    {
      get => gc;
      set => Gc.Assign(value);
    }

    /// <summary>
    /// A value of NoItemsGc.
    /// </summary>
    [JsonPropertyName("noItemsGc")]
    public Common NoItemsGc
    {
      get => noItemsGc ??= new();
      set => noItemsGc = value;
    }

    /// <summary>
    /// A value of CurrItmNoGc.
    /// </summary>
    [JsonPropertyName("currItmNoGc")]
    public Common CurrItmNoGc
    {
      get => currItmNoGc ??= new();
      set => currItmNoGc = value;
    }

    /// <summary>
    /// Gets a value of HiddenGrpGc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpGcGroup> HiddenGrpGc => hiddenGrpGc ??= new(
      HiddenGrpGcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpGc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpGc")]
    [Computed]
    public IList<HiddenGrpGcGroup> HiddenGrpGc_Json
    {
      get => hiddenGrpGc;
      set => HiddenGrpGc.Assign(value);
    }

    /// <summary>
    /// A value of GcPlus.
    /// </summary>
    [JsonPropertyName("gcPlus")]
    public Standard GcPlus
    {
      get => gcPlus ??= new();
      set => gcPlus = value;
    }

    /// <summary>
    /// A value of GcMinus.
    /// </summary>
    [JsonPropertyName("gcMinus")]
    public Standard GcMinus
    {
      get => gcMinus ??= new();
      set => gcMinus = value;
    }

    /// <summary>
    /// A value of HidNoItemsFndGc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndGc")]
    public Common HidNoItemsFndGc
    {
      get => hidNoItemsFndGc ??= new();
      set => hidNoItemsFndGc = value;
    }

    /// <summary>
    /// A value of Gc1.
    /// </summary>
    [JsonPropertyName("gc1")]
    public Standard Gc1
    {
      get => gc1 ??= new();
      set => gc1 = value;
    }

    private Array<GcGroup> gc;
    private Common noItemsGc;
    private Common currItmNoGc;
    private Array<HiddenGrpGcGroup> hiddenGrpGc;
    private Standard gcPlus;
    private Standard gcMinus;
    private Common hidNoItemsFndGc;
    private Standard gc1;
  }
#endregion
}
