// Program: LE_LDET_LEGAL_DETAIL, ID: 371992835, model: 746.
// Short name: SWELDETP
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
/// A program: LE_LDET_LEGAL_DETAIL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This pstep performs maintenance of Legal Action Detail
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLdetLegalDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LDET_LEGAL_DETAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLdetLegalDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLdetLegalDetail.
  /// </summary>
  public LeLdetLegalDetail(IContext context, Import import, Export export):
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
    // 05/30/95  Dave Allen			Initial Code
    // 12/22/95  Tom Redmond    		Add Petitioner/Respondent logic
    // 07/23/97  Paul R. Egger   		Validate frequency codes against code tables.
    // 07/28/97  Paul R. Egger   		If the Legal Action filed date is zero, can't
    // 					flow to obligations.
    // 09/08/97  Alan Samuels	IDCR 374
    // 09/09/97  Alan Samuels	PR 24917
    // 10/30/97  govind	PR 30956	Fixed to display from the selected Legal
    // 					Detail record on return from LOPS
    // 10/31/97  govind	PR 30861	Fixed the fix to protect/unprotect fields
    // 					when a debt has been established.
    // 12/16/97  R Grey	H00033497  	Flow to LOPS on 'O' class
    // 02/24/98  R Grey	Priority # 19  	Add new action block and code to 
    // unprotect
    // 					End Date when le act dtl obligation type is
    // 					deactivated for all supported persons assoc
    // 					with le act dtl.
    // 01/07/98  P Sharp			Made changes based on Phase II assessment
    // 					sheet.
    // 11/01/99  R. Jean	PR#H78577	Import legal action to the security cab.
    // 12/07/99  Anand Katuri	PR#H80730	Protect Fields on Implicit Scrolling
    // 01/31/00  J. Magat	PR#84601,84605,	Check for changes to legal_action
    // 			84607		classification after the intitial display.
    // 			PR#84936	After end-date is entered and F10 delete is
    // 					pressed afterwards, keep the fields protected
    // 03/15/00  D. Jean	PR88003		Add read to warn if associated legal action
    // 					persons are found.
    // 09/07/00  GVandy	PR101818	Correct DOW displaying spaces.
    // 10/24/00  GVandy	PR 91042	County description is not correct when
    // 					multiple FIPS records exist with the same
    // 					state and county abbreviation.  Read the FIPS
    // 					record associated to the legal action's
    // 					tribunal.
    // 03/09/01  GVandy	PR112679	Effective date is required before flowing to
    // 					OACC, ONAC, OFEE, and OREC.
    // 09/14/01  GVandy	WR020127	Flow to LOPS for each newly added J class
    // 					legal detail.
    // 11/15/01  GVandy	PR132144	Correct error highlighting.
    // 04/17/02  KCole		PR142770	Prevent duplicate legal details on I class
    // 					legal actions for update and add.
    // 04/17/02  GVandy	PR145302	Correct abend when updating non-financial
    // 					legal details.
    // 05/03/02  GVandy	PR# 143439	Read for end dated code values when
    // 					retrieving action taken description.
    // 01/06/06  GVandy	PR265078	Effective date cannot be greater than current
    // 					date for non accruing obligation types.
    // 09/24/07  G. Pan	CQ508 		When the user is adding a legal detail for an
    // 			(HEAT #262386)	I class legal action auto flow to LOPS.
    // 01/29/08  M. Fan	CQ2681		Changed to allow legal detail types WA & WC
    // 			(HEAT #327444)	to have the ability to be 0 amount so that
    // 					the attorney does not have to do an IWO Term.
    // 09/22/08  J Huss	CQ 7056		Added check for valid Withholding Percentage
    // 					for WA and WC obligation types.
    // 11/20/14  A Hockman	cq41786		Changed to add WL as a valid legal detail
    // 					type for new IWO lump sum AND add edits to
    // 					only allow OT (one time) payment freq with
    // 					the WL type.  prevent anything but WL on
    // 					ORDIWOLS and NOTIWOLS
    // 01/19/17  AHockman	cq41786		Added WL as a type that does not expect an
    // 					amt in judgement field for N obligation type
    // 					classification.
    // 05/10/17  GVandy	CQ48108		IV-D PEP changes.
    // ---------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.LegalAction.Assign(import.LegalAction);
    export.PromptTribunal.PromptField = import.PromptTribunal.PromptField;
    export.PromptClass.SelectChar = import.PromptClass.SelectChar;
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.ActionTaken.Description = import.ActionTaken.Description;
    export.Starting.Number = import.Starting.Number;
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;
    export.Display.Flag = import.Display.Flag;

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL"))
    {
      export.PromptClass.SelectChar = "";
      export.PromptTribunal.PromptField = "";
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      import.ObTrnSumAmts.Index = -1;

      export.LegalActionDetail.Index = 0;
      export.LegalActionDetail.Clear();

      for(import.LegalActionDetail.Index = 0; import.LegalActionDetail.Index < import
        .LegalActionDetail.Count; ++import.LegalActionDetail.Index)
      {
        if (export.LegalActionDetail.IsFull)
        {
          break;
        }

        ++import.ObTrnSumAmts.Index;
        import.ObTrnSumAmts.CheckSize();

        export.ObTrnSumAmts.Index = import.ObTrnSumAmts.Index;
        export.ObTrnSumAmts.CheckSize();

        export.ObTrnSumAmts.Update.HiddenAmts.Assign(
          import.ObTrnSumAmts.Item.HiddenAmts);
        export.LegalActionDetail.Update.PromptType.SelectChar =
          import.LegalActionDetail.Item.PromptType.SelectChar;
        export.LegalActionDetail.Update.PromptFreq.SelectChar =
          import.LegalActionDetail.Item.PromptFreq.SelectChar;

        if (!Equal(global.Command, "LIST") && !
          Equal(global.Command, "RETCDVL") && !
          Equal(global.Command, "RETOBTL"))
        {
          export.LegalActionDetail.Update.PromptFreq.SelectChar = "";
          export.LegalActionDetail.Update.PromptType.SelectChar = "";
        }

        if (!Equal(global.Command, "RETLOPS") || Equal
          (global.Command, "RETLOPS") && Equal
          (import.HiddenPrevUserAction.Command, "ADD"))
        {
          export.LegalActionDetail.Update.Common.SelectChar =
            import.LegalActionDetail.Item.Common.SelectChar;
        }

        export.LegalActionDetail.Update.LegalActionDetail1.Assign(
          import.LegalActionDetail.Item.LegalActionDetail1);
        export.LegalActionDetail.Update.DetailFrequencyWorkSet.
          FrequencyDescription =
            import.LegalActionDetail.Item.DetailFrequencyWorkSet.
            FrequencyDescription;
        MoveObligationType(import.LegalActionDetail.Item.DetailObligationType,
          export.LegalActionDetail.Update.DetailObligationType);
        export.LegalActionDetail.Next();
      }
    }

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export.
    // ---------------------------------------------
    export.HiddenLegalAction.Assign(import.HiddenLegalAction);
    MoveFips(import.HiddenFips, export.HiddenFips);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenLinkFromOacc.Flag = import.HiddenLinkFromOacc.Flag;
    export.HiddenLinkFromOnac.Flag = import.HiddenLinkFromOnac.Flag;
    export.HiddenLinkFromOrec.Flag = import.HiddenLinkFromOrec.Flag;
    export.HiddenLinkFromOfee.Flag = import.HiddenLinkFromOfee.Flag;

    if (IsEmpty(export.HiddenLinkFromOacc.Flag))
    {
      if (Equal(global.Command, "FROMOACC"))
      {
        export.HiddenLinkFromOacc.Flag = "Y";
      }
    }

    if (IsEmpty(export.HiddenLinkFromOnac.Flag))
    {
      if (Equal(global.Command, "FROMONAC"))
      {
        export.HiddenLinkFromOnac.Flag = "Y";
      }
    }

    if (IsEmpty(export.HiddenLinkFromOrec.Flag))
    {
      if (Equal(global.Command, "FROMOREC"))
      {
        export.HiddenLinkFromOrec.Flag = "Y";
      }
    }

    if (IsEmpty(export.HiddenLinkFromOfee.Flag))
    {
      if (Equal(global.Command, "FROMOFEE"))
      {
        export.HiddenLinkFromOfee.Flag = "Y";
      }
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();
      export.HiddenNextTranInfo.Assign(local.NextTranInfo);
      export.LegalAction.Identifier =
        export.HiddenNextTranInfo.LegalActionIdentifier.GetValueOrDefault();
      export.LegalAction.CourtCaseNumber =
        export.HiddenNextTranInfo.CourtCaseNumber ?? "";
      export.HiddenLegalAction.Assign(export.LegalAction);

      if (local.NextTranInfo.MiscNum1.GetValueOrDefault() > 0)
      {
        export.Starting.Number =
          (int)local.NextTranInfo.MiscNum1.GetValueOrDefault();
      }

      if (export.LegalAction.Identifier == 0)
      {
        return;
      }

      global.Command = "REDISP";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      local.NextTranInfo.LegalActionIdentifier = export.LegalAction.Identifier;
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      // ****************************************************************
      // * 11/01/99		R. Jean
      // * PR#H78577 - Import legal action to the security cab.
      // ****************************************************************
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // ---------------------------------------------
    //             E D I T    L O G I C
    // ---------------------------------------------
    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "DISPLAY"))
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        if (IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          if (export.LegalAction.Identifier == 0)
          {
            ExitState = "ECO_LNK_TO_LAPS_LEG_ACT_BY_PERSN";

            return;
          }
        }
      }

      if (AsChar(export.LegalAction.Classification) != 'P' || AsChar
        (export.LegalAction.Classification) == 'F')
      {
        if (IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LE0000_CT_CASE_REQD_4_NON_PETIT";
        }
      }

      if (IsEmpty(export.LegalAction.Classification))
      {
        var field = GetField(export.LegalAction, "classification");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
      }
      else
      {
        local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = export.LegalAction.Classification;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      if (IsEmpty(export.Fips.StateAbbreviation) && IsEmpty
        (export.FipsTribAddress.Country))
      {
        var field = GetField(export.Fips, "stateAbbreviation");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "LE0000_STATE_CNTRY_AND_CNTY_REQ";
        }
      }

      if (!IsEmpty(export.Fips.StateAbbreviation) && !
        IsEmpty(export.FipsTribAddress.Country))
      {
        var field1 = GetField(export.FipsTribAddress, "country");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "EITHER_STATE_OR_CNTRY_CODE";
        }
      }

      if (!IsEmpty(export.Fips.StateAbbreviation))
      {
        if (IsEmpty(export.Fips.CountyAbbreviation))
        {
          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          export.Fips.CountyDescription = "";

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_TRIB_COUNTY_REQUIRED";
          }
        }
        else if (!ReadFips1())
        {
          if (ReadFips2())
          {
            var field = GetField(export.Fips, "countyAbbreviation");

            field.Error = true;

            export.Fips.CountyDescription = "";

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "INVALID_COUNTY_CODE";
            }
          }
          else
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;

            MoveTribunal(local.InitialisedToSpaces, export.Tribunal);

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }
        }
      }

      if (export.LegalAction.Identifier > 0)
      {
        if (ReadLegalAction3())
        {
          // -- 10/24/00 GVandy PR 91042-County description is not correct when 
          // multiple FIPS records exist with the same state and county
          // abbreviation.  Read the FIPS record associated to the legal action'
          // s tribunal.
          if (ReadFipsTribunal())
          {
            export.Tribunal.Assign(entities.Tribunal);
            export.Fips.Assign(entities.Fips);
          }
        }
        else
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          MoveTribunal(local.InitialisedToSpaces, export.Tribunal);

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LEGAL_ACTION_NF";
          }
        }
      }
      else
      {
        // -----   Legal Action has not been selected yet
        if (!IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          if (!IsEmpty(export.Fips.StateAbbreviation))
          {
            // **** US Tribunal ****
            if (ReadLegalAction1())
            {
              // -- 10/24/00 GVandy PR 91042-County description is not correct 
              // when multiple FIPS records exist with the same state and county
              // abbreviation.  Read the FIPS record associated to the legal
              // action's tribunal.
              if (ReadFipsTribunal())
              {
                export.Tribunal.Assign(entities.Tribunal);
                export.Fips.Assign(entities.Fips);
              }
            }
            else
            {
              var field1 = GetField(export.LegalAction, "courtCaseNumber");

              field1.Error = true;

              var field2 = GetField(export.Fips, "stateAbbreviation");

              field2.Error = true;

              var field3 = GetField(export.Fips, "countyAbbreviation");

              field3.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";
              }
            }
          }
          else
          {
            // ***Foreign tribunal ***
            if (ReadLegalAction2())
            {
              // -- 10/24/00 GVandy PR 91042-County description is not correct 
              // when multiple FIPS records exist with the same state and county
              // abbreviation.  Read the FIPS record associated to the legal
              // action's tribunal.
              if (ReadFipsTribunal())
              {
                export.Tribunal.Assign(entities.Tribunal);
                export.Fips.Assign(entities.Fips);
              }
            }
            else
            {
              var field1 = GetField(export.FipsTribAddress, "country");

              field1.Error = true;

              var field2 = GetField(export.LegalAction, "courtCaseNumber");

              field2.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";
              }
            }
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        MoveTribunal(local.InitialisedToSpaces, export.Tribunal);
        MoveLegalAction(local.InitLegalAction, export.LegalAction);
        export.Fips.CountyDescription = "";
        export.FipsTribAddress.Country = "";
        export.ActionTaken.Description =
          Spaces(CodeValue.Description_MaxLength);

        if (!export.LegalActionDetail.IsEmpty)
        {
          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            export.LegalActionDetail.Update.DetailFrequencyWorkSet.
              FrequencyDescription =
                local.InitFrequencyWorkSet.FrequencyDescription;
            export.LegalActionDetail.Update.LegalActionDetail1.Assign(
              local.InitLegalActionDetail);
            MoveObligationType(local.InitObligationType,
              export.LegalActionDetail.Update.DetailObligationType);
            export.LegalActionDetail.Update.Common.SelectChar = "";
            export.LegalActionDetail.Update.PromptFreq.SelectChar = "";
            export.LegalActionDetail.Update.PromptType.SelectChar = "";
          }
        }

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "LOPS"))
    {
      local.TotalSelected.Count = 0;

      if (AsChar(export.Display.Flag) != 'Y' || AsChar
        (export.LegalAction.Classification) != AsChar
        (export.HiddenLegalAction.Classification))
      {
        export.Display.Flag = "N";

        // *** PR#84601,84605,84607: Ensure legal action classification is
        // not changed after the initial display.
        var field = GetField(export.LegalAction, "classification");

        field.Error = true;

        if (Equal(global.Command, "ADD"))
        {
          ExitState = "LE0000_DISP_BEF_ADDING_LDET";
        }
        else if (Equal(global.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
        }
        else if (Equal(global.Command, "DELETE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
        }
        else
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";
        }

        return;
      }

      for(export.LegalActionDetail.Index = 0; export.LegalActionDetail.Index < export
        .LegalActionDetail.Count; ++export.LegalActionDetail.Index)
      {
        // ---------------------------------------------
        // Validate Sel.
        // ---------------------------------------------
        switch(AsChar(export.LegalActionDetail.Item.Common.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.TotalSelected.Count;

            if (Equal(global.Command, "LOPS"))
            {
              if (local.TotalSelected.Count > 1)
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
                }
              }
            }

            break;
          default:
            var field =
              GetField(export.LegalActionDetail.Item.Common, "selectChar");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }

            break;
        }

        if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S')
        {
          if (IsEmpty(export.LegalActionDetail.Item.DetailObligationType.Code))
          {
            var field =
              GetField(export.LegalActionDetail.Item.DetailObligationType,
              "code");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }

          if (IsEmpty(export.LegalActionDetail.Item.LegalActionDetail1.
            Description))
          {
            var field =
              GetField(export.LegalActionDetail.Item.LegalActionDetail1,
              "description");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }
        }
      }

      if (local.TotalSelected.Count == 0)
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "LE0000_SELECT_LEGAL_DETAIL";
        }
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        for(export.LegalActionDetail.Index = 0; export
          .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
          export.LegalActionDetail.Index)
        {
          if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S')
          {
            switch(AsChar(export.LegalActionDetail.Item.LegalActionDetail1.
              DetailType))
            {
              case ' ':
                var field1 =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "detailType");

                field1.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
                }

                break;
              case 'F':
                // ********************************************
                // 12-22-07	R Grey		H00034827
                // Dissallow ADD or UPDATE of financial type details unless 
                // Classification = 'P', 'I', or 'J'
                // ********************************************
                switch(AsChar(export.LegalAction.Classification))
                {
                  case 'P':
                    break;
                  case 'I':
                    break;
                  case 'J':
                    break;
                  default:
                    var field3 =
                      GetField(export.LegalActionDetail.Item.
                        DetailObligationType, "code");

                    field3.Error = true;

                    var field4 =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "detailType");

                    field4.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "LE0000_INVALID_OBL_TYPE_4_CLASS";
                    }

                    break;
                }

                break;
              case 'N':
                break;
              default:
                var field2 =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "detailType");

                field2.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "LE0000_INVALID_FIN_NON_FIN_IND";
                }

                break;
            }

            if (IsEmpty(export.LegalActionDetail.Item.DetailObligationType.Code))
              
            {
              var field =
                GetField(export.LegalActionDetail.Item.DetailObligationType,
                "code");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            switch(AsChar(export.LegalActionDetail.Item.LegalActionDetail1.
              DetailType))
            {
              case 'F':
                // ************************************************
                // *Obligation Types WA, WL and WC are valid only *
                // *for Legal Action Classification "I" (IWO      *
                // *related legal actions)  and only two docs     *
                // * NOTIWOLS AND ORDIWOLS can use WL type        *
                // ************************************************
                if (AsChar(export.LegalAction.Classification) == 'I')
                {
                  if ((Equal(
                    export.ActionTaken.Description, "NOTICE OF IWO LUMP SUM") ||
                    Equal
                    (export.ActionTaken.Description, "ORDER FOR IWO LUMP SUM")) &&
                    !
                    Equal(export.LegalActionDetail.Item.DetailObligationType.
                      Code, "WL"))
                  {
                    var field1 =
                      GetField(export.LegalActionDetail.Item.
                        DetailObligationType, "code");

                    field1.Error = true;

                    var field2 = GetField(export.ActionTaken, "description");

                    field2.Error = true;

                    ExitState = "INVALID_COMBINATION";
                  }

                  if (Equal(export.LegalActionDetail.Item.DetailObligationType.
                    Code, "WC") || Equal
                    (export.LegalActionDetail.Item.DetailObligationType.Code,
                    "WA") || Equal
                    (export.LegalActionDetail.Item.DetailObligationType.Code,
                    "WL"))
                  {
                    if (Equal(export.LegalActionDetail.Item.
                      DetailObligationType.Code, "WA"))
                    {
                      // cq56627 only arrears may be populated
                      if (export.LegalActionDetail.Item.LegalActionDetail1.
                        ArrearsAmount.GetValueOrDefault() <= 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "arrearsAmount");

                        field.Error = true;

                        ExitState = "WA_ARREARS_MUST_BE_ENTERED";
                      }

                      if (export.LegalActionDetail.Item.LegalActionDetail1.
                        JudgementAmount.GetValueOrDefault() > 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "judgementAmount");

                        field.Error = true;

                        ExitState = "AMTS_IN_ARREARS_ONLY_FOR_WA";
                      }

                      if (export.LegalActionDetail.Item.LegalActionDetail1.
                        BondAmount.GetValueOrDefault() > 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "bondAmount");

                        field.Error = true;

                        ExitState = "AMTS_IN_ARREARS_ONLY_FOR_WA";
                      }
                    }
                  }
                  else
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.
                        DetailObligationType, "code");

                    field.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "LE0000_TYPE_INVALID_FOR_CLASS";
                    }
                  }

                  if (!Equal(export.LegalActionDetail.Item.DetailObligationType.
                    Code, "WL") && Equal
                    (export.LegalActionDetail.Item.LegalActionDetail1.
                      FreqPeriodCode, "OT"))
                  {
                    var field1 =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "freqPeriodCode");

                    field1.Error = true;

                    var field2 =
                      GetField(export.LegalActionDetail.Item.
                        DetailObligationType, "code");

                    field2.Error = true;

                    ExitState = "INVALID_COMBINATION";
                  }

                  if (Equal(export.LegalActionDetail.Item.DetailObligationType.
                    Code, "WL") && !
                    Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                      FreqPeriodCode, "OT"))
                  {
                    var field1 =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "freqPeriodCode");

                    field1.Error = true;

                    var field2 =
                      GetField(export.LegalActionDetail.Item.
                        DetailObligationType, "code");

                    field2.Error = true;

                    ExitState = "INVALID_COMBINATION";
                  }
                }
                else
                {
                  if (Equal(export.LegalActionDetail.Item.DetailObligationType.
                    Code, "WA") || Equal
                    (export.LegalActionDetail.Item.DetailObligationType.Code,
                    "WC") || Equal
                    (export.LegalActionDetail.Item.DetailObligationType.Code,
                    "WL"))
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.
                        DetailObligationType, "code");

                    field.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "LE0000_TYPE_INVALID_FOR_CLASS";
                    }
                  }
                }

                if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                  EffectiveDate, local.InitialisedToZeros.Date))
                {
                  if (ReadObligationType3())
                  {
                    MoveObligationType(entities.ObligationType,
                      export.LegalActionDetail.Update.DetailObligationType);
                  }
                  else
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.
                        DetailObligationType, "code");

                    field.Error = true;

                    if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      ExitState = "LE0000_LEG_DTL_TYPE_OBLG_COMBO";
                    }
                  }
                }
                else if (ReadObligationType4())
                {
                  MoveObligationType(entities.ObligationType,
                    export.LegalActionDetail.Update.DetailObligationType);
                }
                else
                {
                  var field =
                    GetField(export.LegalActionDetail.Item.DetailObligationType,
                    "code");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "LE0000_LEG_DTL_TYPE_OBLG_COMBO";
                  }
                }

                break;
              case 'N':
                local.Code.CodeName = "LEGAL ACTION DETAIL TYPE";
                local.CodeValue.Cdvalue =
                  export.LegalActionDetail.Item.DetailObligationType.Code;
                UseCabValidateCodeValue();

                if (AsChar(local.ValidCode.Flag) != 'Y')
                {
                  var field =
                    GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                    "detailType");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "LE0000_LEG_DTL_TYPE_OBLG_COMBO";
                  }
                }

                break;
              default:
                break;
            }

            if (IsEmpty(export.LegalActionDetail.Item.LegalActionDetail1.
              Description))
            {
              var field =
                GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                "description");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if (Lt(local.InitialisedToZeros.Date,
              export.LegalActionDetail.Item.LegalActionDetail1.EndDate))
            {
              if (Lt(export.LegalActionDetail.Item.LegalActionDetail1.EndDate,
                export.LegalActionDetail.Item.LegalActionDetail1.
                  EffectiveDate))
              {
                var field =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "endDate");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "LE0000_END_DATE_LT_EFF_DATE";
                }
              }

              // *********************************************
              // 01/30/98	RCG	H00025322 add exit message if end date legal 
              // detail with active obligation.
              // *********************************************
              if (AsChar(local.OblgSetUpForLdet.Flag) == 'Y')
              {
                var field =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "endDate");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "FN0000_OBLIG_PYMNT_SCH_ACTIVE";
                }
              }

              // --05/10/17  GVandy CQ48108 (IV-D PEP Changes) Do not allow EP 
              // legal detail to end
              //   dated if a child on the detail has paternity info locked.
              if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                NonFinOblgType, "EP"))
              {
                if (ReadCsePerson())
                {
                  var field =
                    GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                    "endDate");

                  field.Error = true;

                  ExitState = "LE0000_CANNOT_END_DT_PAT_LOCKED";
                }
              }
            }

            // ***if eff date = blank default to filed date
            if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
              EffectiveDate, null))
            {
              if (Lt(local.InitialisedToZeros.Date, export.LegalAction.FiledDate))
                
              {
                export.LegalActionDetail.Update.LegalActionDetail1.
                  EffectiveDate = export.LegalAction.FiledDate;
              }
            }

            // ********************************************************************************
            // 01/29/08  M. Fan CQ2681(HEAT #327444) Added the obligation type 
            // check for WA & WC
            //                                       
            // to bypass 0 amount edit.
            // *********************************************************************************
            switch(AsChar(export.LegalActionDetail.Item.LegalActionDetail1.
              DetailType))
            {
              case 'F':
                if (Equal(export.LegalActionDetail.Item.DetailObligationType.
                  Code, "WA") || Equal
                  (export.LegalActionDetail.Item.DetailObligationType.Code, "WC"))
                  
                {
                }
                else if (export.LegalActionDetail.Item.LegalActionDetail1.
                  CurrentAmount.GetValueOrDefault() == 0 && export
                  .LegalActionDetail.Item.LegalActionDetail1.JudgementAmount.
                    GetValueOrDefault() == 0 && export
                  .LegalActionDetail.Item.LegalActionDetail1.ArrearsAmount.
                    GetValueOrDefault() == 0)
                {
                  var field1 =
                    GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                    "currentAmount");

                  field1.Error = true;

                  var field2 =
                    GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                    "arrearsAmount");

                  field2.Error = true;

                  var field3 =
                    GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                    "judgementAmount");

                  field3.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "LE0000_AMT_REQD_FOR_FIN_LDET";
                  }
                }

                if (ReadObligationType6())
                {
                  // ********************************************************************************
                  // 01/29/08  M. Fan CQ2681(HEAT #327444) Added the obligation 
                  // type check for WC
                  //                                       
                  // to bypass 0 amount edit.
                  // *********************************************************************************
                  switch(AsChar(entities.ObligationType.Classification))
                  {
                    case 'S':
                      if (export.LegalActionDetail.Item.LegalActionDetail1.
                        ArrearsAmount.GetValueOrDefault() == 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "arrearsAmount");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
                        }
                      }

                      break;
                    case 'A':
                      if (Equal(export.LegalActionDetail.Item.
                        DetailObligationType.Code, "WC"))
                      {
                      }
                      else if (export.LegalActionDetail.Item.LegalActionDetail1.
                        CurrentAmount.GetValueOrDefault() == 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "currentAmount");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "LE0000_CURR_AMT_REQD_4_ACCRUING";
                        }
                      }

                      if (export.LegalActionDetail.Item.LegalActionDetail1.
                        ArrearsAmount.GetValueOrDefault() != 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "arrearsAmount");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "LE0000_ARREAR_MUST_BE_0_F_ACCRNG";
                        }
                      }

                      if (export.LegalActionDetail.Item.LegalActionDetail1.
                        JudgementAmount.GetValueOrDefault() != 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "judgementAmount");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "LE0000_JDGMT_AMT_MUST_BE_0_F_ACC";
                        }
                      }

                      break;
                    case 'N':
                      if (export.LegalActionDetail.Item.LegalActionDetail1.
                        CurrentAmount.GetValueOrDefault() != 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "currentAmount");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "LE0000_CURR_AMT_BE_0_ONAC_OREC";
                        }
                      }

                      // ********************************************************************************
                      // 01/29/08  M. Fan CQ2681(HEAT #327444) Added the 
                      // obligation type check for WA
                      //                                       
                      // to bypass 0 amount edit.
                      // *********************************************************************************
                      if (!IsEmpty(export.LegalActionDetail.Item.
                        LegalActionDetail1.FreqPeriodCode))
                      {
                        if (Equal(export.LegalActionDetail.Item.
                          DetailObligationType.Code, "WA"))
                        {
                        }
                        else if (export.LegalActionDetail.Item.
                          LegalActionDetail1.ArrearsAmount.
                            GetValueOrDefault() == 0)
                        {
                          var field =
                            GetField(export.LegalActionDetail.Item.
                              LegalActionDetail1, "arrearsAmount");

                          field.Error = true;

                          if (IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            ExitState = "LE0000_ARREAR_REQD_FOR_ONAC_OREC";
                          }
                        }
                      }
                      else if (export.LegalActionDetail.Item.LegalActionDetail1.
                        ArrearsAmount.GetValueOrDefault() != 0)
                      {
                        var field1 =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "arrearsAmount");

                        field1.Error = true;

                        var field2 =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "freqPeriodCode");

                        field2.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "LE0000_PERIODIC_AMT_WO_FRQ_PRD";
                        }
                      }

                      // *******************************************************
                      // 01/19/17  AHockman cq41786 added WL as a type that does
                      // not expect a judgement
                      //           amount for N obligation type classification.
                      //  
                      // *******************************************************
                      if (Equal(export.LegalActionDetail.Item.
                        DetailObligationType.Code, "WA") || Equal
                        (export.LegalActionDetail.Item.DetailObligationType.
                          Code, "WL"))
                      {
                        // --- Don't expect Judgement Amount for WA - 
                        // Withholding Arrears
                      }
                      else if (export.LegalActionDetail.Item.LegalActionDetail1.
                        JudgementAmount.GetValueOrDefault() == 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "judgementAmount");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "LE0000_JUD_AMT_REQD_4_ONAC_OREC";
                        }
                      }

                      // --  01/06/06  GVandy PR265078  Effective date cannot be
                      // greater than current date for Non Accruing obligation
                      // types.
                      if (Lt(Now().Date,
                        export.LegalActionDetail.Item.LegalActionDetail1.
                          EffectiveDate))
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "effectiveDate");

                        field.Error = true;

                        ExitState = "LE0000_EFFECTIVE_DT_GT_TODAY";
                      }

                      break;
                    default:
                      if (export.LegalActionDetail.Item.LegalActionDetail1.
                        CurrentAmount.GetValueOrDefault() != 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "currentAmount");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "LE0000_CURR_AMT_BE_0_ONAC_OREC";
                        }
                      }

                      if (!IsEmpty(export.LegalActionDetail.Item.
                        LegalActionDetail1.FreqPeriodCode))
                      {
                        if (export.LegalActionDetail.Item.LegalActionDetail1.
                          ArrearsAmount.GetValueOrDefault() == 0)
                        {
                          var field =
                            GetField(export.LegalActionDetail.Item.
                              LegalActionDetail1, "arrearsAmount");

                          field.Error = true;

                          if (IsExitState("ACO_NN0000_ALL_OK"))
                          {
                            ExitState = "LE0000_ARREAR_REQD_FOR_ONAC_OREC";
                          }
                        }
                      }
                      else if (export.LegalActionDetail.Item.LegalActionDetail1.
                        ArrearsAmount.GetValueOrDefault() != 0)
                      {
                        var field1 =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "arrearsAmount");

                        field1.Error = true;

                        var field2 =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "freqPeriodCode");

                        field2.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "LE0000_PERIODIC_AMT_WO_FRQ_PRD";
                        }
                      }

                      if (export.LegalActionDetail.Item.LegalActionDetail1.
                        JudgementAmount.GetValueOrDefault() == 0)
                      {
                        var field =
                          GetField(export.LegalActionDetail.Item.
                            LegalActionDetail1, "judgementAmount");

                        field.Error = true;

                        if (IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          ExitState = "LE0000_JUD_AMT_REQD_4_ONAC_OREC";
                        }
                      }

                      if (AsChar(entities.ObligationType.Classification) == 'M'
                        || AsChar(entities.ObligationType.Classification) == 'F'
                        )
                      {
                        // --  01/06/06  GVandy PR265078  Effective date cannot 
                        // be greater than current date for Medical and Fee
                        // obligation types.
                        if (Lt(Now().Date,
                          export.LegalActionDetail.Item.LegalActionDetail1.
                            EffectiveDate))
                        {
                          var field =
                            GetField(export.LegalActionDetail.Item.
                              LegalActionDetail1, "effectiveDate");

                          field.Error = true;

                          ExitState = "LE0000_EFFECTIVE_DT_GT_TODAY";
                        }
                      }

                      break;
                  }
                }

                break;
              case 'N':
                if (export.LegalActionDetail.Item.LegalActionDetail1.
                  CurrentAmount.GetValueOrDefault() != 0 || export
                  .LegalActionDetail.Item.LegalActionDetail1.JudgementAmount.
                    GetValueOrDefault() != 0 || export
                  .LegalActionDetail.Item.LegalActionDetail1.ArrearsAmount.
                    GetValueOrDefault() != 0 || !
                  IsEmpty(export.LegalActionDetail.Item.LegalActionDetail1.
                    FreqPeriodCode) || export
                  .LegalActionDetail.Item.LegalActionDetail1.DayOfMonth1.
                    GetValueOrDefault() > 0 || export
                  .LegalActionDetail.Item.LegalActionDetail1.DayOfMonth2.
                    GetValueOrDefault() > 0)
                {
                  if (export.LegalActionDetail.Item.LegalActionDetail1.
                    CurrentAmount.GetValueOrDefault() != 0)
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "currentAmount");

                    field.Error = true;
                  }

                  if (export.LegalActionDetail.Item.LegalActionDetail1.
                    ArrearsAmount.GetValueOrDefault() != 0)
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "arrearsAmount");

                    field.Error = true;
                  }

                  if (export.LegalActionDetail.Item.LegalActionDetail1.
                    JudgementAmount.GetValueOrDefault() != 0)
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "judgementAmount");

                    field.Error = true;
                  }

                  if (export.LegalActionDetail.Item.LegalActionDetail1.
                    DayOfMonth1.GetValueOrDefault() != 0)
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "dayOfMonth1");

                    field.Error = true;
                  }

                  if (export.LegalActionDetail.Item.LegalActionDetail1.
                    DayOfMonth2.GetValueOrDefault() != 0)
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "dayOfMonth2");

                    field.Error = true;
                  }

                  if (!IsEmpty(export.LegalActionDetail.Item.LegalActionDetail1.
                    FreqPeriodCode))
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "freqPeriodCode");

                    field.Error = true;
                  }

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "LE0000_NO_ALLOW_WITH_NON_FIN";
                  }
                }

                break;
              default:
                break;
            }

            if (!IsEmpty(export.LegalActionDetail.Item.LegalActionDetail1.
              FreqPeriodCode) || export
              .LegalActionDetail.Item.LegalActionDetail1.DayOfMonth1.
                GetValueOrDefault() != 0 || export
              .LegalActionDetail.Item.LegalActionDetail1.DayOfMonth2.
                GetValueOrDefault() != 0 || export
              .LegalActionDetail.Item.LegalActionDetail1.CurrentAmount.
                GetValueOrDefault() != 0)
            {
              local.ObligationPaymentSchedule.FrequencyCode =
                export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode ?? Spaces(2);
              local.ObligationPaymentSchedule.StartDt =
                export.LegalActionDetail.Item.LegalActionDetail1.EffectiveDate;
              local.Day1.Count =
                export.LegalActionDetail.Item.LegalActionDetail1.DayOfMonth1.
                  GetValueOrDefault();
              local.Day2.Count =
                export.LegalActionDetail.Item.LegalActionDetail1.DayOfMonth2.
                  GetValueOrDefault();

              // **********************************************************
              // Do code validation against the code table.  If not valid, issue
              // exit state and escape.  07/23/97  Paul R. Egger
              // **********************************************************
              if (!Equal(export.LegalActionDetail.Item.DetailObligationType.
                Code, "WL"))
              {
                local.Cd.CodeName = "FREQUENCY";
                local.Cv.Cdvalue =
                  export.LegalActionDetail.Item.LegalActionDetail1.
                    FreqPeriodCode ?? Spaces(10);
                UseCabGetCodeValueDescription();

                if (local.CodeReturnValue.Count == 0)
                {
                  export.LegalActionDetail.Update.DetailFrequencyWorkSet.
                    FrequencyDescription = local.CodeDescription.Description;
                }
                else
                {
                  var field =
                    GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                    "freqPeriodCode");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "FN0000_INVALID_FREQ_CODE";
                  }
                }

                if (export.LegalActionDetail.Item.LegalActionDetail1.
                  DayOfMonth1.GetValueOrDefault() == 0)
                {
                  // ********************************************
                  // 12-31-07	R Grey		H00034832
                  // Require DOM1 if Frequency Period is populated.
                  // ********************************************
                  var field =
                    GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                    "dayOfMonth1");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
                  }
                }

                // ***********************************************************
                // Still allow the old code to execute, but don't count on it to
                // validate the codes themselves.  This logic still does some
                // validation as long as the previous call said that a valid
                // code was used.   Pre 07/23/97
                // ***********************************************************
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseFnValidateFrequencyInfo();

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.LegalActionDetail.Update.LegalActionDetail1.
                      PeriodInd = local.ObligationPaymentSchedule.PeriodInd ?? ""
                      ;

                    if (export.LegalActionDetail.Item.LegalActionDetail1.
                      DayOfMonth1.GetValueOrDefault() == 0 && Equal
                      (export.LegalActionDetail.Item.LegalActionDetail1.
                        FreqPeriodCode, "M"))
                    {
                      export.LegalActionDetail.Update.LegalActionDetail1.
                        DayOfMonth1 =
                          local.ObligationPaymentSchedule.DayOfMonth1.
                          GetValueOrDefault();
                    }
                  }
                  else if (IsExitState("INVALID_DAY_OF_WEEK"))
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "dayOfMonth1");

                    field.Error = true;
                  }
                  else if (IsExitState("INVALID_DAY_2_MUST_BE_ZERO"))
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "dayOfMonth2");

                    field.Error = true;
                  }
                  else if (IsExitState("ACO_NE0000_REQUIRED_DATA_MISSING"))
                  {
                    var field =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "dayOfMonth2");

                    field.Error = true;
                  }
                  else if (IsExitState("INVALID_DAY_OF_MONTH"))
                  {
                    if (export.LegalActionDetail.Item.LegalActionDetail1.
                      DayOfMonth1.GetValueOrDefault() > 31)
                    {
                      var field =
                        GetField(export.LegalActionDetail.Item.
                          LegalActionDetail1, "dayOfMonth1");

                      field.Error = true;
                    }
                    else if (export.LegalActionDetail.Item.LegalActionDetail1.
                      DayOfMonth2.GetValueOrDefault() > 31)
                    {
                      var field =
                        GetField(export.LegalActionDetail.Item.
                          LegalActionDetail1, "dayOfMonth2");

                      field.Error = true;
                    }
                  }
                  else if (IsExitState("DAY1_AND_DAY2_CANNOT_BE_THE_SAME"))
                  {
                    var field1 =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "dayOfMonth1");

                    field1.Error = true;

                    var field2 =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "dayOfMonth2");

                    field2.Error = true;
                  }
                  else if (IsExitState("FN0000_INVALID_FREQ_CODE"))
                  {
                    ExitState = "ACO_NN0000_ALL_OK";
                  }
                  else
                  {
                    var field1 =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "freqPeriodCode");

                    field1.Error = true;

                    var field2 =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "dayOfMonth1");

                    field2.Error = true;

                    var field3 =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "dayOfMonth2");

                    field3.Error = true;
                  }
                }
              }
            }

            // 09/22/2008	J Huss	Added check for valid Withholding Percentage on
            // WA or WC obligation types.
            if (Equal(export.LegalActionDetail.Item.DetailObligationType.Code,
              "WA") || Equal
              (export.LegalActionDetail.Item.DetailObligationType.Code, "WC"))
            {
              switch(export.LegalActionDetail.Item.LegalActionDetail1.Limit.
                GetValueOrDefault())
              {
                case 0:
                  export.LegalActionDetail.Update.LegalActionDetail1.Limit = 50;

                  break;
                case 50:
                  break;
                case 55:
                  break;
                case 60:
                  break;
                case 65:
                  break;
                default:
                  var field =
                    GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                    "limit");

                  field.Error = true;

                  ExitState = "LE0000_LDET_INVLD_WITHHLDNG_PRCT";

                  break;
              }
            }
          }
        }
      }

      if (Equal(global.Command, "DELETE"))
      {
        for(export.LegalActionDetail.Index = 0; export
          .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
          export.LegalActionDetail.Index)
        {
          if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S')
          {
            // --05/10/17  GVandy CQ48108 (IV-D PEP Changes) Do not allow EP 
            // legal detail to be
            //   deleted if a child on the detail has paternity info locked.
            if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
              NonFinOblgType, "EP"))
            {
              if (ReadCsePerson())
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "number");

                field1.Error = true;

                var field2 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field2.Error = true;

                ExitState = "LE0000_CANNOT_DELETE_PAT_LOCKED";

                break;
              }
            }
          }
        }
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ---------------------------------------------
      //        P F K E Y   P R O C E S S I N G
      // ---------------------------------------------
      switch(TrimEnd(global.Command))
      {
        case "LIST":
          if (!IsEmpty(export.PromptClass.SelectChar) && AsChar
            (export.PromptClass.SelectChar) != 'S')
          {
            var field1 = GetField(export.PromptClass, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (!IsEmpty(export.PromptTribunal.PromptField) && AsChar
            (export.PromptTribunal.PromptField) != 'S')
          {
            var field1 = GetField(export.PromptTribunal, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (IsExitState("ACO_NN0000_ALL_OK") && export
            .LegalAction.Identifier > 0 && export.LegalAction.Identifier == export
            .HiddenLegalAction.Identifier)
          {
            // *** PR#84601: Ensure legal action classification
            // code is not changed after the initial display.
            if (AsChar(export.Display.Flag) != 'Y' || AsChar
              (export.LegalAction.Classification) != AsChar
              (export.HiddenLegalAction.Classification))
            {
              export.Display.Flag = "N";

              var field1 = GetField(export.LegalAction, "classification");

              field1.Error = true;

              ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

              goto Test2;
            }
          }

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (!IsEmpty(export.LegalActionDetail.Item.PromptType.SelectChar) &&
              AsChar(export.LegalActionDetail.Item.PromptType.SelectChar) != 'S'
              )
            {
              var field1 =
                GetField(export.LegalActionDetail.Item.PromptType, "selectChar");
                

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            if (!IsEmpty(export.LegalActionDetail.Item.PromptFreq.SelectChar) &&
              AsChar(export.LegalActionDetail.Item.PromptFreq.SelectChar) != 'S'
              )
            {
              var field1 =
                GetField(export.LegalActionDetail.Item.PromptFreq, "selectChar");
                

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test2;
          }

          if (AsChar(export.PromptClass.SelectChar) == 'S')
          {
            export.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
            ++local.Prompt.Count;
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }

          if (AsChar(export.PromptTribunal.PromptField) == 'S')
          {
            ++local.Prompt.Count;
            ExitState = "ECO_LNK_TO_LIST_TRIBUNALS1";
          }

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (AsChar(export.LegalActionDetail.Item.PromptType.SelectChar) == 'S'
              )
            {
              if (AsChar(export.LegalActionDetail.Item.LegalActionDetail1.
                DetailType) == 'F')
              {
                ++local.Prompt.Count;
                ExitState = "ECO_LNK_TO_LST_OBLIGATION_TYPE";
              }
              else if (AsChar(export.LegalActionDetail.Item.LegalActionDetail1.
                DetailType) == 'N')
              {
                export.Code.CodeName = "LEGAL ACTION DETAIL TYPE";
                ++local.Prompt.Count;
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
              }
              else
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "detailType");

                field1.Error = true;

                ++local.Prompt.Count;
                ExitState = "LE0000_OBLIG_TYPE_F_OR_N_REQD";
              }
            }

            if (AsChar(export.LegalActionDetail.Item.PromptFreq.SelectChar) == 'S'
              )
            {
              export.Code.CodeName = "LEGAL ACTION PAYMENT FREQNCY";
              ++local.Prompt.Count;
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
            }
          }

          if (local.Prompt.Count == 1)
          {
          }
          else if (local.Prompt.Count > 1)
          {
            if (AsChar(export.PromptClass.SelectChar) == 'S')
            {
              var field1 = GetField(export.PromptClass, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.PromptTribunal.PromptField) == 'S')
            {
              var field1 = GetField(export.PromptTribunal, "promptField");

              field1.Error = true;
            }

            for(export.LegalActionDetail.Index = 0; export
              .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
              export.LegalActionDetail.Index)
            {
              if (AsChar(export.LegalActionDetail.Item.PromptType.SelectChar) ==
                'S')
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.PromptType,
                  "selectChar");

                field1.Error = true;
              }

              if (AsChar(export.LegalActionDetail.Item.PromptFreq.SelectChar) ==
                'S')
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.PromptFreq,
                  "selectChar");

                field1.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
          }
          else
          {
            var field1 = GetField(export.PromptClass, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
          }

          break;
        case "RETCDVL":
          // ---------------------------------------------
          // Returned from List Code Values screen. Move
          // values to export.
          // ---------------------------------------------
          if (AsChar(export.PromptClass.SelectChar) == 'S')
          {
            export.PromptClass.SelectChar = "";

            if (IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
            {
              var field1 = GetField(export.LegalAction, "classification");

              field1.Protected = false;
              field1.Focused = true;
            }
            else
            {
              export.LegalAction.Classification =
                import.DlgflwSelectedCodeValue.Cdvalue;

              var field1 = GetField(export.Fips, "stateAbbreviation");

              field1.Protected = false;
              field1.Focused = true;

              if (export.LegalAction.Identifier == export
                .HiddenLegalAction.Identifier && Equal
                (export.LegalAction.CourtCaseNumber,
                export.HiddenLegalAction.CourtCaseNumber) && AsChar
                (export.LegalAction.Classification) != AsChar
                (export.HiddenLegalAction.Classification) && export
                .LegalAction.Identifier > 0)
              {
                global.Command = "DISPLAY";
              }
            }
          }

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (Equal(export.LegalActionDetail.Item.DetailObligationType.Code,
              "SP") || Equal
              (export.LegalActionDetail.Item.LegalActionDetail1.EndDate,
              local.InitialisedToZeros.Date))
            {
            }
            else
            {
              var field1 =
                GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                "endDate");

              field1.Color = "cyan";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = true;
            }

            if (AsChar(export.LegalActionDetail.Item.PromptType.SelectChar) == 'S'
              )
            {
              export.LegalActionDetail.Update.PromptType.SelectChar = "";

              if (IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "detailType");

                field1.Protected = false;
                field1.Focused = true;
              }
              else
              {
                export.LegalActionDetail.Update.DetailObligationType.Code =
                  import.DlgflwSelectedCodeValue.Cdvalue;

                var field1 =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "effectiveDate");

                field1.Protected = false;
                field1.Focused = true;
              }
            }

            if (AsChar(export.LegalActionDetail.Item.PromptFreq.SelectChar) == 'S'
              )
            {
              export.LegalActionDetail.Update.PromptFreq.SelectChar = "";

              if (IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "freqPeriodCode");

                field1.Protected = false;
                field1.Focused = true;
              }
              else
              {
                export.LegalActionDetail.Update.LegalActionDetail1.
                  FreqPeriodCode = import.DlgflwSelectedCodeValue.Cdvalue;

                var field1 =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "dayOfMonth1");

                field1.Protected = false;
                field1.Focused = true;
              }
            }
          }

          break;
        case "RETLTRB":
          export.PromptTribunal.PromptField = "";

          var field = GetField(export.Starting, "number");

          field.Protected = false;
          field.Focused = true;

          break;
        case "RETOBTL":
          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (Equal(export.LegalActionDetail.Item.DetailObligationType.Code,
              "SP") || Equal
              (export.LegalActionDetail.Item.LegalActionDetail1.EndDate,
              local.InitialisedToZeros.Date))
            {
            }
            else
            {
              var field1 =
                GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                "endDate");

              field1.Color = "cyan";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = true;
            }

            if (AsChar(export.LegalActionDetail.Item.PromptType.SelectChar) == 'S'
              )
            {
              export.LegalActionDetail.Update.PromptType.SelectChar = "";

              if (IsEmpty(import.DlgflwSelectedObligationType.Code))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.DetailObligationType,
                  "code");

                field1.Protected = false;
                field1.Focused = true;
              }
              else
              {
                export.LegalActionDetail.Update.DetailObligationType.Code =
                  import.DlgflwSelectedObligationType.Code;

                var field1 =
                  GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                  "effectiveDate");

                field1.Protected = false;
                field1.Focused = true;
              }
            }
          }

          break;
        case "RETLACN":
          global.Command = "REDISP";

          break;
        case "RETLAPS":
          global.Command = "REDISP";

          break;
        case "ADD":
          // ---------------------------------------------
          // Make sure that Court Case Number hasn't been
          // changed before an update.
          // ---------------------------------------------
          if (!Equal(import.LegalAction.CourtCaseNumber,
            import.HiddenLegalAction.CourtCaseNumber))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

            goto Test2;
          }

          if (export.LegalAction.Identifier == export
            .HiddenLegalAction.Identifier)
          {
          }
          else
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "LE0000_DISP_BEF_ADDING_LDET";

            goto Test2;
          }

          // ---------------------------------------------
          // Add a new Legal Action Detail.
          // ---------------------------------------------
          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S')
            {
              if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                FreqPeriodCode, "W") || Equal
                (export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode, "BW"))
              {
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek =
                  export.LegalActionDetail.Item.LegalActionDetail1.DayOfMonth1.
                    GetValueOrDefault();
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                  0;
              }

              // -- Do not remove the following SET statement.  The auto IWO 
              // process creates legal details with the description in mixed
              // case text.  If the user Adds over top of an auto IWO detail the
              // description remains in mixed case.  Convert to uppoer case so
              // that we know the record was created by a user and not the auto
              // IWO process.
              export.LegalActionDetail.Update.LegalActionDetail1.Description =
                ToUpper(export.LegalActionDetail.Item.LegalActionDetail1.
                  Description);

              if (ReadObligationType1())
              {
                if (AsChar(export.LegalAction.Classification) == 'I')
                {
                  if (ReadLegalActionDetail4())
                  {
                    ExitState = "LE0000_DUP_LEGAL_ACTION_DETAIL";

                    goto Test2;
                  }
                }
                else if (ReadLegalActionDetail2())
                {
                  ExitState = "LE0000_DUP_LEGAL_ACTION_DETAIL";

                  goto Test2;
                }
              }

              UseLeCreateLegalActionDetail();

              if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                FreqPeriodCode, "W") || Equal
                (export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode, "BW"))
              {
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                  export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
                    GetValueOrDefault();
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek =
                  0;
              }

              if (IsExitState("LE0000_DUP_LEGAL_ACTION_DETAIL"))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;
              }
              else if (IsExitState("LEGAL_ACTION_NF"))
              {
                var field1 = GetField(export.LegalAction, "courtCaseNumber");

                field1.Error = true;
              }
              else if (IsExitState("LEGAL_ACTION_DETAIL_AE"))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;
              }
              else
              {
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;

                UseEabRollbackCics();

                goto Test2;
              }
              else
              {
                // 09/24/07  G. Pan   CQ508 (HEAT #262386)  Added "I" class
                if (AsChar(export.LegalAction.Classification) == 'J' || AsChar
                  (export.LegalAction.Classification) == 'I')
                {
                  // -- Don't set the select character to spaces.  We will be 
                  // flowing the user to LOPS for each newly added legal detail.
                }
                else
                {
                  export.LegalActionDetail.Update.Common.SelectChar = "";
                }
              }
            }
          }

          // 09/24/07  G. Pan   CQ508 (HEAT #262386)  Added "I" class
          if (AsChar(export.LegalAction.Classification) == 'J' || AsChar
            (export.LegalAction.Classification) == 'I')
          {
            for(export.LegalActionDetail.Index = 0; export
              .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
              export.LegalActionDetail.Index)
            {
              if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S'
                )
              {
                export.LegalActionDetail.Update.Common.SelectChar = "";
                export.HiddenPrevUserAction.Command = global.Command;
                export.DlgflwSelectedLegalActionDetail.Number =
                  export.LegalActionDetail.Item.LegalActionDetail1.Number;
                global.Command = "FROMLDET";
                ExitState = "ECO_LNK_TO_LEGAL_OBLIG_PERS";

                return;
              }
            }
          }

          UseLeListLegalActionDetail();

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
              FreqPeriodCode, "W") || Equal
              (export.LegalActionDetail.Item.LegalActionDetail1.FreqPeriodCode,
              "BW"))
            {
              export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
                  GetValueOrDefault();
              export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek = 0;
            }
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          export.Display.Flag = "Y";

          break;
        case "UPDATE":
          // ---------------------------------------------
          // Make sure that Court Case Number hasn't been
          // changed before an update.
          // ---------------------------------------------
          if (!Equal(import.LegalAction.CourtCaseNumber,
            import.HiddenLegalAction.CourtCaseNumber))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

            goto Test2;
          }

          // ---------------------------------------------
          // Verify that a display has been performed
          // before the update can take place.
          // ---------------------------------------------
          if (import.LegalAction.Identifier != import
            .HiddenLegalAction.Identifier || import.LegalAction.Identifier == 0
            )
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

            goto Test2;
          }

          // ---------------------------------------------
          // Update the current Legal Action Detail.
          // ---------------------------------------------
          export.ObTrnSumAmts.Index = -1;

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            ++export.ObTrnSumAmts.Index;
            export.ObTrnSumAmts.CheckSize();

            if (!IsEmpty(export.LegalActionDetail.Item.Common.SelectChar))
            {
              if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                FreqPeriodCode, "W") || Equal
                (export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode, "BW"))
              {
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek =
                  export.LegalActionDetail.Item.LegalActionDetail1.DayOfMonth1.
                    GetValueOrDefault();
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                  0;
              }

              // -- Do not remove the following SET statement.  The auto IWO 
              // process creates legal details with the description in mixed
              // case text.  By always converting to upper case on an update we
              // know if a user has updated these auto IWO records.
              export.LegalActionDetail.Update.LegalActionDetail1.Description =
                ToUpper(export.LegalActionDetail.Item.LegalActionDetail1.
                  Description);

              if (ReadObligationType1())
              {
                // -- Verify that the requested updates will not result in a 
                // duplicate financial legal detail.
                if (ReadLegalActionDetail1())
                {
                  ExitState = "LE0000_DUP_LEGAL_ACTION_DETAIL";

                  goto Test2;
                }
              }

              // -- Verify that the requested changes will not result in a 
              // duplicate I class legal action.
              // For I class legal actions only one record of a given obligation
              // type is allowed.
              if (ReadLegalActionDetail3())
              {
                if (ReadObligationType2())
                {
                  if (!Equal(entities.ObligationType.Code,
                    export.LegalActionDetail.Item.DetailObligationType.Code) &&
                    AsChar(export.LegalAction.Classification) == 'I')
                  {
                    if (ReadLegalActionDetail4())
                    {
                      ExitState = "LE0000_DUP_LEGAL_ACTION_DETAIL";

                      goto Test2;
                    }
                  }
                }
              }

              UseLeLdetUpdateLegalActionDetl();

              if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                FreqPeriodCode, "W") || Equal
                (export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode, "BW"))
              {
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                  export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
                    GetValueOrDefault();
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek =
                  0;
              }

              if (IsExitState("LE0000_DUP_LEGAL_ACTION_DETAIL"))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;
              }
              else if (IsExitState("LEGAL_ACTION_NF"))
              {
                var field1 = GetField(export.LegalAction, "courtCaseNumber");

                field1.Error = true;
              }
              else if (IsExitState("LEGAL_ACTION_DETAIL_NF"))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;
              }
              else if (IsExitState("LEGAL_ACTION_DETAIL_NU"))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;
              }
              else if (IsExitState("LE0000_UPDT_OK_AMT_CHGD_CHK_LOPS"))
              {
                local.LegalActionPersFound.Flag = "Y";
              }
              else
              {
              }

              // ****************************************************************
              // 3/15/00	D. Jean	PR88003- Add read to warn if associated legal 
              // action persons are found.
              // ****************************************************************
              if (!IsExitState("ACO_NN0000_ALL_OK") && !
                IsExitState("LE0000_UPDT_OK_AMT_CHGD_CHK_LOPS"))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;

                UseEabRollbackCics();

                goto Test2;
              }
              else
              {
                export.LegalActionDetail.Update.Common.SelectChar = "";
              }
            }
          }

          UseLeListLegalActionDetail();

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
              FreqPeriodCode, "W") || Equal
              (export.LegalActionDetail.Item.LegalActionDetail1.FreqPeriodCode,
              "BW"))
            {
              export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
                  GetValueOrDefault();
              export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek = 0;
            }
          }

          export.Display.Flag = "Y";

          // ****************************************************************
          // 3/15/00	D. Jean	PR88003- Add read to warn if associated legal 
          // action persons are found.
          // ****************************************************************
          if (AsChar(local.LegalActionPersFound.Flag) == 'Y')
          {
            ExitState = "LE0000_UPDT_OK_AMT_CHGD_CHK_LOPS";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }

          break;
        case "PREV":
          // *** PR# H00080730 Pass thru the rest of the code for Field Edits 
          // while Scrolling (Implicit).
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        case "NEXT":
          // *** PR# H00080730 Pass thru the rest of the code for Field Edits 
          // while Scrolling (Implicit).
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        case "RETURN":
          local.TotalSelected.Count = 0;

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (!IsEmpty(export.LegalActionDetail.Item.Common.SelectChar))
            {
              ++local.TotalSelected.Count;

              if (local.TotalSelected.Count > 1)
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                goto Test2;
              }
            }
          }

          if (local.TotalSelected.Count == 0)
          {
            // --- No record has been selected. So just return.
            ExitState = "ACO_NE0000_RETURN";

            goto Test2;
          }

          // --- A record has been selected. May need to validate the selected 
          // detail if it came from OACC/ ONAC/ OREC.
          if (AsChar(export.HiddenLinkFromOrec.Flag) != 'Y' && AsChar
            (export.HiddenLinkFromOacc.Flag) != 'Y' && AsChar
            (export.HiddenLinkFromOnac.Flag) != 'Y')
          {
            ExitState = "ACO_NE0000_RETURN";

            goto Test2;
          }

          // ---------------------------------------------
          // Verify that a display has been performed
          // before the LINK can take place.
          // ---------------------------------------------
          if (import.LegalAction.Identifier != import
            .HiddenLegalAction.Identifier || export.LegalAction.Identifier == 0
            )
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

            goto Test2;
          }

          if (AsChar(import.LegalAction.Classification) != AsChar
            (import.HiddenLegalAction.Classification))
          {
            // *** PR#84601: Ensure legal action classification
            // code is not changed after the initial display.
            var field1 = GetField(export.LegalAction, "classification");

            field1.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

            goto Test2;
          }

          if (AsChar(export.LegalAction.Classification) != 'J')
          {
            ExitState = "LE0000_LEGAL_ACTION_MUST_BE_J";

            var field1 = GetField(export.LegalAction, "classification");

            field1.Error = true;

            goto Test2;
          }

          local.TotalSelected.Count = 0;

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S')
            {
              if (AsChar(export.LegalActionDetail.Item.LegalActionDetail1.
                DetailType) != 'F')
              {
                ExitState = "LE0000_MUST_BE_A_FIN_OBLIG";

                goto Test2;
              }

              ++local.TotalSelected.Count;

              if (local.TotalSelected.Count > 1)
              {
                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;

                goto Test2;
              }

              export.DlgflwSelectedLegalActionDetail.Number =
                export.LegalActionDetail.Item.LegalActionDetail1.Number;
              MoveObligationType(export.LegalActionDetail.Item.
                DetailObligationType, export.DlgflwSelectedObligationType);

              if (ReadObligationType5())
              {
                if (AsChar(export.HiddenLinkFromOacc.Flag) == 'Y')
                {
                  if (AsChar(entities.ObligationType.Classification) != 'A')
                  {
                    ExitState = "LE0000_OBLIG_TYPE_MUST_BE_ACCR";

                    var field1 =
                      GetField(export.LegalActionDetail.Item.
                        DetailObligationType, "code");

                    field1.Error = true;

                    goto Test2;
                  }
                }

                if (AsChar(export.HiddenLinkFromOnac.Flag) == 'Y')
                {
                  switch(AsChar(entities.ObligationType.Classification))
                  {
                    case 'A':
                      break;
                    case 'R':
                      break;
                    default:
                      goto Test1;
                  }

                  ExitState = "LE0000_OBLIG_MUST_BE_NON_ACCR";

                  var field1 =
                    GetField(export.LegalActionDetail.Item.DetailObligationType,
                    "code");

                  field1.Error = true;

                  goto Test2;
                }

Test1:

                if (AsChar(export.HiddenLinkFromOrec.Flag) == 'Y')
                {
                  if (AsChar(entities.ObligationType.Classification) != 'R')
                  {
                    ExitState = "LE0000_OBLIG_TYPE_MUST_BE_REC";

                    var field1 =
                      GetField(export.LegalActionDetail.Item.
                        DetailObligationType, "code");

                    field1.Error = true;

                    goto Test2;
                  }
                }
              }
              else
              {
                ExitState = "FN0000_OBLIG_TYPE_NF";

                goto Test2;
              }
            }
          }

          ExitState = "ACO_NE0000_RETURN";

          break;
        case "DELETE":
          // ---------------------------------------------
          // Make sure that Court Case Number hasn't been
          // changed before an delete.
          // ---------------------------------------------
          if (!Equal(import.LegalAction.CourtCaseNumber,
            import.HiddenLegalAction.CourtCaseNumber))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

            goto Test2;
          }

          // ---------------------------------------------
          // Verify that a display has been performed
          // before the deletion can take place.
          // ---------------------------------------------
          if (import.LegalAction.Identifier != import
            .HiddenLegalAction.Identifier || import.LegalAction.Identifier == 0
            )
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

            goto Test2;
          }

          // ---------------------------------------------
          // Delete the Legal Action Details that have
          // been selected.
          // ---------------------------------------------
          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S')
            {
              UseDeleteLegalActionDetail();

              if (IsExitState("LEGAL_ACTION_NF"))
              {
                var field1 = GetField(export.LegalAction, "courtCaseNumber");

                field1.Error = true;
              }
              else if (IsExitState("LEGAL_ACTION_DETAIL_NF"))
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;
              }
              else
              {
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();

                goto Test2;
              }
            }
          }

          UseLeListLegalActionDetail();
          export.Display.Flag = "Y";
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

          break;
        case "SIGNOFF":
          break;
        case "LACT":
          // ---------------------------------------------
          // Transfer to "Legal Action" screen.
          // ---------------------------------------------
          ExitState = "ECO_XFR_TO_LEGAL_ACTION";

          return;
        case "LROL":
          // ---------------------------------------------
          // Transfer to "Legal Role" screen.
          // ---------------------------------------------
          ExitState = "ECO_XFR_TO_LEGAL_ROLE";

          return;
        case "LOPS":
          // ---------------------------------------------
          // Link to "Legal Obligation Persons" screen.
          // ---------------------------------------------
          // ---------------------------------------------
          // Verify that a display has been performed
          // before the LINK can take place.
          // ---------------------------------------------
          if (import.LegalAction.Identifier != import
            .HiddenLegalAction.Identifier)
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

            goto Test2;
          }

          // ---------------------------------------------
          // Only allow flow if Classification = 'J' or 'I' or 'O'.
          // ---------------------------------------------
          if (AsChar(export.LegalAction.Classification) == 'I' || AsChar
            (export.LegalAction.Classification) == 'J' || AsChar
            (export.LegalAction.Classification) == 'O')
          {
            for(export.LegalActionDetail.Index = 0; export
              .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
              export.LegalActionDetail.Index)
            {
              if (!IsEmpty(export.LegalActionDetail.Item.Common.SelectChar))
              {
                export.DlgflwSelectedLegalActionDetail.Number =
                  export.LegalActionDetail.Item.LegalActionDetail1.Number;
              }
            }

            // In order to return back to the selected legal detail on return 
            // from LOPS.
            export.Starting.Number =
              export.DlgflwSelectedLegalActionDetail.Number;
            global.Command = "FIRSTIME";
            ExitState = "ECO_LNK_TO_LEGAL_OBLIG_PERS";

            return;
          }
          else
          {
            ExitState = "LE0000_FLOW_TO_LOPS_NOT_ALLOWED";
          }

          break;
        case "RETLOPS":
          // 09/24/07  G. Pan   CQ508 (HEAT #262386)  Added "I" class
          if (Equal(import.HiddenPrevUserAction.Command, "ADD"))
          {
            if (AsChar(export.LegalAction.Classification) == 'J' || AsChar
              (export.LegalAction.Classification) == 'I')
            {
              for(export.LegalActionDetail.Index = 0; export
                .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
                export.LegalActionDetail.Index)
              {
                if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S'
                  )
                {
                  export.LegalActionDetail.Update.Common.SelectChar = "";
                  export.DlgflwSelectedLegalActionDetail.Number =
                    export.LegalActionDetail.Item.LegalActionDetail1.Number;
                  global.Command = "FROMLDET";
                  ExitState = "ECO_LNK_TO_LEGAL_OBLIG_PERS";

                  return;
                }
              }

              export.HiddenPrevUserAction.Command = "";
              UseLeListLegalActionDetail();

              for(export.LegalActionDetail.Index = 0; export
                .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
                export.LegalActionDetail.Index)
              {
                if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode, "W") || Equal
                  (export.LegalActionDetail.Item.LegalActionDetail1.
                    FreqPeriodCode, "BW"))
                {
                  export.LegalActionDetail.Update.LegalActionDetail1.
                    DayOfMonth1 =
                      export.LegalActionDetail.Item.LegalActionDetail1.
                      DayOfWeek.GetValueOrDefault();
                  export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek =
                    0;
                }
              }

              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
              export.Display.Flag = "Y";
            }
          }
          else
          {
            global.Command = "REDISP";
          }

          break;
        case "OBLIG":
          // ---------------------------------------------
          // Verify that a display has been performed
          // before the LINK can take place.
          // ---------------------------------------------
          if (import.LegalAction.Identifier != import
            .HiddenLegalAction.Identifier || export.LegalAction.Identifier == 0
            )
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

            goto Test2;
          }

          if (AsChar(export.LegalAction.Classification) != AsChar
            (export.HiddenLegalAction.Classification))
          {
            // *** PR#84601: Ensure legal action classification
            // code is not changed after the initial display.
            var field1 = GetField(export.LegalAction, "classification");

            field1.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

            goto Test2;
          }

          if (AsChar(export.LegalAction.Classification) != 'J')
          {
            ExitState = "LE0000_LEGAL_ACTION_MUST_BE_J";

            var field1 = GetField(export.LegalAction, "classification");

            field1.Error = true;

            goto Test2;
          }

          if (Equal(export.LegalAction.FiledDate, local.InitialisedToZeros.Date))
            
          {
            ExitState = "FN0000_FILED_DATE_IS_ZERO";

            goto Test2;
          }

          local.TotalSelected.Count = 0;

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S')
            {
              if (AsChar(export.LegalActionDetail.Item.LegalActionDetail1.
                DetailType) != 'F')
              {
                ExitState = "LE0000_MUST_BE_A_FIN_OBLIG";

                goto Test2;
              }

              ++local.TotalSelected.Count;

              if (local.TotalSelected.Count > 1)
              {
                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;

                goto Test2;
              }

              export.DlgflwSelectedLegalActionDetail.Number =
                export.LegalActionDetail.Item.LegalActionDetail1.Number;
              MoveObligationType(export.LegalActionDetail.Item.
                DetailObligationType, export.DlgflwSelectedObligationType);
            }
          }

          if (local.TotalSelected.Count == 0)
          {
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            goto Test2;
          }

          if (export.DlgflwSelectedObligationType.SystemGeneratedIdentifier > 0)
          {
            if (ReadObligationType5())
            {
              // ****************************************************************
              // 3/15/00	D. Jean	PR88003- Add read to warn if associated legal 
              // action persons are found.  Edit LDET vs LOPS amounts
              // ****************************************************************
              UseLeLopsValidateAddedLopsInfo();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              // -- 3/9/01 GVandy.  PR112679  Effective date is required
              // -- before flowing to OACC, ONAC, OFEE, and OREC.
              export.ObTrnSumAmts.Index = -1;

              for(export.LegalActionDetail.Index = 0; export
                .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
                export.LegalActionDetail.Index)
              {
                ++export.ObTrnSumAmts.Index;
                export.ObTrnSumAmts.CheckSize();

                if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S'
                  )
                {
                  if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                    EffectiveDate, local.ZeroDate.Date))
                  {
                    var field1 =
                      GetField(export.LegalActionDetail.Item.LegalActionDetail1,
                      "effectiveDate");

                    field1.Error = true;

                    var field2 =
                      GetField(export.LegalActionDetail.Item.Common,
                      "selectChar");

                    field2.Error = true;

                    ExitState = "LE0000_EFFECTIVE_DATE_REQUIRED";

                    goto Test2;
                  }
                  else
                  {
                    if (!Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                      EffectiveDate,
                      export.ObTrnSumAmts.Item.HiddenAmts.EffectiveDate))
                    {
                      var field1 =
                        GetField(export.LegalActionDetail.Item.
                          LegalActionDetail1, "effectiveDate");

                      field1.Error = true;

                      var field2 =
                        GetField(export.LegalActionDetail.Item.Common,
                        "selectChar");

                      field2.Error = true;

                      ExitState = "LE0000_UPDATE_BEFORE_OBLIG";

                      goto Test2;
                    }

                    break;
                  }
                }
              }

              switch(AsChar(entities.ObligationType.Classification))
              {
                case 'A':
                  global.Command = "FROMLDET";
                  ExitState = "ECO_LNK_TO_MTN_ACCRUING_OBLIG";

                  break;
                case 'R':
                  if (Equal(entities.ObligationType.Code, "FEE"))
                  {
                    ExitState = "ECO_LNK_TO_OFEE";
                  }
                  else
                  {
                    ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";
                  }

                  break;
                default:
                  if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y'
                    )
                  {
                    ExitState = "ECO_LNK_TO_MTN_NON_ACCRUING_OBLG";
                  }
                  else if (Equal(entities.ObligationType.Code, "FEE"))
                  {
                    ExitState = "ECO_LNK_TO_OFEE";
                  }
                  else
                  {
                    ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";
                  }

                  break;
              }
            }
            else
            {
              ExitState = "FN0000_OBLIG_TYPE_NF";

              goto Test2;
            }

            // --- In order to return back to the selected legal detail on 
            // return.
            export.Starting.Number =
              export.DlgflwSelectedLegalActionDetail.Number;
          }

          return;
        case "OACC":
          // ---------------------------------------------
          // Verify that a display has been performed
          // before the LINK can take place.
          // ---------------------------------------------
          if (import.LegalAction.Identifier != import
            .HiddenLegalAction.Identifier || export.LegalAction.Identifier == 0
            )
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "ACO_NE0000_DISPLAY_BEFORE_LINK";

            goto Test2;
          }

          if (Equal(export.LegalAction.FiledDate, local.InitialisedToZeros.Date))
            
          {
            ExitState = "FN0000_FILED_DATE_IS_ZERO";

            goto Test2;
          }

          local.TotalSelected.Count = 0;

          for(export.LegalActionDetail.Index = 0; export
            .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
            export.LegalActionDetail.Index)
          {
            if (AsChar(export.LegalActionDetail.Item.Common.SelectChar) == 'S')
            {
              if (AsChar(export.LegalActionDetail.Item.LegalActionDetail1.
                DetailType) != 'F')
              {
                ExitState = "LE0000_MUST_BE_A_FIN_OBLIG";

                goto Test2;
              }

              ++local.TotalSelected.Count;

              if (local.TotalSelected.Count > 1)
              {
                var field1 =
                  GetField(export.LegalActionDetail.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                goto Test2;
              }

              export.DlgflwSelectedLegalActionDetail.Number =
                export.LegalActionDetail.Item.LegalActionDetail1.Number;
              MoveObligationType(export.LegalActionDetail.Item.
                DetailObligationType, export.DlgflwSelectedObligationType);
            }
          }

          if (local.TotalSelected.Count == 0)
          {
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            goto Test2;
          }

          if (export.DlgflwSelectedObligationType.SystemGeneratedIdentifier > 0)
          {
            if (ReadObligationType5())
            {
              if (AsChar(entities.ObligationType.Classification) == 'A')
              {
                if (Equal(entities.ObligationType.Code, "CS") || Equal
                  (entities.ObligationType.Code, "MC") || Equal
                  (entities.ObligationType.Code, "MS") || Equal
                  (entities.ObligationType.Code, "SP"))
                {
                  local.Max.Date = UseCabSetMaximumDiscontinueDate();

                  foreach(var item in ReadObligationPaymentSchedule())
                  {
                    if (Equal(entities.ObligationPaymentSchedule.EndDt,
                      local.Max.Date))
                    {
                      ExitState = "LE0000_ALL_OBLG_MUST_DISCONTNUED";

                      goto Test2;
                    }
                  }

                  // ****************************************************************
                  // 3/15/00	D. Jean	PR88003- Add read to warn if associated 
                  // legal action persons are found.  Edit LDET vs LOPS amounts
                  // ****************************************************************
                  UseLeLopsValidateAddedLopsInfo();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  global.Command = "TYPE";
                  ExitState = "ECO_LNK_TO_MTN_ACCRUING_OBLIG";

                  return;
                }
                else
                {
                  ExitState = "LE0000_OBLG_TYPE_CODE_INVALID";
                }
              }
              else
              {
                ExitState = "LE0000_OBLG_TYPE_CODE_INVALID";
              }
            }
            else
            {
              ExitState = "FN0000_OBLIG_TYPE_NF";
            }

            // --- In order to return back to the selected legal detail on 
            // return.
            export.Starting.Number =
              export.DlgflwSelectedLegalActionDetail.Number;
          }

          break;
        case "ENTER":
          // ---------------------------------------------
          // The ENTER key will not be used for functionality
          // here. If it is pressed, an exit state message
          // should be output.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_COMMAND";

          break;
        case "FROMOACC":
          break;
        case "FROMONAC":
          break;
        case "FROMOREC":
          break;
        case "INVALID":
          ExitState = "ACO_NE0000_INVALID_PF_KEY";

          break;
        default:
          break;
      }
    }

Test2:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      switch(TrimEnd(global.Command))
      {
        case "DISPLAY":
          // ---------------------------------------------
          // If the screen has already been displayed,
          // (identifier is present and equal to hidden
          // identifier) and the court case number and
          // classification haven't been changed, there is
          // no need to link to list screen to choose a
          // legal action. It is OK to display the screen.
          // ---------------------------------------------
          if (export.LegalAction.Identifier == export
            .HiddenLegalAction.Identifier && export.LegalAction.Identifier > 0
            && Equal
            (export.LegalAction.CourtCaseNumber,
            export.HiddenLegalAction.CourtCaseNumber) && AsChar
            (export.LegalAction.Classification) == AsChar
            (export.HiddenLegalAction.Classification) && (
              Equal(export.Fips.CountyAbbreviation,
            export.HiddenFips.CountyAbbreviation) || IsEmpty
            (export.HiddenFips.CountyAbbreviation)))
          {
            UseLeListLegalActionDetail();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Display.Flag = "Y";
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
          }
          else
          {
            // ---------------------------------------------
            // If the Court Case Number that was entered,
            // exists on more than one Legal Action, display
            // a list screen to select the desired one.
            // ---------------------------------------------
            local.NoOfLegalActionsFound.Count = 0;

            if (!IsEmpty(export.Fips.StateAbbreviation))
            {
              // --- It is a US tribunal
              foreach(var item in ReadLegalActionTribunalFips())
              {
                if (IsEmpty(export.LegalAction.Classification))
                {
                  // *** Continue on want all classifications
                }
                else if (AsChar(export.LegalAction.Classification) == AsChar
                  (entities.LegalAction.Classification))
                {
                  // *** Continue on want only classification that match 
                  // imported classification
                }
                else
                {
                  continue;
                }

                ++local.NoOfLegalActionsFound.Count;

                if (local.NoOfLegalActionsFound.Count > 1)
                {
                  ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                  return;
                }

                export.LegalAction.Assign(entities.LegalAction);
              }
            }
            else
            {
              // --- It is a foreign tribunal
              if (export.Tribunal.Identifier == 0)
              {
                var field = GetField(export.PromptTribunal, "promptField");

                field.Protected = false;
                field.Focused = true;

                ExitState = "LE0000_TRIB_REQD_4_FOREIGN_TRIB";

                goto Test3;
              }

              foreach(var item in ReadLegalActionTribunal())
              {
                if (IsEmpty(export.LegalAction.Classification))
                {
                  // *** Continue on want all classifications
                }
                else if (AsChar(export.LegalAction.Classification) == AsChar
                  (entities.LegalAction.Classification))
                {
                  // *** Continue on want only classification that match 
                  // imported classification
                }
                else
                {
                  continue;
                }

                ++local.NoOfLegalActionsFound.Count;

                if (local.NoOfLegalActionsFound.Count > 1)
                {
                  ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                  return;
                }

                export.LegalAction.Assign(entities.LegalAction);
              }
            }

            if (local.NoOfLegalActionsFound.Count == 0)
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              ExitState = "LEGAL_ACTION_NF";

              goto Test3;
            }

            UseLeListLegalActionDetail();
          }

          if (export.LegalAction.Identifier > 0)
          {
            ExitState = "ACO_NN0000_ALL_OK";
            UseLeGetPetitionerRespondent();
          }

          if (export.LegalActionDetail.IsEmpty)
          {
            export.Display.Flag = "Y";
            ExitState = "LE0000_NO_LGL_DET_AVAIL_TO_DISP";
          }
          else if (export.LegalActionDetail.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";

            for(export.LegalActionDetail.Index = 0; export
              .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
              export.LegalActionDetail.Index)
            {
              if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                FreqPeriodCode, "W") || Equal
                (export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode, "BW"))
              {
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                  export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
                    GetValueOrDefault();
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek =
                  0;
              }
            }
          }
          else
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Display.Flag = "Y";
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }

            for(export.LegalActionDetail.Index = 0; export
              .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
              export.LegalActionDetail.Index)
            {
              if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                FreqPeriodCode, "W") || Equal
                (export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode, "BW"))
              {
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                  export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
                    GetValueOrDefault();
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek =
                  0;
              }
            }
          }

          break;
        case "REDISP":
          if (ReadLegalAction3())
          {
            export.LegalAction.Assign(entities.LegalAction);

            if (ReadTribunal())
            {
              export.Tribunal.Assign(entities.Tribunal);

              if (ReadFips3())
              {
                export.Fips.Assign(entities.Fips);
              }
              else if (ReadFipsTribAddress())
              {
                export.FipsTribAddress.Country =
                  entities.FipsTribAddress.Country;
              }
              else
              {
                return;
              }
            }
            else
            {
              return;
            }
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";

            goto Test3;
          }

          UseLeListLegalActionDetail();

          if (export.LegalAction.Identifier > 0)
          {
            ExitState = "ACO_NN0000_ALL_OK";
            UseLeGetPetitionerRespondent();
          }

          if (export.LegalActionDetail.IsEmpty)
          {
            export.Display.Flag = "Y";
            ExitState = "LE0000_NO_LGL_DET_AVAIL_TO_DISP";
          }
          else if (export.LegalActionDetail.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";

            for(export.LegalActionDetail.Index = 0; export
              .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
              export.LegalActionDetail.Index)
            {
              if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                FreqPeriodCode, "W") || Equal
                (export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode, "BW"))
              {
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                  export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
                    GetValueOrDefault();
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek =
                  0;
              }
            }
          }
          else
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Display.Flag = "Y";
              export.HiddenPrevUserAction.Command = "DISPLAY";
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }

            for(export.LegalActionDetail.Index = 0; export
              .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
              export.LegalActionDetail.Index)
            {
              if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.
                FreqPeriodCode, "W") || Equal
                (export.LegalActionDetail.Item.LegalActionDetail1.
                  FreqPeriodCode, "BW"))
              {
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfMonth1 =
                  export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
                    GetValueOrDefault();
                export.LegalActionDetail.Update.LegalActionDetail1.DayOfWeek =
                  0;
              }
            }
          }

          break;
        default:
          break;
      }
    }

Test3:

    if (IsExitState("LEGAL_ACTION_NF"))
    {
      var field = GetField(export.LegalAction, "courtCaseNumber");

      field.Error = true;

      return;
    }
    else
    {
    }

    if (!IsEmpty(export.LegalAction.ActionTaken))
    {
      UseLeGetActionTakenDescription();
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    for(export.LegalActionDetail.Index = 0; export.LegalActionDetail.Index < export
      .LegalActionDetail.Count; ++export.LegalActionDetail.Index)
    {
      // ------------------------------------------------
      // If these dates were stored as max dates
      // (12312099), convert them to zeros and don't
      // display them on the screen.
      // ------------------------------------------------
      if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.EffectiveDate,
        local.Max.Date))
      {
        export.LegalActionDetail.Update.LegalActionDetail1.EffectiveDate = null;
      }

      if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.EndDate,
        local.Max.Date))
      {
        export.LegalActionDetail.Update.LegalActionDetail1.EndDate = null;
      }

      if (Equal(export.LegalActionDetail.Item.LegalActionDetail1.CreatedBy,
        "CONVERSN") && Equal
        (export.LegalActionDetail.Item.LegalActionDetail1.LastUpdatedBy,
        "CONVERSN"))
      {
        // *** If the legal detail was created by conversion the fields on this 
        // detail need to be left unprotected so that the users can update the
        // detail to re-establish history. This will be a one shot deal. Once
        // the created by and the last update by do not match then the fields
        // should be protected an normal.
        continue;
      }

      if (export.LegalActionDetail.Item.LegalActionDetail1.Number > 0)
      {
        UseLeCabCheckOblgSetUpFLdet();
        UseLeLdetListOnlyRelvntCroles();

        if (AsChar(local.OblgSetUpForLdet.Flag) == 'Y')
        {
          // ********************************************
          // RCG  02/24/98  New, simplified protection logic.
          // If an obligation has ever been created for the legal action, 
          // protect all fields except 'Withold %, Bond, and Description'.
          // ********************************************
          var field1 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1,
            "detailType");

          field1.Color = "cyan";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = true;

          var field2 =
            GetField(export.LegalActionDetail.Item.DetailObligationType, "code");
            

          field2.Color = "cyan";
          field2.Highlighting = Highlighting.Underscore;
          field2.Protected = true;

          var field3 =
            GetField(export.LegalActionDetail.Item.PromptType, "selectChar");

          field3.Color = "cyan";
          field3.Highlighting = Highlighting.Normal;
          field3.Protected = true;

          var field4 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1,
            "effectiveDate");

          field4.Color = "cyan";
          field4.Highlighting = Highlighting.Underscore;
          field4.Protected = true;

          if (Equal(export.LegalActionDetail.Item.DetailObligationType.Code,
            "SP") || Equal
            (export.LegalActionDetail.Item.LegalActionDetail1.EndDate,
            local.InitialisedToZeros.Date))
          {
          }
          else
          {
            var field =
              GetField(export.LegalActionDetail.Item.LegalActionDetail1,
              "endDate");

            field.Color = "cyan";
            field.Highlighting = Highlighting.Underscore;
            field.Protected = true;
          }

          var field5 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1,
            "freqPeriodCode");

          field5.Color = "cyan";
          field5.Highlighting = Highlighting.Underscore;
          field5.Protected = true;

          var field6 =
            GetField(export.LegalActionDetail.Item.PromptFreq, "selectChar");

          field6.Color = "cyan";
          field6.Highlighting = Highlighting.Normal;
          field6.Protected = true;

          var field7 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1,
            "dayOfMonth1");

          field7.Color = "cyan";
          field7.Highlighting = Highlighting.Underscore;
          field7.Protected = true;

          var field8 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1,
            "dayOfMonth2");

          field8.Color = "cyan";
          field8.Highlighting = Highlighting.Underscore;
          field8.Protected = true;

          var field9 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1,
            "currentAmount");

          field9.Color = "cyan";
          field9.Highlighting = Highlighting.Underscore;
          field9.Protected = true;

          var field10 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1,
            "arrearsAmount");

          field10.Color = "cyan";
          field10.Highlighting = Highlighting.Underscore;
          field10.Protected = true;

          var field11 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1,
            "judgementAmount");

          field11.Color = "cyan";
          field11.Highlighting = Highlighting.Underscore;
          field11.Protected = true;

          var field12 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1, "limit");
            

          field12.Color = "green";
          field12.Highlighting = Highlighting.Underscore;
          field12.Protected = false;

          var field13 =
            GetField(export.LegalActionDetail.Item.LegalActionDetail1,
            "bondAmount");

          field13.Color = "green";
          field13.Highlighting = Highlighting.Underscore;
          field13.Protected = false;
        }
        else
        {
          // ********************************************
          // RCG  -  This legal action has never been associated with an 
          // obligation.
          // ********************************************
        }

        if (AsChar(local.OblgSetUpForLdet.Flag) == 'Y')
        {
          // ********************************************
          // RCG  -  02/24/98  If an obligation has been deactivated, unprotect 
          // the legal action detail End Date.
          // ********************************************
          UseLeLdetCalcObligTrnAmount();
          local.TotalDeactivatedSupportd.Count =
            local.CountAccruingObgDiscont.Count + local
            .CountOnacObgRetired.Count;

          if (local.TotalDeactivatedSupportd.Count >= export
            .CountTotalSupported.Count)
          {
            // ********************************************
            // RCG	02/17/98
            // If the legal action obligation for all supported persons 
            // associated with the legal action detail has been deactivated,
            // unprotect end date.
            // *********************************************
            var field =
              GetField(export.LegalActionDetail.Item.LegalActionDetail1,
              "endDate");

            field.Color = "green";
            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
          }
        }
      }
    }

    // ****************************************************************
    // 3/15/00	D. Jean	PR88003- Add read to warn if associated legal action 
    // persons are found.
    // ****************************************************************
    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_DISPLAY_SUCCESSFUL") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_DELETE") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
      ("ACO_NI0000_LIST_IS_FULL") || IsExitState
      ("ACO_NE0000_INVALID_BACKWARD") || IsExitState
      ("ACO_NE0000_NO_SELECTION_MADE") || IsExitState
      ("LE0000_UPDT_OK_AMT_CHGD_CHK_LOPS"))
    {
      // ------------------------------------------------
      // If all processing completed successfully, move
      // all exports to previous exports.
      // ------------------------------------------------
      export.HiddenLegalAction.Assign(export.LegalAction);
      MoveFips(export.Fips, export.HiddenFips);
      export.ObTrnSumAmts.Index = -1;

      if (!export.LegalActionDetail.IsEmpty)
      {
        for(export.LegalActionDetail.Index = 0; export
          .LegalActionDetail.Index < export.LegalActionDetail.Count; ++
          export.LegalActionDetail.Index)
        {
          ++export.ObTrnSumAmts.Index;
          export.ObTrnSumAmts.CheckSize();

          MoveLegalActionDetail2(export.LegalActionDetail.Item.
            LegalActionDetail1, export.ObTrnSumAmts.Update.HiddenAmts);
        }
      }
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.StandardNumber = source.StandardNumber;
    target.PaymentLocation = source.PaymentLocation;
  }

  private static void MoveLegalActionDetail1(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.FreqPeriodCode = source.FreqPeriodCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
    target.Number = source.Number;
    target.DetailType = source.DetailType;
    target.EndDate = source.EndDate;
    target.EffectiveDate = source.EffectiveDate;
    target.Description = source.Description;
    target.BondAmount = source.BondAmount;
    target.NonFinOblgType = source.NonFinOblgType;
    target.Limit = source.Limit;
    target.ArrearsAmount = source.ArrearsAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.JudgementAmount = source.JudgementAmount;
  }

  private static void MoveLegalActionDetail2(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.DetailType = source.DetailType;
    target.EffectiveDate = source.EffectiveDate;
    target.ArrearsAmount = source.ArrearsAmount;
    target.CurrentAmount = source.CurrentAmount;
    target.JudgementAmount = source.JudgementAmount;
  }

  private static void MoveLegalActionDetail3(LeListLegalActionDetail.Export.
    LegalActionDetailGroup source, Export.LegalActionDetailGroup target)
  {
    target.DetailFrequencyWorkSet.FrequencyDescription =
      source.FrequencyWorkSet.FrequencyDescription;
    MoveObligationType(source.Detail, target.DetailObligationType);
    target.Common.SelectChar = source.Common.SelectChar;
    target.LegalActionDetail1.Assign(source.LegalActionDetail1);
    target.PromptType.SelectChar = source.PromptType.SelectChar;
    target.PromptFreq.SelectChar = source.PromptFreq.SelectChar;
  }

  private static void MoveObligationPaymentSchedule1(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
  }

  private static void MoveObligationPaymentSchedule2(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.StartDt = source.StartDt;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.Name = source.Name;
    target.JudicialDistrict = source.JudicialDistrict;
    target.JudicialDivision = source.JudicialDivision;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Cd, useImport.Code);
    useImport.CodeValue.Cdvalue = local.Cv.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.CodeReturnValue.Count = useExport.ReturnCode.Count;
    local.CodeDescription.Assign(useExport.CodeValue);
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseDeleteLegalActionDetail()
  {
    var useImport = new DeleteLegalActionDetail.Import();
    var useExport = new DeleteLegalActionDetail.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionDetail.Number =
      export.LegalActionDetail.Item.LegalActionDetail1.Number;

    Call(DeleteLegalActionDetail.Execute, useImport, useExport);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnValidateFrequencyInfo()
  {
    var useImport = new FnValidateFrequencyInfo.Import();
    var useExport = new FnValidateFrequencyInfo.Export();

    MoveObligationPaymentSchedule2(local.ObligationPaymentSchedule,
      useImport.ObligationPaymentSchedule);
    useImport.Day1.Count = local.Day1.Count;
    useImport.Day2.Count = local.Day2.Count;

    Call(FnValidateFrequencyInfo.Execute, useImport, useExport);

    MoveObligationPaymentSchedule1(useExport.ObligationPaymentSchedule,
      local.ObligationPaymentSchedule);
    local.PaymentFreq.Text13 = useExport.WorkArea.Text13;
    local.ErrorDay1.Count = useExport.ErrorDay1.Count;
    local.ErrorDay2.Count = useExport.ErrorDay2.Count;
  }

  private void UseLeCabCheckOblgSetUpFLdet()
  {
    var useImport = new LeCabCheckOblgSetUpFLdet.Import();
    var useExport = new LeCabCheckOblgSetUpFLdet.Export();

    useImport.LegalActionDetail.Number =
      export.LegalActionDetail.Item.LegalActionDetail1.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeCabCheckOblgSetUpFLdet.Execute, useImport, useExport);

    local.OblgSetUpForLdet.Flag = useExport.OblgSetUpForLdet.Flag;
  }

  private void UseLeCreateLegalActionDetail()
  {
    var useImport = new LeCreateLegalActionDetail.Import();
    var useExport = new LeCreateLegalActionDetail.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.ObligationType.Code =
      export.LegalActionDetail.Item.DetailObligationType.Code;
    useImport.LegalActionDetail.Assign(
      export.LegalActionDetail.Item.LegalActionDetail1);

    Call(LeCreateLegalActionDetail.Execute, useImport, useExport);

    MoveLegalActionDetail1(useExport.LegalActionDetail,
      export.LegalActionDetail.Update.LegalActionDetail1);
  }

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken = export.LegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    export.ActionTaken.Description = useExport.CodeValue.Description;
  }

  private void UseLeGetPetitionerRespondent()
  {
    var useImport = new LeGetPetitionerRespondent.Import();
    var useExport = new LeGetPetitionerRespondent.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeGetPetitionerRespondent.Execute, useImport, useExport);

    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
  }

  private void UseLeLdetCalcObligTrnAmount()
  {
    var useImport = new LeLdetCalcObligTrnAmount.Import();
    var useExport = new LeLdetCalcObligTrnAmount.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionDetail.Number =
      export.LegalActionDetail.Item.LegalActionDetail1.Number;

    Call(LeLdetCalcObligTrnAmount.Execute, useImport, useExport);

    local.CountOnacObgRetired.Count = useExport.CountOnacObgRetired.Count;
    local.CountAccruingObgDiscont.Count =
      useExport.CountAccruingObgDiscon.Count;
    local.NetJudgement.TotalCurrency = useExport.NetJudgement.TotalCurrency;
    local.NetCurrent.TotalCurrency = useExport.NetCurrent.TotalCurrency;
  }

  private void UseLeLdetListOnlyRelvntCroles()
  {
    var useImport = new LeLdetListOnlyRelvntCroles.Import();
    var useExport = new LeLdetListOnlyRelvntCroles.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionDetail.Number =
      export.LegalActionDetail.Item.LegalActionDetail1.Number;

    Call(LeLdetListOnlyRelvntCroles.Execute, useImport, useExport);

    export.CountTotalSupported.Count = useExport.CountTotalSupported.Count;
  }

  private void UseLeLdetUpdateLegalActionDetl()
  {
    var useImport = new LeLdetUpdateLegalActionDetl.Import();
    var useExport = new LeLdetUpdateLegalActionDetl.Export();

    MoveObligationType(export.LegalActionDetail.Item.DetailObligationType,
      useImport.ObligationType);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.LegalActionDetail.Assign(
      export.LegalActionDetail.Item.LegalActionDetail1);

    Call(LeLdetUpdateLegalActionDetl.Execute, useImport, useExport);
  }

  private void UseLeListLegalActionDetail()
  {
    var useImport = new LeListLegalActionDetail.Import();
    var useExport = new LeListLegalActionDetail.Export();

    useImport.Starting.Number = export.Starting.Number;
    useImport.Export1.Assign(export.LegalAction);

    Call(LeListLegalActionDetail.Execute, useImport, useExport);

    export.LegalAction.Assign(useImport.Export1);
    useExport.LegalActionDetail.CopyTo(
      export.LegalActionDetail, MoveLegalActionDetail3);
  }

  private void UseLeLopsValidateAddedLopsInfo()
  {
    var useImport = new LeLopsValidateAddedLopsInfo.Import();
    var useExport = new LeLopsValidateAddedLopsInfo.Export();

    useImport.ObligationType.Code = entities.ObligationType.Code;
    useImport.LegalActionDetail.Number =
      export.DlgflwSelectedLegalActionDetail.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeLopsValidateAddedLopsInfo.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

    useImport.LegalAction.Assign(import.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.Child.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDt", date);
        db.SetNullableInt32(
          command, "ladRNumber",
          export.LegalActionDetail.Item.LegalActionDetail1.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Child.Number = db.GetString(reader, 0);
        entities.Child.Type1 = db.GetString(reader, 1);
        entities.Child.PaternityLockInd = db.GetNullableString(reader, 2);
        entities.Child.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Child.Type1);
      });
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;

    return Read("ReadFipsTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 6);
        entities.Tribunal.Name = db.GetString(reader, 7);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.Tribunal.Identifier = db.GetInt32(reader, 9);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail1()
  {
    entities.LegalActionDetail1.Populated = false;

    return Read("ReadLegalActionDetail1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dayOfMonth1",
          export.LegalActionDetail.Item.LegalActionDetail1.DayOfMonth1.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "dayOfMonth2",
          export.LegalActionDetail.Item.LegalActionDetail1.DayOfMonth2.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "dayOfWeek",
          export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
            GetValueOrDefault());
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "otyId", entities.ObligationType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDt",
          export.LegalActionDetail.Item.LegalActionDetail1.EffectiveDate.
            GetValueOrDefault());
        db.SetNullableString(
          command, "frqPrdCd",
          export.LegalActionDetail.Item.LegalActionDetail1.FreqPeriodCode ?? ""
          );
        db.SetNullableDecimal(
          command, "arrearsAmount",
          export.LegalActionDetail.Item.LegalActionDetail1.ArrearsAmount.
            GetValueOrDefault());
        db.SetNullableDecimal(
          command, "currentAmount",
          export.LegalActionDetail.Item.LegalActionDetail1.CurrentAmount.
            GetValueOrDefault());
        db.SetNullableDecimal(
          command, "judgementAmount",
          export.LegalActionDetail.Item.LegalActionDetail1.JudgementAmount.
            GetValueOrDefault());
        db.SetInt32(
          command, "laDetailNo",
          export.LegalActionDetail.Item.LegalActionDetail1.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail1.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail1.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail1.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail1.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail1.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail1.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionDetail1.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionDetail1.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail1.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.LegalActionDetail1.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionDetail1.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail1.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail1.Limit = db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail1.DetailType = db.GetString(reader, 13);
        entities.LegalActionDetail1.FreqPeriodCode =
          db.GetNullableString(reader, 14);
        entities.LegalActionDetail1.DayOfWeek = db.GetNullableInt32(reader, 15);
        entities.LegalActionDetail1.DayOfMonth1 =
          db.GetNullableInt32(reader, 16);
        entities.LegalActionDetail1.DayOfMonth2 =
          db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail1.PeriodInd =
          db.GetNullableString(reader, 18);
        entities.LegalActionDetail1.Description = db.GetString(reader, 19);
        entities.LegalActionDetail1.OtyId = db.GetNullableInt32(reader, 20);
        entities.LegalActionDetail1.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail1.DetailType);
      });
  }

  private bool ReadLegalActionDetail2()
  {
    entities.LegalActionDetail1.Populated = false;

    return Read("ReadLegalActionDetail2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dayOfMonth1",
          export.LegalActionDetail.Item.LegalActionDetail1.DayOfMonth1.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "dayOfMonth2",
          export.LegalActionDetail.Item.LegalActionDetail1.DayOfMonth2.
            GetValueOrDefault());
        db.SetNullableInt32(
          command, "dayOfWeek",
          export.LegalActionDetail.Item.LegalActionDetail1.DayOfWeek.
            GetValueOrDefault());
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "otyId", entities.ObligationType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDt",
          export.LegalActionDetail.Item.LegalActionDetail1.EffectiveDate.
            GetValueOrDefault());
        db.SetNullableString(
          command, "frqPrdCd",
          export.LegalActionDetail.Item.LegalActionDetail1.FreqPeriodCode ?? ""
          );
        db.SetNullableDecimal(
          command, "arrearsAmount",
          export.LegalActionDetail.Item.LegalActionDetail1.ArrearsAmount.
            GetValueOrDefault());
        db.SetNullableDecimal(
          command, "currentAmount",
          export.LegalActionDetail.Item.LegalActionDetail1.CurrentAmount.
            GetValueOrDefault());
        db.SetNullableDecimal(
          command, "judgementAmount",
          export.LegalActionDetail.Item.LegalActionDetail1.JudgementAmount.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionDetail1.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail1.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail1.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail1.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail1.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail1.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionDetail1.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionDetail1.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail1.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.LegalActionDetail1.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionDetail1.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail1.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail1.Limit = db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail1.DetailType = db.GetString(reader, 13);
        entities.LegalActionDetail1.FreqPeriodCode =
          db.GetNullableString(reader, 14);
        entities.LegalActionDetail1.DayOfWeek = db.GetNullableInt32(reader, 15);
        entities.LegalActionDetail1.DayOfMonth1 =
          db.GetNullableInt32(reader, 16);
        entities.LegalActionDetail1.DayOfMonth2 =
          db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail1.PeriodInd =
          db.GetNullableString(reader, 18);
        entities.LegalActionDetail1.Description = db.GetString(reader, 19);
        entities.LegalActionDetail1.OtyId = db.GetNullableInt32(reader, 20);
        entities.LegalActionDetail1.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail1.DetailType);
      });
  }

  private bool ReadLegalActionDetail3()
  {
    entities.LegalActionDetail1.Populated = false;

    return Read("ReadLegalActionDetail3",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetInt32(
          command, "laDetailNo",
          export.LegalActionDetail.Item.LegalActionDetail1.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail1.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail1.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail1.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail1.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail1.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail1.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionDetail1.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionDetail1.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail1.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.LegalActionDetail1.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionDetail1.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail1.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail1.Limit = db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail1.DetailType = db.GetString(reader, 13);
        entities.LegalActionDetail1.FreqPeriodCode =
          db.GetNullableString(reader, 14);
        entities.LegalActionDetail1.DayOfWeek = db.GetNullableInt32(reader, 15);
        entities.LegalActionDetail1.DayOfMonth1 =
          db.GetNullableInt32(reader, 16);
        entities.LegalActionDetail1.DayOfMonth2 =
          db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail1.PeriodInd =
          db.GetNullableString(reader, 18);
        entities.LegalActionDetail1.Description = db.GetString(reader, 19);
        entities.LegalActionDetail1.OtyId = db.GetNullableInt32(reader, 20);
        entities.LegalActionDetail1.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail1.DetailType);
      });
  }

  private bool ReadLegalActionDetail4()
  {
    entities.LegalActionDetail1.Populated = false;

    return Read("ReadLegalActionDetail4",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetNullableInt32(
          command, "otyId", entities.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail1.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail1.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail1.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail1.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail1.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail1.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionDetail1.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionDetail1.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.LegalActionDetail1.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.LegalActionDetail1.ArrearsAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionDetail1.CurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.LegalActionDetail1.JudgementAmount =
          db.GetNullableDecimal(reader, 11);
        entities.LegalActionDetail1.Limit = db.GetNullableInt32(reader, 12);
        entities.LegalActionDetail1.DetailType = db.GetString(reader, 13);
        entities.LegalActionDetail1.FreqPeriodCode =
          db.GetNullableString(reader, 14);
        entities.LegalActionDetail1.DayOfWeek = db.GetNullableInt32(reader, 15);
        entities.LegalActionDetail1.DayOfMonth1 =
          db.GetNullableInt32(reader, 16);
        entities.LegalActionDetail1.DayOfMonth2 =
          db.GetNullableInt32(reader, 17);
        entities.LegalActionDetail1.PeriodInd =
          db.GetNullableString(reader, 18);
        entities.LegalActionDetail1.Description = db.GetString(reader, 19);
        entities.LegalActionDetail1.OtyId = db.GetNullableInt32(reader, 20);
        entities.LegalActionDetail1.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail1.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal()
  {
    entities.Tribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.Tribunal.Identifier = db.GetInt32(reader, 7);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 8);
        entities.Tribunal.Name = db.GetString(reader, 9);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 10);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 11);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 12);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 13);
        entities.Tribunal.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunalFips()
  {
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunalFips",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.Tribunal.Identifier = db.GetInt32(reader, 7);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 8);
        entities.Tribunal.Name = db.GetString(reader, 9);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 10);
        entities.Fips.Location = db.GetInt32(reader, 10);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 11);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 12);
        entities.Fips.County = db.GetInt32(reader, 12);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 13);
        entities.Fips.State = db.GetInt32(reader, 13);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 14);
        entities.Fips.StateAbbreviation = db.GetString(reader, 15);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 16);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationPaymentSchedule()
  {
    entities.ObligationPaymentSchedule.Populated = false;

    return ReadEach("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetInt32(
          command, "laDetailNo", export.DlgflwSelectedLegalActionDetail.Number);
          
        db.SetString(
          command, "debtTypCd", export.DlgflwSelectedObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 5);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);

        return true;
      });
  }

  private bool ReadObligationType1()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType1",
      (db, command) =>
      {
        db.SetString(
          command, "debtTypCd",
          export.LegalActionDetail.Item.DetailObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 5);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail1.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail1.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 5);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadObligationType3()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType3",
      (db, command) =>
      {
        db.SetString(
          command, "debtTypCd",
          export.LegalActionDetail.Item.DetailObligationType.Code);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 5);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadObligationType4()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType4",
      (db, command) =>
      {
        db.SetString(
          command, "debtTypCd",
          export.LegalActionDetail.Item.DetailObligationType.Code);
        db.SetDate(
          command, "effectiveDt",
          export.LegalActionDetail.Item.LegalActionDetail1.EffectiveDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 5);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadObligationType5()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType5",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          export.DlgflwSelectedObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 5);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadObligationType6()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType6",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          export.LegalActionDetail.Item.DetailObligationType.
            SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 3);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 4);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 5);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
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
    /// <summary>A CseCasesGroup group.</summary>
    [Serializable]
    public class CseCasesGroup
    {
      /// <summary>
      /// A value of CseCase.
      /// </summary>
      [JsonPropertyName("cseCase")]
      public Case1 CseCase
      {
        get => cseCase ??= new();
        set => cseCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 cseCase;
    }

    /// <summary>A LegalActionDetailGroup group.</summary>
    [Serializable]
    public class LegalActionDetailGroup
    {
      /// <summary>
      /// A value of DetailFrequencyWorkSet.
      /// </summary>
      [JsonPropertyName("detailFrequencyWorkSet")]
      public FrequencyWorkSet DetailFrequencyWorkSet
      {
        get => detailFrequencyWorkSet ??= new();
        set => detailFrequencyWorkSet = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
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
      /// A value of LegalActionDetail1.
      /// </summary>
      [JsonPropertyName("legalActionDetail1")]
      public LegalActionDetail LegalActionDetail1
      {
        get => legalActionDetail1 ??= new();
        set => legalActionDetail1 = value;
      }

      /// <summary>
      /// A value of PromptType.
      /// </summary>
      [JsonPropertyName("promptType")]
      public Common PromptType
      {
        get => promptType ??= new();
        set => promptType = value;
      }

      /// <summary>
      /// A value of PromptFreq.
      /// </summary>
      [JsonPropertyName("promptFreq")]
      public Common PromptFreq
      {
        get => promptFreq ??= new();
        set => promptFreq = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private FrequencyWorkSet detailFrequencyWorkSet;
      private ObligationType detailObligationType;
      private Common common;
      private LegalActionDetail legalActionDetail1;
      private Common promptType;
      private Common promptFreq;
    }

    /// <summary>A ObTrnSumAmtsGroup group.</summary>
    [Serializable]
    public class ObTrnSumAmtsGroup
    {
      /// <summary>
      /// A value of HiddenAmts.
      /// </summary>
      [JsonPropertyName("hiddenAmts")]
      public LegalActionDetail HiddenAmts
      {
        get => hiddenAmts ??= new();
        set => hiddenAmts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalActionDetail hiddenAmts;
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
    }

    /// <summary>
    /// A value of HiddenLinkFromOacc.
    /// </summary>
    [JsonPropertyName("hiddenLinkFromOacc")]
    public Common HiddenLinkFromOacc
    {
      get => hiddenLinkFromOacc ??= new();
      set => hiddenLinkFromOacc = value;
    }

    /// <summary>
    /// A value of HiddenLinkFromOnac.
    /// </summary>
    [JsonPropertyName("hiddenLinkFromOnac")]
    public Common HiddenLinkFromOnac
    {
      get => hiddenLinkFromOnac ??= new();
      set => hiddenLinkFromOnac = value;
    }

    /// <summary>
    /// A value of HiddenLinkFromOrec.
    /// </summary>
    [JsonPropertyName("hiddenLinkFromOrec")]
    public Common HiddenLinkFromOrec
    {
      get => hiddenLinkFromOrec ??= new();
      set => hiddenLinkFromOrec = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Standard PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public LegalActionDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedObligationType.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedObligationType")]
    public ObligationType DlgflwSelectedObligationType
    {
      get => dlgflwSelectedObligationType ??= new();
      set => dlgflwSelectedObligationType = value;
    }

    /// <summary>
    /// Gets a value of CseCases.
    /// </summary>
    [JsonIgnore]
    public Array<CseCasesGroup> CseCases => cseCases ??= new(
      CseCasesGroup.Capacity);

    /// <summary>
    /// Gets a value of CseCases for json serialization.
    /// </summary>
    [JsonPropertyName("cseCases")]
    [Computed]
    public IList<CseCasesGroup> CseCases_Json
    {
      get => cseCases;
      set => CseCases.Assign(value);
    }

    /// <summary>
    /// Gets a value of LegalActionDetail.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionDetailGroup> LegalActionDetail =>
      legalActionDetail ??= new(LegalActionDetailGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalActionDetail for json serialization.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    [Computed]
    public IList<LegalActionDetailGroup> LegalActionDetail_Json
    {
      get => legalActionDetail;
      set => LegalActionDetail.Assign(value);
    }

    /// <summary>
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
    }

    /// <summary>
    /// A value of PromptClass.
    /// </summary>
    [JsonPropertyName("promptClass")]
    public Common PromptClass
    {
      get => promptClass ??= new();
      set => promptClass = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedLegalAction.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedLegalAction")]
    public LegalAction DlgflwSelectedLegalAction
    {
      get => dlgflwSelectedLegalAction ??= new();
      set => dlgflwSelectedLegalAction = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCodeValue.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCodeValue")]
    public CodeValue DlgflwSelectedCodeValue
    {
      get => dlgflwSelectedCodeValue ??= new();
      set => dlgflwSelectedCodeValue = value;
    }

    /// <summary>
    /// A value of HiddenLinkFromOfee.
    /// </summary>
    [JsonPropertyName("hiddenLinkFromOfee")]
    public Common HiddenLinkFromOfee
    {
      get => hiddenLinkFromOfee ??= new();
      set => hiddenLinkFromOfee = value;
    }

    /// <summary>
    /// Gets a value of ObTrnSumAmts.
    /// </summary>
    [JsonIgnore]
    public Array<ObTrnSumAmtsGroup> ObTrnSumAmts => obTrnSumAmts ??= new(
      ObTrnSumAmtsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ObTrnSumAmts for json serialization.
    /// </summary>
    [JsonPropertyName("obTrnSumAmts")]
    [Computed]
    public IList<ObTrnSumAmtsGroup> ObTrnSumAmts_Json
    {
      get => obTrnSumAmts;
      set => ObTrnSumAmts.Assign(value);
    }

    /// <summary>
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    private Common display;
    private Common hiddenLinkFromOacc;
    private Common hiddenLinkFromOnac;
    private Common hiddenLinkFromOrec;
    private CodeValue actionTaken;
    private FipsTribAddress fipsTribAddress;
    private NextTranInfo hiddenNextTranInfo;
    private Common hiddenPrevUserAction;
    private Standard promptTribunal;
    private LegalActionDetail starting;
    private Tribunal tribunal;
    private Fips fips;
    private ObligationType dlgflwSelectedObligationType;
    private Array<CseCasesGroup> cseCases;
    private Array<LegalActionDetailGroup> legalActionDetail;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private LegalAction legalAction;
    private LegalAction hiddenLegalAction;
    private Common promptClass;
    private LegalAction dlgflwSelectedLegalAction;
    private CodeValue dlgflwSelectedCodeValue;
    private Common hiddenLinkFromOfee;
    private Array<ObTrnSumAmtsGroup> obTrnSumAmts;
    private Fips hiddenFips;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CseCasesGroup group.</summary>
    [Serializable]
    public class CseCasesGroup
    {
      /// <summary>
      /// A value of CseCase.
      /// </summary>
      [JsonPropertyName("cseCase")]
      public Case1 CseCase
      {
        get => cseCase ??= new();
        set => cseCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 cseCase;
    }

    /// <summary>A LegalActionDetailGroup group.</summary>
    [Serializable]
    public class LegalActionDetailGroup
    {
      /// <summary>
      /// A value of DetailFrequencyWorkSet.
      /// </summary>
      [JsonPropertyName("detailFrequencyWorkSet")]
      public FrequencyWorkSet DetailFrequencyWorkSet
      {
        get => detailFrequencyWorkSet ??= new();
        set => detailFrequencyWorkSet = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
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
      /// A value of LegalActionDetail1.
      /// </summary>
      [JsonPropertyName("legalActionDetail1")]
      public LegalActionDetail LegalActionDetail1
      {
        get => legalActionDetail1 ??= new();
        set => legalActionDetail1 = value;
      }

      /// <summary>
      /// A value of PromptType.
      /// </summary>
      [JsonPropertyName("promptType")]
      public Common PromptType
      {
        get => promptType ??= new();
        set => promptType = value;
      }

      /// <summary>
      /// A value of PromptFreq.
      /// </summary>
      [JsonPropertyName("promptFreq")]
      public Common PromptFreq
      {
        get => promptFreq ??= new();
        set => promptFreq = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private FrequencyWorkSet detailFrequencyWorkSet;
      private ObligationType detailObligationType;
      private Common common;
      private LegalActionDetail legalActionDetail1;
      private Common promptType;
      private Common promptFreq;
    }

    /// <summary>A ObTrnSumAmtsGroup group.</summary>
    [Serializable]
    public class ObTrnSumAmtsGroup
    {
      /// <summary>
      /// A value of HiddenAmts.
      /// </summary>
      [JsonPropertyName("hiddenAmts")]
      public LegalActionDetail HiddenAmts
      {
        get => hiddenAmts ??= new();
        set => hiddenAmts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalActionDetail hiddenAmts;
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public Common Display
    {
      get => display ??= new();
      set => display = value;
    }

    /// <summary>
    /// A value of HiddenLinkFromOacc.
    /// </summary>
    [JsonPropertyName("hiddenLinkFromOacc")]
    public Common HiddenLinkFromOacc
    {
      get => hiddenLinkFromOacc ??= new();
      set => hiddenLinkFromOacc = value;
    }

    /// <summary>
    /// A value of HiddenLinkFromOnac.
    /// </summary>
    [JsonPropertyName("hiddenLinkFromOnac")]
    public Common HiddenLinkFromOnac
    {
      get => hiddenLinkFromOnac ??= new();
      set => hiddenLinkFromOnac = value;
    }

    /// <summary>
    /// A value of HiddenLinkFromOrec.
    /// </summary>
    [JsonPropertyName("hiddenLinkFromOrec")]
    public Common HiddenLinkFromOrec
    {
      get => hiddenLinkFromOrec ??= new();
      set => hiddenLinkFromOrec = value;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of DlgflwSelectedObligationType.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedObligationType")]
    public ObligationType DlgflwSelectedObligationType
    {
      get => dlgflwSelectedObligationType ??= new();
      set => dlgflwSelectedObligationType = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Standard PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public LegalActionDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// Gets a value of CseCases.
    /// </summary>
    [JsonIgnore]
    public Array<CseCasesGroup> CseCases => cseCases ??= new(
      CseCasesGroup.Capacity);

    /// <summary>
    /// Gets a value of CseCases for json serialization.
    /// </summary>
    [JsonPropertyName("cseCases")]
    [Computed]
    public IList<CseCasesGroup> CseCases_Json
    {
      get => cseCases;
      set => CseCases.Assign(value);
    }

    /// <summary>
    /// Gets a value of LegalActionDetail.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionDetailGroup> LegalActionDetail =>
      legalActionDetail ??= new(LegalActionDetailGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalActionDetail for json serialization.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    [Computed]
    public IList<LegalActionDetailGroup> LegalActionDetail_Json
    {
      get => legalActionDetail;
      set => LegalActionDetail.Assign(value);
    }

    /// <summary>
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedLegalActionDetail.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedLegalActionDetail")]
    public LegalActionDetail DlgflwSelectedLegalActionDetail
    {
      get => dlgflwSelectedLegalActionDetail ??= new();
      set => dlgflwSelectedLegalActionDetail = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
    }

    /// <summary>
    /// A value of PromptClass.
    /// </summary>
    [JsonPropertyName("promptClass")]
    public Common PromptClass
    {
      get => promptClass ??= new();
      set => promptClass = value;
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
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
    }

    /// <summary>
    /// A value of HiddenLinkFromOfee.
    /// </summary>
    [JsonPropertyName("hiddenLinkFromOfee")]
    public Common HiddenLinkFromOfee
    {
      get => hiddenLinkFromOfee ??= new();
      set => hiddenLinkFromOfee = value;
    }

    /// <summary>
    /// Gets a value of ObTrnSumAmts.
    /// </summary>
    [JsonIgnore]
    public Array<ObTrnSumAmtsGroup> ObTrnSumAmts => obTrnSumAmts ??= new(
      ObTrnSumAmtsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ObTrnSumAmts for json serialization.
    /// </summary>
    [JsonPropertyName("obTrnSumAmts")]
    [Computed]
    public IList<ObTrnSumAmtsGroup> ObTrnSumAmts_Json
    {
      get => obTrnSumAmts;
      set => ObTrnSumAmts.Assign(value);
    }

    /// <summary>
    /// A value of OnacDebtDetailRetired.
    /// </summary>
    [JsonPropertyName("onacDebtDetailRetired")]
    public Common OnacDebtDetailRetired
    {
      get => onacDebtDetailRetired ??= new();
      set => onacDebtDetailRetired = value;
    }

    /// <summary>
    /// A value of CountTotalSupported.
    /// </summary>
    [JsonPropertyName("countTotalSupported")]
    public Common CountTotalSupported
    {
      get => countTotalSupported ??= new();
      set => countTotalSupported = value;
    }

    /// <summary>
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    private Common display;
    private Common hiddenLinkFromOacc;
    private Common hiddenLinkFromOnac;
    private Common hiddenLinkFromOrec;
    private CodeValue actionTaken;
    private FipsTribAddress fipsTribAddress;
    private NextTranInfo hiddenNextTranInfo;
    private ObligationType dlgflwSelectedObligationType;
    private Common hiddenPrevUserAction;
    private Standard promptTribunal;
    private LegalActionDetail starting;
    private Tribunal tribunal;
    private Fips fips;
    private Array<CseCasesGroup> cseCases;
    private Array<LegalActionDetailGroup> legalActionDetail;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private LegalActionDetail dlgflwSelectedLegalActionDetail;
    private Standard standard;
    private LegalAction legalAction;
    private LegalAction hiddenLegalAction;
    private Common promptClass;
    private Code code;
    private Common displayActiveCasesOnly;
    private Common hiddenLinkFromOfee;
    private Array<ObTrnSumAmtsGroup> obTrnSumAmts;
    private Common onacDebtDetailRetired;
    private Common countTotalSupported;
    private Fips hiddenFips;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LegalActionPersFound.
    /// </summary>
    [JsonPropertyName("legalActionPersFound")]
    public Common LegalActionPersFound
    {
      get => legalActionPersFound ??= new();
      set => legalActionPersFound = value;
    }

    /// <summary>
    /// A value of InitLegalAction.
    /// </summary>
    [JsonPropertyName("initLegalAction")]
    public LegalAction InitLegalAction
    {
      get => initLegalAction ??= new();
      set => initLegalAction = value;
    }

    /// <summary>
    /// A value of InitLegalActionDetail.
    /// </summary>
    [JsonPropertyName("initLegalActionDetail")]
    public LegalActionDetail InitLegalActionDetail
    {
      get => initLegalActionDetail ??= new();
      set => initLegalActionDetail = value;
    }

    /// <summary>
    /// A value of InitObligationType.
    /// </summary>
    [JsonPropertyName("initObligationType")]
    public ObligationType InitObligationType
    {
      get => initObligationType ??= new();
      set => initObligationType = value;
    }

    /// <summary>
    /// A value of InitFrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("initFrequencyWorkSet")]
    public FrequencyWorkSet InitFrequencyWorkSet
    {
      get => initFrequencyWorkSet ??= new();
      set => initFrequencyWorkSet = value;
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
    /// A value of TotalDeactivatedSupportd.
    /// </summary>
    [JsonPropertyName("totalDeactivatedSupportd")]
    public Common TotalDeactivatedSupportd
    {
      get => totalDeactivatedSupportd ??= new();
      set => totalDeactivatedSupportd = value;
    }

    /// <summary>
    /// A value of CountAccruingObgDiscont.
    /// </summary>
    [JsonPropertyName("countAccruingObgDiscont")]
    public Common CountAccruingObgDiscont
    {
      get => countAccruingObgDiscont ??= new();
      set => countAccruingObgDiscont = value;
    }

    /// <summary>
    /// A value of CountOnacObgRetired.
    /// </summary>
    [JsonPropertyName("countOnacObgRetired")]
    public Common CountOnacObgRetired
    {
      get => countOnacObgRetired ??= new();
      set => countOnacObgRetired = value;
    }

    /// <summary>
    /// A value of NetJudgement.
    /// </summary>
    [JsonPropertyName("netJudgement")]
    public Common NetJudgement
    {
      get => netJudgement ??= new();
      set => netJudgement = value;
    }

    /// <summary>
    /// A value of ReturnNetObligTrn.
    /// </summary>
    [JsonPropertyName("returnNetObligTrn")]
    public Common ReturnNetObligTrn
    {
      get => returnNetObligTrn ??= new();
      set => returnNetObligTrn = value;
    }

    /// <summary>
    /// A value of NetCurrent.
    /// </summary>
    [JsonPropertyName("netCurrent")]
    public Common NetCurrent
    {
      get => netCurrent ??= new();
      set => netCurrent = value;
    }

    /// <summary>
    /// A value of OblgSetUpForLdet.
    /// </summary>
    [JsonPropertyName("oblgSetUpForLdet")]
    public Common OblgSetUpForLdet
    {
      get => oblgSetUpForLdet ??= new();
      set => oblgSetUpForLdet = value;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public ObligationPaymentSchedule Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of ErrorDay2.
    /// </summary>
    [JsonPropertyName("errorDay2")]
    public Common ErrorDay2
    {
      get => errorDay2 ??= new();
      set => errorDay2 = value;
    }

    /// <summary>
    /// A value of ErrorDay1.
    /// </summary>
    [JsonPropertyName("errorDay1")]
    public Common ErrorDay1
    {
      get => errorDay1 ??= new();
      set => errorDay1 = value;
    }

    /// <summary>
    /// A value of PaymentFreq.
    /// </summary>
    [JsonPropertyName("paymentFreq")]
    public WorkArea PaymentFreq
    {
      get => paymentFreq ??= new();
      set => paymentFreq = value;
    }

    /// <summary>
    /// A value of Day2.
    /// </summary>
    [JsonPropertyName("day2")]
    public Common Day2
    {
      get => day2 ??= new();
      set => day2 = value;
    }

    /// <summary>
    /// A value of Day1.
    /// </summary>
    [JsonPropertyName("day1")]
    public Common Day1
    {
      get => day1 ??= new();
      set => day1 = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of NoOfLegalActionsFound.
    /// </summary>
    [JsonPropertyName("noOfLegalActionsFound")]
    public Common NoOfLegalActionsFound
    {
      get => noOfLegalActionsFound ??= new();
      set => noOfLegalActionsFound = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of TotalSelected.
    /// </summary>
    [JsonPropertyName("totalSelected")]
    public Common TotalSelected
    {
      get => totalSelected ??= new();
      set => totalSelected = value;
    }

    /// <summary>
    /// A value of HiddenTotalSelected.
    /// </summary>
    [JsonPropertyName("hiddenTotalSelected")]
    public Common HiddenTotalSelected
    {
      get => hiddenTotalSelected ??= new();
      set => hiddenTotalSelected = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public Tribunal InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
    }

    /// <summary>
    /// A value of CodeReturnValue.
    /// </summary>
    [JsonPropertyName("codeReturnValue")]
    public Common CodeReturnValue
    {
      get => codeReturnValue ??= new();
      set => codeReturnValue = value;
    }

    /// <summary>
    /// A value of CodeDescription.
    /// </summary>
    [JsonPropertyName("codeDescription")]
    public CodeValue CodeDescription
    {
      get => codeDescription ??= new();
      set => codeDescription = value;
    }

    /// <summary>
    /// A value of Cd.
    /// </summary>
    [JsonPropertyName("cd")]
    public Code Cd
    {
      get => cd ??= new();
      set => cd = value;
    }

    /// <summary>
    /// A value of Cv.
    /// </summary>
    [JsonPropertyName("cv")]
    public CodeValue Cv
    {
      get => cv ??= new();
      set => cv = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    private Common legalActionPersFound;
    private LegalAction initLegalAction;
    private LegalActionDetail initLegalActionDetail;
    private ObligationType initObligationType;
    private FrequencyWorkSet initFrequencyWorkSet;
    private Common prompt;
    private Common totalDeactivatedSupportd;
    private Common countAccruingObgDiscont;
    private Common countOnacObgRetired;
    private Common netJudgement;
    private Common returnNetObligTrn;
    private Common netCurrent;
    private Common oblgSetUpForLdet;
    private DateWorkArea current;
    private ObligationPaymentSchedule error;
    private Common errorDay2;
    private Common errorDay1;
    private WorkArea paymentFreq;
    private Common day2;
    private Common day1;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private DateWorkArea initialisedToZeros;
    private Common noOfLegalActionsFound;
    private DateWorkArea max;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Common totalSelected;
    private Common hiddenTotalSelected;
    private NextTranInfo nextTranInfo;
    private Tribunal initialisedToSpaces;
    private Common codeReturnValue;
    private CodeValue codeDescription;
    private Code cd;
    private CodeValue cv;
    private DateWorkArea maxDate;
    private DateWorkArea zeroDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of LegalActionDetail1.
    /// </summary>
    [JsonPropertyName("legalActionDetail1")]
    public LegalActionDetail LegalActionDetail1
    {
      get => legalActionDetail1 ??= new();
      set => legalActionDetail1 = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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

    private LegalActionPerson legalActionPerson;
    private CsePerson child;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Obligation obligation;
    private LegalActionDetail legalActionDetail1;
    private FipsTribAddress fipsTribAddress;
    private Case1 case1;
    private ObligationType obligationType;
    private Tribunal tribunal;
    private Fips fips;
    private LegalAction legalAction;
  }
#endregion
}
