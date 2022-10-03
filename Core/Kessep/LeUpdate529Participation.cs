// Program: LE_UPDATE_529_PARTICIPATION, ID: 1902442541, model: 746.
// Short name: SWE00090
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_UPDATE_529_PARTICIPATION.
/// </summary>
[Serializable]
public partial class LeUpdate529Participation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_UPDATE_529_PARTICIPATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeUpdate529Participation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeUpdate529Participation.
  /// </summary>
  public LeUpdate529Participation(IContext context, Import import, Export export)
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

    if (!Read529AccountParticipant())
    {
      ExitState = "529_PARTICIPANT_NF";

      return;
    }

    try
    {
      Update529AccountParticipant();
      export.Data529AccountParticipant.
        Assign(entities.Data529AccountParticipant);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "529_PARTICIPANT_NU";

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

  private bool Read529AccountParticipant()
  {
    entities.Data529AccountParticipant.Populated = false;

    return Read("Read529AccountParticipant",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(
          command, "identifier", import.Data529AccountParticipant.Identifier);
      },
      (db, reader) =>
      {
        entities.Data529AccountParticipant.Identifier = db.GetInt32(reader, 0);
        entities.Data529AccountParticipant.StartDate =
          db.GetNullableDate(reader, 1);
        entities.Data529AccountParticipant.EndDate =
          db.GetNullableDate(reader, 2);
        entities.Data529AccountParticipant.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.Data529AccountParticipant.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.Data529AccountParticipant.CspNumber = db.GetString(reader, 5);
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

  private void Update529AccountParticipant()
  {
    System.Diagnostics.Debug.
      Assert(entities.Data529AccountParticipant.Populated);

    var startDate = import.Data529AccountParticipant.StartDate;
    var endDate = import.Data529AccountParticipant.EndDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.Data529AccountParticipant.Populated = false;
    Update("Update529AccountParticipant",
      (db, command) =>
      {
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "identifier", entities.Data529AccountParticipant.Identifier);
          
        db.SetString(
          command, "cspNumber", entities.Data529AccountParticipant.CspNumber);
      });

    entities.Data529AccountParticipant.StartDate = startDate;
    entities.Data529AccountParticipant.EndDate = endDate;
    entities.Data529AccountParticipant.LastUpdatedBy = lastUpdatedBy;
    entities.Data529AccountParticipant.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.Data529AccountParticipant.Populated = true;
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
    /// A value of Data529AccountParticipant.
    /// </summary>
    [JsonPropertyName("data529AccountParticipant")]
    public Data529AccountParticipant Data529AccountParticipant
    {
      get => data529AccountParticipant ??= new();
      set => data529AccountParticipant = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private Data529AccountParticipant data529AccountParticipant;
    private CsePersonsWorkSet csePersonsWorkSet;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Data529AccountParticipant data529AccountParticipant;
    private CsePerson csePerson;
  }
#endregion
}
