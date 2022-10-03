// Program: SI_ROLE_CASE_ROLE_MAINTENANCE, ID: 371785524, model: 746.
// Short name: SWEROLEP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ROLE_CASE_ROLE_MAINTENANCE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists, adds, and updates the role of CSE PERSONs on a 
/// specified CASE.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiRoleCaseRoleMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ROLE_CASE_ROLE_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRoleCaseRoleMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRoleCaseRoleMaintenance.
  /// </summary>
  public SiRoleCaseRoleMaintenance(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------
    // 			C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 08/21/95  Helen Sharland		Initial Development
    // 05/01/96  Rao Mulpuri	IDCR# 127 & 132	Changes to Return Processing
    // 06/26/96  G. Lofton			Changed ssn to numeric fields.
    // 11/02/96  G. Lofton - MTW		Add new security and removed old.
    // 11/18/96  Ken Evans			Add case unit logic.
    // 01/06/97  G. Lofton - MTW		Add event processing.
    // 03/07/97  G. Lofton - MTW		Fixed return from ORGZ.
    // 05/02/97  J. Rookard - MTW		Begin implementation of Case Role event
    // 					processing.
    // 01/21/98  Sid Chowdhary	IDCR # 408
    // 09/16/98  W. Campbell			A IF-ELSE statement was added to fix a
    // 					problem whereby the ADD PERSON info at the
    // 					bottom of the screen was not being cleared
    // 					out when the user entered a new NEXT CASE
    // 					number and pressed F2 for display of the new
    // 					CASE data.  The BEEN_TO_NAME flag must also
    // 					be reset so that it will work properly for
    // 					the new CASE.
    // 09/16/98  W. Campbell			An ELSE statement was added (see notes). The
    // 					purpose of this logic is to make sure a new
    // 					AR being added is at least 10 yrs old
    // 					provided a DOB has been input for the new
    // 					person.
    // 09/17/98  W. Campbell			Added an IF statement to the case UPDATE
    // 					logic to not allow an end_date in the future.
    // 09/18/98  W. Campbell			Added a move statement to reinitialize the
    // 					NEW CASE_ROLE attributes on the screen on a
    // 					return from NAME or ORGZ and no data is
    // 					brought back.
    // 01/29/99  W.Campbell			Disabled logic which automatically created
    // 					case role=AP when creating a case role=FA.
    // 01/29/99  W.Campbell	IDCR4777	Added logic to use SI_ALTS_CAB_UPDATE_ALIAS
    // 					(called from within SI_VERIFY_AND_CREATE_CSE_PERSON)
    // 					in order to make a non_case related CSE
    // 					person into a case related CSE person, if
    // 					needed. See notes below.
    // 01/29/99  W.Campbell	IDCR477		Added a local view (local_cse ief_supplied
    // 					flag) to allow for the saving and passing of
    // 					a flag to indicate that the person being
    // 					passed is a non-case related CSE person
    // 					(flag = N).  This flag is being passed to
    // 					SI_VERIFY_AND_CREATE_CSE_PERSON used in the
    // 					logic below.
    // 02/12/99  W.Campbell			PFK16 (Command=ORGZ) disabled.  PF16 removed
    // 					as Screen literal, and code disabled.
    // 02/12/99  W.Campbell			Logic was added to validate that if the MO is
    // 					the AP then no other AP's are allowed.
    // 02/17/99 W.Campbell			Added an IF statement to bypass reading the
    // 					ADABAS person with a number of SPACES which
    // 					didn't work very well.
    // 04/10/99 W.Campbell			Added set statement so that the "Add
    // 					Successful" message will appear after adding
    // 					a new AR.
    // 04/10/99 W.Campbell			Replaced ZDEL Exit States.
    // 04/09/99 W.Campbell			New logic added for checking to see if
    // 					another case exist for any child on this case
    // 					with a different AR.
    // 04/27/99 W.Campbell			Disabled future start date capability.  Added
    // 					check so that new case role start date cannot
    // 					be in the future.
    // 04/27/99 W.Campbell			Replaced use of CURRENT DATE with view of
    // 					local current date.
    // 04/27/99 M.Lachowicz			Add Start_Date to Import and Export view
    // 					Hidden_Page_Key Case_Role to fix scrolling
    // 					problem. Additionally changes have been made
    // 					in si_read_case_roles_by_case to fix the
    // 					same problem.
    // 06/25/99 M.Lachowicz			Enable PFK16 (Command=ORGZ).
    // 09/09/99 W.Campbell			Added a set stmt and used the result of the
    // 					set stmt in the following USE stmt for cab
    // 					SI_VERIFY_AND_CREATE_CSE_PERSON to provide a
    // 					view match for the import_retrieve_person_program
    // 					date_work_area date.  This was done as part
    // 					of the work on PR# H72775.  Also, other
    // 					changes were made to called cab
    // 					SI_VERIFY_AND_CREATE_CSE_PERSON for this same
    // 					PR#.
    // 09/20/99 M.Lachowicz	PR# 73468	Check if exist male and female AP and if 
    // more
    // 					than male AP exist when father role is
    // 					already determined
    // 09/21/99 M.Lachowicz	PR# 74520	Check if the same person already plays the
    // 					active role on the same CASE.
    // 09/28/99 M.Lachowicz	PR# 74349	Ignore check if person has two active 
    // child
    // 					roles on other CASEs when new case role  is
    // 					not  'CH'
    // 10/06/99 M.Lachowicz	PR# 75874	User must select person not an 
    // organization
    // 					to flow to COMN.
    // 10/22/99 M.Lachowicz	PR# 77726	Allow user to create start date up 2 
    // years.
    // 10/22/99 M.Lachowicz	PR# 77725	Do not allow AP to become AR.
    // 10/22/99 M.Lachowicz	PR# 77695	User needs to enter Case Role Start Date.
    // 10/28/99 M.Lachowicz			Unprotect all fields for new case role when
    // 					case status is opened.
    // 01/14/2000 W.Campbell	PR# 84468	Removed edit for role start date EQ or GT
    // 					current date - 2 years.
    // 03/20/2000 M.Lachowicz			PRWORA Changes for Paternity Established
    // 					Indicator.
    // 05/23/00  M.Lachowicz	PR# 95763	Set current timestamp when Case Role is
    // 					updated
    // 05/30/00  M.Lachowicz	PR# 96205	Fixed the problem.
    // 06/23/00  M.Lachowicz	PR# 95251	Checked if new AR is different than 
    // current
    // 					one.
    // 06/29/00  M.Lachowicz	PR# 98146	ROLE needs to create the same FV code for
    // the
    // 					current AR.
    // 08/11/00  M.Lachowicz	PR# 88309	Display message when male AP is being 
    // added
    // 					and Father role already exists.
    // 08/23/00  W.Campbell	PR# 94249	Added new logic for additional msg when 
    // the
    // 					new AR start date is less than old AR
    // 					disbursement date.
    // 09/29/00  M.Lachowicz	{R# 104446	Discontinue child role before child 
    // becomes
    // 					AR.
    // 11/17/00  M.Lachowicz	WR 298		Create header information for screens.
    // 08/17/01  M.Lachowicz	PR 125459	Call CSNET any time when you add or 
    // update
    // 					CH, AR or AP.
    // 02/28/02  M.Lachowicz			Disable changes for PR125459 until CSNET is
    // 					not ready.
    // 03/01/02  M.Lachowicz	PR126880	Cursor positioning on ROLE screen
    // 			PR134061	Problem of family violence in ROLE
    // 			PR136512	Male AP has discontinue date = effective date
    // 					 - 1 day
    // 			PR139514	ROLE should not allow to create AP if this
    // 					person already played AR role on this case.
    // 05/08/02  M.Lachowicz	PR 125459	Call CSNET any time when you add or 
    // update
    // 					CH, AR or AP .
    // 07/16/02  M.Lachowicz	PR 150856	Validate person names to check if do not
    // 					contain invalid characters.
    // 09/06/02  T.Bobb	PR00155688	Cannot end AP role when active in open
    // 					Interstate Case.
    // 09/11/02  M.Lachowicz	PR#157363	Fix -811 SQLCODE when  person plays twice
    // 					'CH' role.
    // 11/20/02  SWSRPRM	PR # 159761	When returning from NAME, SSN # info is not
    // 					blanked out when returning w/out a selection.
    // 09/22/03  GVandy	PR186785	Support new flow from LOPS.
    // 05/27/11  Raj S		CQ9690		Set FVI_SET_DATE and FVI_UPDATED_BY when
    // 					setting FVI. Re-clicked the code.
    // 06/20/11  RMathews	CQ20394		Added edit for FA role when child role being
    // 					added exists on AE, Facts, Kanpay or KsCares.
    // 05/10/17  GVandy	CQ48108		Changes to support IV-D PEP.
    // 04/24/18  GVandy	CQ60722		Allow AP to be added if CH has an EP legal 
    // detail
    // 					with the AP being added.
    // ---------------------------------------------------------------------------------------------
    // 05/10/18   JHarden     CQ61889           Flow to ROLE from ARDS.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    export.HiddenReturnRequired.Flag = import.HiddenReturnRequired.Flag;

    if (Equal(global.Command, "CLEAR") && (
      AsChar(import.ChildSuccessfullyAdded.Flag) == 'N' || IsEmpty
      (import.ChildSuccessfullyAdded.Flag)))
    {
      var field = GetField(export.Next, "number");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;

      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.ChildSuccessfullyAdded.Flag = import.ChildSuccessfullyAdded.Flag;
    export.ReturnFromCpat.Flag = import.ReturnFromCpat.Flag;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Standard.Assign(import.Standard);
    export.Next.Number = import.Next.Number;
    export.Case1.Assign(import.Case1);

    // ----------------------------------------------------------------
    // The following IF-ELSE statement was added by W. Campbell
    // on 9/16/98 to fix a problem whereby the ADD PERSON info
    // at the bottom of the screen was not being cleared out when
    // the user entered a new NEXT CASE number and pressed F2
    // for display of the new CASE data.  The BEEN_TO_NAME
    // flag must also be reset so that it will work properly for the
    // new CASE.
    // ----------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") && !
      Equal(import.Next.Number, import.Case1.Number))
    {
      // -------------------------------------------------------------
      // The user has entered a new case number and pressed the
      // function key for DISPLAY.   Reset the BEEN_TO_NAME
      // flag to 'N', and DO NOT populate the EXPORT NEW
      // views with data.
      // -------------------------------------------------------------
      export.BeenToName.Flag = "N";
    }
    else
    {
      export.NewCsePersonsWorkSet.Assign(import.NewCsePersonsWorkSet);
      export.NewSsnWorkArea.Assign(import.NewSsnWorkArea);
      MoveCaseRole1(import.NewCaseRole, export.NewCaseRole);
      export.BeenToName.Flag = import.BeenToName.Flag;
    }

    // ----------------------------------------------------------------
    // End of IF-ELSE statement added by W. Campbell
    // on 9/16/98.
    // ----------------------------------------------------------------
    export.SelectAction.Flag = import.SelectAction.Flag;

    // ----------------------------------------------------
    // 08/23/2000 W.Campbell - Added following move
    // stmt for new view for new logic for  PR 94249.
    // ----------------------------------------------------
    export.SelectAction2.Flag = import.SelectAction2.Flag;
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    export.FromCads.Flag = import.FromCads.Flag;
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    if (IsEmpty(export.SelectAction.Flag))
    {
      export.SelectAction.Flag = "N";
    }

    // ----------------------------------------------------
    // 08/23/2000 W.Campbell - Added following IF
    // stmt for new view for new logic for  PR 94249.
    // ----------------------------------------------------
    if (IsEmpty(export.SelectAction2.Flag))
    {
      export.SelectAction2.Flag = "N";
    }

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      export.Export1.Update.DetailCsePerson.Number =
        import.Import1.Item.DetailCsePerson.Number;
      export.Export1.Update.DetailCsePersonsWorkSet.Assign(
        import.Import1.Item.DetailCsePersonsWorkSet);
      export.Export1.Update.DetailCaseRole.Assign(
        import.Import1.Item.DetailCaseRole);
    }

    import.Import1.CheckIndex();

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenStandard.PageNumber = import.HiddenStandard.PageNumber;

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKeyCsePerson.Number =
        import.HiddenPageKeys.Item.HiddenPageKeyCsePerson.Number;
      MoveCaseRole2(import.HiddenPageKeys.Item.HiddenPageKeyCaseRole,
        export.HiddenPageKeys.Update.HiddenPageKeyCaseRole);
    }

    import.HiddenPageKeys.CheckIndex();

    if (import.HiddenStandard.PageNumber == 0)
    {
      export.HiddenStandard.PageNumber = 1;

      export.HiddenPageKeys.Index = 0;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKeyCaseRole.EndDate =
        UseCabSetMaximumDiscontinueDate1();
    }

    if (Equal(global.Command, "FROMCPAT"))
    {
      export.ReturnFromCpat.Flag = "N";
      export.ChildSuccessfullyAdded.Flag = "N";
      global.Command = "DISPLAY";
    }

    if (AsChar(export.ChildSuccessfullyAdded.Flag) == 'Y' && AsChar
      (export.ReturnFromCpat.Flag) != 'Y' && !Equal(global.Command, "CPAT"))
    {
      export.Standard.NextTransaction = "";
      ExitState = "SI0000_MUST_FLOW_TO_CPAT";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      if (AsChar(export.HiddenReturnRequired.Flag) == 'Y')
      {
        ExitState = "LE0000_USE_PF9_TO_RETURN";

        return;
      }

      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ---------------------------------------------
    //       N E X T   T R A N  O U T G O I N G
    // ---------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      if (AsChar(export.HiddenReturnRequired.Flag) == 'Y')
      {
        ExitState = "LE0000_USE_PF9_TO_RETURN";

        return;
      }

      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    // ------------------------------------------------------------
    // If the case is closed, protect all fields except the
    // selection character.
    // ------------------------------------------------------------
    if (AsChar(export.Case1.Status) == 'C')
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        var field = GetField(export.Export1.Item.DetailCaseRole, "endDate");

        field.Color = "cyan";
        field.Protected = true;
      }

      export.Export1.CheckIndex();

      var field1 = GetField(export.NewCsePersonsWorkSet, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.NewCsePersonsWorkSet, "firstName");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.NewCsePersonsWorkSet, "lastName");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.NewCsePersonsWorkSet, "middleInitial");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.NewCsePersonsWorkSet, "sex");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.NewCsePersonsWorkSet, "dob");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.NewSsnWorkArea, "ssnNumPart1");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.NewSsnWorkArea, "ssnNumPart2");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.NewSsnWorkArea, "ssnNumPart3");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.NewCaseRole, "type1");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.NewCaseRole, "startDate");

      field11.Color = "cyan";
      field11.Protected = true;

      if (Equal(global.Command, "CREATE") || Equal(global.Command, "UPDATE"))
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";
      }
    }
    else
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        // ------------------------------------------------------------
        // If the case role is inactive or the role is AR, protect
        // that occurrence.
        // ------------------------------------------------------------
        if (Lt(export.Export1.Item.DetailCaseRole.EndDate, local.Current.Date) &&
          !
          Equal(export.Export1.Item.DetailCaseRole.EndDate, local.Null1.Date) ||
          Equal(export.Export1.Item.DetailCaseRole.Type1, "AR"))
        {
          var field = GetField(export.Export1.Item.DetailCaseRole, "endDate");

          field.Color = "cyan";
          field.Protected = true;
        }
      }

      export.Export1.CheckIndex();
    }

    local.ErrOnAdabasUnavailable.Flag = "Y";
    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    // ---------------------------------------------
    //       N E X T   T R A N  I N C O M I N G
    // ---------------------------------------------
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Case1, "number");

        field.Error = true;

        return;
      }

      // ------------------------------------
      //  Retrofit by RB Mohapatra  01/22/97
      // ------------------------------------
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        local.LastTran.SystemGeneratedIdentifier =
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.Case1.Number = local.LastTran.CaseNumber ?? Spaces(10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ------------------------------------------------------------
    // If returning from NAME or ORGZ, protect all fields except
    // for 'case_role type' and 'case_role start_date'.
    // ------------------------------------------------------------
    if (Equal(global.Command, "RETNAME") || Equal(global.Command, "RETORGZ"))
    {
      export.BeenToName.Flag = "Y";

      if (!IsEmpty(export.NewCsePersonsWorkSet.Number))
      {
        var field1 = GetField(export.NewCsePersonsWorkSet, "firstName");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.NewCsePersonsWorkSet, "lastName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.NewCsePersonsWorkSet, "middleInitial");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.NewCsePersonsWorkSet, "sex");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.NewCsePersonsWorkSet, "dob");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.NewSsnWorkArea, "ssnNumPart1");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.NewSsnWorkArea, "ssnNumPart2");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.NewSsnWorkArea, "ssnNumPart3");

        field8.Color = "cyan";
        field8.Protected = true;

        global.Command = "DISPLAY";
      }
      else
      {
        // ---------------------------------------------
        // W.Campbell 9/18/98 - Added the following
        // move statement in order to reinitialize the
        // new case_role attributes on the screen.
        // ---------------------------------------------
        MoveCaseRole1(local.BlankCaseRole, export.NewCaseRole);
        MoveSsnWorkArea2(local.BlankSsnWorkArea, export.NewSsnWorkArea);

        return;
      }
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    if (Equal(global.Command, "NAME") || Equal(global.Command, "ORGZ") || Equal
      (global.Command, "COMN") || Equal(global.Command, "RETNAME") || Equal
      (global.Command, "FROMCPAT") || Equal(global.Command, "CPAT"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    // ----------------------------------------------------
    // 08/23/2000 W.Campbell - Added OR clause
    // to the following IF stmt for new logic for
    // PR 94249.
    // ----------------------------------------------------
    if (AsChar(export.SelectAction.Flag) == 'Y' || AsChar
      (export.SelectAction2.Flag) == 'Y')
    {
      switch(AsChar(import.Standard.DeleteConfirmation))
      {
        case 'Y':
          // ----------------------------------------------------
          // 08/23/2000 W.Campbell - Added new logic for
          // additional msg when new AR start date is less
          // than old AR disbursement date.  PR 94249.
          // ----------------------------------------------------
          if (AsChar(export.SelectAction2.Flag) != 'Y')
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              // ----------------------------------------------------
              // 08/23/2000 W.Campbell - Find and save
              // the currently active AR from the group export
              // detail view.
              // PR 94249.
              // ----------------------------------------------------
              if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AR") && Equal
                (export.Export1.Item.DetailCaseRole.EndDate, local.Default1.Date))
                
              {
                local.ArCsePerson.Number =
                  export.Export1.Item.DetailCsePerson.Number;

                break;
              }
            }

            export.Export1.CheckIndex();

            if (IsEmpty(local.ArCsePerson.Number))
            {
              // ----------------------------------------------------
              // 08/23/2000 W.Campbell - This should never happen.
              // PR 94249.
              // ----------------------------------------------------
              ExitState = "SYSTEM_ERROR_HAS_OCCURRED_RB";

              return;
            }

            // ----------------------------------------------------
            // 08/23/2000 W.Campbell - Read for a disbursement
            // for the old AR which has a date GT or EQ the start
            // date of the new AR.
            // PR 94249.
            // ----------------------------------------------------
            if (ReadDisbursementTransaction())
            {
              // ----------------------------------------------------
              // 08/23/2000 W.Campbell - A disbursement exists.  Therefore
              // we must confirm that the user wants this new person to
              // become the AR on this Case. PR 94249.
              // ----------------------------------------------------
              // ------------------------------------------------------------
              // Confirm again that the current AR should be end-dated
              // before replacing with the new AR since we have
              // disbursements for the current AR.  PR 94249.
              // ------------------------------------------------------------
              export.Standard.DeleteText = "Respond to msg with Y or N.";

              var field5 = GetField(export.Standard, "deleteText");

              field5.Color = "white";
              field5.Protected = true;

              var field6 = GetField(export.Standard, "deleteConfirmation");

              field6.Color = "green";
              field6.Highlighting = Highlighting.Underscore;
              field6.Protected = false;
              field6.Focused = true;

              export.Standard.DeleteConfirmation = "";
              export.SelectAction2.Flag = "Y";

              // ----------------------------------
              // Protect all the other fields.
              // ----------------------------------
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                if (!export.Export1.CheckSize())
                {
                  break;
                }

                var field17 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field17.Color = "cyan";
                field17.Protected = true;

                var field18 =
                  GetField(export.Export1.Item.DetailCaseRole, "endDate");

                field18.Color = "cyan";
                field18.Protected = true;
              }

              export.Export1.CheckIndex();

              var field7 = GetField(export.NewCsePersonsWorkSet, "lastName");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 = GetField(export.NewCsePersonsWorkSet, "firstName");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.NewCsePersonsWorkSet, "middleInitial");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 = GetField(export.NewCsePersonsWorkSet, "sex");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 = GetField(export.NewCsePersonsWorkSet, "dob");

              field11.Color = "cyan";
              field11.Protected = true;

              var field12 = GetField(export.NewSsnWorkArea, "ssnNumPart1");

              field12.Color = "cyan";
              field12.Protected = true;

              var field13 = GetField(export.NewSsnWorkArea, "ssnNumPart2");

              field13.Color = "cyan";
              field13.Protected = true;

              var field14 = GetField(export.NewSsnWorkArea, "ssnNumPart3");

              field14.Color = "cyan";
              field14.Protected = true;

              var field15 = GetField(export.NewCaseRole, "startDate");

              field15.Color = "cyan";
              field15.Protected = true;

              var field16 = GetField(export.NewCaseRole, "type1");

              field16.Color = "cyan";
              field16.Protected = true;

              ExitState = "SI0000_NEW_AR_CAUSE_RECOVERIES";

              return;
            }
            else
            {
              // ----------------------------------------------------
              // 08/23/2000 W.Campbell - Everything is fine.  This AR does
              // not have any disbursements to worry about. PR 94249.
              // ----------------------------------------------------
            }
          }
          else
          {
            export.SelectAction2.Flag = "N";
            export.Standard.DeleteText = "";
            export.Standard.DeleteConfirmation = "";
          }

          // ----------------------------------------------------
          // 08/23/2000 W.Campbell - End of new logic for
          // additional msg when new AR start date is less
          // than old AR disbursement date.  PR 94249.
          // ----------------------------------------------------
          export.SelectAction.Flag = "N";
          export.Standard.DeleteText = "";
          export.Standard.DeleteConfirmation = "";

          // -----------------------------------------------
          // Create the person on ADABAS and DB2 if necessary
          // -----------------------------------------------
          if (CharAt(import.NewCsePersonsWorkSet.Number, 10) != 'O')
          {
            // ------------------------------------------------
            // 02/17/99 W.Campbell - Added the following
            // IF statement to bypass reading the ADABAS
            // person with a number of SPACES.  This didn't
            // work very well.
            // ------------------------------------------------
            if (!IsEmpty(import.NewCsePersonsWorkSet.Number))
            {
              // ------------------------------------------------
              // 01/29/99 W.Campbell - Added a local view
              // (local_cse ief_supplied flag) to allow for the saving and
              // passing of a flag to indicate that the person being passed
              // is a non-case related CSE person (flag = N).  This
              // flag is being passed to SI_VERIFY_AND_CREATE_CSE_PERSON
              // used in the logic below.  Work done on IDCR477.
              // The flag is view matched to the export view below
              // which is obtained from CAB_READ_ADABAS_PERSON.
              // ------------------------------------------------
              UseCabReadAdabasPerson1();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();

                return;
              }
            }

            // -----------------------------------------------
            // 09/09/99 W.Campbell - Added the following set stmt and
            // used the result of the set stmt in the following USE stmt to
            // provide a view match for the mport_retrieve_persn_program
            // date_work_area date.  This was done as part of the work on
            // PR# H72775.  Also, other changes were made to called cab
            // SI_VERIFY_AND_CREATE_CSE_PERSON for the PR#.
            // -----------------------------------------------
            local.RetreivePersonProgram.Date = export.NewCaseRole.StartDate;
            UseSiVerifyAndCreateCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }

            if (!IsEmpty(local.AbendData.Type1))
            {
              UseEabRollbackCics();

              return;
            }
          }

          // ------------------------------------------------
          // End-date the current AR with the day before the start
          // date of the new AR.  Create the new AR case role.
          // -----------------------------------------------
          UseSiChangeAr();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          // 05/08/2002 M.Lachowicz Start - enabled  changes
          // 02/28/2002 M.Lachowicz Start
          local.ScreenIdentification.Command = "ROLE";
          global.Command = "UPDATE";
          UseSiCreateAutoCsenetTrans3();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          global.Command = "CREATE";
          local.NewCsePerson.Number = export.NewCsePersonsWorkSet.Number;
          UseSiCreateAutoCsenetTrans4();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          // 02/28/2002 M.Lachowicz End
          // 05/08/2002 M.Lachowicz End - enabled  changes
          // ------------------------------------------------
          // 04/10/99 W.Campbell - Added set statement so "Add
          // Successful" message will appear after adding a new AR.
          // -----------------------------------------------
          local.SuccessfulAdd.Flag = "Y";
          export.NewCsePersonsWorkSet.Assign(local.BlankCsePersonsWorkSet);
          export.NewSsnWorkArea.SsnNumPart1 = 0;
          export.NewSsnWorkArea.SsnNumPart2 = 0;
          export.NewSsnWorkArea.SsnNumPart3 = 0;
          MoveCaseRole1(local.BlankCaseRole, export.NewCaseRole);
          export.HiddenStandard.PageNumber = 1;

          // 05/17/00 M.L Start
          export.ChildSuccessfullyAdded.Flag = "Y";

          // 05/17/00 M.L End
          global.Command = "DISPLAY";

          // 06/29/2000 M.L Start
          foreach(var item in ReadCsePerson7())
          {
            if (AsChar(entities.ChildPaternityEstInd.FamilyViolenceIndicator) ==
              'C')
            {
              local.CsePerson.FamilyViolenceIndicator =
                entities.ChildPaternityEstInd.FamilyViolenceIndicator;

              break;
            }

            if (AsChar(entities.ChildPaternityEstInd.FamilyViolenceIndicator) ==
              AsChar(local.CsePerson.FamilyViolenceIndicator) || AsChar
              (local.CsePerson.FamilyViolenceIndicator) == 'P')
            {
            }
            else
            {
              local.CsePerson.FamilyViolenceIndicator =
                entities.ChildPaternityEstInd.FamilyViolenceIndicator;
            }
          }

          // 03/01/2002 M. Lachowicz
          // Added CSE_PERSON TYPE = 'C' to make processing for
          // people not for organization
          if (ReadCsePerson1())
          {
            if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator) || IsEmpty
              (local.CsePerson.FamilyViolenceIndicator))
            {
            }
            else
            {
              if (IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
              {
                local.GenerateEvent.Flag = "Y";
              }

              try
              {
                UpdateCsePerson2();
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

              if (AsChar(local.GenerateEvent.Flag) == 'Y')
              {
                UseSiFvEvent();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseEabRollbackCics();

                  return;
                }
              }

              UseSiChangeArFvIndicator();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();

                return;
              }
            }
          }

          // 06/29/2000 M.L Start
          break;
        case 'N':
          export.SelectAction.Flag = "N";

          // ----------------------------------------------------
          // 08/23/2000 W.Campbell - Added following set
          // stmt for new logic when new AR start date is less
          // than old AR disbursement date.  PR 94249.
          // ----------------------------------------------------
          export.SelectAction2.Flag = "N";
          export.Standard.DeleteText = "";
          export.Standard.DeleteConfirmation = "";

          var field1 = GetField(export.Standard, "deleteText");

          field1.Color = "green";
          field1.Intensity = Intensity.Dark;
          field1.Protected = true;

          var field2 = GetField(export.Standard, "deleteConfirmation");

          field2.Color = "green";
          field2.Intensity = Intensity.Dark;
          field2.Protected = true;

          ExitState = "NEW_AR_CANCELED";

          return;
        default:
          var field3 = GetField(export.Standard, "deleteText");

          field3.Color = "white";
          field3.Protected = true;

          var field4 = GetField(export.Standard, "deleteConfirmation");

          field4.Color = "red";
          field4.Intensity = Intensity.High;
          field4.Highlighting = Highlighting.ReverseVideo;
          field4.Protected = false;
          field4.Focused = true;

          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
          global.Command = "";

          return;
      }
    }

    // For UPDATE, determine if a row has been selected and modified by the user
    // for processing.
    if (Equal(global.Command, "UPDATE"))
    {
      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (!import.Import1.CheckSize())
        {
          break;
        }

        export.Export1.Index = import.Import1.Index;
        export.Export1.CheckSize();

        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          if (Equal(export.Export1.Item.DetailCaseRole.EndDate, local.Null1.Date))
            
          {
            var field = GetField(export.Export1.Item.DetailCaseRole, "endDate");

            field.Color = "red";
            field.Protected = false;
            field.Focused = true;

            ExitState = "FIELD_REQUIRED_FOR_UPDATE";

            return;
          }

          ++local.Common.Count;
        }
      }

      import.Import1.CheckIndex();

      if (local.Common.Count < 1)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "CPAT":
        ExitState = "ECO_LNK_TO_CPAT";

        return;
      case "PROCESS":
        break;
      case "EXIT":
        if (AsChar(export.HiddenReturnRequired.Flag) == 'Y')
        {
          ExitState = "LE0000_USE_PF9_TO_RETURN";

          return;
        }

        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "NEXT":
        if (export.HiddenStandard.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details to retrieve.
        // ---------------------------------------------
        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (IsEmpty(export.HiddenPageKeys.Item.HiddenPageKeyCsePerson.Number))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.HiddenStandard.PageNumber;

        break;
      case "PREV":
        if (export.HiddenStandard.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenStandard.PageNumber;

        break;
      case "CREATE":
        if (AsChar(export.BeenToName.Flag) != 'Y')
        {
          ExitState = "NAME_SEARCH_REQUIRED";

          return;
        }

        if (IsEmpty(export.NewCsePersonsWorkSet.Number))
        {
          // ------------------------------------------------------------
          // If this is a new person, validate Last Name, First Name,
          // Gender, and DOB.
          // ------------------------------------------------------------
          // ------------------
          // Last Name Required
          // ------------------
          if (IsEmpty(export.NewCsePersonsWorkSet.LastName))
          {
            var field = GetField(export.NewCsePersonsWorkSet, "lastName");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }

          // -------------------
          // First Name Required
          // -------------------
          if (IsEmpty(export.NewCsePersonsWorkSet.FirstName))
          {
            var field = GetField(export.NewCsePersonsWorkSet, "firstName");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }

          // 07/16/2002 M.L Start
          UseSiCheckName();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SI0001_INVALID_NAME";

            var field1 = GetField(export.NewCsePersonsWorkSet, "lastName");

            field1.Error = true;

            var field2 = GetField(export.NewCsePersonsWorkSet, "firstName");

            field2.Error = true;

            var field3 = GetField(export.NewCsePersonsWorkSet, "middleInitial");

            field3.Error = true;

            return;
          }

          // 07/16/2002 M.L End
          // ------------------
          // Validate Gender
          // ------------------
          switch(AsChar(export.NewCsePersonsWorkSet.Sex))
          {
            case 'M':
              break;
            case 'F':
              break;
            default:
              var field = GetField(export.NewCsePersonsWorkSet, "sex");

              field.Error = true;

              ExitState = "INVALID_SEX";

              return;
          }

          // ------------------
          // Validate DOB
          // ------------------
          // ---------------------------------------
          // 04/27/99 W.Campbell - Replaced use of
          // CURRENT DATE with view of local current
          // date.
          // ---------------------------------------
          if (Lt(local.Current.Date, export.NewCsePersonsWorkSet.Dob))
          {
            var field = GetField(export.NewCsePersonsWorkSet, "dob");

            field.Error = true;

            ExitState = "INVALID_DATE_OF_BIRTH";

            return;
          }
        }

        // ----------------------------
        // Validate Start Date
        // ----------------------------
        if (Equal(import.NewCaseRole.StartDate, local.Null1.Date))
        {
          // 10/22/99 M.L PR#77695 - Start
          ExitState = "SI0000_ENTER_START_DATE";

          var field = GetField(export.NewCaseRole, "startDate");

          field.Error = true;

          return;

          // ---------------------------------------
          // 04/27/99 W.Campbell - Replaced use of
          // CURRENT DATE with view of local current
          // date.
          // ---------------------------------------
          // 10/22/99 M.L PR#77695 - End
        }

        if (Lt(export.NewCaseRole.StartDate, import.Case1.CseOpenDate))
        {
          var field = GetField(export.NewCaseRole, "startDate");

          field.Error = true;

          ExitState = "INVALID_CASE_ROLE_START_DT";

          return;
        }

        // 10/22/99 M.L Start  PR77726
        // ---------------------------------------
        // 01/14/2000 W.Campbell - Removed edit for role start date
        // GT or EQ current date - 2 years.  Work done on PR# 84468.
        // ---------------------------------------
        // 10/22/99 M.L End
        // ---------------------------
        // 04/27/99 - Disabled future start date capability.  Added
        // check so new case role start date cannot be in the future.
        // ---------------------------
        // ---------------------------------------
        // 04/27/99 W.Campbell - Replaced use of CURRENT DATE
        // with view of local current date.
        // ---------------------------------------
        if (Lt(local.Current.Date, export.NewCaseRole.StartDate))
        {
          var field = GetField(export.NewCaseRole, "startDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        if (!Equal(export.NewCsePersonsWorkSet.Dob, local.Null1.Date))
        {
          if (Lt(export.NewCaseRole.StartDate, export.NewCsePersonsWorkSet.Dob))
          {
            var field = GetField(export.NewCaseRole, "startDate");

            field.Error = true;

            ExitState = "START_DT_BEFORE_DOB";

            return;
          }
        }

        // ---------------------------
        // Validate Case Role
        // ---------------------------
        switch(TrimEnd(export.NewCaseRole.Type1))
        {
          case "AR":
            break;
          case "AP":
            break;
          case "CH":
            break;
          case "MO":
            // ------------------------------------------------
            // Verify mother is not Male
            // ------------------------------------------------
            if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'M' || IsEmpty
              (export.NewCsePersonsWorkSet.Sex))
            {
              var field1 = GetField(export.NewCaseRole, "type1");

              field1.Error = true;

              ExitState = "MOTHER_MUST_BE_FEMALE";

              return;
            }

            // ------------------------------------------------
            // Verify only one mother active on the case
            // ------------------------------------------------
            UseSiVerifyOneActiveCaseRole3();

            if (AsChar(local.Error.Flag) == 'Y')
            {
              var field1 = GetField(export.NewCaseRole, "type1");

              field1.Error = true;

              ExitState = "INVALID_NUMBER_OF_MOTHERS";

              return;
            }

            break;
          case "FA":
            // ------------------------------------------------
            // Verify father is not female
            // ------------------------------------------------
            if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'F' || IsEmpty
              (export.NewCsePersonsWorkSet.Sex))
            {
              var field1 = GetField(export.NewCaseRole, "type1");

              field1.Error = true;

              ExitState = "FATHER_MUST_BE_MALE";

              return;
            }

            // ------------------------------------------------
            // Verify only one father active on the case
            // ------------------------------------------------
            UseSiVerifyOneActiveCaseRole3();

            if (AsChar(local.Error.Flag) == 'Y')
            {
              var field1 = GetField(export.NewCaseRole, "type1");

              field1.Error = true;

              ExitState = "INVALID_NUMBER_OF_FATHERS";

              return;
            }

            break;
          default:
            var field = GetField(export.NewCaseRole, "type1");

            field.Error = true;

            ExitState = "INVALID_ROLE";

            return;
        }

        MoveSsnWorkArea1(export.NewSsnWorkArea, local.NewSsnWorkArea);
        local.NewSsnWorkArea.ConvertOption = "2";
        UseCabSsnConvertNumToText();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!IsEmpty(export.NewSsnWorkArea.SsnText9))
        {
          export.NewCsePersonsWorkSet.Ssn = export.NewSsnWorkArea.SsnText9;
        }
        else
        {
          export.NewCsePersonsWorkSet.Ssn = "000000000";
        }

        // ------------------------------------------------------------
        // Perform Additional Validation for existing person.
        // ------------------------------------------------------------
        if (!IsEmpty(export.NewCsePersonsWorkSet.Number))
        {
          local.CsePerson.Number = export.NewCsePersonsWorkSet.Number;

          if (CharAt(local.CsePerson.Number, 10) != 'O')
          {
            // ------------------------------------------------
            // 01/29/99 W.Campbell - Added a local view
            // (local_cse ief_supplied flag) to allow for the saving and
            // passing of a flag to indicate that the person being passed
            // is a non-case related CSE person (flag = N).  This
            // flag is being passed to SI_VERIFY_AND_CREATE_CSE_PERSON
            // used in the logic below.  Work done on IDCR477.
            // The flag is view matched to the export view below
            // which is obtained from CAB_READ_ADABAS_PERSON.
            // ------------------------------------------------
            UseCabReadAdabasPerson1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // 04/30/01 M.L Start
            if (Equal(export.NewCaseRole.Type1, "FA"))
            {
              // ------------------------------------------------
              // Verify father is not female
              // ------------------------------------------------
              if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'F' || IsEmpty
                (export.NewCsePersonsWorkSet.Sex))
              {
                var field = GetField(export.NewCaseRole, "type1");

                field.Error = true;

                ExitState = "FATHER_MUST_BE_MALE";

                return;
              }
            }

            if (Equal(export.NewCaseRole.Type1, "MO"))
            {
              // ------------------------------------------------
              // Verify mother is not Male
              // ------------------------------------------------
              if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'M' || IsEmpty
                (export.NewCsePersonsWorkSet.Sex))
              {
                var field = GetField(export.NewCaseRole, "type1");

                field.Error = true;

                ExitState = "MOTHER_MUST_BE_FEMALE";

                return;
              }
            }

            // 04/30/01 M.L End
            if (!Equal(export.NewCsePersonsWorkSet.Ssn,
              export.NewSsnWorkArea.SsnText9))
            {
              local.NewSsnWorkArea.SsnText9 = export.NewCsePersonsWorkSet.Ssn;
              UseCabSsnConvertTextToNum();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            // 09/28/99 M.L
            if (Equal(export.NewCaseRole.Type1, "CH"))
            {
              // ------------------------------------------------------------
              // Person can be an Active Child only on two cases.
              // ------------------------------------------------------------
              UseSiVerifyChildIsOnACase();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (AsChar(local.ActiveCaseCh.Flag) == 'Y')
              {
                if (local.ActiveCaseCh.Count > 1)
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "SI0000_CHILD_ACTIVE_ON_TWO_CASES";

                  return;
                }
              }
            }

            // 09/28/99 M.L end
            switch(TrimEnd(export.NewCaseRole.Type1))
            {
              case "AR":
                // ------------------------------------------------------------
                // AR can be an AP on this case, but the AP role will be end
                // date when the AR role is created.
                // ------------------------------------------------------------
                // ------------------------------------------------------------
                // AR can be a Child on this case, but the CH role will be end
                // dated when the AR role is created.  Check needs to be
                // performed to determine if there is at least one other active
                // CH role before continuing.
                // ------------------------------------------------------------
                UseSiVerifyNotLastChOnCase();

                if (AsChar(local.ChOnCase.Flag) == 'Y' && AsChar
                  (local.OtherChOnCase.Flag) == 'N')
                {
                  var field = GetField(export.NewCsePersonsWorkSet, "number");

                  field.Error = true;

                  ExitState = "SI0000_LAST_ACTIVE_CH_ON_CASE";

                  return;
                }

                // 09/29/00 M.L Start
                if (AsChar(local.ChOnCase.Flag) == 'Y')
                {
                  var field = GetField(export.NewCsePersonsWorkSet, "number");

                  field.Error = true;

                  ExitState = "SI000_DISCONTINUE_CHILD_ROLE";

                  return;
                }

                // 06/23/00 M.L Start
                if (ReadCaseRoleCsePerson1())
                {
                  // ------------------------------------------------
                  // New AR is the same as the current one.
                  // This is error.
                  // -----------------------------------------------
                  ExitState = "SI0000_NEW_AR_SAME_AS_OLD_ONE";

                  var field = GetField(export.NewCsePersonsWorkSet, "number");

                  field.Error = true;

                  return;
                }
                else
                {
                  // ------------------------------------------------
                  // New AR is not the same as the current one. You can 
                  // continue.
                  // -----------------------------------------------
                }

                // 06/23/00 M.L End
                break;
              case "AP":
                // -----------------------------------------------
                // AP cannot be a Child on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "CH";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                // 03/01/2002 M. Lachowicz Start
                // -----------------------------------------------
                // AP cannot be anytime an AR on this case.
                // -----------------------------------------------
                if (ReadCaseRole2())
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                // 03/01/2002 M. Lachowicz End
                // -----------------------------------------------
                // The same AP cannot be added more than once.
                // -----------------------------------------------
                local.CaseRole.Type1 = "AP";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                // -----------------------------------------------
                // If father is determined, no other male APs allowed, except a 
                // new AP role for the FAther.
                // -----------------------------------------------
                MoveCaseRole1(export.NewCaseRole, local.CaseRole);
                local.CaseRole.Type1 = "FA";
                UseSiVerifyOneActiveCaseRole2();

                if (AsChar(local.Error.Flag) == 'Y')
                {
                  // -----------------------------------------------------------
                  // Determine gender of the new AP.  If the new AP is female
                  // (and presumably the MOther), allow the new AP even though
                  // a FAther role is active.
                  // -----------------------------------------------------------
                  if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'F')
                  {
                    goto Test;
                  }

                  // -----------------------------------------------------------
                  // Determine if the new AP is already on the Case in the 
                  // FAther
                  // Case Role. If so, allow the addition of the new AP Case
                  // Role.  Validation has already been performed to determine
                  // that this new AP is not currently an AR on the Case and is
                  // not currently an AP on the Case.
                  // -----------------------------------------------------------
                  if (ReadCaseRole3())
                  {
                    // Currency on the FAther Case Role is acquired.
                    // 06/23/99 M.L
                    //              Change property of the following READ to 
                    // generate
                    //              SELECT ONLY
                    if (ReadCsePerson4())
                    {
                      // New incoming AP Case Role is same CSE Person playing
                      // the FAther Case Role for this Case.  Allow creation of 
                      // the
                      // new AP.
                      goto Test;
                    }
                    else
                    {
                      // The new incoming AP is not currently in the FAther Case
                      // Role for this Case.
                      // Do not allow creation of the new AP Case Role.
                    }
                  }
                  else
                  {
                    // 08/11/2000 M.L Start
                    if (ReadCaseRole4())
                    {
                      // Currency on the FAther Case Role is acquired.
                      // 06/23/99 M.L
                      //              Change property of the following READ to 
                      // generate
                      //              SELECT ONLY
                      if (ReadCsePerson4())
                      {
                        // The new incoming AP Case Role is the same CSE Person 
                        // playing the FAther Case Role for this
                        // Case.  Allow creation of the new AP.
                        goto Test;
                      }
                      else
                      {
                        // The new incoming AP is not currently in the FAther 
                        // Case Role for this Case.
                        // Do not allow creation of the new AP Case Role.
                      }
                    }
                    else
                    {
                      // If FAther case role is not found on this read, a 
                      // database integrity problem must exist,
                      // since the FAther Case Role was read in the previous 
                      // action block, which placed us into this
                      // block of code.  Perform a rollback and exit.
                      UseEabRollbackCics();
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      return;
                    }

                    // 08/11/2000 M.L End
                  }

                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "INVALID_FA_AND_MULTIPLE_AP";

                  return;
                }

Test:

                if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'F')
                {
                  // -----------------------------------------------
                  // If AP is female, no other APs allowed.
                  // -----------------------------------------------
                  local.CaseRole.Type1 = "AP";
                  UseSiVerifyOneActiveCaseRole2();

                  if (AsChar(local.Error.Flag) == 'Y')
                  {
                    var field = GetField(export.NewCaseRole, "type1");

                    field.Error = true;

                    ExitState = "INVALID_AP_MALE_AND_FEMALE";

                    return;
                  }
                }

                // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Cannot add new AP if 
                // any child has an active
                //    EP legal detail.
                foreach(var item in ReadCsePerson6())
                {
                  if (ReadLegalActionDetail())
                  {
                    // 04/24/18 GVandy  CQ60722  Allow AP to be added if CH has 
                    // an EP legal detail with the AP being added.
                    if (ReadLegalActionPerson())
                    {
                      var field = GetField(export.NewCaseRole, "type1");

                      field.Error = true;

                      ExitState = "SI0000_CANT_ADD_AP_WITH_EP_LDET";

                      return;
                    }
                  }
                }

                // -----------------------------------------------
                // 02/12/99 W.Campbell - New Code added.
                // The following READ was added to add the
                // validation that if the MO is the AP then no
                // other AP's are allowed.
                // -----------------------------------------------
                if (ReadCaseRole5())
                {
                  // Currency on the MOther Case Role is acquired.
                  // 06/23/99 M.L
                  //              Change property of the following READ to 
                  // generate
                  //              SELECT ONLY
                  if (ReadCsePerson4())
                  {
                    // The new incoming AP Case Role is the same CSE Person 
                    // playing the MOther Case Role for this Case.
                    // Allow creation of the new AP.
                  }
                  else
                  {
                    // 06/23/99 M.L
                    //              Change property of the following READ to 
                    // generate
                    //              SELECT ONLY
                    if (ReadCsePerson5())
                    {
                      if (ReadCaseRole1())
                      {
                        var field = GetField(export.NewCaseRole, "type1");

                        field.Error = true;

                        ExitState = "INVALID_MO_AND_MULTIPLE_AP";

                        return;
                      }
                    }
                    else
                    {
                      UseEabRollbackCics();
                      ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

                      return;
                    }
                  }
                }

                // -----------------------------------------------
                // 02/12/99 W.Campbell - End of new code added.
                // -----------------------------------------------
                break;
              case "MO":
                // -----------------------------------------------
                // Mother cannot be a child on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "CH";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                local.CaseRole.Type1 = "AR";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  local.SetNewRelToAr.Flag = "Y";
                }

                break;
              case "FA":
                // -----------------------------------------------
                // Father cannot be a child on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "CH";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                local.SetNewRelToAr.Flag = "Y";

                break;
              case "CH":
                // -----------------------------------------------
                // Child cannot be an AR on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "AR";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                // -----------------------------------------------
                // Child cannot be an AP on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "AP";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                // -----------------------------------------------
                // Child cannot be a Mother on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "MO";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                // -----------------------------------------------
                // Child cannot be a Father on this case.
                // -----------------------------------------------
                local.CaseRole.Type1 = "FA";
                UseSiCheckCaseRoleCombinations();

                if (AsChar(local.Common.Flag) == 'Y')
                {
                  var field = GetField(export.NewCaseRole, "type1");

                  field.Error = true;

                  ExitState = "CONFLICT_OF_ROLES";

                  return;
                }

                break;
              default:
                break;
            }

            // -----------------------------------------------
            // The same CH cannot be added more than once. In any Role
            // other than AR.  If a child becomes the AR, the child role is
            // end-dated in the SI_Change_AR action block.  The child
            // must be at least 10 years old to become the AR, per Cheryl
            // Deghand on 9-16-98.
            // -----------------------------------------------
            if (Equal(export.NewCaseRole.Type1, "AR"))
            {
              // Determine if the new AR is currently a child on the Case.  If 
              // so, determine if the child is of age (10) to become the new AR.
              // 09/11/2002 M.L
              //              Change property of the following READ to generate
              //              both SELECT and CURSOR
              if (ReadCaseRole6())
              {
                if (Equal(export.NewCsePersonsWorkSet.Dob, local.Null1.Date))
                {
                }
                else
                {
                  local.OfAge.Date = AddYears(local.Current.Date, -10);

                  if (!Lt(local.OfAge.Date, export.NewCsePersonsWorkSet.Dob))
                  {
                  }
                  else
                  {
                    ExitState = "SI0000_CH_NOT_OF_AGE_FOR_AR";

                    return;
                  }
                }
              }

              // 10/22/99 M.L - PR#77725 Start
              if (ReadCaseRole8())
              {
                ExitState = "SI0000_AP_CAN_NOT_BECOME_AR";

                return;
              }

              // 10/22/99 M.L - PR#77436 Start
            }
            else
            {
              local.CaseRole.Type1 = "CH";
              UseSiCheckCaseRoleCombinations();

              if (AsChar(local.Common.Flag) == 'Y')
              {
                var field = GetField(export.NewCaseRole, "type1");

                field.Error = true;

                ExitState = "CONFLICT_OF_ROLES";

                return;
              }
            }
          }
          else if (!Equal(export.NewCaseRole.Type1, "AR"))
          {
            var field = GetField(export.NewCaseRole, "type1");

            field.Error = true;

            ExitState = "ORGANIZATION_MUST_BE_AR";

            return;
          }
        }
        else
        {
          // -----------------------------------------------
          // ELSE logic is to make sure the AR is at least 10 yrs old
          // provided a DOB has been input for the new person.
          // -----------------------------------------------
          if (Equal(export.NewCaseRole.Type1, "AR"))
          {
            // -----------------------------------------------
            // A person must be at least 10 years old to
            // become the AR, as per Cheryl Deghand on 9-16-98.
            // -----------------------------------------------
            if (Equal(export.NewCsePersonsWorkSet.Dob, local.Null1.Date))
            {
            }
            else
            {
              local.OfAge.Date = AddYears(local.Current.Date, -10);

              if (!Lt(local.OfAge.Date, export.NewCsePersonsWorkSet.Dob))
              {
              }
              else
              {
                ExitState = "SI0000_CH_NOT_OF_AGE_FOR_AR";

                return;
              }
            }
          }

          // -----------------------------------------------
          // End of ELSE statement added by W. Campbell on 09/16/98.
          // -----------------------------------------------
          // 08/11/2000 M.L Start
          // -----------------------------------------------
          // If father is determined, no other male APs allowed, except a
          // new AP role for the FAther.
          // -----------------------------------------------
          if (Equal(export.NewCaseRole.Type1, "AP") && AsChar
            (export.NewCsePersonsWorkSet.Sex) == 'M')
          {
            MoveCaseRole1(export.NewCaseRole, local.CaseRole);
            local.CaseRole.Type1 = "FA";
            UseSiVerifyOneActiveCaseRole2();

            if (AsChar(local.Error.Flag) == 'Y')
            {
              var field = GetField(export.NewCaseRole, "type1");

              field.Error = true;

              ExitState = "INVALID_FA_AND_MULTIPLE_AP";

              return;
            }
          }

          // 08/11/2000 M.L End
          if (Equal(export.NewCaseRole.Type1, "AP"))
          {
            // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Cannot add new AP if any 
            // child has an active
            //    EP legal detail.
            foreach(var item in ReadCsePerson6())
            {
              if (ReadLegalActionDetail())
              {
                var field = GetField(export.NewCaseRole, "type1");

                field.Error = true;

                ExitState = "SI0000_CANT_ADD_AP_WITH_EP_LDET";

                return;
              }
            }
          }
        }

        // ------------------------------------------------
        // Add the new case role.  If an AR is being changed, output a
        // confirmation message and interpret the response.
        // ------------------------------------------------
        if (Equal(import.NewCaseRole.Type1, "AR"))
        {
          // ------------------------------------------------------------
          // Confirm that the current AR should be end-dated before
          // replacing with the new AR
          // ------------------------------------------------------------
          export.Standard.DeleteText = "Inactivate the current AR ?";

          var field1 = GetField(export.Standard, "deleteText");

          field1.Color = "white";
          field1.Protected = true;

          var field2 = GetField(export.Standard, "deleteConfirmation");

          field2.Color = "green";
          field2.Highlighting = Highlighting.Underscore;
          field2.Protected = false;
          field2.Focused = true;

          export.Standard.DeleteConfirmation = "";
          export.SelectAction.Flag = "Y";

          break;
        }

        // 09/20/99 M.L Start
        // Check if already exists active AP for export_new case_role 
        // start_date.
        // Only one active AP is allowed except the situation when Father is not
        // known (it is checked by this procedure later).
        if (Equal(import.NewCaseRole.Type1, "AP"))
        {
          if (ReadCsePersonCaseRole())
          {
            local.ActiveAp.Number = entities.CsePerson.Number;
            local.ErrOnAdabasUnavailable.Flag = "Y";
            UseCabReadAdabasPerson2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            switch(AsChar(local.ActiveAp.Sex))
            {
              case 'F':
                switch(AsChar(export.NewCsePersonsWorkSet.Sex))
                {
                  case 'F':
                    ExitState = "INVALID_NUMBER_OF_FEMALE_APS";

                    return;
                  case 'M':
                    ExitState = "INVALID_AP_MALE_AND_FEMALE";

                    return;
                  case ' ':
                    ExitState = "INVALID_AP_MALE_AND_FEMALE";

                    return;
                  default:
                    break;
                }

                break;
              case 'M':
                switch(AsChar(export.NewCsePersonsWorkSet.Sex))
                {
                  case 'F':
                    ExitState = "INVALID_AP_MALE_AND_FEMALE";

                    return;
                  case 'M':
                    break;
                  case ' ':
                    break;
                  default:
                    break;
                }

                break;
              case ' ':
                switch(AsChar(export.NewCsePersonsWorkSet.Sex))
                {
                  case 'F':
                    ExitState = "INVALID_AP_MALE_AND_FEMALE";

                    return;
                  case 'M':
                    break;
                  case ' ':
                    break;
                  default:
                    break;
                }

                break;
              default:
                break;
            }
          }
        }

        // 09/20/99 M.L End
        // 09/21/99 M.L Start
        if (ReadCaseRole7())
        {
          var field1 = GetField(export.NewCaseRole, "type1");

          field1.Error = true;

          var field2 = GetField(export.NewCsePersonsWorkSet, "firstName");

          field2.Error = true;

          var field3 = GetField(export.NewCsePersonsWorkSet, "lastName");

          field3.Error = true;

          ExitState = "SI_0000_PERSON_ROLE_ACTIVE";

          return;
        }

        // 09/21/99 M.L End
        // 03/20/00 M.L Start
        switch(TrimEnd(export.NewCaseRole.Type1))
        {
          case "CH":
            if (IsEmpty(export.NewCsePersonsWorkSet.Number))
            {
              local.NewCaseRole.Type1 = "FA";
              local.NewCaseRole.StartDate = local.Current.Date;
              UseSiVerifyOneActiveCaseRole1();

              if (AsChar(local.FatherOnCase.Flag) == 'Y')
              {
                ExitState = "SI0000_FATHER_ROLE_NOT_ALLOWED";

                return;
              }
            }
            else if (ReadCsePerson3())
            {
              if (AsChar(entities.ChildPaternityEstInd.
                PaternityEstablishedIndicator) == 'N')
              {
                local.NewCaseRole.Type1 = "FA";
                local.NewCaseRole.StartDate = local.Current.Date;
                UseSiVerifyOneActiveCaseRole1();

                if (AsChar(local.FatherOnCase.Flag) == 'Y')
                {
                  ExitState = "SI0000_FATHER_ROLE_NOT_ALLOWED";

                  return;
                }
              }
              else
              {
                // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Can now have multiple
                // APs even if paternity
                //    established=Y.
              }

              // 06/29/2000 M.L Start
              if (!IsEmpty(entities.ChildPaternityEstInd.FamilyViolenceIndicator))
                
              {
                // 03/01/2002 M. Lachowicz
                // Added cse_person type = 'C' to the next READ to make 
                // processing for people not for organizations.
                if (ReadCsePerson1())
                {
                  if (AsChar(entities.CsePerson.FamilyViolenceIndicator) == 'C'
                    || AsChar
                    (entities.ChildPaternityEstInd.FamilyViolenceIndicator) == AsChar
                    (entities.CsePerson.FamilyViolenceIndicator) || AsChar
                    (entities.CsePerson.FamilyViolenceIndicator) == 'P' && AsChar
                    (entities.ChildPaternityEstInd.FamilyViolenceIndicator) == 'D'
                    )
                  {
                  }
                  else
                  {
                    try
                    {
                      UpdateCsePerson1();
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
              }

              // 06/29/2000 M.L End
            }
            else
            {
              // 05/30/00 M.L Start
              // 05/30/00 M.L End
              // CQ20394 Child exists on AE, Facts, Kanpay or KsCares so read 
              // for FA role here also
              local.NewCaseRole.Type1 = "FA";
              local.NewCaseRole.StartDate = local.Current.Date;
              UseSiVerifyOneActiveCaseRole1();

              if (AsChar(local.FatherOnCase.Flag) == 'Y')
              {
                ExitState = "SI0000_FATHER_ROLE_NOT_ALLOWED";

                return;
              }
            }

            break;
          case "AP":
            if (AsChar(export.NewCsePersonsWorkSet.Sex) == 'M')
            {
              // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Can now have multiple 
              // APs even if paternity
              //    established=Y.
            }

            break;
          case "FA":
            if (ReadCsePerson2())
            {
              ExitState = "SI0000_FATHER_ROLE_NOT_ALLOWED";

              return;
            }

            // 03/01/2002 M. Lachowicz Start
            UseSiCheckApFaOverlappingRoles();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // 03/01/2002 M. Lachowicz End
            break;
          default:
            break;
        }

        // 03/20/00 M.L End
        // -----------------------------------------------
        // Create the person on ADABAS and DB2 if necessary
        // -----------------------------------------------
        local.RetreivePersonProgram.Date = export.NewCaseRole.StartDate;

        // ------------------------------------------------
        // 01/29/99 W.Campbell - Added a local view
        // (local_cse ief_supplied flag) to allow for the saving and
        // passing of a flag to indicate that the person being passed
        // is a non-case related CSE person (flag = N).  This
        // flag is being passed to SI_VERIFY_AND_CREATE_CSE_PERSON
        // used in the logic below.  Work done on IDCR477.
        // The flag is view matched to the import view below.
        // ------------------------------------------------
        UseSiVerifyAndCreateCsePerson();

        if (!IsEmpty(local.AbendData.Type1))
        {
          UseEabRollbackCics();

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        local.CsePerson.Number = export.NewCsePersonsWorkSet.Number;

        if (!Equal(export.NewCsePersonsWorkSet.Ssn,
          local.NewSsnWorkArea.SsnText9))
        {
          local.NewSsnWorkArea.SsnText9 = export.NewCsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        // ---------------------------------------------
        // Create a Case Role - Note that the AR Case Role is never
        // created with the Create_Case_Role action block.
        // ---------------------------------------------
        local.FromRole.Flag = "Y";
        UseSiCreateCaseRole();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        if (Equal(export.NewCaseRole.Type1, "AP") || Equal
          (export.NewCaseRole.Type1, "CH"))
        {
          MoveCaseRole1(export.NewCaseRole, local.NewCaseRole);
        }

        if (Equal(export.NewCaseRole.Type1, "AP"))
        {
          UseLeCabCopyLaCaseroles();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        // ------------------------------------------------------------
        // If the new role is FA and the person is not on the case as
        // an AP and not the AR, create a case role of AP for the new
        // person.
        // ------------------------------------------------------------
        if (Equal(export.NewCaseRole.Type1, "FA"))
        {
          local.CaseRole.Type1 = "AP";
          UseSiCheckCaseRoleCombinations();

          if (AsChar(local.Common.Flag) != 'Y')
          {
            local.CaseRole.Type1 = "AR";
            UseSiCheckCaseRoleCombinations();

            if (AsChar(local.Common.Flag) == 'Y')
            {
              local.SetNewRelToAr.Flag = "Y";
            }
            else
            {
              // ------------------------------------
              // 1/29/99 WCampbell - Disabled code (since which deleted)
              // that automatically created case role = AP when creating a
              // case role = FA.
              // ------------------------------------
            }
          }

          // ------------------------------------
          // If the new role is FA and the person is already on the case as
          // an AP then end date all other male AP's.
          // ------------------------------------
          // 03/01/2002 M.L Start
          // 031/01/2002 M.L End
        }

        if (!IsEmpty(local.NewCaseRole.Type1))
        {
          UseSiAddCuAndCufaForCaseRole();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        // 03/01/2002 M. Lachowicz Start
        // 03/01/2002 M. Lachowicz End
        // ------------------------------------------------------------
        // If the newly added person is a MO or FA and the AR find all
        // the active children on the case and update their case_role.
        // ------------------------------------------------------------
        if (AsChar(local.SetNewRelToAr.Flag) == 'Y')
        {
          MoveCaseRole1(export.NewCaseRole, local.CaseRole);
          UseSiFindActiveKidsForACase();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        // 03/20/00 M.L Start
        // 05/17/00 M.L Start
        if (!Equal(export.NewCaseRole.Type1, "FA") && !
          Equal(export.NewCaseRole.Type1, "MO"))
        {
          export.ChildSuccessfullyAdded.Flag = "Y";
        }

        // 05/17/00 M.L End
        // 03/20/00 M.L End
        // 05/08/2002 M.Lachowicz Start - enabled changes
        // 02/28/2002 M.Lachowicz Start
        if (Equal(export.NewCaseRole.Type1, "CH") || Equal
          (export.NewCaseRole.Type1, "AP"))
        {
          local.ScreenIdentification.Command = "ROLE";
          UseSiCreateAutoCsenetTrans1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        // 02/28/2002 M.Lachowicz End
        // 05/08/2002 M.Lachowicz End - enabled changes
        export.NewCsePersonsWorkSet.Assign(local.BlankCsePersonsWorkSet);
        export.NewSsnWorkArea.SsnNumPart1 = 0;
        export.NewSsnWorkArea.SsnNumPart2 = 0;
        export.NewSsnWorkArea.SsnNumPart3 = 0;
        MoveCaseRole1(local.BlankCaseRole, export.NewCaseRole);
        export.HiddenStandard.PageNumber = 1;
        local.SuccessfulAdd.Flag = "Y";
        global.Command = "DISPLAY";

        break;
      case "UPDATE":
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AR"))
              {
                // ------------------------------------------------
                // No changes are allowed here for the AR.  Only
                // way to change the AR is to add a new one.
                // ------------------------------------------------
                // 	
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Error = true;

                ExitState = "CANNOT_CHANGE_AR_DETAILS";

                return;
              }

              local.DateWorkArea.Date =
                export.Export1.Item.DetailCaseRole.EndDate;
              export.Export1.Update.DetailCaseRole.EndDate =
                UseCabSetMaximumDiscontinueDate2();

              if (Lt(export.Export1.Item.DetailCaseRole.EndDate,
                export.Export1.Item.DetailCaseRole.StartDate))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCaseRole, "endDate");

                field1.Error = true;

                ExitState = "ACO_NE0000_END_LESS_THAN_START";

                return;
              }

              // ------------------------------------------------
              // The following IF statement was added by W.Campbell on 09/17/98.
              // This is to prevent an end_date in the future.
              // ------------------------------------------------
              // ---------------------------------------
              // 04/27/99 W.Campbell - Replaced use of CURRENT DATE with view of
              // local current date.
              // ---------------------------------------
              if (Lt(local.Current.Date,
                export.Export1.Item.DetailCaseRole.EndDate))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCaseRole, "endDate");

                field1.Error = true;

                ExitState = "END_DT_GREATER_THAN_CURRENT_DT";

                return;
              }

              // 06/25/99 M.L
              //              Change property of the following READ to generate
              //              SELECT ONLY
              if (ReadCase())
              {
                // 06/25/99 M.L
                //              Change property of the following READ to 
                // generate
                //              SELECT ONLY
                if (ReadCaseRole9())
                {
                  // >>
                  // 09/06/02  T.Bobb   PR00155688
                  // Cannot end AP role when when active in open interstate case
                  if (Equal(entities.CaseRole.Type1, "AP") && !
                    Lt(Now().Date, import.Import1.Item.DetailCaseRole.EndDate))
                  {
                    if (ReadInterstateRequest())
                    {
                      ExitState = "SI0000_MUST_CLOSE_INTERSTAT_CASE";

                      return;
                    }
                  }

                  // 05/23/00 M.L
                  //              Add last_updated_timestamp and last_updated_by
                  //              attributes to UPDATE statment
                  try
                  {
                    UpdateCaseRole();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "CASE_ROLE_AE";

                        break;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "CASE_ROLE_PV";

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
                  ExitState = "CASE_ROLE_NF";
                }
              }
              else
              {
                ExitState = "CASE_NF";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Error = true;

                UseEabRollbackCics();

                return;
              }

              UseSiCloseCaseUnits();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Error = true;

                UseEabRollbackCics();

                return;
              }

              switch(TrimEnd(export.Export1.Item.DetailCaseRole.Type1))
              {
                case "CH":
                  // The current CHild Case Role is being discontinued.  The 
                  // following action block raises the appropriate
                  // Infrastructure occurrences and discontinues impacted
                  // Monitored Activities and their assignments.
                  UseSpRaiseChApDiscEvents();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    var field1 =
                      GetField(export.Export1.Item.DetailCommon, "selectChar");

                    field1.Error = true;

                    UseEabRollbackCics();

                    return;
                  }

                  break;
                case "AP":
                  // The current AP Case Role is being discontinued.  The 
                  // following action block raises the appropriate
                  // Infrastructure occurrences and discontinues impacted
                  // Monitored Activities and their assignments.
                  UseSpRaiseChApDiscEvents();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    var field1 =
                      GetField(export.Export1.Item.DetailCommon, "selectChar");

                    field1.Error = true;

                    UseEabRollbackCics();

                    return;
                  }

                  break;
                case "FA":
                  // The current FAther Case Role is being discontinued.  The 
                  // following action block raises the appropriate
                  // Infrastructure occurrences.
                  UseSpRaiseFaMoDiscEvents();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    var field1 =
                      GetField(export.Export1.Item.DetailCommon, "selectChar");

                    field1.Error = true;

                    UseEabRollbackCics();

                    return;
                  }

                  break;
                case "MO":
                  // The current MOther Case Role is being discontinued.  The 
                  // following action block raises the appropriate
                  // Infrastructure occurrences.
                  UseSpRaiseFaMoDiscEvents();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    var field1 =
                      GetField(export.Export1.Item.DetailCommon, "selectChar");

                    field1.Error = true;

                    UseEabRollbackCics();

                    return;
                  }

                  break;
                default:
                  break;
              }

              // 05/08/2002 M.Lachowicz Start - enabled changes
              // 02/28/2002 M.Lachowicz Start
              if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH") || Equal
                (export.Export1.Item.DetailCaseRole.Type1, "AP"))
              {
                local.ScreenIdentification.Command = "ROLE";
                UseSiCreateAutoCsenetTrans2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseEabRollbackCics();

                  return;
                }
              }

              // 02/28/2002 M.Lachowicz End
              // 05/08/2002 M.Lachowicz End - enabled changes
              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
          }
        }

        export.Export1.CheckIndex();
        global.Command = "DISPLAY";
        export.HiddenStandard.PageNumber = 1;
        local.SuccessfulUpdate.Flag = "Y";

        break;
      case "NAME":
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        return;
      case "ORGZ":
        // ---------------------------------------------
        // 02/12/99 W.Campbell - PFK16 (Command=ORGZ)
        // disabled.  PF16 removed as Screen literal,
        // and code disabled.  This may need to be
        // reinstalled again in the future.
        // ---------------------------------------------
        // ---------------------------------------------
        // 06/25/99 M.Lachowicz - Enable PFK16 (Command=ORGZ).
        // ---------------------------------------------
        ExitState = "ECO_LNK_TO_ORG_MAINTENANCE";

        return;
      case "COMN":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            // 10/06/99 M.L Start
            if (CharAt(export.Export1.Item.DetailCsePerson.Number, 10) == 'O')
            {
              var field1 =
                GetField(export.Export1.Item.DetailCsePerson, "number");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                "formattedName");

              field2.Error = true;

              ExitState = "SI0000_YOU_MUST_SELECT_PERSON";

              return;
            }

            // 10/06/99 M.L End
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Selected.Number = export.Export1.Item.DetailCsePerson.Number;
            ExitState = "ECO_LNK_TO_COMN";

            return;
          }
        }

        export.Export1.CheckIndex();
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      case "DISPLAY":
        if (!Equal(export.NewCsePersonsWorkSet.Ssn,
          local.NewSsnWorkArea.SsnText9))
        {
          local.NewSsnWorkArea.SsnText9 = export.NewCsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();
        }

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "SIGNOFF":
        if (AsChar(export.HiddenReturnRequired.Flag) == 'Y')
        {
          ExitState = "LE0000_USE_PF9_TO_RETURN";

          return;
        }

        UseScCabSignoff();

        return;
      case "RETURN":
        // ------------------------------------
        //  Retrofit by RB Mohapatra  01/22/97
        // ------------------------------------
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
            ;
        }
        else
        {
          if (AsChar(import.FromCads.Flag) == 'Y')
          {
            ExitState = "ACO_NE0000_RETURN_NM";

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
              {
                export.Export1.Update.DetailCommon.SelectChar = "";

                if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AR"))
                {
                  export.Selected.Number =
                    export.Export1.Item.DetailCsePerson.Number;

                  return;
                }
                else
                {
                  var field1 =
                    GetField(export.Export1.Item.DetailCaseRole, "type1");

                  field1.Error = true;

                  var field2 =
                    GetField(export.Export1.Item.DetailCommon, "selectChar");

                  field2.Error = true;

                  var field3 =
                    GetField(export.Export1.Item.DetailCsePerson, "number");

                  field3.Error = true;

                  ExitState = "AR_RETURN_TO_CADS";

                  return;
                }
              }
            }

            export.Export1.CheckIndex();

            return;
          }

          ExitState = "ACO_NE0000_RETURN";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
              export.Selected.Number =
                export.Export1.Item.DetailCsePerson.Number;

              return;
            }
          }

          export.Export1.CheckIndex();
        }

        break;
      default:
        break;
    }

    // ---------------------------------------------
    // If a display is required, call the action
    // block that reads the next group of data based
    // on the page number.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      if (IsEmpty(export.Case1.Number))
      {
        if (IsEmpty(export.Next.Number))
        {
          var field1 = GetField(export.Next, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          export.Case1.Number = export.Next.Number;
        }
      }
      else if (!IsEmpty(export.Next.Number))
      {
        if (!Equal(export.Case1.Number, export.Next.Number))
        {
          export.Case1.Number = export.Next.Number;
        }
      }

      UseSiReadOfficeOspHeader();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.Export1.Count = 0;

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      UseSiReadCaseRolesByCase();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKeyCaseRole.EndDate =
        local.NextCaseRole.EndDate;

      // Marek L.      05/12/1999
      // Set Start_Date in Page_Key export group view to value passed by 
      // si_read_case_roles_by_case
      export.HiddenPageKeys.Update.HiddenPageKeyCaseRole.StartDate =
        local.NextCaseRole.StartDate;
      export.HiddenPageKeys.Update.HiddenPageKeyCsePerson.Number =
        local.NextCsePerson.Number;

      export.Export1.Index = 0;
      export.Export1.CheckSize();

      var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

      field.Protected = false;
      field.Focused = true;

      if (!export.Export1.IsEmpty)
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        if (AsChar(export.Case1.Status) == 'C')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
        }
      }
      else
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
    }

    if (AsChar(export.SelectAction.Flag) == 'Y')
    {
      // ----------------------------------
      // If prompted for an action, protect all fields.
      // ----------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        var field11 = GetField(export.Export1.Item.DetailCommon, "selectChar");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.Export1.Item.DetailCaseRole, "endDate");

        field12.Color = "cyan";
        field12.Protected = true;
      }

      export.Export1.CheckIndex();

      var field1 = GetField(export.NewCsePersonsWorkSet, "lastName");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.NewCsePersonsWorkSet, "firstName");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.NewCsePersonsWorkSet, "middleInitial");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.NewCsePersonsWorkSet, "sex");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.NewCsePersonsWorkSet, "dob");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.NewSsnWorkArea, "ssnNumPart1");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.NewSsnWorkArea, "ssnNumPart2");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.NewSsnWorkArea, "ssnNumPart3");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.NewCaseRole, "startDate");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.NewCaseRole, "type1");

      field10.Color = "cyan";
      field10.Protected = true;
    }
    else
    {
      // ------------------------------------------------
      // If the case is closed, protect all fields except the selection 
      // character.
      // ------------------------------------------------
      if (AsChar(export.Case1.Status) == 'C')
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          var field = GetField(export.Export1.Item.DetailCaseRole, "endDate");

          field.Color = "cyan";
          field.Protected = true;
        }

        export.Export1.CheckIndex();

        var field1 = GetField(export.NewCsePersonsWorkSet, "number");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.NewCsePersonsWorkSet, "firstName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.NewCsePersonsWorkSet, "lastName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.NewCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.NewCsePersonsWorkSet, "sex");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.NewCsePersonsWorkSet, "dob");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.NewSsnWorkArea, "ssnNumPart1");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.NewSsnWorkArea, "ssnNumPart2");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.NewSsnWorkArea, "ssnNumPart3");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.NewCaseRole, "type1");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.NewCaseRole, "startDate");

        field11.Color = "cyan";
        field11.Protected = true;
      }
      else
      {
        // 10/28/99 M.L Start
        var field1 = GetField(export.NewCsePersonsWorkSet, "firstName");

        field1.Color = "green";
        field1.Highlighting = Highlighting.Underscore;
        field1.Protected = false;

        var field2 = GetField(export.NewCsePersonsWorkSet, "lastName");

        field2.Color = "green";
        field2.Highlighting = Highlighting.Underscore;
        field2.Protected = false;

        var field3 = GetField(export.NewCsePersonsWorkSet, "middleInitial");

        field3.Color = "green";
        field3.Highlighting = Highlighting.Underscore;
        field3.Protected = false;

        var field4 = GetField(export.NewCsePersonsWorkSet, "sex");

        field4.Color = "green";
        field4.Highlighting = Highlighting.Underscore;
        field4.Protected = false;

        var field5 = GetField(export.NewCsePersonsWorkSet, "dob");

        field5.Color = "green";
        field5.Highlighting = Highlighting.Underscore;
        field5.Protected = false;

        var field6 = GetField(export.NewSsnWorkArea, "ssnNumPart1");

        field6.Color = "green";
        field6.Highlighting = Highlighting.Underscore;
        field6.Protected = false;

        var field7 = GetField(export.NewSsnWorkArea, "ssnNumPart2");

        field7.Color = "green";
        field7.Highlighting = Highlighting.Underscore;
        field7.Protected = false;

        var field8 = GetField(export.NewSsnWorkArea, "ssnNumPart3");

        field8.Color = "green";
        field8.Highlighting = Highlighting.Underscore;
        field8.Protected = false;

        var field9 = GetField(export.NewCaseRole, "type1");

        field9.Color = "green";
        field9.Highlighting = Highlighting.Underscore;
        field9.Protected = false;

        var field10 = GetField(export.NewCaseRole, "startDate");

        field10.Color = "green";
        field10.Highlighting = Highlighting.Underscore;
        field10.Protected = false;

        // 10/28/99 M.L End
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          // ------------------------------------------------
          // If the case role is inactive or the role is AR,
          // protect that occurrence
          // ------------------------------------------------
          // ---------------------------------------
          // 04/27/99 W.Campbell - Replaced use of CURRENT DATE
          // with view of local current date.
          // ---------------------------------------
          if (Lt(export.Export1.Item.DetailCaseRole.EndDate, local.Current.Date) &&
            !
            Equal(export.Export1.Item.DetailCaseRole.EndDate, local.Null1.Date) ||
            Equal(export.Export1.Item.DetailCaseRole.Type1, "AR"))
          {
            var field = GetField(export.Export1.Item.DetailCaseRole, "endDate");

            field.Color = "cyan";
            field.Protected = true;
          }
          else
          {
            var field = GetField(export.Export1.Item.DetailCaseRole, "endDate");

            field.Protected = false;
          }
        }

        export.Export1.CheckIndex();
      }

      if (AsChar(local.SuccessfulAdd.Flag) == 'Y')
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }

      if (AsChar(local.SuccessfulUpdate.Flag) == 'Y')
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
    }

    // ------------------------------------------------
    // 04/09/99 W.Campbell - Check to see if another case exist
    // for any child on this case with a different AR.
    // -----------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      // ------------------------------------------------
      // Obtain the active AR for this case.
      // -----------------------------------------------
      if (ReadCaseRoleCsePerson2())
      {
        // ------------------------------------------------
        // Active AR found for this case. Continue processing.
        // -----------------------------------------------
        local.ArCsePerson.Number = entities.CsePerson.Number;
      }
      else
      {
        // ------------------------------------------------
        // No active AR for this case.
        // -----------------------------------------------
        return;
      }

      // ------------------------------------------------
      // Obtain Each of the active CHildren for this case.
      // -----------------------------------------------
      foreach(var item in ReadCaseRoleCsePerson3())
      {
        // ------------------------------------------------
        // An active CHild has been found for this case.
        // Call CAB to see if another Case exist where
        // this CHild is active with a different active AR.
        // -----------------------------------------------
        UseSiChkCasesForChWithDiffAr();

        // ------------------------------------------------
        // If the local Case number returned from the CAB is not spaces
        // then a Case was found with a different AR for this Child.
        // -----------------------------------------------
        if (!IsEmpty(local.Case1.Number))
        {
          // ------------------------------------------------
          // A Case was found with a different AR for this Child.  Modify the 
          // exit state to also provide warning of this condition.
          // -----------------------------------------------
          if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
          {
            ExitState = "DISP_OK_BUT_OTHER_CASE_FOR_CH";
          }
          else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            ExitState = "ADD_OK_BUT_OTHER_CASE_FOR_CH";
          }
          else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
          {
            ExitState = "UPDATE_OK_BUT_OTHER_CASE_FOR_CH";
          }
        }
      }
    }

    // ------------------------------------------------
    // 04/09/99 W.Campbell - End of new logic
    // added for checking to see if
    // another case exist for any child on
    // this case with a different AR.
    // -----------------------------------------------
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCaseRole1(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
  }

  private static void MoveCaseRole2(CaseRole source, CaseRole target)
  {
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.ReplicationIndicator = source.ReplicationIndicator;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveExport1(SiReadCaseRolesByCase.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.DetailCsePerson.Number = source.DetailCsePerson.Number;
    MoveCsePersonsWorkSet1(source.DetailCsePersonsWorkSet,
      target.DetailCsePersonsWorkSet);
    target.DetailCaseRole.Assign(source.DetailCaseRole);
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabReadAdabasPerson1()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = export.NewCsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.Cse.Flag = useExport.Cse.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.NewCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabReadAdabasPerson2()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Number = local.ActiveAp.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.ActiveAp.Assign(useExport.CsePersonsWorkSet);
    local.AbendData.Assign(useExport.AbendData);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.NewSsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    export.NewSsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.NewSsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    export.NewSsnWorkArea.Assign(useExport.SsnWorkArea);
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeCabCopyLaCaseroles()
  {
    var useImport = new LeCabCopyLaCaseroles.Import();
    var useExport = new LeCabCopyLaCaseroles.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(LeCabCopyLaCaseroles.Execute, useImport, useExport);
  }

  private void UseOeCabReadInfrastructure()
  {
    var useImport = new OeCabReadInfrastructure.Import();
    var useExport = new OeCabReadInfrastructure.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.LastTran.SystemGeneratedIdentifier;

    Call(OeCabReadInfrastructure.Execute, useImport, useExport);

    local.LastTran.Assign(useExport.Infrastructure);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiAddCuAndCufaForCaseRole()
  {
    var useImport = new SiAddCuAndCufaForCaseRole.Import();
    var useExport = new SiAddCuAndCufaForCaseRole.Export();

    MoveCaseRole1(local.NewCaseRole, useImport.CaseRole);
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiAddCuAndCufaForCaseRole.Execute, useImport, useExport);
  }

  private void UseSiChangeAr()
  {
    var useImport = new SiChangeAr.Import();
    var useExport = new SiChangeAr.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.NewArCsePersonsWorkSet.Number =
      export.NewCsePersonsWorkSet.Number;
    MoveCaseRole1(export.NewCaseRole, useImport.NewArCaseRole);

    Call(SiChangeAr.Execute, useImport, useExport);
  }

  private void UseSiChangeArFvIndicator()
  {
    var useImport = new SiChangeArFvIndicator.Import();
    var useExport = new SiChangeArFvIndicator.Export();

    useImport.CsePerson.Assign(entities.CsePerson);
    useImport.Case1.Number = export.Case1.Number;

    Call(SiChangeArFvIndicator.Execute, useImport, useExport);
  }

  private void UseSiCheckApFaOverlappingRoles()
  {
    var useImport = new SiCheckApFaOverlappingRoles.Import();
    var useExport = new SiCheckApFaOverlappingRoles.Export();

    useImport.FaCsePersonsWorkSet.Number = export.NewCsePersonsWorkSet.Number;
    useImport.FaCaseRole.StartDate = import.NewCaseRole.StartDate;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiCheckApFaOverlappingRoles.Execute, useImport, useExport);
  }

  private void UseSiCheckCaseRoleCombinations()
  {
    var useImport = new SiCheckCaseRoleCombinations.Import();
    var useExport = new SiCheckCaseRoleCombinations.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Verify.Type1 = local.CaseRole.Type1;
    useImport.Case1.Number = export.Case1.Number;
    useImport.New1.StartDate = export.NewCaseRole.StartDate;

    Call(SiCheckCaseRoleCombinations.Execute, useImport, useExport);

    local.Common.Flag = useExport.Common.Flag;
  }

  private void UseSiCheckName()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    useImport.CsePersonsWorkSet.Assign(export.NewCsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiChkCasesForChWithDiffAr()
  {
    var useImport = new SiChkCasesForChWithDiffAr.Import();
    var useExport = new SiChkCasesForChWithDiffAr.Export();

    useImport.Ar.Number = local.ArCsePerson.Number;
    useImport.Ch.Number = entities.ChCsePerson.Number;

    Call(SiChkCasesForChWithDiffAr.Execute, useImport, useExport);

    local.Case1.Number = useExport.Case1.Number;
  }

  private void UseSiCloseCaseUnits()
  {
    var useImport = new SiCloseCaseUnits.Import();
    var useExport = new SiCloseCaseUnits.Export();

    MoveCase1(export.Case1, useImport.Case1);
    useImport.CsePerson.Number = export.Export1.Item.DetailCsePerson.Number;
    useImport.CaseRole.Assign(export.Export1.Item.DetailCaseRole);

    Call(SiCloseCaseUnits.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans1()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    MoveCase1(import.Case1, useImport.Case1);
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans2()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    MoveCase1(import.Case1, useImport.Case1);
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.CsePerson.Number = export.Export1.Item.DetailCsePerson.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans3()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    MoveCase1(import.Case1, useImport.Case1);
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.CsePerson.Number = local.ArCsePerson.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans4()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    MoveCase1(import.Case1, useImport.Case1);
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.CsePerson.Number = local.NewCsePerson.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreateCaseRole()
  {
    var useImport = new SiCreateCaseRole.Import();
    var useExport = new SiCreateCaseRole.Export();

    useImport.FromRole.Flag = local.FromRole.Flag;
    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePersonsWorkSet.Number = export.NewCsePersonsWorkSet.Number;
    MoveCaseRole1(export.NewCaseRole, useImport.CaseRole);

    Call(SiCreateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiFindActiveKidsForACase()
  {
    var useImport = new SiFindActiveKidsForACase.Import();
    var useExport = new SiFindActiveKidsForACase.Export();

    MoveCaseRole1(local.CaseRole, useImport.CaseRole);
    useImport.Case1.Number = export.Case1.Number;

    Call(SiFindActiveKidsForACase.Execute, useImport, useExport);
  }

  private void UseSiFvEvent()
  {
    var useImport = new SiFvEvent.Import();
    var useExport = new SiFvEvent.Export();

    useImport.CsePerson.Assign(entities.CsePerson);

    Call(SiFvEvent.Execute, useImport, useExport);
  }

  private void UseSiReadCaseRolesByCase()
  {
    var useImport = new SiReadCaseRolesByCase.Import();
    var useExport = new SiReadCaseRolesByCase.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Standard.PageNumber = export.HiddenStandard.PageNumber;
    MoveCaseRole2(export.HiddenPageKeys.Item.HiddenPageKeyCaseRole,
      useImport.PageKeyCaseRole);
    useImport.PageKeyCsePerson.Number =
      export.HiddenPageKeys.Item.HiddenPageKeyCsePerson.Number;

    Call(SiReadCaseRolesByCase.Execute, useImport, useExport);

    MoveCaseRole2(useExport.PageKeyCaseRole, local.NextCaseRole);
    local.NextCsePerson.Number = useExport.PageKeyCsePerson.Number;
    local.AbendData.Assign(useExport.AbendData);
    export.Case1.Assign(useExport.Case1);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private void UseSiVerifyAndCreateCsePerson()
  {
    var useImport = new SiVerifyAndCreateCsePerson.Import();
    var useExport = new SiVerifyAndCreateCsePerson.Export();

    useImport.Cse.Flag = local.Cse.Flag;
    useImport.CsePersonsWorkSet.Assign(export.NewCsePersonsWorkSet);
    useImport.RetreivePersonProgram.Date = local.RetreivePersonProgram.Date;

    Call(SiVerifyAndCreateCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet,
      export.NewCsePersonsWorkSet);
  }

  private void UseSiVerifyChildIsOnACase()
  {
    var useImport = new SiVerifyChildIsOnACase.Import();
    var useExport = new SiVerifyChildIsOnACase.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.CaseRole.StartDate = export.NewCaseRole.StartDate;

    Call(SiVerifyChildIsOnACase.Execute, useImport, useExport);

    local.RelToArIsCh.Flag = useExport.RelToArIsCh.Flag;
    MoveCommon(useExport.ActiveCaseCh, local.ActiveCaseCh);
  }

  private void UseSiVerifyNotLastChOnCase()
  {
    var useImport = new SiVerifyNotLastChOnCase.Import();
    var useExport = new SiVerifyNotLastChOnCase.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiVerifyNotLastChOnCase.Execute, useImport, useExport);

    local.OtherChOnCase.Flag = useExport.OtherChOnCase.Flag;
    local.ChOnCase.Flag = useExport.ChOnCase.Flag;
  }

  private void UseSiVerifyOneActiveCaseRole1()
  {
    var useImport = new SiVerifyOneActiveCaseRole.Import();
    var useExport = new SiVerifyOneActiveCaseRole.Export();

    MoveCaseRole1(local.NewCaseRole, useImport.CaseRole);
    useImport.Case1.Number = export.Case1.Number;

    Call(SiVerifyOneActiveCaseRole.Execute, useImport, useExport);

    local.FatherOnCase.Flag = useExport.Common.Flag;
  }

  private void UseSiVerifyOneActiveCaseRole2()
  {
    var useImport = new SiVerifyOneActiveCaseRole.Import();
    var useExport = new SiVerifyOneActiveCaseRole.Export();

    MoveCaseRole1(local.CaseRole, useImport.CaseRole);
    useImport.Case1.Number = export.Case1.Number;

    Call(SiVerifyOneActiveCaseRole.Execute, useImport, useExport);

    local.Error.Flag = useExport.Common.Flag;
  }

  private void UseSiVerifyOneActiveCaseRole3()
  {
    var useImport = new SiVerifyOneActiveCaseRole.Import();
    var useExport = new SiVerifyOneActiveCaseRole.Export();

    useImport.Case1.Number = export.Case1.Number;
    MoveCaseRole1(export.NewCaseRole, useImport.CaseRole);

    Call(SiVerifyOneActiveCaseRole.Execute, useImport, useExport);

    local.Error.Flag = useExport.Common.Flag;
  }

  private void UseSpRaiseChApDiscEvents()
  {
    var useImport = new SpRaiseChApDiscEvents.Import();
    var useExport = new SpRaiseChApDiscEvents.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.Export1.Item.DetailCsePerson.Number;
    useImport.CaseRole.Assign(export.Export1.Item.DetailCaseRole);

    Call(SpRaiseChApDiscEvents.Execute, useImport, useExport);
  }

  private void UseSpRaiseFaMoDiscEvents()
  {
    var useImport = new SpRaiseFaMoDiscEvents.Import();
    var useExport = new SpRaiseFaMoDiscEvents.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.Export1.Item.DetailCsePerson.Number;
    useImport.CaseRole.Assign(export.Export1.Item.DetailCaseRole);

    Call(SpRaiseFaMoDiscEvents.Execute, useImport, useExport);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
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
      });
  }

  private bool ReadCaseRole2()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "cspNumber", local.CsePerson.Number);
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
      });
  }

  private bool ReadCaseRole3()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
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
      });
  }

  private bool ReadCaseRole4()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          export.NewCaseRole.StartDate.GetValueOrDefault());
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
      });
  }

  private bool ReadCaseRole5()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
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
      });
  }

  private bool ReadCaseRole6()
  {
    entities.ChCaseRole.Populated = false;

    return Read("ReadCaseRole6",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.NewCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ChCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ChCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChCaseRole.Type1 = db.GetString(reader, 2);
        entities.ChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ChCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ChCaseRole.Type1);
      });
  }

  private bool ReadCaseRole7()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole7",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.NewCsePersonsWorkSet.Number);
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "type", export.NewCaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
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
      });
  }

  private bool ReadCaseRole8()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole8",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.NewCsePersonsWorkSet.Number);
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
      });
  }

  private bool ReadCaseRole9()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole9",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "type", export.Export1.Item.DetailCaseRole.Type1);
        db.SetInt32(
          command, "caseRoleId", export.Export1.Item.DetailCaseRole.Identifier);
          
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
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "numb", export.NewCsePersonsWorkSet.Number);
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
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CsePerson.Type1 = db.GetString(reader, 8);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 11);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 12);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 13);
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
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
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
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CsePerson.Type1 = db.GetString(reader, 8);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 11);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 12);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 13);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson3()
  {
    entities.ChCsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.ChCsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ChCsePerson.Populated = true;
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
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 6);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ChildPaternityEstInd.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChildPaternityEstInd.Number = db.GetString(reader, 0);
        entities.ChildPaternityEstInd.Type1 = db.GetString(reader, 1);
        entities.ChildPaternityEstInd.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.ChildPaternityEstInd.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 3);
        entities.ChildPaternityEstInd.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChildPaternityEstInd.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.ChildPaternityEstInd.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", export.NewCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ChildPaternityEstInd.Number = db.GetString(reader, 0);
        entities.ChildPaternityEstInd.Type1 = db.GetString(reader, 1);
        entities.ChildPaternityEstInd.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.ChildPaternityEstInd.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 3);
        entities.ChildPaternityEstInd.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChildPaternityEstInd.Type1);
      });
  }

  private bool ReadCsePerson4()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "numb1", entities.CaseRole.CspNumber);
        db.SetString(command, "numb2", export.NewCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 6);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson5()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson5",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 6);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson6()
  {
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCsePerson6",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "startDate", date);
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChCsePerson.Number = db.GetString(reader, 0);
        entities.ChCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson7()
  {
    entities.ChildPaternityEstInd.Populated = false;

    return ReadEach("ReadCsePerson7",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChildPaternityEstInd.Number = db.GetString(reader, 0);
        entities.ChildPaternityEstInd.Type1 = db.GetString(reader, 1);
        entities.ChildPaternityEstInd.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.ChildPaternityEstInd.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 3);
        entities.ChildPaternityEstInd.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChildPaternityEstInd.Type1);

        return true;
      });
  }

  private bool ReadCsePersonCaseRole()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 4);
        entities.CsePerson.FviSetDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.FviUpdatedBy = db.GetNullableString(reader, 6);
        entities.CaseRole.CasNumber = db.GetString(reader, 7);
        entities.CaseRole.Type1 = db.GetString(reader, 8);
        entities.CaseRole.Identifier = db.GetInt32(reader, 9);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.CaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CaseRole.LastUpdatedBy = db.GetNullableString(reader, 13);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadDisbursementTransaction()
  {
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.ArCsePerson.Number);
        db.SetNullableDate(
          command, "processDate",
          export.NewCaseRole.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 3);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableInt32(command, "croId1", entities.CaseRole.Identifier);
        db.SetNullableString(command, "croType", entities.CaseRole.Type1);
        db.SetNullableString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetNullableString(command, "casNumber", entities.CaseRole.CasNumber);
        db.SetNullableInt32(
          command, "croId2", export.Export1.Item.DetailCaseRole.Identifier);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 6);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.Ep.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDt", date);
        db.SetNullableString(command, "cspNumber", entities.ChCsePerson.Number);
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ep.LgaIdentifier = db.GetInt32(reader, 0);
        entities.Ep.Number = db.GetInt32(reader, 1);
        entities.Ep.EndDate = db.GetNullableDate(reader, 2);
        entities.Ep.EffectiveDate = db.GetDate(reader, 3);
        entities.Ep.NonFinOblgType = db.GetNullableString(reader, 4);
        entities.Ep.DetailType = db.GetString(reader, 5);
        entities.Ep.Populated = true;
        CheckValid<LegalActionDetail>("DetailType", entities.Ep.DetailType);
      });
  }

  private bool ReadLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.Ep.Populated);
    entities.ObligorLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableInt32(command, "ladRNumber", entities.Ep.Number);
        db.
          SetNullableInt32(command, "lgaRIdentifier", entities.Ep.LgaIdentifier);
          
        db.SetDate(command, "effectiveDt", date);
        db.SetNullableString(
          command, "cspNumber", export.NewCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ObligorLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ObligorLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ObligorLegalActionPerson.EffectiveDate = db.GetDate(reader, 2);
        entities.ObligorLegalActionPerson.EndDate =
          db.GetNullableDate(reader, 3);
        entities.ObligorLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.ObligorLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 5);
        entities.ObligorLegalActionPerson.AccountType =
          db.GetNullableString(reader, 6);
        entities.ObligorLegalActionPerson.Populated = true;
      });
  }

  private void UpdateCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);

    var endDate = export.Export1.Item.DetailCaseRole.EndDate;
    var lastUpdatedTimestamp = local.Current.Timestamp;
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

  private void UpdateCsePerson1()
  {
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var familyViolenceIndicator =
      entities.ChildPaternityEstInd.FamilyViolenceIndicator;
    var fviSetDate = local.Current.Date;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson1",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "familyViolInd", familyViolenceIndicator);
        db.SetNullableDate(command, "fviSetDate", fviSetDate);
        db.SetNullableString(command, "fviUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.FamilyViolenceIndicator = familyViolenceIndicator;
    entities.CsePerson.FviSetDate = fviSetDate;
    entities.CsePerson.FviUpdatedBy = lastUpdatedBy;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePerson2()
  {
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var familyViolenceIndicator = local.CsePerson.FamilyViolenceIndicator ?? "";
    var fviSetDate = local.Current.Date;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "familyViolInd", familyViolenceIndicator);
        db.SetNullableDate(command, "fviSetDate", fviSetDate);
        db.SetNullableString(command, "fviUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.FamilyViolenceIndicator = familyViolenceIndicator;
    entities.CsePerson.FviSetDate = fviSetDate;
    entities.CsePerson.FviUpdatedBy = lastUpdatedBy;
    entities.CsePerson.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common detailCommon;
      private CsePerson detailCsePerson;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKeyCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCaseRole")]
      public CaseRole HiddenPageKeyCaseRole
      {
        get => hiddenPageKeyCaseRole ??= new();
        set => hiddenPageKeyCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenPageKeyCsePerson.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCsePerson")]
      public CsePerson HiddenPageKeyCsePerson
      {
        get => hiddenPageKeyCsePerson ??= new();
        set => hiddenPageKeyCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CaseRole hiddenPageKeyCaseRole;
      private CsePerson hiddenPageKeyCsePerson;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of SelectAction2.
    /// </summary>
    [JsonPropertyName("selectAction2")]
    public Common SelectAction2
    {
      get => selectAction2 ??= new();
      set => selectAction2 = value;
    }

    /// <summary>
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
    }

    /// <summary>
    /// A value of ChildSuccessfullyAdded.
    /// </summary>
    [JsonPropertyName("childSuccessfullyAdded")]
    public Common ChildSuccessfullyAdded
    {
      get => childSuccessfullyAdded ??= new();
      set => childSuccessfullyAdded = value;
    }

    /// <summary>
    /// A value of BeenToName.
    /// </summary>
    [JsonPropertyName("beenToName")]
    public Common BeenToName
    {
      get => beenToName ??= new();
      set => beenToName = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of NewCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("newCsePersonsWorkSet")]
    public CsePersonsWorkSet NewCsePersonsWorkSet
    {
      get => newCsePersonsWorkSet ??= new();
      set => newCsePersonsWorkSet = value;
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
    /// A value of SelectAction.
    /// </summary>
    [JsonPropertyName("selectAction")]
    public Common SelectAction
    {
      get => selectAction ??= new();
      set => selectAction = value;
    }

    /// <summary>
    /// A value of NewSsnWorkArea.
    /// </summary>
    [JsonPropertyName("newSsnWorkArea")]
    public SsnWorkArea NewSsnWorkArea
    {
      get => newSsnWorkArea ??= new();
      set => newSsnWorkArea = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of ReturnFromCpat.
    /// </summary>
    [JsonPropertyName("returnFromCpat")]
    public Common ReturnFromCpat
    {
      get => returnFromCpat ??= new();
      set => returnFromCpat = value;
    }

    /// <summary>
    /// A value of HiddenReturnRequired.
    /// </summary>
    [JsonPropertyName("hiddenReturnRequired")]
    public Common HiddenReturnRequired
    {
      get => hiddenReturnRequired ??= new();
      set => hiddenReturnRequired = value;
    }

    /// <summary>
    /// A value of FromCads.
    /// </summary>
    [JsonPropertyName("fromCads")]
    public Common FromCads
    {
      get => fromCads ??= new();
      set => fromCads = value;
    }

    private WorkArea headerLine;
    private Common selectAction2;
    private Common fromRole;
    private Common childSuccessfullyAdded;
    private Common beenToName;
    private Case1 next;
    private Case1 case1;
    private Standard hiddenStandard;
    private CsePersonsWorkSet newCsePersonsWorkSet;
    private CaseRole newCaseRole;
    private Common selectAction;
    private SsnWorkArea newSsnWorkArea;
    private Standard standard;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Common returnFromCpat;
    private Common hiddenReturnRequired;
    private Common fromCads;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common detailCommon;
      private CsePerson detailCsePerson;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKeyCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCaseRole")]
      public CaseRole HiddenPageKeyCaseRole
      {
        get => hiddenPageKeyCaseRole ??= new();
        set => hiddenPageKeyCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenPageKeyCsePerson.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCsePerson")]
      public CsePerson HiddenPageKeyCsePerson
      {
        get => hiddenPageKeyCsePerson ??= new();
        set => hiddenPageKeyCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CaseRole hiddenPageKeyCaseRole;
      private CsePerson hiddenPageKeyCsePerson;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of SelectAction2.
    /// </summary>
    [JsonPropertyName("selectAction2")]
    public Common SelectAction2
    {
      get => selectAction2 ??= new();
      set => selectAction2 = value;
    }

    /// <summary>
    /// A value of ChildSuccessfullyAdded.
    /// </summary>
    [JsonPropertyName("childSuccessfullyAdded")]
    public Common ChildSuccessfullyAdded
    {
      get => childSuccessfullyAdded ??= new();
      set => childSuccessfullyAdded = value;
    }

    /// <summary>
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
    }

    /// <summary>
    /// A value of BeenToName.
    /// </summary>
    [JsonPropertyName("beenToName")]
    public Common BeenToName
    {
      get => beenToName ??= new();
      set => beenToName = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonsWorkSet Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of NewCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("newCsePersonsWorkSet")]
    public CsePersonsWorkSet NewCsePersonsWorkSet
    {
      get => newCsePersonsWorkSet ??= new();
      set => newCsePersonsWorkSet = value;
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
    /// A value of SelectAction.
    /// </summary>
    [JsonPropertyName("selectAction")]
    public Common SelectAction
    {
      get => selectAction ??= new();
      set => selectAction = value;
    }

    /// <summary>
    /// A value of NewSsnWorkArea.
    /// </summary>
    [JsonPropertyName("newSsnWorkArea")]
    public SsnWorkArea NewSsnWorkArea
    {
      get => newSsnWorkArea ??= new();
      set => newSsnWorkArea = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of ReturnFromCpat.
    /// </summary>
    [JsonPropertyName("returnFromCpat")]
    public Common ReturnFromCpat
    {
      get => returnFromCpat ??= new();
      set => returnFromCpat = value;
    }

    /// <summary>
    /// A value of HiddenReturnRequired.
    /// </summary>
    [JsonPropertyName("hiddenReturnRequired")]
    public Common HiddenReturnRequired
    {
      get => hiddenReturnRequired ??= new();
      set => hiddenReturnRequired = value;
    }

    /// <summary>
    /// A value of FromCads.
    /// </summary>
    [JsonPropertyName("fromCads")]
    public Common FromCads
    {
      get => fromCads ??= new();
      set => fromCads = value;
    }

    private WorkArea headerLine;
    private Common selectAction2;
    private Common childSuccessfullyAdded;
    private Common fromRole;
    private Common beenToName;
    private CsePersonsWorkSet selected;
    private Case1 next;
    private Case1 case1;
    private Standard standard;
    private Standard hiddenStandard;
    private CsePersonsWorkSet newCsePersonsWorkSet;
    private CaseRole newCaseRole;
    private Common selectAction;
    private SsnWorkArea newSsnWorkArea;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<ExportGroup> export1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Common returnFromCpat;
    private Common hiddenReturnRequired;
    private Common fromCads;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BlankSsnWorkArea.
    /// </summary>
    [JsonPropertyName("blankSsnWorkArea")]
    public SsnWorkArea BlankSsnWorkArea
    {
      get => blankSsnWorkArea ??= new();
      set => blankSsnWorkArea = value;
    }

    /// <summary>
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
    }

    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public DateWorkArea Default1
    {
      get => default1 ??= new();
      set => default1 = value;
    }

    /// <summary>
    /// A value of GenerateEvent.
    /// </summary>
    [JsonPropertyName("generateEvent")]
    public Common GenerateEvent
    {
      get => generateEvent ??= new();
      set => generateEvent = value;
    }

    /// <summary>
    /// A value of NewCsePerson.
    /// </summary>
    [JsonPropertyName("newCsePerson")]
    public CsePerson NewCsePerson
    {
      get => newCsePerson ??= new();
      set => newCsePerson = value;
    }

    /// <summary>
    /// A value of FatherOnCase.
    /// </summary>
    [JsonPropertyName("fatherOnCase")]
    public Common FatherOnCase
    {
      get => fatherOnCase ??= new();
      set => fatherOnCase = value;
    }

    /// <summary>
    /// A value of Dup.
    /// </summary>
    [JsonPropertyName("dup")]
    public CsePerson Dup
    {
      get => dup ??= new();
      set => dup = value;
    }

    /// <summary>
    /// A value of ActiveAp.
    /// </summary>
    [JsonPropertyName("activeAp")]
    public CsePersonsWorkSet ActiveAp
    {
      get => activeAp ??= new();
      set => activeAp = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public Common Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of RetreivePersonProgram.
    /// </summary>
    [JsonPropertyName("retreivePersonProgram")]
    public DateWorkArea RetreivePersonProgram
    {
      get => retreivePersonProgram ??= new();
      set => retreivePersonProgram = value;
    }

    /// <summary>
    /// A value of OfAge.
    /// </summary>
    [JsonPropertyName("ofAge")]
    public DateWorkArea OfAge
    {
      get => ofAge ??= new();
      set => ofAge = value;
    }

    /// <summary>
    /// A value of FromRole.
    /// </summary>
    [JsonPropertyName("fromRole")]
    public Common FromRole
    {
      get => fromRole ??= new();
      set => fromRole = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of SetNewRelToAr.
    /// </summary>
    [JsonPropertyName("setNewRelToAr")]
    public Common SetNewRelToAr
    {
      get => setNewRelToAr ??= new();
      set => setNewRelToAr = value;
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
    /// A value of ActiveCaseCh.
    /// </summary>
    [JsonPropertyName("activeCaseCh")]
    public Common ActiveCaseCh
    {
      get => activeCaseCh ??= new();
      set => activeCaseCh = value;
    }

    /// <summary>
    /// A value of SuccessfulUpdate.
    /// </summary>
    [JsonPropertyName("successfulUpdate")]
    public Common SuccessfulUpdate
    {
      get => successfulUpdate ??= new();
      set => successfulUpdate = value;
    }

    /// <summary>
    /// A value of SuccessfulAdd.
    /// </summary>
    [JsonPropertyName("successfulAdd")]
    public Common SuccessfulAdd
    {
      get => successfulAdd ??= new();
      set => successfulAdd = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
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
    /// A value of BlankCaseRole.
    /// </summary>
    [JsonPropertyName("blankCaseRole")]
    public CaseRole BlankCaseRole
    {
      get => blankCaseRole ??= new();
      set => blankCaseRole = value;
    }

    /// <summary>
    /// A value of BlankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("blankCsePersonsWorkSet")]
    public CsePersonsWorkSet BlankCsePersonsWorkSet
    {
      get => blankCsePersonsWorkSet ??= new();
      set => blankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of NextCaseRole.
    /// </summary>
    [JsonPropertyName("nextCaseRole")]
    public CaseRole NextCaseRole
    {
      get => nextCaseRole ??= new();
      set => nextCaseRole = value;
    }

    /// <summary>
    /// A value of NextCsePerson.
    /// </summary>
    [JsonPropertyName("nextCsePerson")]
    public CsePerson NextCsePerson
    {
      get => nextCsePerson ??= new();
      set => nextCsePerson = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of NewSsnWorkArea.
    /// </summary>
    [JsonPropertyName("newSsnWorkArea")]
    public SsnWorkArea NewSsnWorkArea
    {
      get => newSsnWorkArea ??= new();
      set => newSsnWorkArea = value;
    }

    /// <summary>
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
    }

    /// <summary>
    /// A value of OtherChOnCase.
    /// </summary>
    [JsonPropertyName("otherChOnCase")]
    public Common OtherChOnCase
    {
      get => otherChOnCase ??= new();
      set => otherChOnCase = value;
    }

    /// <summary>
    /// A value of ChOnCase.
    /// </summary>
    [JsonPropertyName("chOnCase")]
    public Common ChOnCase
    {
      get => chOnCase ??= new();
      set => chOnCase = value;
    }

    private SsnWorkArea blankSsnWorkArea;
    private Common screenIdentification;
    private DateWorkArea default1;
    private Common generateEvent;
    private CsePerson newCsePerson;
    private Common fatherOnCase;
    private CsePerson dup;
    private CsePersonsWorkSet activeAp;
    private Case1 case1;
    private CsePerson arCsePerson;
    private Common cse;
    private DateWorkArea retreivePersonProgram;
    private DateWorkArea ofAge;
    private Common fromRole;
    private DateWorkArea null1;
    private DateWorkArea current;
    private CaseRole newCaseRole;
    private Common setNewRelToAr;
    private Common relToArIsCh;
    private Common activeCaseCh;
    private Common successfulUpdate;
    private Common successfulAdd;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private DateWorkArea dateWorkArea;
    private CaseRole blankCaseRole;
    private CsePersonsWorkSet blankCsePersonsWorkSet;
    private Common errOnAdabasUnavailable;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Common error;
    private CaseRole nextCaseRole;
    private CsePerson nextCsePerson;
    private AbendData abendData;
    private Common common;
    private SsnWorkArea newSsnWorkArea;
    private Infrastructure lastTran;
    private Common otherChOnCase;
    private Common chOnCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("obligorLegalActionPerson")]
    public LegalActionPerson ObligorLegalActionPerson
    {
      get => obligorLegalActionPerson ??= new();
      set => obligorLegalActionPerson = value;
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
    /// A value of Ep.
    /// </summary>
    [JsonPropertyName("ep")]
    public LegalActionDetail Ep
    {
      get => ep ??= new();
      set => ep = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public LegalActionPerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of ChildPaternityEstInd.
    /// </summary>
    [JsonPropertyName("childPaternityEstInd")]
    public CsePerson ChildPaternityEstInd
    {
      get => childPaternityEstInd ??= new();
      set => childPaternityEstInd = value;
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
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
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

    private CsePerson obligorCsePerson;
    private LegalActionPerson obligorLegalActionPerson;
    private LegalAction legalAction;
    private LegalActionDetail ep;
    private LegalActionPerson supported;
    private CaseRole absentParent;
    private InterstateRequest interstateRequest;
    private CsePersonAccount obligee;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson childPaternityEstInd;
    private CsePerson csePerson;
    private CsePerson chCsePerson;
    private CaseRole chCaseRole;
    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}
