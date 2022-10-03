// Program: LE_GET_ACTION_TAKEN_DESCRIPTION, ID: 373338441, model: 746.
// Short name: SWE02082
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_GET_ACTION_TAKEN_DESCRIPTION.
/// </summary>
[Serializable]
public partial class LeGetActionTakenDescription: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_GET_ACTION_TAKEN_DESCRIPTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeGetActionTakenDescription(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeGetActionTakenDescription.
  /// </summary>
  public LeGetActionTakenDescription(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------------------------------------------------------------------------------------------------------------
    // 04/02/02  GVandy	PR 138221	Initial Code
    // --------------------------------------------------------------------------------------------------------------
    export.ValidActionTaken.Flag = "N";
    local.CodeValue.Cdvalue = import.LegalAction.ActionTaken;

    if (!ReadCode())
    {
      return;
    }

    if (ReadCodeValue1())
    {
      export.CodeValue.Description = entities.CodeValue.Description;
      export.ValidActionTaken.Flag = "Y";
    }
    else if (ReadCodeValue2())
    {
      export.CodeValue.Description = entities.CodeValue.Description;
      export.ValidActionTaken.Flag = "Y";
    }
  }

  private bool ReadCode()
  {
    entities.Code.Populated = false;

    return Read("ReadCode",
      null,
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.Populated = true;
      });
  }

  private bool ReadCodeValue1()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.Code.Id);
        db.SetString(command, "cdvalue", local.CodeValue.Cdvalue);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue2()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.Code.Id);
        db.SetString(command, "cdvalue", local.CodeValue.Cdvalue);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
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
    /// A value of ValidActionTaken.
    /// </summary>
    [JsonPropertyName("validActionTaken")]
    public Common ValidActionTaken
    {
      get => validActionTaken ??= new();
      set => validActionTaken = value;
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

    private Common validActionTaken;
    private CodeValue codeValue;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    private DateWorkArea current;
    private CodeValue codeValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
