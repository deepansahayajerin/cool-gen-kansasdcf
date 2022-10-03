// Program: SI_CADS_CASE_CLOSURE_PROCESSING, ID: 371731793, model: 746.
// Short name: SWE01817
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
/// A program: SI_CADS_CASE_CLOSURE_PROCESSING.
/// </para>
/// <para>
/// This CAB closes all the objects and assignments fopr a case closure.
/// </para>
/// </summary>
[Serializable]
public partial class SiCadsCaseClosureProcessing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CADS_CASE_CLOSURE_PROCESSING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCadsCaseClosureProcessing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCadsCaseClosureProcessing.
  /// </summary>
  public SiCadsCaseClosureProcessing(IContext context, Import import,
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
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 12/07/96  G. Lofton - MTW	Initial Dev
    // 01/06/97  G. Lofton - MTW	Add event logic
    // 05/02/97  Sid			IDCR # 257/258
    // 06/19/97  Sid			Change all Assignment discontinue
    // 				date to Current date.
    // ------------------------------------------------------------
    // 06/22/99 W.Campbell             Modified the properties
    //                                 
    // of a READ statement to
    //                                 
    // Select Only.
    // ---------------------------------------------------------
    // 10/25/99 W.Campbell             Added an IF
    //                                 
    // statement in order to
    //                                 
    // delete the case assignment
    //                                 
    // if it is becomes effective
    //                                 
    // in the future.  Work done
    //                                 
    // on PR# H00076080.
    // ----------------------------------------------------
    // 03/26/01 swsrchf   WR 000240   Close (end date) the "NA" person program 
    // record
    //                                
    // for any participant NOT on
    // another OPEN case.
    //                                
    // If an AF is found with a WT/EM
    // Discontinue date
    //                                
    // > Current date, the program is
    // NOT closed
    // ------------------------------------------------------------------------------------
    // 12/18/01 M.Lachowicz   PR 133827   Close (end date) the Interstate 
    // Programs
    //                                    
    // if CSE PERSON is not active on any
    //                                    
    // other case. Use last updated by '
    // SWEICADS' and last
    //                                    
    // updated timestap current timestamp.
    // ------------------------------------------------------------------------------------
    // 10/28/10 GVandy   	CQ109	1. End date monitored documents at case closure.
    // 				2. End date legal action assignments if the legal action has no 
    // association to other open cases.
    // 				3. Delete case alerts.
    // 				4. Don't end date monitored activity assignments.  They will be ended
    // by SRRUN123 based
    // 				   on the monitored activity closure.
    // 			CQ20040	It should not be an error if no active case unit function 
    // assignment is found to close.
    // ---------------------------------------------------------------------------------
    // 12/03/10 GVandy  CQ109 (Segment B)	End date interstate case assignments 
    // at case closure.
    // ---------------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // 12/18/2001 M. Lachowicz Start
    local.Current.Timestamp = Now();

    // 12/18/2001 M. Lachowicz End
    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (ReadCase())
    {
      local.Closure.Date = entities.Case1.StatusDate;

      // *** Close the Case Assignment ***
      ExitState = "CASE_ASSIGNMENT_NF";

      foreach(var item in ReadCaseAssignment())
      {
        // ----------------------------------------------------
        // 10/25/99 W.Campbell - Added the following
        // IF statement in order to delete the case
        // assignment if it is becomes effective in the
        // future.  Work done on PR# H00076080.
        // ----------------------------------------------------
        if (Lt(local.Current.Date, entities.CaseAssignment.EffectiveDate))
        {
          DeleteCaseAssignment();
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }

        try
        {
          UpdateCaseAssignment();
          ExitState = "ACO_NN0000_ALL_OK";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_ASSIGNMENT_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_ASSIGNMENT_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }
    else
    {
      ExitState = "CASE_NOT_CLOSED";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *** Close all Case Units and the Case Unit Function Assignments ***
    foreach(var item in ReadCaseUnit2())
    {
      try
      {
        UpdateCaseUnit();

        // 10/28/10 GVandy  CQ20040  It should not be an error if no active case
        // unit function assignment is found to close.
        foreach(var item1 in ReadCaseUnitFunctionAssignmt())
        {
          try
          {
            UpdateCaseUnitFunctionAssignmt();
            ExitState = "ACO_NN0000_ALL_OK";
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_UNIT_FUNCTION_ASSIGNMT_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CASE_UNIT_FUNCTION_ASSIGNMT_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_UNIT_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASE_UNIT_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // *** Work request 000240
    // *** 03/26/01 swsrchf
    // *** start
    local.Ar.Type1 = "AR";
    local.Ch.Type1 = "CH";
    local.Open.Status = "O";
    local.Em.MedType = "EM";
    local.Wt.MedType = "WT";
    local.Na.SystemGeneratedIdentifier = 12;
    local.Afi.SystemGeneratedIdentifier = 14;
    local.Fci.SystemGeneratedIdentifier = 16;
    local.Mai.SystemGeneratedIdentifier = 17;
    local.Nai.SystemGeneratedIdentifier = 18;
    local.Af.SystemGeneratedIdentifier = 2;
    local.Max.Date = new DateTime(2099, 12, 31);

    // ***
    // *** Close "NA" Person Program records, if the participant is NOT on
    // *** another Open case.
    // ***
    // *** Get all Supported persons for case
    // *************************************************
    //     PR133827   Interstate progams of the active
    //  or inactive child  should  be closed as well.
    // *************************************************
    // *************************************************
    //     PR134080   NA program of the inactive child
    //   should  be closed as well.
    // *************************************************
    foreach(var item in ReadCaseRole3())
    {
      // ***
      // *** Get the Supported person
      // ***
      if (ReadCsePerson())
      {
        local.Work.Count = 0;

        // ***
        // *** Is the Supported person in more than one case???
        // ***
        ReadCaseRole1();

        if (local.Work.Count == 0)
        {
          // ***
          // *** Does the Supported have a current "NA" program???
          // ***
          if (ReadPersonProgramProgram1())
          {
            // ***
            // *** Is there a prevoius "AF" program with a subtype of "EM" or "
            // WT" and
            // *** med discontinue date > current date???
            // ***
            if (ReadPersonProgram())
            {
              goto Read;
            }

            // *** Close (end date) the "NA" Person Program record, participant 
            // NOT on another
            // *** Open case.
            // *** set the Discontinue Date to the Case status date
            try
            {
              UpdatePersonProgram2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PERSON_PROGRAM_NU_RB";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PERSON_PROGRAM_PV_RB";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            // *** No OPEN "NA" program found
          }

          foreach(var item1 in ReadPersonProgramProgram2())
          {
            try
            {
              UpdatePersonProgram1();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "PERSON_PROGRAM_NU_RB";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "PERSON_PROGRAM_PV_RB";

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
      else
      {
        ExitState = "CSE_PERSON_NF_RB";

        return;
      }

Read:
      ;
    }

    // *** end
    // *** 03/26/01 swsrchf
    // *** Work request 000240
    // *** Close all Case Roles ***
    foreach(var item in ReadCaseRole2())
    {
      try
      {
        UpdateCaseRole();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_ROLE_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASE_ROLE_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // *** Close Legal Referrals and the Legal Referral Assignments ***
    foreach(var item in ReadLegalReferral())
    {
      if (AsChar(entities.LegalReferral.Status) != 'C' && AsChar
        (entities.LegalReferral.Status) != 'W' && AsChar
        (entities.LegalReferral.Status) != 'R')
      {
        try
        {
          UpdateLegalReferral();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LEGAL_REFERRAL_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "LEGAL_REFERRAL_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      foreach(var item1 in ReadLegalReferralAssignment())
      {
        try
        {
          UpdateLegalReferralAssignment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LEGAL_REFERRAL_ASSIGNMENT_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "LEGAL_REFERRAL_ASSIGNMENT_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    // *** Close Monitored Activities and Monitored Actvt Assignments ***
    foreach(var item in ReadInfrastructure())
    {
      // 10/28/10 GVandy  CQ109  End date legal action assignments if the legal 
      // action has no association to other open cases.
      foreach(var item1 in ReadLegalActionAssigmentLegalAction())
      {
        // --  Determine if legal action has any ties to open cases via LROL.
        foreach(var item2 in ReadLegalActionPerson2())
        {
          goto ReadEach;
        }

        // --  Determine if legal action has any ties to open cases via LOPS.
        foreach(var item2 in ReadLegalActionPerson1())
        {
          goto ReadEach;
        }

        try
        {
          UpdateLegalActionAssigment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LEGAL_ACTION_ASSIGNMENT_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "LEGAL_ACTION_ASSIGNMENT_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

ReadEach:
        ;
      }

      foreach(var item1 in ReadMonitoredActivity())
      {
        try
        {
          UpdateMonitoredActivity();

          // 10/28/10 GVandy  CQ109  Don't end date monitored activity 
          // assignments.  They will be ended by SRRUN123 based
          // on the monitored activity closure.
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "MONITORED_ACTIVITY_NF";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "MONITORED_ACTIVITY_NF";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // 10/28/10 GVandy  CQ109  End date monitored documents at case closure.
      foreach(var item1 in ReadMonitoredDocument())
      {
        try
        {
          UpdateMonitoredDocument();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_MONITORED_DOC_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_MONITORED_DOC_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // 10/28/10 GVandy  CQ109   Delete case alerts.
      foreach(var item1 in ReadOfficeServiceProviderAlert())
      {
        DeleteOfficeServiceProviderAlert();
      }
    }

    // 12/03/10 GVandy  CQ109 (Segment B)  End date interstate case assignments 
    // at case closure.
    local.InterstateCase.KsCaseId = entities.Case1.Number;

    foreach(var item in ReadInterstateCaseAssignment())
    {
      try
      {
        UpdateInterstateCaseAssignment();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INTERSTATE_CASE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INTERSTATE_CASE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***	Begin Event insertion	***
    // *******************************************************************
    // 		EVENT PROCESSING
    // - Raise Event for "CASE_CLOSED".
    //   (Include Case_Closure_Reason_Code in the detail)
    // - Send alerts to the OSP assigned to the Case/C_Units/Leg_Ref.
    // - Raise events at the Case_Unit level. If there are no case units,
    //   then raise it at the case level.
    // *******************************************************************
    if (ReadInterstateRequest())
    {
      if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "OS";
      }
    }
    else
    {
      local.Infrastructure.InitiatingStateCode = "KS";
    }

    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.EventId = 5;
    local.Infrastructure.ReasonCode = "CASECLOSED";
    local.Infrastructure.BusinessObjectCd = "CAS";
    local.Infrastructure.CaseNumber = import.Case1.Number;
    local.Infrastructure.UserId = "CADS";
    local.Infrastructure.ReferenceDate = local.Closure.Date;

    foreach(var item in ReadCaseUnit1())
    {
      local.Infrastructure.ReasonCode = "CASECLOSED";
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
      local.TextWorkArea.Text30 = "Case Closed :";
      local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
      local.TextWorkArea.Text8 = UseCabConvertDate2String();
      local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
        .TextWorkArea.Text8 + " " + ";";
      local.TextWorkArea.Text30 = " Closure Reason :";
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
        (local.TextWorkArea.Text30) + TrimEnd(entities.Case1.ClosureReason);
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (local.Infrastructure.CaseUnitNumber.GetValueOrDefault() == 0)
    {
      local.Infrastructure.ReasonCode = "CASECLOSEDWOCAU";
      local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
      local.TextWorkArea.Text8 = UseCabConvertDate2String();
      local.Infrastructure.Detail = "Case with no Case Units, Closed :" + TrimEnd
        (local.TextWorkArea.Text8) + ";";
      local.TextWorkArea.Text30 = " Closure Reason :";
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
        (local.TextWorkArea.Text30) + TrimEnd(entities.Case1.ClosureReason);
      UseSpCabCreateInfrastructure();
    }

    // ***	End Event insertion	***
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void DeleteCaseAssignment()
  {
    Update("DeleteCaseAssignment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CaseAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.CaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.CaseAssignment.OffId);
        db.SetString(command, "ospCode", entities.CaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.CaseAssignment.CasNo);
      });
  }

  private void DeleteOfficeServiceProviderAlert()
  {
    Update("DeleteOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier);
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
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 4);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Case1.Note = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Closure.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.CaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 7);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 8);
        entities.CaseAssignment.OspCode = db.GetString(reader, 9);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 10);
        entities.CaseAssignment.CasNo = db.GetString(reader, 11);
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole1()
  {
    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "status", local.Open.Status ?? "");
        db.SetString(command, "numb", entities.Case1.Number);
      },
      (db, reader) =>
      {
        local.Work.Count = db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Closure.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole3()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "type1", local.Ar.Type1);
        db.SetString(command, "type2", local.Ch.Type1);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetDate(command, "startDate", local.Closure.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 3);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.CaseUnit.CasNo = db.GetString(reader, 6);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableDate(
          command, "closureDate", local.Closure.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 3);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.CaseUnit.CasNo = db.GetString(reader, 6);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.CaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CaseUnit.CuNumber);
        db.SetNullableDate(
          command, "discontinueDate", local.Closure.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnitFunctionAssignmt.ReasonCode = db.GetString(reader, 0);
        entities.CaseUnitFunctionAssignmt.OverrideInd = db.GetString(reader, 1);
        entities.CaseUnitFunctionAssignmt.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 7);
        entities.CaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 8);
        entities.CaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 9);
        entities.CaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 10);
        entities.CaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 11);
        entities.CaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 12);
        entities.CaseUnitFunctionAssignmt.Function = db.GetString(reader, 13);
        entities.CaseUnitFunctionAssignmt.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 1);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseAssignment()
  {
    entities.InterstateCaseAssignment.Populated = false;

    return ReadEach("ReadInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "ksCaseId", local.InterstateCase.KsCaseId ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateCaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.InterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.InterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.InterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.InterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.InterstateCaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.InterstateCaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.InterstateCaseAssignment.OspCode = db.GetString(reader, 7);
        entities.InterstateCaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.InterstateCaseAssignment.IcsDate = db.GetDate(reader, 9);
        entities.InterstateCaseAssignment.IcsNo = db.GetInt64(reader, 10);
        entities.InterstateCaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionAssigmentLegalAction()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionAssigment.Populated = false;

    return ReadEach("ReadLegalActionAssigmentLegalAction",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.LegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.LegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.LegalAction.Populated = true;
        entities.LegalActionAssigment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPerson1()
  {
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPerson2()
  {
    entities.LegalActionPerson.Populated = false;

    return ReadEach("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 1);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 5);
        entities.LegalActionPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.LastUpdatedBy = db.GetString(reader, 4);
        entities.LegalReferral.LastUpdatedTimestamp = db.GetDateTime(reader, 5);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralAssignment.Populated = false;

    return ReadEach("ReadLegalReferralAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
        db.SetNullableDate(
          command, "discontinueDate", local.Closure.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.LegalReferralAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.LegalReferralAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 6);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 7);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 8);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 9);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 10);
        entities.LegalReferralAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity()
  {
    entities.MonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infSysGenId",
          entities.Infrastructure.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "closureDate", local.Closure.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 1);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 3);
        entities.MonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.MonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 6);
        entities.MonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredDocument()
  {
    entities.MonitoredDocument.Populated = false;

    return ReadEach("ReadMonitoredDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "actRespDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MonitoredDocument.ActualResponseDate =
          db.GetNullableDate(reader, 0);
        entities.MonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 1);
        entities.MonitoredDocument.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.MonitoredDocument.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.MonitoredDocument.InfId = db.GetInt32(reader, 4);
        entities.MonitoredDocument.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderAlert()
  {
    entities.OfficeServiceProviderAlert.Populated = false;

    return ReadEach("ReadOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 1);
        entities.OfficeServiceProviderAlert.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgram()
  {
    entities.PriorAfWithEmOrWt.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(
          command, "prgGeneratedId", local.Af.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "medTypeDiscDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(command, "medType1", local.Em.MedType ?? "");
        db.SetNullableString(command, "medType2", local.Wt.MedType ?? "");
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PriorAfWithEmOrWt.CspNumber = db.GetString(reader, 0);
        entities.PriorAfWithEmOrWt.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.PriorAfWithEmOrWt.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PriorAfWithEmOrWt.PrgGeneratedId = db.GetInt32(reader, 3);
        entities.PriorAfWithEmOrWt.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PriorAfWithEmOrWt.MedType = db.GetNullableString(reader, 5);
        entities.PriorAfWithEmOrWt.Populated = true;
      });
  }

  private bool ReadPersonProgramProgram1()
  {
    entities.KeyOnly.Populated = false;
    entities.Na.Populated = false;

    return Read("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(
          command, "prgGeneratedId", local.Na.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Na.CspNumber = db.GetString(reader, 0);
        entities.Na.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.Na.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Na.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Na.LastUpdatdTstamp = db.GetNullableDateTime(reader, 4);
        entities.Na.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.KeyOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.KeyOnly.Populated = true;
        entities.Na.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram2()
  {
    entities.Interstate.Populated = false;
    entities.KeyOnly.Populated = false;

    return ReadEach("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.Nai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.Fci.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.Mai.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.Afi.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Interstate.CspNumber = db.GetString(reader, 0);
        entities.Interstate.DiscontinueDate = db.GetNullableDate(reader, 1);
        entities.Interstate.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Interstate.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Interstate.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.Interstate.PrgGeneratedId = db.GetInt32(reader, 5);
        entities.KeyOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Interstate.Populated = true;
        entities.KeyOnly.Populated = true;

        return true;
      });
  }

  private void UpdateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseAssignment.Populated = false;
    Update("UpdateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CaseAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.CaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.CaseAssignment.OffId);
        db.SetString(command, "ospCode", entities.CaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.CaseAssignment.CasNo);
      });

    entities.CaseAssignment.DiscontinueDate = discontinueDate;
    entities.CaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.CaseAssignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseAssignment.Populated = true;
  }

  private void UpdateCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var endDate = local.Closure.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.CaseRole.Populated = false;
    Update("UpdateCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "type", entities.CaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CaseRole.Identifier);
      });

    entities.CaseRole.EndDate = endDate;
    entities.CaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CaseRole.Populated = true;
  }

  private void UpdateCaseUnit()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var closureDate = local.Closure.Date;
    var closureReasonCode = "CA";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnit.Populated = false;
    Update("UpdateCaseUnit",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });

    entities.CaseUnit.ClosureDate = closureDate;
    entities.CaseUnit.ClosureReasonCode = closureReasonCode;
    entities.CaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseUnit.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt()
  {
    System.Diagnostics.Debug.
      Assert(entities.CaseUnitFunctionAssignmt.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.CaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(command, "offId", entities.CaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(command, "csuNo", entities.CaseUnitFunctionAssignmt.CsuNo);
        db.SetString(command, "casNo", entities.CaseUnitFunctionAssignmt.CasNo);
      });

    entities.CaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.CaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CaseUnitFunctionAssignmt.Populated = true;
  }

  private void UpdateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateCaseAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.InterstateCaseAssignment.Populated = false;
    Update("UpdateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.InterstateCaseAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.InterstateCaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.InterstateCaseAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.InterstateCaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.InterstateCaseAssignment.OspDate.GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          entities.InterstateCaseAssignment.IcsDate.GetValueOrDefault());
        db.SetInt64(command, "icsNo", entities.InterstateCaseAssignment.IcsNo);
      });

    entities.InterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.InterstateCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateCaseAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateCaseAssignment.Populated = true;
  }

  private void UpdateLegalActionAssigment()
  {
    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LegalActionAssigment.Populated = false;
    Update("UpdateLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDt", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.LegalActionAssigment.CreatedTimestamp.GetValueOrDefault());
      });

    entities.LegalActionAssigment.DiscontinueDate = discontinueDate;
    entities.LegalActionAssigment.LastUpdatedBy = lastUpdatedBy;
    entities.LegalActionAssigment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LegalActionAssigment.Populated = true;
  }

  private void UpdateLegalReferral()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);

    var statusDate = local.Closure.Date;
    var status = "C";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LegalReferral.Populated = false;
    Update("UpdateLegalReferral",
      (db, command) =>
      {
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "status", status);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
        db.SetInt32(command, "identifier", entities.LegalReferral.Identifier);
      });

    entities.LegalReferral.StatusDate = statusDate;
    entities.LegalReferral.Status = status;
    entities.LegalReferral.LastUpdatedBy = lastUpdatedBy;
    entities.LegalReferral.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.LegalReferral.Populated = true;
  }

  private void UpdateLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LegalReferralAssignment.Populated = false;
    Update("UpdateLegalReferralAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.LegalReferralAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.LegalReferralAssignment.SpdId);
        db.SetInt32(command, "offId", entities.LegalReferralAssignment.OffId);
        db.SetString(
          command, "ospCode", entities.LegalReferralAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.LegalReferralAssignment.CasNo);
        db.SetInt32(command, "lgrId", entities.LegalReferralAssignment.LgrId);
      });

    entities.LegalReferralAssignment.DiscontinueDate = discontinueDate;
    entities.LegalReferralAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.LegalReferralAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.LegalReferralAssignment.Populated = true;
  }

  private void UpdateMonitoredActivity()
  {
    var closureDate = local.Closure.Date;
    var closureReasonCode = "SYS";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.MonitoredActivity.Populated = false;
    Update("UpdateMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.MonitoredActivity.SystemGeneratedIdentifier);
      });

    entities.MonitoredActivity.ClosureDate = closureDate;
    entities.MonitoredActivity.ClosureReasonCode = closureReasonCode;
    entities.MonitoredActivity.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredActivity.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.MonitoredActivity.Populated = true;
  }

  private void UpdateMonitoredDocument()
  {
    System.Diagnostics.Debug.Assert(entities.MonitoredDocument.Populated);

    var actualResponseDate = local.Current.Date;
    var closureReasonCode = "N";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.MonitoredDocument.Populated = false;
    Update("UpdateMonitoredDocument",
      (db, command) =>
      {
        db.SetNullableDate(command, "actRespDt", actualResponseDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "infId", entities.MonitoredDocument.InfId);
      });

    entities.MonitoredDocument.ActualResponseDate = actualResponseDate;
    entities.MonitoredDocument.ClosureReasonCode = closureReasonCode;
    entities.MonitoredDocument.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredDocument.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.MonitoredDocument.Populated = true;
  }

  private void UpdatePersonProgram1()
  {
    System.Diagnostics.Debug.Assert(entities.Interstate.Populated);

    var discontinueDate = local.Closure.Date;
    var lastUpdatedBy = "SWEICADS";
    var lastUpdatdTstamp = local.Current.Timestamp;

    entities.Interstate.Populated = false;
    Update("UpdatePersonProgram1",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetString(command, "cspNumber", entities.Interstate.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Interstate.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(
          command, "prgGeneratedId", entities.Interstate.PrgGeneratedId);
      });

    entities.Interstate.DiscontinueDate = discontinueDate;
    entities.Interstate.LastUpdatedBy = lastUpdatedBy;
    entities.Interstate.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.Interstate.Populated = true;
  }

  private void UpdatePersonProgram2()
  {
    System.Diagnostics.Debug.Assert(entities.Na.Populated);

    var discontinueDate = local.Closure.Date;
    var lastUpdatedBy = "SWEICADS";
    var lastUpdatdTstamp = local.Current.Timestamp;

    entities.Na.Populated = false;
    Update("UpdatePersonProgram2",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetString(command, "cspNumber", entities.Na.CspNumber);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Na.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "prgGeneratedId", entities.Na.PrgGeneratedId);
      });

    entities.Na.DiscontinueDate = discontinueDate;
    entities.Na.LastUpdatedBy = lastUpdatedBy;
    entities.Na.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.Na.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Afi.
    /// </summary>
    [JsonPropertyName("afi")]
    public Program Afi
    {
      get => afi ??= new();
      set => afi = value;
    }

    /// <summary>
    /// A value of Mai.
    /// </summary>
    [JsonPropertyName("mai")]
    public Program Mai
    {
      get => mai ??= new();
      set => mai = value;
    }

    /// <summary>
    /// A value of Fci.
    /// </summary>
    [JsonPropertyName("fci")]
    public Program Fci
    {
      get => fci ??= new();
      set => fci = value;
    }

    /// <summary>
    /// A value of Nai.
    /// </summary>
    [JsonPropertyName("nai")]
    public Program Nai
    {
      get => nai ??= new();
      set => nai = value;
    }

    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public Program Af
    {
      get => af ??= new();
      set => af = value;
    }

    /// <summary>
    /// A value of Em.
    /// </summary>
    [JsonPropertyName("em")]
    public PersonProgram Em
    {
      get => em ??= new();
      set => em = value;
    }

    /// <summary>
    /// A value of Wt.
    /// </summary>
    [JsonPropertyName("wt")]
    public PersonProgram Wt
    {
      get => wt ??= new();
      set => wt = value;
    }

    /// <summary>
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public Program Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public Case1 Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CaseRole Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of Closure.
    /// </summary>
    [JsonPropertyName("closure")]
    public DateWorkArea Closure
    {
      get => closure ??= new();
      set => closure = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private InterstateCase interstateCase;
    private DateWorkArea null1;
    private Program afi;
    private Program mai;
    private Program fci;
    private Program nai;
    private Program af;
    private PersonProgram em;
    private PersonProgram wt;
    private Program na;
    private Case1 open;
    private CaseRole ar;
    private CaseRole ch;
    private DateWorkArea max;
    private Common work;
    private DateWorkArea current;
    private DateWorkArea closure;
    private DateWorkArea dateWorkArea;
    private TextWorkArea textWorkArea;
    private CaseRole caseRole;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public Case1 Open
    {
      get => open ??= new();
      set => open = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    /// <summary>
    /// A value of Interstate.
    /// </summary>
    [JsonPropertyName("interstate")]
    public PersonProgram Interstate
    {
      get => interstate ??= new();
      set => interstate = value;
    }

    /// <summary>
    /// A value of PriorAfWithEmOrWt.
    /// </summary>
    [JsonPropertyName("priorAfWithEmOrWt")]
    public PersonProgram PriorAfWithEmOrWt
    {
      get => priorAfWithEmOrWt ??= new();
      set => priorAfWithEmOrWt = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public Program KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public PersonProgram Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of MonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("monitoredActivityAssignment")]
    public MonitoredActivityAssignment MonitoredActivityAssignment
    {
      get => monitoredActivityAssignment ??= new();
      set => monitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
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
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of CaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("caseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CaseUnitFunctionAssignmt
    {
      get => caseUnitFunctionAssignmt ??= new();
      set => caseUnitFunctionAssignmt = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateCaseAssignment interstateCaseAssignment;
    private InterstateCase interstateCase;
    private Case1 open;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
    private LegalActionAssigment legalActionAssigment;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private OutgoingDocument outgoingDocument;
    private MonitoredDocument monitoredDocument;
    private PersonProgram interstate;
    private PersonProgram priorAfWithEmOrWt;
    private Program keyOnly;
    private PersonProgram na;
    private MonitoredActivityAssignment monitoredActivityAssignment;
    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferral legalReferral;
    private CaseAssignment caseAssignment;
    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private InterstateRequest interstateRequest;
  }
#endregion
}
