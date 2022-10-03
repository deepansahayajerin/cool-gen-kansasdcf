// Program: RECORD_COLLECTION, ID: 371770032, model: 746.
// Short name: SWE01052
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: RECORD_COLLECTION.
/// </summary>
[Serializable]
public partial class RecordCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the RECORD_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new RecordCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of RecordCollection.
  /// </summary>
  public RecordCollection(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****  Make sure the imported persistent view is populated.
    if (!import.PersistentCashReceipt.Populated)
    {
      if (ReadCashReceipt())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }
    }

    // ***** If we know the collection type when the collection is created then 
    // we want to associate the collection to it.  If we do not know the
    // collection type we will create the collection without the association to
    // the collection type & let them make that association at a later time.
    if (!import.PersistentCollectionType.Populated && import
      .CollectionType.SequentialIdentifier != 0)
    {
      if (ReadCollectionType())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0000_COLLECTION_TYPE_NF";

        return;
      }
    }

    // ***** The Sequential Identifier will be generated in the prad & passed 
    // down to the pad because this is more efficient than repeatedly going
    // against a control table for this number.
    try
    {
      CreateCashReceiptDetail();

      // *****  Accumulate Control Totals
      ++export.ImportNumberOfUpdates.Count;
      export.CashReceiptDetail.Assign(entities.CashReceiptDetail);

      // *****  If the collection type is populated, associate the Cash Receipt 
      // Detail to it.
      if (import.PersistentCollectionType.Populated)
      {
        AssociateCashReceiptDetail();
        ++export.ImportNumberOfUpdates.Count;
      }

      local.CashReceiptDetailStatus.SystemGeneratedIdentifier = 1;

      // *****  Create the Cash Receipt Detail Status History.
      UseFnCreateCashRcptDtlStatHis();
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

  private void UseFnCreateCashRcptDtlStatHis()
  {
    var useImport = new FnCreateCashRcptDtlStatHis.Import();
    var useExport = new FnCreateCashRcptDtlStatHis.Export();

    useImport.PersistentCashReceiptDetailStatus.Assign(
      import.PersistentCashReceiptDetailStatus);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      import.PersistentCashReceipt.SequentialNumber;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnCreateCashRcptDtlStatHis.Execute, useImport, useExport);

    import.PersistentCashReceiptDetailStatus.Assign(
      useImport.PersistentCashReceiptDetailStatus);
    export.CashReceiptDetailStatHistory.Assign(
      useExport.CashReceiptDetailStatHistory);
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void AssociateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var cltIdentifier = import.PersistentCollectionType.SequentialIdentifier;

    entities.CashReceiptDetail.Populated = false;
    Update("AssociateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.CltIdentifier = cltIdentifier;
    entities.CashReceiptDetail.Populated = true;
  }

  private void CreateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(import.PersistentCashReceipt.Populated);

    var crvIdentifier = import.PersistentCashReceipt.CrvIdentifier;
    var cstIdentifier = import.PersistentCashReceipt.CstIdentifier;
    var crtIdentifier = import.PersistentCashReceipt.CrtIdentifier;
    var sequentialIdentifier = import.CashReceiptDetail.SequentialIdentifier;
    var interfaceTransId = import.CashReceiptDetail.InterfaceTransId ?? "";
    var adjustmentInd = import.CashReceiptDetail.AdjustmentInd ?? "";
    var courtOrderNumber = import.CashReceiptDetail.CourtOrderNumber ?? "";
    var caseNumber = import.CashReceiptDetail.CaseNumber ?? "";
    var offsetTaxid = import.CashReceiptDetail.OffsetTaxid.GetValueOrDefault();
    var receivedAmount = import.CashReceiptDetail.ReceivedAmount;
    var collectionAmount = import.CashReceiptDetail.CollectionAmount;
    var collectionDate = import.CashReceiptDetail.CollectionDate;
    var multiPayor = import.CashReceiptDetail.MultiPayor ?? "";
    var offsetTaxYear =
      import.CashReceiptDetail.OffsetTaxYear.GetValueOrDefault();
    var jointReturnInd = import.CashReceiptDetail.JointReturnInd ?? "";
    var jointReturnName = import.CashReceiptDetail.JointReturnName ?? "";
    var defaultedCollectionDateInd =
      import.CashReceiptDetail.DefaultedCollectionDateInd ?? "";
    var obligorPersonNumber = import.CashReceiptDetail.ObligorPersonNumber ?? ""
      ;
    var obligorSocialSecurityNumber =
      import.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
    var obligorFirstName = import.CashReceiptDetail.ObligorFirstName ?? "";
    var obligorLastName = import.CashReceiptDetail.ObligorLastName ?? "";
    var obligorMiddleName = import.CashReceiptDetail.ObligorMiddleName ?? "";
    var obligorPhoneNumber = import.CashReceiptDetail.ObligorPhoneNumber ?? "";
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
    var createdBy = global.UserId;
    var createdTmst = Now();
    var reference = import.CashReceiptDetail.Reference ?? "";
    var notes = import.CashReceiptDetail.Notes ?? "";
    var injuredSpouseInd = import.CashReceiptDetail.InjuredSpouseInd ?? "";

    CheckValid<CashReceiptDetail>("MultiPayor", multiPayor);
    CheckValid<CashReceiptDetail>("JointReturnInd", jointReturnInd);
    CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
      defaultedCollectionDateInd);
    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd", "");
    entities.CashReceiptDetail.Populated = false;
    Update("CreateCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "crdId", sequentialIdentifier);
        db.SetNullableString(command, "interfaceTranId", interfaceTransId);
        db.SetNullableString(command, "adjustmentInd", adjustmentInd);
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableInt32(command, "offsetTaxid", offsetTaxid);
        db.SetDecimal(command, "receivedAmount", receivedAmount);
        db.SetDecimal(command, "collectionAmount", collectionAmount);
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "multiPayor", multiPayor);
        db.SetNullableInt32(command, "offsetTaxYear", offsetTaxYear);
        db.SetNullableString(command, "jointReturnInd", jointReturnInd);
        db.SetNullableString(command, "jointReturnName", jointReturnName);
        db.SetNullableString(
          command, "dfltdCollDatInd", defaultedCollectionDateInd);
        db.SetNullableString(command, "oblgorPrsnNbr", obligorPersonNumber);
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMidNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", obligorPhoneNumber);
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
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDecimal(command, "refundedAmt", 0M);
        db.SetNullableString(command, "collamtApplInd", "");
        db.SetNullableString(command, "suppPersNoVol", "");
        db.SetNullableString(command, "referenc", reference);
        db.SetNullableString(command, "notes", notes);
        db.SetNullableString(command, "ovrrdMnlDistInd", "");
        db.SetNullableString(command, "injuredSpouseInd", injuredSpouseInd);
        db.SetNullableDate(command, "jfaReceivedDate", default(DateTime));
      });

    entities.CashReceiptDetail.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetail.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetail.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetail.SequentialIdentifier = sequentialIdentifier;
    entities.CashReceiptDetail.InterfaceTransId = interfaceTransId;
    entities.CashReceiptDetail.AdjustmentInd = adjustmentInd;
    entities.CashReceiptDetail.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetail.CaseNumber = caseNumber;
    entities.CashReceiptDetail.OffsetTaxid = offsetTaxid;
    entities.CashReceiptDetail.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetail.CollectionAmount = collectionAmount;
    entities.CashReceiptDetail.CollectionDate = collectionDate;
    entities.CashReceiptDetail.MultiPayor = multiPayor;
    entities.CashReceiptDetail.OffsetTaxYear = offsetTaxYear;
    entities.CashReceiptDetail.JointReturnInd = jointReturnInd;
    entities.CashReceiptDetail.JointReturnName = jointReturnName;
    entities.CashReceiptDetail.DefaultedCollectionDateInd =
      defaultedCollectionDateInd;
    entities.CashReceiptDetail.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetail.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetail.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetail.ObligorLastName = obligorLastName;
    entities.CashReceiptDetail.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetail.ObligorPhoneNumber = obligorPhoneNumber;
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
    entities.CashReceiptDetail.CreatedBy = createdBy;
    entities.CashReceiptDetail.CreatedTmst = createdTmst;
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd = "";
    entities.CashReceiptDetail.CltIdentifier = null;
    entities.CashReceiptDetail.Reference = reference;
    entities.CashReceiptDetail.Notes = notes;
    entities.CashReceiptDetail.InjuredSpouseInd = injuredSpouseInd;
    entities.CashReceiptDetail.Populated = true;
  }

  private bool ReadCashReceipt()
  {
    import.PersistentCashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.PersistentCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        import.PersistentCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        import.PersistentCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        import.PersistentCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        import.PersistentCashReceipt.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    import.PersistentCollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
      },
      (db, reader) =>
      {
        import.PersistentCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        import.PersistentCollectionType.Code = db.GetString(reader, 1);
        import.PersistentCollectionType.Populated = true;
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
    /// A value of PersistentCollectionType.
    /// </summary>
    [JsonPropertyName("persistentCollectionType")]
    public CollectionType PersistentCollectionType
    {
      get => persistentCollectionType ??= new();
      set => persistentCollectionType = value;
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

    /// <summary>
    /// A value of PersistentCashReceipt.
    /// </summary>
    [JsonPropertyName("persistentCashReceipt")]
    public CashReceipt PersistentCashReceipt
    {
      get => persistentCashReceipt ??= new();
      set => persistentCashReceipt = value;
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
    /// A value of PersistentCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("persistentCashReceiptDetailStatus")]
    public CashReceiptDetailStatus PersistentCashReceiptDetailStatus
    {
      get => persistentCashReceiptDetailStatus ??= new();
      set => persistentCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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

    private CollectionType persistentCollectionType;
    private CollectionType collectionType;
    private CashReceipt persistentCashReceipt;
    private CashReceipt cashReceipt;
    private CashReceiptDetailStatus persistentCashReceiptDetailStatus;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    private CashReceiptDetailStatus cashReceiptDetailStatus;
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
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
  }
#endregion
}
