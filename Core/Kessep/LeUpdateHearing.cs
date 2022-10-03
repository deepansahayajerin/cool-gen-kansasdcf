// Program: LE_UPDATE_HEARING, ID: 372582897, model: 746.
// Short name: SWE00829
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_UPDATE_HEARING.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This updates the Hearing for an Administrative Appeal or a Legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeUpdateHearing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_UPDATE_HEARING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeUpdateHearing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeUpdateHearing.
  /// </summary>
  public LeUpdateHearing(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 06/30/95	Stephen Benton			Initial Code
    // ------------------------------------------------------------
    export.HearingDateChanged.Flag = "N";

    if (ReadHearing())
    {
      if (!Equal(entities.Hearing.ConductedDate, import.Hearing.ConductedDate))
      {
        export.HearingDateChanged.Flag = "Y";
      }

      try
      {
        UpdateHearing();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "HEARING_NU";

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
    else
    {
      ExitState = "HEARING_NF";
    }
  }

  private bool ReadHearing()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing",
      (db, command) =>
      {
        db.SetInt32(
          command, "hearingId", import.Hearing.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.ConductedDate = db.GetDate(reader, 1);
        entities.Hearing.ConductedTime = db.GetTimeSpan(reader, 2);
        entities.Hearing.LastName = db.GetString(reader, 3);
        entities.Hearing.FirstName = db.GetString(reader, 4);
        entities.Hearing.MiddleInt = db.GetNullableString(reader, 5);
        entities.Hearing.Suffix = db.GetNullableString(reader, 6);
        entities.Hearing.Title = db.GetNullableString(reader, 7);
        entities.Hearing.OutcomeReceivedDate = db.GetNullableDate(reader, 8);
        entities.Hearing.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.Hearing.LastUpdatedTstamp = db.GetNullableDateTime(reader, 10);
        entities.Hearing.Outcome = db.GetNullableString(reader, 11);
        entities.Hearing.Populated = true;
      });
  }

  private void UpdateHearing()
  {
    var conductedDate = import.Hearing.ConductedDate;
    var conductedTime = import.Hearing.ConductedTime;
    var lastName = import.Hearing.LastName;
    var firstName = import.Hearing.FirstName;
    var middleInt = import.Hearing.MiddleInt ?? "";
    var suffix = import.Hearing.Suffix ?? "";
    var title = import.Hearing.Title ?? "";
    var outcomeReceivedDate = import.Hearing.OutcomeReceivedDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var outcome = import.Hearing.Outcome ?? "";

    entities.Hearing.Populated = false;
    Update("UpdateHearing",
      (db, command) =>
      {
        db.SetDate(command, "hearingDt", conductedDate);
        db.SetTimeSpan(command, "hearingTime", conductedTime);
        db.SetString(command, "lastNm", lastName);
        db.SetString(command, "firstNm", firstName);
        db.SetNullableString(command, "middleInt", middleInt);
        db.SetNullableString(command, "suffix", suffix);
        db.SetNullableString(command, "title", title);
        db.SetNullableDate(command, "outcomeReceiveDt", outcomeReceivedDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "outcome", outcome);
        db.SetInt32(
          command, "hearingId", entities.Hearing.SystemGeneratedIdentifier);
      });

    entities.Hearing.ConductedDate = conductedDate;
    entities.Hearing.ConductedTime = conductedTime;
    entities.Hearing.LastName = lastName;
    entities.Hearing.FirstName = firstName;
    entities.Hearing.MiddleInt = middleInt;
    entities.Hearing.Suffix = suffix;
    entities.Hearing.Title = title;
    entities.Hearing.OutcomeReceivedDate = outcomeReceivedDate;
    entities.Hearing.LastUpdatedBy = lastUpdatedBy;
    entities.Hearing.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.Hearing.Outcome = outcome;
    entities.Hearing.Populated = true;
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
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    private Hearing hearing;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HearingDateChanged.
    /// </summary>
    [JsonPropertyName("hearingDateChanged")]
    public Common HearingDateChanged
    {
      get => hearingDateChanged ??= new();
      set => hearingDateChanged = value;
    }

    private Common hearingDateChanged;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    private Hearing hearing;
  }
#endregion
}
