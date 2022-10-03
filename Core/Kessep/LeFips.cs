// Program: LE_FIPS, ID: 372019253, model: 746.
// Short name: SWEFIPSP
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
/// A program: LE_FIPS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeFips: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_FIPS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeFips(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeFips.
  /// </summary>
  public LeFips(IContext context, Import import, Export export):
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
    // Date		Developer	Request #	Description
    // 04/27/95	Dave Allen			Initial Code
    // 01/04/97	R. Marchman	Add new security/next tran.
    // 01/31/97        R. Welborn	252		Add logic for reading FIPS/Trib Address.
    // 05/01/97	govind		252		Added flow to ORGZ
    // 10/08/98        D. JEAN                         Add not found exit state,
    // change order of checking for "S" or "M" address
    // 03/09/2000	M Ramirez	WR 163		Automatic FIPS Update - Added call to 
    // le_create_fips_trigger
    // 04/04/2000	M Ramirez	WR 163		Allow user to UPDATE FIPS Location code
    // ------------------------------------------------------------
    // 09/19/00        P Phinney       H00103482       Allow records with "000" 
    // counties to display
    // 04/25/01        Madhu Kumar   Edit check for 4 digit and 5 digit zip 
    // codes.
    // 06/10/11       A Hockman        changes to allow the 90 tribunal to have 
    // connections to multiple states.     CQ239
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.Fips.Assign(import.Fips);
    export.PromptState.SelectChar = import.PromptState.SelectChar;
    export.PromptFips.SelectChar = import.PromptFips.SelectChar;

    if (Equal(global.Command, "LIST") || Equal(global.Command, "RLCVAL") || Equal
      (global.Command, "RETFIPL"))
    {
    }
    else
    {
      export.PromptFips.SelectChar = "";
      export.PromptState.SelectChar = "";
    }

    if (Equal(global.Command, "RETFIPL"))
    {
      export.PromptFips.SelectChar = "";

      var field = GetField(export.Fips, "stateAbbreviation");

      field.Protected = false;
      field.Focused = true;

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.Detail.Index = -1;

      if (!import.Detail.IsEmpty)
      {
        for(import.Detail.Index = 0; import.Detail.Index < import.Detail.Count; ++
          import.Detail.Index)
        {
          ++export.Detail.Index;
          export.Detail.CheckSize();

          export.Detail.Update.GexportLineSelect.SelectChar =
            import.Detail.Item.GimportLineSelect.SelectChar;
          export.Detail.Update.GexportDetail.Assign(
            import.Detail.Item.GimportDetail);
          export.Detail.Update.GexportPromptCdvlAddrTyp.PromptField =
            import.Detail.Item.GimportPromptCdvlAddrTp.PromptField;
          export.Detail.Update.GexportPromptCdvlCountry.PromptField =
            import.Detail.Item.GimportPromptCdvlCountry.PromptField;
          export.Detail.Update.GexportPromptCdvlState.PromptField =
            import.Detail.Item.GimportPromptCdvlStates.PromptField;

          if (Equal(global.Command, "LIST") || Equal
            (global.Command, "RLCVAL") || Equal(global.Command, "RETFIPL"))
          {
          }
          else
          {
            export.Detail.Update.GexportPromptCdvlAddrTyp.PromptField = "";
            export.Detail.Update.GexportPromptCdvlCountry.PromptField = "";
            export.Detail.Update.GexportPromptCdvlState.PromptField = "";
          }
        }
      }
    }

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export.
    // ---------------------------------------------
    export.HiddenFips.Assign(import.HiddenFips);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      UseScCabNextTranPut();

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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "RLCVAL") || Equal(global.Command, "RETFIPL") || Equal
      (global.Command, "EXIT") || Equal(global.Command, "RETURN") || Equal
      (global.Command, "SIGNOFF") || Equal(global.Command, "TRIB") || Equal
      (global.Command, "ORGZ"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (!IsEmpty(export.Fips.StateAbbreviation))
      {
      }
      else
      {
        ExitState = "LE0000_STATE_MUST_BE_ENTERED";

        var field = GetField(export.Fips, "stateAbbreviation");

        field.Error = true;

        return;
      }

      if (export.Fips.County != 0)
      {
        if (!IsEmpty(export.Fips.CountyAbbreviation))
        {
        }
        else
        {
          ExitState = "LE0000_FIPS_COUNTY_ABBR_REQD";

          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          return;
        }

        if (!IsEmpty(export.Fips.CountyDescription))
        {
        }
        else
        {
          ExitState = "LE0000_FIPS_COUNTY_DESC_REQD";

          var field = GetField(export.Fips, "countyDescription");

          field.Error = true;

          return;
        }
      }

      // 09/19/00        P Phinney       H00103482       Allow records with "
      // 000" counties to display
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DISPLAY"))
    {
      if (!IsEmpty(export.Fips.StateAbbreviation))
      {
        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = export.Fips.StateAbbreviation;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }
        }

        export.Fips.StateDescription = local.CodeValue.Description;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      for(export.Detail.Index = 0; export.Detail.Index < export.Detail.Count; ++
        export.Detail.Index)
      {
        if (!export.Detail.CheckSize())
        {
          break;
        }

        // :lss 05/14/2007 PR# 00209846  Added code to display error if no 
        // selection is made,
        //  if more than one selection is made, or if selection code is invalid.
        switch(AsChar(export.Detail.Item.GexportLineSelect.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.GroupPromptCount.Count;

            if (local.GroupPromptCount.Count > 1)
            {
              var field1 =
                GetField(export.Detail.Item.GexportLineSelect, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              return;
            }

            break;
          default:
            var field =
              GetField(export.Detail.Item.GexportLineSelect, "selectChar");

            field.Error = true;

            ++local.GroupEntryNo.Count;

            break;
        }
      }

      export.Detail.CheckIndex();

      if (local.GroupEntryNo.Count > 0)
      {
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        return;
      }

      if (local.GroupPromptCount.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }

      export.Detail.Index = 0;

      for(var limit = export.Detail.Count; export.Detail.Index < limit; ++
        export.Detail.Index)
      {
        if (!export.Detail.CheckSize())
        {
          break;
        }

        if (AsChar(export.Detail.Item.GexportLineSelect.SelectChar) == 'S')
        {
          if (Length(TrimEnd(export.Detail.Item.GexportDetail.ZipCode)) > 0 && Length
            (TrimEnd(export.Detail.Item.GexportDetail.ZipCode)) < 5)
          {
            var field = GetField(export.Detail.Item.GexportDetail, "zipCode");

            field.Error = true;

            ExitState = "OE0000_ZIP_CODE_MUST_BE_5_DIGITS";

            return;
          }

          if (Length(TrimEnd(export.Detail.Item.GexportDetail.ZipCode)) > 0 && Verify
            (export.Detail.Item.GexportDetail.ZipCode, "0123456789") != 0)
          {
            var field = GetField(export.Detail.Item.GexportDetail, "zipCode");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }

          if (Length(TrimEnd(export.Detail.Item.GexportDetail.ZipCode)) == 0
            && Length(TrimEnd(export.Detail.Item.GexportDetail.Zip4)) > 0)
          {
            var field = GetField(export.Detail.Item.GexportDetail, "zipCode");

            field.Error = true;

            ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

            return;
          }

          if (Length(TrimEnd(export.Detail.Item.GexportDetail.ZipCode)) > 0 && Length
            (TrimEnd(export.Detail.Item.GexportDetail.Zip4)) > 0)
          {
            if (Length(TrimEnd(export.Detail.Item.GexportDetail.Zip4)) < 4)
            {
              var field = GetField(export.Detail.Item.GexportDetail, "zip4");

              field.Error = true;

              ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

              return;
            }
            else if (Verify(export.Detail.Item.GexportDetail.Zip4, "0123456789") !=
              0)
            {
              var field = GetField(export.Detail.Item.GexportDetail, "zip4");

              field.Error = true;

              ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

              return;
            }
          }

          if (IsEmpty(export.Detail.Item.GexportDetail.City) && IsEmpty
            (export.Detail.Item.GexportDetail.Street1) && IsEmpty
            (export.Detail.Item.GexportDetail.ZipCode) && export
            .Detail.Item.GexportDetail.AreaCode.GetValueOrDefault() == 0 && export
            .Detail.Item.GexportDetail.FaxAreaCode.GetValueOrDefault() == 0 && IsEmpty
            (export.Detail.Item.GexportDetail.FaxExtension) && export
            .Detail.Item.GexportDetail.FaxNumber.GetValueOrDefault() == 0 && IsEmpty
            (export.Detail.Item.GexportDetail.PhoneExtension) && export
            .Detail.Item.GexportDetail.PhoneNumber.GetValueOrDefault() == 0 && IsEmpty
            (export.Detail.Item.GexportDetail.Street2) && IsEmpty
            (export.Detail.Item.GexportDetail.Zip4))
          {
            if (export.Detail.Item.GexportDetail.Identifier > 0)
            {
              // This address is set for deletion.
              continue;
            }
            else
            {
              // --- It is a new address line being created. So proceed with 
              // validations below.
            }
          }

          if (IsEmpty(export.Detail.Item.GexportDetail.Type1))
          {
            var field = GetField(export.Detail.Item.GexportDetail, "type1");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            return;
          }

          local.Code.CodeName = "ADDRESS TYPE";
          local.CodeValue.Cdvalue = export.Detail.Item.GexportDetail.Type1;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            var field = GetField(export.Detail.Item.GexportDetail, "type1");

            field.Error = true;

            export.Detail.Update.GexportPromptCdvlAddrTyp.PromptField = "S";
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }

          if (IsEmpty(export.Detail.Item.GexportDetail.Street1))
          {
            var field = GetField(export.Detail.Item.GexportDetail, "street1");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            return;
          }

          if (IsEmpty(export.Detail.Item.GexportDetail.City))
          {
            var field = GetField(export.Detail.Item.GexportDetail, "city");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            return;
          }

          if (IsEmpty(export.Detail.Item.GexportDetail.ZipCode))
          {
            var field = GetField(export.Detail.Item.GexportDetail, "zipCode");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            return;
          }
        }
      }

      export.Detail.CheckIndex();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "TRIB":
        ExitState = "ECO_XFR_TO_TRIB";

        return;
      case "ORGZ":
        if (export.Fips.State > 0 && export.Fips.County > 0)
        {
          if (ReadTribunalFips())
          {
            export.DlgflwRequiredOrganization.TaxId =
              entities.ExistingRelated.TaxId;
            export.DlgflwRequiredOrganization.TaxIdSuffix =
              entities.ExistingRelated.TaxIdSuffix;
            export.DlgflwRequiredOrganization.OrganizationName =
              entities.ExistingRelated.Name;
          }
          else
          {
            ExitState = "FIPS_NF";

            return;
          }

          // ---Search for street address
          for(export.Detail.Index = 0; export.Detail.Index < export
            .Detail.Count; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            if (AsChar(export.Detail.Item.GexportDetail.Type1) == 'S')
            {
              export.DlgflwRequiredCsePersonAddress.State =
                export.Detail.Item.GexportDetail.State;
              export.DlgflwRequiredCsePersonAddress.County =
                export.Detail.Item.GexportDetail.County ?? "";
              export.DlgflwRequiredCsePersonAddress.City =
                export.Detail.Item.GexportDetail.City;
              local.SaddressFound.Flag = "Y";

              break;
            }
          }

          export.Detail.CheckIndex();

          // ---If street address not found, search for mailing address
          if (AsChar(local.SaddressFound.Flag) != 'Y')
          {
            for(export.Detail.Index = 0; export.Detail.Index < export
              .Detail.Count; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              if (AsChar(export.Detail.Item.GexportDetail.Type1) == 'M')
              {
                export.DlgflwRequiredCsePersonAddress.State =
                  export.Detail.Item.GexportDetail.State;
                export.DlgflwRequiredCsePersonAddress.County =
                  export.Detail.Item.GexportDetail.County ?? "";
                export.DlgflwRequiredCsePersonAddress.City =
                  export.Detail.Item.GexportDetail.City;

                break;
              }
            }

            export.Detail.CheckIndex();
          }
        }

        ExitState = "ECO_LNK_TO_ORGZ";

        return;
      case "DISPLAY":
        // ---------------------------------------------
        // Display FIPS code.
        // ---------------------------------------------
        // 09/19/00        P Phinney       H00103482       Allow records with "
        // 000" counties to display ---  REMOVED Edit for ZERO County
        if (export.Fips.State == 0)
        {
          var field3 = GetField(export.Fips, "county");

          field3.Error = true;

          var field4 = GetField(export.Fips, "location");

          field4.Error = true;

          var field5 = GetField(export.Fips, "state");

          field5.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        if (ReadFips1())
        {
          export.Fips.Assign(entities.Existing);
          export.HiddenFips.Assign(export.Fips);
          UseFnReadEachFipsTribAddr();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (!local.FromReachAddr.IsEmpty)
            {
              export.Detail.Index = -1;

              for(local.FromReachAddr.Index = 0; local.FromReachAddr.Index < local
                .FromReachAddr.Count; ++local.FromReachAddr.Index)
              {
                ++export.Detail.Index;
                export.Detail.CheckSize();

                export.Detail.Update.GexportDetail.Assign(
                  local.FromReachAddr.Item.GlocalDetail);
              }
            }

            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else
          {
          }
        }
        else
        {
          var field3 = GetField(export.Fips, "state");

          field3.Error = true;

          var field4 = GetField(export.Fips, "county");

          field4.Error = true;

          var field5 = GetField(export.Fips, "location");

          field5.Error = true;

          export.Fips.CountyAbbreviation = "";
          export.Fips.CountyDescription = "";
          export.Fips.StateAbbreviation = "";
          export.Fips.StateDescription = "";
          export.Fips.LocationDescription =
            Spaces(Fips.LocationDescription_MaxLength);
          ExitState = "FIPS_NF";
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "LIST":
        for(export.Detail.Index = 0; export.Detail.Index < export.Detail.Count; ++
          export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (AsChar(export.Detail.Item.GexportPromptCdvlAddrTyp.PromptField) ==
            'S')
          {
            ++local.GroupPromptCount.Count;
          }
        }

        export.Detail.CheckIndex();

        switch(AsChar(export.PromptState.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (AsChar(export.PromptFips.SelectChar) == 'S' || local
              .GroupPromptCount.Count > 0)
            {
              var field3 = GetField(export.PromptState, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              return;
            }
            else
            {
              export.Code.CodeName = "STATE CODE";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              return;
            }

            break;
          default:
            var field = GetField(export.PromptState, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.PromptFips.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (AsChar(export.PromptState.SelectChar) == 'S' || local
              .GroupPromptCount.Count > 0)
            {
              var field3 = GetField(export.PromptFips, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              return;
            }
            else
            {
              ExitState = "ECO_LNK_TO_LIST_FIPS";

              return;
            }

            break;
          default:
            var field = GetField(export.PromptFips, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        for(export.Detail.Index = 0; export.Detail.Index < export.Detail.Count; ++
          export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Detail.Item.GexportPromptCdvlAddrTyp.PromptField))
            
          {
            case ' ':
              break;
            case 'S':
              if (AsChar(export.PromptState.SelectChar) == 'S' || AsChar
                (export.PromptFips.SelectChar) == 'S' || local
                .GroupPromptCount.Count > 1)
              {
                var field3 =
                  GetField(export.Detail.Item.GexportPromptCdvlAddrTyp,
                  "promptField");

                field3.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

                return;
              }
              else
              {
                export.Code.CodeName = "ADDRESS TYPE";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                return;
              }

              break;
            default:
              var field =
                GetField(export.Detail.Item.GexportPromptCdvlAddrTyp,
                "promptField");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }
        }

        export.Detail.CheckIndex();

        var field1 = GetField(export.PromptState, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.PromptFips, "selectChar");

        field2.Error = true;

        ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE1";

        break;
      case "RLCVAL":
        // ---------------------------------------------
        // Returned from List Code Values screen. Move
        // values to export.
        // ---------------------------------------------
        if (!IsEmpty(export.PromptState.SelectChar))
        {
          export.PromptState.SelectChar = "";

          var field = GetField(export.Fips, "countyAbbreviation");

          field.Protected = false;
          field.Focused = true;

          if (!IsEmpty(import.DlgflwSelected.Cdvalue))
          {
            export.Fips.StateAbbreviation = import.DlgflwSelected.Cdvalue;
            export.Fips.StateDescription = import.DlgflwSelected.Description;
          }
        }

        for(export.Detail.Index = 0; export.Detail.Index < export.Detail.Count; ++
          export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Detail.Item.GexportPromptCdvlAddrTyp.PromptField) &&
            Equal(import.HiddenRetlink.CodeName, "ADDRESS TYPE"))
          {
            export.Detail.Update.GexportPromptCdvlAddrTyp.PromptField = "";

            var field = GetField(export.Detail.Item.GexportDetail, "street1");

            field.Protected = false;
            field.Focused = true;

            if (!IsEmpty(import.DlgflwSelected.Cdvalue))
            {
              export.Detail.Update.GexportDetail.Type1 =
                import.DlgflwSelected.Cdvalue;
            }

            return;
          }
        }

        export.Detail.CheckIndex();

        break;
      case "ADD":
        if (export.Fips.State == 0)
        {
          var field = GetField(export.Fips, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;

          // 09/19/00        P Phinney       H00103482       Allow records with
          // "000" counties to display ---  REMOVED Edit for ZERO County
        }
        else
        {
          // ***************************************************************
          // * Test to make sure that the state abbreviation entered matches
          // * the abbreviation for an existing FIPS row of that state
          // ***************************************************************
          // *** changes for cq239 when adding new 90 code don't exit if state 
          // already exists.
          if (ReadFips3())
          {
            if (entities.Existing.State != 90)
            {
              if (!Equal(entities.Existing.StateAbbreviation,
                export.Fips.StateAbbreviation))
              {
                ExitState = "INVALID_STATE_ABBREVIATION";

                var field = GetField(export.Fips, "stateAbbreviation");

                field.Error = true;

                return;
              }
            }
          }
          else
          {
            // ***************************************************************
            // * If not found then you are adding a new state FIPS row
            // ***************************************************************
          }
        }

        // ---------------------------------------------
        // Add a new FIPS code.
        // ---------------------------------------------
        try
        {
          CreateFips();
          export.Detail.Index = 0;

          for(var limit = export.Detail.Count; export.Detail.Index < limit; ++
            export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.Detail.Item.GexportLineSelect.SelectChar))
            {
              UseLeCabCreateFipsTribAddress();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();

                return;
              }
            }
          }

          export.Detail.CheckIndex();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              for(export.Detail.Index = 0; export.Detail.Index < export
                .Detail.Count; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Detail.Item.GexportLineSelect.SelectChar) == 'S'
                  )
                {
                  var field =
                    GetField(export.Detail.Item.GexportLineSelect, "selectChar");
                    

                  field.Error = true;

                  ExitState = "LE0000_FIPS_EXIST_USE_PF6_F_ADDR";

                  return;
                }
              }

              export.Detail.CheckIndex();

              var field3 = GetField(export.Fips, "state");

              field3.Error = true;

              var field4 = GetField(export.Fips, "county");

              field4.Error = true;

              var field5 = GetField(export.Fips, "location");

              field5.Error = true;

              ExitState = "FIPS_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          for(export.Detail.Index = 0; export.Detail.Index < export
            .Detail.Count; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            export.Detail.Update.GexportLineSelect.SelectChar = "";
          }

          export.Detail.CheckIndex();
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        if (export.HiddenFips.State == 0 && export.HiddenFips.County == 0 && export
          .HiddenFips.Location == 0)
        {
          var field3 = GetField(export.Fips, "state");

          field3.Error = true;

          var field4 = GetField(export.Fips, "county");

          field4.Error = true;

          var field5 = GetField(export.Fips, "location");

          field5.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // ---------------------------------------------
        // Make sure that none of the identifier
        // attributes have been changed.
        // ---------------------------------------------
        if (export.Fips.State != export.HiddenFips.State)
        {
          var field = GetField(export.Fips, "state");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        if (export.Fips.County != export.HiddenFips.County)
        {
          var field = GetField(export.Fips, "county");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        // ---------------------------------------------
        // Update the current FIPS code.
        // ---------------------------------------------
        if (ReadFips2())
        {
          // ***************************************************************
          // * Test to make sure that the state abbreviation entered matches
          // * the abbreviation for the existing FIPS row to be updated
          // ***************************************************************
          if (!Equal(entities.Existing.StateAbbreviation,
            export.Fips.StateAbbreviation))
          {
            ExitState = "INVALID_STATE_ABBREVIATION";

            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;

            return;
          }
        }
        else
        {
          export.Fips.Location = export.HiddenFips.Location;

          var field3 = GetField(export.Fips, "state");

          field3.Error = true;

          var field4 = GetField(export.Fips, "county");

          field4.Error = true;

          var field5 = GetField(export.Fips, "location");

          field5.Error = true;

          ExitState = "FIPS_NF";

          break;
        }

        try
        {
          UpdateFips();
          export.Detail.Index = 0;

          for(var limit = export.Detail.Count; export.Detail.Index < limit; ++
            export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            if (AsChar(export.Detail.Item.GexportLineSelect.SelectChar) == 'S')
            {
              if (export.Detail.Item.GexportDetail.Identifier == 0)
              {
                UseLeCabCreateFipsTribAddress();
              }
              else
              {
                UseLeCabUpdateFipsTribAddress();
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();

                return;
              }
            }
          }

          export.Detail.CheckIndex();

          if (export.Fips.Location != export.HiddenFips.Location)
          {
            UseLeFipsUpdateLocationCode();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }
          }

          for(export.Detail.Index = 0; export.Detail.Index < export
            .Detail.Count; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            export.Detail.Update.GexportLineSelect.SelectChar = "";
          }

          export.Detail.CheckIndex();
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              var field3 = GetField(export.Fips, "state");

              field3.Error = true;

              var field4 = GetField(export.Fips, "county");

              field4.Error = true;

              var field5 = GetField(export.Fips, "location");

              field5.Error = true;

              ExitState = "FIPS_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "DELETE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the update can take place.
        // ---------------------------------------------
        if (export.HiddenFips.State == 0 && export.HiddenFips.County == 0 && export
          .HiddenFips.Location == 0)
        {
          var field3 = GetField(export.Fips, "state");

          field3.Error = true;

          var field4 = GetField(export.Fips, "county");

          field4.Error = true;

          var field5 = GetField(export.Fips, "location");

          field5.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFOR_UPD_DEL";

          return;
        }

        if (export.Fips.State != export.HiddenFips.State)
        {
          var field = GetField(export.Fips, "state");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        if (export.Fips.County != export.HiddenFips.County)
        {
          var field = GetField(export.Fips, "county");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        if (export.Fips.Location != export.HiddenFips.Location)
        {
          var field = GetField(export.Fips, "location");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        // ---------------------------------------------
        // Delete the current FIPS code. Don't delete a
        // FIPS if it is associated to a Tribunal or an Organization
        // ---------------------------------------------
        if (ReadFips1())
        {
          if (ReadTribunal())
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;

            ExitState = "LE0000_TRIB_PREVENTS_DELETE";

            return;
          }
          else if (ReadOrganization())
          {
            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;

            ExitState = "LE0000_CANT_DELETE_FIPS";

            return;
          }
        }
        else
        {
          var field3 = GetField(export.Fips, "stateAbbreviation");

          field3.Error = true;

          var field4 = GetField(export.Fips, "state");

          field4.Error = true;

          var field5 = GetField(export.Fips, "county");

          field5.Error = true;

          var field6 = GetField(export.Fips, "location");

          field6.Error = true;

          ExitState = "FIPS_NF";

          break;
        }

        export.Detail.Index = -1;

        foreach(var item in ReadFipsTribAddress())
        {
          ++export.Detail.Index;
          export.Detail.CheckSize();

          DeleteFipsTribAddress();
          export.Detail.Update.GexportDetail.Assign(local.Initialized);
        }

        DeleteFips();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        // ---------------------------------------------
        // Sign the user off the KESSEP system.
        // ---------------------------------------------
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // mjr
    // -------------------------------------------------------
    // 03/09/2000
    // Automatic FIPS Update - create a FIPS trigger
    // --------------------------------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_DELETE"))
    {
      if (export.Fips.Location == export.HiddenFips.Location)
      {
        local.Command.Value = global.Command;
        UseLeCreateFipsTrigger1();
      }
      else
      {
        local.Command.Value = "DELETE";
        UseLeCreateFipsTrigger2();
        local.Command.Value = "ADD";
        UseLeCreateFipsTrigger1();
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveExport1ToFromReachAddr(FnReadEachFipsTribAddr.Export.
    ExportGroup source, Local.FromReachAddrGroup target)
  {
    MoveFipsTribAddress2(source.Detail, target.GlocalDetail);
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.State = source.State;
    target.County = source.County;
    target.Location = source.Location;
  }

  private static void MoveFipsTribAddress1(FipsTribAddress source,
    FipsTribAddress target)
  {
    target.Identifier = source.Identifier;
    target.FaxExtension = source.FaxExtension;
    target.FaxAreaCode = source.FaxAreaCode;
    target.PhoneExtension = source.PhoneExtension;
    target.AreaCode = source.AreaCode;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.County = source.County;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.PhoneNumber = source.PhoneNumber;
    target.FaxNumber = source.FaxNumber;
    target.LastUpdatedTstamp = source.LastUpdatedTstamp;
  }

  private static void MoveFipsTribAddress2(FipsTribAddress source,
    FipsTribAddress target)
  {
    target.Identifier = source.Identifier;
    target.FaxExtension = source.FaxExtension;
    target.FaxAreaCode = source.FaxAreaCode;
    target.PhoneExtension = source.PhoneExtension;
    target.AreaCode = source.AreaCode;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.County = source.County;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.PhoneNumber = source.PhoneNumber;
    target.FaxNumber = source.FaxNumber;
    target.LastUpdatedTstamp = source.LastUpdatedTstamp;
  }

  private static void MoveFipsTribAddress3(FipsTribAddress source,
    FipsTribAddress target)
  {
    target.FaxExtension = source.FaxExtension;
    target.FaxAreaCode = source.FaxAreaCode;
    target.PhoneExtension = source.PhoneExtension;
    target.AreaCode = source.AreaCode;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.County = source.County;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.PhoneNumber = source.PhoneNumber;
    target.FaxNumber = source.FaxNumber;
    target.LastUpdatedTstamp = source.LastUpdatedTstamp;
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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnReadEachFipsTribAddr()
  {
    var useImport = new FnReadEachFipsTribAddr.Import();
    var useExport = new FnReadEachFipsTribAddr.Export();

    MoveFips(entities.Existing, useImport.Fips);

    Call(FnReadEachFipsTribAddr.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.FromReachAddr, MoveExport1ToFromReachAddr);
  }

  private void UseLeCabCreateFipsTribAddress()
  {
    var useImport = new LeCabCreateFipsTribAddress.Import();
    var useExport = new LeCabCreateFipsTribAddress.Export();

    MoveFipsTribAddress3(export.Detail.Item.GexportDetail, useImport.ForCreate);
    MoveFips(entities.Existing, useImport.Fips);

    Call(LeCabCreateFipsTribAddress.Execute, useImport, useExport);
  }

  private void UseLeCabUpdateFipsTribAddress()
  {
    var useImport = new LeCabUpdateFipsTribAddress.Import();
    var useExport = new LeCabUpdateFipsTribAddress.Export();

    MoveFipsTribAddress1(export.Detail.Item.GexportDetail, useImport.ForUpdate);

    Call(LeCabUpdateFipsTribAddress.Execute, useImport, useExport);
  }

  private void UseLeCreateFipsTrigger1()
  {
    var useImport = new LeCreateFipsTrigger.Import();
    var useExport = new LeCreateFipsTrigger.Export();

    useImport.Command.Value = local.Command.Value;
    useImport.Fips.Assign(export.Fips);

    Call(LeCreateFipsTrigger.Execute, useImport, useExport);
  }

  private void UseLeCreateFipsTrigger2()
  {
    var useImport = new LeCreateFipsTrigger.Import();
    var useExport = new LeCreateFipsTrigger.Export();

    useImport.Command.Value = local.Command.Value;
    useImport.Fips.Assign(export.HiddenFips);

    Call(LeCreateFipsTrigger.Execute, useImport, useExport);
  }

  private void UseLeFipsUpdateLocationCode()
  {
    var useImport = new LeFipsUpdateLocationCode.Import();
    var useExport = new LeFipsUpdateLocationCode.Export();

    useImport.New1.Assign(export.Fips);
    useImport.Old.Assign(export.HiddenFips);

    Call(LeFipsUpdateLocationCode.Execute, useImport, useExport);
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
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void CreateFips()
  {
    var state = export.Fips.State;
    var county = export.Fips.County;
    var location = export.Fips.Location;
    var stateDescription = export.Fips.StateDescription ?? "";
    var countyDescription = export.Fips.CountyDescription ?? "";
    var locationDescription = export.Fips.LocationDescription ?? "";
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var stateAbbreviation = export.Fips.StateAbbreviation;
    var countyAbbreviation = export.Fips.CountyAbbreviation ?? "";

    entities.Existing.Populated = false;
    Update("CreateFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", state);
        db.SetInt32(command, "county", county);
        db.SetInt32(command, "location", location);
        db.SetNullableString(command, "stateDesc", stateDescription);
        db.SetNullableString(command, "countyDesc", countyDescription);
        db.SetNullableString(command, "locationDesc", locationDescription);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", null);
        db.SetString(command, "stateAbbreviation", stateAbbreviation);
        db.SetNullableString(command, "countyAbbr", countyAbbreviation);
      });

    entities.Existing.State = state;
    entities.Existing.County = county;
    entities.Existing.Location = location;
    entities.Existing.StateDescription = stateDescription;
    entities.Existing.CountyDescription = countyDescription;
    entities.Existing.LocationDescription = locationDescription;
    entities.Existing.CreatedBy = createdBy;
    entities.Existing.CreatedTstamp = createdTstamp;
    entities.Existing.LastUpdatedBy = "";
    entities.Existing.LastUpdatedTstamp = null;
    entities.Existing.StateAbbreviation = stateAbbreviation;
    entities.Existing.CountyAbbreviation = countyAbbreviation;
    entities.Existing.CspNumber = null;
    entities.Existing.Populated = true;
  }

  private void DeleteFips()
  {
    bool exists;

    Update("DeleteFips#1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", entities.Existing.State);
        db.SetNullableInt32(command, "fipCounty", entities.Existing.County);
        db.SetNullableInt32(command, "fipLocation", entities.Existing.Location);
      });

    exists = Read("DeleteFips#2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", entities.Existing.State);
        db.SetNullableInt32(command, "fipCounty", entities.Existing.County);
        db.SetNullableInt32(command, "fipLocation", entities.Existing.Location);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_TRIBUNAL\".", "50001");
    }

    Update("DeleteFips#3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", entities.Existing.State);
        db.SetNullableInt32(command, "fipCounty", entities.Existing.County);
        db.SetNullableInt32(command, "fipLocation", entities.Existing.Location);
      });
  }

  private void DeleteFipsTribAddress()
  {
    Update("DeleteFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", entities.FipsTribAddress.Identifier);
      });
  }

  private bool ReadFips1()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.Fips.State);
        db.SetInt32(command, "county", export.Fips.County);
        db.SetInt32(command, "location", export.Fips.Location);
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.StateDescription = db.GetNullableString(reader, 3);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 4);
        entities.Existing.LocationDescription = db.GetNullableString(reader, 5);
        entities.Existing.CreatedBy = db.GetString(reader, 6);
        entities.Existing.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Existing.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Existing.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Existing.StateAbbreviation = db.GetString(reader, 10);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Existing.CspNumber = db.GetNullableString(reader, 12);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.Fips.State);
        db.SetInt32(command, "county", export.Fips.County);
        db.SetInt32(command, "location", export.HiddenFips.Location);
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.StateDescription = db.GetNullableString(reader, 3);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 4);
        entities.Existing.LocationDescription = db.GetNullableString(reader, 5);
        entities.Existing.CreatedBy = db.GetString(reader, 6);
        entities.Existing.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Existing.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Existing.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Existing.StateAbbreviation = db.GetString(reader, 10);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Existing.CspNumber = db.GetNullableString(reader, 12);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.Fips.State);
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.StateDescription = db.GetNullableString(reader, 3);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 4);
        entities.Existing.LocationDescription = db.GetNullableString(reader, 5);
        entities.Existing.CreatedBy = db.GetString(reader, 6);
        entities.Existing.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.Existing.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Existing.LastUpdatedTstamp = db.GetNullableDateTime(reader, 9);
        entities.Existing.StateAbbreviation = db.GetString(reader, 10);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Existing.CspNumber = db.GetNullableString(reader, 12);
        entities.Existing.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return ReadEach("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Existing.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Existing.County);
        db.SetNullableInt32(command, "fipState", entities.Existing.State);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 1);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 3);
        entities.FipsTribAddress.Populated = true;

        return true;
      });
  }

  private bool ReadOrganization()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.Organization.Populated = false;

    return Read("ReadOrganization",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Existing.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Organization.Number = db.GetString(reader, 0);
        entities.Organization.Type1 = db.GetString(reader, 1);
        entities.Organization.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Organization.Type1);
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipLocation", entities.Existing.Location);
        db.SetNullableInt32(command, "fipCounty", entities.Existing.County);
        db.SetNullableInt32(command, "fipState", entities.Existing.State);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 3);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 4);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 5);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunalFips()
  {
    entities.ExistingRelated.Populated = false;
    entities.Existing.Populated = false;

    return Read("ReadTribunalFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.Fips.State);
        db.SetInt32(command, "county", export.Fips.County);
        db.SetInt32(command, "location", export.Fips.Location);
      },
      (db, reader) =>
      {
        entities.ExistingRelated.Name = db.GetString(reader, 0);
        entities.ExistingRelated.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 1);
        entities.ExistingRelated.Identifier = db.GetInt32(reader, 2);
        entities.ExistingRelated.TaxIdSuffix = db.GetNullableString(reader, 3);
        entities.ExistingRelated.TaxId = db.GetNullableString(reader, 4);
        entities.ExistingRelated.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Existing.County = db.GetInt32(reader, 5);
        entities.ExistingRelated.FipState = db.GetNullableInt32(reader, 6);
        entities.Existing.State = db.GetInt32(reader, 6);
        entities.Existing.StateDescription = db.GetNullableString(reader, 7);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 8);
        entities.Existing.LocationDescription = db.GetNullableString(reader, 9);
        entities.Existing.CreatedBy = db.GetString(reader, 10);
        entities.Existing.CreatedTstamp = db.GetDateTime(reader, 11);
        entities.Existing.LastUpdatedBy = db.GetNullableString(reader, 12);
        entities.Existing.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 13);
        entities.Existing.StateAbbreviation = db.GetString(reader, 14);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 15);
        entities.Existing.CspNumber = db.GetNullableString(reader, 16);
        entities.ExistingRelated.Populated = true;
        entities.Existing.Populated = true;
      });
  }

  private void UpdateFips()
  {
    var stateDescription = export.Fips.StateDescription ?? "";
    var countyDescription = export.Fips.CountyDescription ?? "";
    var locationDescription = export.Fips.LocationDescription ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var stateAbbreviation = export.Fips.StateAbbreviation;
    var countyAbbreviation = export.Fips.CountyAbbreviation ?? "";

    entities.Existing.Populated = false;
    Update("UpdateFips",
      (db, command) =>
      {
        db.SetNullableString(command, "stateDesc", stateDescription);
        db.SetNullableString(command, "countyDesc", countyDescription);
        db.SetNullableString(command, "locationDesc", locationDescription);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "stateAbbreviation", stateAbbreviation);
        db.SetNullableString(command, "countyAbbr", countyAbbreviation);
        db.SetInt32(command, "state", entities.Existing.State);
        db.SetInt32(command, "county", entities.Existing.County);
        db.SetInt32(command, "location", entities.Existing.Location);
      });

    entities.Existing.StateDescription = stateDescription;
    entities.Existing.CountyDescription = countyDescription;
    entities.Existing.LocationDescription = locationDescription;
    entities.Existing.LastUpdatedBy = lastUpdatedBy;
    entities.Existing.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.Existing.StateAbbreviation = stateAbbreviation;
    entities.Existing.CountyAbbreviation = countyAbbreviation;
    entities.Existing.Populated = true;
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
    /// <summary>A DetailGroup group.</summary>
    [Serializable]
    public class DetailGroup
    {
      /// <summary>
      /// A value of GimportLineSelect.
      /// </summary>
      [JsonPropertyName("gimportLineSelect")]
      public Common GimportLineSelect
      {
        get => gimportLineSelect ??= new();
        set => gimportLineSelect = value;
      }

      /// <summary>
      /// A value of GimportPromptCdvlCountry.
      /// </summary>
      [JsonPropertyName("gimportPromptCdvlCountry")]
      public Standard GimportPromptCdvlCountry
      {
        get => gimportPromptCdvlCountry ??= new();
        set => gimportPromptCdvlCountry = value;
      }

      /// <summary>
      /// A value of GimportPromptCdvlAddrTp.
      /// </summary>
      [JsonPropertyName("gimportPromptCdvlAddrTp")]
      public Standard GimportPromptCdvlAddrTp
      {
        get => gimportPromptCdvlAddrTp ??= new();
        set => gimportPromptCdvlAddrTp = value;
      }

      /// <summary>
      /// A value of GimportPromptCdvlStates.
      /// </summary>
      [JsonPropertyName("gimportPromptCdvlStates")]
      public Standard GimportPromptCdvlStates
      {
        get => gimportPromptCdvlStates ??= new();
        set => gimportPromptCdvlStates = value;
      }

      /// <summary>
      /// A value of GimportDetail.
      /// </summary>
      [JsonPropertyName("gimportDetail")]
      public FipsTribAddress GimportDetail
      {
        get => gimportDetail ??= new();
        set => gimportDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common gimportLineSelect;
      private Standard gimportPromptCdvlCountry;
      private Standard gimportPromptCdvlAddrTp;
      private Standard gimportPromptCdvlStates;
      private FipsTribAddress gimportDetail;
    }

    /// <summary>
    /// A value of HiddenRetlink.
    /// </summary>
    [JsonPropertyName("hiddenRetlink")]
    public Code HiddenRetlink
    {
      get => hiddenRetlink ??= new();
      set => hiddenRetlink = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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

    /// <summary>
    /// A value of PromptState.
    /// </summary>
    [JsonPropertyName("promptState")]
    public Common PromptState
    {
      get => promptState ??= new();
      set => promptState = value;
    }

    /// <summary>
    /// A value of PromptFips.
    /// </summary>
    [JsonPropertyName("promptFips")]
    public Common PromptFips
    {
      get => promptFips ??= new();
      set => promptFips = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// Gets a value of Detail.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGroup> Detail => detail ??= new(DetailGroup.Capacity);

    /// <summary>
    /// Gets a value of Detail for json serialization.
    /// </summary>
    [JsonPropertyName("detail")]
    [Computed]
    public IList<DetailGroup> Detail_Json
    {
      get => detail;
      set => Detail.Assign(value);
    }

    private Code hiddenRetlink;
    private Standard standard;
    private Fips fips;
    private Fips hiddenFips;
    private Common promptState;
    private Common promptFips;
    private CodeValue dlgflwSelected;
    private NextTranInfo hiddenNextTranInfo;
    private Array<DetailGroup> detail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DetailGroup group.</summary>
    [Serializable]
    public class DetailGroup
    {
      /// <summary>
      /// A value of GexportLineSelect.
      /// </summary>
      [JsonPropertyName("gexportLineSelect")]
      public Common GexportLineSelect
      {
        get => gexportLineSelect ??= new();
        set => gexportLineSelect = value;
      }

      /// <summary>
      /// A value of GexportPromptCdvlCountry.
      /// </summary>
      [JsonPropertyName("gexportPromptCdvlCountry")]
      public Standard GexportPromptCdvlCountry
      {
        get => gexportPromptCdvlCountry ??= new();
        set => gexportPromptCdvlCountry = value;
      }

      /// <summary>
      /// A value of GexportPromptCdvlAddrTyp.
      /// </summary>
      [JsonPropertyName("gexportPromptCdvlAddrTyp")]
      public Standard GexportPromptCdvlAddrTyp
      {
        get => gexportPromptCdvlAddrTyp ??= new();
        set => gexportPromptCdvlAddrTyp = value;
      }

      /// <summary>
      /// A value of GexportPromptCdvlState.
      /// </summary>
      [JsonPropertyName("gexportPromptCdvlState")]
      public Standard GexportPromptCdvlState
      {
        get => gexportPromptCdvlState ??= new();
        set => gexportPromptCdvlState = value;
      }

      /// <summary>
      /// A value of GexportDetail.
      /// </summary>
      [JsonPropertyName("gexportDetail")]
      public FipsTribAddress GexportDetail
      {
        get => gexportDetail ??= new();
        set => gexportDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common gexportLineSelect;
      private Standard gexportPromptCdvlCountry;
      private Standard gexportPromptCdvlAddrTyp;
      private Standard gexportPromptCdvlState;
      private FipsTribAddress gexportDetail;
    }

    /// <summary>
    /// A value of OnePassInfMsg1.
    /// </summary>
    [JsonPropertyName("onePassInfMsg1")]
    public WorkArea OnePassInfMsg1
    {
      get => onePassInfMsg1 ??= new();
      set => onePassInfMsg1 = value;
    }

    /// <summary>
    /// A value of OnePassInfMsg2.
    /// </summary>
    [JsonPropertyName("onePassInfMsg2")]
    public WorkArea OnePassInfMsg2
    {
      get => onePassInfMsg2 ??= new();
      set => onePassInfMsg2 = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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

    /// <summary>
    /// A value of PromptState.
    /// </summary>
    [JsonPropertyName("promptState")]
    public Common PromptState
    {
      get => promptState ??= new();
      set => promptState = value;
    }

    /// <summary>
    /// A value of PromptFips.
    /// </summary>
    [JsonPropertyName("promptFips")]
    public Common PromptFips
    {
      get => promptFips ??= new();
      set => promptFips = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// Gets a value of Detail.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGroup> Detail => detail ??= new(DetailGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Detail for json serialization.
    /// </summary>
    [JsonPropertyName("detail")]
    [Computed]
    public IList<DetailGroup> Detail_Json
    {
      get => detail;
      set => Detail.Assign(value);
    }

    /// <summary>
    /// A value of DlgflwRequiredOrganization.
    /// </summary>
    [JsonPropertyName("dlgflwRequiredOrganization")]
    public CsePerson DlgflwRequiredOrganization
    {
      get => dlgflwRequiredOrganization ??= new();
      set => dlgflwRequiredOrganization = value;
    }

    /// <summary>
    /// A value of DlgflwRequiredCsePersonAddress.
    /// </summary>
    [JsonPropertyName("dlgflwRequiredCsePersonAddress")]
    public CsePersonAddress DlgflwRequiredCsePersonAddress
    {
      get => dlgflwRequiredCsePersonAddress ??= new();
      set => dlgflwRequiredCsePersonAddress = value;
    }

    private WorkArea onePassInfMsg1;
    private WorkArea onePassInfMsg2;
    private Standard standard;
    private Fips fips;
    private Fips hiddenFips;
    private Common promptState;
    private Common promptFips;
    private Common displayActiveCasesOnly;
    private Code code;
    private NextTranInfo hiddenNextTranInfo;
    private Array<DetailGroup> detail;
    private CsePerson dlgflwRequiredOrganization;
    private CsePersonAddress dlgflwRequiredCsePersonAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A FromReachAddrGroup group.</summary>
    [Serializable]
    public class FromReachAddrGroup
    {
      /// <summary>
      /// A value of GlocalDetail.
      /// </summary>
      [JsonPropertyName("glocalDetail")]
      public FipsTribAddress GlocalDetail
      {
        get => glocalDetail ??= new();
        set => glocalDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private FipsTribAddress glocalDetail;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Command Command
    {
      get => command ??= new();
      set => command = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public FipsTribAddress Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of GroupPromptCount.
    /// </summary>
    [JsonPropertyName("groupPromptCount")]
    public Common GroupPromptCount
    {
      get => groupPromptCount ??= new();
      set => groupPromptCount = value;
    }

    /// <summary>
    /// Gets a value of FromReachAddr.
    /// </summary>
    [JsonIgnore]
    public Array<FromReachAddrGroup> FromReachAddr => fromReachAddr ??= new(
      FromReachAddrGroup.Capacity);

    /// <summary>
    /// Gets a value of FromReachAddr for json serialization.
    /// </summary>
    [JsonPropertyName("fromReachAddr")]
    [Computed]
    public IList<FromReachAddrGroup> FromReachAddr_Json
    {
      get => fromReachAddr;
      set => FromReachAddr.Assign(value);
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
    /// A value of GroupEntryNo.
    /// </summary>
    [JsonPropertyName("groupEntryNo")]
    public Common GroupEntryNo
    {
      get => groupEntryNo ??= new();
      set => groupEntryNo = value;
    }

    /// <summary>
    /// A value of SaddressFound.
    /// </summary>
    [JsonPropertyName("saddressFound")]
    public Common SaddressFound
    {
      get => saddressFound ??= new();
      set => saddressFound = value;
    }

    private Command command;
    private FipsTribAddress initialized;
    private Common groupPromptCount;
    private Array<FromReachAddrGroup> fromReachAddr;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Common groupEntryNo;
    private Common saddressFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Organization.
    /// </summary>
    [JsonPropertyName("organization")]
    public CsePerson Organization
    {
      get => organization ??= new();
      set => organization = value;
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
    /// A value of ExistingRelated.
    /// </summary>
    [JsonPropertyName("existingRelated")]
    public Tribunal ExistingRelated
    {
      get => existingRelated ??= new();
      set => existingRelated = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Fips Existing
    {
      get => existing ??= new();
      set => existing = value;
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

    private CsePerson organization;
    private FipsTribAddress fipsTribAddress;
    private Tribunal existingRelated;
    private Fips existing;
    private Tribunal tribunal;
  }
#endregion
}
