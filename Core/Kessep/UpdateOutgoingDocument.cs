// Program: UPDATE_OUTGOING_DOCUMENT, ID: 372117142, model: 746.
// Short name: SWE01490
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UPDATE_OUTGOING_DOCUMENT.
/// </summary>
[Serializable]
public partial class UpdateOutgoingDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_OUTGOING_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateOutgoingDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateOutgoingDocument.
  /// </summary>
  public UpdateOutgoingDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.OutgoingDocument.LastUpdatedBy))
    {
      local.OutgoingDocument.LastUpdatedBy =
        import.OutgoingDocument.LastUpdatedBy ?? "";
    }
    else
    {
      local.OutgoingDocument.LastUpdatedBy = global.UserId;
    }

    if (Lt(local.OutgoingDocument.LastUpdatdTstamp,
      import.OutgoingDocument.LastUpdatdTstamp))
    {
      local.OutgoingDocument.LastUpdatdTstamp =
        import.OutgoingDocument.LastUpdatdTstamp;
    }
    else
    {
      local.OutgoingDocument.LastUpdatdTstamp = Now();
    }

    if (ReadOutgoingDocument())
    {
      try
      {
        UpdateOutgoingDocument1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OUTGOING_DOCUMENT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "OUTGOING_DOCUMENT_PV";

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
      ExitState = "OUTGOING_DOCUMENT_NF";
    }
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
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 5);
        entities.OutgoingDocument.Populated = true;
      });
  }

  private void UpdateOutgoingDocument1()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var printSucessfulIndicator =
      import.OutgoingDocument.PrintSucessfulIndicator;
    var lastUpdatedBy = local.OutgoingDocument.LastUpdatedBy ?? "";
    var lastUpdatdTstamp = local.OutgoingDocument.LastUpdatdTstamp;

    entities.OutgoingDocument.Populated = false;
    Update("UpdateOutgoingDocument",
      (db, command) =>
      {
        db.SetString(command, "prntSucessfulInd", printSucessfulIndicator);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetInt32(command, "infId", entities.OutgoingDocument.InfId);
      });

    entities.OutgoingDocument.PrintSucessfulIndicator = printSucessfulIndicator;
    entities.OutgoingDocument.LastUpdatedBy = lastUpdatedBy;
    entities.OutgoingDocument.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.OutgoingDocument.Populated = true;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private OutgoingDocument outgoingDocument;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    private OutgoingDocument outgoingDocument;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of PrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("printerOutputDestination")]
    public PrinterOutputDestination PrinterOutputDestination
    {
      get => printerOutputDestination ??= new();
      set => printerOutputDestination = value;
    }

    /// <summary>
    /// A value of ZdelRecordedDocument.
    /// </summary>
    [JsonPropertyName("zdelRecordedDocument")]
    public ZdelRecordedDocument ZdelRecordedDocument
    {
      get => zdelRecordedDocument ??= new();
      set => zdelRecordedDocument = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private OutgoingDocument outgoingDocument;
    private PrinterOutputDestination printerOutputDestination;
    private ZdelRecordedDocument zdelRecordedDocument;
    private Infrastructure infrastructure;
  }
#endregion
}
