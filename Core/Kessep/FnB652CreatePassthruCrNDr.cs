// Program: FN_B652_CREATE_PASSTHRU_CR_N_DR, ID: 372708301, model: 746.
// Short name: SWE02151
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
/// A program: FN_B652_CREATE_PASSTHRU_CR_N_DR.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block creates the passthru credit record and its corresponding 
/// debits records for a given obligee-month
/// </para>
/// </summary>
[Serializable]
public partial class FnB652CreatePassthruCrNDr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B652_CREATE_PASSTHRU_CR_N_DR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB652CreatePassthruCrNDr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB652CreatePassthruCrNDr.
  /// </summary>
  public FnB652CreatePassthruCrNDr(IContext context, Import import,
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
    // This action block creates the passthru credit record and its 
    // corresponding passthru debit records for a given obligee-month.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // 102197	govind		Initial code.
    // 122297	govind		Set Collection Date to the Passthru Date to enable PACC to
    // list the passthrus
    // 011998	govind		Removed the redundant UPDATE Monthly Obligee Summary 
    // statement
    // 013098	govind		Added a check for monthly obligee summary adc amount 
    // negative
    // 020199  N.Engoor        Removed Hardcoded_Disbursement_Info CAB.
    // 04192000 K.Doshi PR#93468 Skip Non-cash payments when creating passthrus.
    // Also added interstate_ind qualification.
    // 09142000  Fangman  PR 103323  Cleanup views for an AB.
    // ---------------------------------------------
    local.DisbursementTransaction.Amount = 0;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // ---------------------------------------------
    // Added a Set stmnt and removed the CAB hardcoded disbursement info since 
    // only one view was being view matched.
    // ---------------------------------------------
    local.HardcodedPerson.Type1 = "P";
    local.Existing.Year = import.Passthru.YearMonth / 100;
    local.Existing.Month = import.Passthru.YearMonth - local.Existing.Year * 100
      ;
    UseOeGetMonthStartAndEndDate();

    if (!ReadCsePersonObligee())
    {
      ExitState = "FN0000_OBLIGEE_NF";

      return;
    }

    // -------------------------------------------------------------
    // Find out how much how much passthru has already been paid out.
    // --------------------------------------------------------------
    if (ReadMonthlyObligeeSummary())
    {
      local.Existing.Assign(entities.MonthlyObligeeSummary);
      export.MonthlyObligeeSummary.Assign(entities.MonthlyObligeeSummary);
    }
    else
    {
      // ---  Not an error.
      try
      {
        CreateMonthlyObligeeSummary();
        export.MonthlyObligeeSummary.Assign(entities.MonthlyObligeeSummary);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_MTH_OBLIGEE_SUM_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_MTH_OBLIGEE_SUM_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ------------------------------
    // Changed READ stmnt.
    // ------------------------------
    if (!ReadMaximumPassthru())
    {
      ExitState = "MAXIMUM_PASSTHRU_NF";

      return;
    }

    local.MaxPassthruAmount.TotalCurrency = entities.MaximumPassthru.Amount;

    // -----------------
    // The total disbursement amounts including the adjustments that have come 
    // in as disbursements during that month are accumulated and passed from the
    // Prad. A check is done between that amount and the existing passthru
    // amount that is got from the monthly obligee summary for that month. The
    // additional passthru amount is then calculated.
    // -----------------
    // -------------------------------------------------------------
    // Find out how much adc collections received for the month for the Obligee.
    // --------------------------------------------------------------
    // --------------------------------------------------------
    // 04192000 K.Doshi  PR#93468
    // Skip Non-cash payments when creating passthrus. Also added interstate_ind
    // qualification.
    // --------------------------------------------------------
    foreach(var item in ReadDisbursementTransaction())
    {
      if (AsChar(entities.TotalDebit.InterstateInd) == 'Y')
      {
        // -----------
        // Should not happen.
        // -----------
        continue;
      }

      if (!ReadDisbursementTransactionCollectionDisbursementTransactionRln())
      {
        continue;
      }

      if (!ReadDebtDetail())
      {
        // ----------------------------------
        // Should not happen
        // ----------------------------------
        continue;
      }

      if (!Lt(entities.DebtDetail.DueDt, local.PassthruMonthStartDate.Date) && !
        Lt(local.PassthruMonthEndDate.Date, entities.DebtDetail.DueDt))
      {
      }
      else if (Equal(entities.DebtDetail.DueDt, local.Zero.Date))
      {
        // ----------------------------------
        // If the disbursement is voluntary the due date is null.
        // ----------------------------------
        if (!Lt(entities.TotalDebit.CollectionDate,
          local.PassthruMonthStartDate.Date) && !
          Lt(local.PassthruMonthEndDate.Date, entities.TotalDebit.CollectionDate))
          
        {
        }
        else
        {
          continue;
        }
      }
      else
      {
        // ----------------------------------
        // Skip processing.
        // ----------------------------------
        continue;
      }

      local.DisbursementTransaction.Amount += entities.TotalDebit.Amount;
    }

    if (local.DisbursementTransaction.Amount > local
      .MaxPassthruAmount.TotalCurrency)
    {
      local.NewPassthruCredit.Amount = local.MaxPassthruAmount.TotalCurrency;
    }
    else
    {
      local.NewPassthruCredit.Amount = local.DisbursementTransaction.Amount;
    }

    // ---------------------------------------------------------------------
    // Compute the additional passthru payable
    // ---------------------------------------------------------------------
    local.AddlPassthruPayableCr.Amount = local.NewPassthruCredit.Amount - local
      .Existing.PassthruAmount;
    local.Changes.PassthruAmount = local.AddlPassthruPayableCr.Amount;

    if (local.DisbursementTransaction.Amount < 0)
    {
      // ---------------------------------------------------------------------
      // npe - 07/28/1999
      // This should never happen but as of now there are total disb amounts for
      // an AR for a month that is negative. This code  is just to make sure
      // that the passthru amount is calculated correctly.
      // ---------------------------------------------------------------------
      local.AddlPassthruPayableCr.Amount = -local.Existing.PassthruAmount;
      local.Changes.PassthruAmount = 0;

      if (local.AddlPassthruPayableCr.Amount == 0)
      {
        // -----------
        // No passthru to be created.
        // -----------
        return;
      }
    }
    else if (local.AddlPassthruPayableCr.Amount == 0)
    {
      // -----------
      // No passthru to be created.
      // -----------
      return;
    }
    else
    {
      // ----------------------------------------------------
      // Calculate the new passthru amount in the monthly obligee summary for 
      // the AR for the month.
      // ----------------------------------------------------
      local.Changes.PassthruAmount = local.Existing.PassthruAmount + local
        .AddlPassthruPayableCr.Amount;
    }

    // -----------------------
    // Removed call to the CAB - Update monthly Obligee Totals since the cab 
    // generates an alert is to be generated.
    // -----------------------
    try
    {
      UpdateMonthlyObligeeSummary();
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

    // ---------------------------------------------
    // Create passthru credit record for the net passthru amount. (It could be 
    // positive or negative amount)
    // ---------------------------------------------
    // ----------------------------
    // Setting the Interstate_ind to 'N' while creating a Passthru credit 
    // record. Reference number is not being set to make it consistent with the
    // debit records for passthrus.
    // ----------------------------
    for(local.Retry.Count = 1; local.Retry.Count <= 10; ++local.Retry.Count)
    {
      local.Current.Timestamp = Now();

      try
      {
        CreatePassthru();

        break;
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

      ++local.Retry.Count;

      if (local.Retry.Count == 10)
      {
        ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

        return;
      }
    }

    if (!ReadCsePersonAccount())
    {
      ExitState = "CSE_PERSON_ACCOUNT_NF";

      return;
    }

    UseFnCheckForPerDisbSup();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // Create the passthru Disbursement Debits for the passthru Disbursement 
    // Credit created above.
    // ---------------------------------------------
    local.NewDebitPassthru.CashNonCashInd = "C";
    local.NewDebitPassthru.RecapturedInd = "N";

    // ----------------------------
    // Setting the Interstate_ind to 'N' while creating the Passthru debit 
    // record.
    // ----------------------------
    local.NewDebitPassthru.InterstateInd = "N";
    local.NewDebitPassthru.Amount = local.AddlPassthruPayableCr.Amount;
    local.NewDebitPassthru.DisbursementDate =
      import.ProgramProcessingInfo.ProcessDate;

    if (Lt(local.Zero.Date, local.DisbSuppressionStatusHistory.DiscontinueDate))
    {
      local.ReleasedOrSuppressed.Flag = "S";
    }
    else
    {
      local.ReleasedOrSuppressed.Flag = "R";
    }

    export.ReleasedOrSuppressed.Flag = local.ReleasedOrSuppressed.Flag;
    UseFnB652CreatePassthruDisbDeb();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
  }

  private static void MoveDisbursementTransaction1(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.InterstateInd = source.InterstateInd;
    target.DisbursementDate = source.DisbursementDate;
    target.CashNonCashInd = source.CashNonCashInd;
    target.RecapturedInd = source.RecapturedInd;
  }

  private static void MoveDisbursementTransaction2(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.PassthruDate = source.PassthruDate;
  }

  private void UseFnB652CreatePassthruDisbDeb()
  {
    var useImport = new FnB652CreatePassthruDisbDeb.Import();
    var useExport = new FnB652CreatePassthruDisbDeb.Export();

    MoveDisbursementTransaction2(entities.Passthru, useImport.Credit);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ReleaseOrSuppressedInd.Flag = local.ReleasedOrSuppressed.Flag;
    MoveDisbursementTransaction1(local.NewDebitPassthru,
      useImport.NewDebitPassthru);
    useImport.PersistentCsePersonAccount.Assign(entities.Obligee1);

    Call(FnB652CreatePassthruDisbDeb.Execute, useImport, useExport);

    entities.Obligee1.Type1 = useImport.PersistentCsePersonAccount.Type1;
  }

  private void UseFnCheckForPerDisbSup()
  {
    var useImport = new FnCheckForPerDisbSup.Import();
    var useExport = new FnCheckForPerDisbSup.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.CsePerson.Number = import.Obligee.Number;

    Call(FnCheckForPerDisbSup.Execute, useImport, useExport);

    local.DisbSuppressionStatusHistory.DiscontinueDate =
      useExport.DisbSuppressionStatusHistory.DiscontinueDate;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseOeGetMonthStartAndEndDate()
  {
    var useImport = new OeGetMonthStartAndEndDate.Import();
    var useExport = new OeGetMonthStartAndEndDate.Export();

    useImport.DateWorkArea.YearMonth = import.Passthru.YearMonth;

    Call(OeGetMonthStartAndEndDate.Execute, useImport, useExport);

    local.PassthruMonthEndDate.Date = useExport.MonthEndDate.Date;
    local.PassthruMonthStartDate.Date = useExport.MonthStartDate.Date;
  }

  private void CreateMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);

    var year = local.Existing.Year;
    var month = local.Existing.Month;
    var param = 0M;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cpaSType = entities.Obligee1.Type1;
    var cspSNumber = entities.Obligee1.CspNumber;

    CheckValid<MonthlyObligeeSummary>("CpaSType", cpaSType);
    entities.MonthlyObligeeSummary.Populated = false;
    Update("CreateMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetInt32(command, "yer", year);
        db.SetInt32(command, "mnth", month);
        db.SetNullableDecimal(command, "ptRecapAmt", param);
        db.SetDecimal(command, "passthruAmount", param);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableDecimal(command, "adcReimbursedAmt", param);
        db.SetString(command, "cpaSType", cpaSType);
        db.SetString(command, "cspSNumber", cspSNumber);
        db.SetNullableString(command, "zdelType", "");
        db.SetNullableInt32(command, "nbrOfCollections", 0);
      });

    entities.MonthlyObligeeSummary.Year = year;
    entities.MonthlyObligeeSummary.Month = month;
    entities.MonthlyObligeeSummary.PassthruAmount = param;
    entities.MonthlyObligeeSummary.CreatedBy = createdBy;
    entities.MonthlyObligeeSummary.CreatedTimestamp = createdTimestamp;
    entities.MonthlyObligeeSummary.LastUpdatedBy = "";
    entities.MonthlyObligeeSummary.LastUpdatedTmst = null;
    entities.MonthlyObligeeSummary.AdcReimbursedAmount = param;
    entities.MonthlyObligeeSummary.CpaSType = cpaSType;
    entities.MonthlyObligeeSummary.CspSNumber = cspSNumber;
    entities.MonthlyObligeeSummary.Populated = true;
  }

  private void CreatePassthru()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);

    var cpaType = entities.Obligee1.Type1;
    var cspNumber = entities.Obligee1.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "P";
    var amount = local.AddlPassthruPayableCr.Amount;
    var processDate = import.ProgramProcessingInfo.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var collectionDate = local.PassthruMonthEndDate.Date;
    var interstateInd = "N";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    entities.Passthru.Populated = false;
    Update("CreatePassthru",
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
        db.SetNullableDate(command, "disbursementDate", default(DateTime));
        db.SetString(
          command, "cashNonCashInd", GetImplicitValue<DisbursementTransaction,
          string>("CashNonCashInd"));
        db.SetString(command, "recapturedInd", "");
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetDate(command, "passthruDate", collectionDate);
        db.SetNullableString(
          command, "otrTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("OtrTypeDisb"));
        db.SetNullableString(
          command, "cpaTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("CpaTypeDisb"));
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableDate(command, "passthruProcDate", null);
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", "");
        db.SetInt32(command, "uraExcollSnbr", 0);
      });

    entities.Passthru.CpaType = cpaType;
    entities.Passthru.CspNumber = cspNumber;
    entities.Passthru.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Passthru.Type1 = type1;
    entities.Passthru.Amount = amount;
    entities.Passthru.ProcessDate = processDate;
    entities.Passthru.CreatedBy = createdBy;
    entities.Passthru.CreatedTimestamp = createdTimestamp;
    entities.Passthru.CollectionDate = collectionDate;
    entities.Passthru.PassthruDate = collectionDate;
    entities.Passthru.InterstateInd = interstateInd;
    entities.Passthru.PassthruProcDate = null;
    entities.Passthru.DesignatedPayee = "";
    entities.Passthru.ReferenceNumber = "";
    entities.Passthru.Populated = true;
  }

  private bool ReadCsePersonAccount()
  {
    entities.ObligeeForReread.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Obligee2.Number);
      },
      (db, reader) =>
      {
        entities.ObligeeForReread.CspNumber = db.GetString(reader, 0);
        entities.ObligeeForReread.Type1 = db.GetString(reader, 1);
        entities.ObligeeForReread.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.ObligeeForReread.Type1);
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

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otrGeneratedId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.TotalDebit.Populated = false;

    return ReadEach("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
      },
      (db, reader) =>
      {
        entities.TotalDebit.CpaType = db.GetString(reader, 0);
        entities.TotalDebit.CspNumber = db.GetString(reader, 1);
        entities.TotalDebit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.TotalDebit.Type1 = db.GetString(reader, 3);
        entities.TotalDebit.Amount = db.GetDecimal(reader, 4);
        entities.TotalDebit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.TotalDebit.CashNonCashInd = db.GetString(reader, 6);
        entities.TotalDebit.CollectionDate = db.GetNullableDate(reader, 7);
        entities.TotalDebit.DbtGeneratedId = db.GetNullableInt32(reader, 8);
        entities.TotalDebit.InterstateInd = db.GetNullableString(reader, 9);
        entities.TotalDebit.PassthruProcDate = db.GetNullableDate(reader, 10);
        entities.TotalDebit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.TotalDebit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.TotalDebit.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.TotalDebit.CashNonCashInd);

        return true;
      });
  }

  private bool ReadDisbursementTransactionCollectionDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.TotalDebit.Populated);
    entities.DisbursementTransactionRln.Populated = false;
    entities.Collection.Populated = false;
    entities.Credit.Populated = false;

    return Read(
      "ReadDisbursementTransactionCollectionDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.TotalDebit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.TotalDebit.CpaType);
        db.SetString(command, "cspNumber", entities.TotalDebit.CspNumber);
      },
      (db, reader) =>
      {
        entities.Credit.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 0);
        entities.Credit.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 1);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 2);
        entities.Credit.OtyId = db.GetNullableInt32(reader, 3);
        entities.Collection.OtyId = db.GetInt32(reader, 3);
        entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 4);
        entities.Collection.OtrType = db.GetString(reader, 4);
        entities.Credit.OtrId = db.GetNullableInt32(reader, 5);
        entities.Collection.OtrId = db.GetInt32(reader, 5);
        entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 6);
        entities.Credit.CspNumberDisb = db.GetNullableString(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Credit.ObgId = db.GetNullableInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Credit.CrdId = db.GetNullableInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 9);
        entities.Credit.CrvId = db.GetNullableInt32(reader, 10);
        entities.Collection.CrvId = db.GetInt32(reader, 10);
        entities.Credit.CstId = db.GetNullableInt32(reader, 11);
        entities.Collection.CstId = db.GetInt32(reader, 11);
        entities.Credit.CrtId = db.GetNullableInt32(reader, 12);
        entities.Collection.CrtType = db.GetInt32(reader, 12);
        entities.Credit.ColId = db.GetNullableInt32(reader, 13);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 13);
        entities.Collection.AppliedToCode = db.GetString(reader, 14);
        entities.Collection.CollectionDt = db.GetDate(reader, 15);
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 17);
        entities.DisbursementTransactionRln.CspNumber =
          db.GetString(reader, 18);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 19);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 20);
        entities.DisbursementTransactionRln.Populated = true;
        entities.Collection.Populated = true;
        entities.Credit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
        CheckValid<DisbursementTransactionRln>("CpaPType",
          entities.DisbursementTransactionRln.CpaPType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.Credit.OtrTypeDisb);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.Credit.CpaTypeDisb);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<DisbursementTransactionRln>("CpaType",
          entities.DisbursementTransactionRln.CpaType);
      });
  }

  private bool ReadMaximumPassthru()
  {
    entities.MaximumPassthru.Populated = false;

    return Read("ReadMaximumPassthru",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.PassthruMonthStartDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          local.PassthruMonthEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MaximumPassthru.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MaximumPassthru.Amount = db.GetDecimal(reader, 1);
        entities.MaximumPassthru.EffectiveDate = db.GetDate(reader, 2);
        entities.MaximumPassthru.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.MaximumPassthru.Populated = true;
      });
  }

  private bool ReadMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.MonthlyObligeeSummary.Populated = false;

    return Read("ReadMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetString(command, "cpaSType", entities.Obligee1.Type1);
        db.SetString(command, "cspSNumber", entities.Obligee1.CspNumber);
        db.SetInt32(command, "yer", local.Existing.Year);
        db.SetInt32(command, "mnth", local.Existing.Month);
      },
      (db, reader) =>
      {
        entities.MonthlyObligeeSummary.Year = db.GetInt32(reader, 0);
        entities.MonthlyObligeeSummary.Month = db.GetInt32(reader, 1);
        entities.MonthlyObligeeSummary.PassthruAmount =
          db.GetDecimal(reader, 2);
        entities.MonthlyObligeeSummary.CreatedBy = db.GetString(reader, 3);
        entities.MonthlyObligeeSummary.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.MonthlyObligeeSummary.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.MonthlyObligeeSummary.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.MonthlyObligeeSummary.AdcReimbursedAmount =
          db.GetNullableDecimal(reader, 7);
        entities.MonthlyObligeeSummary.CpaSType = db.GetString(reader, 8);
        entities.MonthlyObligeeSummary.CspSNumber = db.GetString(reader, 9);
        entities.MonthlyObligeeSummary.Populated = true;
        CheckValid<MonthlyObligeeSummary>("CpaSType",
          entities.MonthlyObligeeSummary.CpaSType);
      });
  }

  private void UpdateMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.MonthlyObligeeSummary.Populated);

    var passthruAmount = local.Changes.PassthruAmount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;

    entities.MonthlyObligeeSummary.Populated = false;
    Update("UpdateMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetDecimal(command, "passthruAmount", passthruAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "yer", entities.MonthlyObligeeSummary.Year);
        db.SetInt32(command, "mnth", entities.MonthlyObligeeSummary.Month);
        db.SetString(
          command, "cpaSType", entities.MonthlyObligeeSummary.CpaSType);
        db.SetString(
          command, "cspSNumber", entities.MonthlyObligeeSummary.CspSNumber);
      });

    entities.MonthlyObligeeSummary.PassthruAmount = passthruAmount;
    entities.MonthlyObligeeSummary.LastUpdatedBy = lastUpdatedBy;
    entities.MonthlyObligeeSummary.LastUpdatedTmst = lastUpdatedTmst;
    entities.MonthlyObligeeSummary.Populated = true;
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
    /// A value of AfterJune301999.
    /// </summary>
    [JsonPropertyName("afterJune301999")]
    public DisbursementTransaction AfterJune301999
    {
      get => afterJune301999 ??= new();
      set => afterJune301999 = value;
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
    /// A value of NetAdcCurrCollection.
    /// </summary>
    [JsonPropertyName("netAdcCurrCollection")]
    public DisbursementTransaction NetAdcCurrCollection
    {
      get => netAdcCurrCollection ??= new();
      set => netAdcCurrCollection = value;
    }

    private DisbursementTransaction afterJune301999;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson obligee;
    private DateWorkArea passthru;
    private DisbursementTransaction netAdcCurrCollection;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReleasedOrSuppressed.
    /// </summary>
    [JsonPropertyName("releasedOrSuppressed")]
    public Common ReleasedOrSuppressed
    {
      get => releasedOrSuppressed ??= new();
      set => releasedOrSuppressed = value;
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

    private Common releasedOrSuppressed;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public DisbursementTransaction Total
    {
      get => total ??= new();
      set => total = value;
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
    /// A value of AlertToCase.
    /// </summary>
    [JsonPropertyName("alertToCase")]
    public MonthlyObligeeSummary AlertToCase
    {
      get => alertToCase ??= new();
      set => alertToCase = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of CountPassthru.
    /// </summary>
    [JsonPropertyName("countPassthru")]
    public Common CountPassthru
    {
      get => countPassthru ??= new();
      set => countPassthru = value;
    }

    /// <summary>
    /// A value of GenerateNoAlert.
    /// </summary>
    [JsonPropertyName("generateNoAlert")]
    public Common GenerateNoAlert
    {
      get => generateNoAlert ??= new();
      set => generateNoAlert = value;
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
    /// A value of PassthruMonthEndDate.
    /// </summary>
    [JsonPropertyName("passthruMonthEndDate")]
    public DateWorkArea PassthruMonthEndDate
    {
      get => passthruMonthEndDate ??= new();
      set => passthruMonthEndDate = value;
    }

    /// <summary>
    /// A value of ReleasedOrSuppressed.
    /// </summary>
    [JsonPropertyName("releasedOrSuppressed")]
    public Common ReleasedOrSuppressed
    {
      get => releasedOrSuppressed ??= new();
      set => releasedOrSuppressed = value;
    }

    /// <summary>
    /// A value of NewDebitPassthru.
    /// </summary>
    [JsonPropertyName("newDebitPassthru")]
    public DisbursementTransaction NewDebitPassthru
    {
      get => newDebitPassthru ??= new();
      set => newDebitPassthru = value;
    }

    /// <summary>
    /// A value of HardcodedPerson.
    /// </summary>
    [JsonPropertyName("hardcodedPerson")]
    public DisbSuppressionStatusHistory HardcodedPerson
    {
      get => hardcodedPerson ??= new();
      set => hardcodedPerson = value;
    }

    /// <summary>
    /// A value of SuppressPersonDisb.
    /// </summary>
    [JsonPropertyName("suppressPersonDisb")]
    public Common SuppressPersonDisb
    {
      get => suppressPersonDisb ??= new();
      set => suppressPersonDisb = value;
    }

    /// <summary>
    /// A value of Changes.
    /// </summary>
    [JsonPropertyName("changes")]
    public MonthlyObligeeSummary Changes
    {
      get => changes ??= new();
      set => changes = value;
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
    /// A value of Retry.
    /// </summary>
    [JsonPropertyName("retry")]
    public Common Retry
    {
      get => retry ??= new();
      set => retry = value;
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
    /// A value of PassthruMonthStartDate.
    /// </summary>
    [JsonPropertyName("passthruMonthStartDate")]
    public DateWorkArea PassthruMonthStartDate
    {
      get => passthruMonthStartDate ??= new();
      set => passthruMonthStartDate = value;
    }

    /// <summary>
    /// A value of NewPassthruCredit.
    /// </summary>
    [JsonPropertyName("newPassthruCredit")]
    public DisbursementTransaction NewPassthruCredit
    {
      get => newPassthruCredit ??= new();
      set => newPassthruCredit = value;
    }

    /// <summary>
    /// A value of AddlPassthruPayableCr.
    /// </summary>
    [JsonPropertyName("addlPassthruPayableCr")]
    public DisbursementTransaction AddlPassthruPayableCr
    {
      get => addlPassthruPayableCr ??= new();
      set => addlPassthruPayableCr = value;
    }

    /// <summary>
    /// A value of MaxPassthruAmount.
    /// </summary>
    [JsonPropertyName("maxPassthruAmount")]
    public Common MaxPassthruAmount
    {
      get => maxPassthruAmount ??= new();
      set => maxPassthruAmount = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public MonthlyObligeeSummary Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DisbursementTransaction total;
    private DisbursementTransaction disbursementTransaction;
    private MonthlyObligeeSummary alertToCase;
    private Infrastructure infrastructure;
    private Common countPassthru;
    private Common generateNoAlert;
    private DateWorkArea zero;
    private DateWorkArea passthruMonthEndDate;
    private Common releasedOrSuppressed;
    private DisbursementTransaction newDebitPassthru;
    private DisbSuppressionStatusHistory hardcodedPerson;
    private Common suppressPersonDisb;
    private MonthlyObligeeSummary changes;
    private DateWorkArea current;
    private Common retry;
    private Common dummy;
    private DateWorkArea passthruMonthStartDate;
    private DisbursementTransaction newPassthruCredit;
    private DisbursementTransaction addlPassthruPayableCr;
    private Common maxPassthruAmount;
    private MonthlyObligeeSummary existing;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of TotalDebit.
    /// </summary>
    [JsonPropertyName("totalDebit")]
    public DisbursementTransaction TotalDebit
    {
      get => totalDebit ??= new();
      set => totalDebit = value;
    }

    /// <summary>
    /// A value of ObligeeForReread.
    /// </summary>
    [JsonPropertyName("obligeeForReread")]
    public CsePersonAccount ObligeeForReread
    {
      get => obligeeForReread ??= new();
      set => obligeeForReread = value;
    }

    /// <summary>
    /// A value of Passthru.
    /// </summary>
    [JsonPropertyName("passthru")]
    public DisbursementTransaction Passthru
    {
      get => passthru ??= new();
      set => passthru = value;
    }

    /// <summary>
    /// A value of MaximumPassthru.
    /// </summary>
    [JsonPropertyName("maximumPassthru")]
    public MaximumPassthru MaximumPassthru
    {
      get => maximumPassthru ??= new();
      set => maximumPassthru = value;
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

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DisbursementTransactionRln disbursementTransactionRln;
    private Collection collection;
    private DisbursementTransaction credit;
    private DisbursementType disbursementType;
    private DisbursementTransaction totalDebit;
    private CsePersonAccount obligeeForReread;
    private DisbursementTransaction passthru;
    private MaximumPassthru maximumPassthru;
    private CsePersonAccount obligee1;
    private CsePerson obligee2;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }
#endregion
}
