// Program: CREATE_OUTGOING_DOCUMENT, ID: 371914269, model: 746.
// Short name: SWE00151
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CREATE_OUTGOING_DOCUMENT.
/// </summary>
[Serializable]
public partial class CreateOutgoingDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_OUTGOING_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateOutgoingDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateOutgoingDocument.
  /// </summary>
  public CreateOutgoingDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------
    // CHANGE LOG:
    // 01/10/00	PMcElderry
    // PR # 80211 - logic to close monitored activity #55.
    // -----------------------------------------------------------------------
    // --------------------------------------------------------------------------------
    // 09/27/2006    J Bahre     PR285545    Replaced
    // Exit State 'infrastructure record not found' with a new Exit State so 
    // problem
    // with error on screen can be located more easily.
    // --------------------------------------------------------------------------------
    if (!IsEmpty(import.OutgoingDocument.CreatedBy))
    {
      local.OutgoingDocument.CreatedBy = import.OutgoingDocument.CreatedBy;
    }
    else
    {
      local.OutgoingDocument.CreatedBy = global.UserId;
    }

    if (Lt(local.OutgoingDocument.CreatedTimestamp,
      import.OutgoingDocument.CreatedTimestamp))
    {
      local.OutgoingDocument.CreatedTimestamp =
        import.OutgoingDocument.CreatedTimestamp;
    }
    else
    {
      local.OutgoingDocument.CreatedTimestamp = Now();
    }

    local.OutgoingDocument.LastUpdatedBy = local.OutgoingDocument.CreatedBy;
    local.OutgoingDocument.LastUpdatdTstamp =
      local.OutgoingDocument.CreatedTimestamp;

    // ----------------------
    // Beg PR # 80211 changes
    // ----------------------
    if (!ReadPrinterOutputDestination())
    {
      ExitState = "PRINTER_OUTPUT_DESTINATION_NF";

      return;
    }

    // ----------------------
    // End PR # 80211 changes
    // ----------------------
    if (!ReadInfrastructure())
    {
      // --------------------------------------------------
      // JLB  PR285545  09/27/2006 Added a new exit state.
      // --------------------------------------------------
      ExitState = "INFRASTRUCTURE_NF_3";

      return;
    }

    // mjr
    // ------------------------------------------------
    // 10/09/1998
    // Add code to read document (for the association)
    // -------------------------------------------------------------
    if (!ReadDocument())
    {
      ExitState = "DOCUMENT_NF";

      return;
    }

    try
    {
      CreateOutgoingDocument1();
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

  private void CreateOutgoingDocument1()
  {
    var printSucessfulIndicator =
      import.OutgoingDocument.PrintSucessfulIndicator;
    var createdBy = local.OutgoingDocument.CreatedBy;
    var createdTimestamp = local.OutgoingDocument.CreatedTimestamp;
    var podPrinterId = entities.PrinterOutputDestination.PrinterId;
    var lastUpdatedBy = local.OutgoingDocument.LastUpdatedBy ?? "";
    var lastUpdatdTstamp = local.OutgoingDocument.LastUpdatdTstamp;
    var docName = entities.Document.Name;
    var docEffectiveDte = entities.Document.EffectiveDate;
    var infId = entities.Infrastructure.SystemGeneratedIdentifier;

    entities.OutgoingDocument.Populated = false;
    Update("CreateOutgoingDocument",
      (db, command) =>
      {
        db.SetString(command, "prntSucessfulInd", printSucessfulIndicator);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "podPrinterId", podPrinterId);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableString(command, "docName", docName);
        db.SetNullableDate(command, "docEffectiveDte", docEffectiveDte);
        db.SetNullableDate(command, "fieldValArchDt", default(DateTime));
        db.SetNullableString(
          command, "fieldValArchInd", GetImplicitValue<OutgoingDocument,
          string>("FieldValuesArchiveInd"));
        db.SetInt32(command, "infId", infId);
      });

    entities.OutgoingDocument.PrintSucessfulIndicator = printSucessfulIndicator;
    entities.OutgoingDocument.CreatedBy = createdBy;
    entities.OutgoingDocument.CreatedTimestamp = createdTimestamp;
    entities.OutgoingDocument.PodPrinterId = podPrinterId;
    entities.OutgoingDocument.LastUpdatedBy = lastUpdatedBy;
    entities.OutgoingDocument.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.OutgoingDocument.DocName = docName;
    entities.OutgoingDocument.DocEffectiveDte = docEffectiveDte;
    entities.OutgoingDocument.InfId = infId;
    entities.OutgoingDocument.Populated = true;
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "name", import.Document.Name);
        db.SetDate(
          command, "effectiveDate",
          import.Document.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.EffectiveDate = db.GetDate(reader, 1);
        entities.Document.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadPrinterOutputDestination()
  {
    entities.PrinterOutputDestination.Populated = false;

    return Read("ReadPrinterOutputDestination",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGenerated", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.PrinterOutputDestination.PrinterId = db.GetString(reader, 0);
        entities.PrinterOutputDestination.OffGenerated =
          db.GetNullableInt32(reader, 1);
        entities.PrinterOutputDestination.Populated = true;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    private Office office;
    private Document document;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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

    private Office office;
    private Document document;
    private ZdelRecordedDocument zdelRecordedDocument;
    private Infrastructure infrastructure;
    private OutgoingDocument outgoingDocument;
    private PrinterOutputDestination printerOutputDestination;
  }
#endregion
}
