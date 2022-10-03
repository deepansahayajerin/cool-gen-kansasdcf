// Program: CAB_GET_CODE_VALUE_DESCRIPTION, ID: 371454790, model: 746.
// Short name: SWE00055
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_GET_CODE_VALUE_DESCRIPTION.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block validates if the given code value is valid or not.
/// </para>
/// </summary>
[Serializable]
public partial class CabGetCodeValueDescription: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_GET_CODE_VALUE_DESCRIPTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabGetCodeValueDescription(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabGetCodeValueDescription.
  /// </summary>
  public CabGetCodeValueDescription(IContext context, Import import,
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
    // ---------------------------------------------
    // This action block is passed a {code ID or code name} and code value.
    // It reads and returns the code_value description.
    // The following values are returned:
    // export error in decoding ief supplied flag
    //  N - Operation successful
    //  Y - An error was encountered. Description not returned.
    // export_return_code ief_supplied count
    //  0 - operation was successful.
    //  1 - Neither CODE.ID nor CODE.CODE_NAME supplied.
    //  2 - CODE not found for the given ID
    //  3 - CODE not found for the given name
    //  4 - CODE_VALUE not found for the given value.
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    export.ErrorInDecoding.Flag = "Y";
    export.ReturnCode.Count = 0;

    if (import.Code.Id != 0)
    {
      if (!ReadCode2())
      {
        export.ReturnCode.Count = 2;

        return;
      }
    }
    else if (!IsEmpty(import.Code.CodeName))
    {
      if (!ReadCode1())
      {
        export.ReturnCode.Count = 3;

        return;
      }
    }
    else
    {
      // ---------------------------------------------
      // Neither CODE.ID nor CODE.CODE_NAME supplied.
      // ---------------------------------------------
      export.ReturnCode.Count = 1;

      return;
    }

    if (ReadCodeValue())
    {
      export.CodeValue.Assign(entities.ExistingCodeValue);
    }
    else
    {
      export.ReturnCode.Count = 4;

      return;
    }

    export.ErrorInDecoding.Flag = "N";
  }

  private bool ReadCode1()
  {
    entities.ExistingCode.Populated = false;

    return Read("ReadCode1",
      (db, command) =>
      {
        db.SetString(command, "codeName", import.Code.CodeName);
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
    entities.ExistingCode.Populated = false;

    return Read("ReadCode2",
      (db, command) =>
      {
        db.SetInt32(command, "codId", import.Code.Id);
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

  private bool ReadCodeValue()
  {
    entities.ExistingCodeValue.Populated = false;

    return Read("ReadCodeValue",
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
    /// A value of ErrorInDecoding.
    /// </summary>
    [JsonPropertyName("errorInDecoding")]
    public Common ErrorInDecoding
    {
      get => errorInDecoding ??= new();
      set => errorInDecoding = value;
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
    private Common errorInDecoding;
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

    private CodeValue existingCodeValue;
    private Code existingCode;
  }
#endregion
}
