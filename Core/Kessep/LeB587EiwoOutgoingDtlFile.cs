// Program: LE_B587_EIWO_OUTGOING_DTL_FILE, ID: 1902479365, model: 746.
// Short name: SWEL587B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B587_EIWO_OUTGOING_DTL_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB587EiwoOutgoingDtlFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B587_EIWO_OUTGOING_DTL_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB587EiwoOutgoingDtlFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB587EiwoOutgoingDtlFile.
  /// </summary>
  public LeB587EiwoOutgoingDtlFile(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 06/09/15  GVandy	CQ22212		Initial Code.
    // 10/19/16  GVandy	CQ54541		Add new document field LACTKPCNUM to replace
    // 					LACTSTDNUM.
    // 04/06/18  GVandy	CQ61775		Add new OCSE field edits.
    // 08/31/18  GVandy	CQ63304		Update Agency Official Name and Title
    // 10/21/18  GVandy	CQ64924		Batch header Created Date & Time was not
    // 					correctly reset on restarts.
    // -------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Overview
    // A new batch job will be developed to facilitate the generation of 
    // recurring outgoing e-IWO files that will be picked up by OCSE.  A new
    // file will be generated nightly and will contain all new e-IWO submittals
    // to be evaluated by the federal Portal and sent to employers for
    // processing.
    // Business Rules
    // 1. Find each record in the e-IWO Database with a corresponding document 
    // in "D" (ready for
    //    portal) status.
    // 2. For each record, retrieve all of the field values for inclusion in the
    // e-IWO Detail
    //    File (see appendix C for file layout).
    // 3. When all field values are retrieved successfully, the record will be 
    // written to the
    //    e-IWO Detail File.
    // 	a. The LAIS record will be placed in N (sent to portal) status, and 
    // the
    // 	   corresponding document will be placed in E (processed to portal) 
    // status.
    // 	b. An e-IWO Sent to Portal event will be created that will display on 
    // EIWH.
    // 4. If any field values cannot be retrieved, do not write the record to 
    // the e-IWO Detail
    //    File.
    // 	a. The LAIS record will be placed in E (error) status, and the
    // 	   corresponding document will be placed in N status.
    // 	b. An e-IWO Failed Document Field Retrieval event will be created that
    // will
    // 	   display on EIWH.
    // (Note:  Business Rule 5 is handled in procedure step 
    // le_b584_eiwo_error_report)
    // 5. After all records with a corresponding document in "D" status have 
    // been processed, find
    //    each e-IWO record in "E" (error) status with Red severity and write 
    // them to an error
    //    report.
    // 	a. For each record in the error report, include:
    // 		i.   Transaction Number- The e-IWO transaction number
    // 		ii.  Office- current legal action service provider office
    // 		iii. Worker- current legal action service provider
    // 		iv.  Legal Action- legal action taken tied to the transaction (IWO,
    // 		     IWOTERM, etc)
    // 		v.   Error Event- e-IWO Failed Document Field Retrieval or e-IWO
    // 		     Portal Error
    // 		vi.  Error Event Date- The date the transaction went into E status
    // 	b. Sort the report by Error Event date (descending), then by Transaction
    // Number.
    // --------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

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
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage1();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

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
      // -----------------------------------------
      // 	001-022   File Control Number (22)
      // 	023-023   Blank
      // 	024-028   Batch Count (5)
      // 	029-029   Blank
      // 	030-051   Batch Control Number (22)
      // 	052-052   Blank
      // 	053-057   Record Count (5)
      // 	058-058   Blank
      // 	059-067   Last EIN Processed (9)
      // 	068-068   Blank
      // 	069-078   Last NCP Number Processed (10)
      // 	079-079   Blank
      // 	080-088   Total Number of EINs Written to File (9)
      // 	089-089   Blank
      // 	090-098   Total Number of NCPs Written to File (9)
      // -------------------------------------------------------------------------------------
      local.FileEiwoB587HeaderRecord.ControlNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 22);

      // --10/21/18 GVandy  CQ64924  Batch header Created Date & Time was not 
      // correctly reset on restarts.
      local.FileEiwoB587HeaderRecord.CreationDate = "20" + Substring
        (local.ProgramCheckpointRestart.RestartInfo, 250, 6, 6);
      local.FileEiwoB587HeaderRecord.CreationTime =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 12, 6);
      local.FileEiwoB587TrailerRecord.BatchCount =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 24, 5);
      local.BatchCount.Count =
        (int)StringToNumber(local.FileEiwoB587TrailerRecord.BatchCount);
      local.BatchEiwoB587HeaderRecord.ControlNumber =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 30, 22);
      local.BatchEiwoB587TrailerRecord.RecordCount =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 53, 5);
      local.RecordCount.Count =
        (int)StringToNumber(local.BatchEiwoB587TrailerRecord.RecordCount);
      local.RestartEmployer.Ein =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 59, 9);
      local.PreviousEmployer.Ein = local.RestartEmployer.Ein ?? "";
      local.RestartNcp.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 69, 10);
      local.TotalNumbOfEinsWritten.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 80, 9));
      local.TotalNumbOfNcpsWritten.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 90, 9));

      // -- Log restart data to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 11; ++
        local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "Program is restarting...";

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "    File Control Number..............." + local
              .FileEiwoB587HeaderRecord.ControlNumber;

            break;
          case 3:
            local.EabReportSend.RptDetail =
              "    Previously Written Batch Count...." + local
              .FileEiwoB587TrailerRecord.BatchCount;

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "    Restarting at Batch Control Number " + local
              .BatchEiwoB587HeaderRecord.ControlNumber + " at Record " + local
              .BatchEiwoB587TrailerRecord.RecordCount;

            break;
          case 6:
            local.EabReportSend.RptDetail =
              "    Restarting at EIN number.........." + (
                local.RestartEmployer.Ein ?? "");

            break;
          case 7:
            local.EabReportSend.RptDetail =
              "    Restarting at NCP person number..." + local
              .RestartNcp.Number;

            break;
          case 8:
            local.EabReportSend.RptDetail =
              "    EINs Previously Processed.........." + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 22, 9);

            break;
          case 9:
            local.EabReportSend.RptDetail =
              "    NCPs Previously Processed.........." + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 32, 9);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error Writing Restart Info to the Control Report...  Returned Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // @@@ Modify the external to return the batch and record counts on a 
      // restart.
      // -------------------------------------------------------------------------------------
      // --  Extend the Output File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "EXTEND";
      UseLeB587WriteFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error extending output file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // @@@ Uncomment below once batch and record counts are returned from the 
      // external.
    }
    else
    {
      local.FileEiwoB587HeaderRecord.ControlNumber = "";
      local.FileEiwoB587TrailerRecord.BatchCount = "";
      local.BatchEiwoB587HeaderRecord.ControlNumber = "";
      local.BatchEiwoB587TrailerRecord.RecordCount = "";
      local.RestartNcp.Number = "";
      local.RestartEmployer.Ein = "";
      local.TotalNumbOfEinsWritten.Count = 0;
      local.TotalNumbOfNcpsWritten.Count = 0;

      // -------------------------------------------------------------------------------------
      // --  Open the Output File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseLeB587WriteFile7();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error opening output file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -------------------------------------------------------------------------------------
      // -- Write File Header (FHI) record.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "HEADER";
      local.FileEiwoB587HeaderRecord.DocumentCode = "FHI";

      // -- Create file control number in Feds recommended format 
      // 20000YYMMDDHHMMSS0000
      local.BatchTimestampWorkArea.IefTimestamp = Now();
      UseLeCabConvertTimestamp();
      local.FileEiwoB587HeaderRecord.ControlNumber = "20000" + Substring
        (local.BatchTimestampWorkArea.TextDateYyyy,
        BatchTimestampWorkArea.TextDateYyyy_MaxLength, 3, 2) + local
        .BatchTimestampWorkArea.TextDateMm + local
        .BatchTimestampWorkArea.TestDateDd + local
        .BatchTimestampWorkArea.TextTime + "0000";
      local.FileEiwoB587HeaderRecord.StateFipsCode = "20000";
      local.FileEiwoB587HeaderRecord.Ein = "";
      local.FileEiwoB587HeaderRecord.PrimaryEin = "";

      // -- Set creation date and time below.  (formats CCYYMMDD and HHMMSS)
      local.FileEiwoB587HeaderRecord.CreationDate =
        local.BatchTimestampWorkArea.TextDateYyyy + local
        .BatchTimestampWorkArea.TextDateMm + local
        .BatchTimestampWorkArea.TestDateDd;
      local.FileEiwoB587HeaderRecord.CreationTime =
        local.BatchTimestampWorkArea.TextTime;
      local.FileEiwoB587HeaderRecord.ErrorFieldName = "";
      UseLeB587WriteFile6();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing file header record.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Log info to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Program is starting from the beginning.";

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "    File Control Number..............." + local
              .FileEiwoB587HeaderRecord.ControlNumber;

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error Writing Restart Info to the Control Report...  Returned Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    local.Zero.Text11 = "00000000000";

    // -- Set default values in the detail record.  In some cases the values 
    // will be changed in logic
    // -- below that retrieves the field values.  But if no field value is 
    // retrieved then at least
    // -- the default value will be written to the file.
    local.Initialized.DocumentCode = "DTL";
    local.Initialized.DocumentActionCode = "";
    local.Initialized.DocumentDate = NumberToString(Now().Date.Year, 12, 4) + NumberToString
      (Now().Date.Month, 14, 2) + NumberToString(Now().Date.Day, 14, 2);
    local.Initialized.SupportCurrentChildAmount = local.Zero.Text11;
    local.Initialized.SupportCurrentChildFrequency = "";
    local.Initialized.SupportPastDueChildAmount = local.Zero.Text11;
    local.Initialized.SupportPastDueChildFrequency = "";
    local.Initialized.SupportCurrentMedicalAmount = local.Zero.Text11;
    local.Initialized.SupportCurrentMedicalFrequenc = "";
    local.Initialized.SupportPastDueMedicalAmount = local.Zero.Text11;
    local.Initialized.SupportPastDueMedicalFrequen = "";
    local.Initialized.SupportCurrentSpousalAmount = local.Zero.Text11;
    local.Initialized.SupportCurrentSpousalFrequenc = "";
    local.Initialized.SupportPastDueSpousalAmount = local.Zero.Text11;
    local.Initialized.SupportPastDueSpousalFrequen = "";
    local.Initialized.ObligationOtherAmount = local.Zero.Text11;
    local.Initialized.ObligationOtherFrequencyCode = "";
    local.Initialized.ObligationOtherDescription = "";
    local.Initialized.ObligationTotalAmount = local.Zero.Text11;
    local.Initialized.ObligationTotalFrequency = "";
    local.Initialized.Arrears12WkOverdueCode = "N";
    local.Initialized.IwoDeductionWeeklyAmount = local.Zero.Text11;
    local.Initialized.IwoDeductionBiweeklyAmount = local.Zero.Text11;
    local.Initialized.IwoDeductionSemimonthlyAmount = local.Zero.Text11;
    local.Initialized.IwoDeductionMonthlyAmount = local.Zero.Text11;
    local.Initialized.LumpSumPaymentAmount = local.Zero.Text11;
    local.Initialized.StateTribeTerritoryName = "KANSAS";
    local.Initialized.BeginWithholdingWithinDays = "14";
    local.Initialized.SendPaymentWithhinDays = "7";
    local.Initialized.IwoCcpaPercentRate = "50";
    local.Initialized.PayeeRemittanceFipsCode = "2000003";

    // 08/31/18 GVandy  CQ63304  Update Agency Official Name and Title
    local.Initialized.GovernmentOfficialName = "Elizabeth R. Cohn";
    local.Initialized.IssuingOfficialTitle = "Director, Child Support Services";
    local.Initialized.SendEmployeeCopyIndicator = "N";
    local.Initialized.PenaltyLiabilityInfoText =
      "By law, you may be ordered to pay three times the amount withheld and not submitted plus reasonable attorney fees for failure to pay funds in.";
      
    local.Initialized.AntidiscriminationProvisionTxt =
      "Under Kansas law, the civil penalty for such actions shall not exceed $500 and such other equitable relief as the court considers proper.";
      
    local.Initialized.SpecificPayeeWithholdingLimit =
      "See K.S.A. 23-3104(e) for details on administrative fees you may impose.  See K.S.A. 23-3104(f) for details on KS 50% disposable income withholding limitation.";
      

    // -- Read Employee and Employer contact info from code table.
    if (ReadCode())
    {
      local.Common.Count = 0;

      foreach(var item in ReadCodeValue())
      {
        switch(TrimEnd(entities.CodeValue.Cdvalue))
        {
          case "ESCN":
            local.Initialized.EmployeeStateContactName =
              entities.CodeValue.Description;

            break;
          case "ESCPN":
            local.Initialized.EmployeeStateContactPhone =
              entities.CodeValue.Description;

            break;
          case "ESCFN":
            local.Initialized.EmployeeStateContactFax =
              entities.CodeValue.Description;

            break;
          case "ESCEA":
            local.Initialized.EmployeeStateContactEmail =
              entities.CodeValue.Description;

            break;
          case "RSCN":
            local.Initialized.EmployerContactName =
              entities.CodeValue.Description;

            break;
          case "RSCAL1":
            local.Initialized.EmployerContactAddressLine1 =
              entities.CodeValue.Description;

            break;
          case "RSCAL2":
            local.Initialized.EmployerContactAddressLine2 =
              entities.CodeValue.Description;

            break;
          case "RSCAC":
            local.Initialized.EmployerContactAddressCity =
              entities.CodeValue.Description;

            break;
          case "RSCAS":
            local.Initialized.EmployerContactAddressState =
              entities.CodeValue.Description;

            break;
          case "RSCAZ":
            local.Initialized.EmployerContactAddressZip =
              entities.CodeValue.Description;

            break;
          case "RSCAZE":
            local.Initialized.EmployerContactAddressExtZip =
              entities.CodeValue.Description;

            break;
          case "RSCPN":
            local.Initialized.EmployerContactPhone =
              entities.CodeValue.Description;

            break;
          case "RSCFN":
            local.Initialized.EmployerContactFax =
              entities.CodeValue.Description;

            break;
          case "RSCEA":
            local.Initialized.EmployerContactEmail =
              entities.CodeValue.Description;

            break;
          default:
            continue;
        }

        ++local.Common.Count;
      }

      if (local.Common.Count < 12)
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "12 \"EIWO CALL CENTER\" code table values expected.  Only " + NumberToString
          (local.Common.Count, 14, 2) + " values found.";
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "\"EIWO CALL CENTER\" code table not found." + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Current.Timestamp = Now();

    if (ReadAlert1())
    {
      local.Monitored.TypeCode = "AUT";
      local.Monitored.Description = entities.Alert.Description;
      local.Monitored.Message = entities.Alert.Message;
      local.Monitored.OptimizationInd = "N";
      local.Monitored.OptimizedFlag = "N";
      local.Monitored.LastUpdatedBy = local.ProgramProcessingInfo.Name;
      local.Monitored.DistributionDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.Monitored.PrioritizationCode = 1;
      local.Monitored.LastUpdatedTimestamp = local.Current.Timestamp;
      local.Monitored.SituationIdentifier = "1";
    }
    else
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Alert not found for Control Number 396.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (ReadAlert2())
    {
      local.UnMonitored.TypeCode = "AUT";
      local.UnMonitored.Description = entities.Alert.Description;
      local.UnMonitored.Message = entities.Alert.Message;
      local.UnMonitored.OptimizationInd = "N";
      local.UnMonitored.OptimizedFlag = "N";
      local.UnMonitored.LastUpdatedBy = local.ProgramProcessingInfo.Name;
      local.UnMonitored.DistributionDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.UnMonitored.PrioritizationCode = 1;
      local.UnMonitored.LastUpdatedTimestamp = local.Current.Timestamp;
      local.UnMonitored.SituationIdentifier = "1";
    }
    else
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Alert not found for Control Number 442.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (ReadAlert3())
    {
      local.Automatic.TypeCode = "AUT";
      local.Automatic.Description = entities.Alert.Description;
      local.Automatic.Message = entities.Alert.Message;
      local.Automatic.OptimizationInd = "N";
      local.Automatic.OptimizedFlag = "N";
      local.Automatic.LastUpdatedBy = local.ProgramProcessingInfo.Name;
      local.Automatic.DistributionDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.Automatic.PrioritizationCode = 1;
      local.Automatic.LastUpdatedTimestamp = local.Current.Timestamp;
      local.Automatic.SituationIdentifier = "1";
    }
    else
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Alert not found for Control Number 447.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.NumbOfNcpsSinceChckpnt.Count = 0;

    // -------------------------------------------------------------------------------------
    // -- Find each eIWO in 'S' status with a corresponding document in 'D' 
    // status.
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadIwoActionOutgoingDocumentEmployerCsePerson())
    {
      ExitState = "ACO_NN0000_ALL_OK";
      local.IwoAction.Assign(local.NullIwoAction);
      local.Document.Assign(local.NullDocument);
      local.Infrastructure.Assign(local.NullInfrastructure);
      local.Arnmorgz.ObligeeLastName = "";
      local.EabReportSend.RptDetail = "";
      local.Ch2Found.Flag = "N";
      local.Ch3Found.Flag = "N";
      local.Ch4Found.Flag = "N";
      local.Ch5Found.Flag = "N";
      local.Ch6Found.Flag = "N";
      MoveIwoAction(entities.IwoAction, local.IwoAction);

      if (ReadDocumentEventDetail())
      {
        local.Document.Assign(entities.Document);
        local.EventDetail.ExceptionRoutine =
          entities.EventDetail.ExceptionRoutine;
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Document and Event_Detail not found for Outgoing_Document.";
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      if (ReadInfrastructure())
      {
        MoveInfrastructure2(entities.Infrastructure, local.Infrastructure);
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Infrastructure not found for outgoing_document.";
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      if (!ReadLegalAction())
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Legal_Action not found for IWO_Transaction.";
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      // -------------------------------------------------------------------------------------
      // -- Process the iwo_action record and create the outgoing file eIWO 
      // detail record.
      // -------------------------------------------------------------------------------------
      local.EiwoB587DetailRecord.Assign(local.Initialized);
      local.EiwoB587DetailRecord.IncomeWithholdingStartInstruc = "";

      if (Equal(entities.Document.Name, "IWO") || Equal
        (entities.Document.Name, "ORDIWO2") || Equal
        (entities.Document.Name, "ORDIWO2A"))
      {
        local.EiwoB587DetailRecord.DocumentActionCode = "ORG";
        local.EiwoB587DetailRecord.IncomeWithholdingStartInstruc = "RECEIPT";
      }
      else if (Equal(entities.Document.Name, "IWOMODO"))
      {
        local.EiwoB587DetailRecord.DocumentActionCode = "AMD";
        local.EiwoB587DetailRecord.IncomeWithholdingStartInstruc = "RECEIPT";
      }
      else if (Equal(entities.Document.Name, "IWOTERM") || Equal
        (entities.Document.Name, "ORDIWOPT"))
      {
        local.EiwoB587DetailRecord.DocumentActionCode = "TRM";
      }
      else if (Equal(entities.Document.Name, "ORDIWOLS"))
      {
        local.EiwoB587DetailRecord.DocumentActionCode = "LUM";
        local.EiwoB587DetailRecord.IncomeWithholdingStartInstruc = "RECEIPT";
      }
      else
      {
        // -- This is not an eIWO document.
        ++local.DocsSystemError.Count;
        local.WorkArea.Text50 =
          "SYSTEM ERROR:  Document is not an email document";
        local.EabReportSend.RptDetail = "";
        UseSpDocUpdateFailedBatchDoc();

        if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
        {
          // mjr-------> Trouble writing to Error Report:  quit immediately
          return;
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // mjr-------> Trouble updating DB2:  terminate
          break;
        }
        else
        {
          // mjr-------> Document error:  proceed to the next record
          continue;
        }
      }

      if (local.Totals.Count > 0)
      {
        for(local.Totals.Index = 0; local.Totals.Index < local.Totals.Count; ++
          local.Totals.Index)
        {
          if (!local.Totals.CheckSize())
          {
            break;
          }

          if (Equal(local.Document.Name, local.Totals.Item.GlocalTotals.Name) &&
            Equal
            (local.Document.VersionNumber,
            local.Totals.Item.GlocalTotals.VersionNumber))
          {
            break;
          }
        }

        local.Totals.CheckIndex();

        if (!Equal(local.Document.Name, local.Totals.Item.GlocalTotals.Name) ||
          !
          Equal(local.Document.VersionNumber,
          local.Totals.Item.GlocalTotals.VersionNumber))
        {
          local.Totals.Index = local.Totals.Count;
          local.Totals.CheckSize();

          MoveDocument2(local.Document, local.Totals.Update.GlocalTotals);
        }
      }
      else
      {
        local.Totals.Index = 0;
        local.Totals.CheckSize();

        MoveDocument2(local.Document, local.Totals.Update.GlocalTotals);
      }

      ++local.RowLockDocument.Count;
      ++local.DocsRead.Count;
      local.Totals.Update.GlocalTotalsRead.Count =
        local.Totals.Item.GlocalTotalsRead.Count + 1;

      // -- ORDIWO2A document field values are populated by SRRUN115 before the 
      // document is
      //    processed through the e-mail server.  No need to re-populate the 
      // values for the
      //    ORDIWO2A document again.
      if (!Equal(entities.Document.Name, "ORDIWO2A"))
      {
        // -----------------------------------------------------------------------------------
        // The Document Field Value retrieval logic below is copied from 
        // SP_B715_PROCESS_EMAIL_DOCUMENT.
        // -----------------------------------------------------------------------------------
        // -- Retrieve the document field values.
        UseSpPrintDataRetrievalMain();

        if (!IsEmpty(local.ErrorDocumentField.ScreenPrompt))
        {
          if (Equal(local.ErrorDocumentField.ScreenPrompt, "Resource Error"))
          {
            // mjr
            // -----------------------------------------------------------
            // Because a resource (normally ADABAS) is unavailable,
            // there is no need to continue the batch.
            // --------------------------------------------------------------
            local.EabReportSend.RptDetail =
              "ABEND:  Resource Unavailable  (usually ADABAS).";
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }

          local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
            .ErrorDocumentField.ScreenPrompt + (local.ErrorFieldValue.Value ?? ""
            );
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage1();
          local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
            .ExitStateWorkArea.Message;
        }

        if (!IsEmpty(local.EabReportSend.RptDetail))
        {
          // mjr
          // -------------------------------------------------------
          // Document failed print due to system error:
          //   add 1 to SYSTEM ERROR count
          //   write to ERROR REPORT
          // ------------------------------------------------------------
          ++local.DocsSystemError.Count;
          local.Totals.Update.GlocalTotalsSystemError.Count =
            local.Totals.Item.GlocalTotalsSystemError.Count + 1;
          local.WorkArea.Text50 = local.EabReportSend.RptDetail;
          local.EabReportSend.RptDetail = "";
          UseSpDocUpdateFailedBatchDoc();

          if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
          {
            // mjr-------> Trouble writing to Error Report:  quit immediately
            return;
          }
          else if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // mjr-------> Trouble updating DB2:  terminate
            break;
          }
          else
          {
            // mjr-------> Document error:  proceed to the next record
            continue;
          }
        }

        // mjr
        // ------------------------------------------------------
        // Check for missing and mandatory fields.
        // ---------------------------------------------------------
        local.RequiredFieldMissing.Flag = "N";
        local.UserinputField.Flag = "N";

        foreach(var item1 in ReadDocumentField2())
        {
          if (ReadFieldValue())
          {
            local.FieldValue.Value = entities.FieldValue.Value;
          }
          else
          {
            local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
          }

          if (!ReadField())
          {
            local.EabReportSend.RptDetail =
              "ABEND:  Field not found for document_field.";
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto ReadEach;
          }

          if (IsEmpty(local.FieldValue.Value) && AsChar
            (entities.DocumentField.RequiredSwitch) != 'N')
          {
            local.RequiredFieldMissing.Flag = "Y";
          }

          if (Equal(entities.Field.SubroutineName, "USRINPUT"))
          {
            local.UserinputField.Flag = "Y";
          }
        }

        if (AsChar(local.UserinputField.Flag) == 'Y')
        {
          // mjr
          // -------------------------------------------------------
          // Document contains a User Input field:
          //   add 1 to WARNING count
          //   write to ERROR REPORT
          // ------------------------------------------------------------
          ++local.DocsWarning.Count;
          local.Totals.Update.GlocalTotalsWarning.Count =
            local.Totals.Item.GlocalTotalsWarning.Count + 1;
          local.EabReportSend.RptDetail =
            "SYSTEM WARNING:  Document contains one or more User Input fields which cannot be populated.        -- Document Name = " +
            local.Document.Name;
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }

        if (AsChar(local.RequiredFieldMissing.Flag) == 'Y')
        {
          // mjr
          // -------------------------------------------------------
          // Document failed print due to data error:
          //   add 1 to DATA ERROR count
          //   write to ERROR REPORT
          // ------------------------------------------------------------
          ++local.DocsDataError.Count;
          local.Totals.Update.GlocalTotalsDataError.Count =
            local.Totals.Item.GlocalTotalsDataError.Count + 1;
          local.WorkArea.Text50 = "Missing mandatory field";
          local.EabReportSend.RptDetail = "";
          UseSpDocUpdateFailedBatchDoc();

          if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
          {
            // mjr-------> Trouble writing to Error Report:  quit immediately
            return;
          }
          else if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // mjr-------> Trouble updating DB2:  terminate
            break;
          }
          else
          {
            // mjr-------> Document error:  proceed to the next record
            continue;
          }
        }

        if (!IsEmpty(local.ErrorInd.Flag))
        {
          // mjr---->  Determine why the error ind is populated
          local.WorkArea.Text50 = TrimEnd(local.SpDocLiteral.IdDocument) + "RET"
            + local.ErrorInd.Flag;
          UseSpPrintDecodeReturnCode();

          if (!IsExitState("SP0000_DOWNLOAD_SUCCESSFUL"))
          {
            // mjr
            // -------------------------------------------------------
            // Document failed print due to data error:
            //   add 1 to DATA ERROR count
            //   write to ERROR REPORT
            // ------------------------------------------------------------
            ++local.DocsDataError.Count;
            local.Totals.Update.GlocalTotalsDataError.Count =
              local.Totals.Item.GlocalTotalsDataError.Count + 1;
            UseEabExtractExitStateMessage1();
            local.WorkArea.Text50 = local.ExitStateWorkArea.Message;
            local.EabReportSend.RptDetail = "";
            UseSpDocUpdateFailedBatchDoc();

            if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
            {
              // mjr-------> Trouble writing to Error Report:  quit immediately
              return;
            }
            else if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // mjr-------> Trouble updating DB2:  terminate
              break;
            }
            else
            {
              // mjr-------> Document error:  proceed to the next record
              continue;
            }
          }
          else
          {
            // mjr
            // ---------------------------------------------------
            // 12/02/1999
            // Reset exitstate to All_OK after it is changed
            // ----------------------------------------------------------------
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }

        // -- Service Provider email address is not required for eIWO 
        // processing.
        // mjr
        // -----------------------------------------------------------------
        // Document printing is successful.
        // Update outgoing_document, create monitored document (if
        //     necessary), and update infrastructure record.
        // --------------------------------------------------------------------
        local.Infrastructure.SystemGeneratedIdentifier =
          local.Infrastructure.SystemGeneratedIdentifier;
        local.Infrastructure.LastUpdatedBy = local.ProgramProcessingInfo.Name;
        local.Infrastructure.LastUpdatedTimestamp = local.Current.Timestamp;
        UseSpDocUpdateSuccessfulPrint();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Could not update outgoing_document after a successful generation.";
            
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        // -- Don't need to set print_sucessful_ind to "P" for eIWO processing.
        // mjr
        // ---------------------------------------------------------
        // Alert for automatic documents
        // ------------------------------------------------------------
        if (Equal(local.EventDetail.ExceptionRoutine, "AUTODOC"))
        {
          // mjr
          // ---------------------------------------------------------
          // Alert for automatic Locate documents
          // ------------------------------------------------------------
          if (Equal(local.Document.BusinessObject, "LOC"))
          {
            UseSpPrintDataRetrievalKeys2();

            if (!IsEmpty(local.SpDocKey.KeyPerson))
            {
              local.CsePersonsWorkSet.Number = local.SpDocKey.KeyPerson;
            }
            else if (!IsEmpty(local.SpDocKey.KeyAp))
            {
              local.CsePersonsWorkSet.Number = local.SpDocKey.KeyAp;
            }
            else
            {
              goto Test;
            }

            foreach(var item1 in ReadCase())
            {
              local.OfficeServiceProviderAlert.RecipientUserId = "";

              // mjr
              // ------------------------------------------
              // 05/08/2001
              // Send an alert to each OSP assigned to each Open ENF
              // Legal Requests relaed to this case
              // -------------------------------------------------------
              foreach(var item2 in ReadLegalReferral())
              {
                if (Equal(entities.LegalReferral.ReferralReason1, "ENF"))
                {
                }
                else if (Equal(entities.LegalReferral.ReferralReason2, "ENF"))
                {
                }
                else if (Equal(entities.LegalReferral.ReferralReason3, "ENF"))
                {
                }
                else if (Equal(entities.LegalReferral.ReferralReason4, "ENF"))
                {
                }
                else if (Equal(entities.LegalReferral.ReferralReason5, "ENF"))
                {
                }
                else
                {
                  continue;
                }

                foreach(var item3 in ReadLegalReferralAssignment())
                {
                  if (ReadOfficeServiceProviderServiceProviderOffice2())
                  {
                    local.ServiceProvider.SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                    local.Office.SystemGeneratedId =
                      entities.Office.SystemGeneratedId;
                    local.OfficeServiceProvider.RoleCode =
                      entities.OfficeServiceProvider.RoleCode;
                    local.OfficeServiceProvider.EffectiveDate =
                      entities.OfficeServiceProvider.EffectiveDate;
                    MoveOfficeServiceProviderAlert(local.Automatic,
                      local.OfficeServiceProviderAlert);
                    local.OfficeServiceProviderAlert.RecipientUserId =
                      entities.ServiceProvider.UserId;
                    UseSpCabCreateOfcSrvPrvdAlert();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      // mjr
                      // -------------------------------------------------------------
                      // Reset exitstate to All_OK after it is changed
                      // ----------------------------------------------------------------
                      ExitState = "ACO_NN0000_ALL_OK";
                      local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
                        (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":       Unable to send Alert to Service Provider      -- Document Name = " +
                        TrimEnd(local.Document.Name) + ", USERID = " + TrimEnd
                        (local.Infrastructure.UserId);
                      UseCabErrorReport1();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }

                      local.EabReportSend.RptDetail = "";
                    }
                  }
                }
              }

              if (IsEmpty(local.OfficeServiceProviderAlert.RecipientUserId))
              {
                // mjr
                // ----------------------------------------------------
                // No Legal Requests were found for this Case.
                // Send the alert to the Case worker
                // -------------------------------------------------------
                foreach(var item2 in ReadCaseAssignment())
                {
                  if (ReadOfficeServiceProviderServiceProviderOffice1())
                  {
                    local.ServiceProvider.SystemGeneratedId =
                      entities.ServiceProvider.SystemGeneratedId;
                    local.Office.SystemGeneratedId =
                      entities.Office.SystemGeneratedId;
                    local.OfficeServiceProvider.RoleCode =
                      entities.OfficeServiceProvider.RoleCode;
                    local.OfficeServiceProvider.EffectiveDate =
                      entities.OfficeServiceProvider.EffectiveDate;
                    MoveOfficeServiceProviderAlert(local.Automatic,
                      local.OfficeServiceProviderAlert);
                    local.OfficeServiceProviderAlert.RecipientUserId =
                      entities.ServiceProvider.UserId;
                    UseSpCabCreateOfcSrvPrvdAlert();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      // mjr
                      // -------------------------------------------------------------
                      // Reset exitstate to All_OK after it is changed
                      // ----------------------------------------------------------------
                      ExitState = "ACO_NN0000_ALL_OK";
                      local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
                        (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":       Unable to send Alert to Service Provider      -- Document Name = " +
                        TrimEnd(local.Document.Name) + ", USERID = " + TrimEnd
                        (local.Infrastructure.UserId);
                      UseCabErrorReport1();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                        return;
                      }

                      local.EabReportSend.RptDetail = "";
                    }
                  }
                }
              }
            }
          }
          else
          {
            UseSpPrintDataRetrievalKeys1();

            if (!Equal(local.Document.Name, "ARCLOS60") && !
              Equal(local.Document.Name, "NOTCCLOS"))
            {
              local.DateWorkArea.Date = local.Current.Date;
            }
            else
            {
              local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
            }

            UseSpDocGetServiceProvider();
            local.ServiceProvider.SystemGeneratedId =
              local.OutDocRtrnAddr.ServProvSysGenId;
            local.Office.SystemGeneratedId =
              local.OutDocRtrnAddr.OfficeSysGenId;
            local.OfficeServiceProvider.RoleCode =
              local.OutDocRtrnAddr.OspRoleCode;
            local.OfficeServiceProvider.EffectiveDate =
              local.OutDocRtrnAddr.OspEffectiveDate;
            MoveOfficeServiceProviderAlert(local.Automatic,
              local.OfficeServiceProviderAlert);
            local.OfficeServiceProviderAlert.RecipientUserId =
              local.OutDocRtrnAddr.ServProvUserId;
            UseSpCabCreateOfcSrvPrvdAlert();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // mjr
              // -------------------------------------------------------------
              // Reset exitstate to All_OK after it is changed
              // ----------------------------------------------------------------
              ExitState = "ACO_NN0000_ALL_OK";
              local.EabReportSend.RptDetail =
                "     Unable to send Alert to Service Provider for the previous error.";
                
              local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
                (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":       Unable to send Alert to Service Provider      -- Document Name = " +
                TrimEnd(local.Document.Name) + ", USERID = " + TrimEnd
                (local.Infrastructure.UserId);
              UseCabErrorReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.EabReportSend.RptDetail = "";
            }
          }
        }

        // -----------------------------------------------------------------------------------
        // End of SP_B715_PROCESS_EMAIL_DOCUMENT logic.
        // -----------------------------------------------------------------------------------
      }

Test:

      // -------------------------------------------------------------------------------------
      // -- Document field retrieval was successful.  Build the detail record 
      // for the outgoing file.
      // -------------------------------------------------------------------------------------
      if (Equal(local.EiwoB587DetailRecord.DocumentActionCode, "TRM"))
      {
        local.EiwoB587DetailRecord.SendPaymentWithhinDays = "00";
        local.EiwoB587DetailRecord.IwoCcpaPercentRate = "00";
      }
      else
      {
        local.EiwoB587DetailRecord.SendPaymentWithhinDays = "07";
        local.EiwoB587DetailRecord.IwoCcpaPercentRate = "50";
      }

      local.EmployerName.Text33 = "";
      local.EmployerLocation.Text30 = "";

      foreach(var item1 in ReadDocumentField1())
      {
        if (!ReadField())
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Field not found for document_field.";
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (ReadFieldValue())
        {
          local.FieldValue.Value = entities.FieldValue.Value;
        }
        else
        {
          if (Equal(entities.Field.Name, "ARNMORGZ"))
          {
            local.Arnmorgz.ObligeeLastName = "NOT PROVIDED";
          }

          continue;
        }

        // -- Create a view of the field value with leading spaces removed.
        local.Trimmed.Value = UseLeB587TrimLeadingSpaces();

        // -- Create a view of the field value with only characters A-Z, period 
        // (.), hyphen (-), apostophe ('), and embedded spaces.
        UseLeB587RemoveInvalidChars1();

        switch(TrimEnd(entities.Field.Name))
        {
          case "LATRBSTNM":
            local.EiwoB587DetailRecord.IssuingStateTribeTerritoryNm =
              local.Trimmed.Value ?? Spaces(35);
            local.EiwoB587DetailRecord.IssuingTribunalName =
              entities.FieldValue.Value ?? Spaces(35);

            break;
          case "LATRBCITNM":
            if (IsEmpty(local.EiwoB587DetailRecord.IssuingJurisdictionName))
            {
              local.EiwoB587DetailRecord.IssuingJurisdictionName =
                entities.FieldValue.Value ?? Spaces(35);
            }
            else
            {
              local.EiwoB587DetailRecord.IssuingJurisdictionName =
                TrimEnd(entities.FieldValue.Value) + "/" + local
                .EiwoB587DetailRecord.IssuingJurisdictionName;
            }

            break;
          case "LAFIPSCONM":
            if (IsEmpty(local.EiwoB587DetailRecord.IssuingJurisdictionName))
            {
              local.EiwoB587DetailRecord.IssuingJurisdictionName =
                entities.FieldValue.Value ?? Spaces(35);
            }
            else
            {
              local.EiwoB587DetailRecord.IssuingJurisdictionName =
                TrimEnd(local.EiwoB587DetailRecord.IssuingJurisdictionName) + "/"
                + entities.FieldValue.Value;
            }

            break;
          case "CSECASENUM":
            break;
          case "APINCSNM":
            local.EmployerName.Text33 = entities.FieldValue.Value ?? Spaces(33);
            local.EiwoB587DetailRecord.EmployerName =
              local.FirstCharCleaned.Value ?? Spaces(57);

            break;
          case "APINCSADD1":
            local.EiwoB587DetailRecord.EmployerAddressLine1 =
              local.FirstCharCleaned.Value ?? Spaces(25);

            break;
          case "APINCSADD2":
            local.EiwoB587DetailRecord.EmployerAddressLine2 =
              local.FirstCharCleaned.Value ?? Spaces(25);

            break;
          case "APINCSADD3":
            local.EmployerLocation.Text30 = entities.FieldValue.Value ?? Spaces
              (30);
            local.Common.Count = Find(local.Trimmed.Value, ",");

            if (local.Common.Count == 0)
            {
              continue;
            }

            local.EiwoB587DetailRecord.EmployerAddressStateCode =
              Substring(local.Trimmed.Value, local.Common.Count + 2, 2);
            local.EiwoB587DetailRecord.EmployerAddressZipCode =
              Substring(local.Trimmed.Value, local.Common.Count + 6, 5);
            local.EiwoB587DetailRecord.EmployerAddressExtZipCode =
              Substring(local.Trimmed.Value, local.Common.Count + 12, 4);

            // -- Clean first character of City
            local.Trimmed.Value =
              Substring(local.Trimmed.Value, 1, local.Common.Count - 1);
            UseLeB587RemoveInvalidChars2();
            local.EiwoB587DetailRecord.EmployerAddressCityName =
              local.FirstCharCleaned.Value ?? Spaces(22);

            break;
          case "APEMPEIN":
            local.EiwoB587DetailRecord.Ein = entities.FieldValue.Value ?? Spaces
              (9);

            break;
          case "APNM":
            // -- This field was replaced by APFIRSTNM, APMIDINIT, and APLASTNM 
            // below.
            break;
          case "APFIRSTNM":
            local.EiwoB587DetailRecord.EmployeeFirstName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "APMIDINIT":
            local.EiwoB587DetailRecord.EmployeeMiddleName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "APLASTNM":
            local.EiwoB587DetailRecord.EmployeeLastName =
              local.AllCharCleaned.Value ?? Spaces(20);

            break;
          case "APSSN":
            if (Equal(entities.FieldValue.Value, "UNKNOWN"))
            {
              local.EiwoB587DetailRecord.EmployeeSsn = "";
            }
            else
            {
              local.EiwoB587DetailRecord.EmployeeSsn =
                Substring(entities.FieldValue.Value, 1, 3) + Substring
                (entities.FieldValue.Value, 5, 2) + Substring
                (entities.FieldValue.Value, 8, 4);
            }

            break;
          case "APDOB":
            // -- Format must be CCYYMMDD
            if (Equal(entities.FieldValue.Value, "UNKNOWN"))
            {
              local.EiwoB587DetailRecord.EmployeeBirthDate = "";
            }
            else
            {
              local.EiwoB587DetailRecord.EmployeeBirthDate =
                Substring(entities.FieldValue.Value, 7, 4) + Substring
                (entities.FieldValue.Value, 1, 2) + Substring
                (entities.FieldValue.Value, 4, 2);
            }

            break;
          case "ARNMORGZ":
            // -- Save this value in a separate local view.  It is only used if 
            // the ARFIRSTNM field
            //    is spaces.  This is done below this read each, after all 
            // fields are populated.
            local.Arnmorgz.ObligeeLastName = local.AllCharCleaned.Value ?? Spaces
              (57);

            break;
          case "ARFIRSTNM":
            local.EiwoB587DetailRecord.ObligeeFirstName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "ARMIDINIT":
            local.EiwoB587DetailRecord.ObligeeMiddleName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "ARLASTNM":
            local.EiwoB587DetailRecord.ObligeeLastName =
              local.AllCharCleaned.Value ?? Spaces(57);

            break;
          case "LAWCAMT":
            if (Equal(local.EiwoB587DetailRecord.DocumentActionCode, "TRM"))
            {
              local.EiwoB587DetailRecord.SupportCurrentChildAmount =
                local.Zero.Text11;
              local.EiwoB587DetailRecord.SupportCurrentChildFrequency = "";
            }
            else
            {
              local.EiwoB587DetailRecord.SupportCurrentChildFrequency = "M";

              if (IsEmpty(entities.FieldValue.Value))
              {
                local.EiwoB587DetailRecord.SupportCurrentChildAmount =
                  local.Zero.Text11;
              }
              else
              {
                local.Common.Count = Length(TrimEnd(entities.FieldValue.Value));
                local.EiwoB587DetailRecord.SupportCurrentChildAmount =
                  Substring(local.Zero.Text11, WorkArea.Text11_MaxLength, 1,
                  12 - local.Common.Count) + Substring
                  (entities.FieldValue.Value, 1, local.Common.Count - 3) + Substring
                  (entities.FieldValue.Value, local.Common.Count - 1, 2);
              }
            }

            break;
          case "LAWAAMT":
            if (Equal(local.EiwoB587DetailRecord.DocumentActionCode, "TRM"))
            {
              local.EiwoB587DetailRecord.SupportPastDueChildAmount =
                local.Zero.Text11;
              local.EiwoB587DetailRecord.SupportPastDueChildFrequency = "";
            }
            else
            {
              local.EiwoB587DetailRecord.SupportPastDueChildFrequency = "M";

              if (IsEmpty(entities.FieldValue.Value))
              {
                local.EiwoB587DetailRecord.SupportPastDueChildAmount =
                  local.Zero.Text11;
              }
              else
              {
                local.Common.Count = Length(TrimEnd(entities.FieldValue.Value));
                local.EiwoB587DetailRecord.SupportPastDueChildAmount =
                  Substring(local.Zero.Text11, WorkArea.Text11_MaxLength, 1,
                  12 - local.Common.Count) + Substring
                  (entities.FieldValue.Value, 1, local.Common.Count - 3) + Substring
                  (entities.FieldValue.Value, local.Common.Count - 1, 2);
              }
            }

            break;
          case "LAWCWASUM":
            if (Equal(local.EiwoB587DetailRecord.DocumentActionCode, "TRM"))
            {
              local.EiwoB587DetailRecord.ObligationTotalAmount =
                local.Zero.Text11;
              local.EiwoB587DetailRecord.ObligationTotalFrequency = "";
              local.EiwoB587DetailRecord.IwoDeductionMonthlyAmount =
                local.Zero.Text11;
            }
            else
            {
              local.EiwoB587DetailRecord.ObligationTotalFrequency = "M";

              if (IsEmpty(entities.FieldValue.Value))
              {
                local.EiwoB587DetailRecord.ObligationTotalAmount =
                  local.Zero.Text11;
                local.EiwoB587DetailRecord.IwoDeductionMonthlyAmount =
                  local.Zero.Text11;
              }
              else
              {
                local.Common.Count = Length(TrimEnd(entities.FieldValue.Value));
                local.EiwoB587DetailRecord.ObligationTotalAmount =
                  Substring(local.Zero.Text11, WorkArea.Text11_MaxLength, 1,
                  12 - local.Common.Count) + Substring
                  (entities.FieldValue.Value, 1, local.Common.Count - 3) + Substring
                  (entities.FieldValue.Value, local.Common.Count - 1, 2);
                local.EiwoB587DetailRecord.IwoDeductionMonthlyAmount =
                  Substring(local.Zero.Text11, WorkArea.Text11_MaxLength, 1,
                  12 - local.Common.Count) + Substring
                  (entities.FieldValue.Value, 1, local.Common.Count - 3) + Substring
                  (entities.FieldValue.Value, local.Common.Count - 1, 2);
              }
            }

            break;
          case "LAWCWAW":
            if (Equal(local.EiwoB587DetailRecord.DocumentActionCode, "TRM"))
            {
              local.EiwoB587DetailRecord.IwoDeductionWeeklyAmount =
                local.Zero.Text11;
            }
            else if (IsEmpty(entities.FieldValue.Value))
            {
              local.EiwoB587DetailRecord.IwoDeductionWeeklyAmount =
                local.Zero.Text11;
            }
            else
            {
              local.Common.Count = Length(TrimEnd(entities.FieldValue.Value));
              local.EiwoB587DetailRecord.IwoDeductionWeeklyAmount =
                Substring(local.Zero.Text11, WorkArea.Text11_MaxLength, 1, 12 -
                local.Common.Count) + Substring
                (entities.FieldValue.Value, 1, local.Common.Count - 3) + Substring
                (entities.FieldValue.Value, local.Common.Count - 1, 2);
            }

            break;
          case "LAWCWABW":
            if (Equal(local.EiwoB587DetailRecord.DocumentActionCode, "TRM"))
            {
              local.EiwoB587DetailRecord.IwoDeductionBiweeklyAmount =
                local.Zero.Text11;
            }
            else if (IsEmpty(entities.FieldValue.Value))
            {
              local.EiwoB587DetailRecord.IwoDeductionBiweeklyAmount =
                local.Zero.Text11;
            }
            else
            {
              local.Common.Count = Length(TrimEnd(entities.FieldValue.Value));
              local.EiwoB587DetailRecord.IwoDeductionBiweeklyAmount =
                Substring(local.Zero.Text11, WorkArea.Text11_MaxLength, 1, 12 -
                local.Common.Count) + Substring
                (entities.FieldValue.Value, 1, local.Common.Count - 3) + Substring
                (entities.FieldValue.Value, local.Common.Count - 1, 2);
            }

            break;
          case "LAWCWASM":
            if (Equal(local.EiwoB587DetailRecord.DocumentActionCode, "TRM"))
            {
              local.EiwoB587DetailRecord.IwoDeductionSemimonthlyAmount =
                local.Zero.Text11;
            }
            else if (IsEmpty(entities.FieldValue.Value))
            {
              local.EiwoB587DetailRecord.IwoDeductionSemimonthlyAmount =
                local.Zero.Text11;
            }
            else
            {
              local.Common.Count = Length(TrimEnd(entities.FieldValue.Value));
              local.EiwoB587DetailRecord.IwoDeductionSemimonthlyAmount =
                Substring(local.Zero.Text11, WorkArea.Text11_MaxLength, 1, 12 -
                local.Common.Count) + Substring
                (entities.FieldValue.Value, 1, local.Common.Count - 3) + Substring
                (entities.FieldValue.Value, local.Common.Count - 1, 2);
            }

            break;
          case "KPCNM":
            // -- This field will be overwritten below to another states SDU 
            // name for out of state orders.
            local.EiwoB587DetailRecord.PayeeName =
              local.FirstCharCleaned.Value ?? Spaces(57);

            break;
          case "LACTSTDNUM":
            // -- 10/19/16 GVandy  CQ54541  New document field LACTKPCNUM is 
            // replacing LACTSTDNUM.
            //    The logic for LACTSTDNUM was left to support old eIWO 
            // transactions but will not be
            //    executed for newer eIWOs.
            // -- Remove all "*" and "\" from the standard number.
            local.Temp.Value = Spaces(FieldValue.Value_MaxLength);
            local.Common.Count = 1;

            for(var limit = Length(TrimEnd(entities.FieldValue.Value)); local
              .Common.Count <= limit; ++local.Common.Count)
            {
              if (CharAt(entities.FieldValue.Value, local.Common.Count) == '*'
                || CharAt(entities.FieldValue.Value, local.Common.Count) == '\\'
                )
              {
                // -- Continue
              }
              else
              {
                local.Temp.Value = TrimEnd(local.Temp.Value) + Substring
                  (entities.FieldValue.Value, local.Common.Count, 1);
              }
            }

            local.EiwoB587DetailRecord.OrderIdentifier = local.Temp.Value ?? Spaces
              (30);
            local.EiwoB587DetailRecord.RemittanceIdentifier =
              local.Temp.Value ?? Spaces(20);
            local.EiwoB587DetailRecord.CaseIdentifier = local.Temp.Value ?? Spaces
              (15);

            break;
          case "LACTKPCNUM":
            // -- 10/19/16 GVandy  CQ54541  Add new document field LACTKPCNUM.
            local.EiwoB587DetailRecord.OrderIdentifier =
              entities.FieldValue.Value ?? Spaces(30);
            local.EiwoB587DetailRecord.RemittanceIdentifier =
              entities.FieldValue.Value ?? Spaces(20);
            local.EiwoB587DetailRecord.CaseIdentifier =
              entities.FieldValue.Value ?? Spaces(15);

            break;
          case "CH0NM":
            // -- This is replaced with individual name fields below.
            break;
          case "CH0FIRSTNM":
            local.EiwoB587DetailRecord.Child1FirstName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH0MIDINIT":
            local.EiwoB587DetailRecord.Child1MiddleName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH0LASTNM":
            local.EiwoB587DetailRecord.Child1LastName =
              local.AllCharCleaned.Value ?? Spaces(20);

            break;
          case "CH0DOB":
            // -- Format must be CCYYMMDD
            if (Equal(entities.FieldValue.Value, "UNKNOWN"))
            {
              local.EiwoB587DetailRecord.Child1BirthDate = "";
            }
            else
            {
              local.EiwoB587DetailRecord.Child1BirthDate =
                Substring(entities.FieldValue.Value, 7, 4) + Substring
                (entities.FieldValue.Value, 1, 2) + Substring
                (entities.FieldValue.Value, 4, 2);
            }

            break;
          case "CH1NM":
            // -- This is replaced with individual name fields below.
            break;
          case "CH1FIRSTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch2Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child2FirstName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH1MIDINIT":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch2Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child2MiddleName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH1LASTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch2Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child2LastName =
              local.AllCharCleaned.Value ?? Spaces(20);

            break;
          case "CH1DOB":
            // -- Format must be CCYYMMDD
            if (Equal(entities.FieldValue.Value, "UNKNOWN"))
            {
              local.EiwoB587DetailRecord.Child2BirthDate = "";
            }
            else
            {
              local.Ch2Found.Flag = "Y";
              local.EiwoB587DetailRecord.Child2BirthDate =
                Substring(entities.FieldValue.Value, 7, 4) + Substring
                (entities.FieldValue.Value, 1, 2) + Substring
                (entities.FieldValue.Value, 4, 2);
            }

            break;
          case "CH2NM":
            // -- This is replaced with individual name fields below.
            break;
          case "CH2FIRSTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch3Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child3FirstName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH2MIDINIT":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch3Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child3MiddleName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH2LASTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch3Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child3LastName =
              local.AllCharCleaned.Value ?? Spaces(20);

            break;
          case "CH2DOB":
            // -- Format must be CCYYMMDD
            if (Equal(entities.FieldValue.Value, "UNKNOWN"))
            {
              local.EiwoB587DetailRecord.Child3BirthDate = "";
            }
            else
            {
              local.Ch3Found.Flag = "Y";
              local.EiwoB587DetailRecord.Child3BirthDate =
                Substring(entities.FieldValue.Value, 7, 4) + Substring
                (entities.FieldValue.Value, 1, 2) + Substring
                (entities.FieldValue.Value, 4, 2);
            }

            break;
          case "CH3NM":
            // -- This is replaced with individual name fields below.
            break;
          case "CH3FIRSTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch4Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child4FirstName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH3MIDINIT":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch4Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child4MiddleName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH3LASTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch4Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child4LastName =
              local.AllCharCleaned.Value ?? Spaces(20);

            break;
          case "CH3DOB":
            // -- Format must be CCYYMMDD
            if (Equal(entities.FieldValue.Value, "UNKNOWN"))
            {
              local.EiwoB587DetailRecord.Child4BirthDate = "";
            }
            else
            {
              local.Ch4Found.Flag = "Y";
              local.EiwoB587DetailRecord.Child4BirthDate =
                Substring(entities.FieldValue.Value, 7, 4) + Substring
                (entities.FieldValue.Value, 1, 2) + Substring
                (entities.FieldValue.Value, 4, 2);
            }

            break;
          case "CH4NM":
            // -- This is replaced with individual name fields below.
            break;
          case "CH4FIRSTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch5Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child5FirstName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH4MIDINIT":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch5Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child5MiddleName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH4LASTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch5Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child5LastName =
              local.AllCharCleaned.Value ?? Spaces(20);

            break;
          case "CH4DOB":
            // -- Format must be CCYYMMDD
            if (Equal(entities.FieldValue.Value, "UNKNOWN"))
            {
              local.EiwoB587DetailRecord.Child5BirthDate = "";
            }
            else
            {
              local.Ch5Found.Flag = "Y";
              local.EiwoB587DetailRecord.Child5BirthDate =
                Substring(entities.FieldValue.Value, 7, 4) + Substring
                (entities.FieldValue.Value, 1, 2) + Substring
                (entities.FieldValue.Value, 4, 2);
            }

            break;
          case "CH5NM":
            // -- This is replaced with individual name fields below.
            break;
          case "CH5FIRSTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch6Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child6FirstName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH5MIDINIT":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch6Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child6MiddleName =
              local.AllCharCleaned.Value ?? Spaces(15);

            break;
          case "CH5LASTNM":
            if (!IsEmpty(local.AllCharCleaned.Value))
            {
              local.Ch6Found.Flag = "Y";
            }

            local.EiwoB587DetailRecord.Child6LastName =
              local.AllCharCleaned.Value ?? Spaces(20);

            break;
          case "CH5DOB":
            // -- Format must be CCYYMMDD
            if (Equal(entities.FieldValue.Value, "UNKNOWN"))
            {
              local.EiwoB587DetailRecord.Child6BirthDate = "";
            }
            else
            {
              local.Ch6Found.Flag = "Y";
              local.EiwoB587DetailRecord.Child6BirthDate =
                Substring(entities.FieldValue.Value, 7, 4) + Substring
                (entities.FieldValue.Value, 1, 2) + Substring
                (entities.FieldValue.Value, 4, 2);
            }

            break;
          case "LAWLAMT":
            local.EiwoB587DetailRecord.LumpSumPaymentAmount =
              entities.FieldValue.Value ?? Spaces(11);

            if (Equal(local.EiwoB587DetailRecord.DocumentActionCode, "LUM"))
            {
              if (IsEmpty(entities.FieldValue.Value))
              {
                local.EiwoB587DetailRecord.LumpSumPaymentAmount =
                  local.Zero.Text11;
              }
              else
              {
                local.Common.Count = Length(TrimEnd(entities.FieldValue.Value));
                local.EiwoB587DetailRecord.LumpSumPaymentAmount =
                  Substring(local.Zero.Text11, WorkArea.Text11_MaxLength, 1,
                  12 - local.Common.Count) + Substring
                  (entities.FieldValue.Value, 1, local.Common.Count - 3) + Substring
                  (entities.FieldValue.Value, local.Common.Count - 1, 2);
              }
            }
            else
            {
              local.EiwoB587DetailRecord.LumpSumPaymentAmount =
                local.Zero.Text11;
            }

            break;
          default:
            break;
        }
      }

      // -- If obligee first name is blank then replace the obligee last name 
      // with the ARNMORGZ value.
      if (IsEmpty(local.EiwoB587DetailRecord.ObligeeFirstName))
      {
        local.EiwoB587DetailRecord.ObligeeFirstName = ".";
        local.EiwoB587DetailRecord.ObligeeLastName =
          local.Arnmorgz.ObligeeLastName;
      }

      // -- Setup the Payee information.
      switch(TrimEnd(local.EiwoB587DetailRecord.IssuingStateTribeTerritoryNm))
      {
        case "":
          break;
        case "KANSAS":
          // -- Read the address info for the KPC.
          if (ReadFipsTribAddressFips2())
          {
            local.EiwoB587DetailRecord.PayeeAddressLine1 =
              entities.SduFipsTribAddress.Street1;
            local.EiwoB587DetailRecord.PayeeAddressLine2 =
              entities.SduFipsTribAddress.Street2 ?? Spaces(25);
            local.EiwoB587DetailRecord.PayeeAddressCity =
              entities.SduFipsTribAddress.City;
            local.EiwoB587DetailRecord.PayeeAddressStateCode =
              entities.SduFipsTribAddress.State;
            local.EiwoB587DetailRecord.PayeeAddressZipCode =
              entities.SduFipsTribAddress.ZipCode;
            local.EiwoB587DetailRecord.PayeeAddressExtZipCode =
              entities.SduFipsTribAddress.Zip4 ?? Spaces(4);
            local.EiwoB587DetailRecord.PayeeRemittanceFipsCode =
              NumberToString(entities.SduFips.State, 14, 2) + NumberToString
              (entities.SduFips.County, 13, 3) + NumberToString
              (entities.SduFips.Location, 14, 2);
          }
          else
          {
            // -- Set Payee Name to spaces which will be flagged as missing 
            // required field below.
            local.EiwoB587DetailRecord.PayeeName = "";
          }

          break;
        default:
          // -- Read other state SDU info.
          local.EiwoB587DetailRecord.PayeeName = "";

          if (!ReadFips())
          {
            // -- Escape.  Required Payee fields will be flagged as missing 
            // below.
            break;
          }

          if (ReadFipsTribAddressFips1())
          {
            // -- Clean the first character of the payee name.
            local.Trimmed.Value = entities.SduFips.LocationDescription;
            UseLeB587RemoveInvalidChars2();
            local.EiwoB587DetailRecord.PayeeName =
              local.FirstCharCleaned.Value ?? Spaces(57);
            local.EiwoB587DetailRecord.PayeeAddressLine1 =
              entities.SduFipsTribAddress.Street1;
            local.EiwoB587DetailRecord.PayeeAddressLine2 =
              entities.SduFipsTribAddress.Street2 ?? Spaces(25);
            local.EiwoB587DetailRecord.PayeeAddressCity =
              entities.SduFipsTribAddress.City;
            local.EiwoB587DetailRecord.PayeeAddressStateCode =
              entities.SduFipsTribAddress.State;
            local.EiwoB587DetailRecord.PayeeAddressZipCode =
              entities.SduFipsTribAddress.ZipCode;
            local.EiwoB587DetailRecord.PayeeAddressExtZipCode =
              entities.SduFipsTribAddress.Zip4 ?? Spaces(4);
            local.EiwoB587DetailRecord.PayeeRemittanceFipsCode =
              NumberToString(entities.SduFips.State, 14, 2) + NumberToString
              (entities.SduFips.County, 13, 3) + NumberToString
              (entities.SduFips.Location, 14, 2);
          }
          else
          {
            // -- Escape.  Required Payee fields will be flagged as missing 
            // below.
          }

          break;
      }

      local.EiwoStatusMessage.Detail = "EMP: " + TrimEnd
        (local.EmployerName.Text33) + "; LOC: " + local
        .EmployerLocation.Text30;

      // -- Check for missing mandatory eIWO fields...
      local.MissingMandatoryField.Flag = "N";

      for(local.Common.Count = 1; local.Common.Count <= 78; ++
        local.Common.Count)
      {
        local.EabReportSend.RptDetail = "";

        switch(local.Common.Count)
        {
          case 1:
            if (IsEmpty(local.EiwoB587DetailRecord.DocumentCode))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: DOCUMENT CODE";
            }

            break;
          case 2:
            if (IsEmpty(local.EiwoB587DetailRecord.DocumentActionCode))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: DOCUMENT ACTION CODE";
            }

            break;
          case 3:
            if (IsEmpty(local.EiwoB587DetailRecord.DocumentDate))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: DOCUMENT DATE";
            }

            break;
          case 4:
            if (IsEmpty(local.EiwoB587DetailRecord.IssuingStateTribeTerritoryNm))
              
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: ISSUING STATE TRIBE TERRITORY NAME";
                
            }

            break;
          case 5:
            if (IsEmpty(local.EiwoB587DetailRecord.CaseIdentifier))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CASE IDENTIFIER";
            }

            break;
          case 6:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployerName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYER NAME";
            }

            break;
          case 7:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployerAddressLine1))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYER ADDRESS LINE 1";
            }

            break;
          case 8:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployerAddressCityName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYER ADDRESS CITY";
            }

            break;
          case 9:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployerAddressStateCode))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYER ADDRESS STATE CODE";
            }

            break;
          case 10:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployerAddressZipCode))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYER ADDRESS ZIP CODE";
            }

            break;
          case 11:
            if (IsEmpty(local.EiwoB587DetailRecord.Ein))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYER EIN";
            }

            break;
          case 12:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployeeLastName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYEE LAST NAME";
            }

            break;
          case 13:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployeeFirstName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYEE FIRST NAME";
            }

            break;
          case 14:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployeeSsn))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYEE SSN";
            }

            break;
          case 15:
            if (IsEmpty(local.EiwoB587DetailRecord.ObligeeFirstName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: OBLIGEE FIRST NAME";
            }

            break;
          case 16:
            if (IsEmpty(local.EiwoB587DetailRecord.ObligeeLastName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: OBLIGEE LAST NAME";
            }

            break;
          case 17:
            if (IsEmpty(local.EiwoB587DetailRecord.IssuingTribunalName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: TRIBUNAL NAME";
            }

            break;
          case 18:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportCurrentChildAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CURRENT CHILD SUPPORT AMOUNT";
            }

            break;
          case 19:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportCurrentChildFrequency) &&
              !
              Equal(local.EiwoB587DetailRecord.SupportCurrentChildAmount,
              local.Zero.Text11))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CURRENT CHILD SUPPORT FREQUENCY";
                
            }

            break;
          case 20:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportPastDueChildAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAST DUE CHILD SUPPORT AMOUNT";
            }

            break;
          case 21:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportPastDueChildFrequency) &&
              !
              Equal(local.EiwoB587DetailRecord.SupportPastDueChildAmount,
              local.Zero.Text11))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAST DUE CHILD SUPPORT FREQUENCY";
                
            }

            break;
          case 22:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportCurrentMedicalAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CURRENT MEDICAL SUPPORT AMOUNT";
            }

            break;
          case 23:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportCurrentMedicalFrequenc)
              && !
              Equal(local.EiwoB587DetailRecord.SupportCurrentMedicalAmount,
              local.Zero.Text11))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CURRENT MEDICAL SUPPORT FREQUENCY";
                
            }

            break;
          case 24:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportPastDueMedicalAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAST DUE MEDICAL SUPPORT AMOUNT";
                
            }

            break;
          case 25:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportPastDueMedicalFrequen) &&
              !
              Equal(local.EiwoB587DetailRecord.SupportPastDueMedicalAmount,
              local.Zero.Text11))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAST DUE MEDICAL SUPPORT FREQUENCY";
                
            }

            break;
          case 26:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportCurrentSpousalAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CURRENT SPOUSAL SUPPORT AMOUNT";
            }

            break;
          case 27:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportCurrentSpousalFrequenc)
              && !
              Equal(local.EiwoB587DetailRecord.SupportCurrentSpousalAmount,
              local.Zero.Text11))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CURRENT SPOUSAL SUPPORT FREQUENCY";
                
            }

            break;
          case 28:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportPastDueSpousalAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAST DUE SPOUSAL SUPPORT AMOUNT";
                
            }

            break;
          case 29:
            if (IsEmpty(local.EiwoB587DetailRecord.SupportPastDueSpousalFrequen) &&
              !
              Equal(local.EiwoB587DetailRecord.SupportPastDueSpousalAmount,
              local.Zero.Text11))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAST DUE SPOUSAL SUPPORT FREQUENCY";
                
            }

            break;
          case 30:
            if (IsEmpty(local.EiwoB587DetailRecord.ObligationOtherAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: OTHER OBLIGATION AMOUNT";
            }

            break;
          case 31:
            if (IsEmpty(local.EiwoB587DetailRecord.ObligationOtherFrequencyCode) &&
              !
              Equal(local.EiwoB587DetailRecord.ObligationOtherAmount,
              local.Zero.Text11))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: OTHER OBLIGATION FREQUENCY";
            }

            break;
          case 32:
            if (IsEmpty(local.EiwoB587DetailRecord.ObligationOtherDescription) &&
              !
              Equal(local.EiwoB587DetailRecord.ObligationOtherAmount,
              local.Zero.Text11))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: OTHER OBLIGATION DESCRIPTION";
            }

            break;
          case 33:
            if (IsEmpty(local.EiwoB587DetailRecord.ObligationTotalAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: OBLIGATION TOTAL AMOUNT";
            }

            break;
          case 34:
            if (IsEmpty(local.EiwoB587DetailRecord.ObligationTotalFrequency) &&
              !
              Equal(local.EiwoB587DetailRecord.ObligationTotalAmount,
              local.Zero.Text11))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: OBLIGATION TOTAL FREQUENCY";
            }

            break;
          case 35:
            if (IsEmpty(local.EiwoB587DetailRecord.IwoDeductionWeeklyAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: IWO DEDUCTION WEEKLY AMOUNT";
            }

            break;
          case 36:
            if (IsEmpty(local.EiwoB587DetailRecord.IwoDeductionBiweeklyAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: IWO DEDUCTION BI-WEEKLY AMOUNT";
            }

            break;
          case 37:
            if (IsEmpty(local.EiwoB587DetailRecord.IwoDeductionSemimonthlyAmount))
              
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: IWO DEDUCTION SEMI-MONTHLY AMOUNT";
                
            }

            break;
          case 38:
            if (IsEmpty(local.EiwoB587DetailRecord.IwoDeductionMonthlyAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: IWO DEDUCTION MONTHLY AMOUNT";
            }

            break;
          case 39:
            if (IsEmpty(local.EiwoB587DetailRecord.BeginWithholdingWithinDays))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: BEGIN WITHHOLDING WITHIN DAYS";
            }

            break;
          case 40:
            break;
          case 41:
            if (IsEmpty(local.EiwoB587DetailRecord.SendPaymentWithhinDays))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: SEND PAYMENT WITHIN DAYS";
            }

            break;
          case 42:
            if (IsEmpty(local.EiwoB587DetailRecord.IwoCcpaPercentRate))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: IWO CCPA PERCENT RATE";
            }

            break;
          case 43:
            if (IsEmpty(local.EiwoB587DetailRecord.PayeeName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAYEE NAME";
            }

            break;
          case 44:
            if (IsEmpty(local.EiwoB587DetailRecord.PayeeAddressLine1))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAYEE ADDRESS LINE 1";
            }

            break;
          case 45:
            if (IsEmpty(local.EiwoB587DetailRecord.PayeeAddressCity))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAYEE ADDRESS CITY";
            }

            break;
          case 46:
            if (IsEmpty(local.EiwoB587DetailRecord.PayeeAddressStateCode))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAYEE ADDRESS STATE";
            }

            break;
          case 47:
            if (IsEmpty(local.EiwoB587DetailRecord.PayeeAddressZipCode))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAYEE ADDRESS ZIP CODE";
            }

            break;
          case 48:
            if (IsEmpty(local.EiwoB587DetailRecord.PayeeRemittanceFipsCode))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: PAYEE REMITTANCE FIPS CODE";
            }

            break;
          case 49:
            if (IsEmpty(local.EiwoB587DetailRecord.GovernmentOfficialName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: GOVERNMENT OFFICIAL NAME";
            }

            break;
          case 50:
            if (IsEmpty(local.EiwoB587DetailRecord.IssuingOfficialTitle))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: ISSUING OFFICIAL TITLE";
            }

            break;
          case 51:
            if (IsEmpty(local.EiwoB587DetailRecord.SendEmployeeCopyIndicator))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: SEND EMPLOYEE COPY INDICATOR";
            }

            break;
          case 52:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployeeStateContactName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYEE STATE CONTACT NAME";
            }

            break;
          case 53:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployeeStateContactPhone))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYEE STATE CONTACT PHONE";
            }

            break;
          case 54:
            if (IsEmpty(local.EiwoB587DetailRecord.DocumentTrackingNumber))
            {
              // -- The document tracking number will be added below.
            }

            break;
          case 55:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployerContactName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYER STATE CONTACT NAME";
            }

            break;
          case 56:
            if (IsEmpty(local.EiwoB587DetailRecord.EmployerContactPhone))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: EMPLOYER STATE CONTACT PHONE";
            }

            break;
          case 57:
            if (IsEmpty(local.EiwoB587DetailRecord.Child1FirstName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 1 FIRST NAME";
            }

            break;
          case 58:
            if (IsEmpty(local.EiwoB587DetailRecord.Child1LastName))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 1 LAST NAME";
            }

            break;
          case 59:
            if (IsEmpty(local.EiwoB587DetailRecord.Child1BirthDate))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 1 BIRTH DATE";
            }

            break;
          case 60:
            if (IsEmpty(local.EiwoB587DetailRecord.Child2FirstName) && AsChar
              (local.Ch2Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 2 FIRST NAME";
            }

            break;
          case 61:
            if (IsEmpty(local.EiwoB587DetailRecord.Child2LastName) && AsChar
              (local.Ch2Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 2 LAST NAME";
            }

            break;
          case 62:
            if (IsEmpty(local.EiwoB587DetailRecord.Child2BirthDate) && AsChar
              (local.Ch2Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 2 BIRTH DATE";
            }

            break;
          case 63:
            if (IsEmpty(local.EiwoB587DetailRecord.Child3FirstName) && AsChar
              (local.Ch3Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 3 FIRST NAME";
            }

            break;
          case 64:
            if (IsEmpty(local.EiwoB587DetailRecord.Child3LastName) && AsChar
              (local.Ch3Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 3 LAST NAME";
            }

            break;
          case 65:
            if (IsEmpty(local.EiwoB587DetailRecord.Child3BirthDate) && AsChar
              (local.Ch3Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 3 BIRTH DATE";
            }

            break;
          case 66:
            if (IsEmpty(local.EiwoB587DetailRecord.Child4FirstName) && AsChar
              (local.Ch4Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 4 FIRST NAME";
            }

            break;
          case 67:
            if (IsEmpty(local.EiwoB587DetailRecord.Child4LastName) && AsChar
              (local.Ch4Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 4 LAST NAME";
            }

            break;
          case 68:
            if (IsEmpty(local.EiwoB587DetailRecord.Child4BirthDate) && AsChar
              (local.Ch4Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 4 BIRTH DATE";
            }

            break;
          case 69:
            if (IsEmpty(local.EiwoB587DetailRecord.Child5FirstName) && AsChar
              (local.Ch5Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 5 FIRST NAME";
            }

            break;
          case 70:
            if (IsEmpty(local.EiwoB587DetailRecord.Child5LastName) && AsChar
              (local.Ch5Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 5 LAST NAME";
            }

            break;
          case 71:
            if (IsEmpty(local.EiwoB587DetailRecord.Child5BirthDate) && AsChar
              (local.Ch5Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 5 BIRTH DATE";
            }

            break;
          case 72:
            if (IsEmpty(local.EiwoB587DetailRecord.Child6FirstName) && AsChar
              (local.Ch6Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 6 FIRST NAME";
            }

            break;
          case 73:
            if (IsEmpty(local.EiwoB587DetailRecord.Child6LastName) && AsChar
              (local.Ch6Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 6 LAST NAME";
            }

            break;
          case 74:
            if (IsEmpty(local.EiwoB587DetailRecord.Child6BirthDate) && AsChar
              (local.Ch6Found.Flag) == 'Y')
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: CHILD 6 BIRTH DATE";
            }

            break;
          case 75:
            if (IsEmpty(local.EiwoB587DetailRecord.LumpSumPaymentAmount))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: LUMP SUM PAYMENT AMOUNT";
            }

            break;
          case 76:
            if (IsEmpty(local.EiwoB587DetailRecord.RemittanceIdentifier))
            {
              local.MissingMandatoryField.Flag = "Y";
              local.EabReportSend.RptDetail =
                "eIWO mandatory field missing: REMITTANCE IDENTIFIER";
            }

            break;
          case 77:
            if (AsChar(local.MissingMandatoryField.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail = "CSP Number " + entities
                .Ncp.Number + " Legal Action " + NumberToString
                (entities.LegalAction.Identifier, 7, 9) + " IWO Transaction " +
                NumberToString(entities.IwoTransaction.Identifier, 13, 3) + " IWO Action " +
                NumberToString(entities.IwoAction.Identifier, 13, 3) + " Infrastructure ID " +
                NumberToString
                (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);
            }

            break;
          case 78:
            if (AsChar(local.MissingMandatoryField.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail =
                "---------------------------------------------------------------------------------------------------------------------------------";
                
            }

            break;
          default:
            break;
        }

        if (!IsEmpty(local.EabReportSend.RptDetail))
        {
          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }
      }

      if (AsChar(local.MissingMandatoryField.Flag) == 'Y')
      {
        ++local.DocsDataError.Count;
        local.Totals.Update.GlocalTotalsDataError.Count =
          local.Totals.Item.GlocalTotalsDataError.Count + 1;

        // -- Set the IWO to error (E) status.
        local.IwoAction.StatusCd = "E";
        UseLeUpdateIwoActionStatus();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage2();

          for(local.Common.Count = 1; local.Common.Count <= 3; ++
            local.Common.Count)
          {
            switch(local.Common.Count)
            {
              case 1:
                local.EabReportSend.RptDetail = "";

                break;
              case 2:
                local.EabReportSend.RptDetail =
                  "(1) Error Update IWO Action Status: " + local
                  .ExitStateWorkArea.Message;

                break;
              case 3:
                local.EabReportSend.RptDetail = "CSP Number " + entities
                  .Ncp.Number + " IWO Transaction " + NumberToString
                  (entities.IwoTransaction.Identifier, 13, 3) + " IWO Action " +
                  NumberToString(entities.IwoAction.Identifier, 13, 3) + " Infrastructure ID " +
                  NumberToString
                  (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);

                break;
              default:
                break;
            }

            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // -- Set the OUTGOING_DOCUMENT print sucessful ind to NO (N).
        local.OutgoingDocument.PrintSucessfulIndicator = "N";
        UseUpdateOutgoingDocument();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage2();

          for(local.Common.Count = 1; local.Common.Count <= 3; ++
            local.Common.Count)
          {
            switch(local.Common.Count)
            {
              case 1:
                local.EabReportSend.RptDetail = "";

                break;
              case 2:
                local.EabReportSend.RptDetail =
                  "(1) Error Updating Outgoing Document Status: " + local
                  .ExitStateWorkArea.Message;

                break;
              case 3:
                local.EabReportSend.RptDetail = "CSP Number " + entities
                  .Ncp.Number + " IWO Transaction " + NumberToString
                  (entities.IwoTransaction.Identifier, 13, 3) + " IWO Action " +
                  NumberToString(entities.IwoAction.Identifier, 13, 3) + " Infrastructure ID " +
                  NumberToString
                  (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);

                break;
              default:
                break;
            }

            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // -- Update message on the infrastructure record.
        local.Infrastructure.Detail = "eIWO Failed. " + (
          local.EiwoStatusMessage.Detail ?? "");
        UseSpCabUpdateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage2();

          for(local.Common.Count = 1; local.Common.Count <= 3; ++
            local.Common.Count)
          {
            switch(local.Common.Count)
            {
              case 1:
                local.EabReportSend.RptDetail = "";

                break;
              case 2:
                local.EabReportSend.RptDetail =
                  "(1) Error Updating Infrastructure Detail Message: " + local
                  .ExitStateWorkArea.Message;

                break;
              case 3:
                local.EabReportSend.RptDetail = "CSP Number " + entities
                  .Ncp.Number + " IWO Transaction " + NumberToString
                  (entities.IwoTransaction.Identifier, 13, 3) + " IWO Action " +
                  NumberToString(entities.IwoAction.Identifier, 13, 3) + " Infrastructure ID " +
                  NumberToString
                  (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);

                break;
              default:
                break;
            }

            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        continue;
      }

      // -------------------------------------------------------------------------------------
      // -- Determine if Batch Header/Footer need to be written to file.
      // -------------------------------------------------------------------------------------
      if (!Equal(entities.Employer.Ein, local.PreviousEmployer.Ein))
      {
        if (!IsEmpty(local.PreviousEmployer.Ein))
        {
          // -------------------------------------------------------------------------------------
          // -- Write Batch Trailer (BTI) record.
          // -------------------------------------------------------------------------------------
          local.EabFileHandling.Action = "TRAILER";
          local.BatchEiwoB587TrailerRecord.DocumentCode = "BTI";

          // -- Create batch control number in Feds recommended format 
          // 20000YYMMDDHHMMSSxxxx
          local.BatchEiwoB587TrailerRecord.ControlNumber =
            local.BatchEiwoB587HeaderRecord.ControlNumber;
          local.BatchEiwoB587TrailerRecord.BatchCount = "00000";
          local.BatchEiwoB587TrailerRecord.RecordCount =
            NumberToString(local.RecordCount.Count, 11, 5);
          local.BatchEiwoB587TrailerRecord.EmployerSentCount = "00000";
          local.BatchEiwoB587TrailerRecord.StateSentCount = "00000";
          local.BatchEiwoB587TrailerRecord.ErrorFieldName = "";
          UseLeB587WriteFile3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing batch trailer record.  Return status = " + local
              .EabFileHandling.Status;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.TotalNumbOfEinsWritten.Count;
        }

        local.RecordCount.Count = 0;
        ++local.BatchCount.Count;

        // -------------------------------------------------------------------------------------
        // -- Write Batch Header (BHI) record.
        // -------------------------------------------------------------------------------------
        local.EabFileHandling.Action = "HEADER";
        local.BatchEiwoB587HeaderRecord.DocumentCode = "BHI";

        // -- Create batch control number in Feds recommended format 
        // 20000YYMMDDHHMMSSxxxx
        local.BatchEiwoB587HeaderRecord.ControlNumber =
          Substring(local.FileEiwoB587HeaderRecord.ControlNumber,
          EiwoB587HeaderRecord.ControlNumber_MaxLength, 1, 17) + NumberToString
          (local.BatchCount.Count, 12, 4);
        local.BatchEiwoB587HeaderRecord.StateFipsCode = "20000";
        local.BatchEiwoB587HeaderRecord.Ein = entities.Employer.Ein ?? Spaces
          (9);
        local.BatchEiwoB587HeaderRecord.PrimaryEin = "";

        // -- Set creation date and time below.  (formats CCYYMMDD and HHMMSS)
        local.BatchEiwoB587HeaderRecord.CreationDate =
          local.FileEiwoB587HeaderRecord.CreationDate;
        local.BatchEiwoB587HeaderRecord.CreationTime =
          local.FileEiwoB587HeaderRecord.CreationTime;
        local.FileEiwoB587HeaderRecord.ErrorFieldName = "";
        UseLeB587WriteFile4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing batch header record.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.PreviousEmployer.Ein = entities.Employer.Ein;
      }

      // -- Create the document tracking number.
      ReadIwoAction();

      if (IsEmpty(local.Next.DocumentTrackingIdentifier))
      {
        local.Next.DocumentTrackingIdentifier = "000000000001";
      }
      else
      {
        local.Next.DocumentTrackingIdentifier =
          NumberToString(StringToNumber(local.Next.DocumentTrackingIdentifier) +
          1, 4, 12);
      }

      local.EiwoB587DetailRecord.DocumentTrackingNumber =
        "200000000000000000" + (local.Next.DocumentTrackingIdentifier ?? "");

      // -- Write detail record to file.
      local.EabFileHandling.Action = "DETAIL";
      UseLeB587WriteFile1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage2();

        for(local.Common.Count = 1; local.Common.Count <= 3; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail = "";

              break;
            case 2:
              local.EabReportSend.RptDetail =
                "(2) Error Writing Detail Record.  Return Status: " + local
                .EabFileHandling.Status;

              break;
            case 3:
              local.EabReportSend.RptDetail = "CSP Number " + entities
                .Ncp.Number + " IWO Transaction " + NumberToString
                (entities.IwoTransaction.Identifier, 13, 3) + " IWO Action " + NumberToString
                (entities.IwoAction.Identifier, 13, 3) + " Infrastructure ID " +
                NumberToString
                (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);

              break;
            default:
              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Set the IWO to Sent to Portal (N) status.
      local.IwoAction.StatusCd = "N";
      local.IwoAction.DocumentTrackingIdentifier =
        local.Next.DocumentTrackingIdentifier ?? "";
      local.IwoAction.FileControlId =
        local.FileEiwoB587HeaderRecord.ControlNumber;
      local.IwoAction.BatchControlId =
        local.BatchEiwoB587HeaderRecord.ControlNumber;
      UseLeUpdateIwoActionStatus();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage2();

        for(local.Common.Count = 1; local.Common.Count <= 3; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail = "";

              break;
            case 2:
              local.EabReportSend.RptDetail =
                "(2) Error Update IWO Action Status: " + local
                .ExitStateWorkArea.Message;

              break;
            case 3:
              local.EabReportSend.RptDetail = "CSP Number " + entities
                .Ncp.Number + " IWO Transaction " + NumberToString
                (entities.IwoTransaction.Identifier, 13, 3) + " IWO Action " + NumberToString
                (entities.IwoAction.Identifier, 13, 3) + " Infrastructure ID " +
                NumberToString
                (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);

              break;
            default:
              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Set the OUTGOING_DOCUMENT print sucessful ind to Processed to Portal
      // (E).
      local.OutgoingDocument.PrintSucessfulIndicator = "E";
      UseUpdateOutgoingDocument();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage2();

        for(local.Common.Count = 1; local.Common.Count <= 3; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail = "";

              break;
            case 2:
              local.EabReportSend.RptDetail =
                "(2) Error Updating Outgoing Document Status: " + local
                .ExitStateWorkArea.Message;

              break;
            case 3:
              local.EabReportSend.RptDetail = "CSP Number " + entities
                .Ncp.Number + " IWO Transaction " + NumberToString
                (entities.IwoTransaction.Identifier, 13, 3) + " IWO Action " + NumberToString
                (entities.IwoAction.Identifier, 13, 3) + " Infrastructure ID " +
                NumberToString
                (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);

              break;
            default:
              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -- Update message on the infrastructure record.
      local.Infrastructure.Detail = "eIWO Sent. " + (
        local.EiwoStatusMessage.Detail ?? "");
      UseSpCabUpdateInfrastructure();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage2();

        for(local.Common.Count = 1; local.Common.Count <= 3; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail = "";

              break;
            case 2:
              local.EabReportSend.RptDetail =
                "(2) Error Updating Infrastructure Detail Message: " + local
                .ExitStateWorkArea.Message;

              break;
            case 3:
              local.EabReportSend.RptDetail = "CSP Number " + entities
                .Ncp.Number + " IWO Transaction " + NumberToString
                (entities.IwoTransaction.Identifier, 13, 3) + " IWO Action " + NumberToString
                (entities.IwoAction.Identifier, 13, 3) + " Infrastructure ID " +
                NumberToString
                (entities.Infrastructure.SystemGeneratedIdentifier, 7, 9);

              break;
            default:
              break;
          }

          local.EabFileHandling.Action = "WRITE";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.RecordCount.Count;
      ++local.NumbOfNcpsSinceChckpnt.Count;
      ++local.TotalNumbOfNcpsWritten.Count;
      ++local.DocsProcessed.Count;
      local.Totals.Update.GlocalTotalsProcessed.Count =
        local.Totals.Item.GlocalTotalsProcessed.Count + 1;

      if (AsChar(local.DebugOn.Flag) == 'Y')
      {
        local.EabConvertNumeric.SendAmount =
          NumberToString(local.Infrastructure.SystemGeneratedIdentifier, 15);
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
          (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  Successfully Printed      -- Document Name = " +
          TrimEnd(local.Document.Name) + ", USERID = " + TrimEnd
          (local.Infrastructure.UserId);
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }

      // -- File Commit and Checkpoint processing.
      if (local.NumbOfNcpsSinceChckpnt.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // --  COMMIT changes to the Output File.
        local.EabFileHandling.Action = "COMMIT";
        UseLeB587WriteFile7();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error on COMMIT for output file.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // -- Checkpoint.
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	Position  Description
        // 	--------  
        // -----------------------------------------
        // 	001-022   File Control Number (22)
        // 	023-023   Blank
        // 	024-028   Batch Count (5)
        // 	029-029   Blank
        // 	030-051   Batch Control Number (22)
        // 	052-052   Blank
        // 	053-057   Record Count (5)
        // 	058-058   Blank
        // 	059-067   Last EIN Processed (9)
        // 	068-068   Blank
        // 	069-078   Last NCP Number Processed (10)
        // 	079-079   Blank
        // 	080-088   Total Number of EINs Written to File (9)
        // 	089-089   Blank
        // 	090-098   Total Number of NCPs Written to File (9)
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          local.FileEiwoB587HeaderRecord.ControlNumber + " " + NumberToString
          (local.BatchCount.Count, 11, 5) + " " + local
          .BatchEiwoB587HeaderRecord.ControlNumber + " " + NumberToString
          (local.RecordCount.Count, 11, 5) + " " + entities.Employer.Ein + " " +
          entities.Ncp.Number + " " + NumberToString
          (local.TotalNumbOfEinsWritten.Count, 7, 9) + " " + NumberToString
          (local.TotalNumbOfNcpsWritten.Count, 7, 9);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking checkpoint.";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.NumbOfNcpsSinceChckpnt.Count = 0;
      }
    }

ReadEach:

    if (!IsEmpty(local.PreviousEmployer.Ein))
    {
      // -------------------------------------------------------------------------------------
      // -- Write Batch Trailer (BTI) record.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "TRAILER";
      local.BatchEiwoB587TrailerRecord.DocumentCode = "BTI";

      // -- Create batch control number in Feds recommended format 
      // 20000YYMMDDHHMMSSxxxx
      local.BatchEiwoB587TrailerRecord.ControlNumber =
        local.BatchEiwoB587HeaderRecord.ControlNumber;
      local.BatchEiwoB587TrailerRecord.BatchCount = "00000";
      local.BatchEiwoB587TrailerRecord.RecordCount =
        NumberToString(local.RecordCount.Count, 11, 5);
      local.BatchEiwoB587TrailerRecord.EmployerSentCount = "00000";
      local.BatchEiwoB587TrailerRecord.StateSentCount = "00000";
      local.BatchEiwoB587TrailerRecord.ErrorFieldName = "";
      UseLeB587WriteFile3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing batch trailer record.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.TotalNumbOfEinsWritten.Count;
    }

    // -------------------------------------------------------------------------------------
    // -- Write File Trailer (FTI) record.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "TRAILER";
    local.FileEiwoB587TrailerRecord.DocumentCode = "FTI";
    local.FileEiwoB587TrailerRecord.ControlNumber =
      local.FileEiwoB587HeaderRecord.ControlNumber;
    local.FileEiwoB587TrailerRecord.BatchCount =
      NumberToString(local.BatchCount.Count, 11, 5);
    local.FileEiwoB587TrailerRecord.RecordCount = "00000";
    local.FileEiwoB587TrailerRecord.EmployerSentCount = "00000";
    local.FileEiwoB587TrailerRecord.StateSentCount = "00000";
    local.FileEiwoB587TrailerRecord.ErrorFieldName = "";
    UseLeB587WriteFile5();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing file trailer record.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Do a final Commit to the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "COMMIT";
    UseLeB587WriteFile7();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error on final COMMIT for output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

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
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Number of NCPs Written to the Outgoing eIWO File.................." +
            NumberToString(local.TotalNumbOfNcpsWritten.Count, 9, 7);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Number of Batches (EINs) Written to the Outgoing eIWO File........" +
            NumberToString(local.TotalNumbOfEinsWritten.Count, 9, 7);

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(02) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -- Use this cab to write the B715 summary info.
    UseSpB715WriteControlsAndClose();

    // -------------------------------------------------------------------------------------
    // --  Close the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeB587WriteFile7();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close Adabas.
    // ---------------------------------------------------------------------------
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDocument1(Document source, Document target)
  {
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveDocument2(Document source, Document target)
  {
    target.Name = source.Name;
    target.VersionNumber = source.VersionNumber;
  }

  private static void MoveEabConvertNumeric1(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnAmountNonDecimalSigned = source.ReturnAmountNonDecimalSigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
  }

  private static void MoveEabConvertNumeric3(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Dependancy = source.Dependancy;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
    target.CaseUnitState = source.CaseUnitState;
  }

  private static void MoveInfrastructure3(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveInfrastructure4(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveIwoAction(IwoAction source, IwoAction target)
  {
    target.Identifier = source.Identifier;
    target.StatusCd = source.StatusCd;
    target.DocumentTrackingIdentifier = source.DocumentTrackingIdentifier;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveOfficeServiceProviderAlert(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.TypeCode = source.TypeCode;
    target.Message = source.Message;
    target.Description = source.Description;
    target.DistributionDate = source.DistributionDate;
    target.SituationIdentifier = source.SituationIdentifier;
    target.PrioritizationCode = source.PrioritizationCode;
    target.OptimizationInd = source.OptimizationInd;
    target.OptimizedFlag = source.OptimizedFlag;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
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

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAdminAction = source.KeyAdminAction;
    target.KeyAdminActionCert = source.KeyAdminActionCert;
    target.KeyAdminAppeal = source.KeyAdminAppeal;
    target.KeyAp = source.KeyAp;
    target.KeyAppointment = source.KeyAppointment;
    target.KeyAr = source.KeyAr;
    target.KeyBankruptcy = source.KeyBankruptcy;
    target.KeyCase = source.KeyCase;
    target.KeyCashRcptDetail = source.KeyCashRcptDetail;
    target.KeyCashRcptEvent = source.KeyCashRcptEvent;
    target.KeyCashRcptSource = source.KeyCashRcptSource;
    target.KeyCashRcptType = source.KeyCashRcptType;
    target.KeyChild = source.KeyChild;
    target.KeyContact = source.KeyContact;
    target.KeyGeneticTest = source.KeyGeneticTest;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
    target.KeyIncarceration = source.KeyIncarceration;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyInfoRequest = source.KeyInfoRequest;
    target.KeyInterstateRequest = source.KeyInterstateRequest;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyLegalActionDetail = source.KeyLegalActionDetail;
    target.KeyLegalReferral = source.KeyLegalReferral;
    target.KeyMilitaryService = source.KeyMilitaryService;
    target.KeyObligation = source.KeyObligation;
    target.KeyObligationAdminAction = source.KeyObligationAdminAction;
    target.KeyObligationType = source.KeyObligationType;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyPersonAddress = source.KeyPersonAddress;
    target.KeyRecaptureRule = source.KeyRecaptureRule;
    target.KeyResource = source.KeyResource;
    target.KeyTribunal = source.KeyTribunal;
    target.KeyWorkerComp = source.KeyWorkerComp;
    target.KeyWorksheet = source.KeyWorksheet;
  }

  private static void MoveTotals(Local.TotalsGroup source,
    SpB715WriteControlsAndClose.Import.DocumentTotalsGroup target)
  {
    MoveDocument2(source.GlocalTotals, target.G);
    target.GimportDataError.Count = source.GlocalTotalsDataError.Count;
    target.GimportException.Count = source.GlocalTotalsException.Count;
    target.GimportFuture.Count = source.GlocalTotalsFuture.Count;
    target.GimportProcessed.Count = source.GlocalTotalsProcessed.Count;
    target.GimportRead.Count = source.GlocalTotalsRead.Count;
    target.GimportSystemError.Count = source.GlocalTotalsSystemError.Count;
    target.GimportWarning.Count = source.GlocalTotalsWarning.Count;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric3(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    MoveEabConvertNumeric1(local.EabConvertNumeric, useExport.EabConvertNumeric);
      

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric1(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseEabExtractExitStateMessage1()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private string UseEabExtractExitStateMessage2()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseLeB587RemoveInvalidChars1()
  {
    var useImport = new LeB587RemoveInvalidChars.Import();
    var useExport = new LeB587RemoveInvalidChars.Export();

    useImport.FieldValue.Value = local.Trimmed.Value;

    Call(LeB587RemoveInvalidChars.Execute, useImport, useExport);

    local.FirstCharCleaned.Value = useExport.FirstCharCleaned.Value;
    local.AllCharCleaned.Value = useExport.AllCharCleaned.Value;
  }

  private void UseLeB587RemoveInvalidChars2()
  {
    var useImport = new LeB587RemoveInvalidChars.Import();
    var useExport = new LeB587RemoveInvalidChars.Export();

    useImport.FieldValue.Value = local.Trimmed.Value;

    Call(LeB587RemoveInvalidChars.Execute, useImport, useExport);

    local.FirstCharCleaned.Value = useExport.FirstCharCleaned.Value;
  }

  private string UseLeB587TrimLeadingSpaces()
  {
    var useImport = new LeB587TrimLeadingSpaces.Import();
    var useExport = new LeB587TrimLeadingSpaces.Export();

    useImport.FieldValue.Value = entities.FieldValue.Value;

    Call(LeB587TrimLeadingSpaces.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private void UseLeB587WriteFile1()
  {
    var useImport = new LeB587WriteFile.Import();
    var useExport = new LeB587WriteFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EiwoB587DetailRecord.Assign(local.EiwoB587DetailRecord);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB587WriteFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB587WriteFile2()
  {
    var useImport = new LeB587WriteFile.Import();
    var useExport = new LeB587WriteFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.ExtendedFileEiwoB587HeaderRecord.ControlNumber =
      local.ExtendedFileEiwoB587HeaderRecord.ControlNumber;
    useExport.ExtendedFileEiwoB587TrailerRecord.BatchCount =
      local.ExtendedFileEiwoB587TrailerRecord.BatchCount;
    useExport.ExtendedBatchEiwoB587HeaderRecord.ControlNumber =
      local.ExtendedBatchEiwoB587HeaderRecord.ControlNumber;
    useExport.ExtendedBatchEiwoB587TrailerRecord.RecordCount =
      local.ExtendedBatchEiwoB587TrailerRecord.RecordCount;

    Call(LeB587WriteFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.ExtendedFileEiwoB587HeaderRecord.ControlNumber =
      useExport.ExtendedFileEiwoB587HeaderRecord.ControlNumber;
    local.ExtendedFileEiwoB587TrailerRecord.BatchCount =
      useExport.ExtendedFileEiwoB587TrailerRecord.BatchCount;
    local.ExtendedBatchEiwoB587HeaderRecord.ControlNumber =
      useExport.ExtendedBatchEiwoB587HeaderRecord.ControlNumber;
    local.ExtendedBatchEiwoB587TrailerRecord.RecordCount =
      useExport.ExtendedBatchEiwoB587TrailerRecord.RecordCount;
  }

  private void UseLeB587WriteFile3()
  {
    var useImport = new LeB587WriteFile.Import();
    var useExport = new LeB587WriteFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EiwoB587TrailerRecord.Assign(local.BatchEiwoB587TrailerRecord);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB587WriteFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB587WriteFile4()
  {
    var useImport = new LeB587WriteFile.Import();
    var useExport = new LeB587WriteFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EiwoB587HeaderRecord.Assign(local.BatchEiwoB587HeaderRecord);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB587WriteFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB587WriteFile5()
  {
    var useImport = new LeB587WriteFile.Import();
    var useExport = new LeB587WriteFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EiwoB587TrailerRecord.Assign(local.FileEiwoB587TrailerRecord);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB587WriteFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB587WriteFile6()
  {
    var useImport = new LeB587WriteFile.Import();
    var useExport = new LeB587WriteFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EiwoB587HeaderRecord.Assign(local.FileEiwoB587HeaderRecord);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB587WriteFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB587WriteFile7()
  {
    var useImport = new LeB587WriteFile.Import();
    var useExport = new LeB587WriteFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB587WriteFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    local.BatchTimestampWorkArea.Assign(useExport.BatchTimestampWorkArea);
  }

  private void UseLeUpdateIwoActionStatus()
  {
    var useImport = new LeUpdateIwoActionStatus.Import();
    var useExport = new LeUpdateIwoActionStatus.Export();

    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;
    useImport.IwoTransaction.Identifier = entities.IwoTransaction.Identifier;
    useImport.CsePerson.Number = entities.Ncp.Number;
    useImport.IwoAction.Assign(local.IwoAction);

    Call(LeUpdateIwoActionStatus.Execute, useImport, useExport);
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

  private void UseSpB715WriteControlsAndClose()
  {
    var useImport = new SpB715WriteControlsAndClose.Import();
    var useExport = new SpB715WriteControlsAndClose.Export();

    useImport.DocsSystemError.Count = local.DocsSystemError.Count;
    local.Totals.CopyTo(useImport.DocumentTotals, MoveTotals);
    useImport.DocsWarning.Count = local.DocsWarning.Count;
    useImport.DocsDataError.Count = local.DocsDataError.Count;
    useImport.DocsProcessed.Count = local.DocsProcessed.Count;
    useImport.DocsRead.Count = local.DocsRead.Count;

    Call(SpB715WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCabCreateOfcSrvPrvdAlert()
  {
    var useImport = new SpCabCreateOfcSrvPrvdAlert.Import();
    var useExport = new SpCabCreateOfcSrvPrvdAlert.Export();

    useImport.Alerts.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    useImport.OfficeServiceProviderAlert.
      Assign(local.OfficeServiceProviderAlert);
    useImport.ServiceProvider.SystemGeneratedId =
      local.ServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId = local.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(local.OfficeServiceProvider,
      useImport.OfficeServiceProvider);

    Call(SpCabCreateOfcSrvPrvdAlert.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateInfrastructure()
  {
    var useImport = new SpCabUpdateInfrastructure.Import();
    var useExport = new SpCabUpdateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabUpdateInfrastructure.Execute, useImport, useExport);
  }

  private void UseSpDocGetServiceProvider()
  {
    var useImport = new SpDocGetServiceProvider.Import();
    var useExport = new SpDocGetServiceProvider.Export();

    useImport.Current.Date = local.DateWorkArea.Date;
    useImport.Document.BusinessObject = local.Document.BusinessObject;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpDocGetServiceProvider.Execute, useImport, useExport);

    local.OutDocRtrnAddr.Assign(useExport.OutDocRtrnAddr);
  }

  private void UseSpDocUpdateFailedBatchDoc()
  {
    var useImport = new SpDocUpdateFailedBatchDoc.Import();
    var useExport = new SpDocUpdateFailedBatchDoc.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;
    useImport.Document.Assign(local.Document);
    useImport.Message.Text50 = local.WorkArea.Text50;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Monitored.Assign(local.Monitored);
    useImport.UnMonitored.Assign(local.UnMonitored);

    Call(SpDocUpdateFailedBatchDoc.Execute, useImport, useExport);
  }

  private void UseSpDocUpdateSuccessfulPrint()
  {
    var useImport = new SpDocUpdateSuccessfulPrint.Import();
    var useExport = new SpDocUpdateSuccessfulPrint.Export();

    MoveInfrastructure3(local.Infrastructure, useImport.Infrastructure);

    Call(SpDocUpdateSuccessfulPrint.Execute, useImport, useExport);
  }

  private void UseSpPrintDataRetrievalKeys1()
  {
    var useImport = new SpPrintDataRetrievalKeys.Import();
    var useExport = new SpPrintDataRetrievalKeys.Export();

    useImport.Document.Assign(local.Document);
    MoveInfrastructure4(local.Infrastructure, useImport.Infrastructure);
    MoveField(local.Field, useImport.Field);

    Call(SpPrintDataRetrievalKeys.Execute, useImport, useExport);

    local.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    local.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    local.SpDocKey.Assign(useExport.SpDocKey);
  }

  private void UseSpPrintDataRetrievalKeys2()
  {
    var useImport = new SpPrintDataRetrievalKeys.Import();
    var useExport = new SpPrintDataRetrievalKeys.Export();

    useImport.Document.Assign(local.Document);
    MoveInfrastructure4(local.Infrastructure, useImport.Infrastructure);
    MoveField(local.Field, useImport.Field);

    Call(SpPrintDataRetrievalKeys.Execute, useImport, useExport);

    local.SpDocKey.Assign(useExport.SpDocKey);
  }

  private void UseSpPrintDataRetrievalMain()
  {
    var useImport = new SpPrintDataRetrievalMain.Import();
    var useExport = new SpPrintDataRetrievalMain.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      entities.Infrastructure.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo, useImport.Batch);
    MoveDocument1(local.Document, useImport.Document);
    useImport.ExpImpRowLockFieldValue.Count = local.RowLockFieldValue.Count;

    Call(SpPrintDataRetrievalMain.Execute, useImport, useExport);

    local.RowLockFieldValue.Count = useImport.ExpImpRowLockFieldValue.Count;
    local.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    local.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    local.ErrorInd.Flag = useExport.ErrorInd.Flag;
    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.WorkArea.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void UseUpdateOutgoingDocument()
  {
    var useImport = new UpdateOutgoingDocument.Import();
    var useExport = new UpdateOutgoingDocument.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      entities.Infrastructure.SystemGeneratedIdentifier;
    useImport.OutgoingDocument.Assign(local.OutgoingDocument);

    Call(UpdateOutgoingDocument.Execute, useImport, useExport);
  }

  private bool ReadAlert1()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert1",
      null,
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.Populated = true;
      });
  }

  private bool ReadAlert2()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert2",
      null,
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.Populated = true;
      });
  }

  private bool ReadAlert3()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert3",
      null,
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "statusDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePersonsWorkSet.Number);
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

  private IEnumerable<bool> ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
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

        return true;
      });
  }

  private bool ReadCode()
  {
    entities.Code.Populated = false;

    return Read("ReadCode",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.Code.Id = db.GetInt32(reader, 0);
        entities.Code.CodeName = db.GetString(reader, 1);
        entities.Code.EffectiveDate = db.GetDate(reader, 2);
        entities.Code.ExpirationDate = db.GetDate(reader, 3);
        entities.Code.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return ReadEach("ReadCodeValue",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableInt32(command, "codId", entities.Code.Id);
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;

        return true;
      });
  }

  private bool ReadDocumentEventDetail()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.EventDetail.Populated = false;
    entities.Document.Populated = false;

    return Read("ReadDocumentEventDetail",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.OutgoingDocument.DocEffectiveDte.GetValueOrDefault());
        db.SetString(command, "name", entities.OutgoingDocument.DocName ?? "");
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.BusinessObject = db.GetString(reader, 1);
        entities.Document.RequiredResponseDays = db.GetInt32(reader, 2);
        entities.Document.EveNo = db.GetNullableInt32(reader, 3);
        entities.EventDetail.EveNo = db.GetInt32(reader, 3);
        entities.Document.EvdId = db.GetNullableInt32(reader, 4);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.Document.VersionNumber = db.GetString(reader, 6);
        entities.EventDetail.ExceptionRoutine = db.GetNullableString(reader, 7);
        entities.EventDetail.Populated = true;
        entities.Document.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDocumentField1()
  {
    entities.DocumentField.Populated = false;

    return ReadEach("ReadDocumentField1",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.FldName = db.GetString(reader, 2);
        entities.DocumentField.DocName = db.GetString(reader, 3);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 4);
        entities.DocumentField.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDocumentField2()
  {
    entities.DocumentField.Populated = false;

    return ReadEach("ReadDocumentField2",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.FldName = db.GetString(reader, 2);
        entities.DocumentField.DocName = db.GetString(reader, 3);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 4);
        entities.DocumentField.Populated = true;

        return true;
      });
  }

  private bool ReadField()
  {
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    entities.Field.Populated = false;

    return Read("ReadField",
      (db, command) =>
      {
        db.SetString(command, "name", entities.DocumentField.FldName);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.Populated = true;
      });
  }

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.DocumentField.DocEffectiveDte.GetValueOrDefault());
        db.SetString(command, "docName", entities.DocumentField.DocName);
        db.SetString(command, "fldName", entities.DocumentField.FldName);
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.LocationDescription = db.GetNullableString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddressFips1()
  {
    entities.SduFips.Populated = false;
    entities.SduFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddressFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", entities.Fips.State);
      },
      (db, reader) =>
      {
        entities.SduFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.SduFipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.SduFipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.SduFipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.SduFipsTribAddress.City = db.GetString(reader, 4);
        entities.SduFipsTribAddress.State = db.GetString(reader, 5);
        entities.SduFipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.SduFipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.SduFipsTribAddress.FipState = db.GetNullableInt32(reader, 8);
        entities.SduFips.State = db.GetInt32(reader, 8);
        entities.SduFipsTribAddress.FipCounty = db.GetNullableInt32(reader, 9);
        entities.SduFips.County = db.GetInt32(reader, 9);
        entities.SduFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 10);
        entities.SduFips.Location = db.GetInt32(reader, 10);
        entities.SduFips.LocationDescription = db.GetNullableString(reader, 11);
        entities.SduFips.Populated = true;
        entities.SduFipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddressFips2()
  {
    entities.SduFips.Populated = false;
    entities.SduFipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddressFips2",
      null,
      (db, reader) =>
      {
        entities.SduFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.SduFipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.SduFipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.SduFipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.SduFipsTribAddress.City = db.GetString(reader, 4);
        entities.SduFipsTribAddress.State = db.GetString(reader, 5);
        entities.SduFipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.SduFipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.SduFipsTribAddress.FipState = db.GetNullableInt32(reader, 8);
        entities.SduFips.State = db.GetInt32(reader, 8);
        entities.SduFipsTribAddress.FipCounty = db.GetNullableInt32(reader, 9);
        entities.SduFips.County = db.GetInt32(reader, 9);
        entities.SduFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 10);
        entities.SduFips.Location = db.GetInt32(reader, 10);
        entities.SduFips.LocationDescription = db.GetNullableString(reader, 11);
        entities.SduFips.Populated = true;
        entities.SduFipsTribAddress.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.
          SetInt32(command, "systemGeneratedI", entities.OutgoingDocument.InfId);
          
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 3);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 4);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 5);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 6);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 8);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 9);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 10);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 11);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 12);
        entities.Infrastructure.UserId = db.GetString(reader, 13);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 16);
        entities.Infrastructure.Function = db.GetNullableString(reader, 17);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 18);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 19);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadIwoAction()
  {
    local.Next.Populated = false;

    return Read("ReadIwoAction",
      null,
      (db, reader) =>
      {
        local.Next.DocumentTrackingIdentifier = db.GetNullableString(reader, 0);
        local.Next.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIwoActionOutgoingDocumentEmployerCsePerson()
  {
    entities.IwoTransaction.Populated = false;
    entities.Employer.Populated = false;
    entities.OutgoingDocument.Populated = false;
    entities.IwoAction.Populated = false;
    entities.Ncp.Populated = false;

    return ReadEach("ReadIwoActionOutgoingDocumentEmployerCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", local.RestartEmployer.Ein ?? "");
        db.SetString(command, "numb", local.RestartCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 3);
        entities.IwoAction.CspNumber = db.GetString(reader, 4);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 4);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 6);
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 6);
        entities.IwoAction.InfId = db.GetNullableInt32(reader, 7);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 7);
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 8);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 10);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 11);
        entities.Employer.Identifier = db.GetInt32(reader, 12);
        entities.Employer.Ein = db.GetNullableString(reader, 13);
        entities.Ncp.Number = db.GetString(reader, 14);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 14);
        entities.Ncp.Type1 = db.GetString(reader, 15);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 16);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 17);
        entities.IwoTransaction.Populated = true;
        entities.Employer.Populated = true;
        entities.OutgoingDocument.Populated = true;
        entities.IwoAction.Populated = true;
        entities.Ncp.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ncp.Type1);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId", entities.IwoTransaction.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "statusDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 8);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralAssignment.Populated = false;

    return ReadEach("ReadLegalReferralAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 6);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 7);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 8);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 6);
        entities.Office.EffectiveDate = db.GetDate(reader, 7);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.LegalReferralAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.LegalReferralAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.LegalReferralAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 6);
        entities.Office.EffectiveDate = db.GetDate(reader, 7);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.ServiceProvider.Populated = true;
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
    /// <summary>A TotalsGroup group.</summary>
    [Serializable]
    public class TotalsGroup
    {
      /// <summary>
      /// A value of GlocalTotals.
      /// </summary>
      [JsonPropertyName("glocalTotals")]
      public Document GlocalTotals
      {
        get => glocalTotals ??= new();
        set => glocalTotals = value;
      }

      /// <summary>
      /// A value of GlocalTotalsDataError.
      /// </summary>
      [JsonPropertyName("glocalTotalsDataError")]
      public Common GlocalTotalsDataError
      {
        get => glocalTotalsDataError ??= new();
        set => glocalTotalsDataError = value;
      }

      /// <summary>
      /// A value of GlocalTotalsException.
      /// </summary>
      [JsonPropertyName("glocalTotalsException")]
      public Common GlocalTotalsException
      {
        get => glocalTotalsException ??= new();
        set => glocalTotalsException = value;
      }

      /// <summary>
      /// A value of GlocalTotalsFuture.
      /// </summary>
      [JsonPropertyName("glocalTotalsFuture")]
      public Common GlocalTotalsFuture
      {
        get => glocalTotalsFuture ??= new();
        set => glocalTotalsFuture = value;
      }

      /// <summary>
      /// A value of GlocalTotalsProcessed.
      /// </summary>
      [JsonPropertyName("glocalTotalsProcessed")]
      public Common GlocalTotalsProcessed
      {
        get => glocalTotalsProcessed ??= new();
        set => glocalTotalsProcessed = value;
      }

      /// <summary>
      /// A value of GlocalTotalsRead.
      /// </summary>
      [JsonPropertyName("glocalTotalsRead")]
      public Common GlocalTotalsRead
      {
        get => glocalTotalsRead ??= new();
        set => glocalTotalsRead = value;
      }

      /// <summary>
      /// A value of GlocalTotalsSystemError.
      /// </summary>
      [JsonPropertyName("glocalTotalsSystemError")]
      public Common GlocalTotalsSystemError
      {
        get => glocalTotalsSystemError ??= new();
        set => glocalTotalsSystemError = value;
      }

      /// <summary>
      /// A value of GlocalTotalsWarning.
      /// </summary>
      [JsonPropertyName("glocalTotalsWarning")]
      public Common GlocalTotalsWarning
      {
        get => glocalTotalsWarning ??= new();
        set => glocalTotalsWarning = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document glocalTotals;
      private Common glocalTotalsDataError;
      private Common glocalTotalsException;
      private Common glocalTotalsFuture;
      private Common glocalTotalsProcessed;
      private Common glocalTotalsRead;
      private Common glocalTotalsSystemError;
      private Common glocalTotalsWarning;
    }

    /// <summary>
    /// A value of Ch5Found.
    /// </summary>
    [JsonPropertyName("ch5Found")]
    public Common Ch5Found
    {
      get => ch5Found ??= new();
      set => ch5Found = value;
    }

    /// <summary>
    /// A value of Ch4Found.
    /// </summary>
    [JsonPropertyName("ch4Found")]
    public Common Ch4Found
    {
      get => ch4Found ??= new();
      set => ch4Found = value;
    }

    /// <summary>
    /// A value of Ch3Found.
    /// </summary>
    [JsonPropertyName("ch3Found")]
    public Common Ch3Found
    {
      get => ch3Found ??= new();
      set => ch3Found = value;
    }

    /// <summary>
    /// A value of Ch6Found.
    /// </summary>
    [JsonPropertyName("ch6Found")]
    public Common Ch6Found
    {
      get => ch6Found ??= new();
      set => ch6Found = value;
    }

    /// <summary>
    /// A value of Ch2Found.
    /// </summary>
    [JsonPropertyName("ch2Found")]
    public Common Ch2Found
    {
      get => ch2Found ??= new();
      set => ch2Found = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public FieldValue Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of EmployerName.
    /// </summary>
    [JsonPropertyName("employerName")]
    public WorkArea EmployerName
    {
      get => employerName ??= new();
      set => employerName = value;
    }

    /// <summary>
    /// A value of EmployerLocation.
    /// </summary>
    [JsonPropertyName("employerLocation")]
    public TextWorkArea EmployerLocation
    {
      get => employerLocation ??= new();
      set => employerLocation = value;
    }

    /// <summary>
    /// A value of EiwoStatusMessage.
    /// </summary>
    [JsonPropertyName("eiwoStatusMessage")]
    public Infrastructure EiwoStatusMessage
    {
      get => eiwoStatusMessage ??= new();
      set => eiwoStatusMessage = value;
    }

    /// <summary>
    /// A value of FirstCharCleaned.
    /// </summary>
    [JsonPropertyName("firstCharCleaned")]
    public FieldValue FirstCharCleaned
    {
      get => firstCharCleaned ??= new();
      set => firstCharCleaned = value;
    }

    /// <summary>
    /// A value of MissingMandatoryField.
    /// </summary>
    [JsonPropertyName("missingMandatoryField")]
    public Common MissingMandatoryField
    {
      get => missingMandatoryField ??= new();
      set => missingMandatoryField = value;
    }

    /// <summary>
    /// A value of AllCharCleaned.
    /// </summary>
    [JsonPropertyName("allCharCleaned")]
    public FieldValue AllCharCleaned
    {
      get => allCharCleaned ??= new();
      set => allCharCleaned = value;
    }

    /// <summary>
    /// A value of Trimmed.
    /// </summary>
    [JsonPropertyName("trimmed")]
    public FieldValue Trimmed
    {
      get => trimmed ??= new();
      set => trimmed = value;
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
    /// A value of NullIwoAction.
    /// </summary>
    [JsonPropertyName("nullIwoAction")]
    public IwoAction NullIwoAction
    {
      get => nullIwoAction ??= new();
      set => nullIwoAction = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of Arnmorgz.
    /// </summary>
    [JsonPropertyName("arnmorgz")]
    public EiwoB587DetailRecord Arnmorgz
    {
      get => arnmorgz ??= new();
      set => arnmorgz = value;
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public AbendData Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public IwoAction Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public WorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public EiwoB587DetailRecord Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of EiwoB587DetailRecord.
    /// </summary>
    [JsonPropertyName("eiwoB587DetailRecord")]
    public EiwoB587DetailRecord EiwoB587DetailRecord
    {
      get => eiwoB587DetailRecord ??= new();
      set => eiwoB587DetailRecord = value;
    }

    /// <summary>
    /// A value of BatchCount.
    /// </summary>
    [JsonPropertyName("batchCount")]
    public Common BatchCount
    {
      get => batchCount ??= new();
      set => batchCount = value;
    }

    /// <summary>
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    public Common RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of ExtendedBatchEiwoB587TrailerRecord.
    /// </summary>
    [JsonPropertyName("extendedBatchEiwoB587TrailerRecord")]
    public EiwoB587TrailerRecord ExtendedBatchEiwoB587TrailerRecord
    {
      get => extendedBatchEiwoB587TrailerRecord ??= new();
      set => extendedBatchEiwoB587TrailerRecord = value;
    }

    /// <summary>
    /// A value of ExtendedBatchEiwoB587HeaderRecord.
    /// </summary>
    [JsonPropertyName("extendedBatchEiwoB587HeaderRecord")]
    public EiwoB587HeaderRecord ExtendedBatchEiwoB587HeaderRecord
    {
      get => extendedBatchEiwoB587HeaderRecord ??= new();
      set => extendedBatchEiwoB587HeaderRecord = value;
    }

    /// <summary>
    /// A value of ExtendedFileEiwoB587TrailerRecord.
    /// </summary>
    [JsonPropertyName("extendedFileEiwoB587TrailerRecord")]
    public EiwoB587TrailerRecord ExtendedFileEiwoB587TrailerRecord
    {
      get => extendedFileEiwoB587TrailerRecord ??= new();
      set => extendedFileEiwoB587TrailerRecord = value;
    }

    /// <summary>
    /// A value of ExtendedFileEiwoB587HeaderRecord.
    /// </summary>
    [JsonPropertyName("extendedFileEiwoB587HeaderRecord")]
    public EiwoB587HeaderRecord ExtendedFileEiwoB587HeaderRecord
    {
      get => extendedFileEiwoB587HeaderRecord ??= new();
      set => extendedFileEiwoB587HeaderRecord = value;
    }

    /// <summary>
    /// A value of BatchEiwoB587TrailerRecord.
    /// </summary>
    [JsonPropertyName("batchEiwoB587TrailerRecord")]
    public EiwoB587TrailerRecord BatchEiwoB587TrailerRecord
    {
      get => batchEiwoB587TrailerRecord ??= new();
      set => batchEiwoB587TrailerRecord = value;
    }

    /// <summary>
    /// A value of BatchEiwoB587HeaderRecord.
    /// </summary>
    [JsonPropertyName("batchEiwoB587HeaderRecord")]
    public EiwoB587HeaderRecord BatchEiwoB587HeaderRecord
    {
      get => batchEiwoB587HeaderRecord ??= new();
      set => batchEiwoB587HeaderRecord = value;
    }

    /// <summary>
    /// A value of FileEiwoB587TrailerRecord.
    /// </summary>
    [JsonPropertyName("fileEiwoB587TrailerRecord")]
    public EiwoB587TrailerRecord FileEiwoB587TrailerRecord
    {
      get => fileEiwoB587TrailerRecord ??= new();
      set => fileEiwoB587TrailerRecord = value;
    }

    /// <summary>
    /// A value of FileEiwoB587HeaderRecord.
    /// </summary>
    [JsonPropertyName("fileEiwoB587HeaderRecord")]
    public EiwoB587HeaderRecord FileEiwoB587HeaderRecord
    {
      get => fileEiwoB587HeaderRecord ??= new();
      set => fileEiwoB587HeaderRecord = value;
    }

    /// <summary>
    /// A value of PreviousCsePerson.
    /// </summary>
    [JsonPropertyName("previousCsePerson")]
    public CsePerson PreviousCsePerson
    {
      get => previousCsePerson ??= new();
      set => previousCsePerson = value;
    }

    /// <summary>
    /// A value of PreviousEmployer.
    /// </summary>
    [JsonPropertyName("previousEmployer")]
    public Employer PreviousEmployer
    {
      get => previousEmployer ??= new();
      set => previousEmployer = value;
    }

    /// <summary>
    /// A value of TotalNumbOfEinsWritten.
    /// </summary>
    [JsonPropertyName("totalNumbOfEinsWritten")]
    public Common TotalNumbOfEinsWritten
    {
      get => totalNumbOfEinsWritten ??= new();
      set => totalNumbOfEinsWritten = value;
    }

    /// <summary>
    /// A value of RestartCsePerson.
    /// </summary>
    [JsonPropertyName("restartCsePerson")]
    public CsePerson RestartCsePerson
    {
      get => restartCsePerson ??= new();
      set => restartCsePerson = value;
    }

    /// <summary>
    /// A value of RestartEmployer.
    /// </summary>
    [JsonPropertyName("restartEmployer")]
    public Employer RestartEmployer
    {
      get => restartEmployer ??= new();
      set => restartEmployer = value;
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
    /// A value of RestartNcp.
    /// </summary>
    [JsonPropertyName("restartNcp")]
    public CsePerson RestartNcp
    {
      get => restartNcp ??= new();
      set => restartNcp = value;
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
    /// A value of RecordLength.
    /// </summary>
    [JsonPropertyName("recordLength")]
    public Common RecordLength
    {
      get => recordLength ??= new();
      set => recordLength = value;
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
    /// A value of NumberOfNonPaymentDays.
    /// </summary>
    [JsonPropertyName("numberOfNonPaymentDays")]
    public Common NumberOfNonPaymentDays
    {
      get => numberOfNonPaymentDays ??= new();
      set => numberOfNonPaymentDays = value;
    }

    /// <summary>
    /// A value of PaymentCutoffDate.
    /// </summary>
    [JsonPropertyName("paymentCutoffDate")]
    public DateWorkArea PaymentCutoffDate
    {
      get => paymentCutoffDate ??= new();
      set => paymentCutoffDate = value;
    }

    /// <summary>
    /// A value of NcpPostcard.
    /// </summary>
    [JsonPropertyName("ncpPostcard")]
    public EabReportSend NcpPostcard
    {
      get => ncpPostcard ??= new();
      set => ncpPostcard = value;
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
    /// A value of Ncp.
    /// </summary>
    [JsonPropertyName("ncp")]
    public CsePersonAddress Ncp
    {
      get => ncp ??= new();
      set => ncp = value;
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
    /// A value of FirstOfNextMonth.
    /// </summary>
    [JsonPropertyName("firstOfNextMonth")]
    public DateWorkArea FirstOfNextMonth
    {
      get => firstOfNextMonth ??= new();
      set => firstOfNextMonth = value;
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
    /// A value of TotalNumbOfNcpsWritten.
    /// </summary>
    [JsonPropertyName("totalNumbOfNcpsWritten")]
    public Common TotalNumbOfNcpsWritten
    {
      get => totalNumbOfNcpsWritten ??= new();
      set => totalNumbOfNcpsWritten = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

    /// <summary>
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    /// <summary>
    /// A value of ErrorInd.
    /// </summary>
    [JsonPropertyName("errorInd")]
    public Common ErrorInd
    {
      get => errorInd ??= new();
      set => errorInd = value;
    }

    /// <summary>
    /// A value of RowLockFieldValue.
    /// </summary>
    [JsonPropertyName("rowLockFieldValue")]
    public Common RowLockFieldValue
    {
      get => rowLockFieldValue ??= new();
      set => rowLockFieldValue = value;
    }

    /// <summary>
    /// A value of DocsSystemError.
    /// </summary>
    [JsonPropertyName("docsSystemError")]
    public Common DocsSystemError
    {
      get => docsSystemError ??= new();
      set => docsSystemError = value;
    }

    /// <summary>
    /// Gets a value of Totals.
    /// </summary>
    [JsonIgnore]
    public Array<TotalsGroup> Totals => totals ??= new(TotalsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Totals for json serialization.
    /// </summary>
    [JsonPropertyName("totals")]
    [Computed]
    public IList<TotalsGroup> Totals_Json
    {
      get => totals;
      set => Totals.Assign(value);
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Monitored.
    /// </summary>
    [JsonPropertyName("monitored")]
    public OfficeServiceProviderAlert Monitored
    {
      get => monitored ??= new();
      set => monitored = value;
    }

    /// <summary>
    /// A value of UnMonitored.
    /// </summary>
    [JsonPropertyName("unMonitored")]
    public OfficeServiceProviderAlert UnMonitored
    {
      get => unMonitored ??= new();
      set => unMonitored = value;
    }

    /// <summary>
    /// A value of RequiredFieldMissing.
    /// </summary>
    [JsonPropertyName("requiredFieldMissing")]
    public Common RequiredFieldMissing
    {
      get => requiredFieldMissing ??= new();
      set => requiredFieldMissing = value;
    }

    /// <summary>
    /// A value of UserinputField.
    /// </summary>
    [JsonPropertyName("userinputField")]
    public Common UserinputField
    {
      get => userinputField ??= new();
      set => userinputField = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of DocsWarning.
    /// </summary>
    [JsonPropertyName("docsWarning")]
    public Common DocsWarning
    {
      get => docsWarning ??= new();
      set => docsWarning = value;
    }

    /// <summary>
    /// A value of DocsDataError.
    /// </summary>
    [JsonPropertyName("docsDataError")]
    public Common DocsDataError
    {
      get => docsDataError ??= new();
      set => docsDataError = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of Automatic.
    /// </summary>
    [JsonPropertyName("automatic")]
    public OfficeServiceProviderAlert Automatic
    {
      get => automatic ??= new();
      set => automatic = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of DocsProcessed.
    /// </summary>
    [JsonPropertyName("docsProcessed")]
    public Common DocsProcessed
    {
      get => docsProcessed ??= new();
      set => docsProcessed = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

    /// <summary>
    /// A value of NullDocument.
    /// </summary>
    [JsonPropertyName("nullDocument")]
    public Document NullDocument
    {
      get => nullDocument ??= new();
      set => nullDocument = value;
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
    /// A value of RowLockDocument.
    /// </summary>
    [JsonPropertyName("rowLockDocument")]
    public Common RowLockDocument
    {
      get => rowLockDocument ??= new();
      set => rowLockDocument = value;
    }

    /// <summary>
    /// A value of DocsRead.
    /// </summary>
    [JsonPropertyName("docsRead")]
    public Common DocsRead
    {
      get => docsRead ??= new();
      set => docsRead = value;
    }

    private Common ch5Found;
    private Common ch4Found;
    private Common ch3Found;
    private Common ch6Found;
    private Common ch2Found;
    private FieldValue temp;
    private WorkArea employerName;
    private TextWorkArea employerLocation;
    private Infrastructure eiwoStatusMessage;
    private FieldValue firstCharCleaned;
    private Common missingMandatoryField;
    private FieldValue allCharCleaned;
    private FieldValue trimmed;
    private OutgoingDocument outgoingDocument;
    private IwoAction nullIwoAction;
    private IwoAction iwoAction;
    private EiwoB587DetailRecord arnmorgz;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private AbendData read;
    private IwoAction next;
    private WorkArea zero;
    private EiwoB587DetailRecord initialized;
    private EiwoB587DetailRecord eiwoB587DetailRecord;
    private Common batchCount;
    private Common recordCount;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private DateWorkArea dateWorkArea;
    private EiwoB587TrailerRecord extendedBatchEiwoB587TrailerRecord;
    private EiwoB587HeaderRecord extendedBatchEiwoB587HeaderRecord;
    private EiwoB587TrailerRecord extendedFileEiwoB587TrailerRecord;
    private EiwoB587HeaderRecord extendedFileEiwoB587HeaderRecord;
    private EiwoB587TrailerRecord batchEiwoB587TrailerRecord;
    private EiwoB587HeaderRecord batchEiwoB587HeaderRecord;
    private EiwoB587TrailerRecord fileEiwoB587TrailerRecord;
    private EiwoB587HeaderRecord fileEiwoB587HeaderRecord;
    private CsePerson previousCsePerson;
    private Employer previousEmployer;
    private Common totalNumbOfEinsWritten;
    private CsePerson restartCsePerson;
    private Employer restartEmployer;
    private Common numbOfNcpsSinceChckpnt;
    private CsePerson restartNcp;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common recordLength;
    private TextWorkArea textDate;
    private Common numberOfNonPaymentDays;
    private DateWorkArea paymentCutoffDate;
    private EabReportSend ncpPostcard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonAddress ncp;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea firstOfNextMonth;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalNumbOfNcpsWritten;
    private Document document;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
    private Common errorInd;
    private Common rowLockFieldValue;
    private Common docsSystemError;
    private Array<TotalsGroup> totals;
    private WorkArea workArea;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private OfficeServiceProviderAlert monitored;
    private OfficeServiceProviderAlert unMonitored;
    private Common requiredFieldMissing;
    private Common userinputField;
    private FieldValue fieldValue;
    private Common docsWarning;
    private Common docsDataError;
    private SpDocLiteral spDocLiteral;
    private SpDocKey spDocKey;
    private Field field;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private OfficeServiceProviderAlert automatic;
    private EabConvertNumeric2 eabConvertNumeric;
    private OutDocRtrnAddr outDocRtrnAddr;
    private EventDetail eventDetail;
    private Common docsProcessed;
    private Common debugOn;
    private Document nullDocument;
    private Infrastructure nullInfrastructure;
    private Common rowLockDocument;
    private Common docsRead;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of SduFips.
    /// </summary>
    [JsonPropertyName("sduFips")]
    public Fips SduFips
    {
      get => sduFips ??= new();
      set => sduFips = value;
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
    /// A value of SduFipsTribAddress.
    /// </summary>
    [JsonPropertyName("sduFipsTribAddress")]
    public FipsTribAddress SduFipsTribAddress
    {
      get => sduFipsTribAddress ??= new();
      set => sduFipsTribAddress = value;
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
    /// A value of Cp.
    /// </summary>
    [JsonPropertyName("cp")]
    public CsePerson Cp
    {
      get => cp ??= new();
      set => cp = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private EventDetail eventDetail;
    private Fips sduFips;
    private Tribunal tribunal;
    private FipsTribAddress sduFipsTribAddress;
    private Fips fips;
    private CsePerson cp;
    private CsePerson ch;
    private CodeValue codeValue;
    private Code code;
    private Document document;
    private FieldValue fieldValue;
    private Field field;
    private DocumentField documentField;
    private LegalAction legalAction;
    private IwoTransaction iwoTransaction;
    private Employer employer;
    private IncomeSource incomeSource;
    private OutgoingDocument outgoingDocument;
    private IwoAction iwoAction;
    private CsePerson ncp;
    private Infrastructure infrastructure;
    private ServiceProvider serviceProvider;
    private LegalReferral legalReferral;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private LegalReferralAssignment legalReferralAssignment;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Alert alert;
  }
#endregion
}
