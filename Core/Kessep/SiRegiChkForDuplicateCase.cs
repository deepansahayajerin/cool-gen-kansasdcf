// Program: SI_REGI_CHK_FOR_DUPLICATE_CASE, ID: 372711552, model: 746.
// Short name: SWE02568
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
/// A program: SI_REGI_CHK_FOR_DUPLICATE_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiRegiChkForDuplicateCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REGI_CHK_FOR_DUPLICATE_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRegiChkForDuplicateCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRegiChkForDuplicateCase.
  /// </summary>
  public SiRegiChkForDuplicateCase(IContext context, Import import,
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
    // ------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    //   Date   Developer	Description
    // -------- -------------- 
    // -------------------------------
    // 04/28/99 W.Campbell     Rewrite of this
    //                         action block to restructure
    //                         the logic so that it will test
    //                         all the input APs with the AR
    //                         and CHildren for a duplicate
    //                         case.
    // -----------------------------------------------------
    // 08/04/99 M.Lachowicz     Add extra qualification to check that CASE
    //                          role is active.
    // -----------------------------------------------------
    // 09/23/99 M.Lachowicz     Add extra qualification to check
    //                          that it doesn't exist closed duplicate
    //                          CASE.
    //                          PR #73987
    // -----------------------------------------------------
    // 11/12/99 M.Lachowicz     Check only for the same AP/AR/CH
    //                          roles
    // -----------------------------------------------------
    // *** CHECK IF THE SAME AP/AR/CH COMBINATION EXISTS IN ANY OTHER CASE.
    // 08/04/99 M.L Start
    local.Current.Date = Now().Date;

    // 08/04/99 M.L End
    export.DuplicateCase.Flag = "N";
    local.ArFound.Flag = "N";

    local.Local1.Index = 0;
    local.Local1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (local.Local1.IsFull)
      {
        break;
      }

      // ---------------------------------------------
      // 04/28/99 W.Campbell - There should only
      // be one AR on a case.  That validation logic
      // is located elsewhere.
      // ---------------------------------------------
      // ---------------------------------------------
      // 04/28/99 W.Campbell - If AR cse_person number
      // is spaces, then no need to continue with this person.
      // The same is true for the AP & CH (see below).
      // ---------------------------------------------
      if (Equal(import.Import1.Item.DetailCaseRole.Type1, "AR") && !
        IsEmpty(import.Import1.Item.DetailCsePersonsWorkSet.Number) && AsChar
        (local.ArFound.Flag) == 'N')
      {
        if (ReadCsePerson2())
        {
          local.ArFound.Flag = "Y";
        }
        else
        {
          local.ArFound.Flag = "N";
        }

        local.Local1.Next();

        continue;
      }

      // ---------------------------------------------
      // 04/28/99 W.Campbell - If the AP
      // cse_person number is not = spaces,
      // then save this AP in the local group
      // to be used later in the logic for the
      // duplicate case test.
      // ---------------------------------------------
      // ---------------------------------------------
      // 04/28/99 W.Campbell - If AP cse_person number
      // is spaces, then no need to continue with this
      // test.  The same is true for the AR & CH(see
      // above and below).
      // ---------------------------------------------
      if (Equal(import.Import1.Item.DetailCaseRole.Type1, "AP") && !
        IsEmpty(import.Import1.Item.DetailCsePersonsWorkSet.Number))
      {
        // ---------------------------------------------
        // 04/28/99 W.Campbell - Save this AP information
        // into the local group view so that it can be used
        // later in the test for duplicate case.
        // ---------------------------------------------
        local.Local1.Update.DetailCaseRole.Type1 =
          import.Import1.Item.DetailCaseRole.Type1;
        local.Local1.Update.DetailCsePersonsWorkSet.Number =
          import.Import1.Item.DetailCsePersonsWorkSet.Number;
      }

      local.Local1.Next();
    }

    if (AsChar(local.ArFound.Flag) == 'N' || local.Local1.IsEmpty)
    {
      // ---------------------------------------------
      // 04/24/99 W.Campbell - If we don't have
      // both an AR and one or more APs then
      // there is no need to continue with this test.
      // ---------------------------------------------
      return;
    }

    // ---------------------------------------------
    // 04/28/99 W.Campbell - This loop
    // perform the testing for duplicate
    // case.  However, it gets out when
    // the first duplicate case is found.
    // It does not keep looking for other
    // duplicate cases involving the
    // same AR/AP/CH combination.
    // ---------------------------------------------
    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      // ---------------------------------------------
      // 04/28/99 W.Campbell - If the AP exist
      // on cse_person, then use this AP in the
      // logic for the duplicate case test.
      // ---------------------------------------------
      if (!ReadCsePerson1())
      {
        continue;
      }

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        // 08/04/99 M.L Added to improve performance.
        if (Equal(entities.Ap.Number,
          import.Import1.Item.DetailCsePersonsWorkSet.Number))
        {
          continue;
        }

        // 08/04/99 M.L
        // ---------------------------------------------
        // 04/24/99 W.Campbell - If CH cse_person number
        // is spaces, then no need to continue with this test.
        // The same is true for the AR & AP(see above).
        // ---------------------------------------------
        if (Equal(import.Import1.Item.DetailCaseRole.Type1, "CH") && !
          IsEmpty(import.Import1.Item.DetailCsePersonsWorkSet.Number))
        {
          // 08/04/99 M.L Added extra qualifications to check that AR and AP
          //              roles are active.
          // 09/23/99 M.L Added extra qualification to make check also valid for
          // closed CASEs.
          if (ReadCase())
          {
            export.DuplicateCase.Flag = "Y";
            export.Duplicate.Number = entities.Case1.Number;

            return;
          }

          // 11/12/99 M.L Start
          // 11/12/99 M.L End
          // 09/23/99 M.L End.
        }
      }
    }
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber1",
          import.Import1.Item.DetailCsePersonsWorkSet.Number);
        db.SetString(command, "cspNumber2", entities.Ar.Number);
        db.SetString(command, "cspNumber3", entities.Ap.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", local.Local1.Item.DetailCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", import.Import1.Item.DetailCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Populated = true;
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

    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DuplicateCase.
    /// </summary>
    [JsonPropertyName("duplicateCase")]
    public Common DuplicateCase
    {
      get => duplicateCase ??= new();
      set => duplicateCase = value;
    }

    /// <summary>
    /// A value of Duplicate.
    /// </summary>
    [JsonPropertyName("duplicate")]
    public Case1 Duplicate
    {
      get => duplicate ??= new();
      set => duplicate = value;
    }

    private Common duplicateCase;
    private Case1 duplicate;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    /// <summary>
    /// A value of ArFound.
    /// </summary>
    [JsonPropertyName("arFound")]
    public Common ArFound
    {
      get => arFound ??= new();
      set => arFound = value;
    }

    private DateWorkArea current;
    private Array<LocalGroup> local1;
    private Common arFound;
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

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Case1 case1;
    private CaseRole absentParent;
    private CaseRole applicantRecipient;
    private CaseRole child;
    private CsePerson ch;
    private CsePerson ar;
    private CsePerson ap;
  }
#endregion
}
