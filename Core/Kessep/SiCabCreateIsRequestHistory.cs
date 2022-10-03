// Program: SI_CAB_CREATE_IS_REQUEST_HISTORY, ID: 373418870, model: 746.
// Short name: SWE01967
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_CREATE_IS_REQUEST_HISTORY.
/// </summary>
[Serializable]
public partial class SiCabCreateIsRequestHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_CREATE_IS_REQUEST_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabCreateIsRequestHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabCreateIsRequestHistory.
  /// </summary>
  public SiCabCreateIsRequestHistory(IContext context, Import import,
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
    if (!IsEmpty(import.InterstateRequestHistory.CreatedBy))
    {
      local.InterstateRequestHistory.CreatedBy =
        import.InterstateRequestHistory.CreatedBy ?? "";
    }
    else
    {
      local.InterstateRequestHistory.CreatedBy = global.UserId;
    }

    local.InterstateRequestHistory.CreatedTimestamp = Now();
    local.Common.Count = 0;

    if (ReadInterstateRequest())
    {
      while(local.Common.Count < 25)
      {
        try
        {
          CreateInterstateRequestHistory();

          return;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.Common.Count;
              local.InterstateRequestHistory.CreatedTimestamp =
                AddMicroseconds(local.InterstateRequestHistory.CreatedTimestamp,
                1);

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SI0000_INTERSTAT_REQ_HIST_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      ExitState = "SI0000_INTERSTAT_REQ_HIST_AE";
    }
    else
    {
      ExitState = "INTERSTATE_REQUEST_NF";
    }
  }

  private void CreateInterstateRequestHistory()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var createdTimestamp = local.InterstateRequestHistory.CreatedTimestamp;
    var createdBy = local.InterstateRequestHistory.CreatedBy ?? "";
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    private InterstateRequest interstateRequest;
    private InterstateRequestHistory interstateRequestHistory;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    private Common common;
    private InterstateRequestHistory interstateRequestHistory;
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
