// Program: LE_LDET_LIST_ONLY_RELVNT_CROLES, ID: 371993427, model: 746.
// Short name: SWE02212
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
/// A program: LE_LDET_LIST_ONLY_RELVNT_CROLES.
/// </para>
/// <para>
/// RESP: LEGAL
/// This action block is a clone of LE_LOPS_LIST_ONLY_RELVNT_CROLES which has 
/// been modified to return a count of all Legal Action Supported Persons for
/// LDET.
/// </para>
/// </summary>
[Serializable]
public partial class LeLdetListOnlyRelvntCroles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LDET_LIST_ONLY_RELVNT_CROLES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLdetListOnlyRelvntCroles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLdetListOnlyRelvntCroles.
  /// </summary>
  public LeLdetListOnlyRelvntCroles(IContext context, Import import,
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
    // *********************************************
    // RCG	02/25/98	Priority # 19 issues
    // Count number of supported persons to compare with results from CAB 
    // reading for deactivated obligations.  When all supported persons are end-
    // dated, LOPS and LDET record End Dates are unprotected.
    // *********************************************
    // ---------------------------------------------
    // Currently this does not check for case_role effective and expiry dates. 
    // If we check that, we may not be able to see it at a future point after
    // the case_role is expired.
    // The program needs to list all the case roles as well as all the 
    // legal_action_person records !!
    // ---------------------------------------------
    export.CountTotalSupported.Count = 0;

    if (!ReadLegalActionDetailLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // ------------------------------------------------------------
    // Get all case related CSE Persons related to the legal action via legal 
    // action case role
    // ------------------------------------------------------------
    foreach(var item in ReadLegalActionPersonCsePersonLaPersonLaCaseRole())
    {
      if (AsChar(entities.LegalActionPerson.AccountType) == 'S')
      {
        ++export.CountTotalSupported.Count;
      }
    }

    // --- Now list the non cse persons if any exist.
    foreach(var item in ReadLegalActionPersonCsePerson())
    {
      foreach(var item1 in ReadLaPersonLaCaseRole())
      {
        // --- This cse person is not a non cse person. So skip it.
        goto ReadEach;
      }

      if (AsChar(entities.LegalActionPerson.AccountType) == 'S')
      {
        ++export.CountTotalSupported.Count;
      }

ReadEach:
      ;
    }
  }

  private IEnumerable<bool> ReadLaPersonLaCaseRole()
  {
    entities.LaPersonLaCaseRole.Populated = false;

    return ReadEach("ReadLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lapId", entities.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.LaPersonLaCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);

        return true;
      });
  }

  private bool ReadLegalActionDetailLegalAction()
  {
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionDetailLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 4);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 8);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CsePerson.Type1 = db.GetString(reader, 11);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonCsePersonLaPersonLaCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalActionPerson.Populated = false;
    entities.LaPersonLaCaseRole.Populated = false;
    entities.LegalActionCaseRole.Populated = false;

    return ReadEach("ReadLegalActionPersonCsePersonLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 4);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 7);
        entities.LegalActionPerson.ArrearsAmount =
          db.GetNullableDecimal(reader, 8);
        entities.LegalActionPerson.CurrentAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionPerson.JudgementAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CsePerson.Type1 = db.GetString(reader, 11);
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 12);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 13);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 13);
        entities.CaseRole.Identifier = db.GetInt32(reader, 13);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 14);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 14);
        entities.CaseRole.Type1 = db.GetString(reader, 14);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 15);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 15);
        entities.CaseRole.CspNumber = db.GetString(reader, 15);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 16);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 16);
        entities.CaseRole.CasNumber = db.GetString(reader, 16);
        entities.Case1.Number = db.GetString(reader, 16);
        entities.Case1.Number = db.GetString(reader, 16);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 17);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 17);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 18);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 19);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 20);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 21);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;
        entities.LaPersonLaCaseRole.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CountTotalSupported.
    /// </summary>
    [JsonPropertyName("countTotalSupported")]
    public Common CountTotalSupported
    {
      get => countTotalSupported ??= new();
      set => countTotalSupported = value;
    }

    private Common countTotalSupported;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
  }
#endregion
}
