// Program: SP_DMON_DOCUMENT_MONITOR, ID: 371925683, model: 746.
// Short name: SWEDMONP
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
/// A program: SP_DMON_DOCUMENT_MONITOR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpDmonDocumentMonitor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DMON_DOCUMENT_MONITOR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDmonDocumentMonitor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDmonDocumentMonitor.
  /// </summary>
  public SpDmonDocumentMonitor(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // -----------------------------------------------------------------------------------
    // 11/12/1996	Michael Ramirez			Initial Development
    // 02/06/1997	Rod Grey			Add validations on Select and move
    // 						to export for NATE
    // 03/06/1997	Siraj Konkader			Added logic for flow from HIST.
    // 04/15/1997	Siraj Konkader			Allow update if logged on user owns
    // 						monitored document or is at
    // 						a supervisory level.
    // 11/03/1998	M Ramirez			Post-assessment fixes
    // 12/30/1998	swsrkeh				Phase II changes
    // 11/18/1999	SWSRCHF		79800		Unprotected the Actual Response Date
    // 						for 'EMANCIPA' document
    // 12/20/1999	PMcElderry	PR 75671	added explicit view matching AB and
    // 						corresponding PRAD logic
    // 03/31/2000	M Ramirez			Removed UPDATE statement and added call
    // 						to sp_cab_update_monitored_document.
    // 						Also, since closure_date is not used
    // 						throughout the system, removed the
    // 						setting of it
    // 08/23/2000	M Ramirez	101309		Display based on OSP, not SP
    // -----------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.UseNate.NextTransaction = "DMON";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.UserId.UserId = global.UserId;
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // -----------------------
    // MOVE IMPORTS TO EXPORTS
    // -----------------------
    // mjr
    // ---------------------------------------------------
    // 08/23/2000
    // PR# 101309 - DMON is based on OSP not SP
    // Since the SP Name is removed from the screen this is not necessary
    // ----------------------------------------------------------------
    export.FilterInfrastructure.Assign(import.FilterInfrastructure);
    export.FilterDateWorkArea.Date = import.FilterDateWorkArea.Date;

    if (Equal(export.FilterDateWorkArea.Date, local.Initialized.Date))
    {
      export.FilterDateWorkArea.Date = Now().Date.AddMonths(-2);
    }

    export.FilterFipsTribAddress.Country = import.FilterFipsTribAddress.Country;
    MoveFips(import.FilterFips, export.FilterFips);
    export.FilterLegalAction.CourtCaseNumber =
      import.FilterLegalAction.CourtCaseNumber;
    export.FilterServiceProvider.UserId = import.FilterServiceProvider.UserId;
    MoveOfficeServiceProvider(import.FilterOfficeServiceProvider,
      export.FilterOfficeServiceProvider);
    export.FilterOffice.SystemGeneratedId =
      import.FilterOffice.SystemGeneratedId;
    export.FilterShowAll.Text1 = import.FilterShowAll.Text1;
    export.FilterSvpo.SelectChar = import.FilterSvpo.SelectChar;
    export.ErrorForScrollDisplay.Flag = import.ErrorForScrollDisplay.Flag;

    // ----------------------------------------------------------------------
    // Add check to see if filters changed if yes then a display is
    // required before any command other than a display can be
    // executed
    // ----------------------------------------------------------------------
    // ****  Validate All Commands ****
    switch(TrimEnd(global.Command))
    {
      case "DDOC":
        // ----------------------
        // No processing required
        // ----------------------
        break;
      case "DISPLAY":
        // ----------------------
        // No processing required
        // ----------------------
        break;
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          // The user requested a next tran action
          export.Hidden.CaseNumber = export.FilterInfrastructure.CaseNumber ?? ""
            ;
          export.Hidden.CsePersonNumber =
            export.FilterInfrastructure.CsePersonNumber ?? "";
          UseScCabNextTranPut();

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
      case "LIST":
        // ----------------------
        // No processing required
        // ----------------------
        break;
      case "LINK":
        // Coming from HIST with Export Filter Infrastructure Sys Gen Id 
        // populated.
        if (ReadMonitoredDocumentInfrastructure2())
        {
          export.FilterServiceProvider.UserId = entities.Infrastructure.UserId;
        }
        else
        {
          ExitState = "SP0000_MONITORED_DOCUMENT_NF";

          break;
        }

        // mjr
        // ---------------------------------------------------
        // 08/23/2000
        // PR# 101309 - DMON is based on OSP not SP
        // Get the office and OSP from the outgoing_document
        // ----------------------------------------------------------------
        if (!ReadServiceProvider())
        {
          ExitState = "SERVICE_PROVIDER_NF";

          break;
        }

        if (ReadOffice())
        {
          export.FilterOffice.SystemGeneratedId =
            entities.Office.SystemGeneratedId;
        }
        else
        {
          ExitState = "OFFICE_NF";

          break;
        }

        if (ReadOfficeServiceProvider())
        {
          MoveOfficeServiceProvider(entities.OfficeServiceProvider,
            export.FilterOfficeServiceProvider);
        }
        else
        {
          ExitState = "OFFICE_SERVICE_PROVIDER_NF";

          break;
        }

        global.Command = "DISPLAY";

        break;
      case "NATE":
        // ----------------------
        // No processing required
        // ----------------------
        break;
      case "NEXT":
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Update.GxsePersonsWorkSet.FormattedName =
            import.Import1.Item.GisePersonsWorkSet.FormattedName;
          MoveDocument(import.Import1.Item.Giocument,
            export.Export1.Update.Gxocument);
          export.Export1.Update.Gxnfrastructure.Assign(
            import.Import1.Item.Ginfrastructure);
          export.Export1.Update.GxegalAction.CourtCaseNumber =
            import.Import1.Item.GiegalAction.CourtCaseNumber;
          export.Export1.Update.GxonitoredDocument.Assign(
            import.Import1.Item.GionitoredDocument);
          export.Export1.Update.GxodeValue.SelectChar =
            import.Import1.Item.GiodeValue.SelectChar;
          export.Export1.Update.Gxrev.Assign(import.Import1.Item.Girev);
          export.Export1.Update.Gxommon.SelectChar =
            import.Import1.Item.Giommon.SelectChar;
          export.Export1.Update.Gxommon.SelectChar = "";
          export.Export1.Update.Gxommon.SelectChar = "";

          if (!IsEmpty(export.Export1.Item.GxonitoredDocument.ClosureReasonCode) &&
            AsChar
            (export.Export1.Item.GxonitoredDocument.ClosureReasonCode) == AsChar
            (export.Export1.Item.Gxrev.ClosureReasonCode) || Equal
            (export.Export1.Item.Gxocument.Name, "POSTMAST") || Equal
            (export.Export1.Item.Gxocument.Name, "EMPVERIF") || Equal
            (export.Export1.Item.Gxocument.Name, "EMANCIPA") || Equal
            (export.Export1.Item.Gxocument.Name, "EMPVERCO"))
          {
            // *** Problem report  H00079800
            // *** 11/18/99 SWSRCHF
            // *** start
            if (Equal(export.Export1.Item.Gxocument.Name, "EMANCIPA") && IsEmpty
              (export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
            {
              var field1 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "actualResponseDate");

              field1.Protected = false;

              var field2 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "closureReasonCode");

              field2.Protected = false;

              var field3 =
                GetField(export.Export1.Item.GxodeValue, "selectChar");

              field3.Protected = false;
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "actualResponseDate");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "closureReasonCode");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Export1.Item.GxodeValue, "selectChar");

              field3.Intensity = Intensity.Dark;
              field3.Protected = true;
            }

            // *** end
            // *** 11/18/99 SWSRCHF
            // *** Problem report  H00079800
          }

          if (!Lt(local.Current.Date,
            export.Export1.Item.GxonitoredDocument.RequiredResponseDate) && IsEmpty
            (export.Export1.Item.GxonitoredDocument.ClosureReasonCode) || Lt
            (export.Export1.Item.GxonitoredDocument.RequiredResponseDate,
            export.Export1.Item.GxonitoredDocument.ActualResponseDate) && !
            IsEmpty(export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
          {
            var field1 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "requiredResponseDate");

            field1.Color = "red";
            field1.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "requiredResponseDate");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          export.Export1.Next();
        }

        if (AsChar(export.ErrorForScrollDisplay.Flag) == 'Y')
        {
          ExitState = "FN0000_MUST_PERFORM_DISPLAY_1ST";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";
        }

        return;
      case "PREV":
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Update.GxsePersonsWorkSet.FormattedName =
            import.Import1.Item.GisePersonsWorkSet.FormattedName;
          MoveDocument(import.Import1.Item.Giocument,
            export.Export1.Update.Gxocument);
          export.Export1.Update.Gxnfrastructure.Assign(
            import.Import1.Item.Ginfrastructure);
          export.Export1.Update.GxegalAction.CourtCaseNumber =
            import.Import1.Item.GiegalAction.CourtCaseNumber;
          export.Export1.Update.GxonitoredDocument.Assign(
            import.Import1.Item.GionitoredDocument);
          export.Export1.Update.GxodeValue.SelectChar =
            import.Import1.Item.GiodeValue.SelectChar;
          export.Export1.Update.Gxrev.Assign(import.Import1.Item.Girev);
          export.Export1.Update.Gxommon.SelectChar =
            import.Import1.Item.Giommon.SelectChar;
          export.Export1.Update.Gxommon.SelectChar = "";
          export.Export1.Update.Gxommon.SelectChar = "";

          if (!IsEmpty(export.Export1.Item.GxonitoredDocument.ClosureReasonCode) &&
            AsChar
            (export.Export1.Item.GxonitoredDocument.ClosureReasonCode) == AsChar
            (export.Export1.Item.Gxrev.ClosureReasonCode) || Equal
            (export.Export1.Item.Gxocument.Name, "POSTMAST") || Equal
            (export.Export1.Item.Gxocument.Name, "EMPVERIF") || Equal
            (export.Export1.Item.Gxocument.Name, "EMANCIPA") || Equal
            (export.Export1.Item.Gxocument.Name, "EMPVERCO"))
          {
            // *** Problem report  H00079800
            // *** 11/18/99 SWSRCHF
            // *** start
            if (Equal(export.Export1.Item.Gxocument.Name, "EMANCIPA") && IsEmpty
              (export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
            {
              var field1 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "actualResponseDate");

              field1.Protected = false;

              var field2 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "closureReasonCode");

              field2.Protected = false;

              var field3 =
                GetField(export.Export1.Item.GxodeValue, "selectChar");

              field3.Protected = false;
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "actualResponseDate");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "closureReasonCode");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Export1.Item.GxodeValue, "selectChar");

              field3.Intensity = Intensity.Dark;
              field3.Protected = true;
            }

            // *** end
            // *** 11/18/99 SWSRCHF
            // *** Problem report  H00079800
          }

          if (!Lt(local.Current.Date,
            export.Export1.Item.GxonitoredDocument.RequiredResponseDate) && IsEmpty
            (export.Export1.Item.GxonitoredDocument.ClosureReasonCode) || Lt
            (export.Export1.Item.GxonitoredDocument.RequiredResponseDate,
            export.Export1.Item.GxonitoredDocument.ActualResponseDate) && !
            IsEmpty(export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
          {
            var field1 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "requiredResponseDate");

            field1.Color = "red";
            field1.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "requiredResponseDate");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          export.Export1.Next();
        }

        if (!IsEmpty(export.ErrorForScrollDisplay.Flag))
        {
          ExitState = "FN0000_MUST_PERFORM_DISPLAY_1ST";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";
        }

        return;
      case "RETCDVL":
        break;
      case "RETDDOC":
        // ----------------------
        // No processing required
        // ----------------------
        break;
      case "RETSVPO":
        // **** Returning from SVPO ****
        if (!IsEmpty(import.SelectedServiceProvider.UserId))
        {
          export.FilterServiceProvider.UserId =
            import.SelectedServiceProvider.UserId;

          // mjr
          // ---------------------------------------------------
          // 08/23/2000
          // PR# 101309 - DMON is based on OSP not SP
          // Get the office and OSP from the dialog flow
          // ----------------------------------------------------------------
          export.FilterOffice.SystemGeneratedId =
            import.SelectedOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(import.SelectedOfficeServiceProvider,
            export.FilterOfficeServiceProvider);
        }

        var field = GetField(export.FilterSvpo, "selectChar");

        field.Protected = false;
        field.Focused = true;

        global.Command = "DISPLAY";

        break;
      case "RTLIST":
        // **** Returning from Nate ****
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "UPDATE":
        // ----------------------
        // No processing required
        // ----------------------
        break;
      case "XXFMMENU":
        global.Command = "DISPLAY";

        break;
      case "XXNEXTXX":
        // The user is comming into this procedure on a next tran action.
        UseScCabNextTranGet();
        export.FilterInfrastructure.CaseNumber = export.Hidden.CaseNumber ?? "";
        export.FilterInfrastructure.CsePersonNumber =
          export.Hidden.CsePersonNumber ?? "";
        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // ------------------------------------------------------
      // The group view is going to be populated. So don't move
      // group imports to group exports.
      // ------------------------------------------------------
    }
    else
    {
      // **** Housekeeping - roll the import over to the export  ****
      local.SelectCount.Count = 0;

      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        ++local.TotalNumberOfRecords.Count;
        export.Export1.Update.GxsePersonsWorkSet.FormattedName =
          import.Import1.Item.GisePersonsWorkSet.FormattedName;
        MoveDocument(import.Import1.Item.Giocument,
          export.Export1.Update.Gxocument);
        export.Export1.Update.Gxnfrastructure.Assign(
          import.Import1.Item.Ginfrastructure);
        export.Export1.Update.GxegalAction.CourtCaseNumber =
          import.Import1.Item.GiegalAction.CourtCaseNumber;
        export.Export1.Update.GxonitoredDocument.Assign(
          import.Import1.Item.GionitoredDocument);
        export.Export1.Update.GxodeValue.SelectChar =
          import.Import1.Item.GiodeValue.SelectChar;
        export.Export1.Update.Gxrev.Assign(import.Import1.Item.Girev);
        export.Export1.Update.Gxommon.SelectChar =
          import.Import1.Item.Giommon.SelectChar;

        if (AsChar(export.Export1.Item.Gxommon.SelectChar) != 'S')
        {
          export.Export1.Next();

          continue;
        }

        switch(TrimEnd(global.Command))
        {
          case "RETCDVL":
            if (AsChar(export.Export1.Item.GxodeValue.SelectChar) == 'S')
            {
              if (!IsEmpty(import.DlgflwSelected.Cdvalue))
              {
                export.Export1.Update.GxonitoredDocument.ClosureReasonCode =
                  import.DlgflwSelected.Cdvalue;
              }

              export.Export1.Update.GxodeValue.SelectChar = "";

              var field3 =
                GetField(export.Export1.Item.GxodeValue, "selectChar");

              field3.Protected = false;
              field3.Focused = true;

              var field4 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "requiredResponseDate");

              field4.Color = "green";
              field4.Protected = false;
            }

            break;
          case "RTLIST":
            var field1 = GetField(export.Export1.Item.Gxommon, "selectChar");

            field1.Color = "green";
            field1.Protected = false;
            field1.Focused = true;

            break;
          case "RETDDOC":
            var field2 = GetField(export.Export1.Item.Gxommon, "selectChar");

            field2.Color = "green";
            field2.Protected = false;
            field2.Focused = true;

            break;
          default:
            break;
        }

        export.Export1.Next();
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "LIST") || Equal
        (global.Command, "UPDATE"))
      {
        // Validate action level security
        local.CsePersonsWorkSet.Number =
          export.FilterInfrastructure.CsePersonNumber ?? Spaces(10);
        local.Case1.Number = export.FilterInfrastructure.CaseNumber ?? Spaces
          (10);
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test4;
        }
      }

      // ****  Validate the Selections  ****
      if (Equal(global.Command, "DDOC") || Equal(global.Command, "LIST") || Equal
        (global.Command, "NATE") || Equal(global.Command, "UPDATE"))
      {
        local.SelectCount.Count = 0;

        switch(AsChar(export.FilterSvpo.SelectChar))
        {
          case 'S':
            var field1 = GetField(export.FilterSvpo, "selectChar");

            field1.Error = true;

            local.SelectCount.Count = 1;

            if (!Equal(global.Command, "LIST"))
            {
              ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
            }

            break;
          case ' ':
            break;
          default:
            var field2 = GetField(export.FilterSvpo, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Gxommon.SelectChar))
          {
            case 'S':
              var field1 = GetField(export.Export1.Item.Gxommon, "selectChar");

              field1.Error = true;

              local.SelectCount.Count += 10;

              break;
            case '*':
              export.Export1.Update.Gxommon.SelectChar = "";

              break;
            case ' ':
              break;
            default:
              local.SelectCount.Count += 10;

              var field2 = GetField(export.Export1.Item.Gxommon, "selectChar");

              field2.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              break;
          }

          switch(AsChar(export.Export1.Item.GxodeValue.SelectChar))
          {
            case 'S':
              local.SelectCount.Count += 100;

              var field1 =
                GetField(export.Export1.Item.GxodeValue, "selectChar");

              field1.Error = true;

              break;
            case ' ':
              break;
            default:
              local.SelectCount.Count += 100;

              var field2 =
                GetField(export.Export1.Item.GxodeValue, "selectChar");

              field2.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test4;
        }

        switch(local.SelectCount.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            // *** Good for LIST command on SP only ***
            if (Equal(global.Command, "LIST"))
            {
              var field = GetField(export.FilterSvpo, "selectChar");

              field.Protected = false;
              field.Focused = true;

              export.FilterSvpo.SelectChar = "";
              ExitState = "ECO_LNK_TO_OFFICE_SERVICE_PROVDR";

              goto Test4;
            }
            else
            {
              ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
            }

            break;
          case 10:
            // *** Good all commands except LIST ***
            if (Equal(global.Command, "LIST"))
            {
              ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
            }

            break;
          case 100:
            // *** Invalid selection for any command ***
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

            break;
          case 110:
            // *** Good for LIST command on Reason Code only ***
            if (!Equal(global.Command, "LIST"))
            {
              ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test4;
        }
      }

      // ----------------------------------------------------------------------
      //                 C A S E   O F    C O M M A N D
      // ----------------------------------------------------------------------
      switch(TrimEnd(global.Command))
      {
        case "UPDATE":
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Gxommon.SelectChar) != 'S')
            {
              continue;
            }

            if (!Equal(export.Export1.Item.GxonitoredDocument.
              RequiredResponseDate,
              export.Export1.Item.Gxrev.RequiredResponseDate))
            {
              ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";
            }
            else if (Equal(export.Export1.Item.GxonitoredDocument.
              ActualResponseDate,
              export.Export1.Item.Gxrev.ActualResponseDate) && AsChar
              (export.Export1.Item.GxonitoredDocument.ClosureReasonCode) == AsChar
              (export.Export1.Item.Gxrev.ClosureReasonCode))
            {
              ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";
            }
            else if (Lt(local.Initialized.Date,
              export.Export1.Item.GxonitoredDocument.ActualResponseDate) && IsEmpty
              (export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
            {
              var field1 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "closureReasonCode");

              field1.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
            else if (Lt(local.Current.Date,
              export.Export1.Item.GxonitoredDocument.ActualResponseDate))
            {
              var field1 =
                GetField(export.Export1.Item.GxonitoredDocument,
                "actualResponseDate");

              field1.Error = true;

              ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
            }
            else if (!IsEmpty(export.Export1.Item.GxodeValue.SelectChar))
            {
              var field1 =
                GetField(export.Export1.Item.GxodeValue, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test2;
            }

            var field =
              GetField(export.Export1.Item.GxonitoredDocument,
              "closureReasonCode");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";

            // **** Validate Code Value - MONITORED DOC RESPONSE   ****
            foreach(var item in ReadCodeCodeValue())
            {
              if (Equal(entities.CodeValue.Cdvalue,
                export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
              {
                var field1 =
                  GetField(export.Export1.Item.GxonitoredDocument,
                  "closureReasonCode");

                field1.Protected = false;

                ExitState = "ACO_NN0000_ALL_OK";

                break;
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 =
                GetField(export.Export1.Item.GxodeValue, "selectChar");

              field1.Intensity = Intensity.Normal;
              field1.Protected = false;

              goto Test2;
            }

            if (!ReadMonitoredDocumentInfrastructure1())
            {
              ExitState = "SP0000_MONITORED_DOCUMENT_NF";

              goto Test2;
            }

            // -------------------------------------------------------------
            // If the Monitored Document's Infrastructure record does not belong
            // to the current
            // user, check if he/she has authority to update.
            // -------------------------------------------------------------
            if (!Equal(entities.Infrastructure.CreatedBy, local.UserId.UserId))
            {
              UseCoCabIsPersonSupervisor();

              if (AsChar(local.IsSupervisor.Flag) == 'N')
              {
                // -------------------------------------------------------------
                // Check to see if updating userid is in the same office as the
                // creating userid
                // -------------------------------------------------------------
                foreach(var item in ReadServiceProviderOfficeServiceProviderOffice2())
                  
                {
                  if (ReadServiceProviderOfficeServiceProviderOffice1())
                  {
                    goto Test1;
                  }
                }

                var field1 =
                  GetField(export.Export1.Item.Gxommon, "selectChar");

                field1.Error = true;

                ExitState = "CO0000_MUST_HAVE_SUPERVSRY_ROLE";

                goto Test2;
              }
            }

Test1:

            if (Equal(export.Export1.Item.GxonitoredDocument.ActualResponseDate,
              local.Initialized.Date))
            {
              export.Export1.Update.GxonitoredDocument.ActualResponseDate =
                local.Current.Date;
            }

            UseSpCabUpdateMonitoredDocument();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Gxommon.SelectChar = "*";

              var field1 = GetField(export.Export1.Item.Gxommon, "selectChar");

              field1.Protected = false;
              field1.Focused = true;

              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            }

            goto Test2;
          }

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          break;
        case "LIST":
          // ****  For the group view only - not the filters - taken care of 
          // above ****
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Gxommon.SelectChar) == 'S' && AsChar
              (export.Export1.Item.GxodeValue.SelectChar) == 'S')
            {
              export.DlgflwSelected.CodeName = "MONITORED DOC RESPONSE";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              goto Test4;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          break;
        case "DDOC":
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Gxommon.SelectChar) == 'S')
            {
              export.SelectedInfrastructure.Assign(
                export.Export1.Item.Gxnfrastructure);
              ExitState = "ECO_LNK_TO_DDOC";

              goto Test4;
            }
          }

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          break;
        case "NATE":
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Gxommon.SelectChar) == 'S')
            {
              if (export.Export1.Item.Gxnfrastructure.
                SystemGeneratedIdentifier > 0)
              {
                export.SelectedInfrastructure.Assign(
                  export.Export1.Item.Gxnfrastructure);
                export.SelectedDateWorkArea.Date =
                  Date(export.Export1.Item.Gxnfrastructure.CreatedTimestamp);
                export.SelectedLegalAction.CourtCaseNumber =
                  export.Export1.Item.GxegalAction.CourtCaseNumber ?? "";
                export.SelectedCsePersonsWorkSet.FormattedName =
                  export.Export1.Item.GxsePersonsWorkSet.FormattedName;
                ExitState = "ECO_LNK_TO_NATE";
              }
              else
              {
                var field = GetField(export.Export1.Item.Gxommon, "selectChar");

                field.Error = true;

                ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";
              }

              goto Test4;
            }
          }

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          break;
        default:
          break;
      }

Test2:

      if (Equal(global.Command, "DISPLAY"))
      {
        if (!IsEmpty(import.FilterInfrastructure.CaseNumber))
        {
          // MJR***** PAD CASE NUMBER WITH ZEROES ON THE LEFT *****
          local.TextWorkArea.Text10 =
            import.FilterInfrastructure.CaseNumber ?? Spaces(10);
          UseEabPadLeftWithZeros();
          export.FilterInfrastructure.CaseNumber = local.TextWorkArea.Text10;
        }

        if (!IsEmpty(import.FilterInfrastructure.CsePersonNumber))
        {
          // MJR***** PAD PERSON NUMBER WITH ZEROES ON THE LEFT *****
          local.TextWorkArea.Text10 =
            import.FilterInfrastructure.CsePersonNumber ?? Spaces(10);
          UseEabPadLeftWithZeros();
          export.FilterInfrastructure.CsePersonNumber =
            local.TextWorkArea.Text10;
        }

        switch(AsChar(export.FilterShowAll.Text1))
        {
          case 'Y':
            break;
          case 'N':
            break;
          case ' ':
            export.FilterShowAll.Text1 = "N";

            break;
          default:
            var field = GetField(export.FilterShowAll, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

            goto Test4;
        }

        // mjr
        // ---------------------------------------------------
        // 08/23/2000
        // PR# 101309 - DMON is based on OSP not SP
        // Since the SP Name is removed from the screen this is not necessary
        // ----------------------------------------------------------------
        if (IsEmpty(export.FilterServiceProvider.UserId))
        {
          export.FilterServiceProvider.UserId = local.UserId.UserId;

          // mjr
          // ---------------------------------------------------
          // 08/23/2000
          // PR# 101309 - DMON is based on OSP not SP
          // Get default office and OSP
          // ----------------------------------------------------------------
          export.FilterOffice.SystemGeneratedId = 0;
          export.FilterOfficeServiceProvider.RoleCode = "";
          export.FilterOfficeServiceProvider.EffectiveDate =
            local.Initialized.Date;

          if (ReadOfficeOfficeServiceProvider())
          {
            export.FilterOffice.SystemGeneratedId =
              entities.Office.SystemGeneratedId;
            MoveOfficeServiceProvider(entities.OfficeServiceProvider,
              export.FilterOfficeServiceProvider);
          }
        }

        // mjr
        // ---------------------------------------------------
        // 08/23/2000
        // PR# 101309 - DMON is based on OSP not SP
        // Since the SP Name was removed from the screen this READ is not 
        // necessary
        // ----------------------------------------------------------------
        if (!IsEmpty(export.FilterLegalAction.CourtCaseNumber))
        {
          // ----------------------------------------------------
          // Either (State and County) or Country must be entered
          // ----------------------------------------------------
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

            goto Test3;
          }

          if (!IsEmpty(export.FilterFips.StateAbbreviation) && IsEmpty
            (export.FilterFips.CountyAbbreviation))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            var field = GetField(export.FilterFips, "countyAbbreviation");

            field.Error = true;

            goto Test3;
          }

          if (IsEmpty(export.FilterFips.StateAbbreviation) && !
            IsEmpty(export.FilterFips.CountyAbbreviation))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            var field = GetField(export.FilterFips, "stateAbbreviation");

            field.Error = true;

            goto Test3;
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
        }
        else
        {
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

            goto Test3;
          }

          if (!IsEmpty(export.FilterFips.CountyAbbreviation))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            var field1 = GetField(export.FilterLegalAction, "courtCaseNumber");

            field1.Error = true;

            var field2 = GetField(export.FilterFips, "stateAbbreviation");

            field2.Error = true;

            goto Test3;
          }

          if (!IsEmpty(export.FilterFipsTribAddress.Country))
          {
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            var field = GetField(export.FilterLegalAction, "courtCaseNumber");

            field.Error = true;
          }
        }

Test3:

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test4;
        }

        if (IsEmpty(export.FilterFips.CountyAbbreviation) && IsEmpty
          (export.FilterFips.StateAbbreviation) && IsEmpty
          (export.FilterFipsTribAddress.Country) && IsEmpty
          (export.FilterInfrastructure.CaseNumber) && IsEmpty
          (export.FilterInfrastructure.CsePersonNumber) && IsEmpty
          (export.FilterLegalAction.CourtCaseNumber))
        {
          UseSpDmonReadByServiceProvider();
        }
        else if (!IsEmpty(export.FilterInfrastructure.CaseNumber))
        {
          UseSpDmonReadByCase();
        }
        else if (!IsEmpty(export.FilterInfrastructure.CsePersonNumber))
        {
          UseSpDmonReadByCsePerson();
        }
        else
        {
          UseSpDmonReadByLegalAction();
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
      }
    }

Test4:

    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_DISPLAY") || IsExitState
      ("ACO_NI0000_NO_DATA_TO_DISPLAY"))
    {
      // ---------------------------------------------------------
      // Normally, a successful update would be part of this exit
      // state group.  However, per PR # 75671,this is omitted so
      // that users can stay on the same screen that was just
      // updated
      // ---------------------------------------------------------
      // ****  Set protection and Color ****
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.GxonitoredDocument.ClosureReasonCode) &&
          AsChar(export.Export1.Item.GxonitoredDocument.ClosureReasonCode) == AsChar
          (export.Export1.Item.Gxrev.ClosureReasonCode) || Equal
          (export.Export1.Item.Gxocument.Name, "POSTMAST") || Equal
          (export.Export1.Item.Gxocument.Name, "EMPVERIF") || Equal
          (export.Export1.Item.Gxocument.Name, "EMANCIPA") || Equal
          (export.Export1.Item.Gxocument.Name, "EMPVERCO"))
        {
          // *** Problem report  H00079800
          // *** 11/18/99 SWSRCHF
          // *** start
          if (Equal(export.Export1.Item.Gxocument.Name, "EMANCIPA") && IsEmpty
            (export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
          {
            var field1 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "actualResponseDate");

            field1.Color = "";
            field1.Protected = false;

            var field2 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "closureReasonCode");

            field2.Color = "";
            field2.Protected = false;

            var field3 = GetField(export.Export1.Item.GxodeValue, "selectChar");

            field3.Intensity = Intensity.Normal;
            field3.Protected = false;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "actualResponseDate");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "closureReasonCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Export1.Item.GxodeValue, "selectChar");

            field3.Intensity = Intensity.Dark;
            field3.Protected = true;
          }

          // *** end
          // *** 11/18/99 SWSRCHF
          // *** Problem report  H00079800
        }

        if (!Lt(local.Current.Date,
          export.Export1.Item.GxonitoredDocument.RequiredResponseDate) && IsEmpty
          (export.Export1.Item.GxonitoredDocument.ClosureReasonCode) || Lt
          (export.Export1.Item.GxonitoredDocument.RequiredResponseDate,
          export.Export1.Item.GxonitoredDocument.ActualResponseDate) && !
          IsEmpty(export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
        {
          var field =
            GetField(export.Export1.Item.GxonitoredDocument,
            "requiredResponseDate");

          field.Color = "red";
          field.Protected = true;
        }
        else
        {
          var field =
            GetField(export.Export1.Item.GxonitoredDocument,
            "requiredResponseDate");

          field.Color = "cyan";
          field.Protected = true;
        }
      }

      export.ErrorForScrollDisplay.Flag = "";
    }
    else if (IsExitState("ACO_NE0000_INVALID_PF_KEY") || IsExitState
      ("SP0000_MONITORED_DOC_ASSIGN_NF"))
    {
    }
    else
    {
      UseSpDmonImplictToExplict();

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.Gxommon.SelectChar) && AsChar
          (export.Export1.Item.Gxommon.SelectChar) != '*')
        {
          var field = GetField(export.Export1.Item.Gxommon, "selectChar");

          field.Error = true;
        }
        else
        {
          var field = GetField(export.Export1.Item.Gxommon, "selectChar");

          field.Protected = false;
        }

        if (!IsEmpty(export.Export1.Item.GxonitoredDocument.ClosureReasonCode) &&
          AsChar(export.Export1.Item.GxonitoredDocument.ClosureReasonCode) == AsChar
          (export.Export1.Item.Gxrev.ClosureReasonCode) || Equal
          (export.Export1.Item.Gxocument.Name, "POSTMAST") || Equal
          (export.Export1.Item.Gxocument.Name, "EMPVERIF") || Equal
          (export.Export1.Item.Gxocument.Name, "EMANCIPA") || Equal
          (export.Export1.Item.Gxocument.Name, "EMPVERCO"))
        {
          // *** Problem report  H00079800
          // *** 11/18/99 SWSRCHF
          // *** start
          if (Equal(export.Export1.Item.Gxocument.Name, "EMANCIPA") && IsEmpty
            (export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
          {
            var field1 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "actualResponseDate");

            field1.Protected = false;

            var field2 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "closureReasonCode");

            field2.Protected = false;

            var field3 = GetField(export.Export1.Item.GxodeValue, "selectChar");

            field3.Protected = false;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "actualResponseDate");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.GxonitoredDocument,
              "closureReasonCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Export1.Item.GxodeValue, "selectChar");

            field3.Intensity = Intensity.Dark;
            field3.Protected = true;
          }

          // *** end
          // *** 11/18/99 SWSRCHF
          // *** Problem report  H00079800
        }

        if (!Lt(local.Current.Date,
          export.Export1.Item.GxonitoredDocument.RequiredResponseDate) && IsEmpty
          (export.Export1.Item.GxonitoredDocument.ClosureReasonCode) || Lt
          (export.Export1.Item.GxonitoredDocument.RequiredResponseDate,
          export.Export1.Item.GxonitoredDocument.ActualResponseDate) && !
          IsEmpty(export.Export1.Item.GxonitoredDocument.ClosureReasonCode))
        {
          var field =
            GetField(export.Export1.Item.GxonitoredDocument,
            "requiredResponseDate");

          field.Color = "red";
          field.Protected = true;
        }
        else
        {
          var field =
            GetField(export.Export1.Item.GxonitoredDocument,
            "requiredResponseDate");

          field.Color = "cyan";
          field.Protected = true;
        }
      }
    }
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
  }

  private static void MoveExplicit1ToExport1(SpDmonImplictToExplict.Export.
    ExplicitGroup source, Export.ExportGroup target)
  {
    target.Gxrev.Assign(source.Gxrev);
    target.GxegalAction.CourtCaseNumber = source.GxegalAction.CourtCaseNumber;
    target.GxodeValue.SelectChar = source.GxodeValue.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxsePersonsWorkSet.FormattedName;
    target.GxonitoredDocument.Assign(source.GxonitoredDocument);
    target.Gxommon.SelectChar = source.Gxommon.SelectChar;
    target.Gxnfrastructure.Assign(source.Gxnfrastructure);
    MoveDocument(source.Gxocument, target.Gxocument);
  }

  private static void MoveExport2(SpDmonReadByCase.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    target.Gxrev.Assign(source.GxinePrev);
    target.GxegalAction.CourtCaseNumber =
      source.GxineLegalAction.CourtCaseNumber;
    target.GxodeValue.SelectChar = source.GxineCodeValue.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxineCsePersonsWorkSet.FormattedName;
    target.GxonitoredDocument.Assign(source.GxineMonitoredDocument);
    target.Gxommon.SelectChar = source.GxineCommon.SelectChar;
    target.Gxnfrastructure.Assign(source.GxineInfrastructure);
    MoveDocument(source.GxineDocument, target.Gxocument);
  }

  private static void MoveExport3(SpDmonReadByCsePerson.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Gxrev.Assign(source.GxinePrev);
    target.GxegalAction.CourtCaseNumber =
      source.GxineLegalAction.CourtCaseNumber;
    target.GxodeValue.SelectChar = source.GxineCodeValue.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxineCsePersonsWorkSet.FormattedName;
    target.GxonitoredDocument.Assign(source.GxineMonitoredDocument);
    target.Gxommon.SelectChar = source.GxineCommon.SelectChar;
    target.Gxnfrastructure.Assign(source.GxineInfrastructure);
    MoveDocument(source.GxineDocument, target.Gxocument);
  }

  private static void MoveExport4(SpDmonReadByLegalAction.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Gxrev.Assign(source.GxinePrev);
    target.GxegalAction.CourtCaseNumber =
      source.GxineLegalAction.CourtCaseNumber;
    target.GxodeValue.SelectChar = source.GxineCodeValue.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxineCsePersonsWorkSet.FormattedName;
    target.GxonitoredDocument.Assign(source.GxineMonitoredDocument);
    target.Gxommon.SelectChar = source.GxineCommon.SelectChar;
    target.Gxnfrastructure.Assign(source.GxineInfrastructure);
    MoveDocument(source.GxineDocument, target.Gxocument);
  }

  private static void MoveExport5(SpDmonReadByServiceProvider.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Gxrev.Assign(source.GxinePrev);
    target.GxegalAction.CourtCaseNumber =
      source.GxineLegalAction.CourtCaseNumber;
    target.GxodeValue.SelectChar = source.GxineCodeValue.SelectChar;
    target.GxsePersonsWorkSet.FormattedName =
      source.GxineCsePersonsWorkSet.FormattedName;
    target.GxonitoredDocument.Assign(source.GxineMonitoredDocument);
    target.Gxommon.SelectChar = source.GxineCommon.SelectChar;
    target.Gxnfrastructure.Assign(source.GxineInfrastructure);
    MoveDocument(source.GxineDocument, target.Gxocument);
  }

  private static void MoveExport1ToExplicit1(Export.ExportGroup source,
    SpDmonImplictToExplict.Import.ExplicitGroup target)
  {
    target.Girev.Assign(source.Gxrev);
    target.GiegalAction.CourtCaseNumber = source.GxegalAction.CourtCaseNumber;
    target.GiodeValue.SelectChar = source.GxodeValue.SelectChar;
    target.GisePersonsWorkSet.FormattedName =
      source.GxsePersonsWorkSet.FormattedName;
    target.GionitoredDocument.Assign(source.GxonitoredDocument);
    target.Giommon.SelectChar = source.Gxommon.SelectChar;
    target.Ginfrastructure.Assign(source.Gxnfrastructure);
    MoveDocument(source.Gxocument, target.Giocument);
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveMonitoredDocument(MonitoredDocument source,
    MonitoredDocument target)
  {
    target.ActualResponseDate = source.ActualResponseDate;
    target.ClosureReasonCode = source.ClosureReasonCode;
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

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseCoCabIsPersonSupervisor()
  {
    var useImport = new CoCabIsPersonSupervisor.Import();
    var useExport = new CoCabIsPersonSupervisor.Export();

    useImport.ServiceProvider.UserId = local.UserId.UserId;
    useImport.ProcessDtOrCurrentDt.Date = local.Current.Date;

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

    useImport.Case1.Number = local.Case1.Number;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    MoveLegalAction(export.FilterLegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateMonitoredDocument()
  {
    var useImport = new SpCabUpdateMonitoredDocument.Import();
    var useExport = new SpCabUpdateMonitoredDocument.Export();

    MoveMonitoredDocument(export.Export1.Item.GxonitoredDocument,
      useImport.MonitoredDocument);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Export1.Item.Gxnfrastructure.SystemGeneratedIdentifier;

    Call(SpCabUpdateMonitoredDocument.Execute, useImport, useExport);
  }

  private void UseSpDmonImplictToExplict()
  {
    var useImport = new SpDmonImplictToExplict.Import();
    var useExport = new SpDmonImplictToExplict.Export();

    export.Export1.CopyTo(useImport.Explicit1, MoveExport1ToExplicit1);

    Call(SpDmonImplictToExplict.Execute, useImport, useExport);

    export.ErrorForScrollDisplay.Flag = useExport.ErrorForScrollDisplay.Flag;
    useExport.Explicit1.CopyTo(export.Export1, MoveExplicit1ToExport1);
  }

  private void UseSpDmonReadByCase()
  {
    var useImport = new SpDmonReadByCase.Import();
    var useExport = new SpDmonReadByCase.Export();

    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    useImport.FilterInfrastructure.Assign(export.FilterInfrastructure);
    useImport.FilterShowAll.Text1 = export.FilterShowAll.Text1;
    useImport.FilterDateWorkArea.Date = export.FilterDateWorkArea.Date;
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    MoveFips(export.FilterFips, useImport.FilterFips);

    Call(SpDmonReadByCase.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport2);
  }

  private void UseSpDmonReadByCsePerson()
  {
    var useImport = new SpDmonReadByCsePerson.Import();
    var useExport = new SpDmonReadByCsePerson.Export();

    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    useImport.FilterInfrastructure.Assign(export.FilterInfrastructure);
    useImport.FilterShowAll.Text1 = export.FilterShowAll.Text1;
    useImport.FilterDateWorkArea.Date = export.FilterDateWorkArea.Date;
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    MoveFips(export.FilterFips, useImport.FilterFips);

    Call(SpDmonReadByCsePerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport3);
  }

  private void UseSpDmonReadByLegalAction()
  {
    var useImport = new SpDmonReadByLegalAction.Import();
    var useExport = new SpDmonReadByLegalAction.Export();

    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    useImport.FilterInfrastructure.Assign(export.FilterInfrastructure);
    useImport.FilterShowAll.Text1 = export.FilterShowAll.Text1;
    useImport.FilterDateWorkArea.Date = export.FilterDateWorkArea.Date;
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    MoveFips(export.FilterFips, useImport.FilterFips);

    Call(SpDmonReadByLegalAction.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport4);
  }

  private void UseSpDmonReadByServiceProvider()
  {
    var useImport = new SpDmonReadByServiceProvider.Import();
    var useExport = new SpDmonReadByServiceProvider.Export();

    MoveOfficeServiceProvider(export.FilterOfficeServiceProvider,
      useImport.FilterOfficeServiceProvider);
    useImport.FilterOffice.SystemGeneratedId =
      export.FilterOffice.SystemGeneratedId;
    useImport.FilterFipsTribAddress.Country =
      export.FilterFipsTribAddress.Country;
    MoveLegalAction(export.FilterLegalAction, useImport.FilterLegalAction);
    useImport.FilterInfrastructure.Assign(export.FilterInfrastructure);
    useImport.FilterShowAll.Text1 = export.FilterShowAll.Text1;
    useImport.FilterDateWorkArea.Date = export.FilterDateWorkArea.Date;
    useImport.FilterServiceProvider.UserId =
      export.FilterServiceProvider.UserId;
    MoveFips(export.FilterFips, useImport.FilterFips);

    Call(SpDmonReadByServiceProvider.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport5);
  }

  private IEnumerable<bool> ReadCodeCodeValue()
  {
    entities.CodeValue.Populated = false;
    entities.Code.Populated = false;

    return ReadEach("ReadCodeCodeValue",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.CodeValue.Id = db.GetInt32(reader, 4);
        entities.CodeValue.Cdvalue = db.GetString(reader, 5);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 6);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 7);
        entities.CodeValue.Populated = true;
        entities.Code.Populated = true;

        return true;
      });
  }

  private bool ReadMonitoredDocumentInfrastructure1()
  {
    entities.Infrastructure.Populated = false;
    entities.MonitoredDocument.Populated = false;

    return Read("ReadMonitoredDocumentInfrastructure1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.Export1.Item.Gxnfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredDocument.RequiredResponseDate = db.GetDate(reader, 0);
        entities.MonitoredDocument.ActualResponseDate =
          db.GetNullableDate(reader, 1);
        entities.MonitoredDocument.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 3);
        entities.MonitoredDocument.CreatedBy = db.GetString(reader, 4);
        entities.MonitoredDocument.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.MonitoredDocument.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.MonitoredDocument.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.MonitoredDocument.InfId = db.GetInt32(reader, 8);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 10);
        entities.Infrastructure.EventType = db.GetString(reader, 11);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 12);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 13);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 14);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 15);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 16);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 17);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 18);
        entities.Infrastructure.UserId = db.GetString(reader, 19);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 20);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.Populated = true;
        entities.MonitoredDocument.Populated = true;
      });
  }

  private bool ReadMonitoredDocumentInfrastructure2()
  {
    entities.Infrastructure.Populated = false;
    entities.MonitoredDocument.Populated = false;

    return Read("ReadMonitoredDocumentInfrastructure2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.FilterInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredDocument.RequiredResponseDate = db.GetDate(reader, 0);
        entities.MonitoredDocument.ActualResponseDate =
          db.GetNullableDate(reader, 1);
        entities.MonitoredDocument.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 3);
        entities.MonitoredDocument.CreatedBy = db.GetString(reader, 4);
        entities.MonitoredDocument.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.MonitoredDocument.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.MonitoredDocument.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.MonitoredDocument.InfId = db.GetInt32(reader, 8);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 9);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 10);
        entities.Infrastructure.EventType = db.GetString(reader, 11);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 12);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 13);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 14);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 15);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 16);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 17);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 18);
        entities.Infrastructure.UserId = db.GetString(reader, 19);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 20);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.Populated = true;
        entities.MonitoredDocument.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.Infrastructure.ReferenceDate.GetValueOrDefault());
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.EffectiveDate = db.GetDate(reader, 1);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", export.FilterServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.EffectiveDate = db.GetDate(reader, 1);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 6);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          entities.Infrastructure.ReferenceDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", export.FilterServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProviderOffice1()
  {
    entities.UpdatedByServiceProvider.Populated = false;
    entities.UpdatedByOffice.Populated = false;
    entities.UpdateBy.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProviderOffice1",
      (db, command) =>
      {
        db.SetString(command, "userId", local.UserId.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "officeId", entities.CreatedByOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.UpdatedByServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.UpdateBy.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.UpdatedByServiceProvider.UserId = db.GetString(reader, 1);
        entities.UpdateBy.OffGeneratedId = db.GetInt32(reader, 2);
        entities.UpdatedByOffice.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.UpdateBy.RoleCode = db.GetString(reader, 3);
        entities.UpdateBy.EffectiveDate = db.GetDate(reader, 4);
        entities.UpdateBy.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.UpdatedByOffice.OffOffice = db.GetNullableInt32(reader, 6);
        entities.UpdatedByServiceProvider.Populated = true;
        entities.UpdatedByOffice.Populated = true;
        entities.UpdateBy.Populated = true;
      });
  }

  private IEnumerable<bool> ReadServiceProviderOfficeServiceProviderOffice2()
  {
    entities.CreatedByServiceProvider.Populated = false;
    entities.CreatedByOffice.Populated = false;
    entities.CreatedByOfficeServiceProvider.Populated = false;

    return ReadEach("ReadServiceProviderOfficeServiceProviderOffice2",
      (db, command) =>
      {
        db.SetString(command, "userId", entities.Infrastructure.UserId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CreatedByServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.CreatedByOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.CreatedByServiceProvider.UserId = db.GetString(reader, 1);
        entities.CreatedByOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.CreatedByOffice.SystemGeneratedId = db.GetInt32(reader, 2);
        entities.CreatedByOfficeServiceProvider.RoleCode =
          db.GetString(reader, 3);
        entities.CreatedByOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 4);
        entities.CreatedByOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CreatedByOffice.OffOffice = db.GetNullableInt32(reader, 6);
        entities.CreatedByServiceProvider.Populated = true;
        entities.CreatedByOffice.Populated = true;
        entities.CreatedByOfficeServiceProvider.Populated = true;

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
      /// A value of Girev.
      /// </summary>
      [JsonPropertyName("girev")]
      public MonitoredDocument Girev
      {
        get => girev ??= new();
        set => girev = value;
      }

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
      /// A value of GiodeValue.
      /// </summary>
      [JsonPropertyName("giodeValue")]
      public Common GiodeValue
      {
        get => giodeValue ??= new();
        set => giodeValue = value;
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

      /// <summary>
      /// A value of GionitoredDocument.
      /// </summary>
      [JsonPropertyName("gionitoredDocument")]
      public MonitoredDocument GionitoredDocument
      {
        get => gionitoredDocument ??= new();
        set => gionitoredDocument = value;
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
      /// A value of Ginfrastructure.
      /// </summary>
      [JsonPropertyName("ginfrastructure")]
      public Infrastructure Ginfrastructure
      {
        get => ginfrastructure ??= new();
        set => ginfrastructure = value;
      }

      /// <summary>
      /// A value of Giocument.
      /// </summary>
      [JsonPropertyName("giocument")]
      public Document Giocument
      {
        get => giocument ??= new();
        set => giocument = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private MonitoredDocument girev;
      private LegalAction giegalAction;
      private Common giodeValue;
      private CsePersonsWorkSet gisePersonsWorkSet;
      private MonitoredDocument gionitoredDocument;
      private Common giommon;
      private Infrastructure ginfrastructure;
      private Document giocument;
    }

    /// <summary>
    /// A value of ZdelImportFilterSpName.
    /// </summary>
    [JsonPropertyName("zdelImportFilterSpName")]
    public CsePersonsWorkSet ZdelImportFilterSpName
    {
      get => zdelImportFilterSpName ??= new();
      set => zdelImportFilterSpName = value;
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
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
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
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
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
    /// A value of FilterShowAll.
    /// </summary>
    [JsonPropertyName("filterShowAll")]
    public WorkArea FilterShowAll
    {
      get => filterShowAll ??= new();
      set => filterShowAll = value;
    }

    /// <summary>
    /// A value of FilterDateWorkArea.
    /// </summary>
    [JsonPropertyName("filterDateWorkArea")]
    public DateWorkArea FilterDateWorkArea
    {
      get => filterDateWorkArea ??= new();
      set => filterDateWorkArea = value;
    }

    /// <summary>
    /// A value of FilterSvpo.
    /// </summary>
    [JsonPropertyName("filterSvpo")]
    public Common FilterSvpo
    {
      get => filterSvpo ??= new();
      set => filterSvpo = value;
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
    /// A value of UseNate.
    /// </summary>
    [JsonPropertyName("useNate")]
    public Standard UseNate
    {
      get => useNate ??= new();
      set => useNate = value;
    }

    /// <summary>
    /// A value of SelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedServiceProvider")]
    public ServiceProvider SelectedServiceProvider
    {
      get => selectedServiceProvider ??= new();
      set => selectedServiceProvider = value;
    }

    /// <summary>
    /// A value of ErrorForScrollDisplay.
    /// </summary>
    [JsonPropertyName("errorForScrollDisplay")]
    public Common ErrorForScrollDisplay
    {
      get => errorForScrollDisplay ??= new();
      set => errorForScrollDisplay = value;
    }

    /// <summary>
    /// A value of SelectedOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedOfficeServiceProvider")]
    public OfficeServiceProvider SelectedOfficeServiceProvider
    {
      get => selectedOfficeServiceProvider ??= new();
      set => selectedOfficeServiceProvider = value;
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

    private CsePersonsWorkSet zdelImportFilterSpName;
    private Fips filterFips;
    private Office selectedOffice;
    private FipsTribAddress filterFipsTribAddress;
    private CodeValue dlgflwSelected;
    private LegalAction filterLegalAction;
    private Infrastructure filterInfrastructure;
    private WorkArea filterShowAll;
    private DateWorkArea filterDateWorkArea;
    private Common filterSvpo;
    private ServiceProvider filterServiceProvider;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private Standard useNate;
    private ServiceProvider selectedServiceProvider;
    private Common errorForScrollDisplay;
    private OfficeServiceProvider selectedOfficeServiceProvider;
    private OfficeServiceProvider filterOfficeServiceProvider;
    private Office filterOffice;
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
      /// A value of Gxrev.
      /// </summary>
      [JsonPropertyName("gxrev")]
      public MonitoredDocument Gxrev
      {
        get => gxrev ??= new();
        set => gxrev = value;
      }

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
      /// A value of GxodeValue.
      /// </summary>
      [JsonPropertyName("gxodeValue")]
      public Common GxodeValue
      {
        get => gxodeValue ??= new();
        set => gxodeValue = value;
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

      /// <summary>
      /// A value of GxonitoredDocument.
      /// </summary>
      [JsonPropertyName("gxonitoredDocument")]
      public MonitoredDocument GxonitoredDocument
      {
        get => gxonitoredDocument ??= new();
        set => gxonitoredDocument = value;
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
      /// A value of Gxnfrastructure.
      /// </summary>
      [JsonPropertyName("gxnfrastructure")]
      public Infrastructure Gxnfrastructure
      {
        get => gxnfrastructure ??= new();
        set => gxnfrastructure = value;
      }

      /// <summary>
      /// A value of Gxocument.
      /// </summary>
      [JsonPropertyName("gxocument")]
      public Document Gxocument
      {
        get => gxocument ??= new();
        set => gxocument = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private MonitoredDocument gxrev;
      private LegalAction gxegalAction;
      private Common gxodeValue;
      private CsePersonsWorkSet gxsePersonsWorkSet;
      private MonitoredDocument gxonitoredDocument;
      private Common gxommon;
      private Infrastructure gxnfrastructure;
      private Document gxocument;
    }

    /// <summary>
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
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
    /// A value of SelectedDateWorkArea.
    /// </summary>
    [JsonPropertyName("selectedDateWorkArea")]
    public DateWorkArea SelectedDateWorkArea
    {
      get => selectedDateWorkArea ??= new();
      set => selectedDateWorkArea = value;
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
    /// A value of ZdelExportFilterSpName.
    /// </summary>
    [JsonPropertyName("zdelExportFilterSpName")]
    public CsePersonsWorkSet ZdelExportFilterSpName
    {
      get => zdelExportFilterSpName ??= new();
      set => zdelExportFilterSpName = value;
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
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public Code DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
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
    /// A value of FilterShowAll.
    /// </summary>
    [JsonPropertyName("filterShowAll")]
    public WorkArea FilterShowAll
    {
      get => filterShowAll ??= new();
      set => filterShowAll = value;
    }

    /// <summary>
    /// A value of FilterDateWorkArea.
    /// </summary>
    [JsonPropertyName("filterDateWorkArea")]
    public DateWorkArea FilterDateWorkArea
    {
      get => filterDateWorkArea ??= new();
      set => filterDateWorkArea = value;
    }

    /// <summary>
    /// A value of FilterSvpo.
    /// </summary>
    [JsonPropertyName("filterSvpo")]
    public Common FilterSvpo
    {
      get => filterSvpo ??= new();
      set => filterSvpo = value;
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
    /// A value of UseNate.
    /// </summary>
    [JsonPropertyName("useNate")]
    public Standard UseNate
    {
      get => useNate ??= new();
      set => useNate = value;
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
    /// A value of ErrorForScrollDisplay.
    /// </summary>
    [JsonPropertyName("errorForScrollDisplay")]
    public Common ErrorForScrollDisplay
    {
      get => errorForScrollDisplay ??= new();
      set => errorForScrollDisplay = value;
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

    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private LegalAction selectedLegalAction;
    private DateWorkArea selectedDateWorkArea;
    private Infrastructure selectedInfrastructure;
    private CsePersonsWorkSet zdelExportFilterSpName;
    private FipsTribAddress filterFipsTribAddress;
    private Code dlgflwSelected;
    private LegalAction filterLegalAction;
    private Infrastructure filterInfrastructure;
    private WorkArea filterShowAll;
    private DateWorkArea filterDateWorkArea;
    private Common filterSvpo;
    private ServiceProvider filterServiceProvider;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
    private Standard useNate;
    private Fips filterFips;
    private Common errorForScrollDisplay;
    private OfficeServiceProvider filterOfficeServiceProvider;
    private Office filterOffice;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of TotalNumberOfRecords.
    /// </summary>
    [JsonPropertyName("totalNumberOfRecords")]
    public Common TotalNumberOfRecords
    {
      get => totalNumberOfRecords ??= new();
      set => totalNumberOfRecords = value;
    }

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
    /// A value of SelectCount.
    /// </summary>
    [JsonPropertyName("selectCount")]
    public Common SelectCount
    {
      get => selectCount ??= new();
      set => selectCount = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public ServiceProvider UserId
    {
      get => userId ??= new();
      set => userId = value;
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
    /// A value of IsSupervisor.
    /// </summary>
    [JsonPropertyName("isSupervisor")]
    public Common IsSupervisor
    {
      get => isSupervisor ??= new();
      set => isSupervisor = value;
    }

    private DateWorkArea null1;
    private Common totalNumberOfRecords;
    private Common promptCount;
    private Common selectCount;
    private Case1 case1;
    private TextWorkArea textWorkArea;
    private DateWorkArea initialized;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ServiceProvider userId;
    private DateWorkArea current;
    private Common isSupervisor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("printerOutputDestination")]
    public PrinterOutputDestination PrinterOutputDestination
    {
      get => printerOutputDestination ??= new();
      set => printerOutputDestination = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of UpdatedByServiceProvider.
    /// </summary>
    [JsonPropertyName("updatedByServiceProvider")]
    public ServiceProvider UpdatedByServiceProvider
    {
      get => updatedByServiceProvider ??= new();
      set => updatedByServiceProvider = value;
    }

    /// <summary>
    /// A value of CreatedByServiceProvider.
    /// </summary>
    [JsonPropertyName("createdByServiceProvider")]
    public ServiceProvider CreatedByServiceProvider
    {
      get => createdByServiceProvider ??= new();
      set => createdByServiceProvider = value;
    }

    /// <summary>
    /// A value of UpdatedByOffice.
    /// </summary>
    [JsonPropertyName("updatedByOffice")]
    public Office UpdatedByOffice
    {
      get => updatedByOffice ??= new();
      set => updatedByOffice = value;
    }

    /// <summary>
    /// A value of CreatedByOffice.
    /// </summary>
    [JsonPropertyName("createdByOffice")]
    public Office CreatedByOffice
    {
      get => createdByOffice ??= new();
      set => createdByOffice = value;
    }

    /// <summary>
    /// A value of UpdateBy.
    /// </summary>
    [JsonPropertyName("updateBy")]
    public OfficeServiceProvider UpdateBy
    {
      get => updateBy ??= new();
      set => updateBy = value;
    }

    /// <summary>
    /// A value of CreatedByOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("createdByOfficeServiceProvider")]
    public OfficeServiceProvider CreatedByOfficeServiceProvider
    {
      get => createdByOfficeServiceProvider ??= new();
      set => createdByOfficeServiceProvider = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    private PrinterOutputDestination printerOutputDestination;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider updatedByServiceProvider;
    private ServiceProvider createdByServiceProvider;
    private Office updatedByOffice;
    private Office createdByOffice;
    private OfficeServiceProvider updateBy;
    private OfficeServiceProvider createdByOfficeServiceProvider;
    private CodeValue codeValue;
    private Code code;
    private OutgoingDocument outgoingDocument;
    private FipsTribAddress fipsTribAddress;
    private Document document;
    private Fips fips;
    private Tribunal tribunal;
    private Infrastructure infrastructure;
    private LegalAction legalAction;
    private ServiceProvider serviceProvider;
    private MonitoredDocument monitoredDocument;
  }
#endregion
}
