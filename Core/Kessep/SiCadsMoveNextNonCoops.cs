// Program: SI_CADS_MOVE_NEXT_NON_COOPS, ID: 371731798, model: 746.
// Short name: SWE01192
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CADS_MOVE_NEXT_NON_COOPS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCadsMoveNextNonCoops: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CADS_MOVE_NEXT_NON_COOPS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCadsMoveNextNonCoops(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCadsMoveNextNonCoops.
  /// </summary>
  public SiCadsMoveNextNonCoops(IContext context, Import import, Export export):
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
    export.CurrItmNoNc.Count = import.CurrItmNoNc.Count;
    export.HidNoItemsFndNc.Count = import.HidNoItemsFndNc.Count;
    export.Nc1.PageNumber = import.Nc1.PageNumber;
    export.NcMinus.OneChar = import.NcMinus.OneChar;
    export.NcPlus.OneChar = import.NcPlus.OneChar;

    export.Nc.Index = 0;
    export.Nc.CheckSize();

    export.Nc.Count = 1;
    export.NoItemsNc.Count = 0;

    do
    {
      ++export.NoItemsNc.Count;
      ++export.CurrItmNoNc.Count;

      import.HiddenGrpNc.Index = export.CurrItmNoNc.Count - 1;
      import.HiddenGrpNc.CheckSize();

      ++export.Nc.Index;
      export.Nc.CheckSize();

      export.Nc.Update.GncApCsePersonsWorkSet.Number =
        import.HiddenGrpNc.Item.HiddenGNcApCsePersonsWorkSet.Number;
      MoveCaseRole(import.HiddenGrpNc.Item.HiddenGNcApCaseRole,
        export.Nc.Update.GncApCaseRole);
      export.Nc.Update.GncCommon.SelectChar =
        import.HiddenGrpNc.Item.HiddenGNcCommon.SelectChar;
      export.Nc.Update.GncNonCooperation.Assign(
        import.HiddenGrpNc.Item.HiddenGNcNonCooperation);
    }
    while(import.HiddenGrpNc.Index + 1 != export.HidNoItemsFndNc.Count && export
      .Nc.Index != 3);

    if (export.CurrItmNoNc.Count == export.HidNoItemsFndNc.Count)
    {
      export.NcPlus.OneChar = "";

      if (export.HidNoItemsFndNc.Count > 3)
      {
        export.NcMinus.OneChar = "-";
      }
    }
    else if (export.CurrItmNoNc.Count < export.HidNoItemsFndNc.Count)
    {
      export.NcPlus.OneChar = "+";

      if (export.Nc1.PageNumber > 1)
      {
        export.NcMinus.OneChar = "-";
      }
      else
      {
        export.NcMinus.OneChar = "";
      }
    }
    else
    {
      export.NcPlus.OneChar = "+";

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
    /// <summary>A HiddenGrpNcGroup group.</summary>
    [Serializable]
    public class HiddenGrpNcGroup
    {
      /// <summary>
      /// A value of HiddenGNcNonCooperation.
      /// </summary>
      [JsonPropertyName("hiddenGNcNonCooperation")]
      public NonCooperation HiddenGNcNonCooperation
      {
        get => hiddenGNcNonCooperation ??= new();
        set => hiddenGNcNonCooperation = value;
      }

      /// <summary>
      /// A value of HiddenGNcCommon.
      /// </summary>
      [JsonPropertyName("hiddenGNcCommon")]
      public Common HiddenGNcCommon
      {
        get => hiddenGNcCommon ??= new();
        set => hiddenGNcCommon = value;
      }

      /// <summary>
      /// A value of HiddenGNcApCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenGNcApCaseRole")]
      public CaseRole HiddenGNcApCaseRole
      {
        get => hiddenGNcApCaseRole ??= new();
        set => hiddenGNcApCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenGNcApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGNcApCsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGNcApCsePersonsWorkSet
      {
        get => hiddenGNcApCsePersonsWorkSet ??= new();
        set => hiddenGNcApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private NonCooperation hiddenGNcNonCooperation;
      private Common hiddenGNcCommon;
      private CaseRole hiddenGNcApCaseRole;
      private CsePersonsWorkSet hiddenGNcApCsePersonsWorkSet;
    }

    /// <summary>
    /// A value of CurrItmNoNc.
    /// </summary>
    [JsonPropertyName("currItmNoNc")]
    public Common CurrItmNoNc
    {
      get => currItmNoNc ??= new();
      set => currItmNoNc = value;
    }

    /// <summary>
    /// Gets a value of HiddenGrpNc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpNcGroup> HiddenGrpNc => hiddenGrpNc ??= new(
      HiddenGrpNcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpNc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpNc")]
    [Computed]
    public IList<HiddenGrpNcGroup> HiddenGrpNc_Json
    {
      get => hiddenGrpNc;
      set => HiddenGrpNc.Assign(value);
    }

    /// <summary>
    /// A value of HidNoItemsFndNc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndNc")]
    public Common HidNoItemsFndNc
    {
      get => hidNoItemsFndNc ??= new();
      set => hidNoItemsFndNc = value;
    }

    /// <summary>
    /// A value of NcPlus.
    /// </summary>
    [JsonPropertyName("ncPlus")]
    public Standard NcPlus
    {
      get => ncPlus ??= new();
      set => ncPlus = value;
    }

    /// <summary>
    /// A value of NcMinus.
    /// </summary>
    [JsonPropertyName("ncMinus")]
    public Standard NcMinus
    {
      get => ncMinus ??= new();
      set => ncMinus = value;
    }

    /// <summary>
    /// A value of Nc1.
    /// </summary>
    [JsonPropertyName("nc1")]
    public Standard Nc1
    {
      get => nc1 ??= new();
      set => nc1 = value;
    }

    private Common currItmNoNc;
    private Array<HiddenGrpNcGroup> hiddenGrpNc;
    private Common hidNoItemsFndNc;
    private Standard ncPlus;
    private Standard ncMinus;
    private Standard nc1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A NcGroup group.</summary>
    [Serializable]
    public class NcGroup
    {
      /// <summary>
      /// A value of NonCoopRsnPrmpt.
      /// </summary>
      [JsonPropertyName("nonCoopRsnPrmpt")]
      public Common NonCoopRsnPrmpt
      {
        get => nonCoopRsnPrmpt ??= new();
        set => nonCoopRsnPrmpt = value;
      }

      /// <summary>
      /// A value of NonCoopCdPrmpt.
      /// </summary>
      [JsonPropertyName("nonCoopCdPrmpt")]
      public Common NonCoopCdPrmpt
      {
        get => nonCoopCdPrmpt ??= new();
        set => nonCoopCdPrmpt = value;
      }

      /// <summary>
      /// A value of GncCommon.
      /// </summary>
      [JsonPropertyName("gncCommon")]
      public Common GncCommon
      {
        get => gncCommon ??= new();
        set => gncCommon = value;
      }

      /// <summary>
      /// A value of GncApCaseRole.
      /// </summary>
      [JsonPropertyName("gncApCaseRole")]
      public CaseRole GncApCaseRole
      {
        get => gncApCaseRole ??= new();
        set => gncApCaseRole = value;
      }

      /// <summary>
      /// A value of GncApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gncApCsePersonsWorkSet")]
      public CsePersonsWorkSet GncApCsePersonsWorkSet
      {
        get => gncApCsePersonsWorkSet ??= new();
        set => gncApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GncNonCooperation.
      /// </summary>
      [JsonPropertyName("gncNonCooperation")]
      public NonCooperation GncNonCooperation
      {
        get => gncNonCooperation ??= new();
        set => gncNonCooperation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common nonCoopRsnPrmpt;
      private Common nonCoopCdPrmpt;
      private Common gncCommon;
      private CaseRole gncApCaseRole;
      private CsePersonsWorkSet gncApCsePersonsWorkSet;
      private NonCooperation gncNonCooperation;
    }

    /// <summary>A HiddenGrpNcGroup group.</summary>
    [Serializable]
    public class HiddenGrpNcGroup
    {
      /// <summary>
      /// A value of HiddenGNcNonCooperation.
      /// </summary>
      [JsonPropertyName("hiddenGNcNonCooperation")]
      public NonCooperation HiddenGNcNonCooperation
      {
        get => hiddenGNcNonCooperation ??= new();
        set => hiddenGNcNonCooperation = value;
      }

      /// <summary>
      /// A value of HiddenGNcCommon.
      /// </summary>
      [JsonPropertyName("hiddenGNcCommon")]
      public Common HiddenGNcCommon
      {
        get => hiddenGNcCommon ??= new();
        set => hiddenGNcCommon = value;
      }

      /// <summary>
      /// A value of HiddenGNcApCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenGNcApCaseRole")]
      public CaseRole HiddenGNcApCaseRole
      {
        get => hiddenGNcApCaseRole ??= new();
        set => hiddenGNcApCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenGNcApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGNcApCsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGNcApCsePersonsWorkSet
      {
        get => hiddenGNcApCsePersonsWorkSet ??= new();
        set => hiddenGNcApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 45;

      private NonCooperation hiddenGNcNonCooperation;
      private Common hiddenGNcCommon;
      private CaseRole hiddenGNcApCaseRole;
      private CsePersonsWorkSet hiddenGNcApCsePersonsWorkSet;
    }

    /// <summary>
    /// Gets a value of Nc.
    /// </summary>
    [JsonIgnore]
    public Array<NcGroup> Nc => nc ??= new(NcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Nc for json serialization.
    /// </summary>
    [JsonPropertyName("nc")]
    [Computed]
    public IList<NcGroup> Nc_Json
    {
      get => nc;
      set => Nc.Assign(value);
    }

    /// <summary>
    /// A value of NoItemsNc.
    /// </summary>
    [JsonPropertyName("noItemsNc")]
    public Common NoItemsNc
    {
      get => noItemsNc ??= new();
      set => noItemsNc = value;
    }

    /// <summary>
    /// A value of CurrItmNoNc.
    /// </summary>
    [JsonPropertyName("currItmNoNc")]
    public Common CurrItmNoNc
    {
      get => currItmNoNc ??= new();
      set => currItmNoNc = value;
    }

    /// <summary>
    /// Gets a value of HiddenGrpNc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpNcGroup> HiddenGrpNc => hiddenGrpNc ??= new(
      HiddenGrpNcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpNc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpNc")]
    [Computed]
    public IList<HiddenGrpNcGroup> HiddenGrpNc_Json
    {
      get => hiddenGrpNc;
      set => HiddenGrpNc.Assign(value);
    }

    /// <summary>
    /// A value of HidNoItemsFndNc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndNc")]
    public Common HidNoItemsFndNc
    {
      get => hidNoItemsFndNc ??= new();
      set => hidNoItemsFndNc = value;
    }

    /// <summary>
    /// A value of NcMinus.
    /// </summary>
    [JsonPropertyName("ncMinus")]
    public Standard NcMinus
    {
      get => ncMinus ??= new();
      set => ncMinus = value;
    }

    /// <summary>
    /// A value of NcPlus.
    /// </summary>
    [JsonPropertyName("ncPlus")]
    public Standard NcPlus
    {
      get => ncPlus ??= new();
      set => ncPlus = value;
    }

    /// <summary>
    /// A value of Nc1.
    /// </summary>
    [JsonPropertyName("nc1")]
    public Standard Nc1
    {
      get => nc1 ??= new();
      set => nc1 = value;
    }

    private Array<NcGroup> nc;
    private Common noItemsNc;
    private Common currItmNoNc;
    private Array<HiddenGrpNcGroup> hiddenGrpNc;
    private Common hidNoItemsFndNc;
    private Standard ncMinus;
    private Standard ncPlus;
    private Standard nc1;
  }
#endregion
}
