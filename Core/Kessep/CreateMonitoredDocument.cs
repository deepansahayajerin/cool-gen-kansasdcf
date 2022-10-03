// Program: CREATE_MONITORED_DOCUMENT, ID: 372132953, model: 746.
// Short name: SWE01712
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CREATE_MONITORED_DOCUMENT.
/// </summary>
[Serializable]
public partial class CreateMonitoredDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_MONITORED_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateMonitoredDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateMonitoredDocument.
  /// </summary>
  public CreateMonitoredDocument(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.MonitoredDocument.CreatedBy))
    {
      local.MonitoredDocument.CreatedBy = import.MonitoredDocument.CreatedBy;
    }
    else
    {
      local.MonitoredDocument.CreatedBy = global.UserId;
    }

    if (Lt(local.MonitoredDocument.CreatedTimestamp,
      import.MonitoredDocument.CreatedTimestamp))
    {
      local.MonitoredDocument.CreatedTimestamp =
        import.MonitoredDocument.CreatedTimestamp;
    }
    else
    {
      local.MonitoredDocument.CreatedTimestamp = Now();
    }

    if (!ReadOutgoingDocument())
    {
      ExitState = "OUTGOING_DOCUMENT_NF";

      return;
    }

    try
    {
      CreateMonitoredDocument1();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_MONITORED_DOCUMENT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SP0000_MONITORED_DOC_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateMonitoredDocument1()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var requiredResponseDate = import.MonitoredDocument.RequiredResponseDate;
    var actualResponseDate = import.MonitoredDocument.ActualResponseDate;
    var closureReasonCode = import.MonitoredDocument.ClosureReasonCode ?? "";
    var createdBy = local.MonitoredDocument.CreatedBy;
    var createdTimestamp = local.MonitoredDocument.CreatedTimestamp;
    var infId = entities.OutgoingDocument.InfId;

    entities.MonitoredDocument.Populated = false;
    Update("CreateMonitoredDocument",
      (db, command) =>
      {
        db.SetDate(command, "requiredResponse", requiredResponseDate);
        db.SetNullableDate(command, "actRespDt", actualResponseDate);
        db.SetNullableDate(command, "closureDate", default(DateTime));
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "infId", infId);
      });

    entities.MonitoredDocument.RequiredResponseDate = requiredResponseDate;
    entities.MonitoredDocument.ActualResponseDate = actualResponseDate;
    entities.MonitoredDocument.ClosureReasonCode = closureReasonCode;
    entities.MonitoredDocument.CreatedBy = createdBy;
    entities.MonitoredDocument.CreatedTimestamp = createdTimestamp;
    entities.MonitoredDocument.InfId = infId;
    entities.MonitoredDocument.Populated = true;
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
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 1);
        entities.OutgoingDocument.Populated = true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    private Infrastructure infrastructure;
    private MonitoredDocument monitoredDocument;
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
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    private MonitoredDocument monitoredDocument;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
    private MonitoredDocument monitoredDocument;
  }
#endregion
}
