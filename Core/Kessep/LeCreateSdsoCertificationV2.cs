// Program: LE_CREATE_SDSO_CERTIFICATION_V2, ID: 372661583, model: 746.
// Short name: SWE01737
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
/// A program: LE_CREATE_SDSO_CERTIFICATION_V2.
/// </para>
/// <para>
/// Determines if the obligor meets the threshold amount given the constraints 
/// of SDSO.  This action block creats the initial and ongoing SDSO records.
/// </para>
/// </summary>
[Serializable]
public partial class LeCreateSdsoCertificationV2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_SDSO_CERTIFICATION_V2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreateSdsoCertificationV2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreateSdsoCertificationV2.
  /// </summary>
  public LeCreateSdsoCertificationV2(IContext context, Import import,
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
    // ---------------------------------------------------------------
    // MAINTENANCE LOG
    // DATE		DEVELOPER
    // DESCRIPTION
    // 01/21/96	H HOOKS
    // INITIAL DEVELOPMENT
    // 11/25/96	govind
    // IDCR #247. Changed the limit to obligor level instead of
    // obligation level.
    // 06/18/97	govind
    // Prob Req   The 30-day delinquency period is no longer
    // applicable.
    // 03/18/98	rcg
    // Modify code to provide for skipping rollback and abort when
    // SSN is invalid.
    // 01/08-19,21/1999	P McElderry
    // Fixed existing logic; added functionality commiserate w/new
    // requirements.
    // Deleted unneccesary code.
    // Moved:  SSN validation and creation count to PRAD.
    // Added Service Provider logic.
    // Replaced and removed logic dealing with Program_Error,
    // Program_Control_Totals, and Program_Run.
    // Added logic for Bankruptcy exemption.
    // Maureen Brown, March, 1999 - Removed usage of
    // obligation arrears total, and replaced it with logic to check
    // debt details for arrears instead.
    // 04/20-23/1999	PMcElderry
    // Added logic for concurrent obligations.
    // 05/07/1999	PMcElderry
    // Added chapter 7 to existing exemption status; exclude
    // temporary debts.
    // 08/05/1999	PMcElderry
    // Added OBLIGATION TYPE to local group view; check for
    // open case status
    // 11/04/1999	PMcElderry
    // PR # 79004 - Logic to allow archiving
    // 12/16/1999	PMcElderry
    // PR # 81488 - regardless of case status, determine if any
    // debts are outstanding
    // 08/30/2000	PMcElderry
    // PR # 102336 - Removal of AMIN_ACTION_CERT_
    // OBLIGATION and ADM_ACT_CERT_DEBT_DETAIL
    // entity creation
    // 12/13/2000	E.Shirk
    // PR#108126  - Removed logic that determined which obligation of a primary 
    // / secondary combination held the highest debt load.   Added a statement
    // that skipped any secondary obligation
    // 01/08/2001	E.Shirk
    // PR#110339  -  Removed logic that bypassed temporary obligations.   Also 
    // changed Bankruptcy READ to a READ EACH and added necessary logic to
    // handle future discharge and future dismiss/withdraw dates.
    // 02/16/2001	E.Shirk
    // WR#000269   -  General overhaul that removed dead, redundant and 
    // inefficient code.
    // Added read each against ap case role that detected whether the AP had any
    // good cause actions from any AR.   This was desinged to save processing
    // time by not calling the le_check_good_cause_for_debt for non GC AP's.
    // Specific errors corrected include:
    // 1.  Discontinued use of current date and replaced with PPI date.
    // 2.  Debts where erroneously being considered for certification per 
    // obligation, not the total for the AP.
    // 3.  Removed the future bankruptcy logic.  This capability was removed 
    // from the BKRP online screen.
    // 4.  Process was erroneously skipping the entire AP when good cause was 
    // detected.   Only the debt should have been bypassed not the AP.
    // 09/10/07 M Fan
    // WR296917 Part A - AP's in bankruptcy must be certified for SDSO due to 
    // federal bankruptcy law changes.
    // Commented out the codes that checked for active bankruptcy.
    // 09/07/2010	GVandy
    // CQ 20550 - Do not certify debt amounts on Fee and Recovery obligations if
    // the obligor is in bankruptcy.
    // **********************************************************************************
    // Initialize action block variables.
    // **********************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.SdsoRecordCreated.Flag = "N";
    local.AdministrativeActCertification.Type1 = "SDSO";
    local.Process.Date = import.ProgramProcessingInfo.ProcessDate;
    local.TotalForObligor.ChildSupportRelatedAmount = 0;
    local.TotalForObligor.RecoveryAmount = 0;
    UseCabFirstAndLastDateOfMonth();

    // **********************************************************************************
    // Check for active bankruptcy.
    // **********************************************************************************
    local.Ch13Bankruptz.Flag = "";

    // 09/07/2010	GVandy
    // CQ 20550 - Re-enable bankruptcy check so that we do not certify debt 
    // amounts on Fee and Recovery obligations if the obligor is in bankruptcy.
    if (ReadBankruptcy())
    {
      if (AsChar(import.ExcludeBankruptcy.Flag) == 'Y')
      {
        if (Equal(entities.Bankruptcy.BankruptcyType, "13"))
        {
          local.Ch13Bankruptz.Flag = "Y";
          ExitState = "BANKRUPTCY_FILED";

          goto Read;
        }
      }

      // -- Obligor is currently in bankruptcy.
    }

Read:

    // **********************************************************************************
    // Get Obligor.
    // **********************************************************************************
    if (!ReadObligor())
    {
      export.KeyInfo.RptDetail = import.CsePerson.Number;
      ExitState = "CSE_PERSON_NOT_OBLIGOR";

      return;
    }

    // **********************************************************************************
    // The following read for good cause is for efficiency only.   Instead of 
    // checking each individual debt for good cause, let us first determine if
    // the AP has been marked good cause in any case.   Once we have determined
    // if he has been marked good cause in any case, then we can check the
    // individual debts.
    // **********************************************************************************
    local.GoodCauseFoundForAp.Flag = "N";

    foreach(var item in ReadCaseRole())
    {
      if (ReadGoodCause())
      {
        local.GoodCauseFoundForAp.Flag = "Y";

        break;
      }
    }

    // **********************************************************************************
    // Read all obligations tied to the obligor.
    // **********************************************************************************
    foreach(var item in ReadObligation())
    {
      if (AsChar(local.Ch13Bankruptz.Flag) == 'Y')
      {
        break;
      }

      // **********************************************************************************
      // Bypass secondary obligations.
      // **********************************************************************************
      if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'S')
      {
        continue;
      }

      // **********************************************************************************
      // Get obligation type tied to obligation
      // **********************************************************************************
      if (ReadObligationType())
      {
        // Continue
      }
      else
      {
        export.KeyInfo.RptDetail = entities.CsePerson.Number + "," + NumberToString
          (entities.Obligation.SystemGeneratedIdentifier,
          Verify(NumberToString(
            entities.Obligation.SystemGeneratedIdentifier, 15), "0"), 16 -
          Verify(NumberToString(
            entities.Obligation.SystemGeneratedIdentifier, 15), "0"));
        ExitState = "FN0000_OBLIGATION_TYPE_NF";

        return;
      }

      // 09/07/2010	GVandy
      // CQ 20550 - Do not certify debt amounts on Fee and Recovery obligations 
      // if the obligor is in bankruptcy.
      if (entities.Bankruptcy.Populated && (
        AsChar(entities.ObligationType.Classification) == 'F' || AsChar
        (entities.ObligationType.Classification) == 'R'))
      {
        continue;
      }

      // **********************************************************************************
      // Determine if obligation type is SDSO certifiable.
      // **********************************************************************************
      UseLeDetermObligTypeCertifiable();

      if (AsChar(local.ObligationTypeCertifiabl.Flag) == 'N')
      {
        continue;
      }

      // **********************************************************************************
      // Determine if obligation is currently exempted.
      // **********************************************************************************
      foreach(var item1 in ReadObligationAdmActionExemption())
      {
        // -------------------------------------------------------------
        // An exemption is in place - skip this obligation.
        // -------------------------------------------------------------
        goto ReadEach;
      }

      // **********************************************************************************
      // Sum up eligible arrears debts tied to the valid obligation
      // **********************************************************************************
      foreach(var item1 in ReadDebtDebtDetail())
      {
        if (entities.DebtDetail.BalanceDueAmt == 0 && Equal
          (entities.DebtDetail.InterestBalanceDueAmt, 0))
        {
          continue;
        }

        if (AsChar(entities.ObligationType.Classification) == 'A')
        {
          if (Lt(entities.DebtDetail.DueDt, local.FirstDayOfMonth.Date))
          {
            // Arrears debt - continue
          }
          else
          {
            // Debt is not in arrears - go to next one.
            continue;
          }
        }
        else
        {
          // Arrears debt - continue
        }

        // **********************************************************************************
        // Check for good cause exemption tied to the debt
        // **********************************************************************************
        if (AsChar(local.GoodCauseFoundForAp.Flag) == 'Y')
        {
          UseLeCheckGoodCauseForDebt();

          if (AsChar(local.GoodCauseFoundForDebt.Flag) == 'Y')
          {
            continue;
          }
        }

        // **********************************************************************************
        // Sub-total the valid debt.
        // **********************************************************************************
        if (AsChar(local.CsObligIndicator.Flag) == 'Y')
        {
          local.TotalForObligor.ChildSupportRelatedAmount =
            local.TotalForObligor.ChildSupportRelatedAmount + entities
            .DebtDetail.BalanceDueAmt + entities
            .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        }

        if (AsChar(local.RecoveryObligIndicator.Flag) == 'Y')
        {
          local.TotalForObligor.RecoveryAmount =
            local.TotalForObligor.RecoveryAmount.GetValueOrDefault() + entities
            .DebtDetail.BalanceDueAmt + entities
            .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        }
      }

ReadEach:
      ;
    }

    // **********************************************************************************
    // Check total debt threshold amount for certificiaton.
    // **********************************************************************************
    if (local.TotalForObligor.ChildSupportRelatedAmount + local
      .TotalForObligor.RecoveryAmount.GetValueOrDefault() >= 25M)
    {
      // **********************************************************************************
      // Determine if AP has been previously certified for SDSO.
      // **********************************************************************************
      if (ReadAdministrativeActCertification())
      {
        local.PriorSdso.Flag = "Y";
        local.Sdso.OriginalAmount =
          local.Sdso.CurrentAmount.GetValueOrDefault();
      }
      else
      {
        local.PriorSdso.Flag = "N";
      }

      // **********************************************************************************
      // Set original and current amounts.
      // **********************************************************************************
      if (AsChar(local.PriorSdso.Flag) == 'Y')
      {
        local.Sdso.OriginalAmount = entities.StateDebtSetoff.OriginalAmount;
        local.Sdso.CurrentAmount =
          local.TotalForObligor.ChildSupportRelatedAmount + local
          .TotalForObligor.RecoveryAmount.GetValueOrDefault();
      }
      else
      {
        local.Sdso.CurrentAmount =
          local.TotalForObligor.ChildSupportRelatedAmount + local
          .TotalForObligor.RecoveryAmount.GetValueOrDefault();
        local.Sdso.OriginalAmount =
          local.Sdso.CurrentAmount.GetValueOrDefault();
      }

      // **********************************************************************************
      // Build the certification row.
      // **********************************************************************************
      if (ReadAdministrativeAction())
      {
        try
        {
          CreateStateDebtSetoff();
          export.SdsoRecordCreated.Flag = "Y";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.DateConversion.WorkDate =
                DateToInt(entities.StateDebtSetoff.TakenDate);
              local.DateConversion.TextDate =
                NumberToString(local.DateConversion.WorkDate,
                Verify(NumberToString(local.DateConversion.WorkDate, 15), "0"),
                16 -
                Verify(NumberToString(local.DateConversion.WorkDate, 15), "0"));
                
              export.KeyInfo.RptDetail = local.DateConversion.TextDate + "," + "SDSO"
                + "," + entities.CsePerson.Number;
              ExitState = "STATE_DEBT_SETOFF_AE";

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
        ExitState = "ADMINISTRATIVE_ACTION_NF";
      }
    }
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.Code = source.Code;
    target.Classification = source.Classification;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Process.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.FirstDayOfMonth.Date = useExport.First.Date;
  }

  private void UseLeCheckGoodCauseForDebt()
  {
    var useImport = new LeCheckGoodCauseForDebt.Import();
    var useExport = new LeCheckGoodCauseForDebt.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.Debt.SystemGeneratedIdentifier =
      entities.Debt.SystemGeneratedIdentifier;
    useImport.Obligor.Number = import.CsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(LeCheckGoodCauseForDebt.Execute, useImport, useExport);

    local.GoodCauseFoundForDebt.Flag = useExport.GoodCauseFoundForDebt.Flag;
  }

  private void UseLeDetermObligTypeCertifiable()
  {
    var useImport = new LeDetermObligTypeCertifiable.Import();
    var useExport = new LeDetermObligTypeCertifiable.Export();

    useImport.AdministrativeActCertification.Type1 =
      local.AdministrativeActCertification.Type1;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);

    Call(LeDetermObligTypeCertifiable.Execute, useImport, useExport);

    local.CsObligIndicator.Flag = useExport.CsObligIndicator.Flag;
    local.RecoveryObligIndicator.Flag = useExport.RecoveryObligIndicator.Flag;
    local.ObligationTypeCertifiabl.Flag =
      useExport.ObligationTypeCertifiab.Flag;
  }

  private void CreateStateDebtSetoff()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);

    var cpaType = entities.Obligor.Type1;
    var cspNumber = entities.Obligor.CspNumber;
    var type1 = "SDSO";
    var takenDate = import.ProgramProcessingInfo.ProcessDate;
    var aatType = entities.AdministrativeAction.Type1;
    var originalAmount = local.Sdso.OriginalAmount.GetValueOrDefault();
    var currentAmount = local.Sdso.CurrentAmount.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var recoveryAmount =
      local.TotalForObligor.RecoveryAmount.GetValueOrDefault();
    var childSupportRelatedAmount =
      local.TotalForObligor.ChildSupportRelatedAmount;
    var dateSent = local.Null1.Date;

    CheckValid<AdministrativeActCertification>("CpaType", cpaType);
    CheckValid<AdministrativeActCertification>("Type1", type1);
    entities.StateDebtSetoff.Populated = false;
    Update("CreateStateDebtSetoff",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetDate(command, "takenDt", takenDate);
        db.SetNullableString(command, "aatType", aatType);
        db.SetNullableDecimal(command, "originalAmt", originalAmount);
        db.SetNullableDecimal(command, "currentAmt", currentAmount);
        db.SetNullableDate(command, "currentAmtDt", takenDate);
        db.SetNullableDate(command, "decertifiedDt", null);
        db.SetNullableDate(command, "notificationDt", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", createdTstamp);
        db.SetNullableDate(command, "referralDt", default(DateTime));
        db.SetNullableDecimal(command, "recoveryAmt", recoveryAmount);
        db.SetDecimal(command, "childSuppRelAmt", childSupportRelatedAmount);
        db.SetNullableDecimal(command, "adcAmt", 0M);
        db.SetString(command, "witness", "");
        db.SetNullableString(command, "notifiedBy", "");
        db.SetNullableString(command, "reasonWithdraw", "");
        db.SetNullableString(command, "denialReason", "");
        db.SetNullableDate(command, "dateSent", dateSent);
        db.SetNullableString(command, "etypeAdminOffset", "");
        db.SetNullableString(command, "localCode", "");
        db.SetInt32(command, "ssn", 0);
        db.SetString(command, "caseNumber", "");
        db.SetString(command, "lastName", "");
        db.SetInt32(command, "amountOwed", 0);
        db.SetNullableString(command, "transferState", "");
        db.SetNullableInt32(command, "localForTransfer", 0);
        db.SetNullableInt32(command, "processYear", 0);
        db.SetString(command, "tanfCode", "");
        db.SetNullableString(command, "decertifyReason", "");
        db.SetNullableString(command, "addressStreet1", "");
        db.SetNullableString(command, "addressCity", "");
        db.SetNullableString(command, "addressZip", "");
      });

    entities.StateDebtSetoff.CpaType = cpaType;
    entities.StateDebtSetoff.CspNumber = cspNumber;
    entities.StateDebtSetoff.Type1 = type1;
    entities.StateDebtSetoff.TakenDate = takenDate;
    entities.StateDebtSetoff.AatType = aatType;
    entities.StateDebtSetoff.OriginalAmount = originalAmount;
    entities.StateDebtSetoff.CurrentAmount = currentAmount;
    entities.StateDebtSetoff.CurrentAmountDate = takenDate;
    entities.StateDebtSetoff.DecertifiedDate = null;
    entities.StateDebtSetoff.NotificationDate = null;
    entities.StateDebtSetoff.CreatedBy = createdBy;
    entities.StateDebtSetoff.CreatedTstamp = createdTstamp;
    entities.StateDebtSetoff.LastUpdatedBy = createdBy;
    entities.StateDebtSetoff.LastUpdatedTstamp = createdTstamp;
    entities.StateDebtSetoff.RecoveryAmount = recoveryAmount;
    entities.StateDebtSetoff.ChildSupportRelatedAmount =
      childSupportRelatedAmount;
    entities.StateDebtSetoff.NotifiedBy = "";
    entities.StateDebtSetoff.DateSent = dateSent;
    entities.StateDebtSetoff.TanfCode = "";
    entities.StateDebtSetoff.DecertificationReason = "";
    entities.StateDebtSetoff.Populated = true;
  }

  private bool ReadAdministrativeActCertification()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetNullableDate(
          command, "dateSent", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeActCertification.CpaType =
          db.GetString(reader, 0);
        entities.AdministrativeActCertification.CspNumber =
          db.GetString(reader, 1);
        entities.AdministrativeActCertification.Type1 = db.GetString(reader, 2);
        entities.AdministrativeActCertification.TakenDate =
          db.GetDate(reader, 3);
        entities.AdministrativeActCertification.OriginalAmount =
          db.GetNullableDecimal(reader, 4);
        entities.AdministrativeActCertification.DateSent =
          db.GetNullableDate(reader, 5);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 6);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      null,
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private bool ReadBankruptcy()
  {
    entities.Bankruptcy.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "dischargeDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Bankruptcy.CspNumber = db.GetString(reader, 0);
        entities.Bankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.Bankruptcy.BankruptcyType = db.GetString(reader, 2);
        entities.Bankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 3);
        entities.Bankruptcy.BankruptcyDismissWithdrawDate =
          db.GetNullableDate(reader, 4);
        entities.Bankruptcy.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.Ap.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ap.CasNumber = db.GetString(reader, 0);
        entities.Ap.CspNumber = db.GetString(reader, 1);
        entities.Ap.Type1 = db.GetString(reader, 2);
        entities.Ap.Identifier = db.GetInt32(reader, 3);
        entities.Ap.StartDate = db.GetNullableDate(reader, 4);
        entities.Ap.EndDate = db.GetNullableDate(reader, 5);
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ap.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "dueDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.DebtType = db.GetString(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 11);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 12);
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.Ap.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", entities.Ap.CasNumber);
        db.SetNullableInt32(command, "croIdentifier1", entities.Ap.Identifier);
        db.SetNullableString(command, "croType1", entities.Ap.Type1);
        db.SetNullableString(command, "cspNumber1", entities.Ap.CspNumber);
        db.SetNullableDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GoodCause.CasNumber = db.GetString(reader, 4);
        entities.GoodCause.CspNumber = db.GetString(reader, 5);
        entities.GoodCause.CroType = db.GetString(reader, 6);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 8);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 9);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 10);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 11);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationAdmActionExemption()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationAdmActionExemption.Populated = false;

    return ReadEach("ReadObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "effectiveDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.
          SetDate(command, "endDt", local.NullDate.EndDate.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Description =
          db.GetNullableString(reader, 7);
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);

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
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadObligor()
  {
    entities.Obligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of ExcludeBankruptcy.
    /// </summary>
    [JsonPropertyName("excludeBankruptcy")]
    public Common ExcludeBankruptcy
    {
      get => excludeBankruptcy ??= new();
      set => excludeBankruptcy = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson csePerson;
    private Common excludeBankruptcy;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of KeyInfo.
    /// </summary>
    [JsonPropertyName("keyInfo")]
    public EabReportSend KeyInfo
    {
      get => keyInfo ??= new();
      set => keyInfo = value;
    }

    /// <summary>
    /// A value of SdsoRecordCreated.
    /// </summary>
    [JsonPropertyName("sdsoRecordCreated")]
    public Common SdsoRecordCreated
    {
      get => sdsoRecordCreated ??= new();
      set => sdsoRecordCreated = value;
    }

    private EabReportSend keyInfo;
    private Common sdsoRecordCreated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Ch13Bankruptz.
    /// </summary>
    [JsonPropertyName("ch13Bankruptz")]
    public Common Ch13Bankruptz
    {
      get => ch13Bankruptz ??= new();
      set => ch13Bankruptz = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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

    /// <summary>
    /// A value of FirstDayOfMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfMonth")]
    public DateWorkArea FirstDayOfMonth
    {
      get => firstDayOfMonth ??= new();
      set => firstDayOfMonth = value;
    }

    /// <summary>
    /// A value of PriorSdso.
    /// </summary>
    [JsonPropertyName("priorSdso")]
    public Common PriorSdso
    {
      get => priorSdso ??= new();
      set => priorSdso = value;
    }

    /// <summary>
    /// A value of Sdso.
    /// </summary>
    [JsonPropertyName("sdso")]
    public AdministrativeActCertification Sdso
    {
      get => sdso ??= new();
      set => sdso = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of TotalForObligor.
    /// </summary>
    [JsonPropertyName("totalForObligor")]
    public AdministrativeActCertification TotalForObligor
    {
      get => totalForObligor ??= new();
      set => totalForObligor = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public ObligationAdmActionExemption NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of ObligationTypeCertifiabl.
    /// </summary>
    [JsonPropertyName("obligationTypeCertifiabl")]
    public Common ObligationTypeCertifiabl
    {
      get => obligationTypeCertifiabl ??= new();
      set => obligationTypeCertifiabl = value;
    }

    /// <summary>
    /// A value of RecoveryObligIndicator.
    /// </summary>
    [JsonPropertyName("recoveryObligIndicator")]
    public Common RecoveryObligIndicator
    {
      get => recoveryObligIndicator ??= new();
      set => recoveryObligIndicator = value;
    }

    /// <summary>
    /// A value of CsObligIndicator.
    /// </summary>
    [JsonPropertyName("csObligIndicator")]
    public Common CsObligIndicator
    {
      get => csObligIndicator ??= new();
      set => csObligIndicator = value;
    }

    /// <summary>
    /// A value of GoodCauseFoundForDebt.
    /// </summary>
    [JsonPropertyName("goodCauseFoundForDebt")]
    public Common GoodCauseFoundForDebt
    {
      get => goodCauseFoundForDebt ??= new();
      set => goodCauseFoundForDebt = value;
    }

    /// <summary>
    /// A value of GoodCauseFoundForAp.
    /// </summary>
    [JsonPropertyName("goodCauseFoundForAp")]
    public Common GoodCauseFoundForAp
    {
      get => goodCauseFoundForAp ??= new();
      set => goodCauseFoundForAp = value;
    }

    /// <summary>
    /// A value of DateConversion.
    /// </summary>
    [JsonPropertyName("dateConversion")]
    public OblgWork DateConversion
    {
      get => dateConversion ??= new();
      set => dateConversion = value;
    }

    private Common ch13Bankruptz;
    private DateWorkArea process;
    private DateWorkArea null1;
    private DateWorkArea firstDayOfMonth;
    private Common priorSdso;
    private AdministrativeActCertification sdso;
    private AdministrativeActCertification administrativeActCertification;
    private AdministrativeActCertification totalForObligor;
    private ObligationAdmActionExemption nullDate;
    private Common obligationTypeCertifiabl;
    private Common recoveryObligIndicator;
    private Common csObligIndicator;
    private Common goodCauseFoundForDebt;
    private Common goodCauseFoundForAp;
    private OblgWork dateConversion;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of AdministrativeActCertification.
    /// </summary>
    [JsonPropertyName("administrativeActCertification")]
    public AdministrativeActCertification AdministrativeActCertification
    {
      get => administrativeActCertification ??= new();
      set => administrativeActCertification = value;
    }

    /// <summary>
    /// A value of StateDebtSetoff.
    /// </summary>
    [JsonPropertyName("stateDebtSetoff")]
    public AdministrativeActCertification StateDebtSetoff
    {
      get => stateDebtSetoff ??= new();
      set => stateDebtSetoff = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private GoodCause goodCause;
    private CaseRole ap;
    private AdministrativeActCertification administrativeActCertification;
    private AdministrativeActCertification stateDebtSetoff;
    private Bankruptcy bankruptcy;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeAction administrativeAction;
  }
#endregion
}
