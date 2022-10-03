// Program: LE_B599_REFERRAL_REPORTS, ID: 373429283, model: 746.
// Short name: SWEL599B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B599_REFERRAL_REPORTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB599ReferralReports: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B599_REFERRAL_REPORTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB599ReferralReports(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB599ReferralReports.
  /// </summary>
  public LeB599ReferralReports(IContext context, Import import, Export export):
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
    // 07/18/01  GVandy            WR10353/
    //                             WR10359    Initial Development
    // *********************************************************************
    // *********************************************************************
    // This procedure processes 2 input files.
    // The first file is read in its entirety and from the data contained
    // in the file the 'REFERRALS BY COLLECTION OFFICER TO
    // ATTORNEY/CONTRACTOR' report is created.
    // Following completion of the first file, the second file is
    // processed and the 'REFERRALS TO
    // ATTORNEY/CONTRACTOR' report is created.
    // *********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.DateMonth.RptDetail =
      "January  February March    April    May      June     July     August   SeptemberOctober  November December";
      
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
    // Note: This external performs actions for both input files.
    //       It is not necessary to pass a file number on the OPEN or CLOSE
    //       commands, as it OPENs or CLOSEs both files.  The file
    //       number must be passed on a READ command.
    // ***********************************************************************
    local.EabFileHandling.Action = "OPEN";
    UseLeEabReadReferralExtract2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening input file for 'le_eab_read_referral_extract'. Status = " +
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
    // *Call External to Open the Report 01.          *
    // ************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    local.EabReportSend.BlankLineAfterColHead = "Y";
    local.EabReportSend.RptHeading3 =
      "      REFERRALS BY COLLECTION OFFICER TO ATTORNEY/CONTRACTOR";
    UseCabBusinessReport2();

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
    local.End.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate,
      -(Day(local.ProgramProcessingInfo.ProcessDate) - 1));
    local.Start.Date = AddMonths(local.End.Date, -1);
    local.Spaces.RptDetail = "";
    local.Header.RptDetail = "During";
    local.Header.RptDetail = TrimEnd(local.Header.RptDetail) + " " + Substring
      (local.DateMonth.RptDetail, EabReportSend.RptDetail_MaxLength,
      Month(local.Start.Date) * 9 - 8, 9);
    local.Header.RptDetail = TrimEnd(local.Header.RptDetail) + ", " + NumberToString
      (Year(local.Start.Date), 12, 4);
    local.Header.RptDetail =
      Substring(local.Spaces.RptDetail, EabReportSend.RptDetail_MaxLength, 1,
      (80 - Length(TrimEnd(local.Header.RptDetail))) / 2) + TrimEnd
      (local.Header.RptDetail);

    // -- Write header to report 1.
    local.EabFileHandling.Action = "WRITE";
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

    // ************************************************
    // *Call External to Open the Report 02.          *
    // ************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    local.EabReportSend.BlankLineAfterColHead = "Y";
    local.EabReportSend.RptHeading3 =
      "                REFERRALS TO ATTORNEY/CONTRACTOR";
    UseCabBusinessReport6();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening output file for 'cab_business_report_02'";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -- Write header to report 2.
    local.EabFileHandling.Action = "WRITE";
    UseCabBusinessReport8();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing output file for 'cab_business_report_02'";
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
    UseCabBusinessReport7();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error writing output file for 'cab_business_report_02'";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_WRITE_ERROR_RB";

      return;
    }

    // -- Write Report 1
    local.FirstRecord.Flag = "Y";

    do
    {
      local.EabFileHandling.Action = "READ";

      // -- The eab reads either input file depending upon the file number that 
      // is sent (1 or 2).
      local.FileNumber.Count = 1;
      UseLeEabReadReferralExtract1();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
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
            "Error reading input file for 'le_eab_referral_extract'.";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
      }

      // If the Office, Supervisor, or Collection officer has changed then write
      // the totals for the collection officer, reset the counters, and page
      // break.
      if (local.CoOffice.SystemGeneratedId != local
        .PrevCoOffice.SystemGeneratedId || !
        Equal(local.Supervisor.FirstName, local.PrevSupervisor.FirstName) || AsChar
        (local.Supervisor.MiddleInitial) != AsChar
        (local.PrevSupervisor.MiddleInitial) || !
        Equal(local.Supervisor.LastName, local.PrevSupervisor.LastName) || !
        Equal(local.CoServiceProvider.FirstName,
        local.PrevCoServiceProvider.FirstName) || AsChar
        (local.CoServiceProvider.MiddleInitial) != AsChar
        (local.PrevCoServiceProvider.MiddleInitial) || !
        Equal(local.CoServiceProvider.LastName,
        local.PrevCoServiceProvider.LastName) || AsChar
        (local.EndOfInputFile.Flag) == 'Y')
      {
        if (AsChar(local.FirstRecord.Flag) == 'Y')
        {
          local.FirstRecord.Flag = "N";
        }
        else
        {
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

          // -- Write the summary count header.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "                    ENF         EST         ADM         Total by Status";
            
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
          local.EabReportSend.RptDetail =
            "                    ---         ---         ---         ---------------";
            
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

          // -- Write the number of Sent referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfSent.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfSent.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Sent           " + local
            .Number.Text8;

          if (local.EstSent.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstSent.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmSent.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmSent.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.SentTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.SentTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "                " + local
            .Number.Text8;
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

          // -- Write the number of Open referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfOpen.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfOpen.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Open           " + local
            .Number.Text8;

          if (local.EstOpen.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstOpen.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmOpen.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmOpen.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.OpenTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.OpenTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "                " + local
            .Number.Text8;
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

          // -- Write the number of Rejected referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfRejected.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfRejected.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Rejected       " + local
            .Number.Text8;

          if (local.EstRejected.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstRejected.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmRejected.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmRejected.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.RejectedTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 =
              NumberToString(local.RejectedTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "                " + local
            .Number.Text8;
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

          // -- Write the number of Withdrawn referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfWithdrawn.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfWithdrawn.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Withdrawn      " + local
            .Number.Text8;

          if (local.EstWithdrawn.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstWithdrawn.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmWithdrawn.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmWithdrawn.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.WithdrawnTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 =
              NumberToString(local.WithdrawnTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "                " + local
            .Number.Text8;
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

          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Total by";
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

          // -- Write the Total number of referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Referral Type  " + local
            .Number.Text8;

          if (local.EstTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;
          local.GrandTotal.Count = local.AdmTotal.Count + local
            .EnfTotal.Count + local.EstTotal.Count;

          if (local.GrandTotal.Count == 0)
          {
            local.Number.Text8 = "0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.GrandTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Number.Text8, local.CountLength.Count, 9 -
              local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "         Total Referrals " +
            local.Number.Text8;
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

          if (AsChar(local.EndOfInputFile.Flag) == 'Y')
          {
            continue;
          }

          // -- Re-initialize all the counters.
          local.AdmSent.Count = 0;
          local.AdmOpen.Count = 0;
          local.AdmRejected.Count = 0;
          local.AdmWithdrawn.Count = 0;
          local.AdmTotal.Count = 0;
          local.EnfSent.Count = 0;
          local.EnfOpen.Count = 0;
          local.EnfRejected.Count = 0;
          local.EnfWithdrawn.Count = 0;
          local.EnfTotal.Count = 0;
          local.EstSent.Count = 0;
          local.EstOpen.Count = 0;
          local.EstRejected.Count = 0;
          local.EstWithdrawn.Count = 0;
          local.EstTotal.Count = 0;
          local.SentTotal.Count = 0;
          local.OpenTotal.Count = 0;
          local.RejectedTotal.Count = 0;
          local.WithdrawnTotal.Count = 0;

          // -- Page Break.
          local.EabFileHandling.Action = "NEWPAGE";
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

          // -- Write the header on the new page.
          local.EabFileHandling.Action = "WRITE";
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

        // -- Write the office name and number.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Office:            " + TrimEnd
          (local.CoOffice.Name) + " " + NumberToString
          (local.CoOffice.SystemGeneratedId, 13, 3);
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

        // -- Write the Supervisor name (if provided).
        local.EabFileHandling.Action = "WRITE";

        if (IsEmpty(local.Supervisor.FirstName) && IsEmpty
          (local.Supervisor.MiddleInitial) && IsEmpty
          (local.Supervisor.LastName))
        {
          local.EabReportSend.RptDetail = "Supervisor:";
        }
        else
        {
          local.EabReportSend.RptDetail = "Supervisor:        " + local
            .Supervisor.LastName;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + ", " + local
            .Supervisor.FirstName;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + " " + local
            .Supervisor.MiddleInitial;
        }

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

        // -- Write the Collection Officer name.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Collection Officer:       " + local
          .CoServiceProvider.LastName;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + ", " + local
          .CoServiceProvider.FirstName;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + " " + local
          .CoServiceProvider.MiddleInitial;
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

        local.PrevLegal.Assign(local.NullServiceProvider);
      }

      // If the Attorney has changed then write the attorney name and the AP 
      // name, case #, Date Sent, etc. header.
      if (!Equal(local.Legal.FirstName, local.PrevLegal.FirstName) || AsChar
        (local.Legal.MiddleInitial) != AsChar
        (local.PrevLegal.MiddleInitial) || !
        Equal(local.Legal.LastName, local.PrevLegal.LastName))
      {
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

        // -- Write the Attorney name.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Legal Service Provider:   " + local
          .Legal.LastName;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + ", " + local
          .Legal.FirstName;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + " " + local
          .Legal.MiddleInitial;
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
        local.EabReportSend.RptDetail =
          "                                                                    Ref";
          
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

        // -- Write the referral header information.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "AP Name                       Case #     Date Sent  RSN             Type Status";
          
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

      // -- Create the list of referral reasons.
      local.Rsn.Text30 = "";

      if (!IsEmpty(local.LegalReferral.ReferralReason1))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason1) + ",";
      }

      if (!IsEmpty(local.LegalReferral.ReferralReason2))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason2) + ",";
      }

      if (!IsEmpty(local.LegalReferral.ReferralReason3))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason3) + ",";
      }

      if (!IsEmpty(local.LegalReferral.ReferralReason4))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason4) + ",";
      }

      if (!IsEmpty(local.LegalReferral.ReferralReason5))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason5) + ",";
      }

      local.RsnLength.Count = Length(TrimEnd(local.Rsn.Text30));

      if (local.RsnLength.Count > 0)
      {
        // -- Remove the "," from the end of the list.
        local.Rsn.Text30 =
          Substring(local.Rsn.Text30, 1, local.RsnLength.Count - 1);
      }

      // -- Determine the Referral Type for the report.  PAT referral reason is 
      // reported as EST.
      // If there is more than one reason on the referral then:
      // 	ADM has the highest priority.
      // 	EST is the second highest priority.
      // 	ENF is the lowest priority.
      if (Find(local.Rsn.Text30, "ADM") > 0)
      {
        local.RefType.Text4 = "ADM";
      }
      else if (Find(local.Rsn.Text30, "EST") > 0)
      {
        local.RefType.Text4 = "EST";
      }
      else if (Find(local.Rsn.Text30, "PAT") > 0)
      {
        local.RefType.Text4 = "EST";
      }
      else if (Find(local.Rsn.Text30, "ENF") > 0)
      {
        local.RefType.Text4 = "ENF";
      }

      // -- Increment the appropriate counters.
      switch(TrimEnd(TrimEnd(local.RefType.Text4)))
      {
        case "ENF":
          ++local.EnfTotal.Count;

          switch(AsChar(local.LegalReferral.Status))
          {
            case 'S':
              ++local.EnfSent.Count;

              break;
            case 'O':
              ++local.EnfOpen.Count;

              break;
            case 'W':
              ++local.EnfWithdrawn.Count;

              break;
            case 'R':
              ++local.EnfRejected.Count;

              break;
            default:
              break;
          }

          break;
        case "EST":
          ++local.EstTotal.Count;

          switch(AsChar(local.LegalReferral.Status))
          {
            case 'S':
              ++local.EstSent.Count;

              break;
            case 'O':
              ++local.EstOpen.Count;

              break;
            case 'W':
              ++local.EstWithdrawn.Count;

              break;
            case 'R':
              ++local.EstRejected.Count;

              break;
            default:
              break;
          }

          break;
        case "ADM":
          ++local.AdmTotal.Count;

          switch(AsChar(local.LegalReferral.Status))
          {
            case 'S':
              ++local.AdmSent.Count;

              break;
            case 'O':
              ++local.AdmOpen.Count;

              break;
            case 'W':
              ++local.AdmWithdrawn.Count;

              break;
            case 'R':
              ++local.AdmRejected.Count;

              break;
            default:
              break;
          }

          break;
        default:
          break;
      }

      if (!IsEmpty(local.RefType.Text4))
      {
        switch(AsChar(local.LegalReferral.Status))
        {
          case 'S':
            ++local.SentTotal.Count;

            break;
          case 'O':
            ++local.OpenTotal.Count;

            break;
          case 'W':
            ++local.WithdrawnTotal.Count;

            break;
          case 'R':
            ++local.RejectedTotal.Count;

            break;
          default:
            break;
        }
      }

      // -- Write the referral detail info.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        Substring(local.CsePersonsWorkSet.FormattedName,
        CsePersonsWorkSet.FormattedName_MaxLength, 1, 29) + " " + local
        .Case1.Number;
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + " " +
        NumberToString(Month(local.LegalReferral.ReferralDate), 14, 2);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "/"
        + NumberToString(Day(local.LegalReferral.ReferralDate), 14, 2);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "/"
        + NumberToString(Year(local.LegalReferral.ReferralDate), 12, 4);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + " " +
        Substring(local.Rsn.Text30, TextWorkArea.Text30_MaxLength, 1, 15) + " " +
        local.RefType.Text4;
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "  " +
        (local.LegalReferral.Status ?? "");
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

      MoveOffice(local.CoOffice, local.PrevCoOffice);
      local.PrevCoServiceProvider.Assign(local.CoServiceProvider);
      local.PrevLegal.Assign(local.Legal);
      local.PrevSupervisor.Assign(local.Supervisor);
      ++local.RecordsWritten.Count;
    }
    while(AsChar(local.EndOfInputFile.Flag) != 'Y');

    // -- Write the control report record counts.
    local.EabReportSend.RptDetail =
      "Report 1 - Total Number Of Records Read :     " + NumberToString
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

    local.EabReportSend.RptDetail =
      "Report 1 - Total Number Of Records Written :  " + NumberToString
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

    // -- Re-initialize all the counters.
    local.AdmSent.Count = 0;
    local.AdmOpen.Count = 0;
    local.AdmRejected.Count = 0;
    local.AdmWithdrawn.Count = 0;
    local.AdmTotal.Count = 0;
    local.EnfSent.Count = 0;
    local.EnfOpen.Count = 0;
    local.EnfRejected.Count = 0;
    local.EnfWithdrawn.Count = 0;
    local.EnfTotal.Count = 0;
    local.EstSent.Count = 0;
    local.EstOpen.Count = 0;
    local.EstRejected.Count = 0;
    local.EstWithdrawn.Count = 0;
    local.EstTotal.Count = 0;
    local.SentTotal.Count = 0;
    local.OpenTotal.Count = 0;
    local.RejectedTotal.Count = 0;
    local.WithdrawnTotal.Count = 0;

    // -- Write Report 2
    local.FirstRecord.Flag = "Y";
    local.EndOfInputFile.Flag = "N";
    local.RecordsRead.Count = 0;
    local.RecordsWritten.Count = 0;

    do
    {
      local.EabFileHandling.Action = "READ";

      // -- The eab reads either input file depending upon the file number that 
      // is sent (1 or 2).
      local.FileNumber.Count = 2;
      UseLeEabReadReferralExtract1();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
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
            "Error reading input file for 'le_eab_referral_extract'.";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_READ_ERROR_WITH_RB";

          return;
      }

      // If the Area Office or Attorney have changed then write the totals for 
      // the Attorney, reset the counters, and page break.
      if (!Equal(local.Area.Name, local.PrevArea.Name) || !
        Equal(local.Legal.FirstName, local.PrevLegal.FirstName) || AsChar
        (local.Legal.MiddleInitial) != AsChar
        (local.PrevLegal.MiddleInitial) || !
        Equal(local.Legal.LastName, local.PrevLegal.LastName) || AsChar
        (local.EndOfInputFile.Flag) == 'Y')
      {
        if (AsChar(local.FirstRecord.Flag) == 'Y')
        {
          local.FirstRecord.Flag = "N";
        }
        else
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "";
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          // -- Write the summary count header.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "                    ENF         EST         ADM         Total by Status";
            
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
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
            "                    ---         ---         ---         ---------------";
            
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          // -- Write the number of Sent referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfSent.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfSent.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Sent           " + local
            .Number.Text8;

          if (local.EstSent.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstSent.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmSent.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmSent.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.SentTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.SentTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "                " + local
            .Number.Text8;
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          // -- Write the number of Open referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfOpen.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfOpen.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Open           " + local
            .Number.Text8;

          if (local.EstOpen.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstOpen.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmOpen.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmOpen.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.OpenTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.OpenTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "                " + local
            .Number.Text8;
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          // -- Write the number of Rejected referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfRejected.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfRejected.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Rejected       " + local
            .Number.Text8;

          if (local.EstRejected.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstRejected.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmRejected.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmRejected.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.RejectedTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 =
              NumberToString(local.RejectedTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "                " + local
            .Number.Text8;
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          // -- Write the number of Withdrawn referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfWithdrawn.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfWithdrawn.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Withdrawn      " + local
            .Number.Text8;

          if (local.EstWithdrawn.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstWithdrawn.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmWithdrawn.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmWithdrawn.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.WithdrawnTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 =
              NumberToString(local.WithdrawnTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "                " + local
            .Number.Text8;
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
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
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
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
          local.EabReportSend.RptDetail = "Total by";
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          // -- Write the Total number of referrals.
          local.EabFileHandling.Action = "WRITE";

          if (local.EnfTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EnfTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail = "Referral Type  " + local
            .Number.Text8;

          if (local.EstTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.EstTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;

          if (local.AdmTotal.Count == 0)
          {
            local.Number.Text8 = "       0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.AdmTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Spaces.RptDetail,
              EabReportSend.RptDetail_MaxLength, 1, local.CountLength.Count -
              1) + Substring
              (local.Number.Text8, TextWorkArea.Text8_MaxLength,
              local.CountLength.Count, 9 - local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "    " + local
            .Number.Text8;
          local.GrandTotal.Count = local.AdmTotal.Count + local
            .EnfTotal.Count + local.EstTotal.Count;

          if (local.GrandTotal.Count == 0)
          {
            local.Number.Text8 = "0";
          }
          else
          {
            // -- Remove leading zeros and right justify the count.
            local.Number.Text8 = NumberToString(local.GrandTotal.Count, 8, 8);
            local.CountLength.Count = Verify(local.Number.Text8, "0");
            local.Number.Text8 =
              Substring(local.Number.Text8, local.CountLength.Count, 9 -
              local.CountLength.Count);
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "         Total Referrals " +
            local.Number.Text8;
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          if (AsChar(local.EndOfInputFile.Flag) == 'Y')
          {
            continue;
          }

          // -- Re-initialize all the counters.
          local.AdmSent.Count = 0;
          local.AdmOpen.Count = 0;
          local.AdmRejected.Count = 0;
          local.AdmWithdrawn.Count = 0;
          local.AdmTotal.Count = 0;
          local.EnfSent.Count = 0;
          local.EnfOpen.Count = 0;
          local.EnfRejected.Count = 0;
          local.EnfWithdrawn.Count = 0;
          local.EnfTotal.Count = 0;
          local.EstSent.Count = 0;
          local.EstOpen.Count = 0;
          local.EstRejected.Count = 0;
          local.EstWithdrawn.Count = 0;
          local.EstTotal.Count = 0;
          local.SentTotal.Count = 0;
          local.OpenTotal.Count = 0;
          local.RejectedTotal.Count = 0;
          local.WithdrawnTotal.Count = 0;

          // -- Page Break.
          local.EabFileHandling.Action = "NEWPAGE";
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "FILE_WRITE_ERROR_RB";

            return;
          }

          // -- Write the header on the new page.
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport8();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
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
          UseCabBusinessReport7();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error writing output file for 'cab_business_report_02'";
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

        // -- Write the area office name.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Area Office:       " + local.Area.Name;
        UseCabBusinessReport7();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_02'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        // -- Write the attorney name.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Attorney:          " + local
          .Legal.LastName;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + ", " + local
          .Legal.FirstName;
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + " " + local
          .Legal.MiddleInitial;
        UseCabBusinessReport7();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_02'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        MoveOffice(local.NullOffice, local.PrevCoOffice);
      }

      // If the CO Office has changed then write the CO Office name and number 
      // and the AP name, case #, Date Sent, etc. header.
      if (!Equal(local.CoOffice.Name, local.PrevCoOffice.Name) || local
        .CoOffice.SystemGeneratedId != local.PrevCoOffice.SystemGeneratedId)
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "";
        UseCabBusinessReport7();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_02'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        // -- Write the CO office name and number.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Office:            " + TrimEnd
          (local.CoOffice.Name) + " " + NumberToString
          (local.CoOffice.SystemGeneratedId, 13, 3);
        UseCabBusinessReport7();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_02'";
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
          "                                                                    Ref";
          
        UseCabBusinessReport7();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_02'";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "FILE_WRITE_ERROR_RB";

          return;
        }

        // -- Write the referral header information.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "AP Name                       Case #     Date Sent  RSN             Type Status";
          
        UseCabBusinessReport7();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error writing output file for 'cab_business_report_02'";
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

      // -- Create the list of referral reasons.
      local.Rsn.Text30 = "";

      if (!IsEmpty(local.LegalReferral.ReferralReason1))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason1) + ",";
      }

      if (!IsEmpty(local.LegalReferral.ReferralReason2))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason2) + ",";
      }

      if (!IsEmpty(local.LegalReferral.ReferralReason3))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason3) + ",";
      }

      if (!IsEmpty(local.LegalReferral.ReferralReason4))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason4) + ",";
      }

      if (!IsEmpty(local.LegalReferral.ReferralReason5))
      {
        local.Rsn.Text30 = TrimEnd(local.Rsn.Text30) + TrimEnd
          (local.LegalReferral.ReferralReason5) + ",";
      }

      local.RsnLength.Count = Length(TrimEnd(local.Rsn.Text30));

      if (local.RsnLength.Count > 0)
      {
        // -- Remove the "," from the end of the list.
        local.Rsn.Text30 =
          Substring(local.Rsn.Text30, 1, local.RsnLength.Count - 1);
      }

      // -- Determine the Referral Type for the report.  PAT referral reason is 
      // reported as EST.
      // If there is more than one reason on the referral then:
      // 	ADM has the highest priority.
      // 	EST is the second highest priority.
      // 	ENF is the lowest priority.
      if (Find(local.Rsn.Text30, "ADM") > 0)
      {
        local.RefType.Text4 = "ADM";
      }
      else if (Find(local.Rsn.Text30, "EST") > 0)
      {
        local.RefType.Text4 = "EST";
      }
      else if (Find(local.Rsn.Text30, "PAT") > 0)
      {
        local.RefType.Text4 = "EST";
      }
      else if (Find(local.Rsn.Text30, "ENF") > 0)
      {
        local.RefType.Text4 = "ENF";
      }

      // -- Increment the appropriate counters.
      switch(TrimEnd(TrimEnd(local.RefType.Text4)))
      {
        case "ENF":
          ++local.EnfTotal.Count;

          switch(AsChar(local.LegalReferral.Status))
          {
            case 'S':
              ++local.EnfSent.Count;

              break;
            case 'O':
              ++local.EnfOpen.Count;

              break;
            case 'W':
              ++local.EnfWithdrawn.Count;

              break;
            case 'R':
              ++local.EnfRejected.Count;

              break;
            default:
              break;
          }

          break;
        case "EST":
          ++local.EstTotal.Count;

          switch(AsChar(local.LegalReferral.Status))
          {
            case 'S':
              ++local.EstSent.Count;

              break;
            case 'O':
              ++local.EstOpen.Count;

              break;
            case 'W':
              ++local.EstWithdrawn.Count;

              break;
            case 'R':
              ++local.EstRejected.Count;

              break;
            default:
              break;
          }

          break;
        case "ADM":
          ++local.AdmTotal.Count;

          switch(AsChar(local.LegalReferral.Status))
          {
            case 'S':
              ++local.AdmSent.Count;

              break;
            case 'O':
              ++local.AdmOpen.Count;

              break;
            case 'W':
              ++local.AdmWithdrawn.Count;

              break;
            case 'R':
              ++local.AdmRejected.Count;

              break;
            default:
              break;
          }

          break;
        default:
          break;
      }

      if (!IsEmpty(local.RefType.Text4))
      {
        switch(AsChar(local.LegalReferral.Status))
        {
          case 'S':
            ++local.SentTotal.Count;

            break;
          case 'O':
            ++local.OpenTotal.Count;

            break;
          case 'W':
            ++local.WithdrawnTotal.Count;

            break;
          case 'R':
            ++local.RejectedTotal.Count;

            break;
          default:
            break;
        }
      }

      // -- Write the referral detail info.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        Substring(local.CsePersonsWorkSet.FormattedName,
        CsePersonsWorkSet.FormattedName_MaxLength, 1, 29) + " " + local
        .Case1.Number;
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + " " +
        NumberToString(Month(local.LegalReferral.ReferralDate), 14, 2);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "/"
        + NumberToString(Day(local.LegalReferral.ReferralDate), 14, 2);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "/"
        + NumberToString(Year(local.LegalReferral.ReferralDate), 12, 4);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + " " +
        Substring(local.Rsn.Text30, TextWorkArea.Text30_MaxLength, 1, 15) + " " +
        local.RefType.Text4;
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "  " +
        (local.LegalReferral.Status ?? "");
      UseCabBusinessReport7();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error writing output file for 'cab_business_report_02'";
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "FILE_WRITE_ERROR_RB";

        return;
      }

      MoveOffice(local.CoOffice, local.PrevCoOffice);
      local.PrevLegal.Assign(local.Legal);
      local.PrevArea.Name = local.Area.Name;
      ++local.RecordsWritten.Count;
    }
    while(AsChar(local.EndOfInputFile.Flag) != 'Y');

    // -- Write the control report record counts.
    local.EabReportSend.RptDetail =
      "Report 2 - Total Number Of Records Read :     " + NumberToString
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

    local.EabReportSend.RptDetail =
      "Report 2 - Total Number Of Records Written :  " + NumberToString
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
    local.EabFileHandling.Action = "CLOSE";
    UseLeEabReadReferralExtract2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing input file for 'le_eab_read_referral_extract'.";
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
    // * Close Report 1.
    // 
    // *
    // ************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file for 'cab_business_report_01'.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "FILE_OPEN_ERROR";

      return;
    }

    // ************************************************
    // * Close Report 2.
    // 
    // *
    // ************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport5();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file for 'cab_business_report_02'.";
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
  }

  private static void MoveEabReportSend2(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveLegalReferral(LegalReferral source,
    LegalReferral target)
  {
    target.ReferralDate = source.ReferralDate;
    target.ReferralReason1 = source.ReferralReason1;
    target.ReferralReason2 = source.ReferralReason2;
    target.ReferralReason3 = source.ReferralReason3;
    target.ReferralReason5 = source.ReferralReason5;
    target.ReferralReason4 = source.ReferralReason4;
    target.Status = source.Status;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

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

  private void UseCabBusinessReport4()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.Header.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport5()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport6()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport7()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport02.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport8()
  {
    var useImport = new CabBusinessReport02.Import();
    var useExport = new CabBusinessReport02.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.Header.RptDetail;

    Call(CabBusinessReport02.Execute, useImport, useExport);

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

  private void UseLeEabReadReferralExtract1()
  {
    var useImport = new LeEabReadReferralExtract.Import();
    var useExport = new LeEabReadReferralExtract.Export();

    useImport.FileNumber.Count = local.FileNumber.Count;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.CsePersonsWorkSet.FormattedName =
      local.CsePersonsWorkSet.FormattedName;
    MoveLegalReferral(local.LegalReferral, useExport.LegalReferral);
    useExport.Case1.Number = local.Case1.Number;
    useExport.Legal.Assign(local.Legal);
    useExport.CoServiceProvider.Assign(local.CoServiceProvider);
    useExport.Supervisor.Assign(local.Supervisor);
    MoveOffice(local.CoOffice, useExport.CoOffice);
    useExport.Area.Name = local.Area.Name;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadReferralExtract.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
    local.LegalReferral.Assign(useExport.LegalReferral);
    local.Case1.Number = useExport.Case1.Number;
    local.Legal.Assign(useExport.Legal);
    local.CoServiceProvider.Assign(useExport.CoServiceProvider);
    local.Supervisor.Assign(useExport.Supervisor);
    MoveOffice(useExport.CoOffice, local.CoOffice);
    local.Area.Name = useExport.Area.Name;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeEabReadReferralExtract2()
  {
    var useImport = new LeEabReadReferralExtract.Import();
    var useExport = new LeEabReadReferralExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabReadReferralExtract.Execute, useImport, useExport);

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
    /// A value of NullOffice.
    /// </summary>
    [JsonPropertyName("nullOffice")]
    public Office NullOffice
    {
      get => nullOffice ??= new();
      set => nullOffice = value;
    }

    /// <summary>
    /// A value of PrevArea.
    /// </summary>
    [JsonPropertyName("prevArea")]
    public Office PrevArea
    {
      get => prevArea ??= new();
      set => prevArea = value;
    }

    /// <summary>
    /// A value of GrandTotal.
    /// </summary>
    [JsonPropertyName("grandTotal")]
    public Common GrandTotal
    {
      get => grandTotal ??= new();
      set => grandTotal = value;
    }

    /// <summary>
    /// A value of CountLength.
    /// </summary>
    [JsonPropertyName("countLength")]
    public Common CountLength
    {
      get => countLength ??= new();
      set => countLength = value;
    }

    /// <summary>
    /// A value of Number.
    /// </summary>
    [JsonPropertyName("number")]
    public TextWorkArea Number
    {
      get => number ??= new();
      set => number = value;
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
    /// A value of RsnLength.
    /// </summary>
    [JsonPropertyName("rsnLength")]
    public Common RsnLength
    {
      get => rsnLength ??= new();
      set => rsnLength = value;
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
    /// A value of NullServiceProvider.
    /// </summary>
    [JsonPropertyName("nullServiceProvider")]
    public ServiceProvider NullServiceProvider
    {
      get => nullServiceProvider ??= new();
      set => nullServiceProvider = value;
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
    /// A value of AdmTotal.
    /// </summary>
    [JsonPropertyName("admTotal")]
    public Common AdmTotal
    {
      get => admTotal ??= new();
      set => admTotal = value;
    }

    /// <summary>
    /// A value of EstTotal.
    /// </summary>
    [JsonPropertyName("estTotal")]
    public Common EstTotal
    {
      get => estTotal ??= new();
      set => estTotal = value;
    }

    /// <summary>
    /// A value of EnfTotal.
    /// </summary>
    [JsonPropertyName("enfTotal")]
    public Common EnfTotal
    {
      get => enfTotal ??= new();
      set => enfTotal = value;
    }

    /// <summary>
    /// A value of WithdrawnTotal.
    /// </summary>
    [JsonPropertyName("withdrawnTotal")]
    public Common WithdrawnTotal
    {
      get => withdrawnTotal ??= new();
      set => withdrawnTotal = value;
    }

    /// <summary>
    /// A value of RejectedTotal.
    /// </summary>
    [JsonPropertyName("rejectedTotal")]
    public Common RejectedTotal
    {
      get => rejectedTotal ??= new();
      set => rejectedTotal = value;
    }

    /// <summary>
    /// A value of OpenTotal.
    /// </summary>
    [JsonPropertyName("openTotal")]
    public Common OpenTotal
    {
      get => openTotal ??= new();
      set => openTotal = value;
    }

    /// <summary>
    /// A value of SentTotal.
    /// </summary>
    [JsonPropertyName("sentTotal")]
    public Common SentTotal
    {
      get => sentTotal ??= new();
      set => sentTotal = value;
    }

    /// <summary>
    /// A value of AdmWithdrawn.
    /// </summary>
    [JsonPropertyName("admWithdrawn")]
    public Common AdmWithdrawn
    {
      get => admWithdrawn ??= new();
      set => admWithdrawn = value;
    }

    /// <summary>
    /// A value of AdmRejected.
    /// </summary>
    [JsonPropertyName("admRejected")]
    public Common AdmRejected
    {
      get => admRejected ??= new();
      set => admRejected = value;
    }

    /// <summary>
    /// A value of AdmOpen.
    /// </summary>
    [JsonPropertyName("admOpen")]
    public Common AdmOpen
    {
      get => admOpen ??= new();
      set => admOpen = value;
    }

    /// <summary>
    /// A value of AdmSent.
    /// </summary>
    [JsonPropertyName("admSent")]
    public Common AdmSent
    {
      get => admSent ??= new();
      set => admSent = value;
    }

    /// <summary>
    /// A value of EstWithdrawn.
    /// </summary>
    [JsonPropertyName("estWithdrawn")]
    public Common EstWithdrawn
    {
      get => estWithdrawn ??= new();
      set => estWithdrawn = value;
    }

    /// <summary>
    /// A value of EstRejected.
    /// </summary>
    [JsonPropertyName("estRejected")]
    public Common EstRejected
    {
      get => estRejected ??= new();
      set => estRejected = value;
    }

    /// <summary>
    /// A value of EstOpen.
    /// </summary>
    [JsonPropertyName("estOpen")]
    public Common EstOpen
    {
      get => estOpen ??= new();
      set => estOpen = value;
    }

    /// <summary>
    /// A value of EstSent.
    /// </summary>
    [JsonPropertyName("estSent")]
    public Common EstSent
    {
      get => estSent ??= new();
      set => estSent = value;
    }

    /// <summary>
    /// A value of EnfWithdrawn.
    /// </summary>
    [JsonPropertyName("enfWithdrawn")]
    public Common EnfWithdrawn
    {
      get => enfWithdrawn ??= new();
      set => enfWithdrawn = value;
    }

    /// <summary>
    /// A value of EnfRejected.
    /// </summary>
    [JsonPropertyName("enfRejected")]
    public Common EnfRejected
    {
      get => enfRejected ??= new();
      set => enfRejected = value;
    }

    /// <summary>
    /// A value of EnfOpen.
    /// </summary>
    [JsonPropertyName("enfOpen")]
    public Common EnfOpen
    {
      get => enfOpen ??= new();
      set => enfOpen = value;
    }

    /// <summary>
    /// A value of EnfSent.
    /// </summary>
    [JsonPropertyName("enfSent")]
    public Common EnfSent
    {
      get => enfSent ??= new();
      set => enfSent = value;
    }

    /// <summary>
    /// A value of PrevCoOffice.
    /// </summary>
    [JsonPropertyName("prevCoOffice")]
    public Office PrevCoOffice
    {
      get => prevCoOffice ??= new();
      set => prevCoOffice = value;
    }

    /// <summary>
    /// A value of PrevSupervisor.
    /// </summary>
    [JsonPropertyName("prevSupervisor")]
    public ServiceProvider PrevSupervisor
    {
      get => prevSupervisor ??= new();
      set => prevSupervisor = value;
    }

    /// <summary>
    /// A value of PrevCoServiceProvider.
    /// </summary>
    [JsonPropertyName("prevCoServiceProvider")]
    public ServiceProvider PrevCoServiceProvider
    {
      get => prevCoServiceProvider ??= new();
      set => prevCoServiceProvider = value;
    }

    /// <summary>
    /// A value of PrevLegal.
    /// </summary>
    [JsonPropertyName("prevLegal")]
    public ServiceProvider PrevLegal
    {
      get => prevLegal ??= new();
      set => prevLegal = value;
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
    /// A value of CoOffice.
    /// </summary>
    [JsonPropertyName("coOffice")]
    public Office CoOffice
    {
      get => coOffice ??= new();
      set => coOffice = value;
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
    /// A value of CoServiceProvider.
    /// </summary>
    [JsonPropertyName("coServiceProvider")]
    public ServiceProvider CoServiceProvider
    {
      get => coServiceProvider ??= new();
      set => coServiceProvider = value;
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
    /// A value of DateMonth.
    /// </summary>
    [JsonPropertyName("dateMonth")]
    public EabReportSend DateMonth
    {
      get => dateMonth ??= new();
      set => dateMonth = value;
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

    private Office nullOffice;
    private Office prevArea;
    private Common grandTotal;
    private Common countLength;
    private TextWorkArea number;
    private EabReportSend spaces;
    private Common fileNumber;
    private Common rsnLength;
    private TextWorkArea refType;
    private TextWorkArea rsn;
    private ServiceProvider nullServiceProvider;
    private Common endOfInputFile;
    private Common admTotal;
    private Common estTotal;
    private Common enfTotal;
    private Common withdrawnTotal;
    private Common rejectedTotal;
    private Common openTotal;
    private Common sentTotal;
    private Common admWithdrawn;
    private Common admRejected;
    private Common admOpen;
    private Common admSent;
    private Common estWithdrawn;
    private Common estRejected;
    private Common estOpen;
    private Common estSent;
    private Common enfWithdrawn;
    private Common enfRejected;
    private Common enfOpen;
    private Common enfSent;
    private Office prevCoOffice;
    private ServiceProvider prevSupervisor;
    private ServiceProvider prevCoServiceProvider;
    private ServiceProvider prevLegal;
    private Office area;
    private Office coOffice;
    private ServiceProvider supervisor;
    private ServiceProvider coServiceProvider;
    private ServiceProvider legal;
    private Case1 case1;
    private LegalReferral legalReferral;
    private EabReportSend dateMonth;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea end;
    private DateWorkArea start;
    private EabReportSend header;
    private Common firstRecord;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common recordsRead;
    private Common recordsWritten;
  }
#endregion
}
