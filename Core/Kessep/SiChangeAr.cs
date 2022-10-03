// Program: SI_CHANGE_AR, ID: 371785561, model: 746.
// Short name: SWE01114
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
/// A program: SI_CHANGE_AR.
/// </para>
/// <para>
/// RESP: SRVINIT		
/// This AB end-dates the current AR and creates the new AR for a case.
/// </para>
/// </summary>
[Serializable]
public partial class SiChangeAr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHANGE_AR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiChangeAr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiChangeAr.
  /// </summary>
  public SiChangeAr(IContext context, Import import, Export export):
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
    // ??/??/96  Ken Evans - MTW	Initial Development
    // 03/13/97  G. Lofton - MTW	Add logic to copy the case
    // 				unit function assignment.
    // 04/29/97   SHERAZ MALIK		CHANGE CURRENT_DATE
    // 05/??/97   J. Rookard - MTW     Add event code for AR changes.
    // 06/23/97   J. Rookard - MTW     Add code supporting closure and 
    // duplicating of Monitored Activities when AR changes.
    // 11/20/97   J. Rookard - MTW     Infrastructure Performance Enhancement 
    // changes
    // ------------------------------------------------------------
    // 02/12/99 W.Campbell             Modified an IF stmt
    //                                 
    // to remove the test for
    //                                 
    // local_ch_becomes_ar
    //                                 
    // ief_supplied_flag = 'Y'.
    //                                 
    // The original IF was copied
    //                                 
    // and disabled to save it in
    //                                 
    // case it is needed again.
    // ----------------------------------------------
    // 02/16/99 W.Campbell             Modified another IF stmt
    //                                 
    // to remove the test for
    //                                 
    // local_ch_becomes_ar
    //                                 
    // ief_supplied_flag = 'Y'.
    //                                 
    // The original IF was copied
    //                                 
    // and disabled to save it in
    //                                 
    // case it is needed again.
    // ----------------------------------------------
    // 02/17/99 W.Campbell             Added an IF stmt
    //                                 
    // in order to skip the remaining
    //                                 
    // work in a loop if the new_ar
    //                                 
    // is the ch so that a new case
    //                                 
    // unit is not created with
    //                                 
    // the ch as the ar for itself.
    // ----------------------------------------------
    // 02/22/99 W.Campbell             Disabled an
    //                                 
    // EXIT STATE IS and ESCAPE
    //                                 
    // within a not found condition
    //                                 
    // so that it is not an error for
    //                                 
    // a CH to become the AR when
    //                                 
    // there is no AP on the CASE.
    // ----------------------------------------------
    // 05/25/99 M. Lachowicz           Replace zdel exit state by
    //                                 
    // by new exit state.
    // ----------------------------------------------
    // 06/23/99  M. Lachowicz     Change property of READ
    //                            (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 07/26/99  M. Lachowicz     Change code to avoid situation
    //                            that two active ARs exist for the same day.
    // ------------------------------------------------------------
    // 10/22/99  M. Lachowicz     Change condition of
    //                            case_assignment effective_date from
    //                            greater than current date to equal or
    //                            greater than curren date.  PR #77568.
    // ------------------------------------------------------------
    // 01/27/00  M. Lachowicz     Delete Case Unit
    //                            function assignment if discontinue date
    //                            is before start date.
    //                            PR #85988.
    // ------------------------------------------------------------
    // 04/28/00 W.Campbell        Changed the created_by
    //                            in a set stmt from USER_ID to
    //                            the created by user id for the
    //                            curr_ar Monitored Activity which
    //                            is being outdated.  Disabled the
    //                            previous set stmt.
    //                            Work done on PR# 88664.
    // --------------------------------------------
    // 06/23/00 M.Lachowicz       Changed  back value of
    //                            override indicator form X to N.
    //                            Deleted all CASE UNIT FUNCTION
    //                            ASSIGNMENTS which have discontinue
    //                            date less than effective date.
    //                            Work done on PR# 95251.
    // --------------------------------------------
    // 01/12/00 M.Lachowicz       Fixed problem 108484.
    // --------------------------------------------
    // 01/31/01  M. Lachowicz          Use previous information
    //                                 
    // if person already played import
    // role.
    //                                 
    // ------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.ApBecomesAr.Flag = "N";
    local.ChBecomesAr.Flag = "N";

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCase())
    {
      // Determine the Infrastructure Initiating State Code for any events 
      // raised.
      if (ReadInterstateRequest())
      {
        local.Infrastructure.InitiatingStateCode = "OS";
      }
      else
      {
        local.Infrastructure.InitiatingStateCode = "KS";
      }

      // M.L 10/22/99 Start
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              CURSOR ONLY
      if (!ReadCaseAssignment1())
      {
        ExitState = "CASE_ASSIGNMENT_NF";

        return;
      }

      // M.L 10/22/99 End
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (!ReadOfficeServiceProvider1())
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";

        return;
      }

      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (!ReadCsePerson1())
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }

      // 06/23/99 M.L
      //              Change property of the following READ EACH to
      //              OPTIMIZE FOR 1 ROW
      if (ReadCaseRole2())
      {
        local.LastCaseRole.Identifier = entities.Related.Identifier;
      }

      local.Group.Index = 0;
      local.Group.CheckSize();

      local.Group.Update.Det.FuncText3 = "LOC";

      local.Group.Index = 1;
      local.Group.CheckSize();

      local.Group.Update.Det.FuncText3 = "PAT";

      local.Group.Index = 2;
      local.Group.CheckSize();

      local.Group.Update.Det.FuncText3 = "OBG";

      local.Group.Index = 3;
      local.Group.CheckSize();

      local.Group.Update.Det.FuncText3 = "ENF";
      local.End.Date = AddDays(import.NewArCaseRole.StartDate, -1);
      UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // ------------------------------------------------
    // End Date the current AR Role
    // ------------------------------------------------
    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCaseRoleCsePerson())
    {
      if (Lt(local.End.Date, entities.CurrArCaseRole.StartDate))
      {
        // 07/26/99  M. L  Start
        ExitState = "SI0000_TWO_ACTIVE_AR";

        return;

        // 07/26/99  M. L  End
      }
      else
      {
        local.Use.Date = local.End.Date;
      }

      if (Lt(local.Use.Date, local.Current.Date))
      {
        local.Ar.ArChgProcReqInd = "Y";
      }
      else
      {
        local.Ar.ArChgProcReqInd = "N";
      }

      if (!Lt(AddDays(entities.CurrArCaseRole.StartDate, 1), local.Use.Date))
      {
        local.Ar.ArInvalidInd = "Y";
      }
      else
      {
        local.Ar.ArInvalidInd = "N";
      }

      try
      {
        UpdateCaseRole2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_ROLE_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASE_ROLE_PV_RB";

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
      ExitState = "AR_NF_RB";

      return;
    }

    // Populate the non-variable local views for creating Infrastructure 
    // occurrences.
    UseCabConvertDate2String();
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = "ROLE";
    local.Infrastructure.ReferenceDate = local.Current.Date;
    local.Infrastructure.EventId = 11;
    local.Infrastructure.CaseNumber = entities.Case1.Number;
    local.Infrastructure.SituationNumber = 0;

    // Determine if the new incoming AR is currently an AP on the Case.  If so, 
    // close the Case Units and the Case Unit assignments related to those Case
    // Units, where the AP (who is becoming the AR) participates and end date
    // the AP occurrence.
    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadCaseRole3())
    {
      local.ApBecomesAr.Flag = "Y";
    }

    if (AsChar(local.ApBecomesAr.Flag) == 'Y')
    {
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (!ReadCsePerson3())
      {
        // 05/25/99 M. Lachowicz      Replace zdel exit state by
        //                            by new exit state.
        ExitState = "CSE_PERSON_NF";

        return;
      }

      foreach(var item in ReadCaseUnit5())
      {
        try
        {
          UpdateCaseUnit2();

          foreach(var item1 in ReadCaseUnitFunctionAssignmt4())
          {
            // 01/27/00 - M.L Start
            if (Lt(local.Current.Date,
              entities.CurrApCaseUnitFunctionAssignmt.EffectiveDate))
            {
              DeleteCaseUnitFunctionAssignmt2();
            }
            else
            {
              try
              {
                UpdateCaseUnitFunctionAssignmt3();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_NU_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            // 01/27/00 - M.L End
          }

          // Close all open Monitored Activities for the Case Unit being 
          // deactivated.  Since the AP is becoming the AR closure is ok.
          foreach(var item1 in ReadMonitoredActivity1())
          {
            try
            {
              UpdateMonitoredActivity1();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "MONITORED_ACTIVITY_AE_WITH_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "MONITORED_ACTIVITY_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            foreach(var item2 in ReadMonitoredActivityAssignment1())
            {
              try
              {
                UpdateMonitoredActivityAssignment1();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_AE";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          // Create the "AP becomes the AR" Infrastructure event occurrence.
          local.Infrastructure.ReasonCode = "APBECOMEAR";
          local.Infrastructure.CaseUnitNumber =
            entities.CurrApCaseUnit.CuNumber;
          local.Infrastructure.BusinessObjectCd = "CAU";
          local.Infrastructure.CsePersonNumber = entities.Ap.Number;
          local.Infrastructure.Detail = "Case Unit AP ";
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Ap.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " has become AR for Case ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Case1.Number + " on ";
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
            .TextWorkArea.Text8 + ".";
          UseSpCabCreateInfrastructure2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_UNIT_AE_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_UNIT_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      try
      {
        UpdateCaseRole1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_ROLE_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASE_ROLE_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (AsChar(local.ApBecomesAr.Flag) == 'Y')
    {
      // If AP becomes AR flag is Y,then incoming AR cannot be a CHild on the 
      // case
      // that is becoming the AR....so following logic is skipped.
    }
    else
    {
      // Determine if the new AR is currently a CHild on the Case.
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCaseRole4())
      {
        local.ChBecomesAr.Flag = "Y";
      }
    }

    if (AsChar(local.ChBecomesAr.Flag) == 'Y')
    {
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (!ReadCsePerson5())
      {
        ExitState = "CHILD_NF_RB";

        return;
      }

      // Do not need to validate that incoming CHild AR is not last child on 
      // Case.  That has already been done in the ROLE procedure step.
      // Update the Case Unit that the Child (who is becoming the AR) 
      // participates in and create new Case Units for the remaining children
      // with the new AR (former CHild) and all active AP's.
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCaseUnit1())
      {
        try
        {
          UpdateCaseUnit3();

          foreach(var item in ReadCaseUnitFunctionAssignmt5())
          {
            // 01/27/00 - M.L Start
            if (Lt(local.Current.Date,
              entities.CurrChCaseUnitFunctionAssignmt.EffectiveDate))
            {
              DeleteCaseUnitFunctionAssignmt3();
            }
            else
            {
              try
              {
                UpdateCaseUnitFunctionAssignmt4();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_NU_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            // 01/27/00 - M.L End
          }

          // Close open Monitored Activities for the current Child Case Unit.  
          // Since the CHild is becoming the AR closure is okay.
          foreach(var item in ReadMonitoredActivity3())
          {
            try
            {
              UpdateMonitoredActivity3();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "MONITORED_ACTIVITY_AE_WITH_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "MONITORED_ACTIVITY_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            foreach(var item1 in ReadMonitoredActivityAssignment3())
            {
              try
              {
                UpdateMonitoredActivityAssignment3();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_AE";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          // Create the "Child becomes the AR" event Infrastructure occurrence.
          local.Infrastructure.ReasonCode = "CHBECOMESAR";
          local.Infrastructure.CaseUnitNumber =
            entities.CurrChCaseUnit.CuNumber;
          local.Infrastructure.BusinessObjectCd = "CAU";
          local.Infrastructure.CsePersonNumber = entities.Ch.Number;
          local.Infrastructure.Detail = "Case Unit CH ";
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Ch.Number;
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " has become AR for Case ";
            
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
            .Case1.Number + " on ";
          local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
            .TextWorkArea.Text8 + ".";
          UseSpCabCreateInfrastructure2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_UNIT_AE_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_UNIT_PV_RB";

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
        // ------------------------------------
        // 02/22/99 W.Campbell - Disabled the following
        // EXIT STATE IS and ESCAPE so that it is not
        // an error for a CH to become the AR when there
        // is no AP on the CASE.
        // ------------------------------------
      }

      try
      {
        UpdateCaseRole3();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_ROLE_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASE_ROLE_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ------------------------------------------------
    // Create new AR Role
    // ------------------------------------------------
    // 01/31/01 M.L Start
    ReadCaseRole1();

    // 01/31/01 M.L End
    ++local.LastCaseRole.Identifier;

    try
    {
      CreateCaseRole();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CASE_ROLE_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CASE_ROLE_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ------------------------------------------------------------
    // Close case units and case unit function assignments for
    // old AR.
    // ------------------------------------------------------------
    foreach(var item in ReadCaseUnit4())
    {
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCsePerson2())
      {
        // 06/23/99 M.L
        //              Change property of the following READ to generate
        //              SELECT ONLY
        if (!ReadCsePerson4())
        {
          ExitState = "CO0000_CHILD_NF";

          return;
        }
      }
      else
      {
        ExitState = "CO0000_AP_NF";

        return;
      }

      if (Lt(local.End.Date, entities.CurrCaseUnit.StartDate))
      {
        local.Use.Date = entities.CurrCaseUnit.StartDate;
      }
      else
      {
        local.Use.Date = local.End.Date;
      }

      try
      {
        UpdateCaseUnit1();

        // Discontinue any active Case Unit Function assignments for the Curr 
        // Case_Unit being deactivated. Ignore any occurrences that have a
        // discontinue date less than or equal to the current date.
        // 01/27/00 M.L Start
        // Change current date to end date of previous AR.
        foreach(var item1 in ReadCaseUnitFunctionAssignmt3())
        {
          // 01/27/00 M.L Start
          // Set discontinue date to end date of case role instead of
          // current date.
          try
          {
            UpdateCaseUnitFunctionAssignmt1();
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_NU_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_UNIT_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASE_UNIT_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // ------------------------------------------------------------
      // Read for an existing Case Unit that the NEW AR may have had
      // previously with the same AP and CH and reactivate those Case Units.
      // Create new Case Unit Function assignments.
      // ------------------------------------------------------------
      // 06/23/99 M.L
      //              Change property of the following READ to generate
      //              SELECT ONLY
      if (ReadCaseUnit3())
      {
        // 06/23/00 M.L Start
        // Updated override indicator from X to N.
        // Deleted Case Unit Function Assignment where discontinue
        // date is less than effective date.
        foreach(var item1 in ReadCaseUnitFunctionAssignmt2())
        {
          if (Lt(entities.CurrCaseUnitFunctionAssignmt.DiscontinueDate,
            entities.CurrCaseUnitFunctionAssignmt.EffectiveDate))
          {
            DeleteCaseUnitFunctionAssignmt1();
          }
          else
          {
            try
            {
              UpdateCaseUnitFunctionAssignmt2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_AE_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        // 06/23/00 M.L End
        // ------------------------------------------------------------
        // Reactivate the previous Case Unit that already exists.
        // ------------------------------------------------------------
        try
        {
          UpdateCaseUnit5();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_UNIT_NU_RB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_UNIT_PV_RB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // Discontinue any existing active Case Unit Function assignments for 
        // target
        // reactivated Case Unit.  Create new Case Unit Function assignments for
        // the
        // reactivated Case Unit - assign to OSP currently RSP assigned to Case.
        foreach(var item1 in ReadCaseUnitFunctionAssignmt6())
        {
          // 01/29/00 M.L Start
          if (Lt(local.Current.Date,
            entities.PrevCaseUnitFunctionAssignmt.EffectiveDate))
          {
            DeleteCaseUnitFunctionAssignmt4();
          }
          else
          {
            try
            {
              UpdateCaseUnitFunctionAssignmt5();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_NU_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          // 01/29/00 M.L End
        }

        for(local.Group.Index = 0; local.Group.Index < Local
          .GroupGroup.Capacity; ++local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          try
          {
            CreateCaseUnitFunctionAssignmt3();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_AE_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        local.Group.CheckIndex();
      }
      else
      {
        // ----------------------------------------------
        // 02/17/99 W.Campbell - Added the following
        // IF stmt in order to skip the remaining
        // work in this loop if the new_ar is the ch
        // so that a new case unit is not created
        // with the ch as the ar for itself.
        // ----------------------------------------------
        if (Equal(entities.Ch.Number, entities.NewArCsePerson.Number))
        {
          // 06/23/00 M.L Start
          // Updated override indicator from X to N.
          // Deleted Case Unit Function Assignment where discontinue
          // date is less than effective date.
          foreach(var item1 in ReadCaseUnitFunctionAssignmt2())
          {
            if (Lt(entities.CurrCaseUnitFunctionAssignmt.DiscontinueDate,
              entities.CurrCaseUnitFunctionAssignmt.EffectiveDate))
            {
              DeleteCaseUnitFunctionAssignmt1();
            }
            else
            {
              try
              {
                UpdateCaseUnitFunctionAssignmt2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          // 06/23/00 M.L End
          continue;
        }

        local.Mona.Count = 0;

        // ------------------------------------------------------------
        // Create new case unit for new AR with current AP and CH.
        // Create new case unit function assignments for new case unit.
        // ------------------------------------------------------------
        UseSpDtrCaseSrvcType();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.Hold.StartDate = import.NewArCaseRole.StartDate;
        UseSpCreateCaseUnit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // Obtain currency on the newly created Case Unit.
        // 06/23/99 M.L
        //              Change property of the following READ to generate
        //              SELECT ONLY
        if (!ReadCaseUnit2())
        {
          ExitState = "CASE_UNIT_NF_RB";

          return;
        }

        // ----------------------------------------------
        // 02/16/99 W.Campbell - Modified the following
        // IF stmt to remove the test for
        // OR local_ch_becomes_ar ief_supplied_flag = 'Y'.
        // The original IF was copied and disabled to save
        // if in case it is needed again.
        // ----------------------------------------------
        if (AsChar(local.ApBecomesAr.Flag) == 'Y')
        {
          UseSpDtrInitialCaseUnitState();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        else
        {
          try
          {
            UpdateCaseUnit4();
            local.Infrastructure.ReasonCode = "CAUARCHG";
            local.Infrastructure.CaseUnitNumber = entities.NewCaseUnit.CuNumber;
            local.Infrastructure.BusinessObjectCd = "CAU";
            local.Infrastructure.CsePersonNumber =
              entities.NewArCsePerson.Number;
            local.HoldCu.Text15 =
              NumberToString(entities.NewCaseUnit.CuNumber, 15);
            local.HoldCu.Text15 = Substring(local.HoldCu.Text15, 13, 3);
            local.Infrastructure.Detail = "Initial State for CU " + TrimEnd
              (local.HoldCu.Text15);
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " set to ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + entities
              .CurrCaseUnit.State;
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + ", replaces CU ";
            local.HoldCu.Text15 =
              NumberToString(entities.CurrCaseUnit.CuNumber, 15);
            local.HoldCu.Text15 = Substring(local.HoldCu.Text15, 13, 3);
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + TrimEnd
              (local.HoldCu.Text15);
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + " effc ";
            local.Infrastructure.Detail =
              (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
            UseSpCabCreateInfrastructure2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              return;
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_UNIT_AE_RB";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CASE_UNIT_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        for(local.Group.Index = 0; local.Group.Index < Local
          .GroupGroup.Capacity; ++local.Group.Index)
        {
          if (!local.Group.CheckSize())
          {
            break;
          }

          foreach(var item1 in ReadCaseUnitFunctionAssignmt1())
          {
            // 06/23/99 M.L
            //              Change property of the following READ to generate
            //              SELECT ONLY
            if (ReadOfficeServiceProvider3())
            {
              // 01/27/00  M.L Start
              if (ReadCaseAssignment2())
              {
                if (Lt(entities.CaseAssignment.EffectiveDate,
                  import.NewArCaseRole.StartDate))
                {
                  local.CaseUnitFunctionAssignmt.EffectiveDate =
                    import.NewArCaseRole.StartDate;
                }
                else
                {
                  local.CaseUnitFunctionAssignmt.EffectiveDate =
                    entities.CaseAssignment.EffectiveDate;
                }

                if (Lt(entities.CaseAssignment.DiscontinueDate, local.Max.Date))
                {
                  local.CaseUnitFunctionAssignmt.DiscontinueDate =
                    entities.CaseAssignment.DiscontinueDate;
                }
                else
                {
                  local.CaseUnitFunctionAssignmt.DiscontinueDate =
                    local.Max.Date;
                }
              }
              else
              {
                // 12/01/00 M.L Start
                if (Lt(entities.CurrCaseUnitFunctionAssignmt.EffectiveDate,
                  import.NewArCaseRole.StartDate))
                {
                  local.CaseUnitFunctionAssignmt.EffectiveDate =
                    import.NewArCaseRole.StartDate;
                }
                else
                {
                  local.CaseUnitFunctionAssignmt.EffectiveDate =
                    entities.CurrCaseUnitFunctionAssignmt.EffectiveDate;
                }

                local.CaseUnitFunctionAssignmt.DiscontinueDate = local.Max.Date;

                // 12/01/00 M.L End
              }

              // 01/27/00  M.L End
              try
              {
                CreateCaseUnitFunctionAssignmt1();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

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
              try
              {
                CreateCaseUnitFunctionAssignmt2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            // 01/27/00 M.L Start
            if (Lt(entities.CurrCaseUnitFunctionAssignmt.DiscontinueDate,
              entities.CurrCaseUnitFunctionAssignmt.EffectiveDate))
            {
              DeleteCaseUnitFunctionAssignmt1();
            }
            else
            {
              try
              {
                UpdateCaseUnitFunctionAssignmt2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "SP0000_CASE_UNIT_FNC_ASSGN_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            // 01/27/00 M.L End
          }
        }

        local.Group.CheckIndex();

        // ----------------------------------------------
        // 02/12/99 W.Campbell - Modified the following
        // IF stmt to remove the test for
        // OR local_ch_becomes_ar ief_supplied_flag = 'Y'.
        // The original IF was copied and disabled to save
        // if in case it is needed again.
        // ----------------------------------------------
        if (AsChar(local.ApBecomesAr.Flag) == 'Y')
        {
        }
        else
        {
          // The new incoming AR has never been the AP for this case.
          // For each of the active Case Units for this case being deactivated
          // in response to this AR change, duplicate the open Monitored 
          // Activities
          // and their associated Infrastructure records for the newly created
          // Case Units that specify the new AR and close the currently existing
          // Monitored Activity and any assignments that are open for it.
          foreach(var item1 in ReadMonitoredActivity2())
          {
            // 06/23/99 M.L
            //              Change property of the following READ to generate
            //              SELECT ONLY
            if (ReadInfrastructure1())
            {
              MoveInfrastructure2(entities.CurrArInfrastructure,
                local.NewInfrastructure);
              local.NewInfrastructure.ProcessStatus = "H";
              local.NewInfrastructure.CaseUnitNumber =
                entities.NewCaseUnit.CuNumber;
              local.NewInfrastructure.ReferenceDate = local.Current.Date;
              local.NewInfrastructure.UserId = "ROLE";
              local.HoldCu.Text15 =
                NumberToString(entities.CurrCaseUnit.CuNumber, 15);
              local.HoldCu.Text15 = Substring(local.HoldCu.Text15, 13, 3);
              local.NewInfrastructure.Detail =
                "Dup hist rec - mon acts for CU " + TrimEnd
                (local.HoldCu.Text15);
              local.NewInfrastructure.Detail =
                (local.NewInfrastructure.Detail ?? "") + " trnsfrd to CU ";
              local.HoldCu.Text15 =
                NumberToString(entities.NewCaseUnit.CuNumber, 15);
              local.HoldCu.Text15 = Substring(local.HoldCu.Text15, 13, 3);
              local.NewInfrastructure.Detail =
                (local.NewInfrastructure.Detail ?? "") + TrimEnd
                (local.HoldCu.Text15);
              local.NewInfrastructure.Detail =
                (local.NewInfrastructure.Detail ?? "") + " on ";
              local.NewInfrastructure.Detail =
                (local.NewInfrastructure.Detail ?? "") + local
                .TextWorkArea.Text8;
              UseSpCabCreateInfrastructure1();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                return;
              }

              // 06/23/99 M.L
              //              Change property of the following READ to generate
              //              SELECT ONLY
              if (!ReadInfrastructure2())
              {
                ExitState = "INFRASTRUCTURE_NF";

                return;
              }

              MoveMonitoredActivity2(entities.CurrArMonitoredActivity,
                local.NewMonitoredActivity);
              local.NewMonitoredActivity.SystemGeneratedIdentifier = 0;
              local.NewMonitoredActivity.ClosureReasonCode = "";
              local.NewMonitoredActivity.LastUpdatedBy = "";
              local.NewMonitoredActivity.LastUpdatedTimestamp =
                local.Initialized.Timestamp;
              local.NewMonitoredActivity.CaseUnitClosedInd = "";
              local.NewMonitoredActivity.ClosureDate = local.Max.Date;

              // --------------------------------------------
              // 04/28/00 W.Campbell - Changed the created_by
              // in the following set stmt from USER_ID to
              // the created by user id for the curr_ar
              // Monitored Activity which is being outdated.
              // Disabled the previous set stmt.
              // Work done on PR# 88664.
              // --------------------------------------------
              local.NewMonitoredActivity.CreatedBy =
                entities.CurrArMonitoredActivity.CreatedBy;
              local.NewMonitoredActivity.CreatedTimestamp = Now();
              UseCreateMonitoredActivity();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ++local.Mona.Count;
                local.NewMonitoredActivity.SystemGeneratedIdentifier =
                  export.MonitoredActivity.SystemGeneratedIdentifier;
              }
              else
              {
                return;
              }

              try
              {
                UpdateMonitoredActivity2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "MONITORED_ACTIVITY_AE_WITH_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "MONITORED_ACTIVITY_PV_RB";

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
              ExitState = "INFRASTRUCTURE_NF";

              return;
            }

            foreach(var item2 in ReadMonitoredActivityAssignment2())
            {
              // 06/23/99 M.L
              //              Change property of the following READ to generate
              //              SELECT ONLY
              if (ReadOfficeServiceProvider2())
              {
                // 06/23/99 M.L
                //              Change property of the following READ to 
                // generate
                //              SELECT ONLY
                if (ReadServiceProvider())
                {
                  // 06/23/99 M.L
                  //              Change property of the following READ to 
                  // generate
                  //              SELECT ONLY
                  if (!ReadOffice())
                  {
                    ExitState = "OFFICE_NF";

                    return;
                  }
                }
                else
                {
                  ExitState = "SERVICE_PROVIDER_NF";

                  return;
                }

                local.NewMonitoredActivityAssignment.Assign(
                  entities.CurrArMonitoredActivityAssignment);
                UseSpCabCreateMonActAssignment();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
                else
                {
                  return;
                }

                try
                {
                  UpdateMonitoredActivityAssignment2();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_AE";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "MONITORED_ACTIVITY_ASSIGNMEN_PV";

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
                ExitState = "OFFICE_SERVICE_PROVIDER_NF";

                return;
              }
            }

            if (local.Mona.Count == 1)
            {
              // Create the Case Unit specific "Monitored Activities 
              // transferred" event Infrastructure occurrence.
              local.Infrastructure.ReasonCode = "CHNGMONACTV";
              local.Infrastructure.CaseUnitNumber =
                entities.NewCaseUnit.CuNumber;
              local.Infrastructure.BusinessObjectCd = "CAU";
              local.Infrastructure.CsePersonNumber = entities.Ap.Number;
              local.HoldCu.Text15 =
                NumberToString(entities.CurrCaseUnit.CuNumber, 15);
              local.HoldCu.Text15 = Substring(local.HoldCu.Text15, 13, 3);
              local.Infrastructure.Detail = "Case " + entities.Case1.Number;
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " mon actvs for CU ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + TrimEnd
                (local.HoldCu.Text15);
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " trnsfrd to CU ";
              local.HoldCu.Text15 =
                NumberToString(entities.NewCaseUnit.CuNumber, 15);
              local.HoldCu.Text15 = Substring(local.HoldCu.Text15, 13, 3);
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + TrimEnd
                (local.HoldCu.Text15);
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + " on ";
              local.Infrastructure.Detail =
                (local.Infrastructure.Detail ?? "") + local.TextWorkArea.Text8;
              UseSpCabCreateInfrastructure2();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                return;
              }
            }
          }
        }
      }
    }

    // Create the generic Case level "AR Change" event Infrastructure 
    // occurrence.
    local.Infrastructure.ReasonCode = "CASARCHG";
    local.Infrastructure.CaseUnitNumber = 0;
    local.Infrastructure.BusinessObjectCd = "CAS";
    local.Infrastructure.CsePersonNumber = entities.NewArCsePerson.Number;
    local.Infrastructure.Detail = "CSE Person ";
    local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
      .NewArCsePerson.Number;
    local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + " has become AR for Case ";
      
    local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + entities
      .Case1.Number + " on ";
    local.Infrastructure.Detail = (local.Infrastructure.Detail ?? "") + local
      .TextWorkArea.Text8 + ".";
    UseSpCabCreateInfrastructure2();
  }

  private static void MoveCaseUnit1(CaseUnit source, CaseUnit target)
  {
    target.CuNumber = source.CuNumber;
    target.State = source.State;
    target.StartDate = source.StartDate;
  }

  private static void MoveCaseUnit2(CaseUnit source, CaseUnit target)
  {
    target.State = source.State;
    target.StartDate = source.StartDate;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveMonitoredActivity1(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Name = source.Name;
    target.TypeCode = source.TypeCode;
  }

  private static void MoveMonitoredActivity2(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.Name = source.Name;
    target.ActivityControlNumber = source.ActivityControlNumber;
    target.TypeCode = source.TypeCode;
    target.FedNonComplianceDate = source.FedNonComplianceDate;
    target.FedNearNonComplDate = source.FedNearNonComplDate;
    target.OtherNonComplianceDate = source.OtherNonComplianceDate;
    target.OtherNearNonComplDate = source.OtherNearNonComplDate;
    target.StartDate = source.StartDate;
    target.ClosureDate = source.ClosureDate;
    target.ClosureReasonCode = source.ClosureReasonCode;
    target.CaseUnitClosedInd = source.CaseUnitClosedInd;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveMonitoredActivityAssignment(
    MonitoredActivityAssignment source, MonitoredActivityAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.ResponsibilityCode = source.ResponsibilityCode;
    target.EffectiveDate = source.EffectiveDate;
    target.OverrideInd = source.OverrideInd;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Use.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseCreateMonitoredActivity()
  {
    var useImport = new CreateMonitoredActivity.Import();
    var useExport = new CreateMonitoredActivity.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      entities.NewInfrastructure.SystemGeneratedIdentifier;
    useImport.MonitoredActivity.Assign(local.NewMonitoredActivity);

    Call(CreateMonitoredActivity.Execute, useImport, useExport);

    export.MonitoredActivity.SystemGeneratedIdentifier =
      useExport.MonitoredActivity.SystemGeneratedIdentifier;
  }

  private void UseSpCabCreateInfrastructure1()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure1(local.NewInfrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    export.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
  }

  private void UseSpCabCreateInfrastructure2()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCabCreateMonActAssignment()
  {
    var useImport = new SpCabCreateMonActAssignment.Import();
    var useExport = new SpCabCreateMonActAssignment.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.CurrArMona,
      useImport.OfficeServiceProvider);
    MoveMonitoredActivityAssignment(local.NewMonitoredActivityAssignment,
      useImport.MonitoredActivityAssignment);
    MoveMonitoredActivity1(local.NewMonitoredActivity,
      useImport.MonitoredActivity);

    Call(SpCabCreateMonActAssignment.Execute, useImport, useExport);
  }

  private void UseSpCreateCaseUnit()
  {
    var useImport = new SpCreateCaseUnit.Import();
    var useExport = new SpCreateCaseUnit.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Ar.Number = entities.NewArCsePerson.Number;
    useImport.Ap.Number = entities.Ap.Number;
    useImport.Child.Number = entities.Ch.Number;
    MoveCaseUnit2(local.Hold, useImport.CaseUnit);

    Call(SpCreateCaseUnit.Execute, useImport, useExport);

    local.Hold.Assign(useExport.CaseUnit);
  }

  private void UseSpDtrCaseSrvcType()
  {
    var useImport = new SpDtrCaseSrvcType.Import();
    var useExport = new SpDtrCaseSrvcType.Export();

    useImport.Case1.Number = entities.Case1.Number;

    Call(SpDtrCaseSrvcType.Execute, useImport, useExport);

    local.Hold.State = useExport.CaseUnit.State;
  }

  private void UseSpDtrInitialCaseUnitState()
  {
    var useImport = new SpDtrInitialCaseUnitState.Import();
    var useExport = new SpDtrInitialCaseUnitState.Export();

    MoveCaseUnit1(local.Hold, useImport.CaseUnit);
    useImport.CsePerson.Number = entities.Ap.Number;
    useImport.Case1.Number = entities.Case1.Number;

    Call(SpDtrInitialCaseUnitState.Execute, useImport, useExport);
  }

  private void CreateCaseRole()
  {
    var casNumber = entities.Case1.Number;
    var cspNumber = entities.NewArCsePerson.Number;
    var type1 = "AR";
    var identifier = local.LastCaseRole.Identifier;
    var startDate = import.NewArCaseRole.StartDate;
    var endDate = local.Max.Date;
    var onSsInd = entities.PrevAr.OnSsInd;
    var healthInsuranceIndicator = entities.PrevAr.HealthInsuranceIndicator;
    var medicalSupportIndicator = entities.PrevAr.MedicalSupportIndicator;
    var contactFirstName = entities.PrevAr.ContactFirstName;
    var contactMiddleInitial = entities.PrevAr.ContactMiddleInitial;
    var contactPhone = entities.PrevAr.ContactPhone;
    var contactLastName = entities.PrevAr.ContactLastName;
    var childCareExpenses = entities.PrevAr.ChildCareExpenses;
    var assignmentDate = entities.PrevAr.AssignmentDate;
    var assignmentTerminationCode = entities.PrevAr.AssignmentTerminationCode;
    var assignmentOfRights = entities.PrevAr.AssignmentOfRights;
    var assignmentTerminatedDt = entities.PrevAr.AssignmentTerminatedDt;
    var param = 0M;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var arChgProcReqInd = entities.PrevAr.ArChgProcReqInd;
    var arChgProcessedDate = entities.PrevAr.ArChgProcessedDate;
    var arInvalidInd = entities.PrevAr.ArInvalidInd;
    var note = entities.PrevAr.Note;

    CheckValid<CaseRole>("Type1", type1);
    entities.NewArCaseRole.Populated = false;
    Update("CreateCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetInt32(command, "caseRoleId", identifier);
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "onSsInd", onSsInd);
        db.SetNullableString(command, "healthInsInd", healthInsuranceIndicator);
        db.
          SetNullableString(command, "medicalSuppInd", medicalSupportIndicator);
          
        db.SetNullableString(command, "mothersFirstNm", "");
        db.SetNullableString(command, "mothersMidInit", "");
        db.SetNullableString(command, "fathersLastName", "");
        db.SetNullableString(command, "parentType", "");
        db.SetNullableDate(command, "notifiedDate", default(DateTime));
        db.SetNullableInt32(
          command, "numberOfChildren", GetImplicitValue<CaseRole,
          int?>("NumberOfChildren").GetValueOrDefault());
        db.SetNullableString(
          command, "livingWithArInd", GetImplicitValue<CaseRole,
          string>("LivingWithArIndicator"));
        db.SetNullableString(command, "contactFirstName", contactFirstName);
        db.SetNullableString(command, "contactMidInit", contactMiddleInitial);
        db.SetNullableString(command, "contactPhone", contactPhone);
        db.SetNullableString(command, "contactLastName", contactLastName);
        db.SetNullableDecimal(command, "childCareExpense", childCareExpenses);
        db.SetNullableDate(command, "assignmentDate", assignmentDate);
        db.SetNullableString(
          command, "assignmentTermCd", assignmentTerminationCode);
        db.SetNullableString(command, "assignOfRights", assignmentOfRights);
        db.SetNullableDate(command, "assignmentTermDt", assignmentTerminatedDt);
        db.SetNullableDecimal(command, "priorMedicalSupp", param);
        db.SetNullableDecimal(command, "fcCostOfCare", param);
        db.SetNullableString(command, "fcIvECaseNo", "");
        db.SetNullableString(command, "fcPlacementName", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "arChgPrcReqInd", arChgProcReqInd);
        db.SetNullableDate(command, "arChgProcDt", arChgProcessedDate);
        db.SetNullableString(command, "arInvalidInd", arInvalidInd);
        db.SetNullableString(command, "note", note);
      });

    entities.NewArCaseRole.CasNumber = casNumber;
    entities.NewArCaseRole.CspNumber = cspNumber;
    entities.NewArCaseRole.Type1 = type1;
    entities.NewArCaseRole.Identifier = identifier;
    entities.NewArCaseRole.StartDate = startDate;
    entities.NewArCaseRole.EndDate = endDate;
    entities.NewArCaseRole.OnSsInd = onSsInd;
    entities.NewArCaseRole.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.NewArCaseRole.MedicalSupportIndicator = medicalSupportIndicator;
    entities.NewArCaseRole.ContactFirstName = contactFirstName;
    entities.NewArCaseRole.ContactMiddleInitial = contactMiddleInitial;
    entities.NewArCaseRole.ContactPhone = contactPhone;
    entities.NewArCaseRole.ContactLastName = contactLastName;
    entities.NewArCaseRole.ChildCareExpenses = childCareExpenses;
    entities.NewArCaseRole.AssignmentDate = assignmentDate;
    entities.NewArCaseRole.AssignmentTerminationCode =
      assignmentTerminationCode;
    entities.NewArCaseRole.AssignmentOfRights = assignmentOfRights;
    entities.NewArCaseRole.AssignmentTerminatedDt = assignmentTerminatedDt;
    entities.NewArCaseRole.LastUpdatedTimestamp = null;
    entities.NewArCaseRole.LastUpdatedBy = "";
    entities.NewArCaseRole.CreatedTimestamp = createdTimestamp;
    entities.NewArCaseRole.CreatedBy = createdBy;
    entities.NewArCaseRole.ArChgProcReqInd = arChgProcReqInd;
    entities.NewArCaseRole.ArChgProcessedDate = arChgProcessedDate;
    entities.NewArCaseRole.ArInvalidInd = arInvalidInd;
    entities.NewArCaseRole.Note = note;
    entities.NewArCaseRole.Populated = true;
  }

  private void CreateCaseUnitFunctionAssignmt1()
  {
    System.Diagnostics.Debug.Assert(entities.CurrCufa.Populated);
    System.Diagnostics.Debug.Assert(entities.NewCaseUnit.Populated);

    var reasonCode = entities.CurrCaseUnitFunctionAssignmt.ReasonCode;
    var overrideInd = "N";
    var effectiveDate = local.CaseUnitFunctionAssignmt.EffectiveDate;
    var discontinueDate = local.CaseUnitFunctionAssignmt.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.CurrCufa.SpdGeneratedId;
    var offId = entities.CurrCufa.OffGeneratedId;
    var ospCode = entities.CurrCufa.RoleCode;
    var ospDate = entities.CurrCufa.EffectiveDate;
    var csuNo = entities.NewCaseUnit.CuNumber;
    var casNo = entities.NewCaseUnit.CasNo;
    var function = local.Group.Item.Det.FuncText3;

    entities.NewCaseUnitFunctionAssignmt.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt1",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "csuNo", csuNo);
        db.SetString(command, "casNo", casNo);
        db.SetString(command, "function", function);
      });

    entities.NewCaseUnitFunctionAssignmt.ReasonCode = reasonCode;
    entities.NewCaseUnitFunctionAssignmt.OverrideInd = overrideInd;
    entities.NewCaseUnitFunctionAssignmt.EffectiveDate = effectiveDate;
    entities.NewCaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.NewCaseUnitFunctionAssignmt.CreatedBy = createdBy;
    entities.NewCaseUnitFunctionAssignmt.CreatedTimestamp = createdTimestamp;
    entities.NewCaseUnitFunctionAssignmt.LastUpdatedBy = "";
    entities.NewCaseUnitFunctionAssignmt.LastUpdatedTimestamp = null;
    entities.NewCaseUnitFunctionAssignmt.SpdId = spdId;
    entities.NewCaseUnitFunctionAssignmt.OffId = offId;
    entities.NewCaseUnitFunctionAssignmt.OspCode = ospCode;
    entities.NewCaseUnitFunctionAssignmt.OspDate = ospDate;
    entities.NewCaseUnitFunctionAssignmt.CsuNo = csuNo;
    entities.NewCaseUnitFunctionAssignmt.CasNo = casNo;
    entities.NewCaseUnitFunctionAssignmt.Function = function;
    entities.NewCaseUnitFunctionAssignmt.Populated = true;
  }

  private void CreateCaseUnitFunctionAssignmt2()
  {
    System.Diagnostics.Debug.Assert(entities.NewCaseUnit.Populated);
    System.Diagnostics.Debug.Assert(entities.Case2.Populated);

    var reasonCode = entities.CurrCaseUnitFunctionAssignmt.ReasonCode;
    var overrideInd = "N";
    var effectiveDate = import.NewArCaseRole.StartDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.Case2.SpdGeneratedId;
    var offId = entities.Case2.OffGeneratedId;
    var ospCode = entities.Case2.RoleCode;
    var ospDate = entities.Case2.EffectiveDate;
    var csuNo = entities.NewCaseUnit.CuNumber;
    var casNo = entities.NewCaseUnit.CasNo;
    var function = local.Group.Item.Det.FuncText3;

    entities.NewCaseUnitFunctionAssignmt.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt2",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "csuNo", csuNo);
        db.SetString(command, "casNo", casNo);
        db.SetString(command, "function", function);
      });

    entities.NewCaseUnitFunctionAssignmt.ReasonCode = reasonCode;
    entities.NewCaseUnitFunctionAssignmt.OverrideInd = overrideInd;
    entities.NewCaseUnitFunctionAssignmt.EffectiveDate = effectiveDate;
    entities.NewCaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.NewCaseUnitFunctionAssignmt.CreatedBy = createdBy;
    entities.NewCaseUnitFunctionAssignmt.CreatedTimestamp = createdTimestamp;
    entities.NewCaseUnitFunctionAssignmt.LastUpdatedBy = "";
    entities.NewCaseUnitFunctionAssignmt.LastUpdatedTimestamp = null;
    entities.NewCaseUnitFunctionAssignmt.SpdId = spdId;
    entities.NewCaseUnitFunctionAssignmt.OffId = offId;
    entities.NewCaseUnitFunctionAssignmt.OspCode = ospCode;
    entities.NewCaseUnitFunctionAssignmt.OspDate = ospDate;
    entities.NewCaseUnitFunctionAssignmt.CsuNo = csuNo;
    entities.NewCaseUnitFunctionAssignmt.CasNo = casNo;
    entities.NewCaseUnitFunctionAssignmt.Function = function;
    entities.NewCaseUnitFunctionAssignmt.Populated = true;
  }

  private void CreateCaseUnitFunctionAssignmt3()
  {
    System.Diagnostics.Debug.Assert(entities.PrevCaseUnit.Populated);
    System.Diagnostics.Debug.Assert(entities.Case2.Populated);

    var reasonCode = "RSP";
    var overrideInd = "N";
    var effectiveDate = local.Current.Date;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var spdId = entities.Case2.SpdGeneratedId;
    var offId = entities.Case2.OffGeneratedId;
    var ospCode = entities.Case2.RoleCode;
    var ospDate = entities.Case2.EffectiveDate;
    var csuNo = entities.PrevCaseUnit.CuNumber;
    var casNo = entities.PrevCaseUnit.CasNo;
    var function = local.Group.Item.Det.FuncText3;

    entities.NewCaseUnitFunctionAssignmt.Populated = false;
    Update("CreateCaseUnitFunctionAssignmt3",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetInt32(command, "csuNo", csuNo);
        db.SetString(command, "casNo", casNo);
        db.SetString(command, "function", function);
      });

    entities.NewCaseUnitFunctionAssignmt.ReasonCode = reasonCode;
    entities.NewCaseUnitFunctionAssignmt.OverrideInd = overrideInd;
    entities.NewCaseUnitFunctionAssignmt.EffectiveDate = effectiveDate;
    entities.NewCaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.NewCaseUnitFunctionAssignmt.CreatedBy = createdBy;
    entities.NewCaseUnitFunctionAssignmt.CreatedTimestamp = createdTimestamp;
    entities.NewCaseUnitFunctionAssignmt.LastUpdatedBy = "";
    entities.NewCaseUnitFunctionAssignmt.LastUpdatedTimestamp = null;
    entities.NewCaseUnitFunctionAssignmt.SpdId = spdId;
    entities.NewCaseUnitFunctionAssignmt.OffId = offId;
    entities.NewCaseUnitFunctionAssignmt.OspCode = ospCode;
    entities.NewCaseUnitFunctionAssignmt.OspDate = ospDate;
    entities.NewCaseUnitFunctionAssignmt.CsuNo = csuNo;
    entities.NewCaseUnitFunctionAssignmt.CasNo = casNo;
    entities.NewCaseUnitFunctionAssignmt.Function = function;
    entities.NewCaseUnitFunctionAssignmt.Populated = true;
  }

  private void DeleteCaseUnitFunctionAssignmt1()
  {
    Update("DeleteCaseUnitFunctionAssignmt1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CurrCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.CurrCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.CurrCaseUnitFunctionAssignmt.CasNo);
      });
  }

  private void DeleteCaseUnitFunctionAssignmt2()
  {
    Update("DeleteCaseUnitFunctionAssignmt2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrApCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrApCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrApCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CurrApCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrApCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.CurrApCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.CurrApCaseUnitFunctionAssignmt.CasNo);
      });
  }

  private void DeleteCaseUnitFunctionAssignmt3()
  {
    Update("DeleteCaseUnitFunctionAssignmt3",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrChCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrChCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrChCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CurrChCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrChCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.CurrChCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.CurrChCaseUnitFunctionAssignmt.CasNo);
      });
  }

  private void DeleteCaseUnitFunctionAssignmt4()
  {
    Update("DeleteCaseUnitFunctionAssignmt4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PrevCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.PrevCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.PrevCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.PrevCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.PrevCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.PrevCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.PrevCaseUnitFunctionAssignmt.CasNo);
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

  private bool ReadCaseAssignment1()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseAssignment2()
  {
    System.Diagnostics.Debug.Assert(entities.CurrCufa.Populated);
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "ospDate",
          entities.CurrCufa.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "ospCode", entities.CurrCufa.RoleCode);
        db.SetInt32(command, "offId", entities.CurrCufa.OffGeneratedId);
        db.SetInt32(command, "spdId", entities.CurrCufa.SpdGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.End.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.PrevAr.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.PrevAr.CasNumber = db.GetString(reader, 0);
        entities.PrevAr.CspNumber = db.GetString(reader, 1);
        entities.PrevAr.Type1 = db.GetString(reader, 2);
        entities.PrevAr.Identifier = db.GetInt32(reader, 3);
        entities.PrevAr.StartDate = db.GetNullableDate(reader, 4);
        entities.PrevAr.EndDate = db.GetNullableDate(reader, 5);
        entities.PrevAr.OnSsInd = db.GetNullableString(reader, 6);
        entities.PrevAr.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.PrevAr.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.PrevAr.ContactFirstName = db.GetNullableString(reader, 9);
        entities.PrevAr.ContactMiddleInitial = db.GetNullableString(reader, 10);
        entities.PrevAr.ContactPhone = db.GetNullableString(reader, 11);
        entities.PrevAr.ContactLastName = db.GetNullableString(reader, 12);
        entities.PrevAr.ChildCareExpenses = db.GetNullableDecimal(reader, 13);
        entities.PrevAr.AssignmentDate = db.GetNullableDate(reader, 14);
        entities.PrevAr.AssignmentTerminationCode =
          db.GetNullableString(reader, 15);
        entities.PrevAr.AssignmentOfRights = db.GetNullableString(reader, 16);
        entities.PrevAr.AssignmentTerminatedDt = db.GetNullableDate(reader, 17);
        entities.PrevAr.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.PrevAr.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.PrevAr.CreatedTimestamp = db.GetDateTime(reader, 20);
        entities.PrevAr.CreatedBy = db.GetString(reader, 21);
        entities.PrevAr.ArChgProcReqInd = db.GetNullableString(reader, 22);
        entities.PrevAr.ArChgProcessedDate = db.GetNullableDate(reader, 23);
        entities.PrevAr.ArInvalidInd = db.GetNullableString(reader, 24);
        entities.PrevAr.Note = db.GetNullableString(reader, 25);
        entities.PrevAr.Populated = true;
        CheckValid<CaseRole>("Type1", entities.PrevAr.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.Related.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Related.CasNumber = db.GetString(reader, 0);
        entities.Related.CspNumber = db.GetString(reader, 1);
        entities.Related.Type1 = db.GetString(reader, 2);
        entities.Related.Identifier = db.GetInt32(reader, 3);
        entities.Related.StartDate = db.GetNullableDate(reader, 4);
        entities.Related.EndDate = db.GetNullableDate(reader, 5);
        entities.Related.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.Related.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Related.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Related.Type1);
      });
  }

  private bool ReadCaseRole3()
  {
    entities.CurrApCaseRole.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.NewArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CurrApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.CurrApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.CurrApCaseRole.Type1 = db.GetString(reader, 2);
        entities.CurrApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CurrApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CurrApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CurrApCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrApCaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CurrApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CurrApCaseRole.Type1);
      });
  }

  private bool ReadCaseRole4()
  {
    entities.CurrChCaseRole.Populated = false;

    return Read("ReadCaseRole4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.NewArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CurrChCaseRole.CasNumber = db.GetString(reader, 0);
        entities.CurrChCaseRole.CspNumber = db.GetString(reader, 1);
        entities.CurrChCaseRole.Type1 = db.GetString(reader, 2);
        entities.CurrChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CurrChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CurrChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CurrChCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrChCaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CurrChCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CurrChCaseRole.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.CurrArCsePerson.Populated = false;
    entities.CurrArCaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          import.NewArCaseRole.StartDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.CurrArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.CurrArCsePerson.Number = db.GetString(reader, 1);
        entities.CurrArCaseRole.Type1 = db.GetString(reader, 2);
        entities.CurrArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CurrArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CurrArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CurrArCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrArCaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CurrArCaseRole.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.CurrArCaseRole.CreatedBy = db.GetString(reader, 9);
        entities.CurrArCaseRole.ArChgProcReqInd =
          db.GetNullableString(reader, 10);
        entities.CurrArCaseRole.ArChgProcessedDate =
          db.GetNullableDate(reader, 11);
        entities.CurrArCaseRole.ArInvalidInd = db.GetNullableString(reader, 12);
        entities.CurrArCsePerson.Populated = true;
        entities.CurrArCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CurrArCaseRole.Type1);
      });
  }

  private bool ReadCaseUnit1()
  {
    entities.CurrChCaseUnit.Populated = false;

    return Read("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoChild", entities.Ch.Number);
      },
      (db, reader) =>
      {
        entities.CurrChCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CurrChCaseUnit.State = db.GetString(reader, 1);
        entities.CurrChCaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CurrChCaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CurrChCaseUnit.ClosureReasonCode =
          db.GetNullableString(reader, 4);
        entities.CurrChCaseUnit.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.CurrChCaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrChCaseUnit.CasNo = db.GetString(reader, 7);
        entities.CurrChCaseUnit.CspNoChild = db.GetNullableString(reader, 8);
        entities.CurrChCaseUnit.Populated = true;
      });
  }

  private bool ReadCaseUnit2()
  {
    entities.NewCaseUnit.Populated = false;

    return Read("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetInt32(command, "cuNumber", local.Hold.CuNumber);
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.NewCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.NewCaseUnit.State = db.GetString(reader, 1);
        entities.NewCaseUnit.StartDate = db.GetDate(reader, 2);
        entities.NewCaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.NewCaseUnit.ClosureReasonCode =
          db.GetNullableString(reader, 4);
        entities.NewCaseUnit.CreatedBy = db.GetString(reader, 5);
        entities.NewCaseUnit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.NewCaseUnit.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.NewCaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.NewCaseUnit.CasNo = db.GetString(reader, 9);
        entities.NewCaseUnit.Populated = true;
      });
  }

  private bool ReadCaseUnit3()
  {
    entities.PrevCaseUnit.Populated = false;

    return Read("ReadCaseUnit3",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetNullableString(command, "cspNoAr", entities.NewArCsePerson.Number);
          
        db.SetNullableString(command, "cspNoAp", entities.Ap.Number);
        db.SetNullableString(command, "cspNoChild", entities.Ch.Number);
      },
      (db, reader) =>
      {
        entities.PrevCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.PrevCaseUnit.State = db.GetString(reader, 1);
        entities.PrevCaseUnit.StartDate = db.GetDate(reader, 2);
        entities.PrevCaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.PrevCaseUnit.ClosureReasonCode =
          db.GetNullableString(reader, 4);
        entities.PrevCaseUnit.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.PrevCaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.PrevCaseUnit.CasNo = db.GetString(reader, 7);
        entities.PrevCaseUnit.CspNoAr = db.GetNullableString(reader, 8);
        entities.PrevCaseUnit.CspNoAp = db.GetNullableString(reader, 9);
        entities.PrevCaseUnit.CspNoChild = db.GetNullableString(reader, 10);
        entities.PrevCaseUnit.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit4()
  {
    entities.CurrCaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit4",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetNullableString(command, "cspNoAr", entities.CurrArCsePerson.Number);
          
        db.SetNullableDate(
          command, "closureDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CurrCaseUnit.State = db.GetString(reader, 1);
        entities.CurrCaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CurrCaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CurrCaseUnit.ClosureReasonCode =
          db.GetNullableString(reader, 4);
        entities.CurrCaseUnit.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.CurrCaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrCaseUnit.CasNo = db.GetString(reader, 7);
        entities.CurrCaseUnit.CspNoAr = db.GetNullableString(reader, 8);
        entities.CurrCaseUnit.CspNoAp = db.GetNullableString(reader, 9);
        entities.CurrCaseUnit.CspNoChild = db.GetNullableString(reader, 10);
        entities.CurrCaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit5()
  {
    entities.CurrApCaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit5",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAp", entities.Ap.Number);
      },
      (db, reader) =>
      {
        entities.CurrApCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CurrApCaseUnit.State = db.GetString(reader, 1);
        entities.CurrApCaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CurrApCaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CurrApCaseUnit.ClosureReasonCode =
          db.GetNullableString(reader, 4);
        entities.CurrApCaseUnit.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.CurrApCaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrApCaseUnit.CasNo = db.GetString(reader, 7);
        entities.CurrApCaseUnit.CspNoAp = db.GetNullableString(reader, 8);
        entities.CurrApCaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt1()
  {
    System.Diagnostics.Debug.Assert(entities.CurrCaseUnit.Populated);
    entities.CurrCaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CurrCaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CurrCaseUnit.CuNumber);
        db.SetString(command, "function", local.Group.Item.Det.FuncText3);
      },
      (db, reader) =>
      {
        entities.CurrCaseUnitFunctionAssignmt.ReasonCode =
          db.GetString(reader, 0);
        entities.CurrCaseUnitFunctionAssignmt.OverrideInd =
          db.GetString(reader, 1);
        entities.CurrCaseUnitFunctionAssignmt.EffectiveDate =
          db.GetDate(reader, 2);
        entities.CurrCaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CurrCaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CurrCaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CurrCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrCaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 7);
        entities.CurrCaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 8);
        entities.CurrCaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 9);
        entities.CurrCaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 10);
        entities.CurrCaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 11);
        entities.CurrCaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 12);
        entities.CurrCaseUnitFunctionAssignmt.Function =
          db.GetString(reader, 13);
        entities.CurrCaseUnitFunctionAssignmt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt2()
  {
    System.Diagnostics.Debug.Assert(entities.CurrCaseUnit.Populated);
    entities.CurrCaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CurrCaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CurrCaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.CurrCaseUnitFunctionAssignmt.ReasonCode =
          db.GetString(reader, 0);
        entities.CurrCaseUnitFunctionAssignmt.OverrideInd =
          db.GetString(reader, 1);
        entities.CurrCaseUnitFunctionAssignmt.EffectiveDate =
          db.GetDate(reader, 2);
        entities.CurrCaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CurrCaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CurrCaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CurrCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrCaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 7);
        entities.CurrCaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 8);
        entities.CurrCaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 9);
        entities.CurrCaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 10);
        entities.CurrCaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 11);
        entities.CurrCaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 12);
        entities.CurrCaseUnitFunctionAssignmt.Function =
          db.GetString(reader, 13);
        entities.CurrCaseUnitFunctionAssignmt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt3()
  {
    System.Diagnostics.Debug.Assert(entities.CurrCaseUnit.Populated);
    entities.CurrCaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt3",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CurrCaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CurrCaseUnit.CuNumber);
        db.SetNullableDate(
          command, "discontinueDate", local.End.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrCaseUnitFunctionAssignmt.ReasonCode =
          db.GetString(reader, 0);
        entities.CurrCaseUnitFunctionAssignmt.OverrideInd =
          db.GetString(reader, 1);
        entities.CurrCaseUnitFunctionAssignmt.EffectiveDate =
          db.GetDate(reader, 2);
        entities.CurrCaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CurrCaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CurrCaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CurrCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrCaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 7);
        entities.CurrCaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 8);
        entities.CurrCaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 9);
        entities.CurrCaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 10);
        entities.CurrCaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 11);
        entities.CurrCaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 12);
        entities.CurrCaseUnitFunctionAssignmt.Function =
          db.GetString(reader, 13);
        entities.CurrCaseUnitFunctionAssignmt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt4()
  {
    System.Diagnostics.Debug.Assert(entities.CurrApCaseUnit.Populated);
    entities.CurrApCaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt4",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CurrApCaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CurrApCaseUnit.CuNumber);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrApCaseUnitFunctionAssignmt.ReasonCode =
          db.GetString(reader, 0);
        entities.CurrApCaseUnitFunctionAssignmt.OverrideInd =
          db.GetString(reader, 1);
        entities.CurrApCaseUnitFunctionAssignmt.EffectiveDate =
          db.GetDate(reader, 2);
        entities.CurrApCaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CurrApCaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CurrApCaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CurrApCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrApCaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 7);
        entities.CurrApCaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 8);
        entities.CurrApCaseUnitFunctionAssignmt.OspCode =
          db.GetString(reader, 9);
        entities.CurrApCaseUnitFunctionAssignmt.OspDate =
          db.GetDate(reader, 10);
        entities.CurrApCaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 11);
        entities.CurrApCaseUnitFunctionAssignmt.CasNo =
          db.GetString(reader, 12);
        entities.CurrApCaseUnitFunctionAssignmt.Function =
          db.GetString(reader, 13);
        entities.CurrApCaseUnitFunctionAssignmt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt5()
  {
    System.Diagnostics.Debug.Assert(entities.CurrChCaseUnit.Populated);
    entities.CurrChCaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt5",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.CurrChCaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.CurrChCaseUnit.CuNumber);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrChCaseUnitFunctionAssignmt.ReasonCode =
          db.GetString(reader, 0);
        entities.CurrChCaseUnitFunctionAssignmt.OverrideInd =
          db.GetString(reader, 1);
        entities.CurrChCaseUnitFunctionAssignmt.EffectiveDate =
          db.GetDate(reader, 2);
        entities.CurrChCaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CurrChCaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CurrChCaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CurrChCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrChCaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 7);
        entities.CurrChCaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 8);
        entities.CurrChCaseUnitFunctionAssignmt.OspCode =
          db.GetString(reader, 9);
        entities.CurrChCaseUnitFunctionAssignmt.OspDate =
          db.GetDate(reader, 10);
        entities.CurrChCaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 11);
        entities.CurrChCaseUnitFunctionAssignmt.CasNo =
          db.GetString(reader, 12);
        entities.CurrChCaseUnitFunctionAssignmt.Function =
          db.GetString(reader, 13);
        entities.CurrChCaseUnitFunctionAssignmt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnitFunctionAssignmt6()
  {
    System.Diagnostics.Debug.Assert(entities.PrevCaseUnit.Populated);
    entities.PrevCaseUnitFunctionAssignmt.Populated = false;

    return ReadEach("ReadCaseUnitFunctionAssignmt6",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.PrevCaseUnit.CasNo);
        db.SetInt32(command, "csuNo", entities.PrevCaseUnit.CuNumber);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PrevCaseUnitFunctionAssignmt.ReasonCode =
          db.GetString(reader, 0);
        entities.PrevCaseUnitFunctionAssignmt.OverrideInd =
          db.GetString(reader, 1);
        entities.PrevCaseUnitFunctionAssignmt.EffectiveDate =
          db.GetDate(reader, 2);
        entities.PrevCaseUnitFunctionAssignmt.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PrevCaseUnitFunctionAssignmt.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.PrevCaseUnitFunctionAssignmt.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.PrevCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.PrevCaseUnitFunctionAssignmt.SpdId = db.GetInt32(reader, 7);
        entities.PrevCaseUnitFunctionAssignmt.OffId = db.GetInt32(reader, 8);
        entities.PrevCaseUnitFunctionAssignmt.OspCode = db.GetString(reader, 9);
        entities.PrevCaseUnitFunctionAssignmt.OspDate = db.GetDate(reader, 10);
        entities.PrevCaseUnitFunctionAssignmt.CsuNo = db.GetInt32(reader, 11);
        entities.PrevCaseUnitFunctionAssignmt.CasNo = db.GetString(reader, 12);
        entities.PrevCaseUnitFunctionAssignmt.Function =
          db.GetString(reader, 13);
        entities.PrevCaseUnitFunctionAssignmt.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.NewArCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.NewArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.NewArCsePerson.Number = db.GetString(reader, 0);
        entities.NewArCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.CurrCaseUnit.Populated);
    entities.Ap.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CurrCaseUnit.CspNoAp ?? "");
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.CurrApCaseRole.Populated);
    entities.Ap.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CurrApCaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private bool ReadCsePerson4()
  {
    System.Diagnostics.Debug.Assert(entities.CurrCaseUnit.Populated);
    entities.Ch.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CurrCaseUnit.CspNoChild ?? "");
      },
      (db, reader) =>
      {
        entities.Ch.Number = db.GetString(reader, 0);
        entities.Ch.Populated = true;
      });
  }

  private bool ReadCsePerson5()
  {
    System.Diagnostics.Debug.Assert(entities.CurrChCaseRole.Populated);
    entities.Ch.Populated = false;

    return Read("ReadCsePerson5",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CurrChCaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.Ch.Number = db.GetString(reader, 0);
        entities.Ch.Populated = true;
      });
  }

  private bool ReadInfrastructure1()
  {
    System.Diagnostics.Debug.Assert(entities.CurrArMonitoredActivity.Populated);
    entities.CurrArInfrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.CurrArMonitoredActivity.InfSysGenId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrArInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CurrArInfrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.CurrArInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.CurrArInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.CurrArInfrastructure.ReasonCode = db.GetString(reader, 4);
        entities.CurrArInfrastructure.BusinessObjectCd =
          db.GetString(reader, 5);
        entities.CurrArInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 6);
        entities.CurrArInfrastructure.DenormText12 =
          db.GetNullableString(reader, 7);
        entities.CurrArInfrastructure.DenormDate =
          db.GetNullableDate(reader, 8);
        entities.CurrArInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CurrArInfrastructure.InitiatingStateCode =
          db.GetString(reader, 10);
        entities.CurrArInfrastructure.CaseNumber =
          db.GetNullableString(reader, 11);
        entities.CurrArInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 12);
        entities.CurrArInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 13);
        entities.CurrArInfrastructure.UserId = db.GetString(reader, 14);
        entities.CurrArInfrastructure.CreatedBy = db.GetString(reader, 15);
        entities.CurrArInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.CurrArInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.CurrArInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.CurrArInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 19);
        entities.CurrArInfrastructure.Detail = db.GetNullableString(reader, 20);
        entities.CurrArInfrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.NewInfrastructure.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.NewInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.NewInfrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.NewInfrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.NewInfrastructure.EventId = db.GetInt32(reader, 3);
        entities.NewInfrastructure.ReasonCode = db.GetString(reader, 4);
        entities.NewInfrastructure.BusinessObjectCd = db.GetString(reader, 5);
        entities.NewInfrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 6);
        entities.NewInfrastructure.DenormText12 =
          db.GetNullableString(reader, 7);
        entities.NewInfrastructure.DenormDate = db.GetNullableDate(reader, 8);
        entities.NewInfrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.NewInfrastructure.InitiatingStateCode =
          db.GetString(reader, 10);
        entities.NewInfrastructure.CaseNumber =
          db.GetNullableString(reader, 11);
        entities.NewInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 12);
        entities.NewInfrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 13);
        entities.NewInfrastructure.UserId = db.GetString(reader, 14);
        entities.NewInfrastructure.CreatedBy = db.GetString(reader, 15);
        entities.NewInfrastructure.CreatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.NewInfrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.NewInfrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 18);
        entities.NewInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 19);
        entities.NewInfrastructure.Detail = db.GetNullableString(reader, 20);
        entities.NewInfrastructure.Populated = true;
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

  private IEnumerable<bool> ReadMonitoredActivity1()
  {
    entities.CurrApMonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity1",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetNullableInt32(
          command, "caseUnitNum", entities.CurrApCaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.CurrApMonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CurrApMonitoredActivity.StartDate = db.GetDate(reader, 1);
        entities.CurrApMonitoredActivity.ClosureDate =
          db.GetNullableDate(reader, 2);
        entities.CurrApMonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 3);
        entities.CurrApMonitoredActivity.CaseUnitClosedInd =
          db.GetString(reader, 4);
        entities.CurrApMonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CurrApMonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrApMonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 7);
        entities.CurrApMonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity2()
  {
    entities.CurrArMonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity2",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetNullableInt32(
          command, "caseUnitNum", entities.CurrCaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.CurrArMonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CurrArMonitoredActivity.Name = db.GetString(reader, 1);
        entities.CurrArMonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 2);
        entities.CurrArMonitoredActivity.TypeCode =
          db.GetNullableString(reader, 3);
        entities.CurrArMonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 4);
        entities.CurrArMonitoredActivity.FedNearNonComplDate =
          db.GetNullableDate(reader, 5);
        entities.CurrArMonitoredActivity.OtherNonComplianceDate =
          db.GetNullableDate(reader, 6);
        entities.CurrArMonitoredActivity.OtherNearNonComplDate =
          db.GetNullableDate(reader, 7);
        entities.CurrArMonitoredActivity.StartDate = db.GetDate(reader, 8);
        entities.CurrArMonitoredActivity.ClosureDate =
          db.GetNullableDate(reader, 9);
        entities.CurrArMonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 10);
        entities.CurrArMonitoredActivity.CaseUnitClosedInd =
          db.GetString(reader, 11);
        entities.CurrArMonitoredActivity.CreatedBy = db.GetString(reader, 12);
        entities.CurrArMonitoredActivity.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.CurrArMonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.CurrArMonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.CurrArMonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 16);
        entities.CurrArMonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity3()
  {
    entities.CurrChMonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity3",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", entities.Case1.Number);
        db.SetNullableInt32(
          command, "caseUnitNum", entities.CurrChCaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.CurrChMonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CurrChMonitoredActivity.StartDate = db.GetDate(reader, 1);
        entities.CurrChMonitoredActivity.ClosureDate =
          db.GetNullableDate(reader, 2);
        entities.CurrChMonitoredActivity.ClosureReasonCode =
          db.GetNullableString(reader, 3);
        entities.CurrChMonitoredActivity.CaseUnitClosedInd =
          db.GetString(reader, 4);
        entities.CurrChMonitoredActivity.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CurrChMonitoredActivity.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CurrChMonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 7);
        entities.CurrChMonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignment1()
  {
    entities.CurrApMonitoredActivityAssignment.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignment1",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId",
          entities.CurrApMonitoredActivity.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrApMonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 0);
        entities.CurrApMonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.CurrApMonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.CurrApMonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.CurrApMonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.CurrApMonitoredActivityAssignment.SpdId =
          db.GetInt32(reader, 5);
        entities.CurrApMonitoredActivityAssignment.OffId =
          db.GetInt32(reader, 6);
        entities.CurrApMonitoredActivityAssignment.OspCode =
          db.GetString(reader, 7);
        entities.CurrApMonitoredActivityAssignment.OspDate =
          db.GetDate(reader, 8);
        entities.CurrApMonitoredActivityAssignment.MacId =
          db.GetInt32(reader, 9);
        entities.CurrApMonitoredActivityAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignment2()
  {
    entities.CurrArMonitoredActivityAssignment.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId",
          entities.CurrArMonitoredActivity.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrArMonitoredActivityAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.CurrArMonitoredActivityAssignment.ResponsibilityCode =
          db.GetString(reader, 1);
        entities.CurrArMonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 2);
        entities.CurrArMonitoredActivityAssignment.OverrideInd =
          db.GetString(reader, 3);
        entities.CurrArMonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CurrArMonitoredActivityAssignment.CreatedBy =
          db.GetString(reader, 5);
        entities.CurrArMonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.CurrArMonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.CurrArMonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CurrArMonitoredActivityAssignment.SpdId =
          db.GetInt32(reader, 9);
        entities.CurrArMonitoredActivityAssignment.OffId =
          db.GetInt32(reader, 10);
        entities.CurrArMonitoredActivityAssignment.OspCode =
          db.GetString(reader, 11);
        entities.CurrArMonitoredActivityAssignment.OspDate =
          db.GetDate(reader, 12);
        entities.CurrArMonitoredActivityAssignment.MacId =
          db.GetInt32(reader, 13);
        entities.CurrArMonitoredActivityAssignment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivityAssignment3()
  {
    entities.CurrChMonitoredActivityAssignment.Populated = false;

    return ReadEach("ReadMonitoredActivityAssignment3",
      (db, command) =>
      {
        db.SetInt32(
          command, "macId",
          entities.CurrChMonitoredActivity.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CurrChMonitoredActivityAssignment.EffectiveDate =
          db.GetDate(reader, 0);
        entities.CurrChMonitoredActivityAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.CurrChMonitoredActivityAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.CurrChMonitoredActivityAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.CurrChMonitoredActivityAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.CurrChMonitoredActivityAssignment.SpdId =
          db.GetInt32(reader, 5);
        entities.CurrChMonitoredActivityAssignment.OffId =
          db.GetInt32(reader, 6);
        entities.CurrChMonitoredActivityAssignment.OspCode =
          db.GetString(reader, 7);
        entities.CurrChMonitoredActivityAssignment.OspDate =
          db.GetDate(reader, 8);
        entities.CurrChMonitoredActivityAssignment.MacId =
          db.GetInt32(reader, 9);
        entities.CurrChMonitoredActivityAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CurrArMona.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", entities.CurrArMona.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Case2.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.Case2.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.Case2.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Case2.RoleCode = db.GetString(reader, 2);
        entities.Case2.EffectiveDate = db.GetDate(reader, 3);
        entities.Case2.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Case2.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrArMonitoredActivityAssignment.Populated);
    entities.CurrArMona.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CurrArMonitoredActivityAssignment.OspDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode",
          entities.CurrArMonitoredActivityAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId",
          entities.CurrArMonitoredActivityAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.CurrArMonitoredActivityAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.CurrArMona.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.CurrArMona.OffGeneratedId = db.GetInt32(reader, 1);
        entities.CurrArMona.RoleCode = db.GetString(reader, 2);
        entities.CurrArMona.EffectiveDate = db.GetDate(reader, 3);
        entities.CurrArMona.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.CurrArMona.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider3()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrCaseUnitFunctionAssignmt.Populated);
    entities.CurrCufa.Populated = false;

    return Read("ReadOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CurrCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.CurrCaseUnitFunctionAssignmt.OspCode);
        db.SetInt32(
          command, "offGeneratedId",
          entities.CurrCaseUnitFunctionAssignmt.OffId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.CurrCaseUnitFunctionAssignmt.SpdId);
      },
      (db, reader) =>
      {
        entities.CurrCufa.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.CurrCufa.OffGeneratedId = db.GetInt32(reader, 1);
        entities.CurrCufa.RoleCode = db.GetString(reader, 2);
        entities.CurrCufa.EffectiveDate = db.GetDate(reader, 3);
        entities.CurrCufa.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.CurrCufa.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.CurrArMona.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", entities.CurrArMona.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.Populated = true;
      });
  }

  private void UpdateCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.CurrApCaseRole.Populated);

    var endDate = local.Use.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.CurrApCaseRole.Populated = false;
    Update("UpdateCaseRole1",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "casNumber", entities.CurrApCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CurrApCaseRole.CspNumber);
        db.SetString(command, "type", entities.CurrApCaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CurrApCaseRole.Identifier);
      });

    entities.CurrApCaseRole.EndDate = endDate;
    entities.CurrApCaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CurrApCaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CurrApCaseRole.Populated = true;
  }

  private void UpdateCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.CurrArCaseRole.Populated);

    var endDate = local.Use.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var arChgProcReqInd = local.Ar.ArChgProcReqInd ?? "";
    var arChgProcessedDate = local.Initialized.Date;
    var arInvalidInd = local.Ar.ArInvalidInd ?? "";

    entities.CurrArCaseRole.Populated = false;
    Update("UpdateCaseRole2",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "arChgPrcReqInd", arChgProcReqInd);
        db.SetNullableDate(command, "arChgProcDt", arChgProcessedDate);
        db.SetNullableString(command, "arInvalidInd", arInvalidInd);
        db.SetString(command, "casNumber", entities.CurrArCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CurrArCaseRole.CspNumber);
        db.SetString(command, "type", entities.CurrArCaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CurrArCaseRole.Identifier);
      });

    entities.CurrArCaseRole.EndDate = endDate;
    entities.CurrArCaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CurrArCaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CurrArCaseRole.ArChgProcReqInd = arChgProcReqInd;
    entities.CurrArCaseRole.ArChgProcessedDate = arChgProcessedDate;
    entities.CurrArCaseRole.ArInvalidInd = arInvalidInd;
    entities.CurrArCaseRole.Populated = true;
  }

  private void UpdateCaseRole3()
  {
    System.Diagnostics.Debug.Assert(entities.CurrChCaseRole.Populated);

    var endDate = local.Use.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.CurrChCaseRole.Populated = false;
    Update("UpdateCaseRole3",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "casNumber", entities.CurrChCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.CurrChCaseRole.CspNumber);
        db.SetString(command, "type", entities.CurrChCaseRole.Type1);
        db.SetInt32(command, "caseRoleId", entities.CurrChCaseRole.Identifier);
      });

    entities.CurrChCaseRole.EndDate = endDate;
    entities.CurrChCaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CurrChCaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.CurrChCaseRole.Populated = true;
  }

  private void UpdateCaseUnit1()
  {
    System.Diagnostics.Debug.Assert(entities.CurrCaseUnit.Populated);

    var closureDate = local.Use.Date;
    var closureReasonCode = "AR";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrCaseUnit.Populated = false;
    Update("UpdateCaseUnit1",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CurrCaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CurrCaseUnit.CasNo);
      });

    entities.CurrCaseUnit.ClosureDate = closureDate;
    entities.CurrCaseUnit.ClosureReasonCode = closureReasonCode;
    entities.CurrCaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CurrCaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CurrCaseUnit.Populated = true;
  }

  private void UpdateCaseUnit2()
  {
    System.Diagnostics.Debug.Assert(entities.CurrApCaseUnit.Populated);

    var closureDate = local.Use.Date;
    var closureReasonCode = "AR";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrApCaseUnit.Populated = false;
    Update("UpdateCaseUnit2",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CurrApCaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CurrApCaseUnit.CasNo);
      });

    entities.CurrApCaseUnit.ClosureDate = closureDate;
    entities.CurrApCaseUnit.ClosureReasonCode = closureReasonCode;
    entities.CurrApCaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CurrApCaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CurrApCaseUnit.Populated = true;
  }

  private void UpdateCaseUnit3()
  {
    System.Diagnostics.Debug.Assert(entities.CurrChCaseUnit.Populated);

    var closureDate = local.Use.Date;
    var closureReasonCode = "AR";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrChCaseUnit.Populated = false;
    Update("UpdateCaseUnit3",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CurrChCaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CurrChCaseUnit.CasNo);
      });

    entities.CurrChCaseUnit.ClosureDate = closureDate;
    entities.CurrChCaseUnit.ClosureReasonCode = closureReasonCode;
    entities.CurrChCaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CurrChCaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CurrChCaseUnit.Populated = true;
  }

  private void UpdateCaseUnit4()
  {
    System.Diagnostics.Debug.Assert(entities.NewCaseUnit.Populated);

    var state = entities.CurrCaseUnit.State;

    entities.NewCaseUnit.Populated = false;
    Update("UpdateCaseUnit4",
      (db, command) =>
      {
        db.SetString(command, "state", state);
        db.SetInt32(command, "cuNumber", entities.NewCaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.NewCaseUnit.CasNo);
      });

    entities.NewCaseUnit.State = state;
    entities.NewCaseUnit.Populated = true;
  }

  private void UpdateCaseUnit5()
  {
    System.Diagnostics.Debug.Assert(entities.PrevCaseUnit.Populated);

    var startDate = import.NewArCaseRole.StartDate;
    var closureDate = local.Max.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.PrevCaseUnit.Populated = false;
    Update("UpdateCaseUnit5",
      (db, command) =>
      {
        db.SetDate(command, "startDate", startDate);
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", "");
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.PrevCaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.PrevCaseUnit.CasNo);
      });

    entities.PrevCaseUnit.StartDate = startDate;
    entities.PrevCaseUnit.ClosureDate = closureDate;
    entities.PrevCaseUnit.ClosureReasonCode = "";
    entities.PrevCaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.PrevCaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.PrevCaseUnit.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt1()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrCaseUnitFunctionAssignmt.Populated);

    var overrideInd = "X";
    var discontinueDate = local.End.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrCaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt1",
      (db, command) =>
      {
        db.SetString(command, "overrideInd", overrideInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CurrCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.CurrCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.CurrCaseUnitFunctionAssignmt.CasNo);
      });

    entities.CurrCaseUnitFunctionAssignmt.OverrideInd = overrideInd;
    entities.CurrCaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.CurrCaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.CurrCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CurrCaseUnitFunctionAssignmt.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt2()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrCaseUnitFunctionAssignmt.Populated);

    var overrideInd = "N";

    entities.CurrCaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt2",
      (db, command) =>
      {
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CurrCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.CurrCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.CurrCaseUnitFunctionAssignmt.CasNo);
      });

    entities.CurrCaseUnitFunctionAssignmt.OverrideInd = overrideInd;
    entities.CurrCaseUnitFunctionAssignmt.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt3()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrApCaseUnitFunctionAssignmt.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrApCaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt3",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrApCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrApCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrApCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CurrApCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrApCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.CurrApCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.CurrApCaseUnitFunctionAssignmt.CasNo);
      });

    entities.CurrApCaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.CurrApCaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.CurrApCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CurrApCaseUnitFunctionAssignmt.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt4()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrChCaseUnitFunctionAssignmt.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrChCaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt4",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrChCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrChCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrChCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.CurrChCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrChCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.CurrChCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.CurrChCaseUnitFunctionAssignmt.CasNo);
      });

    entities.CurrChCaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.CurrChCaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.CurrChCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CurrChCaseUnitFunctionAssignmt.Populated = true;
  }

  private void UpdateCaseUnitFunctionAssignmt5()
  {
    System.Diagnostics.Debug.Assert(
      entities.PrevCaseUnitFunctionAssignmt.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.PrevCaseUnitFunctionAssignmt.Populated = false;
    Update("UpdateCaseUnitFunctionAssignmt5",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PrevCaseUnitFunctionAssignmt.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.PrevCaseUnitFunctionAssignmt.SpdId);
        db.SetInt32(
          command, "offId", entities.PrevCaseUnitFunctionAssignmt.OffId);
        db.SetString(
          command, "ospCode", entities.PrevCaseUnitFunctionAssignmt.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.PrevCaseUnitFunctionAssignmt.OspDate.GetValueOrDefault());
        db.SetInt32(
          command, "csuNo", entities.PrevCaseUnitFunctionAssignmt.CsuNo);
        db.SetString(
          command, "casNo", entities.PrevCaseUnitFunctionAssignmt.CasNo);
      });

    entities.PrevCaseUnitFunctionAssignmt.DiscontinueDate = discontinueDate;
    entities.PrevCaseUnitFunctionAssignmt.LastUpdatedBy = lastUpdatedBy;
    entities.PrevCaseUnitFunctionAssignmt.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.PrevCaseUnitFunctionAssignmt.Populated = true;
  }

  private void UpdateMonitoredActivity1()
  {
    var closureDate = local.Use.Date;
    var closureReasonCode = "SYS";
    var caseUnitClosedInd = "Y";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrApMonitoredActivity.Populated = false;
    Update("UpdateMonitoredActivity1",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetString(command, "caseUnitClosedI", caseUnitClosedInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.CurrApMonitoredActivity.SystemGeneratedIdentifier);
      });

    entities.CurrApMonitoredActivity.ClosureDate = closureDate;
    entities.CurrApMonitoredActivity.ClosureReasonCode = closureReasonCode;
    entities.CurrApMonitoredActivity.CaseUnitClosedInd = caseUnitClosedInd;
    entities.CurrApMonitoredActivity.LastUpdatedBy = lastUpdatedBy;
    entities.CurrApMonitoredActivity.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CurrApMonitoredActivity.Populated = true;
  }

  private void UpdateMonitoredActivity2()
  {
    var closureDate = local.Use.Date;
    var closureReasonCode = "SYS";
    var caseUnitClosedInd = "Y";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrArMonitoredActivity.Populated = false;
    Update("UpdateMonitoredActivity2",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetString(command, "caseUnitClosedI", caseUnitClosedInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.CurrArMonitoredActivity.SystemGeneratedIdentifier);
      });

    entities.CurrArMonitoredActivity.ClosureDate = closureDate;
    entities.CurrArMonitoredActivity.ClosureReasonCode = closureReasonCode;
    entities.CurrArMonitoredActivity.CaseUnitClosedInd = caseUnitClosedInd;
    entities.CurrArMonitoredActivity.LastUpdatedBy = lastUpdatedBy;
    entities.CurrArMonitoredActivity.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CurrArMonitoredActivity.Populated = true;
  }

  private void UpdateMonitoredActivity3()
  {
    var closureDate = local.Use.Date;
    var closureReasonCode = "SYS";
    var caseUnitClosedInd = "Y";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrChMonitoredActivity.Populated = false;
    Update("UpdateMonitoredActivity3",
      (db, command) =>
      {
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetString(command, "caseUnitClosedI", caseUnitClosedInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.CurrChMonitoredActivity.SystemGeneratedIdentifier);
      });

    entities.CurrChMonitoredActivity.ClosureDate = closureDate;
    entities.CurrChMonitoredActivity.ClosureReasonCode = closureReasonCode;
    entities.CurrChMonitoredActivity.CaseUnitClosedInd = caseUnitClosedInd;
    entities.CurrChMonitoredActivity.LastUpdatedBy = lastUpdatedBy;
    entities.CurrChMonitoredActivity.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CurrChMonitoredActivity.Populated = true;
  }

  private void UpdateMonitoredActivityAssignment1()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrApMonitoredActivityAssignment.Populated);

    var discontinueDate = local.Use.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrApMonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment1",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrApMonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrApMonitoredActivityAssignment.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrApMonitoredActivityAssignment.OffId);
        db.SetString(
          command, "ospCode",
          entities.CurrApMonitoredActivityAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrApMonitoredActivityAssignment.OspDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "macId", entities.CurrApMonitoredActivityAssignment.MacId);
      });

    entities.CurrApMonitoredActivityAssignment.DiscontinueDate =
      discontinueDate;
    entities.CurrApMonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.CurrApMonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CurrApMonitoredActivityAssignment.Populated = true;
  }

  private void UpdateMonitoredActivityAssignment2()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrArMonitoredActivityAssignment.Populated);

    var discontinueDate = local.Current.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrArMonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment2",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrArMonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrArMonitoredActivityAssignment.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrArMonitoredActivityAssignment.OffId);
        db.SetString(
          command, "ospCode",
          entities.CurrArMonitoredActivityAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrArMonitoredActivityAssignment.OspDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "macId", entities.CurrArMonitoredActivityAssignment.MacId);
      });

    entities.CurrArMonitoredActivityAssignment.DiscontinueDate =
      discontinueDate;
    entities.CurrArMonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.CurrArMonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CurrArMonitoredActivityAssignment.Populated = true;
  }

  private void UpdateMonitoredActivityAssignment3()
  {
    System.Diagnostics.Debug.Assert(
      entities.CurrChMonitoredActivityAssignment.Populated);

    var discontinueDate = local.Use.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CurrChMonitoredActivityAssignment.Populated = false;
    Update("UpdateMonitoredActivityAssignment3",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CurrChMonitoredActivityAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.CurrChMonitoredActivityAssignment.SpdId);
        db.SetInt32(
          command, "offId", entities.CurrChMonitoredActivityAssignment.OffId);
        db.SetString(
          command, "ospCode",
          entities.CurrChMonitoredActivityAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CurrChMonitoredActivityAssignment.OspDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "macId", entities.CurrChMonitoredActivityAssignment.MacId);
      });

    entities.CurrChMonitoredActivityAssignment.DiscontinueDate =
      discontinueDate;
    entities.CurrChMonitoredActivityAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.CurrChMonitoredActivityAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CurrChMonitoredActivityAssignment.Populated = true;
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
    /// A value of NewArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("newArCsePersonsWorkSet")]
    public CsePersonsWorkSet NewArCsePersonsWorkSet
    {
      get => newArCsePersonsWorkSet ??= new();
      set => newArCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NewArCaseRole.
    /// </summary>
    [JsonPropertyName("newArCaseRole")]
    public CaseRole NewArCaseRole
    {
      get => newArCaseRole ??= new();
      set => newArCaseRole = value;
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

    private CsePersonsWorkSet newArCsePersonsWorkSet;
    private CaseRole newArCaseRole;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
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
      public CaseFuncWorkSet Det
      {
        get => det ??= new();
        set => det = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CaseFuncWorkSet det;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of HoldCu.
    /// </summary>
    [JsonPropertyName("holdCu")]
    public WorkArea HoldCu
    {
      get => holdCu ??= new();
      set => holdCu = value;
    }

    /// <summary>
    /// A value of Mona.
    /// </summary>
    [JsonPropertyName("mona")]
    public Common Mona
    {
      get => mona ??= new();
      set => mona = value;
    }

    /// <summary>
    /// A value of NewMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("newMonitoredActivityAssignment")]
    public MonitoredActivityAssignment NewMonitoredActivityAssignment
    {
      get => newMonitoredActivityAssignment ??= new();
      set => newMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of NewMonitoredActivity.
    /// </summary>
    [JsonPropertyName("newMonitoredActivity")]
    public MonitoredActivity NewMonitoredActivity
    {
      get => newMonitoredActivity ??= new();
      set => newMonitoredActivity = value;
    }

    /// <summary>
    /// A value of NewInfrastructure.
    /// </summary>
    [JsonPropertyName("newInfrastructure")]
    public Infrastructure NewInfrastructure
    {
      get => newInfrastructure ??= new();
      set => newInfrastructure = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public CaseUnit Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of ChBecomesAr.
    /// </summary>
    [JsonPropertyName("chBecomesAr")]
    public Common ChBecomesAr
    {
      get => chBecomesAr ??= new();
      set => chBecomesAr = value;
    }

    /// <summary>
    /// A value of ApBecomesAr.
    /// </summary>
    [JsonPropertyName("apBecomesAr")]
    public Common ApBecomesAr
    {
      get => apBecomesAr ??= new();
      set => apBecomesAr = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Use.
    /// </summary>
    [JsonPropertyName("use")]
    public DateWorkArea Use
    {
      get => use ??= new();
      set => use = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of LastCaseRole.
    /// </summary>
    [JsonPropertyName("lastCaseRole")]
    public CaseRole LastCaseRole
    {
      get => lastCaseRole ??= new();
      set => lastCaseRole = value;
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
    /// A value of LastCaseUnit.
    /// </summary>
    [JsonPropertyName("lastCaseUnit")]
    public CaseUnit LastCaseUnit
    {
      get => lastCaseUnit ??= new();
      set => lastCaseUnit = value;
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

    private CaseUnitFunctionAssignmt caseUnitFunctionAssignmt;
    private CaseRole ar;
    private WorkArea holdCu;
    private Common mona;
    private MonitoredActivityAssignment newMonitoredActivityAssignment;
    private DateWorkArea initialized;
    private MonitoredActivity newMonitoredActivity;
    private Infrastructure newInfrastructure;
    private CaseUnit hold;
    private Common chBecomesAr;
    private Common apBecomesAr;
    private TextWorkArea textWorkArea;
    private Infrastructure infrastructure;
    private DateWorkArea current;
    private DateWorkArea end;
    private DateWorkArea use;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private CaseRole lastCaseRole;
    private DateWorkArea max;
    private CaseUnit lastCaseUnit;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PrevAr.
    /// </summary>
    [JsonPropertyName("prevAr")]
    public CaseRole PrevAr
    {
      get => prevAr ??= new();
      set => prevAr = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of CurrArMona.
    /// </summary>
    [JsonPropertyName("currArMona")]
    public OfficeServiceProvider CurrArMona
    {
      get => currArMona ??= new();
      set => currArMona = value;
    }

    /// <summary>
    /// A value of NewInfrastructure.
    /// </summary>
    [JsonPropertyName("newInfrastructure")]
    public Infrastructure NewInfrastructure
    {
      get => newInfrastructure ??= new();
      set => newInfrastructure = value;
    }

    /// <summary>
    /// A value of CurrArInfrastructure.
    /// </summary>
    [JsonPropertyName("currArInfrastructure")]
    public Infrastructure CurrArInfrastructure
    {
      get => currArInfrastructure ??= new();
      set => currArInfrastructure = value;
    }

    /// <summary>
    /// A value of CurrArMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("currArMonitoredActivityAssignment")]
    public MonitoredActivityAssignment CurrArMonitoredActivityAssignment
    {
      get => currArMonitoredActivityAssignment ??= new();
      set => currArMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of CurrArMonitoredActivity.
    /// </summary>
    [JsonPropertyName("currArMonitoredActivity")]
    public MonitoredActivity CurrArMonitoredActivity
    {
      get => currArMonitoredActivity ??= new();
      set => currArMonitoredActivity = value;
    }

    /// <summary>
    /// A value of CurrChMonitoredActivity.
    /// </summary>
    [JsonPropertyName("currChMonitoredActivity")]
    public MonitoredActivity CurrChMonitoredActivity
    {
      get => currChMonitoredActivity ??= new();
      set => currChMonitoredActivity = value;
    }

    /// <summary>
    /// A value of CurrChMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("currChMonitoredActivityAssignment")]
    public MonitoredActivityAssignment CurrChMonitoredActivityAssignment
    {
      get => currChMonitoredActivityAssignment ??= new();
      set => currChMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of CurrChInfrastructure.
    /// </summary>
    [JsonPropertyName("currChInfrastructure")]
    public Infrastructure CurrChInfrastructure
    {
      get => currChInfrastructure ??= new();
      set => currChInfrastructure = value;
    }

    /// <summary>
    /// A value of CurrApInfrastructure.
    /// </summary>
    [JsonPropertyName("currApInfrastructure")]
    public Infrastructure CurrApInfrastructure
    {
      get => currApInfrastructure ??= new();
      set => currApInfrastructure = value;
    }

    /// <summary>
    /// A value of CurrApMonitoredActivityAssignment.
    /// </summary>
    [JsonPropertyName("currApMonitoredActivityAssignment")]
    public MonitoredActivityAssignment CurrApMonitoredActivityAssignment
    {
      get => currApMonitoredActivityAssignment ??= new();
      set => currApMonitoredActivityAssignment = value;
    }

    /// <summary>
    /// A value of CurrApMonitoredActivity.
    /// </summary>
    [JsonPropertyName("currApMonitoredActivity")]
    public MonitoredActivity CurrApMonitoredActivity
    {
      get => currApMonitoredActivity ??= new();
      set => currApMonitoredActivity = value;
    }

    /// <summary>
    /// A value of CurrChCaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("currChCaseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CurrChCaseUnitFunctionAssignmt
    {
      get => currChCaseUnitFunctionAssignmt ??= new();
      set => currChCaseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of CurrChCaseUnit.
    /// </summary>
    [JsonPropertyName("currChCaseUnit")]
    public CaseUnit CurrChCaseUnit
    {
      get => currChCaseUnit ??= new();
      set => currChCaseUnit = value;
    }

    /// <summary>
    /// A value of CurrChCaseRole.
    /// </summary>
    [JsonPropertyName("currChCaseRole")]
    public CaseRole CurrChCaseRole
    {
      get => currChCaseRole ??= new();
      set => currChCaseRole = value;
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

    /// <summary>
    /// A value of CurrApCaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("currApCaseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CurrApCaseUnitFunctionAssignmt
    {
      get => currApCaseUnitFunctionAssignmt ??= new();
      set => currApCaseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of CurrApCaseUnit.
    /// </summary>
    [JsonPropertyName("currApCaseUnit")]
    public CaseUnit CurrApCaseUnit
    {
      get => currApCaseUnit ??= new();
      set => currApCaseUnit = value;
    }

    /// <summary>
    /// A value of CurrApCaseRole.
    /// </summary>
    [JsonPropertyName("currApCaseRole")]
    public CaseRole CurrApCaseRole
    {
      get => currApCaseRole ??= new();
      set => currApCaseRole = value;
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
    /// A value of CurrCufa.
    /// </summary>
    [JsonPropertyName("currCufa")]
    public OfficeServiceProvider CurrCufa
    {
      get => currCufa ??= new();
      set => currCufa = value;
    }

    /// <summary>
    /// A value of NewArCsePerson.
    /// </summary>
    [JsonPropertyName("newArCsePerson")]
    public CsePerson NewArCsePerson
    {
      get => newArCsePerson ??= new();
      set => newArCsePerson = value;
    }

    /// <summary>
    /// A value of CurrArCsePerson.
    /// </summary>
    [JsonPropertyName("currArCsePerson")]
    public CsePerson CurrArCsePerson
    {
      get => currArCsePerson ??= new();
      set => currArCsePerson = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of NewArCaseRole.
    /// </summary>
    [JsonPropertyName("newArCaseRole")]
    public CaseRole NewArCaseRole
    {
      get => newArCaseRole ??= new();
      set => newArCaseRole = value;
    }

    /// <summary>
    /// A value of CurrArCaseRole.
    /// </summary>
    [JsonPropertyName("currArCaseRole")]
    public CaseRole CurrArCaseRole
    {
      get => currArCaseRole ??= new();
      set => currArCaseRole = value;
    }

    /// <summary>
    /// A value of Related.
    /// </summary>
    [JsonPropertyName("related")]
    public CaseRole Related
    {
      get => related ??= new();
      set => related = value;
    }

    /// <summary>
    /// A value of CurrCaseUnit.
    /// </summary>
    [JsonPropertyName("currCaseUnit")]
    public CaseUnit CurrCaseUnit
    {
      get => currCaseUnit ??= new();
      set => currCaseUnit = value;
    }

    /// <summary>
    /// A value of PrevCaseUnit.
    /// </summary>
    [JsonPropertyName("prevCaseUnit")]
    public CaseUnit PrevCaseUnit
    {
      get => prevCaseUnit ??= new();
      set => prevCaseUnit = value;
    }

    /// <summary>
    /// A value of NewCaseUnit.
    /// </summary>
    [JsonPropertyName("newCaseUnit")]
    public CaseUnit NewCaseUnit
    {
      get => newCaseUnit ??= new();
      set => newCaseUnit = value;
    }

    /// <summary>
    /// A value of CurrCaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("currCaseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt CurrCaseUnitFunctionAssignmt
    {
      get => currCaseUnitFunctionAssignmt ??= new();
      set => currCaseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of PrevCaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("prevCaseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt PrevCaseUnitFunctionAssignmt
    {
      get => prevCaseUnitFunctionAssignmt ??= new();
      set => prevCaseUnitFunctionAssignmt = value;
    }

    /// <summary>
    /// A value of NewCaseUnitFunctionAssignmt.
    /// </summary>
    [JsonPropertyName("newCaseUnitFunctionAssignmt")]
    public CaseUnitFunctionAssignmt NewCaseUnitFunctionAssignmt
    {
      get => newCaseUnitFunctionAssignmt ??= new();
      set => newCaseUnitFunctionAssignmt = value;
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
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public OfficeServiceProvider Case2
    {
      get => case2 ??= new();
      set => case2 = value;
    }

    private CaseRole prevAr;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider currArMona;
    private Infrastructure newInfrastructure;
    private Infrastructure currArInfrastructure;
    private MonitoredActivityAssignment currArMonitoredActivityAssignment;
    private MonitoredActivity currArMonitoredActivity;
    private MonitoredActivity currChMonitoredActivity;
    private MonitoredActivityAssignment currChMonitoredActivityAssignment;
    private Infrastructure currChInfrastructure;
    private Infrastructure currApInfrastructure;
    private MonitoredActivityAssignment currApMonitoredActivityAssignment;
    private MonitoredActivity currApMonitoredActivity;
    private CaseUnitFunctionAssignmt currChCaseUnitFunctionAssignmt;
    private CaseUnit currChCaseUnit;
    private CaseRole currChCaseRole;
    private InterstateRequest interstateRequest;
    private CaseUnitFunctionAssignmt currApCaseUnitFunctionAssignmt;
    private CaseUnit currApCaseUnit;
    private CaseRole currApCaseRole;
    private Case1 case1;
    private OfficeServiceProvider currCufa;
    private CsePerson newArCsePerson;
    private CsePerson currArCsePerson;
    private CsePerson ap;
    private CsePerson ch;
    private CaseRole newArCaseRole;
    private CaseRole currArCaseRole;
    private CaseRole related;
    private CaseUnit currCaseUnit;
    private CaseUnit prevCaseUnit;
    private CaseUnit newCaseUnit;
    private CaseUnitFunctionAssignmt currCaseUnitFunctionAssignmt;
    private CaseUnitFunctionAssignmt prevCaseUnitFunctionAssignmt;
    private CaseUnitFunctionAssignmt newCaseUnitFunctionAssignmt;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider case2;
  }
#endregion
}
