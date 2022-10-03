// Program: FN_UPDATE_MAXIMUM_PASSTHRU, ID: 371808068, model: 746.
// Short name: SWE00657
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_MAXIMUM_PASSTHRU.
/// </summary>
[Serializable]
public partial class FnUpdateMaximumPassthru: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_MAXIMUM_PASSTHRU program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateMaximumPassthru(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateMaximumPassthru.
  /// </summary>
  public FnUpdateMaximumPassthru(IContext context, Import import, Export export):
    
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

    // ***** EDIT AREA *****
    local.Flag.Flag = "";

    if (!Equal(import.MaximumPassthru.DiscontinueDate, null))
    {
      local.MaximumPassthru.DiscontinueDate = UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.MaximumPassthru.DiscontinueDate =
        import.MaximumPassthru.DiscontinueDate;
    }

    if (ReadMaximumPassthru1())
    {
      local.Flag.Flag = "Y";
    }

    ReadMaximumPassthru3();
    ReadMaximumPassthru4();

    if (!Lt(entities.Test1.DiscontinueDate,
      import.MaximumPassthru.DiscontinueDate) || !
      Lt(import.MaximumPassthru.EffectiveDate, entities.Test2.EffectiveDate))
    {
      local.Flag.Flag = "Y";
    }
    else if (entities.Test1.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier && (
        !Lt(entities.Test1.EffectiveDate, import.MaximumPassthru.EffectiveDate) &&
      !
      Lt(import.MaximumPassthru.DiscontinueDate, entities.Test2.DiscontinueDate) &&
      !
      Lt(entities.Test1.EffectiveDate, import.MaximumPassthru.EffectiveDate) &&
      import.MaximumPassthru.SystemGeneratedIdentifier != entities
      .Test1.SystemGeneratedIdentifier || !
      Lt(import.MaximumPassthru.DiscontinueDate, entities.Test2.DiscontinueDate) &&
      import.MaximumPassthru.SystemGeneratedIdentifier != entities
      .Test2.SystemGeneratedIdentifier))
    {
      ExitState = "CANNOT_HAVE_2_MAX_PASSTHRUS";

      return;
    }
    else
    {
      foreach(var item in ReadMaximumPassthru5())
      {
        if (Equal(import.MaximumPassthru.EffectiveDate,
          entities.ReadForUpdate.EffectiveDate) || Equal
          (import.MaximumPassthru.DiscontinueDate,
          entities.ReadForUpdate.DiscontinueDate) || Equal
          (import.MaximumPassthru.EffectiveDate,
          entities.ReadForUpdate.DiscontinueDate) || Equal
          (import.MaximumPassthru.DiscontinueDate,
          entities.ReadForUpdate.EffectiveDate) && (
            Lt(entities.ReadForUpdate.EffectiveDate,
          import.MaximumPassthru.DiscontinueDate) || Lt
          (import.MaximumPassthru.DiscontinueDate,
          entities.ReadForUpdate.DiscontinueDate)) || Lt
          (entities.ReadForUpdate.EffectiveDate,
          import.MaximumPassthru.EffectiveDate) && Lt
          (import.MaximumPassthru.EffectiveDate,
          entities.ReadForUpdate.DiscontinueDate))
        {
          ExitState = "CANNOT_HAVE_2_MAX_PASSTHRUS";

          return;
        }
        else
        {
          continue;
        }
      }

      local.Flag.Flag = "Y";
    }

    if (AsChar(local.Flag.Flag) == 'Y')
    {
      if (ReadMaximumPassthru2())
      {
        try
        {
          UpdateMaximumPassthru();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "MAXIMUM_PASSTHRU_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "MAXIMUM_PASSTHRU_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "MAXIMUM_PASSTHRU_NF";
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadMaximumPassthru1()
  {
    entities.ReadForUpdate.Populated = false;

    return Read("ReadMaximumPassthru1",
      (db, command) =>
      {
        db.SetInt32(
          command, "maxPassthruId",
          import.MaximumPassthru.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate",
          import.MaximumPassthru.EffectiveDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          import.MaximumPassthru.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReadForUpdate.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReadForUpdate.Amount = db.GetDecimal(reader, 1);
        entities.ReadForUpdate.EffectiveDate = db.GetDate(reader, 2);
        entities.ReadForUpdate.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.ReadForUpdate.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.ReadForUpdate.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ReadForUpdate.Description = db.GetNullableString(reader, 6);
        entities.ReadForUpdate.Populated = true;
      });
  }

  private bool ReadMaximumPassthru2()
  {
    entities.ReadForUpdate.Populated = false;

    return Read("ReadMaximumPassthru2",
      (db, command) =>
      {
        db.SetInt32(
          command, "maxPassthruId",
          import.MaximumPassthru.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReadForUpdate.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReadForUpdate.Amount = db.GetDecimal(reader, 1);
        entities.ReadForUpdate.EffectiveDate = db.GetDate(reader, 2);
        entities.ReadForUpdate.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.ReadForUpdate.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.ReadForUpdate.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ReadForUpdate.Description = db.GetNullableString(reader, 6);
        entities.ReadForUpdate.Populated = true;
      });
  }

  private bool ReadMaximumPassthru3()
  {
    entities.Test1.Populated = false;

    return Read("ReadMaximumPassthru3",
      null,
      (db, reader) =>
      {
        entities.Test1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test1.Amount = db.GetDecimal(reader, 1);
        entities.Test1.EffectiveDate = db.GetDate(reader, 2);
        entities.Test1.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Test1.CreatedBy = db.GetString(reader, 4);
        entities.Test1.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Test1.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Test1.LastUpdatedTmst = db.GetNullableDateTime(reader, 7);
        entities.Test1.Description = db.GetNullableString(reader, 8);
        entities.Test1.Populated = true;
      });
  }

  private bool ReadMaximumPassthru4()
  {
    entities.Test2.Populated = false;

    return Read("ReadMaximumPassthru4",
      null,
      (db, reader) =>
      {
        entities.Test2.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Test2.Amount = db.GetDecimal(reader, 1);
        entities.Test2.EffectiveDate = db.GetDate(reader, 2);
        entities.Test2.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Test2.CreatedBy = db.GetString(reader, 4);
        entities.Test2.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Test2.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Test2.LastUpdatedTmst = db.GetNullableDateTime(reader, 7);
        entities.Test2.Description = db.GetNullableString(reader, 8);
        entities.Test2.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMaximumPassthru5()
  {
    entities.ReadForUpdate.Populated = false;

    return ReadEach("ReadMaximumPassthru5",
      (db, command) =>
      {
        db.SetInt32(
          command, "maxPassthruId",
          import.MaximumPassthru.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReadForUpdate.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReadForUpdate.Amount = db.GetDecimal(reader, 1);
        entities.ReadForUpdate.EffectiveDate = db.GetDate(reader, 2);
        entities.ReadForUpdate.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.ReadForUpdate.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.ReadForUpdate.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.ReadForUpdate.Description = db.GetNullableString(reader, 6);
        entities.ReadForUpdate.Populated = true;

        return true;
      });
  }

  private void UpdateMaximumPassthru()
  {
    var amount = import.MaximumPassthru.Amount;
    var effectiveDate = import.MaximumPassthru.EffectiveDate;
    var discontinueDate = import.MaximumPassthru.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var description = import.MaximumPassthru.Description ?? "";

    entities.ReadForUpdate.Populated = false;
    Update("UpdateMaximumPassthru",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "maxPassthruId",
          entities.ReadForUpdate.SystemGeneratedIdentifier);
      });

    entities.ReadForUpdate.Amount = amount;
    entities.ReadForUpdate.EffectiveDate = effectiveDate;
    entities.ReadForUpdate.DiscontinueDate = discontinueDate;
    entities.ReadForUpdate.LastUpdatedBy = lastUpdatedBy;
    entities.ReadForUpdate.LastUpdatedTmst = lastUpdatedTmst;
    entities.ReadForUpdate.Description = description;
    entities.ReadForUpdate.Populated = true;
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
    /// A value of MaximumPassthru.
    /// </summary>
    [JsonPropertyName("maximumPassthru")]
    public MaximumPassthru MaximumPassthru
    {
      get => maximumPassthru ??= new();
      set => maximumPassthru = value;
    }

    private MaximumPassthru maximumPassthru;
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
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// A value of MaximumPassthru.
    /// </summary>
    [JsonPropertyName("maximumPassthru")]
    public MaximumPassthru MaximumPassthru
    {
      get => maximumPassthru ??= new();
      set => maximumPassthru = value;
    }

    private Common flag;
    private MaximumPassthru maximumPassthru;
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
    public MaximumPassthru Test2
    {
      get => test2 ??= new();
      set => test2 = value;
    }

    /// <summary>
    /// A value of Test1.
    /// </summary>
    [JsonPropertyName("test1")]
    public MaximumPassthru Test1
    {
      get => test1 ??= new();
      set => test1 = value;
    }

    /// <summary>
    /// A value of ReadForEditCheck.
    /// </summary>
    [JsonPropertyName("readForEditCheck")]
    public MaximumPassthru ReadForEditCheck
    {
      get => readForEditCheck ??= new();
      set => readForEditCheck = value;
    }

    /// <summary>
    /// A value of ReadForUpdate.
    /// </summary>
    [JsonPropertyName("readForUpdate")]
    public MaximumPassthru ReadForUpdate
    {
      get => readForUpdate ??= new();
      set => readForUpdate = value;
    }

    private MaximumPassthru test2;
    private MaximumPassthru test1;
    private MaximumPassthru readForEditCheck;
    private MaximumPassthru readForUpdate;
  }
#endregion
}
