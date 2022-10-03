// Program: UPDATE_COST_RECOVERY_FEE_INFO, ID: 371810370, model: 746.
// Short name: SWE01470
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UPDATE_COST_RECOVERY_FEE_INFO.
/// </summary>
[Serializable]
public partial class UpdateCostRecoveryFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_COST_RECOVERY_FEE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateCostRecoveryFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateCostRecoveryFeeInfo.
  /// </summary>
  public UpdateCostRecoveryFeeInfo(IContext context, Import import,
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
    // ***** EDIT AREA *****
    local.Update.Flag = "";

    // *** GET CURRENCY ON PARENT TRIBUNAL
    if (ReadTribunal())
    {
      if (ReadTribunalFeeInformation1())
      {
        local.Update.Flag = "Y";
      }
    }
    else
    {
      ExitState = "TRIBUNAL_NF";

      return;
    }

    ReadTribunalFeeInformation3();
    ReadTribunalFeeInformation4();

    if (!Lt(entities.Test1.DiscontinueDate,
      import.TribunalFeeInformation.DiscontinueDate) || !
      Lt(import.TribunalFeeInformation.EffectiveDate,
      entities.Test2.EffectiveDate))
    {
      local.Update.Flag = "Y";
    }
    else if (entities.Test1.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier && (
        !Lt(entities.Test1.EffectiveDate,
      import.TribunalFeeInformation.EffectiveDate) && !
      Lt(import.TribunalFeeInformation.DiscontinueDate,
      entities.Test2.DiscontinueDate) || !
      Lt(entities.Test1.EffectiveDate,
      import.TribunalFeeInformation.EffectiveDate) && import
      .TribunalFeeInformation.SystemGeneratedIdentifier != entities
      .Test1.SystemGeneratedIdentifier || !
      Lt(import.TribunalFeeInformation.DiscontinueDate,
      entities.Test2.DiscontinueDate) && import
      .TribunalFeeInformation.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier))
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";

      return;
    }
    else
    {
      foreach(var item in ReadTribunalFeeInformation5())
      {
        if (Equal(import.TribunalFeeInformation.EffectiveDate,
          entities.TribunalFeeInformation.EffectiveDate) || Equal
          (import.TribunalFeeInformation.DiscontinueDate,
          entities.TribunalFeeInformation.DiscontinueDate) || Equal
          (import.TribunalFeeInformation.DiscontinueDate,
          entities.TribunalFeeInformation.EffectiveDate) || Equal
          (import.TribunalFeeInformation.EffectiveDate,
          entities.TribunalFeeInformation.DiscontinueDate) || Lt
          (entities.TribunalFeeInformation.EffectiveDate,
          import.TribunalFeeInformation.DiscontinueDate) && Lt
          (import.TribunalFeeInformation.DiscontinueDate,
          entities.TribunalFeeInformation.EffectiveDate) || Lt
          (entities.TribunalFeeInformation.EffectiveDate,
          import.TribunalFeeInformation.EffectiveDate) && Lt
          (import.TribunalFeeInformation.EffectiveDate,
          entities.TribunalFeeInformation.DiscontinueDate))
        {
          ExitState = "ACO_NE0000_DATE_OVERLAP";

          return;
        }
        else
        {
          continue;
        }
      }

      local.Update.Flag = "Y";
    }

    if (AsChar(local.Update.Flag) == 'Y')
    {
      if (ReadTribunalFeeInformation2())
      {
        try
        {
          UpdateTribunalFeeInformation();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "TRIBUNAL_FEE_INFORMATION_NU";

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
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 3);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation1()
  {
    entities.TribunalFeeInformation.Populated = false;

    return Read("ReadTribunalFeeInformation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "tribunalFeeId",
          import.TribunalFeeInformation.SystemGeneratedIdentifier);
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetDate(
          command, "effectiveDate",
          import.TribunalFeeInformation.EffectiveDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.TribunalFeeInformation.DiscontinueDate.GetValueOrDefault());
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
        entities.TribunalFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.TribunalFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.TribunalFeeInformation.Description =
          db.GetNullableString(reader, 8);
        entities.TribunalFeeInformation.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation2()
  {
    entities.TribunalFeeInformation.Populated = false;

    return Read("ReadTribunalFeeInformation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "tribunalFeeId",
          import.TribunalFeeInformation.SystemGeneratedIdentifier);
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
        entities.TribunalFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.TribunalFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.TribunalFeeInformation.Description =
          db.GetNullableString(reader, 8);
        entities.TribunalFeeInformation.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation3()
  {
    entities.Test1.Populated = false;

    return Read("ReadTribunalFeeInformation3",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Test1.TrbId = db.GetInt32(reader, 0);
        entities.Test1.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Test1.EffectiveDate = db.GetDate(reader, 2);
        entities.Test1.Rate = db.GetNullableDecimal(reader, 3);
        entities.Test1.Cap = db.GetNullableDecimal(reader, 4);
        entities.Test1.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Test1.CreatedBy = db.GetString(reader, 6);
        entities.Test1.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Test1.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Test1.LastUpdatedTmst = db.GetNullableDateTime(reader, 9);
        entities.Test1.Description = db.GetNullableString(reader, 10);
        entities.Test1.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation4()
  {
    entities.Test2.Populated = false;

    return Read("ReadTribunalFeeInformation4",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Test2.TrbId = db.GetInt32(reader, 0);
        entities.Test2.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.Test2.EffectiveDate = db.GetDate(reader, 2);
        entities.Test2.Rate = db.GetNullableDecimal(reader, 3);
        entities.Test2.Cap = db.GetNullableDecimal(reader, 4);
        entities.Test2.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.Test2.CreatedBy = db.GetString(reader, 6);
        entities.Test2.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Test2.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Test2.LastUpdatedTmst = db.GetNullableDateTime(reader, 9);
        entities.Test2.Description = db.GetNullableString(reader, 10);
        entities.Test2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTribunalFeeInformation5()
  {
    entities.TribunalFeeInformation.Populated = false;

    return ReadEach("ReadTribunalFeeInformation5",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetInt32(
          command, "tribunalFeeId",
          import.TribunalFeeInformation.SystemGeneratedIdentifier);
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
        entities.TribunalFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.TribunalFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.TribunalFeeInformation.Description =
          db.GetNullableString(reader, 8);
        entities.TribunalFeeInformation.Populated = true;

        return true;
      });
  }

  private void UpdateTribunalFeeInformation()
  {
    System.Diagnostics.Debug.Assert(entities.TribunalFeeInformation.Populated);

    var effectiveDate = import.TribunalFeeInformation.EffectiveDate;
    var rate = import.TribunalFeeInformation.Rate.GetValueOrDefault();
    var cap = import.TribunalFeeInformation.Cap.GetValueOrDefault();
    var discontinueDate = import.TribunalFeeInformation.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var description = import.TribunalFeeInformation.Description ?? "";

    entities.TribunalFeeInformation.Populated = false;
    Update("UpdateTribunalFeeInformation",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDecimal(command, "rate", rate);
        db.SetNullableDecimal(command, "cap", cap);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "description", description);
        db.SetInt32(command, "trbId", entities.TribunalFeeInformation.TrbId);
        db.SetInt32(
          command, "tribunalFeeId",
          entities.TribunalFeeInformation.SystemGeneratedIdentifier);
      });

    entities.TribunalFeeInformation.EffectiveDate = effectiveDate;
    entities.TribunalFeeInformation.Rate = rate;
    entities.TribunalFeeInformation.Cap = cap;
    entities.TribunalFeeInformation.DiscontinueDate = discontinueDate;
    entities.TribunalFeeInformation.LastUpdatedBy = lastUpdatedBy;
    entities.TribunalFeeInformation.LastUpdatedTmst = lastUpdatedTmst;
    entities.TribunalFeeInformation.Description = description;
    entities.TribunalFeeInformation.Populated = true;
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
    /// A value of TribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("tribunalFeeInformation")]
    public TribunalFeeInformation TribunalFeeInformation
    {
      get => tribunalFeeInformation ??= new();
      set => tribunalFeeInformation = value;
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

    private TribunalFeeInformation tribunalFeeInformation;
    private Tribunal tribunal;
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
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private Common update;
    private ExpireEffectiveDateAttributes expireEffectiveDateAttributes;
    private TribunalFeeInformation tribunalFeeInformation;
    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Test2.
    /// </summary>
    [JsonPropertyName("test2")]
    public TribunalFeeInformation Test2
    {
      get => test2 ??= new();
      set => test2 = value;
    }

    /// <summary>
    /// A value of Test1.
    /// </summary>
    [JsonPropertyName("test1")]
    public TribunalFeeInformation Test1
    {
      get => test1 ??= new();
      set => test1 = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private TribunalFeeInformation test2;
    private TribunalFeeInformation test1;
    private TribunalFeeInformation tribunalFeeInformation;
    private Tribunal tribunal;
    private TribunalFeeInformation read;
  }
#endregion
}
