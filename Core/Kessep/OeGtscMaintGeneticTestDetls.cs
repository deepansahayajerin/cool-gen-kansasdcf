// Program: OE_GTSC_MAINT_GENETIC_TEST_DETLS, ID: 371797126, model: 746.
// Short name: SWEGTSCP
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
/// A program: OE_GTSC_MAINT_GENETIC_TEST_DETLS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure step facilitates maintenance of GENETIC_TEST_DETAILS.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeGtscMaintGeneticTestDetls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_MAINT_GENETIC_TEST_DETLS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscMaintGeneticTestDetls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscMaintGeneticTestDetls.
  /// </summary>
  public OeGtscMaintGeneticTestDetls(IContext context, Import import,
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
    //                          M A I N T E N A N C E      L O G
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 12/28/94  Govinderaj			Initial Code
    // 02/13/96  T.O.Redmond			Retrofit
    // 06/17/96  Konkader			Print function.
    // 11/14/96  R. Marchman			Add new security and next tran.
    // 12/16/96  Sid Chowdhary			Add Events.
    // 01/21/97  Raju				NEXTTRAN code - HIST/MONA
    // 06/30/97  Sid				Display for closed cases.
    // 12/30/98  M Ramirez			Revised print process.
    // 12/30/98  M Ramirez			Changed security to check CRUD
    //                                 	
    // actions only.
    // 03/08/98  S. Johnson			Modified the display to wipe out
    // 					previous data when id # changed.
    // 05/17/99  PMcElderry			Added CSEnet logic
    // 07/09/99  R. Jean			Singleton reads changed to select only.
    // 01/03/00  Sree Veettil			If no pa_refer_part is found, create
    // 					history record without external alert.
    // 01/06/00  M Ramirez	83300		NEXT TRAN needs to be cleared before the
    // 					print process is invoked.
    // 03/30/00  Vithal MadhiraPR# 81845       The code is modified to allow 
    // user to select
    // 					an 'AP' and 'CHILD' ( Mother is optional) on
    // 					COMP  to do "Motherless Comparisons" genetic
    // 					tests on GTSC.
    // 11/17/00  M.Lachowicz	WR 298		Create header information for screens.
    // 11/27/00  P Phinney	I00106450	Check the Expiry Date to see if the Vendor
    // 					Address is Current
    // 04/10/01  Vithal	PR# 117676	User must flow back to GTSC by pressing PF9
    // 					(RETURN) on COMP after selecting AP/Mother/Child.
    // 					PF15 (GTSC)  on COMP will no longer be used
    // 					and will be deleted. The dialog flow from
    // 					GTSC to COMP is changed from TRANSFER to LINK.
    // 08/15/01  Madhu Kumar			PR corrected error message when trying to
    // 					reschedule dad and child.
    // 11/13/01  Madhu Kumar	PR# 128587	Error when printing ARTEST letter when 
    // AR is
    // 					an organization.
    // 12/13/01  T.Bobb 	PR00133863 	Fix for previous PR.
    // 05/10/17  GVandy	CQ48108		IV-D PEP Changes.
    // ---------------------------------------------------------------------------------------------
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This procedure step facilitates maintenance of Genetic Test Details.
    // PROCESSING:
    // Commands:
    // ENTER:
    // On flow from select cse persons for genetic test. It displays the details
    // of the cse persons selected and details of genetic test if already
    // scheduled.
    // LIST
    // Flows to List Vendor screen
    // SCHEDULE
    // The user enters the genetic test details. The system creates new GENETIC 
    // TEST and new PERSON GENETIC TEST records.
    // RCVCONFRM
    // The user enters confirmation of drawing of sample(s). The system updates 
    // PERSON GENETIC TEST record.
    // RCVRSLT
    // The user enters genetic test results. The system updates GENETIC TEST 
    // record.
    // ACTION BLOCKS:
    // 	OE_DISP_FATHER_MOTHER_COMB
    // 	OE_VALIDATE_MAINT_GENETIC_TEST
    // 	OE_SCHEDULE_GENETIC_TEST
    // 	OE_RECEIVE_GENETIC_TEST_CONFRM
    // 	OE_RECEIVE_GENETIC_TEST_RESULT
    // ENTITY TYPES USED
    // 	CSE_PERSON		- R - -
    // 	CASE			- R - -
    // 	CASE_ROLE		- R - -
    // 	VENDOR			- R - -
    // 	VENDOR_ADDRESS		- R - -
    // 	GENETIC_TEST		C R U -
    // 	PERSON_GENETIC_TEST	C R U -
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.OverrideExitstate.Flag = "N";
    UseSpDocSetLiterals();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      export.GeneticTestInformation.CaseNumber =
        import.GeneticTestInformation.CaseNumber;
      export.ServiceProvider.LastName = import.ServiceProvider.LastName;
      MoveOffice(import.Office, export.Office);

      return;
    }

    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    MoveOffice(import.Office, export.Office);
    export.GeneticTestInformation.Assign(import.GeneticTestInformation);
    export.HiddenGeneticTestInformation.Assign(
      import.HiddenGeneticTestInformation);
    export.HiddenForEvents.Assign(import.HiddenForEvents);
    export.Selected.Number = import.SelectedCase.Number;
    export.HiddenSelected.Assign(import.SelectedLegalAction);
    export.SelectedMother.Number = import.SelectedMother.Number;

    if (!Equal(global.Command, "RETLACS"))
    {
      if (IsEmpty(export.GeneticTestInformation.CourtOrderNo))
      {
        export.HiddenSelected.Identifier = 0;
        export.HiddenSelected.CourtCaseNumber = "";
      }
      else if (!Equal(export.GeneticTestInformation.CourtOrderNo,
        export.HiddenSelected.CourtCaseNumber))
      {
        export.HiddenSelected.Identifier = 0;
        export.HiddenSelected.CourtCaseNumber = "";
      }
    }

    export.HiddenListPrevSampCh.Flag = import.HiddenListPrevSampCh.Flag;
    export.HiddenListPrevSampMo.Flag = import.HiddenListPrevSampMo.Flag;
    export.HiddenListPrevSampFa.Flag = import.HiddenListPrevSampFa.Flag;
    export.HiddenGeneticTest.TestNumber = import.HiddenGeneticTest.TestNumber;
    export.ListLegalActionsLacs.PromptField =
      import.ListLegalActionsLacs.PromptField;
    export.ListGenTestAccount.PromptField =
      import.ListGenTestAccount.PromptField;
    export.ListGenTestType.PromptField = import.ListGenTestType.PromptField;
    export.ListDrawSiteFather.PromptField =
      import.ListDrawSiteFather.PromptField;
    export.ListDrawSiteMother.PromptField =
      import.ListDrawSiteMother.PromptField;
    export.ListDrawSiteChild.PromptField = import.ListDrawSiteChild.PromptField;
    export.ListPrevSampFather.PromptField =
      import.ListPrevSampFather.PromptField;
    export.ListPrevSampMother.PromptField =
      import.ListPrevSampMother.PromptField;
    export.ListPrevSampChild.PromptField = import.ListPrevSampChild.PromptField;
    export.ListTestSite.PromptField = import.ListTestSite.PromptField;
    export.GeneticTestType.Description = import.GeneticTestType.Description;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.CaseRoleInactive.Flag = import.CaseRoleInactive.Flag;
    export.ActiveAp.Flag = import.ActiveAp.Flag;
    export.ActiveChild.Flag = import.ActiveChild.Flag;
    export.HiddenComp.Flag = import.HiddenComp.Flag;

    // 11/17/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/17/00 M.L End
    // ----------------------------------------------------------
    // I00106450       11/27/2000            P Phinney
    // Check the Expiry Date to see if the Vendor Address is Current
    // ----------------------------------------------------------
    local.Current.Date = Now().Date;

    if (!Equal(export.GeneticTestInformation.CaseNumber, export.Selected.Number) &&
      !IsEmpty(export.Selected.Number))
    {
      local.CaseChange.Flag = "Y";
    }
    else
    {
      local.CaseChange.Flag = "";
    }

    if (IsEmpty(local.CaseChange.Flag))
    {
      for(import.HiddenSelCombn.Index = 0; import.HiddenSelCombn.Index < 3; ++
        import.HiddenSelCombn.Index)
      {
        if (!import.HiddenSelCombn.CheckSize())
        {
          break;
        }

        export.HiddenSelCombn.Index = import.HiddenSelCombn.Index;
        export.HiddenSelCombn.CheckSize();

        export.HiddenSelCombn.Update.DetailHiddenCombnCaseRole.Assign(
          import.HiddenSelCombn.Item.DetailHiddenCombnCaseRole);
        export.HiddenSelCombn.Update.DetailHiddenCombnCsePerson.Number =
          import.HiddenSelCombn.Item.DetailHiddenCombnCsePerson.Number;
      }

      import.HiddenSelCombn.CheckIndex();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (AsChar(export.GeneticTestInformation.FatherPrevSampExistsInd) == 'N')
    {
      var field = GetField(export.ListPrevSampFather, "promptField");

      field.Color = "cyan";
      field.Protected = true;
    }
    else
    {
      var field = GetField(export.ListPrevSampFather, "promptField");

      field.Protected = false;
    }

    if (AsChar(export.GeneticTestInformation.MotherPrevSampExistsInd) == 'N')
    {
      var field = GetField(export.ListPrevSampMother, "promptField");

      field.Color = "cyan";
      field.Protected = true;
    }
    else
    {
      var field = GetField(export.ListPrevSampMother, "promptField");

      field.Protected = false;
    }

    if (AsChar(export.GeneticTestInformation.ChildPrevSampExistsInd) == 'N')
    {
      var field = GetField(export.ListPrevSampChild, "promptField");

      field.Color = "cyan";
      field.Protected = true;
    }
    else
    {
      var field = GetField(export.ListPrevSampChild, "promptField");

      field.Protected = false;
    }

    if (Equal(global.Command, "RETURN"))
    {
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
          ;
      }
      else
      {
        ExitState = "ACO_NE0000_RETURN";

        return;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.GeneticTestInformation.CaseNumber =
        export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);

      // ---------------------------------------------
      // Start of Code (Raju 01/20/97:1345 hrs CST)
      // ---------------------------------------------
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        local.LastTran.SystemGeneratedIdentifier =
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.Selected.Number = local.LastTran.CaseNumber ?? Spaces(10);
        export.GeneticTestInformation.CaseNumber =
          local.LastTran.CaseNumber ?? Spaces(10);
        export.HiddenGeneticTest.TestNumber =
          (int)local.LastTran.DenormNumeric12.GetValueOrDefault();
        local.Case1.Number = local.LastTran.CaseNumber ?? Spaces(10);
        local.CaseUnit.CuNumber =
          local.LastTran.CaseUnitNumber.GetValueOrDefault();
        UseOeGtscGetCseNumber();
      }

      // ---------------------------------------------
      // End  of Code
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      export.GeneticTestInformation.CaseNumber = export.Selected.Number;
      global.Command = "DISPLAY";
    }

    if (!IsEmpty(export.Selected.Number) && !
      Equal(export.Selected.Number, export.GeneticTestInformation.CaseNumber))
    {
      // --------------------------------------
      // If the
      // case number has changed, these
      // views
      // will wipe out the other data
      // displayed
      // on the screen.
      // 
      // --------------------------------------
      export.GeneticTestInformation.Assign(local.ResetValues);
      export.GeneticTestInformation.CaseNumber =
        import.GeneticTestInformation.CaseNumber;
      MoveGeneticTestInformation7(local.Initialized,
        export.HiddenGeneticTestInformation);
      MoveGeneticTestInformation7(local.Initialized, export.HiddenForEvents);

      export.HiddenGeneticTest.TestNumber = 0;
      export.GeneticTestType.Description =
        Spaces(CodeValue.Description_MaxLength);
      export.CaseOpen.Flag = "";
    }

    if (!IsEmpty(export.GeneticTestInformation.CaseNumber))
    {
      local.ZeroFill.Text10 = export.GeneticTestInformation.CaseNumber;
      UseEabPadLeftWithZeros();
      export.GeneticTestInformation.CaseNumber = local.ZeroFill.Text10;
      export.Selected.Number = export.GeneticTestInformation.CaseNumber;
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *If the next tran info is not equal to spaces, *
      // *this implies the user requested a next tran   *
      // *action. Now validate that action.             *
      // ************************************************
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.HiddenNextTranInfo.CaseNumber =
        export.GeneticTestInformation.CaseNumber;
      UseScCabNextTranPut1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETGTAL") && !
      Equal(global.Command, "RETGTSL") && !Equal(global.Command, "RETVENL") && !
      Equal(global.Command, "RETCDVL") && !Equal(global.Command, "RETLACS") && !
      Equal(global.Command, "RETCOMP"))
    {
      export.ListLegalActionsLacs.PromptField = "";
      export.ListGenTestAccount.PromptField = "";
      export.ListGenTestType.PromptField = "";
      export.ListDrawSiteFather.PromptField = "";
      export.ListDrawSiteMother.PromptField = "";
      export.ListDrawSiteChild.PromptField = "";
      export.ListPrevSampFather.PromptField = "";
      export.ListPrevSampMother.PromptField = "";
      export.ListPrevSampChild.PromptField = "";
      export.ListTestSite.PromptField = "";
    }

    if (Equal(global.Command, "CCOMP"))
    {
      export.Selected.Number = export.GeneticTestInformation.CaseNumber;
      export.HiddenComp.Flag = "Y";
      ExitState = "ECO_XFR_TO_COMP_CASE_COMPOSITN";

      return;
    }

    if (Equal(global.Command, "FLWCOMP"))
    {
      export.GeneticTestInformation.CaseNumber = export.Selected.Number;

      for(export.HiddenSelCombn.Index = 0; export.HiddenSelCombn.Index < 3; ++
        export.HiddenSelCombn.Index)
      {
        if (!export.HiddenSelCombn.CheckSize())
        {
          break;
        }

        switch(export.HiddenSelCombn.Index + 1)
        {
          case 1:
            export.HiddenSelCombn.Update.DetailHiddenCombnCaseRole.Type1 = "AP";
            export.HiddenSelCombn.Update.DetailHiddenCombnCsePerson.Number =
              import.SelectedAllegedFather.Number;

            break;
          case 2:
            // -----------------------------------------------------------------
            // PR# 81845 :  Mother is optional for conducting genetic test. In 
            // some cases user may select only an 'AP' and a 'CHILD' on COMP .
            //                                          
            // --- Vithal (03/30/2000)
            // ------------------------------------------------------------------
            export.HiddenSelCombn.Update.DetailHiddenCombnCaseRole.Type1 = "MO";
            export.HiddenSelCombn.Update.DetailHiddenCombnCsePerson.Number =
              import.SelectedMother.Number;

            if (IsEmpty(import.SelectedMother.Number))
            {
              continue;
            }

            break;
          case 3:
            export.HiddenSelCombn.Update.DetailHiddenCombnCaseRole.Type1 = "CH";
            export.HiddenSelCombn.Update.DetailHiddenCombnCsePerson.Number =
              import.SelectedChild.Number;

            break;
          default:
            break;
        }

        UseOeGtscGetLatestCaseRole();
      }

      export.HiddenSelCombn.CheckIndex();
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    // If the court order no is supplied and SCHEDULE or UPDATE key is pressed: 
    // check if the legal action for the court order. If there is only one legal
    // action record, use that; if there is more than one, link to LACN screen
    // to allow selection of a legal action for the court case no.
    // ---------------------------------------------
    if (Equal(global.Command, "SCHEDULE") || Equal(global.Command, "UPDATE"))
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";

        return;
      }

      if (AsChar(export.CaseRoleInactive.Flag) == 'Y')
      {
        ExitState = "CANNOT_MODIFY_INACTIVE_CASE_ROLE";

        return;
      }

      if (!IsEmpty(export.GeneticTestInformation.CourtOrderNo))
      {
        if (export.HiddenSelected.Identifier == 0)
        {
          local.NoOfLegalActions.Count = 0;

          foreach(var item in ReadLegalAction())
          {
            ++local.NoOfLegalActions.Count;

            if (local.NoOfLegalActions.Count > 1)
            {
              break;
            }
          }

          if (local.NoOfLegalActions.Count == 1)
          {
            export.HiddenSelected.Assign(entities.ExistingPatEstabOrder);
          }
          else if (local.NoOfLegalActions.Count > 1)
          {
            export.HiddenSelected.Classification = "O";
            export.HiddenSelected.CourtCaseNumber =
              export.GeneticTestInformation.CourtOrderNo;
            ExitState = "ECO_LNK_TO_LIST_LEG_ACT_BY_CC";

            return;
          }
        }
      }
    }

    // mjr
    // ----------------------------------------------
    // 12/30/1998
    // Changed security to check CRUD actions only
    // -----------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "PRINT") || Equal(global.Command, "SCHEDULE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "PRINT":
        if (IsEmpty(export.GeneticTestInformation.GeneticTestAccountNo) || !
          Equal(export.GeneticTestInformation.GeneticTestAccountNo,
          export.HiddenGeneticTestInformation.GeneticTestAccountNo) || !
          Equal(export.HiddenGeneticTestInformation.ChildDrawSiteId,
          export.GeneticTestInformation.ChildDrawSiteId) || !
          Equal(export.HiddenGeneticTestInformation.ChildSchedTestDate,
          export.GeneticTestInformation.ChildSchedTestDate) || !
          Equal(export.HiddenGeneticTestInformation.ChildSchedTestTime,
          export.GeneticTestInformation.ChildSchedTestTime) || !
          Equal(export.HiddenGeneticTestInformation.CourtOrderNo,
          export.GeneticTestInformation.CourtOrderNo) || !
          Equal(export.HiddenGeneticTestInformation.FatherDrawSiteId,
          export.GeneticTestInformation.FatherDrawSiteId) || !
          Equal(export.HiddenGeneticTestInformation.FatherSchedTestDate,
          export.GeneticTestInformation.FatherSchedTestDate) || !
          Equal(export.HiddenGeneticTestInformation.FatherSchedTestTime,
          export.GeneticTestInformation.FatherSchedTestTime) || !
          Equal(export.HiddenGeneticTestInformation.MotherDrawSiteId,
          export.GeneticTestInformation.MotherDrawSiteId) || !
          Equal(export.HiddenGeneticTestInformation.MotherSchedTestDate,
          export.GeneticTestInformation.MotherSchedTestDate) || !
          Equal(export.HiddenGeneticTestInformation.MotherSchedTestTime,
          export.GeneticTestInformation.MotherSchedTestTime) || !
          Equal(export.HiddenGeneticTestInformation.TestSiteVendorId,
          export.GeneticTestInformation.TestSiteVendorId))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";
        }
        else
        {
          export.DocmProtectFilter.Flag = "Y";
          export.Print.Type1 = "GTSC";
          ExitState = "ECO_LNK_TO_DOCUMENT_MAINT";
        }

        return;
      case "RETDOCM":
        if (IsEmpty(import.Print.Name))
        {
          ExitState = "SP0000_NO_DOC_SEL_FOR_PRINT";

          return;
        }

        // *************************************
        //    mxk  Begin code changes for PR # 128587
        // *************************************
        // 12/13/01 T.Bobb PR00133863 Change the compare to only compare the 1st
        //  two characters of document name.
        // **********************************************************************
        if (Equal(import.Print.Name, 1, 2, "AR"))
        {
          if (ReadCsePerson1())
          {
            if (AsChar(entities.CsePerson.Type1) == 'O')
            {
              ExitState = "ACO_0000_NO_AR_LTR_TO_ORG";

              return;
            }
          }

          // *************************************
          //    mxk  End  code changes for PR # 128587
          // *************************************
        }

        // mjr
        // ------------------------------------------
        // 01/06/2000
        // NEXT TRAN needs to be cleared before the print process is invoked.
        // -------------------------------------------------------
        export.HiddenNextTranInfo.Assign(local.NullNextTranInfo);
        export.Standard.NextTransaction = "DKEY";
        export.HiddenNextTranInfo.MiscText2 =
          TrimEnd(local.SpDocLiteral.IdDocument) + import.Print.Name;

        // mjr
        // ----------------------------------------------------
        // Place identifiers into next tran
        // -------------------------------------------------------
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(local.SpDocLiteral.IdGenetic) + NumberToString
          (export.HiddenGeneticTest.TestNumber, 8, 8);
        UseScCabNextTranPut2();

        // mjr---> DKEY's trancode = SRPD
        //  Can change this to do a READ instead of hardcoding
        global.NextTran = "SRPD PRINT";

        return;
      case "PRINTRET":
        // mjr
        // ---------------------------------------------------------
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done
        // now.
        // ------------------------------------------------------------
        UseScCabNextTranGet();

        // mjr
        // ----------------------------------------------------
        // Extract identifiers from next tran
        // -------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText1,
          NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdGenetic));

        if (local.Position.Count <= 0)
        {
          break;
        }

        local.BatchConvertNumToText.Number15 =
          StringToNumber(Substring(
            export.HiddenNextTranInfo.MiscText1, 50, local.Position.Count +
          8, 8));
        export.HiddenGeneticTest.TestNumber =
          (int)local.BatchConvertNumToText.Number15;

        if (ReadCase())
        {
          export.Selected.Number = entities.Case1.Number;
        }
        else
        {
          break;
        }

        if (ReadCaseRole2())
        {
          export.HiddenSelCombn.Index = 0;
          export.HiddenSelCombn.CheckSize();

          export.HiddenSelCombn.Update.DetailHiddenCombnCaseRole.Assign(
            entities.CaseRole);

          if (ReadCsePerson2())
          {
            export.HiddenSelCombn.Update.DetailHiddenCombnCsePerson.Number =
              entities.CsePerson.Number;
          }
          else
          {
            break;
          }
        }
        else
        {
          break;
        }

        if (ReadCaseRole3())
        {
          ++export.HiddenSelCombn.Index;
          export.HiddenSelCombn.CheckSize();

          export.HiddenSelCombn.Update.DetailHiddenCombnCaseRole.Assign(
            entities.CaseRole);

          if (ReadCsePerson2())
          {
            export.HiddenSelCombn.Update.DetailHiddenCombnCsePerson.Number =
              entities.CsePerson.Number;
          }
          else
          {
            break;
          }
        }
        else
        {
          break;
        }

        if (ReadCaseRole1())
        {
          ++export.HiddenSelCombn.Index;
          export.HiddenSelCombn.CheckSize();

          export.HiddenSelCombn.Update.DetailHiddenCombnCaseRole.Assign(
            entities.CaseRole);

          if (ReadCsePerson2())
          {
            export.HiddenSelCombn.Update.DetailHiddenCombnCsePerson.Number =
              entities.CsePerson.Number;
          }
          else
          {
            break;
          }
        }
        else
        {
          break;
        }

        global.Command = "DISPLAY";

        break;
      case "EXIT":
        if (IsEmpty(export.Selected.Number))
        {
          export.Selected.Number = export.GeneticTestInformation.CaseNumber;
        }

        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "DISPLAY":
        break;
      case "LIST":
        if (!IsEmpty(export.ListLegalActionsLacs.PromptField) && AsChar
          (export.ListLegalActionsLacs.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListLegalActionsLacs, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListGenTestAccount.PromptField) && AsChar
          (export.ListGenTestAccount.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListGenTestAccount, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListGenTestType.PromptField) && AsChar
          (export.ListGenTestType.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListGenTestType, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListDrawSiteFather.PromptField) && AsChar
          (export.ListDrawSiteFather.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListDrawSiteFather, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListPrevSampFather.PromptField) && AsChar
          (export.ListPrevSampFather.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListPrevSampFather, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListDrawSiteMother.PromptField) && AsChar
          (export.ListDrawSiteMother.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListDrawSiteMother, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListPrevSampMother.PromptField) && AsChar
          (export.ListPrevSampMother.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListPrevSampMother, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListDrawSiteChild.PromptField) && AsChar
          (export.ListDrawSiteChild.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListDrawSiteChild, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListPrevSampChild.PromptField) && AsChar
          (export.ListPrevSampChild.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListPrevSampChild, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(export.ListTestSite.PromptField) && AsChar
          (export.ListTestSite.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListTestSite, "promptField");

          field.Error = true;
        }

        if (IsEmpty(export.ListGenTestAccount.PromptField) && IsEmpty
          (export.ListLegalActionsLacs.PromptField) && IsEmpty
          (export.ListDrawSiteFather.PromptField) && IsEmpty
          (export.ListPrevSampFather.PromptField) && IsEmpty
          (export.ListDrawSiteMother.PromptField) && IsEmpty
          (export.ListPrevSampMother.PromptField) && IsEmpty
          (export.ListDrawSiteChild.PromptField) && IsEmpty
          (export.ListPrevSampChild.PromptField) && IsEmpty
          (export.ListTestSite.PromptField) && IsEmpty
          (export.ListGenTestType.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field3 = GetField(export.ListLegalActionsLacs, "promptField");

          field3.Error = true;

          var field4 = GetField(export.ListGenTestAccount, "promptField");

          field4.Error = true;

          var field5 = GetField(export.ListGenTestType, "promptField");

          field5.Error = true;

          var field6 = GetField(export.ListDrawSiteFather, "promptField");

          field6.Error = true;

          var field7 = GetField(export.ListDrawSiteMother, "promptField");

          field7.Error = true;

          var field8 = GetField(export.ListDrawSiteChild, "promptField");

          field8.Error = true;

          var field9 = GetField(export.ListPrevSampFather, "promptField");

          field9.Error = true;

          var field10 = GetField(export.ListPrevSampMother, "promptField");

          field10.Error = true;

          var field11 = GetField(export.ListPrevSampChild, "promptField");

          field11.Error = true;

          var field12 = GetField(export.ListTestSite, "promptField");

          field12.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.ListGenTestAccount.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_GTAL_GEN_TEST_AC_LIST";

          return;
        }

        if (AsChar(export.ListLegalActionsLacs.PromptField) == 'S')
        {
          if (IsEmpty(export.GeneticTestInformation.CaseNumber))
          {
            var field = GetField(export.GeneticTestInformation, "caseNumber");

            field.Error = true;

            ExitState = "OE0000_CASE_NO_REQUIRED_FOR_LACS";

            return;
          }

          export.Selected.Number = export.GeneticTestInformation.CaseNumber;
          ExitState = "ECO_LNK_TO_LIST_LEG_ACT_BY_CC";

          return;
        }

        if (AsChar(export.ListGenTestType.PromptField) == 'S')
        {
          export.SelectedForList.CodeName = "GENETIC TEST TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.ListDrawSiteFather.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_VENDOR";

          return;
        }

        if (AsChar(export.ListPrevSampFather.PromptField) == 'S')
        {
          export.SelectedForPrevSample.Number =
            export.GeneticTestInformation.FatherPersonNo;
          ExitState = "ECO_LNK_2_GTSL_GEN_TEST_SAMP_LST";

          return;
        }

        if (AsChar(export.ListDrawSiteMother.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_VENDOR";

          return;
        }

        if (AsChar(export.ListPrevSampMother.PromptField) == 'S')
        {
          export.SelectedForPrevSample.Number =
            export.GeneticTestInformation.MotherPersonNo;
          ExitState = "ECO_LNK_2_GTSL_GEN_TEST_SAMP_LST";

          return;
        }

        if (AsChar(export.ListDrawSiteChild.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_VENDOR";

          return;
        }

        if (AsChar(export.ListPrevSampChild.PromptField) == 'S')
        {
          export.SelectedForPrevSample.Number =
            export.GeneticTestInformation.ChildPersonNo;
          ExitState = "ECO_LNK_2_GTSL_GEN_TEST_SAMP_LST";

          return;
        }

        if (AsChar(export.ListTestSite.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_VENDOR";

          return;
        }

        break;
      case "RETLACS":
        export.ListLegalActionsLacs.PromptField = "";

        if (IsEmpty(import.SelectedLegalAction.CourtCaseNumber))
        {
        }
        else
        {
          export.GeneticTestInformation.CourtOrderNo =
            import.SelectedLegalAction.CourtCaseNumber ?? Spaces(17);
        }

        var field1 = GetField(export.GeneticTestInformation, "testType");

        field1.Protected = false;
        field1.Focused = true;

        break;
      case "RETCDVL":
        if (AsChar(export.ListGenTestType.PromptField) == 'S')
        {
          export.ListGenTestType.PromptField = "";

          if (IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            var field = GetField(export.GeneticTestInformation, "testType");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.GeneticTestInformation.TestType =
              import.DlgflwSelected.Cdvalue;
            export.GeneticTestType.Description =
              import.DlgflwSelected.Description;

            var field =
              GetField(export.GeneticTestInformation, "fatherDrawSiteId");

            field.Protected = false;
            field.Focused = true;
          }
        }

        break;
      case "RETGTAL":
        export.ListGenTestAccount.PromptField = "";

        if (IsEmpty(import.SelectedGeneticTestAccount.AccountNumber))
        {
        }
        else
        {
          export.GeneticTestInformation.GeneticTestAccountNo =
            import.SelectedGeneticTestAccount.AccountNumber;
        }

        var field2 = GetField(export.ListLegalActionsLacs, "promptField");

        field2.Protected = false;
        field2.Focused = true;

        break;
      case "RETGTSL":
        if (AsChar(export.ListPrevSampFather.PromptField) == 'S')
        {
          export.ListPrevSampFather.PromptField = "";

          var field =
            GetField(export.GeneticTestInformation, "motherDrawSiteId");

          field.Protected = false;
          field.Focused = true;

          if (import.SelectedPrevSampleGeneticTest.TestNumber == 0 || import
            .SelectedPrevSamplePersonGeneticTest.Identifier == 0)
          {
            // ---------------------------------------------
            // Returned without selecting any sample. Blank out corresponding 
            // screen fields.
            // ---------------------------------------------
            export.GeneticTestInformation.FatherCollectSampleInd = "Y";
            export.GeneticTestInformation.FatherReuseSampleInd = "N";
            export.GeneticTestInformation.FatherPrevSampGtestNumber = 0;
            export.GeneticTestInformation.FatherPrevSampPerGenTestId = 0;
            export.GeneticTestInformation.FatherPrevSampTestType = "";
            export.GeneticTestInformation.FatherPrevSampleLabCaseNo = "";
            export.GeneticTestInformation.FatherPrevSampSpecimenId = "";

            break;
          }

          // ---------------------------------------------
          // ELSE move selected sample details for reuse
          // ---------------------------------------------
          export.GeneticTestInformation.FatherCollectSampleInd = "N";
          export.GeneticTestInformation.FatherReuseSampleInd = "Y";
          export.GeneticTestInformation.FatherSampleCollectedInd = "N";
          export.GeneticTestInformation.FatherShowInd = "";
          export.GeneticTestInformation.FatherSpecimenId = "";
          export.GeneticTestInformation.FatherDrawSiteId =
            NumberToString(import.SelectedPrevSampleVendor.Identifier, 8, 8);
          export.GeneticTestInformation.FatherDrawSiteVendorName =
            import.SelectedPrevSampleVendor.Name;
          export.GeneticTestInformation.FatherDrawSiteCity =
            import.SelectedPrevSampleVendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.FatherDrawSiteState =
            import.SelectedPrevSampleVendorAddress.State ?? Spaces(2);
          export.GeneticTestInformation.FatherPrevSampGtestNumber =
            import.SelectedPrevSampleGeneticTest.TestNumber;
          export.GeneticTestInformation.FatherPrevSampPerGenTestId =
            import.SelectedPrevSamplePersonGeneticTest.Identifier;
          export.GeneticTestInformation.FatherPrevSampTestType =
            import.SelectedPrevSampleGeneticTest.TestType ?? Spaces(2);
          export.GeneticTestInformation.FatherPrevSampleLabCaseNo =
            import.SelectedPrevSampleGeneticTest.LabCaseNo ?? Spaces(11);
          export.GeneticTestInformation.FatherPrevSampSpecimenId =
            import.SelectedPrevSamplePersonGeneticTest.SpecimenId ?? Spaces
            (10);
          export.GeneticTestInformation.FatherSchedTestDate =
            import.SelectedPrevSamplePersonGeneticTest.ScheduledTestDate;

          if (import.SelectedPrevSamplePersonGeneticTest.ScheduledTestTime.
            GetValueOrDefault() == TimeSpan.Zero)
          {
            export.GeneticTestInformation.FatherSchedTestTime = "";
          }
          else
          {
            local.WorkTime.TimeWithAmPm = "";
            local.WorkTime.Wtime =
              import.SelectedPrevSamplePersonGeneticTest.ScheduledTestTime.
                GetValueOrDefault();
            UseCabConvertTimeFormat();
            export.GeneticTestInformation.FatherSchedTestTime =
              local.WorkTime.TimeWithAmPm;
          }

          break;
        }

        if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
        {
          if (AsChar(export.ListPrevSampMother.PromptField) == 'S')
          {
            export.ListPrevSampMother.PromptField = "";

            var field =
              GetField(export.GeneticTestInformation, "childDrawSiteId");

            field.Protected = false;
            field.Focused = true;

            if (import.SelectedPrevSampleGeneticTest.TestNumber == 0 || import
              .SelectedPrevSamplePersonGeneticTest.Identifier == 0)
            {
              // ---------------------------------------------
              // Returned without selecting any sample. Blank out corresponding 
              // screen fields.
              // ---------------------------------------------
              export.GeneticTestInformation.MotherCollectSampleInd = "Y";
              export.GeneticTestInformation.MotherReuseSampleInd = "N";
              export.GeneticTestInformation.MotherPrevSampGtestNumber = 0;
              export.GeneticTestInformation.MotherPrevSampPerGenTestId = 0;
              export.GeneticTestInformation.MotherPrevSampTestType = "";
              export.GeneticTestInformation.MotherPrevSampLabCaseNo = "";
              export.GeneticTestInformation.MotherPrevSampSpecimenId = "";

              break;
            }

            // -----
            // ELSE
            // -----
            export.GeneticTestInformation.MotherCollectSampleInd = "N";
            export.GeneticTestInformation.MotherReuseSampleInd = "Y";
            export.GeneticTestInformation.MotherSampleCollectedInd = "N";
            export.GeneticTestInformation.MotherShowInd = "";
            export.GeneticTestInformation.MotherSpecimenId = "";
            export.GeneticTestInformation.MotherDrawSiteId =
              NumberToString(import.SelectedPrevSampleVendor.Identifier, 8, 8);
            export.GeneticTestInformation.MotherDrawSiteVendorName =
              import.SelectedPrevSampleVendor.Name;
            export.GeneticTestInformation.MotherDrawSiteCity =
              import.SelectedPrevSampleVendorAddress.City ?? Spaces(15);
            export.GeneticTestInformation.MotherDrawSiteState =
              import.SelectedPrevSampleVendorAddress.State ?? Spaces(2);
            export.GeneticTestInformation.MotherPrevSampGtestNumber =
              import.SelectedPrevSampleGeneticTest.TestNumber;
            export.GeneticTestInformation.MotherPrevSampPerGenTestId =
              import.SelectedPrevSamplePersonGeneticTest.Identifier;
            export.GeneticTestInformation.MotherPrevSampTestType =
              import.SelectedPrevSampleGeneticTest.TestType ?? Spaces(2);
            export.GeneticTestInformation.MotherPrevSampLabCaseNo =
              import.SelectedPrevSampleGeneticTest.LabCaseNo ?? Spaces(11);
            export.GeneticTestInformation.MotherPrevSampSpecimenId =
              import.SelectedPrevSamplePersonGeneticTest.SpecimenId ?? Spaces
              (10);
            export.GeneticTestInformation.MotherSchedTestDate =
              import.SelectedPrevSamplePersonGeneticTest.ScheduledTestDate;

            if (import.SelectedPrevSamplePersonGeneticTest.ScheduledTestTime.
              GetValueOrDefault() == TimeSpan.Zero)
            {
              export.GeneticTestInformation.MotherSchedTestTime = "";
            }
            else
            {
              local.WorkTime.TimeWithAmPm = "";
              local.WorkTime.Wtime =
                import.SelectedPrevSamplePersonGeneticTest.ScheduledTestTime.
                  GetValueOrDefault();
              UseCabConvertTimeFormat();
              export.GeneticTestInformation.MotherSchedTestTime =
                local.WorkTime.TimeWithAmPm;
            }

            break;
          }
        }

        if (AsChar(export.ListPrevSampChild.PromptField) == 'S')
        {
          export.ListPrevSampChild.PromptField = "";

          var field =
            GetField(export.GeneticTestInformation, "testSiteVendorId");

          field.Protected = false;
          field.Focused = true;

          if (import.SelectedPrevSampleGeneticTest.TestNumber == 0 || import
            .SelectedPrevSamplePersonGeneticTest.Identifier == 0)
          {
            // ---------------------------------------------
            // Returned without selecting any sample. Blank out corresponding 
            // screen fields.
            // ---------------------------------------------
            export.GeneticTestInformation.ChildCollectSampleInd = "Y";
            export.GeneticTestInformation.ChildReuseSampleInd = "N";
            export.GeneticTestInformation.ChildPrevSampGtestNumber = 0;
            export.GeneticTestInformation.ChildPrevSampPerGenTestId = 0;
            export.GeneticTestInformation.ChildPrevSampTestType = "";
            export.GeneticTestInformation.ChildPrevSampLabCaseNo = "";
            export.GeneticTestInformation.ChildPrevSampSpecimenId = "";

            break;
          }

          export.GeneticTestInformation.ChildCollectSampleInd = "N";
          export.GeneticTestInformation.ChildReuseSampleInd = "Y";
          export.GeneticTestInformation.ChildSampleCollectedInd = "N";
          export.GeneticTestInformation.ChildShowInd = "";
          export.GeneticTestInformation.ChildSpecimenId = "";
          export.GeneticTestInformation.ChildDrawSiteId =
            NumberToString(import.SelectedPrevSampleVendor.Identifier, 8, 8);
          export.GeneticTestInformation.ChildDrawSiteVendorName =
            import.SelectedPrevSampleVendor.Name;
          export.GeneticTestInformation.ChildDrawSiteCity =
            import.SelectedPrevSampleVendorAddress.City ?? Spaces(15);
          export.GeneticTestInformation.ChildDrawSiteState =
            import.SelectedPrevSampleVendorAddress.State ?? Spaces(2);
          export.GeneticTestInformation.ChildPrevSampGtestNumber =
            import.SelectedPrevSampleGeneticTest.TestNumber;
          export.GeneticTestInformation.ChildPrevSampPerGenTestId =
            import.SelectedPrevSamplePersonGeneticTest.Identifier;
          export.GeneticTestInformation.ChildPrevSampTestType =
            import.SelectedPrevSampleGeneticTest.TestType ?? Spaces(2);
          export.GeneticTestInformation.ChildPrevSampLabCaseNo =
            import.SelectedPrevSampleGeneticTest.LabCaseNo ?? Spaces(11);
          export.GeneticTestInformation.ChildPrevSampSpecimenId =
            import.SelectedPrevSamplePersonGeneticTest.SpecimenId ?? Spaces
            (10);
          export.GeneticTestInformation.ChildSchedTestDate =
            import.SelectedPrevSamplePersonGeneticTest.ScheduledTestDate;

          if (import.SelectedPrevSamplePersonGeneticTest.ScheduledTestTime.
            GetValueOrDefault() == TimeSpan.Zero)
          {
            export.GeneticTestInformation.ChildSchedTestTime = "";
          }
          else
          {
            local.WorkTime.TimeWithAmPm = "";
            local.WorkTime.Wtime =
              import.SelectedPrevSamplePersonGeneticTest.ScheduledTestTime.
                GetValueOrDefault();
            UseCabConvertTimeFormat();
            export.GeneticTestInformation.ChildSchedTestTime =
              local.WorkTime.TimeWithAmPm;
          }
        }

        break;
      case "RETVENL":
        // ---------------------------------------------
        // Returned from Select Vendor.
        // ---------------------------------------------
        if (AsChar(export.ListDrawSiteFather.PromptField) == 'S')
        {
          export.ListDrawSiteFather.PromptField = "";

          if (import.SelectedVendor.Identifier == 0)
          {
            var field =
              GetField(export.GeneticTestInformation, "fatherDrawSiteId");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field =
              GetField(export.GeneticTestInformation, "fatherSchedTestDate");

            field.Protected = false;
            field.Focused = true;

            export.GeneticTestInformation.FatherDrawSiteId =
              NumberToString(import.SelectedVendor.Identifier, 8, 8);
            export.GeneticTestInformation.FatherDrawSiteVendorName =
              import.SelectedVendor.Name;
            UseOeCabGetVendorAddress();
            export.GeneticTestInformation.FatherDrawSiteCity =
              local.Selected.City ?? Spaces(15);
            export.GeneticTestInformation.FatherDrawSiteState =
              local.Selected.State ?? Spaces(2);
          }

          return;
        }

        if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
        {
          if (AsChar(export.ListDrawSiteMother.PromptField) == 'S')
          {
            export.ListDrawSiteMother.PromptField = "";

            if (import.SelectedVendor.Identifier == 0)
            {
              var field =
                GetField(export.GeneticTestInformation, "motherDrawSiteId");

              field.Protected = false;
              field.Focused = true;
            }
            else
            {
              var field =
                GetField(export.GeneticTestInformation, "motherSchedTestDate");

              field.Protected = false;
              field.Focused = true;

              export.GeneticTestInformation.MotherDrawSiteId =
                NumberToString(import.SelectedVendor.Identifier, 8, 8);
              export.GeneticTestInformation.MotherDrawSiteVendorName =
                import.SelectedVendor.Name;
              UseOeCabGetVendorAddress();
              export.GeneticTestInformation.MotherDrawSiteCity =
                local.Selected.City ?? Spaces(15);
              export.GeneticTestInformation.MotherDrawSiteState =
                local.Selected.State ?? Spaces(2);
            }

            return;
          }
        }

        if (AsChar(export.ListDrawSiteChild.PromptField) == 'S')
        {
          export.ListDrawSiteChild.PromptField = "";

          if (import.SelectedVendor.Identifier == 0)
          {
            var field =
              GetField(export.GeneticTestInformation, "childDrawSiteId");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field =
              GetField(export.GeneticTestInformation, "childSchedTestDate");

            field.Protected = false;
            field.Focused = true;

            export.GeneticTestInformation.ChildDrawSiteId =
              NumberToString(import.SelectedVendor.Identifier, 8, 8);
            export.GeneticTestInformation.ChildDrawSiteVendorName =
              import.SelectedVendor.Name;
            UseOeCabGetVendorAddress();
            export.GeneticTestInformation.ChildDrawSiteCity =
              local.Selected.City ?? Spaces(15);
            export.GeneticTestInformation.ChildDrawSiteState =
              local.Selected.State ?? Spaces(2);
          }

          return;
        }

        if (AsChar(export.ListTestSite.PromptField) == 'S')
        {
          export.ListTestSite.PromptField = "";

          if (import.SelectedVendor.Identifier == 0)
          {
            var field =
              GetField(export.GeneticTestInformation, "testSiteVendorId");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.GeneticTestInformation, "labCaseNo");

            field.Protected = false;
            field.Focused = true;

            export.GeneticTestInformation.TestSiteVendorId =
              NumberToString(import.SelectedVendor.Identifier, 8, 8);
            export.GeneticTestInformation.TestSiteVendorName =
              import.SelectedVendor.Name;
            UseOeCabGetVendorAddress();
            export.GeneticTestInformation.TestSiteCity =
              local.Selected.City ?? Spaces(15);
            export.GeneticTestInformation.TestSiteState =
              local.Selected.State ?? Spaces(2);
          }

          return;
        }

        break;
      case "SCHEDULE":
        local.Standard.Command = global.Command;
        UseOeGtscValidateGeneticTest();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // ---------------------------------------------
          // One or more errors exist.
          // EXIT STATE messages are set below.
          // ---------------------------------------------
          break;
        }

        UseOeScheduleGeneticTest();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        UseOeGtscDispFaMoChComb1();

        if (AsChar(export.GeneticTestInformation.FatherSampleCollectedInd) == 'Y'
          )
        {
          if (AsChar(export.GeneticTestInformation.MotherSampleCollectedInd) ==
            'Y' && AsChar
            (export.GeneticTestInformation.ChildSampleCollectedInd) != 'Y')
          {
            if (Lt(export.RecievedCh.ExpiryDate, local.Current.Date) && Lt
              (local.NullDateWorkArea.Date, export.RecievedCh.ExpiryDate))
            {
              var field =
                GetField(export.GeneticTestInformation, "childDrawSiteId");

              field.Error = true;

              ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

              return;
            }
          }

          if (AsChar(export.GeneticTestInformation.ChildSampleCollectedInd) == 'Y'
            && AsChar
            (export.GeneticTestInformation.MotherSampleCollectedInd) != 'Y')
          {
            if (Lt(export.ReceivedMo.ExpiryDate, local.Current.Date) && Lt
              (local.NullDateWorkArea.Date, export.ReceivedMo.ExpiryDate))
            {
              var field =
                GetField(export.GeneticTestInformation, "motherDrawSiteId");

              field.Error = true;

              ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

              return;
            }
          }

          if (Lt(export.RecievedCh.ExpiryDate, local.Current.Date) && Lt
            (local.NullDateWorkArea.Date, export.RecievedCh.ExpiryDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "childDrawSiteId");

            field.Error = true;

            ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

            return;
          }

          if (Lt(export.ReceivedMo.ExpiryDate, local.Current.Date) && Lt
            (local.NullDateWorkArea.Date, export.ReceivedMo.ExpiryDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "motherDrawSiteId");

            field.Error = true;

            ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

            return;
          }
        }
        else if (AsChar(export.GeneticTestInformation.MotherSampleCollectedInd) ==
          'Y')
        {
          if (AsChar(export.GeneticTestInformation.FatherSampleCollectedInd) ==
            'Y' && AsChar
            (export.GeneticTestInformation.ChildSampleCollectedInd) != 'Y')
          {
            if (Lt(export.RecievedCh.ExpiryDate, local.Current.Date) && Lt
              (local.NullDateWorkArea.Date, export.RecievedCh.ExpiryDate))
            {
              var field =
                GetField(export.GeneticTestInformation, "childDrawSiteId");

              field.Error = true;

              ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

              return;
            }
          }

          if (AsChar(export.GeneticTestInformation.ChildSampleCollectedInd) == 'Y'
            && AsChar
            (export.GeneticTestInformation.FatherSampleCollectedInd) != 'Y')
          {
            if (Lt(export.ReceivedFa.ExpiryDate, local.Current.Date) && Lt
              (local.NullDateWorkArea.Date, export.ReceivedFa.ExpiryDate))
            {
              var field =
                GetField(export.GeneticTestInformation, "fatherDrawSiteId");

              field.Error = true;

              ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

              return;
            }
          }

          if (Lt(export.RecievedCh.ExpiryDate, local.Current.Date) && Lt
            (local.NullDateWorkArea.Date, export.RecievedCh.ExpiryDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "childDrawSiteId");

            field.Error = true;

            ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

            return;
          }

          if (Lt(export.ReceivedFa.ExpiryDate, local.Current.Date) && Lt
            (local.NullDateWorkArea.Date, export.ReceivedFa.ExpiryDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "fatherDrawSiteId");

            field.Error = true;

            ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

            return;
          }
        }
        else if (AsChar(export.GeneticTestInformation.ChildSampleCollectedInd) ==
          'Y')
        {
          if (AsChar(export.GeneticTestInformation.MotherSampleCollectedInd) ==
            'Y' && AsChar
            (export.GeneticTestInformation.FatherSampleCollectedInd) != 'Y')
          {
            if (Lt(export.ReceivedFa.ExpiryDate, local.Current.Date) && Lt
              (local.NullDateWorkArea.Date, export.ReceivedFa.ExpiryDate))
            {
              var field =
                GetField(export.GeneticTestInformation, "fatherDrawSiteId");

              field.Error = true;

              ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

              return;
            }
          }

          if (AsChar(export.GeneticTestInformation.FatherSampleCollectedInd) ==
            'Y' && AsChar
            (export.GeneticTestInformation.MotherSampleCollectedInd) != 'Y')
          {
            if (Lt(export.ReceivedMo.ExpiryDate, local.Current.Date) && Lt
              (local.NullDateWorkArea.Date, export.ReceivedMo.ExpiryDate))
            {
              var field =
                GetField(export.GeneticTestInformation, "motherDrawSiteId");

              field.Error = true;

              ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

              return;
            }
          }

          if (Lt(export.ReceivedFa.ExpiryDate, local.Current.Date) && Lt
            (local.NullDateWorkArea.Date, export.ReceivedFa.ExpiryDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "fatherDrawSiteId");

            field.Error = true;

            ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

            return;
          }

          if (Lt(export.ReceivedMo.ExpiryDate, local.Current.Date) && Lt
            (local.NullDateWorkArea.Date, export.ReceivedMo.ExpiryDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "motherDrawSiteId");

            field.Error = true;

            ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

            return;
          }
        }
        else
        {
          if (Lt(export.RecievedCh.ExpiryDate, local.Current.Date) && Lt
            (local.NullDateWorkArea.Date, export.RecievedCh.ExpiryDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "childDrawSiteId");

            field.Error = true;

            ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

            return;
          }

          if (Lt(export.ReceivedFa.ExpiryDate, local.Current.Date) && Lt
            (local.NullDateWorkArea.Date, export.ReceivedFa.ExpiryDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "fatherDrawSiteId");

            field.Error = true;

            ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

            return;
          }

          if (Lt(export.ReceivedMo.ExpiryDate, local.Current.Date) && Lt
            (local.NullDateWorkArea.Date, export.ReceivedMo.ExpiryDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "motherDrawSiteId");

            field.Error = true;

            ExitState = "OE0000_CANNOT_SCHED_EXP_DATE_RB";

            return;
          }
        }

        // ----------------------------------------------------------
        //   00124663      08/16/2001           M Kumar
        //   Error message when trying to reschedule dad and child.
        //   The lines below have been commented out as part of the PR .
        // ----------------------------------------------------------
        if (AsChar(export.GeneticTestInformation.FatherPrevSampExistsInd) == 'N'
          )
        {
          var field = GetField(export.ListPrevSampFather, "promptField");

          field.Color = "cyan";
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.ListPrevSampFather, "promptField");

          field.Protected = false;
        }

        if (AsChar(export.GeneticTestInformation.MotherPrevSampExistsInd) == 'N'
          )
        {
          var field = GetField(export.ListPrevSampMother, "promptField");

          field.Color = "cyan";
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.ListPrevSampMother, "promptField");

          field.Protected = false;
        }

        if (AsChar(export.GeneticTestInformation.ChildPrevSampExistsInd) == 'N')
        {
          var field = GetField(export.ListPrevSampChild, "promptField");

          field.Color = "cyan";
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.ListPrevSampChild, "promptField");

          field.Protected = false;
        }

        // *******  Code added to support Events Insertions  ******
        // *******  Code for setting hidden views to spaces deleted.
        // ------------------------------------------------------------
        // CSEnet functionality -
        // RS code PIPNS sent if the case is an initiating or
        // responding interstate case and if the AP's test date or time
        // is rescheduled.
        // PIBTS sent if the case is an initiating or responding
        // interstate case and a blood test for ANY participant on the
        // case is scheduled.
        // ------------------------------------------------------------
        // ---------------
        // DETERMINE PIPNS
        // ---------------
        if (!Equal(export.HiddenGeneticTestInformation.FatherSchedTestDate, null)
          && !IsEmpty(export.HiddenGeneticTestInformation.FatherSchedTestTime))
        {
          if (!Equal(export.GeneticTestInformation.FatherSchedTestDate,
            export.HiddenGeneticTestInformation.FatherSchedTestDate))
          {
            local.ScreenIdentification.Command = "GTSC PIPNS";
          }

          if (!Equal(export.GeneticTestInformation.FatherSchedTestTime,
            export.HiddenGeneticTestInformation.FatherSchedTestTime))
          {
            local.ScreenIdentification.Command = "GTSC PIPNS";
          }
        }
        else
        {
          // -------------------
          // continue processing
          // -------------------
        }

        if (!Equal(export.GeneticTestInformation.ChildSchedTestDate,
          export.HiddenGeneticTestInformation.ChildSchedTestDate) && Equal
          (export.HiddenGeneticTestInformation.ChildSchedTestDate, null))
        {
          local.ScreenIdentification.Command = "GTSC PIBTS";
        }
        else if (!Equal(export.GeneticTestInformation.MotherSchedTestDate,
          export.HiddenGeneticTestInformation.MotherSchedTestDate) && Equal
          (export.HiddenGeneticTestInformation.MotherSchedTestDate, null))
        {
          local.ScreenIdentification.Command = "GTSC PIBTS";
        }
        else if (!Equal(export.GeneticTestInformation.FatherSchedTestDate,
          export.HiddenGeneticTestInformation.FatherSchedTestDate) && Equal
          (export.HiddenGeneticTestInformation.FatherSchedTestDate, null))
        {
          local.ScreenIdentification.Command = "GTSC PIBTS";
        }
        else
        {
          // -------------------
          // continue processing
          // -------------------
        }

        if (IsEmpty(local.ScreenIdentification.Command))
        {
        }
        else
        {
          UseSiCreateAutoCsenetTrans();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            // --------------------------------------
            // CICs rollback occurs in AB
            // --------------------------------------
            return;
          }
        }

        ExitState = "OE0000_SCHED_RESCHED_SUCCESSFUL";
        MoveGeneticTestInformation4(export.GeneticTestInformation,
          export.HiddenGeneticTestInformation);

        break;
      case "UPDATE":
        // ***************************************************************
        // If the test date is in the past, do not allow any updates to the
        // draw site, test date, or test time
        // ***************************************************************
        if (Lt(export.HiddenGeneticTestInformation.FatherSchedTestDate,
          Now().Date) && Lt
          (local.Initialized.FatherSchedTestDate,
          export.HiddenGeneticTestInformation.FatherSchedTestDate))
        {
          if (!Equal(export.HiddenGeneticTestInformation.FatherDrawSiteId,
            export.GeneticTestInformation.FatherDrawSiteId))
          {
            var field =
              GetField(export.GeneticTestInformation, "fatherDrawSiteId");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0186_GENETIC_SCHED_IN_PAST";
            }
          }

          if (!Equal(export.HiddenGeneticTestInformation.FatherSchedTestDate,
            export.GeneticTestInformation.FatherSchedTestDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "fatherSchedTestDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0186_GENETIC_SCHED_IN_PAST";
            }
          }

          if (!Equal(export.GeneticTestInformation.FatherSchedTestTime,
            export.HiddenGeneticTestInformation.FatherSchedTestTime))
          {
            var field =
              GetField(export.GeneticTestInformation, "fatherSchedTestTime");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0186_GENETIC_SCHED_IN_PAST";
            }
          }
        }
        else if (!Lt(Now().Date,
          export.GeneticTestInformation.FatherSchedTestDate))
        {
          var field =
            GetField(export.GeneticTestInformation, "fatherSchedTestDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0187_GENETIC_SCHED_DATE_ERROR";
          }
        }

        if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
        {
          if (Lt(export.HiddenGeneticTestInformation.MotherSchedTestDate,
            Now().Date) && Lt
            (local.Initialized.MotherSchedTestDate,
            export.HiddenGeneticTestInformation.MotherSchedTestDate))
          {
            if (!Equal(export.HiddenGeneticTestInformation.MotherDrawSiteId,
              export.GeneticTestInformation.MotherDrawSiteId))
            {
              var field =
                GetField(export.GeneticTestInformation, "motherDrawSiteId");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "OE0186_GENETIC_SCHED_IN_PAST";
              }
            }

            if (!Equal(export.HiddenGeneticTestInformation.MotherSchedTestDate,
              export.GeneticTestInformation.MotherSchedTestDate))
            {
              var field =
                GetField(export.GeneticTestInformation, "motherSchedTestDate");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "OE0186_GENETIC_SCHED_IN_PAST";
              }
            }

            if (!Equal(export.GeneticTestInformation.MotherSchedTestTime,
              export.HiddenGeneticTestInformation.MotherSchedTestTime))
            {
              var field =
                GetField(export.GeneticTestInformation, "motherSchedTestTime");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "OE0186_GENETIC_SCHED_IN_PAST";
              }
            }
          }
          else if (!Lt(Now().Date,
            export.GeneticTestInformation.MotherSchedTestDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "motherSchedTestDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0187_GENETIC_SCHED_DATE_ERROR";
            }
          }
        }

        if (Lt(export.HiddenGeneticTestInformation.ChildSchedTestDate,
          Now().Date) && Lt
          (local.Initialized.ChildSchedTestDate,
          export.HiddenGeneticTestInformation.ChildSchedTestDate))
        {
          if (!Equal(export.HiddenGeneticTestInformation.ChildDrawSiteId,
            export.GeneticTestInformation.ChildDrawSiteId))
          {
            var field =
              GetField(export.GeneticTestInformation, "childDrawSiteId");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0186_GENETIC_SCHED_IN_PAST";
            }
          }

          if (!Equal(export.HiddenGeneticTestInformation.ChildSchedTestDate,
            export.GeneticTestInformation.ChildSchedTestDate))
          {
            var field =
              GetField(export.GeneticTestInformation, "childSchedTestDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0186_GENETIC_SCHED_IN_PAST";
            }
          }

          if (!Equal(export.GeneticTestInformation.ChildSchedTestTime,
            export.HiddenGeneticTestInformation.ChildSchedTestTime))
          {
            var field =
              GetField(export.GeneticTestInformation, "childSchedTestTime");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "OE0186_GENETIC_SCHED_IN_PAST";
            }
          }
        }
        else if (!Lt(Now().Date,
          export.GeneticTestInformation.ChildSchedTestDate))
        {
          var field =
            GetField(export.GeneticTestInformation, "childSchedTestDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OE0187_GENETIC_SCHED_DATE_ERROR";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.Standard.Command = global.Command;
        UseOeGtscValidateGeneticTest();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // ---------------------------------------------
          // One or more errors exist.
          // EXIT state error messages are set below.
          // ---------------------------------------------
          break;
        }

        UseOeGtscUpdateGenTestDetls();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        // --05/10/2017 GVandy CQ48104 (IV-D PEP Changes)  Do not update 
        // paternity information from GTSC.
        UseOeGtscDispFaMoChComb1();

        if (AsChar(export.GeneticTestInformation.FatherPrevSampExistsInd) == 'N'
          )
        {
          var field = GetField(export.ListPrevSampFather, "promptField");

          field.Color = "cyan";
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.ListPrevSampFather, "promptField");

          field.Protected = false;
        }

        if (AsChar(export.GeneticTestInformation.MotherPrevSampExistsInd) == 'N'
          )
        {
          var field = GetField(export.ListPrevSampMother, "promptField");

          field.Color = "cyan";
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.ListPrevSampMother, "promptField");

          field.Protected = false;
        }

        if (AsChar(export.GeneticTestInformation.ChildPrevSampExistsInd) == 'N')
        {
          var field = GetField(export.ListPrevSampChild, "promptField");

          field.Color = "cyan";
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.ListPrevSampChild, "promptField");

          field.Protected = false;
        }

        MoveGeneticTestInformation4(export.GeneticTestInformation,
          export.HiddenGeneticTestInformation);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        // ----------------------------------------------------------
        // I00106450       11/27/2000            P Phinney
        // Check the Expiry Date to see if the Vendor Address is Current
        // ----------------------------------------------------------
        // ----------------------------------------------------------
        // 00124663      08/16/2001           M Kumar
        //   Error message when trying to reschedule dad and child.
        //  The lines below have been commented out as part of the PR .
        // ----------------------------------------------------------
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // -------------------------------------------------------------------
    // PR# 81845: The following code is written to protect all the fields 
    // related to 'MOTHER' if the mother is not selected on COMP screen ("
    // Motherless Comparisons" Genetic Test).
    //                                                          
    // Vithal (03/30/2000)
    // -------------------------------------------------------------------
    if (IsEmpty(import.SelectedMother.Number))
    {
      var field1 = GetField(export.GeneticTestInformation, "motherPersonNo");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.GeneticTestInformation, "motherDrawSiteId");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.ListDrawSiteMother, "promptField");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 =
        GetField(export.GeneticTestInformation, "motherSchedTestDate");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 =
        GetField(export.GeneticTestInformation, "motherSchedTestTime");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 =
        GetField(export.GeneticTestInformation, "motherPrevSampExistsInd");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 =
        GetField(export.GeneticTestInformation, "motherCollectSampleInd");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 =
        GetField(export.GeneticTestInformation, "motherReuseSampleInd");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 =
        GetField(export.GeneticTestInformation, "motherSampleCollectedInd");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.GeneticTestInformation, "motherShowInd");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.GeneticTestInformation, "motherSpecimenId");

      field11.Color = "cyan";
      field11.Protected = true;

      var field12 =
        GetField(export.GeneticTestInformation, "motherRescheduledInd");

      field12.Color = "cyan";
      field12.Protected = true;

      var field13 =
        GetField(export.GeneticTestInformation, "motherFormattedName");

      field13.Color = "cyan";
      field13.Protected = true;

      var field14 =
        GetField(export.GeneticTestInformation, "motherPrevSampTestType");

      field14.Color = "cyan";
      field14.Protected = true;

      var field15 =
        GetField(export.GeneticTestInformation, "motherPrevSampLabCaseNo");

      field15.Color = "cyan";
      field15.Protected = true;

      var field16 =
        GetField(export.GeneticTestInformation, "motherPrevSampSpecimenId");

      field16.Color = "cyan";
      field16.Protected = true;

      var field17 =
        GetField(export.GeneticTestInformation, "motherDrawSiteVendorName");

      field17.Color = "cyan";
      field17.Protected = true;

      var field18 =
        GetField(export.GeneticTestInformation, "motherDrawSiteCity");

      field18.Color = "cyan";
      field18.Protected = true;

      var field19 =
        GetField(export.GeneticTestInformation, "motherDrawSiteState");

      field19.Color = "cyan";
      field19.Protected = true;

      var field20 = GetField(export.ListPrevSampMother, "promptField");

      field20.Color = "cyan";
      field20.Protected = true;
    }

    // mjr
    // ---------------------------------------------
    // 12/30/1998
    // Pulled Display out of main case of command for return from Print
    // ----------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      UseSiReadOfficeOspHeader();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.GeneticTestInformation, "caseNumber");

        field.Error = true;

        return;
      }

      // When this procedure step is entered for the First Time.
      for(export.HiddenSelCombn.Index = 0; export.HiddenSelCombn.Index < 3; ++
        export.HiddenSelCombn.Index)
      {
        if (!export.HiddenSelCombn.CheckSize())
        {
          break;
        }

        if (Equal(export.HiddenSelCombn.Item.DetailHiddenCombnCaseRole.Type1,
          "MO"))
        {
          continue;
        }

        if (IsEmpty(export.HiddenSelCombn.Item.DetailHiddenCombnCsePerson.Number))
          
        {
          ExitState = "OE0000_GO_TO_COMP_TO_DISP_GTSC";

          return;
        }
      }

      export.HiddenSelCombn.CheckIndex();
      UseOeGtscDispFaMoChComb2();

      if (IsExitState("ACO_NI0000_SUCCESSFUL_DISPLAY"))
      {
        // mjr
        // -----------------------------------------------
        // 12/15/1998
        // Added check for an exitstate returned from Print
        // ------------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.HiddenNextTranInfo.MiscText2,
          NextTranInfo.MiscText2_MaxLength),
          TrimEnd(local.SpDocLiteral.IdDocument));

        if (local.Position.Count <= 0)
        {
          if (AsChar(export.ActiveAp.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_INACTIVE_AP";
          }

          if (AsChar(export.ActiveChild.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_INACTIVE_CHILD";
          }

          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
        }
        else
        {
          // mjr---> Determines the appropriate exitstate for the Print process
          local.Print.Text50 = export.HiddenNextTranInfo.MiscText2 ?? Spaces
            (50);
          UseSpPrintDecodeReturnCode();
          export.HiddenNextTranInfo.MiscText2 = local.Print.Text50;
        }

        MoveGeneticTestInformation4(export.GeneticTestInformation,
          export.HiddenGeneticTestInformation);
        MoveGeneticTestInformation6(export.GeneticTestInformation,
          export.HiddenForEvents);
      }

      if (!IsEmpty(export.GeneticTestInformation.TestType))
      {
        local.Code.CodeName = "GENETIC TEST TYPE";
        local.CodeValue.Cdvalue = export.GeneticTestInformation.TestType;
        UseCabGetCodeValueDescription();
      }

      // ---------------------------------------------
      // If there was an error, exit state would not be ALL_OK and corresponding
      // exit state error message will be displayed.
      // ---------------------------------------------
      if (AsChar(export.GeneticTestInformation.FatherPrevSampExistsInd) == 'N')
      {
        var field = GetField(export.ListPrevSampFather, "promptField");

        field.Color = "cyan";
        field.Protected = true;
      }
      else
      {
        var field = GetField(export.ListPrevSampFather, "promptField");

        field.Protected = false;
      }

      if (!IsEmpty(export.GeneticTestInformation.MotherPersonNo))
      {
        if (AsChar(export.GeneticTestInformation.MotherPrevSampExistsInd) == 'N'
          )
        {
          var field = GetField(export.ListPrevSampMother, "promptField");

          field.Color = "cyan";
          field.Protected = true;
        }
        else
        {
          var field = GetField(export.ListPrevSampMother, "promptField");

          field.Protected = false;
        }
      }

      if (AsChar(export.GeneticTestInformation.ChildPrevSampExistsInd) == 'N')
      {
        var field = GetField(export.ListPrevSampChild, "promptField");

        field.Color = "cyan";
        field.Protected = true;
      }
      else
      {
        var field = GetField(export.ListPrevSampChild, "promptField");

        field.Protected = false;
      }

      // *********************************************************************
      //  PR-124 663           08/29/01           M Kumar
      //  The lines below have been commented out as a part of the above PR.
      // *********************************************************************
      return;
    }

    if (local.LastErrorEntryNo.Count != 0)
    {
      // ---------------------------------------------
      // One or more errors exist. Highlight errors in the reverse order.
      // ---------------------------------------------
      local.ErrorCodes.Index = local.LastErrorEntryNo.Count - 1;
      local.ErrorCodes.CheckSize();

      while(local.ErrorCodes.Index >= 0)
      {
        switch(local.ErrorCodes.Item.EntryErrorCode.Count)
        {
          case 1:
            var field1 = GetField(export.GeneticTestInformation, "caseNumber");

            field1.Error = true;

            ExitState = "OE0020_INVALID_CASE_NO";

            break;
          case 2:
            var field2 =
              GetField(export.GeneticTestInformation, "geneticTestAccountNo");

            field2.Error = true;

            ExitState = "OE0034_INVALID_GENETIC_TEST_AC";

            break;
          case 3:
            var field3 =
              GetField(export.GeneticTestInformation, "geneticTestAccountNo");

            field3.Error = true;

            ExitState = "OE0034_INVALID_GENETIC_TEST_AC";

            break;
          case 4:
            var field4 = GetField(export.GeneticTestInformation, "testType");

            field4.Error = true;

            ExitState = "OE0035_INVALID_GEN_TEST_TYPE";

            break;
          case 5:
            var field5 = GetField(export.GeneticTestInformation, "testType");

            field5.Error = true;

            ExitState = "OE0035_INVALID_GEN_TEST_TYPE";

            break;
          case 6:
            var field6 =
              GetField(export.GeneticTestInformation, "fatherDrawSiteId");

            field6.Error = true;

            ExitState = "OE0031_INVALID_DRAW_SITE_ID_FA";

            break;
          case 7:
            var field7 =
              GetField(export.GeneticTestInformation, "fatherCollectSampleInd");
              

            field7.Error = true;

            ExitState = "OE0023_INVALID_COLL_SAMP_FA";

            break;
          case 8:
            var field8 =
              GetField(export.GeneticTestInformation, "fatherReuseSampleInd");

            field8.Error = true;

            ExitState = "OE0046_INVALID_SAMP_REUSBL_FA";

            break;
          case 9:
            var field9 =
              GetField(export.GeneticTestInformation, "motherDrawSiteId");

            field9.Error = true;

            ExitState = "OE0032_INVALID_DRAW_SITE_ID_MO";

            break;
          case 10:
            var field10 =
              GetField(export.GeneticTestInformation, "motherCollectSampleInd");
              

            field10.Error = true;

            ExitState = "OE0024_INVALID_COLL_SAMP_MOTH";

            break;
          case 11:
            var field11 =
              GetField(export.GeneticTestInformation, "motherReuseSampleInd");

            field11.Error = true;

            ExitState = "OE0047_INVALID_SAMP_REUSBL_MO";

            break;
          case 12:
            var field12 =
              GetField(export.GeneticTestInformation, "childDrawSiteId");

            field12.Error = true;

            ExitState = "OE0030_INVALID_DRAW_SITE_ID_CH";

            break;
          case 13:
            var field13 =
              GetField(export.GeneticTestInformation, "childCollectSampleInd");

            field13.Error = true;

            ExitState = "OE0022_INVALID_COLL_SAMP_CHILD";

            break;
          case 14:
            var field14 =
              GetField(export.GeneticTestInformation, "childReuseSampleInd");

            field14.Error = true;

            ExitState = "OE0045_INVALID_SAMP_REUSBL_CH";

            break;
          case 15:
            var field15 =
              GetField(export.GeneticTestInformation, "testSiteVendorId");

            field15.Error = true;

            ExitState = "OE0052_INVALID_TEST_SITE_ID";

            break;
          case 16:
            var field16 =
              GetField(export.GeneticTestInformation, "fatherSampleCollectedInd");
              

            field16.Error = true;

            ExitState = "OE0043_INVALID_SAMP_COLL_FA";

            break;
          case 17:
            var field17 =
              GetField(export.GeneticTestInformation, "motherSampleCollectedInd");
              

            field17.Error = true;

            ExitState = "OE0044_INVALID_SAMP_COLL_MO";

            break;
          case 18:
            var field18 =
              GetField(export.GeneticTestInformation, "childSampleCollectedInd");
              

            field18.Error = true;

            ExitState = "OE0042_INVALID_SAMP_COLL_CH";

            break;
          case 19:
            var field19 =
              GetField(export.GeneticTestInformation, "actualTestDate");

            field19.Error = true;

            ExitState = "OE0000_INVALID_GTEST_ACT_TEST_DT";

            break;
          case 20:
            var field20 =
              GetField(export.GeneticTestInformation, "resultReceivedDate");

            field20.Error = true;

            ExitState = "OE0000_INVALID_GTEST_RSLT_RCVDDT";

            break;
          case 21:
            var field21 =
              GetField(export.GeneticTestInformation, "paternityExcludedInd");

            field21.Error = true;

            ExitState = "OE0040_INVALID_PAT_EXC_IND";

            break;
          case 22:
            var field22 =
              GetField(export.GeneticTestInformation, "paternityProbability");

            field22.Error = true;

            ExitState = "OE0041_INVALID_PAT_PROB";

            break;
          case 23:
            var field23 =
              GetField(export.GeneticTestInformation, "resultContestedInd");

            field23.Error = true;

            ExitState = "OE0009_INVALID_RESLT_CONT_IND";

            break;
          case 24:
            var field24 =
              GetField(export.GeneticTestInformation, "fatherShowInd");

            field24.Error = true;

            ExitState = "OE0000_INVALID_GTEST_SHOW_IND";

            break;
          case 25:
            var field25 =
              GetField(export.GeneticTestInformation, "motherShowInd");

            field25.Error = true;

            ExitState = "OE0000_INVALID_GTEST_SHOW_IND";

            break;
          case 26:
            var field26 =
              GetField(export.GeneticTestInformation, "childShowInd");

            field26.Error = true;

            ExitState = "OE0000_INVALID_GTEST_SHOW_IND";

            break;
          case 27:
            var field27 =
              GetField(export.GeneticTestInformation, "fatherPrevSampTestType");
              

            field27.Error = true;

            var field28 =
              GetField(export.GeneticTestInformation,
              "fatherPrevSampleLabCaseNo");

            field28.Error = true;

            var field29 =
              GetField(export.GeneticTestInformation, "fatherPrevSampSpecimenId");
              

            field29.Error = true;

            ExitState = "OE0000_INVALID_PREV_GTEST_SAMP";

            break;
          case 28:
            var field30 =
              GetField(export.GeneticTestInformation, "motherPrevSampSpecimenId");
              

            field30.Error = true;

            var field31 =
              GetField(export.GeneticTestInformation, "motherPrevSampLabCaseNo");
              

            field31.Error = true;

            var field32 =
              GetField(export.GeneticTestInformation, "motherPrevSampTestType");
              

            field32.Error = true;

            ExitState = "OE0000_INVALID_PREV_GTEST_SAMP";

            break;
          case 29:
            var field33 =
              GetField(export.GeneticTestInformation, "childPrevSampSpecimenId");
              

            field33.Error = true;

            var field34 =
              GetField(export.GeneticTestInformation, "childPrevSampLabCaseNo");
              

            field34.Error = true;

            var field35 =
              GetField(export.GeneticTestInformation, "childPrevSampTestType");

            field35.Error = true;

            ExitState = "OE0000_INVALID_PREV_GTEST_SAMP";

            break;
          case 30:
            if (!IsEmpty(export.GeneticTestInformation.FatherPrevSampSpecimenId))
              
            {
              var field =
                GetField(export.GeneticTestInformation,
                "fatherPrevSampSpecimenId");

              field.Error = true;
            }

            if (!IsEmpty(export.GeneticTestInformation.MotherPrevSampLabCaseNo))
            {
              var field =
                GetField(export.GeneticTestInformation,
                "fatherPrevSampleLabCaseNo");

              field.Error = true;
            }

            if (!IsEmpty(export.GeneticTestInformation.FatherPrevSampTestType))
            {
              var field =
                GetField(export.GeneticTestInformation, "fatherPrevSampTestType");
                

              field.Error = true;
            }

            ExitState = "OE0000_INVALID_PREV_GTEST_SAMP";

            break;
          case 31:
            if (!IsEmpty(export.GeneticTestInformation.MotherPrevSampSpecimenId))
              
            {
              var field =
                GetField(export.GeneticTestInformation,
                "motherPrevSampSpecimenId");

              field.Error = true;
            }

            if (!IsEmpty(export.GeneticTestInformation.MotherPrevSampLabCaseNo))
            {
              var field =
                GetField(export.GeneticTestInformation,
                "motherPrevSampLabCaseNo");

              field.Error = true;
            }

            if (!IsEmpty(export.GeneticTestInformation.MotherPrevSampTestType))
            {
              var field =
                GetField(export.GeneticTestInformation, "motherPrevSampTestType");
                

              field.Error = true;
            }

            ExitState = "OE0000_INVALID_PREV_GTEST_SAMP";

            break;
          case 32:
            if (!IsEmpty(export.GeneticTestInformation.ChildPrevSampSpecimenId))
            {
              var field =
                GetField(export.GeneticTestInformation,
                "childPrevSampSpecimenId");

              field.Error = true;
            }

            if (!IsEmpty(export.GeneticTestInformation.ChildPrevSampLabCaseNo))
            {
              var field =
                GetField(export.GeneticTestInformation, "childPrevSampLabCaseNo");
                

              field.Error = true;
            }

            if (!IsEmpty(export.GeneticTestInformation.ChildPrevSampTestType))
            {
              var field =
                GetField(export.GeneticTestInformation, "childPrevSampTestType");
                

              field.Error = true;
            }

            ExitState = "OE0000_INVALID_PREV_GTEST_SAMP";

            break;
          case 33:
            var field36 =
              GetField(export.GeneticTestInformation, "contestStartedDate");

            field36.Error = true;

            ExitState = "OE0000_INVALID_GTEST_CONTST_STRT";

            break;
          case 34:
            var field37 =
              GetField(export.GeneticTestInformation, "contestEndedDate");

            field37.Error = true;

            ExitState = "OE0000_INVALID_GTEST_CONTST_ENDD";

            break;
          case 35:
            var field38 =
              GetField(export.GeneticTestInformation, "fatherSchedTestDate");

            field38.Error = true;

            ExitState = "OE0000_INVALID_GTEST_SCHED_DATE";

            break;
          case 36:
            var field39 =
              GetField(export.GeneticTestInformation, "motherSchedTestDate");

            field39.Error = true;

            ExitState = "OE0000_INVALID_GTEST_SCHED_DATE";

            break;
          case 37:
            var field40 =
              GetField(export.GeneticTestInformation, "childSchedTestDate");

            field40.Error = true;

            ExitState = "OE0000_INVALID_GTEST_SCHED_DATE";

            break;
          case 38:
            var field41 =
              GetField(export.GeneticTestInformation, "fatherSchedTestTime");

            field41.Error = true;

            ExitState = "OE0000_INVALID_FATHER_SCHED_TIME";

            break;
          case 39:
            var field42 =
              GetField(export.GeneticTestInformation, "motherSchedTestTime");

            field42.Error = true;

            ExitState = "OE0000_INVALID_MTHR_SCHD_TEST_TM";

            break;
          case 40:
            var field43 =
              GetField(export.GeneticTestInformation, "childSchedTestTime");

            field43.Error = true;

            ExitState = "OE0000_INVALID_CHILD_SCHD_TESTTM";

            break;
          case 41:
            var field44 =
              GetField(export.GeneticTestInformation, "courtOrderNo");

            field44.Error = true;

            ExitState = "OE0000_INVALID_CT_ORD_NO_SELECTD";

            break;
          case 42:
            var field45 =
              GetField(export.GeneticTestInformation, "fatherSampleCollectedInd");
              

            field45.Error = true;

            ExitState = "OE0000_SAMP_COLL_IND_FA_MUST_B_Y";

            break;
          case 43:
            var field46 =
              GetField(export.GeneticTestInformation, "motherSampleCollectedInd");
              

            field46.Error = true;

            ExitState = "OE0000_SAMP_COLL_IND_MO_MUST_B_Y";

            break;
          case 44:
            var field47 =
              GetField(export.GeneticTestInformation, "childSampleCollectedInd");
              

            field47.Error = true;

            ExitState = "OE0000_SAMP_COLL_IND_CH_MUST_B_Y";

            break;
          case 45:
            var field48 =
              GetField(export.GeneticTestInformation, "paternityExcludedInd");

            field48.Error = true;

            ExitState = "OE0000_PAT_IND_NOT_SET_TO_N";

            break;
          case 46:
            var field49 =
              GetField(export.GeneticTestInformation, "actualTestDate");

            field49.Error = true;

            ExitState = "OE0000_ACTUAL_TESTDATE_NOT_ENTRD";

            break;
          case 47:
            var field50 =
              GetField(export.GeneticTestInformation, "resultReceivedDate");

            field50.Error = true;

            ExitState = "OE0000_RESULT_RECD_DT_NOT_ENTERD";

            break;
          case 48:
            var field51 =
              GetField(export.GeneticTestInformation, "paternityProbability");

            field51.Error = true;

            ExitState = "OE0000_PAR_PROB_NOT_ENTERED";

            break;
          case 49:
            var field52 =
              GetField(export.GeneticTestInformation, "fatherShowInd");

            field52.Error = true;

            ExitState = "OE0000_SHOW_IND_NOT_SET_TO_Y";

            break;
          case 50:
            var field53 =
              GetField(export.GeneticTestInformation, "motherShowInd");

            field53.Error = true;

            ExitState = "OE0000_SHOW_IND_NOT_SET_TO_Y";

            break;
          case 51:
            var field54 =
              GetField(export.GeneticTestInformation, "childShowInd");

            field54.Error = true;

            ExitState = "OE0000_SHOW_IND_NOT_SET_TO_Y";

            break;
          case 52:
            var field55 =
              GetField(export.GeneticTestInformation, "testSiteVendorId");

            field55.Error = true;

            var field56 =
              GetField(export.GeneticTestInformation, "resultReceivedDate");

            field56.Error = true;

            ExitState = "OE0052_INVALID_TEST_SITE_ID";

            break;
          case 53:
            var field57 =
              GetField(export.GeneticTestInformation, "paternityProbability");

            field57.Error = true;

            var field58 =
              GetField(export.GeneticTestInformation, "resultReceivedDate");

            field58.Error = true;

            ExitState = "OE0041_INVALID_PAT_PROB";

            break;
          case 54:
            var field59 =
              GetField(export.GeneticTestInformation, "fatherSampleCollectedInd");
              

            field59.Error = true;

            ExitState = "OE0000_SAMPLE_COLLECTD_PREMATURE";

            break;
          case 55:
            var field60 =
              GetField(export.GeneticTestInformation, "motherSampleCollectedInd");
              

            field60.Error = true;

            ExitState = "OE0000_SAMPLE_COLLECTD_PREMATURE";

            break;
          case 56:
            var field61 =
              GetField(export.GeneticTestInformation, "childSampleCollectedInd");
              

            field61.Error = true;

            ExitState = "OE0000_SAMPLE_COLLECTD_PREMATURE";

            break;
          case 57:
            var field62 =
              GetField(export.GeneticTestInformation, "fatherShowInd");

            field62.Error = true;

            ExitState = "OE0000_SHOW_IND_PREMATURE";

            break;
          case 58:
            var field63 =
              GetField(export.GeneticTestInformation, "motherShowInd");

            field63.Error = true;

            ExitState = "OE0000_SHOW_IND_PREMATURE";

            break;
          case 59:
            var field64 =
              GetField(export.GeneticTestInformation, "childShowInd");

            field64.Error = true;

            ExitState = "OE0000_SHOW_IND_PREMATURE";

            break;
          case 60:
            var field65 =
              GetField(export.GeneticTestInformation, "paternityExcludedInd");

            field65.Error = true;

            var field66 =
              GetField(export.GeneticTestInformation, "paternityProbability");

            field66.Error = true;

            ExitState = "OE0000_PAT_IND_AND_PROB_BOTH_SET";

            break;
          case 61:
            var field67 =
              GetField(export.GeneticTestInformation, "paternityProbability");

            field67.Error = true;

            ExitState = "OE0000_PROB_IS_MANDATORY";

            break;
          default:
            ExitState = "OE0004_UNKNOWN_ERROR_CODE";

            break;
        }

        --local.ErrorCodes.Index;
        local.ErrorCodes.CheckSize();
      }

      return;
    }

    // *******  Code added to support Events Insertions  ******
    if (IsExitState("OE0000_SCHED_RESCHED_SUCCESSFUL") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      local.Infrastructure.SituationNumber = 0;

      for(local.NumberOfEvents.TotalInteger = 1; local
        .NumberOfEvents.TotalInteger <= 10; ++
        local.NumberOfEvents.TotalInteger)
      {
        local.RaiseEventFlag.Text1 = "N";
        local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
        local.DateText.Text8 = "";

        if (local.NumberOfEvents.TotalInteger == 1)
        {
          local.NotPaRefrlPersonFlag.Flag = "N";
          local.Infrastructure.ReasonCode = "RESULTNEG";
          local.Infrastructure.ReferenceDate =
            export.GeneticTestInformation.ResultReceivedDate;
          local.Date.Date = export.GeneticTestInformation.ResultReceivedDate;
          UseCabConvertDate2String();

          if (!Equal(export.GeneticTestInformation.ResultReceivedDate,
            export.HiddenForEvents.ResultReceivedDate))
          {
            if (AsChar(export.GeneticTestInformation.PaternityExcludedInd) == 'Y'
              )
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail =
                "Negative Genetic Test Result Received Date : " + local
                .DateText.Text8;
            }
          }
          else if (AsChar(export.GeneticTestInformation.PaternityExcludedInd) !=
            AsChar(export.HiddenForEvents.PaternityExcludedInd))
          {
            if (AsChar(export.GeneticTestInformation.PaternityExcludedInd) == 'Y'
              )
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail =
                "Negative Genetic Test Result Received Date : " + local
                .DateText.Text8;
            }
          }

          if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
          {
            // ---------------------------------------------
            // Code added - external alerts
            //    Raju : 01/06/97:1655 hrs CST
            // ---------------------------------------------
            // ---------------------------------------------
            // Start of Code
            // ---------------------------------------------
            local.InterfaceAlert.AlertCode = "44";
            local.Ar.Number = export.GeneticTestInformation.MotherPersonNo;
            local.Ap.Number = export.GeneticTestInformation.FatherPersonNo;
            local.Ch.Number = export.GeneticTestInformation.ChildPersonNo;
            local.Case1.Number = export.GeneticTestInformation.CaseNumber;
            UseSpGtscExternalAlert();

            if (IsExitState("OE0000_SCHED_RESCHED_SUCCESSFUL") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else if (IsExitState("SI0000_CASE_NOT_FROM_REFERRAL") || IsExitState
              ("PA_REFERRRAL_PARTICIPANT_NF"))
            {
              local.NotPaRefrlPersonFlag.Flag = "Y";
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
              local.RaiseEventFlag.Text1 = "Y";
            }
            else
            {
              return;
            }

            // ---------------------------------------------
            // End   of Code
            // ---------------------------------------------
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 2)
        {
          local.Infrastructure.ReasonCode = "RESULTPOS";
          local.Infrastructure.ReferenceDate =
            export.GeneticTestInformation.ResultReceivedDate;
          local.Date.Date = export.GeneticTestInformation.ResultReceivedDate;
          UseCabConvertDate2String();

          if (!Equal(export.GeneticTestInformation.ResultReceivedDate,
            export.HiddenForEvents.ResultReceivedDate))
          {
            if (AsChar(export.GeneticTestInformation.PaternityExcludedInd) == 'N'
              || export.GeneticTestInformation.PaternityProbability > 0)
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail =
                "Positive Genetic Test Result Received Date : " + local
                .DateText.Text8;
            }
          }
          else if (AsChar(export.GeneticTestInformation.PaternityExcludedInd) !=
            AsChar(export.HiddenForEvents.PaternityExcludedInd) && AsChar
            (export.GeneticTestInformation.PaternityExcludedInd) == 'N' || export
            .GeneticTestInformation.PaternityProbability != export
            .HiddenForEvents.PaternityProbability && export
            .GeneticTestInformation.PaternityProbability > 0)
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.Detail =
              "Positive Genetic Test Result Received Date : " + local
              .DateText.Text8;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 3)
        {
          local.Infrastructure.ReasonCode = "FASCHED";
          local.Infrastructure.ReferenceDate =
            export.GeneticTestInformation.FatherSchedTestDate;
          local.Date.Date = export.GeneticTestInformation.FatherSchedTestDate;
          UseCabConvertDate2String();

          if (!Equal(export.GeneticTestInformation.FatherSchedTestDate,
            export.HiddenForEvents.FatherSchedTestDate))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.Detail =
              "Genetic Test Scheduled Date for Father :" + local
              .DateText.Text8;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 4)
        {
          local.Infrastructure.ReasonCode = "MOSCHED";
          local.Infrastructure.ReferenceDate =
            export.GeneticTestInformation.MotherSchedTestDate;
          local.Date.Date = export.GeneticTestInformation.MotherSchedTestDate;
          UseCabConvertDate2String();

          if (!Equal(export.GeneticTestInformation.MotherSchedTestDate,
            export.HiddenForEvents.MotherSchedTestDate))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.Detail =
              "Genetic Test Scheduled Date for Mother :" + local
              .DateText.Text8;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 5)
        {
          local.Infrastructure.ReasonCode = "CHSCHED";
          local.Infrastructure.ReferenceDate =
            export.GeneticTestInformation.ChildSchedTestDate;
          local.Date.Date = export.GeneticTestInformation.ChildSchedTestDate;
          UseCabConvertDate2String();

          if (!Equal(export.GeneticTestInformation.ChildSchedTestDate,
            export.HiddenForEvents.ChildSchedTestDate))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.Detail =
              "Genetic Test Scheduled Date for Child :" + local.DateText.Text8;
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 6)
        {
          local.Infrastructure.ReasonCode = "GTNOSHOFA";
          local.Date.Date = export.GeneticTestInformation.FatherSchedTestDate;
          UseCabConvertDate2String();

          if (AsChar(export.GeneticTestInformation.FatherShowInd) != AsChar
            (export.HiddenForEvents.FatherShowInd))
          {
            if (AsChar(export.GeneticTestInformation.FatherShowInd) == 'N')
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail =
                "No Show by Father for Genetic Test Scheduled Date :" + local
                .DateText.Text8;
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 7)
        {
          local.Infrastructure.ReasonCode = "GTNOSHOMO";
          local.Date.Date = export.GeneticTestInformation.MotherSchedTestDate;
          UseCabConvertDate2String();

          if (AsChar(export.GeneticTestInformation.MotherShowInd) != AsChar
            (export.HiddenForEvents.MotherShowInd))
          {
            if (AsChar(export.GeneticTestInformation.MotherShowInd) == 'N')
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail =
                "No Show by Mother for Genetic Test Scheduled Date :" + local
                .DateText.Text8;
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 8)
        {
          local.Infrastructure.ReasonCode = "GTNOSHOCH";
          local.Date.Date = export.GeneticTestInformation.ChildSchedTestDate;
          UseCabConvertDate2String();

          if (AsChar(export.GeneticTestInformation.ChildShowInd) != AsChar
            (export.HiddenForEvents.ChildShowInd))
          {
            if (AsChar(export.GeneticTestInformation.ChildShowInd) == 'N')
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail =
                "No Show by Child for Genetic Test Scheduled Date :" + local
                .DateText.Text8;
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 9)
        {
          local.Infrastructure.ReasonCode = "GENTESCONSTART";
          local.Date.Date = export.GeneticTestInformation.ContestStartedDate;
          UseCabConvertDate2String();

          if (AsChar(export.GeneticTestInformation.ResultContestedInd) == 'Y')
          {
            if (AsChar(export.GeneticTestInformation.ResultContestedInd) != AsChar
              (export.HiddenForEvents.ResultContestedInd) || !
              Equal(export.GeneticTestInformation.ContestStartedDate,
              export.HiddenForEvents.ContestStartedDate))
            {
              local.RaiseEventFlag.Text1 = "Y";
              local.Infrastructure.Detail =
                "Genetic Test Results Contested Start Date:" + local
                .DateText.Text8;
            }
          }
        }
        else if (local.NumberOfEvents.TotalInteger == 10)
        {
          local.Infrastructure.ReasonCode = "GENTESCONEND";
          local.Date.Date = export.GeneticTestInformation.ContestEndedDate;
          UseCabConvertDate2String();

          if (AsChar(export.GeneticTestInformation.ResultContestedInd) == 'Y'
            && !
            Equal(export.GeneticTestInformation.ContestEndedDate,
            export.HiddenForEvents.ContestEndedDate))
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.Detail =
              "Genetic Test Results Contested End Date:" + local
              .DateText.Text8;
          }
        }
        else
        {
        }

        if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
        {
          // --------------------------------------------
          // This is to aid the event processor to
          //    gather events from a single situation
          // This is an extremely important piece of code
          // --------------------------------------------
          local.Infrastructure.DenormNumeric12 =
            export.HiddenGeneticTest.TestNumber;
          UseOeGtscRaiseEvents();

          if (IsExitState("OE0000_SCHED_RESCHED_SUCCESSFUL") || IsExitState
            ("ACO_NI0000_SUCCESSFUL_UPDATE"))
          {
          }
          else
          {
            UseEabRollbackCics();

            return;
          }

          local.RaiseEventFlag.Text1 = "N";
        }
      }

      if (AsChar(local.NotPaRefrlPersonFlag.Flag) == 'Y')
      {
        ExitState = "SI0000_CASE_NOT_FROM_REFERRAL_1";
      }

      // ------------------------------------------------------------------
      // WR# 000160:  This code is to override the above EXIT STATE and also 
      // display the 'Successfully Updated' message.
      //                                            
      // Vithal.(  03/28/2000)
      // -------------------------------------------------------------------
      if (AsChar(local.OverrideExitstate.Flag) == 'Y')
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }

      export.HiddenForEvents.FatherShowInd =
        export.GeneticTestInformation.FatherShowInd;
      export.HiddenForEvents.MotherShowInd =
        export.GeneticTestInformation.MotherShowInd;
      export.HiddenForEvents.ChildShowInd =
        export.GeneticTestInformation.ChildShowInd;
      export.HiddenForEvents.PaternityExcludedInd =
        export.GeneticTestInformation.PaternityExcludedInd;
      export.HiddenForEvents.PaternityProbability =
        export.GeneticTestInformation.PaternityProbability;
      export.HiddenForEvents.FatherSchedTestDate =
        export.HiddenGeneticTestInformation.FatherSchedTestDate;
      export.HiddenForEvents.MotherSchedTestDate =
        export.HiddenGeneticTestInformation.MotherSchedTestDate;
      export.HiddenForEvents.ChildSchedTestDate =
        export.HiddenGeneticTestInformation.ChildSchedTestDate;
      export.HiddenForEvents.ResultReceivedDate =
        export.HiddenGeneticTestInformation.ResultReceivedDate;
    }

    // ******* End of Code added to support Events Insertions *******
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveErrorCodes1(Local.ErrorCodesGroup source,
    OeGtscValidateGeneticTest.Export.ErrorCodesGroup target)
  {
    target.DetailErrorCode.Count = source.EntryErrorCode.Count;
  }

  private static void MoveErrorCodes2(OeGtscValidateGeneticTest.Export.
    ErrorCodesGroup source, Local.ErrorCodesGroup target)
  {
    target.EntryErrorCode.Count = source.DetailErrorCode.Count;
  }

  private static void MoveExport1ToHiddenSelCombn(OeGtscGetCseNumber.Export.
    ExportGroup source, Export.HiddenSelCombnGroup target)
  {
    target.DetailHiddenCombnCsePerson.Number = source.DetailCsePerson.Number;
    target.DetailHiddenCombnCaseRole.Assign(source.DetailCaseRole);
  }

  private static void MoveGeneticTestInformation1(GeneticTestInformation source,
    GeneticTestInformation target)
  {
    target.ChildDob = source.ChildDob;
    target.CaseNumber = source.CaseNumber;
    target.CourtOrderNo = source.CourtOrderNo;
    target.GeneticTestAccountNo = source.GeneticTestAccountNo;
    target.LabCaseNo = source.LabCaseNo;
    target.TestType = source.TestType;
    target.FatherPersonNo = source.FatherPersonNo;
    target.FatherFormattedName = source.FatherFormattedName;
    target.FatherLastName = source.FatherLastName;
    target.FatherMi = source.FatherMi;
    target.FatherFirstName = source.FatherFirstName;
    target.FatherDrawSiteId = source.FatherDrawSiteId;
    target.FatherDrawSiteVendorName = source.FatherDrawSiteVendorName;
    target.FatherDrawSiteCity = source.FatherDrawSiteCity;
    target.FatherDrawSiteState = source.FatherDrawSiteState;
    target.FatherSchedTestDate = source.FatherSchedTestDate;
    target.FatherSchedTestTime = source.FatherSchedTestTime;
    target.FatherCollectSampleInd = source.FatherCollectSampleInd;
    target.FatherReuseSampleInd = source.FatherReuseSampleInd;
    target.FatherShowInd = source.FatherShowInd;
    target.FatherSampleCollectedInd = source.FatherSampleCollectedInd;
    target.FatherPrevSampExistsInd = source.FatherPrevSampExistsInd;
    target.FatherPrevSampGtestNumber = source.FatherPrevSampGtestNumber;
    target.FatherPrevSampTestType = source.FatherPrevSampTestType;
    target.FatherPrevSampleLabCaseNo = source.FatherPrevSampleLabCaseNo;
    target.FatherPrevSampSpecimenId = source.FatherPrevSampSpecimenId;
    target.FatherPrevSampPerGenTestId = source.FatherPrevSampPerGenTestId;
    target.FatherSpecimenId = source.FatherSpecimenId;
    target.FatherRescheduledInd = source.FatherRescheduledInd;
    target.MotherPersonNo = source.MotherPersonNo;
    target.MotherFormattedName = source.MotherFormattedName;
    target.MotherLastName = source.MotherLastName;
    target.MotherMi = source.MotherMi;
    target.MotherFirstName = source.MotherFirstName;
    target.MotherDrawSiteId = source.MotherDrawSiteId;
    target.MotherDrawSiteVendorName = source.MotherDrawSiteVendorName;
    target.MotherDrawSiteCity = source.MotherDrawSiteCity;
    target.MotherDrawSiteState = source.MotherDrawSiteState;
    target.MotherSchedTestDate = source.MotherSchedTestDate;
    target.MotherSchedTestTime = source.MotherSchedTestTime;
    target.MotherCollectSampleInd = source.MotherCollectSampleInd;
    target.MotherReuseSampleInd = source.MotherReuseSampleInd;
    target.MotherShowInd = source.MotherShowInd;
    target.MotherSampleCollectedInd = source.MotherSampleCollectedInd;
    target.MotherPrevSampExistsInd = source.MotherPrevSampExistsInd;
    target.MotherPrevSampGtestNumber = source.MotherPrevSampGtestNumber;
    target.MotherPrevSampTestType = source.MotherPrevSampTestType;
    target.MotherPrevSampLabCaseNo = source.MotherPrevSampLabCaseNo;
    target.MotherPrevSampSpecimenId = source.MotherPrevSampSpecimenId;
    target.MotherPrevSampPerGenTestId = source.MotherPrevSampPerGenTestId;
    target.MotherSpecimenId = source.MotherSpecimenId;
    target.MotherRescheduledInd = source.MotherRescheduledInd;
    target.ChildPersonNo = source.ChildPersonNo;
    target.ChildFormattedName = source.ChildFormattedName;
    target.ChildLastName = source.ChildLastName;
    target.ChildMi = source.ChildMi;
    target.ChildFirstName = source.ChildFirstName;
    target.ChildDrawSiteId = source.ChildDrawSiteId;
    target.ChildDrawSiteVendorName = source.ChildDrawSiteVendorName;
    target.ChildDrawSiteCity = source.ChildDrawSiteCity;
    target.ChildDrawSiteState = source.ChildDrawSiteState;
    target.ChildSchedTestDate = source.ChildSchedTestDate;
    target.ChildSchedTestTime = source.ChildSchedTestTime;
    target.ChildCollectSampleInd = source.ChildCollectSampleInd;
    target.ChildReuseSampleInd = source.ChildReuseSampleInd;
    target.ChildShowInd = source.ChildShowInd;
    target.ChildSampleCollectedInd = source.ChildSampleCollectedInd;
    target.ChildPrevSampExistsInd = source.ChildPrevSampExistsInd;
    target.ChildPrevSampGtestNumber = source.ChildPrevSampGtestNumber;
    target.ChildPrevSampTestType = source.ChildPrevSampTestType;
    target.ChildPrevSampLabCaseNo = source.ChildPrevSampLabCaseNo;
    target.ChildPrevSampSpecimenId = source.ChildPrevSampSpecimenId;
    target.ChildPrevSampPerGenTestId = source.ChildPrevSampPerGenTestId;
    target.ChildSpecimenId = source.ChildSpecimenId;
    target.ChildReschedInd = source.ChildReschedInd;
    target.TestSiteVendorId = source.TestSiteVendorId;
    target.TestSiteVendorName = source.TestSiteVendorName;
    target.TestSiteCity = source.TestSiteCity;
    target.TestSiteState = source.TestSiteState;
    target.ActualTestDate = source.ActualTestDate;
    target.ScheduledTestDate = source.ScheduledTestDate;
    target.ResultReceivedDate = source.ResultReceivedDate;
    target.PaternityExcludedInd = source.PaternityExcludedInd;
    target.PaternityProbability = source.PaternityProbability;
    target.ResultContestedInd = source.ResultContestedInd;
    target.ContestStartedDate = source.ContestStartedDate;
    target.ContestEndedDate = source.ContestEndedDate;
  }

  private static void MoveGeneticTestInformation2(GeneticTestInformation source,
    GeneticTestInformation target)
  {
    target.CaseNumber = source.CaseNumber;
    target.CourtOrderNo = source.CourtOrderNo;
    target.GeneticTestAccountNo = source.GeneticTestAccountNo;
    target.LabCaseNo = source.LabCaseNo;
    target.TestType = source.TestType;
    target.FatherPersonNo = source.FatherPersonNo;
    target.FatherFormattedName = source.FatherFormattedName;
    target.FatherLastName = source.FatherLastName;
    target.FatherMi = source.FatherMi;
    target.FatherFirstName = source.FatherFirstName;
    target.FatherDrawSiteId = source.FatherDrawSiteId;
    target.FatherDrawSiteVendorName = source.FatherDrawSiteVendorName;
    target.FatherDrawSiteCity = source.FatherDrawSiteCity;
    target.FatherDrawSiteState = source.FatherDrawSiteState;
    target.FatherSchedTestDate = source.FatherSchedTestDate;
    target.FatherSchedTestTime = source.FatherSchedTestTime;
    target.FatherCollectSampleInd = source.FatherCollectSampleInd;
    target.FatherReuseSampleInd = source.FatherReuseSampleInd;
    target.FatherShowInd = source.FatherShowInd;
    target.FatherSampleCollectedInd = source.FatherSampleCollectedInd;
    target.FatherPrevSampExistsInd = source.FatherPrevSampExistsInd;
    target.FatherPrevSampGtestNumber = source.FatherPrevSampGtestNumber;
    target.FatherPrevSampTestType = source.FatherPrevSampTestType;
    target.FatherPrevSampleLabCaseNo = source.FatherPrevSampleLabCaseNo;
    target.FatherPrevSampSpecimenId = source.FatherPrevSampSpecimenId;
    target.FatherPrevSampPerGenTestId = source.FatherPrevSampPerGenTestId;
    target.FatherSpecimenId = source.FatherSpecimenId;
    target.FatherRescheduledInd = source.FatherRescheduledInd;
    target.MotherPersonNo = source.MotherPersonNo;
    target.MotherFormattedName = source.MotherFormattedName;
    target.MotherLastName = source.MotherLastName;
    target.MotherMi = source.MotherMi;
    target.MotherFirstName = source.MotherFirstName;
    target.MotherDrawSiteId = source.MotherDrawSiteId;
    target.MotherDrawSiteVendorName = source.MotherDrawSiteVendorName;
    target.MotherDrawSiteCity = source.MotherDrawSiteCity;
    target.MotherDrawSiteState = source.MotherDrawSiteState;
    target.MotherSchedTestDate = source.MotherSchedTestDate;
    target.MotherSchedTestTime = source.MotherSchedTestTime;
    target.MotherCollectSampleInd = source.MotherCollectSampleInd;
    target.MotherReuseSampleInd = source.MotherReuseSampleInd;
    target.MotherShowInd = source.MotherShowInd;
    target.MotherSampleCollectedInd = source.MotherSampleCollectedInd;
    target.MotherPrevSampExistsInd = source.MotherPrevSampExistsInd;
    target.MotherPrevSampGtestNumber = source.MotherPrevSampGtestNumber;
    target.MotherPrevSampTestType = source.MotherPrevSampTestType;
    target.MotherPrevSampLabCaseNo = source.MotherPrevSampLabCaseNo;
    target.MotherPrevSampSpecimenId = source.MotherPrevSampSpecimenId;
    target.MotherPrevSampPerGenTestId = source.MotherPrevSampPerGenTestId;
    target.MotherSpecimenId = source.MotherSpecimenId;
    target.MotherRescheduledInd = source.MotherRescheduledInd;
    target.ChildPersonNo = source.ChildPersonNo;
    target.ChildFormattedName = source.ChildFormattedName;
    target.ChildLastName = source.ChildLastName;
    target.ChildMi = source.ChildMi;
    target.ChildFirstName = source.ChildFirstName;
    target.ChildDrawSiteId = source.ChildDrawSiteId;
    target.ChildDrawSiteVendorName = source.ChildDrawSiteVendorName;
    target.ChildDrawSiteCity = source.ChildDrawSiteCity;
    target.ChildDrawSiteState = source.ChildDrawSiteState;
    target.ChildSchedTestDate = source.ChildSchedTestDate;
    target.ChildSchedTestTime = source.ChildSchedTestTime;
    target.ChildCollectSampleInd = source.ChildCollectSampleInd;
    target.ChildReuseSampleInd = source.ChildReuseSampleInd;
    target.ChildShowInd = source.ChildShowInd;
    target.ChildSampleCollectedInd = source.ChildSampleCollectedInd;
    target.ChildPrevSampExistsInd = source.ChildPrevSampExistsInd;
    target.ChildPrevSampGtestNumber = source.ChildPrevSampGtestNumber;
    target.ChildPrevSampTestType = source.ChildPrevSampTestType;
    target.ChildPrevSampLabCaseNo = source.ChildPrevSampLabCaseNo;
    target.ChildPrevSampSpecimenId = source.ChildPrevSampSpecimenId;
    target.ChildPrevSampPerGenTestId = source.ChildPrevSampPerGenTestId;
    target.ChildSpecimenId = source.ChildSpecimenId;
    target.ChildReschedInd = source.ChildReschedInd;
    target.TestSiteVendorId = source.TestSiteVendorId;
    target.TestSiteVendorName = source.TestSiteVendorName;
    target.TestSiteCity = source.TestSiteCity;
    target.TestSiteState = source.TestSiteState;
    target.ActualTestDate = source.ActualTestDate;
    target.ScheduledTestDate = source.ScheduledTestDate;
    target.ResultReceivedDate = source.ResultReceivedDate;
    target.PaternityExcludedInd = source.PaternityExcludedInd;
    target.PaternityProbability = source.PaternityProbability;
    target.ResultContestedInd = source.ResultContestedInd;
    target.ContestStartedDate = source.ContestStartedDate;
    target.ContestEndedDate = source.ContestEndedDate;
  }

  private static void MoveGeneticTestInformation3(GeneticTestInformation source,
    GeneticTestInformation target)
  {
    target.CaseNumber = source.CaseNumber;
    target.CourtOrderNo = source.CourtOrderNo;
    target.FatherPersonNo = source.FatherPersonNo;
    target.FatherSchedTestDate = source.FatherSchedTestDate;
    target.FatherShowInd = source.FatherShowInd;
    target.MotherPersonNo = source.MotherPersonNo;
    target.MotherSchedTestDate = source.MotherSchedTestDate;
    target.MotherShowInd = source.MotherShowInd;
    target.ChildPersonNo = source.ChildPersonNo;
    target.ChildSchedTestDate = source.ChildSchedTestDate;
    target.ChildShowInd = source.ChildShowInd;
    target.ResultReceivedDate = source.ResultReceivedDate;
    target.PaternityExcludedInd = source.PaternityExcludedInd;
    target.PaternityProbability = source.PaternityProbability;
  }

  private static void MoveGeneticTestInformation4(GeneticTestInformation source,
    GeneticTestInformation target)
  {
    target.CourtOrderNo = source.CourtOrderNo;
    target.GeneticTestAccountNo = source.GeneticTestAccountNo;
    target.FatherDrawSiteId = source.FatherDrawSiteId;
    target.FatherSchedTestDate = source.FatherSchedTestDate;
    target.FatherSchedTestTime = source.FatherSchedTestTime;
    target.FatherShowInd = source.FatherShowInd;
    target.MotherDrawSiteId = source.MotherDrawSiteId;
    target.MotherSchedTestDate = source.MotherSchedTestDate;
    target.MotherSchedTestTime = source.MotherSchedTestTime;
    target.MotherShowInd = source.MotherShowInd;
    target.ChildDrawSiteId = source.ChildDrawSiteId;
    target.ChildSchedTestDate = source.ChildSchedTestDate;
    target.ChildSchedTestTime = source.ChildSchedTestTime;
    target.ChildShowInd = source.ChildShowInd;
    target.TestSiteVendorId = source.TestSiteVendorId;
    target.ResultReceivedDate = source.ResultReceivedDate;
    target.PaternityExcludedInd = source.PaternityExcludedInd;
    target.PrevPaternityExcludedInd = source.PrevPaternityExcludedInd;
    target.PaternityProbability = source.PaternityProbability;
  }

  private static void MoveGeneticTestInformation5(GeneticTestInformation source,
    GeneticTestInformation target)
  {
    target.FatherPersonNo = source.FatherPersonNo;
    target.MotherPersonNo = source.MotherPersonNo;
    target.ChildPersonNo = source.ChildPersonNo;
  }

  private static void MoveGeneticTestInformation6(GeneticTestInformation source,
    GeneticTestInformation target)
  {
    target.FatherSchedTestDate = source.FatherSchedTestDate;
    target.FatherShowInd = source.FatherShowInd;
    target.MotherSchedTestDate = source.MotherSchedTestDate;
    target.MotherShowInd = source.MotherShowInd;
    target.ChildSchedTestDate = source.ChildSchedTestDate;
    target.ChildShowInd = source.ChildShowInd;
    target.ResultReceivedDate = source.ResultReceivedDate;
    target.PaternityExcludedInd = source.PaternityExcludedInd;
    target.PaternityProbability = source.PaternityProbability;
    target.ResultContestedInd = source.ResultContestedInd;
    target.ContestStartedDate = source.ContestStartedDate;
    target.ContestEndedDate = source.ContestEndedDate;
  }

  private static void MoveGeneticTestInformation7(GeneticTestInformation source,
    GeneticTestInformation target)
  {
    target.FatherSchedTestDate = source.FatherSchedTestDate;
    target.MotherSchedTestDate = source.MotherSchedTestDate;
    target.ChildSchedTestDate = source.ChildSchedTestDate;
  }

  private static void MoveHiddenSelCombnToImport2(Import.
    HiddenSelCombnGroup source, OeGtscDispFaMoChComb.Import.ImportGroup target)
  {
    target.DetailCsePerson.Number = source.DetailHiddenCombnCsePerson.Number;
    target.DetailCaseRole.Assign(source.DetailHiddenCombnCaseRole);
  }

  private static void MoveHiddenSelCombnToImport3(Export.
    HiddenSelCombnGroup source, OeGtscDispFaMoChComb.Import.ImportGroup target)
  {
    target.DetailCsePerson.Number = source.DetailHiddenCombnCsePerson.Number;
    target.DetailCaseRole.Assign(source.DetailHiddenCombnCaseRole);
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdGenetic = source.IdGenetic;
  }

  private static void MoveVendorAddress(VendorAddress source,
    VendorAddress target)
  {
    target.City = source.City;
    target.State = source.State;
  }

  private static void MoveWorkTime(WorkTime source, WorkTime target)
  {
    target.Wtime = source.Wtime;
    target.TimeWithAmPm = source.TimeWithAmPm;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Date.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.DateText.Text8 = useExport.TextWorkArea.Text8;
  }

  private void UseCabConvertTimeFormat()
  {
    var useImport = new CabConvertTimeFormat.Import();
    var useExport = new CabConvertTimeFormat.Export();

    MoveWorkTime(local.WorkTime, useImport.WorkTime);

    Call(CabConvertTimeFormat.Execute, useImport, useExport);

    local.ErrorInTimeConversion.Flag = useExport.ErrorInConversion.Flag;
    MoveWorkTime(useExport.WorkTime, local.WorkTime);
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Code, useImport.Code);
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    export.GeneticTestType.Description = useExport.CodeValue.Description;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabGetVendorAddress()
  {
    var useImport = new OeCabGetVendorAddress.Import();
    var useExport = new OeCabGetVendorAddress.Export();

    useImport.Vendor.Identifier = import.SelectedVendor.Identifier;

    Call(OeCabGetVendorAddress.Execute, useImport, useExport);

    MoveVendorAddress(useExport.VendorAddress, local.Selected);
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

  private void UseOeGtscDispFaMoChComb1()
  {
    var useImport = new OeGtscDispFaMoChComb.Import();
    var useExport = new OeGtscDispFaMoChComb.Export();

    useImport.Case1.Number = import.SelectedCase.Number;
    import.HiddenSelCombn.
      CopyTo(useImport.Import1, MoveHiddenSelCombnToImport2);

    Call(OeGtscDispFaMoChComb.Execute, useImport, useExport);

    export.Recieved.ExpiryDate = useExport.PassTestSite.ExpiryDate;
    export.ReceivedFa.ExpiryDate = useExport.PassFaDrawSite.ExpiryDate;
    export.ReceivedMo.ExpiryDate = useExport.PassMoDrawSite.ExpiryDate;
    export.RecievedCh.ExpiryDate = useExport.PassChDrawSite.ExpiryDate;
    export.HiddenGeneticTest.TestNumber = useExport.GeneticTest.TestNumber;
    MoveLegalAction3(useExport.PatEstab, export.HiddenSelected);
    export.GeneticTestInformation.Assign(useExport.GeneticTestInformation);
  }

  private void UseOeGtscDispFaMoChComb2()
  {
    var useImport = new OeGtscDispFaMoChComb.Import();
    var useExport = new OeGtscDispFaMoChComb.Export();

    useImport.Case1.Number = export.Selected.Number;
    export.HiddenSelCombn.
      CopyTo(useImport.Import1, MoveHiddenSelCombnToImport3);

    Call(OeGtscDispFaMoChComb.Execute, useImport, useExport);

    export.Recieved.ExpiryDate = useExport.PassTestSite.ExpiryDate;
    export.ReceivedFa.ExpiryDate = useExport.PassFaDrawSite.ExpiryDate;
    export.ReceivedMo.ExpiryDate = useExport.PassMoDrawSite.ExpiryDate;
    export.RecievedCh.ExpiryDate = useExport.PassChDrawSite.ExpiryDate;
    export.ActiveAp.Flag = useExport.ActiveAp.Flag;
    export.ActiveChild.Flag = useExport.ActiveChild.Flag;
    export.CaseRoleInactive.Flag = useExport.CaseRoleInactive.Flag;
    export.HiddenGeneticTest.TestNumber = useExport.GeneticTest.TestNumber;
    MoveLegalAction3(useExport.PatEstab, export.HiddenSelected);
    export.GeneticTestInformation.Assign(useExport.GeneticTestInformation);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
  }

  private void UseOeGtscGetCseNumber()
  {
    var useImport = new OeGtscGetCseNumber.Import();
    var useExport = new OeGtscGetCseNumber.Export();

    useImport.Case1.Number = local.Case1.Number;
    useImport.CaseUnit.CuNumber = local.CaseUnit.CuNumber;

    Call(OeGtscGetCseNumber.Execute, useImport, useExport);

    useExport.Export1.
      CopyTo(export.HiddenSelCombn, MoveExport1ToHiddenSelCombn);
  }

  private void UseOeGtscGetLatestCaseRole()
  {
    var useImport = new OeGtscGetLatestCaseRole.Import();
    var useExport = new OeGtscGetLatestCaseRole.Export();

    useImport.Case1.Number = export.Selected.Number;
    useImport.CsePerson.Number =
      export.HiddenSelCombn.Item.DetailHiddenCombnCsePerson.Number;
    useImport.CaseRole.Type1 =
      export.HiddenSelCombn.Item.DetailHiddenCombnCaseRole.Type1;

    Call(OeGtscGetLatestCaseRole.Execute, useImport, useExport);

    export.HiddenSelCombn.Update.DetailHiddenCombnCaseRole.Assign(
      useExport.CaseRole);
  }

  private void UseOeGtscRaiseEvents()
  {
    var useImport = new OeGtscRaiseEvents.Import();
    var useExport = new OeGtscRaiseEvents.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);
    MoveGeneticTestInformation3(export.GeneticTestInformation,
      useImport.GeneticTestInformation);

    Call(OeGtscRaiseEvents.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseOeGtscUpdateGenTestDetls()
  {
    var useImport = new OeGtscUpdateGenTestDetls.Import();
    var useExport = new OeGtscUpdateGenTestDetls.Export();

    MoveLegalAction3(export.HiddenSelected, useImport.LegalAction);
    useImport.GeneticTestInformation.Assign(export.GeneticTestInformation);

    Call(OeGtscUpdateGenTestDetls.Execute, useImport, useExport);

    MoveGeneticTestInformation1(useExport.GeneticTestInformation,
      export.GeneticTestInformation);
  }

  private void UseOeGtscValidateGeneticTest()
  {
    var useImport = new OeGtscValidateGeneticTest.Import();
    var useExport = new OeGtscValidateGeneticTest.Export();

    useImport.Standard.Command = local.Standard.Command;
    useImport.PatEstab.Assign(export.HiddenSelected);
    useImport.GeneticTestInformation.Assign(export.GeneticTestInformation);
    local.ErrorCodes.CopyTo(useExport.ErrorCodes, MoveErrorCodes1);

    Call(OeGtscValidateGeneticTest.Execute, useImport, useExport);

    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes2);
    MoveGeneticTestInformation2(useExport.GeneticTestInformation,
      export.GeneticTestInformation);
  }

  private void UseOeScheduleGeneticTest()
  {
    var useImport = new OeScheduleGeneticTest.Import();
    var useExport = new OeScheduleGeneticTest.Export();

    MoveLegalAction3(export.HiddenSelected, useImport.LegalAction);
    useImport.GeneticTestInformation.Assign(export.GeneticTestInformation);

    Call(OeScheduleGeneticTest.Execute, useImport, useExport);

    MoveGeneticTestInformation2(useExport.GeneticTestInformation,
      export.GeneticTestInformation);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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

    MoveLegalAction2(import.SelectedLegalAction, useImport.LegalAction);
    useImport.CsePersonsWorkSet.Number = import.SelectedChild.Number;
    useImport.Case1.Number = export.Selected.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.Zdel.Number = local.Ap.Number;
    MoveLegalAction1(export.HiddenSelected, useImport.LegalAction);
    useImport.Case1.Number = export.Selected.Number;
    MoveGeneticTestInformation5(export.GeneticTestInformation,
      useImport.GeneticTestInformation);

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Selected.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.Office);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
  }

  private void UseSpGtscExternalAlert()
  {
    var useImport = new SpGtscExternalAlert.Import();
    var useExport = new SpGtscExternalAlert.Export();

    useImport.InterfaceAlert.AlertCode = local.InterfaceAlert.AlertCode;
    useImport.Ap.Number = local.Ap.Number;
    useImport.Ar.Number = local.Ar.Number;
    useImport.Child.Number = local.Ch.Number;
    useImport.Case1.Number = local.Case1.Number;

    Call(SpGtscExternalAlert.Execute, useImport, useExport);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.Print.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.Print.Text50 = useExport.WorkArea.Text50;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", export.HiddenGeneticTest.TestNumber);
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
        db.SetInt32(command, "testNumber", export.HiddenGeneticTest.TestNumber);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
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
        db.SetInt32(command, "testNumber", export.HiddenGeneticTest.TestNumber);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
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
        db.SetInt32(command, "testNumber", export.HiddenGeneticTest.TestNumber);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", export.GeneticTestInformation.ChildPersonNo);
        db.SetNullableString(
          command, "gtaAccountNumber",
          export.GeneticTestInformation.GeneticTestAccountNo);
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
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
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

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.ExistingPatEstabOrder.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.GeneticTestInformation.CourtOrderNo);
      },
      (db, reader) =>
      {
        entities.ExistingPatEstabOrder.Identifier = db.GetInt32(reader, 0);
        entities.ExistingPatEstabOrder.Classification = db.GetString(reader, 1);
        entities.ExistingPatEstabOrder.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingPatEstabOrder.Populated = true;

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
    /// <summary>A HiddenSelCombnGroup group.</summary>
    [Serializable]
    public class HiddenSelCombnGroup
    {
      /// <summary>
      /// A value of DetailHiddenCombnCsePerson.
      /// </summary>
      [JsonPropertyName("detailHiddenCombnCsePerson")]
      public CsePerson DetailHiddenCombnCsePerson
      {
        get => detailHiddenCombnCsePerson ??= new();
        set => detailHiddenCombnCsePerson = value;
      }

      /// <summary>
      /// A value of DetailHiddenCombnCaseRole.
      /// </summary>
      [JsonPropertyName("detailHiddenCombnCaseRole")]
      public CaseRole DetailHiddenCombnCaseRole
      {
        get => detailHiddenCombnCaseRole ??= new();
        set => detailHiddenCombnCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePerson detailHiddenCombnCsePerson;
      private CaseRole detailHiddenCombnCaseRole;
    }

    /// <summary>
    /// A value of ActiveAp.
    /// </summary>
    [JsonPropertyName("activeAp")]
    public Common ActiveAp
    {
      get => activeAp ??= new();
      set => activeAp = value;
    }

    /// <summary>
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    /// <summary>
    /// A value of CaseRoleInactive.
    /// </summary>
    [JsonPropertyName("caseRoleInactive")]
    public Common CaseRoleInactive
    {
      get => caseRoleInactive ??= new();
      set => caseRoleInactive = value;
    }

    /// <summary>
    /// A value of HiddenForEvents.
    /// </summary>
    [JsonPropertyName("hiddenForEvents")]
    public GeneticTestInformation HiddenForEvents
    {
      get => hiddenForEvents ??= new();
      set => hiddenForEvents = value;
    }

    /// <summary>
    /// A value of HiddenGeneticTest.
    /// </summary>
    [JsonPropertyName("hiddenGeneticTest")]
    public GeneticTest HiddenGeneticTest
    {
      get => hiddenGeneticTest ??= new();
      set => hiddenGeneticTest = value;
    }

    /// <summary>
    /// A value of HiddenGeneticTestInformation.
    /// </summary>
    [JsonPropertyName("hiddenGeneticTestInformation")]
    public GeneticTestInformation HiddenGeneticTestInformation
    {
      get => hiddenGeneticTestInformation ??= new();
      set => hiddenGeneticTestInformation = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Document Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of GeneticTestType.
    /// </summary>
    [JsonPropertyName("geneticTestType")]
    public CodeValue GeneticTestType
    {
      get => geneticTestType ??= new();
      set => geneticTestType = value;
    }

    /// <summary>
    /// A value of SelectedLegalAction.
    /// </summary>
    [JsonPropertyName("selectedLegalAction")]
    public LegalAction SelectedLegalAction
    {
      get => selectedLegalAction ??= new();
      set => selectedLegalAction = value;
    }

    /// <summary>
    /// A value of ListLegalActionsLacs.
    /// </summary>
    [JsonPropertyName("listLegalActionsLacs")]
    public Standard ListLegalActionsLacs
    {
      get => listLegalActionsLacs ??= new();
      set => listLegalActionsLacs = value;
    }

    /// <summary>
    /// A value of SelectedChild.
    /// </summary>
    [JsonPropertyName("selectedChild")]
    public CsePersonsWorkSet SelectedChild
    {
      get => selectedChild ??= new();
      set => selectedChild = value;
    }

    /// <summary>
    /// A value of SelectedMother.
    /// </summary>
    [JsonPropertyName("selectedMother")]
    public CsePersonsWorkSet SelectedMother
    {
      get => selectedMother ??= new();
      set => selectedMother = value;
    }

    /// <summary>
    /// A value of SelectedAllegedFather.
    /// </summary>
    [JsonPropertyName("selectedAllegedFather")]
    public CsePersonsWorkSet SelectedAllegedFather
    {
      get => selectedAllegedFather ??= new();
      set => selectedAllegedFather = value;
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// A value of SelectedGeneticTestAccount.
    /// </summary>
    [JsonPropertyName("selectedGeneticTestAccount")]
    public GeneticTestAccount SelectedGeneticTestAccount
    {
      get => selectedGeneticTestAccount ??= new();
      set => selectedGeneticTestAccount = value;
    }

    /// <summary>
    /// A value of ListGenTestAccount.
    /// </summary>
    [JsonPropertyName("listGenTestAccount")]
    public Standard ListGenTestAccount
    {
      get => listGenTestAccount ??= new();
      set => listGenTestAccount = value;
    }

    /// <summary>
    /// A value of ListGenTestType.
    /// </summary>
    [JsonPropertyName("listGenTestType")]
    public Standard ListGenTestType
    {
      get => listGenTestType ??= new();
      set => listGenTestType = value;
    }

    /// <summary>
    /// A value of ListDrawSiteFather.
    /// </summary>
    [JsonPropertyName("listDrawSiteFather")]
    public Standard ListDrawSiteFather
    {
      get => listDrawSiteFather ??= new();
      set => listDrawSiteFather = value;
    }

    /// <summary>
    /// A value of ListDrawSiteMother.
    /// </summary>
    [JsonPropertyName("listDrawSiteMother")]
    public Standard ListDrawSiteMother
    {
      get => listDrawSiteMother ??= new();
      set => listDrawSiteMother = value;
    }

    /// <summary>
    /// A value of ListDrawSiteChild.
    /// </summary>
    [JsonPropertyName("listDrawSiteChild")]
    public Standard ListDrawSiteChild
    {
      get => listDrawSiteChild ??= new();
      set => listDrawSiteChild = value;
    }

    /// <summary>
    /// A value of ListPrevSampFather.
    /// </summary>
    [JsonPropertyName("listPrevSampFather")]
    public Standard ListPrevSampFather
    {
      get => listPrevSampFather ??= new();
      set => listPrevSampFather = value;
    }

    /// <summary>
    /// A value of ListPrevSampMother.
    /// </summary>
    [JsonPropertyName("listPrevSampMother")]
    public Standard ListPrevSampMother
    {
      get => listPrevSampMother ??= new();
      set => listPrevSampMother = value;
    }

    /// <summary>
    /// A value of ListPrevSampChild.
    /// </summary>
    [JsonPropertyName("listPrevSampChild")]
    public Standard ListPrevSampChild
    {
      get => listPrevSampChild ??= new();
      set => listPrevSampChild = value;
    }

    /// <summary>
    /// A value of ListTestSite.
    /// </summary>
    [JsonPropertyName("listTestSite")]
    public Standard ListTestSite
    {
      get => listTestSite ??= new();
      set => listTestSite = value;
    }

    /// <summary>
    /// A value of SelectedPrevSampleVendorAddress.
    /// </summary>
    [JsonPropertyName("selectedPrevSampleVendorAddress")]
    public VendorAddress SelectedPrevSampleVendorAddress
    {
      get => selectedPrevSampleVendorAddress ??= new();
      set => selectedPrevSampleVendorAddress = value;
    }

    /// <summary>
    /// A value of SelectedPrevSampleVendor.
    /// </summary>
    [JsonPropertyName("selectedPrevSampleVendor")]
    public Vendor SelectedPrevSampleVendor
    {
      get => selectedPrevSampleVendor ??= new();
      set => selectedPrevSampleVendor = value;
    }

    /// <summary>
    /// A value of SelectedPrevSampleGeneticTest.
    /// </summary>
    [JsonPropertyName("selectedPrevSampleGeneticTest")]
    public GeneticTest SelectedPrevSampleGeneticTest
    {
      get => selectedPrevSampleGeneticTest ??= new();
      set => selectedPrevSampleGeneticTest = value;
    }

    /// <summary>
    /// A value of SelectedPrevSamplePersonGeneticTest.
    /// </summary>
    [JsonPropertyName("selectedPrevSamplePersonGeneticTest")]
    public PersonGeneticTest SelectedPrevSamplePersonGeneticTest
    {
      get => selectedPrevSamplePersonGeneticTest ??= new();
      set => selectedPrevSamplePersonGeneticTest = value;
    }

    /// <summary>
    /// A value of HiddenListPrevSampCh.
    /// </summary>
    [JsonPropertyName("hiddenListPrevSampCh")]
    public Common HiddenListPrevSampCh
    {
      get => hiddenListPrevSampCh ??= new();
      set => hiddenListPrevSampCh = value;
    }

    /// <summary>
    /// A value of HiddenListPrevSampMo.
    /// </summary>
    [JsonPropertyName("hiddenListPrevSampMo")]
    public Common HiddenListPrevSampMo
    {
      get => hiddenListPrevSampMo ??= new();
      set => hiddenListPrevSampMo = value;
    }

    /// <summary>
    /// A value of HiddenListPrevSampFa.
    /// </summary>
    [JsonPropertyName("hiddenListPrevSampFa")]
    public Common HiddenListPrevSampFa
    {
      get => hiddenListPrevSampFa ??= new();
      set => hiddenListPrevSampFa = value;
    }

    /// <summary>
    /// A value of SelectedVendorAddress.
    /// </summary>
    [JsonPropertyName("selectedVendorAddress")]
    public VendorAddress SelectedVendorAddress
    {
      get => selectedVendorAddress ??= new();
      set => selectedVendorAddress = value;
    }

    /// <summary>
    /// A value of SelectedCase.
    /// </summary>
    [JsonPropertyName("selectedCase")]
    public Case1 SelectedCase
    {
      get => selectedCase ??= new();
      set => selectedCase = value;
    }

    /// <summary>
    /// A value of SelectedVendor.
    /// </summary>
    [JsonPropertyName("selectedVendor")]
    public Vendor SelectedVendor
    {
      get => selectedVendor ??= new();
      set => selectedVendor = value;
    }

    /// <summary>
    /// A value of GeneticTestInformation.
    /// </summary>
    [JsonPropertyName("geneticTestInformation")]
    public GeneticTestInformation GeneticTestInformation
    {
      get => geneticTestInformation ??= new();
      set => geneticTestInformation = value;
    }

    /// <summary>
    /// Gets a value of HiddenSelCombn.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSelCombnGroup> HiddenSelCombn => hiddenSelCombn ??= new(
      HiddenSelCombnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenSelCombn for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSelCombn")]
    [Computed]
    public IList<HiddenSelCombnGroup> HiddenSelCombn_Json
    {
      get => hiddenSelCombn;
      set => HiddenSelCombn.Assign(value);
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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
    /// A value of HiddenComp.
    /// </summary>
    [JsonPropertyName("hiddenComp")]
    public Common HiddenComp
    {
      get => hiddenComp ??= new();
      set => hiddenComp = value;
    }

    private Common activeAp;
    private Common activeChild;
    private Common caseRoleInactive;
    private GeneticTestInformation hiddenForEvents;
    private GeneticTest hiddenGeneticTest;
    private GeneticTestInformation hiddenGeneticTestInformation;
    private Document print;
    private CodeValue geneticTestType;
    private LegalAction selectedLegalAction;
    private Standard listLegalActionsLacs;
    private CsePersonsWorkSet selectedChild;
    private CsePersonsWorkSet selectedMother;
    private CsePersonsWorkSet selectedAllegedFather;
    private CodeValue dlgflwSelected;
    private GeneticTestAccount selectedGeneticTestAccount;
    private Standard listGenTestAccount;
    private Standard listGenTestType;
    private Standard listDrawSiteFather;
    private Standard listDrawSiteMother;
    private Standard listDrawSiteChild;
    private Standard listPrevSampFather;
    private Standard listPrevSampMother;
    private Standard listPrevSampChild;
    private Standard listTestSite;
    private VendorAddress selectedPrevSampleVendorAddress;
    private Vendor selectedPrevSampleVendor;
    private GeneticTest selectedPrevSampleGeneticTest;
    private PersonGeneticTest selectedPrevSamplePersonGeneticTest;
    private Common hiddenListPrevSampCh;
    private Common hiddenListPrevSampMo;
    private Common hiddenListPrevSampFa;
    private VendorAddress selectedVendorAddress;
    private Case1 selectedCase;
    private Vendor selectedVendor;
    private GeneticTestInformation geneticTestInformation;
    private Array<HiddenSelCombnGroup> hiddenSelCombn;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private WorkArea headerLine;
    private Common hiddenComp;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenSelCombnGroup group.</summary>
    [Serializable]
    public class HiddenSelCombnGroup
    {
      /// <summary>
      /// A value of DetailHiddenCombnCsePerson.
      /// </summary>
      [JsonPropertyName("detailHiddenCombnCsePerson")]
      public CsePerson DetailHiddenCombnCsePerson
      {
        get => detailHiddenCombnCsePerson ??= new();
        set => detailHiddenCombnCsePerson = value;
      }

      /// <summary>
      /// A value of DetailHiddenCombnCaseRole.
      /// </summary>
      [JsonPropertyName("detailHiddenCombnCaseRole")]
      public CaseRole DetailHiddenCombnCaseRole
      {
        get => detailHiddenCombnCaseRole ??= new();
        set => detailHiddenCombnCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePerson detailHiddenCombnCsePerson;
      private CaseRole detailHiddenCombnCaseRole;
    }

    /// <summary>
    /// A value of DocmProtectFilter.
    /// </summary>
    [JsonPropertyName("docmProtectFilter")]
    public Common DocmProtectFilter
    {
      get => docmProtectFilter ??= new();
      set => docmProtectFilter = value;
    }

    /// <summary>
    /// A value of ActiveAp.
    /// </summary>
    [JsonPropertyName("activeAp")]
    public Common ActiveAp
    {
      get => activeAp ??= new();
      set => activeAp = value;
    }

    /// <summary>
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    /// <summary>
    /// A value of CaseRoleInactive.
    /// </summary>
    [JsonPropertyName("caseRoleInactive")]
    public Common CaseRoleInactive
    {
      get => caseRoleInactive ??= new();
      set => caseRoleInactive = value;
    }

    /// <summary>
    /// A value of HiddenForEvents.
    /// </summary>
    [JsonPropertyName("hiddenForEvents")]
    public GeneticTestInformation HiddenForEvents
    {
      get => hiddenForEvents ??= new();
      set => hiddenForEvents = value;
    }

    /// <summary>
    /// A value of HiddenGeneticTest.
    /// </summary>
    [JsonPropertyName("hiddenGeneticTest")]
    public GeneticTest HiddenGeneticTest
    {
      get => hiddenGeneticTest ??= new();
      set => hiddenGeneticTest = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Document Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of HiddenGeneticTestInformation.
    /// </summary>
    [JsonPropertyName("hiddenGeneticTestInformation")]
    public GeneticTestInformation HiddenGeneticTestInformation
    {
      get => hiddenGeneticTestInformation ??= new();
      set => hiddenGeneticTestInformation = value;
    }

    /// <summary>
    /// A value of GeneticTestType.
    /// </summary>
    [JsonPropertyName("geneticTestType")]
    public CodeValue GeneticTestType
    {
      get => geneticTestType ??= new();
      set => geneticTestType = value;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public LegalAction HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// A value of ListLegalActionsLacs.
    /// </summary>
    [JsonPropertyName("listLegalActionsLacs")]
    public Standard ListLegalActionsLacs
    {
      get => listLegalActionsLacs ??= new();
      set => listLegalActionsLacs = value;
    }

    /// <summary>
    /// A value of SelectedForList.
    /// </summary>
    [JsonPropertyName("selectedForList")]
    public Code SelectedForList
    {
      get => selectedForList ??= new();
      set => selectedForList = value;
    }

    /// <summary>
    /// A value of ListGenTestAccount.
    /// </summary>
    [JsonPropertyName("listGenTestAccount")]
    public Standard ListGenTestAccount
    {
      get => listGenTestAccount ??= new();
      set => listGenTestAccount = value;
    }

    /// <summary>
    /// A value of ListGenTestType.
    /// </summary>
    [JsonPropertyName("listGenTestType")]
    public Standard ListGenTestType
    {
      get => listGenTestType ??= new();
      set => listGenTestType = value;
    }

    /// <summary>
    /// A value of ListDrawSiteFather.
    /// </summary>
    [JsonPropertyName("listDrawSiteFather")]
    public Standard ListDrawSiteFather
    {
      get => listDrawSiteFather ??= new();
      set => listDrawSiteFather = value;
    }

    /// <summary>
    /// A value of ListDrawSiteMother.
    /// </summary>
    [JsonPropertyName("listDrawSiteMother")]
    public Standard ListDrawSiteMother
    {
      get => listDrawSiteMother ??= new();
      set => listDrawSiteMother = value;
    }

    /// <summary>
    /// A value of ListDrawSiteChild.
    /// </summary>
    [JsonPropertyName("listDrawSiteChild")]
    public Standard ListDrawSiteChild
    {
      get => listDrawSiteChild ??= new();
      set => listDrawSiteChild = value;
    }

    /// <summary>
    /// A value of ListPrevSampFather.
    /// </summary>
    [JsonPropertyName("listPrevSampFather")]
    public Standard ListPrevSampFather
    {
      get => listPrevSampFather ??= new();
      set => listPrevSampFather = value;
    }

    /// <summary>
    /// A value of ListPrevSampMother.
    /// </summary>
    [JsonPropertyName("listPrevSampMother")]
    public Standard ListPrevSampMother
    {
      get => listPrevSampMother ??= new();
      set => listPrevSampMother = value;
    }

    /// <summary>
    /// A value of ListPrevSampChild.
    /// </summary>
    [JsonPropertyName("listPrevSampChild")]
    public Standard ListPrevSampChild
    {
      get => listPrevSampChild ??= new();
      set => listPrevSampChild = value;
    }

    /// <summary>
    /// A value of ListTestSite.
    /// </summary>
    [JsonPropertyName("listTestSite")]
    public Standard ListTestSite
    {
      get => listTestSite ??= new();
      set => listTestSite = value;
    }

    /// <summary>
    /// A value of SelectedForPrevSample.
    /// </summary>
    [JsonPropertyName("selectedForPrevSample")]
    public CsePerson SelectedForPrevSample
    {
      get => selectedForPrevSample ??= new();
      set => selectedForPrevSample = value;
    }

    /// <summary>
    /// A value of HiddenListPrevSampCh.
    /// </summary>
    [JsonPropertyName("hiddenListPrevSampCh")]
    public Common HiddenListPrevSampCh
    {
      get => hiddenListPrevSampCh ??= new();
      set => hiddenListPrevSampCh = value;
    }

    /// <summary>
    /// A value of HiddenListPrevSampMo.
    /// </summary>
    [JsonPropertyName("hiddenListPrevSampMo")]
    public Common HiddenListPrevSampMo
    {
      get => hiddenListPrevSampMo ??= new();
      set => hiddenListPrevSampMo = value;
    }

    /// <summary>
    /// A value of HiddenListPrevSampFa.
    /// </summary>
    [JsonPropertyName("hiddenListPrevSampFa")]
    public Common HiddenListPrevSampFa
    {
      get => hiddenListPrevSampFa ??= new();
      set => hiddenListPrevSampFa = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Case1 Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of GeneticTestInformation.
    /// </summary>
    [JsonPropertyName("geneticTestInformation")]
    public GeneticTestInformation GeneticTestInformation
    {
      get => geneticTestInformation ??= new();
      set => geneticTestInformation = value;
    }

    /// <summary>
    /// Gets a value of HiddenSelCombn.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSelCombnGroup> HiddenSelCombn => hiddenSelCombn ??= new(
      HiddenSelCombnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenSelCombn for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSelCombn")]
    [Computed]
    public IList<HiddenSelCombnGroup> HiddenSelCombn_Json
    {
      get => hiddenSelCombn;
      set => HiddenSelCombn.Assign(value);
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of SelectedMother.
    /// </summary>
    [JsonPropertyName("selectedMother")]
    public CsePersonsWorkSet SelectedMother
    {
      get => selectedMother ??= new();
      set => selectedMother = value;
    }

    /// <summary>
    /// A value of Recieved.
    /// </summary>
    [JsonPropertyName("recieved")]
    public VendorAddress Recieved
    {
      get => recieved ??= new();
      set => recieved = value;
    }

    /// <summary>
    /// A value of ReceivedFa.
    /// </summary>
    [JsonPropertyName("receivedFa")]
    public VendorAddress ReceivedFa
    {
      get => receivedFa ??= new();
      set => receivedFa = value;
    }

    /// <summary>
    /// A value of ReceivedMo.
    /// </summary>
    [JsonPropertyName("receivedMo")]
    public VendorAddress ReceivedMo
    {
      get => receivedMo ??= new();
      set => receivedMo = value;
    }

    /// <summary>
    /// A value of RecievedCh.
    /// </summary>
    [JsonPropertyName("recievedCh")]
    public VendorAddress RecievedCh
    {
      get => recievedCh ??= new();
      set => recievedCh = value;
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
    /// A value of HiddenComp.
    /// </summary>
    [JsonPropertyName("hiddenComp")]
    public Common HiddenComp
    {
      get => hiddenComp ??= new();
      set => hiddenComp = value;
    }

    private Common docmProtectFilter;
    private Common activeAp;
    private Common activeChild;
    private Common caseRoleInactive;
    private GeneticTestInformation hiddenForEvents;
    private GeneticTest hiddenGeneticTest;
    private Document print;
    private GeneticTestInformation hiddenGeneticTestInformation;
    private CodeValue geneticTestType;
    private LegalAction hiddenSelected;
    private Standard listLegalActionsLacs;
    private Code selectedForList;
    private Standard listGenTestAccount;
    private Standard listGenTestType;
    private Standard listDrawSiteFather;
    private Standard listDrawSiteMother;
    private Standard listDrawSiteChild;
    private Standard listPrevSampFather;
    private Standard listPrevSampMother;
    private Standard listPrevSampChild;
    private Standard listTestSite;
    private CsePerson selectedForPrevSample;
    private Common hiddenListPrevSampCh;
    private Common hiddenListPrevSampMo;
    private Common hiddenListPrevSampFa;
    private Case1 selected;
    private GeneticTestInformation geneticTestInformation;
    private Array<HiddenSelCombnGroup> hiddenSelCombn;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private ServiceProvider serviceProvider;
    private Office office;
    private Common caseOpen;
    private CsePersonsWorkSet selectedMother;
    private VendorAddress recieved;
    private VendorAddress receivedFa;
    private VendorAddress receivedMo;
    private VendorAddress recievedCh;
    private WorkArea headerLine;
    private Common hiddenComp;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of EntryErrorCode.
      /// </summary>
      [JsonPropertyName("entryErrorCode")]
      public Common EntryErrorCode
      {
        get => entryErrorCode ??= new();
        set => entryErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common entryErrorCode;
    }

    /// <summary>
    /// A value of NullNextTranInfo.
    /// </summary>
    [JsonPropertyName("nullNextTranInfo")]
    public NextTranInfo NullNextTranInfo
    {
      get => nullNextTranInfo ??= new();
      set => nullNextTranInfo = value;
    }

    /// <summary>
    /// A value of Specific.
    /// </summary>
    [JsonPropertyName("specific")]
    public InterstateRequestHistory Specific
    {
      get => specific ??= new();
      set => specific = value;
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
    /// A value of ResetValues.
    /// </summary>
    [JsonPropertyName("resetValues")]
    public GeneticTestInformation ResetValues
    {
      get => resetValues ??= new();
      set => resetValues = value;
    }

    /// <summary>
    /// A value of CaseChange.
    /// </summary>
    [JsonPropertyName("caseChange")]
    public Common CaseChange
    {
      get => caseChange ??= new();
      set => caseChange = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public WorkArea Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public GeneticTestInformation Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of NotPaRefrlPersonFlag.
    /// </summary>
    [JsonPropertyName("notPaRefrlPersonFlag")]
    public Common NotPaRefrlPersonFlag
    {
      get => notPaRefrlPersonFlag ??= new();
      set => notPaRefrlPersonFlag = value;
    }

    /// <summary>
    /// A value of DateText.
    /// </summary>
    [JsonPropertyName("dateText")]
    public TextWorkArea DateText
    {
      get => dateText ??= new();
      set => dateText = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of NoOfLegalActions.
    /// </summary>
    [JsonPropertyName("noOfLegalActions")]
    public Common NoOfLegalActions
    {
      get => noOfLegalActions ??= new();
      set => noOfLegalActions = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public VendorAddress Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of ErrorInTimeConversion.
    /// </summary>
    [JsonPropertyName("errorInTimeConversion")]
    public Common ErrorInTimeConversion
    {
      get => errorInTimeConversion ??= new();
      set => errorInTimeConversion = value;
    }

    /// <summary>
    /// A value of WorkTime.
    /// </summary>
    [JsonPropertyName("workTime")]
    public WorkTime WorkTime
    {
      get => workTime ??= new();
      set => workTime = value;
    }

    /// <summary>
    /// A value of RetCodeFromDispCombn.
    /// </summary>
    [JsonPropertyName("retCodeFromDispCombn")]
    public Common RetCodeFromDispCombn
    {
      get => retCodeFromDispCombn ??= new();
      set => retCodeFromDispCombn = value;
    }

    /// <summary>
    /// A value of ErrorInReceiveResult.
    /// </summary>
    [JsonPropertyName("errorInReceiveResult")]
    public Common ErrorInReceiveResult
    {
      get => errorInReceiveResult ??= new();
      set => errorInReceiveResult = value;
    }

    /// <summary>
    /// A value of ErrorInRecvConfirmation.
    /// </summary>
    [JsonPropertyName("errorInRecvConfirmation")]
    public Common ErrorInRecvConfirmation
    {
      get => errorInRecvConfirmation ??= new();
      set => errorInRecvConfirmation = value;
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
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    /// <summary>
    /// A value of StandardChar.
    /// </summary>
    [JsonPropertyName("standardChar")]
    public StandardChar StandardChar
    {
      get => standardChar ??= new();
      set => standardChar = value;
    }

    /// <summary>
    /// A value of NumberOfEvents.
    /// </summary>
    [JsonPropertyName("numberOfEvents")]
    public Common NumberOfEvents
    {
      get => numberOfEvents ??= new();
      set => numberOfEvents = value;
    }

    /// <summary>
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// A value of ChildInfrastructure.
    /// </summary>
    [JsonPropertyName("childInfrastructure")]
    public Infrastructure ChildInfrastructure
    {
      get => childInfrastructure ??= new();
      set => childInfrastructure = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of OverrideExitstate.
    /// </summary>
    [JsonPropertyName("overrideExitstate")]
    public Common OverrideExitstate
    {
      get => overrideExitstate ??= new();
      set => overrideExitstate = value;
    }

    private NextTranInfo nullNextTranInfo;
    private InterstateRequestHistory specific;
    private Common screenIdentification;
    private GeneticTestInformation resetValues;
    private Common caseChange;
    private BatchConvertNumToText batchConvertNumToText;
    private WorkArea print;
    private SpDocLiteral spDocLiteral;
    private Common position;
    private GeneticTestInformation initialized;
    private Common notPaRefrlPersonFlag;
    private TextWorkArea dateText;
    private DateWorkArea date;
    private Infrastructure infrastructure;
    private TextWorkArea zeroFill;
    private Code code;
    private CodeValue codeValue;
    private Common noOfLegalActions;
    private VendorAddress selected;
    private Common temp;
    private Common errorInTimeConversion;
    private WorkTime workTime;
    private Common retCodeFromDispCombn;
    private Common errorInReceiveResult;
    private Common errorInRecvConfirmation;
    private Standard standard;
    private Common lastErrorEntryNo;
    private Array<ErrorCodesGroup> errorCodes;
    private StandardChar standardChar;
    private Common numberOfEvents;
    private WorkArea raiseEventFlag;
    private InterfaceAlert interfaceAlert;
    private CsePerson ap;
    private CsePerson ar;
    private CsePerson ch;
    private Case1 case1;
    private Infrastructure lastTran;
    private CaseUnit caseUnit;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea current;
    private Infrastructure childInfrastructure;
    private CsePerson childCsePerson;
    private Common overrideExitstate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChildGt.
    /// </summary>
    [JsonPropertyName("childGt")]
    public CsePerson ChildGt
    {
      get => childGt ??= new();
      set => childGt = value;
    }

    /// <summary>
    /// A value of Gt.
    /// </summary>
    [JsonPropertyName("gt")]
    public GeneticTestAccount Gt
    {
      get => gt ??= new();
      set => gt = value;
    }

    /// <summary>
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
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
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
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
    /// A value of ExistingPatEstabOrder.
    /// </summary>
    [JsonPropertyName("existingPatEstabOrder")]
    public LegalAction ExistingPatEstabOrder
    {
      get => existingPatEstabOrder ??= new();
      set => existingPatEstabOrder = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    private CsePerson childGt;
    private GeneticTestAccount gt;
    private CaseRole childCaseRole;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private GeneticTest geneticTest;
    private Case1 case1;
    private LegalAction existingPatEstabOrder;
    private CsePerson childCsePerson;
  }
#endregion
}
