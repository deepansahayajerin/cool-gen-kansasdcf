// Program: CAB_VALIDATE_STATE_COUNTY_CODES, ID: 372582885, model: 746.
// Short name: SWE00106
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_VALIDATE_STATE_COUNTY_CODES.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block validates the state and the county code by looking 
/// up FIPS entity.
/// </para>
/// </summary>
[Serializable]
public partial class CabValidateStateCountyCodes: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_VALIDATE_STATE_COUNTY_CODES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabValidateStateCountyCodes(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabValidateStateCountyCodes.
  /// </summary>
  public CabValidateStateCountyCodes(IContext context, Import import,
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
    if (ReadFips())
    {
      export.Fips.Assign(entities.Existing);
      export.ValidFipsStateCounty.Flag = "Y";
    }
    else
    {
      export.ValidFipsStateCounty.Flag = "N";
    }
  }

  private bool ReadFips()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", import.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", import.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.StateDescription = db.GetNullableString(reader, 3);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 4);
        entities.Existing.LocationDescription = db.GetNullableString(reader, 5);
        entities.Existing.StateAbbreviation = db.GetString(reader, 6);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 7);
        entities.Existing.Populated = true;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Fips fips;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ValidFipsStateCounty.
    /// </summary>
    [JsonPropertyName("validFipsStateCounty")]
    public Common ValidFipsStateCounty
    {
      get => validFipsStateCounty ??= new();
      set => validFipsStateCounty = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Common validFipsStateCounty;
    private Fips fips;
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
    public Fips Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Fips existing;
  }
#endregion
}
