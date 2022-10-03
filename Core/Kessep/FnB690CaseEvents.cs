// Program: FN_B690_CASE_EVENTS, ID: 372450314, model: 746.
// Short name: SWEF690B
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
/// A program: FN_B690_CASE_EVENTS.
/// </para>
/// <para>
/// Resp: Finance
/// This PRAD(Procedure) is designed to read all Obligations for a each CASE and
/// send an ALERT to the Case Coordinator when all DEBTs on the CASE are
/// satisfied.
/// Satisfaction implies that there is no Balance and or Interest remaining on 
/// any DEBT_DETAIL and there are no further ACCRUAL_INSTRUCTIONS.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB690CaseEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B690_CASE_EVENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB690CaseEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB690CaseEvents.
  /// </summary>
  public FnB690CaseEvents(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------
    // NPE - 06/21/1999   -   Added a new CAB to determine if all the 
    // obligations for the case have been satisfied.
    // Vithal Madhira      WR# 000243, PR# 114578, PR# 114066    02/28/2001
    // 1) Eliminate obligation alert when obligation paid off if other 
    // obligations are still active for the same court order.
    // 2) Alerts are needed when RECOVERY obligation is paid off.
    // K Cole	9/5/02	PR#142883, 144905. 151513
    // Only send alerts to workers on other cases if those cases contain the 
    // same ap/ch combination as the case with the activated or deactivated
    // debt.
    // ---------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Timestamp = Now();
    local.ProgramProcessingInfo.Name = "SWEFB690";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *****************************************************************
    // Open ERROR Report, DD Name = RPT99
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // *****************************************************************
    // Open CONTROL Report, DD Name = RPT98
    // *****************************************************************
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    foreach(var item in ReadCaseCsePerson())
    {
      if (!ReadCsePerson())
      {
        continue;
      }

      local.FeeRecoveryObFound.Flag = "N";
      local.ObligationFound.Flag = "N";

      foreach(var item1 in ReadObligationObligationType())
      {
        if (Equal(entities.ObligationType.Code, "VOL"))
        {
          continue;
        }

        if (AsChar(entities.ObligationType.Classification) == 'F' || AsChar
          (entities.ObligationType.Classification) == 'R')
        {
          local.FeeRecoveryObFound.Flag = "Y";
        }
        else
        {
          local.ObligationFound.Flag = "Y";
        }
      }

      ++local.NoOfCasesRead.Count;
      ++local.Commit.Count;

      // This extra read of case was added to improve performance of the big 
      // read above - it reduces the number of cursors opened. KMC
      if (!ReadCase())
      {
        local.EabReportSend.RptDetail =
          "Case not able to be re-read; Case Number = " + entities
          .Read.Number + ", Person Number = " + entities
          .ObligorCsePerson.Number;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        break;
      }

      UseFnReportCaseObligStatus();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();

        if (IsExitState("SP0000_EVENT_DETAIL_NF"))
        {
          local.EabConvertNumeric.SendAmount =
            NumberToString(local.Error.EventId, 15);
          local.EabConvertNumeric.SendNonSuppressPos = 1;
          UseEabConvertNumeric1();
          local.EabReportSend.RptDetail =
            TrimEnd(local.ExitStateWorkArea.Message) + ", Event ID = " + local
            .EabConvertNumeric.ReturnNoCommasInNonDecimal + ", Reason Code = " +
            local.Error.ReasonCode;
        }
        else
        {
          local.EabReportSend.RptDetail =
            TrimEnd(local.ExitStateWorkArea.Message) + ", Case Number = " + entities
            .Read.Number + ", Person Number = " + entities
            .ObligorCsePerson.Number;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        break;
      }

      if (AsChar(local.EventCreated.Flag) == 'Y')
      {
        ++local.NoOfEventsRaised.Count;

        try
        {
          UpdateCase();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail =
            TrimEnd(local.ExitStateWorkArea.Message) + ", Case Number = " + entities
            .Read.Number + ", Person Number = " + entities
            .ObligorCsePerson.Number;
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          break;
        }

        if (AsChar(local.ActiveOrDeactive.Flag) == 'D')
        {
          ++local.NoOfCasesDeactivated.Count;
        }
        else if (AsChar(local.ActivatedForFirstTime.Flag) == 'Y')
        {
          ++local.ActivatedForFirstTime.Count;
        }
        else
        {
          ++local.NoOfCasesReactivated.Count;
        }
      }

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.Commit.Count = 0;
        UseExtToDoACommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ------------------
      // Do the commit for the records not yet committed.
      // ------------------
      UseExtToDoACommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

        return;
      }
    }

    do
    {
      ++local.Repeat.Count;

      switch(local.Repeat.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF OPEN CASES READ                    :" + NumberToString
            (local.NoOfCasesRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF CASES FOR WHICH EVENTS WERE RAISED  :" + NumberToString
            (local.NoOfEventsRaised.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF CASES ACTIVATED FOR THE FIRST TIME  :" + NumberToString
            (local.ActivatedForFirstTime.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF CASES REACTIVATED                   :" + NumberToString
            (local.NoOfCasesReactivated.Count, 15);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "TOTAL NUMBER OF CASES DEACTIVATED                   :" + NumberToString
            (local.NoOfCasesDeactivated.Count, 15);

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    while(local.Repeat.Count < 5);

    // *****************************************************************
    // Close CONTROL Report, DD Name = RPT98
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

      return;
    }

    // *****************************************************************
    // Close ERROR Report, DD Name = RPT99
    // *****************************************************************
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.LastCaseEvent = source.LastCaseEvent;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
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

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode =
      local.PassAreaExternal.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassAreaExternal.NumericReturnCode =
      useExport.External.NumericReturnCode;
  }

  private void UseFnReportCaseObligStatus()
  {
    var useImport = new FnReportCaseObligStatus.Import();
    var useExport = new FnReportCaseObligStatus.Export();

    useImport.ObligationFound.Flag = local.ObligationFound.Flag;
    useImport.FeeRecoveryObFound.Flag = local.FeeRecoveryObFound.Flag;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Obligor.Number = entities.ObligorCsePerson.Number;
    MoveCase1(entities.Update, useImport.Case1);

    Call(FnReportCaseObligStatus.Execute, useImport, useExport);

    local.EventCreated.Flag = useExport.EventCreated.Flag;
    local.ActiveOrDeactive.Flag = useExport.ActiveDeactive.Flag;
    local.ActivatedForFirstTime.Flag = useExport.ActivatedForFirstTime.Flag;
    MoveInfrastructure(useExport.Error, local.Error);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadCase()
  {
    entities.Update.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Read.Number);
      },
      (db, reader) =>
      {
        entities.Update.Number = db.GetString(reader, 0);
        entities.Update.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.Update.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Update.LastCaseEvent = db.GetNullableString(reader, 3);
        entities.Update.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCsePerson()
  {
    entities.Read.Populated = false;
    entities.ObligorCsePerson.Populated = false;

    return ReadEach("ReadCaseCsePerson",
      null,
      (db, reader) =>
      {
        entities.Read.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Number = db.GetString(reader, 1);
        entities.Read.Populated = true;
        entities.ObligorCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Read.Number);
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationType()
  {
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 4);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 5);
        entities.ObligationType.Code = db.GetString(reader, 6);
        entities.ObligationType.Classification = db.GetString(reader, 7);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private void UpdateCase()
  {
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var lastCaseEvent = local.ActiveOrDeactive.Flag;

    entities.Update.Populated = false;
    Update("UpdateCase",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "lastCaseEvent", lastCaseEvent);
        db.SetString(command, "numb", entities.Update.Number);
      });

    entities.Update.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Update.LastUpdatedBy = lastUpdatedBy;
    entities.Update.LastCaseEvent = lastCaseEvent;
    entities.Update.Populated = true;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
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
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Infrastructure Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of ActivatedForFirstTime.
    /// </summary>
    [JsonPropertyName("activatedForFirstTime")]
    public Common ActivatedForFirstTime
    {
      get => activatedForFirstTime ??= new();
      set => activatedForFirstTime = value;
    }

    /// <summary>
    /// A value of EventCreated.
    /// </summary>
    [JsonPropertyName("eventCreated")]
    public Common EventCreated
    {
      get => eventCreated ??= new();
      set => eventCreated = value;
    }

    /// <summary>
    /// A value of ActiveOrDeactive.
    /// </summary>
    [JsonPropertyName("activeOrDeactive")]
    public Common ActiveOrDeactive
    {
      get => activeOrDeactive ??= new();
      set => activeOrDeactive = value;
    }

    /// <summary>
    /// A value of SetLastCaseEvent.
    /// </summary>
    [JsonPropertyName("setLastCaseEvent")]
    public Common SetLastCaseEvent
    {
      get => setLastCaseEvent ??= new();
      set => setLastCaseEvent = value;
    }

    /// <summary>
    /// A value of NoOfCasesDeactivated.
    /// </summary>
    [JsonPropertyName("noOfCasesDeactivated")]
    public Common NoOfCasesDeactivated
    {
      get => noOfCasesDeactivated ??= new();
      set => noOfCasesDeactivated = value;
    }

    /// <summary>
    /// A value of NoOfCasesReactivated.
    /// </summary>
    [JsonPropertyName("noOfCasesReactivated")]
    public Common NoOfCasesReactivated
    {
      get => noOfCasesReactivated ??= new();
      set => noOfCasesReactivated = value;
    }

    /// <summary>
    /// A value of Repeat.
    /// </summary>
    [JsonPropertyName("repeat")]
    public Common Repeat
    {
      get => repeat ??= new();
      set => repeat = value;
    }

    /// <summary>
    /// A value of NoOfEventsRaised.
    /// </summary>
    [JsonPropertyName("noOfEventsRaised")]
    public Common NoOfEventsRaised
    {
      get => noOfEventsRaised ??= new();
      set => noOfEventsRaised = value;
    }

    /// <summary>
    /// A value of NoOfCasesRead.
    /// </summary>
    [JsonPropertyName("noOfCasesRead")]
    public Common NoOfCasesRead
    {
      get => noOfCasesRead ??= new();
      set => noOfCasesRead = value;
    }

    /// <summary>
    /// A value of ObligationFound.
    /// </summary>
    [JsonPropertyName("obligationFound")]
    public Common ObligationFound
    {
      get => obligationFound ??= new();
      set => obligationFound = value;
    }

    /// <summary>
    /// A value of PassAreaExternal.
    /// </summary>
    [JsonPropertyName("passAreaExternal")]
    public External PassAreaExternal
    {
      get => passAreaExternal ??= new();
      set => passAreaExternal = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of FeeRecoveryObFound.
    /// </summary>
    [JsonPropertyName("feeRecoveryObFound")]
    public Common FeeRecoveryObFound
    {
      get => feeRecoveryObFound ??= new();
      set => feeRecoveryObFound = value;
    }

    private EabConvertNumeric2 eabConvertNumeric;
    private ExitStateWorkArea exitStateWorkArea;
    private Common commit;
    private Infrastructure error;
    private Common activatedForFirstTime;
    private Common eventCreated;
    private Common activeOrDeactive;
    private Common setLastCaseEvent;
    private Common noOfCasesDeactivated;
    private Common noOfCasesReactivated;
    private Common repeat;
    private Common noOfEventsRaised;
    private Common noOfCasesRead;
    private Common obligationFound;
    private External passAreaExternal;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea current;
    private Common feeRecoveryObFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Case1 Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public Case1 Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of SupportedCaseRole.
    /// </summary>
    [JsonPropertyName("supportedCaseRole")]
    public CaseRole SupportedCaseRole
    {
      get => supportedCaseRole ??= new();
      set => supportedCaseRole = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private Case1 update;
    private Case1 read;
    private CaseRole supportedCaseRole;
    private CaseRole caseRole;
    private CsePerson supportedCsePerson;
    private CsePerson obligorCsePerson;
    private CsePersonAccount obligorCsePersonAccount;
    private CsePersonAccount supportedCsePersonAccount;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }
#endregion
}
