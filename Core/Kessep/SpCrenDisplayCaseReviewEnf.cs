// Program: SP_CREN_DISPLAY_CASE_REVIEW_ENF, ID: 374546388, model: 746.
// Short name: SWE03128
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREN_DISPLAY_CASE_REVIEW_ENF.
/// </summary>
[Serializable]
public partial class SpCrenDisplayCaseReviewEnf: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREN_DISPLAY_CASE_REVIEW_ENF program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrenDisplayCaseReviewEnf(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrenDisplayCaseReviewEnf.
  /// </summary>
  public SpCrenDisplayCaseReviewEnf(IContext context, Import import,
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
    // ******************************************************************
    // This action block contains the Display logic for the CREN screen.
    // It was written as part of an overhaul to the CREN screen to fix
    // multiple issues, including CQ# 9346 and CQ# 365.
    // ******************************************************************
    // ------------------------------------------------------------------
    //                   M A I N T E N A N C E   L O G
    // Date		Developer	Request #	Description
    // 03/11/10	J Huss		CQ# 9346 & 365	Initial development.
    // 08/13/10	J Huss		CQ# 19624	Moved location of monthly due amount 
    // initialization into
    // 						standard number initialization section.
    // 08/17/10	J Huss		CQ# 21355	Display ??Yr for child's age if the birthdate 
    // is unknown.
    // 04/01/2011	T Pierce	CQ# 23212	Removed references to obsolete Narrative 
    // entity type.
    // ------------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Min.Date = new DateTime(1, 1, 1);
    local.Max.Date = new DateTime(2099, 12, 31);

    // Verify that the case number passed in is valid.
    if (ReadCase1())
    {
      // If the case is closed, use the closed date as the processing date.  
      // Otherwise, use the current date.
      if (AsChar(entities.Case1.Status) == 'C')
      {
        local.Process.Date = entities.Case1.StatusDate;
      }
      else
      {
        local.Process.Date = local.Current.Date;
      }

      MoveCase1(entities.Case1, export.Case1);
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // ********************
    // Get AR details
    // ********************
    if (ReadCsePerson1())
    {
      export.Ar.Number = entities.CsePerson.Number;

      // Get AR's formatted name
      UseSiReadCsePerson3();

      if (!IsEmpty(local.AbendData.Type1))
      {
        ExitState = "ACO_ADABAS_UNAVAILABLE";

        return;
      }

      // Determine the AR's monthly income
      export.ArMonthlyIncome.TotalCurrency = 0;

      foreach(var item in ReadIncomeSource2())
      {
        // Find the most recent income history for this income source
        foreach(var item1 in ReadPersonIncomeHistory())
        {
          UseCabComputeAvgMonthlyIncome();
          export.ArMonthlyIncome.TotalCurrency += local.Temp.TotalCurrency;

          goto ReadEach1;
        }

ReadEach1:
        ;
      }
    }

    // ********************
    // Get AP details
    // ********************
    // If the AP is provided, use it.  Otherwise, find the AP that was added to 
    // the case first.
    if (!IsEmpty(import.SelectedAp.Number))
    {
      if (ReadCsePerson2())
      {
        local.Ap.Assign(entities.CsePerson);
      }

      export.Ap.Number = import.SelectedAp.Number;
    }
    else
    {
      // Find the first AP added to the case that is still active as of the 
      // processing date.
      if (ReadCsePersonCaseRoleCase())
      {
        local.Ap.Assign(entities.CsePerson);
        export.Ap.Number = entities.CsePerson.Number;
      }
    }

    // Get AP's formatted name if one was found.
    if (!IsEmpty(export.Ap.Number))
    {
      UseSiReadCsePerson2();

      if (!IsEmpty(local.AbendData.Type1))
      {
        ExitState = "ACO_ADABAS_UNAVAILABLE";

        return;
      }
    }

    // If the unemployment_ind is Y, then return Y.  If it's blank or N, then 
    // return N.
    if (AsChar(local.Ap.UnemploymentInd) == 'Y')
    {
      export.CsePerson.UnemploymentInd = "Y";
    }
    else
    {
      export.CsePerson.UnemploymentInd = "N";
    }

    // ********************
    // Get child details
    // ********************
    export.AgesOfChildren.Index = -1;
    export.NumberOfChildren.Count = 0;

    foreach(var item in ReadCsePerson3())
    {
      ++export.AgesOfChildren.Index;
      export.AgesOfChildren.CheckSize();

      if (export.AgesOfChildren.Index >= Export.AgesOfChildrenGroup.Capacity)
      {
        break;
      }

      local.Child.Number = entities.CsePerson.Number;

      // Find the child's date of birth
      UseSiReadCsePerson1();

      if (!IsEmpty(local.AbendData.Type1))
      {
        ExitState = "ACO_ADABAS_UNAVAILABLE";

        return;
      }

      UseCabCalcCurrentAgeFromDob();

      if (Equal(local.Child.Dob, null))
      {
        // The birthdate is unknown.  Display ??Yr so the worker knows the 
        // birthdate is missing.
        export.AgesOfChildren.Update.AgeOfChild.Text4 = "??Yr";
      }
      else if (local.ChildsAge.TotalInteger > 0)
      {
        // If the child is at least a year old, display the age as ##Yr.  
        // Otherwise, determine age in months.
        export.AgesOfChildren.Update.AgeOfChild.Text4 =
          NumberToString(local.ChildsAge.TotalInteger, 14, 2) + "Yr";
      }
      else
      {
        // If the child was born this year, subtract the current month from the 
        // month of birth to determine
        // the total months since birth.
        if (Year(local.Child.Dob) == Year(local.Current.Date))
        {
          local.ChildsAge.TotalInteger = Month(local.Current.Date) - Month
            (local.Child.Dob);
        }
        else
        {
          // The child was born last year.  Subtract the birth month from 12 to 
          // determine the number of months the
          // child was alive last year, then add the current month to determine 
          // the total months since birth.
          local.ChildsAge.TotalInteger = 12 - Month(local.Child.Dob) + Month
            (local.Current.Date);
        }

        // Display the age as ##Mo
        export.AgesOfChildren.Update.AgeOfChild.Text4 =
          NumberToString(local.ChildsAge.TotalInteger, 14, 2) + "Mo";
      }

      ++export.NumberOfChildren.Count;
    }

    // ********************
    // Get number of APs
    // ********************
    export.NumberOfAps.Count = 0;

    foreach(var item in ReadCaseRole())
    {
      ++export.NumberOfAps.Count;
    }

    // ********************
    // Get AR modification denial date
    // ********************
    if (ReadInfrastructure2())
    {
      export.ArLastModificationDen.Date =
        Date(entities.Infrastructure.CreatedTimestamp);
    }

    // ********************
    // Get last modification request date
    // ********************
    if (ReadInfrastructure4())
    {
      export.LastModificationRequest.Date =
        Date(entities.Infrastructure.CreatedTimestamp);
    }

    // ********************
    // Get narrative detail
    // ********************
    // Find any narrative details that match the imported Infrastructure ID.  
    // Sort them by line number
    // and compile them into one large string.
    export.ReviewNote.Text = "";
    local.Temp.Count = 0;

    foreach(var item in ReadNarrativeDetail())
    {
      // Narrative details are used by many other screens, so we must check to 
      // see if this is an enforcement review detail.
      if (Equal(entities.NarrativeDetail.NarrativeText, 1, 14, "ENFORCEMENT --"))
        
      {
        export.ReviewNote.Text = TrimEnd(export.ReviewNote.Text) + Substring
          (entities.NarrativeDetail.NarrativeText,
          NarrativeDetail.NarrativeText_MaxLength, 16, 53);

        // The field on the screen can only handle 4 lines of narrative, so stop
        // reading at that point
        ++local.Temp.Count;

        if (local.Temp.Count >= 4)
        {
          break;
        }
      }
    }

    // If no AP was found then there is no further information to retrieve.
    if (IsEmpty(export.Ap.Number))
    {
      return;
    }

    // ********************
    // Get AP's monthly income
    // ********************
    export.ApMonthlyIncome.TotalCurrency = 0;

    foreach(var item in ReadIncomeSource1())
    {
      // Find the most recent income history for this income source
      foreach(var item1 in ReadPersonIncomeHistory())
      {
        UseCabComputeAvgMonthlyIncome();
        export.ApMonthlyIncome.TotalCurrency += local.Temp.TotalCurrency;

        goto ReadEach2;
      }

ReadEach2:
      ;
    }

    // ********************
    // Get last modification review date
    // ********************
    if (ReadInfrastructure3())
    {
      export.LastReviewDate.Date =
        Date(entities.Infrastructure.CreatedTimestamp);
    }

    // ********************
    // Get AP modification denial date
    // ********************
    if (ReadInfrastructure1())
    {
      export.ApLastModificationDen.Date =
        Date(entities.Infrastructure.CreatedTimestamp);
    }

    // ********************
    // Get last income increase date
    // ********************
    if (ReadInfrastructure5())
    {
      export.LastIncomeIncrease.Date =
        Date(entities.Infrastructure.CreatedTimestamp);
    }

    // ********************
    // Get last legal referral date
    // ********************
    foreach(var item in ReadLegalReferral())
    {
      // If the case is open, but the legal referral is closed or withdrawn, see
      // if there are other referrals.
      if (AsChar(export.Case1.Status) == 'O' && (
        AsChar(entities.LegalReferral.Status) == 'C' || AsChar
        (entities.LegalReferral.Status) == 'W'))
      {
        continue;
      }

      export.Last.ReferralDate = entities.LegalReferral.ReferralDate;

      break;
    }

    // ********************
    // Determine if AP is known to other cases
    // ********************
    local.Temp.Count = 0;
    export.ApKnownToOtherCases.Flag = "N";

    foreach(var item in ReadCase2())
    {
      ++local.Temp.Count;

      if (local.Temp.Count > 1)
      {
        export.ApKnownToOtherCases.Flag = "Y";

        break;
      }
    }

    // ********************
    // Get Good Cause code
    // ********************
    if (ReadGoodCause())
    {
      export.GoodCause.Code = entities.GoodCause.Code;
    }

    // ********************
    // Get last Bankruptcy date
    // ********************
    if (ReadBankruptcy())
    {
      export.Bankruptcy.BankruptcyFilingDate =
        entities.Bankruptcy.BankruptcyFilingDate;
    }

    // ********************
    // Get FDSO flag
    // ********************
    // Find any FDSO Admin Action Certifications against this AP
    export.FdsoCertification.Flag = "N";

    if (ReadAdministrativeActCertification1())
    {
      // The most recent FDSO record is for decertification.
      if (Lt(local.Min.Date,
        entities.AdministrativeActCertification.DecertifiedDate))
      {
        goto Read1;
      }

      // Check to see if AP has been given exemtpion for FDSO on *any* 
      // obligation.
      if (ReadObligationAdmActionExemption2())
      {
        // AP has been exempted from FDSO on one obligation.
        goto Read1;
      }

      export.FdsoCertification.Flag = "Y";
    }

Read1:

    // ********************
    // Get SDSO flag
    // ********************
    // Find any SDSO Admin Action Certifications against this AP
    export.SdsoCertification.Flag = "N";

    if (ReadAdministrativeActCertification2())
    {
      // The most recent SDSO record is for decertification.
      if (Lt(local.Min.Date,
        entities.AdministrativeActCertification.DecertifiedDate))
      {
        goto Read2;
      }

      // Check to see if AP has been given exemtpion for FDSO on *any* 
      // obligation.
      if (ReadObligationAdmActionExemption1())
      {
        // AP has been exempted from SDSO on one obligation.
        goto Read2;
      }

      export.SdsoCertification.Flag = "Y";
    }

Read2:

    // ********************
    // Get CRED flag
    // ********************
    // Find any credit reporting actions against this AP.
    export.CredCertification.Flag = "N";

    foreach(var item in ReadCreditReportingAction())
    {
      // If the most recent reporting action is a cancelation or deletion, 
      // indicate N on the screen
      if (Equal(entities.CreditReportingAction.CseActionCode, "CAN") || Equal
        (entities.CreditReportingAction.CseActionCode, "DEL") || Equal
        (entities.CreditReportingAction.CseActionCode, "XAD") || Equal
        (entities.CreditReportingAction.CseActionCode, "XBR") || Equal
        (entities.CreditReportingAction.CseActionCode, "XGC"))
      {
        // Indicator is set to N before the READ EACH, no need to set it again.
        break;
      }
      else if (Equal(entities.CreditReportingAction.CseActionCode, "UPD") || Equal
        (entities.CreditReportingAction.CseActionCode, "ISS") || Equal
        (entities.CreditReportingAction.CseActionCode, "REL"))
      {
        // If the most recent reporting action is an issue or update, indicate Y
        // on the screen
        export.CredCertification.Flag = "Y";

        break;
      }
    }

    // ********************
    // Get finance group details
    // ********************
    // Find all of the legal actions that are associated to the AP and the case.
    export.FinanceDetails.Index = -1;

    foreach(var item in ReadLegalAction1())
    {
      // Only display legal actions that have court order numbers.
      if (IsEmpty(entities.LegalAction.StandardNumber))
      {
        continue;
      }

      // If the standard number has changed, move to the next group record and 
      // set values.
      if (!Equal(entities.LegalAction.StandardNumber,
        local.Previous.StandardNumber))
      {
        ++export.FinanceDetails.Index;
        export.FinanceDetails.CheckSize();

        if (export.FinanceDetails.Index >= Export.FinanceDetailsGroup.Capacity)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";

          break;
        }

        export.FinanceDetails.Update.LegalAction.Identifier =
          entities.LegalAction.Identifier;
        export.FinanceDetails.Update.LegalAction.StandardNumber =
          entities.LegalAction.StandardNumber;
        local.Previous.StandardNumber = entities.LegalAction.StandardNumber;

        // Initialize monthly due amount for this standard number.
        export.FinanceDetails.Update.MonthlyDue.TotalCurrency = 0;

        // Check to see if an IWO or MWO has been filed under the same standard 
        // number
        // where the AP is the obligor.
        export.FinanceDetails.Update.MwoDate.Date = local.Null1.Date;
        export.FinanceDetails.Update.IwoDate.Date = local.Null1.Date;

        foreach(var item1 in ReadLegalAction2())
        {
          // If this is the first IWO found, use it's filed date
          if (Equal(entities.Withholding.ActionTaken, "IWO") && Equal
            (export.FinanceDetails.Item.IwoDate.Date, local.Null1.Date))
          {
            export.FinanceDetails.Update.IwoDate.Date =
              entities.Withholding.FiledDate;
          }

          // If this is the first MWO found, use it's filed date
          if (Equal(entities.Withholding.ActionTaken, "MWO") && Equal
            (export.FinanceDetails.Item.MwoDate.Date, local.Null1.Date))
          {
            export.FinanceDetails.Update.MwoDate.Date =
              entities.Withholding.FiledDate;
          }

          // If dates have been found for both the IWO and MWO, no need to 
          // continue processing.
          if (!Equal(export.FinanceDetails.Item.IwoDate.Date, local.Null1.Date) &&
            !Equal(export.FinanceDetails.Item.MwoDate.Date, local.Null1.Date))
          {
            break;
          }
        }

        // Find the last payment made by this obligor for this standard number
        if (ReadCashReceiptDetail())
        {
          export.FinanceDetails.Update.LastPayment.Date =
            entities.CashReceiptDetail.CollectionDate;
        }
      }

      // Find the current monthly due amount for all of the obligations related 
      // to this legal action
      // where this AP is the obligor
      foreach(var item1 in ReadObligationObligationTypeCsePerson())
      {
        UseFnCalcAmtsDueForObligation();
        export.FinanceDetails.Update.MonthlyDue.TotalCurrency =
          export.FinanceDetails.Item.MonthlyDue.TotalCurrency + local
          .ScreenDueAmounts.TotalAmountDue;
      }

      // Find the current balance due by this AP for all of the obligations 
      // related to this legal action.
      UseFnComputeSummaryTotals();
      export.FinanceDetails.Update.Payoff.TotalCurrency =
        export.FinanceDetails.Item.Payoff.TotalCurrency + local
        .ScreenOwedAmounts.TotalAmountOwed;
    }

    // If the list can't hold one more entry then there's no need to continue 
    // processing.
    if (export.FinanceDetails.Index + 2 > Export.FinanceDetailsGroup.Capacity)
    {
      ExitState = "ACO_NI0000_LST_RETURNED_FULL";

      return;
    }

    // Find the most recent payment made by this AP that is not associated to a 
    // court order.
    if (ReadCashReceiptDetailCashReceiptSourceType())
    {
      ++export.FinanceDetails.Index;
      export.FinanceDetails.CheckSize();

      export.FinanceDetails.Update.LegalAction.StandardNumber =
        entities.CashReceiptSourceType.Code;
      export.FinanceDetails.Update.LastPayment.Date =
        entities.CashReceiptDetail.CollectionDate;
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseCabCalcCurrentAgeFromDob()
  {
    var useImport = new CabCalcCurrentAgeFromDob.Import();
    var useExport = new CabCalcCurrentAgeFromDob.Export();

    useImport.CsePersonsWorkSet.Dob = local.Child.Dob;

    Call(CabCalcCurrentAgeFromDob.Execute, useImport, useExport);

    local.ChildsAge.TotalInteger = useExport.Common.TotalInteger;
  }

  private void UseCabComputeAvgMonthlyIncome()
  {
    var useImport = new CabComputeAvgMonthlyIncome.Import();
    var useExport = new CabComputeAvgMonthlyIncome.Export();

    useImport.New1.Assign(entities.PersonIncomeHistory);

    Call(CabComputeAvgMonthlyIncome.Execute, useImport, useExport);

    local.Temp.TotalCurrency = useExport.Common.TotalCurrency;
  }

  private void UseFnCalcAmtsDueForObligation()
  {
    var useImport = new FnCalcAmtsDueForObligation.Import();
    var useExport = new FnCalcAmtsDueForObligation.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);

    Call(FnCalcAmtsDueForObligation.Execute, useImport, useExport);

    local.ScreenDueAmounts.TotalAmountDue =
      useExport.ScreenDueAmounts.TotalAmountDue;
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = entities.CsePerson.Number;
    useImport.FilterByIdLegalAction.Identifier =
      entities.LegalAction.Identifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    local.ScreenOwedAmounts.TotalAmountOwed =
      useExport.ScreenOwedAmounts.TotalAmountOwed;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Child.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Type1 = useExport.AbendData.Type1;
    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.Child);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Type1 = useExport.AbendData.Type1;
    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, export.Ap);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Type1 = useExport.AbendData.Type1;
    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, export.Ar);
  }

  private bool ReadAdministrativeActCertification1()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Ap.Number);
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
        entities.AdministrativeActCertification.AatType =
          db.GetNullableString(reader, 4);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 5);
        entities.AdministrativeActCertification.DateStayed =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 7);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadAdministrativeActCertification2()
  {
    entities.AdministrativeActCertification.Populated = false;

    return Read("ReadAdministrativeActCertification2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Ap.Number);
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
        entities.AdministrativeActCertification.AatType =
          db.GetNullableString(reader, 4);
        entities.AdministrativeActCertification.DecertifiedDate =
          db.GetNullableDate(reader, 5);
        entities.AdministrativeActCertification.DateStayed =
          db.GetNullableDate(reader, 6);
        entities.AdministrativeActCertification.TanfCode =
          db.GetString(reader, 7);
        entities.AdministrativeActCertification.Populated = true;
        CheckValid<AdministrativeActCertification>("CpaType",
          entities.AdministrativeActCertification.CpaType);
        CheckValid<AdministrativeActCertification>("Type1",
          entities.AdministrativeActCertification.Type1);
      });
  }

  private bool ReadBankruptcy()
  {
    entities.Bankruptcy.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "dischargeDate", local.Min.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Bankruptcy.CspNumber = db.GetString(reader, 0);
        entities.Bankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.Bankruptcy.BankruptcyFilingDate = db.GetDate(reader, 2);
        entities.Bankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 3);
        entities.Bankruptcy.BankruptcyDismissWithdrawDate =
          db.GetNullableDate(reader, 4);
        entities.Bankruptcy.Populated = true;
      });
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Process.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtOrderNumber", entities.LegalAction.StandardNumber ?? ""
          );
        db.SetNullableString(command, "oblgorPrsnNbr", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCashReceiptSourceType()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptSourceType",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 8);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCreditReportingAction()
  {
    entities.CreditReportingAction.Populated = false;

    return ReadEach("ReadCreditReportingAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.CreditReportingAction.Identifier = db.GetInt32(reader, 0);
        entities.CreditReportingAction.CseActionCode =
          db.GetNullableString(reader, 1);
        entities.CreditReportingAction.CraTransDate =
          db.GetNullableDate(reader, 2);
        entities.CreditReportingAction.CpaType = db.GetString(reader, 3);
        entities.CreditReportingAction.CspNumber = db.GetString(reader, 4);
        entities.CreditReportingAction.AacType = db.GetString(reader, 5);
        entities.CreditReportingAction.AacTakenDate = db.GetDate(reader, 6);
        entities.CreditReportingAction.AacTanfCode = db.GetString(reader, 7);
        entities.CreditReportingAction.Populated = true;
        CheckValid<CreditReportingAction>("CpaType",
          entities.CreditReportingAction.CpaType);
        CheckValid<CreditReportingAction>("AacType",
          entities.CreditReportingAction.AacType);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Process.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SelectedAp.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Process.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePersonCaseRoleCase()
  {
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCaseRoleCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Process.Date.GetValueOrDefault());
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 2);
        entities.CaseRole.CasNumber = db.GetString(reader, 3);
        entities.Case1.Number = db.GetString(reader, 3);
        entities.CaseRole.Type1 = db.GetString(reader, 4);
        entities.CaseRole.Identifier = db.GetInt32(reader, 5);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.CaseRole.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Case1.Status = db.GetNullableString(reader, 9);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 10);
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadGoodCause()
  {
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "effectiveDate", local.Process.Date.GetValueOrDefault());
        db.SetNullableString(command, "cspNumber1", export.Ap.Number);
        db.SetNullableString(command, "casNumber1", export.Case1.Number);
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

  private IEnumerable<bool> ReadIncomeSource1()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource1",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", export.Ap.Number);
        db.SetNullableDate(
          command, "startDt", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 3);
        entities.IncomeSource.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource2()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource2",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDt", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 3);
        entities.IncomeSource.Populated = true;

        return true;
      });
  }

  private bool ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 4);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 4);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure3()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure3",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 4);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure4()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure4",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 4);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadInfrastructure5()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure5",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", export.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 4);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 5);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Ap.Number);
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 2);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.Withholding.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Withholding.Identifier = db.GetInt32(reader, 0);
        entities.Withholding.ActionTaken = db.GetString(reader, 1);
        entities.Withholding.FiledDate = db.GetNullableDate(reader, 2);
        entities.Withholding.StandardNumber = db.GetNullableString(reader, 3);
        entities.Withholding.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.Withholding.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 10);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail()
  {
    entities.NarrativeDetail.Populated = false;

    return ReadEach("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.NarrativeDetail.InfrastructureId = db.GetInt32(reader, 0);
        entities.NarrativeDetail.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.NarrativeDetail.NarrativeText =
          db.GetNullableString(reader, 2);
        entities.NarrativeDetail.LineNumber = db.GetInt32(reader, 3);
        entities.NarrativeDetail.Populated = true;

        return true;
      });
  }

  private bool ReadObligationAdmActionExemption1()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Ap.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
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
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
      });
  }

  private bool ReadObligationAdmActionExemption2()
  {
    entities.ObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Ap.Number);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
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
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);
      });
  }

  private IEnumerable<bool> ReadObligationObligationTypeCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach("ReadObligationObligationTypeCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Ap.Number);
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "endDt", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 6);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 7);
        entities.ObligationType.Classification = db.GetString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 9);
        entities.CsePerson.Type1 = db.GetString(reader, 10);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 11);
        entities.CsePerson.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonIncomeHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.PersonIncomeHistory.Populated = false;

    return ReadEach("ReadPersonIncomeHistory",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetNullableDate(
          command, "incomeEffDt", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonIncomeHistory.CspNumber = db.GetString(reader, 0);
        entities.PersonIncomeHistory.IsrIdentifier = db.GetDateTime(reader, 1);
        entities.PersonIncomeHistory.Identifier = db.GetDateTime(reader, 2);
        entities.PersonIncomeHistory.IncomeEffDt =
          db.GetNullableDate(reader, 3);
        entities.PersonIncomeHistory.IncomeAmt =
          db.GetNullableDecimal(reader, 4);
        entities.PersonIncomeHistory.Freq = db.GetNullableString(reader, 5);
        entities.PersonIncomeHistory.CspINumber = db.GetString(reader, 6);
        entities.PersonIncomeHistory.MilitaryBaqAllotment =
          db.GetNullableDecimal(reader, 7);
        entities.PersonIncomeHistory.Populated = true;

        return true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of SelectedAp.
    /// </summary>
    [JsonPropertyName("selectedAp")]
    public CsePersonsWorkSet SelectedAp
    {
      get => selectedAp ??= new();
      set => selectedAp = value;
    }

    private Case1 case1;
    private Infrastructure infrastructure;
    private CsePersonsWorkSet selectedAp;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FinanceDetailsGroup group.</summary>
    [Serializable]
    public class FinanceDetailsGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of IwoDate.
      /// </summary>
      [JsonPropertyName("iwoDate")]
      public DateWorkArea IwoDate
      {
        get => iwoDate ??= new();
        set => iwoDate = value;
      }

      /// <summary>
      /// A value of LastPayment.
      /// </summary>
      [JsonPropertyName("lastPayment")]
      public DateWorkArea LastPayment
      {
        get => lastPayment ??= new();
        set => lastPayment = value;
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
      /// A value of MonthlyDue.
      /// </summary>
      [JsonPropertyName("monthlyDue")]
      public Common MonthlyDue
      {
        get => monthlyDue ??= new();
        set => monthlyDue = value;
      }

      /// <summary>
      /// A value of MwoDate.
      /// </summary>
      [JsonPropertyName("mwoDate")]
      public DateWorkArea MwoDate
      {
        get => mwoDate ??= new();
        set => mwoDate = value;
      }

      /// <summary>
      /// A value of Payoff.
      /// </summary>
      [JsonPropertyName("payoff")]
      public Common Payoff
      {
        get => payoff ??= new();
        set => payoff = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private DateWorkArea iwoDate;
      private DateWorkArea lastPayment;
      private LegalAction legalAction;
      private Common monthlyDue;
      private DateWorkArea mwoDate;
      private Common payoff;
    }

    /// <summary>A AgesOfChildrenGroup group.</summary>
    [Serializable]
    public class AgesOfChildrenGroup
    {
      /// <summary>
      /// A value of AgeOfChild.
      /// </summary>
      [JsonPropertyName("ageOfChild")]
      public TextWorkArea AgeOfChild
      {
        get => ageOfChild ??= new();
        set => ageOfChild = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private TextWorkArea ageOfChild;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of ApKnownToOtherCases.
    /// </summary>
    [JsonPropertyName("apKnownToOtherCases")]
    public Common ApKnownToOtherCases
    {
      get => apKnownToOtherCases ??= new();
      set => apKnownToOtherCases = value;
    }

    /// <summary>
    /// A value of ApLastModificationDen.
    /// </summary>
    [JsonPropertyName("apLastModificationDen")]
    public DateWorkArea ApLastModificationDen
    {
      get => apLastModificationDen ??= new();
      set => apLastModificationDen = value;
    }

    /// <summary>
    /// A value of ApMonthlyIncome.
    /// </summary>
    [JsonPropertyName("apMonthlyIncome")]
    public Common ApMonthlyIncome
    {
      get => apMonthlyIncome ??= new();
      set => apMonthlyIncome = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ArLastModificationDen.
    /// </summary>
    [JsonPropertyName("arLastModificationDen")]
    public DateWorkArea ArLastModificationDen
    {
      get => arLastModificationDen ??= new();
      set => arLastModificationDen = value;
    }

    /// <summary>
    /// A value of ArMonthlyIncome.
    /// </summary>
    [JsonPropertyName("arMonthlyIncome")]
    public Common ArMonthlyIncome
    {
      get => arMonthlyIncome ??= new();
      set => arMonthlyIncome = value;
    }

    /// <summary>
    /// A value of CredCertification.
    /// </summary>
    [JsonPropertyName("credCertification")]
    public Common CredCertification
    {
      get => credCertification ??= new();
      set => credCertification = value;
    }

    /// <summary>
    /// Gets a value of FinanceDetails.
    /// </summary>
    [JsonIgnore]
    public Array<FinanceDetailsGroup> FinanceDetails => financeDetails ??= new(
      FinanceDetailsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FinanceDetails for json serialization.
    /// </summary>
    [JsonPropertyName("financeDetails")]
    [Computed]
    public IList<FinanceDetailsGroup> FinanceDetails_Json
    {
      get => financeDetails;
      set => FinanceDetails.Assign(value);
    }

    /// <summary>
    /// A value of FdsoCertification.
    /// </summary>
    [JsonPropertyName("fdsoCertification")]
    public Common FdsoCertification
    {
      get => fdsoCertification ??= new();
      set => fdsoCertification = value;
    }

    /// <summary>
    /// Gets a value of AgesOfChildren.
    /// </summary>
    [JsonIgnore]
    public Array<AgesOfChildrenGroup> AgesOfChildren => agesOfChildren ??= new(
      AgesOfChildrenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AgesOfChildren for json serialization.
    /// </summary>
    [JsonPropertyName("agesOfChildren")]
    [Computed]
    public IList<AgesOfChildrenGroup> AgesOfChildren_Json
    {
      get => agesOfChildren;
      set => AgesOfChildren.Assign(value);
    }

    /// <summary>
    /// A value of LastIncomeIncrease.
    /// </summary>
    [JsonPropertyName("lastIncomeIncrease")]
    public DateWorkArea LastIncomeIncrease
    {
      get => lastIncomeIncrease ??= new();
      set => lastIncomeIncrease = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public LegalReferral Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of LastModificationRequest.
    /// </summary>
    [JsonPropertyName("lastModificationRequest")]
    public DateWorkArea LastModificationRequest
    {
      get => lastModificationRequest ??= new();
      set => lastModificationRequest = value;
    }

    /// <summary>
    /// A value of LastReviewDate.
    /// </summary>
    [JsonPropertyName("lastReviewDate")]
    public DateWorkArea LastReviewDate
    {
      get => lastReviewDate ??= new();
      set => lastReviewDate = value;
    }

    /// <summary>
    /// A value of NumberOfAps.
    /// </summary>
    [JsonPropertyName("numberOfAps")]
    public Common NumberOfAps
    {
      get => numberOfAps ??= new();
      set => numberOfAps = value;
    }

    /// <summary>
    /// A value of NumberOfChildren.
    /// </summary>
    [JsonPropertyName("numberOfChildren")]
    public Common NumberOfChildren
    {
      get => numberOfChildren ??= new();
      set => numberOfChildren = value;
    }

    /// <summary>
    /// A value of ReviewNote.
    /// </summary>
    [JsonPropertyName("reviewNote")]
    public NarrativeWork ReviewNote
    {
      get => reviewNote ??= new();
      set => reviewNote = value;
    }

    /// <summary>
    /// A value of SdsoCertification.
    /// </summary>
    [JsonPropertyName("sdsoCertification")]
    public Common SdsoCertification
    {
      get => sdsoCertification ??= new();
      set => sdsoCertification = value;
    }

    private Bankruptcy bankruptcy;
    private Case1 case1;
    private CsePerson csePerson;
    private GoodCause goodCause;
    private CsePersonsWorkSet ap;
    private Common apKnownToOtherCases;
    private DateWorkArea apLastModificationDen;
    private Common apMonthlyIncome;
    private CsePersonsWorkSet ar;
    private DateWorkArea arLastModificationDen;
    private Common arMonthlyIncome;
    private Common credCertification;
    private Array<FinanceDetailsGroup> financeDetails;
    private Common fdsoCertification;
    private Array<AgesOfChildrenGroup> agesOfChildren;
    private DateWorkArea lastIncomeIncrease;
    private LegalReferral last;
    private DateWorkArea lastModificationRequest;
    private DateWorkArea lastReviewDate;
    private Common numberOfAps;
    private Common numberOfChildren;
    private NarrativeWork reviewNote;
    private Common sdsoCertification;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePersonsWorkSet Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of ChildsAge.
    /// </summary>
    [JsonPropertyName("childsAge")]
    public Common ChildsAge
    {
      get => childsAge ??= new();
      set => childsAge = value;
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
    /// A value of Min.
    /// </summary>
    [JsonPropertyName("min")]
    public DateWorkArea Min
    {
      get => min ??= new();
      set => min = value;
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
    /// A value of NarrativeWork.
    /// </summary>
    [JsonPropertyName("narrativeWork")]
    public NarrativeWork NarrativeWork
    {
      get => narrativeWork ??= new();
      set => narrativeWork = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LegalAction Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private AbendData abendData;
    private CsePerson ap;
    private CsePersonsWorkSet child;
    private Common childsAge;
    private DateWorkArea current;
    private DateWorkArea min;
    private DateWorkArea max;
    private NarrativeWork narrativeWork;
    private DateWorkArea null1;
    private DateWorkArea process;
    private LegalAction previous;
    private ScreenDueAmounts screenDueAmounts;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common temp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
    }

    /// <summary>
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
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
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
    }

    /// <summary>
    /// A value of Withholding.
    /// </summary>
    [JsonPropertyName("withholding")]
    public LegalAction Withholding
    {
      get => withholding ??= new();
      set => withholding = value;
    }

    private AdministrativeActCertification administrativeActCertification;
    private AdministrativeAction administrativeAction;
    private Bankruptcy bankruptcy;
    private Case1 case1;
    private CaseRole caseRole;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CreditReportingAction creditReportingAction;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private GoodCause goodCause;
    private IncomeSource incomeSource;
    private Infrastructure infrastructure;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private LegalReferral legalReferral;
    private LegalReferralCaseRole legalReferralCaseRole;
    private NarrativeDetail narrativeDetail;
    private Obligation obligation;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private ObligationType obligationType;
    private OutgoingDocument outgoingDocument;
    private PersonIncomeHistory personIncomeHistory;
    private LegalAction withholding;
  }
#endregion
}
