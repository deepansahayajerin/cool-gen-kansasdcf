// Program: FN_B651_APPLY_DUP_TO_POT_RCV_OBL, ID: 371004906, model: 746.
// Short name: SWE02691
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_APPLY_DUP_TO_POT_RCV_OBL.
/// </summary>
[Serializable]
public partial class FnB651ApplyDupToPotRcvObl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_APPLY_DUP_TO_POT_RCV_OBL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651ApplyDupToPotRcvObl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651ApplyDupToPotRcvObl.
  /// </summary>
  public FnB651ApplyDupToPotRcvObl(IContext context, Import import,
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
    // ****************************************************************
    // 09-25-00  PR 98039  Fangman - New code to apply disbursements to 
    // potential recovery obligations that have not been created or denied.
    // 12-07-00  PR 108996  Fangman - Added code to set the filler to D if the 
    // payment request is being updated for a duplicate disbursement.  Removed
    // unused view.
    // 04-13-01  PR 118495  Fangman - Changed code to net the positive 
    // disbursements with the negative disbursements when calculating
    // Amt_To_Apply_To_Pot_Rcv.
    // 01/31/02  WR 000235  Fangman - PSUM redesign.  Added code to keep track 
    // of the type of monthly totals created in the AB.
    // 05/08/02  PR 146227  Added code to increase the 3 monthly summary fields 
    // by the amount that was associated to the recovery obligation.
    // ****************************************************************
    // Look at each Potential Recovery Obligation that has not been created or 
    // denied.  If that Potential Recovery Obligation has disbursements from the
    // same Cash Receipt Detail (ref # is the same) then money can be applied
    // to the Potential Recovery Obligation.
    foreach(var item in ReadPaymentRequestPaymentStatusHistory())
    {
      // Total all of the negative disbursements for the same cash receipt 
      // detail (ref nbr) that make up all or part of this potential recovery
      // obligation.  This total is the max amt of disbursements that can be
      // applied to the potential recovery obligation.
      ReadDisbursement();

      // If any negative disbursements for the cash receipt detail (ref nbr) 
      // were found then the total would be less than 0.
      if (local.AmtToApplyToPotRcv.Amount < 0)
      {
        // Mulitiply by -1 to make it a postive amt.
        local.AmtToApplyToPotRcv.Amount = -local.AmtToApplyToPotRcv.Amount;

        // We can't apply more money than the total of the potential recovery 
        // obligation.  This check is needed because there could have been
        // positive disbursements from other cash receipt details (ref nbrs)
        // that were combined with the negatives to make up this potential
        // recovery obligation.
        if (local.AmtToApplyToPotRcv.Amount > entities.PaymentRequest.Amount)
        {
          local.AmtToApplyToPotRcv.Amount = entities.PaymentRequest.Amount;
        }

        // We can't apply more money than is currently left to be processed from
        // the disbursement.
        if (local.AmtToApplyToPotRcv.Amount > import
          .ExpAmtRemainingToDisburs.Amount)
        {
          local.AmtToApplyToPotRcv.Amount =
            import.ExpAmtRemainingToDisburs.Amount;
        }

        if (AsChar(import.TestDisplayInd.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.UnformattedAmt.Number112 = local.AmtToApplyToPotRcv.Amount;
          UseCabFormat112AmtFieldTo8();
          local.EabReportSend.RptDetail =
            "  D Suppr:  Amount of disb to apply to RCV " + local
            .FormattedAmt.Text9;
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // Apply the amount to the potential recovery obligation by decreasing 
        // the amt.
        try
        {
          UpdatePaymentRequest();

          // Keep track of the amount of the disb remaining in case we can apply
          // some of it to the next potential recovery obligation.
          import.ExpDatabaseUpdated.Flag = "Y";
          import.ExpAmtRemainingToDisburs.Amount -= local.AmtToApplyToPotRcv.
            Amount;

          if (entities.PaymentRequest.Amount == 0)
          {
            // If the money being applied to the potential recovery obligation 
            // pays off the entire obligation then the status of the potential
            // recovery obligation should be set to denied.
            try
            {
              UpdatePaymentStatusHistory();

              try
              {
                CreatePaymentStatusHistory();
              }
              catch(Exception e2)
              {
                switch(GetErrorCode(e2))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_PAYMENT_STATUS_HIST_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PMT_STAT_HIST_PV";

                    return;
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
                  ExitState = "FN0000_PMT_STAT_HIST_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_PMT_STAT_HIST_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_PAYMENT_REQUEST_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_PAYMENT_REQUEST_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // Now create the DB disbursement and associate it to the potential 
        // recovery obligation setting the status of the new disbursement to
        // processed (also set the process date).
        local.ForCreateDisbursementTransaction.Type1 = "D";
        local.ForCreateDisbursementTransaction.Amount =
          local.AmtToApplyToPotRcv.Amount;
        local.ForCreateDisbursementTransaction.DisbursementDate =
          import.ProgramProcessingInfo.ProcessDate;
        local.ForCreateDisbursementTransaction.ProcessDate =
          import.ProgramProcessingInfo.ProcessDate;
        local.ForCreateDisbursementTransaction.CashNonCashInd =
          import.CashReceiptType.CategoryIndicator;
        UseFnCreateDisbursementNew();
        ++import.ExpNbrOfDisbCreated.Count;
        import.ExpAmtOfDisbCreated.TotalCurrency += local.
          ForCreateDisbursementTransaction.Amount;

        if (AsChar(import.PerDisbursementTransaction.ExcessUraInd) == 'Y')
        {
          export.ImpExpToUpdate.TotExcessUraAmt =
            export.ImpExpToUpdate.TotExcessUraAmt.GetValueOrDefault() + local
            .ForCreateDisbursementTransaction.Amount;
        }
        else if (ReadDisbursementType())
        {
          if (Equal(entities.DisbursementType.ProgramCode, "AF"))
          {
            export.ImpExpToUpdate.AdcReimbursedAmount =
              export.ImpExpToUpdate.AdcReimbursedAmount.GetValueOrDefault() + local
              .ForCreateDisbursementTransaction.Amount;
          }
          else
          {
            export.ImpExpToUpdate.CollectionsDisbursedToAr =
              export.ImpExpToUpdate.CollectionsDisbursedToAr.
                GetValueOrDefault() + local
              .ForCreateDisbursementTransaction.Amount;
          }
        }
        else
        {
          ExitState = "FN0000_DISB_TYPE_NF";

          return;
        }

        // Associate this disbursement to the potential recovery obligation.
        if (ReadDisbursementTransaction())
        {
          AssociateDisbursementTransaction();
        }
        else
        {
          ExitState = "FN0000_DISB_TRANS_NF";

          return;
        }

        // *****  Escape if no money left  *****
        if (import.ExpAmtRemainingToDisburs.Amount <= 0)
        {
          return;
        }
      }
    }

    // At this point we could take any remaining disbursement amount & look for
    // "created" potential recovery obligations associated to disbursements
    // with the same reference number where the recovery obligation has a
    // remaining balance & is turned on for recaptures & there are recapture
    // rules for this AR of 100% for the same type (current/arrears) as the
    // disbursement.  If found we could release the remaining disbursement
    // amount & it would be recaptured.
    // For now we determined that these conditions would not happen often enough
    // to justify this coding.
    // If money is left over then it must be suppressed.  Create a 'D' 
    // suppression rule.
    if (ReadDisbSuppressionStatusHistory1())
    {
      // If a "D" rule already exits for this person for the same date then do 
      // not create another one.
      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "  D Suppr:  D suppr rule already exists for effective date: " + NumberToString
          (DateToInt(import.ProgramProcessingInfo.ProcessDate), 8, 8);
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      return;
    }

    local.LastId.SystemGeneratedIdentifier = 0;

    if (ReadDisbSuppressionStatusHistory2())
    {
      local.LastId.SystemGeneratedIdentifier =
        entities.ReadForId.SystemGeneratedIdentifier;
    }

    try
    {
      CreateDisbSuppressionStatusHistory();
      import.ExpDatabaseUpdated.Flag = "Y";

      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "  D Suppr:  Creating D suppr rule for effective date: " + NumberToString
          (DateToInt(import.ProgramProcessingInfo.ProcessDate), 8, 8);
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DISB_SUPP_STAT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DISB_SUPP_STAT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
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

  private void UseCabFormat112AmtFieldTo8()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.UnformattedAmt.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedAmt.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseFnCreateDisbursementNew()
  {
    var useImport = new FnCreateDisbursementNew.Import();
    var useExport = new FnCreateDisbursementNew.Export();

    useImport.PerObligee.Assign(import.PerObligeeCsePersonAccount);
    useImport.PerCredit.Assign(import.PerDisbursementTransaction);
    useImport.DisbursementType.SystemGeneratedIdentifier =
      import.DisbursementType.SystemGeneratedIdentifier;
    useImport.Per1AfCcs.Assign(import.Per1AfCcs);
    useImport.Per2AfAcs.Assign(import.Per2AfAcs);
    useImport.Per4NaCcs.Assign(import.Per4NaCcs);
    useImport.Per5NaAcs.Assign(import.Per5NaAcs);
    useImport.Per73CrFee.Assign(import.Per73CrFee);
    useImport.Per2Processed.Assign(import.Per2Processed);
    useImport.DisbursementStatus.SystemGeneratedIdentifier =
      import.Per2Processed.SystemGeneratedIdentifier;
    useImport.Per1.Assign(import.PerDisbursementTranRlnRsn);
    useImport.Max.Date = import.Max.Date;
    useImport.HighestSuppressionDate.Date = import.Max.Date;
    useImport.TestDisplayInd.Flag = import.TestDisplayInd.Flag;
    useImport.ForCreate.SuppressionReason =
      local.ForCreateDisbursementStatusHistory.SuppressionReason;
    useImport.New1.Assign(local.ForCreateDisbursementTransaction);

    Call(FnCreateDisbursementNew.Execute, useImport, useExport);

    import.PerObligeeCsePersonAccount.Assign(useImport.PerObligee);
    import.PerDisbursementTransaction.Assign(useImport.PerCredit);
    import.Per1AfCcs.Assign(useImport.Per1AfCcs);
    import.Per2AfAcs.Assign(useImport.Per2AfAcs);
    import.Per4NaCcs.Assign(useImport.Per4NaCcs);
    import.Per5NaAcs.Assign(useImport.Per5NaAcs);
    import.Per73CrFee.Assign(useImport.Per73CrFee);
    import.Per2Processed.Assign(useImport.Per2Processed);
    import.PerDisbursementTranRlnRsn.Assign(useImport.Per1);
    local.ForAssociation.SystemGeneratedIdentifier =
      useExport.New1.SystemGeneratedIdentifier;
  }

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.ForAssociation.Populated);

    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;

    entities.ForAssociation.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetString(command, "cpaType", entities.ForAssociation.CpaType);
        db.SetString(command, "cspNumber", entities.ForAssociation.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.ForAssociation.SystemGeneratedIdentifier);
      });

    entities.ForAssociation.PrqGeneratedId = prqGeneratedId;
    entities.ForAssociation.Populated = true;
  }

  private void CreateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.
      Assert(import.PerObligeeCsePersonAccount.Populated);

    var cpaType = import.PerObligeeCsePersonAccount.Type1;
    var cspNumber = import.PerObligeeCsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = local.LastId.SystemGeneratedIdentifier + 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = import.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = "D";

    CheckValid<DisbSuppressionStatusHistory>("CpaType", cpaType);
    CheckValid<DisbSuppressionStatusHistory>("Type1", type1);
    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("CreateDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "dssGeneratedId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "personDisbFiller", "");
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.DisbSuppressionStatusHistory.CpaType = cpaType;
    entities.DisbSuppressionStatusHistory.CspNumber = cspNumber;
    entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbSuppressionStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.CreatedBy = createdBy;
    entities.DisbSuppressionStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbSuppressionStatusHistory.Type1 = type1;
    entities.DisbSuppressionStatusHistory.ReasonText = "";
    entities.DisbSuppressionStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = import.Per23Denied.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      entities.PaymentStatusHistory.SystemGeneratedIdentifier + 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = import.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.New1.Populated = false;
    Update("CreatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.New1.PstGeneratedId = pstGeneratedId;
    entities.New1.PrqGeneratedId = prqGeneratedId;
    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.ReasonText = "";
    entities.New1.Populated = true;
  }

  private bool ReadDisbSuppressionStatusHistory1()
  {
    System.Diagnostics.Debug.
      Assert(import.PerObligeeCsePersonAccount.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory1",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", import.PerObligeeCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", import.PerObligeeCsePersonAccount.CspNumber);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.Populated = true;
      });
  }

  private bool ReadDisbSuppressionStatusHistory2()
  {
    System.Diagnostics.Debug.
      Assert(import.PerObligeeCsePersonAccount.Populated);
    entities.ReadForId.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory2",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", import.PerObligeeCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", import.PerObligeeCsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.ReadForId.CpaType = db.GetString(reader, 0);
        entities.ReadForId.CspNumber = db.GetString(reader, 1);
        entities.ReadForId.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ReadForId.Populated = true;
      });
  }

  private bool ReadDisbursement()
  {
    local.AmtToApplyToPotRcv.Populated = false;

    return Read("ReadDisbursement",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "referenceNumber",
          import.PerDisbursementTransaction.ReferenceNumber ?? "");
      },
      (db, reader) =>
      {
        local.AmtToApplyToPotRcv.Amount = db.GetDecimal(reader, 0);
        local.AmtToApplyToPotRcv.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction()
  {
    System.Diagnostics.Debug.
      Assert(import.PerObligeeCsePersonAccount.Populated);
    entities.ForAssociation.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", import.PerObligeeCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", import.PerObligeeCsePersonAccount.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          local.ForAssociation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ForAssociation.CpaType = db.GetString(reader, 0);
        entities.ForAssociation.CspNumber = db.GetString(reader, 1);
        entities.ForAssociation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ForAssociation.Type1 = db.GetString(reader, 3);
        entities.ForAssociation.Amount = db.GetDecimal(reader, 4);
        entities.ForAssociation.PrqGeneratedId = db.GetNullableInt32(reader, 5);
        entities.ForAssociation.Populated = true;
      });
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
        entities.DisbursementType.ProgramCode = db.GetString(reader, 1);
        entities.DisbursementType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequestPaymentStatusHistory()
  {
    entities.PaymentRequest.Populated = false;
    entities.PaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentRequestPaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNumber", import.PerObligeeCsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", import.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 1);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 3);
        entities.PaymentRequest.RecoveryFiller = db.GetString(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 7);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.PaymentRequest.Populated = true;
        entities.PaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private void UpdatePaymentRequest()
  {
    var amount =
      entities.PaymentRequest.Amount - local.AmtToApplyToPotRcv.Amount;
    var recoveryFiller = "D";

    entities.PaymentRequest.Populated = false;
    Update("UpdatePaymentRequest",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "recoveryFiller", recoveryFiller);
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    entities.PaymentRequest.Amount = amount;
    entities.PaymentRequest.RecoveryFiller = recoveryFiller;
    entities.PaymentRequest.Populated = true;
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = import.ProgramProcessingInfo.ProcessDate;

    entities.PaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.Populated = true;
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
    /// A value of ExpAmtRemainingToDisburs.
    /// </summary>
    [JsonPropertyName("expAmtRemainingToDisburs")]
    public DisbursementTransaction ExpAmtRemainingToDisburs
    {
      get => expAmtRemainingToDisburs ??= new();
      set => expAmtRemainingToDisburs = value;
    }

    /// <summary>
    /// A value of PerObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("perObligeeCsePerson")]
    public CsePerson PerObligeeCsePerson
    {
      get => perObligeeCsePerson ??= new();
      set => perObligeeCsePerson = value;
    }

    /// <summary>
    /// A value of PerObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("perObligeeCsePersonAccount")]
    public CsePersonAccount PerObligeeCsePersonAccount
    {
      get => perObligeeCsePersonAccount ??= new();
      set => perObligeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of PerDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("perDisbursementTransaction")]
    public DisbursementTransaction PerDisbursementTransaction
    {
      get => perDisbursementTransaction ??= new();
      set => perDisbursementTransaction = value;
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
    /// A value of Per2Processed.
    /// </summary>
    [JsonPropertyName("per2Processed")]
    public DisbursementStatus Per2Processed
    {
      get => per2Processed ??= new();
      set => per2Processed = value;
    }

    /// <summary>
    /// A value of PerDisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("perDisbursementTranRlnRsn")]
    public DisbursementTranRlnRsn PerDisbursementTranRlnRsn
    {
      get => perDisbursementTranRlnRsn ??= new();
      set => perDisbursementTranRlnRsn = value;
    }

    /// <summary>
    /// A value of Per23Denied.
    /// </summary>
    [JsonPropertyName("per23Denied")]
    public PaymentStatus Per23Denied
    {
      get => per23Denied ??= new();
      set => per23Denied = value;
    }

    /// <summary>
    /// A value of ExpNbrOfDisbCreated.
    /// </summary>
    [JsonPropertyName("expNbrOfDisbCreated")]
    public Common ExpNbrOfDisbCreated
    {
      get => expNbrOfDisbCreated ??= new();
      set => expNbrOfDisbCreated = value;
    }

    /// <summary>
    /// A value of ExpAmtOfDisbCreated.
    /// </summary>
    [JsonPropertyName("expAmtOfDisbCreated")]
    public Common ExpAmtOfDisbCreated
    {
      get => expAmtOfDisbCreated ??= new();
      set => expAmtOfDisbCreated = value;
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
    private DisbursementTransaction expAmtRemainingToDisburs;
    private CsePerson perObligeeCsePerson;
    private CsePersonAccount perObligeeCsePersonAccount;
    private DisbursementTransaction perDisbursementTransaction;
    private CashReceiptType cashReceiptType;
    private DisbursementType disbursementType;
    private DisbursementType per1AfCcs;
    private DisbursementType per2AfAcs;
    private DisbursementType per4NaCcs;
    private DisbursementType per5NaAcs;
    private DisbursementType per73CrFee;
    private DisbursementStatus per2Processed;
    private DisbursementTranRlnRsn perDisbursementTranRlnRsn;
    private PaymentStatus per23Denied;
    private Common expNbrOfDisbCreated;
    private Common expAmtOfDisbCreated;
    private ProgramProcessingInfo programProcessingInfo;
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
    /// A value of ImpExpToUpdate.
    /// </summary>
    [JsonPropertyName("impExpToUpdate")]
    public MonthlyObligeeSummary ImpExpToUpdate
    {
      get => impExpToUpdate ??= new();
      set => impExpToUpdate = value;
    }

    private MonthlyObligeeSummary impExpToUpdate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForCreateDisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("forCreateDisbursementStatusHistory")]
    public DisbursementStatusHistory ForCreateDisbursementStatusHistory
    {
      get => forCreateDisbursementStatusHistory ??= new();
      set => forCreateDisbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of ForAssociation.
    /// </summary>
    [JsonPropertyName("forAssociation")]
    public DisbursementTransaction ForAssociation
    {
      get => forAssociation ??= new();
      set => forAssociation = value;
    }

    /// <summary>
    /// A value of ForCreateDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("forCreateDisbursementTransaction")]
    public DisbursementTransaction ForCreateDisbursementTransaction
    {
      get => forCreateDisbursementTransaction ??= new();
      set => forCreateDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of AmtToApplyToPotRcv.
    /// </summary>
    [JsonPropertyName("amtToApplyToPotRcv")]
    public DisbursementTransaction AmtToApplyToPotRcv
    {
      get => amtToApplyToPotRcv ??= new();
      set => amtToApplyToPotRcv = value;
    }

    /// <summary>
    /// A value of UnformattedAmt.
    /// </summary>
    [JsonPropertyName("unformattedAmt")]
    public NumericWorkSet UnformattedAmt
    {
      get => unformattedAmt ??= new();
      set => unformattedAmt = value;
    }

    /// <summary>
    /// A value of FormattedAmt.
    /// </summary>
    [JsonPropertyName("formattedAmt")]
    public WorkArea FormattedAmt
    {
      get => formattedAmt ??= new();
      set => formattedAmt = value;
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

    /// <summary>
    /// A value of LastId.
    /// </summary>
    [JsonPropertyName("lastId")]
    public DisbSuppressionStatusHistory LastId
    {
      get => lastId ??= new();
      set => lastId = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    private DisbursementStatusHistory forCreateDisbursementStatusHistory;
    private DisbursementTransaction forAssociation;
    private DisbursementTransaction forCreateDisbursementTransaction;
    private DisbursementTransaction amtToApplyToPotRcv;
    private NumericWorkSet unformattedAmt;
    private WorkArea formattedAmt;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DisbSuppressionStatusHistory lastId;
    private Common error;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public PaymentStatusHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of ForTotalArRefNbr.
    /// </summary>
    [JsonPropertyName("forTotalArRefNbr")]
    public DisbursementTransaction ForTotalArRefNbr
    {
      get => forTotalArRefNbr ??= new();
      set => forTotalArRefNbr = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
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
    /// A value of ReadForId.
    /// </summary>
    [JsonPropertyName("readForId")]
    public DisbSuppressionStatusHistory ReadForId
    {
      get => readForId ??= new();
      set => readForId = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of ForAssociation.
    /// </summary>
    [JsonPropertyName("forAssociation")]
    public DisbursementTransaction ForAssociation
    {
      get => forAssociation ??= new();
      set => forAssociation = value;
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

    private PaymentStatusHistory new1;
    private PaymentRequest paymentRequest;
    private DisbursementTransaction forTotalArRefNbr;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private DisbSuppressionStatusHistory readForId;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DisbursementTransaction forAssociation;
    private DisbursementType disbursementType;
  }
#endregion
}
