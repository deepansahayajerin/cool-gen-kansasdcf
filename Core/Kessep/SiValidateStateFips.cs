// Program: SI_VALIDATE_STATE_FIPS, ID: 372393246, model: 746.
// Short name: SWE01264
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_VALIDATE_STATE_FIPS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB will check the State Abbv. and return the State FIPS for a valid 
/// code.
/// </para>
/// </summary>
[Serializable]
public partial class SiValidateStateFips: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_VALIDATE_STATE_FIPS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiValidateStateFips(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiValidateStateFips.
  /// </summary>
  public SiValidateStateFips(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.Common.State))
    {
      if (ReadFips2())
      {
        export.Fips.Assign(entities.Fips);
      }
      else
      {
        export.Error.Flag = "Y";
        ExitState = "ACO_NE0000_INVALID_STATE_CODE";
      }
    }
    else if (ReadFips1())
    {
      export.Fips.Assign(entities.Fips);
    }
    else
    {
      export.Error.Flag = "Y";
      ExitState = "FIPS_NF";
    }
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.State.State);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetString(command, "stateAbbreviation", import.Common.State);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
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
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Fips State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Fips state;
    private Common common;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
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

    private Common error;
    private Fips fips;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
