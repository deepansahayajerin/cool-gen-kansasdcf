// Program: FN_UPDATE_CR_DETAIL, ID: 371770210, model: 746.
// Short name: SWE02039
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_CR_DETAIL.
/// </summary>
[Serializable]
public partial class FnUpdateCrDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_CR_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateCrDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateCrDetail.
  /// </summary>
  public FnUpdateCrDetail(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ---------------------------------------------------------------------------------------
    // Date 	  	Developer Name		Request #	Description
    // 10/01/98	Sunya Sharp				Make changes per screen assessment signed 9/22/
    // 98.  Remove unnecessary reads.  Add logic to update the history table
    // when any fields are successfully updated.  Add missing attributes from
    // the update statement.  Changed FDSO information to be display only.
    // Removed fields from the update statement.
    // ---------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadCashReceiptDetail())
    {
      local.CashReceiptDetail.Assign(entities.CashReceiptDetail);

      try
      {
        UpdateCashReceiptDetail();
        local.CashReceiptDetailHistory.Attribute1SupportedPersonFirstName =
          local.CashReceiptDetail.Attribute1SupportedPersonFirstName ?? "";
        local.CashReceiptDetailHistory.Attribute1SupportedPersonLastName =
          local.CashReceiptDetail.Attribute1SupportedPersonLastName ?? "";
        local.CashReceiptDetailHistory.Attribute1SupportedPersonMiddleName =
          local.CashReceiptDetail.Attribute1SupportedPersonMiddleName ?? "";
        local.CashReceiptDetailHistory.Attribute2SupportedPersonFirstName =
          local.CashReceiptDetail.Attribute2SupportedPersonFirstName ?? "";
        local.CashReceiptDetailHistory.Attribute2SupportedPersonLastName =
          local.CashReceiptDetail.Attribute2SupportedPersonLastName ?? "";
        local.CashReceiptDetailHistory.Attribute2SupportedPersonMiddleName =
          local.CashReceiptDetail.Attribute2SupportedPersonMiddleName ?? "";
        local.CashReceiptDetailHistory.AdjustmentInd =
          local.CashReceiptDetail.AdjustmentInd ?? "";
        local.CashReceiptDetailHistory.CaseNumber =
          local.CashReceiptDetail.CaseNumber ?? "";
        local.CashReceiptDetailHistory.CashReceiptEventNumber =
          import.CashReceiptEvent.SystemGeneratedIdentifier;
        local.CashReceiptDetailHistory.CashReceiptSourceType =
          import.CashReceiptSourceType.SystemGeneratedIdentifier;
        local.CashReceiptDetailHistory.CashReceiptType =
          import.CashReceiptType.SystemGeneratedIdentifier;
        local.CashReceiptDetailHistory.CashReceiptNumber =
          import.CashReceipt.SequentialNumber;
        local.CashReceiptDetailHistory.CollectionAmtFullyAppliedInd =
          local.CashReceiptDetail.CollectionAmtFullyAppliedInd ?? "";
        local.CashReceiptDetailHistory.CollectionAmount =
          local.CashReceiptDetail.CollectionAmount;
        local.CashReceiptDetailHistory.CollectionDate =
          local.CashReceiptDetail.CollectionDate;
        local.CashReceiptDetailHistory.CollectionTypeIdentifier =
          import.CollectionType.SequentialIdentifier;
        local.CashReceiptDetailHistory.CourtOrderNumber =
          local.CashReceiptDetail.CourtOrderNumber ?? "";
        local.CashReceiptDetailHistory.CreatedBy =
          local.CashReceiptDetail.CreatedBy;
        local.CashReceiptDetailHistory.CreatedTmst =
          local.CashReceiptDetail.CreatedTmst;
        local.CashReceiptDetailHistory.DefaultedCollectionDateInd =
          local.CashReceiptDetail.DefaultedCollectionDateInd ?? "";
        local.CashReceiptDetailHistory.DistributedAmount =
          local.CashReceiptDetail.DistributedAmount.GetValueOrDefault();
        local.CashReceiptDetailHistory.InterfaceTransId =
          local.CashReceiptDetail.InterfaceTransId ?? "";
        local.CashReceiptDetailHistory.JointReturnInd =
          local.CashReceiptDetail.JointReturnInd ?? "";
        local.CashReceiptDetailHistory.JointReturnName =
          local.CashReceiptDetail.JointReturnName ?? "";

        if (IsEmpty(local.CashReceiptDetail.LastUpdatedBy))
        {
          local.CashReceiptDetailHistory.LastUpdatedBy =
            local.CashReceiptDetail.CreatedBy;
          local.CashReceiptDetailHistory.LastUpdatedTmst =
            local.CashReceiptDetail.CreatedTmst;
        }
        else
        {
          local.CashReceiptDetailHistory.LastUpdatedBy =
            local.CashReceiptDetail.LastUpdatedBy ?? Spaces(8);
          local.CashReceiptDetailHistory.LastUpdatedTmst =
            local.CashReceiptDetail.LastUpdatedTmst;
        }

        local.CashReceiptDetailHistory.MultiPayor =
          local.CashReceiptDetail.MultiPayor ?? "";
        local.CashReceiptDetailHistory.Notes =
          local.CashReceiptDetail.Notes ?? "";
        local.CashReceiptDetailHistory.ObligorFirstName =
          local.CashReceiptDetail.ObligorFirstName ?? "";
        local.CashReceiptDetailHistory.ObligorLastName =
          local.CashReceiptDetail.ObligorLastName ?? "";
        local.CashReceiptDetailHistory.ObligorMiddleName =
          local.CashReceiptDetail.ObligorMiddleName ?? "";
        local.CashReceiptDetailHistory.ObligorPersonNumber =
          local.CashReceiptDetail.ObligorPersonNumber ?? "";
        local.CashReceiptDetailHistory.ObligorPhoneNumber =
          local.CashReceiptDetail.ObligorPhoneNumber ?? "";
        local.CashReceiptDetailHistory.ObligorSocialSecurityNumber =
          local.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
        local.CashReceiptDetailHistory.OffsetTaxYear =
          local.CashReceiptDetail.OffsetTaxYear.GetValueOrDefault();
        local.CashReceiptDetailHistory.OffsetTaxid =
          local.CashReceiptDetail.OffsetTaxid.GetValueOrDefault();
        local.CashReceiptDetailHistory.PayeeFirstName =
          local.CashReceiptDetail.PayeeFirstName ?? "";
        local.CashReceiptDetailHistory.PayeeLastName =
          local.CashReceiptDetail.PayeeLastName ?? "";
        local.CashReceiptDetailHistory.PayeeMiddleName =
          local.CashReceiptDetail.PayeeMiddleName ?? "";
        local.CashReceiptDetailHistory.ReceivedAmount =
          local.CashReceiptDetail.ReceivedAmount;
        local.CashReceiptDetailHistory.Reference =
          local.CashReceiptDetail.Reference ?? "";
        local.CashReceiptDetailHistory.RefundedAmount =
          local.CashReceiptDetail.RefundedAmount.GetValueOrDefault();
        local.CashReceiptDetailHistory.SequentialIdentifier =
          local.CashReceiptDetail.SequentialIdentifier;
        UseFnLogCashRcptDtlHistory();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0054_CASH_RCPT_DTL_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0056_CASH_RCPT_DTL_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";
    }
  }

  private void UseFnLogCashRcptDtlHistory()
  {
    var useImport = new FnLogCashRcptDtlHistory.Import();
    var useExport = new FnLogCashRcptDtlHistory.Export();

    useImport.CashReceiptDetailHistory.Assign(local.CashReceiptDetailHistory);

    Call(FnLogCashRcptDtlHistory.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.OffsetTaxid = db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 9);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 10);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 11);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 13);
        entities.CashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 16);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 19);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 21);
        entities.CashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetail.PayeeFirstName =
          db.GetNullableString(reader, 23);
        entities.CashReceiptDetail.PayeeMiddleName =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetail.PayeeLastName =
          db.GetNullableString(reader, 25);
        entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 26);
        entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 27);
        entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 28);
        entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 30);
        entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 31);
        entities.CashReceiptDetail.CreatedBy = db.GetString(reader, 32);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 33);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 35);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 37);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 38);
        entities.CashReceiptDetail.SuppPersonNoForVol =
          db.GetNullableString(reader, 39);
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 40);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 41);
        entities.CashReceiptDetail.JfaReceivedDate =
          db.GetNullableDate(reader, 42);
        entities.CashReceiptDetail.CruProcessedDate =
          db.GetNullableDate(reader, 43);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.CashReceiptDetail.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.CashReceiptDetail.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var payeeFirstName = import.CashReceiptDetail.PayeeFirstName ?? "";
    var payeeMiddleName = import.CashReceiptDetail.PayeeMiddleName ?? "";
    var payeeLastName = import.CashReceiptDetail.PayeeLastName ?? "";
    var attribute1SupportedPersonFirstName =
      import.CashReceiptDetail.Attribute1SupportedPersonFirstName ?? "";
    var attribute1SupportedPersonMiddleName =
      import.CashReceiptDetail.Attribute1SupportedPersonMiddleName ?? "";
    var attribute1SupportedPersonLastName =
      import.CashReceiptDetail.Attribute1SupportedPersonLastName ?? "";
    var attribute2SupportedPersonFirstName =
      import.CashReceiptDetail.Attribute2SupportedPersonFirstName ?? "";
    var attribute2SupportedPersonLastName =
      import.CashReceiptDetail.Attribute2SupportedPersonLastName ?? "";
    var attribute2SupportedPersonMiddleName =
      import.CashReceiptDetail.Attribute2SupportedPersonMiddleName ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var jfaReceivedDate = import.CashReceiptDetail.JfaReceivedDate;
    var cruProcessedDate = import.CashReceiptDetail.CruProcessedDate;

    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "payeeFirstName", payeeFirstName);
        db.SetNullableString(command, "payeeMiddleName", payeeMiddleName);
        db.SetNullableString(command, "payeeLastName", payeeLastName);
        db.SetNullableString(
          command, "supPrsnFrstNm1", attribute1SupportedPersonFirstName);
        db.SetNullableString(
          command, "supPrsnMidNm1", attribute1SupportedPersonMiddleName);
        db.SetNullableString(
          command, "supPrsnLstNm1", attribute1SupportedPersonLastName);
        db.SetNullableString(
          command, "supPrsnFrstNm2", attribute2SupportedPersonFirstName);
        db.SetNullableString(
          command, "supPrsnLstNm2", attribute2SupportedPersonLastName);
        db.SetNullableString(
          command, "supPrsnMidNm2", attribute2SupportedPersonMiddleName);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDate(command, "jfaReceivedDate", jfaReceivedDate);
        db.SetNullableDate(command, "cruProcessedDate", cruProcessedDate);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.PayeeFirstName = payeeFirstName;
    entities.CashReceiptDetail.PayeeMiddleName = payeeMiddleName;
    entities.CashReceiptDetail.PayeeLastName = payeeLastName;
    entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
      attribute1SupportedPersonFirstName;
    entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
      attribute1SupportedPersonMiddleName;
    entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
      attribute1SupportedPersonLastName;
    entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
      attribute2SupportedPersonFirstName;
    entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
      attribute2SupportedPersonLastName;
    entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
      attribute2SupportedPersonMiddleName;
    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.JfaReceivedDate = jfaReceivedDate;
    entities.CashReceiptDetail.CruProcessedDate = cruProcessedDate;
    entities.CashReceiptDetail.Populated = true;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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

    private CollectionType collectionType;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
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
    /// A value of CashReceiptDetailHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailHistory")]
    public CashReceiptDetailHistory CashReceiptDetailHistory
    {
      get => cashReceiptDetailHistory ??= new();
      set => cashReceiptDetailHistory = value;
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

    private CashReceiptDetailHistory cashReceiptDetailHistory;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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

    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
  }
#endregion
}
