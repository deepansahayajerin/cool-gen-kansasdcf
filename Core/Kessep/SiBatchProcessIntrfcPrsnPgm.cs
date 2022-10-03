// Program: SI_BATCH_PROCESS_INTRFC_PRSN_PGM, ID: 371787300, model: 746.
// Short name: SWE01194
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
/// A program: SI_BATCH_PROCESS_INTRFC_PRSN_PGM.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block will use a passed interface record and
/// either create or update a person_program record depending on
/// the AE program and subtype and the participation code.
/// </para>
/// </summary>
[Serializable]
public partial class SiBatchProcessIntrfcPrsnPgm: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BATCH_PROCESS_INTRFC_PRSN_PGM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBatchProcessIntrfcPrsnPgm(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBatchProcessIntrfcPrsnPgm.
  /// </summary>
  public SiBatchProcessIntrfcPrsnPgm(IContext context, Import import,
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
    // 06/04/96  G. Lofton         		Initial Development
    // 05/12/97  Sid Chowdhary	  IDCR # 230	Set CASE ADC Open and
    // 					Closed Dates.
    // 08/12/97  Siraj Konkader    PR# 26062   Changed criteria for automatic 
    // rollover to NA. Earlier, Case would rollover to NA except when closure
    // reason code was DC and CR. Now only exception is DC.
    // 09/15/97   Siraj Konkader
    // Added IF EX ST <> ALL OK after CAB SI_SET_CASE_ADC_OPEN_CL..
    // Update CSE_PERSON if either AE or KSC case # is blank and incoming record
    // has that value.
    // ----------------------------------------------------------
    // ****************************************************************
    // 11/23/1998   C. Ott    Added SET statements for two attributes added to 
    // Person Program by IDCR # 449.
    // ***************************************************************
    // ****************************************************************
    // 12/02/1998   C. Ott    Modified Program to print rollover letters as 
    // indicated when Interface record is being processed in order to avoid a
    // second call of this cab.
    // ***************************************************************
    // ****************************************************************
    // 02/04/1999   M Ramirez      Added creation of document triggers,
    // and checks for unprocessed document triggers in the case of a
    // 'revert to open'.
    // ***************************************************************
    // ****************************************************************
    // 03/04/1999   C. Ott    A determination was made that a rollover from 'NA'
    // should not occur in Revert to Open instances.  The 'NA' Person Program
    // should be deleted instead.
    // ****************************************************************
    // ***************************************************************
    // 05/21/1999   C. Ott    Added action block to create CSENet notification 
    // of Case Type change.
    // ***************************************************************
    // ***************************************************************
    // 06/22/1999   C. Ott    Added a condition to discontinue an existing AF 
    // program for an incoming FC program.
    // ***************************************************************
    // ***************************************************************
    // 08/20/1999   C. Ott    Modified to correct error in EM and WT 
    // participation code processing.
    // ***************************************************************
    // ***************************************************************
    // 08/23/1999   C. Ott    Added a condition to discontinue an existing NA 
    // program for an incoming CC program.
    // ***************************************************************
    // **************************************************************
    // 10/01/1999   C. Ott    Problem # 76648 CSE_PERSON_ACCOUNT 
    // PGM_CHG_EFFECTIVE_DATE is used to trigger recomputation of distribution
    // when a program change is made.
    // *************************************************************
    // ****************************************************************
    // 10/13/1999   C. Ott    PR # 76678.  Add check for any currently open AE 
    // program to correct sending the wrong rollover letter..
    // ****************************************************************
    // **************************************************************
    // 10/19/1999   C. Ott    PR # 77321.   Update Case PA Medical Service from
    // 'MO' to 'MC' on each Case where the person has an active role as child
    // when another AE program is opened on a Medical Only case.
    // **************************************************************
    // ****************************************************************
    // 10/21/1999   C. Ott   PR # 77320.  When medical program closes on a 
    // Medical Only (MO) case, do not rollover to NA, but send an Alert.
    // ****************************************************************
    // ****************************************************************
    // 11/18/1999   C. Ott   PR # 80443, 80502  Restructure EM & WT processing 
    // for MA programs and re-opened WT.
    // ****************************************************************
    // *****************************************************************
    // 11/22/1999   C. Ott   Work Request # 7, and Problem Report # 78289.  
    // Added ability to delete a Person program record if an AE participation is
    // deleted.
    // ****************************************************************
    // ****************************************************************
    // 12/16/99  C. Ott  WR # 7.  Prevent program effective date from being 
    // equal to discontinue date.
    // ****************************************************************
    // ****************************************************************
    // 01/13/00  C. Ott  WR # 7/PR # 79456.  Update the AR CSE Person record 
    // with the case number when the child's program is received.
    // ****************************************************************
    // ****************************************************************
    // 01/13/00  C. Ott  WR # 7/PR # 79445.  Update the FC IV-E case number.
    // ****************************************************************
    // ****************************************************************
    // 01/25/00  C. Ott   PR # 84155. GA Foster care was not properly set for 
    // History records.
    // ****************************************************************
    // ****************************************************************
    // 01/28/00  C. Ott  PR # 85941.  Check for latest program end date to 
    // determine effective date for rollover to NA.
    // ****************************************************************
    // ************************************************************
    // 03/22/00  C. Ott  PR # 90432.    Restructured Revert to Open logic so 
    // that when program is not found to revert open, a new program is created.
    // ************************************************************
    // ************************************************************
    // 03/22/00  C. Ott  PR # 91516.  An extra FC program was entered when case 
    // closure.
    // ************************************************************
    // ************************************************************
    // 06/01/00  M. Lachowicz  PR # 96542. Use processing date
    // not discontinue date for Adcrollo and Parollo letters.
    // ************************************************************
    // ************************************************************
    // 09/12/00  M. Lachowicz  PR # 96630. Remove all referrences to produce 
    // adcrollo and parollo letters.
    // ************************************************************
    // ************************************************************
    // 08/08/01  M. Lachowicz  PR # 120956. Do not update
    // Person Program if it Person Program Interface contains
    // the same information.
    // ************************************************************
    // ************************************************************
    // 08/09/01  M. Lachowicz  PR # 125196. Do not close
    // 'NF' Person Program when 'AF' is opened.
    // ************************************************************
    // ************************************************************
    // 08/09/02  M. Lachowicz Fixed Madhu's chnage.
    //           Pass interface person program to
    //           SI_BATCH_END_PRSN_PGM_START_NA_1.
    // ************************************************************
    local.Current.Date = Now().Date;
    export.CntlTotAdcCaseOpens.Count = import.CntlTotAdcCaseOpens.Count;
    export.CntlTotAdcCaseCloses.Count = import.CntlTotAdcCaseCloses.Count;
    export.CntlTotPersPgmCreates.Count = import.CntlTotPersPgmCreates.Count;
    export.CntlTotPersPgmCloses.Count = import.CntlTotPersPgmCloses.Count;
    UseCabSetMaximumDiscontinueDate();
    local.InterfacePersonProgram.Assign(import.InterfacePersonProgram);

    // M.L 04/18/00 Start
    if (Equal(local.InterfacePersonProgram.MedTypeDiscontinueDate,
      local.Blank.Date))
    {
      local.InterfacePersonProgram.MedTypeDiscontinueDate = local.Max.Date;
    }

    // M.L 04/18/00 End
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      local.ProgramProcessingInfo);

    // ***************************************************************
    // 9/15/97:  Update AE case # and/or KS case # if blank.
    // ***************************************************************
    if (ReadCsePerson2())
    {
      if (!Equal(entities.Scan.AeCaseNumber,
        import.InterfacePersonProgram.PaCaseNumber))
      {
        if (Equal(import.InterfacePersonProgram.From, "KAE") && !
          IsEmpty(import.InterfacePersonProgram.PaCaseNumber))
        {
          try
          {
            UpdateCsePerson3();
            ++export.CntlTotCsePersUpdates.Count;
            export.DatabaseUpdated.Flag = "Y";

            // ****************************************************************
            // 01/13/00  C. Ott  WR # 7/PR # 79445.  Update the FC IV-E case 
            // number.
            // ****************************************************************
            if (Equal(import.InterfacePersonProgram.ProgramCode, "FC"))
            {
              foreach(var item in ReadCaseRole())
              {
                try
                {
                  UpdateCaseRole();
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
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
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CSE_PERSON_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CSE_PERSON_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      if (!Equal(entities.Scan.KscaresNumber,
        import.InterfacePersonProgram.PaCaseNumber))
      {
        if (Equal(import.InterfacePersonProgram.From, "KSC") && !
          IsEmpty(import.InterfacePersonProgram.PaCaseNumber))
        {
          try
          {
            UpdateCsePerson4();
            ++export.CntlTotCsePersUpdates.Count;
            export.DatabaseUpdated.Flag = "Y";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CSE_PERSON_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CSE_PERSON_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      // ****************************************************************
      // 01/13/00  C. Ott  WR # 7/PR # 79456.  Update the AR CSE Person record 
      // with the case number when the child's program is received.
      // ****************************************************************
      foreach(var item in ReadCase2())
      {
        foreach(var item1 in ReadCsePerson3())
        {
          if (!Equal(entities.Ar.AeCaseNumber,
            import.InterfacePersonProgram.PaCaseNumber))
          {
            if (Equal(import.InterfacePersonProgram.From, "KAE") && !
              IsEmpty(import.InterfacePersonProgram.PaCaseNumber))
            {
              try
              {
                UpdateCsePerson1();
                ++export.CntlTotCsePersUpdates.Count;
                export.DatabaseUpdated.Flag = "Y";
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CSE_PERSON_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "CSE_PERSON_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          if (!Equal(entities.Ar.KscaresNumber,
            import.InterfacePersonProgram.PaCaseNumber))
          {
            if (Equal(import.InterfacePersonProgram.From, "KSC") && !
              IsEmpty(import.InterfacePersonProgram.PaCaseNumber))
            {
              try
              {
                UpdateCsePerson2();
                ++export.CntlTotCsePersUpdates.Count;
                export.DatabaseUpdated.Flag = "Y";
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CSE_PERSON_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "CSE_PERSON_PV";

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
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ************************************************************
      // 03/22/00  C. Ott  PR # 90432.    Restructured Revert to Open logic so 
      // that when program is not found to revert open, a new program is
      // created.  Moved IF record type = 'R' into single IF statement so that '
      // IN' participation code can be accomodated.
      // ************************************************************
      if (AsChar(import.InterfacePersonProgram.RecordType) == 'R')
      {
        if (ReadPersonProgramProgram1())
        {
          MoveProgram2(entities.Program, local.Program);

          // ************************************************************
          // 09/07/00 A Doty - Correct a problem with setting the trigger date.
          // ************************************************************
          MovePersonProgram4(entities.PersonProgram, local.Tmp);
          local.PersonProgram.ClosureReason = "";
          local.PersonProgram.MedType =
            import.InterfacePersonProgram.MedType ?? "";
          local.PersonProgram.MedTypeDiscontinueDate =
            import.InterfacePersonProgram.MedTypeDiscontinueDate;

          if (Equal(import.InterfacePersonProgram.ProgramEndDate,
            local.Blank.Date))
          {
            local.PersonProgram.DiscontinueDate = local.Max.Date;
          }
          else
          {
            local.PersonProgram.DiscontinueDate =
              import.InterfacePersonProgram.ProgramEndDate;
          }

          local.PersonProgram.EffectiveDate =
            AddDays(entities.PersonProgram.DiscontinueDate, 1);
          local.CsePersonsWorkSet.Number =
            import.InterfacePersonProgram.CsePersonNumber;
          local.RecomputeDistribution.Flag = "Y";

          // ****************************************************************
          // 12/16/99  C. Ott  WR # 7.  If the following update would result in 
          // the program effective date being equal to the discontinue date, do
          // not perform update but delete the program.
          // ****************************************************************
          if (Equal(entities.PersonProgram.EffectiveDate,
            local.PersonProgram.DiscontinueDate))
          {
            UseSiPeprDeletePersonProgram2();
          }
          else
          {
            // 08/08/2001 M.Lachowicz Start
            if (Equal(entities.PersonProgram.DiscontinueDate,
              local.PersonProgram.DiscontinueDate) && Equal
              (entities.PersonProgram.ClosureReason,
              local.PersonProgram.ClosureReason) && Equal
              (entities.PersonProgram.MedType, local.PersonProgram.MedType) && Equal
              (entities.PersonProgram.MedTypeDiscontinueDate,
              local.PersonProgram.MedTypeDiscontinueDate))
            {
              goto Test1;
            }

            // 08/08/2001 M.Lachowicz Start
            try
            {
              UpdatePersonProgram1();

              // ************************************************************
              // 09/07/00 A Doty - Correct a problem with setting the trigger 
              // date.
              // ************************************************************
              // : Only set the trigger if the discontinue date has actually 
              // changed.
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

                  if (ReadCsePersonAccount())
                  {
                    if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
                      local.Blank.Date) || Lt
                      (local.Tmp.DiscontinueDate,
                      entities.CsePersonAccount.PgmChgEffectiveDate))
                    {
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
                    // This date is only updated for Supported Persons, CSE 
                    // Person Type 'S'
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
                  ExitState = "PERSON_PROGRAM_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PERSON_PROGRAM_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

Test1:

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++import.ExpCheckpointNumbUpdates.Count;
            export.DatabaseUpdated.Flag = "Y";

            // ***************************************************************
            // 05/21/1999  C. Ott    Added action block to create CSENet 
            // notification of Case Type change.
            // ***************************************************************
            local.SendToCsenetProgram.Code = local.Program.Code;
            local.SendToCsenetCsePerson.Number = local.CsePersonsWorkSet.Number;
            UseSiPersPgmCreateCsenetTran();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INTERSTATE_CASE_NF";

              return;
            }
          }
          else
          {
            return;
          }

          // ****************************************************************
          // Check for a rollover to NA, the effective date of such a program 
          // should be the old AF program discontinue date + 1 (i.e. same as the
          // effective date of the newly created record).
          // ****************************************************************
          // ****************************************************************
          // 3/4/1999    C. Ott    A determination was made that a rollover from
          // 'NA' should not occur.  The 'NA' Person Program should be deleted
          // instead.
          // ****************************************************************
          if (ReadPersonProgramProgram2())
          {
            local.RecomputeDistribution.Flag = "Y";
            UseSiPeprDeletePersonProgram1();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }
          else
          {
            // ******Continue
          }

          // 09/12/00 M.L Start
          // Do not send adcrollo or parollo letters.
          // 0912/00 M.L End
          // ****** Escape after one iteration.
          goto Test4;
        }

        // ****************************************************************
        // 03/22/00  C. Ott  PR # 90432.  If the Person Program was not found 
        // for Revert to Open, treat it as a new program.  Set participation
        // code to IN.
        // *****************************************************************
        local.InterfacePersonProgram.ParticipationCode = "IN";
      }

      if (AsChar(import.InterfacePersonProgram.RecordType) == 'H')
      {
        // ****************************************************************
        // Record Type of "H" indicates that this is a history record.
        // Create Person Program from Interface Person Program record.
        // ****************************************************************
        if (!IsEmpty(import.InterfacePersonProgram.MedType) && Equal
          (import.InterfacePersonProgram.ProgramEndDate, local.Blank.Date))
        {
          // *****************************************************************
          // 01/31/00  C. Ott  PR # 86838.  If a MED TYPE does not have an end 
          // date for a Program, this condition can not be handled.
          // ****************************************************************
          goto Test4;
        }

        local.CsePersonsWorkSet.Number =
          import.InterfacePersonProgram.CsePersonNumber;
        local.Program.Code = import.InterfacePersonProgram.ProgramCode;

        // ****************************************************************
        // 01/25/00  C. Ott   PR # 84155. GA Foster care was not properly set 
        // for History records.
        // ****************************************************************
        if (Equal(import.InterfacePersonProgram.ProgramCode, "FC") && Equal
          (import.InterfacePersonProgram.SourceOfFunds, "GA"))
        {
          local.Program.Code = "NF";
        }

        local.PersonProgram.ClosureReason =
          import.InterfacePersonProgram.ClosureReason ?? "";
        local.PersonProgram.EffectiveDate =
          import.InterfacePersonProgram.ProgEffectiveDate;
        local.PersonProgram.DiscontinueDate =
          import.InterfacePersonProgram.ProgramEndDate;
        local.PersonProgram.AssignedDate =
          import.InterfacePersonProgram.AssignedDate;
        local.PersonProgram.MedType = import.InterfacePersonProgram.MedType ?? ""
          ;
        local.PersonProgram.MedTypeDiscontinueDate =
          import.InterfacePersonProgram.MedTypeDiscontinueDate;
        local.History.Flag = "Y";
        local.RecomputeDistribution.Flag = "Y";
        UseSiBatchCreatePersonPgm2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++import.ExpCheckpointNumbUpdates.Count;
          export.DatabaseUpdated.Flag = "Y";
        }
        else
        {
          return;
        }
      }
      else if (AsChar(import.InterfacePersonProgram.RecordType) == 'D')
      {
        // *****************************************************************
        // 11/22/99   C. Ott   Work Request # 7, and Problem Report # 78289.  
        // Added ability to delete a Person Program record if an AE or KSC
        // participation is deleted.
        // EM and WT participations are part of another program record and are 
        // not deleted but updated to spaces.
        // ****************************************************************
        if (Equal(import.InterfacePersonProgram.ParticipationCode, "EM") || Equal
          (import.InterfacePersonProgram.ParticipationCode, "WT"))
        {
          foreach(var item in ReadPersonProgram7())
          {
            try
            {
              UpdatePersonProgram4();
              export.DatabaseUpdated.Flag = "Y";

              goto Test4;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PERSON_PROGRAM_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PERSON_PROGRAM_PV";

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
          foreach(var item in ReadPersonProgram3())
          {
            local.RecomputeDistribution.Flag = "Y";
            local.Program.Code = import.InterfacePersonProgram.ProgramCode;
            local.CsePersonsWorkSet.Number =
              import.InterfacePersonProgram.CsePersonNumber;
            UseSiPeprDeletePersonProgram2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.DatabaseUpdated.Flag = "Y";

              // ***************************************************************
              // Create Infrastructure record for Alert.
              // ***************************************************************
              foreach(var item1 in ReadCase3())
              {
                local.Infrastructure.CaseNumber = entities.Case1.Number;
                local.Infrastructure.CsePersonNumber =
                  import.InterfacePersonProgram.CsePersonNumber;
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.EventId = 5;
                local.Infrastructure.ReasonCode = "AECCERROR";
                local.Infrastructure.BusinessObjectCd = "CAS";
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Error.Message = "AECCERROR";

                  return;
                }
              }
            }
            else
            {
              return;
            }
          }
        }
      }
      else if (AsChar(import.InterfacePersonProgram.RecordType) == 'C')
      {
        // *****************************************************************
        // 12/03/99   C. Ott   Work Request # 7, add capability to change the 
        // end date of an existing program.
        // ****************************************************************
        foreach(var item in ReadPersonProgram3())
        {
          // ************************************************************
          // 09/07/00 A Doty - Correct a problem with setting the trigger date.
          // ************************************************************
          if (Equal(import.InterfacePersonProgram.ProgramEndDate,
            local.Blank.Date))
          {
            local.PersonProgram.DiscontinueDate = local.Max.Date;
          }
          else
          {
            local.PersonProgram.DiscontinueDate =
              import.InterfacePersonProgram.ProgramEndDate;
          }

          if (!Equal(local.PersonProgram.DiscontinueDate,
            entities.PersonProgram.DiscontinueDate))
          {
            // ************************************************************
            // 09/07/00 A Doty - Correct a problem with setting the trigger 
            // date.
            // ************************************************************
            MovePersonProgram4(entities.PersonProgram, local.Tmp);

            // 04/18/00 M.L Start
            // Changed import_interface_person_program med_type_discontinue_date
            // to  local_interface_person_program med_type_discontinue_date
            // 04/18/00 M.L End
            try
            {
              UpdatePersonProgram2();
              export.DatabaseUpdated.Flag = "Y";

              if (Equal(import.InterfacePersonProgram.ProgramCode, "AF") || Equal
                (import.InterfacePersonProgram.ProgramCode, "NF") || Equal
                (import.InterfacePersonProgram.ProgramCode, "NA") || Equal
                (import.InterfacePersonProgram.ProgramCode, "NC") || Equal
                (import.InterfacePersonProgram.ProgramCode, "AFI") || Equal
                (import.InterfacePersonProgram.ProgramCode, "FC") || Equal
                (import.InterfacePersonProgram.ProgramCode, "FCI") || Equal
                (import.InterfacePersonProgram.ProgramCode, "MAI") || Equal
                (import.InterfacePersonProgram.ProgramCode, "NAI"))
              {
                // ************************************************************
                // 09/07/00 A Doty - Correct a problem with setting the trigger 
                // date.
                // ************************************************************
                if (Lt(local.PersonProgram.DiscontinueDate,
                  local.Tmp.DiscontinueDate))
                {
                  local.Tmp.DiscontinueDate =
                    local.PersonProgram.DiscontinueDate;
                }

                if (ReadCsePersonAccount())
                {
                  if (Equal(entities.CsePersonAccount.PgmChgEffectiveDate,
                    local.Blank.Date) || Lt
                    (local.Tmp.DiscontinueDate,
                    entities.CsePersonAccount.PgmChgEffectiveDate))
                  {
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
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PERSON_PROGRAM_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PERSON_PROGRAM_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            goto Test4;
          }
        }
      }
      else if (!IsEmpty(import.InterfacePersonProgram.ClosureReason))
      {
        // **************************************************************
        // If a closure reason has been supplied, which means that the entire AE
        // or KS CASE has been closed, perform action block to close person
        // program.  This may also cause the generation of a Case Rollover
        // letter.
        // This is in contrast to a participation Code of 'OU' which pertains 
        // only to the closure of a single Person-Program and not the entire
        // Case.
        // **************************************************************
        local.SendAdcRolloverLetter.Flag = "N";
        local.SendPaRolloverLetter.Flag = "N";
        local.CaseClosureStartNa.Flag = "N";

        if (Lt(local.Blank.Date, import.InterfacePersonProgram.ProgramEndDate))
        {
          local.CsePersonsWorkSet.Number =
            import.InterfacePersonProgram.CsePersonNumber;

          // ************************************************************
          // 03/22/00  C. Ott  PR # 91516.  An extra FC program was entered when
          // case closure.
          // ************************************************************
          if (Equal(import.InterfacePersonProgram.ProgramCode, "FC") && Equal
            (import.InterfacePersonProgram.SourceOfFunds, "GA"))
          {
            local.Program.Code = "NF";
          }
          else
          {
            local.Program.Code = import.InterfacePersonProgram.ProgramCode;
          }

          local.PersonProgram.EffectiveDate =
            import.InterfacePersonProgram.ProgEffectiveDate;
          local.PersonProgram.DiscontinueDate =
            import.InterfacePersonProgram.ProgramEndDate;
          local.PersonProgram.ClosureReason =
            import.InterfacePersonProgram.ClosureReason ?? "";

          // **************************************************************
          // 12/03/99   C. Ott   Problem # 78289, Pass MED TYPE & MED TYPE 
          // DISCONTINUE DATE to action block.
          // *************************************************************
          local.PersonProgram.MedType =
            import.InterfacePersonProgram.MedType ?? "";
          local.PersonProgram.MedTypeDiscontinueDate =
            import.InterfacePersonProgram.MedTypeDiscontinueDate;
          local.RecomputeDistribution.Flag = "Y";

          if (ReadPersonProgram2())
          {
            UseSiBatchUpdatePersonPgm();

            if (IsExitState("ACO_NN0000_ALL_OK") && AsChar
              (local.AeCaseDenied.Flag) == 'Y')
            {
              foreach(var item in ReadCase3())
              {
                local.Infrastructure.CaseNumber = entities.Case1.Number;
                local.Infrastructure.EventId = 5;
                local.Infrastructure.ReasonCode = "AEDENIED";
                local.Infrastructure.BusinessObjectCd = "CAS";
                local.Infrastructure.ProcessStatus = "Q";
                local.Infrastructure.CaseNumber = entities.Case1.Number;
                local.Infrastructure.CsePersonNumber =
                  import.InterfacePersonProgram.CsePersonNumber;
                UseSpCabCreateInfrastructure();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Error.Message = "AEDENIED";

                  return;
                }
              }
            }
          }
          else
          {
            // ***************************************************************
            // A Case closure has been received for a person Program that does 
            // not exist.  Need to create the person program.
            // ***************************************************************
            // **************************************************************
            // 01/21/00  C. Ott   Do not create programs that start and end on 
            // the same date.
            // **************************************************************
            if (Equal(import.InterfacePersonProgram.ProgEffectiveDate,
              import.InterfacePersonProgram.ProgramEndDate))
            {
              goto Test4;
            }

            foreach(var item in ReadPersonProgramProgram5())
            {
              local.ExistingPersonProgram.Assign(entities.PersonProgram);
              local.ExistingProgram.Code = entities.Program.Code;

              if (Equal(local.Program.Code, local.ExistingProgram.Code))
              {
                local.ProgramExists.Flag = "Y";
                ExitState = "PERSON_PROGRAM_AE";

                break;
              }
              else if (CharAt(local.ExistingProgram.Code, 3) == 'I' || Equal
                (local.ExistingProgram.Code, "NA") || Equal
                (local.ExistingProgram.Code, "NF") && (
                  Equal(local.Program.Code, "AF") || Equal
                (local.Program.Code, "FC")) || Equal
                (local.ExistingProgram.Code, "AF") && Equal
                (local.Program.Code, "NF") || Equal
                (local.ExistingProgram.Code, "AF") && Equal
                (local.Program.Code, "FC") || Equal
                (local.ExistingProgram.Code, "FC") && Equal
                (local.Program.Code, "NF") || Equal
                (local.ExistingProgram.Code, "NC") && Equal
                (local.Program.Code, "CI") || Equal
                (local.ExistingProgram.Code, "NA") && Equal
                (local.Program.Code, "CC"))
              {
                local.ExistingPersonProgram.DiscontinueDate =
                  AddDays(local.PersonProgram.EffectiveDate, -1);

                if (Lt(local.ExistingPersonProgram.DiscontinueDate,
                  local.ExistingPersonProgram.EffectiveDate))
                {
                  local.ExistingPersonProgram.DiscontinueDate =
                    local.ExistingPersonProgram.EffectiveDate;
                }

                if (Equal(local.ExistingProgram.Code, "NA"))
                {
                  local.NonAdcPgmClosed.Flag = "Y";
                }

                local.StartNaCommon.Flag = "N";
                local.RecomputeDistribution.Flag = "Y";
                UseSiBatchEndPrsnPgmStartNa3();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.DatabaseUpdated.Flag = "Y";
                }
                else
                {
                  goto Test4;
                }
              }
            }

            UseSiBatchCreatePersonPgm3();
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++import.ExpCheckpointNumbUpdates.Count;
            export.DatabaseUpdated.Flag = "Y";
          }
          else
          {
            return;
          }

          // ***************************************************************
          // 02/09/00  C. Ott  Do not cause rollover to NA if AE program 
          // eligibility has been denied.
          // ***************************************************************
          if (AsChar(local.AeCaseDenied.Flag) == 'Y')
          {
            goto Test4;
          }

          // ****************************************************************
          // 10/13/99  C. Ott  PR # 76678.  Add check for any currently open AE 
          // program.
          // ****************************************************************
          // ****************************************************************
          // 01/28/00  C. Ott  PR # 85941.  Check for latest program end date to
          // determine effective date for rollover to NA.
          // ****************************************************************
          foreach(var item in ReadPersonProgramProgram7())
          {
            if (Equal(entities.Program.Code, "AF") || Equal
              (entities.Program.Code, "CC") || Equal
              (entities.Program.Code, "FS") || Equal
              (entities.Program.Code, "MA") || Equal
              (entities.Program.Code, "MK") || Equal
              (entities.Program.Code, "MP") || Equal
              (entities.Program.Code, "MS") || Equal
              (entities.Program.Code, "SI"))
            {
              if (Lt(Now().Date, entities.PersonProgram.DiscontinueDate))
              {
                local.AeProgramOpen.Flag = "Y";

                break;
              }
              else if (Lt(local.PersonProgram.DiscontinueDate,
                entities.PersonProgram.DiscontinueDate))
              {
                local.PersonProgram.DiscontinueDate =
                  entities.PersonProgram.DiscontinueDate;
              }
            }

            // ***************************************************************
            // 01/19/00  C. Ott  An active Med Type is also considered an active
            // program.
            // ***************************************************************
            if (!IsEmpty(entities.PersonProgram.MedType) && Lt
              (Now().Date, entities.PersonProgram.MedTypeDiscontinueDate))
            {
              local.AeProgramOpen.Flag = "Y";

              break;
            }
          }

          // ****************************************************************
          // 10/21/99  C. Ott  PR # 77320.  When medical program closes on a 
          // Medical Only (MO) case, do not rollover to NA, but send an Alert.
          // ****************************************************************
          if (Equal(local.Program.Code, "AF"))
          {
            // ·¼¼¼¼¼¼¼¼¼¼¼¼¼
            // Removed "and not equal to CR" per PR# 26062 by Pam Vickers
            // ·©©©©©©©©©©©©©
            if (Equal(import.InterfacePersonProgram.ClosureReason, "DC"))
            {
              // ************************************************
              // For AF closure with closure reason "DC", set the
              // ADC close date and raise the "ADC Closed history"
              // ************************************************
              local.OpenClosedAdc.Flag = "D";
              UseSiSetCaseAdcOpenCloseDates();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                // **************************************************************
                // 12/15/99  C. Ott  Delete any NA programs that may have been 
                // rolled over from a prior closure of this program.
                // *************************************************************
                foreach(var item in ReadPersonProgram4())
                {
                  local.Program.Code = "NA";
                  local.RecomputeDistribution.Flag = "Y";
                  UseSiPeprDeletePersonProgram2();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }
              }
              else
              {
                return;
              }
            }
            else
            {
              if (AsChar(local.AeProgramOpen.Flag) == 'Y')
              {
                local.SendAdcRolloverLetter.Flag = "Y";
              }
              else
              {
                local.SendPaRolloverLetter.Flag = "Y";
              }

              local.CaseClosureStartNa.Flag = "Y";
              local.StartNaPersonProgram.EffectiveDate =
                AddDays(local.PersonProgram.DiscontinueDate, 1);

              // *****************************************************************
              //  Set Changed_Ind for Finance
              // *****************************************************************
              local.StartNaPersonProgram.ChangedInd = "N";
              local.StartNaPersonProgram.ChangeDate = local.Blank.Date;
            }
          }
          else if (Equal(local.Program.Code, "MA") || Equal
            (local.Program.Code, "MK") || Equal(local.Program.Code, "MP") || Equal
            (local.Program.Code, "MS"))
          {
            if (AsChar(local.AeProgramOpen.Flag) == 'Y')
            {
            }
            else
            {
              // ***************************************************************
              // Check for another incoming active AE program.
              // ***************************************************************
              local.CaseClosureStartNa.Flag = "Y";
              local.StartNaPersonProgram.EffectiveDate =
                AddDays(local.PersonProgram.DiscontinueDate, 1);

              foreach(var item in ReadInterfacePersonProgram2())
              {
                if (Equal(entities.InterfacePersonProgram.ProgramCode, "AF") ||
                  Equal(entities.InterfacePersonProgram.ProgramCode, "CC") || Equal
                  (entities.InterfacePersonProgram.ProgramCode, "FS") || Equal
                  (entities.InterfacePersonProgram.ProgramCode, "MA") || Equal
                  (entities.InterfacePersonProgram.ProgramCode, "MK") || Equal
                  (entities.InterfacePersonProgram.ProgramCode, "MP") || Equal
                  (entities.InterfacePersonProgram.ProgramCode, "MS") || Equal
                  (entities.InterfacePersonProgram.ProgramCode, "SI"))
                {
                  goto Test2;
                }
              }

              local.SendPaRolloverLetter.Flag = "Y";

              foreach(var item in ReadCase1())
              {
                try
                {
                  UpdateCase2();
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

                local.SendPaRolloverLetter.Flag = "N";
                local.CaseClosureStartNa.Flag = "N";

                foreach(var item1 in ReadCase3())
                {
                  local.Infrastructure.CaseNumber = entities.Case1.Number;
                  local.Infrastructure.EventId = 5;
                  local.Infrastructure.ReasonCode = "MOCLOSE";
                  local.Infrastructure.BusinessObjectCd = "CAS";
                  local.Infrastructure.ProcessStatus = "Q";
                  local.Infrastructure.CaseNumber = entities.Case1.Number;
                  local.Infrastructure.CsePersonNumber =
                    import.InterfacePersonProgram.CsePersonNumber;
                  UseSpCabCreateInfrastructure();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.Error.Message = "MOCLOSE";

                    return;
                  }
                }
              }
            }
          }
          else
          {
            // *****************************************************************
            // 12/7/1998   C. Ott    Added "SI" to the following IF statement 
            // based on discussions with Pam Vickers and Cheryl Deghand.
            // *****************************************************************
            if (Equal(local.Program.Code, "CC") || Equal
              (local.Program.Code, "FS") || Equal(local.Program.Code, "MA") || Equal
              (local.Program.Code, "MK") || Equal(local.Program.Code, "MP") || Equal
              (local.Program.Code, "MS") || Equal(local.Program.Code, "SI"))
            {
              if (AsChar(local.AeProgramOpen.Flag) == 'Y')
              {
              }
              else
              {
                local.SendPaRolloverLetter.Flag = "Y";
              }

              // **********************************************************
              // As Per request all "DC" closures will be processes same.
              // **********************************************************
              if (!Equal(import.InterfacePersonProgram.ClosureReason, "DC"))
              {
                local.CaseClosureStartNa.Flag = "Y";
                local.StartNaPersonProgram.EffectiveDate =
                  AddDays(local.PersonProgram.DiscontinueDate, 1);

                // ****************************************************************
                // Set Changed_Ind for Finance
                // ****************************************************************
                if (Equal(local.Program.Code, "NF") || Equal
                  (local.Program.Code, "FC"))
                {
                  local.StartNaPersonProgram.ChangedInd = "N";
                  local.StartNaPersonProgram.ChangeDate = local.Blank.Date;
                }
              }
            }
          }

Test2:

          if (AsChar(local.CaseClosureStartNa.Flag) == 'Y')
          {
            local.RecomputeDistribution.Flag = "Y";
            UseSiBatchStartNaPrsnPgm();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ***************************************************************
              // 05/21/1999  C. Ott    Added action block to create CSENet 
              // notification of Case Type change.
              // ***************************************************************
              local.SendToCsenetProgram.Code = "NA";
              local.SendToCsenetCsePerson.Number =
                local.CsePersonsWorkSet.Number;
              UseSiPersPgmCreateCsenetTran();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "INTERSTATE_CASE_NF";

                return;
              }
            }
            else
            {
              return;
            }

            // **********************************************
            // Code put in by Sid. 05/12/1997.
            // Set the ADC Closed Date, if there are no more
            // ADC programs open for that Case.
            // **********************************************
            // ****************************************************************
            // 02/07/00  C. Ott   Only set ADC close date and generate ADC to 
            // NADC alert when AF closes.
            // ****************************************************************
            if (Equal(import.InterfacePersonProgram.ProgramCode, "AF"))
            {
              local.OpenClosedAdc.Flag = "C";
              UseSiSetCaseAdcOpenCloseDates();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }
          }
        }
        else
        {
          ExitState = "SI0000_DISCONTINUE_DATE_BLANK";

          return;
        }

        // ****************************************************************
        // 01/14/00  C. Ott  PR # 84608.  Create worker alert when FC program 
        // closes.
        // ****************************************************************
        if (Equal(import.InterfacePersonProgram.ProgramCode, "FC"))
        {
          foreach(var item in ReadCase3())
          {
            local.Infrastructure.CaseNumber = entities.Case1.Number;
            local.Infrastructure.EventId = 5;
            local.Infrastructure.ReasonCode = "FOSTERCLOSE";
            local.Infrastructure.BusinessObjectCd = "CAS";
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.CsePersonNumber =
              import.InterfacePersonProgram.CsePersonNumber;
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Error.Message = "FOSTERCLOSE";

              return;
            }
          }
        }
      }
      else
      {
        local.CsePersonsWorkSet.Number =
          import.InterfacePersonProgram.CsePersonNumber;
        local.PersonProgram.EffectiveDate =
          import.InterfacePersonProgram.ProgEffectiveDate;
        local.PersonProgram.AssignedDate =
          import.InterfacePersonProgram.AssignedDate;
        local.PersonProgram.DiscontinueDate =
          import.InterfacePersonProgram.ProgramEndDate;
        local.ProgramExists.Flag = "N";
        local.Program.Code = import.InterfacePersonProgram.ProgramCode;

        // ****************************************************************
        // 11/23/1998    C. Ott    Added SET statements for two attributes added
        // to Person Program by IDCR # 449.
        // ***************************************************************
        local.PersonProgram.MedType = import.InterfacePersonProgram.MedType ?? ""
          ;
        local.PersonProgram.MedTypeDiscontinueDate =
          import.InterfacePersonProgram.MedTypeDiscontinueDate;

        if (ReadCsePerson1())
        {
          // ****************************************************************
          // Process of the interface record depends upon the participation
          // code.  Code IN is primarily an add and the OU is a closing
          // or ending of a particular program for a person.
          // ****************************************************************
          // ***************************************************************
          // Participation code EM refers to Extended Medical and WT is Work 
          // Transtition.  These Participation Codes are only valid for Program
          // Code AF.
          // ****************************************************************
          switch(TrimEnd(local.InterfacePersonProgram.ParticipationCode))
          {
            case "EM":
              local.AdcPgmClosed.Flag = "N";

              if (Lt(import.InterfacePersonProgram.MedTypeDiscontinueDate,
                local.Current.Date) && !
                Equal(import.InterfacePersonProgram.MedTypeDiscontinueDate,
                local.Blank.Date))
              {
                // ***************************************************************
                // EM participation exists and should be closed
                // ***************************************************************
                foreach(var item in ReadPersonProgram5())
                {
                  // 04/18/00 M.L Start
                  // Changed import_interface_person_program 
                  // med_type_discontinue_date to
                  // local_interface_person_program med_type_discontinue_date
                  // 04/18/00 M.L End
                  try
                  {
                    UpdatePersonProgram3();
                    export.DatabaseUpdated.Flag = "Y";

                    // ***************************************************************
                    // 01/19/00  C. Ott   If Med Type closes but did not have a 
                    // closure reason code, send PA rollover letter if no other
                    // programs open .
                    // ***************************************************************
                    foreach(var item1 in ReadPersonProgramProgram6())
                    {
                      if (Equal(entities.Program.Code, "AF") || Equal
                        (entities.Program.Code, "CC") || Equal
                        (entities.Program.Code, "FS") || Equal
                        (entities.Program.Code, "MA") || Equal
                        (entities.Program.Code, "MK") || Equal
                        (entities.Program.Code, "MP") || Equal
                        (entities.Program.Code, "MS") || Equal
                        (entities.Program.Code, "SI"))
                      {
                        local.PaProgramOpen.Flag = "Y";
                      }
                    }

                    if (IsEmpty(local.PaProgramOpen.Flag))
                    {
                      local.SendPaRolloverLetter.Flag = "Y";
                    }

                    goto Test4;
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "PERSON_PROGRAM_AE";

                        goto Test4;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "PERSON_PROGRAM_PV";

                        goto Test4;
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
                // *****************************************************************
                // 11/17/99   C. Ott   Look for a currently closed AF or MA 
                // program.
                // ****************************************************************
                if (ReadPersonProgram1())
                {
                  // ***************************************************************
                  // Person Program is already closed, update Med Type.
                  // ***************************************************************
                  // 04/18/00 M.L Start
                  // Changed import_interface_person_program 
                  // med_type_discontinue_date to
                  // local_interface_person_program med_type_discontinue_date
                  // 04/18/00 M.L End
                  try
                  {
                    UpdatePersonProgram3();
                    export.DatabaseUpdated.Flag = "Y";
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "PERSON_PROGRAM_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "PERSON_PROGRAM_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
                else
                {
                  // *****************************************************************
                  // 11/17/99   C. Ott   A currently closed AF or MA person 
                  // program with existing Med Type was not found.  Look first
                  // for AF program closed within one year and if not found look
                  // for MA program closed within one year.
                  // ****************************************************************
                  foreach(var item in ReadPersonProgram8())
                  {
                    if (Lt(AddYears(local.Current.Date, -1),
                      entities.PersonProgram.DiscontinueDate))
                    {
                      // 04/18/00 M.L Start
                      // Changed import_interface_person_program 
                      // med_type_discontinue_date to
                      // local_interface_person_program
                      // med_type_discontinue_date
                      // 04/18/00 M.L End
                      try
                      {
                        UpdatePersonProgram3();
                        export.DatabaseUpdated.Flag = "Y";

                        goto Test4;
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "PERSON_PROGRAM_AE";

                            goto Test4;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "PERSON_PROGRAM_PV";

                            goto Test4;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }
                  }

                  foreach(var item in ReadPersonProgram9())
                  {
                    if (Lt(AddYears(local.Current.Date, -1),
                      entities.PersonProgram.DiscontinueDate))
                    {
                      // 04/18/00 M.L Start
                      // Changed import_interface_person_program 
                      // med_type_discontinue_date to
                      // local_interface_person_program
                      // med_type_discontinue_date
                      // 04/18/00 M.L End
                      try
                      {
                        UpdatePersonProgram3();
                        export.DatabaseUpdated.Flag = "Y";

                        goto Test4;
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "PERSON_PROGRAM_AE";

                            goto Test4;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "PERSON_PROGRAM_PV";

                            goto Test4;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }
                  }

                  ExitState = "INVALID_PROGRAM";
                }
              }

              break;
            case "IN":
              // ****************************************************************
              // A participation code of IN requires that a program is given
              // with the effective date.  A scan will look for any programs
              // that can not be active at the same time and will end date
              // these program(s) 1 day prior to the effective date of the
              // new program.
              // ***************************************************************
              // ****************************************************************
              // Validate Program Code imported on Interface Person Program 
              // record.
              // ****************************************************************
              switch(TrimEnd(import.InterfacePersonProgram.ProgramCode))
              {
                case "AF":
                  break;
                case "CC":
                  break;
                case "CI":
                  break;
                case "FC":
                  if (Equal(import.InterfacePersonProgram.SourceOfFunds, "GA"))
                  {
                    local.Program.Code = "NF";
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
                  ExitState = "SI0000_PGM_AND_PART_CODE_NOT_DEF";

                  goto Test4;
              }

              if (!Lt(local.Blank.Date, local.PersonProgram.EffectiveDate))
              {
                ExitState = "EFFECTIVE_DATE_REQUIRED";

                goto Test4;
              }

              if (!Lt(local.Blank.Date, local.PersonProgram.DiscontinueDate))
              {
                local.PersonProgram.DiscontinueDate = local.Max.Date;
              }

              local.NonAdcPgmClosed.Flag = "N";

              if (IsEmpty(import.InterfacePersonProgram.AeProgramSubtype) || Equal
                (import.InterfacePersonProgram.ProgramCode, "CI"))
              {
                // 02/21/2002 Marek Lachowicz
                // Enable Madhu's change. Use interface_person_program 
                // prog_effective_date instead of  process date.
                // ***********************************************************
                //       As a part of PR # 00115534  the read has been
                // changed to  person_prog discontinue_date >=  import 
                // interface_person_program ,
                //   earlier it was person_program discontinue_date  >
                // process date .
                //                       Madhu Kumar   05/08/2001
                // ***********************************************************
                foreach(var item in ReadPersonProgramProgram3())
                {
                  local.ExistingPersonProgram.Assign(entities.PersonProgram);
                  local.ExistingProgram.Code = entities.Program.Code;

                  if (Equal(local.Program.Code, local.ExistingProgram.Code))
                  {
                    local.ProgramExists.Flag = "Y";
                    ExitState = "PERSON_PROGRAM_AE";

                    break;
                  }
                  else
                  {
                    // ***************************************************************
                    // 06/22/1999    C. Ott    Added a condition to the 
                    // following IF statement for existing AF program and an
                    // incoming FC program.
                    // ***************************************************************
                    // ***************************************************************
                    // 08/20/1999    C. Ott    Added a condition to the 
                    // following IF statement for existing NC program and an
                    // incoming CI program.
                    // ***************************************************************
                    // ***************************************************************
                    // 08/23/1999    C. Ott    Added a condition to the 
                    // following IF statement for existing NA program and an
                    // incoming CC program.
                    // ***************************************************************
                    if (CharAt(local.ExistingProgram.Code, 3) == 'I' || Equal
                      (local.ExistingProgram.Code, "NA") || Equal
                      (local.ExistingProgram.Code, "NF") && (
                        Equal(local.Program.Code, "AF") || Equal
                      (local.Program.Code, "FC")) || Equal
                      (local.ExistingProgram.Code, "AF") && Equal
                      (local.Program.Code, "NF") || Equal
                      (local.ExistingProgram.Code, "AF") && Equal
                      (local.Program.Code, "FC") || Equal
                      (local.ExistingProgram.Code, "FC") && Equal
                      (local.Program.Code, "NF") || Equal
                      (local.ExistingProgram.Code, "NC") && Equal
                      (local.Program.Code, "CI") || Equal
                      (local.ExistingProgram.Code, "NA") && Equal
                      (local.Program.Code, "CC"))
                    {
                      // 08/09/2001 M.Lachowicz Start
                      if (Equal(local.ExistingProgram.Code, "NF") || Equal
                        (local.ExistingProgram.Code, "FC") || Equal
                        (local.ExistingProgram.Code, "NC"))
                      {
                        continue;
                      }

                      // 08/09/2001 M.Lachowicz End
                      local.ExistingPersonProgram.DiscontinueDate =
                        AddDays(local.PersonProgram.EffectiveDate, -1);

                      if (Lt(local.ExistingPersonProgram.DiscontinueDate,
                        local.ExistingPersonProgram.EffectiveDate))
                      {
                        local.ExistingPersonProgram.DiscontinueDate =
                          local.ExistingPersonProgram.EffectiveDate;
                      }

                      if (Equal(local.ExistingProgram.Code, "NA"))
                      {
                        local.NonAdcPgmClosed.Flag = "Y";
                      }

                      local.StartNaCommon.Flag = "N";
                      local.RecomputeDistribution.Flag = "Y";

                      // 02/21/2002 Marek Lachowicz
                      // Enabled Madhu's changes
                      // *******************************************************
                      // As a part of PR # 00115534 we added a new action
                      // block  SI_BATCH_END_PRSN_PGM_START_NA_1
                      // which is a copy of  SI_BATCH_END_PRSN_PGM_START_NA with
                      // some changes.
                      //                       Madhu Kumar   05/08/2001
                      // *******************************************************
                      // *******************************************************
                      // Passed interface person program to the called CAB
                      // Marek Lachowicz   08/09/2002
                      // *******************************************************
                      UseSiBatchEndPrsnPgmStartNa1();

                      // 02/21/2002 Marek Lachowicz  End
                      if (IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        export.DatabaseUpdated.Flag = "Y";
                      }
                      else
                      {
                        goto Test4;
                      }
                    }
                  }
                }

                if (AsChar(local.ProgramExists.Flag) != 'Y')
                {
                  if (Equal(local.Program.Code, "AF") || Equal
                    (local.Program.Code, "NF") || Equal
                    (local.Program.Code, "FC"))
                  {
                    if (AsChar(local.NonAdcPgmClosed.Flag) == 'Y')
                    {
                      local.PersonProgram.ChangedInd = "A";
                    }
                    else
                    {
                      local.PersonProgram.ChangedInd = "";
                    }
                  }

                  local.RecomputeDistribution.Flag = "Y";
                  UseSiBatchCreatePersonPgm3();

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.DatabaseUpdated.Flag = "Y";
                    ++import.ExpCheckpointNumbUpdates.Count;

                    // ***************************************************************
                    // 05/21/1999  C. Ott    Added action block to create CSENet
                    // notification of Case Type change.
                    // ***************************************************************
                    local.SendToCsenetProgram.Code = local.Program.Code;
                    local.SendToCsenetCsePerson.Number =
                      local.CsePersonsWorkSet.Number;
                    UseSiPersPgmCreateCsenetTran();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "INTERSTATE_CASE_NF";

                      goto Test4;
                    }
                  }
                  else
                  {
                    goto Test4;
                  }
                }
              }
              else if (Equal(import.InterfacePersonProgram.ProgramCode, "AF") &&
                Equal(import.InterfacePersonProgram.AeProgramSubtype, "UP") || Equal
                (import.InterfacePersonProgram.ProgramCode, "MA") && Equal
                (import.InterfacePersonProgram.AeProgramSubtype, "UP"))
              {
                if (!Lt(local.Blank.Date, local.PersonProgram.DiscontinueDate))
                {
                  ExitState = "SI0000_DISCONTINUE_DATE_BLANK";

                  goto Test4;
                }

                foreach(var item in ReadPersonProgramProgram4())
                {
                  local.ExistingPersonProgram.Assign(entities.PersonProgram);
                  local.ExistingProgram.Code = entities.Program.Code;

                  if (Equal(local.ExistingProgram.Code, "AF") || Equal
                    (local.ExistingProgram.Code, "NF") || Equal
                    (local.ExistingProgram.Code, "FS") || Equal
                    (local.ExistingProgram.Code, "MA") || Equal
                    (local.ExistingProgram.Code, "MK"))
                  {
                    if (!Lt(local.ExistingPersonProgram.EffectiveDate,
                      local.PersonProgram.DiscontinueDate))
                    {
                      local.ExistingPersonProgram.DiscontinueDate =
                        AddDays(local.ExistingPersonProgram.EffectiveDate, 1);
                    }
                    else
                    {
                      local.ExistingPersonProgram.DiscontinueDate =
                        local.PersonProgram.DiscontinueDate;
                    }

                    local.StartNaCommon.Flag = "N";
                    local.RecomputeDistribution.Flag = "Y";
                    UseSiBatchEndPrsnPgmStartNa3();

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      export.DatabaseUpdated.Flag = "Y";
                    }
                    else
                    {
                      goto Test4;
                    }
                  }
                }

                // ****************************************************************
                // 11/23/1998   C. Ott     Add call to action block to create 
                // Person Program that is defined in Interface Person Program.
                // ****************************************************************
                local.RecomputeDistribution.Flag = "Y";
                UseSiBatchCreatePersonPgm1();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.DatabaseUpdated.Flag = "Y";
                  ++import.ExpCheckpointNumbUpdates.Count;
                }
                else
                {
                  goto Test4;
                }
              }

              // -----------------------------------------------------------
              // 10/19/1999  C. Ott     PR # 77321.   Update Case PA Medical 
              // Service from 'MO' to 'MC' on each Case where the person has an
              // active role as child when another AE program is opened on a
              // Medical Only case.
              // ----------------------------------------------------------
              if (Equal(import.InterfacePersonProgram.ProgramCode, "AF") || Equal
                (import.InterfacePersonProgram.ProgramCode, "CC") || Equal
                (import.InterfacePersonProgram.ProgramCode, "CI") || Equal
                (import.InterfacePersonProgram.ProgramCode, "FC") || Equal
                (import.InterfacePersonProgram.ProgramCode, "FS") || Equal
                (import.InterfacePersonProgram.ProgramCode, "SI"))
              {
                foreach(var item in ReadCase1())
                {
                  try
                  {
                    UpdateCase1();
                    export.DatabaseUpdated.Flag = "Y";
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

              break;
            case "OU":
              // ****************************************************************
              // A participation code of OU requires that a program is given
              // and the discontinue date.  To insure that the correct program
              // is discontinued, the effective date should also be given as
              // this date identifies the correct person program.
              // ***************************************************************
              if (!Lt(local.Blank.Date, local.PersonProgram.DiscontinueDate))
              {
                ExitState = "SI0000_DISCONTINUE_DATE_BLANK";

                goto Test4;
              }

              local.Program.Code = import.InterfacePersonProgram.ProgramCode;

              // ****************************************************************
              // Set flag for start of NA Program based on the Program Code of 
              // the Interface record.
              // ****************************************************************
              switch(TrimEnd(import.InterfacePersonProgram.ProgramCode))
              {
                case "AF":
                  // *****************************************************************
                  // 12/7/1998    C. Ott     Changed the following flag value 
                  // from "N" to "Y" based on discussions with Pam Vickers and
                  // Cheryl Deghand.
                  // ****************************************************************
                  local.StartNaCommon.Flag = "Y";

                  break;
                case "CC":
                  local.StartNaCommon.Flag = "Y";

                  break;
                case "CI":
                  local.StartNaCommon.Flag = "N";

                  break;
                case "FC":
                  local.StartNaCommon.Flag = "N";

                  if (Equal(import.InterfacePersonProgram.SourceOfFunds, "GA"))
                  {
                    local.Program.Code = "NF";
                  }
                  else
                  {
                    local.Program.Code = "FC";
                  }

                  break;
                case "FS":
                  local.StartNaCommon.Flag = "Y";

                  break;
                case "MA":
                  local.StartNaCommon.Flag = "Y";

                  break;
                case "MK":
                  local.StartNaCommon.Flag = "Y";

                  break;
                case "MP":
                  local.StartNaCommon.Flag = "Y";

                  break;
                case "MS":
                  local.StartNaCommon.Flag = "Y";

                  break;
                case "SI":
                  local.StartNaCommon.Flag = "Y";

                  break;
                default:
                  ExitState = "SI0000_PGM_AND_PART_CODE_NOT_DEF";

                  goto Test4;
              }

              // ****************************************************************
              // 10/21/99  C. Ott  PR # 77320.  When medical program closes on a
              // Medical Only (MO) case, do not rollover to NA, but send an
              // Alert.
              // ****************************************************************
              if (Equal(import.InterfacePersonProgram.ProgramCode, "MA") || Equal
                (import.InterfacePersonProgram.ProgramCode, "MK") || Equal
                (import.InterfacePersonProgram.ProgramCode, "MP") || Equal
                (import.InterfacePersonProgram.ProgramCode, "MS"))
              {
                // ****************************************************************
                // 10/21/99  C. Ott  PR # 77320.  Add check for any currently 
                // open AE program.
                // ****************************************************************
                foreach(var item in ReadPersonProgramProgram6())
                {
                  if (Equal(entities.Program.Code, "AF") || Equal
                    (entities.Program.Code, "CC") || Equal
                    (entities.Program.Code, "FS") || Equal
                    (entities.Program.Code, "MA") || Equal
                    (entities.Program.Code, "MK") || Equal
                    (entities.Program.Code, "MP") || Equal
                    (entities.Program.Code, "MS") || Equal
                    (entities.Program.Code, "SI"))
                  {
                    if (Equal(entities.Program.Code,
                      import.InterfacePersonProgram.ProgramCode))
                    {
                      // **************************************************************
                      // The incoming program closure has not been processed 
                      // yet.  This allows it to be bypassed in this check for
                      // other open programs.
                      // **************************************************************
                      continue;
                    }

                    goto Test3;
                  }
                }

                foreach(var item in ReadInterfacePersonProgram2())
                {
                  if (Equal(entities.InterfacePersonProgram.ProgramCode, "AF") ||
                    Equal
                    (entities.InterfacePersonProgram.ProgramCode, "CC") || Equal
                    (entities.InterfacePersonProgram.ProgramCode, "FS") || Equal
                    (entities.InterfacePersonProgram.ProgramCode, "MA") || Equal
                    (entities.InterfacePersonProgram.ProgramCode, "MK") || Equal
                    (entities.InterfacePersonProgram.ProgramCode, "MP") || Equal
                    (entities.InterfacePersonProgram.ProgramCode, "MS") || Equal
                    (entities.InterfacePersonProgram.ProgramCode, "SI"))
                  {
                    goto Test3;
                  }
                }

                foreach(var item in ReadCase1())
                {
                  try
                  {
                    UpdateCase2();
                    export.DatabaseUpdated.Flag = "Y";
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

                  local.StartNaCommon.Flag = "N";
                  local.Infrastructure.EventId = 5;
                  local.Infrastructure.ReasonCode = "MOCLOSE";
                  local.Infrastructure.ProcessStatus = "Q";
                  local.Infrastructure.BusinessObjectCd = "CAS";
                  local.Infrastructure.CaseNumber = entities.Case1.Number;
                  local.Infrastructure.CsePersonNumber =
                    import.InterfacePersonProgram.CsePersonNumber;
                  UseSpCabCreateInfrastructure();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.Error.Message = "MOCLOSE";

                    return;
                  }
                }
              }

Test3:

              // ***************************************************************
              // Call action block to close the Person Program defined by the 
              // Interface record.  The action block will create an NA Person
              // Program if there are no other active programs.
              // ****************************************************************
              local.RecomputeDistribution.Flag = "Y";
              UseSiBatchEndPrsnPgmStartNa2();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.DatabaseUpdated.Flag = "Y";

                if (AsChar(local.StartNaCommon.Flag) == 'Y')
                {
                  // ***************************************************************
                  // 05/21/1999  C. Ott    Added action block to create CSENet 
                  // notification of Case Type change.
                  // ***************************************************************
                  local.SendToCsenetProgram.Code = "NA";
                  local.SendToCsenetCsePerson.Number =
                    local.CsePersonsWorkSet.Number;
                  UseSiPersPgmCreateCsenetTran();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "INTERSTATE_CASE_NF";

                    goto Test4;
                  }
                }
              }
              else
              {
                goto Test4;
              }

              // ****************************************************************
              // 01/14/00  C. Ott  PR # 84608.  Create worker alert when FC 
              // program closes.
              // ****************************************************************
              if (Equal(import.InterfacePersonProgram.ProgramCode, "FC"))
              {
                foreach(var item in ReadCase3())
                {
                  local.Infrastructure.CaseNumber = entities.Case1.Number;
                  local.Infrastructure.EventId = 5;
                  local.Infrastructure.ReasonCode = "FOSTERCLOSE";
                  local.Infrastructure.BusinessObjectCd = "CAS";
                  local.Infrastructure.ProcessStatus = "Q";
                  local.Infrastructure.CsePersonNumber =
                    import.InterfacePersonProgram.CsePersonNumber;
                  UseSpCabCreateInfrastructure();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.Error.Message = "FOSTERCLOSE";

                    return;
                  }
                }
              }

              // ***************************************************************
              // 01/19/00  C. Ott  KSCares program does not have case closure 
              // reason code.  If CC program closes and no other PA program is
              // open, send PA Rollover letter.
              // ***************************************************************
              if (Equal(import.InterfacePersonProgram.ProgramCode, "CC"))
              {
                foreach(var item in ReadPersonProgramProgram6())
                {
                  if (Equal(entities.Program.Code, "AF") || Equal
                    (entities.Program.Code, "CC") || Equal
                    (entities.Program.Code, "FS") || Equal
                    (entities.Program.Code, "MA") || Equal
                    (entities.Program.Code, "MK") || Equal
                    (entities.Program.Code, "MP") || Equal
                    (entities.Program.Code, "MS") || Equal
                    (entities.Program.Code, "SI"))
                  {
                    local.PaProgramOpen.Flag = "Y";
                  }
                }

                if (IsEmpty(local.PaProgramOpen.Flag))
                {
                  local.SendPaRolloverLetter.Flag = "Y";
                }
              }

              break;
            case "WT":
              local.AdcPgmClosed.Flag = "N";

              if (Lt(import.InterfacePersonProgram.MedTypeDiscontinueDate,
                local.Current.Date) && !
                Equal(import.InterfacePersonProgram.MedTypeDiscontinueDate,
                local.Blank.Date))
              {
                // ***************************************************************
                // WT participation exists and should be closed
                // ***************************************************************
                foreach(var item in ReadPersonProgram6())
                {
                  // 04/18/00 M.L Start
                  // Changed import_interface_person_program 
                  // med_type_discontinue_date to
                  // local_interface_person_program med_type_discontinue_date
                  // 04/18/00 M.L End
                  try
                  {
                    UpdatePersonProgram3();
                    export.DatabaseUpdated.Flag = "Y";

                    // ***************************************************************
                    // 01/19/00  C. Ott   If Med Type closes but did not have a 
                    // closure reason code, send PA rollover letter if no other
                    // programs open .
                    // ***************************************************************
                    foreach(var item1 in ReadPersonProgramProgram6())
                    {
                      if (Equal(entities.Program.Code, "AF") || Equal
                        (entities.Program.Code, "CC") || Equal
                        (entities.Program.Code, "FS") || Equal
                        (entities.Program.Code, "MA") || Equal
                        (entities.Program.Code, "MK") || Equal
                        (entities.Program.Code, "MP") || Equal
                        (entities.Program.Code, "MS") || Equal
                        (entities.Program.Code, "SI"))
                      {
                        local.PaProgramOpen.Flag = "Y";
                      }
                    }

                    if (IsEmpty(local.PaProgramOpen.Flag))
                    {
                      local.SendPaRolloverLetter.Flag = "Y";
                    }

                    goto Test4;
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "PERSON_PROGRAM_AE";

                        goto Test4;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "PERSON_PROGRAM_PV";

                        goto Test4;
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
                // *****************************************************************
                // 11/17/99   C. Ott   Look for a currently closed AF or MA 
                // program.
                // ****************************************************************
                if (ReadPersonProgram1())
                {
                  // ***************************************************************
                  // Person Program is already closed, update Med Type.
                  // ***************************************************************
                  // 04/18/00 M.L Start
                  // Changed import_interface_person_program 
                  // med_type_discontinue_date to
                  // local_interface_person_program med_type_discontinue_date
                  // 04/18/00 M.L End
                  try
                  {
                    UpdatePersonProgram3();
                    export.DatabaseUpdated.Flag = "Y";
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "PERSON_PROGRAM_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "PERSON_PROGRAM_PV";

                        break;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
                else
                {
                  // *****************************************************************
                  // 11/17/99   C. Ott   A currently closed AF or MA person 
                  // program with existing Med Type was not found.  Look first
                  // for AF program closed within one year and if not found look
                  // for MA program closed within one year.
                  // ****************************************************************
                  foreach(var item in ReadPersonProgram8())
                  {
                    if (Lt(AddYears(local.Current.Date, -1),
                      entities.PersonProgram.DiscontinueDate))
                    {
                      // 04/18/00 M.L Start
                      // Changed import_interface_person_program 
                      // med_type_discontinue_date to
                      // local_interface_person_program
                      // med_type_discontinue_date
                      // 04/18/00 M.L End
                      try
                      {
                        UpdatePersonProgram3();
                        export.DatabaseUpdated.Flag = "Y";

                        goto Test4;
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "PERSON_PROGRAM_AE";

                            goto Test4;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "PERSON_PROGRAM_PV";

                            goto Test4;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }
                  }

                  foreach(var item in ReadPersonProgram9())
                  {
                    if (Lt(AddYears(local.Current.Date, -1),
                      entities.PersonProgram.DiscontinueDate))
                    {
                      // 04/18/00 M.L Start
                      // Changed import_interface_person_program 
                      // med_type_discontinue_date to
                      // local_interface_person_program
                      // med_type_discontinue_date
                      // 04/18/00 M.L End
                      try
                      {
                        UpdatePersonProgram3();
                        export.DatabaseUpdated.Flag = "Y";

                        goto Test4;
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "PERSON_PROGRAM_AE";

                            goto Test4;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "PERSON_PROGRAM_PV";

                            goto Test4;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }
                  }

                  ExitState = "INVALID_PROGRAM";
                }
              }

              break;
            default:
              ExitState = "SI0000_PARTICIPATION_CDE_NOT_DEF";

              return;
          }
        }
        else
        {
          ExitState = "CSE_PERSON_NF";

          return;
        }
      }

      // 09/12/00 M.L Start
      // Do not send ADCROLLO or PAROLLO letters.
    }
    else
    {
      return;
    }

Test4:

    // ¼¼¼¼¼¼¼¼¼¼¼¼
    // Added IF stmt... SAK 7/28/97
    // ©©©©©©©©©©©©
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // **********************************************
      // Code put in by Sid. 05/12/1997.
      // Set the ADC Closed Date, if there are no more
      // ADC programs open for that Case.
      // **********************************************
      if (Equal(import.InterfacePersonProgram.ProgramCode, "AF") && Equal
        (import.InterfacePersonProgram.ParticipationCode, "IN"))
      {
        if (AsChar(local.BypassAdcOpenAlert.Flag) == 'Y')
        {
          goto Test5;
        }

        local.OpenClosedAdc.Flag = "O";
        UseSiSetCaseAdcOpenCloseDates();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

Test5:

    // ¼¼¼¼¼¼¼¼¼¼¼¼
    // Added IF stmt... SAK 7/28/97
    // ©©©©©©©©©©©©
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (ReadInterfacePersonProgram1())
      {
        try
        {
          UpdateInterfacePersonProgram();
          export.DatabaseUpdated.Flag = "Y";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SI0000_INTERFACE_PERSON_PRGM_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SI0000_INTERFACE_PERSON_PRGM_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "SI0000_INTERFACE_PERSON_PRGM_NF";
      }
    }
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

  private static void MoveInterfacePersonProgram(InterfacePersonProgram source,
    InterfacePersonProgram target)
  {
    target.ProgramCode = source.ProgramCode;
    target.CsePersonNumber = source.CsePersonNumber;
    target.ClosureReason = source.ClosureReason;
    target.ProgEffectiveDate = source.ProgEffectiveDate;
    target.ProgramEndDate = source.ProgramEndDate;
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
    target.ClosureReason = source.ClosureReason;
    target.EffectiveDate = source.EffectiveDate;
    target.AssignedDate = source.AssignedDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePersonProgram3(PersonProgram source,
    PersonProgram target)
  {
    target.ClosureReason = source.ClosureReason;
    target.EffectiveDate = source.EffectiveDate;
    target.AssignedDate = source.AssignedDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.ChangedInd = source.ChangedInd;
    target.MedTypeDiscontinueDate = source.MedTypeDiscontinueDate;
    target.MedType = source.MedType;
  }

  private static void MovePersonProgram4(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePersonProgram5(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.ChangedInd = source.ChangedInd;
  }

  private static void MoveProgram1(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveProgram2(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiBatchCreatePersonPgm1()
  {
    var useImport = new SiBatchCreatePersonPgm.Import();
    var useExport = new SiBatchCreatePersonPgm.Export();

    useImport.CntlTotPersPgmCreates.Count = import.CntlTotPersPgmCreates.Count;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Program.Code = local.Program.Code;
    MovePersonProgram3(local.PersonProgram, useImport.PersonProgram);
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;

    Call(SiBatchCreatePersonPgm.Execute, useImport, useExport);

    export.CntlTotPersPgmCreates.Count = useExport.CntlTotPersPgmCreates.Count;
  }

  private void UseSiBatchCreatePersonPgm2()
  {
    var useImport = new SiBatchCreatePersonPgm.Import();
    var useExport = new SiBatchCreatePersonPgm.Export();

    useImport.History.Flag = local.History.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Program.Code = local.Program.Code;
    MovePersonProgram3(local.PersonProgram, useImport.PersonProgram);
    useImport.CntlTotPersPgmCreates.Count = export.CntlTotPersPgmCreates.Count;
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;

    Call(SiBatchCreatePersonPgm.Execute, useImport, useExport);

    export.CntlTotPersPgmCreates.Count = useExport.CntlTotPersPgmCreates.Count;
  }

  private void UseSiBatchCreatePersonPgm3()
  {
    var useImport = new SiBatchCreatePersonPgm.Import();
    var useExport = new SiBatchCreatePersonPgm.Export();

    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Program.Code = local.Program.Code;
    MovePersonProgram3(local.PersonProgram, useImport.PersonProgram);
    useImport.CntlTotPersPgmCreates.Count = export.CntlTotPersPgmCreates.Count;

    Call(SiBatchCreatePersonPgm.Execute, useImport, useExport);

    export.CntlTotPersPgmCreates.Count = useExport.CntlTotPersPgmCreates.Count;
  }

  private void UseSiBatchEndPrsnPgmStartNa2()
  {
    var useImport = new SiBatchEndPrsnPgmStartNa.Import();
    var useExport = new SiBatchEndPrsnPgmStartNa.Export();

    useImport.ExpCheckpointNumbUpdates.Count =
      import.ExpCheckpointNumbUpdates.Count;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.StartNa.Flag = local.StartNaCommon.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Program.Code = local.Program.Code;
    useImport.PersonProgram.Assign(local.PersonProgram);
    useImport.CntlTotPersPgmCloses.Count = export.CntlTotPersPgmCloses.Count;
    useImport.CntlTotPersPgmCreates.Count = export.CntlTotPersPgmCreates.Count;
    useImport.InterfacePersonProgram.Assign(import.InterfacePersonProgram);
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;

    Call(SiBatchEndPrsnPgmStartNa.Execute, useImport, useExport);

    import.ExpCheckpointNumbUpdates.Count =
      useImport.ExpCheckpointNumbUpdates.Count;
    export.CntlTotPersPgmCloses.Count = useExport.CntlTotPersPgmCloses.Count;
    export.CntlTotPersPgmCreates.Count = useExport.CntlTotPersPgmCreates.Count;
    local.BypassAdcOpenAlert.Flag = useExport.BypassAdcOpenAlert.Flag;
  }

  private void UseSiBatchEndPrsnPgmStartNa3()
  {
    var useImport = new SiBatchEndPrsnPgmStartNa.Import();
    var useExport = new SiBatchEndPrsnPgmStartNa.Export();

    useImport.ExpCheckpointNumbUpdates.Count =
      import.ExpCheckpointNumbUpdates.Count;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;
    useImport.StartNa.Flag = local.StartNaCommon.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Program.Code = local.ExistingProgram.Code;
    useImport.PersonProgram.Assign(local.ExistingPersonProgram);
    useImport.CntlTotPersPgmCloses.Count = export.CntlTotPersPgmCloses.Count;
    useImport.CntlTotPersPgmCreates.Count = export.CntlTotPersPgmCreates.Count;

    Call(SiBatchEndPrsnPgmStartNa.Execute, useImport, useExport);

    import.ExpCheckpointNumbUpdates.Count =
      useImport.ExpCheckpointNumbUpdates.Count;
    local.BypassAdcOpenAlert.Flag = useExport.BypassAdcOpenAlert.Flag;
    export.CntlTotPersPgmCloses.Count = useExport.CntlTotPersPgmCloses.Count;
    export.CntlTotPersPgmCreates.Count = useExport.CntlTotPersPgmCreates.Count;
  }

  private void UseSiBatchEndPrsnPgmStartNa1()
  {
    var useImport = new SiBatchEndPrsnPgmStartNa1.Import();
    var useExport = new SiBatchEndPrsnPgmStartNa1.Export();

    useImport.ExpCheckpointNumbUpdates.Count =
      import.ExpCheckpointNumbUpdates.Count;
    useImport.CntlTotPersPgmCloses.Count = export.CntlTotPersPgmCloses.Count;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Program.Code = local.ExistingProgram.Code;
    useImport.PersonProgram.Assign(local.ExistingPersonProgram);
    useImport.StartNa.Flag = local.StartNaCommon.Flag;
    useImport.CntlTotPersPgmCreates.Count = export.CntlTotPersPgmCreates.Count;
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;
    useImport.InterfacePersonProgram.Assign(import.InterfacePersonProgram);

    Call(SiBatchEndPrsnPgmStartNa1.Execute, useImport, useExport);

    import.ExpCheckpointNumbUpdates.Count =
      useImport.ExpCheckpointNumbUpdates.Count;
    export.CntlTotPersPgmCloses.Count = useExport.CntlTotPersPgmCloses.Count;
    export.CntlTotPersPgmCreates.Count = useExport.CntlTotPersPgmCreates.Count;
    local.BypassAdcOpenAlert.Flag = useExport.BypassAdcOpenAlert.Flag;
  }

  private void UseSiBatchStartNaPrsnPgm()
  {
    var useImport = new SiBatchStartNaPrsnPgm.Import();
    var useExport = new SiBatchStartNaPrsnPgm.Export();

    useImport.ExpCheckpointNumbUpdates.Count =
      import.ExpCheckpointNumbUpdates.Count;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    MovePersonProgram5(local.StartNaPersonProgram, useImport.PersonProgram);
    useImport.CntlTotPersPgmCreates.Count = export.CntlTotPersPgmCreates.Count;
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;

    Call(SiBatchStartNaPrsnPgm.Execute, useImport, useExport);

    import.ExpCheckpointNumbUpdates.Count =
      useImport.ExpCheckpointNumbUpdates.Count;
    export.CntlTotPersPgmCreates.Count = useExport.CntlTotPersPgmCreates.Count;
  }

  private void UseSiBatchUpdatePersonPgm()
  {
    var useImport = new SiBatchUpdatePersonPgm.Import();
    var useExport = new SiBatchUpdatePersonPgm.Export();

    useImport.CntlTotPersPgmCloses.Count = export.CntlTotPersPgmCloses.Count;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.PersonProgram.Assign(local.PersonProgram);
    useImport.Program.Code = local.Program.Code;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;

    Call(SiBatchUpdatePersonPgm.Execute, useImport, useExport);

    MovePersonProgram2(useExport.PersonProgram, local.PersonProgram);
    export.CntlTotPersPgmCloses.Count = useExport.CntlTotPersPgmCloses.Count;
    MoveProgram2(useExport.Program, local.Program);
    local.AeCaseDenied.Flag = useExport.AeCaseDenied.Flag;
  }

  private void UseSiPeprDeletePersonProgram1()
  {
    var useImport = new SiPeprDeletePersonProgram.Import();
    var useExport = new SiPeprDeletePersonProgram.Export();

    MovePersonProgram1(entities.PersonProgram, useImport.PersonProgram);
    MoveProgram1(entities.Program, useImport.Program);
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiPeprDeletePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPeprDeletePersonProgram2()
  {
    var useImport = new SiPeprDeletePersonProgram.Import();
    var useExport = new SiPeprDeletePersonProgram.Export();

    MovePersonProgram1(entities.PersonProgram, useImport.PersonProgram);
    useImport.RecomputeDistribution.Flag = local.RecomputeDistribution.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Program.Code = local.Program.Code;

    Call(SiPeprDeletePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPersPgmCreateCsenetTran()
  {
    var useImport = new SiPersPgmCreateCsenetTran.Import();
    var useExport = new SiPersPgmCreateCsenetTran.Export();

    useImport.CsePerson.Number = local.SendToCsenetCsePerson.Number;
    useImport.Program.Code = local.SendToCsenetProgram.Code;

    Call(SiPersPgmCreateCsenetTran.Execute, useImport, useExport);
  }

  private void UseSiSetCaseAdcOpenCloseDates()
  {
    var useImport = new SiSetCaseAdcOpenCloseDates.Import();
    var useExport = new SiSetCaseAdcOpenCloseDates.Export();

    useImport.ExpCheckpointNumbUpdates.Count =
      import.ExpCheckpointNumbUpdates.Count;
    MoveInterfacePersonProgram(import.InterfacePersonProgram,
      useImport.InterfacePersonProgram);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.OpenClosedAdc.Flag = local.OpenClosedAdc.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.CntlTotAdcCaseOpens.Count = export.CntlTotAdcCaseOpens.Count;
    useImport.CntlTotAdcCaseCloses.Count = export.CntlTotAdcCaseCloses.Count;

    Call(SiSetCaseAdcOpenCloseDates.Execute, useImport, useExport);

    import.ExpCheckpointNumbUpdates.Count =
      useImport.ExpCheckpointNumbUpdates.Count;
    export.CntlTotAdcCaseOpens.Count = useExport.CntlTotAdcCaseOpens.Count;
    export.CntlTotAdcCaseCloses.Count = useExport.CntlTotAdcCaseCloses.Count;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase1()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 1);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Scan.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 1);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase3()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase3",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 1);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Scan.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.FcIvECaseNumber = db.GetNullableString(reader, 6);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Scan.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", import.InterfacePersonProgram.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.Scan.Number = db.GetString(reader, 0);
        entities.Scan.Type1 = db.GetString(reader, 1);
        entities.Scan.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.Scan.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Scan.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Scan.KscaresNumber = db.GetNullableString(reader, 5);
        entities.Scan.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Scan.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.Ar.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.AeCaseNumber = db.GetNullableString(reader, 2);
        entities.Ar.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 3);
        entities.Ar.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Ar.KscaresNumber = db.GetNullableString(reader, 5);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);

        return true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
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

  private bool ReadInterfacePersonProgram1()
  {
    entities.InterfacePersonProgram.Populated = false;

    return Read("ReadInterfacePersonProgram1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
        db.SetString(
          command, "programCode", import.InterfacePersonProgram.ProgramCode);
        db.SetDateTime(
          command, "createdTimestamp",
          import.InterfacePersonProgram.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterfacePersonProgram.RecordType = db.GetString(reader, 0);
        entities.InterfacePersonProgram.SourceOfFunds =
          db.GetNullableString(reader, 1);
        entities.InterfacePersonProgram.ProgramCode = db.GetString(reader, 2);
        entities.InterfacePersonProgram.StatusInd =
          db.GetNullableString(reader, 3);
        entities.InterfacePersonProgram.ClosureReason =
          db.GetNullableString(reader, 4);
        entities.InterfacePersonProgram.From = db.GetNullableString(reader, 5);
        entities.InterfacePersonProgram.ProgEffectiveDate =
          db.GetDate(reader, 6);
        entities.InterfacePersonProgram.ProgramEndDate = db.GetDate(reader, 7);
        entities.InterfacePersonProgram.CreatedBy =
          db.GetNullableString(reader, 8);
        entities.InterfacePersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.InterfacePersonProgram.ProcessDate =
          db.GetNullableDate(reader, 10);
        entities.InterfacePersonProgram.AssignedDate =
          db.GetNullableDate(reader, 11);
        entities.InterfacePersonProgram.ParticipationCode =
          db.GetString(reader, 12);
        entities.InterfacePersonProgram.AeProgramSubtype =
          db.GetNullableString(reader, 13);
        entities.InterfacePersonProgram.CsePersonNumber =
          db.GetString(reader, 14);
        entities.InterfacePersonProgram.PaCaseNumber =
          db.GetNullableString(reader, 15);
        entities.InterfacePersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 16);
        entities.InterfacePersonProgram.MedType =
          db.GetNullableString(reader, 17);
        entities.InterfacePersonProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterfacePersonProgram2()
  {
    entities.InterfacePersonProgram.Populated = false;

    return ReadEach("ReadInterfacePersonProgram2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.InterfacePersonProgram.RecordType = db.GetString(reader, 0);
        entities.InterfacePersonProgram.SourceOfFunds =
          db.GetNullableString(reader, 1);
        entities.InterfacePersonProgram.ProgramCode = db.GetString(reader, 2);
        entities.InterfacePersonProgram.StatusInd =
          db.GetNullableString(reader, 3);
        entities.InterfacePersonProgram.ClosureReason =
          db.GetNullableString(reader, 4);
        entities.InterfacePersonProgram.From = db.GetNullableString(reader, 5);
        entities.InterfacePersonProgram.ProgEffectiveDate =
          db.GetDate(reader, 6);
        entities.InterfacePersonProgram.ProgramEndDate = db.GetDate(reader, 7);
        entities.InterfacePersonProgram.CreatedBy =
          db.GetNullableString(reader, 8);
        entities.InterfacePersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.InterfacePersonProgram.ProcessDate =
          db.GetNullableDate(reader, 10);
        entities.InterfacePersonProgram.AssignedDate =
          db.GetNullableDate(reader, 11);
        entities.InterfacePersonProgram.ParticipationCode =
          db.GetString(reader, 12);
        entities.InterfacePersonProgram.AeProgramSubtype =
          db.GetNullableString(reader, 13);
        entities.InterfacePersonProgram.CsePersonNumber =
          db.GetString(reader, 14);
        entities.InterfacePersonProgram.PaCaseNumber =
          db.GetNullableString(reader, 15);
        entities.InterfacePersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 16);
        entities.InterfacePersonProgram.MedType =
          db.GetNullableString(reader, 17);
        entities.InterfacePersonProgram.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.
          SetString(command, "code", import.InterfacePersonProgram.ProgramCode);
          
        db.SetDate(
          command, "progEffectiveDate",
          import.InterfacePersonProgram.ProgEffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Scan.Number);
        db.SetString(command, "code", local.Program.Code);
        db.SetDate(
          command, "effectiveDate",
          local.PersonProgram.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.PersonProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgram3()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram3",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
        db.
          SetString(command, "code", import.InterfacePersonProgram.ProgramCode);
          
        db.SetDate(
          command, "effectiveDate",
          import.InterfacePersonProgram.ProgEffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgram4()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram4",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
        db.SetDate(
          command, "programEndDate",
          import.InterfacePersonProgram.ProgramEndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgram5()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgram6()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgram7()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram7",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
        db.
          SetString(command, "code", import.InterfacePersonProgram.ProgramCode);
          
        db.SetNullableString(
          command, "medType", import.InterfacePersonProgram.MedType ?? "");
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgram8()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram8",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgram9()
  {
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgram9",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgramProgram1()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.
          SetString(command, "code", import.InterfacePersonProgram.ProgramCode);
          
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
        db.SetDate(
          command, "effectiveDate",
          import.InterfacePersonProgram.ProgEffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.Program.Code = db.GetString(reader, 13);
        entities.Program.DistributionProgramType = db.GetString(reader, 14);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram2()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
        db.SetDate(
          command, "effectiveDate",
          local.PersonProgram.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.Program.Code = db.GetString(reader, 13);
        entities.Program.DistributionProgramType = db.GetString(reader, 14);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram3()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          import.InterfacePersonProgram.ProgEffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.Program.Code = db.GetString(reader, 13);
        entities.Program.DistributionProgramType = db.GetString(reader, 14);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram4()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.PersonProgram.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.Program.Code = db.GetString(reader, 13);
        entities.Program.DistributionProgramType = db.GetString(reader, 14);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram5()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Scan.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.Program.Code = db.GetString(reader, 13);
        entities.Program.DistributionProgramType = db.GetString(reader, 14);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram6()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram6",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
        db.SetNullableDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.Program.Code = db.GetString(reader, 13);
        entities.Program.DistributionProgramType = db.GetString(reader, 14);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram7()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return ReadEach("ReadPersonProgramProgram7",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.InterfacePersonProgram.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.ClosureReason = db.GetNullableString(reader, 2);
        entities.PersonProgram.AssignedDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PersonProgram.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 8);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 9);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 10);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 12);
        entities.Program.Code = db.GetString(reader, 13);
        entities.Program.DistributionProgramType = db.GetString(reader, 14);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private void UpdateCase1()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var paMedicalService = "MC";

    entities.Case1.Populated = false;
    Update("UpdateCase1",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "paMedicalService", paMedicalService);
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.PaMedicalService = paMedicalService;
    entities.Case1.Populated = true;
  }

  private void UpdateCase2()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.Case1.Populated = false;
    Update("UpdateCase2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "paMedicalService", "");
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.PaMedicalService = "";
    entities.Case1.Populated = true;
  }

  private void UpdateCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var fcIvECaseNumber = import.InterfacePersonProgram.PaCaseNumber ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.CaseRole.Populated = false;
    Update("UpdateCaseRole",
      (db, command) =>
      {
        db.SetNullableString(command, "fcIvECaseNo", fcIvECaseNumber);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "type", entities.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CaseRole.Identifier);
      });

    entities.CaseRole.FcIvECaseNumber = fcIvECaseNumber;
    entities.CaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CaseRole.Populated = true;
  }

  private void UpdateCsePerson1()
  {
    var aeCaseNumber = import.InterfacePersonProgram.PaCaseNumber ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.Ar.Populated = false;
    Update("UpdateCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "aeCaseNumber", aeCaseNumber);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.Ar.Number);
      });

    entities.Ar.AeCaseNumber = aeCaseNumber;
    entities.Ar.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Ar.LastUpdatedBy = lastUpdatedBy;
    entities.Ar.Populated = true;
  }

  private void UpdateCsePerson2()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var kscaresNumber = import.InterfacePersonProgram.PaCaseNumber ?? "";

    entities.Ar.Populated = false;
    Update("UpdateCsePerson2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "kscaresNumber", kscaresNumber);
        db.SetString(command, "numb", entities.Ar.Number);
      });

    entities.Ar.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Ar.LastUpdatedBy = lastUpdatedBy;
    entities.Ar.KscaresNumber = kscaresNumber;
    entities.Ar.Populated = true;
  }

  private void UpdateCsePerson3()
  {
    var aeCaseNumber = import.InterfacePersonProgram.PaCaseNumber ?? "";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.Scan.Populated = false;
    Update("UpdateCsePerson3",
      (db, command) =>
      {
        db.SetNullableString(command, "aeCaseNumber", aeCaseNumber);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.Scan.Number);
      });

    entities.Scan.AeCaseNumber = aeCaseNumber;
    entities.Scan.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Scan.LastUpdatedBy = lastUpdatedBy;
    entities.Scan.Populated = true;
  }

  private void UpdateCsePerson4()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var kscaresNumber = import.InterfacePersonProgram.PaCaseNumber ?? "";

    entities.Scan.Populated = false;
    Update("UpdateCsePerson4",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "kscaresNumber", kscaresNumber);
        db.SetString(command, "numb", entities.Scan.Number);
      });

    entities.Scan.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Scan.LastUpdatedBy = lastUpdatedBy;
    entities.Scan.KscaresNumber = kscaresNumber;
    entities.Scan.Populated = true;
  }

  private void UpdateCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var pgmChgEffectiveDate = AddDays(local.Tmp.DiscontinueDate, 1);

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

  private void UpdateInterfacePersonProgram()
  {
    var statusInd = "P";
    var processDate = import.ProgramProcessingInfo.ProcessDate;

    entities.InterfacePersonProgram.Populated = false;
    Update("UpdateInterfacePersonProgram",
      (db, command) =>
      {
        db.SetNullableString(command, "statusInd", statusInd);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(
          command, "programCode", entities.InterfacePersonProgram.ProgramCode);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.InterfacePersonProgram.CreatedTimestamp.GetValueOrDefault());
          
        db.SetString(
          command, "cspNumber",
          entities.InterfacePersonProgram.CsePersonNumber);
      });

    entities.InterfacePersonProgram.StatusInd = statusInd;
    entities.InterfacePersonProgram.ProcessDate = processDate;
    entities.InterfacePersonProgram.Populated = true;
  }

  private void UpdatePersonProgram1()
  {
    System.Diagnostics.Debug.Assert(entities.PersonProgram.Populated);

    var closureReason = local.PersonProgram.ClosureReason ?? "";
    var discontinueDate = local.PersonProgram.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var medTypeDiscontinueDate = local.PersonProgram.MedTypeDiscontinueDate;
    var medType = local.PersonProgram.MedType ?? "";

    entities.PersonProgram.Populated = false;
    Update("UpdatePersonProgram1",
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

  private void UpdatePersonProgram2()
  {
    System.Diagnostics.Debug.Assert(entities.PersonProgram.Populated);

    var discontinueDate = local.PersonProgram.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var medTypeDiscontinueDate =
      local.InterfacePersonProgram.MedTypeDiscontinueDate;
    var medType = import.InterfacePersonProgram.MedType ?? "";

    entities.PersonProgram.Populated = false;
    Update("UpdatePersonProgram2",
      (db, command) =>
      {
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

    entities.PersonProgram.DiscontinueDate = discontinueDate;
    entities.PersonProgram.LastUpdatedBy = lastUpdatedBy;
    entities.PersonProgram.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.PersonProgram.MedTypeDiscontinueDate = medTypeDiscontinueDate;
    entities.PersonProgram.MedType = medType;
    entities.PersonProgram.Populated = true;
  }

  private void UpdatePersonProgram3()
  {
    System.Diagnostics.Debug.Assert(entities.PersonProgram.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var medTypeDiscontinueDate =
      local.InterfacePersonProgram.MedTypeDiscontinueDate;
    var medType = import.InterfacePersonProgram.MedType ?? "";

    entities.PersonProgram.Populated = false;
    Update("UpdatePersonProgram3",
      (db, command) =>
      {
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

    entities.PersonProgram.LastUpdatedBy = lastUpdatedBy;
    entities.PersonProgram.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.PersonProgram.MedTypeDiscontinueDate = medTypeDiscontinueDate;
    entities.PersonProgram.MedType = medType;
    entities.PersonProgram.Populated = true;
  }

  private void UpdatePersonProgram4()
  {
    System.Diagnostics.Debug.Assert(entities.PersonProgram.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var medTypeDiscontinueDate = local.Blank.Date;

    entities.PersonProgram.Populated = false;
    Update("UpdatePersonProgram4",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableDate(command, "medTypeDiscDate", medTypeDiscontinueDate);
        db.SetNullableString(command, "medType", "");
        db.SetString(command, "cspNumber", entities.PersonProgram.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PersonProgram.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.PersonProgram.PrgGeneratedId);
      });

    entities.PersonProgram.LastUpdatedBy = lastUpdatedBy;
    entities.PersonProgram.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.PersonProgram.MedTypeDiscontinueDate = medTypeDiscontinueDate;
    entities.PersonProgram.MedType = "";
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
    /// A value of ExpAdcRolloverLetterSent.
    /// </summary>
    [JsonPropertyName("expAdcRolloverLetterSent")]
    public Common ExpAdcRolloverLetterSent
    {
      get => expAdcRolloverLetterSent ??= new();
      set => expAdcRolloverLetterSent = value;
    }

    /// <summary>
    /// A value of ExpPaRolloverLetterSent.
    /// </summary>
    [JsonPropertyName("expPaRolloverLetterSent")]
    public Common ExpPaRolloverLetterSent
    {
      get => expPaRolloverLetterSent ??= new();
      set => expPaRolloverLetterSent = value;
    }

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
    /// A value of CntlTotCsePersUpdates.
    /// </summary>
    [JsonPropertyName("cntlTotCsePersUpdates")]
    public Common CntlTotCsePersUpdates
    {
      get => cntlTotCsePersUpdates ??= new();
      set => cntlTotCsePersUpdates = value;
    }

    /// <summary>
    /// A value of CntlTotAdcCaseOpens.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseOpens")]
    public Common CntlTotAdcCaseOpens
    {
      get => cntlTotAdcCaseOpens ??= new();
      set => cntlTotAdcCaseOpens = value;
    }

    /// <summary>
    /// A value of CntlTotAdcCaseCloses.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseCloses")]
    public Common CntlTotAdcCaseCloses
    {
      get => cntlTotAdcCaseCloses ??= new();
      set => cntlTotAdcCaseCloses = value;
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
    /// A value of InterfacePersonProgram.
    /// </summary>
    [JsonPropertyName("interfacePersonProgram")]
    public InterfacePersonProgram InterfacePersonProgram
    {
      get => interfacePersonProgram ??= new();
      set => interfacePersonProgram = value;
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
    /// A value of CntlTotPersPgmCreates.
    /// </summary>
    [JsonPropertyName("cntlTotPersPgmCreates")]
    public Common CntlTotPersPgmCreates
    {
      get => cntlTotPersPgmCreates ??= new();
      set => cntlTotPersPgmCreates = value;
    }

    private Common expAdcRolloverLetterSent;
    private Common expPaRolloverLetterSent;
    private Common expCheckpointNumbUpdates;
    private Common cntlTotCsePersUpdates;
    private Common cntlTotAdcCaseOpens;
    private Common cntlTotAdcCaseCloses;
    private Common cntlTotPersPgmCloses;
    private InterfacePersonProgram interfacePersonProgram;
    private ProgramProcessingInfo programProcessingInfo;
    private Common cntlTotPersPgmCreates;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public ExitStateWorkArea Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of CntlTotCsePersUpdates.
    /// </summary>
    [JsonPropertyName("cntlTotCsePersUpdates")]
    public Common CntlTotCsePersUpdates
    {
      get => cntlTotCsePersUpdates ??= new();
      set => cntlTotCsePersUpdates = value;
    }

    /// <summary>
    /// A value of CntlTotAdcCaseOpens.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseOpens")]
    public Common CntlTotAdcCaseOpens
    {
      get => cntlTotAdcCaseOpens ??= new();
      set => cntlTotAdcCaseOpens = value;
    }

    /// <summary>
    /// A value of CntlTotAdcCaseCloses.
    /// </summary>
    [JsonPropertyName("cntlTotAdcCaseCloses")]
    public Common CntlTotAdcCaseCloses
    {
      get => cntlTotAdcCaseCloses ??= new();
      set => cntlTotAdcCaseCloses = value;
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

    private ExitStateWorkArea error;
    private Common cntlTotCsePersUpdates;
    private Common cntlTotAdcCaseOpens;
    private Common cntlTotAdcCaseCloses;
    private Common cntlTotPersPgmCloses;
    private Common cntlTotPersPgmCreates;
    private Common databaseUpdated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of AeCaseDenied.
    /// </summary>
    [JsonPropertyName("aeCaseDenied")]
    public Common AeCaseDenied
    {
      get => aeCaseDenied ??= new();
      set => aeCaseDenied = value;
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

    /// <summary>
    /// A value of PaProgramOpen.
    /// </summary>
    [JsonPropertyName("paProgramOpen")]
    public Common PaProgramOpen
    {
      get => paProgramOpen ??= new();
      set => paProgramOpen = value;
    }

    /// <summary>
    /// A value of AeProgramOpen.
    /// </summary>
    [JsonPropertyName("aeProgramOpen")]
    public Common AeProgramOpen
    {
      get => aeProgramOpen ??= new();
      set => aeProgramOpen = value;
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

    /// <summary>
    /// A value of RecomputeDistribution.
    /// </summary>
    [JsonPropertyName("recomputeDistribution")]
    public Common RecomputeDistribution
    {
      get => recomputeDistribution ??= new();
      set => recomputeDistribution = value;
    }

    /// <summary>
    /// A value of SendToCsenetCsePerson.
    /// </summary>
    [JsonPropertyName("sendToCsenetCsePerson")]
    public CsePerson SendToCsenetCsePerson
    {
      get => sendToCsenetCsePerson ??= new();
      set => sendToCsenetCsePerson = value;
    }

    /// <summary>
    /// A value of SendToCsenetProgram.
    /// </summary>
    [JsonPropertyName("sendToCsenetProgram")]
    public Program SendToCsenetProgram
    {
      get => sendToCsenetProgram ??= new();
      set => sendToCsenetProgram = value;
    }

    /// <summary>
    /// A value of ConvertBatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("convertBatchConvertNumToText")]
    public BatchConvertNumToText ConvertBatchConvertNumToText
    {
      get => convertBatchConvertNumToText ??= new();
      set => convertBatchConvertNumToText = value;
    }

    /// <summary>
    /// A value of ConvertDateWorkArea.
    /// </summary>
    [JsonPropertyName("convertDateWorkArea")]
    public DateWorkArea ConvertDateWorkArea
    {
      get => convertDateWorkArea ??= new();
      set => convertDateWorkArea = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of CaseClosureStartNa.
    /// </summary>
    [JsonPropertyName("caseClosureStartNa")]
    public Common CaseClosureStartNa
    {
      get => caseClosureStartNa ??= new();
      set => caseClosureStartNa = value;
    }

    /// <summary>
    /// A value of SendPaRolloverLetter.
    /// </summary>
    [JsonPropertyName("sendPaRolloverLetter")]
    public Common SendPaRolloverLetter
    {
      get => sendPaRolloverLetter ??= new();
      set => sendPaRolloverLetter = value;
    }

    /// <summary>
    /// A value of SendAdcRolloverLetter.
    /// </summary>
    [JsonPropertyName("sendAdcRolloverLetter")]
    public Common SendAdcRolloverLetter
    {
      get => sendAdcRolloverLetter ??= new();
      set => sendAdcRolloverLetter = value;
    }

    /// <summary>
    /// A value of History.
    /// </summary>
    [JsonPropertyName("history")]
    public Common History
    {
      get => history ??= new();
      set => history = value;
    }

    /// <summary>
    /// A value of OpenClosedAdc.
    /// </summary>
    [JsonPropertyName("openClosedAdc")]
    public Common OpenClosedAdc
    {
      get => openClosedAdc ??= new();
      set => openClosedAdc = value;
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
    /// A value of StartNaCommon.
    /// </summary>
    [JsonPropertyName("startNaCommon")]
    public Common StartNaCommon
    {
      get => startNaCommon ??= new();
      set => startNaCommon = value;
    }

    /// <summary>
    /// A value of AdcPgmClosed.
    /// </summary>
    [JsonPropertyName("adcPgmClosed")]
    public Common AdcPgmClosed
    {
      get => adcPgmClosed ??= new();
      set => adcPgmClosed = value;
    }

    /// <summary>
    /// A value of NonAdcPgmClosed.
    /// </summary>
    [JsonPropertyName("nonAdcPgmClosed")]
    public Common NonAdcPgmClosed
    {
      get => nonAdcPgmClosed ??= new();
      set => nonAdcPgmClosed = value;
    }

    /// <summary>
    /// A value of ProgramExists.
    /// </summary>
    [JsonPropertyName("programExists")]
    public Common ProgramExists
    {
      get => programExists ??= new();
      set => programExists = value;
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
    /// A value of StartProgram.
    /// </summary>
    [JsonPropertyName("startProgram")]
    public Program StartProgram
    {
      get => startProgram ??= new();
      set => startProgram = value;
    }

    /// <summary>
    /// A value of StartPersonProgram.
    /// </summary>
    [JsonPropertyName("startPersonProgram")]
    public PersonProgram StartPersonProgram
    {
      get => startPersonProgram ??= new();
      set => startPersonProgram = value;
    }

    /// <summary>
    /// A value of StartNaPersonProgram.
    /// </summary>
    [JsonPropertyName("startNaPersonProgram")]
    public PersonProgram StartNaPersonProgram
    {
      get => startNaPersonProgram ??= new();
      set => startNaPersonProgram = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public PersonProgram Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    private InterfacePersonProgram interfacePersonProgram;
    private Common aeCaseDenied;
    private Common bypassAdcOpenAlert;
    private Common paProgramOpen;
    private Common aeProgramOpen;
    private CsePersonAccount csePersonAccount;
    private Common recomputeDistribution;
    private CsePerson sendToCsenetCsePerson;
    private Program sendToCsenetProgram;
    private BatchConvertNumToText convertBatchConvertNumToText;
    private DateWorkArea convertDateWorkArea;
    private OutgoingDocument outgoingDocument;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private SpDocKey spDocKey;
    private Common caseClosureStartNa;
    private Common sendPaRolloverLetter;
    private Common sendAdcRolloverLetter;
    private Common history;
    private Common openClosedAdc;
    private CsePerson csePerson;
    private Common startNaCommon;
    private Common adcPgmClosed;
    private Common nonAdcPgmClosed;
    private Common programExists;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Program program;
    private PersonProgram personProgram;
    private Program startProgram;
    private PersonProgram startPersonProgram;
    private PersonProgram startNaPersonProgram;
    private Program existingProgram;
    private PersonProgram existingPersonProgram;
    private DateWorkArea blank;
    private DateWorkArea max;
    private Document document;
    private ProgramProcessingInfo programProcessingInfo;
    private PersonProgram tmp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of Scan.
    /// </summary>
    [JsonPropertyName("scan")]
    public CsePerson Scan
    {
      get => scan ??= new();
      set => scan = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson ar;
    private CsePersonAccount csePersonAccount;
    private InterfacePersonProgram preceeding;
    private CaseRole child;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson scan;
    private PersonProgram personProgram;
    private Program program;
    private CsePerson csePerson;
    private InterfacePersonProgram interfacePersonProgram;
  }
#endregion
}
