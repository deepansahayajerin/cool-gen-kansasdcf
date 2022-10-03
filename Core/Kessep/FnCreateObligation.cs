// Program: FN_CREATE_OBLIGATION, ID: 372086138, model: 746.
// Short name: SWE00377
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_OBLIGATION.
/// </para>
/// <para>
/// This action block sets up an Obligation.  It is called by the Obligation 
/// maintenance procedures.
/// It creates a CSE Person Account if one does not already exist, an 
/// Obligation, and an Obligation Payment Schedule if the Obligation is
/// accruing.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateObligation.
  /// </summary>
  public FnCreateObligation(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***--- 05/01/97 - Sumanta - MTW
    // ***--- Changed DB2 current date to IEF current date..
    // ***--- 07/24/97 - Paul R. Egger-MTW
    // ****---- Always setting the Primary_Secondary_Code to Spaces
    // ================================================
    // 10/2/98  -  B Adams  -  deleted all references to CURRENT_TIMESTAMP and 
    // CURRENT_DATE and imported those values.  Deleted the use of
    // FN_Hardcode_Debt_Distribution and imported those values.  Deleted all
    // ROUNDED functions.  All this adds tremendous amount of overhead.
    // ================================================
    // =================================================
    // 12/1/98 - B Adams  -  Added 'use fn_read_case_no_and_
    //   worker_id in order to get the proper worker to assign to Fee
    //   obligations.
    //   Import Supported_CSE_Person is needed only for Fees (right
    //   now) so the proper Worker can be assigned to the Obligation.
    // 4/22/99 - bud adams  -  Worker assigned to Fees is the
    //   worker who creates them.  Removed this USE and all prior
    //   code that was intended to perform this function.
    // Oct, 2000, MBrown, PR#94117 - not using random sys gen id cab any more
    // when we create recapture inclusions.   Now we add 1 to the max id.
    // =================================================
    ExitState = "ACO_NN0000_ALL_OK";
    MoveObligation2(import.Obligation, export.Obligation);

    // =================================================
    // 12/28/98 - b adams  -  For some reason, the test on if the
    //   persistent view of obligation_type were populated started
    //   failing.  Couldn't figure it out, and didn't feel like chasing it
    //   down.  Replaced it with this Read.
    //   READ properties set
    // 2/19/1999 - bud adams  -  also removed persistent view of
    //   cse_person and all logic related to it.
    // =================================================
    if (!ReadObligationType())
    {
      ExitState = "FN0000_OBLIG_TYPE_NF_RB";

      return;
    }

    // *** Read Legal_Action thru the Identifier to have Currency on it ***
    local.Interstate.Flag = "K";

    if (import.LegalAction.Identifier != 0)
    {
      if (ReadLegalAction())
      {
        // *** Legal_Action successfully Read, Continue Processing
        // =================================================
        // 12/30/98 - b adams  -  New one-to-many relationship between
        //   Obligation and Legal_Action_Detail implemented.  It's fully
        //   optional, but will always be populated if coming from LDET
        // =================================================
        if (!ReadLegalActionDetail())
        {
          ExitState = "LEGAL_ACTION_DETAIL_NF";

          return;
        }

        // =================================================
        // 12/30/98 - b adams  -  Removed a Read of FIPS.  The data
        //   was not used for anything.  At one time it was used for the
        //   interstate attributes, which is incorrect.
        // =================================================
      }
      else
      {
        ExitState = "LEGAL_ACTION_NF";

        return;
      }
    }

    if (ReadCsePersonAccount())
    {
      // ** ALL OK **
    }
    else
    {
      // *** If the CSE_PERSON is not an obligor, Set him up as an Obligor ***
      if (ReadCsePerson())
      {
        try
        {
          CreateCsePersonAccount();

          // ** ALL OK **
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIGOR_ACCT_AE_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIGOR_ACCT_PV_RB";

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
        ExitState = "CSE_PERSON_NF_RB";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // =================================================
    // All accruing and non-accruing obligations are a result of a
    //   legal action.  Some fees and recoveries are a result of a
    //   legal action, but no voluntary are ever due to a legal action.
    // =================================================
    if (AsChar(entities.ObligationType.Classification) == AsChar
      (import.HcOtCVoluntaryClassif.Classification) || (
        AsChar(entities.ObligationType.Classification) == AsChar
      (import.HcOtCRecoverClassific.Classification) || AsChar
      (entities.ObligationType.Classification) == AsChar
      (import.HcOtCFeesClassificati.Classification)) && import
      .LegalAction.Identifier == 0)
    {
      // =================================================
      // 10/14/98 - B Adams  -  Changed IF - ASSOCIATE - [end if] that
      //   processed the Associate to Legal_Action into 2 different
      //   Create actions, to improve performance.  The separate
      //   Associate action would result in a separate UPDATE of
      //   the newly created Obligation.
      // =================================================
      if (import.LegalAction.Identifier != 0)
      {
        try
        {
          CreateObligation1();
          MoveObligation1(entities.Obligation, export.Obligation);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIGATION_AE_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIGATION_PV_RB";

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
        try
        {
          CreateObligation3();
          MoveObligation1(entities.Obligation, export.Obligation);
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_OBLIGATION_AE_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_OBLIGATION_PV_RB";

              break;
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
      try
      {
        CreateObligation2();
        MoveObligation1(entities.Obligation, export.Obligation);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGATION_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIGATION_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // =================================================
    // 2/17/1999 - b adams  -  This is also meant for Fees; if this
    //   flag is set to "Y" in OFEE, then that means it's a non-court
    //   ordered debt and, so, will have interest suspended.
    // =================================================
    if (AsChar(import.HistoryIndicator.Flag) == 'Y' && (
      AsChar(entities.ObligationType.Classification) == AsChar
      (import.HcOtCAccruingClassifi.Classification) || AsChar
      (entities.ObligationType.Classification) == AsChar
      (import.HcOtCFeesClassificati.Classification)))
    {
      // <<< Suspend Interest for a HISTORY Accruing Obligation >>>
      local.CreateAttemptCount.Count = 0;

      if (Equal(import.ObligationPaymentSchedule.EndDt, local.Blank.Date))
      {
        local.InterestSuppStatusHistory.DiscontinueDate = import.Max.Date;
      }
      else
      {
        local.InterestSuppStatusHistory.DiscontinueDate =
          import.ObligationPaymentSchedule.EndDt;
      }

      if (Equal(import.ObligationPaymentSchedule.StartDt, local.Blank.Date))
      {
        local.InterestSuppStatusHistory.EffectiveDate = import.Current.Date;
      }
      else
      {
        local.InterestSuppStatusHistory.EffectiveDate =
          import.ObligationPaymentSchedule.StartDt;
      }

      if (AsChar(entities.ObligationType.Classification) == AsChar
        (import.HcOtCFeesClassificati.Classification))
      {
        local.InterestSuppStatusHistory.ReasonText =
          "All non-court ordered Fee obligations have interest suspended.";
      }
      else
      {
        local.InterestSuppStatusHistory.ReasonText =
          "Interest is suspended when history obligation is created.";
      }

      while(local.CreateAttemptCount.Count < 10)
      {
        // <<< RBM  10/27/97  The CREATE will be attempted 10 times
        //     to avoid possible duplicates >>>
        try
        {
          CreateInterestSuppStatusHistory();

          break;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ++local.CreateAttemptCount.Count;

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_INTEREST_SUPP_CREATE_PVRB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (local.CreateAttemptCount.Count == 10)
      {
        ExitState = "FN0000_INT_SUPP_STAT_HIST_NU_RB";

        return;
      }

      if (AsChar(entities.ObligationType.Classification) == AsChar
        (import.HcOtCAccruingClassifi.Classification))
      {
        // =================================================
        // 7/5/99 - Bud Adams  -  For history obligations, both coupons
        //   and statements should be suppressed (doc-type-to-suppress = S)
        //   and it should be done at the Obligation level (type = O)
        //   Accruing obligations only.
        // 8/8/99 - mfb - Set the discontinue date of the suppression to the
        //  last day of next month.
        // =================================================
        local.NextMonth.Date = AddMonths(import.Current.Date, 1);
        UseCabFirstAndLastDateOfMonth();

        while(local.CreateAttemptCount.Count < 5)
        {
          try
          {
            CreateStmtCouponSuppStatusHist();

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                if (local.CreateAttemptCount.Count == 4)
                {
                  ExitState = "FN0000_STMT_CPN_SUPP_NOT_CRTD_RB";

                  return;
                }

                ++local.CreateAttemptCount.Count;

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_STMT_CPN_SUPP_S_HST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    // *** 08/08/96 :: It was confirmed by the users that a Payment Schedule can
    // be created for all kinds of Obligations and not alone for Accruing
    // Obligations as was before.
    if (IsEmpty(import.ObligationPaymentSchedule.FrequencyCode))
    {
      // *** Payment Schedule not to be created
    }
    else
    {
      try
      {
        CreateObligationPaymentSchedule();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_PAYMENT_SCHEDULE_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_PAYMENT_SCHEDULE_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // *** In case of a Recovery Obligation, an Obligor Recapture_Rule needs to 
    // exist.  If none does, then create one and intialize the settings to the
    // Default_Recapture_Rule for the obligation type. The Obligation also needs
    // to be included in the Recapture_Inclusion
    // ** Try to get the active Default_Rule. If not found, get the one having 
    // an effective_date closest to current date.
    if (AsChar(entities.ObligationType.Classification) == AsChar
      (import.HcOtCRecoverClassific.Classification))
    {
      // =================================================
      // 2/26/1999 - Bud Adams  -  If a recovery is Interstate then a
      //   Recapture_Inclusion record should not be created.
      // =================================================
      // 3/2/1999 - Bud Adams  -  Also, the Obligor_Rule should not
      //   be created.  And, if the Obligor is an Oranization, neither
      //   record should be created.
      // =================================================
      if (AsChar(import.Obligation.OrderTypeCode) == 'I' || CharAt
        (import.CsePerson.Number, 10) == 'O')
      {
        goto Test;
      }

      // ***
      // Added this read 04/14/97 	A.Kinney
      // 		***
      // =================================================
      // 3/11/1999 - The 'discontinue_date' selection criteria had to
      //   be changed from  >=  to  >  or else multiple rows were being
      //   returned.
      // =================================================
      if (ReadObligorRule())
      {
        // *** An obligor rule exists, no need to create another one.
        //  To UPDATE the rule use RCAP tran. ***
      }
      else
      {
        if (ReadDefaultRule())
        {
          local.Work.Flag = "Y";
        }

        if (AsChar(local.Work.Flag) == 'Y')
        {
          // <<< RBM  10/27/97  The CREATE will be attempted 10 times
          //     to avoid possible duplicates >>>
          local.CreateAttemptCount.Count = 0;

          while(local.CreateAttemptCount.Count < 10)
          {
            try
            {
              CreateObligorRule();

              // ** Recapture Rule for the Obligor created
              goto Read;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ++local.CreateAttemptCount.Count;

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_RECAPTURE_RULE_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          if (local.CreateAttemptCount.Count == 10)
          {
            ExitState = "FN0000_RECAPTURE_RULE_AE_RB";

            return;
          }
        }
        else
        {
          ExitState = "DEFAULT_RULE_NF_RB";

          return;
        }
      }

Read:

      // <<< RBM  10/27/97  The CREATE will be attempted 10 times
      //     to avoid possible duplicates >>>
      // Oct, 2000, MBrown, PR#94117 - not using random sys gen id cab any more.
      // Now we add 1 to the last assigned id.  Since this is a new obligation,
      // just set it to 1.
      // Also removed loop for create, since it is not necessary.
      try
      {
        CreateRecaptureInclusion();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_RECAPTURE_INCLUSION_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_RECAPTURE_INCLUSION_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

Test:

    // ********************************************************************
    // The following set of statements will associate the obligation with
    // the payment request if the obligation is rooted in a payment request.
    // Skip Hardy  MTW 12/15/1997
    // ********************************************************************
    if (import.PaymentRequest.SystemGeneratedIdentifier != 0)
    {
      // ****** Read payment request ******
      if (ReadPaymentRequest())
      {
        // ****** Associate payment request and obligation ******
        try
        {
          UpdatePaymentRequest();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_PAYMENT_REQUEST_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_PAYMENT_REQUEST_PV_RB";

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
        ExitState = "FN0000_PAYMENT_REQUEST_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ****** Read current payment status history for payment request ******
      if (ReadPaymentStatusHistory())
      {
        // ****** Update current payment status history with discontinue date 
        // ******
        try
        {
          UpdatePaymentStatusHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_PYMNT_STAT_HIST_NU_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        local.PaymentStatusHistoryId.Attribute3DigitRandomNumber =
          entities.PaymentStatusHistory.SystemGeneratedIdentifier + 1;
      }
      else
      {
        ExitState = "FN0000_PYMNT_STAT_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ****** Read payment status for the status of Recovery Created ******
      if (ReadPaymentStatus())
      {
        // ****** Create payment status history with status of Recovery Created 
        // ******
        try
        {
          CreatePaymentStatusHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_PYMNT_STAT_HIST_AE_RB";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_PYMNT_STAT_HIST_PV_RB";

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
        ExitState = "FN0000_PYMNT_STAT_NF_RB";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
    }
  }

  private static void MoveObligation1(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveObligation2(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.NextMonth.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.NextMonth.Date = useExport.Last.Date;
  }

  private int UseFnAssignObligationId()
  {
    var useImport = new FnAssignObligationId.Import();
    var useExport = new FnAssignObligationId.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(FnAssignObligationId.Execute, useImport, useExport);

    return useExport.Obligation.SystemGeneratedIdentifier;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateCsePersonAccount()
  {
    var cspNumber = entities.CsePerson.Number;
    var type1 = import.HardcodeCpaObligor.Type1;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;

    CheckValid<CsePersonAccount>("Type1", type1);
    entities.CsePersonAccount.Populated = false;
    Update("CreateCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDate(command, "recompBalFromDt", default(DateTime));
        db.SetNullableDecimal(command, "stdTotGiftColl", 0M);
        db.SetNullableString(command, "triggerType", "");
      });

    entities.CsePersonAccount.CspNumber = cspNumber;
    entities.CsePersonAccount.Type1 = type1;
    entities.CsePersonAccount.CreatedBy = createdBy;
    entities.CsePersonAccount.CreatedTmst = createdTmst;
    entities.CsePersonAccount.Populated = true;
  }

  private void CreateInterestSuppStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var obgId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var otyId = entities.Obligation.DtyGeneratedId;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var effectiveDate = local.InterestSuppStatusHistory.EffectiveDate;
    var discontinueDate = local.InterestSuppStatusHistory.DiscontinueDate;
    var reasonText = local.InterestSuppStatusHistory.ReasonText ?? "";
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;

    CheckValid<InterestSuppStatusHistory>("CpaType", cpaType);
    entities.InterestSuppStatusHistory.Populated = false;
    Update("CreateInterestSuppStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otyId", otyId);
        db.SetInt32(command, "collId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
      });

    entities.InterestSuppStatusHistory.ObgId = obgId;
    entities.InterestSuppStatusHistory.CspNumber = cspNumber;
    entities.InterestSuppStatusHistory.CpaType = cpaType;
    entities.InterestSuppStatusHistory.OtyId = otyId;
    entities.InterestSuppStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.InterestSuppStatusHistory.EffectiveDate = effectiveDate;
    entities.InterestSuppStatusHistory.DiscontinueDate = discontinueDate;
    entities.InterestSuppStatusHistory.ReasonText = reasonText;
    entities.InterestSuppStatusHistory.CreatedBy = createdBy;
    entities.InterestSuppStatusHistory.CreatedTmst = createdTmst;
    entities.InterestSuppStatusHistory.Populated = true;
  }

  private void CreateObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseFnAssignObligationId();
    var dtyGeneratedId = entities.ObligationType.SystemGeneratedIdentifier;
    var lgaId = entities.LegalAction.Identifier;
    var otherStateAbbr = import.Obligation.OtherStateAbbr ?? "";
    var description = import.Obligation.Description ?? "";
    var historyInd = import.Obligation.HistoryInd ?? "";
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var orderTypeCode = import.Obligation.OrderTypeCode;
    var lgaIdentifier = entities.LegalActionDetail.LgaIdentifier;
    var ladNumber = entities.LegalActionDetail.Number;

    CheckValid<Obligation>("CpaType", cpaType);
    CheckValid<Obligation>("PrimarySecondaryCode", "");
    CheckValid<Obligation>("OrderTypeCode", orderTypeCode);
    entities.Obligation.Populated = false;
    Update("CreateObligation1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obId", systemGeneratedIdentifier);
        db.SetInt32(command, "dtyGeneratedId", dtyGeneratedId);
        db.SetNullableInt32(command, "lgaId", lgaId);
        db.SetNullableString(command, "otherStateAbbr", otherStateAbbr);
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "historyInd", historyInd);
        db.SetNullableString(command, "primSecCd", "");
        db.SetNullableInt32(command, "preConvDebtNo", 0);
        db.SetNullableString(command, "precnvrsnCaseNbr", "");
        db.SetNullableDecimal(command, "aodNadArrBal", 0M);
        db.SetNullableDate(command, "lastPymntDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetString(command, "ordTypCd", orderTypeCode);
        db.SetNullableString(command, "dormantInd", "");
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetNullableInt32(command, "ladNumber", ladNumber);
      });

    entities.Obligation.CpaType = cpaType;
    entities.Obligation.CspNumber = cspNumber;
    entities.Obligation.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Obligation.DtyGeneratedId = dtyGeneratedId;
    entities.Obligation.PrqId = null;
    entities.Obligation.LgaId = lgaId;
    entities.Obligation.OtherStateAbbr = otherStateAbbr;
    entities.Obligation.Description = description;
    entities.Obligation.HistoryInd = historyInd;
    entities.Obligation.PrimarySecondaryCode = "";
    entities.Obligation.PreConversionDebtNumber = 0;
    entities.Obligation.PreConversionCaseNumber = "";
    entities.Obligation.CreatedBy = createdBy;
    entities.Obligation.CreatedTmst = createdTmst;
    entities.Obligation.OrderTypeCode = orderTypeCode;
    entities.Obligation.LgaIdentifier = lgaIdentifier;
    entities.Obligation.LadNumber = ladNumber;
    entities.Obligation.Populated = true;
  }

  private void CreateObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseFnAssignObligationId();
    var dtyGeneratedId = entities.ObligationType.SystemGeneratedIdentifier;
    var lgaId = entities.LegalAction.Identifier;
    var otherStateAbbr = import.Obligation.OtherStateAbbr ?? "";
    var description = import.Obligation.Description ?? "";
    var historyInd = import.Obligation.HistoryInd ?? "";
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var orderTypeCode = import.Obligation.OrderTypeCode;
    var lgaIdentifier = entities.LegalActionDetail.LgaIdentifier;
    var ladNumber = entities.LegalActionDetail.Number;

    CheckValid<Obligation>("CpaType", cpaType);
    CheckValid<Obligation>("PrimarySecondaryCode", "");
    CheckValid<Obligation>("OrderTypeCode", orderTypeCode);
    entities.Obligation.Populated = false;
    Update("CreateObligation2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obId", systemGeneratedIdentifier);
        db.SetInt32(command, "dtyGeneratedId", dtyGeneratedId);
        db.SetNullableInt32(command, "lgaId", lgaId);
        db.SetNullableString(command, "otherStateAbbr", otherStateAbbr);
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "historyInd", historyInd);
        db.SetNullableString(command, "primSecCd", "");
        db.SetNullableInt32(command, "preConvDebtNo", 0);
        db.SetNullableString(command, "precnvrsnCaseNbr", "");
        db.SetNullableDecimal(command, "aodNadArrBal", 0M);
        db.SetNullableDate(command, "lastPymntDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetString(command, "ordTypCd", orderTypeCode);
        db.SetNullableString(command, "dormantInd", "");
        db.SetNullableInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetNullableInt32(command, "ladNumber", ladNumber);
      });

    entities.Obligation.CpaType = cpaType;
    entities.Obligation.CspNumber = cspNumber;
    entities.Obligation.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Obligation.DtyGeneratedId = dtyGeneratedId;
    entities.Obligation.PrqId = null;
    entities.Obligation.LgaId = lgaId;
    entities.Obligation.OtherStateAbbr = otherStateAbbr;
    entities.Obligation.Description = description;
    entities.Obligation.HistoryInd = historyInd;
    entities.Obligation.PrimarySecondaryCode = "";
    entities.Obligation.PreConversionDebtNumber = 0;
    entities.Obligation.PreConversionCaseNumber = "";
    entities.Obligation.CreatedBy = createdBy;
    entities.Obligation.CreatedTmst = createdTmst;
    entities.Obligation.OrderTypeCode = orderTypeCode;
    entities.Obligation.LgaIdentifier = lgaIdentifier;
    entities.Obligation.LadNumber = ladNumber;
    entities.Obligation.Populated = true;
  }

  private void CreateObligation3()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseFnAssignObligationId();
    var dtyGeneratedId = entities.ObligationType.SystemGeneratedIdentifier;
    var otherStateAbbr = import.Obligation.OtherStateAbbr ?? "";
    var description = import.Obligation.Description ?? "";
    var historyInd = import.Obligation.HistoryInd ?? "";
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var orderTypeCode = import.Obligation.OrderTypeCode;

    CheckValid<Obligation>("CpaType", cpaType);
    CheckValid<Obligation>("PrimarySecondaryCode", "");
    CheckValid<Obligation>("OrderTypeCode", orderTypeCode);
    entities.Obligation.Populated = false;
    Update("CreateObligation3",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obId", systemGeneratedIdentifier);
        db.SetInt32(command, "dtyGeneratedId", dtyGeneratedId);
        db.SetNullableString(command, "otherStateAbbr", otherStateAbbr);
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "historyInd", historyInd);
        db.SetNullableString(command, "primSecCd", "");
        db.SetNullableInt32(command, "preConvDebtNo", 0);
        db.SetNullableString(command, "precnvrsnCaseNbr", "");
        db.SetNullableDecimal(command, "aodNadArrBal", 0M);
        db.SetNullableDate(command, "lastPymntDt", default(DateTime));
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetString(command, "ordTypCd", orderTypeCode);
        db.SetNullableString(command, "dormantInd", "");
      });

    entities.Obligation.CpaType = cpaType;
    entities.Obligation.CspNumber = cspNumber;
    entities.Obligation.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Obligation.DtyGeneratedId = dtyGeneratedId;
    entities.Obligation.PrqId = null;
    entities.Obligation.LgaId = null;
    entities.Obligation.OtherStateAbbr = otherStateAbbr;
    entities.Obligation.Description = description;
    entities.Obligation.HistoryInd = historyInd;
    entities.Obligation.PrimarySecondaryCode = "";
    entities.Obligation.PreConversionDebtNumber = 0;
    entities.Obligation.PreConversionCaseNumber = "";
    entities.Obligation.CreatedBy = createdBy;
    entities.Obligation.CreatedTmst = createdTmst;
    entities.Obligation.OrderTypeCode = orderTypeCode;
    entities.Obligation.LgaIdentifier = null;
    entities.Obligation.LadNumber = null;
    entities.Obligation.Populated = true;
  }

  private void CreateObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otyType = entities.Obligation.DtyGeneratedId;
    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var obgCspNumber = entities.Obligation.CspNumber;
    var obgCpaType = entities.Obligation.CpaType;
    var startDt = import.ObligationPaymentSchedule.StartDt;
    var amount = import.ObligationPaymentSchedule.Amount.GetValueOrDefault();
    var endDt = import.ObligationPaymentSchedule.EndDt;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var frequencyCode = import.ObligationPaymentSchedule.FrequencyCode;
    var dayOfWeek =
      import.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault();
    var dayOfMonth1 =
      import.ObligationPaymentSchedule.DayOfMonth1.GetValueOrDefault();
    var dayOfMonth2 =
      import.ObligationPaymentSchedule.DayOfMonth2.GetValueOrDefault();
    var periodInd = import.ObligationPaymentSchedule.PeriodInd ?? "";

    CheckValid<ObligationPaymentSchedule>("ObgCpaType", obgCpaType);
    CheckValid<ObligationPaymentSchedule>("FrequencyCode", frequencyCode);
    CheckValid<ObligationPaymentSchedule>("PeriodInd", periodInd);
    entities.ObligationPaymentSchedule.Populated = false;
    Update("CreateObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "obgCspNumber", obgCspNumber);
        db.SetString(command, "obgCpaType", obgCpaType);
        db.SetDate(command, "startDt", startDt);
        db.SetNullableDecimal(command, "obligPschAmt", amount);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdateBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetString(command, "frqPrdCd", frequencyCode);
        db.SetNullableInt32(command, "dayOfWeek", dayOfWeek);
        db.SetNullableInt32(command, "dayOfMonth1", dayOfMonth1);
        db.SetNullableInt32(command, "dayOfMonth2", dayOfMonth2);
        db.SetNullableString(command, "periodInd", periodInd);
        db.SetNullableDate(command, "repymtLtrPrtDt", default(DateTime));
      });

    entities.ObligationPaymentSchedule.OtyType = otyType;
    entities.ObligationPaymentSchedule.ObgGeneratedId = obgGeneratedId;
    entities.ObligationPaymentSchedule.ObgCspNumber = obgCspNumber;
    entities.ObligationPaymentSchedule.ObgCpaType = obgCpaType;
    entities.ObligationPaymentSchedule.StartDt = startDt;
    entities.ObligationPaymentSchedule.Amount = amount;
    entities.ObligationPaymentSchedule.EndDt = endDt;
    entities.ObligationPaymentSchedule.CreatedBy = createdBy;
    entities.ObligationPaymentSchedule.CreatedTmst = createdTmst;
    entities.ObligationPaymentSchedule.FrequencyCode = frequencyCode;
    entities.ObligationPaymentSchedule.DayOfWeek = dayOfWeek;
    entities.ObligationPaymentSchedule.DayOfMonth1 = dayOfMonth1;
    entities.ObligationPaymentSchedule.DayOfMonth2 = dayOfMonth2;
    entities.ObligationPaymentSchedule.PeriodInd = periodInd;
    entities.ObligationPaymentSchedule.Populated = true;
  }

  private void CreateObligorRule()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var cpaDType = entities.CsePersonAccount.Type1;
    var cspDNumber = entities.CsePersonAccount.CspNumber;
    var effectiveDate = import.Current.Date;
    var negotiatedDate = entities.DefaultRule.NegotiatedDate;
    var discontinueDate = import.Max.Date;
    var nonAdcArrearsMaxAmount = entities.DefaultRule.NonAdcArrearsMaxAmount;
    var nonAdcArrearsAmount = entities.DefaultRule.NonAdcArrearsAmount;
    var nonAdcArrearsPercentage = entities.DefaultRule.NonAdcArrearsPercentage;
    var nonAdcCurrentMaxAmount = entities.DefaultRule.NonAdcCurrentMaxAmount;
    var nonAdcCurrentAmount = entities.DefaultRule.NonAdcCurrentAmount;
    var nonAdcCurrentPercentage = entities.DefaultRule.NonAdcCurrentPercentage;
    var passthruPercentage = entities.DefaultRule.PassthruPercentage;
    var passthruAmount = entities.DefaultRule.PassthruAmount;
    var passthruMaxAmount = entities.DefaultRule.PassthruMaxAmount;
    var createdBy = global.UserId;
    var createdTimestamp = import.Current.Timestamp;
    var type1 = "O";
    var repaymentLetterPrintDate =
      entities.DefaultRule.RepaymentLetterPrintDate;

    CheckValid<RecaptureRule>("CpaDType", cpaDType);
    CheckValid<RecaptureRule>("Type1", type1);
    entities.ObligorRule.Populated = false;
    Update("CreateObligorRule",
      (db, command) =>
      {
        db.SetInt32(command, "recaptureRuleId", systemGeneratedIdentifier);
        db.SetNullableString(command, "cpaDType", cpaDType);
        db.SetNullableString(command, "cspDNumber", cspDNumber);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "negotiatedDate", negotiatedDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.
          SetNullableDecimal(command, "naArrearsMaxAmt", nonAdcArrearsMaxAmount);
          
        db.SetNullableDecimal(command, "naArrearsAmount", nonAdcArrearsAmount);
        db.SetNullableInt32(command, "naArrearsPct", nonAdcArrearsPercentage);
        db.
          SetNullableDecimal(command, "naCurrMaxAmount", nonAdcCurrentMaxAmount);
          
        db.SetNullableDecimal(command, "naCurrAmount", nonAdcCurrentAmount);
        db.
          SetNullableInt32(command, "naCurrPercentage", nonAdcCurrentPercentage);
          
        db.SetNullableInt32(command, "passthruPercentag", passthruPercentage);
        db.SetNullableDecimal(command, "passthruAmount", passthruAmount);
        db.SetNullableDecimal(command, "passthruMaxAmt", passthruMaxAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "defaultRuleFille", "");
        db.SetNullableString(command, "obligorRuleFille", "");
        db.SetString(command, "type", type1);
        db.
          SetNullableDate(command, "repymntLtrPrtDt", repaymentLetterPrintDate);
          
      });

    entities.ObligorRule.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.ObligorRule.CpaDType = cpaDType;
    entities.ObligorRule.CspDNumber = cspDNumber;
    entities.ObligorRule.EffectiveDate = effectiveDate;
    entities.ObligorRule.NegotiatedDate = negotiatedDate;
    entities.ObligorRule.DiscontinueDate = discontinueDate;
    entities.ObligorRule.NonAdcArrearsMaxAmount = nonAdcArrearsMaxAmount;
    entities.ObligorRule.NonAdcArrearsAmount = nonAdcArrearsAmount;
    entities.ObligorRule.NonAdcArrearsPercentage = nonAdcArrearsPercentage;
    entities.ObligorRule.NonAdcCurrentMaxAmount = nonAdcCurrentMaxAmount;
    entities.ObligorRule.NonAdcCurrentAmount = nonAdcCurrentAmount;
    entities.ObligorRule.NonAdcCurrentPercentage = nonAdcCurrentPercentage;
    entities.ObligorRule.PassthruPercentage = passthruPercentage;
    entities.ObligorRule.PassthruAmount = passthruAmount;
    entities.ObligorRule.PassthruMaxAmount = passthruMaxAmount;
    entities.ObligorRule.CreatedBy = createdBy;
    entities.ObligorRule.CreatedTimestamp = createdTimestamp;
    entities.ObligorRule.LastUpdatedBy = "";
    entities.ObligorRule.LastUpdatedTmst = null;
    entities.ObligorRule.ObligorRuleFiller = "";
    entities.ObligorRule.Type1 = type1;
    entities.ObligorRule.RepaymentLetterPrintDate = repaymentLetterPrintDate;
    entities.ObligorRule.Populated = true;
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      local.PaymentStatusHistoryId.Attribute3DigitRandomNumber;
    var effectiveDate = import.Current.Date;
    var discontinueDate = import.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = import.Current.Timestamp;

    entities.PaymentStatusHistory.Populated = false;
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

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = "";
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreateRecaptureInclusion()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otyType = entities.Obligation.DtyGeneratedId;
    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var systemGeneratedId = 1;
    var discontinueDate = import.Max.Date;
    var effectiveDate = import.Current.Date;
    var createdBy = global.UserId;
    var createdTimestamp = import.Current.Timestamp;

    CheckValid<RecaptureInclusion>("CpaType", cpaType);
    entities.RecaptureInclusion.Populated = false;
    Update("CreateRecaptureInclusion",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "recapInclSysid", systemGeneratedId);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
      });

    entities.RecaptureInclusion.OtyType = otyType;
    entities.RecaptureInclusion.ObgGeneratedId = obgGeneratedId;
    entities.RecaptureInclusion.CspNumber = cspNumber;
    entities.RecaptureInclusion.CpaType = cpaType;
    entities.RecaptureInclusion.SystemGeneratedId = systemGeneratedId;
    entities.RecaptureInclusion.DiscontinueDate = discontinueDate;
    entities.RecaptureInclusion.EffectiveDate = effectiveDate;
    entities.RecaptureInclusion.CreatedBy = createdBy;
    entities.RecaptureInclusion.CreatedTimestamp = createdTimestamp;
    entities.RecaptureInclusion.LastUpdatedBy = "";
    entities.RecaptureInclusion.LastUpdatedTmst = null;
    entities.RecaptureInclusion.Populated = true;
  }

  private void CreateStmtCouponSuppStatusHist()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "O";
    var effectiveDate = import.Current.Date;
    var discontinueDate = local.NextMonth.Date;
    var reasonText = "History Obligation";
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var otyId = entities.Obligation.DtyGeneratedId;
    var cpaTypeOblig = entities.Obligation.CpaType;
    var cspNumberOblig = entities.Obligation.CspNumber;
    var obgId = entities.Obligation.SystemGeneratedIdentifier;
    var docTypeToSuppress = "S";

    CheckValid<StmtCouponSuppStatusHist>("CpaType", cpaType);
    CheckValid<StmtCouponSuppStatusHist>("Type1", type1);
    CheckValid<StmtCouponSuppStatusHist>("CpaTypeOblig", cpaTypeOblig);
    CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress", docTypeToSuppress);
      
    entities.StmtCouponSuppStatusHist.Populated = false;
    Update("CreateStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "collId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "obligationFiller", "");
        db.SetNullableInt32(command, "otyId", otyId);
        db.SetNullableString(command, "cpaTypeOblig", cpaTypeOblig);
        db.SetNullableString(command, "cspNumberOblig", cspNumberOblig);
        db.SetNullableInt32(command, "obgId", obgId);
        db.SetString(command, "docTypeToSupp", docTypeToSuppress);
      });

    entities.StmtCouponSuppStatusHist.CpaType = cpaType;
    entities.StmtCouponSuppStatusHist.CspNumber = cspNumber;
    entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.StmtCouponSuppStatusHist.Type1 = type1;
    entities.StmtCouponSuppStatusHist.EffectiveDate = effectiveDate;
    entities.StmtCouponSuppStatusHist.DiscontinueDate = discontinueDate;
    entities.StmtCouponSuppStatusHist.ReasonText = reasonText;
    entities.StmtCouponSuppStatusHist.CreatedBy = createdBy;
    entities.StmtCouponSuppStatusHist.CreatedTmst = createdTmst;
    entities.StmtCouponSuppStatusHist.OtyId = otyId;
    entities.StmtCouponSuppStatusHist.CpaTypeOblig = cpaTypeOblig;
    entities.StmtCouponSuppStatusHist.CspNumberOblig = cspNumberOblig;
    entities.StmtCouponSuppStatusHist.ObgId = obgId;
    entities.StmtCouponSuppStatusHist.DocTypeToSuppress = docTypeToSuppress;
    entities.StmtCouponSuppStatusHist.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "type", import.HardcodeCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.CreatedBy = db.GetString(reader, 2);
        entities.CsePersonAccount.CreatedTmst = db.GetDateTime(reader, 3);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadDefaultRule()
  {
    entities.DefaultRule.Populated = false;

    return Read("ReadDefaultRule",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DefaultRule.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DefaultRule.DtyGeneratedId = db.GetNullableInt32(reader, 1);
        entities.DefaultRule.EffectiveDate = db.GetDate(reader, 2);
        entities.DefaultRule.NegotiatedDate = db.GetNullableDate(reader, 3);
        entities.DefaultRule.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.DefaultRule.NonAdcArrearsMaxAmount =
          db.GetNullableDecimal(reader, 5);
        entities.DefaultRule.NonAdcArrearsAmount =
          db.GetNullableDecimal(reader, 6);
        entities.DefaultRule.NonAdcArrearsPercentage =
          db.GetNullableInt32(reader, 7);
        entities.DefaultRule.NonAdcCurrentMaxAmount =
          db.GetNullableDecimal(reader, 8);
        entities.DefaultRule.NonAdcCurrentAmount =
          db.GetNullableDecimal(reader, 9);
        entities.DefaultRule.NonAdcCurrentPercentage =
          db.GetNullableInt32(reader, 10);
        entities.DefaultRule.PassthruPercentage =
          db.GetNullableInt32(reader, 11);
        entities.DefaultRule.PassthruAmount = db.GetNullableDecimal(reader, 12);
        entities.DefaultRule.PassthruMaxAmount =
          db.GetNullableDecimal(reader, 13);
        entities.DefaultRule.CreatedBy = db.GetString(reader, 14);
        entities.DefaultRule.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.DefaultRule.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.DefaultRule.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 17);
        entities.DefaultRule.DefaultRuleFiller =
          db.GetNullableString(reader, 18);
        entities.DefaultRule.Type1 = db.GetString(reader, 19);
        entities.DefaultRule.RepaymentLetterPrintDate =
          db.GetNullableDate(reader, 20);
        entities.DefaultRule.Populated = true;
        CheckValid<RecaptureRule>("Type1", entities.DefaultRule.Type1);
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.ForeignFipsState = db.GetNullableInt32(reader, 1);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadObligorRule()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.ObligorRule.Populated = false;

    return Read("ReadObligorRule",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaDType", entities.CsePersonAccount.Type1);
        db.SetNullableString(
          command, "cspDNumber", entities.CsePersonAccount.CspNumber);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
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
        entities.ObligorRule.CreatedBy = db.GetString(reader, 15);
        entities.ObligorRule.CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.ObligorRule.LastUpdatedBy = db.GetNullableString(reader, 17);
        entities.ObligorRule.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 18);
        entities.ObligorRule.ObligorRuleFiller =
          db.GetNullableString(reader, 19);
        entities.ObligorRule.Type1 = db.GetString(reader, 20);
        entities.ObligorRule.RepaymentLetterPrintDate =
          db.GetNullableDate(reader, 21);
        entities.ObligorRule.Populated = true;
        CheckValid<RecaptureRule>("CpaDType", entities.ObligorRule.CpaDType);
        CheckValid<RecaptureRule>("Type1", entities.ObligorRule.Type1);
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          import.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Type1 = db.GetString(reader, 2);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      null,
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
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
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private void UpdatePaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var processDate = import.Current.Date;
    var systemGeneratedIdentifier =
      entities.PaymentRequest.SystemGeneratedIdentifier;

    entities.PaymentRequest.Populated = false;
    entities.Obligation.Populated = false;
    Update("UpdatePaymentRequest#1",
      (db, command) =>
      {
        db.SetDate(command, "processDate", processDate);
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
      });

    Update("UpdatePaymentRequest#2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "prqId", systemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.PaymentRequest.ProcessDate = processDate;
    entities.Obligation.PrqId = systemGeneratedIdentifier;
    entities.PaymentRequest.Populated = true;
    entities.Obligation.Populated = true;
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = import.Current.Date;

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
  protected readonly Local local;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of HcOtCVoluntaryClassif.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntaryClassif")]
    public ObligationType HcOtCVoluntaryClassif
    {
      get => hcOtCVoluntaryClassif ??= new();
      set => hcOtCVoluntaryClassif = value;
    }

    /// <summary>
    /// A value of HcOtCRecoverClassific.
    /// </summary>
    [JsonPropertyName("hcOtCRecoverClassific")]
    public ObligationType HcOtCRecoverClassific
    {
      get => hcOtCRecoverClassific ??= new();
      set => hcOtCRecoverClassific = value;
    }

    /// <summary>
    /// A value of HcOtCFeesClassificati.
    /// </summary>
    [JsonPropertyName("hcOtCFeesClassificati")]
    public ObligationType HcOtCFeesClassificati
    {
      get => hcOtCFeesClassificati ??= new();
      set => hcOtCFeesClassificati = value;
    }

    /// <summary>
    /// A value of HcOtCAccruingClassifi.
    /// </summary>
    [JsonPropertyName("hcOtCAccruingClassifi")]
    public ObligationType HcOtCAccruingClassifi
    {
      get => hcOtCAccruingClassifi ??= new();
      set => hcOtCAccruingClassifi = value;
    }

    /// <summary>
    /// A value of HardcodeCpaObligor.
    /// </summary>
    [JsonPropertyName("hardcodeCpaObligor")]
    public CsePersonAccount HardcodeCpaObligor
    {
      get => hardcodeCpaObligor ??= new();
      set => hardcodeCpaObligor = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of HistoryIndicator.
    /// </summary>
    [JsonPropertyName("historyIndicator")]
    public Common HistoryIndicator
    {
      get => historyIndicator ??= new();
      set => historyIndicator = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private LegalActionDetail legalActionDetail;
    private DateWorkArea max;
    private ObligationType hcOtCVoluntaryClassif;
    private ObligationType hcOtCRecoverClassific;
    private ObligationType hcOtCFeesClassificati;
    private ObligationType hcOtCAccruingClassifi;
    private CsePersonAccount hardcodeCpaObligor;
    private DateWorkArea current;
    private PaymentRequest paymentRequest;
    private Common historyIndicator;
    private LegalAction legalAction;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationType obligationType;
    private CsePerson supported;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Assign1.
    /// </summary>
    [JsonPropertyName("assign1")]
    public CsePersonsWorkSet Assign1
    {
      get => assign1 ??= new();
      set => assign1 = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private CsePersonsWorkSet assign1;
    private ServiceProvider serviceProvider;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of RecaptureInclusion.
    /// </summary>
    [JsonPropertyName("recaptureInclusion")]
    public RecaptureInclusion RecaptureInclusion
    {
      get => recaptureInclusion ??= new();
      set => recaptureInclusion = value;
    }

    /// <summary>
    /// A value of NextMonth.
    /// </summary>
    [JsonPropertyName("nextMonth")]
    public DateWorkArea NextMonth
    {
      get => nextMonth ??= new();
      set => nextMonth = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistoryId.
    /// </summary>
    [JsonPropertyName("paymentStatusHistoryId")]
    public SystemGenerated PaymentStatusHistoryId
    {
      get => paymentStatusHistoryId ??= new();
      set => paymentStatusHistoryId = value;
    }

    /// <summary>
    /// A value of CreateAttemptCount.
    /// </summary>
    [JsonPropertyName("createAttemptCount")]
    public Common CreateAttemptCount
    {
      get => createAttemptCount ??= new();
      set => createAttemptCount = value;
    }

    /// <summary>
    /// A value of Interstate.
    /// </summary>
    [JsonPropertyName("interstate")]
    public Common Interstate
    {
      get => interstate ??= new();
      set => interstate = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      recaptureInclusion = null;
      nextMonth = null;
      officeServiceProvider = null;
      office = null;
      paymentStatusHistoryId = null;
      createAttemptCount = null;
      work = null;
      interestSuppStatusHistory = null;
      blank = null;
    }

    private RecaptureInclusion recaptureInclusion;
    private DateWorkArea nextMonth;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private SystemGenerated paymentStatusHistoryId;
    private Common createAttemptCount;
    private Common interstate;
    private Common work;
    private InterestSuppStatusHistory interestSuppStatusHistory;
    private DateWorkArea blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of ObligorRule.
    /// </summary>
    [JsonPropertyName("obligorRule")]
    public RecaptureRule ObligorRule
    {
      get => obligorRule ??= new();
      set => obligorRule = value;
    }

    /// <summary>
    /// A value of DefaultRule.
    /// </summary>
    [JsonPropertyName("defaultRule")]
    public RecaptureRule DefaultRule
    {
      get => defaultRule ??= new();
      set => defaultRule = value;
    }

    /// <summary>
    /// A value of RecaptureInclusion.
    /// </summary>
    [JsonPropertyName("recaptureInclusion")]
    public RecaptureInclusion RecaptureInclusion
    {
      get => recaptureInclusion ??= new();
      set => recaptureInclusion = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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

    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private LegalActionDetail legalActionDetail;
    private ObligationType obligationType;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentRequest paymentRequest;
    private InterestSuppStatusHistory interestSuppStatusHistory;
    private RecaptureRule obligorRule;
    private RecaptureRule defaultRule;
    private RecaptureInclusion recaptureInclusion;
    private LegalAction legalAction;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePerson csePerson;
  }
#endregion
}
