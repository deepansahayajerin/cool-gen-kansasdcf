// Program: FN_HARDCODED_CASH_RECEIPTING, ID: 371721600, model: 746.
// Short name: SWE00485
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_HARDCODED_CASH_RECEIPTING.
/// </para>
/// <para>
/// RESP: CASHMGMT	
/// This action block sets some hardcoded values for the funding processes.  
/// This approach was taken because the funding area is external to CSE and at
/// the time was being redesigned by another project.  Maybe this will all be
/// replaced someday.
/// </para>
/// </summary>
[Serializable]
public partial class FnHardcodedCashReceipting: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_HARDCODED_CASH_RECEIPTING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnHardcodedCashReceipting(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnHardcodedCashReceipting.
  /// </summary>
  public FnHardcodedCashReceipting(IContext context, Import import,
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
    // *****  Cash Receipt Status
    export.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier = 1;
    export.CrsSystemId.CrsIdBalanced.SystemGeneratedIdentifier = 2;
    export.CrsSystemId.CrsIdInterface.SystemGeneratedIdentifier = 3;
    export.CrsSystemId.CrsIdDeposited.SystemGeneratedIdentifier = 4;
    export.CrsSystemId.CrsIdForwarded.SystemGeneratedIdentifier = 6;
    export.CrsSystemId.CrsIdPended.SystemGeneratedIdentifier = 7;
    export.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier = 8;

    // *****  Cash Receipt Detail Status
    export.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier = 1;
    export.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier = 2;
    export.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier = 3;
    export.CrdsSystemId.CrdsIdDistributed.SystemGeneratedIdentifier = 4;
    export.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier = 5;
    export.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier = 6;
    export.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier = 7;

    // *****  Cash Receipt Type System ID
    export.CrtSystemId.CrtIdCheck.SystemGeneratedIdentifier = 1;
    export.CrtSystemId.CrtIdFcrtRec.SystemGeneratedIdentifier = 2;
    export.CrtSystemId.CrtIdMnyOrd.SystemGeneratedIdentifier = 3;
    export.CrtSystemId.CrtIdCurrency.SystemGeneratedIdentifier = 4;
    export.CrtSystemId.CrtIdCreditCrd.SystemGeneratedIdentifier = 5;
    export.CrtSystemId.CrtIdEft.SystemGeneratedIdentifier = 6;
    export.CrtSystemId.CrtIdFdirPmt.SystemGeneratedIdentifier = 7;
    export.CrtSystemId.CrtIdCsenet.SystemGeneratedIdentifier = 8;
    export.CrtSystemId.CrtIdCourtRecord.SystemGeneratedIdentifier = 9;
    export.CrtSystemId.CrtIdInterfund.SystemGeneratedIdentifier = 10;
    export.CrtSystemId.CrtIdRdirPmt.SystemGeneratedIdentifier = 11;
    export.CrtSystemId.CrtIdManint.SystemGeneratedIdentifier = 12;
    export.CrtSystemId.CrtIdRecap.SystemGeneratedIdentifier = 13;
    export.CrtSystemId.CrtIdCourtDisbursed.SystemGeneratedIdentifier = 2;
    export.CrtSystemId.CrtIdDirectPay.SystemGeneratedIdentifier = 7;

    // *****  Cash Receipt Type Category
    export.CrtCategory.CrtCatCash.CategoryIndicator = "C";
    export.CrtCategory.CrtCatNonCash.CategoryIndicator = "N";
    export.CrtSystemId.CrtIdFcrtRec.CategoryIndicator = "N";
    export.CrtSystemId.CrtIdFdirPmt.CategoryIndicator = "N";

    // *****  Collection Type -- Cash or Non-Cash
    export.CollectionType.CtCash.CashNonCashInd = "C";
    export.CollectionType.CtNonCash.CashNonCashInd = "N";

    // *****  Cash Receipt Detail Fee Type
    export.CrdFeeType.CrdftSrsCostRec.SystemGeneratedIdentifier = 1;
    export.CrdFeeType.CrdftCourt.SystemGeneratedIdentifier = 2;
    export.CrdFeeType.CrdftMiscellaneous.SystemGeneratedIdentifier = 3;
    export.CrdFeeType.CrdftOtherState.SystemGeneratedIdentifier = 4;
    export.CrdFeeType.CrdftInterstate.SystemGeneratedIdentifier = 17;

    // *****  Cash Receipt Cash Balance Reason
    export.CrCashBalanceReason.CrOver.CashBalanceReason = "OVER";
    export.CrCashBalanceReason.CrUnder.CashBalanceReason = "UNDER";

    // *****  Cash Receipt Relationship Reason
    export.CrRlnRsnSystemId.CrrrCourtInterface.SystemGeneratedIdentifier = 1;

    // *****  Cash Receipt Detail Relationship Reason
    export.CrdTaxAdj.SequentialIdentifier = 9;

    // *****  Cash Receipt Detail Status History Reason Code
    export.CrdshReasonCode.CrdshSupcase.ReasonCodeId = "SUPCASE";
    export.CrdshReasonCode.CrdshSupcord.ReasonCodeId = "SUPCORD";
    export.CrdshReasonCode.CrdshSupmnl.ReasonCodeId = "SUPMNL";
    export.CrdshReasonCode.CrdshSupnoob.ReasonCodeId = "SUPNOOB";
    export.CrdshReasonCode.CrdshSupprsn.ReasonCodeId = "SUPPRSN";
    export.CrdshReasonCode.CrdshSuprfnd.ReasonCodeId = "SUPRFND";
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
    /// <summary>A CrsSystemIdGroup group.</summary>
    [Serializable]
    public class CrsSystemIdGroup
    {
      /// <summary>
      /// A value of CrsIdRecorded.
      /// </summary>
      [JsonPropertyName("crsIdRecorded")]
      public CashReceiptStatus CrsIdRecorded
      {
        get => crsIdRecorded ??= new();
        set => crsIdRecorded = value;
      }

      /// <summary>
      /// A value of CrsIdBalanced.
      /// </summary>
      [JsonPropertyName("crsIdBalanced")]
      public CashReceiptStatus CrsIdBalanced
      {
        get => crsIdBalanced ??= new();
        set => crsIdBalanced = value;
      }

      /// <summary>
      /// A value of CrsIdInterface.
      /// </summary>
      [JsonPropertyName("crsIdInterface")]
      public CashReceiptStatus CrsIdInterface
      {
        get => crsIdInterface ??= new();
        set => crsIdInterface = value;
      }

      /// <summary>
      /// A value of CrsIdDeposited.
      /// </summary>
      [JsonPropertyName("crsIdDeposited")]
      public CashReceiptStatus CrsIdDeposited
      {
        get => crsIdDeposited ??= new();
        set => crsIdDeposited = value;
      }

      /// <summary>
      /// A value of CrsIdForwarded.
      /// </summary>
      [JsonPropertyName("crsIdForwarded")]
      public CashReceiptStatus CrsIdForwarded
      {
        get => crsIdForwarded ??= new();
        set => crsIdForwarded = value;
      }

      /// <summary>
      /// A value of CrsIdPended.
      /// </summary>
      [JsonPropertyName("crsIdPended")]
      public CashReceiptStatus CrsIdPended
      {
        get => crsIdPended ??= new();
        set => crsIdPended = value;
      }

      /// <summary>
      /// A value of CrsIdDeleted.
      /// </summary>
      [JsonPropertyName("crsIdDeleted")]
      public CashReceiptStatus CrsIdDeleted
      {
        get => crsIdDeleted ??= new();
        set => crsIdDeleted = value;
      }

      private CashReceiptStatus crsIdRecorded;
      private CashReceiptStatus crsIdBalanced;
      private CashReceiptStatus crsIdInterface;
      private CashReceiptStatus crsIdDeposited;
      private CashReceiptStatus crsIdForwarded;
      private CashReceiptStatus crsIdPended;
      private CashReceiptStatus crsIdDeleted;
    }

    /// <summary>A CrdsSystemIdGroup group.</summary>
    [Serializable]
    public class CrdsSystemIdGroup
    {
      /// <summary>
      /// A value of CrdsIdRecorded.
      /// </summary>
      [JsonPropertyName("crdsIdRecorded")]
      public CashReceiptDetailStatus CrdsIdRecorded
      {
        get => crdsIdRecorded ??= new();
        set => crdsIdRecorded = value;
      }

      /// <summary>
      /// A value of CrdsIdAdjusted.
      /// </summary>
      [JsonPropertyName("crdsIdAdjusted")]
      public CashReceiptDetailStatus CrdsIdAdjusted
      {
        get => crdsIdAdjusted ??= new();
        set => crdsIdAdjusted = value;
      }

      /// <summary>
      /// A value of CrdsIdSuspended.
      /// </summary>
      [JsonPropertyName("crdsIdSuspended")]
      public CashReceiptDetailStatus CrdsIdSuspended
      {
        get => crdsIdSuspended ??= new();
        set => crdsIdSuspended = value;
      }

      /// <summary>
      /// A value of CrdsIdDistributed.
      /// </summary>
      [JsonPropertyName("crdsIdDistributed")]
      public CashReceiptDetailStatus CrdsIdDistributed
      {
        get => crdsIdDistributed ??= new();
        set => crdsIdDistributed = value;
      }

      /// <summary>
      /// A value of CrdsIdRefunded.
      /// </summary>
      [JsonPropertyName("crdsIdRefunded")]
      public CashReceiptDetailStatus CrdsIdRefunded
      {
        get => crdsIdRefunded ??= new();
        set => crdsIdRefunded = value;
      }

      /// <summary>
      /// A value of CrdsIdReleased.
      /// </summary>
      [JsonPropertyName("crdsIdReleased")]
      public CashReceiptDetailStatus CrdsIdReleased
      {
        get => crdsIdReleased ??= new();
        set => crdsIdReleased = value;
      }

      /// <summary>
      /// A value of CrdsIdPended.
      /// </summary>
      [JsonPropertyName("crdsIdPended")]
      public CashReceiptDetailStatus CrdsIdPended
      {
        get => crdsIdPended ??= new();
        set => crdsIdPended = value;
      }

      private CashReceiptDetailStatus crdsIdRecorded;
      private CashReceiptDetailStatus crdsIdAdjusted;
      private CashReceiptDetailStatus crdsIdSuspended;
      private CashReceiptDetailStatus crdsIdDistributed;
      private CashReceiptDetailStatus crdsIdRefunded;
      private CashReceiptDetailStatus crdsIdReleased;
      private CashReceiptDetailStatus crdsIdPended;
    }

    /// <summary>A CrdshReasonCodeGroup group.</summary>
    [Serializable]
    public class CrdshReasonCodeGroup
    {
      /// <summary>
      /// A value of CrdshSupprsn.
      /// </summary>
      [JsonPropertyName("crdshSupprsn")]
      public CashReceiptDetailStatHistory CrdshSupprsn
      {
        get => crdshSupprsn ??= new();
        set => crdshSupprsn = value;
      }

      /// <summary>
      /// A value of CrdshSupcase.
      /// </summary>
      [JsonPropertyName("crdshSupcase")]
      public CashReceiptDetailStatHistory CrdshSupcase
      {
        get => crdshSupcase ??= new();
        set => crdshSupcase = value;
      }

      /// <summary>
      /// A value of CrdshSupcord.
      /// </summary>
      [JsonPropertyName("crdshSupcord")]
      public CashReceiptDetailStatHistory CrdshSupcord
      {
        get => crdshSupcord ??= new();
        set => crdshSupcord = value;
      }

      /// <summary>
      /// A value of CrdshSuprfnd.
      /// </summary>
      [JsonPropertyName("crdshSuprfnd")]
      public CashReceiptDetailStatHistory CrdshSuprfnd
      {
        get => crdshSuprfnd ??= new();
        set => crdshSuprfnd = value;
      }

      /// <summary>
      /// A value of CrdshSupnoob.
      /// </summary>
      [JsonPropertyName("crdshSupnoob")]
      public CashReceiptDetailStatHistory CrdshSupnoob
      {
        get => crdshSupnoob ??= new();
        set => crdshSupnoob = value;
      }

      /// <summary>
      /// A value of CrdshSupmnl.
      /// </summary>
      [JsonPropertyName("crdshSupmnl")]
      public CashReceiptDetailStatHistory CrdshSupmnl
      {
        get => crdshSupmnl ??= new();
        set => crdshSupmnl = value;
      }

      private CashReceiptDetailStatHistory crdshSupprsn;
      private CashReceiptDetailStatHistory crdshSupcase;
      private CashReceiptDetailStatHistory crdshSupcord;
      private CashReceiptDetailStatHistory crdshSuprfnd;
      private CashReceiptDetailStatHistory crdshSupnoob;
      private CashReceiptDetailStatHistory crdshSupmnl;
    }

    /// <summary>A CrtSystemIdGroup group.</summary>
    [Serializable]
    public class CrtSystemIdGroup
    {
      /// <summary>
      /// A value of CrtIdCheck.
      /// </summary>
      [JsonPropertyName("crtIdCheck")]
      public CashReceiptType CrtIdCheck
      {
        get => crtIdCheck ??= new();
        set => crtIdCheck = value;
      }

      /// <summary>
      /// A value of CrtIdFcrtRec.
      /// </summary>
      [JsonPropertyName("crtIdFcrtRec")]
      public CashReceiptType CrtIdFcrtRec
      {
        get => crtIdFcrtRec ??= new();
        set => crtIdFcrtRec = value;
      }

      /// <summary>
      /// A value of CrtIdMnyOrd.
      /// </summary>
      [JsonPropertyName("crtIdMnyOrd")]
      public CashReceiptType CrtIdMnyOrd
      {
        get => crtIdMnyOrd ??= new();
        set => crtIdMnyOrd = value;
      }

      /// <summary>
      /// A value of CrtIdCurrency.
      /// </summary>
      [JsonPropertyName("crtIdCurrency")]
      public CashReceiptType CrtIdCurrency
      {
        get => crtIdCurrency ??= new();
        set => crtIdCurrency = value;
      }

      /// <summary>
      /// A value of CrtIdCreditCrd.
      /// </summary>
      [JsonPropertyName("crtIdCreditCrd")]
      public CashReceiptType CrtIdCreditCrd
      {
        get => crtIdCreditCrd ??= new();
        set => crtIdCreditCrd = value;
      }

      /// <summary>
      /// A value of CrtIdEft.
      /// </summary>
      [JsonPropertyName("crtIdEft")]
      public CashReceiptType CrtIdEft
      {
        get => crtIdEft ??= new();
        set => crtIdEft = value;
      }

      /// <summary>
      /// A value of CrtIdFdirPmt.
      /// </summary>
      [JsonPropertyName("crtIdFdirPmt")]
      public CashReceiptType CrtIdFdirPmt
      {
        get => crtIdFdirPmt ??= new();
        set => crtIdFdirPmt = value;
      }

      /// <summary>
      /// A value of CrtIdCsenet.
      /// </summary>
      [JsonPropertyName("crtIdCsenet")]
      public CashReceiptType CrtIdCsenet
      {
        get => crtIdCsenet ??= new();
        set => crtIdCsenet = value;
      }

      /// <summary>
      /// A value of CrtIdCourtRecord.
      /// </summary>
      [JsonPropertyName("crtIdCourtRecord")]
      public CashReceiptType CrtIdCourtRecord
      {
        get => crtIdCourtRecord ??= new();
        set => crtIdCourtRecord = value;
      }

      /// <summary>
      /// A value of CrtIdInterfund.
      /// </summary>
      [JsonPropertyName("crtIdInterfund")]
      public CashReceiptType CrtIdInterfund
      {
        get => crtIdInterfund ??= new();
        set => crtIdInterfund = value;
      }

      /// <summary>
      /// A value of CrtIdRdirPmt.
      /// </summary>
      [JsonPropertyName("crtIdRdirPmt")]
      public CashReceiptType CrtIdRdirPmt
      {
        get => crtIdRdirPmt ??= new();
        set => crtIdRdirPmt = value;
      }

      /// <summary>
      /// A value of CrtIdManint.
      /// </summary>
      [JsonPropertyName("crtIdManint")]
      public CashReceiptType CrtIdManint
      {
        get => crtIdManint ??= new();
        set => crtIdManint = value;
      }

      /// <summary>
      /// A value of CrtIdRecap.
      /// </summary>
      [JsonPropertyName("crtIdRecap")]
      public CashReceiptType CrtIdRecap
      {
        get => crtIdRecap ??= new();
        set => crtIdRecap = value;
      }

      /// <summary>
      /// A value of CrtIdDirectPay.
      /// </summary>
      [JsonPropertyName("crtIdDirectPay")]
      public CashReceiptType CrtIdDirectPay
      {
        get => crtIdDirectPay ??= new();
        set => crtIdDirectPay = value;
      }

      /// <summary>
      /// A value of CrtIdCourtDisbursed.
      /// </summary>
      [JsonPropertyName("crtIdCourtDisbursed")]
      public CashReceiptType CrtIdCourtDisbursed
      {
        get => crtIdCourtDisbursed ??= new();
        set => crtIdCourtDisbursed = value;
      }

      private CashReceiptType crtIdCheck;
      private CashReceiptType crtIdFcrtRec;
      private CashReceiptType crtIdMnyOrd;
      private CashReceiptType crtIdCurrency;
      private CashReceiptType crtIdCreditCrd;
      private CashReceiptType crtIdEft;
      private CashReceiptType crtIdFdirPmt;
      private CashReceiptType crtIdCsenet;
      private CashReceiptType crtIdCourtRecord;
      private CashReceiptType crtIdInterfund;
      private CashReceiptType crtIdRdirPmt;
      private CashReceiptType crtIdManint;
      private CashReceiptType crtIdRecap;
      private CashReceiptType crtIdDirectPay;
      private CashReceiptType crtIdCourtDisbursed;
    }

    /// <summary>A CrtCategoryGroup group.</summary>
    [Serializable]
    public class CrtCategoryGroup
    {
      /// <summary>
      /// A value of CrtCatCash.
      /// </summary>
      [JsonPropertyName("crtCatCash")]
      public CashReceiptType CrtCatCash
      {
        get => crtCatCash ??= new();
        set => crtCatCash = value;
      }

      /// <summary>
      /// A value of CrtCatNonCash.
      /// </summary>
      [JsonPropertyName("crtCatNonCash")]
      public CashReceiptType CrtCatNonCash
      {
        get => crtCatNonCash ??= new();
        set => crtCatNonCash = value;
      }

      private CashReceiptType crtCatCash;
      private CashReceiptType crtCatNonCash;
    }

    /// <summary>A CollectionTypeGroup group.</summary>
    [Serializable]
    public class CollectionTypeGroup
    {
      /// <summary>
      /// A value of CtCash.
      /// </summary>
      [JsonPropertyName("ctCash")]
      public CollectionType CtCash
      {
        get => ctCash ??= new();
        set => ctCash = value;
      }

      /// <summary>
      /// A value of CtNonCash.
      /// </summary>
      [JsonPropertyName("ctNonCash")]
      public CollectionType CtNonCash
      {
        get => ctNonCash ??= new();
        set => ctNonCash = value;
      }

      private CollectionType ctCash;
      private CollectionType ctNonCash;
    }

    /// <summary>A CrdFeeTypeGroup group.</summary>
    [Serializable]
    public class CrdFeeTypeGroup
    {
      /// <summary>
      /// A value of CrdftSrsCostRec.
      /// </summary>
      [JsonPropertyName("crdftSrsCostRec")]
      public CashReceiptDetailFeeType CrdftSrsCostRec
      {
        get => crdftSrsCostRec ??= new();
        set => crdftSrsCostRec = value;
      }

      /// <summary>
      /// A value of CrdftCourt.
      /// </summary>
      [JsonPropertyName("crdftCourt")]
      public CashReceiptDetailFeeType CrdftCourt
      {
        get => crdftCourt ??= new();
        set => crdftCourt = value;
      }

      /// <summary>
      /// A value of CrdftMiscellaneous.
      /// </summary>
      [JsonPropertyName("crdftMiscellaneous")]
      public CashReceiptDetailFeeType CrdftMiscellaneous
      {
        get => crdftMiscellaneous ??= new();
        set => crdftMiscellaneous = value;
      }

      /// <summary>
      /// A value of CrdftOtherState.
      /// </summary>
      [JsonPropertyName("crdftOtherState")]
      public CashReceiptDetailFeeType CrdftOtherState
      {
        get => crdftOtherState ??= new();
        set => crdftOtherState = value;
      }

      /// <summary>
      /// A value of CrdftInterstate.
      /// </summary>
      [JsonPropertyName("crdftInterstate")]
      public CashReceiptDetailFeeType CrdftInterstate
      {
        get => crdftInterstate ??= new();
        set => crdftInterstate = value;
      }

      private CashReceiptDetailFeeType crdftSrsCostRec;
      private CashReceiptDetailFeeType crdftCourt;
      private CashReceiptDetailFeeType crdftMiscellaneous;
      private CashReceiptDetailFeeType crdftOtherState;
      private CashReceiptDetailFeeType crdftInterstate;
    }

    /// <summary>A CrCashBalanceReasonGroup group.</summary>
    [Serializable]
    public class CrCashBalanceReasonGroup
    {
      /// <summary>
      /// A value of CrUnder.
      /// </summary>
      [JsonPropertyName("crUnder")]
      public CashReceipt CrUnder
      {
        get => crUnder ??= new();
        set => crUnder = value;
      }

      /// <summary>
      /// A value of CrOver.
      /// </summary>
      [JsonPropertyName("crOver")]
      public CashReceipt CrOver
      {
        get => crOver ??= new();
        set => crOver = value;
      }

      private CashReceipt crUnder;
      private CashReceipt crOver;
    }

    /// <summary>A CrRlnRsnSystemIdGroup group.</summary>
    [Serializable]
    public class CrRlnRsnSystemIdGroup
    {
      /// <summary>
      /// A value of CrrrCourtInterface.
      /// </summary>
      [JsonPropertyName("crrrCourtInterface")]
      public CashReceiptRlnRsn CrrrCourtInterface
      {
        get => crrrCourtInterface ??= new();
        set => crrrCourtInterface = value;
      }

      private CashReceiptRlnRsn crrrCourtInterface;
    }

    /// <summary>
    /// A value of CrdTaxAdj.
    /// </summary>
    [JsonPropertyName("crdTaxAdj")]
    public CashReceiptDetailRlnRsn CrdTaxAdj
    {
      get => crdTaxAdj ??= new();
      set => crdTaxAdj = value;
    }

    /// <summary>
    /// Gets a value of CrsSystemId.
    /// </summary>
    [JsonPropertyName("crsSystemId")]
    public CrsSystemIdGroup CrsSystemId
    {
      get => crsSystemId ?? (crsSystemId = new());
      set => crsSystemId = value;
    }

    /// <summary>
    /// Gets a value of CrdsSystemId.
    /// </summary>
    [JsonPropertyName("crdsSystemId")]
    public CrdsSystemIdGroup CrdsSystemId
    {
      get => crdsSystemId ?? (crdsSystemId = new());
      set => crdsSystemId = value;
    }

    /// <summary>
    /// Gets a value of CrdshReasonCode.
    /// </summary>
    [JsonPropertyName("crdshReasonCode")]
    public CrdshReasonCodeGroup CrdshReasonCode
    {
      get => crdshReasonCode ?? (crdshReasonCode = new());
      set => crdshReasonCode = value;
    }

    /// <summary>
    /// Gets a value of CrtSystemId.
    /// </summary>
    [JsonPropertyName("crtSystemId")]
    public CrtSystemIdGroup CrtSystemId
    {
      get => crtSystemId ?? (crtSystemId = new());
      set => crtSystemId = value;
    }

    /// <summary>
    /// Gets a value of CrtCategory.
    /// </summary>
    [JsonPropertyName("crtCategory")]
    public CrtCategoryGroup CrtCategory
    {
      get => crtCategory ?? (crtCategory = new());
      set => crtCategory = value;
    }

    /// <summary>
    /// Gets a value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionTypeGroup CollectionType
    {
      get => collectionType ?? (collectionType = new());
      set => collectionType = value;
    }

    /// <summary>
    /// Gets a value of CrdFeeType.
    /// </summary>
    [JsonPropertyName("crdFeeType")]
    public CrdFeeTypeGroup CrdFeeType
    {
      get => crdFeeType ?? (crdFeeType = new());
      set => crdFeeType = value;
    }

    /// <summary>
    /// Gets a value of CrCashBalanceReason.
    /// </summary>
    [JsonPropertyName("crCashBalanceReason")]
    public CrCashBalanceReasonGroup CrCashBalanceReason
    {
      get => crCashBalanceReason ?? (crCashBalanceReason = new());
      set => crCashBalanceReason = value;
    }

    /// <summary>
    /// Gets a value of CrRlnRsnSystemId.
    /// </summary>
    [JsonPropertyName("crRlnRsnSystemId")]
    public CrRlnRsnSystemIdGroup CrRlnRsnSystemId
    {
      get => crRlnRsnSystemId ?? (crRlnRsnSystemId = new());
      set => crRlnRsnSystemId = value;
    }

    private CashReceiptDetailRlnRsn crdTaxAdj;
    private CrsSystemIdGroup crsSystemId;
    private CrdsSystemIdGroup crdsSystemId;
    private CrdshReasonCodeGroup crdshReasonCode;
    private CrtSystemIdGroup crtSystemId;
    private CrtCategoryGroup crtCategory;
    private CollectionTypeGroup collectionType;
    private CrdFeeTypeGroup crdFeeType;
    private CrCashBalanceReasonGroup crCashBalanceReason;
    private CrRlnRsnSystemIdGroup crRlnRsnSystemId;
  }
#endregion
}
