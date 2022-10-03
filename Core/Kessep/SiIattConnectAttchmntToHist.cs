// Program: SI_IATT_CONNECT_ATTCHMNT_TO_HIST, ID: 372381438, model: 746.
// Short name: SWE02310
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_IATT_CONNECT_ATTCHMNT_TO_HIST.
/// </summary>
[Serializable]
public partial class SiIattConnectAttchmntToHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IATT_CONNECT_ATTCHMNT_TO_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIattConnectAttchmntToHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIattConnectAttchmntToHist.
  /// </summary>
  public SiIattConnectAttchmntToHist(IContext context, Import import,
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
    // 02/09/1999	  Carl Ott		Initial Development
    // ------------------------------------------------------------
    // ****************************************************************
    // In order to delete Interstate Request Attachments, it is necessary to 
    // identify the Interstate Request History record that corresponds to the
    // attachment to be deleted.  This action blocks creates a connection
    // between the entity records using the Created Timestamp.
    // *****************************************************************
    foreach(var item in ReadInterstateRequestAttachmentInterstateRequest())
    {
      if (ReadInterstateRequestHistory())
      {
        continue;
      }
      else
      {
        try
        {
          UpdateInterstateRequestAttachment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
  }

  private IEnumerable<bool> ReadInterstateRequestAttachmentInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;
    entities.InterstateRequestAttachment.Populated = false;

    return ReadEach("ReadInterstateRequestAttachmentInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "intHGeneratedId", import.InterstateRequest.IntHGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.InterstateRequestAttachment.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequestAttachment.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateRequestAttachment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.InterstateRequestAttachment.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.InterstateRequestAttachment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.InterstateRequest.Populated = true;
        entities.InterstateRequestAttachment.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequestHistory()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          entities.InterstateRequestAttachment.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private void UpdateInterstateRequestAttachment()
  {
    System.Diagnostics.Debug.Assert(
      entities.InterstateRequestAttachment.Populated);

    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var createdTimestamp = import.InterstateRequestHistory.CreatedTimestamp;

    entities.InterstateRequestAttachment.Populated = false;
    Update("UpdateInterstateRequestAttachment",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetInt32(
          command, "intHGeneratedId",
          entities.InterstateRequestAttachment.IntHGeneratedId);
        db.SetInt32(
          command, "sysGenSeqNbr",
          entities.InterstateRequestAttachment.SystemGeneratedSequenceNum);
      });

    entities.InterstateRequestAttachment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateRequestAttachment.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequestAttachment.CreatedTimestamp = createdTimestamp;
    entities.InterstateRequestAttachment.Populated = true;
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

    /// <summary>
    /// A value of InterstateRequestAttachment.
    /// </summary>
    [JsonPropertyName("interstateRequestAttachment")]
    public InterstateRequestAttachment InterstateRequestAttachment
    {
      get => interstateRequestAttachment ??= new();
      set => interstateRequestAttachment = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private InterstateRequestAttachment interstateRequestAttachment;
  }
#endregion
}
