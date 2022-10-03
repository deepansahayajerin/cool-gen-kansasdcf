// Program: SP_DOC_CREATE_RET_FLD_VAL_TRG, ID: 374535437, model: 746.
// Short name: SWE03116
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DOC_CREATE_RET_FLD_VAL_TRG.
/// </para>
/// <para>
/// Updates the infrastructure record and the associated outgoing_doc.
/// </para>
/// </summary>
[Serializable]
public partial class SpDocCreateRetFldValTrg: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_CREATE_RET_FLD_VAL_TRG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocCreateRetFldValTrg(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocCreateRetFldValTrg.
  /// </summary>
  public SpDocCreateRetFldValTrg(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 09/03/2009	J Huss		CQ# 211		Initial development
    // 						Moved from DDOC screen logic
    // ----------------------------------------------------------------------------
    if (!ReadOutgoingDocument())
    {
      ExitState = "OUTGOING_DOCUMENT_NF";

      return;
    }

    // Verify field values are archived
    if (AsChar(entities.OutgoingDocument.FieldValuesArchiveInd) != 'Y')
    {
      ExitState = "SP0000_FIELD_VALUES_NOT_ARCHIVED";

      return;
    }

    // Check for duplicate request
    if (ReadRetrieveFieldValueTrigger())
    {
      ExitState = "SP0000_FIELD_VAL_REQST_EXISTS";
    }
    else
    {
      // Create new request
      try
      {
        CreateRetrieveFieldValueTrigger();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_FIELD_VAL_REQST_EXISTS";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_TRIGGER_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void CreateRetrieveFieldValueTrigger()
  {
    var archiveDate = entities.OutgoingDocument.FieldValuesArchiveDate;
    var infId = import.Infrastructure.SystemGeneratedIdentifier;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.RetrieveFieldValueTrigger.Populated = false;
    Update("CreateRetrieveFieldValueTrigger",
      (db, command) =>
      {
        db.SetDate(command, "archiveDate", archiveDate);
        db.SetInt32(command, "infId", infId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "srvPrvdLogonId", createdBy);
      });

    entities.RetrieveFieldValueTrigger.ArchiveDate = archiveDate;
    entities.RetrieveFieldValueTrigger.InfId = infId;
    entities.RetrieveFieldValueTrigger.CreatedBy = createdBy;
    entities.RetrieveFieldValueTrigger.CreatedTimestamp = createdTimestamp;
    entities.RetrieveFieldValueTrigger.ServiceProviderLogonId = createdBy;
    entities.RetrieveFieldValueTrigger.Populated = true;
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 1);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 2);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 3);
        entities.OutgoingDocument.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
      });
  }

  private bool ReadRetrieveFieldValueTrigger()
  {
    entities.RetrieveFieldValueTrigger.Populated = false;

    return Read("ReadRetrieveFieldValueTrigger",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.RetrieveFieldValueTrigger.ArchiveDate = db.GetDate(reader, 0);
        entities.RetrieveFieldValueTrigger.InfId = db.GetInt32(reader, 1);
        entities.RetrieveFieldValueTrigger.CreatedBy = db.GetString(reader, 2);
        entities.RetrieveFieldValueTrigger.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.RetrieveFieldValueTrigger.ServiceProviderLogonId =
          db.GetString(reader, 4);
        entities.RetrieveFieldValueTrigger.Populated = true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of RetrieveFieldValueTrigger.
    /// </summary>
    [JsonPropertyName("retrieveFieldValueTrigger")]
    public RetrieveFieldValueTrigger RetrieveFieldValueTrigger
    {
      get => retrieveFieldValueTrigger ??= new();
      set => retrieveFieldValueTrigger = value;
    }

    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private RetrieveFieldValueTrigger retrieveFieldValueTrigger;
  }
#endregion
}
