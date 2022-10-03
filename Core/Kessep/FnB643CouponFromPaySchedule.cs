// Program: FN_B643_COUPON_FROM_PAY_SCHEDULE, ID: 372764761, model: 746.
// Short name: SWE02463
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_COUPON_FROM_PAY_SCHEDULE.
/// </summary>
[Serializable]
public partial class FnB643CouponFromPaySchedule: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_COUPON_FROM_PAY_SCHEDULE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643CouponFromPaySchedule(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643CouponFromPaySchedule.
  /// </summary>
  public FnB643CouponFromPaySchedule(IContext context, Import import,
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
    export.CouponsCreated.Count = import.CouponsCreated.Count;
    export.VendorFileRecordCount.Count = import.VendorFileRecordCount.Count;
    export.SortSequenceNumber.Count = import.SortSequenceNumber.Count;
    local.Max.DayOfMonth1 = Day(import.CouponEnd.Date);

    switch(TrimEnd(import.ObligationPaymentSchedule.FrequencyCode))
    {
      case "M":
        if (import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault() < 1
          || import
          .ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault() > 31)
        {
          local.ObligationPaymentSchedule.DayOfMonth1 = 1;
        }
        else if (import.ObligationPaymentSchedule.DayOfMonth1.
          GetValueOrDefault() > local.Max.DayOfMonth1.GetValueOrDefault())
        {
          local.ObligationPaymentSchedule.DayOfMonth1 =
            local.Max.DayOfMonth1.GetValueOrDefault();
        }
        else
        {
          local.ObligationPaymentSchedule.DayOfMonth1 =
            import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
        }

        if (import.ObligationPaymentSchedule.Amount.GetValueOrDefault() > import
          .PastDueAmt.AverageCurrency)
        {
          local.AmtDue.AverageCurrency = import.PastDueAmt.AverageCurrency;
        }
        else
        {
          local.AmtDue.AverageCurrency =
            import.ObligationPaymentSchedule.Amount.GetValueOrDefault();
        }

        local.DebtDetail.DueDt = import.CouponBegin.Date;
        local.DueBy.Text10 =
          NumberToString(Month(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
          (local.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault(), 14,
          2) + "/" + NumberToString(Year(local.DebtDetail.DueDt), 12, 4);
        local.PastDueAsOf.Text10 = import.StmtEndDate.Text10;
        UseFnB643CouponWrite();

        break;
      case "W":
        UseFnB643DetermineCoupons();
        local.AmtDue.AverageCurrency =
          import.ObligationPaymentSchedule.Amount.GetValueOrDefault();

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (Lt(local.Group.Item.DebtDetail.DueDt, import.CouponBegin.Date))
          {
            return;
          }

          local.DebtDetail.DueDt = local.Group.Item.DebtDetail.DueDt;
          local.DueBy.Text10 =
            NumberToString(Month(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
            (Day(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
            (Year(local.DebtDetail.DueDt), 12, 4);
          local.PastDueAsOf.Text10 = import.StmtEndDate.Text10;
          UseFnB643CouponWrite();
        }

        break;
      case "BW":
        UseFnB643DetermineCoupons();
        local.AmtDue.AverageCurrency =
          import.ObligationPaymentSchedule.Amount.GetValueOrDefault();

        for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
          local.Group.Index)
        {
          if (Lt(local.Group.Item.DebtDetail.DueDt, import.CouponBegin.Date))
          {
            return;
          }

          local.DebtDetail.DueDt = local.Group.Item.DebtDetail.DueDt;
          local.DueBy.Text10 =
            NumberToString(Month(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
            (Day(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
            (Year(local.DebtDetail.DueDt), 12, 4);
          local.PastDueAsOf.Text10 = import.StmtEndDate.Text10;
          UseFnB643CouponWrite();
        }

        break;
      case "SM":
        if (import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault() < 1
          || import
          .ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault() > 31 || import
          .ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault() < 1 || import
          .ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault() > 31 || import
          .ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault() == import
          .ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault())
        {
          local.ObligationPaymentSchedule.DayOfMonth1 = 1;
          local.ObligationPaymentSchedule.DayOfMonth2 = 15;
        }
        else
        {
          if (import.ObligationPaymentSchedule.DayOfMonth1.
            GetValueOrDefault() < import
            .ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault())
          {
            local.ObligationPaymentSchedule.DayOfMonth1 =
              import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
            local.ObligationPaymentSchedule.DayOfMonth2 =
              import.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault();
          }
          else
          {
            local.ObligationPaymentSchedule.DayOfMonth1 =
              import.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault();
            local.ObligationPaymentSchedule.DayOfMonth2 =
              import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
          }

          if (local.ObligationPaymentSchedule.DayOfMonth1.
            GetValueOrDefault() > local.Max.DayOfMonth1.GetValueOrDefault())
          {
            local.ObligationPaymentSchedule.DayOfMonth1 = 15;
          }

          if (local.ObligationPaymentSchedule.DayOfMonth2.
            GetValueOrDefault() > local.Max.DayOfMonth1.GetValueOrDefault())
          {
            local.ObligationPaymentSchedule.DayOfMonth2 =
              local.Max.DayOfMonth1.GetValueOrDefault();
          }
        }

        if (import.ObligationPaymentSchedule.Amount.GetValueOrDefault() > import
          .PastDueAmt.AverageCurrency)
        {
          local.AmtDue.AverageCurrency = import.PastDueAmt.AverageCurrency;
        }
        else
        {
          local.AmtDue.AverageCurrency =
            import.ObligationPaymentSchedule.Amount.GetValueOrDefault();
        }

        local.DebtDetail.DueDt = import.CouponBegin.Date;
        local.DueBy.Text10 =
          NumberToString(Month(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
          (local.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault(), 14,
          2) + "/" + NumberToString(Year(local.DebtDetail.DueDt), 12, 4);
        local.PastDueAsOf.Text10 = import.StmtEndDate.Text10;
        UseFnB643CouponWrite();

        if (import.PastDueAmt.AverageCurrency > local.AmtDue.AverageCurrency)
        {
          local.DebtDetail.DueDt = import.CouponBegin.Date;
          local.DueBy.Text10 =
            NumberToString(Month(local.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
            (local.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault(),
            14, 2) + "/" + NumberToString(Year(local.DebtDetail.DueDt), 12, 4);
          local.PastDueAsOf.Text10 = import.StmtEndDate.Text10;
          UseFnB643CouponWrite();
        }

        break;
      default:
        break;
    }
  }

  private static void MoveExport1ToGroup(FnB643DetermineCoupons.Export.
    ExportGroup source, Local.GroupGroup target)
  {
    target.DebtDetail.DueDt = source.DebtDetail.DueDt;
  }

  private void UseFnB643CouponWrite()
  {
    var useImport = new FnB643CouponWrite.Import();
    var useExport = new FnB643CouponWrite.Export();

    useImport.PaymentAddress.Assign(import.PaymentAddress);
    useImport.PayToName.Text30 = import.PayToName.Text30;
    useImport.PastDueAmt.AverageCurrency = import.PastDueAmt.AverageCurrency;
    useImport.PastDueAsOf.Text10 = import.StmtEndDate.Text10;
    useImport.AmtDue.AverageCurrency = local.AmtDue.AverageCurrency;
    useImport.DueBy.Text10 = local.DueBy.Text10;
    useImport.CouponBegin.Date = import.CouponBegin.Date;
    useImport.CpnReportingMonthYear.Text30 =
      import.CpnReportingMonthYear.Text30;
    useImport.LegalAction.Assign(import.LegalAction);
    useImport.StmtEndDate.Text10 = import.StmtEndDate.Text10;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.Client.Assign(import.Client);
    useImport.CouponsCreated.Count = export.CouponsCreated.Count;
    useImport.VendorFileRecordCount.Count = export.VendorFileRecordCount.Count;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;

    Call(FnB643CouponWrite.Execute, useImport, useExport);

    export.CouponsCreated.Count = useExport.CouponsCreated.Count;
    export.VendorFileRecordCount.Count = useExport.VendorFileRecordCount.Count;
    export.SortSequenceNumber.Count = useExport.SortSequenceNumber.Count;
    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB643DetermineCoupons()
  {
    var useImport = new FnB643DetermineCoupons.Import();
    var useExport = new FnB643DetermineCoupons.Export();

    useImport.CouponBegin.Date = import.CouponBegin.Date;
    useImport.CouponEnd.Date = import.CouponEnd.Date;
    useImport.ObligationPaymentSchedule.
      Assign(import.ObligationPaymentSchedule);

    Call(FnB643DetermineCoupons.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Group, MoveExport1ToGroup);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
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
    /// A value of PastDueAmt.
    /// </summary>
    [JsonPropertyName("pastDueAmt")]
    public Common PastDueAmt
    {
      get => pastDueAmt ??= new();
      set => pastDueAmt = value;
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
    /// A value of PaymentAddress.
    /// </summary>
    [JsonPropertyName("paymentAddress")]
    public CsePersonAddress PaymentAddress
    {
      get => paymentAddress ??= new();
      set => paymentAddress = value;
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
    /// A value of StmtEndDate.
    /// </summary>
    [JsonPropertyName("stmtEndDate")]
    public TextWorkArea StmtEndDate
    {
      get => stmtEndDate ??= new();
      set => stmtEndDate = value;
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
    /// A value of StmtNumber.
    /// </summary>
    [JsonPropertyName("stmtNumber")]
    public Common StmtNumber
    {
      get => stmtNumber ??= new();
      set => stmtNumber = value;
    }

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
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
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

    private DateWorkArea couponBegin;
    private DateWorkArea couponEnd;
    private Common pastDueAmt;
    private TextWorkArea payToName;
    private CsePersonAddress paymentAddress;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private TextWorkArea cpnReportingMonthYear;
    private LegalAction legalAction;
    private TextWorkArea stmtEndDate;
    private CsePerson client;
    private Common stmtNumber;
    private Common couponsCreated;
    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
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
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Common couponsCreated;
    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private DebtDetail debtDetail;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public ObligationPaymentSchedule Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of AmtDue.
    /// </summary>
    [JsonPropertyName("amtDue")]
    public Common AmtDue
    {
      get => amtDue ??= new();
      set => amtDue = value;
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
    /// A value of PastDueAsOf.
    /// </summary>
    [JsonPropertyName("pastDueAsOf")]
    public TextWorkArea PastDueAsOf
    {
      get => pastDueAsOf ??= new();
      set => pastDueAsOf = value;
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

    private Array<GroupGroup> group;
    private ObligationPaymentSchedule max;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Common amtDue;
    private DebtDetail debtDetail;
    private Common recordType;
    private EabFileHandling eabFileHandling;
    private EabReportSend variableLine1;
    private EabReportSend variableLine2;
    private TextWorkArea pastDueAsOf;
    private TextWorkArea dueBy;
  }
#endregion
}
