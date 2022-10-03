// Program: SP_DUDE_DOCUMENT_DETAIL_MAINT, ID: 372107117, model: 746.
// Short name: SWEDUDEP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DUDE_DOCUMENT_DETAIL_MAINT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpDudeDocumentDetailMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DUDE_DOCUMENT_DETAIL_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDudeDocumentDetailMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDudeDocumentDetailMaint.
  /// </summary>
  public SpDudeDocumentDetailMaint(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 10/24/96	Regan Welborn	Initial Development
    // 09/08/1998	M. Ramirez	Post assesment fixes
    // 09/16/1998	M. Ramirez	Added new attributes
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    MoveDocument2(import.Document, export.Document);
    MoveDocument2(import.HiddenDocument, export.HiddenDocument);
    MoveEventDetail(import.EventDetail, export.EventDetail);
    MoveEvent1(import.Event1, export.Event1);
    export.HiddenEvent.ControlNumber = import.HiddenEvent.ControlNumber;
    MoveEventDetail(import.HiddenEventDetail, export.HiddenEventDetail);
    export.PromptDocument.PromptField = import.PromptDocument.PromptField;
    export.PromptScreen.PromptField = import.PromptScreen.PromptField;
    export.PromptBusObj.PromptField = import.PromptBusObj.PromptField;
    export.PromptEventDetail.PromptField = import.PromptEventDetail.PromptField;

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
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
      return;
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      local.NextTranInfo.Assign(import.HiddenNextTranInfo);

      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // Flow from the menu
      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // mjr
    // ---------------------------------------------
    // 09/15/1998
    // Validate prompt fields
    // ----------------------------------------------------------
    switch(AsChar(export.PromptScreen.PromptField))
    {
      case 'S':
        ++local.Prompt.Count;

        break;
      case '+':
        export.PromptScreen.PromptField = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptScreen, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        break;
    }

    switch(AsChar(export.PromptEventDetail.PromptField))
    {
      case 'S':
        ++local.Prompt.Count;

        break;
      case '+':
        export.PromptEventDetail.PromptField = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptEventDetail, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        break;
    }

    switch(AsChar(export.PromptBusObj.PromptField))
    {
      case 'S':
        ++local.Prompt.Count;

        break;
      case '+':
        export.PromptBusObj.PromptField = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptBusObj, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        break;
    }

    switch(AsChar(export.PromptDocument.PromptField))
    {
      case 'S':
        ++local.Prompt.Count;

        break;
      case '+':
        export.PromptDocument.PromptField = "";

        break;
      case ' ':
        break;
      default:
        var field = GetField(export.PromptDocument, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // mjr
    // -------------------------------------------------
    // 09/15/1998
    // Validate number of prompts selected
    // --------------------------------------------------------------
    if (local.Prompt.Count > 1)
    {
      if (AsChar(export.PromptScreen.PromptField) == 'S')
      {
        var field = GetField(export.PromptScreen, "promptField");

        field.Error = true;
      }

      if (Equal(export.EventDetail.DetailName, "S"))
      {
        var field = GetField(export.PromptEventDetail, "promptField");

        field.Error = true;
      }

      if (AsChar(export.PromptBusObj.PromptField) == 'S')
      {
        var field = GetField(export.PromptBusObj, "promptField");

        field.Error = true;
      }

      if (AsChar(export.PromptDocument.PromptField) == 'S')
      {
        var field = GetField(export.PromptDocument, "promptField");

        field.Error = true;
      }

      ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

      return;
    }
    else if (local.Prompt.Count == 1)
    {
      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
        (global.Command, "DELETE"))
      {
        ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
      }
      else if (Equal(global.Command, "DISPLAY") || Equal
        (global.Command, "RETURN"))
      {
        ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
      }
      else if (Equal(global.Command, "LIST") || Equal
        (global.Command, "SIGNOFF"))
      {
        // mjr
        // -------------------------------------------
        // 09/16/1998
        // Continue
        // --------------------------------------------------------
        goto Test;
      }
      else
      {
        // mjr
        // -------------------------------------------
        // 09/16/1998
        // Command is RETCDVL or RETLINK
        // --------------------------------------------------------
        goto Test;
      }

      if (AsChar(export.PromptScreen.PromptField) == 'S')
      {
        var field = GetField(export.PromptScreen, "promptField");

        field.Error = true;
      }

      if (Equal(export.EventDetail.DetailName, "S"))
      {
        var field = GetField(export.PromptEventDetail, "promptField");

        field.Error = true;
      }

      if (AsChar(export.PromptBusObj.PromptField) == 'S')
      {
        var field = GetField(export.PromptBusObj, "promptField");

        field.Error = true;
      }

      if (AsChar(export.PromptDocument.PromptField) == 'S')
      {
        var field = GetField(export.PromptDocument, "promptField");

        field.Error = true;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else if (Equal(global.Command, "LIST"))
    {
      var field = GetField(export.PromptDocument, "promptField");

      field.Error = true;

      ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

      return;
    }

Test:

    // mjr
    // -------------------------------------------------
    // 09/08/1998
    // Pulled common validations out for ADD and UPDATE
    // --------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.Document.Type1))
      {
        var field = GetField(export.Document, "type1");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.EventDetail.DetailName) || export
        .Event1.ControlNumber <= 0)
      {
        // mjr
        // ---------------------------------------------
        // 09/08/1998
        // This field becomes unprotected if made ERROR
        // Set the Video attributes manually
        // ----------------------------------------------------------
        var field1 = GetField(export.EventDetail, "detailName");

        field1.Error = true;

        var field2 = GetField(export.PromptEventDetail, "promptField");

        field2.Protected = false;
        field2.Focused = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.Document.BusinessObject))
      {
        var field = GetField(export.Document, "businessObject");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.Document.Description))
      {
        var field = GetField(export.Document, "description");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (IsEmpty(export.Document.Name))
      {
        var field = GetField(export.Document, "name");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.CodeValue.Cdvalue = export.Document.BusinessObject;
      local.Code.CodeName = "BUSINESS OBJECT CODE";
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) != 'Y')
      {
        var field = GetField(export.Document, "businessObject");

        field.Error = true;

        // mjr
        // ----------------------------------------
        // 09/08/1998
        // The exitstate is not set in the cab
        // Use standard procedures
        // -----------------------------------------------------
        ExitState = "ACO_NE0000_INVALID_CODE";

        return;
      }

      local.CodeValue.Cdvalue = export.Document.Type1;
      local.Code.CodeName = "DOCUMENT TYPE";
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) != 'Y')
      {
        var field = GetField(export.Document, "type1");

        field.Error = true;

        // mjr
        // ----------------------------------------
        // 09/08/1998
        // The exitstate is not set in the cab
        // Use standard procedures
        // -----------------------------------------------------
        ExitState = "ACO_NE0000_INVALID_CODE";

        return;
      }

      // mjr
      // ----------------------------------------------
      // 09/16/1998
      // Add defaulted values
      // -----------------------------------------------------------
      if (IsEmpty(export.Document.VersionNumber))
      {
        export.Document.VersionNumber = "001";
      }

      switch(AsChar(export.Document.PrintPreviewSwitch))
      {
        case 'Y':
          break;
        case 'N':
          break;
        case ' ':
          export.Document.PrintPreviewSwitch = "N";

          break;
        default:
          var field = GetField(export.Document, "printPreviewSwitch");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";

          return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RLCVAL":
        if (Equal(import.PassCode.CodeName, "DOCUMENT TYPE"))
        {
          export.Document.Type1 = import.PassCodeValue.Cdvalue;
          local.Prompt.Count = 4;
        }
        else if (Equal(import.PassCode.CodeName, "BUSINESS OBJECT CODE"))
        {
          export.Document.BusinessObject = import.PassCodeValue.Cdvalue;
          local.Prompt.Count = 2;
        }

        break;
      case "DISPLAY":
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "LIST":
        if (AsChar(export.PromptEventDetail.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_EDLM";
        }
        else if (AsChar(export.PromptDocument.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_DOCM";
        }
        else if (AsChar(export.PromptScreen.PromptField) == 'S')
        {
          export.Pass.CodeName = "DOCUMENT TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }
        else if (AsChar(export.PromptBusObj.PromptField) == 'S')
        {
          export.Pass.CodeName = "BUSINESS OBJECT CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        return;
      case "ADD":
        if (!Equal(export.Document.ExpirationDate, local.Null1.Date) && !
          Equal(export.Document.ExpirationDate, local.Max.Date))
        {
          var field = GetField(export.Document, "expirationDate");

          field.Error = true;

          ExitState = "ACO_NI0000_INVALID_DATE";

          return;
        }

        export.Document.ExpirationDate = local.Max.Date;
        UseSpCabCreateDocument();
        export.Document.ExpirationDate = local.Null1.Date;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
        }
        else
        {
          if (IsExitState("SP0000_EXPIRE_DOCUMENT_BEFOR_ADD"))
          {
            var field = GetField(export.Document, "expirationDate");

            field.Error = true;
          }
          else if (IsExitState("SP0000_EVENT_DETAIL_NF"))
          {
            var field1 = GetField(export.EventDetail, "detailName");

            field1.Error = true;

            var field2 = GetField(export.PromptEventDetail, "promptField");

            field2.Protected = false;
            field2.Focused = true;
          }
          else if (IsExitState("DOCUMENT_AE"))
          {
            var field = GetField(export.Document, "name");

            field.Error = true;
          }
          else
          {
          }

          return;
        }

        break;
      case "UPDATE":
        if (!Equal(export.Document.Name, export.HiddenDocument.Name))
        {
          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        if (Equal(export.Document.Description, export.HiddenDocument.Description)
          && Equal(export.Document.Type1, export.HiddenDocument.Type1) && Equal
          (export.Document.BusinessObject, export.HiddenDocument.BusinessObject) &&
          Equal
          (export.Document.DetailedDescription,
          export.HiddenDocument.DetailedDescription) && Equal
          (export.Document.Name, export.HiddenDocument.Name) && export
          .Document.RequiredResponseDays == export
          .HiddenDocument.RequiredResponseDays && AsChar
          (export.Document.PrintPreviewSwitch) == AsChar
          (export.HiddenDocument.PrintPreviewSwitch) && Equal
          (export.Document.ExpirationDate, export.HiddenDocument.ExpirationDate) &&
          Equal
          (export.Document.VersionNumber, export.HiddenDocument.VersionNumber))
        {
          if (export.Event1.ControlNumber == export
            .HiddenEvent.ControlNumber && export
            .EventDetail.SystemGeneratedIdentifier == export
            .HiddenEventDetail.SystemGeneratedIdentifier)
          {
            ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";

            return;
          }
        }

        if (Equal(export.Document.ExpirationDate, local.Null1.Date))
        {
          export.Document.ExpirationDate = local.Max.Date;
        }
        else if (Equal(export.Document.ExpirationDate, local.Max.Date))
        {
        }
        else if (!Equal(export.Document.ExpirationDate, local.Current.Date))
        {
          var field = GetField(export.Document, "expirationDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DT_MUST_B_CURRENT_DT";

          return;
        }

        UseSpCabUpdateDocument();

        if (Equal(export.Document.ExpirationDate, local.Max.Date))
        {
          export.Document.ExpirationDate = local.Null1.Date;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
        }
        else
        {
          if (IsExitState("FN0000_CANT_UPDATE_KEYS_CHANGED"))
          {
            var field = GetField(export.EventDetail, "detailName");

            field.Error = true;
          }
          else
          {
            var field = GetField(export.Document, "name");

            field.Error = true;
          }

          return;
        }

        break;
      case "DELETE":
        if (!Equal(export.Document.Name, export.HiddenDocument.Name))
        {
          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        UseSpCabDeleteDocument();

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETLINK":
        if (!IsEmpty(import.Return1.Name))
        {
          MoveDocument1(import.Return1, export.Document);
          global.Command = "DISPLAY";
        }
        else if (AsChar(export.PromptScreen.PromptField) == 'S')
        {
          local.Prompt.Count = 4;
        }
        else if (AsChar(export.PromptDocument.PromptField) == 'S')
        {
          local.Prompt.Count = 1;
        }
        else if (AsChar(export.PromptBusObj.PromptField) == 'S')
        {
          local.Prompt.Count = 2;
        }
        else if (AsChar(export.PromptEventDetail.PromptField) == 'S')
        {
          if (export.Event1.ControlNumber > 0 && export
            .EventDetail.SystemGeneratedIdentifier > 0)
          {
            if (ReadEvent())
            {
              MoveEvent1(entities.Event1, export.Event1);

              if (ReadEventDetail())
              {
                MoveEventDetail(entities.EventDetail, export.EventDetail);
              }
              else
              {
                ExitState = "SP0000_EVENT_DETAIL_NF";
              }
            }
            else
            {
              ExitState = "SP0000_EVENT_NF";
            }

            local.Prompt.Count = 3;
          }
          else
          {
            local.Prompt.Count = 2;
            export.Event1.Name = "";
            export.EventDetail.DetailName = "";
          }
        }
        else
        {
          // nop
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.Document.Name))
      {
        var field = GetField(export.Document, "name");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      UseSpCabReadDocument();

      if (Equal(export.Document.ExpirationDate, local.Max.Date))
      {
        export.Document.ExpirationDate = local.Null1.Date;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else
      {
        var field = GetField(export.Document, "name");

        field.Error = true;

        return;
      }

      if (ReadEvent())
      {
        MoveEvent1(entities.Event1, export.Event1);
      }
      else
      {
        ExitState = "SP0000_EVENT_NF";

        return;
      }
    }

    // mjr
    // -------------------------------------------
    // 09/23/1998
    // Blank the selection characters
    // --------------------------------------------------------
    export.PromptScreen.PromptField = "";
    export.PromptEventDetail.PromptField = "";
    export.PromptBusObj.PromptField = "";
    export.PromptDocument.PromptField = "";

    switch(local.Prompt.Count)
    {
      case 1:
        var field1 = GetField(export.Document, "description");

        field1.Protected = false;
        field1.Focused = true;

        break;
      case 2:
        var field2 = GetField(export.PromptEventDetail, "promptField");

        field2.Protected = false;
        field2.Focused = true;

        break;
      case 3:
        var field3 = GetField(export.Document, "type1");

        field3.Protected = false;
        field3.Focused = true;

        break;
      case 4:
        var field4 = GetField(export.Document, "versionNumber");

        field4.Protected = false;
        field4.Focused = true;

        break;
      default:
        var field5 = GetField(export.Document, "name");

        field5.Protected = false;
        field5.Focused = true;

        break;
    }
  }

  private static void MoveDocument1(Document source, Document target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
  }

  private static void MoveDocument2(Document source, Document target)
  {
    target.Name = source.Name;
    target.Type1 = source.Type1;
    target.BusinessObject = source.BusinessObject;
    target.RequiredResponseDays = source.RequiredResponseDays;
    target.EffectiveDate = source.EffectiveDate;
    target.ExpirationDate = source.ExpirationDate;
    target.PrintPreviewSwitch = source.PrintPreviewSwitch;
    target.VersionNumber = source.VersionNumber;
    target.Description = source.Description;
    target.DetailedDescription = source.DetailedDescription;
  }

  private static void MoveDocument3(Document source, Document target)
  {
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveEvent1(Event1 source, Event1 target)
  {
    target.ControlNumber = source.ControlNumber;
    target.Name = source.Name;
  }

  private static void MoveEventDetail(EventDetail source, EventDetail target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DetailName = source.DetailName;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
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

    useImport.NextTranInfo.Assign(import.HiddenNextTranInfo);
    useImport.Standard.NextTransaction = import.Standard.NextTransaction;

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

  private void UseSpCabCreateDocument()
  {
    var useImport = new SpCabCreateDocument.Import();
    var useExport = new SpCabCreateDocument.Export();

    MoveEventDetail(export.EventDetail, useImport.EventDetail);
    useImport.Event1.ControlNumber = export.Event1.ControlNumber;
    useImport.Document.Assign(export.Document);

    Call(SpCabCreateDocument.Execute, useImport, useExport);
  }

  private void UseSpCabDeleteDocument()
  {
    var useImport = new SpCabDeleteDocument.Import();
    var useExport = new SpCabDeleteDocument.Export();

    MoveDocument3(export.Document, useImport.Document);

    Call(SpCabDeleteDocument.Execute, useImport, useExport);
  }

  private void UseSpCabReadDocument()
  {
    var useImport = new SpCabReadDocument.Import();
    var useExport = new SpCabReadDocument.Export();

    useImport.Document.Assign(export.Document);

    Call(SpCabReadDocument.Execute, useImport, useExport);

    MoveEventDetail(useExport.CheckEventDetail, export.HiddenEventDetail);
    MoveEventDetail(useExport.EventDetail, export.EventDetail);
    export.HiddenEvent.ControlNumber = useExport.CheckEvent.ControlNumber;
    export.Event1.ControlNumber = useExport.Event1.ControlNumber;
    export.Document.Assign(useExport.Document);
    export.HiddenDocument.Assign(useExport.HiddenCheck);
  }

  private void UseSpCabUpdateDocument()
  {
    var useImport = new SpCabUpdateDocument.Import();
    var useExport = new SpCabUpdateDocument.Export();

    MoveEventDetail(export.HiddenEventDetail, useImport.CheckEventDetail);
    useImport.CheckEvent.ControlNumber = export.HiddenEvent.ControlNumber;
    MoveEventDetail(export.EventDetail, useImport.EventDetail);
    useImport.Event1.ControlNumber = export.Event1.ControlNumber;
    useImport.Document.Assign(export.Document);

    Call(SpCabUpdateDocument.Execute, useImport, useExport);
  }

  private bool ReadEvent()
  {
    entities.Event1.Populated = false;

    return Read("ReadEvent",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", export.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.Event1.Name = db.GetString(reader, 1);
        entities.Event1.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.EveNo = db.GetInt32(reader, 2);
        entities.EventDetail.Populated = true;
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
    /// A value of HiddenDocument.
    /// </summary>
    [JsonPropertyName("hiddenDocument")]
    public Document HiddenDocument
    {
      get => hiddenDocument ??= new();
      set => hiddenDocument = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of HiddenEvent.
    /// </summary>
    [JsonPropertyName("hiddenEvent")]
    public Event1 HiddenEvent
    {
      get => hiddenEvent ??= new();
      set => hiddenEvent = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of HiddenEventDetail.
    /// </summary>
    [JsonPropertyName("hiddenEventDetail")]
    public EventDetail HiddenEventDetail
    {
      get => hiddenEventDetail ??= new();
      set => hiddenEventDetail = value;
    }

    /// <summary>
    /// A value of PromptDocument.
    /// </summary>
    [JsonPropertyName("promptDocument")]
    public Standard PromptDocument
    {
      get => promptDocument ??= new();
      set => promptDocument = value;
    }

    /// <summary>
    /// A value of PromptBusObj.
    /// </summary>
    [JsonPropertyName("promptBusObj")]
    public Standard PromptBusObj
    {
      get => promptBusObj ??= new();
      set => promptBusObj = value;
    }

    /// <summary>
    /// A value of PromptEventDetail.
    /// </summary>
    [JsonPropertyName("promptEventDetail")]
    public Standard PromptEventDetail
    {
      get => promptEventDetail ??= new();
      set => promptEventDetail = value;
    }

    /// <summary>
    /// A value of PromptScreen.
    /// </summary>
    [JsonPropertyName("promptScreen")]
    public Standard PromptScreen
    {
      get => promptScreen ??= new();
      set => promptScreen = value;
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
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public Document Return1
    {
      get => return1 ??= new();
      set => return1 = value;
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

    private Document document;
    private Document hiddenDocument;
    private Event1 event1;
    private Event1 hiddenEvent;
    private EventDetail eventDetail;
    private EventDetail hiddenEventDetail;
    private Standard promptDocument;
    private Standard promptBusObj;
    private Standard promptEventDetail;
    private Standard promptScreen;
    private CodeValue passCodeValue;
    private Code passCode;
    private Document return1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of HiddenDocument.
    /// </summary>
    [JsonPropertyName("hiddenDocument")]
    public Document HiddenDocument
    {
      get => hiddenDocument ??= new();
      set => hiddenDocument = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of HiddenEvent.
    /// </summary>
    [JsonPropertyName("hiddenEvent")]
    public Event1 HiddenEvent
    {
      get => hiddenEvent ??= new();
      set => hiddenEvent = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of HiddenEventDetail.
    /// </summary>
    [JsonPropertyName("hiddenEventDetail")]
    public EventDetail HiddenEventDetail
    {
      get => hiddenEventDetail ??= new();
      set => hiddenEventDetail = value;
    }

    /// <summary>
    /// A value of PromptDocument.
    /// </summary>
    [JsonPropertyName("promptDocument")]
    public Standard PromptDocument
    {
      get => promptDocument ??= new();
      set => promptDocument = value;
    }

    /// <summary>
    /// A value of PromptBusObj.
    /// </summary>
    [JsonPropertyName("promptBusObj")]
    public Standard PromptBusObj
    {
      get => promptBusObj ??= new();
      set => promptBusObj = value;
    }

    /// <summary>
    /// A value of PromptEventDetail.
    /// </summary>
    [JsonPropertyName("promptEventDetail")]
    public Standard PromptEventDetail
    {
      get => promptEventDetail ??= new();
      set => promptEventDetail = value;
    }

    /// <summary>
    /// A value of PromptScreen.
    /// </summary>
    [JsonPropertyName("promptScreen")]
    public Standard PromptScreen
    {
      get => promptScreen ??= new();
      set => promptScreen = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Code Pass
    {
      get => pass ??= new();
      set => pass = value;
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

    private Document document;
    private Document hiddenDocument;
    private Event1 event1;
    private Event1 hiddenEvent;
    private EventDetail eventDetail;
    private EventDetail hiddenEventDetail;
    private Standard promptDocument;
    private Standard promptBusObj;
    private Standard promptEventDetail;
    private Standard promptScreen;
    private Code pass;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Common prompt;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
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

    private Event1 event1;
    private EventDetail eventDetail;
    private Document document;
  }
#endregion
}
