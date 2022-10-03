// Program: FN_CREATE_DISBURSEMENT_NEW, ID: 370952974, model: 746.
// Short name: SWE02689
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_DISBURSEMENT_NEW.
/// </para>
/// <para>
/// RESP: Finance
/// This action block will create a disbursement transaction and a Disbursement 
/// Status History.  Associate it to a Disb Collection, Disbursement Type,
/// Disbursement Status.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateDisbursementNew: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_DISBURSEMENT_NEW program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateDisbursementNew(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateDisbursementNew.
  /// </summary>
  public FnCreateDisbursementNew(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // ---------------------------------------------------------------------------------------
    // 00/09/06  Fangman  PR 103323  Copied from FN_CREATE_DISBURSEMENT and 
    // recoded for batch I/O.
    // 00/09/18  Fangman  PRWORA  Changed proporties on read stmts.
    // 00/09/28  Fangman  PR 98039  Changed code for creating the disbursement 
    // status history.
    // ---------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Timestamp = Now();

    if (AsChar(import.PerCredit.InterstateInd) == 'Y')
    {
      if (!ReadInterstateRequest())
      {
        ExitState = "INTERSTATE_REQUEST_NF";

        return;
      }
    }

    if (import.DisbursementType.SystemGeneratedIdentifier == 1 || import
      .DisbursementType.SystemGeneratedIdentifier == 2 || import
      .DisbursementType.SystemGeneratedIdentifier == 4 || import
      .DisbursementType.SystemGeneratedIdentifier == 5 || import
      .DisbursementType.SystemGeneratedIdentifier == 73)
    {
      // These 5 disb types are passed in as persistent views so Disb Type does 
      // not have to be read.
    }
    else if (!ReadDisbursementType())
    {
      ExitState = "FN0000_DISB_TYPE_NF";

      return;
    }

    if (import.DisbursementType.SystemGeneratedIdentifier == 73)
    {
      local.ForUpdate.ExcessUraInd = "";
    }
    else
    {
      local.ForUpdate.ExcessUraInd = import.PerCredit.ExcessUraInd;
    }

    local.Dummy.Flag = "Y";

    if (AsChar(local.Dummy.Flag) == 'Y')
    {
      local.TranCreated.Flag = "N";

      for(local.CreateAttempts.Count = 1; local.CreateAttempts.Count <= 10; ++
        local.CreateAttempts.Count)
      {
        switch(import.DisbursementType.SystemGeneratedIdentifier)
        {
          case 1:
            if (entities.InterstateRequest.Populated)
            {
              try
              {
                CreateDisbursementTransaction3();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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
            else
            {
              try
              {
                CreateDisbursementTransaction4();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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

            break;
          case 2:
            if (entities.InterstateRequest.Populated)
            {
              try
              {
                CreateDisbursementTransaction5();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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
            else
            {
              try
              {
                CreateDisbursementTransaction6();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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

            break;
          case 4:
            if (entities.InterstateRequest.Populated)
            {
              try
              {
                CreateDisbursementTransaction7();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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
            else
            {
              try
              {
                CreateDisbursementTransaction8();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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

            break;
          case 5:
            if (entities.InterstateRequest.Populated)
            {
              try
              {
                CreateDisbursementTransaction9();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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
            else
            {
              try
              {
                CreateDisbursementTransaction10();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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

            break;
          case 73:
            if (entities.InterstateRequest.Populated)
            {
              try
              {
                CreateDisbursementTransaction11();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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
            else
            {
              try
              {
                CreateDisbursementTransaction12();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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

            break;
          default:
            if (entities.InterstateRequest.Populated)
            {
              try
              {
                CreateDisbursementTransaction1();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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
            else
            {
              try
              {
                CreateDisbursementTransaction2();
                local.TranCreated.Flag = "Y";

                goto Test;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
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

            break;
        }
      }

      ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

      return;
    }

Test:

    if (AsChar(local.TranCreated.Flag) == 'N')
    {
      ExitState = "FN0000_DISB_TRANSACTION_AE";

      return;
    }

    export.New1.SystemGeneratedIdentifier =
      entities.DisbursementTransaction.SystemGeneratedIdentifier;
    import.ExpDatabaseUpdated.Flag = "Y";

    if (AsChar(import.TestDisplayInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Created DB Disb w/ ID " + NumberToString
        (export.New1.SystemGeneratedIdentifier, 7, 9) + "  Type " + NumberToString
        (import.DisbursementType.SystemGeneratedIdentifier, 13, 3) + "  Amt " +
        NumberToString
        ((long)(entities.DisbursementTransaction.Amount * 100), 9, 7) + "  Status " +
        NumberToString
        (import.DisbursementStatus.SystemGeneratedIdentifier, 13, 3);
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // Create the disbursement transaction relation and associate it to the disb
    // collection disbursement transaction and the disbursement tran rln rsn
    // and to the new disbursement.
    try
    {
      CreateDisbursementTransactionRln();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
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

    // Create the disbursement status history and associate it to the 
    // disbursement status and the new disbursement disbursement transaction.
    if (import.DisbursementStatus.SystemGeneratedIdentifier == 3 && Lt
      (local.Initialized.Date, import.HighestSuppressionDate.Date))
    {
      // *****  If this disbursement is to be suppressed(3) then give it the 
      // highest found suppression end date.  *****
      local.DiscontinueDate.Date = import.HighestSuppressionDate.Date;
    }
    else
    {
      local.DiscontinueDate.Date = import.Max.Date;
    }

    switch(import.DisbursementStatus.SystemGeneratedIdentifier)
    {
      case 1:
        try
        {
          CreateDisbursementStatusHistory1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_STAT_HIST_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case 2:
        try
        {
          CreateDisbursementStatusHistory2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_STAT_HIST_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case 3:
        // -------------------  add statement to 
        // set the reason code
        // --------------------------------------
        try
        {
          CreateDisbursementStatusHistory3();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_STAT_HIST_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      default:
        ExitState = "FN0000_DISB_STATUS_NF";

        break;
    }
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateDisbursementStatusHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var dbsGeneratedId = import.Per1Released.SystemGeneratedIdentifier;
    var dtrGeneratedId =
      entities.DisbursementTransaction.SystemGeneratedIdentifier;
    var cspNumber = entities.DisbursementTransaction.CspNumber;
    var cpaType = entities.DisbursementTransaction.CpaType;
    var systemGeneratedIdentifier = 1;
    var effectiveDate = import.New1.DisbursementDate;
    var discontinueDate = local.DiscontinueDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var suppressionReason = import.ForCreate.SuppressionReason ?? "";

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.DisbursementStatusHistory.Populated = false;
    Update("CreateDisbursementStatusHistory1",
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
        db.SetNullableString(command, "reasonText", "");
        db.SetNullableString(command, "suppressionReason", suppressionReason);
      });

    entities.DisbursementStatusHistory.DbsGeneratedId = dbsGeneratedId;
    entities.DisbursementStatusHistory.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementStatusHistory.CspNumber = cspNumber;
    entities.DisbursementStatusHistory.CpaType = cpaType;
    entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.CreatedBy = createdBy;
    entities.DisbursementStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbursementStatusHistory.SuppressionReason = suppressionReason;
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void CreateDisbursementStatusHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var dbsGeneratedId = import.Per2Processed.SystemGeneratedIdentifier;
    var dtrGeneratedId =
      entities.DisbursementTransaction.SystemGeneratedIdentifier;
    var cspNumber = entities.DisbursementTransaction.CspNumber;
    var cpaType = entities.DisbursementTransaction.CpaType;
    var systemGeneratedIdentifier = 1;
    var effectiveDate = import.New1.DisbursementDate;
    var discontinueDate = local.DiscontinueDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var suppressionReason = import.ForCreate.SuppressionReason ?? "";

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.DisbursementStatusHistory.Populated = false;
    Update("CreateDisbursementStatusHistory2",
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
        db.SetNullableString(command, "reasonText", "");
        db.SetNullableString(command, "suppressionReason", suppressionReason);
      });

    entities.DisbursementStatusHistory.DbsGeneratedId = dbsGeneratedId;
    entities.DisbursementStatusHistory.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementStatusHistory.CspNumber = cspNumber;
    entities.DisbursementStatusHistory.CpaType = cpaType;
    entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.CreatedBy = createdBy;
    entities.DisbursementStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbursementStatusHistory.SuppressionReason = suppressionReason;
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void CreateDisbursementStatusHistory3()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var dbsGeneratedId = import.Per3Suppressed.SystemGeneratedIdentifier;
    var dtrGeneratedId =
      entities.DisbursementTransaction.SystemGeneratedIdentifier;
    var cspNumber = entities.DisbursementTransaction.CspNumber;
    var cpaType = entities.DisbursementTransaction.CpaType;
    var systemGeneratedIdentifier = 1;
    var effectiveDate = import.New1.DisbursementDate;
    var discontinueDate = local.DiscontinueDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var suppressionReason = import.ForCreate.SuppressionReason ?? "";

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.DisbursementStatusHistory.Populated = false;
    Update("CreateDisbursementStatusHistory3",
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
        db.SetNullableString(command, "reasonText", "");
        db.SetNullableString(command, "suppressionReason", suppressionReason);
      });

    entities.DisbursementStatusHistory.DbsGeneratedId = dbsGeneratedId;
    entities.DisbursementStatusHistory.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementStatusHistory.CspNumber = cspNumber;
    entities.DisbursementStatusHistory.CpaType = cpaType;
    entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.CreatedBy = createdBy;
    entities.DisbursementStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbursementStatusHistory.SuppressionReason = suppressionReason;
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void CreateDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = entities.DisbursementType.RecaptureInd ?? Spaces(1);
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = entities.DisbursementType.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var intInterId = entities.InterstateRequest.IntHGeneratedId;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction1",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = intInterId;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction10()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per5NaAcs.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction10",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = null;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction11()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per73CrFee.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var intInterId = entities.InterstateRequest.IntHGeneratedId;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction11",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = intInterId;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction12()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per73CrFee.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction12",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = null;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = entities.DisbursementType.RecaptureInd ?? Spaces(1);
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = entities.DisbursementType.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction2",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = null;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction3()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per1AfCcs.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var intInterId = entities.InterstateRequest.IntHGeneratedId;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction3",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = intInterId;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction4()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per1AfCcs.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction4",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = null;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction5()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per2AfAcs.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var intInterId = entities.InterstateRequest.IntHGeneratedId;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction5",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = intInterId;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction6()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per2AfAcs.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction6",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = null;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction7()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per4NaCcs.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var intInterId = entities.InterstateRequest.IntHGeneratedId;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction7",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = intInterId;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction8()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per4NaCcs.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction8",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = null;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransaction9()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    var cpaType = import.PerObligee.Type1;
    var cspNumber = import.PerObligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "D";
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = "N";
    var collectionDate = import.PerCredit.CollectionDate;
    var dbtGeneratedId = import.Per5NaAcs.SystemGeneratedIdentifier;
    var interstateInd = import.PerCredit.InterstateInd;
    var referenceNumber = import.PerCredit.ReferenceNumber;
    var intInterId = entities.InterstateRequest.IntHGeneratedId;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction9",
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
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
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
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.DisbursementDate = disbursementDate;
    entities.DisbursementTransaction.CashNonCashInd = cashNonCashInd;
    entities.DisbursementTransaction.RecapturedInd = recapturedInd;
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.DbtGeneratedId = dbtGeneratedId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = intInterId;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(import.PerCredit.Populated);
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var systemGeneratedIdentifier = 1;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var dnrGeneratedId = import.Per1.SystemGeneratedIdentifier;
    var cspNumber = entities.DisbursementTransaction.CspNumber;
    var cpaType = entities.DisbursementTransaction.CpaType;
    var dtrGeneratedId =
      entities.DisbursementTransaction.SystemGeneratedIdentifier;
    var cspPNumber = import.PerCredit.CspNumber;
    var cpaPType = import.PerCredit.CpaType;
    var dtrPGeneratedId = import.PerCredit.SystemGeneratedIdentifier;

    CheckValid<DisbursementTransactionRln>("CpaType", cpaType);
    CheckValid<DisbursementTransactionRln>("CpaPType", cpaPType);
    entities.DisbursementTransactionRln.Populated = false;
    Update("CreateDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "disbTranRlnId", systemGeneratedIdentifier);
        db.SetNullableString(command, "description", "");
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

  private bool ReadDisbursementType()
  {
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          import.DisbursementType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 1);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(import.PerCredit.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          import.PerCredit.IntInterId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
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
    /// A value of ExpDatabaseUpdated.
    /// </summary>
    [JsonPropertyName("expDatabaseUpdated")]
    public Common ExpDatabaseUpdated
    {
      get => expDatabaseUpdated ??= new();
      set => expDatabaseUpdated = value;
    }

    /// <summary>
    /// A value of PerObligee.
    /// </summary>
    [JsonPropertyName("perObligee")]
    public CsePersonAccount PerObligee
    {
      get => perObligee ??= new();
      set => perObligee = value;
    }

    /// <summary>
    /// A value of PerCredit.
    /// </summary>
    [JsonPropertyName("perCredit")]
    public DisbursementTransaction PerCredit
    {
      get => perCredit ??= new();
      set => perCredit = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbursementTransaction New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of HighestSuppressionDate.
    /// </summary>
    [JsonPropertyName("highestSuppressionDate")]
    public DateWorkArea HighestSuppressionDate
    {
      get => highestSuppressionDate ??= new();
      set => highestSuppressionDate = value;
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
    /// A value of Per1AfCcs.
    /// </summary>
    [JsonPropertyName("per1AfCcs")]
    public DisbursementType Per1AfCcs
    {
      get => per1AfCcs ??= new();
      set => per1AfCcs = value;
    }

    /// <summary>
    /// A value of Per2AfAcs.
    /// </summary>
    [JsonPropertyName("per2AfAcs")]
    public DisbursementType Per2AfAcs
    {
      get => per2AfAcs ??= new();
      set => per2AfAcs = value;
    }

    /// <summary>
    /// A value of Per4NaCcs.
    /// </summary>
    [JsonPropertyName("per4NaCcs")]
    public DisbursementType Per4NaCcs
    {
      get => per4NaCcs ??= new();
      set => per4NaCcs = value;
    }

    /// <summary>
    /// A value of Per5NaAcs.
    /// </summary>
    [JsonPropertyName("per5NaAcs")]
    public DisbursementType Per5NaAcs
    {
      get => per5NaAcs ??= new();
      set => per5NaAcs = value;
    }

    /// <summary>
    /// A value of Per73CrFee.
    /// </summary>
    [JsonPropertyName("per73CrFee")]
    public DisbursementType Per73CrFee
    {
      get => per73CrFee ??= new();
      set => per73CrFee = value;
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
    /// A value of Per1Released.
    /// </summary>
    [JsonPropertyName("per1Released")]
    public DisbursementStatus Per1Released
    {
      get => per1Released ??= new();
      set => per1Released = value;
    }

    /// <summary>
    /// A value of Per2Processed.
    /// </summary>
    [JsonPropertyName("per2Processed")]
    public DisbursementStatus Per2Processed
    {
      get => per2Processed ??= new();
      set => per2Processed = value;
    }

    /// <summary>
    /// A value of Per3Suppressed.
    /// </summary>
    [JsonPropertyName("per3Suppressed")]
    public DisbursementStatus Per3Suppressed
    {
      get => per3Suppressed ??= new();
      set => per3Suppressed = value;
    }

    /// <summary>
    /// A value of Per1.
    /// </summary>
    [JsonPropertyName("per1")]
    public DisbursementTranRlnRsn Per1
    {
      get => per1 ??= new();
      set => per1 = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public DisbursementStatusHistory ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
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
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    private Common expDatabaseUpdated;
    private CsePersonAccount perObligee;
    private DisbursementTransaction perCredit;
    private DisbursementTransaction new1;
    private DateWorkArea highestSuppressionDate;
    private DisbursementType disbursementType;
    private DisbursementType per1AfCcs;
    private DisbursementType per2AfAcs;
    private DisbursementType per4NaCcs;
    private DisbursementType per5NaAcs;
    private DisbursementType per73CrFee;
    private DisbursementStatus disbursementStatus;
    private DisbursementStatus per1Released;
    private DisbursementStatus per2Processed;
    private DisbursementStatus per3Suppressed;
    private DisbursementTranRlnRsn per1;
    private DisbursementStatusHistory forCreate;
    private DateWorkArea max;
    private Common testDisplayInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbursementTransaction New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private DisbursementTransaction new1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public DisbursementTransaction ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of DiscontinueDate.
    /// </summary>
    [JsonPropertyName("discontinueDate")]
    public DateWorkArea DiscontinueDate
    {
      get => discontinueDate ??= new();
      set => discontinueDate = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of TranCreated.
    /// </summary>
    [JsonPropertyName("tranCreated")]
    public Common TranCreated
    {
      get => tranCreated ??= new();
      set => tranCreated = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private DisbursementTransaction forUpdate;
    private DateWorkArea discontinueDate;
    private DateWorkArea initialized;
    private DateWorkArea current;
    private Common createAttempts;
    private Common tranCreated;
    private Common dummy;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

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
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    private InterstateRequest interstateRequest;
    private DisbursementType disbursementType;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementStatusHistory disbursementStatusHistory;
  }
#endregion
}
