// Program: FN_B795_PROCESS_CASE, ID: 1902456060, model: 746.
// Short name: SWE03735
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B795_PROCESS_CASE.
/// </summary>
[Serializable]
public partial class FnB795ProcessCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B795_PROCESS_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB795ProcessCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB795ProcessCase.
  /// </summary>
  public FnB795ProcessCase(IContext context, Import import, Export export):
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
    // Assumptions:
    // 	1) This report will run once a month, and will report information for 
    // the prior month.
    // 	2) All information will be reported at a case level.  The starting case 
    // universe will
    // 	   be taken from the Dashboard Pyramid table (DB_STAGE_PRI_4).  This 
    // will include all
    // 	   cases open on the last day of the report month. This will not include
    // no jurisdiction
    // 	   cases.
    // 	3) Only NCPs active on the run date will be listed on the report.
    // 	4) All date fields will be formatted as: MM-DD-YYYY
    // 	5) For all debt/payment information, the tie from court order to case 
    // will be made
    // 	   through LROL:
    // 		a. NCP is active on the case
    // 		b. NCP is listed on LROL with an AP Role with the Case # for J and O 
    // court
    // 		   order legal action
    // 	6) Data will provide in three formats:
    // 		a. Two possible record types for each case
    // 			i. Type A- These records contain case specific information.  Every
    // 			   reported case will have one or more Type A record (one record
    // 			   will be created for each active NCP on the case).  All case
    // 			   information will be repeated for each Type A record for a single 
    // case.
    // 			ii. Type B- These records contain court order specific information.
    // 			   Reported cases will have zero to many Type B records (depending
    // 			   on the number of associated court orders).
    // 		b. There will be one record type that has the case number, one court 
    // order and
    // 		   NCP combination.  This will repeat if there is more than one court 
    // order or
    // 		   NCP for a case.
    // 		c. Same as Number 6a. with two separate files.  All Type A records go 
    // in one
    // 		   file and all Type B records go in another file with the case number 
    // as the tie.
    // --------------------------------------------------------------------------------------------------
    local.NullTextDate.Text10 = "01-01-0001";
    local.FileDefault.Date = new DateTime(1, 1, 1);
    MoveContractorCaseUniverse4(import.NullCaseNcpCp, local.NullNcpData);
    local.NullCourtOrder.Assign(import.NullCourtOrder);
    MoveContractorCaseUniverse2(import.NullCaseNcpCp,
      local.ContractorCaseUniverse);
    local.Ar.Type1 = "AR";
    local.Ch.Type1 = "CH";
    local.Ap.Type1 = "AP";

    // --------------------------------------------------------------------------------------------------
    // --          C A S E   S P E C I F I C    D A T A    E L E M E N T S   (
    // TYPE A RECORD)           --
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Contractor
    // The contractor name that the case is assigned to on the last day of the 
    // report period.
    // The cases are assigned to a judicial district based on Office of 
    // Assignment.  The contractor
    // name is assigned to a judicial district.
    // Use Contractor_Num reported in the DB_STAGE_PRI_4 (Pyramid) table.  
    // Report actual contractor name.
    // --------------------------------------------------------------------------------------------------
    if (ReadCseOrganization())
    {
      local.ContractorCaseUniverse.ContractorName = entities.Contractor.Name;
    }
    else
    {
      // -- This should never happen.
      local.ContractorCaseUniverse.ContractorName = "";
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Judicial District
    // The judicial district that the case is assigned to on the last day of the
    // report period.
    // The cases are assigned to a judicial district based on Office of 
    // Assignment.
    // Use Judicial_District reported in the DB_STAGE_PRI_4 (Pyramid) table.
    // --------------------------------------------------------------------------------------------------
    local.ContractorCaseUniverse.JudicialDistrict =
      import.DashboardStagingPriority4.JudicialDistrict;

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Case Number
    // This will include all cases reported in the pyramid report  (cases open 
    // on the last day of the report period).
    // Use Case_Number reported in the DB_STAGE_PRI_4 (Pyramid) table.
    // --------------------------------------------------------------------------------------------------
    local.ContractorCaseUniverse.CaseNumber =
      import.DashboardStagingPriority4.CaseNumber;

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Case Open Date
    // This will match the CSE Open Date reported on the CADS screen as of the 
    // run date of the report.
    // DATA ELEMENT: Pending Case Closure Date (as of the report date)
    // 1) From the CADS screen.  Report CSE Closure Ltr Date + 60 days.
    // 2) Report 01/01/0001 if no value.
    // --------------------------------------------------------------------------------------------------
    if (ReadCase())
    {
      local.DateWorkArea.Date = entities.Case1.CseOpenDate;
      local.ContractorCaseUniverse.CaseOpenDate = UseCabFormatDate3();

      if (Equal(entities.Case1.ClosureLetterDate, local.Null1.Date))
      {
        local.ContractorCaseUniverse.PendingCaseClosureDate =
          local.NullTextDate.Text10;
      }
      else
      {
        local.DateWorkArea.Date = AddDays(entities.Case1.ClosureLetterDate, 60);
        local.ContractorCaseUniverse.PendingCaseClosureDate =
          UseCabFormatDate3();
      }
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Assigned Worker
    // The worker the case is assigned to on the last day of the report period.
    // Use Worker ID reported in DB_STAGE_PRI_4 (Pyramid) table.  Report worker
    // s first and last name.
    // --------------------------------------------------------------------------------------------------
    if (ReadServiceProvider())
    {
      local.ContractorCaseUniverse.AssignedCaseworkerFirstName =
        entities.ServiceProvider.FirstName;
      local.ContractorCaseUniverse.AssignedCaseworkerLastName =
        entities.ServiceProvider.LastName;
    }
    else
    {
      local.ContractorCaseUniverse.AssignedCaseworkerFirstName = "";
      local.ContractorCaseUniverse.AssignedCaseworkerLastName = "";
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Pyramid Category
    // 1) Report category derived from DB_STAGE_PRI_4 (Pyramid) table.  This 
    // will display the one
    //    category that the cases is in on last day of the report period.
    // 2) Categories to report:
    // 	a. Paying = Paying_Case_Ind = Y (These are cases with current support 
    // owed where
    // 	   at least 75% of the current child support due for the last 3 months 
    // has been
    // 	   collected/distributed as current child support)
    // 	b. Non-Paying = Paying_Case_Ind = N  (These are cases with current 
    // support owed
    // 	   where less than 75% of the current child support due for the last 3 
    // months has been
    // 	   collected/distributed as current child support)
    // 	c. Other Obligation = Other_Obg_Ind = Y (These are cases that meet the
    // federal
    // 	   definition of a case under order that dont have current support 
    // owed)
    // 	d. No Obligation/Paternity = Paternity_Est_Ind = N (These are cases 
    // with no obligation
    // 	   and at least one active child doesnt have paternity established)
    // 	e. No Obligation/Non-Paternity = Paternity_Est_Ind = Y (These are 
    // cases with no
    // 	   obligation and all active children have paternity established)
    // --------------------------------------------------------------------------------------------------
    if (AsChar(import.DashboardStagingPriority4.PayingCaseInd) == 'Y')
    {
      local.ContractorCaseUniverse.PyramidCategory = "PAYING";
    }
    else if (AsChar(import.DashboardStagingPriority4.PayingCaseInd) == 'N')
    {
      local.ContractorCaseUniverse.PyramidCategory = "NON-PAYING";
    }
    else if (AsChar(import.DashboardStagingPriority4.OtherObgInd) == 'Y')
    {
      local.ContractorCaseUniverse.PyramidCategory = "OTHER OBLIGATION";
    }
    else if (AsChar(import.DashboardStagingPriority4.PaternityEstInd) == 'N')
    {
      local.ContractorCaseUniverse.PyramidCategory = "NO OBLIGATION/PATERNITY";
    }
    else if (AsChar(import.DashboardStagingPriority4.PaternityEstInd) == 'Y')
    {
      local.ContractorCaseUniverse.PyramidCategory =
        "NO OBLIGATION/NON-PATERNITY";
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Address Active Y/N (NOTE: This is at the case level)
    // Cases where at least one active (non-end dated) residential or mailing 
    // address exists for any active NCP on the case.
    // 	1) Use Address_Ver_Ind from DB_STAGE_PRI_4 (Pyramid) table
    // 	2) Report Y where Address_Ver_Ind = Y or N
    // 	3) Report N where Address_Ver_Ind =blank
    // --------------------------------------------------------------------------------------------------
    if (AsChar(import.DashboardStagingPriority4.AddressVerInd) == 'Y' || AsChar
      (import.DashboardStagingPriority4.AddressVerInd) == 'N')
    {
      local.ContractorCaseUniverse.AddressActive = "Y";
    }
    else if (IsEmpty(import.DashboardStagingPriority4.AddressVerInd))
    {
      local.ContractorCaseUniverse.AddressActive = "N";
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Employer Active Y/N (NOTE: This is at the case level)
    // Cases where at least one active (non-end dated) qualifying Employer 
    // record exists for any active NCP on the case.
    // 	1) Use Employer_Ver_Ind from DB_STAGE_PRI_4 (Pyramid) table
    // 	2) Report Y where Employer_Ver_Ind = Y or N
    // 	3) Report N where Employer_Ver_Ind =blank
    // --------------------------------------------------------------------------------------------------
    if (AsChar(import.DashboardStagingPriority4.EmployerVerInd) == 'Y' || AsChar
      (import.DashboardStagingPriority4.EmployerVerInd) == 'N')
    {
      local.ContractorCaseUniverse.EmployerActive = "Y";
    }
    else if (IsEmpty(import.DashboardStagingPriority4.EmployerVerInd))
    {
      local.ContractorCaseUniverse.EmployerActive = "N";
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Current Support Due Last 3 Months (NOTE: This is at the 
    // case level)
    // This is dollar amount of current child support (CS obligation) due for 
    // the last 3 months (report period month plus 2 months prior).
    // 	1) Use CS_Due_Amt reported in the DB_STAGE_PRI_4 (Pyramid) table
    // 	2) Report 0.00 if no value
    // --------------------------------------------------------------------------------------------------
    local.Common.TotalCurrency =
      import.DashboardStagingPriority4.CsDueAmt.GetValueOrDefault();
    local.ContractorCaseUniverse.CurrentSupportDue =
      UseFnB795ConvertNumToText();

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Current Support Paid Last 3 Months (NOTE: This is at the 
    // case level)
    // This is dollar amount distributed as current to the child support 
    // obligations (CS) due for the last 3 months (report period month plus 2
    // months prior).
    // 	1) Use CS_Collected_Amt reported in the DB_STAGE_PRI_4 (Pyramid) table
    // 	2) Report 0.00 if no value
    // --------------------------------------------------------------------------------------------------
    local.Common.TotalCurrency =
      import.DashboardStagingPriority4.CsCollectedAmt.GetValueOrDefault();
    local.ContractorCaseUniverse.CurrentSupportPaid =
      UseFnB795ConvertNumToText();

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Collection Rate Last 3 Months (NOTE: This is at the case 
    // level)
    // 	1) This is a new calculation.  Use (Current Support Paid Last 3 Months 
    // amount)
    // 	   divided by (Current Support Due Last 3 Months amount) x 100.
    // 	2) Report to 1 decimal place.
    // 	3) Report 0.0 if no value
    // --------------------------------------------------------------------------------------------------
    if (import.DashboardStagingPriority4.CsDueAmt.GetValueOrDefault() == 0 || import
      .DashboardStagingPriority4.CsCollectedAmt.GetValueOrDefault() == 0)
    {
      local.ContractorCaseUniverse.CollectionRate = "0000000.0";
    }
    else
    {
      local.Common.TotalCurrency =
        Math.Round(
          import.DashboardStagingPriority4.CsCollectedAmt.GetValueOrDefault() /
        import
        .DashboardStagingPriority4.CsDueAmt.GetValueOrDefault() *
        1000, 2, MidpointRounding.AwayFromZero);
      local.Common.TotalInteger =
        (long)Math.Round(
          local.Common.TotalCurrency, MidpointRounding.AwayFromZero);
      local.ContractorCaseUniverse.CollectionRate =
        NumberToString(local.Common.TotalInteger, 8, 7) + "." + NumberToString
        (local.Common.TotalInteger, 15, 1);
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Case Paying Arrears Y,N, NA  (Y = paying, N = not paying, 
    // NA = no arrears due in FFY)
    // Cases with arrears owed where money was distributed to arrears during the
    // Federal Fiscal year.
    // 	1) From DB_AUDIT_DATA.
    // 	2) Report Y when case number is listed with one of the following
    // 	   Dashboard_Priorities:
    // 		a. 1-3(N)#1
    // 		b. 1-3(N)#2
    // 	3) Report N when case number is listed with one of the following
    // 	   Dashboard_Priorities:
    // 		a. 1-3(D)#1
    // 		b. 1-3(D)#2
    // 		c. 1-3(D)#3
    // 		d. 1-3(D)#4
    // 		e. 1-3(D)#5
    // 	   AND
    // 	   Case number is NOT listed with one of the following 
    // Dashboard_Priorities:
    // 		a.  1-3(N)#1
    // 		b. 1-3(N)#2
    // 	4) Report NA when case number is NOT listed with one of the following
    // 	   Dashboard_Priorities:
    // 		a. 1-3(D)#1
    // 		b. 1-3(D)#2
    // 		c. 1-3(D)#3
    // 		d. 1-3(D)#4
    // 		e. 1-3(D)#5
    // --------------------------------------------------------------------------------------------------
    local.DashboardAuditData.ReportMonth =
      import.DashboardStagingPriority4.ReportMonth;
    local.DashboardAuditData.RunNumber =
      import.DashboardStagingPriority4.RunNumber;

    if (ReadDashboardAuditData2())
    {
      local.Local13Numerator.Flag = "Y";
    }
    else
    {
      local.Local13Numerator.Flag = "N";
    }

    if (ReadDashboardAuditData1())
    {
      local.Local13Denominator.Flag = "Y";
    }
    else
    {
      local.Local13Denominator.Flag = "N";
    }

    if (AsChar(local.Local13Numerator.Flag) == 'Y')
    {
      local.ContractorCaseUniverse.CasePayingArrears = "Y";
    }
    else if (AsChar(local.Local13Numerator.Flag) == 'N' && AsChar
      (local.Local13Denominator.Flag) == 'Y')
    {
      local.ContractorCaseUniverse.CasePayingArrears = "N";
    }
    else if (AsChar(local.Local13Denominator.Flag) == 'N')
    {
      local.ContractorCaseUniverse.CasePayingArrears = "NA";
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Case Function
    // This will match the Case Function reported on the CADS  screen (LOC, PAT,
    // OBG, ENF) as of
    // the run date of the report.
    // --------------------------------------------------------------------------------------------------
    local.Case1.Number = import.DashboardStagingPriority4.CaseNumber;
    UseSiCabReturnCaseFunction();
    local.ContractorCaseUniverse.CaseFunction = local.CaseFuncWorkSet.FuncText3;

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Case Program
    // This will match the Case Program reported on the CADS screen
    // (AF, NA, NAI, FCI, AFI, FC, NF, NC, MAI) as of the run date of the 
    // report.
    // --------------------------------------------------------------------------------------------------
    local.Case1.Number = import.DashboardStagingPriority4.CaseNumber;
    UseSiReadCaseProgramType();
    local.ContractorCaseUniverse.CaseProgram = local.Program.Code;

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Determining Case Program...Case  " + import
        .DashboardStagingPriority4.CaseNumber + " " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: End Date of PA Program
    // 1) Report the most recent end date of a CC or AF program for any active 
    // CP or child(ren)
    //    on PEPR as of the run date of the report.    If the program on PEPR is
    // active report
    //    12/31/2099
    // 2) Report 01/01/0001 if no value.
    //    06/26/2017 - cq56416
    // 3) Added FS progam to the check for the end of date
    // --------------------------------------------------------------------------------------------------
    local.DateWorkArea.Date = local.FileDefault.Date;

    // -- AF program = 2,  CC program = 5
    local.Af.SystemGeneratedIdentifier = 2;
    local.Cc.SystemGeneratedIdentifier = 5;
    local.Fs.SystemGeneratedIdentifier = 4;
    ReadPersonProgram();

    if (Equal(local.DateWorkArea.Date, local.Null1.Date))
    {
      local.DateWorkArea.Date = local.FileDefault.Date;
    }

    local.ContractorCaseUniverse.PaProgramEndDate = UseCabFormatDate3();

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: CURA Amount
    // 1) From the CURA screen, report the combined dollar amount of the 
    // uncollected AF/FC URA
    //    for all active children on the case as of the report run date (amount 
    // reported in the
    //    URA column on CURA for AF/FC-Total.
    // 2) Do not include Medical URA, or amounts that have already been 
    // collected.
    // 3) Report 0.00 if no value
    // --------------------------------------------------------------------------------------------------
    ReadImHouseholdMbrMnthlySum();
    local.ContractorCaseUniverse.CuraAmount = UseFnB795ConvertNumToText();

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Family Violence (Y/N)
    // As viewed on the COMN screen.  Report Y where family violence is set (
    // C, D, or P code)
    // for any active person on the case as of the run date of the report, 
    // otherwise, report N.
    // --------------------------------------------------------------------------------------------------
    local.ContractorCaseUniverse.FamilyViolence = "N";
    local.Cfv.FamilyViolenceIndicator = "C";
    local.Dfv.FamilyViolenceIndicator = "D";
    local.Pfv.FamilyViolenceIndicator = "P";

    if (ReadCsePerson1())
    {
      local.ContractorCaseUniverse.FamilyViolence = "Y";
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: CP Non-Cooperation (Y/N)
    // From the CADS screen.  Report Y when the most recent (date) Non Coop 
    // indicator = Y
    // for the active CP on the case as of the run date, otherwise report N.
    // --------------------------------------------------------------------------------------------------
    local.ContractorCaseUniverse.CpNoncooperation = "N";

    if (ReadCaseRole())
    {
      if (ReadNonCooperation())
      {
        if (AsChar(entities.NonCooperation.Code) == 'Y')
        {
          local.ContractorCaseUniverse.CpNoncooperation = "Y";
        }
      }
    }

    // --------------------------------------------------------------------------------------------------
    // DATA ELEMENT: Date of Emancipation
    // 1) Determine the emancipation date for all active children on the case as
    // of the report
    //    run date (DOB + 18 years).
    // 2) Report the future emancipation date of the next active child to 
    // emancipate if that date
    //    will occur in the next 12 months from the report run date.
    // 3) Report 01/01/0001 if none found.
    // DATA ELEMENT: Youngest Active Child Emancipation Date
    // 1) For the youngest active child (CH case role as of the run date) on the
    // case, report
    //    the childs DOB + 18 years.
    // 2) Report 01/01/0001 if no value. (if there isnt a child on the case 
    // with a birth date)
    // DATA ELEMENT: Child BOW (Y/N)
    // From the CPAT screen, report Y if Born Out of Wedlock = Y for any 
    // active child on the
    // case as of the report run date.  Otherwise report N.
    // --------------------------------------------------------------------------------------------------
    local.ContractorCaseUniverse.DateOfEmancipation = local.NullTextDate.Text10;
    local.Emancipation.Date = UseCabSetMaximumDiscontinueDate();
    local.ContractorCaseUniverse.YoungestEmancipationDate =
      local.NullTextDate.Text10;
    local.YoungestEmancipation.Date = local.Null1.Date;
    local.ContractorCaseUniverse.ChildBow = "N";

    foreach(var item in ReadCaseRoleCsePerson())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Reading ADABAS...CH " + entities
          .CsePerson.Number + " " + local.ExitStateWorkArea.Message;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      if (!Lt(AddYears(local.CsePersonsWorkSet.Dob, 18),
        import.ProgramProcessingInfo.ProcessDate) && Lt
        (AddYears(local.CsePersonsWorkSet.Dob, 18), local.Emancipation.Date) &&
        !
        Lt(AddMonths(import.ProgramProcessingInfo.ProcessDate, 12),
        AddYears(local.CsePersonsWorkSet.Dob, 18)))
      {
        local.Emancipation.Date = AddYears(local.CsePersonsWorkSet.Dob, 18);
        local.ContractorCaseUniverse.DateOfEmancipation = UseCabFormatDate2();
      }

      if (Lt(local.Null1.Date, local.CsePersonsWorkSet.Dob) && Lt
        (local.YoungestEmancipation.Date, AddYears(local.CsePersonsWorkSet.Dob,
        18)))
      {
        local.YoungestEmancipation.Date =
          AddYears(local.CsePersonsWorkSet.Dob, 18);
        local.ContractorCaseUniverse.YoungestEmancipationDate =
          UseCabFormatDate1();
      }

      if (AsChar(entities.CsePerson.BornOutOfWedlock) == 'Y')
      {
        local.ContractorCaseUniverse.ChildBow = "Y";
      }
    }

    // -- CP specific data elements.
    if (ReadCsePerson2())
    {
      // -- Read CP info from adabas.
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Reading ADABAS...CP " + entities
          .CsePerson.Number + " " + local.ExitStateWorkArea.Message;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ALL_OK";
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: CP DOB
      // 1) Report the DOB for the active CP as of the run date of the report.
      // 2) Report 01/01/0001 if no value.
      // --------------------------------------------------------------------------------------------------
      if (Equal(local.CsePersonsWorkSet.Dob, local.Null1.Date))
      {
        local.ContractorCaseUniverse.CpDateOfBirth = local.NullTextDate.Text10;
      }
      else
      {
        local.DateWorkArea.Date = local.CsePersonsWorkSet.Dob;
        local.ContractorCaseUniverse.CpDateOfBirth = UseCabFormatDate3();
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: CP Ethnicity
      // 1) From ARDS, report the active CPs Race as of the run date of the 
      // report
      //    (AI, AJ, BL, DC, HI, HP, OT, SA, UK, WH).
      // 2) Leave blank if no value is entered.
      // --------------------------------------------------------------------------------------------------
      local.ContractorCaseUniverse.CpEthnicity = entities.CsePerson.Race ?? Spaces
        (2);

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: CP Zip Code
      // 1) From ADDR as of the report run date report the zip of the most 
      // recent (Verified Date) verified, non-end dated record for the active
      // CP.
      // 2) Leave blank if none exists.
      // --------------------------------------------------------------------------------------------------
      local.Dtype.LocationType = "D";

      if (ReadCsePersonAddress1())
      {
        local.ContractorCaseUniverse.CpZipCode =
          entities.CsePersonAddress.ZipCode ?? Spaces(5);
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: CP Phone Home
      // 1) From ARDS as of the run date of the report.  Use value entered for 
      // Home #.
      //    (Include area code if known, otherwise report 000, area code and 
      // phone number (no hyphen)/two columns).
      // 2) Leave blank if none entered.
      // --------------------------------------------------------------------------------------------------
      if (Lt(0, entities.CsePerson.HomePhoneAreaCode))
      {
        local.ContractorCaseUniverse.CpHomePhoneAreaCode =
          NumberToString(entities.CsePerson.HomePhoneAreaCode.
            GetValueOrDefault(), 13, 3);
      }

      if (Lt(0, entities.CsePerson.HomePhone))
      {
        local.ContractorCaseUniverse.CpHomePhone =
          NumberToString(entities.CsePerson.HomePhone.GetValueOrDefault(), 9, 7);
          
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: CP Phone Cell
      // 1) From ARDS as of the run date of the report.  Use value entered for 
      // Other # if Type  C.
      //    (Include area code if known, otherwise report 000, area code and 
      // phone number(no hyphen)/two columns)
      // 2) Leave blank if none entered.
      // --------------------------------------------------------------------------------------------------
      if (AsChar(entities.CsePerson.OtherPhoneType) == 'C')
      {
        if (Lt(0, entities.CsePerson.OtherAreaCode))
        {
          local.ContractorCaseUniverse.CpCellPhoneAreaCode =
            NumberToString(entities.CsePerson.OtherAreaCode.GetValueOrDefault(),
            13, 3);
        }

        if (Lt(0, entities.CsePerson.OtherNumber))
        {
          local.ContractorCaseUniverse.CpCellPhone =
            NumberToString(entities.CsePerson.OtherNumber.GetValueOrDefault(),
            9, 7);
        }
      }
    }

    local.NcpFound.Flag = "N";

    // -- NCP specific data elements.
    foreach(var item in ReadCsePerson3())
    {
      local.NcpFound.Flag = "Y";

      // -- Initialize the NCP attributes.
      MoveContractorCaseUniverse3(local.NullNcpData,
        local.ContractorCaseUniverse);

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP Person Number
      // 1) Report each NCP active on the case  as of the run date of the 
      // report.  A separate Type A record will be created for each NCP on the
      // case.
      // --------------------------------------------------------------------------------------------------
      local.ContractorCaseUniverse.NcpPersonNumber = entities.CsePerson.Number;

      // -- Read NCP info from adabas.
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Reading ADABAS...NCP " + entities
          .CsePerson.Number + " " + local.ExitStateWorkArea.Message;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ALL_OK";
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP Last Name
      // 1) Calculated for each NCP.  Report the NCPs last name.
      // --------------------------------------------------------------------------------------------------
      local.ContractorCaseUniverse.NcpLastName =
        local.CsePersonsWorkSet.LastName;

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP First Name
      // 1) Calculated for each NCP.  Report the NCPs first name.
      // --------------------------------------------------------------------------------------------------
      local.ContractorCaseUniverse.NcpFirstName =
        local.CsePersonsWorkSet.FirstName;

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP DOB
      // 1) Calculated for each NCP.  Report the NCPs DOB as of the run date of
      // the report.
      // 2) Report 01/01/0001 if blank.
      // --------------------------------------------------------------------------------------------------
      if (Equal(local.CsePersonsWorkSet.Dob, local.Null1.Date))
      {
        local.ContractorCaseUniverse.NcpDateOfBirth = local.NullTextDate.Text10;
      }
      else
      {
        local.DateWorkArea.Date = local.CsePersonsWorkSet.Dob;
        local.ContractorCaseUniverse.NcpDateOfBirth = UseCabFormatDate3();
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP Ethnicity
      // 1) Calculated for each NCP.  From APDS, report the NCPs Race as of the
      // run date of the
      //    report (AI, AJ, BL, DC, HI, HP, OT, SA, UK, WH).
      // 2) Leave blank if no value is entered.
      // --------------------------------------------------------------------------------------------------
      local.ContractorCaseUniverse.NcpEthnicity = entities.CsePerson.Race ?? Spaces
        (2);

      // -- Find the most recently verified non-end dated address.
      if (ReadCsePersonAddress2())
      {
        // --------------------------------------------------------------------------------------------------
        // DATA ELEMENT: NCP Locate Date
        // 1) Calculated for each NCP.  From ADDR/FADS as of the report run 
        // date.  Check for all records
        //    with a Verified Date and no End Date.
        // 2) Report the most recent Verified Date from any qualifying records.
        // 3) Report 01/01/0001 if date not found.
        // --------------------------------------------------------------------------------------------------
        local.DateWorkArea.Date = entities.CsePersonAddress.VerifiedDate;
        local.ContractorCaseUniverse.NcpLocateDate = UseCabFormatDate3();

        switch(AsChar(entities.CsePersonAddress.LocationType))
        {
          case 'D':
            // --------------------------------------------------------------------------------------------------
            // DATA ELEMENT: NCP Zip Code
            // 1) Calculated for each NCP.  From ADDR/FADS as of the report run 
            // date.
            // 2) If most recent address is a domestic address report the zip of
            // the most recent
            //    (Verified Date) verified, non-end dated record.
            // 3) If the most recent address is a foreign address report blank 
            // and display the country
            //    code in the NCP Foreign Country Code field
            // --------------------------------------------------------------------------------------------------
            local.ContractorCaseUniverse.NcpZipCode =
              entities.CsePersonAddress.ZipCode ?? Spaces(5);

            break;
          case 'F':
            // --------------------------------------------------------------------------------------------------
            // DATA ELEMENT: NCP Foreign Country Code
            // 1) If the most recent address is a foreign address Report Foreign
            // Country Code from FADS.
            // 2) Otherwise leave blank.
            // --------------------------------------------------------------------------------------------------
            local.ContractorCaseUniverse.NcpForeignCountryCode =
              entities.CsePersonAddress.Country ?? Spaces(2);

            break;
          default:
            break;
        }
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP Phone Home
      // 1) Calculated for each NCP.  From APDS as of the run date of the 
      // report.  Use value entered
      //    for Home #.
      //    (Include area code if known, otherwise report 000, area code and 
      // phone number (no hyphen)/two columns).
      // 2) Leave blank if none entered.
      // --------------------------------------------------------------------------------------------------
      if (Lt(0, entities.CsePerson.HomePhoneAreaCode))
      {
        local.ContractorCaseUniverse.NcpHomePhoneAreaCode =
          NumberToString(entities.CsePerson.HomePhoneAreaCode.
            GetValueOrDefault(), 13, 3);
      }

      if (Lt(0, entities.CsePerson.HomePhone))
      {
        local.ContractorCaseUniverse.NcpHomePhone =
          NumberToString(entities.CsePerson.HomePhone.GetValueOrDefault(), 9, 7);
          
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP Phone Cell
      // 1) Calculated for each NCP.  From APDS as of the run date of the 
      // report.  Use value entered
      //    for Other # if Type  C.
      //    (Include area code if known, otherwise report 000, area code and 
      // phone number (no hyphen)/two columns).
      // 2) Leave blank if none entered.
      // --------------------------------------------------------------------------------------------------
      if (AsChar(entities.CsePerson.OtherPhoneType) == 'C')
      {
        if (Lt(0, entities.CsePerson.OtherAreaCode))
        {
          local.ContractorCaseUniverse.NcpCellPhoneAreaCode =
            NumberToString(entities.CsePerson.OtherAreaCode.GetValueOrDefault(),
            13, 3);
        }

        if (Lt(0, entities.CsePerson.OtherNumber))
        {
          local.ContractorCaseUniverse.NcpCellPhone =
            NumberToString(entities.CsePerson.OtherNumber.GetValueOrDefault(),
            9, 7);
        }
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP Incarcerated (Y/N)
      // 1) Calculated for each NCP.  From the JAIL screen as of the report run 
      // date.  Report Y
      //    when Verified Date or Entered Incarceration Date is entered, and no 
      // Release Date is
      //    entered for the most recent Prison/Jail Record otherwise report N.
      // --------------------------------------------------------------------------------------------------
      local.ContractorCaseUniverse.NcpIncarcerated = "N";
      local.Jtype.Type1 = "J";
      local.Ptype.Type1 = "P";

      if (ReadIncarceration())
      {
        if ((Lt(local.Null1.Date, entities.Incarceration.VerifiedDate) || Lt
          (local.Null1.Date, entities.Incarceration.StartDate)) && Lt
          (import.ProgramProcessingInfo.ProcessDate,
          entities.Incarceration.EndDate))
        {
          local.ContractorCaseUniverse.NcpIncarcerated = "Y";
        }
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP in Bankruptcy (Y/N)
      // 1) Calculated for each NCP.  From the BKRP screen as of the report run 
      // date.  Report Y
      //    when Filed  Date is entered, and no Discharged or Dis/With  Date is 
      // entered,
      //    otherwise report N.
      // --------------------------------------------------------------------------------------------------
      if (ReadBankruptcy())
      {
        local.ContractorCaseUniverse.NcpInBankruptcy = "Y";
      }
      else
      {
        local.ContractorCaseUniverse.NcpInBankruptcy = "N";
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP Represented by Council (Y/N)
      // 1) Calculated for each NCP.  From the ATTY screen as of the run date.  
      // Report Y when
      //    Retained Date is entered, and no Withdrawn Date is entered.  Look 
      // for any NCP attorney
      //    record on any court order (does not have to be a court order 
      // associated to the case),
      //    other report N.
      // --------------------------------------------------------------------------------------------------
      if (ReadPersonPrivateAttorney())
      {
        local.ContractorCaseUniverse.NcpRepresentedByCouncil = "Y";
      }
      else
      {
        local.ContractorCaseUniverse.NcpRepresentedByCouncil = "N";
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP Employer Name
      // 1) Calculated for each NCP.  Report the employer name of the most 
      // recent Returned Date
      //    and no end date with the Type/Return Code of E/E, M/A.
      // 2) Leave blank if none found.
      // --------------------------------------------------------------------------------------------------
      local.Ee.Type1 = "E";
      local.Ee.ReturnCd = "E";
      local.Ma.Type1 = "M";
      local.Ma.ReturnCd = "A";

      if (ReadIncomeSource2())
      {
        if (IsEmpty(entities.IncomeSource.Name))
        {
          if (ReadEmployer())
          {
            local.ContractorCaseUniverse.NcpEmployerName =
              entities.Employer.Name ?? Spaces(36);
          }
        }
        else
        {
          local.ContractorCaseUniverse.NcpEmployerName =
            entities.IncomeSource.Name ?? Spaces(36);
        }
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: NCP Other Income Source
      // This will include non-employment income sources (unemployment, SSA, 
      // SSI, VA, Workers
      // Comp, etc)
      // 1) Calculated for each NCP.  Report the income source name of the most 
      // recent Returned
      //    Date and no end date with the Type/Return Code of 0/V (any type code
      // ), E/U, E/W, and M/R.
      // 2) Leave blank if none found.
      // --------------------------------------------------------------------------------------------------
      local.Ov.Type1 = "O";
      local.Ov.ReturnCd = "V";
      local.Eu.Type1 = "E";
      local.Eu.ReturnCd = "U";
      local.Ew.Type1 = "E";
      local.Ew.ReturnCd = "W";
      local.Mr.Type1 = "M";
      local.Mr.ReturnCd = "R";

      if (ReadIncomeSource1())
      {
        if (IsEmpty(entities.IncomeSource.Name))
        {
          if (ReadEmployer())
          {
            local.ContractorCaseUniverse.NcpOtherIncomeSource =
              entities.Employer.Name ?? Spaces(36);
          }
        }
        else
        {
          local.ContractorCaseUniverse.NcpOtherIncomeSource =
            entities.IncomeSource.Name ?? Spaces(36);
        }
      }

      // --------------------------------------------------------------------------------------------------
      // DATA ELEMENT: Interstate- Initiating (Y/N)
      // 1) Report Y where active outgoing interstate case is open as of the 
      // run date of the report
      // (OINR/CADS), otherwise report N.
      // DATA ELEMENT: Interstate- Responding (Y/N)
      // Report Y where active incoming interstate case is open as of the run 
      // date of the report
      // (IIMC/CADS), otherwise report N.
      // --------------------------------------------------------------------------------------------------
      local.ContractorCaseUniverse.NcpInterstateInitiating = "N";
      local.ContractorCaseUniverse.NcpInterstateResponding = "N";
      local.Open.OtherStateCaseStatus = "O";
      local.Csi.FunctionalTypeCode = "CSI";
      local.Lo1.FunctionalTypeCode = "LO1";

      foreach(var item1 in ReadInterstateRequest())
      {
        ReadInterstateRequestHistory1();
        ReadInterstateRequestHistory2();

        if (local.CsiLo1Total.Count == 0 && local.NonCseLo1Total.Count == 0)
        {
          continue;
        }

        switch(AsChar(entities.InterstateRequest.KsCaseInd))
        {
          case 'Y':
            local.ContractorCaseUniverse.NcpInterstateInitiating = "Y";

            break;
          case 'N':
            local.ContractorCaseUniverse.NcpInterstateResponding = "Y";

            break;
          default:
            break;
        }

        // -- If both are Y then escape?
        if (AsChar(local.ContractorCaseUniverse.NcpInterstateInitiating) == 'Y'
          && AsChar(local.ContractorCaseUniverse.NcpInterstateResponding) == 'Y'
          )
        {
          break;
        }
      }

      local.NullCourtOrder.ContractorName =
        local.ContractorCaseUniverse.ContractorName;
      local.NullCourtOrder.JudicialDistrict =
        local.ContractorCaseUniverse.JudicialDistrict;
      local.NullCourtOrder.CaseNumber = local.ContractorCaseUniverse.CaseNumber;
      local.NullCourtOrder.NcpPersonNumber =
        local.ContractorCaseUniverse.NcpPersonNumber;

      // -- Gather court order data elements...
      UseFnB795ProcessCourtOrders();

      // -- Write all 3 Data Format records to file(s)...
      for(local.Counter.Count = 1; local.Counter.Count <= 3; ++
        local.Counter.Count)
      {
        local.EabFileHandling.Action = "WRITE";

        switch(local.Counter.Count)
        {
          case 1:
            UseFnB795ProcessDataFormat4();

            break;
          case 2:
            UseFnB795ProcessDataFormat6();

            break;
          case 3:
            UseFnB795ProcessDataFormat8();

            break;
          default:
            break;
        }

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing Data Format " + NumberToString
            (local.Counter.Count, 15, 1) + " records.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    if (AsChar(local.NcpFound.Flag) == 'N')
    {
      // -- No active AP exists on the case.  Write out the case information to 
      // the files.
      // -- Initialize the NCP attributes.
      MoveContractorCaseUniverse3(local.NullNcpData,
        local.ContractorCaseUniverse);

      // -- Write all 3 Data Format records to file(s)...
      for(local.Counter.Count = 1; local.Counter.Count <= 3; ++
        local.Counter.Count)
      {
        local.EabFileHandling.Action = "WRITE";

        switch(local.Counter.Count)
        {
          case 1:
            UseFnB795ProcessDataFormat5();

            break;
          case 2:
            UseFnB795ProcessDataFormat7();

            break;
          case 3:
            UseFnB795ProcessDataFormat9();

            break;
          default:
            break;
        }

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error writing Data Format " + NumberToString
            (local.Counter.Count, 15, 1) + " records.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }
  }

  private static void MoveContractorCaseUniverse1(ContractorCaseUniverse source,
    ContractorCaseUniverse target)
  {
    target.ContractorName = source.ContractorName;
    target.JudicialDistrict = source.JudicialDistrict;
    target.CaseNumber = source.CaseNumber;
    target.NcpPersonNumber = source.NcpPersonNumber;
  }

  private static void MoveContractorCaseUniverse2(ContractorCaseUniverse source,
    ContractorCaseUniverse target)
  {
    target.CaseOpenDate = source.CaseOpenDate;
    target.CurrentSupportDue = source.CurrentSupportDue;
    target.CurrentSupportPaid = source.CurrentSupportPaid;
    target.CollectionRate = source.CollectionRate;
    target.PaProgramEndDate = source.PaProgramEndDate;
    target.CuraAmount = source.CuraAmount;
    target.PendingCaseClosureDate = source.PendingCaseClosureDate;
    target.DateOfEmancipation = source.DateOfEmancipation;
    target.YoungestEmancipationDate = source.YoungestEmancipationDate;
    target.CpDateOfBirth = source.CpDateOfBirth;
    target.NcpDateOfBirth = source.NcpDateOfBirth;
    target.NcpLocateDate = source.NcpLocateDate;
  }

  private static void MoveContractorCaseUniverse3(ContractorCaseUniverse source,
    ContractorCaseUniverse target)
  {
    target.NcpPersonNumber = source.NcpPersonNumber;
    target.NcpLastName = source.NcpLastName;
    target.NcpFirstName = source.NcpFirstName;
    target.NcpDateOfBirth = source.NcpDateOfBirth;
    target.NcpEthnicity = source.NcpEthnicity;
    target.NcpLocateDate = source.NcpLocateDate;
    target.NcpZipCode = source.NcpZipCode;
    target.NcpForeignCountryCode = source.NcpForeignCountryCode;
    target.NcpHomePhoneAreaCode = source.NcpHomePhoneAreaCode;
    target.NcpHomePhone = source.NcpHomePhone;
    target.NcpCellPhoneAreaCode = source.NcpCellPhoneAreaCode;
    target.NcpCellPhone = source.NcpCellPhone;
    target.NcpIncarcerated = source.NcpIncarcerated;
    target.NcpInBankruptcy = source.NcpInBankruptcy;
    target.NcpRepresentedByCouncil = source.NcpRepresentedByCouncil;
    target.NcpEmployerName = source.NcpEmployerName;
    target.NcpOtherIncomeSource = source.NcpOtherIncomeSource;
    target.NcpInterstateInitiating = source.NcpInterstateInitiating;
    target.NcpInterstateResponding = source.NcpInterstateResponding;
  }

  private static void MoveContractorCaseUniverse4(ContractorCaseUniverse source,
    ContractorCaseUniverse target)
  {
    target.NcpDateOfBirth = source.NcpDateOfBirth;
    target.NcpLocateDate = source.NcpLocateDate;
  }

  private static void MoveDashboardAuditData(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.ReportMonth = source.ReportMonth;
    target.RunNumber = source.RunNumber;
  }

  private static void MoveGroup1(Local.GroupGroup source,
    FnB795ProcessDataFormat1.Import.GroupGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveGroup2(Local.GroupGroup source,
    FnB795ProcessDataFormat2.Import.GroupGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveGroup3(Local.GroupGroup source,
    FnB795ProcessDataFormat3.Import.GroupGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveGroup4(FnB795ProcessCourtOrders.Export.
    GroupGroup source, Local.GroupGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.Code = source.Code;
    target.DistributionProgramType = source.DistributionProgramType;
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

  private string UseCabFormatDate1()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.YoungestEmancipation.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    return useExport.FormattedDate.Text10;
  }

  private string UseCabFormatDate2()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.Emancipation.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    return useExport.FormattedDate.Text10;
  }

  private string UseCabFormatDate3()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    return useExport.FormattedDate.Text10;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Emancipation.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private string UseFnB795ConvertNumToText()
  {
    var useImport = new FnB795ConvertNumToText.Import();
    var useExport = new FnB795ConvertNumToText.Export();

    useImport.Common.TotalCurrency = local.Common.TotalCurrency;

    Call(FnB795ConvertNumToText.Execute, useImport, useExport);

    return useExport.WorkArea.Text15;
  }

  private void UseFnB795ProcessCourtOrders()
  {
    var useImport = new FnB795ProcessCourtOrders.Import();
    var useExport = new FnB795ProcessCourtOrders.Export();

    MoveDashboardAuditData(local.DashboardAuditData,
      useImport.DashboardAuditData);
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    MoveContractorCaseUniverse1(local.ContractorCaseUniverse,
      useImport.ContractorCaseUniverse);
    useImport.NullCourtOrder.Assign(local.NullCourtOrder);

    Call(FnB795ProcessCourtOrders.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Group, MoveGroup4);
  }

  private void UseFnB795ProcessDataFormat4()
  {
    var useImport = new FnB795ProcessDataFormat1.Import();
    var useExport = new FnB795ProcessDataFormat1.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.Group.CopyTo(useImport.Group, MoveGroup1);
    useImport.ContractorCaseUniverse.Assign(local.ContractorCaseUniverse);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType1Records.Count = export.FileType1Records.Count;

    Call(FnB795ProcessDataFormat1.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    export.FileType1Records.Count = useExport.FileType1Records.Count;
  }

  private void UseFnB795ProcessDataFormat5()
  {
    var useImport = new FnB795ProcessDataFormat1.Import();
    var useExport = new FnB795ProcessDataFormat1.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ContractorCaseUniverse.Assign(local.ContractorCaseUniverse);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType1Records.Count = export.FileType1Records.Count;

    Call(FnB795ProcessDataFormat1.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    export.FileType1Records.Count = useExport.FileType1Records.Count;
  }

  private void UseFnB795ProcessDataFormat6()
  {
    var useImport = new FnB795ProcessDataFormat2.Import();
    var useExport = new FnB795ProcessDataFormat2.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.NullCourtOrder.Assign(import.NullCourtOrder);
    local.Group.CopyTo(useImport.Group, MoveGroup2);
    useImport.ContractorCaseUniverse.Assign(local.ContractorCaseUniverse);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType2Records.Count = export.FileType2Records.Count;

    Call(FnB795ProcessDataFormat2.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    export.FileType2Records.Count = useExport.FileType2Records.Count;
  }

  private void UseFnB795ProcessDataFormat7()
  {
    var useImport = new FnB795ProcessDataFormat2.Import();
    var useExport = new FnB795ProcessDataFormat2.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.NullCourtOrder.Assign(import.NullCourtOrder);
    useImport.ContractorCaseUniverse.Assign(local.ContractorCaseUniverse);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType2Records.Count = export.FileType2Records.Count;

    Call(FnB795ProcessDataFormat2.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    export.FileType2Records.Count = useExport.FileType2Records.Count;
  }

  private void UseFnB795ProcessDataFormat8()
  {
    var useImport = new FnB795ProcessDataFormat3.Import();
    var useExport = new FnB795ProcessDataFormat3.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.Group.CopyTo(useImport.Group, MoveGroup3);
    useImport.ContractorCaseUniverse.Assign(local.ContractorCaseUniverse);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType3ARecords.Count = export.FileType3ARecords.Count;
    useImport.FileType3BRecords.Count = export.FileType3BRecords.Count;

    Call(FnB795ProcessDataFormat3.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    export.FileType3ARecords.Count = useExport.FileType3ARecords.Count;
    export.FileType3BRecords.Count = useExport.FileType3BRecords.Count;
  }

  private void UseFnB795ProcessDataFormat9()
  {
    var useImport = new FnB795ProcessDataFormat3.Import();
    var useExport = new FnB795ProcessDataFormat3.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.ContractorCaseUniverse.Assign(local.ContractorCaseUniverse);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FileType3ARecords.Count = export.FileType3ARecords.Count;
    useImport.FileType3BRecords.Count = export.FileType3BRecords.Count;

    Call(FnB795ProcessDataFormat3.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    export.FileType3ARecords.Count = useExport.FileType3ARecords.Count;
    export.FileType3BRecords.Count = useExport.FileType3BRecords.Count;
  }

  private void UseSiCabReturnCaseFunction()
  {
    var useImport = new SiCabReturnCaseFunction.Import();
    var useExport = new SiCabReturnCaseFunction.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(SiCabReturnCaseFunction.Execute, useImport, useExport);

    local.CaseFuncWorkSet.Assign(useExport.CaseFuncWorkSet);
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.Program);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadBankruptcy()
  {
    entities.Bankruptcy.Populated = false;

    return Read("ReadBankruptcy",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(command, "filingDate", local.Null1.Date.GetValueOrDefault());
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

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "numb", import.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "type", local.Ar.Type1);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(
          command, "casNumber", import.DashboardStagingPriority4.CaseNumber);
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
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", import.DashboardStagingPriority4.CaseNumber);
        db.SetString(command, "type", local.Ch.Type1);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Type1 = db.GetString(reader, 6);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 7);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 8);
        entities.CsePerson.Race = db.GetNullableString(reader, 9);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 10);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 11);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 12);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 13);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 14);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCseOrganization()
  {
    entities.Contractor.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(
          command, "organztnId",
          import.DashboardStagingPriority4.ContractorNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Contractor.Code = db.GetString(reader, 0);
        entities.Contractor.Type1 = db.GetString(reader, 1);
        entities.Contractor.Name = db.GetString(reader, 2);
        entities.Contractor.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "familyViolenceIndicator1",
          local.Cfv.FamilyViolenceIndicator ?? "");
        db.SetNullableString(
          command, "familyViolenceIndicator2",
          local.Dfv.FamilyViolenceIndicator ?? "");
        db.SetNullableString(
          command, "familyViolenceIndicator3",
          local.Pfv.FamilyViolenceIndicator ?? "");
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(
          command, "casNumber", import.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 2);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 3);
        entities.CsePerson.Race = db.GetNullableString(reader, 4);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 5);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 6);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 7);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 8);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 9);
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
        db.SetString(command, "type", local.Ar.Type1);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(
          command, "casNumber", import.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 2);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 3);
        entities.CsePerson.Race = db.GetNullableString(reader, 4);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 5);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 6);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 7);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 8);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 9);
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
        db.SetString(command, "type", local.Ap.Type1);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(
          command, "casNumber", import.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 2);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 3);
        entities.CsePerson.Race = db.GetNullableString(reader, 4);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 5);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 6);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 7);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 8);
        entities.CsePerson.BornOutOfWedlock = db.GetNullableString(reader, 9);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(command, "locationType", local.Dtype.LocationType);
        db.SetNullableDate(
          command, "verifiedDate", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 6);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 6);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadDashboardAuditData1()
  {
    entities.DashboardAuditData.Populated = false;

    return Read("ReadDashboardAuditData1",
      (db, command) =>
      {
        db.
          SetInt32(command, "reportMonth", local.DashboardAuditData.ReportMonth);
          
        db.SetInt32(command, "runNumber", local.DashboardAuditData.RunNumber);
        db.SetNullableString(
          command, "caseNumber", import.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        entities.DashboardAuditData.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardAuditData.DashboardPriority = db.GetString(reader, 1);
        entities.DashboardAuditData.RunNumber = db.GetInt32(reader, 2);
        entities.DashboardAuditData.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.DashboardAuditData.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.DashboardAuditData.Populated = true;
      });
  }

  private bool ReadDashboardAuditData2()
  {
    entities.DashboardAuditData.Populated = false;

    return Read("ReadDashboardAuditData2",
      (db, command) =>
      {
        db.
          SetInt32(command, "reportMonth", local.DashboardAuditData.ReportMonth);
          
        db.SetInt32(command, "runNumber", local.DashboardAuditData.RunNumber);
        db.SetNullableString(
          command, "caseNumber", import.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        entities.DashboardAuditData.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardAuditData.DashboardPriority = db.GetString(reader, 1);
        entities.DashboardAuditData.RunNumber = db.GetInt32(reader, 2);
        entities.DashboardAuditData.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.DashboardAuditData.CaseNumber =
          db.GetNullableString(reader, 4);
        entities.DashboardAuditData.Populated = true;
      });
  }

  private bool ReadEmployer()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.IncomeSource.EmpId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Name = db.GetNullableString(reader, 1);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadImHouseholdMbrMnthlySum()
  {
    return Read("ReadImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetString(command, "type", local.Ch.Type1);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(
          command, "casNumber", import.DashboardStagingPriority4.CaseNumber);
      },
      (db, reader) =>
      {
        local.Common.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadIncarceration()
  {
    entities.Incarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(command, "type1", local.Jtype.Type1 ?? "");
        db.SetNullableString(command, "type2", local.Ptype.Type1 ?? "");
      },
      (db, reader) =>
      {
        entities.Incarceration.CspNumber = db.GetString(reader, 0);
        entities.Incarceration.Identifier = db.GetInt32(reader, 1);
        entities.Incarceration.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.Incarceration.EndDate = db.GetNullableDate(reader, 3);
        entities.Incarceration.StartDate = db.GetNullableDate(reader, 4);
        entities.Incarceration.Type1 = db.GetNullableString(reader, 5);
        entities.Incarceration.Populated = true;
      });
  }

  private bool ReadIncomeSource1()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource1",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetString(command, "type1", local.Ov.Type1);
        db.SetNullableString(command, "returnCd1", local.Ov.ReturnCd ?? "");
        db.SetString(command, "type2", local.Eu.Type1);
        db.SetNullableString(command, "returnCd2", local.Eu.ReturnCd ?? "");
        db.SetString(command, "type3", local.Ew.Type1);
        db.SetNullableString(command, "returnCd3", local.Ew.ReturnCd ?? "");
        db.SetString(command, "type4", local.Mr.Type1);
        db.SetNullableString(command, "returnCd4", local.Mr.ReturnCd ?? "");
        db.SetNullableDate(
          command, "endDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.CspINumber = db.GetString(reader, 5);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 6);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 7);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 8);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
      });
  }

  private bool ReadIncomeSource2()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource2",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetString(command, "type1", local.Ee.Type1);
        db.SetNullableString(command, "returnCd1", local.Ee.ReturnCd ?? "");
        db.SetString(command, "type2", local.Ma.Type1);
        db.SetNullableString(command, "returnCd2", local.Ma.ReturnCd ?? "");
        db.SetNullableDate(
          command, "endDt",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.CspINumber = db.GetString(reader, 5);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 6);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 7);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 8);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(
          command, "casINumber", import.DashboardStagingPriority4.CaseNumber);
        db.
          SetString(command, "othStCaseStatus", local.Open.OtherStateCaseStatus);
          
        db.SetNullableString(command, "croType", local.Ap.Type1);
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 7);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequestHistory1()
  {
    return Read("ReadInterstateRequestHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetString(
          command, "functionalTypeCode1", local.Csi.FunctionalTypeCode);
        db.SetString(
          command, "functionalTypeCode2", local.Lo1.FunctionalTypeCode);
      },
      (db, reader) =>
      {
        local.CsiLo1Total.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadInterstateRequestHistory2()
  {
    return Read("ReadInterstateRequestHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetString(
          command, "functionalTypeCode1", local.Csi.FunctionalTypeCode);
        db.SetString(
          command, "functionalTypeCode2", local.Lo1.FunctionalTypeCode);
      },
      (db, reader) =>
      {
        local.NonCseLo1Total.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.NonCooperation.Populated = false;

    return Read("ReadNonCooperation",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.CaseRole.Identifier);
        db.SetString(command, "croType", entities.CaseRole.Type1);
        db.SetString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.NonCooperation.Code = db.GetNullableString(reader, 0);
        entities.NonCooperation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.NonCooperation.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.NonCooperation.CasNumber = db.GetString(reader, 3);
        entities.NonCooperation.CspNumber = db.GetString(reader, 4);
        entities.NonCooperation.CroType = db.GetString(reader, 5);
        entities.NonCooperation.CroIdentifier = db.GetInt32(reader, 6);
        entities.NonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType", entities.NonCooperation.CroType);
      });
  }

  private bool ReadPersonPrivateAttorney()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.
          SetDate(command, "dateRetained", local.Null1.Date.GetValueOrDefault());
          
        db.SetDate(
          command, "dateDismissed",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.DateRetained = db.GetDate(reader, 2);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 3);
        entities.PersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPersonProgram()
  {
    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", import.DashboardStagingPriority4.CaseNumber);
        db.SetString(command, "type1", local.Ar.Type1);
        db.SetString(command, "type2", local.Ch.Type1);
        db.SetNullableDate(
          command, "startDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.Af.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.Fs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.Cc.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        local.DateWorkArea.Date = db.GetDate(reader, 0);
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "userId", import.DashboardStagingPriority4.WorkerId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
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
    /// A value of DashboardStagingPriority4.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority4")]
    public DashboardStagingPriority4 DashboardStagingPriority4
    {
      get => dashboardStagingPriority4 ??= new();
      set => dashboardStagingPriority4 = value;
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
    /// A value of NullCourtOrder.
    /// </summary>
    [JsonPropertyName("nullCourtOrder")]
    public ContractorCaseUniverse NullCourtOrder
    {
      get => nullCourtOrder ??= new();
      set => nullCourtOrder = value;
    }

    /// <summary>
    /// A value of NullCaseNcpCp.
    /// </summary>
    [JsonPropertyName("nullCaseNcpCp")]
    public ContractorCaseUniverse NullCaseNcpCp
    {
      get => nullCaseNcpCp ??= new();
      set => nullCaseNcpCp = value;
    }

    private DashboardStagingPriority4 dashboardStagingPriority4;
    private ProgramProcessingInfo programProcessingInfo;
    private ContractorCaseUniverse nullCourtOrder;
    private ContractorCaseUniverse nullCaseNcpCp;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FileType1Records.
    /// </summary>
    [JsonPropertyName("fileType1Records")]
    public Common FileType1Records
    {
      get => fileType1Records ??= new();
      set => fileType1Records = value;
    }

    /// <summary>
    /// A value of FileType2Records.
    /// </summary>
    [JsonPropertyName("fileType2Records")]
    public Common FileType2Records
    {
      get => fileType2Records ??= new();
      set => fileType2Records = value;
    }

    /// <summary>
    /// A value of FileType3ARecords.
    /// </summary>
    [JsonPropertyName("fileType3ARecords")]
    public Common FileType3ARecords
    {
      get => fileType3ARecords ??= new();
      set => fileType3ARecords = value;
    }

    /// <summary>
    /// A value of FileType3BRecords.
    /// </summary>
    [JsonPropertyName("fileType3BRecords")]
    public Common FileType3BRecords
    {
      get => fileType3BRecords ??= new();
      set => fileType3BRecords = value;
    }

    private Common fileType1Records;
    private Common fileType2Records;
    private Common fileType3ARecords;
    private Common fileType3BRecords;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public ContractorCaseUniverse G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private ContractorCaseUniverse g;
    }

    /// <summary>
    /// A value of Fs.
    /// </summary>
    [JsonPropertyName("fs")]
    public Program Fs
    {
      get => fs ??= new();
      set => fs = value;
    }

    /// <summary>
    /// A value of Lo1.
    /// </summary>
    [JsonPropertyName("lo1")]
    public InterstateRequestHistory Lo1
    {
      get => lo1 ??= new();
      set => lo1 = value;
    }

    /// <summary>
    /// A value of Csi.
    /// </summary>
    [JsonPropertyName("csi")]
    public InterstateRequestHistory Csi
    {
      get => csi ??= new();
      set => csi = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public InterstateRequest Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Mr.
    /// </summary>
    [JsonPropertyName("mr")]
    public IncomeSource Mr
    {
      get => mr ??= new();
      set => mr = value;
    }

    /// <summary>
    /// A value of Ew.
    /// </summary>
    [JsonPropertyName("ew")]
    public IncomeSource Ew
    {
      get => ew ??= new();
      set => ew = value;
    }

    /// <summary>
    /// A value of Eu.
    /// </summary>
    [JsonPropertyName("eu")]
    public IncomeSource Eu
    {
      get => eu ??= new();
      set => eu = value;
    }

    /// <summary>
    /// A value of Ov.
    /// </summary>
    [JsonPropertyName("ov")]
    public IncomeSource Ov
    {
      get => ov ??= new();
      set => ov = value;
    }

    /// <summary>
    /// A value of Ma.
    /// </summary>
    [JsonPropertyName("ma")]
    public IncomeSource Ma
    {
      get => ma ??= new();
      set => ma = value;
    }

    /// <summary>
    /// A value of Ee.
    /// </summary>
    [JsonPropertyName("ee")]
    public IncomeSource Ee
    {
      get => ee ??= new();
      set => ee = value;
    }

    /// <summary>
    /// A value of Ptype.
    /// </summary>
    [JsonPropertyName("ptype")]
    public Incarceration Ptype
    {
      get => ptype ??= new();
      set => ptype = value;
    }

    /// <summary>
    /// A value of Jtype.
    /// </summary>
    [JsonPropertyName("jtype")]
    public Incarceration Jtype
    {
      get => jtype ??= new();
      set => jtype = value;
    }

    /// <summary>
    /// A value of Dtype.
    /// </summary>
    [JsonPropertyName("dtype")]
    public CsePersonAddress Dtype
    {
      get => dtype ??= new();
      set => dtype = value;
    }

    /// <summary>
    /// A value of Pfv.
    /// </summary>
    [JsonPropertyName("pfv")]
    public CsePerson Pfv
    {
      get => pfv ??= new();
      set => pfv = value;
    }

    /// <summary>
    /// A value of Dfv.
    /// </summary>
    [JsonPropertyName("dfv")]
    public CsePerson Dfv
    {
      get => dfv ??= new();
      set => dfv = value;
    }

    /// <summary>
    /// A value of Cfv.
    /// </summary>
    [JsonPropertyName("cfv")]
    public CsePerson Cfv
    {
      get => cfv ??= new();
      set => cfv = value;
    }

    /// <summary>
    /// A value of Cc.
    /// </summary>
    [JsonPropertyName("cc")]
    public Program Cc
    {
      get => cc ??= new();
      set => cc = value;
    }

    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public Program Af
    {
      get => af ??= new();
      set => af = value;
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
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CaseRole Ch
    {
      get => ch ??= new();
      set => ch = value;
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
    /// A value of NcpFound.
    /// </summary>
    [JsonPropertyName("ncpFound")]
    public Common NcpFound
    {
      get => ncpFound ??= new();
      set => ncpFound = value;
    }

    /// <summary>
    /// A value of NullCourtOrder.
    /// </summary>
    [JsonPropertyName("nullCourtOrder")]
    public ContractorCaseUniverse NullCourtOrder
    {
      get => nullCourtOrder ??= new();
      set => nullCourtOrder = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of NullNcpData.
    /// </summary>
    [JsonPropertyName("nullNcpData")]
    public ContractorCaseUniverse NullNcpData
    {
      get => nullNcpData ??= new();
      set => nullNcpData = value;
    }

    /// <summary>
    /// A value of YoungestEmancipation.
    /// </summary>
    [JsonPropertyName("youngestEmancipation")]
    public DateWorkArea YoungestEmancipation
    {
      get => youngestEmancipation ??= new();
      set => youngestEmancipation = value;
    }

    /// <summary>
    /// A value of Emancipation.
    /// </summary>
    [JsonPropertyName("emancipation")]
    public DateWorkArea Emancipation
    {
      get => emancipation ??= new();
      set => emancipation = value;
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
    /// A value of NullTextDate.
    /// </summary>
    [JsonPropertyName("nullTextDate")]
    public TextWorkArea NullTextDate
    {
      get => nullTextDate ??= new();
      set => nullTextDate = value;
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
    /// A value of NonCseLo1Total.
    /// </summary>
    [JsonPropertyName("nonCseLo1Total")]
    public Common NonCseLo1Total
    {
      get => nonCseLo1Total ??= new();
      set => nonCseLo1Total = value;
    }

    /// <summary>
    /// A value of CsiLo1Total.
    /// </summary>
    [JsonPropertyName("csiLo1Total")]
    public Common CsiLo1Total
    {
      get => csiLo1Total ??= new();
      set => csiLo1Total = value;
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
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
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
    /// A value of PaEndDate.
    /// </summary>
    [JsonPropertyName("paEndDate")]
    public DateWorkArea PaEndDate
    {
      get => paEndDate ??= new();
      set => paEndDate = value;
    }

    /// <summary>
    /// A value of FileDefault.
    /// </summary>
    [JsonPropertyName("fileDefault")]
    public DateWorkArea FileDefault
    {
      get => fileDefault ??= new();
      set => fileDefault = value;
    }

    /// <summary>
    /// A value of Local13Denominator.
    /// </summary>
    [JsonPropertyName("local13Denominator")]
    public Common Local13Denominator
    {
      get => local13Denominator ??= new();
      set => local13Denominator = value;
    }

    /// <summary>
    /// A value of Local13Numerator.
    /// </summary>
    [JsonPropertyName("local13Numerator")]
    public Common Local13Numerator
    {
      get => local13Numerator ??= new();
      set => local13Numerator = value;
    }

    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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
    /// A value of ContractorCaseUniverse.
    /// </summary>
    [JsonPropertyName("contractorCaseUniverse")]
    public ContractorCaseUniverse ContractorCaseUniverse
    {
      get => contractorCaseUniverse ??= new();
      set => contractorCaseUniverse = value;
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

    private Program fs;
    private InterstateRequestHistory lo1;
    private InterstateRequestHistory csi;
    private InterstateRequest open;
    private IncomeSource mr;
    private IncomeSource ew;
    private IncomeSource eu;
    private IncomeSource ov;
    private IncomeSource ma;
    private IncomeSource ee;
    private Incarceration ptype;
    private Incarceration jtype;
    private CsePersonAddress dtype;
    private CsePerson pfv;
    private CsePerson dfv;
    private CsePerson cfv;
    private Program cc;
    private Program af;
    private CaseRole ap;
    private CaseRole ch;
    private CaseRole ar;
    private Common ncpFound;
    private ContractorCaseUniverse nullCourtOrder;
    private Common counter;
    private Common common;
    private Array<GroupGroup> group;
    private ContractorCaseUniverse nullNcpData;
    private DateWorkArea youngestEmancipation;
    private DateWorkArea emancipation;
    private CsePersonsWorkSet csePersonsWorkSet;
    private TextWorkArea nullTextDate;
    private DateWorkArea null1;
    private Common nonCseLo1Total;
    private Common csiLo1Total;
    private Program program;
    private CaseFuncWorkSet caseFuncWorkSet;
    private Case1 case1;
    private DateWorkArea paEndDate;
    private DateWorkArea fileDefault;
    private Common local13Denominator;
    private Common local13Numerator;
    private DashboardAuditData dashboardAuditData;
    private DateWorkArea dateWorkArea;
    private ContractorCaseUniverse contractorCaseUniverse;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
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
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
    }

    private Employer employer;
    private IncomeSource incomeSource;
    private Incarceration incarceration;
    private PersonPrivateAttorney personPrivateAttorney;
    private Bankruptcy bankruptcy;
    private CsePersonAddress csePersonAddress;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private NonCooperation nonCooperation;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private Program program;
    private PersonProgram personProgram;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private DashboardAuditData dashboardAuditData;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private CseOrganization contractor;
  }
#endregion
}
