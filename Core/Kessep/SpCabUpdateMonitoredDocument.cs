// Program: SP_CAB_UPDATE_MONITORED_DOCUMENT, ID: 372447441, model: 746.
// Short name: SWE02326
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_MONITORED_DOCUMENT.
/// </para>
/// <para>
/// This cab updates a given monitored document.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateMonitoredDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_MONITORED_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateMonitoredDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateMonitoredDocument.
  /// </summary>
  public SpCabUpdateMonitoredDocument(IContext context, Import import,
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
    // mjr
    // ------------------------------------------------------------------
    // Date		Developer	Description
    // ---------------------------------------------------------------------
    // 03/09/1999	M Ramirez	Initial Development
    // ---------------------------------------------------------------------
    if (import.Infrastructure.SystemGeneratedIdentifier <= 0)
    {
      ExitState = "INFRASTRUCTURE_NF";

      return;
    }

    if (!IsEmpty(import.MonitoredDocument.LastUpdatedBy))
    {
      local.MonitoredDocument.LastUpdatedBy =
        import.MonitoredDocument.LastUpdatedBy ?? "";
    }
    else
    {
      local.MonitoredDocument.LastUpdatedBy = global.UserId;
    }

    if (Lt(local.MonitoredDocument.LastUpdatedTimestamp,
      import.MonitoredDocument.LastUpdatedTimestamp))
    {
      local.MonitoredDocument.LastUpdatedTimestamp =
        import.MonitoredDocument.LastUpdatedTimestamp;
    }
    else
    {
      local.MonitoredDocument.LastUpdatedTimestamp = Now();
    }

    if (!ReadMonitoredDocument())
    {
      ExitState = "SP0000_MONITORED_DOCUMENT_NF";

      return;
    }

    try
    {
      UpdateMonitoredDocument();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_MONITORED_DOC_NU";

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

  private bool ReadMonitoredDocument()
  {
    entities.MonitoredDocument.Populated = false;

    return Read("ReadMonitoredDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredDocument.ActualResponseDate =
          db.GetNullableDate(reader, 0);
        entities.MonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 1);
        entities.MonitoredDocument.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.MonitoredDocument.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.MonitoredDocument.InfId = db.GetInt32(reader, 4);
        entities.MonitoredDocument.Populated = true;
      });
  }

  private void UpdateMonitoredDocument()
  {
    System.Diagnostics.Debug.Assert(entities.MonitoredDocument.Populated);

    var actualResponseDate = import.MonitoredDocument.ActualResponseDate;
    var closureReasonCode = import.MonitoredDocument.ClosureReasonCode ?? "";
    var lastUpdatedBy = local.MonitoredDocument.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.MonitoredDocument.LastUpdatedTimestamp;

    entities.MonitoredDocument.Populated = false;
    Update("UpdateMonitoredDocument",
      (db, command) =>
      {
        db.SetNullableDate(command, "actRespDt", actualResponseDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "infId", entities.MonitoredDocument.InfId);
      });

    entities.MonitoredDocument.ActualResponseDate = actualResponseDate;
    entities.MonitoredDocument.ClosureReasonCode = closureReasonCode;
    entities.MonitoredDocument.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredDocument.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.MonitoredDocument.Populated = true;
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
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
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

    private MonitoredDocument monitoredDocument;
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
