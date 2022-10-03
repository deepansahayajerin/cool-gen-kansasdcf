// Program: LE_CAB_COPY_LA_CASEROLES, ID: 371727798, model: 746.
// Short name: SWE01833
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
/// A program: LE_CAB_COPY_LA_CASEROLES.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block copies the Legal Action Case Role and LA Person LA 
/// Caserol records for the specified (new) cse case.
/// This is required when a case is split and a new cse case is created, the new
/// caseroles must be associated with the Legal Action records already created.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabCopyLaCaseroles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_COPY_LA_CASEROLES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabCopyLaCaseroles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabCopyLaCaseroles.
  /// </summary>
  public LeCabCopyLaCaseroles(IContext context, Import import, Export export):
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
    // Date	 By	IDCR#	Description
    // 12/20/96 govind	254	Initial code.
    // 04/29/97 A.Kinney	Changed Current_Date
    // *********************************************
    // 06/22/99  M. Lachowicz  Change property of READ
    //                         (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 07/15/99  M. Lachowicz  Change back property of one READ
    //                         (Both Select and Cursor Only)
    // ------------------------------------------------------------
    // 08/16/99  M. Lachowicz  Make whole process for all active APs
    // ------------------------------------------------------------
    // ---------------------------------------------
    // This action block copies the legal action case roles and associated 
    // entity types' occurrences for a new cse case regsitered.
    // When a new cse case is registered, the system should check if any legal 
    // actions already exist for the AP-Child and relate the new cse case with
    // those legal actions via legal action case roles. Otherwise when a child
    // moves to another household, the new cse case will not be tied to the
    // legal actions established already for the child.
    // The Effective and End dates are not checked for Legal Action Person 
    // records. We need to copy all the records irrespective of whether those
    // legal actions are valid now or not.
    // ---------------------------------------------
    local.Current.Date = Now().Date;

    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // 08/16/99 M.L Replace READ existing_ap case_role by READ EACH
    //          existing_ap case_role to process all APs.
    //          The rest of the existing code will be in the READ EACH loop.
    foreach(var item in ReadCaseRole())
    {
      // 06/22/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (!ReadCsePerson())
      {
        continue;
      }

      // --- Process for each child in the new cse case.
      foreach(var item1 in ReadCaseRoleCsePerson())
      {
        // --- Look for Legal Action Case Roles, LA Person LA Caseroles via LOPS
        // info. i.e. Legal Action --> Legal Detail --> Legal ACtion Person
        foreach(var item2 in ReadLegalActionLegalActionDetailLegalActionPerson())
          
        {
          // --- Located one instance of Legal Action - Legal Detail - Legal 
          // Action Person for the AP-Child combination. Create legal action
          // caseroles
          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadLegalActionCaseRole2())
          {
            // --- Already created.
          }
          else
          {
            // --- Create now.
            try
            {
              CreateLegalActionCaseRole1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  goto ReadEach;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadLaPersonLaCaseRole1())
          {
            // --- Already created
          }
          else
          {
            try
            {
              CreateLaPersonLaCaseRole1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  goto ReadEach;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadLegalActionCaseRole3())
          {
            // --- Already created.
          }
          else
          {
            // --- Create now.
            try
            {
              CreateLegalActionCaseRole2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  goto ReadEach;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadLaPersonLaCaseRole2())
          {
            // --- Already created
          }
          else
          {
            try
            {
              CreateLaPersonLaCaseRole2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  goto ReadEach;
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

        // --- Look for Legal Action Case Roles, LA Person LA Caseroles via LROL
        // info. i.e. Legal Action --> Legal ACtion Person
        foreach(var item2 in ReadLegalActionLegalActionPersonLegalActionPerson())
          
        {
          // --- Located one instance of Legal Action -  Legal Action Person for
          // the Petitioner/Respondent - Child combination. Create legal action
          // caseroles
          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadLegalActionCaseRole1())
          {
            // --- Already created.
          }
          else
          {
            // --- Create now.
            try
            {
              CreateLegalActionCaseRole1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  goto ReadEach;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadLaPersonLaCaseRole1())
          {
            // --- Already created
          }
          else
          {
            try
            {
              CreateLaPersonLaCaseRole1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  goto ReadEach;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadLegalActionCaseRole3())
          {
            // --- Already created.
          }
          else
          {
            // --- Create now.
            try
            {
              CreateLegalActionCaseRole2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  goto ReadEach;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // 06/22/99 M.L
          //              Change property of the following READ to generate
          //              SELECT ONLY
          if (ReadLaPersonLaCaseRole2())
          {
            // --- Already created
          }
          else
          {
            try
            {
              CreateLaPersonLaCaseRole2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  goto ReadEach;
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

ReadEach:
      ;
    }

    // 08/16/99 M.L End.
  }

  private void CreateLaPersonLaCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);

    var identifier = 1;
    var croId = entities.LegalActionCaseRole.CroIdentifier;
    var croType = entities.LegalActionCaseRole.CroType;
    var cspNum = entities.LegalActionCaseRole.CspNumber;
    var casNum = entities.LegalActionCaseRole.CasNumber;
    var lgaId = entities.LegalActionCaseRole.LgaId;
    var lapId = entities.ExistingObligorLegalActionPerson.Identifier;

    CheckValid<LaPersonLaCaseRole>("CroType", croType);
    entities.LaPersonLaCaseRole.Populated = false;
    Update("CreateLaPersonLaCaseRole1",
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

  private void CreateLaPersonLaCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);

    var identifier = 1;
    var croId = entities.LegalActionCaseRole.CroIdentifier;
    var croType = entities.LegalActionCaseRole.CroType;
    var cspNum = entities.LegalActionCaseRole.CspNumber;
    var casNum = entities.LegalActionCaseRole.CasNumber;
    var lgaId = entities.LegalActionCaseRole.LgaId;
    var lapId = entities.ExistingSupportedLegalActionPerson.Identifier;

    CheckValid<LaPersonLaCaseRole>("CroType", croType);
    entities.LaPersonLaCaseRole.Populated = false;
    Update("CreateLaPersonLaCaseRole2",
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

  private void CreateLegalActionCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAp.Populated);

    var casNumber = entities.ExistingAp.CasNumber;
    var cspNumber = entities.ExistingAp.CspNumber;
    var croType = entities.ExistingAp.Type1;
    var croIdentifier = entities.ExistingAp.Identifier;
    var lgaId = entities.LegalAction.Identifier;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var initialCreationInd = "N";

    CheckValid<LegalActionCaseRole>("CroType", croType);
    entities.LegalActionCaseRole.Populated = false;
    Update("CreateLegalActionCaseRole1",
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

  private void CreateLegalActionCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingChild.Populated);

    var casNumber = entities.ExistingChild.CasNumber;
    var cspNumber = entities.ExistingChild.CspNumber;
    var croType = entities.ExistingChild.Type1;
    var croIdentifier = entities.ExistingChild.Identifier;
    var lgaId = entities.LegalAction.Identifier;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var initialCreationInd = "N";

    CheckValid<LegalActionCaseRole>("CroType", croType);
    entities.LegalActionCaseRole.Populated = false;
    Update("CreateLegalActionCaseRole2",
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

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.ExistingAp.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAp.CasNumber = db.GetString(reader, 0);
        entities.ExistingAp.CspNumber = db.GetString(reader, 1);
        entities.ExistingAp.Type1 = db.GetString(reader, 2);
        entities.ExistingAp.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAp.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingAp.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingAp.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingAp.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.ExistingChild.Populated = false;
    entities.ExistingSupportedCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingChild.CasNumber = db.GetString(reader, 0);
        entities.ExistingChild.CspNumber = db.GetString(reader, 1);
        entities.ExistingSupportedCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingChild.Type1 = db.GetString(reader, 2);
        entities.ExistingChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingChild.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingChild.Populated = true;
        entities.ExistingSupportedCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingChild.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAp.Populated);
    entities.ExistingObligorCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingAp.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingObligorCsePerson.Populated = true;
      });
  }

  private bool ReadLaPersonLaCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);
    entities.LaPersonLaCaseRole.Populated = false;

    return Read("ReadLaPersonLaCaseRole1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalActionCaseRole.LgaId);
        db.SetString(command, "casNum", entities.LegalActionCaseRole.CasNumber);
        db.
          SetInt32(command, "croId", entities.LegalActionCaseRole.CroIdentifier);
          
        db.SetString(command, "cspNum", entities.LegalActionCaseRole.CspNumber);
        db.SetString(command, "croType", entities.LegalActionCaseRole.CroType);
        db.SetInt32(
          command, "lapId",
          entities.ExistingObligorLegalActionPerson.Identifier);
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

  private bool ReadLaPersonLaCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionCaseRole.Populated);
    entities.LaPersonLaCaseRole.Populated = false;

    return Read("ReadLaPersonLaCaseRole2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalActionCaseRole.LgaId);
        db.SetString(command, "casNum", entities.LegalActionCaseRole.CasNumber);
        db.
          SetInt32(command, "croId", entities.LegalActionCaseRole.CroIdentifier);
          
        db.SetString(command, "cspNum", entities.LegalActionCaseRole.CspNumber);
        db.SetString(command, "croType", entities.LegalActionCaseRole.CroType);
        db.SetInt32(
          command, "lapId",
          entities.ExistingSupportedLegalActionPerson.Identifier);
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

  private bool ReadLegalActionCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAp.Populated);
    entities.LegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetInt32(command, "croIdentifier", entities.ExistingAp.Identifier);
        db.SetString(command, "croType", entities.ExistingAp.Type1);
        db.SetString(command, "cspNumber", entities.ExistingAp.CspNumber);
        db.SetString(command, "casNumber", entities.ExistingAp.CasNumber);
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

  private bool ReadLegalActionCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAp.Populated);
    entities.LegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetInt32(command, "croIdentifier", entities.ExistingAp.Identifier);
        db.SetString(command, "croType", entities.ExistingAp.Type1);
        db.SetString(command, "cspNumber", entities.ExistingAp.CspNumber);
        db.SetString(command, "casNumber", entities.ExistingAp.CasNumber);
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

  private bool ReadLegalActionCaseRole3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingChild.Populated);
    entities.LegalActionCaseRole.Populated = false;

    return Read("ReadLegalActionCaseRole3",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.
          SetInt32(command, "croIdentifier", entities.ExistingChild.Identifier);
          
        db.SetString(command, "croType", entities.ExistingChild.Type1);
        db.SetString(command, "cspNumber", entities.ExistingChild.CspNumber);
        db.SetString(command, "casNumber", entities.ExistingChild.CasNumber);
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

  private IEnumerable<bool> ReadLegalActionLegalActionDetailLegalActionPerson()
  {
    entities.ExistingSupportedLegalActionPerson.Populated = false;
    entities.ExistingObligorLegalActionPerson.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionDetailLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber1", entities.ExistingObligorCsePerson.Number);
        db.SetNullableString(
          command, "cspNumber2", entities.ExistingSupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.ExistingObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 1);
        entities.ExistingObligorLegalActionPerson.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingObligorLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ExistingObligorLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ExistingObligorLegalActionPerson.Role =
          db.GetString(reader, 6);
        entities.ExistingObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 8);
        entities.ExistingSupportedLegalActionPerson.Identifier =
          db.GetInt32(reader, 9);
        entities.ExistingSupportedLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingSupportedLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingSupportedLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 12);
        entities.ExistingSupportedLegalActionPerson.Role =
          db.GetString(reader, 13);
        entities.ExistingSupportedLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 14);
        entities.ExistingSupportedLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingSupportedLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingSupportedLegalActionPerson.AccountType =
          db.GetNullableString(reader, 17);
        entities.ExistingSupportedLegalActionPerson.Populated = true;
        entities.ExistingObligorLegalActionPerson.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionLegalActionPersonLegalActionPerson()
  {
    entities.ExistingSupportedLegalActionPerson.Populated = false;
    entities.ExistingObligorLegalActionPerson.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionLegalActionPersonLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber1", entities.ExistingObligorCsePerson.Number);
        db.SetNullableString(
          command, "cspNumber2", entities.ExistingSupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingObligorLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingObligorLegalActionPerson.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingObligorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingObligorLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingObligorLegalActionPerson.Role =
          db.GetString(reader, 4);
        entities.ExistingObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.ExistingObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 8);
        entities.ExistingSupportedLegalActionPerson.Identifier =
          db.GetInt32(reader, 9);
        entities.ExistingSupportedLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingSupportedLegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingSupportedLegalActionPerson.EffectiveDate =
          db.GetDate(reader, 12);
        entities.ExistingSupportedLegalActionPerson.Role =
          db.GetString(reader, 13);
        entities.ExistingSupportedLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 14);
        entities.ExistingSupportedLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingSupportedLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 16);
        entities.ExistingSupportedLegalActionPerson.AccountType =
          db.GetNullableString(reader, 17);
        entities.ExistingSupportedLegalActionPerson.Populated = true;
        entities.ExistingObligorLegalActionPerson.Populated = true;
        entities.LegalAction.Populated = true;

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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Case1 case1;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CreatedUsingLops.
    /// </summary>
    [JsonPropertyName("createdUsingLops")]
    public Common CreatedUsingLops
    {
      get => createdUsingLops ??= new();
      set => createdUsingLops = value;
    }

    private DateWorkArea current;
    private Common createdUsingLops;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingSupportedLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingSupportedLegalActionPerson")]
    public LegalActionPerson ExistingSupportedLegalActionPerson
    {
      get => existingSupportedLegalActionPerson ??= new();
      set => existingSupportedLegalActionPerson = value;
    }

    /// <summary>
    /// A value of ExistingObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingObligorLegalActionPerson")]
    public LegalActionPerson ExistingObligorLegalActionPerson
    {
      get => existingObligorLegalActionPerson ??= new();
      set => existingObligorLegalActionPerson = value;
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

    /// <summary>
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CaseRole ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
    }

    /// <summary>
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CaseRole ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
    }

    /// <summary>
    /// A value of ExistingSupportedCsePerson.
    /// </summary>
    [JsonPropertyName("existingSupportedCsePerson")]
    public CsePerson ExistingSupportedCsePerson
    {
      get => existingSupportedCsePerson ??= new();
      set => existingSupportedCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingObligorCsePerson.
    /// </summary>
    [JsonPropertyName("existingObligorCsePerson")]
    public CsePerson ExistingObligorCsePerson
    {
      get => existingObligorCsePerson ??= new();
      set => existingObligorCsePerson = value;
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

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson existingSupportedLegalActionPerson;
    private LegalActionPerson existingObligorLegalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private CaseRole existingChild;
    private CaseRole existingAp;
    private CsePerson existingSupportedCsePerson;
    private CsePerson existingObligorCsePerson;
    private Case1 case1;
  }
#endregion
}
