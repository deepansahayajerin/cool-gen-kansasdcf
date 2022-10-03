// Program: SI_VERIFY_CHILD_IS_ON_A_CASE, ID: 371727791, model: 746.
// Short name: SWE01266
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
/// A program: SI_VERIFY_CHILD_IS_ON_A_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT		
/// This AB checks to see if the child is on a case and if so how many cases and
/// what the relationship is to the AR.
/// </para>
/// </summary>
[Serializable]
public partial class SiVerifyChildIsOnACase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_VERIFY_CHILD_IS_ON_A_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiVerifyChildIsOnACase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiVerifyChildIsOnACase.
  /// </summary>
  public SiVerifyChildIsOnACase(IContext context, Import import, Export export):
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
    // 11-14-96  Ken Evans		Change to read for all
    // 				active children.
    // 11/20/96  G. Lofton - MTW	Added additional check to
    // 				determine what kind of
    // 				relation to the AR.
    // 04/29/97  JeHoward - DIR        Current date fix.
    // ------------------------------------------------------------
    // 06/21/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only)
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;

    // ------------------------------------------------------------
    // See if person is an active child on an existing case.
    // ------------------------------------------------------------
    export.RelToArIsCh.Flag = "N";
    export.ActiveCaseCh.Flag = "N";
    export.InactiveCaseCh.Flag = "N";

    // M.L 06/21/99 Start   Change property of READ to generate
    //              SELECT ONLY
    if (ReadCsePerson1())
    {
      // M.L 06/21/99 End
    }
    else
    {
      return;
    }

    // M.L 06/21/99 End
    foreach(var item in ReadCaseRoleCase2())
    {
      export.Existing.Number = entities.Case1.Number;
      ++export.ActiveCaseCh.Count;
      export.ActiveCaseCh.Flag = "Y";

      // 21/06/99 M.L Start
      //              Change property of the following READ to generate
      //              Select only
      if (ReadCsePerson2())
      {
        // 21/06/99 M.L Start
        //              Change property of the following READ to generate
        //              Select only
        if (ReadMother())
        {
          export.RelToArIsCh.Flag = "Y";
        }

        // 21/06/99 M.L Start
        //              Change property of the following READ to generate
        //              Select only
        if (ReadFather())
        {
          export.RelToArIsCh.Flag = "Y";
        }
      }

      // 21/06/99 M.L End
    }

    if (export.ActiveCaseCh.Count == 0)
    {
      // ------------------------------------------------------------
      // See if person is an inactive child on an existing case.
      // ------------------------------------------------------------
      // 21/06/99 M.L Start
      //              Change property of the following READ to generate
      //              cursor only
      if (ReadCaseRoleCase1())
      {
        export.Existing.Number = entities.Case1.Number;
        export.InactiveCaseCh.Flag = "Y";
      }

      // 21/06/99 M.L End
    }
  }

  private bool ReadCaseRoleCase1()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return Read("ReadCaseRoleCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.StartDate = db.GetNullableDate(reader, 4);
        entities.Child.EndDate = db.GetNullableDate(reader, 5);
        entities.Child.RelToAr = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCase2()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return ReadEach("ReadCaseRoleCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.StartDate = db.GetNullableDate(reader, 4);
        entities.Child.EndDate = db.GetNullableDate(reader, 5);
        entities.Child.RelToAr = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingAr.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAr.Number = db.GetString(reader, 0);
        entities.ExistingAr.Populated = true;
      });
  }

  private bool ReadFather()
  {
    entities.ExistingFather.Populated = false;

    return Read("ReadFather",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ExistingAr.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFather.CasNumber = db.GetString(reader, 0);
        entities.ExistingFather.CspNumber = db.GetString(reader, 1);
        entities.ExistingFather.Type1 = db.GetString(reader, 2);
        entities.ExistingFather.Identifier = db.GetInt32(reader, 3);
        entities.ExistingFather.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingFather.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingFather.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingFather.Type1);
      });
  }

  private bool ReadMother()
  {
    entities.ExistingMother.Populated = false;

    return Read("ReadMother",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ExistingAr.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingMother.CasNumber = db.GetString(reader, 0);
        entities.ExistingMother.CspNumber = db.GetString(reader, 1);
        entities.ExistingMother.Type1 = db.GetString(reader, 2);
        entities.ExistingMother.Identifier = db.GetInt32(reader, 3);
        entities.ExistingMother.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingMother.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingMother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingMother.Type1);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CaseRole caseRole;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActiveCaseCh.
    /// </summary>
    [JsonPropertyName("activeCaseCh")]
    public Common ActiveCaseCh
    {
      get => activeCaseCh ??= new();
      set => activeCaseCh = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of InactiveCaseCh.
    /// </summary>
    [JsonPropertyName("inactiveCaseCh")]
    public Common InactiveCaseCh
    {
      get => inactiveCaseCh ??= new();
      set => inactiveCaseCh = value;
    }

    /// <summary>
    /// A value of RelToArIsCh.
    /// </summary>
    [JsonPropertyName("relToArIsCh")]
    public Common RelToArIsCh
    {
      get => relToArIsCh ??= new();
      set => relToArIsCh = value;
    }

    private Common activeCaseCh;
    private Case1 existing;
    private Common inactiveCaseCh;
    private Common relToArIsCh;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingAr.
    /// </summary>
    [JsonPropertyName("existingAr")]
    public CsePerson ExistingAr
    {
      get => existingAr ??= new();
      set => existingAr = value;
    }

    /// <summary>
    /// A value of ExistingMother.
    /// </summary>
    [JsonPropertyName("existingMother")]
    public CaseRole ExistingMother
    {
      get => existingMother ??= new();
      set => existingMother = value;
    }

    /// <summary>
    /// A value of ExistingFather.
    /// </summary>
    [JsonPropertyName("existingFather")]
    public CaseRole ExistingFather
    {
      get => existingFather ??= new();
      set => existingFather = value;
    }

    /// <summary>
    /// A value of ExistingApplicantRecipient.
    /// </summary>
    [JsonPropertyName("existingApplicantRecipient")]
    public CaseRole ExistingApplicantRecipient
    {
      get => existingApplicantRecipient ??= new();
      set => existingApplicantRecipient = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson existingAr;
    private CaseRole existingMother;
    private CaseRole existingFather;
    private CaseRole existingApplicantRecipient;
    private Case1 case1;
    private CaseRole child;
    private CsePerson csePerson;
  }
#endregion
}
