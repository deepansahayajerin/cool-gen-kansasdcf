// Program: DELETE_LEGAL_ACTION_DETAIL, ID: 371993419, model: 746.
// Short name: SWE00185
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DELETE_LEGAL_ACTION_DETAIL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process deletes a specific occurrence of Legal Action Detail.
/// </para>
/// </summary>
[Serializable]
public partial class DeleteLegalActionDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_LEGAL_ACTION_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteLegalActionDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteLegalActionDetail.
  /// </summary>
  public DeleteLegalActionDetail(IContext context, Import import, Export export):
    
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
    // 05/30/95	Dave Allen			Initial Code
    // 12/03/96	Govind		IDCR 254	Modified to delete LA PERSON LA CASE ROLE and 
    // associated entities
    // 03/23/98	Siraj Konkader		ZDEL cleanup
    // ------------------------------------------------------------
    if (ReadObligationTransaction())
    {
      ExitState = "LE0000_DEBT_PREVENTS_DELETION";
    }
    else if (ReadLegalActionDetail())
    {
      foreach(var item in ReadLegalActionPersonCsePerson())
      {
        foreach(var item1 in ReadLaPersonLaCaseRole())
        {
          if (ReadLegalActionCaseRole())
          {
            // -------------------
            // continue processing
            // -------------------
          }
          else
          {
            // --- Cannot happen. If it happens ..
            DeleteLaPersonLaCaseRole();

            continue;
          }

          DeleteLaPersonLaCaseRole();

          if (ReadLaPersonLaCaseRoleLegalActionPerson())
          {
            // ---------------------------------------------------------------------
            // Wait.. There is at least one more legal action person tied to
            // this legal action case role.
            // ---------------------------------------------------------------------
          }
          else
          {
            // ---------------------------------------------------------------------
            // The legal action case role has no legal action person tied to
            // it. So go ahead and delete the legal action case role.
            // ---------------------------------------------------------------------
            DeleteLegalActionCaseRole();
          }
        }

        DeleteLegalActionPerson();
      }

      if (Equal(entities.LegalActionDetail.CreatedBy, "CONVERSN"))
      {
        // ---------------------------------------------------------------
        // in very rare instances some converted records will have
        // invalid data associated to them; this is to valildate that data
        // and prevent a RI -532 error
        // ---------------------------------------------------------------
        if (IsEmpty(entities.CsePerson.Number))
        {
          ExitState = "LE0000_DETAIL_INVALID_END_DT_DET";

          return;
        }
      }
      else
      {
        // -------------------
        // continue processing
        // -------------------
      }

      DeleteLegalActionDetail1();
    }
    else
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";
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

  private void DeleteLegalActionDetail1()
  {
    Update("DeleteLegalActionDetail#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
      });

    Update("DeleteLegalActionDetail#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
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

  private bool ReadLaPersonLaCaseRoleLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);
    entities.AnotherLegalActionPerson.Populated = false;
    entities.AnotherLaPersonLaCaseRole.Populated = false;

    return Read("ReadLaPersonLaCaseRoleLegalActionPerson",
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
        entities.AnotherLaPersonLaCaseRole.Identifier = db.GetInt32(reader, 0);
        entities.AnotherLaPersonLaCaseRole.CroId = db.GetInt32(reader, 1);
        entities.AnotherLaPersonLaCaseRole.CroType = db.GetString(reader, 2);
        entities.AnotherLaPersonLaCaseRole.CspNum = db.GetString(reader, 3);
        entities.AnotherLaPersonLaCaseRole.CasNum = db.GetString(reader, 4);
        entities.AnotherLaPersonLaCaseRole.LgaId = db.GetInt32(reader, 5);
        entities.AnotherLaPersonLaCaseRole.LapId = db.GetInt32(reader, 6);
        entities.AnotherLegalActionPerson.Identifier = db.GetInt32(reader, 6);
        entities.AnotherLegalActionPerson.Populated = true;
        entities.AnotherLaPersonLaCaseRole.Populated = true;
        CheckValid<LaPersonLaCaseRole>("CroType",
          entities.AnotherLaPersonLaCaseRole.CroType);
      });
  }

  private bool ReadLegalActionCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.LaPersonLaCaseRole.Populated);
    entities.LegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LaPersonLaCaseRole.LgaId);
        db.SetString(command, "casNumber", entities.LaPersonLaCaseRole.CasNum);
        db.
          SetInt32(command, "croIdentifier", entities.LaPersonLaCaseRole.CroId);
          
        db.SetString(command, "cspNumber", entities.LaPersonLaCaseRole.CspNum);
        db.SetString(command, "croType", entities.LaPersonLaCaseRole.CroType);
      },
      (db, reader) =>
      {
        entities.LegalActionCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalActionCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalActionCaseRole.CroType = db.GetString(reader, 2);
        entities.LegalActionCaseRole.CroIdentifier = db.GetInt32(reader, 3);
        entities.LegalActionCaseRole.LgaId = db.GetInt32(reader, 4);
        entities.LegalActionCaseRole.InitialCreationInd =
          db.GetString(reader, 5);
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.CreatedBy = db.GetString(reader, 2);
        entities.LegalActionDetail.Populated = true;
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
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 3);
        entities.CsePerson.Populated = true;
        entities.LegalActionPerson.Populated = true;

        return true;
      });
  }

  private bool ReadObligationTransaction()
  {
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", import.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "ladRNumber", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 8);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
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
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of AnotherLegalActionPerson.
    /// </summary>
    [JsonPropertyName("anotherLegalActionPerson")]
    public LegalActionPerson AnotherLegalActionPerson
    {
      get => anotherLegalActionPerson ??= new();
      set => anotherLegalActionPerson = value;
    }

    /// <summary>
    /// A value of AnotherLaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("anotherLaPersonLaCaseRole")]
    public LaPersonLaCaseRole AnotherLaPersonLaCaseRole
    {
      get => anotherLaPersonLaCaseRole ??= new();
      set => anotherLaPersonLaCaseRole = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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

    private Obligation obligation;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson anotherLegalActionPerson;
    private LaPersonLaCaseRole anotherLaPersonLaCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private ObligationTransaction obligationTransaction;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
