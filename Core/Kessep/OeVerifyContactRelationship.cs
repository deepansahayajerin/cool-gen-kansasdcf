// Program: OE_VERIFY_CONTACT_RELATIONSHIP, ID: 371181048, model: 746.
// Short name: SWE02005
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_VERIFY_CONTACT_RELATIONSHIP.
/// </summary>
[Serializable]
public partial class OeVerifyContactRelationship: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VERIFY_CONTACT_RELATIONSHIP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeVerifyContactRelationship(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeVerifyContactRelationship.
  /// </summary>
  public OeVerifyContactRelationship(IContext context, Import import,
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
    local.Code.CodeName = "EDS RELATIONSHIPS";
    local.CodeValue.Cdvalue = import.Contact.RelationshipToCsePerson ?? Spaces
      (10);

    if (ReadCodeValue())
    {
      export.CodeValueValid.Flag = "Y";
    }
    else
    {
      export.CodeValueValid.Flag = "N";
    }
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cdvalue", local.CodeValue.Cdvalue);
        db.SetString(command, "codeName", local.Code.CodeName);
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
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
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    private Contact contact;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CodeValueValid.
    /// </summary>
    [JsonPropertyName("codeValueValid")]
    public Common CodeValueValid
    {
      get => codeValueValid ??= new();
      set => codeValueValid = value;
    }

    private Common codeValueValid;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    private Code code;
    private CodeValue codeValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private Code code;
    private CodeValue codeValue;
  }
#endregion
}
