// Program: SI_CHK_CASES_FOR_CH_WITH_DIFF_AR, ID: 372627741, model: 746.
// Short name: SWE02556
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CHK_CASES_FOR_CH_WITH_DIFF_AR.
/// </summary>
[Serializable]
public partial class SiChkCasesForChWithDiffAr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHK_CASES_FOR_CH_WITH_DIFF_AR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiChkCasesForChWithDiffAr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiChkCasesForChWithDiffAr.
  /// </summary>
  public SiChkCasesForChWithDiffAr(IContext context, Import import,
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
    // Date	  Developer		Description
    // 04-05-99 W.Campbell		Initial development.
    // ------------------------------------------------------------
    // 06/21/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only)
    // ------------------------------------------------------------
    // 03/10/00  M. Lachowicz          Send back Paternity
    //                                 
    // Established Indicator WR#
    // 000160.
    // ------------------------------------------------------------
    // ------------------------------------------------------------
    // The purpose of this CAB is to try to find
    // another Case where the import CHild
    // is active and that Case has an active
    // AR which is different from the import AR.
    // This is a condition which can exist in the
    // Data Base, but needs to be fixed so that
    // all the cases where a CH is active (Max=2)
    // have the same AR.
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;

    // ------------------------------------------------------------
    // See if person is an active child on an existing case.
    // ------------------------------------------------------------
    // 21/06/99 M.L Start
    //              Change property of the following READ to generate
    //              select only
    if (ReadCsePerson2())
    {
      // 03/10/00 M.L Start
      MoveCsePerson(entities.ExistingCh, export.ChPaternityEstInd);

      // 03/10/00 M.L End
    }
    else
    {
      return;
    }

    // 21/06/99 M.L End
    foreach(var item in ReadCaseRoleCase())
    {
      // ------------------------------------------------------------
      // A case exist where this child is active.
      // Now see if the AR is different.
      // ------------------------------------------------------------
      // 21/06/99 M.L Start
      //              Change property of the following READ to generate
      //              cursor only
      if (ReadCsePerson1())
      {
        if (!Equal(entities.ExistingAr.Number, import.Ar.Number))
        {
          // ------------------------------------------------------------
          // The AR is different.  Export the case number
          // when this condition is found.
          // ------------------------------------------------------------
          export.Case1.Number = entities.ExistingCase.Number;

          return;
        }
      }
      else
      {
        // ------------------------------------------------------------
        // Case should not exist without an active AR?
        // ------------------------------------------------------------
        export.Case1.Number = entities.ExistingCase.Number;
        ExitState = "AR_NF_ON_OTHER_CASE_WITH_CH";

        return;
      }

      // 21/06/99 M.L End
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.PaternityEstablishedIndicator = source.PaternityEstablishedIndicator;
  }

  private IEnumerable<bool> ReadCaseRoleCase()
  {
    entities.Child.Populated = false;
    entities.ExistingCase.Populated = false;

    return ReadEach("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCh.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.StartDate = db.GetNullableDate(reader, 4);
        entities.Child.EndDate = db.GetNullableDate(reader, 5);
        entities.Child.RelToAr = db.GetNullableString(reader, 6);
        entities.Child.Populated = true;
        entities.ExistingCase.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingAr.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAr.Number = db.GetString(reader, 0);
        entities.ExistingAr.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingCh.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ch.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCh.Number = db.GetString(reader, 0);
        entities.ExistingCh.Type1 = db.GetString(reader, 1);
        entities.ExistingCh.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 2);
        entities.ExistingCh.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCh.Type1);
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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

    private CsePerson ar;
    private CsePerson ch;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ChPaternityEstInd.
    /// </summary>
    [JsonPropertyName("chPaternityEstInd")]
    public CsePerson ChPaternityEstInd
    {
      get => chPaternityEstInd ??= new();
      set => chPaternityEstInd = value;
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

    private CsePerson chPaternityEstInd;
    private Case1 case1;
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
    /// A value of ExistingCh.
    /// </summary>
    [JsonPropertyName("existingCh")]
    public CsePerson ExistingCh
    {
      get => existingCh ??= new();
      set => existingCh = value;
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
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
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

    private CsePerson existingAr;
    private CsePerson existingCh;
    private CaseRole child;
    private Case1 existingCase;
    private CaseRole existingApplicantRecipient;
  }
#endregion
}
