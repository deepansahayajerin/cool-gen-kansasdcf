// Program: SP_B706_RESTORE_ARCHIVED_DOCS, ID: 372990746, model: 746.
// Short name: SWEP706B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B706_RESTORE_ARCHIVED_DOCS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB706RestoreArchivedDocs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B706_RESTORE_ARCHIVED_DOCS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB706RestoreArchivedDocs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB706RestoreArchivedDocs.
  /// </summary>
  public SpB706RestoreArchivedDocs(IContext context, Import import,
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
    // --------------------------------------------------------------------
    // Date		Developer	Request #      Description
    // --------------------------------------------------------------------
    // 01/18/2000	M Ramirez			Initial Dev
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB706Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -----------------------------------------------
      // Message is from exitstate
      // -----------------------------------------------
      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.LcontrolTotalRead.Count = 0;
    local.RowLock.Count = 0;

    foreach(var item in ReadOutgoingDocument())
    {
      ++local.LcontrolTotalRead.Count;
      ++local.RowLock.Count;
      local.EabReportSend.RptDetail = "";

      if (!ReadInfrastructure())
      {
        ++local.LcontrolTotalErred.Count;
        local.EabConvertNumeric.SendNonSuppressPos = 9;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Infrastructure.SystemGeneratedIdentifier, 15);
          
        UseEabConvertNumeric1();
        local.WorkArea.Text9 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
        local.EabReportSend.RptDetail =
          "ERROR:  Infrastructure not found for document:  " + local
          .WorkArea.Text9;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";

        continue;
      }

      if (ReadRetrieveFieldValueTrigger())
      {
        ++local.LcontrolTotalWarned.Count;
        local.EabConvertNumeric.SendNonSuppressPos = 9;
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Infrastructure.SystemGeneratedIdentifier, 15);
          
        UseEabConvertNumeric1();
        local.WorkArea.Text9 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
        local.EabReportSend.RptDetail =
          "WARNING:  Restore trigger already exists for document:  " + local
          .WorkArea.Text9;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }
      else
      {
        try
        {
          CreateRetrieveFieldValueTrigger();
          ++local.LcontrolTotalProcessed.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.LcontrolTotalErred.Count;
              local.EabConvertNumeric.SendNonSuppressPos = 9;
              local.EabConvertNumeric.SendAmount =
                NumberToString(entities.Infrastructure.
                  SystemGeneratedIdentifier, 15);
              UseEabConvertNumeric1();
              local.WorkArea.Text9 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7,
                9);
              local.EabReportSend.RptDetail =
                "ERROR:  Restore trigger already exists for document:  " + local
                .WorkArea.Text9;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";

              break;
            case ErrorCode.PermittedValueViolation:
              ++local.LcontrolTotalErred.Count;
              local.EabConvertNumeric.SendNonSuppressPos = 9;
              local.EabConvertNumeric.SendAmount =
                NumberToString(entities.Infrastructure.
                  SystemGeneratedIdentifier, 15);
              UseEabConvertNumeric1();
              local.WorkArea.Text9 =
                Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7,
                9);
              local.EabReportSend.RptDetail =
                "ERROR:  Restore trigger permitted value violation for document:  " +
                local.WorkArea.Text9;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (local.RowLock.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .RowLock.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (local.External.NumericReturnCode > 0)
        {
          ++local.LcontrolTotalErred.Count;
          local.EabReportSend.RptDetail = "ERROR:  Unsuccessful commit";

          break;
        }

        local.RowLock.Count = 0;
      }
    }

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      // -----------------------------------------------------------
      // Ending as an ABEND
      // -----------------------------------------------------------
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      // -----------------------------------------------------------
      // Successful end for this program
      // -----------------------------------------------------------
      UseExtToDoACommit();

      if (local.External.NumericReturnCode > 0)
      {
        local.EabReportSend.RptDetail = "ERROR:  Unsuccessful commit";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    // -----------------------------------------------------------
    // Write control totals and close reports
    // -----------------------------------------------------------
    UseSpB706WriteControlsAndClose();
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpB706Housekeeping()
  {
    var useImport = new SpB706Housekeeping.Import();
    var useExport = new SpB706Housekeeping.Export();

    Call(SpB706Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.Current.Timestamp = useExport.Current.Timestamp;
    local.ParmDebug.Flag = useExport.Debug.Flag;
    local.ParmStop.Timestamp = useExport.DateStop.Timestamp;
    local.ParmStart.Timestamp = useExport.DateStart.Timestamp;
    local.Document.Name = useExport.Document.Name;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseSpB706WriteControlsAndClose()
  {
    var useImport = new SpB706WriteControlsAndClose.Import();
    var useExport = new SpB706WriteControlsAndClose.Export();

    useImport.RecsWarned.Count = local.LcontrolTotalWarned.Count;
    useImport.RecsDataErred.Count = local.LcontrolTotalErred.Count;
    useImport.RecsProcessed.Count = local.LcontrolTotalProcessed.Count;
    useImport.RecsRead.Count = local.LcontrolTotalRead.Count;

    Call(SpB706WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void CreateRetrieveFieldValueTrigger()
  {
    var archiveDate = entities.OutgoingDocument.FieldValuesArchiveDate;
    var infId = entities.Infrastructure.SystemGeneratedIdentifier;
    var createdBy = local.ProgramProcessingInfo.Name;
    var createdTimestamp = local.Current.Timestamp;

    entities.RetrieveFieldValueTrigger.Populated = false;
    Update("CreateRetrieveFieldValueTrigger",
      (db, command) =>
      {
        db.SetDate(command, "archiveDate", archiveDate);
        db.SetInt32(command, "infId", infId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "srvPrvdLogonId", createdBy);
      });

    entities.RetrieveFieldValueTrigger.ArchiveDate = archiveDate;
    entities.RetrieveFieldValueTrigger.InfId = infId;
    entities.RetrieveFieldValueTrigger.CreatedBy = createdBy;
    entities.RetrieveFieldValueTrigger.CreatedTimestamp = createdTimestamp;
    entities.RetrieveFieldValueTrigger.ServiceProviderLogonId = createdBy;
    entities.RetrieveFieldValueTrigger.Populated = true;
  }

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.
          SetInt32(command, "systemGeneratedI", entities.OutgoingDocument.InfId);
          
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 1);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1", local.ParmStart.Timestamp.GetValueOrDefault());
          
        db.SetDateTime(
          command, "timestamp2", local.ParmStop.Timestamp.GetValueOrDefault());
        db.SetNullableString(command, "docName", local.Document.Name);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 0);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 1);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 2);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 4);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 5);
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private bool ReadRetrieveFieldValueTrigger()
  {
    entities.RetrieveFieldValueTrigger.Populated = false;

    return Read("ReadRetrieveFieldValueTrigger",
      (db, command) =>
      {
        db.SetDate(
          command, "archiveDate",
          entities.OutgoingDocument.FieldValuesArchiveDate.GetValueOrDefault());
          
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.RetrieveFieldValueTrigger.ArchiveDate = db.GetDate(reader, 0);
        entities.RetrieveFieldValueTrigger.InfId = db.GetInt32(reader, 1);
        entities.RetrieveFieldValueTrigger.CreatedBy = db.GetString(reader, 2);
        entities.RetrieveFieldValueTrigger.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.RetrieveFieldValueTrigger.ServiceProviderLogonId =
          db.GetString(reader, 4);
        entities.RetrieveFieldValueTrigger.Populated = true;
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
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
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
    /// A value of RowLock.
    /// </summary>
    [JsonPropertyName("rowLock")]
    public Common RowLock
    {
      get => rowLock ??= new();
      set => rowLock = value;
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
    /// A value of ParmDebug.
    /// </summary>
    [JsonPropertyName("parmDebug")]
    public Common ParmDebug
    {
      get => parmDebug ??= new();
      set => parmDebug = value;
    }

    /// <summary>
    /// A value of LcontrolTotalWarned.
    /// </summary>
    [JsonPropertyName("lcontrolTotalWarned")]
    public Common LcontrolTotalWarned
    {
      get => lcontrolTotalWarned ??= new();
      set => lcontrolTotalWarned = value;
    }

    /// <summary>
    /// A value of ParmStop.
    /// </summary>
    [JsonPropertyName("parmStop")]
    public DateWorkArea ParmStop
    {
      get => parmStop ??= new();
      set => parmStop = value;
    }

    /// <summary>
    /// A value of ParmStart.
    /// </summary>
    [JsonPropertyName("parmStart")]
    public DateWorkArea ParmStart
    {
      get => parmStart ??= new();
      set => parmStart = value;
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
    /// A value of LcontrolTotalErred.
    /// </summary>
    [JsonPropertyName("lcontrolTotalErred")]
    public Common LcontrolTotalErred
    {
      get => lcontrolTotalErred ??= new();
      set => lcontrolTotalErred = value;
    }

    /// <summary>
    /// A value of LcontrolTotalProcessed.
    /// </summary>
    [JsonPropertyName("lcontrolTotalProcessed")]
    public Common LcontrolTotalProcessed
    {
      get => lcontrolTotalProcessed ??= new();
      set => lcontrolTotalProcessed = value;
    }

    /// <summary>
    /// A value of LcontrolTotalRead.
    /// </summary>
    [JsonPropertyName("lcontrolTotalRead")]
    public Common LcontrolTotalRead
    {
      get => lcontrolTotalRead ??= new();
      set => lcontrolTotalRead = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    private External external;
    private WorkArea workArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common rowLock;
    private DateWorkArea current;
    private Common parmDebug;
    private Common lcontrolTotalWarned;
    private DateWorkArea parmStop;
    private DateWorkArea parmStart;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common lcontrolTotalErred;
    private Common lcontrolTotalProcessed;
    private Common lcontrolTotalRead;
    private Document document;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabConvertNumeric2 eabConvertNumeric;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of RetrieveFieldValueTrigger.
    /// </summary>
    [JsonPropertyName("retrieveFieldValueTrigger")]
    public RetrieveFieldValueTrigger RetrieveFieldValueTrigger
    {
      get => retrieveFieldValueTrigger ??= new();
      set => retrieveFieldValueTrigger = value;
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

    private RetrieveFieldValueTrigger retrieveFieldValueTrigger;
    private Infrastructure infrastructure;
    private Document document;
    private OutgoingDocument outgoingDocument;
  }
#endregion
}
