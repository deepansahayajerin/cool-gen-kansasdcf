// Program: OE_CHECK_OPEN_MILITARY_SERVICE, ID: 371920203, model: 746.
// Short name: SWE00884
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CHECK_OPEN_MILITARY_SERVICE.
/// </para>
/// <para>
/// RESP: OBLGESTB.
/// This Common Action Block(CAB) checks for an Open Military Service Record. An
/// open Military Service Record is one without an End date.
/// </para>
/// </summary>
[Serializable]
public partial class OeCheckOpenMilitaryService: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CHECK_OPEN_MILITARY_SERVICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCheckOpenMilitaryService(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCheckOpenMilitaryService.
  /// </summary>
  public OeCheckOpenMilitaryService(IContext context, Import import,
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
    // This CAB checks for an OPEN military service record (i.e. - one without 
    // an end date).
    ExitState = "ACO_NN0000_ALL_OK";
    local.MilitaryService.EndDate = null;

    if (ReadMilitaryService())
    {
      ExitState = "OPEN_MILITARY_SERVICE_EXISTS";
    }
  }

  private bool ReadMilitaryService()
  {
    entities.MilitaryService.Populated = false;

    return Read("ReadMilitaryService",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "endDate",
          local.MilitaryService.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.MilitaryService.CspNumber = db.GetString(reader, 1);
        entities.MilitaryService.StartDate = db.GetNullableDate(reader, 2);
        entities.MilitaryService.EndDate = db.GetNullableDate(reader, 3);
        entities.MilitaryService.Populated = true;
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
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
    }

    private MilitaryService militaryService;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
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

    private MilitaryService militaryService;
    private CsePerson csePerson;
  }
#endregion
}
