// Program: CAB_PREP_FEDERAL_DEBT_SETOFF, ID: 372665445, model: 746.
// Short name: SWE02375
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
/// A program: CAB_PREP_FEDERAL_DEBT_SETOFF.
/// </para>
/// <para>
/// Navigate thru database to prepare administrative_act_certification subtype 
/// federal_debt_setoff.
/// </para>
/// </summary>
[Serializable]
public partial class CabPrepFederalDebtSetoff: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_PREP_FEDERAL_DEBT_SETOFF program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabPrepFederalDebtSetoff(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabPrepFederalDebtSetoff.
  /// </summary>
  public CabPrepFederalDebtSetoff(IContext context, Import import, Export export)
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
    // **************************** 
    // MAINTENANCE LOG
    // ***************************************
    // 2000-06-12	JMagat		PR#93757
    // 1) When determining AP's obligations, secondary debts
    // should not be included in the certification process.
    // 2) Added import view of obligor cse_persons_work_set.
    // 06-19-2000   E.Shirk   		PRWORA
    // Altered logic to change program code to AF when a program code of NA and 
    // program state of CA is detected.
    // 02-06-2001  Madhu Kumar
    // Added logic to add new exemption code  ALBP .
    // 02-22-2001	E.Shirk		WR00269
    // 1) Added read each against AP Case Role to determine if AP had any Good 
    // Cause actions filed by any AR.   This later eliminates the calling of
    // le_check_good_cause_for_debt when the AP has no good cause set.
    // 2)   Corrected error when returning from le_check_good_cause_for_debt.  
    // Previously the entire AP was being skipped, now only the debt is
    // bypassed.
    // 01-20-2002	E.Shirk		PR133691
    // Altered module to begin segregating arrears amounts into greater than 90 
    // day, and less than 90 day accumulators for TANF and non-TANF amounts.
    // Also removed modifications to the TANF and non-TANF indicators.
    // 07-09-2002	E.Shirk		PR142609
    // Altered process to no longer certify obligors who are tied to only closed
    // cases.     Removed all closed case logic and relocated the case logic to
    // beginning of process to facilitate an exit of the CAB when only closed
    // cases are detected.
    // 01/25/2004 cmj pr 223879 - federal requlation change removing the 
    // criteria that arrears must be 90 days old prior to certification for FDSO
    // 03/15/2006	WR 258945	M.J. Quinn		Allow for FDSO certification of 
    // bankruptcy
    // 05/23/2007	GVandy		WR289942
    // Effective 10/01/2007, the age restriction on supported person when 
    // determining Non-TAF certification will be removed.
    // 05/23/2007	GVandy		WR 289942
    // Retain obligors address as reported to feds to support Z (Address Changes
    // ) transactions.
    // 07/25/2007  GVandy	PR313068	Changes to correctly send transactions per 
    // case type (i.e. adc verses non-adc).
    // 10/01/2014  DDupree	PR42515	Changes to correctly select the current good 
    // casuse.
    // **********************************************************************************
    // **  Initialize process variables.
    // **********************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.GoodCauseFoundForDebt.Flag = "N";
    local.FirstChildInd.Flag = "Y";
    local.Child18OrOver.Count = 0;
    local.OpenCase.Count = 0;
    export.NontanfGt90DayArrears.BalanceDueAmt = 0;
    export.NontanfLt90DayArrears.BalanceDueAmt = 0;
    export.TanfGt90DayArrears.BalanceDueAmt = 0;
    export.TanfLt90DayArrears.BalanceDueAmt = 0;
    export.FederalDebtSetoff.AdcAmount = 0;
    export.FederalDebtSetoff.NonAdcAmount = 0;
    local.CsePersonsWorkSet.Number = import.Obligor.Number;
    local.CsePersonsWorkSet.Assign(import.Obligor);
    export.FederalDebtSetoff.TakenDate =
      import.ProgramProcessingInfo.ProcessDate;
    export.FederalDebtSetoff.ProcessYear =
      Year(import.ProgramProcessingInfo.ProcessDate);
    export.FederalDebtSetoff.Ssn =
      (int)StringToNumber(local.CsePersonsWorkSet.Ssn);
    export.FederalDebtSetoff.CaseNumber = local.CsePersonsWorkSet.Number;
    export.FederalDebtSetoff.FirstName = local.CsePersonsWorkSet.FirstName;
    local.Case1.Index = -1;

    // *** Remove any leading and imbedded blanks in the
    // LAST_NAME; as well as leading, imbedded and trailing
    // periods and apostrophes in the LAST_NAME.
    local.Zero.Count = 0;

    while(Find(TrimEnd(local.CsePersonsWorkSet.LastName), " ") > local
      .Zero.Count)
    {
      // *** Found a Blank char.  Now, get its position.
      local.Posn.Count =
        Find(String(
          local.CsePersonsWorkSet.LastName,
        CsePersonsWorkSet.LastName_MaxLength), " ");

      if (local.Posn.Count == 1)
      {
        // *** Found in 1st position of the field, shift 1 byte to left.
        local.CsePersonsWorkSet.LastName =
          Substring(local.CsePersonsWorkSet.LastName, 2,
          CsePersonsWorkSet.LastName_MaxLength - 1);
      }
      else
      {
        // *** Found in middle position of the field, skip that position.
        local.CsePersonsWorkSet.LastName =
          Substring(local.CsePersonsWorkSet.LastName,
          CsePersonsWorkSet.LastName_MaxLength, 1, local.Posn.Count - 1) + Substring
          (local.CsePersonsWorkSet.LastName,
          CsePersonsWorkSet.LastName_MaxLength, local.Posn.Count +
          1, CsePersonsWorkSet.LastName_MaxLength - local.Posn.Count);
      }
    }

    while(Find(TrimEnd(local.CsePersonsWorkSet.LastName), "'") > local
      .Zero.Count)
    {
      // *** Found an Apostrophe.  Now, get its position.
      local.Posn.Count = Find(local.CsePersonsWorkSet.LastName, "'");

      if (local.Posn.Count == 1)
      {
        // *** Found in 1st position of the field, shift 1 byte to left.
        local.CsePersonsWorkSet.LastName =
          Substring(local.CsePersonsWorkSet.LastName, 2,
          CsePersonsWorkSet.LastName_MaxLength - 1);
      }
      else if (local.Posn.Count == CsePersonsWorkSet.LastName_MaxLength)
      {
        // *** Found in last position of the field, truncate last position.
        local.CsePersonsWorkSet.LastName =
          Substring(local.CsePersonsWorkSet.LastName, 1, local.Posn.Count - 1);
      }
      else
      {
        // *** Found in middle position of the field, skip that position.
        local.CsePersonsWorkSet.LastName =
          Substring(local.CsePersonsWorkSet.LastName,
          CsePersonsWorkSet.LastName_MaxLength, 1, local.Posn.Count - 1) + Substring
          (local.CsePersonsWorkSet.LastName,
          CsePersonsWorkSet.LastName_MaxLength, local.Posn.Count +
          1, CsePersonsWorkSet.LastName_MaxLength - local.Posn.Count);
      }
    }

    while(Find(TrimEnd(local.CsePersonsWorkSet.LastName), ".") > local
      .Zero.Count)
    {
      // *** Found a Period.  Now, get its position.
      local.Posn.Count = Find(local.CsePersonsWorkSet.LastName, ".");

      if (local.Posn.Count == 1)
      {
        // *** Found in 1st position of the field, shift 1 byte to left.
        local.CsePersonsWorkSet.LastName =
          Substring(local.CsePersonsWorkSet.LastName, 2,
          CsePersonsWorkSet.LastName_MaxLength - 1);
      }
      else if (local.Posn.Count == CsePersonsWorkSet.LastName_MaxLength)
      {
        // *** Found in last position of the field, truncate last position.
        local.CsePersonsWorkSet.LastName =
          Substring(local.CsePersonsWorkSet.LastName, 1, local.Posn.Count - 1);
      }
      else
      {
        // *** Found in middle position of the field, skip that position.
        local.CsePersonsWorkSet.LastName =
          Substring(local.CsePersonsWorkSet.LastName,
          CsePersonsWorkSet.LastName_MaxLength, 1, local.Posn.Count - 1) + Substring
          (local.CsePersonsWorkSet.LastName,
          CsePersonsWorkSet.LastName_MaxLength, local.Posn.Count +
          1, CsePersonsWorkSet.LastName_MaxLength - local.Posn.Count);
      }
    }

    export.FederalDebtSetoff.LastName = local.CsePersonsWorkSet.LastName;

    // cmj 1/25/2005 pr 223879  changed from 3 months to 0 months
    local.PpiMinus90Day.ProcessDate =
      AddMonths(import.ProgramProcessingInfo.ProcessDate, -0);
    local.WorkDate.Text10 =
      NumberToString(Year(import.ProgramProcessingInfo.ProcessDate), 12, 4) + "-"
      + NumberToString
      (Month(import.ProgramProcessingInfo.ProcessDate), 14, 2) + "-" + "01";
    local.FirstOfPpiMonth.Date = StringToDate(local.WorkDate.Text10);

    // cmj 1/25/2005 pr 223879  changed from 3 months to 0 months
    local.Local1StRunMonthMinus90Day.ProcessDate =
      AddMonths(local.FirstOfPpiMonth.Date, -0);

    // **********************************************************************************
    // **   Get current AP.
    // **********************************************************************************
    if (!ReadCsePersonObligor())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // **********************************************************************************
    // **   Check for an active Bankruptcy.
    // **********************************************************************************
    if (ReadBankruptcy())
    {
      if (Equal(entities.Bankruptcy.BankruptcyType, "13"))
      {
        export.BankruptCh13.Flag = "Y";

        goto Read;
      }

      if (!Lt(entities.Bankruptcy.BankruptcyFilingDate, new DateTime(2005, 10,
        17)))
      {
        export.Bankruptcy.ExpectedBkrpDischargeDate =
          entities.Bankruptcy.ExpectedBkrpDischargeDate;
        export.BankrptAfter20051017.Flag = "Y";
      }
      else
      {
        export.BankrptPriorTo20051017.Flag = "Y";
      }
    }
    else
    {
      // -- Continue.
    }

Read:

    // **********************************************************************************
    // **  Find each case AP is related.
    // **********************************************************************************
    foreach(var item in ReadCaseCaseRole())
    {
      // **********************************************************************************
      // **  Get most recent case assignment
      // **********************************************************************************
      local.CaseAsignFound.Flag = "N";

      if (ReadCaseAssignment())
      {
        local.CaseAsignFound.Flag = "Y";
      }

      // **********************************************************************************
      // **  Bypass case if no assignment found.
      // **********************************************************************************
      if (AsChar(local.CaseAsignFound.Flag) == 'N')
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Obligor " + entities
          .CsePerson.Number + " - Case assignment not found for Case Number " +
          entities.Case2.Number;
        UseCabErrorReport();

        continue;
      }

      // **********************************************************************************
      // **  Get office number.
      // **********************************************************************************
      if (!ReadOfficeServiceProviderOffice())
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Obligor " + entities
          .CsePerson.Number + " - Office Service Provider not found for Case Number " +
          entities.Case2.Number;
        UseCabErrorReport();

        continue;
      }

      // **********************************************************************************
      // **  Load case number, status and office number to local group.
      // **********************************************************************************
      if (local.Case1.Index + 1 == Local.CaseGroup.Capacity)
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Obligor " + entities
          .CsePerson.Number + " - Number of cases exceeded table limit.";
        UseCabErrorReport();

        break;
      }
      else
      {
        ++local.Case1.Index;
        local.Case1.CheckSize();

        MoveCase1(entities.Case2, local.Case1.Update.CaseCase);
        local.Case1.Update.CaseOffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;
      }
    }

    // **********************************************************************************
    // **  Spin through the case array counting the open cases.
    // **********************************************************************************
    if (local.Case1.Count > 0)
    {
      for(local.Case1.Index = 0; local.Case1.Index < local.Case1.Count; ++
        local.Case1.Index)
      {
        if (!local.Case1.CheckSize())
        {
          break;
        }

        if (AsChar(local.Case1.Item.CaseCase.Status) == 'O')
        {
          ++local.OpenCase.Count;
        }
      }

      local.Case1.CheckIndex();
    }

    // **********************************************************************************
    // **  Evaluate number of open cases:  When
    // **   0 open cases - exit process
    // **   1 open cases - select that office number and continue with arrears 
    // calc.
    // **  >1 open cases - set office # to 022, unless the mult cases point to 
    // the
    // **                  same office number.  Continue with arrears calc.
    // **********************************************************************************
    switch(local.OpenCase.Count)
    {
      case 0:
        return;
      case 1:
        for(local.Case1.Index = 0; local.Case1.Index < local.Case1.Count; ++
          local.Case1.Index)
        {
          if (!local.Case1.CheckSize())
          {
            break;
          }

          if (AsChar(local.Case1.Item.CaseCase.Status) == 'O')
          {
            export.FederalDebtSetoff.LocalCode =
              NumberToString(entities.Office.SystemGeneratedId, 3);

            break;
          }
        }

        local.Case1.CheckIndex();

        break;
      default:
        local.Case1.Index = 0;
        local.Case1.CheckSize();

        local.Hold.SystemGeneratedId =
          local.Case1.Item.CaseOffice.SystemGeneratedId;
        export.FederalDebtSetoff.LocalCode =
          NumberToString(local.Case1.Item.CaseOffice.SystemGeneratedId, 3);

        for(local.Case1.Index = 0; local.Case1.Index < local.Case1.Count; ++
          local.Case1.Index)
        {
          if (!local.Case1.CheckSize())
          {
            break;
          }

          if (AsChar(local.Case1.Item.CaseCase.Status) == 'O')
          {
            if (local.Hold.SystemGeneratedId == local
              .Case1.Item.CaseOffice.SystemGeneratedId)
            {
              continue;
            }
            else
            {
              export.FederalDebtSetoff.LocalCode = "022";

              break;
            }
          }
        }

        local.Case1.CheckIndex();

        break;
    }

    // **********************************************************************************
    // ** Determine if AP has been marked good cause in any case, then later 
    // only
    // ** call the le_check_good_cause_for_debt when the flag is 'Y'.  Saves the
    // ** unnecessary checking of G.C. on every debt.
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

    // 05/23/2007 GVandy  WR 289942  Retain obligors address as reported to feds
    // to support Z (Address Changes) transactions.
    UseSiGetCsePersonMailingAddr();

    if (AsChar(local.CsePersonAddress.LocationType) == 'D')
    {
      // -- For a domestic address we send street_1, street_2, city, state, zip 
      // code, zip + 4.
      export.FederalDebtSetoff.AddressStreet1 =
        local.CsePersonAddress.Street1 ?? "";
      export.FederalDebtSetoff.AddressStreet2 =
        local.CsePersonAddress.Street2 ?? "";
      export.FederalDebtSetoff.AddressCity = local.CsePersonAddress.City ?? "";
      export.FederalDebtSetoff.AddressState = local.CsePersonAddress.State ?? ""
        ;
      export.FederalDebtSetoff.AddressZip = (local.CsePersonAddress.ZipCode ?? ""
        ) + (local.CsePersonAddress.Zip4 ?? "");
    }
    else if (AsChar(local.CsePersonAddress.LocationType) == 'F')
    {
      // -- For a foreign address we send street_1, street_2, city, and country 
      // in the place of state.
      //    No zip, postal code, or province is sent.
      //    This is per Brian Peeler at OCSE.
      export.FederalDebtSetoff.AddressStreet1 =
        local.CsePersonAddress.Street1 ?? "";
      export.FederalDebtSetoff.AddressStreet2 =
        local.CsePersonAddress.Street2 ?? "";
      export.FederalDebtSetoff.AddressCity = local.CsePersonAddress.City ?? "";
      export.FederalDebtSetoff.AddressState =
        local.CsePersonAddress.Country ?? "";
    }

    // ***********************************************************************************
    // **  Read AP's obligations
    // ***********************************************************************************
    foreach(var item in ReadObligationLegalActionTribunal())
    {
      // ***********************************************************************************
      // **   Bypass secondary obligations.
      // ***********************************************************************************
      if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'S')
      {
        continue;
      }

      // ***********************************************************************************
      // **   Filter certifiable FDSO obligation types.
      // ***********************************************************************************
      if (ReadObligationType())
      {
        if (Equal(entities.ObligationType.Code, "CS") || Equal
          (entities.ObligationType.Code, "SP") || Equal
          (entities.ObligationType.Code, "MS") || Equal
          (entities.ObligationType.Code, "MC") || Equal
          (entities.ObligationType.Code, "SAJ") || Equal
          (entities.ObligationType.Code, "718B") || Equal
          (entities.ObligationType.Code, "IJ") || Equal
          (entities.ObligationType.Code, "MJ") || Equal
          (entities.ObligationType.Code, "CRCH") || Equal
          (entities.ObligationType.Code, "AJ"))
        {
        }
        else
        {
          continue;
        }

        local.Debt.DebtType = "D";
        local.ObligationType.Classification =
          entities.ObligationType.Classification;
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_TYPE_NF";

        return;
      }

      // ---------------------------------------------
      // If the Obligation Type is equal to 'SP' (Spousal Support) or
      // 'SAJ' (Spousal Arrears Judgement), then the Obligation is
      // certifiable only if there exists a Child Support Obligation ('CS')
      // on the same Court Order. If this is not the case, then ignore
      // this obligation and read the next one.
      // ---------------------------------------------
      if (Equal(entities.ObligationType.Code, "SP") || Equal
        (entities.ObligationType.Code, "SAJ"))
      {
        if (!ReadLegalAction())
        {
          // *** No CS obligation found within the same court case
          continue;
        }
      }

      // --------------------------------------------------------------------------------
      // Determine exclusion types:
      // A read each checks if for that obligation, exemption exists
      // and administrative action type is equal to ALL, then skip this
      // obligation. Otherwise based on this type, set corresponding
      // etype FDSO.
      // --------------------------------------------------------------------------------
      foreach(var item1 in ReadObligationAdmActionExemptionAdministrativeAction())
        
      {
        if (Equal(entities.AdministrativeAction.Type1, "ADM"))
        {
          if (Equal(entities.ObligationAdmActionExemption.Reason,
            "BANKRUPT ADM EXCL"))
          {
            export.FederalDebtSetoff.EtypeAdmBankrupt = "Y";
          }
          else
          {
            export.FederalDebtSetoff.EtypeAdministrativeOffset = "Y";
          }
        }
        else if (Equal(entities.AdministrativeAction.Type1, "RET"))
        {
          export.FederalDebtSetoff.EtypeFederalRetirement = "Y";
        }
        else if (Equal(entities.AdministrativeAction.Type1, "SAL"))
        {
          export.FederalDebtSetoff.EtypeFederalSalary = "Y";
        }
        else if (Equal(entities.AdministrativeAction.Type1, "FIN"))
        {
          export.FederalDebtSetoff.EtypeFinancialInstitution = "Y";
        }
        else if (Equal(entities.AdministrativeAction.Type1, "PAS"))
        {
          export.FederalDebtSetoff.EtypePassportDenial = "Y";
        }
        else if (Equal(entities.AdministrativeAction.Type1, "TAX"))
        {
          export.FederalDebtSetoff.EtypeTaxRefund = "Y";
        }
        else if (Equal(entities.AdministrativeAction.Type1, "VEN"))
        {
          export.FederalDebtSetoff.EtypeVendorPaymentOrMisc = "Y";
        }
        else if (Equal(entities.AdministrativeAction.Type1, "ALBP"))
        {
          export.FederalDebtSetoff.EtypeAdministrativeOffset = "Y";
          export.FederalDebtSetoff.EtypeFederalRetirement = "Y";
          export.FederalDebtSetoff.EtypeFederalSalary = "Y";
          export.FederalDebtSetoff.EtypeFinancialInstitution = "Y";
          export.FederalDebtSetoff.EtypeTaxRefund = "Y";
          export.FederalDebtSetoff.EtypeVendorPaymentOrMisc = "Y";
        }
        else if (Equal(entities.AdministrativeAction.Type1, "ALL"))
        {
          goto ReadEach;
        }
      }

      if ((AsChar(export.FederalDebtSetoff.EtypeAdmBankrupt) == 'Y' || AsChar
        (export.FederalDebtSetoff.EtypeAdministrativeOffset) == 'Y') && AsChar
        (export.FederalDebtSetoff.EtypeFederalRetirement) == 'Y' && AsChar
        (export.FederalDebtSetoff.EtypeFederalSalary) == 'Y' && AsChar
        (export.FederalDebtSetoff.EtypeFinancialInstitution) == 'Y' && AsChar
        (export.FederalDebtSetoff.EtypeTaxRefund) == 'Y' && AsChar
        (export.FederalDebtSetoff.EtypeVendorPaymentOrMisc) == 'Y' && AsChar
        (export.FederalDebtSetoff.EtypePassportDenial) == 'Y')
      {
        export.FederalDebtSetoff.EtypeAdministrativeOffset = "";
        export.FederalDebtSetoff.EtypeFederalRetirement = "";
        export.FederalDebtSetoff.EtypeFederalSalary = "";
        export.FederalDebtSetoff.EtypeFinancialInstitution = "";
        export.FederalDebtSetoff.EtypeTaxRefund = "";
        export.FederalDebtSetoff.EtypeVendorPaymentOrMisc = "";
        export.FederalDebtSetoff.EtypePassportDenial = "";

        // **********************************************************
        //      This kind of a situation would arise if we have 2
        //  exemptions for the same obligation like ALBP and
        //  PAS which is equivalent to  ALL .
        //      In this case we should ship the present obligor and
        //   go for the next one.
        // **********************************************************
        continue;
      }

      // ***********************************************************************************
      // **   Read debts tied to valid obligations.
      // ***********************************************************************************
      foreach(var item1 in ReadDebtDebtDetail())
      {
        // ***********************************************************************************
        // **  Accruing type debts are considered in arrears only if due date is
        // prior to beginning of month.
        // ***********************************************************************************
        if (entities.ObligationType.SystemGeneratedIdentifier == 1 || entities
          .ObligationType.SystemGeneratedIdentifier == 2 || entities
          .ObligationType.SystemGeneratedIdentifier == 3 || entities
          .ObligationType.SystemGeneratedIdentifier == 19)
        {
          if (!Lt(entities.DebtDetail.DueDt, local.FirstOfPpiMonth.Date))
          {
            continue;
          }
        }

        if (AsChar(local.GoodCauseFoundForAp.Flag) == 'Y')
        {
          local.GoodCauseFoundForDebt.Flag = "";
          UseLeCheckGoodCauseForDebt();

          if (AsChar(local.GoodCauseFoundForDebt.Flag) == 'Y')
          {
            continue;
          }
        }

        if (!ReadCsePerson())
        {
          // *** Not possible. A supported person exists at this point.
        }

        // ***********************************************************************
        // **  Determine program code for debt
        // ***********************************************************************
        UseFnDeterminePgmForDebtDetail();

        // *** WR164 - PWRORA Distribution.
        if (Equal(local.Program.Code, "NA") && Equal
          (local.DprProgram.ProgramState, "CA"))
        {
          local.Program.Code = "AF";
        }

        // ***********************************************************************
        // **   Filter out interstate debts
        // ***********************************************************************
        if (Equal(local.Program.Code, "AF") || Equal
          (local.Program.Code, "FC") || Equal(local.Program.Code, "NA") || Equal
          (local.Program.Code, "NF") || Equal(local.Program.Code, "NC"))
        {
        }
        else
        {
          continue;
        }

        // ***********************************************************************
        // **   Set TANF on non-TANF amounts that are greater than  and less 
        // than 90 days in arrears.
        // ***********************************************************************
        if (Equal(local.Program.Code, "AF") || Equal(local.Program.Code, "FC"))
        {
          // ***********************************************************************
          // **   Accruing debts are evaluated against the 1st day of the run 
          // month.
          // ***********************************************************************
          if (entities.ObligationType.SystemGeneratedIdentifier == 1 || entities
            .ObligationType.SystemGeneratedIdentifier == 2 || entities
            .ObligationType.SystemGeneratedIdentifier == 3 || entities
            .ObligationType.SystemGeneratedIdentifier == 19)
          {
            if (Lt(entities.DebtDetail.DueDt,
              local.Local1StRunMonthMinus90Day.ProcessDate))
            {
              export.TanfGt90DayArrears.BalanceDueAmt =
                export.TanfGt90DayArrears.BalanceDueAmt + entities
                .DebtDetail.BalanceDueAmt + entities
                .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
            }
            else
            {
              export.TanfLt90DayArrears.BalanceDueAmt =
                export.TanfLt90DayArrears.BalanceDueAmt + entities
                .DebtDetail.BalanceDueAmt + entities
                .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
            }
          }
          else if (Lt(entities.DebtDetail.DueDt, local.PpiMinus90Day.ProcessDate))
            
          {
            export.TanfGt90DayArrears.BalanceDueAmt =
              export.TanfGt90DayArrears.BalanceDueAmt + entities
              .DebtDetail.BalanceDueAmt + entities
              .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          }
          else
          {
            export.TanfLt90DayArrears.BalanceDueAmt =
              export.TanfLt90DayArrears.BalanceDueAmt + entities
              .DebtDetail.BalanceDueAmt + entities
              .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          }
        }
        else
        {
          // 05-23-2007  GVandy   WR289942
          // Effective 10/01/2007, the age restriction on supported person when 
          // determining Non-TAF certification will be removed.
          if (Lt(import.ProgramProcessingInfo.ProcessDate, new DateTime(2007,
            10, 1)))
          {
            // **  Check for emancipation on non-TANF debts.  **
            local.Child18OrOver.Flag = "N";
            UseLeFdsoCheckChild18OrOver();

            if (AsChar(local.Child18OrOver.Flag) == 'Y')
            {
              if (AsChar(local.FirstChildInd.Flag) == 'Y')
              {
                local.FirstChildInd.Flag = "N";
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail = "Obligor " + entities
                  .CsePerson.Number + " - Child over 18 years, cannot be certified.";
                  
                UseCabErrorReport();
              }

              continue;
            }
          }

          // ***********************************************************************
          // **   Accruing debts are evaluated against the 1st day of the run 
          // month.   Non-accruing debts are evaluated against the PPI date.
          // ***********************************************************************
          if (entities.ObligationType.SystemGeneratedIdentifier == 1 || entities
            .ObligationType.SystemGeneratedIdentifier == 2 || entities
            .ObligationType.SystemGeneratedIdentifier == 3 || entities
            .ObligationType.SystemGeneratedIdentifier == 19)
          {
            if (Lt(entities.DebtDetail.DueDt,
              local.Local1StRunMonthMinus90Day.ProcessDate))
            {
              export.NontanfGt90DayArrears.BalanceDueAmt =
                export.NontanfGt90DayArrears.BalanceDueAmt + entities
                .DebtDetail.BalanceDueAmt + entities
                .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
            }
            else
            {
              export.NontanfLt90DayArrears.BalanceDueAmt =
                export.NontanfLt90DayArrears.BalanceDueAmt + entities
                .DebtDetail.BalanceDueAmt + entities
                .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
            }
          }
          else if (Lt(entities.DebtDetail.DueDt, local.PpiMinus90Day.ProcessDate))
            
          {
            export.NontanfGt90DayArrears.BalanceDueAmt =
              export.NontanfGt90DayArrears.BalanceDueAmt + entities
              .DebtDetail.BalanceDueAmt + entities
              .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          }
          else
          {
            export.NontanfLt90DayArrears.BalanceDueAmt =
              export.NontanfLt90DayArrears.BalanceDueAmt + entities
              .DebtDetail.BalanceDueAmt + entities
              .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          }
        }
      }

ReadEach:
      ;
    }

    export.FederalDebtSetoff.AdcAmount =
      export.TanfLt90DayArrears.BalanceDueAmt + export
      .TanfGt90DayArrears.BalanceDueAmt;
    export.FederalDebtSetoff.NonAdcAmount =
      export.NontanfLt90DayArrears.BalanceDueAmt + export
      .NontanfGt90DayArrears.BalanceDueAmt;
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Country = source.Country;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.Supported1.Number;
    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
  }

  private void UseLeCheckGoodCauseForDebt()
  {
    var useImport = new LeCheckGoodCauseForDebt.Import();
    var useExport = new LeCheckGoodCauseForDebt.Export();

    useImport.Debt.SystemGeneratedIdentifier =
      entities.Debt.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = entities.CsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(LeCheckGoodCauseForDebt.Execute, useImport, useExport);

    local.GoodCauseFoundForDebt.Flag = useExport.GoodCauseFoundForDebt.Flag;
  }

  private void UseLeFdsoCheckChild18OrOver()
  {
    var useImport = new LeFdsoCheckChild18OrOver.Import();
    var useExport = new LeFdsoCheckChild18OrOver.Export();

    useImport.PersistantDebt.Assign(entities.Debt);
    useImport.PersistantObligor.Assign(entities.Obligor);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;

    Call(LeFdsoCheckChild18OrOver.Execute, useImport, useExport);

    local.Child18OrOver.Flag = useExport.Child18OrOver.Flag;
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private bool ReadBankruptcy()
  {
    entities.Bankruptcy.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "dischargeDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Bankruptcy.CspNumber = db.GetString(reader, 0);
        entities.Bankruptcy.Identifier = db.GetInt32(reader, 1);
        entities.Bankruptcy.BankruptcyType = db.GetString(reader, 2);
        entities.Bankruptcy.BankruptcyFilingDate = db.GetDate(reader, 3);
        entities.Bankruptcy.BankruptcyDischargeDate =
          db.GetNullableDate(reader, 4);
        entities.Bankruptcy.ExpectedBkrpDischargeDate =
          db.GetNullableDate(reader, 5);
        entities.Bankruptcy.BankruptcyDismissWithdrawDate =
          db.GetNullableDate(reader, 6);
        entities.Bankruptcy.Populated = true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case2.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.Case2.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case2.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.Case2.Status = db.GetNullableString(reader, 1);
        entities.CaseRole.CspNumber = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.Populated = true;
        entities.Case2.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Supported1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.Supported1.Populated = true;
      });
  }

  private bool ReadCsePersonObligor()
  {
    entities.CsePerson.Populated = false;
    entities.Obligor.Populated = false;

    return Read("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
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
        db.SetString(command, "debtTyp", local.Debt.DebtType);
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
        entities.Debt.CreatedBy = db.GetString(reader, 5);
        entities.Debt.DebtType = db.GetString(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 9);
        entities.DebtDetail.DueDt = db.GetDate(reader, 10);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 11);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 12);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 13);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 14);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 15);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
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
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.
          SetNullableString(command, "casNumber1", entities.CaseRole.CasNumber);
          
        db.SetNullableInt32(
          command, "croIdentifier1", entities.CaseRole.Identifier);
        db.SetNullableString(command, "croType1", entities.CaseRole.Type1);
        db.
          SetNullableString(command, "cspNumber1", entities.CaseRole.CspNumber);
          
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

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.ChildSupportLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.ChildSupportLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ChildSupportLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ChildSupportLegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.ChildSupportLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadObligationAdmActionExemptionAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AdministrativeAction.Populated = false;
    entities.ObligationAdmActionExemption.Populated = false;

    return ReadEach("ReadObligationAdmActionExemptionAdministrativeAction",
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
        db.SetDate(command, "endDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAdmActionExemption.OtyType = db.GetInt32(reader, 0);
        entities.ObligationAdmActionExemption.AatType = db.GetString(reader, 1);
        entities.AdministrativeAction.Type1 = db.GetString(reader, 1);
        entities.ObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ObligationAdmActionExemption.CpaType = db.GetString(reader, 4);
        entities.ObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ObligationAdmActionExemption.EndDate = db.GetDate(reader, 6);
        entities.ObligationAdmActionExemption.Reason = db.GetString(reader, 7);
        entities.ObligationAdmActionExemption.Description =
          db.GetNullableString(reader, 8);
        entities.AdministrativeAction.Populated = true;
        entities.ObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ObligationAdmActionExemption.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationLegalActionTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.Tribunal.Populated = false;
    entities.Obligation.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadObligationLegalActionTribunal",
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
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.Tribunal.Identifier = db.GetInt32(reader, 8);
        entities.Tribunal.Populated = true;
        entities.Obligation.Populated = true;
        entities.LegalAction.Populated = true;
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
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadOfficeServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(command, "spdId", entities.CaseAssignment.SpdId);
        db.SetInt32(command, "offId", entities.CaseAssignment.OffId);
        db.SetString(command, "ospCode", entities.CaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private CsePersonsWorkSet obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of BankrptAfter20051017.
    /// </summary>
    [JsonPropertyName("bankrptAfter20051017")]
    public Common BankrptAfter20051017
    {
      get => bankrptAfter20051017 ??= new();
      set => bankrptAfter20051017 = value;
    }

    /// <summary>
    /// A value of BankrptPriorTo20051017.
    /// </summary>
    [JsonPropertyName("bankrptPriorTo20051017")]
    public Common BankrptPriorTo20051017
    {
      get => bankrptPriorTo20051017 ??= new();
      set => bankrptPriorTo20051017 = value;
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
    /// A value of FederalDebtSetoff.
    /// </summary>
    [JsonPropertyName("federalDebtSetoff")]
    public AdministrativeActCertification FederalDebtSetoff
    {
      get => federalDebtSetoff ??= new();
      set => federalDebtSetoff = value;
    }

    /// <summary>
    /// A value of NontanfLt90DayArrears.
    /// </summary>
    [JsonPropertyName("nontanfLt90DayArrears")]
    public DebtDetail NontanfLt90DayArrears
    {
      get => nontanfLt90DayArrears ??= new();
      set => nontanfLt90DayArrears = value;
    }

    /// <summary>
    /// A value of NontanfGt90DayArrears.
    /// </summary>
    [JsonPropertyName("nontanfGt90DayArrears")]
    public DebtDetail NontanfGt90DayArrears
    {
      get => nontanfGt90DayArrears ??= new();
      set => nontanfGt90DayArrears = value;
    }

    /// <summary>
    /// A value of TanfLt90DayArrears.
    /// </summary>
    [JsonPropertyName("tanfLt90DayArrears")]
    public DebtDetail TanfLt90DayArrears
    {
      get => tanfLt90DayArrears ??= new();
      set => tanfLt90DayArrears = value;
    }

    /// <summary>
    /// A value of TanfGt90DayArrears.
    /// </summary>
    [JsonPropertyName("tanfGt90DayArrears")]
    public DebtDetail TanfGt90DayArrears
    {
      get => tanfGt90DayArrears ??= new();
      set => tanfGt90DayArrears = value;
    }

    /// <summary>
    /// A value of BankruptCh13.
    /// </summary>
    [JsonPropertyName("bankruptCh13")]
    public Common BankruptCh13
    {
      get => bankruptCh13 ??= new();
      set => bankruptCh13 = value;
    }

    private Common bankrptAfter20051017;
    private Common bankrptPriorTo20051017;
    private Bankruptcy bankruptcy;
    private AdministrativeActCertification federalDebtSetoff;
    private DebtDetail nontanfLt90DayArrears;
    private DebtDetail nontanfGt90DayArrears;
    private DebtDetail tanfLt90DayArrears;
    private DebtDetail tanfGt90DayArrears;
    private Common bankruptCh13;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A CaseGroup group.</summary>
    [Serializable]
    public class CaseGroup
    {
      /// <summary>
      /// A value of CaseCase.
      /// </summary>
      [JsonPropertyName("caseCase")]
      public Case1 CaseCase
      {
        get => caseCase ??= new();
        set => caseCase = value;
      }

      /// <summary>
      /// A value of CaseOffice.
      /// </summary>
      [JsonPropertyName("caseOffice")]
      public Office CaseOffice
      {
        get => caseOffice ??= new();
        set => caseOffice = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 caseCase;
      private Office caseOffice;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of CaseAsignFound.
    /// </summary>
    [JsonPropertyName("caseAsignFound")]
    public Common CaseAsignFound
    {
      get => caseAsignFound ??= new();
      set => caseAsignFound = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public Office Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of OpenCase.
    /// </summary>
    [JsonPropertyName("openCase")]
    public Common OpenCase
    {
      get => openCase ??= new();
      set => openCase = value;
    }

    /// <summary>
    /// Gets a value of Case1.
    /// </summary>
    [JsonIgnore]
    public Array<CaseGroup> Case1 => case1 ??= new(CaseGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Case1 for json serialization.
    /// </summary>
    [JsonPropertyName("case1")]
    [Computed]
    public IList<CaseGroup> Case1_Json
    {
      get => case1;
      set => Case1.Assign(value);
    }

    /// <summary>
    /// A value of FirstOfPpiMonth.
    /// </summary>
    [JsonPropertyName("firstOfPpiMonth")]
    public DateWorkArea FirstOfPpiMonth
    {
      get => firstOfPpiMonth ??= new();
      set => firstOfPpiMonth = value;
    }

    /// <summary>
    /// A value of WorkDate.
    /// </summary>
    [JsonPropertyName("workDate")]
    public TextWorkArea WorkDate
    {
      get => workDate ??= new();
      set => workDate = value;
    }

    /// <summary>
    /// A value of CaseCount.
    /// </summary>
    [JsonPropertyName("caseCount")]
    public Common CaseCount
    {
      get => caseCount ??= new();
      set => caseCount = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Child18OrOver.
    /// </summary>
    [JsonPropertyName("child18OrOver")]
    public Common Child18OrOver
    {
      get => child18OrOver ??= new();
      set => child18OrOver = value;
    }

    /// <summary>
    /// A value of FirstChildInd.
    /// </summary>
    [JsonPropertyName("firstChildInd")]
    public Common FirstChildInd
    {
      get => firstChildInd ??= new();
      set => firstChildInd = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of PpiMinus90Day.
    /// </summary>
    [JsonPropertyName("ppiMinus90Day")]
    public ProgramProcessingInfo PpiMinus90Day
    {
      get => ppiMinus90Day ??= new();
      set => ppiMinus90Day = value;
    }

    /// <summary>
    /// A value of Local1StRunMonthMinus90Day.
    /// </summary>
    [JsonPropertyName("local1StRunMonthMinus90Day")]
    public ProgramProcessingInfo Local1StRunMonthMinus90Day
    {
      get => local1StRunMonthMinus90Day ??= new();
      set => local1StRunMonthMinus90Day = value;
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
    /// A value of Posn.
    /// </summary>
    [JsonPropertyName("posn")]
    public Common Posn
    {
      get => posn ??= new();
      set => posn = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public Common Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
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
    /// A value of GoodCauseFoundForDebt.
    /// </summary>
    [JsonPropertyName("goodCauseFoundForDebt")]
    public Common GoodCauseFoundForDebt
    {
      get => goodCauseFoundForDebt ??= new();
      set => goodCauseFoundForDebt = value;
    }

    private CsePersonAddress csePersonAddress;
    private Common caseAsignFound;
    private Office hold;
    private Common openCase;
    private Array<CaseGroup> case1;
    private DateWorkArea firstOfPpiMonth;
    private TextWorkArea workDate;
    private Common caseCount;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private Program program;
    private Common child18OrOver;
    private Common firstChildInd;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ObligationTransaction debt;
    private ProgramProcessingInfo ppiMinus90Day;
    private ProgramProcessingInfo local1StRunMonthMinus90Day;
    private ObligationType obligationType;
    private Common posn;
    private Common zero;
    private DprProgram dprProgram;
    private Common goodCauseFoundForAp;
    private Common goodCauseFoundForDebt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public Case1 Case2
    {
      get => case2 ??= new();
      set => case2 = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ChildSupportLegalAction.
    /// </summary>
    [JsonPropertyName("childSupportLegalAction")]
    public LegalAction ChildSupportLegalAction
    {
      get => childSupportLegalAction ??= new();
      set => childSupportLegalAction = value;
    }

    /// <summary>
    /// A value of ChildSupportObligation.
    /// </summary>
    [JsonPropertyName("childSupportObligation")]
    public Obligation ChildSupportObligation
    {
      get => childSupportObligation ??= new();
      set => childSupportObligation = value;
    }

    /// <summary>
    /// A value of ChildSupportObligationType.
    /// </summary>
    [JsonPropertyName("childSupportObligationType")]
    public ObligationType ChildSupportObligationType
    {
      get => childSupportObligationType ??= new();
      set => childSupportObligationType = value;
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

    private Bankruptcy bankruptcy;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case2;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private CsePerson supported1;
    private CsePersonAccount supported2;
    private Tribunal tribunal;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private AdministrativeAction administrativeAction;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private LegalAction legalAction;
    private CsePersonAccount obligor;
    private LegalAction childSupportLegalAction;
    private Obligation childSupportObligation;
    private ObligationType childSupportObligationType;
    private GoodCause goodCause;
  }
#endregion
}
