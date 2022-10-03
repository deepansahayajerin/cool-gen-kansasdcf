// Program: LE_B605_LEG_REF_MOD_REQUEST_EXT, ID: 1625411271, model: 746.
// Short name: SWEL605B
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
/// A program: LE_B605_LEG_REF_MOD_REQUEST_EXT.
/// </para>
/// <para>
/// This Batch process extracts modification reason codes (MOC/MON/MIC/MIN/MOO) 
/// and associated
/// office, collection officer, person &amp; cases information etc.  The 
/// information will be
/// selected only for previous month (previous month will be determined based on
/// the process
/// date).   The extracted information will be written onto to sequential 
/// dataset and the same
/// will be email to Business Table Maintenance team @ dcf.csetablemaintenance@
/// ks.gov.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB605LegRefModRequestExt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B605_LEG_REF_MOD_REQUEST_EXT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB605LegRefModRequestExt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB605LegRefModRequestExt.
  /// </summary>
  public LeB605LegRefModRequestExt(IContext context, Import import,
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
    // ******************************************************************************************
    // Date        Developer         Request #  Description
    // --------    ----------------  ---------  ------------------------
    // 05/03/2021  Raj               CQ68844    Initial Development(Cloened from
    // B599 Batch).
    // ******************************************************************************************
    // ******************************************************************************************
    // This Batch process extracts modification reason codes (MOC/MON/MIC/MIN/
    // MOO) and associated
    // office, collection officer, person & cases information etc.  The 
    // information will be
    // selected only for previous month (previous month will be determined based
    // on the process
    // date).   The extracted information will be written onto to sequential 
    // dataset and the same
    // will be email to Business Table Maintenance team @ dcf.
    // csetablemaintenance@ks.gov.
    // ******************************************************************************************
    // ******************************************************************************************
    // Check if DCF KEES SYNC database is avaialble for client information 
    // processing.
    // ******************************************************************************************
    UseCabReadAdabasPersonBatch1();

    if (Equal(local.AbendData.AdabasResponseCd, "0148"))
    {
      ExitState = "ADABAS_UNAVAILABLE_RB";

      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // ******************************************************************************************
    // Read Batch Program Program processing table to obtain batch process date 
    // and program
    // related parameter for batch processing.
    // ******************************************************************************************
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ******************************************************************************************
    // Open Error Report File.
    // ******************************************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ******************************************************************************************
    // Open Cotrol Report File.
    // ******************************************************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ******************************************************************************************
    // Open Legal Referrals modification reason code extract file.
    // ******************************************************************************************
    local.EabFileHandling.Action = "OPEN";
    UseLeB605EabWriteLegrefModExt3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'LE_B605_EAB_LEG_REF_MOD_EXTRACT'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    local.FileOpened.Flag = "Y";

    // ******************************************************************************************
    // Determine the reporting period start and end dates for the Extract 
    // Processing.
    // The end date is set to the first day of the current month.  The start 
    // date is set to the
    // first day of the previous month.  We will then look for legal referrals 
    // with a created
    // timestamp greater or equal to the start date and less than the end date.
    // ******************************************************************************************
    local.End.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate,
      -(Day(local.ProgramProcessingInfo.ProcessDate) - 1));

    // local start is 1st day of month  (previous )
    local.Start.Date = AddMonths(local.End.Date, -1);

    // ********************************************************************************************
    // Extract all Legacl Referrals created during the reporting month with the 
    // modification reason
    // codes (MOC/MON/MIC/MIN/MOO).
    // ********************************************************************************************
    foreach(var item in ReadLegalReferralCase())
    {
      // ********************************************************************************************
      // Skip referrals that have been closed unless created and closed during 
      // report month.
      // ********************************************************************************************
      if (AsChar(entities.LegalReferral.Status) == 'O' || AsChar
        (entities.LegalReferral.Status) == 'S')
      {
      }
      else
      {
        if (!Lt(entities.LegalReferral.StatusDate, local.Start.Date) && Lt
          (entities.LegalReferral.StatusDate, local.End.Date))
        {
          goto Test;
        }

        continue;
      }

Test:

      // ********************************************************************************************
      // Extract FIPS County & Location Description.
      // ********************************************************************************************
      if (ReadCaseTribunalFipsLegalAction())
      {
        local.Fips.Assign(entities.ExistingFips);
        local.LegalAction.Assign(entities.ExistingLegalAction);
      }
      else
      {
        local.Fips.Assign(local.NullFips);
        local.LegalAction.Assign(local.NullLegalAction);
      }

      // ********************************************************************************************
      // Extract Absent Parent person information.
      // ********************************************************************************************
      local.CsePersonsWorkSet.FormattedName = "";

      if (ReadCsePerson())
      {
        // ********************************************************************************************
        // Retrieve the AP name from KEES SYNC Database (Master Person Index) 
        // shared by KDHE KEES &
        // DCF CSS,FACTS,KMIS applications.
        // ********************************************************************************************
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseCabReadAdabasPersonBatch2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ********************************************************************************************
          // The below listed common action block formats the AP Name.
          // ********************************************************************************************
          UseSiFormatCsePersonName();
        }
        else
        {
          if (AsChar(local.AbendData.Type1) == 'A' && Equal
            (local.AbendData.AdabasResponseCd, "0113"))
          {
            // ********************************************************************************************
            // KEES DCF SYNC Database error during client information extract. 
            // Person Not Found in KEES DB.
            // ********************************************************************************************
          }
          else
          {
            // ********************************************************************************************
            // Unknown/Internal Error returned from KEES DCF SYNC client 
            // information extract.
            // ********************************************************************************************
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Fatal error in Adabas for person number : " + entities
              .CsePerson.Number;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ", Abend Type Code=";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + local.AbendData.Type1;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ", Response Code=";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + local
              .AbendData.AdabasResponseCd;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ", File Number=";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + local
              .AbendData.AdabasFileNumber;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ", File Action=";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + local
              .AbendData.AdabasFileAction;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.TotalErrors.Count;
          }

          ExitState = "ACO_NN0000_ALL_OK";
          local.CsePersonsWorkSet.FormattedName =
            "** Name not found in KEES SYNC **";
        }
      }
      else
      {
        // ********************************************************************************************
        // Selected AP Person Id is not found in CSS CSE Person Table, continue 
        // the process with AP
        // Name as Blank.
        // ********************************************************************************************
      }

      // ********************************************************************************************
      // Obtain Collection Officer information for the selected legal referral 
      // and associated CSS
      // Case.
      // ********************************************************************************************
      if (!ReadOfficeOfficeServiceProviderServiceProvider())
      {
        // ********************************************************************************************
        // Selected Legral Referral CSS Case associate collection officer 
        // information not found in CSS
        // DB, the selected legal referral records will be skipped and error 
        // information will be added
        // to the error report.  Since, the report is generated for collection 
        // officer, officer etc.
        // if associated collection officer, officet etc. information is not 
        // avaialble then selected
        // referral will be skipped.
        // ********************************************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "No CO assigned for Legal Referral id : " + NumberToString
          (entities.LegalReferral.Identifier, 13, 3) + " on Case : " + entities
          .ExistingCase.Number;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      // ********************************************************************************************
      // Extract associated Area Office information which oversees the selected 
      // Collection Officer/
      // Office.
      // ********************************************************************************************
      local.Area.Name = "";

      if (ReadCseOrganization())
      {
        local.Area.Name = entities.AreaCseOrganization.Name;
      }
      else
      {
        // ********************************************************************************************
        // Area officer information is missing, selected legal referral 
        // information will be skipped
        // from reporting because generated extract/report information is based 
        // on Area Office/Officer.
        // ********************************************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading area cse_organization for co office " + NumberToString
          (entities.CoOffice.SystemGeneratedId, 12, 4) + " " + TrimEnd
          (entities.CoOffice.Name) + " - Legal Referral id :" + " " + NumberToString
          (entities.LegalReferral.Identifier, 13, 3) + " on Case : " + entities
          .ExistingCase.Number;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      // ********************************************************************************************
      // Select Legal Service Provider responsible for the selected Legal 
      // Referral Modification
      // Requests.
      // ********************************************************************************************
      if (AsChar(entities.LegalReferral.Status) == 'R' || AsChar
        (entities.LegalReferral.Status) == 'W')
      {
        // -- The legal service provider assignment is end dated when the 
        // referral status is set to Rejected or Withdrawn.  Report the referral
        // under the most recent service provider assigned to the referral.
        ReadLegalReferralAssignment2();
      }
      else
      {
        // ********************************************************************************************
        // Extract the legal service provider who is actively assigned to the 
        // legal referral.
        // ********************************************************************************************
        ReadLegalReferralAssignment1();
      }

      if (entities.LegalReferralAssignment.Populated)
      {
        if (!ReadServiceProvider1())
        {
          // ********************************************************************************************
          // Legal Referral Service Provider not found for the selected legal 
          // referral/assignment and
          // skip selected referral and report the information via error report.
          // ********************************************************************************************
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading Attorney for Legal Referral number : " + NumberToString
            (entities.LegalReferral.Identifier, 13, 3) + " on Case : " + entities
            .ExistingCase.Number;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.TotalErrors.Count;

          continue;
        }
      }
      else
      {
        // ********************************************************************************************
        // Legal Referral Service Provider not found for the selected legal 
        // referral/assignment and
        // skip selected referral and report the information via error report.
        // ********************************************************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "No Attorney assigned for Legal Referral number : " + NumberToString
          (entities.LegalReferral.Identifier, 13, 3) + " on Case : " + entities
          .ExistingCase.Number;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      local.Supervisor.Assign(local.NullServiceProvider);

      // ********************************************************************************************
      // Determine the number of supervisors to which the collection officer 
      // reports.
      // ********************************************************************************************
      ReadServiceProvider2();

      if (local.NumberOfSupervisors.Count == 0)
      {
        // ********************************************************************************************
        // Write the referral for the collection officer.  No supervisor will be
        // listed.
        // ********************************************************************************************
        local.EabFileHandling.Action = "WRITE";
        UseLeB605EabWriteLegrefModExt1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing record in 'LE_B605_EAB_LEG_REF_MOD_EXTRACT'.";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.Record.Count;
      }
      else
      {
        // ********************************************************************************************
        // Report the referral under each supervisor for the collection officer.
        // ********************************************************************************************
        foreach(var item1 in ReadServiceProvider3())
        {
          local.Supervisor.Assign(entities.SupervisorServiceProvider);
          local.EabFileHandling.Action = "WRITE";
          UseLeB605EabWriteLegrefModExt2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing record in 'LE_B605_EAB_LEG_REF_MOD_EXTRACT'.";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ++local.Record.Count;
        }
      }
    }

    local.EabReportSend.RptDetail =
      "Total Number Of Legal Referrals Written :  " + NumberToString
      (local.Record.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Legal Referrals Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Number of Errors Written :           " + NumberToString
      (local.TotalErrors.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Errors Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.FileOpened.Flag) == 'Y')
    {
      // ******************************************************************************************
      // CLOSE Legal Referrals modification reason code extract file.
      // ******************************************************************************************
      local.EabFileHandling.Action = "CLOSE";
      UseLeB605EabWriteLegrefModExt3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error in closing external file  for 'le_write_referral_extract'.";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_CLOSE_ERROR";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while closing control report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
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

  private void UseCabReadAdabasPersonBatch1()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseCabReadAdabasPersonBatch2()
  {
    var useImport = new CabReadAdabasPersonBatch.Import();
    var useExport = new CabReadAdabasPersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(CabReadAdabasPersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseLeB605EabWriteLegrefModExt1()
  {
    var useImport = new LeB605EabWriteLegrefModExt.Import();
    var useExport = new LeB605EabWriteLegrefModExt.Export();

    useImport.CsePersonsWorkSet.FormattedName =
      local.CsePersonsWorkSet.FormattedName;
    useImport.LegalReferral.Assign(entities.LegalReferral);
    useImport.Case1.Number = entities.ExistingCase.Number;
    useImport.Legal.Assign(entities.LegalServiceProvider);
    useImport.CoServiceProvider.Assign(entities.CoServiceProvider);
    useImport.Supervisor.Assign(local.Supervisor);
    MoveOffice(entities.CoOffice, useImport.CoOffice);
    useImport.Area.Name = local.Area.Name;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.Fips.Assign(local.Fips);
    MoveLegalAction(entities.ExistingLegalAction, useImport.LegalAction);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB605EabWriteLegrefModExt.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB605EabWriteLegrefModExt2()
  {
    var useImport = new LeB605EabWriteLegrefModExt.Import();
    var useExport = new LeB605EabWriteLegrefModExt.Export();

    useImport.CsePersonsWorkSet.FormattedName =
      local.CsePersonsWorkSet.FormattedName;
    useImport.LegalReferral.Assign(entities.LegalReferral);
    useImport.Case1.Number = entities.ExistingCase.Number;
    useImport.Legal.Assign(entities.LegalServiceProvider);
    useImport.CoServiceProvider.Assign(entities.CoServiceProvider);
    useImport.Supervisor.Assign(local.Supervisor);
    MoveOffice(entities.CoOffice, useImport.CoOffice);
    useImport.Area.Name = local.Area.Name;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.Fips.Assign(local.Fips);
    MoveLegalAction(local.LegalAction, useImport.LegalAction);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB605EabWriteLegrefModExt.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB605EabWriteLegrefModExt3()
  {
    var useImport = new LeB605EabWriteLegrefModExt.Import();
    var useExport = new LeB605EabWriteLegrefModExt.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB605EabWriteLegrefModExt.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCaseTribunalFipsLegalAction()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingFips.Populated = false;
    entities.Fips.Populated = false;

    return Read("ReadCaseTribunalFipsLegalAction",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalReferral.CourtCaseNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Fips.Number = db.GetString(reader, 0);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 1);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.ExistingFips.County = db.GetInt32(reader, 3);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.ExistingFips.State = db.GetInt32(reader, 4);
        entities.ExistingFips.StateDescription =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 6);
        entities.ExistingFips.LocationDescription =
          db.GetNullableString(reader, 7);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 8);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 9);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 10);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingFips.Populated = true;
        entities.Fips.Populated = true;
      });
  }

  private bool ReadCseOrganization()
  {
    System.Diagnostics.Debug.Assert(entities.CoOffice.Populated);
    entities.AreaCseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(
          command, "cogParentType", entities.CoOffice.CogTypeCode ?? "");
        db.SetString(command, "cogParentCode", entities.CoOffice.CogCode ?? "");
      },
      (db, reader) =>
      {
        entities.AreaCseOrganization.Code = db.GetString(reader, 0);
        entities.AreaCseOrganization.Type1 = db.GetString(reader, 1);
        entities.AreaCseOrganization.Name = db.GetString(reader, 2);
        entities.AreaCseOrganization.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadLegalReferralAssignment1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment1",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
        db.SetDate(
          command, "effectiveDate",
          entities.LegalReferral.ReferralDate.GetValueOrDefault());
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
      });
  }

  private bool ReadLegalReferralAssignment2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralAssignment.Populated = false;

    return Read("ReadLegalReferralAssignment2",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
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
      });
  }

  private IEnumerable<bool> ReadLegalReferralCase()
  {
    entities.ExistingCase.Populated = false;
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferralCase",
      null,
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 9);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 10);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 11);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 12);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 13);
        entities.ExistingCase.Populated = true;
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeOfficeServiceProviderServiceProvider()
  {
    entities.CoServiceProvider.Populated = false;
    entities.CoOffice.Populated = false;
    entities.CoOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.LegalReferral.ReferralDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.CoOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.CoOffice.Name = db.GetString(reader, 1);
        entities.CoOffice.CogTypeCode = db.GetNullableString(reader, 2);
        entities.CoOffice.CogCode = db.GetNullableString(reader, 3);
        entities.CoOffice.OffOffice = db.GetNullableInt32(reader, 4);
        entities.CoOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 5);
        entities.CoOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 6);
        entities.CoOfficeServiceProvider.RoleCode = db.GetString(reader, 7);
        entities.CoOfficeServiceProvider.EffectiveDate = db.GetDate(reader, 8);
        entities.CoServiceProvider.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.CoServiceProvider.LastName = db.GetString(reader, 10);
        entities.CoServiceProvider.FirstName = db.GetString(reader, 11);
        entities.CoServiceProvider.MiddleInitial = db.GetString(reader, 12);
        entities.CoServiceProvider.Populated = true;
        entities.CoOffice.Populated = true;
        entities.CoOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);
    entities.LegalServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", entities.LegalReferralAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.LegalServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.LegalServiceProvider.LastName = db.GetString(reader, 1);
        entities.LegalServiceProvider.FirstName = db.GetString(reader, 2);
        entities.LegalServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.LegalServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.CoOfficeServiceProvider.Populated);

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(
          command, "ospRoleCode", entities.CoOfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          entities.CoOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.CoOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.CoOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        local.NumberOfSupervisors.Count = db.GetInt32(reader, 0);
      });
  }

  private IEnumerable<bool> ReadServiceProvider3()
  {
    System.Diagnostics.Debug.Assert(entities.CoOfficeServiceProvider.Populated);
    entities.SupervisorServiceProvider.Populated = false;

    return ReadEach("ReadServiceProvider3",
      (db, command) =>
      {
        db.SetString(
          command, "ospRoleCode", entities.CoOfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          entities.CoOfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.CoOfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.CoOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.SupervisorServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.SupervisorServiceProvider.LastName = db.GetString(reader, 1);
        entities.SupervisorServiceProvider.FirstName = db.GetString(reader, 2);
        entities.SupervisorServiceProvider.MiddleInitial =
          db.GetString(reader, 3);
        entities.SupervisorServiceProvider.Populated = true;

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
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    private DateWorkArea processDate;
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
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public DateWorkArea Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public DateWorkArea First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of Area.
    /// </summary>
    [JsonPropertyName("area")]
    public Office Area
    {
      get => area ??= new();
      set => area = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public ServiceProvider Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

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
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of TotalErrors.
    /// </summary>
    [JsonPropertyName("totalErrors")]
    public Common TotalErrors
    {
      get => totalErrors ??= new();
      set => totalErrors = value;
    }

    /// <summary>
    /// A value of NumberOfSupervisors.
    /// </summary>
    [JsonPropertyName("numberOfSupervisors")]
    public Common NumberOfSupervisors
    {
      get => numberOfSupervisors ??= new();
      set => numberOfSupervisors = value;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
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
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public CsePersonsWorkSet Close
    {
      get => close ??= new();
      set => close = value;
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
    /// A value of NullFips.
    /// </summary>
    [JsonPropertyName("nullFips")]
    public Fips NullFips
    {
      get => nullFips ??= new();
      set => nullFips = value;
    }

    /// <summary>
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
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

    private DateWorkArea last;
    private DateWorkArea first;
    private Office area;
    private ServiceProvider supervisor;
    private AbendData abendData;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common fileOpened;
    private DateWorkArea end;
    private DateWorkArea start;
    private Common totalErrors;
    private Common numberOfSupervisors;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common errOnAdabasUnavailable;
    private ServiceProvider nullServiceProvider;
    private Common record;
    private CsePersonsWorkSet close;
    private Fips fips;
    private Fips nullFips;
    private LegalAction nullLegalAction;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    /// <summary>
    /// A value of SupervisorOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("supervisorOfficeServiceProvider")]
    public OfficeServiceProvider SupervisorOfficeServiceProvider
    {
      get => supervisorOfficeServiceProvider ??= new();
      set => supervisorOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SupervisorServiceProvider.
    /// </summary>
    [JsonPropertyName("supervisorServiceProvider")]
    public ServiceProvider SupervisorServiceProvider
    {
      get => supervisorServiceProvider ??= new();
      set => supervisorServiceProvider = value;
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
    /// A value of AreaOffice.
    /// </summary>
    [JsonPropertyName("areaOffice")]
    public Office AreaOffice
    {
      get => areaOffice ??= new();
      set => areaOffice = value;
    }

    /// <summary>
    /// A value of AreaCseOrganization.
    /// </summary>
    [JsonPropertyName("areaCseOrganization")]
    public CseOrganization AreaCseOrganization
    {
      get => areaCseOrganization ??= new();
      set => areaCseOrganization = value;
    }

    /// <summary>
    /// A value of CoServiceProvider.
    /// </summary>
    [JsonPropertyName("coServiceProvider")]
    public ServiceProvider CoServiceProvider
    {
      get => coServiceProvider ??= new();
      set => coServiceProvider = value;
    }

    /// <summary>
    /// A value of CoOffice.
    /// </summary>
    [JsonPropertyName("coOffice")]
    public Office CoOffice
    {
      get => coOffice ??= new();
      set => coOffice = value;
    }

    /// <summary>
    /// A value of CoOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("coOfficeServiceProvider")]
    public OfficeServiceProvider CoOfficeServiceProvider
    {
      get => coOfficeServiceProvider ??= new();
      set => coOfficeServiceProvider = value;
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
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
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
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
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
    /// A value of LegalOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("legalOfficeServiceProvider")]
    public OfficeServiceProvider LegalOfficeServiceProvider
    {
      get => legalOfficeServiceProvider ??= new();
      set => legalOfficeServiceProvider = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalServiceProvider.
    /// </summary>
    [JsonPropertyName("legalServiceProvider")]
    public ServiceProvider LegalServiceProvider
    {
      get => legalServiceProvider ??= new();
      set => legalServiceProvider = value;
    }

    /// <summary>
    /// A value of CoCseOrganization.
    /// </summary>
    [JsonPropertyName("coCseOrganization")]
    public CseOrganization CoCseOrganization
    {
      get => coCseOrganization ??= new();
      set => coCseOrganization = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("existingLegalActionCaseRole")]
    public LegalActionCaseRole ExistingLegalActionCaseRole
    {
      get => existingLegalActionCaseRole ??= new();
      set => existingLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Case1 Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private OfficeServiceProvider supervisorOfficeServiceProvider;
    private ServiceProvider supervisorServiceProvider;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private Office areaOffice;
    private CseOrganization areaCseOrganization;
    private ServiceProvider coServiceProvider;
    private Office coOffice;
    private OfficeServiceProvider coOfficeServiceProvider;
    private CaseAssignment caseAssignment;
    private Case1 existingCase;
    private CaseRole caseRole;
    private LegalReferralCaseRole legalReferralCaseRole;
    private CsePerson csePerson;
    private OfficeServiceProvider legalOfficeServiceProvider;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferral legalReferral;
    private ServiceProvider legalServiceProvider;
    private CseOrganization coCseOrganization;
    private Tribunal existingTribunal;
    private LegalActionCaseRole existingLegalActionCaseRole;
    private LegalAction existingLegalAction;
    private Fips existingFips;
    private Case1 fips;
  }
#endregion
}
