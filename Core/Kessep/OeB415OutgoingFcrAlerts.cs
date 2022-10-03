// Program: OE_B415_OUTGOING_FCR_ALERTS, ID: 371416559, model: 746.
// Short name: SWEE415B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B415_OUTGOING_FCR_ALERTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB415OutgoingFcrAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B415_OUTGOING_FCR_ALERTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB415OutgoingFcrAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB415OutgoingFcrAlerts.
  /// </summary>
  public OeB415OutgoingFcrAlerts(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************************************************************************
    // When a CSE person SSN is changed and the person is known to Federal Case 
    // Registry(FCR)
    // then an History record will be generated, NO worker alerts will be 
    // generated.
    // This batch process will be reading the input dataset created out of job 
    // SRRUN070 and
    // program SRC07015 which determines whether the persons Primary/Alternate 
    // SSN are
    // changed by the worker, if so, the program creates a record in the output 
    // dataset and
    // the same will be used by this batch process to generate the history 
    // records and
    // these history records can be viewed on HIST screen by the worker.
    // **************************************************************************************
    // ***************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // ***************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------
    // 
    // -----------------------------------------*
    // * 02/02/2009  Raj S              CQ114       ***** Initial Coding *****
    // *
    // *
    // 
    // *
    // * 07/16/2009  DDupree            CQ7189      Modified to check SSN 
    // against the invalid*
    // *
    // 
    // SSN list, if found this will not be      *
    // *
    // 
    // transmitted to FCR.                      *
    // *
    // 
    // *
    // * 07/16/2009  Raj S              CQ11688     Modified to fix the court-
    // order indicator*
    // *
    // 
    // problem, current our FCR process is not  *
    // *
    // 
    // sending the court order changes. This fix*
    // *
    // 
    // will start sending all court order       *
    // *
    // 
    // changes to FCR.                          *
    // ***************************************************************************************
    // ***************************************************************************************
    // * 06/02/2010  LSS                CQ18368     Modified to pad 10 digit cse
    // person      *
    // *
    // 
    // number with '00000' to make a 15 digit   *
    // *
    // 
    // number that can be successfully sent to  *
    // *
    // 
    // FCR.                                     *
    // *
    // 
    // Removed RESTART logic.                   *
    // ***************************************************************************************
    // 07/16/2009   DDupree     We will first check the file that was created 
    // earlier to
    // be sent to fcr. If the  cse person number and ssn combination are on the 
    // invalid
    // ssn table then the record will not be sent, If the addtional sssn1 or 
    // ssn2 is on the
    // invalid ssn table with the cse person number int question then just that 
    // piece of
    // information will not be sent and the rest of the record will go intact. 
    // These
    // checks apply when either there is a add or change for a case or person 
    // reocrd.
    // The second change to program is cleaning up the creation of hist records.
    // If we
    // have to not sent a record or a ssn to fcr because it was a invalid 
    // combination on
    // the invalid ssn table then we will not create a hist record saying we 
    // sent it.
    // Part of CQ7189.
    // __________________________________________________________________________________
    ExitState = "ACO_NN0000_ALL_OK";

    // **************************************************************************************
    // The below mentioned housekeeping action block will get the program 
    // parameters, set
    // Required hard coded values required for this batch process.
    // **************************************************************************************
    UseOeB415Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.Case1.Count = 0;
    local.Case1.Index = -1;

    do
    {
      local.Eab.FileInstruction = "READ";

      // **************************************************************************************
      // The below mentioned External Action Block (EAB) will read the fcr 
      // master file
      // which will then check the person number and ssn are not an invalid
      // combination. If it is an invalid cobination then it will not be sent to
      // fcr.
      // **************************************************************************************
      UseSiEabReadOutgoingFcrFile1();

      if (Equal(local.Eab.TextReturnCode, "EF"))
      {
        break;
      }

      if (!Equal(local.Eab.TextReturnCode, "00"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        return;
      }

      if (Equal(local.Header.RecordIdentifier, "FA"))
      {
        local.Eab.FileInstruction = "WRITE";
        UseSiEabWriteOutgoingFcrFile1();

        if (!Equal(local.Eab.TextReturnCode, "00"))
        {
          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
        }

        ++local.RecordCount.Count;
        local.Header.Assign(local.ClearHeader);
        local.WriteCaseRecord.Assign(local.ClearCaseRecord);
        local.CaseRecord.Assign(local.ClearCaseRecord);
        local.WritePersonRecord.Assign(local.ClearPersonRecord);
        local.PersonRecord.Assign(local.ClearPersonRecord);
        local.QueryRecord.Assign(local.ClearQueryRecord);
        MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
        MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);

        continue;
      }
      else if (Equal(local.CaseRecord.RecordIdentifier, "FC"))
      {
        if (!Equal(local.CurrentCase.CaseId, local.CaseRecord.CaseId))
        {
          if (!local.Case1.IsEmpty)
          {
            if (AsChar(local.AdultPassed.Flag) == 'Y' && AsChar
              (local.ChildPassed.Flag) == 'Y' || AsChar
              (local.ChangeCaseRecordFound.Flag) == 'Y')
            {
              local.Case1.Index = 0;

              for(var limit = local.Case1.Count; local.Case1.Index < limit; ++
                local.Case1.Index)
              {
                if (!local.Case1.CheckSize())
                {
                  break;
                }

                if (AsChar(local.Case1.Item.SendRecord.Flag) == 'Y')
                {
                  if (!IsEmpty(local.Case1.Item.CaseRecord.RecordIdentifier))
                  {
                    local.WriteCaseRecord.Assign(local.Case1.Item.CaseRecord);
                    local.WritePersonRecord.Assign(local.ClearPersonRecord);
                  }
                  else
                  {
                    local.WriteCaseRecord.Assign(local.ClearCaseRecord);
                    local.WritePersonRecord.
                      Assign(local.Case1.Item.PersonRecord);
                  }

                  local.Eab.FileInstruction = "WRITE";
                  UseSiEabWriteOutgoingFcrFile1();

                  if (!Equal(local.Eab.TextReturnCode, "00"))
                  {
                    ExitState = "FILE_READ_ERROR_WITH_RB";

                    return;
                  }

                  ++local.RecordCount.Count;
                }
              }

              local.Case1.CheckIndex();
            }

            for(local.Case1.Index = 0; local.Case1.Index < local.Case1.Count; ++
              local.Case1.Index)
            {
              if (!local.Case1.CheckSize())
              {
                break;
              }

              local.Case1.Update.SendRecord.Flag = "";
              local.Case1.Update.CaseRecord.Assign(local.ClearCaseRecord);
              local.Case1.Update.PersonRecord.Assign(local.ClearPersonRecord);
            }

            local.Case1.CheckIndex();
            local.Case1.Count = 0;
            local.Case1.Index = -1;
            local.AdultPassed.Flag = "";
            local.ChildPassed.Flag = "";
            local.ChangeCaseRecordFound.Flag = "";
          }
        }

        local.KpcCheck.Text3 = Substring(local.CaseRecord.CaseId, 1, 3);

        if (Equal(local.KpcCheck.Text3, "KPC"))
        {
          local.WriteCaseRecord.Assign(local.CaseRecord);
          local.Eab.FileInstruction = "WRITE";
          UseSiEabWriteOutgoingFcrFile1();

          if (!Equal(local.Eab.TextReturnCode, "00"))
          {
            ExitState = "FILE_READ_ERROR_WITH_RB";

            break;
          }

          ++local.RecordCount.Count;
          local.Header.Assign(local.ClearHeader);
          local.WriteCaseRecord.Assign(local.ClearCaseRecord);
          local.CaseRecord.Assign(local.ClearCaseRecord);
          local.WritePersonRecord.Assign(local.ClearPersonRecord);
          local.PersonRecord.Assign(local.ClearPersonRecord);
          local.QueryRecord.Assign(local.ClearQueryRecord);
          MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
          MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);

          continue;
        }
        else if (AsChar(local.CaseRecord.ActionTypeCode) == 'D')
        {
          // when a case is being deleted only the case record is sent since it 
          // is assumes that everyone tied to the case would be deleted also
          local.WriteCaseRecord.Assign(local.CaseRecord);
          local.WritePersonRecord.Assign(local.ClearPersonRecord);
          local.Eab.FileInstruction = "WRITE";
          UseSiEabWriteOutgoingFcrFile1();

          if (!Equal(local.Eab.TextReturnCode, "00"))
          {
            ExitState = "FILE_READ_ERROR_WITH_RB";

            return;
          }

          ++local.RecordCount.Count;
          local.Header.Assign(local.ClearHeader);
          local.WriteCaseRecord.Assign(local.ClearCaseRecord);
          local.CaseRecord.Assign(local.ClearCaseRecord);
          local.WritePersonRecord.Assign(local.ClearPersonRecord);
          local.ClearPersonRecord.Assign(local.ClearPersonRecord);
          local.QueryRecord.Assign(local.ClearQueryRecord);
          MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
          MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);
        }
        else
        {
          // **************************************************************************************
          // The below mentioned new code added to set the Local Change Case 
          // record found in order
          // to write the change case record to output file.  This change 
          // records from previous
          // FCR Process SRC07015.    ----- CQ11688 -----
          // **************************************************************************************
          if (AsChar(local.CaseRecord.ActionTypeCode) == 'C')
          {
            local.ChangeCaseRecordFound.Flag = "Y";
          }

          local.CurrentCase.CaseId = local.CaseRecord.CaseId;

          ++local.Case1.Index;
          local.Case1.CheckSize();

          local.Case1.Update.CaseRecord.Assign(local.CaseRecord);
          local.Case1.Update.SendRecord.Flag = "Y";
        }
      }
      else if (Equal(local.QueryRecord.RecordIdentifier, "FQ"))
      {
        local.Eab.FileInstruction = "WRITE";
        UseSiEabWriteOutgoingFcrFile1();

        if (!Equal(local.Eab.TextReturnCode, "00"))
        {
          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
        }

        ++local.RecordCount.Count;
        local.Header.Assign(local.ClearHeader);
        local.WriteCaseRecord.Assign(local.ClearCaseRecord);
        local.CaseRecord.Assign(local.ClearCaseRecord);
        local.WritePersonRecord.Assign(local.ClearPersonRecord);
        local.PersonRecord.Assign(local.ClearPersonRecord);
        local.QueryRecord.Assign(local.ClearQueryRecord);
        MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
        MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);

        continue;
      }
      else if (Equal(local.PersonRecord.RecordIdentifier, "FP"))
      {
        if (!Equal(local.CurrentCase.CaseId, local.PersonRecord.CaseId))
        {
          if (!local.Case1.IsEmpty)
          {
            if (AsChar(local.AdultPassed.Flag) == 'Y' && AsChar
              (local.ChildPassed.Flag) == 'Y' || AsChar
              (local.ChangeCaseRecordFound.Flag) == 'Y')
            {
              local.Case1.Index = 0;

              for(var limit = local.Case1.Count; local.Case1.Index < limit; ++
                local.Case1.Index)
              {
                if (!local.Case1.CheckSize())
                {
                  break;
                }

                if (AsChar(local.Case1.Item.SendRecord.Flag) == 'Y')
                {
                  if (!IsEmpty(local.Case1.Item.CaseRecord.RecordIdentifier))
                  {
                    local.WriteCaseRecord.Assign(local.Case1.Item.CaseRecord);
                    local.WritePersonRecord.Assign(local.ClearPersonRecord);
                  }
                  else
                  {
                    local.WriteCaseRecord.Assign(local.ClearCaseRecord);
                    local.WritePersonRecord.
                      Assign(local.Case1.Item.PersonRecord);
                  }

                  local.Eab.FileInstruction = "WRITE";
                  UseSiEabWriteOutgoingFcrFile1();

                  if (!Equal(local.Eab.TextReturnCode, "00"))
                  {
                    ExitState = "FILE_READ_ERROR_WITH_RB";

                    return;
                  }

                  ++local.RecordCount.Count;
                }
              }

              local.Case1.CheckIndex();
            }

            for(local.Case1.Index = 0; local.Case1.Index < local.Case1.Count; ++
              local.Case1.Index)
            {
              if (!local.Case1.CheckSize())
              {
                break;
              }

              local.Case1.Update.SendRecord.Flag = "";
              local.Case1.Update.CaseRecord.Assign(local.ClearCaseRecord);
              local.Case1.Update.PersonRecord.Assign(local.ClearPersonRecord);
            }

            local.Case1.CheckIndex();
            local.Case1.Count = 0;
            local.Case1.Index = -1;
            local.AdultPassed.Flag = "";
            local.ChildPassed.Flag = "";
            local.ChangeCaseRecordFound.Flag = "";
          }
        }

        local.KpcCheck.Text3 = Substring(local.PersonRecord.CaseId, 1, 3);

        if (Equal(local.KpcCheck.Text3, "KPC"))
        {
          local.WriteCaseRecord.Assign(local.ClearCaseRecord);
          local.WritePersonRecord.Assign(local.PersonRecord);
          local.Eab.FileInstruction = "WRITE";
          UseSiEabWriteOutgoingFcrFile1();

          if (!Equal(local.Eab.TextReturnCode, "00"))
          {
            ExitState = "FILE_READ_ERROR_WITH_RB";

            return;
          }

          ++local.RecordCount.Count;
          local.Header.Assign(local.ClearHeader);
          local.WriteCaseRecord.Assign(local.ClearCaseRecord);
          local.CaseRecord.Assign(local.ClearCaseRecord);
          local.WritePersonRecord.Assign(local.ClearPersonRecord);
          local.PersonRecord.Assign(local.ClearPersonRecord);
          local.QueryRecord.Assign(local.ClearQueryRecord);
          MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
          MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);

          continue;
        }
        else if (!Equal(local.CurrentCase.CaseId, local.PersonRecord.CaseId))
        {
          // for indivial records
          if (AsChar(local.PersonRecord.ActionTypeCode) == 'A' || AsChar
            (local.PersonRecord.ActionTypeCode) == 'C')
          {
            local.MemberId.Text10 =
              Substring(local.PersonRecord.MemberId, 6, 10);

            if (ReadCsePerson2())
            {
              local.Read.SsnNum9 = (int)StringToNumber(local.PersonRecord.Ssn);

              if (ReadInvalidSsn1())
              {
                // since this is a invalid ssn and person number combination 
                // then we can not send it to fcr
                local.Header.Assign(local.ClearHeader);
                local.WriteCaseRecord.Assign(local.ClearCaseRecord);
                local.CaseRecord.Assign(local.ClearCaseRecord);
                local.WritePersonRecord.Assign(local.ClearPersonRecord);
                local.ClearPersonRecord.Assign(local.ClearPersonRecord);
                local.QueryRecord.Assign(local.ClearQueryRecord);
                MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
                  
                MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);

                continue;
              }
              else
              {
                // since the primary ssn and person number combination was not 
                // on the invalid ssn
                //  table we can send the record but we do have to check the 
                // rest of the ssn and
                // not send any invalid combination to FCR
                local.WritePersonRecord.Assign(local.ClearPersonRecord);
                local.WriteCaseRecord.Assign(local.ClearCaseRecord);
                local.WritePersonRecord.Assign(local.PersonRecord);

                if (!IsEmpty(local.PersonRecord.PreviousSsn))
                {
                  local.Read.SsnNum9 =
                    (int)StringToNumber(local.PersonRecord.PreviousSsn);

                  if (ReadInvalidSsn1())
                  {
                    // since this is a invalid ssn and person number combination
                    // then we do not want to send it to fcr
                    local.WritePersonRecord.PreviousSsn = "";
                  }
                  else
                  {
                    // this is fine so send the ssn to fcr for this person
                  }
                }

                if (!IsEmpty(local.PersonRecord.AdditionalSsn1))
                {
                  local.Read.SsnNum9 =
                    (int)StringToNumber(local.PersonRecord.AdditionalSsn1);

                  if (ReadInvalidSsn1())
                  {
                    // since this is a invalid ssn and person number combination
                    // then we do not want to send it to fcr
                    local.WritePersonRecord.AdditionalSsn1 = "";
                  }
                  else
                  {
                    // this is fine so send the ssn to fcr for this person
                  }
                }

                if (!IsEmpty(local.PersonRecord.AdditionalSsn2))
                {
                  local.Read.SsnNum9 =
                    (int)StringToNumber(local.PersonRecord.AdditionalSsn2);

                  if (ReadInvalidSsn1())
                  {
                    // since this is a invalid ssn and person number combination
                    // then we do not want to send it to fcr
                    local.WritePersonRecord.AdditionalSsn2 = "";
                  }
                  else
                  {
                    // this is fine so send the ssn to fcr for this person
                  }
                }

                if (IsEmpty(local.WritePersonRecord.AdditionalSsn1) && !
                  IsEmpty(local.WritePersonRecord.AdditionalSsn2))
                {
                  local.WritePersonRecord.AdditionalSsn1 =
                    local.WritePersonRecord.AdditionalSsn2;
                }

                local.Eab.FileInstruction = "WRITE";
                UseSiEabWriteOutgoingFcrFile1();

                if (!Equal(local.Eab.TextReturnCode, "00"))
                {
                  ExitState = "FILE_READ_ERROR_WITH_RB";

                  return;
                }

                ++local.RecordCount.Count;
                local.Header.Assign(local.ClearHeader);
                local.WriteCaseRecord.Assign(local.ClearCaseRecord);
                local.CaseRecord.Assign(local.ClearCaseRecord);
                local.WritePersonRecord.Assign(local.ClearPersonRecord);
                local.PersonRecord.Assign(local.ClearPersonRecord);
                local.QueryRecord.Assign(local.ClearQueryRecord);
                MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
                  
                MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);
              }
            }
            else
            {
              local.ErrorCsePerson.Number = local.MemberId.Text10;
              local.PersonRecord.Assign(local.ClearPersonRecord);
              ExitState = "CSE_PERSON_NF";
            }
          }
          else
          {
            // all these records will be sent
            local.WritePersonRecord.Assign(local.PersonRecord);

            if (AsChar(local.PersonRecord.ActionTypeCode) == 'D')
            {
              local.MemberId.Text10 =
                Substring(local.PersonRecord.MemberId, 6, 10);

              if (ReadCsePerson2())
              {
                local.Read.SsnNum9 =
                  (int)StringToNumber(local.PersonRecord.Ssn);

                if (ReadInvalidSsn1())
                {
                  try
                  {
                    UpdateInvalidSsn();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "INVALID_SSN_NU";
                        local.ErrorCsePerson.Number = entities.Existing.Number;
                        local.WritePersonRecord.Assign(local.ClearPersonRecord);
                        local.PersonRecord.Assign(local.ClearPersonRecord);

                        goto Test;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "INVALID_SSN_PV";
                        local.ErrorCsePerson.Number = entities.Existing.Number;
                        local.WritePersonRecord.Assign(local.ClearPersonRecord);
                        local.PersonRecord.Assign(local.ClearPersonRecord);

                        goto Test;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }

                  // since this is a invalid ssn and person number combination 
                  // then we need to send this delete to fcr
                }
                else
                {
                  // since the primary ssn and person number combination was not
                  // on the invalid ssn
                  //  table we can send the record but we do have to check the 
                  // rest of the ssn and
                  // not send any invalid combination to FCR
                }
              }
              else
              {
                local.ErrorCsePerson.Number = local.MemberId.Text10;
                ExitState = "CSE_PERSON_NF";
                local.WritePersonRecord.Assign(local.ClearPersonRecord);
                local.PersonRecord.Assign(local.ClearPersonRecord);

                goto Test;
              }
            }
            else if (ReadCsePerson2())
            {
              local.Read.SsnNum9 = (int)StringToNumber(local.PersonRecord.Ssn);

              if (ReadInvalidSsn1())
              {
                // since this is a invalid ssn and person number combination 
                // then we can not send it to fcr
                local.Header.Assign(local.ClearHeader);
                local.WriteCaseRecord.Assign(local.ClearCaseRecord);
                local.CaseRecord.Assign(local.ClearCaseRecord);
                local.WritePersonRecord.Assign(local.ClearPersonRecord);
                local.PersonRecord.Assign(local.ClearPersonRecord);
                local.QueryRecord.Assign(local.ClearQueryRecord);
                MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
                  
                MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);

                continue;
              }
              else
              {
                // since the primary ssn and person number combination was not 
                // on the invalid ssn
                //  table we can send the record but we do have to check the 
                // rest of the ssn and
                // not send any invalid combination to FCR
              }

              if (!IsEmpty(local.PersonRecord.PreviousSsn))
              {
                local.Read.SsnNum9 =
                  (int)StringToNumber(local.PersonRecord.PreviousSsn);

                if (ReadInvalidSsn1())
                {
                  // since this is a invalid ssn and person number combination 
                  // then we do not want to send it to fcr
                  local.WritePersonRecord.PreviousSsn = "";
                }
                else
                {
                  // this is fine so send the ssn to fcr for this person
                }
              }

              if (!IsEmpty(local.PersonRecord.AdditionalSsn1))
              {
                local.Read.SsnNum9 =
                  (int)StringToNumber(local.PersonRecord.AdditionalSsn1);

                if (ReadInvalidSsn1())
                {
                  // since this is a invalid ssn and person number combination 
                  // then we do not want to send it to fcr
                  local.WritePersonRecord.AdditionalSsn1 = "";
                }
                else
                {
                  // this is fine so send the ssn to fcr for this person
                }
              }

              if (!IsEmpty(local.PersonRecord.AdditionalSsn2))
              {
                local.Read.SsnNum9 =
                  (int)StringToNumber(local.PersonRecord.AdditionalSsn2);

                if (ReadInvalidSsn1())
                {
                  // since this is a invalid ssn and person number combination 
                  // then we do not want to send it to fcr
                  local.WritePersonRecord.AdditionalSsn2 = "";
                }
                else
                {
                  // this is fine so send the ssn to fcr for this person
                }
              }

              if (IsEmpty(local.WritePersonRecord.AdditionalSsn1) && !
                IsEmpty(local.WritePersonRecord.AdditionalSsn2))
              {
                local.WritePersonRecord.AdditionalSsn1 =
                  local.WritePersonRecord.AdditionalSsn2;
              }
            }
            else
            {
              local.ErrorCsePerson.Number = local.MemberId.Text10;
              ExitState = "CSE_PERSON_NF";
              local.WritePersonRecord.Assign(local.ClearPersonRecord);
              local.PersonRecord.Assign(local.ClearPersonRecord);

              goto Test;
            }

            local.Eab.FileInstruction = "WRITE";
            UseSiEabWriteOutgoingFcrFile1();

            if (!Equal(local.Eab.TextReturnCode, "00"))
            {
              ExitState = "FILE_READ_ERROR_WITH_RB";

              return;
            }

            ++local.RecordCount.Count;
            local.Header.Assign(local.ClearHeader);
            local.WriteCaseRecord.Assign(local.ClearCaseRecord);
            local.CaseRecord.Assign(local.ClearCaseRecord);
            local.WritePersonRecord.Assign(local.ClearPersonRecord);
            local.PersonRecord.Assign(local.ClearPersonRecord);
            local.QueryRecord.Assign(local.ClearQueryRecord);
            MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
            MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);
          }
        }
        else
        {
          // for all the records tied to a case
          if (AsChar(local.PersonRecord.ActionTypeCode) == 'A' || AsChar
            (local.PersonRecord.ActionTypeCode) == 'C')
          {
            local.MemberId.Text10 =
              Substring(local.PersonRecord.MemberId, 6, 10);

            if (ReadCsePerson2())
            {
              ++local.Case1.Index;
              local.Case1.CheckSize();

              local.Case1.Update.SendRecord.Flag = "";
              local.Case1.Update.CaseRecord.Assign(local.CaseRecord);
              local.Case1.Update.PersonRecord.Assign(local.PersonRecord);
              local.Read.SsnNum9 = (int)StringToNumber(local.PersonRecord.Ssn);

              if (ReadInvalidSsn1())
              {
                // since this is a invalid ssn and person number combination 
                // then we can not send it to fcr
                local.Header.Assign(local.ClearHeader);
                local.WriteCaseRecord.Assign(local.ClearCaseRecord);
                local.CaseRecord.Assign(local.ClearCaseRecord);
                local.WritePersonRecord.Assign(local.ClearPersonRecord);
                local.ClearPersonRecord.Assign(local.ClearPersonRecord);
                local.QueryRecord.Assign(local.ClearQueryRecord);
                MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
                  
                MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);

                continue;
              }
              else
              {
                // since the primary ssn and person number combination was not 
                // on the invalid ssn
                //  table we can send the record but we do have to check the 
                // rest of the ssn and
                // not send any invalid combination to FCR
                local.Case1.Update.SendRecord.Flag = "Y";

                if (Equal(local.PersonRecord.ParticipantType, "CP") || Equal
                  (local.PersonRecord.ParticipantType, "NP") || Equal
                  (local.PersonRecord.ParticipantType, "PF"))
                {
                  local.AdultPassed.Flag = "Y";
                }

                if (Equal(local.PersonRecord.ParticipantType, "CH"))
                {
                  local.ChildPassed.Flag = "Y";
                }

                if (!IsEmpty(local.PersonRecord.PreviousSsn))
                {
                  local.Read.SsnNum9 =
                    (int)StringToNumber(local.PersonRecord.PreviousSsn);

                  if (ReadInvalidSsn1())
                  {
                    // since this is a invalid ssn and person number combination
                    // then we do not want to send it to fcr
                    local.PersonRecord.PreviousSsn = "";
                  }
                  else
                  {
                    // this is fine so send the ssn to fcr for this person
                  }
                }

                if (!IsEmpty(local.PersonRecord.AdditionalSsn1))
                {
                  local.Read.SsnNum9 =
                    (int)StringToNumber(local.PersonRecord.AdditionalSsn1);

                  if (ReadInvalidSsn1())
                  {
                    // since this is a invalid ssn and person number combination
                    // then we do not want to send it to fcr
                    local.PersonRecord.AdditionalSsn1 = "";
                  }
                  else
                  {
                    // this is fine so send the ssn to fcr for this person
                  }
                }

                if (!IsEmpty(local.PersonRecord.AdditionalSsn2))
                {
                  local.Read.SsnNum9 =
                    (int)StringToNumber(local.PersonRecord.AdditionalSsn2);

                  if (ReadInvalidSsn1())
                  {
                    // since this is a invalid ssn and person number combination
                    // then we do not want to send it to fcr
                    local.PersonRecord.AdditionalMidleName2 = "";
                  }
                  else
                  {
                    // this is fine so send the ssn to fcr for this person
                  }
                }

                if (IsEmpty(local.PersonRecord.AdditionalSsn1) && !
                  IsEmpty(local.PersonRecord.AdditionalSsn2))
                {
                  local.PersonRecord.AdditionalSsn1 =
                    local.PersonRecord.AdditionalSsn2;
                }

                local.Case1.Update.PersonRecord.Assign(local.PersonRecord);
              }
            }
            else
            {
              local.ErrorCsePerson.Number = local.MemberId.Text10;
              local.PersonRecord.Assign(local.ClearPersonRecord);
              ExitState = "CSE_PERSON_NF";
            }
          }
          else
          {
            local.WritePersonRecord.Assign(local.PersonRecord);

            if (AsChar(local.PersonRecord.ActionTypeCode) == 'D')
            {
              local.MemberId.Text10 =
                Substring(local.PersonRecord.MemberId, 6, 10);

              if (ReadCsePerson2())
              {
                local.Read.SsnNum9 =
                  (int)StringToNumber(local.PersonRecord.Ssn);

                if (ReadInvalidSsn1())
                {
                  try
                  {
                    UpdateInvalidSsn();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "INVALID_SSN_NU";
                        local.ErrorCsePerson.Number = entities.Existing.Number;
                        local.WritePersonRecord.Assign(local.ClearPersonRecord);
                        local.PersonRecord.Assign(local.ClearPersonRecord);

                        goto Test;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "INVALID_SSN_PV";
                        local.ErrorCsePerson.Number = entities.Existing.Number;
                        local.WritePersonRecord.Assign(local.ClearPersonRecord);
                        local.PersonRecord.Assign(local.ClearPersonRecord);

                        goto Test;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }

                  // since this is a invalid ssn and person number combination 
                  // then we need to send this delete to fcr
                }
                else
                {
                  // all deletes need go to fcr
                }

                if (Equal(local.WritePersonRecord.ParticipantType, "CP") || Equal
                  (local.WritePersonRecord.ParticipantType, "NP") || Equal
                  (local.WritePersonRecord.ParticipantType, "PF"))
                {
                  local.AdultPassed.Flag = "Y";
                }

                if (Equal(local.WritePersonRecord.ParticipantType, "CH"))
                {
                  local.ChildPassed.Flag = "Y";
                }
              }
              else
              {
                local.ErrorCsePerson.Number = local.MemberId.Text10;
                ExitState = "CSE_PERSON_NF";
                local.WritePersonRecord.Assign(local.ClearPersonRecord);
                local.PersonRecord.Assign(local.ClearPersonRecord);

                goto Test;
              }
            }
            else
            {
              local.MemberId.Text10 =
                Substring(local.PersonRecord.MemberId, 6, 10);

              if (ReadCsePerson2())
              {
                local.Read.SsnNum9 =
                  (int)StringToNumber(local.PersonRecord.Ssn);

                if (ReadInvalidSsn1())
                {
                  // since this is a invalid ssn and person number combination 
                  // then we can not send it to fcr
                  local.Header.Assign(local.ClearHeader);
                  local.WriteCaseRecord.Assign(local.ClearCaseRecord);
                  local.CaseRecord.Assign(local.ClearCaseRecord);
                  local.WritePersonRecord.Assign(local.ClearPersonRecord);
                  local.PersonRecord.Assign(local.ClearPersonRecord);
                  local.QueryRecord.Assign(local.ClearQueryRecord);
                  MoveFcrRecord(local.ClearTrailerRecord,
                    local.WriteTrailerRecord);
                  MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);

                  continue;
                }
                else
                {
                  // since the primary ssn and person number combination was not
                  // on the invalid ssn
                  //  table we can send the record but we do have to check the 
                  // rest of the ssn and
                  // not send any invalid combination to FCR
                }

                if (!IsEmpty(local.PersonRecord.PreviousSsn))
                {
                  local.Read.SsnNum9 =
                    (int)StringToNumber(local.PersonRecord.PreviousSsn);

                  if (ReadInvalidSsn1())
                  {
                    // since this is a invalid ssn and person number combination
                    // then we do not want to send it to fcr
                    local.WritePersonRecord.PreviousSsn = "";
                  }
                  else
                  {
                    // this is fine so send the ssn to fcr for this person
                  }
                }

                if (!IsEmpty(local.PersonRecord.AdditionalSsn1))
                {
                  local.Read.SsnNum9 =
                    (int)StringToNumber(local.PersonRecord.AdditionalSsn1);

                  if (ReadInvalidSsn1())
                  {
                    // since this is a invalid ssn and person number combination
                    // then we do not want to send it to fcr
                    local.WritePersonRecord.AdditionalSsn1 = "";
                  }
                  else
                  {
                    // this is fine so send the ssn to fcr for this person
                  }
                }

                if (!IsEmpty(local.PersonRecord.AdditionalSsn2))
                {
                  local.Read.SsnNum9 =
                    (int)StringToNumber(local.PersonRecord.AdditionalSsn2);

                  if (ReadInvalidSsn1())
                  {
                    // since this is a invalid ssn and person number combination
                    // then we do not want to send it to fcr
                    local.WritePersonRecord.AdditionalSsn2 = "";
                  }
                  else
                  {
                    // this is fine so send the ssn to fcr for this person
                  }
                }

                if (IsEmpty(local.WritePersonRecord.AdditionalSsn1) && !
                  IsEmpty(local.WritePersonRecord.AdditionalSsn2))
                {
                  local.WritePersonRecord.AdditionalSsn1 =
                    local.WritePersonRecord.AdditionalSsn2;
                }
              }
              else
              {
                local.ErrorCsePerson.Number = local.MemberId.Text10;
                ExitState = "CSE_PERSON_NF";
                local.WritePersonRecord.Assign(local.ClearPersonRecord);
                local.PersonRecord.Assign(local.ClearPersonRecord);

                goto Test;
              }
            }

            local.Eab.FileInstruction = "WRITE";
            UseSiEabWriteOutgoingFcrFile1();

            if (!Equal(local.Eab.TextReturnCode, "00"))
            {
              ExitState = "FILE_READ_ERROR_WITH_RB";

              return;
            }

            ++local.RecordCount.Count;
            local.Header.Assign(local.ClearHeader);
            local.WriteCaseRecord.Assign(local.ClearCaseRecord);
            local.CaseRecord.Assign(local.ClearCaseRecord);
            local.WritePersonRecord.Assign(local.ClearPersonRecord);
            local.PersonRecord.Assign(local.ClearPersonRecord);
            local.QueryRecord.Assign(local.ClearQueryRecord);
            MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
            MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);
          }
        }
      }
      else if (Equal(local.TrailerRecord.RecordIdentifier, "FZ"))
      {
        // process any records that have been checked but not sent
        if (!local.Case1.IsEmpty)
        {
          if (AsChar(local.AdultPassed.Flag) == 'Y' && AsChar
            (local.ChildPassed.Flag) == 'Y')
          {
            local.Case1.Index = 0;

            for(var limit = local.Case1.Count; local.Case1.Index < limit; ++
              local.Case1.Index)
            {
              if (!local.Case1.CheckSize())
              {
                break;
              }

              if (AsChar(local.Case1.Item.SendRecord.Flag) == 'Y')
              {
                if (!IsEmpty(local.Case1.Item.CaseRecord.RecordIdentifier))
                {
                  local.WriteCaseRecord.Assign(local.Case1.Item.CaseRecord);
                  local.WritePersonRecord.Assign(local.ClearPersonRecord);
                }
                else
                {
                  local.WriteCaseRecord.Assign(local.ClearCaseRecord);
                  local.WritePersonRecord.Assign(local.Case1.Item.PersonRecord);
                }

                local.Eab.FileInstruction = "WRITE";
                UseSiEabWriteOutgoingFcrFile1();

                if (!Equal(local.Eab.TextReturnCode, "00"))
                {
                  ExitState = "FILE_READ_ERROR_WITH_RB";

                  return;
                }

                ++local.RecordCount.Count;
              }
            }

            local.Case1.CheckIndex();
          }

          for(local.Case1.Index = 0; local.Case1.Index < local.Case1.Count; ++
            local.Case1.Index)
          {
            if (!local.Case1.CheckSize())
            {
              break;
            }

            local.Case1.Update.SendRecord.Flag = "";
            local.Case1.Update.CaseRecord.Assign(local.ClearCaseRecord);
            local.Case1.Update.PersonRecord.Assign(local.ClearPersonRecord);
          }

          local.Case1.CheckIndex();
          local.Case1.Count = 0;
          local.Case1.Index = -1;
          local.AdultPassed.Flag = "";
          local.ChildPassed.Flag = "";
        }

        // look for any invalid ssn records that have not been sent to fcr and 
        // if one is found send a delete for it.
        foreach(var item in ReadInvalidSsn2())
        {
          if (ReadCsePerson3())
          {
            local.ConvertSsn.Text15 =
              NumberToString(entities.InvalidSsn.Ssn, 15);
            local.PersonRecord.Ssn = Substring(local.ConvertSsn.Text15, 7, 9);
            local.Previous.Number = "";

            foreach(var item1 in ReadCaseCaseRole())
            {
              if (!Equal(entities.Case2.Number, local.Previous.Number))
              {
                local.WritePersonRecord.Assign(local.ClearPersonRecord);
                local.WritePersonRecord.RecordIdentifier = "FP";
                local.WritePersonRecord.ActionTypeCode = "D";
                local.WritePersonRecord.CaseId = entities.Case2.Number;

                // CQ18368 LSS
                // Pad 5 zeros ("00000") before our 10 digit cse person number 
                // before sending
                // it to FCR which requires 15 digits.
                local.WritePersonRecord.MemberId = "00000" + entities
                  .Existing.Number;
                local.WritePersonRecord.Ssn = local.PersonRecord.Ssn;
                local.WriteTrailerRecord.RecordCount =
                  NumberToString(local.RecordCount.Count, 8);
                local.Eab.FileInstruction = "WRITE";
                UseSiEabWriteOutgoingFcrFile1();

                if (!Equal(local.Eab.TextReturnCode, "00"))
                {
                  ExitState = "FILE_READ_ERROR_WITH_RB";

                  return;
                }

                ++local.RecordCount.Count;
                local.Previous.Number = entities.Case2.Number;
              }
            }

            try
            {
              UpdateInvalidSsn();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "INVALID_SSN_NU";
                  local.ErrorCsePerson.Number = entities.Existing.Number;

                  goto Test;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "INVALID_SSN_PV";
                  local.ErrorCsePerson.Number = entities.Existing.Number;

                  goto Test;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            ExitState = "CSE_PERSON_NF";
            local.ConvertSsn.Text15 =
              NumberToString(entities.InvalidSsn.Ssn, 15);
            local.ErrorFcrRecord.Ssn = Substring(local.ConvertSsn.Text15, 7, 9);

            goto Test;
          }
        }

        local.Header.Assign(local.ClearHeader);
        local.WriteCaseRecord.Assign(local.ClearCaseRecord);
        local.CaseRecord.Assign(local.ClearCaseRecord);
        local.WritePersonRecord.Assign(local.ClearPersonRecord);
        local.PersonRecord.Assign(local.ClearPersonRecord);
        local.QueryRecord.Assign(local.ClearQueryRecord);
        MoveFcrRecord(local.TrailerRecord, local.WriteTrailerRecord);
        ++local.RecordCount.Count;
        local.WriteTrailerRecord.RecordCount =
          NumberToString(local.RecordCount.Count, 8);
        local.Eab.FileInstruction = "WRITE";
        UseSiEabWriteOutgoingFcrFile1();

        if (!Equal(local.Eab.TextReturnCode, "00"))
        {
          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
        }

        MoveFcrRecord(local.ClearTrailerRecord, local.WriteTrailerRecord);
        MoveFcrRecord(local.ClearTrailerRecord, local.TrailerRecord);

        continue;
      }

Test:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();

        if (!IsEmpty(local.ErrorCsePerson.Number))
        {
          local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
            (local.ExitStateWorkArea.Message) + " CSE Person # " + local
            .ErrorCsePerson.Number;
        }
        else
        {
          local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
            (local.ExitStateWorkArea.Message) + " SSN # " + local
            .ErrorFcrRecord.Ssn;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.ErrorCsePerson.Number = "";
        local.ErrorFcrRecord.Ssn = "";
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }
    while(!Equal(local.Eab.TextReturnCode, "EF"));

    do
    {
      local.Eab.FileInstruction = "READ";

      // **************************************************************************************
      // The below mentioned External Action Block (EAB) will read the input 
      // records from the
      // Dataset required for processing.
      // **************************************************************************************
      UseOeEabReadOutFcrAlertRecord();

      if (Equal(local.Eab.TextReturnCode, "EF"))
      {
        break;
      }

      if (!IsEmpty(local.Eab.TextReturnCode))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        break;
      }

      ++local.RecordsRead.Count;

      // **************************************************************************************
      // Read the CSE person entity type using the person number from the input 
      // data set and
      // Get the person information from ADABAS.
      // **************************************************************************************
      if (ReadCsePerson1())
      {
        local.Obligor.Number = local.FcrAlert.PersonId;

        // **************************************************************************************
        // The below mentioned action block will return the CSE person details 
        // by calling a EAB
        // Which in turn will access ADABAS and all person information like 
        // Name, SSN etc. will
        // be return to this process.
        // **************************************************************************************
        UseSiCabReadAdabasBatch();

        if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ACO_ADABAS_PERSON_NF_113"))
        {
          ++local.RecordsPersonNotFound.Count;
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
        else if (IsExitState("ADABAS_INVALID_SSN_W_RB"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          break;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          break;
        }
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Person Not Found - AP # " + (
          local.Infrastructure.CsePersonNumber ?? "") + " " + " ";
        ++local.RecordsPersonNotFound.Count;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        continue;
      }

      // **************************************************************************************
      // Generate History records for the selected person by checking the Old 
      // and new SSNs, to
      // Generate the history record Action Block OE_B412_CREATE_INFRASTRUCTURE 
      // will be used.
      // **************************************************************************************
      local.Infrastructure.CaseNumber = Substring(local.FcrAlert.CaseId, 1, 10);
      local.Infrastructure.CsePersonNumber = local.FcrAlert.PersonId;
      local.Infrastructure.ProcessStatus = "H";
      local.SsnChangeCount.Count = 0;
      local.SsnSkipCount.Count = 0;

      do
      {
        ++local.SsnChangeCount.Count;

        switch(local.SsnChangeCount.Count)
        {
          case 1:
            if (!Equal(local.FcrAlert.CseSsn, local.FcrAlert.FcrSsn))
            {
              local.Read.SsnNum9 = (int)StringToNumber(local.FcrAlert.CseSsn);

              if (ReadInvalidSsn1())
              {
                // added this check so we will not send a invalid ssn and cse 
                // person combination
                // to fcr, we stopped it earlier no we have to make sure no hist
                // records are created.
                ++local.SkipRecordCount.Count;

                // since this is the primary we would not sent any record at 
                // all, so we will go to the next record
                goto Next;
              }
              else
              {
                // this is ok so proceed
              }

              ++local.PriSsnChangeRecsRead.Count;

              if (IsEmpty(local.FcrAlert.FcrSsn) || Equal
                (local.FcrAlert.FcrSsn, "000000000"))
              {
                local.Infrastructure.ReasonCode = "CSEBLKSSNCH";
                local.Infrastructure.Detail = "XXX-XX-XXXX to " + Substring
                  (local.FcrAlert.CseSsn, FcrAlertRecord.CseSsn_MaxLength, 1, 3) +
                  "-" + Substring
                  (local.FcrAlert.CseSsn, FcrAlertRecord.CseSsn_MaxLength, 4, 2) +
                  "-" + Substring
                  (local.FcrAlert.CseSsn, FcrAlertRecord.CseSsn_MaxLength, 6, 4) +
                  "";
              }
              else if (IsEmpty(local.FcrAlert.CseSsn) || Equal
                (local.FcrAlert.CseSsn, "000000000"))
              {
                local.Infrastructure.ReasonCode = "CSESSNBLKCH";
                local.Infrastructure.Detail =
                  Substring(local.FcrAlert.FcrSsn,
                  FcrAlertRecord.FcrSsn_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.FcrSsn, FcrAlertRecord.FcrSsn_MaxLength, 4, 2) +
                  "-" + Substring
                  (local.FcrAlert.FcrSsn, FcrAlertRecord.FcrSsn_MaxLength, 6, 4) +
                  " to XXX-XX-XXXX ";
              }
              else
              {
                local.Infrastructure.ReasonCode = "CSESSNCH";
                local.Infrastructure.Detail =
                  Substring(local.FcrAlert.FcrSsn,
                  FcrAlertRecord.FcrSsn_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.FcrSsn, FcrAlertRecord.FcrSsn_MaxLength, 4, 2) +
                  "-" + Substring
                  (local.FcrAlert.FcrSsn, FcrAlertRecord.FcrSsn_MaxLength, 6, 4) +
                  " to " + Substring
                  (local.FcrAlert.CseSsn, FcrAlertRecord.CseSsn_MaxLength, 1, 3) +
                  "-" + Substring
                  (local.FcrAlert.CseSsn, FcrAlertRecord.CseSsn_MaxLength, 4, 2) +
                  "-" + Substring
                  (local.FcrAlert.CseSsn, FcrAlertRecord.CseSsn_MaxLength, 6, 4);
                  
              }
            }
            else
            {
              ++local.SsnSkipCount.Count;

              continue;
            }

            break;
          case 2:
            if (!Equal(local.FcrAlert.CseAdditionalSsn1,
              local.FcrAlert.FcrAdditionalSsn1))
            {
              local.Read.SsnNum9 =
                (int)StringToNumber(local.FcrAlert.CseAdditionalSsn1);

              if (ReadInvalidSsn1())
              {
                // added this check so we will not send a invalid ssn and cse 
                // person combination
                // to fcr, we stopped it earlier no we have to make sure no hist
                // records are created.
                ++local.SkipRecordCount.Count;

                // if the primary we will still send the record but we will not 
                // send this ssn along with
                // it so we will not create a hist record for it
                continue;
              }
              else
              {
                // this is ok so proceed
              }

              ++local.AltSsnChangeRecsRead.Count;

              if (IsEmpty(local.FcrAlert.FcrAdditionalSsn1) || Equal
                (local.FcrAlert.FcrAdditionalSsn1, "000000000"))
              {
                local.Infrastructure.ReasonCode = "CSEALTBLKSSNCH";
                local.Infrastructure.Detail = "XXX-XX-XXXX to " + Substring
                  (local.FcrAlert.CseAdditionalSsn1,
                  FcrAlertRecord.CseAdditionalSsn1_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.CseAdditionalSsn1,
                  FcrAlertRecord.CseAdditionalSsn1_MaxLength, 4, 2) + "-" + Substring
                  (local.FcrAlert.CseAdditionalSsn1,
                  FcrAlertRecord.CseAdditionalSsn1_MaxLength, 6, 4) + "";
              }
              else if (IsEmpty(local.FcrAlert.CseAdditionalSsn1) || Equal
                (local.FcrAlert.CseAdditionalSsn1, "000000000"))
              {
                local.Infrastructure.ReasonCode = "CSEALTSSNCH";
                local.Infrastructure.Detail =
                  Substring(local.FcrAlert.FcrAdditionalSsn1,
                  FcrAlertRecord.FcrAdditionalSsn1_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.FcrAdditionalSsn1,
                  FcrAlertRecord.FcrAdditionalSsn1_MaxLength, 4, 2) + "-" + Substring
                  (local.FcrAlert.FcrAdditionalSsn1,
                  FcrAlertRecord.FcrAdditionalSsn1_MaxLength, 6, 4) + " to XXX-XX-XXXX ";
                  
              }
              else
              {
                local.Infrastructure.ReasonCode = "CSEALTSSNCH";
                local.Infrastructure.Detail =
                  Substring(local.FcrAlert.FcrAdditionalSsn1,
                  FcrAlertRecord.FcrAdditionalSsn1_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.FcrAdditionalSsn1,
                  FcrAlertRecord.FcrAdditionalSsn1_MaxLength, 4, 2) + "-" + Substring
                  (local.FcrAlert.FcrAdditionalSsn1,
                  FcrAlertRecord.FcrAdditionalSsn1_MaxLength, 6, 4) + " to " + Substring
                  (local.FcrAlert.CseAdditionalSsn1,
                  FcrAlertRecord.CseAdditionalSsn1_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.CseAdditionalSsn1,
                  FcrAlertRecord.CseAdditionalSsn1_MaxLength, 4, 2) + "-" + Substring
                  (local.FcrAlert.CseAdditionalSsn1,
                  FcrAlertRecord.CseAdditionalSsn1_MaxLength, 6, 4);
              }
            }
            else
            {
              ++local.SsnSkipCount.Count;

              continue;
            }

            break;
          case 3:
            if (!Equal(local.FcrAlert.CseAdditionalSsn2,
              local.FcrAlert.FcrAdditionalSsn2))
            {
              local.Read.SsnNum9 =
                (int)StringToNumber(local.FcrAlert.CseAdditionalSsn2);

              if (ReadInvalidSsn1())
              {
                // added this check so we will not send a invalid ssn and cse 
                // person combination
                // to fcr, we stopped it earlier no we have to make sure no hist
                // records are created.
                ++local.SkipRecordCount.Count;

                // if the primary we will still send the record but we will not 
                // send this ssn along with
                // it so we will not create a hist record for it
                continue;
              }
              else
              {
                // this is ok so proceed
              }

              ++local.AltSsnChangeRecsRead.Count;

              if (IsEmpty(local.FcrAlert.FcrAdditionalSsn2) || Equal
                (local.FcrAlert.FcrAdditionalSsn2, "000000000"))
              {
                local.Infrastructure.ReasonCode = "CSEALTBLKSSNCH";
                local.Infrastructure.Detail = "XXX-XX-XXXX to " + Substring
                  (local.FcrAlert.CseAdditionalSsn2,
                  FcrAlertRecord.CseAdditionalSsn2_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.CseAdditionalSsn2,
                  FcrAlertRecord.CseAdditionalSsn2_MaxLength, 4, 2) + "-" + Substring
                  (local.FcrAlert.CseAdditionalSsn2,
                  FcrAlertRecord.CseAdditionalSsn2_MaxLength, 6, 4) + "";
              }
              else if (IsEmpty(local.FcrAlert.CseAdditionalSsn2) || Equal
                (local.FcrAlert.CseAdditionalSsn2, "000000000"))
              {
                local.Infrastructure.ReasonCode = "CSEALTSSNCH";
                local.Infrastructure.Detail =
                  Substring(local.FcrAlert.FcrAdditionalSsn2,
                  FcrAlertRecord.FcrAdditionalSsn2_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.FcrAdditionalSsn2,
                  FcrAlertRecord.FcrAdditionalSsn2_MaxLength, 4, 2) + "-" + Substring
                  (local.FcrAlert.FcrAdditionalSsn2,
                  FcrAlertRecord.FcrAdditionalSsn2_MaxLength, 6, 4) + " to XXX-XX-XXXX ";
                  
              }
              else
              {
                local.Infrastructure.ReasonCode = "CSEALTSSNCH";
                local.Infrastructure.Detail =
                  Substring(local.FcrAlert.FcrAdditionalSsn2,
                  FcrAlertRecord.FcrAdditionalSsn2_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.FcrAdditionalSsn2,
                  FcrAlertRecord.FcrAdditionalSsn2_MaxLength, 4, 2) + "-" + Substring
                  (local.FcrAlert.FcrAdditionalSsn2,
                  FcrAlertRecord.FcrAdditionalSsn2_MaxLength, 6, 4) + " to " + Substring
                  (local.FcrAlert.CseAdditionalSsn2,
                  FcrAlertRecord.CseAdditionalSsn2_MaxLength, 1, 3) + "-" + Substring
                  (local.FcrAlert.CseAdditionalSsn2,
                  FcrAlertRecord.CseAdditionalSsn2_MaxLength, 4, 2) + "-" + Substring
                  (local.FcrAlert.CseAdditionalSsn2,
                  FcrAlertRecord.CseAdditionalSsn2_MaxLength, 6, 4);
              }
            }
            else
            {
              ++local.SsnSkipCount.Count;

              continue;
            }

            break;
          default:
            goto AfterCycle1;
        }

        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " SENT TO FCR " +
          local.Obligor.Number + " " + TrimEnd(local.Obligor.LastName) + ", " +
          TrimEnd(local.Obligor.FirstName);
        local.PrevTotHistRecsCreated.Count =
          local.TotalHistRecordsCreated.Count;
        UseOeB412CreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto AfterCycle2;
        }

        if (local.PrevTotHistRecsCreated.Count != local
          .TotalHistRecordsCreated.Count)
        {
          if (local.SsnChangeCount.Count == 1)
          {
            ++local.PriSsnHistRecsCreated.Count;
          }
          else
          {
            ++local.AltSsnHistRecsCreated.Count;
          }
        }
      }
      while(local.SsnChangeCount.Count <= 3);

AfterCycle1:

      if (local.SsnSkipCount.Count >= 3)
      {
        ++local.SkipRecordCount.Count;
      }
      else
      {
        ++local.TotalSsnChgInputRecs.Count;
      }

Next:
      ;
    }
    while(!Equal(local.Eab.TextReturnCode, "EF"));

AfterCycle2:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.SkipRecordCount.Count += local.RecordsPersonNotFound.Count;
      UseOeB415Close();
      local.Eab.FileInstruction = "CLOSE";
      UseSiEabReadOutgoingFcrFile2();

      if (!Equal(local.Eab.TextReturnCode, "00"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        return;
      }

      local.Eab.FileInstruction = "CLOSE";
      UseSiEabWriteOutgoingFcrFile2();

      if (!Equal(local.Eab.TextReturnCode, "00"))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        return;
      }

      local.ProgramCheckpointRestart.RestartInd = "";
      local.ProgramCheckpointRestart.ProgramName =
        local.ProgramProcessingInfo.Name;
      UseUpdatePgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NN0000_ALL_OK";
      local.SkipRecordCount.Count += local.RecordsPersonNotFound.Count;
      UseOeB415Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveExternal1(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveExternal2(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
    target.TextLine8 = source.TextLine8;
  }

  private static void MoveFcrRecord(FcrRecord source, FcrRecord target)
  {
    target.RecordCount = source.RecordCount;
    target.RecordIdentifier = source.RecordIdentifier;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.ProcessStatus = source.ProcessStatus;
    target.ReasonCode = source.ReasonCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.Detail = source.Detail;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInd = source.RestartInd;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseOeB412CreateInfrastructure()
  {
    var useImport = new OeB412CreateInfrastructure.Import();
    var useExport = new OeB412CreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ItemsCreated.Count = local.TotalHistRecordsCreated.Count;

    Call(OeB412CreateInfrastructure.Execute, useImport, useExport);

    local.TotalHistRecordsCreated.Count = useExport.ItemsCreated.Count;
  }

  private void UseOeB415Close()
  {
    var useImport = new OeB415Close.Import();
    var useExport = new OeB415Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.TotalSsnChgInputRecs.Count = local.TotalSsnChgInputRecs.Count;
    useImport.SkipRecordCount.Count = local.SkipRecordCount.Count;
    useImport.RecordsPersonNotFound.Count = local.RecordsPersonNotFound.Count;
    useImport.TotalHistRecordsCreate.Count =
      local.TotalHistRecordsCreated.Count;
    useImport.PriSsnHistRecsCreated.Count = local.PriSsnHistRecsCreated.Count;
    useImport.AltSsnHistRecsCreated.Count = local.AltSsnHistRecsCreated.Count;

    Call(OeB415Close.Execute, useImport, useExport);
  }

  private void UseOeB415Housekeeping()
  {
    var useImport = new OeB415Housekeeping.Import();
    var useExport = new OeB415Housekeeping.Export();

    Call(OeB415Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseOeEabReadOutFcrAlertRecord()
  {
    var useImport = new OeEabReadOutFcrAlertRecord.Import();
    var useExport = new OeEabReadOutFcrAlertRecord.Export();

    MoveExternal2(local.Eab, useImport.ExternalFileStatus);
    MoveExternal2(local.Eab, useExport.ExternalFileStatus);
    useExport.FcrAlert.Assign(local.FcrAlert);

    Call(OeEabReadOutFcrAlertRecord.Execute, useImport, useExport);

    local.Eab.Assign(useExport.ExternalFileStatus);
    local.FcrAlert.Assign(useExport.FcrAlert);
  }

  private void UseSiCabReadAdabasBatch()
  {
    var useImport = new SiCabReadAdabasBatch.Import();
    var useExport = new SiCabReadAdabasBatch.Export();

    useImport.Obligor.Number = local.Obligor.Number;

    Call(SiCabReadAdabasBatch.Execute, useImport, useExport);

    local.Obligor.Assign(useExport.Obligor);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseSiEabReadOutgoingFcrFile1()
  {
    var useImport = new SiEabReadOutgoingFcrFile.Import();
    var useExport = new SiEabReadOutgoingFcrFile.Export();

    useImport.External.FileInstruction = local.Eab.FileInstruction;
    useExport.Header.Assign(local.Header);
    useExport.CaseRecord.Assign(local.CaseRecord);
    useExport.PersonRecord.Assign(local.PersonRecord);
    useExport.QueryRecord.Assign(local.QueryRecord);
    MoveFcrRecord(local.TrailerRecord, useExport.TrailerRecord);
    useExport.External.Assign(local.Eab);

    Call(SiEabReadOutgoingFcrFile.Execute, useImport, useExport);

    local.Header.Assign(useExport.Header);
    local.CaseRecord.Assign(useExport.CaseRecord);
    local.PersonRecord.Assign(useExport.PersonRecord);
    local.QueryRecord.Assign(useExport.QueryRecord);
    MoveFcrRecord(useExport.TrailerRecord, local.TrailerRecord);
    MoveExternal1(useExport.External, local.Eab);
  }

  private void UseSiEabReadOutgoingFcrFile2()
  {
    var useImport = new SiEabReadOutgoingFcrFile.Import();
    var useExport = new SiEabReadOutgoingFcrFile.Export();

    useImport.External.FileInstruction = local.Eab.FileInstruction;
    useExport.External.Assign(local.Eab);

    Call(SiEabReadOutgoingFcrFile.Execute, useImport, useExport);

    MoveExternal1(useExport.External, local.Eab);
  }

  private void UseSiEabWriteOutgoingFcrFile1()
  {
    var useImport = new SiEabWriteOutgoingFcrFile.Import();
    var useExport = new SiEabWriteOutgoingFcrFile.Export();

    MoveFcrRecord(local.WriteTrailerRecord, useImport.TrailerRecord);
    useImport.CaseRecord.Assign(local.WriteCaseRecord);
    useImport.PersonRecord.Assign(local.WritePersonRecord);
    useImport.Header.Assign(local.Header);
    useImport.QueryRecord.Assign(local.QueryRecord);
    useImport.External.FileInstruction = local.Eab.FileInstruction;

    Call(SiEabWriteOutgoingFcrFile.Execute, useImport, useExport);
  }

  private void UseSiEabWriteOutgoingFcrFile2()
  {
    var useImport = new SiEabWriteOutgoingFcrFile.Import();
    var useExport = new SiEabWriteOutgoingFcrFile.Export();

    useImport.External.FileInstruction = local.Eab.FileInstruction;

    Call(SiEabWriteOutgoingFcrFile.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case2.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Existing.Number);
      },
      (db, reader) =>
      {
        entities.Case2.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case2.Status = db.GetNullableString(reader, 1);
        entities.CaseRole.CspNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.Case2.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Existing.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.FcrAlert.PersonId);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Existing.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", local.MemberId.Text10);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.InvalidSsn.Populated);
    entities.Existing.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.InvalidSsn.CspNumber);
      },
      (db, reader) =>
      {
        entities.Existing.Number = db.GetString(reader, 0);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadInvalidSsn1()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Existing.Number);
        db.SetInt32(command, "ssn", local.Read.SsnNum9);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 2);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 3);
        entities.InvalidSsn.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInvalidSsn2()
  {
    entities.InvalidSsn.Populated = false;

    return ReadEach("ReadInvalidSsn2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "fcrSentDate", local.NullDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.FcrSentDate = db.GetNullableDate(reader, 2);
        entities.InvalidSsn.FcrProcessInd = db.GetNullableString(reader, 3);
        entities.InvalidSsn.Populated = true;

        return true;
      });
  }

  private void UpdateInvalidSsn()
  {
    System.Diagnostics.Debug.Assert(entities.InvalidSsn.Populated);

    var fcrSentDate = local.ProgramProcessingInfo.ProcessDate;

    entities.InvalidSsn.Populated = false;
    Update("UpdateInvalidSsn",
      (db, command) =>
      {
        db.SetNullableDate(command, "fcrSentDate", fcrSentDate);
        db.SetString(command, "cspNumber", entities.InvalidSsn.CspNumber);
        db.SetInt32(command, "ssn", entities.InvalidSsn.Ssn);
      });

    entities.InvalidSsn.FcrSentDate = fcrSentDate;
    entities.InvalidSsn.Populated = true;
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
    /// <summary>A CaseGroup group.</summary>
    [Serializable]
    public class CaseGroup
    {
      /// <summary>
      /// A value of SendRecord.
      /// </summary>
      [JsonPropertyName("sendRecord")]
      public Common SendRecord
      {
        get => sendRecord ??= new();
        set => sendRecord = value;
      }

      /// <summary>
      /// A value of PersonRecord.
      /// </summary>
      [JsonPropertyName("personRecord")]
      public FcrRecord PersonRecord
      {
        get => personRecord ??= new();
        set => personRecord = value;
      }

      /// <summary>
      /// A value of CaseRecord.
      /// </summary>
      [JsonPropertyName("caseRecord")]
      public FcrRecord CaseRecord
      {
        get => caseRecord ??= new();
        set => caseRecord = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common sendRecord;
      private FcrRecord personRecord;
      private FcrRecord caseRecord;
    }

    /// <summary>
    /// A value of ErrorFcrRecord.
    /// </summary>
    [JsonPropertyName("errorFcrRecord")]
    public FcrRecord ErrorFcrRecord
    {
      get => errorFcrRecord ??= new();
      set => errorFcrRecord = value;
    }

    /// <summary>
    /// A value of ConvertSsn.
    /// </summary>
    [JsonPropertyName("convertSsn")]
    public WorkArea ConvertSsn
    {
      get => convertSsn ??= new();
      set => convertSsn = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Case1 Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public SsnWorkArea Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of MemberId.
    /// </summary>
    [JsonPropertyName("memberId")]
    public WorkArea MemberId
    {
      get => memberId ??= new();
      set => memberId = value;
    }

    /// <summary>
    /// A value of CurrentCase.
    /// </summary>
    [JsonPropertyName("currentCase")]
    public FcrRecord CurrentCase
    {
      get => currentCase ??= new();
      set => currentCase = value;
    }

    /// <summary>
    /// A value of ChangeCaseRecordFound.
    /// </summary>
    [JsonPropertyName("changeCaseRecordFound")]
    public Common ChangeCaseRecordFound
    {
      get => changeCaseRecordFound ??= new();
      set => changeCaseRecordFound = value;
    }

    /// <summary>
    /// A value of ChildPassed.
    /// </summary>
    [JsonPropertyName("childPassed")]
    public Common ChildPassed
    {
      get => childPassed ??= new();
      set => childPassed = value;
    }

    /// <summary>
    /// A value of AdultPassed.
    /// </summary>
    [JsonPropertyName("adultPassed")]
    public Common AdultPassed
    {
      get => adultPassed ??= new();
      set => adultPassed = value;
    }

    /// <summary>
    /// Gets a value of Case1.
    /// </summary>
    [JsonIgnore]
    public Array<CaseGroup> Case1 => case1 ??= new(CaseGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Case1 for json serialization.
    /// </summary>
    [JsonPropertyName("case1")]
    [Computed]
    public IList<CaseGroup> Case1_Json
    {
      get => case1;
      set => Case1.Assign(value);
    }

    /// <summary>
    /// A value of ClearTrailerRecord.
    /// </summary>
    [JsonPropertyName("clearTrailerRecord")]
    public FcrRecord ClearTrailerRecord
    {
      get => clearTrailerRecord ??= new();
      set => clearTrailerRecord = value;
    }

    /// <summary>
    /// A value of ClearHeader.
    /// </summary>
    [JsonPropertyName("clearHeader")]
    public FcrRecord ClearHeader
    {
      get => clearHeader ??= new();
      set => clearHeader = value;
    }

    /// <summary>
    /// A value of ClearQueryRecord.
    /// </summary>
    [JsonPropertyName("clearQueryRecord")]
    public FcrRecord ClearQueryRecord
    {
      get => clearQueryRecord ??= new();
      set => clearQueryRecord = value;
    }

    /// <summary>
    /// A value of ClearCaseRecord.
    /// </summary>
    [JsonPropertyName("clearCaseRecord")]
    public FcrRecord ClearCaseRecord
    {
      get => clearCaseRecord ??= new();
      set => clearCaseRecord = value;
    }

    /// <summary>
    /// A value of ClearPersonRecord.
    /// </summary>
    [JsonPropertyName("clearPersonRecord")]
    public FcrRecord ClearPersonRecord
    {
      get => clearPersonRecord ??= new();
      set => clearPersonRecord = value;
    }

    /// <summary>
    /// A value of CaseId.
    /// </summary>
    [JsonPropertyName("caseId")]
    public WorkArea CaseId
    {
      get => caseId ??= new();
      set => caseId = value;
    }

    /// <summary>
    /// A value of KpcCheck.
    /// </summary>
    [JsonPropertyName("kpcCheck")]
    public WorkArea KpcCheck
    {
      get => kpcCheck ??= new();
      set => kpcCheck = value;
    }

    /// <summary>
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    public Common RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
    }

    /// <summary>
    /// A value of WriteTrailerRecord.
    /// </summary>
    [JsonPropertyName("writeTrailerRecord")]
    public FcrRecord WriteTrailerRecord
    {
      get => writeTrailerRecord ??= new();
      set => writeTrailerRecord = value;
    }

    /// <summary>
    /// A value of WriteCaseRecord.
    /// </summary>
    [JsonPropertyName("writeCaseRecord")]
    public FcrRecord WriteCaseRecord
    {
      get => writeCaseRecord ??= new();
      set => writeCaseRecord = value;
    }

    /// <summary>
    /// A value of WritePersonRecord.
    /// </summary>
    [JsonPropertyName("writePersonRecord")]
    public FcrRecord WritePersonRecord
    {
      get => writePersonRecord ??= new();
      set => writePersonRecord = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public FcrRecord Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of CaseRecord.
    /// </summary>
    [JsonPropertyName("caseRecord")]
    public FcrRecord CaseRecord
    {
      get => caseRecord ??= new();
      set => caseRecord = value;
    }

    /// <summary>
    /// A value of PersonRecord.
    /// </summary>
    [JsonPropertyName("personRecord")]
    public FcrRecord PersonRecord
    {
      get => personRecord ??= new();
      set => personRecord = value;
    }

    /// <summary>
    /// A value of QueryRecord.
    /// </summary>
    [JsonPropertyName("queryRecord")]
    public FcrRecord QueryRecord
    {
      get => queryRecord ??= new();
      set => queryRecord = value;
    }

    /// <summary>
    /// A value of TrailerRecord.
    /// </summary>
    [JsonPropertyName("trailerRecord")]
    public FcrRecord TrailerRecord
    {
      get => trailerRecord ??= new();
      set => trailerRecord = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public External Eab
    {
      get => eab ??= new();
      set => eab = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of FcrAlert.
    /// </summary>
    [JsonPropertyName("fcrAlert")]
    public FcrAlertRecord FcrAlert
    {
      get => fcrAlert ??= new();
      set => fcrAlert = value;
    }

    /// <summary>
    /// A value of RecordsPersonNotFound.
    /// </summary>
    [JsonPropertyName("recordsPersonNotFound")]
    public Common RecordsPersonNotFound
    {
      get => recordsPersonNotFound ??= new();
      set => recordsPersonNotFound = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of AltSsnHistRecsCreated.
    /// </summary>
    [JsonPropertyName("altSsnHistRecsCreated")]
    public Common AltSsnHistRecsCreated
    {
      get => altSsnHistRecsCreated ??= new();
      set => altSsnHistRecsCreated = value;
    }

    /// <summary>
    /// A value of PriSsnHistRecsCreated.
    /// </summary>
    [JsonPropertyName("priSsnHistRecsCreated")]
    public Common PriSsnHistRecsCreated
    {
      get => priSsnHistRecsCreated ??= new();
      set => priSsnHistRecsCreated = value;
    }

    /// <summary>
    /// A value of AltSsnChangeRecsRead.
    /// </summary>
    [JsonPropertyName("altSsnChangeRecsRead")]
    public Common AltSsnChangeRecsRead
    {
      get => altSsnChangeRecsRead ??= new();
      set => altSsnChangeRecsRead = value;
    }

    /// <summary>
    /// A value of PriSsnChangeRecsRead.
    /// </summary>
    [JsonPropertyName("priSsnChangeRecsRead")]
    public Common PriSsnChangeRecsRead
    {
      get => priSsnChangeRecsRead ??= new();
      set => priSsnChangeRecsRead = value;
    }

    /// <summary>
    /// A value of SsnChangeCount.
    /// </summary>
    [JsonPropertyName("ssnChangeCount")]
    public Common SsnChangeCount
    {
      get => ssnChangeCount ??= new();
      set => ssnChangeCount = value;
    }

    /// <summary>
    /// A value of SsnSkipCount.
    /// </summary>
    [JsonPropertyName("ssnSkipCount")]
    public Common SsnSkipCount
    {
      get => ssnSkipCount ??= new();
      set => ssnSkipCount = value;
    }

    /// <summary>
    /// A value of SkipRecordCount.
    /// </summary>
    [JsonPropertyName("skipRecordCount")]
    public Common SkipRecordCount
    {
      get => skipRecordCount ??= new();
      set => skipRecordCount = value;
    }

    /// <summary>
    /// A value of TotalHistRecordsCreated.
    /// </summary>
    [JsonPropertyName("totalHistRecordsCreated")]
    public Common TotalHistRecordsCreated
    {
      get => totalHistRecordsCreated ??= new();
      set => totalHistRecordsCreated = value;
    }

    /// <summary>
    /// A value of PrevTotHistRecsCreated.
    /// </summary>
    [JsonPropertyName("prevTotHistRecsCreated")]
    public Common PrevTotHistRecsCreated
    {
      get => prevTotHistRecsCreated ??= new();
      set => prevTotHistRecsCreated = value;
    }

    /// <summary>
    /// A value of TotalSsnChgInputRecs.
    /// </summary>
    [JsonPropertyName("totalSsnChgInputRecs")]
    public Common TotalSsnChgInputRecs
    {
      get => totalSsnChgInputRecs ??= new();
      set => totalSsnChgInputRecs = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ErrorCsePerson.
    /// </summary>
    [JsonPropertyName("errorCsePerson")]
    public CsePerson ErrorCsePerson
    {
      get => errorCsePerson ??= new();
      set => errorCsePerson = value;
    }

    private FcrRecord errorFcrRecord;
    private WorkArea convertSsn;
    private Case1 previous;
    private DateWorkArea nullDate;
    private SsnWorkArea read;
    private WorkArea memberId;
    private FcrRecord currentCase;
    private Common changeCaseRecordFound;
    private Common childPassed;
    private Common adultPassed;
    private Array<CaseGroup> case1;
    private FcrRecord clearTrailerRecord;
    private FcrRecord clearHeader;
    private FcrRecord clearQueryRecord;
    private FcrRecord clearCaseRecord;
    private FcrRecord clearPersonRecord;
    private WorkArea caseId;
    private WorkArea kpcCheck;
    private Common recordCount;
    private FcrRecord writeTrailerRecord;
    private FcrRecord writeCaseRecord;
    private FcrRecord writePersonRecord;
    private FcrRecord header;
    private FcrRecord caseRecord;
    private FcrRecord personRecord;
    private FcrRecord queryRecord;
    private FcrRecord trailerRecord;
    private CsePersonsWorkSet obligor;
    private Infrastructure infrastructure;
    private External eab;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private AbendData abendData;
    private FcrAlertRecord fcrAlert;
    private Common recordsPersonNotFound;
    private Common recordsRead;
    private Common altSsnHistRecsCreated;
    private Common priSsnHistRecsCreated;
    private Common altSsnChangeRecsRead;
    private Common priSsnChangeRecsRead;
    private Common ssnChangeCount;
    private Common ssnSkipCount;
    private Common skipRecordCount;
    private Common totalHistRecordsCreated;
    private Common prevTotHistRecsCreated;
    private Common totalSsnChgInputRecs;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson errorCsePerson;
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
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public Case1 Case2
    {
      get => case2 ??= new();
      set => case2 = value;
    }

    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CsePerson Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private CaseRole caseRole;
    private Case1 case2;
    private InvalidSsn invalidSsn;
    private CsePerson existing;
  }
#endregion
}
