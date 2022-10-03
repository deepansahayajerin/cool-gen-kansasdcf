// Program: LE_B601_REFRAL_FUNCTION_REPORT, ID: 374510564, model: 746.
// Short name: SWEL601B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B601_REFRAL_FUNCTION_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB601RefralFunctionReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B601_REFRAL_FUNCTION_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB601RefralFunctionReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB601RefralFunctionReport.
  /// </summary>
  public LeB601RefralFunctionReport(IContext context, Import import,
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
    // 09/24/09  DDupree            CQ12650/
    //                             CQ12650    Initial Development
    // *********************************************************************
    // *********************************************************************
    // This procedure processes 1 input file.
    // The file is read in its entirety and from the data contained
    // in the file the 'REFERRALS BY CASE FUNCTION PER LEGAL SERVICE PROVIDER' 
    // report is created.
    // *********************************************************************
    local.EabReportSend.RptDetail = local.EabHeader.ColHeading1;
    ExitState = "ACO_NN0000_ALL_OK";
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

    // ***********************************************************************
    // Call External to Open the input Files.
    // Note: This external performs actions for input file.
    //       It is not necessary to pass a file number on the OPEN or CLOSE
    //       commands, as it OPENs or CLOSEs both files.  The file
    //       number must be passed on a READ command.
    // ***********************************************************************
    local.External.FileInstruction = "OPEN";
    UseLeEabReadReferCaseFunction1();

    if (!Equal(local.External.TextReturnCode, "00"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening input file for 'le_eab_read_refer_case_function'. Status = " +
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
    // *Call External to Open the Report.          *
    // ************************************************
    local.DateWorkArea.TextDate =
      NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8);
    local.Common.TotalInteger =
      StringToNumber(Substring(
        local.DateWorkArea.TextDate, DateWorkArea.TextDate_MaxLength, 1, 6) +
      "01");
    local.FirstDayCurrentMonth.Date = IntToDate((int)local.Common.TotalInteger);
    local.LastDayPrevMonth.Date = AddDays(local.FirstDayCurrentMonth.Date, -1);
    local.DateWorkArea.TextDate =
      NumberToString(DateToInt(local.LastDayPrevMonth.Date), 8);
    local.Translate.TextDateMm = Substring(local.DateWorkArea.TextDate, 5, 2);
    local.Translate.TextDateYyyy = Substring(local.DateWorkArea.TextDate, 1, 4);
    local.Translate.TestDateDd = Substring(local.DateWorkArea.TextDate, 7, 2);

    switch(TrimEnd(local.Translate.TextDateMm))
    {
      case "01":
        local.Date.Text18 = "January";

        break;
      case "02":
        local.Date.Text18 = "February";

        break;
      case "03":
        local.Date.Text18 = "March";

        break;
      case "04":
        local.Date.Text18 = "April";

        break;
      case "05":
        local.Date.Text18 = "May";

        break;
      case "06":
        local.Date.Text18 = "June";

        break;
      case "07":
        local.Date.Text18 = "July";

        break;
      case "08":
        local.Date.Text18 = "August";

        break;
      case "09":
        local.Date.Text18 = "September";

        break;
      case "10":
        local.Date.Text18 = "October";

        break;
      case "11":
        local.Date.Text18 = "November";

        break;
      case "12":
        local.Date.Text18 = "December";

        break;
      default:
        local.Date.Text18 = "";

        break;
    }

    if (!IsEmpty(local.Date.Text18))
    {
      local.Date.Text18 = TrimEnd(local.Date.Text18) + " " + local
        .Translate.TestDateDd + ", " + local.Translate.TextDateYyyy;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabHeader.BlankLineAfterHeading = "Y";
    local.EabHeader.BlankLineAfterColHead = "Y";
    local.EabHeader.RptHeading3 =
      "SRRUN216  Legal Referral by Case Function as of";
    local.EabHeader.RptHeading3 = TrimEnd(local.EabHeader.RptHeading3) + " " + local
      .Date.Text18;
    local.EabHeader.ColHeading1 =
      "                                       Locate         Paternity      Obligation     Enforcement    Total Cases Referred";
      
    local.EabHeader.ProgramName = global.UserId;
    local.EabHeader.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabBusinessReport1();

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

    // -----------------------------------------------------------------------------------------------
    // Create the header for the reports.  The header identifies the month for 
    // which the reports apply.
    // (i.e. The full calendar month for the month prior to the processing date
    // ).
    // -----------------------------------------------------------------------------------------------
    local.EabReportSend.RptDetail = local.EabHeader.ColHeading1;
    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport3();

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

    // -- Write Report
    local.FirstRecord.Flag = "Y";

    do
    {
      local.External.FileInstruction = "READ";
      UseLeEabReadReferCaseFunction2();

      switch(TrimEnd(local.External.TextReturnCode))
      {
        case "00":
          ++local.RecordsRead.Count;

          break;
        case "EF":
          local.EndOfInputFile.Flag = "Y";

          if (AsChar(local.FirstRecord.Flag) == 'Y')
          {
            continue;
          }

          break;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error reading input file for 'le_eab_refer_case function'.";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
      }

      if (local.CurrentLegal.SystemGeneratedId == local
        .PreviousLegal.SystemGeneratedId || local
        .PreviousLegal.SystemGeneratedId <= 0)
      {
        if (Equal(local.Current.Name, local.Previous.Name) || IsEmpty
          (local.Previous.Name))
        {
          switch(AsChar(local.CaseFuncWorkSet.FuncText1))
          {
            case 'L':
              ++local.LocateTotalOffice.Count;

              break;
            case 'P':
              ++local.PaternityTotalOffice.Count;

              break;
            case 'O':
              ++local.ObligationTotalOffice.Count;

              break;
            case 'E':
              ++local.EnfTotalOffice.Count;

              break;
            default:
              break;
          }

          ++local.OfficeTotalCommon.Count;
          local.PreviousLegal.Assign(local.CurrentLegal);

          if (IsEmpty(local.Previous.Name))
          {
            // this is the first time through so we will write out the lspo name
            // now, instead of later
            local.Previous.Name = local.Current.Name;

            if (Equal(local.PreviousLegal.LastName, "AAAAA") && Equal
              (local.PreviousLegal.FirstName, "AAAAA"))
            {
              local.EabReportSend.RptDetail = "Service Provider Not Found";
            }
            else
            {
              local.CsePersonsWorkSet.FormattedName = "";
              local.CsePersonsWorkSet.LastName = local.PreviousLegal.LastName;
              local.CsePersonsWorkSet.FirstName = local.PreviousLegal.FirstName;
              UseSiFormatCsePersonName();
              local.EabReportSend.RptDetail =
                local.CsePersonsWorkSet.FormattedName;
            }

            // -- Write the lspo name.
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport3();

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

          local.Previous.Name = local.Current.Name;

          continue;
        }
        else
        {
          // -- write the previous office totals before processing the next 
          // office
          local.LocateTotal.Text15 =
            NumberToString(local.LocateTotalOffice.Count, 15);
          local.PaternityTotal.Text15 =
            NumberToString(local.PaternityTotalOffice.Count, 15);
          local.ObligationTotal.Text15 =
            NumberToString(local.ObligationTotalOffice.Count, 15);
          local.EnforacementTotal.Text15 =
            NumberToString(local.EnfTotalOffice.Count, 15);
          local.OfficeTotalWorkArea.Text15 =
            NumberToString(local.OfficeTotalCommon.Count, 15);

          if (local.LocateTotalOffice.Count > 0)
          {
            local.PlaceCount.Count = Verify(local.LocateTotal.Text15, "0");
            local.LocateTotal.Text15 =
              Substring(local.LocateTotal.Text15, local.PlaceCount.Count, 16 -
              local.PlaceCount.Count);
          }
          else
          {
            local.LocateTotal.Text15 = "0";
          }

          if (local.PaternityTotalOffice.Count > 0)
          {
            local.PlaceCount.Count = Verify(local.PaternityTotal.Text15, "0");
            local.PaternityTotal.Text15 =
              Substring(local.PaternityTotal.Text15, local.PlaceCount.Count,
              16 - local.PlaceCount.Count);
          }
          else
          {
            local.PaternityTotal.Text15 = "0";
          }

          if (local.ObligationTotalOffice.Count > 0)
          {
            local.PlaceCount.Count = Verify(local.ObligationTotal.Text15, "0");
            local.ObligationTotal.Text15 =
              Substring(local.ObligationTotal.Text15, local.PlaceCount.Count,
              16 - local.PlaceCount.Count);
          }
          else
          {
            local.ObligationTotal.Text15 = "0";
          }

          if (local.EnfTotalOffice.Count > 0)
          {
            local.PlaceCount.Count =
              Verify(local.EnforacementTotal.Text15, "0");
            local.EnforacementTotal.Text15 =
              Substring(local.EnforacementTotal.Text15, local.PlaceCount.Count,
              16 - local.PlaceCount.Count);
          }
          else
          {
            local.EnforacementTotal.Text15 = "0";
          }

          if (local.OfficeTotalCommon.Count > 0)
          {
            local.PlaceCount.Count =
              Verify(local.OfficeTotalWorkArea.Text15, "0");
            local.OfficeTotalWorkArea.Text15 =
              Substring(local.OfficeTotalWorkArea.Text15,
              local.PlaceCount.Count, 16 - local.PlaceCount.Count);
          }
          else
          {
            local.OfficeTotalWorkArea.Text15 = "0";
          }

          // -- Write the referral detail info.
          local.EabFileHandling.Action = "WRITE";
          local.Name.Text40 = "     " + local.Previous.Name;
          local.EabReportSend.RptDetail = local.Name.Text40 + local
            .LocateTotal.Text15 + local.PaternityTotal.Text15 + local
            .ObligationTotal.Text15 + local.EnforacementTotal.Text15 + local
            .OfficeTotalWorkArea.Text15;
          UseCabBusinessReport3();

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

          // -- start counting for the next office for the same lspo
          local.LspoLocateTotal.Count += local.LocateTotalOffice.Count;
          local.LspoPaternityTotal.Count += local.PaternityTotalOffice.Count;
          local.LspoObligationTotal.Count += local.ObligationTotalOffice.Count;
          local.LspoEnfTotal.Count += local.EnfTotalOffice.Count;
          local.LspoOfficeTotal.Count += local.OfficeTotalCommon.Count;
          local.LocateTotalOffice.Count = 0;
          local.PaternityTotalOffice.Count = 0;
          local.ObligationTotalOffice.Count = 0;
          local.EnfTotalOffice.Count = 0;
          local.OfficeTotalCommon.Count = 0;

          switch(AsChar(local.CaseFuncWorkSet.FuncText1))
          {
            case 'L':
              ++local.LocateTotalOffice.Count;

              break;
            case 'P':
              ++local.PaternityTotalOffice.Count;

              break;
            case 'O':
              ++local.ObligationTotalOffice.Count;

              break;
            case 'E':
              ++local.EnfTotalOffice.Count;

              break;
            default:
              break;
          }

          ++local.OfficeTotalCommon.Count;
          local.Previous.Name = local.Current.Name;

          continue;
        }
      }
      else
      {
        // -- write the previous office total for the lspo
        local.LocateTotal.Text15 =
          NumberToString(local.LocateTotalOffice.Count, 15);
        local.PaternityTotal.Text15 =
          NumberToString(local.PaternityTotalOffice.Count, 15);
        local.ObligationTotal.Text15 =
          NumberToString(local.ObligationTotalOffice.Count, 15);
        local.EnforacementTotal.Text15 =
          NumberToString(local.EnfTotalOffice.Count, 15);
        local.OfficeTotalWorkArea.Text15 =
          NumberToString(local.OfficeTotalCommon.Count, 15);

        if (local.LocateTotalOffice.Count > 0)
        {
          local.PlaceCount.Count = Verify(local.LocateTotal.Text15, "0");
          local.LocateTotal.Text15 =
            Substring(local.LocateTotal.Text15, local.PlaceCount.Count, 16 -
            local.PlaceCount.Count);
        }
        else
        {
          local.LocateTotal.Text15 = "0";
        }

        if (local.PaternityTotalOffice.Count > 0)
        {
          local.PlaceCount.Count = Verify(local.PaternityTotal.Text15, "0");
          local.PaternityTotal.Text15 =
            Substring(local.PaternityTotal.Text15, local.PlaceCount.Count, 16 -
            local.PlaceCount.Count);
        }
        else
        {
          local.PaternityTotal.Text15 = "0";
        }

        if (local.ObligationTotalOffice.Count > 0)
        {
          local.PlaceCount.Count = Verify(local.ObligationTotal.Text15, "0");
          local.ObligationTotal.Text15 =
            Substring(local.ObligationTotal.Text15, local.PlaceCount.Count, 16 -
            local.PlaceCount.Count);
        }
        else
        {
          local.ObligationTotal.Text15 = "0";
        }

        if (local.EnfTotalOffice.Count > 0)
        {
          local.PlaceCount.Count = Verify(local.EnforacementTotal.Text15, "0");
          local.EnforacementTotal.Text15 =
            Substring(local.EnforacementTotal.Text15, local.PlaceCount.Count,
            16 - local.PlaceCount.Count);
        }
        else
        {
          local.EnforacementTotal.Text15 = "0";
        }

        if (local.OfficeTotalCommon.Count > 0)
        {
          local.PlaceCount.Count =
            Verify(local.OfficeTotalWorkArea.Text15, "0");
          local.OfficeTotalWorkArea.Text15 =
            Substring(local.OfficeTotalWorkArea.Text15, local.PlaceCount.Count,
            16 - local.PlaceCount.Count);
        }
        else
        {
          local.OfficeTotalWorkArea.Text15 = "0";
        }

        // -- Write the referral detail info.
        local.EabFileHandling.Action = "WRITE";
        local.Name.Text40 = "     " + local.Previous.Name;
        local.EabReportSend.RptDetail = local.Name.Text40 + local
          .LocateTotal.Text15 + local.PaternityTotal.Text15 + local
          .ObligationTotal.Text15 + local.EnforacementTotal.Text15 + local
          .OfficeTotalWorkArea.Text15;
        UseCabBusinessReport3();

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
        local.EabReportSend.RptDetail = "";

        // now we are totalling up the previous legal service provider, writing 
        // it out to the report, then starting on the next provider
        local.LspoLocateTotal.Count += local.LocateTotalOffice.Count;
        local.LspoPaternityTotal.Count += local.PaternityTotalOffice.Count;
        local.LspoObligationTotal.Count += local.ObligationTotalOffice.Count;
        local.LspoEnfTotal.Count += local.EnfTotalOffice.Count;
        local.LspoOfficeTotal.Count += local.OfficeTotalCommon.Count;
        local.LocateTotal.Text15 =
          NumberToString(local.LspoLocateTotal.Count, 15);
        local.PaternityTotal.Text15 =
          NumberToString(local.LspoPaternityTotal.Count, 15);
        local.ObligationTotal.Text15 =
          NumberToString(local.LspoObligationTotal.Count, 15);
        local.EnforacementTotal.Text15 =
          NumberToString(local.LspoEnfTotal.Count, 15);
        local.OfficeTotalWorkArea.Text15 =
          NumberToString(local.LspoOfficeTotal.Count, 15);

        if (local.LspoLocateTotal.Count > 0)
        {
          local.PlaceCount.Count = Verify(local.LocateTotal.Text15, "0");
          local.LocateTotal.Text15 =
            Substring(local.LocateTotal.Text15, local.PlaceCount.Count, 16 -
            local.PlaceCount.Count);
        }
        else
        {
          local.LocateTotal.Text15 = "0";
        }

        if (local.LspoPaternityTotal.Count > 0)
        {
          local.PlaceCount.Count = Verify(local.PaternityTotal.Text15, "0");
          local.PaternityTotal.Text15 =
            Substring(local.PaternityTotal.Text15, local.PlaceCount.Count, 16 -
            local.PlaceCount.Count);
        }
        else
        {
          local.PaternityTotal.Text15 = "0";
        }

        if (local.LspoObligationTotal.Count > 0)
        {
          local.PlaceCount.Count = Verify(local.ObligationTotal.Text15, "0");
          local.ObligationTotal.Text15 =
            Substring(local.ObligationTotal.Text15, local.PlaceCount.Count, 16 -
            local.PlaceCount.Count);
        }
        else
        {
          local.ObligationTotal.Text15 = "0";
        }

        if (local.LspoEnfTotal.Count > 0)
        {
          local.PlaceCount.Count = Verify(local.EnforacementTotal.Text15, "0");
          local.EnforacementTotal.Text15 =
            Substring(local.EnforacementTotal.Text15, local.PlaceCount.Count,
            16 - local.PlaceCount.Count);
        }
        else
        {
          local.EnforacementTotal.Text15 = "0";
        }

        if (local.LspoOfficeTotal.Count > 0)
        {
          local.PlaceCount.Count =
            Verify(local.OfficeTotalWorkArea.Text15, "0");
          local.OfficeTotalWorkArea.Text15 =
            Substring(local.OfficeTotalWorkArea.Text15, local.PlaceCount.Count,
            16 - local.PlaceCount.Count);
        }
        else
        {
          local.OfficeTotalWorkArea.Text15 = "0";
        }

        // -- Write the lspo summary info.
        local.EabFileHandling.Action = "WRITE";
        local.Name.Text40 = "Sum All Offices";
        local.EabReportSend.RptDetail = local.Name.Text40 + local
          .LocateTotal.Text15 + local.PaternityTotal.Text15 + local
          .ObligationTotal.Text15 + local.EnforacementTotal.Text15 + local
          .OfficeTotalWorkArea.Text15;
        UseCabBusinessReport3();

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
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "";
        UseCabBusinessReport3();

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
        UseCabBusinessReport3();

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

        // page break and write a new page header
        local.EabFileHandling.Action = "NEWPAGE";
        local.EabReportSend.RptDetail = "";
        UseCabBusinessReport3();

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

        local.EabReportSend.RptDetail = local.EabHeader.ColHeading1;
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport3();

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

        local.EabReportSend.RptDetail = "";
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport3();

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

        local.RptLocateTotal.Count += local.LspoLocateTotal.Count;
        local.RptPaternityTotal.Count += local.LspoPaternityTotal.Count;
        local.RptObligaitonTotal.Count += local.LspoObligationTotal.Count;
        local.RptEnforcementTotal.Count += local.LspoEnfTotal.Count;
        local.RptOfficeTotal.Count += local.LspoOfficeTotal.Count;
        local.LocateTotalOffice.Count = 0;
        local.PaternityTotalOffice.Count = 0;
        local.ObligationTotalOffice.Count = 0;
        local.EnfTotalOffice.Count = 0;
        local.OfficeTotalCommon.Count = 0;
        local.LspoLocateTotal.Count = 0;
        local.LspoPaternityTotal.Count = 0;
        local.LspoObligationTotal.Count = 0;
        local.LspoEnfTotal.Count = 0;
        local.LspoOfficeTotal.Count = 0;

        // -- Start counting the next lspo functions.
        switch(AsChar(local.CaseFuncWorkSet.FuncText1))
        {
          case 'L':
            ++local.LocateTotalOffice.Count;

            break;
          case 'P':
            ++local.PaternityTotalOffice.Count;

            break;
          case 'O':
            ++local.ObligationTotalOffice.Count;

            break;
          case 'E':
            ++local.EnfTotalOffice.Count;

            break;
          default:
            break;
        }

        ++local.OfficeTotalCommon.Count;
        local.PreviousLegal.Assign(local.CurrentLegal);
        local.Previous.Name = local.Current.Name;

        if (Equal(local.PreviousLegal.LastName, "AAAAA") && Equal
          (local.PreviousLegal.FirstName, "AAAAA"))
        {
          local.EabReportSend.RptDetail = "Service Provider Not Found";
        }
        else
        {
          local.CsePersonsWorkSet.FormattedName = "";
          local.CsePersonsWorkSet.LastName = local.PreviousLegal.LastName;
          local.CsePersonsWorkSet.FirstName = local.PreviousLegal.FirstName;
          UseSiFormatCsePersonName();
          local.EabReportSend.RptDetail = local.CsePersonsWorkSet.FormattedName;
        }

        // -- Write the lspo name.
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport3();

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
    }
    while(AsChar(local.EndOfInputFile.Flag) != 'Y');

    // -- process the last lspo office
    local.LocateTotal.Text15 =
      NumberToString(local.LocateTotalOffice.Count, 15);
    local.PaternityTotal.Text15 =
      NumberToString(local.PaternityTotalOffice.Count, 15);
    local.ObligationTotal.Text15 =
      NumberToString(local.ObligationTotalOffice.Count, 15);
    local.EnforacementTotal.Text15 =
      NumberToString(local.EnfTotalOffice.Count, 15);
    local.OfficeTotalWorkArea.Text15 =
      NumberToString(local.OfficeTotalCommon.Count, 15);

    if (local.LocateTotalOffice.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.LocateTotal.Text15, "0");
      local.LocateTotal.Text15 =
        Substring(local.LocateTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.LocateTotal.Text15 = "0";
    }

    if (local.PaternityTotalOffice.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.PaternityTotal.Text15, "0");
      local.PaternityTotal.Text15 =
        Substring(local.PaternityTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.PaternityTotal.Text15 = "0";
    }

    if (local.ObligationTotalOffice.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.ObligationTotal.Text15, "0");
      local.ObligationTotal.Text15 =
        Substring(local.ObligationTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.ObligationTotal.Text15 = "0";
    }

    if (local.EnfTotalOffice.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.EnforacementTotal.Text15, "0");
      local.EnforacementTotal.Text15 =
        Substring(local.EnforacementTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.EnforacementTotal.Text15 = "0";
    }

    if (local.OfficeTotalCommon.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.OfficeTotalWorkArea.Text15, "0");
      local.OfficeTotalWorkArea.Text15 =
        Substring(local.OfficeTotalWorkArea.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.OfficeTotalWorkArea.Text15 = "0";
    }

    // -- Write the referral detail info.
    local.EabFileHandling.Action = "WRITE";
    local.Name.Text40 = "     " + local.Previous.Name;
    local.EabReportSend.RptDetail = local.Name.Text40 + local
      .LocateTotal.Text15 + local.PaternityTotal.Text15 + local
      .ObligationTotal.Text15 + local.EnforacementTotal.Text15 + local
      .OfficeTotalWorkArea.Text15;
    UseCabBusinessReport3();

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

    // now we are totalling up the last legal service provider, writing it out 
    // to the report, then totaling out the report
    local.LspoLocateTotal.Count += local.LocateTotalOffice.Count;
    local.LspoPaternityTotal.Count += local.PaternityTotalOffice.Count;
    local.LspoObligationTotal.Count += local.ObligationTotalOffice.Count;
    local.LspoEnfTotal.Count += local.EnfTotalOffice.Count;
    local.LspoOfficeTotal.Count += local.OfficeTotalCommon.Count;
    local.LocateTotal.Text15 = NumberToString(local.LspoLocateTotal.Count, 15);
    local.PaternityTotal.Text15 =
      NumberToString(local.LspoPaternityTotal.Count, 15);
    local.ObligationTotal.Text15 =
      NumberToString(local.LspoObligationTotal.Count, 15);
    local.EnforacementTotal.Text15 =
      NumberToString(local.LspoEnfTotal.Count, 15);
    local.OfficeTotalWorkArea.Text15 =
      NumberToString(local.LspoOfficeTotal.Count, 15);

    if (local.LspoLocateTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.LocateTotal.Text15, "0");
      local.LocateTotal.Text15 =
        Substring(local.LocateTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.LocateTotal.Text15 = "0";
    }

    if (local.LspoPaternityTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.PaternityTotal.Text15, "0");
      local.PaternityTotal.Text15 =
        Substring(local.PaternityTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.PaternityTotal.Text15 = "0";
    }

    if (local.LspoObligationTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.ObligationTotal.Text15, "0");
      local.ObligationTotal.Text15 =
        Substring(local.ObligationTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.ObligationTotal.Text15 = "0";
    }

    if (local.LspoEnfTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.EnforacementTotal.Text15, "0");
      local.EnforacementTotal.Text15 =
        Substring(local.EnforacementTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.EnforacementTotal.Text15 = "0";
    }

    if (local.LspoOfficeTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.OfficeTotalWorkArea.Text15, "0");
      local.OfficeTotalWorkArea.Text15 =
        Substring(local.OfficeTotalWorkArea.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.OfficeTotalWorkArea.Text15 = "0";
    }

    // -- Write the lspo summary info.
    local.EabFileHandling.Action = "WRITE";
    local.Name.Text40 = "Sum All Offices";
    local.EabReportSend.RptDetail = local.Name.Text40 + local
      .LocateTotal.Text15 + local.PaternityTotal.Text15 + local
      .ObligationTotal.Text15 + local.EnforacementTotal.Text15 + local
      .OfficeTotalWorkArea.Text15;
    UseCabBusinessReport3();

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

    // put space at the end of report
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
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

    // page break and will write a new header
    local.EabFileHandling.Action = "NEWPAGE";
    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport3();

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

    // col heading 1
    local.EabReportSend.RptDetail = local.EabHeader.ColHeading1;
    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport3();

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

    // spaces
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport3();

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

    // figure report totals
    local.RptLocateTotal.Count += local.LspoLocateTotal.Count;
    local.RptPaternityTotal.Count += local.LspoPaternityTotal.Count;
    local.RptObligaitonTotal.Count += local.LspoObligationTotal.Count;
    local.RptEnforcementTotal.Count += local.LspoEnfTotal.Count;
    local.RptOfficeTotal.Count += local.LspoOfficeTotal.Count;
    local.LocateTotal.Text15 = NumberToString(local.RptLocateTotal.Count, 15);
    local.PaternityTotal.Text15 =
      NumberToString(local.RptPaternityTotal.Count, 15);
    local.ObligationTotal.Text15 =
      NumberToString(local.RptObligaitonTotal.Count, 15);
    local.EnforacementTotal.Text15 =
      NumberToString(local.RptEnforcementTotal.Count, 15);
    local.OfficeTotalWorkArea.Text15 =
      NumberToString(local.RptOfficeTotal.Count, 15);

    if (local.RptLocateTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.LocateTotal.Text15, "0");
      local.LocateTotal.Text15 =
        Substring(local.LocateTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.LocateTotal.Text15 = "0";
    }

    if (local.RptPaternityTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.PaternityTotal.Text15, "0");
      local.PaternityTotal.Text15 =
        Substring(local.PaternityTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.PaternityTotal.Text15 = "0";
    }

    if (local.RptObligaitonTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.ObligationTotal.Text15, "0");
      local.ObligationTotal.Text15 =
        Substring(local.ObligationTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.ObligationTotal.Text15 = "0";
    }

    if (local.RptEnforcementTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.EnforacementTotal.Text15, "0");
      local.EnforacementTotal.Text15 =
        Substring(local.EnforacementTotal.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.EnforacementTotal.Text15 = "0";
    }

    if (local.RptOfficeTotal.Count > 0)
    {
      local.PlaceCount.Count = Verify(local.OfficeTotalWorkArea.Text15, "0");
      local.OfficeTotalWorkArea.Text15 =
        Substring(local.OfficeTotalWorkArea.Text15, local.PlaceCount.Count, 16 -
        local.PlaceCount.Count);
    }
    else
    {
      local.OfficeTotalWorkArea.Text15 = "0";
    }

    local.Name.Text40 = "Statewide Summary";
    local.EabReportSend.RptDetail = local.Name.Text40 + local
      .LocateTotal.Text15 + local.PaternityTotal.Text15 + local
      .ObligationTotal.Text15 + local.EnforacementTotal.Text15 + local
      .OfficeTotalWorkArea.Text15;

    // -- Write the report summary info.
    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing output file for 'cab_business_report'";
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

    // spaces
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport3();

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

    // -- Write the control report record counts.
    local.EabReportSend.RptDetail = "Total Number Of Records Read :     " + NumberToString
      (local.RecordsRead.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Total Number Of Records Written :  " + NumberToString
      (local.RecordsWritten.Count, 15);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while writting control report.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***********************************************************************
    // Call External to Close the input Files.
    // Note: This external performs actions for both input files.
    //       It is not necessary to pass a file number on the OPEN or CLOSE
    //       commands, as it OPENs or CLOSEs both files.  The file
    //       number must be passed on a READ command.
    // ***********************************************************************
    local.External.FileInstruction = "CLOSE";
    UseLeEabReadReferCaseFunction1();

    if (!Equal(local.External.TextReturnCode, "00"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing input file for 'le_eab_read_refer_case_function'.";
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
    // * Close Report.
    // 
    // *
    // ************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file for 'cab_business_report'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    // -- Close the control report.
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

    // -- Close the error report.
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterHeading = source.BlankLineAfterHeading;
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.RptHeading3 = source.RptHeading3;
    target.ColHeading1 = source.ColHeading1;
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    MoveEabReportSend1(local.EabHeader, useImport.NeededToOpen);
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

  private void UseLeEabReadReferCaseFunction1()
  {
    var useImport = new LeEabReadReferCaseFunction.Import();
    var useExport = new LeEabReadReferCaseFunction.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useExport.External.Assign(local.External);

    Call(LeEabReadReferCaseFunction.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseLeEabReadReferCaseFunction2()
  {
    var useImport = new LeEabReadReferCaseFunction.Import();
    var useExport = new LeEabReadReferCaseFunction.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useExport.Legal.Assign(local.CurrentLegal);
    useExport.Office.Name = local.Current.Name;
    useExport.CaseFuncWorkSet.FuncText1 = local.CaseFuncWorkSet.FuncText1;
    useExport.External.Assign(local.External);

    Call(LeEabReadReferCaseFunction.Execute, useImport, useExport);

    local.CurrentLegal.Assign(useExport.Legal);
    local.Current.Name = useExport.Office.Name;
    local.CaseFuncWorkSet.FuncText1 = useExport.CaseFuncWorkSet.FuncText1;
    local.External.Assign(useExport.External);
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
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
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
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public WorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Translate.
    /// </summary>
    [JsonPropertyName("translate")]
    public BatchTimestampWorkArea Translate
    {
      get => translate ??= new();
      set => translate = value;
    }

    /// <summary>
    /// A value of EabHeader.
    /// </summary>
    [JsonPropertyName("eabHeader")]
    public EabReportSend EabHeader
    {
      get => eabHeader ??= new();
      set => eabHeader = value;
    }

    /// <summary>
    /// A value of PlaceCount.
    /// </summary>
    [JsonPropertyName("placeCount")]
    public Common PlaceCount
    {
      get => placeCount ??= new();
      set => placeCount = value;
    }

    /// <summary>
    /// A value of OfficeTotalWorkArea.
    /// </summary>
    [JsonPropertyName("officeTotalWorkArea")]
    public WorkArea OfficeTotalWorkArea
    {
      get => officeTotalWorkArea ??= new();
      set => officeTotalWorkArea = value;
    }

    /// <summary>
    /// A value of EnforacementTotal.
    /// </summary>
    [JsonPropertyName("enforacementTotal")]
    public WorkArea EnforacementTotal
    {
      get => enforacementTotal ??= new();
      set => enforacementTotal = value;
    }

    /// <summary>
    /// A value of ObligationTotal.
    /// </summary>
    [JsonPropertyName("obligationTotal")]
    public WorkArea ObligationTotal
    {
      get => obligationTotal ??= new();
      set => obligationTotal = value;
    }

    /// <summary>
    /// A value of PaternityTotal.
    /// </summary>
    [JsonPropertyName("paternityTotal")]
    public WorkArea PaternityTotal
    {
      get => paternityTotal ??= new();
      set => paternityTotal = value;
    }

    /// <summary>
    /// A value of LocateTotal.
    /// </summary>
    [JsonPropertyName("locateTotal")]
    public WorkArea LocateTotal
    {
      get => locateTotal ??= new();
      set => locateTotal = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of PreviousLegal.
    /// </summary>
    [JsonPropertyName("previousLegal")]
    public ServiceProvider PreviousLegal
    {
      get => previousLegal ??= new();
      set => previousLegal = value;
    }

    /// <summary>
    /// A value of CurrentLegal.
    /// </summary>
    [JsonPropertyName("currentLegal")]
    public ServiceProvider CurrentLegal
    {
      get => currentLegal ??= new();
      set => currentLegal = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Office Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Office Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Spaces.
    /// </summary>
    [JsonPropertyName("spaces")]
    public EabReportSend Spaces
    {
      get => spaces ??= new();
      set => spaces = value;
    }

    /// <summary>
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public Common FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
    }

    /// <summary>
    /// A value of RefType.
    /// </summary>
    [JsonPropertyName("refType")]
    public TextWorkArea RefType
    {
      get => refType ??= new();
      set => refType = value;
    }

    /// <summary>
    /// A value of Rsn.
    /// </summary>
    [JsonPropertyName("rsn")]
    public TextWorkArea Rsn
    {
      get => rsn ??= new();
      set => rsn = value;
    }

    /// <summary>
    /// A value of EndOfInputFile.
    /// </summary>
    [JsonPropertyName("endOfInputFile")]
    public Common EndOfInputFile
    {
      get => endOfInputFile ??= new();
      set => endOfInputFile = value;
    }

    /// <summary>
    /// A value of LocateTotalOffice.
    /// </summary>
    [JsonPropertyName("locateTotalOffice")]
    public Common LocateTotalOffice
    {
      get => locateTotalOffice ??= new();
      set => locateTotalOffice = value;
    }

    /// <summary>
    /// A value of PaternityTotalOffice.
    /// </summary>
    [JsonPropertyName("paternityTotalOffice")]
    public Common PaternityTotalOffice
    {
      get => paternityTotalOffice ??= new();
      set => paternityTotalOffice = value;
    }

    /// <summary>
    /// A value of EnfTotalOffice.
    /// </summary>
    [JsonPropertyName("enfTotalOffice")]
    public Common EnfTotalOffice
    {
      get => enfTotalOffice ??= new();
      set => enfTotalOffice = value;
    }

    /// <summary>
    /// A value of ObligationTotalOffice.
    /// </summary>
    [JsonPropertyName("obligationTotalOffice")]
    public Common ObligationTotalOffice
    {
      get => obligationTotalOffice ??= new();
      set => obligationTotalOffice = value;
    }

    /// <summary>
    /// A value of OfficeTotalCommon.
    /// </summary>
    [JsonPropertyName("officeTotalCommon")]
    public Common OfficeTotalCommon
    {
      get => officeTotalCommon ??= new();
      set => officeTotalCommon = value;
    }

    /// <summary>
    /// A value of LspoLocateTotal.
    /// </summary>
    [JsonPropertyName("lspoLocateTotal")]
    public Common LspoLocateTotal
    {
      get => lspoLocateTotal ??= new();
      set => lspoLocateTotal = value;
    }

    /// <summary>
    /// A value of LspoPaternityTotal.
    /// </summary>
    [JsonPropertyName("lspoPaternityTotal")]
    public Common LspoPaternityTotal
    {
      get => lspoPaternityTotal ??= new();
      set => lspoPaternityTotal = value;
    }

    /// <summary>
    /// A value of LspoObligationTotal.
    /// </summary>
    [JsonPropertyName("lspoObligationTotal")]
    public Common LspoObligationTotal
    {
      get => lspoObligationTotal ??= new();
      set => lspoObligationTotal = value;
    }

    /// <summary>
    /// A value of LspoEnfTotal.
    /// </summary>
    [JsonPropertyName("lspoEnfTotal")]
    public Common LspoEnfTotal
    {
      get => lspoEnfTotal ??= new();
      set => lspoEnfTotal = value;
    }

    /// <summary>
    /// A value of LspoOfficeTotal.
    /// </summary>
    [JsonPropertyName("lspoOfficeTotal")]
    public Common LspoOfficeTotal
    {
      get => lspoOfficeTotal ??= new();
      set => lspoOfficeTotal = value;
    }

    /// <summary>
    /// A value of RptLocateTotal.
    /// </summary>
    [JsonPropertyName("rptLocateTotal")]
    public Common RptLocateTotal
    {
      get => rptLocateTotal ??= new();
      set => rptLocateTotal = value;
    }

    /// <summary>
    /// A value of RptPaternityTotal.
    /// </summary>
    [JsonPropertyName("rptPaternityTotal")]
    public Common RptPaternityTotal
    {
      get => rptPaternityTotal ??= new();
      set => rptPaternityTotal = value;
    }

    /// <summary>
    /// A value of RptObligaitonTotal.
    /// </summary>
    [JsonPropertyName("rptObligaitonTotal")]
    public Common RptObligaitonTotal
    {
      get => rptObligaitonTotal ??= new();
      set => rptObligaitonTotal = value;
    }

    /// <summary>
    /// A value of RptEnforcementTotal.
    /// </summary>
    [JsonPropertyName("rptEnforcementTotal")]
    public Common RptEnforcementTotal
    {
      get => rptEnforcementTotal ??= new();
      set => rptEnforcementTotal = value;
    }

    /// <summary>
    /// A value of RptOfficeTotal.
    /// </summary>
    [JsonPropertyName("rptOfficeTotal")]
    public Common RptOfficeTotal
    {
      get => rptOfficeTotal ??= new();
      set => rptOfficeTotal = value;
    }

    /// <summary>
    /// A value of TotalRecordsWritten.
    /// </summary>
    [JsonPropertyName("totalRecordsWritten")]
    public Common TotalRecordsWritten
    {
      get => totalRecordsWritten ??= new();
      set => totalRecordsWritten = value;
    }

    /// <summary>
    /// A value of Legal.
    /// </summary>
    [JsonPropertyName("legal")]
    public ServiceProvider Legal
    {
      get => legal ??= new();
      set => legal = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of ColumnHeading.
    /// </summary>
    [JsonPropertyName("columnHeading")]
    public EabReportSend ColumnHeading
    {
      get => columnHeading ??= new();
      set => columnHeading = value;
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
    /// A value of Headers.
    /// </summary>
    [JsonPropertyName("headers")]
    public EabReportSend Headers
    {
      get => headers ??= new();
      set => headers = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of RecordsWritten.
    /// </summary>
    [JsonPropertyName("recordsWritten")]
    public Common RecordsWritten
    {
      get => recordsWritten ??= new();
      set => recordsWritten = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of FirstDayCurrentMonth.
    /// </summary>
    [JsonPropertyName("firstDayCurrentMonth")]
    public DateWorkArea FirstDayCurrentMonth
    {
      get => firstDayCurrentMonth ??= new();
      set => firstDayCurrentMonth = value;
    }

    /// <summary>
    /// A value of LastDayPrevMonth.
    /// </summary>
    [JsonPropertyName("lastDayPrevMonth")]
    public DateWorkArea LastDayPrevMonth
    {
      get => lastDayPrevMonth ??= new();
      set => lastDayPrevMonth = value;
    }

    private WorkArea name;
    private External external;
    private WorkArea date;
    private BatchTimestampWorkArea translate;
    private EabReportSend eabHeader;
    private Common placeCount;
    private WorkArea officeTotalWorkArea;
    private WorkArea enforacementTotal;
    private WorkArea obligationTotal;
    private WorkArea paternityTotal;
    private WorkArea locateTotal;
    private CaseFuncWorkSet caseFuncWorkSet;
    private ServiceProvider previousLegal;
    private ServiceProvider currentLegal;
    private Office previous;
    private Office current;
    private EabReportSend spaces;
    private Common fileNumber;
    private TextWorkArea refType;
    private TextWorkArea rsn;
    private Common endOfInputFile;
    private Common locateTotalOffice;
    private Common paternityTotalOffice;
    private Common enfTotalOffice;
    private Common obligationTotalOffice;
    private Common officeTotalCommon;
    private Common lspoLocateTotal;
    private Common lspoPaternityTotal;
    private Common lspoObligationTotal;
    private Common lspoEnfTotal;
    private Common lspoOfficeTotal;
    private Common rptLocateTotal;
    private Common rptPaternityTotal;
    private Common rptObligaitonTotal;
    private Common rptEnforcementTotal;
    private Common rptOfficeTotal;
    private Common totalRecordsWritten;
    private ServiceProvider legal;
    private Case1 case1;
    private LegalReferral legalReferral;
    private EabReportSend columnHeading;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea end;
    private DateWorkArea start;
    private EabReportSend headers;
    private Common firstRecord;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common recordsRead;
    private Common recordsWritten;
    private DateWorkArea dateWorkArea;
    private Common common;
    private DateWorkArea firstDayCurrentMonth;
    private DateWorkArea lastDayPrevMonth;
  }
#endregion
}
