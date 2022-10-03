// Program: SP_DDOC_DEAD_DOCUMENT, ID: 372148310, model: 746.
// Short name: SWEDDOCP
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
/// A program: SP_DDOC_DEAD_DOCUMENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpDdocDeadDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DDOC_DEAD_DOCUMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDdocDeadDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDdocDeadDocument.
  /// </summary>
  public SpDdocDeadDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 11/18/1996	M Ramirez			Initial Development
    // 10/27/1998	M Ramirez			Re-development
    // 11/15/1999	M Ramirez	80123		Added check for Command
    // 						= PRINT to perform updates
    // 						immediately
    // 11/29/1999	Srini Ganji 	WR#156		Necessary code added because
    //                                                 
    // of Field Values
    //                                                 
    // archival/retrieval process
    // 07/10/2000	M Ramirez	80722		Added code to support new
    // 						Required Field indicator
    // 						value 'U'
    // 08/08/2000	M Ramirez	99884		Archived documents will be
    // 						able to show the ' KEY' fields
    // 10/24/2000	M Ramirez	105251		Enabled return of ERROR_IND
    // 						from
    // 						sp_print_data_retrieval_main
    // 02/12/2001	M Ramirez	WR# 187 Seg B	On PRINT give message to user
    // 						and don't allow download
    // 02/11/2002	M Ramirez	PR#111335	Keep infrastructure information.
    // 03/11/2002	M Ramirez	PR141140	Print preview documents not handled 
    // correctly.
    // 03/24/2006      M J Quinn       PR266315        When printing from GTSC 
    // screen, only print attorney
    // 						information for the specified case.   Export Last
    // 						Tran to SP_PRINT_DATA_RETRIEVAL_MAIN.
    // 09/03/2009	JHuss		CQ# 211		Move field value retrieval logic to separate 
    // action block.
    // 06/09/2015	GVandy		CQ22212		Changes to support electronic Income 
    // Withholding (e-IWO).
    // ----------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.FieldValue.LastUpdatedBy = global.UserId;
    local.FieldValue.LastUpdatdTstamp = Now();
    UseSpDocSetLiterals();
    export.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    local.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    export.NextTranInfo.Assign(import.NextTranInfo);

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ----------------------------------------------------------------------
    // MOVE IMPORTS TO EXPORTS
    // ----------------------------------------------------------------------
    export.Filter.Assign(import.Filter);
    export.Previous.Command = import.Previous.Command;
    export.RequiredFieldMissing.Flag = import.RequiredFieldMissing.Flag;
    export.UserInputField.Flag = import.UserInputField.Flag;
    export.PrintPreview.Flag = import.PrintPreview.Flag;
    export.ErrorInd.Flag = import.ErrorInd.Flag;

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.GdocumentField.Assign(
        import.Import1.Item.GdocumentField);
      export.Export1.Update.GfieldValue.Value =
        import.Import1.Item.GfieldValue.Value;
      export.Export1.Update.GexportPrevious.Value =
        import.Import1.Item.GimportPrevious.Value;
      MoveField(import.Import1.Item.GimportHidden,
        export.Export1.Update.GexportHidden);
    }

    import.Import1.CheckIndex();
    MoveStandard(import.Scrolling, export.Scrolling);

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      MoveDocumentField(import.HiddenPageKeys.Item.GimportHidden,
        export.HiddenPageKeys.Update.GexportHidden);
    }

    import.HiddenPageKeys.CheckIndex();

    // mjr
    // -------------------------------------------------------------
    // Next tran and Security start here
    // ----------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // The user requested a next tran action
      export.NextTranInfo.InfrastructureId =
        export.Infrastructure.SystemGeneratedIdentifier;
      UseScCabNextTranPut1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // The user is comming into this procedure on a next tran action.
      UseScCabNextTranGet();
      export.Infrastructure.SystemGeneratedIdentifier =
        export.NextTranInfo.InfrastructureId.GetValueOrDefault();
      export.NextTranInfo.LastTran = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // Flow from the menu
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // mjr
    // -------------------------------------------------------------
    // Next tran and Security end here
    // ----------------------------------------------------------------
    if (export.Scrolling.PageNumber == 0 || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      export.Scrolling.PageNumber = 1;

      export.HiddenPageKeys.Index = 0;
      export.HiddenPageKeys.CheckSize();
    }

    // mjr
    // ----------------------------------------------------
    // UPDATE any USRINPUT fields
    // -------------------------------------------------------
    if (Equal(export.Previous.Command, "PRINT") || Equal
      (global.Command, "PRINT"))
    {
      // mjr
      // -------------------------------------------------------
      // 11/15/1999
      // Added check for Command = PRINT to perform updates immediately
      // --------------------------------------------------------------------
      // mjr
      // ------------------------------------------
      // 10/14/1998
      // Before scrolling, update any field_values that have changed
      // -------------------------------------------------------
      if (Equal(global.Command, "DISPLAY") || Equal
        (global.Command, "PRINT") || Equal(global.Command, "PREV") || Equal
        (global.Command, "NEXT"))
      {
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          // mjr
          // ----------------------------------------
          // 07/10/2000
          // Added code to support new Required Field indicator value 'U'
          // -----------------------------------------------------
          if (!Equal(export.Export1.Item.GfieldValue.Value,
            export.Export1.Item.GexportPrevious.Value))
          {
            if (AsChar(export.Export1.Item.GdocumentField.RequiredSwitch) == 'U'
              || Equal
              (export.Export1.Item.GexportHidden.SubroutineName, "USRINPUT"))
            {
              local.FieldValue.Value =
                export.Export1.Item.GfieldValue.Value ?? "";
              UseSpCabCreateUpdateFieldValue();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field1 = GetField(export.Export1.Item.GfieldValue, "value");

                field1.Error = true;

                return;
              }

              export.Export1.Update.GexportPrevious.Value =
                export.Export1.Item.GfieldValue.Value ?? "";
            }
          }

          local.SkipDataRetrievalCab.Flag = "Y";
        }

        export.Export1.CheckIndex();
      }
    }

    // ----------------------------------------------------------------------
    //                 C A S E   O F    C O M M A N D
    // ----------------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.Previous.Command = "";
        export.PrintPreview.Flag = "";

        if (ReadInfrastructure())
        {
          export.RequiredFieldMissing.Flag = "N";
          export.UserInputField.Flag = "N";
        }
        else
        {
          ExitState = "INFRASTRUCTURE_NF";

          return;
        }

        break;
      case "PRINT":
        // mjr
        // ------------------------------------------------
        // 09/11/1998
        // Print is used from DDOC when the user is in the regular print
        // process.  The Emulator session (Extra!) intercepts this Command
        // and starts a script (written in Extra! Visual Basic).  The
        // script extracts the Field values from the screen, then lets
        // KESSEP know that it's ready for the next page of data by sending
        // a Next command.
        // The script is through with KESSEP when the More Indicator
        // on the screen indicates that no more Field values exist.
        // KESSEP can then return the user to the Originating screen, while
        // the script file continues processing on the PC through WordPerfect.
        // -------------------------------------------------------------
        export.Previous.Command = global.Command;

        if (ReadOutgoingDocumentDocument())
        {
          // *****************************************************************
          // 11/29/99                Srini Ganji                        WR#156
          // Following code added for Field value archival/retrieval process
          // *****************************************************************
          if (AsChar(entities.OutgoingDocument.FieldValuesArchiveInd) == 'Y')
          {
            // *****************************************************************
            // Outgoing Document field values are archived into external file.
            // *****************************************************************
            ExitState = "SP0000_FIELD_VALUES_ARCHIVED";

            return;
          }

          // *****************************************************************
          // End of code
          // 11/29/99                Srini Ganji                        WR#156
          // *****************************************************************
          export.Filter.Assign(entities.Document);

          // mjr
          // --------------------------------------
          // 02/12/2001
          // WR# 187 - Auto IWO
          // Changed following IF statement to check for Ms
          // Remove check for B, which is obsolete
          // ---------------------------------------------------
          if (AsChar(entities.OutgoingDocument.PrintSucessfulIndicator) == 'G')
          {
            export.Previous.Command = "";
            ExitState = "SP0000_PRINT_UNAVAILABLE_BATCH";

            return;
          }
          else if (AsChar(entities.OutgoingDocument.PrintSucessfulIndicator) ==
            'M')
          {
            export.Previous.Command = "";
            ExitState = "SP0000_PRINT_UNAVAILABLE_EMAIL";

            return;
          }

          if (AsChar(entities.OutgoingDocument.PrintSucessfulIndicator) == 'Y'
            || AsChar(export.PrintPreview.Flag) == '1')
          {
            // mjr
            // ----------------------------------------------
            // 11/09/1998
            // In the event of a link, this may not be populated.  Add it now.
            // ------------------------------------------------------------
            local.MiscText.Text50 = export.NextTranInfo.MiscText2 ?? Spaces(50);
            local.KeyName.Text33 = local.SpDocLiteral.IdDocument;
            local.KeyValue.Text33 = "RET0";
            UseSpPrintManipulateKeys();
            export.NextTranInfo.MiscText2 = local.MiscText.Text50;
            export.RequiredFieldMissing.Flag = "N";
            export.UserInputField.Flag = "N";

            // mjr
            // -------------------------------------------------------
            // 03/11/2002
            // PR141140 - Print preview documents are not handled correctly
            // Only ESCAPE if the document was already successfully printed
            // --------------------------------------------------------------------
            if (AsChar(entities.OutgoingDocument.PrintSucessfulIndicator) == 'Y'
              )
            {
              break;
            }
          }
        }
        else
        {
          export.Previous.Command = "";
          ExitState = "OUTGOING_DOCUMENT_NF";

          return;
        }

        if (IsEmpty(export.Filter.Name))
        {
          export.Previous.Command = "";
          export.Filter.Description = "";
          export.Filter.VersionNumber = "";

          var field1 = GetField(export.Filter, "name");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        if (!IsEmpty(export.ErrorInd.Flag))
        {
          if (AsChar(export.ErrorInd.Flag) >= '0' && AsChar
            (export.ErrorInd.Flag) <= '9')
          {
            local.ErrorInd.Flag = export.ErrorInd.Flag;
          }
          else
          {
            local.ErrorInd.Flag = "0";
          }
        }
        else
        {
          local.ErrorInd.Flag = "0";
        }

        if (AsChar(local.SkipDataRetrievalCab.Flag) != 'Y')
        {
          // mjr
          // ---------------------------------------
          // 09/11/1998
          // Call Data_retrieval cab
          // ----------------------------------------------------
          UseSpPrintDataRetrievalMain();

          if (!IsEmpty(local.ErrorDocumentField.ScreenPrompt))
          {
            export.Export1.Index = 0;
            export.Export1.CheckSize();

            export.Export1.Count = 1;
            export.Export1.Update.GdocumentField.ScreenPrompt =
              local.ErrorDocumentField.ScreenPrompt;
            export.Export1.Update.GdocumentField.RequiredSwitch = "";
            export.Export1.Update.GdocumentField.Position = 0;
            export.Export1.Update.GfieldValue.Value =
              local.ErrorFieldValue.Value ?? "";

            var field1 = GetField(export.Export1.Item.GfieldValue, "value");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "SP0000_DOWNLOAD_UNSUCCESSFUL";
            }
          }
          else if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Filter.Description = "";
            export.Filter.VersionNumber = "";

            var field1 = GetField(export.Filter, "name");

            field1.Error = true;
          }
          else if (!IsEmpty(export.ErrorInd.Flag))
          {
            // mjr
            // --------------------------------------------
            // 10/24/20000
            // PR# 105251 - Enabled return of ERROR_IND from
            // sp_print_data_retrieval_main
            // ----------------------------------------------------------
            local.ErrorInd.Flag = export.ErrorInd.Flag;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Previous.Command = "";
            UseEabRollbackCics();

            return;
          }
        }
        else
        {
          // PR# 111335 - adding else statment to avoid losing infrastructure 
          // information.  MJR
          if (ReadInfrastructure())
          {
            local.Infrastructure.Assign(entities.Infrastructure);
          }
          else
          {
            ExitState = "INFRASTRUCTURE_NF";

            return;
          }
        }

        // mjr
        // -----------------------------------------------------------------
        // Check for missing and mandatory field_values
        // --------------------------------------------------------------------
        export.RequiredFieldMissing.Flag = "N";
        export.UserInputField.Flag = "N";

        foreach(var item in ReadDocumentFieldOutgoingDocument())
        {
          if (ReadFieldValue())
          {
            local.FieldValue.Value = entities.FieldValue.Value;
          }
          else
          {
            local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
          }

          if (IsEmpty(local.FieldValue.Value))
          {
            ReadField();

            // mjr
            // ----------------------------------------
            // 08/01/2000
            // Added code to support new Required Field indicator value 'U'
            // -----------------------------------------------------
            if (AsChar(entities.DocumentField.RequiredSwitch) == 'Y')
            {
              export.RequiredFieldMissing.Flag = "Y";
            }
            else if (AsChar(entities.DocumentField.RequiredSwitch) == 'U')
            {
              export.UserInputField.Flag = "Y";
            }
            else if (Equal(entities.Field.SubroutineName, "USRINPUT"))
            {
              if (AsChar(local.SkipDataRetrievalCab.Flag) != 'Y')
              {
                export.UserInputField.Flag = "Y";
              }
            }
            else
            {
            }
          }

          if (AsChar(export.RequiredFieldMissing.Flag) == 'Y' || AsChar
            (export.UserInputField.Flag) == 'Y')
          {
            break;
          }
        }

        // mjr
        // -------------------------------------------------------
        // 11/15/1999
        // Changed processing to check for successful documents
        // rather than unsuccessful documents
        // --------------------------------------------------------------------
        // mjr
        // -------------------------------------------------------
        // 03/11/2002
        // PR141140 - Print preview documents are not handled correctly
        // Changed 'AND document print_preview_switch IS EQUAL TO "N"'
        // to 'AND export_print_preview_ind IS EQUAL TO "1"'
        // --------------------------------------------------------------------
        if (AsChar(export.RequiredFieldMissing.Flag) == 'N' && AsChar
          (export.UserInputField.Flag) == 'N' && AsChar
          (export.PrintPreview.Flag) == '1' && (AsChar(local.ErrorInd.Flag) == '0'
          || AsChar(local.ErrorInd.Flag) == '1'))
        {
          // mjr
          // -----------------------------------------------------------------
          // Document printing is successful.
          // Update outgoing_document, create monitored document (if necessary),
          // and update infrastructure record.
          // --------------------------------------------------------------------
          local.Infrastructure.LastUpdatedBy = "";
          local.Infrastructure.LastUpdatedTimestamp = local.Null1.Timestamp;
          UseSpDocUpdateSuccessfulPrint();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }
        else
        {
          // mjr
          // -----------------------------------------------------------------
          // Document printing is unsuccessful.
          // --------------------------------------------------------------------
          // mjr
          // ----------------------------------------------------------
          // Some values are received from sp_print_data_retrieval_main
          // Update infrastructure now to record them
          // -------------------------------------------------------------
          UseSpCabUpdateInfrastructure();

          if (AsChar(export.RequiredFieldMissing.Flag) == 'Y')
          {
            local.ErrorInd.Flag = "Z";
          }
          else if (AsChar(export.UserInputField.Flag) == 'Y')
          {
            local.ErrorInd.Flag = "Y";
          }
          else
          {
            // mjr
            // ------------------------------------------------------------
            // The return_code is set, don't overwrite it and don't update
            // the Print Successful Indicator.
            // ---------------------------------------------------------------
          }
        }

        // mjr
        // -----------------------------------------------------------------
        // Extract the document name from the next_tran_info misc_text_2
        // Replace it with a return code
        // --------------------------------------------------------------------
        local.MiscText.Text50 = export.NextTranInfo.MiscText2 ?? Spaces(50);
        local.KeyName.Text33 = local.SpDocLiteral.IdDocument;
        local.KeyValue.Text33 = "RET" + local.ErrorInd.Flag;
        UseSpPrintManipulateKeys();
        export.NextTranInfo.MiscText2 = local.MiscText.Text50;

        break;
      case "RETURN":
        if (IsEmpty(export.NextTranInfo.LastTran))
        {
          ExitState = "ACO_NE0000_RETURN";

          return;
        }

        if (ReadOutgoingDocument())
        {
          if (AsChar(entities.OutgoingDocument.PrintSucessfulIndicator) == 'N')
          {
            // mjr
            // -----------------------------------------------------------------
            // Check for missing and mandatory field_values
            // --------------------------------------------------------------------
            export.RequiredFieldMissing.Flag = "";

            foreach(var item in ReadDocumentField1())
            {
              if (ReadFieldValue())
              {
                local.FieldValue.Value = entities.FieldValue.Value;
              }
              else
              {
                local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
              }

              // mjr
              // ----------------------------------------
              // 07/10/2000
              // Added code to support new Required Field indicator value 'U'
              // -----------------------------------------------------
              if (IsEmpty(local.FieldValue.Value) && AsChar
                (entities.DocumentField.RequiredSwitch) != 'N')
              {
                export.RequiredFieldMissing.Flag = "Y";

                break;
              }
            }

            if (!IsEmpty(export.RequiredFieldMissing.Flag))
            {
              goto Read;
            }

            // mjr
            // -----------------------------------------------------------------
            // Determine the return code
            // --------------------------------------------------------------------
            local.MiscText.Text50 = export.NextTranInfo.MiscText2 ?? Spaces(50);
            UseSpPrintDecodeReturnCode();

            // mjr
            // ----------------------------------------------------
            // 11/18/1999
            // User may have pressed return before actually Printing, in the
            // case of Print Preview.
            // This should not be a successful print, so I added the check
            // here.
            // -----------------------------------------------------------------
            if (AsChar(export.PrintPreview.Flag) != 'N')
            {
              // mjr
              // -----------------------------------------------------------------
              // User canceled Print.  Set return code
              // --------------------------------------------------------------------
              local.MiscText.Text50 = export.NextTranInfo.MiscText2 ?? Spaces
                (50);
              local.KeyName.Text33 = local.SpDocLiteral.IdDocument;
              local.KeyValue.Text33 = "RETZ";
              UseSpPrintManipulateKeys();
              export.NextTranInfo.MiscText2 = local.MiscText.Text50;
            }
            else if (!IsExitState("SP0000_DOWNLOAD_UNSUCCESSFUL") && !
              IsExitState("SP0000_PRINT_REQUIRES_LEGAL_DTL"))
            {
              ExitState = "ACO_NN0000_ALL_OK";

              // mjr
              // -----------------------------------------------------------------
              // Document printing is successful.
              // Update outgoing_document, create monitored document (if 
              // necessary),
              // and update infrastructure record.
              // --------------------------------------------------------------------
              // mjr
              // -----------------------------------------------
              // 07/27/1999
              // Set userid to spaces, so none of the denorm ids will be set
              // ------------------------------------------------------------
              local.Infrastructure.UserId = "";
              local.Infrastructure.SystemGeneratedIdentifier =
                export.Infrastructure.SystemGeneratedIdentifier;
              local.Infrastructure.LastUpdatedBy = "";
              local.Infrastructure.LastUpdatedTimestamp = local.Null1.Timestamp;
              UseSpDocUpdateSuccessfulPrint();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();

                return;
              }
            }
          }
        }

Read:

        // mjr
        // ---------------------------------------
        // 09/11/1998
        // return user to originating screen, with previous display
        // ----------------------------------------------------
        if (ReadTransaction())
        {
          export.Standard.NextTransaction = entities.Transaction.ScreenId;
        }
        else
        {
          ExitState = "SC0002_SCREEN_ID_NF";

          return;
        }

        if (Equal(export.Standard.NextTransaction, "ODCM") || Equal
          (export.Standard.NextTransaction, "LAIS"))
        {
          // mjr
          // ---------------------------------------------------------
          // 01/25/1999
          // ODCM needs the infrastructure id upon return, but the rest don't.
          // (HIST and DMON use a link, not this next_tran return.)
          // ----------------------------------------------------------------------
          export.NextTranInfo.InfrastructureId =
            export.Infrastructure.SystemGeneratedIdentifier;
        }
        else
        {
          export.NextTranInfo.InfrastructureId = 0;
        }

        local.PrintProcess.Flag = "Y";
        local.PrintProcess.Command = "PRINTRET";
        UseScCabNextTranPut2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.Standard, "nextTransaction");

          field1.Error = true;
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "PREV":
        if (export.Scrolling.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.Scrolling.PageNumber;

        break;
      case "NEXT":
        if (export.Scrolling.PageNumber == Export.HiddenPageKeysGroup.Capacity)
        {
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";

          return;
        }

        export.HiddenPageKeys.Index = export.Scrolling.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (export.HiddenPageKeys.Item.GexportHidden.Position == 0 && IsEmpty
          (export.HiddenPageKeys.Item.GexportHidden.ScreenPrompt))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.Scrolling.PageNumber;

        break;
      case "RETRIEVE":
        UseSpDocCreateRetFldValTrg();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          ExitState = "SP0000_FIELD_VAL_RETRVE_REQST_OK";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT") || Equal(global.Command, "PRINT"))
    {
      export.Export1.Count = 0;
      export.Filter.Description = "";
      export.Filter.VersionNumber = "";

      if (!ReadOutgoingDocument())
      {
        ExitState = "OUTGOING_DOCUMENT_NF";

        return;
      }

      if (ReadDocument())
      {
        export.Filter.Assign(entities.Document);

        if (IsEmpty(export.PrintPreview.Flag))
        {
          export.PrintPreview.Flag = entities.Document.PrintPreviewSwitch;
        }
      }
      else
      {
        ExitState = "DOCUMENT_NF";

        return;
      }

      export.HiddenPageKeys.Index = export.Scrolling.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      MoveDocumentField(export.HiddenPageKeys.Item.GexportHidden,
        local.PageStartKey);
      export.Export1.Index = -1;

      foreach(var item in ReadDocumentField2())
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        if (export.Export1.Index >= Export.ExportGroup.Capacity)
        {
          break;
        }

        export.Export1.Update.GdocumentField.Assign(entities.DocumentField);

        if (ReadFieldValue())
        {
          export.Export1.Update.GfieldValue.Value = entities.FieldValue.Value;
          export.Export1.Update.GexportPrevious.Value =
            entities.FieldValue.Value;
        }
        else
        {
          export.Export1.Update.GfieldValue.Value =
            Spaces(FieldValue.Value_MaxLength);
          export.Export1.Update.GexportPrevious.Value =
            Spaces(FieldValue.Value_MaxLength);
        }

        if (ReadField())
        {
          MoveField(entities.Field, export.Export1.Update.GexportHidden);
        }
        else
        {
          var field1 = GetField(export.Export1.Item.GfieldValue, "value");

          field1.Error = true;

          ExitState = "FIELD_NF";

          return;
        }

        if (IsEmpty(export.Export1.Item.GfieldValue.Value))
        {
          // mjr
          // ----------------------------------------
          // 07/10/2000
          // Added code to support new Required Field indicator value 'U'
          // -----------------------------------------------------
          if (Equal(entities.Field.SubroutineName, "USRINPUT"))
          {
            if (AsChar(export.Export1.Item.GdocumentField.RequiredSwitch) == 'Y'
              )
            {
              var field1 = GetField(export.Export1.Item.GfieldValue, "value");

              field1.Color = "green";
              field1.Intensity = Intensity.High;
              field1.Highlighting = Highlighting.ReverseVideo;
              field1.Protected = false;
              field1.Focused = true;
            }
            else
            {
              var field1 = GetField(export.Export1.Item.GfieldValue, "value");

              field1.Color = "green";
              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;
              field1.Focused = true;
            }
          }
          else if (AsChar(export.Export1.Item.GdocumentField.RequiredSwitch) ==
            'U')
          {
            var field1 = GetField(export.Export1.Item.GfieldValue, "value");

            field1.Color = "green";
            field1.Intensity = Intensity.High;
            field1.Highlighting = Highlighting.ReverseVideo;
            field1.Protected = false;
            field1.Focused = true;
          }
          else if (AsChar(export.Export1.Item.GdocumentField.RequiredSwitch) ==
            'Y')
          {
            var field1 = GetField(export.Export1.Item.GfieldValue, "value");

            field1.Error = true;
          }
          else
          {
            var field1 = GetField(export.Export1.Item.GfieldValue, "value");

            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = true;
          }
        }
      }

      if (export.Export1.IsEmpty)
      {
        // mjr
        // -------------------------------------------------------------
        // This will only happen on the first page, because on following
        // pages we check to make sure a record exists even before we scroll.
        // ----------------------------------------------------------------
        export.Scrolling.ScrollingMessage = "MORE";
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else if (export.Export1.IsFull && export.Export1.Index >= Export
        .ExportGroup.Capacity)
      {
        // mjr
        // -------------------------------------------------------------
        // This page is full AND we found at least one more record to occupy
        // another page.
        // ----------------------------------------------------------------
        if (export.HiddenPageKeys.Index + 1 == Export
          .HiddenPageKeysGroup.Capacity)
        {
          // mjr
          // -------------------------------------------------------------
          // The user has scrolled the maximum number of pages, and at
          // least one more record exists.
          // ----------------------------------------------------------------
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";
          export.Scrolling.ScrollingMessage = "MORE - +";
        }
        else
        {
          // mjr
          // -------------------------------------------------------------
          // The user has NOT scrolled the maximum number of pages, and at
          // least one more record exists.
          // ----------------------------------------------------------------
          export.HiddenPageKeys.Index = export.Scrolling.PageNumber;
          export.HiddenPageKeys.CheckSize();

          MoveDocumentField(entities.DocumentField,
            export.HiddenPageKeys.Update.GexportHidden);

          if (export.Scrolling.PageNumber > 1)
          {
            export.Scrolling.ScrollingMessage = "MORE - +";
          }
          else
          {
            export.Scrolling.ScrollingMessage = "MORE   +";
          }
        }
      }
      else
      {
        // mjr
        // -------------------------------------------------------------
        // This page is not full (or it is full but no more records exist).
        // User cannot scroll anymore.
        // ----------------------------------------------------------------
        export.HiddenPageKeys.Index = export.Scrolling.PageNumber;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportHidden.Position = 0;
        export.HiddenPageKeys.Update.GexportHidden.ScreenPrompt = "";

        if (export.Scrolling.PageNumber > 1)
        {
          export.Scrolling.ScrollingMessage = "MORE -";
        }
        else
        {
          export.Scrolling.ScrollingMessage = "MORE";
        }
      }

      // mjr
      // ------------------------------------------------------
      // 08/08/2000
      // PR# 99884 - Field values are archived but we can display
      // the ' KEY' fields
      // -------------------------------------------------------------------
      if (AsChar(entities.OutgoingDocument.FieldValuesArchiveInd) == 'Y')
      {
        ExitState = "SP0000_FIELD_VALUES_ARCHIVED";

        return;
      }

      if (Equal(export.Previous.Command, "PRINT"))
      {
        local.MiscText.Text50 = export.NextTranInfo.MiscText2 ?? Spaces(50);
        UseSpPrintDecodeReturnCode();

        if (IsExitState("SP0000_DOWNLOAD_UNSUCCESSFUL"))
        {
          if (AsChar(export.RequiredFieldMissing.Flag) == 'Y')
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
          else if (AsChar(export.UserInputField.Flag) == 'Y')
          {
            ExitState = "SP0000_PRINT_USER_INPUT_FIELD";
          }
        }

        // mjr
        // ---------------------------------------
        // 09/11/1998
        // Determine if we should hide the screen.
        // ----------------------------------------------------
        if (AsChar(export.RequiredFieldMissing.Flag) == 'N' && AsChar
          (export.UserInputField.Flag) == 'N')
        {
          if (AsChar(export.PrintPreview.Flag) == '1')
          {
            if (Equal(global.Command, "PRINT"))
            {
              export.PrintPreview.Flag = "N";
            }
            else
            {
              ExitState = "SP0000_PRINT_PREVIEW";
            }
          }
          else if (AsChar(export.PrintPreview.Flag) == 'Y')
          {
            ExitState = "SP0000_PRINT_PREVIEW";

            if (Equal(global.Command, "PRINT"))
            {
              export.PrintPreview.Flag = "1";
            }
          }

          if (IsExitState("SP0000_PRINT_CANCELED") || IsExitState
            ("SP0000_DOWNLOAD_UNSUCCESSFUL") || IsExitState
            ("SP0000_PRINT_REQUIRES_LEGAL_DTL") || IsExitState
            ("SP0000_PRINT_PREVIEW"))
          {
            // mjr
            // ---------------------------------------
            // 12/29/1998
            // These exitstates indicate that the print process
            // should not continue.
            // ----------------------------------------------------
          }
          else
          {
            // mjr
            // -----------------------------------------------
            // Switch the Field name displayed on the screen to
            // the actual field name.
            // --------------------------------------------------
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (!export.Export1.CheckSize())
              {
                break;
              }

              export.Export1.Update.GdocumentField.ScreenPrompt =
                export.Export1.Item.GexportHidden.Name;
            }

            export.Export1.CheckIndex();
          }
        }
      }
      else if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveDocumentField(DocumentField source,
    DocumentField target)
  {
    target.Position = source.Position;
    target.ScreenPrompt = source.ScreenPrompt;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Name = source.Name;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveFieldValue(FieldValue source, FieldValue target)
  {
    target.Value = source.Value;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatdTstamp = source.LastUpdatdTstamp;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.ScrollingMessage = source.ScrollingMessage;
    target.PageNumber = source.PageNumber;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveCommon(local.PrintProcess, useImport.PrintProcess);
    useImport.NextTranInfo.Assign(export.NextTranInfo);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    MoveFieldValue(local.FieldValue, useImport.FieldValue);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = export.Export1.Item.GexportHidden.Name;

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateInfrastructure()
  {
    var useImport = new SpCabUpdateInfrastructure.Import();
    var useExport = new SpCabUpdateInfrastructure.Export();

    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabUpdateInfrastructure.Execute, useImport, useExport);
  }

  private void UseSpDocCreateRetFldValTrg()
  {
    var useImport = new SpDocCreateRetFldValTrg.Import();
    var useExport = new SpDocCreateRetFldValTrg.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;

    Call(SpDocCreateRetFldValTrg.Execute, useImport, useExport);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    local.SpDocLiteral.IdDocument = useExport.SpDocLiteral.IdDocument;
  }

  private void UseSpDocUpdateSuccessfulPrint()
  {
    var useImport = new SpDocUpdateSuccessfulPrint.Import();
    var useExport = new SpDocUpdateSuccessfulPrint.Export();

    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(SpDocUpdateSuccessfulPrint.Execute, useImport, useExport);
  }

  private void UseSpPrintDataRetrievalMain()
  {
    var useImport = new SpPrintDataRetrievalMain.Import();
    var useExport = new SpPrintDataRetrievalMain.Export();

    useImport.NextTranInfo.LastTran = export.NextTranInfo.LastTran;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;
    useImport.Document.Name = export.Filter.Name;

    Call(SpPrintDataRetrievalMain.Execute, useImport, useExport);

    local.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    local.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    local.Infrastructure.Assign(useExport.Infrastructure);
    export.ErrorInd.Flag = useExport.ErrorInd.Flag;
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.MiscText.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);
  }

  private void UseSpPrintManipulateKeys()
  {
    var useImport = new SpPrintManipulateKeys.Import();
    var useExport = new SpPrintManipulateKeys.Export();

    useImport.KeyListing.Text50 = local.MiscText.Text50;
    useImport.NewKeyName.Text33 = local.KeyName.Text33;
    useImport.NewKeyValue.Text33 = local.KeyValue.Text33;

    Call(SpPrintManipulateKeys.Execute, useImport, useExport);

    local.MiscText.Text50 = useExport.KeyListing.Text50;
  }

  private bool ReadDocument()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.OutgoingDocument.DocEffectiveDte.GetValueOrDefault());
        db.SetString(command, "name", entities.OutgoingDocument.DocName ?? "");
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Description = db.GetNullableString(reader, 1);
        entities.Document.EffectiveDate = db.GetDate(reader, 2);
        entities.Document.PrintPreviewSwitch = db.GetString(reader, 3);
        entities.Document.VersionNumber = db.GetString(reader, 4);
        entities.Document.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDocumentField1()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.DocumentField.Populated = false;

    return ReadEach("ReadDocumentField1",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.OutgoingDocument.DocEffectiveDte.GetValueOrDefault());
        db.
          SetString(command, "docName", entities.OutgoingDocument.DocName ?? "");
          
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.ScreenPrompt = db.GetString(reader, 2);
        entities.DocumentField.FldName = db.GetString(reader, 3);
        entities.DocumentField.DocName = db.GetString(reader, 4);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 5);
        entities.DocumentField.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDocumentField2()
  {
    entities.DocumentField.Populated = false;

    return ReadEach("ReadDocumentField2",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
        db.SetInt32(command, "orderPosition", local.PageStartKey.Position);
        db.SetString(command, "screenPrompt", local.PageStartKey.ScreenPrompt);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.ScreenPrompt = db.GetString(reader, 2);
        entities.DocumentField.FldName = db.GetString(reader, 3);
        entities.DocumentField.DocName = db.GetString(reader, 4);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 5);
        entities.DocumentField.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDocumentFieldOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;
    entities.DocumentField.Populated = false;

    return ReadEach("ReadDocumentFieldOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", export.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.ScreenPrompt = db.GetString(reader, 2);
        entities.DocumentField.FldName = db.GetString(reader, 3);
        entities.DocumentField.DocName = db.GetString(reader, 4);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 5);
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 6);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 7);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 8);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 9);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 10);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 11);
        entities.OutgoingDocument.Populated = true;
        entities.DocumentField.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);

        return true;
      });
  }

  private bool ReadField()
  {
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    entities.Field.Populated = false;

    return Read("ReadField",
      (db, command) =>
      {
        db.SetString(command, "name", entities.DocumentField.FldName);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.SubroutineName = db.GetString(reader, 1);
        entities.Field.Populated = true;
      });
  }

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.DocumentField.DocEffectiveDte.GetValueOrDefault());
        db.SetString(command, "docName", entities.DocumentField.DocName);
        db.SetString(command, "fldName", entities.DocumentField.FldName);
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 5);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 6);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 7);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 9);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 10);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 11);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 12);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 13);
        entities.Infrastructure.UserId = db.GetString(reader, 14);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 17);
        entities.Infrastructure.Function = db.GetNullableString(reader, 18);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 19);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", export.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 1);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 2);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 4);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 5);
        entities.OutgoingDocument.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
      });
  }

  private bool ReadOutgoingDocumentDocument()
  {
    entities.OutgoingDocument.Populated = false;
    entities.Document.Populated = false;

    return Read("ReadOutgoingDocumentDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", export.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 1);
        entities.Document.Name = db.GetString(reader, 1);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 2);
        entities.Document.EffectiveDate = db.GetDate(reader, 2);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 4);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 5);
        entities.Document.Description = db.GetNullableString(reader, 6);
        entities.Document.PrintPreviewSwitch = db.GetString(reader, 7);
        entities.Document.VersionNumber = db.GetString(reader, 8);
        entities.OutgoingDocument.Populated = true;
        entities.Document.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
      });
  }

  private bool ReadTransaction()
  {
    entities.Transaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "trancode", export.NextTranInfo.LastTran ?? "");
      },
      (db, reader) =>
      {
        entities.Transaction.ScreenId = db.GetString(reader, 0);
        entities.Transaction.Trancode = db.GetString(reader, 1);
        entities.Transaction.Populated = true;
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
      /// A value of GdocumentField.
      /// </summary>
      [JsonPropertyName("gdocumentField")]
      public DocumentField GdocumentField
      {
        get => gdocumentField ??= new();
        set => gdocumentField = value;
      }

      /// <summary>
      /// A value of GfieldValue.
      /// </summary>
      [JsonPropertyName("gfieldValue")]
      public FieldValue GfieldValue
      {
        get => gfieldValue ??= new();
        set => gfieldValue = value;
      }

      /// <summary>
      /// A value of GimportPrevious.
      /// </summary>
      [JsonPropertyName("gimportPrevious")]
      public FieldValue GimportPrevious
      {
        get => gimportPrevious ??= new();
        set => gimportPrevious = value;
      }

      /// <summary>
      /// A value of GimportHidden.
      /// </summary>
      [JsonPropertyName("gimportHidden")]
      public Field GimportHidden
      {
        get => gimportHidden ??= new();
        set => gimportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 16;

      private DocumentField gdocumentField;
      private FieldValue gfieldValue;
      private FieldValue gimportPrevious;
      private Field gimportHidden;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GimportHidden.
      /// </summary>
      [JsonPropertyName("gimportHidden")]
      public DocumentField GimportHidden
      {
        get => gimportHidden ??= new();
        set => gimportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private DocumentField gimportHidden;
    }

    /// <summary>
    /// A value of ErrorInd.
    /// </summary>
    [JsonPropertyName("errorInd")]
    public Common ErrorInd
    {
      get => errorInd ??= new();
      set => errorInd = value;
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
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Document Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of PrintPreview.
    /// </summary>
    [JsonPropertyName("printPreview")]
    public Common PrintPreview
    {
      get => printPreview ??= new();
      set => printPreview = value;
    }

    /// <summary>
    /// A value of RequiredFieldMissing.
    /// </summary>
    [JsonPropertyName("requiredFieldMissing")]
    public Common RequiredFieldMissing
    {
      get => requiredFieldMissing ??= new();
      set => requiredFieldMissing = value;
    }

    /// <summary>
    /// A value of UserInputField.
    /// </summary>
    [JsonPropertyName("userInputField")]
    public Common UserInputField
    {
      get => userInputField ??= new();
      set => userInputField = value;
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
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    private Common errorInd;
    private Infrastructure infrastructure;
    private Document filter;
    private Common printPreview;
    private Common requiredFieldMissing;
    private Common userInputField;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard scrolling;
    private NextTranInfo nextTranInfo;
    private Standard standard;
    private Standard previous;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GexportHidden.
      /// </summary>
      [JsonPropertyName("gexportHidden")]
      public DocumentField GexportHidden
      {
        get => gexportHidden ??= new();
        set => gexportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private DocumentField gexportHidden;
    }

    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of GdocumentField.
      /// </summary>
      [JsonPropertyName("gdocumentField")]
      public DocumentField GdocumentField
      {
        get => gdocumentField ??= new();
        set => gdocumentField = value;
      }

      /// <summary>
      /// A value of GfieldValue.
      /// </summary>
      [JsonPropertyName("gfieldValue")]
      public FieldValue GfieldValue
      {
        get => gfieldValue ??= new();
        set => gfieldValue = value;
      }

      /// <summary>
      /// A value of GexportPrevious.
      /// </summary>
      [JsonPropertyName("gexportPrevious")]
      public FieldValue GexportPrevious
      {
        get => gexportPrevious ??= new();
        set => gexportPrevious = value;
      }

      /// <summary>
      /// A value of GexportHidden.
      /// </summary>
      [JsonPropertyName("gexportHidden")]
      public Field GexportHidden
      {
        get => gexportHidden ??= new();
        set => gexportHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 16;

      private DocumentField gdocumentField;
      private FieldValue gfieldValue;
      private FieldValue gexportPrevious;
      private Field gexportHidden;
    }

    /// <summary>
    /// A value of ErrorInd.
    /// </summary>
    [JsonPropertyName("errorInd")]
    public Common ErrorInd
    {
      get => errorInd ??= new();
      set => errorInd = value;
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
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Document Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of PrintPreview.
    /// </summary>
    [JsonPropertyName("printPreview")]
    public Common PrintPreview
    {
      get => printPreview ??= new();
      set => printPreview = value;
    }

    /// <summary>
    /// A value of RequiredFieldMissing.
    /// </summary>
    [JsonPropertyName("requiredFieldMissing")]
    public Common RequiredFieldMissing
    {
      get => requiredFieldMissing ??= new();
      set => requiredFieldMissing = value;
    }

    /// <summary>
    /// A value of UserInputField.
    /// </summary>
    [JsonPropertyName("userInputField")]
    public Common UserInputField
    {
      get => userInputField ??= new();
      set => userInputField = value;
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
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    private Common errorInd;
    private Infrastructure infrastructure;
    private Document filter;
    private Common printPreview;
    private Common requiredFieldMissing;
    private Common userInputField;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Array<ExportGroup> export1;
    private Standard scrolling;
    private NextTranInfo nextTranInfo;
    private Standard standard;
    private Standard previous;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of PrintProcess.
    /// </summary>
    [JsonPropertyName("printProcess")]
    public Common PrintProcess
    {
      get => printProcess ??= new();
      set => printProcess = value;
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
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of SkipDataRetrievalCab.
    /// </summary>
    [JsonPropertyName("skipDataRetrievalCab")]
    public Common SkipDataRetrievalCab
    {
      get => skipDataRetrievalCab ??= new();
      set => skipDataRetrievalCab = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of Position2.
    /// </summary>
    [JsonPropertyName("position2")]
    public Common Position2
    {
      get => position2 ??= new();
      set => position2 = value;
    }

    /// <summary>
    /// A value of ErrorInd.
    /// </summary>
    [JsonPropertyName("errorInd")]
    public Common ErrorInd
    {
      get => errorInd ??= new();
      set => errorInd = value;
    }

    /// <summary>
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

    /// <summary>
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    /// <summary>
    /// A value of PageStartKey.
    /// </summary>
    [JsonPropertyName("pageStartKey")]
    public DocumentField PageStartKey
    {
      get => pageStartKey ??= new();
      set => pageStartKey = value;
    }

    /// <summary>
    /// A value of Position1.
    /// </summary>
    [JsonPropertyName("position1")]
    public Common Position1
    {
      get => position1 ??= new();
      set => position1 = value;
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
    /// A value of MiscText.
    /// </summary>
    [JsonPropertyName("miscText")]
    public WorkArea MiscText
    {
      get => miscText ??= new();
      set => miscText = value;
    }

    /// <summary>
    /// A value of KeyValue.
    /// </summary>
    [JsonPropertyName("keyValue")]
    public WorkArea KeyValue
    {
      get => keyValue ??= new();
      set => keyValue = value;
    }

    /// <summary>
    /// A value of KeyName.
    /// </summary>
    [JsonPropertyName("keyName")]
    public WorkArea KeyName
    {
      get => keyName ??= new();
      set => keyName = value;
    }

    private Field field;
    private CsePerson csePerson;
    private Common printProcess;
    private DateWorkArea null1;
    private SpDocLiteral spDocLiteral;
    private Common skipDataRetrievalCab;
    private Infrastructure infrastructure;
    private FieldValue fieldValue;
    private Common position2;
    private Common errorInd;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
    private DocumentField pageStartKey;
    private Common position1;
    private OutgoingDocument outgoingDocument;
    private WorkArea miscText;
    private WorkArea keyValue;
    private WorkArea keyName;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of RetrieveFieldValueTrigger.
    /// </summary>
    [JsonPropertyName("retrieveFieldValueTrigger")]
    public RetrieveFieldValueTrigger RetrieveFieldValueTrigger
    {
      get => retrieveFieldValueTrigger ??= new();
      set => retrieveFieldValueTrigger = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
    }

    private CsePerson csePerson;
    private RetrieveFieldValueTrigger retrieveFieldValueTrigger;
    private Field field;
    private OutgoingDocument outgoingDocument;
    private Document document;
    private DocumentField documentField;
    private FieldValue fieldValue;
    private Infrastructure infrastructure;
    private Transaction transaction;
  }
#endregion
}
