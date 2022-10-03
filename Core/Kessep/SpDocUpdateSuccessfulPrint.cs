// Program: SP_DOC_UPDATE_SUCCESSFUL_PRINT, ID: 372132411, model: 746.
// Short name: SWE02256
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DOC_UPDATE_SUCCESSFUL_PRINT.
/// </para>
/// <para>
/// This CAB update document infrastructure, outgoing_document, and 
/// monitored_document in the case of a successful print.
/// </para>
/// </summary>
[Serializable]
public partial class SpDocUpdateSuccessfulPrint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_UPDATE_SUCCESSFUL_PRINT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocUpdateSuccessfulPrint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocUpdateSuccessfulPrint.
  /// </summary>
  public SpDocUpdateSuccessfulPrint(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Maintenance Log
    // ----------------------------------------------------------------------------
    // 10/01/1998	M Ramirez		Initial creation
    // 01/04/1999	M Ramirez		Added check for batch document
    // 07/27/1999	M Ramirez		Added denorm identifier attributes
    // 02/08/2000	M Ramirez	84216	Remove B status
    // 03/31/2000	M Ramirez	80211	Update Inf Process_Status to "Q" so
    // 					EP will process.
    // ----------------------------------------------------------------------------
    // ------------------------------------------------------------------------------
    // 09/27/2006	J Bahre		PR285545	Replaced Exit State 'Infrastructure
    // 						record not found' with a new Exit State so problem
    // 						with error on screen can be located more easily.
    // 09/03/2009	JHuss		CQ# 211		Allow additional infrastructure details to be 
    // set
    // 						after successful document generation
    // ------------------------------------------------------------------------------
    if (ReadInfrastructure())
    {
      local.Infrastructure.Assign(entities.Infrastructure);

      if (!IsEmpty(import.Infrastructure.UserId))
      {
        local.Infrastructure.CaseNumber = import.Infrastructure.CaseNumber ?? ""
          ;
        local.Infrastructure.CaseUnitNumber =
          import.Infrastructure.CaseUnitNumber.GetValueOrDefault();
        local.Infrastructure.CsePersonNumber =
          import.Infrastructure.CsePersonNumber ?? "";
        local.Infrastructure.DenormDate = import.Infrastructure.DenormDate;
        local.Infrastructure.DenormText12 =
          import.Infrastructure.DenormText12 ?? "";
        local.Infrastructure.DenormTimestamp =
          import.Infrastructure.DenormTimestamp;
        local.Infrastructure.DenormNumeric12 =
          import.Infrastructure.DenormNumeric12.GetValueOrDefault();
        local.Infrastructure.UserId = import.Infrastructure.UserId;
      }

      if (!IsEmpty(import.Infrastructure.LastUpdatedBy))
      {
        local.Infrastructure.LastUpdatedBy =
          import.Infrastructure.LastUpdatedBy ?? "";
        local.Batch.Flag = "Y";
      }
      else
      {
        local.Infrastructure.LastUpdatedBy = global.UserId;
      }

      if (Lt(local.Null1.Timestamp, import.Infrastructure.LastUpdatedTimestamp))
      {
        local.Infrastructure.LastUpdatedTimestamp =
          import.Infrastructure.LastUpdatedTimestamp;
        local.Batch.Flag = "Y";
      }
      else
      {
        local.Infrastructure.LastUpdatedTimestamp = Now();
      }
    }
    else
    {
      // -----------------------------------------------
      // JLB pr285545 09/27/2006 Added a new exit state.
      // ------------------------------------------------
      ExitState = "INFRASTRUCTURE_NF_5";

      return;
    }

    if (!ReadOutgoingDocument())
    {
      ExitState = "OUTGOING_DOCUMENT_NF";

      return;
    }

    // JHuss	CQ# 211	Allow additional infrastructure details to be set
    // 		after successful document generation
    if (ReadDocument())
    {
      local.EmployerName.Text33 = "";
      local.EmployerLocation.Text30 = "";

      if (Equal(entities.Document.Name, "ORDIWO2") || Equal
        (entities.Document.Name, "ORDIWO2A"))
      {
        // JHuss	Retrieve name of employer
        if (ReadFieldValue4())
        {
          local.EmployerName.Text33 = TrimEnd(entities.FieldValue.Value);
        }

        // JHuss	Retrieve location of employer
        if (ReadFieldValue3())
        {
          local.EmployerLocation.Text30 = TrimEnd(entities.FieldValue.Value);
        }

        // JHuss	Set infrastructure detail field to the name and location of
        // 	the employer that the ORDIWO2 was generated for
        local.Infrastructure.Detail = "EMP: " + TrimEnd
          (local.EmployerName.Text33) + "; LOC: " + TrimEnd
          (local.EmployerLocation.Text30);
      }
      else if (Equal(entities.Document.Name, "MWONOTHC") || Equal
        (entities.Document.Name, "MWONOHCA"))
      {
        // JHuss	Retrieve name of employer
        if (ReadFieldValue2())
        {
          local.EmployerName.Text33 = TrimEnd(entities.FieldValue.Value);
        }

        // JHuss	Retrieve location of employer
        if (ReadFieldValue1())
        {
          local.EmployerLocation.Text30 = TrimEnd(entities.FieldValue.Value);
        }

        // JHuss	Set infrastructure detail field to the name and location of
        // 	the employer that the MWONOTHC was generated for
        local.Infrastructure.Detail = "EMP: " + TrimEnd
          (local.EmployerName.Text33) + "; LOC: " + TrimEnd
          (local.EmployerLocation.Text30);
      }
      else
      {
        local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);
      }
    }
    else
    {
      ExitState = "DOCUMENT_NF";

      return;
    }

    // Infrastructure
    local.Infrastructure.ProcessStatus = "Q";
    UseSpCabUpdateInfrastructure();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // Outgoing document
    local.OutgoingDocument.PrintSucessfulIndicator = "Y";
    local.OutgoingDocument.LastUpdatedBy =
      local.Infrastructure.LastUpdatedBy ?? "";
    local.OutgoingDocument.LastUpdatdTstamp =
      local.Infrastructure.LastUpdatedTimestamp;
    UseUpdateOutgoingDocument();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // Monitored Document
    if (entities.Document.RequiredResponseDays > 0)
    {
      // mjr
      // ------------------------------------------------
      // 10/30/1998
      // Monitored document is assigned to the service provider
      // that is designated in the infrastructure user_id
      // -------------------------------------------------------------
      local.MonitoredDocument.RequiredResponseDate =
        AddDays(entities.Infrastructure.ReferenceDate,
        entities.Document.RequiredResponseDays);
      local.MonitoredDocument.CreatedBy =
        local.Infrastructure.LastUpdatedBy ?? Spaces(8);
      local.MonitoredDocument.CreatedTimestamp =
        local.Infrastructure.LastUpdatedTimestamp;
      UseCreateMonitoredDocument();
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveMonitoredDocument(MonitoredDocument source,
    MonitoredDocument target)
  {
    target.RequiredResponseDate = source.RequiredResponseDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCreateMonitoredDocument()
  {
    var useImport = new CreateMonitoredDocument.Import();
    var useExport = new CreateMonitoredDocument.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    MoveMonitoredDocument(local.MonitoredDocument, useImport.MonitoredDocument);

    Call(CreateMonitoredDocument.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateInfrastructure()
  {
    var useImport = new SpCabUpdateInfrastructure.Import();
    var useExport = new SpCabUpdateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabUpdateInfrastructure.Execute, useImport, useExport);
  }

  private void UseUpdateOutgoingDocument()
  {
    var useImport = new UpdateOutgoingDocument.Import();
    var useExport = new UpdateOutgoingDocument.Export();

    useImport.OutgoingDocument.Assign(local.OutgoingDocument);
    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;

    Call(UpdateOutgoingDocument.Execute, useImport, useExport);
  }

  private bool ReadDocument()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.OutgoingDocument.DocEffectiveDte.GetValueOrDefault());
        db.SetString(command, "name", entities.OutgoingDocument.DocName ?? "");
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.Description = db.GetNullableString(reader, 2);
        entities.Document.RequiredResponseDays = db.GetInt32(reader, 3);
        entities.Document.EffectiveDate = db.GetDate(reader, 4);
        entities.Document.PrintPreviewSwitch = db.GetString(reader, 5);
        entities.Document.VersionNumber = db.GetString(reader, 6);
        entities.Document.Populated = true;
      });
  }

  private bool ReadFieldValue1()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue1",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadFieldValue2()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue2",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadFieldValue3()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue3",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadFieldValue4()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue4",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
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
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 4);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 5);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 6);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 7);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 9);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 10);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 11);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 12);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 13);
        entities.Infrastructure.UserId = db.GetString(reader, 14);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 17);
        entities.Infrastructure.Function = db.GetNullableString(reader, 18);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 19);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 1);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 2);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 4);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 5);
        entities.OutgoingDocument.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
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
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of EmployerLocation.
    /// </summary>
    [JsonPropertyName("employerLocation")]
    public TextWorkArea EmployerLocation
    {
      get => employerLocation ??= new();
      set => employerLocation = value;
    }

    /// <summary>
    /// A value of EmployerName.
    /// </summary>
    [JsonPropertyName("employerName")]
    public WorkArea EmployerName
    {
      get => employerName ??= new();
      set => employerName = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private Common batch;
    private TextWorkArea employerLocation;
    private WorkArea employerName;
    private DateWorkArea null1;
    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    private OutgoingDocument outgoingDocument;
    private Document document;
    private Infrastructure infrastructure;
    private FieldValue fieldValue;
    private DocumentField documentField;
    private Field field;
  }
#endregion
}
