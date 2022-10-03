// Program: FN_B652_GEN_PASSTHRU_F_OBLIGEE, ID: 372708320, model: 746.
// Short name: SWE02153
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
/// A program: FN_B652_GEN_PASSTHRU_F_OBLIGEE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block creates the passthru warrant/ potential recovery depending
/// on whether the net additional passthru amount payable is positive or
/// negative.
/// </para>
/// </summary>
[Serializable]
public partial class FnB652GenPassthruFObligee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B652_GEN_PASSTHRU_F_OBLIGEE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB652GenPassthruFObligee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB652GenPassthruFObligee.
  /// </summary>
  public FnB652GenPassthruFObligee(IContext context, Import import,
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
    // Date	By	IDCR#	Description
    // 111797	govind		Initial code
    // 050699  N.Engoor        No payment request to be created for the obligee 
    // if the passthru debit records created for the obligee are suppressed.
    // 051999  N.Engoor        Raise an Alert if the amount exceeds $480 for the
    // Obligee. The alert would be raised on all the cases on which the Obligee
    // is the AR.
    // ---------------------------------------------
    // ---------------------------------------------
    // This action block creates:
    //    - one Warrant for all the months in which the net was positive.
    //    - one potential recovery for all the months in which the net was 
    // negative.
    // The business policy is : we cannot offset one month's passthru against 
    // another month's passthru recovery.
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    MoveElectronicFundTransmission(import.ElectronicFundTransmission,
      export.ElectronicFundTransmission);
    export.EftCreated.Flag = import.EftCreated.Flag;
    local.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Zero.Date))
    {
      local.ProgramProcessingInfo.ProcessDate = local.Current.Date;
    }

    // ---------------------
    // Removed the hardcoded info CAB.
    // ---------------------
    local.HardcodedPassthru.SystemGeneratedIdentifier = 71;
    local.HardcodedSuppressed.SystemGeneratedIdentifier = 3;
    local.HardcodedDebit.Type1 = "D";
    local.PassthruWarrantOrEft.Type1 = "WAR";
    local.PassthruPotRecovery.Type1 = "RCV";

    if (!ReadCsePersonObligee())
    {
      ExitState = "FN0000_OBLIGEE_NF";

      return;
    }

    if (!ReadDisbursementStatus())
    {
      ExitState = "FN0000_DISB_STATUS_NF";

      return;
    }

    // ---------------------
    // The passthru debit disbursement transaction Collection date contains the 
    // passthru date. The attribute 'PASSTHRU DATE' is valid only for the
    // subtype DISBURSEMENT TRANSACTION PASSTHRU (the credit record). But we
    // need to group the passthru disbursement debits by that passthru date.
    // ---------------------
    local.FirstTime.Flag = "Y";

    foreach(var item in ReadDisbursementTransaction1())
    {
      export.UnsuppressedFlag.Flag = "Y";
      local.Passthru.Date = entities.PassthruDebit.CollectionDate;
      UseCabGetYearMonthFromDate();

      if (local.Passthru.YearMonth == local.LastPassthru.YearMonth)
      {
        // --- It is for the same passthru month. Accumulate the passthru 
        // disbursement transaction amount
        local.NetPassthru.Amount += entities.PassthruDebit.Amount;

        // --- Read the next passthru disbursement Debit
        continue;
      }
      else
      {
        // --- A new month has been read for the obligee
        if (AsChar(local.FirstTime.Flag) == 'Y')
        {
          local.FirstTime.Flag = "N";
        }
        else
        {
          // --- It is not the first time. So finish off the processing for the 
          // last obligee-month
          if (local.NetPassthru.Amount > 0)
          {
            // -----------------------
            // Associate the disbursements for that month to the Warrant Payment
            // Request. If the payment request has not been created, create one
            // now.
            // -----------------------
            UseFnB652AssocPtDisbDebits1();

            if (local.EftPersonPreferredPaymentMethod.AbaRoutingNumber.
              GetValueOrDefault() != 0)
            {
              local.Save.Assign(local.EftPersonPreferredPaymentMethod);
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            // --- associate the disbursements for that month to the Potential 
            // Recovery Payment Request. If the payment request has not been
            // created, create one now.
            UseFnB652AssocPtDisbDebits2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        // --- Now process the new obligee-month
        local.NetPassthru.Assign(local.Initialised);
        local.NetPassthru.Amount = entities.PassthruDebit.Amount;
        local.LastPassthru.YearMonth = local.Passthru.YearMonth;
      }
    }

    if (AsChar(local.FirstTime.Flag) == 'N')
    {
      if (local.NetPassthru.Amount > 0)
      {
        UseFnB652AssocPtDisbDebits1();

        if (local.EftPersonPreferredPaymentMethod.AbaRoutingNumber.
          GetValueOrDefault() != 0)
        {
          local.Save.Assign(local.EftPersonPreferredPaymentMethod);
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      else
      {
        UseFnB652AssocPtDisbDebits2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }

    if (local.PassthruPotRecovery.SystemGeneratedIdentifier != 0)
    {
      export.NoOfPotRecsCreated.Count = 1;

      // --------------------------------
      // N.Engoor 02/05/99.
      // Change the Payment Request amount to positive at the end.
      // --------------------------------
      if (ReadPaymentRequest1())
      {
        if (entities.WarrEftOrPotRecovery.Amount < 0)
        {
          try
          {
            UpdatePaymentRequest1();
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
        }
      }
      else
      {
        ExitState = "FN0000_PAYMENT_REQUEST_NF";

        return;
      }
    }

    if (local.PassthruWarrantOrEft.SystemGeneratedIdentifier != 0)
    {
      if (Equal(local.PassthruWarrantOrEft.Type1, "WAR"))
      {
        export.NoOfWarrantsCreated.Count = 1;
      }

      // ---------------------------------------------
      // Recapture any recovery obligation if possible.
      // ---------------------------------------------
      UseFnRcapProcessRecapture();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.Recap.Amount > 0)
      {
        export.NoOfRcapForObligee.Count = 1;
      }

      if (ReadPaymentRequest2())
      {
        local.ForEftDesig.Number =
          entities.WarrEftOrPotRecovery.DesignatedPayeeCsePersonNo ?? Spaces
          (10);

        if (Equal(entities.WarrEftOrPotRecovery.Type1, "RCV"))
        {
          local.PaymentStatus.SystemGeneratedIdentifier = 27;
        }
        else
        {
          local.PaymentStatus.SystemGeneratedIdentifier = 1;
        }
      }
      else
      {
        ExitState = "FN0000_PAYMENT_REQUEST_NF";

        return;
      }

      if (!ReadPaymentStatus())
      {
        ExitState = "PAYMENT_STATUS_NF";

        return;
      }

      if (local.Recap.Amount >= local.PassthruWarrantOrEft.Amount)
      {
        foreach(var item in ReadPaymentStatusHistory())
        {
          DeletePaymentStatusHistory();
        }

        DeletePaymentRequest();
      }
      else
      {
        try
        {
          UpdatePaymentRequest2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (entities.WarrEftOrPotRecovery.Amount > 480)
        {
          // --------------------------------
          // Raise an Alert if the payment request created for the Obligee 
          // exceeds $480. The alert is to be raised for all the cases in which
          // the AR is the Obligee.
          // --------------------------------
          foreach(var item in ReadCase())
          {
            local.RaiseAlert.CaseNumber = entities.Case1.Number;
            local.RaiseAlert.BusinessObjectCd = "CAS";
            local.RaiseAlert.CsePersonNumber = import.Obligee.Number;
            local.RaiseAlert.ReasonCode = "EXPASSTH";
            local.RaiseAlert.EventId = 5;
            local.RaiseAlert.ProcessStatus = "Q";
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          // --------------------------------
          // Since the payment request amount is greater than $ 480 suppress all
          // the unsuppressed passthrus for that person.
          // --------------------------------
          foreach(var item in ReadDisbursementTransaction2())
          {
            if (ReadDisbursementStatusHistory())
            {
              local.Last.SystemGeneratedIdentifier =
                entities.DisbursementStatusHistory.SystemGeneratedIdentifier + 1
                ;

              try
              {
                UpdateDisbursementStatusHistory();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    break;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            try
            {
              CreateDisbursementStatusHistory();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  break;
                case ErrorCode.PermittedValueViolation:
                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            // --------------------------------
            // Before deleting the payment request disassociate the disbursement
            // debits associated to that payment request.
            // --------------------------------
            DisassociateDisbursementTransaction();
          }

          foreach(var item in ReadPaymentStatusHistory())
          {
            DeletePaymentStatusHistory();
          }

          DeletePaymentRequest();
        }
        else if (Equal(local.PassthruWarrantOrEft.Type1, "EFT"))
        {
          // ----------------------
          // In case of Passthrus since they are AF cases no cost recovery fees 
          // would be deducted. If the Payment request created for the Obligee
          // is an EFT create an EFT transmission record for the same.
          // ----------------------
          if (AsChar(export.EftCreated.Flag) == 'Y')
          {
            MoveElectronicFundTransmission(export.ElectronicFundTransmission,
              local.EftElectronicFundTransmission);
          }
          else if (ReadElectronicFundTransmission())
          {
            local.EftElectronicFundTransmission.Assign(
              entities.ElectronicFundTransmission);
            ++local.EftElectronicFundTransmission.TransmissionIdentifier;
            local.EftElectronicFundTransmission.TraceNumber =
              local.EftElectronicFundTransmission.TraceNumber.
                GetValueOrDefault() + 1;
          }

          export.NoOfEftsCreated.Count = 1;
          local.BackoffInd.Flag = "N";
          UseFnCreateEftTransmissionRec();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            MoveElectronicFundTransmission(local.EftElectronicFundTransmission,
              export.ElectronicFundTransmission);
            export.EftCreated.Flag = "Y";

            // ------------------------
            // Update the control table with the most recent value of the EFT 
            // transmission id and trace number.
            // ------------------------
            if (ReadControlTable1())
            {
              try
              {
                UpdateControlTable1();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CONTROL_TABLE_VALUE_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
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
              ExitState = "CONTROL_TABLE_ID_NF";
            }

            if (ReadControlTable2())
            {
              try
              {
                UpdateControlTable2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CONTROL_TABLE_VALUE_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
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
              ExitState = "CONTROL_TABLE_ID_NF";
            }
          }
        }
      }
    }
  }

  private static void MoveElectronicFundTransmission(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.TransmissionIdentifier = source.TransmissionIdentifier;
    target.TraceNumber = source.TraceNumber;
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private void UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Passthru.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    local.Passthru.YearMonth = useExport.DateWorkArea.YearMonth;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnB652AssocPtDisbDebits1()
  {
    var useImport = new FnB652AssocPtDisbDebits.Import();
    var useExport = new FnB652AssocPtDisbDebits.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.WarrOrPotRec.Assign(local.PassthruWarrantOrEft);
    useImport.Obligee.Number = entities.Obligee2.Number;
    useImport.PassthruMonth.YearMonth = local.LastPassthru.YearMonth;
    MoveElectronicFundTransmission(export.ElectronicFundTransmission,
      useImport.ElectronicFundTransmission);
    useImport.EftCreated.Flag = export.EftCreated.Flag;

    Call(FnB652AssocPtDisbDebits.Execute, useImport, useExport);

    MovePaymentRequest(useExport.EftWarrOrPotRec, local.PassthruWarrantOrEft);
    MoveElectronicFundTransmission(useExport.ElectronicFundTransmission,
      export.ElectronicFundTransmission);
    export.EftCreated.Flag = useExport.EftCreated.Flag;
    local.EftPersonPreferredPaymentMethod.Assign(useExport.Eft);
  }

  private void UseFnB652AssocPtDisbDebits2()
  {
    var useImport = new FnB652AssocPtDisbDebits.Import();
    var useExport = new FnB652AssocPtDisbDebits.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.WarrOrPotRec.Assign(local.PassthruPotRecovery);
    useImport.Obligee.Number = entities.Obligee2.Number;
    useImport.PassthruMonth.YearMonth = local.LastPassthru.YearMonth;

    Call(FnB652AssocPtDisbDebits.Execute, useImport, useExport);

    MovePaymentRequest(useExport.EftWarrOrPotRec, local.PassthruPotRecovery);
  }

  private void UseFnCreateEftTransmissionRec()
  {
    var useImport = new FnCreateEftTransmissionRec.Import();
    var useExport = new FnCreateEftTransmissionRec.Export();

    useImport.Persistent.Assign(entities.WarrEftOrPotRecovery);
    useImport.Obligee.Number = entities.Obligee2.Number;
    useImport.DesignatedPayee.Number = local.ForEftDesig.Number;
    useImport.BackoffInd.Flag = local.BackoffInd.Flag;
    MoveElectronicFundTransmission(local.EftElectronicFundTransmission,
      useImport.ExportNext);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.PersonPreferredPaymentMethod.Assign(local.Save);

    Call(FnCreateEftTransmissionRec.Execute, useImport, useExport);

    entities.WarrEftOrPotRecovery.Assign(useImport.Persistent);
    MoveElectronicFundTransmission(useImport.ExportNext,
      local.EftElectronicFundTransmission);
  }

  private void UseFnRcapProcessRecapture()
  {
    var useImport = new FnRcapProcessRecapture.Import();
    var useExport = new FnRcapProcessRecapture.Export();

    useImport.PaymentRequest.SystemGeneratedIdentifier =
      local.PassthruWarrantOrEft.SystemGeneratedIdentifier;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Obligee.Number = entities.Obligee2.Number;

    Call(FnRcapProcessRecapture.Execute, useImport, useExport);

    local.Recap.Amount = useExport.Recap.Amount;
    export.NoOfRcapForObligee.Count = useExport.NbrOfRcpPmtsCreated.Count;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.RaiseAlert);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private void CreateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PassthruDebit.Populated);

    var dbsGeneratedId = entities.DisbursementStatus.SystemGeneratedIdentifier;
    var dtrGeneratedId = entities.PassthruDebit.SystemGeneratedIdentifier;
    var cspNumber = entities.PassthruDebit.CspNumber;
    var cpaType = entities.PassthruDebit.CpaType;
    var systemGeneratedIdentifier = local.Last.SystemGeneratedIdentifier;
    var effectiveDate = local.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.New1.Populated = false;
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
        db.SetNullableString(command, "reasonText", "");
        db.SetNullableString(command, "suppressionReason", "");
      });

    entities.New1.DbsGeneratedId = dbsGeneratedId;
    entities.New1.DtrGeneratedId = dtrGeneratedId;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.Populated = true;
  }

  private void DeletePaymentRequest()
  {
    bool exists;

    Update("DeletePaymentRequest#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ELEC_FUND_TRAN\".",
        "50001");
    }

    Update("DeletePaymentRequest#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier);
      });

    Update("DeletePaymentRequest#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier);
      });

    exists = Read("DeletePaymentRequest#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_POT_RECOVERY\".",
        "50001");
    }

    Update("DeletePaymentRequest#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier);
      });
  }

  private void DeletePaymentStatusHistory()
  {
    Update("DeletePaymentStatusHistory",
      (db, command) =>
      {
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
  }

  private void DisassociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.PassthruDebit.Populated);
    entities.PassthruDebit.Populated = false;
    Update("DisassociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.PassthruDebit.CpaType);
        db.SetString(command, "cspNumber", entities.PassthruDebit.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.PassthruDebit.SystemGeneratedIdentifier);
      });

    entities.PassthruDebit.PrqGeneratedId = null;
    entities.PassthruDebit.Populated = true;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAr", entities.Obligee2.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadControlTable1()
  {
    entities.OutboundEftNumber.Populated = false;

    return Read("ReadControlTable1",
      null,
      (db, reader) =>
      {
        entities.OutboundEftNumber.Identifier = db.GetString(reader, 0);
        entities.OutboundEftNumber.LastUsedNumber = db.GetInt32(reader, 1);
        entities.OutboundEftNumber.Populated = true;
      });
  }

  private bool ReadControlTable2()
  {
    entities.OutboundEftTraceNumber.Populated = false;

    return Read("ReadControlTable2",
      null,
      (db, reader) =>
      {
        entities.OutboundEftTraceNumber.Identifier = db.GetString(reader, 0);
        entities.OutboundEftTraceNumber.LastUsedNumber = db.GetInt32(reader, 1);
        entities.OutboundEftTraceNumber.Populated = true;
      });
  }

  private bool ReadCsePersonObligee()
  {
    entities.Obligee1.Populated = false;
    entities.Obligee2.Populated = false;

    return Read("ReadCsePersonObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee2.Number = db.GetString(reader, 0);
        entities.Obligee1.CspNumber = db.GetString(reader, 0);
        entities.Obligee1.Type1 = db.GetString(reader, 1);
        entities.Obligee1.Populated = true;
        entities.Obligee2.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee1.Type1);
      });
  }

  private bool ReadDisbursementStatus()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId", import.Suppressed.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Populated = true;
      });
  }

  private bool ReadDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PassthruDebit.Populated);
    entities.DisbursementStatusHistory.Populated = false;

    return Read("ReadDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.PassthruDebit.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.PassthruDebit.CspNumber);
        db.SetString(command, "cpaType", entities.PassthruDebit.CpaType);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
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
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.PassthruDebit.Populated = false;

    return ReadEach("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
        db.SetNullableDate(
          command, "processDate", local.Zero.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "dbtGeneratedId",
          local.HardcodedPassthru.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbsGeneratedId",
          local.HardcodedSuppressed.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PassthruDebit.CpaType = db.GetString(reader, 0);
        entities.PassthruDebit.CspNumber = db.GetString(reader, 1);
        entities.PassthruDebit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PassthruDebit.Type1 = db.GetString(reader, 3);
        entities.PassthruDebit.Amount = db.GetDecimal(reader, 4);
        entities.PassthruDebit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.PassthruDebit.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PassthruDebit.LastUpdateTmst =
          db.GetNullableDateTime(reader, 7);
        entities.PassthruDebit.DisbursementDate = db.GetNullableDate(reader, 8);
        entities.PassthruDebit.CashNonCashInd = db.GetString(reader, 9);
        entities.PassthruDebit.RecapturedInd = db.GetString(reader, 10);
        entities.PassthruDebit.CollectionDate = db.GetNullableDate(reader, 11);
        entities.PassthruDebit.DbtGeneratedId = db.GetNullableInt32(reader, 12);
        entities.PassthruDebit.PrqGeneratedId = db.GetNullableInt32(reader, 13);
        entities.PassthruDebit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.PassthruDebit.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.PassthruDebit.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.PassthruDebit.CashNonCashInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.PassthruDebit.Populated = false;

    return ReadEach("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
        db.SetNullableDate(
          command, "processDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "dbtGeneratedId",
          local.HardcodedPassthru.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PassthruDebit.CpaType = db.GetString(reader, 0);
        entities.PassthruDebit.CspNumber = db.GetString(reader, 1);
        entities.PassthruDebit.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PassthruDebit.Type1 = db.GetString(reader, 3);
        entities.PassthruDebit.Amount = db.GetDecimal(reader, 4);
        entities.PassthruDebit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.PassthruDebit.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.PassthruDebit.LastUpdateTmst =
          db.GetNullableDateTime(reader, 7);
        entities.PassthruDebit.DisbursementDate = db.GetNullableDate(reader, 8);
        entities.PassthruDebit.CashNonCashInd = db.GetString(reader, 9);
        entities.PassthruDebit.RecapturedInd = db.GetString(reader, 10);
        entities.PassthruDebit.CollectionDate = db.GetNullableDate(reader, 11);
        entities.PassthruDebit.DbtGeneratedId = db.GetNullableInt32(reader, 12);
        entities.PassthruDebit.PrqGeneratedId = db.GetNullableInt32(reader, 13);
        entities.PassthruDebit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.PassthruDebit.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.PassthruDebit.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.PassthruDebit.CashNonCashInd);

        return true;
      });
  }

  private bool ReadElectronicFundTransmission()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return Read("ReadElectronicFundTransmission",
      null,
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.TransmittalAmount =
          db.GetDecimal(reader, 0);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 1);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 2);
        entities.ElectronicFundTransmission.TraceNumber =
          db.GetNullableInt64(reader, 3);
        entities.ElectronicFundTransmission.ReceivingDfiAccountNumber =
          db.GetNullableString(reader, 4);
        entities.ElectronicFundTransmission.Populated = true;
      });
  }

  private bool ReadPaymentRequest1()
  {
    entities.WarrEftOrPotRecovery.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          local.PassthruPotRecovery.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.WarrEftOrPotRecovery.ProcessDate = db.GetDate(reader, 1);
        entities.WarrEftOrPotRecovery.Amount = db.GetDecimal(reader, 2);
        entities.WarrEftOrPotRecovery.CreatedBy = db.GetString(reader, 3);
        entities.WarrEftOrPotRecovery.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.WarrEftOrPotRecovery.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.WarrEftOrPotRecovery.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.WarrEftOrPotRecovery.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.WarrEftOrPotRecovery.Classification = db.GetString(reader, 8);
        entities.WarrEftOrPotRecovery.Type1 = db.GetString(reader, 9);
        entities.WarrEftOrPotRecovery.PrqRGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.WarrEftOrPotRecovery.InterstateInd =
          db.GetNullableString(reader, 11);
        entities.WarrEftOrPotRecovery.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.WarrEftOrPotRecovery.Type1);
          
      });
  }

  private bool ReadPaymentRequest2()
  {
    entities.WarrEftOrPotRecovery.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          local.PassthruWarrantOrEft.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.WarrEftOrPotRecovery.ProcessDate = db.GetDate(reader, 1);
        entities.WarrEftOrPotRecovery.Amount = db.GetDecimal(reader, 2);
        entities.WarrEftOrPotRecovery.CreatedBy = db.GetString(reader, 3);
        entities.WarrEftOrPotRecovery.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.WarrEftOrPotRecovery.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.WarrEftOrPotRecovery.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.WarrEftOrPotRecovery.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.WarrEftOrPotRecovery.Classification = db.GetString(reader, 8);
        entities.WarrEftOrPotRecovery.Type1 = db.GetString(reader, 9);
        entities.WarrEftOrPotRecovery.PrqRGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.WarrEftOrPotRecovery.InterstateInd =
          db.GetNullableString(reader, 11);
        entities.WarrEftOrPotRecovery.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.WarrEftOrPotRecovery.Type1);
          
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          local.PaymentStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return ReadEach("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.PaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private void UpdateControlTable1()
  {
    var lastUsedNumber =
      local.EftElectronicFundTransmission.TransmissionIdentifier - 1;

    entities.OutboundEftNumber.Populated = false;
    Update("UpdateControlTable1",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.
          SetString(command, "cntlTblId", entities.OutboundEftNumber.Identifier);
          
      });

    entities.OutboundEftNumber.LastUsedNumber = lastUsedNumber;
    entities.OutboundEftNumber.Populated = true;
  }

  private void UpdateControlTable2()
  {
    var lastUsedNumber =
      (int)local.EftElectronicFundTransmission.TraceNumber.GetValueOrDefault() -
      1;

    entities.OutboundEftTraceNumber.Populated = false;
    Update("UpdateControlTable2",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(
          command, "cntlTblId", entities.OutboundEftTraceNumber.Identifier);
      });

    entities.OutboundEftTraceNumber.LastUsedNumber = lastUsedNumber;
    entities.OutboundEftTraceNumber.Populated = true;
  }

  private void UpdateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.DisbursementStatusHistory.Populated);

    var discontinueDate = local.ProgramProcessingInfo.ProcessDate;

    entities.DisbursementStatusHistory.Populated = false;
    Update("UpdateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "dbsGeneratedId",
          entities.DisbursementStatusHistory.DbsGeneratedId);
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementStatusHistory.DtrGeneratedId);
        db.SetString(
          command, "cspNumber", entities.DisbursementStatusHistory.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementStatusHistory.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void UpdatePaymentRequest1()
  {
    var amount = -entities.WarrEftOrPotRecovery.Amount;

    entities.WarrEftOrPotRecovery.Populated = false;
    Update("UpdatePaymentRequest1",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetInt32(
          command, "paymentRequestId",
          entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier);
      });

    entities.WarrEftOrPotRecovery.Amount = amount;
    entities.WarrEftOrPotRecovery.Populated = true;
  }

  private void UpdatePaymentRequest2()
  {
    var amount = entities.WarrEftOrPotRecovery.Amount - local.Recap.Amount;

    entities.WarrEftOrPotRecovery.Populated = false;
    Update("UpdatePaymentRequest2",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetInt32(
          command, "paymentRequestId",
          entities.WarrEftOrPotRecovery.SystemGeneratedIdentifier);
      });

    entities.WarrEftOrPotRecovery.Amount = amount;
    entities.WarrEftOrPotRecovery.Populated = true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CsePerson Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of EftCreated.
    /// </summary>
    [JsonPropertyName("eftCreated")]
    public Common EftCreated
    {
      get => eftCreated ??= new();
      set => eftCreated = value;
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
    /// A value of Suppressed.
    /// </summary>
    [JsonPropertyName("suppressed")]
    public DisbursementStatus Suppressed
    {
      get => suppressed ??= new();
      set => suppressed = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private CsePerson persistent;
    private ElectronicFundTransmission electronicFundTransmission;
    private Common eftCreated;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus suppressed;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson obligee;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of EftCreated.
    /// </summary>
    [JsonPropertyName("eftCreated")]
    public Common EftCreated
    {
      get => eftCreated ??= new();
      set => eftCreated = value;
    }

    /// <summary>
    /// A value of NoOfEftsCreated.
    /// </summary>
    [JsonPropertyName("noOfEftsCreated")]
    public Common NoOfEftsCreated
    {
      get => noOfEftsCreated ??= new();
      set => noOfEftsCreated = value;
    }

    /// <summary>
    /// A value of NoOfRcapPmntsCreated.
    /// </summary>
    [JsonPropertyName("noOfRcapPmntsCreated")]
    public Common NoOfRcapPmntsCreated
    {
      get => noOfRcapPmntsCreated ??= new();
      set => noOfRcapPmntsCreated = value;
    }

    /// <summary>
    /// A value of UnsuppressedFlag.
    /// </summary>
    [JsonPropertyName("unsuppressedFlag")]
    public Common UnsuppressedFlag
    {
      get => unsuppressedFlag ??= new();
      set => unsuppressedFlag = value;
    }

    /// <summary>
    /// A value of NoOfRcapForObligee.
    /// </summary>
    [JsonPropertyName("noOfRcapForObligee")]
    public Common NoOfRcapForObligee
    {
      get => noOfRcapForObligee ??= new();
      set => noOfRcapForObligee = value;
    }

    /// <summary>
    /// A value of NoOfPotRecsCreated.
    /// </summary>
    [JsonPropertyName("noOfPotRecsCreated")]
    public Common NoOfPotRecsCreated
    {
      get => noOfPotRecsCreated ??= new();
      set => noOfPotRecsCreated = value;
    }

    /// <summary>
    /// A value of NoOfWarrantsCreated.
    /// </summary>
    [JsonPropertyName("noOfWarrantsCreated")]
    public Common NoOfWarrantsCreated
    {
      get => noOfWarrantsCreated ??= new();
      set => noOfWarrantsCreated = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private Common eftCreated;
    private Common noOfEftsCreated;
    private Common noOfRcapPmntsCreated;
    private Common unsuppressedFlag;
    private Common noOfRcapForObligee;
    private Common noOfPotRecsCreated;
    private Common noOfWarrantsCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public PersonPreferredPaymentMethod Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of EftPersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("eftPersonPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod EftPersonPreferredPaymentMethod
    {
      get => eftPersonPreferredPaymentMethod ??= new();
      set => eftPersonPreferredPaymentMethod = value;
    }

    /// <summary>
    /// A value of ForEftDesig.
    /// </summary>
    [JsonPropertyName("forEftDesig")]
    public CsePerson ForEftDesig
    {
      get => forEftDesig ??= new();
      set => forEftDesig = value;
    }

    /// <summary>
    /// A value of BackoffInd.
    /// </summary>
    [JsonPropertyName("backoffInd")]
    public Common BackoffInd
    {
      get => backoffInd ??= new();
      set => backoffInd = value;
    }

    /// <summary>
    /// A value of EftElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("eftElectronicFundTransmission")]
    public ElectronicFundTransmission EftElectronicFundTransmission
    {
      get => eftElectronicFundTransmission ??= new();
      set => eftElectronicFundTransmission = value;
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
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public DisbursementStatusHistory Last
    {
      get => last ??= new();
      set => last = value;
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
    /// A value of Net.
    /// </summary>
    [JsonPropertyName("net")]
    public PaymentRequest Net
    {
      get => net ??= new();
      set => net = value;
    }

    /// <summary>
    /// A value of RaiseAlert.
    /// </summary>
    [JsonPropertyName("raiseAlert")]
    public Infrastructure RaiseAlert
    {
      get => raiseAlert ??= new();
      set => raiseAlert = value;
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

    /// <summary>
    /// A value of PassthruWarrantOrEft.
    /// </summary>
    [JsonPropertyName("passthruWarrantOrEft")]
    public PaymentRequest PassthruWarrantOrEft
    {
      get => passthruWarrantOrEft ??= new();
      set => passthruWarrantOrEft = value;
    }

    /// <summary>
    /// A value of PassthruPotRecovery.
    /// </summary>
    [JsonPropertyName("passthruPotRecovery")]
    public PaymentRequest PassthruPotRecovery
    {
      get => passthruPotRecovery ??= new();
      set => passthruPotRecovery = value;
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
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
    }

    /// <summary>
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public PaymentRequest Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
    }

    /// <summary>
    /// A value of NetPassthru.
    /// </summary>
    [JsonPropertyName("netPassthru")]
    public PaymentRequest NetPassthru
    {
      get => netPassthru ??= new();
      set => netPassthru = value;
    }

    /// <summary>
    /// A value of LastPassthru.
    /// </summary>
    [JsonPropertyName("lastPassthru")]
    public DateWorkArea LastPassthru
    {
      get => lastPassthru ??= new();
      set => lastPassthru = value;
    }

    /// <summary>
    /// A value of Passthru.
    /// </summary>
    [JsonPropertyName("passthru")]
    public DateWorkArea Passthru
    {
      get => passthru ??= new();
      set => passthru = value;
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of HardcodedPassthru.
    /// </summary>
    [JsonPropertyName("hardcodedPassthru")]
    public DisbursementType HardcodedPassthru
    {
      get => hardcodedPassthru ??= new();
      set => hardcodedPassthru = value;
    }

    /// <summary>
    /// A value of HardcodedDebit.
    /// </summary>
    [JsonPropertyName("hardcodedDebit")]
    public DisbursementTransaction HardcodedDebit
    {
      get => hardcodedDebit ??= new();
      set => hardcodedDebit = value;
    }

    /// <summary>
    /// A value of HardcodeDisbursed.
    /// </summary>
    [JsonPropertyName("hardcodeDisbursed")]
    public DisbursementTranRlnRsn HardcodeDisbursed
    {
      get => hardcodeDisbursed ??= new();
      set => hardcodeDisbursed = value;
    }

    /// <summary>
    /// A value of HardcodedSuppressed.
    /// </summary>
    [JsonPropertyName("hardcodedSuppressed")]
    public DisbursementStatus HardcodedSuppressed
    {
      get => hardcodedSuppressed ??= new();
      set => hardcodedSuppressed = value;
    }

    /// <summary>
    /// A value of HardcodedReleased.
    /// </summary>
    [JsonPropertyName("hardcodedReleased")]
    public DisbursementStatus HardcodedReleased
    {
      get => hardcodedReleased ??= new();
      set => hardcodedReleased = value;
    }

    private PersonPreferredPaymentMethod save;
    private PersonPreferredPaymentMethod eftPersonPreferredPaymentMethod;
    private CsePerson forEftDesig;
    private Common backoffInd;
    private ElectronicFundTransmission eftElectronicFundTransmission;
    private PaymentStatus paymentStatus;
    private DisbursementStatusHistory last;
    private DateWorkArea max;
    private PaymentRequest net;
    private Infrastructure raiseAlert;
    private PaymentRequest recap;
    private PaymentRequest passthruWarrantOrEft;
    private PaymentRequest passthruPotRecovery;
    private ProgramProcessingInfo programProcessingInfo;
    private Common firstTime;
    private PaymentRequest initialised;
    private PaymentRequest netPassthru;
    private DateWorkArea lastPassthru;
    private DateWorkArea passthru;
    private DateWorkArea current;
    private DateWorkArea zero;
    private DisbursementType hardcodedPassthru;
    private DisbursementTransaction hardcodedDebit;
    private DisbursementTranRlnRsn hardcodeDisbursed;
    private DisbursementStatus hardcodedSuppressed;
    private DisbursementStatus hardcodedReleased;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OutboundEftNumber.
    /// </summary>
    [JsonPropertyName("outboundEftNumber")]
    public ControlTable OutboundEftNumber
    {
      get => outboundEftNumber ??= new();
      set => outboundEftNumber = value;
    }

    /// <summary>
    /// A value of OutboundEftTraceNumber.
    /// </summary>
    [JsonPropertyName("outboundEftTraceNumber")]
    public ControlTable OutboundEftTraceNumber
    {
      get => outboundEftTraceNumber ??= new();
      set => outboundEftTraceNumber = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbursementStatusHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    /// <summary>
    /// A value of WarrEftOrPotRecovery.
    /// </summary>
    [JsonPropertyName("warrEftOrPotRecovery")]
    public PaymentRequest WarrEftOrPotRecovery
    {
      get => warrEftOrPotRecovery ??= new();
      set => warrEftOrPotRecovery = value;
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
    /// A value of PassthruDebit.
    /// </summary>
    [JsonPropertyName("passthruDebit")]
    public DisbursementTransaction PassthruDebit
    {
      get => passthruDebit ??= new();
      set => passthruDebit = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    private ControlTable outboundEftNumber;
    private ControlTable outboundEftTraceNumber;
    private ElectronicFundTransmission electronicFundTransmission;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private CaseUnit caseUnit;
    private Case1 case1;
    private DisbursementTransaction disbursement;
    private DisbursementStatusHistory new1;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus disbursementStatus;
    private PaymentRequest warrEftOrPotRecovery;
    private DisbursementType disbursementType;
    private DisbursementTransaction passthruDebit;
    private CsePersonAccount obligee1;
    private CsePerson obligee2;
  }
#endregion
}
