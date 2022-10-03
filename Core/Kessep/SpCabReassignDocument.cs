// Program: SP_CAB_REASSIGN_DOCUMENT, ID: 370978314, model: 746.
// Short name: SWE02717
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_REASSIGN_DOCUMENT.
/// </summary>
[Serializable]
public partial class SpCabReassignDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_REASSIGN_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReassignDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReassignDocument.
  /// </summary>
  public SpCabReassignDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------
    // Reassign the given document
    // --------------------------------------------------------------------
    // --------------------------------------------------------------------
    // Date		Developer	Request		Desc
    // --------------------------------------------------------------------
    // 08/23/2000	M Ramirez	101309		Initial Development
    // --------------------------------------------------------------------
    if (IsEmpty(import.Infrastructure.LastUpdatedBy))
    {
      local.Infrastructure.LastUpdatedBy = global.UserId;
    }
    else
    {
      local.Infrastructure.LastUpdatedBy =
        import.Infrastructure.LastUpdatedBy ?? "";
    }

    if (!Lt(local.Null1.Timestamp, import.Infrastructure.LastUpdatedTimestamp))
    {
      local.Infrastructure.LastUpdatedTimestamp = Now();
    }
    else
    {
      local.Infrastructure.LastUpdatedTimestamp =
        import.Infrastructure.LastUpdatedTimestamp;
    }

    if (import.Infrastructure.SystemGeneratedIdentifier <= 0)
    {
      return;
    }

    if (IsEmpty(import.NewServiceProvider.UserId))
    {
      return;
    }

    if (import.NewOffice.SystemGeneratedId <= 0)
    {
      return;
    }

    if (IsEmpty(import.NewOfficeServiceProvider.RoleCode))
    {
      return;
    }

    if (!Lt(local.Null1.Date, import.NewOfficeServiceProvider.EffectiveDate))
    {
      return;
    }

    if (IsEmpty(import.OldServiceProvider.UserId))
    {
      return;
    }

    if (import.OldOffice.SystemGeneratedId <= 0)
    {
      return;
    }

    if (IsEmpty(import.OldOfficeServiceProvider.RoleCode))
    {
      return;
    }

    if (!Lt(local.Null1.Date, import.OldOfficeServiceProvider.EffectiveDate))
    {
      return;
    }

    if (!ReadInfrastructure())
    {
      ExitState = "INFRASTRUCTURE_NF";

      return;
    }

    // mjr
    // --------------------------------------
    // Infrastructure must have a document
    // -----------------------------------------
    if (ReadOutgoingDocument())
    {
      local.OutgoingDocument.Assign(entities.OutgoingDocument);
    }
    else
    {
      return;
    }

    // mjr
    // --------------------------------------
    // New SP
    // -----------------------------------------
    if (!Equal(import.OldServiceProvider.UserId,
      import.NewServiceProvider.UserId))
    {
      try
      {
        UpdateInfrastructure();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_INFRASTRUCTURE_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_INFRASTRUCTURE_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // mjr
    // --------------------------------------
    // New office
    // -----------------------------------------
    if (import.OldOffice.SystemGeneratedId != import
      .NewOffice.SystemGeneratedId)
    {
      if (!ReadDocument())
      {
        ExitState = "DOCUMENT_NF";

        return;
      }

      if (!ReadPrinterOutputDestination())
      {
        ExitState = "PRINTER_OUTPUT_DESTINATION_NF";

        return;
      }

      if (ReadMonitoredDocument())
      {
        local.MonitoredDocument.Assign(entities.MonitoredDocument);
        DeleteMonitoredDocument();
      }
      else
      {
        // -------------------------------------------------------
        // There may not be a monitored document
        // -------------------------------------------------------
      }

      local.Group.Index = 0;
      local.Group.Clear();

      foreach(var item in ReadFieldValue())
      {
        if (!ReadField())
        {
          local.Group.Next();

          continue;
        }

        local.Group.Update.Gfield.Name = entities.Field.Name;
        local.Group.Update.GfieldValue.Assign(entities.FieldValue);
        DeleteFieldValue();
        local.Group.Next();
      }

      DeleteOutgoingDocument();

      try
      {
        CreateOutgoingDocument();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OUTGOING_DOCUMENT_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "OUTGOING_DOCUMENT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsEmpty(local.MonitoredDocument.CreatedBy))
      {
        try
        {
          CreateMonitoredDocument();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SP0000_MONITORED_DOCUMENT_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_MONITORED_DOC_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!ReadDocumentField())
        {
          continue;
        }

        try
        {
          CreateFieldValue();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FIELD_VALUE_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FIELD_VALUE_PV";

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

  private void CreateFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var createdBy = local.Group.Item.GfieldValue.CreatedBy;
    var createdTimestamp = local.Group.Item.GfieldValue.CreatedTimestamp;
    var lastUpdatedBy = local.Group.Item.GfieldValue.LastUpdatedBy;
    var lastUpdatdTstamp = local.Group.Item.GfieldValue.LastUpdatdTstamp;
    var value = local.Group.Item.GfieldValue.Value ?? "";
    var fldName = entities.DocumentField.FldName;
    var docName = entities.DocumentField.DocName;
    var docEffectiveDte = entities.DocumentField.DocEffectiveDte;
    var infIdentifier = entities.OutgoingDocument.InfId;

    entities.FieldValue.Populated = false;
    Update("CreateFieldValue",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableString(command, "valu", value);
        db.SetString(command, "fldName", fldName);
        db.SetString(command, "docName", docName);
        db.SetDate(command, "docEffectiveDte", docEffectiveDte);
        db.SetInt32(command, "infIdentifier", infIdentifier);
      });

    entities.FieldValue.CreatedBy = createdBy;
    entities.FieldValue.CreatedTimestamp = createdTimestamp;
    entities.FieldValue.LastUpdatedBy = lastUpdatedBy;
    entities.FieldValue.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.FieldValue.Value = value;
    entities.FieldValue.FldName = fldName;
    entities.FieldValue.DocName = docName;
    entities.FieldValue.DocEffectiveDte = docEffectiveDte;
    entities.FieldValue.InfIdentifier = infIdentifier;
    entities.FieldValue.Populated = true;
  }

  private void CreateMonitoredDocument()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var requiredResponseDate = local.MonitoredDocument.RequiredResponseDate;
    var actualResponseDate = local.MonitoredDocument.ActualResponseDate;
    var closureDate = local.MonitoredDocument.ClosureDate;
    var closureReasonCode = local.MonitoredDocument.ClosureReasonCode ?? "";
    var createdBy = local.MonitoredDocument.CreatedBy;
    var createdTimestamp = local.MonitoredDocument.CreatedTimestamp;
    var lastUpdatedBy = local.MonitoredDocument.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.MonitoredDocument.LastUpdatedTimestamp;
    var infId = entities.OutgoingDocument.InfId;

    entities.MonitoredDocument.Populated = false;
    Update("CreateMonitoredDocument",
      (db, command) =>
      {
        db.SetDate(command, "requiredResponse", requiredResponseDate);
        db.SetNullableDate(command, "actRespDt", actualResponseDate);
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "infId", infId);
      });

    entities.MonitoredDocument.RequiredResponseDate = requiredResponseDate;
    entities.MonitoredDocument.ActualResponseDate = actualResponseDate;
    entities.MonitoredDocument.ClosureDate = closureDate;
    entities.MonitoredDocument.ClosureReasonCode = closureReasonCode;
    entities.MonitoredDocument.CreatedBy = createdBy;
    entities.MonitoredDocument.CreatedTimestamp = createdTimestamp;
    entities.MonitoredDocument.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredDocument.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.MonitoredDocument.InfId = infId;
    entities.MonitoredDocument.Populated = true;
  }

  private void CreateOutgoingDocument()
  {
    var printSucessfulIndicator =
      local.OutgoingDocument.PrintSucessfulIndicator;
    var createdBy = local.OutgoingDocument.CreatedBy;
    var createdTimestamp = local.OutgoingDocument.CreatedTimestamp;
    var podPrinterId = entities.PrinterOutputDestination.PrinterId;
    var lastUpdatedBy = local.OutgoingDocument.LastUpdatedBy ?? "";
    var lastUpdatdTstamp = local.OutgoingDocument.LastUpdatdTstamp;
    var docName = entities.Document.Name;
    var docEffectiveDte = entities.Document.EffectiveDate;
    var fieldValuesArchiveDate = local.OutgoingDocument.FieldValuesArchiveDate;
    var fieldValuesArchiveInd =
      local.OutgoingDocument.FieldValuesArchiveInd ?? "";
    var infId = entities.Infrastructure.SystemGeneratedIdentifier;

    CheckValid<OutgoingDocument>("FieldValuesArchiveInd", fieldValuesArchiveInd);
      
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
        db.SetNullableDate(command, "fieldValArchDt", fieldValuesArchiveDate);
        db.SetNullableString(command, "fieldValArchInd", fieldValuesArchiveInd);
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
    entities.OutgoingDocument.FieldValuesArchiveDate = fieldValuesArchiveDate;
    entities.OutgoingDocument.FieldValuesArchiveInd = fieldValuesArchiveInd;
    entities.OutgoingDocument.InfId = infId;
    entities.OutgoingDocument.Populated = true;
  }

  private void DeleteFieldValue()
  {
    Update("DeleteFieldValue",
      (db, command) =>
      {
        db.SetString(command, "fldName", entities.FieldValue.FldName);
        db.SetString(command, "docName", entities.FieldValue.DocName);
        db.SetDate(
          command, "docEffectiveDte",
          entities.FieldValue.DocEffectiveDte.GetValueOrDefault());
        db.
          SetInt32(command, "infIdentifier", entities.FieldValue.InfIdentifier);
          
      });
  }

  private void DeleteMonitoredDocument()
  {
    Update("DeleteMonitoredDocument",
      (db, command) =>
      {
        db.SetInt32(command, "infId", entities.MonitoredDocument.InfId);
      });
  }

  private void DeleteOutgoingDocument()
  {
    Update("DeleteOutgoingDocument#1",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      });

    Update("DeleteOutgoingDocument#2",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      });

    Update("DeleteOutgoingDocument#3",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      });
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
        entities.Document.EffectiveDate = db.GetDate(reader, 1);
        entities.Document.Populated = true;
      });
  }

  private bool ReadDocumentField()
  {
    entities.DocumentField.Populated = false;

    return Read("ReadDocumentField",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
        db.SetString(command, "fldName", local.Group.Item.Gfield.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.FldName = db.GetString(reader, 1);
        entities.DocumentField.DocName = db.GetString(reader, 2);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 3);
        entities.DocumentField.Populated = true;
      });
  }

  private bool ReadField()
  {
    System.Diagnostics.Debug.Assert(entities.FieldValue.Populated);
    entities.Field.Populated = false;

    return Read("ReadField",
      (db, command) =>
      {
        db.SetString(command, "name1", entities.FieldValue.FldName);
        db.SetDate(
          command, "effectiveDate",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "docEffectiveDte",
          entities.FieldValue.DocEffectiveDte.GetValueOrDefault());
        db.SetString(command, "name2", entities.Document.Name);
        db.SetString(command, "docName", entities.FieldValue.DocName);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    return ReadEach("ReadFieldValue",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.FieldValue.CreatedBy = db.GetString(reader, 0);
        entities.FieldValue.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.FieldValue.LastUpdatedBy = db.GetString(reader, 2);
        entities.FieldValue.LastUpdatdTstamp = db.GetDateTime(reader, 3);
        entities.FieldValue.Value = db.GetNullableString(reader, 4);
        entities.FieldValue.FldName = db.GetString(reader, 5);
        entities.FieldValue.DocName = db.GetString(reader, 6);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 7);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 8);
        entities.FieldValue.Populated = true;

        return true;
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
        entities.Infrastructure.UserId = db.GetString(reader, 1);
        entities.Infrastructure.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadMonitoredDocument()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.MonitoredDocument.Populated = false;

    return Read("ReadMonitoredDocument",
      (db, command) =>
      {
        db.SetInt32(command, "infId", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.MonitoredDocument.RequiredResponseDate = db.GetDate(reader, 0);
        entities.MonitoredDocument.ActualResponseDate =
          db.GetNullableDate(reader, 1);
        entities.MonitoredDocument.ClosureDate = db.GetNullableDate(reader, 2);
        entities.MonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 3);
        entities.MonitoredDocument.CreatedBy = db.GetString(reader, 4);
        entities.MonitoredDocument.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.MonitoredDocument.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.MonitoredDocument.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.MonitoredDocument.InfId = db.GetInt32(reader, 8);
        entities.MonitoredDocument.Populated = true;
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
        entities.OutgoingDocument.CreatedBy = db.GetString(reader, 1);
        entities.OutgoingDocument.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.OutgoingDocument.PodPrinterId =
          db.GetNullableString(reader, 3);
        entities.OutgoingDocument.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 6);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 7);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 8);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 9);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 10);
        entities.OutgoingDocument.Populated = true;
      });
  }

  private bool ReadPrinterOutputDestination()
  {
    entities.PrinterOutputDestination.Populated = false;

    return Read("ReadPrinterOutputDestination",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGenerated", import.NewOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.PrinterOutputDestination.PrinterId = db.GetString(reader, 0);
        entities.PrinterOutputDestination.OffGenerated =
          db.GetNullableInt32(reader, 1);
        entities.PrinterOutputDestination.Populated = true;
      });
  }

  private void UpdateInfrastructure()
  {
    var userId = import.NewServiceProvider.UserId;
    var lastUpdatedBy = local.Infrastructure.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.Infrastructure.LastUpdatedTimestamp;

    entities.Infrastructure.Populated = false;
    Update("UpdateInfrastructure",
      (db, command) =>
      {
        db.SetString(command, "userId", userId);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Infrastructure.SystemGeneratedIdentifier);
      });

    entities.Infrastructure.UserId = userId;
    entities.Infrastructure.LastUpdatedBy = lastUpdatedBy;
    entities.Infrastructure.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Infrastructure.Populated = true;
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
    /// A value of OldServiceProvider.
    /// </summary>
    [JsonPropertyName("oldServiceProvider")]
    public ServiceProvider OldServiceProvider
    {
      get => oldServiceProvider ??= new();
      set => oldServiceProvider = value;
    }

    /// <summary>
    /// A value of OldOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("oldOfficeServiceProvider")]
    public OfficeServiceProvider OldOfficeServiceProvider
    {
      get => oldOfficeServiceProvider ??= new();
      set => oldOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OldOffice.
    /// </summary>
    [JsonPropertyName("oldOffice")]
    public Office OldOffice
    {
      get => oldOffice ??= new();
      set => oldOffice = value;
    }

    /// <summary>
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
    }

    /// <summary>
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of NewOffice.
    /// </summary>
    [JsonPropertyName("newOffice")]
    public Office NewOffice
    {
      get => newOffice ??= new();
      set => newOffice = value;
    }

    private Infrastructure infrastructure;
    private ServiceProvider oldServiceProvider;
    private OfficeServiceProvider oldOfficeServiceProvider;
    private Office oldOffice;
    private ServiceProvider newServiceProvider;
    private OfficeServiceProvider newOfficeServiceProvider;
    private Office newOffice;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Gfield.
      /// </summary>
      [JsonPropertyName("gfield")]
      public Field Gfield
      {
        get => gfield ??= new();
        set => gfield = value;
      }

      /// <summary>
      /// A value of GfieldValue.
      /// </summary>
      [JsonPropertyName("gfieldValue")]
      public FieldValue GfieldValue
      {
        get => gfieldValue ??= new();
        set => gfieldValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Field gfield;
      private FieldValue gfieldValue;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private Array<GroupGroup> group;
    private MonitoredDocument monitoredDocument;
    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
    private DateWorkArea null1;
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
    /// A value of PrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("printerOutputDestination")]
    public PrinterOutputDestination PrinterOutputDestination
    {
      get => printerOutputDestination ??= new();
      set => printerOutputDestination = value;
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
    private PrinterOutputDestination printerOutputDestination;
    private Document document;
    private DocumentField documentField;
    private Field field;
    private FieldValue fieldValue;
    private Infrastructure infrastructure;
    private MonitoredDocument monitoredDocument;
    private OutgoingDocument outgoingDocument;
  }
#endregion
}
