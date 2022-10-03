// Program: SI_IATT_UPDATE_IS_REQUEST_ATTACH, ID: 372729678, model: 746.
// Short name: SWE01254
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_IATT_UPDATE_IS_REQUEST_ATTACH.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB updates the Interstate Request Attachments.
/// </para>
/// </summary>
[Serializable]
public partial class SiIattUpdateIsRequestAttach: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IATT_UPDATE_IS_REQUEST_ATTACH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIattUpdateIsRequestAttach(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIattUpdateIsRequestAttach.
  /// </summary>
  public SiIattUpdateIsRequestAttach(IContext context, Import import,
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
    if (!ReadInterstateRequest())
    {
      ExitState = "INTERSTATE_REQUEST_NF";

      return;
    }

    if (ReadInterstateRequestAttachment())
    {
      try
      {
        UpdateInterstateRequestAttachment();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
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
    else
    {
      ExitState = "INTERSTATE_REQ_ATTACHMENT_NF";
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
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequestAttachment()
  {
    entities.InterstateRequestAttachment.Populated = false;

    return Read("ReadInterstateRequestAttachment",
      (db, command) =>
      {
        db.SetInt32(
          command, "intHGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetInt32(
          command, "sysGenSeqNbr",
          import.InterstateRequestAttachment.SystemGeneratedSequenceNum);
      },
      (db, reader) =>
      {
        entities.InterstateRequestAttachment.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestAttachment.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateRequestAttachment.ReceivedDate =
          db.GetNullableDate(reader, 2);
        entities.InterstateRequestAttachment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.InterstateRequestAttachment.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.InterstateRequestAttachment.Populated = true;
      });
  }

  private void UpdateInterstateRequestAttachment()
  {
    System.Diagnostics.Debug.Assert(
      entities.InterstateRequestAttachment.Populated);

    var receivedDate = import.InterstateRequestAttachment.ReceivedDate;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.InterstateRequestAttachment.Populated = false;
    Update("UpdateInterstateRequestAttachment",
      (db, command) =>
      {
        db.SetNullableDate(command, "receivedDate", receivedDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "intHGeneratedId",
          entities.InterstateRequestAttachment.IntHGeneratedId);
        db.SetInt32(
          command, "sysGenSeqNbr",
          entities.InterstateRequestAttachment.SystemGeneratedSequenceNum);
      });

    entities.InterstateRequestAttachment.ReceivedDate = receivedDate;
    entities.InterstateRequestAttachment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateRequestAttachment.LastUpdatedBy = lastUpdatedBy;
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

    private InterstateRequest interstateRequest;
    private InterstateRequestAttachment interstateRequestAttachment;
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

    private InterstateRequest interstateRequest;
    private InterstateRequestAttachment interstateRequestAttachment;
  }
#endregion
}
