// Program: FN_CAB_CHECK_PERS_OBLR_IN_LACT, ID: 372418901, model: 746.
// Short name: SWE02075
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_CHECK_PERS_OBLR_IN_LACT.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block checks if the cse person is an obligor in a legal action 
/// given the standard number.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabCheckPersOblrInLact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_CHECK_PERS_OBLR_IN_LACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabCheckPersOblrInLact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabCheckPersOblrInLact.
  /// </summary>
  public FnCabCheckPersOblrInLact(IContext context, Import import, Export export)
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
    // ================================================
    // 01/26/00  H00085703  PPhinney  Changed read to use Case-role
    // instead of account-type
    // 03/28/00  P. Phinney  H00091186 -  Add code to verify if Person is on ANY
    // Legal Action with supplied STANDARD NUMBER.
    // ================================================
    export.PersObligorInLact.Flag = "N";

    if (!ReadCsePerson())
    {
      ExitState = "ZD_CSE_PERSON_NF_3";

      return;
    }

    if (import.LegalAction.Identifier != 0)
    {
      if (!ReadLegalAction())
      {
        ExitState = "ZD_LEGAL_ACTION_NF_3";

        return;
      }

      // ================================================
      // 01/26/00  H00085703  PPhinney  Changed read to use Case-role
      // instead of account-type
      // ================================================
      if (ReadCaseRoleLegalActionCaseRole())
      {
        export.PersObligorInLact.Flag = "Y";

        return;
      }

      return;
    }

    // ================================================
    // 01/26/00  H00085703  PPhinney  Changed read to use Case-role
    // instead of account-type
    // 03/28/00  P. Phinney  H00091186 -  Add code to verify if Person is on ANY
    // Legal Action with supplied STANDARD NUMBER.   Classification = "J"
    // ================================================
    if (ReadCaseRoleLegalActionCaseRoleLegalAction())
    {
      export.PersObligorInLact.Flag = "Y";
    }
  }

  private bool ReadCaseRoleLegalActionCaseRole()
  {
    entities.LegalActionCaseRole.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 5);
        entities.LegalActionCaseRole.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
      });
  }

  private bool ReadCaseRoleLegalActionCaseRoleLegalAction()
  {
    entities.LegalActionCaseRole.Populated = false;
    entities.CaseRole.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadCaseRoleLegalActionCaseRoleLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 5);
        entities.LegalAction.Classification = db.GetString(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalActionCaseRole.Populated = true;
        entities.CaseRole.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
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

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson csePerson;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PersObligorInLact.
    /// </summary>
    [JsonPropertyName("persObligorInLact")]
    public Common PersObligorInLact
    {
      get => persObligorInLact ??= new();
      set => persObligorInLact = value;
    }

    private Common persObligorInLact;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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

    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }
#endregion
}
