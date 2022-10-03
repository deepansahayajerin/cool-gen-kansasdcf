// Program: SP_B709_HOUSEKEEPING, ID: 372132406, model: 746.
// Short name: SWE02262
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B709_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB709Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B709_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB709Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB709Housekeeping.
  /// </summary>
  public SpB709Housekeeping(IContext context, Import import, Export export):
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
    // 01/30/2001	M Ramirez	WR 281		Alert for failed documents
    // 04/04/2001	M Ramirez	WR 187		Alert for automatic documents
    // 09/13/2001	M Ashworth	PR 126727       Changed group view size from 30 to 
    // 50
    // 11/17/2011	G Vandy		CQ8728		Extract Number of Days til Excluded
    // 						Documents are Cancelled from parm list.
    // ----------------------------------------------------------------------------
    // --------------------------------------------------------------
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // --------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB709";
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

    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // SET RUNTIME PARAMETERS TO DEFAULTS
      // --------------------------------------------------------------------
      export.DebugOn.Flag = "N";
    }
    else
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

      if (local.Position.Count <= 0)
      {
        export.DebugOn.Flag = "N";
      }
      else
      {
        export.DebugOn.Flag =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 6, 1);
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "CANCEL:");

      if (local.Position.Count <= 0)
      {
        export.NumberOfDaysTilCancel.Count = 0;
      }
      else
      {
        local.Position.Count += 7;
        local.TextWorkArea.Text30 =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count,
          Length(local.ProgramProcessingInfo.ParameterList) - local
          .Position.Count + 1);
        local.Position.Count =
          Verify(local.TextWorkArea.Text30 + "/", "0123456789");
        export.NumberOfDaysTilCancel.Count =
          (int)StringToNumber(Substring(
            local.TextWorkArea.Text30, TextWorkArea.Text30_MaxLength, 1,
          local.Position.Count - 1));
      }

      export.Exceptions.Index = -1;

      foreach(var item in ReadDocument())
      {
        if (Equal(entities.Document.Name, local.Previous.Name))
        {
          continue;
        }
        else
        {
          local.Previous.Name = entities.Document.Name;
        }

        local.Position.Count =
          Find(local.ProgramProcessingInfo.ParameterList,
          TrimEnd(entities.Document.Name));

        if (local.Position.Count <= 0)
        {
          continue;
        }

        if (export.Exceptions.Index + 1 >= Export.ExceptionsGroup.Capacity)
        {
          break;
        }

        ++export.Exceptions.Index;
        export.Exceptions.CheckSize();

        export.Exceptions.Update.G.Name = entities.Document.Name;
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
    // WRITE INITIAL LINES TO CONTROL REPORT 98
    // -----------------------------------------------------------
    if (export.Exceptions.Index < 0)
    {
      local.EabReportSend.RptDetail = "No exception documents";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
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

        if (!IsEmpty(export.Exceptions.Item.G.Name))
        {
          local.EabReportSend.RptDetail = export.Exceptions.Item.G.Name;
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

    local.EabReportSend.RptDetail =
      "Number of days until excluded documents are cancelled: " + NumberToString
      (export.NumberOfDaysTilCancel.Count, 10, 6);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
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

  private IEnumerable<bool> ReadDocument()
  {
    entities.Document.Populated = false;

    return ReadEach("ReadDocument",
      null,
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.EffectiveDate = db.GetDate(reader, 2);
        entities.Document.Populated = true;

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
    /// <summary>A ExceptionsGroup group.</summary>
    [Serializable]
    public class ExceptionsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Document G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of ZdelGExportType.
      /// </summary>
      [JsonPropertyName("zdelGExportType")]
      public Common ZdelGExportType
      {
        get => zdelGExportType ??= new();
        set => zdelGExportType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document g;
      private Common zdelGExportType;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of UnMonitored.
    /// </summary>
    [JsonPropertyName("unMonitored")]
    public OfficeServiceProviderAlert UnMonitored
    {
      get => unMonitored ??= new();
      set => unMonitored = value;
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
    /// A value of NumberOfDaysTilCancel.
    /// </summary>
    [JsonPropertyName("numberOfDaysTilCancel")]
    public Common NumberOfDaysTilCancel
    {
      get => numberOfDaysTilCancel ??= new();
      set => numberOfDaysTilCancel = value;
    }

    private Common debugOn;
    private DateWorkArea current;
    private SpDocLiteral spDocLiteral;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Array<ExceptionsGroup> exceptions;
    private OfficeServiceProviderAlert monitored;
    private OfficeServiceProviderAlert unMonitored;
    private OfficeServiceProviderAlert automatic;
    private Common numberOfDaysTilCancel;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Document Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public ProgramProcessingInfo Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of DividerPosition.
    /// </summary>
    [JsonPropertyName("dividerPosition")]
    public Common DividerPosition
    {
      get => dividerPosition ??= new();
      set => dividerPosition = value;
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

    private Document previous;
    private TextWorkArea textWorkArea;
    private ProgramProcessingInfo temp;
    private Common dividerPosition;
    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
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

    private Alert alert;
    private Document document;
  }
#endregion
}
