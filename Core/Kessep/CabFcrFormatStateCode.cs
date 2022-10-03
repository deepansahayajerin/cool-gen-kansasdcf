// Program: CAB_FCR_FORMAT_STATE_CODE, ID: 371067402, model: 746.
// Short name: SWE01288
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FCR_FORMAT_STATE_CODE.
/// </summary>
[Serializable]
public partial class CabFcrFormatStateCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FCR_FORMAT_STATE_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFcrFormatStateCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFcrFormatStateCode.
  /// </summary>
  public CabFcrFormatStateCode(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    MoveCsePerson(import.CsePerson, export.CsePerson);
    local.Code.Id = 169;

    if (!IsEmpty(export.CsePerson.BirthPlaceState) && !
      Equal(export.CsePerson.BirthPlaceState, "NK"))
    {
      if (!ReadCodeValue())
      {
        export.CsePerson.BirthPlaceState = "";
      }
    }
    else
    {
      export.CsePerson.BirthPlaceState = "";
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.BirthPlaceState = source.BirthPlaceState;
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", local.Code.Id);
        db.SetDate(
          command, "expirationDate", import.Process.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "birthPlaceState", export.CsePerson.BirthPlaceState ?? "");
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 3);
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private DateWorkArea process;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private CsePerson csePerson;
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

    private Code code;
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
