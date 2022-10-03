// Program: LE_LROL_CHECK_FOR_PATERNITY, ID: 374381929, model: 746.
// Short name: SWE00836
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
/// A program: LE_LROL_CHECK_FOR_PATERNITY.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block validates if legal case roles can be added.  Addition of a
/// legal case role depends on whether the child(ren)'s paternity has been
/// established.
/// </para>
/// </summary>
[Serializable]
public partial class LeLrolCheckForPaternity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LROL_CHECK_FOR_PATERNITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLrolCheckForPaternity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLrolCheckForPaternity.
  /// </summary>
  public LeLrolCheckForPaternity(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // This AB validates:
    // If a male AP's legal case role is being added, all the children's 
    // paternity must be established first, if not already.
    // -------------------------------------------------------------------
    // Date      Developer	Description
    // 03/15/00  Jess Magat	WR# 000160-M - Initial code.
    // 05/10/17  GVandy	CQ48108 IV-D PEP Changes.
    // -------------------------------------------------------------------
    import.LegalActionPerson.Index = 0;

    for(var limit = import.LegalActionPerson.Count; import
      .LegalActionPerson.Index < limit; ++import.LegalActionPerson.Index)
    {
      if (!import.LegalActionPerson.CheckSize())
      {
        break;
      }

      // *** Check for existing cse_person with defined role.
      if (Lt("0000000000",
        import.LegalActionPerson.Item.CsePersonsWorkSet.Number) && !
        IsEmpty(import.LegalActionPerson.Item.LegalActionPerson1.Role))
      {
        if (IsEmpty(export.Cpat.Number) && !
          IsEmpty(import.LegalActionPerson.Item.Common.SelectChar))
        {
          // *** Set a Default Case# for CPAT.
          export.Cpat.Number =
            import.LegalActionPerson.Item.DetailDisplayed.Number;
        }

        if (Equal(import.LegalActionPerson.Item.CaseRole.Type1, "AP"))
        {
          UseSiReadCsePerson();

          // *** Check for gender of AP.
          if (AsChar(local.CsePersonsWorkSet.Sex) == 'M')
          {
            ++local.ApMale.Count;
          }
          else
          {
            ++local.ApFemale.Count;
          }
        }

        if (Equal(import.LegalActionPerson.Item.CaseRole.Type1, "CH"))
        {
          // *** Check if child's paternity has been established.
          if (ReadCsePerson())
          {
            // --05/10/17 GVandy CQ48108 (IV-D PEP Changes)  Add check if 
            // paternity is established but not locked.
            if (AsChar(entities.Child.BornOutOfWedlock) == 'U' || AsChar
              (entities.Child.CseToEstblPaternity) == 'U' || AsChar
              (entities.Child.PaternityEstablishedIndicator) == 'Y' && AsChar
              (entities.Child.PaternityLockInd) != 'Y')
            {
              // --05/10/17 GVandy CQ48108 (IV-D PEP Changes)  Skip if this case
              // has previously been sent to CPAT.
              for(import.ExpPrevCpatCases.Index = 0; import
                .ExpPrevCpatCases.Index < import.ExpPrevCpatCases.Count; ++
                import.ExpPrevCpatCases.Index)
              {
                if (!import.ExpPrevCpatCases.CheckSize())
                {
                  break;
                }

                if (Equal(import.ExpPrevCpatCases.Item.GimpExpCpat.Number,
                  import.LegalActionPerson.Item.DetailDisplayed.Number))
                {
                  goto Test;
                }
              }

              import.ExpPrevCpatCases.CheckIndex();
              ++local.UnestablishedCpat.Count;

              if (local.UnestablishedCpat.Count == 1)
              {
                // *** Save first encountered Case# with
                // unestablished child paternity.
                export.Cpat.Number =
                  import.LegalActionPerson.Item.DetailDisplayed.Number;

                // --05/10/17 GVandy CQ48108 (IV-D PEP Changes)  Add this case 
                // to group indicating it has been sent to CPAT.
                import.ExpPrevCpatCases.Index = import.ExpPrevCpatCases.Count;
                import.ExpPrevCpatCases.CheckSize();

                import.ExpPrevCpatCases.Update.GimpExpCpat.Number =
                  import.LegalActionPerson.Item.DetailDisplayed.Number;
              }
            }
          }
          else
          {
            // *** Not Possible.
          }
        }
      }

Test:
      ;
    }

    import.LegalActionPerson.CheckIndex();

    if (local.ApMale.Count == 0)
    {
      // ** If no male AP(s), no child paternity check validation required.
      return;
    }

    if (local.UnestablishedCpat.Count == 0)
    {
      // *** All children have paternity established.
    }
    else
    {
      // *** At least one child has unestablished/unknown paternity.
      // Set error message requiring User to go to CPAT.
      ExitState = "SI0000_INV_PATERNITY_IND_COMB";
    }
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      import.LegalActionPerson.Item.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Sex = useExport.CsePersonsWorkSet.Sex;
  }

  private bool ReadCsePerson()
  {
    entities.Child.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          import.LegalActionPerson.Item.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Type1 = db.GetString(reader, 1);
        entities.Child.BornOutOfWedlock = db.GetNullableString(reader, 2);
        entities.Child.CseToEstblPaternity = db.GetNullableString(reader, 3);
        entities.Child.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 4);
        entities.Child.PaternityLockInd = db.GetNullableString(reader, 5);
        entities.Child.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Child.Type1);
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
    /// <summary>A LegalActionPersonGroup group.</summary>
    [Serializable]
    public class LegalActionPersonGroup
    {
      /// <summary>
      /// A value of DetailDisplayed.
      /// </summary>
      [JsonPropertyName("detailDisplayed")]
      public Case1 DetailDisplayed
      {
        get => detailDisplayed ??= new();
        set => detailDisplayed = value;
      }

      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of CaseRole.
      /// </summary>
      [JsonPropertyName("caseRole")]
      public CaseRole CaseRole
      {
        get => caseRole ??= new();
        set => caseRole = value;
      }

      /// <summary>
      /// A value of LegalActionPerson1.
      /// </summary>
      [JsonPropertyName("legalActionPerson1")]
      public LegalActionPerson LegalActionPerson1
      {
        get => legalActionPerson1 ??= new();
        set => legalActionPerson1 = value;
      }

      /// <summary>
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 60;

      private Case1 detailDisplayed;
      private Common common;
      private CaseRole caseRole;
      private LegalActionPerson legalActionPerson1;
      private CsePersonsWorkSet csePersonsWorkSet;
    }

    /// <summary>A ExpPrevCpatCasesGroup group.</summary>
    [Serializable]
    public class ExpPrevCpatCasesGroup
    {
      /// <summary>
      /// A value of GimpExpCpat.
      /// </summary>
      [JsonPropertyName("gimpExpCpat")]
      public Case1 GimpExpCpat
      {
        get => gimpExpCpat ??= new();
        set => gimpExpCpat = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 gimpExpCpat;
    }

    /// <summary>
    /// Gets a value of LegalActionPerson.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionPersonGroup> LegalActionPerson =>
      legalActionPerson ??= new(LegalActionPersonGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalActionPerson for json serialization.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    [Computed]
    public IList<LegalActionPersonGroup> LegalActionPerson_Json
    {
      get => legalActionPerson;
      set => LegalActionPerson.Assign(value);
    }

    /// <summary>
    /// Gets a value of ExpPrevCpatCases.
    /// </summary>
    [JsonIgnore]
    public Array<ExpPrevCpatCasesGroup> ExpPrevCpatCases =>
      expPrevCpatCases ??= new(ExpPrevCpatCasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ExpPrevCpatCases for json serialization.
    /// </summary>
    [JsonPropertyName("expPrevCpatCases")]
    [Computed]
    public IList<ExpPrevCpatCasesGroup> ExpPrevCpatCases_Json
    {
      get => expPrevCpatCases;
      set => ExpPrevCpatCases.Assign(value);
    }

    private Array<LegalActionPersonGroup> legalActionPerson;
    private Array<ExpPrevCpatCasesGroup> expPrevCpatCases;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Cpat.
    /// </summary>
    [JsonPropertyName("cpat")]
    public Case1 Cpat
    {
      get => cpat ??= new();
      set => cpat = value;
    }

    private Case1 cpat;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApMale.
    /// </summary>
    [JsonPropertyName("apMale")]
    public Common ApMale
    {
      get => apMale ??= new();
      set => apMale = value;
    }

    /// <summary>
    /// A value of ApFemale.
    /// </summary>
    [JsonPropertyName("apFemale")]
    public Common ApFemale
    {
      get => apFemale ??= new();
      set => apFemale = value;
    }

    /// <summary>
    /// A value of UnestablishedCpat.
    /// </summary>
    [JsonPropertyName("unestablishedCpat")]
    public Common UnestablishedCpat
    {
      get => unestablishedCpat ??= new();
      set => unestablishedCpat = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common apMale;
    private Common apFemale;
    private Common unestablishedCpat;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    private CsePerson child;
  }
#endregion
}
