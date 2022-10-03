// Program: SI_REGI_CASE_REGISTRATION, ID: 371727757, model: 746.
// Short name: SWEREGIP
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
/// A program: SI_REGI_CASE_REGISTRATION.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiRegiCaseRegistration: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_REGI_CASE_REGISTRATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRegiCaseRegistration(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRegiCaseRegistration.
  /// </summary>
  public SiRegiCaseRegistration(IContext context, Import import, Export export):
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
    // 05-16-95  Helen Sharland - MTW	Initial Dev
    // 02-01-96  Lewis Tribble		Rearrange CREATE.
    // 06/26/96  G. Lofton - MTW	Changed ssn to numeric fields.
    // 09/10/96  G. Lofton - MTW	Add PF key to COMP
    // 09/12/96  Sid Chowdhary		Force flow back to IAPI when
    // 				coming from there.
    // 11/02/96  G. Lofton - MTW	Add new security.
    // 11/12/96  Ken Evans		Add Case Unit logic.
    // 03/08/97  G. Lofton - MTW	Add Information Request #.
    // 05/20/97  Sid Chowdhary		Cleanup and Fixes.
    // 06/23/97  Sid			Warn user is case is not being
    // 				registered from a referral.
    // 06/25/97  Sid Chowdhary		IDCR # 316 (Create Person Program
    // 				for Cases being created from INRD).
    // 07/02/97  Sid Chowdhary		Create Alias record in AE to mark
    // 				the person as known to CSE.
    // 12/12/97  Sid Chowdhary         Add flow to IIMC.
    // 01/21/98  Sid Chowdhary		IDCR # 408
    // ------------------------------------------------------------
    // 11/12/98  W. Campbell           Added stmt to set an
    //                                 
    // exit state to provide an error
    //                                 
    // msg for no selection on a
    //                                 
    // prompt (PFK=4 'LIST').
    // ---------------------------------------------------------
    // 01/11/99 W.Campbell             Deleted set statements
    //                                 
    // for zdel_verified_code
    //                                 
    // and zdel_start_date.
    //                                 
    // Work done on IDCR454.
    // ----------------------------------------------------
    // 01/30/99 W.Campbell             Added logic to check to
    // 02/01/99                        see if an existing person is
    //                                 
    // a NON-CASE related CSE person
    //                                 
    // (CSE flag = 'N'), and if they
    //                                 
    // are, then change them in
    //                                 
    // ADABAS to a CASE related
    //                                 
    // CSE person (CSE flag = 'Y').
    // ---------------------------------------------
    // 02/08/99 W.Campbell             Added a set
    //                                 
    // statement to set the
    //                                 
    // cse_person number which
    //                                 
    // is being passed to
    //                                 
    // SI_REGI_COPY_ADABAS_PERSON_PGMS
    //                                 
    // to the new person number
    //                                 
    // just created in the
    //                                 
    // command = ADD logic.
    // -------------------------------------------------------
    // 03/19/99 W.Campbell             New code added to
    //                                 
    // satisfy CSEnet requirements.
    // -------------------------------------------------------
    // 03/24/99 W.Campbell             New logic added
    //                                 
    // to support Interstate Cases.
    //                                 
    // Added validation that: If this
    //                                 
    // is an Interstate Case, then the
    //                                 
    // case must have an AP.
    // ------------------------------------------------------------
    // 04/05/99 W.Campbell             New logic added to insure
    //                                 
    // that a CHild on this case
    //                                 
    // (registration) does not exist
    //                                 
    // as an active CHild on another
    //                                 
    // case with a different AR.
    // ------------------------------------------------------------
    // 04/23/99 W.Campbell             Test to make sure
    //                                 
    // that the CH is not the same
    // person
    //                                 
    // as the AR will only be done if
    // the
    //                                 
    // AR has a CSE person number.
    //                                 
    // This change was made to allow
    //                                 
    // for ALL participants in a CASE
    // to
    //                                 
    // come into REGI without any
    //                                 
    // CSE person number.  This was
    //                                 
    // needed for Interstate cases.
    // ------------------------------------------------------------
    // 04/28/99 W.Campbell             Rewrite of CAB
    //                                 
    // si_regi_check_for_duplicate_case
    //                                 
    // to restructure the logic so that
    //                                 
    // it will test all the input APs
    // with
    //                                 
    // the AR and CHildren for a
    //                                 
    // duplicate case.
    // ----------------------------------------------------
    // 05/17/99 W.Campbell             Disabled some statements
    //                                 
    // to execute logic only for
    //                                 
    // child case role.
    // -----------------------------------------------------
    // 06/04/99 W.Campbell             Renamed CAB
    //                                 
    // si_create_is_ack_for_new_ks_case
    // TO
    //                                 
    // si_updt_ic_ref_with_new_ks_case.
    //                                 
    // Also within the CAB, disabled
    //                                 
    // logic to create an
    // acknowledgment
    //                                 
    // going back to the State which
    //                                 
    // requested that we establish this
    //                                 
    // case.  As per request by Curtis
    //                                 
    // Scroggins based on what he
    //                                 
    // learned at the ACF conference
    //                                 
    // in order to satisfy CSEnet
    //                                 
    // requirements.
    // --------------------------------------------
    // 06/21/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only)
    // ------------------------------------------------------------
    // 07/29/99  M. Lachowicz          Check if active programs or
    //                                 
    // future programs exits in ADABASE
    // ------------------------------------------------------------
    // 07/30/99  M. Lachowicz          Disabling the NEXT TRAN ability
    //                                 
    // from REGI when processing a case
    //                                 
    // resulting from PAR1 (PA REFERRAL
    // ).
    // ------------------------------------------------------------
    // 08/10/99  M. Lachowicz          Check if coming Export Group
    //                                 
    // View contains valid person
    // numbers.
    //                                 
    // Valid Person Number can be
    // spaces
    //                                 
    // or 10 digits number or 9 digits
    //                                 
    // number + "O".
    //  
    // ------------------------------------------------------------
    // 08/23/99  M. Lachowicz          Check if AR person number is
    //                                 
    // not equal to AP person number.
    //  
    // ------------------------------------------------------------
    // 09/08/99 W.Campbell             Added logic to
    //                                 
    // test an existing adabas
    //                                 
    // person to see if they are
    //                                 
    // known to CSE and if not
    //                                 
    // to make them know to CSE
    //                                 
    // when adding them to a
    //                                 
    // CSE case.  This was to fix
    //                                 
    // a bug discovered 2 days
    //                                 
    // after going into production.
    //                                 
    // Also had to set a switch to
    //                                 
    // cause the person-program
    //                                 
    // info to be copied for these
    //                                 
    // folks.
    // ---------------------------------------------
    // 10/05/99 M.Lachowicz            Check if there are no
    //                                 
    // duplicate person/role rows.
    //                                 
    // PR #75485.
    // ---------------------------------------------
    // 01/10/2000 W.Campbell           Inserted 2 move
    //                                 
    // statements to populate the
    //                                 
    // zip code being passed to
    //                                 
    // EAB_RETURN_KS_COUNTY_BY_ZIP
    //                                 
    // the routine which is used to
    //                                 
    // determine the KS county
    //                                 
    // code for an address.  This
    //                                 
    // is in the INRD logic.
    //                                 
    // The county code may be
    //                                 
    // used later in the assignment
    //                                 
    // of the case and case units
    //                                 
    // to the service providers in an
    //                                 
    // Office.  Work done on PR# 77898.
    // -------------------------------------------------
    // 01/26/2000 M.Lachowicz          Reactivated Willie's code
    //                                 
    // from 09/21/99 to fix PR74943.
    // -------------------------------------------------
    // 03/10/2000 M.Lachowicz         WR# 000160 - PRWORA .
    // -------------------------------------------------
    // 05/30/2000 M.Lachowicz         Set case number to space
    //                                
    // when ROLLBACK is issued.
    //                                
    // PR 96270.
    // -------------------------------------------------
    // 06/26/2000 M.Lachowicz         REGI needs to create
    //                                
    // the same FV code for the AR.
    //                                
    // PR 98145.
    // -------------------------------------------------
    // 07/11/00  M. Lachowicz         Pass sex AP to
    //                                
    // SI_SET_CLIENT_CASE_ROLE_DETAILS
    //                                
    // PR 98482.
    // ------------------------------------------------------------
    // 09/11/00 W.Campbell            New code added
    //                                
    // to move logic from PAR1
    //                                
    // to REGI to deactivate the
    //                                
    // PA Referral.
    //                                
    // Work done on WR#00205.
    // --------------------------------------------
    // 22/02/01 M.Lachowicz           Do not allow for
    //                                
    // space sex code.
    //                                
    // Work done on PR#113332.
    // --------------------------------------------
    // 09/13/01 M.Lachowicz           Pass SWEIINRD
    //                                
    // to
    // SI_PEPR_CREATE_PERSON_PROGRAM.
    //                                
    // Work done on PR#127465.
    // --------------------------------------------
    // 05/15/2002 M.Lachowicz         Do not create address
    //                                
    // if AR is orgaznization..
    //                                
    // Work done on PR#146531.
    // --------------------------------------------
    // 05/24/2002 M.Lachowicz         Do not allow to create Person
    //                                
    // if sex code is different than '
    // M' or 'F'.
    //                                
    // Work done on PR#147220.
    // --------------------------------------------
    // 07/16/2002 M.Lachowicz         Validate person names to check
    //                                
    // if do not contain invalid
    // characters.
    //                                
    // Work done on PR#150856.
    // --------------------------------------------
    // 09/30/2002 M.Lachowicz         Do not allow to create Person
    //                                
    // if sex code is different than '
    // M' or 'F'
    //                                
    // and control comes from IAPI.
    //                                
    // Work done on PR#158975.
    // --------------------------------------------
    // PR176146. Changed the SSN for added CSE persons to display in a text 
    // format. This allows the program to issue a diagnostic when blanks are
    // left in the SSN during the PF5 (add) process. 4-18-03. L. Bachura
    // ----------------------------------------------------------------------------
    // 10/29/07     LSS     PR# 180608 / CQ406
    // Added verify statement to error out if ssn contains a non-numeric 
    // character.
    // ----------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------
    // G. Pan     4/30/2008   CQ4530
    // 			Added IF statement - when the SEX code and Role have not been entered
    //                         it should not have gotten the SEX code error 
    // message.
    // ----------------------------------------------------------------------------------------
    // **************************************************************************************************
    // *                                      
    // Maintenance Log
    // 
    // *
    // **************************************************************************************************
    // *    DATE       NAME             PR/SR #       DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------     
    // --------------------------------------------------*
    // * 05/27/2011  Raj S              CQ9690        Set FVI_SET_DATE and 
    // FVI_UPDATED_BY when setting  *
    // *
    // 
    // FVI. Re-clicked the code.
    // *
    // **************************************************************************************************
    // **************************************************************************************************
    // *    DATE       NAME             PR/SR #       DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------     
    // --------------------------------------------------*
    // * 12/05/13    LSS                CQ42035       Change view using for 
    // abend_data to match         *
    // *
    // 
    // the view returning the error message
    // *
    // *
    // 
    // (local_eab abend_data to local abend_data)
    // *
    // **************************************************************************************************
    // **************************************************************************************************
    // *    DATE       NAME             PR/SR #       DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------     
    // --------------------------------------------------*
    // * 05/10/17    GVandy             
    // CQ48108       IV-D PEP Changes.
    // 
    // *
    // **************************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.ErrOnAdabasUnavailable.Flag = "Y";

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Hidden.Assign(import.Hidden);
    export.NewCsePersonsWorkSet.Assign(import.NewCsePersonsWorkSet);
    export.NewSsnWorkArea.Assign(import.NewSsnWorkArea);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    export.Case1.Number = import.Case1.Number;
    MoveOffice(import.Office, export.Office);
    export.SelectActionWorkArea.Text60 = import.SelectActionWorkArea.Text60;
    export.SelectActionCommon.Assign(import.SelectActionCommon);
    export.SelectActionCsePersonsWorkSet.Number =
      import.SelectActionCsePersonsWorkSet.Number;
    export.BeenToName.Flag = import.BeenToName.Flag;
    export.RegisterSuccessful.Flag = import.RegisterSuccessful.Flag;
    export.FromIapi.Flag = import.FromIapi.Flag;
    export.FromInterstateCase.Assign(import.FromInterstateCase);
    export.FromInrdCommon.Flag = import.FromInrdCommon.Flag;
    export.FromInrdInformationRequest.Number =
      import.FromInrdInformationRequest.Number;
    export.FromPar1.Flag = import.FromPar1.Flag;
    export.FromPaReferral.Assign(import.FromPaReferral);
    export.NotFromReferral.Flag = import.NotFromReferral.Flag;

    // 03/10/00 M.L Start
    export.ReturnFromCpat.Flag = import.ReturnFromCpat.Flag;

    // 03/10/00 M.L End
    if (IsEmpty(export.SelectActionCommon.Flag))
    {
      export.SelectActionCommon.Flag = "N";
    }

    // 09/13/01 M. Lachowicz Start
    UseCabSetMaximumDiscontinueDate();

    // 09/13/01 M. Lachowicz End
    if (Equal(global.Command, "FROMNAME"))
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.FromName.Index = 0; import.FromName.Index < import
        .FromName.Count; ++import.FromName.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetailCsePersonsWorkSet.Assign(
          import.FromName.Item.NameDetail);
        export.Export1.Next();
      }

      export.BeenToName.Flag = "Y";
    }
    else
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetailCaseRole.Type1 =
          import.Import1.Item.DetailCaseRole.Type1;
        export.Export1.Update.DetailFamily.Type1 =
          import.Import1.Item.DetailFamily.Type1;
        export.Export1.Update.DetailCsePersonsWorkSet.Assign(
          import.Import1.Item.DetailCsePersonsWorkSet);
        export.Export1.Update.DetailCaseCnfrm.Flag =
          import.Import1.Item.DetailCaseCnfrm.Flag;
        export.Export1.Next();
      }
    }

    if (Equal(global.Command, "RETNAME"))
    {
      export.BeenToName.Flag = "Y";

      if (!IsEmpty(import.NameReturn.Number))
      {
        export.NewCsePersonsWorkSet.Assign(import.NameReturn);
      }
      else
      {
        return;
      }

      if (!IsEmpty(export.NewCsePersonsWorkSet.Ssn))
      {
        local.New1.SsnText9 = export.NewCsePersonsWorkSet.Ssn;
        UseCabSsnConvertTextToNum();
      }
      else
      {
        export.NewSsnWorkArea.SsnNumPart1 = 0;
        export.NewSsnWorkArea.SsnNumPart2 = 0;
        export.NewSsnWorkArea.SsnNumPart3 = 0;
      }

      global.Command = "ADD";
    }

    if (export.Office.SystemGeneratedId == 0)
    {
      var field = GetField(export.Office, "systemGeneratedId");

      field.Protected = false;
      field.Focused = true;
    }
    else
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        var field = GetField(export.Export1.Item.DetailCaseRole, "type1");

        field.Protected = false;
        field.Focused = true;

        break;
      }
    }

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenPrev.Number = import.HiddenPrev.Number;
    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    // 03/10/00 M.L start
    if (Equal(global.Command, "FROMCPAT"))
    {
      export.ReturnFromCpat.Flag = "Y";

      return;
    }

    if (AsChar(export.RegisterSuccessful.Flag) == 'Y' && AsChar
      (export.ReturnFromCpat.Flag) != 'Y' && !Equal(global.Command, "CPAT"))
    {
      export.Standard.NextTransaction = "";
      ExitState = "SI0000_MUST_FLOW_TO_CPAT";

      return;
    }

    // ---------------------------------------------
    // 09/11/00 W.Campbell - New code added
    // to move logic from PAR1 to REGI to
    // deactivate the PA Referral.
    // Work done on WR#00205.
    // Disabled code which would force
    // a flow back to PAR1 after case
    // registration.
    // --------------------------------------------
    // ---------------------------------------------
    // 09/11/00 W.Campbell - New code added
    // to move logic from PAR1 to REGI to
    // deactivate the PA Referral.
    // Work done on WR#00205.
    // End of disabled code which would force
    // a flow back to PAR1 after case
    // registration.
    // --------------------------------------------
    // ---------------------------------------------
    //         	N E X T   T R A N
    // --------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      if (!IsEmpty(export.Next.Number))
      {
        export.Hidden.CaseNumber = export.Next.Number;
      }
      else if (!IsEmpty(export.Case1.Number))
      {
        export.Hidden.CaseNumber = export.Case1.Number;
      }

      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "COMN") || Equal(global.Command, "IIDC") || Equal
      (global.Command, "ORGZ") || Equal(global.Command, "NAME") || Equal
      (global.Command, "COMP") || Equal(global.Command, "FROMNAME") || Equal
      (global.Command, "IIMC") || Equal(global.Command, "CPAT") || Equal
      (global.Command, "FROMCPAT"))
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
    // ---------------------------------
    // Disallow further processing after
    // Case is registered.
    // ---------------------------------
    if (!IsEmpty(export.Case1.Number) && (Equal(global.Command, "ADD") || Equal
      (global.Command, "CREATE") || Equal(global.Command, "LIST")))
    {
      ExitState = "SI0000_CHANGE_NOT_ALLWD_AFTR_REG";

      return;
    }

    // ---------------------------------------------
    // After a successful create, force a flow back
    // to IAPI, if that is was the calling procedure
    // ---------------------------------------------
    // 03/10/00 M.L Start
    if (Equal(global.Command, "CPAT"))
    {
    }
    else if (AsChar(export.RegisterSuccessful.Flag) == 'Y' && AsChar
      (export.FromIapi.Flag) == 'Y' && !Equal(global.Command, "RETURN"))
    {
      ExitState = "SI0000_MUST_FLOW_BACK_TO_IAPI";

      return;
    }

    // 03/10/00 M.L End
    // ---------------------------------------------
    // After a successful create, force a flow back
    // to PAR1, if that is was the calling procedure
    // ---------------------------------------------
    if (AsChar(export.SelectActionCommon.Flag) == 'Y')
    {
      switch(AsChar(export.SelectActionCommon.SelectChar))
      {
        case 'C':
          // ---------------------------------
          // CSE chose to continue processing.
          // ---------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DetailCsePersonsWorkSet.Number,
              export.SelectActionCsePersonsWorkSet.Number))
            {
              export.Export1.Update.DetailCaseCnfrm.Flag = "Y";
            }
          }

          export.SelectActionCommon.Flag = "N";
          export.SelectActionCommon.Command = "";
          export.SelectActionWorkArea.Text60 = "";
          export.SelectActionCommon.SelectChar = "";
          export.SelectActionCsePersonsWorkSet.Number = "";
          global.Command = "CREATE";

          break;
        case 'V':
          // ---------------------------------
          // CSE chose to review existing case
          // on which the child in question is
          // inactive.
          // ---------------------------------
          export.SelectActionCommon.SelectChar = "";
          ExitState = "ECO_LNK_TO_COMN";

          return;
        case 'Q':
          // ---------------------------------
          // CSE chose to abort processing.
          // ---------------------------------
          export.SelectActionCommon.Flag = "N";
          export.SelectActionCommon.Command = "";
          export.SelectActionWorkArea.Text60 = "";
          export.SelectActionCommon.SelectChar = "";
          export.SelectActionCsePersonsWorkSet.Number = "";

          return;
        default:
          var field = GetField(export.SelectActionCommon, "selectChar");

          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          if (Equal(export.SelectActionCommon.Command, "ACTIVE"))
          {
            export.SelectActionCommon.SelectChar = "Q";
            ExitState = "ACTIVE_CHILD_ON_ANOTHER_CASE";
          }
          else if (Equal(export.SelectActionCommon.Command, "INACTIVE"))
          {
            export.SelectActionCommon.SelectChar = "C";
            ExitState = "CHILD_IS_INACTIVE_ON_ANOTHR_CASE";
          }

          global.Command = "";

          break;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "CPAT":
        // 03/10/00 M.L Start
        if (AsChar(export.RegisterSuccessful.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_TO_CPAT";

          return;
        }
        else
        {
          ExitState = "SI0000_LNK_TO_CPAT_NOT_ALLOWED";

          return;
        }

        // 03/10/00 M.L End
        break;
      case "IIMC":
        ExitState = "ECO_XFR_TO_SI_IIMC";

        return;
      case "ADD":
        if (AsChar(export.BeenToName.Flag) == 'N')
        {
          ExitState = "NAME_SEARCH_REQUIRED";

          return;
        }

        if (IsEmpty(export.NewCsePersonsWorkSet.Number))
        {
          // ---------------------------------
          // If this is a new person, validate
          // and create.
          // ---------------------------------
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
          UseSiCheckName1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "SI0001_INVALID_NAME";

            var field1 = GetField(export.NewCsePersonsWorkSet, "firstName");

            field1.Error = true;

            var field2 = GetField(export.NewCsePersonsWorkSet, "lastName");

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
            case ' ':
              // 02/21/01 M.L Start
              var field1 = GetField(export.NewCsePersonsWorkSet, "sex");

              field1.Error = true;

              ExitState = "INVALID_SEX";

              return;

              // 02/21/01 M.L End
              break;
            default:
              var field2 = GetField(export.NewCsePersonsWorkSet, "sex");

              field2.Error = true;

              ExitState = "INVALID_SEX";

              return;
          }

          // PR176146. Validate that the SSN contains no blanks.
          local.SsnPart.Count = 0;

          if (IsEmpty(Substring(export.NewSsnWorkArea.SsnTextPart1, 1, 1)))
          {
            local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
          }

          if (IsEmpty(Substring(export.NewSsnWorkArea.SsnTextPart1, 2, 1)))
          {
            local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
          }

          if (IsEmpty(Substring(export.NewSsnWorkArea.SsnTextPart1, 3, 1)))
          {
            local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
          }

          if (IsEmpty(Substring(export.NewSsnWorkArea.SsnTextPart2, 1, 1)))
          {
            local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
          }

          if (IsEmpty(Substring(export.NewSsnWorkArea.SsnTextPart2, 2, 1)))
          {
            local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
          }

          if (IsEmpty(Substring(export.NewSsnWorkArea.SsnTextPart3, 1, 1)))
          {
            local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
          }

          if (IsEmpty(Substring(export.NewSsnWorkArea.SsnTextPart3, 2, 1)))
          {
            local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
          }

          if (IsEmpty(Substring(export.NewSsnWorkArea.SsnTextPart3, 3, 1)))
          {
            local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
          }

          if (IsEmpty(Substring(export.NewSsnWorkArea.SsnTextPart3, 4, 1)))
          {
            local.SsnPart.Count = (int)((long)local.SsnPart.Count + 1);
          }

          if (local.SsnPart.Count > 0)
          {
            if (local.SsnPart.Count > 8)
            {
              goto Test1;
            }

            if (local.SsnPart.Count > 0 && local.SsnPart.Count < 9)
            {
              var field1 = GetField(export.NewSsnWorkArea, "ssnTextPart1");

              field1.Error = true;

              var field2 = GetField(export.NewSsnWorkArea, "ssnTextPart2");

              field2.Error = true;

              var field3 = GetField(export.NewSsnWorkArea, "ssnTextPart3");

              field3.Error = true;

              ExitState = "LE0000_SSN_CONTAINS_NONNUM";

              return;
            }
          }

Test1:

          // PR176146. End validation of the SSN for blanks.
          MoveSsnWorkArea(export.NewSsnWorkArea, local.New1);
          local.New1.ConvertOption = "2";
          UseCabSsnConvertNumToText();

          if (!IsEmpty(export.NewSsnWorkArea.SsnText9))
          {
            export.NewCsePersonsWorkSet.Ssn = export.NewSsnWorkArea.SsnText9;
          }
          else
          {
            export.NewCsePersonsWorkSet.Ssn = "000000000";
          }

          // PR176146. Load ssn for ADABAS call after validation of the SSN for 
          // blanks.
          if (!IsEmpty(export.NewSsnWorkArea.SsnTextPart1) && !
            IsEmpty(export.NewSsnWorkArea.SsnTextPart2) && !
            IsEmpty(export.NewSsnWorkArea.SsnTextPart3))
          {
            local.SsnConcat.Text8 = export.NewSsnWorkArea.SsnTextPart2 + export
              .NewSsnWorkArea.SsnTextPart3;
            export.NewCsePersonsWorkSet.Ssn =
              export.NewSsnWorkArea.SsnTextPart1 + local.SsnConcat.Text8;

            // PR# 180608 / CQ406   10/29/07   LSS   Added verify / ERROR 
            // statements.
            if (Verify(export.NewCsePersonsWorkSet.Ssn, "0123456789") != 0)
            {
              var field1 = GetField(export.NewSsnWorkArea, "ssnTextPart1");

              field1.Error = true;

              var field2 = GetField(export.NewSsnWorkArea, "ssnTextPart2");

              field2.Error = true;

              var field3 = GetField(export.NewSsnWorkArea, "ssnTextPart3");

              field3.Error = true;

              ExitState = "LE0000_SSN_CONTAINS_NONNUM";

              return;
            }
          }

          // PR176146. End code for  load ssn for ADABAS call after validation 
          // of the SSN for blanks.
          UseCabCreateAdabasPerson2();

          if (AsChar(local.AbendData.Type1) == 'A' || AsChar
            (local.AbendData.Type1) == 'C')
          {
            return;
          }

          local.CsePerson.Type1 = "C";
          UseSiCreateCsePerson1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // ---------------------------------------------
          // Create an Alias record in AE with the unique
          // key set to spaces to indicate to AE that the
          // person is known to CSE.
          // ---------------------------------------------
          MoveCsePersonsWorkSet2(export.NewCsePersonsWorkSet, local.Alias);
          local.Alias.UniqueKey = "";
          UseSiAltsCabUpdateAlias();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          // -------------------------------------------------------
          // 	Retreive the person program history from AE
          // -------------------------------------------------------
          // -------------------------------------------------------
          // 02/08/99 W.Campbell - Added the
          // following set statement to set the
          // cse_person number which is being passed to
          // SI_REGI_COPY_ADABAS_PERSON_PGMS
          // to the new person number just created
          // above.
          // -------------------------------------------------------
          local.CsePerson.Number = export.NewCsePersonsWorkSet.Number;
          UseSiRegiCopyAdabasPersonPgms();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("CHILD_PERSON_PROG_NF"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            UseEabRollbackCics();

            return;
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.NewCsePersonsWorkSet.Number,
            export.Export1.Item.DetailCsePersonsWorkSet.Number))
          {
            var field =
              GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");

            field.Error = true;

            export.NewCsePersonsWorkSet.Assign(local.Blank);
            export.NewSsnWorkArea.SsnNumPart1 = 0;
            export.NewSsnWorkArea.SsnNumPart2 = 0;
            export.NewSsnWorkArea.SsnNumPart3 = 0;
            ExitState = "SI0000_PERSON_ALREADY_ON_CASE";

            return;
          }
        }

        if (CharAt(export.NewCsePersonsWorkSet.Number, 10) == 'O')
        {
        }
        else
        {
          UseSiFormatCsePersonName();
        }

        UseSiAddPersonToCase();
        export.NewCsePersonsWorkSet.Assign(local.Blank);
        export.NewSsnWorkArea.SsnNumPart1 = 0;
        export.NewSsnWorkArea.SsnNumPart2 = 0;
        export.NewSsnWorkArea.SsnNumPart3 = 0;
        export.NewSsnWorkArea.SsnTextPart1 = "";
        export.NewSsnWorkArea.SsnTextPart2 = "";
        export.NewSsnWorkArea.SsnTextPart3 = "";
        ExitState = "SI0000_PERSON_ADDED";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "CREATE":
        // ---------------------------------------------
        //     R E G I S T E R     P R O C E S S I N G
        // --------------------------------------------
        // --------------------------------------------
        // Office required.
        // --------------------------------------------
        if (export.Office.SystemGeneratedId == 0)
        {
          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (IsEmpty(export.NotFromReferral.Flag))
        {
          if (IsEmpty(export.FromIapi.Flag) && IsEmpty
            (export.FromInrdCommon.Flag) && IsEmpty(export.FromPar1.Flag))
          {
            ExitState = "SI0000_CASE_NOT_FROM_REFERRAL";
            export.NotFromReferral.Flag = "Y";

            return;
          }
        }

        if (AsChar(export.FromPar1.Flag) == 'Y' || AsChar
          (export.FromIapi.Flag) == 'Y')
        {
        }
        else
        {
          // this was added to force the work flow to start in INRD go to NAME 
          // and them come to
          // REGI. each step has to be completed successfully before a case can 
          // be created except
          // when they created by a pa refferal or interstate case
          if (import.FromInrdInformationRequest.Number > 0)
          {
            if (ReadInformationRequest())
            {
              if (AsChar(entities.InformationRequest.ApplicationProcessedInd) ==
                'Y' || AsChar
                (entities.InformationRequest.NameSearchComplete) == 'Y')
              {
                if (AsChar(entities.InformationRequest.ApplicationProcessedInd) ==
                  'Y' && AsChar
                  (entities.InformationRequest.NameSearchComplete) == 'Y')
                {
                  // this is good, we can proceed
                }
                else
                {
                  if (AsChar(entities.InformationRequest.ApplicationProcessedInd)
                    == 'Y')
                  {
                    ExitState = "NAME_SEARCH_REQUIRED";

                    return;
                  }

                  if (AsChar(entities.InformationRequest.NameSearchComplete) ==
                    'Y')
                  {
                    ExitState = "MUST_COMPLETE_ENROLLMENT_REC_1ST";

                    return;
                  }
                }
              }
              else
              {
                ExitState = "FLOW_FROM_INRD_TO_NAME_TO_REGI";

                return;
              }
            }
            else
            {
              ExitState = "INFORMATION_REQUEST_NF";

              return;
            }
          }
          else
          {
            ExitState = "NO_INFORMATION_REQUEST_INFO_PASS";

            return;
          }
        }

        local.ArCommon.Count = 0;
        local.Mother.Count = 0;
        local.FatherCommon.Count = 0;
        local.ChCommon.Count = 0;
        local.FemaleAp.Count = 0;

        // ---------------------------------------------
        // Check for Duplicate Cases.....
        // ---------------------------------------------
        // ----------------------------------------------------
        // 04/28/99 W.Campbell -  Rewrite of CAB
        // si_regi_check_for_duplicate_case
        // to restructure the logic so that
        // it will test all the input APs with
        // the AR and CHildren for a
        // duplicate case.
        // -----------------------------------------------------
        UseSiRegiChkForDuplicateCase();

        // ----------------------------------------------------
        // 04/28/99 W.Campbell - Disabled
        // following USE statement as a result
        // of replaceing it with above USE
        // statement.
        // -----------------------------------------------------
        if (AsChar(local.DuplicateCase.Flag) == 'Y')
        {
          if (AsChar(export.FromIapi.Flag) == 'Y')
          {
            ExitState = "DUPLICATE_CASE_EXIST_INTERSTATE";
          }
          else
          {
            ExitState = "DUPLICATE_CASE_EXISTS";
          }

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          // 08/10/99 M.L Start
          if (IsEmpty(export.Export1.Item.DetailCsePersonsWorkSet.Number))
          {
            // 07/16/2002 M.L Start
            if (IsEmpty(export.Export1.Item.DetailCaseRole.Type1) && IsEmpty
              (export.Export1.Item.DetailFamily.Type1))
            {
            }
            else
            {
              UseSiCheckName2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                  "formattedName");

                field.Error = true;

                return;
              }
            }

            // 07/16/2002 M.L End
            // 05/24/2002 M.L Start
            // 09/30/2002 M. Lachowicz
            // Check if control comes from IAPI.
            if (AsChar(export.FromPar1.Flag) == 'Y' || AsChar
              (export.FromIapi.Flag) == 'Y')
            {
              if (AsChar(export.Export1.Item.DetailCsePersonsWorkSet.Sex) == 'M'
                || AsChar(export.Export1.Item.DetailCsePersonsWorkSet.Sex) == 'F'
                )
              {
              }
              else
              {
                // ----------------------------------------------------------------------------------------
                // G. Pan     4/30/2008   CQ4530  Added IF statement
                // ----------------------------------------------------------------------------------------
                if (!IsEmpty(export.Export1.Item.DetailCaseRole.Type1))
                {
                  var field =
                    GetField(export.Export1.Item.DetailCsePersonsWorkSet, "sex");
                    

                  field.Color = "red";
                  field.Intensity = Intensity.High;
                  field.Highlighting = Highlighting.ReverseVideo;
                  field.Protected = false;
                  field.Focused = true;

                  ExitState = "SI0000_SEX_CODE_PAR1";

                  return;
                }
              }
            }

            // 05/24/2002 M.L End
          }
          else if (Verify(export.Export1.Item.DetailCsePersonsWorkSet.Number,
            "0123456789") == 0)
          {
          }
          else if (CharAt(export.Export1.Item.DetailCsePersonsWorkSet.Number, 10)
            == 'O')
          {
            if (Verify(Substring(
              export.Export1.Item.DetailCsePersonsWorkSet.Number,
              CsePersonsWorkSet.Number_MaxLength, 1, 9), "0123456789") == 0)
            {
            }
            else
            {
              var field =
                GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                

              field.Error = true;

              ExitState = "SI0000_INVALID_PERSON_NUMBER";

              return;
            }
          }
          else
          {
            var field =
              GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "SI0000_INVALID_PERSON_NUMBER";

            return;
          }

          // 08/10/99 M.L End
          // ---------------------------------------------
          // Role must be AP, AR or Child
          // ---------------------------------------------
          switch(TrimEnd(export.Export1.Item.DetailCaseRole.Type1))
          {
            case "AR":
              ++local.ArCommon.Count;
              local.ArCsePersonsWorkSet.Number =
                export.Export1.Item.DetailCsePersonsWorkSet.Number;

              break;
            case "CH":
              ++local.ChCommon.Count;

              break;
            case "AP":
              if (AsChar(export.Export1.Item.DetailCsePersonsWorkSet.Sex) == 'F'
                )
              {
                ++local.FemaleAp.Count;
                ++local.ApCommon.Count;
              }
              else
              {
                // 03/10/00 M.L Start
                if (AsChar(export.Export1.Item.DetailCsePersonsWorkSet.Sex) == 'M'
                  )
                {
                  ++local.MaleApCommon.Count;
                  local.MaleApCsePerson.Number =
                    export.Export1.Item.DetailCsePersonsWorkSet.Number;
                }

                // 03/10/00 M.L End
                ++local.ApCommon.Count;
              }

              break;
            case "":
              break;
            default:
              var field = GetField(export.Export1.Item.DetailCaseRole, "type1");

              field.Error = true;

              ExitState = "INVALID_CASE_ROLE_TYPE";

              return;
          }

          // ---------------------------------------------
          // Family Role must be Mother or Father
          // ---------------------------------------------
          switch(TrimEnd(export.Export1.Item.DetailFamily.Type1))
          {
            case "MO":
              if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AR"))
              {
                local.SetRelToAr.Flag = "Y";
              }

              ++local.Mother.Count;

              break;
            case "FA":
              if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AR"))
              {
                local.SetRelToAr.Flag = "Y";
              }

              ++local.FatherCommon.Count;
              local.FatherCsePersonsWorkSet.Number =
                export.Export1.Item.DetailCsePersonsWorkSet.Number;

              break;
            case "":
              break;
            default:
              var field = GetField(export.Export1.Item.DetailFamily, "type1");

              field.Error = true;

              ExitState = "INVALID_FAMILY_ROLE";

              return;
          }

          // ---------------------------------------------
          // Check that this person is not active as a child
          // on any other case.  If so, give the CSE the option
          // to view the existing case.
          // ---------------------------------------------
          if (!IsEmpty(export.Export1.Item.DetailCsePersonsWorkSet.Number))
          {
            // ----------------------------------------------------
            // 05/17/99 W.Campbell - Disabled
            // following statement to execute
            // logic only for child case role.
            // -----------------------------------------------------
            if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH"))
            {
              local.CsePerson.Number =
                export.Export1.Item.DetailCsePersonsWorkSet.Number;
              UseSiVerifyChildIsOnACase();

              if (local.ActiveChildOnCase.Count >= 2)
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                  

                field.Color = "red";
                field.Highlighting = Highlighting.Underscore;
                field.Protected = true;

                ExitState = "SI0000_CHILD_ACTIVE_ON_TWO_CASES";

                return;
              }

              if (AsChar(local.RelToArIsCh.Flag) == 'Y')
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                  

                field.Color = "red";
                field.Highlighting = Highlighting.Underscore;
                field.Protected = true;

                ExitState = "ACTIVE_CHILD_ON_MO_FA_CASE";

                return;
              }

              if (AsChar(local.ActiveChildOnCase.Flag) == 'Y' && AsChar
                (export.Export1.Item.DetailCaseCnfrm.Flag) != 'Y')
              {
                export.SelectActionCommon.Flag = "Y";
                export.SelectActionCommon.Command = "ACTIVE";
                export.SelectActionWorkArea.Text60 =
                  "(C)ontinue registration, (V)iew existing case, or (Q)uit?";
                export.SelectActionCommon.SelectChar = "V";

                var field1 =
                  GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                  

                field1.Color = "red";
                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = true;

                var field2 = GetField(export.SelectActionCommon, "selectChar");

                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = false;
                field2.Focused = true;

                export.SelectActionCsePersonsWorkSet.Number =
                  export.Export1.Item.DetailCsePersonsWorkSet.Number;
                ExitState = "ACTIVE_CHILD_ON_ANOTHER_CASE";

                return;
              }
            }

            // ---------------------------------------------
            // If this person is a child, see if the child is
            // an inactive child on an existing case.  If so,
            // give the CSE the option to either continue
            // registration, review the existing case, or
            // abort processing.
            // The case confirm flag indicates that the user
            // has chosen to continue processing for that
            // person.
            // ---------------------------------------------
            if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH") && AsChar
              (local.InactiveChildOnCase.Flag) == 'Y' && AsChar
              (export.Export1.Item.DetailCaseCnfrm.Flag) != 'Y')
            {
              export.SelectActionCommon.Flag = "Y";
              export.SelectActionCommon.Command = "INACTIVE";
              export.SelectActionWorkArea.Text60 =
                "(C)ontinue registration, (V)iew existing case, or (Q)uit?";
              export.SelectActionCommon.SelectChar = "V";

              var field1 =
                GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                

              field1.Color = "red";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = true;

              var field2 = GetField(export.SelectActionCommon, "selectChar");

              field2.Highlighting = Highlighting.Underscore;
              field2.Protected = false;
              field2.Focused = true;

              export.SelectActionCsePersonsWorkSet.Number =
                export.Export1.Item.DetailCsePersonsWorkSet.Number;
              ExitState = "CHILD_IS_INACTIVE_ON_ANOTHR_CASE";

              return;
            }
          }

          // ------------------------------------------------------------
          // An Organization can only be an AR
          // ------------------------------------------------------------
          if (CharAt(export.Export1.Item.DetailCsePersonsWorkSet.Number, 10) ==
            'O')
          {
            if (!Equal(export.Export1.Item.DetailCaseRole.Type1, "AR"))
            {
              var field1 =
                GetField(export.Export1.Item.DetailCaseRole, "type1");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                

              field2.Error = true;

              ExitState = "ORGANIZATION_MUST_BE_AR";

              return;
            }
          }

          // --------------------------------------------
          // AP cannot be Child
          // --------------------------------------------
          if (Equal(import.Import1.Item.DetailCaseRole.Type1, "AP") && Equal
            (import.Import1.Item.DetailFamily.Type1, "CH"))
          {
            var field1 = GetField(export.Export1.Item.DetailFamily, "type1");

            field1.Error = true;

            var field2 = GetField(export.Export1.Item.DetailCaseRole, "type1");

            field2.Error = true;

            ExitState = "INVALID_CASE_ROLE_COMBINATION";

            return;
          }

          // ---------------------------------------------
          // Child cannot be Mother or Father
          // ---------------------------------------------
          if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH") && (
            Equal(export.Export1.Item.DetailFamily.Type1, "MO") || Equal
            (export.Export1.Item.DetailFamily.Type1, "FA")))
          {
            var field1 = GetField(export.Export1.Item.DetailFamily, "type1");

            field1.Error = true;

            var field2 = GetField(export.Export1.Item.DetailCaseRole, "type1");

            field2.Error = true;

            ExitState = "INVALID_CASE_ROLE_COMBINATION";

            return;
          }

          // ---------------------------------------------
          // Child cannot be AP
          // ---------------------------------------------
          if (Equal(export.Export1.Item.DetailFamily.Type1, "CH") && Equal
            (export.Export1.Item.DetailCaseRole.Type1, "AP"))
          {
            var field1 = GetField(export.Export1.Item.DetailCaseRole, "type1");

            field1.Error = true;

            var field2 = GetField(export.Export1.Item.DetailFamily, "type1");

            field2.Error = true;

            ExitState = "INVALID_CASE_ROLE_COMBINATION";

            return;
          }

          // ---------------------------------------------
          // Mother must be Female
          // ---------------------------------------------
          if (Equal(export.Export1.Item.DetailFamily.Type1, "MO") && AsChar
            (export.Export1.Item.DetailCsePersonsWorkSet.Sex) != 'F')
          {
            var field = GetField(export.Export1.Item.DetailFamily, "type1");

            field.Error = true;

            ExitState = "MOTHER_MUST_BE_FEMALE";

            return;
          }

          // ---------------------------------------------
          // Father must be Male
          // ---------------------------------------------
          if (Equal(export.Export1.Item.DetailFamily.Type1, "FA") && AsChar
            (export.Export1.Item.DetailCsePersonsWorkSet.Sex) != 'M')
          {
            var field = GetField(export.Export1.Item.DetailFamily, "type1");

            field.Error = true;

            ExitState = "FATHER_MUST_BE_MALE";

            return;
          }
        }

        // ------------------------------------------------------------
        // 04/23/99 W.Campbell - Test to make sure
        // that the CH is not the same person as the
        // AR will only be done if the AR has a
        // CSE person number.  This change was made
        // to allow for ALL participants in a CASE to
        // come into REGI without any CSE person
        // number. This was needed for Interstate cases.
        // ------------------------------------------------------------
        if (!IsEmpty(local.ArCsePersonsWorkSet.Number))
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            // ---------------------------------------------
            // 	Child cannot be AR.
            // ---------------------------------------------
            if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH"))
            {
              if (Equal(export.Export1.Item.DetailCsePersonsWorkSet.Number,
                local.ArCsePersonsWorkSet.Number))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                  

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.DetailCaseRole, "type1");

                field2.Error = true;

                ExitState = "SI0000_CHILD_CANNOT_BE_AR";

                return;
              }
            }
          }
        }

        // ---------------------------------------------
        // Must be one and only one AR on the case.
        // Can be only one Mother and Father on the
        // case.  Must be at least one child. Only one
        // female AP allowed.
        // ---------------------------------------------
        if (local.ArCommon.Count == 0)
        {
          ExitState = "INVALID_NUMBER_OF_ARS_LEAST_ONE";

          return;
        }
        else if (local.ArCommon.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AR"))
            {
              var field = GetField(export.Export1.Item.DetailCaseRole, "type1");

              field.Error = true;
            }
          }

          ExitState = "INVALID_NUMBER_OF_ARS_ONLY_ONE";

          return;
        }

        if (local.ChCommon.Count < 1)
        {
          ExitState = "INVALID_NUMBER_OF_CHILDREN";

          return;
        }

        if (local.Mother.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DetailFamily.Type1, "MO"))
            {
              var field = GetField(export.Export1.Item.DetailCaseRole, "type1");

              field.Error = true;
            }
          }

          ExitState = "INVALID_NUMBER_OF_MOTHERS";

          return;
        }

        if (local.FatherCommon.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DetailFamily.Type1, "FA"))
            {
              var field = GetField(export.Export1.Item.DetailCaseRole, "type1");

              field.Error = true;
            }
          }

          ExitState = "INVALID_NUMBER_OF_FATHERS";

          return;
        }

        if (local.FemaleAp.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AP"))
            {
              var field = GetField(export.Export1.Item.DetailCaseRole, "type1");

              field.Error = true;
            }
          }

          ExitState = "INVALID_NUMBER_OF_FEMALE_APS";

          return;
        }

        // ------------------------------------------------------------
        // If AP is female, no other APs allowed
        // ------------------------------------------------------------
        if (local.FemaleAp.Count > 0)
        {
          if (local.ApCommon.Count > 1)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AP"))
              {
                var field =
                  GetField(export.Export1.Item.DetailCaseRole, "type1");

                field.Error = true;
              }
            }

            ExitState = "INVALID_AP_MALE_AND_FEMALE";

            return;
          }
        }

        // ------------------------------------------------------------
        // If father is determined, no other male APs allowed
        // ------------------------------------------------------------
        if (local.FatherCommon.Count > 0)
        {
          if (local.ApCommon.Count > 0)
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AP") && AsChar
                (export.Export1.Item.DetailCsePersonsWorkSet.Sex) == 'M')
              {
                if (!Equal(export.Export1.Item.DetailCsePersonsWorkSet.Number,
                  local.FatherCsePersonsWorkSet.Number))
                {
                  var field =
                    GetField(export.Export1.Item.DetailCaseRole, "type1");

                  field.Error = true;

                  ExitState = "INVALID_FA_AND_MULTIPLE_MALE_AP";
                }
              }
            }
          }
        }

        // ------------------------------------------------------------
        // 03/24/99 W.Campbell - New logic added
        // to support Interstate Cases.
        // If this is an Interstate Case, then the
        // case must have an AP.
        // ------------------------------------------------------------
        if (AsChar(export.FromIapi.Flag) == 'Y')
        {
          if (local.ApCommon.Count < 1)
          {
            ExitState = "INTERSTATE_CASE_REQUIRES_AN_AP";

            return;
          }
        }

        // ------------------------------------------------------------
        // 03/24/99 W.Campbell - End of new logic
        // added to support Interstate Cases.
        // ------------------------------------------------------------
        // ------------------------------------------------------------
        // 04/05/99 W.Campbell - New logic
        // added to insure that a CHild on this case
        // does not exist as an active CHild
        // on another case with a different AR.
        // ------------------------------------------------------------
        local.ArCsePerson.Number = local.ArCsePersonsWorkSet.Number;

        // 03/10/00 M.L Start
        local.AllChildrenPaternityOff.Flag = "Y";
        local.AllChildrenPaternityOn.Flag = "Y";
        local.ActiveOtherMaleAp.Flag = "N";

        // 03/10/00 M.L End
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH"))
          {
            // 08/10/99 M.L Start
            // Make process only for person numbers different than spaces
            if (IsEmpty(export.Export1.Item.DetailCsePersonsWorkSet.Number))
            {
              continue;
            }

            // 08/10/99 M.L End
            local.ChCsePerson.Number =
              export.Export1.Item.DetailCsePersonsWorkSet.Number;
            UseSiChkCasesForChWithDiffAr();

            // ------------------------------------------------------------
            // Check for an error return from the CAB.
            // ------------------------------------------------------------
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // ------------------------------------------------------------
            // If the local case number is not spaces, then
            // it is a case number which has the same CH
            // with a different AR.  Therefore, we will not
            // allow this case to be created(registered).
            // ------------------------------------------------------------
            if (!IsEmpty(local.Case2.Number))
            {
              var field = GetField(export.Export1.Item.DetailCaseRole, "type1");

              field.Error = true;

              ExitState = "CASE_EXIST_FOR_CH_AND_DIFF_AR";

              return;
            }

            // 03/10/00 M.L Start
            // --05/10/2017 GVandy CQ48108 (IV-D PEP Changes) Do not set 
            // paternity indicator using PAR1 info.
            if (AsChar(local.ChildPaternityEstInd.PaternityEstablishedIndicator) ==
              'Y')
            {
              local.AllChildrenPaternityOff.Flag = "N";
            }
            else
            {
              local.AllChildrenPaternityOn.Flag = "N";
            }

            if (local.MaleApCommon.Count > 0)
            {
              UseSiCheckApChCombinations();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";
                UseEabRollbackCics();

                return;
              }

              if (AsChar(local.ActiveOtherMaleAp.Flag) == 'Y')
              {
                ExitState = "SI0000_CASE_CH_DIFF_MALE_AP";

                return;
              }
            }

            // 03/10/00 M.L End
          }
        }

        // 08/23/99 M.L  Check if AR person number is not equal to AP person 
        // number.
        if (!IsEmpty(local.ArCsePersonsWorkSet.Number))
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DetailCsePersonsWorkSet.Number,
              local.ArCsePersonsWorkSet.Number) && Equal
              (export.Export1.Item.DetailCaseRole.Type1, "AP"))
            {
              var field =
                GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                

              field.Error = true;

              ExitState = "SI0000_AP_THE_SAME_AS_AR";

              break;
            }
          }

          if (IsExitState("SI0000_AP_THE_SAME_AS_AR"))
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (Equal(export.Export1.Item.DetailCsePersonsWorkSet.Number,
                local.ArCsePersonsWorkSet.Number) && Equal
                (export.Export1.Item.DetailCaseRole.Type1, "AR"))
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                  

                field.Error = true;

                break;
              }
            }
          }
        }

        // 08/23/99 M.L  End.
        // 10/05/99 M.L Start
        if (AsChar(export.FromPar1.Flag) == 'Y')
        {
          for(import.Import1.Index = 0; import.Import1.Index < import
            .Import1.Count; ++import.Import1.Index)
          {
            local.DupFirstTime.Flag = "";

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (Equal(export.Export1.Item.DetailCsePersonsWorkSet.Number,
                import.Import1.Item.DetailCsePersonsWorkSet.Number) && Equal
                (export.Export1.Item.DetailCaseRole.Type1,
                import.Import1.Item.DetailCaseRole.Type1) && Equal
                (export.Export1.Item.DetailFamily.Type1,
                import.Import1.Item.DetailFamily.Type1))
              {
                if (IsEmpty(local.DupFirstTime.Flag))
                {
                  local.DupFirstTime.Flag = "N";
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                    "number");

                  field.Error = true;

                  ExitState = "SI0000_PERSON_ACTIVE_TWICE";

                  goto AfterCycle;
                }
              }
            }
          }

AfterCycle:
          ;
        }

        // 10/05/99 M.L End
        // 03/10/00 M.L Start
        if (AsChar(local.AllChildrenPaternityOff.Flag) == 'N')
        {
          if (local.MaleApCommon.Count > 1)
          {
            ExitState = "SI0000_MULTIPLE_APS_NOT_ALLOWED";
          }
        }

        if (AsChar(local.AllChildrenPaternityOn.Flag) == 'N')
        {
          if (local.FatherCommon.Count > 0)
          {
            ExitState = "SI0000_FATHER_ROLE_NOT_ALLOWED";
          }
        }

        // 03/10/00 M.L End
        // ------------------------------------------------------------
        // 04/05/99 W.Campbell - End of new logic
        // added to insure that a CHild on this case
        // does not exist as an active CHild
        // on another case with a different AR.
        // ------------------------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -------------------------------------
        // Validation Complete.
        // Create the Case.
        // -------------------------------------
        UseSiRegiCreateCase();

        if (IsExitState("OFFICE_NF"))
        {
          export.Case1.Number = "";

          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          export.NotFromReferral.Flag = "";
          UseEabRollbackCics();

          return;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Case1.Number = local.Case2.Number;
        }
        else
        {
          export.Case1.Number = "";
          export.NotFromReferral.Flag = "";
          UseEabRollbackCics();

          return;
        }

        // ---------------------------------------------
        // Assign all of the people to the case created.
        // If there is no number assigned to the person,
        // create the person on ADABAS.
        // Retrieve the AE Person Programs
        // --------------------------------------------
        // 07/11/00 M.L Start
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AP"))
          {
            local.ApSex.Sex = export.Export1.Item.DetailCsePersonsWorkSet.Sex;

            break;
          }
        }

        // 07/11/00 M.L End
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetailCaseRole.Type1) || !
            IsEmpty(export.Export1.Item.DetailFamily.Type1))
          {
            if (IsEmpty(export.Export1.Item.DetailCsePersonsWorkSet.Ssn))
            {
              export.Export1.Update.DetailCsePersonsWorkSet.Ssn = "000000000";
            }

            if (CharAt(export.Export1.Item.DetailCsePersonsWorkSet.Number, 10) !=
              'O')
            {
              // ---------------------------------------------
              // See if the CSE Person does not exist on DB2.
              // ---------------------------------------------
              if (!IsEmpty(export.Export1.Item.DetailCsePersonsWorkSet.Number))
              {
                // M.L 06/21/99 Start   Change property of READ to generate
                //              SELECT ONLY
                if (ReadCsePerson2())
                {
                  // M.L 06/21/99 End
                  // 06/26/2000 M.L Start
                  if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH"))
                  {
                    switch(AsChar(local.FvIndicator.FamilyViolenceIndicator))
                    {
                      case 'C':
                        break;
                      case 'P':
                        if (AsChar(entities.CsePerson.FamilyViolenceIndicator) ==
                          'C')
                        {
                          local.FvIndicator.FamilyViolenceIndicator =
                            entities.CsePerson.FamilyViolenceIndicator;
                        }

                        break;
                      case 'D':
                        if (AsChar(entities.CsePerson.FamilyViolenceIndicator) ==
                          'C' || AsChar
                          (entities.CsePerson.FamilyViolenceIndicator) == 'P')
                        {
                          local.FvIndicator.FamilyViolenceIndicator =
                            entities.CsePerson.FamilyViolenceIndicator;
                        }

                        break;
                      case ' ':
                        local.FvIndicator.FamilyViolenceIndicator =
                          entities.CsePerson.FamilyViolenceIndicator;

                        break;
                      default:
                        break;
                    }
                  }

                  if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AR"))
                  {
                    local.ArFvIndicator.FamilyViolenceIndicator =
                      entities.CsePerson.FamilyViolenceIndicator;
                    local.ArFvIndicator.Number = entities.CsePerson.Number;
                  }

                  // 06/26/2000 M.L End
                  // ---------------------------------------------
                  // No need to create CSE Person on the system.
                  // ---------------------------------------------
                  // ---------------------------------------------
                  // 01/30/99 W.Campbell and again on
                  // 09/08/99 W.Campbell -  However, we must check
                  // to see if this person is a non-case related CSE
                  // person (CSE flag = 'N' or SPACES), and if they
                  // are we must change them in ADABAS to a case
                  // related CSE person (CSE flag = 'Y').
                  // ---------------------------------------------
                  local.ErrOnAdabasUnavailable.Flag = "Y";
                  UseCabReadAdabasPerson();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.Case1.Number = "";
                    export.NotFromReferral.Flag = "";
                    UseEabRollbackCics();

                    return;
                  }

                  // ---------------------------------------------
                  // 01/26/2000 M.Lachowicz - Start
                  // ---------------------------------------------
                  // ---------------------------------------------
                  // 09/21/99 W.Campbell -
                  // ---------------------------------------------
                  local.CopyPersonProgram.Flag = "Y";

                  // ---------------------------------------------
                  // 01/26/2000 M.Lachowicz - End
                  // ---------------------------------------------
                  if (AsChar(local.Cse.Flag) == 'N' || IsEmpty(local.Cse.Flag))
                  {
                    // ---------------------------------------------
                    // 09/08/99 W.Campbell - Added
                    // OR test to the above IF stmt to
                    // test for spaces also.
                    // ---------------------------------------------
                    // ---------------------------------------------
                    // 09/08/99 W.Campbell - Added
                    // the following IF stmt to
                    // test for spaces and if so then
                    // set the flag to Y go get person
                    // programs copied into KESSEP
                    // for this person.
                    // ---------------------------------------------
                    if (IsEmpty(local.Cse.Flag))
                    {
                      local.CopyPersonProgram.Flag = "Y";
                    }

                    // ---------------------------------------------
                    // 02/01/99 W.Campbell - Update the ADABAS
                    // record with the unique key set to spaces to
                    // indicate that the person is known to CSE
                    // now as a CASE related person rather than
                    // a NON-CASE related type CSE person.  This
                    // should change the flag from a 'N' or a space
                    // to a 'Y'.
                    // ---------------------------------------------
                    MoveCsePersonsWorkSet1(export.Export1.Item.
                      DetailCsePersonsWorkSet, local.Alias);
                    local.Alias.UniqueKey = "";
                    UseSiAltsCabUpdateAlias();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      export.Case1.Number = "";
                      export.NotFromReferral.Flag = "";
                      UseEabRollbackCics();

                      return;
                    }
                  }

                  // ---------------------------------------------
                  // 01/30/99 W.Campbell - End of code inserted to
                  // check to see if this person is a non-case
                  // related CSE person (CSE flag = 'N'), and if they
                  // are we must change them in ADABAS to a case
                  // related CSE person (CSE flag = 'Y').
                  // ---------------------------------------------
                }
                else
                {
                  if (AsChar(local.Test.Flag) == 'Y')
                  {
                    // ---------------------------------------------
                    // Create CSE Person on the system.
                    // ---------------------------------------------
                    UseCabCreateAdabasPerson1();

                    if (AsChar(local.AbendData.Type1) == 'A' || AsChar
                      (local.AbendData.Type1) == 'C')
                    {
                      export.Case1.Number = "";
                      UseEabRollbackCics();
                      export.NotFromReferral.Flag = "";
                      ExitState = "ADABAS_ADD_UNSUCCESSFUL_W_RB";

                      return;
                    }
                  }

                  local.CsePerson.Type1 = "C";
                  local.CsePerson.Number =
                    export.Export1.Item.DetailCsePersonsWorkSet.Number;
                  UseSiCreateCsePerson2();

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    local.CopyPersonProgram.Flag = "Y";

                    // ---------------------------------------------
                    // Create an Alias record in AE with the unique
                    // key set to spaces to indicate to AE that the
                    // person is known to CSE.
                    // ---------------------------------------------
                    MoveCsePersonsWorkSet1(export.Export1.Item.
                      DetailCsePersonsWorkSet, local.Alias);
                    local.Alias.UniqueKey = "";
                    UseSiAltsCabUpdateAlias();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      UseEabRollbackCics();
                      export.NotFromReferral.Flag = "";
                      export.Case1.Number = "";

                      return;
                    }
                  }
                  else if (IsExitState("CSE_PERSON_AE"))
                  {
                    export.Case1.Number = "";
                    ExitState = "CSE_PERSON_AE_RB";

                    var field1 =
                      GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                      "number");

                    field1.Color = "red";
                    field1.Highlighting = Highlighting.ReverseVideo;
                    field1.Protected = true;

                    var field2 =
                      GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                      "formattedName");

                    field2.Color = "red";
                    field2.Highlighting = Highlighting.ReverseVideo;
                    field2.Protected = true;
                  }
                  else if (IsExitState("CSE_PERSON_PV"))
                  {
                    export.Case1.Number = "";
                    ExitState = "CSE_PERSON_PV_RB";

                    var field1 =
                      GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                      "formattedName");

                    field1.Color = "red";
                    field1.Highlighting = Highlighting.ReverseVideo;
                    field1.Protected = true;

                    var field2 =
                      GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                      "number");

                    field2.Color = "red";
                    field2.Highlighting = Highlighting.ReverseVideo;
                    field2.Protected = true;
                  }
                  else
                  {
                  }

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    UseEabRollbackCics();
                    export.NotFromReferral.Flag = "";
                    export.Case1.Number = "";

                    return;
                  }
                }
              }
              else
              {
                // ---------------------------------------------
                // Create CSE Person on the system.
                // ---------------------------------------------
                UseCabCreateAdabasPerson1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Case1.Number = "";
                  export.NotFromReferral.Flag = "";
                  UseEabRollbackCics();

                  return;
                }

                local.CsePerson.Type1 = "C";
                local.CsePerson.Number = local.CsePersonsWorkSet.Number;
                export.Export1.Update.DetailCsePersonsWorkSet.Number =
                  local.CsePersonsWorkSet.Number;
                UseSiCreateCsePerson2();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.CopyPersonProgram.Flag = "Y";

                  // ---------------------------------------------
                  // Create an Alias record in AE with the unique
                  // key set to spaces to indicate to AE that the
                  // person is known to CSE.
                  // ---------------------------------------------
                  MoveCsePersonsWorkSet1(export.Export1.Item.
                    DetailCsePersonsWorkSet, local.Alias);
                  local.Alias.UniqueKey = "";
                  UseSiAltsCabUpdateAlias();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    UseEabRollbackCics();
                    export.NotFromReferral.Flag = "";
                    export.Case1.Number = "";

                    return;
                  }
                }
                else if (IsExitState("CSE_PERSON_AE"))
                {
                  ExitState = "CSE_PERSON_AE_RB";

                  var field1 =
                    GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                    "formattedName");

                  field1.Color = "red";
                  field1.Highlighting = Highlighting.ReverseVideo;
                  field1.Protected = true;

                  var field2 =
                    GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                    "number");

                  field2.Color = "red";
                  field2.Highlighting = Highlighting.ReverseVideo;
                  field2.Protected = true;
                }
                else if (IsExitState("CSE_PERSON_PV"))
                {
                  ExitState = "CSE_PERSON_PV_RB";

                  var field1 =
                    GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                    "formattedName");

                  field1.Color = "red";
                  field1.Highlighting = Highlighting.ReverseVideo;
                  field1.Protected = true;

                  var field2 =
                    GetField(export.Export1.Item.DetailCsePersonsWorkSet,
                    "number");

                  field2.Color = "red";
                  field2.Highlighting = Highlighting.ReverseVideo;
                  field2.Protected = true;
                }
                else
                {
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Case1.Number = "";
                  export.NotFromReferral.Flag = "";
                  UseEabRollbackCics();

                  return;
                }
              }
            }

            local.CsePerson.Number =
              export.Export1.Item.DetailCsePersonsWorkSet.Number;
            local.CsePersonsWorkSet.Number =
              export.Export1.Item.DetailCsePersonsWorkSet.Number;

            // -------------------------------------------------------
            // 	Retreive the person program history from AE
            // -------------------------------------------------------
            if (AsChar(local.CopyPersonProgram.Flag) == 'Y')
            {
              UseSiRegiCopyAdabasPersonPgms();

              if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
                ("CHILD_PERSON_PROG_NF"))
              {
                local.CopyPersonProgram.Flag = "";
                ExitState = "ACO_NN0000_ALL_OK";
              }
              else
              {
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";
                UseEabRollbackCics();

                return;
              }
            }

            if (!IsEmpty(export.Export1.Item.DetailCaseRole.Type1))
            {
              if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH"))
              {
                if (AsChar(local.SetRelToAr.Flag) == 'Y')
                {
                  local.CaseRole.RelToAr = "CH";
                }
              }

              local.CaseRole.Type1 = export.Export1.Item.DetailCaseRole.Type1;
              local.CaseRole.StartDate = local.Current.Date;
              UseSiCreateCaseRole();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";
                ExitState = "REGISTRATION_UNSUCCESSFUL_RB";

                return;
              }

              if (AsChar(export.FromIapi.Flag) == 'Y' || AsChar
                (export.FromInrdCommon.Flag) == 'Y' || AsChar
                (export.FromPar1.Flag) == 'Y')
              {
                if (CharAt(local.CsePersonsWorkSet.Number, 10) != 'O')
                {
                  UseSiSetClientCaseRoleDetails();
                }
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";
                ExitState = "REGISTRATION_UNSUCCESSFUL_RB";

                return;
              }

              local.CaseRole.RelToAr = "";
            }

            if (!IsEmpty(export.Export1.Item.DetailFamily.Type1))
            {
              local.CaseRole.Type1 = export.Export1.Item.DetailFamily.Type1;
              local.CaseRole.StartDate = local.Current.Date;
              UseSiCreateCaseRole();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";
                ExitState = "REGISTRATION_UNSUCCESSFUL_RB";

                return;
              }
            }

            if (Equal(export.Export1.Item.DetailCsePersonsWorkSet.Ssn,
              "000000000"))
            {
              export.Export1.Update.DetailCsePersonsWorkSet.Ssn = "";
            }

            // ---------------------------------------------
            // 08/09/99 M.Lachowicz - Replace the above code.
            // --------------------------------------------
            switch(TrimEnd(export.Export1.Item.DetailCaseRole.Type1))
            {
              case "AR":
                local.ArCsePerson.Number =
                  export.Export1.Item.DetailCsePersonsWorkSet.Number;
                local.ArCsePersonsWorkSet.Number =
                  export.Export1.Item.DetailCsePersonsWorkSet.Number;

                // 06/26/2000 M.L Start
                local.ArFvIndicator.Number = local.ArCsePerson.Number;

                // 06/26/2000 M.L End
                break;
              case "AP":
                local.ApCsePerson.Number =
                  export.Export1.Item.DetailCsePersonsWorkSet.Number;

                break;
              default:
                break;
            }
          }
        }

        // ---------------------------------------------
        // Copy AR's AE address for ADC cases.
        // --------------------------------------------
        if (AsChar(export.FromPar1.Flag) == 'Y')
        {
          local.CsePerson.Number = local.ArCsePersonsWorkSet.Number;

          // * Copy the AE "M"ailing address.
          local.AddressType.Flag = "M";
          UseCabReadAdabasAddress();

          if (!IsEmpty(local.Ae.Street1))
          {
            local.Ae.LocationType = "D";
            local.Ae.Type1 = "M";
            local.Ae.Source = "AE";

            // ---------------------------------------------
            // 01/11/99 W.Campbell - Deleted set statements
            // for zdel_verified_code and zdel_start_date.
            // --------------------------------------------
            local.Ae.VerifiedDate = Now().Date;
            UseSiCheckForDuplicateAddress();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Case1.Number = "";
              export.NotFromReferral.Flag = "";
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
              export.Case1.Number = "";
              export.NotFromReferral.Flag = "";
              UseEabRollbackCics();

              return;
            }
          }

Test2:

          local.CsePerson.Number = local.ArCsePersonsWorkSet.Number;

          // * Copy the AE "R"esidential address.
          local.AddressType.Flag = "R";
          UseCabReadAdabasAddress();

          if (!IsEmpty(local.Ae.Street1))
          {
            local.Ae.LocationType = "D";
            local.Ae.Type1 = "R";
            local.Ae.Source = "AE";

            // ---------------------------------------------
            // 01/11/99 W.Campbell - Deleted set statements
            // for zdel_verified_code and zdel_start_date.
            // --------------------------------------------
            local.Ae.VerifiedDate = Now().Date;
            UseSiCheckForDuplicateAddress();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Case1.Number = "";
              export.NotFromReferral.Flag = "";
              UseEabRollbackCics();

              return;
            }

            if (AsChar(local.DuplicateAddress.Flag) == 'Y')
            {
              goto Test3;
            }

            UseSiCreateCsePersonAddress();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Case1.Number = "";
              export.NotFromReferral.Flag = "";
              UseEabRollbackCics();

              return;
            }
          }
        }

Test3:

        // ---------------------------------------------
        // Copy AR's INRD address for NADC cases.
        // --------------------------------------------
        if (AsChar(export.FromInrdCommon.Flag) == 'Y')
        {
          // 05/1/5/2002 M. Lachowicz End
          // * Copy the INRD "M"ailing address.
          UseSiInrdReadInformationReq();

          if (AsChar(local.ArAddress.Type1) == 'P')
          {
            local.CsePerson.Number = local.ApCsePerson.Number;
          }
          else
          {
            local.CsePerson.Number = local.ArCsePersonsWorkSet.Number;
          }

          // 05/1/5/2002 M. Lachowicz Start
          if (CharAt(local.CsePerson.Number, 10) == 'O')
          {
            goto Test4;
          }

          if (!IsEmpty(local.ArAddress.ApplicantStreet1))
          {
            local.Ae.State = local.ArAddress.ApplicantState ?? "";

            if (Equal(local.ArAddress.ApplicantState, "KS"))
            {
              // ---------------------------------------
              // 01/10/2000 W.Campbell - Inserted 2 move
              // statements to populate the zip code being
              // passed to EAB_RETURN_KS_COUNTY_BY_ZIP
              // the routine which is used to determine the
              // KS county code for an address.  This is
              // in the INRD logic.  The county code may be
              // used later in the assignment of the case
              // and case units to the service providers
              // in an Office.  Work done on PR# 77898.
              // -------------------------------------------------
              local.Ae.LocationType = "D";
              local.Ae.ZipCode = local.ArAddress.ApplicantZip5 ?? "";
              UseEabReturnKsCountyByZip();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";
                UseEabRollbackCics();

                return;
              }
            }

            local.Ae.Street1 = local.ArAddress.ApplicantStreet1 ?? "";
            local.Ae.Street2 = local.ArAddress.ApplicantStreet2 ?? "";
            local.Ae.City = local.ArAddress.ApplicantCity ?? "";
            local.Ae.State = local.ArAddress.ApplicantState ?? "";
            local.Ae.ZipCode = local.ArAddress.ApplicantZip5 ?? "";
            local.Ae.Zip4 = local.ArAddress.ApplicantZip4 ?? "";
            local.Ae.LocationType = "D";
            local.Ae.Type1 = "M";

            if (AsChar(local.ArAddress.Type1) == 'P')
            {
              local.Ae.Source = "AP";
            }
            else
            {
              local.Ae.Source = "AR";
            }

            // ---------------------------------------------
            // 01/11/99 W.Campbell - Deleted set statements
            // for zdel_verified_code and zdel_start_date.
            // --------------------------------------------
            local.Ae.VerifiedDate = Now().Date;
            UseSiCheckForDuplicateAddress();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Case1.Number = "";
              export.NotFromReferral.Flag = "";
              UseEabRollbackCics();

              return;
            }

            if (AsChar(local.DuplicateAddress.Flag) == 'Y')
            {
              goto Test4;
            }

            UseSiCreateCsePersonAddress();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Case1.Number = "";
              export.NotFromReferral.Flag = "";
              UseEabRollbackCics();

              return;
            }
          }
        }

Test4:

        // ---------------------------------------------
        // Create Person Programs for Cases being created from INRD
        // --------------------------------------------
        if (AsChar(export.FromInrdCommon.Flag) == 'Y')
        {
          // 07/29/99 M.L Start
          local.ReferralProgram.Code = "NA";
          local.ReferralPersonProgram.EffectiveDate = local.Current.Date;

          // 07/29/99 M.L End
          // 09/13/2001 M.L Start
          local.Security.Userid = "SWEIINRD";

          // 09/13/2001 M.L End
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH") || Equal
              (export.Export1.Item.DetailCaseRole.Type1, "AR"))
            {
              // 07/29/99 M.L Start
              UseSiRegiCheckPersonPrograms();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                // 05/30/00 M.L Start
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";

                // 05/30/00 M.L End
                UseEabRollbackCics();

                return;
              }

              if (AsChar(local.ActivePrograms.Flag) == 'Y')
              {
                continue;
              }

              // 09/13/2001 M.L Start
              if (AsChar(local.FuturePrograms.Flag) == 'Y')
              {
                continue;
              }
              else
              {
                local.ReferralPersonProgram.DiscontinueDate =
                  local.Maximum.Date;
              }

              // 09/13/2001 M.L End
              UseSiPeprCreatePersonProgram2();

              // 05/30/00 M.L Start
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";
                UseEabRollbackCics();

                return;
              }

              // 05/30/00 M.L End
              // 07/29/99 M.L End
            }
          }
        }

        // ---------------------------------------------
        // Create Person Programs for Cases being created from ICAS
        // --------------------------------------------
        if (AsChar(export.FromIapi.Flag) == 'Y')
        {
          // 07/29/99 M.L Start
          local.ReferralProgram.Code = export.FromInterstateCase.CaseType;
          local.ReferralPersonProgram.EffectiveDate = local.Current.Date;

          // 07/29/99 M.L End
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DetailCaseRole.Type1, "CH"))
            {
              // 07/29/99 M.L Start
              UseSiRegiCheckPersonPrograms();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                // 05/30/00 M.L Start
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";

                // 05/30/00 M.L End
                UseEabRollbackCics();

                return;
              }

              if (AsChar(local.ActivePrograms.Flag) == 'Y')
              {
                continue;
              }

              if (IsEmpty(local.FuturePrograms.Flag))
              {
                local.ReferralPersonProgram.DiscontinueDate =
                  local.InitialDate.Date;
              }

              UseSiPeprCreatePersonProgram1();

              // 07/29/99 M.L End
              // 05/30/00 M.L Start
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Case1.Number = "";
                export.NotFromReferral.Flag = "";
                UseEabRollbackCics();

                return;
              }

              // 05/30/00 M.L End
            }
          }
        }

        // ---------------------------------------------
        // Create Case Units
        // --------------------------------------------
        if (local.ApCommon.Count > 0)
        {
          UseSiDetermineCaseUnits();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }

          UseLeCabCopyLaCaseroles();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }
        }

        UseSiCaseAndCaseUnitAssignment();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Case1.Number = "";
          export.NotFromReferral.Flag = "";
          UseEabRollbackCics();

          return;
        }

        UseSiRegiRaiseEvents();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Case1.Number = "";
          export.NotFromReferral.Flag = "";
          UseEabRollbackCics();

          return;
        }

        // ---------------------------------------------
        // 03/19/99 W.Campbell - New code added
        // to satisfy CSEnet requirements.
        // --------------------------------------------
        if (AsChar(export.FromIapi.Flag) == 'Y')
        {
          // ---------------------------------------------
          // 03/24/99 W.Campbell - New code added
          // to create an acknowledgment going back
          // to the State which requested that we
          // establish this case.  This is to satisfy
          // CSEnet requirements.
          // --------------------------------------------
          // ---------------------------------------------
          // 06/04/99 W.Campbell - Renamed CAB
          // si_create_is_ack_for_new_ks_case TO
          // si_updt_ic_ref_with_new_ks_case.
          // Also within the CAB, disabled logic
          // to create an acknowledgment going back
          // to the State which requested that we
          // establish this case.  As per request by
          // Curtis Scroggins based on what he learned
          // at the ACF conference in order to satisfy
          // CSEnet requirements.
          // --------------------------------------------
          UseSiUpdtIcRefWithNewKsCase();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }

          UseSiCreateIcIsReqFrmReferral();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }

          UseSiUpdateReferralDeactStatus();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }
        }

        // 06/26/2000 M.L Start
        if (IsEmpty(local.ArFvIndicator.Number) || CharAt
          (local.ArFvIndicator.Number, 10) == 'O' || IsEmpty
          (local.FvIndicator.FamilyViolenceIndicator) || AsChar
          (local.ArFvIndicator.FamilyViolenceIndicator) == AsChar
          (local.FvIndicator.FamilyViolenceIndicator) || !
          IsEmpty(local.ArFvIndicator.FamilyViolenceIndicator))
        {
        }
        else
        {
          if (ReadCsePerson1())
          {
            try
            {
              UpdateCsePerson();
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
          else
          {
            ExitState = "CSE_PERSON_NF";
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }

          if (IsEmpty(local.ArFvIndicator.FamilyViolenceIndicator))
          {
            local.ArFvIndicator.FamilyViolenceIndicator =
              entities.CsePerson.FamilyViolenceIndicator;
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

        // 06/26/2000 M.L End
        // ---------------------------------------------
        // 03/19/99 W.Campbell - End of new code added
        // to satisfy CSEnet requirements.
        // --------------------------------------------
        // ---------------------------------------------
        // 09/11/00 W.Campbell - New code added
        // to move logic from PAR1 to REGI to
        // deactivate the PA Referral.
        // Work done on WR#00205.
        // --------------------------------------------
        if (AsChar(export.FromPar1.Flag) == 'Y')
        {
          // ---------------------------------------------
          // Must read the PA Referral to get the
          // FROM attribute data for the Referral.
          // It is not passed from PAR1.
          // --------------------------------------------
          if (ReadPaReferral())
          {
            MovePaReferral1(entities.PaReferral, local.PaReferral);
          }
          else
          {
            ExitState = "PA_REFERRAL_NF";
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }

          // ---------------------------------------------
          // Get the (an) AP to pass to the next
          // used action block.  For a PAR1 referral
          // there should only be one AP at most.
          // --------------------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AP"))
            {
              export.HiddenSelected.Number =
                export.Export1.Item.DetailCsePersonsWorkSet.Number;

              break;
            }
          }

          UseSiPar1AssocPaReferralCase();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ******************************************************
            // All OK, keep on going.
            // ******************************************************
          }
          else
          {
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }

          MovePaReferral3(export.FromPaReferral, local.PaReferral);

          // ---------------------------------------------
          // I think the value 'A' in the following statement
          // means that the PA Referral has been assigned
          // to a CSE CASE and deactivated..
          // 09/11/00 - W.Campbell
          // --------------------------------------------
          local.PaReferral.AssignDeactivateInd = "A";
          UseSiPar1DeactPaReferral();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ******************************************************
            // All OK, keep on going.
            // ******************************************************
          }
          else
          {
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }

          UseEabUpdatePaReferral();

          // START CQ42035 - Change the use of local_eab abend_date to local 
          // abend_data
          // to match the view returning the error message from 
          // eab_update_pa_referral.
          switch(AsChar(local.AbendData.Type1))
          {
            case ' ':
              // ******************************************************
              // All OK, keep on going.
              // ******************************************************
              break;
            case 'A':
              if (Equal(local.AbendData.Type1, "0000"))
              {
                ExitState = "ACO_ADABAS_UNAVAILABLE";
              }
              else
              {
                ExitState = "ADABAS_INVALID_RETURN_CODE";
              }

              break;
            default:
              ExitState = "ADABAS_INVALID_RETURN_CODE";

              break;
          }

          // END CQ42035 - Change the use of local_eab abend_date to local 
          // abend_data
          // to match the view returning the error message from 
          // eab_update_pa_referral.
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            // ******************************************************
            // All OK, keep on going.
            // ******************************************************
          }
          else
          {
            export.Case1.Number = "";
            export.NotFromReferral.Flag = "";
            UseEabRollbackCics();

            return;
          }
        }

        // ---------------------------------------------
        // 09/11/00 W.Campbell - End of new code added
        // to move logic from PAR1 to REGI to
        // deactivate the PA Referral.
        // Work done on WR#00205.
        // --------------------------------------------
        ExitState = "SI0000_CASE_REGISTRATION_SUCC";
        export.RegisterSuccessful.Flag = "Y";
        export.NotFromReferral.Flag = "";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETURN":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AP"))
          {
            export.HiddenSelected.Number =
              export.Export1.Item.DetailCsePersonsWorkSet.Number;
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        return;
      case "NAME":
        export.FromRegiToName.Flag = "Y";
        ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

        return;
      case "COMP":
        ExitState = "ECO_XFR_TO_CASE_COMPOSITION";

        return;
      case "IIDC":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.DetailCaseRole.Type1, "AP"))
          {
            export.HiddenSelected.Number =
              export.Export1.Item.DetailCsePersonsWorkSet.Number;
          }
        }

        ExitState = "ECO_LNK_TO_IIDC";

        return;
      case "ORGZ":
        ExitState = "ECO_LNK_TO_ORG_MAINTENANCE";

        return;
      case "LIST":
        switch(AsChar(import.Prompt.SelectChar))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_LIST_OFFICE";
            export.Prompt.SelectChar = "";

            return;
          case ' ':
            break;
          default:
            var field = GetField(export.Prompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        // -----------------------------------------------
        // 11/12/98 W. Campbell  Added stmt to set
        // exit state to provide an error msg for no
        // selection on a prompt (PFK=4 'LIST').
        // -----------------------------------------------
        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      default:
        break;
    }

    // ----------------------------------
    // If the CSE has been prompted
    // for an action, protect all fields.
    // ----------------------------------
    if (AsChar(export.SelectActionCommon.Flag) == 'Y')
    {
      var field1 = GetField(export.Office, "systemGeneratedId");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Prompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (Equal(export.Export1.Item.DetailCsePersonsWorkSet.Number,
          export.SelectActionCsePersonsWorkSet.Number))
        {
          var field14 = GetField(export.Export1.Item.DetailCaseRole, "type1");

          field14.Color = "red";
          field14.Intensity = Intensity.High;
          field14.Highlighting = Highlighting.ReverseVideo;
          field14.Protected = true;
        }
        else
        {
          var field14 = GetField(export.Export1.Item.DetailCaseRole, "type1");

          field14.Color = "cyan";
          field14.Protected = true;
        }

        var field = GetField(export.Export1.Item.DetailFamily, "type1");

        field.Color = "cyan";
        field.Protected = true;
      }

      var field3 = GetField(export.NewCsePersonsWorkSet, "lastName");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.NewCsePersonsWorkSet, "firstName");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.NewCsePersonsWorkSet, "middleInitial");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.NewCsePersonsWorkSet, "sex");

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

      var field10 = GetField(export.NewSsnWorkArea, "ssnTextPart1");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.NewSsnWorkArea, "ssnTextPart2");

      field11.Color = "cyan";
      field11.Protected = true;

      var field12 = GetField(export.NewSsnWorkArea, "ssnTextPart3");

      field12.Color = "cyan";
      field12.Protected = true;

      var field13 = GetField(export.NewCsePersonsWorkSet, "dob");

      field13.Color = "cyan";
      field13.Protected = true;
    }
  }

  private static void MoveCaseRole1(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.Note = source.Note;
    target.OnSsInd = source.OnSsInd;
    target.HealthInsuranceIndicator = source.HealthInsuranceIndicator;
    target.MedicalSupportIndicator = source.MedicalSupportIndicator;
    target.AbsenceReasonCode = source.AbsenceReasonCode;
    target.PriorMedicalSupport = source.PriorMedicalSupport;
    target.ArWaivedInsurance = source.ArWaivedInsurance;
    target.DateOfEmancipation = source.DateOfEmancipation;
    target.FcAdoptionDisruptionInd = source.FcAdoptionDisruptionInd;
    target.FcApNotified = source.FcApNotified;
    target.FcCincInd = source.FcCincInd;
    target.FcCostOfCare = source.FcCostOfCare;
    target.FcCostOfCareFreq = source.FcCostOfCareFreq;
    target.FcCountyChildRemovedFrom = source.FcCountyChildRemovedFrom;
    target.FcDateOfInitialCustody = source.FcDateOfInitialCustody;
    target.FcInHomeServiceInd = source.FcInHomeServiceInd;
    target.FcIvECaseNumber = source.FcIvECaseNumber;
    target.FcJuvenileCourtOrder = source.FcJuvenileCourtOrder;
    target.FcJuvenileOffenderInd = source.FcJuvenileOffenderInd;
    target.FcLevelOfCare = source.FcLevelOfCare;
    target.FcNextJuvenileCtDt = source.FcNextJuvenileCtDt;
    target.FcOrderEstBy = source.FcOrderEstBy;
    target.FcOtherBenefitInd = source.FcOtherBenefitInd;
    target.FcParentalRights = source.FcParentalRights;
    target.FcPrevPayeeFirstName = source.FcPrevPayeeFirstName;
    target.FcPrevPayeeMiddleInitial = source.FcPrevPayeeMiddleInitial;
    target.FcPlacementDate = source.FcPlacementDate;
    target.FcPlacementName = source.FcPlacementName;
    target.FcPlacementReason = source.FcPlacementReason;
    target.FcPreviousPa = source.FcPreviousPa;
    target.FcPreviousPayeeLastName = source.FcPreviousPayeeLastName;
    target.FcSourceOfFunding = source.FcSourceOfFunding;
    target.FcSrsPayee = source.FcSrsPayee;
    target.FcSsa = source.FcSsa;
    target.FcSsi = source.FcSsi;
    target.FcVaInd = source.FcVaInd;
    target.FcWardsAccount = source.FcWardsAccount;
    target.FcZebInd = source.FcZebInd;
    target.Over18AndInSchool = source.Over18AndInSchool;
    target.ResidesWithArIndicator = source.ResidesWithArIndicator;
    target.SpecialtyArea = source.SpecialtyArea;
    target.RelToAr = source.RelToAr;
  }

  private static void MoveCaseRole2(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.RelToAr = source.RelToAr;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.Flag = source.Flag;
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
    target.UnemploymentInd = source.UnemploymentInd;
    target.FederalInd = source.FederalInd;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.PaternityEstablishedIndicator = source.PaternityEstablishedIndicator;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZdelStartDate = source.ZdelStartDate;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.County = source.County;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonAddress4(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.Flag = source.Flag;
    target.LastName = source.LastName;
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
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
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

  private static void MoveExport1ToImport2(Export.ExportGroup source,
    SiDetermineCaseUnits.Import.ImportGroup target)
  {
    target.DetailCsePersonsWorkSet.Assign(source.DetailCsePersonsWorkSet);
    target.DetailCaseRole.Type1 = source.DetailCaseRole.Type1;
    target.DetailFamily.Type1 = source.DetailFamily.Type1;
    target.DetailCaseCnfrm.Flag = source.DetailCaseCnfrm.Flag;
  }

  private static void MoveExport1ToImport3(Export.ExportGroup source,
    SiAddPersonToCase.Import.ImportGroup target)
  {
    target.DetailCsePersonsWorkSet.Assign(source.DetailCsePersonsWorkSet);
    target.DetailCaseRole.Type1 = source.DetailCaseRole.Type1;
    target.DetailFamily.Type1 = source.DetailFamily.Type1;
    target.DetailCaseConfrm.Flag = source.DetailCaseCnfrm.Flag;
  }

  private static void MoveExport1ToImport4(Export.ExportGroup source,
    SiRegiChkForDuplicateCase.Import.ImportGroup target)
  {
    target.DetailCsePersonsWorkSet.Assign(source.DetailCsePersonsWorkSet);
    target.DetailCaseRole.Type1 = source.DetailCaseRole.Type1;
    target.DetailFamily.Type1 = source.DetailFamily.Type1;
    target.DetailCaseCnfrm.Flag = source.DetailCaseCnfrm.Flag;
  }

  private static void MoveImport1ToExport1(SiAddPersonToCase.Import.
    ImportGroup source, Export.ExportGroup target)
  {
    target.DetailCsePersonsWorkSet.Assign(source.DetailCsePersonsWorkSet);
    target.DetailCaseRole.Type1 = source.DetailCaseRole.Type1;
    target.DetailFamily.Type1 = source.DetailFamily.Type1;
    target.DetailCaseCnfrm.Flag = source.DetailCaseConfrm.Flag;
  }

  private static void MoveInformationRequest(InformationRequest source,
    InformationRequest target)
  {
    target.ApplicantStreet1 = source.ApplicantStreet1;
    target.ApplicantStreet2 = source.ApplicantStreet2;
    target.ApplicantCity = source.ApplicantCity;
    target.ApplicantState = source.ApplicantState;
    target.ApplicantZip5 = source.ApplicantZip5;
    target.ApplicantZip4 = source.ApplicantZip4;
    target.ApplicantZip3 = source.ApplicantZip3;
    target.Type1 = source.Type1;
    target.ReopenReasonType = source.ReopenReasonType;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MovePaReferral1(PaReferral source, PaReferral target)
  {
    target.From = source.From;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.Type1 = source.Type1;
  }

  private static void MovePaReferral2(PaReferral source, PaReferral target)
  {
    target.From = source.From;
    target.Number = source.Number;
  }

  private static void MovePaReferral3(PaReferral source, PaReferral target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Number = source.Number;
    target.Type1 = source.Type1;
  }

  private static void MoveSsnWorkArea(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabCreateAdabasPerson1()
  {
    var useImport = new CabCreateAdabasPerson.Import();
    var useExport = new CabCreateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(
      export.Export1.Item.DetailCsePersonsWorkSet);

    Call(CabCreateAdabasPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Number = useExport.CsePersonsWorkSet.Number;
  }

  private void UseCabCreateAdabasPerson2()
  {
    var useImport = new CabCreateAdabasPerson.Import();
    var useExport = new CabCreateAdabasPerson.Export();

    useImport.CsePersonsWorkSet.Assign(export.NewCsePersonsWorkSet);

    Call(CabCreateAdabasPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet3(useExport.CsePersonsWorkSet,
      export.NewCsePersonsWorkSet);
  }

  private void UseCabReadAdabasAddress()
  {
    var useImport = new CabReadAdabasAddress.Import();
    var useExport = new CabReadAdabasAddress.Export();

    useImport.CsePersonsWorkSet.Number = local.ArCsePersonsWorkSet.Number;
    useImport.AddressType.Flag = local.AddressType.Flag;

    Call(CabReadAdabasAddress.Execute, useImport, useExport);

    MoveCsePersonAddress2(useExport.Ae, local.Ae);
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number =
      export.Export1.Item.DetailCsePersonsWorkSet.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    local.Cse.Flag = useExport.Cse.Flag;
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Maximum.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    useImport.SsnWorkArea.Assign(local.New1);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea(useExport.SsnWorkArea, export.NewSsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = local.New1.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea(useExport.SsnWorkArea, export.NewSsnWorkArea);
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseEabReturnKsCountyByZip()
  {
    var useImport = new EabReturnKsCountyByZip.Import();
    var useExport = new EabReturnKsCountyByZip.Export();

    MoveCsePersonAddress4(local.Ae, useImport.CsePersonAddress);
    MoveCsePersonAddress3(local.Ae, useExport.CsePersonAddress);

    Call(EabReturnKsCountyByZip.Execute, useImport, useExport);

    MoveCsePersonAddress3(useExport.CsePersonAddress, local.Ae);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseEabUpdatePaReferral()
  {
    var useImport = new EabUpdatePaReferral.Import();
    var useExport = new EabUpdatePaReferral.Export();

    MovePaReferral2(entities.PaReferral, useImport.PaReferral);
    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonsWorkSet.Number = export.HiddenSelected.Number;
    useImport.Case1.Number = export.Case1.Number;
    useExport.AbendData.Assign(local.AbendData);

    Call(EabUpdatePaReferral.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseLeCabCopyLaCaseroles()
  {
    var useImport = new LeCabCopyLaCaseroles.Import();
    var useExport = new LeCabCopyLaCaseroles.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(LeCabCopyLaCaseroles.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

  private void UseSiAddPersonToCase()
  {
    var useImport = new SiAddPersonToCase.Import();
    var useExport = new SiAddPersonToCase.Export();

    useImport.Add.Assign(export.NewCsePersonsWorkSet);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport3);

    Call(SiAddPersonToCase.Execute, useImport, useExport);

    useImport.Import1.CopyTo(export.Export1, MoveImport1ToExport1);
  }

  private void UseSiAltsCabUpdateAlias()
  {
    var useImport = new SiAltsCabUpdateAlias.Import();
    var useExport = new SiAltsCabUpdateAlias.Export();

    useImport.CsePersonsWorkSet.Assign(local.Alias);

    Call(SiAltsCabUpdateAlias.Execute, useImport, useExport);
  }

  private void UseSiCaseAndCaseUnitAssignment()
  {
    var useImport = new SiCaseAndCaseUnitAssignment.Import();
    var useExport = new SiCaseAndCaseUnitAssignment.Export();

    useImport.Case1.Number = local.Case2.Number;
    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;

    Call(SiCaseAndCaseUnitAssignment.Execute, useImport, useExport);
  }

  private void UseSiChangeArFvIndicator()
  {
    var useImport = new SiChangeArFvIndicator.Import();
    var useExport = new SiChangeArFvIndicator.Export();

    useImport.CsePerson.Assign(entities.CsePerson);
    useImport.Case1.Number = export.Next.Number;

    Call(SiChangeArFvIndicator.Execute, useImport, useExport);
  }

  private void UseSiCheckApChCombinations()
  {
    var useImport = new SiCheckApChCombinations.Import();
    var useExport = new SiCheckApChCombinations.Export();

    useImport.Ap.Number = local.MaleApCsePerson.Number;
    useImport.Ch.Number = local.ChCsePerson.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiCheckApChCombinations.Execute, useImport, useExport);

    local.ActiveOtherMaleAp.Flag = useExport.ActiveOtherMaleAp.Flag;
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseSiCheckForDuplicateAddress()
  {
    var useImport = new SiCheckForDuplicateAddress.Import();
    var useExport = new SiCheckForDuplicateAddress.Export();

    MoveCsePersonAddress1(local.Ae, useImport.CsePersonAddress);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiCheckForDuplicateAddress.Execute, useImport, useExport);

    local.DuplicateAddress.Flag = useExport.DuplicateAddress.Flag;
  }

  private void UseSiCheckName1()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    useImport.CsePersonsWorkSet.Assign(export.NewCsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiCheckName2()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    useImport.CsePersonsWorkSet.Assign(
      export.Export1.Item.DetailCsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiChkCasesForChWithDiffAr()
  {
    var useImport = new SiChkCasesForChWithDiffAr.Import();
    var useExport = new SiChkCasesForChWithDiffAr.Export();

    useImport.Ch.Number = local.ChCsePerson.Number;
    useImport.Ar.Number = local.ArCsePerson.Number;

    Call(SiChkCasesForChWithDiffAr.Execute, useImport, useExport);

    local.Case2.Number = useExport.Case1.Number;
    MoveCsePerson2(useExport.ChPaternityEstInd, local.ChildPaternityEstInd);
  }

  private void UseSiCreateCaseRole()
  {
    var useImport = new SiCreateCaseRole.Import();
    var useExport = new SiCreateCaseRole.Export();

    MoveCaseRole2(local.CaseRole, useImport.CaseRole);
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Case1.Number = local.Case2.Number;

    Call(SiCreateCaseRole.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePerson1()
  {
    var useImport = new SiCreateCsePerson.Import();
    var useExport = new SiCreateCsePerson.Export();

    MoveCsePerson1(local.CsePerson, useImport.CsePerson);
    useImport.CsePersonsWorkSet.Assign(export.NewCsePersonsWorkSet);

    Call(SiCreateCsePerson.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePerson2()
  {
    var useImport = new SiCreateCsePerson.Import();
    var useExport = new SiCreateCsePerson.Export();

    MoveCsePerson1(local.CsePerson, useImport.CsePerson);
    useImport.CsePersonsWorkSet.Assign(
      export.Export1.Item.DetailCsePersonsWorkSet);

    Call(SiCreateCsePerson.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePersonAddress()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    MoveCsePersonAddress1(local.Ae, useImport.CsePersonAddress);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress, local.Ae);
  }

  private void UseSiCreateIcIsReqFrmReferral()
  {
    var useImport = new SiCreateIcIsReqFrmReferral.Import();
    var useExport = new SiCreateIcIsReqFrmReferral.Export();

    useImport.Ap.Number = local.ApCsePerson.Number;
    useImport.NewlyCreated.Number = export.Case1.Number;
    MoveInterstateCase(export.FromInterstateCase, useImport.InterstateCase);

    Call(SiCreateIcIsReqFrmReferral.Execute, useImport, useExport);
  }

  private void UseSiDetermineCaseUnits()
  {
    var useImport = new SiDetermineCaseUnits.Import();
    var useExport = new SiDetermineCaseUnits.Export();

    useImport.Ar.Number = local.ArCsePersonsWorkSet.Number;
    useImport.Ap.Count = local.ApCommon.Count;
    useImport.Ch.Count = local.ChCommon.Count;
    useImport.Case1.Number = export.Case1.Number;
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport2);

    Call(SiDetermineCaseUnits.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.NewCsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.NewCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFvEvent()
  {
    var useImport = new SiFvEvent.Import();
    var useExport = new SiFvEvent.Export();

    useImport.CsePerson.Assign(local.ArFvIndicator);

    Call(SiFvEvent.Execute, useImport, useExport);
  }

  private void UseSiInrdReadInformationReq()
  {
    var useImport = new SiInrdReadInformationReq.Import();
    var useExport = new SiInrdReadInformationReq.Export();

    useImport.InformationRequest.Number =
      import.FromInrdInformationRequest.Number;

    Call(SiInrdReadInformationReq.Execute, useImport, useExport);

    MoveInformationRequest(useExport.InformationRequest, local.ArAddress);
  }

  private void UseSiPar1AssocPaReferralCase()
  {
    var useImport = new SiPar1AssocPaReferralCase.Import();
    var useExport = new SiPar1AssocPaReferralCase.Export();

    useImport.Ap.Number = export.HiddenSelected.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.PaReferral.Assign(export.FromPaReferral);

    Call(SiPar1AssocPaReferralCase.Execute, useImport, useExport);
  }

  private void UseSiPar1DeactPaReferral()
  {
    var useImport = new SiPar1DeactPaReferral.Import();
    var useExport = new SiPar1DeactPaReferral.Export();

    useImport.PaReferral.Assign(local.PaReferral);

    Call(SiPar1DeactPaReferral.Execute, useImport, useExport);
  }

  private void UseSiPeprCreatePersonProgram1()
  {
    var useImport = new SiPeprCreatePersonProgram.Import();
    var useExport = new SiPeprCreatePersonProgram.Export();

    useImport.Program.Code = local.ReferralProgram.Code;
    useImport.PersonProgram.Assign(local.ReferralPersonProgram);
    useImport.CsePersonsWorkSet.Number =
      export.Export1.Item.DetailCsePersonsWorkSet.Number;

    Call(SiPeprCreatePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiPeprCreatePersonProgram2()
  {
    var useImport = new SiPeprCreatePersonProgram.Import();
    var useExport = new SiPeprCreatePersonProgram.Export();

    useImport.Program.Code = local.ReferralProgram.Code;
    useImport.PersonProgram.Assign(local.ReferralPersonProgram);
    useImport.CsePersonsWorkSet.Number =
      export.Export1.Item.DetailCsePersonsWorkSet.Number;
    useImport.Security.Userid = local.Security.Userid;

    Call(SiPeprCreatePersonProgram.Execute, useImport, useExport);
  }

  private void UseSiRegiCheckPersonPrograms()
  {
    var useImport = new SiRegiCheckPersonPrograms.Import();
    var useExport = new SiRegiCheckPersonPrograms.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Export1.Item.DetailCsePersonsWorkSet.Number;

    Call(SiRegiCheckPersonPrograms.Execute, useImport, useExport);

    local.FuturePrograms.Flag = useExport.FuturePrograms.Flag;
    local.ActivePrograms.Flag = useExport.ActivePrograms.Flag;
    local.ReferralPersonProgram.DiscontinueDate =
      useExport.DiscontinueDate.DiscontinueDate;
  }

  private void UseSiRegiChkForDuplicateCase()
  {
    var useImport = new SiRegiChkForDuplicateCase.Import();
    var useExport = new SiRegiChkForDuplicateCase.Export();

    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport4);

    Call(SiRegiChkForDuplicateCase.Execute, useImport, useExport);

    local.DuplicateCase.Flag = useExport.DuplicateCase.Flag;
  }

  private void UseSiRegiCopyAdabasPersonPgms()
  {
    var useImport = new SiRegiCopyAdabasPersonPgms.Import();
    var useExport = new SiRegiCopyAdabasPersonPgms.Export();

    useImport.CseReferralReceived.Date = local.Current.Date;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiRegiCopyAdabasPersonPgms.Execute, useImport, useExport);
  }

  private void UseSiRegiCreateCase()
  {
    var useImport = new SiRegiCreateCase.Import();
    var useExport = new SiRegiCreateCase.Export();

    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;
    useImport.FromIapi.Flag = export.FromIapi.Flag;
    useImport.FromInrdInformationRequest.Number =
      export.FromInrdInformationRequest.Number;
    MoveInterstateCase(export.FromInterstateCase, useImport.FromInterstateCase);
    useImport.FromInrdCommon.Flag = export.FromInrdCommon.Flag;
    useImport.FromPar1.Flag = export.FromPar1.Flag;
    useImport.FromPaReferral.Assign(export.FromPaReferral);

    Call(SiRegiCreateCase.Execute, useImport, useExport);

    local.Case2.Number = useExport.Case1.Number;
    export.FromInterstateCase.Assign(useExport.InterstateCase);
  }

  private void UseSiRegiRaiseEvents()
  {
    var useImport = new SiRegiRaiseEvents.Import();
    var useExport = new SiRegiRaiseEvents.Export();

    useImport.Ap.Count = local.ApCommon.Count;
    useImport.Case1.Number = export.Case1.Number;
    useImport.FromIapi.Flag = export.FromIapi.Flag;
    useImport.FromInrd.Flag = export.FromInrdCommon.Flag;
    useImport.FromPar1.Flag = export.FromPar1.Flag;

    Call(SiRegiRaiseEvents.Execute, useImport, useExport);
  }

  private void UseSiSetClientCaseRoleDetails()
  {
    var useImport = new SiSetClientCaseRoleDetails.Import();
    var useExport = new SiSetClientCaseRoleDetails.Export();

    useImport.CaseRole.Type1 = local.CaseRole.Type1;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.FromIapi.Flag = export.FromIapi.Flag;
    useImport.FromInrdInformationRequest.Number =
      export.FromInrdInformationRequest.Number;
    MoveInterstateCase(export.FromInterstateCase, useImport.FromInterstateCase);
    useImport.FromInrdCommon.Flag = export.FromInrdCommon.Flag;
    useImport.FromPar1.Flag = export.FromPar1.Flag;
    useImport.FromPaReferral.Assign(export.FromPaReferral);
    useImport.ApSex.Sex = local.ApSex.Sex;

    Call(SiSetClientCaseRoleDetails.Execute, useImport, useExport);

    MoveCaseRole1(useExport.ChCaseRole, local.CaseRole);
  }

  private void UseSiUpdateReferralDeactStatus()
  {
    var useImport = new SiUpdateReferralDeactStatus.Import();
    var useExport = new SiUpdateReferralDeactStatus.Export();

    MoveInterstateCase(export.FromInterstateCase, useImport.InterstateCase);

    Call(SiUpdateReferralDeactStatus.Execute, useImport, useExport);
  }

  private void UseSiUpdtIcRefWithNewKsCase()
  {
    var useImport = new SiUpdtIcRefWithNewKsCase.Import();
    var useExport = new SiUpdtIcRefWithNewKsCase.Export();

    useImport.Case1.Number = export.Case1.Number;
    MoveInterstateCase(export.FromInterstateCase, useImport.InterstateCase);

    Call(SiUpdtIcRefWithNewKsCase.Execute, useImport, useExport);
  }

  private void UseSiVerifyChildIsOnACase()
  {
    var useImport = new SiVerifyChildIsOnACase.Import();
    var useExport = new SiVerifyChildIsOnACase.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.CaseRole.StartDate = local.CaseRole.StartDate;

    Call(SiVerifyChildIsOnACase.Execute, useImport, useExport);

    local.RelToArIsCh.Flag = useExport.RelToArIsCh.Flag;
    MoveCommon(useExport.ActiveCaseCh, local.ActiveChildOnCase);
    local.InactiveChildOnCase.Flag = useExport.InactiveCaseCh.Flag;
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.ArFvIndicator.Number);
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
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.Export1.Item.DetailCsePersonsWorkSet.Number);
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

  private bool ReadInformationRequest()
  {
    entities.InformationRequest.Populated = false;

    return Read("ReadInformationRequest",
      (db, command) =>
      {
        db.SetInt64(command, "numb", import.FromInrdInformationRequest.Number);
      },
      (db, reader) =>
      {
        entities.InformationRequest.Number = db.GetInt64(reader, 0);
        entities.InformationRequest.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InformationRequest.ApplicationProcessedInd =
          db.GetNullableString(reader, 2);
        entities.InformationRequest.NameSearchComplete =
          db.GetNullableString(reader, 3);
        entities.InformationRequest.FkCktCasenumb = db.GetString(reader, 4);
        entities.InformationRequest.Populated = true;
      });
  }

  private bool ReadPaReferral()
  {
    entities.PaReferral.Populated = false;

    return Read("ReadPaReferral",
      (db, command) =>
      {
        db.SetString(command, "numb", export.FromPaReferral.Number);
        db.SetString(command, "type", export.FromPaReferral.Type1);
        db.SetDateTime(
          command, "createdTstamp",
          export.FromPaReferral.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaReferral.Number = db.GetString(reader, 0);
        entities.PaReferral.Type1 = db.GetString(reader, 1);
        entities.PaReferral.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PaReferral.From = db.GetNullableString(reader, 3);
        entities.PaReferral.Populated = true;
      });
  }

  private void UpdateCsePerson()
  {
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var familyViolenceIndicator = local.FvIndicator.FamilyViolenceIndicator ?? ""
      ;
    var fviSetDate = local.Current.Date;

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson",
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

      /// <summary>
      /// A value of DetailFamily.
      /// </summary>
      [JsonPropertyName("detailFamily")]
      public CaseRole DetailFamily
      {
        get => detailFamily ??= new();
        set => detailFamily = value;
      }

      /// <summary>
      /// A value of DetailCaseCnfrm.
      /// </summary>
      [JsonPropertyName("detailCaseCnfrm")]
      public Common DetailCaseCnfrm
      {
        get => detailCaseCnfrm ??= new();
        set => detailCaseCnfrm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
      private CaseRole detailFamily;
      private Common detailCaseCnfrm;
    }

    /// <summary>A FromNameGroup group.</summary>
    [Serializable]
    public class FromNameGroup
    {
      /// <summary>
      /// A value of NameDetail.
      /// </summary>
      [JsonPropertyName("nameDetail")]
      public CsePersonsWorkSet NameDetail
      {
        get => nameDetail ??= new();
        set => nameDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet nameDetail;
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
    /// A value of NotFromReferral.
    /// </summary>
    [JsonPropertyName("notFromReferral")]
    public Common NotFromReferral
    {
      get => notFromReferral ??= new();
      set => notFromReferral = value;
    }

    /// <summary>
    /// A value of RegisterSuccessful.
    /// </summary>
    [JsonPropertyName("registerSuccessful")]
    public Common RegisterSuccessful
    {
      get => registerSuccessful ??= new();
      set => registerSuccessful = value;
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
    /// A value of NameReturn.
    /// </summary>
    [JsonPropertyName("nameReturn")]
    public CsePersonsWorkSet NameReturn
    {
      get => nameReturn ??= new();
      set => nameReturn = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Case1 HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of SelectActionWorkArea.
    /// </summary>
    [JsonPropertyName("selectActionWorkArea")]
    public WorkArea SelectActionWorkArea
    {
      get => selectActionWorkArea ??= new();
      set => selectActionWorkArea = value;
    }

    /// <summary>
    /// A value of SelectActionCommon.
    /// </summary>
    [JsonPropertyName("selectActionCommon")]
    public Common SelectActionCommon
    {
      get => selectActionCommon ??= new();
      set => selectActionCommon = value;
    }

    /// <summary>
    /// A value of SelectActionCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectActionCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectActionCsePersonsWorkSet
    {
      get => selectActionCsePersonsWorkSet ??= new();
      set => selectActionCsePersonsWorkSet = value;
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
    /// A value of FromIapi.
    /// </summary>
    [JsonPropertyName("fromIapi")]
    public Common FromIapi
    {
      get => fromIapi ??= new();
      set => fromIapi = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

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
    /// Gets a value of FromName.
    /// </summary>
    [JsonIgnore]
    public Array<FromNameGroup> FromName => fromName ??= new(
      FromNameGroup.Capacity);

    /// <summary>
    /// Gets a value of FromName for json serialization.
    /// </summary>
    [JsonPropertyName("fromName")]
    [Computed]
    public IList<FromNameGroup> FromName_Json
    {
      get => fromName;
      set => FromName.Assign(value);
    }

    /// <summary>
    /// A value of FromInrdInformationRequest.
    /// </summary>
    [JsonPropertyName("fromInrdInformationRequest")]
    public InformationRequest FromInrdInformationRequest
    {
      get => fromInrdInformationRequest ??= new();
      set => fromInrdInformationRequest = value;
    }

    /// <summary>
    /// A value of FromInterstateCase.
    /// </summary>
    [JsonPropertyName("fromInterstateCase")]
    public InterstateCase FromInterstateCase
    {
      get => fromInterstateCase ??= new();
      set => fromInterstateCase = value;
    }

    /// <summary>
    /// A value of FromInrdCommon.
    /// </summary>
    [JsonPropertyName("fromInrdCommon")]
    public Common FromInrdCommon
    {
      get => fromInrdCommon ??= new();
      set => fromInrdCommon = value;
    }

    /// <summary>
    /// A value of FromPar1.
    /// </summary>
    [JsonPropertyName("fromPar1")]
    public Common FromPar1
    {
      get => fromPar1 ??= new();
      set => fromPar1 = value;
    }

    /// <summary>
    /// A value of FromPaReferral.
    /// </summary>
    [JsonPropertyName("fromPaReferral")]
    public PaReferral FromPaReferral
    {
      get => fromPaReferral ??= new();
      set => fromPaReferral = value;
    }

    private Common returnFromCpat;
    private Common notFromReferral;
    private Common registerSuccessful;
    private Common beenToName;
    private CsePersonsWorkSet nameReturn;
    private Common prompt;
    private Office office;
    private Case1 hiddenPrev;
    private Case1 case1;
    private Standard standard;
    private Case1 next;
    private CsePersonsWorkSet newCsePersonsWorkSet;
    private WorkArea selectActionWorkArea;
    private Common selectActionCommon;
    private CsePersonsWorkSet selectActionCsePersonsWorkSet;
    private SsnWorkArea newSsnWorkArea;
    private Common fromIapi;
    private NextTranInfo hidden;
    private Array<ImportGroup> import1;
    private Array<FromNameGroup> fromName;
    private InformationRequest fromInrdInformationRequest;
    private InterstateCase fromInterstateCase;
    private Common fromInrdCommon;
    private Common fromPar1;
    private PaReferral fromPaReferral;
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

      /// <summary>
      /// A value of DetailFamily.
      /// </summary>
      [JsonPropertyName("detailFamily")]
      public CaseRole DetailFamily
      {
        get => detailFamily ??= new();
        set => detailFamily = value;
      }

      /// <summary>
      /// A value of DetailCaseCnfrm.
      /// </summary>
      [JsonPropertyName("detailCaseCnfrm")]
      public Common DetailCaseCnfrm
      {
        get => detailCaseCnfrm ??= new();
        set => detailCaseCnfrm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private CaseRole detailCaseRole;
      private CaseRole detailFamily;
      private Common detailCaseCnfrm;
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
    /// A value of NotFromReferral.
    /// </summary>
    [JsonPropertyName("notFromReferral")]
    public Common NotFromReferral
    {
      get => notFromReferral ??= new();
      set => notFromReferral = value;
    }

    /// <summary>
    /// A value of FromRegiToName.
    /// </summary>
    [JsonPropertyName("fromRegiToName")]
    public Common FromRegiToName
    {
      get => fromRegiToName ??= new();
      set => fromRegiToName = value;
    }

    /// <summary>
    /// A value of RegisterSuccessful.
    /// </summary>
    [JsonPropertyName("registerSuccessful")]
    public Common RegisterSuccessful
    {
      get => registerSuccessful ??= new();
      set => registerSuccessful = value;
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
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public CsePersonsWorkSet HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Case1 HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of SelectActionWorkArea.
    /// </summary>
    [JsonPropertyName("selectActionWorkArea")]
    public WorkArea SelectActionWorkArea
    {
      get => selectActionWorkArea ??= new();
      set => selectActionWorkArea = value;
    }

    /// <summary>
    /// A value of SelectActionCommon.
    /// </summary>
    [JsonPropertyName("selectActionCommon")]
    public Common SelectActionCommon
    {
      get => selectActionCommon ??= new();
      set => selectActionCommon = value;
    }

    /// <summary>
    /// A value of SelectActionCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectActionCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectActionCsePersonsWorkSet
    {
      get => selectActionCsePersonsWorkSet ??= new();
      set => selectActionCsePersonsWorkSet = value;
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
    /// A value of FromIapi.
    /// </summary>
    [JsonPropertyName("fromIapi")]
    public Common FromIapi
    {
      get => fromIapi ??= new();
      set => fromIapi = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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
    /// A value of FromInrdInformationRequest.
    /// </summary>
    [JsonPropertyName("fromInrdInformationRequest")]
    public InformationRequest FromInrdInformationRequest
    {
      get => fromInrdInformationRequest ??= new();
      set => fromInrdInformationRequest = value;
    }

    /// <summary>
    /// A value of FromInterstateCase.
    /// </summary>
    [JsonPropertyName("fromInterstateCase")]
    public InterstateCase FromInterstateCase
    {
      get => fromInterstateCase ??= new();
      set => fromInterstateCase = value;
    }

    /// <summary>
    /// A value of FromInrdCommon.
    /// </summary>
    [JsonPropertyName("fromInrdCommon")]
    public Common FromInrdCommon
    {
      get => fromInrdCommon ??= new();
      set => fromInrdCommon = value;
    }

    /// <summary>
    /// A value of FromPar1.
    /// </summary>
    [JsonPropertyName("fromPar1")]
    public Common FromPar1
    {
      get => fromPar1 ??= new();
      set => fromPar1 = value;
    }

    /// <summary>
    /// A value of FromPaReferral.
    /// </summary>
    [JsonPropertyName("fromPaReferral")]
    public PaReferral FromPaReferral
    {
      get => fromPaReferral ??= new();
      set => fromPaReferral = value;
    }

    private Common returnFromCpat;
    private Common notFromReferral;
    private Common fromRegiToName;
    private Common registerSuccessful;
    private Common beenToName;
    private CsePersonsWorkSet hiddenSelected;
    private Common prompt;
    private Office office;
    private Case1 hiddenPrev;
    private Case1 case1;
    private Standard standard;
    private Case1 next;
    private CsePersonsWorkSet newCsePersonsWorkSet;
    private WorkArea selectActionWorkArea;
    private Common selectActionCommon;
    private CsePersonsWorkSet selectActionCsePersonsWorkSet;
    private SsnWorkArea newSsnWorkArea;
    private Common fromIapi;
    private NextTranInfo hidden;
    private Array<ExportGroup> export1;
    private InformationRequest fromInrdInformationRequest;
    private InterstateCase fromInterstateCase;
    private Common fromInrdCommon;
    private Common fromPar1;
    private PaReferral fromPaReferral;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePersonsWorkSet detail;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Common Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of SsnConcat.
    /// </summary>
    [JsonPropertyName("ssnConcat")]
    public TextWorkArea SsnConcat
    {
      get => ssnConcat ??= new();
      set => ssnConcat = value;
    }

    /// <summary>
    /// A value of SsnPart.
    /// </summary>
    [JsonPropertyName("ssnPart")]
    public Common SsnPart
    {
      get => ssnPart ??= new();
      set => ssnPart = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
    }

    /// <summary>
    /// A value of ApSex.
    /// </summary>
    [JsonPropertyName("apSex")]
    public CsePersonsWorkSet ApSex
    {
      get => apSex ??= new();
      set => apSex = value;
    }

    /// <summary>
    /// A value of ArFvIndicator.
    /// </summary>
    [JsonPropertyName("arFvIndicator")]
    public CsePerson ArFvIndicator
    {
      get => arFvIndicator ??= new();
      set => arFvIndicator = value;
    }

    /// <summary>
    /// A value of FvIndicator.
    /// </summary>
    [JsonPropertyName("fvIndicator")]
    public CsePerson FvIndicator
    {
      get => fvIndicator ??= new();
      set => fvIndicator = value;
    }

    /// <summary>
    /// A value of MaleApCsePerson.
    /// </summary>
    [JsonPropertyName("maleApCsePerson")]
    public CsePerson MaleApCsePerson
    {
      get => maleApCsePerson ??= new();
      set => maleApCsePerson = value;
    }

    /// <summary>
    /// A value of ActiveOtherMaleAp.
    /// </summary>
    [JsonPropertyName("activeOtherMaleAp")]
    public Common ActiveOtherMaleAp
    {
      get => activeOtherMaleAp ??= new();
      set => activeOtherMaleAp = value;
    }

    /// <summary>
    /// A value of MaleApCommon.
    /// </summary>
    [JsonPropertyName("maleApCommon")]
    public Common MaleApCommon
    {
      get => maleApCommon ??= new();
      set => maleApCommon = value;
    }

    /// <summary>
    /// A value of AllChildrenPaternityOff.
    /// </summary>
    [JsonPropertyName("allChildrenPaternityOff")]
    public Common AllChildrenPaternityOff
    {
      get => allChildrenPaternityOff ??= new();
      set => allChildrenPaternityOff = value;
    }

    /// <summary>
    /// A value of AllChildrenPaternityOn.
    /// </summary>
    [JsonPropertyName("allChildrenPaternityOn")]
    public Common AllChildrenPaternityOn
    {
      get => allChildrenPaternityOn ??= new();
      set => allChildrenPaternityOn = value;
    }

    /// <summary>
    /// A value of ChildrenPaternityEstInd.
    /// </summary>
    [JsonPropertyName("childrenPaternityEstInd")]
    public CsePerson ChildrenPaternityEstInd
    {
      get => childrenPaternityEstInd ??= new();
      set => childrenPaternityEstInd = value;
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
    /// A value of DupFirstTime.
    /// </summary>
    [JsonPropertyName("dupFirstTime")]
    public Common DupFirstTime
    {
      get => dupFirstTime ??= new();
      set => dupFirstTime = value;
    }

    /// <summary>
    /// A value of InitialDate.
    /// </summary>
    [JsonPropertyName("initialDate")]
    public DateWorkArea InitialDate
    {
      get => initialDate ??= new();
      set => initialDate = value;
    }

    /// <summary>
    /// A value of FuturePrograms.
    /// </summary>
    [JsonPropertyName("futurePrograms")]
    public Common FuturePrograms
    {
      get => futurePrograms ??= new();
      set => futurePrograms = value;
    }

    /// <summary>
    /// A value of ActivePrograms.
    /// </summary>
    [JsonPropertyName("activePrograms")]
    public Common ActivePrograms
    {
      get => activePrograms ??= new();
      set => activePrograms = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of CopyPersonProgram.
    /// </summary>
    [JsonPropertyName("copyPersonProgram")]
    public Common CopyPersonProgram
    {
      get => copyPersonProgram ??= new();
      set => copyPersonProgram = value;
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
    /// A value of ArAddress.
    /// </summary>
    [JsonPropertyName("arAddress")]
    public InformationRequest ArAddress
    {
      get => arAddress ??= new();
      set => arAddress = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public CsePersonAddress Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of Alias.
    /// </summary>
    [JsonPropertyName("alias")]
    public CsePersonsWorkSet Alias
    {
      get => alias ??= new();
      set => alias = value;
    }

    /// <summary>
    /// A value of ReferralProgram.
    /// </summary>
    [JsonPropertyName("referralProgram")]
    public Program ReferralProgram
    {
      get => referralProgram ??= new();
      set => referralProgram = value;
    }

    /// <summary>
    /// A value of ReferralPersonProgram.
    /// </summary>
    [JsonPropertyName("referralPersonProgram")]
    public PersonProgram ReferralPersonProgram
    {
      get => referralPersonProgram ??= new();
      set => referralPersonProgram = value;
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
    /// A value of DuplicateCase.
    /// </summary>
    [JsonPropertyName("duplicateCase")]
    public Common DuplicateCase
    {
      get => duplicateCase ??= new();
      set => duplicateCase = value;
    }

    /// <summary>
    /// A value of SetRelToAr.
    /// </summary>
    [JsonPropertyName("setRelToAr")]
    public Common SetRelToAr
    {
      get => setRelToAr ??= new();
      set => setRelToAr = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public Common Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of FatherCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fatherCsePersonsWorkSet")]
    public CsePersonsWorkSet FatherCsePersonsWorkSet
    {
      get => fatherCsePersonsWorkSet ??= new();
      set => fatherCsePersonsWorkSet = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CsePersonsWorkSet Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of ActiveChildOnCase.
    /// </summary>
    [JsonPropertyName("activeChildOnCase")]
    public Common ActiveChildOnCase
    {
      get => activeChildOnCase ??= new();
      set => activeChildOnCase = value;
    }

    /// <summary>
    /// A value of InactiveChildOnCase.
    /// </summary>
    [JsonPropertyName("inactiveChildOnCase")]
    public Common InactiveChildOnCase
    {
      get => inactiveChildOnCase ??= new();
      set => inactiveChildOnCase = value;
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
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public Case1 Case2
    {
      get => case2 ??= new();
      set => case2 = value;
    }

    /// <summary>
    /// A value of FemaleAp.
    /// </summary>
    [JsonPropertyName("femaleAp")]
    public Common FemaleAp
    {
      get => femaleAp ??= new();
      set => femaleAp = value;
    }

    /// <summary>
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of ChCommon.
    /// </summary>
    [JsonPropertyName("chCommon")]
    public Common ChCommon
    {
      get => chCommon ??= new();
      set => chCommon = value;
    }

    /// <summary>
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
    }

    /// <summary>
    /// A value of FatherCommon.
    /// </summary>
    [JsonPropertyName("fatherCommon")]
    public Common FatherCommon
    {
      get => fatherCommon ??= new();
      set => fatherCommon = value;
    }

    /// <summary>
    /// A value of Mother.
    /// </summary>
    [JsonPropertyName("mother")]
    public Common Mother
    {
      get => mother ??= new();
      set => mother = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public SsnWorkArea New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public Common Test
    {
      get => test ??= new();
      set => test = value;
    }

    /// <summary>
    /// A value of AddressType.
    /// </summary>
    [JsonPropertyName("addressType")]
    public Common AddressType
    {
      get => addressType ??= new();
      set => addressType = value;
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
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public AbendData Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    private Common case1;
    private TextWorkArea ssnConcat;
    private Common ssnPart;
    private DateWorkArea maximum;
    private Security2 security;
    private CsePersonsWorkSet apSex;
    private CsePerson arFvIndicator;
    private CsePerson fvIndicator;
    private CsePerson maleApCsePerson;
    private Common activeOtherMaleAp;
    private Common maleApCommon;
    private Common allChildrenPaternityOff;
    private Common allChildrenPaternityOn;
    private CsePerson childrenPaternityEstInd;
    private CsePerson childPaternityEstInd;
    private Common dupFirstTime;
    private DateWorkArea initialDate;
    private Common futurePrograms;
    private Common activePrograms;
    private CsePerson chCsePerson;
    private CsePerson arCsePerson;
    private CsePerson apCsePerson;
    private Common cse;
    private Common errOnAdabasUnavailable;
    private Common copyPersonProgram;
    private Common duplicateAddress;
    private InformationRequest arAddress;
    private CsePersonAddress ae;
    private CsePersonsWorkSet alias;
    private Program referralProgram;
    private PersonProgram referralPersonProgram;
    private DateWorkArea current;
    private Common duplicateCase;
    private Common setRelToAr;
    private Common child;
    private Common relToArIsCh;
    private CsePersonsWorkSet fatherCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet blank;
    private CsePerson csePerson;
    private AbendData abendData;
    private Common activeChildOnCase;
    private Common inactiveChildOnCase;
    private CaseRole caseRole;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Case1 case2;
    private Common femaleAp;
    private Common apCommon;
    private Common chCommon;
    private Common arCommon;
    private Common fatherCommon;
    private Common mother;
    private Common common;
    private SsnWorkArea new1;
    private Array<LocalGroup> local1;
    private Common test;
    private Common addressType;
    private PaReferral paReferral;
    private AbendData eab;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Case1 case1;
    private InformationRequest informationRequest;
    private PaReferralParticipant paReferralParticipant;
    private PaReferral paReferral;
    private CsePerson csePerson;
  }
#endregion
}
