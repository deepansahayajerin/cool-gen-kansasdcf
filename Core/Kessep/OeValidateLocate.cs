// Program: OE_VALIDATE_LOCATE, ID: 374437706, model: 746.
// Short name: SWE02613
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_VALIDATE_LOCATE.
/// </summary>
[Serializable]
public partial class OeValidateLocate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_VALIDATE_LOCATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeValidateLocate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeValidateLocate.
  /// </summary>
  public OeValidateLocate(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.Agency.Cdvalue = Substring(import.LocateRequest.AgencyNumber, 2, 4);

    if (!ReadCodeCodeValue())
    {
      ExitState = "CODE_VALUE_NF";
    }
  }

  private bool ReadCodeCodeValue()
  {
    entities.Code.Populated = false;
    entities.CodeValue.Populated = false;

    return Read("ReadCodeCodeValue",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cdvalue", local.Agency.Cdvalue);
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.CodeValue.Id = db.GetInt32(reader, 4);
        entities.CodeValue.Cdvalue = db.GetString(reader, 5);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 6);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 7);
        entities.CodeValue.Description = db.GetString(reader, 8);
        entities.Code.Populated = true;
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.LocateRequest.CsePersonNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    private LocateRequest locateRequest;
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
    /// A value of Agency.
    /// </summary>
    [JsonPropertyName("agency")]
    public CodeValue Agency
    {
      get => agency ??= new();
      set => agency = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Infrastructure Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private CodeValue agency;
    private Infrastructure pass;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    private CsePerson csePerson;
    private Code code;
    private CodeValue codeValue;
  }
#endregion
}
