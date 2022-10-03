// Program: SP_CAB_UPDATE_TRIGGER, ID: 374350004, model: 746.
// Short name: SWE02521
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_TRIGGER.
/// </summary>
[Serializable]
public partial class SpCabUpdateTrigger: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_TRIGGER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateTrigger(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateTrigger.
  /// </summary>
  public SpCabUpdateTrigger(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.Trigger.LastUpdatedBy))
    {
      local.Trigger.LastUpdatedBy = import.Trigger.LastUpdatedBy ?? "";
    }
    else
    {
      local.Trigger.LastUpdatedBy = global.UserId;
    }

    if (Lt(local.Trigger.UpdatedTimestamp, import.Trigger.UpdatedTimestamp))
    {
      local.Trigger.UpdatedTimestamp = import.Trigger.UpdatedTimestamp;
    }
    else
    {
      local.Trigger.UpdatedTimestamp = Now();
    }

    local.Code.CodeName = "TRIGGER ACTION";

    if (!IsEmpty(import.Trigger.Action))
    {
      local.CodeValue.Cdvalue = import.Trigger.Action ?? Spaces(10);
    }
    else
    {
      local.CodeValue.Cdvalue = "BLANK";
    }

    UseCabValidateCodeValue2();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      ExitState = "SP0000_TRIGGER_PV";

      return;
    }

    local.Code.CodeName = "TRIGGER STATUS";

    if (!IsEmpty(import.Trigger.Status))
    {
      local.CodeValue.Cdvalue = import.Trigger.Status ?? Spaces(10);
    }
    else
    {
      local.CodeValue.Cdvalue = "BLANK";
    }

    UseCabValidateCodeValue2();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      ExitState = "SP0000_TRIGGER_PV";

      return;
    }

    if (!IsEmpty(import.Trigger.Type1))
    {
      local.Code.CodeName = "TRIGGER TYPE";
      local.CodeValue.Cdvalue = import.Trigger.Type1;
      UseCabValidateCodeValue2();
    }
    else
    {
      local.ValidCode.Flag = "N";
    }

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      ExitState = "SP0000_TRIGGER_PV";

      return;
    }

    local.Code.CodeName = "TRIGGER TYPE";
    local.CodeValue.Cdvalue = import.Trigger.Type1;
    local.CrossValidationCode.CodeName = "TRIGGER ACTION";

    if (!IsEmpty(import.Trigger.Action))
    {
      local.CrossValidationCodeValue.Cdvalue = import.Trigger.Action ?? Spaces
        (10);
    }
    else
    {
      local.CrossValidationCodeValue.Cdvalue = "BLANK";
    }

    UseCabValidateCodeValue1();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      ExitState = "SP0000_TRIGGER_PV";

      return;
    }

    local.Code.CodeName = "TRIGGER TYPE";
    local.CodeValue.Cdvalue = import.Trigger.Type1;
    local.CrossValidationCode.CodeName = "TRIGGER STATUS";

    if (!IsEmpty(import.Trigger.Status))
    {
      local.CrossValidationCodeValue.Cdvalue = import.Trigger.Status ?? Spaces
        (10);
    }
    else
    {
      local.CrossValidationCodeValue.Cdvalue = "BLANK";
    }

    UseCabValidateCodeValue1();

    if (AsChar(local.ValidCode.Flag) != 'Y')
    {
      ExitState = "SP0000_TRIGGER_PV";

      return;
    }

    if (ReadTrigger())
    {
      try
      {
        UpdateTrigger();
        MoveTrigger(entities.Trigger, export.Trigger);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_TRIGGER_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_TRIGGER_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "SP0000_TRIGGER_NF";
    }
  }

  private static void MoveTrigger(Trigger source, Trigger target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.Action = source.Action;
    target.Status = source.Status;
    target.DenormNumeric1 = source.DenormNumeric1;
    target.DenormNumeric2 = source.DenormNumeric2;
    target.DenormNumeric3 = source.DenormNumeric3;
    target.DenormText1 = source.DenormText1;
    target.DenormText2 = source.DenormText2;
    target.DenormText3 = source.DenormText3;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.UpdatedTimestamp = source.UpdatedTimestamp;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CrossValidationCode.CodeName = local.CrossValidationCode.CodeName;
    useImport.CrossValidationCodeValue.Cdvalue =
      local.CrossValidationCodeValue.Cdvalue;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private bool ReadTrigger()
  {
    entities.Trigger.Populated = false;

    return Read("ReadTrigger",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Trigger.Identifier);
      },
      (db, reader) =>
      {
        entities.Trigger.Identifier = db.GetInt32(reader, 0);
        entities.Trigger.Type1 = db.GetString(reader, 1);
        entities.Trigger.Action = db.GetNullableString(reader, 2);
        entities.Trigger.Status = db.GetNullableString(reader, 3);
        entities.Trigger.DenormNumeric1 = db.GetNullableInt32(reader, 4);
        entities.Trigger.DenormNumeric2 = db.GetNullableInt32(reader, 5);
        entities.Trigger.DenormNumeric3 = db.GetNullableInt32(reader, 6);
        entities.Trigger.DenormText1 = db.GetNullableString(reader, 7);
        entities.Trigger.DenormText2 = db.GetNullableString(reader, 8);
        entities.Trigger.DenormText3 = db.GetNullableString(reader, 9);
        entities.Trigger.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.Trigger.UpdatedTimestamp = db.GetNullableDateTime(reader, 11);
        entities.Trigger.Populated = true;
      });
  }

  private void UpdateTrigger()
  {
    var type1 = import.Trigger.Type1;
    var action1 = import.Trigger.Action ?? "";
    var status = import.Trigger.Status ?? "";
    var denormNumeric1 = import.Trigger.DenormNumeric1.GetValueOrDefault();
    var denormNumeric2 = import.Trigger.DenormNumeric2.GetValueOrDefault();
    var denormNumeric3 = import.Trigger.DenormNumeric3.GetValueOrDefault();
    var denormText1 = import.Trigger.DenormText1 ?? "";
    var denormText2 = import.Trigger.DenormText2 ?? "";
    var denormText3 = import.Trigger.DenormText3 ?? "";
    var lastUpdatedBy = local.Trigger.LastUpdatedBy ?? "";
    var updatedTimestamp = local.Trigger.UpdatedTimestamp;

    entities.Trigger.Populated = false;
    Update("UpdateTrigger",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "action0", action1);
        db.SetNullableString(command, "status", status);
        db.SetNullableInt32(command, "denormNumeric1", denormNumeric1);
        db.SetNullableInt32(command, "denormNumeric2", denormNumeric2);
        db.SetNullableInt32(command, "denormNumeric3", denormNumeric3);
        db.SetNullableString(command, "denormText1", denormText1);
        db.SetNullableString(command, "denormText2", denormText2);
        db.SetNullableString(command, "denormText3", denormText3);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "updatedTimestamp", updatedTimestamp);
        db.SetInt32(command, "identifier", entities.Trigger.Identifier);
      });

    entities.Trigger.Type1 = type1;
    entities.Trigger.Action = action1;
    entities.Trigger.Status = status;
    entities.Trigger.DenormNumeric1 = denormNumeric1;
    entities.Trigger.DenormNumeric2 = denormNumeric2;
    entities.Trigger.DenormNumeric3 = denormNumeric3;
    entities.Trigger.DenormText1 = denormText1;
    entities.Trigger.DenormText2 = denormText2;
    entities.Trigger.DenormText3 = denormText3;
    entities.Trigger.LastUpdatedBy = lastUpdatedBy;
    entities.Trigger.UpdatedTimestamp = updatedTimestamp;
    entities.Trigger.Populated = true;
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
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    private Trigger trigger;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    private Trigger trigger;
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
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of CrossValidationCode.
    /// </summary>
    [JsonPropertyName("crossValidationCode")]
    public Code CrossValidationCode
    {
      get => crossValidationCode ??= new();
      set => crossValidationCode = value;
    }

    /// <summary>
    /// A value of CrossValidationCodeValue.
    /// </summary>
    [JsonPropertyName("crossValidationCodeValue")]
    public CodeValue CrossValidationCodeValue
    {
      get => crossValidationCodeValue ??= new();
      set => crossValidationCodeValue = value;
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
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    private DateWorkArea current;
    private Common count;
    private Code crossValidationCode;
    private CodeValue crossValidationCodeValue;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Trigger trigger;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    private Trigger trigger;
  }
#endregion
}
