// Program: SI_IATT_CREATE_IS_REQUEST_ATTACH, ID: 372381422, model: 746.
// Short name: SWE01136
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_IATT_CREATE_IS_REQUEST_ATTACH.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the Interstate Request Attachments.
/// </para>
/// </summary>
[Serializable]
public partial class SiIattCreateIsRequestAttach: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IATT_CREATE_IS_REQUEST_ATTACH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIattCreateIsRequestAttach(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIattCreateIsRequestAttach.
  /// </summary>
  public SiIattCreateIsRequestAttach(IContext context, Import import,
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
    MoveInterstateRequestAttachment(import.InterstateRequestAttachment,
      export.InterstateRequestAttachment);

    if (ReadInterstateRequest())
    {
      if (ReadInterstateRequestAttachment2())
      {
        ExitState = "SI0000_IATT_DUPL_VALUE_EXISTS";

        return;
      }
    }
    else
    {
      ExitState = "INTERSTATE_REQUEST_NF";

      return;
    }

    if (ReadCase())
    {
      if (AsChar(entities.Case1.Status) == 'C')
      {
        ExitState = "SI0000_NO_CSENET_OUT_CLOSED_CASE";

        return;
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadAbsentParent())
    {
      ExitState = "AP_FOR_CASE_NF";

      return;
    }

    ReadInterstateRequestAttachment1();
    export.InterstateRequestAttachment.SystemGeneratedSequenceNum =
      entities.InterstateRequestAttachment.SystemGeneratedSequenceNum + 1;

    try
    {
      CreateInterstateRequestAttachment();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "INTERSTATE_REQ_ATTACHMENT_AE";

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

  private static void MoveInterstateRequestAttachment(
    InterstateRequestAttachment source, InterstateRequestAttachment target)
  {
    target.SentDate = source.SentDate;
    target.RequestDate = source.RequestDate;
    target.ReceivedDate = source.ReceivedDate;
    target.DataTypeCode = source.DataTypeCode;
    target.Note = source.Note;
  }

  private void CreateInterstateRequestAttachment()
  {
    var intHGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var systemGeneratedSequenceNum =
      export.InterstateRequestAttachment.SystemGeneratedSequenceNum;
    var sentDate = import.InterstateRequestAttachment.SentDate;
    var requestDate = import.InterstateRequestAttachment.RequestDate;
    var receivedDate = import.InterstateRequestAttachment.ReceivedDate;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var dataTypeCode = import.InterstateRequestAttachment.DataTypeCode;
    var note = import.InterstateRequestAttachment.Note ?? "";

    entities.InterstateRequestAttachment.Populated = false;
    Update("CreateInterstateRequestAttachment",
      (db, command) =>
      {
        db.SetInt32(command, "intHGeneratedId", intHGeneratedId);
        db.SetInt32(command, "sysGenSeqNbr", systemGeneratedSequenceNum);
        db.SetNullableDate(command, "sentDate", sentDate);
        db.SetNullableDate(command, "requestDate", requestDate);
        db.SetNullableDate(command, "receivedDate", receivedDate);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", lastUpdatedTimestamp);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetNullableString(command, "incompleteInd", "");
        db.SetString(command, "dataTypeCode", dataTypeCode);
        db.SetNullableString(command, "note", note);
      });

    entities.InterstateRequestAttachment.IntHGeneratedId = intHGeneratedId;
    entities.InterstateRequestAttachment.SystemGeneratedSequenceNum =
      systemGeneratedSequenceNum;
    entities.InterstateRequestAttachment.SentDate = sentDate;
    entities.InterstateRequestAttachment.RequestDate = requestDate;
    entities.InterstateRequestAttachment.ReceivedDate = receivedDate;
    entities.InterstateRequestAttachment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateRequestAttachment.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequestAttachment.CreatedTimestamp =
      lastUpdatedTimestamp;
    entities.InterstateRequestAttachment.CreatedBy = lastUpdatedBy;
    entities.InterstateRequestAttachment.DataTypeCode = dataTypeCode;
    entities.InterstateRequestAttachment.Note = note;
    entities.InterstateRequestAttachment.Populated = true;
  }

  private bool ReadAbsentParent()
  {
    entities.AbsentParent.Populated = false;

    return Read("ReadAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
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

  private bool ReadInterstateRequestAttachment1()
  {
    entities.InterstateRequestAttachment.Populated = false;

    return Read("ReadInterstateRequestAttachment1",
      (db, command) =>
      {
        db.SetInt32(
          command, "intHGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequestAttachment.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestAttachment.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateRequestAttachment.SentDate =
          db.GetNullableDate(reader, 2);
        entities.InterstateRequestAttachment.RequestDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequestAttachment.ReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.InterstateRequestAttachment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.InterstateRequestAttachment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestAttachment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.InterstateRequestAttachment.CreatedBy =
          db.GetString(reader, 8);
        entities.InterstateRequestAttachment.DataTypeCode =
          db.GetString(reader, 9);
        entities.InterstateRequestAttachment.Note =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestAttachment.Populated = true;
      });
  }

  private bool ReadInterstateRequestAttachment2()
  {
    entities.InterstateRequestAttachment.Populated = false;

    return Read("ReadInterstateRequestAttachment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "intHGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetString(
          command, "dataTypeCode",
          import.InterstateRequestAttachment.DataTypeCode);
        db.SetNullableDate(
          command, "sentDate",
          import.InterstateRequestAttachment.SentDate.GetValueOrDefault());
        db.SetDate(command, "date", local.NullDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "requestDate",
          import.InterstateRequestAttachment.RequestDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequestAttachment.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestAttachment.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateRequestAttachment.SentDate =
          db.GetNullableDate(reader, 2);
        entities.InterstateRequestAttachment.RequestDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequestAttachment.ReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.InterstateRequestAttachment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.InterstateRequestAttachment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestAttachment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.InterstateRequestAttachment.CreatedBy =
          db.GetString(reader, 8);
        entities.InterstateRequestAttachment.DataTypeCode =
          db.GetString(reader, 9);
        entities.InterstateRequestAttachment.Note =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestAttachment.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet ap;
    private InterstateRequestAttachment interstateRequestAttachment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateRequestAttachment.
    /// </summary>
    [JsonPropertyName("interstateRequestAttachment")]
    public InterstateRequestAttachment InterstateRequestAttachment
    {
      get => interstateRequestAttachment ??= new();
      set => interstateRequestAttachment = value;
    }

    private InterstateRequestAttachment interstateRequestAttachment;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private DateWorkArea nullDate;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private Case1 case1;
    private CaseRole absentParent;
    private CsePerson ap;
    private InterstateRequest interstateRequest;
    private InterstateRequestAttachment interstateRequestAttachment;
  }
#endregion
}
