// Program: FN_AB_RECORD_CR_DETAIL_HISTORY, ID: 372566725, model: 746.
// Short name: SWE02207
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AB_RECORD_CR_DETAIL_HISTORY.
/// </summary>
[Serializable]
public partial class FnAbRecordCrDetailHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_RECORD_CR_DETAIL_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbRecordCrDetailHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbRecordCrDetailHistory.
  /// </summary>
  public FnAbRecordCrDetailHistory(IContext context, Import import,
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
    // Read cash receipt detail record that will be updated.
    // --------------------------------------------------------------------
    if (!ReadCashReceiptDetail())
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";

      return;
    }

    // --------------------------------------------------------------------
    // Set all history attributes to the current cash receipt detail
    // attributes.
    // --------------------------------------------------------------------
    local.CashReceiptDetailHistory.CashReceiptSourceType =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    local.CashReceiptDetailHistory.CashReceiptEventNumber =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    local.CashReceiptDetailHistory.CashReceiptType =
      import.CashReceiptType.SystemGeneratedIdentifier;
    local.CashReceiptDetailHistory.CashReceiptNumber =
      import.CashReceipt.SequentialNumber;
    local.CashReceiptDetailHistory.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    local.CashReceiptDetailHistory.CollectionTypeIdentifier =
      import.CollectionType.SequentialIdentifier;
    local.CashReceiptDetailHistory.InterfaceTransId =
      entities.EavCashReceiptDetail.InterfaceTransId;
    local.CashReceiptDetailHistory.CourtOrderNumber =
      entities.EavCashReceiptDetail.CourtOrderNumber;
    local.CashReceiptDetailHistory.CaseNumber =
      entities.EavCashReceiptDetail.CaseNumber;
    local.CashReceiptDetailHistory.CollectionDate =
      entities.EavCashReceiptDetail.CollectionDate;
    local.CashReceiptDetailHistory.ReceivedAmount =
      entities.EavCashReceiptDetail.ReceivedAmount;
    local.CashReceiptDetailHistory.CollectionAmount =
      entities.EavCashReceiptDetail.CollectionAmount;
    local.CashReceiptDetailHistory.DistributedAmount =
      entities.EavCashReceiptDetail.DistributedAmount;
    local.CashReceiptDetailHistory.RefundedAmount =
      entities.EavCashReceiptDetail.RefundedAmount;
    local.CashReceiptDetailHistory.MultiPayor =
      entities.EavCashReceiptDetail.MultiPayor;
    local.CashReceiptDetailHistory.CollectionAmtFullyAppliedInd =
      entities.EavCashReceiptDetail.CollectionAmtFullyAppliedInd;
    local.CashReceiptDetailHistory.AdjustmentInd =
      entities.EavCashReceiptDetail.AdjustmentInd;
    local.CashReceiptDetailHistory.DefaultedCollectionDateInd =
      entities.EavCashReceiptDetail.DefaultedCollectionDateInd;
    local.CashReceiptDetailHistory.ObligorPersonNumber =
      entities.EavCashReceiptDetail.ObligorPersonNumber;
    local.CashReceiptDetailHistory.ObligorSocialSecurityNumber =
      entities.EavCashReceiptDetail.ObligorSocialSecurityNumber;
    local.CashReceiptDetailHistory.ObligorPhoneNumber =
      entities.EavCashReceiptDetail.ObligorPhoneNumber;
    local.CashReceiptDetailHistory.ObligorFirstName =
      entities.EavCashReceiptDetail.ObligorFirstName;
    local.CashReceiptDetailHistory.ObligorMiddleName =
      entities.EavCashReceiptDetail.ObligorMiddleName;
    local.CashReceiptDetailHistory.ObligorLastName =
      entities.EavCashReceiptDetail.ObligorLastName;
    local.CashReceiptDetailHistory.PayeeFirstName =
      entities.EavCashReceiptDetail.PayeeFirstName;
    local.CashReceiptDetailHistory.PayeeMiddleName =
      entities.EavCashReceiptDetail.PayeeMiddleName;
    local.CashReceiptDetailHistory.PayeeLastName =
      entities.EavCashReceiptDetail.PayeeLastName;
    local.CashReceiptDetailHistory.Attribute1SupportedPersonFirstName =
      entities.EavCashReceiptDetail.Attribute1SupportedPersonFirstName;
    local.CashReceiptDetailHistory.Attribute1SupportedPersonMiddleName =
      entities.EavCashReceiptDetail.Attribute1SupportedPersonMiddleName;
    local.CashReceiptDetailHistory.Attribute1SupportedPersonLastName =
      entities.EavCashReceiptDetail.Attribute1SupportedPersonLastName;
    local.CashReceiptDetailHistory.Attribute2SupportedPersonFirstName =
      entities.EavCashReceiptDetail.Attribute2SupportedPersonFirstName;
    local.CashReceiptDetailHistory.Attribute2SupportedPersonMiddleName =
      entities.EavCashReceiptDetail.Attribute2SupportedPersonMiddleName;
    local.CashReceiptDetailHistory.Attribute2SupportedPersonLastName =
      entities.EavCashReceiptDetail.Attribute2SupportedPersonLastName;
    local.CashReceiptDetailHistory.OffsetTaxYear =
      entities.EavCashReceiptDetail.OffsetTaxYear;
    local.CashReceiptDetailHistory.OffsetTaxid =
      entities.EavCashReceiptDetail.OffsetTaxid;
    local.CashReceiptDetailHistory.JointReturnInd =
      entities.EavCashReceiptDetail.JointReturnInd;
    local.CashReceiptDetailHistory.JointReturnName =
      entities.EavCashReceiptDetail.JointReturnName;
    local.CashReceiptDetailHistory.Notes = entities.EavCashReceiptDetail.Notes;
    local.CashReceiptDetailHistory.Reference =
      entities.EavCashReceiptDetail.Reference;
    local.CashReceiptDetailHistory.CreatedBy =
      entities.EavCashReceiptDetail.CreatedBy;
    local.CashReceiptDetailHistory.CreatedTmst =
      entities.EavCashReceiptDetail.CreatedTmst;

    if (IsEmpty(entities.EavCashReceiptDetail.LastUpdatedBy))
    {
      local.CashReceiptDetailHistory.LastUpdatedBy =
        entities.EavCashReceiptDetail.CreatedBy;
      local.CashReceiptDetailHistory.LastUpdatedTmst =
        entities.EavCashReceiptDetail.CreatedTmst;
    }
    else
    {
      local.CashReceiptDetailHistory.LastUpdatedBy =
        entities.EavCashReceiptDetail.LastUpdatedBy ?? Spaces(8);
      local.CashReceiptDetailHistory.LastUpdatedTmst =
        entities.EavCashReceiptDetail.LastUpdatedTmst;
    }

    // ------------------------------------------------------------------
    // Create new detail history record with current values from the
    // detail record to be updated.
    // ------------------------------------------------------------------
    try
    {
      CreateCashReceiptDetailHistory();
      ++export.ImportNumberOfUpdates.Count;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0049_CASH_RCPT_DTL_HIST_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0050_CASH_RCPT_DTL_HIST_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateCashReceiptDetailHistory()
  {
    var lastUpdatedTmst = local.CashReceiptDetailHistory.LastUpdatedTmst;
    var interfaceTransId = local.CashReceiptDetailHistory.InterfaceTransId ?? ""
      ;
    var offsetTaxid =
      local.CashReceiptDetailHistory.OffsetTaxid.GetValueOrDefault();
    var jointReturnInd = local.CashReceiptDetailHistory.JointReturnInd ?? "";
    var jointReturnName = local.CashReceiptDetailHistory.JointReturnName ?? "";
    var refundedAmount =
      local.CashReceiptDetailHistory.RefundedAmount.GetValueOrDefault();
    var distributedAmount =
      local.CashReceiptDetailHistory.DistributedAmount.GetValueOrDefault();
    var adjustmentInd = local.CashReceiptDetailHistory.AdjustmentInd ?? "";
    var sequentialIdentifier =
      local.CashReceiptDetailHistory.SequentialIdentifier.GetValueOrDefault();
    var attribute2SupportedPersonFirstName =
      local.CashReceiptDetailHistory.Attribute2SupportedPersonFirstName ?? "";
    var attribute2SupportedPersonLastName =
      local.CashReceiptDetailHistory.Attribute2SupportedPersonLastName ?? "";
    var attribute2SupportedPersonMiddleName =
      local.CashReceiptDetailHistory.Attribute2SupportedPersonMiddleName ?? "";
    var collectionTypeIdentifier =
      local.CashReceiptDetailHistory.CollectionTypeIdentifier;
    var cashReceiptEventNumber =
      local.CashReceiptDetailHistory.CashReceiptEventNumber;
    var cashReceiptNumber = local.CashReceiptDetailHistory.CashReceiptNumber;
    var collectionDate = local.CashReceiptDetailHistory.CollectionDate;
    var obligorPersonNumber =
      local.CashReceiptDetailHistory.ObligorPersonNumber ?? "";
    var courtOrderNumber = local.CashReceiptDetailHistory.CourtOrderNumber ?? ""
      ;
    var caseNumber = local.CashReceiptDetailHistory.CaseNumber ?? "";
    var obligorFirstName = local.CashReceiptDetailHistory.ObligorFirstName ?? ""
      ;
    var obligorLastName = local.CashReceiptDetailHistory.ObligorLastName ?? "";
    var obligorMiddleName =
      local.CashReceiptDetailHistory.ObligorMiddleName ?? "";
    var obligorPhoneNumber =
      local.CashReceiptDetailHistory.ObligorPhoneNumber ?? "";
    var obligorSocialSecurityNumber =
      local.CashReceiptDetailHistory.ObligorSocialSecurityNumber ?? "";
    var offsetTaxYear =
      local.CashReceiptDetailHistory.OffsetTaxYear.GetValueOrDefault();
    var defaultedCollectionDateInd =
      local.CashReceiptDetailHistory.DefaultedCollectionDateInd ?? "";
    var multiPayor = local.CashReceiptDetailHistory.MultiPayor ?? "";
    var receivedAmount =
      local.CashReceiptDetailHistory.ReceivedAmount.GetValueOrDefault();
    var collectionAmount =
      local.CashReceiptDetailHistory.CollectionAmount.GetValueOrDefault();
    var payeeFirstName = local.CashReceiptDetailHistory.PayeeFirstName ?? "";
    var payeeMiddleName = local.CashReceiptDetailHistory.PayeeMiddleName ?? "";
    var payeeLastName = local.CashReceiptDetailHistory.PayeeLastName ?? "";
    var attribute1SupportedPersonFirstName =
      local.CashReceiptDetailHistory.Attribute1SupportedPersonFirstName ?? "";
    var attribute1SupportedPersonMiddleName =
      local.CashReceiptDetailHistory.Attribute1SupportedPersonMiddleName ?? "";
    var attribute1SupportedPersonLastName =
      local.CashReceiptDetailHistory.Attribute1SupportedPersonLastName ?? "";
    var createdBy = local.CashReceiptDetailHistory.CreatedBy;
    var createdTmst = local.CashReceiptDetailHistory.CreatedTmst;
    var lastUpdatedBy = local.CashReceiptDetailHistory.LastUpdatedBy;
    var collectionAmtFullyAppliedInd =
      local.CashReceiptDetailHistory.CollectionAmtFullyAppliedInd ?? "";
    var cashReceiptType1 = local.CashReceiptDetailHistory.CashReceiptType;
    var cashReceiptSourceType1 =
      local.CashReceiptDetailHistory.CashReceiptSourceType;
    var reference = local.CashReceiptDetailHistory.Reference ?? "";
    var notes = local.CashReceiptDetailHistory.Notes ?? "";

    CheckValid<CashReceiptDetailHistory>("JointReturnInd", jointReturnInd);
    CheckValid<CashReceiptDetailHistory>("DefaultedCollectionDateInd",
      defaultedCollectionDateInd);
    CheckValid<CashReceiptDetailHistory>("MultiPayor", multiPayor);
    CheckValid<CashReceiptDetailHistory>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.New1.Populated = false;
    Update("CreateCashReceiptDetailHistory",
      (db, command) =>
      {
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "interfaceTransId", interfaceTransId);
        db.SetNullableInt32(command, "offsetTaxid", offsetTaxid);
        db.SetNullableString(command, "jointReturnInd", jointReturnInd);
        db.SetNullableString(command, "jointReturnName", jointReturnName);
        db.SetNullableDecimal(command, "refundedAmount", refundedAmount);
        db.SetNullableDecimal(command, "distributedAmount", distributedAmount);
        db.SetNullableString(command, "adjustmentInd", adjustmentInd);
        db.SetNullableInt32(command, "crdetailHistId", sequentialIdentifier);
        db.SetNullableString(
          command, "suppPrsnFn2", attribute2SupportedPersonFirstName);
        db.SetNullableString(
          command, "suppPrsnLn2", attribute2SupportedPersonLastName);
        db.SetNullableString(
          command, "suppPrsnMn2", attribute2SupportedPersonMiddleName);
        db.SetInt32(command, "collctTypeId", collectionTypeIdentifier);
        db.SetInt32(command, "creventNbrId", cashReceiptEventNumber);
        db.SetInt32(command, "crNbrId", cashReceiptNumber);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "oblgorPersNbrId", obligorPersonNumber);
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMiddleNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", obligorPhoneNumber);
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableInt32(command, "offsetTaxYear", offsetTaxYear);
        db.SetNullableString(
          command, "dfltCllctnDtInd", defaultedCollectionDateInd);
        db.SetNullableString(command, "multiPayor", multiPayor);
        db.SetNullableDecimal(command, "receivedAmount", receivedAmount);
        db.SetNullableDecimal(command, "collectionAmount", collectionAmount);
        db.SetNullableString(command, "payeeFirstName", payeeFirstName);
        db.SetNullableString(command, "payeeMiddleName", payeeMiddleName);
        db.SetNullableString(command, "payeeLastName", payeeLastName);
        db.SetNullableString(
          command, "suppPrsnFn1", attribute1SupportedPersonFirstName);
        db.SetNullableString(
          command, "supPrsnMidNm1", attribute1SupportedPersonMiddleName);
        db.SetNullableString(
          command, "supPrsnLstNm1", attribute1SupportedPersonLastName);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(
          command, "collectionAmtFul", collectionAmtFullyAppliedInd);
        db.SetInt32(command, "cashRecType", cashReceiptType1);
        db.SetInt32(command, "cashRecSrcType", cashReceiptSourceType1);
        db.SetNullableString(command, "referenc", reference);
        db.SetNullableString(command, "notes", notes);
      });

    entities.New1.LastUpdatedTmst = lastUpdatedTmst;
    entities.New1.InterfaceTransId = interfaceTransId;
    entities.New1.OffsetTaxid = offsetTaxid;
    entities.New1.JointReturnInd = jointReturnInd;
    entities.New1.JointReturnName = jointReturnName;
    entities.New1.RefundedAmount = refundedAmount;
    entities.New1.DistributedAmount = distributedAmount;
    entities.New1.AdjustmentInd = adjustmentInd;
    entities.New1.SequentialIdentifier = sequentialIdentifier;
    entities.New1.Attribute2SupportedPersonFirstName =
      attribute2SupportedPersonFirstName;
    entities.New1.Attribute2SupportedPersonLastName =
      attribute2SupportedPersonLastName;
    entities.New1.Attribute2SupportedPersonMiddleName =
      attribute2SupportedPersonMiddleName;
    entities.New1.CollectionTypeIdentifier = collectionTypeIdentifier;
    entities.New1.CashReceiptEventNumber = cashReceiptEventNumber;
    entities.New1.CashReceiptNumber = cashReceiptNumber;
    entities.New1.CollectionDate = collectionDate;
    entities.New1.ObligorPersonNumber = obligorPersonNumber;
    entities.New1.CourtOrderNumber = courtOrderNumber;
    entities.New1.CaseNumber = caseNumber;
    entities.New1.ObligorFirstName = obligorFirstName;
    entities.New1.ObligorLastName = obligorLastName;
    entities.New1.ObligorMiddleName = obligorMiddleName;
    entities.New1.ObligorPhoneNumber = obligorPhoneNumber;
    entities.New1.ObligorSocialSecurityNumber = obligorSocialSecurityNumber;
    entities.New1.OffsetTaxYear = offsetTaxYear;
    entities.New1.DefaultedCollectionDateInd = defaultedCollectionDateInd;
    entities.New1.MultiPayor = multiPayor;
    entities.New1.ReceivedAmount = receivedAmount;
    entities.New1.CollectionAmount = collectionAmount;
    entities.New1.PayeeFirstName = payeeFirstName;
    entities.New1.PayeeMiddleName = payeeMiddleName;
    entities.New1.PayeeLastName = payeeLastName;
    entities.New1.Attribute1SupportedPersonFirstName =
      attribute1SupportedPersonFirstName;
    entities.New1.Attribute1SupportedPersonMiddleName =
      attribute1SupportedPersonMiddleName;
    entities.New1.Attribute1SupportedPersonLastName =
      attribute1SupportedPersonLastName;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.LastUpdatedBy = lastUpdatedBy;
    entities.New1.CollectionAmtFullyAppliedInd = collectionAmtFullyAppliedInd;
    entities.New1.CashReceiptType = cashReceiptType1;
    entities.New1.CashReceiptSourceType = cashReceiptSourceType1;
    entities.New1.Reference = reference;
    entities.New1.Notes = notes;
    entities.New1.Populated = true;
  }

  private bool ReadCashReceiptDetail()
  {
    entities.EavCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.EavCashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.EavCashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.EavCashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.EavCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.EavCashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.EavCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.EavCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.EavCashReceiptDetail.CaseNumber =
          db.GetNullableString(reader, 7);
        entities.EavCashReceiptDetail.OffsetTaxid =
          db.GetNullableInt32(reader, 8);
        entities.EavCashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 9);
        entities.EavCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 10);
        entities.EavCashReceiptDetail.CollectionDate = db.GetDate(reader, 11);
        entities.EavCashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 12);
        entities.EavCashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 13);
        entities.EavCashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 14);
        entities.EavCashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 15);
        entities.EavCashReceiptDetail.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 16);
        entities.EavCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.EavCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.EavCashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 19);
        entities.EavCashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 20);
        entities.EavCashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 21);
        entities.EavCashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 22);
        entities.EavCashReceiptDetail.PayeeFirstName =
          db.GetNullableString(reader, 23);
        entities.EavCashReceiptDetail.PayeeMiddleName =
          db.GetNullableString(reader, 24);
        entities.EavCashReceiptDetail.PayeeLastName =
          db.GetNullableString(reader, 25);
        entities.EavCashReceiptDetail.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 26);
        entities.EavCashReceiptDetail.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 27);
        entities.EavCashReceiptDetail.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 28);
        entities.EavCashReceiptDetail.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 29);
        entities.EavCashReceiptDetail.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 30);
        entities.EavCashReceiptDetail.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 31);
        entities.EavCashReceiptDetail.CreatedBy = db.GetString(reader, 32);
        entities.EavCashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 33);
        entities.EavCashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.EavCashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 35);
        entities.EavCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.EavCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 37);
        entities.EavCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 38);
        entities.EavCashReceiptDetail.SuppPersonNoForVol =
          db.GetNullableString(reader, 39);
        entities.EavCashReceiptDetail.Reference =
          db.GetNullableString(reader, 40);
        entities.EavCashReceiptDetail.Notes = db.GetNullableString(reader, 41);
        entities.EavCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.EavCashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.EavCashReceiptDetail.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.EavCashReceiptDetail.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.EavCashReceiptDetail.CollectionAmtFullyAppliedInd);
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CashReceiptDetailHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailHistory")]
    public CashReceiptDetailHistory CashReceiptDetailHistory
    {
      get => cashReceiptDetailHistory ??= new();
      set => cashReceiptDetailHistory = value;
    }

    private CashReceiptDetailHistory cashReceiptDetailHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EavCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("eavCashReceiptSourceType")]
    public CashReceiptSourceType EavCashReceiptSourceType
    {
      get => eavCashReceiptSourceType ??= new();
      set => eavCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of EavCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("eavCashReceiptEvent")]
    public CashReceiptEvent EavCashReceiptEvent
    {
      get => eavCashReceiptEvent ??= new();
      set => eavCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of EavCashReceiptType.
    /// </summary>
    [JsonPropertyName("eavCashReceiptType")]
    public CashReceiptType EavCashReceiptType
    {
      get => eavCashReceiptType ??= new();
      set => eavCashReceiptType = value;
    }

    /// <summary>
    /// A value of EavCashReceipt.
    /// </summary>
    [JsonPropertyName("eavCashReceipt")]
    public CashReceipt EavCashReceipt
    {
      get => eavCashReceipt ??= new();
      set => eavCashReceipt = value;
    }

    /// <summary>
    /// A value of EavCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("eavCashReceiptDetail")]
    public CashReceiptDetail EavCashReceiptDetail
    {
      get => eavCashReceiptDetail ??= new();
      set => eavCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptDetailHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CashReceiptSourceType eavCashReceiptSourceType;
    private CashReceiptEvent eavCashReceiptEvent;
    private CashReceiptType eavCashReceiptType;
    private CashReceipt eavCashReceipt;
    private CashReceiptDetail eavCashReceiptDetail;
    private CashReceiptDetailHistory new1;
  }
#endregion
}
