// Program: LE_TRIB_TRIBUNAL, ID: 372021515, model: 746.
// Short name: SWETRIBP
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
/// A program: LE_TRIB_TRIBUNAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step action block maintains TRIBUNAL.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeTribTribunal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_TRIB_TRIBUNAL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeTribTribunal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeTribTribunal.
  /// </summary>
  public LeTribTribunal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    //   Date		Developer     Request#	Description
    // 09-20-95        Govind			Initial development
    // 01/04/97	R. Marchman		Add new security/next tran.
    // 10/08/98	D. Jean     		Combined group view protection, remove group view
    // 					initialization, removed state prompt logic
    // 03/09/00	M Ramirez	WR163	Automatic FIPS Update - added call to 
    // le_create_fips_trigger
    // 04/03/00	M Ramirez	WR163	Disabled Auto FIPS Update code - not necessary 
    // for Tribunal updates
    // 09/15/00	GVandy	     PR102557	Add tribunal document header attributes to 
    // the screen
    // 					and pass appropriately to/from cabs.
    // 10-02-00	P.Phinney   H00104201   Add FIPS on flow from LTRB
    // 11/08/00	GVandy	      PR92039	Correct display of foreign tribunals when 
    // returning from LTRB.
    // ***************************************************************************************
    // 06/16/11       JHarden         CQ27835  Have it error if address type  
    // not selected.
    // ************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.HighlightError.Flag = "N";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move imports to exports
    // ---------------------------------------------
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.PromptCountry.SelectChar = import.PromptCountry.SelectChar;
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;
    export.ListFipsCodes.PromptField = import.ListFipsCodes.PromptField;
    export.ListTribunal.PromptField = import.ListTribunal.PromptField;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    export.HiddenPrev.Assign(import.HiddenPrev);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;

    if (Equal(global.Command, "RETFIPL") || Equal(global.Command, "RETFIPS"))
    {
      if (AsChar(export.ListFipsCodes.PromptField) == 'S')
      {
        export.ListFipsCodes.PromptField = "";
      }

      if (import.SelectedFips.State == 0 && import.SelectedFips.County == 0 && import
        .SelectedFips.Location == 0)
      {
        var field = GetField(export.Fips, "state");

        field.Protected = false;
        field.Focused = true;
      }
      else
      {
        export.Fips.Assign(import.SelectedFips);

        if (ReadFips())
        {
          export.Fips.Assign(entities.Fips);
        }
        else
        {
          ExitState = "FIPS_NF";

          var field1 = GetField(export.Fips, "state");

          field1.Error = true;

          var field2 = GetField(export.Fips, "county");

          field2.Error = true;

          var field3 = GetField(export.Fips, "location");

          field3.Error = true;

          return;
        }

        export.Export1.Index = -1;

        foreach(var item in ReadFipsTribAddress())
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.Detail.Assign(entities.FipsTribAddress);
        }

        var field = GetField(export.FipsTribAddress, "country");

        field.Protected = false;
        field.Focused = true;

        ExitState = "LE0000_FIPS_DETAILS_RETRIEVED";
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "RETFIPL") || Equal(global.Command, "RETFIPS"))
    {
    }
    else if (!import.Import1.IsEmpty)
    {
      local.GroupEntryNo.Count = 0;

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        ++local.GroupEntryNo.Count;

        export.Export1.Index = local.GroupEntryNo.Count - 1;
        export.Export1.CheckSize();

        export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
        export.Export1.Update.DetailListAddrTp.PromptField =
          import.Import1.Item.DetailListAddrTp.PromptField;
        export.Export1.Update.DetailSelAddr.SelectChar =
          import.Import1.Item.DetailSelAddr.SelectChar;
      }
    }

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
      global.Command = "DISPLAY";
    }

    if (export.Fips.State != 0 || !IsEmpty(export.Fips.StateAbbreviation))
    {
      for(export.Export1.Index = 0; export.Export1.Index < Export
        .ExportGroup.Capacity; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        var field1 = GetField(export.Export1.Item.DetailSelAddr, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Export1.Item.DetailListAddrTp, "promptField");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Export1.Item.Detail, "type1");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.Export1.Item.Detail, "street1");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Export1.Item.Detail, "street2");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.Detail, "street3");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Export1.Item.Detail, "street4");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.Export1.Item.Detail, "city");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.Export1.Item.Detail, "state");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.Export1.Item.Detail, "county");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.Export1.Item.Detail, "province");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.Export1.Item.Detail, "country");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.Export1.Item.Detail, "postalCode");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.Export1.Item.Detail, "zipCode");

        field14.Color = "cyan";
        field14.Protected = true;

        var field15 = GetField(export.Export1.Item.Detail, "zip4");

        field15.Color = "cyan";
        field15.Protected = true;

        var field16 = GetField(export.Export1.Item.Detail, "zip3");

        field16.Color = "cyan";
        field16.Protected = true;

        var field17 = GetField(export.Export1.Item.Detail, "areaCode");

        field17.Color = "cyan";
        field17.Protected = true;

        var field18 = GetField(export.Export1.Item.Detail, "phoneNumber");

        field18.Color = "cyan";
        field18.Protected = true;

        var field19 = GetField(export.Export1.Item.Detail, "phoneExtension");

        field19.Color = "cyan";
        field19.Protected = true;

        var field20 = GetField(export.Export1.Item.Detail, "faxAreaCode");

        field20.Color = "cyan";
        field20.Protected = true;

        var field21 = GetField(export.Export1.Item.Detail, "faxNumber");

        field21.Color = "cyan";
        field21.Protected = true;

        var field22 = GetField(export.Export1.Item.Detail, "faxExtension");

        field22.Color = "cyan";
        field22.Protected = true;
      }

      export.Export1.CheckIndex();
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL"))
    {
      local.GroupEntryNo.Count = 0;

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        ++local.GroupEntryNo.Count;

        export.Export1.Index = local.GroupEntryNo.Count - 1;
        export.Export1.CheckSize();

        export.Export1.Update.DetailListAddrTp.PromptField = "";
      }
    }

    if (Equal(global.Command, "RLTRIB") || Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "RETFIPL") || Equal(global.Command, "RETFIPS") || Equal
      (global.Command, "ORGZ"))
    {
    }
    else
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Export1.Item.DetailSelAddr.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            local.AddrTypeErrorFlag.Flag = "Y";

            break;
          default:
            ExitState = "ZD_ACO_NE0000_INVALID_SELECTION";

            var field =
              GetField(export.Export1.Item.DetailSelAddr, "selectChar");

            field.Error = true;

            return;
        }
      }

      export.Export1.CheckIndex();

      // CQ27835 added error message to select address type
      if (AsChar(local.AddrTypeErrorFlag.Flag) != 'Y')
      {
        ExitState = "PLEASE_SELECT_ADDRESS_TYPE";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          var field = GetField(export.Export1.Item.DetailSelAddr, "selectChar");

          field.Error = true;
        }

        export.Export1.CheckIndex();

        return;
      }
    }

    if (Equal(global.Command, "RLTRIB"))
    {
      if (AsChar(export.ListTribunal.PromptField) == 'S')
      {
        export.ListTribunal.PromptField = "";

        if (import.Tribunal.Identifier == 0)
        {
          var field = GetField(export.ListFipsCodes, "promptField");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          export.Fips.Assign(local.NullFips);
          export.FipsTribAddress.Country = local.NullFipsTribAddress.Country;
          global.Command = "DISPLAY";
        }
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RLTRIB":
        break;
      case "ORGZ":
        // --- What do we need to pass ?? Should we not list all the 
        // organizations ? Why would the user flow to ORGZ from TRIB at all ?
        ExitState = "ECO_LNK_TO_ORGZ";

        return;
      case "LIST":
        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (AsChar(import.Import1.Item.DetailListAddrTp.PromptField) == 'S')
          {
            ++local.GroupPrompt.Count;
          }
        }

        if (AsChar(export.PromptCountry.SelectChar) != 'S' && !
          IsEmpty(export.PromptCountry.SelectChar))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.PromptCountry, "selectChar");

          field1.Error = true;

          break;
        }

        if (AsChar(export.PromptCountry.SelectChar) == 'S')
        {
          if (AsChar(export.ListFipsCodes.PromptField) == 'S' || AsChar
            (export.ListTribunal.PromptField) == 'S' || local
            .GroupPrompt.Count > 0)
          {
            var field1 = GetField(export.PromptCountry, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
          }
          else
          {
            export.Required.CodeName = "COUNTRY CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          }
        }

        if (AsChar(export.ListFipsCodes.PromptField) != 'S' && !
          IsEmpty(export.ListFipsCodes.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.ListFipsCodes, "promptField");

          field1.Error = true;

          break;
        }

        if (AsChar(export.ListFipsCodes.PromptField) == 'S')
        {
          if (AsChar(export.PromptCountry.SelectChar) == 'S' || AsChar
            (export.ListTribunal.PromptField) == 'S' || local
            .GroupPrompt.Count > 0)
          {
            var field1 = GetField(export.ListFipsCodes, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
          }
          else
          {
            if (export.Fips.State == 0)
            {
              export.Fips.StateAbbreviation = "KS";
            }

            ExitState = "ECO_LNK_TO_LIST_FIPS";

            break;
          }
        }

        if (AsChar(export.ListTribunal.PromptField) != 'S' && !
          IsEmpty(export.ListTribunal.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.ListTribunal, "promptField");

          field1.Error = true;

          break;
        }

        if (AsChar(export.ListTribunal.PromptField) == 'S')
        {
          if (AsChar(export.ListFipsCodes.PromptField) == 'S' || AsChar
            (export.PromptCountry.SelectChar) == 'S' || local
            .GroupPrompt.Count > 0)
          {
            var field1 = GetField(export.ListTribunal, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
          }
          else
          {
            ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";

            break;
          }
        }

        local.GroupEntryNo.Count = 0;

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          ++local.GroupEntryNo.Count;

          export.Export1.Index = local.GroupEntryNo.Count - 1;
          export.Export1.CheckSize();

          if (AsChar(export.Export1.Item.DetailListAddrTp.PromptField) != 'S'
            && !IsEmpty(export.Export1.Item.DetailListAddrTp.PromptField))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field1 =
              GetField(export.Export1.Item.DetailListAddrTp, "promptField");

            field1.Error = true;

            goto Test;
          }

          if (AsChar(export.Export1.Item.DetailListAddrTp.PromptField) == 'S')
          {
            if (AsChar(export.ListFipsCodes.PromptField) == 'S' || AsChar
              (export.ListTribunal.PromptField) == 'S' || AsChar
              (export.PromptCountry.SelectChar) == 'S' || local
              .GroupPrompt.Count > 1)
            {
              var field1 =
                GetField(export.Export1.Item.DetailListAddrTp, "promptField");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              goto Test;
            }
            else
            {
              export.Required.CodeName = "ADDRESS TYPE";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              goto Test;
            }
          }
        }

        var field = GetField(export.ListFipsCodes, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RETCDVL":
        if (AsChar(export.PromptCountry.SelectChar) == 'S')
        {
          export.PromptCountry.SelectChar = "";

          if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
          {
            var field1 = GetField(export.ListTribunal, "promptField");

            field1.Protected = false;
            field1.Focused = true;

            export.FipsTribAddress.Country = import.SelectedCodeValue.Cdvalue;

            break;
          }
        }

        local.GroupEntryNo.Count = 0;

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          ++local.GroupEntryNo.Count;

          export.Export1.Index = local.GroupEntryNo.Count - 1;
          export.Export1.CheckSize();

          if (AsChar(export.Export1.Item.DetailListAddrTp.PromptField) == 'S')
          {
            export.Export1.Update.DetailListAddrTp.PromptField = "";

            if (IsEmpty(import.SelectedCodeValue.Cdvalue))
            {
              var field1 = GetField(export.Export1.Item.Detail, "type1");

              field1.Protected = false;
              field1.Focused = true;
            }
            else
            {
              export.Export1.Update.Detail.Type1 =
                import.SelectedCodeValue.Cdvalue;

              var field1 = GetField(export.Export1.Item.Detail, "street1");

              field1.Protected = false;
              field1.Focused = true;
            }

            goto Test;
          }
        }

        break;
      case "RETFIPL":
        break;
      case "RETFIPS":
        break;
      case "DISPLAY":
        if (export.Fips.State != 0)
        {
          if (ReadFips())
          {
            export.Fips.Assign(entities.Fips);
          }
          else
          {
            ExitState = "FIPS_NF";

            var field1 = GetField(export.Fips, "state");

            field1.Error = true;

            var field2 = GetField(export.Fips, "county");

            field2.Error = true;

            var field3 = GetField(export.Fips, "location");

            field3.Error = true;

            export.Tribunal.Assign(local.InitialisedToSpacesTribunal);
            export.Fips.CountyAbbreviation = "";
            export.Fips.StateAbbreviation = "";
            export.FipsTribAddress.Country = "";

            break;
          }
        }
        else if (!IsEmpty(export.FipsTribAddress.Country))
        {
        }
        else if (export.Tribunal.Identifier != 0)
        {
        }
        else
        {
          var field1 = GetField(export.Fips, "state");

          field1.Error = true;

          var field2 = GetField(export.Fips, "county");

          field2.Error = true;

          var field3 = GetField(export.Fips, "location");

          field3.Error = true;

          var field4 = GetField(export.FipsTribAddress, "country");

          field4.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        UseLeTribDisplayTribunal();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenDisplayPerformed.Flag = "N";
          export.FipsTribAddress.Country = "";
        }
        else
        {
          export.HiddenDisplayPerformed.Flag = "Y";
          export.FipsTribAddress.Country = "";

          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Export1.Item.Detail.Country))
              {
                export.FipsTribAddress.Country =
                  export.Export1.Item.Detail.Country ?? "";

                break;
              }
            }

            export.Export1.CheckIndex();
          }
          else
          {
            // ***************************************************************
            // * If the address is for a domestic tribunal then protect all
            // * the address occurances.  Address maintenance for domestic
            // * addresses must be done through the FIPS screen, only
            // * foreign tribunal addresses can be maintained in this procedure.
            // ***************************************************************
            for(export.Export1.Index = 0; export.Export1.Index < Export
              .ExportGroup.Capacity; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              var field1 =
                GetField(export.Export1.Item.DetailSelAddr, "selectChar");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Export1.Item.DetailListAddrTp, "promptField");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Export1.Item.Detail, "type1");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.Export1.Item.Detail, "street1");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.Export1.Item.Detail, "street2");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 = GetField(export.Export1.Item.Detail, "street3");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 = GetField(export.Export1.Item.Detail, "street4");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 = GetField(export.Export1.Item.Detail, "city");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 = GetField(export.Export1.Item.Detail, "state");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 = GetField(export.Export1.Item.Detail, "county");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 = GetField(export.Export1.Item.Detail, "province");

              field11.Color = "cyan";
              field11.Protected = true;

              var field12 = GetField(export.Export1.Item.Detail, "country");

              field12.Color = "cyan";
              field12.Protected = true;

              var field13 = GetField(export.Export1.Item.Detail, "postalCode");

              field13.Color = "cyan";
              field13.Protected = true;

              var field14 = GetField(export.Export1.Item.Detail, "zipCode");

              field14.Color = "cyan";
              field14.Protected = true;

              var field15 = GetField(export.Export1.Item.Detail, "zip4");

              field15.Color = "cyan";
              field15.Protected = true;

              var field16 = GetField(export.Export1.Item.Detail, "zip3");

              field16.Color = "cyan";
              field16.Protected = true;

              var field17 = GetField(export.Export1.Item.Detail, "areaCode");

              field17.Color = "cyan";
              field17.Protected = true;

              var field18 = GetField(export.Export1.Item.Detail, "phoneNumber");

              field18.Color = "cyan";
              field18.Protected = true;

              var field19 =
                GetField(export.Export1.Item.Detail, "phoneExtension");

              field19.Color = "cyan";
              field19.Protected = true;

              var field20 = GetField(export.Export1.Item.Detail, "faxAreaCode");

              field20.Color = "cyan";
              field20.Protected = true;

              var field21 = GetField(export.Export1.Item.Detail, "faxNumber");

              field21.Color = "cyan";
              field21.Protected = true;

              var field22 =
                GetField(export.Export1.Item.Detail, "faxExtension");

              field22.Color = "cyan";
              field22.Protected = true;
            }

            export.Export1.CheckIndex();
          }

          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "ADD":
        export.HiddenDisplayPerformed.Flag = "N";
        local.UserAction.Command = global.Command;
        UseLeTribValidateTribunal();

        if (local.LastErrorEntry.Count != 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        UseLeTribCreateTribunal();

        if (!IsExitState("ACO_NN0000_ALL_OK") && !
          IsExitState("LE0000_TRIB_ADDRESS_NOT_CREATED"))
        {
          UseEabRollbackCics();
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

            if (!IsEmpty(export.Export1.Item.DetailSelAddr.SelectChar))
            {
              export.Export1.Update.DetailSelAddr.SelectChar = "";
            }
          }

          export.Export1.CheckIndex();
          export.HiddenDisplayPerformed.Flag = "Y";
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        if (export.Tribunal.Identifier != export.HiddenPrev.Identifier || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y' || !
          Equal(import.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(import.HiddenPrevUserAction.Command, "PREV") && !
          Equal(import.HiddenPrevUserAction.Command, "NEXT") && !
          Equal(import.HiddenPrevUserAction.Command, "UPDATE") && !
          Equal(import.HiddenPrevUserAction.Command, "RETCDVL") && !
          Equal(import.HiddenPrevUserAction.Command, "RETFIPL") && !
          Equal(import.HiddenPrevUserAction.Command, "ADD"))
        {
          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          break;
        }

        local.UserAction.Command = global.Command;
        UseLeTribValidateTribunal();

        if (local.LastErrorEntry.Count != 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        UseLeTribUpdateTribunal();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
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

            if (!IsEmpty(export.Export1.Item.DetailSelAddr.SelectChar))
            {
              export.Export1.Update.DetailSelAddr.SelectChar = "";
            }
          }

          export.Export1.CheckIndex();
          UseLeTribDisplayTribunal();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.HiddenDisplayPerformed.Flag = "N";
          }
          else
          {
            export.HiddenDisplayPerformed.Flag = "Y";

            if (IsEmpty(export.Fips.StateAbbreviation))
            {
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                if (!export.Export1.CheckSize())
                {
                  break;
                }

                if (!IsEmpty(export.Export1.Item.Detail.Country))
                {
                  export.FipsTribAddress.Country =
                    export.Export1.Item.Detail.Country ?? "";

                  break;
                }
              }

              export.Export1.CheckIndex();
            }
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        if (export.Tribunal.Identifier != export.HiddenPrev.Identifier || AsChar
          (import.HiddenDisplayPerformed.Flag) != 'Y' || !
          Equal(import.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(import.HiddenPrevUserAction.Command, "ADD") && !
          Equal(import.HiddenPrevUserAction.Command, "PREV") && !
          Equal(import.HiddenPrevUserAction.Command, "NEXT") && !
          Equal(import.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        local.UserAction.Command = global.Command;
        UseLeTribValidateTribunal();

        if (local.LastErrorEntry.Count != 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        UseLeTribDeleteTribunal();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
        }
        else
        {
          export.Fips.Assign(local.InitialisedToSpacesFips);
          export.Tribunal.Assign(local.InitialisedToSpacesTribunal);
          export.FipsTribAddress.Country = "";
          local.GroupEntryNo.Count = 0;

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            // --------------------
            // export group to be cleared
            // ---------------------
            export.Export1.Update.Detail.Assign(
              local.InitialisedToSpacesFipsTribAddress);
            export.Export1.Update.DetailListAddrTp.PromptField = "";
            export.Export1.Update.DetailSelAddr.SelectChar = "";
          }

          export.Export1.CheckIndex();
          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "FIPS":
        ExitState = "ECO_LNK_TO_FIPS1";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test:

    MoveTribunal(export.Tribunal, export.HiddenPrev);

    if (AsChar(export.HiddenDisplayPerformed.Flag) == 'N')
    {
      export.HiddenPrevUserAction.Command = "INVALID";
    }
    else
    {
      export.HiddenPrevUserAction.Command = global.Command;
    }

    // ---------------------------------------------
    // If any validation error was encountered, highlight the error and display 
    // the error message.
    // ---------------------------------------------
    if (AsChar(local.HighlightError.Flag) == 'Y')
    {
      local.Errors.Index = local.LastErrorEntry.Count - 1;
      local.Errors.CheckSize();

      while(local.Errors.Index >= 0)
      {
        switch(local.Errors.Item.DetailErrorCode.Count)
        {
          case 1:
            var field1 = GetField(export.Fips, "location");

            field1.Error = true;

            var field2 = GetField(export.Fips, "county");

            field2.Error = true;

            var field3 = GetField(export.Fips, "state");

            field3.Error = true;

            var field4 = GetField(export.FipsTribAddress, "country");

            field4.Error = true;

            ExitState = "COUNTRY_OR_STATE_CODE_REQD";

            break;
          case 2:
            ExitState = "INVALID_FIPS_STATE_COUNTY_LOCN";

            var field5 = GetField(export.Fips, "location");

            field5.Error = true;

            var field6 = GetField(export.Fips, "county");

            field6.Error = true;

            var field7 = GetField(export.Fips, "state");

            field7.Error = true;

            break;
          case 3:
            ExitState = "LE0000_TRIBUNAL_NAME_REQD";

            var field8 = GetField(export.Tribunal, "name");

            field8.Error = true;

            break;
          case 4:
            ExitState = "LE0000_CANT_ENTR_STATE_N_COUNTRY";

            var field9 = GetField(export.Fips, "state");

            field9.Error = true;

            var field10 = GetField(export.Fips, "county");

            field10.Error = true;

            var field11 = GetField(export.Fips, "location");

            field11.Error = true;

            var field12 = GetField(export.FipsTribAddress, "country");

            field12.Error = true;

            break;
          case 5:
            ExitState = "TRIBUNAL_AE";

            var field13 = GetField(export.Tribunal, "name");

            field13.Error = true;

            if (export.Tribunal.Identifier > 0)
            {
              var field = GetField(export.Tribunal, "identifier");

              field.Error = true;
            }

            if (!IsEmpty(export.Tribunal.JudicialDivision))
            {
              var field = GetField(export.Tribunal, "judicialDivision");

              field.Error = true;
            }

            if (!IsEmpty(export.Tribunal.JudicialDistrict))
            {
              var field = GetField(export.Tribunal, "judicialDistrict");

              field.Error = true;
            }

            break;
          case 6:
            ExitState = "TRIBUNAL_NF";

            if (export.Tribunal.Identifier > 0)
            {
              var field = GetField(export.Tribunal, "identifier");

              field.Error = true;
            }

            if (!IsEmpty(export.Tribunal.JudicialDivision))
            {
              var field = GetField(export.Tribunal, "judicialDivision");

              field.Error = true;
            }

            if (!IsEmpty(export.Tribunal.JudicialDistrict))
            {
              var field = GetField(export.Tribunal, "judicialDistrict");

              field.Error = true;
            }

            break;
          case 7:
            ExitState = "LE0000_TRIBUNAL_ADDRESS_REQD";

            export.Export1.Index = 0;
            export.Export1.CheckSize();

            var field14 = GetField(export.Export1.Item.Detail, "city");

            field14.Error = true;

            var field15 = GetField(export.Export1.Item.Detail, "street1");

            field15.Error = true;

            var field16 = GetField(export.Export1.Item.Detail, "type1");

            field16.Error = true;

            break;
          case 8:
            ExitState = "LE0000_INVALID_ADDRESS_TYPE";

            export.Export1.Index = local.Errors.Item.DetailErrEntryNo.Count - 1;
            export.Export1.CheckSize();

            var field17 = GetField(export.Export1.Item.Detail, "type1");

            field17.Error = true;

            break;
          case 9:
            ExitState = "LE0000_STREET_1_REQD";

            export.Export1.Index = local.Errors.Item.DetailErrEntryNo.Count - 1;
            export.Export1.CheckSize();

            var field18 = GetField(export.Export1.Item.Detail, "street1");

            field18.Error = true;

            break;
          case 10:
            ExitState = "LE0000_CITY_REQD";

            export.Export1.Index = local.Errors.Item.DetailErrEntryNo.Count - 1;
            export.Export1.CheckSize();

            var field19 = GetField(export.Export1.Item.Detail, "city");

            field19.Error = true;

            break;
          case 11:
            break;
          case 12:
            break;
          case 13:
            break;
          case 14:
            break;
          case 15:
            break;
          case 16:
            var field20 = GetField(export.FipsTribAddress, "country");

            field20.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            break;
          case 17:
            var field21 = GetField(export.Tribunal, "documentHeader1");

            field21.Error = true;

            ExitState = "LE0000_TRIB_DOC_HEADER_REQUIRED";

            break;
          default:
            ExitState = "ACO_NE0000_UNKNOWN_ERROR_CODE";

            break;
        }

        --local.Errors.Index;
        local.Errors.CheckSize();
      }
    }
  }

  private static void MoveErrorCodesToErrors(LeTribValidateTribunal.Export.
    ErrorCodesGroup source, Local.ErrorsGroup target)
  {
    target.DetailErrEntryNo.Count = source.DetailErrEntryNo.Count;
    target.DetailErrorCode.Count = source.DetailErrorCode.Count;
  }

  private static void MoveExport2(LeTribDisplayTribunal.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailSelAddr.SelectChar = source.DetailSelAddr.SelectChar;
    target.DetailListAddrTp.PromptField = source.DetailListAddrTp.PromptField;
    target.DetailListStates.PromptField = source.DetailListStates.PromptField;
    target.Detail.Assign(source.Detail);
  }

  private static void MoveExport3(LeTribUpdateTribunal.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailSelAddr.SelectChar = source.DetailSelAddr.SelectChar;
    target.DetailListAddrTp.PromptField = source.DetailListAddrTp.PromptField;
    target.DetailListStates.PromptField = source.DetailListStates.PromptField;
    target.Detail.Assign(source.Detail);
  }

  private static void MoveExport4(LeTribCreateTribunal.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailSelAddr.SelectChar = source.DetailSelAddr.SelectChar;
    target.DetailListAddrTp.PromptField = source.DetailListAddrTp.PromptField;
    target.DetailListStates.PromptField = source.DetailListStates.PromptField;
    target.Detail.Assign(source.Detail);
  }

  private static void MoveExport1ToImport2(Export.ExportGroup source,
    LeTribUpdateTribunal.Import.ImportGroup target)
  {
    target.DetailSelAddr.SelectChar = source.DetailSelAddr.SelectChar;
    target.DetailListAddrTp.PromptField = source.DetailListAddrTp.PromptField;
    target.DetailListStates.PromptField = source.DetailListStates.PromptField;
    target.Detail.Assign(source.Detail);
  }

  private static void MoveExport1ToImport3(Export.ExportGroup source,
    LeTribCreateTribunal.Import.ImportGroup target)
  {
    target.DetailSelAddr.SelectChar = source.DetailSelAddr.SelectChar;
    target.DetailListAddrTp.PromptField = source.DetailListAddrTp.PromptField;
    target.DetailListStates.PromptField = source.DetailListStates.PromptField;
    target.Detail.Assign(source.Detail);
  }

  private static void MoveExport1ToImport4(Export.ExportGroup source,
    LeTribValidateTribunal.Import.ImportGroup target)
  {
    target.DetailSelAddr.SelectChar = source.DetailSelAddr.SelectChar;
    target.DetailListAddrTp.PromptField = source.DetailListAddrTp.PromptField;
    target.DetailListStates.PromptField = source.DetailListStates.PromptField;
    target.Detail.Assign(source.Detail);
  }

  private static void MoveFips1(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.State = source.State;
    target.County = source.County;
    target.Location = source.Location;
  }

  private static void MoveFips2(Fips source, Fips target)
  {
    target.State = source.State;
    target.County = source.County;
    target.Location = source.Location;
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

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
    target.JudicialDivision = source.JudicialDivision;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeTribCreateTribunal()
  {
    var useImport = new LeTribCreateTribunal.Import();
    var useExport = new LeTribCreateTribunal.Export();

    useImport.FipsTribAddress.Country = export.FipsTribAddress.Country;
    useImport.Fips.Assign(export.Fips);
    useImport.Tribunal.Assign(export.Tribunal);
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source has greater capacity than target.");
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport3);

    Call(LeTribCreateTribunal.Execute, useImport, useExport);

    export.FipsTribAddress.Country = useExport.FipsTribAddress.Country;
    MoveFips1(useExport.Fips, export.Fips);
    export.Tribunal.Assign(useExport.Tribunal);
    useExport.Export1.CopyTo(export.Export1, MoveExport4);
  }

  private void UseLeTribDeleteTribunal()
  {
    var useImport = new LeTribDeleteTribunal.Import();
    var useExport = new LeTribDeleteTribunal.Export();

    MoveTribunal(export.Tribunal, useImport.Tribunal);

    Call(LeTribDeleteTribunal.Execute, useImport, useExport);
  }

  private void UseLeTribDisplayTribunal()
  {
    var useImport = new LeTribDisplayTribunal.Import();
    var useExport = new LeTribDisplayTribunal.Export();

    useImport.Fips.Assign(export.Fips);
    MoveTribunal(export.Tribunal, useImport.Tribunal);

    Call(LeTribDisplayTribunal.Execute, useImport, useExport);

    export.Fips.Assign(useExport.Fips);
    export.Tribunal.Assign(useExport.Tribunal);
    useExport.Export1.CopyTo(export.Export1, MoveExport2);
  }

  private void UseLeTribUpdateTribunal()
  {
    var useImport = new LeTribUpdateTribunal.Import();
    var useExport = new LeTribUpdateTribunal.Export();

    useImport.FipsTribAddress.Country = export.FipsTribAddress.Country;
    useImport.Fips.Assign(export.Fips);
    useImport.Tribunal.Assign(export.Tribunal);
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source has greater capacity than target.");
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport2);

    Call(LeTribUpdateTribunal.Execute, useImport, useExport);

    MoveFips2(useExport.Fips, export.Fips);
    export.Tribunal.Assign(useExport.Tribunal);
    useExport.Export1.CopyTo(export.Export1, MoveExport3);
  }

  private void UseLeTribValidateTribunal()
  {
    var useImport = new LeTribValidateTribunal.Import();
    var useExport = new LeTribValidateTribunal.Export();

    useImport.FipsTribAddress.Country = export.FipsTribAddress.Country;
    useImport.UserAction.Command = local.UserAction.Command;
    useImport.Fips.Assign(export.Fips);
    useImport.Tribunal.Assign(export.Tribunal);
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source has greater capacity than target.");
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport4);

    Call(LeTribValidateTribunal.Execute, useImport, useExport);

    export.Fips.Assign(useExport.Fips);
    local.LastErrorEntry.Count = useExport.LastErrorEntry.Count;
    useExport.ErrorCodes.CopyTo(local.Errors, MoveErrorCodesToErrors);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", export.Fips.State);
        db.SetInt32(command, "county", export.Fips.County);
        db.SetInt32(command, "location", export.Fips.Location);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 4);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 5);
        entities.Fips.StateAbbreviation = db.GetString(reader, 6);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 7);
        entities.Fips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return ReadEach("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", export.Fips.State);
        db.SetNullableInt32(command, "fipCounty", export.Fips.County);
        db.SetNullableInt32(command, "fipLocation", export.Fips.Location);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FaxExtension = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.FaxAreaCode = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.FipsTribAddress.AreaCode = db.GetNullableInt32(reader, 4);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.City = db.GetString(reader, 8);
        entities.FipsTribAddress.State = db.GetString(reader, 9);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 11);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 13);
        entities.FipsTribAddress.Street3 = db.GetNullableString(reader, 14);
        entities.FipsTribAddress.Street4 = db.GetNullableString(reader, 15);
        entities.FipsTribAddress.Province = db.GetNullableString(reader, 16);
        entities.FipsTribAddress.PostalCode = db.GetNullableString(reader, 17);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 18);
        entities.FipsTribAddress.PhoneNumber = db.GetNullableInt32(reader, 19);
        entities.FipsTribAddress.FaxNumber = db.GetNullableInt32(reader, 20);
        entities.FipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.FipsTribAddress.CreatedTstamp = db.GetDateTime(reader, 22);
        entities.FipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.FipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 25);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 26);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 27);
        entities.FipsTribAddress.Populated = true;

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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailSelAddr.
      /// </summary>
      [JsonPropertyName("detailSelAddr")]
      public Common DetailSelAddr
      {
        get => detailSelAddr ??= new();
        set => detailSelAddr = value;
      }

      /// <summary>
      /// A value of DetailListAddrTp.
      /// </summary>
      [JsonPropertyName("detailListAddrTp")]
      public Standard DetailListAddrTp
      {
        get => detailListAddrTp ??= new();
        set => detailListAddrTp = value;
      }

      /// <summary>
      /// A value of DetailListStates.
      /// </summary>
      [JsonPropertyName("detailListStates")]
      public Standard DetailListStates
      {
        get => detailListStates ??= new();
        set => detailListStates = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public FipsTribAddress Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common detailSelAddr;
      private Standard detailListAddrTp;
      private Standard detailListStates;
      private FipsTribAddress detail;
    }

    /// <summary>
    /// A value of ListTribunal.
    /// </summary>
    [JsonPropertyName("listTribunal")]
    public Standard ListTribunal
    {
      get => listTribunal ??= new();
      set => listTribunal = value;
    }

    /// <summary>
    /// A value of SelectedFips.
    /// </summary>
    [JsonPropertyName("selectedFips")]
    public Fips SelectedFips
    {
      get => selectedFips ??= new();
      set => selectedFips = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Tribunal HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of ListFipsCodes.
    /// </summary>
    [JsonPropertyName("listFipsCodes")]
    public Standard ListFipsCodes
    {
      get => listFipsCodes ??= new();
      set => listFipsCodes = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of PromptCountry.
    /// </summary>
    [JsonPropertyName("promptCountry")]
    public Common PromptCountry
    {
      get => promptCountry ??= new();
      set => promptCountry = value;
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

    private Standard listTribunal;
    private Fips selectedFips;
    private CodeValue selectedCodeValue;
    private Common hiddenPrevUserAction;
    private Tribunal hiddenPrev;
    private Common hiddenDisplayPerformed;
    private Standard listFipsCodes;
    private Fips fips;
    private Tribunal tribunal;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private Common promptCountry;
    private FipsTribAddress fipsTribAddress;
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
      /// A value of DetailSelAddr.
      /// </summary>
      [JsonPropertyName("detailSelAddr")]
      public Common DetailSelAddr
      {
        get => detailSelAddr ??= new();
        set => detailSelAddr = value;
      }

      /// <summary>
      /// A value of DetailListAddrTp.
      /// </summary>
      [JsonPropertyName("detailListAddrTp")]
      public Standard DetailListAddrTp
      {
        get => detailListAddrTp ??= new();
        set => detailListAddrTp = value;
      }

      /// <summary>
      /// A value of DetailListStates.
      /// </summary>
      [JsonPropertyName("detailListStates")]
      public Standard DetailListStates
      {
        get => detailListStates ??= new();
        set => detailListStates = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public FipsTribAddress Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common detailSelAddr;
      private Standard detailListAddrTp;
      private Standard detailListStates;
      private FipsTribAddress detail;
    }

    /// <summary>
    /// A value of DlgflwRequiredCsePerson.
    /// </summary>
    [JsonPropertyName("dlgflwRequiredCsePerson")]
    public CsePerson DlgflwRequiredCsePerson
    {
      get => dlgflwRequiredCsePerson ??= new();
      set => dlgflwRequiredCsePerson = value;
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

    /// <summary>
    /// A value of ListTribunal.
    /// </summary>
    [JsonPropertyName("listTribunal")]
    public Standard ListTribunal
    {
      get => listTribunal ??= new();
      set => listTribunal = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public Code Required
    {
      get => required ??= new();
      set => required = value;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Tribunal HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of ListFipsCodes.
    /// </summary>
    [JsonPropertyName("listFipsCodes")]
    public Standard ListFipsCodes
    {
      get => listFipsCodes ??= new();
      set => listFipsCodes = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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
    /// A value of PromptCountry.
    /// </summary>
    [JsonPropertyName("promptCountry")]
    public Common PromptCountry
    {
      get => promptCountry ??= new();
      set => promptCountry = value;
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

    private CsePerson dlgflwRequiredCsePerson;
    private CsePersonAddress dlgflwRequiredCsePersonAddress;
    private Standard listTribunal;
    private Code required;
    private Common hiddenPrevUserAction;
    private Tribunal hiddenPrev;
    private Common hiddenDisplayPerformed;
    private Standard listFipsCodes;
    private Fips fips;
    private Tribunal tribunal;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
    private Common promptCountry;
    private FipsTribAddress fipsTribAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ErrorsGroup group.</summary>
    [Serializable]
    public class ErrorsGroup
    {
      /// <summary>
      /// A value of DetailErrEntryNo.
      /// </summary>
      [JsonPropertyName("detailErrEntryNo")]
      public Common DetailErrEntryNo
      {
        get => detailErrEntryNo ??= new();
        set => detailErrEntryNo = value;
      }

      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrEntryNo;
      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of AddrTypeErrorFlag.
    /// </summary>
    [JsonPropertyName("addrTypeErrorFlag")]
    public Common AddrTypeErrorFlag
    {
      get => addrTypeErrorFlag ??= new();
      set => addrTypeErrorFlag = value;
    }

    /// <summary>
    /// A value of NullFipsTribAddress.
    /// </summary>
    [JsonPropertyName("nullFipsTribAddress")]
    public FipsTribAddress NullFipsTribAddress
    {
      get => nullFipsTribAddress ??= new();
      set => nullFipsTribAddress = value;
    }

    /// <summary>
    /// A value of NullFips.
    /// </summary>
    [JsonPropertyName("nullFips")]
    public Fips NullFips
    {
      get => nullFips ??= new();
      set => nullFips = value;
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
    /// A value of GroupPrompt.
    /// </summary>
    [JsonPropertyName("groupPrompt")]
    public Common GroupPrompt
    {
      get => groupPrompt ??= new();
      set => groupPrompt = value;
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
    /// A value of LastErrorEntry.
    /// </summary>
    [JsonPropertyName("lastErrorEntry")]
    public Common LastErrorEntry
    {
      get => lastErrorEntry ??= new();
      set => lastErrorEntry = value;
    }

    /// <summary>
    /// A value of InitialisedToSpacesFipsTribAddress.
    /// </summary>
    [JsonPropertyName("initialisedToSpacesFipsTribAddress")]
    public FipsTribAddress InitialisedToSpacesFipsTribAddress
    {
      get => initialisedToSpacesFipsTribAddress ??= new();
      set => initialisedToSpacesFipsTribAddress = value;
    }

    /// <summary>
    /// A value of InitialisedToSpacesTribunal.
    /// </summary>
    [JsonPropertyName("initialisedToSpacesTribunal")]
    public Tribunal InitialisedToSpacesTribunal
    {
      get => initialisedToSpacesTribunal ??= new();
      set => initialisedToSpacesTribunal = value;
    }

    /// <summary>
    /// A value of InitialisedToSpacesFips.
    /// </summary>
    [JsonPropertyName("initialisedToSpacesFips")]
    public Fips InitialisedToSpacesFips
    {
      get => initialisedToSpacesFips ??= new();
      set => initialisedToSpacesFips = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of HighlightError.
    /// </summary>
    [JsonPropertyName("highlightError")]
    public Common HighlightError
    {
      get => highlightError ??= new();
      set => highlightError = value;
    }

    /// <summary>
    /// Gets a value of Errors.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorsGroup> Errors => errors ??= new(ErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Errors for json serialization.
    /// </summary>
    [JsonPropertyName("errors")]
    [Computed]
    public IList<ErrorsGroup> Errors_Json
    {
      get => errors;
      set => Errors.Assign(value);
    }

    private Common addrTypeErrorFlag;
    private FipsTribAddress nullFipsTribAddress;
    private Fips nullFips;
    private Command command;
    private Common groupPrompt;
    private Common groupEntryNo;
    private Common lastErrorEntry;
    private FipsTribAddress initialisedToSpacesFipsTribAddress;
    private Tribunal initialisedToSpacesTribunal;
    private Fips initialisedToSpacesFips;
    private Common userAction;
    private Common highlightError;
    private Array<ErrorsGroup> errors;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private Fips fips;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
  }
#endregion
}
