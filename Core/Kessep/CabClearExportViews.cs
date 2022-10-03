// Program: CAB_CLEAR_EXPORT_VIEWS, ID: 371870092, model: 746.
// Short name: SWE00085
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_CLEAR_EXPORT_VIEWS.
/// </summary>
[Serializable]
public partial class CabClearExportViews: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_CLEAR_EXPORT_VIEWS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabClearExportViews(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabClearExportViews.
  /// </summary>
  public CabClearExportViews(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************
    // Initial Version - K Doshi, 12/3/98
    // ************************************************
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// <summary>A DisbGroup group.</summary>
    [Serializable]
    public class DisbGroup
    {
      /// <summary>
      /// A value of DisbDetailCashReceipt.
      /// </summary>
      [JsonPropertyName("disbDetailCashReceipt")]
      public CashReceipt DisbDetailCashReceipt
      {
        get => disbDetailCashReceipt ??= new();
        set => disbDetailCashReceipt = value;
      }

      /// <summary>
      /// A value of ZdeleteGroupExportRefNumber.
      /// </summary>
      [JsonPropertyName("zdeleteGroupExportRefNumber")]
      public CrdCrComboNo ZdeleteGroupExportRefNumber
      {
        get => zdeleteGroupExportRefNumber ??= new();
        set => zdeleteGroupExportRefNumber = value;
      }

      /// <summary>
      /// A value of DetailSelect.
      /// </summary>
      [JsonPropertyName("detailSelect")]
      public Common DetailSelect
      {
        get => detailSelect ??= new();
        set => detailSelect = value;
      }

      /// <summary>
      /// A value of DisbDetailDisbursementType.
      /// </summary>
      [JsonPropertyName("disbDetailDisbursementType")]
      public DisbursementType DisbDetailDisbursementType
      {
        get => disbDetailDisbursementType ??= new();
        set => disbDetailDisbursementType = value;
      }

      /// <summary>
      /// A value of Coll.
      /// </summary>
      [JsonPropertyName("coll")]
      public DisbursementTransaction Coll
      {
        get => coll ??= new();
        set => coll = value;
      }

      /// <summary>
      /// A value of DisbDetailDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbDetailDisbursementTransaction")]
      public DisbursementTransaction DisbDetailDisbursementTransaction
      {
        get => disbDetailDisbursementTransaction ??= new();
        set => disbDetailDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of RefNumber.
      /// </summary>
      [JsonPropertyName("refNumber")]
      public WorkArea RefNumber
      {
        get => refNumber ??= new();
        set => refNumber = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CashReceipt disbDetailCashReceipt;
      private CrdCrComboNo zdeleteGroupExportRefNumber;
      private Common detailSelect;
      private DisbursementType disbDetailDisbursementType;
      private DisbursementTransaction coll;
      private DisbursementTransaction disbDetailDisbursementTransaction;
      private WorkArea refNumber;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of AddrMailed.
    /// </summary>
    [JsonPropertyName("addrMailed")]
    public LocalWorkAddr AddrMailed
    {
      get => addrMailed ??= new();
      set => addrMailed = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of DesigPayee.
    /// </summary>
    [JsonPropertyName("desigPayee")]
    public CsePersonsWorkSet DesigPayee
    {
      get => desigPayee ??= new();
      set => desigPayee = value;
    }

    /// <summary>
    /// A value of ReisFrom.
    /// </summary>
    [JsonPropertyName("reisFrom")]
    public PaymentRequest ReisFrom
    {
      get => reisFrom ??= new();
      set => reisFrom = value;
    }

    /// <summary>
    /// A value of ReisTo.
    /// </summary>
    [JsonPropertyName("reisTo")]
    public PaymentRequest ReisTo
    {
      get => reisTo ??= new();
      set => reisTo = value;
    }

    /// <summary>
    /// Gets a value of Disb.
    /// </summary>
    [JsonIgnore]
    public Array<DisbGroup> Disb => disb ??= new(DisbGroup.Capacity);

    /// <summary>
    /// Gets a value of Disb for json serialization.
    /// </summary>
    [JsonPropertyName("disb")]
    [Computed]
    public IList<DisbGroup> Disb_Json
    {
      get => disb;
      set => Disb.Assign(value);
    }

    /// <summary>
    /// A value of PassThruFlowCsePerson.
    /// </summary>
    [JsonPropertyName("passThruFlowCsePerson")]
    public CsePerson PassThruFlowCsePerson
    {
      get => passThruFlowCsePerson ??= new();
      set => passThruFlowCsePerson = value;
    }

    /// <summary>
    /// A value of PassThruFlowPaymentRequest.
    /// </summary>
    [JsonPropertyName("passThruFlowPaymentRequest")]
    public PaymentRequest PassThruFlowPaymentRequest
    {
      get => passThruFlowPaymentRequest ??= new();
      set => passThruFlowPaymentRequest = value;
    }

    /// <summary>
    /// A value of MailedTo.
    /// </summary>
    [JsonPropertyName("mailedTo")]
    public CsePersonsWorkSet MailedTo
    {
      get => mailedTo ??= new();
      set => mailedTo = value;
    }

    private PaymentRequest paymentRequest;
    private PaymentStatus paymentStatus;
    private LocalWorkAddr addrMailed;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet desigPayee;
    private PaymentRequest reisFrom;
    private PaymentRequest reisTo;
    private Array<DisbGroup> disb;
    private CsePerson passThruFlowCsePerson;
    private PaymentRequest passThruFlowPaymentRequest;
    private CsePersonsWorkSet mailedTo;
  }
#endregion
}
