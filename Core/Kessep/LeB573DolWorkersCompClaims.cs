// Program: LE_B573_DOL_WORKERS_COMP_CLAIMS, ID: 1902561492, model: 746.
// Short name: SWEL573B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B573_DOL_WORKERS_COMP_CLAIMS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB573DolWorkersCompClaims: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B573_DOL_WORKERS_COMP_CLAIMS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB573DolWorkersCompClaims(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB573DolWorkersCompClaims.
  /// </summary>
  public LeB573DolWorkersCompClaims(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 07/15/16  GVandy	CQ51956		Initial Development.
    // 09/27/16  GVandy	CQ55196		Change the CSLN entry to display the 
    // Administrative
    // 					Claim # instead of the Agency Claim #.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Business Rules
    // Phase 1
    // 	1. See Auto IWO business rules - Automatic Income Withholding - DOL UI 
    // Benefits
    // 	   (ORDIWO2B) section.
    // 	2. Department of Labor will use the same file that we send for UI 
    // benefits match to
    // 	   compare against the Workers compensation database.  The file will 
    // include all NCPs
    // 	   that meet the criteria not just new NCPs for the week.
    // 	3. DOL will send back a file with any Workers comp matches. The 
    // information for
    // 	   docketed cases will include the following.
    // 		a. Return of the NCP # that DCF sent
    // 		b. Claimant Name
    // 		c. Claimant Address
    // 		d. Claimant Attorney
    // 		e. Claimant Attorney Address
    // 		f. Employer
    // 		g. Employer Address
    // 		h. Docket Number for docketed cases
    // 		i. Insurance Carrier
    // 		j. Insurance Carrier Address
    // 		k. Insurance Company Attorney
    // 		l. Insurance Company Attorney Address
    // 		m. Name of Contact for Insurance
    // 		n. Insurance Contact Phone Number
    // 		o. Insurance Claim Number
    // 		p. Date of Loss - this is the date the injury was first reported
    // 		q. Information from Accident Report
    // 			i.   EmployerID
    // 			ii.  EmpAddrCode - send default
    // 			iii. DateofAccident
    // 			iv.  GrAvgWeekWg
    // 			v.   AccidentCity
    // 			vi.  AccidentCounty
    // 			vii. AccidentState
    // 			viii.CauseCode description- what caused the accident
    // 			ix.  Severity Code description - How much time was lost from the job
    // 			x.   ReturnedtworkDate
    // 			xi.  CompenPaid
    // 			xii. CompPayDate
    // 			xiii.Weeklyrate
    // 			xiv. DateofDeath
    // 			xv.  TPAName
    // 			xvi. PolicyNbr
    // 			xvii.ClaimNbr
    // 			xviii.FiledDate
    // 			xix.  agencyClaimNumber
    // 		r. A docketed/un-docketed indicator
    // 	4. This information isnt available to DOL until the hearing is being 
    // scheduled.
    // 	   There will be a manual process between DOL and CSS for getting this 
    // information.
    // 	   DOL will contact Daric Smith and James Orth in CSS.
    // 		a. Date of Hearing
    // 		b. Name of Special Administrative Law Judge (ALJ)
    // 		c. Address for Special ALJ
    // 		d. Phone Number for Special ALJ
    // 		e. Fax Number for Special ALJ
    // 	5. The return file from DOL will be received daily.
    // 	6. DOL will flag claimants in their database that match with child 
    // support NCPs.
    // 	7. DOL will compare this weeks file of NCPs with last weeks file of NCP
    // s to determine
    // 	   if the flag can be removed from claimants in their database.
    // 	8. A tab delimited file will be created and sent to Workers Comp email 
    // box.
    // 	   (Business will import the file to Excel.)
    // 	9. A HIST record will be created for each case.
    // 	10. A Narrative will be created for each case that contains the 
    // information from
    // 	   the return file.
    // 		a. CSS ADMIN: DOL WORKERS COMP INSURANCE DATA MATCH
    // 		b. NAME OF INS CO
    // 		c. DATE OF LOSS
    // 		d. CLAIM# XXXXXX
    // 		e. CONTACT: ADJ NAME, ADJ ADDR, PHONE #, FAX, EMAIL
    // 	11. THIS REQUIREMENT REMOVED FOR PHASE 1.
    // 	    An Alert will be created if there is a different address.  There 
    // will be an
    // 	   alert for each caseworker. Alert will read  Address received from 
    // Workers
    // 	   Comp.  Review for updates.
    // 	12. Create a table to store the information from Phase 1 so information 
    // can be
    // 	   populated to be viewed online once screens are developed in Phase 2.
    // --------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------
    // --  Set the field delimiter to the <tab> character.
    // -------------------------------------------------------------------------------------
    local.FieldDelimiter.Text1 = "\t";

    // -------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // ------------------------------------------------------------------------------
    // -- Read for restart info.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // ------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      //  Checkpoint Info...
      // 	Position  Description
      // 	--------  
      // ---------------------------------------------------------
      //         001-009   Number of Records Previously Processed from Claim 
      // File
      //         010-010   Blank
      // 	011-020   Last NCP Number Processed
      // -------------------------------------------------------------------------------------
      local.TotalNumbOfClaimsRead.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 9));
      local.RestartNcp.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 11, 10);
      local.RestartRecordNumber.Count = local.TotalNumbOfClaimsRead.Count;

      // -------------------------------------------------------------------------------------
      // --  Open the Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseLeB573ReadWrkCompFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error opening input file on restart.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -------------------------------------------------------------------------------------
      // --  Position the Input File.
      // -------------------------------------------------------------------------------------
      local.Common.Count = 1;

      for(var limit = local.RestartRecordNumber.Count; local.Common.Count <= limit
        ; ++local.Common.Count)
      {
        local.EabFileHandling.Action = "READ";
        UseLeB573ReadWrkCompFile1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          if (Equal(local.EabFileHandling.Status, "EF"))
          {
            local.EabReportSend.RptDetail =
              "End of file encountered before finding restart record  " + NumberToString
              (local.RestartRecordNumber.Count, 9, 6);
          }
          else
          {
            local.EabReportSend.RptDetail =
              "Error positioning input file.  Return status = " + local
              .EabFileHandling.Status;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      if (Equal(local.RestartNcp.Number,
        local.WorkersCompClaimFileRecord.NcpNumber))
      {
        // -- Log restart information to the control report.
        for(local.Common.Count = 1; local.Common.Count <= 3; ++
          local.Common.Count)
        {
          if (local.Common.Count == 1)
          {
            local.EabReportSend.RptDetail =
              "Program is restarting after file record number " + NumberToString
              (local.RestartRecordNumber.Count, 10, 6) + " CSP Number " + local
              .RestartNcp.Number;
          }
          else
          {
            local.EabReportSend.RptDetail = "";
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabControlReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // -- Write to the error report.
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(02) Error Writing Control Report...  Returned Status = " + local
              .EabFileHandling.Status;
            UseCabErrorReport2();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }
      else
      {
        // -- If the person number does not match the checkpoint person # then 
        // log to error report.
        local.EabReportSend.RptDetail = "Restart CSP # " + local
          .RestartNcp.Number + " does not match file record # " + NumberToString
          (local.RestartRecordNumber.Count, 10, 6) + " CSP # " + local
          .WorkersCompClaimFileRecord.NcpNumber;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      local.TotalNumbOfClaimsRead.Count = 0;

      // -------------------------------------------------------------------------------------
      // --  Open the Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseLeB573ReadWrkCompFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error opening input file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.NumbClaimsSinceChckpnt.Count = 0;

    // -- Determine the largest identifier in the workers_comp_claim table.
    ReadWorkersCompClaim();

    // -------------------------------------------------------------------------------------
    // -- Read each claim record from the input file.
    // -------------------------------------------------------------------------------------
    do
    {
      // ---------------------------------------------------------------------------------------------------
      // MATCH RESPONSE FILE RECORD LAYOUT (FROM KDOL)
      // Field name			type(len)	Max length	Note
      // -------------------------	-------------	----------	----------------
      // NCP				char(14)	14	
      // ClmtFirstName			varchar(50)	20	
      // ClmtLastName			varchar(50)	39	
      // ClmtMiddleName			varchar(50)	16	
      // ClmtAddress1			varchar(100)	52	
      // ClmtCity			varchar(50)	47	
      // ClmtState			varchar(10)	2	
      // ClmtZip				varchar(10)	10	
      // ClmtAttyFirstName		varchar(25)	11	
      // ClmtAttyLastName		varchar(25)	20	
      // ClmtAttyFirmName		varchar(50)	50	
      // ClmtAttyAddr1			varchar(50)	35	
      // ClmtAttyCity			varchar(25)	17	
      // ClmtAttyState			varchar(2)	2	
      // ClmtAttyZip			varchar(15)	10	
      // Employer			varchar(100)	71	
      // EmpAddr1			varchar(50)	49	
      // EmpAddr_City			varchar(25)	22	
      // EmpAddr_State			varchar(2)	2	
      // EmpAddr_Zip			varchar(15)	10	
      // DocketNumber			int		7	
      // InsuranceCarrier		varchar(50)	50	
      // CarAddr1			varchar(50)	48	
      // CarAddr_City			varchar(25)	22	
      // CarAddr_State			varchar(2)	2	
      // CarAddr_Zip			varchar(15)	10	
      // CarrAttyFirmName		varchar(50)	50	
      // CarrAttyAddr1			varchar(50)	35	
      // CarrAttyCity			varchar(25)	17	
      // CarrAttyState			varchar(2)	2	
      // CarrAttyZip			varchar(15)	10	
      // CarrContactName1		varchar(50)	35	
      // CarrContactName2		varchar(50)	35	
      // CarrContactPhone		varchar(25)	20	
      // PolicyNumber			varchar(30)	30	
      // DateOfLoss			date		10		YYYY-MM-DD
      // Employer_FEIN			varchar(30)	10	
      // EmployerAddress1		varchar(40)	40	
      // EmployerCity			varchar(30)	15	
      // EmployerState			varchar(2)	2	
      // EmployerZip			varchar(9)	9	
      // DateOfAccident			date		10		YYYY-MM-DD
      // WageAmount			varchar(15)	12	
      // AccidentCity			varchar(25)	25	
      // AccidentCounty			varchar(4)	3	
      // AccidentState			varchar(2)	12	
      // AccidentDescription		varchar(500)	500	
      // TimeLost			varchar(15)	12	
      // ReturnedtWorkDate		date		10		YYYY-MM-DD
      // CompenPaid			char(1)		1	
      // CompPayDate			date		10		YYYY-MM-DD
      // WeeklyRate			money		7		9999.99
      // DateOfDeath			date		10		YYYY-MM-DD
      // TPAName				varchar(40)	40	
      // AdminClaimNumber		varchar(30)	25	
      // FiledDate			date		19		YYYY-MM-DD
      // agencyClaimNumber		varchar(30)	12	
      // 				----------------------
      // 				Field Max	1711	
      // 				Tabs		56	
      // 				Total Length	1767	
      // ---------------------------------------------------------------------------------------------------
      ++local.NumbClaimsSinceChckpnt.Count;

      // -- Initialize views for receiving data from the input file.
      local.WorkersCompClaim.Assign(local.NullWorkersCompClaim);
      local.WorkersCompAddress.Assign(local.NullWorkersCompAddress);
      local.InsuranceCarrier.Assign(local.NullWorkersCompAddress);
      local.WorkersCompClaimFileRecord.Assign(
        local.NullWorkersCompClaimFileRecord);

      // -------------------------------------------------------------------------------------
      // -- Read claim record from the input file.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "READ";
      UseLeB573ReadWrkCompFile1();

      if (Equal(local.EabFileHandling.Status, "EF"))
      {
        continue;
      }

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail =
          "Error reading input file.  Return status = " + local
          .EabFileHandling.Status;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      ++local.TotalNumbOfClaimsRead.Count;

      // -- The logic contained in the IF statement below is used only for 
      // testing checkpoint/restart logic.
      if (Equal(TrimEnd(local.WorkersCompClaimFileRecord.NcpNumber), "ABEND"))
      {
        local.EabReportSend.RptDetail =
          "Testing Checkpoint/Restart Logic for NCP # " + local
          .WorkersCompClaimFileRecord.NcpNumber;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      if (!ReadCsePerson())
      {
        local.EabReportSend.RptDetail =
          "NCP Not Found.  Returned NCP Number " + local
          .WorkersCompClaimFileRecord.NcpNumber;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        continue;
      }

      // -- Setup local workers_comp_claim view with trimmed file record data.
      local.WorkersCompClaim.AccidentCity =
        TrimEnd(local.WorkersCompClaimFileRecord.AccidentCity);
      local.WorkersCompClaim.AccidentCounty =
        TrimEnd(local.WorkersCompClaimFileRecord.AccidentCounty);
      local.WorkersCompClaim.AccidentDescription =
        TrimEnd(local.WorkersCompClaimFileRecord.AccidentDescription);
      local.WorkersCompClaim.AccidentState =
        TrimEnd(local.WorkersCompClaimFileRecord.AccidentState);
      local.WorkersCompClaim.AdministrativeClaimNo =
        TrimEnd(local.WorkersCompClaimFileRecord.AdministrativeClaimNumber);
      local.WorkersCompClaim.AgencyClaimNo =
        TrimEnd(local.WorkersCompClaimFileRecord.AgencyClaimNo);
      local.WorkersCompClaim.ClaimantAttorneyFirmName =
        TrimEnd(local.WorkersCompClaimFileRecord.ClaimantAttorneyFirmName);
      local.WorkersCompClaim.ClaimantAttorneyFirstName =
        TrimEnd(local.WorkersCompClaimFileRecord.ClaimantAttorneyFirstName);
      local.WorkersCompClaim.ClaimantAttorneyLastName =
        TrimEnd(local.WorkersCompClaimFileRecord.ClaimantAttorneyLastName);
      local.WorkersCompClaim.ClaimantFirstName =
        TrimEnd(local.WorkersCompClaimFileRecord.ClaimantFirstName);
      local.WorkersCompClaim.ClaimantLastName =
        TrimEnd(local.WorkersCompClaimFileRecord.ClaimantLastName);
      local.WorkersCompClaim.ClaimantMiddleName =
        TrimEnd(local.WorkersCompClaimFileRecord.ClaimantMiddleName);
      local.WorkersCompClaim.CompensationPaidFlag =
        TrimEnd(local.WorkersCompClaimFileRecord.CompensationPaidFlag);
      local.WorkersCompClaim.DocketNumber =
        TrimEnd(local.WorkersCompClaimFileRecord.DocketNumber);
      local.WorkersCompClaim.EmployerFein =
        TrimEnd(local.WorkersCompClaimFileRecord.EmployerFein);
      local.WorkersCompClaim.EmployerName =
        TrimEnd(local.WorkersCompClaimFileRecord.EmployerName);
      local.WorkersCompClaim.PolicyNumber =
        TrimEnd(local.WorkersCompClaimFileRecord.PolicyNumber);
      local.WorkersCompClaim.InsurerAttorneyFirmName =
        TrimEnd(local.WorkersCompClaimFileRecord.InsurerAttorneyFirmName);
      local.WorkersCompClaim.InsurerContactName1 =
        TrimEnd(local.WorkersCompClaimFileRecord.InsurerContactName1);
      local.WorkersCompClaim.InsurerContactName2 =
        TrimEnd(local.WorkersCompClaimFileRecord.InsurerContactName2);
      local.WorkersCompClaim.InsurerContactPhone =
        TrimEnd(local.WorkersCompClaimFileRecord.InsurerContactPhone);
      local.WorkersCompClaim.InsurerName =
        TrimEnd(local.WorkersCompClaimFileRecord.InsurerName);
      local.WorkersCompClaim.SeverityCodeDescription =
        TrimEnd(local.WorkersCompClaimFileRecord.SeverityCodeDescription);
      local.WorkersCompClaim.ThirdPartyAdministratorName =
        TrimEnd(local.WorkersCompClaimFileRecord.ThirdPartyAdministratorName);
      local.WorkersCompClaim.WageAmount =
        TrimEnd(local.WorkersCompClaimFileRecord.WageAmount);
      local.WorkersCompClaim.WeeklyRate =
        TrimEnd(local.WorkersCompClaimFileRecord.WeeklyRate);

      // -- Convert from text values to numeric values.
      if (IsEmpty(local.WorkersCompClaimFileRecord.DocketNumber))
      {
      }
      else
      {
      }

      // -- Convert from text values to date values.
      if (IsEmpty(local.WorkersCompClaimFileRecord.ClaimFiledDate))
      {
        local.WorkersCompClaim.ClaimFiledDate = new DateTime(1, 1, 1);
      }
      else
      {
        local.WorkersCompClaim.ClaimFiledDate =
          StringToDate(local.WorkersCompClaimFileRecord.ClaimFiledDate);
      }

      if (IsEmpty(local.WorkersCompClaimFileRecord.CompensationPaidDate))
      {
        local.WorkersCompClaim.CompensationPaidDate = new DateTime(1, 1, 1);
      }
      else
      {
        local.WorkersCompClaim.CompensationPaidDate =
          StringToDate(local.WorkersCompClaimFileRecord.CompensationPaidDate);
      }

      if (IsEmpty(local.WorkersCompClaimFileRecord.DateOfAccident))
      {
        local.WorkersCompClaim.DateOfAccident = new DateTime(1, 1, 1);
      }
      else
      {
        local.WorkersCompClaim.DateOfAccident =
          StringToDate(local.WorkersCompClaimFileRecord.DateOfAccident);
      }

      if (IsEmpty(local.WorkersCompClaimFileRecord.DateOfDeath))
      {
        local.WorkersCompClaim.DateOfDeath = new DateTime(1, 1, 1);
      }
      else
      {
        local.WorkersCompClaim.DateOfDeath =
          StringToDate(local.WorkersCompClaimFileRecord.DateOfDeath);
      }

      if (IsEmpty(local.WorkersCompClaimFileRecord.DateOfLoss))
      {
        local.WorkersCompClaim.DateOfLoss = new DateTime(1, 1, 1);
      }
      else
      {
        local.WorkersCompClaim.DateOfLoss =
          StringToDate(local.WorkersCompClaimFileRecord.DateOfLoss);
      }

      if (IsEmpty(local.WorkersCompClaimFileRecord.ReturnedToWorkDate))
      {
        local.WorkersCompClaim.ReturnedToWorkDate = new DateTime(1, 1, 1);
      }
      else
      {
        local.WorkersCompClaim.ReturnedToWorkDate =
          StringToDate(local.WorkersCompClaimFileRecord.ReturnedToWorkDate);
      }

      ++local.Max.Identifier;

      try
      {
        CreateWorkersCompClaim();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "WORKERS_COMP_CLAIM_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "WORKERS_COMP_CLAIM_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error creating workers comp claim record..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      // -- Store addresses.
      for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
      {
        local.WorkersCompAddress.Assign(local.NullWorkersCompAddress);

        switch(local.Common.Count)
        {
          case 1:
            if (IsEmpty(local.WorkersCompClaimFileRecord.ClaimantStreet))
            {
              continue;
            }
            else
            {
              local.WorkersCompAddress.TypeCode = "CMT";
              local.WorkersCompAddress.StreetAddress =
                local.WorkersCompClaimFileRecord.ClaimantStreet;
              local.WorkersCompAddress.City =
                local.WorkersCompClaimFileRecord.ClaimantCity;
              local.WorkersCompAddress.State =
                local.WorkersCompClaimFileRecord.ClaimantState;
              local.WorkersCompAddress.ZipCode =
                local.WorkersCompClaimFileRecord.ClaimantZip;
            }

            break;
          case 2:
            if (IsEmpty(local.WorkersCompClaimFileRecord.ClaimantAttorneyStreet))
              
            {
              continue;
            }
            else
            {
              local.WorkersCompAddress.TypeCode = "CAT";
              local.WorkersCompAddress.StreetAddress =
                local.WorkersCompClaimFileRecord.ClaimantAttorneyStreet;
              local.WorkersCompAddress.City =
                local.WorkersCompClaimFileRecord.ClaimantAttorneyCity;
              local.WorkersCompAddress.State =
                local.WorkersCompClaimFileRecord.ClaimantAttorneyState;
              local.WorkersCompAddress.ZipCode =
                local.WorkersCompClaimFileRecord.ClaimantAttorneyZip;
            }

            break;
          case 3:
            if (IsEmpty(local.WorkersCompClaimFileRecord.EmployerStreet))
            {
              continue;
            }
            else
            {
              local.WorkersCompAddress.TypeCode = "EMP";
              local.WorkersCompAddress.StreetAddress =
                local.WorkersCompClaimFileRecord.EmployerStreet;
              local.WorkersCompAddress.City =
                local.WorkersCompClaimFileRecord.EmployerCity;
              local.WorkersCompAddress.State =
                local.WorkersCompClaimFileRecord.EmployerState;
              local.WorkersCompAddress.ZipCode =
                local.WorkersCompClaimFileRecord.EmployerZip;
            }

            break;
          case 4:
            if (IsEmpty(local.WorkersCompClaimFileRecord.InsurerStreet))
            {
              continue;
            }
            else
            {
              local.WorkersCompAddress.TypeCode = "INS";
              local.WorkersCompAddress.StreetAddress =
                local.WorkersCompClaimFileRecord.InsurerStreet;
              local.WorkersCompAddress.City =
                local.WorkersCompClaimFileRecord.InsurerCity;
              local.WorkersCompAddress.State =
                local.WorkersCompClaimFileRecord.InsurerState;
              local.WorkersCompAddress.ZipCode =
                local.WorkersCompClaimFileRecord.InsurerZip;
            }

            // -- This view is needed for creating the CSLN entry below.
            local.InsuranceCarrier.Assign(local.WorkersCompAddress);

            break;
          case 5:
            if (IsEmpty(local.WorkersCompClaimFileRecord.InsurerAttorneyStreet))
            {
              continue;
            }
            else
            {
              local.WorkersCompAddress.TypeCode = "IAT";
              local.WorkersCompAddress.StreetAddress =
                local.WorkersCompClaimFileRecord.InsurerAttorneyStreet;
              local.WorkersCompAddress.City =
                local.WorkersCompClaimFileRecord.InsurerAttorneyCity;
              local.WorkersCompAddress.State =
                local.WorkersCompClaimFileRecord.InsurerAttorneyState;
              local.WorkersCompAddress.ZipCode =
                local.WorkersCompClaimFileRecord.InsurerAttorneyZip;
            }

            break;
          default:
            break;
        }

        try
        {
          CreateWorkersCompAddress();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "WORKERS_COMP_CLAIM_ADDRESS_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "WORKERS_COMP_CLAIM_ADDRESS_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Extract the exit state message and write to the error report.
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error creating workers comp claim address record..." + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
        }
      }

      // -- Initialize local infrastructure view...
      local.Infrastructure.Assign(local.NullInfrastructure);

      // -------------------------------------------------------------------------------------
      // -- Raise Event to create HIST record and create CSLN entry.
      // -------------------------------------------------------------------------------------
      foreach(var item in ReadCase())
      {
        local.Infrastructure.EventId = 10;
        local.Infrastructure.ReasonCode = "WRKCOMPCLMRCVD";
        local.Infrastructure.Detail =
          "A Workers Compensation Claim was received from KDOL";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CsePersonNumber = entities.Ncp.Number;
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        if (ReadCaseUnit())
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        }
        else
        {
          continue;
        }

        local.Infrastructure.CsenetInOutCode = "";
        local.Infrastructure.InitiatingStateCode = "KS";
        local.Infrastructure.ReferenceDate =
          local.ProgramProcessingInfo.ProcessDate;
        local.Infrastructure.UserId = global.UserId;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error Creating Infrastructure... " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
        }

        // -------------------------------------------------------------------------------------
        // -- Create CSLN entry.
        // -------------------------------------------------------------------------------------
        local.NarrativeDetail.InfrastructureId =
          local.Infrastructure.SystemGeneratedIdentifier;
        local.NarrativeDetail.CaseNumber = entities.Case1.Number;
        local.NarrativeDetail.CreatedBy = global.UserId;
        local.NarrativeDetail.CreatedTimestamp =
          local.Infrastructure.CreatedTimestamp;

        for(local.NarrativeDetail.LineNumber = 1; local
          .NarrativeDetail.LineNumber <= 13; ++
          local.NarrativeDetail.LineNumber)
        {
          // --Set the narrative values...
          switch(local.NarrativeDetail.LineNumber)
          {
            case 1:
              local.NarrativeDetail.NarrativeText =
                "THE FOLLOWING WORKERS COMPENSATION CLAIM INFORMATION WAS RETURNED BY";
                

              break;
            case 2:
              local.NarrativeDetail.NarrativeText = "KDOL FOR NCP " + entities
                .Ncp.Number + ":";

              break;
            case 3:
              local.NarrativeDetail.NarrativeText = "";

              break;
            case 4:
              // --09/27/16  GVandy  CQ55196  Change the CSLN entry to display 
              // the Administrative
              //   Claim # instead of the Agency Claim #.
              local.NarrativeDetail.NarrativeText = " Date of Loss: " + NumberToString
                (Month(local.WorkersCompClaim.DateOfLoss), 14, 2) + NumberToString
                (Day(local.WorkersCompClaim.DateOfLoss), 14, 2) + NumberToString
                (Year(local.WorkersCompClaim.DateOfLoss), 12, 4) + "  Claim#: " +
                (local.WorkersCompClaim.AdministrativeClaimNo ?? "");

              break;
            case 5:
              local.NarrativeDetail.NarrativeText = " Insurance Company: " + TrimEnd
                (local.WorkersCompClaim.InsurerName);

              break;
            case 6:
              local.NarrativeDetail.NarrativeText = " Contact: " + TrimEnd
                (local.WorkersCompClaim.InsurerContactName1) + " " + TrimEnd
                (local.WorkersCompClaim.InsurerContactName2);

              break;
            case 7:
              local.NarrativeDetail.NarrativeText = " Address: " + TrimEnd
                (local.InsuranceCarrier.StreetAddress);

              break;
            case 8:
              local.NarrativeDetail.NarrativeText = "          " + TrimEnd
                (local.InsuranceCarrier.City) + ", " + (
                  local.InsuranceCarrier.State ?? "") + " " + (
                  local.InsuranceCarrier.ZipCode ?? "");

              break;
            case 9:
              local.NarrativeDetail.NarrativeText = " Phone: " + TrimEnd
                (local.WorkersCompClaim.InsurerContactPhone);

              break;
            case 10:
              local.NarrativeDetail.NarrativeText = "";

              break;
            case 11:
              if (IsEmpty(local.WorkersCompClaimFileRecord.
                ThirdPartyAdministratorName))
              {
                continue;
              }
              else
              {
                local.NarrativeDetail.NarrativeText =
                  " Third Party Administrator: " + TrimEnd
                  (local.WorkersCompClaim.ThirdPartyAdministratorName);
              }

              break;
            case 12:
              if (IsEmpty(local.WorkersCompClaimFileRecord.
                ThirdPartyAdministratorName))
              {
                continue;
              }
              else
              {
                local.NarrativeDetail.NarrativeText = " Filed Date: " + NumberToString
                  (Month(local.WorkersCompClaim.ClaimFiledDate), 14, 2) + NumberToString
                  (Day(local.WorkersCompClaim.ClaimFiledDate), 14, 2) + NumberToString
                  (Year(local.WorkersCompClaim.ClaimFiledDate), 12, 4);
              }

              break;
            case 13:
              if (IsEmpty(local.WorkersCompClaimFileRecord.
                ThirdPartyAdministratorName))
              {
                continue;
              }
              else
              {
                local.NarrativeDetail.NarrativeText = "";
              }

              break;
            default:
              break;
          }

          UseSpCabCreateNarrativeDetail();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Error Creating Narrative... " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      // -- Commit processing.
      if (local.NumbClaimsSinceChckpnt.Count > local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -- Checkpoint.
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	Position  Description
        // 	--------  
        // ---------------------------------------------------------
        //         001-009   Number of Records Previously Processed from Claim 
        // File
        //         010-010   Blank
        // 	011-020   Last NCP Number Processed
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(local.TotalNumbOfClaimsRead.Count, 7, 9) + " " + entities
          .Ncp.Number;
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error committing.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.NumbClaimsSinceChckpnt.Count = 0;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -------------------------------------------------------------------------------------
      // --  Write Totals to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail =
            "Number of Claims Read from Workers Comp File.........." + NumberToString
            (local.TotalNumbOfClaimsRead.Count, 9, 7);
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(02) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto Test;
        }
      }

      // ------------------------------------------------------------------------------
      // -- Take a final checkpoint.
      // ------------------------------------------------------------------------------
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdateCheckpointRstAndCommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error taking final checkpoint.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

Test:

    // -------------------------------------------------------------------------------------
    // --  Close the Input File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeB573ReadWrkCompFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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

  private void UseLeB573ReadWrkCompFile1()
  {
    var useImport = new LeB573ReadWrkCompFile.Import();
    var useExport = new LeB573ReadWrkCompFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.FieldDelimiter.Text1 = local.FieldDelimiter.Text1;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.WorkersCompClaimFileRecord.
      Assign(local.WorkersCompClaimFileRecord);

    Call(LeB573ReadWrkCompFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.WorkersCompClaimFileRecord.
      Assign(useExport.WorkersCompClaimFileRecord);
  }

  private void UseLeB573ReadWrkCompFile2()
  {
    var useImport = new LeB573ReadWrkCompFile.Import();
    var useExport = new LeB573ReadWrkCompFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB573ReadWrkCompFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabCreateNarrativeDetail()
  {
    var useImport = new SpCabCreateNarrativeDetail.Import();
    var useExport = new SpCabCreateNarrativeDetail.Export();

    useImport.NarrativeDetail.Assign(local.NarrativeDetail);

    Call(SpCabCreateNarrativeDetail.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateWorkersCompAddress()
  {
    System.Diagnostics.Debug.Assert(entities.WorkersCompClaim.Populated);

    var cspNumber = entities.WorkersCompClaim.CspNumber;
    var wccIdentifier = entities.WorkersCompClaim.Identifier;
    var typeCode = local.WorkersCompAddress.TypeCode;
    var streetAddress = local.WorkersCompAddress.StreetAddress ?? "";
    var city = local.WorkersCompAddress.City ?? "";
    var state = local.WorkersCompAddress.State ?? "";
    var zipCode = local.WorkersCompAddress.ZipCode ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.WorkersCompAddress.Populated = false;
    Update("CreateWorkersCompAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "wccIdentifier", wccIdentifier);
        db.SetString(command, "typeCode", typeCode);
        db.SetNullableString(command, "streetAddress", streetAddress);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
      });

    entities.WorkersCompAddress.CspNumber = cspNumber;
    entities.WorkersCompAddress.WccIdentifier = wccIdentifier;
    entities.WorkersCompAddress.TypeCode = typeCode;
    entities.WorkersCompAddress.StreetAddress = streetAddress;
    entities.WorkersCompAddress.City = city;
    entities.WorkersCompAddress.State = state;
    entities.WorkersCompAddress.ZipCode = zipCode;
    entities.WorkersCompAddress.CreatedBy = createdBy;
    entities.WorkersCompAddress.CreatedTimestamp = createdTimestamp;
    entities.WorkersCompAddress.LastUpdatedBy = "";
    entities.WorkersCompAddress.LastUpdatedTimestamp = null;
    entities.WorkersCompAddress.Populated = true;
  }

  private void CreateWorkersCompClaim()
  {
    var cspNumber = entities.Ncp.Number;
    var identifier = local.Max.Identifier;
    var claimantFirstName = local.WorkersCompClaim.ClaimantFirstName ?? "";
    var claimantMiddleName = local.WorkersCompClaim.ClaimantMiddleName ?? "";
    var claimantLastName = local.WorkersCompClaim.ClaimantLastName ?? "";
    var claimantAttorneyFirstName =
      local.WorkersCompClaim.ClaimantAttorneyFirstName ?? "";
    var claimantAttorneyLastName =
      local.WorkersCompClaim.ClaimantAttorneyLastName ?? "";
    var claimantAttorneyFirmName =
      local.WorkersCompClaim.ClaimantAttorneyFirmName ?? "";
    var employerName = local.WorkersCompClaim.EmployerName ?? "";
    var docketNumber = local.WorkersCompClaim.DocketNumber ?? "";
    var insurerName = local.WorkersCompClaim.InsurerName ?? "";
    var insurerAttorneyFirmName =
      local.WorkersCompClaim.InsurerAttorneyFirmName ?? "";
    var insurerContactName1 = local.WorkersCompClaim.InsurerContactName1 ?? "";
    var insurerContactName2 = local.WorkersCompClaim.InsurerContactName2 ?? "";
    var insurerContactPhone = local.WorkersCompClaim.InsurerContactPhone ?? "";
    var policyNumber = local.WorkersCompClaim.PolicyNumber ?? "";
    var dateOfLoss = local.WorkersCompClaim.DateOfLoss;
    var employerFein = local.WorkersCompClaim.EmployerFein ?? "";
    var dateOfAccident = local.WorkersCompClaim.DateOfAccident;
    var wageAmount = local.WorkersCompClaim.WageAmount ?? "";
    var accidentCity = local.WorkersCompClaim.AccidentCity ?? "";
    var accidentState = local.WorkersCompClaim.AccidentState ?? "";
    var accidentCounty = local.WorkersCompClaim.AccidentCounty ?? "";
    var severityCodeDescription =
      local.WorkersCompClaim.SeverityCodeDescription ?? "";
    var returnedToWorkDate = local.WorkersCompClaim.ReturnedToWorkDate;
    var compensationPaidFlag = local.WorkersCompClaim.CompensationPaidFlag ?? ""
      ;
    var compensationPaidDate = local.WorkersCompClaim.CompensationPaidDate;
    var weeklyRate = local.WorkersCompClaim.WeeklyRate ?? "";
    var dateOfDeath = local.WorkersCompClaim.DateOfDeath;
    var thirdPartyAdministratorName =
      local.WorkersCompClaim.ThirdPartyAdministratorName ?? "";
    var administrativeClaimNo =
      local.WorkersCompClaim.AdministrativeClaimNo ?? "";
    var claimFiledDate = local.WorkersCompClaim.ClaimFiledDate;
    var agencyClaimNo = local.WorkersCompClaim.AgencyClaimNo ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var accidentDescription = local.WorkersCompClaim.AccidentDescription ?? "";

    entities.WorkersCompClaim.Populated = false;
    Update("CreateWorkersCompClaim",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "cltFirstName", claimantFirstName);
        db.SetNullableString(command, "cltMiddleName", claimantMiddleName);
        db.SetNullableString(command, "cltLastName", claimantLastName);
        db.SetNullableString(
          command, "cltAttyFirstNm", claimantAttorneyFirstName);
        db.
          SetNullableString(command, "cltAttyLastNm", claimantAttorneyLastName);
          
        db.SetNullableString(
          command, "cltAttyFirmName", claimantAttorneyFirmName);
        db.SetNullableString(command, "employerName", employerName);
        db.SetNullableString(command, "docketNumber", docketNumber);
        db.SetNullableString(command, "insurerName", insurerName);
        db.SetNullableString(command, "insAttyFirmNm", insurerAttorneyFirmName);
        db.SetNullableString(command, "insContact1Nm", insurerContactName1);
        db.SetNullableString(command, "insContact2Nm", insurerContactName2);
        db.SetNullableString(command, "insContactPhone", insurerContactPhone);
        db.SetNullableString(command, "policyNo", policyNumber);
        db.SetNullableDate(command, "lossDate", dateOfLoss);
        db.SetNullableString(command, "employerFein", employerFein);
        db.SetNullableDate(command, "accidentDate", dateOfAccident);
        db.SetNullableString(command, "wageAmount", wageAmount);
        db.SetNullableString(command, "accidentCity", accidentCity);
        db.SetNullableString(command, "accidentState", accidentState);
        db.SetNullableString(command, "accidentCounty", accidentCounty);
        db.
          SetNullableString(command, "severityCdDesc", severityCodeDescription);
          
        db.SetNullableDate(command, "returnedWorkDt", returnedToWorkDate);
        db.SetNullableString(command, "compPaidFlag", compensationPaidFlag);
        db.SetNullableDate(command, "compPaidDate", compensationPaidDate);
        db.SetNullableString(command, "weeklyRate", weeklyRate);
        db.SetNullableDate(command, "dateOfDeath", dateOfDeath);
        db.SetNullableString(
          command, "thrdPtyAdminNm", thirdPartyAdministratorName);
        db.SetNullableString(command, "adminClaimNo", administrativeClaimNo);
        db.SetNullableDate(command, "claimFiledDate", claimFiledDate);
        db.SetNullableString(command, "agencyClaimNo", agencyClaimNo);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "accidentDesc", accidentDescription);
      });

    entities.WorkersCompClaim.CspNumber = cspNumber;
    entities.WorkersCompClaim.Identifier = identifier;
    entities.WorkersCompClaim.ClaimantFirstName = claimantFirstName;
    entities.WorkersCompClaim.ClaimantMiddleName = claimantMiddleName;
    entities.WorkersCompClaim.ClaimantLastName = claimantLastName;
    entities.WorkersCompClaim.ClaimantAttorneyFirstName =
      claimantAttorneyFirstName;
    entities.WorkersCompClaim.ClaimantAttorneyLastName =
      claimantAttorneyLastName;
    entities.WorkersCompClaim.ClaimantAttorneyFirmName =
      claimantAttorneyFirmName;
    entities.WorkersCompClaim.EmployerName = employerName;
    entities.WorkersCompClaim.DocketNumber = docketNumber;
    entities.WorkersCompClaim.InsurerName = insurerName;
    entities.WorkersCompClaim.InsurerAttorneyFirmName = insurerAttorneyFirmName;
    entities.WorkersCompClaim.InsurerContactName1 = insurerContactName1;
    entities.WorkersCompClaim.InsurerContactName2 = insurerContactName2;
    entities.WorkersCompClaim.InsurerContactPhone = insurerContactPhone;
    entities.WorkersCompClaim.PolicyNumber = policyNumber;
    entities.WorkersCompClaim.DateOfLoss = dateOfLoss;
    entities.WorkersCompClaim.EmployerFein = employerFein;
    entities.WorkersCompClaim.DateOfAccident = dateOfAccident;
    entities.WorkersCompClaim.WageAmount = wageAmount;
    entities.WorkersCompClaim.AccidentCity = accidentCity;
    entities.WorkersCompClaim.AccidentState = accidentState;
    entities.WorkersCompClaim.AccidentCounty = accidentCounty;
    entities.WorkersCompClaim.SeverityCodeDescription = severityCodeDescription;
    entities.WorkersCompClaim.ReturnedToWorkDate = returnedToWorkDate;
    entities.WorkersCompClaim.CompensationPaidFlag = compensationPaidFlag;
    entities.WorkersCompClaim.CompensationPaidDate = compensationPaidDate;
    entities.WorkersCompClaim.WeeklyRate = weeklyRate;
    entities.WorkersCompClaim.DateOfDeath = dateOfDeath;
    entities.WorkersCompClaim.ThirdPartyAdministratorName =
      thirdPartyAdministratorName;
    entities.WorkersCompClaim.AdministrativeClaimNo = administrativeClaimNo;
    entities.WorkersCompClaim.ClaimFiledDate = claimFiledDate;
    entities.WorkersCompClaim.AgencyClaimNo = agencyClaimNo;
    entities.WorkersCompClaim.CreatedBy = createdBy;
    entities.WorkersCompClaim.CreatedTimestamp = createdTimestamp;
    entities.WorkersCompClaim.LastUpdatedBy = "";
    entities.WorkersCompClaim.LastUpdatedTimestamp = null;
    entities.WorkersCompClaim.AccidentDescription = accidentDescription;
    entities.WorkersCompClaim.Populated = true;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Ncp.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.Ncp.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.
          SetString(command, "numb", local.WorkersCompClaimFileRecord.NcpNumber);
          
      },
      (db, reader) =>
      {
        entities.Ncp.Number = db.GetString(reader, 0);
        entities.Ncp.Populated = true;
      });
  }

  private bool ReadWorkersCompClaim()
  {
    local.Max.Populated = false;

    return Read("ReadWorkersCompClaim",
      null,
      (db, reader) =>
      {
        local.Max.Identifier = db.GetInt32(reader, 0);
        local.Max.Populated = true;
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
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public WorkersCompClaim Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of NullWorkersCompClaimFileRecord.
    /// </summary>
    [JsonPropertyName("nullWorkersCompClaimFileRecord")]
    public WorkersCompClaimFileRecord NullWorkersCompClaimFileRecord
    {
      get => nullWorkersCompClaimFileRecord ??= new();
      set => nullWorkersCompClaimFileRecord = value;
    }

    /// <summary>
    /// A value of WorkersCompClaimFileRecord.
    /// </summary>
    [JsonPropertyName("workersCompClaimFileRecord")]
    public WorkersCompClaimFileRecord WorkersCompClaimFileRecord
    {
      get => workersCompClaimFileRecord ??= new();
      set => workersCompClaimFileRecord = value;
    }

    /// <summary>
    /// A value of FieldDelimiter.
    /// </summary>
    [JsonPropertyName("fieldDelimiter")]
    public WorkArea FieldDelimiter
    {
      get => fieldDelimiter ??= new();
      set => fieldDelimiter = value;
    }

    /// <summary>
    /// A value of NullInfrastructure.
    /// </summary>
    [JsonPropertyName("nullInfrastructure")]
    public Infrastructure NullInfrastructure
    {
      get => nullInfrastructure ??= new();
      set => nullInfrastructure = value;
    }

    /// <summary>
    /// A value of NullWorkersCompClaim.
    /// </summary>
    [JsonPropertyName("nullWorkersCompClaim")]
    public WorkersCompClaim NullWorkersCompClaim
    {
      get => nullWorkersCompClaim ??= new();
      set => nullWorkersCompClaim = value;
    }

    /// <summary>
    /// A value of NullWorkersCompAddress.
    /// </summary>
    [JsonPropertyName("nullWorkersCompAddress")]
    public WorkersCompAddress NullWorkersCompAddress
    {
      get => nullWorkersCompAddress ??= new();
      set => nullWorkersCompAddress = value;
    }

    /// <summary>
    /// A value of InsuranceCarrier.
    /// </summary>
    [JsonPropertyName("insuranceCarrier")]
    public WorkersCompAddress InsuranceCarrier
    {
      get => insuranceCarrier ??= new();
      set => insuranceCarrier = value;
    }

    /// <summary>
    /// A value of WorkersCompAddress.
    /// </summary>
    [JsonPropertyName("workersCompAddress")]
    public WorkersCompAddress WorkersCompAddress
    {
      get => workersCompAddress ??= new();
      set => workersCompAddress = value;
    }

    /// <summary>
    /// A value of WorkersCompClaim.
    /// </summary>
    [JsonPropertyName("workersCompClaim")]
    public WorkersCompClaim WorkersCompClaim
    {
      get => workersCompClaim ??= new();
      set => workersCompClaim = value;
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
    /// A value of NewAddress.
    /// </summary>
    [JsonPropertyName("newAddress")]
    public Common NewAddress
    {
      get => newAddress ??= new();
      set => newAddress = value;
    }

    /// <summary>
    /// A value of RestartRecordNumber.
    /// </summary>
    [JsonPropertyName("restartRecordNumber")]
    public Common RestartRecordNumber
    {
      get => restartRecordNumber ??= new();
      set => restartRecordNumber = value;
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
    /// A value of RestartNcp.
    /// </summary>
    [JsonPropertyName("restartNcp")]
    public CsePerson RestartNcp
    {
      get => restartNcp ??= new();
      set => restartNcp = value;
    }

    /// <summary>
    /// A value of NumbClaimsSinceChckpnt.
    /// </summary>
    [JsonPropertyName("numbClaimsSinceChckpnt")]
    public Common NumbClaimsSinceChckpnt
    {
      get => numbClaimsSinceChckpnt ??= new();
      set => numbClaimsSinceChckpnt = value;
    }

    /// <summary>
    /// A value of TotalNumbOfClaimsRead.
    /// </summary>
    [JsonPropertyName("totalNumbOfClaimsRead")]
    public Common TotalNumbOfClaimsRead
    {
      get => totalNumbOfClaimsRead ??= new();
      set => totalNumbOfClaimsRead = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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

    private WorkersCompClaim max;
    private WorkersCompClaimFileRecord nullWorkersCompClaimFileRecord;
    private WorkersCompClaimFileRecord workersCompClaimFileRecord;
    private WorkArea fieldDelimiter;
    private Infrastructure nullInfrastructure;
    private WorkersCompClaim nullWorkersCompClaim;
    private WorkersCompAddress nullWorkersCompAddress;
    private WorkersCompAddress insuranceCarrier;
    private WorkersCompAddress workersCompAddress;
    private WorkersCompClaim workersCompClaim;
    private CsePerson csePerson;
    private Common newAddress;
    private Common restartRecordNumber;
    private NarrativeDetail narrativeDetail;
    private CsePerson restartNcp;
    private Common numbClaimsSinceChckpnt;
    private Common totalNumbOfClaimsRead;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of WorkersCompAddress.
    /// </summary>
    [JsonPropertyName("workersCompAddress")]
    public WorkersCompAddress WorkersCompAddress
    {
      get => workersCompAddress ??= new();
      set => workersCompAddress = value;
    }

    /// <summary>
    /// A value of WorkersCompClaim.
    /// </summary>
    [JsonPropertyName("workersCompClaim")]
    public WorkersCompClaim WorkersCompClaim
    {
      get => workersCompClaim ??= new();
      set => workersCompClaim = value;
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
    /// A value of Ncp.
    /// </summary>
    [JsonPropertyName("ncp")]
    public CsePerson Ncp
    {
      get => ncp ??= new();
      set => ncp = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private WorkersCompAddress workersCompAddress;
    private WorkersCompClaim workersCompClaim;
    private CsePersonAddress csePersonAddress;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson ncp;
    private CaseUnit caseUnit;
  }
#endregion
}
