// Program: DELETE_LEGAL_ACTION, ID: 372114105, model: 746.
// Short name: SWE00184
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DELETE_LEGAL_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process deletes LEGAL ACTION if no pertinent relationships exist.
/// </para>
/// </summary>
[Serializable]
public partial class DeleteLegalAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_LEGAL_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteLegalAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteLegalAction.
  /// </summary>
  public DeleteLegalAction(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------
    //                          M A I N T E N A N C E      L O G
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 06/15/95  Dave Allen			Initial Code
    // 03/23/98  Siraj Konkader		ZDEL cleanup
    // 12/02/99  R. Jean			Removed read eff date qualifier from Income_Source 
    // READ statement
    // 05/04/00  J. Magat	PR#94086	Added deletion of LEGAL_ACTION_CASE_ROLE and
    // 					LA_PERSON_LA_CASE_ROLE entries.
    // 10/19/10  T. Pierce			Removed reference to obsolete
    // 					Related_Legal_Action table.
    // 02/28/12  R. Mathews	CQ31970		Added read of disbursement suppression 
    // status
    // 					history. If found for J class legal action,
    // 					display error.  If found for non J class
    // 					legal action, delete the disbursement
    // 					suppression records.
    // 11/12/15  GVandy	CQ50342		Disallow delete for specific I class legal
    // 					actions if a document has been printed or an
    // 					eIWO submitted.
    // 05/10/17  GVandy	CQ48108		IV-D PEP Changes.
    // ---------------------------------------------------------------------------------------------
    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // 11/12/15  GVandy  CQ50342  Disallow delete for specific I class legal
    // actions if a document has been printed or an eIWO submitted.
    if ((Equal(entities.LegalAction.ActionTaken, "IWO") || Equal
      (entities.LegalAction.ActionTaken, "IWOMODO") || Equal
      (entities.LegalAction.ActionTaken, "ORDIWO2") || Equal
      (entities.LegalAction.ActionTaken, "IWOTERM") || Equal
      (entities.LegalAction.ActionTaken, "ORDIWOLS") || Equal
      (entities.LegalAction.ActionTaken, "ORDIWOPT")) && AsChar
      (entities.LegalAction.Classification) == 'I')
    {
      if (ReadIwoTransaction())
      {
        ExitState = "LE0000_IWO_OR_EIWO_STOPS_DELETE";

        return;
      }
    }

    if (ReadHearing())
    {
      ExitState = "LE0000_HEAR_PREVENTS_DELA";

      return;
    }

    if (ReadServiceProcess())
    {
      ExitState = "LE0000_SERV_PREVENTS_DELA";

      return;
    }

    if (ReadChildSupportWorksheet())
    {
      ExitState = "LE0000_WORKSHEET_PREVENTS_DELA";

      return;
    }

    if (ReadDiscovery())
    {
      ExitState = "LE0000_DISC_PREVENTS_DELA";

      return;
    }

    if (ReadLegalActionIncomeSource())
    {
      ExitState = "LE0000_IWO_GARN_PREVENTS_DELA";

      return;
    }

    if (ReadLegalActionResponse())
    {
      ExitState = "LE0000_LRES_PREVENTS_DELA";

      return;
    }

    if (ReadLegalActionAppeal())
    {
      ExitState = "LE0000_LAPP_PREVENTS_DELA";

      return;
    }

    if (ReadLegalActionPersonResource())
    {
      ExitState = "LE0000_LA_PERS_RESO_PREVENT_DELA";

      return;
    }

    if (ReadObligation())
    {
      ExitState = "LE0000_OBLG_PREVENTS_DELA";

      return;
    }

    if (AsChar(entities.LegalAction.Classification) == 'J')
    {
      if (ReadDisbSuppressionStatusHistory1())
      {
        ExitState = "LE0000_DISB_SUPPR_PREVENTS_DELA";

        return;
      }
    }
    else
    {
      foreach(var item in ReadDisbSuppressionStatusHistory2())
      {
        DeleteDisbSuppressionStatusHistory();
      }
    }

    // --05/10/17  GVandy CQ48108 (IV-D PEP Changes) Do not allow a legal action
    // with an EP
    //   legal detail to be end dated if paternity info for any active child on 
    // the detail
    //   is locked.
    foreach(var item in ReadLegalActionDetail1())
    {
      if (ReadCsePerson())
      {
        ExitState = "LE0000_CANNOT_DELETE_PAT_LOCKED";

        return;
      }
    }

    foreach(var item in ReadLegalActionDetail2())
    {
      foreach(var item1 in ReadLegalActionPerson2())
      {
        foreach(var item2 in ReadLaPersonLaCaseRole())
        {
          // *** Look for LEGAL_ACTION_CASE_ROLE, prior to any deletion.
          // Delete LA_PERSON_LA_CASE_ROLE first, if found.
          // Then, delete LEGAL_ACTION_CASE_ROLE.
          if (ReadLegalActionCaseRole())
          {
            DeleteLaPersonLaCaseRole();
            DeleteLegalActionCaseRole();
          }
          else
          {
            DeleteLaPersonLaCaseRole();

            continue;
          }
        }

        DeleteLegalActionPerson();
      }

      DeleteLegalActionDetail();
    }

    foreach(var item in ReadLegalActionPerson1())
    {
      foreach(var item1 in ReadLaPersonLaCaseRole())
      {
        // *** Look for LEGAL_ACTION_CASE_ROLE, prior to any deletion.
        // Delete LA_PERSON_LA_CASE_ROLE first, if found.
        // Then, delete LEGAL_ACTION_CASE_ROLE.
        if (ReadLegalActionCaseRole())
        {
          DeleteLaPersonLaCaseRole();
          DeleteLegalActionCaseRole();
        }
        else
        {
          DeleteLaPersonLaCaseRole();

          continue;
        }
      }

      DeleteLegalActionPerson();
    }

    foreach(var item in ReadCourtCaption())
    {
      DeleteCourtCaption();
    }

    DeleteLegalAction1();
  }

  private void DeleteCourtCaption()
  {
    Update("DeleteCourtCaption",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.CourtCaption.LgaIdentifier);
        db.SetInt32(command, "courtCaptionNo", entities.CourtCaption.Number);
      });
  }

  private void DeleteDisbSuppressionStatusHistory()
  {
    Update("DeleteDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });
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

  private void DeleteLegalAction1()
  {
    bool exists;

    Update("DeleteLegalAction#1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      });

    Update("DeleteLegalAction#2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      });

    exists = Read("DeleteLegalAction#3",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_LEG_ACT_APPEAL\".",
        "50001");
    }

    Update("DeleteLegalAction#4",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      });

    Update("DeleteLegalAction#5",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      });

    Update("DeleteLegalAction#6",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      });

    Update("DeleteLegalAction#7",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      });

    exists = Read("DeleteLegalAction#8",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_OBLIGATION\".", "50001");
        
    }

    Update("DeleteLegalAction#9",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
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

  private void DeleteLegalActionDetail()
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

  private bool ReadChildSupportWorksheet()
  {
    entities.ChildSupportWorksheet.Populated = false;

    return Read("ReadChildSupportWorksheet",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ChildSupportWorksheet.Identifier = db.GetInt64(reader, 0);
        entities.ChildSupportWorksheet.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.ChildSupportWorksheet.CsGuidelineYear = db.GetInt32(reader, 2);
        entities.ChildSupportWorksheet.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCourtCaption()
  {
    entities.CourtCaption.Populated = false;

    return ReadEach("ReadCourtCaption",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Child.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetDate(command, "effectiveDt", date);
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Type1 = db.GetString(reader, 1);
        entities.Child.PaternityLockInd = db.GetNullableString(reader, 2);
        entities.Child.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Child.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory1()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
      });
  }

  private IEnumerable<bool> ReadDisbSuppressionStatusHistory2()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return ReadEach("ReadDisbSuppressionStatusHistory2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);

        return true;
      });
  }

  private bool ReadDiscovery()
  {
    entities.Discovery.Populated = false;

    return Read("ReadDiscovery",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Discovery.LgaIdentifier = db.GetInt32(reader, 0);
        entities.Discovery.RequestedDate = db.GetDate(reader, 1);
        entities.Discovery.Populated = true;
      });
  }

  private bool ReadHearing()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.Populated = true;
      });
  }

  private bool ReadIwoTransaction()
  {
    entities.IwoTransaction.Populated = false;

    return Read("ReadIwoTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 1);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 2);
        entities.IwoTransaction.Populated = true;
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
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionAppeal()
  {
    entities.LegalActionAppeal.Populated = false;

    return Read("ReadLegalActionAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionAppeal.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAppeal.AplId = db.GetInt32(reader, 1);
        entities.LegalActionAppeal.LgaId = db.GetInt32(reader, 2);
        entities.LegalActionAppeal.Populated = true;
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
        entities.LegalActionCaseRole.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalActionCaseRole.Populated = true;
        CheckValid<LegalActionCaseRole>("CroType",
          entities.LegalActionCaseRole.CroType);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail1()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(command, "effectiveDt", date);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail2()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadLegalActionIncomeSource()
  {
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSource",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionIncomeSource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPerson1()
  {
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
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
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 7);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 8);
        entities.LegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPerson2",
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
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 7);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 8);
        entities.LegalActionPerson.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionPersonResource()
  {
    entities.LegalActionPersonResource.Populated = false;

    return Read("ReadLegalActionPersonResource",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.LegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 4);
        entities.LegalActionPersonResource.Populated = true;
      });
  }

  private bool ReadLegalActionResponse()
  {
    entities.LegalActionResponse.Populated = false;

    return Read("ReadLegalActionResponse",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionResponse.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionResponse.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.LegalActionResponse.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadServiceProcess()
  {
    entities.ServiceProcess.Populated = false;

    return Read("ReadServiceProcess",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ServiceProcess.ServiceDocumentType = db.GetString(reader, 1);
        entities.ServiceProcess.ServiceRequestDate = db.GetDate(reader, 2);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 3);
        entities.ServiceProcess.Populated = true;
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

    private LegalAction legalAction;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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
    /// A value of CourtCaption.
    /// </summary>
    [JsonPropertyName("courtCaption")]
    public CourtCaption CourtCaption
    {
      get => courtCaption ??= new();
      set => courtCaption = value;
    }

    /// <summary>
    /// A value of LegalActionAppeal.
    /// </summary>
    [JsonPropertyName("legalActionAppeal")]
    public LegalActionAppeal LegalActionAppeal
    {
      get => legalActionAppeal ??= new();
      set => legalActionAppeal = value;
    }

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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
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
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionResponse.
    /// </summary>
    [JsonPropertyName("legalActionResponse")]
    public LegalActionResponse LegalActionResponse
    {
      get => legalActionResponse ??= new();
      set => legalActionResponse = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
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

    private CsePerson child;
    private IwoTransaction iwoTransaction;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private LegalActionDetail legalActionDetail;
    private CourtCaption courtCaption;
    private LegalActionAppeal legalActionAppeal;
    private Obligation obligation;
    private LegalAction legalAction;
    private Hearing hearing;
    private ServiceProcess serviceProcess;
    private ChildSupportWorksheet childSupportWorksheet;
    private Discovery discovery;
    private LegalActionPerson legalActionPerson;
    private LegalActionIncomeSource legalActionIncomeSource;
    private LegalActionResponse legalActionResponse;
    private LegalActionPersonResource legalActionPersonResource;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
  }
#endregion
}
