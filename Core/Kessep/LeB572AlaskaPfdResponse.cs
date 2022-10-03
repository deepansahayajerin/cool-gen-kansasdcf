// Program: LE_B572_ALASKA_PFD_RESPONSE, ID: 1902534728, model: 746.
// Short name: SWEL572B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B572_ALASKA_PFD_RESPONSE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB572AlaskaPfdResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B572_ALASKA_PFD_RESPONSE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB572AlaskaPfdResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB572AlaskaPfdResponse.
  /// </summary>
  public LeB572AlaskaPfdResponse(IContext context, Import import, Export export):
    
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
    // 05/06/16  GVandy	CQ51956		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Business Rules
    // 	1. We must send KS obligor data by May 31st for matching to AKs PFD for
    // this years
    // 	   (2016) funds.
    // 		a. Must have an active NCP role.
    // 		b. Must have a Social Security Number
    // 	2. KS will receive a return match file the 2nd week of June.
    // 		a. Once we receive the match file back, workers determine which 
    // obligors
    // 		   qualify for PFDO program and begin sending requests to AK.
    // 			a. Must owe $50.00 in arrears
    // 			b. Do not send a PFDO transmittal if there is an open case
    // 			   between AK and KS, these cases will have an automatic interception
    // 			   of the AK funds.
    // 			c. Do not send a PFDO transmittal if Good Cause is set.
    // 		b. After the match all requests from KS must be received by AK no later
    // 		   than Monday, August 1st to ensure set up and collection of the PFD.
    // 		c. All submissions need to be submitted electronically by fax or email.
    // 		   Workers will prepare a transmittal #1.
    // 		d. AK will only honor withholding orders from AKs CSS Division, this 
    // is
    // 		   the only way States can collect AK funds.
    // 	3. A CSENet transaction must be sent prior to sending the required 
    // documents if
    // 	   KS is active with AK on CSENet.
    // 		a. Indicate on CSENet the transaction is for PFD only.
    // 		b. Send a CSENet closure if a CSENet open was sent to AK, do not use
    // 		   miscellaneous as a closure reason.
    // 	4. Required Documents:
    // 		a. CSE Transmittal #1 with the appropriate areas completed.
    // 		b. A copy of the signed order or judgment.
    // 		c. The direct phone number of the CS contact for KS.
    // 	5. A HIST and ALRT record will be created indicating the case(s) have a 
    // match with
    // 	   a AK Permanent Fund Dividend.
    // 	6. A narrative will be created on CSLN that will contain the information
    // from the
    // 	   results file.
    // 	7. An AK PFDM report will be created and be sorted by Contractor, Office
    // and Worker.
    // 	   The report will contain the information from the results file and the
    // following
    // 	   information:
    // 		a. Person Number
    // 		b. Case Number
    // 		c. Worker Name
    // 		d. Office Number
    // 	8. The report will be emailed to Deanne Dinkel with a cc to Ashley 
    // Dexter and
    // 	   Julie Heiman.
    // --------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -- The statement below is setting the view to the <tab> character.
    local.TabCharacter.Text1 = "\t";

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
      // 	001-010   Last NCP Number Processed
      // 	011-011   Blank
      // 	012-020   Total Number of NCPs Read from Response File
      // 	021-021   Blank
      // 	022-030   Total Number of Records Written to the Tab Delimited File
      // -------------------------------------------------------------------------------------
      local.RestartNcp.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.TotalNumbOfNcpsRead.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 12, 9));
      local.TotalNumbOfNcpsWritten.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 22, 9));

      // -------------------------------------------------------------------------------------
      // --  Extend the Output File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "EXTEND";
      UseLeB572WriteTabDelimitedFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error extending output file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -------------------------------------------------------------------------------------
      // --  Open the Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseLeB572ReadResponseFile2();

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
      do
      {
        local.EabFileHandling.Action = "READ";
        UseLeB572ReadResponseFile1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          if (Equal(local.EabFileHandling.Status, "EF"))
          {
            local.EabReportSend.RptDetail =
              "End of file encountered before finding restart NCP " + local
              .RestartNcp.Number;
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

        local.NcpCsePersonsWorkSet.Number =
          Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 15, 10);
      }
      while(!Equal(local.NcpCsePersonsWorkSet.Number, local.RestartNcp.Number));
    }
    else
    {
      local.TotalNumbOfNcpsRead.Count = 0;
      local.TotalNumbOfNcpsWritten.Count = 0;

      // -------------------------------------------------------------------------------------
      // --  Open the Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseLeB572ReadResponseFile2();

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

      // -------------------------------------------------------------------------------------
      // --  Open the Output File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseLeB572WriteTabDelimitedFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error opening output file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -------------------------------------------------------------------------------------
      // --  Write a header to the Output File.
      // -------------------------------------------------------------------------------------
    }

    local.NumbOfNcpsSinceChckpnt.Count = 0;

    // -------------------------------------------------------------------------------------
    // -- Read each match response record from the input file.
    // -------------------------------------------------------------------------------------
    do
    {
      // ---------------------------------------------------------------------------------------------------
      // MATCH RESPONSE FILE RECORD LAYOUT
      // 			Data
      // Data Element		Type	Length	Start	End	Note
      // ---------------------	-------	
      // ------	-----	---	
      // ----------------------------------
      // KS-FIPS State Code	Numeric	2	1	2	20
      // KS-FIPS County Code	Numeric	3	3	5	000
      // KS-NCP SSN		Numeric	9	6	14
      // KS-NCP Person Number	Text	15	15	29	Left justified w/ trailing blanks
      // KS-NCP Last Name	Text	20	30	49	Left justified w/ trailing blanks
      // KS-NCP First Name	Text	15	50	64	Left justified w/ trailing blanks
      // KS-NCP Date of Birth	Numeric	8	65	72	CCYYMMDD or 00000000
      // Filler			Text	8	73	80	Blanks
      // AK-NCP Last Name	Text	20	81	100	Left justified w/ trailing blanks
      // AK-NCP First Name	Text	12	101	112	Left justified w/ trailing blanks
      // AK-NCP Middle Initial	Text	1	113	113	Left justified w/ trailing blanks
      // AK-Address Line 1	Text	40	114	153	Left justified w/ trailing blanks
      // AK-Address Line 2	Text	40	154	193	Left justified w/ trailing blanks
      // AK-Address City		Text	30	194	223	Left justified w/ trailing blanks
      // AK-Address State	Text	2	224	225	Left justified w/ trailing blanks
      // AK-Address Zip		Text	9	226	234	Left justified w/ trailing blanks
      // AK-Date of Birth	Numeric	8	235	242	CCYYMMDD
      // AK-Birth State		Text	2	243	244
      // AK-Sex			Text	1	245	245	
      // ---------------------------------------------------------------------------------------------------
      ++local.NumbOfNcpsSinceChckpnt.Count;
      local.AlaskaPermanentFund.Assign(local.NullAlaskaPermanentFund);

      // -------------------------------------------------------------------------------------
      // -- Read match response record from the input file.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "READ";
      UseLeB572ReadResponseFile1();

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

      ++local.TotalNumbOfNcpsRead.Count;

      // -------------------------------------------------------------------------------------
      // -- Extract attributes from the match response file record.
      // -------------------------------------------------------------------------------------
      local.AlaskaPermanentFund.KsFipsStateCode =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 1, 2);
      local.AlaskaPermanentFund.KsFipsCountyCode =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 3, 3);
      local.AlaskaPermanentFund.KsNcpSsn =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 6, 9);
      local.AlaskaPermanentFund.KsNcpPersonNumber =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 15, 15);
      local.AlaskaPermanentFund.KsNcpLastName =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 30, 20);
      local.AlaskaPermanentFund.KsNcpFirstName =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 50, 15);
      local.AlaskaPermanentFund.KsNcpDateOfBirth =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 65, 8);
      local.AlaskaPermanentFund.AkNcpLastName =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 81, 20);
      local.AlaskaPermanentFund.AkNcpFirstName =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 101, 12);
      local.AlaskaPermanentFund.AkNcpMiddleInitial =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 113, 1);
      local.AlaskaPermanentFund.AkAddressLine1 =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 114, 40);
      local.AlaskaPermanentFund.AkAddressLine2 =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 154, 40);
      local.AlaskaPermanentFund.AkAddressCity =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 194, 30);
      local.AlaskaPermanentFund.AkAddressState =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 224, 2);
      local.AlaskaPermanentFund.AkAddressZip =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 226, 9);
      local.AlaskaPermanentFund.AkDateOfBirth =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 235, 8);
      local.AlaskaPermanentFund.AkBirthState =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 243, 2);
      local.AlaskaPermanentFund.AkSex =
        Substring(local.AlaskaPermanentFund.IncomingResponseRecord, 245, 1);

      if (!ReadCsePerson())
      {
        local.EabReportSend.RptDetail =
          "NCP Not Found.  Returned NCP Number " + local
          .AlaskaPermanentFund.KsNcpPersonNumber;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        continue;
      }

      // -------------------------------------------------------------------------------------
      // -- Raise Event to create HIST record and alert.
      // -------------------------------------------------------------------------------------
      foreach(var item in ReadCase())
      {
        local.Infrastructure.EventId = 10;
        local.Infrastructure.ReasonCode = "AKPFDMATCHRCVD";
        local.Infrastructure.Detail =
          "A match was returned from the Alaska Permanent Fund.  See CSLN for details.";
          
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
          .NarrativeDetail.LineNumber <= 9; ++local.NarrativeDetail.LineNumber)
        {
          switch(local.NarrativeDetail.LineNumber)
          {
            case 1:
              local.NarrativeDetail.NarrativeText =
                "The following infomation was returned by Alaska for NCP " + entities
                .Ncp.Number + ":";

              break;
            case 2:
              local.NarrativeDetail.NarrativeText = "";

              break;
            case 3:
              local.NarrativeDetail.NarrativeText = "  Name: " + TrimEnd
                (local.AlaskaPermanentFund.AkNcpFirstName) + " " + TrimEnd
                (local.AlaskaPermanentFund.AkNcpMiddleInitial) + " " + TrimEnd
                (local.AlaskaPermanentFund.AkNcpLastName) + "";

              break;
            case 4:
              local.NarrativeDetail.NarrativeText = "  DOB: " + TrimEnd
                (local.AlaskaPermanentFund.AkDateOfBirth) + "  Birth State: " +
                local.AlaskaPermanentFund.AkBirthState + "  Sex: " + local
                .AlaskaPermanentFund.AkSex;

              break;
            case 5:
              local.NarrativeDetail.NarrativeText = "  Address: " + TrimEnd
                (local.AlaskaPermanentFund.AkAddressLine1);

              break;
            case 6:
              local.NarrativeDetail.NarrativeText = "           " + TrimEnd
                (local.AlaskaPermanentFund.AkAddressLine2);

              break;
            case 7:
              local.NarrativeDetail.NarrativeText = "           " + TrimEnd
                (local.AlaskaPermanentFund.AkAddressCity) + ", " + local
                .AlaskaPermanentFund.AkAddressState + " " + local
                .AlaskaPermanentFund.AkAddressZip;

              break;
            case 8:
              local.NarrativeDetail.NarrativeText = "";

              break;
            case 9:
              local.NarrativeDetail.NarrativeText =
                "Please see Alaska Permanent Fund Process.";

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

        // -- Find the service provider and office where the case resides.
        if (ReadServiceProviderOffice())
        {
          MoveServiceProvider(entities.ServiceProvider, local.ServiceProvider);
          local.OfficeId.Text4 =
            NumberToString(entities.Office.SystemGeneratedId, 12, 4);
        }
        else
        {
          MoveServiceProvider(local.NullServiceProvider, local.ServiceProvider);
          local.OfficeId.Text4 = "";
          local.Contractor.Name = "";
        }

        if (entities.Office.Populated)
        {
          // -- Find the Judicial District in which the office resides.
          if (!ReadCseOrganization2())
          {
            local.Contractor.Name = "";

            goto Test1;
          }

          // -- Determine the contractor responsible for the Judicial District.
          if (ReadCseOrganization1())
          {
            local.Contractor.Name = entities.Contractor.Name;
          }
          else
          {
            local.Contractor.Name = "";
          }
        }

Test1:

        // ---------------------------------------------------------------------------------------------------
        // MATCH RESPONSE TAB DELIMITED OUTPUT FILE RECORD LAYOUT
        // 			Data
        // Data Element		Type	Max Length	Note
        // --------------		-------	----------	
        // -------------------------------------
        // Contractor Name		Text		20
        // Office Number		Text		4
        // Worker Name		Text		31	Last, First
        // Case Number		Text		10
        // KS-FIPS State Code	Numeric		2	20
        // KS-FIPS County Code	Numeric		3	000
        // KS-NCP SSN		Numeric		9
        // KS-NCP Person Number	Text		15
        // KS-NCP Last Name	Text		20
        // KS-NCP First Name	Text		15
        // KS-NCP Date of Birth	Numeric		8	CCYYMMDD or 00000000
        // AK-NCP Last Name	Text		20
        // AK-NCP First Name	Text		12
        // AK-NCP Middle Initial	Text		1
        // AK-Address Line 1	Text		40
        // AK-Address Line 2	Text		40
        // AK-Address City		Text		30
        // AK-Address State	Text		2
        // AK-Address Zip		Text		9
        // AK-Date of Birth	Numeric		8	CCYYMMDD
        // AK-Birth State		Text		2
        // AK-Sex			Text		1
        // ---------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------
        // -- Format the Tab delimited output record.
        // -------------------------------------------------------------------------------------
        local.AlaskaPermanentFund.TabDelimitedRecord =
          Spaces(AlaskaPermanentFund.TabDelimitedRecord_MaxLength);
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.Contractor.Name) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.OfficeId.Text4) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.ServiceProvider.LastName) + ", " + TrimEnd
          (local.ServiceProvider.FirstName) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (entities.Case1.Number) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.KsFipsStateCode) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.KsFipsCountyCode) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.KsNcpSsn) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.KsNcpPersonNumber) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.KsNcpLastName) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.KsNcpFirstName) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.KsNcpDateOfBirth) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkNcpLastName) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkNcpFirstName) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkNcpMiddleInitial) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkAddressLine1) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkAddressLine2) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkAddressCity) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkAddressState) + local
          .TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkAddressZip) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkDateOfBirth) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkBirthState) + local.TabCharacter.Text1;
        local.AlaskaPermanentFund.TabDelimitedRecord =
          TrimEnd(local.AlaskaPermanentFund.TabDelimitedRecord) + TrimEnd
          (local.AlaskaPermanentFund.AkSex) + local.TabCharacter.Text1;

        // -------------------------------------------------------------------------------------
        // -- Write to the Tab delimited output file.
        // -------------------------------------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        UseLeB572WriteTabDelimitedFile1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing tab delimited output file.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
        }

        ++local.TotalNumbOfNcpsWritten.Count;
      }

      // -- Commit processing.
      if (local.NumbOfNcpsSinceChckpnt.Count > local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -- Checkpoint.
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	Position  Description
        // 	--------  
        // ---------------------------------------------------------
        // 	001-010   Last NCP Number Processed
        // 	011-011   Blank
        // 	012-020   Total Number of NCPs Read from Response File
        // 	021-021   Blank
        // 	022-030   Total Number of Records Written to the Tab Delimited File
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = entities.Ncp.Number + " " +
          NumberToString(local.TotalNumbOfNcpsRead.Count, 7, 9) + " " + NumberToString
          (local.TotalNumbOfNcpsWritten.Count, 7, 9);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error committing.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        // -------------------------------------------------------------------------------------
        // --  Commit to the Output File.
        // -------------------------------------------------------------------------------------
        local.EabFileHandling.Action = "COMMIT";
        UseLeB572WriteTabDelimitedFile2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error commiting output file.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.NumbOfNcpsSinceChckpnt.Count = 0;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -------------------------------------------------------------------------------------
      // --  Write Totals to the Control Report.
      // -------------------------------------------------------------------------------------
      for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Number of NCPs Read from Alaska Permanent Fund Response File.........." +
              NumberToString(local.TotalNumbOfNcpsRead.Count, 9, 7);

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "Number of Records Written to Tab Delimited Output File................." +
              NumberToString(local.TotalNumbOfNcpsWritten.Count, 9, 7);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
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

          goto Test2;
        }
      }

      // -------------------------------------------------------------------------------------
      // --  Do a final Commit to the Output File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "COMMIT";
      UseLeB572WriteTabDelimitedFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error on final COMMIT for output file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
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

Test2:

    // -------------------------------------------------------------------------------------
    // --  Close the Input File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeB572ReadResponseFile2();

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
    // --  Close the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeB572WriteTabDelimitedFile2();

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

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
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

  private void UseLeB572ReadResponseFile1()
  {
    var useImport = new LeB572ReadResponseFile.Import();
    var useExport = new LeB572ReadResponseFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.AlaskaPermanentFund.IncomingResponseRecord =
      local.AlaskaPermanentFund.IncomingResponseRecord;

    Call(LeB572ReadResponseFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.AlaskaPermanentFund.IncomingResponseRecord =
      useExport.AlaskaPermanentFund.IncomingResponseRecord;
  }

  private void UseLeB572ReadResponseFile2()
  {
    var useImport = new LeB572ReadResponseFile.Import();
    var useExport = new LeB572ReadResponseFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB572ReadResponseFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB572WriteTabDelimitedFile1()
  {
    var useImport = new LeB572WriteTabDelimitedFile.Import();
    var useExport = new LeB572WriteTabDelimitedFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.AlaskaPermanentFund.TabDelimitedRecord =
      local.AlaskaPermanentFund.TabDelimitedRecord;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB572WriteTabDelimitedFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB572WriteTabDelimitedFile2()
  {
    var useImport = new LeB572WriteTabDelimitedFile.Import();
    var useExport = new LeB572WriteTabDelimitedFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB572WriteTabDelimitedFile.Execute, useImport, useExport);

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

  private bool ReadCseOrganization1()
  {
    entities.Contractor.Populated = false;

    return Read("ReadCseOrganization1",
      (db, command) =>
      {
        db.SetString(command, "cogParentType", entities.Jd.Type1);
        db.SetString(command, "cogParentCode", entities.Jd.Code);
      },
      (db, reader) =>
      {
        entities.Contractor.Code = db.GetString(reader, 0);
        entities.Contractor.Type1 = db.GetString(reader, 1);
        entities.Contractor.Name = db.GetString(reader, 2);
        entities.Contractor.Populated = true;
      });
  }

  private bool ReadCseOrganization2()
  {
    System.Diagnostics.Debug.Assert(entities.Office.Populated);
    entities.Jd.Populated = false;

    return Read("ReadCseOrganization2",
      (db, command) =>
      {
        db.
          SetString(command, "cogParentType", entities.Office.CogTypeCode ?? "");
          
        db.SetString(command, "cogParentCode", entities.Office.CogCode ?? "");
      },
      (db, reader) =>
      {
        entities.Jd.Code = db.GetString(reader, 0);
        entities.Jd.Type1 = db.GetString(reader, 1);
        entities.Jd.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.Ncp.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "ksNcpPersonNumber",
          local.AlaskaPermanentFund.KsNcpPersonNumber);
      },
      (db, reader) =>
      {
        entities.Ncp.Number = db.GetString(reader, 0);
        entities.Ncp.Populated = true;
      });
  }

  private bool ReadServiceProviderOffice()
  {
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadServiceProviderOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 4);
        entities.Office.CogCode = db.GetNullableString(reader, 5);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.ServiceProvider.Populated = true;
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
    /// A value of Contractor.
    /// </summary>
    [JsonPropertyName("contractor")]
    public CseOrganization Contractor
    {
      get => contractor ??= new();
      set => contractor = value;
    }

    /// <summary>
    /// A value of OfficeId.
    /// </summary>
    [JsonPropertyName("officeId")]
    public WorkArea OfficeId
    {
      get => officeId ??= new();
      set => officeId = value;
    }

    /// <summary>
    /// A value of TabCharacter.
    /// </summary>
    [JsonPropertyName("tabCharacter")]
    public WorkArea TabCharacter
    {
      get => tabCharacter ??= new();
      set => tabCharacter = value;
    }

    /// <summary>
    /// A value of NullServiceProvider.
    /// </summary>
    [JsonPropertyName("nullServiceProvider")]
    public ServiceProvider NullServiceProvider
    {
      get => nullServiceProvider ??= new();
      set => nullServiceProvider = value;
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
    /// A value of NarrativeDetail.
    /// </summary>
    [JsonPropertyName("narrativeDetail")]
    public NarrativeDetail NarrativeDetail
    {
      get => narrativeDetail ??= new();
      set => narrativeDetail = value;
    }

    /// <summary>
    /// A value of NullAlaskaPermanentFund.
    /// </summary>
    [JsonPropertyName("nullAlaskaPermanentFund")]
    public AlaskaPermanentFund NullAlaskaPermanentFund
    {
      get => nullAlaskaPermanentFund ??= new();
      set => nullAlaskaPermanentFund = value;
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
    /// A value of NcpCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("ncpCsePersonsWorkSet")]
    public CsePersonsWorkSet NcpCsePersonsWorkSet
    {
      get => ncpCsePersonsWorkSet ??= new();
      set => ncpCsePersonsWorkSet = value;
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
    /// A value of AlaskaPermanentFund.
    /// </summary>
    [JsonPropertyName("alaskaPermanentFund")]
    public AlaskaPermanentFund AlaskaPermanentFund
    {
      get => alaskaPermanentFund ??= new();
      set => alaskaPermanentFund = value;
    }

    /// <summary>
    /// A value of MatchFile.
    /// </summary>
    [JsonPropertyName("matchFile")]
    public WorkArea MatchFile
    {
      get => matchFile ??= new();
      set => matchFile = value;
    }

    /// <summary>
    /// A value of TotalNumbOfNcpsWritten.
    /// </summary>
    [JsonPropertyName("totalNumbOfNcpsWritten")]
    public Common TotalNumbOfNcpsWritten
    {
      get => totalNumbOfNcpsWritten ??= new();
      set => totalNumbOfNcpsWritten = value;
    }

    /// <summary>
    /// A value of NumbOfNcpsSinceChckpnt.
    /// </summary>
    [JsonPropertyName("numbOfNcpsSinceChckpnt")]
    public Common NumbOfNcpsSinceChckpnt
    {
      get => numbOfNcpsSinceChckpnt ??= new();
      set => numbOfNcpsSinceChckpnt = value;
    }

    /// <summary>
    /// A value of TotalNumbOfNcpsRead.
    /// </summary>
    [JsonPropertyName("totalNumbOfNcpsRead")]
    public Common TotalNumbOfNcpsRead
    {
      get => totalNumbOfNcpsRead ??= new();
      set => totalNumbOfNcpsRead = value;
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
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public TextWorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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

    private CseOrganization contractor;
    private WorkArea officeId;
    private WorkArea tabCharacter;
    private ServiceProvider nullServiceProvider;
    private ServiceProvider serviceProvider;
    private NarrativeDetail narrativeDetail;
    private AlaskaPermanentFund nullAlaskaPermanentFund;
    private CsePerson restartNcp;
    private CsePersonsWorkSet ncpCsePersonsWorkSet;
    private CsePersonAddress ncpCsePersonAddress;
    private AlaskaPermanentFund alaskaPermanentFund;
    private WorkArea matchFile;
    private Common totalNumbOfNcpsWritten;
    private Common numbOfNcpsSinceChckpnt;
    private Common totalNumbOfNcpsRead;
    private ProgramCheckpointRestart programCheckpointRestart;
    private TextWorkArea textDate;
    private DateWorkArea nullDateWorkArea;
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
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public CseOrganization County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
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

    /// <summary>
    /// A value of Jd.
    /// </summary>
    [JsonPropertyName("jd")]
    public CseOrganization Jd
    {
      get => jd ??= new();
      set => jd = value;
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

    private CseOrganization county;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private CseOrganization contractor;
    private CseOrganization jd;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson ncp;
    private CaseUnit caseUnit;
  }
#endregion
}
