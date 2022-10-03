// Program: FN_BFX5_AE_INCOME_RECORDS_FIX, ID: 372903960, model: 746.
// Short name: SWEFFX5B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFX5_AE_INCOME_RECORDS_FIX.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfx5AeIncomeRecordsFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFX5_AE_INCOME_RECORDS_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfx5AeIncomeRecordsFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfx5AeIncomeRecordsFix.
  /// </summary>
  public FnBfx5AeIncomeRecordsFix(IContext context, Import import, Export export)
    :
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
    // N.Engoor - 10/07/1999
    // Fix to create Income Interface Notification records only for those 
    // obligees that have the net amount of disbursements for the month of Sep
    // greater than 0 and are AF cases.
    // ---------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();
    local.Eab.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport();

    if (!Equal(local.Eab.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.Eab.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.Start.Date = local.ProgramProcessingInfo.ProcessDate;
    local.End.Date = StringToDate(local.ProgramProcessingInfo.ParameterList);
    local.Total.Amount = 0;
    local.CountRetry.Count = 0;
    local.NoOfInterfaceDeleted.Count = 0;
    local.TotalCommitCount.Count = 0;

    // ---------------------------
    // Read all Passthru debits created for all Obligees for the month of Sep 
    // 1999.
    // --------------------------
    foreach(var item in ReadCsePersonObligeeDisbursementTransaction())
    {
      if (Equal(entities.Obligee2.Number, "000000017O"))
      {
        continue;
      }

      if (ReadDisbursementStatusHistory())
      {
        if (ReadDisbursementStatus())
        {
          if (entities.PassthruDisbursementStatus.SystemGeneratedIdentifier == 3
            )
          {
            // ---------------------------
            // Passthru is suppressed. Do not notify AE.
            // --------------------------
            continue;
          }
        }
      }

      // ---------------------------
      // Read all AF debits for that Obligee read that are
      // AF CCS   -  Current Child Support
      // AF CMC   -  Current Medical Cost
      // AF CMS   -  Current Medical Support
      // AF VOL   -  ADC Voluntary Payment
      // --------------------------
      foreach(var item1 in ReadDisbursementTransaction())
      {
        if (!ReadCollectionDisbursementTransactionDisbursementTransactionRln())
        {
          continue;
        }

        local.Total.Amount += entities.DisbursementTransaction.Amount;
      }

      if (local.Total.Amount <= 0)
      {
        // -----------------------------
        // If the total disbursement amount for that month sums up  to be zero 
        // or less, skip creation of the interface income notification record
        // for that obligee.
        // -----------------------------
        if (ReadInterfaceIncomeNotification())
        {
          DeleteInterfaceIncomeNotification();
          ++local.NoOfInterfaceDeleted.Count;
        }

        local.Total.Amount = 0;
      }

      ++local.TotalCommitCount.Count;

      if (local.TotalCommitCount.Count > 100)
      {
        UseExtToDoACommit();

        if (local.Numeric.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.TotalCommitCount.Count = 0;
      }
    }

    local.Eab.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "No: Of Interface Income Records deleted   : " + NumberToString
      (local.NoOfInterfaceDeleted.Count, 7, 9);
    UseCabControlReport1();

    if (!Equal(local.Eab.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.Eab.Action = "CLOSE";
    local.EabReportSend.RptDetail = "";
    UseCabErrorReport();

    if (!Equal(local.Eab.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseCabControlReport1();

    if (!Equal(local.Eab.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Eab.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Eab.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.Eab.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Eab.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.Eab.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Eab.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.Numeric.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.Numeric.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void DeleteInterfaceIncomeNotification()
  {
    Update("DeleteInterfaceIncomeNotification",
      (db, command) =>
      {
        db.SetInt32(
          command, "intrfcIncNtfId",
          entities.PassthruInterfaceIncomeNotification.
            SystemGeneratedIdentifier);
      });
  }

  private bool ReadCollectionDisbursementTransactionDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementTransactionRln.Populated = false;
    entities.Credit.Populated = false;
    entities.Collection.Populated = false;

    return Read(
      "ReadCollectionDisbursementTransactionDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Credit.ColId = db.GetNullableInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Credit.CrtId = db.GetNullableInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Credit.CstId = db.GetNullableInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Credit.CrvId = db.GetNullableInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Credit.CrdId = db.GetNullableInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Credit.ObgId = db.GetNullableInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Credit.CspNumberDisb = db.GetNullableString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Credit.OtrId = db.GetNullableInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Credit.OtyId = db.GetNullableInt32(reader, 12);
        entities.Collection.Amount = db.GetDecimal(reader, 13);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 14);
        entities.Credit.CpaType = db.GetString(reader, 15);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 15);
        entities.Credit.CspNumber = db.GetString(reader, 16);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 16);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 17);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 17);
        entities.Credit.Type1 = db.GetString(reader, 18);
        entities.Credit.Amount = db.GetDecimal(reader, 19);
        entities.Credit.ProcessDate = db.GetNullableDate(reader, 20);
        entities.Credit.CreatedBy = db.GetString(reader, 21);
        entities.Credit.CreatedTimestamp = db.GetDateTime(reader, 22);
        entities.Credit.LastUpdatedBy = db.GetNullableString(reader, 23);
        entities.Credit.LastUpdateTmst = db.GetNullableDateTime(reader, 24);
        entities.Credit.CollectionDate = db.GetNullableDate(reader, 25);
        entities.Credit.CollectionProcessDate = db.GetDate(reader, 26);
        entities.Credit.InterstateInd = db.GetNullableString(reader, 27);
        entities.Credit.PassthruProcDate = db.GetNullableDate(reader, 28);
        entities.Credit.DesignatedPayee = db.GetNullableString(reader, 29);
        entities.Credit.ReferenceNumber = db.GetNullableString(reader, 30);
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 31);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 32);
        entities.DisbursementTransactionRln.CspNumber =
          db.GetString(reader, 33);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 34);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 35);
        entities.DisbursementTransactionRln.Populated = true;
        entities.Credit.Populated = true;
        entities.Collection.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligeeDisbursementTransaction()
  {
    entities.PassthruDisbursementTransaction.Populated = false;
    entities.Obligee1.Populated = false;
    entities.Obligee2.Populated = false;

    return ReadEach("ReadCsePersonObligeeDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "collectionDate", local.End.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Obligee2.Number = db.GetString(reader, 0);
        entities.Obligee1.CspNumber = db.GetString(reader, 0);
        entities.PassthruDisbursementTransaction.CspNumber =
          db.GetString(reader, 0);
        entities.Obligee2.Type1 = db.GetString(reader, 1);
        entities.Obligee1.Type1 = db.GetString(reader, 2);
        entities.PassthruDisbursementTransaction.CpaType =
          db.GetString(reader, 2);
        entities.Obligee1.CreatedBy = db.GetString(reader, 3);
        entities.Obligee1.CreatedTmst = db.GetDateTime(reader, 4);
        entities.Obligee1.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Obligee1.LastUpdatedTmst = db.GetNullableDateTime(reader, 6);
        entities.Obligee1.PgmChgEffectiveDate = db.GetNullableDate(reader, 7);
        entities.PassthruDisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.PassthruDisbursementTransaction.Type1 =
          db.GetString(reader, 9);
        entities.PassthruDisbursementTransaction.Amount =
          db.GetDecimal(reader, 10);
        entities.PassthruDisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 11);
        entities.PassthruDisbursementTransaction.CreatedBy =
          db.GetString(reader, 12);
        entities.PassthruDisbursementTransaction.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.PassthruDisbursementTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.PassthruDisbursementTransaction.LastUpdateTmst =
          db.GetNullableDateTime(reader, 15);
        entities.PassthruDisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 16);
        entities.PassthruDisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 17);
        entities.PassthruDisbursementTransaction.InterstateInd =
          db.GetNullableString(reader, 18);
        entities.PassthruDisbursementTransaction.PassthruProcDate =
          db.GetNullableDate(reader, 19);
        entities.PassthruDisbursementTransaction.DesignatedPayee =
          db.GetNullableString(reader, 20);
        entities.PassthruDisbursementTransaction.Populated = true;
        entities.Obligee1.Populated = true;
        entities.Obligee2.Populated = true;

        return true;
      });
  }

  private bool ReadDisbursementStatus()
  {
    System.Diagnostics.Debug.Assert(
      entities.PassthruDisbursementStatusHistory.Populated);
    entities.PassthruDisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId",
          entities.PassthruDisbursementStatusHistory.DbsGeneratedId);
      },
      (db, reader) =>
      {
        entities.PassthruDisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PassthruDisbursementStatus.Populated = true;
      });
  }

  private bool ReadDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.PassthruDisbursementTransaction.Populated);
    entities.PassthruDisbursementStatusHistory.Populated = false;

    return Read("ReadDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.PassthruDisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber",
          entities.PassthruDisbursementTransaction.CspNumber);
        db.SetString(
          command, "cpaType", entities.PassthruDisbursementTransaction.CpaType);
          
      },
      (db, reader) =>
      {
        entities.PassthruDisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.PassthruDisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.PassthruDisbursementStatusHistory.CspNumber =
          db.GetString(reader, 2);
        entities.PassthruDisbursementStatusHistory.CpaType =
          db.GetString(reader, 3);
        entities.PassthruDisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.PassthruDisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.PassthruDisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.PassthruDisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.PassthruDisbursementStatusHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
        db.SetDate(command, "date1", local.Start.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.End.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.CreatedBy = db.GetString(reader, 6);
        entities.DisbursementTransaction.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbursementTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbursementTransaction.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 10);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.DisbursementTransaction.PassthruProcDate =
          db.GetNullableDate(reader, 12);
        entities.DisbursementTransaction.DesignatedPayee =
          db.GetNullableString(reader, 13);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 14);
        entities.DisbursementTransaction.Populated = true;

        return true;
      });
  }

  private bool ReadInterfaceIncomeNotification()
  {
    entities.PassthruInterfaceIncomeNotification.Populated = false;

    return Read("ReadInterfaceIncomeNotification",
      (db, command) =>
      {
        db.SetString(command, "suppCspNumber", entities.Obligee2.Number);
        db.SetDate(command, "processDt", local.Blank.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PassthruInterfaceIncomeNotification.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PassthruInterfaceIncomeNotification.SupportedCsePersonNumber =
          db.GetString(reader, 1);
        entities.PassthruInterfaceIncomeNotification.ObligorCsePersonNumber =
          db.GetString(reader, 2);
        entities.PassthruInterfaceIncomeNotification.CaseNumber =
          db.GetString(reader, 3);
        entities.PassthruInterfaceIncomeNotification.CollectionDate =
          db.GetDate(reader, 4);
        entities.PassthruInterfaceIncomeNotification.CollectionAmount =
          db.GetDecimal(reader, 5);
        entities.PassthruInterfaceIncomeNotification.PersonProgram =
          db.GetString(reader, 6);
        entities.PassthruInterfaceIncomeNotification.ProgramAppliedTo =
          db.GetString(reader, 7);
        entities.PassthruInterfaceIncomeNotification.AppliedToCode =
          db.GetString(reader, 8);
        entities.PassthruInterfaceIncomeNotification.DistributionDate =
          db.GetDate(reader, 9);
        entities.PassthruInterfaceIncomeNotification.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.PassthruInterfaceIncomeNotification.CreatedBy =
          db.GetString(reader, 11);
        entities.PassthruInterfaceIncomeNotification.ProcessDate =
          db.GetDate(reader, 12);
        entities.PassthruInterfaceIncomeNotification.Populated = true;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public EabFileHandling Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    /// <summary>
    /// A value of NoOfInterfaceDeleted.
    /// </summary>
    [JsonPropertyName("noOfInterfaceDeleted")]
    public Common NoOfInterfaceDeleted
    {
      get => noOfInterfaceDeleted ??= new();
      set => noOfInterfaceDeleted = value;
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
    /// A value of CollectionDate.
    /// </summary>
    [JsonPropertyName("collectionDate")]
    public DateWorkArea CollectionDate
    {
      get => collectionDate ??= new();
      set => collectionDate = value;
    }

    /// <summary>
    /// A value of Numeric.
    /// </summary>
    [JsonPropertyName("numeric")]
    public External Numeric
    {
      get => numeric ??= new();
      set => numeric = value;
    }

    /// <summary>
    /// A value of TotalCommitCount.
    /// </summary>
    [JsonPropertyName("totalCommitCount")]
    public Common TotalCommitCount
    {
      get => totalCommitCount ??= new();
      set => totalCommitCount = value;
    }

    /// <summary>
    /// A value of CountRetry.
    /// </summary>
    [JsonPropertyName("countRetry")]
    public Common CountRetry
    {
      get => countRetry ??= new();
      set => countRetry = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public DisbursementTransaction Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eab;
    private Common noOfInterfaceDeleted;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea collectionDate;
    private External numeric;
    private Common totalCommitCount;
    private Common countRetry;
    private DateWorkArea blank;
    private DisbursementTransaction total;
    private DateWorkArea end;
    private DateWorkArea start;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PassthruDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("passthruDisbursementTransaction")]
    public DisbursementTransaction PassthruDisbursementTransaction
    {
      get => passthruDisbursementTransaction ??= new();
      set => passthruDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of PassthruDisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("passthruDisbursementStatusHistory")]
    public DisbursementStatusHistory PassthruDisbursementStatusHistory
    {
      get => passthruDisbursementStatusHistory ??= new();
      set => passthruDisbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of PassthruDisbursementStatus.
    /// </summary>
    [JsonPropertyName("passthruDisbursementStatus")]
    public DisbursementStatus PassthruDisbursementStatus
    {
      get => passthruDisbursementStatus ??= new();
      set => passthruDisbursementStatus = value;
    }

    /// <summary>
    /// A value of PassthruPaymentRequest.
    /// </summary>
    [JsonPropertyName("passthruPaymentRequest")]
    public PaymentRequest PassthruPaymentRequest
    {
      get => passthruPaymentRequest ??= new();
      set => passthruPaymentRequest = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    /// <summary>
    /// A value of PassthruInterfaceIncomeNotification.
    /// </summary>
    [JsonPropertyName("passthruInterfaceIncomeNotification")]
    public InterfaceIncomeNotification PassthruInterfaceIncomeNotification
    {
      get => passthruInterfaceIncomeNotification ??= new();
      set => passthruInterfaceIncomeNotification = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    private DisbursementTransaction passthruDisbursementTransaction;
    private DisbursementStatusHistory passthruDisbursementStatusHistory;
    private DisbursementStatus passthruDisbursementStatus;
    private PaymentRequest passthruPaymentRequest;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction credit;
    private Collection collection;
    private DisbursementType disbursementType;
    private CsePersonAccount obligee1;
    private PaymentRequest paymentRequest;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private CsePerson obligee2;
    private InterfaceIncomeNotification passthruInterfaceIncomeNotification;
    private DisbursementTransaction disbursementTransaction;
  }
#endregion
}
