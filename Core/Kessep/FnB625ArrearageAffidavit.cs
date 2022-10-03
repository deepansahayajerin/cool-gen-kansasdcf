// Program: FN_B625_ARREARAGE_AFFIDAVIT, ID: 373022547, model: 746.
// Short name: SWEF625B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B625_ARREARAGE_AFFIDAVIT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB625ArrearageAffidavit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B625_ARREARAGE_AFFIDAVIT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB625ArrearageAffidavit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB625ArrearageAffidavit.
  /// </summary>
  public FnB625ArrearageAffidavit(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    // resp: Finance
    // This procedure produces an arrearage affidavit.  It uses the online
    // print function to produce the report.  The program is initiated from
    // the DEBT screen.
    // The input parameters are:
    //     - AP number             : positions 1 - 10
    //     - Court Order Standard #: positions 12 - 23
    //     - From Date             : positions 25 - 34
    //     - To Date               : positions 36 - 45
    //     - Comments indicator    : position  47 - 47
    // AP number and Court order number and date range are mandatory input 
    // parms.
    // They are all passed in by the DEBT screen.
    // The comments indicator is to say whether or not the report is to include
    // debt adjustment reason text.
    // *****************************************************************
    // *****************************************************************
    // MAINTENANCE LOG
    // Date    Developer    Request #   Description
    // --------------------------------------------------------------------
    // 05/30/00  Maureen Brown              Initial Development
    // --------------------------------------------------------------------
    // : Feb, 2002, M. Brown, PR# 139291 - added code to show state and division
    //   on the report.
    // : PR# 149349, M. Brown, July 1, 2002
    //   Beginning balance should be added into the total obligation field.
    // : PR# 158654, M. Brown, July, 2002
    //   View overflow problem.  Changed local report data view from 500 to 5000
    // entries.  If the comments option is chosen, this view can end up being
    // bigger than the local group view, since it has within it a group view of
    // comments, and each of these becomes 1 row in the local report data view.
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****************************************************************
    // Housekeeping
    // *****************************************************************
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // : Set up current date in a text field.
    local.TextMm.Text2 = NumberToString(DateToInt(local.Current.Date), 12, 2);
    local.TextDd.Text2 = NumberToString(DateToInt(local.Current.Date), 14, 2);
    local.TextYyyy.Text4 = NumberToString(DateToInt(local.Current.Date), 8, 4);
    local.TextCurrentDate.Text10 = local.TextMm.Text2 + "/" + local
      .TextDd.Text2 + "/" + local.TextYyyy.Text4;
    local.Max.Date = new DateTime(2099, 12, 31);
    local.NeededToOpen.ProgramName = "SWEFB625";
    local.Group.Index = -1;
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport1();
    local.EabFileHandling.Action = "WRITE";

    // *****************************************************************
    // Get the SYSIN Parm Values
    // *****************************************************************
    UseFnExtGetParmsThruJclSysin();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_SYSIN_PARM_ERROR_A";

      return;
    }

    if (IsEmpty(local.Sysin.ParameterList))
    {
      ExitState = "FN0000_BLANK_SYSIN_PARM_LIST_AB";

      return;
    }

    local.Job.Name = Substring(local.Sysin.ParameterList, 1, 8);

    if (!ReadJob())
    {
      local.NeededToWrite.RptDetail = local.Sysin.ParameterList ?? Spaces(132);
      UseCabErrorReport2();
      ExitState = "CO0000_JOB_NF_AB";

      return;
    }

    local.JobRun.SystemGenId =
      (int)StringToNumber(Substring(local.Sysin.ParameterList, 10, 9));

    if (!ReadJobRun())
    {
      ExitState = "CO0000_JOB_RUN_NF_AB";

      return;
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    // **** SET status TO "PROCESSING"
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
    // : Mandatory Parm Values.
    local.Obligor.Number = Substring(entities.ExistingJobRun.ParmInfo, 1, 10);

    if (IsEmpty(local.Obligor.Number))
    {
      local.NeededToWrite.RptDetail =
        "No AP number found in job run parameter information";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.LegalAction.StandardNumber =
      Substring(entities.ExistingJobRun.ParmInfo, 12, 12);

    if (IsEmpty(local.LegalAction.StandardNumber))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "No legal action standard number in job_run parameter info.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
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

      UseExtToDoACommit();

      return;
    }

    local.ParmFrom.Date =
      StringToDate(Substring(entities.ExistingJobRun.ParmInfo, 41, 10));

    if (Equal(local.ParmFrom.Date, local.NullDateWorkArea.Date))
    {
      local.ParmFrom.Date = AddYears(local.Current.Date, -15);
    }

    local.ParmTo.Date =
      StringToDate(Substring(entities.ExistingJobRun.ParmInfo, 52, 10));

    if (Equal(local.ParmTo.Date, local.NullDateWorkArea.Date))
    {
      local.ParmTo.Date = local.Current.Date;
    }

    // : The comments flag is in the jcl sysin, not the job run table.
    local.Comments1.Flag = Substring(local.Sysin.ParameterList, 20, 1);

    // *****************************************************************
    // Mainline Process
    // *****************************************************************
    UseFnB625RetrieveReportData();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail =
        TrimEnd(local.ExitStateWorkArea.Message) + "  Person #: " + local
        .Obligor.Number;
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport2();

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

      UseExtToDoACommit();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // Format Report and Add to the Report Repository
    // *****************************************************************
    // : Format Header Lines and add to REPORT_DATA.
    // : Feb, 2002, M. Brown, PR# 139291 - added code to show state and division
    //   on the report.
    local.Header.Index = 0;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "";
    local.Header.Update.Header1.LineText =
      Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength, 1,
      25) + "In the District Court of " + TrimEnd
      (local.Fips.CountyDescription) + " County";

    for(local.Caption.Index = 0; local.Caption.Index < local.Caption.Count; ++
      local.Caption.Index)
    {
      ++local.Header.Index;
      local.Header.CheckSize();

      local.Header.Update.Header1.FirstPageOnlyInd = "Y";
      local.Header.Update.Header1.LineControl = "";
      local.Header.Update.Header1.LineText =
        local.Caption.Item.CourtCaption.Line ?? Spaces(132);
    }

    ++local.Header.Index;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "Y";
    local.Header.Update.Header1.LineControl = "01";
    local.Header.Update.Header1.LineText = "AP: " + local.Obligor.FormattedName;

    for(local.Cases.Index = 0; local.Cases.Index < local.Cases.Count; ++
      local.Cases.Index)
    {
      if (!local.Cases.CheckSize())
      {
        break;
      }

      ++local.Header.Index;
      local.Header.CheckSize();

      local.Header.Update.Header1.FirstPageOnlyInd = "Y";

      if (local.Cases.Index == 0)
      {
        local.Header.Update.Header1.LineText = "KAECSES #: " + local
          .Cases.Item.Case1.Number;
        local.Header.Update.Header1.LineControl = "01";
      }
      else
      {
        local.Header.Update.Header1.LineText = "           " + local
          .Cases.Item.Case1.Number;
        local.Header.Update.Header1.LineControl = "";
      }
    }

    local.Cases.CheckIndex();

    ++local.Header.Index;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "01";
    local.Header.Update.Header1.LineText = "Court Order #: " + (
      local.LegalAction.StandardNumber ?? "");

    ++local.Header.Index;
    local.Header.CheckSize();

    local.Header.Update.Header1.FirstPageOnlyInd = "N";
    local.Header.Update.Header1.LineControl = "01";

    if (AsChar(local.Comments1.Flag) == 'Y')
    {
      local.Header.Update.Header1.LineText = "Mnth/Yr" + "      Oblig." + "      Coll" +
        "  Interest" + "   Net Adj" + "   Balance " + "Comment" + "";
    }
    else
    {
      local.Header.Update.Header1.LineText = "Mnth/Yr" + "      Oblig." + "      Coll" +
        "  Interest" + "   Net Adj" + "   Balance " + "" + "";
    }

    local.Header.Index = 0;

    for(var limit = local.Header.Count; local.Header.Index < limit; ++
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

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CO0000_REPORT_DATA_PV_AB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();

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

        UseExtToDoACommit();

        return;
      }
    }

    local.Header.CheckIndex();

    if (local.Group.IsEmpty)
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "No Data Retrieved for the report.";
      UseCabErrorReport2();
    }
    else
    {
      // : Now create the detail lines on Report Data table.
      local.CommaInd.Flag = "Y";
      local.RptLines.Index = -1;

      // : Beginning Balance line.
      local.CommaInd.Flag = "Y";
      UseFnCabCurrencyToText4();

      if (!IsEmpty(local.ReturnCode.Text10))
      {
        local.Balance.Text10 = local.ReturnCode.Text10;
      }

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "01";
      local.RptLines.Update.ReportData.LineText = "Beginning Balance" + Substring
        (local.NullReportData.LineText, ReportData.LineText_MaxLength, 1, 33) +
        local.Balance.Text10;

      for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        local.Group.Item.Comments.Index = 0;
        local.Group.Item.Comments.CheckSize();

        if (local.Group.Item.NetCollection.TotalCurrency == 0 && local
          .Group.Item.NetInterest.TotalCurrency == 0 && local
          .Group.Item.NewDebts.TotalCurrency == 0 && local
          .Group.Item.DebtAdj.TotalCurrency == 0)
        {
          continue;
        }

        UseFnCabCurrencyToText5();
        UseFnCabCurrencyToText9();
        UseFnCabCurrencyToText7();
        UseFnCabCurrencyToText8();
        UseFnCabCurrencyToText6();

        ++local.RptLines.Index;
        local.RptLines.CheckSize();

        local.RptLines.Update.ReportData.LineText =
          local.Group.Item.YymmText.Text8 + "  " + local.NewDebts.Text10 + ""
          + local.NetColl.Text10 + "" + local.InterestAmt.Text10 + "" + local
          .DebtAdj.Text10 + "" + local.Balance.Text10 + " " + local
          .Group.Item.Comments.Item.Comments1.Text24;
        local.RptLines.Update.ReportData.LineControl = "";

        if (AsChar(local.Comments1.Flag) == 'Y')
        {
          if (local.Group.Item.Comments.Index + 1 < local
            .Group.Item.Comments.Count)
          {
            for(++local.Group.Item.Comments.Index; local
              .Group.Item.Comments.Index < local.Group.Item.Comments.Count; ++
              local.Group.Item.Comments.Index)
            {
              if (!local.Group.Item.Comments.CheckSize())
              {
                break;
              }

              if (IsEmpty(local.Group.Item.Comments.Item.Comments1.Text24))
              {
                break;
              }

              ++local.RptLines.Index;
              local.RptLines.CheckSize();

              local.RptLines.Update.ReportData.LineText =
                Substring(local.NullReportData.LineText,
                ReportData.LineText_MaxLength, 1, 60) + local
                .Group.Item.Comments.Item.Comments1.Text24;
            }

            local.Group.Item.Comments.CheckIndex();
          }
        }
      }

      local.Group.CheckIndex();

      // : PR# 149349, M. Brown, July 1, 2002
      //   Beginning balance should be added into the total obligation field.
      local.TotalDebts.TotalCurrency += local.BeginBalance.TotalCurrency;
      UseFnCabCurrencyToText2();

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "01";
      local.RptLines.Update.ReportData.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 38) + "Total Obligation:  " + local.NewDebts.Text10;
      UseFnCabCurrencyToText1();

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "";
      local.RptLines.Update.ReportData.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 38) + "Total Amount Paid: " + local.NetColl.Text10;
      UseFnCabCurrencyToText3();

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "";
      local.RptLines.Update.ReportData.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 38) + "Total Adjustments: " + local.DebtAdj.Text10;

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "";
      local.RptLines.Update.ReportData.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 38) + "Total Amount Owed: " + local.Balance.Text10;

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "01";
      local.RptLines.Update.ReportData.LineText =
        "I do hereby swear and affirm that to the best of my knowledge the above";
        

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "";
      local.RptLines.Update.ReportData.LineText =
        "record is an accurate account of arrearage owed and due from " + TrimEnd
        ("");

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "";
      local.RptLines.Update.ReportData.LineText =
        TrimEnd(local.Obligor.FormattedName) + TrimEnd(" ") + " and that effective " +
        local.TextCurrentDate.Text10 + "" + " " + " ";

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "";
      local.RptLines.Update.ReportData.LineText = "the arrearage owed is " + TrimEnd
        (local.Balance.Text10) + ".  Plus any statutory interest";

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "";
      local.RptLines.Update.ReportData.LineText =
        "which may be due and owing pursuant to the Kansas Statutes Annotated.";
        

      ++local.RptLines.Index;
      local.RptLines.CheckSize();

      local.RptLines.Update.ReportData.LineControl = "04";
      local.RptLines.Update.ReportData.LineText =
        Substring(local.NullReportData.LineText, ReportData.LineText_MaxLength,
        1, 65) + "Affiant";
      local.RptLines.Index = 0;

      for(var limit = local.RptLines.Count; local.RptLines.Index < limit; ++
        local.RptLines.Index)
      {
        if (!local.RptLines.CheckSize())
        {
          break;
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

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CO0000_REPORT_DATA_PV_AB";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();

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

          UseExtToDoACommit();

          return;
        }
      }

      local.RptLines.CheckIndex();
    }

    // *****************************************************************
    // Update the Status of the Report in JOB_RUN.
    // *****************************************************************
    if (Equal(entities.ExistingJobRun.OutputType, "ONLINE"))
    {
      local.JobRun.Status = "COMPLETE";
    }
    else
    {
      local.JobRun.Status = "WAIT";
    }

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

  private static void MoveCaption(FnB625RetrieveReportData.Export.
    CaptionGroup source, Local.CaptionGroup target)
  {
    MoveCourtCaption(source.CourtCaption, target.CourtCaption);
  }

  private static void MoveCases(FnB625RetrieveReportData.Export.
    CasesGroup source, Local.CasesGroup target)
  {
    target.Case1.Number = source.Case1.Number;
  }

  private static void MoveComments(FnB625RetrieveReportData.Export.
    CommentsGroup source, Local.CommentsGroup target)
  {
    target.Comments1.Text24 = source.Comment.Text24;
  }

  private static void MoveCourtCaption(CourtCaption source, CourtCaption target)
  {
    target.Number = source.Number;
    target.Line = source.Line;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveGroup(FnB625RetrieveReportData.Export.
    GroupGroup source, Local.GroupGroup target)
  {
    target.YymmText.Text8 = source.Yymm.Text8;
    target.NetInterest.TotalCurrency = source.NetInterest.TotalCurrency;
    target.DebtAdj.TotalCurrency = source.DebtAdj.TotalCurrency;
    target.NetCollection.TotalCurrency = source.NetCollection.TotalCurrency;
    target.NewDebts.TotalCurrency = source.NewDebts.TotalCurrency;
    target.EomBal.TotalCurrency = source.EomBal.TotalCurrency;
    source.Comments.CopyTo(target.Comments, MoveComments);
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
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

  private void UseFnB625RetrieveReportData()
  {
    var useImport = new FnB625RetrieveReportData.Import();
    var useExport = new FnB625RetrieveReportData.Export();

    useImport.CommentsInd.Flag = local.Comments1.Flag;
    useImport.SearchAp.Number = local.Obligor.Number;
    useImport.Search.StandardNumber = local.LegalAction.StandardNumber;
    useImport.Current.Date = local.Current.Date;
    useImport.SearchFrom.Date = local.ParmFrom.Date;
    useImport.SearchTo.Date = local.ParmTo.Date;

    Call(FnB625RetrieveReportData.Execute, useImport, useExport);

    local.ScreenOwedAmounts.TotalAmountOwed =
      useExport.ScreenOwedAmounts.TotalAmountOwed;
    local.TotalCollections.TotalCurrency =
      useExport.TotalCollections.TotalCurrency;
    local.TotalDebts.TotalCurrency = useExport.TotalDebts.TotalCurrency;
    local.TotalAdjustments.TotalCurrency =
      useExport.TotalAdjustments.TotalCurrency;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.Obligor);
    local.BeginBalance.TotalCurrency = useExport.BeginBalance.TotalCurrency;
    useExport.Cases.CopyTo(local.Cases, MoveCases);
    useExport.Group.CopyTo(local.Group, MoveGroup);
    useExport.Caption.CopyTo(local.Caption, MoveCaption);
    local.Fips.CountyDescription = useExport.Fips.CountyDescription;
    local.Tribunal.JudicialDivision = useExport.Tribunal.JudicialDivision;
  }

  private void UseFnCabCurrencyToText1()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.TotalCollections.TotalCurrency;
    useImport.CommasRequired.Flag = local.CommaInd.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.NetColl.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText2()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.TotalDebts.TotalCurrency;
    useImport.CommasRequired.Flag = local.CommaInd.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.NewDebts.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText3()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.TotalAdjustments.TotalCurrency;
    useImport.CommasRequired.Flag = local.CommaInd.Flag;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.DebtAdj.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText4()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommaInd.Flag;
    useImport.Common.TotalCurrency = local.BeginBalance.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.Balance.Text10 = useExport.TextWorkArea.Text10;
    local.ReturnCode.Text10 = useExport.ReturnCode.Text10;
  }

  private void UseFnCabCurrencyToText5()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommaInd.Flag;
    useImport.Common.TotalCurrency = local.Group.Item.EomBal.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.Balance.Text10 = useExport.TextWorkArea.Text10;
    local.ReturnCode.Text10 = useExport.ReturnCode.Text10;
  }

  private void UseFnCabCurrencyToText6()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommaInd.Flag;
    useImport.Common.TotalCurrency = local.Group.Item.NewDebts.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.NewDebts.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText7()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommaInd.Flag;
    useImport.Common.TotalCurrency = local.Group.Item.DebtAdj.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.DebtAdj.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText8()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommaInd.Flag;
    useImport.Common.TotalCurrency =
      local.Group.Item.NetCollection.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.NetColl.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText9()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.CommasRequired.Flag = local.CommaInd.Flag;
    useImport.Common.TotalCurrency = local.Group.Item.NetInterest.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.InterestAmt.Text10 = useExport.TextWorkArea.Text10;
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
    var sequenceNumber = local.RptLines.Index + 1;
    var firstPageOnlyInd = "N";
    var lineControl = local.RptLines.Item.ReportData.LineControl;
    var lineText = local.RptLines.Item.ReportData.LineText;
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

  private void UpdateJobRun1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var endTimestamp = local.Current.Timestamp;
    var status = local.JobRun.Status;

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

    var endTimestamp = local.Current.Timestamp;
    var status = "ERROR";
    var errorMsg = local.ExitStateWorkArea.Message;

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

    var endTimestamp = local.Current.Timestamp;
    var status = "ERROR";
    var errorMsg = Substring(local.NeededToWrite.RptDetail, 1, 80);

    entities.ExistingJobRun.Populated = false;
    Update("UpdateJobRun3",
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

  private void UpdateJobRun4()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingJobRun.Populated);

    var status = "PROCESSING";

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
    /// <summary>A CasesGroup group.</summary>
    [Serializable]
    public class CasesGroup
    {
      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 case1;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of YymmText.
      /// </summary>
      [JsonPropertyName("yymmText")]
      public WorkArea YymmText
      {
        get => yymmText ??= new();
        set => yymmText = value;
      }

      /// <summary>
      /// A value of NetInterest.
      /// </summary>
      [JsonPropertyName("netInterest")]
      public Common NetInterest
      {
        get => netInterest ??= new();
        set => netInterest = value;
      }

      /// <summary>
      /// A value of DebtAdj.
      /// </summary>
      [JsonPropertyName("debtAdj")]
      public Common DebtAdj
      {
        get => debtAdj ??= new();
        set => debtAdj = value;
      }

      /// <summary>
      /// A value of NetCollection.
      /// </summary>
      [JsonPropertyName("netCollection")]
      public Common NetCollection
      {
        get => netCollection ??= new();
        set => netCollection = value;
      }

      /// <summary>
      /// A value of NewDebts.
      /// </summary>
      [JsonPropertyName("newDebts")]
      public Common NewDebts
      {
        get => newDebts ??= new();
        set => newDebts = value;
      }

      /// <summary>
      /// A value of EomBal.
      /// </summary>
      [JsonPropertyName("eomBal")]
      public Common EomBal
      {
        get => eomBal ??= new();
        set => eomBal = value;
      }

      /// <summary>
      /// Gets a value of Comments.
      /// </summary>
      [JsonIgnore]
      public Array<CommentsGroup> Comments => comments ??= new(
        CommentsGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of Comments for json serialization.
      /// </summary>
      [JsonPropertyName("comments")]
      [Computed]
      public IList<CommentsGroup> Comments_Json
      {
        get => comments;
        set => Comments.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private WorkArea yymmText;
      private Common netInterest;
      private Common debtAdj;
      private Common netCollection;
      private Common newDebts;
      private Common eomBal;
      private Array<CommentsGroup> comments;
    }

    /// <summary>A CommentsGroup group.</summary>
    [Serializable]
    public class CommentsGroup
    {
      /// <summary>
      /// A value of Comments1.
      /// </summary>
      [JsonPropertyName("comments1")]
      public WorkArea Comments1
      {
        get => comments1 ??= new();
        set => comments1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private WorkArea comments1;
    }

    /// <summary>A CaptionGroup group.</summary>
    [Serializable]
    public class CaptionGroup
    {
      /// <summary>
      /// A value of CourtCaption.
      /// </summary>
      [JsonPropertyName("courtCaption")]
      public CourtCaption CourtCaption
      {
        get => courtCaption ??= new();
        set => courtCaption = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CourtCaption courtCaption;
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

    /// <summary>A RptLinesGroup group.</summary>
    [Serializable]
    public class RptLinesGroup
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
      public const int Capacity = 5000;

      private ReportData reportData;
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
    /// A value of Comments1.
    /// </summary>
    [JsonPropertyName("comments1")]
    public Common Comments1
    {
      get => comments1 ??= new();
      set => comments1 = value;
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
    /// A value of TotalCollections.
    /// </summary>
    [JsonPropertyName("totalCollections")]
    public Common TotalCollections
    {
      get => totalCollections ??= new();
      set => totalCollections = value;
    }

    /// <summary>
    /// A value of TotalDebts.
    /// </summary>
    [JsonPropertyName("totalDebts")]
    public Common TotalDebts
    {
      get => totalDebts ??= new();
      set => totalDebts = value;
    }

    /// <summary>
    /// A value of TotalAdjustments.
    /// </summary>
    [JsonPropertyName("totalAdjustments")]
    public Common TotalAdjustments
    {
      get => totalAdjustments ??= new();
      set => totalAdjustments = value;
    }

    /// <summary>
    /// A value of Balance.
    /// </summary>
    [JsonPropertyName("balance")]
    public TextWorkArea Balance
    {
      get => balance ??= new();
      set => balance = value;
    }

    /// <summary>
    /// A value of NewDebts.
    /// </summary>
    [JsonPropertyName("newDebts")]
    public TextWorkArea NewDebts
    {
      get => newDebts ??= new();
      set => newDebts = value;
    }

    /// <summary>
    /// A value of DebtAdj.
    /// </summary>
    [JsonPropertyName("debtAdj")]
    public TextWorkArea DebtAdj
    {
      get => debtAdj ??= new();
      set => debtAdj = value;
    }

    /// <summary>
    /// A value of NetColl.
    /// </summary>
    [JsonPropertyName("netColl")]
    public TextWorkArea NetColl
    {
      get => netColl ??= new();
      set => netColl = value;
    }

    /// <summary>
    /// A value of InterestAmt.
    /// </summary>
    [JsonPropertyName("interestAmt")]
    public TextWorkArea InterestAmt
    {
      get => interestAmt ??= new();
      set => interestAmt = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public TextWorkArea Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of CommaInd.
    /// </summary>
    [JsonPropertyName("commaInd")]
    public Common CommaInd
    {
      get => commaInd ??= new();
      set => commaInd = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of BeginBalance.
    /// </summary>
    [JsonPropertyName("beginBalance")]
    public Common BeginBalance
    {
      get => beginBalance ??= new();
      set => beginBalance = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of TextCurrentDate.
    /// </summary>
    [JsonPropertyName("textCurrentDate")]
    public WorkArea TextCurrentDate
    {
      get => textCurrentDate ??= new();
      set => textCurrentDate = value;
    }

    /// <summary>
    /// Gets a value of Cases.
    /// </summary>
    [JsonIgnore]
    public Array<CasesGroup> Cases => cases ??= new(CasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Cases for json serialization.
    /// </summary>
    [JsonPropertyName("cases")]
    [Computed]
    public IList<CasesGroup> Cases_Json
    {
      get => cases;
      set => Cases.Assign(value);
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public TextWorkArea ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    /// <summary>
    /// A value of SystemGenerated.
    /// </summary>
    [JsonPropertyName("systemGenerated")]
    public SystemGenerated SystemGenerated
    {
      get => systemGenerated ??= new();
      set => systemGenerated = value;
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
    /// Gets a value of Caption.
    /// </summary>
    [JsonIgnore]
    public Array<CaptionGroup> Caption =>
      caption ??= new(CaptionGroup.Capacity);

    /// <summary>
    /// Gets a value of Caption for json serialization.
    /// </summary>
    [JsonPropertyName("caption")]
    [Computed]
    public IList<CaptionGroup> Caption_Json
    {
      get => caption;
      set => Caption.Assign(value);
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
    /// Gets a value of RptLines.
    /// </summary>
    [JsonIgnore]
    public Array<RptLinesGroup> RptLines => rptLines ??= new(
      RptLinesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of RptLines for json serialization.
    /// </summary>
    [JsonPropertyName("rptLines")]
    [Computed]
    public IList<RptLinesGroup> RptLines_Json
    {
      get => rptLines;
      set => RptLines.Assign(value);
    }

    private Fips fips;
    private Tribunal tribunal;
    private Common comments1;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common totalCollections;
    private Common totalDebts;
    private Common totalAdjustments;
    private TextWorkArea balance;
    private TextWorkArea newDebts;
    private TextWorkArea debtAdj;
    private TextWorkArea netColl;
    private TextWorkArea interestAmt;
    private TextWorkArea obligation;
    private Common commaInd;
    private CsePersonsWorkSet obligor;
    private Common beginBalance;
    private LegalAction legalAction;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private ProgramProcessingInfo sysin;
    private Job job;
    private JobRun jobRun;
    private External external;
    private ReportData nullReportData;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea max;
    private DateWorkArea current;
    private DateWorkArea parmFrom;
    private DateWorkArea parmTo;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private WorkArea textCurrentDate;
    private Array<CasesGroup> cases;
    private TextWorkArea returnCode;
    private SystemGenerated systemGenerated;
    private Array<GroupGroup> group;
    private Array<CaptionGroup> caption;
    private Array<HeaderGroup> header;
    private Array<RptLinesGroup> rptLines;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private Job existingJob;
    private JobRun existingJobRun;
    private ReportData new1;
  }
#endregion
}
