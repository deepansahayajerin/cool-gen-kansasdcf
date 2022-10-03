// Program: FN_RCAP_RECAP_AMOUNT_F_DISB_TRAN, ID: 372674869, model: 746.
// Short name: SWE02164
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_RCAP_RECAP_AMOUNT_F_DISB_TRAN.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block recaptures
///   - a specified amount
///   - from the specified Disbursement Transaction
///   - and transfer it to a Recap Payment Request
/// </para>
/// </summary>
[Serializable]
public partial class FnRcapRecapAmountFDisbTran: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RCAP_RECAP_AMOUNT_F_DISB_TRAN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRcapRecapAmountFDisbTran(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRcapRecapAmountFDisbTran.
  /// </summary>
  public FnRcapRecapAmountFDisbTran(IContext context, Import import,
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
    // ---------------------------------------------
    // This action block recaptures:
    //   - a specified amount
    //   - from a specified Debit Disbursement Transaction
    // and transfers it to a specified Recapture Payment Request.
    // If the Recapture Payment Request is not specified, it
    // creates one.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date 	By	IDCR#	Description
    // ---------------------------------------------
    // 120397	  govind    initial code.
    // 04-25-99  Fangman   Removed unnecessary reads.
    // 06-01-99  Fangman   Added code to delete the Disb Tran RLN so that the 
    // Disbursement could be deleted.
    // 11-02-99  Fangman   PR 77907 - Set Cash Non Cash indicator for recaptures
    // to 'C'.
    // 10-03-01  Fangman   PR 128332  Change code to not delete a disb that is 
    // 100% recaptured.  Recapture disbursements should have same stathist as
    // the original.
    // ---------------------------------------------
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (!ReadDisbursementTransaction2())
    {
      ExitState = "FN0000_DISB_TRANSACTION_NF";

      return;
    }

    // ---------------------------------------------
    // Get the type for the disbursement so that the recapture type can be 
    // determined.
    // ---------------------------------------------
    if (ReadDisbursementType1())
    {
      // ---------------------------------------------
      // Assumption: Suffix "R" is expected for the Recaptured Disb Type.
      // ---------------------------------------------
      local.NewRecaptured.Code = TrimEnd(entities.DisbursementType.Code) + "R";

      if (!ReadDisbursementType2())
      {
        ExitState = "FN0000_CORR_RECAPT_DISB_TYPE_NF";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_DISB_TYPE_NF";

      return;
    }

    if (entities.Original.Amount - import.RecapAmountDebit.Amount == 0)
    {
      // ---------------------------------------------
      // The entire disbursement is being recaptured so:
      // 1. Disassociate disbursement from the warrant payment request.
      // 2. Disassociate disbursement from the old disbursement type.
      // 3. Associate disbursement to the recapture disbursement type.
      // 4. Associate disbursement to the recapture payment request.
      // Note:
      // The disbursements status has already been set to processed by the 
      // preceeding AB.
      // ---------------------------------------------
      if (ReadPaymentRequest())
      {
        DisassociatePaymentRequest();
      }
      else
      {
        ExitState = "FN0000_WARRANT_NF";

        return;
      }

      TransferDisbursementTransaction();
      MoveDisbursementTransaction(entities.Original,
        local.DisbursementTransaction);
    }
    else
    {
      // ---------------------------------------------
      // Part of the disbursement is being recaptured so:
      // 1. Reduce the amount of the disbursement by the amount being 
      // recaptured.
      // 2. Create a new disbursement for the recaptured amount.
      // 3. Associate disbursement to the recapture disbursement type.
      // 4. Associate disbursement to the recapture payment request.
      // 5. Copy each of the previous disbursements statuses so that the status 
      // history of the recpatured part of the disbursement is no lost.
      // ---------------------------------------------
      try
      {
        UpdateDisbursementTransaction();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DISB_TRAN_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_TRANS_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!ReadObligee())
      {
        ExitState = "FN0000_OBLIGEE_NF";

        return;
      }

      if (ReadDisbursementTransactionRln())
      {
        local.DisbursementTransactionRln.Assign(
          entities.DisbursementTransactionRln);
      }
      else
      {
        ExitState = "FN0000_DISB_TRANS_RLN_NF";

        return;
      }

      if (ReadDisbursementTransaction1())
      {
        if (AsChar(entities.Credit.InterstateInd) == 'Y')
        {
          if (!ReadInterstateRequest())
          {
            ExitState = "INTERSTATE_REQUEST_NF";

            return;
          }
        }
      }
      else
      {
        ExitState = "FN0000_CREDIT_DISB_TRAN_NF";

        return;
      }

      if (!ReadDisbursementTranRlnRsn())
      {
        ExitState = "FN0000_DISB_TRANS_RLN_RSN_NF";

        return;
      }

      local.Dummy.Flag = "Y";

      if (AsChar(local.Dummy.Flag) == 'Y')
      {
        for(local.CreateAttempts.Count = 1; local.CreateAttempts.Count <= 20; ++
          local.CreateAttempts.Count)
        {
          try
          {
            CreateDisbursementTransaction();
            MoveDisbursementTransaction(entities.
              RecapturedDisbursementTransaction, local.DisbursementTransaction);
              

            if (entities.InterstateRequest.Populated)
            {
              AssociateDisbursementTransaction();
            }

            try
            {
              CreateDisbursementTransactionRln();

              goto Test;
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_DISB_TRANS_RLN_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_DISB_TRAND_RLN_PV";

                  return;
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
                // Continue in loop until unique ID is found.
                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DISB_TRANS_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

        return;
      }

Test:

      foreach(var item in ReadDisbursementStatusHistoryDisbursementStatus())
      {
        try
        {
          CreateDisbursementStatusHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_STAT_HIST_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    if (import.Recap.SystemGeneratedIdentifier != 0)
    {
      export.Recap.SystemGeneratedIdentifier =
        import.Recap.SystemGeneratedIdentifier;
      UseFnIncludeDisbTranToPmtReq();
    }
    else
    {
      local.Recapture.Type1 = "RCP";
      local.Recapture.Amount = import.RecapAmountDebit.Amount;
      UseFnCreatePaymentRequest();
    }
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MovePaymentRequest1(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MovePaymentRequest2(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCreatePaymentRequest()
  {
    var useImport = new FnCreatePaymentRequest.Import();
    var useExport = new FnCreatePaymentRequest.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Obligee.Number = import.Obligee.Number;
    useImport.DisbursementTransaction.SystemGeneratedIdentifier =
      local.DisbursementTransaction.SystemGeneratedIdentifier;
    MovePaymentRequest2(local.Recapture, useImport.PaymentRequest);

    Call(FnCreatePaymentRequest.Execute, useImport, useExport);

    MovePaymentRequest1(useExport.PaymentRequest, export.Recap);
  }

  private void UseFnIncludeDisbTranToPmtReq()
  {
    var useImport = new FnIncludeDisbTranToPmtReq.Import();
    var useExport = new FnIncludeDisbTranToPmtReq.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Obligee.Number = import.Obligee.Number;
    MoveDisbursementTransaction(local.DisbursementTransaction,
      useImport.DisbursementTransaction);
    useImport.PaymentRequest.SystemGeneratedIdentifier =
      export.Recap.SystemGeneratedIdentifier;

    Call(FnIncludeDisbTranToPmtReq.Execute, useImport, useExport);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(
      entities.RecapturedDisbursementTransaction.Populated);

    var intInterId = entities.InterstateRequest.IntHGeneratedId;

    entities.RecapturedDisbursementTransaction.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetString(
          command, "cpaType",
          entities.RecapturedDisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.RecapturedDisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.RecapturedDisbursementTransaction.SystemGeneratedIdentifier);
          
      });

    entities.RecapturedDisbursementTransaction.IntInterId = intInterId;
    entities.RecapturedDisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.RecapturedDisbursementTransaction.Populated);

    var dbsGeneratedId = entities.DisbursementStatus.SystemGeneratedIdentifier;
    var dtrGeneratedId =
      entities.RecapturedDisbursementTransaction.SystemGeneratedIdentifier;
    var cspNumber = entities.RecapturedDisbursementTransaction.CspNumber;
    var cpaType = entities.RecapturedDisbursementTransaction.CpaType;
    var systemGeneratedIdentifier =
      entities.DisbursementStatusHistory.SystemGeneratedIdentifier;
    var effectiveDate = entities.DisbursementStatusHistory.EffectiveDate;
    var discontinueDate = entities.DisbursementStatusHistory.DiscontinueDate;
    var createdBy = entities.DisbursementStatusHistory.CreatedBy;
    var createdTimestamp = entities.DisbursementStatusHistory.CreatedTimestamp;
    var reasonText = entities.DisbursementStatusHistory.ReasonText;
    var suppressionReason =
      entities.DisbursementStatusHistory.SuppressionReason;

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.NewForRecapDisb.Populated = false;
    Update("CreateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "dbsGeneratedId", dbsGeneratedId);
        db.SetInt32(command, "dtrGeneratedId", dtrGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "disbStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "suppressionReason", suppressionReason);
      });

    entities.NewForRecapDisb.DbsGeneratedId = dbsGeneratedId;
    entities.NewForRecapDisb.DtrGeneratedId = dtrGeneratedId;
    entities.NewForRecapDisb.CspNumber = cspNumber;
    entities.NewForRecapDisb.CpaType = cpaType;
    entities.NewForRecapDisb.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.NewForRecapDisb.EffectiveDate = effectiveDate;
    entities.NewForRecapDisb.DiscontinueDate = discontinueDate;
    entities.NewForRecapDisb.CreatedBy = createdBy;
    entities.NewForRecapDisb.CreatedTimestamp = createdTimestamp;
    entities.NewForRecapDisb.ReasonText = reasonText;
    entities.NewForRecapDisb.SuppressionReason = suppressionReason;
    entities.NewForRecapDisb.Populated = true;
  }

  private void CreateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee2.Populated);

    var cpaType = entities.Obligee2.Type1;
    var cspNumber = entities.Obligee2.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = entities.Original.Type1;
    var amount = import.RecapAmountDebit.Amount;
    var processDate = entities.Original.ProcessDate;
    var createdBy = entities.Original.CreatedBy;
    var createdTimestamp = entities.Original.CreatedTimestamp;
    var lastUpdatedBy = entities.Original.LastUpdatedBy;
    var lastUpdateTmst = entities.Original.LastUpdateTmst;
    var disbursementDate = entities.Original.DisbursementDate;
    var cashNonCashInd = entities.Original.CashNonCashInd;
    var recapturedInd = "Y";
    var collectionDate = entities.Original.CollectionDate;
    var dbtGeneratedId =
      entities.RecapturedDisbursementType.SystemGeneratedIdentifier;
    var interstateInd = entities.Original.InterstateInd;
    var passthruProcDate = entities.Original.PassthruProcDate;
    var designatedPayee = entities.Original.DesignatedPayee;
    var referenceNumber = entities.Original.ReferenceNumber;
    var excessUraInd = entities.Original.ExcessUraInd;

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.RecapturedDisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "disbTranId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "amount", amount);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetNullableDate(command, "disbursementDate", disbursementDate);
        db.SetString(command, "cashNonCashInd", cashNonCashInd);
        db.SetString(command, "recapturedInd", recapturedInd);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetDate(command, "collctnProcessDt", default(DateTime));
        db.SetNullableInt32(command, "dbtGeneratedId", dbtGeneratedId);
        db.SetNullableString(
          command, "otrTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("OtrTypeDisb"));
        db.SetNullableString(
          command, "cpaTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("CpaTypeDisb"));
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableDate(command, "passthruProcDate", passthruProcDate);
        db.SetNullableString(command, "designatedPayee", designatedPayee);
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.RecapturedDisbursementTransaction.CpaType = cpaType;
    entities.RecapturedDisbursementTransaction.CspNumber = cspNumber;
    entities.RecapturedDisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.RecapturedDisbursementTransaction.Type1 = type1;
    entities.RecapturedDisbursementTransaction.Amount = amount;
    entities.RecapturedDisbursementTransaction.ProcessDate = processDate;
    entities.RecapturedDisbursementTransaction.CreatedBy = createdBy;
    entities.RecapturedDisbursementTransaction.CreatedTimestamp =
      createdTimestamp;
    entities.RecapturedDisbursementTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.RecapturedDisbursementTransaction.LastUpdateTmst = lastUpdateTmst;
    entities.RecapturedDisbursementTransaction.DisbursementDate =
      disbursementDate;
    entities.RecapturedDisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.RecapturedDisbursementTransaction.RecapturedInd = recapturedInd;
    entities.RecapturedDisbursementTransaction.CollectionDate = collectionDate;
    entities.RecapturedDisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.RecapturedDisbursementTransaction.InterstateInd = interstateInd;
    entities.RecapturedDisbursementTransaction.PassthruProcDate =
      passthruProcDate;
    entities.RecapturedDisbursementTransaction.DesignatedPayee =
      designatedPayee;
    entities.RecapturedDisbursementTransaction.ReferenceNumber =
      referenceNumber;
    entities.RecapturedDisbursementTransaction.IntInterId = null;
    entities.RecapturedDisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.RecapturedDisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    System.Diagnostics.Debug.Assert(
      entities.RecapturedDisbursementTransaction.Populated);

    var systemGeneratedIdentifier = 1;
    var description = local.DisbursementTransactionRln.Description ?? "";
    var createdBy = local.DisbursementTransactionRln.CreatedBy;
    var createdTimestamp = local.DisbursementTransactionRln.CreatedTimestamp;
    var dnrGeneratedId =
      entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier;
    var cspNumber = entities.RecapturedDisbursementTransaction.CspNumber;
    var cpaType = entities.RecapturedDisbursementTransaction.CpaType;
    var dtrGeneratedId =
      entities.RecapturedDisbursementTransaction.SystemGeneratedIdentifier;
    var cspPNumber = entities.Credit.CspNumber;
    var cpaPType = entities.Credit.CpaType;
    var dtrPGeneratedId = entities.Credit.SystemGeneratedIdentifier;

    CheckValid<DisbursementTransactionRln>("CpaType", cpaType);
    CheckValid<DisbursementTransactionRln>("CpaPType", cpaPType);
    entities.DisbursementTransactionRln.Populated = false;
    Update("CreateDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "disbTranRlnId", systemGeneratedIdentifier);
        db.SetNullableString(command, "description", description);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "dnrGeneratedId", dnrGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "dtrGeneratedId", dtrGeneratedId);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetString(command, "cpaPType", cpaPType);
        db.SetInt32(command, "dtrPGeneratedId", dtrPGeneratedId);
      });

    entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransactionRln.Description = description;
    entities.DisbursementTransactionRln.CreatedBy = createdBy;
    entities.DisbursementTransactionRln.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransactionRln.DnrGeneratedId = dnrGeneratedId;
    entities.DisbursementTransactionRln.CspNumber = cspNumber;
    entities.DisbursementTransactionRln.CpaType = cpaType;
    entities.DisbursementTransactionRln.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementTransactionRln.CspPNumber = cspPNumber;
    entities.DisbursementTransactionRln.CpaPType = cpaPType;
    entities.DisbursementTransactionRln.DtrPGeneratedId = dtrPGeneratedId;
    entities.DisbursementTransactionRln.Populated = true;
  }

  private void DisassociatePaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Original.Populated);
    entities.Original.Populated = false;
    Update("DisassociatePaymentRequest",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Original.CpaType);
        db.SetString(command, "cspNumber", entities.Original.CspNumber);
        db.SetInt32(
          command, "disbTranId", entities.Original.SystemGeneratedIdentifier);
      });

    entities.Original.PrqGeneratedId = null;
    entities.Original.Populated = true;
  }

  private IEnumerable<bool> ReadDisbursementStatusHistoryDisbursementStatus()
  {
    System.Diagnostics.Debug.Assert(entities.Original.Populated);
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementStatus.Populated = false;

    return ReadEach("ReadDisbursementStatusHistoryDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.Original.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Original.CspNumber);
        db.SetString(command, "cpaType", entities.Original.CpaType);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.CreatedBy = db.GetString(reader, 7);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 10);
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementStatus.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);

        return true;
      });
  }

  private bool ReadDisbursementTranRlnRsn()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbursementTransactionRln.Populated);
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTrnRlnRsId",
          entities.DisbursementTransactionRln.DnrGeneratedId);
      },
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbursementTransactionRln.Populated);
    entities.Credit.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransactionRln.DtrPGeneratedId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransactionRln.CpaPType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransactionRln.CspPNumber);
          
      },
      (db, reader) =>
      {
        entities.Credit.CpaType = db.GetString(reader, 0);
        entities.Credit.CspNumber = db.GetString(reader, 1);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Credit.Type1 = db.GetString(reader, 3);
        entities.Credit.InterstateInd = db.GetNullableString(reader, 4);
        entities.Credit.IntInterId = db.GetNullableInt32(reader, 5);
        entities.Credit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Credit.Type1);
      });
  }

  private bool ReadDisbursementTransaction2()
  {
    entities.Original.Populated = false;

    return Read("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          import.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Original.CpaType = db.GetString(reader, 0);
        entities.Original.CspNumber = db.GetString(reader, 1);
        entities.Original.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Original.Type1 = db.GetString(reader, 3);
        entities.Original.Amount = db.GetDecimal(reader, 4);
        entities.Original.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Original.CreatedBy = db.GetString(reader, 6);
        entities.Original.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.Original.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Original.LastUpdateTmst = db.GetNullableDateTime(reader, 9);
        entities.Original.DisbursementDate = db.GetNullableDate(reader, 10);
        entities.Original.CashNonCashInd = db.GetString(reader, 11);
        entities.Original.RecapturedInd = db.GetString(reader, 12);
        entities.Original.CollectionDate = db.GetNullableDate(reader, 13);
        entities.Original.DbtGeneratedId = db.GetNullableInt32(reader, 14);
        entities.Original.PrqGeneratedId = db.GetNullableInt32(reader, 15);
        entities.Original.InterstateInd = db.GetNullableString(reader, 16);
        entities.Original.PassthruProcDate = db.GetNullableDate(reader, 17);
        entities.Original.DesignatedPayee = db.GetNullableString(reader, 18);
        entities.Original.ReferenceNumber = db.GetNullableString(reader, 19);
        entities.Original.ExcessUraInd = db.GetNullableString(reader, 20);
        entities.Original.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Original.CpaType);
          
        CheckValid<DisbursementTransaction>("Type1", entities.Original.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Original.CashNonCashInd);
      });
  }

  private bool ReadDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Original.Populated);
    entities.DisbursementTransactionRln.Populated = false;

    return Read("ReadDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.Original.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Original.CpaType);
        db.SetString(command, "cspNumber", entities.Original.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionRln.Description =
          db.GetNullableString(reader, 1);
        entities.DisbursementTransactionRln.CreatedBy = db.GetString(reader, 2);
        entities.DisbursementTransactionRln.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 4);
        entities.DisbursementTransactionRln.CspNumber = db.GetString(reader, 5);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 6);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 7);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 8);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 9);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 10);
        entities.DisbursementTransactionRln.Populated = true;
        CheckValid<DisbursementTransactionRln>("CpaType",
          entities.DisbursementTransactionRln.CpaType);
        CheckValid<DisbursementTransactionRln>("CpaPType",
          entities.DisbursementTransactionRln.CpaPType);
      });
  }

  private bool ReadDisbursementType1()
  {
    System.Diagnostics.Debug.Assert(entities.Original.Populated);
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          entities.Original.DbtGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.CurrentArrearsInd =
          db.GetNullableString(reader, 2);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 3);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 4);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadDisbursementType2()
  {
    entities.RecapturedDisbursementType.Populated = false;

    return Read("ReadDisbursementType2",
      (db, command) =>
      {
        db.SetString(command, "code", local.NewRecaptured.Code);
      },
      (db, reader) =>
      {
        entities.RecapturedDisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.RecapturedDisbursementType.Code = db.GetString(reader, 1);
        entities.RecapturedDisbursementType.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.Credit.IntInterId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee2.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee2.CspNumber = db.GetString(reader, 0);
        entities.Obligee2.Type1 = db.GetString(reader, 1);
        entities.Obligee2.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee2.Type1);
      });
  }

  private bool ReadPaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Original.Populated);
    entities.Warrant.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.Original.PrqGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Warrant.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Warrant.ProcessDate = db.GetDate(reader, 1);
        entities.Warrant.Amount = db.GetDecimal(reader, 2);
        entities.Warrant.Type1 = db.GetString(reader, 3);
        entities.Warrant.PrqRGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Warrant.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.Warrant.Type1);
      });
  }

  private void TransferDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Original.Populated);

    var dbtGeneratedId =
      entities.RecapturedDisbursementType.SystemGeneratedIdentifier;

    entities.Original.Populated = false;
    Update("TransferDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "dbtGeneratedId", dbtGeneratedId);
        db.SetString(command, "cpaType", entities.Original.CpaType);
        db.SetString(command, "cspNumber", entities.Original.CspNumber);
        db.SetInt32(
          command, "disbTranId", entities.Original.SystemGeneratedIdentifier);
      });

    entities.Original.DbtGeneratedId = dbtGeneratedId;
    entities.Original.Populated = true;
  }

  private void UpdateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Original.Populated);

    var amount = entities.Original.Amount - import.RecapAmountDebit.Amount;
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = Now();

    entities.Original.Populated = false;
    Update("UpdateDisbursementTransaction",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.Original.CpaType);
        db.SetString(command, "cspNumber", entities.Original.CspNumber);
        db.SetInt32(
          command, "disbTranId", entities.Original.SystemGeneratedIdentifier);
      });

    entities.Original.Amount = amount;
    entities.Original.LastUpdatedBy = lastUpdatedBy;
    entities.Original.LastUpdateTmst = lastUpdateTmst;
    entities.Original.Populated = true;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of RecapAmountDebit.
    /// </summary>
    [JsonPropertyName("recapAmountDebit")]
    public DisbursementTransaction RecapAmountDebit
    {
      get => recapAmountDebit ??= new();
      set => recapAmountDebit = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Recap.
    /// </summary>
    [JsonPropertyName("recap")]
    public PaymentRequest Recap
    {
      get => recap ??= new();
      set => recap = value;
    }

    private DisbursementTransaction disbursementTransaction;
    private ProgramProcessingInfo programProcessingInfo;
    private DisbursementTransaction recapAmountDebit;
    private CsePerson obligee;
    private PaymentRequest recap;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Recap.
    /// </summary>
    [JsonPropertyName("recap")]
    public PaymentRequest Recap
    {
      get => recap ??= new();
      set => recap = value;
    }

    private PaymentRequest recap;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
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
    /// A value of Recapture.
    /// </summary>
    [JsonPropertyName("recapture")]
    public PaymentRequest Recapture
    {
      get => recapture ??= new();
      set => recapture = value;
    }

    /// <summary>
    /// A value of NewRecaptured.
    /// </summary>
    [JsonPropertyName("newRecaptured")]
    public DisbursementType NewRecaptured
    {
      get => newRecaptured ??= new();
      set => newRecaptured = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CreateAttempts.
    /// </summary>
    [JsonPropertyName("createAttempts")]
    public Common CreateAttempts
    {
      get => createAttempts ??= new();
      set => createAttempts = value;
    }

    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    private DisbursementTransaction disbursementTransaction;
    private DisbursementTransactionRln disbursementTransactionRln;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private PaymentRequest recapture;
    private DisbursementType newRecaptured;
    private DateWorkArea max;
    private DateWorkArea current;
    private Common createAttempts;
    private Common dummy;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Original.
    /// </summary>
    [JsonPropertyName("original")]
    public DisbursementTransaction Original
    {
      get => original ??= new();
      set => original = value;
    }

    /// <summary>
    /// A value of RecapturedDisbursementType.
    /// </summary>
    [JsonPropertyName("recapturedDisbursementType")]
    public DisbursementType RecapturedDisbursementType
    {
      get => recapturedDisbursementType ??= new();
      set => recapturedDisbursementType = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of NewForRecapDisb.
    /// </summary>
    [JsonPropertyName("newForRecapDisb")]
    public DisbursementStatusHistory NewForRecapDisb
    {
      get => newForRecapDisb ??= new();
      set => newForRecapDisb = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    /// <summary>
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of Warrant.
    /// </summary>
    [JsonPropertyName("warrant")]
    public PaymentRequest Warrant
    {
      get => warrant ??= new();
      set => warrant = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePerson Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePersonAccount Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of RecapturedDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("recapturedDisbursementTransaction")]
    public DisbursementTransaction RecapturedDisbursementTransaction
    {
      get => recapturedDisbursementTransaction ??= new();
      set => recapturedDisbursementTransaction = value;
    }

    private DisbursementTransaction original;
    private DisbursementType recapturedDisbursementType;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatusHistory newForRecapDisb;
    private DisbursementStatus disbursementStatus;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
    private DisbursementTransaction credit;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementType disbursementType;
    private PaymentRequest warrant;
    private CsePerson obligee1;
    private CsePersonAccount obligee2;
    private InterstateRequest interstateRequest;
    private DisbursementTransaction recapturedDisbursementTransaction;
  }
#endregion
}
