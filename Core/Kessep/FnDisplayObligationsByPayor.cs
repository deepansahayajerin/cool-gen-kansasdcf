// Program: FN_DISPLAY_OBLIGATIONS_BY_PAYOR, ID: 371948409, model: 746.
// Short name: SWE00446
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
/// A program: FN_DISPLAY_OBLIGATIONS_BY_PAYOR.
/// </para>
/// <para>
/// RESP: FINANCE
/// This cab is used to create list of data to be displayed by prad list 
/// obligations by payor.
/// </para>
/// </summary>
[Serializable]
public partial class FnDisplayObligationsByPayor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISPLAY_OBLIGATIONS_BY_PAYOR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisplayObligationsByPayor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisplayObligationsByPayor.
  /// </summary>
  public FnDisplayObligationsByPayor(IContext context, Import import,
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
    // *****
    // Cab List obligations by ap/payor
    // Developed by MTW Consulting for KESSEP
    // D.M. Nilsen 07/21/95
    // Change Log
    // DEVELOPER          DATE     DESCRIPTION
    // Skip Hardy MTW   11/05/97   Added logic to determine if an Obligation
    // 			    has an Obligation Transaction with an open
    // 			    Designated Payee.
    // Adwait Phadnis           Problem log no. 31379
    // A Samuels	02/26/98	PR #38007,31424
    // E. Parker	2/18/1999  Corrected logic to display Mnthly Due on Active 
    // Secondary Obligations.
    // E. Parker  6/19/1999  Changed logic to use fn_read_case_no_and_worker_id.
    // *****
    // =================================================
    // : 03/30/1999  A Doty    Modified the logic to support the calculation of 
    // summary totals real-time.  Removed the use of the Summary Entities.
    // 7/6/99 - Bud Adams  -  eliminated persistent obligation view.
    //   added unnecessary overhead; no technical requirement for
    //   it.
    //   Read properties set
    // =================================================
    local.Current.Date = Now().Date;

    // : Initialize Hardcoded Values
    UseFnHardcodedDebtDistribution();
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.YearMonth = UseCabGetYearMonthFromDate();
    local.FirstDayOfCurrentMonth.Date =
      AddDays(AddDays(local.Current.Date, -Day(local.Current.Date)), 1);
    local.FirstDayOfNextMonth.Date = AddMonths(local.Current.Date, 1);
    local.FirstDayOfNextMonth.Date =
      AddDays(AddDays(
        local.FirstDayOfNextMonth.Date, -
      Day(local.FirstDayOfNextMonth.Date)), 1);
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    if (ReadCsePerson())
    {
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      ExitState = "FN0000_AP_PAYOR_NF";

      return;
    }

    local.OmitCrdInd.Flag = "Y";
    local.OmitUndistAmtInd.Flag = "Y";
    local.OmitUnprocTrnCheckInd.Flag = "Y";
    export.Export1.Index = -1;

    foreach(var item in ReadObligation())
    {
      export.Obligation.SystemGeneratedIdentifier =
        entities.Obligation.SystemGeneratedIdentifier;
      ReadObligationType();

      // =================================================
      // 7/6/99 - bud adams  -  removed persistent read of obligation
      // =================================================
      // ************************************************
      // *Determine if this Obligation is               *
      // *Inactive - Obligation has an Open Balance but *
      // *           the Accrual has been discontinued. *
      // *Active   - Balance and Accrual Instructions   *
      // *Deactivated - No Balance and No Accrual       *
      // *Instructions.
      // 
      // *
      // ************************************************
      UseFnGetObligationStatus();
      local.DebtDetailStatusHistory.Code =
        local.ScreenObligationStatus.ObligationStatus;

      // : Check to see if inactive obligations are to be displayed
      if (AsChar(import.ShowInactive.SelectChar) != 'Y' && AsChar
        (local.DebtDetailStatusHistory.Code) == AsChar
        (local.DeactiveStatus.Code))
      {
        // : SKIP inactive obligations
        continue;
      }

      ++export.Export1.Index;
      export.Export1.CheckSize();

      MoveObligation(entities.Obligation, export.Export1.Update.DetailObligation);
        
      export.Export1.Update.DetailDebtDetailStatusHistory.Code =
        local.DebtDetailStatusHistory.Code;
      MoveObligationPaymentSchedule(local.Local1.
        DetailObligationPaymentSchedule,
        export.Export1.Update.DetailObligationPaymentSchedule);
      export.Export1.Update.DetailObligationType.
        Assign(entities.ObligationType);
      export.Export1.Update.DetailAcNonAc.SelectChar =
        entities.ObligationType.Classification;

      if (ReadLegalAction())
      {
        export.Export1.Update.DetailLegalAction.Assign(entities.LegalAction);
      }
      else
      {
        // : Ok, relationship is not mandatory
      }

      export.Export1.Update.GexportPriSecAndIntrstInd.State =
        (export.Export1.Item.DetailObligation.PrimarySecondaryCode ?? "") + entities
        .Obligation.OrderTypeCode;

      // ---------------------------------------------
      // Read Manual Distribution Instructions and  set the indicator 
      // accordingly
      // ---------------------------------------------
      if (ReadManualDistributionAudit())
      {
        local.ManualDistInstExists.OneChar = "M";
      }
      else
      {
        local.ManualDistInstExists.OneChar = "";
      }

      if (ReadObligCollProtectionHist())
      {
        local.ObCollProtActiveInd.Text1 = "P";
      }
      else
      {
        local.ObCollProtActiveInd.Text1 = "";
      }

      export.Export1.Update.DetailConcatInds.Text8 =
        export.Export1.Item.DetailAcNonAc.SelectChar + export
        .Export1.Item.DetailDebtDetailStatusHistory.Code + entities
        .Obligation.PrimarySecondaryCode + entities.Obligation.OrderTypeCode + local
        .ManualDistInstExists.OneChar + local.ObCollProtActiveInd.Text1;

      // Oct 25, 1999, PR# 77622, M Brown - Remove logic for service provider -
      // it is no longer displayed.
      UseFnComputeSummaryTotals1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      // ---------------------------------------------
      // Move the following back to IF ..THEN
      // : Put screen amounts into detail lines
      export.Export1.Update.DetailDark.Flag = "";
      export.Export1.Update.DetailArrearsOwed.TotalCurrency =
        local.ScreenOwedAmounts1.ArrearsAmountOwed;
      export.Export1.Update.DetailCurrentOwed.TotalCurrency =
        local.ScreenOwedAmounts1.CurrentAmountOwed;
      export.Export1.Update.DetailIntrestDue.TotalCurrency =
        local.ScreenOwedAmounts1.InterestAmountOwed;
      export.Export1.Update.DetailTotalDue.TotalCurrency =
        local.ScreenOwedAmounts1.TotalAmountOwed;
      UseFnCalcAmtsDueForObligation();

      if (AsChar(entities.Obligation.PrimarySecondaryCode) != 'S')
      {
        local.TotalPeriodicDue.TotalCurrency =
          local.ScreenDueAmounts.PeriodicAmountDue;
        local.TotalCurrentSupportDue.TotalCurrency =
          local.ScreenDueAmounts.CurrentAmountDue;
        local.TotalMothlyDue.TotalCurrency =
          local.ScreenDueAmounts.TotalAmountDue;
        export.Export1.Update.DetailMonthlyDue.TotalCurrency =
          local.TotalMothlyDue.TotalCurrency;
        export.PeriodicAmountDue.TotalCurrency += local.TotalPeriodicDue.
          TotalCurrency;
        export.CurrentSupportDue.TotalCurrency += local.TotalCurrentSupportDue.
          TotalCurrency;

        // ---------------------------------------------
        // Deleted SET stmt
        // SET export_total_monthly_due ief_supplied total_currency TO 
        // export_total_monthly_due ief_supplied total_currency +
        // local_total_mothly_due ief_supplied total_currency
        // -----------------------------------------------
      }
      else
      {
        local.TotalMothlyDue.TotalCurrency =
          local.ScreenDueAmounts.TotalAmountDue;
        export.Export1.Update.DetailMonthlyDue.TotalCurrency =
          local.TotalMothlyDue.TotalCurrency;
      }

      if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
      {
        break;
      }
    }

    export.TotalMonthlyDue.TotalCurrency =
      export.CurrentSupportDue.TotalCurrency + export
      .PeriodicAmountDue.TotalCurrency;
    UseFnComputeSummaryTotals2();
    export.CurrentSupportOwed.TotalCurrency =
      local.ScreenOwedAmounts1.CurrentAmountOwed;
    export.TotalArrearsOwed.TotalCurrency =
      local.ScreenOwedAmounts1.ArrearsAmountOwed;
    export.TotalIntrestOwed.TotalCurrency =
      local.ScreenOwedAmounts1.InterestAmountOwed;
    export.TotalMonthlyOwed.TotalCurrency =
      local.ScreenOwedAmounts1.TotalAmountOwed;
    export.ScreenOwedAmounts.Assign(local.ScreenOwedAmounts1);
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveScreenObligationStatus(ScreenObligationStatus source,
    ScreenObligationStatus target)
  {
    target.ObligationStatusTxt = source.ObligationStatusTxt;
    target.ObligationStatus = source.ObligationStatus;
  }

  private int UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Maximum.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCalcAmtsDueForObligation()
  {
    var useImport = new FnCalcAmtsDueForObligation.Import();
    var useExport = new FnCalcAmtsDueForObligation.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;

    Call(FnCalcAmtsDueForObligation.Execute, useImport, useExport);

    local.ScreenDueAmounts.Assign(useExport.ScreenDueAmounts);
  }

  private void UseFnComputeSummaryTotals1()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Filter.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = entities.ObligorCsePerson.Number;
    useImport.OmitUnprocTrnCheckInd.Flag = local.OmitUnprocTrnCheckInd.Flag;
    useImport.OmitUndistAmtInd.Flag = local.OmitUndistAmtInd.Flag;
    useImport.OmitCrdInd.Flag = local.OmitCrdInd.Flag;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    local.ScreenOwedAmounts1.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnComputeSummaryTotals2()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = entities.ObligorCsePerson.Number;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    local.ScreenOwedAmounts1.Assign(useExport.ScreenOwedAmounts);
    export.Undistributed.TotalCurrency = useExport.UndistAmt.TotalCurrency;
  }

  private void UseFnGetObligationStatus()
  {
    var useImport = new FnGetObligationStatus.Import();
    var useExport = new FnGetObligationStatus.Export();

    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = entities.ObligorCsePerson.Number;
    useImport.HcOtCVoluntary.Classification =
      local.HcOtCVoluntary.Classification;
    useImport.Current.Date = local.Current.Date;
    useImport.HcOtCAccruing.Classification =
      local.HardcodeAccruing.Classification;
    useImport.CsePersonAccount.Type1 = local.HcCpaObligor.Type1;

    Call(FnGetObligationStatus.Execute, useImport, useExport);

    MoveScreenObligationStatus(useExport.ScreenObligationStatus,
      local.ScreenObligationStatus);
    MoveObligationPaymentSchedule(useExport.ObligationPaymentSchedule,
      local.Local1.DetailObligationPaymentSchedule);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.NonAccuringClassifica.Classification =
      useExport.OtCNonAccruingClassification.Classification;
    local.DeactiveStatus.Code = useExport.DdshDeactivedStatus.Code;
    local.Debt.Type1 = useExport.OtrnTDebt.Type1;
    local.MonthlyObligorSummary.Type1 = useExport.MosObligation.Type1;
    local.HardcodeAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HcCpaObligor.Type1 = useExport.CpaObligor.Type1;
    MoveObligationTransaction(useExport.OtrnDtDebtDetail, local.DebtDetail);
    MoveObligationTransaction(useExport.OtrnDtAccrualInstructions,
      local.AccrualInstructions);
    local.HcOtCVoluntary.Classification =
      useExport.OtCVoluntaryClassification.Classification;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private bool ReadCsePerson()
  {
    entities.ObligorCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ObligorCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ObligorCsePerson.Type1);
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ManualDistributionAudit.Populated = false;

    return Read("ReadManualDistributionAudit",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ManualDistributionAudit.OtyType = db.GetInt32(reader, 0);
        entities.ManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ManualDistributionAudit.CspNumber = db.GetString(reader, 2);
        entities.ManualDistributionAudit.CpaType = db.GetString(reader, 3);
        entities.ManualDistributionAudit.EffectiveDt = db.GetDate(reader, 4);
        entities.ManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ManualDistributionAudit.CpaType);
      });
  }

  private bool ReadObligCollProtectionHist()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          entities.Obligation.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "otyIdentifier", entities.Obligation.DtyGeneratedId);
          
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetDate(
          command, "deactivationDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 1);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 2);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 3);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 4);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 5);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cpaType", local.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.Description = db.GetNullableString(reader, 5);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 6);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 7);
        entities.Obligation.PreConversionDebtNumber =
          db.GetNullableInt32(reader, 8);
        entities.Obligation.PreConversionCaseNumber =
          db.GetNullableString(reader, 9);
        entities.Obligation.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 10);
        entities.Obligation.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 11);
        entities.Obligation.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 12);
        entities.Obligation.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 13);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 14);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 15);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 16);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 17);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 18);
        entities.Obligation.TillDtCsCollCurrArr =
          db.GetNullableDecimal(reader, 19);
        entities.Obligation.TillDtSpCollCurrArr =
          db.GetNullableDecimal(reader, 20);
        entities.Obligation.TillDtMsCollCurrArr =
          db.GetNullableDecimal(reader, 21);
        entities.Obligation.TillDtNadArrColl =
          db.GetNullableDecimal(reader, 22);
        entities.Obligation.TillDtNadIntColl =
          db.GetNullableDecimal(reader, 23);
        entities.Obligation.TillDtAdcArrColl =
          db.GetNullableDecimal(reader, 24);
        entities.Obligation.TillDtAdcIntColl =
          db.GetNullableDecimal(reader, 25);
        entities.Obligation.AsOfDtTotRecColl =
          db.GetNullableDecimal(reader, 26);
        entities.Obligation.AsOfDtTotRecIntColl =
          db.GetNullableDecimal(reader, 27);
        entities.Obligation.AsOfDtTotFeeColl =
          db.GetNullableDecimal(reader, 28);
        entities.Obligation.AsOfDtTotFeeIntColl =
          db.GetNullableDecimal(reader, 29);
        entities.Obligation.AsOfDtTotCollAll =
          db.GetNullableDecimal(reader, 30);
        entities.Obligation.LastCollAmt = db.GetNullableDecimal(reader, 31);
        entities.Obligation.LastCollDt = db.GetNullableDate(reader, 32);
        entities.Obligation.CreatedBy = db.GetString(reader, 33);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 34);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 35);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 36);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 37);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 3);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ShowInactive.
    /// </summary>
    [JsonPropertyName("showInactive")]
    public Common ShowInactive
    {
      get => showInactive ??= new();
      set => showInactive = value;
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

    private CsePerson csePerson;
    private Common showInactive;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailMonthlyDue.
      /// </summary>
      [JsonPropertyName("detailMonthlyDue")]
      public Common DetailMonthlyDue
      {
        get => detailMonthlyDue ??= new();
        set => detailMonthlyDue = value;
      }

      /// <summary>
      /// A value of DetailDark.
      /// </summary>
      [JsonPropertyName("detailDark")]
      public Common DetailDark
      {
        get => detailDark ??= new();
        set => detailDark = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailAcNonAc.
      /// </summary>
      [JsonPropertyName("detailAcNonAc")]
      public Common DetailAcNonAc
      {
        get => detailAcNonAc ??= new();
        set => detailAcNonAc = value;
      }

      /// <summary>
      /// A value of DetailDebtDetailStatusHistory.
      /// </summary>
      [JsonPropertyName("detailDebtDetailStatusHistory")]
      public DebtDetailStatusHistory DetailDebtDetailStatusHistory
      {
        get => detailDebtDetailStatusHistory ??= new();
        set => detailDebtDetailStatusHistory = value;
      }

      /// <summary>
      /// A value of GexportPriSecAndIntrstInd.
      /// </summary>
      [JsonPropertyName("gexportPriSecAndIntrstInd")]
      public Common GexportPriSecAndIntrstInd
      {
        get => gexportPriSecAndIntrstInd ??= new();
        set => gexportPriSecAndIntrstInd = value;
      }

      /// <summary>
      /// A value of DetailObligationTransaction.
      /// </summary>
      [JsonPropertyName("detailObligationTransaction")]
      public ObligationTransaction DetailObligationTransaction
      {
        get => detailObligationTransaction ??= new();
        set => detailObligationTransaction = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailMultipleSp.
      /// </summary>
      [JsonPropertyName("detailMultipleSp")]
      public Common DetailMultipleSp
      {
        get => detailMultipleSp ??= new();
        set => detailMultipleSp = value;
      }

      /// <summary>
      /// A value of DetailCurrentOwed.
      /// </summary>
      [JsonPropertyName("detailCurrentOwed")]
      public Common DetailCurrentOwed
      {
        get => detailCurrentOwed ??= new();
        set => detailCurrentOwed = value;
      }

      /// <summary>
      /// A value of DetailObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("detailObligationPaymentSchedule")]
      public ObligationPaymentSchedule DetailObligationPaymentSchedule
      {
        get => detailObligationPaymentSchedule ??= new();
        set => detailObligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of DetailArrearsOwed.
      /// </summary>
      [JsonPropertyName("detailArrearsOwed")]
      public Common DetailArrearsOwed
      {
        get => detailArrearsOwed ??= new();
        set => detailArrearsOwed = value;
      }

      /// <summary>
      /// A value of DetailIntrestDue.
      /// </summary>
      [JsonPropertyName("detailIntrestDue")]
      public Common DetailIntrestDue
      {
        get => detailIntrestDue ??= new();
        set => detailIntrestDue = value;
      }

      /// <summary>
      /// A value of DetailTotalDue.
      /// </summary>
      [JsonPropertyName("detailTotalDue")]
      public Common DetailTotalDue
      {
        get => detailTotalDue ??= new();
        set => detailTotalDue = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailConcatInds.
      /// </summary>
      [JsonPropertyName("detailConcatInds")]
      public TextWorkArea DetailConcatInds
      {
        get => detailConcatInds ??= new();
        set => detailConcatInds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private Common detailMonthlyDue;
      private Common detailDark;
      private Common detailCommon;
      private LegalAction detailLegalAction;
      private ObligationType detailObligationType;
      private Common detailAcNonAc;
      private DebtDetailStatusHistory detailDebtDetailStatusHistory;
      private Common gexportPriSecAndIntrstInd;
      private ObligationTransaction detailObligationTransaction;
      private ServiceProvider detailServiceProvider;
      private Common detailMultipleSp;
      private Common detailCurrentOwed;
      private ObligationPaymentSchedule detailObligationPaymentSchedule;
      private Common detailArrearsOwed;
      private Common detailIntrestDue;
      private Common detailTotalDue;
      private Obligation detailObligation;
      private TextWorkArea detailConcatInds;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of TotalMonthlyOwed.
    /// </summary>
    [JsonPropertyName("totalMonthlyOwed")]
    public Common TotalMonthlyOwed
    {
      get => totalMonthlyOwed ??= new();
      set => totalMonthlyOwed = value;
    }

    /// <summary>
    /// A value of CurrentSupportOwed.
    /// </summary>
    [JsonPropertyName("currentSupportOwed")]
    public Common CurrentSupportOwed
    {
      get => currentSupportOwed ??= new();
      set => currentSupportOwed = value;
    }

    /// <summary>
    /// A value of TotalArrearsOwed.
    /// </summary>
    [JsonPropertyName("totalArrearsOwed")]
    public Common TotalArrearsOwed
    {
      get => totalArrearsOwed ??= new();
      set => totalArrearsOwed = value;
    }

    /// <summary>
    /// A value of TotalIntrestOwed.
    /// </summary>
    [JsonPropertyName("totalIntrestOwed")]
    public Common TotalIntrestOwed
    {
      get => totalIntrestOwed ??= new();
      set => totalIntrestOwed = value;
    }

    /// <summary>
    /// A value of CurrentSupportDue.
    /// </summary>
    [JsonPropertyName("currentSupportDue")]
    public Common CurrentSupportDue
    {
      get => currentSupportDue ??= new();
      set => currentSupportDue = value;
    }

    /// <summary>
    /// A value of PeriodicAmountDue.
    /// </summary>
    [JsonPropertyName("periodicAmountDue")]
    public Common PeriodicAmountDue
    {
      get => periodicAmountDue ??= new();
      set => periodicAmountDue = value;
    }

    /// <summary>
    /// A value of TotalMonthlyDue.
    /// </summary>
    [JsonPropertyName("totalMonthlyDue")]
    public Common TotalMonthlyDue
    {
      get => totalMonthlyDue ??= new();
      set => totalMonthlyDue = value;
    }

    /// <summary>
    /// A value of Undistributed.
    /// </summary>
    [JsonPropertyName("undistributed")]
    public Common Undistributed
    {
      get => undistributed ??= new();
      set => undistributed = value;
    }

    private Obligation obligation;
    private ScreenOwedAmounts screenOwedAmounts;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ExportGroup> export1;
    private Common totalMonthlyOwed;
    private Common currentSupportOwed;
    private Common totalArrearsOwed;
    private Common totalIntrestOwed;
    private Common currentSupportDue;
    private Common periodicAmountDue;
    private Common totalMonthlyDue;
    private Common undistributed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of DetailDark.
      /// </summary>
      [JsonPropertyName("detailDark")]
      public Common DetailDark
      {
        get => detailDark ??= new();
        set => detailDark = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailAcNonAc.
      /// </summary>
      [JsonPropertyName("detailAcNonAc")]
      public Common DetailAcNonAc
      {
        get => detailAcNonAc ??= new();
        set => detailAcNonAc = value;
      }

      /// <summary>
      /// A value of DetailDebtDetailStatusHistory.
      /// </summary>
      [JsonPropertyName("detailDebtDetailStatusHistory")]
      public DebtDetailStatusHistory DetailDebtDetailStatusHistory
      {
        get => detailDebtDetailStatusHistory ??= new();
        set => detailDebtDetailStatusHistory = value;
      }

      /// <summary>
      /// A value of DetailPsIsInd.
      /// </summary>
      [JsonPropertyName("detailPsIsInd")]
      public Common DetailPsIsInd
      {
        get => detailPsIsInd ??= new();
        set => detailPsIsInd = value;
      }

      /// <summary>
      /// A value of DetailObligationTransaction.
      /// </summary>
      [JsonPropertyName("detailObligationTransaction")]
      public ObligationTransaction DetailObligationTransaction
      {
        get => detailObligationTransaction ??= new();
        set => detailObligationTransaction = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailMultipleSp.
      /// </summary>
      [JsonPropertyName("detailMultipleSp")]
      public Common DetailMultipleSp
      {
        get => detailMultipleSp ??= new();
        set => detailMultipleSp = value;
      }

      /// <summary>
      /// A value of DetailCurrentOwed.
      /// </summary>
      [JsonPropertyName("detailCurrentOwed")]
      public Common DetailCurrentOwed
      {
        get => detailCurrentOwed ??= new();
        set => detailCurrentOwed = value;
      }

      /// <summary>
      /// A value of DetailObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("detailObligationPaymentSchedule")]
      public ObligationPaymentSchedule DetailObligationPaymentSchedule
      {
        get => detailObligationPaymentSchedule ??= new();
        set => detailObligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of DetailArrearsOwed.
      /// </summary>
      [JsonPropertyName("detailArrearsOwed")]
      public Common DetailArrearsOwed
      {
        get => detailArrearsOwed ??= new();
        set => detailArrearsOwed = value;
      }

      /// <summary>
      /// A value of DetailInterestDue.
      /// </summary>
      [JsonPropertyName("detailInterestDue")]
      public Common DetailInterestDue
      {
        get => detailInterestDue ??= new();
        set => detailInterestDue = value;
      }

      /// <summary>
      /// A value of DetailTotalDue.
      /// </summary>
      [JsonPropertyName("detailTotalDue")]
      public Common DetailTotalDue
      {
        get => detailTotalDue ??= new();
        set => detailTotalDue = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      private Common detailDark;
      private LegalAction detailLegalAction;
      private ObligationType detailObligationType;
      private Common detailAcNonAc;
      private DebtDetailStatusHistory detailDebtDetailStatusHistory;
      private Common detailPsIsInd;
      private ObligationTransaction detailObligationTransaction;
      private ServiceProvider detailServiceProvider;
      private Common detailMultipleSp;
      private Common detailCurrentOwed;
      private ObligationPaymentSchedule detailObligationPaymentSchedule;
      private Common detailArrearsOwed;
      private Common detailInterestDue;
      private Common detailTotalDue;
      private Obligation detailObligation;
    }

    /// <summary>
    /// A value of HcOtCVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntary")]
    public ObligationType HcOtCVoluntary
    {
      get => hcOtCVoluntary ??= new();
      set => hcOtCVoluntary = value;
    }

    /// <summary>
    /// A value of OmitUnprocTrnCheckInd.
    /// </summary>
    [JsonPropertyName("omitUnprocTrnCheckInd")]
    public Common OmitUnprocTrnCheckInd
    {
      get => omitUnprocTrnCheckInd ??= new();
      set => omitUnprocTrnCheckInd = value;
    }

    /// <summary>
    /// A value of OmitUndistAmtInd.
    /// </summary>
    [JsonPropertyName("omitUndistAmtInd")]
    public Common OmitUndistAmtInd
    {
      get => omitUndistAmtInd ??= new();
      set => omitUndistAmtInd = value;
    }

    /// <summary>
    /// A value of OmitCrdInd.
    /// </summary>
    [JsonPropertyName("omitCrdInd")]
    public Common OmitCrdInd
    {
      get => omitCrdInd ??= new();
      set => omitCrdInd = value;
    }

    /// <summary>
    /// A value of DesigPayeeExists.
    /// </summary>
    [JsonPropertyName("desigPayeeExists")]
    public Standard DesigPayeeExists
    {
      get => desigPayeeExists ??= new();
      set => desigPayeeExists = value;
    }

    /// <summary>
    /// A value of ManualDistInstExists.
    /// </summary>
    [JsonPropertyName("manualDistInstExists")]
    public Standard ManualDistInstExists
    {
      get => manualDistInstExists ??= new();
      set => manualDistInstExists = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonPropertyName("local1")]
    public LocalGroup Local1
    {
      get => local1 ?? (local1 = new());
      set => local1 = value;
    }

    /// <summary>
    /// A value of ScreenObligationStatus.
    /// </summary>
    [JsonPropertyName("screenObligationStatus")]
    public ScreenObligationStatus ScreenObligationStatus
    {
      get => screenObligationStatus ??= new();
      set => screenObligationStatus = value;
    }

    /// <summary>
    /// A value of ScreenDueAmounts.
    /// </summary>
    [JsonPropertyName("screenDueAmounts")]
    public ScreenDueAmounts ScreenDueAmounts
    {
      get => screenDueAmounts ??= new();
      set => screenDueAmounts = value;
    }

    /// <summary>
    /// A value of TotalCurrentSupportDue.
    /// </summary>
    [JsonPropertyName("totalCurrentSupportDue")]
    public Common TotalCurrentSupportDue
    {
      get => totalCurrentSupportDue ??= new();
      set => totalCurrentSupportDue = value;
    }

    /// <summary>
    /// A value of TotalPeriodicDue.
    /// </summary>
    [JsonPropertyName("totalPeriodicDue")]
    public Common TotalPeriodicDue
    {
      get => totalPeriodicDue ??= new();
      set => totalPeriodicDue = value;
    }

    /// <summary>
    /// A value of TotalMothlyDue.
    /// </summary>
    [JsonPropertyName("totalMothlyDue")]
    public Common TotalMothlyDue
    {
      get => totalMothlyDue ??= new();
      set => totalMothlyDue = value;
    }

    /// <summary>
    /// A value of NonAccuringClassifica.
    /// </summary>
    [JsonPropertyName("nonAccuringClassifica")]
    public ObligationType NonAccuringClassifica
    {
      get => nonAccuringClassifica ??= new();
      set => nonAccuringClassifica = value;
    }

    /// <summary>
    /// A value of DeactiveStatus.
    /// </summary>
    [JsonPropertyName("deactiveStatus")]
    public DebtDetailStatusHistory DeactiveStatus
    {
      get => deactiveStatus ??= new();
      set => deactiveStatus = value;
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
    /// A value of HardcodeActive.
    /// </summary>
    [JsonPropertyName("hardcodeActive")]
    public DebtDetailStatusHistory HardcodeActive
    {
      get => hardcodeActive ??= new();
      set => hardcodeActive = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts1.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts1")]
    public ScreenOwedAmounts ScreenOwedAmounts1
    {
      get => screenOwedAmounts1 ??= new();
      set => screenOwedAmounts1 = value;
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
    /// A value of MonthlyObligorSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligorSummary")]
    public MonthlyObligorSummary MonthlyObligorSummary
    {
      get => monthlyObligorSummary ??= new();
      set => monthlyObligorSummary = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of FirstDayOfNextMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfNextMonth")]
    public DateWorkArea FirstDayOfNextMonth
    {
      get => firstDayOfNextMonth ??= new();
      set => firstDayOfNextMonth = value;
    }

    /// <summary>
    /// A value of FirstDayOfCurrentMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfCurrentMonth")]
    public DateWorkArea FirstDayOfCurrentMonth
    {
      get => firstDayOfCurrentMonth ??= new();
      set => firstDayOfCurrentMonth = value;
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
    /// A value of HardcodeAccruing.
    /// </summary>
    [JsonPropertyName("hardcodeAccruing")]
    public ObligationType HardcodeAccruing
    {
      get => hardcodeAccruing ??= new();
      set => hardcodeAccruing = value;
    }

    /// <summary>
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public ObligationTransaction DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of ScreenOwedAmounts2.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts2")]
    public ScreenOwedAmounts ScreenOwedAmounts2
    {
      get => screenOwedAmounts2 ??= new();
      set => screenOwedAmounts2 = value;
    }

    /// <summary>
    /// A value of ObCollProtActiveInd.
    /// </summary>
    [JsonPropertyName("obCollProtActiveInd")]
    public TextWorkArea ObCollProtActiveInd
    {
      get => obCollProtActiveInd ??= new();
      set => obCollProtActiveInd = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private ObligationType hcOtCVoluntary;
    private Common omitUnprocTrnCheckInd;
    private Common omitUndistAmtInd;
    private Common omitCrdInd;
    private Standard desigPayeeExists;
    private Standard manualDistInstExists;
    private LocalGroup local1;
    private ScreenObligationStatus screenObligationStatus;
    private ScreenDueAmounts screenDueAmounts;
    private Common totalCurrentSupportDue;
    private Common totalPeriodicDue;
    private Common totalMothlyDue;
    private ObligationType nonAccuringClassifica;
    private DebtDetailStatusHistory deactiveStatus;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private DebtDetailStatusHistory hardcodeActive;
    private ScreenOwedAmounts screenOwedAmounts1;
    private ObligationTransaction debt;
    private MonthlyObligorSummary monthlyObligorSummary;
    private AbendData abendData;
    private DateWorkArea current;
    private DateWorkArea firstDayOfNextMonth;
    private DateWorkArea firstDayOfCurrentMonth;
    private DateWorkArea maximum;
    private ObligationType hardcodeAccruing;
    private CsePersonAccount hcCpaObligor;
    private ObligationTransaction debtDetail;
    private ObligationTransaction accrualInstructions;
    private ScreenOwedAmounts screenOwedAmounts2;
    private TextWorkArea obCollProtActiveInd;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("manualDistributionAudit")]
    public ManualDistributionAudit ManualDistributionAudit
    {
      get => manualDistributionAudit ??= new();
      set => manualDistributionAudit = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    private CsePersonAccount obligorCsePersonAccount;
    private ManualDistributionAudit manualDistributionAudit;
    private Tribunal tribunal;
    private Fips fips;
    private LegalActionDetail legalActionDetail;
    private ObligationTransaction obligationTransaction;
    private LegalAction legalAction;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson obligorCsePerson;
    private ObligCollProtectionHist obligCollProtectionHist;
  }
#endregion
}
