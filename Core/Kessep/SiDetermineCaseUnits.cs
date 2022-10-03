// Program: SI_DETERMINE_CASE_UNITS, ID: 371727796, model: 746.
// Short name: SWE01693
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_DETERMINE_CASE_UNITS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiDetermineCaseUnits: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DETERMINE_CASE_UNITS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDetermineCaseUnits(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDetermineCaseUnits.
  /// </summary>
  public SiDetermineCaseUnits(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 11-12-96  Ken Evans		Initial Dev
    // 06-04-97  Jack Rookard          Add support for determining initial Case 
    // Unit State.
    // ------------------------------------------------------------
    // 06/22/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    local.Ar.Number = import.Ar.Number;

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF_RB";

      return;
    }

    if (import.Ap.Count == 1 && import.Ch.Count == 1)
    {
      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (Equal(import.Import1.Item.DetailCaseRole.Type1, "AP"))
        {
          local.Ap.Number = import.Import1.Item.DetailCsePersonsWorkSet.Number;
        }
        else if (Equal(import.Import1.Item.DetailCaseRole.Type1, "CH"))
        {
          local.Child.Number =
            import.Import1.Item.DetailCsePersonsWorkSet.Number;
        }

        if (!IsEmpty(local.Ap.Number) && !IsEmpty(local.Child.Number))
        {
          break;
        }
      }

      UseSpDtrCaseSrvcType();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      UseSpCreateCaseUnit();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      UseSpDtrInitialCaseUnitState();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      return;
    }

    if (import.Ap.Count == 1 && import.Ch.Count > 1)
    {
      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (Equal(import.Import1.Item.DetailCaseRole.Type1, "AP"))
        {
          local.Ap.Number = import.Import1.Item.DetailCsePersonsWorkSet.Number;

          break;
        }
      }

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (Equal(import.Import1.Item.DetailCaseRole.Type1, "CH"))
        {
          local.Child.Number =
            import.Import1.Item.DetailCsePersonsWorkSet.Number;
          UseSpDtrCaseSrvcType();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          UseSpCreateCaseUnit();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          UseSpDtrInitialCaseUnitState();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          local.Child.Number = "";
        }
      }

      return;
    }

    if (import.Ch.Count == 1 && import.Ap.Count > 1)
    {
      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (Equal(import.Import1.Item.DetailCaseRole.Type1, "CH"))
        {
          local.Child.Number =
            import.Import1.Item.DetailCsePersonsWorkSet.Number;

          break;
        }
      }

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (Equal(import.Import1.Item.DetailCaseRole.Type1, "AP"))
        {
          local.Ap.Number = import.Import1.Item.DetailCsePersonsWorkSet.Number;
          UseSpDtrCaseSrvcType();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          UseSpCreateCaseUnit();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          UseSpDtrInitialCaseUnitState();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          local.Ap.Number = "";
        }
      }

      return;
    }

    if (import.Ch.Count > 1 && import.Ap.Count > 1)
    {
      local.Local1.Index = 0;
      local.Local1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (local.Local1.IsFull)
        {
          break;
        }

        local.Local1.Update.DetailCaseRole.Type1 =
          import.Import1.Item.DetailCaseRole.Type1;
        local.Local1.Update.DetailCsePersonsWorkSet.Assign(
          import.Import1.Item.DetailCsePersonsWorkSet);
        local.Local1.Update.DetailCaseCn.Flag =
          import.Import1.Item.DetailCaseCnfrm.Flag;
        local.Local1.Update.DetailFamily.Type1 =
          import.Import1.Item.DetailFamily.Type1;
        local.Local1.Next();
      }

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (Equal(import.Import1.Item.DetailCaseRole.Type1, "CH"))
        {
          local.Child.Number =
            import.Import1.Item.DetailCsePersonsWorkSet.Number;

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (Equal(local.Local1.Item.DetailCaseRole.Type1, "AP"))
            {
              local.Ap.Number =
                local.Local1.Item.DetailCsePersonsWorkSet.Number;
              UseSpDtrCaseSrvcType();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                return;
              }

              UseSpCreateCaseUnit();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                return;
              }

              UseSpDtrInitialCaseUnitState();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                return;
              }

              local.Ap.Number = "";
            }
          }

          local.Child.Number = "";
        }
      }
    }
  }

  private static void MoveCaseUnit(CaseUnit source, CaseUnit target)
  {
    target.CuNumber = source.CuNumber;
    target.State = source.State;
  }

  private void UseSpCreateCaseUnit()
  {
    var useImport = new SpCreateCaseUnit.Import();
    var useExport = new SpCreateCaseUnit.Export();

    useImport.CaseUnit.State = local.Hold.State;
    useImport.Case1.Number = entities.Case1.Number;
    useImport.Child.Number = local.Child.Number;
    useImport.Ap.Number = local.Ap.Number;
    useImport.Ar.Number = local.Ar.Number;

    Call(SpCreateCaseUnit.Execute, useImport, useExport);

    MoveCaseUnit(useExport.CaseUnit, local.Returned);
  }

  private void UseSpDtrCaseSrvcType()
  {
    var useImport = new SpDtrCaseSrvcType.Import();
    var useExport = new SpDtrCaseSrvcType.Export();

    useImport.Case1.Number = entities.Case1.Number;

    Call(SpDtrCaseSrvcType.Execute, useImport, useExport);

    local.Hold.State = useExport.CaseUnit.State;
  }

  private void UseSpDtrInitialCaseUnitState()
  {
    var useImport = new SpDtrInitialCaseUnitState.Import();
    var useExport = new SpDtrInitialCaseUnitState.Export();

    MoveCaseUnit(local.Returned, useImport.CaseUnit);
    useImport.CsePerson.Number = local.Ap.Number;
    useImport.Case1.Number = entities.Case1.Number;

    Call(SpDtrInitialCaseUnitState.Execute, useImport, useExport);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>
      /// A value of DetailFamily.
      /// </summary>
      [JsonPropertyName("detailFamily")]
      public CaseRole DetailFamily
      {
        get => detailFamily ??= new();
        set => detailFamily = value;
      }

      /// <summary>
      /// A value of DetailCaseCnfrm.
      /// </summary>
      [JsonPropertyName("detailCaseCnfrm")]
      public Common DetailCaseCnfrm
      {
        get => detailCaseCnfrm ??= new();
        set => detailCaseCnfrm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
      private CaseRole detailFamily;
      private Common detailCaseCnfrm;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public Common Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public Common Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private CsePersonsWorkSet ar;
    private Case1 case1;
    private Common ch;
    private Common ap;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>
      /// A value of DetailFamily.
      /// </summary>
      [JsonPropertyName("detailFamily")]
      public CaseRole DetailFamily
      {
        get => detailFamily ??= new();
        set => detailFamily = value;
      }

      /// <summary>
      /// A value of DetailCaseCn.
      /// </summary>
      [JsonPropertyName("detailCaseCn")]
      public Common DetailCaseCn
      {
        get => detailCaseCn ??= new();
        set => detailCaseCn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
      private CaseRole detailFamily;
      private Common detailCaseCn;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public CaseUnit Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public CaseUnit Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    private CaseUnit returned;
    private CaseUnit hold;
    private CsePerson child;
    private CsePerson ap;
    private CsePerson ar;
    private Array<LocalGroup> local1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Case1 case1;
  }
#endregion
}
