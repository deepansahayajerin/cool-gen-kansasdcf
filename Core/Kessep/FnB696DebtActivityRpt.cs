// Program: FN_B696_DEBT_ACTIVITY_RPT, ID: 371131424, model: 746.
// Short name: SWEF696B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B696_DEBT_ACTIVITY_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB696DebtActivityRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B696_DEBT_ACTIVITY_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB696DebtActivityRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB696DebtActivityRpt.
  /// </summary>
  public FnB696DebtActivityRpt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Date    Developer    Request #   Description
    // --------------------------------------------------------------------
    // 12/15/00  Maureen Brown              Initial Development
    // --------------------------------------------------------------------
    // 12/02/01  Maureen Brown              PRs: 133138, 133140, 133141 - Job 
    // name was hardcoded into heading,
    // and it was the wrong name - changed it to use the Job name field from the
    // JOB table.
    // End timestamp was not being updated when the JOB_RUN table status is 
    // updated.  Added this.
    // CSE Person number was being truncated on the report - fixed this.
    // 09/24/02  Maureen Brown              PR: 158652
    //  - View overflow problem was resulting in the program completing with an 
    // error, rather than providing an overflow message on the report.  Fixed
    // this.
    // --------------------------------------------------------------------
    // *****************************************************************
    // resp: Finance
    // This procedure produces a report of debt activity for a given A/P.  It 
    // can filter by Obligor, Obligation ID, Obligation type, Court Order,
    // Reporting Period.  It can also omit debt adjustments and/or Collection
    // adjustments, as well as show only debts with an amount owed.
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Max.Date = new DateTime(2099, 12, 31);
    local.Debt.Index = -1;
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport1();
    local.EabFileHandling.Action = "WRITE";

    // *****************************************************************
    // Get the SYSIN Parm Values
    // *****************************************************************
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode == 0)
    {
      if (IsEmpty(local.Sysin.ParameterList))
      {
        ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    local.Job.Name = Substring(local.Sysin.ParameterList, 1, 8);

    if (!ReadJob())
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = local.Sysin.ParameterList ?? Spaces(132);
      UseCabErrorReport3();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.JobRun.SystemGenId =
      (int)StringToNumber(Substring(local.Sysin.ParameterList, 10, 9));

    if (!ReadJobRun())
    {
      local.NeededToWrite.RptDetail = "Job Run Not Found";
      UseCabErrorReport2();
      ExitState = "CO0000_JOB_RUN_NF_AB";

      return;
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    // **** SET status TO "PROCESSING"
    try
    {
      UpdateJobRun3();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CO0000_JOB_RUN_NU_AB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CO0000_JOB_RUN_PV_AB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // : Perform a DB2 Commit to Free Up the JOB_RUN row.
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    // *****************************************************************
    // Extract Filters from Parameter Information
    // *****************************************************************
    // **** IF local job_run parameter_information = SPACES ........
    //      ERROR: Invalid Parm Information!!!!
    //      Update JOB_RUN Error Message & Get Out!!!!
    // : Mandatory Parm Values.
    local.ParmCsePerson.Number =
      Substring(entities.ExistingJobRun.ParmInfo, 1, 10);

    if (IsEmpty(local.ParmCsePerson.Number))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Invalid CSE Person in job_run parameter info.";
      UseCabErrorReport3();

      try
      {
        UpdateJobRun2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    // : Optional Parm Values.
    local.ParmLegalAction.StandardNumber =
      Substring(entities.ExistingJobRun.ParmInfo, 12, 20);

    if (IsEmpty(Substring(entities.ExistingJobRun.ParmInfo, 33, 3)))
    {
      local.ParmObligation.SystemGeneratedIdentifier = 0;
    }
    else
    {
      local.ParmObligation.SystemGeneratedIdentifier =
        (int)StringToNumber(Substring(entities.ExistingJobRun.ParmInfo, 33, 3));
        
    }

    if (IsEmpty(Substring(entities.ExistingJobRun.ParmInfo, 37, 3)))
    {
      local.ParmObligationType.SystemGeneratedIdentifier = 0;
    }
    else
    {
      local.ParmObligationType.SystemGeneratedIdentifier =
        (int)StringToNumber(Substring(entities.ExistingJobRun.ParmInfo, 37, 3));
        
    }

    local.ParmFrom.Date =
      StringToDate(Substring(entities.ExistingJobRun.ParmInfo, 41, 10));
    local.ParmTo.Date =
      StringToDate(Substring(entities.ExistingJobRun.ParmInfo, 52, 10));

    if (Equal(local.ParmTo.Date, local.NullDateWorkArea.Date))
    {
      local.ParmTo.Date = local.Max.Date;
    }

    local.ParmShowDebtAdj.Text1 =
      Substring(entities.ExistingJobRun.ParmInfo, 63, 1);
    local.ParmShowCollAdj.Text1 =
      Substring(entities.ExistingJobRun.ParmInfo, 65, 1);
    local.ParmDebtsWithAmtOwed.SelectChar =
      Substring(entities.ExistingJobRun.ParmInfo, 67, 1);
    local.ParmShowFutureColl.Text1 =
      Substring(entities.ExistingJobRun.ParmInfo, 69, 1);

    // *****************************************************************
    // Mainline Process
    // *****************************************************************
    UseFnB696ReadDebtActivity();

    // 09/24/02  Maureen Brown              PR: 158652
    //  - Checked for overflow exitstate, as we don't want to process
    //   this condition as an error.
    if (!IsExitState("ACO_NN0000_ALL_OK") && !
      IsExitState("FN0000_GROUP_VIEW_OVERFLOW"))
    {
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail =
        TrimEnd(local.ExitStateWorkArea.Message) + "  Person #: " + local
        .ParmCsePerson.Number;
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport3();

      try
      {
        UpdateJobRun2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // Format Report and Add to the Report Repository
    // *****************************************************************
    // : Format Header Lines and add to REPORT_DATA.
    local.Header.Index = 0;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText = entities.ExistingJob.Name + Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 58) + "STATE OF KANSAS" +
      Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 41) + "PAGE:     1";
      
    local.TextMm.Text2 =
      NumberToString(Month(entities.ExistingJobRun.StartTimestamp), 14, 2);
    local.TextDd.Text2 =
      NumberToString(Day(entities.ExistingJobRun.StartTimestamp), 14, 2);
    local.TextYyyy.Text4 =
      NumberToString(Year(entities.ExistingJobRun.StartTimestamp), 12, 4);
    local.TextDate.Text10 = local.TextMm.Text2 + "/" + local.TextDd.Text2 + "/"
      + local.TextYyyy.Text4;

    local.Header.Index = 1;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText = "RUN DATE: " + local
      .TextDate.Text10 + Substring
      (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 36) + "DEPARTMENT FOR CHILDREN AND FAMILIES";
      

    local.Header.Index = 2;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      62) + "KAECSES - CHILD SUPPORT";

    local.Header.Index = 3;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "01";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      66) + "DEBT ACTIVITY";

    local.Header.Index = 4;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "01";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      19) + "FILTERS: AP#: " + local.ParmCsePerson.Number;

    local.Header.Index = 5;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ParmLegalAction.StandardNumber))
    {
      local.Header.Update.Header1.LineText =
        "                    COURT ORDER: N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + " COURT ORDER: " + (
          local.ParmLegalAction.StandardNumber ?? "");
    }

    local.Header.Index = 6;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (local.ParmObligation.SystemGeneratedIdentifier == 0)
    {
      local.Header.Update.Header1.LineText =
        "                     OBLIGATION: N/A";
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 21) + "OBLIGATION: " + NumberToString
        (local.ParmObligation.SystemGeneratedIdentifier, 13, 3);
    }

    local.Header.Index = 7;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (local.ParmObligationType.SystemGeneratedIdentifier == 0)
    {
      local.Header.Update.Header1.LineText =
        "                OBLIGATION TYPE: N/A";
    }
    else if (ReadObligationType())
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "OBLIGATION TYPE: " + entities.ObligationType.Code;
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 19) + "OBLIGATION TYPE: " + "UNKNOWN";
    }

    local.TextMm.Text2 = NumberToString(DateToInt(local.ParmFrom.Date), 12, 2);
    local.TextDd.Text2 = NumberToString(DateToInt(local.ParmFrom.Date), 14, 2);
    local.TextYyyy.Text4 = NumberToString(DateToInt(local.ParmFrom.Date), 8, 4);
    local.TextDate.Text10 = local.TextMm.Text2 + "/" + local.TextDd.Text2 + "/"
      + local.TextYyyy.Text4;

    local.Header.Index = 8;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "01";
    local.Header.Update.Header1.LineText =
      NumberToString(DateToInt(local.ParmFrom.Date), 8, 8);
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      22) + "FROM DATE: " + local.TextDate.Text10;
    local.TextMm.Text2 = NumberToString(DateToInt(local.ParmTo.Date), 12, 2);
    local.TextDd.Text2 = NumberToString(DateToInt(local.ParmTo.Date), 14, 2);
    local.TextYyyy.Text4 = NumberToString(DateToInt(local.ParmTo.Date), 8, 4);
    local.TextDate.Text10 = local.TextMm.Text2 + "/" + local.TextDd.Text2 + "/"
      + local.TextYyyy.Text4;

    local.Header.Index = 9;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      NumberToString(DateToInt(local.ParmTo.Date), 8, 8);
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      22) + "  TO DATE: " + local.TextDate.Text10;

    if (IsEmpty(local.ParmShowDebtAdj.Text1))
    {
      local.ParmShowDebtAdj.Text1 = "N";
    }

    local.Header.Index = 10;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      10) + "SHOW DEBT ADJUSTMENTS: " + local.ParmShowDebtAdj.Text1;

    if (IsEmpty(local.ParmShowCollAdj.Text1))
    {
      local.ParmShowCollAdj.Text1 = "N";
    }

    local.Header.Index = 11;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      10) + "SHOW COLL ADJUSTMENTS: " + local.ParmShowCollAdj.Text1;

    if (IsEmpty(local.ParmDebtsWithAmtOwed.SelectChar))
    {
      local.ParmDebtsWithAmtOwed.SelectChar = "N";
    }

    local.Header.Index = 12;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      7) + "ONLY DEBTS WITH AMT OWED: " + local
      .ParmDebtsWithAmtOwed.SelectChar;

    if (IsEmpty(local.ParmShowFutureColl.Text1))
    {
      local.ParmShowFutureColl.Text1 = "N";
    }

    local.Header.Index = 13;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";

    if (IsEmpty(local.ScreenOwedAmounts.ErrorInformationLine))
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 13) + "FUTURE COLLECTIONS: " + local.ParmShowFutureColl.Text1;
    }
    else
    {
      local.Header.Update.Header1.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 13) + "FUTURE COLLECTIONS: " + local.ParmShowFutureColl.Text1 + "                                   " +
        local.ScreenOwedAmounts.ErrorInformationLine;
    }

    local.Header.Index = 14;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "02";
    local.Header.Update.Header1.LineText =
      "TRN  OB-ID  OB-TYPE   DUE-DT     PGM/ST  ORD-TYPE  COURT-ORDER    WRKR-ID     AMT-DUE   CURR-OWED  ARRS-OWED   INT-OWED   TOT-OWED";
      

    local.Header.Index = 15;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      "    COLL/ADJ-DT   SOURCE      PROCESS-DT   TRN-AMT    APPL-TO    APPL-TO-PGM        ADJ-RSN           COLL-DIST-METHOD";
      

    local.Header.Index = 16;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      "====================================================================================================================================";
      

    for(local.Header.Index = 0; local.Header.Index < local.Header.Count; ++
      local.Header.Index)
    {
      if (!local.Header.CheckSize())
      {
        break;
      }

      try
      {
        CreateReportData2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_REPORT_DATA_AE_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_REPORT_DATA_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    local.Header.CheckIndex();

    if (local.Debt.IsEmpty)
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "No Data Retrieved for the report.";
      UseCabErrorReport3();
    }
    else
    {
      for(local.Debt.Index = 0; local.Debt.Index < local.Debt.Count; ++
        local.Debt.Index)
      {
        if (!local.Debt.CheckSize())
        {
          break;
        }

        if (Equal(local.Debt.Item.ReportData.LineText, 1, 2, "DE"))
        {
          local.ReportData.LineControl = "01";
        }
        else
        {
          local.ReportData.LineControl = "";
        }

        try
        {
          CreateReportData1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CO0000_REPORT_DATA_AE_AB";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CO0000_REPORT_DATA_PV_AB";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      local.Debt.CheckIndex();
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    if (Equal(entities.ExistingJobRun.OutputType, "ONLINE"))
    {
      try
      {
        UpdateJobRun1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

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
        UpdateJobRun4();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CO0000_JOB_RUN_NU_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_JOB_RUN_PV_AB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveDebt(FnB696ReadDebtActivity.Export.DebtGroup source,
    Local.DebtGroup target)
  {
    target.ReportData.LineText = source.Debt1.LineText;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
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

  private void UseFnB696ReadDebtActivity()
  {
    var useImport = new FnB696ReadDebtActivity.Import();
    var useExport = new FnB696ReadDebtActivity.Export();

    MoveCsePerson(local.ParmCsePerson, useImport.SearchCsePerson);
    useImport.SearchLegalAction.StandardNumber =
      local.ParmLegalAction.StandardNumber;
    useImport.SearchObligation.SystemGeneratedIdentifier =
      local.ParmObligation.SystemGeneratedIdentifier;
    useImport.SearchObligationType.SystemGeneratedIdentifier =
      local.ParmObligationType.SystemGeneratedIdentifier;
    useImport.SearchFrom.Date = local.ParmFrom.Date;
    useImport.SearchTo.Date = local.ParmTo.Date;
    useImport.SearchShowDebtAdj.Text1 = local.ParmShowDebtAdj.Text1;
    useImport.SearchShowCollAdj.Text1 = local.ParmShowCollAdj.Text1;
    useImport.ListDebtsWithAmtOwed.SelectChar =
      local.ParmDebtsWithAmtOwed.SelectChar;
    useImport.Current.Date = local.Current.Date;

    Call(FnB696ReadDebtActivity.Execute, useImport, useExport);

    useExport.Debt.CopyTo(local.Debt, MoveDebt);
  }

  private void UseFnExtGetParmsThruJclSysin()
  {
    var useImport = new FnExtGetParmsThruJclSysin.Import();
    var useExport = new FnExtGetParmsThruJclSysin.Export();

    useExport.ProgramProcessingInfo.ParameterList = local.Sysin.ParameterList;
    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(FnExtGetParmsThruJclSysin.Execute, useImport, useExport);

    local.Sysin.ParameterList = useExport.ProgramProcessingInfo.ParameterList;
    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void CreateReportData1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var type1 = "D";
    var sequenceNumber = local.Debt.Index + 1;
    var firstPageOnlyInd = "N";
    var lineControl = local.ReportData.LineControl;
    var lineText = local.Debt.Item.ReportData.LineText;
    var jobName = entities.ExistingJobRun.JobName;
    var jruSystemGenId = entities.ExistingJobRun.SystemGenId;

    entities.New1.Populated = false;
    Update("CreateReportData1",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", lineControl);
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.New1.Type1 = type1;
    entities.New1.SequenceNumber = sequenceNumber;
    entities.New1.FirstPageOnlyInd = firstPageOnlyInd;
    entities.New1.LineControl = lineControl;
    entities.New1.LineText = lineText;
    entities.New1.JobName = jobName;
    entities.New1.JruSystemGenId = jruSystemGenId;
    entities.New1.Populated = true;
  }

  private void CreateReportData2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var type1 = "H";
    var sequenceNumber = local.Header.Index + 1;
    var firstPageOnlyInd = local.Header.Item.Header1.FirstPageOnlyInd ?? "";
    var lineControl = local.Header.Item.Header1.LineControl;
    var lineText = local.Header.Item.Header1.LineText;
    var jobName = entities.ExistingJobRun.JobName;
    var jruSystemGenId = entities.ExistingJobRun.SystemGenId;

    entities.New1.Populated = false;
    Update("CreateReportData2",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableString(command, "firstPageOnlyIn", firstPageOnlyInd);
        db.SetString(command, "lineControl", lineControl);
        db.SetString(command, "lineText", lineText);
        db.SetString(command, "jobName", jobName);
        db.SetInt32(command, "jruSystemGenId", jruSystemGenId);
      });

    entities.New1.Type1 = type1;
    entities.New1.SequenceNumber = sequenceNumber;
    entities.New1.FirstPageOnlyInd = firstPageOnlyInd;
    entities.New1.LineControl = lineControl;
    entities.New1.LineText = lineText;
    entities.New1.JobName = jobName;
    entities.New1.JruSystemGenId = jruSystemGenId;
    entities.New1.Populated = true;
  }

  private bool ReadJob()
  {
    entities.ExistingJob.Populated = false;

    return Read("ReadJob",
      (db, command) =>
      {
        db.SetString(command, "name", local.Job.Name);
      },
      (db, reader) =>
      {
        entities.ExistingJob.Name = db.GetString(reader, 0);
        entities.ExistingJob.Populated = true;
      });
  }

  private bool ReadJobRun()
  {
    entities.ExistingJobRun.Populated = false;

    return Read("ReadJobRun",
      (db, command) =>
      {
        db.SetString(command, "jobName", entities.ExistingJob.Name);
        db.SetInt32(command, "systemGenId", local.JobRun.SystemGenId);
      },
      (db, reader) =>
      {
        entities.ExistingJobRun.StartTimestamp = db.GetDateTime(reader, 0);
        entities.ExistingJobRun.EndTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ExistingJobRun.Status = db.GetString(reader, 2);
        entities.ExistingJobRun.OutputType = db.GetString(reader, 3);
        entities.ExistingJobRun.ErrorMsg = db.GetNullableString(reader, 4);
        entities.ExistingJobRun.ParmInfo = db.GetNullableString(reader, 5);
        entities.ExistingJobRun.JobName = db.GetString(reader, 6);
        entities.ExistingJobRun.SystemGenId = db.GetInt32(reader, 7);
        entities.ExistingJobRun.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          local.ParmObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
      });
  }

  private void UpdateJobRun1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var endTimestamp = local.Current.Timestamp;
    var status = "COMPLETE";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun1",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "endTimestamp", endTimestamp);
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.EndTimestamp = endTimestamp;
    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var endTimestamp = Now();
    var status = "ERROR";
    var errorMsg = Substring(local.NeededToWrite.RptDetail, 1, 80);

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun2",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "endTimestamp", endTimestamp);
        db.SetString(command, "status", status);
        db.SetNullableString(command, "errorMsg", errorMsg);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.EndTimestamp = endTimestamp;
    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.ErrorMsg = errorMsg;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "PROCESSING";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun3",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
  }

  private void UpdateJobRun4()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "WAIT";

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun4",
      (db, command) =>
      {
        db.SetString(command, "status", status);
        db.SetString(command, "jobName", entities.ExistingJobRun.JobName);
        db.
          SetInt32(command, "systemGenId", entities.ExistingJobRun.SystemGenId);
          
      });

    entities.ExistingJobRun.Status = status;
    entities.ExistingJobRun.Populated = true;
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
    /// <summary>A DebtGroup group.</summary>
    [Serializable]
    public class DebtGroup
    {
      /// <summary>
      /// A value of ReportData.
      /// </summary>
      [JsonPropertyName("reportData")]
      public ReportData ReportData
      {
        get => reportData ??= new();
        set => reportData = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private ReportData reportData;
    }

    /// <summary>A HeaderGroup group.</summary>
    [Serializable]
    public class HeaderGroup
    {
      /// <summary>
      /// A value of Header1.
      /// </summary>
      [JsonPropertyName("header1")]
      public ReportData Header1
      {
        get => header1 ??= new();
        set => header1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ReportData header1;
    }

    /// <summary>
    /// A value of ReportData.
    /// </summary>
    [JsonPropertyName("reportData")]
    public ReportData ReportData
    {
      get => reportData ??= new();
      set => reportData = value;
    }

    /// <summary>
    /// Gets a value of Debt.
    /// </summary>
    [JsonIgnore]
    public Array<DebtGroup> Debt => debt ??= new(DebtGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Debt for json serialization.
    /// </summary>
    [JsonPropertyName("debt")]
    [Computed]
    public IList<DebtGroup> Debt_Json
    {
      get => debt;
      set => Debt.Assign(value);
    }

    /// <summary>
    /// A value of ParmCsePerson.
    /// </summary>
    [JsonPropertyName("parmCsePerson")]
    public CsePerson ParmCsePerson
    {
      get => parmCsePerson ??= new();
      set => parmCsePerson = value;
    }

    /// <summary>
    /// A value of ParmLegalAction.
    /// </summary>
    [JsonPropertyName("parmLegalAction")]
    public LegalAction ParmLegalAction
    {
      get => parmLegalAction ??= new();
      set => parmLegalAction = value;
    }

    /// <summary>
    /// A value of ParmObligation.
    /// </summary>
    [JsonPropertyName("parmObligation")]
    public Obligation ParmObligation
    {
      get => parmObligation ??= new();
      set => parmObligation = value;
    }

    /// <summary>
    /// A value of ParmObligationType.
    /// </summary>
    [JsonPropertyName("parmObligationType")]
    public ObligationType ParmObligationType
    {
      get => parmObligationType ??= new();
      set => parmObligationType = value;
    }

    /// <summary>
    /// A value of ParmFrom.
    /// </summary>
    [JsonPropertyName("parmFrom")]
    public DateWorkArea ParmFrom
    {
      get => parmFrom ??= new();
      set => parmFrom = value;
    }

    /// <summary>
    /// A value of ParmTo.
    /// </summary>
    [JsonPropertyName("parmTo")]
    public DateWorkArea ParmTo
    {
      get => parmTo ??= new();
      set => parmTo = value;
    }

    /// <summary>
    /// A value of ParmShowDebtAdj.
    /// </summary>
    [JsonPropertyName("parmShowDebtAdj")]
    public TextWorkArea ParmShowDebtAdj
    {
      get => parmShowDebtAdj ??= new();
      set => parmShowDebtAdj = value;
    }

    /// <summary>
    /// A value of ParmShowCollAdj.
    /// </summary>
    [JsonPropertyName("parmShowCollAdj")]
    public TextWorkArea ParmShowCollAdj
    {
      get => parmShowCollAdj ??= new();
      set => parmShowCollAdj = value;
    }

    /// <summary>
    /// A value of ParmDebtsWithAmtOwed.
    /// </summary>
    [JsonPropertyName("parmDebtsWithAmtOwed")]
    public Common ParmDebtsWithAmtOwed
    {
      get => parmDebtsWithAmtOwed ??= new();
      set => parmDebtsWithAmtOwed = value;
    }

    /// <summary>
    /// A value of ParmShowFutureColl.
    /// </summary>
    [JsonPropertyName("parmShowFutureColl")]
    public TextWorkArea ParmShowFutureColl
    {
      get => parmShowFutureColl ??= new();
      set => parmShowFutureColl = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of Sysin.
    /// </summary>
    [JsonPropertyName("sysin")]
    public ProgramProcessingInfo Sysin
    {
      get => sysin ??= new();
      set => sysin = value;
    }

    /// <summary>
    /// A value of Job.
    /// </summary>
    [JsonPropertyName("job")]
    public Job Job
    {
      get => job ??= new();
      set => job = value;
    }

    /// <summary>
    /// A value of JobRun.
    /// </summary>
    [JsonPropertyName("jobRun")]
    public JobRun JobRun
    {
      get => jobRun ??= new();
      set => jobRun = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// Gets a value of Header.
    /// </summary>
    [JsonIgnore]
    public Array<HeaderGroup> Header => header ??= new(HeaderGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Header for json serialization.
    /// </summary>
    [JsonPropertyName("header")]
    [Computed]
    public IList<HeaderGroup> Header_Json
    {
      get => header;
      set => Header.Assign(value);
    }

    /// <summary>
    /// A value of NullReportData.
    /// </summary>
    [JsonPropertyName("nullReportData")]
    public ReportData NullReportData
    {
      get => nullReportData ??= new();
      set => nullReportData = value;
    }

    /// <summary>
    /// A value of ParmAp.
    /// </summary>
    [JsonPropertyName("parmAp")]
    public TextWorkArea ParmAp
    {
      get => parmAp ??= new();
      set => parmAp = value;
    }

    /// <summary>
    /// A value of TextMm.
    /// </summary>
    [JsonPropertyName("textMm")]
    public TextWorkArea TextMm
    {
      get => textMm ??= new();
      set => textMm = value;
    }

    /// <summary>
    /// A value of TextDd.
    /// </summary>
    [JsonPropertyName("textDd")]
    public TextWorkArea TextDd
    {
      get => textDd ??= new();
      set => textDd = value;
    }

    /// <summary>
    /// A value of TextYyyy.
    /// </summary>
    [JsonPropertyName("textYyyy")]
    public TextWorkArea TextYyyy
    {
      get => textYyyy ??= new();
      set => textYyyy = value;
    }

    /// <summary>
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public WorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    private ReportData reportData;
    private Array<DebtGroup> debt;
    private CsePerson parmCsePerson;
    private LegalAction parmLegalAction;
    private Obligation parmObligation;
    private ObligationType parmObligationType;
    private DateWorkArea parmFrom;
    private DateWorkArea parmTo;
    private TextWorkArea parmShowDebtAdj;
    private TextWorkArea parmShowCollAdj;
    private Common parmDebtsWithAmtOwed;
    private TextWorkArea parmShowFutureColl;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private ProgramProcessingInfo sysin;
    private Job job;
    private JobRun jobRun;
    private DateWorkArea current;
    private DateWorkArea nullDateWorkArea;
    private External external;
    private Array<HeaderGroup> header;
    private ReportData nullReportData;
    private TextWorkArea parmAp;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private WorkArea textDate;
    private DateWorkArea max;
    private ScreenOwedAmounts screenOwedAmounts;
    private EabReportSend neededToOpen;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingJob.
    /// </summary>
    [JsonPropertyName("existingJob")]
    public Job ExistingJob
    {
      get => existingJob ??= new();
      set => existingJob = value;
    }

    /// <summary>
    /// A value of ExistingJobRun.
    /// </summary>
    [JsonPropertyName("existingJobRun")]
    public JobRun ExistingJobRun
    {
      get => existingJobRun ??= new();
      set => existingJobRun = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ReportData New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private ObligationType obligationType;
    private Job existingJob;
    private JobRun existingJobRun;
    private ReportData new1;
  }
#endregion
}
