// Program: FN_CREATE_CASH_RCPT_HISTORY, ID: 372676221, model: 746.
// Short name: SWE00346
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_CASH_RCPT_HISTORY.
/// </para>
/// <para>
/// RESP:  FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateCashRcptHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CASH_RCPT_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCashRcptHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCashRcptHistory.
  /// </summary>
  public FnCreateCashRcptHistory(IContext context, Import import, Export export):
    
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
    // Date	By	Description
    // -----------------------------------------------------------------------
    // ??????	???????	-Initial code
    // 061797	govind	-Removed all the nested structure and
    // 		cleaned up. Fixed to associate with
    // 		collection type.
    // 022398	govind	-Added multipayor to the import view. In
    // 		the absence of it it was defaulting it to
    // 		'F'.
    // 01jan99	lxj	-Fixed status and added new code to
    // 		include cash receipt int the current day
    // 		receipting business ( daily deposit ).
    // 		-Redundand code removed.
    // 06/09/99  Fangman  Added Deposit_Release_Date & Balanced_Timestamp to 
    // Cash_Receipt per Judy & Tim.
    // 06/18/99  Fangman  Set Cash_Receipt_Detail Joint_Ind to spaces per Sunya.
    // 06/22/99  Fangman   Create the Cash Receipt Detail Address per Sunya.
    // -----------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    local.ControlTable.Identifier = "CASH RECEIPT";

    if (ReadCashReceiptStatus())
    {
      if (ReadCollectionType())
      {
        if (ReadCashReceiptSourceType())
        {
          local.UniqueEventIdAttempts.Count = 0;

          do
          {
            try
            {
              CreateCashReceiptEvent();
              ExitState = "ACO_NN0000_ALL_OK";

              // ----------------------------
              // N.Engoor - 03/11/99
              // Changed the escape stmnt to come out of the Repeat loop once 
              // successful.
              // -----------------------------
              break;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ++local.UniqueEventIdAttempts.Count;
                  ExitState = "FN0076_CASH_RCPT_EVENT_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0080_CASH_RCPT_EVENT_PV";

                  goto Read;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          while(local.UniqueEventIdAttempts.Count <= 5);

          if (ReadCashReceiptType())
          {
            // -------------------------------------------------------------------
            // Retrieve the next sequential cash receipt number
            // ------------------------------------------------------------------
            local.ControlTable.Identifier = "CASH RECEIPT";
            UseAccessControlTable();
          }
          else
          {
            ExitState = "FN0113_CASH_RCPT_TYPE_NF";
          }
        }
        else
        {
          ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
        }
      }
      else
      {
        ExitState = "FN0000_COLLECTION_TYPE_NF";
      }
    }
    else
    {
      ExitState = "FN0109_CASH_RCPT_STAT_NF_RB";
    }

Read:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (ReadFundTransaction())
    {
      local.FundTransaction.Amount = entities.FundTransaction.Amount;
    }

    if (!entities.FundTransaction.Populated)
    {
      ExitState = "FN0000_FUND_TRANS_NF";

      return;
    }

    try
    {
      CreateCashReceipt();
      export.CashReceipt.SequentialNumber =
        entities.CashReceipt.SequentialNumber;
      local.FundTransaction.Amount += entities.CashReceipt.ReceiptAmount;

      try
      {
        CreateCashReceiptStatusHistory();

        // ---------------------------------------------
        // Create Cash Receipt Detail.
        // ---------------------------------------------
        try
        {
          CreateCashReceiptDetail();

          try
          {
            CreateCashReceiptDetailAddress();
          }
          catch(Exception e3)
          {
            switch(GetErrorCode(e3))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0038_CASH_RCPT_DTL_ADDR_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0041_CASH_RCPT_DTL_ADDR_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (ReadCashReceiptDetailStatus1())
          {
            try
            {
              CreateCashReceiptDetailStatHistory2();
            }
            catch(Exception e3)
            {
              switch(GetErrorCode(e3))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0063_CASH_RCPT_DTL_STAT_HST_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0066_CASH_RCPT_DTL_STAT_HST_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          else
          {
            ExitState = "FN0072_CASH_RCPT_DTL_STAT_NF_RB";

            return;
          }

          if (ReadCashReceiptDetailStatus2())
          {
            try
            {
              CreateCashReceiptDetailStatHistory1();

              try
              {
                UpdateFundTransaction();
              }
              catch(Exception e4)
              {
                switch(GetErrorCode(e4))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_FUND_TRANS_NU_RB";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_FUND_TRANS_RLN_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            catch(Exception e3)
            {
              switch(GetErrorCode(e3))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0063_CASH_RCPT_DTL_STAT_HST_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0066_CASH_RCPT_DTL_STAT_HST_PV";

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
            ExitState = "FN0072_CASH_RCPT_DTL_STAT_NF_RB";
          }
        }
        catch(Exception e2)
        {
          switch(GetErrorCode(e2))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0118_CASH_RCPT_DTL_AE";

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
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0062_CASH_RCPT_DTL_STAT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0066_CASH_RCPT_DTL_STAT_HST_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0021_CASH_RCPT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0091_CASH_RCPT_PV_RB";

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

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);

    var crvIdentifier = entities.CashReceiptEvent.SystemGeneratedIdentifier;
    var cstIdentifier = entities.CashReceiptEvent.CstIdentifier;
    var crtIdentifier = entities.CashReceiptType.SystemGeneratedIdentifier;
    var receiptAmount = import.CashReceipt.ReceiptAmount;
    var sequentialNumber = local.ControlTable.LastUsedNumber;
    var receiptDate = local.Current.Date;
    var checkType = import.CashReceipt.CheckType ?? "";
    var checkNumber = import.CashReceipt.CheckNumber ?? "";
    var checkDate = import.CashReceipt.CheckDate;
    var receivedDate = import.CashReceipt.ReceivedDate;
    var depositReleaseDate = import.CashReceipt.DepositReleaseDate;
    var payorFirstName = import.CashReceipt.PayorFirstName ?? "";
    var payorMiddleName = import.CashReceipt.PayorMiddleName ?? "";
    var payorLastName = import.CashReceipt.PayorLastName ?? "";
    var balancedTimestamp = import.CashReceipt.BalancedTimestamp;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var fttIdentifier = entities.FundTransaction.FttIdentifier;
    var pcaCode = entities.FundTransaction.PcaCode;
    var pcaEffectiveDate = entities.FundTransaction.PcaEffectiveDate;
    var funIdentifier = entities.FundTransaction.FunIdentifier;
    var ftrIdentifier = entities.FundTransaction.SystemGeneratedIdentifier;
    var note = import.CashReceipt.Note ?? "";

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
        db.SetNullableDate(command, "checkDate", checkDate);
        db.SetDate(command, "receivedDate", receivedDate);
        db.SetNullableDate(command, "depositRlseDt", depositReleaseDate);
        db.SetNullableString(command, "referenceNumber", "");
        db.SetNullableString(command, "payorOrganization", "");
        db.SetNullableString(command, "payorFirstName", payorFirstName);
        db.SetNullableString(command, "payorMiddleName", payorMiddleName);
        db.SetNullableString(command, "payorLastName", payorLastName);
        db.SetNullableString(command, "frwrdStreet1", "");
        db.SetNullableString(command, "frwrdState", "");
        db.SetNullableString(command, "frwrdZip5", "");
        db.SetNullableString(command, "frwrdZip4", "");
        db.SetNullableString(command, "frwrdZip3", "");
        db.SetNullableDateTime(command, "balTmst", balancedTimestamp);
        db.SetNullableDecimal(command, "totalCashTransac", 0M);
        db.SetNullableInt32(command, "totCashTranCnt", 0);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "fttIdentifier", fttIdentifier);
        db.SetNullableString(command, "pcaCode", pcaCode);
        db.SetNullableDate(command, "pcaEffectiveDate", pcaEffectiveDate);
        db.SetNullableInt32(command, "funIdentifier", funIdentifier);
        db.SetNullableInt32(command, "ftrIdentifier", ftrIdentifier);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "note", note);
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
    entities.CashReceipt.CheckDate = checkDate;
    entities.CashReceipt.ReceivedDate = receivedDate;
    entities.CashReceipt.DepositReleaseDate = depositReleaseDate;
    entities.CashReceipt.PayorFirstName = payorFirstName;
    entities.CashReceipt.PayorMiddleName = payorMiddleName;
    entities.CashReceipt.PayorLastName = payorLastName;
    entities.CashReceipt.BalancedTimestamp = balancedTimestamp;
    entities.CashReceipt.CreatedBy = createdBy;
    entities.CashReceipt.CreatedTimestamp = createdTimestamp;
    entities.CashReceipt.FttIdentifier = fttIdentifier;
    entities.CashReceipt.PcaCode = pcaCode;
    entities.CashReceipt.PcaEffectiveDate = pcaEffectiveDate;
    entities.CashReceipt.FunIdentifier = funIdentifier;
    entities.CashReceipt.FtrIdentifier = ftrIdentifier;
    entities.CashReceipt.Note = note;
    entities.CashReceipt.Populated = true;
  }

  private void CreateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var crvIdentifier = entities.CashReceipt.CrvIdentifier;
    var cstIdentifier = entities.CashReceipt.CstIdentifier;
    var crtIdentifier = entities.CashReceipt.CrtIdentifier;
    var sequentialIdentifier = import.CashReceiptDetail.SequentialIdentifier;
    var courtOrderNumber = import.CashReceiptDetail.CourtOrderNumber ?? "";
    var receivedAmount = import.CashReceiptDetail.ReceivedAmount;
    var collectionAmount = import.CashReceiptDetail.CollectionAmount;
    var collectionDate = import.CashReceiptDetail.CollectionDate;
    var multiPayor = import.CashReceiptDetail.MultiPayor ?? "";
    var obligorPersonNumber = import.CashReceiptDetail.ObligorPersonNumber ?? ""
      ;
    var obligorSocialSecurityNumber =
      import.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
    var obligorFirstName = import.CashReceiptDetail.ObligorFirstName ?? "";
    var obligorLastName = import.CashReceiptDetail.ObligorLastName ?? "";
    var obligorMiddleName = import.CashReceiptDetail.ObligorMiddleName ?? "";
    var createdBy = global.UserId;
    var createdTmst = local.Current.Timestamp;
    var cltIdentifier = entities.CollectionType.SequentialIdentifier;
    var reference = import.CashReceiptDetail.Reference ?? "";
    var notes = import.CashReceiptDetail.Notes ?? "";

    CheckValid<CashReceiptDetail>("MultiPayor", multiPayor);
    CheckValid<CashReceiptDetail>("JointReturnInd", "");
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
        db.SetDecimal(command, "collectionAmount", collectionAmount);
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "multiPayor", multiPayor);
        db.SetNullableInt32(command, "offsetTaxYear", 0);
        db.SetNullableString(command, "jointReturnInd", "");
        db.SetNullableString(command, "jointReturnName", "");
        db.SetNullableString(
          command, "dfltdCollDatInd", GetImplicitValue<CashReceiptDetail,
          string>("DefaultedCollectionDateInd"));
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
        db.SetNullableString(command, "referenc", reference);
        db.SetNullableString(command, "notes", notes);
        db.SetNullableDate(command, "jfaReceivedDate", default(DateTime));
      });

    entities.CashReceiptDetail.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetail.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetail.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetail.SequentialIdentifier = sequentialIdentifier;
    entities.CashReceiptDetail.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetail.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetail.CollectionAmount = collectionAmount;
    entities.CashReceiptDetail.CollectionDate = collectionDate;
    entities.CashReceiptDetail.MultiPayor = multiPayor;
    entities.CashReceiptDetail.JointReturnInd = "";
    entities.CashReceiptDetail.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetail.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetail.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetail.ObligorLastName = obligorLastName;
    entities.CashReceiptDetail.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetail.CreatedBy = createdBy;
    entities.CashReceiptDetail.CreatedTmst = createdTmst;
    entities.CashReceiptDetail.CltIdentifier = cltIdentifier;
    entities.CashReceiptDetail.Reference = reference;
    entities.CashReceiptDetail.Notes = notes;
    entities.CashReceiptDetail.Populated = true;
  }

  private void CreateCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var systemGeneratedIdentifier = Now();
    var street1 = import.CsePersonAddress.Street1 ?? "";
    var street2 = import.CsePersonAddress.Street2 ?? "";
    var city = import.CsePersonAddress.City ?? "";
    var state = import.CsePersonAddress.State ?? "";
    var zipCode5 = import.CsePersonAddress.ZipCode ?? "";
    var zipCode4 = import.CsePersonAddress.Zip4 ?? "";
    var zipCode3 = import.CsePersonAddress.Zip3 ?? "";
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;

    entities.CashReceiptDetailAddress.Populated = false;
    Update("CreateCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetDateTime(command, "crdetailAddressI", systemGeneratedIdentifier);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "state", state);
        db.SetString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zipCode3", zipCode3);
        db.SetNullableInt32(command, "crtIdentifier", crtIdentifier);
        db.SetNullableInt32(command, "cstIdentifier", cstIdentifier);
        db.SetNullableInt32(command, "crvIdentifier", crvIdentifier);
        db.SetNullableInt32(command, "crdIdentifier", crdIdentifier);
      });

    entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptDetailAddress.Street1 = street1;
    entities.CashReceiptDetailAddress.Street2 = street2;
    entities.CashReceiptDetailAddress.City = city;
    entities.CashReceiptDetailAddress.State = state;
    entities.CashReceiptDetailAddress.ZipCode5 = zipCode5;
    entities.CashReceiptDetailAddress.ZipCode4 = zipCode4;
    entities.CashReceiptDetailAddress.ZipCode3 = zipCode3;
    entities.CashReceiptDetailAddress.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailAddress.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailAddress.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailAddress.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailAddress.Populated = true;
  }

  private void CreateCashReceiptDetailStatHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier =
      entities.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var discontinueDate = local.Maximum.Date;
    var reasonText = "PAYMENT HISTORY ENTRY";

    entities.CashReceiptDetailStatHistory.Populated = false;
    Update("CreateCashReceiptDetailStatHistory1",
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

  private void CreateCashReceiptDetailStatHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cdsIdentifier =
      entities.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var discontinueDate = entities.CashReceipt.ReceiptDate;
    var reasonText = "PAYMENT HISTORY ENTRY";

    entities.CashReceiptDetailStatHistory.Populated = false;
    Update("CreateCashReceiptDetailStatHistory2",
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
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var receivedDate = import.CashReceipt.ReceiptDate;
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
    var discontinueDate = local.Maximum.Date;

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

  private bool ReadCashReceiptDetailStatus1()
  {
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus1",
      null,
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus2()
  {
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          import.CashReceiptDetailStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      null,
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.Code = db.GetString(reader, 1);
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
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadFundTransaction()
  {
    entities.FundTransaction.Populated = false;

    return Read("ReadFundTransaction",
      null,
      (db, reader) =>
      {
        entities.FundTransaction.FttIdentifier = db.GetInt32(reader, 0);
        entities.FundTransaction.PcaCode = db.GetString(reader, 1);
        entities.FundTransaction.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.FundTransaction.FunIdentifier = db.GetInt32(reader, 3);
        entities.FundTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransaction.DepositNumber = db.GetNullableInt32(reader, 5);
        entities.FundTransaction.Amount = db.GetDecimal(reader, 6);
        entities.FundTransaction.BusinessDate = db.GetDate(reader, 7);
        entities.FundTransaction.CreatedBy = db.GetString(reader, 8);
        entities.FundTransaction.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.FundTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.FundTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.FundTransaction.Populated = true;
      });
  }

  private void UpdateFundTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    var amount = local.FundTransaction.Amount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.FundTransaction.Populated = false;
    Update("UpdateFundTransaction",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetString(command, "pcaCode", entities.FundTransaction.PcaCode);
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetInt32(
          command, "fundTransId",
          entities.FundTransaction.SystemGeneratedIdentifier);
      });

    entities.FundTransaction.Amount = amount;
    entities.FundTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.FundTransaction.LastUpdatedTmst = lastUpdatedTmst;
    entities.FundTransaction.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Continue1.
    /// </summary>
    [JsonPropertyName("continue1")]
    public Common Continue1
    {
      get => continue1 ??= new();
      set => continue1 = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private CsePerson csePerson;
    private CollectionType collectionType;
    private Common continue1;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceipt cashReceipt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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

    /// <summary>
    /// A value of UniqueEventIdAttempts.
    /// </summary>
    [JsonPropertyName("uniqueEventIdAttempts")]
    public Common UniqueEventIdAttempts
    {
      get => uniqueEventIdAttempts ??= new();
      set => uniqueEventIdAttempts = value;
    }

    private FundTransaction fundTransaction;
    private DateWorkArea current;
    private DateWorkArea maximum;
    private ControlTable controlTable;
    private Common uniqueEventIdAttempts;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of FundTransactionType.
    /// </summary>
    [JsonPropertyName("fundTransactionType")]
    public FundTransactionType FundTransactionType
    {
      get => fundTransactionType ??= new();
      set => fundTransactionType = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private FundTransaction fundTransaction;
    private FundTransactionType fundTransactionType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private ObligationType obligationType;
  }
#endregion
}
