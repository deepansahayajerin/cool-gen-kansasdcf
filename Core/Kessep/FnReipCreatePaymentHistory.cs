// Program: FN_REIP_CREATE_PAYMENT_HISTORY, ID: 372418912, model: 746.
// Short name: SWE02452
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_REIP_CREATE_PAYMENT_HISTORY.
/// </para>
/// <para>
/// RESP:  FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnReipCreatePaymentHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REIP_CREATE_PAYMENT_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReipCreatePaymentHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReipCreatePaymentHistory.
  /// </summary>
  public FnReipCreatePaymentHistory(IContext context, Import import,
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
    // -----------------------------------------------------------------------
    // Date	By	IDCR #	Description
    // -----------------------------------------------------------------------
    // ??????	???????		Initial code
    // 061797	govind		Removed all the nested structure and cleaned
    // 			up. Fixed to associate with collection type.
    // 022398	govind		Added multipayor to the import view. In the
    // 			absence of it it was defaulting it to 'F'.
    // 12/19/98  J. Katz	Set multipayor indicator to spaces.
    // 			Modify logic to always expect collection type
    // 			to be imported.
    // 06/08/99  J. Katz	Analyzed READ statements and changed read
    // 			property to Select Only where appropriate.
    // -----------------------------------------------------------------------
    // 01/04/00  P. Phinney  H00082731  Do NOT allow collection date prior
    // to 01/01/1960 or Before Effective Date.
    // ---------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------------------------------
    // Set up local views.
    // ---------------------------------------------------------------------
    local.Current.Timestamp = Now();
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // ---------------------------------------------------------------------
    // Validate imported code table values.
    // ---------------------------------------------------------------------
    if (ReadCashReceiptSourceType())
    {
      // -->  Continue
    }
    else
    {
      ExitState = "FN0000_CASH_RCPT_SRC_TYP_NF_W_RB";

      return;
    }

    if (ReadCashReceiptType())
    {
      // -->  Continue
    }
    else
    {
      ExitState = "FN0114_CASH_RCPT_TYPE_NF_RB";

      return;
    }

    // 01/04/00  P. Phinney  H00082731  Do NOT allow invalid collection date
    local.CollectionTypeFound.Flag = "N";

    if (ReadCollectionType1())
    {
      local.CollectionTypeFound.Flag = "Y";
    }

    if (AsChar(local.CollectionTypeFound.Flag) != 'Y')
    {
      ExitState = "FN0000_COLLECTION_TYPE_NF_RB";

      return;
    }

    if (ReadCollectionType2())
    {
      // -->  Continue
    }
    else
    {
      ExitState = "FN0000_COLL_DATE_NOT_VALID";

      return;
    }

    // ---------------------------------------------
    // Create Cash Receipt Event.
    // ---------------------------------------------
    local.GetUniqueIdAttempts.Count = 0;

    do
    {
      local.SystemGeneratedId.Count = UseGenerate9DigitRandomNumber();

      try
      {
        CreateCashReceiptEvent();

        break;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ++local.GetUniqueIdAttempts.Count;
            ExitState = "FN0076_CASH_RCPT_EVENT_AE_W_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0080_CASH_RCPT_EVENT_PV_W_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(local.GetUniqueIdAttempts.Count <= 5);

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------
    // Create Cash Receipt.
    // Retrieve the next sequential cash receipt number from
    // the Access Control Table using Identifier = "CASH RECEIPT"
    // -------------------------------------------------------------------
    local.ControlTable.Identifier = "CASH RECEIPT";
    UseAccessControlTable();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    try
    {
      CreateCashReceipt();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0126_CASH_RCPT_AE_W_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0091_CASH_RCPT_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ---------------------------------------------
    // Create Cash Receipt Status History.
    // ---------------------------------------------
    if (ReadCashReceiptStatus())
    {
      // -->  Continue
    }
    else
    {
      ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";

      return;
    }

    try
    {
      CreateCashReceiptStatusHistory();

      // -->  Continue
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0101_CASH_RCPT_STAT_HIST_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0105_CASH_RCPT_STAT_HIST_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ---------------------------------------------
    // Create Cash Receipt Detail.
    // ---------------------------------------------
    try
    {
      CreateCashReceiptDetail();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0119_CASH_RCPT_DTL_AE_W_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0057_CASH_RCPT_DTL_PV_W_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // ---------------------------------------------
    // Create Cash Receipt Detail Status History.
    // ---------------------------------------------
    if (ReadCashReceiptDetailStatus())
    {
      // -->  Continue
    }
    else
    {
      ExitState = "FN0072_CASH_RCPT_DTL_STAT_NF_RB";

      return;
    }

    try
    {
      CreateCashReceiptDetailStatHistory();

      // -->  Payment history record successfully created.
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0067_CSH_RCPT_DTL_ST_HS_AE_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0068_CSH_RCPT_DTL_ST_HS_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsReceipted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrdsRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);

    var crvIdentifier = entities.CashReceiptEvent.SystemGeneratedIdentifier;
    var cstIdentifier = entities.CashReceiptEvent.CstIdentifier;
    var crtIdentifier = entities.CashReceiptType.SystemGeneratedIdentifier;
    var receiptAmount = import.CashReceiptDetail.CollectionAmount;
    var sequentialNumber = local.ControlTable.LastUsedNumber;
    var receiptDate = local.Current.Date;
    var checkType = "CSE";
    var checkNumber = import.CashReceipt.CheckNumber ?? "";
    var receivedDate = import.CashReceiptDetail.CollectionDate;
    var referenceNumber = import.CashReceipt.ReferenceNumber ?? "";
    var payorFirstName = import.CsePersonsWorkSet.FirstName;
    var payorMiddleName = import.CsePersonsWorkSet.MiddleInitial;
    var payorLastName = import.CsePersonsWorkSet.LastName;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;

    entities.CashReceipt.Populated = false;
    Update("CreateCashReceipt",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetDecimal(command, "receiptAmount", receiptAmount);
        db.SetInt32(command, "cashReceiptId", sequentialNumber);
        db.SetDate(command, "receiptDate", receiptDate);
        db.SetNullableString(command, "checkType", checkType);
        db.SetNullableString(command, "checkNumber", checkNumber);
        db.SetNullableDate(command, "checkDate", default(DateTime));
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetNullableString(command, "payorOrganization", "");
        db.SetNullableString(command, "payorFirstName", payorFirstName);
        db.SetNullableString(command, "payorMiddleName", payorMiddleName);
        db.SetNullableString(command, "payorLastName", payorLastName);
        db.SetNullableString(command, "frwrdStreet1", "");
        db.SetNullableString(command, "frwrdState", "");
        db.SetNullableString(command, "frwrdZip5", "");
        db.SetNullableString(command, "frwrdZip4", "");
        db.SetNullableString(command, "frwrdZip3", "");
        db.SetNullableDateTime(command, "balTmst", default(DateTime));
        db.SetNullableDecimal(command, "totalCashTransac", 0M);
        db.SetNullableInt32(command, "totCashTranCnt", 0);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableString(command, "note", "");
        db.SetNullableString(command, "payorSsn", "");
      });

    entities.CashReceipt.CrvIdentifier = crvIdentifier;
    entities.CashReceipt.CstIdentifier = cstIdentifier;
    entities.CashReceipt.CrtIdentifier = crtIdentifier;
    entities.CashReceipt.ReceiptAmount = receiptAmount;
    entities.CashReceipt.SequentialNumber = sequentialNumber;
    entities.CashReceipt.ReceiptDate = receiptDate;
    entities.CashReceipt.CheckType = checkType;
    entities.CashReceipt.CheckNumber = checkNumber;
    entities.CashReceipt.ReceivedDate = receivedDate;
    entities.CashReceipt.ReferenceNumber = referenceNumber;
    entities.CashReceipt.PayorFirstName = payorFirstName;
    entities.CashReceipt.PayorMiddleName = payorMiddleName;
    entities.CashReceipt.PayorLastName = payorLastName;
    entities.CashReceipt.CreatedBy = createdBy;
    entities.CashReceipt.CreatedTimestamp = createdTimestamp;
    entities.CashReceipt.Populated = true;
  }

  private void CreateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var sequentialIdentifier = 1;
    var courtOrderNumber = import.LegalAction.StandardNumber ?? "";
    var receivedAmount = import.CashReceiptDetail.CollectionAmount;
    var collectionDate = import.CashReceiptDetail.CollectionDate;
    var defaultedCollectionDateInd = "N";
    var obligorPersonNumber = import.CsePersonsWorkSet.Number;
    var obligorSocialSecurityNumber = import.CsePersonsWorkSet.Ssn;
    var obligorFirstName = import.CsePersonsWorkSet.FirstName;
    var obligorLastName = import.CsePersonsWorkSet.LastName;
    var obligorMiddleName = import.CsePersonsWorkSet.MiddleInitial;
    var createdBy = global.UserId;
    var createdTmst = local.Current.Timestamp;
    var cltIdentifier = entities.CollectionType.SequentialIdentifier;

    CheckValid<CashReceiptDetail>("MultiPayor", "");
    CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
      defaultedCollectionDateInd);
    entities.CashReceiptDetail.Populated = false;
    Update("CreateCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "crdId", sequentialIdentifier);
        db.SetNullableString(command, "interfaceTranId", "");
        db.SetNullableString(command, "adjustmentInd", "");
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "caseNumber", "");
        db.SetNullableInt32(command, "offsetTaxid", 0);
        db.SetDecimal(command, "receivedAmount", receivedAmount);
        db.SetDecimal(command, "collectionAmount", receivedAmount);
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "multiPayor", "");
        db.SetNullableInt32(command, "offsetTaxYear", 0);
        db.SetNullableString(
          command, "jointReturnInd", GetImplicitValue<CashReceiptDetail,
          string>("JointReturnInd"));
        db.SetNullableString(command, "jointReturnName", "");
        db.SetNullableString(
          command, "dfltdCollDatInd", defaultedCollectionDateInd);
        db.SetNullableString(command, "oblgorPrsnNbr", obligorPersonNumber);
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMidNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", "");
        db.SetNullableString(command, "payeeFirstName", "");
        db.SetNullableString(command, "payeeLastName", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDecimal(command, "refundedAmt", 0M);
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetNullableString(command, "referenc", "");
        db.SetNullableDate(command, "jfaReceivedDate", default(DateTime));
      });

    entities.CashReceiptDetail.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetail.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetail.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetail.SequentialIdentifier = sequentialIdentifier;
    entities.CashReceiptDetail.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetail.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetail.CollectionAmount = receivedAmount;
    entities.CashReceiptDetail.CollectionDate = collectionDate;
    entities.CashReceiptDetail.MultiPayor = "";
    entities.CashReceiptDetail.DefaultedCollectionDateInd =
      defaultedCollectionDateInd;
    entities.CashReceiptDetail.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetail.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetail.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetail.ObligorLastName = obligorLastName;
    entities.CashReceiptDetail.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetail.CreatedBy = createdBy;
    entities.CashReceiptDetail.CreatedTmst = createdTmst;
    entities.CashReceiptDetail.CltIdentifier = cltIdentifier;
    entities.CashReceiptDetail.Populated = true;
  }

  private void CreateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier =
      entities.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;
    var reasonText = "PAYMENT HISTORY ENTRY";

    entities.CashReceiptDetailStatHistory.Populated = false;
    Update("CreateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cdsIdentifier", cdsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonCodeId", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.CashReceiptDetailStatHistory.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailStatHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailStatHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailStatHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailStatHistory.CdsIdentifier = cdsIdentifier;
    entities.CashReceiptDetailStatHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptDetailStatHistory.CreatedBy = createdBy;
    entities.CashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptDetailStatHistory.ReasonText = reasonText;
    entities.CashReceiptDetailStatHistory.Populated = true;
  }

  private void CreateCashReceiptEvent()
  {
    var cstIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = local.SystemGeneratedId.Count;
    var receivedDate = import.CashReceiptDetail.CollectionDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var param = 0M;

    entities.CashReceiptEvent.Populated = false;
    Update("CreateCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "creventId", systemGeneratedIdentifier);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "sourceCreationDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableInt32(command, "totNonCshtrnCnt", 0);
        db.SetNullableDecimal(command, "totCashFeeAmt", param);
        db.SetNullableDecimal(command, "anticCheckAmt", param);
      });

    entities.CashReceiptEvent.CstIdentifier = cstIdentifier;
    entities.CashReceiptEvent.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptEvent.ReceivedDate = receivedDate;
    entities.CashReceiptEvent.CreatedBy = createdBy;
    entities.CashReceiptEvent.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptEvent.Populated = true;
  }

  private void CreateCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var crsIdentifier = entities.CashReceiptStatus.SystemGeneratedIdentifier;
    var createdTimestamp = local.Current.Timestamp;
    var createdBy = global.UserId;
    var discontinueDate = local.Max.Date;

    entities.CashReceiptStatusHistory.Populated = false;
    Update("CreateCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "crsIdentifier", crsIdentifier);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.CashReceiptStatusHistory.CrtIdentifier = crtIdentifier;
    entities.CashReceiptStatusHistory.CstIdentifier = cstIdentifier;
    entities.CashReceiptStatusHistory.CrvIdentifier = crvIdentifier;
    entities.CashReceiptStatusHistory.CrsIdentifier = crsIdentifier;
    entities.CashReceiptStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptStatusHistory.CreatedBy = createdBy;
    entities.CashReceiptStatusHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptStatusHistory.Populated = true;
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          local.HardcodedCrdsRecorded.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 1);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedCrsReceipted.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.EffectiveDate = db.GetDate(reader, 1);
        entities.CashReceiptStatus.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId",
          import.CashReceiptType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.EffectiveDate = db.GetDate(reader, 1);
        entities.CashReceiptType.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCollectionType1()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.EffectiveDate = db.GetDate(reader, 2);
        entities.CollectionType.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCollectionType2()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType2",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.CashReceiptDetail.CollectionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.EffectiveDate = db.GetDate(reader, 2);
        entities.CollectionType.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CollectionType.Populated = true;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
    private CashReceipt cashReceipt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
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
    /// A value of CollectionTypeFound.
    /// </summary>
    [JsonPropertyName("collectionTypeFound")]
    public Common CollectionTypeFound
    {
      get => collectionTypeFound ??= new();
      set => collectionTypeFound = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of HardcodedCrsReceipted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsReceipted")]
    public CashReceiptStatus HardcodedCrsReceipted
    {
      get => hardcodedCrsReceipted ??= new();
      set => hardcodedCrsReceipted = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsRecorded")]
    public CashReceiptDetailStatus HardcodedCrdsRecorded
    {
      get => hardcodedCrdsRecorded ??= new();
      set => hardcodedCrdsRecorded = value;
    }

    /// <summary>
    /// A value of SystemGeneratedId.
    /// </summary>
    [JsonPropertyName("systemGeneratedId")]
    public Common SystemGeneratedId
    {
      get => systemGeneratedId ??= new();
      set => systemGeneratedId = value;
    }

    /// <summary>
    /// A value of GetUniqueIdAttempts.
    /// </summary>
    [JsonPropertyName("getUniqueIdAttempts")]
    public Common GetUniqueIdAttempts
    {
      get => getUniqueIdAttempts ??= new();
      set => getUniqueIdAttempts = value;
    }

    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private Common collectionTypeFound;
    private DateWorkArea current;
    private DateWorkArea max;
    private CashReceiptStatus hardcodedCrsReceipted;
    private CashReceiptDetailStatus hardcodedCrdsRecorded;
    private Common systemGeneratedId;
    private Common getUniqueIdAttempts;
    private ControlTable controlTable;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CollectionType collectionType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
  }
#endregion
}
