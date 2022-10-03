// Program: LE_AAAD_ADMIN_APPEAL_ADDRESS, ID: 372576156, model: 746.
// Short name: SWEAAADP
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
/// A program: LE_AAAD_ADMIN_APPEAL_ADDRESS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeAaadAdminAppealAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AAAD_ADMIN_APPEAL_ADDRESS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAaadAdminAppealAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAaadAdminAppealAddress.
  /// </summary>
  public LeAaadAdminAppealAddress(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    //   Date		Developer	Request #	Description
    // 06-02-95        S. Benton			Initial development
    // 03/24/98	Siraj Konkader		ZDEL cleanup
    // 04/14/00        Curtis Scroggins Added view matching to enforce security 
    // for family
    //                 violence.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    export.Debug.Msg80 = "";
    export.AdministrativeAppeal.Assign(import.AdministrativeAppeal);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.ListAdmAppealsPrompt.PromptField =
      import.ListAdmAppealsPrompt.PromptField;
    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (export.SsnWorkArea.SsnNumPart1 > 0 || export.SsnWorkArea.SsnNumPart2 > 0
      || export.SsnWorkArea.SsnNumPart3 > 0)
    {
      export.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = export.SsnWorkArea.SsnText9;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else if (!import.Import1.IsEmpty)
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

        export.Export1.Update.DetailAdminAppealAppellantAddress.Assign(
          import.Import1.Item.DetailAdminAppealAppellantAddress);
        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailAddrTpPrmp.SelectChar =
          import.Import1.Item.DetailAddrTpPrmp.SelectChar;
        export.Export1.Update.DetailCountryPrmp.SelectChar =
          import.Import1.Item.DetailCountryProm.SelectChar;
        export.Export1.Update.DetailStatePrompt.SelectChar =
          import.Import1.Item.DetailStatePrompt.SelectChar;
        export.Export1.Update.DetailCountyPrmpt.SelectChar =
          import.Import1.Item.DetailCountyPromp.SelectChar;
        export.Export1.Next();
      }
    }

    // *********************************************
    // Move Hidden Imports to Hidden Exports.
    // *********************************************
    export.HiddenAdministrativeAppeal.Assign(import.HiddenAdministrativeAppeal);
    MoveCsePersonsWorkSet3(import.HiddenCsePersonsWorkSet,
      export.HiddenCsePersonsWorkSet);

    export.Hidden.Index = 0;
    export.Hidden.Clear();

    for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
      import.Hidden.Index)
    {
      if (export.Hidden.IsFull)
      {
        break;
      }

      export.Hidden.Update.HiddenAdminAppealAppellantAddress.Assign(
        import.Hidden.Item.HiddenAdminAppealAppellantAddress);
      export.Hidden.Next();
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RLCVAL"))
    {
      export.ListAdmAppealsPrompt.PromptField = "";
    }

    if (Equal(global.Command, "RETAAPS"))
    {
      global.Command = "DISPLAY";
    }
    else
    {
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.CsePersonsWorkSet.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
        (10);

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "RLCVAL") || Equal(global.Command, "ENTER"))
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
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // *********************************************
    // Perform selection logic common to CREATEs,
    // UPDATEs, and DELETEs.
    // *********************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      local.Prompt.Count = 0;

      if (export.AdministrativeAppeal.Identifier == 0)
      {
        ExitState = "LE0000_ADM_APP_MUST_BE_READ_1ST";

        var field = GetField(export.AdministrativeAppeal, "number");

        field.Error = true;

        return;
      }

      // *********************************************
      // Verify that a display has been performed
      // before the add or update or delete can take place.
      // *********************************************
      if (export.AdministrativeAppeal.Identifier == export
        .HiddenAdministrativeAppeal.Identifier)
      {
        if (!Equal(export.AdministrativeAppeal.Number,
          export.HiddenAdministrativeAppeal.Number))
        {
          var field = GetField(export.AdministrativeAppeal, "number");

          field.Error = true;

          if (Equal(global.Command, "UPDATE"))
          {
            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
          }
          else
          {
            ExitState = "LE0000_ADM_APP_MUST_BE_READ_1ST";
          }

          return;
        }
      }
      else
      {
        var field = GetField(export.AdministrativeAppeal, "number");

        field.Error = true;

        ExitState = "LE0000_ADM_APP_MUST_BE_READ_1ST";

        return;
      }

      // *********************************************
      // Check to see if any address has been selected
      // to be added, updated, or deleted.
      // *********************************************
      if (import.Import1.IsEmpty)
      {
        ExitState = "ADDRESS_NOT_ENTERED";

        return;
      }

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.Prompt.Count;
          }
          else
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.Prompt.Count == 0)
      {
        // *********************************************
        // No address has been selected.  Set the Exit
        // State and Escape.
        // *********************************************
        ExitState = "ZD_ACO_NE0000_NO_SELECTN_MADE_2";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          break;
        }

        return;
      }
      else
      {
      }
    }

    // *********************************************
    // Perform validations common to both CREATEs
    // and UPDATEs.
    // *********************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // *********************************************
      // Required fields  EDIT LOGIC
      // *********************************************
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          if (IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
            Type1))
          {
            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "type1");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }

          if (IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
            Street1))
          {
            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "street1");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }

          if (IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.City))
            
          {
            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "city");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }

          // *****************************************************************
          // State Code > spaces   ==> US address
          // State Code = Spaces  ==> None US address
          // *****************************************************************
          if (!IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
            StateProvince))
          {
            if (IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
              ZipCode))
            {
              var field =
                GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
                "zipCode");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }
            else
            {
              do
              {
                ++local.CheckZip.Count;
                local.CheckZip.Flag =
                  Substring(export.Export1.Item.
                    DetailAdminAppealAppellantAddress.ZipCode,
                  local.CheckZip.Count, 1);

                if (IsEmpty(local.CheckZip.Flag))
                {
                  ++local.CheckZip.TotalReal;
                }

                if (AsChar(local.CheckZip.Flag) < '0' || AsChar
                  (local.CheckZip.Flag) > '9')
                {
                  var field =
                    GetField(export.Export1.Item.
                      DetailAdminAppealAppellantAddress, "zipCode");

                  field.Error = true;

                  local.CheckZip.SelectChar = "Y";
                }
              }
              while(local.CheckZip.Count < 5);

              if (local.CheckZip.TotalReal > 0)
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_INCOMPLETE";
                }
              }
              else if (AsChar(local.CheckZip.SelectChar) == 'Y')
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                }
              }
            }

            if ((!IsEmpty(
              export.Export1.Item.DetailAdminAppealAppellantAddress.Zip4) || !
              IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.Zip3))
              && IsEmpty
              (export.Export1.Item.DetailAdminAppealAppellantAddress.ZipCode))
            {
              var field =
                GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
                "zipCode");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if (!IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
              Zip4))
            {
              local.CheckZip.Count = 0;
              local.CheckZip.TotalReal = 0;

              do
              {
                ++local.CheckZip.Count;
                local.CheckZip.Flag =
                  Substring(export.Export1.Item.
                    DetailAdminAppealAppellantAddress.Zip4,
                  local.CheckZip.Count, 1);

                if (IsEmpty(local.CheckZip.Flag))
                {
                  local.CheckZip.TotalReal = (long)local.Prompt.Count + 1;
                }

                if (AsChar(local.CheckZip.Flag) < '0' || AsChar
                  (local.CheckZip.Flag) > '9')
                {
                  var field =
                    GetField(export.Export1.Item.
                      DetailAdminAppealAppellantAddress, "zip4");

                  field.Error = true;

                  local.CheckZip.SelectChar = "Y";
                }
              }
              while(local.CheckZip.Count < 4);

              if (local.CheckZip.TotalReal > 0)
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_INCOMPLETE";
                }
              }
              else if (AsChar(local.CheckZip.SelectChar) == 'Y')
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                }
              }
            }

            if (!IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
              Country) && !
              Equal(export.Export1.Item.DetailAdminAppealAppellantAddress.
                Country, "US"))
            {
              var field =
                GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
                "country");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_COUNTRY_CODE_MUST_US";
              }
            }
          }
          else
          {
            if (IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
              Country))
            {
              var field =
                GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
                "country");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }
            else if (Equal(export.Export1.Item.
              DetailAdminAppealAppellantAddress.Country, "US"))
            {
              var field =
                GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
                "country");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_COUNTRY_CODE_NONE_US";
              }
            }

            if (IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
              PostalCode))
            {
              var field =
                GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
                "postalCode");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // *********************************************
      // Code Value  EDIT LOGIC
      // *********************************************
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          local.ToBeValidatedCode.CodeName = "ADDRESS TYPE";
          local.ToBeValidatedCodeValue.Cdvalue =
            export.Export1.Item.DetailAdminAppealAppellantAddress.Type1;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "type1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }
        }

        if (!IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
          StateProvince))
        {
          local.ToBeValidatedCode.CodeName = "STATE CODE";
          local.ToBeValidatedCodeValue.Cdvalue =
            export.Export1.Item.DetailAdminAppealAppellantAddress.StateProvince;
            
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "stateProvince");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }
        }

        if (!IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
          Country))
        {
          local.ToBeValidatedCode.CodeName = "COUNTRY CODE";
          local.ToBeValidatedCodeValue.Cdvalue =
            export.Export1.Item.DetailAdminAppealAppellantAddress.Country ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "country");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }
        }
      }
    }

    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // *********************************************
        // Required fields  EDIT LOGIC
        // *********************************************
        if (IsEmpty(export.AdministrativeAppeal.Number) && export
          .AdministrativeAppeal.Identifier == 0)
        {
          var field1 = GetField(export.AdministrativeAppeal, "number");

          field1.Error = true;

          var field2 = GetField(export.ListAdmAppealsPrompt, "promptField");

          field2.Error = true;

          ExitState = "LE0000_ADM_APP_NO_REQD_OR_SEL";

          return;
        }

        if (IsEmpty(export.CsePersonsWorkSet.Number) && !
          IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          local.SearchOption.Flag = "1";
          UseCabMatchCsePerson();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }

          local.Local1.Index = 0;
          local.Local1.CheckSize();

          if (!IsEmpty(local.Local1.Item.CsePersonsWorkSet.Number))
          {
            MoveCsePersonsWorkSet2(local.Local1.Item.CsePersonsWorkSet,
              export.CsePersonsWorkSet);
            UseSiFormatCsePersonName();
          }
        }
        else if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseSiReadCsePerson();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
          }
          else if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }
          else
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }
        }

        UseCabReadAdminAppealAddress2();

        if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          export.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.AdministrativeAppeal, "number");

          field.Error = true;

          return;
        }

        if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseScSecurityCheckForFv();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        if (export.Display.IsEmpty)
        {
          ExitState = "ADMIN_APPEAL_ADDRESS_NF";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Update.DetailAdminAppealAppellantAddress.Assign(
            export.Display.Item.Display1);
          export.Export1.Update.DetailCommon.SelectChar = "";
          export.Export1.Update.DetailAddrTpPrmp.SelectChar = "";
          export.Export1.Update.DetailCountryPrmp.SelectChar = "";
          export.Export1.Update.DetailCountyPrmpt.SelectChar = "";
          export.Export1.Update.DetailStatePrompt.SelectChar = "";
          export.Export1.Next();
        }

        break;
      case "EXIT":
        // ********************************************
        // Allows the user to flow back to the previous
        // screen.
        // ********************************************
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        local.Prompt.Count = 0;

        // *********************************************
        // This command allows the user to link to the
        // Code Value selection list and retrieve the
        // appropriate value, not losing any of the data
        // already entered.
        // *********************************************
        if (!IsEmpty(export.ListAdmAppealsPrompt.PromptField) && AsChar
          (export.ListAdmAppealsPrompt.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListAdmAppealsPrompt, "promptField");

          field.Error = true;

          return;
        }

        if (AsChar(export.ListAdmAppealsPrompt.PromptField) == 'S')
        {
          ++local.Prompt.Count;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailAddrTpPrmp.SelectChar))
          {
            case 'S':
              ++local.Prompt.Count;

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailAddrTpPrmp, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }

          switch(AsChar(export.Export1.Item.DetailCountyPrmpt.SelectChar))
          {
            case 'S':
              ++local.Prompt.Count;

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCountyPrmpt, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }

          switch(AsChar(export.Export1.Item.DetailStatePrompt.SelectChar))
          {
            case 'S':
              ++local.Prompt.Count;

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailStatePrompt, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }

          switch(AsChar(export.Export1.Item.DetailCountryPrmp.SelectChar))
          {
            case 'S':
              ++local.Prompt.Count;

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCountryPrmp, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }
        }

        switch(local.Prompt.Count)
        {
          case 0:
            // *********************************************
            // There was no prompt selected for the LIST
            // command.
            // *********************************************
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
          case 1:
            if (AsChar(export.ListAdmAppealsPrompt.PromptField) == 'S')
            {
              export.HiddenSecurity1.LinkIndicator = "L";
              ExitState = "ECO_LNK_2_ADMN_APPEAL_BY_CSE_PER";

              return;
            }

            for(import.Import1.Index = 0; import.Import1.Index < import
              .Import1.Count; ++import.Import1.Index)
            {
              if (AsChar(import.Import1.Item.DetailAddrTpPrmp.SelectChar) == 'S'
                )
              {
                export.ToDisplay.CodeName = "ADDRESS TYPE";

                break;
              }

              if (AsChar(import.Import1.Item.DetailCountyPromp.SelectChar) == 'S'
                )
              {
                export.ToDisplay.CodeName = "COUNTY CODE";

                break;
              }

              if (AsChar(import.Import1.Item.DetailStatePrompt.SelectChar) == 'S'
                )
              {
                export.ToDisplay.CodeName = "STATE CODE";

                break;
              }

              if (AsChar(import.Import1.Item.DetailCountryProm.SelectChar) == 'S'
                )
              {
                export.ToDisplay.CodeName = "COUNTRY CODE";

                break;
              }
            }

            export.DisplayActiveCasesOnly.Flag = "Y";
            export.HiddenSecurity1.LinkIndicator = "L";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            if (AsChar(export.ListAdmAppealsPrompt.PromptField) == 'S')
            {
              var field = GetField(export.ListAdmAppealsPrompt, "promptField");

              field.Error = true;
            }

            // *********************************************
            // More than one prompt field was selected for
            // the List command.
            // *********************************************
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.DetailAddrTpPrmp.SelectChar) == 'S'
                )
              {
                var field =
                  GetField(export.Export1.Item.DetailAddrTpPrmp, "selectChar");

                field.Error = true;
              }

              if (AsChar(export.Export1.Item.DetailCountyPrmpt.SelectChar) == 'S'
                )
              {
                var field =
                  GetField(export.Export1.Item.DetailCountyPrmpt, "selectChar");
                  

                field.Error = true;
              }

              if (AsChar(export.Export1.Item.DetailStatePrompt.SelectChar) == 'S'
                )
              {
                var field =
                  GetField(export.Export1.Item.DetailStatePrompt, "selectChar");
                  

                field.Error = true;
              }

              if (AsChar(export.Export1.Item.DetailCountryPrmp.SelectChar) == 'S'
                )
              {
                var field =
                  GetField(export.Export1.Item.DetailCountryPrmp, "selectChar");
                  

                field.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
        }

        break;
      case "ADD":
        // *********************************************
        // Insert the USE statement here to call the
        // CREATE action block.
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
              Country) && !
              IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
                StateProvince))
            {
              export.Export1.Update.DetailAdminAppealAppellantAddress.Country =
                "US";
            }

            UseLeCreateAdminAppealAddress();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
            }
            else if (IsExitState("LE0000_ADMINISTRATIVE_APPEAL_NF"))
            {
              var field = GetField(export.AdministrativeAppeal, "type1");

              field.Error = true;

              return;
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
                "type1");

              field1.Error = true;

              var field2 = GetField(export.AdministrativeAppeal, "number");

              field2.Error = true;

              return;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

          // *****************************************************************
          // Per Jan Brigham:  You can not update or delete an address after an 
          // add operation, 12/30/98.
          // *****************************************************************
          export.AdministrativeAppeal.Identifier = 0;
        }
        else
        {
          UseEabRollbackCics();
        }

        break;
      case "UPDATE":
        if (!Equal(import.AdministrativeAppeal.Number,
          import.HiddenAdministrativeAppeal.Number))
        {
          var field = GetField(export.AdministrativeAppeal, "number");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        local.Hidden.Index = -1;

        for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
          import.Hidden.Index)
        {
          ++local.Hidden.Index;
          local.Hidden.CheckSize();

          local.Hidden.Update.Hidden1.Type1 =
            import.Hidden.Item.HiddenAdminAppealAppellantAddress.Type1;
        }

        // *********************************************
        // Insert the USE statement here to call the
        // UPDATE action block.
        // *********************************************
        local.Hidden.Index = -1;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          ++local.Hidden.Index;
          local.Hidden.CheckSize();

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Export1.Item.DetailAdminAppealAppellantAddress.
              Type1) != AsChar(local.Hidden.Item.Hidden1.Type1))
            {
              var field =
                GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
                "type1");

              field.Error = true;

              ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

              return;
            }
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
              Country) && !
              IsEmpty(export.Export1.Item.DetailAdminAppealAppellantAddress.
                StateProvince))
            {
              export.Export1.Update.DetailAdminAppealAppellantAddress.Country =
                "US";
            }

            UseLeUpdateAdminAppealAddress();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.DetailCommon.SelectChar = "";

              continue;
            }
            else
            {
              UseEabRollbackCics();

              var field1 =
                GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
                "type1");

              field1.Error = true;

              var field2 = GetField(export.AdministrativeAppeal, "number");

              field2.Error = true;

              return;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          // *****************************************************************
          // Per Jan Brigham:  You can not update or delete an address after an 
          // add or update operation, 12/30/98.  You must first perform an
          // display.
          // *****************************************************************
          export.AdministrativeAppeal.Identifier = 0;
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ZD_ACO_NE0000_INVALID_FORWARD_3";

        break;
      case "DELETE":
        if (!Equal(import.AdministrativeAppeal.Number,
          import.HiddenAdministrativeAppeal.Number))
        {
          var field = GetField(export.AdministrativeAppeal, "number");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        // *********************************************
        // Insert the USE statement here to call the
        // DELETE action block.
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            UseLeDeleteAdminAppealAddress();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.DetailCommon.SelectChar = "";

              continue;
            }
            else
            {
              UseEabRollbackCics();

              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.AdministrativeAppeal, "number");

              field2.Error = true;

              return;
            }
          }
        }

        // *********************************************
        // Redisplay the address(es) after a successful
        // delete.
        // *********************************************
        UseCabReadAdminAppealAddress1();

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(export.Display.Index = 0; export.Display.Index < export
          .Display.Count; ++export.Display.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Update.DetailAdminAppealAppellantAddress.Assign(
            export.Display.Item.Display1);
          export.Export1.Next();
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "SIGNOFF":
        // ********************************************
        // Sign the user off the Kessep system
        // ********************************************
        // **** begin group F ****
        UseScCabSignoff();

        return;

        // **** end   group F ****
        break;
      case "AAPP":
        ExitState = "ECO_XFR_TO_ADMIN_APPEALS";

        break;
      case "AHEA":
        ExitState = "ECO_XFR_TO_ADMIN_APPEAL_HEARING";

        break;
      case "POST":
        ExitState = "ECO_XFR_TO_POSITION_STATEMENT";

        break;
      case "RLCVAL":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailAddrTpPrmp.SelectChar) == 'S')
          {
            export.Export1.Update.DetailAddrTpPrmp.SelectChar = "";
            export.Export1.Update.DetailAdminAppealAppellantAddress.Type1 =
              import.Dlgflw.Cdvalue;

            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "street1");

            field.Protected = false;
            field.Focused = true;
          }

          if (AsChar(export.Export1.Item.DetailCountyPrmpt.SelectChar) == 'S')
          {
            export.Export1.Update.DetailCountyPrmpt.SelectChar = "";

            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "stateProvince");

            field.Protected = false;
            field.Focused = true;
          }

          if (AsChar(export.Export1.Item.DetailStatePrompt.SelectChar) == 'S')
          {
            export.Export1.Update.DetailStatePrompt.SelectChar = "";
            export.Export1.Update.DetailAdminAppealAppellantAddress.
              StateProvince = import.Dlgflw.Cdvalue;

            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "stateProvince");

            field.Protected = false;
            field.Focused = true;
          }

          if (AsChar(export.Export1.Item.DetailCountryPrmp.SelectChar) == 'S')
          {
            export.Export1.Update.DetailCountryPrmp.SelectChar = "";
            export.Export1.Update.DetailAdminAppealAppellantAddress.Country =
              import.Dlgflw.Cdvalue;

            var field =
              GetField(export.Export1.Item.DetailAdminAppealAppellantAddress,
              "country");

            field.Protected = false;
            field.Focused = true;
          }
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // *********************************************
    // If all processing completed successfully,
    // move all imports to previous exports .
    // *********************************************
    export.HiddenAdministrativeAppeal.Assign(export.AdministrativeAppeal);
    MoveCsePersonsWorkSet3(import.CsePersonsWorkSet,
      export.HiddenCsePersonsWorkSet);

    export.Hidden.Index = 0;
    export.Hidden.Clear();

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (export.Hidden.IsFull)
      {
        break;
      }

      export.Hidden.Update.HiddenAdminAppealAppellantAddress.Assign(
        export.Export1.Item.DetailAdminAppealAppellantAddress);
      export.Hidden.Next();
    }
  }

  private static void MoveAdministrativeAppeal1(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
  }

  private static void MoveAdministrativeAppeal2(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AppellantLastName = source.AppellantLastName;
    target.AppellantFirstName = source.AppellantFirstName;
    target.AppellantMiddleInitial = source.AppellantMiddleInitial;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Percentage = source.Percentage;
    target.Flag = source.Flag;
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
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1ToDisplay(CabReadAdminAppealAddress.Export.
    ExportGroup source, Export.DisplayGroup target)
  {
    target.Display1.Assign(source.AdminAppealAppellantAddress);
  }

  private static void MoveExport1ToLocal1(CabMatchCsePerson.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    MoveCsePersonsWorkSet1(source.Detail, target.CsePersonsWorkSet);
    target.Local5.Flag = source.Ae.Flag;
    target.Local4.Flag = source.Cse.Flag;
    target.Local3.Flag = source.Kanpay.Flag;
    target.Local2.Flag = source.Kscares.Flag;
    target.Local6.Flag = source.Alt.Flag;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabMatchCsePerson()
  {
    var useImport = new CabMatchCsePerson.Import();
    var useExport = new CabMatchCsePerson.Export();

    MoveCommon(local.SearchOption, useImport.Search);
    useImport.CsePersonsWorkSet.Ssn = export.CsePersonsWorkSet.Ssn;

    Call(CabMatchCsePerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
  }

  private void UseCabReadAdminAppealAddress1()
  {
    var useImport = new CabReadAdminAppealAddress.Import();
    var useExport = new CabReadAdminAppealAddress.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
    MoveAdministrativeAppeal1(import.AdministrativeAppeal,
      useImport.AdministrativeAppeal);

    Call(CabReadAdminAppealAddress.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    useExport.Export1.CopyTo(export.Display, MoveExport1ToDisplay);
    MoveAdministrativeAppeal2(useExport.AdministrativeAppeal,
      export.AdministrativeAppeal);
    export.Appellant.Text33 = useExport.AppellantName.Text33;
  }

  private void UseCabReadAdminAppealAddress2()
  {
    var useImport = new CabReadAdminAppealAddress.Import();
    var useExport = new CabReadAdminAppealAddress.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
    MoveAdministrativeAppeal1(export.AdministrativeAppeal,
      useImport.AdministrativeAppeal);

    Call(CabReadAdminAppealAddress.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    useExport.Export1.CopyTo(export.Display, MoveExport1ToDisplay);
    MoveAdministrativeAppeal2(useExport.AdministrativeAppeal,
      export.AdministrativeAppeal);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(export.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = export.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.ToBeValidatedCodeValue.Cdvalue;
    useImport.Code.CodeName = local.ToBeValidatedCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeCreateAdminAppealAddress()
  {
    var useImport = new LeCreateAdminAppealAddress.Import();
    var useExport = new LeCreateAdminAppealAddress.Export();

    useImport.AdminAppealAppellantAddress.Assign(
      export.Export1.Item.DetailAdminAppealAppellantAddress);
    MoveAdministrativeAppeal1(export.AdministrativeAppeal,
      useImport.AdministrativeAppeal);

    Call(LeCreateAdminAppealAddress.Execute, useImport, useExport);
  }

  private void UseLeDeleteAdminAppealAddress()
  {
    var useImport = new LeDeleteAdminAppealAddress.Import();
    var useExport = new LeDeleteAdminAppealAddress.Export();

    MoveAdministrativeAppeal1(import.AdministrativeAppeal,
      useImport.AdministrativeAppeal);
    useImport.AdminAppealAppellantAddress.Type1 =
      export.Export1.Item.DetailAdminAppealAppellantAddress.Type1;

    Call(LeDeleteAdminAppealAddress.Execute, useImport, useExport);
  }

  private void UseLeUpdateAdminAppealAddress()
  {
    var useImport = new LeUpdateAdminAppealAddress.Import();
    var useExport = new LeUpdateAdminAppealAddress.Export();

    MoveAdministrativeAppeal1(import.AdministrativeAppeal,
      useImport.AdministrativeAppeal);
    useImport.AdminAppealAppellantAddress.Assign(
      export.Export1.Item.DetailAdminAppealAppellantAddress);

    Call(LeUpdateAdminAppealAddress.Execute, useImport, useExport);
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

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HiddenCommon.
      /// </summary>
      [JsonPropertyName("hiddenCommon")]
      public Common HiddenCommon
      {
        get => hiddenCommon ??= new();
        set => hiddenCommon = value;
      }

      /// <summary>
      /// A value of HiddenAdminAppealAppellantAddress.
      /// </summary>
      [JsonPropertyName("hiddenAdminAppealAppellantAddress")]
      public AdminAppealAppellantAddress HiddenAdminAppealAppellantAddress
      {
        get => hiddenAdminAppealAppellantAddress ??= new();
        set => hiddenAdminAppealAppellantAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common hiddenCommon;
      private AdminAppealAppellantAddress hiddenAdminAppealAppellantAddress;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCountyPromp.
      /// </summary>
      [JsonPropertyName("detailCountyPromp")]
      public Common DetailCountyPromp
      {
        get => detailCountyPromp ??= new();
        set => detailCountyPromp = value;
      }

      /// <summary>
      /// A value of DetailCountryProm.
      /// </summary>
      [JsonPropertyName("detailCountryProm")]
      public Common DetailCountryProm
      {
        get => detailCountryProm ??= new();
        set => detailCountryProm = value;
      }

      /// <summary>
      /// A value of DetailStatePrompt.
      /// </summary>
      [JsonPropertyName("detailStatePrompt")]
      public Common DetailStatePrompt
      {
        get => detailStatePrompt ??= new();
        set => detailStatePrompt = value;
      }

      /// <summary>
      /// A value of DetailAddrTpPrmp.
      /// </summary>
      [JsonPropertyName("detailAddrTpPrmp")]
      public Common DetailAddrTpPrmp
      {
        get => detailAddrTpPrmp ??= new();
        set => detailAddrTpPrmp = value;
      }

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
      /// A value of DetailAdminAppealAppellantAddress.
      /// </summary>
      [JsonPropertyName("detailAdminAppealAppellantAddress")]
      public AdminAppealAppellantAddress DetailAdminAppealAppellantAddress
      {
        get => detailAdminAppealAppellantAddress ??= new();
        set => detailAdminAppealAppellantAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common detailCountyPromp;
      private Common detailCountryProm;
      private Common detailStatePrompt;
      private Common detailAddrTpPrmp;
      private Common detailCommon;
      private AdminAppealAppellantAddress detailAdminAppealAppellantAddress;
    }

    /// <summary>A DisplayGroup group.</summary>
    [Serializable]
    public class DisplayGroup
    {
      /// <summary>
      /// A value of Display1.
      /// </summary>
      [JsonPropertyName("display1")]
      public AdminAppealAppellantAddress Display1
      {
        get => display1 ??= new();
        set => display1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private AdminAppealAppellantAddress display1;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of ListAdmAppealsPrompt.
    /// </summary>
    [JsonPropertyName("listAdmAppealsPrompt")]
    public Standard ListAdmAppealsPrompt
    {
      get => listAdmAppealsPrompt ??= new();
      set => listAdmAppealsPrompt = value;
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
    /// A value of Dlgflw.
    /// </summary>
    [JsonPropertyName("dlgflw")]
    public CodeValue Dlgflw
    {
      get => dlgflw ??= new();
      set => dlgflw = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of HiddenAppellant.
    /// </summary>
    [JsonPropertyName("hiddenAppellant")]
    public WorkArea HiddenAppellant
    {
      get => hiddenAppellant ??= new();
      set => hiddenAppellant = value;
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
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public ErrorMessageText Debug
    {
      get => debug ??= new();
      set => debug = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
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
    /// Gets a value of Display.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayGroup> Display =>
      display ??= new(DisplayGroup.Capacity);

    /// <summary>
    /// Gets a value of Display for json serialization.
    /// </summary>
    [JsonPropertyName("display")]
    [Computed]
    public IList<DisplayGroup> Display_Json
    {
      get => display;
      set => Display.Assign(value);
    }

    private SsnWorkArea ssnWorkArea;
    private Standard listAdmAppealsPrompt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CodeValue dlgflw;
    private AdministrativeAppeal administrativeAppeal;
    private Standard standard;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private WorkArea hiddenAppellant;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private ErrorMessageText debug;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private Array<HiddenGroup> hidden;
    private Array<ImportGroup> import1;
    private Array<DisplayGroup> display;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HiddenCommon.
      /// </summary>
      [JsonPropertyName("hiddenCommon")]
      public Common HiddenCommon
      {
        get => hiddenCommon ??= new();
        set => hiddenCommon = value;
      }

      /// <summary>
      /// A value of HiddenAdminAppealAppellantAddress.
      /// </summary>
      [JsonPropertyName("hiddenAdminAppealAppellantAddress")]
      public AdminAppealAppellantAddress HiddenAdminAppealAppellantAddress
      {
        get => hiddenAdminAppealAppellantAddress ??= new();
        set => hiddenAdminAppealAppellantAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common hiddenCommon;
      private AdminAppealAppellantAddress hiddenAdminAppealAppellantAddress;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailCountyPrmpt.
      /// </summary>
      [JsonPropertyName("detailCountyPrmpt")]
      public Common DetailCountyPrmpt
      {
        get => detailCountyPrmpt ??= new();
        set => detailCountyPrmpt = value;
      }

      /// <summary>
      /// A value of DetailCountryPrmp.
      /// </summary>
      [JsonPropertyName("detailCountryPrmp")]
      public Common DetailCountryPrmp
      {
        get => detailCountryPrmp ??= new();
        set => detailCountryPrmp = value;
      }

      /// <summary>
      /// A value of DetailStatePrompt.
      /// </summary>
      [JsonPropertyName("detailStatePrompt")]
      public Common DetailStatePrompt
      {
        get => detailStatePrompt ??= new();
        set => detailStatePrompt = value;
      }

      /// <summary>
      /// A value of DetailAddrTpPrmp.
      /// </summary>
      [JsonPropertyName("detailAddrTpPrmp")]
      public Common DetailAddrTpPrmp
      {
        get => detailAddrTpPrmp ??= new();
        set => detailAddrTpPrmp = value;
      }

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
      /// A value of DetailAdminAppealAppellantAddress.
      /// </summary>
      [JsonPropertyName("detailAdminAppealAppellantAddress")]
      public AdminAppealAppellantAddress DetailAdminAppealAppellantAddress
      {
        get => detailAdminAppealAppellantAddress ??= new();
        set => detailAdminAppealAppellantAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common detailCountyPrmpt;
      private Common detailCountryPrmp;
      private Common detailStatePrompt;
      private Common detailAddrTpPrmp;
      private Common detailCommon;
      private AdminAppealAppellantAddress detailAdminAppealAppellantAddress;
    }

    /// <summary>A DisplayGroup group.</summary>
    [Serializable]
    public class DisplayGroup
    {
      /// <summary>
      /// A value of Display1.
      /// </summary>
      [JsonPropertyName("display1")]
      public AdminAppealAppellantAddress Display1
      {
        get => display1 ??= new();
        set => display1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private AdminAppealAppellantAddress display1;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of ListAdmAppealsPrompt.
    /// </summary>
    [JsonPropertyName("listAdmAppealsPrompt")]
    public Standard ListAdmAppealsPrompt
    {
      get => listAdmAppealsPrompt ??= new();
      set => listAdmAppealsPrompt = value;
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
    /// A value of Dlgflw.
    /// </summary>
    [JsonPropertyName("dlgflw")]
    public CodeValue Dlgflw
    {
      get => dlgflw ??= new();
      set => dlgflw = value;
    }

    /// <summary>
    /// A value of ToDisplay.
    /// </summary>
    [JsonPropertyName("toDisplay")]
    public Code ToDisplay
    {
      get => toDisplay ??= new();
      set => toDisplay = value;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of Appellant.
    /// </summary>
    [JsonPropertyName("appellant")]
    public WorkArea Appellant
    {
      get => appellant ??= new();
      set => appellant = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of HiddenAppellant.
    /// </summary>
    [JsonPropertyName("hiddenAppellant")]
    public WorkArea HiddenAppellant
    {
      get => hiddenAppellant ??= new();
      set => hiddenAppellant = value;
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
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public ErrorMessageText Debug
    {
      get => debug ??= new();
      set => debug = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
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
    /// Gets a value of Display.
    /// </summary>
    [JsonIgnore]
    public Array<DisplayGroup> Display =>
      display ??= new(DisplayGroup.Capacity);

    /// <summary>
    /// Gets a value of Display for json serialization.
    /// </summary>
    [JsonPropertyName("display")]
    [Computed]
    public IList<DisplayGroup> Display_Json
    {
      get => display;
      set => Display.Assign(value);
    }

    private SsnWorkArea ssnWorkArea;
    private Standard listAdmAppealsPrompt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CodeValue dlgflw;
    private Code toDisplay;
    private Common displayActiveCasesOnly;
    private AdministrativeAppeal administrativeAppeal;
    private WorkArea appellant;
    private Standard standard;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private WorkArea hiddenAppellant;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private ErrorMessageText debug;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private Array<HiddenGroup> hidden;
    private Array<ExportGroup> export1;
    private Array<DisplayGroup> display;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Local5.
      /// </summary>
      [JsonPropertyName("local5")]
      public Common Local5
      {
        get => local5 ??= new();
        set => local5 = value;
      }

      /// <summary>
      /// A value of Local4.
      /// </summary>
      [JsonPropertyName("local4")]
      public Common Local4
      {
        get => local4 ??= new();
        set => local4 = value;
      }

      /// <summary>
      /// A value of Local3.
      /// </summary>
      [JsonPropertyName("local3")]
      public Common Local3
      {
        get => local3 ??= new();
        set => local3 = value;
      }

      /// <summary>
      /// A value of Local2.
      /// </summary>
      [JsonPropertyName("local2")]
      public Common Local2
      {
        get => local2 ??= new();
        set => local2 = value;
      }

      /// <summary>
      /// A value of Local6.
      /// </summary>
      [JsonPropertyName("local6")]
      public Common Local6
      {
        get => local6 ??= new();
        set => local6 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet csePersonsWorkSet;
      private Common local5;
      private Common local4;
      private Common local3;
      private Common local2;
      private Common local6;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
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
      /// A value of Hidden1.
      /// </summary>
      [JsonPropertyName("hidden1")]
      public AdminAppealAppellantAddress Hidden1
      {
        get => hidden1 ??= new();
        set => hidden1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common common;
      private AdminAppealAppellantAddress hidden1;
    }

    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public Common CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
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
    /// A value of SearchOption.
    /// </summary>
    [JsonPropertyName("searchOption")]
    public Common SearchOption
    {
      get => searchOption ??= new();
      set => searchOption = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of ToBeValidatedCodeValue.
    /// </summary>
    [JsonPropertyName("toBeValidatedCodeValue")]
    public CodeValue ToBeValidatedCodeValue
    {
      get => toBeValidatedCodeValue ??= new();
      set => toBeValidatedCodeValue = value;
    }

    /// <summary>
    /// A value of ToBeValidatedCode.
    /// </summary>
    [JsonPropertyName("toBeValidatedCode")]
    public Code ToBeValidatedCode
    {
      get => toBeValidatedCode ??= new();
      set => toBeValidatedCode = value;
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

    private Common checkZip;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common searchOption;
    private Array<LocalGroup> local1;
    private TextWorkArea textWorkArea;
    private Array<HiddenGroup> hidden;
    private Common prompt;
    private Common validCode;
    private CodeValue toBeValidatedCodeValue;
    private Code toBeValidatedCode;
    private NextTranInfo nextTranInfo;
  }
#endregion
}
