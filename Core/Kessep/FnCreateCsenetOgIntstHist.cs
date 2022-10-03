// Program: FN_CREATE_CSENET_OG_INTST_HIST, ID: 373493448, model: 746.
// Short name: SWE01307
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_CSENET_OG_INTST_HIST.
/// </summary>
[Serializable]
public partial class FnCreateCsenetOgIntstHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CSENET_OG_INTST_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCsenetOgIntstHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCsenetOgIntstHist.
  /// </summary>
  public FnCreateCsenetOgIntstHist(IContext context, Import import,
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
    if (ReadInterstateRequest())
    {
      try
      {
        CreateInterstateRequestHistory();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INTERSTATE_REQUEST_HISTORY_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INTERSTATE_REQUEST_HISTORY_PV";

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
      ExitState = "INTERSTATE_REQUEST_NF";
    }
  }

  private void CreateInterstateRequestHistory()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var createdTimestamp = import.InterstateRequestHistory.CreatedTimestamp;
    var createdBy = import.InterstateRequestHistory.CreatedBy ?? "";
    var transactionDirectionInd =
      import.InterstateRequestHistory.TransactionDirectionInd;
    var transactionSerialNum =
      import.InterstateRequestHistory.TransactionSerialNum;
    var actionCode = import.InterstateRequestHistory.ActionCode;
    var functionalTypeCode = import.InterstateRequestHistory.FunctionalTypeCode;
    var transactionDate = import.InterstateRequestHistory.TransactionDate;
    var actionReasonCode = import.InterstateRequestHistory.ActionReasonCode ?? ""
      ;
    var actionResolutionDate =
      import.InterstateRequestHistory.ActionResolutionDate;
    var attachmentIndicator =
      import.InterstateRequestHistory.AttachmentIndicator ?? "";
    var note = import.InterstateRequestHistory.Note ?? "";

    entities.InterstateRequestHistory.Populated = false;
    Update("CreateInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDateTime(command, "createdTstamp", createdTimestamp);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetString(command, "transactionDirect", transactionDirectionInd);
        db.SetInt64(command, "transactionSerial", transactionSerialNum);
        db.SetString(command, "actionCode", actionCode);
        db.SetString(command, "functionalTypeCo", functionalTypeCode);
        db.SetDate(command, "transactionDate", transactionDate);
        db.SetNullableString(command, "actionReasonCode", actionReasonCode);
        db.SetNullableDate(command, "actionResDte", actionResolutionDate);
        db.SetNullableString(command, "attachmentIndicat", attachmentIndicator);
        db.SetNullableString(command, "note", note);
      });

    entities.InterstateRequestHistory.IntGeneratedId = intGeneratedId;
    entities.InterstateRequestHistory.CreatedTimestamp = createdTimestamp;
    entities.InterstateRequestHistory.CreatedBy = createdBy;
    entities.InterstateRequestHistory.TransactionDirectionInd =
      transactionDirectionInd;
    entities.InterstateRequestHistory.TransactionSerialNum =
      transactionSerialNum;
    entities.InterstateRequestHistory.ActionCode = actionCode;
    entities.InterstateRequestHistory.FunctionalTypeCode = functionalTypeCode;
    entities.InterstateRequestHistory.TransactionDate = transactionDate;
    entities.InterstateRequestHistory.ActionReasonCode = actionReasonCode;
    entities.InterstateRequestHistory.ActionResolutionDate =
      actionResolutionDate;
    entities.InterstateRequestHistory.AttachmentIndicator = attachmentIndicator;
    entities.InterstateRequestHistory.Note = note;
    entities.InterstateRequestHistory.Populated = true;
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
        entities.InterstateRequest.Populated = true;
      });
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
