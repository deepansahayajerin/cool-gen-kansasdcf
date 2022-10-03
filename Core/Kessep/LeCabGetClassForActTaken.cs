// Program: LE_CAB_GET_CLASS_FOR_ACT_TAKEN, ID: 371985176, model: 746.
// Short name: SWE01714
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CAB_GET_CLASS_FOR_ACT_TAKEN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block returns the Legal Action Classification for the 
/// specified Legal Action Action Taken.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabGetClassForActTaken: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_GET_CLASS_FOR_ACT_TAKEN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabGetClassForActTaken(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabGetClassForActTaken.
  /// </summary>
  public LeCabGetClassForActTaken(IContext context, Import import, Export export)
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
    local.Current.Date = Now().Date;

    if (!ReadCode2())
    {
      ExitState = "LE0000_CODE_FOR_LACT_CLASS_NF";

      return;
    }

    if (!ReadCode1())
    {
      ExitState = "LE0000_CODE_FOR_ACT_TAKEN_NF";

      return;
    }

    if (!ReadCodeValue())
    {
      ExitState = "LE0000_INV_LACT_ACT_TAKEN";

      return;
    }

    if (ReadCodeValueCombinationCodeValue())
    {
      export.LegalAction.Classification =
        entities.ExistingLactClassificationCodeValue.Cdvalue;
    }
  }

  private bool ReadCode1()
  {
    entities.ExistingLactActionTakenCode.Populated = false;

    return Read("ReadCode1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLactActionTakenCode.Id = db.GetInt32(reader, 0);
        entities.ExistingLactActionTakenCode.CodeName = db.GetString(reader, 1);
        entities.ExistingLactActionTakenCode.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingLactActionTakenCode.ExpirationDate =
          db.GetDate(reader, 3);
        entities.ExistingLactActionTakenCode.Populated = true;
      });
  }

  private bool ReadCode2()
  {
    entities.ExistingLactClassificationCode.Populated = false;

    return Read("ReadCode2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLactClassificationCode.Id = db.GetInt32(reader, 0);
        entities.ExistingLactClassificationCode.CodeName =
          db.GetString(reader, 1);
        entities.ExistingLactClassificationCode.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingLactClassificationCode.ExpirationDate =
          db.GetDate(reader, 3);
        entities.ExistingLactClassificationCode.Populated = true;
      });
  }

  private bool ReadCodeValue()
  {
    entities.ExistingLactActionTakenCodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "codId", entities.ExistingLactActionTakenCode.Id);
        db.SetString(command, "actionTaken", import.LegalAction.ActionTaken);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLactActionTakenCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingLactActionTakenCodeValue.CodId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingLactActionTakenCodeValue.Cdvalue =
          db.GetString(reader, 2);
        entities.ExistingLactActionTakenCodeValue.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLactActionTakenCodeValue.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingLactActionTakenCodeValue.Populated = true;
      });
  }

  private bool ReadCodeValueCombinationCodeValue()
  {
    entities.Existing.Populated = false;
    entities.ExistingLactClassificationCodeValue.Populated = false;

    return Read("ReadCodeValueCombinationCodeValue",
      (db, command) =>
      {
        db.SetInt32(
          command, "covId", entities.ExistingLactActionTakenCodeValue.Id);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Existing.Id = db.GetInt32(reader, 0);
        entities.Existing.CovId = db.GetInt32(reader, 1);
        entities.Existing.CovSId = db.GetInt32(reader, 2);
        entities.ExistingLactClassificationCodeValue.Id =
          db.GetInt32(reader, 2);
        entities.Existing.EffectiveDate = db.GetDate(reader, 3);
        entities.Existing.ExpirationDate = db.GetDate(reader, 4);
        entities.ExistingLactClassificationCodeValue.Cdvalue =
          db.GetString(reader, 5);
        entities.ExistingLactClassificationCodeValue.EffectiveDate =
          db.GetDate(reader, 6);
        entities.ExistingLactClassificationCodeValue.ExpirationDate =
          db.GetDate(reader, 7);
        entities.Existing.Populated = true;
        entities.ExistingLactClassificationCodeValue.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
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

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CodeValueCombination Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingLactClassificationCodeValue.
    /// </summary>
    [JsonPropertyName("existingLactClassificationCodeValue")]
    public CodeValue ExistingLactClassificationCodeValue
    {
      get => existingLactClassificationCodeValue ??= new();
      set => existingLactClassificationCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingLactClassificationCode.
    /// </summary>
    [JsonPropertyName("existingLactClassificationCode")]
    public Code ExistingLactClassificationCode
    {
      get => existingLactClassificationCode ??= new();
      set => existingLactClassificationCode = value;
    }

    /// <summary>
    /// A value of ExistingLactActionTakenCodeValue.
    /// </summary>
    [JsonPropertyName("existingLactActionTakenCodeValue")]
    public CodeValue ExistingLactActionTakenCodeValue
    {
      get => existingLactActionTakenCodeValue ??= new();
      set => existingLactActionTakenCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingLactActionTakenCode.
    /// </summary>
    [JsonPropertyName("existingLactActionTakenCode")]
    public Code ExistingLactActionTakenCode
    {
      get => existingLactActionTakenCode ??= new();
      set => existingLactActionTakenCode = value;
    }

    private CodeValueCombination existing;
    private CodeValue existingLactClassificationCodeValue;
    private Code existingLactClassificationCode;
    private CodeValue existingLactActionTakenCodeValue;
    private Code existingLactActionTakenCode;
  }
#endregion
}
