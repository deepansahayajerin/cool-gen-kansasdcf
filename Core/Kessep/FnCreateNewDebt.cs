// Program: FN_CREATE_NEW_DEBT, ID: 371968288, model: 746.
// Short name: SWE00375
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_NEW_DEBT.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process will create a debt for a obligation during the accrual process.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateNewDebt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_NEW_DEBT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateNewDebt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateNewDebt.
  /// </summary>
  public FnCreateNewDebt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ===================================================================================
    // 12/21/99 - b adams  -  PR# 83302: removed unused attributes
    //   Preconversion_ISN & Preconversion_Receipt_Number
    //   from Obligation_Transaction (debt) view so they can be
    //   deleted from the model.
    // 01/11/2006 GVandy  PR267430  Set DEBT new_debt_process_date so that retro
    // processing is bypassed if the debt_detail due_date >= processing date + 1
    // month.
    // 07/19/2010 GVandy CQ14223 Use processing date of previous run to 
    // determine if
    // retro processing is necessary for newly created debts.
    // ===================================================================================
    // ***** HARDCODE AREA *****
    UseFnHardcodedDebtDistribution();

    // *****  Ensure that all of the import persistent views are populated.
    if (ReadObligation())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    if (ReadSupported())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "FN0000_SUPP_PERSON_ACCT_NF";

      return;
    }

    if (ReadDebt())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "FN0229_DEBT_NF";

      return;
    }

    if (ReadObligationTransactionRlnRsn())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF";

      return;
    }

    if (ReadLegalActionPerson())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "FN0000_LEGAL_ACT_PERSON_NF";

      return;
    }

    if (Equal(import.Current.Timestamp, local.Current.Timestamp))
    {
      local.Current.Timestamp = Now();
    }
    else
    {
      local.Current.Timestamp = import.Current.Timestamp;
    }

    // ***** MAIN-LINE AREA *****
    // 07/19/2010 GVandy CQ14223 Use processing date of previous run to 
    // determine if
    // retro processing is necessary for newly created debts.
    if (Lt(AddMonths(import.PreviousProcessingDate.Date, 1),
      import.DebtDetail.DueDt))
    {
      local.Debt.NewDebtProcessDate = import.ProgramProcessingInfo.ProcessDate;
    }

    for(local.Loop.Count = 1; local.Loop.Count <= 5; ++local.Loop.Count)
    {
      try
      {
        CreateDebt();
        ++export.ImportNumberOfUpdates.Count;
        MoveObligationTransaction2(entities.New1, export.Debt);

        break;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            if (local.Loop.Count < 5)
            {
              continue;
            }
            else
            {
              ExitState = "FN0228_DEBT_AE";

              return;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0231_DEBT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    try
    {
      CreateDebtDetail();
      ++export.ImportNumberOfUpdates.Count;
      MoveDebtDetail(entities.DebtDetail, export.DebtDetail);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0209_DEBT_DETAIL_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0217_DEBT_DETAIL_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    for(local.Loop.Count = 1; local.Loop.Count <= 5; ++local.Loop.Count)
    {
      // ****MFB - removed associate to Debt Detail and
      // **** Debt Detail Oblig. Txn
      try
      {
        CreateDebtDetailStatusHistory();
        ++export.ImportNumberOfUpdates.Count;
        export.DebtDetailStatusHistory.Assign(entities.DebtDetailStatusHistory);

        break;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            if (local.Loop.Count < 5)
            {
              continue;
            }
            else
            {
              ExitState = "FN0220_DEBT_DETL_STAT_HIST_AE";

              return;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0226_DEBT_DETL_STAT_HIST_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    for(local.Loop.Count = 1; local.Loop.Count <= 5; ++local.Loop.Count)
    {
      try
      {
        CreateObligationTransactionRln();
        ++export.ImportNumberOfUpdates.Count;
        MoveObligationTransactionRln(entities.ObligationTransactionRln,
          export.ObligationTransactionRln);

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            if (local.Loop.Count < 10)
            {
              continue;
            }
            else
            {
              ExitState = "FN0000_OBLIG_TRANS_RLN_AE";

              return;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIG_TRANS_RLN_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.AdcDt = source.AdcDt;
    target.CreatedTmst = source.CreatedTmst;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveObligationTransaction1(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.DebtType = source.DebtType;
    target.DebtAdjustmentInd = source.DebtAdjustmentInd;
    target.NewDebtProcessDate = source.NewDebtProcessDate;
  }

  private static void MoveObligationTransactionRln(
    ObligationTransactionRln source, ObligationTransactionRln target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
  }

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    MoveObligationTransaction1(useExport.OtrnDtDebtDetail,
      local.HardcodeDebtDetail);
    local.HardcodeDebt.Type1 = useExport.OtrnTDebt.Type1;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateDebt()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = local.HardcodeDebt.Type1;
    var amount = import.Debt.Amount;
    var debtAdjustmentInd = "N";
    var createdBy = global.UserId;
    var createdTmst = local.Current.Timestamp;
    var debtType = local.HardcodeDebtDetail.DebtType;
    var cspSupNumber = entities.Supported.CspNumber;
    var cpaSupType = entities.Supported.Type1;
    var otyType = entities.Obligation.DtyGeneratedId;
    var lapId = entities.LegalActionPerson.Identifier;
    var newDebtProcessDate = local.Debt.NewDebtProcessDate;

    CheckValid<ObligationTransaction>("CpaType", cpaType);
    CheckValid<ObligationTransaction>("Type1", type1);
    CheckValid<ObligationTransaction>("DebtAdjustmentInd", debtAdjustmentInd);
    CheckValid<ObligationTransaction>("DebtType", debtType);
    CheckValid<ObligationTransaction>("CpaSupType", cpaSupType);
    entities.New1.Populated = false;
    Update("CreateDebt",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obTrnId", systemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", type1);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetString(command, "debtAdjInd", debtAdjustmentInd);
        db.SetString(
          command, "debtAdjTyp", GetImplicitValue<ObligationTransaction,
          string>("DebtAdjustmentType"));
        db.SetDate(command, "debAdjDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "debtTyp", debtType);
        db.SetNullableInt32(command, "volPctAmt", 0);
        db.SetNullableInt32(command, "zdelPrecnvRcptN", 0);
        db.SetNullableInt32(command, "zdelPrecnvrsnIsn", 0);
        db.SetNullableString(command, "cspSupNumber", cspSupNumber);
        db.SetNullableString(command, "cpaSupType", cpaSupType);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "daCaProcReqInd", "");
        db.SetString(command, "rsnCd", "");
        db.SetNullableInt32(command, "lapId", lapId);
        db.SetNullableDate(command, "newDebtProcDt", newDebtProcessDate);
      });

    entities.New1.ObgGeneratedId = obgGeneratedId;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.Type1 = type1;
    entities.New1.Amount = amount;
    entities.New1.DebtAdjustmentInd = debtAdjustmentInd;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTmst = createdTmst;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTmst = null;
    entities.New1.DebtType = debtType;
    entities.New1.VoluntaryPercentageAmount = 0;
    entities.New1.CspSupNumber = cspSupNumber;
    entities.New1.CpaSupType = cpaSupType;
    entities.New1.OtyType = otyType;
    entities.New1.LapId = lapId;
    entities.New1.NewDebtProcessDate = newDebtProcessDate;
    entities.New1.Populated = true;
  }

  private void CreateDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.New1.Populated);

    var obgGeneratedId = entities.New1.ObgGeneratedId;
    var cspNumber = entities.New1.CspNumber;
    var cpaType = entities.New1.CpaType;
    var otrGeneratedId = entities.New1.SystemGeneratedIdentifier;
    var otyType = entities.New1.OtyType;
    var otrType = entities.New1.Type1;
    var dueDt = import.DebtDetail.DueDt;
    var balanceDueAmt = import.Debt.Amount;
    var interestBalanceDueAmt =
      import.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
    var createdTmst = local.Current.Timestamp;
    var createdBy = global.UserId;

    CheckValid<DebtDetail>("CpaType", cpaType);
    CheckValid<DebtDetail>("OtrType", otrType);
    entities.DebtDetail.Populated = false;
    Update("CreateDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otrGeneratedId", otrGeneratedId);
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "otrType", otrType);
        db.SetDate(command, "dueDt", dueDt);
        db.SetDecimal(command, "balDueAmt", balanceDueAmt);
        db.SetNullableDecimal(command, "intBalDueAmt", interestBalanceDueAmt);
        db.SetNullableDate(command, "adcDt", null);
        db.SetNullableDate(command, "retiredDt", null);
        db.SetNullableDate(command, "cvrdPrdStartDt", null);
        db.SetNullableDate(command, "cvdPrdEndDt", null);
        db.SetNullableString(command, "precnvrsnPgmCd", "");
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "lastUpdatedBy", "");
      });

    entities.DebtDetail.ObgGeneratedId = obgGeneratedId;
    entities.DebtDetail.CspNumber = cspNumber;
    entities.DebtDetail.CpaType = cpaType;
    entities.DebtDetail.OtrGeneratedId = otrGeneratedId;
    entities.DebtDetail.OtyType = otyType;
    entities.DebtDetail.OtrType = otrType;
    entities.DebtDetail.DueDt = dueDt;
    entities.DebtDetail.BalanceDueAmt = balanceDueAmt;
    entities.DebtDetail.InterestBalanceDueAmt = interestBalanceDueAmt;
    entities.DebtDetail.AdcDt = null;
    entities.DebtDetail.RetiredDt = null;
    entities.DebtDetail.CoveredPrdStartDt = null;
    entities.DebtDetail.CoveredPrdEndDt = null;
    entities.DebtDetail.PreconversionProgramCode = "";
    entities.DebtDetail.CreatedTmst = createdTmst;
    entities.DebtDetail.CreatedBy = createdBy;
    entities.DebtDetail.Populated = true;
  }

  private void CreateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDt = import.DebtDetail.DueDt;
    var discontinueDt = UseCabSetMaximumDiscontinueDate();
    var createdBy = global.UserId;
    var createdTmst = local.Current.Timestamp;
    var otrType = entities.DebtDetail.OtrType;
    var otrId = entities.DebtDetail.OtrGeneratedId;
    var cpaType = entities.DebtDetail.CpaType;
    var cspNumber = entities.DebtDetail.CspNumber;
    var obgId = entities.DebtDetail.ObgGeneratedId;
    var code = import.DebtDetailStatusHistory.Code;
    var otyType = entities.DebtDetail.OtyType;
    var reasonTxt = import.DebtDetailStatusHistory.ReasonTxt ?? "";

    CheckValid<DebtDetailStatusHistory>("OtrType", otrType);
    CheckValid<DebtDetailStatusHistory>("CpaType", cpaType);
    entities.DebtDetailStatusHistory.Populated = false;
    Update("CreateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "obTrnStatHstId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrId", otrId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "obTrnStCd", code);
        db.SetInt32(command, "otyType", otyType);
        db.SetNullableString(command, "rsnTxt", reasonTxt);
      });

    entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtDetailStatusHistory.EffectiveDt = effectiveDt;
    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.CreatedBy = createdBy;
    entities.DebtDetailStatusHistory.CreatedTmst = createdTmst;
    entities.DebtDetailStatusHistory.OtrType = otrType;
    entities.DebtDetailStatusHistory.OtrId = otrId;
    entities.DebtDetailStatusHistory.CpaType = cpaType;
    entities.DebtDetailStatusHistory.CspNumber = cspNumber;
    entities.DebtDetailStatusHistory.ObgId = obgId;
    entities.DebtDetailStatusHistory.Code = code;
    entities.DebtDetailStatusHistory.OtyType = otyType;
    entities.DebtDetailStatusHistory.ReasonTxt = reasonTxt;
    entities.DebtDetailStatusHistory.Populated = true;
  }

  private void CreateObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    System.Diagnostics.Debug.Assert(entities.New1.Populated);

    var onrGeneratedId =
      entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    var otrType = entities.New1.Type1;
    var otrGeneratedId = entities.New1.SystemGeneratedIdentifier;
    var cpaType = entities.New1.CpaType;
    var cspNumber = entities.New1.CspNumber;
    var obgGeneratedId = entities.New1.ObgGeneratedId;
    var otrPType = entities.AccrualInstructions.Type1;
    var otrPGeneratedId =
      entities.AccrualInstructions.SystemGeneratedIdentifier;
    var cpaPType = entities.AccrualInstructions.CpaType;
    var cspPNumber = entities.AccrualInstructions.CspNumber;
    var obgPGeneratedId = entities.AccrualInstructions.ObgGeneratedId;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var createdBy = global.UserId;
    var createdTmst = local.Current.Timestamp;
    var otyTypePrimary = entities.AccrualInstructions.OtyType;
    var otyTypeSecondary = entities.New1.OtyType;

    CheckValid<ObligationTransactionRln>("OtrType", otrType);
    CheckValid<ObligationTransactionRln>("CpaType", cpaType);
    CheckValid<ObligationTransactionRln>("OtrPType", otrPType);
    CheckValid<ObligationTransactionRln>("CpaPType", cpaPType);
    entities.ObligationTransactionRln.Populated = false;
    Update("CreateObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "onrGeneratedId", onrGeneratedId);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrGeneratedId", otrGeneratedId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "otrPType", otrPType);
        db.SetInt32(command, "otrPGeneratedId", otrPGeneratedId);
        db.SetString(command, "cpaPType", cpaPType);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetInt32(command, "obgPGeneratedId", obgPGeneratedId);
        db.SetInt32(command, "obTrnRlnId", systemGeneratedIdentifier);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "otyTypePrimary", otyTypePrimary);
        db.SetInt32(command, "otyTypeSecondary", otyTypeSecondary);
        db.SetNullableString(command, "obTrnRlnDsc", "");
      });

    entities.ObligationTransactionRln.OnrGeneratedId = onrGeneratedId;
    entities.ObligationTransactionRln.OtrType = otrType;
    entities.ObligationTransactionRln.OtrGeneratedId = otrGeneratedId;
    entities.ObligationTransactionRln.CpaType = cpaType;
    entities.ObligationTransactionRln.CspNumber = cspNumber;
    entities.ObligationTransactionRln.ObgGeneratedId = obgGeneratedId;
    entities.ObligationTransactionRln.OtrPType = otrPType;
    entities.ObligationTransactionRln.OtrPGeneratedId = otrPGeneratedId;
    entities.ObligationTransactionRln.CpaPType = cpaPType;
    entities.ObligationTransactionRln.CspPNumber = cspPNumber;
    entities.ObligationTransactionRln.ObgPGeneratedId = obgPGeneratedId;
    entities.ObligationTransactionRln.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.ObligationTransactionRln.CreatedBy = createdBy;
    entities.ObligationTransactionRln.CreatedTmst = createdTmst;
    entities.ObligationTransactionRln.OtyTypePrimary = otyTypePrimary;
    entities.ObligationTransactionRln.OtyTypeSecondary = otyTypeSecondary;
    entities.ObligationTransactionRln.Populated = true;
  }

  private bool ReadDebt()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadDebt",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnId",
          import.AccrualInstructions.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 1);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 2);
        entities.AccrualInstructions.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.AccrualInstructions.Type1 = db.GetString(reader, 4);
        entities.AccrualInstructions.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.AccrualInstructions.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.AccrualInstructions.OtyType = db.GetInt32(reader, 7);
        entities.AccrualInstructions.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.AccrualInstructions.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.AccrualInstructions.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.AccrualInstructions.CpaSupType);
      });
  }

  private bool ReadLegalActionPerson()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "laPersonId", import.LegalActionPerson.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Populated = true;
      });
  }

  private bool ReadSupported()
  {
    entities.Supported.Populated = false;

    return Read("ReadSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public ObligationTransaction AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of PreviousProcessingDate.
    /// </summary>
    [JsonPropertyName("previousProcessingDate")]
    public DateWorkArea PreviousProcessingDate
    {
      get => previousProcessingDate ??= new();
      set => previousProcessingDate = value;
    }

    private LegalActionPerson legalActionPerson;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson obligor;
    private CsePerson supported;
    private ObligationTransaction accrualInstructions;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea previousProcessingDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private ObligationTransactionRln obligationTransactionRln;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
    }

    /// <summary>
    /// A value of HardcodeDebtDetail.
    /// </summary>
    [JsonPropertyName("hardcodeDebtDetail")]
    public ObligationTransaction HardcodeDebtDetail
    {
      get => hardcodeDebtDetail ??= new();
      set => hardcodeDebtDetail = value;
    }

    /// <summary>
    /// A value of HardcodeDebt.
    /// </summary>
    [JsonPropertyName("hardcodeDebt")]
    public ObligationTransaction HardcodeDebt
    {
      get => hardcodeDebt ??= new();
      set => hardcodeDebt = value;
    }

    private ObligationTransaction debt;
    private DateWorkArea current;
    private Common loop;
    private ObligationTransaction hardcodeDebtDetail;
    private ObligationTransaction hardcodeDebt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public ObligationTransaction AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ObligationTransaction New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    private LegalActionPerson legalActionPerson;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransaction accrualInstructions;
    private CsePersonAccount supported;
    private Obligation obligation;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationType obligationType;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private ObligationTransaction new1;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
  }
#endregion
}
