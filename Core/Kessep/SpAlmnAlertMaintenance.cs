// Program: SP_ALMN_ALERT_MAINTENANCE, ID: 371748979, model: 746.
// Short name: SWEALMNP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_ALMN_ALERT_MAINTENANCE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpAlmnAlertMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_ALMN_ALERT_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpAlmnAlertMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpAlmnAlertMaintenance.
  /// </summary>
  public SpAlmnAlertMaintenance(IContext context, Import import, Export export):
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
    // 11/01/96 Alan Samuels                Completed Development
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Alert.Assign(import.Alert);
    export.Alert.ControlNumber = import.Alert.ControlNumber;

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        if (!IsEmpty(import.Standard.NextTransaction) && Equal
          (global.Command, "ENTER"))
        {
          // ---------------------------------------------
          // User is going out of this screen to another
          // ---------------------------------------------
          local.NextTranInfo.Assign(import.Hidden);

          // ---------------------------------------------
          // Set up local next_tran_info for saving the current values for the 
          // next screen
          // ---------------------------------------------
          UseScCabNextTranPut();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          var field1 = GetField(export.Standard, "nextTransaction");

          field1.Error = true;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "ADD":
        // Add logic is located at bottom of PrAD.
        break;
      case "DISPLAY":
        // Display logic is located at bottom of PrAD.
        break;
      case "DELETE":
        // Delete logic is located at bottom of PrAD.
        break;
      case "LIST":
        if (AsChar(import.Name.PromptField) == 'S')
        {
          ExitState = "ECO_LNK_TO_ALLS";

          return;
        }
        else
        {
          var field1 = GetField(export.Name, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "UPDATE":
        // Update logic is located at bottom of PrAD.
        break;
      case "RETALLS":
        export.Name.PromptField = "";

        var field = GetField(export.Name, "promptField");

        field.Protected = false;
        field.Focused = true;

        if (!IsEmpty(import.FromLink.Name))
        {
          export.Name.PromptField = "";
          export.Alert.ControlNumber = import.FromLink.ControlNumber;
          export.Alert.Name = import.FromLink.Name;
          global.Command = "DISPLAY";
        }

        break;
      case "XXFMMENU":
        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

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
        export.Hidden.Assign(local.NextTranInfo);

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
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

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.Alert.Assign(import.Alert);
      export.HiddenCheck.Assign(import.HiddenCheck);
      export.Name.PromptField = import.Name.PromptField;
    }

    // ---------------------------------------------
    // An entry in the prompt field is only valid
    // on a PF4 List command.
    // ---------------------------------------------
    // ---------------------------------------------
    // A display must be performed before an add,
    // update, or delete.
    // ---------------------------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (import.Alert.ControlNumber == import.HiddenCheck.ControlNumber)
      {
      }
      else
      {
        var field = GetField(export.Alert, "controlNumber");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_FIRST";

        return;
      }
    }

    // ---------------------------------------------
    // Description is mandatory.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(import.Alert.Description))
      {
        var field = GetField(export.Alert, "description");

        field.Error = true;

        ExitState = "SP0000_REQUIRED_FIELD_MISSING";
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "ADD":
        local.Error.Count = 0;

        // ---------------------------------------------
        // External indicator must be 'Y' or 'N'.
        // Default value is 'N'.
        // ---------------------------------------------
        switch(AsChar(export.Alert.ExternalIndicator))
        {
          case ' ':
            export.Alert.ExternalIndicator = "N";

            break;
          case 'N':
            break;
          case 'Y':
            break;
          default:
            ++local.Error.Count;

            var field = GetField(export.Alert, "externalIndicator");

            field.Error = true;

            ExitState = "SP0000_INVALID_EXTRNL_INDICATOR";

            break;
        }

        // ---------------------------------------------
        // Message is mandatory.
        // ---------------------------------------------
        if (IsEmpty(export.Alert.Message))
        {
          ++local.Error.Count;

          var field = GetField(export.Alert, "message");

          field.Error = true;

          ExitState = "SP0000_REQUIRED_FIELD_MISSING";
        }

        // ---------------------------------------------
        // Name is mandatory.  If entered, validate
        // against alert table.  If not entered, place
        // 'S' in prompt field and set exit state.
        // ---------------------------------------------
        if (IsEmpty(export.Alert.Name))
        {
          ++local.Error.Count;

          var field = GetField(export.Alert, "name");

          field.Error = true;

          export.Name.PromptField = "S";
          ExitState = "SP0000_REQUIRED_FIELD_USE_PF4";
        }
        else if (ReadAlert2())
        {
          ++local.Error.Count;

          var field = GetField(export.Alert, "name");

          field.Error = true;

          ExitState = "SP0000_NAME_AE_FOR_ALERT";
        }
        else
        {
          // Continue
        }

        // ---------------------------------------------
        // If alert is internal, do not allow user to
        // enter an alert number.  Set number to next
        // sequential number > 99.  If alert is
        // external, user must enter a value < 100 that
        // has not already been used.
        // ---------------------------------------------
        if (AsChar(export.Alert.ExternalIndicator) == 'N')
        {
          if (export.Alert.ControlNumber > 0)
          {
            ++local.Error.Count;

            var field = GetField(export.Alert, "controlNumber");

            field.Error = true;

            ExitState = "SP0000_INVALID_CNTRL_NBR";
          }
        }
        else if (export.Alert.ControlNumber < 1 || export
          .Alert.ControlNumber >= 100)
        {
          ++local.Error.Count;

          var field = GetField(export.Alert, "controlNumber");

          field.Error = true;

          ExitState = "SP0000_INVALID_CNTRL_NBR_LESS100";
        }
        else if (ReadAlert1())
        {
          ++local.Error.Count;

          var field = GetField(export.Alert, "controlNumber");

          field.Error = true;

          ExitState = "SP0000_INVALID_CNTRL_NBR_AE";
        }
        else
        {
          // Continue
        }

        switch(local.Error.Count)
        {
          case 0:
            break;
          case 1:
            return;
          default:
            ExitState = "SP0000_MULTIPLE_ERRORS";

            return;
        }

        local.PassTo.Assign(export.Alert);
        UseSpCabCreateAlert();

        if (IsExitState("ACO_NI0000_ADD_SUCCESSFUL"))
        {
          export.Alert.Assign(local.ReturnFrom);
          export.HiddenCheck.Assign(local.ReturnFrom);
        }

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Some data must be changed on an update.
        // ---------------------------------------------
        local.Error.Count = 0;

        if (Equal(import.Alert.Name, import.HiddenCheck.Name) && Equal
          (import.Alert.Message, import.HiddenCheck.Message) && AsChar
          (import.Alert.ExternalIndicator) == AsChar
          (import.HiddenCheck.ExternalIndicator) && Equal
          (import.Alert.Description, import.HiddenCheck.Description) && import
          .Alert.ControlNumber == import.HiddenCheck.ControlNumber)
        {
          ExitState = "SP0000_DATA_NOT_CHANGED";

          return;
        }
        else
        {
          // ---------------------------------------------
          // External indicator may not be updated.
          // ---------------------------------------------
          if (AsChar(import.Alert.ExternalIndicator) == AsChar
            (import.HiddenCheck.ExternalIndicator))
          {
          }
          else
          {
            var field = GetField(export.Alert, "externalIndicator");

            field.Error = true;

            export.Alert.ExternalIndicator =
              import.HiddenCheck.ExternalIndicator;
            ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
            ++local.Error.Count;
          }

          // ---------------------------------------------
          // Message is mandatory.
          // ---------------------------------------------
          if (IsEmpty(import.Alert.Message))
          {
            var field = GetField(export.Alert, "message");

            field.Error = true;

            ExitState = "SP0000_REQUIRED_FIELD_MISSING";
            ++local.Error.Count;
          }
        }

        // ---------------------------------------------
        // Name is required, and if changed, must be unique.
        // ---------------------------------------------
        if (IsEmpty(import.Alert.Name))
        {
          var field = GetField(export.Alert, "name");

          field.Error = true;

          export.Name.PromptField = "S";
          ExitState = "SP0000_REQUIRED_FIELD_USE_PF4";
          ++local.Error.Count;
        }
        else if (Equal(import.Alert.Name, import.HiddenCheck.Name))
        {
        }
        else if (ReadAlert3())
        {
          var field = GetField(export.Alert, "name");

          field.Error = true;

          ExitState = "SP0000_NAME_AE_FOR_ALERT";
          ++local.Error.Count;
        }
        else
        {
          // Continue
        }

        // ---------------------------------------------
        // Control number may not be updated.
        // ---------------------------------------------
        if (import.Alert.ControlNumber == import.HiddenCheck.ControlNumber)
        {
        }
        else
        {
          var field = GetField(export.Alert, "controlNumber");

          field.Error = true;

          export.Alert.ControlNumber = import.HiddenCheck.ControlNumber;
          ExitState = "SP0000_FIELD_NOT_UPDATEABLE";
          ++local.Error.Count;
        }

        switch(local.Error.Count)
        {
          case 0:
            break;
          case 1:
            return;
          default:
            ExitState = "SP0000_MULTIPLE_ERRORS";

            return;
        }

        local.PassTo.Assign(export.Alert);
        UseSpCabUpdateAlert();

        if (IsExitState("ACO_NI0000_UPDATE_SUCCESSFUL"))
        {
          export.Alert.Assign(local.ReturnFrom);
          export.HiddenCheck.Assign(local.ReturnFrom);
        }

        break;
      case "DELETE":
        if (ReadAlertDistributionRule())
        {
          var field = GetField(export.Alert, "controlNumber");

          field.Error = true;

          ExitState = "SP0000_RELATED_DETAILS_EXIST";

          return;
        }
        else
        {
          // Continue
        }

        local.PassTo.Assign(import.Alert);
        UseSpCabDeleteAlert();

        if (IsExitState("ACO_NI0000_DELETE_SUCCESSFUL"))
        {
          export.Alert.Assign(local.ReturnFrom);
          export.HiddenCheck.Assign(local.ReturnFrom);
        }

        break;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (export.Alert.ControlNumber == 0)
      {
        var field1 = GetField(export.Alert, "controlNumber");

        field1.Error = true;

        var field2 = GetField(export.Alert, "name");

        field2.Error = true;

        ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
      }
      else if (ReadAlert1())
      {
        export.Alert.Assign(entities.Alert);
        export.HiddenCheck.Assign(entities.Alert);
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
      }
      else
      {
        var field = GetField(export.Alert, "controlNumber");

        field.Error = true;

        ExitState = "SP0000_ALERT_NF";
        export.Alert.ControlNumber = import.Alert.ControlNumber;
        export.HiddenCheck.ControlNumber = import.Alert.ControlNumber;
      }
    }
  }

  private static void MoveAlert(Alert source, Alert target)
  {
    target.ControlNumber = source.ControlNumber;
    target.Name = source.Name;
    target.Message = source.Message;
    target.Description = source.Description;
    target.ExternalIndicator = source.ExternalIndicator;
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

  private void UseSpCabCreateAlert()
  {
    var useImport = new SpCabCreateAlert.Import();
    var useExport = new SpCabCreateAlert.Export();

    useImport.Alert.Assign(local.PassTo);

    Call(SpCabCreateAlert.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.Alert);
  }

  private void UseSpCabDeleteAlert()
  {
    var useImport = new SpCabDeleteAlert.Import();
    var useExport = new SpCabDeleteAlert.Export();

    MoveAlert(local.PassTo, useImport.Alert);

    Call(SpCabDeleteAlert.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateAlert()
  {
    var useImport = new SpCabUpdateAlert.Import();
    var useExport = new SpCabUpdateAlert.Export();

    MoveAlert(local.PassTo, useImport.Alert);

    Call(SpCabUpdateAlert.Execute, useImport, useExport);

    local.ReturnFrom.Assign(useExport.Alert);
  }

  private bool ReadAlert1()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert1",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", export.Alert.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.ExternalIndicator = db.GetString(reader, 4);
        entities.Alert.Populated = true;
      });
  }

  private bool ReadAlert2()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert2",
      (db, command) =>
      {
        db.SetString(command, "name", export.Alert.Name);
      },
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.ExternalIndicator = db.GetString(reader, 4);
        entities.Alert.Populated = true;
      });
  }

  private bool ReadAlert3()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert3",
      (db, command) =>
      {
        db.SetString(command, "name", import.Alert.Name);
      },
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.ExternalIndicator = db.GetString(reader, 4);
        entities.Alert.Populated = true;
      });
  }

  private bool ReadAlertDistributionRule()
  {
    entities.AlertDistributionRule.Populated = false;

    return Read("ReadAlertDistributionRule",
      (db, command) =>
      {
        db.SetNullableInt32(command, "aleNo", import.Alert.ControlNumber);
      },
      (db, reader) =>
      {
        entities.AlertDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AlertDistributionRule.EveNo = db.GetInt32(reader, 1);
        entities.AlertDistributionRule.EvdId = db.GetInt32(reader, 2);
        entities.AlertDistributionRule.AleNo = db.GetNullableInt32(reader, 3);
        entities.AlertDistributionRule.Populated = true;
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
    /// A value of FromLink.
    /// </summary>
    [JsonPropertyName("fromLink")]
    public Alert FromLink
    {
      get => fromLink ??= new();
      set => fromLink = value;
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
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    /// <summary>
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Alert HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
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

    private Alert fromLink;
    private Standard standard;
    private NextTranInfo hidden;
    private Alert alert;
    private Alert hiddenCheck;
    private Standard name;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    /// <summary>
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Alert HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
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

    private Standard standard;
    private NextTranInfo hidden;
    private Alert alert;
    private Alert hiddenCheck;
    private Standard name;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of ReturnFrom.
    /// </summary>
    [JsonPropertyName("returnFrom")]
    public Alert ReturnFrom
    {
      get => returnFrom ??= new();
      set => returnFrom = value;
    }

    /// <summary>
    /// A value of PassTo.
    /// </summary>
    [JsonPropertyName("passTo")]
    public Alert PassTo
    {
      get => passTo ??= new();
      set => passTo = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    private Common error;
    private Alert returnFrom;
    private Alert passTo;
    private NextTranInfo nextTranInfo;
    private Common validCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AlertDistributionRule.
    /// </summary>
    [JsonPropertyName("alertDistributionRule")]
    public AlertDistributionRule AlertDistributionRule
    {
      get => alertDistributionRule ??= new();
      set => alertDistributionRule = value;
    }

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private AlertDistributionRule alertDistributionRule;
    private Alert alert;
  }
#endregion
}
