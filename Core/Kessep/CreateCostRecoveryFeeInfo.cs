// Program: CREATE_COST_RECOVERY_FEE_INFO, ID: 371810380, model: 746.
// Short name: SWE00131
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_COST_RECOVERY_FEE_INFO.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class CreateCostRecoveryFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_COST_RECOVERY_FEE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateCostRecoveryFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateCostRecoveryFeeInfo.
  /// </summary>
  public CreateCostRecoveryFeeInfo(IContext context, Import import,
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
    export.Fips.CountyDescription = import.Fips.CountyDescription;
    export.Tribunal.Assign(import.Tribunal);
    MoveTribunalFeeInformation(import.TribunalFeeInformation,
      export.TribunalFeeInformation);

    if (ReadTribunal())
    {
      export.Tribunal.Assign(entities.ExistingTribunal);

      if (ReadFips())
      {
        export.Fips.CountyDescription = entities.ExistingFips.CountyDescription;
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

    // ------------------------------
    // While creating a new Tribunal Fee Information for the same Tribunal Check
    // to be done to  make sure that no Overlaps exists with any of the
    // existing Tribunal Fee Informations for the same Tribunal.
    // ------------------------------
    if (ReadTribunalFeeInformation())
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";
    }
    else
    {
      try
      {
        CreateTribunalFeeInformation();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "TRIBUNAL_FEE_INFORMATION_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "TRIBUNAL_FEE_INFORMATION_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveTribunalFeeInformation(TribunalFeeInformation source,
    TribunalFeeInformation target)
  {
    target.Rate = source.Rate;
    target.Cap = source.Cap;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.Description = source.Description;
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void CreateTribunalFeeInformation()
  {
    var trbId = entities.ExistingTribunal.Identifier;
    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDate = export.TribunalFeeInformation.EffectiveDate;
    var rate = export.TribunalFeeInformation.Rate.GetValueOrDefault();
    var cap = export.TribunalFeeInformation.Cap.GetValueOrDefault();
    var discontinueDate = export.TribunalFeeInformation.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var description = export.TribunalFeeInformation.Description ?? "";

    entities.Read.Populated = false;
    Update("CreateTribunalFeeInformation",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", trbId);
        db.SetInt32(command, "tribunalFeeId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDecimal(command, "rate", rate);
        db.SetNullableDecimal(command, "cap", cap);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.Read.TrbId = trbId;
    entities.Read.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Read.EffectiveDate = effectiveDate;
    entities.Read.Rate = rate;
    entities.Read.Cap = cap;
    entities.Read.DiscontinueDate = discontinueDate;
    entities.Read.CreatedBy = createdBy;
    entities.Read.CreatedTimestamp = createdTimestamp;
    entities.Read.Description = description;
    entities.Read.Populated = true;
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingTribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingTribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingTribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.ExistingTribunal.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation()
  {
    entities.Read.Populated = false;

    return Read("ReadTribunalFeeInformation",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          import.TribunalFeeInformation.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.TribunalFeeInformation.DiscontinueDate.GetValueOrDefault());
        db.SetInt32(command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Read.TrbId = db.GetInt32(reader, 0);
        entities.Read.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Read.EffectiveDate = db.GetDate(reader, 2);
        entities.Read.Rate = db.GetNullableDecimal(reader, 3);
        entities.Read.Cap = db.GetNullableDecimal(reader, 4);
        entities.Read.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Read.CreatedBy = db.GetString(reader, 6);
        entities.Read.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Read.Description = db.GetNullableString(reader, 8);
        entities.Read.Populated = true;
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
    /// A value of Import2.
    /// </summary>
    [JsonPropertyName("import2")]
    public TribunalFeeInformation Import2
    {
      get => import2 ??= new();
      set => import2 = value;
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

    private TribunalFeeInformation import2;
    private Fips fips;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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

    private Fips fips;
    private Tribunal tribunal;
    private TribunalFeeInformation tribunalFeeInformation;
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
    /// A value of CreateAttempts.
    /// </summary>
    [JsonPropertyName("createAttempts")]
    public Common CreateAttempts
    {
      get => createAttempts ??= new();
      set => createAttempts = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    /// <summary>
    /// A value of ExpireEffectiveDateAttributes.
    /// </summary>
    [JsonPropertyName("expireEffectiveDateAttributes")]
    public ExpireEffectiveDateAttributes ExpireEffectiveDateAttributes
    {
      get => expireEffectiveDateAttributes ??= new();
      set => expireEffectiveDateAttributes = value;
    }

    private DateWorkArea current;
    private Common createAttempts;
    private DateWorkArea initialized;
    private TribunalFeeInformation tribunalFeeInformation;
    private ExpireEffectiveDateAttributes expireEffectiveDateAttributes;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public TribunalFeeInformation Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public TribunalFeeInformation Create
    {
      get => create ??= new();
      set => create = value;
    }

    private Fips existingFips;
    private Tribunal existingTribunal;
    private TribunalFeeInformation read;
    private TribunalFeeInformation create;
  }
#endregion
}
