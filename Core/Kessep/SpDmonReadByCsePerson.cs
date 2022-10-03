// Program: SP_DMON_READ_BY_CSE_PERSON, ID: 372916951, model: 746.
// Short name: SWE01802
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DMON_READ_BY_CSE_PERSON.
/// </para>
/// <para>
/// This action Diagram was created by Carl Galka. It will read monitored 
/// documents that are for  a specific CASE .
/// </para>
/// </summary>
[Serializable]
public partial class SpDmonReadByCsePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DMON_READ_BY_CSE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDmonReadByCsePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDmonReadByCsePerson.
  /// </summary>
  public SpDmonReadByCsePerson(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------
    // This AB reads monitored documents that are for a specific CSE PERSON
    // -----------------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // -----------------------------------------------------------------------------------
    // 		C Galka				Initial Development
    // 08/23/2000	M Ramirez	101309		DMON is based on OSP not SP
    // 08/31/2000	M Ramirez	none		Changed to improve performance
    // 11/05/2001	GVandy		PR119460	Changed to improve performance
    // -----------------------------------------------------------------------------------
    local.CsePersonsWorkSet.Number =
      import.FilterInfrastructure.CsePersonNumber ?? Spaces(10);
    UseSiReadCsePerson();

    if (!IsEmpty(local.AbendData.Type1))
    {
      local.CsePersonsWorkSet.FormattedName = "**Name NF";
    }

    if (AsChar(import.FilterShowAll.Text1) == 'Y')
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadMonitoredDocumentInfrastructure2())
      {
        MoveInfrastructure(entities.IndexOnly, local.Infrastructure);

        if (!ReadInfrastructure())
        {
          // -- This should never occur.
          export.Export1.Next();

          continue;
        }

        local.LegalAction.Identifier =
          (int)entities.Infrastructure.DenormNumeric12.GetValueOrDefault();

        if (!IsEmpty(import.FilterLegalAction.CourtCaseNumber))
        {
          if (!IsEmpty(import.FilterFipsTribAddress.Country) && !
            Equal(import.FilterFipsTribAddress.Country, "US"))
          {
            if (ReadLegalAction2())
            {
              export.Export1.Update.GxineLegalAction.CourtCaseNumber =
                entities.LegalAction.CourtCaseNumber;
            }
            else
            {
              export.Export1.Next();

              continue;
            }
          }
          else if (ReadLegalAction1())
          {
            export.Export1.Update.GxineLegalAction.CourtCaseNumber =
              entities.LegalAction.CourtCaseNumber;
          }
          else
          {
            export.Export1.Next();

            continue;
          }
        }
        else if (!Equal(entities.Infrastructure.DenormNumeric12, 0))
        {
          if (ReadLegalAction3())
          {
            export.Export1.Update.GxineLegalAction.CourtCaseNumber =
              entities.LegalAction.CourtCaseNumber;
          }
          else
          {
            export.Export1.Update.GxineLegalAction.CourtCaseNumber =
              "**Legal Action NF";
          }
        }

        export.Export1.Update.GxineCsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FormattedName;

        if (ReadDocument())
        {
          MoveDocument(entities.Document, export.Export1.Update.GxineDocument);
        }
        else
        {
          export.Export1.Update.GxineDocument.Name = "**Error";
        }

        // ------------------------------------------------------------------------
        // MOVE ACQUIRED DATA INTO THE GROUP EXPORT VIEW TO BE DISPLAYED
        // ------------------------------------------------------------------------
        export.Export1.Update.GxineCommon.SelectChar = "";
        export.Export1.Update.GxineCodeValue.SelectChar = "";
        export.Export1.Update.GxineMonitoredDocument.Assign(
          entities.MonitoredDocument);
        export.Export1.Update.GxinePrev.Assign(entities.MonitoredDocument);
        export.Export1.Update.GxineInfrastructure.
          Assign(entities.Infrastructure);
        export.Export1.Next();
      }
    }
    else
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadMonitoredDocumentInfrastructure1())
      {
        MoveInfrastructure(entities.IndexOnly, local.Infrastructure);

        if (!ReadInfrastructure())
        {
          // -- This should never occur.
          export.Export1.Next();

          continue;
        }

        local.LegalAction.Identifier =
          (int)entities.Infrastructure.DenormNumeric12.GetValueOrDefault();

        if (!IsEmpty(import.FilterLegalAction.CourtCaseNumber))
        {
          if (!IsEmpty(import.FilterFipsTribAddress.Country) && !
            Equal(import.FilterFipsTribAddress.Country, "US"))
          {
            if (ReadLegalAction2())
            {
              export.Export1.Update.GxineLegalAction.CourtCaseNumber =
                entities.LegalAction.CourtCaseNumber;
            }
            else
            {
              export.Export1.Next();

              continue;
            }
          }
          else if (ReadLegalAction1())
          {
            export.Export1.Update.GxineLegalAction.CourtCaseNumber =
              entities.LegalAction.CourtCaseNumber;
          }
          else
          {
            export.Export1.Next();

            continue;
          }
        }
        else if (!Equal(entities.Infrastructure.DenormNumeric12, 0))
        {
          if (ReadLegalAction3())
          {
            export.Export1.Update.GxineLegalAction.CourtCaseNumber =
              entities.LegalAction.CourtCaseNumber;
          }
          else
          {
            export.Export1.Update.GxineLegalAction.CourtCaseNumber =
              "**Legal Action NF";
          }
        }

        export.Export1.Update.GxineCsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FormattedName;

        if (ReadDocument())
        {
          MoveDocument(entities.Document, export.Export1.Update.GxineDocument);
        }
        else
        {
          export.Export1.Update.GxineDocument.Name = "**Error";
        }

        // ------------------------------------------------------------------------
        // MOVE ACQUIRED DATA INTO THE GROUP EXPORT VIEW TO BE DISPLAYED
        // ------------------------------------------------------------------------
        export.Export1.Update.GxineCommon.SelectChar = "";
        export.Export1.Update.GxineCodeValue.SelectChar = "";
        export.Export1.Update.GxineMonitoredDocument.Assign(
          entities.MonitoredDocument);
        export.Export1.Update.GxinePrev.Assign(entities.MonitoredDocument);
        export.Export1.Update.GxineInfrastructure.
          Assign(entities.Infrastructure);
        export.Export1.Next();
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.UserId = source.UserId;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.AbendData.Type1 = useExport.AbendData.Type1;
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Description = db.GetNullableString(reader, 1);
        entities.Document.EffectiveDate = db.GetDate(reader, 2);
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
          local.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.Infrastructure.CaseUnitNumber = db.GetNullableInt32(reader, 7);
        entities.Infrastructure.UserId = db.GetString(reader, 8);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 9);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 10);
        entities.Infrastructure.Function = db.GetNullableString(reader, 11);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", import.FilterLegalAction.CourtCaseNumber ?? ""
          );
        db.SetString(
          command, "stateAbbreviation", import.FilterFips.StateAbbreviation);
        db.SetNullableString(
          command, "countyAbbr", import.FilterFips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", import.FilterLegalAction.CourtCaseNumber ?? ""
          );
        db.SetNullableString(
          command, "country", import.FilterFipsTribAddress.Country ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMonitoredDocumentInfrastructure1()
  {
    return ReadEach("ReadMonitoredDocumentInfrastructure1",
      (db, command) =>
      {
        db.SetDate(
          command, "requiredResponse",
          import.FilterDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "actRespDt", local.Initialized.Date.GetValueOrDefault());
        db.SetString(command, "userId", import.FilterServiceProvider.UserId);
        db.SetNullableString(
          command, "csePersonNum",
          import.FilterInfrastructure.CsePersonNumber ?? "");
        db.SetInt32(
          command, "systemGeneratedI",
          import.FilterInfrastructure.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "offGenerated", import.FilterOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.MonitoredDocument.RequiredResponseDate = db.GetDate(reader, 0);
        entities.MonitoredDocument.ActualResponseDate =
          db.GetNullableDate(reader, 1);
        entities.MonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 2);
        entities.MonitoredDocument.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.MonitoredDocument.InfId = db.GetInt32(reader, 4);
        entities.IndexOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.IndexOnly.CsePersonNumber = db.GetNullableString(reader, 6);
        entities.IndexOnly.UserId = db.GetString(reader, 7);
        entities.MonitoredDocument.Populated = true;
        entities.IndexOnly.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredDocumentInfrastructure2()
  {
    return ReadEach("ReadMonitoredDocumentInfrastructure2",
      (db, command) =>
      {
        db.SetDate(
          command, "requiredResponse",
          import.FilterDateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "userId", import.FilterServiceProvider.UserId);
        db.SetNullableString(
          command, "csePersonNum",
          import.FilterInfrastructure.CsePersonNumber ?? "");
        db.SetInt32(
          command, "systemGeneratedI",
          import.FilterInfrastructure.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "offGenerated", import.FilterOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.MonitoredDocument.RequiredResponseDate = db.GetDate(reader, 0);
        entities.MonitoredDocument.ActualResponseDate =
          db.GetNullableDate(reader, 1);
        entities.MonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 2);
        entities.MonitoredDocument.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.MonitoredDocument.InfId = db.GetInt32(reader, 4);
        entities.IndexOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.IndexOnly.CsePersonNumber = db.GetNullableString(reader, 6);
        entities.IndexOnly.UserId = db.GetString(reader, 7);
        entities.MonitoredDocument.Populated = true;
        entities.IndexOnly.Populated = true;

        return true;
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
    /// A value of FilterLegalAction.
    /// </summary>
    [JsonPropertyName("filterLegalAction")]
    public LegalAction FilterLegalAction
    {
      get => filterLegalAction ??= new();
      set => filterLegalAction = value;
    }

    /// <summary>
    /// A value of FilterFipsTribAddress.
    /// </summary>
    [JsonPropertyName("filterFipsTribAddress")]
    public FipsTribAddress FilterFipsTribAddress
    {
      get => filterFipsTribAddress ??= new();
      set => filterFipsTribAddress = value;
    }

    /// <summary>
    /// A value of FilterFips.
    /// </summary>
    [JsonPropertyName("filterFips")]
    public Fips FilterFips
    {
      get => filterFips ??= new();
      set => filterFips = value;
    }

    /// <summary>
    /// A value of FilterServiceProvider.
    /// </summary>
    [JsonPropertyName("filterServiceProvider")]
    public ServiceProvider FilterServiceProvider
    {
      get => filterServiceProvider ??= new();
      set => filterServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterDateWorkArea.
    /// </summary>
    [JsonPropertyName("filterDateWorkArea")]
    public DateWorkArea FilterDateWorkArea
    {
      get => filterDateWorkArea ??= new();
      set => filterDateWorkArea = value;
    }

    /// <summary>
    /// A value of FilterInfrastructure.
    /// </summary>
    [JsonPropertyName("filterInfrastructure")]
    public Infrastructure FilterInfrastructure
    {
      get => filterInfrastructure ??= new();
      set => filterInfrastructure = value;
    }

    /// <summary>
    /// A value of FilterShowAll.
    /// </summary>
    [JsonPropertyName("filterShowAll")]
    public WorkArea FilterShowAll
    {
      get => filterShowAll ??= new();
      set => filterShowAll = value;
    }

    /// <summary>
    /// A value of FilterOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("filterOfficeServiceProvider")]
    public OfficeServiceProvider FilterOfficeServiceProvider
    {
      get => filterOfficeServiceProvider ??= new();
      set => filterOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of FilterOffice.
    /// </summary>
    [JsonPropertyName("filterOffice")]
    public Office FilterOffice
    {
      get => filterOffice ??= new();
      set => filterOffice = value;
    }

    private LegalAction filterLegalAction;
    private FipsTribAddress filterFipsTribAddress;
    private Fips filterFips;
    private ServiceProvider filterServiceProvider;
    private DateWorkArea filterDateWorkArea;
    private Infrastructure filterInfrastructure;
    private WorkArea filterShowAll;
    private OfficeServiceProvider filterOfficeServiceProvider;
    private Office filterOffice;
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
      /// A value of GxinePrev.
      /// </summary>
      [JsonPropertyName("gxinePrev")]
      public MonitoredDocument GxinePrev
      {
        get => gxinePrev ??= new();
        set => gxinePrev = value;
      }

      /// <summary>
      /// A value of GxineLegalAction.
      /// </summary>
      [JsonPropertyName("gxineLegalAction")]
      public LegalAction GxineLegalAction
      {
        get => gxineLegalAction ??= new();
        set => gxineLegalAction = value;
      }

      /// <summary>
      /// A value of GxineCodeValue.
      /// </summary>
      [JsonPropertyName("gxineCodeValue")]
      public Common GxineCodeValue
      {
        get => gxineCodeValue ??= new();
        set => gxineCodeValue = value;
      }

      /// <summary>
      /// A value of GxineCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gxineCsePersonsWorkSet")]
      public CsePersonsWorkSet GxineCsePersonsWorkSet
      {
        get => gxineCsePersonsWorkSet ??= new();
        set => gxineCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GxineMonitoredDocument.
      /// </summary>
      [JsonPropertyName("gxineMonitoredDocument")]
      public MonitoredDocument GxineMonitoredDocument
      {
        get => gxineMonitoredDocument ??= new();
        set => gxineMonitoredDocument = value;
      }

      /// <summary>
      /// A value of GxineCommon.
      /// </summary>
      [JsonPropertyName("gxineCommon")]
      public Common GxineCommon
      {
        get => gxineCommon ??= new();
        set => gxineCommon = value;
      }

      /// <summary>
      /// A value of GxineInfrastructure.
      /// </summary>
      [JsonPropertyName("gxineInfrastructure")]
      public Infrastructure GxineInfrastructure
      {
        get => gxineInfrastructure ??= new();
        set => gxineInfrastructure = value;
      }

      /// <summary>
      /// A value of GxineDocument.
      /// </summary>
      [JsonPropertyName("gxineDocument")]
      public Document GxineDocument
      {
        get => gxineDocument ??= new();
        set => gxineDocument = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private MonitoredDocument gxinePrev;
      private LegalAction gxineLegalAction;
      private Common gxineCodeValue;
      private CsePersonsWorkSet gxineCsePersonsWorkSet;
      private MonitoredDocument gxineMonitoredDocument;
      private Common gxineCommon;
      private Infrastructure gxineInfrastructure;
      private Document gxineDocument;
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
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea current;
    private AbendData abendData;
    private DateWorkArea initialized;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of PrinterOutputDestination.
    /// </summary>
    [JsonPropertyName("printerOutputDestination")]
    public PrinterOutputDestination PrinterOutputDestination
    {
      get => printerOutputDestination ??= new();
      set => printerOutputDestination = value;
    }

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
    /// A value of IndexOnly.
    /// </summary>
    [JsonPropertyName("indexOnly")]
    public Infrastructure IndexOnly
    {
      get => indexOnly ??= new();
      set => indexOnly = value;
    }

    private LegalAction legalAction;
    private Infrastructure infrastructure;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private Fips fips;
    private Document document;
    private OutgoingDocument outgoingDocument;
    private MonitoredDocument monitoredDocument;
    private PrinterOutputDestination printerOutputDestination;
    private Office office;
    private Infrastructure indexOnly;
  }
#endregion
}
