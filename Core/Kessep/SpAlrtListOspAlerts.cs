// Program: SP_ALRT_LIST_OSP_ALERTS, ID: 371924212, model: 746.
// Short name: SWEALRTP
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
/// A program: SP_ALRT_LIST_OSP_ALERTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpAlrtListOspAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ALRT_LIST_OSP_ALERTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAlrtListOspAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAlrtListOspAlerts.
  /// </summary>
  public SpAlrtListOspAlerts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ±ææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÉ
    // ø Date		Developer	Request #      Description                           ø
    // øææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææø
    // ø 11/15/96      d. brokaw                  Init Development
    // ø
    // ø 12/19/96      Michael Ramirez
    // Completion
    // 
    // ø
    // ø 04/15/97      Siraj Konkader             Allow delete if  logged on 
    // user owns alertø
    // ø
    // 
    // or is at a supervisory level.
    // ø
    // ø 12/26/98      swsrkeh                    Phase II changes
    // ø
    // ø 09/22/99      swsrchf        H00074168   bypass alerts alerts not yet 
    // optimized    ø
    // ø 11/19/99      Raj Bhatt      H00080740   Alerts Display
    // ø
    // ø 12/09/99      Raj Bhatt      H00082012   'Role' filter needs to be 
    // considered when ø
    // ø
    // 
    // displaying alerts
    // ø
    // ø 12/14/99      Raj Bhatt      H00082728   Manual alert display is not 
    // suitable      ø
    // ø 01/18/00      swsrchf        H00084400   Make the default date 1 week 
    // prior to the ø
    // ø
    // 
    // CURRENT date
    // ø
    // ø 08/30/00      swsrchf        000192      Default the type code to "ALL"
    // and allow  ø
    // ø
    // 
    // multiple deletes also edit the selection
    // ø
    // ø
    // 
    // code
    // ø
    // ø 09/12/00      swsrchf        H00102745   Re-display the screen after 
    // deletion      ø
    // ø
    // 
    // processing
    // ø
    // ø 09/26/00      swsrchf        000221      Pass Case # and CSE Person # 
    // when an Alertø
    // ø
    // 
    // selected
    // ø
    // ø 11/20/00      swsrchf        000249      Added new filter for EVENT 
    // TYPE           ø
    // ø 12/14/00      swsrchf        000238      Added a new function key PF18 
    // (Detail).   ø
    // ø
    // 
    // Use the NEXTTRAN function to go to the
    // ø
    // ø
    // 
    // appropriate screen depending on the
    // ø
    // ø
    // 
    // Event/Event Detail combination from the
    // ø
    // ø
    // 
    // Infrastructure records associated  with
    // ø
    // ø
    // 
    // the Alert.
    // ø
    // ø
    // 
    // ø
    // ø
    // 
    // GO to    ø
    // ø
    // 
    // Event Id     Event Detail Id     Screen
    // ø
    // ø
    // 
    // --------     ---------------     ------
    // ø
    // ø
    // 
    // 9            600, 601, 602, 603  OPAY
    // ø
    // ø
    // 
    // 9            604, 605, 606       OPAY
    // ø
    // ø
    // 
    // 9            608, 610            PACC
    // ø
    // ø
    // 
    // 10           4, 291, 557         ALOM
    // ø
    // ø
    // 
    // 10           100                 ADDR
    // ø
    // ø
    // 
    // 10           547                 INCS
    // ø
    // ø
    // 
    // 12           ---- ALL ----       ISTM
    // ø
    // ø
    // 
    // 13           ---- ALL ----       ISTM
    // ø
    // ø
    // 
    // 14           ---- ALL ----       ISTM
    // ø
    // ø
    // 
    // 15           ---- ALL ----       ISTM
    // ø
    // ø
    // 
    // 16           ---- ALL ----       ISTM
    // ø
    // ø
    // 
    // 17           ---- ALL ----       ISTM
    // ø
    // ø
    // 
    // 18           688, 689            JAIL
    // ø
    // ø
    // 
    // 24           379                 ADDR
    // ø
    // ø
    // 
    // 47           73                  OPAY
    // ø
    // ø 04/03/01      swsrchf        I00116862   Changed check to test for "** 
    // ALRT **"    ø
    // ø 06/18/02      swsrmca        PR133306   Added code to enable 
    // transfering Transaction
    // ø
    // 
    // id from ALERT to ISTM. Changed
    // setting hidden view from
    // ø
    // 
    // denorm numeric to denorm text.
    // øþæææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææææÊ
    // --------------------------------------------------------------------------------------
    // 08/14/02 SWSRVXM     WR# 020332 When PF18 (Detail) pressed on ALRT by 
    // selecting a
    // 
    // CSENET record, the system must  flow to ICAS instead of ISTM screen with
    // next_tran info.
    // 10/28/02	SWSRPRM
    // WR # 227 - Allowed a MANually generated alert to display
    // case/person/court case information.
    // Deleted disabled code.
    // 04/08/03 SWDPBEL  PR#171116 - corrected fips not found error.
    // 05/19/09  GVandy  CQ11149 Correct display logic to display only fully 
    // optimized AUT alerts.
    // ---------------------------------------------------------------------------------------
    // 06/21/16  JHarden  CQ50346  Make SRV PRVR and Office field enterable on 
    // ALRT screen.
    // 11-06-17  JHarden  CQ56117  Alert Updates - ALRT
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.SvpoPrompt.SelectChar = import.SvpoPrompt.SelectChar;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      var field1 = GetField(export.FilterServiceProvider, "userId");

      field1.Color = "green";
      field1.Highlighting = Highlighting.Underscore;
      field1.Protected = false;
      field1.Focused = true;

      var field2 = GetField(export.FilterOffice, "systemGeneratedId");

      field2.Color = "green";
      field2.Highlighting = Highlighting.Underscore;
      field2.Protected = false;

      return;
    }

    // CQ50346 moved 4 lines below, to clear screen
    export.FilterServiceProvider.UserId = import.FilterServiceProvider.UserId;
    export.FilterOffice.SystemGeneratedId =
      import.FilterOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(import.FilterOfficeServiceProvider,
      export.FilterOfficeServiceProvider);
    export.FilterOfficeServiceProviderAlert.Assign(
      import.FilterOfficeServiceProviderAlert);
    MoveInfrastructure2(import.FilterInfrastructure, export.FilterInfrastructure);
      
    export.FilterFipsTribAddress.Country = import.FilterFipsTribAddress.Country;
    MoveLegalAction(import.FilterLegalAction, export.FilterLegalAction);
    MoveFips(import.FilterFips, export.FilterFips);
    MoveCodeValue(import.FilterSort, export.FilterSort);
    export.FilterEventType.Cdvalue = import.FilterEventType.Cdvalue;
    MoveFips(import.HiddenFilterFips, export.HiddenFilterFips);
    export.HiddenFilterFipsTribAddress.Country =
      import.HiddenFilterFipsTribAddress.Country;
    MoveInfrastructure2(import.HiddenFilterInfrastructure,
      export.HiddenFilterInfrastructure);
    MoveLegalAction(import.HiddenFilterLegalAction,
      export.HiddenFilterLegalAction);
    export.HiddenFilterOffice.SystemGeneratedId =
      import.HiddenFilterOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(import.HiddenFilterOfficeServiceProvider,
      export.HiddenFilterOfficeServiceProvider);
    export.HiddenFilterOfficeServiceProviderAlert.Assign(
      import.HiddenFilterOfficeServiceProviderAlert);
    export.HiddenFilterServiceProvider.UserId =
      import.HiddenFilterServiceProvider.UserId;
    export.HiddenFilterSort.Cdvalue = import.HiddenFilterSort.Cdvalue;
    export.HiddenFilterEventType.Cdvalue = import.HiddenFilterEventType.Cdvalue;
    export.CurrentPage.Count = import.CurrentPage.Count;

    // ***  Work request 000249
    // *** 11/20/00 SWSRCHF
    // *** start
    export.AlertPrompt.SelectChar = import.AlertPrompt.SelectChar;
    export.EventTypePrompt.SelectChar = import.EventTypePrompt.SelectChar;
    export.SortPrompt.SelectChar = import.SortPrompt.SelectChar;

    if (Equal(global.Command, "RETCDVL"))
    {
      export.HiddenFilterEventType.Cdvalue = export.FilterEventType.Cdvalue;
    }

    // *** end
    // *** 11/20/00 SWSRCHF
    // ***  Work request 000249
    // **** Check to see if filters have changed ****
    if (!Equal(export.FilterServiceProvider.UserId,
      export.HiddenFilterServiceProvider.UserId))
    {
      local.Change.Flag = "Y";
    }
    else if (export.FilterOffice.SystemGeneratedId != export
      .HiddenFilterOffice.SystemGeneratedId)
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterOfficeServiceProvider.RoleCode,
      export.HiddenFilterOfficeServiceProvider.RoleCode))
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterOfficeServiceProviderAlert.DistributionDate,
      export.HiddenFilterOfficeServiceProviderAlert.DistributionDate))
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterOfficeServiceProviderAlert.TypeCode,
      export.HiddenFilterOfficeServiceProviderAlert.TypeCode))
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterInfrastructure.CaseNumber,
      export.HiddenFilterInfrastructure.CaseNumber))
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterInfrastructure.CsePersonNumber,
      export.HiddenFilterInfrastructure.CsePersonNumber))
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterLegalAction.CourtCaseNumber,
      export.HiddenFilterLegalAction.CourtCaseNumber))
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterFips.StateAbbreviation,
      export.HiddenFilterFips.StateAbbreviation))
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterFips.CountyAbbreviation,
      export.HiddenFilterFips.CountyAbbreviation))
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterFipsTribAddress.Country,
      export.HiddenFilterFipsTribAddress.Country))
    {
      local.Change.Flag = "Y";

      // ***  Work request 000249
      // *** 11/20/00 SWSRCHF
      // *** start
    }
    else if (!Equal(export.FilterEventType.Cdvalue,
      export.HiddenFilterEventType.Cdvalue))
    {
      local.Change.Flag = "Y";

      // *** end
    }
    else if (!Equal(export.FilterOfficeServiceProviderAlert.Message,
      export.HiddenFilterOfficeServiceProviderAlert.Message))
    {
      local.Change.Flag = "Y";
    }
    else if (!Equal(export.FilterSort.Cdvalue, export.HiddenFilterSort.Cdvalue))
    {
      local.Change.Flag = "Y";
    }
    else
    {
      local.Change.Flag = "N";
    }

    local.CurrentDate.Date = Now().Date;

    for(import.PageKeys.Index = 0; import.PageKeys.Index < import
      .PageKeys.Count; ++import.PageKeys.Index)
    {
      if (!import.PageKeys.CheckSize())
      {
        break;
      }

      export.PageKeys.Index = import.PageKeys.Index;
      export.PageKeys.CheckSize();

      export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.Assign(
        import.PageKeys.Item.PageKeyOfficeServiceProviderAlert);
      MoveInfrastructure2(import.PageKeys.Item.PageKeyInfrastructure,
        export.PageKeys.Update.PageKeyInfrastructure);
    }

    import.PageKeys.CheckIndex();
    export.Scroll.ScrollingMessage = import.Scroll.ScrollingMessage;
    export.CurrentPage.Count = import.CurrentPage.Count;

    // **** Validate all Commands
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        break;
      case "ENTER":
        // if the next tran info is not equal to spaces, this implies the user 
        // requested a next tran action. now validate
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // *** Work request 000221
          // *** 09/26/00 SWSRCHF
          // *** start
          export.Hidden.CaseNumber = "";
          export.Hidden.CsePersonNumber = "";
          export.Hidden.CsePersonNumberAp = "";

          // *** end
          // ****
          // this is where you would set the local next_tran_info attributes to 
          // the import view attributes for the data to be passed to the next
          // transaction
          // ****
          export.Hidden.CaseNumber = export.FilterInfrastructure.CaseNumber ?? ""
            ;
          export.Hidden.CsePersonNumber =
            export.FilterInfrastructure.CsePersonNumber ?? "";

          // *** Work request 000221
          // *** 09/26/00 SWSRCHF
          // *** start
          export.Standard.NextTransaction = import.Standard.NextTransaction;

          if (!import.Import1.IsEmpty)
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

              export.Export1.Update.GxsePersonsWorkSet.FormattedName =
                import.Import1.Item.GisePersonsWorkSet.FormattedName;
              export.Export1.Update.Gxommon.SelectChar =
                import.Import1.Item.Giommon.SelectChar;
              export.Export1.Update.Gxnfrastructure.Assign(
                import.Import1.Item.Ginfrastructure);
              export.Export1.Update.GxegalAction.CourtCaseNumber =
                import.Import1.Item.GiegalAction.CourtCaseNumber;
              MoveFips(import.Import1.Item.GianualFips,
                export.Export1.Update.GxanualFips);
              export.Export1.Update.GxanualFipsTribAddress.Country =
                import.Import1.Item.GianualFipsTribAddress.Country;
              export.Export1.Update.GxfficeServiceProviderAlert.Assign(
                import.Import1.Item.GifficeServiceProviderAlert);
            }

            import.Import1.CheckIndex();
          }

          local.Count.Count = 0;
          local.Pointer.Count = 0;
          local.SelectError.Flag = "N";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.Export1.Item.Gxommon.SelectChar))
            {
              if (AsChar(export.Export1.Item.Gxommon.SelectChar) == 'S')
              {
                if (local.Pointer.Count == 0)
                {
                  local.Pointer.Count = export.Export1.Index + 1;
                }

                ++local.Count.Count;
              }
              else
              {
                // *** This is an ERROR the selection character is NOT an "S"
                if (AsChar(local.SelectError.Flag) == 'N')
                {
                  var field =
                    GetField(export.Export1.Item.Gxommon, "selectChar");

                  field.Color = "red";
                  field.Protected = false;
                  field.Focused = true;

                  local.SelectError.Flag = "Y";
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.Gxommon, "selectChar");

                  field.Color = "red";
                  field.Protected = false;
                }
              }
            }
          }

          export.Export1.CheckIndex();

          if (AsChar(local.SelectError.Flag) == 'Y')
          {
            // *** Invalid selection character found
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
          }

          switch(local.Count.Count)
          {
            case 0:
              break;
            case 1:
              export.Export1.Index = local.Pointer.Count - 1;
              export.Export1.CheckSize();

              export.Hidden.CaseNumber =
                export.Export1.Item.Gxnfrastructure.CaseNumber ?? "";
              export.Hidden.CsePersonNumber =
                export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";
              export.Hidden.CsePersonNumberAp =
                export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";

              break;
            default:
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                if (!export.Export1.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Export1.Item.Gxommon.SelectChar) == 'S')
                {
                  if (export.Export1.Index + 1 == local.Pointer.Count)
                  {
                    var field =
                      GetField(export.Export1.Item.Gxommon, "selectChar");

                    field.Color = "red";
                    field.Protected = false;
                    field.Focused = true;
                  }
                  else
                  {
                    var field =
                      GetField(export.Export1.Item.Gxommon, "selectChar");

                    field.Color = "red";
                    field.Protected = false;
                  }
                }
              }

              export.Export1.CheckIndex();
              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              return;
          }

          // *** end
          UseScCabNextTranPut1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.Standard, "nextTransaction");

            field.Error = true;

            break;
          }

          return;
        }

        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        // *** Work Request 000238
        // *** 12/14/00 SWSRCHF
        // *** added DETAIL to the case of command structure
        break;
      case "DETAIL":
        break;
      case "NEXT":
        break;
      case "PREV":
        break;
      case "DELETE":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;

        // ***  Work request 000249
        // *** 11/20/00 SWSRCHF
        // *** start
        break;
      case "RETCDVL":
        if (AsChar(export.EventTypePrompt.SelectChar) == 'S')
        {
          export.EventTypePrompt.SelectChar = "";

          if (!IsEmpty(import.SelectedFromCdvl.Cdvalue))
          {
            export.FilterEventType.Cdvalue = import.SelectedFromCdvl.Cdvalue;
          }
        }
        else if (AsChar(export.SortPrompt.SelectChar) == 'S')
        {
          export.SortPrompt.SelectChar = "";

          if (!IsEmpty(import.SelectedFromCdvl.Cdvalue))
          {
            MoveCodeValue(import.SelectedFromCdvl, export.FilterSort);
          }
        }

        global.Command = "DISPLAY";

        // *** end
        break;
      case "RETALNA":
        export.AlertPrompt.SelectChar = "";
        global.Command = "DISPLAY";

        break;
      case "SPAD":
        break;
      case "HIST":
        break;
      case "RETSVPO":
        export.SvpoPrompt.SelectChar = "";
        global.Command = "DISPLAY";

        break;
      case "RETHIST":
        break;
      case "RETSPAD":
        break;
      case "XXFMMENU":
        // ****
        // this is where you set your command to do whatever is necessary to do 
        // on a flow from the menu, maybe just escape....
        // ****
        // You should get this information from the Dialog Flow Diagram.  It is 
        // the SEND CMD on the propertis for a Transfer from one  procedure to
        // another.
        // *** the statement would read COMMAND IS display   *****
        global.Command = "DISPLAY";

        break;
      case "XXNEXTXX":
        // ****
        // this is where you set your export value to the export hidden next 
        // tran values if the user is comming into this procedure on a next tran
        // action.
        // ****
        UseScCabNextTranGet();

        // --------------------------------------------------------------------------------
        // Added following code as part of WR# 020332.
        //  Vithal(08/16/2002)
        // ---------------------------------------------------------------------------------
        local.FindAlrt.Count = 0;
        local.FindAlrt.Count =
          Find(
            String(export.Hidden.MiscText2, NextTranInfo.MiscText2_MaxLength),
          TrimEnd("ALRT"));

        if (local.FindAlrt.Count != 0)
        {
          export.FilterServiceProvider.UserId =
            Substring(export.Hidden.MiscText1, 1, 7);
          export.FilterOffice.SystemGeneratedId =
            (int)StringToNumber(Substring(export.Hidden.MiscText1, 50, 19, 4));
          export.FilterOfficeServiceProvider.RoleCode =
            Substring(export.Hidden.MiscText1, 23, 2);
          export.FilterOfficeServiceProvider.EffectiveDate =
            IntToDate((int)StringToNumber(Substring(
              export.Hidden.MiscText1, 50, 32, 8)));
          export.FilterOfficeServiceProviderAlert.DistributionDate =
            IntToDate((int)StringToNumber(Substring(
              export.Hidden.MiscText2, 50, 12, 8)));
          export.FilterEventType.Cdvalue =
            Substring(export.Hidden.MiscText2, 20, 10);
          local.RepositionRecord.SystemGeneratedIdentifier =
            (int)export.Hidden.MiscNum1.GetValueOrDefault();
          export.Hidden.MiscText1 = "";
          export.Hidden.MiscText2 = "";
          export.Hidden.CourtOrderNumber = "";
          global.Command = "DISPLAY";

          break;
        }

        // *** Work request 000238
        // *** 12/14/00 SWSRCHF
        // *** start
        // *** Problem report I00116862
        // *** 04/03/01 swsrchf
        // *** changed the IF statement
        if (Equal(export.Hidden.CourtOrderNumber, "** ALRT **"))
        {
          export.FilterServiceProvider.UserId =
            Substring(export.Hidden.MiscText1, 1, 7);
          export.FilterOffice.SystemGeneratedId =
            (int)StringToNumber(Substring(export.Hidden.MiscText1, 50, 19, 4));
          export.FilterOfficeServiceProvider.RoleCode =
            Substring(export.Hidden.MiscText1, 23, 2);
          export.FilterOfficeServiceProvider.EffectiveDate =
            IntToDate((int)StringToNumber(Substring(
              export.Hidden.MiscText1, 50, 32, 8)));
          export.FilterOfficeServiceProviderAlert.DistributionDate =
            IntToDate((int)StringToNumber(Substring(
              export.Hidden.MiscText2, 50, 43, 8)));
          export.Hidden.MiscText1 = "";
          export.Hidden.MiscText2 = "";

          // *** Problem report I00116862
          // *** 04/03/01 swsrchf
          export.Hidden.CourtOrderNumber = "";
        }

        // *** end
        // *** 12/14/00 SWSRCHF
        // *** Work Request 000238
        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // All this is moved from the CASE OF DISPLAY above.  The problem is 
        // that several of the other commands ultimately change the command to
        // DISPLAY and this logic was not executing for them.
        for(export.PageKeys.Index = 0; export.PageKeys.Index < export
          .PageKeys.Count; ++export.PageKeys.Index)
        {
          if (!export.PageKeys.CheckSize())
          {
            break;
          }

          export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.Assign(
            local.NullOfficeServiceProviderAlert);
          MoveInfrastructure2(local.NullInfrastructure,
            export.PageKeys.Update.PageKeyInfrastructure);
        }

        export.PageKeys.CheckIndex();
        export.CurrentPage.Count = 1;
        export.PageKeys.Count = 0;

        export.PageKeys.Index = 0;
        export.PageKeys.CheckSize();

        if (Equal(export.FilterOfficeServiceProviderAlert.DistributionDate,
          local.Initialized.Date))
        {
          // *** Problem report H00084400
          // *** 01/18/00 SWSRCHF
          // *** start
          // CQ 56117
          export.FilterOfficeServiceProviderAlert.DistributionDate =
            AddDays(local.CurrentDate.Date, -60);

          // *** end
        }
        else
        {
        }

        // --Initialize Paging Group values
        if (Equal(export.FilterSort.Cdvalue, "DD"))
        {
          export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.
            CreatedTimestamp =
              AddMicroseconds(new DateTime(2099, 12, 31, 23, 59, 59), 999999);
          export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.
            DistributionDate = new DateTime(9999, 12, 31);
        }
        else
        {
          export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.
            CreatedTimestamp = new DateTime(1, 1, 1);
          export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.
            DistributionDate = new DateTime(1, 1, 1);
        }

        break;
      case "PREV":
        if (AsChar(local.Change.Flag) == 'Y')
        {
          var field1 = GetField(export.FilterSort, "cdvalue");

          field1.Error = true;

          var field2 =
            GetField(export.FilterOfficeServiceProviderAlert, "message");

          field2.Error = true;

          var field3 = GetField(export.FilterFipsTribAddress, "country");

          field3.Error = true;

          var field4 = GetField(export.FilterFips, "countyAbbreviation");

          field4.Error = true;

          var field5 = GetField(export.FilterFips, "stateAbbreviation");

          field5.Error = true;

          var field6 = GetField(export.FilterLegalAction, "courtCaseNumber");

          field6.Error = true;

          var field7 = GetField(export.FilterInfrastructure, "csePersonNumber");

          field7.Error = true;

          var field8 = GetField(export.FilterInfrastructure, "caseNumber");

          field8.Error = true;

          var field9 = GetField(export.FilterEventType, "cdvalue");

          field9.Error = true;

          var field10 =
            GetField(export.FilterOfficeServiceProviderAlert, "typeCode");

          field10.Error = true;

          var field11 =
            GetField(export.FilterOfficeServiceProviderAlert, "distributionDate");
            

          field11.Error = true;

          var field12 = GetField(export.FilterOffice, "systemGeneratedId");

          field12.Error = true;

          var field13 =
            GetField(export.FilterOfficeServiceProvider, "roleCode");

          field13.Error = true;

          var field14 = GetField(export.FilterServiceProvider, "userId");

          field14.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

          break;
        }

        if (export.CurrentPage.Count <= 1)
        {
          if (import.PageKeys.IsEmpty)
          {
            // -----*****  First time
            ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

            break;
          }

          ExitState = "ACO_NI0000_TOP_OF_LIST";

          break;
        }

        // -----*****  Decrement page number and group view
        --export.CurrentPage.Count;

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        global.Command = "DISPLAY";

        break;
      case "NEXT":
        // **** if filter has changed then issue a message that  ****
        // **** a display has to be done before a Prev or next    ****
        // **** can be performed
        // 
        // ****
        if (AsChar(local.Change.Flag) == 'Y')
        {
          var field1 = GetField(export.FilterSort, "cdvalue");

          field1.Error = true;

          var field2 =
            GetField(export.FilterOfficeServiceProviderAlert, "message");

          field2.Error = true;

          var field3 = GetField(export.FilterFipsTribAddress, "country");

          field3.Error = true;

          var field4 = GetField(export.FilterFips, "countyAbbreviation");

          field4.Error = true;

          var field5 = GetField(export.FilterFips, "stateAbbreviation");

          field5.Error = true;

          var field6 = GetField(export.FilterLegalAction, "courtCaseNumber");

          field6.Error = true;

          var field7 = GetField(export.FilterInfrastructure, "csePersonNumber");

          field7.Error = true;

          var field8 = GetField(export.FilterInfrastructure, "caseNumber");

          field8.Error = true;

          var field9 = GetField(export.FilterEventType, "cdvalue");

          field9.Error = true;

          var field10 =
            GetField(export.FilterOfficeServiceProviderAlert, "typeCode");

          field10.Error = true;

          var field11 =
            GetField(export.FilterOfficeServiceProviderAlert, "distributionDate");
            

          field11.Error = true;

          var field12 = GetField(export.FilterOffice, "systemGeneratedId");

          field12.Error = true;

          var field13 =
            GetField(export.FilterOfficeServiceProvider, "roleCode");

          field13.Error = true;

          var field14 = GetField(export.FilterServiceProvider, "userId");

          field14.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

          break;
        }

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        if (export.PageKeys.Index == -1)
        {
          // -----*****  First time
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        if (export.PageKeys.Index + 1 == export.PageKeys.Count)
        {
          ExitState = "SP0000_END_OF_SCROLL_LIST";

          break;
        }

        if (export.PageKeys.Index + 1 == Export.PageKeysGroup.Capacity)
        {
          ExitState = "SP0000_END_OF_SCROLL_LIST";

          break;
        }

        // -----*****  Increment page number and group view
        ++export.CurrentPage.Count;

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        export.Export1.Count = 0;
        global.Command = "DISPLAY";

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // --- The group view is going to be populated. So don't move group 
      // imports to group exports.
    }
    else if (!import.Import1.IsEmpty)
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

        export.Export1.Update.GxsePersonsWorkSet.FormattedName =
          import.Import1.Item.GisePersonsWorkSet.FormattedName;
        export.Export1.Update.Gxommon.SelectChar =
          import.Import1.Item.Giommon.SelectChar;

        if (AsChar(import.Import1.Item.Giommon.SelectChar) == 'S' && (
          Equal(global.Command, "RETHIST") || Equal
          (global.Command, "RETSPAD")))
        {
          var field = GetField(export.Export1.Item.Gxommon, "selectChar");

          field.Protected = false;
          field.Focused = true;

          export.Export1.Update.Gxommon.SelectChar = "";
        }

        export.Export1.Update.Gxnfrastructure.Assign(
          import.Import1.Item.Ginfrastructure);
        export.Export1.Update.GxegalAction.CourtCaseNumber =
          import.Import1.Item.GiegalAction.CourtCaseNumber;
        MoveFips(import.Import1.Item.GianualFips,
          export.Export1.Update.GxanualFips);
        export.Export1.Update.GxanualFipsTribAddress.Country =
          import.Import1.Item.GianualFipsTribAddress.Country;
        export.Export1.Update.GxfficeServiceProviderAlert.Assign(
          import.Import1.Item.GifficeServiceProviderAlert);
      }

      import.Import1.CheckIndex();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "LIST"))
    {
      local.CsePersonsWorkSet.Number =
        export.FilterInfrastructure.CsePersonNumber ?? Spaces(10);
      local.Case1.Number = export.FilterInfrastructure.CaseNumber ?? Spaces(10);
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // *** Work Request 000238
    // *** 12/14/00 SWSRCHF
    // *** ADDED check for DETAIL
    if (Equal(global.Command, "HIST") || Equal(global.Command, "SPAD") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "DETAIL"))
    {
      if (AsChar(local.Change.Flag) == 'Y')
      {
        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }

      local.SelectError.Flag = "N";

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Gxommon.SelectChar) == 'S')
        {
          local.Pointer.Count = export.Export1.Index + 1;
          ++local.Count.Count;
        }
        else
        {
          // *** Work request 000192
          // *** 08/30/00 SWSRCHF
          // *** start
          if (!IsEmpty(export.Export1.Item.Gxommon.SelectChar))
          {
            // *** This is an ERROR the selection character is NOT an "S"
            var field = GetField(export.Export1.Item.Gxommon, "selectChar");

            field.Error = true;

            local.SelectError.Flag = "Y";
          }
        }
      }

      export.Export1.CheckIndex();

      if (AsChar(local.SelectError.Flag) == 'Y')
      {
        // *** Invalid selection character found
        ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

        return;
      }

      // *** end
      switch(local.Count.Count)
      {
        case 0:
          if (Equal(global.Command, "SPAD"))
          {
            goto Test1;
          }

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        case 1:
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          var field = GetField(export.Export1.Item.Gxommon, "selectChar");

          field.Protected = false;
          field.Focused = true;

          break;
        default:
          // *** Work request 000192
          // *** 08/30/00 SWSRCHF
          // *** start
          if (Equal(global.Command, "DELETE"))
          {
            // *** Multiple selections allowed on a DELETE
            goto Test1;
          }

          // *** end
          // **** Only one selection is allowed for HIST and SPAD ****
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
      }
    }

Test1:

    switch(TrimEnd(global.Command))
    {
      case "DELETE":
        local.LoggedOnUser.UserId = global.UserId;

        // *** Work request 000192
        // *** 08/30/00 SWSRCHF
        // *** start
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Gxommon.SelectChar) != 'S')
          {
            continue;
          }

          if (Equal(export.Export1.Item.GxfficeServiceProviderAlert.
            RecipientUserId, local.LoggedOnUser.UserId) || Equal
            (export.Export1.Item.GxfficeServiceProviderAlert.CreatedBy,
            local.LoggedOnUser.UserId))
          {
            // **** Current User either created the Alert or is the Recipient of
            // the Alert, ****
            // **** thus has the authority to delete.
            // 
            // ****
          }
          else
          {
            UseCoCabIsPersonSupervisor();

            if (AsChar(local.IsSupervisor.Flag) == 'N')
            {
              var field1 = GetField(export.Export1.Item.Gxommon, "selectChar");

              field1.Error = true;

              ExitState = "CO0000_MUST_HAVE_SUPERVSRY_ROLE";

              return;
            }
          }

          UseSpDeleteOspAlerts();

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("ACO_NI0000_DELETE_SUCCESSFUL"))
          {
            // *** Problem report H00102745
            // *** 09/12/00 SWSRCHF
            // *** Moving an asterisk to the Select CHAR is no longer required
            export.Export1.Update.Gxommon.SelectChar = "";
            ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
          }
          else
          {
            var field1 = GetField(export.Export1.Item.Gxommon, "selectChar");

            field1.Error = true;

            return;
          }
        }

        export.Export1.CheckIndex();

        // *** end
        // *** 08/30/00 SWSRCHF
        // *** Work request 000192
        // *** Problem report H00102745
        // *** 09/12/00 SWSRCHF
        // *** start
        for(export.Export1.Index = 0; export.Export1.Index < Export
          .ExportGroup.Capacity; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          // *** Initialize the group view prior to the Re-display
          export.Export1.Update.GxsePersonsWorkSet.FormattedName =
            local.NullCsePersonsWorkSet.FormattedName;
          export.Export1.Update.Gxommon.SelectChar =
            local.NullCommon.SelectChar;
          MoveInfrastructure1(local.NullInfrastructure,
            export.Export1.Update.Gxnfrastructure);
          export.Export1.Update.GxegalAction.CourtCaseNumber =
            local.NullLegalAction.CourtCaseNumber;
          MoveFips(local.NullFips, export.Export1.Update.GxanualFips);
          export.Export1.Update.GxanualFipsTribAddress.Country =
            local.NullFipsTribAddress.Country;
          export.Export1.Update.GxfficeServiceProviderAlert.Assign(
            local.NullOfficeServiceProviderAlert);
        }

        export.Export1.CheckIndex();
        local.Previous.Command = global.Command;

        export.PageKeys.Index = export.CurrentPage.Count - 1;
        export.PageKeys.CheckSize();

        ExitState = "ACO_NN0000_ALL_OK";

        // *** Re-display the screen after deletion processing
        global.Command = "DISPLAY";

        // *** end
        break;
      case "LIST":
        local.PromptCount.Count = 0;

        switch(AsChar(export.SortPrompt.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field1 = GetField(export.SortPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.AlertPrompt.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field1 = GetField(export.AlertPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.EventTypePrompt.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field1 = GetField(export.EventTypePrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.SvpoPrompt.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field1 = GetField(export.SvpoPrompt, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        switch(local.PromptCount.Count)
        {
          case 0:
            var field1 = GetField(export.SortPrompt, "selectChar");

            field1.Error = true;

            var field2 = GetField(export.AlertPrompt, "selectChar");

            field2.Error = true;

            var field3 = GetField(export.EventTypePrompt, "selectChar");

            field3.Error = true;

            var field4 = GetField(export.SvpoPrompt, "selectChar");

            field4.Error = true;

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            if (AsChar(export.SvpoPrompt.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_SVPO";
            }
            else if (AsChar(export.EventTypePrompt.SelectChar) == 'S')
            {
              export.Flow.CodeName = "EVENT TYPE";
              ExitState = "ECO_LNK_TO_CODE_VALUES";
            }
            else if (AsChar(export.AlertPrompt.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_ALNA";
            }
            else if (AsChar(export.SortPrompt.SelectChar) == 'S')
            {
              export.Flow.CodeName = "ALERT SORT";
              ExitState = "ECO_LNK_TO_CODE_VALUES";
            }

            break;
          default:
            if (AsChar(export.SortPrompt.SelectChar) == 'S')
            {
              var field5 = GetField(export.SortPrompt, "selectChar");

              field5.Error = true;
            }

            if (AsChar(export.AlertPrompt.SelectChar) == 'S')
            {
              var field5 = GetField(export.AlertPrompt, "selectChar");

              field5.Error = true;
            }

            if (AsChar(export.EventTypePrompt.SelectChar) == 'S')
            {
              var field5 = GetField(export.EventTypePrompt, "selectChar");

              field5.Error = true;
            }

            if (AsChar(export.SvpoPrompt.SelectChar) == 'S')
            {
              var field5 = GetField(export.SvpoPrompt, "selectChar");

              field5.Error = true;
            }

            if (AsChar(export.SvpoPrompt.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_SVPO";
            }
            else if (AsChar(export.EventTypePrompt.SelectChar) == 'S')
            {
              export.Flow.CodeName = "EVENT TYPE";
              ExitState = "ECO_LNK_TO_CODE_VALUES";
            }
            else if (AsChar(export.AlertPrompt.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_ALNA";
            }
            else if (AsChar(export.SortPrompt.SelectChar) == 'S')
            {
              export.Flow.CodeName = "ALERT SORT";
              ExitState = "ECO_LNK_TO_CODE_VALUES";
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "SPAD":
        if (local.Count.Count == 1)
        {
          export.SelectedInfrastructure.SystemGeneratedIdentifier =
            export.Export1.Item.Gxnfrastructure.SystemGeneratedIdentifier;
          export.SelectedOfficeServiceProviderAlert.SystemGeneratedIdentifier =
            export.Export1.Item.GxfficeServiceProviderAlert.
              SystemGeneratedIdentifier;
        }

        ExitState = "ECO_XFR_TO_SPAD";

        // *** Work Request 000238
        // *** 12/14/00 SWSRCHF
        // *** start
        break;
      case "DETAIL":
        export.Export1.Index = local.Pointer.Count - 1;
        export.Export1.CheckSize();

        if (export.Export1.Item.Gxnfrastructure.EventId == 0)
        {
          var field1 = GetField(export.Export1.Item.Gxommon, "selectChar");

          field1.Error = true;

          ExitState = "SP0000_INVALID_DETAIL_LINK";

          return;
        }

        // ***
        // *** Obtain the Event Detail using the Event Id and Event Detail Name
        // *** from Infrastructure
        // ***
        // ******************************************************************
        // 04/10/02 T.Bobb PR00133291
        // Qualified read by adding reason code to get unique
        // occurance of event_detail.
        // ******************************************************************
        if (ReadEventDetail())
        {
          // ***
          // *** Initialize attributes to SPACES
          // ***
          export.Hidden.CaseNumber = "";
          export.Hidden.CsePersonNumber = "";
          export.Hidden.CsePersonNumberAp = "";
          export.Hidden.CsePersonNumberObligee = "";

          // ***
          // *** Initialize attribute to ZERO
          // ***
          export.Hidden.MiscNum1 = 0;
          export.Hidden.InfrastructureId = 0;

          // ***
          // *** Initialize attribute to "N"
          // ***
          local.SuperNexttran.Flag = "N";

          // ***
          // *** Initialize attribute to "ALRT"
          // ***
          export.Hidden.LastTran = "ALRT";

          switch(export.Export1.Item.Gxnfrastructure.EventId)
          {
            case 9:
              if (entities.ExistingEventDetail.SystemGeneratedIdentifier == 600
                || entities.ExistingEventDetail.SystemGeneratedIdentifier == 601
                || entities.ExistingEventDetail.SystemGeneratedIdentifier == 602
                || entities.ExistingEventDetail.SystemGeneratedIdentifier == 603
                || entities.ExistingEventDetail.SystemGeneratedIdentifier == 604
                || entities.ExistingEventDetail.SystemGeneratedIdentifier == 605
                || entities.ExistingEventDetail.SystemGeneratedIdentifier == 606
                )
              {
                // ***
                // *** Set attribute to the NEXT screen Id
                // ***
                export.Standard.NextTransaction = "OPAY";

                // ***
                // *** Set Next_Tran_Info data
                // ***
                export.Hidden.CsePersonNumber =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";
                export.Hidden.CsePersonNumberAp =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";

                // ***
                // *** Set the Super "NEXT TRAN" flag
                // ***
                local.SuperNexttran.Flag = "Y";
              }
              else if (entities.ExistingEventDetail.SystemGeneratedIdentifier ==
                608 || entities
                .ExistingEventDetail.SystemGeneratedIdentifier == 610)
              {
                // ***
                // *** Set attribute to the NEXT screen Id
                // ***
                export.Standard.NextTransaction = "PACC";

                // ***
                // *** Set Next_Tran_Info data
                // ***
                export.Hidden.CsePersonNumber =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";
                export.Hidden.CsePersonNumberObligee =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";

                // ***
                // *** Set the Super "NEXT TRAN" flag
                // ***
                local.SuperNexttran.Flag = "Y";
              }

              break;
            case 10:
              if (entities.ExistingEventDetail.SystemGeneratedIdentifier == 4
                || entities.ExistingEventDetail.SystemGeneratedIdentifier == 291
                || entities.ExistingEventDetail.SystemGeneratedIdentifier == 557
                )
              {
                // ***
                // *** Set attribute to the NEXT screen Id
                // ***
                export.Standard.NextTransaction = "ALOM";

                // ***
                // *** Set Next_Tran_Info data
                // ***
                export.Hidden.CaseNumber =
                  export.Export1.Item.Gxnfrastructure.CaseNumber ?? "";
                export.Hidden.CsePersonNumber =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";
                export.Hidden.CsePersonNumberAp =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";

                // ***
                // *** Set the Super "NEXT TRAN" flag
                // ***
                local.SuperNexttran.Flag = "Y";
              }
              else if (entities.ExistingEventDetail.SystemGeneratedIdentifier ==
                547)
              {
                // ***
                // *** Set attribute to the NEXT screen Id
                // ***
                export.Standard.NextTransaction = "INCS";

                // ***
                // *** Set Next_Tran_Info data
                // ***
                export.Hidden.CsePersonNumber =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";
                export.Hidden.CsePersonNumberAp =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";

                // ***
                // *** Set the Super "NEXT TRAN" flag
                // ***
                local.SuperNexttran.Flag = "Y";
              }
              else if (entities.ExistingEventDetail.SystemGeneratedIdentifier ==
                100)
              {
                // ***
                // *** Set attribute to the NEXT screen Id
                // ***
                export.Standard.NextTransaction = "ADDR";

                // ***
                // *** Set Next_Tran_Info data
                // ***
                export.Hidden.CaseNumber =
                  export.Export1.Item.Gxnfrastructure.CaseNumber ?? "";
                export.Hidden.InfrastructureId =
                  export.Export1.Item.Gxnfrastructure.SystemGeneratedIdentifier;
                  

                // ***
                // *** Set the Super "NEXT TRAN" flag
                // ***
                local.SuperNexttran.Flag = "Y";
              }

              break;
            case 18:
              if (entities.ExistingEventDetail.SystemGeneratedIdentifier == 688
                || entities.ExistingEventDetail.SystemGeneratedIdentifier == 689
                )
              {
                // ***
                // *** Set attribute to the NEXT screen Id
                // ***
                export.Standard.NextTransaction = "JAIL";

                // ***
                // *** Set Next_Tran_Info data
                // ***
                export.Hidden.CaseNumber =
                  export.Export1.Item.Gxnfrastructure.CaseNumber ?? "";
                export.Hidden.CsePersonNumber =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";
                export.Hidden.CsePersonNumberAp =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";
                export.Hidden.InfrastructureId =
                  export.Export1.Item.Gxnfrastructure.SystemGeneratedIdentifier;
                  

                // ***
                // *** Set the Super "NEXT TRAN" flag
                // ***
                local.SuperNexttran.Flag = "Y";
              }

              break;
            case 24:
              if (entities.ExistingEventDetail.SystemGeneratedIdentifier == 379)
              {
                // ***
                // *** Set attribute to the NEXT screen Id
                // ***
                export.Standard.NextTransaction = "ADDR";

                // ***
                // *** Set Next_Tran_Info data
                // ***
                export.Hidden.CaseNumber =
                  export.Export1.Item.Gxnfrastructure.CaseNumber ?? "";

                // ***
                // *** Set the Super "NEXT TRAN" flag
                // ***
                local.SuperNexttran.Flag = "Y";
              }

              break;
            case 47:
              if (entities.ExistingEventDetail.SystemGeneratedIdentifier == 73)
              {
                // ***
                // *** Set attribute to the NEXT screen Id
                // ***
                export.Standard.NextTransaction = "OPAY";

                // ***
                // *** Set Next_Tran_Info data
                // ***
                export.Hidden.CsePersonNumber =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";
                export.Hidden.CsePersonNumberAp =
                  export.Export1.Item.Gxnfrastructure.CsePersonNumber ?? "";

                // ***
                // *** Set the Super "NEXT TRAN" flag
                // ***
                local.SuperNexttran.Flag = "Y";
              }

              break;
            default:
              if (export.Export1.Item.Gxnfrastructure.EventId == 12 || export
                .Export1.Item.Gxnfrastructure.EventId == 13 || export
                .Export1.Item.Gxnfrastructure.EventId == 14 || export
                .Export1.Item.Gxnfrastructure.EventId == 15 || export
                .Export1.Item.Gxnfrastructure.EventId == 16 || export
                .Export1.Item.Gxnfrastructure.EventId == 17)
              {
                // ***
                // *** Set attribute to the NEXT screen Id
                // ***
                // --------------------------------------------------------------------------------------
                // WR# 020332:  When PF18 (Detail) pressed on ALRT by selecting 
                // a CSENET record, the system must  flow to ICAS instead of
                // ISTM screen with next_tran info. The following code is added
                // as part of this work request.
                // Instead of passing the Interstate_case 'trans_serial_number' 
                // (denorm_text_12 or denorm_numeric_12 on Infrastructure record
                // )  to the 'Next_tran_put'  CAB,  pass the Infrastructure
                // system_generated_ID into Next_tran_put and in  ICAS PSTEP
                // extract the 'Interstate_case'  identifiers ( '
                // trans_serial_number' and 'transaction_date') from
                // infrastructure record  based on the Infrastructure system_
                // generated_ID.
                // The reason for passing Infrastructure system_generated_Id to
                // 'Next_tran_put' CAB is, the next_transaction entity has no
                // attribute of type 'Date' to set the 'denorm_date' (ie,
                // transaction_date) of infrastructure record.
                //                                                             
                // Vithal(08/14/02)
                // ---------------------------------------------------------------------------------------
                export.Standard.NextTransaction = "ICAS";

                // ***
                // *** Set Next_Tran_Info data
                // ***
                // ********************************************************************
                // Added code to enable transfering Transaction id from ALERT to
                // ISTM. There was a change made to incomming batch to set the
                // tran id to denorm text 12 instead of denorm numeric 12,
                // therefore the older transactions will have the numeric
                // populated.  Check text first then numeric. PR 133306 MCA
                // ********************************************************************
                if (!IsEmpty(export.Export1.Item.Gxnfrastructure.DenormText12) &&
                  Verify
                  (export.Export1.Item.Gxnfrastructure.DenormText12,
                  " 1234567890") == 0)
                {
                  export.Hidden.InfrastructureId =
                    export.Export1.Item.Gxnfrastructure.
                      SystemGeneratedIdentifier;
                }
                else if (export.Export1.Item.Gxnfrastructure.DenormNumeric12.
                  GetValueOrDefault() != 0)
                {
                  export.Hidden.InfrastructureId =
                    export.Export1.Item.Gxnfrastructure.
                      SystemGeneratedIdentifier;
                }
                else
                {
                  var field1 =
                    GetField(export.Export1.Item.Gxommon, "selectChar");

                  field1.Error = true;

                  export.Standard.NextTransaction = "";
                  ExitState = "SP0000_INVALID_DETAIL_LINK";

                  return;
                }

                // ***
                // *** Set the Super "NEXT TRAN" flag
                // ***
                local.SuperNexttran.Flag = "Y";
                export.Hidden.MiscText2 = TrimEnd("ALRT") + TrimEnd
                  (NumberToString(
                    DateToInt(export.Export1.Item.GxfficeServiceProviderAlert.
                    DistributionDate), 15)) + TrimEnd
                  (export.FilterEventType.Cdvalue);

                // -----------------------------------------------------------------------------------
                // PR# 160737: Save the 'office_service_provider_alert' 
                // sys_gen_id in 'misc_num_1. On return from 'ICAS', in 'case '
                // XXNEXTXX', read the office_service_provider_alert based on
                // the 'misc_num_1' and get the corresponding '
                // created_timestamp' to display the correct page of 'ALRT'
                // screen from where user went to 'ICAS'.
                // See the READ in case of 'XXNEXTXX' which will use the '
                // misc_num_1'.
                //                                                           
                // Vithal(10/15/2002)
                // -----------------------------------------------------------------------------------
                export.Hidden.MiscNum1 =
                  export.Export1.Item.GxfficeServiceProviderAlert.
                    SystemGeneratedIdentifier;
              }

              break;
          }

          if (AsChar(local.SuperNexttran.Flag) == 'Y')
          {
            export.Hidden.MiscText1 =
              TrimEnd(export.FilterServiceProvider.UserId) + NumberToString
              (export.FilterOffice.SystemGeneratedId, 15) + export
              .FilterOfficeServiceProvider.RoleCode + NumberToString
              (DateToInt(export.FilterOfficeServiceProvider.EffectiveDate), 15);
              

            // ---------------------------------------------------------------------------------------
            // The 'misc_text_2' is populated above for events 12,13,14,15,16 
            // and 17. Bypass this.
            // --------------------------------------------------------------------------------------
            if (IsEmpty(export.Hidden.MiscText2))
            {
              // *** Problem report I00116862
              // *** 04/03/01 swsrchf
              // *** start
              export.Hidden.MiscText2 =
                NumberToString(DateToInt(
                  export.Export1.Item.GxfficeServiceProviderAlert.
                  DistributionDate), 50);
              export.Hidden.CourtOrderNumber = "** ALRT **";

              // *** end
            }

            UseScCabNextTranPut2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ***
              // *** "NEXT TRAN" to ADDR, ALOM, INCL, JAIL, ISTM, OPAY or PACC
              // ***
              return;
            }
          }
          else
          {
            export.Standard.NextTransaction = "";
            ExitState = "SP0000_INVALID_DETAIL_LINK";
          }
        }
        else
        {
          var field1 = GetField(export.Export1.Item.Gxommon, "selectChar");

          field1.Error = true;

          ExitState = "SP0000_EVENT_DETAIL_NF";

          return;
        }

        var field = GetField(export.Export1.Item.Gxommon, "selectChar");

        field.Error = true;

        return;

        // *** end
        // *** 12/14/00 SWSRCHF
        // *** Work Request 000238
        break;
      case "HIST":
        if (Equal(export.Export1.Item.GxfficeServiceProviderAlert.TypeCode,
          "MAN") || export
          .Export1.Item.Gxnfrastructure.SystemGeneratedIdentifier == 0)
        {
          var field1 = GetField(export.Export1.Item.Gxommon, "selectChar");

          field1.Error = true;

          ExitState = "SP0000_INVALID_FLOW_MANUAL_ALERT";
        }
        else
        {
          export.SelectedInfrastructure.SystemGeneratedIdentifier =
            export.Export1.Item.Gxnfrastructure.SystemGeneratedIdentifier;
          ExitState = "ECO_LNK_TO_HIST";
        }

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // ------------------------------------------------------------------------------------
      // -- Edits on filter/sort criteria.  The edits are done from bottom right
      // to top left
      // -- so that all errors are highlighted and the message pertains the 
      // topmost left
      // -- error. DO NOT ESCAPE OUT OF THESE EDITS!
      // ------------------------------------------------------------------------------------
      // ------------------------------------------------------------------------------------
      // --Sort Code edits/default
      // ------------------------------------------------------------------------------------
      local.DefaultSort.Cdvalue = "DA";

      if (IsEmpty(export.FilterSort.Cdvalue))
      {
        export.FilterSort.Cdvalue = local.DefaultSort.Cdvalue;
      }

      local.Code.CodeName = "ALERT SORT";
      UseCabValidateCodeValue2();

      if (AsChar(local.ValidCode.Flag) == 'Y')
      {
        MoveCodeValue(local.CodeValue, export.FilterSort);
      }
      else
      {
        var field = GetField(export.FilterSort, "cdvalue");

        field.Error = true;

        export.FilterSort.Description = Spaces(CodeValue.Description_MaxLength);
        ExitState = "ACO_NE0000_INVALID_CODE";
      }

      if (!Equal(export.FilterSort.Cdvalue, "DA"))
      {
        if (IsEmpty(export.FilterServiceProvider.UserId) && export
          .FilterOffice.SystemGeneratedId == 0)
        {
          var field = GetField(export.FilterSort, "cdvalue");

          field.Error = true;

          ExitState = "SP0000_SORT_ONLY_WITH_SP_OR_OFF";
        }
      }

      // ------------------------------------------------------------------------------------
      // --Alert Name edits
      // ------------------------------------------------------------------------------------
      if (!IsEmpty(export.FilterOfficeServiceProviderAlert.Message))
      {
        if (IsEmpty(export.FilterServiceProvider.UserId) && export
          .FilterOffice.SystemGeneratedId == 0)
        {
          var field =
            GetField(export.FilterOfficeServiceProviderAlert, "message");

          field.Error = true;

          ExitState = "SP0000_ALERT_FILTER_W_SP_OR_OFF";
        }
      }

      // ------------------------------------------------------------------------------------
      // --Legal Action Court Order Number, State, County, & Country edits
      // ------------------------------------------------------------------------------------
      if (!IsEmpty(export.FilterLegalAction.CourtCaseNumber))
      {
        // Either (State and County) or Country must be entered
        if (IsEmpty(export.FilterFipsTribAddress.Country) && IsEmpty
          (export.FilterFips.StateAbbreviation) && IsEmpty
          (export.FilterFips.CountyAbbreviation))
        {
          ExitState = "SP0000_ENTER_ST_AND_CO_OR_CY";

          var field1 = GetField(export.FilterFips, "countyAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.FilterFips, "stateAbbreviation");

          field2.Error = true;

          var field3 = GetField(export.FilterFipsTribAddress, "country");

          field3.Error = true;
        }

        if (!IsEmpty(export.FilterFips.StateAbbreviation) && IsEmpty
          (export.FilterFips.CountyAbbreviation))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.FilterFips, "countyAbbreviation");

          field.Error = true;
        }

        if (IsEmpty(export.FilterFips.StateAbbreviation) && !
          IsEmpty(export.FilterFips.CountyAbbreviation))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.FilterFips, "stateAbbreviation");

          field.Error = true;
        }

        if ((!IsEmpty(export.FilterFips.CountyAbbreviation) || !
          IsEmpty(export.FilterFips.StateAbbreviation)) && !
          IsEmpty(export.FilterFipsTribAddress.Country))
        {
          ExitState = "SP0000_ENTER_ST_AND_CO_OR_CY";

          var field1 = GetField(export.FilterFips, "countyAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.FilterFips, "stateAbbreviation");

          field2.Error = true;

          var field3 = GetField(export.FilterFipsTribAddress, "country");

          field3.Error = true;
        }

        if (!IsEmpty(export.FilterFips.CountyAbbreviation))
        {
          local.Code.CodeName = "COUNTY CODE";
          local.CodeValue.Cdvalue = export.FilterFips.CountyAbbreviation ?? Spaces
            (10);
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            var field = GetField(export.FilterFips, "countyAbbreviation");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";
          }
        }

        if (!IsEmpty(export.FilterFips.StateAbbreviation))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = export.FilterFips.StateAbbreviation;
          UseCabValidateCodeValue1();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            var field = GetField(export.FilterFips, "stateAbbreviation");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";
          }
        }
      }
      else
      {
        if (!IsEmpty(export.FilterFipsTribAddress.Country))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.FilterLegalAction, "courtCaseNumber");

          field.Error = true;
        }

        if (!IsEmpty(export.FilterFips.CountyAbbreviation))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field1 = GetField(export.FilterLegalAction, "courtCaseNumber");

          field1.Error = true;

          var field2 = GetField(export.FilterFips, "stateAbbreviation");

          field2.Error = true;
        }

        if (!IsEmpty(export.FilterFips.StateAbbreviation))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.FilterLegalAction, "courtCaseNumber");

          field.Error = true;

          if (IsEmpty(export.FilterFips.CountyAbbreviation))
          {
            var field1 = GetField(export.FilterFips, "countyAbbreviation");

            field1.Error = true;
          }
        }
      }

      // ------------------------------------------------------------------------------------
      // --Person Number edits
      // ------------------------------------------------------------------------------------
      if (!IsEmpty(export.FilterInfrastructure.CsePersonNumber))
      {
        // MJR***** PAD PERSON NUMBER WITH ZEROES ON THE LEFT *****
        local.TextWorkArea.Text10 =
          export.FilterInfrastructure.CsePersonNumber ?? Spaces(10);
        UseEabPadLeftWithZeros();
        export.FilterInfrastructure.CsePersonNumber = local.TextWorkArea.Text10;

        if (!ReadCsePerson())
        {
          var field = GetField(export.FilterInfrastructure, "csePersonNumber");

          field.Error = true;

          ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";
        }
      }

      // ------------------------------------------------------------------------------------
      // --Case Number edits
      // ------------------------------------------------------------------------------------
      if (!IsEmpty(import.FilterInfrastructure.CaseNumber))
      {
        // MJR***** PAD CASE NUMBER WITH ZEROES ON THE LEFT *****
        local.TextWorkArea.Text10 = import.FilterInfrastructure.CaseNumber ?? Spaces
          (10);
        UseEabPadLeftWithZeros();
        export.FilterInfrastructure.CaseNumber = local.TextWorkArea.Text10;

        if (!ReadCase())
        {
          var field = GetField(export.FilterInfrastructure, "caseNumber");

          field.Error = true;

          ExitState = "CASE_NF";
        }
      }

      // ------------------------------------------------------------------------------------
      // --Event Type edits
      // ------------------------------------------------------------------------------------
      if (!IsEmpty(export.FilterEventType.Cdvalue))
      {
        if (Equal(export.FilterEventType.Cdvalue,
          export.HiddenFilterEventType.Cdvalue))
        {
          goto Test2;
        }

        local.WorkCode.CodeName = "EVENT TYPE";

        if (!ReadCodeCodeValue())
        {
          var field = GetField(export.FilterEventType, "cdvalue");

          field.Error = true;

          ExitState = "CODE_VALUE_NF";
        }
      }

Test2:

      // ------------------------------------------------------------------------------------
      // --Alert Type edits/default
      // ------------------------------------------------------------------------------------
      local.Default1.TypeCode = "ALL";

      if (IsEmpty(export.FilterOfficeServiceProviderAlert.TypeCode))
      {
        // *** Work request 000192
        // *** 08/30/00 SWSRCHF
        // *** default the OSP Alert type code to "ALL"
        export.FilterOfficeServiceProviderAlert.TypeCode =
          local.Default1.TypeCode;
      }

      // *** Work request 000192
      // *** 08/30/00 SWSRCHF
      // *** add check for "ALL" to the IF statement
      if (Equal(export.FilterOfficeServiceProviderAlert.TypeCode, "MAN") || Equal
        (export.FilterOfficeServiceProviderAlert.TypeCode, "AUT") || Equal
        (export.FilterOfficeServiceProviderAlert.TypeCode, "ALL"))
      {
      }
      else
      {
        var field =
          GetField(export.FilterOfficeServiceProviderAlert, "typeCode");

        field.Error = true;

        ExitState = "SP0000_INVALID_TYPE_CODE";
      }

      // ------------------------------------------------------------------------------------
      // --Alert Distribution Date edits/default
      // ------------------------------------------------------------------------------------
      local.Default1.DistributionDate = AddDays(local.CurrentDate.Date, -60);

      if (Equal(export.FilterOfficeServiceProviderAlert.DistributionDate,
        local.Initialized.Date))
      {
        // *** Problem report H00084400
        // *** 01/18/00 SWSRCHF
        // *** start
        // CQ56117
        export.FilterOfficeServiceProviderAlert.DistributionDate =
          local.Default1.DistributionDate;
        export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.
          DistributionDate = local.Default1.DistributionDate;

        // *** end
        export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.
          CreatedTimestamp = new DateTime(1, 1, 1);
        export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.
          SystemGeneratedIdentifier = 0;
      }

      // --This flag is used later to determine whether a read should be done 
      // against the
      //   entere service provider, office, and role code.
      local.OffSpRoleValid.Flag = "Y";

      // ------------------------------------------------------------------------------------
      // --Office Service Provider Role Code edits
      // ------------------------------------------------------------------------------------
      if (!Equal(export.FilterOfficeServiceProvider.RoleCode,
        export.HiddenFilterOfficeServiceProvider.RoleCode))
      {
        export.FilterOfficeServiceProvider.EffectiveDate =
          local.NullDateWorkArea.Date;
      }

      if (!IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
      {
        local.Code.CodeName = "OFFICE SERVICE PROVIDER ROLE";
        local.CodeValue.Cdvalue = import.FilterOfficeServiceProvider.RoleCode;
        UseCabValidateCodeValue1();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          local.OffSpRoleValid.Flag = "N";

          var field = GetField(export.FilterOfficeServiceProvider, "roleCode");

          field.Error = true;

          ExitState = "INVALID_ROLE_CODE";
        }
      }

      // ------------------------------------------------------------------------------------
      // --Office edits
      // ------------------------------------------------------------------------------------
      if (export.FilterOffice.SystemGeneratedId != export
        .HiddenFilterOffice.SystemGeneratedId)
      {
        export.FilterOfficeServiceProvider.EffectiveDate =
          local.NullDateWorkArea.Date;
      }

      if (import.FilterOffice.SystemGeneratedId > 0)
      {
        if (!ReadOffice())
        {
          local.OffSpRoleValid.Flag = "N";

          var field = GetField(export.FilterOffice, "systemGeneratedId");

          field.Error = true;

          ExitState = "FN0000_OFFICE_NF";
        }
      }

      // ------------------------------------------------------------------------------------
      // --Service Provider edits/defaults
      // ------------------------------------------------------------------------------------
      if (!Equal(export.FilterServiceProvider.UserId,
        export.HiddenFilterServiceProvider.UserId))
      {
        export.FilterOfficeServiceProvider.EffectiveDate =
          local.NullDateWorkArea.Date;
      }

      if (!IsEmpty(import.FilterServiceProvider.UserId))
      {
        if (!ReadServiceProvider())
        {
          local.OffSpRoleValid.Flag = "N";

          var field = GetField(export.FilterServiceProvider, "userId");

          field.Error = true;

          ExitState = "SERVICE_PROVIDER_NF";
        }
      }

      if (!IsEmpty(import.FilterServiceProvider.UserId) && import
        .FilterOffice.SystemGeneratedId != 0 && !
        IsEmpty(import.FilterOfficeServiceProvider.RoleCode))
      {
        if (AsChar(local.OffSpRoleValid.Flag) == 'N')
        {
          goto Test3;
        }

        if (!ReadOfficeServiceProvider())
        {
          var field1 = GetField(export.FilterOfficeServiceProvider, "roleCode");

          field1.Error = true;

          var field2 = GetField(export.FilterOffice, "systemGeneratedId");

          field2.Error = true;

          var field3 = GetField(export.FilterServiceProvider, "userId");

          field3.Error = true;

          ExitState = "OFFICE_SERVICE_PROVIDER_NF";
          ExitState = "SERVICE_PROVIDER_NF_IN_OFFICE";
        }
      }

Test3:

      if (IsEmpty(export.FilterServiceProvider.UserId) && export
        .FilterOffice.SystemGeneratedId == 0 && IsEmpty
        (export.FilterOfficeServiceProvider.RoleCode) && Equal
        (export.FilterOfficeServiceProviderAlert.DistributionDate,
        local.Default1.DistributionDate) && Equal
        (export.FilterOfficeServiceProviderAlert.TypeCode,
        local.Default1.TypeCode) && IsEmpty(export.FilterEventType.Cdvalue) && IsEmpty
        (export.FilterInfrastructure.CaseNumber) && IsEmpty
        (export.FilterInfrastructure.CsePersonNumber) && IsEmpty
        (export.FilterLegalAction.CourtCaseNumber) && IsEmpty
        (export.FilterFips.StateAbbreviation) && IsEmpty
        (export.FilterFips.CountyAbbreviation) && IsEmpty
        (export.FilterFipsTribAddress.Country) && IsEmpty
        (export.FilterOfficeServiceProviderAlert.Message) && Equal
        (export.FilterSort.Cdvalue, local.DefaultSort.Cdvalue))
      {
        // CQ50346
        ReadOfficeServiceProviderOffice();

        if (entities.ExistingOfficeServiceProvider.Populated)
        {
          var field = GetField(export.SvpoPrompt, "selectChar");

          field.Protected = false;
          field.Focused = true;

          export.FilterServiceProvider.UserId = global.UserId;
          export.FilterOfficeServiceProvider.RoleCode =
            entities.ExistingOfficeServiceProvider.RoleCode;
          export.FilterOfficeServiceProvider.EffectiveDate =
            entities.ExistingOfficeServiceProvider.EffectiveDate;
          export.FilterOffice.SystemGeneratedId =
            entities.ExistingOffice.SystemGeneratedId;
        }
        else
        {
          var field = GetField(export.FilterServiceProvider, "userId");

          field.Error = true;

          ExitState = "OFFICE_SERVICE_PROVIDER_NF";
        }
      }

      // --End Filter Edits
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // *** Problem report H00102745
        // *** 09/12/00 SWSRCHF
        // *** start
        if (Equal(local.Previous.Command, "DELETE"))
        {
          goto Test4;
        }

        // *** end
        return;
      }

Test4:

      if (!IsEmpty(export.FilterInfrastructure.CaseNumber))
      {
        UseSpAlrtSearchByCase();
      }
      else if (!IsEmpty(export.FilterInfrastructure.CsePersonNumber))
      {
        UseSpAlrtSearchByPerson();
      }
      else if (!IsEmpty(export.FilterServiceProvider.UserId))
      {
        UseSpAlrtSearchByServicePrvder();
      }
      else if (!IsEmpty(export.FilterLegalAction.CourtCaseNumber))
      {
        UseSpAlrtSearchByCourtOrder();
      }
      else if (export.FilterOffice.SystemGeneratedId != 0)
      {
        UseSpAlrtSearchByOffice();
      }
      else if (!IsEmpty(export.FilterEventType.Cdvalue))
      {
        UseSpAlrtSearchByEventType();
      }

      if (export.Export1.Count == Export.ExportGroup.Capacity)
      {
        if (export.CurrentPage.Count >= Export.PageKeysGroup.Capacity)
        {
          ExitState = "SP0000_LIST_IS_FULL";
        }
        else
        {
          export.Export1.Index = Export.ExportGroup.Capacity - 1;
          export.Export1.CheckSize();

          export.PageKeys.Index = export.CurrentPage.Count;
          export.PageKeys.CheckSize();

          MoveInfrastructure2(export.Export1.Item.Gxnfrastructure,
            export.PageKeys.Update.PageKeyInfrastructure);
          export.PageKeys.Update.PageKeyOfficeServiceProviderAlert.Assign(
            export.Export1.Item.GxfficeServiceProviderAlert);
        }
      }

      // ************************************
      // Scrolling indicator processing.
      // ************************************
      if (export.CurrentPage.Count == 1 && export.Export1.IsEmpty)
      {
        export.CurrentPage.Count = 0;

        // The only time this can happen is when no data is found for search 
        // criteria entered.
        export.Scroll.ScrollingMessage = "More";
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else if (export.CurrentPage.Count == 1 && !export.Export1.IsFull)
      {
        export.Scroll.ScrollingMessage = "More";
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else if (export.CurrentPage.Count == 1 && export.Export1.IsFull)
      {
        if (export.PageKeys.Count > 1)
        {
          export.Scroll.ScrollingMessage = "More +";
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else if (export.CurrentPage.Count > 1 && !export.Export1.IsFull)
      {
        export.Scroll.ScrollingMessage = "More -";
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else if (export.CurrentPage.Count > 1 && export.Export1.IsFull)
      {
        if (export.PageKeys.Count > export.CurrentPage.Count)
        {
          export.Scroll.ScrollingMessage = "More -+";
        }
        else
        {
          export.Scroll.ScrollingMessage = "More -";
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }

      // *** Problem report H00102745
      // *** 09/12/00 SWSRCHF
      // *** start
      if (Equal(local.Previous.Command, "DELETE"))
      {
        ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
      }

      // *** end
      MoveFips(export.FilterFips, export.HiddenFilterFips);
      export.HiddenFilterFipsTribAddress.Country =
        export.FilterFipsTribAddress.Country;
      MoveInfrastructure2(export.FilterInfrastructure,
        export.HiddenFilterInfrastructure);
      MoveLegalAction(export.FilterLegalAction, export.HiddenFilterLegalAction);
      export.HiddenFilterOffice.SystemGeneratedId =
        export.FilterOffice.SystemGeneratedId;
      MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
        export.HiddenFilterOfficeServiceProvider);
      export.HiddenFilterOfficeServiceProviderAlert.Assign(
        export.FilterOfficeServiceProviderAlert);
      export.HiddenFilterServiceProvider.UserId =
        export.FilterServiceProvider.UserId;
      export.HiddenFilterSort.Cdvalue = export.FilterSort.Cdvalue;

      // ***  Work request 000249
      // *** 11/20/00 SWSRCHF
      export.HiddenFilterEventType.Cdvalue = export.FilterEventType.Cdvalue;
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveExport2(SpAlrtSearchByServicePrvder.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.GxegalAction.CourtCaseNumber = source.GxegalAction.CourtCaseNumber;
    target.GxanualFipsTribAddress.Country =
      source.GxanualFipsTribAddress.Country;
    MoveFips(source.GxanualFips, target.GxanualFips);
    target.Gxnfrastructure.Assign(source.Gxnfrastructure);
    target.GxfficeServiceProviderAlert.
      Assign(source.GxfficeServiceProviderAlert);
    target.Gxommon.SelectChar = source.Gxommon.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxsePersonsWorkSet.FormattedName;
  }

  private static void MoveExport3(SpAlrtSearchByPerson.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.GxegalAction.CourtCaseNumber = source.GxegalAction.CourtCaseNumber;
    target.GxanualFipsTribAddress.Country =
      source.GxanualFipsTribAddress.Country;
    MoveFips(source.GxanualFips, target.GxanualFips);
    target.Gxnfrastructure.Assign(source.Gxnfrastructure);
    target.GxfficeServiceProviderAlert.
      Assign(source.GxfficeServiceProviderAlert);
    target.Gxommon.SelectChar = source.Gxommon.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxsePersonsWorkSet.FormattedName;
  }

  private static void MoveExport4(SpAlrtSearchByCourtOrder.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.GxegalAction.CourtCaseNumber = source.GxegalAction.CourtCaseNumber;
    target.GxanualFipsTribAddress.Country =
      source.GxanualFipsTribAddress.Country;
    MoveFips(source.GxanualFips, target.GxanualFips);
    target.Gxnfrastructure.Assign(source.Gxnfrastructure);
    target.GxfficeServiceProviderAlert.
      Assign(source.GxfficeServiceProviderAlert);
    target.Gxommon.SelectChar = source.Gxommon.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxsePersonsWorkSet.FormattedName;
  }

  private static void MoveExport5(SpAlrtSearchByOffice.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.GxegalAction.CourtCaseNumber = source.GxegalAction.CourtCaseNumber;
    target.GxanualFipsTribAddress.Country =
      source.GxanualFipsTribAddress.Country;
    MoveFips(source.GxanualFips, target.GxanualFips);
    target.Gxnfrastructure.Assign(source.Gxnfrastructure);
    target.GxfficeServiceProviderAlert.
      Assign(source.GxfficeServiceProviderAlert);
    target.Gxommon.SelectChar = source.Gxommon.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxsePersonsWorkSet.FormattedName;
  }

  private static void MoveExport6(SpAlrtSearchByEventType.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.GxegalAction.CourtCaseNumber = source.GxegalAction.CourtCaseNumber;
    target.GxanualFipsTribAddress.Country =
      source.GxanualFipsTribAddress.Country;
    MoveFips(source.GxanualFips, target.GxanualFips);
    target.Gxnfrastructure.Assign(source.Gxnfrastructure);
    target.GxfficeServiceProviderAlert.
      Assign(source.GxfficeServiceProviderAlert);
    target.Gxommon.SelectChar = source.Gxommon.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxsePersonsWorkSet.FormattedName;
  }

  private static void MoveExport7(SpAlrtSearchByCase.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.GxegalAction.CourtCaseNumber = source.GxegalAction.CourtCaseNumber;
    target.GxanualFipsTribAddress.Country =
      source.GxanualFipsTribAddress.Country;
    MoveFips(source.GxanualFips, target.GxanualFips);
    target.Gxnfrastructure.Assign(source.Gxnfrastructure);
    target.GxfficeServiceProviderAlert.
      Assign(source.GxfficeServiceProviderAlert);
    target.Gxommon.SelectChar = source.Gxommon.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxsePersonsWorkSet.FormattedName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EventId = source.EventId;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = export.FilterSort.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseCoCabIsPersonSupervisor()
  {
    var useImport = new CoCabIsPersonSupervisor.Import();
    var useExport = new CoCabIsPersonSupervisor.Export();

    useImport.ServiceProvider.UserId = local.LoggedOnUser.UserId;
    useImport.ProcessDtOrCurrentDt.Date = local.CurrentDate.Date;

    Call(CoCabIsPersonSupervisor.Execute, useImport, useExport);

    local.IsSupervisor.Flag = useExport.IsSupervisor.Flag;
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

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

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

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.Case1.Number = local.Case1.Number;
    MoveLegalAction(export.FilterLegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpAlrtSearchByCase()
  {
    var useImport = new SpAlrtSearchByCase.Import();
    var useExport = new SpAlrtSearchByCase.Export();

    useImport.FilterOfficeServiceProviderAlert.Assign(
      export.FilterOfficeServiceProviderAlert);
    MoveFips(export.FilterFips, useImport.FilterFips);
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    MoveInfrastructure2(export.FilterInfrastructure,
      useImport.FilterInfrastructure);
    useImport.FilterCodeValue.Cdvalue = export.FilterEventType.Cdvalue;
    useImport.SortDesc.Cdvalue = export.FilterSort.Cdvalue;
    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.StartPageKeyOfficeServiceProviderAlert.Assign(
      export.PageKeys.Item.PageKeyOfficeServiceProviderAlert);
    MoveInfrastructure2(export.PageKeys.Item.PageKeyInfrastructure,
      useImport.StartPageKeyInfrastructure);
    useImport.RepositionRecord.SystemGeneratedIdentifier =
      local.RepositionRecord.SystemGeneratedIdentifier;

    Call(SpAlrtSearchByCase.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport7);
  }

  private void UseSpAlrtSearchByCourtOrder()
  {
    var useImport = new SpAlrtSearchByCourtOrder.Import();
    var useExport = new SpAlrtSearchByCourtOrder.Export();

    useImport.FilterOfficeServiceProviderAlert.Assign(
      export.FilterOfficeServiceProviderAlert);
    MoveFips(export.FilterFips, useImport.FilterFips);
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    MoveInfrastructure2(export.FilterInfrastructure,
      useImport.FilterInfrastructure);
    useImport.FilterCodeValue.Cdvalue = export.FilterEventType.Cdvalue;
    useImport.SortDesc.Cdvalue = export.FilterSort.Cdvalue;
    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.StartPageKeyOfficeServiceProviderAlert.Assign(
      export.PageKeys.Item.PageKeyOfficeServiceProviderAlert);
    MoveInfrastructure2(export.PageKeys.Item.PageKeyInfrastructure,
      useImport.StartPageKeyInfrastructure);
    useImport.RepositionRecord.SystemGeneratedIdentifier =
      local.RepositionRecord.SystemGeneratedIdentifier;

    Call(SpAlrtSearchByCourtOrder.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport4);
  }

  private void UseSpAlrtSearchByEventType()
  {
    var useImport = new SpAlrtSearchByEventType.Import();
    var useExport = new SpAlrtSearchByEventType.Export();

    useImport.FilterCodeValue.Cdvalue = export.FilterEventType.Cdvalue;
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    MoveInfrastructure2(export.FilterInfrastructure,
      useImport.FilterInfrastructure);
    useImport.FilterOfficeServiceProviderAlert.Assign(
      export.FilterOfficeServiceProviderAlert);
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveFips(export.FilterFips, useImport.FilterFips);
    useImport.RepositionRecord.SystemGeneratedIdentifier =
      local.RepositionRecord.SystemGeneratedIdentifier;
    useImport.StartPageKeyOfficeServiceProviderAlert.Assign(
      export.PageKeys.Item.PageKeyOfficeServiceProviderAlert);
    MoveInfrastructure2(export.PageKeys.Item.PageKeyInfrastructure,
      useImport.StartPageKeyInfrastructure);
    useImport.SortDesc.Cdvalue = export.FilterSort.Cdvalue;

    Call(SpAlrtSearchByEventType.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport6);
  }

  private void UseSpAlrtSearchByOffice()
  {
    var useImport = new SpAlrtSearchByOffice.Import();
    var useExport = new SpAlrtSearchByOffice.Export();

    useImport.FilterOfficeServiceProviderAlert.Assign(
      export.FilterOfficeServiceProviderAlert);
    MoveFips(export.FilterFips, useImport.FilterFips);
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    MoveInfrastructure2(export.FilterInfrastructure,
      useImport.FilterInfrastructure);
    useImport.FilterCodeValue.Cdvalue = export.FilterEventType.Cdvalue;
    useImport.SortDesc.Cdvalue = export.FilterSort.Cdvalue;
    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.StartPageKeyOfficeServiceProviderAlert.Assign(
      export.PageKeys.Item.PageKeyOfficeServiceProviderAlert);
    MoveInfrastructure2(export.PageKeys.Item.PageKeyInfrastructure,
      useImport.StartPageKeyInfrastructure);
    useImport.RepositionRecord.SystemGeneratedIdentifier =
      local.RepositionRecord.SystemGeneratedIdentifier;

    Call(SpAlrtSearchByOffice.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport5);
  }

  private void UseSpAlrtSearchByPerson()
  {
    var useImport = new SpAlrtSearchByPerson.Import();
    var useExport = new SpAlrtSearchByPerson.Export();

    useImport.FilterOfficeServiceProviderAlert.Assign(
      export.FilterOfficeServiceProviderAlert);
    MoveFips(export.FilterFips, useImport.FilterFips);
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    MoveInfrastructure2(export.FilterInfrastructure,
      useImport.FilterInfrastructure);
    useImport.FilterCodeValue.Cdvalue = export.FilterEventType.Cdvalue;
    useImport.SortDesc.Cdvalue = export.FilterSort.Cdvalue;
    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.StartPageKeyOfficeServiceProviderAlert.Assign(
      export.PageKeys.Item.PageKeyOfficeServiceProviderAlert);
    MoveInfrastructure2(export.PageKeys.Item.PageKeyInfrastructure,
      useImport.StartPageKeyInfrastructure);
    useImport.RepositionRecord.SystemGeneratedIdentifier =
      local.RepositionRecord.SystemGeneratedIdentifier;

    Call(SpAlrtSearchByPerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport3);
  }

  private void UseSpAlrtSearchByServicePrvder()
  {
    var useImport = new SpAlrtSearchByServicePrvder.Import();
    var useExport = new SpAlrtSearchByServicePrvder.Export();

    MoveInfrastructure2(export.PageKeys.Item.PageKeyInfrastructure,
      useImport.StartPageKeyInfrastructure);
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    MoveCodeValue(export.FilterSort, useImport.SortDesc);
    useImport.FilterCodeValue.Cdvalue = export.FilterEventType.Cdvalue;
    MoveInfrastructure2(export.FilterInfrastructure,
      useImport.FilterInfrastructure);
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveFips(export.FilterFips, useImport.FilterFips);
    useImport.FilterOfficeServiceProviderAlert.Assign(
      export.FilterOfficeServiceProviderAlert);
    useImport.StartPageKeyOfficeServiceProviderAlert.Assign(
      export.PageKeys.Item.PageKeyOfficeServiceProviderAlert);

    Call(SpAlrtSearchByServicePrvder.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport2);
  }

  private void UseSpDeleteOspAlerts()
  {
    var useImport = new SpDeleteOspAlerts.Import();
    var useExport = new SpDeleteOspAlerts.Export();

    useImport.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
      export.Export1.Item.GxfficeServiceProviderAlert.SystemGeneratedIdentifier;
      

    Call(SpDeleteOspAlerts.Execute, useImport, useExport);
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.FilterInfrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCodeCodeValue()
  {
    entities.ExistingCode.Populated = false;
    entities.ExistingCodeValue.Populated = false;

    return Read("ReadCodeCodeValue",
      (db, command) =>
      {
        db.SetString(command, "codeName", local.WorkCode.CodeName);
        db.SetString(command, "cdvalue", export.FilterEventType.Cdvalue);
      },
      (db, reader) =>
      {
        entities.ExistingCode.Id = db.GetInt32(reader, 0);
        entities.ExistingCodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.ExistingCode.CodeName = db.GetString(reader, 1);
        entities.ExistingCodeValue.Id = db.GetInt32(reader, 2);
        entities.ExistingCodeValue.Cdvalue = db.GetString(reader, 3);
        entities.ExistingCode.Populated = true;
        entities.ExistingCodeValue.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.FilterInfrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.ExistingEventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "eveNo", export.Export1.Item.Gxnfrastructure.EventId);
        db.SetString(
          command, "eventDetailName",
          export.Export1.Item.Gxnfrastructure.EventDetailName);
        db.SetString(
          command, "reasonCode",
          export.Export1.Item.Gxnfrastructure.ReasonCode);
      },
      (db, reader) =>
      {
        entities.ExistingEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingEventDetail.DetailName = db.GetString(reader, 1);
        entities.ExistingEventDetail.ReasonCode = db.GetString(reader, 2);
        entities.ExistingEventDetail.EveNo = db.GetInt32(reader, 3);
        entities.ExistingEventDetail.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.FilterOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "roleCode", import.FilterOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "offGeneratedId", import.FilterOffice.SystemGeneratedId);
        db.SetString(command, "userId", import.FilterServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderOffice()
  {
    entities.ExistingOffice.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
        db.SetDate(
          command, "effectiveDate", local.CurrentDate.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 5);
        entities.ExistingOffice.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", import.FilterServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.Populated = true;
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
      /// A value of GiegalAction.
      /// </summary>
      [JsonPropertyName("giegalAction")]
      public LegalAction GiegalAction
      {
        get => giegalAction ??= new();
        set => giegalAction = value;
      }

      /// <summary>
      /// A value of GianualFipsTribAddress.
      /// </summary>
      [JsonPropertyName("gianualFipsTribAddress")]
      public FipsTribAddress GianualFipsTribAddress
      {
        get => gianualFipsTribAddress ??= new();
        set => gianualFipsTribAddress = value;
      }

      /// <summary>
      /// A value of GianualFips.
      /// </summary>
      [JsonPropertyName("gianualFips")]
      public Fips GianualFips
      {
        get => gianualFips ??= new();
        set => gianualFips = value;
      }

      /// <summary>
      /// A value of Ginfrastructure.
      /// </summary>
      [JsonPropertyName("ginfrastructure")]
      public Infrastructure Ginfrastructure
      {
        get => ginfrastructure ??= new();
        set => ginfrastructure = value;
      }

      /// <summary>
      /// A value of GifficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("gifficeServiceProviderAlert")]
      public OfficeServiceProviderAlert GifficeServiceProviderAlert
      {
        get => gifficeServiceProviderAlert ??= new();
        set => gifficeServiceProviderAlert = value;
      }

      /// <summary>
      /// A value of Giommon.
      /// </summary>
      [JsonPropertyName("giommon")]
      public Common Giommon
      {
        get => giommon ??= new();
        set => giommon = value;
      }

      /// <summary>
      /// A value of GisePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gisePersonsWorkSet")]
      public CsePersonsWorkSet GisePersonsWorkSet
      {
        get => gisePersonsWorkSet ??= new();
        set => gisePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalAction giegalAction;
      private FipsTribAddress gianualFipsTribAddress;
      private Fips gianualFips;
      private Infrastructure ginfrastructure;
      private OfficeServiceProviderAlert gifficeServiceProviderAlert;
      private Common giommon;
      private CsePersonsWorkSet gisePersonsWorkSet;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKeyInfrastructure.
      /// </summary>
      [JsonPropertyName("pageKeyInfrastructure")]
      public Infrastructure PageKeyInfrastructure
      {
        get => pageKeyInfrastructure ??= new();
        set => pageKeyInfrastructure = value;
      }

      /// <summary>
      /// A value of PageKeyOfficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("pageKeyOfficeServiceProviderAlert")]
      public OfficeServiceProviderAlert PageKeyOfficeServiceProviderAlert
      {
        get => pageKeyOfficeServiceProviderAlert ??= new();
        set => pageKeyOfficeServiceProviderAlert = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Infrastructure pageKeyInfrastructure;
      private OfficeServiceProviderAlert pageKeyOfficeServiceProviderAlert;
    }

    /// <summary>
    /// A value of SelectedFromCdvl.
    /// </summary>
    [JsonPropertyName("selectedFromCdvl")]
    public CodeValue SelectedFromCdvl
    {
      get => selectedFromCdvl ??= new();
      set => selectedFromCdvl = value;
    }

    /// <summary>
    /// A value of FilterSort.
    /// </summary>
    [JsonPropertyName("filterSort")]
    public CodeValue FilterSort
    {
      get => filterSort ??= new();
      set => filterSort = value;
    }

    /// <summary>
    /// A value of HiddenFilterSort.
    /// </summary>
    [JsonPropertyName("hiddenFilterSort")]
    public CodeValue HiddenFilterSort
    {
      get => hiddenFilterSort ??= new();
      set => hiddenFilterSort = value;
    }

    /// <summary>
    /// A value of SortPrompt.
    /// </summary>
    [JsonPropertyName("sortPrompt")]
    public Common SortPrompt
    {
      get => sortPrompt ??= new();
      set => sortPrompt = value;
    }

    /// <summary>
    /// A value of AlertPrompt.
    /// </summary>
    [JsonPropertyName("alertPrompt")]
    public Common AlertPrompt
    {
      get => alertPrompt ??= new();
      set => alertPrompt = value;
    }

    /// <summary>
    /// A value of EventTypePrompt.
    /// </summary>
    [JsonPropertyName("eventTypePrompt")]
    public Common EventTypePrompt
    {
      get => eventTypePrompt ??= new();
      set => eventTypePrompt = value;
    }

    /// <summary>
    /// A value of HiddenFilterEventType.
    /// </summary>
    [JsonPropertyName("hiddenFilterEventType")]
    public CodeValue HiddenFilterEventType
    {
      get => hiddenFilterEventType ??= new();
      set => hiddenFilterEventType = value;
    }

    /// <summary>
    /// A value of HiddenFilterFips.
    /// </summary>
    [JsonPropertyName("hiddenFilterFips")]
    public Fips HiddenFilterFips
    {
      get => hiddenFilterFips ??= new();
      set => hiddenFilterFips = value;
    }

    /// <summary>
    /// A value of HiddenFilterServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenFilterServiceProvider")]
    public ServiceProvider HiddenFilterServiceProvider
    {
      get => hiddenFilterServiceProvider ??= new();
      set => hiddenFilterServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenFilterFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hiddenFilterFipsTribAddress")]
    public FipsTribAddress HiddenFilterFipsTribAddress
    {
      get => hiddenFilterFipsTribAddress ??= new();
      set => hiddenFilterFipsTribAddress = value;
    }

    /// <summary>
    /// A value of HiddenFilterOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenFilterOfficeServiceProvider")]
    public OfficeServiceProvider HiddenFilterOfficeServiceProvider
    {
      get => hiddenFilterOfficeServiceProvider ??= new();
      set => hiddenFilterOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenFilterLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenFilterLegalAction")]
    public LegalAction HiddenFilterLegalAction
    {
      get => hiddenFilterLegalAction ??= new();
      set => hiddenFilterLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenFilterInfrastructure.
    /// </summary>
    [JsonPropertyName("hiddenFilterInfrastructure")]
    public Infrastructure HiddenFilterInfrastructure
    {
      get => hiddenFilterInfrastructure ??= new();
      set => hiddenFilterInfrastructure = value;
    }

    /// <summary>
    /// A value of HiddenFilterOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("hiddenFilterOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert HiddenFilterOfficeServiceProviderAlert
    {
      get => hiddenFilterOfficeServiceProviderAlert ??= new();
      set => hiddenFilterOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of HiddenFilterOffice.
    /// </summary>
    [JsonPropertyName("hiddenFilterOffice")]
    public Office HiddenFilterOffice
    {
      get => hiddenFilterOffice ??= new();
      set => hiddenFilterOffice = value;
    }

    /// <summary>
    /// A value of FilterFips.
    /// </summary>
    [JsonPropertyName("filterFips")]
    public Fips FilterFips
    {
      get => filterFips ??= new();
      set => filterFips = value;
    }

    /// <summary>
    /// A value of FilterServiceProvider.
    /// </summary>
    [JsonPropertyName("filterServiceProvider")]
    public ServiceProvider FilterServiceProvider
    {
      get => filterServiceProvider ??= new();
      set => filterServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterFipsTribAddress.
    /// </summary>
    [JsonPropertyName("filterFipsTribAddress")]
    public FipsTribAddress FilterFipsTribAddress
    {
      get => filterFipsTribAddress ??= new();
      set => filterFipsTribAddress = value;
    }

    /// <summary>
    /// A value of FilterOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("filterOfficeServiceProvider")]
    public OfficeServiceProvider FilterOfficeServiceProvider
    {
      get => filterOfficeServiceProvider ??= new();
      set => filterOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterLegalAction.
    /// </summary>
    [JsonPropertyName("filterLegalAction")]
    public LegalAction FilterLegalAction
    {
      get => filterLegalAction ??= new();
      set => filterLegalAction = value;
    }

    /// <summary>
    /// A value of FilterInfrastructure.
    /// </summary>
    [JsonPropertyName("filterInfrastructure")]
    public Infrastructure FilterInfrastructure
    {
      get => filterInfrastructure ??= new();
      set => filterInfrastructure = value;
    }

    /// <summary>
    /// A value of FilterOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("filterOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert FilterOfficeServiceProviderAlert
    {
      get => filterOfficeServiceProviderAlert ??= new();
      set => filterOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of FilterOffice.
    /// </summary>
    [JsonPropertyName("filterOffice")]
    public Office FilterOffice
    {
      get => filterOffice ??= new();
      set => filterOffice = value;
    }

    /// <summary>
    /// A value of SvpoPrompt.
    /// </summary>
    [JsonPropertyName("svpoPrompt")]
    public Common SvpoPrompt
    {
      get => svpoPrompt ??= new();
      set => svpoPrompt = value;
    }

    /// <summary>
    /// A value of FilterEventType.
    /// </summary>
    [JsonPropertyName("filterEventType")]
    public CodeValue FilterEventType
    {
      get => filterEventType ??= new();
      set => filterEventType = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    /// <summary>
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public Standard Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
    }

    /// <summary>
    /// A value of CurrentPage.
    /// </summary>
    [JsonPropertyName("currentPage")]
    public Common CurrentPage
    {
      get => currentPage ??= new();
      set => currentPage = value;
    }

    private CodeValue selectedFromCdvl;
    private CodeValue filterSort;
    private CodeValue hiddenFilterSort;
    private Common sortPrompt;
    private Common alertPrompt;
    private Common eventTypePrompt;
    private CodeValue hiddenFilterEventType;
    private Fips hiddenFilterFips;
    private ServiceProvider hiddenFilterServiceProvider;
    private FipsTribAddress hiddenFilterFipsTribAddress;
    private OfficeServiceProvider hiddenFilterOfficeServiceProvider;
    private LegalAction hiddenFilterLegalAction;
    private Infrastructure hiddenFilterInfrastructure;
    private OfficeServiceProviderAlert hiddenFilterOfficeServiceProviderAlert;
    private Office hiddenFilterOffice;
    private Fips filterFips;
    private ServiceProvider filterServiceProvider;
    private FipsTribAddress filterFipsTribAddress;
    private OfficeServiceProvider filterOfficeServiceProvider;
    private LegalAction filterLegalAction;
    private Infrastructure filterInfrastructure;
    private OfficeServiceProviderAlert filterOfficeServiceProviderAlert;
    private Office filterOffice;
    private Common svpoPrompt;
    private CodeValue filterEventType;
    private Array<ImportGroup> import1;
    private Standard standard;
    private NextTranInfo hidden;
    private Array<PageKeysGroup> pageKeys;
    private Standard scroll;
    private Common currentPage;
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
      /// A value of GxegalAction.
      /// </summary>
      [JsonPropertyName("gxegalAction")]
      public LegalAction GxegalAction
      {
        get => gxegalAction ??= new();
        set => gxegalAction = value;
      }

      /// <summary>
      /// A value of GxanualFipsTribAddress.
      /// </summary>
      [JsonPropertyName("gxanualFipsTribAddress")]
      public FipsTribAddress GxanualFipsTribAddress
      {
        get => gxanualFipsTribAddress ??= new();
        set => gxanualFipsTribAddress = value;
      }

      /// <summary>
      /// A value of GxanualFips.
      /// </summary>
      [JsonPropertyName("gxanualFips")]
      public Fips GxanualFips
      {
        get => gxanualFips ??= new();
        set => gxanualFips = value;
      }

      /// <summary>
      /// A value of Gxnfrastructure.
      /// </summary>
      [JsonPropertyName("gxnfrastructure")]
      public Infrastructure Gxnfrastructure
      {
        get => gxnfrastructure ??= new();
        set => gxnfrastructure = value;
      }

      /// <summary>
      /// A value of GxfficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("gxfficeServiceProviderAlert")]
      public OfficeServiceProviderAlert GxfficeServiceProviderAlert
      {
        get => gxfficeServiceProviderAlert ??= new();
        set => gxfficeServiceProviderAlert = value;
      }

      /// <summary>
      /// A value of Gxommon.
      /// </summary>
      [JsonPropertyName("gxommon")]
      public Common Gxommon
      {
        get => gxommon ??= new();
        set => gxommon = value;
      }

      /// <summary>
      /// A value of GxsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gxsePersonsWorkSet")]
      public CsePersonsWorkSet GxsePersonsWorkSet
      {
        get => gxsePersonsWorkSet ??= new();
        set => gxsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalAction gxegalAction;
      private FipsTribAddress gxanualFipsTribAddress;
      private Fips gxanualFips;
      private Infrastructure gxnfrastructure;
      private OfficeServiceProviderAlert gxfficeServiceProviderAlert;
      private Common gxommon;
      private CsePersonsWorkSet gxsePersonsWorkSet;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of PageKeyInfrastructure.
      /// </summary>
      [JsonPropertyName("pageKeyInfrastructure")]
      public Infrastructure PageKeyInfrastructure
      {
        get => pageKeyInfrastructure ??= new();
        set => pageKeyInfrastructure = value;
      }

      /// <summary>
      /// A value of PageKeyOfficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("pageKeyOfficeServiceProviderAlert")]
      public OfficeServiceProviderAlert PageKeyOfficeServiceProviderAlert
      {
        get => pageKeyOfficeServiceProviderAlert ??= new();
        set => pageKeyOfficeServiceProviderAlert = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Infrastructure pageKeyInfrastructure;
      private OfficeServiceProviderAlert pageKeyOfficeServiceProviderAlert;
    }

    /// <summary>
    /// A value of FilterSort.
    /// </summary>
    [JsonPropertyName("filterSort")]
    public CodeValue FilterSort
    {
      get => filterSort ??= new();
      set => filterSort = value;
    }

    /// <summary>
    /// A value of HiddenFilterSort.
    /// </summary>
    [JsonPropertyName("hiddenFilterSort")]
    public CodeValue HiddenFilterSort
    {
      get => hiddenFilterSort ??= new();
      set => hiddenFilterSort = value;
    }

    /// <summary>
    /// A value of SortPrompt.
    /// </summary>
    [JsonPropertyName("sortPrompt")]
    public Common SortPrompt
    {
      get => sortPrompt ??= new();
      set => sortPrompt = value;
    }

    /// <summary>
    /// A value of AlertPrompt.
    /// </summary>
    [JsonPropertyName("alertPrompt")]
    public Common AlertPrompt
    {
      get => alertPrompt ??= new();
      set => alertPrompt = value;
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public Code Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    /// <summary>
    /// A value of EventTypePrompt.
    /// </summary>
    [JsonPropertyName("eventTypePrompt")]
    public Common EventTypePrompt
    {
      get => eventTypePrompt ??= new();
      set => eventTypePrompt = value;
    }

    /// <summary>
    /// A value of FilterEventType.
    /// </summary>
    [JsonPropertyName("filterEventType")]
    public CodeValue FilterEventType
    {
      get => filterEventType ??= new();
      set => filterEventType = value;
    }

    /// <summary>
    /// A value of HiddenFilterEventType.
    /// </summary>
    [JsonPropertyName("hiddenFilterEventType")]
    public CodeValue HiddenFilterEventType
    {
      get => hiddenFilterEventType ??= new();
      set => hiddenFilterEventType = value;
    }

    /// <summary>
    /// A value of HiddenFilterFips.
    /// </summary>
    [JsonPropertyName("hiddenFilterFips")]
    public Fips HiddenFilterFips
    {
      get => hiddenFilterFips ??= new();
      set => hiddenFilterFips = value;
    }

    /// <summary>
    /// A value of HiddenFilterServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenFilterServiceProvider")]
    public ServiceProvider HiddenFilterServiceProvider
    {
      get => hiddenFilterServiceProvider ??= new();
      set => hiddenFilterServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenFilterFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hiddenFilterFipsTribAddress")]
    public FipsTribAddress HiddenFilterFipsTribAddress
    {
      get => hiddenFilterFipsTribAddress ??= new();
      set => hiddenFilterFipsTribAddress = value;
    }

    /// <summary>
    /// A value of HiddenFilterOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("hiddenFilterOfficeServiceProvider")]
    public OfficeServiceProvider HiddenFilterOfficeServiceProvider
    {
      get => hiddenFilterOfficeServiceProvider ??= new();
      set => hiddenFilterOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of HiddenFilterLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenFilterLegalAction")]
    public LegalAction HiddenFilterLegalAction
    {
      get => hiddenFilterLegalAction ??= new();
      set => hiddenFilterLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenFilterInfrastructure.
    /// </summary>
    [JsonPropertyName("hiddenFilterInfrastructure")]
    public Infrastructure HiddenFilterInfrastructure
    {
      get => hiddenFilterInfrastructure ??= new();
      set => hiddenFilterInfrastructure = value;
    }

    /// <summary>
    /// A value of HiddenFilterOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("hiddenFilterOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert HiddenFilterOfficeServiceProviderAlert
    {
      get => hiddenFilterOfficeServiceProviderAlert ??= new();
      set => hiddenFilterOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of HiddenFilterOffice.
    /// </summary>
    [JsonPropertyName("hiddenFilterOffice")]
    public Office HiddenFilterOffice
    {
      get => hiddenFilterOffice ??= new();
      set => hiddenFilterOffice = value;
    }

    /// <summary>
    /// A value of FilterFips.
    /// </summary>
    [JsonPropertyName("filterFips")]
    public Fips FilterFips
    {
      get => filterFips ??= new();
      set => filterFips = value;
    }

    /// <summary>
    /// A value of SelectedOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("selectedOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert SelectedOfficeServiceProviderAlert
    {
      get => selectedOfficeServiceProviderAlert ??= new();
      set => selectedOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of SelectedInfrastructure.
    /// </summary>
    [JsonPropertyName("selectedInfrastructure")]
    public Infrastructure SelectedInfrastructure
    {
      get => selectedInfrastructure ??= new();
      set => selectedInfrastructure = value;
    }

    /// <summary>
    /// A value of SvpoPrompt.
    /// </summary>
    [JsonPropertyName("svpoPrompt")]
    public Common SvpoPrompt
    {
      get => svpoPrompt ??= new();
      set => svpoPrompt = value;
    }

    /// <summary>
    /// A value of FilterServiceProvider.
    /// </summary>
    [JsonPropertyName("filterServiceProvider")]
    public ServiceProvider FilterServiceProvider
    {
      get => filterServiceProvider ??= new();
      set => filterServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("filterOfficeServiceProvider")]
    public OfficeServiceProvider FilterOfficeServiceProvider
    {
      get => filterOfficeServiceProvider ??= new();
      set => filterOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterOffice.
    /// </summary>
    [JsonPropertyName("filterOffice")]
    public Office FilterOffice
    {
      get => filterOffice ??= new();
      set => filterOffice = value;
    }

    /// <summary>
    /// A value of FilterFipsTribAddress.
    /// </summary>
    [JsonPropertyName("filterFipsTribAddress")]
    public FipsTribAddress FilterFipsTribAddress
    {
      get => filterFipsTribAddress ??= new();
      set => filterFipsTribAddress = value;
    }

    /// <summary>
    /// A value of FilterOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("filterOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert FilterOfficeServiceProviderAlert
    {
      get => filterOfficeServiceProviderAlert ??= new();
      set => filterOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of FilterInfrastructure.
    /// </summary>
    [JsonPropertyName("filterInfrastructure")]
    public Infrastructure FilterInfrastructure
    {
      get => filterInfrastructure ??= new();
      set => filterInfrastructure = value;
    }

    /// <summary>
    /// A value of FilterLegalAction.
    /// </summary>
    [JsonPropertyName("filterLegalAction")]
    public LegalAction FilterLegalAction
    {
      get => filterLegalAction ??= new();
      set => filterLegalAction = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of CurrentPage.
    /// </summary>
    [JsonPropertyName("currentPage")]
    public Common CurrentPage
    {
      get => currentPage ??= new();
      set => currentPage = value;
    }

    /// <summary>
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public Standard Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
    }

    /// <summary>
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
    }

    private CodeValue filterSort;
    private CodeValue hiddenFilterSort;
    private Common sortPrompt;
    private Common alertPrompt;
    private Code flow;
    private Common eventTypePrompt;
    private CodeValue filterEventType;
    private CodeValue hiddenFilterEventType;
    private Fips hiddenFilterFips;
    private ServiceProvider hiddenFilterServiceProvider;
    private FipsTribAddress hiddenFilterFipsTribAddress;
    private OfficeServiceProvider hiddenFilterOfficeServiceProvider;
    private LegalAction hiddenFilterLegalAction;
    private Infrastructure hiddenFilterInfrastructure;
    private OfficeServiceProviderAlert hiddenFilterOfficeServiceProviderAlert;
    private Office hiddenFilterOffice;
    private Fips filterFips;
    private OfficeServiceProviderAlert selectedOfficeServiceProviderAlert;
    private Infrastructure selectedInfrastructure;
    private Common svpoPrompt;
    private ServiceProvider filterServiceProvider;
    private OfficeServiceProvider filterOfficeServiceProvider;
    private Office filterOffice;
    private FipsTribAddress filterFipsTribAddress;
    private OfficeServiceProviderAlert filterOfficeServiceProviderAlert;
    private Infrastructure filterInfrastructure;
    private LegalAction filterLegalAction;
    private Array<ExportGroup> export1;
    private Standard standard;
    private NextTranInfo hidden;
    private Common currentPage;
    private Standard scroll;
    private Array<PageKeysGroup> pageKeys;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
    }

    /// <summary>
    /// A value of OffSpRoleValid.
    /// </summary>
    [JsonPropertyName("offSpRoleValid")]
    public Common OffSpRoleValid
    {
      get => offSpRoleValid ??= new();
      set => offSpRoleValid = value;
    }

    /// <summary>
    /// A value of DefaultSort.
    /// </summary>
    [JsonPropertyName("defaultSort")]
    public CodeValue DefaultSort
    {
      get => defaultSort ??= new();
      set => defaultSort = value;
    }

    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public OfficeServiceProviderAlert Default1
    {
      get => default1 ??= new();
      set => default1 = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of NextPageKeyOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("nextPageKeyOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert NextPageKeyOfficeServiceProviderAlert
    {
      get => nextPageKeyOfficeServiceProviderAlert ??= new();
      set => nextPageKeyOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of NextPageKeyInfrastructure.
    /// </summary>
    [JsonPropertyName("nextPageKeyInfrastructure")]
    public Infrastructure NextPageKeyInfrastructure
    {
      get => nextPageKeyInfrastructure ??= new();
      set => nextPageKeyInfrastructure = value;
    }

    /// <summary>
    /// A value of RecordFound.
    /// </summary>
    [JsonPropertyName("recordFound")]
    public Common RecordFound
    {
      get => recordFound ??= new();
      set => recordFound = value;
    }

    /// <summary>
    /// A value of FipsLengthCount.
    /// </summary>
    [JsonPropertyName("fipsLengthCount")]
    public Common FipsLengthCount
    {
      get => fipsLengthCount ??= new();
      set => fipsLengthCount = value;
    }

    /// <summary>
    /// A value of FipsInformation.
    /// </summary>
    [JsonPropertyName("fipsInformation")]
    public TextWorkArea FipsInformation
    {
      get => fipsInformation ??= new();
      set => fipsInformation = value;
    }

    /// <summary>
    /// A value of LotalTotalDetailLength.
    /// </summary>
    [JsonPropertyName("lotalTotalDetailLength")]
    public Common LotalTotalDetailLength
    {
      get => lotalTotalDetailLength ??= new();
      set => lotalTotalDetailLength = value;
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
    /// A value of SuperNexttran.
    /// </summary>
    [JsonPropertyName("superNexttran")]
    public Common SuperNexttran
    {
      get => superNexttran ??= new();
      set => superNexttran = value;
    }

    /// <summary>
    /// A value of Local3.
    /// </summary>
    [JsonPropertyName("local3")]
    public OfficeServiceProviderAlert Local3
    {
      get => local3 ??= new();
      set => local3 = value;
    }

    /// <summary>
    /// A value of Y.
    /// </summary>
    [JsonPropertyName("y")]
    public OfficeServiceProviderAlert Y
    {
      get => y ??= new();
      set => y = value;
    }

    /// <summary>
    /// A value of N.
    /// </summary>
    [JsonPropertyName("n")]
    public OfficeServiceProviderAlert N
    {
      get => n ??= new();
      set => n = value;
    }

    /// <summary>
    /// A value of AutOrMan.
    /// </summary>
    [JsonPropertyName("autOrMan")]
    public OfficeServiceProviderAlert AutOrMan
    {
      get => autOrMan ??= new();
      set => autOrMan = value;
    }

    /// <summary>
    /// A value of WorkCode.
    /// </summary>
    [JsonPropertyName("workCode")]
    public Code WorkCode
    {
      get => workCode ??= new();
      set => workCode = value;
    }

    /// <summary>
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
    }

    /// <summary>
    /// A value of NullInfrastructure.
    /// </summary>
    [JsonPropertyName("nullInfrastructure")]
    public Infrastructure NullInfrastructure
    {
      get => nullInfrastructure ??= new();
      set => nullInfrastructure = value;
    }

    /// <summary>
    /// A value of NullOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("nullOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert NullOfficeServiceProviderAlert
    {
      get => nullOfficeServiceProviderAlert ??= new();
      set => nullOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of NullCommon.
    /// </summary>
    [JsonPropertyName("nullCommon")]
    public Common NullCommon
    {
      get => nullCommon ??= new();
      set => nullCommon = value;
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Standard Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of SelectError.
    /// </summary>
    [JsonPropertyName("selectError")]
    public Common SelectError
    {
      get => selectError ??= new();
      set => selectError = value;
    }

    /// <summary>
    /// A value of ForManualDisplay.
    /// </summary>
    [JsonPropertyName("forManualDisplay")]
    public OfficeServiceProviderAlert ForManualDisplay
    {
      get => forManualDisplay ??= new();
      set => forManualDisplay = value;
    }

    /// <summary>
    /// A value of Change.
    /// </summary>
    [JsonPropertyName("change")]
    public Common Change
    {
      get => change ??= new();
      set => change = value;
    }

    /// <summary>
    /// A value of Initial.
    /// </summary>
    [JsonPropertyName("initial")]
    public CsePersonsWorkSet Initial
    {
      get => initial ??= new();
      set => initial = value;
    }

    /// <summary>
    /// A value of InitialInfrastr.
    /// </summary>
    [JsonPropertyName("initialInfrastr")]
    public Infrastructure InitialInfrastr
    {
      get => initialInfrastr ??= new();
      set => initialInfrastr = value;
    }

    /// <summary>
    /// A value of WorkInfrastructure.
    /// </summary>
    [JsonPropertyName("workInfrastructure")]
    public Infrastructure WorkInfrastructure
    {
      get => workInfrastructure ??= new();
      set => workInfrastructure = value;
    }

    /// <summary>
    /// A value of InitialLa.
    /// </summary>
    [JsonPropertyName("initialLa")]
    public LegalAction InitialLa
    {
      get => initialLa ??= new();
      set => initialLa = value;
    }

    /// <summary>
    /// A value of La.
    /// </summary>
    [JsonPropertyName("la")]
    public LegalAction La
    {
      get => la ??= new();
      set => la = value;
    }

    /// <summary>
    /// A value of IsSupervisor.
    /// </summary>
    [JsonPropertyName("isSupervisor")]
    public Common IsSupervisor
    {
      get => isSupervisor ??= new();
      set => isSupervisor = value;
    }

    /// <summary>
    /// A value of LoggedOnUser.
    /// </summary>
    [JsonPropertyName("loggedOnUser")]
    public ServiceProvider LoggedOnUser
    {
      get => loggedOnUser ??= new();
      set => loggedOnUser = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of Pointer.
    /// </summary>
    [JsonPropertyName("pointer")]
    public Common Pointer
    {
      get => pointer ??= new();
      set => pointer = value;
    }

    /// <summary>
    /// A value of FindAlrt.
    /// </summary>
    [JsonPropertyName("findAlrt")]
    public Common FindAlrt
    {
      get => findAlrt ??= new();
      set => findAlrt = value;
    }

    /// <summary>
    /// A value of RepositionRecord.
    /// </summary>
    [JsonPropertyName("repositionRecord")]
    public OfficeServiceProviderAlert RepositionRecord
    {
      get => repositionRecord ??= new();
      set => repositionRecord = value;
    }

    private Common promptCount;
    private Common offSpRoleValid;
    private CodeValue defaultSort;
    private OfficeServiceProviderAlert default1;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private DateWorkArea nullDateWorkArea;
    private OfficeServiceProviderAlert nextPageKeyOfficeServiceProviderAlert;
    private Infrastructure nextPageKeyInfrastructure;
    private Common recordFound;
    private Common fipsLengthCount;
    private TextWorkArea fipsInformation;
    private Common lotalTotalDetailLength;
    private FipsTribAddress nullFipsTribAddress;
    private Fips nullFips;
    private Common superNexttran;
    private OfficeServiceProviderAlert local3;
    private OfficeServiceProviderAlert y;
    private OfficeServiceProviderAlert n;
    private OfficeServiceProviderAlert autOrMan;
    private Code workCode;
    private LegalAction nullLegalAction;
    private Infrastructure nullInfrastructure;
    private OfficeServiceProviderAlert nullOfficeServiceProviderAlert;
    private Common nullCommon;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private Standard previous;
    private Common selectError;
    private OfficeServiceProviderAlert forManualDisplay;
    private Common change;
    private CsePersonsWorkSet initial;
    private Infrastructure initialInfrastr;
    private Infrastructure workInfrastructure;
    private LegalAction initialLa;
    private LegalAction la;
    private Common isSupervisor;
    private ServiceProvider loggedOnUser;
    private DateWorkArea currentDate;
    private DateWorkArea initialized;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private TextWorkArea textWorkArea;
    private Case1 case1;
    private Common count;
    private Common pointer;
    private Common findAlrt;
    private OfficeServiceProviderAlert repositionRecord;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingEventDetail.
    /// </summary>
    [JsonPropertyName("existingEventDetail")]
    public EventDetail ExistingEventDetail
    {
      get => existingEventDetail ??= new();
      set => existingEventDetail = value;
    }

    /// <summary>
    /// A value of ExistingEvent.
    /// </summary>
    [JsonPropertyName("existingEvent")]
    public Event1 ExistingEvent
    {
      get => existingEvent ??= new();
      set => existingEvent = value;
    }

    /// <summary>
    /// A value of ExistingCode.
    /// </summary>
    [JsonPropertyName("existingCode")]
    public Code ExistingCode
    {
      get => existingCode ??= new();
      set => existingCode = value;
    }

    /// <summary>
    /// A value of ExistingCodeValue.
    /// </summary>
    [JsonPropertyName("existingCodeValue")]
    public CodeValue ExistingCodeValue
    {
      get => existingCodeValue ??= new();
      set => existingCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert ExistingOfficeServiceProviderAlert
    {
      get => existingOfficeServiceProviderAlert ??= new();
      set => existingOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    private CsePerson existingCsePerson;
    private Case1 existingCase;
    private EventDetail existingEventDetail;
    private Event1 existingEvent;
    private Code existingCode;
    private CodeValue existingCodeValue;
    private LegalAction existingLegalAction;
    private Tribunal existingTribunal;
    private Fips existingFips;
    private FipsTribAddress existingFipsTribAddress;
    private OfficeServiceProviderAlert existingOfficeServiceProviderAlert;
    private Office existingOffice;
    private Infrastructure existingInfrastructure;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private ServiceProvider existingServiceProvider;
  }
#endregion
}
