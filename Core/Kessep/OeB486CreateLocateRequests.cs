// Program: OE_B486_CREATE_LOCATE_REQUESTS, ID: 374418044, model: 746.
// Short name: SWEE486B
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
/// A program: OE_B486_CREATE_LOCATE_REQUESTS.
/// </para>
/// <para>
/// This procedure populates a sequential file containing locate request 
/// information for CSE members requiring location verification..  This file is
/// sent to state licensing agencies and state agencies with jurisdiction over
/// real and personal property.
/// Information contained on the file is the following:
/// Position    Respresenting Value:
/// 001-009	CSE social security number (fields 1-9)
/// 010-039	CSE person name (last, first, MI)
/// 040-047	Date of birth
/// 048-049	Address Type Indicator
/// 049-073	CSE person address
/// 074-098	License number
/// 099-106	License expiration date
/// 107-116	SRS CSE Person Number
/// 117-121	Name of agency
/// 122-150	Filler
/// Positions 107-150 is the total filler on the record.  However, positions 107
/// -120 will be considered as RESERVED filler.
/// Locate matches are be procured via a two-step batch process: the first 
/// process (OE_B510_CREATE_LOCATE_REQUESTS) will determine which CSE persons
/// are candidates for the locate process; the second process (OE_B515_
/// PROCESS_LOCATE_RESPONSES) will process the information obtained from the
/// participating state agencies.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB486CreateLocateRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B486_CREATE_LOCATE_REQUESTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB486CreateLocateRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB486CreateLocateRequests.
  /// </summary>
  public OeB486CreateLocateRequests(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // ----------------------------------------
    // 07/??/2000  SWSCBRS	?????		Initial Coding
    // 03/??/2001  SWSRPRM	WR # 291 	Add License Suspension
    // 10/11/2005  GVandy	?????		Performance Improvements.
    // 03/05/2007  GVandy	PR261671	Re-written to improve performance.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.NullDate.Date = null;

    // -----------------------------------------------------------------------------------------------
    // Retrieve the PPI info.
    // -----------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB486";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Open Control Report
    // -----------------------------------------------------------------------------------------------
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

    // -----------------------------------------------------------------------------------------------
    // Get the DB2 commit frequency counts and determine if we are restarting.
    // -----------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -- Extract the restart person number.
      local.Restart.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);

      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail =
            "PROGRAM RESTARTING AT CSE PERSON NUMBER " + local.Restart.Number;
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered while writing Restart Person Number to control report.";
            
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Extract parameters from the PPI record.
    // -----------------------------------------------------------------------------------------------
    // -- Extract Process License Suspension Only Flag (Position 1)
    local.ProcessLicSuspOnly.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

    // --  Extract Display Indicator (Position 2)
    local.DisplayIndicator.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 2, 1);

    // -- Extract License Suspension threshold amount (Positions 3 - 7)
    if (Verify(Substring(local.ProgramProcessingInfo.ParameterList, 3, 5),
      "0123456789") == 0)
    {
      local.LicSuspThresholdAmount.TotalCurrency =
        StringToNumber(
          Substring(local.ProgramProcessingInfo.ParameterList, 3, 5));
    }

    // -----------------------------------------------------------------------------------------------
    // Retrieve family violence application indicator.
    // -----------------------------------------------------------------------------------------------
    if (ReadCodeValue1())
    {
      local.FviApply.Flag = Substring(entities.CodeValue.Description, 1, 1);
    }
    else
    {
      local.FviApply.Flag = "N";
    }

    // -----------------------------------------------------------------------------------------------
    // Retrieve the Department of Health locate/license suspense timeframe 
    // information.
    // -----------------------------------------------------------------------------------------------
    if (!ReadCodeValue2())
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Licensing Agencies Timeframes code value not found.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Write parameters to the control report
    // -----------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Process License Suspension Only Parameter = " + local
            .ProcessLicSuspOnly.Flag;

          break;
        case 2:
          local.EabReportSend.RptDetail = "Display Indicator Parameter = " + local
            .DisplayIndicator.Flag;

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "License Suspension Threshold Amount Parameter = " + NumberToString
            ((long)local.LicSuspThresholdAmount.TotalCurrency, 11, 5);

          break;
        case 5:
          if (AsChar(local.DisplayIndicator.Flag) == 'Y')
          {
            local.DateWorkArea.Time = Time(Now());
            local.EabReportSend.RptDetail = "";
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 18) + NumberToString
              (TimeToInt(local.DateWorkArea.Time), 15);
            local.EabReportSend.RptDetail =
              Substring(local.EabReportSend.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, 35) + "Starting cursor open...";
              
          }
          else
          {
            local.EabReportSend.RptDetail = "";
          }

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writing PPI Parameters to the control report.";
          
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.NumberOfCsePersonsRead.Count = 0;

    // -----------------------------------------------------------------------------------------------
    // Read each appropriate CSE person and determine if they meet the locate/
    // license suspense criteria.
    // -----------------------------------------------------------------------------------------------
    foreach(var item in ReadCsePerson())
    {
      ExitState = "ACO_NN0000_ALL_OK";

      if (AsChar(local.FviApply.Flag) == 'Y')
      {
        if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
        {
          continue;
        }
      }

      ++local.NumberOfCsePersonsRead.Count;

      if (AsChar(local.ProcessLicSuspOnly.Flag) == 'Y')
      {
        // -- Check if this person meets the license suspension requirements.
        UseOeValidateLicenseSuspension();
      }
      else
      {
        // -- Check if this person meets the locate requirements.
        UseOeValidateLocateReqCriteria();
      }

      if (AsChar(local.DisplayIndicator.Flag) == 'Y')
      {
        // -- Display count of number of persons read, current time, person 
        // number, and whether that person meets the locate/license suspense
        // criteria.
        local.DateWorkArea.Time = Time(Now());
        local.EabReportSend.RptDetail = "";
        local.EabReportSend.RptDetail = " " + NumberToString
          (local.NumberOfCsePersonsRead.Count, 15);
        local.EabReportSend.RptDetail =
          Substring(local.EabReportSend.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 18) + NumberToString
          (TimeToInt(local.DateWorkArea.Time), 15);
        local.EabReportSend.RptDetail =
          Substring(local.EabReportSend.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 35) + "CSP Number ";
        local.EabReportSend.RptDetail =
          Substring(local.EabReportSend.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 46) + entities
          .CsePerson.Number;
        local.EabReportSend.RptDetail =
          Substring(local.EabReportSend.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 58) + local
          .PersonMeetsCriteria.Flag;
        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered while writing control report(number of request created).";
            
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      if (AsChar(local.PersonMeetsCriteria.Flag) != 'Y')
      {
        // -- Person did not qualify based on the license suspension or locate 
        // rules.  Skip this person.
        continue;
      }

      UseOeCreateLocateRequests();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // Log the error to the error report and set an abort exit state when 
        // appropriate.
        if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          local.EabReportSend.RptDetail = "Adabas Unavailable.";
        }
        else if (IsExitState("ADABAS_READ_UNSUCCESSFUL"))
        {
          local.EabReportSend.RptDetail =
            "Information not found on adabas, CSE Person : " + entities
            .CsePerson.Number;
        }
        else
        {
          // Extract exit state message and log to error report...
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message + "   CSP Number = " +
            entities.CsePerson.Number;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        if (IsExitState("ADABAS_UNAVAILABLE_RB"))
        {
          ExitState = "ACO_AE0000_BATCH_ABEND";

          return;
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      if (local.NoLocateRequestCreated.Count + local
        .NoLocateRequestUpdated.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.TotalLocateReqCreated.Count += local.NoLocateRequestCreated.Count;
        local.TotalLocateReqUpdated.Count += local.NoLocateRequestUpdated.Count;
        local.NoLocateRequestCreated.Count = 0;
        local.NoLocateRequestUpdated.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.ProgramName = global.UserId;
        local.ProgramCheckpointRestart.RestartInfo = entities.CsePerson.Number;
        UseUpdatePgmCheckpointRestart();
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }
      }
    }

    local.TotalLocateReqCreated.Count += local.NoLocateRequestCreated.Count;
    local.TotalLocateReqUpdated.Count += local.NoLocateRequestUpdated.Count;

    // -----------------------------------------------------------------------------------------------
    // Write totals to the control report.
    // -----------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "Total CSE Persons Read : " + NumberToString
            (local.NumberOfCsePersonsRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Total Number of Locate Requests created : " + NumberToString
            (local.TotalLocateReqCreated.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Total Number of Locate Requests Updated : " + NumberToString
            (local.TotalLocateReqUpdated.Count, 15);

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writing totals to the control report.";
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Take a final checkpoint.
    // -----------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    // -----------------------------------------------------------------------------------------------
    // Close Control Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
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

    // -----------------------------------------------------------------------------------------------
    // Close Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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

  private static void MoveProgramCheckpointRestart1(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramCheckpointRestart2(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
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

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeCreateLocateRequests()
  {
    var useImport = new OeCreateLocateRequests.Import();
    var useExport = new OeCreateLocateRequests.Export();

    useImport.CodeValue.Cdvalue = entities.CodeValue.Cdvalue;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.LicSuspQualification.Flag = local.PersonMeetsCriteria.Flag;
    useExport.ImportExportNoLocReqCreated.Count =
      local.NoLocateRequestCreated.Count;
    useExport.ImportExportNoLocReqUpdated.Count =
      local.NoLocateRequestUpdated.Count;

    Call(OeCreateLocateRequests.Execute, useImport, useExport);

    local.NoLocateRequestCreated.Count =
      useExport.ImportExportNoLocReqCreated.Count;
    local.NoLocateRequestUpdated.Count =
      useExport.ImportExportNoLocReqUpdated.Count;
  }

  private void UseOeValidateLicenseSuspension()
  {
    var useImport = new OeValidateLicenseSuspension.Import();
    var useExport = new OeValidateLicenseSuspension.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.LicSuspThreshold.TotalCurrency =
      local.LicSuspThresholdAmount.TotalCurrency;

    Call(OeValidateLicenseSuspension.Execute, useImport, useExport);

    local.PersonMeetsCriteria.Flag = useExport.LicSuspQualification.Flag;
  }

  private void UseOeValidateLocateReqCriteria()
  {
    var useImport = new OeValidateLocateReqCriteria.Import();
    var useExport = new OeValidateLocateReqCriteria.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;

    Call(OeValidateLocateReqCriteria.Execute, useImport, useExport);

    local.PersonMeetsCriteria.Flag = useExport.LocateReqQualification.Flag;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart2(useExport.ProgramCheckpointRestart,
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

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    MoveProgramCheckpointRestart1(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadCodeValue1()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue1",
      null,
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue2()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
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
      });
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "dateOfDeath", local.NullDate.Date.GetValueOrDefault());
        db.SetString(command, "flag", local.ProcessLicSuspOnly.Flag);
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "numb", local.Restart.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
    /// A value of FviApply.
    /// </summary>
    [JsonPropertyName("fviApply")]
    public Common FviApply
    {
      get => fviApply ??= new();
      set => fviApply = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of ProcessLicSuspOnly.
    /// </summary>
    [JsonPropertyName("processLicSuspOnly")]
    public Common ProcessLicSuspOnly
    {
      get => processLicSuspOnly ??= new();
      set => processLicSuspOnly = value;
    }

    /// <summary>
    /// A value of DisplayIndicator.
    /// </summary>
    [JsonPropertyName("displayIndicator")]
    public Common DisplayIndicator
    {
      get => displayIndicator ??= new();
      set => displayIndicator = value;
    }

    /// <summary>
    /// A value of LicSuspThresholdAmount.
    /// </summary>
    [JsonPropertyName("licSuspThresholdAmount")]
    public Common LicSuspThresholdAmount
    {
      get => licSuspThresholdAmount ??= new();
      set => licSuspThresholdAmount = value;
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
    /// A value of NumberOfCsePersonsRead.
    /// </summary>
    [JsonPropertyName("numberOfCsePersonsRead")]
    public Common NumberOfCsePersonsRead
    {
      get => numberOfCsePersonsRead ??= new();
      set => numberOfCsePersonsRead = value;
    }

    /// <summary>
    /// A value of PersonMeetsCriteria.
    /// </summary>
    [JsonPropertyName("personMeetsCriteria")]
    public Common PersonMeetsCriteria
    {
      get => personMeetsCriteria ??= new();
      set => personMeetsCriteria = value;
    }

    /// <summary>
    /// A value of NoLocateRequestCreated.
    /// </summary>
    [JsonPropertyName("noLocateRequestCreated")]
    public Common NoLocateRequestCreated
    {
      get => noLocateRequestCreated ??= new();
      set => noLocateRequestCreated = value;
    }

    /// <summary>
    /// A value of NoLocateRequestUpdated.
    /// </summary>
    [JsonPropertyName("noLocateRequestUpdated")]
    public Common NoLocateRequestUpdated
    {
      get => noLocateRequestUpdated ??= new();
      set => noLocateRequestUpdated = value;
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
    /// A value of TotalLocateReqCreated.
    /// </summary>
    [JsonPropertyName("totalLocateReqCreated")]
    public Common TotalLocateReqCreated
    {
      get => totalLocateReqCreated ??= new();
      set => totalLocateReqCreated = value;
    }

    /// <summary>
    /// A value of TotalLocateReqUpdated.
    /// </summary>
    [JsonPropertyName("totalLocateReqUpdated")]
    public Common TotalLocateReqUpdated
    {
      get => totalLocateReqUpdated ??= new();
      set => totalLocateReqUpdated = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private Common fviApply;
    private DateWorkArea nullDate;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restart;
    private Common common;
    private Common processLicSuspOnly;
    private Common displayIndicator;
    private Common licSuspThresholdAmount;
    private DateWorkArea dateWorkArea;
    private Common numberOfCsePersonsRead;
    private Common personMeetsCriteria;
    private Common noLocateRequestCreated;
    private Common noLocateRequestUpdated;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalLocateReqCreated;
    private Common totalLocateReqUpdated;
    private External passArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private CodeValue codeValue;
    private Code code;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
