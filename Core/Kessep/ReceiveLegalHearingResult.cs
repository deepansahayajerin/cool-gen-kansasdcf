// Program: RECEIVE_LEGAL_HEARING_RESULT, ID: 372012091, model: 746.
// Short name: SWE01049
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: RECEIVE_LEGAL_HEARING_RESULT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process updates HEARING.
/// </para>
/// </summary>
[Serializable]
public partial class ReceiveLegalHearingResult: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the RECEIVE_LEGAL_HEARING_RESULT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ReceiveLegalHearingResult(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ReceiveLegalHearingResult.
  /// </summary>
  public ReceiveLegalHearingResult(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 06/30/95	Dave Allen			Initial Code
    // ------------------------------------------------------------
    // 09/28/17    JHarden     CQ58574  Allow a note to be added to the HEAR 
    // screen.
    export.HearingDateChanged.Flag = "N";
    export.OutcomeRcvdDateChanged.Flag = "N";

    if (ReadHearing())
    {
      if (!Equal(import.Hearing.ConductedDate, entities.Hearing.ConductedDate))
      {
        export.HearingDateChanged.Flag = "Y";
      }

      if (!Equal(import.Hearing.OutcomeReceivedDate,
        entities.Hearing.OutcomeReceivedDate))
      {
        export.OutcomeRcvdDateChanged.Flag = "Y";
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
        entities.Hearing.Type1 = db.GetNullableString(reader, 3);
        entities.Hearing.LastName = db.GetString(reader, 4);
        entities.Hearing.FirstName = db.GetString(reader, 5);
        entities.Hearing.MiddleInt = db.GetNullableString(reader, 6);
        entities.Hearing.Suffix = db.GetNullableString(reader, 7);
        entities.Hearing.Title = db.GetNullableString(reader, 8);
        entities.Hearing.OutcomeReceivedDate = db.GetNullableDate(reader, 9);
        entities.Hearing.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.Hearing.LastUpdatedTstamp = db.GetNullableDateTime(reader, 11);
        entities.Hearing.Outcome = db.GetNullableString(reader, 12);
        entities.Hearing.Note = db.GetNullableString(reader, 13);
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
    var note = import.Hearing.Note ?? "";

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
        db.SetNullableString(command, "note", note);
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
    entities.Hearing.Note = note;
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
    /// A value of OutcomeRcvdDateChanged.
    /// </summary>
    [JsonPropertyName("outcomeRcvdDateChanged")]
    public Common OutcomeRcvdDateChanged
    {
      get => outcomeRcvdDateChanged ??= new();
      set => outcomeRcvdDateChanged = value;
    }

    /// <summary>
    /// A value of HearingDateChanged.
    /// </summary>
    [JsonPropertyName("hearingDateChanged")]
    public Common HearingDateChanged
    {
      get => hearingDateChanged ??= new();
      set => hearingDateChanged = value;
    }

    private Common outcomeRcvdDateChanged;
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
