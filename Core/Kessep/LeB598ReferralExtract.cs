// Program: LE_B598_REFERRAL_EXTRACT, ID: 373427952, model: 746.
// Short name: SWEL598B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B598_REFERRAL_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB598ReferralExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B598_REFERRAL_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB598ReferralExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB598ReferralExtract.
  /// </summary>
  public LeB598ReferralExtract(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************************************************
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 07/17/01  GVandy            WR10353/
    //                             WR10359    Initial Development
    // 02/23/04  CMJ               PR198700   Report should show a more accurate
    // recall of the
    // 				       referral person working the referral.  Change dates
    // 				       to reflect the referral date and not the processing date.
    // 08/06/10  RMathews          CQ363      Include referrals created and 
    // closed in report month
    // 08/26/11  GVandy            CQ29124    Add reason_code = 'RC' when 
    // reading for regional office.
    // *****************************************************************************************
    // ****************************
    // Check if ADABAS is available
    // ****************************
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

    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

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

    // ************************************************
    // *Call External to Open the Extract File.       *
    // ************************************************
    local.EabFileHandling.Action = "OPEN";
    UseLeEabWriteReferralExtract2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error in file open for 'le_eab_write_referral_extract'.";
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

    // -----------------------------------------------------------------------------------------------
    // Determine the reporting period start and end dates..
    // The end date is set to the first day of the current month.  The start 
    // date is set to the first day of the previous month.  We will then look
    // for legal referrals with a created timestamp greater or equal to the
    // start date and less than the end date.
    // -----------------------------------------------------------------------------------------------
    local.End.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate,
      -(Day(local.ProgramProcessingInfo.ProcessDate) - 1));

    // local start is 1st day of month  (previous )
    local.Start.Date = AddMonths(local.End.Date, -1);

    // -- Find all legal referrals created during the reporting month.
    foreach(var item in ReadLegalReferralCase())
    {
      if (AsChar(entities.LegalReferral.Status) == 'C')
      {
        // -- Skip referrals that have been closed unless created and closed 
        // during report month (CQ363 change).
        if (!Lt(entities.LegalReferral.StatusDate, local.Start.Date) && Lt
          (entities.LegalReferral.StatusDate, local.End.Date))
        {
          goto Test;
        }

        continue;
      }

Test:

      // -- Find the AP on the referral.
      local.CsePersonsWorkSet.FormattedName = "";

      if (ReadCsePerson())
      {
        // -- Retrieve the AP name from ADABAS.
        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseCabReadAdabasPersonBatch2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Format the AP name
          UseSiFormatCsePersonName();
        }
        else
        {
          if (AsChar(local.AbendData.Type1) == 'A' && Equal
            (local.AbendData.AdabasResponseCd, "0113"))
          {
            // -------------------------------------------------------
            //   Adabas not found..
            // -------------------------------------------------------
          }
          else
          {
            // -------------------------------------------------------
            //   Unknown error response returned from adabas.
            // -------------------------------------------------------
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
            "*** Name not found in ADABAS ***";
        }
      }
      else
      {
        // Continue...  The AP name will be blank on the report.
      }

      // -- Find the collection officer assigned to the case.
      // cmj pr  198700 2/26/2004 changed to referral date instead of processing
      // date to get a more accurate report of who is working the referral
      if (!ReadOfficeOfficeServiceProviderServiceProvider())
      {
        // Write to the error file and skip the record...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "No CO assigned for Legal Referral id : " + NumberToString
          (entities.LegalReferral.Identifier, 13, 3) + " on Case : " + entities
          .Case1.Number;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      // -- Find the area office which oversees the CO office.
      local.Area.Name = "";

      // 08/26/11  GVandy CQ29124  Add reason_code = 'RC' when reading for 
      // regional office.
      if (ReadCseOrganization())
      {
        local.Area.Name = entities.AreaCseOrganization.Name;
      }
      else
      {
        // Write to the error file and skip the record...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error reading area cse_organization for co office " + NumberToString
          (entities.CoOffice.SystemGeneratedId, 12, 4) + " " + TrimEnd
          (entities.CoOffice.Name) + " - Legal Referral id :" + " " + NumberToString
          (entities.LegalReferral.Identifier, 13, 3) + " on Case : " + entities
          .Case1.Number;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      // -- Find the legal service provider responsible for the referral.
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
        // -- Find the legal service provider who is actively assigned to the 
        // legal referral.
        // cmj 02/26/2004  pr198700  changed date to referral date instead of 
        // processing date to retrieve more accurate report of work load for
        // referral
        ReadLegalReferralAssignment1();
      }

      if (entities.LegalReferralAssignment.Populated)
      {
        if (!ReadServiceProvider1())
        {
          // Write to the error file and skip the record...
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading Attorney for Legal Referral number : " + NumberToString
            (entities.LegalReferral.Identifier, 13, 3) + " on Case : " + entities
            .Case1.Number;
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
        // Write to the error file and skip the record...
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "No Attorney assigned for Legal Referral number : " + NumberToString
          (entities.LegalReferral.Identifier, 13, 3) + " on Case : " + entities
          .Case1.Number;
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalErrors.Count;

        continue;
      }

      local.Supervisor.Assign(local.Null1);

      // -- Determine the number of supervisors to which the collection officer 
      // reports.
      ReadServiceProvider2();

      if (local.NumberOfSupervisors.Count == 0)
      {
        // -- Report the referral for the collection officer.  No supervisor 
        // will be listed.
        local.EabFileHandling.Action = "WRITE";
        UseLeEabWriteReferralExtract1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing record in 'le_eab_write_referral_extract'.";
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
        // -- Report the referral under each supervisor for the collection 
        // officer.
        foreach(var item1 in ReadServiceProvider3())
        {
          local.Supervisor.Assign(entities.SupervisorServiceProvider);
          local.EabFileHandling.Action = "WRITE";
          UseLeEabWriteReferralExtract1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing record in 'le_eab_write_referral_extract'.";
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
      // ******************************************
      // *  Close Referral extract File           *
      // ******************************************
      local.EabFileHandling.Action = "CLOSE";
      UseLeEabWriteReferralExtract2();

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

    local.Close.Ssn = "close";
    UseEabReadCsePersonUsingSsn();

    // Do NOT need to verify on Return
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
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

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.Close.Ssn;

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);
  }

  private void UseLeEabWriteReferralExtract1()
  {
    var useImport = new LeEabWriteReferralExtract.Import();
    var useExport = new LeEabWriteReferralExtract.Export();

    useImport.CsePersonsWorkSet.FormattedName =
      local.CsePersonsWorkSet.FormattedName;
    useImport.LegalReferral.Assign(entities.LegalReferral);
    useImport.Case1.Number = entities.Case1.Number;
    useImport.Legal.Assign(entities.LegalServiceProvider);
    useImport.CoServiceProvider.Assign(entities.CoServiceProvider);
    useImport.Supervisor.Assign(local.Supervisor);
    MoveOffice(entities.CoOffice, useImport.CoOffice);
    useImport.Area.Name = local.Area.Name;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabWriteReferralExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeEabWriteReferralExtract2()
  {
    var useImport = new LeEabWriteReferralExtract.Import();
    var useExport = new LeEabWriteReferralExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabWriteReferralExtract.Execute, useImport, useExport);

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
    entities.Case1.Populated = false;
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferralCase",
      (db, command) =>
      {
        db.SetDate(
          command, "referralDate1", local.Start.Date.GetValueOrDefault());
        db.
          SetDate(command, "referralDate2", local.End.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 4);
        entities.LegalReferral.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 9);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 10);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 11);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 12);
        entities.Case1.Populated = true;
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
        db.SetString(command, "casNo", entities.Case1.Number);
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public ServiceProvider Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    private ServiceProvider null1;
    private Common record;
    private CsePersonsWorkSet close;
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
    private Case1 case1;
    private CaseRole caseRole;
    private LegalReferralCaseRole legalReferralCaseRole;
    private CsePerson csePerson;
    private OfficeServiceProvider legalOfficeServiceProvider;
    private LegalReferralAssignment legalReferralAssignment;
    private LegalReferral legalReferral;
    private ServiceProvider legalServiceProvider;
    private CseOrganization coCseOrganization;
  }
#endregion
}
