// Program: SI_B283_EMP_NEW_HIRE_INITIATIVE, ID: 1902519486, model: 746.
// Short name: SWEI283B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B283_EMP_NEW_HIRE_INITIATIVE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB283EmpNewHireInitiative: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B283_EMP_NEW_HIRE_INITIATIVE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB283EmpNewHireInitiative(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB283EmpNewHireInitiative.
  /// </summary>
  public SiB283EmpNewHireInitiative(IContext context, Import import,
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
    // 01/12/16  GVandy	CQ50569		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // General Overview
    // A new initiative is being implemented with the goal of increasing New 
    // Hire reporting by Kansas employers.  The initiative will involve
    // providing information about New Hire reporting regulations to employers
    // who have been discovered to be not consistently reporting new hire
    // information.  The intent is to educate the employer on the requirements
    // for New Hire reporting and therefore increase the timely issuance of
    // Income Withholding Orders.
    // One of several methods will be used to communicate the New Hire 
    // information to the employers. These intervention methods will include
    // phone calls, pamphlets, letters, and postcards.  In addition a control
    // group of employers will be established who will receive no communication.
    // The incoming state new hire file from SRRUN104 will then be monitored to 
    // determine the number of new hires reported by the employers.  At the end
    // of the initiative it will be possible to determine which communication
    // method produced the best New Hire reporting results.
    // A new database table will be created to store the employers to be tracked
    // as part of this initiative.  Business will provide a list of employers
    // included in this initiative which will be loaded into the new table via
    // SQL.  Data tracked in the new table will include the employers FEIN,
    // Kansas ID, Name, Address, Date of Intervention, Type of Intervention, and
    // an initiative identifier.  The initiative identifier will provide
    // support for multiple rounds of employer interventions if necessary.
    // The State New Hire job (SRRUN104) will be modified to log all reported 
    // new hire information from the tracked employers into a new file.  Data
    // contained in the new file will include the employers FEIN, Kansas ID,
    // Name, Address, Type of Intervention, NCP Person Number, Hire Date,
    // Received Date minus one day (the assumed date that Department of Labor
    // received the information), the number of days between Hire Date and
    // Reporting Date.  This new file will be appended to each day to create a
    // large file that can be forwarded to business at a future date for
    // analysis.
    // --------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // --      I M P O R T A N T      N O T E
    // --
    // -- This program deliberately does not checkpoint/restart.  When records 
    // are written
    // -- to the output file they actually go to a temporary file.  At the end 
    // of this
    // -- program a COMMIT action is done on the file which causes the records 
    // in the
    // -- temporary file to be written to the permanent file.
    // --
    // -- Restarts to this program are therefore always from the beginning which
    // will
    // -- recreate the temporary file before then COMMITing at the end.
    // --
    // -------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // ------------------------------------------------------------------------------
    // -- Read for checkpoint/restart info.
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

    // -------------------------------------------------------------------------------------
    // --  Open the New Hire Input File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseSiB283EabReadNewHireFile2();

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
    UseSiB283EabWriteInitiativeFil2();

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
    // -- Check for Group ID parameter on the PPI record.
    // -------------------------------------------------------------------------------------
    switch(TrimEnd(Substring(local.ProgramProcessingInfo.ParameterList, 1, 3)))
    {
      case "":
        // -- No value was entered on the PPI parameter.  Default to the largest
        // group id value.
        ReadNewHireInitiative1();
        local.EabReportSend.RptDetail =
          "No Group ID entered on the PPI parameter.  Defaulting to largest Group ID number......" +
          NumberToString(local.NewHireInitiative.GroupId, 13, 3);

        break;
      case "ALL":
        // -- The PPI parameter indicates to report against all group ids.  This
        // is accomplished by setting the control group id to zero.
        local.NewHireInitiative.GroupId = 0;
        local.EabReportSend.RptDetail =
          "Group ALL requested on the PPI parameter.  All existing groups will be reported.";
          

        break;
      default:
        if (Verify(Substring(local.ProgramProcessingInfo.ParameterList, 1, 3),
          " 0123456789") == 0)
        {
          // -- Use the group id entered on the PPI parameter.
          local.NewHireInitiative.GroupId =
            (int)StringToNumber(Substring(
              local.ProgramProcessingInfo.ParameterList, 1, 3));
          local.EabReportSend.RptDetail =
            "Group ID entered on the PPI parameter......." + NumberToString
            (local.NewHireInitiative.GroupId, 13, 3);
        }
        else
        {
          // -- The group id entered on the PPI parameter was not numeric.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Invalid group id entered on the PPI parameter.  Value must be ALL or a numeric group id.  PPI value = " +
            Substring(local.ProgramProcessingInfo.ParameterList, 1, 3);
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        break;
    }

    // -------------------------------------------------------------------------------------
    // -- Log the group id being reported to the control report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      if (local.Common.Count == 1)
      {
        // -- The local eab_report_send rpt_detail value was set above.
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
          "(01) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.NumbOfReadsSinceCommit.Count = 0;
    local.TotalNumOfGroupMatches.Count = 0;

    // -- Local delimiter value below is set to <TAB> character.
    local.Delimiter.Text1 = "\t";

    do
    {
      local.Input.Assign(local.NullNewHireInitiativeRecord);

      // -------------------------------------------------------------------------------------
      // --  Read the New Hire Input File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "READ";
      UseSiB283EabReadNewHireFile1();

      // -- Check for End of File.
      if (Equal(local.EabFileHandling.Status, "EF"))
      {
        continue;
      }

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading input file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // -------------------------------------------------------------------------------------
      // --  Determine if this employer is on the list of employers to monitor.
      // -------------------------------------------------------------------------------------
      foreach(var item in ReadNewHireInitiative2())
      {
        ++local.TotalNumOfGroupMatches.Count;
        ++local.NumbOfReadsSinceCommit.Count;
        local.Input.GroupId =
          NumberToString(entities.NewHireInitiative.GroupId, 7, 9);
        local.Input.InterventionType =
          entities.NewHireInitiative.InterventionType ?? Spaces(15);

        // -- The date received at KDOL is assumed to be the processing date - 1
        // day.
        local.Received.Date =
          AddDays(local.ProgramProcessingInfo.ProcessDate, -1);
        local.Input.ReceivedDate = UseCabFormatDate2();

        // -- The hire date returned from the EAB is in format mm/dd/yyyy.  
        // Convert to format mm-dd-yyyy.
        local.Hire.Date = StringToDate(local.Input.HireDate);
        local.Temp.HireDate = UseCabFormatDate1();

        // -- Calculate the number of days from the hire date to the received 
        // date.
        local.Input.NumberOfDays =
          NumberToString((long)DaysFromAD(local.Received.Date) -
          DaysFromAD(local.Hire.Date), 7, 9);

        // -- Format the output record as follows.  A <TAB> delimiter is 
        // inserted after each field.
        // Name		Type		Max Length	Description
        // Identifier	Numeric		9		Identifies the New Hire Initiative in which 
        // the employer is participating.
        // FEIN		Text		9		Federal Employer Identification Number
        // Kansas ID	Text 		6		Kansas Employer Identification Number
        // Name		Text		45		Employer Name
        // Address Line 1	Text		40		Street Address Line 1
        // Address Line 2	Text		40		Street Address Line 2
        // City		Text		25		City
        // State		Text		2		State
        // Zip		Text		5		Zip
        // Zip Extension	Text		4		Zip + 4
        // Intervention- 	Text		15		Identifies the type of communication to the 
        // employer
        // Type						(Phone Call, Pamphlet, Letter, Postcard, Control Group)
        // Person Number	Text		10		CSP Number
        // Hire Date	Date		10		Format mm-dd-yyyy
        // Received Date	Date		10		Format mm-dd-yyyy
        // Number of Days	Numeric		9		Identifies the date of communication to 
        // the employer
        local.Output.OutputRecord = "";
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.GroupId) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.Fein) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.KansasId) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.EmployerName) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.AddressLine1) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.AddressLine2) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.City) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.State) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.ZipCode) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.ZipExtension) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.InterventionType) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.PersonNumber) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Temp.HireDate) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.ReceivedDate) + local.Delimiter.Text1;
        local.Output.OutputRecord = TrimEnd(local.Output.OutputRecord) + TrimEnd
          (local.Input.NumberOfDays) + local.Delimiter.Text1;

        // -------------------------------------------------------------------------------------
        // --  Write the Output File.
        // -------------------------------------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        UseSiB283EabWriteInitiativeFil1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // -------------------------------------------------------------------------------------
      // --      I M P O R T A N T      N O T E
      // --
      // -- This program deliberately does no checkpoint/restart.  When records 
      // are written
      // -- to the output file they actually go to a temporary file.  At the end
      // of this
      // -- program a COMMIT action is done on the file which causes the records
      // in the
      // -- temporary file to be written to the permanent file.
      // --
      // -- Restarts to this program are therefore always from the beginning 
      // which will
      // -- recreate the temporary file before then COMMITing at the end.
      // --
      // -------------------------------------------------------------------------------------
      // -- Commit.
      if (local.NumbOfReadsSinceCommit.Count > local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // --  COMMIT.
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

          return;
        }

        local.NumbOfReadsSinceCommit.Count = 0;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

    // -------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      if (local.Common.Count == 1)
      {
        local.EabReportSend.RptDetail =
          "Number of Employer Matches Written to the Output File.................." +
          NumberToString(local.TotalNumOfGroupMatches.Count, 9, 7);
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

    // -------------------------------------------------------------------------------------
    // --  Do a final Commit to the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "COMMIT";
    UseSiB283EabWriteInitiativeFil2();

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

    // -------------------------------------------------------------------------------------
    // --  Close the Input File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseSiB283EabReadNewHireFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing input file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseSiB283EabWriteInitiativeFil2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
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

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveNewHireInitiativeRecord(
    NewHireInitiativeRecord source, NewHireInitiativeRecord target)
  {
    target.Fein = source.Fein;
    target.KansasId = source.KansasId;
    target.EmployerName = source.EmployerName;
    target.AddressLine1 = source.AddressLine1;
    target.AddressLine2 = source.AddressLine2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.ZipExtension = source.ZipExtension;
    target.PersonNumber = source.PersonNumber;
    target.HireDate = source.HireDate;
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

  private string UseCabFormatDate1()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.Hire.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    return useExport.FormattedDate.Text10;
  }

  private string UseCabFormatDate2()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.Received.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    return useExport.FormattedDate.Text10;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
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

  private void UseSiB283EabReadNewHireFile1()
  {
    var useImport = new SiB283EabReadNewHireFile.Import();
    var useExport = new SiB283EabReadNewHireFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.NewHireInitiativeRecord.Assign(local.Input);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SiB283EabReadNewHireFile.Execute, useImport, useExport);

    MoveNewHireInitiativeRecord(useExport.NewHireInitiativeRecord, local.Input);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB283EabReadNewHireFile2()
  {
    var useImport = new SiB283EabReadNewHireFile.Import();
    var useExport = new SiB283EabReadNewHireFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SiB283EabReadNewHireFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB283EabWriteInitiativeFil1()
  {
    var useImport = new SiB283EabWriteInitiativeFil.Import();
    var useExport = new SiB283EabWriteInitiativeFil.Export();

    useImport.NewHireInitiativeRecord.OutputRecord = local.Output.OutputRecord;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SiB283EabWriteInitiativeFil.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB283EabWriteInitiativeFil2()
  {
    var useImport = new SiB283EabWriteInitiativeFil.Import();
    var useExport = new SiB283EabWriteInitiativeFil.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(SiB283EabWriteInitiativeFil.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadNewHireInitiative1()
  {
    local.NewHireInitiative.Populated = false;

    return Read("ReadNewHireInitiative1",
      null,
      (db, reader) =>
      {
        local.NewHireInitiative.GroupId = db.GetInt32(reader, 0);
        local.NewHireInitiative.Populated = true;
      });
  }

  private IEnumerable<bool> ReadNewHireInitiative2()
  {
    entities.NewHireInitiative.Populated = false;

    return ReadEach("ReadNewHireInitiative2",
      (db, command) =>
      {
        db.SetString(command, "fein", local.Input.Fein);
        db.SetInt32(command, "groupId", local.NewHireInitiative.GroupId);
      },
      (db, reader) =>
      {
        entities.NewHireInitiative.GroupId = db.GetInt32(reader, 0);
        entities.NewHireInitiative.Fein = db.GetString(reader, 1);
        entities.NewHireInitiative.InterventionType =
          db.GetNullableString(reader, 2);
        entities.NewHireInitiative.Populated = true;

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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public NewHireInitiativeRecord Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Output.
    /// </summary>
    [JsonPropertyName("output")]
    public NewHireInitiativeRecord Output
    {
      get => output ??= new();
      set => output = value;
    }

    /// <summary>
    /// A value of Delimiter.
    /// </summary>
    [JsonPropertyName("delimiter")]
    public TextWorkArea Delimiter
    {
      get => delimiter ??= new();
      set => delimiter = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of Hire.
    /// </summary>
    [JsonPropertyName("hire")]
    public DateWorkArea Hire
    {
      get => hire ??= new();
      set => hire = value;
    }

    /// <summary>
    /// A value of Received.
    /// </summary>
    [JsonPropertyName("received")]
    public DateWorkArea Received
    {
      get => received ??= new();
      set => received = value;
    }

    /// <summary>
    /// A value of NewHireInitiative.
    /// </summary>
    [JsonPropertyName("newHireInitiative")]
    public NewHireInitiative NewHireInitiative
    {
      get => newHireInitiative ??= new();
      set => newHireInitiative = value;
    }

    /// <summary>
    /// A value of NullNewHireInitiativeRecord.
    /// </summary>
    [JsonPropertyName("nullNewHireInitiativeRecord")]
    public NewHireInitiativeRecord NullNewHireInitiativeRecord
    {
      get => nullNewHireInitiativeRecord ??= new();
      set => nullNewHireInitiativeRecord = value;
    }

    /// <summary>
    /// A value of Input.
    /// </summary>
    [JsonPropertyName("input")]
    public NewHireInitiativeRecord Input
    {
      get => input ??= new();
      set => input = value;
    }

    /// <summary>
    /// A value of NumbOfReadsSinceCommit.
    /// </summary>
    [JsonPropertyName("numbOfReadsSinceCommit")]
    public Common NumbOfReadsSinceCommit
    {
      get => numbOfReadsSinceCommit ??= new();
      set => numbOfReadsSinceCommit = value;
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
    /// A value of TotalNumOfGroupMatches.
    /// </summary>
    [JsonPropertyName("totalNumOfGroupMatches")]
    public Common TotalNumOfGroupMatches
    {
      get => totalNumOfGroupMatches ??= new();
      set => totalNumOfGroupMatches = value;
    }

    private NewHireInitiativeRecord temp;
    private NewHireInitiativeRecord output;
    private TextWorkArea delimiter;
    private External external;
    private DateWorkArea hire;
    private DateWorkArea received;
    private NewHireInitiative newHireInitiative;
    private NewHireInitiativeRecord nullNewHireInitiativeRecord;
    private NewHireInitiativeRecord input;
    private Common numbOfReadsSinceCommit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea nullDateWorkArea;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalNumOfGroupMatches;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NewHireInitiative.
    /// </summary>
    [JsonPropertyName("newHireInitiative")]
    public NewHireInitiative NewHireInitiative
    {
      get => newHireInitiative ??= new();
      set => newHireInitiative = value;
    }

    private NewHireInitiative newHireInitiative;
  }
#endregion
}
