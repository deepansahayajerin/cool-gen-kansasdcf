// Program: LE_CREATE_529_PARTICIPATION, ID: 1902442540, model: 746.
// Short name: SWE00089
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CREATE_529_PARTICIPATION.
/// </summary>
[Serializable]
public partial class LeCreate529Participation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_529_PARTICIPATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreate529Participation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreate529Participation.
  /// </summary>
  public LeCreate529Participation(IContext context, Import import, Export export)
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
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.Data529AccountParticipant.Identifier = 1;

    if (Read529AccountParticipant())
    {
      local.Data529AccountParticipant.Identifier =
        entities.Data529AccountParticipant.Identifier + 1;
    }

    try
    {
      Create529AccountParticipant();
      export.Data529AccountParticipant.
        Assign(entities.Data529AccountParticipant);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "529_PARTICIPANT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "529_PARTICIPANT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void Create529AccountParticipant()
  {
    var identifier = local.Data529AccountParticipant.Identifier;
    var standardNumber = import.Data529AccountParticipant.StandardNumber ?? "";
    var startDate = import.Data529AccountParticipant.StartDate;
    var endDate = import.Data529AccountParticipant.EndDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cspNumber = entities.CsePerson.Number;

    entities.Data529AccountParticipant.Populated = false;
    Update("Create529AccountParticipant",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "standardNo", standardNumber);
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "cspNumber", cspNumber);
      });

    entities.Data529AccountParticipant.Identifier = identifier;
    entities.Data529AccountParticipant.StandardNumber = standardNumber;
    entities.Data529AccountParticipant.StartDate = startDate;
    entities.Data529AccountParticipant.EndDate = endDate;
    entities.Data529AccountParticipant.CreatedBy = createdBy;
    entities.Data529AccountParticipant.CreatedTimestamp = createdTimestamp;
    entities.Data529AccountParticipant.LastUpdatedBy = "";
    entities.Data529AccountParticipant.LastUpdatedTimestamp = null;
    entities.Data529AccountParticipant.CspNumber = cspNumber;
    entities.Data529AccountParticipant.Populated = true;
  }

  private bool Read529AccountParticipant()
  {
    entities.Data529AccountParticipant.Populated = false;

    return Read("Read529AccountParticipant",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Data529AccountParticipant.Identifier = db.GetInt32(reader, 0);
        entities.Data529AccountParticipant.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.Data529AccountParticipant.StartDate =
          db.GetNullableDate(reader, 2);
        entities.Data529AccountParticipant.EndDate =
          db.GetNullableDate(reader, 3);
        entities.Data529AccountParticipant.CreatedBy = db.GetString(reader, 4);
        entities.Data529AccountParticipant.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.Data529AccountParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.Data529AccountParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.Data529AccountParticipant.CspNumber = db.GetString(reader, 8);
        entities.Data529AccountParticipant.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Data529AccountParticipant.
    /// </summary>
    [JsonPropertyName("data529AccountParticipant")]
    public Data529AccountParticipant Data529AccountParticipant
    {
      get => data529AccountParticipant ??= new();
      set => data529AccountParticipant = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Data529AccountParticipant data529AccountParticipant;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Data529AccountParticipant.
    /// </summary>
    [JsonPropertyName("data529AccountParticipant")]
    public Data529AccountParticipant Data529AccountParticipant
    {
      get => data529AccountParticipant ??= new();
      set => data529AccountParticipant = value;
    }

    private Data529AccountParticipant data529AccountParticipant;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Data529AccountParticipant.
    /// </summary>
    [JsonPropertyName("data529AccountParticipant")]
    public Data529AccountParticipant Data529AccountParticipant
    {
      get => data529AccountParticipant ??= new();
      set => data529AccountParticipant = value;
    }

    private Data529AccountParticipant data529AccountParticipant;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Data529AccountParticipant.
    /// </summary>
    [JsonPropertyName("data529AccountParticipant")]
    public Data529AccountParticipant Data529AccountParticipant
    {
      get => data529AccountParticipant ??= new();
      set => data529AccountParticipant = value;
    }

    private CsePerson csePerson;
    private Data529AccountParticipant data529AccountParticipant;
  }
#endregion
}
