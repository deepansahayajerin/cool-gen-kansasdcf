// Program: FN_BZZZ_UPDATE_DEBT_ADJUSTMENTS, ID: 372960491, model: 746.
// Short name: SWEFBXXX
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BZZZ_UPDATE_DEBT_ADJUSTMENTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBzzzUpdateDebtAdjustments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BZZZ_UPDATE_DEBT_ADJUSTMENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBzzzUpdateDebtAdjustments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBzzzUpdateDebtAdjustments.
  /// </summary>
  public FnBzzzUpdateDebtAdjustments(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.ReportNeeded.Flag = "Y";
    UseFnBxxxHousekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    foreach(var item in ReadCsePersonCsePersonAccount())
    {
      local.AdjustmentFound.Flag = "N";

      if (ReadDebtAdjustmentDebtDetail())
      {
        local.AdjustmentFound.Flag = "Y";
        ++local.DebtAdjustmentsRead.Count;
      }

      if (AsChar(local.AdjustmentFound.Flag) == 'Y')
      {
        local.DebtDtlDueDate.Text10 =
          NumberToString(Month(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
          (Day(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
          (Year(entities.DebtDetail.DueDt), 12, 4);

        if (AsChar(local.ReportNeeded.Flag) == 'Y')
        {
          if (AsChar(local.Update.Flag) == 'Y')
          {
            local.NeededToWrite.RptDetail = "          " + " " + local
              .DebtDtlDueDate.Text10 + " " + entities.CsePerson.Number + "   " +
              "  " + "   " + local.EndDate.Text10 + "   " + "UPDATED";
          }
          else
          {
            local.NeededToWrite.RptDetail = "          " + " " + local
              .DebtDtlDueDate.Text10 + " " + entities.CsePerson.Number + "   " +
              "  " + "   " + local.EndDate.Text10 + "   " + "";
          }

          UseCabBusinessReport01();
        }

        if (AsChar(local.Update.Flag) == 'Y')
        {
          try
          {
            UpdateDebtAdjustment();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OBLIG_TRANS_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIG_TRANS_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++local.DebtAdjustmentsUpdated.Count;
          ++local.CommitCount.Count;

          if (local.CommitCount.Count > 500)
          {
            local.CommitCount.Count = 0;
            UseExtToDoACommit();
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseCabErrorReport();

          return;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeBxxxClosing();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseOeBxxxClosing();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnBxxxHousekeeping()
  {
    var useImport = new FnBxxxHousekeeping.Import();
    var useExport = new FnBxxxHousekeeping.Export();

    Call(FnBxxxHousekeeping.Execute, useImport, useExport);

    local.Update.Flag = useExport.Delete.Flag;
    local.Current.Date = useExport.Process.Date;
  }

  private void UseOeBxxxClosing()
  {
    var useImport = new OeBxxxClosing.Import();
    var useExport = new OeBxxxClosing.Export();

    useImport.Cases.Count = local.DebtAdjustmentsRead.Count;
    useImport.MothersDeleted.Count = local.DebtAdjustmentsUpdated.Count;

    Call(OeBxxxClosing.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCsePersonCsePersonAccount()
  {
    entities.CsePerson.Populated = false;
    entities.CsePersonAccount.Populated = false;

    return ReadEach("ReadCsePersonCsePersonAccount",
      null,
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 2);
        entities.CsePerson.Populated = true;
        entities.CsePersonAccount.Populated = true;

        return true;
      });
  }

  private bool ReadDebtAdjustmentDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.DebtDetail.Populated = false;
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustmentDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetDate(
          command, "debtAdjProcDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.DebtAdjustment.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 8);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 10);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 11);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 12);
        entities.DebtDetail.CspNumber = db.GetString(reader, 13);
        entities.DebtDetail.CpaType = db.GetString(reader, 14);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 15);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 16);
        entities.DebtDetail.OtrType = db.GetString(reader, 17);
        entities.DebtDetail.DueDt = db.GetDate(reader, 18);
        entities.DebtDetail.Populated = true;
        entities.DebtAdjustment.Populated = true;
      });
  }

  private void UpdateDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.DebtAdjustment.Populated);

    var lastUpdatedBy = "SWEFBXXX";
    var lastUpdatedTmst = Now();
    var debtAdjustmentProcessDate = local.Null1.Date;

    entities.DebtAdjustment.Populated = false;
    Update("UpdateDebtAdjustment",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetDate(command, "debtAdjProcDate", debtAdjustmentProcessDate);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtAdjustment.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtAdjustment.CspNumber);
        db.SetString(command, "cpaType", entities.DebtAdjustment.CpaType);
        db.SetInt32(
          command, "obTrnId",
          entities.DebtAdjustment.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", entities.DebtAdjustment.Type1);
        db.SetInt32(command, "otyType", entities.DebtAdjustment.OtyType);
      });

    entities.DebtAdjustment.LastUpdatedBy = lastUpdatedBy;
    entities.DebtAdjustment.LastUpdatedTmst = lastUpdatedTmst;
    entities.DebtAdjustment.DebtAdjustmentProcessDate =
      debtAdjustmentProcessDate;
    entities.DebtAdjustment.Populated = true;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public TextWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of DebtDtlDueDate.
    /// </summary>
    [JsonPropertyName("debtDtlDueDate")]
    public TextWorkArea DebtDtlDueDate
    {
      get => debtDtlDueDate ??= new();
      set => debtDtlDueDate = value;
    }

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
    /// A value of CommitCount.
    /// </summary>
    [JsonPropertyName("commitCount")]
    public Common CommitCount
    {
      get => commitCount ??= new();
      set => commitCount = value;
    }

    /// <summary>
    /// A value of DebtAdjustmentsRead.
    /// </summary>
    [JsonPropertyName("debtAdjustmentsRead")]
    public Common DebtAdjustmentsRead
    {
      get => debtAdjustmentsRead ??= new();
      set => debtAdjustmentsRead = value;
    }

    /// <summary>
    /// A value of DebtAdjustmentsUpdated.
    /// </summary>
    [JsonPropertyName("debtAdjustmentsUpdated")]
    public Common DebtAdjustmentsUpdated
    {
      get => debtAdjustmentsUpdated ??= new();
      set => debtAdjustmentsUpdated = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
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
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of AdjustmentFound.
    /// </summary>
    [JsonPropertyName("adjustmentFound")]
    public Common AdjustmentFound
    {
      get => adjustmentFound ??= new();
      set => adjustmentFound = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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

    private DateWorkArea null1;
    private TextWorkArea endDate;
    private TextWorkArea debtDtlDueDate;
    private External external;
    private Common commitCount;
    private Common debtAdjustmentsRead;
    private Common debtAdjustmentsUpdated;
    private Common update;
    private ExitStateWorkArea exitStateWorkArea;
    private Common reportNeeded;
    private EabReportSend neededToWrite;
    private Common adjustmentFound;
    private DateWorkArea current;
    private DateWorkArea process;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private ObligationTransaction debtAdjustment;
    private Obligation obligation;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
  }
#endregion
}
