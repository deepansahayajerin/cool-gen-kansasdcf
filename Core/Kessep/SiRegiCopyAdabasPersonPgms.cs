// Program: SI_REGI_COPY_ADABAS_PERSON_PGMS, ID: 371727801, model: 746.
// Short name: SWE01670
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
/// A program: SI_REGI_COPY_ADABAS_PERSON_PGMS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiRegiCopyAdabasPersonPgms: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REGI_COPY_ADABAS_PERSON_PGMS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRegiCopyAdabasPersonPgms(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRegiCopyAdabasPersonPgms.
  /// </summary>
  public SiRegiCopyAdabasPersonPgms(IContext context, Import import,
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date    Developer       Request #       Description
    // 10/17/96  G. Lofton                       Initial Development
    // ----------------------------------------------------------
    // 10/02/98  W. Campbell   Renamed this action block from:
    //                         SI_PAR1_COPY_ADABAS_PERSON_PGMS
    //                         to:
    //                         SI_REGI_COPY_ADABAS_PERSON_PGMS,
    //                         since it no longer has anything
    //                         to do with PAR1.
    // ------------------------------------------------------------
    // 06/18/99  M. Lachowicz  Change property of READ (Select
    //                         only)
    // ------------------------------------------------------------
    // **************************************************************
    // 10/05/99   C. Ott   Problem # 76648,
    // CSE_PERSON_ACCOUNT PGM_CHG_EFFECTIVE_DATE
    // is used to trigger recomputation of distribution
    // when a program change is made.
    // *************************************************************
    // ------------------------------------------
    // 01/10/2000 W.Campbell   Changed GT or
    //                         EQ in an IF statement
    //                         to just a GT as per
    //                         Terri Studer.
    //                         Work done on PR#84135.
    //                         See note in code for where
    //                         in the code the chg was made.
    // ------------------------------------------
    // **************************************************************
    // 09/06/00   M.Lachowicz    WR # 00188.
    //                           Made changes for JJA
    // *************************************************************
    // **************************************************************
    // 02/22/01   M.Lachowicz    WR # 00188.
    //                           Made additional changes for JJA
    // *************************************************************
    // **************************************************************
    // 02/07/02   M.Lachowicz    PR # 137225.
    //                           Do not call EAB if person plays
    //                           any case role.
    // *************************************************************
    // 11/04/02  M. Lachowicz  Added  SWEIRMAD for SRC8.
    //                         Work done on PR162543.
    // ------------------------------------------------------------
    // 02/07/02 M.L Start
    if (ReadCaseRole())
    {
      return;
    }

    // 02/07/02 M.L End
    local.Current.Date = Now().Date;

    // 09/06/00 M.L Start
    switch(TrimEnd(global.TranCode))
    {
      case "SRC8":
        local.UserId.Userid = "SWEIRMAD";

        break;
      case "SR2G":
        local.UserId.Userid = "SWEIREGI";

        break;
      case "SR2H":
        local.UserId.Userid = "SWEIROLE";

        break;
      case "SRPG":
        local.UserId.Userid = "SWEIPPFX";

        break;
      default:
        break;
    }

    // 09/06/00 M.L End
    UseSiEabRetrieveAdabasPrsnPgms();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (local.Group.IsEmpty)
    {
      ExitState = "CHILD_PERSON_PROG_NF";

      return;
    }

    if (local.Group.Count > 0)
    {
      UseCabSetMaximumDiscontinueDate();

      // Set the effective date to the earliest effective date of the
      // programs to copy.  This date may be necessary to end date
      // some existing programs.
      local.Hold.EffectiveDate = local.Max.Date;

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        if (Lt(local.Group.Item.Det.ProgEffectiveDate, local.Hold.EffectiveDate))
          
        {
          local.Hold.EffectiveDate = local.Group.Item.Det.ProgEffectiveDate;
        }
      }

      local.Group.CheckIndex();

      // M.L 06/18/99 Start
      if (!ReadCsePerson())
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }

      // M.L 06/18/99 End
      // Search for programs that are already assigned and active for
      // the cse person that are either interstate programs or a non
      // adc program. Close these programs if found.
      foreach(var item in ReadProgramPersonProgram())
      {
        local.ExistingProgram.Code = entities.SearchProgram.Code;
        MovePersonProgram(entities.SearchPersonProgram,
          local.ExistingPersonProgram);

        if (Equal(local.ExistingProgram.Code, "NA") || CharAt
          (local.ExistingProgram.Code, 3) == 'I')
        {
          // M.L 06/18/99 Start   Change property of READ to generate
          //              SELECT ONLY
          if (ReadProgram2())
          {
            // M.L 06/18/99 End
            // M.L 06/18/99 Start
            //              Change generation property of READ (generate cursor 
            // only).
            if (ReadPersonProgram1())
            {
              // M.L 06/18/99 End
              if (Lt(entities.PersonProgram.EffectiveDate,
                AddDays(local.Hold.EffectiveDate, -1)))
              {
                if (Lt(AddDays(local.Hold.EffectiveDate, -1),
                  entities.PersonProgram.DiscontinueDate))
                {
                  try
                  {
                    UpdatePersonProgram();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "PERSON_PROGRAM_NU";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "PERSON_PROGRAM_PV";

                        return;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
              }
              else
              {
                // 02/21/01 M.L Start
                DeletePersonProgram();

                // 02/21/01 M.L End
              }
            }
            else
            {
              ExitState = "PERSON_PROGRAM_NF";

              return;
            }
          }
          else
          {
            ExitState = "PROGRAM_NF";

            return;
          }
        }
      }

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        local.Program.Code = local.Group.Item.Det.ProgramCode;

        switch(TrimEnd(local.Program.Code))
        {
          case "AF":
            break;
          case "CC":
            break;
          case "CI":
            break;
          case "FC":
            if (Equal(local.Group.Item.Det.SourceOfFunds, "GA"))
            {
              local.Program.Code = "NF";
            }

            if (Equal(local.Group.Item.Det.SourceOfFunds, "JJ"))
            {
              local.Program.Code = "NC";
            }

            break;
          case "FS":
            break;
          case "MA":
            break;
          case "MK":
            break;
          case "MP":
            break;
          case "MS":
            break;
          case "SI":
            break;
          default:
            continue;
        }

        if (Lt(local.Blank.Date, local.Group.Item.Det.ProgEffectiveDate))
        {
          local.PersonProgram.EffectiveDate =
            local.Group.Item.Det.ProgEffectiveDate;
        }
        else
        {
          // Effective date cannot be blank.
          continue;
        }

        if (Lt(local.Blank.Date, local.Group.Item.Det.ProgramEndDate))
        {
          local.PersonProgram.DiscontinueDate =
            local.Group.Item.Det.ProgramEndDate;
        }
        else
        {
          local.PersonProgram.DiscontinueDate = local.Max.Date;
        }

        // M.L 06/18/99 Start   Change property of READ to generate
        //              SELECT ONLY
        if (ReadProgram1())
        {
          // M.L 06/18/99 End
        }
        else
        {
          ExitState = "PROGRAM_NF";

          return;
        }

        // **************************************************************
        // 10/28/99   C. Ott   Problem # 76648, Added logic for MED TYPE.
        // *************************************************************
        if (Equal(local.Group.Item.Det.AeProgramSubtype, "EM") || Equal
          (local.Group.Item.Det.AeProgramSubtype, "WT"))
        {
          local.PersonProgram.MedType =
            local.Group.Item.Det.AeProgramSubtype ?? "";
          local.PersonProgram.MedTypeDiscontinueDate = local.Max.Date;
        }
        else
        {
          local.PersonProgram.MedType = "";
          local.PersonProgram.MedTypeDiscontinueDate = local.Blank.Date;
        }

        foreach(var item in ReadPersonProgram2())
        {
          // ----------------------------------
          // 01/10/2000 W.Campbell - Changed GT
          // or EQ in following IF statement to
          // just a GT as per Terri Studer.
          // Work done on PR#84135.
          // ----------------------------------
          if (Lt(local.PersonProgram.EffectiveDate,
            entities.PersonProgram.DiscontinueDate))
          {
            // Program is already active.
            goto Next;
          }
        }

        // 09/06/00 M.L Start
        // Set last_updated_by and created_by to name of calling module instead 
        // of CICS userid.
        // 09/06/00 M.L End
        try
        {
          CreatePersonProgram();

          if (AsChar(import.RecomputeDistribution.Flag) == 'Y')
          {
            // **************************************************************
            // 10/05/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
            // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of
            // distribution when a program change is made.
            // *************************************************************
            if (Equal(entities.Program.Code, "AF") || Equal
              (entities.Program.Code, "NF") || Equal
              (entities.Program.Code, "NA") || Equal
              (entities.Program.Code, "NC") || Equal
              (entities.Program.Code, "AFI") || Equal
              (entities.Program.Code, "FC") || Equal
              (entities.Program.Code, "FCI") || Equal
              (entities.Program.Code, "MAI") || Equal
              (entities.Program.Code, "NAI") || local.Group.Count == 1)
            {
              if (ReadCsePersonAccount())
              {
                local.CsePersonAccount.PgmChgEffectiveDate =
                  local.PersonProgram.EffectiveDate;

                try
                {
                  UpdateCsePersonAccount();
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NU";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
              else
              {
                // ***********************************************************
                // This date is only updated for Supported Persons, CSE Person 
                // Type 'S'
                // **********************************************************
              }
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PERSON_PROGRAM_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PERSON_PROGRAM_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

Next:
        ;
      }

      local.Group.CheckIndex();
    }
  }

  private static void MoveGroup1(Local.GroupGroup source,
    SiEabRetrieveAdabasPrsnPgms.Export.GroupGroup target)
  {
    target.Det.Assign(source.Det);
  }

  private static void MoveGroup2(SiEabRetrieveAdabasPrsnPgms.Export.
    GroupGroup source, Local.GroupGroup target)
  {
    target.Det.Assign(source.Det);
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiEabRetrieveAdabasPrsnPgms()
  {
    var useImport = new SiEabRetrieveAdabasPrsnPgms.Import();
    var useExport = new SiEabRetrieveAdabasPrsnPgms.Export();

    useImport.Current.Date = import.CseReferralReceived.Date;
    useImport.CsePerson.Number = import.CsePerson.Number;
    local.Group.CopyTo(useExport.Group, MoveGroup1);

    Call(SiEabRetrieveAdabasPrsnPgms.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Group, MoveGroup2);
  }

  private void CreatePersonProgram()
  {
    var cspNumber = entities.CsePerson.Number;
    var effectiveDate = local.PersonProgram.EffectiveDate;
    var discontinueDate = local.PersonProgram.DiscontinueDate;
    var createdBy = local.UserId.Userid;
    var createdTimestamp = Now();
    var changedInd = local.PersonProgram.ChangedInd ?? "";
    var changeDate = local.Blank.Date;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;
    var medTypeDiscontinueDate = local.PersonProgram.MedTypeDiscontinueDate;
    var medType = local.PersonProgram.MedType ?? "";

    entities.PersonProgram.Populated = false;
    Update("CreatePersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "status", "");
        db.SetNullableString(command, "closureReason", "");
        db.SetNullableDate(command, "assignedDate", null);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", createdTimestamp);
        db.SetNullableString(command, "changedInd", changedInd);
        db.SetNullableDate(command, "changeDate", changeDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetNullableDate(command, "medTypeDiscDate", medTypeDiscontinueDate);
        db.SetNullableString(command, "medType", medType);
      });

    entities.PersonProgram.CspNumber = cspNumber;
    entities.PersonProgram.EffectiveDate = effectiveDate;
    entities.PersonProgram.Status = "";
    entities.PersonProgram.ClosureReason = "";
    entities.PersonProgram.AssignedDate = null;
    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.CreatedBy = createdBy;
    entities.PersonProgram.CreatedTimestamp = createdTimestamp;
    entities.PersonProgram.LastUpdatedBy = createdBy;
    entities.PersonProgram.LastUpdatdTstamp = createdTimestamp;
    entities.PersonProgram.ChangedInd = changedInd;
    entities.PersonProgram.ChangeDate = changeDate;
    entities.PersonProgram.PrgGeneratedId = prgGeneratedId;
    entities.PersonProgram.MedTypeDiscontinueDate = medTypeDiscontinueDate;
    entities.PersonProgram.MedType = medType;
    entities.PersonProgram.Populated = true;
  }

  private void DeletePersonProgram()
  {
    Update("DeletePersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.PersonProgram.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.PersonProgram.PrgGeneratedId);
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
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
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.CsePersonAccount.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.CsePersonAccount.PgmChgEffectiveDate =
          db.GetNullableDate(reader, 4);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 3);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.PersonProgram.CreatedBy = db.GetString(reader, 6);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 10);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 11);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 12);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 14);
        entities.PersonProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 3);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.PersonProgram.CreatedBy = db.GetString(reader, 6);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 10);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 11);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 12);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 14);
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private bool ReadProgram1()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(command, "code", local.Program.Code);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.DistributionProgramType = db.GetString(reader, 2);
        entities.Program.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram2",
      (db, command) =>
      {
        db.SetString(command, "code", local.ExistingProgram.Code);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.DistributionProgramType = db.GetString(reader, 2);
        entities.Program.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgramPersonProgram()
  {
    entities.SearchProgram.Populated = false;
    entities.SearchPersonProgram.Populated = false;

    return ReadEach("ReadProgramPersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.Hold.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.SearchProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.SearchPersonProgram.PrgGeneratedId = db.GetInt32(reader, 0);
        entities.SearchProgram.Code = db.GetString(reader, 1);
        entities.SearchPersonProgram.CspNumber = db.GetString(reader, 2);
        entities.SearchPersonProgram.EffectiveDate = db.GetDate(reader, 3);
        entities.SearchPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.SearchPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.SearchProgram.Populated = true;
        entities.SearchPersonProgram.Populated = true;

        return true;
      });
  }

  private void UpdateCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = local.CsePersonAccount.PgmChgEffectiveDate;

    entities.CsePersonAccount.Populated = false;
    Update("UpdateCsePersonAccount",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "recompBalFromDt", pgmChgEffectiveDate);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetString(command, "type", entities.CsePersonAccount.Type1);
      });

    entities.CsePersonAccount.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonAccount.LastUpdatedTmst = lastUpdatedTmst;
    entities.CsePersonAccount.PgmChgEffectiveDate = pgmChgEffectiveDate;
    entities.CsePersonAccount.Populated = true;
  }

  private void UpdatePersonProgram()
  {
    System.Diagnostics.Debug.Assert(entities.PersonProgram.Populated);

    var discontinueDate = AddDays(local.Hold.EffectiveDate, -1);
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();

    entities.PersonProgram.Populated = false;
    Update("UpdatePersonProgram",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetString(command, "cspNumber", entities.PersonProgram.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.PersonProgram.PrgGeneratedId);
      });

    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.LastUpdatedBy = lastUpdatedBy;
    entities.PersonProgram.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.PersonProgram.Populated = true;
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
    /// A value of CseReferralReceived.
    /// </summary>
    [JsonPropertyName("cseReferralReceived")]
    public DateWorkArea CseReferralReceived
    {
      get => cseReferralReceived ??= new();
      set => cseReferralReceived = value;
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
    /// A value of RecomputeDistribution.
    /// </summary>
    [JsonPropertyName("recomputeDistribution")]
    public Common RecomputeDistribution
    {
      get => recomputeDistribution ??= new();
      set => recomputeDistribution = value;
    }

    private DateWorkArea cseReferralReceived;
    private CsePerson csePerson;
    private Common recomputeDistribution;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Det.
      /// </summary>
      [JsonPropertyName("det")]
      public InterfacePersonProgram Det
      {
        get => det ??= new();
        set => det = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private InterfacePersonProgram det;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public Security2 UserId
    {
      get => userId ??= new();
      set => userId = value;
    }

    /// <summary>
    /// A value of PgmExists.
    /// </summary>
    [JsonPropertyName("pgmExists")]
    public Common PgmExists
    {
      get => pgmExists ??= new();
      set => pgmExists = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of ExistingProgram.
    /// </summary>
    [JsonPropertyName("existingProgram")]
    public Program ExistingProgram
    {
      get => existingProgram ??= new();
      set => existingProgram = value;
    }

    /// <summary>
    /// A value of ExistingPersonProgram.
    /// </summary>
    [JsonPropertyName("existingPersonProgram")]
    public PersonProgram ExistingPersonProgram
    {
      get => existingPersonProgram ??= new();
      set => existingPersonProgram = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public PersonProgram Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    private Security2 userId;
    private Common pgmExists;
    private Array<GroupGroup> group;
    private Program program;
    private PersonProgram personProgram;
    private Program existingProgram;
    private PersonProgram existingPersonProgram;
    private DateWorkArea blank;
    private DateWorkArea max;
    private PersonProgram hold;
    private DateWorkArea current;
    private CsePersonAccount csePersonAccount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of SearchProgram.
    /// </summary>
    [JsonPropertyName("searchProgram")]
    public Program SearchProgram
    {
      get => searchProgram ??= new();
      set => searchProgram = value;
    }

    /// <summary>
    /// A value of SearchPersonProgram.
    /// </summary>
    [JsonPropertyName("searchPersonProgram")]
    public PersonProgram SearchPersonProgram
    {
      get => searchPersonProgram ??= new();
      set => searchPersonProgram = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    private CaseRole caseRole;
    private Program program;
    private CsePerson csePerson;
    private PersonProgram personProgram;
    private Program searchProgram;
    private PersonProgram searchPersonProgram;
    private CsePersonAccount csePersonAccount;
  }
#endregion
}
