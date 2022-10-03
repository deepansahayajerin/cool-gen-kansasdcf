// Program: SI_IATT_DELETE_IS_REQUEST_ATTACH, ID: 372381437, model: 746.
// Short name: SWE02303
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_IATT_DELETE_IS_REQUEST_ATTACH.
/// </summary>
[Serializable]
public partial class SiIattDeleteIsRequestAttach: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IATT_DELETE_IS_REQUEST_ATTACH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIattDeleteIsRequestAttach(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIattDeleteIsRequestAttach.
  /// </summary>
  public SiIattDeleteIsRequestAttach(IContext context, Import import,
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
    // 02/08/1999	  Carl Ott		Initial Development
    // ------------------------------------------------------------
    // ****************************************************************
    // This section performs validation that these attachments are available for
    // deletion.
    // ****************************************************************
    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Details.Assign(import.Import1.Item.Details);
      export.Export1.Update.Select.SelectChar =
        import.Import1.Item.Select.SelectChar;

      if (ReadInterstateRequestAttachmentInterstateRequest())
      {
        if (ReadInterstateCase())
        {
          if (AsChar(import.Import1.Item.Select.SelectChar) == 'S')
          {
            if (entities.InterstateCase.SentDate != null)
            {
              export.Export1.Update.Select.SelectChar = "E";
              ExitState = "SI0000_ATTACHMENT_DELETE_INVALID";
            }
          }
          else
          {
          }
        }
        else
        {
          ExitState = "INTERSTATE_CASE_NF";
        }
      }
      else
      {
        ExitState = "INTERSTATE_REQUEST_NF";
      }

      export.Export1.Next();
    }

    // ****************************************************************
    // This section performs the deletion of attachments.
    // ****************************************************************
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Changes.Index = -1;

      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.Details.Assign(import.Import1.Item.Details);
        export.Export1.Update.Select.SelectChar =
          import.Import1.Item.Select.SelectChar;

        if (AsChar(import.Import1.Item.Select.SelectChar) == 'S')
        {
          if (ReadInterstateRequestAttachmentInterstateRequest())
          {
            if (Equal(entities.InterstateRequestHistory.CreatedTimestamp,
              local.InterstateRequestHistory.CreatedTimestamp))
            {
            }
            else
            {
              ++local.Changes.Index;
              local.Changes.CheckSize();

              local.Changes.Update.IdentifiersInterstateRequestHistory.Assign(
                entities.InterstateRequestHistory);
              local.Changes.Update.IdentifiersInterstateRequest.
                IntHGeneratedId = entities.InterstateRequest.IntHGeneratedId;

              if (entities.InterstateRequestAttachment.RequestDate != null)
              {
                local.Changes.Update.Update.Note =
                  "PLEASE SEND THE REQUESTED DOCUMENTS :";
              }
              else if (entities.InterstateRequestAttachment.SentDate != null)
              {
                local.Changes.Update.Update.Note =
                  "THE REQUESTED/REQUIRED DOCUMENTS WERE SENT :";
              }
            }

            DeleteInterstateRequestAttachment();
            ExitState = "SI0000_IATT_DELETE_SUCCESSFUL";
            local.InterstateRequestHistory.CreatedTimestamp =
              entities.InterstateRequestHistory.CreatedTimestamp;
          }
          else
          {
            ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";
            export.Export1.Next();

            return;
          }
        }

        export.Export1.Next();
      }

      // ****************************************************************
      // This section rebuilds the Interstate Request History, Interstate 
      // Request Attachment and Interstate Miscellaneous after the deletions.
      // ****************************************************************
      for(local.Changes.Index = 0; local.Changes.Index < local.Changes.Count; ++
        local.Changes.Index)
      {
        if (!local.Changes.CheckSize())
        {
          break;
        }

        local.RemainingNotes.Count = 0;
        local.Update.Note = local.Changes.Item.Update.Note ?? "";

        foreach(var item in ReadInterstateRequestAttachment())
        {
          local.Update.Note = (local.Update.Note ?? "") + entities
            .InterstateRequestAttachment.Note + ";";
          ++local.RemainingNotes.Count;
        }

        if (local.RemainingNotes.Count == 0)
        {
          local.Update.Note = Spaces(InterstateRequestHistory.Note_MaxLength);
        }

        if (ReadInterstateRequestHistory())
        {
          try
          {
            UpdateInterstateRequestHistory();
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
        else
        {
          ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

          return;
        }

        if (ReadInterstateMiscellaneous())
        {
          if (local.RemainingNotes.Count == 0)
          {
            DeleteInterstateMiscellaneous();

            if (ReadCsenetTransactionEnvelop())
            {
              try
              {
                UpdateCsenetTransactionEnvelop();
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
            else
            {
              ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

              return;
            }
          }
          else
          {
            local.NoteLength.Count = Length(local.Update.Note);

            if (local.NoteLength.Count < 80)
            {
              local.InterstateMiscellaneous.InformationTextLine1 =
                TrimEnd(local.Update.Note);
            }
            else if (local.NoteLength.Count < 160)
            {
              local.InterstateMiscellaneous.InformationTextLine1 =
                Substring(local.Update.Note, 1, 80);
              local.InterstateMiscellaneous.InformationTextLine2 =
                Substring(local.Update.Note, 81, 80);
            }
            else if (local.NoteLength.Count < 240)
            {
              local.InterstateMiscellaneous.InformationTextLine1 =
                Substring(local.Update.Note, 1, 80);
              local.InterstateMiscellaneous.InformationTextLine2 =
                Substring(local.Update.Note, 81, 80);
              local.InterstateMiscellaneous.InformationTextLine3 =
                Substring(local.Update.Note, 161, 80);
            }
            else if (local.NoteLength.Count < 320)
            {
            }
            else
            {
            }

            try
            {
              UpdateInterstateMiscellaneous();
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
        else
        {
          ExitState = "ACO_RE0000_DB_INTEGRITY_ERROR";

          return;
        }
      }

      local.Changes.CheckIndex();
    }
  }

  private void DeleteInterstateMiscellaneous()
  {
    Update("DeleteInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.InterstateMiscellaneous.CcaTransSerNum);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateMiscellaneous.CcaTransactionDt.
            GetValueOrDefault());
      });
  }

  private void DeleteInterstateRequestAttachment()
  {
    Update("DeleteInterstateRequestAttachment",
      (db, command) =>
      {
        db.SetInt32(
          command, "intHGeneratedId",
          entities.InterstateRequestAttachment.IntHGeneratedId);
        db.SetInt32(
          command, "sysGenSeqNbr",
          entities.InterstateRequestAttachment.SystemGeneratedSequenceNum);
      });
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.InterstateRequestHistory.TransactionSerialNum);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateRequestHistory.TransactionDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr",
          entities.InterstateRequestHistory.TransactionSerialNum);
        db.SetDate(
          command, "transactionDate",
          entities.InterstateRequestHistory.TransactionDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 2);
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateMiscellaneous()
  {
    entities.InterstateMiscellaneous.Populated = false;

    return Read("ReadInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.InterstateRequestHistory.TransactionSerialNum);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateRequestHistory.TransactionDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateMiscellaneous.InformationTextLine1 =
          db.GetNullableString(reader, 0);
        entities.InterstateMiscellaneous.InformationTextLine2 =
          db.GetNullableString(reader, 1);
        entities.InterstateMiscellaneous.InformationTextLine3 =
          db.GetNullableString(reader, 2);
        entities.InterstateMiscellaneous.CcaTransSerNum =
          db.GetInt64(reader, 3);
        entities.InterstateMiscellaneous.CcaTransactionDt =
          db.GetDate(reader, 4);
        entities.InterstateMiscellaneous.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestAttachment()
  {
    entities.InterstateRequestAttachment.Populated = false;

    return ReadEach("ReadInterstateRequestAttachment",
      (db, command) =>
      {
        db.SetInt32(
          command, "intHGeneratedId",
          local.Changes.Item.IdentifiersInterstateRequest.IntHGeneratedId);
        db.SetDateTime(
          command, "createdTimestamp",
          local.Changes.Item.IdentifiersInterstateRequestHistory.
            CreatedTimestamp.GetValueOrDefault());
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
        entities.InterstateRequestAttachment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.InterstateRequestAttachment.DataTypeCode =
          db.GetString(reader, 5);
        entities.InterstateRequestAttachment.Note =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestAttachment.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateRequestAttachmentInterstateRequest()
  {
    entities.InterstateRequestAttachment.Populated = false;
    entities.InterstateRequestHistory.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequestAttachmentInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", import.Case1.Number);
        db.SetString(
          command, "dataTypeCode", import.Import1.Item.Details.DataTypeCode);
        db.SetInt32(
          command, "sysGenSeqNbr",
          import.Import1.Item.Details.SystemGeneratedSequenceNum);
      },
      (db, reader) =>
      {
        entities.InterstateRequestAttachment.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestAttachment.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateRequestAttachment.SentDate =
          db.GetNullableDate(reader, 2);
        entities.InterstateRequestAttachment.RequestDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequestAttachment.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.InterstateRequestAttachment.DataTypeCode =
          db.GetString(reader, 5);
        entities.InterstateRequestAttachment.Note =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 7);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 8);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 9);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 10);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 12);
        entities.InterstateRequestAttachment.Populated = true;
        entities.InterstateRequestHistory.Populated = true;
        entities.InterstateRequest.Populated = true;
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
          local.Changes.Item.IdentifiersInterstateRequest.IntHGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          local.Changes.Item.IdentifiersInterstateRequestHistory.
            CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 2);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 4);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 5);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 6);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private void UpdateCsenetTransactionEnvelop()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var processingStatusCode = "P";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "processingStatus", processingStatusCode);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.CsenetTransactionEnvelop.CcaTransactionDt.
            GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.CsenetTransactionEnvelop.CcaTransSerNum);
      });

    entities.CsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.CsenetTransactionEnvelop.Populated = true;
  }

  private void UpdateInterstateMiscellaneous()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateMiscellaneous.Populated);

    var informationTextLine1 =
      local.InterstateMiscellaneous.InformationTextLine1 ?? "";
    var informationTextLine2 =
      local.InterstateMiscellaneous.InformationTextLine2 ?? "";
    var informationTextLine3 =
      local.InterstateMiscellaneous.InformationTextLine3 ?? "";

    entities.InterstateMiscellaneous.Populated = false;
    Update("UpdateInterstateMiscellaneous",
      (db, command) =>
      {
        db.SetNullableString(command, "infoText1", informationTextLine1);
        db.SetNullableString(command, "infoText2", informationTextLine2);
        db.SetNullableString(command, "infoText3", informationTextLine3);
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.InterstateMiscellaneous.CcaTransSerNum);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateMiscellaneous.CcaTransactionDt.
            GetValueOrDefault());
      });

    entities.InterstateMiscellaneous.InformationTextLine1 =
      informationTextLine1;
    entities.InterstateMiscellaneous.InformationTextLine2 =
      informationTextLine2;
    entities.InterstateMiscellaneous.InformationTextLine3 =
      informationTextLine3;
    entities.InterstateMiscellaneous.Populated = true;
  }

  private void UpdateInterstateRequestHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.InterstateRequestHistory.Populated);

    var note = local.Update.Note ?? "";

    entities.InterstateRequestHistory.Populated = false;
    Update("UpdateInterstateRequestHistory",
      (db, command) =>
      {
        db.SetNullableString(command, "note", note);
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequestHistory.IntGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          entities.InterstateRequestHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.InterstateRequestHistory.Note = note;
    entities.InterstateRequestHistory.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public InterstateRequestAttachment Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common select;
      private InterstateRequestAttachment details;
    }

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
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private Case1 case1;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of Details.
      /// </summary>
      [JsonPropertyName("details")]
      public InterstateRequestAttachment Details
      {
        get => details ??= new();
        set => details = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common select;
      private InterstateRequestAttachment details;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ChangesGroup group.</summary>
    [Serializable]
    public class ChangesGroup
    {
      /// <summary>
      /// A value of Update.
      /// </summary>
      [JsonPropertyName("update")]
      public InterstateRequestAttachment Update
      {
        get => update ??= new();
        set => update = value;
      }

      /// <summary>
      /// A value of IdentifiersInterstateRequest.
      /// </summary>
      [JsonPropertyName("identifiersInterstateRequest")]
      public InterstateRequest IdentifiersInterstateRequest
      {
        get => identifiersInterstateRequest ??= new();
        set => identifiersInterstateRequest = value;
      }

      /// <summary>
      /// A value of IdentifiersInterstateRequestHistory.
      /// </summary>
      [JsonPropertyName("identifiersInterstateRequestHistory")]
      public InterstateRequestHistory IdentifiersInterstateRequestHistory
      {
        get => identifiersInterstateRequestHistory ??= new();
        set => identifiersInterstateRequestHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private InterstateRequestAttachment update;
      private InterstateRequest identifiersInterstateRequest;
      private InterstateRequestHistory identifiersInterstateRequestHistory;
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

    /// <summary>
    /// A value of RemainingNotes.
    /// </summary>
    [JsonPropertyName("remainingNotes")]
    public Common RemainingNotes
    {
      get => remainingNotes ??= new();
      set => remainingNotes = value;
    }

    /// <summary>
    /// A value of NoteLength.
    /// </summary>
    [JsonPropertyName("noteLength")]
    public Common NoteLength
    {
      get => noteLength ??= new();
      set => noteLength = value;
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// Gets a value of Changes.
    /// </summary>
    [JsonIgnore]
    public Array<ChangesGroup> Changes => changes ??= new(
      ChangesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Changes for json serialization.
    /// </summary>
    [JsonPropertyName("changes")]
    [Computed]
    public IList<ChangesGroup> Changes_Json
    {
      get => changes;
      set => Changes.Assign(value);
    }

    /// <summary>
    /// A value of NoteUpdated.
    /// </summary>
    [JsonPropertyName("noteUpdated")]
    public Common NoteUpdated
    {
      get => noteUpdated ??= new();
      set => noteUpdated = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public InterstateRequestHistory Update
    {
      get => update ??= new();
      set => update = value;
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
    private Common remainingNotes;
    private Common noteLength;
    private InterstateMiscellaneous interstateMiscellaneous;
    private Array<ChangesGroup> changes;
    private Common noteUpdated;
    private InterstateRequestHistory update;
    private InterstateRequest interstateRequest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

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
    /// A value of InterstateRequestAttachment.
    /// </summary>
    [JsonPropertyName("interstateRequestAttachment")]
    public InterstateRequestAttachment InterstateRequestAttachment
    {
      get => interstateRequestAttachment ??= new();
      set => interstateRequestAttachment = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateMiscellaneous interstateMiscellaneous;
    private Case1 case1;
    private InterstateRequestAttachment interstateRequestAttachment;
    private InterstateCase interstateCase;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
  }
#endregion
}
