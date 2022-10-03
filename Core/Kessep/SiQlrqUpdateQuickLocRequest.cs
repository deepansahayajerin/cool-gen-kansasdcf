// Program: SI_QLRQ_UPDATE_QUICK_LOC_REQUEST, ID: 372393248, model: 746.
// Short name: SWE02306
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QLRQ_UPDATE_QUICK_LOC_REQUEST.
/// </summary>
[Serializable]
public partial class SiQlrqUpdateQuickLocRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QLRQ_UPDATE_QUICK_LOC_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQlrqUpdateQuickLocRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQlrqUpdateQuickLocRequest.
  /// </summary>
  public SiQlrqUpdateQuickLocRequest(IContext context, Import import,
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
    //           M A I N T E N A N C E   L O G
    // Date	          Developer		Description
    // 2/1/1999	  Carl Ott		Initial Development
    // ------------------------------------------------------------
    if (ReadInterstateRequest())
    {
      if (ReadInterstateRequestHistory())
      {
        try
        {
          UpdateInterstateRequestHistory();
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SI0000_INTERSTAT_REQ_HIST_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SI0000_INTERSTAT_REQ_HIST_PV";

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
        ExitState = "INTERSTATE_REQUEST_HISTORY_NF";
      }
    }
    else
    {
      ExitState = "INTERSTATE_REQUEST_NF";
    }
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequestHistory()
  {
    entities.Existing.Populated = false;

    return Read("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          import.InterstateRequestHistory.CreatedTimestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Existing.IntGeneratedId = db.GetInt32(reader, 0);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Existing.CreatedBy = db.GetNullableString(reader, 2);
        entities.Existing.TransactionDirectionInd = db.GetString(reader, 3);
        entities.Existing.TransactionSerialNum = db.GetInt64(reader, 4);
        entities.Existing.ActionCode = db.GetString(reader, 5);
        entities.Existing.FunctionalTypeCode = db.GetString(reader, 6);
        entities.Existing.TransactionDate = db.GetDate(reader, 7);
        entities.Existing.ActionReasonCode = db.GetNullableString(reader, 8);
        entities.Existing.ActionResolutionDate = db.GetNullableDate(reader, 9);
        entities.Existing.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.Existing.Note = db.GetNullableString(reader, 11);
        entities.Existing.Populated = true;
      });
  }

  private void UpdateInterstateRequestHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);

    var actionResolutionDate =
      import.InterstateRequestHistory.ActionResolutionDate;

    entities.Existing.Populated = false;
    Update("UpdateInterstateRequestHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "actionResDte", actionResolutionDate);
        db.
          SetInt32(command, "intGeneratedId", entities.Existing.IntGeneratedId);
          
        db.SetDateTime(
          command, "createdTstamp",
          entities.Existing.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Existing.ActionResolutionDate = actionResolutionDate;
    entities.Existing.Populated = true;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateRequestHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public InterstateRequestHistory Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRequestHistory new1;
    private InterstateRequestHistory existing;
    private InterstateRequest interstateRequest;
  }
#endregion
}
