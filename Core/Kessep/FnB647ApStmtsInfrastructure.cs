// Program: FN_B647_AP_STMTS_INFRASTRUCTURE, ID: 372995966, model: 746.
// Short name: SWEF647B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B647_AP_STMTS_INFRASTRUCTURE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB647ApStmtsInfrastructure: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B647_AP_STMTS_INFRASTRUCTURE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB647ApStmtsInfrastructure(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB647ApStmtsInfrastructure.
  /// </summary>
  public FnB647ApStmtsInfrastructure(IContext context, Import import,
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
    // ***************************************************************************
    // * The objective of this procedure is to build monthly infrastructure
    // * records.
    // ***************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB647Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***********************************************************
    // * Event Id and Reason Code are used to identify EVENT and *
    // * EVENT DETAIL
    // 
    // *
    // ***********************************************************
    local.Event1.ControlNumber = 48;
    local.EventDetail.ReasonCode = "STMTCREATED";

    if (ReadEventEventDetail())
    {
      if ((Lt(local.Null1.Date, local.Deletion.Date) || Lt
        (local.Null1.Date, local.Purge.Date)) && AsChar
        (local.ProgramCheckpointRestart.RestartInd) == 'N')
      {
        UseFnB647DeleteApStmtInfrastr();

        // ***** Call an external that does a DB2 commit using a Cobol program.
        UseExtToDoACommit();
      }

      if (AsChar(entities.EventDetail.LogToDiaryInd) == 'Y')
      {
        local.NextCommit.Count =
          local.ProgramCheckpointRestart.UpdateFrequencyCount.
            GetValueOrDefault();

        if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
        {
          local.CsePerson.Number =
            Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
          local.StatementNumber.Text4 =
            Substring(local.ProgramCheckpointRestart.RestartInfo, 11, 4);
        }
        else
        {
          local.CsePerson.Number = "0000000000";
          local.StatementNumber.Text4 = "001";
        }

        local.EabFileHandling.Action = "READ";

        while(!Equal(local.EabFileHandling.Status, "EOF"))
        {
          UseEabReadApStmtsVendorFile();

          switch(TrimEnd(local.EabFileHandling.Status))
          {
            case "OK":
              if (Lt(local.StoreStmtMonth.Text10, local.StmtMonth.Text10))
              {
                local.StoreStmtMonth.Text10 = local.StmtMonth.Text10;
              }

              break;
            case "EOF":
              if (Equal(local.CsePerson.Number, "0000000000"))
              {
                goto AfterCycle;
              }

              break;
            default:
              goto AfterCycle;
          }

          UseFnB647Mainline();
          ++local.InfrastructureCreated.Count;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            break;
          }

          if (local.InfrastructureCreated.Count >= local.NextCommit.Count)
          {
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo =
              local.CsePerson.Number + local.StatementNumber.Text4;
            UseUpdatePgmCheckpointRestart();

            // ***** Call an external that does a DB2 commit using a Cobol 
            // program.
            UseExtToDoACommit();
            local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-" + NumberToString
              (Now().Date.Month, 14, 2) + "-" + NumberToString
              (Now().Date.Day, 14, 2);
            local.Time.Text8 = NumberToString(Time(Now()).Hours, 14, 2) + ":"
              + NumberToString(Time(Now()).Minutes, 14, 2) + ":" + NumberToString
              (Time(Now()).Seconds, 14, 2);
            local.EabReportSend.RptDetail =
              "Checkpoint taken, obligor number: " + local.CsePerson.Number + " at: " +
              local.Date.Text10 + "  " + local.Time.Text8 + "  after " + NumberToString
              (local.NextCommit.Count, 5, 11) + " updates";
            UseCabControlReport();
            local.NextCommit.Count += local.ProgramCheckpointRestart.
              UpdateFrequencyCount.GetValueOrDefault();
          }
        }

AfterCycle:
        ;
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "The Event_Detail attribute Log_to_diary = N.  No infrastructure records will be created.";
          
        UseCabControlReport();
      }
    }
    else
    {
      ExitState = "SP0000_EVENT_DETAIL_NF";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();
      UseFnB647Closing();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Person number = " + local
        .CsePerson.Number + " Message: " + local.ExitStateWorkArea.Message + "  " +
        "  ";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnB647Closing();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEvent1(Event1 source, Event1 target)
  {
    target.ControlNumber = source.ControlNumber;
    target.Type1 = source.Type1;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadApStmtsVendorFile()
  {
    var useImport = new EabReadApStmtsVendorFile.Import();
    var useExport = new EabReadApStmtsVendorFile.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.StatementNumber.Text4 = local.StatementNumber.Text4;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.CsePerson.Number = local.CsePerson.Number;
    useExport.StatementNumber.Text4 = local.StatementNumber.Text4;
    useExport.StmtYear.Text4 = local.StmtYear.Text4;
    useExport.StmtMonth.Text10 = local.StmtMonth.Text10;
    useExport.Coupons.Flag = local.CouponInd.Flag;
    useExport.CourtCase.Text30 = local.CourtCase.Text30;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadApStmtsVendorFile.Execute, useImport, useExport);

    local.CsePerson.Number = useExport.CsePerson.Number;
    local.StatementNumber.Text4 = useExport.StatementNumber.Text4;
    local.StmtYear.Text4 = useExport.StmtYear.Text4;
    local.StmtMonth.Text10 = useExport.StmtMonth.Text10;
    local.CouponInd.Flag = useExport.Coupons.Flag;
    local.CourtCase.Text30 = useExport.CourtCase.Text30;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void UseFnB647Closing()
  {
    var useImport = new FnB647Closing.Import();
    var useExport = new FnB647Closing.Export();

    useImport.InfrastructureDeleted.Count = local.Deleted.Count;
    useImport.InfrastructureCreated.Count = local.InfrastructureCreated.Count;

    Call(FnB647Closing.Execute, useImport, useExport);
  }

  private void UseFnB647DeleteApStmtInfrastr()
  {
    var useImport = new FnB647DeleteApStmtInfrastr.Import();
    var useExport = new FnB647DeleteApStmtInfrastr.Export();

    MoveEvent1(entities.Event1, useImport.Event1);
    useImport.EventDetail.ReasonCode = entities.EventDetail.ReasonCode;
    useImport.Purge.Date = local.Purge.Date;
    useImport.Deletion.Date = local.Deletion.Date;
    useImport.ProgramCheckpointRestart.UpdateFrequencyCount =
      local.ProgramCheckpointRestart.UpdateFrequencyCount;

    Call(FnB647DeleteApStmtInfrastr.Execute, useImport, useExport);

    local.Deleted.Count = useExport.Deleted.Count;
  }

  private void UseFnB647Housekeeping()
  {
    var useImport = new FnB647Housekeeping.Import();
    var useExport = new FnB647Housekeeping.Export();

    Call(FnB647Housekeeping.Execute, useImport, useExport);

    local.Purge.Date = useExport.Purge.Date;
    local.CsePerson.Number = useExport.Restart.Number;
    local.StatementNumber.Text4 = useExport.RestartStatementNumber.Text4;
    local.Deletion.Date = useExport.Deletion.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseFnB647Mainline()
  {
    var useImport = new FnB647Mainline.Import();
    var useExport = new FnB647Mainline.Export();

    useImport.Event1.ControlNumber = entities.Event1.ControlNumber;
    useImport.EventDetail.ReasonCode = entities.EventDetail.ReasonCode;
    useImport.Month.Text10 = local.StoreStmtMonth.Text10;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.Year.Text4 = local.StmtYear.Text4;
    useImport.CouponIndicator.Flag = local.CouponInd.Flag;
    useImport.CourtCase.Text30 = local.CourtCase.Text30;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(FnB647Mainline.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadEventEventDetail()
  {
    entities.Event1.Populated = false;
    entities.EventDetail.Populated = false;

    return Read("ReadEventEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", local.Event1.ControlNumber);
        db.SetString(command, "reasonCode", local.EventDetail.ReasonCode);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.Event1.Type1 = db.GetString(reader, 1);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.EventDetail.ReasonCode = db.GetString(reader, 3);
        entities.EventDetail.LogToDiaryInd = db.GetString(reader, 4);
        entities.Event1.Populated = true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Purge.
    /// </summary>
    [JsonPropertyName("purge")]
    public DateWorkArea Purge
    {
      get => purge ??= new();
      set => purge = value;
    }

    /// <summary>
    /// A value of StoreStmtMonth.
    /// </summary>
    [JsonPropertyName("storeStmtMonth")]
    public TextWorkArea StoreStmtMonth
    {
      get => storeStmtMonth ??= new();
      set => storeStmtMonth = value;
    }

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
    /// A value of Deleted.
    /// </summary>
    [JsonPropertyName("deleted")]
    public Common Deleted
    {
      get => deleted ??= new();
      set => deleted = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of StatementNumber.
    /// </summary>
    [JsonPropertyName("statementNumber")]
    public TextWorkArea StatementNumber
    {
      get => statementNumber ??= new();
      set => statementNumber = value;
    }

    /// <summary>
    /// A value of StmtYear.
    /// </summary>
    [JsonPropertyName("stmtYear")]
    public TextWorkArea StmtYear
    {
      get => stmtYear ??= new();
      set => stmtYear = value;
    }

    /// <summary>
    /// A value of StmtMonth.
    /// </summary>
    [JsonPropertyName("stmtMonth")]
    public TextWorkArea StmtMonth
    {
      get => stmtMonth ??= new();
      set => stmtMonth = value;
    }

    /// <summary>
    /// A value of CouponInd.
    /// </summary>
    [JsonPropertyName("couponInd")]
    public Common CouponInd
    {
      get => couponInd ??= new();
      set => couponInd = value;
    }

    /// <summary>
    /// A value of CourtCase.
    /// </summary>
    [JsonPropertyName("courtCase")]
    public TextWorkArea CourtCase
    {
      get => courtCase ??= new();
      set => courtCase = value;
    }

    /// <summary>
    /// A value of Deletion.
    /// </summary>
    [JsonPropertyName("deletion")]
    public DateWorkArea Deletion
    {
      get => deletion ??= new();
      set => deletion = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of InfrastructureCreated.
    /// </summary>
    [JsonPropertyName("infrastructureCreated")]
    public Common InfrastructureCreated
    {
      get => infrastructureCreated ??= new();
      set => infrastructureCreated = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of NextCommit.
    /// </summary>
    [JsonPropertyName("nextCommit")]
    public Common NextCommit
    {
      get => nextCommit ??= new();
      set => nextCommit = value;
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

    private DateWorkArea purge;
    private TextWorkArea storeStmtMonth;
    private Event1 event1;
    private EventDetail eventDetail;
    private Common deleted;
    private CsePerson csePerson;
    private TextWorkArea statementNumber;
    private TextWorkArea stmtYear;
    private TextWorkArea stmtMonth;
    private Common couponInd;
    private TextWorkArea courtCase;
    private DateWorkArea deletion;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea null1;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea date;
    private TextWorkArea time;
    private Common infrastructureCreated;
    private EabFileHandling eabFileHandling;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common nextCommit;
    private EabReportSend eabReportSend;
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

    private Event1 event1;
    private EventDetail eventDetail;
  }
#endregion
}
