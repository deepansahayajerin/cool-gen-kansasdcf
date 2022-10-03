// Program: SP_AMEN_APPOINTMENT_MANAGEMENT, ID: 371749338, model: 746.
// Short name: SWEAMENP
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
/// A program: SP_AMEN_APPOINTMENT_MANAGEMENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpAmenAppointmentManagement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_AMEN_APPOINTMENT_MANAGEMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAmenAppointmentManagement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAmenAppointmentManagement.
  /// </summary>
  public SpAmenAppointmentManagement(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    //                M A I N T E N A N C E    L O G
    // ----------------------------------------------------------------------------
    // Date	        Developer	  Request #	Description
    // 10/24/1996	Regan Welborn     Initial Development
    // 01/14/1997	Raju			event insertion
    // 01/23/1997	Regan/Raju		added local timestamp for
    // 					HIST display
    // 04/28/1997	Siraj Konkader		Fixed explicit scrolling and
    // 					other bugs.
    // 12/03/1998	M. Ramirez		Revised print process.
    // 05/25/1999      swsrkeh           Phase II changes
    // 01/06/2000	M Ramirez	83300	NEXT TRAN needs to be cleared
    // 					before invoking print process
    // 02/20/2009      Arun Mathias   CQ#8968  If the user presses PF15 then 
    // force
    //                                         
    // the user to select two records.
    //                                         
    // Also, Fixed scrolling abend.
    // 04/06/2009      Arun Mathias   CQ#10202 Should display correct SP info,
    //                                         
    // when SP known to multiple offices/roles
    // ----------------------------------------------------------------------------
    // **** Start of HouseKeeping - roll Imports to Exports ****
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.Appointment.CreatedTimestamp = local.Initialized.Timestamp;

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    export.Comp.SelectChar = import.Comp.SelectChar;
    export.ServiceProvider.SelectChar = import.ServiceProvider.SelectChar;
    export.DisplayAll.Flag = import.DisplayAll.Flag;

    if (IsEmpty(import.DisplayAll.Flag))
    {
      export.DisplayAll.Flag = "N";
    }

    export.FormattedSrvcprvdrName.Text32 = import.FormattedSrvcprvdrName.Text32;
    export.SearchCase.Number = import.SearchCase.Number;
    MoveCsePerson(import.SearchCsePerson, export.SearchCsePerson);
    MoveOffice(import.SearchOffice, export.SearchOffice);
    MoveOfficeServiceProvider(import.SearchOfficeServiceProvider,
      export.SearchOfficeServiceProvider);
    MoveServiceProvider(import.SearchServiceProvider,
      export.SearchServiceProvider);
    export.Starting.Date = import.Starting.Date;
    export.SearchHiddenCheckCase.Number = import.SearchHiddenCheckCase.Number;
    MoveCsePerson(import.SearchHiddenCheckCsePerson,
      export.SearchHiddenCheckCsePerson);
    MoveOffice(import.SearchHiddenCheckOffice, export.SearchHiddenCheckOffice);
    MoveOfficeServiceProvider(import.SearchHiddenCheckOfficeServiceProvider,
      export.SearchHiddenCheckOfficeServiceProvider);
    MoveServiceProvider(import.SearchHiddenCheckServiceProvider,
      export.SearchHiddenCheckServiceProvider);
    export.StartingHiddenCheck.Date = import.StartingHiddenCheck.Date;
    export.DiplayAllHiddenCheck.Flag = import.DisplayAllHiddenCheck.Flag;
    MoveStandard(import.Standard, export.Standard);
    export.Hidden.Assign(import.Hidden);
    export.HiddenPageCount.PageNumber = import.HiddenPageCount.PageNumber;
    export.HiddenPrev.Command = import.HiddenPrev.Command;
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // Start of code - Raju 01/14/97:1126 hrs CST
    // ---------------------------------------------
    for(import.Imp3.Index = 0; import.Imp3.Index < import.Imp3.Count; ++
      import.Imp3.Index)
    {
      if (!import.Imp3.CheckSize())
      {
        break;
      }

      export.Exp3.Index = import.Imp3.Index;
      export.Exp3.CheckSize();

      MoveAppointment1(import.Imp3.Item.Imp3LastReadHidden,
        export.Exp3.Update.Exp3LastReadHidden);
    }

    import.Imp3.CheckIndex();

    if (!IsEmpty(export.SearchCase.Number))
    {
      UseCabZeroFillNumber2();
    }

    if (!IsEmpty(export.SearchCsePerson.Number))
    {
      UseCabZeroFillNumber1();
    }

    if (Equal(import.Starting.Date, local.Initialized.Date))
    {
      export.Starting.Date = local.Current.Date;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      if (import.Import1.IsEmpty)
      {
      }
      else
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

          export.Export1.Update.GrAppointment.Assign(
            import.Import1.Item.GrAppointment);
          export.Export1.Update.GrExportAmPmInd.SelectChar =
            import.Import1.Item.GrImportAmPmInd.SelectChar;
          export.Export1.Update.GrCase.Number =
            import.Import1.Item.GrCase.Number;
          MoveCsePerson(import.Import1.Item.GrCsePerson,
            export.Export1.Update.GrCsePerson);
          export.Export1.Update.GrCaseRole.Type1 =
            import.Import1.Item.GrCaseRole.Type1;
          export.Export1.Update.GrOffice.SystemGeneratedId =
            import.Import1.Item.GrOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(import.Import1.Item.GrOfficeServiceProvider,
            export.Export1.Update.GrOfficeServiceProvider);
          MoveServiceProvider(import.Import1.Item.GrServiceProvider,
            export.Export1.Update.GrServiceProvider);
          export.Export1.Update.GrExportCdvlReason.SelectChar =
            import.Import1.Item.GrImportCdvlReason.SelectChar;
          export.Export1.Update.GrExportCdvlResult.SelectChar =
            import.Import1.Item.GrImportCdvlResult.SelectChar;
          export.Export1.Update.GrExportCdvlType.SelectChar =
            import.Import1.Item.GrImportCdvlType.SelectChar;
          export.Export1.Update.GrExportCsePerson.SelectChar =
            import.Import1.Item.GrImportCsePerson.SelectChar;
          export.Export1.Update.GrExportServiceProvider.SelectChar =
            import.Import1.Item.GrImportServiceProvider.SelectChar;
          export.Export1.Update.GrExportLineSelect.SelectChar =
            import.Import1.Item.GrImportLineSelect.SelectChar;

          if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) == 'S')
          {
            local.Pointer.Count = export.Export1.Index + 1;
          }
        }

        import.Import1.CheckIndex();
      }

      if (import.HiddenCheck.IsEmpty)
      {
      }
      else
      {
        for(import.HiddenCheck.Index = 0; import.HiddenCheck.Index < import
          .HiddenCheck.Count; ++import.HiddenCheck.Index)
        {
          if (!import.HiddenCheck.CheckSize())
          {
            break;
          }

          export.HiddenCheck.Index = import.HiddenCheck.Index;
          export.HiddenCheck.CheckSize();

          export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Assign(
            import.HiddenCheck.Item.GrImportHiddenCheckAppointment);
          export.HiddenCheck.Update.GrExportHiddenAmPmInd.SelectChar =
            import.HiddenCheck.Item.GrImportHiddenAmPmInd.SelectChar;
          export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
            import.HiddenCheck.Item.GrImportHiddenCheckCase.Number;
          MoveCsePerson(import.HiddenCheck.Item.GrImportHiddenCheckCsePerson,
            export.HiddenCheck.Update.GrExportHiddenCheckCsePerson);
          export.HiddenCheck.Update.GrExportHiddenCheckOffice.
            SystemGeneratedId =
              import.HiddenCheck.Item.GrImportHiddenCheckOffice.
              SystemGeneratedId;
          MoveOfficeServiceProvider(import.HiddenCheck.Item.
            GrImportHiddenCheckOfficeServiceProvider,
            export.HiddenCheck.Update.GrExportHiddenCheckOfficeServiceProvider);
            
          MoveServiceProvider(import.HiddenCheck.Item.
            GrImportHiddenCheckServiceProvider,
            export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider);
        }

        import.HiddenCheck.CheckIndex();
      }

      for(import.HiddenCheck.Index = 0; import.HiddenCheck.Index < import
        .HiddenCheck.Count; ++import.HiddenCheck.Index)
      {
        if (!import.HiddenCheck.CheckSize())
        {
          break;
        }

        export.HiddenCheck.Index = import.HiddenCheck.Index;
        export.HiddenCheck.CheckSize();

        export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Assign(
          import.HiddenCheck.Item.GrImportHiddenCheckAppointment);
        export.HiddenCheck.Update.GrExportHiddenAmPmInd.SelectChar =
          import.HiddenCheck.Item.GrImportHiddenAmPmInd.SelectChar;
        export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
          import.HiddenCheck.Item.GrImportHiddenCheckCase.Number;
        MoveCsePerson(import.HiddenCheck.Item.GrImportHiddenCheckCsePerson,
          export.HiddenCheck.Update.GrExportHiddenCheckCsePerson);
        export.HiddenCheck.Update.GrExportHiddenCheckOffice.SystemGeneratedId =
          import.HiddenCheck.Item.GrImportHiddenCheckOffice.SystemGeneratedId;
        MoveOfficeServiceProvider(import.HiddenCheck.Item.
          GrImportHiddenCheckOfficeServiceProvider,
          export.HiddenCheck.Update.GrExportHiddenCheckOfficeServiceProvider);
        MoveServiceProvider(import.HiddenCheck.Item.
          GrImportHiddenCheckServiceProvider,
          export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider);
      }

      import.HiddenCheck.CheckIndex();

      if (import.HiddenPageKeys.IsEmpty)
      {
      }
      else
      {
        for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
          .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
        {
          if (!import.HiddenPageKeys.CheckSize())
          {
            break;
          }

          export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
          export.HiddenPageKeys.CheckSize();

          export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
            import.HiddenPageKeys.Item.GimportHiddenPageKeys);
        }

        import.HiddenPageKeys.CheckIndex();
      }
    }

    // **** End of HouseKeeping - roll Imports to Exports ****
    // **** Validate all Commands ****
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        break;
      case "DELETE":
        break;
      case "DISPLAY":
        break;
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          export.Hidden.CaseNumber = import.SearchCase.Number;
          export.Hidden.CsePersonNumber = import.SearchCsePerson.Number;

          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          UseScCabNextTranPut1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.Standard, "nextTransaction");

            field1.Error = true;

            break;
          }

          return;
        }

        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "HIST":
        break;
      case "LINKPRED":
        break;
      case "LIST":
        break;
      case "NEXT":
        // *** CQ#8968 Added OR clause to not abend if it exceeds maximum pages 
        // ***
        if (!export.Export1.IsFull || import.HiddenPageCount.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        local.PageCount.PageNumber = import.HiddenPageCount.PageNumber + 1;

        if (Equal(import.HiddenPrev.Command, "SUCC") || Equal
          (import.HiddenPrev.Command, "PRED"))
        {
          import.HiddenPageKeys.Index = local.PageCount.PageNumber - 1;
          import.HiddenPageKeys.CheckSize();

          local.PageKeys.CreatedTimestamp =
            import.HiddenPageKeys.Item.GimportHiddenPageKeys.CreatedTimestamp;

          break;
        }

        // ***************************************************************
        //   Remember that, in the display, the first occurrence of
        //   the repeating group was set to the initialized values.
        //   But the page number was incremented inside the display cab.
        // ***************************************************************
        export.HiddenPageKeys.Index = local.PageCount.PageNumber - 1;
        export.HiddenPageKeys.CheckSize();

        local.PageKeys.Assign(export.HiddenPageKeys.Item.GexportHiddenPageKeys);

        if (!Equal(export.SearchCase.Number, export.SearchHiddenCheckCase.Number))
          
        {
          var field1 = GetField(export.SearchCase, "number");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (!Equal(export.SearchCsePerson.Number,
          export.SearchHiddenCheckCsePerson.Number))
        {
          var field1 = GetField(export.SearchCsePerson, "number");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (export.SearchOffice.SystemGeneratedId != export
          .SearchHiddenCheckOffice.SystemGeneratedId)
        {
          var field1 = GetField(export.SearchOffice, "systemGeneratedId");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (!Equal(export.SearchOfficeServiceProvider.RoleCode,
          export.SearchHiddenCheckOfficeServiceProvider.RoleCode))
        {
          var field1 = GetField(export.SearchOfficeServiceProvider, "roleCode");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (!Equal(export.SearchServiceProvider.UserId,
          export.SearchHiddenCheckServiceProvider.UserId))
        {
          var field1 = GetField(export.SearchServiceProvider, "userId");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }
        else
        {
          local.Read.UserId = export.SearchServiceProvider.UserId;
        }

        if (!Equal(export.Starting.Date, export.StartingHiddenCheck.Date))
        {
          var field1 = GetField(export.Starting, "date");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (AsChar(export.DisplayAll.Flag) != AsChar
          (export.DiplayAllHiddenCheck.Flag))
        {
          var field1 = GetField(export.DisplayAll, "flag");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        break;
      case "PRED":
        break;
      case "PREV":
        if (Equal(import.Standard.ScrollingMessage, "More   +") || Equal
          (import.Standard.ScrollingMessage, "More") || IsEmpty
          (import.Standard.ScrollingMessage))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        if (Equal(import.HiddenPrev.Command, "PRED") || Equal
          (import.HiddenPrev.Command, "SUCC"))
        {
          import.HiddenPageKeys.Index = import.HiddenPageCount.PageNumber - 2;
          import.HiddenPageKeys.CheckSize();

          local.PageKeys.CreatedTimestamp =
            import.HiddenPageKeys.Item.GimportHiddenPageKeys.CreatedTimestamp;

          // ***************************************************************
          //   In order to have one read each, instead of a prev and next
          //   CAB, the page number must be decremented to allow for the
          //   incrementation that occurs within the CAB.
          // ***************************************************************
          local.PageCount.PageNumber = import.HiddenPageCount.PageNumber - 1;

          break;
        }

        // ***************************************************************
        //   Remember that, in the display, the first occurrence of
        //   the repeating group was set to the initialized values.
        //   But the page number was incremented inside the display cab.
        // ***************************************************************
        export.HiddenPageKeys.Index = import.HiddenPageCount.PageNumber - 2;
        export.HiddenPageKeys.CheckSize();

        local.PageKeys.Assign(export.HiddenPageKeys.Item.GexportHiddenPageKeys);
        local.PageCount.PageNumber = import.HiddenPageCount.PageNumber - 1;

        if (!Equal(export.SearchCase.Number, export.SearchHiddenCheckCase.Number))
          
        {
          var field1 = GetField(export.SearchCase, "number");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (!Equal(export.SearchCsePerson.Number,
          export.SearchHiddenCheckCsePerson.Number))
        {
          var field1 = GetField(export.SearchCsePerson, "number");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (export.SearchOffice.SystemGeneratedId != export
          .SearchHiddenCheckOffice.SystemGeneratedId)
        {
          var field1 = GetField(export.SearchOffice, "systemGeneratedId");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (!Equal(export.SearchOfficeServiceProvider.RoleCode,
          export.SearchHiddenCheckOfficeServiceProvider.RoleCode))
        {
          var field1 = GetField(export.SearchOfficeServiceProvider, "roleCode");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (!Equal(export.SearchServiceProvider.UserId,
          export.SearchHiddenCheckServiceProvider.UserId))
        {
          var field1 = GetField(export.SearchServiceProvider, "userId");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }
        else
        {
          local.Read.UserId = export.SearchServiceProvider.UserId;
        }

        if (!Equal(export.Starting.Date, export.StartingHiddenCheck.Date))
        {
          var field1 = GetField(export.Starting, "date");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        if (AsChar(export.DisplayAll.Flag) != AsChar
          (export.DiplayAllHiddenCheck.Flag))
        {
          var field1 = GetField(export.DisplayAll, "flag");

          field1.Error = true;

          ExitState = "CO0000_SEARCH_CRIT_CHGD_PRESS_F2";
        }

        break;
      case "PRINT":
        break;
      case "PRINTRET":
        // ---------------------------------------------
        // User entered this screen from DOCUMENT
        // printing function
        // ---------------------------------------------
        // ------------------------------------------------------
        // printret command comes out of the DOC Printing
        // functionality via a NEXTRAN
        // ------------------------------------------------------
        export.HiddenPrev.Command = global.Command;
        global.Command = "DISPLAY";

        // mjr
        // -----------------------------------------------
        // 11/30/1998
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done
        // now.
        // ------------------------------------------------------------
        UseScCabNextTranGet();

        // mjr
        // ---------------------------------------------------
        // Extract appointment created_timestamp from next_tran_info
        // misc_text_1 = "blah blah;APPT:<text timestamp>;blah blah"
        // ------------------------------------------------------
        local.Position.Count = Find(export.Hidden.MiscText1, "APPT:");

        if (local.Position.Count > 0)
        {
          local.Print.Text50 =
            Substring(export.Hidden.MiscText1, local.Position.Count +
            5, Length(TrimEnd(export.Hidden.MiscText1)) -
            (local.Position.Count + 4));
          local.Position.Count = Find(export.Hidden.MiscText1, ";");

          if (local.Position.Count <= 0)
          {
            local.Print.Text50 =
              Substring(local.Print.Text50, 1, local.Position.Count - 1);
          }

          // mjr---> A valid timestamp has numbers (0123456789), hyphens (-) and
          // periods (.)
          local.Position.Count = Verify(local.Print.Text50, "0123456789-.");

          if (local.Position.Count > 0)
          {
            goto Test1;
          }

          local.Appointment.CreatedTimestamp = Timestamp(local.Print.Text50);
        }

Test1:

        // ------------------------------------------------------
        // Extract filter sp userid, sp's office, role code from
        // next_tran_info misc_text_1 = "blah blah;A1:
        // <SP's Userid><SP's Offc><SP's Role Code>;blah
        //  blah"  (A1:UUUUUUUUOOOORR;)
        // ------------------------------------------------------
        export.SearchCase.Number = export.Hidden.CaseNumber ?? Spaces(10);
        export.SearchCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
        local.Position.Count = Find(export.Hidden.MiscText1, "A1:");

        if (local.Position.Count > 0)
        {
          local.Print.Text50 =
            Substring(export.Hidden.MiscText1, local.Position.Count +
            3, Length(TrimEnd(export.Hidden.MiscText1)) -
            (local.Position.Count + 2));
          local.Position.Count = NextTranInfo.MiscText1_MaxLength;

          if (local.Position.Count >= 15)
          {
            export.SearchServiceProvider.UserId =
              Substring(local.Print.Text50, 1, 8);
            export.SearchOffice.SystemGeneratedId =
              (int)StringToNumber(Substring(
                local.Print.Text50, WorkArea.Text50_MaxLength, 9, 4));
            export.SearchOfficeServiceProvider.RoleCode =
              Substring(local.Print.Text50, 13, 2);

            if (ReadOfficeOfficeServiceProviderServiceProvider())
            {
              MoveOffice(entities.Office, export.SearchOffice);
              MoveServiceProvider(entities.ServiceProvider,
                export.SearchServiceProvider);
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.SearchOfficeServiceProvider);
              MoveOffice(entities.Office, export.SearchHiddenCheckOffice);
              MoveServiceProvider(entities.ServiceProvider,
                export.SearchHiddenCheckServiceProvider);
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.SearchHiddenCheckOfficeServiceProvider);
            }
            else
            {
              export.SearchServiceProvider.UserId = "";
              export.SearchOffice.SystemGeneratedId = 0;
              export.SearchOfficeServiceProvider.RoleCode = "";
            }
          }
        }

        break;
      case "RETCOMP":
        if (local.Pointer.Count == 0)
        {
          export.Comp.SelectChar = "";

          var field1 = GetField(export.Comp, "selectChar");

          field1.Color = "green";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;
          field1.Focused = true;

          if (!IsEmpty(import.ReturnCsePersonsWorkSet.Number))
          {
            export.SearchCsePerson.Number =
              import.ReturnCsePersonsWorkSet.Number;
            export.SearchHiddenCheckCsePerson.Number =
              import.ReturnCsePersonsWorkSet.Number;
            export.SearchCase.Number = import.ReturnCase.Number;
            export.SearchHiddenCheckCase.Number = import.ReturnCase.Number;
            global.Command = "DISPLAY";
          }
        }
        else
        {
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          var field1 =
            GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

          field1.Color = "green";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;
          field1.Focused = false;

          var field2 =
            GetField(export.Export1.Item.GrExportCsePerson, "selectChar");

          field2.Color = "green";
          field2.Highlighting = Highlighting.Underscore;
          field2.Protected = false;
          field2.Focused = true;

          if (!IsEmpty(import.ReturnCsePersonsWorkSet.Number))
          {
            export.Export1.Update.GrExportCsePerson.SelectChar = "";
            export.Export1.Update.GrCsePerson.Number =
              import.ReturnCsePersonsWorkSet.Number;
            export.Export1.Update.GrCase.Number = import.ReturnCase.Number;
            export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
              import.ReturnCase.Number;

            if (ReadCaseCaseRoleCsePerson())
            {
              export.Export1.Update.GrCaseRole.Type1 = entities.CaseRole.Type1;
            }
          }
        }

        break;
      case "RETDOCM":
        if (IsEmpty(import.Ret.Name))
        {
          ExitState = "SP0000_NO_DOC_SEL_FOR_PRINT";

          break;
        }

        export.Export1.Index = local.Pointer.Count - 1;
        export.Export1.CheckSize();

        if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) == 'S')
        {
          // mjr
          // -------------------------------------------
          // 12/03/1998
          // Call to sp_print_amen_docs was removed
          // Added new method to execute Print
          // --------------------------------------------------------
          // mjr
          // -------------------------------------------
          // 01/06/2000
          // NEXT TRAN needs to be cleared before invoking print process
          // --------------------------------------------------------
          export.Hidden.Assign(local.Null1);
          export.Standard.NextTransaction = "DKEY";
          export.Hidden.MiscText2 = "PRINT:" + import.Ret.Name;
          local.BatchTimestampWorkArea.IefTimestamp =
            export.Export1.Item.GrAppointment.CreatedTimestamp;
          UseLeCabConvertTimestamp();
          export.Hidden.MiscText1 = "APPT:" + local
            .BatchTimestampWorkArea.TextTimestamp;

          // --------------------------------------------------------
          // Store filters for use after print process completes
          // --------------------------------------------------------
          export.Hidden.CaseNumber = export.SearchCase.Number;
          export.Hidden.CsePersonNumber = export.SearchCsePerson.Number;
          local.OffcText.Text4 =
            NumberToString(export.SearchOffice.SystemGeneratedId, 12, 4);
          export.Hidden.MiscText1 = TrimEnd(export.Hidden.MiscText1) + ";A1:"
            + export.SearchServiceProvider.UserId + local.OffcText.Text4 + export
            .SearchOfficeServiceProvider.RoleCode;
          UseScCabNextTranPut2();

          // mjr---> DKEY's trancode = SRPD
          //  Can change this to do a READ instead of hardcoding
          global.NextTran = "SRPD PRINT";
        }

        break;
      case "RTFRMLNK":
        // *** to be replaced with rethist command ****
        export.Export1.Index = local.Pointer.Count - 1;
        export.Export1.CheckSize();

        var field =
          GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;

        break;
      case "RETSVPO":
        if (local.Pointer.Count == 0)
        {
          export.ServiceProvider.SelectChar = "";

          var field1 = GetField(export.ServiceProvider, "selectChar");

          field1.Color = "green";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;
          field1.Focused = true;

          if (import.ReturnServiceProvider.SystemGeneratedId != 0)
          {
            MoveServiceProvider(import.ReturnServiceProvider,
              export.SearchServiceProvider);
            MoveOfficeServiceProvider(import.ReturnOfficeServiceProvider,
              export.SearchOfficeServiceProvider);
            MoveOffice(import.ReturnOffice, export.SearchOffice);
            MoveServiceProvider(import.ReturnServiceProvider,
              export.SearchHiddenCheckServiceProvider);
            MoveOfficeServiceProvider(import.ReturnOfficeServiceProvider,
              export.SearchHiddenCheckOfficeServiceProvider);
            MoveOffice(import.ReturnOffice, export.SearchHiddenCheckOffice);
            UseSpCabReadSrvcPrvdrSetName();
            MoveServiceProvider(export.SearchServiceProvider,
              export.SearchHiddenCheckServiceProvider);
            global.Command = "DISPLAY";
          }
        }
        else
        {
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          export.Export1.Update.GrExportServiceProvider.SelectChar = "";

          var field1 =
            GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

          field1.Color = "green";
          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;

          var field2 =
            GetField(export.Export1.Item.GrExportServiceProvider, "selectChar");
            

          field2.Color = "green";
          field2.Highlighting = Highlighting.Underscore;
          field2.Protected = false;
          field2.Focused = true;

          if (import.ReturnServiceProvider.SystemGeneratedId != 0)
          {
            export.Export1.Update.GrExportServiceProvider.SelectChar = "";
            MoveServiceProvider(import.ReturnServiceProvider,
              export.Export1.Update.GrServiceProvider);
            MoveOfficeServiceProvider(import.ReturnOfficeServiceProvider,
              export.Export1.Update.GrOfficeServiceProvider);
            export.Export1.Update.GrOffice.SystemGeneratedId =
              import.ReturnOffice.SystemGeneratedId;
            MoveServiceProvider(import.ReturnServiceProvider,
              export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider);
            MoveOfficeServiceProvider(import.ReturnOfficeServiceProvider,
              export.HiddenCheck.Update.
                GrExportHiddenCheckOfficeServiceProvider);
            export.HiddenCheck.Update.GrExportHiddenCheckOffice.
              SystemGeneratedId = import.ReturnOffice.SystemGeneratedId;
            MoveServiceProvider(export.SearchServiceProvider,
              export.SearchHiddenCheckServiceProvider);
          }
        }

        break;
      case "RETURN":
        if (Equal(export.Hidden.LastTran, "SRPT") || Equal
          (export.Hidden.LastTran, "SRPU"))
        {
          global.NextTran = (export.Hidden.LastTran ?? "") + " " + "XXNEXTXX";

          return;
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "RLCVAL":
        export.Export1.Index = local.Pointer.Count - 1;
        export.Export1.CheckSize();

        switch(TrimEnd(import.PassCode.CodeName))
        {
          case "APPOINTMENT REASON":
            export.HiddenCheck.Index = export.Export1.Index;
            export.HiddenCheck.CheckSize();

            var field1 =
              GetField(export.Export1.Item.GrExportCdvlReason, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            if (AsChar(export.Export1.Item.GrExportCdvlReason.SelectChar) == 'S'
              && !IsEmpty(import.PassCodeValue.Cdvalue))
            {
              export.Export1.Update.GrExportCdvlReason.SelectChar = "";

              var field4 =
                GetField(export.Export1.Item.GrExportCdvlReason, "selectChar");

              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = false;
              field4.Focused = true;

              export.Export1.Update.GrAppointment.ReasonCode =
                import.PassCodeValue.Cdvalue;
              export.HiddenCheck.Update.GrExportHiddenCheckAppointment.
                ReasonCode = import.PassCodeValue.Cdvalue;
            }
            else
            {
              export.Export1.Update.GrExportCdvlReason.SelectChar = "";
            }

            break;
          case "APPOINTMENT RESULT":
            var field2 =
              GetField(export.Export1.Item.GrExportCdvlResult, "selectChar");

            field2.Protected = false;
            field2.Focused = true;

            if (AsChar(export.Export1.Item.GrExportCdvlResult.SelectChar) == 'S'
              && !IsEmpty(import.PassCodeValue.Cdvalue))
            {
              var field4 =
                GetField(export.Export1.Item.GrExportCdvlResult, "selectChar");

              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = false;
              field4.Focused = true;

              export.Export1.Update.GrExportCdvlResult.SelectChar = "";
              export.Export1.Update.GrAppointment.Result =
                import.PassCodeValue.Cdvalue;
              export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Result =
                import.PassCodeValue.Cdvalue;
            }
            else
            {
              export.Export1.Update.GrExportCdvlResult.SelectChar = "";
            }

            break;
          case "APPOINTMENT TYPE":
            var field3 =
              GetField(export.Export1.Item.GrExportCdvlType, "selectChar");

            field3.Protected = false;
            field3.Focused = true;

            if (AsChar(export.Export1.Item.GrExportCdvlType.SelectChar) == 'S'
              && !IsEmpty(import.PassCodeValue.Cdvalue))
            {
              var field4 =
                GetField(export.Export1.Item.GrExportCdvlType, "selectChar");

              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = false;
              field4.Focused = true;

              export.Export1.Update.GrExportCdvlType.SelectChar = "";
              export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Type1 =
                import.PassCodeValue.Cdvalue;
              export.Export1.Update.GrAppointment.Type1 =
                import.PassCodeValue.Cdvalue;
            }
            else
            {
              export.Export1.Update.GrExportCdvlType.SelectChar = "";
            }

            break;
          default:
            break;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "SUCC":
        break;
      case "UPDATE":
        break;
      case "XXNEXTXX":
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();

        // ---------------------------------------------
        // Populate export views from local next_tran_info view read from the 
        // data base
        // Set command to initial command required or ESCAPE
        // ---------------------------------------------
        export.SearchCase.Number = import.Hidden.CaseNumber ?? Spaces(10);
        export.SearchCsePerson.Number = import.Hidden.CsePersonNumber ?? Spaces
          (10);

        // ---------------------------------------------
        // Start of Code (Raju 01/20/97:1035 hrs CST)
        // ---------------------------------------------
        if (Equal(export.Hidden.LastTran, "SRPT") || Equal
          (export.Hidden.LastTran, "SRPU"))
        {
          local.LastTran.SystemGeneratedIdentifier =
            export.Hidden.InfrastructureId.GetValueOrDefault();
          UseOeCabReadInfrastructure();
          export.SearchCase.Number = local.LastTran.CaseNumber ?? Spaces(10);
          export.SearchCsePerson.Number = local.LastTran.CsePersonNumber ?? Spaces
            (10);
          local.Appointment.CreatedTimestamp = local.LastTran.DenormTimestamp;
          export.SearchServiceProvider.UserId = local.Infrastructure.UserId;
          export.SearchHiddenCheckServiceProvider.UserId =
            local.Infrastructure.UserId;
          export.DisplayAll.Flag = "N";
        }

        // ---------------------------------------------
        // End  of Code
        // ---------------------------------------------
        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // ****  Security Check ****
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "HIST") || Equal(global.Command, "LINKPRED") || Equal
      (global.Command, "LIST") || Equal(global.Command, "PRINT") || Equal
      (global.Command, "PRED") || Equal(global.Command, "SUCC") || Equal
      (global.Command, "UPDATE"))
    {
      // **** Check selection for all command that require a selection ****
      if (!IsEmpty(export.ServiceProvider.SelectChar))
      {
        var field = GetField(export.ServiceProvider, "selectChar");

        field.Error = true;

        local.PromptSelCnt.Count = 10000000;
      }

      if (!IsEmpty(export.Comp.SelectChar))
      {
        var field = GetField(export.Comp, "selectChar");

        field.Error = true;

        local.PromptSelCnt.Count += 1000000;
      }

      // **** Validate the correct selection/prompt senario ****
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Export1.Item.GrExportLineSelect.SelectChar))
        {
          var field =
            GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

          field.Error = true;

          if (local.LineSelCnt.Count == 0)
          {
            local.LineSelCnt.Count = 100000;
            local.Pointer.Count = export.Export1.Index + 1;
          }
          else if (local.LineSelCnt.Count == 100000)
          {
            local.LineSelCnt.Count = 200000;
            local.PointerSucc.Count = export.Export1.Index + 1;
          }
          else if (local.LineSelCnt.Count == 200000)
          {
            local.LineSelCnt.Count = 300000;
          }
        }

        if (!IsEmpty(export.Export1.Item.GrExportCsePerson.SelectChar))
        {
          var field =
            GetField(export.Export1.Item.GrExportCsePerson, "selectChar");

          field.Error = true;

          if (local.PcsePerCnt.Count == 0)
          {
            local.PcsePerCnt.Count = 10000;
            local.Pointer.Count = export.Export1.Index + 1;
          }
          else if (local.PcsePerCnt.Count == 10000)
          {
            local.PcsePerCnt.Count = 20000;
          }
        }

        if (!IsEmpty(export.Export1.Item.GrExportCdvlReason.SelectChar))
        {
          var field =
            GetField(export.Export1.Item.GrExportCdvlReason, "selectChar");

          field.Error = true;

          if (local.PresCnt.Count == 0)
          {
            local.PrsnCnt.Count = 1000;
            local.Pointer.Count = export.Export1.Index + 1;
          }
          else if (local.PcsePerCnt.Count == 1000)
          {
            local.PrsnCnt.Count = 2000;
          }
        }

        if (!IsEmpty(export.Export1.Item.GrExportCdvlType.SelectChar))
        {
          var field =
            GetField(export.Export1.Item.GrExportCdvlType, "selectChar");

          field.Error = true;

          if (local.PtypCnt.Count == 0)
          {
            local.PtypCnt.Count = 100;
            local.Pointer.Count = export.Export1.Index + 1;
          }
          else if (local.PtypCnt.Count == 100)
          {
            local.PtypCnt.Count = 200;
          }
        }

        if (!IsEmpty(export.Export1.Item.GrExportCdvlResult.SelectChar))
        {
          var field =
            GetField(export.Export1.Item.GrExportCdvlResult, "selectChar");

          field.Error = true;

          if (local.PrsnCnt.Count == 0)
          {
            local.PrsnCnt.Count = 10;
            local.Pointer.Count = export.Export1.Index + 1;
          }
          else if (local.PrsnCnt.Count == 10)
          {
            local.PrsnCnt.Count = 20;
          }
        }

        if (!IsEmpty(export.Export1.Item.GrExportServiceProvider.SelectChar))
        {
          var field =
            GetField(export.Export1.Item.GrExportServiceProvider, "selectChar");
            

          field.Error = true;

          if (local.PspCnt.Count == 0)
          {
            local.PspCnt.Count = 1;
            local.Pointer.Count = export.Export1.Index + 1;
          }
          else if (local.PspCnt.Count == 1)
          {
            local.PcsePerCnt.Count = 2;
          }
        }
      }

      export.Export1.CheckIndex();
      local.PromptSelCnt.Count = local.LineSelCnt.Count + local
        .PcsePerCnt.Count + local.PresCnt.Count + local.PrsnCnt.Count + local
        .PspCnt.Count + local.PtypCnt.Count + local.PromptSelCnt.Count;

      export.Export1.Index = local.Pointer.Count - 1;
      export.Export1.CheckSize();

      switch(local.PromptSelCnt.Count)
      {
        case 0:
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        case 1:
          if (Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
          else
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }

          break;
        case 10:
          if (Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
          else
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }

          break;
        case 100:
          if (Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
          else
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }

          break;
        case 1000:
          if (Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
          else
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }

          break;
        case 10000:
          if (Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
          else
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }

          break;
        case 100000:
          if (Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            // *** CQ#8968 Changes Begin Here ***
            // *** Added the Else-If statement below ***
          }
          else if (Equal(global.Command, "LINKPRED"))
          {
            ExitState = "ACO_NE0000_MUST_SELECT_2";

            // *** CQ#8968 Changes End   Here ***
          }

          break;
        case 100001:
          if (!Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }
          else if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) ==
            'S' && AsChar
            (export.Export1.Item.GrExportServiceProvider.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_TO_SVPO";
          }
          else if (IsEmpty(export.Export1.Item.GrExportLineSelect.SelectChar) ||
            IsEmpty(export.Export1.Item.GrExportServiceProvider.SelectChar))
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }
          else
          {
            if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) == 'S'
              )
            {
              var field =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            if (AsChar(export.Export1.Item.GrExportServiceProvider.SelectChar) ==
              'S')
            {
              var field =
                GetField(export.Export1.Item.GrExportServiceProvider,
                "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }
          }

          break;
        case 100010:
          if (!Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }
          else if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) ==
            'S' && AsChar
            (export.Export1.Item.GrExportCdvlResult.SelectChar) == 'S')
          {
            export.PassCode.CodeName = "APPOINTMENT RESULT";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else if (IsEmpty(export.Export1.Item.GrExportLineSelect.SelectChar) ||
            IsEmpty(export.Export1.Item.GrExportCdvlResult.SelectChar))
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }
          else
          {
            if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) == 'S'
              )
            {
              var field =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            if (AsChar(export.Export1.Item.GrExportCdvlResult.SelectChar) == 'S'
              )
            {
              var field =
                GetField(export.Export1.Item.GrExportCdvlResult, "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }
          }

          break;
        case 100100:
          if (!Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }
          else if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) ==
            'S' && AsChar(export.Export1.Item.GrExportCdvlType.SelectChar) == 'S'
            )
          {
            export.PassCode.CodeName = "APPOINTMENT TYPE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else if (IsEmpty(export.Export1.Item.GrExportLineSelect.SelectChar) ||
            IsEmpty(export.Export1.Item.GrExportCdvlType.SelectChar))
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }
          else
          {
            if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) == 'S'
              )
            {
              var field =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            if (AsChar(export.Export1.Item.GrExportCdvlType.SelectChar) == 'S')
            {
              var field =
                GetField(export.Export1.Item.GrExportCdvlType, "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }
          }

          break;
        case 101000:
          if (!Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }
          else if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) ==
            'S' && AsChar
            (export.Export1.Item.GrExportCdvlReason.SelectChar) == 'S')
          {
            export.PassCode.CodeName = "APPOINTMENT REASON";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else if (IsEmpty(export.Export1.Item.GrExportLineSelect.SelectChar) ||
            IsEmpty(export.Export1.Item.GrExportCdvlReason.SelectChar))
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }
          else
          {
            if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) == 'S'
              )
            {
              var field =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            if (AsChar(export.Export1.Item.GrExportCdvlReason.SelectChar) == 'S'
              )
            {
              var field =
                GetField(export.Export1.Item.GrExportCdvlReason, "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }
          }

          break;
        case 110000:
          if (!Equal(global.Command, "LIST"))
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }
          else if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) ==
            'S' && AsChar(export.Export1.Item.GrExportCsePerson.SelectChar) == 'S'
            )
          {
            ExitState = "ECO_LNK_TO_SI_COMP_CASE_COMP";
          }
          else if (IsEmpty(export.Export1.Item.GrExportLineSelect.SelectChar) ||
            IsEmpty(export.Export1.Item.GrExportCsePerson.SelectChar))
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }
          else
          {
            if (AsChar(export.Export1.Item.GrExportLineSelect.SelectChar) == 'S'
              )
            {
              var field =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            if (AsChar(export.Export1.Item.GrExportCsePerson.SelectChar) == 'S')
            {
              var field =
                GetField(export.Export1.Item.GrExportCsePerson, "selectChar");

              field.Protected = false;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }
          }

          break;
        case 200000:
          if (!Equal(global.Command, "LINKPRED"))
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
          }

          break;
        case 1000000:
          if (AsChar(export.Comp.SelectChar) == 'S' && Equal
            (global.Command, "LIST"))
          {
            ExitState = "ECO_LNK_TO_SI_COMP_CASE_COMP";

            goto Test2;
          }

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          break;
        case 10000000:
          if (AsChar(export.ServiceProvider.SelectChar) == 'S' && Equal
            (global.Command, "LIST"))
          {
            ExitState = "ECO_LNK_TO_SVPO";

            goto Test2;
          }

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          break;
        default:
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          if (Equal(global.Command, "LINKPRED"))
          {
            ExitState = "ACO_NE0000_MUST_SELECT_2";
          }

          break;
      }
    }

Test2:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      switch(TrimEnd(global.Command))
      {
        case "ADD":
          // ---------------------------------------------
          // Start of code - Raju 01/14/97:1140 hrs CST
          // ---------------------------------------------
          for(export.Exp3.Index = 0; export.Exp3.Index < export.Exp3.Count; ++
            export.Exp3.Index)
          {
            if (!export.Exp3.CheckSize())
            {
              break;
            }

            MoveAppointment1(local.Blank, export.Exp3.Update.Exp3LastReadHidden);
              
          }

          export.Exp3.CheckIndex();

          // ---------------------------------------------
          // End   of code
          // ---------------------------------------------
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          export.HiddenCheck.Index = export.Export1.Index;
          export.HiddenCheck.CheckSize();

          var field1 =
            GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

          field1.Color = "red";
          field1.Intensity = Intensity.High;
          field1.Protected = false;

          if (Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
            export.HiddenCheck.Item.GrExportHiddenCheckAppointment.
              CreatedTimestamp) && !
            Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
            local.Initialized.Timestamp))
          {
            ExitState = "CO0000_APPOINTMENT_AE";

            goto Test6;
          }
          else if (!Equal(export.HiddenCheck.Item.
            GrExportHiddenCheckAppointment.CreatedTimestamp,
            local.Initialized.Timestamp))
          {
            ExitState = "SP0000_ADD_FROM_BLANK_ONLY";

            goto Test6;
          }

          if (IsEmpty(export.Export1.Item.GrOfficeServiceProvider.RoleCode) || export
            .Export1.Item.GrOffice.SystemGeneratedId == 0 || IsEmpty
            (export.Export1.Item.GrServiceProvider.UserId))
          {
            // **** Default to SP's Userid sign-on - if SP is in more than one *
            // ***
            // **** office or has more than one role - set a warning exitstate *
            // ***
            local.LineSelCnt.Count = 0;

            foreach(var item in ReadOfficeServiceProviderOfficeServiceProvider())
              
            {
              ++local.LineSelCnt.Count;

              if (local.LineSelCnt.Count > 1)
              {
                local.LineSelCnt.Count = 1;

                var field9 =
                  GetField(export.Export1.Item.GrServiceProvider, "userId");

                field9.Error = true;

                var field10 =
                  GetField(export.Export1.Item.GrOfficeServiceProvider,
                  "roleCode");

                field10.Error = true;

                var field11 =
                  GetField(export.Export1.Item.GrOffice, "systemGeneratedId");

                field11.Error = true;

                var field12 =
                  GetField(export.Export1.Item.GrExportServiceProvider,
                  "selectChar");

                field12.Intensity = Intensity.High;
                field12.Protected = false;
                field12.Focused = true;

                ExitState = "SP0000_SP_MULTI_OFFC_ROLES";

                goto Test3;
              }

              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.Export1.Update.GrOfficeServiceProvider);
              MoveServiceProvider(entities.ServiceProvider,
                export.Export1.Update.GrServiceProvider);
              export.Export1.Update.GrOffice.SystemGeneratedId =
                entities.Office.SystemGeneratedId;
            }

            if (local.LineSelCnt.Count == 1)
            {
              goto Test3;
            }

            local.LineSelCnt.Count = 1;

            var field5 =
              GetField(export.Export1.Item.GrServiceProvider, "userId");

            field5.Error = true;

            var field6 =
              GetField(export.Export1.Item.GrOfficeServiceProvider, "roleCode");
              

            field6.Error = true;

            var field7 =
              GetField(export.Export1.Item.GrOffice, "systemGeneratedId");

            field7.Error = true;

            var field8 =
              GetField(export.Export1.Item.GrExportServiceProvider, "selectChar");
              

            field8.Intensity = Intensity.High;
            field8.Protected = false;
            field8.Focused = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

Test3:

          if (!IsEmpty(export.Export1.Item.GrAppointment.Result))
          {
            var field = GetField(export.Export1.Item.GrAppointment, "result");

            field.Error = true;

            ExitState = "SP0000_CANT_ADD_APPT_W_RESULT";
          }

          if (IsEmpty(export.Export1.Item.GrAppointment.ReasonCode))
          {
            if (local.LineSelCnt.Count > 0)
            {
            }
            else
            {
              var field5 =
                GetField(export.Export1.Item.GrExportCdvlReason, "selectChar");

              field5.Intensity = Intensity.High;
              field5.Protected = false;
              field5.Focused = true;

              local.LineSelCnt.Count = 1;
            }

            var field =
              GetField(export.Export1.Item.GrAppointment, "reasonCode");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          if (IsEmpty(export.Export1.Item.GrAppointment.Type1))
          {
            if (local.LineSelCnt.Count > 0)
            {
            }
            else
            {
              var field5 =
                GetField(export.Export1.Item.GrExportCdvlType, "selectChar");

              field5.Intensity = Intensity.High;
              field5.Protected = false;
              field5.Focused = true;

              local.LineSelCnt.Count = 1;
            }

            var field = GetField(export.Export1.Item.GrAppointment, "type1");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          if (export.Export1.Item.GrAppointment.Time == local.Initialized.Time)
          {
            var field = GetField(export.Export1.Item.GrAppointment, "time");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          if (export.Export1.Item.GrAppointment.Time < new
            TimeSpan(1, 0, 0) || export.Export1.Item.GrAppointment.Time >= new
            TimeSpan(13, 0, 0))
          {
            var field = GetField(export.Export1.Item.GrAppointment, "time");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          switch(AsChar(export.Export1.Item.GrExportAmPmInd.SelectChar))
          {
            case 'A':
              if (export.Export1.Item.GrAppointment.Time < new
                TimeSpan(6, 0, 0))
              {
                var field = GetField(export.Export1.Item.GrAppointment, "time");

                field.Error = true;

                ExitState = "SP0000_APPOINTMENT_TIME_OWH";
              }
              else if (export.Export1.Item.GrAppointment.Time > new
                TimeSpan(12, 0, 0))
              {
                var field7 =
                  GetField(export.Export1.Item.GrAppointment, "time");

                field7.Error = true;

                var field8 =
                  GetField(export.Export1.Item.GrExportAmPmInd, "selectChar");

                field8.Error = true;

                ExitState = "SP0000_WRONG_AM_PM_TIME_COMBO";
              }

              break;
            case 'P':
              if (export.Export1.Item.GrAppointment.Time > new
                TimeSpan(12, 0, 0) && export.Export1.Item.GrAppointment.Time < new
                TimeSpan(13, 0, 0))
              {
              }
              else if (export.Export1.Item.GrAppointment.Time > new
                TimeSpan(6, 0, 0))
              {
                var field = GetField(export.Export1.Item.GrAppointment, "time");

                field.Error = true;

                ExitState = "SP0000_APPOINTMENT_TIME_OWH";
              }

              break;
            case ' ':
              var field5 =
                GetField(export.Export1.Item.GrExportAmPmInd, "selectChar");

              field5.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              break;
            default:
              var field6 =
                GetField(export.Export1.Item.GrExportAmPmInd, "selectChar");

              field6.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              break;
          }

          if (Equal(export.Export1.Item.GrAppointment.Date,
            local.Initialized.Date))
          {
            var field = GetField(export.Export1.Item.GrAppointment, "date");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          if (IsEmpty(export.Export1.Item.GrCsePerson.Number) || IsEmpty
            (export.Export1.Item.GrCase.Number))
          {
            if (local.LineSelCnt.Count > 0)
            {
            }
            else
            {
              var field =
                GetField(export.Export1.Item.GrExportCsePerson, "selectChar");

              field.Intensity = Intensity.High;
              field.Protected = false;
              field.Focused = true;

              local.LineSelCnt.Count = 1;
            }

            var field5 = GetField(export.Export1.Item.GrCase, "number");

            field5.Error = true;

            var field6 = GetField(export.Export1.Item.GrCsePerson, "number");

            field6.Error = true;

            var field7 =
              GetField(export.Export1.Item.GrExportCsePerson, "selectChar");

            field7.Protected = false;
            field7.Focused = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
          else
          {
            UseCabZeroFillNumber3();
            UseCabZeroFillNumber4();
            export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
              export.Export1.Item.GrCase.Number;
            MoveCsePerson(export.Export1.Item.GrCsePerson,
              export.HiddenCheck.Update.GrExportHiddenCheckCsePerson);
          }

          if (Lt(export.Export1.Item.GrAppointment.Date, local.Current.Date))
          {
            var field = GetField(export.Export1.Item.GrAppointment, "date");

            field.Error = true;

            ExitState = "SP0000_APPOINTMENT_IN_PAST";
          }

          // **** Check for day of week - schedule appointment can only be Mon. 
          // thru Fri.  ****
          local.Weekday.Text10 =
            DayOfWeek(export.Export1.Item.GrAppointment.Date);

          if (Equal(local.Weekday.Text10, "SATURDAY") || Equal
            (local.Weekday.Text10, "SUNDAY"))
          {
            var field = GetField(export.Export1.Item.GrAppointment, "date");

            field.Error = true;

            ExitState = "SP0000_APPOINTMENT_ON_WEEKEND";
          }

          switch(AsChar(export.Export1.Item.GrExportAmPmInd.SelectChar))
          {
            case 'A':
              local.Start.Time = export.Export1.Item.GrAppointment.Time - new
                TimeSpan(0, 59, 0);

              break;
            case 'P':
              if (export.Export1.Item.GrAppointment.Time > new
                TimeSpan(12, 0, 0))
              {
                local.Start.Time = export.Export1.Item.GrAppointment.Time - new
                  TimeSpan(0, 59, 0);
              }
              else
              {
                local.Start.Time = export.Export1.Item.GrAppointment.Time + new
                  TimeSpan(12, 0, 0) - new TimeSpan(0, 59, 0);
              }

              break;
            default:
              break;
          }

          local.Start.Date = export.Export1.Item.GrAppointment.Date;
          local.End.Time = local.Start.Time + new TimeSpan(0, 118, 0);

          foreach(var item in ReadAppointment5())
          {
            if (Equal(entities.Successor.Date,
              export.Export1.Item.GrAppointment.Date) && entities
              .Successor.Time >= local.Start.Time && entities
              .Successor.Time <= local.End.Time)
            {
              var field5 = GetField(export.Export1.Item.GrAppointment, "date");

              field5.Error = true;

              var field6 = GetField(export.Export1.Item.GrAppointment, "time");

              field6.Error = true;

              ExitState = "SP0000_PRIOR_APPOINTMENT_EXISTS";

              break;
            }
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            goto Test6;
          }

          local.Code.CodeName = "APPOINTMENT REASON";
          local.CodeValue.Cdvalue =
            export.Export1.Item.GrAppointment.ReasonCode;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field5 =
                GetField(export.Export1.Item.GrExportCdvlReason, "selectChar");

              field5.Error = true;

              var field6 =
                GetField(export.Export1.Item.GrAppointment, "reasonCode");

              field6.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              goto Test6;
            }
          }

          local.Code.CodeName = "APPOINTMENT TYPE";
          local.CodeValue.Cdvalue = export.Export1.Item.GrAppointment.Type1;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field5 =
                GetField(export.Export1.Item.GrExportCdvlType, "selectChar");

              field5.Error = true;

              var field6 = GetField(export.Export1.Item.GrAppointment, "type1");

              field6.Error = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              goto Test6;
            }
          }

          local.Pass.GrLocalPassAppointment.Assign(
            export.Export1.Item.GrAppointment);
          local.Pass.PassApptAmPmInd.SelectChar =
            export.Export1.Item.GrExportAmPmInd.SelectChar;
          local.Pass.GrLocalPassCase.Number = export.Export1.Item.GrCase.Number;
          MoveCsePerson(export.Export1.Item.GrCsePerson,
            local.Pass.GrLocalPassCsePerson);
          local.Pass.GrLocalPassOffice.SystemGeneratedId =
            export.Export1.Item.GrOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(export.Export1.Item.GrOfficeServiceProvider,
            local.Pass.GrLocalPassOfficeServiceProvider);
          MoveServiceProvider(export.Export1.Item.GrServiceProvider,
            local.Pass.GrLocalPassServiceProvider);
          UseSpCabAddAppointment();
          export.Export1.Update.GrAppointment.CreatedTimestamp =
            local.Appointment.CreatedTimestamp;
          export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Assign(
            export.Export1.Item.GrAppointment);
          export.HiddenCheck.Update.GrExportHiddenAmPmInd.SelectChar =
            export.Export1.Item.GrExportAmPmInd.SelectChar;
          export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
            export.Export1.Item.GrCase.Number;
          MoveCsePerson(export.Export1.Item.GrCsePerson,
            export.HiddenCheck.Update.GrExportHiddenCheckCsePerson);
          export.HiddenCheck.Update.GrExportHiddenCheckOffice.
            SystemGeneratedId = export.Export1.Item.GrOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(export.Export1.Item.GrOfficeServiceProvider,
            export.HiddenCheck.Update.GrExportHiddenCheckOfficeServiceProvider);
            
          MoveServiceProvider(export.Export1.Item.GrServiceProvider,
            export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider);
          export.Export1.Update.GrExportLineSelect.SelectChar = "";

          var field2 =
            GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

          field2.Protected = false;
          field2.Focused = true;

          if (!IsExitState("ACO_NI0000_ADD_SUCCESSFUL"))
          {
            break;
          }

          export.Exp3.Index = export.Export1.Index;
          export.Exp3.CheckSize();

          MoveAppointment1(export.Export1.Item.GrAppointment,
            export.Exp3.Update.Exp3LastReadHidden);
          local.Infrastructure.EventId = 5;
          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.UserId = "AMEN";
          local.Infrastructure.BusinessObjectCd = "CAS";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.ReferenceDate =
            export.Export1.Item.GrAppointment.Date;
          local.Date.Date = local.Infrastructure.ReferenceDate;
          local.DetailText10.Text10 = UseCabConvertDate2String();
          local.RaiseEventFlag.Text1 = "N";

          if (AsChar(export.Export1.Item.GrAppointment.ReasonCode) == 'N' && AsChar
            (export.Export1.Item.GrAppointment.Type1) == 'A')
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.ReasonCode = "APPTARSNENF";
            local.Infrastructure.Detail = "Active Appointment set on : ";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText10.Text10;
            local.DetailText30.Text30 = " to discuss Enforcement";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
          }
          else if (AsChar(export.Export1.Item.GrAppointment.ReasonCode) == 'L'
            && AsChar(export.Export1.Item.GrAppointment.Type1) == 'A')
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.ReasonCode = "APPTARSNLOC";
            local.Infrastructure.Detail = "Active Appointment set on : ";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText10.Text10;
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + " to discuss Locating Absent Parent";
              
          }
          else if (AsChar(export.Export1.Item.GrAppointment.ReasonCode) == 'E'
            && AsChar(export.Export1.Item.GrAppointment.Type1) == 'A')
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.ReasonCode = "APPTARSNOE";
            local.Infrastructure.Detail = "Active Appointment set on : ";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText10.Text10;
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + " to discuss Obligation Establishment";
              
          }
          else if (AsChar(export.Export1.Item.GrAppointment.ReasonCode) == 'P'
            && AsChar(export.Export1.Item.GrAppointment.Type1) == 'A')
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.ReasonCode = "APPTARSNPAT";
            local.Infrastructure.Detail = "Active Appointment set on : ";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText10.Text10;
            local.DetailText30.Text30 = " to discuss Paternity of Child";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
          }
          else if (AsChar(export.Export1.Item.GrAppointment.ReasonCode) == 'L'
            && AsChar(export.Export1.Item.GrAppointment.Type1) == 'N')
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.ReasonCode = "APPTNRSNLOC";
            local.Infrastructure.Detail = "New Appointment set on : ";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText10.Text10;
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + " to discuss Locating Absent Parent";
              
          }
          else if (AsChar(export.Export1.Item.GrAppointment.Result) == 'P' && AsChar
            (export.Export1.Item.GrAppointment.Type1) == 'N')
          {
            local.RaiseEventFlag.Text1 = "Y";
            local.Infrastructure.ReasonCode = "APPTNRSNPAT";
            local.Infrastructure.Detail = "New Appointment set on : ";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText10.Text10;
            local.DetailText30.Text30 = " to discuss Paternity of Child";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
          }

          if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
          {
            local.Inf1.Number = export.Export1.Item.GrCase.Number;
            local.Inf2.Number = export.Export1.Item.GrCsePerson.Number;
            local.Appointment.CreatedTimestamp =
              export.Export1.Item.GrAppointment.CreatedTimestamp;
            UseSpAmenRaiseEvent();

            if (IsExitState("ACO_NI0000_ADD_SUCCESSFUL"))
            {
            }
            else
            {
              UseEabRollbackCics();

              goto Test6;
            }
          }

          break;
        case "UPDATE":
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          export.HiddenCheck.Index = export.Export1.Index;
          export.HiddenCheck.CheckSize();

          var field3 =
            GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

          field3.Color = "red";
          field3.Intensity = Intensity.High;
          field3.Protected = false;

          if (Equal(export.Export1.Item.GrAppointment.Date,
            export.HiddenCheck.Item.GrExportHiddenCheckAppointment.Date))
          {
          }
          else if (Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
            local.Initialized.Timestamp) || !
            Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
            export.HiddenCheck.Item.GrExportHiddenCheckAppointment.
              CreatedTimestamp))
          {
            ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

            goto Test6;
          }

          local.Code.CodeName = "APPOINTMENT RESULT";
          local.CodeValue.Cdvalue = export.Export1.Item.GrAppointment.Result;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) != 'Y')
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.GrExportCdvlResult.SelectChar = "S";

              var field5 =
                GetField(export.Export1.Item.GrAppointment, "result");

              field5.Error = true;

              var field6 =
                GetField(export.Export1.Item.GrExportCdvlResult, "selectChar");

              field6.Intensity = Intensity.High;
              field6.Protected = false;
              field6.Focused = true;

              ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";

              goto Test6;
            }
          }

          UseSpCabUpdateAppointment();

          if (IsExitState("ACO_NI0000_UPDATE_SUCCESSFUL"))
          {
            export.Export1.Update.GrExportLineSelect.SelectChar = "";

            var field =
              GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

            field.Protected = false;
            field.Focused = true;

            export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Assign(
              export.Export1.Item.GrAppointment);
          }

          break;
        case "DELETE":
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          export.HiddenCheck.Index = export.Export1.Index;
          export.HiddenCheck.CheckSize();

          var field4 =
            GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

          field4.Color = "red";
          field4.Intensity = Intensity.High;
          field4.Protected = false;

          if (Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
            local.Initialized.Timestamp) || !
            Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
            export.HiddenCheck.Item.GrExportHiddenCheckAppointment.
              CreatedTimestamp))
          {
            ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

            goto Test6;
          }

          // **** Cannot delete If a Infrastructure record is associated to this
          // appointment  ****
          if (ReadInfrastructureAppointment())
          {
            ExitState = "SP0000_CANNOT_DEL_APPT";

            goto Test6;
          }
          else
          {
            // **** Continue processing  ****
          }

          UseSpCabDeleteAppointment();

          if (IsExitState("ACO_NI0000_DELETE_SUCCESSFUL"))
          {
            export.Export1.Update.GrExportLineSelect.SelectChar = "";

            var field =
              GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field =
              GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

            field.Error = true;
          }

          break;
        case "LINKPRED":
          local.Counter.Count = 0;

          do
          {
            // **** Assign the selected Appointments to local_holding_(1 or 2)  
            // ****
            ++local.Counter.Count;

            switch(local.Counter.Count)
            {
              case 1:
                export.Export1.Index = local.Pointer.Count - 1;
                export.Export1.CheckSize();

                export.HiddenCheck.Index = export.Export1.Index;
                export.HiddenCheck.CheckSize();

                local.Holding1Appointment.Assign(
                  export.Export1.Item.GrAppointment);
                local.PredSuccCheck.GrLocalPredSuccCheckCsePerson.Number =
                  export.Export1.Item.GrCsePerson.Number;
                local.PredSuccCheck.GrLocalPredSuccCheckCase.Number =
                  export.Export1.Item.GrCase.Number;

                break;
              case 2:
                export.Export1.Index = local.PointerSucc.Count - 1;
                export.Export1.CheckSize();

                export.HiddenCheck.Index = export.Export1.Index;
                export.HiddenCheck.CheckSize();

                local.Holding2Appointment.Assign(
                  export.Export1.Item.GrAppointment);

                break;
              default:
                break;
            }

            if (Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
              local.Initialized.Timestamp) || !
              Equal(export.Export1.Item.GrCsePerson.Number,
              export.HiddenCheck.Item.GrExportHiddenCheckCsePerson.Number) || !
              Equal(export.Export1.Item.GrCase.Number,
              export.HiddenCheck.Item.GrExportHiddenCheckCase.Number) || !
              Equal(export.Export1.Item.GrServiceProvider.UserId,
              export.HiddenCheck.Item.GrExportHiddenCheckServiceProvider.
                UserId) || !
              Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
              export.HiddenCheck.Item.GrExportHiddenCheckAppointment.
                CreatedTimestamp))
            {
              ExitState = "CO0000_INFO_CHNGD_REDISPLAY";

              goto Test4;
            }
          }
          while(local.Counter.Count != 2);

          // **** Validate same case and same person  ****
          if (!Equal(local.PredSuccCheck.GrLocalPredSuccCheckCase.Number,
            export.Export1.Item.GrCase.Number))
          {
            ExitState = "SP0000_CASE_NBR_NOT_SAME";

            break;
          }

          if (!Equal(local.PredSuccCheck.GrLocalPredSuccCheckCsePerson.Number,
            export.Export1.Item.GrCsePerson.Number))
          {
            ExitState = "SP0000_PERSON_NBR_NOT_SAME";

            break;
          }

          // **** Determine which appointment is the predecessor and which one 
          // is the successor  ****
          if (Lt(local.Holding1Appointment.CreatedTimestamp,
            local.Holding2Appointment.CreatedTimestamp))
          {
            local.Predecessor.Assign(local.Holding1Appointment);
            local.PointerPred.Count = local.Pointer.Count;
            local.Successor.Assign(local.Holding2Appointment);

            // **** Local_pointer_succ is set above  ****
          }
          else
          {
            local.Predecessor.Assign(local.Holding2Appointment);
            local.PointerPred.Count = local.PointerSucc.Count;
            local.Successor.Assign(local.Holding1Appointment);
            local.PointerSucc.Count = local.Pointer.Count;
          }

          // **** The predecessor has to be a closed appointment with a results 
          // code  ****
          if (IsEmpty(local.Predecessor.Result))
          {
            export.Export1.Index = local.PointerSucc.Count - 1;
            export.Export1.CheckSize();

            var field5 =
              GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

            field5.Protected = false;

            export.Export1.Index = local.PointerPred.Count - 1;
            export.Export1.CheckSize();

            var field6 =
              GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

            field6.Error = true;

            ExitState = "SP0000_PRED_APPT_NOT_CLOSED";

            goto Test6;
          }

          // **** Determine if the predecessor already has a successor  ****
          if (ReadAppointment1())
          {
            if (ReadAppointment4())
            {
              export.Export1.Index = local.PointerSucc.Count - 1;
              export.Export1.CheckSize();

              var field5 =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field5.Protected = false;

              export.Export1.Index = local.PointerPred.Count - 1;
              export.Export1.CheckSize();

              var field6 =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field6.Error = true;

              ExitState = "SP0000_PRED_APPT_HAS_SUCC_APPT";

              goto Test6;
            }
          }

          // **** Determine if the successor already has a predecessor   ****
          if (ReadAppointment3())
          {
            if (ReadAppointment2())
            {
              export.Export1.Index = local.PointerPred.Count - 1;
              export.Export1.CheckSize();

              var field5 =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field5.Protected = false;

              export.Export1.Index = local.PointerSucc.Count - 1;
              export.Export1.CheckSize();

              var field6 =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field6.Error = true;

              ExitState = "SP0000_SUCC_APPT_HAS_PRED_APPT";

              goto Test6;
            }
          }

          UseSpLinkPredSuccAppointments();

          if (IsExitState("SP0000_PRED_LINKED_SUCCESSFUL"))
          {
            export.Export1.Index = local.PointerSucc.Count - 1;
            export.Export1.CheckSize();

            var field5 =
              GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

            field5.Protected = false;

            export.Export1.Update.GrExportLineSelect.SelectChar = "";

            export.Export1.Index = local.PointerPred.Count - 1;
            export.Export1.CheckSize();

            var field6 =
              GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

            field6.Protected = false;

            export.Export1.Update.GrExportLineSelect.SelectChar = "";
          }

          break;
        case "PRED":
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          export.HiddenCheck.Index = export.Export1.Index;
          export.HiddenCheck.CheckSize();

          export.HiddenPrev.Command = global.Command;
          local.PageKeys.Assign(export.Export1.Item.GrAppointment);
          local.Pass.GrLocalPassAppointment.Assign(
            export.Export1.Item.GrAppointment);
          local.Pass.GrLocalPassCase.Number = export.Export1.Item.GrCase.Number;
          MoveCsePerson(export.Export1.Item.GrCsePerson,
            local.Pass.GrLocalPassCsePerson);
          local.Pass.GrLocalPassOffice.SystemGeneratedId =
            export.Export1.Item.GrOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(export.Export1.Item.GrOfficeServiceProvider,
            local.Pass.GrLocalPassOfficeServiceProvider);
          MoveServiceProvider(export.Export1.Item.GrServiceProvider,
            local.Pass.GrLocalPassServiceProvider);

          // ************************************************************
          //   A display resets the page key info to spaces, and new
          //   page key info is recovered via the display
          // ************************************************************
          for(export.HiddenPageKeys.Index = 0; export.HiddenPageKeys.Index < Export
            .HiddenPageKeysGroup.Capacity; ++export.HiddenPageKeys.Index)
          {
            if (!export.HiddenPageKeys.CheckSize())
            {
              break;
            }

            MoveAppointment2(local.Holding2Appointment,
              export.HiddenPageKeys.Update.GexportHiddenPageKeys);
          }

          export.HiddenPageKeys.CheckIndex();
          local.PageCount.PageNumber = 1;

          export.HiddenPageKeys.Index = 0;
          export.HiddenPageKeys.CheckSize();

          export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
            local.PageKeys);
          UseSpCabReadPredecessorAppts2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (export.Export1.IsFull)
            {
              export.HiddenPageKeys.Index = local.PageCount.PageNumber - 1;
              export.HiddenPageKeys.CheckSize();

              export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
                local.PageKeys);
              export.HiddenPageCount.PageNumber = local.PageCount.PageNumber - 1
                ;
            }
            else
            {
              export.HiddenPageCount.PageNumber = local.PageCount.PageNumber;
            }

            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }

          break;
        case "SUCC":
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          export.HiddenCheck.Index = export.Export1.Index;
          export.HiddenCheck.CheckSize();

          local.PageKeys.Assign(export.Export1.Item.GrAppointment);
          local.Pass.GrLocalPassAppointment.Assign(
            export.Export1.Item.GrAppointment);
          local.Pass.GrLocalPassCase.Number = export.Export1.Item.GrCase.Number;
          MoveCsePerson(export.Export1.Item.GrCsePerson,
            local.Pass.GrLocalPassCsePerson);
          local.Pass.GrLocalPassOffice.SystemGeneratedId =
            export.Export1.Item.GrOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(export.Export1.Item.GrOfficeServiceProvider,
            local.Pass.GrLocalPassOfficeServiceProvider);
          MoveServiceProvider(export.Export1.Item.GrServiceProvider,
            local.Pass.GrLocalPassServiceProvider);

          // ************************************************************
          //   A display resets the page key info to spaces, and new
          //   page key info is recovered via the display
          // ************************************************************
          for(export.HiddenPageKeys.Index = 0; export.HiddenPageKeys.Index < Export
            .HiddenPageKeysGroup.Capacity; ++export.HiddenPageKeys.Index)
          {
            if (!export.HiddenPageKeys.CheckSize())
            {
              break;
            }

            export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
              local.Pass.GrLocalPassAppointment);
          }

          export.HiddenPageKeys.CheckIndex();
          local.PageCount.PageNumber = 1;

          export.HiddenPageKeys.Index = 0;
          export.HiddenPageKeys.CheckSize();

          export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
            local.PageKeys);
          UseSpCabShowSuccAppts2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.HiddenPrev.Command = global.Command;

            if (export.Export1.IsFull)
            {
              export.HiddenPageKeys.Index = local.PageCount.PageNumber - 1;
              export.HiddenPageKeys.CheckSize();

              export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
                local.PageKeys);
              export.HiddenPageCount.PageNumber = local.PageCount.PageNumber - 1
                ;
            }
            else
            {
              export.HiddenPageCount.PageNumber = local.PageCount.PageNumber;
            }

            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }

          break;
        case "HIST":
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          export.HiddenCheck.Index = export.Export1.Index;
          export.HiddenCheck.CheckSize();

          if (!Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
            local.Initialized.Timestamp))
          {
            // ************************************************************
            //   If here checks to see if the repeating group was
            //   populated via a display.  If not, force a display.
            // ************************************************************
            if (!Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
              export.HiddenCheck.Item.GrExportHiddenCheckAppointment.
                CreatedTimestamp) || !
              Equal(export.Export1.Item.GrCsePerson.Number,
              export.HiddenCheck.Item.GrExportHiddenCheckCsePerson.Number) || !
              Equal(export.Export1.Item.GrCase.Number,
              export.HiddenCheck.Item.GrExportHiddenCheckCase.Number) || !
              Equal(export.Export1.Item.GrServiceProvider.UserId,
              export.HiddenCheck.Item.GrExportHiddenCheckServiceProvider.
                UserId) || !
              Equal(export.Export1.Item.GrAppointment.Date,
              export.HiddenCheck.Item.GrExportHiddenCheckAppointment.Date) || export
              .Export1.Item.GrAppointment.Time != export
              .HiddenCheck.Item.GrExportHiddenCheckAppointment.Time || AsChar
              (export.Export1.Item.GrAppointment.Type1) != AsChar
              (export.HiddenCheck.Item.GrExportHiddenCheckAppointment.Type1) ||
              AsChar(export.Export1.Item.GrAppointment.ReasonCode) != AsChar
              (export.HiddenCheck.Item.GrExportHiddenCheckAppointment.ReasonCode))
              
            {
              // ************************************************************
              //   Check to see if information in the line was changed prior
              //   to this action.  If so, force redisplay.
              // ************************************************************
              var field =
                GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

              field.Error = true;

              ExitState = "CO0000_INFO_CHNGD_REDISPLAY";

              goto Test6;
            }
          }
          else
          {
            var field =
              GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

            field.Error = true;

            ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

            goto Test6;
          }

          export.Pass.GrExportPassAppointment.Assign(
            export.Export1.Item.GrAppointment);
          export.GrExportPass.Number = export.Export1.Item.GrCase.Number;
          MoveCsePerson(export.Export1.Item.GrCsePerson,
            export.Pass.GrExportPassCsePerson);
          export.Pass.GrExportPassOffice.SystemGeneratedId =
            export.Export1.Item.GrOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(export.Export1.Item.GrOfficeServiceProvider,
            export.Pass.GrExportPassOfficeServiceProvider);
          MoveServiceProvider(export.Export1.Item.GrServiceProvider,
            export.Pass.GrExportPassServiceProvider);
          export.GrExportPass.Number = export.Export1.Item.GrCase.Number;

          if (ReadInfrastructure())
          {
            // ************************************************************
            //   Move to holding, continue processing.
            // ************************************************************
            export.PassInfrastructure.Assign(entities.Infrastructure);
            ExitState = "ECO_XFR_TO_SP_HIST";

            goto Test6;
          }
          else
          {
            // ************************************************************
            //    If no infrastructure record currently exists for this
            //    appointment, if it is not a type A appointment
            //    (activity), there is no need to flow to HIST.  If it IS
            //    a type A, then an infrastructure record must be created.
            // ************************************************************
            ExitState = "CO0000_NO_HISTORY_EXIST_FOR_APPT";

            goto Test6;
          }

          break;
        case "PRINT":
          // ************************************************************
          //  A flow will be initiated to DOCM.  On return from DOCM, the
          //  infrastructure needs to be created.
          // ************************************************************
          export.Export1.Index = local.Pointer.Count - 1;
          export.Export1.CheckSize();

          export.HiddenCheck.Index = export.Export1.Index;
          export.HiddenCheck.CheckSize();

          if (AsChar(export.Export1.Item.GrAppointment.Type1) == 'A')
          {
            var field =
              GetField(export.Export1.Item.GrExportLineSelect, "selectChar");

            field.Error = true;

            ExitState = "SP0000_NO_DOC_4_APPT_TYP_A";

            goto Test6;
          }

          // mjr
          // -------------------------------------------------
          // 01/29/1999
          // Removed check of a related infrastructure record.  If we need
          // to limit them from printing only one, then we need to check
          // for an outgoing_document of the same name and same key fields.
          // --------------------------------------------------------------
          export.ToDocmFilter.Type1 = "AMEN";
          export.DocmProtectFilter.Flag = "Y";

          // mjr
          // -------------------------------------------------
          // 01/07/2000
          // Moved the following (commented) code to the RETDOCM command
          // --------------------------------------------------------------
          ExitState = "ECO_LNK_TO_DOCM";

          return;
        default:
          break;
      }

Test4:

      if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
        (global.Command, "NEXT"))
      {
        if (Equal(global.Command, "PREV") || Equal(global.Command, "NEXT"))
        {
          if (Equal(export.HiddenPrev.Command, "PRED"))
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            UseSpCabReadPredecessorAppts1();

            if (Equal(global.Command, "NEXT"))
            {
              if (local.PageCount.PageNumber > Export
                .HiddenPageKeysGroup.Capacity)
              {
                ExitState = "ACO_NE0000_MAX_PAGES_REACHED";
              }
              else if (export.Export1.IsFull)
              {
                export.HiddenPageCount.PageNumber =
                  local.PageCount.PageNumber - 1;

                export.HiddenPageKeys.Index = local.PageCount.PageNumber - 1;
                export.HiddenPageKeys.CheckSize();

                export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
                  local.PageKeys);
              }
              else
              {
                export.HiddenPageCount.PageNumber = local.PageCount.PageNumber;

                export.HiddenPageKeys.Index = local.PageCount.PageNumber - 1;
                export.HiddenPageKeys.CheckSize();

                export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
                  local.PageKeys);
              }
            }

            goto Test6;
          }
          else if (Equal(export.HiddenPrev.Command, "SUCC"))
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            UseSpCabShowSuccAppts1();

            if (Equal(global.Command, "NEXT"))
            {
              if (local.PageCount.PageNumber > Export
                .HiddenPageKeysGroup.Capacity)
              {
                ExitState = "ACO_NE0000_MAX_PAGES_REACHED";
              }
              else if (export.Export1.IsFull)
              {
                export.HiddenPageKeys.Index = local.PageCount.PageNumber - 1;
                export.HiddenPageKeys.CheckSize();

                export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
                  local.PageKeys);
                export.HiddenPageCount.PageNumber =
                  local.PageCount.PageNumber - 1;
              }
              else
              {
                export.HiddenPageCount.PageNumber = local.PageCount.PageNumber;
              }
            }
            else
            {
              export.HiddenPageCount.PageNumber = local.PageCount.PageNumber;
            }

            goto Test6;
          }
        }
        else
        {
          // ---------------------------------------------
          // Start of code - Raju 01/14/97:1145 hrs CST
          // ---------------------------------------------
          for(export.Exp3.Index = 0; export.Exp3.Index < export.Exp3.Count; ++
            export.Exp3.Index)
          {
            if (!export.Exp3.CheckSize())
            {
              break;
            }

            MoveAppointment1(local.Blank, export.Exp3.Update.Exp3LastReadHidden);
              
          }

          export.Exp3.CheckIndex();

          // ---------------------------------------------
          // End   of code
          // ---------------------------------------------
          if (IsEmpty(export.SearchServiceProvider.UserId))
          {
            export.SearchServiceProvider.UserId = global.UserId;
            local.Read.UserId = export.SearchServiceProvider.UserId;
            local.Counter.Count = 0;

            foreach(var item in ReadServiceProviderOfficeServiceProviderOffice())
              
            {
              ++local.Counter.Count;

              if (local.Counter.Count > 1)
              {
                ExitState = "SP0000_SP_MULTI_OFFC_ROLES";

                goto Test6;
              }

              MoveOffice(entities.Office, export.SearchOffice);
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.SearchOfficeServiceProvider);
              MoveServiceProvider(entities.ServiceProvider,
                export.SearchServiceProvider);

              // *** CQ#10202 Changes Begin Here ***
              MoveOffice(entities.Office, export.SearchHiddenCheckOffice);
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.SearchHiddenCheckOfficeServiceProvider);
              MoveServiceProvider(entities.ServiceProvider,
                export.SearchHiddenCheckServiceProvider);

              // *** CQ#10202 Changes End   Here ***
              UseSpCabReadSrvcPrvdrSetName();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field = GetField(export.SearchServiceProvider, "userId");

                field.Error = true;
              }
              else
              {
                MoveServiceProvider(export.SearchServiceProvider,
                  export.SearchHiddenCheckServiceProvider);
              }
            }
          }
          else if (Equal(export.SearchServiceProvider.UserId, "*"))
          {
            local.Read.UserId = "";
          }
          else
          {
            if (Equal(export.SearchServiceProvider.UserId,
              export.SearchHiddenCheckServiceProvider.UserId))
            {
              goto Test5;
            }

            local.Read.UserId = export.SearchServiceProvider.UserId;
            local.Counter.Count = 0;

            foreach(var item in ReadServiceProviderOfficeServiceProviderOffice())
              
            {
              ++local.Counter.Count;

              if (local.Counter.Count > 1)
              {
                var field = GetField(export.ServiceProvider, "selectChar");

                field.Error = true;

                ExitState = "SP0000_SP_MULTI_OFFC_ROLES";

                goto Test6;
              }

              MoveOffice(entities.Office, export.SearchOffice);
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.SearchOfficeServiceProvider);
              MoveServiceProvider(entities.ServiceProvider,
                export.SearchServiceProvider);
              MoveOffice(entities.Office, export.SearchHiddenCheckOffice);
              MoveOfficeServiceProvider(entities.OfficeServiceProvider,
                export.SearchHiddenCheckOfficeServiceProvider);
              MoveServiceProvider(entities.ServiceProvider,
                export.SearchHiddenCheckServiceProvider);
              UseSpCabReadSrvcPrvdrSetName();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field = GetField(export.SearchServiceProvider, "userId");

                field.Error = true;
              }
              else
              {
                MoveServiceProvider(export.SearchServiceProvider,
                  export.SearchHiddenCheckServiceProvider);
              }
            }
          }

Test5:

          if (local.Counter.Count <= 1)
          {
            if (Equal(export.SearchServiceProvider.UserId, "*"))
            {
              export.FormattedSrvcprvdrName.Text32 = "";
            }
            else
            {
              UseSpCabReadSrvcPrvdrSetName();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field = GetField(export.SearchServiceProvider, "userId");

                field.Error = true;
              }
              else
              {
                MoveServiceProvider(export.SearchServiceProvider,
                  export.SearchHiddenCheckServiceProvider);
              }
            }

            if (import.SearchOffice.SystemGeneratedId != import
              .SearchHiddenCheckOffice.SystemGeneratedId)
            {
              export.SearchOffice.Name = "";

              if (ReadOffice())
              {
                MoveOffice(entities.Office, export.SearchOffice);
              }
              else if (import.SearchOffice.SystemGeneratedId > 0)
              {
                var field = GetField(export.SearchOffice, "systemGeneratedId");

                field.Error = true;

                ExitState = "OFFICE_NF";
              }
            }

            if (!IsEmpty(export.SearchOfficeServiceProvider.RoleCode))
            {
              local.Code.CodeName = "OFFICE SERVICE PROVIDER ROLE";
              local.CodeValue.Cdvalue =
                export.SearchOfficeServiceProvider.RoleCode;
              UseCabValidateCodeValue();

              if (AsChar(local.ValidCode.Flag) != 'Y')
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  var field =
                    GetField(export.SearchOfficeServiceProvider, "roleCode");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
                }
              }
            }
          }

          if (import.HiddenPageCount.PageNumber == 0)
          {
            // ************************************************************
            //   The page key info is initialized to blanks and 0's.
            // ************************************************************
            local.PageCount.PageNumber = 1;

            export.HiddenPageKeys.Index = local.PageCount.PageNumber - 1;
            export.HiddenPageKeys.CheckSize();

            export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
              local.PageKeys);
          }
          else
          {
            // ************************************************************
            //   A display resets the page key info to spaces, and new
            //   page key info is recovered via the display
            // ************************************************************
            for(export.HiddenPageKeys.Index = 0; export.HiddenPageKeys.Index < Export
              .HiddenPageKeysGroup.Capacity; ++export.HiddenPageKeys.Index)
            {
              if (!export.HiddenPageKeys.CheckSize())
              {
                break;
              }

              export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
                local.PageKeys);
            }

            export.HiddenPageKeys.CheckIndex();
            local.PageCount.PageNumber = 1;
          }

          export.SearchHiddenCheckCase.Number = export.SearchCase.Number;
          MoveCsePerson(export.SearchCsePerson,
            export.SearchHiddenCheckCsePerson);
          MoveOffice(export.SearchOffice, export.SearchHiddenCheckOffice);
          MoveOfficeServiceProvider(export.SearchOfficeServiceProvider,
            export.SearchHiddenCheckOfficeServiceProvider);
          MoveServiceProvider(export.SearchServiceProvider,
            export.SearchHiddenCheckServiceProvider);
          export.StartingHiddenCheck.Date = export.Starting.Date;
          export.DiplayAllHiddenCheck.Flag = export.DisplayAll.Flag;
          export.HiddenPrev.Command = global.Command;
        }

        if (Equal(local.Appointment.CreatedTimestamp,
          local.Initialized.Timestamp))
        {
          local.Read.UserId = export.SearchServiceProvider.UserId;
          UseSpCabReadEachAppointment();
        }
        else
        {
          UseSpCabReadDistinctAppt();
          export.SearchHiddenCheckCase.Number = export.SearchCase.Number;
          MoveCsePerson(export.SearchCsePerson,
            export.SearchHiddenCheckCsePerson);
          MoveOffice(export.SearchOffice, export.SearchHiddenCheckOffice);
          MoveOfficeServiceProvider(export.SearchOfficeServiceProvider,
            export.SearchHiddenCheckOfficeServiceProvider);
          MoveServiceProvider(export.SearchServiceProvider,
            export.SearchHiddenCheckServiceProvider);
          export.StartingHiddenCheck.Date = export.Starting.Date;
          export.DiplayAllHiddenCheck.Flag = export.DisplayAll.Flag;
          UseSpCabReadSrvcPrvdrSetName();
          MoveServiceProvider(export.SearchServiceProvider,
            export.SearchHiddenCheckServiceProvider);
        }

        if (!export.Export1.IsEmpty)
        {
          if (local.PageCount.PageNumber > Export.HiddenPageKeysGroup.Capacity)
          {
            ExitState = "ACO_NE0000_MAX_PAGES_REACHED";

            goto Test6;
          }
          else if (export.Export1.IsFull && !Equal(global.Command, "PREV"))
          {
            export.HiddenPageKeys.Index = local.PageCount.PageNumber - 1;
            export.HiddenPageKeys.CheckSize();

            export.HiddenPageKeys.Update.GexportHiddenPageKeys.Assign(
              local.PageKeys);
            export.HiddenPageCount.PageNumber = local.PageCount.PageNumber - 1;
          }
          else
          {
            export.HiddenPageCount.PageNumber = local.PageCount.PageNumber;
          }

          // mjr
          // -----------------------------------------------
          // 12/03/1998
          // Added check for an exitstate returned from Print
          // ------------------------------------------------------------
          local.Position.Count = Find(export.Hidden.MiscText2, "PRINT:");

          if (local.Position.Count <= 0)
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else
          {
            // mjr---> Determines the appropriate exitstate for the Print 
            // process
            local.Print.Text50 = export.Hidden.MiscText2 ?? Spaces(50);
            UseSpPrintDecodeReturnCode();
            export.Hidden.MiscText2 = local.Print.Text50;
          }

          // ---------------------------------------------
          // Start of code - Raju 01/14/97:1134 hrs CST
          // ---------------------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            export.Exp3.Index = export.Export1.Index;
            export.Exp3.CheckSize();

            MoveAppointment1(export.Export1.Item.GrAppointment,
              export.Exp3.Update.Exp3LastReadHidden);
          }

          export.Export1.CheckIndex();

          // ---------------------------------------------
          // End   of code
          // ---------------------------------------------
          // *** CQ#8968 Changes Begin Here ***
          // *** Added the Else Part to avoid the Scrolling abend ***
          // It abends when on last record and if you go to the next page and 
          // press PREV
        }
        else
        {
          export.HiddenPageCount.PageNumber = local.PageCount.PageNumber;

          // *** CQ#8968 Changes End   Here ***
        }
      }
    }

Test6:

    // ****  Set Protection ****
    if (!export.Export1.IsEmpty)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        export.HiddenCheck.Index = export.Export1.Index;
        export.HiddenCheck.CheckSize();

        if (Equal(export.Export1.Item.GrAppointment.CreatedTimestamp,
          local.Initialized.Timestamp))
        {
          continue;
        }

        var field1 =
          GetField(export.Export1.Item.GrExportCsePerson, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Export1.Item.GrExportCdvlReason, "selectChar");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Export1.Item.GrExportCdvlType, "selectChar");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Export1.Item.GrExportServiceProvider, "selectChar");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 =
          GetField(export.Export1.Item.GrExportAmPmInd, "selectChar");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Export1.Item.GrAppointment, "date");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Export1.Item.GrAppointment, "time");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.Export1.Item.GrAppointment, "type1");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.Export1.Item.GrAppointment, "reasonCode");

        field9.Color = "cyan";
        field9.Protected = true;

        if (!IsEmpty(export.Export1.Item.GrAppointment.Result) && AsChar
          (export.Export1.Item.GrAppointment.Result) == AsChar
          (export.HiddenCheck.Item.GrExportHiddenCheckAppointment.Result))
        {
          var field10 =
            GetField(export.Export1.Item.GrExportCdvlResult, "selectChar");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.Export1.Item.GrAppointment, "result");

          field11.Color = "cyan";
          field11.Protected = true;
        }
      }

      export.Export1.CheckIndex();
    }
  }

  private static void MoveAppointment1(Appointment source, Appointment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Type1 = source.Type1;
  }

  private static void MoveAppointment2(Appointment source, Appointment target)
  {
    target.Type1 = source.Type1;
    target.Result = source.Result;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveAppointment3(Appointment source, Appointment target)
  {
    target.Result = source.Result;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveAppointment4(Appointment source, Appointment target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveExport2(SpCabReadPredecessorAppts.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveOfficeServiceProvider(source.GrOfficeServiceProvider,
      target.GrOfficeServiceProvider);
    target.GrExportLineSelect.SelectChar = source.GrCommon.SelectChar;
    target.GrCase.Number = source.GrCase.Number;
    MoveCsePerson(source.GrCsePerson, target.GrCsePerson);
    target.GrExportCsePerson.SelectChar = source.GrExportCsePerson.SelectChar;
    target.GrCaseRole.Type1 = source.GrCaseRole.Type1;
    target.GrAppointment.Assign(source.GrAppointment);
    target.GrExportAmPmInd.SelectChar = source.GrExportApptAmPmInd.SelectChar;
    target.GrExportCdvlReason.SelectChar = source.GrExportCdvlReason.SelectChar;
    target.GrExportCdvlType.SelectChar = source.GrExportCdvlType.SelectChar;
    target.GrExportCdvlResult.SelectChar = source.GrExportCdvlResult.SelectChar;
    target.GrOffice.SystemGeneratedId = source.GrOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrServiceProvider, target.GrServiceProvider);
    target.GrExportServiceProvider.SelectChar =
      source.GrExportServiceProvider.SelectChar;
  }

  private static void MoveExport3(SpCabReadEachAppointment.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveOfficeServiceProvider(source.GrOfficeServiceProvider,
      target.GrOfficeServiceProvider);
    target.GrExportLineSelect.SelectChar = source.GrCommon.SelectChar;
    target.GrCase.Number = source.GrCase.Number;
    MoveCsePerson(source.GrCsePerson, target.GrCsePerson);
    target.GrExportCsePerson.SelectChar = source.GrExportCsePerson.SelectChar;
    target.GrCaseRole.Type1 = source.GrCaseRole.Type1;
    target.GrAppointment.Assign(source.GrAppointment);
    target.GrExportAmPmInd.SelectChar = source.GrExportApptAmPmInd.SelectChar;
    target.GrExportCdvlReason.SelectChar = source.GrExportCdvlReason.SelectChar;
    target.GrExportCdvlType.SelectChar = source.GrExportCdvlType.SelectChar;
    target.GrExportCdvlResult.SelectChar = source.GrExportCdvlResult.SelectChar;
    target.GrOffice.SystemGeneratedId = source.GrOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrServiceProvider, target.GrServiceProvider);
    target.GrExportServiceProvider.SelectChar =
      source.GrExportServiceProvider.SelectChar;
  }

  private static void MoveExport4(SpCabReadDistinctAppt.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveOfficeServiceProvider(source.GrOfficeServiceProvider,
      target.GrOfficeServiceProvider);
    target.GrExportLineSelect.SelectChar = source.GrCommon.SelectChar;
    target.GrCase.Number = source.GrCase.Number;
    MoveCsePerson(source.GrCsePerson, target.GrCsePerson);
    target.GrExportCsePerson.SelectChar = source.GrExportCsePerson.SelectChar;
    target.GrCaseRole.Type1 = source.GrCaseRole.Type1;
    target.GrAppointment.Assign(source.GrAppointment);
    target.GrExportAmPmInd.SelectChar = source.GrExportApptAmPmInd.SelectChar;
    target.GrExportCdvlReason.SelectChar = source.GrExportCdvlReason.SelectChar;
    target.GrExportCdvlType.SelectChar = source.GrExportCdvlType.SelectChar;
    target.GrExportCdvlResult.SelectChar = source.GrExportCdvlResult.SelectChar;
    target.GrOffice.SystemGeneratedId = source.GrOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrServiceProvider, target.GrServiceProvider);
    target.GrExportServiceProvider.SelectChar =
      source.GrExportServiceProvider.SelectChar;
  }

  private static void MoveExport5(SpCabShowSuccAppts.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    MoveOfficeServiceProvider(source.GrOfficeServiceProvider,
      target.GrOfficeServiceProvider);
    target.GrExportLineSelect.SelectChar = source.GrCommon.SelectChar;
    target.GrCase.Number = source.GrCase.Number;
    MoveCsePerson(source.GrCsePerson, target.GrCsePerson);
    target.GrExportCsePerson.SelectChar = source.GrExportCsePerson.SelectChar;
    target.GrCaseRole.Type1 = source.GrCaseRole.Type1;
    target.GrAppointment.Assign(source.GrAppointment);
    target.GrExportAmPmInd.SelectChar = source.GrExportApptAmPmInd.SelectChar;
    target.GrExportCdvlReason.SelectChar = source.GrExportCdvlReason.SelectChar;
    target.GrExportCdvlType.SelectChar = source.GrExportCdvlType.SelectChar;
    target.GrExportCdvlResult.SelectChar = source.GrExportCdvlResult.SelectChar;
    target.GrOffice.SystemGeneratedId = source.GrOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrServiceProvider, target.GrServiceProvider);
    target.GrExportServiceProvider.SelectChar =
      source.GrExportServiceProvider.SelectChar;
  }

  private static void MoveHiddenCheck1(SpCabReadPredecessorAppts.Export.
    HiddenCheckGroup source, Export.HiddenCheckGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: move results in empty operation.");
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
  }

  private static void MoveHiddenCheck2(SpCabReadEachAppointment.Export.
    HiddenCheckGroup source, Export.HiddenCheckGroup target)
  {
    target.GrExportHiddenCheckCase.Number =
      source.GrExportHiddenCheckCase.Number;
    MoveCsePerson(source.GrExportHiddenCheckCsePerson,
      target.GrExportHiddenCheckCsePerson);
    target.GrExportHiddenCheckAppointment.Assign(
      source.GrExportHiddenCheckAppointment);
    target.GrExportHiddenAmPmInd.SelectChar =
      source.GrExportHiddenApptAmPmInd.SelectChar;
    target.GrExportHiddenCheckOffice.SystemGeneratedId =
      source.GrExportHiddenCheckOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrExportHiddenCheckServiceProvider,
      target.GrExportHiddenCheckServiceProvider);
    target.GrExportHiddenCheckOfficeServiceProvider.RoleCode =
      source.GrExportHiddenCheckOfficeServiceProvider.RoleCode;
  }

  private static void MoveHiddenCheck3(SpCabReadDistinctAppt.Export.
    HiddenCheckGroup source, Export.HiddenCheckGroup target)
  {
    target.GrExportHiddenCheckCase.Number =
      source.GrExportHiddenCheckCase.Number;
    MoveCsePerson(source.GrExportHiddenCheckCsePerson,
      target.GrExportHiddenCheckCsePerson);
    target.GrExportHiddenCheckAppointment.Assign(
      source.GrExportHiddenCheckAppointment);
    target.GrExportHiddenAmPmInd.SelectChar =
      source.GrExportHiddenApptAmPmInd.SelectChar;
    target.GrExportHiddenCheckOffice.SystemGeneratedId =
      source.GrExportHiddenCheckOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrExportHiddenCheckServiceProvider,
      target.GrExportHiddenCheckServiceProvider);
    target.GrExportHiddenCheckOfficeServiceProvider.RoleCode =
      source.GrExportHiddenCheckOfficeServiceProvider.RoleCode;
  }

  private static void MoveHiddenCheck4(SpCabShowSuccAppts.Export.
    HiddenCheckGroup source, Export.HiddenCheckGroup target)
  {
    target.GrExportHiddenCheckCase.Number =
      source.GrExportHiddenCheckCase.Number;
    MoveCsePerson(source.GrExportHiddenCheckCsePerson,
      target.GrExportHiddenCheckCsePerson);
    target.GrExportHiddenCheckAppointment.Assign(
      source.GrExportHiddenCheckAppointment);
    target.GrExportHiddenAmPmInd.SelectChar =
      source.GrExportHiddenApptAmPmInd.SelectChar;
    target.GrExportHiddenCheckOffice.SystemGeneratedId =
      source.GrExportHiddenCheckOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrExportHiddenCheckServiceProvider,
      target.GrExportHiddenCheckServiceProvider);
    target.GrExportHiddenCheckOfficeServiceProvider.RoleCode =
      source.GrExportHiddenCheckOfficeServiceProvider.RoleCode;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MovePassToImport2(Local.PassGroup source,
    SpCabReadPredecessorAppts.Import.ImportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.GrOfficeServiceProvider.RoleCode =
      source.GrLocalPassOfficeServiceProvider.RoleCode;
    target.GrCase.Number = source.GrLocalPassCase.Number;
    MoveCsePerson(source.GrLocalPassCsePerson, target.GrCsePerson);
    target.GrAppointment.Assign(source.GrLocalPassAppointment);
    target.GrOffice.SystemGeneratedId =
      source.GrLocalPassOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrLocalPassServiceProvider,
      target.GrServiceProvider);
    target.GrImportApptAmPmInd.SelectChar = source.PassApptAmPmInd.SelectChar;
  }

  private static void MovePassToImport3(Local.PassGroup source,
    SpCabAddAppointment.Import.ImportGroup target)
  {
    MoveOfficeServiceProvider(source.GrLocalPassOfficeServiceProvider,
      target.GrOfficeServiceProvider);
    target.GrCase.Number = source.GrLocalPassCase.Number;
    MoveCsePerson(source.GrLocalPassCsePerson, target.GrCsePerson);
    target.GrAppointment.Assign(source.GrLocalPassAppointment);
    target.GrImportApptAmPmInd.SelectChar = source.PassApptAmPmInd.SelectChar;
    target.GrOffice.SystemGeneratedId =
      source.GrLocalPassOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrLocalPassServiceProvider,
      target.GrServiceProvider);
  }

  private static void MovePassToImport4(Local.PassGroup source,
    SpCabShowSuccAppts.Import.ImportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.GrOfficeServiceProvider.RoleCode =
      source.GrLocalPassOfficeServiceProvider.RoleCode;
    target.GrCase.Number = source.GrLocalPassCase.Number;
    MoveCsePerson(source.GrLocalPassCsePerson, target.GrCsePerson);
    target.GrAppointment.Assign(source.GrLocalPassAppointment);
    target.GrOffice.SystemGeneratedId =
      source.GrLocalPassOffice.SystemGeneratedId;
    MoveServiceProvider(source.GrLocalPassServiceProvider,
      target.GrServiceProvider);
    target.GrImportApptAmPmInd.SelectChar = source.PassApptAmPmInd.SelectChar;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.ScrollingMessage = source.ScrollingMessage;
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Date.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
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

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = export.SearchCsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.SearchCsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.SearchCase.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.SearchCase.Number = useImport.Case1.Number;
  }

  private void UseCabZeroFillNumber3()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Export1.Item.GrCase.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Export1.Update.GrCase.Number = useImport.Case1.Number;
  }

  private void UseCabZeroFillNumber4()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = export.Export1.Item.GrCsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Export1.Update.GrCsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpAmenRaiseEvent()
  {
    var useImport = new SpAmenRaiseEvent.Import();
    var useExport = new SpAmenRaiseEvent.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Case1.Number = local.Inf1.Number;
    useImport.CsePerson.Number = local.Inf2.Number;
    useImport.Appointment.CreatedTimestamp = local.Appointment.CreatedTimestamp;

    Call(SpAmenRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabAddAppointment()
  {
    var useImport = new SpCabAddAppointment.Import();
    var useExport = new SpCabAddAppointment.Export();

    MovePassToImport3(local.Pass, useImport.Import1);

    Call(SpCabAddAppointment.Execute, useImport, useExport);

    local.Appointment.CreatedTimestamp = useExport.Pass.CreatedTimestamp;
  }

  private void UseSpCabDeleteAppointment()
  {
    var useImport = new SpCabDeleteAppointment.Import();
    var useExport = new SpCabDeleteAppointment.Export();

    MoveAppointment3(export.Export1.Item.GrAppointment, useImport.Appointment);

    Call(SpCabDeleteAppointment.Execute, useImport, useExport);
  }

  private void UseSpCabReadDistinctAppt()
  {
    var useImport = new SpCabReadDistinctAppt.Import();
    var useExport = new SpCabReadDistinctAppt.Export();

    useImport.DistinctTimestamp.CreatedTimestamp =
      local.Appointment.CreatedTimestamp;

    Call(SpCabReadDistinctAppt.Execute, useImport, useExport);

    MoveAppointment4(useExport.PageKeys, local.PageKeys);
    useExport.Export1.CopyTo(export.Export1, MoveExport4);
    useExport.HiddenCheck.CopyTo(export.HiddenCheck, MoveHiddenCheck3);
    MoveCsePerson(useExport.SearchCsePerson, export.SearchCsePerson);
    export.SearchCase.Number = useExport.SearchCase.Number;
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    export.DisplayAll.Flag = useExport.DisplayAll.Flag;
    export.Starting.Date = useExport.Starting.Date;
    MoveOfficeServiceProvider(useExport.SearchOfficeServiceProvider,
      export.SearchOfficeServiceProvider);
    MoveOffice(useExport.SearchOffice, export.SearchOffice);
    export.SearchServiceProvider.UserId =
      useExport.SearchServiceProvider.UserId;
  }

  private void UseSpCabReadEachAppointment()
  {
    var useImport = new SpCabReadEachAppointment.Import();
    var useExport = new SpCabReadEachAppointment.Export();

    useImport.PageKeys.Assign(local.PageKeys);
    useImport.PageNumber.PageNumber = local.PageCount.PageNumber;
    useImport.SearchServiceProvider.UserId = local.Read.UserId;
    MoveCsePerson(export.SearchCsePerson, useImport.SearchCsePerson);
    useImport.SearchCase.Number = export.SearchCase.Number;
    useImport.DisplayAll.Flag = export.DisplayAll.Flag;
    useImport.Starting.Date = export.Starting.Date;
    MoveOfficeServiceProvider(export.SearchOfficeServiceProvider,
      useImport.SearchOfficeServiceProvider);
    MoveOffice(export.SearchOffice, useImport.SearchOffice);

    Call(SpCabReadEachAppointment.Execute, useImport, useExport);

    MoveAppointment4(useExport.PageKeys, local.PageKeys);
    local.PageCount.PageNumber = useExport.PageNumber.PageNumber;
    useExport.Export1.CopyTo(export.Export1, MoveExport3);
    useExport.HiddenCheck.CopyTo(export.HiddenCheck, MoveHiddenCheck2);
    MoveCsePerson(useExport.SearchCsePerson, export.SearchCsePerson);
    export.SearchCase.Number = useExport.SearchCase.Number;
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    export.DisplayAll.Flag = useExport.DisplayAll.Flag;
    export.Starting.Date = useExport.Starting.Date;
    MoveOfficeServiceProvider(useExport.SearchOfficeServiceProvider,
      export.SearchOfficeServiceProvider);
    MoveOffice(useExport.SearchOffice, export.SearchOffice);
  }

  private void UseSpCabReadPredecessorAppts1()
  {
    var useImport = new SpCabReadPredecessorAppts.Import();
    var useExport = new SpCabReadPredecessorAppts.Export();

    MovePassToImport2(local.Pass, useImport.Import1);
    useImport.Starting.CreatedTimestamp = local.PageKeys.CreatedTimestamp;
    useImport.PageKeys.Assign(local.PageKeys);
    useImport.PageNumber.PageNumber = local.PageCount.PageNumber;

    Call(SpCabReadPredecessorAppts.Execute, useImport, useExport);

    MoveAppointment4(useExport.PageKeys, local.PageKeys);
    export.HiddenPageCount.PageNumber = useExport.PageNumber.PageNumber;
    useExport.Export1.CopyTo(export.Export1, MoveExport2);
    useExport.HiddenCheck.CopyTo(export.HiddenCheck, MoveHiddenCheck1);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
  }

  private void UseSpCabReadPredecessorAppts2()
  {
    var useImport = new SpCabReadPredecessorAppts.Import();
    var useExport = new SpCabReadPredecessorAppts.Export();

    useImport.PageKeys.Assign(local.PageKeys);
    useImport.PageNumber.PageNumber = local.PageCount.PageNumber;
    useImport.Import1.GrOfficeServiceProvider.RoleCode =
      export.Export1.Item.GrOfficeServiceProvider.RoleCode;
    useImport.Import1.GrCase.Number = export.Export1.Item.GrCase.Number;
    MoveCsePerson(export.Export1.Item.GrCsePerson, useImport.Import1.GrCsePerson);
      
    useImport.Import1.GrCaseRole.Type1 = export.Export1.Item.GrCaseRole.Type1;
    useImport.Starting.CreatedTimestamp =
      export.Export1.Item.GrAppointment.CreatedTimestamp;
    useImport.Import1.GrAppointment.Assign(export.Export1.Item.GrAppointment);
    useImport.Import1.GrImportApptAmPmInd.SelectChar =
      export.Export1.Item.GrExportAmPmInd.SelectChar;
    useImport.Import1.GrOffice.SystemGeneratedId =
      export.Export1.Item.GrOffice.SystemGeneratedId;
    MoveServiceProvider(export.Export1.Item.GrServiceProvider,
      useImport.Import1.GrServiceProvider);

    Call(SpCabReadPredecessorAppts.Execute, useImport, useExport);

    MoveAppointment4(useExport.PageKeys, local.PageKeys);
    local.PageCount.PageNumber = useExport.PageNumber.PageNumber;
    useExport.Export1.CopyTo(export.Export1, MoveExport2);
    useExport.HiddenCheck.CopyTo(export.HiddenCheck, MoveHiddenCheck1);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
  }

  private void UseSpCabReadSrvcPrvdrSetName()
  {
    var useImport = new SpCabReadSrvcPrvdrSetName.Import();
    var useExport = new SpCabReadSrvcPrvdrSetName.Export();

    useImport.ServiceProvider.UserId = export.SearchServiceProvider.UserId;

    Call(SpCabReadSrvcPrvdrSetName.Execute, useImport, useExport);

    export.FormattedSrvcprvdrName.Text32 = useExport.WorkArea.Text32;
    MoveServiceProvider(useExport.ServiceProvider, export.SearchServiceProvider);
      
  }

  private void UseSpCabShowSuccAppts1()
  {
    var useImport = new SpCabShowSuccAppts.Import();
    var useExport = new SpCabShowSuccAppts.Export();

    MovePassToImport4(local.Pass, useImport.Import1);
    useImport.PageKeys.Assign(local.PageKeys);
    useImport.Starting.CreatedTimestamp = local.PageKeys.CreatedTimestamp;
    useImport.PageNumber.PageNumber = local.PageCount.PageNumber;

    Call(SpCabShowSuccAppts.Execute, useImport, useExport);

    MoveAppointment4(useExport.PageKeys, local.PageKeys);
    local.PageCount.PageNumber = useExport.PageNumber.PageNumber;
    useExport.Export1.CopyTo(export.Export1, MoveExport5);
    useExport.HiddenCheck.CopyTo(export.HiddenCheck, MoveHiddenCheck4);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
  }

  private void UseSpCabShowSuccAppts2()
  {
    var useImport = new SpCabShowSuccAppts.Import();
    var useExport = new SpCabShowSuccAppts.Export();

    useImport.PageKeys.Assign(local.PageKeys);
    useImport.PageNumber.PageNumber = local.PageCount.PageNumber;
    useImport.Import1.GrOfficeServiceProvider.RoleCode =
      export.Export1.Item.GrOfficeServiceProvider.RoleCode;
    useImport.Import1.GrCase.Number = export.Export1.Item.GrCase.Number;
    MoveCsePerson(export.Export1.Item.GrCsePerson, useImport.Import1.GrCsePerson);
      
    useImport.Import1.GrCaseRole.Type1 = export.Export1.Item.GrCaseRole.Type1;
    useImport.Starting.CreatedTimestamp =
      export.Export1.Item.GrAppointment.CreatedTimestamp;
    useImport.Import1.GrAppointment.Assign(export.Export1.Item.GrAppointment);
    useImport.Import1.GrImportApptAmPmInd.SelectChar =
      export.Export1.Item.GrExportAmPmInd.SelectChar;
    useImport.Import1.GrOffice.SystemGeneratedId =
      export.Export1.Item.GrOffice.SystemGeneratedId;
    MoveServiceProvider(export.Export1.Item.GrServiceProvider,
      useImport.Import1.GrServiceProvider);

    Call(SpCabShowSuccAppts.Execute, useImport, useExport);

    MoveAppointment4(useExport.PageKeys, local.PageKeys);
    local.PageCount.PageNumber = useExport.PageNumber.PageNumber;
    useExport.Export1.CopyTo(export.Export1, MoveExport5);
    useExport.HiddenCheck.CopyTo(export.HiddenCheck, MoveHiddenCheck4);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
  }

  private void UseSpCabUpdateAppointment()
  {
    var useImport = new SpCabUpdateAppointment.Import();
    var useExport = new SpCabUpdateAppointment.Export();

    MoveAppointment3(export.Export1.Item.GrAppointment, useImport.Appointment);

    Call(SpCabUpdateAppointment.Execute, useImport, useExport);
  }

  private void UseSpLinkPredSuccAppointments()
  {
    var useImport = new SpLinkPredSuccAppointments.Import();
    var useExport = new SpLinkPredSuccAppointments.Export();

    useImport.PredecessorAppointment.CreatedTimestamp =
      local.Predecessor.CreatedTimestamp;
    useImport.SuccessorAppointment.CreatedTimestamp =
      local.Successor.CreatedTimestamp;

    Call(SpLinkPredSuccAppointments.Execute, useImport, useExport);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.Print.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.Print.Text50 = useExport.WorkArea.Text50;
  }

  private bool ReadAppointment1()
  {
    entities.Predecessor.Populated = false;

    return Read("ReadAppointment1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.Predecessor.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Predecessor.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.Predecessor.InfId = db.GetNullableInt32(reader, 1);
        entities.Predecessor.AppTstamp = db.GetNullableDateTime(reader, 2);
        entities.Predecessor.Populated = true;
      });
  }

  private bool ReadAppointment2()
  {
    entities.Predecessor.Populated = false;

    return Read("ReadAppointment2",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "appTstamp",
          entities.Successor.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Predecessor.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.Predecessor.InfId = db.GetNullableInt32(reader, 1);
        entities.Predecessor.AppTstamp = db.GetNullableDateTime(reader, 2);
        entities.Predecessor.Populated = true;
      });
  }

  private bool ReadAppointment3()
  {
    entities.Successor.Populated = false;

    return Read("ReadAppointment3",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.Successor.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Successor.Date = db.GetDate(reader, 0);
        entities.Successor.Time = db.GetTimeSpan(reader, 1);
        entities.Successor.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Successor.InfId = db.GetNullableInt32(reader, 3);
        entities.Successor.AppTstamp = db.GetNullableDateTime(reader, 4);
        entities.Successor.CasNumber = db.GetNullableString(reader, 5);
        entities.Successor.CspNumber = db.GetNullableString(reader, 6);
        entities.Successor.CroType = db.GetNullableString(reader, 7);
        entities.Successor.CroId = db.GetNullableInt32(reader, 8);
        entities.Successor.Populated = true;
        CheckValid<Appointment>("CroType", entities.Successor.CroType);
      });
  }

  private bool ReadAppointment4()
  {
    System.Diagnostics.Debug.Assert(entities.Predecessor.Populated);
    entities.Successor.Populated = false;

    return Read("ReadAppointment4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Predecessor.AppTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Successor.Date = db.GetDate(reader, 0);
        entities.Successor.Time = db.GetTimeSpan(reader, 1);
        entities.Successor.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Successor.InfId = db.GetNullableInt32(reader, 3);
        entities.Successor.AppTstamp = db.GetNullableDateTime(reader, 4);
        entities.Successor.CasNumber = db.GetNullableString(reader, 5);
        entities.Successor.CspNumber = db.GetNullableString(reader, 6);
        entities.Successor.CroType = db.GetNullableString(reader, 7);
        entities.Successor.CroId = db.GetNullableInt32(reader, 8);
        entities.Successor.Populated = true;
        CheckValid<Appointment>("CroType", entities.Successor.CroType);
      });
  }

  private IEnumerable<bool> ReadAppointment5()
  {
    entities.Successor.Populated = false;

    return ReadEach("ReadAppointment5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", export.Export1.Item.GrCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Successor.Date = db.GetDate(reader, 0);
        entities.Successor.Time = db.GetTimeSpan(reader, 1);
        entities.Successor.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Successor.InfId = db.GetNullableInt32(reader, 3);
        entities.Successor.AppTstamp = db.GetNullableDateTime(reader, 4);
        entities.Successor.CasNumber = db.GetNullableString(reader, 5);
        entities.Successor.CspNumber = db.GetNullableString(reader, 6);
        entities.Successor.CroType = db.GetNullableString(reader, 7);
        entities.Successor.CroId = db.GetNullableInt32(reader, 8);
        entities.Successor.Populated = true;
        CheckValid<Appointment>("CroType", entities.Successor.CroType);

        return true;
      });
  }

  private bool ReadCaseCaseRoleCsePerson()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Export1.Item.GrCase.Number);
        db.SetString(
          command, "cspNumber", export.Export1.Item.GrCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.Export1.Item.GrAppointment.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructureAppointment()
  {
    entities.Key.Populated = false;
    entities.Successor.Populated = false;

    return Read("ReadInfrastructureAppointment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.Export1.Item.GrAppointment.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Key.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Successor.InfId = db.GetNullableInt32(reader, 0);
        entities.Successor.Date = db.GetDate(reader, 1);
        entities.Successor.Time = db.GetTimeSpan(reader, 2);
        entities.Successor.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Successor.AppTstamp = db.GetNullableDateTime(reader, 4);
        entities.Successor.CasNumber = db.GetNullableString(reader, 5);
        entities.Successor.CspNumber = db.GetNullableString(reader, 6);
        entities.Successor.CroType = db.GetNullableString(reader, 7);
        entities.Successor.CroId = db.GetNullableInt32(reader, 8);
        entities.Key.Populated = true;
        entities.Successor.Populated = true;
        CheckValid<Appointment>("CroType", entities.Successor.CroType);
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.SearchOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProviderServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.SearchOffice.SystemGeneratedId);
        db.SetString(
          command, "roleCode", export.SearchOfficeServiceProvider.RoleCode);
        db.SetString(command, "userId", export.SearchServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 3);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 4);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.ServiceProvider.UserId = db.GetString(reader, 7);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.Name = db.GetString(reader, 5);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.ServiceProvider.UserId = db.GetString(reader, 7);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeServiceProviderOffice()
  {
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return ReadEach("ReadServiceProviderOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetString(command, "userId", local.Read.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 2);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 3);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.Office.Name = db.GetString(reader, 6);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 7);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;

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
    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GimportHiddenPageKeys.
      /// </summary>
      [JsonPropertyName("gimportHiddenPageKeys")]
      public Appointment GimportHiddenPageKeys
      {
        get => gimportHiddenPageKeys ??= new();
        set => gimportHiddenPageKeys = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private Appointment gimportHiddenPageKeys;
    }

    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of GrOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("grOfficeServiceProvider")]
      public OfficeServiceProvider GrOfficeServiceProvider
      {
        get => grOfficeServiceProvider ??= new();
        set => grOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of GrImportLineSelect.
      /// </summary>
      [JsonPropertyName("grImportLineSelect")]
      public Common GrImportLineSelect
      {
        get => grImportLineSelect ??= new();
        set => grImportLineSelect = value;
      }

      /// <summary>
      /// A value of GrCase.
      /// </summary>
      [JsonPropertyName("grCase")]
      public Case1 GrCase
      {
        get => grCase ??= new();
        set => grCase = value;
      }

      /// <summary>
      /// A value of GrCsePerson.
      /// </summary>
      [JsonPropertyName("grCsePerson")]
      public CsePerson GrCsePerson
      {
        get => grCsePerson ??= new();
        set => grCsePerson = value;
      }

      /// <summary>
      /// A value of GrImportCsePerson.
      /// </summary>
      [JsonPropertyName("grImportCsePerson")]
      public Common GrImportCsePerson
      {
        get => grImportCsePerson ??= new();
        set => grImportCsePerson = value;
      }

      /// <summary>
      /// A value of GrCaseRole.
      /// </summary>
      [JsonPropertyName("grCaseRole")]
      public CaseRole GrCaseRole
      {
        get => grCaseRole ??= new();
        set => grCaseRole = value;
      }

      /// <summary>
      /// A value of GrAppointment.
      /// </summary>
      [JsonPropertyName("grAppointment")]
      public Appointment GrAppointment
      {
        get => grAppointment ??= new();
        set => grAppointment = value;
      }

      /// <summary>
      /// A value of GrImportAmPmInd.
      /// </summary>
      [JsonPropertyName("grImportAmPmInd")]
      public Common GrImportAmPmInd
      {
        get => grImportAmPmInd ??= new();
        set => grImportAmPmInd = value;
      }

      /// <summary>
      /// A value of GrImportCdvlReason.
      /// </summary>
      [JsonPropertyName("grImportCdvlReason")]
      public Common GrImportCdvlReason
      {
        get => grImportCdvlReason ??= new();
        set => grImportCdvlReason = value;
      }

      /// <summary>
      /// A value of GrImportCdvlType.
      /// </summary>
      [JsonPropertyName("grImportCdvlType")]
      public Common GrImportCdvlType
      {
        get => grImportCdvlType ??= new();
        set => grImportCdvlType = value;
      }

      /// <summary>
      /// A value of GrImportCdvlResult.
      /// </summary>
      [JsonPropertyName("grImportCdvlResult")]
      public Common GrImportCdvlResult
      {
        get => grImportCdvlResult ??= new();
        set => grImportCdvlResult = value;
      }

      /// <summary>
      /// A value of GrOffice.
      /// </summary>
      [JsonPropertyName("grOffice")]
      public Office GrOffice
      {
        get => grOffice ??= new();
        set => grOffice = value;
      }

      /// <summary>
      /// A value of GrServiceProvider.
      /// </summary>
      [JsonPropertyName("grServiceProvider")]
      public ServiceProvider GrServiceProvider
      {
        get => grServiceProvider ??= new();
        set => grServiceProvider = value;
      }

      /// <summary>
      /// A value of GrImportServiceProvider.
      /// </summary>
      [JsonPropertyName("grImportServiceProvider")]
      public Common GrImportServiceProvider
      {
        get => grImportServiceProvider ??= new();
        set => grImportServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private OfficeServiceProvider grOfficeServiceProvider;
      private Common grImportLineSelect;
      private Case1 grCase;
      private CsePerson grCsePerson;
      private Common grImportCsePerson;
      private CaseRole grCaseRole;
      private Appointment grAppointment;
      private Common grImportAmPmInd;
      private Common grImportCdvlReason;
      private Common grImportCdvlType;
      private Common grImportCdvlResult;
      private Office grOffice;
      private ServiceProvider grServiceProvider;
      private Common grImportServiceProvider;
    }

    /// <summary>A HiddenCheckGroup group.</summary>
    [Serializable]
    public class HiddenCheckGroup
    {
      /// <summary>
      /// A value of GrImportHiddenCheckCase.
      /// </summary>
      [JsonPropertyName("grImportHiddenCheckCase")]
      public Case1 GrImportHiddenCheckCase
      {
        get => grImportHiddenCheckCase ??= new();
        set => grImportHiddenCheckCase = value;
      }

      /// <summary>
      /// A value of GrImportHiddenCheckCsePerson.
      /// </summary>
      [JsonPropertyName("grImportHiddenCheckCsePerson")]
      public CsePerson GrImportHiddenCheckCsePerson
      {
        get => grImportHiddenCheckCsePerson ??= new();
        set => grImportHiddenCheckCsePerson = value;
      }

      /// <summary>
      /// A value of GrHidden.
      /// </summary>
      [JsonPropertyName("grHidden")]
      public CaseRole GrHidden
      {
        get => grHidden ??= new();
        set => grHidden = value;
      }

      /// <summary>
      /// A value of GrImportHiddenCheckAppointment.
      /// </summary>
      [JsonPropertyName("grImportHiddenCheckAppointment")]
      public Appointment GrImportHiddenCheckAppointment
      {
        get => grImportHiddenCheckAppointment ??= new();
        set => grImportHiddenCheckAppointment = value;
      }

      /// <summary>
      /// A value of GrImportHiddenAmPmInd.
      /// </summary>
      [JsonPropertyName("grImportHiddenAmPmInd")]
      public Common GrImportHiddenAmPmInd
      {
        get => grImportHiddenAmPmInd ??= new();
        set => grImportHiddenAmPmInd = value;
      }

      /// <summary>
      /// A value of GrImportHiddenCheckOffice.
      /// </summary>
      [JsonPropertyName("grImportHiddenCheckOffice")]
      public Office GrImportHiddenCheckOffice
      {
        get => grImportHiddenCheckOffice ??= new();
        set => grImportHiddenCheckOffice = value;
      }

      /// <summary>
      /// A value of GrImportHiddenCheckServiceProvider.
      /// </summary>
      [JsonPropertyName("grImportHiddenCheckServiceProvider")]
      public ServiceProvider GrImportHiddenCheckServiceProvider
      {
        get => grImportHiddenCheckServiceProvider ??= new();
        set => grImportHiddenCheckServiceProvider = value;
      }

      /// <summary>
      /// A value of GrImportHiddenCheckOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("grImportHiddenCheckOfficeServiceProvider")]
      public OfficeServiceProvider GrImportHiddenCheckOfficeServiceProvider
      {
        get => grImportHiddenCheckOfficeServiceProvider ??= new();
        set => grImportHiddenCheckOfficeServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Case1 grImportHiddenCheckCase;
      private CsePerson grImportHiddenCheckCsePerson;
      private CaseRole grHidden;
      private Appointment grImportHiddenCheckAppointment;
      private Common grImportHiddenAmPmInd;
      private Office grImportHiddenCheckOffice;
      private ServiceProvider grImportHiddenCheckServiceProvider;
      private OfficeServiceProvider grImportHiddenCheckOfficeServiceProvider;
    }

    /// <summary>A Imp3Group group.</summary>
    [Serializable]
    public class Imp3Group
    {
      /// <summary>
      /// A value of Imp3LastReadHidden.
      /// </summary>
      [JsonPropertyName("imp3LastReadHidden")]
      public Appointment Imp3LastReadHidden
      {
        get => imp3LastReadHidden ??= new();
        set => imp3LastReadHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Appointment imp3LastReadHidden;
    }

    /// <summary>
    /// A value of Ret.
    /// </summary>
    [JsonPropertyName("ret")]
    public Document Ret
    {
      get => ret ??= new();
      set => ret = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Common HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of HiddenPageCount.
    /// </summary>
    [JsonPropertyName("hiddenPageCount")]
    public Standard HiddenPageCount
    {
      get => hiddenPageCount ??= new();
      set => hiddenPageCount = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckCsePerson.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckCsePerson")]
    public CsePerson SearchHiddenCheckCsePerson
    {
      get => searchHiddenCheckCsePerson ??= new();
      set => searchHiddenCheckCsePerson = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckCase.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckCase")]
    public Case1 SearchHiddenCheckCase
    {
      get => searchHiddenCheckCase ??= new();
      set => searchHiddenCheckCase = value;
    }

    /// <summary>
    /// A value of DisplayAllHiddenCheck.
    /// </summary>
    [JsonPropertyName("displayAllHiddenCheck")]
    public Common DisplayAllHiddenCheck
    {
      get => displayAllHiddenCheck ??= new();
      set => displayAllHiddenCheck = value;
    }

    /// <summary>
    /// A value of StartingHiddenCheck.
    /// </summary>
    [JsonPropertyName("startingHiddenCheck")]
    public DateWorkArea StartingHiddenCheck
    {
      get => startingHiddenCheck ??= new();
      set => startingHiddenCheck = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckOfficeServiceProvider")]
    public OfficeServiceProvider SearchHiddenCheckOfficeServiceProvider
    {
      get => searchHiddenCheckOfficeServiceProvider ??= new();
      set => searchHiddenCheckOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckOffice.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckOffice")]
    public Office SearchHiddenCheckOffice
    {
      get => searchHiddenCheckOffice ??= new();
      set => searchHiddenCheckOffice = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckServiceProvider.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckServiceProvider")]
    public ServiceProvider SearchHiddenCheckServiceProvider
    {
      get => searchHiddenCheckServiceProvider ??= new();
      set => searchHiddenCheckServiceProvider = value;
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
    /// Gets a value of HiddenCheck.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenCheckGroup> HiddenCheck => hiddenCheck ??= new(
      HiddenCheckGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenCheck for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    [Computed]
    public IList<HiddenCheckGroup> HiddenCheck_Json
    {
      get => hiddenCheck;
      set => HiddenCheck.Assign(value);
    }

    /// <summary>
    /// A value of ReturnCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("returnCsePersonsWorkSet")]
    public CsePersonsWorkSet ReturnCsePersonsWorkSet
    {
      get => returnCsePersonsWorkSet ??= new();
      set => returnCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ReturnOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("returnOfficeServiceProvider")]
    public OfficeServiceProvider ReturnOfficeServiceProvider
    {
      get => returnOfficeServiceProvider ??= new();
      set => returnOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ReturnServiceProvider.
    /// </summary>
    [JsonPropertyName("returnServiceProvider")]
    public ServiceProvider ReturnServiceProvider
    {
      get => returnServiceProvider ??= new();
      set => returnServiceProvider = value;
    }

    /// <summary>
    /// A value of ReturnOffice.
    /// </summary>
    [JsonPropertyName("returnOffice")]
    public Office ReturnOffice
    {
      get => returnOffice ??= new();
      set => returnOffice = value;
    }

    /// <summary>
    /// A value of ReturnCase.
    /// </summary>
    [JsonPropertyName("returnCase")]
    public Case1 ReturnCase
    {
      get => returnCase ??= new();
      set => returnCase = value;
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
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of FormattedSrvcprvdrName.
    /// </summary>
    [JsonPropertyName("formattedSrvcprvdrName")]
    public WorkArea FormattedSrvcprvdrName
    {
      get => formattedSrvcprvdrName ??= new();
      set => formattedSrvcprvdrName = value;
    }

    /// <summary>
    /// A value of DisplayAll.
    /// </summary>
    [JsonPropertyName("displayAll")]
    public Common DisplayAll
    {
      get => displayAll ??= new();
      set => displayAll = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public Common ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Comp.
    /// </summary>
    [JsonPropertyName("comp")]
    public Common Comp
    {
      get => comp ??= new();
      set => comp = value;
    }

    /// <summary>
    /// A value of SearchOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("searchOfficeServiceProvider")]
    public OfficeServiceProvider SearchOfficeServiceProvider
    {
      get => searchOfficeServiceProvider ??= new();
      set => searchOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchServiceProvider.
    /// </summary>
    [JsonPropertyName("searchServiceProvider")]
    public ServiceProvider SearchServiceProvider
    {
      get => searchServiceProvider ??= new();
      set => searchServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchOffice.
    /// </summary>
    [JsonPropertyName("searchOffice")]
    public Office SearchOffice
    {
      get => searchOffice ??= new();
      set => searchOffice = value;
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
    /// A value of PassCodeValue.
    /// </summary>
    [JsonPropertyName("passCodeValue")]
    public CodeValue PassCodeValue
    {
      get => passCodeValue ??= new();
      set => passCodeValue = value;
    }

    /// <summary>
    /// A value of PassCode.
    /// </summary>
    [JsonPropertyName("passCode")]
    public Code PassCode
    {
      get => passCode ??= new();
      set => passCode = value;
    }

    /// <summary>
    /// Gets a value of Imp3.
    /// </summary>
    [JsonIgnore]
    public Array<Imp3Group> Imp3 => imp3 ??= new(Imp3Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Imp3 for json serialization.
    /// </summary>
    [JsonPropertyName("imp3")]
    [Computed]
    public IList<Imp3Group> Imp3_Json
    {
      get => imp3;
      set => Imp3.Assign(value);
    }

    private Document ret;
    private Common hiddenPrev;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard hiddenPageCount;
    private CsePerson searchHiddenCheckCsePerson;
    private Case1 searchHiddenCheckCase;
    private Common displayAllHiddenCheck;
    private DateWorkArea startingHiddenCheck;
    private OfficeServiceProvider searchHiddenCheckOfficeServiceProvider;
    private Office searchHiddenCheckOffice;
    private ServiceProvider searchHiddenCheckServiceProvider;
    private Array<ImportGroup> import1;
    private Array<HiddenCheckGroup> hiddenCheck;
    private CsePersonsWorkSet returnCsePersonsWorkSet;
    private OfficeServiceProvider returnOfficeServiceProvider;
    private ServiceProvider returnServiceProvider;
    private Office returnOffice;
    private Case1 returnCase;
    private CsePerson searchCsePerson;
    private Case1 searchCase;
    private WorkArea formattedSrvcprvdrName;
    private Common displayAll;
    private DateWorkArea starting;
    private Common serviceProvider;
    private Common comp;
    private OfficeServiceProvider searchOfficeServiceProvider;
    private ServiceProvider searchServiceProvider;
    private Office searchOffice;
    private NextTranInfo hidden;
    private Standard standard;
    private CodeValue passCodeValue;
    private Code passCode;
    private Array<Imp3Group> imp3;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PassGroup group.</summary>
    [Serializable]
    public class PassGroup
    {
      /// <summary>
      /// A value of GrExportPassCsePerson.
      /// </summary>
      [JsonPropertyName("grExportPassCsePerson")]
      public CsePerson GrExportPassCsePerson
      {
        get => grExportPassCsePerson ??= new();
        set => grExportPassCsePerson = value;
      }

      /// <summary>
      /// A value of GrExportPassAppointment.
      /// </summary>
      [JsonPropertyName("grExportPassAppointment")]
      public Appointment GrExportPassAppointment
      {
        get => grExportPassAppointment ??= new();
        set => grExportPassAppointment = value;
      }

      /// <summary>
      /// A value of GrExportPassOffice.
      /// </summary>
      [JsonPropertyName("grExportPassOffice")]
      public Office GrExportPassOffice
      {
        get => grExportPassOffice ??= new();
        set => grExportPassOffice = value;
      }

      /// <summary>
      /// A value of GrExportPassServiceProvider.
      /// </summary>
      [JsonPropertyName("grExportPassServiceProvider")]
      public ServiceProvider GrExportPassServiceProvider
      {
        get => grExportPassServiceProvider ??= new();
        set => grExportPassServiceProvider = value;
      }

      /// <summary>
      /// A value of GrExportPassOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("grExportPassOfficeServiceProvider")]
      public OfficeServiceProvider GrExportPassOfficeServiceProvider
      {
        get => grExportPassOfficeServiceProvider ??= new();
        set => grExportPassOfficeServiceProvider = value;
      }

      private CsePerson grExportPassCsePerson;
      private Appointment grExportPassAppointment;
      private Office grExportPassOffice;
      private ServiceProvider grExportPassServiceProvider;
      private OfficeServiceProvider grExportPassOfficeServiceProvider;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GexportHiddenPageKeys.
      /// </summary>
      [JsonPropertyName("gexportHiddenPageKeys")]
      public Appointment GexportHiddenPageKeys
      {
        get => gexportHiddenPageKeys ??= new();
        set => gexportHiddenPageKeys = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private Appointment gexportHiddenPageKeys;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of GrOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("grOfficeServiceProvider")]
      public OfficeServiceProvider GrOfficeServiceProvider
      {
        get => grOfficeServiceProvider ??= new();
        set => grOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of GrExportLineSelect.
      /// </summary>
      [JsonPropertyName("grExportLineSelect")]
      public Common GrExportLineSelect
      {
        get => grExportLineSelect ??= new();
        set => grExportLineSelect = value;
      }

      /// <summary>
      /// A value of GrCase.
      /// </summary>
      [JsonPropertyName("grCase")]
      public Case1 GrCase
      {
        get => grCase ??= new();
        set => grCase = value;
      }

      /// <summary>
      /// A value of GrCsePerson.
      /// </summary>
      [JsonPropertyName("grCsePerson")]
      public CsePerson GrCsePerson
      {
        get => grCsePerson ??= new();
        set => grCsePerson = value;
      }

      /// <summary>
      /// A value of GrExportCsePerson.
      /// </summary>
      [JsonPropertyName("grExportCsePerson")]
      public Common GrExportCsePerson
      {
        get => grExportCsePerson ??= new();
        set => grExportCsePerson = value;
      }

      /// <summary>
      /// A value of GrCaseRole.
      /// </summary>
      [JsonPropertyName("grCaseRole")]
      public CaseRole GrCaseRole
      {
        get => grCaseRole ??= new();
        set => grCaseRole = value;
      }

      /// <summary>
      /// A value of GrAppointment.
      /// </summary>
      [JsonPropertyName("grAppointment")]
      public Appointment GrAppointment
      {
        get => grAppointment ??= new();
        set => grAppointment = value;
      }

      /// <summary>
      /// A value of GrExportAmPmInd.
      /// </summary>
      [JsonPropertyName("grExportAmPmInd")]
      public Common GrExportAmPmInd
      {
        get => grExportAmPmInd ??= new();
        set => grExportAmPmInd = value;
      }

      /// <summary>
      /// A value of GrExportCdvlReason.
      /// </summary>
      [JsonPropertyName("grExportCdvlReason")]
      public Common GrExportCdvlReason
      {
        get => grExportCdvlReason ??= new();
        set => grExportCdvlReason = value;
      }

      /// <summary>
      /// A value of GrExportCdvlType.
      /// </summary>
      [JsonPropertyName("grExportCdvlType")]
      public Common GrExportCdvlType
      {
        get => grExportCdvlType ??= new();
        set => grExportCdvlType = value;
      }

      /// <summary>
      /// A value of GrExportCdvlResult.
      /// </summary>
      [JsonPropertyName("grExportCdvlResult")]
      public Common GrExportCdvlResult
      {
        get => grExportCdvlResult ??= new();
        set => grExportCdvlResult = value;
      }

      /// <summary>
      /// A value of GrOffice.
      /// </summary>
      [JsonPropertyName("grOffice")]
      public Office GrOffice
      {
        get => grOffice ??= new();
        set => grOffice = value;
      }

      /// <summary>
      /// A value of GrServiceProvider.
      /// </summary>
      [JsonPropertyName("grServiceProvider")]
      public ServiceProvider GrServiceProvider
      {
        get => grServiceProvider ??= new();
        set => grServiceProvider = value;
      }

      /// <summary>
      /// A value of GrExportServiceProvider.
      /// </summary>
      [JsonPropertyName("grExportServiceProvider")]
      public Common GrExportServiceProvider
      {
        get => grExportServiceProvider ??= new();
        set => grExportServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private OfficeServiceProvider grOfficeServiceProvider;
      private Common grExportLineSelect;
      private Case1 grCase;
      private CsePerson grCsePerson;
      private Common grExportCsePerson;
      private CaseRole grCaseRole;
      private Appointment grAppointment;
      private Common grExportAmPmInd;
      private Common grExportCdvlReason;
      private Common grExportCdvlType;
      private Common grExportCdvlResult;
      private Office grOffice;
      private ServiceProvider grServiceProvider;
      private Common grExportServiceProvider;
    }

    /// <summary>A HiddenCheckGroup group.</summary>
    [Serializable]
    public class HiddenCheckGroup
    {
      /// <summary>
      /// A value of GrExportHiddenCheckCase.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckCase")]
      public Case1 GrExportHiddenCheckCase
      {
        get => grExportHiddenCheckCase ??= new();
        set => grExportHiddenCheckCase = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckCsePerson.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckCsePerson")]
      public CsePerson GrExportHiddenCheckCsePerson
      {
        get => grExportHiddenCheckCsePerson ??= new();
        set => grExportHiddenCheckCsePerson = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckAppointment.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckAppointment")]
      public Appointment GrExportHiddenCheckAppointment
      {
        get => grExportHiddenCheckAppointment ??= new();
        set => grExportHiddenCheckAppointment = value;
      }

      /// <summary>
      /// A value of GrExportHiddenAmPmInd.
      /// </summary>
      [JsonPropertyName("grExportHiddenAmPmInd")]
      public Common GrExportHiddenAmPmInd
      {
        get => grExportHiddenAmPmInd ??= new();
        set => grExportHiddenAmPmInd = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckOffice.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckOffice")]
      public Office GrExportHiddenCheckOffice
      {
        get => grExportHiddenCheckOffice ??= new();
        set => grExportHiddenCheckOffice = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckServiceProvider.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckServiceProvider")]
      public ServiceProvider GrExportHiddenCheckServiceProvider
      {
        get => grExportHiddenCheckServiceProvider ??= new();
        set => grExportHiddenCheckServiceProvider = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckOfficeServiceProvider")]
      public OfficeServiceProvider GrExportHiddenCheckOfficeServiceProvider
      {
        get => grExportHiddenCheckOfficeServiceProvider ??= new();
        set => grExportHiddenCheckOfficeServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Case1 grExportHiddenCheckCase;
      private CsePerson grExportHiddenCheckCsePerson;
      private Appointment grExportHiddenCheckAppointment;
      private Common grExportHiddenAmPmInd;
      private Office grExportHiddenCheckOffice;
      private ServiceProvider grExportHiddenCheckServiceProvider;
      private OfficeServiceProvider grExportHiddenCheckOfficeServiceProvider;
    }

    /// <summary>A Exp3Group group.</summary>
    [Serializable]
    public class Exp3Group
    {
      /// <summary>
      /// A value of Exp3LastReadHidden.
      /// </summary>
      [JsonPropertyName("exp3LastReadHidden")]
      public Appointment Exp3LastReadHidden
      {
        get => exp3LastReadHidden ??= new();
        set => exp3LastReadHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Appointment exp3LastReadHidden;
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
    /// A value of ToDocmFilter.
    /// </summary>
    [JsonPropertyName("toDocmFilter")]
    public Document ToDocmFilter
    {
      get => toDocmFilter ??= new();
      set => toDocmFilter = value;
    }

    /// <summary>
    /// A value of GrExportPass.
    /// </summary>
    [JsonPropertyName("grExportPass")]
    public Case1 GrExportPass
    {
      get => grExportPass ??= new();
      set => grExportPass = value;
    }

    /// <summary>
    /// Gets a value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public PassGroup Pass
    {
      get => pass ?? (pass = new());
      set => pass = value;
    }

    /// <summary>
    /// A value of PassInfrastructure.
    /// </summary>
    [JsonPropertyName("passInfrastructure")]
    public Infrastructure PassInfrastructure
    {
      get => passInfrastructure ??= new();
      set => passInfrastructure = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Common HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of HiddenPageCount.
    /// </summary>
    [JsonPropertyName("hiddenPageCount")]
    public Standard HiddenPageCount
    {
      get => hiddenPageCount ??= new();
      set => hiddenPageCount = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckCsePerson.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckCsePerson")]
    public CsePerson SearchHiddenCheckCsePerson
    {
      get => searchHiddenCheckCsePerson ??= new();
      set => searchHiddenCheckCsePerson = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckCase.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckCase")]
    public Case1 SearchHiddenCheckCase
    {
      get => searchHiddenCheckCase ??= new();
      set => searchHiddenCheckCase = value;
    }

    /// <summary>
    /// A value of DiplayAllHiddenCheck.
    /// </summary>
    [JsonPropertyName("diplayAllHiddenCheck")]
    public Common DiplayAllHiddenCheck
    {
      get => diplayAllHiddenCheck ??= new();
      set => diplayAllHiddenCheck = value;
    }

    /// <summary>
    /// A value of StartingHiddenCheck.
    /// </summary>
    [JsonPropertyName("startingHiddenCheck")]
    public DateWorkArea StartingHiddenCheck
    {
      get => startingHiddenCheck ??= new();
      set => startingHiddenCheck = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckOfficeServiceProvider")]
    public OfficeServiceProvider SearchHiddenCheckOfficeServiceProvider
    {
      get => searchHiddenCheckOfficeServiceProvider ??= new();
      set => searchHiddenCheckOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckOffice.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckOffice")]
    public Office SearchHiddenCheckOffice
    {
      get => searchHiddenCheckOffice ??= new();
      set => searchHiddenCheckOffice = value;
    }

    /// <summary>
    /// A value of SearchHiddenCheckServiceProvider.
    /// </summary>
    [JsonPropertyName("searchHiddenCheckServiceProvider")]
    public ServiceProvider SearchHiddenCheckServiceProvider
    {
      get => searchHiddenCheckServiceProvider ??= new();
      set => searchHiddenCheckServiceProvider = value;
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
    /// Gets a value of HiddenCheck.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenCheckGroup> HiddenCheck => hiddenCheck ??= new(
      HiddenCheckGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenCheck for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    [Computed]
    public IList<HiddenCheckGroup> HiddenCheck_Json
    {
      get => hiddenCheck;
      set => HiddenCheck.Assign(value);
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
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
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
    /// A value of FormattedSrvcprvdrName.
    /// </summary>
    [JsonPropertyName("formattedSrvcprvdrName")]
    public WorkArea FormattedSrvcprvdrName
    {
      get => formattedSrvcprvdrName ??= new();
      set => formattedSrvcprvdrName = value;
    }

    /// <summary>
    /// A value of DisplayAll.
    /// </summary>
    [JsonPropertyName("displayAll")]
    public Common DisplayAll
    {
      get => displayAll ??= new();
      set => displayAll = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public Common ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Comp.
    /// </summary>
    [JsonPropertyName("comp")]
    public Common Comp
    {
      get => comp ??= new();
      set => comp = value;
    }

    /// <summary>
    /// A value of SearchOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("searchOfficeServiceProvider")]
    public OfficeServiceProvider SearchOfficeServiceProvider
    {
      get => searchOfficeServiceProvider ??= new();
      set => searchOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchOffice.
    /// </summary>
    [JsonPropertyName("searchOffice")]
    public Office SearchOffice
    {
      get => searchOffice ??= new();
      set => searchOffice = value;
    }

    /// <summary>
    /// A value of SearchServiceProvider.
    /// </summary>
    [JsonPropertyName("searchServiceProvider")]
    public ServiceProvider SearchServiceProvider
    {
      get => searchServiceProvider ??= new();
      set => searchServiceProvider = value;
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
    /// A value of PassCode.
    /// </summary>
    [JsonPropertyName("passCode")]
    public Code PassCode
    {
      get => passCode ??= new();
      set => passCode = value;
    }

    /// <summary>
    /// Gets a value of Exp3.
    /// </summary>
    [JsonIgnore]
    public Array<Exp3Group> Exp3 => exp3 ??= new(Exp3Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Exp3 for json serialization.
    /// </summary>
    [JsonPropertyName("exp3")]
    [Computed]
    public IList<Exp3Group> Exp3_Json
    {
      get => exp3;
      set => Exp3.Assign(value);
    }

    private Common docmProtectFilter;
    private Document toDocmFilter;
    private Case1 grExportPass;
    private PassGroup pass;
    private Infrastructure passInfrastructure;
    private Common hiddenPrev;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard hiddenPageCount;
    private CsePerson searchHiddenCheckCsePerson;
    private Case1 searchHiddenCheckCase;
    private Common diplayAllHiddenCheck;
    private DateWorkArea startingHiddenCheck;
    private OfficeServiceProvider searchHiddenCheckOfficeServiceProvider;
    private Office searchHiddenCheckOffice;
    private ServiceProvider searchHiddenCheckServiceProvider;
    private Array<ExportGroup> export1;
    private Array<HiddenCheckGroup> hiddenCheck;
    private CsePerson searchCsePerson;
    private Case1 searchCase;
    private Standard standard;
    private WorkArea formattedSrvcprvdrName;
    private Common displayAll;
    private DateWorkArea starting;
    private Common serviceProvider;
    private Common comp;
    private OfficeServiceProvider searchOfficeServiceProvider;
    private Office searchOffice;
    private ServiceProvider searchServiceProvider;
    private NextTranInfo hidden;
    private Code passCode;
    private Array<Exp3Group> exp3;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A PredSuccCheckGroup group.</summary>
    [Serializable]
    public class PredSuccCheckGroup
    {
      /// <summary>
      /// A value of GrLocalPredSuccCheckCsePerson.
      /// </summary>
      [JsonPropertyName("grLocalPredSuccCheckCsePerson")]
      public CsePerson GrLocalPredSuccCheckCsePerson
      {
        get => grLocalPredSuccCheckCsePerson ??= new();
        set => grLocalPredSuccCheckCsePerson = value;
      }

      /// <summary>
      /// A value of GrLocalPredSuccCheckCase.
      /// </summary>
      [JsonPropertyName("grLocalPredSuccCheckCase")]
      public Case1 GrLocalPredSuccCheckCase
      {
        get => grLocalPredSuccCheckCase ??= new();
        set => grLocalPredSuccCheckCase = value;
      }

      private CsePerson grLocalPredSuccCheckCsePerson;
      private Case1 grLocalPredSuccCheckCase;
    }

    /// <summary>A PassGroup group.</summary>
    [Serializable]
    public class PassGroup
    {
      /// <summary>
      /// A value of GrLocalPassOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("grLocalPassOfficeServiceProvider")]
      public OfficeServiceProvider GrLocalPassOfficeServiceProvider
      {
        get => grLocalPassOfficeServiceProvider ??= new();
        set => grLocalPassOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of GrLocalPassCase.
      /// </summary>
      [JsonPropertyName("grLocalPassCase")]
      public Case1 GrLocalPassCase
      {
        get => grLocalPassCase ??= new();
        set => grLocalPassCase = value;
      }

      /// <summary>
      /// A value of GrLocalPassCsePerson.
      /// </summary>
      [JsonPropertyName("grLocalPassCsePerson")]
      public CsePerson GrLocalPassCsePerson
      {
        get => grLocalPassCsePerson ??= new();
        set => grLocalPassCsePerson = value;
      }

      /// <summary>
      /// A value of GrLocalPassAppointment.
      /// </summary>
      [JsonPropertyName("grLocalPassAppointment")]
      public Appointment GrLocalPassAppointment
      {
        get => grLocalPassAppointment ??= new();
        set => grLocalPassAppointment = value;
      }

      /// <summary>
      /// A value of PassApptAmPmInd.
      /// </summary>
      [JsonPropertyName("passApptAmPmInd")]
      public Common PassApptAmPmInd
      {
        get => passApptAmPmInd ??= new();
        set => passApptAmPmInd = value;
      }

      /// <summary>
      /// A value of GrLocalPassOffice.
      /// </summary>
      [JsonPropertyName("grLocalPassOffice")]
      public Office GrLocalPassOffice
      {
        get => grLocalPassOffice ??= new();
        set => grLocalPassOffice = value;
      }

      /// <summary>
      /// A value of GrLocalPassServiceProvider.
      /// </summary>
      [JsonPropertyName("grLocalPassServiceProvider")]
      public ServiceProvider GrLocalPassServiceProvider
      {
        get => grLocalPassServiceProvider ??= new();
        set => grLocalPassServiceProvider = value;
      }

      private OfficeServiceProvider grLocalPassOfficeServiceProvider;
      private Case1 grLocalPassCase;
      private CsePerson grLocalPassCsePerson;
      private Appointment grLocalPassAppointment;
      private Common passApptAmPmInd;
      private Office grLocalPassOffice;
      private ServiceProvider grLocalPassServiceProvider;
    }

    /// <summary>
    /// A value of OffcText.
    /// </summary>
    [JsonPropertyName("offcText")]
    public SpTextWorkArea OffcText
    {
      get => offcText ??= new();
      set => offcText = value;
    }

    /// <summary>
    /// A value of Holding1Case.
    /// </summary>
    [JsonPropertyName("holding1Case")]
    public Case1 Holding1Case
    {
      get => holding1Case ??= new();
      set => holding1Case = value;
    }

    /// <summary>
    /// A value of Holding1CsePerson.
    /// </summary>
    [JsonPropertyName("holding1CsePerson")]
    public CsePerson Holding1CsePerson
    {
      get => holding1CsePerson ??= new();
      set => holding1CsePerson = value;
    }

    /// <summary>
    /// A value of Holding2Case.
    /// </summary>
    [JsonPropertyName("holding2Case")]
    public Case1 Holding2Case
    {
      get => holding2Case ??= new();
      set => holding2Case = value;
    }

    /// <summary>
    /// A value of Holding2CsePerson.
    /// </summary>
    [JsonPropertyName("holding2CsePerson")]
    public CsePerson Holding2CsePerson
    {
      get => holding2CsePerson ??= new();
      set => holding2CsePerson = value;
    }

    /// <summary>
    /// A value of PointerPred.
    /// </summary>
    [JsonPropertyName("pointerPred")]
    public Common PointerPred
    {
      get => pointerPred ??= new();
      set => pointerPred = value;
    }

    /// <summary>
    /// A value of Predecessor.
    /// </summary>
    [JsonPropertyName("predecessor")]
    public Appointment Predecessor
    {
      get => predecessor ??= new();
      set => predecessor = value;
    }

    /// <summary>
    /// A value of Successor.
    /// </summary>
    [JsonPropertyName("successor")]
    public Appointment Successor
    {
      get => successor ??= new();
      set => successor = value;
    }

    /// <summary>
    /// A value of PointerSucc.
    /// </summary>
    [JsonPropertyName("pointerSucc")]
    public Common PointerSucc
    {
      get => pointerSucc ??= new();
      set => pointerSucc = value;
    }

    /// <summary>
    /// A value of PtypCnt.
    /// </summary>
    [JsonPropertyName("ptypCnt")]
    public Common PtypCnt
    {
      get => ptypCnt ??= new();
      set => ptypCnt = value;
    }

    /// <summary>
    /// A value of PresCnt.
    /// </summary>
    [JsonPropertyName("presCnt")]
    public Common PresCnt
    {
      get => presCnt ??= new();
      set => presCnt = value;
    }

    /// <summary>
    /// A value of PspCnt.
    /// </summary>
    [JsonPropertyName("pspCnt")]
    public Common PspCnt
    {
      get => pspCnt ??= new();
      set => pspCnt = value;
    }

    /// <summary>
    /// A value of PrsnCnt.
    /// </summary>
    [JsonPropertyName("prsnCnt")]
    public Common PrsnCnt
    {
      get => prsnCnt ??= new();
      set => prsnCnt = value;
    }

    /// <summary>
    /// A value of PcsePerCnt.
    /// </summary>
    [JsonPropertyName("pcsePerCnt")]
    public Common PcsePerCnt
    {
      get => pcsePerCnt ??= new();
      set => pcsePerCnt = value;
    }

    /// <summary>
    /// A value of LineSelCnt.
    /// </summary>
    [JsonPropertyName("lineSelCnt")]
    public Common LineSelCnt
    {
      get => lineSelCnt ??= new();
      set => lineSelCnt = value;
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
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    /// <summary>
    /// A value of Weekday.
    /// </summary>
    [JsonPropertyName("weekday")]
    public TextWorkArea Weekday
    {
      get => weekday ??= new();
      set => weekday = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
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
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
    }

    /// <summary>
    /// Gets a value of PredSuccCheck.
    /// </summary>
    [JsonPropertyName("predSuccCheck")]
    public PredSuccCheckGroup PredSuccCheck
    {
      get => predSuccCheck ?? (predSuccCheck = new());
      set => predSuccCheck = value;
    }

    /// <summary>
    /// A value of PredSuccCount.
    /// </summary>
    [JsonPropertyName("predSuccCount")]
    public Common PredSuccCount
    {
      get => predSuccCount ??= new();
      set => predSuccCount = value;
    }

    /// <summary>
    /// A value of Holding2Appointment.
    /// </summary>
    [JsonPropertyName("holding2Appointment")]
    public Appointment Holding2Appointment
    {
      get => holding2Appointment ??= new();
      set => holding2Appointment = value;
    }

    /// <summary>
    /// A value of Holding1Appointment.
    /// </summary>
    [JsonPropertyName("holding1Appointment")]
    public Appointment Holding1Appointment
    {
      get => holding1Appointment ??= new();
      set => holding1Appointment = value;
    }

    /// <summary>
    /// Gets a value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public PassGroup Pass
    {
      get => pass ?? (pass = new());
      set => pass = value;
    }

    /// <summary>
    /// A value of PageKeys.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    public Appointment PageKeys
    {
      get => pageKeys ??= new();
      set => pageKeys = value;
    }

    /// <summary>
    /// A value of PageCount.
    /// </summary>
    [JsonPropertyName("pageCount")]
    public Standard PageCount
    {
      get => pageCount ??= new();
      set => pageCount = value;
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
    /// A value of PromptSelCnt.
    /// </summary>
    [JsonPropertyName("promptSelCnt")]
    public Common PromptSelCnt
    {
      get => promptSelCnt ??= new();
      set => promptSelCnt = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public ServiceProvider Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public NextTranInfo Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public Appointment Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
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
    /// A value of Inf1.
    /// </summary>
    [JsonPropertyName("inf1")]
    public Case1 Inf1
    {
      get => inf1 ??= new();
      set => inf1 = value;
    }

    /// <summary>
    /// A value of Inf2.
    /// </summary>
    [JsonPropertyName("inf2")]
    public CsePerson Inf2
    {
      get => inf2 ??= new();
      set => inf2 = value;
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
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
    }

    /// <summary>
    /// A value of DetailText30.
    /// </summary>
    [JsonPropertyName("detailText30")]
    public TextWorkArea DetailText30
    {
      get => detailText30 ??= new();
      set => detailText30 = value;
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
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    private SpTextWorkArea offcText;
    private Case1 holding1Case;
    private CsePerson holding1CsePerson;
    private Case1 holding2Case;
    private CsePerson holding2CsePerson;
    private Common pointerPred;
    private Appointment predecessor;
    private Appointment successor;
    private Common pointerSucc;
    private Common ptypCnt;
    private Common presCnt;
    private Common pspCnt;
    private Common prsnCnt;
    private Common pcsePerCnt;
    private Common lineSelCnt;
    private Common pointer;
    private Common counter;
    private TextWorkArea weekday;
    private WorkArea print;
    private Common position;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private DateWorkArea end;
    private DateWorkArea start;
    private DateWorkArea current;
    private SystemGenerated systemGenerated;
    private PredSuccCheckGroup predSuccCheck;
    private Common predSuccCount;
    private Appointment holding2Appointment;
    private Appointment holding1Appointment;
    private PassGroup pass;
    private Appointment pageKeys;
    private Standard pageCount;
    private DateWorkArea initialized;
    private Common promptSelCnt;
    private ServiceProvider read;
    private NextTranInfo null1;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Appointment blank;
    private Infrastructure infrastructure;
    private WorkArea raiseEventFlag;
    private Common numberOfEvents;
    private Case1 inf1;
    private CsePerson inf2;
    private DateWorkArea date;
    private TextWorkArea detailText10;
    private TextWorkArea detailText30;
    private Infrastructure lastTran;
    private Appointment appointment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Key.
    /// </summary>
    [JsonPropertyName("key")]
    public Infrastructure Key
    {
      get => key ??= new();
      set => key = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Successor.
    /// </summary>
    [JsonPropertyName("successor")]
    public Appointment Successor
    {
      get => successor ??= new();
      set => successor = value;
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
    /// A value of Predecessor.
    /// </summary>
    [JsonPropertyName("predecessor")]
    public Appointment Predecessor
    {
      get => predecessor ??= new();
      set => predecessor = value;
    }

    private Infrastructure key;
    private Case1 case1;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Office office;
    private Appointment successor;
    private Infrastructure infrastructure;
    private Appointment predecessor;
  }
#endregion
}
