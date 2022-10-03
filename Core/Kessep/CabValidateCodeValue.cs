// Program: CAB_VALIDATE_CODE_VALUE, ID: 371423289, model: 746.
// Short name: SWE00103
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_VALIDATE_CODE_VALUE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block validates if the given code value is valid or not.
/// </para>
/// </summary>
[Serializable]
public partial class CabValidateCodeValue: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_VALIDATE_CODE_VALUE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabValidateCodeValue(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabValidateCodeValue.
  /// </summary>
  public CabValidateCodeValue(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // This action block is passed a code name and code value and optionally 
    // cross-validation code name and cross validation code value.
    // It checks if the code is a valid value in CODE_VALUE entity. If a cross 
    // validation code name and its value are specified it also checks if it is
    // a valid value in CODE_VALUE_COMBINATION entity.
    // This acyion block returns returns two parameters, a flag and a return 
    // code. If only Y/N value is required, then the calling action block needs
    // to check only export_valid_code ief supplied flag. If exact reason for
    // invalidity is required, the calling action block should use
    // export_return_code ief supplied count value.
    // EXPORT_VALID_CODE
    //  Y - input code value was valid.
    //  N - input code value was not valid.
    // EXPORT_RETURN_CODE
    //  0 - input code value was valid
    //  1 - invalid code name
    //  2 - invalid code value
    //  3 - invalid cross validation code name
    //  4 - invalid cross validation code value
    //  5 - invalid combination of code values.
    // ---------------------------------------------
    // A.Kinney	04/28/97	Changed Current_date
    // ------------------------------------------
    local.Current.Date = Now().Date;
    export.ValidCode.Flag = "N";
    export.ReturnCode.Count = 0;

    if (!ReadCode1())
    {
      export.ReturnCode.Count = 1;

      return;
    }

    if (ReadCodeValue1())
    {
      export.CodeValue.Assign(entities.ExistingCodeValue);
    }
    else
    {
      export.ReturnCode.Count = 2;

      return;
    }

    if (IsEmpty(import.CrossValidationCode.CodeName))
    {
      // ---------------------------------------------
      // No cross validation code has been specified. So no need to cross-
      // validate with another code value.
      // ---------------------------------------------
      export.ValidCode.Flag = "Y";

      return;
    }

    if (!ReadCode2())
    {
      export.ReturnCode.Count = 3;

      return;
    }

    if (!ReadCodeValue2())
    {
      export.ReturnCode.Count = 4;

      return;
    }

    if (!ReadCodeValueCombination())
    {
      export.ReturnCode.Count = 5;

      return;
    }

    // ---------------------------------------------
    // Both individual and cross validations are successful.
    // ---------------------------------------------
    export.ValidCode.Flag = "Y";
  }

  private bool ReadCode1()
  {
    entities.ExistingCode.Populated = false;

    return Read("ReadCode1",
      (db, command) =>
      {
        db.SetString(command, "codeName", import.Code.CodeName);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCode.Id = db.GetInt32(reader, 0);
        entities.ExistingCode.CodeName = db.GetString(reader, 1);
        entities.ExistingCode.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingCode.ExpirationDate = db.GetDate(reader, 3);
        entities.ExistingCode.DisplayTitle = db.GetString(reader, 4);
        entities.ExistingCode.Populated = true;
      });
  }

  private bool ReadCode2()
  {
    entities.ExistingCrossValidationCode.Populated = false;

    return Read("ReadCode2",
      (db, command) =>
      {
        db.SetString(command, "codeName", import.CrossValidationCode.CodeName);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCrossValidationCode.Id = db.GetInt32(reader, 0);
        entities.ExistingCrossValidationCode.CodeName = db.GetString(reader, 1);
        entities.ExistingCrossValidationCode.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingCrossValidationCode.ExpirationDate =
          db.GetDate(reader, 3);
        entities.ExistingCrossValidationCode.DisplayTitle =
          db.GetString(reader, 4);
        entities.ExistingCrossValidationCode.Populated = true;
      });
  }

  private bool ReadCodeValue1()
  {
    entities.ExistingCodeValue.Populated = false;

    return Read("ReadCodeValue1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingCode.Id);
        db.SetString(command, "cdvalue", import.CodeValue.Cdvalue);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingCodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.ExistingCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingCodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.ExistingCodeValue.Description = db.GetString(reader, 5);
        entities.ExistingCodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue2()
  {
    entities.ExistingCrossValidationCodeValue.Populated = false;

    return Read("ReadCodeValue2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "codId", entities.ExistingCrossValidationCode.Id);
        db.
          SetString(command, "cdvalue", import.CrossValidationCodeValue.Cdvalue);
          
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCrossValidationCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingCrossValidationCodeValue.CodId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingCrossValidationCodeValue.Cdvalue =
          db.GetString(reader, 2);
        entities.ExistingCrossValidationCodeValue.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingCrossValidationCodeValue.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingCrossValidationCodeValue.Description =
          db.GetString(reader, 5);
        entities.ExistingCrossValidationCodeValue.Populated = true;
      });
  }

  private bool ReadCodeValueCombination()
  {
    entities.ExistingCrossValidationCodeValueCombination.Populated = false;

    return Read("ReadCodeValueCombination",
      (db, command) =>
      {
        db.SetInt32(command, "covId", entities.ExistingCodeValue.Id);
        db.SetInt32(
          command, "covSId", entities.ExistingCrossValidationCodeValue.Id);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCrossValidationCodeValueCombination.Id =
          db.GetInt32(reader, 0);
        entities.ExistingCrossValidationCodeValueCombination.CovId =
          db.GetInt32(reader, 1);
        entities.ExistingCrossValidationCodeValueCombination.CovSId =
          db.GetInt32(reader, 2);
        entities.ExistingCrossValidationCodeValueCombination.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingCrossValidationCodeValueCombination.ExpirationDate =
          db.GetDate(reader, 4);
        entities.ExistingCrossValidationCodeValueCombination.Populated = true;
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
    /// A value of CrossValidationCodeValue.
    /// </summary>
    [JsonPropertyName("crossValidationCodeValue")]
    public CodeValue CrossValidationCodeValue
    {
      get => crossValidationCodeValue ??= new();
      set => crossValidationCodeValue = value;
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

    private CodeValue crossValidationCodeValue;
    private Code crossValidationCode;
    private CodeValue codeValue;
    private Code code;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    private CodeValue codeValue;
    private Common validCode;
    private Common returnCode;
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
    /// A value of ExistingCrossValidationCodeValueCombination.
    /// </summary>
    [JsonPropertyName("existingCrossValidationCodeValueCombination")]
    public CodeValueCombination ExistingCrossValidationCodeValueCombination
    {
      get => existingCrossValidationCodeValueCombination ??= new();
      set => existingCrossValidationCodeValueCombination = value;
    }

    /// <summary>
    /// A value of ExistingCrossValidationCodeValue.
    /// </summary>
    [JsonPropertyName("existingCrossValidationCodeValue")]
    public CodeValue ExistingCrossValidationCodeValue
    {
      get => existingCrossValidationCodeValue ??= new();
      set => existingCrossValidationCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingCrossValidationCode.
    /// </summary>
    [JsonPropertyName("existingCrossValidationCode")]
    public Code ExistingCrossValidationCode
    {
      get => existingCrossValidationCode ??= new();
      set => existingCrossValidationCode = value;
    }

    /// <summary>
    /// A value of ExistingCodeValue.
    /// </summary>
    [JsonPropertyName("existingCodeValue")]
    public CodeValue ExistingCodeValue
    {
      get => existingCodeValue ??= new();
      set => existingCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingCode.
    /// </summary>
    [JsonPropertyName("existingCode")]
    public Code ExistingCode
    {
      get => existingCode ??= new();
      set => existingCode = value;
    }

    private CodeValueCombination existingCrossValidationCodeValueCombination;
    private CodeValue existingCrossValidationCodeValue;
    private Code existingCrossValidationCode;
    private CodeValue existingCodeValue;
    private Code existingCode;
  }
#endregion
}
