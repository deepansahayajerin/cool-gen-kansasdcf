// Program: LE_B597_IWO_DELINQUENCY_REPORT, ID: 373500090, model: 746.
// Short name: SWEL597B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B597_IWO_DELINQUENCY_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB597IwoDelinquencyReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B597_IWO_DELINQUENCY_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB597IwoDelinquencyReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB597IwoDelinquencyReport.
  /// </summary>
  public LeB597IwoDelinquencyReport(IContext context, Import import,
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
    // *********************************************************************
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 06/17/01  GVandy            WR10358    Initial Development
    // 11/27/13  DDupree           CQ41960    Change the period when the IWO 
    // that were added
    // to be evulated by changing the hard coded value with a parameter pass 
    // into the program.
    // *********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.DateMonth.RptDetail = "JanFebMarAprMayJunJulAugSepOctNovDec";
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
    // *Call External to Open the input File.         *
    // ************************************************
    local.EabFileHandling.Action = "OPEN";
    UseLeEabReadIwo45DayInfo2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening input file for 'le_iwo_45_day_delinquency_rpt'. Status = " +
        local.EabFileHandling.Status;
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR_AB";

      return;
    }

    // ************************************************
    // *Call External to Open the output File.        *
    // ************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    local.EabReportSend.BlankLineAfterColHead = "Y";
    UseCabBusinessReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening output file for 'cab_business_report_01'";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.StartCommon.Count = 1;
    local.Current.Count = 1;
    local.CurrentPosition.Count = 1;
    local.FieldNumber.Count = 0;

    do
    {
      local.Postion.Text1 =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.CurrentPosition.Count, 1);

      if (AsChar(local.Postion.Text1) == ',')
      {
        ++local.FieldNumber.Count;
        local.WorkArea.Text15 = "";

        switch(local.FieldNumber.Count)
        {
          case 1:
            if (local.Current.Count == 1)
            {
              local.NumberOfWeeks.Count = 3;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.NumberOfWeeks.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          case 2:
            if (local.Current.Count == 1)
            {
              local.RptPeriodNumOfWeeks.Count = 1;
            }
            else
            {
              local.WorkArea.Text15 =
                Substring(local.ProgramProcessingInfo.ParameterList,
                local.StartCommon.Count, local.Current.Count - 1);
              local.RptPeriodNumOfWeeks.Count =
                (int)StringToNumber(local.WorkArea.Text15);
            }

            local.StartCommon.Count = local.CurrentPosition.Count + 1;
            local.Current.Count = 0;

            break;
          default:
            break;
        }
      }
      else if (IsEmpty(local.Postion.Text1))
      {
        break;
      }

      ++local.CurrentPosition.Count;
      ++local.Current.Count;
    }
    while(!Equal(global.Command, "COMMAND"));

    if (local.RptPeriodNumOfWeeks.Count <= 0)
    {
      local.RptPeriodNumOfWeeks.Count = 1;
    }

    if (local.NumberOfWeeks.Count <= 0)
    {
      local.NumberOfWeeks.Count = 3;
    }

    local.NumberOfDaysInPeriod.Count =
      (int)((long)local.RptPeriodNumOfWeeks.Count * 7 - 1);
    local.NumberOfDays.Count = (int)((long)local.NumberOfWeeks.Count * 7 - 2);

    // -- Write new header info.
    local.EabFileHandling.Action = "WRITE";
    local.End.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate, -local.NumberOfDays.Count);
      
    local.StartDateWorkArea.Date =
      AddDays(local.End.Date, -local.NumberOfDaysInPeriod.Count);
    local.Header.RptDetail = "                  Sent Week Of";
    local.Header.RptDetail = TrimEnd(local.Header.RptDetail) + " " + Substring
      (local.DateMonth.RptDetail, EabReportSend.RptDetail_MaxLength,
      Month(local.StartDateWorkArea.Date) * 3 - 2, 3);
    local.Header.RptDetail = TrimEnd(local.Header.RptDetail) + " " + NumberToString
      (Day(local.StartDateWorkArea.Date), 14, 2);
    local.Header.RptDetail = TrimEnd(local.Header.RptDetail) + ", " + NumberToString
      (Year(local.StartDateWorkArea.Date), 12, 4);
    local.Header.RptDetail = TrimEnd(local.Header.RptDetail) + " to ";
    local.Header.RptDetail = TrimEnd(local.Header.RptDetail) + " " + Substring
      (local.DateMonth.RptDetail, EabReportSend.RptDetail_MaxLength,
      Month(local.End.Date) * 3 - 2, 3);
    local.Header.RptDetail = TrimEnd(local.Header.RptDetail) + " " + NumberToString
      (Day(local.End.Date), 14, 2);
    local.Header.RptDetail = TrimEnd(local.Header.RptDetail) + ", " + NumberToString
      (Year(local.End.Date), 12, 4);
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing output file for 'cab_business_report_01'";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport4();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing output file for 'cab_business_report_01'";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    local.FirstRecord.Flag = "Y";

    do
    {
      local.EabFileHandling.Action = "READ";
      UseLeEabReadIwo45DayInfo1();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          ++local.RecordsRead.Count;

          break;
        case "EF":
          continue;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading input file for 'le_iwo_45_day_delinquency_rpt'.";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
      }

      if (!Equal(local.ServiceProvider2.FirstName,
        local.PreviousServiceProvider.FirstName) || !
        Equal(local.ServiceProvider2.LastName,
        local.PreviousServiceProvider.LastName) || AsChar
        (local.ServiceProvider2.MiddleInitial) != AsChar
        (local.PreviousServiceProvider.MiddleInitial) || !
        Equal(local.Office.Name, local.PreviousOffice.Name) || AsChar
        (local.FirstRecord.Flag) == 'Y')
      {
        if (AsChar(local.FirstRecord.Flag) == 'Y')
        {
          local.FirstRecord.Flag = "N";
        }
        else
        {
          // -- Page Break.
          local.EabFileHandling.Action = "NEWPAGE";
          UseCabBusinessReport4();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_01'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          // -- Write new header info.
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_01'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "";
          UseCabBusinessReport4();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_01'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Area: " + local.Area.Name;
        UseCabBusinessReport4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Office: " + local.Office.Name;
        UseCabBusinessReport4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";

        if (IsEmpty(local.ServiceProvider2.LastName) && IsEmpty
          (local.ServiceProvider2.FirstName) && IsEmpty
          (local.ServiceProvider2.MiddleInitial))
        {
          local.EabReportSend.RptDetail = "Legal Service Provider:";
        }
        else
        {
          local.EabReportSend.RptDetail = "Legal Service Provider: " + TrimEnd
            (local.ServiceProvider2.LastName) + ", " + TrimEnd
            (local.ServiceProvider2.FirstName) + " " + local
            .ServiceProvider2.MiddleInitial;
        }

        UseCabBusinessReport4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "";
        UseCabBusinessReport4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "                                                                                                                IWGL";
          
        UseCabBusinessReport4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "                                       Case Worker";
        UseCabBusinessReport4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "AP Name                              Person No      Standard Number        Payor / Payor City & State           Created Date";
          
        UseCabBusinessReport4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "---------------------------------    ----------     --------------------   ------------------------------       ------------";
          
        UseCabBusinessReport4();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_01'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }
      }

      local.PreviousOffice.Name = local.Office.Name;
      local.PreviousServiceProvider.Assign(local.ServiceProvider2);
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = local.CsePersonsWorkSet.FormattedName + "    " +
        local.CsePerson.Number + "     " + (
          local.LegalAction.StandardNumber ?? "") + "   " + (
          local.Employer.Name ?? "") + "    " + NumberToString
        (Month(local.IwglCreatedDate.Date), 14, 2) + "/" + NumberToString
        (Day(local.IwglCreatedDate.Date), 14, 2) + "/" + NumberToString
        (Year(local.IwglCreatedDate.Date), 14, 2);
      UseCabBusinessReport4();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing output file for 'cab_business_report_01'";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_WRITE_ERROR_RB";

        return;
      }

      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "                                       " + local
        .ServiceProvider1.FormattedName + "   " + TrimEnd
        (local.EmployerAddress.City) + ", " + (
          local.EmployerAddress.State ?? "");
      UseCabBusinessReport4();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing output file for 'cab_business_report_01'";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_WRITE_ERROR_RB";

        return;
      }

      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "";
      UseCabBusinessReport4();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing output file for 'cab_business_report_01'";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_WRITE_ERROR_RB";

        return;
      }

      ++local.RecordsWritten.Count;
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

    local.EabReportSend.RptDetail =
      "Total Number Of Delinquent IWO Records Read  :  " + NumberToString
      (local.RecordsRead.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Delinquent IWO Records Read).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total Number Of Delinquent IWO Records Written  :  " + NumberToString
      (local.RecordsWritten.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report(Total number of Delinquent IWO Records Written).";
        
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ************************************************
    // *Close the input file.                         *
    // ************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseLeEabReadIwo45DayInfo2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing input file for 'le_iwo_45_day_delinquency_rpt'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_CLOSE_ERROR";

      return;
    }

    // ************************************************
    // * Close the output file.                       *
    // ************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file for 'le_iwo_45_day_delinquency_rpt'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

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

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.Header.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport3()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport4()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

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
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeEabReadIwo45DayInfo1()
  {
    var useImport = new LeEabReadIwo45DayInfo.Import();
    var useExport = new LeEabReadIwo45DayInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.ReportingPeriodStart.Date = local.StartDateWorkArea.Date;
    useExport.IwglCreatedDate.Date = local.IwglCreatedDate.Date;
    useExport.CsePersonsWorkSet.FormattedName =
      local.CsePersonsWorkSet.FormattedName;
    useExport.EmployerAddress.Assign(local.EmployerAddress);
    useExport.Employer.Name = local.Employer.Name;
    useExport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    useExport.CsePerson.Number = local.CsePerson.Number;
    useExport.ServiceProvider.Assign(local.ServiceProvider2);
    useExport.Office.Name = local.Office.Name;
    MoveEabFileHandling(local.EabFileHandling, useExport.EabFileHandling);
    MoveOffice(local.Area, useExport.Area);
    useExport.Caseworker.FormattedName = local.ServiceProvider1.FormattedName;

    Call(LeEabReadIwo45DayInfo.Execute, useImport, useExport);

    local.StartDateWorkArea.Date = useExport.ReportingPeriodStart.Date;
    local.IwglCreatedDate.Date = useExport.IwglCreatedDate.Date;
    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
    local.EmployerAddress.Assign(useExport.EmployerAddress);
    local.Employer.Name = useExport.Employer.Name;
    local.LegalAction.StandardNumber = useExport.LegalAction.StandardNumber;
    local.CsePerson.Number = useExport.CsePerson.Number;
    MoveServiceProvider(useExport.ServiceProvider, local.ServiceProvider2);
    local.Office.Name = useExport.Office.Name;
    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
    MoveOffice(useExport.Area, local.Area);
    local.ServiceProvider1.FormattedName = useExport.Caseworker.FormattedName;
  }

  private void UseLeEabReadIwo45DayInfo2()
  {
    var useImport = new LeEabReadIwo45DayInfo.Import();
    var useExport = new LeEabReadIwo45DayInfo.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabFileHandling(local.EabFileHandling, useExport.EabFileHandling);

    Call(LeEabReadIwo45DayInfo.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of ServiceProvider1.
    /// </summary>
    [JsonPropertyName("serviceProvider1")]
    public CsePersonsWorkSet ServiceProvider1
    {
      get => serviceProvider1 ??= new();
      set => serviceProvider1 = value;
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
    /// A value of PreviousOffice.
    /// </summary>
    [JsonPropertyName("previousOffice")]
    public Office PreviousOffice
    {
      get => previousOffice ??= new();
      set => previousOffice = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public EabReportSend Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of FirstRecord.
    /// </summary>
    [JsonPropertyName("firstRecord")]
    public Common FirstRecord
    {
      get => firstRecord ??= new();
      set => firstRecord = value;
    }

    /// <summary>
    /// A value of DateMonth.
    /// </summary>
    [JsonPropertyName("dateMonth")]
    public EabReportSend DateMonth
    {
      get => dateMonth ??= new();
      set => dateMonth = value;
    }

    /// <summary>
    /// A value of PreviousServiceProvider.
    /// </summary>
    [JsonPropertyName("previousServiceProvider")]
    public ServiceProvider PreviousServiceProvider
    {
      get => previousServiceProvider ??= new();
      set => previousServiceProvider = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of ServiceProvider2.
    /// </summary>
    [JsonPropertyName("serviceProvider2")]
    public ServiceProvider ServiceProvider2
    {
      get => serviceProvider2 ??= new();
      set => serviceProvider2 = value;
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
    /// A value of IwglCreatedDate.
    /// </summary>
    [JsonPropertyName("iwglCreatedDate")]
    public DateWorkArea IwglCreatedDate
    {
      get => iwglCreatedDate ??= new();
      set => iwglCreatedDate = value;
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
    /// A value of StartDateWorkArea.
    /// </summary>
    [JsonPropertyName("startDateWorkArea")]
    public DateWorkArea StartDateWorkArea
    {
      get => startDateWorkArea ??= new();
      set => startDateWorkArea = value;
    }

    /// <summary>
    /// A value of RecordsWritten.
    /// </summary>
    [JsonPropertyName("recordsWritten")]
    public Common RecordsWritten
    {
      get => recordsWritten ??= new();
      set => recordsWritten = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
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
    /// A value of StartCommon.
    /// </summary>
    [JsonPropertyName("startCommon")]
    public Common StartCommon
    {
      get => startCommon ??= new();
      set => startCommon = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
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
    /// A value of NumberOfDays.
    /// </summary>
    [JsonPropertyName("numberOfDays")]
    public Common NumberOfDays
    {
      get => numberOfDays ??= new();
      set => numberOfDays = value;
    }

    /// <summary>
    /// A value of NumberOfWeeks.
    /// </summary>
    [JsonPropertyName("numberOfWeeks")]
    public Common NumberOfWeeks
    {
      get => numberOfWeeks ??= new();
      set => numberOfWeeks = value;
    }

    /// <summary>
    /// A value of RptPeriodNumOfWeeks.
    /// </summary>
    [JsonPropertyName("rptPeriodNumOfWeeks")]
    public Common RptPeriodNumOfWeeks
    {
      get => rptPeriodNumOfWeeks ??= new();
      set => rptPeriodNumOfWeeks = value;
    }

    /// <summary>
    /// A value of NumberOfDaysInPeriod.
    /// </summary>
    [JsonPropertyName("numberOfDaysInPeriod")]
    public Common NumberOfDaysInPeriod
    {
      get => numberOfDaysInPeriod ??= new();
      set => numberOfDaysInPeriod = value;
    }

    private CsePersonsWorkSet serviceProvider1;
    private Office area;
    private Office previousOffice;
    private EabReportSend header;
    private Common firstRecord;
    private EabReportSend dateMonth;
    private ServiceProvider previousServiceProvider;
    private Office office;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private Employer employer;
    private EmployerAddress employerAddress;
    private ServiceProvider serviceProvider2;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea iwglCreatedDate;
    private DateWorkArea end;
    private DateWorkArea startDateWorkArea;
    private Common recordsWritten;
    private Common recordsRead;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common fileOpened;
    private Common startCommon;
    private Common current;
    private Common currentPosition;
    private Common fieldNumber;
    private TextWorkArea postion;
    private WorkArea workArea;
    private Common numberOfDays;
    private Common numberOfWeeks;
    private Common rptPeriodNumOfWeeks;
    private Common numberOfDaysInPeriod;
  }
#endregion
}
