// Program: SI_COMN_REOPEN_CASE_N_CASE_UNIT, ID: 371761719, model: 746.
// Short name: SWE01695
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
/// A program: SI_COMN_REOPEN_CASE_N_CASE_UNIT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiComnReopenCaseNCaseUnit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_COMN_REOPEN_CASE_N_CASE_UNIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiComnReopenCaseNCaseUnit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiComnReopenCaseNCaseUnit.
  /// </summary>
  public SiComnReopenCaseNCaseUnit(IContext context, Import import,
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
    // Date	  Developer	   Description
    // 11-14-96  G. Lofton - MTW  Initial Dev
    // 04-29-97  Sid	           Complete Case Re-Open
    // 11/05/98  W. Campbell      A SORTED BY clause was added
    //                            to a READ EACH SEARCH CASE_ROLE
    //                            statement to obtain the requested
    //                            data in an order which provided for
    //                            proper subsequent processing.  This
    //                            was to solve a problem whereby the
    //                            AP was not being found in a called
    //                            CAB.
    // 11/06/98 W. Campbell       Added a set stmt to UPDATE
    //                            statement to update the CASE
    //                            cse_open_date to current date
    //                            when REOPENing a CLOSED CASE.
    // 05/24/99 M. Lachowicz      Replace zdel exit state by
    //                            by new exit state.
    // -------------------------------------------------------
    // 06/15/99  M. Lachowicz - Change code for raising existing
    //                          events and add new code for new events.
    // ------------------------------------------------------------
    // 06/28/99  M. Lachowicz - Changed code to fix the problem which
    //                          caused that reason code CASEOPEN hasn't
    //                          been raised for event 5 (this peoblem is
    //                          result of 6/15/99 change).
    // ------------------------------------------------------------
    // 06/30/99  M. Lachowicz - Changed code to raise events for
    //                          active only case units.
    // ------------------------------------------------------------
    // 07/01/99 M.Lachowicz      Change property of READ
    //                           (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 07/05/99 M.Lachowicz      Changed code to pass Case Unit Number
    //                           and Person Number to infrastructure
    //                           for reason code 'CASEREOPENNOLOC',
    //                           'CASREOPENPATUNK' and
    //                           'CASREOPENPATEST'.
    // ------------------------------------------------------------
    // 07/05/99 M.Lachowicz      Update Closure Letter Date of CASE
    //                           to Max Sys Date.
    // ------------------------------------------------------------
    // 07/27/99 M.Lachowicz      Update Closure Letter Date of CASE
    //                           to 0001-01-01.
    // ------------------------------------------------------------
    // 09/22/99 W.Campbell       Deactivated an IF stmt
    //                           in order to allow
    //                           the reopening of a case
    //                           which did not have any
    //                           active children at the
    //                           time it was closed.
    // ----------------------------------------------------------
    // 11/17/99 W.Campbell       Disabled an IF statement
    //                           and replaced it with a CASE
    //                           statement with OTHERWISE
    //                           logic to provide an error if
    //                           case status is not Open or
    //                           Closed.  Work done on PR# 79699.
    // ----------------------------------------
    // 11/17/99 W.Campbell       Disabled some code
    //                           and replaced it as it did
    //                           not appear to be doing
    //                           anything.  Replace it with
    //                           some of the disabled logic.
    //                           Work done on PR# 79699.
    // ----------------------------------------
    // 11/17/99 W.CAMPBELL       Disabled old code to
    //                           Update Case Role and
    //                           Inserted Code to Create
    //                           a new Case Role for history.
    //                           Also inserted new code to
    //                           update case role.
    //                           Work done on PR# 79699.
    // --------------------------------------------
    // 03/06/00 W.CAMPBELL       Removed logic and
    //                           views which referenced ZDEL
    //                           attributes for paternity type
    //                           attributes in the Case Role
    //                           entity type.
    //                           Work done on WR# 000160-I.
    // --------------------------------------------
    // 03/06/00 W.Campbell       Inserted new logic to
    //                           detect if attempting to reopen a
    //                           case which has a CH with its
    //                           paternity established ind = Y,
    //                           and there will be multiple
    //                           active APs.  The logic must not
    //                           allow this case to be reopened.
    //                           Work done on WR# 000160-I.
    // ----------------------------------------
    // 08/10/00 W.Campbell       Inserted new logic to
    //                           accommodate the new attribute
    //                           Application Processed Ind which
    //                           has been added to entity
    //                           Information Request.  Also, added
    //                           logic to associate the re-opened
    //                           case with the Info Request if the
    //                           case reopening was due to an
    //                           Info Request.
    //                           Work done on PR# 100532-C.
    // ----------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Curent.Date = Now().Date;

    // -----------------------------------------------
    // 08/10/00 W.Campbell - Added following
    // set stmt to initialize local current timestamp.
    // Work done on PR# 100532-C.
    // -----------------------------------------------
    // 07/12/01 M.Lachowicz      Fix problem for
    //                           Interstate Request. PR123159
    // ------------------------------------------------------------
    local.Curent.Timestamp = Now();

    if (ReadCase())
    {
      local.Case1.Assign(entities.Case1);

      // ----------------------------------------
      // 11/17/99 W.Campbell - Disabled the
      // following IF statement and replaced
      // with a CASE statement with OTHERWISE
      // logic.  Work done on PR# 79699.
      // ----------------------------------------
      switch(AsChar(entities.Case1.Status))
      {
        case 'O':
          ExitState = "SI0000_CASE_NOT_CLOSED";

          return;
        case 'C':
          if (Equal(entities.Case1.StatusDate, local.Curent.Date))
          {
            ExitState = "SI0000_CLOSED_N_REOPEN_SAME_DAY";

            return;
          }

          local.CaseClosedDate.StatusDate = entities.Case1.StatusDate;
          local.TextWorkArea.Text4 =
            NumberToString(entities.Case1.OfficeIdentifier.GetValueOrDefault(),
            4);

          break;
        default:
          // ----------------------------------------
          // 11/27/99 W.Campbell - Added logic
          // to provide an error if case status
          // is not Open or Closed.  Work done
          // on PR# 79699.
          // ----------------------------------------
          ExitState = "SI0000_BAD_REOPEN_CASE_STATUS";

          return;
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // Validate case roles.
    local.AtLeastOneChReopened.Flag = "N";
    local.ArFound.Flag = "N";
    local.CaseRole.StartDate = local.Curent.Date;
    local.Group.Index = -1;
    local.Group.Count = 0;

    // -------------------------------------------------------
    // 11/05/98 W. Campbell  -  A SORTED BY clause
    // was added to the following READ EACH to obtain
    // the requested data in an order which provided
    // for proper subsequent processing.
    // -------------------------------------------------------
    foreach(var item in ReadCaseRoleCsePerson3())
    {
      if (Equal(entities.SearchCaseRole.Type1, "CH"))
      {
        local.CsePerson.Number = entities.SearchCsePerson.Number;

        // ----------------------------------------
        // 11/17/99 W.Campbell - Disabled the
        // following code and replaced it as
        // it does not appear to be doing anything.
        // Replace it with some of the disabled logic
        // Work done on PR# 79699.
        // ----------------------------------------
        // ----------------------------------------
        // 03/06/00 W.Campbell - Deleted
        // disabled code in order to delete
        // zdel paternity related views.
        // Work done on WR# 000160-I.
        // ----------------------------------------
        // ----------------------------------------
        // 11/17/99 W.Campbell - Replaced the above
        // disabled code with some of the logic from
        // the disabled code.
        // Work done on PR# 79699.
        // ----------------------------------------
        // ----------------------------------------
        // 03/06/00 W.Campbell - Inserted new logic
        // to detect if attempting to reopen a
        // case which has a CH with its
        // paternity established ind = Y, and there
        // will be multiple active APs.  The logic
        // must not allow this case to be reopened.
        // Work done on WR# 000160-I.
        // ----------------------------------------
        if (AsChar(entities.SearchCsePerson.PaternityEstablishedIndicator) == 'Y'
          )
        {
          local.Paternity.Flag = "Y";
        }

        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.DetCsePerson.Number =
          entities.SearchCsePerson.Number;
        local.Group.Update.DetCaseRole.Assign(entities.SearchCaseRole);
        local.Group.Update.DetCaseRole.StartDate = local.Curent.Date;
        local.AtLeastOneChReopened.Flag = "Y";
      }
      else
      {
        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.DetCsePerson.Number =
          entities.SearchCsePerson.Number;
        local.Group.Update.DetCaseRole.Assign(entities.SearchCaseRole);
        local.Group.Update.DetCaseRole.StartDate = local.Curent.Date;

        if (Equal(entities.SearchCaseRole.Type1, "AR"))
        {
          local.ArFound.Flag = "Y";
        }
        else
        {
          // ----------------------------------------
          // 03/06/00 W.Campbell - Inserted new logic
          // to detect if attempting to reopen a
          // case which has a CH with its
          // paternity established ind = Y, and there
          // will be multiple active APs.  The logic
          // must not allow this case to be reopened.
          // Work done on WR# 000160-I.
          // ----------------------------------------
          if (Equal(entities.SearchCaseRole.Type1, "AP"))
          {
            // ----------------------------------------
            // 03/06/00 W.Campbell - Count the
            // number of APs which will be on
            // the Reopened Case.
            // Work done on WR# 000160-I.
            // ----------------------------------------
            ++local.Paternity.Count;
          }
        }
      }
    }

    if (AsChar(local.ArFound.Flag) == 'N')
    {
      ExitState = "SI0000_CASE_NOT_OPENED_NO_AR";

      return;
    }

    // ----------------------------------------------------------
    // 09/22/99 W.Campbell - Deactivated the following
    // IF stmt in order to allow the reopening of a case
    // which did not have any active children at the
    // time it was closed.
    // ----------------------------------------------------------
    // ----------------------------------------
    // 03/06/00 W.Campbell - Inserted new logic
    // to detect if attempting to reopen a
    // case which has a CH with its
    // paternity established ind = Y, and there
    // will be multiple active APs.  The logic
    // must not allow this case to be reopened.
    // Work done on WR# 000160-I.
    // ----------------------------------------
    if (AsChar(local.Paternity.Flag) == 'Y')
    {
      local.Paternity.Flag = "";

      // --------------------------------------------
      // We have at least one CH with
      // Paternity Established Ind = Y.
      // --------------------------------------------
      if (local.Paternity.Count > 1)
      {
        // --------------------------------------------
        // The case has more than one AP
        // which will be Reopened and at
        // least one CH with Paternity Est.
        // which will be Reopened.  Cannot
        // allow this case to be Reopened.
        // --------------------------------------------
        ExitState = "SI0000_INV_REOPEN_PATERNITY_EST";

        return;
      }
    }

    // ************************************
    // Reopen Case and Case Unit.
    // ************************************
    UseCabSetMaximumDiscontinueDate();

    // -------------------------------------------------------
    // 11/17/98 W. Campbell  -  Added code to
    // Create new Case Roles and Case Units
    // for history purposes.
    // Work done on PR# 79699.
    // -------------------------------------------------------
    // --------------------------------------------
    // 11/17/99 W.CAMPBELL - This READ is to
    // get the largest Case Role identifier,
    // which will be used in the Creation of
    // new Case Roles.
    // --------------------------------------------
    if (ReadCaseRole2())
    {
      // --------------------------------------------
      // 11/17/99 W.CAMPBELL - Property of this
      // READ EACH set to OPTIMIZE FOR 1 ROW.
      // --------------------------------------------
      local.GreatestCaseRole.Identifier = entities.SearchCaseRole.Identifier;
    }

    // --------------------------------------------
    // 11/17/99 W.CAMPBELL - This READ is to
    // get the largest Case Unit cu_number,
    // which will be used in the Creation of
    // new Case Units.
    // --------------------------------------------
    if (ReadCaseUnit1())
    {
      // --------------------------------------------
      // 11/17/99 W.CAMPBELL - Property of this
      // READ EACH set to OPTIMIZE FOR 1 ROW.
      // --------------------------------------------
      local.GreatestCaseUnit.CuNumber = entities.CaseUnit.CuNumber;
    }

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!local.Group.CheckSize())
      {
        break;
      }

      // 07/01/99 M.L         Change property of READ to generate
      //                      Select Only
      // ------------------------------------------------------------
      if (!ReadCsePerson1())
      {
        // 05/24/99 M. Lachowicz      Replace zdel exit state by
        //                            by new exit state.
        ExitState = "CSE_PERSON_NF";

        return;
      }

      // 07/01/99 M.L         Change property of READ to generate
      //                      Select Only
      // ------------------------------------------------------------
      // --------------------------------------------
      // 11/17/99 W.CAMPBELL - Disabled existing code
      // to Update Case Role and Inserted new code to
      // Create a new Case Role for history purposes.
      // Also inserted new code for updating the old
      // case role.
      // --------------------------------------------
      // ----------------------------------------
      // 03/06/00 W.Campbell - Deleted
      // disabled code in order to delete
      // zdel paternity related views.
      // Work done on WR# 000160-I.
      // ----------------------------------------
      // --------------------------------------------
      // 11/17/99 W.CAMPBELL - Inserted new code to
      // Create a new Case Role.  Must set most of the
      // attributes in the New Case Role to the same
      // values as they were in the Old Case Role.
      // --------------------------------------------
      if (ReadCaseRole1())
      {
        // --------------------------------------------
        // 11/17/99 W.CAMPBELL - Increment the
        // Case Role identifier for create of new
        // Case Role.
        // --------------------------------------------
        ++local.GreatestCaseRole.Identifier;

        // -----------------------------------------
        // 11/17/99 W.Campbell - Create a new case role
        // and initialize it from the old case role.  This
        // new one will look like the old one for history
        // purposes.
        // -----------------------------------------
        try
        {
          CreateCaseRole();

          // -----------------------------------------
          // 11/17/99 W.Campbell - Do any associations
          // needed for the case role.  It is not possible
          // to do all the possible associations because
          // of the data model design and the fact that
          // some of the relations are identifiers on the
          // non-case role end of the relationship.  These
          // are the relations to:
          // 1. non cooperation
          // 2. good cause
          // 3. cse person support worksheet
          // 4. legal action case role
          // 5. legal referral case role
          // 6. health insurance viability
          // -----------------------------------------
          // -----------------------------------------
          // 11/18/99 W.Campbell - Associate with
          // Appointment if needed for the new case role.
          // -----------------------------------------
          if (ReadAppointment())
          {
            AssociateAppointment();
          }

          // -----------------------------------------
          // 11/18/99 W.Campbell - Do Associations for
          // each of the subtypes of case role, if needed.
          // -----------------------------------------
          switch(TrimEnd(entities.Old.Type1))
          {
            case "AP":
              // -----------------------------------------
              // 11/18/99 W.Campbell - Do Associations for
              // the subtype AP of case role, if needed.
              // -----------------------------------------
              // -----------------------------------------
              // 11/18/99 W.Campbell - Associate with
              // Interstate request if needed for
              // the new case role.
              // -----------------------------------------
              if (ReadInterstateRequest2())
              {
                // 07/12/01 M.L Start
                // 07/12/01 M.L End
              }

              // -----------------------------------------
              // 11/18/99 W.Campbell - Associate with
              // Genetic Test if needed for
              // the new case role.
              // -----------------------------------------
              if (ReadGeneticTest2())
              {
                // 07/12/01 M.L Start
                // 07/12/01 M.L End
              }

              break;
            case "AR":
              // -----------------------------------------
              // 11/18/99 W.Campbell - Do Associations for
              // the subtype AR of case role, if needed.
              // -----------------------------------------
              export.Ar.Number = local.Group.Item.DetCsePerson.Number;

              break;
            case "CH":
              // -----------------------------------------
              // 11/18/99 W.Campbell - Do Associations for
              // the subtype CH of case role, if needed.
              // -----------------------------------------
              // -----------------------------------------
              // 11/18/99 W.Campbell - Associate with
              // Genetic Test if needed for
              // the new case role.
              // -----------------------------------------
              if (ReadGeneticTest1())
              {
                AssociateGeneticTest1();
              }

              // -----------------------------------------
              // 11/18/99 W.Campbell - No logic was
              // included to Associate
              // Father Child Assignment with the
              // new case role, as there is no
              // other supporting logic anywhere
              // in the system at this time.
              // -----------------------------------------
              break;
            case "MO":
              // -----------------------------------------
              // 11/18/99 W.Campbell - Do Associations for
              // the subtype MO of case role, if needed.
              // -----------------------------------------
              // -----------------------------------------
              // 11/18/99 W.Campbell - Associate with
              // Genetic Test if needed for
              // the new case role.
              // -----------------------------------------
              if (ReadGeneticTest3())
              {
                AssociateGeneticTest2();
              }

              break;
            case "FA":
              // -----------------------------------------
              // 11/18/99 W.Campbell - Do Associations for
              // the subtype FA of case role, if needed.
              // -----------------------------------------
              // -----------------------------------------
              // 11/18/99 W.Campbell - No logic was
              // included to Associate
              // Father Child Assignment with the
              // new case role, as there is no
              // other supporting logic anywhere
              // in the system at this time.
              // -----------------------------------------
              break;
            default:
              ExitState = "UNKNOWN_CASE_ROLE_TYPE";

              return;
          }
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

        // -----------------------------------------
        // 11/18/99 W.Campbell - Update the old
        // case role to make it active again.  This
        // in effect, reactiates the case role with
        // all of the existing relationships still
        // available from when it was deactivated
        // due to case closure.
        // -----------------------------------------
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
        ExitState = "CASE_ROLE_NF_RB";

        return;
      }

      if (Equal(local.Group.Item.DetCaseRole.Type1, "CH"))
      {
        foreach(var item in ReadCaseUnit2())
        {
          try
          {
            UpdateCaseUnit();

            // 06/30/99 M.L Code was disable based on Terri and Pam 
            // recommendation
            // ----------------------------------------
            // 03/06/00 W.Campbell - Deleted
            // disabled code in order to delete
            // zdel paternity related views.
            // Work done on WR# 000160-I.
            // ----------------------------------------
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
        }
      }
    }

    local.Group.CheckIndex();

    // -----------------------------------------------
    // 11/06/98 W. Campbell  -  Added set stmt to UPDATE
    // statement to update the CASE cse_open_date to
    // current date when REOPENing a CLOSED CASE.
    // -----------------------------------------------
    // 07/23/99 M.Lachowicz      Update Closure Letter Date of CASE
    //                           to Max Sys Date.
    // 07/27/99 M.Lachowicz      Update Closure Letter Date of CASE
    //                           to Max Sys Date.
    try
    {
      UpdateCase1();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CASE_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CASE_PV_RB";

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

    // ----------------------------------------
    // 08/10/00 W.Campbell - Inserted new logic to
    // accommodate the new attribute
    // Application Processed Ind which
    // has been added to entity
    // Information Request.  Also, added
    // logic to associate the re-opened
    // case with the Info Request if the
    // case reopening was due to an
    // Info Request.  Work done on PR# 100532-C.
    // ----------------------------------------
    if (import.InformationRequest.Number > 0)
    {
      // -----------------------------------------------
      // Read INFO REQ using the number from
      // the import view.
      // -----------------------------------------------
      if (ReadInformationRequest1())
      {
        if (AsChar(entities.NewInformationRequest.Type1) != 'R')
        {
          export.GoToInrdReopen.Text1 = "Y";

          return;
        }

        local.InformationRequest.Assign(entities.NewInformationRequest);
      }
      else
      {
        ExitState = "INQUIRY_NF";

        return;
      }

      // -----------------------------------------------
      // Read the currently related INFO REQ,
      // If there is one.  This is a 1 to 1 relatioship.
      // -----------------------------------------------
      local.RecordCount.Count = 0;

      foreach(var item in ReadInformationRequest2())
      {
        if (import.InformationRequest.Number == entities
          .InformationRequest.Number)
        {
          local.RecordCount.Count = (int)((long)local.RecordCount.Count + 1);

          break;
        }
      }

      if (local.RecordCount.Count <= 0)
      {
        // -----------------------------------------------
        // This case is not currently related to an
        // INFO REQ,  Therefore, we can associate
        // this case to the INFO REQ number from
        // the import view.
        // -----------------------------------------------
        AssociateCase();
      }

      if (!IsEmpty(entities.NewInformationRequest.ReopenReasonType))
      {
        local.InformationRequest.Assign(entities.NewInformationRequest);
        local.Compare.Street1 = local.InformationRequest.ApplicantStreet1 ?? "";
        local.Compare.Street2 = local.InformationRequest.ApplicantStreet2 ?? "";
        local.Compare.City = local.InformationRequest.ApplicantCity ?? "";
        local.Compare.State = local.InformationRequest.ApplicantState ?? "";
        local.Compare.ZipCode = local.InformationRequest.ApplicantZip5 ?? "";

        if (AsChar(entities.NewInformationRequest.ReopenReasonType) == 'P')
        {
          if (!IsEmpty(entities.NewInformationRequest.ApplicantStreet1))
          {
            if (ReadCaseRoleCsePerson2())
            {
              local.Check.Number = entities.CsePerson.Number;
            }

            local.CsePersonsWorkSet.Number = local.Check.Number;
            UseFnCabReadCsePersonAddress1();

            if (Equal(local.Compare.Street1, local.CsePersonAddress.Street1) &&
              Equal(local.Compare.City, local.CsePersonAddress.City) && Equal
              (local.Compare.State, local.CsePersonAddress.State) && Equal
              (local.Compare.ZipCode, local.CsePersonAddress.ZipCode))
            {
              // this is the AR address so we can't us it for this  enrollment 
              // type, need to get the address for the AP
              if (ReadCaseRoleCsePerson1())
              {
                local.Ar.Number = entities.CsePerson.Number;
              }

              local.CsePersonsWorkSet.Number = local.Ar.Number;
              local.Check.Number = local.Ar.Number;
              UseFnCabReadCsePersonAddress2();

              // already have the address on addr, nothing new to add
              goto Test2;
            }
            else
            {
              if (ReadCaseRoleCsePerson1())
              {
                local.Ar.Number = entities.CsePerson.Number;
              }

              local.CsePersonsWorkSet.Number = local.Check.Number;
              UseFnCabReadCsePersonAddress1();

              if (Equal(local.Compare.Street1, local.CsePersonAddress.Street1) &&
                Equal(local.Compare.City, local.CsePersonAddress.City) && Equal
                (local.Compare.State, local.CsePersonAddress.State) && Equal
                (local.Compare.ZipCode, local.CsePersonAddress.ZipCode))
              {
                goto Test2;
              }
              else
              {
                local.Check.Number = local.Ar.Number;

                // assuming the address in not the AR's address
                local.CsePersonAddress.Street1 =
                  local.InformationRequest.ApplicantStreet1 ?? "";
                local.CsePersonAddress.Street2 =
                  local.InformationRequest.ApplicantStreet2 ?? "";
                local.CsePersonAddress.City =
                  local.InformationRequest.ApplicantCity ?? "";
                local.CsePersonAddress.State =
                  local.InformationRequest.ApplicantState ?? "";
                local.CsePersonAddress.ZipCode =
                  local.InformationRequest.ApplicantZip5 ?? "";
              }
            }
          }
          else
          {
            // no address so we need to get the ap's address
            if (ReadCaseRoleCsePerson1())
            {
              local.Check.Number = entities.CsePerson.Number;
            }

            local.CsePersonsWorkSet.Number = local.Check.Number;
            local.CsePerson.Number = local.Ar.Number;
            UseFnCabReadCsePersonAddress1();
          }

          local.CsePersonAddress.Source = "AP";
        }
        else
        {
          if (AsChar(entities.NewInformationRequest.ReopenReasonType) == 'I'
            || AsChar(entities.NewInformationRequest.ReopenReasonType) == 'J'
            || AsChar(entities.NewInformationRequest.ReopenReasonType) == 'F')
          {
            goto Test2;
          }

          // need the AR address
          local.CsePersonAddress.Source = "AR";

          if (ReadCaseRoleCsePerson2())
          {
            local.Check.Number = entities.CsePerson.Number;
          }

          local.CsePerson.Number = local.CsePersonsWorkSet.Number;
          local.CsePersonsWorkSet.Number = local.Check.Number;
          UseFnCabReadCsePersonAddress1();

          if (Equal(local.Compare.Street1, local.CsePersonAddress.Street1) && Equal
            (local.Compare.City, local.CsePersonAddress.City) && Equal
            (local.Compare.State, local.CsePersonAddress.State) && Equal
            (local.Compare.ZipCode, local.CsePersonAddress.ZipCode))
          {
            if (!IsEmpty(local.CsePersonAddress.Street2) && !
              IsEmpty(local.Compare.Street2))
            {
              if (Equal(local.Compare.Street2, local.CsePersonAddress.Street2))
              {
                goto Test2;
              }
              else
              {
                local.CsePersonAddress.Street1 =
                  local.InformationRequest.ApplicantStreet1 ?? "";
                local.CsePersonAddress.Street2 =
                  local.InformationRequest.ApplicantStreet2 ?? "";
                local.CsePersonAddress.City =
                  local.InformationRequest.ApplicantCity ?? "";
                local.CsePersonAddress.State =
                  local.InformationRequest.ApplicantState ?? "";
                local.CsePersonAddress.ZipCode =
                  local.InformationRequest.ApplicantZip5 ?? "";

                goto Test1;
              }
            }

            // no update need, no new address
            goto Test2;
          }
          else
          {
            local.CsePersonAddress.Street1 =
              local.InformationRequest.ApplicantStreet1 ?? "";
            local.CsePersonAddress.Street2 =
              local.InformationRequest.ApplicantStreet2 ?? "";
            local.CsePersonAddress.City =
              local.InformationRequest.ApplicantCity ?? "";
            local.CsePersonAddress.State =
              local.InformationRequest.ApplicantState ?? "";
            local.CsePersonAddress.ZipCode =
              local.InformationRequest.ApplicantZip5 ?? "";
          }
        }

Test1:

        local.CsePersonAddress.LocationType = "D";
        local.CsePersonAddress.Type1 = "M";

        if (Equal(local.CsePersonAddress.State, "KS"))
        {
          UseEabReturnKsCountyByZip();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        local.CsePersonAddress.VerifiedDate = Now().Date;
        UseSiCheckForDuplicateAddress();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        if (AsChar(local.DuplicateAddress.Flag) == 'Y')
        {
          goto Test2;
        }

        UseSiCreateCsePersonAddress();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }
      }

Test2:

      // we are changing how we are reopening cases, we are not going to be 
      // disassociate
      // information request record from a case record before we associate the 
      // new information
      // request record.
      // -----------------------------------------------
      // Now, update the new attribute:
      // Application Processed Ind in the
      // INFO REQ to indicate that the
      // INFO REQ has been processed.
      // -----------------------------------------------
      try
      {
        UpdateInformationRequest();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INQUIRY_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INQUIRY_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // -----------------------------------------------
      // Now, update the attribute: information
      // request number in the Case entity type.
      // This attribute is reduntant as it is in
      // addition to the Foreign Key.
      // -----------------------------------------------
      try
      {
        UpdateCase2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CASE_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CASE_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else if (!IsEmpty(import.PaReferral.Number) || import
      .InterstateCase.TransSerialNumber > 0)
    {
    }
    else
    {
      // we are reopening a case so we need to go to inrd to create a reopen 
      // record, there we
      //  will have get the reason it is reopening,
      export.GoToInrdReopen.Text1 = "Y";

      return;
    }

    // ----------------------------------------
    // 08/10/00 W.Campbell - End of Inserted new
    // logic to accommodate the new attribute
    // Application Processed Ind which
    // has been added to entity
    // Information Request.  Also, added
    // logic to associate the re-opened
    // case with the Info Request if the
    // case reopening was due to an
    // Info Request.  Work done on PR# 100532-C.
    // ----------------------------------------
    // -----------------------------------------------
    // The Case has been reopened and the Case Units
    // have been re-activated. Now Assign the Case
    // and the Case Units.
    // -----------------------------------------------
    UseSiCaseAndCaseUnitAssignment();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ML 06/15/99 Start of disable code.
    // ML 06/15/99 End of disable code
    // ML 06/15/99 Beginning of new
    // code for event insertion.
    // ***	Begin Event insertion	***
    if (ReadInterstateRequest1())
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
    local.Infrastructure.BusinessObjectCd = "CAS";
    local.Infrastructure.CaseNumber = import.Case1.Number;
    local.Infrastructure.UserId = "COMN";
    local.Infrastructure.ReferenceDate = local.Curent.Date;
    local.Infrastructure.CaseUnitNumber = 0;
    local.CaseUnit.Flag = "N";

    // 06/30/99 M.L  Added extra qualification to read active only case
    //               units.
    foreach(var item in ReadCaseUnit3())
    {
      local.CaseUnit.Flag = "Y";
      local.Locate.Flag = Substring(entities.CaseUnit.State, 3, 1);
      local.Obligation.Flag = Substring(entities.CaseUnit.State, 5, 1);
      local.Paternity.Flag = Substring(entities.CaseUnit.State, 4, 1);

      if (IsEmpty(local.CasreopenwthoblGenerated.Flag))
      {
        if (AsChar(local.Obligation.Flag) == 'Y')
        {
          local.Infrastructure.EventId = 5;
          local.Infrastructure.ReasonCode = "CASREOPENWTHOBL";
          local.TextWorkArea.Text30 = "Case Reopened :";
          local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
          local.TextWorkArea.Text10 = UseCabConvertDate2String();
          local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
            .TextWorkArea.Text10;
          UseSpCabCreateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          local.CasreopenwthoblGenerated.Flag = "Y";
        }
      }

      if (AsChar(local.Locate.Flag) == 'N')
      {
        local.Infrastructure.EventId = 10;
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

        // 07/05/99 M.L Start
        // ------------------------------------
        // READ for the AP for this CASE UNIT here.
        // ------------------------------------
        if (ReadCsePerson2())
        {
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
        }
        else
        {
          ExitState = "AP_NF_RB";

          return;
        }

        // 07/05/99 M.L End
        local.Infrastructure.ReasonCode = "CASEREOPENNOLOC";
        local.TextWorkArea.Text30 = "Case Reopened :";
        local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
        local.TextWorkArea.Text10 = UseCabConvertDate2String();
        local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
          .TextWorkArea.Text10;
        UseSpCabCreateInfrastructure();
        local.Infrastructure.CsePersonNumber = "";
        local.Infrastructure.CaseUnitNumber = 0;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
      else if (AsChar(local.Paternity.Flag) == 'U')
      {
        local.Infrastructure.ReasonCode = "CASREOPENPATUNK";
        local.Infrastructure.EventId = 5;

        // 07/05/99 M.L Start
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

        // ------------------------------------
        // READ for the AP for this CASE UNIT here.
        // ------------------------------------
        if (ReadCsePerson2())
        {
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
        }
        else
        {
          ExitState = "AP_NF_RB";

          return;
        }

        // 07/05/99 M.L End
        local.TextWorkArea.Text30 = "Case Reopened :";
        local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
        local.TextWorkArea.Text10 = UseCabConvertDate2String();
        local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
          .TextWorkArea.Text10;
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        // 07/05/99 M.L Start
        local.Infrastructure.CsePersonNumber = "";
        local.Infrastructure.CaseUnitNumber = 0;

        // 07/05/99 M.L End
      }
      else if (AsChar(local.Paternity.Flag) == 'Y' && IsEmpty
        (local.CasreopenwthoblGenerated.Flag))
      {
        // 06/30/99 M.L Start
        // ------------------------------------
        // READ for the AP for this CASE UNIT here.
        // ------------------------------------
        if (ReadCsePerson2())
        {
          local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
        }
        else
        {
          ExitState = "AP_NF_RB";

          return;
        }

        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

        // 06/30/99 M.L End
        local.Infrastructure.ReasonCode = "CASREOPENPATEST";
        local.Infrastructure.EventId = 5;
        local.TextWorkArea.Text30 = "Case Reopened :";
        local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
        local.TextWorkArea.Text10 = UseCabConvertDate2String();
        local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
          .TextWorkArea.Text10;
        UseSpCabCreateInfrastructure();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        // 07/05/99 M.L Start
        // 07/05/99 M.L End
        local.Infrastructure.CsePersonNumber = "";
        local.Infrastructure.CaseUnitNumber = 0;
      }
    }

    local.Infrastructure.EventId = 5;

    if (AsChar(local.CaseUnit.Flag) == 'Y')
    {
      // 6/28/99 M.L Start
      if (import.InterstateCase.TransSerialNumber > 0)
      {
        local.Infrastructure.ReasonCode = "REOPEN_INTSTATE";
        local.TextWorkArea.Text30 = "Case Reopened :";
        local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
        local.TextWorkArea.Text10 = UseCabConvertDate2String();
        local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
          .TextWorkArea.Text10;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + "; from Interstate_Case Transaction_Serial_Number :";
          
      }
      else if (import.InformationRequest.Number > 0)
      {
        local.Infrastructure.ReasonCode = "REOPEN_INFOREQ";
        local.TextWorkArea.Text30 = "Case Reopened :";
        local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
        local.TextWorkArea.Text10 = UseCabConvertDate2String();
        local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
          .TextWorkArea.Text10;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + "; from Information Request #";
          
        local.TextWorkArea.Text10 =
          NumberToString(import.InformationRequest.Number, 10);
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
          (local.TextWorkArea.Text10);
      }
      else if (!IsEmpty(import.PaReferral.Number))
      {
        local.Infrastructure.ReasonCode = "REOPEN_PARFRL";
        local.TextWorkArea.Text30 = "Case Reopened :";
        local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
        local.TextWorkArea.Text10 = UseCabConvertDate2String();
        local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
          .TextWorkArea.Text10;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + "; from PA Referral #";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
          (import.PaReferral.Number);
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + "; Type :";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
          (import.PaReferral.Type1);
      }
      else
      {
        local.Infrastructure.ReasonCode = "CASEREOPEN";
        local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
        local.TextWorkArea.Text10 = UseCabConvertDate2String();
        local.Infrastructure.Detail =
          TrimEnd("Case Reopened from Other Source :") + local
          .TextWorkArea.Text10;
      }

      // 6/28/99 M.L End
      UseSpCabCreateInfrastructure();

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
      local.Infrastructure.ReasonCode = "CASEREOPNWOCAU";
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + "; w/o CU.";
        
      UseSpCabCreateInfrastructure();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (!Equal(import.Reopen.SystemGeneratedId, entities.Case1.OfficeIdentifier))
      
    {
      local.Infrastructure.ReasonCode = "CASEREOPEN_OFF";
      local.TextWorkArea.Text30 = "Case Reopened :";
      local.TextWorkArea.Text4 =
        NumberToString(import.Reopen.SystemGeneratedId, 4);
      local.Infrastructure.CaseUnitNumber = 0;
      local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
      local.TextWorkArea.Text10 = UseCabConvertDate2String();
      local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + local
        .TextWorkArea.Text10;
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + "; in another Office :";
        
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
        .TextWorkArea.Text4;
      UseSpCabCreateInfrastructure();
    }

    // ***	End Event insertion	***
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.VerifiedDate = source.VerifiedDate;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.County = source.County;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZipCode = source.ZipCode;
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseEabReturnKsCountyByZip()
  {
    var useImport = new EabReturnKsCountyByZip.Import();
    var useExport = new EabReturnKsCountyByZip.Export();

    MoveCsePersonAddress3(local.CsePersonAddress, useImport.CsePersonAddress);
    MoveCsePersonAddress2(local.CsePersonAddress, useExport.CsePersonAddress);

    Call(EabReturnKsCountyByZip.Execute, useImport, useExport);

    MoveCsePersonAddress2(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnCabReadCsePersonAddress1()
  {
    var useImport = new FnCabReadCsePersonAddress.Import();
    var useExport = new FnCabReadCsePersonAddress.Export();

    useImport.CsePerson.Number = local.Check.Number;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(FnCabReadCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress, local.CsePersonAddress);
    local.AddressFound.Flag = useExport.AddressFound.Flag;
  }

  private void UseFnCabReadCsePersonAddress2()
  {
    var useImport = new FnCabReadCsePersonAddress.Import();
    var useExport = new FnCabReadCsePersonAddress.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.CsePerson.Number = local.Ar.Number;

    Call(FnCabReadCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress, local.CsePersonAddress);
    local.AddressFound.Flag = useExport.AddressFound.Flag;
  }

  private void UseSiCaseAndCaseUnitAssignment()
  {
    var useImport = new SiCaseAndCaseUnitAssignment.Import();
    var useExport = new SiCaseAndCaseUnitAssignment.Export();

    useImport.Office.SystemGeneratedId = import.Reopen.SystemGeneratedId;
    useImport.Case1.Number = import.Case1.Number;

    Call(SiCaseAndCaseUnitAssignment.Execute, useImport, useExport);
  }

  private void UseSiCheckForDuplicateAddress()
  {
    var useImport = new SiCheckForDuplicateAddress.Import();
    var useExport = new SiCheckForDuplicateAddress.Export();

    useImport.CsePerson.Number = local.Check.Number;
    MoveCsePersonAddress1(local.CsePersonAddress, useImport.CsePersonAddress);

    Call(SiCheckForDuplicateAddress.Execute, useImport, useExport);

    local.DuplicateAddress.Flag = useExport.DuplicateAddress.Flag;
  }

  private void UseSiCreateCsePersonAddress()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    useImport.CsePerson.Number = local.Check.Number;
    MoveCsePersonAddress1(local.CsePersonAddress, useImport.CsePersonAddress);

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void AssociateAppointment()
  {
    System.Diagnostics.Debug.Assert(entities.NewCaseRole.Populated);

    var casNumber = entities.NewCaseRole.CasNumber;
    var cspNumber = entities.NewCaseRole.CspNumber;
    var croType = entities.NewCaseRole.Type1;
    var croId = entities.NewCaseRole.Identifier;

    CheckValid<Appointment>("CroType", croType);
    entities.Appointment.Populated = false;
    Update("AssociateAppointment",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "croType", croType);
        db.SetNullableInt32(command, "croId", croId);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Appointment.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Appointment.CasNumber = casNumber;
    entities.Appointment.CspNumber = cspNumber;
    entities.Appointment.CroType = croType;
    entities.Appointment.CroId = croId;
    entities.Appointment.Populated = true;
  }

  private void AssociateCase()
  {
    var fkCktCasenumb = entities.Case1.Number;

    entities.NewInformationRequest.Populated = false;
    Update("AssociateCase",
      (db, command) =>
      {
        db.SetString(command, "fkCktCasenumb", fkCktCasenumb);
        db.SetInt64(command, "numb", entities.NewInformationRequest.Number);
      });

    entities.NewInformationRequest.FkCktCasenumb = fkCktCasenumb;
    entities.NewInformationRequest.Populated = true;
  }

  private void AssociateGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(entities.NewCaseRole.Populated);

    var casNumber = entities.NewCaseRole.CasNumber;
    var cspNumber = entities.NewCaseRole.CspNumber;
    var croType = entities.NewCaseRole.Type1;
    var croIdentifier = entities.NewCaseRole.Identifier;

    CheckValid<GeneticTest>("CroType", croType);
    entities.GeneticTest.Populated = false;
    Update("AssociateGeneticTest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "croType", croType);
        db.SetNullableInt32(command, "croIdentifier", croIdentifier);
        db.SetInt32(command, "testNumber", entities.GeneticTest.TestNumber);
      });

    entities.GeneticTest.CasNumber = casNumber;
    entities.GeneticTest.CspNumber = cspNumber;
    entities.GeneticTest.CroType = croType;
    entities.GeneticTest.CroIdentifier = croIdentifier;
    entities.GeneticTest.Populated = true;
  }

  private void AssociateGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.NewCaseRole.Populated);

    var casMNumber = entities.NewCaseRole.CasNumber;
    var cspMNumber = entities.NewCaseRole.CspNumber;
    var croMType = entities.NewCaseRole.Type1;
    var croMIdentifier = entities.NewCaseRole.Identifier;

    CheckValid<GeneticTest>("CroMType", croMType);
    entities.GeneticTest.Populated = false;
    Update("AssociateGeneticTest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casMNumber", casMNumber);
        db.SetNullableString(command, "cspMNumber", cspMNumber);
        db.SetNullableString(command, "croMType", croMType);
        db.SetNullableInt32(command, "croMIdentifier", croMIdentifier);
        db.SetInt32(command, "testNumber", entities.GeneticTest.TestNumber);
      });

    entities.GeneticTest.CasMNumber = casMNumber;
    entities.GeneticTest.CspMNumber = cspMNumber;
    entities.GeneticTest.CroMType = croMType;
    entities.GeneticTest.CroMIdentifier = croMIdentifier;
    entities.GeneticTest.Populated = true;
  }

  private void CreateCaseRole()
  {
    var casNumber = entities.Case1.Number;
    var cspNumber = entities.CsePerson.Number;
    var type1 = entities.Old.Type1;
    var identifier = local.GreatestCaseRole.Identifier;
    var startDate = entities.Old.StartDate;
    var endDate = entities.Old.EndDate;
    var onSsInd = entities.Old.OnSsInd;
    var healthInsuranceIndicator = entities.Old.HealthInsuranceIndicator;
    var medicalSupportIndicator = entities.Old.MedicalSupportIndicator;
    var mothersFirstName = entities.Old.MothersFirstName;
    var mothersMiddleInitial = entities.Old.MothersMiddleInitial;
    var fathersLastName = entities.Old.FathersLastName;
    var fathersMiddleInitial = entities.Old.FathersMiddleInitial;
    var fathersFirstName = entities.Old.FathersFirstName;
    var mothersMaidenLastName = entities.Old.MothersMaidenLastName;
    var parentType = entities.Old.ParentType;
    var notifiedDate = entities.Old.NotifiedDate;
    var numberOfChildren = entities.Old.NumberOfChildren;
    var livingWithArIndicator = entities.Old.LivingWithArIndicator;
    var nonpaymentCategory = entities.Old.NonpaymentCategory;
    var contactFirstName = entities.Old.ContactFirstName;
    var contactMiddleInitial = entities.Old.ContactMiddleInitial;
    var contactPhone = entities.Old.ContactPhone;
    var contactLastName = entities.Old.ContactLastName;
    var childCareExpenses = entities.Old.ChildCareExpenses;
    var assignmentDate = entities.Old.AssignmentDate;
    var assignmentTerminationCode = entities.Old.AssignmentTerminationCode;
    var assignmentOfRights = entities.Old.AssignmentOfRights;
    var assignmentTerminatedDt = entities.Old.AssignmentTerminatedDt;
    var absenceReasonCode = entities.Old.AbsenceReasonCode;
    var priorMedicalSupport = entities.Old.PriorMedicalSupport;
    var arWaivedInsurance = entities.Old.ArWaivedInsurance;
    var dateOfEmancipation = entities.Old.DateOfEmancipation;
    var fcAdoptionDisruptionInd = entities.Old.FcAdoptionDisruptionInd;
    var fcApNotified = entities.Old.FcApNotified;
    var fcCincInd = entities.Old.FcCincInd;
    var fcCostOfCare = entities.Old.FcCostOfCare;
    var fcCostOfCareFreq = entities.Old.FcCostOfCareFreq;
    var fcCountyChildRemovedFrom = entities.Old.FcCountyChildRemovedFrom;
    var fcDateOfInitialCustody = entities.Old.FcDateOfInitialCustody;
    var fcInHomeServiceInd = entities.Old.FcInHomeServiceInd;
    var fcIvECaseNumber = entities.Old.FcIvECaseNumber;
    var fcJuvenileCourtOrder = entities.Old.FcJuvenileCourtOrder;
    var fcJuvenileOffenderInd = entities.Old.FcJuvenileOffenderInd;
    var fcLevelOfCare = entities.Old.FcLevelOfCare;
    var fcNextJuvenileCtDt = entities.Old.FcNextJuvenileCtDt;
    var fcOrderEstBy = entities.Old.FcOrderEstBy;
    var fcOtherBenefitInd = entities.Old.FcOtherBenefitInd;
    var fcParentalRights = entities.Old.FcParentalRights;
    var fcPrevPayeeFirstName = entities.Old.FcPrevPayeeFirstName;
    var fcPrevPayeeMiddleInitial = entities.Old.FcPrevPayeeMiddleInitial;
    var fcPlacementDate = entities.Old.FcPlacementDate;
    var fcPlacementName = entities.Old.FcPlacementName;
    var fcPlacementReason = entities.Old.FcPlacementReason;
    var fcPreviousPa = entities.Old.FcPreviousPa;
    var fcPreviousPayeeLastName = entities.Old.FcPreviousPayeeLastName;
    var fcSourceOfFunding = entities.Old.FcSourceOfFunding;
    var fcSrsPayee = entities.Old.FcSrsPayee;
    var fcSsa = entities.Old.FcSsa;
    var fcSsi = entities.Old.FcSsi;
    var fcVaInd = entities.Old.FcVaInd;
    var fcWardsAccount = entities.Old.FcWardsAccount;
    var fcZebInd = entities.Old.FcZebInd;
    var over18AndInSchool = entities.Old.Over18AndInSchool;
    var residesWithArIndicator = entities.Old.ResidesWithArIndicator;
    var specialtyArea = entities.Old.SpecialtyArea;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var confirmedType = entities.Old.ConfirmedType;
    var relToAr = entities.Old.RelToAr;
    var arChgProcReqInd = entities.Old.ArChgProcReqInd;
    var arChgProcessedDate = entities.Old.ArChgProcessedDate;
    var arInvalidInd = entities.Old.ArInvalidInd;
    var note = entities.Old.Note;

    CheckValid<CaseRole>("Type1", type1);
    CheckValid<CaseRole>("ParentType", parentType);
    CheckValid<CaseRole>("LivingWithArIndicator", livingWithArIndicator);
    CheckValid<CaseRole>("ResidesWithArIndicator", residesWithArIndicator);
    CheckValid<CaseRole>("SpecialtyArea", specialtyArea);
    entities.NewCaseRole.Populated = false;
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
          
        db.SetNullableString(command, "mothersFirstNm", mothersFirstName);
        db.SetNullableString(command, "mothersMidInit", mothersMiddleInitial);
        db.SetNullableString(command, "fathersLastName", fathersLastName);
        db.SetNullableString(command, "fathersMidInit", fathersMiddleInitial);
        db.SetNullableString(command, "fathersFirstName", fathersFirstName);
        db.
          SetNullableString(command, "motherMaidenLast", mothersMaidenLastName);
          
        db.SetNullableString(command, "parentType", parentType);
        db.SetNullableDate(command, "notifiedDate", notifiedDate);
        db.SetNullableInt32(command, "numberOfChildren", numberOfChildren);
        db.SetNullableString(command, "livingWithArInd", livingWithArIndicator);
        db.SetNullableString(command, "nonpaymentCat", nonpaymentCategory);
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
        db.SetNullableString(command, "absenceReasonCd", absenceReasonCode);
        db.SetNullableString(command, "bcFathersMi", "");
        db.SetNullableString(command, "bcFatherFirstNm", "");
        db.SetNullableDecimal(command, "priorMedicalSupp", priorMedicalSupport);
        db.SetNullableString(command, "arWaivedIns", arWaivedInsurance);
        db.SetNullableString(command, "bcFatherLastNm", "");
        db.SetNullableDate(command, "emancipationDt", dateOfEmancipation);
        db.
          SetNullableString(command, "fcAdoptDisrupt", fcAdoptionDisruptionInd);
          
        db.SetNullableString(command, "fcApNotified", fcApNotified);
        db.SetNullableString(command, "fcCincInd", fcCincInd);
        db.SetNullableDecimal(command, "fcCostOfCare", fcCostOfCare);
        db.SetNullableString(command, "fcCareCostFreq", fcCostOfCareFreq);
        db.SetNullableString(
          command, "fcCountyRemFrom", fcCountyChildRemovedFrom);
        db.SetNullableDate(command, "fcInitCustodyDt", fcDateOfInitialCustody);
        db.SetNullableString(command, "fcInHmServInd", fcInHomeServiceInd);
        db.SetNullableString(command, "fcIvECaseNo", fcIvECaseNumber);
        db.SetNullableString(command, "fcJvCrtOrder", fcJuvenileCourtOrder);
        db.SetNullableString(command, "fcJvOffenderInd", fcJuvenileOffenderInd);
        db.SetNullableString(command, "fcLevelOfCare", fcLevelOfCare);
        db.SetNullableDate(command, "fcNextJvCtDt", fcNextJuvenileCtDt);
        db.SetNullableString(command, "fcOrderEstBy", fcOrderEstBy);
        db.SetNullableString(command, "fcOtherBenInd", fcOtherBenefitInd);
        db.SetNullableString(command, "fcParentalRights", fcParentalRights);
        db.SetNullableString(command, "fcPrvPayFrstNm", fcPrevPayeeFirstName);
        db.SetNullableString(command, "fcPrvPayMi", fcPrevPayeeMiddleInitial);
        db.SetNullableDate(command, "fcPlacementDate", fcPlacementDate);
        db.SetNullableString(command, "fcPlacementName", fcPlacementName);
        db.SetNullableString(command, "fcPlacementRsn", fcPlacementReason);
        db.SetNullableString(command, "fcPreviousPa", fcPreviousPa);
        db.
          SetNullableString(command, "fcPrvPayLastNm", fcPreviousPayeeLastName);
          
        db.SetNullableString(command, "fcSrceOfFunding", fcSourceOfFunding);
        db.SetNullableString(command, "fcSrsPayee", fcSrsPayee);
        db.SetNullableString(command, "fcSsa", fcSsa);
        db.SetNullableString(command, "fcSsi", fcSsi);
        db.SetNullableString(command, "fcVaInd", fcVaInd);
        db.SetNullableString(command, "fcWardsAccount", fcWardsAccount);
        db.SetNullableString(command, "fcZebInd", fcZebInd);
        db.SetNullableString(command, "inSchoolOver18", over18AndInSchool);
        db.
          SetNullableString(command, "resideWithArInd", residesWithArIndicator);
          
        db.SetNullableString(command, "specialtyArea", specialtyArea);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", lastUpdatedTimestamp);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetNullableString(command, "confirmedType", confirmedType);
        db.SetNullableString(command, "relToAr", relToAr);
        db.SetNullableString(command, "arChgPrcReqInd", arChgProcReqInd);
        db.SetNullableDate(command, "arChgProcDt", arChgProcessedDate);
        db.SetNullableString(command, "arInvalidInd", arInvalidInd);
        db.SetNullableString(command, "note", note);
      });

    entities.NewCaseRole.CasNumber = casNumber;
    entities.NewCaseRole.CspNumber = cspNumber;
    entities.NewCaseRole.Type1 = type1;
    entities.NewCaseRole.Identifier = identifier;
    entities.NewCaseRole.StartDate = startDate;
    entities.NewCaseRole.EndDate = endDate;
    entities.NewCaseRole.OnSsInd = onSsInd;
    entities.NewCaseRole.HealthInsuranceIndicator = healthInsuranceIndicator;
    entities.NewCaseRole.MedicalSupportIndicator = medicalSupportIndicator;
    entities.NewCaseRole.MothersFirstName = mothersFirstName;
    entities.NewCaseRole.MothersMiddleInitial = mothersMiddleInitial;
    entities.NewCaseRole.FathersLastName = fathersLastName;
    entities.NewCaseRole.FathersMiddleInitial = fathersMiddleInitial;
    entities.NewCaseRole.FathersFirstName = fathersFirstName;
    entities.NewCaseRole.MothersMaidenLastName = mothersMaidenLastName;
    entities.NewCaseRole.ParentType = parentType;
    entities.NewCaseRole.NotifiedDate = notifiedDate;
    entities.NewCaseRole.NumberOfChildren = numberOfChildren;
    entities.NewCaseRole.LivingWithArIndicator = livingWithArIndicator;
    entities.NewCaseRole.NonpaymentCategory = nonpaymentCategory;
    entities.NewCaseRole.ContactFirstName = contactFirstName;
    entities.NewCaseRole.ContactMiddleInitial = contactMiddleInitial;
    entities.NewCaseRole.ContactPhone = contactPhone;
    entities.NewCaseRole.ContactLastName = contactLastName;
    entities.NewCaseRole.ChildCareExpenses = childCareExpenses;
    entities.NewCaseRole.AssignmentDate = assignmentDate;
    entities.NewCaseRole.AssignmentTerminationCode = assignmentTerminationCode;
    entities.NewCaseRole.AssignmentOfRights = assignmentOfRights;
    entities.NewCaseRole.AssignmentTerminatedDt = assignmentTerminatedDt;
    entities.NewCaseRole.AbsenceReasonCode = absenceReasonCode;
    entities.NewCaseRole.PriorMedicalSupport = priorMedicalSupport;
    entities.NewCaseRole.ArWaivedInsurance = arWaivedInsurance;
    entities.NewCaseRole.DateOfEmancipation = dateOfEmancipation;
    entities.NewCaseRole.FcAdoptionDisruptionInd = fcAdoptionDisruptionInd;
    entities.NewCaseRole.FcApNotified = fcApNotified;
    entities.NewCaseRole.FcCincInd = fcCincInd;
    entities.NewCaseRole.FcCostOfCare = fcCostOfCare;
    entities.NewCaseRole.FcCostOfCareFreq = fcCostOfCareFreq;
    entities.NewCaseRole.FcCountyChildRemovedFrom = fcCountyChildRemovedFrom;
    entities.NewCaseRole.FcDateOfInitialCustody = fcDateOfInitialCustody;
    entities.NewCaseRole.FcInHomeServiceInd = fcInHomeServiceInd;
    entities.NewCaseRole.FcIvECaseNumber = fcIvECaseNumber;
    entities.NewCaseRole.FcJuvenileCourtOrder = fcJuvenileCourtOrder;
    entities.NewCaseRole.FcJuvenileOffenderInd = fcJuvenileOffenderInd;
    entities.NewCaseRole.FcLevelOfCare = fcLevelOfCare;
    entities.NewCaseRole.FcNextJuvenileCtDt = fcNextJuvenileCtDt;
    entities.NewCaseRole.FcOrderEstBy = fcOrderEstBy;
    entities.NewCaseRole.FcOtherBenefitInd = fcOtherBenefitInd;
    entities.NewCaseRole.FcParentalRights = fcParentalRights;
    entities.NewCaseRole.FcPrevPayeeFirstName = fcPrevPayeeFirstName;
    entities.NewCaseRole.FcPrevPayeeMiddleInitial = fcPrevPayeeMiddleInitial;
    entities.NewCaseRole.FcPlacementDate = fcPlacementDate;
    entities.NewCaseRole.FcPlacementName = fcPlacementName;
    entities.NewCaseRole.FcPlacementReason = fcPlacementReason;
    entities.NewCaseRole.FcPreviousPa = fcPreviousPa;
    entities.NewCaseRole.FcPreviousPayeeLastName = fcPreviousPayeeLastName;
    entities.NewCaseRole.FcSourceOfFunding = fcSourceOfFunding;
    entities.NewCaseRole.FcSrsPayee = fcSrsPayee;
    entities.NewCaseRole.FcSsa = fcSsa;
    entities.NewCaseRole.FcSsi = fcSsi;
    entities.NewCaseRole.FcVaInd = fcVaInd;
    entities.NewCaseRole.FcWardsAccount = fcWardsAccount;
    entities.NewCaseRole.FcZebInd = fcZebInd;
    entities.NewCaseRole.Over18AndInSchool = over18AndInSchool;
    entities.NewCaseRole.ResidesWithArIndicator = residesWithArIndicator;
    entities.NewCaseRole.SpecialtyArea = specialtyArea;
    entities.NewCaseRole.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NewCaseRole.LastUpdatedBy = lastUpdatedBy;
    entities.NewCaseRole.CreatedTimestamp = lastUpdatedTimestamp;
    entities.NewCaseRole.CreatedBy = lastUpdatedBy;
    entities.NewCaseRole.ConfirmedType = confirmedType;
    entities.NewCaseRole.RelToAr = relToAr;
    entities.NewCaseRole.ArChgProcReqInd = arChgProcReqInd;
    entities.NewCaseRole.ArChgProcessedDate = arChgProcessedDate;
    entities.NewCaseRole.ArInvalidInd = arInvalidInd;
    entities.NewCaseRole.Note = note;
    entities.NewCaseRole.Populated = true;
  }

  private bool ReadAppointment()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);
    entities.Appointment.Populated = false;

    return Read("ReadAppointment",
      (db, command) =>
      {
        db.SetNullableInt32(command, "croId", entities.Old.Identifier);
        db.SetNullableString(command, "croType", entities.Old.Type1);
        db.SetNullableString(command, "cspNumber", entities.Old.CspNumber);
        db.SetNullableString(command, "casNumber", entities.Old.CasNumber);
      },
      (db, reader) =>
      {
        entities.Appointment.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.Appointment.AppTstamp = db.GetNullableDateTime(reader, 1);
        entities.Appointment.CasNumber = db.GetNullableString(reader, 2);
        entities.Appointment.CspNumber = db.GetNullableString(reader, 3);
        entities.Appointment.CroType = db.GetNullableString(reader, 4);
        entities.Appointment.CroId = db.GetNullableInt32(reader, 5);
        entities.Appointment.Populated = true;
        CheckValid<Appointment>("CroType", entities.Appointment.CroType);
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
        entities.Case1.InformationRequestNumber =
          db.GetNullableInt64(reader, 2);
        entities.Case1.Status = db.GetNullableString(reader, 3);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 4);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 5);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 6);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 7);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 9);
        entities.Case1.Note = db.GetNullableString(reader, 10);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.Old.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(
          command, "caseRoleId", local.Group.Item.DetCaseRole.Identifier);
        db.SetString(command, "type", local.Group.Item.DetCaseRole.Type1);
      },
      (db, reader) =>
      {
        entities.Old.CasNumber = db.GetString(reader, 0);
        entities.Old.CspNumber = db.GetString(reader, 1);
        entities.Old.Type1 = db.GetString(reader, 2);
        entities.Old.Identifier = db.GetInt32(reader, 3);
        entities.Old.StartDate = db.GetNullableDate(reader, 4);
        entities.Old.EndDate = db.GetNullableDate(reader, 5);
        entities.Old.OnSsInd = db.GetNullableString(reader, 6);
        entities.Old.HealthInsuranceIndicator = db.GetNullableString(reader, 7);
        entities.Old.MedicalSupportIndicator = db.GetNullableString(reader, 8);
        entities.Old.MothersFirstName = db.GetNullableString(reader, 9);
        entities.Old.MothersMiddleInitial = db.GetNullableString(reader, 10);
        entities.Old.FathersLastName = db.GetNullableString(reader, 11);
        entities.Old.FathersMiddleInitial = db.GetNullableString(reader, 12);
        entities.Old.FathersFirstName = db.GetNullableString(reader, 13);
        entities.Old.MothersMaidenLastName = db.GetNullableString(reader, 14);
        entities.Old.ParentType = db.GetNullableString(reader, 15);
        entities.Old.NotifiedDate = db.GetNullableDate(reader, 16);
        entities.Old.NumberOfChildren = db.GetNullableInt32(reader, 17);
        entities.Old.LivingWithArIndicator = db.GetNullableString(reader, 18);
        entities.Old.NonpaymentCategory = db.GetNullableString(reader, 19);
        entities.Old.ContactFirstName = db.GetNullableString(reader, 20);
        entities.Old.ContactMiddleInitial = db.GetNullableString(reader, 21);
        entities.Old.ContactPhone = db.GetNullableString(reader, 22);
        entities.Old.ContactLastName = db.GetNullableString(reader, 23);
        entities.Old.ChildCareExpenses = db.GetNullableDecimal(reader, 24);
        entities.Old.AssignmentDate = db.GetNullableDate(reader, 25);
        entities.Old.AssignmentTerminationCode =
          db.GetNullableString(reader, 26);
        entities.Old.AssignmentOfRights = db.GetNullableString(reader, 27);
        entities.Old.AssignmentTerminatedDt = db.GetNullableDate(reader, 28);
        entities.Old.AbsenceReasonCode = db.GetNullableString(reader, 29);
        entities.Old.PriorMedicalSupport = db.GetNullableDecimal(reader, 30);
        entities.Old.ArWaivedInsurance = db.GetNullableString(reader, 31);
        entities.Old.DateOfEmancipation = db.GetNullableDate(reader, 32);
        entities.Old.FcAdoptionDisruptionInd = db.GetNullableString(reader, 33);
        entities.Old.FcApNotified = db.GetNullableString(reader, 34);
        entities.Old.FcCincInd = db.GetNullableString(reader, 35);
        entities.Old.FcCostOfCare = db.GetNullableDecimal(reader, 36);
        entities.Old.FcCostOfCareFreq = db.GetNullableString(reader, 37);
        entities.Old.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 38);
        entities.Old.FcDateOfInitialCustody = db.GetNullableDate(reader, 39);
        entities.Old.FcInHomeServiceInd = db.GetNullableString(reader, 40);
        entities.Old.FcIvECaseNumber = db.GetNullableString(reader, 41);
        entities.Old.FcJuvenileCourtOrder = db.GetNullableString(reader, 42);
        entities.Old.FcJuvenileOffenderInd = db.GetNullableString(reader, 43);
        entities.Old.FcLevelOfCare = db.GetNullableString(reader, 44);
        entities.Old.FcNextJuvenileCtDt = db.GetNullableDate(reader, 45);
        entities.Old.FcOrderEstBy = db.GetNullableString(reader, 46);
        entities.Old.FcOtherBenefitInd = db.GetNullableString(reader, 47);
        entities.Old.FcParentalRights = db.GetNullableString(reader, 48);
        entities.Old.FcPrevPayeeFirstName = db.GetNullableString(reader, 49);
        entities.Old.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 50);
        entities.Old.FcPlacementDate = db.GetNullableDate(reader, 51);
        entities.Old.FcPlacementName = db.GetNullableString(reader, 52);
        entities.Old.FcPlacementReason = db.GetNullableString(reader, 53);
        entities.Old.FcPreviousPa = db.GetNullableString(reader, 54);
        entities.Old.FcPreviousPayeeLastName = db.GetNullableString(reader, 55);
        entities.Old.FcSourceOfFunding = db.GetNullableString(reader, 56);
        entities.Old.FcSrsPayee = db.GetNullableString(reader, 57);
        entities.Old.FcSsa = db.GetNullableString(reader, 58);
        entities.Old.FcSsi = db.GetNullableString(reader, 59);
        entities.Old.FcVaInd = db.GetNullableString(reader, 60);
        entities.Old.FcWardsAccount = db.GetNullableString(reader, 61);
        entities.Old.FcZebInd = db.GetNullableString(reader, 62);
        entities.Old.Over18AndInSchool = db.GetNullableString(reader, 63);
        entities.Old.ResidesWithArIndicator = db.GetNullableString(reader, 64);
        entities.Old.SpecialtyArea = db.GetNullableString(reader, 65);
        entities.Old.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 66);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 67);
        entities.Old.CreatedTimestamp = db.GetDateTime(reader, 68);
        entities.Old.CreatedBy = db.GetString(reader, 69);
        entities.Old.ConfirmedType = db.GetNullableString(reader, 70);
        entities.Old.RelToAr = db.GetNullableString(reader, 71);
        entities.Old.ArChgProcReqInd = db.GetNullableString(reader, 72);
        entities.Old.ArChgProcessedDate = db.GetNullableDate(reader, 73);
        entities.Old.ArInvalidInd = db.GetNullableString(reader, 74);
        entities.Old.Note = db.GetNullableString(reader, 75);
        entities.Old.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Old.Type1);
        CheckValid<CaseRole>("ParentType", entities.Old.ParentType);
        CheckValid<CaseRole>("LivingWithArIndicator",
          entities.Old.LivingWithArIndicator);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.Old.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea", entities.Old.SpecialtyArea);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.SearchCaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.SearchCaseRole.CasNumber = db.GetString(reader, 0);
        entities.SearchCaseRole.CspNumber = db.GetString(reader, 1);
        entities.SearchCaseRole.Type1 = db.GetString(reader, 2);
        entities.SearchCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.SearchCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.SearchCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.SearchCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.SearchCaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.SearchCaseRole.RelToAr = db.GetNullableString(reader, 8);
        entities.SearchCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.SearchCaseRole.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson3()
  {
    entities.SearchCaseRole.Populated = false;
    entities.SearchCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate",
          local.CaseClosedDate.StatusDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.SearchCaseRole.CasNumber = db.GetString(reader, 0);
        entities.SearchCaseRole.CspNumber = db.GetString(reader, 1);
        entities.SearchCsePerson.Number = db.GetString(reader, 1);
        entities.SearchCaseRole.Type1 = db.GetString(reader, 2);
        entities.SearchCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.SearchCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.SearchCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.SearchCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.SearchCaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.SearchCaseRole.RelToAr = db.GetNullableString(reader, 8);
        entities.SearchCsePerson.Type1 = db.GetString(reader, 9);
        entities.SearchCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 10);
        entities.SearchCaseRole.Populated = true;
        entities.SearchCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.SearchCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.SearchCsePerson.Type1);

        return true;
      });
  }

  private bool ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseUnit.CasNo = db.GetString(reader, 7);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 8);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 9);
        entities.CaseUnit.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoChild", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "closureDate",
          local.CaseClosedDate.StatusDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseUnit.CasNo = db.GetString(reader, 7);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 8);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 9);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit3()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit3",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(command, "startDate", local.Curent.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseUnit.CasNo = db.GetString(reader, 7);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 8);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 9);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Group.Item.DetCsePerson.Number);
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
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoAp ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadGeneticTest1()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);
    entities.GeneticTest.Populated = false;

    return Read("ReadGeneticTest1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "croIdentifier", entities.Old.Identifier);
        db.SetNullableString(command, "croType", entities.Old.Type1);
        db.SetNullableString(command, "casNumber", entities.Old.CasNumber);
        db.SetNullableString(command, "cspNumber", entities.Old.CspNumber);
      },
      (db, reader) =>
      {
        entities.GeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.GeneticTest.CasNumber = db.GetNullableString(reader, 1);
        entities.GeneticTest.CspNumber = db.GetNullableString(reader, 2);
        entities.GeneticTest.CroType = db.GetNullableString(reader, 3);
        entities.GeneticTest.CroIdentifier = db.GetNullableInt32(reader, 4);
        entities.GeneticTest.CasMNumber = db.GetNullableString(reader, 5);
        entities.GeneticTest.CspMNumber = db.GetNullableString(reader, 6);
        entities.GeneticTest.CroMType = db.GetNullableString(reader, 7);
        entities.GeneticTest.CroMIdentifier = db.GetNullableInt32(reader, 8);
        entities.GeneticTest.CasANumber = db.GetNullableString(reader, 9);
        entities.GeneticTest.CspANumber = db.GetNullableString(reader, 10);
        entities.GeneticTest.CroAType = db.GetNullableString(reader, 11);
        entities.GeneticTest.CroAIdentifier = db.GetNullableInt32(reader, 12);
        entities.GeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.GeneticTest.CroType);
        CheckValid<GeneticTest>("CroMType", entities.GeneticTest.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.GeneticTest.CroAType);
      });
  }

  private bool ReadGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);
    entities.GeneticTest.Populated = false;

    return Read("ReadGeneticTest2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "croAIdentifier", entities.Old.Identifier);
        db.SetNullableString(command, "croAType", entities.Old.Type1);
        db.SetNullableString(command, "casANumber", entities.Old.CasNumber);
        db.SetNullableString(command, "cspANumber", entities.Old.CspNumber);
      },
      (db, reader) =>
      {
        entities.GeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.GeneticTest.CasNumber = db.GetNullableString(reader, 1);
        entities.GeneticTest.CspNumber = db.GetNullableString(reader, 2);
        entities.GeneticTest.CroType = db.GetNullableString(reader, 3);
        entities.GeneticTest.CroIdentifier = db.GetNullableInt32(reader, 4);
        entities.GeneticTest.CasMNumber = db.GetNullableString(reader, 5);
        entities.GeneticTest.CspMNumber = db.GetNullableString(reader, 6);
        entities.GeneticTest.CroMType = db.GetNullableString(reader, 7);
        entities.GeneticTest.CroMIdentifier = db.GetNullableInt32(reader, 8);
        entities.GeneticTest.CasANumber = db.GetNullableString(reader, 9);
        entities.GeneticTest.CspANumber = db.GetNullableString(reader, 10);
        entities.GeneticTest.CroAType = db.GetNullableString(reader, 11);
        entities.GeneticTest.CroAIdentifier = db.GetNullableInt32(reader, 12);
        entities.GeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.GeneticTest.CroType);
        CheckValid<GeneticTest>("CroMType", entities.GeneticTest.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.GeneticTest.CroAType);
      });
  }

  private bool ReadGeneticTest3()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);
    entities.GeneticTest.Populated = false;

    return Read("ReadGeneticTest3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "croMIdentifier", entities.Old.Identifier);
        db.SetNullableString(command, "croMType", entities.Old.Type1);
        db.SetNullableString(command, "casMNumber", entities.Old.CasNumber);
        db.SetNullableString(command, "cspMNumber", entities.Old.CspNumber);
      },
      (db, reader) =>
      {
        entities.GeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.GeneticTest.CasNumber = db.GetNullableString(reader, 1);
        entities.GeneticTest.CspNumber = db.GetNullableString(reader, 2);
        entities.GeneticTest.CroType = db.GetNullableString(reader, 3);
        entities.GeneticTest.CroIdentifier = db.GetNullableInt32(reader, 4);
        entities.GeneticTest.CasMNumber = db.GetNullableString(reader, 5);
        entities.GeneticTest.CspMNumber = db.GetNullableString(reader, 6);
        entities.GeneticTest.CroMType = db.GetNullableString(reader, 7);
        entities.GeneticTest.CroMIdentifier = db.GetNullableInt32(reader, 8);
        entities.GeneticTest.CasANumber = db.GetNullableString(reader, 9);
        entities.GeneticTest.CspANumber = db.GetNullableString(reader, 10);
        entities.GeneticTest.CroAType = db.GetNullableString(reader, 11);
        entities.GeneticTest.CroAIdentifier = db.GetNullableInt32(reader, 12);
        entities.GeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.GeneticTest.CroType);
        CheckValid<GeneticTest>("CroMType", entities.GeneticTest.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.GeneticTest.CroAType);
      });
  }

  private bool ReadInformationRequest1()
  {
    entities.NewInformationRequest.Populated = false;

    return Read("ReadInformationRequest1",
      (db, command) =>
      {
        db.SetInt64(command, "numb", import.InformationRequest.Number);
      },
      (db, reader) =>
      {
        entities.NewInformationRequest.Number = db.GetInt64(reader, 0);
        entities.NewInformationRequest.ApplicantLastName =
          db.GetNullableString(reader, 1);
        entities.NewInformationRequest.ApplicantFirstName =
          db.GetNullableString(reader, 2);
        entities.NewInformationRequest.ApplicantMiddleInitial =
          db.GetNullableString(reader, 3);
        entities.NewInformationRequest.ApplicantStreet1 =
          db.GetNullableString(reader, 4);
        entities.NewInformationRequest.ApplicantStreet2 =
          db.GetNullableString(reader, 5);
        entities.NewInformationRequest.ApplicantCity =
          db.GetNullableString(reader, 6);
        entities.NewInformationRequest.ApplicantState =
          db.GetNullableString(reader, 7);
        entities.NewInformationRequest.ApplicantZip5 =
          db.GetNullableString(reader, 8);
        entities.NewInformationRequest.ApplicantZip4 =
          db.GetNullableString(reader, 9);
        entities.NewInformationRequest.ApplicantZip3 =
          db.GetNullableString(reader, 10);
        entities.NewInformationRequest.ApplicantPhone =
          db.GetNullableInt32(reader, 11);
        entities.NewInformationRequest.Type1 = db.GetString(reader, 12);
        entities.NewInformationRequest.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.NewInformationRequest.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.NewInformationRequest.ApplicationProcessedInd =
          db.GetNullableString(reader, 15);
        entities.NewInformationRequest.ReopenReasonType =
          db.GetNullableString(reader, 16);
        entities.NewInformationRequest.MiscellaneousReason =
          db.GetNullableString(reader, 17);
        entities.NewInformationRequest.FkCktCasenumb = db.GetString(reader, 18);
        entities.NewInformationRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInformationRequest2()
  {
    entities.InformationRequest.Populated = false;

    return ReadEach("ReadInformationRequest2",
      (db, command) =>
      {
        db.SetString(command, "fkCktCasenumb", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InformationRequest.Number = db.GetInt64(reader, 0);
        entities.InformationRequest.ApplicantLastName =
          db.GetNullableString(reader, 1);
        entities.InformationRequest.ApplicantFirstName =
          db.GetNullableString(reader, 2);
        entities.InformationRequest.ApplicantMiddleInitial =
          db.GetNullableString(reader, 3);
        entities.InformationRequest.ApplicantStreet1 =
          db.GetNullableString(reader, 4);
        entities.InformationRequest.ApplicantStreet2 =
          db.GetNullableString(reader, 5);
        entities.InformationRequest.ApplicantCity =
          db.GetNullableString(reader, 6);
        entities.InformationRequest.ApplicantState =
          db.GetNullableString(reader, 7);
        entities.InformationRequest.ApplicantZip5 =
          db.GetNullableString(reader, 8);
        entities.InformationRequest.Type1 = db.GetString(reader, 9);
        entities.InformationRequest.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.InformationRequest.ReopenReasonType =
          db.GetNullableString(reader, 11);
        entities.InformationRequest.FkCktCasenumb = db.GetString(reader, 12);
        entities.InformationRequest.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 6);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "croId", entities.Old.Identifier);
        db.SetNullableString(command, "croType", entities.Old.Type1);
        db.SetNullableString(command, "cspNumber", entities.Old.CspNumber);
        db.SetNullableString(command, "casNumber", entities.Old.CasNumber);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 6);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private void UpdateCase1()
  {
    var status = "O";
    var statusDate = local.Curent.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var closureLetterDate = local.Initialized.Date;

    entities.Case1.Populated = false;
    Update("UpdateCase1",
      (db, command) =>
      {
        db.SetNullableString(command, "closureReason", "");
        db.SetNullableString(command, "status", status);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableDate(command, "cseOpenDate", statusDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "closureLetrDate", closureLetterDate);
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.ClosureReason = "";
    entities.Case1.Status = status;
    entities.Case1.StatusDate = statusDate;
    entities.Case1.CseOpenDate = statusDate;
    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.ClosureLetterDate = closureLetterDate;
    entities.Case1.Populated = true;
  }

  private void UpdateCase2()
  {
    var informationRequestNumber = import.InformationRequest.Number;

    entities.Case1.Populated = false;
    Update("UpdateCase2",
      (db, command) =>
      {
        db.SetNullableInt64(command, "infoRequestNo", informationRequestNumber);
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.InformationRequestNumber = informationRequestNumber;
    entities.Case1.Populated = true;
  }

  private void UpdateCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.Old.Populated);

    var startDate = local.Curent.Date;
    var endDate = local.Max.Date;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.Old.Populated = false;
    Update("UpdateCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "casNumber", entities.Old.CasNumber);
        db.SetString(command, "cspNumber", entities.Old.CspNumber);
        db.SetString(command, "type", entities.Old.Type1);
        db.SetInt32(command, "caseRoleId", entities.Old.Identifier);
      });

    entities.Old.StartDate = startDate;
    entities.Old.EndDate = endDate;
    entities.Old.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Old.LastUpdatedBy = lastUpdatedBy;
    entities.Old.Populated = true;
  }

  private void UpdateCaseUnit()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var startDate = local.Curent.Date;
    var closureDate = local.Max.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnit.Populated = false;
    Update("UpdateCaseUnit",
      (db, command) =>
      {
        db.SetDate(command, "startDate", startDate);
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", "");
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });

    entities.CaseUnit.StartDate = startDate;
    entities.CaseUnit.ClosureDate = closureDate;
    entities.CaseUnit.ClosureReasonCode = "";
    entities.CaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseUnit.Populated = true;
  }

  private void UpdateInformationRequest()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Curent.Timestamp;
    var applicationProcessedInd = "Y";

    entities.NewInformationRequest.Populated = false;
    Update("UpdateInformationRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "applProcInd", applicationProcessedInd);
        db.SetInt64(command, "numb", entities.NewInformationRequest.Number);
      });

    entities.NewInformationRequest.LastUpdatedBy = lastUpdatedBy;
    entities.NewInformationRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NewInformationRequest.ApplicationProcessedInd =
      applicationProcessedInd;
    entities.NewInformationRequest.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of Reopen.
    /// </summary>
    [JsonPropertyName("reopen")]
    public Office Reopen
    {
      get => reopen ??= new();
      set => reopen = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private InterstateCase interstateCase;
    private PaReferral paReferral;
    private InformationRequest informationRequest;
    private Office reopen;
    private Case1 case1;
    private CsePersonsWorkSet ar;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of GoToInrdReopen.
    /// </summary>
    [JsonPropertyName("goToInrdReopen")]
    public WorkArea GoToInrdReopen
    {
      get => goToInrdReopen ??= new();
      set => goToInrdReopen = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private CsePersonsWorkSet ar;
    private WorkArea goToInrdReopen;
    private InformationRequest informationRequest;
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
      /// A value of DetCsePerson.
      /// </summary>
      [JsonPropertyName("detCsePerson")]
      public CsePerson DetCsePerson
      {
        get => detCsePerson ??= new();
        set => detCsePerson = value;
      }

      /// <summary>
      /// A value of DetCaseRole.
      /// </summary>
      [JsonPropertyName("detCaseRole")]
      public CaseRole DetCaseRole
      {
        get => detCaseRole ??= new();
        set => detCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePerson detCsePerson;
      private CaseRole detCaseRole;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public CsePersonAddress Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public CsePerson Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// A value of DuplicateAddress.
    /// </summary>
    [JsonPropertyName("duplicateAddress")]
    public Common DuplicateAddress
    {
      get => duplicateAddress ??= new();
      set => duplicateAddress = value;
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
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
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
    /// A value of GreatestCaseUnit.
    /// </summary>
    [JsonPropertyName("greatestCaseUnit")]
    public CaseUnit GreatestCaseUnit
    {
      get => greatestCaseUnit ??= new();
      set => greatestCaseUnit = value;
    }

    /// <summary>
    /// A value of GreatestCaseRole.
    /// </summary>
    [JsonPropertyName("greatestCaseRole")]
    public CaseRole GreatestCaseRole
    {
      get => greatestCaseRole ??= new();
      set => greatestCaseRole = value;
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
    /// A value of CasreopenpatestGenerated.
    /// </summary>
    [JsonPropertyName("casreopenpatestGenerated")]
    public Common CasreopenpatestGenerated
    {
      get => casreopenpatestGenerated ??= new();
      set => casreopenpatestGenerated = value;
    }

    /// <summary>
    /// A value of CasreopenpatunkGenerated.
    /// </summary>
    [JsonPropertyName("casreopenpatunkGenerated")]
    public Common CasreopenpatunkGenerated
    {
      get => casreopenpatunkGenerated ??= new();
      set => casreopenpatunkGenerated = value;
    }

    /// <summary>
    /// A value of CasreopennolocGenerated.
    /// </summary>
    [JsonPropertyName("casreopennolocGenerated")]
    public Common CasreopennolocGenerated
    {
      get => casreopennolocGenerated ??= new();
      set => casreopennolocGenerated = value;
    }

    /// <summary>
    /// A value of CasreopenwthoblGenerated.
    /// </summary>
    [JsonPropertyName("casreopenwthoblGenerated")]
    public Common CasreopenwthoblGenerated
    {
      get => casreopenwthoblGenerated ??= new();
      set => casreopenwthoblGenerated = value;
    }

    /// <summary>
    /// A value of Paternity.
    /// </summary>
    [JsonPropertyName("paternity")]
    public Common Paternity
    {
      get => paternity ??= new();
      set => paternity = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Common Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Locate.
    /// </summary>
    [JsonPropertyName("locate")]
    public Common Locate
    {
      get => locate ??= new();
      set => locate = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public Common CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of CaseClosedDate.
    /// </summary>
    [JsonPropertyName("caseClosedDate")]
    public Case1 CaseClosedDate
    {
      get => caseClosedDate ??= new();
      set => caseClosedDate = value;
    }

    /// <summary>
    /// A value of Curent.
    /// </summary>
    [JsonPropertyName("curent")]
    public DateWorkArea Curent
    {
      get => curent ??= new();
      set => curent = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of AtLeastOneChReopened.
    /// </summary>
    [JsonPropertyName("atLeastOneChReopened")]
    public Common AtLeastOneChReopened
    {
      get => atLeastOneChReopened ??= new();
      set => atLeastOneChReopened = value;
    }

    /// <summary>
    /// A value of AtLeastOneChNotReopen.
    /// </summary>
    [JsonPropertyName("atLeastOneChNotReopen")]
    public Common AtLeastOneChNotReopen
    {
      get => atLeastOneChNotReopen ??= new();
      set => atLeastOneChNotReopen = value;
    }

    /// <summary>
    /// A value of RelToArIsCh.
    /// </summary>
    [JsonPropertyName("relToArIsCh")]
    public Common RelToArIsCh
    {
      get => relToArIsCh ??= new();
      set => relToArIsCh = value;
    }

    /// <summary>
    /// A value of ArFound.
    /// </summary>
    [JsonPropertyName("arFound")]
    public Common ArFound
    {
      get => arFound ??= new();
      set => arFound = value;
    }

    /// <summary>
    /// A value of ActiveCases.
    /// </summary>
    [JsonPropertyName("activeCases")]
    public Common ActiveCases
    {
      get => activeCases ??= new();
      set => activeCases = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of AddressFound.
    /// </summary>
    [JsonPropertyName("addressFound")]
    public Common AddressFound
    {
      get => addressFound ??= new();
      set => addressFound = value;
    }

    private CsePersonAddress compare;
    private CsePerson check;
    private Common duplicateAddress;
    private CsePersonsWorkSet csePersonsWorkSet;
    private InformationRequest informationRequest;
    private Common recordCount;
    private CaseUnit greatestCaseUnit;
    private CaseRole greatestCaseRole;
    private DateWorkArea initialized;
    private Common casreopenpatestGenerated;
    private Common casreopenpatunkGenerated;
    private Common casreopennolocGenerated;
    private Common casreopenwthoblGenerated;
    private Common paternity;
    private Common obligation;
    private Common locate;
    private Common caseUnit;
    private Case1 caseClosedDate;
    private DateWorkArea curent;
    private Array<GroupGroup> group;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Common atLeastOneChReopened;
    private Common atLeastOneChNotReopen;
    private Common relToArIsCh;
    private Common arFound;
    private Common activeCases;
    private DateWorkArea max;
    private Infrastructure infrastructure;
    private TextWorkArea textWorkArea;
    private DateWorkArea dateWorkArea;
    private CsePerson ar;
    private CsePersonAddress csePersonAddress;
    private Common addressFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NewInformationRequest.
    /// </summary>
    [JsonPropertyName("newInformationRequest")]
    public InformationRequest NewInformationRequest
    {
      get => newInformationRequest ??= new();
      set => newInformationRequest = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    /// <summary>
    /// A value of NewCaseRole.
    /// </summary>
    [JsonPropertyName("newCaseRole")]
    public CaseRole NewCaseRole
    {
      get => newCaseRole ??= new();
      set => newCaseRole = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public CaseRole Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of SearchCaseRole.
    /// </summary>
    [JsonPropertyName("searchCaseRole")]
    public CaseRole SearchCaseRole
    {
      get => searchCaseRole ??= new();
      set => searchCaseRole = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
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

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private InformationRequest newInformationRequest;
    private InformationRequest informationRequest;
    private GeneticTest geneticTest;
    private Appointment appointment;
    private CaseRole newCaseRole;
    private CaseRole old;
    private CaseRole searchCaseRole;
    private CsePerson searchCsePerson;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CsePerson csePerson;
    private InterstateRequest interstateRequest;
    private CaseRole caseRole;
  }
#endregion
}
