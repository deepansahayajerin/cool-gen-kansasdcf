// Program: SP_CAB_CREATE_TRIGGER, ID: 374350003, model: 746.
// Short name: SWE02520
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_TRIGGER.
/// </summary>
[Serializable]
public partial class SpCabCreateTrigger: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_TRIGGER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateTrigger(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateTrigger.
  /// </summary>
  public SpCabCreateTrigger(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.Trigger.CreatedBy))
    {
      local.Trigger.CreatedBy = import.Trigger.CreatedBy ?? "";
    }
    else
    {
      local.Trigger.CreatedBy = global.UserId;
    }

    if (Lt(local.Trigger.CreatedTimestamp, import.Trigger.CreatedTimestamp))
    {
      local.Trigger.CreatedTimestamp = import.Trigger.CreatedTimestamp;
    }
    else
    {
      local.Trigger.CreatedTimestamp = Now();
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

    local.Count.Count = 0;
    local.Trigger.Identifier =
      (int)(Microsecond(local.Trigger.CreatedTimestamp) + Day
      (local.Trigger.CreatedTimestamp) * (long)1000000);

    do
    {
      local.Trigger.Identifier += local.Count.Count;

      try
      {
        CreateTrigger();
        MoveTrigger(entities.Trigger, export.Trigger);

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ++local.Count.Count;

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_TRIGGER_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(local.Count.Count <= 100);

    ExitState = "SP0000_TRIGGER_AE";
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
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
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

  private void CreateTrigger()
  {
    var identifier = local.Trigger.Identifier;
    var type1 = import.Trigger.Type1;
    var action1 = import.Trigger.Action ?? "";
    var status = import.Trigger.Status ?? "";
    var denormNumeric1 = import.Trigger.DenormNumeric1.GetValueOrDefault();
    var denormNumeric2 = import.Trigger.DenormNumeric2.GetValueOrDefault();
    var denormNumeric3 = import.Trigger.DenormNumeric3.GetValueOrDefault();
    var denormText1 = import.Trigger.DenormText1 ?? "";
    var denormText2 = import.Trigger.DenormText2 ?? "";
    var denormText3 = import.Trigger.DenormText3 ?? "";
    var createdBy = local.Trigger.CreatedBy ?? "";
    var createdTimestamp = local.Trigger.CreatedTimestamp;

    entities.Trigger.Populated = false;
    Update("CreateTrigger",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "action0", action1);
        db.SetNullableString(command, "status", status);
        db.SetNullableInt32(command, "denormNumeric1", denormNumeric1);
        db.SetNullableInt32(command, "denormNumeric2", denormNumeric2);
        db.SetNullableInt32(command, "denormNumeric3", denormNumeric3);
        db.SetNullableString(command, "denormText1", denormText1);
        db.SetNullableString(command, "denormText2", denormText2);
        db.SetNullableString(command, "denormText3", denormText3);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "updatedTimestamp", default(DateTime));
      });

    entities.Trigger.Identifier = identifier;
    entities.Trigger.Type1 = type1;
    entities.Trigger.Action = action1;
    entities.Trigger.Status = status;
    entities.Trigger.DenormNumeric1 = denormNumeric1;
    entities.Trigger.DenormNumeric2 = denormNumeric2;
    entities.Trigger.DenormNumeric3 = denormNumeric3;
    entities.Trigger.DenormText1 = denormText1;
    entities.Trigger.DenormText2 = denormText2;
    entities.Trigger.DenormText3 = denormText3;
    entities.Trigger.CreatedBy = createdBy;
    entities.Trigger.CreatedTimestamp = createdTimestamp;
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
