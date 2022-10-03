// Program: FN_READ_RECOVERY_OBLIGATION, ID: 372159457, model: 746.
// Short name: SWE00583
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
/// A program: FN_READ_RECOVERY_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This CAB will read the CSE Person, Obligation, Obligation Transaction (&amp;
/// related entities) and all of the support CSE Person's associated to the
/// Obligation.
/// Required Import Views:
/// 	CSE Person Number
/// 	Obligation Sys Gen Id
/// 	Obligation Transaction Sys Gen ID
/// </para>
/// </summary>
[Serializable]
public partial class FnReadRecoveryObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_RECOVERY_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadRecoveryObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadRecoveryObligation.
  /// </summary>
  public FnReadRecoveryObligation(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************
    // ***--- Sumanta - MTW - 05/01/97
    // ***--- Changed the use of DB2 current date to IEF current date..
    // ***--- Paul R. Egger - MTW - 07/28/97
    // ****---- If only ONE recovery obligation exists, read it.  If multiple 
    // exist, say that.
    // 3/23/98		Siraj Konkader			ZDEL cleanup
    // ************************************************************
    // =================================================
    // 9-3-98  B Adams	Deleted fn-hardcode-debt-distribution & imported
    // 		values
    // 		Combined 2 identical IF constructs.
    // 12-31-98 - b adams  -  READ properties set
    // 6/10/99 - b adams  -  added logic to determine if an obligation is 
    // deactivated
    // 11/4/99 - b adams  -  PR# 78500: see note below.
    // =================================================
    // ***** HARDCODE AREA *****
    export.ManualDistributionInd.Flag = "N";
    export.PaymentScheduleInd.Flag = "N";
    export.InterestSuspensionInd.Flag = "N";

    // ***** MAIN-LINE AREA *****
    if (ReadObligationType1())
    {
      export.ObligationType.Assign(entities.ObligationType);
    }
    else
    {
      if (AsChar(import.ObligationType.Classification) == AsChar
        (import.HcOtcFee.Classification))
      {
        ExitState = "FN0300_NO_FEE_OBLIGATIONS_EXIST";
      }
      else
      {
        ExitState = "FN0301_NO_RECOVERY_OBLIGATIONS";
      }

      if (import.Obligation.SystemGeneratedIdentifier == 0)
      {
        // Set potential bad exit state, but don't escape yet.
      }
      else
      {
        ExitState = "FN0000_OBLIG_TYPE_NF";

        return;
      }
    }

    if (ReadCsePersonCsePersonAccount())
    {
      if (import.Obligation.SystemGeneratedIdentifier == 0)
      {
        local.NumberOfObligations.Count = 0;

        foreach(var item in ReadObligationObligationType())
        {
          // =================================================
          // 6/10/99 - bud adams  -  ONLY consider obligations that are
          //   Active.  If Deactive, treat them as if they don't exist for the
          //   Display function.
          // =================================================
          UseFnGetObligationStatus();

          if (AsChar(local.ScreenObligationStatus.ObligationStatus) == 'D')
          {
            // =================================================
            // 1/28/00 - b adams  -  PR# 83295: If a second FEE obligation
            //   is about to be added to a Legal_Action_Detail, display an
            //   appropriate message so the user is aware, before they
            //   add the next one.
            // =================================================
            export.DeactiveObligtionExists.Flag = "Y";
          }
          else
          {
            // =================================================
            // 6/24/99 - b adams  -  Moved the SET of local_recover_obligation
            //   s-g-i to here from inside the IF numer = 1.  There, it could be
            //   the number of the discontinued obligation.
            // =================================================
            local.RecoveryObligation.SystemGeneratedIdentifier =
              entities.CountObligation.SystemGeneratedIdentifier;
            ++local.NumberOfObligations.Count;
          }

          if (local.NumberOfObligations.Count > 1)
          {
            ExitState = "FN0000_MULT_RECOVERY_OBLIG";

            return;
          }
        }

        // ***  Combined IF into IF inside the Read Each  ***
        if (local.NumberOfObligations.Count == 1)
        {
          ReadObligation1();

          // ===============================================
          // 10/9/98 - Bud Adams  -  deleted Read of Obligation_Type.
          // It had already been read earlier, and the above Read
          // confirms it is the one related to this Obligation.
          // ===============================================
        }

        if (local.NumberOfObligations.Count == 0 && !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!entities.ObligationType.Populated)
        {
          if (ReadObligationType2())
          {
            export.ObligationType.Assign(entities.ObligationType);
          }
        }
      }
      else
      {
        local.RecoveryObligation.SystemGeneratedIdentifier =
          import.Obligation.SystemGeneratedIdentifier;
      }

      if (ReadObligation2())
      {
        export.Obligation.Assign(entities.Obligation);

        // <<< RBM   02/09/98  Read the Interstate Case Information >>>
        if (ReadInterstateRequest())
        {
          export.InterstateRequest.Assign(entities.InterstateRequest);
          local.Code.CodeName = "COUNTRY CODE";
          local.CodeValue.Cdvalue = export.InterstateRequest.Country ?? Spaces
            (10);
          UseCabValidateCodeValue();
        }
        else
        {
          export.InterstateRequest.OtherStateCaseId = "";
        }

        // *****  If an assignment exists retrieve that data
        UseFnGetObligationAssignment();

        // =================================================
        // 4/6/99 - bud adams  - Use FN_Check_Obligation_For_Activity
        //   deleted.  Not necessary anymore.
        // =================================================
        ExitState = "ACO_NN0000_ALL_OK";

        // : Check to see if there is an Obligation Payment Schedule.
        if (ReadObligationPaymentSchedule())
        {
          MoveObligationPaymentSchedule(entities.ObligationPaymentSchedule,
            export.ObligationPaymentSchedule);
          export.PaymentScheduleInd.Flag = "Y";
          UseFnSetFrequencyTextField();
        }
        else
        {
          export.PaymentScheduleInd.Flag = "N";

          // =================================================
          // 6/8/99 - bud adams  -  Removed code that Read O_P_S two
          //   more times: first to see if there was a record that would be
          //   in effect in the 'future'; and, if not, to see if there already
          //   had been one that was now 'expired'.  It would then send
          //   that text to be displayed on the screen where frequency
          //   shows up ('future'  'expired').  Nobody could see the value
          //   or reason for such a message.
          // =================================================
        }

        // *** Check if there is Manual Distribution associated with this 
        // Obligation
        if (ReadManualDistributionAudit())
        {
          export.ManualDistributionInd.Flag = "Y";
        }
        else
        {
          export.ManualDistributionInd.Flag = "N";
        }

        // *** Check if there is a current Interest suspension
        if (ReadInterestSuppStatusHistory())
        {
          export.InterestSuspensionInd.Flag = "Y";
        }
        else
        {
          export.InterestSuspensionInd.Flag = "N";
        }

        // : Get the Court Order Number
        // =================================================
        // 10/28/98 - B Adams  -  Determining if an Obligation is interstate
        //   or not is corrected by the addition of Read of FIPS below.
        // 12/31/98 - B Adams  -  This has changed.  Legal is no longer
        //   setting this information.  It will only be set in Debt.  Use
        //   whatever value is stored in Obligation Other_State_Abbr.
        // =================================================
        // =================================================
        // 1/5/00 - bud adams  -  PR# 80445: Read of Legal_Action was
        //   using obsolete relationship between it and Obligation.   And
        //   Legal_Action_Detail is required by FN_Retrieve_Interstate_
        //   Request.
        // =================================================
        if (ReadLegalActionLegalActionDetail())
        {
          export.LegalActionDetail.Number = entities.LegalActionDetail.Number;

          // ***--- Sumanta - MTW - 04/29/97
          // =================================================
          // ***--- Read to FIPS has been deleted. The following read
          //        has been added  -  10/18/98
          // ***---
          // =================================================
          // 12/31/98 - b adams  -  moved alternate_addr read from above
          //   because now it's contingent upon this newly added read
          //   of alternate_addr via legal action.
          //   New Rule:  If it's set by Legal, then it cannot be changed by
          //   finance.  Send value of LE so screen will protect the field
          //   and FN so it will not.
          // =================================================
          if (ReadCsePerson2())
          {
            local.Alt.Number = entities.AlternateAddr.Number;
            local.TextWorkArea.Text10 = local.Alt.Number;
            export.Alternate.Char2 = "LE";
          }
          else if (ReadCsePerson1())
          {
            local.Alt.Number = entities.AlternateAddr.Number;
            local.TextWorkArea.Text10 = local.Alt.Number;
            export.Alternate.Char2 = "FN";
          }
          else
          {
            // ***--- OK to continue ...
          }

          export.OrderClassification.Assign(entities.LegalAction);
        }
        else
        {
          if (ReadCsePerson1())
          {
            local.Alt.Number = entities.AlternateAddr.Number;
            local.TextWorkArea.Text10 = local.Alt.Number;
            export.Alternate.Char2 = "FN";
          }
          else
          {
            // ***--- OK to continue ...
          }

          // ** OK **
        }

        if (IsEmpty(local.Alt.Number))
        {
          export.Alternate.Number = "";
        }
        else
        {
          UseEabPadLeftWithZeros();
          export.Alternate.Number = local.TextWorkArea.Text10;
        }

        // ****** Read to see if the obligation is a result of a payment request
        // ******
        if (ReadPaymentRequest())
        {
          MovePaymentRequest(entities.PaymentRequest, export.PaymentRequest);
        }
        else
        {
          // ****** OKAY if not found ******
        }

        // : Get the Obligation Details.  For this Obligation Type, there
        //   should only be one Obligation Transaction.
        if (ReadObligationTransaction1())
        {
          MoveObligationTransaction(entities.ObligationTransaction,
            export.ObligationTransaction);

          if (ReadDebtDetailDebtDetailStatusHistory())
          {
            export.DebtDetail.CoveredPrdEndDt =
              entities.DebtDetail.CoveredPrdEndDt;
            export.DebtDetail.CoveredPrdStartDt =
              entities.DebtDetail.CoveredPrdStartDt;
            export.DebtDetail.DueDt = entities.DebtDetail.DueDt;

            // =================================================
            // 6/9/99 - bud adams  -  Now displaying the DDSH Effective_Date
            //   if status is "D"eactive instead of the DD Retired_Date.
            // =================================================
            if (AsChar(entities.DebtDetailStatusHistory.Code) == 'D')
            {
              export.DebtDetailStatusHistory.EffectiveDt =
                entities.DebtDetailStatusHistory.EffectiveDt;
            }

            export.BalanceOwed.TotalCurrency =
              entities.DebtDetail.BalanceDueAmt;
            export.InterestOwed.TotalCurrency =
              entities.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();

            if (AsChar(entities.DebtDetailStatusHistory.Code) == AsChar
              (import.HcDdshActiveStatus.Code))
            {
              export.ObligationAmt.TotalCurrency += entities.
                ObligationTransaction.Amount;
            }
          }
          else
          {
            ExitState = "FN0211_DEBT_DETAIL_NF";
          }
        }

        // =================================================
        // 11/4/99 - b adams  -  PR# 78500: If adjustments exist, to not
        //   allow the Debt_Detail Balance_Due_Amt to be updated.
        // =================================================
        export.AdjustmentExists.Flag = "N";

        if (ReadObligationTransaction2())
        {
          export.AdjustmentExists.Flag = "Y";
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_NF";
      }
    }
    else
    {
      ExitState = "FN0000_OBLIG_NOT_YET_CREATED";
    }

    export.TotalOwed.TotalCurrency = export.BalanceOwed.TotalCurrency + export
      .InterestOwed.TotalCurrency;
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
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private static void MoveScreenObligationStatus(ScreenObligationStatus source,
    ScreenObligationStatus target)
  {
    target.ObligationStatusTxt = source.ObligationStatusTxt;
    target.ObligationStatus = source.ObligationStatus;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    export.Country.Assign(useExport.CodeValue);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnGetObligationAssignment()
  {
    var useImport = new FnGetObligationAssignment.Import();
    var useExport = new FnGetObligationAssignment.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = entities.ObligorCsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Current.Date = import.Current.Date;
    useImport.CsePersonAccount.Type1 = import.HcCpaObligor.Type1;

    Call(FnGetObligationAssignment.Execute, useImport, useExport);

    export.ServiceProvider.Assign(useExport.ServiceProvider);
    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseFnGetObligationStatus()
  {
    var useImport = new FnGetObligationStatus.Import();
    var useExport = new FnGetObligationStatus.Export();

    useImport.ObligationType.Assign(entities.CountObligationType);
    useImport.Current.Date = import.Current.Date;
    useImport.CsePersonAccount.Type1 = import.HcCpaObligor.Type1;
    useImport.CsePerson.Number = entities.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.CountObligation.SystemGeneratedIdentifier;
    useImport.HcOtCVoluntary.Classification =
      import.HcOtCVoluntary.Classification;
    useImport.HcOtCAccruing.Classification =
      import.HcOtCAccruing.Classification;

    Call(FnGetObligationStatus.Execute, useImport, useExport);

    MoveScreenObligationStatus(useExport.ScreenObligationStatus,
      local.ScreenObligationStatus);
  }

  private void UseFnSetFrequencyTextField()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    useImport.ObligationPaymentSchedule.Assign(
      entities.ObligationPaymentSchedule);

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    export.FrequencyWorkSet.FrequencyDescription =
      useExport.FrequencyWorkSet.FrequencyDescription;
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.AlternateAddr.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligorCsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.AlternateAddr.Number = db.GetString(reader, 0);
        entities.AlternateAddr.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.AlternateAddr.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.LegalAction.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.AlternateAddr.Number = db.GetString(reader, 0);
        entities.AlternateAddr.Populated = true;
      });
  }

  private bool ReadCsePersonCsePersonAccount()
  {
    entities.ObligorCsePerson.Populated = false;
    entities.ObligorCsePersonAccount.Populated = false;

    return Read("ReadCsePersonCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Obligor.Number);
        db.SetString(command, "type", import.HcCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.ObligorCsePerson.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePersonAccount.Type1 = db.GetString(reader, 2);
        entities.ObligorCsePerson.Populated = true;
        entities.ObligorCsePersonAccount.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ObligorCsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligorCsePersonAccount.Type1);
      });
  }

  private bool ReadDebtDetailDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;
    entities.DebtDetailStatusHistory.Populated = false;

    return Read("ReadDebtDetailDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetNullableDate(
          command, "discontinueDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 14);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 15);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 16);
        entities.DebtDetailStatusHistory.ReasonTxt =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.Populated = true;
        entities.DebtDetailStatusHistory.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
      });
  }

  private bool ReadInterestSuppStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.InterestSuppStatusHistory.Populated = false;

    return Read("ReadInterestSuppStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterestSuppStatusHistory.ObgId = db.GetInt32(reader, 0);
        entities.InterestSuppStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.InterestSuppStatusHistory.CpaType = db.GetString(reader, 2);
        entities.InterestSuppStatusHistory.OtyId = db.GetInt32(reader, 3);
        entities.InterestSuppStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.InterestSuppStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.InterestSuppStatusHistory.DiscontinueDate =
          db.GetDate(reader, 6);
        entities.InterestSuppStatusHistory.Populated = true;
        CheckValid<InterestSuppStatusHistory>("CpaType",
          entities.InterestSuppStatusHistory.CpaType);
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "orderEffDate", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 5);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 6);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadLegalActionLegalActionDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionLegalActionDetail",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier",
          entities.Obligation.LgaIdentifier.GetValueOrDefault());
        db.SetNullableInt32(
          command, "ladNumber",
          entities.Obligation.LadNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 3);
        entities.LegalAction.CspNumber = db.GetNullableString(reader, 4);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 5);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 6);
        entities.LegalAction.Populated = true;
        entities.LegalActionDetail.Populated = true;
      });
  }

  private bool ReadManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ManualDistributionAudit.Populated = false;

    return Read("ReadManualDistributionAudit",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
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

  private bool ReadObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", local.RecoveryObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligorCsePersonAccount.CspNumber);
        db.SetString(
          command, "debtTypClass", import.ObligationType.Classification);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrqId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 5);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 6);
        entities.Obligation.Description = db.GetNullableString(reader, 7);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 8);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 9);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 10);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 11);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 12);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 13);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 14);
        entities.Obligation.CreatedBy = db.GetString(reader, 15);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 17);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 18);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 19);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 20);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 21);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", local.RecoveryObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligorCsePersonAccount.CspNumber);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrqId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 5);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 6);
        entities.Obligation.Description = db.GetNullableString(reader, 7);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 8);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 9);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 10);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 11);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 12);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 13);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 14);
        entities.Obligation.CreatedBy = db.GetString(reader, 15);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 17);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 18);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 19);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 20);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 21);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private IEnumerable<bool> ReadObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.CountObligationType.Populated = false;
    entities.CountObligation.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligorCsePersonAccount.CspNumber);
        db.SetString(
          command, "debtTypClass", import.ObligationType.Classification);
      },
      (db, reader) =>
      {
        entities.CountObligation.CpaType = db.GetString(reader, 0);
        entities.CountObligation.CspNumber = db.GetString(reader, 1);
        entities.CountObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CountObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.CountObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.CountObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.CountObligation.OrderTypeCode = db.GetString(reader, 5);
        entities.CountObligationType.Code = db.GetString(reader, 6);
        entities.CountObligationType.Classification = db.GetString(reader, 7);
        entities.CountObligationType.Populated = true;
        entities.CountObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.CountObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.CountObligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.CountObligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.CountObligationType.Classification);

        return true;
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
        db.SetDate(command, "startDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 8);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 9);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 11);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
      });
  }

  private bool ReadObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction1",
      (db, command) =>
      {
        db.SetString(command, "debtTyp", import.HcOtrnDtDebtDetail.DebtType);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 7);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 10);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.ObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligationTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "type", import.HcOtrnTDebtAdjustment.Type1);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 7);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 10);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.ObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligationType1()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType1",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType2",
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
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadPaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.Obligation.PrqId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Type1 = db.GetString(reader, 1);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
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
    /// A value of HcOtrnTDebtAdjustment.
    /// </summary>
    [JsonPropertyName("hcOtrnTDebtAdjustment")]
    public ObligationTransaction HcOtrnTDebtAdjustment
    {
      get => hcOtrnTDebtAdjustment ??= new();
      set => hcOtrnTDebtAdjustment = value;
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
    /// A value of HcOtCAccruing.
    /// </summary>
    [JsonPropertyName("hcOtCAccruing")]
    public ObligationType HcOtCAccruing
    {
      get => hcOtCAccruing ??= new();
      set => hcOtCAccruing = value;
    }

    /// <summary>
    /// A value of HcOtcFee.
    /// </summary>
    [JsonPropertyName("hcOtcFee")]
    public ObligationType HcOtcFee
    {
      get => hcOtcFee ??= new();
      set => hcOtcFee = value;
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
    /// A value of HcDdshActiveStatus.
    /// </summary>
    [JsonPropertyName("hcDdshActiveStatus")]
    public DebtDetailStatusHistory HcDdshActiveStatus
    {
      get => hcDdshActiveStatus ??= new();
      set => hcDdshActiveStatus = value;
    }

    /// <summary>
    /// A value of HcOtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("hcOtrnDtDebtDetail")]
    public ObligationTransaction HcOtrnDtDebtDetail
    {
      get => hcOtrnDtDebtDetail ??= new();
      set => hcOtrnDtDebtDetail = value;
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
    /// A value of HcOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hcOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HcOtrrConcurrentObliga
    {
      get => hcOtrrConcurrentObliga ??= new();
      set => hcOtrrConcurrentObliga = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private ObligationTransaction hcOtrnTDebtAdjustment;
    private ObligationType hcOtCVoluntary;
    private ObligationType hcOtCAccruing;
    private ObligationType hcOtcFee;
    private DateWorkArea current;
    private DebtDetailStatusHistory hcDdshActiveStatus;
    private ObligationTransaction hcOtrnDtDebtDetail;
    private CsePersonAccount hcCpaObligor;
    private ObligationTransactionRlnRsn hcOtrrConcurrentObliga;
    private ObligationType obligationType;
    private CsePerson obligor;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of DeactiveObligtionExists.
    /// </summary>
    [JsonPropertyName("deactiveObligtionExists")]
    public Common DeactiveObligtionExists
    {
      get => deactiveObligtionExists ??= new();
      set => deactiveObligtionExists = value;
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
    /// A value of AdjustmentExists.
    /// </summary>
    [JsonPropertyName("adjustmentExists")]
    public Common AdjustmentExists
    {
      get => adjustmentExists ??= new();
      set => adjustmentExists = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of InterestSuspensionInd.
    /// </summary>
    [JsonPropertyName("interestSuspensionInd")]
    public Common InterestSuspensionInd
    {
      get => interestSuspensionInd ??= new();
      set => interestSuspensionInd = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of OrderClassification.
    /// </summary>
    [JsonPropertyName("orderClassification")]
    public LegalAction OrderClassification
    {
      get => orderClassification ??= new();
      set => orderClassification = value;
    }

    /// <summary>
    /// A value of PaymentScheduleInd.
    /// </summary>
    [JsonPropertyName("paymentScheduleInd")]
    public Common PaymentScheduleInd
    {
      get => paymentScheduleInd ??= new();
      set => paymentScheduleInd = value;
    }

    /// <summary>
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of ObligationAmt.
    /// </summary>
    [JsonPropertyName("obligationAmt")]
    public Common ObligationAmt
    {
      get => obligationAmt ??= new();
      set => obligationAmt = value;
    }

    /// <summary>
    /// A value of BalanceOwed.
    /// </summary>
    [JsonPropertyName("balanceOwed")]
    public Common BalanceOwed
    {
      get => balanceOwed ??= new();
      set => balanceOwed = value;
    }

    /// <summary>
    /// A value of InterestOwed.
    /// </summary>
    [JsonPropertyName("interestOwed")]
    public Common InterestOwed
    {
      get => interestOwed ??= new();
      set => interestOwed = value;
    }

    /// <summary>
    /// A value of TotalOwed.
    /// </summary>
    [JsonPropertyName("totalOwed")]
    public Common TotalOwed
    {
      get => totalOwed ??= new();
      set => totalOwed = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
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

    private CodeValue country;
    private Common deactiveObligtionExists;
    private LegalActionDetail legalActionDetail;
    private Common adjustmentExists;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private PaymentRequest paymentRequest;
    private ServiceProvider serviceProvider;
    private CsePersonsWorkSet csePersonsWorkSet;
    private FrequencyWorkSet frequencyWorkSet;
    private Common interestSuspensionInd;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private ObligationType obligationType;
    private LegalAction orderClassification;
    private Common paymentScheduleInd;
    private Common manualDistributionInd;
    private Common obligationAmt;
    private Common balanceOwed;
    private Common interestOwed;
    private Common totalOwed;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePersonsWorkSet alternate;
    private InterstateRequest interstateRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Alt.
    /// </summary>
    [JsonPropertyName("alt")]
    public CsePerson Alt
    {
      get => alt ??= new();
      set => alt = value;
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
    /// A value of NumberOfObligations.
    /// </summary>
    [JsonPropertyName("numberOfObligations")]
    public Common NumberOfObligations
    {
      get => numberOfObligations ??= new();
      set => numberOfObligations = value;
    }

    /// <summary>
    /// A value of RecoveryObligation.
    /// </summary>
    [JsonPropertyName("recoveryObligation")]
    public Obligation RecoveryObligation
    {
      get => recoveryObligation ??= new();
      set => recoveryObligation = value;
    }

    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private ScreenObligationStatus screenObligationStatus;
    private TextWorkArea textWorkArea;
    private CsePerson alt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common numberOfObligations;
    private Obligation recoveryObligation;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CountObligationType.
    /// </summary>
    [JsonPropertyName("countObligationType")]
    public ObligationType CountObligationType
    {
      get => countObligationType ??= new();
      set => countObligationType = value;
    }

    /// <summary>
    /// A value of CountObligation.
    /// </summary>
    [JsonPropertyName("countObligation")]
    public Obligation CountObligation
    {
      get => countObligation ??= new();
      set => countObligation = value;
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
    /// A value of AlternateAddr.
    /// </summary>
    [JsonPropertyName("alternateAddr")]
    public CsePerson AlternateAddr
    {
      get => alternateAddr ??= new();
      set => alternateAddr = value;
    }

    /// <summary>
    /// A value of InterestSuppStatusHistory.
    /// </summary>
    [JsonPropertyName("interestSuppStatusHistory")]
    public InterestSuppStatusHistory InterestSuppStatusHistory
    {
      get => interestSuppStatusHistory ??= new();
      set => interestSuppStatusHistory = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
    }

    private ObligationType countObligationType;
    private Obligation countObligation;
    private PaymentRequest paymentRequest;
    private CsePerson alternateAddr;
    private InterestSuppStatusHistory interestSuppStatusHistory;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private CsePerson obligorCsePerson;
    private CsePersonAccount obligorCsePersonAccount;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ManualDistributionAudit manualDistributionAudit;
    private InterstateRequest interstateRequest;
    private InterstateRequestObligation interstateRequestObligation;
  }
#endregion
}
