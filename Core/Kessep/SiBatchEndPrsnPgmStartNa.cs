// Program: SI_BATCH_END_PRSN_PGM_START_NA, ID: 371787554, model: 746.
// Short name: SWE01563
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_BATCH_END_PRSN_PGM_START_NA.
/// </summary>
[Serializable]
public partial class SiBatchEndPrsnPgmStartNa: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BATCH_END_PRSN_PGM_START_NA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBatchEndPrsnPgmStartNa(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBatchEndPrsnPgmStartNa.
  /// </summary>
  public SiBatchEndPrsnPgmStartNa(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date    Developer       Request #       Description
    // 07/10/96  G. Lofton                       Initial Development
    // ----------------------------------------------------------
    // *****************************************************************
    // 12/07/1998   C. Ott   IDCR # 449.  Added SET statements to UPDATE for 
    // med_type and med_type_discontinue_date attributes.
    // ****************************************************************
    // ****************************************************************
    // 07/29/1999   C. Ott   Added logic to allow open and closure of a Person 
    // Program on the same day if the progam was not previously open.
    // ****************************************************************
    // **************************************************************
    // 10/01/1999   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
    // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of distribution
    // when a program change is made.
    // *************************************************************
    // ***************************************************************
    // 10/18/1999   C. Ott   PR # 73777.  This  Problem Report permits manual 
    // closure of NF and FC programs on the PPFX screen.  If this has occurred,
    // need to make sure program closure is not duplicated.
    // ***************************************************************
    // **************************************************************
    // 12/03/99   C. Ott   Problem # 81730.  Send Alert when an Interstate case 
    // is closed.
    // *************************************************************
    // ****************************************************************
    // 12/16/99  C. Ott  WR # 7.  Prevent program effective date from being 
    // equal to discontinue date.
    // ****************************************************************
    local.Max.Date = UseCabSetMaximumDiscontinueDate2();
    export.CntlTotPersPgmCloses.Count = import.CntlTotPersPgmCloses.Count;
    export.CntlTotPersPgmCreates.Count = import.CntlTotPersPgmCreates.Count;
    MovePersonProgram2(import.PersonProgram, local.PersonProgram);
    local.Program.Code = import.Program.Code;

    if (!Lt(local.Blank.Date, import.PersonProgram.DiscontinueDate))
    {
      ExitState = "SI0000_DISCONTINUE_DATE_REQUIRED";

      return;
    }

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadProgram1())
    {
      ExitState = "PROGRAM_NF";

      return;
    }

    // ****************************************************************
    // Read and update the person program with the new discontinue date.
    // ****************************************************************
    if (ReadPersonProgram1())
    {
      // ************************************************************
      // 09/07/00 A Doty - Correct a problem with setting the trigger date.
      // ************************************************************
      local.Tmp.DiscontinueDate = entities.PersonProgram.DiscontinueDate;

      if (Lt(local.PersonProgram.DiscontinueDate,
        entities.PersonProgram.EffectiveDate))
      {
        local.PersonProgram.DiscontinueDate =
          AddDays(entities.PersonProgram.EffectiveDate, 1);
      }

      // ****************************************************************
      // 12/16/99  C. Ott  WR # 7.  If the following update would result in the 
      // program effective date being equal to the discontinue date, do not
      // perform update but delete the program.
      // ****************************************************************
      if (Equal(entities.PersonProgram.EffectiveDate,
        local.PersonProgram.DiscontinueDate))
      {
        UseSiPeprDeletePersonProgram();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      else
      {
        // *****************************************************************
        // 12/7/1998   C. Ott    IDCR # 449.  Added SET statements for med_type 
        // and med_type_discontinue_date attributes.
        // ****************************************************************
        try
        {
          UpdatePersonProgram();

          // **************************************************************
          // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
          // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of
          // distribution when a program change is made.
          // *************************************************************
          if (AsChar(import.RecomputeDistribution.Flag) == 'Y')
          {
            // ************************************************************
            // 09/07/00 A Doty - Correct a problem with setting the trigger 
            // date.
            // ************************************************************
            if (!Equal(local.Tmp.DiscontinueDate,
              local.PersonProgram.DiscontinueDate))
            {
              if (Equal(entities.Program.Code, "AF") || Equal
                (entities.Program.Code, "NF") || Equal
                (entities.Program.Code, "NA") || Equal
                (entities.Program.Code, "NC") || Equal
                (entities.Program.Code, "AFI") || Equal
                (entities.Program.Code, "FC") || Equal
                (entities.Program.Code, "FCI") || Equal
                (entities.Program.Code, "MAI") || Equal
                (entities.Program.Code, "NAI"))
              {
                if (ReadCsePersonAccount2())
                {
                  // ************************************************************
                  // 09/07/00 A Doty - Correct a problem with setting the 
                  // trigger date.
                  // ************************************************************
                  if (Lt(local.PersonProgram.DiscontinueDate,
                    local.Tmp.DiscontinueDate))
                  {
                    local.Tmp.DiscontinueDate =
                      local.PersonProgram.DiscontinueDate;
                  }

                  local.CsePersonAccount.PgmChgEffectiveDate =
                    AddDays(local.Tmp.DiscontinueDate, 1);

                  if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
                    local.Blank.Date) || Lt
                    (local.CsePersonAccount.PgmChgEffectiveDate,
                    entities.CsePersonAccount.PgmChgEffectiveDate))
                  {
                    try
                    {
                      UpdateCsePersonAccount3();
                    }
                    catch(Exception e1)
                    {
                      switch(GetErrorCode(e1))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "FN0000_CSE_PERSON_ACCOUNT_NU";

                          break;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

                          break;
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
                  // ***********************************************************
                  // This date is only updated for Supported Persons, CSE Person
                  // Type 'S'
                  // **********************************************************
                }
              }
            }
          }
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

      ++import.ExpCheckpointNumbUpdates.Count;
      ++export.CntlTotPersPgmCloses.Count;

      // **************************************************************
      // 12/03/99   C. Ott   Problem # 81730.  Send Alert when an Interstate 
      // case is closed.
      // *************************************************************
      if (CharAt(entities.Program.Code, 3) == 'I')
      {
        // ***************************************************************
        // Create Infrastructure record for Alert.
        // ***************************************************************
        foreach(var item in ReadCase())
        {
          export.BypassAdcOpenAlert.Flag = "Y";
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.EventId = 5;
          local.Infrastructure.ReasonCode = "INTERSTATETOTAF";
          local.Infrastructure.BusinessObjectCd = "CAS";
          UseSpCabCreateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }
    }
    else
    {
      // ****************************************************************
      // 07/29/99   C. Ott   Added the following logic to allow open and closure
      // of a Person Program on the same day if the progam was not previously
      // open.
      // ****************************************************************
      if (Equal(import.InterfacePersonProgram.ParticipationCode, "OU"))
      {
        if (ReadPersonProgram2())
        {
          // ***************************************************************
          // 10/18/99   C. Ott  PR # 73777.  This  Problem Report permits manual
          // closure of NF and FC programs on the PPFX screen.  If this has
          // occurred, need to make sure program closure is not duplicated.
          // ***************************************************************
          return;
        }

        ReadInterfacePersonProgram();

        if (entities.Preceeding.Populated)
        {
          local.InOu.EffectiveDate = entities.Preceeding.ProgEffectiveDate;
        }
        else
        {
          local.InOu.EffectiveDate = import.PersonProgram.EffectiveDate;
        }

        try
        {
          CreatePersonProgram1();
          ++import.ExpCheckpointNumbUpdates.Count;
          ++export.CntlTotPersPgmCloses.Count;

          // **************************************************************
          // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
          // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of
          // distribution when a program change is made.
          // *************************************************************
          if (AsChar(import.RecomputeDistribution.Flag) == 'Y')
          {
            if (Equal(entities.Program.Code, "AF") || Equal
              (entities.Program.Code, "NF") || Equal
              (entities.Program.Code, "NA") || Equal
              (entities.Program.Code, "NC") || Equal
              (entities.Program.Code, "AFI") || Equal
              (entities.Program.Code, "FC") || Equal
              (entities.Program.Code, "FCI") || Equal
              (entities.Program.Code, "MAI") || Equal
              (entities.Program.Code, "NAI"))
            {
              if (ReadCsePersonAccount1())
              {
                if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
                  local.Blank.Date) || Lt
                  (local.InOu.EffectiveDate,
                  entities.CsePersonAccount.PgmChgEffectiveDate))
                {
                  try
                  {
                    UpdateCsePersonAccount2();
                  }
                  catch(Exception e1)
                  {
                    switch(GetErrorCode(e1))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "FN0000_CSE_PERSON_ACCOUNT_NU";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

                        break;
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
                // ***********************************************************
                // This date is only updated for Supported Persons, CSE Person 
                // Type 'S'
                // **********************************************************
              }
            }
          }

          goto Read;
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
      }

      ExitState = "PERSON_PROGRAM_NF";

      return;
    }

Read:

    // ****************************************************************
    // If the previous person program was discontinued, check flag
    // to determine if a NA program is to be started.
    // ****************************************************************
    if (AsChar(import.StartNa.Flag) != 'Y')
    {
      return;
    }

    UseCabSetMaximumDiscontinueDate1();
    local.PersonProgram.EffectiveDate =
      AddDays(local.PersonProgram.DiscontinueDate, 1);
    local.PersonProgram.DiscontinueDate = local.Max.Date;

    // ****************************************************************
    // Before a NA program can be started check to verify no other
    // programs are in effect.
    // ****************************************************************
    if (ReadPersonProgram3())
    {
      return;
    }

    if (!ReadProgram2())
    {
      ExitState = "PROGRAM_NF";

      return;
    }

    try
    {
      CreatePersonProgram2();

      // **************************************************************
      // 10/01/99   C. Ott   Problem # 76648, CSE_PERSON_ACCOUNT 
      // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of distribution
      // when a program change is made.
      // *************************************************************
      if (AsChar(import.RecomputeDistribution.Flag) == 'Y')
      {
        if (ReadCsePersonAccount1())
        {
          if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
            local.Blank.Date) || Lt
            (local.PersonProgram.EffectiveDate,
            entities.CsePersonAccount.PgmChgEffectiveDate))
          {
            try
            {
              UpdateCsePersonAccount1();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_CSE_PERSON_ACCOUNT_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

                  break;
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
          // ***********************************************************
          // This date is only updated for Supported Persons, CSE Person Type '
          // S'
          // **********************************************************
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

    ++import.ExpCheckpointNumbUpdates.Count;
    ++export.CntlTotPersPgmCreates.Count;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MovePersonProgram1(PersonProgram source,
    PersonProgram target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ClosureReason = source.ClosureReason;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePersonProgram2(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSiPeprDeletePersonProgram()
  {
    var useImport = new SiPeprDeletePersonProgram.Import();
    var useExport = new SiPeprDeletePersonProgram.Export();

    MovePersonProgram1(entities.PersonProgram, useImport.PersonProgram);
    useImport.Program.Code = entities.Program.Code;
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.RecomputeDistribution.Flag = import.RecomputeDistribution.Flag;

    Call(SiPeprDeletePersonProgram.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private void CreatePersonProgram1()
  {
    var cspNumber = entities.CsePerson.Number;
    var effectiveDate = local.InOu.EffectiveDate;
    var assignedDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = import.PersonProgram.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var changeDate = local.Blank.Date;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;
    var medTypeDiscontinueDate = import.PersonProgram.MedTypeDiscontinueDate;
    var medType = import.PersonProgram.MedType ?? "";

    entities.PersonProgram.Populated = false;
    Update("CreatePersonProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "status", "");
        db.SetNullableString(command, "closureReason", "");
        db.SetNullableDate(command, "assignedDate", assignedDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", createdTimestamp);
        db.SetNullableString(command, "changedInd", "");
        db.SetNullableDate(command, "changeDate", changeDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetNullableDate(command, "medTypeDiscDate", medTypeDiscontinueDate);
        db.SetNullableString(command, "medType", medType);
      });

    entities.PersonProgram.CspNumber = cspNumber;
    entities.PersonProgram.EffectiveDate = effectiveDate;
    entities.PersonProgram.ClosureReason = "";
    entities.PersonProgram.AssignedDate = assignedDate;
    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.CreatedBy = createdBy;
    entities.PersonProgram.CreatedTimestamp = createdTimestamp;
    entities.PersonProgram.LastUpdatedBy = createdBy;
    entities.PersonProgram.LastUpdatdTstamp = createdTimestamp;
    entities.PersonProgram.ChangedInd = "";
    entities.PersonProgram.ChangeDate = changeDate;
    entities.PersonProgram.PrgGeneratedId = prgGeneratedId;
    entities.PersonProgram.MedTypeDiscontinueDate = medTypeDiscontinueDate;
    entities.PersonProgram.MedType = medType;
    entities.PersonProgram.Populated = true;
  }

  private void CreatePersonProgram2()
  {
    var cspNumber = entities.CsePerson.Number;
    var effectiveDate = local.PersonProgram.EffectiveDate;
    var assignedDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.PersonProgram.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var changeDate = local.Blank.Date;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.PersonProgram.Populated = false;
    Update("CreatePersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "status", "");
        db.SetNullableString(command, "closureReason", "");
        db.SetNullableDate(command, "assignedDate", assignedDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", createdTimestamp);
        db.SetNullableString(command, "changedInd", "");
        db.SetNullableDate(command, "changeDate", changeDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetNullableDate(command, "medTypeDiscDate", null);
        db.SetNullableString(command, "medType", "");
      });

    entities.PersonProgram.CspNumber = cspNumber;
    entities.PersonProgram.EffectiveDate = effectiveDate;
    entities.PersonProgram.ClosureReason = "";
    entities.PersonProgram.AssignedDate = assignedDate;
    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.CreatedBy = createdBy;
    entities.PersonProgram.CreatedTimestamp = createdTimestamp;
    entities.PersonProgram.LastUpdatedBy = createdBy;
    entities.PersonProgram.LastUpdatdTstamp = createdTimestamp;
    entities.PersonProgram.ChangedInd = "";
    entities.PersonProgram.ChangeDate = changeDate;
    entities.PersonProgram.PrgGeneratedId = prgGeneratedId;
    entities.PersonProgram.MedTypeDiscontinueDate = null;
    entities.PersonProgram.MedType = "";
    entities.PersonProgram.Populated = true;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
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
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount1()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
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

  private bool ReadCsePersonAccount2()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount2",
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

  private bool ReadInterfacePersonProgram()
  {
    entities.Preceeding.Populated = false;

    return Read("ReadInterfacePersonProgram",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.InterfacePersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "programCode", entities.Program.Code);
        db.SetDate(
          command, "progEffectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.
          SetDate(command, "programEndDate", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Preceeding.RecordType = db.GetString(reader, 0);
        entities.Preceeding.ProgramCode = db.GetString(reader, 1);
        entities.Preceeding.StatusInd = db.GetNullableString(reader, 2);
        entities.Preceeding.ClosureReason = db.GetNullableString(reader, 3);
        entities.Preceeding.From = db.GetNullableString(reader, 4);
        entities.Preceeding.ProgEffectiveDate = db.GetDate(reader, 5);
        entities.Preceeding.ProgramEndDate = db.GetDate(reader, 6);
        entities.Preceeding.CreatedBy = db.GetNullableString(reader, 7);
        entities.Preceeding.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Preceeding.ProcessDate = db.GetNullableDate(reader, 9);
        entities.Preceeding.AssignedDate = db.GetNullableDate(reader, 10);
        entities.Preceeding.ParticipationCode = db.GetString(reader, 11);
        entities.Preceeding.CsePersonNumber = db.GetString(reader, 12);
        entities.Preceeding.Populated = true;
      });
  }

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedBy = db.GetString(reader, 5);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 9);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 10);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 11);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 13);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedBy = db.GetString(reader, 5);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 9);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 10);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 11);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 13);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram3()
  {
    entities.Re.Populated = false;

    return Read("ReadPersonProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.PersonProgram.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Re.CspNumber = db.GetString(reader, 0);
        entities.Re.EffectiveDate = db.GetDate(reader, 1);
        entities.Re.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Re.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Re.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Re.Populated = true;
      });
  }

  private bool ReadProgram1()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(command, "code", import.Program.Code);
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
      null,
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.DistributionProgramType = db.GetString(reader, 2);
        entities.Program.Populated = true;
      });
  }

  private void UpdateCsePersonAccount1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = local.PersonProgram.EffectiveDate;

    entities.CsePersonAccount.Populated = false;
    Update("UpdateCsePersonAccount1",
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

  private void UpdateCsePersonAccount2()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = local.InOu.EffectiveDate;

    entities.CsePersonAccount.Populated = false;
    Update("UpdateCsePersonAccount2",
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

  private void UpdateCsePersonAccount3()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = local.CsePersonAccount.PgmChgEffectiveDate;

    entities.CsePersonAccount.Populated = false;
    Update("UpdateCsePersonAccount3",
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

    var closureReason = import.PersonProgram.ClosureReason ?? "";
    var discontinueDate = local.PersonProgram.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var medTypeDiscontinueDate = import.PersonProgram.MedTypeDiscontinueDate;
    var medType = import.PersonProgram.MedType ?? "";

    entities.PersonProgram.Populated = false;
    Update("UpdatePersonProgram",
      (db, command) =>
      {
        db.SetNullableString(command, "closureReason", closureReason);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableDate(command, "medTypeDiscDate", medTypeDiscontinueDate);
        db.SetNullableString(command, "medType", medType);
        db.SetString(command, "cspNumber", entities.PersonProgram.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.PersonProgram.PrgGeneratedId);
      });

    entities.PersonProgram.ClosureReason = closureReason;
    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.LastUpdatedBy = lastUpdatedBy;
    entities.PersonProgram.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.PersonProgram.MedTypeDiscontinueDate = medTypeDiscontinueDate;
    entities.PersonProgram.MedType = medType;
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
    /// A value of ExpCheckpointNumbUpdates.
    /// </summary>
    [JsonPropertyName("expCheckpointNumbUpdates")]
    public Common ExpCheckpointNumbUpdates
    {
      get => expCheckpointNumbUpdates ??= new();
      set => expCheckpointNumbUpdates = value;
    }

    /// <summary>
    /// A value of CntlTotPersPgmCloses.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCloses")]
    public Common CntlTotPersPgmCloses
    {
      get => cntlTotPersPgmCloses ??= new();
      set => cntlTotPersPgmCloses = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of StartNa.
    /// </summary>
    [JsonPropertyName("startNa")]
    public Common StartNa
    {
      get => startNa ??= new();
      set => startNa = value;
    }

    /// <summary>
    /// A value of CntlTotPersPgmCreates.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCreates")]
    public Common CntlTotPersPgmCreates
    {
      get => cntlTotPersPgmCreates ??= new();
      set => cntlTotPersPgmCreates = value;
    }

    /// <summary>
    /// A value of InterfacePersonProgram.
    /// </summary>
    [JsonPropertyName("interfacePersonProgram")]
    public InterfacePersonProgram InterfacePersonProgram
    {
      get => interfacePersonProgram ??= new();
      set => interfacePersonProgram = value;
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

    private Common expCheckpointNumbUpdates;
    private Common cntlTotPersPgmCloses;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Program program;
    private PersonProgram personProgram;
    private Common startNa;
    private Common cntlTotPersPgmCreates;
    private InterfacePersonProgram interfacePersonProgram;
    private Common recomputeDistribution;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CntlTotPersPgmCloses.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCloses")]
    public Common CntlTotPersPgmCloses
    {
      get => cntlTotPersPgmCloses ??= new();
      set => cntlTotPersPgmCloses = value;
    }

    /// <summary>
    /// A value of CntlTotPersPgmCreates.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCreates")]
    public Common CntlTotPersPgmCreates
    {
      get => cntlTotPersPgmCreates ??= new();
      set => cntlTotPersPgmCreates = value;
    }

    /// <summary>
    /// A value of DatabaseUpdated.
    /// </summary>
    [JsonPropertyName("databaseUpdated")]
    public Common DatabaseUpdated
    {
      get => databaseUpdated ??= new();
      set => databaseUpdated = value;
    }

    /// <summary>
    /// A value of BypassAdcOpenAlert.
    /// </summary>
    [JsonPropertyName("bypassAdcOpenAlert")]
    public Common BypassAdcOpenAlert
    {
      get => bypassAdcOpenAlert ??= new();
      set => bypassAdcOpenAlert = value;
    }

    private Common cntlTotPersPgmCloses;
    private Common cntlTotPersPgmCreates;
    private Common databaseUpdated;
    private Common bypassAdcOpenAlert;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of InOu.
    /// </summary>
    [JsonPropertyName("inOu")]
    public PersonProgram InOu
    {
      get => inOu ??= new();
      set => inOu = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of PrsnPgmFnd.
    /// </summary>
    [JsonPropertyName("prsnPgmFnd")]
    public Common PrsnPgmFnd
    {
      get => prsnPgmFnd ??= new();
      set => prsnPgmFnd = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public PersonProgram Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    private CsePersonAccount csePersonAccount;
    private PersonProgram inOu;
    private PersonProgram personProgram;
    private Program program;
    private DateWorkArea max;
    private Common prsnPgmFnd;
    private DateWorkArea blank;
    private Infrastructure infrastructure;
    private PersonProgram tmp;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Preceeding.
    /// </summary>
    [JsonPropertyName("preceeding")]
    public InterfacePersonProgram Preceeding
    {
      get => preceeding ??= new();
      set => preceeding = value;
    }

    /// <summary>
    /// A value of Re.
    /// </summary>
    [JsonPropertyName("re")]
    public PersonProgram Re
    {
      get => re ??= new();
      set => re = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    private Case1 case1;
    private InterfacePersonProgram preceeding;
    private PersonProgram re;
    private CsePerson csePerson;
    private PersonProgram personProgram;
    private Program program;
    private CsePersonAccount csePersonAccount;
  }
#endregion
}
