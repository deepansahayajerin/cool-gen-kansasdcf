// Program: FN_CREATE_MAXIMUM_PASSTHRU, ID: 371808069, model: 746.
// Short name: SWE00371
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_MAXIMUM_PASSTHRU.
/// </summary>
[Serializable]
public partial class FnCreateMaximumPassthru: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_MAXIMUM_PASSTHRU program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateMaximumPassthru(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateMaximumPassthru.
  /// </summary>
  public FnCreateMaximumPassthru(IContext context, Import import, Export export):
    
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
    // All validations should be checked already at this entry point.
    if (ReadMaximumPassthru())
    {
      ExitState = "CANNOT_HAVE_2_MAX_PASSTHRUS";
    }
    else
    {
      try
      {
        CreateMaximumPassthru();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "MAXIMUM_PASSTHRU_AE";

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
  }

  private int UseFnAssignMaxPassthruId()
  {
    var useImport = new FnAssignMaxPassthruId.Import();
    var useExport = new FnAssignMaxPassthruId.Export();

    Call(FnAssignMaxPassthruId.Execute, useImport, useExport);

    return useExport.MaximumPassthru.SystemGeneratedIdentifier;
  }

  private void CreateMaximumPassthru()
  {
    var systemGeneratedIdentifier = UseFnAssignMaxPassthruId();
    var amount = import.MaximumPassthru.Amount;
    var effectiveDate = import.MaximumPassthru.EffectiveDate;
    var discontinueDate = import.MaximumPassthru.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var description = import.MaximumPassthru.Description ?? "";

    entities.Create.Populated = false;
    Update("CreateMaximumPassthru",
      (db, command) =>
      {
        db.SetInt32(command, "maxPassthruId", systemGeneratedIdentifier);
        db.SetDecimal(command, "amount", amount);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.Create.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Create.Amount = amount;
    entities.Create.EffectiveDate = effectiveDate;
    entities.Create.DiscontinueDate = discontinueDate;
    entities.Create.CreatedBy = createdBy;
    entities.Create.CreatedTimestamp = createdTimestamp;
    entities.Create.Description = description;
    entities.Create.Populated = true;
  }

  private bool ReadMaximumPassthru()
  {
    entities.Create.Populated = false;

    return Read("ReadMaximumPassthru",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate1",
          import.MaximumPassthru.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.MaximumPassthru.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Create.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Create.Amount = db.GetDecimal(reader, 1);
        entities.Create.EffectiveDate = db.GetDate(reader, 2);
        entities.Create.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Create.CreatedBy = db.GetString(reader, 4);
        entities.Create.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Create.Description = db.GetNullableString(reader, 6);
        entities.Create.Populated = true;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public MaximumPassthru Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public MaximumPassthru Create
    {
      get => create ??= new();
      set => create = value;
    }

    private MaximumPassthru read;
    private MaximumPassthru create;
  }
#endregion
}
