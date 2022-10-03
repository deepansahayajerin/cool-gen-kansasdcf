// Program: SP_CREATE_OUTGOING_EXT_ALERT, ID: 371733005, model: 746.
// Short name: SWE01853
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CREATE_OUTGOING_EXT_ALERT.
/// </para>
/// <para>
/// Place this AB in any PrAD that is capable of producing a condition for which
/// an EXTERNAL ALERT is needed.
/// This AB should be placed at the end of the main CASE UPDATE and main CASE 
/// ADD, and enclosed in an IF statement.
/// The IF statement is true when the condition for creating an EXTERNAL ALERT 
/// exists.
/// </para>
/// </summary>
[Serializable]
public partial class SpCreateOutgoingExtAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_OUTGOING_EXT_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateOutgoingExtAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateOutgoingExtAlert.
  /// </summary>
  public SpCreateOutgoingExtAlert(IContext context, Import import, Export export)
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
    // mjr
    // -----------------------------------------------------------------
    //  SET STANDARD INTERFACE_ALERT ATTRIBUTES
    //  01-26-98	RGREY	H00034011 add exit state msgs
    // --------------------------------------------------------------------
    local.InterfaceAlert.CreatedTimestamp = Now();
    local.InterfaceAlert.CreatedBy = global.UserId;
    local.InterfaceAlert.ProcessStatus = "C";
    local.InterfaceAlert.SendingSystem = "CSE";
    local.InterfaceAlert.NoteText = import.InterfaceAlert.NoteText ?? "";
    local.InterfaceAlert.CsePersonNumber =
      import.InterfaceAlert.CsePersonNumber ?? "";
    local.CreateOk.Flag = "N";

    // mjr
    // -----------------------------------------------------------------
    //  CREATE NUMERIC VALUE FOR ALERT CONTROL_NUMBER
    // --------------------------------------------------------------------
    local.TempInterfaceAlertCode.Text5 = "000" + (
      import.InterfaceAlert.AlertCode ?? "");
    local.Temp.ControlNumber =
      (int)StringToNumber(local.TempInterfaceAlertCode.Text5);

    // mjr
    // -----------------------------------------------------------------
    //  DECIDE WHICH SYSTEM RECEIVES THIS ALERT
    // --------------------------------------------------------------------
    if (local.Temp.ControlNumber >= 40 && local.Temp.ControlNumber <= 59)
    {
      local.InterfaceAlert.ReceivingSystem = "KAE";
    }
    else if (local.Temp.ControlNumber >= 60 && local.Temp.ControlNumber <= 79)
    {
      local.InterfaceAlert.ReceivingSystem = "KSC";
    }
    else
    {
    }

    // mjr
    // -----------------------------------------------------------------
    //  READ ALERT FOR ALERT_CODE AND ALERT_NAME
    // --------------------------------------------------------------------
    if (ReadAlert())
    {
      local.InterfaceAlert.AlertCode = import.InterfaceAlert.AlertCode ?? "";
      local.InterfaceAlert.AlertName = entities.Alert.Name;
    }
    else
    {
      ExitState = "SP0000_ALERT_NF";

      return;
    }

    // mjr
    // -----------------------------------------------------------------
    //  CREATE NEW INTERFACE_ALERT
    // --------------------------------------------------------------------
    do
    {
      // mjr
      // -----------------------------------------------------------------
      //  GET UNIQUE TIMESTAMP FOR INTERFACE_ALERT IDENTIFIER
      // --------------------------------------------------------------------
      try
      {
        CreateInterfaceAlert();
        local.CreateOk.Flag = "Y";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.CreateOk.Flag = "N";

            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(AsChar(local.CreateOk.Flag) != 'Y');

    // mjr
    // -----------------------------------------------------------------
    //  SOME INTERFACE_ALERTS ARE DELIVERED TO BOTH SYSTEMS
    // --------------------------------------------------------------------
    if (local.Temp.ControlNumber == 40 || local.Temp.ControlNumber == 41 || local
      .Temp.ControlNumber == 45 || local.Temp.ControlNumber == 46)
    {
      if (IsEmpty(import.KscParticipation.Flag))
      {
        return;
      }

      local.CreateOk.Flag = "N";
      local.InterfaceAlert.ReceivingSystem = "KSC";

      switch(local.Temp.ControlNumber)
      {
        case 40:
          local.InterfaceAlert.AlertCode = "60";

          break;
        case 41:
          local.InterfaceAlert.AlertCode = "61";

          break;
        case 45:
          local.InterfaceAlert.AlertCode = "65";

          break;
        case 46:
          return;

          local.InterfaceAlert.AlertCode = "66";

          break;
        default:
          break;
      }

      do
      {
        try
        {
          CreateInterfaceAlert();
          local.CreateOk.Flag = "Y";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.CreateOk.Flag = "N";

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      while(AsChar(local.CreateOk.Flag) != 'Y');
    }
  }

  private string UseSpSetIntrfcAlertIdentifier()
  {
    var useImport = new SpSetIntrfcAlertIdentifier.Import();
    var useExport = new SpSetIntrfcAlertIdentifier.Export();

    Call(SpSetIntrfcAlertIdentifier.Execute, useImport, useExport);

    return useExport.InterfaceAlert.Identifier;
  }

  private void CreateInterfaceAlert()
  {
    var identifier = UseSpSetIntrfcAlertIdentifier();
    var csePersonNumber = local.InterfaceAlert.CsePersonNumber ?? "";
    var alertCode = local.InterfaceAlert.AlertCode ?? "";
    var alertName = local.InterfaceAlert.AlertName ?? "";
    var sendingSystem = local.InterfaceAlert.SendingSystem ?? "";
    var receivingSystem = local.InterfaceAlert.ReceivingSystem ?? "";
    var processStatus = local.InterfaceAlert.ProcessStatus ?? "";
    var createdBy = local.InterfaceAlert.CreatedBy ?? "";
    var createdTimestamp = local.InterfaceAlert.CreatedTimestamp;
    var noteText = local.InterfaceAlert.NoteText ?? "";

    entities.InterfaceAlert.Populated = false;
    Update("CreateInterfaceAlert",
      (db, command) =>
      {
        db.SetString(command, "identifier", identifier);
        db.SetNullableString(command, "cspNumber", csePersonNumber);
        db.SetNullableString(command, "alertCode", alertCode);
        db.SetNullableString(command, "alertName", alertName);
        db.SetNullableString(command, "sendingSystem", sendingSystem);
        db.SetNullableString(command, "receivingSystem", receivingSystem);
        db.SetNullableString(command, "processStatus", processStatus);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetNullableString(command, "noteText", noteText);
      });

    entities.InterfaceAlert.Identifier = identifier;
    entities.InterfaceAlert.CsePersonNumber = csePersonNumber;
    entities.InterfaceAlert.AlertCode = alertCode;
    entities.InterfaceAlert.AlertName = alertName;
    entities.InterfaceAlert.SendingSystem = sendingSystem;
    entities.InterfaceAlert.ReceivingSystem = receivingSystem;
    entities.InterfaceAlert.ProcessStatus = processStatus;
    entities.InterfaceAlert.CreatedBy = createdBy;
    entities.InterfaceAlert.CreatedTimestamp = createdTimestamp;
    entities.InterfaceAlert.LastUpdatedTmstamp = null;
    entities.InterfaceAlert.NoteText = noteText;
    entities.InterfaceAlert.Populated = true;
  }

  private bool ReadAlert()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", local.Temp.ControlNumber);
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
    /// A value of KscParticipation.
    /// </summary>
    [JsonPropertyName("kscParticipation")]
    public Common KscParticipation
    {
      get => kscParticipation ??= new();
      set => kscParticipation = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private Common kscParticipation;
    private InterfaceAlert interfaceAlert;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CreateOk.
    /// </summary>
    [JsonPropertyName("createOk")]
    public Common CreateOk
    {
      get => createOk ??= new();
      set => createOk = value;
    }

    /// <summary>
    /// A value of TempInterfaceAlertCode.
    /// </summary>
    [JsonPropertyName("tempInterfaceAlertCode")]
    public WorkArea TempInterfaceAlertCode
    {
      get => tempInterfaceAlertCode ??= new();
      set => tempInterfaceAlertCode = value;
    }

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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Alert Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private Common createOk;
    private WorkArea tempInterfaceAlertCode;
    private Common common;
    private Alert temp;
    private InterfaceAlert interfaceAlert;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ZdelOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("zdelOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert ZdelOfficeServiceProviderAlert
    {
      get => zdelOfficeServiceProviderAlert ??= new();
      set => zdelOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of ZdelAlertDistributionRule.
    /// </summary>
    [JsonPropertyName("zdelAlertDistributionRule")]
    public AlertDistributionRule ZdelAlertDistributionRule
    {
      get => zdelAlertDistributionRule ??= new();
      set => zdelAlertDistributionRule = value;
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
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private OfficeServiceProviderAlert zdelOfficeServiceProviderAlert;
    private AlertDistributionRule zdelAlertDistributionRule;
    private Alert alert;
    private InterfaceAlert interfaceAlert;
  }
#endregion
}
