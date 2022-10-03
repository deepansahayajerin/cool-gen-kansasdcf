// Program: SP_EVMN_EVENT_MAINTENANCE, ID: 371778836, model: 746.
// Short name: SWEEVMNP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_EVMN_EVENT_MAINTENANCE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpEvmnEventMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EVMN_EVENT_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEvmnEventMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEvmnEventMaintenance.
  /// </summary>
  public SpEvmnEventMaintenance(IContext context, Import import, Export export):
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
    // Date	  Developer	Request #	Description
    // 10/24/96 Regan Welborn               Initial Development
    // 09/20/98  Anita Massey                 fixes per screen
    //                                         
    // assessment document
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }
    else if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

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
      export.Hidden.Assign(local.NextTranInfo);

      return;
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

    export.Event1.Assign(import.Event1);
    export.HiddenCheck.Assign(import.HiddenCheck);
    export.Name.PromptField = import.Name.PromptField;
    export.Type1.PromptField = import.Type1.PromptField;
    export.BusObj.PromptField = import.BusObj.PromptField;

    if (Equal(global.Command, "RLCVAL") || Equal(global.Command, "RETEVLS") || Equal
      (global.Command, "DISPLAY"))
    {
      export.BusObj.PromptField = "";
      export.Name.PromptField = "";
      export.Type1.PromptField = "";
    }

    if (!Equal(global.Command, "LIST"))
    {
      if (AsChar(export.Name.PromptField) == 'S')
      {
        var field = GetField(export.Name, "promptField");

        field.Error = true;

        if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
          (global.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";

          return;
        }
        else if (Equal(global.Command, "EXIT") || !
          Equal(global.Command, "HELP"))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          return;
        }
      }

      if (AsChar(export.Type1.PromptField) == 'S')
      {
        var field = GetField(export.Type1, "promptField");

        field.Error = true;

        if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
          (global.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";

          return;
        }
        else if (Equal(global.Command, "DISPLAY") || Equal
          (global.Command, "EXIT") || !Equal(global.Command, "HELP"))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          return;
        }
      }

      if (AsChar(export.BusObj.PromptField) == 'S')
      {
        var field = GetField(export.BusObj, "promptField");

        field.Error = true;

        if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
          (global.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";

          return;
        }
        else if (Equal(global.Command, "DISPLAY") || Equal
          (global.Command, "EXIT") || !Equal(global.Command, "HELP"))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          return;
        }
      }
    }
    else
    {
      if (AsChar(export.Name.PromptField) == 'S' || AsChar
        (export.Name.PromptField) == '+' || IsEmpty(export.Name.PromptField))
      {
      }
      else
      {
        var field = GetField(export.Name, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        return;
      }

      if (AsChar(export.Type1.PromptField) == 'S' || AsChar
        (export.Type1.PromptField) == '+' || IsEmpty(export.Type1.PromptField))
      {
      }
      else
      {
        var field = GetField(export.Type1, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        return;
      }

      if (AsChar(export.BusObj.PromptField) == 'S' || AsChar
        (export.BusObj.PromptField) == '+' || IsEmpty
        (export.BusObj.PromptField))
      {
      }
      else
      {
        var field = GetField(export.BusObj, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

        return;
      }
    }

    if (Equal(global.Command, "RLCVAL") || Equal(global.Command, "RETEVLS"))
    {
    }
    else
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
    if (Equal(global.Command, "RETEVLS"))
    {
      if (import.PassEvent.ControlNumber > 0)
      {
        export.Event1.Assign(import.PassEvent);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RLCVAL":
        switch(TrimEnd(import.PassCode.CodeName))
        {
          case "BUSINESS OBJECT CODE":
            if (IsEmpty(import.PassCodeValue.Cdvalue))
            {
            }
            else
            {
              export.Event1.BusinessObjectCode = import.PassCodeValue.Cdvalue;
            }

            break;
          case "EVENT TYPE":
            if (IsEmpty(import.PassCodeValue.Cdvalue))
            {
            }
            else
            {
              export.Event1.Type1 = import.PassCodeValue.Cdvalue;
            }

            break;
          default:
            break;
        }

        break;
      case "DISPLAY":
        if (export.Event1.ControlNumber == 0)
        {
          var field1 = GetField(export.Event1, "controlNumber");

          field1.Error = true;

          var field2 = GetField(export.Event1, "name");

          field2.Error = true;

          export.Name.PromptField = "S";
          ExitState = "SP0000_EVENT_NUMBER_NOT_ENTERED";

          return;
        }

        if (export.Event1.ControlNumber > 0)
        {
          UseSpCabReadEvent();

          if (IsExitState("SP0000_EVENT_NF"))
          {
            export.Event1.Assign(import.Event1);

            var field1 = GetField(export.Event1, "controlNumber");

            field1.Error = true;

            var field2 = GetField(export.Event1, "name");

            field2.Error = true;

            export.Name.PromptField = "S";
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        if (!IsEmpty(export.Name.PromptField))
        {
          if (AsChar(export.Name.PromptField) == 'S')
          {
            ++local.Common.Count;
            ExitState = "ECO_LNK_TO_LIST_EVENT";
          }
          else
          {
            var field = GetField(export.Name, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
          }
        }

        if (!IsEmpty(export.BusObj.PromptField))
        {
          if (AsChar(export.BusObj.PromptField) == 'S')
          {
            ++local.Common.Count;
            export.Pass.CodeName = "BUSINESS OBJECT CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            var field = GetField(export.BusObj, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
          }
        }

        if (!IsEmpty(export.Type1.PromptField))
        {
          if (AsChar(export.Type1.PromptField) == 'S')
          {
            ++local.Common.Count;
            export.Pass.CodeName = "EVENT TYPE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field = GetField(export.Type1, "promptField");

            field.Error = true;

            return;
          }
        }

        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }
        else if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        break;
      case "ADD":
        if (export.Event1.ControlNumber == 0)
        {
          var field = GetField(export.Event1, "controlNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (IsEmpty(export.Event1.Name))
        {
          var field = GetField(export.Event1, "name");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (IsEmpty(export.Event1.Type1))
        {
          var field = GetField(export.Event1, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (IsEmpty(export.Event1.Description))
        {
          var field = GetField(export.Event1, "description");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (IsEmpty(export.Event1.BusinessObjectCode))
        {
          var field = GetField(export.Event1, "businessObjectCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        local.CodeValue.Cdvalue = export.Event1.Type1;
        local.Code.CodeName = "EVENT TYPE";
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.Event1, "type1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Type1.PromptField = "S";
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }
        }

        local.Code.CodeName = "BUSINESS OBJECT CODE";
        local.CodeValue.Cdvalue = export.Event1.BusinessObjectCode;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.Event1, "businessObjectCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.BusObj.PromptField = "S";
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }
        }

        if (ReadEvent())
        {
          var field = GetField(export.Event1, "name");

          field.Error = true;

          ExitState = "SP0000_NAME_AE_FOR_EVENT";

          return;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        // _____________________________________________
        // Data has passed validation, now create event
        // _____________________________________________
        UseSpCabCreateEvent();

        break;
      case "UPDATE":
        if (export.Event1.ControlNumber != export.HiddenCheck.ControlNumber)
        {
          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        if (Equal(export.Event1.Name, export.HiddenCheck.Name) && Equal
          (export.Event1.Type1, export.HiddenCheck.Type1) && Equal
          (export.Event1.BusinessObjectCode,
          export.HiddenCheck.BusinessObjectCode) && Equal
          (export.Event1.Description, export.HiddenCheck.Description))
        {
          ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";

          return;
        }

        if (export.Event1.ControlNumber == 0)
        {
          var field = GetField(export.Event1, "controlNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (IsEmpty(export.Event1.Name))
        {
          var field = GetField(export.Event1, "name");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (IsEmpty(export.Event1.Type1))
        {
          var field = GetField(export.Event1, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (IsEmpty(export.Event1.BusinessObjectCode))
        {
          var field = GetField(export.Event1, "businessObjectCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }
        else if (IsEmpty(export.Event1.Description))
        {
          var field = GetField(export.Event1, "description");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        local.Code.CodeName = "EVENT TYPE";
        local.CodeValue.Cdvalue = export.Event1.Type1;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.Event1, "type1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Type1.PromptField = "S";
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }
        }

        local.Code.CodeName = "BUSINESS OBJECT CODE";
        local.CodeValue.Cdvalue = export.Event1.BusinessObjectCode;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.Event1, "businessObjectCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.BusObj.PromptField = "S";
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

            return;
          }
        }

        if (ReadEvent())
        {
          var field = GetField(export.Event1, "name");

          field.Error = true;

          ExitState = "SP0000_NAME_AE_FOR_EVENT";

          return;
        }

        UseSpCabUpdateEvent();

        if (IsExitState("SP0000_EVENT_NF"))
        {
          var field1 = GetField(export.Event1, "controlNumber");

          field1.Error = true;

          var field2 = GetField(export.Event1, "name");

          field2.Error = true;

          export.Name.PromptField = "S";
          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }

        break;
      case "DELETE":
        if (export.Event1.ControlNumber != export.HiddenCheck.ControlNumber)
        {
          ExitState = "ACO_NE0000_DISPLAY_FIRST";

          return;
        }

        UseSpCabDeleteEvent();

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCabCreateEvent()
  {
    var useImport = new SpCabCreateEvent.Import();
    var useExport = new SpCabCreateEvent.Export();

    useImport.Event1.Assign(import.Event1);

    Call(SpCabCreateEvent.Execute, useImport, useExport);

    export.HiddenCheck.Assign(useExport.HiddenCheck);
    export.Event1.Assign(useExport.Event1);
  }

  private void UseSpCabDeleteEvent()
  {
    var useImport = new SpCabDeleteEvent.Import();
    var useExport = new SpCabDeleteEvent.Export();

    useImport.Event1.ControlNumber = import.Event1.ControlNumber;

    Call(SpCabDeleteEvent.Execute, useImport, useExport);
  }

  private void UseSpCabReadEvent()
  {
    var useImport = new SpCabReadEvent.Import();
    var useExport = new SpCabReadEvent.Export();

    useImport.Event1.ControlNumber = export.Event1.ControlNumber;

    Call(SpCabReadEvent.Execute, useImport, useExport);

    export.HiddenCheck.Assign(useExport.HiddenCheck);
    export.Event1.Assign(useExport.Event1);
  }

  private void UseSpCabUpdateEvent()
  {
    var useImport = new SpCabUpdateEvent.Import();
    var useExport = new SpCabUpdateEvent.Export();

    useImport.Event1.Assign(import.Event1);

    Call(SpCabUpdateEvent.Execute, useImport, useExport);

    export.HiddenCheck.Assign(useExport.HiddenCheck);
    export.Event1.Assign(useExport.Event1);
  }

  private bool ReadEvent()
  {
    entities.Event1.Populated = false;

    return Read("ReadEvent",
      (db, command) =>
      {
        db.SetString(command, "name", export.Event1.Name);
        db.SetInt32(command, "controlNumber", export.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.Event1.Name = db.GetString(reader, 1);
        entities.Event1.Populated = true;
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
    /// A value of PassEvent.
    /// </summary>
    [JsonPropertyName("passEvent")]
    public Event1 PassEvent
    {
      get => passEvent ??= new();
      set => passEvent = value;
    }

    /// <summary>
    /// A value of BusObj.
    /// </summary>
    [JsonPropertyName("busObj")]
    public Standard BusObj
    {
      get => busObj ??= new();
      set => busObj = value;
    }

    /// <summary>
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public Standard Type1
    {
      get => type1 ??= new();
      set => type1 = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public Standard Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Event1 HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
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

    private Event1 passEvent;
    private Standard busObj;
    private Standard type1;
    private Standard name;
    private Event1 hiddenCheck;
    private Event1 event1;
    private NextTranInfo hidden;
    private Standard standard;
    private CodeValue passCodeValue;
    private Code passCode;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of BusObj.
    /// </summary>
    [JsonPropertyName("busObj")]
    public Standard BusObj
    {
      get => busObj ??= new();
      set => busObj = value;
    }

    /// <summary>
    /// A value of Type1.
    /// </summary>
    [JsonPropertyName("type1")]
    public Standard Type1
    {
      get => type1 ??= new();
      set => type1 = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public Standard Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Event1 HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Code Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    private Standard busObj;
    private Standard type1;
    private Standard name;
    private Event1 hiddenCheck;
    private Event1 event1;
    private NextTranInfo hidden;
    private Standard standard;
    private Code pass;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    private Common common;
    private Code code;
    private Common validCode;
    private NextTranInfo nextTranInfo;
    private CodeValue codeValue;
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

    private Event1 event1;
  }
#endregion
}
