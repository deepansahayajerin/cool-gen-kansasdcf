// Program: FN_LOG_CASH_RCPT_DTL_HISTORY, ID: 371769284, model: 746.
// Short name: SWE00501
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_LOG_CASH_RCPT_DTL_HISTORY.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// his action block will log a before image of a cash receipt detail prior to 
/// it being updated or deleted.
/// </para>
/// </summary>
[Serializable]
public partial class FnLogCashRcptDtlHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_LOG_CASH_RCPT_DTL_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnLogCashRcptDtlHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnLogCashRcptDtlHistory.
  /// </summary>
  public FnLogCashRcptDtlHistory(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************************
    // DATE		PROGRAMMER			DESCRIPTION
    // ----------------------------------------------------------------------
    // 09/25/98	Sunya Sharp
    // Make changes per screen assessment for CRCN signed 9/14/98. Added 
    // missing set statements from create.  Added views for the missing
    // statements.  Changed the created by, created timestamp, last updated
    // by and last updated timestamp to be what is on the cash receipt
    // detail before the update occured on the cash receipt detail.
    // *********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    try
    {
      CreateCashReceiptDetailHistory();
      export.CashReceiptDetailHistory.Assign(entities.CashReceiptDetailHistory);
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
    var lastUpdatedTmst = import.CashReceiptDetailHistory.LastUpdatedTmst;
    var interfaceTransId = import.CashReceiptDetailHistory.InterfaceTransId ?? ""
      ;
    var offsetTaxid =
      import.CashReceiptDetailHistory.OffsetTaxid.GetValueOrDefault();
    var jointReturnInd = import.CashReceiptDetailHistory.JointReturnInd ?? "";
    var jointReturnName = import.CashReceiptDetailHistory.JointReturnName ?? "";
    var refundedAmount =
      import.CashReceiptDetailHistory.RefundedAmount.GetValueOrDefault();
    var distributedAmount =
      import.CashReceiptDetailHistory.DistributedAmount.GetValueOrDefault();
    var adjustmentInd = import.CashReceiptDetailHistory.AdjustmentInd ?? "";
    var sequentialIdentifier =
      import.CashReceiptDetailHistory.SequentialIdentifier.GetValueOrDefault();
    var attribute2SupportedPersonFirstName =
      import.CashReceiptDetailHistory.Attribute2SupportedPersonFirstName ?? "";
    var attribute2SupportedPersonLastName =
      import.CashReceiptDetailHistory.Attribute2SupportedPersonLastName ?? "";
    var attribute2SupportedPersonMiddleName =
      import.CashReceiptDetailHistory.Attribute2SupportedPersonMiddleName ?? ""
      ;
    var collectionTypeIdentifier =
      import.CashReceiptDetailHistory.CollectionTypeIdentifier;
    var cashReceiptEventNumber =
      import.CashReceiptDetailHistory.CashReceiptEventNumber;
    var cashReceiptNumber = import.CashReceiptDetailHistory.CashReceiptNumber;
    var collectionDate = import.CashReceiptDetailHistory.CollectionDate;
    var obligorPersonNumber =
      import.CashReceiptDetailHistory.ObligorPersonNumber ?? "";
    var courtOrderNumber = import.CashReceiptDetailHistory.CourtOrderNumber ?? ""
      ;
    var caseNumber = import.CashReceiptDetailHistory.CaseNumber ?? "";
    var obligorFirstName = import.CashReceiptDetailHistory.ObligorFirstName ?? ""
      ;
    var obligorLastName = import.CashReceiptDetailHistory.ObligorLastName ?? "";
    var obligorMiddleName =
      import.CashReceiptDetailHistory.ObligorMiddleName ?? "";
    var obligorPhoneNumber =
      import.CashReceiptDetailHistory.ObligorPhoneNumber ?? "";
    var obligorSocialSecurityNumber =
      import.CashReceiptDetailHistory.ObligorSocialSecurityNumber ?? "";
    var offsetTaxYear =
      import.CashReceiptDetailHistory.OffsetTaxYear.GetValueOrDefault();
    var defaultedCollectionDateInd =
      import.CashReceiptDetailHistory.DefaultedCollectionDateInd ?? "";
    var multiPayor = import.CashReceiptDetailHistory.MultiPayor ?? "";
    var receivedAmount =
      import.CashReceiptDetailHistory.ReceivedAmount.GetValueOrDefault();
    var collectionAmount =
      import.CashReceiptDetailHistory.CollectionAmount.GetValueOrDefault();
    var payeeFirstName = import.CashReceiptDetailHistory.PayeeFirstName ?? "";
    var payeeMiddleName = import.CashReceiptDetailHistory.PayeeMiddleName ?? "";
    var payeeLastName = import.CashReceiptDetailHistory.PayeeLastName ?? "";
    var attribute1SupportedPersonFirstName =
      import.CashReceiptDetailHistory.Attribute1SupportedPersonFirstName ?? "";
    var attribute1SupportedPersonMiddleName =
      import.CashReceiptDetailHistory.Attribute1SupportedPersonMiddleName ?? ""
      ;
    var attribute1SupportedPersonLastName =
      import.CashReceiptDetailHistory.Attribute1SupportedPersonLastName ?? "";
    var createdBy = import.CashReceiptDetailHistory.CreatedBy;
    var createdTmst = import.CashReceiptDetailHistory.CreatedTmst;
    var lastUpdatedBy = import.CashReceiptDetailHistory.LastUpdatedBy;
    var collectionAmtFullyAppliedInd =
      import.CashReceiptDetailHistory.CollectionAmtFullyAppliedInd ?? "";
    var cashReceiptType = import.CashReceiptDetailHistory.CashReceiptType;
    var cashReceiptSourceType =
      import.CashReceiptDetailHistory.CashReceiptSourceType;
    var reference = import.CashReceiptDetailHistory.Reference ?? "";
    var notes = import.CashReceiptDetailHistory.Notes ?? "";

    CheckValid<CashReceiptDetailHistory>("JointReturnInd", jointReturnInd);
    CheckValid<CashReceiptDetailHistory>("DefaultedCollectionDateInd",
      defaultedCollectionDateInd);
    CheckValid<CashReceiptDetailHistory>("MultiPayor", multiPayor);
    CheckValid<CashReceiptDetailHistory>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.CashReceiptDetailHistory.Populated = false;
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
        db.SetInt32(command, "cashRecType", cashReceiptType);
        db.SetInt32(command, "cashRecSrcType", cashReceiptSourceType);
        db.SetNullableString(command, "referenc", reference);
        db.SetNullableString(command, "notes", notes);
      });

    entities.CashReceiptDetailHistory.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetailHistory.InterfaceTransId = interfaceTransId;
    entities.CashReceiptDetailHistory.OffsetTaxid = offsetTaxid;
    entities.CashReceiptDetailHistory.JointReturnInd = jointReturnInd;
    entities.CashReceiptDetailHistory.JointReturnName = jointReturnName;
    entities.CashReceiptDetailHistory.RefundedAmount = refundedAmount;
    entities.CashReceiptDetailHistory.DistributedAmount = distributedAmount;
    entities.CashReceiptDetailHistory.AdjustmentInd = adjustmentInd;
    entities.CashReceiptDetailHistory.SequentialIdentifier =
      sequentialIdentifier;
    entities.CashReceiptDetailHistory.Attribute2SupportedPersonFirstName =
      attribute2SupportedPersonFirstName;
    entities.CashReceiptDetailHistory.Attribute2SupportedPersonLastName =
      attribute2SupportedPersonLastName;
    entities.CashReceiptDetailHistory.Attribute2SupportedPersonMiddleName =
      attribute2SupportedPersonMiddleName;
    entities.CashReceiptDetailHistory.CollectionTypeIdentifier =
      collectionTypeIdentifier;
    entities.CashReceiptDetailHistory.CashReceiptEventNumber =
      cashReceiptEventNumber;
    entities.CashReceiptDetailHistory.CashReceiptNumber = cashReceiptNumber;
    entities.CashReceiptDetailHistory.CollectionDate = collectionDate;
    entities.CashReceiptDetailHistory.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetailHistory.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetailHistory.CaseNumber = caseNumber;
    entities.CashReceiptDetailHistory.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetailHistory.ObligorLastName = obligorLastName;
    entities.CashReceiptDetailHistory.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetailHistory.ObligorPhoneNumber = obligorPhoneNumber;
    entities.CashReceiptDetailHistory.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetailHistory.OffsetTaxYear = offsetTaxYear;
    entities.CashReceiptDetailHistory.DefaultedCollectionDateInd =
      defaultedCollectionDateInd;
    entities.CashReceiptDetailHistory.MultiPayor = multiPayor;
    entities.CashReceiptDetailHistory.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetailHistory.CollectionAmount = collectionAmount;
    entities.CashReceiptDetailHistory.PayeeFirstName = payeeFirstName;
    entities.CashReceiptDetailHistory.PayeeMiddleName = payeeMiddleName;
    entities.CashReceiptDetailHistory.PayeeLastName = payeeLastName;
    entities.CashReceiptDetailHistory.Attribute1SupportedPersonFirstName =
      attribute1SupportedPersonFirstName;
    entities.CashReceiptDetailHistory.Attribute1SupportedPersonMiddleName =
      attribute1SupportedPersonMiddleName;
    entities.CashReceiptDetailHistory.Attribute1SupportedPersonLastName =
      attribute1SupportedPersonLastName;
    entities.CashReceiptDetailHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailHistory.CreatedTmst = createdTmst;
    entities.CashReceiptDetailHistory.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetailHistory.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
    entities.CashReceiptDetailHistory.CashReceiptType = cashReceiptType;
    entities.CashReceiptDetailHistory.CashReceiptSourceType =
      cashReceiptSourceType;
    entities.CashReceiptDetailHistory.Reference = reference;
    entities.CashReceiptDetailHistory.Notes = notes;
    entities.CashReceiptDetailHistory.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
#endregion
}
