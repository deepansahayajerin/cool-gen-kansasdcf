// Program: SP_B309_HOUSEKEEPING, ID: 370941758, model: 746.
// Short name: SWE02718
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B309_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB309Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B309_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB309Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB309Housekeeping.
  /// </summary>
  public SpB309Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // --------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB309";
    export.Current.Timestamp = Now();
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
    // --------------------------------------------------------------------
    export.DebugOn.Flag = "N";

    // --------------------------------------------------------------------
    // EXTRACT RUNTIME PARAMETERS FROM PPI
    // --------------------------------------------------------------------
    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

      if (local.Position.Count > 0)
      {
        export.DebugOn.Flag = "Y";
      }
    }

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

    // --------------------------------------------------------------------
    // SET RESTART PARAMETERS TO DEFAULTS
    // --------------------------------------------------------------------
    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // --------------------------------------------------------------------
      // EXTRACT RESTART PARAMETERS FROM RESTART_INFO
      // --------------------------------------------------------------------
    }

    // -------------------------------------------------------
    // OPEN OUTPUT ERROR REPORT 99
    // -------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

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

    // ------------------------------------------------------------
    // OPEN ARCHIVE FILE
    // ------------------------------------------------------------
    local.External.FileInstruction = "OPEN";
    UseSpEabRetrieveFieldValues();

    if (!IsEmpty(local.External.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error Opening External File : " + NumberToString
        (local.External.NumericReturnCode, 14, 2);
      UseCabErrorReport1();
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO ERROR REPORT 99
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "Archived date       Inf Ind         Result";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO CONTROL REPORT 98
    // -----------------------------------------------------------
    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "R U N   T I M E   P A R A M E T E R S";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "Parameter List : " + (
      local.ProgramProcessingInfo.ParameterList ?? "");
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.DebugOn.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "     DEBUG:  ON";
    }
    else
    {
      local.EabReportSend.RptDetail = "     DEBUG:  OFF";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
    export.EndOfFile.Flag = "N";

    if (ReadAlert1())
    {
      export.Export416.TypeCode = "AUT";
      export.Export416.SituationIdentifier = "416";
      export.Export416.PrioritizationCode = 1;
      export.Export416.OptimizedFlag = "N";
      export.Export416.Message = entities.Alert.Message;
      export.Export416.OptimizationInd = "N";
      export.Export416.DistributionDate =
        export.ProgramProcessingInfo.ProcessDate;
      export.Export416.LastUpdatedBy = export.ProgramProcessingInfo.Name;
      export.Export416.LastUpdatedTimestamp = export.Current.Timestamp;
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Alert not found; Alert Control Number = 416";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    if (ReadAlert2())
    {
      export.Export417.TypeCode = "AUT";
      export.Export417.SituationIdentifier = "417";
      export.Export417.PrioritizationCode = 1;
      export.Export417.OptimizedFlag = "N";
      export.Export417.Message = entities.Alert.Message;
      export.Export417.OptimizationInd = "N";
      export.Export417.DistributionDate =
        export.ProgramProcessingInfo.ProcessDate;
      export.Export417.LastUpdatedBy = export.ProgramProcessingInfo.Name;
      export.Export417.LastUpdatedTimestamp = export.Current.Timestamp;
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Alert not found; Alert Control Number = 417";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    if (!ReadEvent())
    {
      local.EabReportSend.RptDetail =
        "Event not found; Event Control Number = 999";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    if (!ReadEventDetail())
    {
      local.EabReportSend.RptDetail =
        "Event Detail not found; Event Control Number = 999, Event Detail Reason Code = FLDVALRET";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    local.Infrastructure.SystemGeneratedIdentifier = 0;
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.ProcessStatus = "P";
    local.Infrastructure.EventId = entities.Event1.ControlNumber;
    local.Infrastructure.ReasonCode = entities.EventDetail.ReasonCode;
    local.Infrastructure.BusinessObjectCd = entities.Event1.BusinessObjectCode;
    local.Infrastructure.InitiatingStateCode =
      entities.EventDetail.InitiatingStateCode;
    local.Infrastructure.CsenetInOutCode = entities.EventDetail.CsenetInOutCode;
    local.Infrastructure.UserId = export.ProgramProcessingInfo.Name;
    local.Infrastructure.CreatedBy = export.ProgramProcessingInfo.Name;
    local.Infrastructure.CreatedTimestamp = export.Current.Timestamp;
    local.Infrastructure.LastUpdatedBy = export.ProgramProcessingInfo.Name;
    local.Infrastructure.LastUpdatedTimestamp = export.Current.Timestamp;
    local.Infrastructure.ReferenceDate =
      export.ProgramProcessingInfo.ProcessDate;
    local.Infrastructure.Detail =
      "Retrieval of requested document field values";
    UseSpCabCreateInfrastructure();

    if (export.Infrastructure.SystemGeneratedIdentifier == 0)
    {
      local.EabReportSend.RptDetail =
        "Unable to create Infrastructure; Restart job";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
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

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    export.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
  }

  private void UseSpEabRetrieveFieldValues()
  {
    var useImport = new SpEabRetrieveFieldValues.Import();
    var useExport = new SpEabRetrieveFieldValues.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    MoveExternal(local.External, useExport.External);

    Call(SpEabRetrieveFieldValues.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
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
        entities.Alert.ExternalIndicator = db.GetString(reader, 4);
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
        entities.Alert.ExternalIndicator = db.GetString(reader, 4);
        entities.Alert.Populated = true;
      });
  }

  private bool ReadEvent()
  {
    entities.Event1.Populated = false;

    return Read("ReadEvent",
      null,
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.Event1.Name = db.GetString(reader, 1);
        entities.Event1.Type1 = db.GetString(reader, 2);
        entities.Event1.Description = db.GetNullableString(reader, 3);
        entities.Event1.BusinessObjectCode = db.GetString(reader, 4);
        entities.Event1.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.DetailName = db.GetString(reader, 1);
        entities.EventDetail.Description = db.GetNullableString(reader, 2);
        entities.EventDetail.InitiatingStateCode = db.GetString(reader, 3);
        entities.EventDetail.CsenetInOutCode = db.GetString(reader, 4);
        entities.EventDetail.ReasonCode = db.GetString(reader, 5);
        entities.EventDetail.EveNo = db.GetInt32(reader, 6);
        entities.EventDetail.Function = db.GetNullableString(reader, 7);
        entities.EventDetail.Populated = true;
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
    /// <summary>
    /// A value of EndOfFile.
    /// </summary>
    [JsonPropertyName("endOfFile")]
    public Common EndOfFile
    {
      get => endOfFile ??= new();
      set => endOfFile = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Export417.
    /// </summary>
    [JsonPropertyName("export417")]
    public OfficeServiceProviderAlert Export417
    {
      get => export417 ??= new();
      set => export417 = value;
    }

    /// <summary>
    /// A value of Export416.
    /// </summary>
    [JsonPropertyName("export416")]
    public OfficeServiceProviderAlert Export416
    {
      get => export416 ??= new();
      set => export416 = value;
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

    private Common endOfFile;
    private DateWorkArea current;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common debugOn;
    private ProgramProcessingInfo programProcessingInfo;
    private OfficeServiceProviderAlert export417;
    private OfficeServiceProviderAlert export416;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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

    private External external;
    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Infrastructure infrastructure;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
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

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private Event1 event1;
    private EventDetail eventDetail;
    private Alert alert;
  }
#endregion
}
