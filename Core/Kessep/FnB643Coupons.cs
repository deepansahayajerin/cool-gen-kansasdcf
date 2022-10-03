// Program: FN_B643_COUPONS, ID: 372683790, model: 746.
// Short name: SWE02408
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_COUPONS.
/// </summary>
[Serializable]
public partial class FnB643Coupons: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_COUPONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643Coupons(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643Coupons.
  /// </summary>
  public FnB643Coupons(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************************************
    // * PR NUM     DATE     NAME      DESCRIPTION                        *
    // * ------  ----------  --------  
    // ----------------------------------
    // *
    // * # 413   08-23-1999  Ed Lyman  Fix coupon problems:               *
    // *
    // 
    // Consolidate where possible.
    // *
    // *
    // 
    // Suppress when future payment.
    // *
    // ********************************************************************
    export.CouponsCreated.Count = import.CouponsCreated.Count;
    export.VendorFileRecordCount.Count = import.VendorFileRecordCount.Count;
    export.SortSequenceNumber.Count = import.SortSequenceNumber.Count;

    // **************************************************************
    // Get ending monthly obligor summary for this obligation.
    // **************************************************************
    if (ReadMonthlyObligorSummary())
    {
      local.PastDueAmt.AverageCurrency =
        entities.MonthlyObligorSummary.ForMthCurrBal.GetValueOrDefault();
    }
    else
    {
      ExitState = "FN0000_MTH_OBLIGOR_SUM_NF";

      return;
    }

    // **************************************************************
    // Get the coupon payment address for this obligation.
    // **************************************************************
    local.AddressNotFound.Flag = "";
    UseFnB643CouponPaymentAddress();

    if (AsChar(local.AddressNotFound.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport();

      return;
    }

    // **************************************************************
    // If there is a payment schedule, get each accrual.
    // **************************************************************
    local.PayScheduleExists.Flag = "";

    if (ReadObligationPaymentSchedule())
    {
      local.PayScheduleExists.Flag = "Y";
    }

    if (AsChar(local.PayScheduleExists.Flag) == 'Y')
    {
      if (ReadObligationType())
      {
        if (AsChar(entities.ObligationType.Classification) == 'A')
        {
          // **************************************************************
          // * If this is an accruing obligation then read debt details to
          // * build the coupon.
          // **************************************************************
          local.CouponNeeded.Flag = "N";
          local.AmtDue.AverageCurrency = 0;

          foreach(var item in ReadDebtDetailDebt())
          {
            if (Lt(entities.DebtDetail.DueDt, import.CouponBegin.Date))
            {
              break;
            }

            if (Lt(import.CouponEnd.Date, entities.DebtDetail.DueDt))
            {
              continue;
            }

            // *** Do we need to write a previous coupon out? ***
            if (AsChar(local.CouponNeeded.Flag) == 'Y' && !
              Equal(entities.DebtDetail.DueDt, local.DebtDetail.DueDt))
            {
              local.PastDue.Text10 = import.StmtEndDate.Text10;
              local.DueBy.Text10 =
                NumberToString(Month(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
                (Day(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
                (Year(local.DebtDetail.DueDt), 12, 4);
              UseFnB643CouponWrite();
              local.AmtDue.AverageCurrency = 0;
            }

            // *************CHANGE TO BALANCE DUE AMOUNT PR # 413**************
            local.AmtDue.AverageCurrency += entities.DebtDetail.BalanceDueAmt;

            // ****************************************************************
            local.DebtDetail.DueDt = entities.DebtDetail.DueDt;

            // *************FOLLOWING CODE INSERTED PR # 413 ******************
            if (local.AmtDue.AverageCurrency <= -
              local.PastDueAmt.AverageCurrency)
            {
              continue;
            }

            // ****************************************************************
            local.CouponNeeded.Flag = "Y";
          }

          if (AsChar(local.CouponNeeded.Flag) == 'Y')
          {
            local.PastDue.Text10 = import.StmtEndDate.Text10;
            local.DueBy.Text10 =
              NumberToString(Month(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
              (Day(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
              (Year(local.DebtDetail.DueDt), 12, 4);
            UseFnB643CouponWrite();
          }
        }
        else
        {
          // **************************************************************
          // * If this is a non accruing obligation then use the payment
          // * schedule to build the coupon.  There must be a past due
          // * amount in order to generate a coupon.
          // **************************************************************
          if (local.PastDueAmt.AverageCurrency > 0)
          {
            UseFnB643CouponFromPaySchedule();
          }
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_TYPE_NF";
      }
    }
    else
    {
      if (!Lt(0, entities.MonthlyObligorSummary.ForMthCurrBal))
      {
        return;
      }

      local.AmtDue.AverageCurrency =
        entities.MonthlyObligorSummary.ForMthCurrBal.GetValueOrDefault();
      local.DebtDetail.DueDt = import.CouponBegin.Date;
      local.PastDue.Text10 = import.StmtEndDate.Text10;
      local.DueBy.Text10 =
        NumberToString(Month(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
        (Day(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
        (Year(local.DebtDetail.DueDt), 12, 4);
      UseFnB643CouponWrite();
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643CouponFromPaySchedule()
  {
    var useImport = new FnB643CouponFromPaySchedule.Import();
    var useExport = new FnB643CouponFromPaySchedule.Export();

    useImport.ObligationPaymentSchedule.Assign(
      entities.ObligationPaymentSchedule);
    useImport.StmtEndDate.Text10 = import.StmtEndDate.Text10;
    useImport.CpnReportingMonthYear.Text30 =
      import.CpnReportingMonthYear.Text30;
    useImport.LegalAction.Assign(import.LegalAction);
    useImport.Client.Assign(import.Client);
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.CouponBegin.Date = import.CouponBegin.Date;
    useImport.CouponEnd.Date = import.CouponEnd.Date;
    useImport.PayToName.Text30 = local.PayToName.Text30;
    useImport.PaymentAddress.Assign(local.PaymentAddress);
    useImport.PastDueAmt.AverageCurrency = local.PastDueAmt.AverageCurrency;
    useImport.CouponsCreated.Count = export.CouponsCreated.Count;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useImport.VendorFileRecordCount.Count = export.VendorFileRecordCount.Count;

    Call(FnB643CouponFromPaySchedule.Execute, useImport, useExport);

    export.CouponsCreated.Count = useExport.CouponsCreated.Count;
    export.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    export.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643CouponPaymentAddress()
  {
    var useImport = new FnB643CouponPaymentAddress.Import();
    var useExport = new FnB643CouponPaymentAddress.Export();

    useImport.Client.Assign(import.Client);
    useImport.CouponBegin.Date = import.CouponBegin.Date;
    useImport.LegalAction.Assign(import.LegalAction);
    useImport.Obligation.Assign(import.Obligation);

    Call(FnB643CouponPaymentAddress.Execute, useImport, useExport);

    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
    local.AddressNotFound.Flag = useExport.AddressNotFound.Flag;
    local.PayToName.Text30 = useExport.PaymentAddressPayTo.Text30;
    local.PaymentAddress.Assign(useExport.CsePersonAddress);
    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643CouponWrite()
  {
    var useImport = new FnB643CouponWrite.Import();
    var useExport = new FnB643CouponWrite.Export();

    useImport.StmtEndDate.Text10 = import.StmtEndDate.Text10;
    useImport.CpnReportingMonthYear.Text30 =
      import.CpnReportingMonthYear.Text30;
    useImport.LegalAction.Assign(import.LegalAction);
    useImport.Client.Assign(import.Client);
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.CouponBegin.Date = import.CouponBegin.Date;
    useImport.PayToName.Text30 = local.PayToName.Text30;
    useImport.PaymentAddress.Assign(local.PaymentAddress);
    useImport.DueBy.Text10 = local.DueBy.Text10;
    useImport.PastDueAsOf.Text10 = local.PastDue.Text10;
    useImport.AmtDue.AverageCurrency = local.AmtDue.AverageCurrency;
    useImport.PastDueAmt.AverageCurrency = local.PastDueAmt.AverageCurrency;
    useImport.CouponsCreated.Count = export.CouponsCreated.Count;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useImport.VendorFileRecordCount.Count = export.VendorFileRecordCount.Count;

    Call(FnB643CouponWrite.Execute, useImport, useExport);

    export.CouponsCreated.Count = useExport.CouponsCreated.Count;
    export.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    export.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadDebtDetailDebt()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;

    return ReadEach("ReadDebtDetailDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.Debt.Amount = db.GetDecimal(reader, 9);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 10);
        entities.Debt.DebtType = db.GetString(reader, 11);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 12);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 13);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.Debt.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadMonthlyObligorSummary()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.MonthlyObligorSummary.Populated = false;

    return Read("ReadMonthlyObligorSummary",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "otyType", import.Obligation.DtyGeneratedId);
          
        db.SetNullableInt32(
          command, "obgSGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.
          SetNullableString(command, "cspSNumber", import.Obligation.CspNumber);
          
        db.SetNullableString(command, "cpaSType", import.Obligation.CpaType);
        db.SetInt32(command, "fnclMsumYrMth", import.Ending.YearMonth);
      },
      (db, reader) =>
      {
        entities.MonthlyObligorSummary.Type1 = db.GetString(reader, 0);
        entities.MonthlyObligorSummary.YearMonth = db.GetInt32(reader, 1);
        entities.MonthlyObligorSummary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.MonthlyObligorSummary.CpaSType =
          db.GetNullableString(reader, 3);
        entities.MonthlyObligorSummary.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.MonthlyObligorSummary.ObgSGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.MonthlyObligorSummary.OtyType = db.GetNullableInt32(reader, 6);
        entities.MonthlyObligorSummary.ForMthCurrBal =
          db.GetNullableDecimal(reader, 7);
        entities.MonthlyObligorSummary.Populated = true;
        CheckValid<MonthlyObligorSummary>("Type1",
          entities.MonthlyObligorSummary.Type1);
        CheckValid<MonthlyObligorSummary>("CpaSType",
          entities.MonthlyObligorSummary.CpaSType);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", import.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", import.Obligation.CpaType);
        db.SetDate(
          command, "startDt", import.CouponBegin.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 8);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 9);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 11);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", import.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// A value of CouponsCreated.
    /// </summary>
    [JsonPropertyName("couponsCreated")]
    public Common CouponsCreated
    {
      get => couponsCreated ??= new();
      set => couponsCreated = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of StmtEndDate.
    /// </summary>
    [JsonPropertyName("stmtEndDate")]
    public TextWorkArea StmtEndDate
    {
      get => stmtEndDate ??= new();
      set => stmtEndDate = value;
    }

    /// <summary>
    /// A value of CpnReportingMonthYear.
    /// </summary>
    [JsonPropertyName("cpnReportingMonthYear")]
    public TextWorkArea CpnReportingMonthYear
    {
      get => cpnReportingMonthYear ??= new();
      set => cpnReportingMonthYear = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of StmtNumber.
    /// </summary>
    [JsonPropertyName("stmtNumber")]
    public Common StmtNumber
    {
      get => stmtNumber ??= new();
      set => stmtNumber = value;
    }

    /// <summary>
    /// A value of CouponBegin.
    /// </summary>
    [JsonPropertyName("couponBegin")]
    public DateWorkArea CouponBegin
    {
      get => couponBegin ??= new();
      set => couponBegin = value;
    }

    /// <summary>
    /// A value of CouponEnd.
    /// </summary>
    [JsonPropertyName("couponEnd")]
    public DateWorkArea CouponEnd
    {
      get => couponEnd ??= new();
      set => couponEnd = value;
    }

    /// <summary>
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public MonthlyObligorSummary Ending
    {
      get => ending ??= new();
      set => ending = value;
    }

    private Common couponsCreated;
    private Common sortSequenceNumber;
    private TextWorkArea stmtEndDate;
    private TextWorkArea cpnReportingMonthYear;
    private LegalAction legalAction;
    private Obligation obligation;
    private CsePerson client;
    private Common vendorFileRecordCount;
    private Common stmtNumber;
    private DateWorkArea couponBegin;
    private DateWorkArea couponEnd;
    private MonthlyObligorSummary ending;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CouponsCreated.
    /// </summary>
    [JsonPropertyName("couponsCreated")]
    public Common CouponsCreated
    {
      get => couponsCreated ??= new();
      set => couponsCreated = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
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

    private Common couponsCreated;
    private Common sortSequenceNumber;
    private Common vendorFileRecordCount;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CouponNeeded.
    /// </summary>
    [JsonPropertyName("couponNeeded")]
    public Common CouponNeeded
    {
      get => couponNeeded ??= new();
      set => couponNeeded = value;
    }

    /// <summary>
    /// A value of PayToName.
    /// </summary>
    [JsonPropertyName("payToName")]
    public TextWorkArea PayToName
    {
      get => payToName ??= new();
      set => payToName = value;
    }

    /// <summary>
    /// A value of AddressNotFound.
    /// </summary>
    [JsonPropertyName("addressNotFound")]
    public Common AddressNotFound
    {
      get => addressNotFound ??= new();
      set => addressNotFound = value;
    }

    /// <summary>
    /// A value of PaymentAddress.
    /// </summary>
    [JsonPropertyName("paymentAddress")]
    public CsePersonAddress PaymentAddress
    {
      get => paymentAddress ??= new();
      set => paymentAddress = value;
    }

    /// <summary>
    /// A value of PayScheduleExists.
    /// </summary>
    [JsonPropertyName("payScheduleExists")]
    public Common PayScheduleExists
    {
      get => payScheduleExists ??= new();
      set => payScheduleExists = value;
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
    /// A value of DueBy.
    /// </summary>
    [JsonPropertyName("dueBy")]
    public TextWorkArea DueBy
    {
      get => dueBy ??= new();
      set => dueBy = value;
    }

    /// <summary>
    /// A value of PastDue.
    /// </summary>
    [JsonPropertyName("pastDue")]
    public TextWorkArea PastDue
    {
      get => pastDue ??= new();
      set => pastDue = value;
    }

    /// <summary>
    /// A value of AmtDue.
    /// </summary>
    [JsonPropertyName("amtDue")]
    public Common AmtDue
    {
      get => amtDue ??= new();
      set => amtDue = value;
    }

    /// <summary>
    /// A value of PastDueAmt.
    /// </summary>
    [JsonPropertyName("pastDueAmt")]
    public Common PastDueAmt
    {
      get => pastDueAmt ??= new();
      set => pastDueAmt = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of VariableLine1.
    /// </summary>
    [JsonPropertyName("variableLine1")]
    public EabReportSend VariableLine1
    {
      get => variableLine1 ??= new();
      set => variableLine1 = value;
    }

    /// <summary>
    /// A value of VariableLine2.
    /// </summary>
    [JsonPropertyName("variableLine2")]
    public EabReportSend VariableLine2
    {
      get => variableLine2 ??= new();
      set => variableLine2 = value;
    }

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
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

    private Common couponNeeded;
    private TextWorkArea payToName;
    private Common addressNotFound;
    private CsePersonAddress paymentAddress;
    private Common payScheduleExists;
    private DebtDetail debtDetail;
    private TextWorkArea dueBy;
    private TextWorkArea pastDue;
    private Common amtDue;
    private Common pastDueAmt;
    private Common recordType;
    private EabFileHandling eabFileHandling;
    private EabReportSend variableLine1;
    private EabReportSend variableLine2;
    private GlobalStatementMessage globalStatementMessage;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of MonthlyObligorSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligorSummary")]
    public MonthlyObligorSummary MonthlyObligorSummary
    {
      get => monthlyObligorSummary ??= new();
      set => monthlyObligorSummary = value;
    }

    private ObligationType obligationType;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private MonthlyObligorSummary monthlyObligorSummary;
  }
#endregion
}
