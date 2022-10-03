// Program: FN_READ_COST_RECOVERY_FEE_INFO, ID: 371810381, model: 746.
// Short name: SWE00547
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_COST_RECOVERY_FEE_INFO.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will readh the cost recovery fee information for a 
/// specific court for a specific date.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadCostRecoveryFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_COST_RECOVERY_FEE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCostRecoveryFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCostRecoveryFeeInfo.
  /// </summary>
  public FnReadCostRecoveryFeeInfo(IContext context, Import import,
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
    MoveTribunalFeeInformation(import.TribunalFeeInformation,
      export.TribunalFeeInformation);

    // ---------------------------------
    // If no date was entered then we will set the date to the current date so 
    // that we can display the current cost recovery fees for this tribunal.
    // ---------------------------------
    if (ReadTribunal())
    {
      export.Tribunal.Assign(entities.Tribunal);

      if (ReadFips())
      {
        export.Fips.CountyDescription = entities.Fips.CountyDescription;
      }
      else
      {
        ExitState = "FIPS_NF";

        return;
      }
    }
    else
    {
      ExitState = "TRIBUNAL_NF";

      return;
    }

    if (AsChar(import.Recf.Flag) == 'Y')
    {
      if (ReadTribunalFeeInformation1())
      {
        export.TribunalFeeInformation.Assign(entities.TribunalFeeInformation);
      }
    }
    else
    {
      if (ReadTribunalFeeInformation2())
      {
        export.TribunalFeeInformation.Assign(entities.TribunalFeeInformation);

        return;
      }

      ExitState = "TRIBUNAL_FEE_INFORMATION_NF";
    }
  }

  private static void MoveTribunalFeeInformation(TribunalFeeInformation source,
    TribunalFeeInformation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation1()
  {
    entities.TribunalFeeInformation.Populated = false;

    return Read("ReadTribunalFeeInformation1",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetDate(
          command, "effectiveDate",
          import.TribunalFeeInformation.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.TribunalFeeInformation.TrbId = db.GetInt32(reader, 0);
        entities.TribunalFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.TribunalFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.TribunalFeeInformation.Rate = db.GetNullableDecimal(reader, 3);
        entities.TribunalFeeInformation.Cap = db.GetNullableDecimal(reader, 4);
        entities.TribunalFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.TribunalFeeInformation.CreatedBy = db.GetString(reader, 6);
        entities.TribunalFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.TribunalFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.TribunalFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.TribunalFeeInformation.Description =
          db.GetNullableString(reader, 10);
        entities.TribunalFeeInformation.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation2()
  {
    entities.TribunalFeeInformation.Populated = false;

    return Read("ReadTribunalFeeInformation2",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.TribunalFeeInformation.TrbId = db.GetInt32(reader, 0);
        entities.TribunalFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.TribunalFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.TribunalFeeInformation.Rate = db.GetNullableDecimal(reader, 3);
        entities.TribunalFeeInformation.Cap = db.GetNullableDecimal(reader, 4);
        entities.TribunalFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.TribunalFeeInformation.CreatedBy = db.GetString(reader, 6);
        entities.TribunalFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.TribunalFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.TribunalFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.TribunalFeeInformation.Description =
          db.GetNullableString(reader, 10);
        entities.TribunalFeeInformation.Populated = true;
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
    /// A value of Recf.
    /// </summary>
    [JsonPropertyName("recf")]
    public Common Recf
    {
      get => recf ??= new();
      set => recf = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    private Common recf;
    private Tribunal tribunal;
    private TribunalFeeInformation tribunalFeeInformation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    private Tribunal tribunal;
    private Fips fips;
    private TribunalFeeInformation tribunalFeeInformation;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public TribunalFeeInformation Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public TribunalFeeInformation Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private TribunalFeeInformation search;
    private TribunalFeeInformation initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    /// <summary>
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
    }

    private Tribunal tribunal;
    private Fips fips;
    private TribunalFeeInformation tribunalFeeInformation;
  }
#endregion
}
