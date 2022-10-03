// Program: SP_B715_HOUSEKEEPING, ID: 371339564, model: 746.
// Short name: SWE02241
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B715_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB715Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B715_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB715Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB715Housekeeping.
  /// </summary>
  public SpB715Housekeeping(IContext context, Import import, Export export):
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
    // 01/28/2008	M Ramirez	WR 276, 277	Initial creation
    // ----------------------------------------------------------------------------
    // --------------------------------------------------------------
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // --------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB715";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      export.ProgramProcessingInfo);

    // mjr---> If ppi process_date is null, set it to current date
    if (!Lt(local.Null1.Date, export.ProgramProcessingInfo.ProcessDate))
    {
      export.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    // --------------------------------------------------------------------
    // SET RUNTIME PARAMETERS TO DEFAULTS
    // Debug On defaults to N			(No debug messages)
    // StopTime defaults to Max Timestamp	(Never stops)
    // SleepMinutes defaults to 0		(Never sleeps)
    // --------------------------------------------------------------------
    export.DebugOn.Flag = "N";
    export.Stoptime.Time = new TimeSpan(23, 59, 59);
    export.SleepLength.TotalInteger = 0;

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG=");

      if (local.Position.Count > 0)
      {
        export.DebugOn.Flag =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 6, 1);

        if (IsEmpty(export.DebugOn.Flag))
        {
          export.DebugOn.Flag = "N";
        }
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "STOPTIME=");

      if (local.Position.Count > 0)
      {
        local.BatchTimestampWorkArea.TestTimeHh =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 9, 2);

        if (!Lt(local.BatchTimestampWorkArea.TestTimeHh, "00") && !
          Lt("23", local.BatchTimestampWorkArea.TestTimeHh))
        {
          local.BatchTimestampWorkArea.TextTimeMm =
            Substring(local.ProgramProcessingInfo.ParameterList,
            local.Position.Count + 12, 2);

          if (!Lt(local.BatchTimestampWorkArea.TextTimeMm, "00") && !
            Lt("59", local.BatchTimestampWorkArea.TextTimeMm))
          {
            export.Stoptime.Time = local.Null1.Time;
            export.Stoptime.Time += new TimeSpan((int)StringToNumber(
              local.BatchTimestampWorkArea.TestTimeHh), 0, 0);
            export.Stoptime.Time += new TimeSpan(
              0, (int)StringToNumber(local.BatchTimestampWorkArea.TextTimeMm),
              0);
          }
        }
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "SLEEPMINUTES=");

      if (local.Position.Count > 0)
      {
        local.BatchTimestampWorkArea.TextTimeMm =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 13, 2);

        if (!Lt(local.BatchTimestampWorkArea.TextTimeMm, "00") && !
          Lt("99", local.BatchTimestampWorkArea.TextTimeMm))
        {
          export.SleepLength.TotalInteger =
            StringToNumber(local.BatchTimestampWorkArea.TextTimeMm);
        }
      }
    }

    // --------------------------------------------------------------------
    // StopTime is really a 'Don't start before Time'.
    // Make sure the process Ends (instead of Restarts) if the SleepTime is
    // greater than the difference between the StopTime and the CurrentTime.
    // --------------------------------------------------------------------
    export.Stoptime.Time -= new TimeSpan(
      0, (int)export.SleepLength.TotalInteger, 0);

    // --------------------------------------------------------------------
    // DETERMINE IF THIS IS A RESTART SITUATION
    // --------------------------------------------------------------------
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------
    // OPEN OUTPUT ERROR REPORT 99
    // -------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ------------------------------------------------------------
    // OPEN OUTPUT CONTROL REPORT 98
    // ------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO ERROR REPORT 99
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // Determine document exceptions
    // Determine which type of alert is required for each document
    // -----------------------------------------------------------
    export.Exceptions.Index = -1;
    export.DocumentDetails.Index = -1;

    foreach(var item in ReadDocumentEventDetail())
    {
      // -----------------------------------------------------------
      // Determine which type of alert is required
      // Required Response Days is used to determine if the document is a 
      // Monitored
      // document.
      // Event Detail Exception Routine is used to determine if the document
      // is an AutoDoc.
      // Business Object is used to determine the recipient of AutoDoc alerts.
      // -----------------------------------------------------------
      if (export.DocumentDetails.Index + 1 >= Export
        .DocumentDetailsGroup.Capacity)
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Overflow on Group View for Document Details";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++export.DocumentDetails.Index;
      export.DocumentDetails.CheckSize();

      export.DocumentDetails.Update.GexportDocumentDetailDocument.Assign(
        entities.Document);
      export.DocumentDetails.Update.GexportDocumentDetailEventDetail.
        ExceptionRoutine = entities.EventDetail.ExceptionRoutine;

      // -----------------------------------------------------------
      // Determine document exceptions
      // -----------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList,
        TrimEnd(entities.Document.Name));

      if (local.Position.Count > 0)
      {
        if (export.Exceptions.Index + 1 >= Export.ExceptionsGroup.Capacity)
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Overflow on Group View for Exception Documents";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        ++export.Exceptions.Index;
        export.Exceptions.CheckSize();

        export.Exceptions.Update.GexportException.Name = entities.Document.Name;
      }
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO CONTROL REPORT 98
    // -----------------------------------------------------------
    if (export.Exceptions.Index < 0)
    {
    }
    else
    {
      local.EabReportSend.RptDetail = "Document Exceptions";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "-------------------";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      export.Exceptions.Index = 0;

      for(var limit = export.Exceptions.Count; export.Exceptions.Index < limit; ++
        export.Exceptions.Index)
      {
        if (!export.Exceptions.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Exceptions.Item.GexportException.Name))
        {
          local.EabReportSend.RptDetail =
            export.Exceptions.Item.GexportException.Name;
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }

      export.Exceptions.CheckIndex();
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
    UseSpDocSetLiterals();
    export.Current.Timestamp = Now();

    if (ReadAlert1())
    {
      export.Monitored.TypeCode = "AUT";
      export.Monitored.Description = entities.Alert.Description;
      export.Monitored.Message = entities.Alert.Message;
      export.Monitored.OptimizationInd = "N";
      export.Monitored.OptimizedFlag = "N";
      export.Monitored.LastUpdatedBy = export.ProgramProcessingInfo.Name;
      export.Monitored.DistributionDate =
        export.ProgramProcessingInfo.ProcessDate;
      export.Monitored.PrioritizationCode = 1;
      export.Monitored.LastUpdatedTimestamp = export.Current.Timestamp;
      export.Monitored.SituationIdentifier = "1";
    }
    else
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Alert not found for Control Number 396.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (ReadAlert2())
    {
      export.UnMonitored.TypeCode = "AUT";
      export.UnMonitored.Description = entities.Alert.Description;
      export.UnMonitored.Message = entities.Alert.Message;
      export.UnMonitored.OptimizationInd = "N";
      export.UnMonitored.OptimizedFlag = "N";
      export.UnMonitored.LastUpdatedBy = export.ProgramProcessingInfo.Name;
      export.UnMonitored.DistributionDate =
        export.ProgramProcessingInfo.ProcessDate;
      export.UnMonitored.PrioritizationCode = 1;
      export.UnMonitored.LastUpdatedTimestamp = export.Current.Timestamp;
      export.UnMonitored.SituationIdentifier = "1";
    }
    else
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Alert not found for Control Number 442.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (ReadAlert3())
    {
      export.Automatic.TypeCode = "AUT";
      export.Automatic.Description = entities.Alert.Description;
      export.Automatic.Message = entities.Alert.Message;
      export.Automatic.OptimizationInd = "N";
      export.Automatic.OptimizedFlag = "N";
      export.Automatic.LastUpdatedBy = export.ProgramProcessingInfo.Name;
      export.Automatic.DistributionDate =
        export.ProgramProcessingInfo.ProcessDate;
      export.Automatic.PrioritizationCode = 1;
      export.Automatic.LastUpdatedTimestamp = export.Current.Timestamp;
      export.Automatic.SituationIdentifier = "1";
    }
    else
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Alert not found for Control Number 447.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdAdminActCert = source.IdAdminActCert;
    target.IdAdminAction = source.IdAdminAction;
    target.IdAppointment = source.IdAppointment;
    target.IdBankruptcy = source.IdBankruptcy;
    target.IdContact = source.IdContact;
    target.IdChNumber = source.IdChNumber;
    target.IdDocument = source.IdDocument;
    target.IdGenetic = source.IdGenetic;
    target.IdHealthInsCoverage = source.IdHealthInsCoverage;
    target.IdIncomeSource = source.IdIncomeSource;
    target.IdInfoRequest = source.IdInfoRequest;
    target.IdJail = source.IdJail;
    target.IdMilitary = source.IdMilitary;
    target.IdObligationAdminAction = source.IdObligationAdminAction;
    target.IdObligationType = source.IdObligationType;
    target.IdPrNumber = source.IdPrNumber;
    target.IdResource = source.IdResource;
    target.IdTribunal = source.IdTribunal;
    target.IdWorkerComp = source.IdWorkerComp;
    target.IdWorksheet = source.IdWorksheet;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, export.SpDocLiteral);
  }

  private bool ReadAlert1()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert1",
      null,
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.Populated = true;
      });
  }

  private bool ReadAlert2()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert2",
      null,
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.Populated = true;
      });
  }

  private bool ReadAlert3()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert3",
      null,
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDocumentEventDetail()
  {
    entities.Document.Populated = false;
    entities.EventDetail.Populated = false;

    return ReadEach("ReadDocumentEventDetail",
      null,
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.BusinessObject = db.GetString(reader, 2);
        entities.Document.RequiredResponseDays = db.GetInt32(reader, 3);
        entities.Document.EveNo = db.GetNullableInt32(reader, 4);
        entities.EventDetail.EveNo = db.GetInt32(reader, 4);
        entities.Document.EvdId = db.GetNullableInt32(reader, 5);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.Document.EffectiveDate = db.GetDate(reader, 6);
        entities.Document.VersionNumber = db.GetString(reader, 7);
        entities.EventDetail.ExceptionRoutine = db.GetNullableString(reader, 8);
        entities.Document.Populated = true;
        entities.EventDetail.Populated = true;

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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DocumentDetailsGroup group.</summary>
    [Serializable]
    public class DocumentDetailsGroup
    {
      /// <summary>
      /// A value of GexportDocumentDetailDocument.
      /// </summary>
      [JsonPropertyName("gexportDocumentDetailDocument")]
      public Document GexportDocumentDetailDocument
      {
        get => gexportDocumentDetailDocument ??= new();
        set => gexportDocumentDetailDocument = value;
      }

      /// <summary>
      /// A value of GexportDocumentDetailEventDetail.
      /// </summary>
      [JsonPropertyName("gexportDocumentDetailEventDetail")]
      public EventDetail GexportDocumentDetailEventDetail
      {
        get => gexportDocumentDetailEventDetail ??= new();
        set => gexportDocumentDetailEventDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document gexportDocumentDetailDocument;
      private EventDetail gexportDocumentDetailEventDetail;
    }

    /// <summary>A ExceptionsGroup group.</summary>
    [Serializable]
    public class ExceptionsGroup
    {
      /// <summary>
      /// A value of GexportException.
      /// </summary>
      [JsonPropertyName("gexportException")]
      public Document GexportException
      {
        get => gexportException ??= new();
        set => gexportException = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document gexportException;
    }

    /// <summary>
    /// A value of Automatic.
    /// </summary>
    [JsonPropertyName("automatic")]
    public OfficeServiceProviderAlert Automatic
    {
      get => automatic ??= new();
      set => automatic = value;
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
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

    /// <summary>
    /// A value of Monitored.
    /// </summary>
    [JsonPropertyName("monitored")]
    public OfficeServiceProviderAlert Monitored
    {
      get => monitored ??= new();
      set => monitored = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of SleepLength.
    /// </summary>
    [JsonPropertyName("sleepLength")]
    public Common SleepLength
    {
      get => sleepLength ??= new();
      set => sleepLength = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of Stoptime.
    /// </summary>
    [JsonPropertyName("stoptime")]
    public DateWorkArea Stoptime
    {
      get => stoptime ??= new();
      set => stoptime = value;
    }

    /// <summary>
    /// A value of UnMonitored.
    /// </summary>
    [JsonPropertyName("unMonitored")]
    public OfficeServiceProviderAlert UnMonitored
    {
      get => unMonitored ??= new();
      set => unMonitored = value;
    }

    /// <summary>
    /// Gets a value of DocumentDetails.
    /// </summary>
    [JsonIgnore]
    public Array<DocumentDetailsGroup> DocumentDetails =>
      documentDetails ??= new(DocumentDetailsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DocumentDetails for json serialization.
    /// </summary>
    [JsonPropertyName("documentDetails")]
    [Computed]
    public IList<DocumentDetailsGroup> DocumentDetails_Json
    {
      get => documentDetails;
      set => DocumentDetails.Assign(value);
    }

    /// <summary>
    /// Gets a value of Exceptions.
    /// </summary>
    [JsonIgnore]
    public Array<ExceptionsGroup> Exceptions => exceptions ??= new(
      ExceptionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Exceptions for json serialization.
    /// </summary>
    [JsonPropertyName("exceptions")]
    [Computed]
    public IList<ExceptionsGroup> Exceptions_Json
    {
      get => exceptions;
      set => Exceptions.Assign(value);
    }

    private OfficeServiceProviderAlert automatic;
    private DateWorkArea current;
    private Common debugOn;
    private OfficeServiceProviderAlert monitored;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private Common sleepLength;
    private SpDocLiteral spDocLiteral;
    private DateWorkArea stoptime;
    private OfficeServiceProviderAlert unMonitored;
    private Array<DocumentDetailsGroup> documentDetails;
    private Array<ExceptionsGroup> exceptions;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea null1;
    private Common position;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
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
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    private Alert alert;
    private Document document;
    private EventDetail eventDetail;
  }
#endregion
}
