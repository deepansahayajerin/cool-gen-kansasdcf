// Program: LE_DELETE_LEGAL_ROLE_AND_PERSON, ID: 371998944, model: 746.
// Short name: SWE00758
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DELETE_LEGAL_ROLE_AND_PERSON.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will Delete the Legal Action Case Role related to the CSE 
/// Person for the CSE Case number entered, and Legal Action entered. If that is
/// the last case role associated with the legal action person, it will delete
/// the legal action person.
/// </para>
/// </summary>
[Serializable]
public partial class LeDeleteLegalRoleAndPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DELETE_LEGAL_ROLE_AND_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDeleteLegalRoleAndPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDeleteLegalRoleAndPerson.
  /// </summary>
  public LeDeleteLegalRoleAndPerson(IContext context, Import import,
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
    // Date		Developer	Request #	Description
    // 05/22/95	Dave Allen			Initial Code
    // 11/27/96	govind		IDCR 254	Modified to delete legal action case role not 
    // related to any legal action person
    // 12/21/98 P. Sharp    Removed entity action view of legal_action. Marked 
    // import view of legal_action for delete.
    // ------------------------------------------------------------
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!IsEmpty(import.Case1.Number) && !IsEmpty(import.CaseRole.Type1))
    {
      if (!ReadCase())
      {
        ExitState = "CASE_NF";

        return;
      }

      if (!ReadCaseRole())
      {
        ExitState = "CASE_ROLE_NF";

        return;
      }
    }

    if (ReadLegalActionPerson1())
    {
      if (IsEmpty(import.Case1.Number) && IsEmpty(import.CaseRole.Type1))
      {
        // --- This is a non-case related person.
        DeleteLegalActionPerson();
      }
      else
      {
        if (ReadLaPersonLaCaseRoleLegalActionCaseRole())
        {
          DeleteLaPersonLaCaseRole();

          if (ReadLegalActionPerson2())
          {
            // --- This legal action case role is tied to another legal action 
            // person record. So it cannot be deleted.
          }
          else
          {
            // --- The legal action case role is tied only to this legal action 
            // person record and no (an)other legal action person record. So
            // delete the legal action case role record before deleting the
            // legal action person record. (Otherwise that legal action case
            // role record will remain orphan and result in unpredictable
            // results in other procedures)
            DeleteLegalActionCaseRole();
          }
        }

        if (ReadLaPersonLaCaseRole())
        {
          // --- There is at least one (more) Legal Action Case Role associated 
          // with the Legal Action Person. So this is not the last cse case role
          // for the legal action person. Leave the legal action person as it
          // is.
        }
        else
        {
          // --- No more cse case roles associated with the legal action person.
          // So go ahead and delete the legal action person.
          DeleteLegalActionPerson();
        }
      }
    }
    else
    {
      ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";
    }
  }

  private void DeleteLaPersonLaCaseRole()
  {
    Update("DeleteLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.LaPersonLaCaseRole.Identifier);
        db.SetInt32(command, "croId", entities.LaPersonLaCaseRole.CroId);
        db.SetString(command, "croType", entities.LaPersonLaCaseRole.CroType);
        db.SetString(command, "cspNum", entities.LaPersonLaCaseRole.CspNum);
        db.SetString(command, "casNum", entities.LaPersonLaCaseRole.CasNum);
        db.SetInt32(command, "lgaId", entities.LaPersonLaCaseRole.LgaId);
        db.SetInt32(command, "lapId", entities.LaPersonLaCaseRole.LapId);
      });
  }

  private void DeleteLegalActionCaseRole()
  {
    Update("DeleteLegalActionCaseRole#1",
      (db, command) =>
      {
        db.SetString(command, "casNum", entities.LegalActionCaseRole.CasNumber);
        db.SetString(command, "cspNum", entities.LegalActionCaseRole.CspNumber);
        db.SetString(command, "croType", entities.LegalActionCaseRole.CroType);
        db.
          SetInt32(command, "croId", entities.LegalActionCaseRole.CroIdentifier);
          
        db.SetInt32(command, "lgaId", entities.LegalActionCaseRole.LgaId);
      });

    Update("DeleteLegalActionCaseRole#2",
      (db, command) =>
      {
        db.SetString(command, "casNum", entities.LegalActionCaseRole.CasNumber);
        db.SetString(command, "cspNum", entities.LegalActionCaseRole.CspNumber);
        db.SetString(command, "croType", entities.LegalActionCaseRole.CroType);
        db.
          SetInt32(command, "croId", entities.LegalActionCaseRole.CroIdentifier);
          
        db.SetInt32(command, "lgaId", entities.LegalActionCaseRole.LgaId);
      });
  }

  private void DeleteLegalActionPerson()
  {
    Update("DeleteLegalActionPerson#1",
      (db, command) =>
      {
        db.SetInt32(command, "lapId", entities.LegalActionPerson.Identifier);
      });

    Update("DeleteLegalActionPerson#2",
      (db, command) =>
      {
        db.SetInt32(command, "lapId", entities.LegalActionPerson.Identifier);
      });
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

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", import.CaseRole.Identifier);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadLaPersonLaCaseRole()
  {
    entities.LaPersonLaCaseRole.Populated = false;

    return Read("ReadLaPersonLaCaseRole",
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
      });
  }

  private bool ReadLaPersonLaCaseRoleLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LaPersonLaCaseRole.Populated = false;
    entities.LegalActionCaseRole.Populated = false;

    return Read("ReadLaPersonLaCaseRoleLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lapId", entities.LegalActionPerson.Identifier);
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.LaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 1);
        entities.LaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 3);
        entities.LaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 4);
        entities.LaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.LaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 7);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.LaPersonLaCaseRole.Populated = true;
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.LaPersonLaCaseRole.CroType);
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
      });
  }

  private bool ReadLegalActionPerson1()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", import.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);
    entities.ExistingAnotherLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalActionCaseRole.LgaId);
        db.SetString(command, "casNum", entities.LegalActionCaseRole.CasNumber);
        db.
          SetInt32(command, "croId", entities.LegalActionCaseRole.CroIdentifier);
          
        db.SetString(command, "cspNum", entities.LegalActionCaseRole.CspNumber);
        db.SetString(command, "croType", entities.LegalActionCaseRole.CroType);
        db.
          SetInt32(command, "laPersonId", entities.LegalActionPerson.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingAnotherLegalActionPerson.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingAnotherLegalActionPerson.Populated = true;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public LegalAction Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CaseRole caseRole;
    private LegalActionPerson legalActionPerson;
    private LegalAction zdel;
    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
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
    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public CaseRole Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    private CaseRole zero;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingAnotherLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingAnotherLegalActionPerson")]
    public LegalActionPerson ExistingAnotherLegalActionPerson
    {
      get => existingAnotherLegalActionPerson ??= new();
      set => existingAnotherLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ExistingAnotherLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("existingAnotherLaPersonLaCaseRole")]
    public LaPersonLaCaseRole ExistingAnotherLaPersonLaCaseRole
    {
      get => existingAnotherLaPersonLaCaseRole ??= new();
      set => existingAnotherLaPersonLaCaseRole = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private LegalActionPerson existingAnotherLegalActionPerson;
    private LaPersonLaCaseRole existingAnotherLaPersonLaCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson legalActionPerson;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
  }
#endregion
}
