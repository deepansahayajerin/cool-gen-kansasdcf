// Program: FN_B734_PRIORITY_5_18, ID: 945148976, model: 746.
// Short name: SWE03710
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
/// A program: FN_B734_PRIORITY_5_18.
/// </para>
/// <para>
/// Priority 3-18: Federal Timeframes- Days from IWO to IWO payment
/// </para>
/// </summary>
[Serializable]
public partial class FnB734Priority518: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B734_PRIORITY_5_18 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB734Priority518(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB734Priority518.
  /// </summary>
  public FnB734Priority518(IContext context, Import import, Export export):
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
    // Priority 5-18: Federal Timeframes- Days from IWO to IWO payment
    // -------------------------------------------------------------------------------------
    // 45CFR 303.100
    // This will count the average number of days from IWO issuance to an 
    // employer to first I type payment received.
    // Report Level: Attorney
    // Report: Month
    // 1.	Count the first occurrence of an I type payment on a court order with 
    // a received date in the current report period.  To qualify, the previous I
    // type payment on the order must have occurred greater than 40 days ago.
    // 2.	Find most recent ORDIWO2 created for that court order.  An associated 
    // entry must exist on the IWGL screen.  ORDIWO2s with created by the DOL
    // process will be excluded.
    // 3.	If a different I type payment was created in between the ORDIWO2 
    // creation date and the most recent I type payment date, do not count case/
    // order.
    // 4.	Calculate number of days between ORDIWO2 creation date and I type 
    // payment date.  An associated entry must exist on the IWGL screen.
    // 5.	To find office to credit, look for attorney assigned to the ORDIWO2 on
    // the created date.
    // -------------------------------------------------------------------------------------
    MoveDashboardAuditData2(import.DashboardAuditData,
      local.InitializedDashboardAuditData);
    MoveProgramCheckpointRestart(import.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);

    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    //  089-089    Blank
    //  090-110    cash receipt detail create timestamp
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      if (Equal(import.ProgramCheckpointRestart.RestartInfo, 81, 4, "5-18"))
      {
        if (!IsEmpty(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 90, 26)))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            Substring(import.ProgramCheckpointRestart.RestartInfo, 90, 26);
          UseLeCabConvertTimestamp();
          local.Checkpoint.CreatedTmst =
            local.BatchTimestampWorkArea.IefTimestamp;
        }

        if (Lt(local.Null1.Timestamp, local.Checkpoint.CreatedTmst))
        {
        }
        else
        {
          // this is when there is a month in change in the middle of a week. we
          // do not want to double count the results
          local.Checkpoint.CreatedTmst =
            AddMonths(import.ReportEndDate.Timestamp, 6);

          foreach(var item in ReadDashboardStagingPriority2())
          {
            // need to clear the previcously determined totals before the 
            // program begins or the numbers will not be correct, they will
            // reflect previous run numbers also
            try
            {
              UpdateDashboardStagingPriority2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error creating/updating Dashboard_Staging_Priority_3_5.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
        }
      }
      else
      {
        local.Checkpoint.CreatedTmst =
          AddMonths(import.ReportEndDate.Timestamp, 6);

        foreach(var item in ReadDashboardStagingPriority2())
        {
          // need to clear the previcously determined totals before the program 
          // begins or the numbers will not be correct, they will reflect
          // previous run numbers also
          try
          {
            UpdateDashboardStagingPriority2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error creating/updating Dashboard_Staging_Priority_3_5.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }
    }
    else
    {
      local.Checkpoint.CreatedTmst =
        AddMonths(import.ReportEndDate.Timestamp, 6);

      foreach(var item in ReadDashboardStagingPriority2())
      {
        // need to clear the previcously determined totals before the program 
        // begins or the numbers will not be correct, they will reflect previous
        // run numbers also
        try
        {
          UpdateDashboardStagingPriority2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error creating/updating Dashboard_Staging_Priority_3_5.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    foreach(var item in ReadCashReceiptDetail2())
    {
      local.CashReceipt.Date = entities.CashReceiptDetail.CollectionDate;
      local.CashReceipt.Time =
        StringToTime("23.59.59.999999").GetValueOrDefault();
      UseFnBuildTimestampFrmDateTime();
      local.CashReceipt.Timestamp = entities.CashReceiptDetail.CreatedTmst;

      foreach(var item1 in ReadCashReceiptDetail3())
      {
        if (Lt(entities.N2dRead.CreatedTmst,
          entities.CashReceiptDetail.CreatedTmst))
        {
        }
        else if (Lt(entities.CashReceiptDetail.CreatedTmst,
          entities.N2dRead.CreatedTmst))
        {
          // the program has already processed a record for this day for this 
          // obligor and court order number
          goto ReadEach;
        }
      }

      local.LegalActionFound.Flag = "";

      if (ReadLegalAction())
      {
        local.LegalActionFound.Flag = "Y";
      }

      if (AsChar(local.LegalActionFound.Flag) != 'Y')
      {
        continue;
      }

      if (ReadFipsTribunal())
      {
        if (entities.Fips.State != 20)
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      local.MiniumOrderDt.Date =
        AddDays(entities.CashReceiptDetail.CollectionDate, -40);

      if (ReadCashReceiptDetail1())
      {
        continue;

        // THERE IS ANOTHER 'I' PAYMENT SO SO NOT USE
      }

      ReadLegalActionIncomeSourceCsePerson();

      if (entities.LegalActionIncomeSource.Populated)
      {
        // we can count this record since it has a iwgl record tied to the 
        // current legal action for the obligor
      }
      else
      {
        continue;
      }

      local.InitializedDashboardAuditData.JudicialDistrict = "";
      local.InitializedDashboardAuditData.Office = 0;
      local.DashboardAuditData.Assign(local.InitializedDashboardAuditData);
      local.DashboardStagingPriority35.Assign(
        local.InitializedDashboardStagingPriority35);
      local.PreviousRecord.StandardNumber = entities.LegalAction.StandardNumber;
      local.Checkpoint.CreatedTmst = entities.CashReceiptDetail.CreatedTmst;
      local.DashboardAuditData.DebtType =
        entities.LegalActionDetail.NonFinOblgType;
      local.Convert.Date = Date(entities.LegalAction.CreatedTstamp);
      local.DashboardAuditData.DaysReported =
        DaysFromAD(entities.CashReceiptDetail.CollectionDate) - DaysFromAD
        (local.Convert.Date);
      local.DashboardStagingPriority35.ReportLevelId = "";

      if (ReadLegalActionAssigmentServiceProvider())
      {
        local.DashboardStagingPriority35.ReportLevelId =
          entities.ServiceProvider.UserId;
      }

      if (IsEmpty(local.DashboardStagingPriority35.ReportLevelId))
      {
        continue;
      }

      local.DashboardStagingPriority35.AsOfDate =
        import.ProgramProcessingInfo.ProcessDate;

      if (Equal(entities.ServiceProvider.RoleCode, "AT") || Equal
        (entities.ServiceProvider.RoleCode, "CT"))
      {
        local.DashboardStagingPriority35.ReportLevel = "AT";
      }
      else
      {
        local.DashboardStagingPriority35.ReportLevel = "CA";
      }

      local.DashboardStagingPriority35.ReportMonth =
        local.DashboardAuditData.ReportMonth;
      local.DashboardAuditData.WorkerId =
        local.DashboardStagingPriority35.ReportLevelId;
      local.DashboardAuditData.DashboardPriority = "5-18";
      local.DashboardAuditData.StandardNumber =
        entities.LegalAction.StandardNumber;

      if (AsChar(import.AuditFlag.Flag) == 'Y')
      {
        // -- Log to the dashboard audit table.
        UseFnB734CreateDashboardAudit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      local.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
        local.DashboardStagingPriority35.DaysToIwoPaymentDenominator.
          GetValueOrDefault() + 1;
      local.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
        local.DashboardAuditData.DaysReported.GetValueOrDefault() + local
        .DashboardStagingPriority35.DaysToIwoPaymentNumerator.
          GetValueOrDefault();

      if (local.DashboardStagingPriority35.DaysToIwoPaymentDenominator.
        GetValueOrDefault() <= 0 || local
        .DashboardStagingPriority35.DaysToIwoPaymentNumerator.
          GetValueOrDefault() <= 0)
      {
        local.DashboardStagingPriority35.DaysToIwoPaymentAverage = 0;
      }
      else
      {
        local.DashboardStagingPriority35.DaysToIwoPaymentAverage =
          (decimal)local.DashboardStagingPriority35.DaysToIwoPaymentNumerator.
            GetValueOrDefault() / local
          .DashboardStagingPriority35.DaysToIwoPaymentDenominator.
            GetValueOrDefault();
      }

      if (ReadDashboardStagingPriority1())
      {
        local.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
          local.DashboardStagingPriority35.DaysToIwoPaymentDenominator.
            GetValueOrDefault() + entities
          .DashboardStagingPriority35.DaysToIwoPaymentDenominator.
            GetValueOrDefault();
        local.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
          entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator.
            GetValueOrDefault() + local
          .DashboardStagingPriority35.DaysToIwoPaymentNumerator.
            GetValueOrDefault();

        if (local.DashboardStagingPriority35.DaysToIwoPaymentDenominator.
          GetValueOrDefault() <= 0 || local
          .DashboardStagingPriority35.DaysToIwoPaymentNumerator.
            GetValueOrDefault() <= 0)
        {
          local.DashboardStagingPriority35.DaysToIwoPaymentAverage = 0;
        }
        else
        {
          local.DashboardStagingPriority35.DaysToIwoPaymentAverage =
            (decimal)local.DashboardStagingPriority35.DaysToIwoPaymentNumerator.
              GetValueOrDefault() / local
            .DashboardStagingPriority35.DaysToIwoPaymentDenominator.
              GetValueOrDefault();
        }

        try
        {
          UpdateDashboardStagingPriority1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        try
        {
          CreateDashboardStagingPriority35();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "DASHBOARD_STAGING_PRI_3_5_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error creating/updating Dashboard_Staging_Priority_3_5.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.RecordProcessed.Count;

      if (local.RecordProcessed.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // -- Checkpoint Info
        // Positions   Value
        // ---------   
        // ------------------------------------
        //  001-080    General Checkpoint Info for PRAD
        //  081-088    Dashboard Priority
        //  089-089    Blank
        //  090-116    cash reciept detail create timestamp
        local.BatchTimestampWorkArea.TextTimestamp = "";
        local.BatchTimestampWorkArea.IefTimestamp =
          local.Checkpoint.CreatedTmst;
        UseLeCabConvertTimestamp();
        local.ProgramCheckpointRestart.RestartInfo =
          Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-18    " +
          " " + local.BatchTimestampWorkArea.TextTimestamp;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.CheckpointCount = 0;
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking checkpoint.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.RecordProcessed.Count = 0;
      }

ReadEach:
      ;
    }

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint for restarting at the next priority.
    // ------------------------------------------------------------------------------
    // -- Checkpoint Info
    // Positions   Value
    // ---------   
    // ------------------------------------
    //  001-080    General Checkpoint Info for PRAD
    //  081-088    Dashboard Priority
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInfo =
      Substring(import.ProgramCheckpointRestart.RestartInfo, 250, 1, 80) + "5-19     ";
      
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking checkpoint.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveDashboardAuditData1(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.ReportMonth = source.ReportMonth;
    target.DashboardPriority = source.DashboardPriority;
    target.RunNumber = source.RunNumber;
    target.Office = source.Office;
    target.JudicialDistrict = source.JudicialDistrict;
    target.WorkerId = source.WorkerId;
    target.CaseNumber = source.CaseNumber;
    target.StandardNumber = source.StandardNumber;
    target.PayorCspNumber = source.PayorCspNumber;
    target.SuppCspNumber = source.SuppCspNumber;
    target.Fte = source.Fte;
    target.CollectionAmount = source.CollectionAmount;
    target.CollAppliedToCd = source.CollAppliedToCd;
    target.CollectionCreatedDate = source.CollectionCreatedDate;
    target.CollectionType = source.CollectionType;
    target.DebtBalanceDue = source.DebtBalanceDue;
    target.DebtDueDate = source.DebtDueDate;
    target.DebtType = source.DebtType;
    target.LegalActionDate = source.LegalActionDate;
    target.LegalReferralDate = source.LegalReferralDate;
    target.LegalReferralNumber = source.LegalReferralNumber;
    target.DaysReported = source.DaysReported;
    target.VerifiedDate = source.VerifiedDate;
    target.CaseDate = source.CaseDate;
    target.ReviewDate = source.ReviewDate;
  }

  private static void MoveDashboardAuditData2(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.ReportMonth = source.ReportMonth;
    target.RunNumber = source.RunNumber;
  }

  private static void MoveDateWorkArea1(DateWorkArea source, DateWorkArea target)
    
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveDateWorkArea2(DateWorkArea source, DateWorkArea target)
    
  {
    target.Date = source.Date;
    target.Time = source.Time;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
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

  private void UseFnB734CreateDashboardAudit()
  {
    var useImport = new FnB734CreateDashboardAudit.Import();
    var useExport = new FnB734CreateDashboardAudit.Export();

    MoveDashboardAuditData1(local.DashboardAuditData,
      useImport.DashboardAuditData);

    Call(FnB734CreateDashboardAudit.Execute, useImport, useExport);
  }

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea1(local.CashReceipt, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea2(useExport.DateWorkArea, local.CashReceipt);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void CreateDashboardStagingPriority35()
  {
    var reportMonth = local.DashboardStagingPriority35.ReportMonth;
    var reportLevel = local.DashboardStagingPriority35.ReportLevel;
    var reportLevelId = local.DashboardStagingPriority35.ReportLevelId;
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var param = 0M;
    var daysToIwoPaymentNumerator =
      local.DashboardStagingPriority35.DaysToIwoPaymentNumerator.
        GetValueOrDefault();
    var daysToIwoPaymentDenominator =
      local.DashboardStagingPriority35.DaysToIwoPaymentDenominator.
        GetValueOrDefault();
    var daysToIwoPaymentAverage =
      local.DashboardStagingPriority35.DaysToIwoPaymentAverage.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("CreateDashboardStagingPriority35",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", reportMonth);
        db.SetString(command, "reportLevel", reportLevel);
        db.SetString(command, "reportLevelId", reportLevelId);
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.SetNullableInt32(command, "casWEstRef", 0);
        db.SetNullableDecimal(command, "fullTimeEqvlnt", param);
        db.SetNullableDecimal(command, "STypeCollAmt", param);
        db.SetNullableDecimal(command, "STypeCollPer", param);
        db.
          SetNullableInt32(command, "iwoPmtDaysNmr", daysToIwoPaymentNumerator);
          
        db.SetNullableInt32(
          command, "iwoPmtDaysDnom", daysToIwoPaymentDenominator);
        db.
          SetNullableDecimal(command, "iwoPmtDaysAvg", daysToIwoPaymentAverage);
          
      });

    entities.DashboardStagingPriority35.ReportMonth = reportMonth;
    entities.DashboardStagingPriority35.ReportLevel = reportLevel;
    entities.DashboardStagingPriority35.ReportLevelId = reportLevelId;
    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
      daysToIwoPaymentNumerator;
    entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
      daysToIwoPaymentDenominator;
    entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
      daysToIwoPaymentAverage;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private bool ReadCashReceiptDetail1()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr",
          entities.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetNullableString(
          command, "courtOrderNumber",
          entities.CashReceiptDetail.CourtOrderNumber ?? "");
        db.SetDate(
          command, "collectionDate1",
          entities.CashReceiptDetail.CollectionDate.GetValueOrDefault());
        db.SetDate(
          command, "collectionDate2",
          local.MiniumOrderDt.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalAction.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.N2dRead.CrvIdentifier = db.GetInt32(reader, 0);
        entities.N2dRead.CstIdentifier = db.GetInt32(reader, 1);
        entities.N2dRead.CrtIdentifier = db.GetInt32(reader, 2);
        entities.N2dRead.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.N2dRead.CourtOrderNumber = db.GetNullableString(reader, 4);
        entities.N2dRead.CaseNumber = db.GetNullableString(reader, 5);
        entities.N2dRead.CollectionAmount = db.GetDecimal(reader, 6);
        entities.N2dRead.CollectionDate = db.GetDate(reader, 7);
        entities.N2dRead.ObligorPersonNumber = db.GetNullableString(reader, 8);
        entities.N2dRead.CreatedTmst = db.GetDateTime(reader, 9);
        entities.N2dRead.CltIdentifier = db.GetNullableInt32(reader, 10);
        entities.N2dRead.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail2()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.ReportEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst",
          local.Checkpoint.CreatedTmst.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 9);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail3()
  {
    entities.N2dRead.Populated = false;

    return ReadEach("ReadCashReceiptDetail3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr",
          entities.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetNullableString(
          command, "courtOrderNumber",
          entities.CashReceiptDetail.CourtOrderNumber ?? "");
        db.SetDate(
          command, "collectionDate",
          entities.CashReceiptDetail.CollectionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.N2dRead.CrvIdentifier = db.GetInt32(reader, 0);
        entities.N2dRead.CstIdentifier = db.GetInt32(reader, 1);
        entities.N2dRead.CrtIdentifier = db.GetInt32(reader, 2);
        entities.N2dRead.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.N2dRead.CourtOrderNumber = db.GetNullableString(reader, 4);
        entities.N2dRead.CaseNumber = db.GetNullableString(reader, 5);
        entities.N2dRead.CollectionAmount = db.GetDecimal(reader, 6);
        entities.N2dRead.CollectionDate = db.GetDate(reader, 7);
        entities.N2dRead.ObligorPersonNumber = db.GetNullableString(reader, 8);
        entities.N2dRead.CreatedTmst = db.GetDateTime(reader, 9);
        entities.N2dRead.CltIdentifier = db.GetNullableInt32(reader, 10);
        entities.N2dRead.Populated = true;

        return true;
      });
  }

  private bool ReadDashboardStagingPriority1()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return Read("ReadDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", local.DashboardStagingPriority35.ReportMonth);
          
        db.SetString(
          command, "reportLevel", local.DashboardStagingPriority35.ReportLevel);
          
        db.SetString(
          command, "reportLevelId",
          local.DashboardStagingPriority35.ReportLevelId);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardStagingPriority2()
  {
    entities.DashboardStagingPriority35.Populated = false;

    return ReadEach("ReadDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", import.DashboardAuditData.ReportMonth);
      },
      (db, reader) =>
      {
        entities.DashboardStagingPriority35.ReportMonth =
          db.GetInt32(reader, 0);
        entities.DashboardStagingPriority35.ReportLevel =
          db.GetString(reader, 1);
        entities.DashboardStagingPriority35.ReportLevelId =
          db.GetString(reader, 2);
        entities.DashboardStagingPriority35.AsOfDate =
          db.GetNullableDate(reader, 3);
        entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
          db.GetNullableInt32(reader, 4);
        entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
          db.GetNullableInt32(reader, 5);
        entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
          db.GetNullableDecimal(reader, 6);
        entities.DashboardStagingPriority35.Populated = true;

        return true;
      });
  }

  private bool ReadFipsTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadFipsTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Tribunal.Name = db.GetString(reader, 5);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 6);
        entities.Tribunal.Identifier = db.GetInt32(reader, 7);
        entities.Fips.Populated = true;
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo",
          entities.CashReceiptDetail.CourtOrderNumber ?? "");
        db.SetNullableDate(
          command, "endDate", import.ReportStartDate.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.CashReceiptDetail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.Type1 = db.GetString(reader, 3);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 5);
        entities.LegalAction.InitiatingCounty = db.GetNullableString(reader, 6);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 7);
        entities.LegalAction.RespondingCounty = db.GetNullableString(reader, 8);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 10);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 11);
        entities.LegalAction.CreatedBy = db.GetString(reader, 12);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 13);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 14);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 15);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionAssigmentServiceProvider()
  {
    entities.ServiceProvider.Populated = false;
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigmentServiceProvider",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalAction.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.ServiceProvider.UserId = db.GetString(reader, 10);
        entities.ServiceProvider.RoleCode = db.GetNullableString(reader, 11);
        entities.ServiceProvider.Populated = true;
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private bool ReadLegalActionIncomeSourceCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSourceCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetString(
          command, "cspINumber",
          entities.CashReceiptDetail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 5);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 7);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 8);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 9);
        entities.CsePerson.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
      });
  }

  private void UpdateDashboardStagingPriority1()
  {
    var asOfDate = local.DashboardStagingPriority35.AsOfDate;
    var daysToIwoPaymentNumerator =
      local.DashboardStagingPriority35.DaysToIwoPaymentNumerator.
        GetValueOrDefault();
    var daysToIwoPaymentDenominator =
      local.DashboardStagingPriority35.DaysToIwoPaymentDenominator.
        GetValueOrDefault();
    var daysToIwoPaymentAverage =
      local.DashboardStagingPriority35.DaysToIwoPaymentAverage.
        GetValueOrDefault();

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority1",
      (db, command) =>
      {
        db.SetNullableDate(command, "asOfDate", asOfDate);
        db.
          SetNullableInt32(command, "iwoPmtDaysNmr", daysToIwoPaymentNumerator);
          
        db.SetNullableInt32(
          command, "iwoPmtDaysDnom", daysToIwoPaymentDenominator);
        db.
          SetNullableDecimal(command, "iwoPmtDaysAvg", daysToIwoPaymentAverage);
          
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.AsOfDate = asOfDate;
    entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator =
      daysToIwoPaymentNumerator;
    entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator =
      daysToIwoPaymentDenominator;
    entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
      daysToIwoPaymentAverage;
    entities.DashboardStagingPriority35.Populated = true;
  }

  private void UpdateDashboardStagingPriority2()
  {
    var daysToIwoPaymentAverage = 0M;

    entities.DashboardStagingPriority35.Populated = false;
    Update("UpdateDashboardStagingPriority2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "iwoPmtDaysNmr", 0);
        db.SetNullableInt32(command, "iwoPmtDaysDnom", 0);
        db.
          SetNullableDecimal(command, "iwoPmtDaysAvg", daysToIwoPaymentAverage);
          
        db.SetInt32(
          command, "reportMonth",
          entities.DashboardStagingPriority35.ReportMonth);
        db.SetString(
          command, "reportLevel",
          entities.DashboardStagingPriority35.ReportLevel);
        db.SetString(
          command, "reportLevelId",
          entities.DashboardStagingPriority35.ReportLevelId);
      });

    entities.DashboardStagingPriority35.DaysToIwoPaymentNumerator = 0;
    entities.DashboardStagingPriority35.DaysToIwoPaymentDenominator = 0;
    entities.DashboardStagingPriority35.DaysToIwoPaymentAverage =
      daysToIwoPaymentAverage;
    entities.DashboardStagingPriority35.Populated = true;
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
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
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
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    /// <summary>
    /// A value of ReportStartDate.
    /// </summary>
    [JsonPropertyName("reportStartDate")]
    public DateWorkArea ReportStartDate
    {
      get => reportStartDate ??= new();
      set => reportStartDate = value;
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
    /// A value of AuditFlag.
    /// </summary>
    [JsonPropertyName("auditFlag")]
    public Common AuditFlag
    {
      get => auditFlag ??= new();
      set => auditFlag = value;
    }

    private DashboardAuditData dashboardAuditData;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea reportEndDate;
    private DateWorkArea reportStartDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common auditFlag;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public DateWorkArea CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of Checkpoint.
    /// </summary>
    [JsonPropertyName("checkpoint")]
    public CashReceiptDetail Checkpoint
    {
      get => checkpoint ??= new();
      set => checkpoint = value;
    }

    /// <summary>
    /// A value of LegalActionFound.
    /// </summary>
    [JsonPropertyName("legalActionFound")]
    public Common LegalActionFound
    {
      get => legalActionFound ??= new();
      set => legalActionFound = value;
    }

    /// <summary>
    /// A value of MiniumOrderDt.
    /// </summary>
    [JsonPropertyName("miniumOrderDt")]
    public DateWorkArea MiniumOrderDt
    {
      get => miniumOrderDt ??= new();
      set => miniumOrderDt = value;
    }

    /// <summary>
    /// A value of InitializedDashboardAuditData.
    /// </summary>
    [JsonPropertyName("initializedDashboardAuditData")]
    public DashboardAuditData InitializedDashboardAuditData
    {
      get => initializedDashboardAuditData ??= new();
      set => initializedDashboardAuditData = value;
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
    /// A value of DateWorkAttributes.
    /// </summary>
    [JsonPropertyName("dateWorkAttributes")]
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
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
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    /// <summary>
    /// A value of PreviousRecord.
    /// </summary>
    [JsonPropertyName("previousRecord")]
    public LegalAction PreviousRecord
    {
      get => previousRecord ??= new();
      set => previousRecord = value;
    }

    /// <summary>
    /// A value of RecordProcessed.
    /// </summary>
    [JsonPropertyName("recordProcessed")]
    public Common RecordProcessed
    {
      get => recordProcessed ??= new();
      set => recordProcessed = value;
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
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public DateWorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of DashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority35")]
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of InitializedDashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("initializedDashboardStagingPriority35")]
    public DashboardStagingPriority35 InitializedDashboardStagingPriority35
    {
      get => initializedDashboardStagingPriority35 ??= new();
      set => initializedDashboardStagingPriority35 = value;
    }

    private DateWorkArea null1;
    private DateWorkArea cashReceipt;
    private CashReceiptDetail checkpoint;
    private Common legalActionFound;
    private DateWorkArea miniumOrderDt;
    private DashboardAuditData initializedDashboardAuditData;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkAttributes dateWorkAttributes;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private DashboardAuditData dashboardAuditData;
    private LegalAction previousRecord;
    private Common recordProcessed;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea convert;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private DashboardStagingPriority35 initializedDashboardStagingPriority35;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of N2dRead.
    /// </summary>
    [JsonPropertyName("n2dRead")]
    public CashReceiptDetail N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of DashboardStagingPriority35.
    /// </summary>
    [JsonPropertyName("dashboardStagingPriority35")]
    public DashboardStagingPriority35 DashboardStagingPriority35
    {
      get => dashboardStagingPriority35 ??= new();
      set => dashboardStagingPriority35 = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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

    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private CashReceiptDetail n2dRead;
    private Collection collection;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private LegalAction legalAction;
    private Fips fips;
    private Tribunal tribunal;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalReferral legalReferral;
    private DashboardStagingPriority35 dashboardStagingPriority35;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IncomeSource employment;
    private Employer employer;
    private ServiceProvider serviceProvider;
    private LegalActionAssigment legalActionAssigment;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
