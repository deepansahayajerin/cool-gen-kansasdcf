// Program: OE_HIAV_AVAILABLE_INS_DELETE, ID: 371857739, model: 746.
// Short name: SWE02300
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_HIAV_AVAILABLE_INS_DELETE.
/// </summary>
[Serializable]
public partial class OeHiavAvailableInsDelete: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HIAV_AVAILABLE_INS_DELETE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHiavAvailableInsDelete(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHiavAvailableInsDelete.
  /// </summary>
  public OeHiavAvailableInsDelete(IContext context, Import import, Export export)
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE 		DESCRIPTION
    // C. Chhun	01/05/99	Initial Code
    // ******** END MAINTENANCE LOG ****************
    if (ReadHealthInsuranceAvailability())
    {
      DeleteHealthInsuranceAvailability();
    }
    else
    {
      ExitState = "OE0189_AVAILABLE_HEALTH_INS_NF";
    }
  }

  private void DeleteHealthInsuranceAvailability()
  {
    Update("DeleteHealthInsuranceAvailability",
      (db, command) =>
      {
        db.SetInt32(
          command, "insuranceId",
          entities.HealthInsuranceAvailability.InsuranceId);
        db.SetString(
          command, "cspNumber", entities.HealthInsuranceAvailability.CspNumber);
          
      });
  }

  private bool ReadHealthInsuranceAvailability()
  {
    entities.HealthInsuranceAvailability.Populated = false;

    return Read("ReadHealthInsuranceAvailability",
      (db, command) =>
      {
        db.SetInt32(
          command, "insuranceId",
          import.HealthInsuranceAvailability.InsuranceId);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceAvailability.InsuranceId =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceAvailability.CspNumber =
          db.GetString(reader, 1);
        entities.HealthInsuranceAvailability.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
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

    private HealthInsuranceAvailability healthInsuranceAvailability;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of HealthInsuranceAvailability.
    /// </summary>
    [JsonPropertyName("healthInsuranceAvailability")]
    public HealthInsuranceAvailability HealthInsuranceAvailability
    {
      get => healthInsuranceAvailability ??= new();
      set => healthInsuranceAvailability = value;
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

    private HealthInsuranceAvailability healthInsuranceAvailability;
    private CsePerson csePerson;
  }
#endregion
}
