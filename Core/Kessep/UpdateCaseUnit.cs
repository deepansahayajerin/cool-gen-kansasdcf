// Program: UPDATE_CASE_UNIT, ID: 372068826, model: 746.
// Short name: SWE01860
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_CASE_UNIT.
/// </para>
/// <para>
/// Elementary process.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateCaseUnit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_CASE_UNIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateCaseUnit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateCaseUnit.
  /// </summary>
  public UpdateCaseUnit(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadCaseUnit())
    {
      ExitState = "CASE_UNIT_NF";

      return;
    }

    try
    {
      UpdateCaseUnit1();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CASE_ROLE_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CASE_UNIT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", import.Case1.Number);
        db.SetInt32(command, "cuNumber", import.CaseUnit.CuNumber);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.CaseUnit.CasNo = db.GetString(reader, 4);
        entities.CaseUnit.Populated = true;
      });
  }

  private void UpdateCaseUnit1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);

    var state = import.CaseUnit.State;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.CaseUnit.Populated = false;
    Update("UpdateCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "state", state);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "cuNumber", entities.CaseUnit.CuNumber);
        db.SetString(command, "casNo", entities.CaseUnit.CasNo);
      });

    entities.CaseUnit.State = state;
    entities.CaseUnit.LastUpdatedBy = lastUpdatedBy;
    entities.CaseUnit.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CaseUnit.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private Case1 case1;
    private CaseUnit caseUnit;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private Case1 case1;
    private CaseUnit caseUnit;
  }
#endregion
}
