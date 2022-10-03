// Program: LE_BFX9_PROCESS_NCP, ID: 1902447930, model: 746.
// Short name: SWE02140
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_BFX9_PROCESS_NCP.
/// </summary>
[Serializable]
public partial class LeBfx9ProcessNcp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_BFX9_PROCESS_NCP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeBfx9ProcessNcp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeBfx9ProcessNcp.
  /// </summary>
  public LeBfx9ProcessNcp(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------------------
    // Business Rules
    // Pilot Group Data Analysis - added 10/9/14
    // The following data elements will be pulled for the NCPs (unless 
    // otherwise noted).  The data elements will be sorted by Statewide (
    // qualifying NCP/CH participants), Pilot Counties (SG and WY) and Pilot
    // Sample (1,000 chosen by research group).
    // 	26. Date of Birth
    // 	27. Gender
    // 	    a. Male
    // 	    b. Female
    // 	28. Race/ethnicity
    // 	    a. AI-American Indian/Alaskan Native
    // 	    b. AJ-American Indian/Tribal Job
    // 	    c. BL-Black/African American
    // 	    d. DC-Declined
    // 	    e. HI-Hispanic
    // 	    f. HP-Native Hawaiian/Pacific Islander
    // 	    g. OT-Other
    // 	    h. SA-Asian
    // 	    i. WH-White
    // 	    j. Blank
    // 	29. Currently employed (Y/N)
    // 	    a. At least one active (non-end dated) employer (INCS) exists for 
    // the
    // 	       NCP as of the run date.
    // 	    b. Count only records with one of the following types: E(mployment),
    // 	       M(ilitary), O(ther).  Do not count R(esource) type.
    // 	    c. Count records with the following Type/Return Code combinations:
    // 	       E/E, M/A, M/R and O/V. Do not count records with the following
    // 	       Type/Return Code combinations: E/F, E/L, E/N, E/O, E/Q, E/U, E/W,
    // 	       M/I, M/N, M/U, O/N.
    // 	30. Earned income in the past year- use income source for the last four
    // 	    quarters to calculate the total.  Include active and inactive income
    // 	    sources.
    // 	31. Number of child support courts order for the NCP. The court orders
    // 	    will be active and obligated.
    // 	32. Family arrears balance on all qualifying orders.  Included NA and 
    // NAI.
    // 	33. Total arrears owed across all orders for the qualifying NCP.
    // 	34. Date of last incoming withholding (I) payment for qualifying NCP.
    // 	35. Date of last UI withholding (U) payment for qualifying NCP.
    // 	36. Date of most recent payment for qualifying NCP. Include REIP and
    // 	    CSENet payments.
    // 	37. New Order for qualifying NCP. Count if there isnt a payment in the
    // 	    last year and the oldest created timestamp of the debts is within 
    // the
    // 	    previous 12 months. Y/N
    // 	38. Number of minor age children.
    // 	    a. Count all children under the age of 18.
    // 	    b. The child must be tied to the qualifying NCP.
    // 	    c. The NCP must be the NCP or CP on the qualifying case.
    // 	39. NCP ever incarcerated in jail or prison. Include only the date of 
    // the
    // 	    most recent record.
    // 	    a. Most recent start date or verified date entered jail or prison.
    // 	       This may be blank.
    // 	    OR
    // 	    b. Most recent release date from jail or prison. This may be blank.
    // 	
    // The following data elements will be on a separate file.
    // 	40. Federal benefit receipt (Include all open programs). If there are 
    // not
    // 	    any open programs display as blank. This is  for NCP, CP and CH that
    // 	    meet qualifications. Indicate a "Y" for each participant if any of
    // 	    the following programs are open, if the participant isnt open for 
    // any
    // 	    of the programs indicate "N":
    // 	    a. AF    AFDC
    // 	    b. AFI   AFDC INTERSTATE
    // 	    c. CC    CHILD CARE
    // 	    d. CI    MA - FOR CHILD IN AN INSTITUTION
    // 	    e. FC    AFDC FOSTER CARE
    // 	    f. FCI   FOSTER CARE INTERSTATE
    // 	    g. FS    FOOD STAMPS
    // 	    h. MA    MEDICAID RELATED TO TAF
    // 	    i. MAI   MEDICAL ASSISTANCE INTERSTATE
    // 	    j. MK    POVERTY LEVEL MEDICAID (OLD)
    // 	    k. MP    MEDICAL PG/CHILD/HEALTHWAVE
    // 	    l. MS    MEDICAID RELATED TO SSI
    // 	    m. NA    NON - AFDC
    // 	    n. NAI   NON - AFDC INTERSTATE
    // 	    o. NC    JUVENILE JUSTICE AUTHORITY
    // 	    p. NF    GA FOSTER CARE
    // 	    q. SI    MA - CHILD RECEIVES SSI
    // 	41. Count any qualifying NCP/CH combination where there is an accruing
    // 	    child support debt detail for the current month. Y/N
    // 	42. Count if the qualifying NCP/CH combination is arrears only, no
    // 	    current support owed.  Y/N
    // 	43. Birthdate of each qualifying child.
    // --------------------------------------------------------------------------------------------------
    // -- Views local program_procssing_info process_date and local_current 
    // work_area date
    //    are set to the same value and used interchangably.
    //    Both views are set to the import program_processing_info process_date.
    local.Current.Date = import.ProgramProcessingInfo.ProcessDate;
    local.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.FirstOfMonth.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate,
      -(Day(local.ProgramProcessingInfo.ProcessDate) - 1));
    UseFnHardcodedDebtDistribution();
    local.Ap.Type1 = "AP";
    local.Ar.Type1 = "AR";
    local.Default1.AfIndicator = "N";
    local.Default1.AfiIndicator = "N";
    local.Default1.CcIndicator = "N";
    local.Default1.CiIndicator = "N";
    local.Default1.FcIndicator = "N";
    local.Default1.FciIndicator = "N";
    local.Default1.FsIndicator = "N";
    local.Default1.MaIndicator = "N";
    local.Default1.MaiIndicator = "N";
    local.Default1.MkIndicator = "N";
    local.Default1.MpIndicator = "N";
    local.Default1.MsIndicator = "N";
    local.Default1.NaIndicator = "N";
    local.Default1.NaiIndicator = "N";
    local.Default1.NcIndicator = "N";
    local.Default1.NfIndicator = "N";
    local.Default1.SiIndicator = "N";

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	26. Date of Birth
    // 	27. Gender
    // 	    a. Male
    // 	    b. Female
    // --------------------------------------------------------------------------------------------------
    // 	
    local.NcpCsePersonsWorkSet.Number = import.Ncp.Number;
    UseSiReadCsePerson2();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error Reading ADABAS...NCP " + entities
        .NcpCsePerson.Number + " " + local.ExitStateWorkArea.Message;
      UseCabErrorReport();

      return;
    }

    local.NcpDob.Text10 =
      NumberToString(Month(local.NcpCsePersonsWorkSet.Dob), 14, 2) + "-" + NumberToString
      (Day(local.NcpCsePersonsWorkSet.Dob), 14, 2) + "-" + NumberToString
      (Year(local.NcpCsePersonsWorkSet.Dob), 12, 4);

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	28. Race/ethnicity
    // 	    a. AI-American Indian/Alaskan Native
    // 	    b. AJ-American Indian/Tribal Job
    // 	    c. BL-Black/African American
    // 	    d. DC-Declined
    // 	    e. HI-Hispanic
    // 	    f. HP-Native Hawaiian/Pacific Islander
    // 	    g. OT-Other
    // 	    h. SA-Asian
    // 	    i. WH-White
    // 	    j. Blank
    // --------------------------------------------------------------------------------------------------
    if (!ReadCsePerson3())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	29. Currently employed (Y/N)
    // 	    a. At least one active (non-end dated) employer (INCS) exists for 
    // the
    // 	       NCP as of the run date.
    // 	    b. Count only records with one of the following types: E(mployment),
    // 	       M(ilitary), O(ther).  Do not count R(esource) type.
    // 	    c. Count records with the following Type/Return Code combinations:
    // 	       E/E, M/A, M/R and O/V. Do not count records with the following
    // 	       Type/Return Code combinations: E/F, E/L, E/N, E/O, E/Q, E/U, E/W,
    // 	       M/I, M/N, M/U, O/N.
    // 	30. Earned income in the past year- use income source for the last four
    // 	    quarters to calculate the total.  Include active and inactive income
    // 	    sources.
    // --------------------------------------------------------------------------------------------------
    local.IncomeSource.LastQtr = "1";
    local.IncomeSource.Attribute2NdQtr = "2";
    local.IncomeSource.Attribute3RdQtr = "3";
    local.IncomeSource.Attribute4ThQtr = "4";

    if (Month(local.Current.Date) <= 3)
    {
      local.IncomeSource.LastQtrYr = Year(local.Current.Date) - 1;
      local.IncomeSource.Attribute2NdQtrYr = Year(local.Current.Date) - 1;
      local.IncomeSource.Attribute3RdQtrYr = Year(local.Current.Date) - 1;
      local.IncomeSource.Attribute4ThQtrYr = Year(local.Current.Date) - 1;
    }
    else if (Month(local.Current.Date) <= 6)
    {
      local.IncomeSource.LastQtrYr = Year(local.Current.Date);
      local.IncomeSource.Attribute2NdQtrYr = Year(local.Current.Date) - 1;
      local.IncomeSource.Attribute3RdQtrYr = Year(local.Current.Date) - 1;
      local.IncomeSource.Attribute4ThQtrYr = Year(local.Current.Date) - 1;
    }
    else if (Month(local.Current.Date) <= 9)
    {
      local.IncomeSource.LastQtrYr = Year(local.Current.Date);
      local.IncomeSource.Attribute2NdQtrYr = Year(local.Current.Date);
      local.IncomeSource.Attribute3RdQtrYr = Year(local.Current.Date) - 1;
      local.IncomeSource.Attribute4ThQtrYr = Year(local.Current.Date) - 1;
    }
    else
    {
      local.IncomeSource.LastQtrYr = Year(local.Current.Date);
      local.IncomeSource.Attribute2NdQtrYr = Year(local.Current.Date);
      local.IncomeSource.Attribute3RdQtrYr = Year(local.Current.Date);
      local.IncomeSource.Attribute4ThQtrYr = Year(local.Current.Date) - 1;
    }

    local.NcpCurrentlyEmployed.Flag = "N";

    foreach(var item in ReadIncomeSource())
    {
      if (Lt(local.Current.Date, entities.IncomeSource.EndDt))
      {
        switch(AsChar(entities.IncomeSource.Type1))
        {
          case 'E':
            if (AsChar(entities.IncomeSource.ReturnCd) == 'E')
            {
              local.NcpCurrentlyEmployed.Flag = "Y";
            }

            break;
          case 'M':
            if (AsChar(entities.IncomeSource.ReturnCd) == 'A')
            {
              local.NcpCurrentlyEmployed.Flag = "Y";
            }

            if (AsChar(entities.IncomeSource.ReturnCd) == 'R')
            {
              local.NcpCurrentlyEmployed.Flag = "Y";
            }

            break;
          case 'O':
            if (AsChar(entities.IncomeSource.ReturnCd) == 'V')
            {
              local.NcpCurrentlyEmployed.Flag = "Y";
            }

            break;
          default:
            break;
        }
      }

      if (AsChar(entities.IncomeSource.LastQtr) == AsChar
        (local.IncomeSource.LastQtr) && Equal
        (entities.IncomeSource.LastQtrYr,
        local.IncomeSource.LastQtrYr.GetValueOrDefault()))
      {
        local.IncomeSource.LastQtrIncome =
          local.IncomeSource.LastQtrIncome.GetValueOrDefault() + entities
          .IncomeSource.LastQtrIncome.GetValueOrDefault();
      }

      if (AsChar(entities.IncomeSource.Attribute2NdQtr) == AsChar
        (local.IncomeSource.Attribute2NdQtr) && Equal
        (entities.IncomeSource.Attribute2NdQtrYr,
        local.IncomeSource.Attribute2NdQtrYr.GetValueOrDefault()))
      {
        local.IncomeSource.Attribute2NdQtrIncome =
          local.IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault() + entities
          .IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault();
      }

      if (AsChar(entities.IncomeSource.Attribute3RdQtr) == AsChar
        (local.IncomeSource.Attribute3RdQtr) && Equal
        (entities.IncomeSource.Attribute3RdQtrYr,
        local.IncomeSource.Attribute3RdQtrYr.GetValueOrDefault()))
      {
        local.IncomeSource.Attribute3RdQtrIncome =
          local.IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault() + entities
          .IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault();
      }

      if (AsChar(entities.IncomeSource.Attribute4ThQtr) == AsChar
        (local.IncomeSource.Attribute4ThQtr) && Equal
        (entities.IncomeSource.Attribute4ThQtrYr,
        local.IncomeSource.Attribute4ThQtrYr.GetValueOrDefault()))
      {
        local.IncomeSource.Attribute4ThQtrIncome =
          local.IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault() + entities
          .IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault();
      }
    }

    local.NcpIncomeLast4QtrsCommon.TotalCurrency =
      local.IncomeSource.LastQtrIncome.GetValueOrDefault() + local
      .IncomeSource.Attribute2NdQtrIncome.GetValueOrDefault() + local
      .IncomeSource.Attribute3RdQtrIncome.GetValueOrDefault() + local
      .IncomeSource.Attribute4ThQtrIncome.GetValueOrDefault();
    local.NcpIncomeLast4QtrsTextWorkArea.Text10 =
      NumberToString((long)local.NcpIncomeLast4QtrsCommon.TotalCurrency, 9, 7) +
      "." + NumberToString
      ((long)(local.NcpIncomeLast4QtrsCommon.TotalCurrency * 100), 14, 2);

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	31. Number of child support court orders for the NCP. The court orders
    // 	    will be active and obligated.
    // --------------------------------------------------------------------------------------------------
    ReadLegalAction();
    local.NumberChildSupportOrderWorkArea.Text3 =
      NumberToString(local.NumberChildSupportOrderCommon.Count, 13, 3);

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	32. Family arrears balance on all qualifying orders.  Included NA and 
    // NAI.
    // --------------------------------------------------------------------------------------------------
    // 	
    // --  This requires eligibility logic.  This calculation is done below and 
    // put into local_total_ncp_family_arrears ief_supplied total_currency.
    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	33. Total arrears owed across all orders for the qualifying NCP.
    // --------------------------------------------------------------------------------------------------
    // 	
    ReadDebtDetail();
    local.NcpTotalArrearsBalanceTextWorkArea.Text10 =
      NumberToString((long)local.NcpTotalArrearsBalanceCommon.TotalCurrency, 9,
      7) + "." + NumberToString
      ((long)(local.NcpTotalArrearsBalanceCommon.TotalCurrency * 100), 14, 2);

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	34. Date of last incoming withholding (I) payment for qualifying NCP.
    // --------------------------------------------------------------------------------------------------
    // 	
    local.Itype.CollectionDate = new DateTime(1, 1, 1);

    if (ReadCashReceiptDetail1())
    {
      local.Itype.CollectionDate = entities.CashReceiptDetail.CollectionDate;
    }

    local.LastIwoPaymentDate.Text10 =
      NumberToString(Month(local.Itype.CollectionDate), 14, 2) + "-" + NumberToString
      (Day(local.Itype.CollectionDate), 14, 2) + "-" + NumberToString
      (Year(local.Itype.CollectionDate), 12, 4);

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	35. Date of last UI withholding (U) payment for qualifying NCP.
    // --------------------------------------------------------------------------------------------------
    // 	
    local.Utype.CollectionDate = new DateTime(1, 1, 1);

    if (ReadCashReceiptDetail2())
    {
      local.Utype.CollectionDate = entities.CashReceiptDetail.CollectionDate;
    }

    local.LastUiPaymentDate.Text10 =
      NumberToString(Month(local.Utype.CollectionDate), 14, 2) + "-" + NumberToString
      (Day(local.Utype.CollectionDate), 14, 2) + "-" + NumberToString
      (Year(local.Utype.CollectionDate), 12, 4);

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	36. Date of most recent payment for qualifying NCP. Include REIP and
    // 	    CSENet payments.
    // --------------------------------------------------------------------------------------------------
    // 	
    local.AllTypes.CollectionDate = new DateTime(1, 1, 1);

    if (ReadCashReceiptDetail3())
    {
      local.AllTypes.CollectionDate = entities.CashReceiptDetail.CollectionDate;
    }

    local.MostRecentPaymentDate.Text10 =
      NumberToString(Month(local.AllTypes.CollectionDate), 14, 2) + "-" + NumberToString
      (Day(local.AllTypes.CollectionDate), 14, 2) + "-" + NumberToString
      (Year(local.AllTypes.CollectionDate), 12, 4);

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	37. New Order for qualifying NCP. Count if there isnt a payment in the
    // 	    last year and the oldest created timestamp of the debts is within 
    // the
    // 	    previous 12 months. Y/N
    // --------------------------------------------------------------------------------------------------
    // 	
    ReadDebt();

    if (Lt(Now().AddYears(-1), entities.Oldest.CreatedTmst) && Lt
      (local.AllTypes.CollectionDate,
      AddYears(local.ProgramProcessingInfo.ProcessDate, -1)))
    {
      local.NewOrder.Flag = "Y";
    }
    else
    {
      local.NewOrder.Flag = "N";
    }

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	38. Number of minor age children.
    // 	    a. Count all children under the age of 18.
    // 	    b. The child must be tied to the qualifying NCP.
    // 	    c. The NCP must be the NCP or CP on the qualifying case.
    // --------------------------------------------------------------------------------------------------
    // 	
    foreach(var item in ReadCsePerson4())
    {
      // -- Get child date of birth from adabase
      local.Child1.Number = entities.DistinctChild.Number;
      UseSiReadCsePerson1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Reading ADABAS...CH " + entities
          .Ch.Number + " " + local.ExitStateWorkArea.Message;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      if (!Lt(AddYears(local.ProgramProcessingInfo.ProcessDate, -18),
        local.Child1.Dob))
      {
        // -- skip if CH is oder than 18 years of age
        continue;
      }

      ++local.NumberOfChildren.Count;
    }

    local.NumberOfMinorChildren.Text2 =
      NumberToString(local.NumberOfChildren.Count, 14, 2);

    // --------------------------------------------------------------------------------------------------
    // NCP Info
    // 	39. NCP ever incarcerated in jail or prison. Include only the date of 
    // the
    // 	    most recent record.
    // 	    a. Most recent start date or verified date entered jail or prison.
    // 	       This may be blank.
    // 	    OR
    // 	    b. Most recent release date from jail or prison. This may be blank.
    // --------------------------------------------------------------------------------------------------
    // 	
    local.Incarceration.StartDate = new DateTime(1, 1, 1);
    local.Incarceration.EndDate = new DateTime(1, 1, 1);

    if (ReadIncarceration())
    {
      MoveIncarceration(entities.Incarceration, local.Incarceration);
    }

    local.IncarcerationStartDate.Text10 =
      NumberToString(Month(local.Incarceration.StartDate), 14, 2) + "-" + NumberToString
      (Day(local.Incarceration.StartDate), 14, 2) + "-" + NumberToString
      (Year(local.Incarceration.StartDate), 12, 4);
    local.IncarcerationEndDate.Text10 =
      NumberToString(Month(local.Incarceration.EndDate), 14, 2) + "-" + NumberToString
      (Day(local.Incarceration.EndDate), 14, 2) + "-" + NumberToString
      (Year(local.Incarceration.EndDate), 12, 4);

    // The following data elements will be on the NCP/CH/CP info output file.
    // --------------------------------------------------------------------------------------------------
    // NCP/CH/CP Info
    // 	40. Federal benefit receipt (Include all open programs). If there are 
    // not
    // 	    any open programs display as blank. This is  for NCP, CP and CH that
    // 	    meet qualifications. Indicate a "Y" for each participant if any of
    // 	    the following programs are open, if the participant isnt open for 
    // any
    // 	    of the programs indicate "N":
    // 	    a. AF    AFDC
    // 	    b. AFI   AFDC INTERSTATE
    // 	    c. CC    CHILD CARE
    // 	    d. CI    MA - FOR CHILD IN AN INSTITUTION
    // 	    e. FC    AFDC FOSTER CARE
    // 	    f. FCI   FOSTER CARE INTERSTATE
    // 	    g. FS    FOOD STAMPS
    // 	    h. MA    MEDICAID RELATED TO TAF
    // 	    i. MAI   MEDICAL ASSISTANCE INTERSTATE
    // 	    j. MK    POVERTY LEVEL MEDICAID (OLD)
    // 	    k. MP    MEDICAL PG/CHILD/HEALTHWAVE
    // 	    l. MS    MEDICAID RELATED TO SSI
    // 	    m. NA    NON - AFDC
    // 	    n. NAI   NON - AFDC INTERSTATE
    // 	    o. NC    JUVENILE JUSTICE AUTHORITY
    // 	    p. NF    GA FOSTER CARE
    // 	    q. SI    MA - CHILD RECEIVES SSI
    // --------------------------------------------------------------------------------------------------
    local.CssiProgramWorkset.Assign(local.Default1);

    foreach(var item in ReadPersonProgramProgram3())
    {
      switch(TrimEnd(entities.Program.Code))
      {
        case "AF":
          local.CssiProgramWorkset.AfIndicator = "Y";

          break;
        case "AFI":
          local.CssiProgramWorkset.AfiIndicator = "Y";

          break;
        case "CC":
          local.CssiProgramWorkset.CcIndicator = "Y";

          break;
        case "CI":
          local.CssiProgramWorkset.CiIndicator = "Y";

          break;
        case "FC":
          local.CssiProgramWorkset.FcIndicator = "Y";

          break;
        case "FCI":
          local.CssiProgramWorkset.FciIndicator = "Y";

          break;
        case "FS":
          local.CssiProgramWorkset.FsIndicator = "Y";

          break;
        case "MA":
          local.CssiProgramWorkset.MaIndicator = "Y";

          break;
        case "MAI":
          local.CssiProgramWorkset.MaiIndicator = "Y";

          break;
        case "MK":
          local.CssiProgramWorkset.MkIndicator = "Y";

          break;
        case "MP":
          local.CssiProgramWorkset.MpIndicator = "Y";

          break;
        case "MS":
          local.CssiProgramWorkset.MsIndicator = "Y";

          break;
        case "NA":
          local.CssiProgramWorkset.NaIndicator = "Y";

          break;
        case "NAI":
          local.CssiProgramWorkset.NaiIndicator = "Y";

          break;
        case "NC":
          local.CssiProgramWorkset.NcIndicator = "Y";

          break;
        case "NF":
          local.CssiProgramWorkset.NfIndicator = "Y";

          break;
        case "SI":
          local.CssiProgramWorkset.SiIndicator = "Y";

          break;
        default:
          break;
      }
    }

    local.NcpCssiProgramWorkset.Assign(local.CssiProgramWorkset);
    local.ChildFound.Flag = "N";

    // ------------------------------------------------------------------------------------------------
    // --  Elegibility logic from the mailer program (SWELBFX7) is duplicated 
    // below to
    // --  Determine which children/CPs are eligible and have only family owed 
    // arrears.
    // --  Some logic has been commented out if it was necessary for this 
    // process.
    // ------------------------------------------------------------------------------------------------
    foreach(var item in ReadCsePersonObligorSupported())
    {
      if (ReadCsePerson1())
      {
        if (Lt(local.Null1.Date, entities.CsePerson.DateOfDeath))
        {
          // -- Skip if CH is deceased.
          continue;
        }

        // -- Get child date of birth from adabase
        local.Child1.Number = entities.Ch.Number;
        UseSiReadCsePerson1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error Reading ADABAS...CH " + entities
            .Ch.Number + " " + local.ExitStateWorkArea.Message;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }

        if (!Lt(AddYears(local.ProgramProcessingInfo.ProcessDate, -18),
          local.Child1.Dob))
        {
          // -- skip if CH is oder than 18 years of age
          continue;
        }
      }

      // -- Initialize local group
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        local.Local1.Update.G.CountyAbbreviation = "";
        local.Local1.Update.G.StateAbbreviation = "";
        local.Local1.Update.GlocalStateOwed.TotalCurrency = 0;
        local.Local1.Update.GlocalFamilyOwed.TotalCurrency = 0;
      }

      local.Local1.CheckIndex();
      local.Local1.Count = 0;

      // -- This is new for BFX9.
      local.CurrentSupport.Flag = "N";
      local.ChFamilyArrears.TotalCurrency = 0;

      // There must be one or more active arrears debts for the NCP/CH which is 
      // either state owed (AF/FC/NC/NF) or owed to the family (NA/NAI).
      foreach(var item1 in ReadDebtDetailObligationObligationType())
      {
        if (AsChar(entities.ObligationType.Classification) == 'A' && !
          Lt(entities.DebtDetail.DueDt, local.FirstOfMonth.Date))
        {
          // -- Skip this debt.  It is for current support.
          // -- This is new for BFX9.
          local.CurrentSupport.Flag = "Y";

          continue;
        }

        UseFnDeterminePgmForDebtDetail();

        switch(TrimEnd(local.Program.Code))
        {
          case "AF":
            break;
          case "FC":
            break;
          case "NC":
            break;
          case "NF":
            break;
          case "NA":
            break;
          case "NAI":
            break;
          default:
            continue;
        }

        // -- This was added for BFX9.
        if (Equal(local.Program.Code, "NA") || Equal(local.Program.Code, "NAI"))
        {
          local.ChFamilyArrears.TotalCurrency += entities.DebtDetail.
            BalanceDueAmt;
        }

        if (ReadFips())
        {
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            if (Equal(local.Local1.Item.G.StateAbbreviation,
              entities.Fips.StateAbbreviation) && Equal
              (local.Local1.Item.G.CountyAbbreviation,
              entities.Fips.CountyAbbreviation))
            {
              if (Equal(local.Program.Code, "NA") || Equal
                (local.Program.Code, "NAI"))
              {
                local.Local1.Update.GlocalFamilyOwed.TotalCurrency =
                  local.Local1.Item.GlocalFamilyOwed.TotalCurrency + entities
                  .DebtDetail.BalanceDueAmt;

                goto Read;
              }

              local.Local1.Update.GlocalStateOwed.TotalCurrency =
                local.Local1.Item.GlocalStateOwed.TotalCurrency + entities
                .DebtDetail.BalanceDueAmt;

              goto Read;
            }
          }

          local.Local1.CheckIndex();

          local.Local1.Index = local.Local1.Count;
          local.Local1.CheckSize();

          MoveFips(entities.Fips, local.Local1.Update.G);

          if (Equal(local.Program.Code, "NA") || Equal
            (local.Program.Code, "NAI"))
          {
            local.Local1.Update.GlocalFamilyOwed.TotalCurrency =
              entities.DebtDetail.BalanceDueAmt;

            goto Read;
          }

          local.Local1.Update.GlocalStateOwed.TotalCurrency =
            entities.DebtDetail.BalanceDueAmt;
        }
        else
        {
          // --  Read FIPS Trib Address for foreign orders...
          if (ReadFipsTribAddress())
          {
            for(local.Local1.Index = 0; local.Local1.Index < local
              .Local1.Count; ++local.Local1.Index)
            {
              if (!local.Local1.CheckSize())
              {
                break;
              }

              if (Equal(local.Local1.Item.G.StateAbbreviation,
                entities.FipsTribAddress.Country) && IsEmpty
                (local.Local1.Item.G.CountyAbbreviation))
              {
                if (Equal(local.Program.Code, "NA") || Equal
                  (local.Program.Code, "NAI"))
                {
                  local.Local1.Update.GlocalFamilyOwed.TotalCurrency =
                    local.Local1.Item.GlocalFamilyOwed.TotalCurrency + entities
                    .DebtDetail.BalanceDueAmt;

                  goto Read;
                }

                local.Local1.Update.GlocalStateOwed.TotalCurrency =
                  local.Local1.Item.GlocalStateOwed.TotalCurrency + entities
                  .DebtDetail.BalanceDueAmt;

                goto Read;
              }
            }

            local.Local1.CheckIndex();

            local.Local1.Index = local.Local1.Count;
            local.Local1.CheckSize();

            local.Local1.Update.G.StateAbbreviation =
              entities.FipsTribAddress.Country ?? Spaces(2);
            local.Local1.Update.G.CountyAbbreviation = "";

            if (Equal(local.Program.Code, "NA") || Equal
              (local.Program.Code, "NAI"))
            {
              local.Local1.Update.GlocalFamilyOwed.TotalCurrency =
                entities.DebtDetail.BalanceDueAmt;

              goto Read;
            }

            local.Local1.Update.GlocalStateOwed.TotalCurrency =
              entities.DebtDetail.BalanceDueAmt;
          }
        }

Read:
        ;
      }

      if (local.Local1.Count == 0)
      {
        // -- Skip the NCP/CH.  There are no state or family owed arrears for 
        // the NCP/CH.
        continue;
      }

      // --   If a case exists with the NCP/CH both currently active that case 
      // will be used
      //      for the 529 letter.
      ReadCaseChild();

      if (entities.Case1.Populated)
      {
      }
      else
      {
        // -- Find case where NCP/CH were previously active....
        // --   If a case exists where the NCP/CH are both active then that case
        // will be used
        //      for the 529 letter.  Otherwise, the case where the NCP/CH were 
        // most
        //      recently active will be used.
        local.Case1.Number = "";
        local.Overlap.Date = local.Null1.Date;
        local.DateWorkArea.Date = local.Null1.Date;

        foreach(var item1 in ReadCaseChildAbsentParent())
        {
          if (Lt(entities.AbsentParent.StartDate, entities.Child.StartDate))
          {
            local.DateWorkArea.Date = entities.Child.StartDate;
          }
          else
          {
            local.DateWorkArea.Date = entities.AbsentParent.StartDate;
          }

          if (Lt(local.Overlap.Date, local.DateWorkArea.Date))
          {
            // -- Use case where AP/Supported combo became active most recently.
            local.Overlap.Date = local.DateWorkArea.Date;
            local.Case1.Number = entities.Case1.Number;
            local.Child2.Assign(entities.Child);
          }
        }

        if (!IsEmpty(local.Case1.Number))
        {
          // -- Reestablish currency on Case and Child.
          if (ReadCase())
          {
            ReadChild();
          }
        }
      }

      if (entities.Case1.Populated)
      {
      }
      else
      {
        // -- Skip NCP/CH since there is no common case.
        continue;
      }

      // -- Find the CP on the case...
      ReadCsePerson2();

      // -- Find Office where case is assigned.
      if (!ReadOffice())
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Skipped - Case assignment not found for case " + entities
          .Case1.Number;
        UseCabErrorReport();

        continue;
      }

      local.StateOwedArrears.Flag = "N";

      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (local.Local1.Item.GlocalStateOwed.TotalCurrency > 0)
        {
          // -- This flag is used to determine which flyer the NCP will receive 
          // (family arrears only or state arrears)
          local.StateOwedArrears.Flag = "Y";
        }
      }

      local.Local1.CheckIndex();

      if (entities.Office.SystemGeneratedId == 51 || AsChar
        (entities.Child.FcParentalRights) != 'N' && !
        IsEmpty(entities.Child.FcParentalRights))
      {
        // -- Cases in office 51 and cases where parental rights are severed 
        // will be worked manually by central office.
        //    Do not generate letters for these cases.
        continue;
      }

      // -- Everything below is new for BFX9...
      // -- Continue only if no state owed arrears.
      if (AsChar(local.StateOwedArrears.Flag) == 'Y')
      {
        continue;
      }

      // --------------------------------------------------------------------------------------------------
      // NCP Info
      // 	32. Family arrears balance on all qualifying orders.  Included NA and 
      // NAI.
      // --------------------------------------------------------------------------------------------------
      local.TotalNcpFamilyArrears.TotalCurrency += local.ChFamilyArrears.
        TotalCurrency;

      // --------------------------------------------------------------------------------------------------
      // NCP/CH/CP Info
      // 	40. Federal benefit receipt (Include all open programs). If there are 
      // not
      // 	    any open programs display as blank. This is  for NCP, CP and CH 
      // that
      // 	    meet qualifications. Indicate a "Y" for each participant if any of
      // 	    the following programs are open, if the participant isnt open for
      // any
      // 	    of the programs indicate "N":
      // 	    a. AF    AFDC
      // 	    b. AFI   AFDC INTERSTATE
      // 	    c. CC    CHILD CARE
      // 	    d. CI    MA - FOR CHILD IN AN INSTITUTION
      // 	    e. FC    AFDC FOSTER CARE
      // 	    f. FCI   FOSTER CARE INTERSTATE
      // 	    g. FS    FOOD STAMPS
      // 	    h. MA    MEDICAID RELATED TO TAF
      // 	    i. MAI   MEDICAL ASSISTANCE INTERSTATE
      // 	    j. MK    POVERTY LEVEL MEDICAID (OLD)
      // 	    k. MP    MEDICAL PG/CHILD/HEALTHWAVE
      // 	    l. MS    MEDICAID RELATED TO SSI
      // 	    m. NA    NON - AFDC
      // 	    n. NAI   NON - AFDC INTERSTATE
      // 	    o. NC    JUVENILE JUSTICE AUTHORITY
      // 	    p. NF    GA FOSTER CARE
      // 	    q. SI    MA - CHILD RECEIVES SSI
      // --------------------------------------------------------------------------------------------------
      local.CssiProgramWorkset.Assign(local.Default1);

      foreach(var item1 in ReadPersonProgramProgram1())
      {
        switch(TrimEnd(entities.Program.Code))
        {
          case "AF":
            local.CssiProgramWorkset.AfIndicator = "Y";

            break;
          case "AFI":
            local.CssiProgramWorkset.AfiIndicator = "Y";

            break;
          case "CC":
            local.CssiProgramWorkset.CcIndicator = "Y";

            break;
          case "CI":
            local.CssiProgramWorkset.CiIndicator = "Y";

            break;
          case "FC":
            local.CssiProgramWorkset.FcIndicator = "Y";

            break;
          case "FCI":
            local.CssiProgramWorkset.FciIndicator = "Y";

            break;
          case "FS":
            local.CssiProgramWorkset.FsIndicator = "Y";

            break;
          case "MA":
            local.CssiProgramWorkset.MaIndicator = "Y";

            break;
          case "MAI":
            local.CssiProgramWorkset.MaiIndicator = "Y";

            break;
          case "MK":
            local.CssiProgramWorkset.MkIndicator = "Y";

            break;
          case "MP":
            local.CssiProgramWorkset.MpIndicator = "Y";

            break;
          case "MS":
            local.CssiProgramWorkset.MsIndicator = "Y";

            break;
          case "NA":
            local.CssiProgramWorkset.NaIndicator = "Y";

            break;
          case "NAI":
            local.CssiProgramWorkset.NaiIndicator = "Y";

            break;
          case "NC":
            local.CssiProgramWorkset.NcIndicator = "Y";

            break;
          case "NF":
            local.CssiProgramWorkset.NfIndicator = "Y";

            break;
          case "SI":
            local.CssiProgramWorkset.SiIndicator = "Y";

            break;
          default:
            break;
        }
      }

      local.Ch.Assign(local.CssiProgramWorkset);
      local.CssiProgramWorkset.Assign(local.Default1);

      foreach(var item1 in ReadPersonProgramProgram2())
      {
        switch(TrimEnd(entities.Program.Code))
        {
          case "AF":
            local.CssiProgramWorkset.AfIndicator = "Y";

            break;
          case "AFI":
            local.CssiProgramWorkset.AfiIndicator = "Y";

            break;
          case "CC":
            local.CssiProgramWorkset.CcIndicator = "Y";

            break;
          case "CI":
            local.CssiProgramWorkset.CiIndicator = "Y";

            break;
          case "FC":
            local.CssiProgramWorkset.FcIndicator = "Y";

            break;
          case "FCI":
            local.CssiProgramWorkset.FciIndicator = "Y";

            break;
          case "FS":
            local.CssiProgramWorkset.FsIndicator = "Y";

            break;
          case "MA":
            local.CssiProgramWorkset.MaIndicator = "Y";

            break;
          case "MAI":
            local.CssiProgramWorkset.MaiIndicator = "Y";

            break;
          case "MK":
            local.CssiProgramWorkset.MkIndicator = "Y";

            break;
          case "MP":
            local.CssiProgramWorkset.MpIndicator = "Y";

            break;
          case "MS":
            local.CssiProgramWorkset.MsIndicator = "Y";

            break;
          case "NA":
            local.CssiProgramWorkset.NaIndicator = "Y";

            break;
          case "NAI":
            local.CssiProgramWorkset.NaiIndicator = "Y";

            break;
          case "NC":
            local.CssiProgramWorkset.NcIndicator = "Y";

            break;
          case "NF":
            local.CssiProgramWorkset.NfIndicator = "Y";

            break;
          case "SI":
            local.CssiProgramWorkset.SiIndicator = "Y";

            break;
          default:
            break;
        }
      }

      local.CpCssiProgramWorkset.Assign(local.CssiProgramWorkset);

      // --------------------------------------------------------------------------------------------------
      // NCP/CH/CP Info
      // 	41. Count any qualifying NCP/CH combination where there is an accruing
      // 	    child support debt detail for the current month. Y/N
      // --------------------------------------------------------------------------------------------------
      // -- Report local_current_support ief_supplied flag which is set in the 
      // above logic.
      // --------------------------------------------------------------------------------------------------
      // NCP/CH/CP Info
      // 	42. Count if the qualifying NCP/CH combination is arrears only, no
      // 	    current support owed.  Y/N
      // --------------------------------------------------------------------------------------------------
      if (AsChar(local.CurrentSupport.Flag) == 'Y')
      {
        local.ArrearsOnly.Flag = "N";
      }
      else
      {
        local.ArrearsOnly.Flag = "Y";
      }

      // --------------------------------------------------------------------------------------------------
      // NCP/CH/CP Info
      // 	43. Birthdate of each qualifying child.
      // --------------------------------------------------------------------------------------------------
      // -- Child birthday was read above to check for the child greater than 18
      // years of age.
      //    Use local_child cse_person_worket dob.
      local.ChDob.Text10 = NumberToString(Month(local.Child1.Dob), 14, 2) + "-"
        + NumberToString(Day(local.Child1.Dob), 14, 2) + "-" + NumberToString
        (Year(local.Child1.Dob), 12, 4);

      // --------------------------------------------------------------------------------------------------
      // Build the NCP/CH/CP File record.
      // --------------------------------------------------------------------------------------------------
      local.NcpChildCp.NcpChildFileLayout = entities.NcpCsePerson.Number + " " +
        local.NcpCssiProgramWorkset.AfIndicator + local
        .NcpCssiProgramWorkset.AfiIndicator + local
        .NcpCssiProgramWorkset.CcIndicator + local
        .NcpCssiProgramWorkset.CiIndicator + local
        .NcpCssiProgramWorkset.FcIndicator + local
        .NcpCssiProgramWorkset.FciIndicator + local
        .NcpCssiProgramWorkset.FsIndicator + local
        .NcpCssiProgramWorkset.MaIndicator + local
        .NcpCssiProgramWorkset.MaiIndicator + local
        .NcpCssiProgramWorkset.MkIndicator + local
        .NcpCssiProgramWorkset.MpIndicator + local
        .NcpCssiProgramWorkset.MsIndicator + local
        .NcpCssiProgramWorkset.NaIndicator + local
        .NcpCssiProgramWorkset.NaiIndicator + local
        .NcpCssiProgramWorkset.NcIndicator + local
        .NcpCssiProgramWorkset.NfIndicator + local
        .NcpCssiProgramWorkset.SiIndicator;
      local.NcpChildCp.NcpChildFileLayout =
        TrimEnd(local.NcpChildCp.NcpChildFileLayout) + " " + entities
        .Ch.Number + " " + local.Ch.AfIndicator + local.Ch.AfiIndicator + local
        .Ch.CcIndicator + local.Ch.CiIndicator + local.Ch.FcIndicator + local
        .Ch.FciIndicator + local.Ch.FsIndicator + local.Ch.MaIndicator + local
        .Ch.MaiIndicator + local.Ch.MkIndicator + local.Ch.MpIndicator + local
        .Ch.MsIndicator + local.Ch.NaIndicator + local.Ch.NaiIndicator + local
        .Ch.NcIndicator + local.Ch.NfIndicator + local.Ch.SiIndicator;
      local.NcpChildCp.NcpChildFileLayout =
        TrimEnd(local.NcpChildCp.NcpChildFileLayout) + " " + entities
        .CpCsePerson.Number + " " + local.CpCssiProgramWorkset.AfIndicator + local
        .CpCssiProgramWorkset.AfiIndicator + local
        .CpCssiProgramWorkset.CcIndicator + local
        .CpCssiProgramWorkset.CiIndicator + local
        .CpCssiProgramWorkset.FcIndicator + local
        .CpCssiProgramWorkset.FciIndicator + local
        .CpCssiProgramWorkset.FsIndicator + local
        .CpCssiProgramWorkset.MaIndicator + local
        .CpCssiProgramWorkset.MaiIndicator + local
        .CpCssiProgramWorkset.MkIndicator + local
        .CpCssiProgramWorkset.MpIndicator + local
        .CpCssiProgramWorkset.MsIndicator + local
        .CpCssiProgramWorkset.NaIndicator + local
        .CpCssiProgramWorkset.NaiIndicator + local
        .CpCssiProgramWorkset.NcIndicator + local
        .CpCssiProgramWorkset.NfIndicator + local
        .CpCssiProgramWorkset.SiIndicator;
      local.NcpChildCp.NcpChildFileLayout =
        TrimEnd(local.NcpChildCp.NcpChildFileLayout) + " " + local
        .CurrentSupport.Flag + " " + local.ArrearsOnly.Flag + " " + local
        .ChDob.Text10 + " ";
      local.EabFileHandling.Action = "WRITE";
      UseLeBfx9WriteNcpChildFile();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing NCP/CH/CP Info.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.ChildFound.Flag = "Y";
      ++import.ExportNcpChCpRecord.Count;
    }

    // --------------------------------------------------------------------------------------------------
    // Build the NCP File record.
    // --------------------------------------------------------------------------------------------------
    local.NcpFamilyArrearsBalance.Text10 =
      NumberToString((long)local.TotalNcpFamilyArrears.TotalCurrency, 9, 7) + "."
      + NumberToString
      ((long)(local.TotalNcpFamilyArrears.TotalCurrency * 100), 14, 2);
    local.NcpCssiWorkset.NcpFileLayout = entities.NcpCsePerson.Number + " " + local
      .NcpDob.Text10 + " " + local.NcpCsePersonsWorkSet.Sex + " " + entities
      .NcpCsePerson.Race + " " + local.NcpCurrentlyEmployed.Flag + " " + local
      .NcpIncomeLast4QtrsTextWorkArea.Text10 + " " + local
      .NumberChildSupportOrderWorkArea.Text3 + " " + local
      .NcpFamilyArrearsBalance.Text10 + " " + local
      .NcpTotalArrearsBalanceTextWorkArea.Text10 + " " + local
      .LastIwoPaymentDate.Text10 + " " + local.LastUiPaymentDate.Text10 + " " +
      local.MostRecentPaymentDate.Text10 + " " + local.NewOrder.Flag + " " + local
      .NumberOfMinorChildren.Text2 + " " + local
      .IncarcerationStartDate.Text10 + " " + local
      .IncarcerationEndDate.Text10 + " ";
    local.EabFileHandling.Action = "WRITE";
    UseLeBfx9WriteNcpFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing NCP Info.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ++import.ExportNcpRecord.Count;

    // --------------------------------------------------------------------------------------------------
    // Log to the error report if no eligible child was found for the NCP.
    // This could happen if time elapses between creating the mailing list and 
    // running this report.
    // The child was eligible when the the mailing list was created but is no 
    // longer eligible.
    // --------------------------------------------------------------------------------------------------
    if (AsChar(local.ChildFound.Flag) == 'N')
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "No eligible children found for NCP " + entities
        .NcpCsePerson.Number;
      UseCabErrorReport();
    }
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveIncarceration(Incarceration source,
    Incarceration target)
  {
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.SupportedPerson.Number = entities.Ch.Number;
    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.DebtDetail.Assign(entities.DebtDetail);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    useImport.HardcodedAccruing.Classification =
      local.HardcodedAccruing.Classification;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
    local.Program.Code = useExport.Program.Code;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
  }

  private void UseLeBfx9WriteNcpChildFile()
  {
    var useImport = new LeBfx9WriteNcpChildFile.Import();
    var useExport = new LeBfx9WriteNcpChildFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CssiWorkset.NcpChildFileLayout =
      local.NcpChildCp.NcpChildFileLayout;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx9WriteNcpChildFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeBfx9WriteNcpFile()
  {
    var useImport = new LeBfx9WriteNcpFile.Import();
    var useExport = new LeBfx9WriteNcpFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CssiWorkset.NcpFileLayout = local.NcpCssiWorkset.NcpFileLayout;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeBfx9WriteNcpFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Child1.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Child1.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.NcpCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.NcpCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseChild()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;

    return Read("ReadCaseChild",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Ch.Number);
        db.SetString(command, "cspNumber2", entities.NcpCsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.StartDate = db.GetNullableDate(reader, 4);
        entities.Child.EndDate = db.GetNullableDate(reader, 5);
        entities.Child.FcParentalRights = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseChildAbsentParent()
  {
    entities.Case1.Populated = false;
    entities.Child.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadCaseChildAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Ch.Number);
        db.SetString(command, "cspNumber2", entities.NcpCsePerson.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.StartDate = db.GetNullableDate(reader, 4);
        entities.Child.EndDate = db.GetNullableDate(reader, 5);
        entities.Child.FcParentalRights = db.GetNullableString(reader, 6);
        entities.AbsentParent.CasNumber = db.GetString(reader, 7);
        entities.AbsentParent.CspNumber = db.GetString(reader, 8);
        entities.AbsentParent.Type1 = db.GetString(reader, 9);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 10);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 11);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 12);
        entities.Case1.Populated = true;
        entities.Child.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadCashReceiptDetail1()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail2()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail3()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadChild()
  {
    entities.Child.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.Ch.Number);
        db.SetString(command, "type", local.Child2.Type1);
        db.SetInt32(command, "caseRoleId", local.Child2.Identifier);
      },
      (db, reader) =>
      {
        entities.Child.CasNumber = db.GetString(reader, 0);
        entities.Child.CspNumber = db.GetString(reader, 1);
        entities.Child.Type1 = db.GetString(reader, 2);
        entities.Child.Identifier = db.GetInt32(reader, 3);
        entities.Child.StartDate = db.GetNullableDate(reader, 4);
        entities.Child.EndDate = db.GetNullableDate(reader, 5);
        entities.Child.FcParentalRights = db.GetNullableString(reader, 6);
        entities.Child.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Ch.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CpCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "type", local.Ar.Type1);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CpCsePerson.Number = db.GetString(reader, 0);
        entities.CpCsePerson.Type1 = db.GetString(reader, 1);
        entities.CpCsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CpCsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CpCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CpCsePerson.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    entities.NcpCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ncp.Number);
      },
      (db, reader) =>
      {
        entities.NcpCsePerson.Number = db.GetString(reader, 0);
        entities.NcpCsePerson.Type1 = db.GetString(reader, 1);
        entities.NcpCsePerson.Race = db.GetNullableString(reader, 2);
        entities.NcpCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.NcpCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson4()
  {
    entities.DistinctChild.Populated = false;

    return ReadEach("ReadCsePerson4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "dateOfDeath", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DistinctChild.Number = db.GetString(reader, 0);
        entities.DistinctChild.Type1 = db.GetString(reader, 1);
        entities.DistinctChild.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.DistinctChild.Populated = true;
        CheckValid<CsePerson>("Type1", entities.DistinctChild.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonObligorSupported()
  {
    entities.Ch.Populated = false;
    entities.Obligor.Populated = false;
    entities.Supported.Populated = false;

    return ReadEach("ReadCsePersonObligorSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Ch.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 1);
        entities.Obligor.Type1 = db.GetString(reader, 2);
        entities.Supported.CspNumber = db.GetString(reader, 3);
        entities.Supported.Type1 = db.GetString(reader, 4);
        entities.Ch.Populated = true;
        entities.Obligor.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);

        return true;
      });
  }

  private bool ReadDebt()
  {
    entities.Oldest.Populated = false;

    return Read("ReadDebt",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Oldest.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Oldest.CspNumber = db.GetString(reader, 1);
        entities.Oldest.CpaType = db.GetString(reader, 2);
        entities.Oldest.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Oldest.Type1 = db.GetString(reader, 4);
        entities.Oldest.CreatedTmst = db.GetDateTime(reader, 5);
        entities.Oldest.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Oldest.CpaSupType = db.GetNullableString(reader, 7);
        entities.Oldest.OtyType = db.GetInt32(reader, 8);
        entities.Oldest.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Oldest.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Oldest.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.Oldest.CpaSupType);
      });
  }

  private bool ReadDebtDetail()
  {
    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
        db.
          SetDate(command, "dueDt", local.FirstOfMonth.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        local.NcpTotalArrearsBalanceCommon.TotalCurrency =
          db.GetDecimal(reader, 0);
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadDebtDetailObligationObligationType",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 11);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 13);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 15);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 16);
        entities.Obligation.LastObligationEvent =
          db.GetNullableString(reader, 17);
        entities.ObligationType.Code = db.GetString(reader, 18);
        entities.ObligationType.Classification = db.GetString(reader, 19);
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.IncomeSource.LastQtr = db.GetNullableString(reader, 3);
        entities.IncomeSource.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.IncomeSource.Attribute2NdQtr = db.GetNullableString(reader, 6);
        entities.IncomeSource.Attribute2NdQtrYr =
          db.GetNullableInt32(reader, 7);
        entities.IncomeSource.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 8);
        entities.IncomeSource.Attribute3RdQtr = db.GetNullableString(reader, 9);
        entities.IncomeSource.Attribute3RdQtrYr =
          db.GetNullableInt32(reader, 10);
        entities.IncomeSource.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 11);
        entities.IncomeSource.Attribute4ThQtr =
          db.GetNullableString(reader, 12);
        entities.IncomeSource.Attribute4ThQtrYr =
          db.GetNullableInt32(reader, 13);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 14);
        entities.IncomeSource.CspINumber = db.GetString(reader, 15);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 16);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
        db.
          SetDate(command, "dueDt", local.FirstOfMonth.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        local.NumberChildSupportOrderCommon.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram1()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Ch.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram2()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram3()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Ncp.
    /// </summary>
    [JsonPropertyName("ncp")]
    public CsePerson Ncp
    {
      get => ncp ??= new();
      set => ncp = value;
    }

    /// <summary>
    /// A value of ExportNcpRecord.
    /// </summary>
    [JsonPropertyName("exportNcpRecord")]
    public Common ExportNcpRecord
    {
      get => exportNcpRecord ??= new();
      set => exportNcpRecord = value;
    }

    /// <summary>
    /// A value of ExportNcpChCpRecord.
    /// </summary>
    [JsonPropertyName("exportNcpChCpRecord")]
    public Common ExportNcpChCpRecord
    {
      get => exportNcpChCpRecord ??= new();
      set => exportNcpChCpRecord = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson ncp;
    private Common exportNcpRecord;
    private Common exportNcpChCpRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Fips G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GlocalStateOwed.
      /// </summary>
      [JsonPropertyName("glocalStateOwed")]
      public Common GlocalStateOwed
      {
        get => glocalStateOwed ??= new();
        set => glocalStateOwed = value;
      }

      /// <summary>
      /// A value of GlocalFamilyOwed.
      /// </summary>
      [JsonPropertyName("glocalFamilyOwed")]
      public Common GlocalFamilyOwed
      {
        get => glocalFamilyOwed ??= new();
        set => glocalFamilyOwed = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Fips g;
      private Common glocalStateOwed;
      private Common glocalFamilyOwed;
    }

    /// <summary>
    /// A value of ChFamilyArrears.
    /// </summary>
    [JsonPropertyName("chFamilyArrears")]
    public Common ChFamilyArrears
    {
      get => chFamilyArrears ??= new();
      set => chFamilyArrears = value;
    }

    /// <summary>
    /// A value of ChildFound.
    /// </summary>
    [JsonPropertyName("childFound")]
    public Common ChildFound
    {
      get => childFound ??= new();
      set => childFound = value;
    }

    /// <summary>
    /// A value of NcpCssiWorkset.
    /// </summary>
    [JsonPropertyName("ncpCssiWorkset")]
    public CssiWorkset NcpCssiWorkset
    {
      get => ncpCssiWorkset ??= new();
      set => ncpCssiWorkset = value;
    }

    /// <summary>
    /// A value of NewOrder.
    /// </summary>
    [JsonPropertyName("newOrder")]
    public Common NewOrder
    {
      get => newOrder ??= new();
      set => newOrder = value;
    }

    /// <summary>
    /// A value of NumberChildSupportOrderWorkArea.
    /// </summary>
    [JsonPropertyName("numberChildSupportOrderWorkArea")]
    public WorkArea NumberChildSupportOrderWorkArea
    {
      get => numberChildSupportOrderWorkArea ??= new();
      set => numberChildSupportOrderWorkArea = value;
    }

    /// <summary>
    /// A value of NumberOfMinorChildren.
    /// </summary>
    [JsonPropertyName("numberOfMinorChildren")]
    public TextWorkArea NumberOfMinorChildren
    {
      get => numberOfMinorChildren ??= new();
      set => numberOfMinorChildren = value;
    }

    /// <summary>
    /// A value of NcpIncomeLast4QtrsCommon.
    /// </summary>
    [JsonPropertyName("ncpIncomeLast4QtrsCommon")]
    public Common NcpIncomeLast4QtrsCommon
    {
      get => ncpIncomeLast4QtrsCommon ??= new();
      set => ncpIncomeLast4QtrsCommon = value;
    }

    /// <summary>
    /// A value of NcpTotalArrearsBalanceTextWorkArea.
    /// </summary>
    [JsonPropertyName("ncpTotalArrearsBalanceTextWorkArea")]
    public TextWorkArea NcpTotalArrearsBalanceTextWorkArea
    {
      get => ncpTotalArrearsBalanceTextWorkArea ??= new();
      set => ncpTotalArrearsBalanceTextWorkArea = value;
    }

    /// <summary>
    /// A value of NcpFamilyArrearsBalance.
    /// </summary>
    [JsonPropertyName("ncpFamilyArrearsBalance")]
    public TextWorkArea NcpFamilyArrearsBalance
    {
      get => ncpFamilyArrearsBalance ??= new();
      set => ncpFamilyArrearsBalance = value;
    }

    /// <summary>
    /// A value of NcpIncomeLast4QtrsTextWorkArea.
    /// </summary>
    [JsonPropertyName("ncpIncomeLast4QtrsTextWorkArea")]
    public TextWorkArea NcpIncomeLast4QtrsTextWorkArea
    {
      get => ncpIncomeLast4QtrsTextWorkArea ??= new();
      set => ncpIncomeLast4QtrsTextWorkArea = value;
    }

    /// <summary>
    /// A value of IncarcerationEndDate.
    /// </summary>
    [JsonPropertyName("incarcerationEndDate")]
    public TextWorkArea IncarcerationEndDate
    {
      get => incarcerationEndDate ??= new();
      set => incarcerationEndDate = value;
    }

    /// <summary>
    /// A value of IncarcerationStartDate.
    /// </summary>
    [JsonPropertyName("incarcerationStartDate")]
    public TextWorkArea IncarcerationStartDate
    {
      get => incarcerationStartDate ??= new();
      set => incarcerationStartDate = value;
    }

    /// <summary>
    /// A value of MostRecentPaymentDate.
    /// </summary>
    [JsonPropertyName("mostRecentPaymentDate")]
    public TextWorkArea MostRecentPaymentDate
    {
      get => mostRecentPaymentDate ??= new();
      set => mostRecentPaymentDate = value;
    }

    /// <summary>
    /// A value of LastUiPaymentDate.
    /// </summary>
    [JsonPropertyName("lastUiPaymentDate")]
    public TextWorkArea LastUiPaymentDate
    {
      get => lastUiPaymentDate ??= new();
      set => lastUiPaymentDate = value;
    }

    /// <summary>
    /// A value of LastIwoPaymentDate.
    /// </summary>
    [JsonPropertyName("lastIwoPaymentDate")]
    public TextWorkArea LastIwoPaymentDate
    {
      get => lastIwoPaymentDate ??= new();
      set => lastIwoPaymentDate = value;
    }

    /// <summary>
    /// A value of NcpDob.
    /// </summary>
    [JsonPropertyName("ncpDob")]
    public TextWorkArea NcpDob
    {
      get => ncpDob ??= new();
      set => ncpDob = value;
    }

    /// <summary>
    /// A value of ChDob.
    /// </summary>
    [JsonPropertyName("chDob")]
    public TextWorkArea ChDob
    {
      get => chDob ??= new();
      set => chDob = value;
    }

    /// <summary>
    /// A value of ArrearsOnly.
    /// </summary>
    [JsonPropertyName("arrearsOnly")]
    public Common ArrearsOnly
    {
      get => arrearsOnly ??= new();
      set => arrearsOnly = value;
    }

    /// <summary>
    /// A value of NcpChildCp.
    /// </summary>
    [JsonPropertyName("ncpChildCp")]
    public CssiWorkset NcpChildCp
    {
      get => ncpChildCp ??= new();
      set => ncpChildCp = value;
    }

    /// <summary>
    /// A value of TotalNcpFamilyArrears.
    /// </summary>
    [JsonPropertyName("totalNcpFamilyArrears")]
    public Common TotalNcpFamilyArrears
    {
      get => totalNcpFamilyArrears ??= new();
      set => totalNcpFamilyArrears = value;
    }

    /// <summary>
    /// A value of CurrentSupport.
    /// </summary>
    [JsonPropertyName("currentSupport")]
    public Common CurrentSupport
    {
      get => currentSupport ??= new();
      set => currentSupport = value;
    }

    /// <summary>
    /// A value of CssiProgramWorkset.
    /// </summary>
    [JsonPropertyName("cssiProgramWorkset")]
    public CssiProgramWorkset CssiProgramWorkset
    {
      get => cssiProgramWorkset ??= new();
      set => cssiProgramWorkset = value;
    }

    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public CssiProgramWorkset Default1
    {
      get => default1 ??= new();
      set => default1 = value;
    }

    /// <summary>
    /// A value of CpCssiProgramWorkset.
    /// </summary>
    [JsonPropertyName("cpCssiProgramWorkset")]
    public CssiProgramWorkset CpCssiProgramWorkset
    {
      get => cpCssiProgramWorkset ??= new();
      set => cpCssiProgramWorkset = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CssiProgramWorkset Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of NcpCssiProgramWorkset.
    /// </summary>
    [JsonPropertyName("ncpCssiProgramWorkset")]
    public CssiProgramWorkset NcpCssiProgramWorkset
    {
      get => ncpCssiProgramWorkset ??= new();
      set => ncpCssiProgramWorkset = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
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
    /// A value of AllTypes.
    /// </summary>
    [JsonPropertyName("allTypes")]
    public CashReceiptDetail AllTypes
    {
      get => allTypes ??= new();
      set => allTypes = value;
    }

    /// <summary>
    /// A value of Utype.
    /// </summary>
    [JsonPropertyName("utype")]
    public CashReceiptDetail Utype
    {
      get => utype ??= new();
      set => utype = value;
    }

    /// <summary>
    /// A value of Itype.
    /// </summary>
    [JsonPropertyName("itype")]
    public CashReceiptDetail Itype
    {
      get => itype ??= new();
      set => itype = value;
    }

    /// <summary>
    /// A value of NcpTotalArrearsBalanceCommon.
    /// </summary>
    [JsonPropertyName("ncpTotalArrearsBalanceCommon")]
    public Common NcpTotalArrearsBalanceCommon
    {
      get => ncpTotalArrearsBalanceCommon ??= new();
      set => ncpTotalArrearsBalanceCommon = value;
    }

    /// <summary>
    /// A value of NumberChildSupportOrderCommon.
    /// </summary>
    [JsonPropertyName("numberChildSupportOrderCommon")]
    public Common NumberChildSupportOrderCommon
    {
      get => numberChildSupportOrderCommon ??= new();
      set => numberChildSupportOrderCommon = value;
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
    /// A value of NcpCurrentlyEmployed.
    /// </summary>
    [JsonPropertyName("ncpCurrentlyEmployed")]
    public Common NcpCurrentlyEmployed
    {
      get => ncpCurrentlyEmployed ??= new();
      set => ncpCurrentlyEmployed = value;
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
    /// A value of HardcodedAccruing.
    /// </summary>
    [JsonPropertyName("hardcodedAccruing")]
    public ObligationType HardcodedAccruing
    {
      get => hardcodedAccruing ??= new();
      set => hardcodedAccruing = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of TbdLocalFamilyNcpDocs.
    /// </summary>
    [JsonPropertyName("tbdLocalFamilyNcpDocs")]
    public Common TbdLocalFamilyNcpDocs
    {
      get => tbdLocalFamilyNcpDocs ??= new();
      set => tbdLocalFamilyNcpDocs = value;
    }

    /// <summary>
    /// A value of TbdLocalNumberOfCpDocs.
    /// </summary>
    [JsonPropertyName("tbdLocalNumberOfCpDocs")]
    public Common TbdLocalNumberOfCpDocs
    {
      get => tbdLocalNumberOfCpDocs ??= new();
      set => tbdLocalNumberOfCpDocs = value;
    }

    /// <summary>
    /// A value of FirstOfMonth.
    /// </summary>
    [JsonPropertyName("firstOfMonth")]
    public DateWorkArea FirstOfMonth
    {
      get => firstOfMonth ??= new();
      set => firstOfMonth = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of NcpCsePersonAddress.
    /// </summary>
    [JsonPropertyName("ncpCsePersonAddress")]
    public CsePersonAddress NcpCsePersonAddress
    {
      get => ncpCsePersonAddress ??= new();
      set => ncpCsePersonAddress = value;
    }

    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CsePersonsWorkSet Child1
    {
      get => child1 ??= new();
      set => child1 = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of Overlap.
    /// </summary>
    [JsonPropertyName("overlap")]
    public DateWorkArea Overlap
    {
      get => overlap ??= new();
      set => overlap = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CaseRole Child2
    {
      get => child2 ??= new();
      set => child2 = value;
    }

    /// <summary>
    /// A value of CpCsePersonAddress.
    /// </summary>
    [JsonPropertyName("cpCsePersonAddress")]
    public CsePersonAddress CpCsePersonAddress
    {
      get => cpCsePersonAddress ??= new();
      set => cpCsePersonAddress = value;
    }

    /// <summary>
    /// A value of Deceased.
    /// </summary>
    [JsonPropertyName("deceased")]
    public Common Deceased
    {
      get => deceased ??= new();
      set => deceased = value;
    }

    /// <summary>
    /// A value of NcpCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("ncpCsePersonsWorkSet")]
    public CsePersonsWorkSet NcpCsePersonsWorkSet
    {
      get => ncpCsePersonsWorkSet ??= new();
      set => ncpCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CpCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("cpCsePersonsWorkSet")]
    public CsePersonsWorkSet CpCsePersonsWorkSet
    {
      get => cpCsePersonsWorkSet ??= new();
      set => cpCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of StateOwedArrears.
    /// </summary>
    [JsonPropertyName("stateOwedArrears")]
    public Common StateOwedArrears
    {
      get => stateOwedArrears ??= new();
      set => stateOwedArrears = value;
    }

    /// <summary>
    /// A value of StateOwedAmount.
    /// </summary>
    [JsonPropertyName("stateOwedAmount")]
    public TextWorkArea StateOwedAmount
    {
      get => stateOwedAmount ??= new();
      set => stateOwedAmount = value;
    }

    /// <summary>
    /// A value of FamilyOwedAmount.
    /// </summary>
    [JsonPropertyName("familyOwedAmount")]
    public TextWorkArea FamilyOwedAmount
    {
      get => familyOwedAmount ??= new();
      set => familyOwedAmount = value;
    }

    /// <summary>
    /// A value of TbdLocalStateNcpDocs.
    /// </summary>
    [JsonPropertyName("tbdLocalStateNcpDocs")]
    public Common TbdLocalStateNcpDocs
    {
      get => tbdLocalStateNcpDocs ??= new();
      set => tbdLocalStateNcpDocs = value;
    }

    /// <summary>
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public TextWorkArea FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
    }

    /// <summary>
    /// A value of NcpFlyer.
    /// </summary>
    [JsonPropertyName("ncpFlyer")]
    public EabReportSend NcpFlyer
    {
      get => ncpFlyer ??= new();
      set => ncpFlyer = value;
    }

    private Common chFamilyArrears;
    private Common childFound;
    private CssiWorkset ncpCssiWorkset;
    private Common newOrder;
    private WorkArea numberChildSupportOrderWorkArea;
    private TextWorkArea numberOfMinorChildren;
    private Common ncpIncomeLast4QtrsCommon;
    private TextWorkArea ncpTotalArrearsBalanceTextWorkArea;
    private TextWorkArea ncpFamilyArrearsBalance;
    private TextWorkArea ncpIncomeLast4QtrsTextWorkArea;
    private TextWorkArea incarcerationEndDate;
    private TextWorkArea incarcerationStartDate;
    private TextWorkArea mostRecentPaymentDate;
    private TextWorkArea lastUiPaymentDate;
    private TextWorkArea lastIwoPaymentDate;
    private TextWorkArea ncpDob;
    private TextWorkArea chDob;
    private Common arrearsOnly;
    private CssiWorkset ncpChildCp;
    private Common totalNcpFamilyArrears;
    private Common currentSupport;
    private CssiProgramWorkset cssiProgramWorkset;
    private CssiProgramWorkset default1;
    private CssiProgramWorkset cpCssiProgramWorkset;
    private CssiProgramWorkset ch;
    private CssiProgramWorkset ncpCssiProgramWorkset;
    private Incarceration incarceration;
    private Common numberOfChildren;
    private CashReceiptDetail allTypes;
    private CashReceiptDetail utype;
    private CashReceiptDetail itype;
    private Common ncpTotalArrearsBalanceCommon;
    private Common numberChildSupportOrderCommon;
    private IncomeSource incomeSource;
    private Common ncpCurrentlyEmployed;
    private DateWorkArea current;
    private ObligationType hardcodedAccruing;
    private CaseRole ap;
    private CaseRole ar;
    private Common tbdLocalFamilyNcpDocs;
    private Common tbdLocalNumberOfCpDocs;
    private DateWorkArea firstOfMonth;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea null1;
    private CsePersonAddress ncpCsePersonAddress;
    private CsePersonsWorkSet child1;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Array<LocalGroup> local1;
    private DprProgram dprProgram;
    private Program program;
    private Case1 case1;
    private DateWorkArea overlap;
    private DateWorkArea dateWorkArea;
    private CaseRole child2;
    private CsePersonAddress cpCsePersonAddress;
    private Common deceased;
    private CsePersonsWorkSet ncpCsePersonsWorkSet;
    private CsePersonsWorkSet cpCsePersonsWorkSet;
    private Common stateOwedArrears;
    private TextWorkArea stateOwedAmount;
    private TextWorkArea familyOwedAmount;
    private Common tbdLocalStateNcpDocs;
    private TextWorkArea fileNumber;
    private EabReportSend ncpFlyer;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Oldest.
    /// </summary>
    [JsonPropertyName("oldest")]
    public ObligationTransaction Oldest
    {
      get => oldest ??= new();
      set => oldest = value;
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
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of DistinctChild.
    /// </summary>
    [JsonPropertyName("distinctChild")]
    public CsePerson DistinctChild
    {
      get => distinctChild ??= new();
      set => distinctChild = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of DistinctStandardNo.
    /// </summary>
    [JsonPropertyName("distinctStandardNo")]
    public LegalAction DistinctStandardNo
    {
      get => distinctStandardNo ??= new();
      set => distinctStandardNo = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of NcpCsePerson.
    /// </summary>
    [JsonPropertyName("ncpCsePerson")]
    public CsePerson NcpCsePerson
    {
      get => ncpCsePerson ??= new();
      set => ncpCsePerson = value;
    }

    /// <summary>
    /// A value of NcpCsePersonAddress.
    /// </summary>
    [JsonPropertyName("ncpCsePersonAddress")]
    public CsePersonAddress NcpCsePersonAddress
    {
      get => ncpCsePersonAddress ??= new();
      set => ncpCsePersonAddress = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of CpCsePerson.
    /// </summary>
    [JsonPropertyName("cpCsePerson")]
    public CsePerson CpCsePerson
    {
      get => cpCsePerson ??= new();
      set => cpCsePerson = value;
    }

    /// <summary>
    /// A value of CpCsePersonAddress.
    /// </summary>
    [JsonPropertyName("cpCsePersonAddress")]
    public CsePersonAddress CpCsePersonAddress
    {
      get => cpCsePersonAddress ??= new();
      set => cpCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    private CaseRole caseRole;
    private ObligationTransaction oldest;
    private Program program;
    private PersonProgram personProgram;
    private Incarceration incarceration;
    private CsePerson distinctChild;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private AccrualInstructions accrualInstructions;
    private LegalAction distinctStandardNo;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private CsePerson ncpCsePerson;
    private CsePersonAddress ncpCsePersonAddress;
    private CsePerson ch;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private Fips fips;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount obligor;
    private CsePersonAccount supported;
    private Case1 case1;
    private CaseRole child;
    private CaseRole absentParent;
    private CsePerson cpCsePerson;
    private CsePersonAddress cpCsePersonAddress;
    private CaseRole applicantRecipient;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
  }
#endregion
}
