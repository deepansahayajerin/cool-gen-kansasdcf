// Program: FN_RCAP_PROCESS_RECAPTURE, ID: 372673980, model: 746.
// Short name: SWE02163
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
/// A program: FN_RCAP_PROCESS_RECAPTURE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block creates recaptures for a specified warrant payment request
/// </para>
/// </summary>
[Serializable]
public partial class FnRcapProcessRecapture: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RCAP_PROCESS_RECAPTURE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRcapProcessRecapture(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRcapProcessRecapture.
  /// </summary>
  public FnRcapProcessRecapture(IContext context, Import import, Export export):
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
    // Assumption: The limits specified in Obligor Rule is for the
    // 'Recapture' month and not 'Collection' Month. For e.g. If we
    // are generating a warrant in Apr 97 for collections with coll
    // dates in Jan 97, Feb 97 and Mar 97, these limits specify
    // how much we can recapture in Apr 97.
    // ---------------------------------------------
    // ---------------------------------------------
    // This CAB creates the Recapture Payment Request and the
    // associated Disbursement Transactions from the specified
    // warant payment request.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	By	Description
    // 120397	govind	Initial code
    // 121197	govind	Modified not to expect the obligee
    // 		person number (because it could be
    // 		desig payee ...)
    // 022898	govind	Modified to limit to the monthly max
    // 		amount only if obligor rule specifies a
    // 		nonzero maximum amount. If the obligor
    // 		rule max amount is zero, treat it as 'no
    // 		caps specified'.
    // 042299  Fangman   Took out unnecessary reads, deleted zero debits & 
    // payments.  Pgm does expect an import obligee.  Fixed read of obligor rule
    // to use date range.
    // 010800  Fangman   PR 84663  - Fix rounding error.
    // 021102  Fangman  WR 000235 - PSUM redesign.  Changed code to use new 
    // Monthly Obligee fields.
    // ---------------------------------------------
    export.NbrOfRcpPmtsCreated.Count = 0;

    if (!ReadObligorRule())
    {
      // --- No recapture rule specified for the obligor. So nothing to 
      // recapture.
      return;
    }

    UseFnRcapCompRecDebtsBalFObe();

    if (local.TotalRecoveryDebtsBal.TotalCurrency <= 0)
    {
      // --- Nothing to recapture
      return;
    }

    local.UpdByProcDt.Year = Year(import.ProgramProcessingInfo.ProcessDate);
    local.UpdByProcDt.Month = Month(import.ProgramProcessingInfo.ProcessDate);

    if (ReadMonthlyObligeeSummary())
    {
      local.UpdByProcDt.Assign(entities.MonthlyObligeeSummary);
    }
    else
    {
      // This is OK.  The loc view will be initialized & the monthly row will be
      // created at the end of the AB.
    }

    // ---------------------------------------------
    // Assumption: The limits specified in Obligor Rule is for the 'Recapture' 
    // month and not 'Collection' Month. For e.g. If we are generating a warrant
    // in Apr 97 for collections with coll dates in Jan 97, Feb 97 and Mar 97,
    // these limits specify how much we can recapture in Apr 97.
    // ---------------------------------------------
    foreach(var item in ReadDisbursementTransactionDisbursementType1())
    {
      if ((AsChar(entities.DisbursementType.CurrentArrearsInd) == 'C' || AsChar
        (entities.DisbursementType.CurrentArrearsInd) == 'A') && Equal
        (entities.DisbursementType.ProgramCode, "NA"))
      {
        // --- NADC Current or Arrears disbursement
      }
      else if (Equal(entities.DisbursementType.Code, "PT"))
      {
        // --- Passthru disbursement
      }
      else
      {
        // --- Skip anything other than NADC Current, NADC Arrears, Passthru 
        // disbursements
        continue;
      }

      // --- accumulate the totals by Non ADC Current, Non ADC Arrears and 
      // Passthru
      if (Equal(entities.DisbursementType.Code, "PT"))
      {
        local.TotalPassthruDisb.TotalCurrency += entities.Debit.Amount;
      }
      else if (AsChar(entities.DisbursementType.CurrentArrearsInd) == 'C')
      {
        local.TotalNadcCurrDisb.TotalCurrency += entities.Debit.Amount;
      }
      else if (AsChar(entities.DisbursementType.CurrentArrearsInd) == 'A')
      {
        local.TotalNadcArrDisb.TotalCurrency += entities.Debit.Amount;
      }
    }

    if (local.TotalNadcCurrDisb.TotalCurrency < 0)
    {
      local.TotalNadcArrDisb.TotalCurrency += local.TotalNadcCurrDisb.
        TotalCurrency;
      local.TotalNadcCurrDisb.TotalCurrency = 0;
    }
    else if (local.TotalNadcArrDisb.TotalCurrency < 0)
    {
      local.TotalNadcCurrDisb.TotalCurrency += local.TotalNadcArrDisb.
        TotalCurrency;
      local.TotalNadcArrDisb.TotalCurrency = 0;
    }

    // ---------------------------------------------
    // Now we know:
    //   - the total recovery debt balance for the obligor
    //   - the total disbursements in that warrant payment request
    // by NADC current, NADC arrears and Passthru.
    // Now get the amount to be recaptured based on the obligor
    // rule, by NADC current, NADC arrears and Passthru
    // ---------------------------------------------
    if (local.TotalNadcCurrDisb.TotalCurrency > 0)
    {
      if (Lt(0, entities.ObligorRule.NonAdcCurrentPercentage))
      {
        local.TotalNadcCurrToRecapture.TotalCurrency =
          local.TotalNadcCurrDisb.TotalCurrency * entities
          .ObligorRule.NonAdcCurrentPercentage.GetValueOrDefault() / 100;
      }
      else if (Lt(0, entities.ObligorRule.NonAdcCurrentAmount))
      {
        local.TotalNadcCurrToRecapture.TotalCurrency =
          entities.ObligorRule.NonAdcCurrentAmount.GetValueOrDefault();

        if (local.TotalNadcCurrToRecapture.TotalCurrency > local
          .TotalNadcCurrDisb.TotalCurrency)
        {
          local.TotalNadcCurrToRecapture.TotalCurrency =
            local.TotalNadcCurrDisb.TotalCurrency;
        }
      }

      if (local.TotalNadcCurrToRecapture.TotalCurrency > local
        .TotalRecoveryDebtsBal.TotalCurrency)
      {
        local.TotalNadcCurrToRecapture.TotalCurrency =
          local.TotalRecoveryDebtsBal.TotalCurrency;
      }

      // ---------------------------------------------------------------------------------------
      // If a monthly maximum is specified, limit the recapture to the remaining
      // recapturable amount for the month.
      // ---------------------------------------------------------------------------------------
      if (Lt(0, entities.ObligorRule.NonAdcCurrentMaxAmount))
      {
        if (Lt(entities.ObligorRule.NonAdcCurrentMaxAmount,
          local.UpdByProcDt.NaCurrRecapAmt.GetValueOrDefault() +
          local.TotalNadcCurrToRecapture.TotalCurrency))
        {
          local.TotalNadcCurrToRecapture.TotalCurrency =
            entities.ObligorRule.NonAdcCurrentMaxAmount.GetValueOrDefault() - local
            .UpdByProcDt.NaCurrRecapAmt.GetValueOrDefault();
        }
      }

      local.TotalRecoveryDebtsBal.TotalCurrency -= local.
        TotalNadcCurrToRecapture.TotalCurrency;
    }

    if (local.TotalRecoveryDebtsBal.TotalCurrency > 0)
    {
      if (local.TotalNadcArrDisb.TotalCurrency > 0)
      {
        if (Lt(0, entities.ObligorRule.NonAdcArrearsPercentage))
        {
          local.TotalNadcArrToRecapture.TotalCurrency =
            local.TotalNadcArrDisb.TotalCurrency * entities
            .ObligorRule.NonAdcArrearsPercentage.GetValueOrDefault() / 100;
        }
        else if (Lt(0, entities.ObligorRule.NonAdcArrearsAmount))
        {
          local.TotalNadcArrToRecapture.TotalCurrency =
            entities.ObligorRule.NonAdcArrearsAmount.GetValueOrDefault();

          if (local.TotalNadcArrToRecapture.TotalCurrency > local
            .TotalNadcArrDisb.TotalCurrency)
          {
            local.TotalNadcArrToRecapture.TotalCurrency =
              local.TotalNadcArrDisb.TotalCurrency;
          }
        }

        if (local.TotalNadcArrToRecapture.TotalCurrency > local
          .TotalRecoveryDebtsBal.TotalCurrency)
        {
          local.TotalNadcArrToRecapture.TotalCurrency =
            local.TotalRecoveryDebtsBal.TotalCurrency;
        }

        // ---------------------------------------------------------------------------------------
        // If a monthly maximum is specified, limit the recapture to the 
        // remaining recapturable amount for the month.
        // ---------------------------------------------------------------------------------------
        if (Lt(0, entities.ObligorRule.NonAdcArrearsMaxAmount))
        {
          if (Lt(entities.ObligorRule.NonAdcArrearsMaxAmount,
            local.UpdByProcDt.NaArrearsRecapAmt.GetValueOrDefault() +
            local.TotalNadcArrToRecapture.TotalCurrency))
          {
            local.TotalNadcArrToRecapture.TotalCurrency =
              entities.ObligorRule.NonAdcArrearsMaxAmount.GetValueOrDefault() -
              local.UpdByProcDt.NaArrearsRecapAmt.GetValueOrDefault();
          }
        }

        local.TotalRecoveryDebtsBal.TotalCurrency -= local.
          TotalNadcArrToRecapture.TotalCurrency;
      }
    }

    if (local.TotalPassthruDisb.TotalCurrency > 0)
    {
      if (Lt(0, entities.ObligorRule.PassthruPercentage))
      {
        local.TotalPassthruToRecapture.TotalCurrency =
          local.TotalPassthruDisb.TotalCurrency * entities
          .ObligorRule.PassthruPercentage.GetValueOrDefault() / 100;
      }
      else if (Lt(0, entities.ObligorRule.PassthruAmount))
      {
        local.TotalPassthruToRecapture.TotalCurrency =
          entities.ObligorRule.PassthruAmount.GetValueOrDefault();

        if (local.TotalPassthruToRecapture.TotalCurrency > local
          .TotalPassthruDisb.TotalCurrency)
        {
          local.TotalPassthruToRecapture.TotalCurrency =
            local.TotalPassthruDisb.TotalCurrency;
        }
      }

      if (local.TotalPassthruToRecapture.TotalCurrency > local
        .TotalRecoveryDebtsBal.TotalCurrency)
      {
        local.TotalPassthruToRecapture.TotalCurrency =
          local.TotalRecoveryDebtsBal.TotalCurrency;
      }

      // ---------------------------------------------------------------------------------------
      // If a monthly maximum is specified, limit the recapture to the remaining
      // recapturable amount for the month.
      // ---------------------------------------------------------------------------------------
      if (Lt(0, entities.ObligorRule.PassthruMaxAmount))
      {
        if (Lt(entities.ObligorRule.PassthruMaxAmount,
          local.UpdByProcDt.PassthruRecapAmt.GetValueOrDefault() +
          local.TotalPassthruToRecapture.TotalCurrency))
        {
          local.TotalPassthruToRecapture.TotalCurrency =
            entities.ObligorRule.PassthruMaxAmount.GetValueOrDefault() - local
            .UpdByProcDt.PassthruRecapAmt.GetValueOrDefault();
        }
      }
    }

    if (local.TotalPassthruToRecapture.TotalCurrency + local
      .TotalNadcArrToRecapture.TotalCurrency + local
      .TotalNadcCurrToRecapture.TotalCurrency <= 0)
    {
      return;
    }

    if (!ReadCsePersonAccount())
    {
      ExitState = "FN0000_OBLIGEE_NF";

      return;
    }

    // ---------------------------------------------
    // Now we have computed the recaptured amount by NADC current, NADC arrears 
    // and Passthru.
    // ---------------------------------------------
    // ---------------------------------------------
    // Read Each Disb Tran; Recapture the amount that can be taken out. Split 
    // the Disb Tran into two, one with the reduced amount and the other with
    // the recaptured amount. Assoc the recaptured disb tran with Recap Payment
    // Request.
    // ---------------------------------------------
    foreach(var item in ReadDisbursementTransactionDisbursementType2())
    {
      // --- Initialize the amount to be recaptured from this debit disb tran.
      local.RecapAmountDebit.Amount = 0;

      if ((AsChar(entities.DisbursementType.CurrentArrearsInd) == 'C' || AsChar
        (entities.DisbursementType.CurrentArrearsInd) == 'A') && Equal
        (entities.DisbursementType.ProgramCode, "NA"))
      {
        // --- NADC Current or Arrears disbursement
      }
      else if (Equal(entities.DisbursementType.Code, "PT"))
      {
        // --- Passthru disbursement
      }
      else
      {
        // --- Skip anything other than NADC Current, NADC Arrears, Passthru 
        // disbursements
        continue;
      }

      if (Equal(entities.DisbursementType.Code, "PT"))
      {
        if (local.TotalPassthruToRecapture.TotalCurrency > 0)
        {
          if (Lt(0, entities.ObligorRule.PassthruPercentage))
          {
            local.RecapAmountDebit.Amount = entities.Debit.Amount * (
              (decimal?)entities.ObligorRule.PassthruPercentage / 100
              ).GetValueOrDefault();

            if (local.RecapAmountDebit.Amount > local
              .TotalPassthruToRecapture.TotalCurrency)
            {
              local.RecapAmountDebit.Amount =
                local.TotalPassthruToRecapture.TotalCurrency;
            }
          }
          else if (entities.Debit.Amount > local
            .TotalPassthruToRecapture.TotalCurrency)
          {
            local.RecapAmountDebit.Amount =
              local.TotalPassthruToRecapture.TotalCurrency;
          }
          else
          {
            local.RecapAmountDebit.Amount = entities.Debit.Amount;
          }

          local.TotalPassthruToRecapture.TotalCurrency -= local.
            RecapAmountDebit.Amount;
          local.TotPassthruRecaptured.TotalCurrency += local.RecapAmountDebit.
            Amount;
        }
        else
        {
          continue;
        }
      }
      else if (AsChar(entities.DisbursementType.CurrentArrearsInd) == 'C')
      {
        if (local.TotalNadcCurrToRecapture.TotalCurrency > 0)
        {
          if (Lt(0, entities.ObligorRule.NonAdcCurrentPercentage))
          {
            local.RecapAmountDebit.Amount = entities.Debit.Amount * (
              (decimal?)entities.ObligorRule.NonAdcCurrentPercentage / 100
              ).GetValueOrDefault();

            if (local.RecapAmountDebit.Amount > local
              .TotalNadcCurrToRecapture.TotalCurrency)
            {
              local.RecapAmountDebit.Amount =
                local.TotalNadcCurrToRecapture.TotalCurrency;
            }
          }
          else if (entities.Debit.Amount > local
            .TotalNadcCurrToRecapture.TotalCurrency)
          {
            local.RecapAmountDebit.Amount =
              local.TotalNadcCurrToRecapture.TotalCurrency;
          }
          else
          {
            local.RecapAmountDebit.Amount = entities.Debit.Amount;
          }

          local.TotalNadcCurrToRecapture.TotalCurrency -= local.
            RecapAmountDebit.Amount;
          local.TotNaCurrRecaptured.TotalCurrency += local.RecapAmountDebit.
            Amount;
        }
        else
        {
          continue;
        }
      }
      else if (AsChar(entities.DisbursementType.CurrentArrearsInd) == 'A')
      {
        if (local.TotalNadcArrToRecapture.TotalCurrency > 0)
        {
          if (Lt(0, entities.ObligorRule.NonAdcArrearsPercentage))
          {
            local.RecapAmountDebit.Amount = entities.Debit.Amount * (
              (decimal?)entities.ObligorRule.NonAdcArrearsPercentage / 100
              ).GetValueOrDefault();

            if (local.RecapAmountDebit.Amount > local
              .TotalNadcArrToRecapture.TotalCurrency)
            {
              local.RecapAmountDebit.Amount =
                local.TotalNadcArrToRecapture.TotalCurrency;
            }
          }
          else if (entities.Debit.Amount > local
            .TotalNadcArrToRecapture.TotalCurrency)
          {
            local.RecapAmountDebit.Amount =
              local.TotalNadcArrToRecapture.TotalCurrency;
          }
          else
          {
            local.RecapAmountDebit.Amount = entities.Debit.Amount;
          }

          local.TotalNadcArrToRecapture.TotalCurrency -= local.RecapAmountDebit.
            Amount;
          local.TotNaArrRecaptured.TotalCurrency += local.RecapAmountDebit.
            Amount;
        }
        else
        {
          continue;
        }
      }

      export.Recap.Amount += local.RecapAmountDebit.Amount;

      // ---------------------------------------------
      // Initially local_recapture payment request identifier will be
      // zero; so the cab will create a new RCAP Payment Request.
      // Subsequently it will contain the identifier of the RCAP
      // Payment Request created. So all the subsequent Disb
      // Debits will be associated with that RCAP Payment Request.
      // In other words there will be only one RCAP Payment
      // Request created.
      // ---------------------------------------------
      UseFnRcapRecapAmountFDisbTran();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.UpdByCollDt.Year = Year(entities.Debit.CollectionDate);
      local.UpdByCollDt.Month = Month(entities.Debit.CollectionDate);

      if (AsChar(entities.Debit.ExcessUraInd) == 'Y')
      {
        local.UpdByCollDt.TotExcessUraAmt = -local.RecapAmountDebit.Amount;
      }
      else if (entities.DisbursementType.SystemGeneratedIdentifier == 71)
      {
        local.UpdByCollDt.PassthruAmount = -local.RecapAmountDebit.Amount;
      }
      else
      {
        local.UpdByCollDt.CollectionsDisbursedToAr =
          -local.RecapAmountDebit.Amount;
      }

      local.UpdByCollDt.RecapturedAmt = local.RecapAmountDebit.Amount;
      UseFnUpdateObligeeMonthlyTotals2();

      if (local.TotalNadcCurrToRecapture.TotalCurrency == 0 && local
        .TotalNadcArrToRecapture.TotalCurrency == 0 && local
        .TotalPassthruToRecapture.TotalCurrency == 0)
      {
        // --- Nothing left to recapture
        break;
      }
    }

    export.NbrOfRcpPmtsCreated.Count = 1;
    local.UpdByProcDt.PassthruRecapAmt =
      local.TotPassthruRecaptured.TotalCurrency;
    local.UpdByProcDt.NaCurrRecapAmt = local.TotNaCurrRecaptured.TotalCurrency;
    local.UpdByProcDt.NaArrearsRecapAmt =
      local.TotNaArrRecaptured.TotalCurrency;
    UseFnUpdateObligeeMonthlyTotals1();

    // ---------------------------------------------
    // Now create a Cash Receipt for the recaptured amount.
    // ---------------------------------------------
    UseFnRcapCreateCashReceipt();
  }

  private static void MoveCsePersonAccount(CsePersonAccount source,
    CsePersonAccount target)
  {
    target.Type1 = source.Type1;
    target.CspNumber = source.CspNumber;
  }

  private static void MoveMonthlyObligeeSummary1(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.PassthruRecapAmt = source.PassthruRecapAmt;
    target.NaArrearsRecapAmt = source.NaArrearsRecapAmt;
    target.NaCurrRecapAmt = source.NaCurrRecapAmt;
  }

  private static void MoveMonthlyObligeeSummary2(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.RecapturedAmt = source.RecapturedAmt;
    target.PassthruAmount = source.PassthruAmount;
    target.CollectionsDisbursedToAr = source.CollectionsDisbursedToAr;
    target.TotExcessUraAmt = source.TotExcessUraAmt;
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private void UseFnRcapCompRecDebtsBalFObe()
  {
    var useImport = new FnRcapCompRecDebtsBalFObe.Import();
    var useExport = new FnRcapCompRecDebtsBalFObe.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Obligee.Number = import.Obligee.Number;

    Call(FnRcapCompRecDebtsBalFObe.Execute, useImport, useExport);

    local.TotalRecoveryDebtsBal.TotalCurrency =
      useExport.TotalRecoveryDebtBal.TotalCurrency;
  }

  private void UseFnRcapCreateCashReceipt()
  {
    var useImport = new FnRcapCreateCashReceipt.Import();
    var useExport = new FnRcapCreateCashReceipt.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Obligee.Number = import.Obligee.Number;
    useImport.Recapture.Amount = export.Recap.Amount;

    Call(FnRcapCreateCashReceipt.Execute, useImport, useExport);
  }

  private void UseFnRcapRecapAmountFDisbTran()
  {
    var useImport = new FnRcapRecapAmountFDisbTran.Import();
    var useExport = new FnRcapRecapAmountFDisbTran.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.RecapAmountDebit.Amount = local.RecapAmountDebit.Amount;
    useImport.Obligee.Number = import.Obligee.Number;
    useImport.Recap.SystemGeneratedIdentifier =
      local.Recapture.SystemGeneratedIdentifier;
    useImport.DisbursementTransaction.SystemGeneratedIdentifier =
      entities.Debit.SystemGeneratedIdentifier;

    Call(FnRcapRecapAmountFDisbTran.Execute, useImport, useExport);

    MovePaymentRequest(useExport.Recap, local.Recapture);
  }

  private void UseFnUpdateObligeeMonthlyTotals1()
  {
    var useImport = new FnUpdateObligeeMonthlyTotals.Import();
    var useExport = new FnUpdateObligeeMonthlyTotals.Export();

    useImport.Per.Assign(entities.ObligeeCsePersonAccount);
    MoveMonthlyObligeeSummary1(local.UpdByProcDt,
      useImport.MonthlyObligeeSummary);

    Call(FnUpdateObligeeMonthlyTotals.Execute, useImport, useExport);

    MoveCsePersonAccount(useImport.Per, entities.ObligeeCsePersonAccount);
  }

  private void UseFnUpdateObligeeMonthlyTotals2()
  {
    var useImport = new FnUpdateObligeeMonthlyTotals.Import();
    var useExport = new FnUpdateObligeeMonthlyTotals.Export();

    useImport.Per.Assign(entities.ObligeeCsePersonAccount);
    MoveMonthlyObligeeSummary2(local.UpdByCollDt,
      useImport.MonthlyObligeeSummary);

    Call(FnUpdateObligeeMonthlyTotals.Execute, useImport, useExport);

    MoveCsePersonAccount(useImport.Per, entities.ObligeeCsePersonAccount);
  }

  private bool ReadCsePersonAccount()
  {
    entities.ObligeeCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.ObligeeCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.ObligeeCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.ObligeeCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligeeCsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionDisbursementType1()
  {
    entities.Debit.Populated = false;
    entities.DisbursementType.Populated = false;

    return ReadEach("ReadDisbursementTransactionDisbursementType1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Debit.CpaType = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debit.Type1 = db.GetString(reader, 3);
        entities.Debit.Amount = db.GetDecimal(reader, 4);
        entities.Debit.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Debit.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Debit.LastUpdateTmst = db.GetNullableDateTime(reader, 7);
        entities.Debit.CashNonCashInd = db.GetString(reader, 8);
        entities.Debit.CollectionDate = db.GetNullableDate(reader, 9);
        entities.Debit.DbtGeneratedId = db.GetNullableInt32(reader, 10);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.Debit.PrqGeneratedId = db.GetNullableInt32(reader, 11);
        entities.Debit.ReferenceNumber = db.GetNullableString(reader, 12);
        entities.Debit.ExcessUraInd = db.GetNullableString(reader, 13);
        entities.DisbursementType.Code = db.GetString(reader, 14);
        entities.DisbursementType.CurrentArrearsInd =
          db.GetNullableString(reader, 15);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 16);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 17);
        entities.Debit.Populated = true;
        entities.DisbursementType.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Debit.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Debit.CashNonCashInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionDisbursementType2()
  {
    entities.Debit.Populated = false;
    entities.DisbursementType.Populated = false;

    return ReadEach("ReadDisbursementTransactionDisbursementType2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Debit.CpaType = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debit.Type1 = db.GetString(reader, 3);
        entities.Debit.Amount = db.GetDecimal(reader, 4);
        entities.Debit.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Debit.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Debit.LastUpdateTmst = db.GetNullableDateTime(reader, 7);
        entities.Debit.CashNonCashInd = db.GetString(reader, 8);
        entities.Debit.CollectionDate = db.GetNullableDate(reader, 9);
        entities.Debit.DbtGeneratedId = db.GetNullableInt32(reader, 10);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.Debit.PrqGeneratedId = db.GetNullableInt32(reader, 11);
        entities.Debit.ReferenceNumber = db.GetNullableString(reader, 12);
        entities.Debit.ExcessUraInd = db.GetNullableString(reader, 13);
        entities.DisbursementType.Code = db.GetString(reader, 14);
        entities.DisbursementType.CurrentArrearsInd =
          db.GetNullableString(reader, 15);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 16);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 17);
        entities.Debit.Populated = true;
        entities.DisbursementType.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Debit.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.Debit.CashNonCashInd);

        return true;
      });
  }

  private bool ReadMonthlyObligeeSummary()
  {
    entities.MonthlyObligeeSummary.Populated = false;

    return Read("ReadMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetString(command, "cspSNumber", import.Obligee.Number);
        db.SetInt32(command, "yer", local.UpdByProcDt.Year);
        db.SetInt32(command, "mnth", local.UpdByProcDt.Month);
      },
      (db, reader) =>
      {
        entities.MonthlyObligeeSummary.Year = db.GetInt32(reader, 0);
        entities.MonthlyObligeeSummary.Month = db.GetInt32(reader, 1);
        entities.MonthlyObligeeSummary.PassthruRecapAmt =
          db.GetNullableDecimal(reader, 2);
        entities.MonthlyObligeeSummary.NaArrearsRecapAmt =
          db.GetNullableDecimal(reader, 3);
        entities.MonthlyObligeeSummary.PassthruAmount =
          db.GetDecimal(reader, 4);
        entities.MonthlyObligeeSummary.CreatedBy = db.GetString(reader, 5);
        entities.MonthlyObligeeSummary.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.MonthlyObligeeSummary.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.MonthlyObligeeSummary.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.MonthlyObligeeSummary.CpaSType = db.GetString(reader, 9);
        entities.MonthlyObligeeSummary.CspSNumber = db.GetString(reader, 10);
        entities.MonthlyObligeeSummary.NaCurrRecapAmt =
          db.GetNullableDecimal(reader, 11);
        entities.MonthlyObligeeSummary.Populated = true;
        CheckValid<MonthlyObligeeSummary>("CpaSType",
          entities.MonthlyObligeeSummary.CpaSType);
      });
  }

  private bool ReadObligorRule()
  {
    entities.ObligorRule.Populated = false;

    return Read("ReadObligorRule",
      (db, command) =>
      {
        db.SetNullableString(command, "cspDNumber", import.Obligee.Number);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligorRule.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ObligorRule.CpaDType = db.GetNullableString(reader, 1);
        entities.ObligorRule.CspDNumber = db.GetNullableString(reader, 2);
        entities.ObligorRule.EffectiveDate = db.GetDate(reader, 3);
        entities.ObligorRule.NegotiatedDate = db.GetNullableDate(reader, 4);
        entities.ObligorRule.DiscontinueDate = db.GetNullableDate(reader, 5);
        entities.ObligorRule.NonAdcArrearsMaxAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ObligorRule.NonAdcArrearsAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ObligorRule.NonAdcArrearsPercentage =
          db.GetNullableInt32(reader, 8);
        entities.ObligorRule.NonAdcCurrentMaxAmount =
          db.GetNullableDecimal(reader, 9);
        entities.ObligorRule.NonAdcCurrentAmount =
          db.GetNullableDecimal(reader, 10);
        entities.ObligorRule.NonAdcCurrentPercentage =
          db.GetNullableInt32(reader, 11);
        entities.ObligorRule.PassthruPercentage =
          db.GetNullableInt32(reader, 12);
        entities.ObligorRule.PassthruAmount = db.GetNullableDecimal(reader, 13);
        entities.ObligorRule.PassthruMaxAmount =
          db.GetNullableDecimal(reader, 14);
        entities.ObligorRule.Type1 = db.GetString(reader, 15);
        entities.ObligorRule.Populated = true;
        CheckValid<RecaptureRule>("CpaDType", entities.ObligorRule.CpaDType);
        CheckValid<RecaptureRule>("Type1", entities.ObligorRule.Type1);
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private CsePerson obligee;
    private ProgramProcessingInfo programProcessingInfo;
    private PaymentRequest paymentRequest;
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

    /// <summary>
    /// A value of NbrOfRcpPmtsCreated.
    /// </summary>
    [JsonPropertyName("nbrOfRcpPmtsCreated")]
    public Common NbrOfRcpPmtsCreated
    {
      get => nbrOfRcpPmtsCreated ??= new();
      set => nbrOfRcpPmtsCreated = value;
    }

    private PaymentRequest recap;
    private Common nbrOfRcpPmtsCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TotPassthruRecaptured.
    /// </summary>
    [JsonPropertyName("totPassthruRecaptured")]
    public Common TotPassthruRecaptured
    {
      get => totPassthruRecaptured ??= new();
      set => totPassthruRecaptured = value;
    }

    /// <summary>
    /// A value of TotNaCurrRecaptured.
    /// </summary>
    [JsonPropertyName("totNaCurrRecaptured")]
    public Common TotNaCurrRecaptured
    {
      get => totNaCurrRecaptured ??= new();
      set => totNaCurrRecaptured = value;
    }

    /// <summary>
    /// A value of TotNaArrRecaptured.
    /// </summary>
    [JsonPropertyName("totNaArrRecaptured")]
    public Common TotNaArrRecaptured
    {
      get => totNaArrRecaptured ??= new();
      set => totNaArrRecaptured = value;
    }

    /// <summary>
    /// A value of TempOrigObligee.
    /// </summary>
    [JsonPropertyName("tempOrigObligee")]
    public CsePerson TempOrigObligee
    {
      get => tempOrigObligee ??= new();
      set => tempOrigObligee = value;
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
    /// A value of RecapAmountDebit.
    /// </summary>
    [JsonPropertyName("recapAmountDebit")]
    public DisbursementTransaction RecapAmountDebit
    {
      get => recapAmountDebit ??= new();
      set => recapAmountDebit = value;
    }

    /// <summary>
    /// A value of UpdByCollDt.
    /// </summary>
    [JsonPropertyName("updByCollDt")]
    public MonthlyObligeeSummary UpdByCollDt
    {
      get => updByCollDt ??= new();
      set => updByCollDt = value;
    }

    /// <summary>
    /// A value of UpdByProcDt.
    /// </summary>
    [JsonPropertyName("updByProcDt")]
    public MonthlyObligeeSummary UpdByProcDt
    {
      get => updByProcDt ??= new();
      set => updByProcDt = value;
    }

    /// <summary>
    /// A value of ToInitialize.
    /// </summary>
    [JsonPropertyName("toInitialize")]
    public MonthlyObligeeSummary ToInitialize
    {
      get => toInitialize ??= new();
      set => toInitialize = value;
    }

    /// <summary>
    /// A value of TotalRecoveryDebtsBal.
    /// </summary>
    [JsonPropertyName("totalRecoveryDebtsBal")]
    public Common TotalRecoveryDebtsBal
    {
      get => totalRecoveryDebtsBal ??= new();
      set => totalRecoveryDebtsBal = value;
    }

    /// <summary>
    /// A value of TotalPassthruToRecapture.
    /// </summary>
    [JsonPropertyName("totalPassthruToRecapture")]
    public Common TotalPassthruToRecapture
    {
      get => totalPassthruToRecapture ??= new();
      set => totalPassthruToRecapture = value;
    }

    /// <summary>
    /// A value of TotalNadcCurrToRecapture.
    /// </summary>
    [JsonPropertyName("totalNadcCurrToRecapture")]
    public Common TotalNadcCurrToRecapture
    {
      get => totalNadcCurrToRecapture ??= new();
      set => totalNadcCurrToRecapture = value;
    }

    /// <summary>
    /// A value of TotalNadcArrToRecapture.
    /// </summary>
    [JsonPropertyName("totalNadcArrToRecapture")]
    public Common TotalNadcArrToRecapture
    {
      get => totalNadcArrToRecapture ??= new();
      set => totalNadcArrToRecapture = value;
    }

    /// <summary>
    /// A value of TotalPassthruDisb.
    /// </summary>
    [JsonPropertyName("totalPassthruDisb")]
    public Common TotalPassthruDisb
    {
      get => totalPassthruDisb ??= new();
      set => totalPassthruDisb = value;
    }

    /// <summary>
    /// A value of TotalNadcCurrDisb.
    /// </summary>
    [JsonPropertyName("totalNadcCurrDisb")]
    public Common TotalNadcCurrDisb
    {
      get => totalNadcCurrDisb ??= new();
      set => totalNadcCurrDisb = value;
    }

    /// <summary>
    /// A value of TotalNadcArrDisb.
    /// </summary>
    [JsonPropertyName("totalNadcArrDisb")]
    public Common TotalNadcArrDisb
    {
      get => totalNadcArrDisb ??= new();
      set => totalNadcArrDisb = value;
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

    private Common totPassthruRecaptured;
    private Common totNaCurrRecaptured;
    private Common totNaArrRecaptured;
    private CsePerson tempOrigObligee;
    private PaymentRequest recapture;
    private DisbursementTransaction recapAmountDebit;
    private MonthlyObligeeSummary updByCollDt;
    private MonthlyObligeeSummary updByProcDt;
    private MonthlyObligeeSummary toInitialize;
    private Common totalRecoveryDebtsBal;
    private Common totalPassthruToRecapture;
    private Common totalNadcCurrToRecapture;
    private Common totalNadcArrToRecapture;
    private Common totalPassthruDisb;
    private Common totalNadcCurrDisb;
    private Common totalNadcArrDisb;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligeeCsePersonAccount")]
    public CsePersonAccount ObligeeCsePersonAccount
    {
      get => obligeeCsePersonAccount ??= new();
      set => obligeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ObligorRule.
    /// </summary>
    [JsonPropertyName("obligorRule")]
    public RecaptureRule ObligorRule
    {
      get => obligorRule ??= new();
      set => obligorRule = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public PaymentRequest Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of ObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("obligeeCsePerson")]
    public CsePerson ObligeeCsePerson
    {
      get => obligeeCsePerson ??= new();
      set => obligeeCsePerson = value;
    }

    /// <summary>
    /// A value of ReadForCurrency.
    /// </summary>
    [JsonPropertyName("readForCurrency")]
    public CsePersonAccount ReadForCurrency
    {
      get => readForCurrency ??= new();
      set => readForCurrency = value;
    }

    private CsePersonAccount obligeeCsePersonAccount;
    private RecaptureRule obligorRule;
    private CsePersonAccount obligor;
    private PaymentRequest current;
    private DisbursementTransaction debit;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private DisbursementType disbursementType;
    private CsePersonAccount obligee;
    private CsePerson obligeeCsePerson;
    private CsePersonAccount readForCurrency;
  }
#endregion
}
