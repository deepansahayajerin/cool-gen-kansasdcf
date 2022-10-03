// Program: SP_DEML_DOCUMENT_EMAIL, ID: 371055495, model: 746.
// Short name: SWEDEMLP
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
/// A program: SP_DEML_DOCUMENT_EMAIL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpDemlDocumentEmail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DEML_DOCUMENT_EMAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDemlDocumentEmail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDemlDocumentEmail.
  /// </summary>
  public SpDemlDocumentEmail(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 01/15/2001	M Ramirez	WR 187		Initial Development.  Copied from DDOC
    // 03/06/2002	M Ramirez	PR140827	After emailing the document, send it also 
    // through batch
    // 07/23/2003	GVandy		PR179129	Set export_error_ind to spaces before going 
    // to next document.
    // 09/27/2006      J Bahre         PR285545        Replaced Exit State '
    // Infrastructure Record Not Found'
    // with a new Exit State so problem with error on screen can be located more
    // easily.
    // ----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.FieldValue.LastUpdatedBy = global.UserId;
    local.FieldValue.LastUpdatdTstamp = Now();
    UseSpDocSetLiterals();
    export.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    local.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    export.NextTranInfo.Assign(import.NextTranInfo);

    // mjr
    // ----------------------------------------------------
    // 01/16/2001
    // Command not active on screen
    // -----------------------------------------------------------------
    // ----------------------------------------------------------------------
    // MOVE IMPORTS TO EXPORTS
    // ----------------------------------------------------------------------
    export.FilterDocument.Assign(import.FilterDocument);
    MoveServiceProvider(import.FilterServiceProvider,
      export.FilterServiceProvider);
    export.Previous.Command = import.Previous.Command;
    export.RequiredFieldMissing.Flag = import.RequiredFieldMissing.Flag;
    export.UserInputField.Flag = import.UserInputField.Flag;
    export.PrintPreview.Flag = import.PrintPreview.Flag;
    export.EmailMessageType.Text3 = import.EmailMessageType.Text3;
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
      UseScCabNextTranPut();

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

      return;
    }

    // mjr
    // ----------------------------------------------------
    // 01/16/2001
    // Command not active on screen
    // -----------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "PRINT") && !
      Equal(local.FieldValue.LastUpdatedBy, "SWKSPDC"))
    {
      // mjr
      // ----------------------------------------------
      // 01/15/2001
      // The only user authorized to Print from this screen is the
      // automated email system.
      // ----------------------------------------------------------
      ExitState = "SC0001_USER_NOT_AUTH_COMMAND";

      return;
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
    // 01/16/2001
    // Cannot be any USRINPUT fields (since the user is the automated system)
    // -----------------------------------------------------------------
    // ----------------------------------------------------------------------
    //                 C A S E   O F    C O M M A N D
    // ----------------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.Previous.Command = "";
        export.PrintPreview.Flag = "";
        export.RequiredFieldMissing.Flag = "N";
        export.UserInputField.Flag = "N";
        export.FilterDocument.Assign(local.NullDocument);
        MoveServiceProvider(local.NullServiceProvider,
          export.FilterServiceProvider);
        export.Infrastructure.SystemGeneratedIdentifier = 0;

        if (ReadInfrastructureOutgoingDocument1())
        {
          export.Infrastructure.SystemGeneratedIdentifier =
            entities.Infrastructure.SystemGeneratedIdentifier;
        }

        if (export.Infrastructure.SystemGeneratedIdentifier == 0)
        {
          export.Export1.Count = 0;
          ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";

          return;
        }

        break;
      case "PRINT":
        export.Previous.Command = global.Command;
        export.FilterDocument.Assign(local.NullDocument);
        MoveServiceProvider(local.NullServiceProvider,
          export.FilterServiceProvider);
        export.Infrastructure.SystemGeneratedIdentifier = 0;

        if (ReadOutgoingDocumentInfrastructure())
        {
          export.Infrastructure.SystemGeneratedIdentifier =
            entities.Infrastructure.SystemGeneratedIdentifier;
        }

        if (export.Infrastructure.SystemGeneratedIdentifier == 0)
        {
          export.Export1.Count = 0;
          export.Previous.Command = "";
          ExitState = "ACO_NI0000_PROCESSING_COMPLETE";

          return;
        }

        if (ReadDocument1())
        {
          export.FilterDocument.Assign(entities.Document);

          // mjr
          // --------------------------------------
          // 03/06/2002
          // If the Print Preview switch is turned on set the
          // 'email message type' to FYI.
          // That tells the KAECSES Automated eMail program
          // to send a message to notify the user that this is their
          // Print Preview
          // --------------------------------------------------
          // mjr
          // --------------------------------------
          // 01/15/2001
          // Print Preview is impossible, so it will be bypassed
          // no matter the value
          // --------------------------------------------------
          export.PrintPreview.Flag = entities.Document.PrintPreviewSwitch;

          if (AsChar(export.PrintPreview.Flag) == 'Y')
          {
            export.EmailMessageType.Text3 = "FYI";
          }
          else
          {
            export.EmailMessageType.Text3 = "";
          }
        }
        else
        {
          export.Previous.Command = "";
          ExitState = "SP0000_DOCUMENT_NF";

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
            export.FilterDocument.Description = "";
            export.FilterDocument.VersionNumber = "";

            var field1 = GetField(export.FilterDocument, "name");

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
          else if (IsEmpty(local.Infrastructure.UserId))
          {
            local.ErrorInd.Flag = "3";
          }
          else if (ReadServiceProvider2())
          {
            if (IsEmpty(entities.ServiceProvider.EmailAddress))
            {
              local.ErrorInd.Flag = "3";
            }
          }
          else
          {
            local.ErrorInd.Flag = "3";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Previous.Command = "";
            UseEabRollbackCics();

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
        // 03/06/2002
        // PR139864 - Removed AND print_preview = N from the following if 
        // statement
        // --------------------------------------------------------------------
        // mjr
        // -------------------------------------------------------
        // 11/15/1999
        // Changed processing to check for successful documents
        // rather than unsuccessful documents
        // --------------------------------------------------------------------
        if (AsChar(export.RequiredFieldMissing.Flag) == 'N' && AsChar
          (export.UserInputField.Flag) == 'N' && (
            AsChar(local.ErrorInd.Flag) == '0' || AsChar
          (local.ErrorInd.Flag) == '1'))
        {
          // mjr
          // -------------------------------------------------------
          // 03/06/2002
          // PR139864 - If the print preview switch is set to Y then don't
          // update as a regular document.  Instead prepare the document for 
          // batch.
          // --------------------------------------------------------------------
          if (AsChar(export.PrintPreview.Flag) == 'Y')
          {
            local.Document.Name = export.FilterDocument.Name;
            local.Document.EffectiveDate = entities.Document.EffectiveDate;
            local.Document.Type1 = "BTCH";
            local.Field.Dependancy = " KEY";
            UseSpPrintDataRetrievalKeys();
            UseSpCreateDocumentInfrastruct();
          }
          else
          {
            // mjr
            // -----------------------------------------------------------------
            // Document printing is successful.
            // Update outgoing_document, create monitored document (if necessary
            // ),
            // and update infrastructure record.
            // --------------------------------------------------------------------
            local.Infrastructure.LastUpdatedBy = "";
            local.Infrastructure.LastUpdatedTimestamp =
              local.NullDateWorkArea.Timestamp;
            UseSpDocUpdateSuccessfulPrint();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }
          }
        }
        else
        {
          // mjr
          // -----------------------------------------------------------------
          // Document printing is unsuccessful.
          // --------------------------------------------------------------------
          local.OutgoingDocument.PrintSucessfulIndicator = "N";
          local.OutgoingDocument.LastUpdatedBy = local.FieldValue.LastUpdatedBy;
          local.OutgoingDocument.LastUpdatdTstamp =
            local.FieldValue.LastUpdatdTstamp;
          UseUpdateOutgoingDocument();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          // mjr
          // ----------------------------------------------------------
          // Some values are received from sp_print_data_retrieval_main
          // Update infrastructure now to record them
          // -------------------------------------------------------------
          local.Infrastructure.Detail =
            "Document was not successfully printed.";
          UseSpCabUpdateInfrastructure();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

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
            // The return_code is set, don't overwrite it
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
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "PREV":
        if (export.Infrastructure.SystemGeneratedIdentifier == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE";

          return;
        }

        if (export.Scrolling.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.Scrolling.PageNumber;

        break;
      case "NEXT":
        if (export.Infrastructure.SystemGeneratedIdentifier == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE";

          return;
        }

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
      case "NEXT1":
        export.ErrorInd.Flag = "";
        export.Previous.Command = "";
        export.PrintPreview.Flag = "";
        export.RequiredFieldMissing.Flag = "N";
        export.UserInputField.Flag = "N";
        export.FilterDocument.Assign(local.NullDocument);
        MoveServiceProvider(local.NullServiceProvider,
          export.FilterServiceProvider);

        if (export.Infrastructure.SystemGeneratedIdentifier == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE";

          return;
        }

        if (ReadInfrastructureOutgoingDocument3())
        {
          local.OutgoingDocument.LastUpdatdTstamp =
            entities.OutgoingDocument.LastUpdatdTstamp;
          local.Infrastructure.SystemGeneratedIdentifier =
            entities.Infrastructure.SystemGeneratedIdentifier;
        }
        else
        {
          export.Export1.Count = 0;

          // -------------------------------------------------------
          // JLB  PR285545 09/27/2006 Added a new exit state.
          // -------------------------------------------------------
          ExitState = "INFRASTRUCTURE_NF_1";

          return;
        }

        export.Infrastructure.SystemGeneratedIdentifier = 0;

        if (ReadInfrastructureOutgoingDocument2())
        {
          export.Infrastructure.SystemGeneratedIdentifier =
            entities.Infrastructure.SystemGeneratedIdentifier;
        }

        if (export.Infrastructure.SystemGeneratedIdentifier == 0)
        {
          export.Export1.Count = 0;
          ExitState = "NO_MORE_ITEMS_TO_SCROLL";

          return;
        }

        global.Command = "DISPLAY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT") || Equal(global.Command, "PRINT"))
    {
      export.Export1.Count = 0;

      if (ReadInfrastructure())
      {
        export.FilterServiceProvider.UserId = entities.Infrastructure.UserId;

        if (IsEmpty(export.FilterServiceProvider.UserId))
        {
          var field1 = GetField(export.FilterServiceProvider, "userId");

          field1.Error = true;

          ExitState = "SERVICE_PROVIDER_NF";

          goto Read;
        }

        if (ReadServiceProvider1())
        {
          export.FilterServiceProvider.EmailAddress =
            entities.ServiceProvider.EmailAddress;

          if (IsEmpty(export.FilterServiceProvider.EmailAddress))
          {
            var field1 = GetField(export.FilterServiceProvider, "emailAddress");

            field1.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }
        else
        {
          var field1 = GetField(export.FilterServiceProvider, "userId");

          field1.Error = true;

          ExitState = "SERVICE_PROVIDER_NF";
        }
      }
      else
      {
        // ------------------------------------------------------
        // JLB  PR285545 09/27/2006 Added a new exit state.
        // -------------------------------------------------------
        ExitState = "INFRASTRUCTURE_NF_2";

        return;
      }

Read:

      if (!ReadOutgoingDocument())
      {
        ExitState = "OUTGOING_DOCUMENT_NF";

        return;
      }

      if (ReadDocument2())
      {
        export.FilterDocument.Assign(entities.Document);

        if (IsEmpty(export.PrintPreview.Flag))
        {
          // mjr
          // --------------------------------------
          // 03/06/2002
          // If the Print Preview switch is turned on set the
          // 'email message type' to FYI.
          // That tells the KAECSES Automated eMail program
          // to send a message to notify the user that this is their
          // Print Preview
          // --------------------------------------------------
          // mjr
          // --------------------------------------
          // 01/15/2001
          // Print Preview is impossible, so it will be bypassed
          // no matter the value
          // --------------------------------------------------
          export.PrintPreview.Flag = entities.Document.PrintPreviewSwitch;

          if (AsChar(export.PrintPreview.Flag) == 'Y')
          {
            export.EmailMessageType.Text3 = "FYI";
          }
          else
          {
            export.EmailMessageType.Text3 = "";
          }
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

      foreach(var item in ReadDocumentField())
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
      // --------------------------------------
      // 01/16/2001
      // Since we only display unprocessed documents, they
      // can't be archived.
      // --------------------------------------------------
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
          // mjr
          // --------------------------------------
          // 01/16/2001
          // Print Preview is impossible
          // --------------------------------------------------
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

  private static void MoveDocument1(Document source, Document target)
  {
    target.Name = source.Name;
    target.Type1 = source.Type1;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveDocument2(Document source, Document target)
  {
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
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
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveInfrastructure3(Infrastructure source,
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

  private static void MoveInfrastructure4(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.UserId = source.UserId;
    target.EmailAddress = source.EmailAddress;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAdminAction = source.KeyAdminAction;
    target.KeyAdminActionCert = source.KeyAdminActionCert;
    target.KeyAdminAppeal = source.KeyAdminAppeal;
    target.KeyAp = source.KeyAp;
    target.KeyAppointment = source.KeyAppointment;
    target.KeyAr = source.KeyAr;
    target.KeyBankruptcy = source.KeyBankruptcy;
    target.KeyCase = source.KeyCase;
    target.KeyCashRcptDetail = source.KeyCashRcptDetail;
    target.KeyCashRcptEvent = source.KeyCashRcptEvent;
    target.KeyCashRcptSource = source.KeyCashRcptSource;
    target.KeyCashRcptType = source.KeyCashRcptType;
    target.KeyChild = source.KeyChild;
    target.KeyContact = source.KeyContact;
    target.KeyGeneticTest = source.KeyGeneticTest;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
    target.KeyIncarceration = source.KeyIncarceration;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyInfoRequest = source.KeyInfoRequest;
    target.KeyInterstateRequest = source.KeyInterstateRequest;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyLegalActionDetail = source.KeyLegalActionDetail;
    target.KeyLegalReferral = source.KeyLegalReferral;
    target.KeyLocateRequestAgency = source.KeyLocateRequestAgency;
    target.KeyLocateRequestSource = source.KeyLocateRequestSource;
    target.KeyMilitaryService = source.KeyMilitaryService;
    target.KeyObligation = source.KeyObligation;
    target.KeyObligationAdminAction = source.KeyObligationAdminAction;
    target.KeyObligationType = source.KeyObligationType;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyPersonAddress = source.KeyPersonAddress;
    target.KeyRecaptureRule = source.KeyRecaptureRule;
    target.KeyResource = source.KeyResource;
    target.KeyTribunal = source.KeyTribunal;
    target.KeyWorkerComp = source.KeyWorkerComp;
    target.KeyWorksheet = source.KeyWorksheet;
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

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.NextTranInfo);

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

  private void UseSpCabUpdateInfrastructure()
  {
    var useImport = new SpCabUpdateInfrastructure.Import();
    var useExport = new SpCabUpdateInfrastructure.Export();

    MoveInfrastructure1(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabUpdateInfrastructure.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    MoveDocument1(local.Document, useImport.Document);
    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
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

    MoveInfrastructure3(local.Infrastructure, useImport.Infrastructure);

    Call(SpDocUpdateSuccessfulPrint.Execute, useImport, useExport);
  }

  private void UseSpPrintDataRetrievalKeys()
  {
    var useImport = new SpPrintDataRetrievalKeys.Import();
    var useExport = new SpPrintDataRetrievalKeys.Export();

    MoveDocument2(local.Document, useImport.Document);
    useImport.Field.Dependancy = local.Field.Dependancy;
    MoveInfrastructure4(local.Infrastructure, useImport.Infrastructure);

    Call(SpPrintDataRetrievalKeys.Execute, useImport, useExport);

    local.SpDocKey.Assign(useExport.SpDocKey);
  }

  private void UseSpPrintDataRetrievalMain()
  {
    var useImport = new SpPrintDataRetrievalMain.Import();
    var useExport = new SpPrintDataRetrievalMain.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      export.Infrastructure.SystemGeneratedIdentifier;
    useImport.Document.Name = export.FilterDocument.Name;

    Call(SpPrintDataRetrievalMain.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
    local.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    local.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
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
    useImport.NewKeyValue.Text33 = local.KeyValue.Text33;
    useImport.NewKeyName.Text33 = local.KeyName.Text33;

    Call(SpPrintManipulateKeys.Execute, useImport, useExport);

    local.MiscText.Text50 = useExport.KeyListing.Text50;
  }

  private void UseUpdateOutgoingDocument()
  {
    var useImport = new UpdateOutgoingDocument.Import();
    var useExport = new UpdateOutgoingDocument.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    useImport.OutgoingDocument.Assign(local.OutgoingDocument);

    Call(UpdateOutgoingDocument.Execute, useImport, useExport);
  }

  private bool ReadDocument1()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument1",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", export.Infrastructure.SystemGeneratedIdentifier);
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

  private bool ReadDocument2()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Document.Populated = false;

    return Read("ReadDocument2",
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

  private IEnumerable<bool> ReadDocumentField()
  {
    entities.DocumentField.Populated = false;

    return ReadEach("ReadDocumentField",
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
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 8);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 9);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 10);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 11);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 12);
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

  private bool ReadInfrastructureOutgoingDocument1()
  {
    entities.OutgoingDocument.Populated = false;
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructureOutgoingDocument1",
      null,
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 0);
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
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 20);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 22);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 23);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 24);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 25);
        entities.OutgoingDocument.Populated = true;
        entities.Infrastructure.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
      });
  }

  private bool ReadInfrastructureOutgoingDocument2()
  {
    entities.OutgoingDocument.Populated = false;
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructureOutgoingDocument2",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "lastUpdatdTstamp",
          local.OutgoingDocument.LastUpdatdTstamp.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedI",
          local.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 0);
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
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 20);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 22);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 23);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 24);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 25);
        entities.OutgoingDocument.Populated = true;
        entities.Infrastructure.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
      });
  }

  private bool ReadInfrastructureOutgoingDocument3()
  {
    entities.OutgoingDocument.Populated = false;
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructureOutgoingDocument3",
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
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 0);
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
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 20);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 21);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 22);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 23);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 24);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 25);
        entities.OutgoingDocument.Populated = true;
        entities.Infrastructure.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
      });
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 1);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 2);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 4);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.OutgoingDocument.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
      });
  }

  private bool ReadOutgoingDocumentInfrastructure()
  {
    entities.OutgoingDocument.Populated = false;
    entities.Infrastructure.Populated = false;

    return Read("ReadOutgoingDocumentInfrastructure",
      null,
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 1);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 2);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 4);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 5);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 6);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 7);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 8);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 9);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 10);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 11);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 12);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 13);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 15);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 16);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 17);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 18);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 19);
        entities.Infrastructure.UserId = db.GetString(reader, 20);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 21);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 22);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 23);
        entities.Infrastructure.Function = db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.OutgoingDocument.Populated = true;
        entities.Infrastructure.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "userId", export.FilterServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 2);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", local.Infrastructure.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 2);
        entities.ServiceProvider.Populated = true;
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
    /// A value of FilterServiceProvider.
    /// </summary>
    [JsonPropertyName("filterServiceProvider")]
    public ServiceProvider FilterServiceProvider
    {
      get => filterServiceProvider ??= new();
      set => filterServiceProvider = value;
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
    /// A value of FilterDocument.
    /// </summary>
    [JsonPropertyName("filterDocument")]
    public Document FilterDocument
    {
      get => filterDocument ??= new();
      set => filterDocument = value;
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

    /// <summary>
    /// A value of EmailMessageType.
    /// </summary>
    [JsonPropertyName("emailMessageType")]
    public WorkArea EmailMessageType
    {
      get => emailMessageType ??= new();
      set => emailMessageType = value;
    }

    private ServiceProvider filterServiceProvider;
    private Common errorInd;
    private Infrastructure infrastructure;
    private Document filterDocument;
    private Common printPreview;
    private Common requiredFieldMissing;
    private Common userInputField;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard scrolling;
    private NextTranInfo nextTranInfo;
    private Standard standard;
    private Standard previous;
    private WorkArea emailMessageType;
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
    /// A value of FilterServiceProvider.
    /// </summary>
    [JsonPropertyName("filterServiceProvider")]
    public ServiceProvider FilterServiceProvider
    {
      get => filterServiceProvider ??= new();
      set => filterServiceProvider = value;
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
    /// A value of FilterDocument.
    /// </summary>
    [JsonPropertyName("filterDocument")]
    public Document FilterDocument
    {
      get => filterDocument ??= new();
      set => filterDocument = value;
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

    /// <summary>
    /// A value of EmailMessageType.
    /// </summary>
    [JsonPropertyName("emailMessageType")]
    public WorkArea EmailMessageType
    {
      get => emailMessageType ??= new();
      set => emailMessageType = value;
    }

    private ServiceProvider filterServiceProvider;
    private Common errorInd;
    private Infrastructure infrastructure;
    private Document filterDocument;
    private Common printPreview;
    private Common requiredFieldMissing;
    private Common userInputField;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Array<ExportGroup> export1;
    private Standard scrolling;
    private NextTranInfo nextTranInfo;
    private Standard standard;
    private Standard previous;
    private WorkArea emailMessageType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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
    /// A value of NullServiceProvider.
    /// </summary>
    [JsonPropertyName("nullServiceProvider")]
    public ServiceProvider NullServiceProvider
    {
      get => nullServiceProvider ??= new();
      set => nullServiceProvider = value;
    }

    /// <summary>
    /// A value of NullDocument.
    /// </summary>
    [JsonPropertyName("nullDocument")]
    public Document NullDocument
    {
      get => nullDocument ??= new();
      set => nullDocument = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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

    private SpDocKey spDocKey;
    private Document document;
    private ServiceProvider nullServiceProvider;
    private Document nullDocument;
    private Field field;
    private CsePerson csePerson;
    private Common printProcess;
    private DateWorkArea nullDateWorkArea;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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

    private ServiceProvider serviceProvider;
    private CsePerson csePerson;
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
