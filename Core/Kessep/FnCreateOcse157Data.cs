// Program: FN_CREATE_OCSE157_DATA, ID: 371094645, model: 746.
// Short name: SWE02912
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_OCSE157_DATA.
/// </summary>
[Serializable]
public partial class FnCreateOcse157Data: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_OCSE157_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateOcse157Data(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateOcse157Data.
  /// </summary>
  public FnCreateOcse157Data(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    for(local.NbrOfTries.Count = 1; local.NbrOfTries.Count <= 5; ++
      local.NbrOfTries.Count)
    {
      try
      {
        CreateOcse157Data();

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // -------------------------------------------------------------
    // Primary Key is Created_ts. So we should never hit ae
    // condition. Hence no Exit State is necessary.
    // ------------------------------------------------------------
  }

  private void CreateOcse157Data()
  {
    var fiscalYear = import.Ocse157Data.FiscalYear.GetValueOrDefault();
    var runNumber = import.Ocse157Data.RunNumber.GetValueOrDefault();
    var lineNumber = import.Ocse157Data.LineNumber ?? "";
    var column = import.Ocse157Data.Column ?? "";
    var createdTimestamp = Now();
    var number = import.Ocse157Data.Number.GetValueOrDefault();

    entities.Ocse157Data.Populated = false;
    Update("CreateOcse157Data",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fiscalYear", fiscalYear);
        db.SetNullableInt32(command, "runNumber", runNumber);
        db.SetNullableString(command, "lineNumber", lineNumber);
        db.SetNullableString(command, "column0", column);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt64(command, "number", number);
      });

    entities.Ocse157Data.FiscalYear = fiscalYear;
    entities.Ocse157Data.RunNumber = runNumber;
    entities.Ocse157Data.LineNumber = lineNumber;
    entities.Ocse157Data.Column = column;
    entities.Ocse157Data.CreatedTimestamp = createdTimestamp;
    entities.Ocse157Data.Number = number;
    entities.Ocse157Data.Populated = true;
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
    /// A value of Ocse157Data.
    /// </summary>
    [JsonPropertyName("ocse157Data")]
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
    }

    private Ocse157Data ocse157Data;
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
    /// A value of NbrOfTries.
    /// </summary>
    [JsonPropertyName("nbrOfTries")]
    public Common NbrOfTries
    {
      get => nbrOfTries ??= new();
      set => nbrOfTries = value;
    }

    private Common nbrOfTries;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ocse157Data.
    /// </summary>
    [JsonPropertyName("ocse157Data")]
    public Ocse157Data Ocse157Data
    {
      get => ocse157Data ??= new();
      set => ocse157Data = value;
    }

    private Ocse157Data ocse157Data;
  }
#endregion
}
