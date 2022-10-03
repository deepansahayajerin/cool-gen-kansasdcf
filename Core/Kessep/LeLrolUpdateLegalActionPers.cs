// Program: LE_LROL_UPDATE_LEGAL_ACTION_PERS, ID: 371998951, model: 746.
// Short name: SWE00832
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LROL_UPDATE_LEGAL_ACTION_PERS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will create or update a Legal Action Person related to the
/// CSE Person related to the CSE Case number entered, and Legal Action
/// entered.
/// </para>
/// </summary>
[Serializable]
public partial class LeLrolUpdateLegalActionPers: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LROL_UPDATE_LEGAL_ACTION_PERS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLrolUpdateLegalActionPers(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLrolUpdateLegalActionPers.
  /// </summary>
  public LeLrolUpdateLegalActionPers(IContext context, Import import,
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
    // 10/31/97	GOVIND	Modified to make identifier as random.
    // 			Moved CREATE and UPDATE to action block
    // 02/21/98	R Grey	H00030734	Remove edit to escape when
    // 			legal action person found on an ADD/UPDT
    // 			request.
    // ------------------------------------------------------------
    // ---------------------------------------------
    // Delete legal action case role if no more legal action person record is 
    // tied to it.
    // ---------------------------------------------
    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.LegalActionPerson.Assign(import.LegalActionPerson);
    MoveLegalActionPerson(import.LegalActionPerson, local.LegalActionPerson);

    if (!Lt(local.ZeroDateWorkArea.Date, local.LegalActionPerson.EndDate))
    {
      local.LegalActionPerson.EndDate = UseCabSetMaximumDiscontinueDate();
    }

    // ------------------------------------------------------------
    // Legal Action.
    // ------------------------------------------------------------
    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (!ReadCsePerson())
    {
      ExitState = "ZD_CSE_PERSON_NF_3";

      return;
    }

    if (import.LegalActionPerson.Identifier > 0)
    {
      // --- It is an update to an existing legal action person
      if (ReadLegalActionPerson3())
      {
        UseLeCabUpdateLegActPersRec();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.LegalActionPerson.Assign(local.LegalActionPerson);
      }
      else
      {
        ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";

        return;
      }
    }
    else
    {
      // --- It is a new legal action person
      if (ReadLegalActionPerson1())
      {
        // --- Legal Action Person has already been created.
        // ********************************************
        // RCG 02/20/98  Even if found, create new legal action person record.  
        // This will give unique identifier to resolve H00037578.  Remove ESCAPE
        // statement.
        // ********************************************
      }

      UseLeLrolCreateLegActPersRec();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // --- Get the currency on legal action person created. It is required 
      // later below.
      if (!ReadLegalActionPerson2())
      {
        ExitState = "CO0000_LEGAL_ACTION_PERSON_NF";

        return;
      }
    }

    // ---------------------------------------------
    // If legal action caserole is required, create one (if does not exist 
    // already).
    // ---------------------------------------------
    if (!IsEmpty(import.Case1.Number) && !IsEmpty(import.CaseRole.Type1))
    {
      if (!ReadCaseRoleCase())
      {
        ExitState = "CASE_ROLE_NF";

        return;
      }

      if (!ReadLegalActionCaseRole())
      {
        try
        {
          CreateLegalActionCaseRole();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LE0000_LEGAL_ACTION_CASE_ROLE_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      // --- Relate the Legal Action Case Role with Legal Action Person via LA 
      // PERSON LA CASE ROLE entity.
      if (ReadLaPersonLaCaseRole())
      {
        // --- already created  RCG - this will not happen, this logic is 
        // executed only when no legal action person is found.
      }
      else
      {
        try
        {
          CreateLaPersonLaCaseRole();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveLegalActionPerson(LegalActionPerson source,
    LegalActionPerson target)
  {
    target.Identifier = source.Identifier;
    target.Role = source.Role;
    target.EffectiveDate = source.EffectiveDate;
    target.EndDate = source.EndDate;
    target.EndReason = source.EndReason;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseLeCabUpdateLegActPersRec()
  {
    var useImport = new LeCabUpdateLegActPersRec.Import();
    var useExport = new LeCabUpdateLegActPersRec.Export();

    useImport.LegalActionPerson.Assign(local.LegalActionPerson);

    Call(LeCabUpdateLegActPersRec.Execute, useImport, useExport);
  }

  private void UseLeLrolCreateLegActPersRec()
  {
    var useImport = new LeLrolCreateLegActPersRec.Import();
    var useExport = new LeLrolCreateLegActPersRec.Export();

    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;
    MoveCsePerson(entities.CsePerson, useImport.CsePerson);
    useImport.LegalActionPerson.Assign(import.LegalActionPerson);

    Call(LeLrolCreateLegActPersRec.Execute, useImport, useExport);

    export.LegalActionPerson.Assign(useExport.LegalActionPerson);
  }

  private void CreateLaPersonLaCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);

    var identifier = 1;
    var croId = entities.LegalActionCaseRole.CroIdentifier;
    var croType = entities.LegalActionCaseRole.CroType;
    var cspNum = entities.LegalActionCaseRole.CspNumber;
    var casNum = entities.LegalActionCaseRole.CasNumber;
    var lgaId = entities.LegalActionCaseRole.LgaId;
    var lapId = entities.LegalActionPerson.Identifier;

    CheckValid<LaPersonLaCaseRole>("CroType", croType);
    entities.LaPersonLaCaseRole.Populated = false;
    Update("CreateLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetInt32(command, "croId", croId);
        db.SetString(command, "croType", croType);
        db.SetString(command, "cspNum", cspNum);
        db.SetString(command, "casNum", casNum);
        db.SetInt32(command, "lgaId", lgaId);
        db.SetInt32(command, "lapId", lapId);
      });

    entities.LaPersonLaCaseRole.Identifier = identifier;
    entities.LaPersonLaCaseRole.CroId = croId;
    entities.LaPersonLaCaseRole.CroType = croType;
    entities.LaPersonLaCaseRole.CspNum = cspNum;
    entities.LaPersonLaCaseRole.CasNum = casNum;
    entities.LaPersonLaCaseRole.LgaId = lgaId;
    entities.LaPersonLaCaseRole.LapId = lapId;
    entities.LaPersonLaCaseRole.Populated = true;
  }

  private void CreateLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var casNumber = entities.CaseRole.CasNumber;
    var cspNumber = entities.CaseRole.CspNumber;
    var croType = entities.CaseRole.Type1;
    var croIdentifier = entities.CaseRole.Identifier;
    var lgaId = entities.LegalAction.Identifier;
    var createdBy = global.UserId;
    var createdTstamp = local.Current.Timestamp;
    var initialCreationInd = "Y";

    CheckValid<LegalActionCaseRole>("CroType", croType);
    entities.LegalActionCaseRole.Populated = false;
    Update("CreateLegalActionCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "croIdentifier", croIdentifier);
        db.SetInt32(command, "lgaId", lgaId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "initCrInd", initialCreationInd);
      });

    entities.LegalActionCaseRole.CasNumber = casNumber;
    entities.LegalActionCaseRole.CspNumber = cspNumber;
    entities.LegalActionCaseRole.CroType = croType;
    entities.LegalActionCaseRole.CroIdentifier = croIdentifier;
    entities.LegalActionCaseRole.LgaId = lgaId;
    entities.LegalActionCaseRole.CreatedBy = createdBy;
    entities.LegalActionCaseRole.CreatedTstamp = createdTstamp;
    entities.LegalActionCaseRole.InitialCreationInd = initialCreationInd;
    entities.LegalActionCaseRole.Populated = true;
  }

  private bool ReadCaseRoleCase()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCase",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", import.CaseRole.Identifier);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.Case1.Populated = true;
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
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);
    entities.LaPersonLaCaseRole.Populated = false;

    return Read("ReadLaPersonLaCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalActionCaseRole.LgaId);
        db.SetString(command, "casNum", entities.LegalActionCaseRole.CasNumber);
        db.
          SetInt32(command, "croId", entities.LegalActionCaseRole.CroIdentifier);
          
        db.SetString(command, "cspNum", entities.LegalActionCaseRole.CspNumber);
        db.SetString(command, "croType", entities.LegalActionCaseRole.CroType);
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
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.LegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 7);
        entities.LegalActionCaseRole.Populated = true;
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
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 6);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 8);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", export.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 6);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 8);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson3()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson3",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", import.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 6);
        entities.LegalActionPerson.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.LegalActionPerson.CreatedBy = db.GetString(reader, 8);
        entities.LegalActionPerson.Populated = true;
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
    /// A value of LaCaseroleCreatedInd.
    /// </summary>
    [JsonPropertyName("laCaseroleCreatedInd")]
    public Standard LaCaseroleCreatedInd
    {
      get => laCaseroleCreatedInd ??= new();
      set => laCaseroleCreatedInd = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private Standard laCaseroleCreatedInd;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalActionPerson legalActionPerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private LegalActionPerson legalActionPerson;
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

    /// <summary>
    /// A value of ZeroDateWorkArea.
    /// </summary>
    [JsonPropertyName("zeroDateWorkArea")]
    public DateWorkArea ZeroDateWorkArea
    {
      get => zeroDateWorkArea ??= new();
      set => zeroDateWorkArea = value;
    }

    /// <summary>
    /// A value of ZeroCaseRole.
    /// </summary>
    [JsonPropertyName("zeroCaseRole")]
    public CaseRole ZeroCaseRole
    {
      get => zeroCaseRole ??= new();
      set => zeroCaseRole = value;
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

    private DateWorkArea current;
    private DateWorkArea zeroDateWorkArea;
    private CaseRole zeroCaseRole;
    private LegalActionPerson legalActionPerson;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    private Case1 case1;
    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private CsePerson csePerson;
  }
#endregion
}
