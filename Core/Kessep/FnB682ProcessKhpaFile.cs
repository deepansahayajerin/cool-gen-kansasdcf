// Program: FN_B682_PROCESS_KHPA_FILE, ID: 374579046, model: 746.
// Short name: SWEF682B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B682_PROCESS_KHPA_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB682ProcessKhpaFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B682_PROCESS_KHPA_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB682ProcessKhpaFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB682ProcessKhpaFile.
  /// </summary>
  public FnB682ProcessKhpaFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 07/23/2010  DDupree	PR17035	Initial Development.  Process file from KHPA,
    // trying to find health coverage for children on a CSE Case.
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  This program process the file from KHPA with processing policy info 
    // from a query from CSE. The policy info will be used to identify people
    // who are covered by the policy info sent by KHPA and trying to match them
    // as being on a open CSE case. Once a person has been identified as being
    // on a open cse case then they will be written to a report that CSE
    // business people than can then work.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Read.Action = "READ";
    local.WriteEabFileHandling.Action = "WRITE";
    local.Close.Action = "CLOSE";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  General Housekeeping and Initializations.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB682BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (!IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Initialization Cab Error..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // -- Write new header info.
    local.EabFileHandling.Action = "WRITE";
    local.End.Date = AddDays(local.ProgramProcessingInfo.ProcessDate, -40);
    local.Start.Date = AddDays(local.End.Date, -6);
    local.EabReportSend.BlankLineAfterHeading = "Y";
    local.Record.Count = 0;
    local.ReadCount.Count = 0;
    local.CoverPersonCount.Count = 0;

    do
    {
      // -------------------------------------------------------------------------------------------------------------------------
      // --  Get a record from the sorted/summed extract file.
      // --
      // --  Note that the external can return more data than what we actually 
      // need.  Not all views need to be returned for what we're doing here.
      // -------------------------------------------------------------------------------------------------------------------------
      UseFnB682ProcessKhpaPolicyfile1();

      if (!Equal(local.Main.TextReturnCode, "00") && !
        Equal(local.Main.TextReturnCode, "EF"))
      {
        // --  write to error file...
        local.EabReportSend.RptDetail =
          "(01) Error reading extract file...  Returned Status = " + local
          .External.TextReturnCode;
        UseCabErrorReport1();
        ExitState = "ERROR_READING_FILE_AB";

        return;
      }

      ++local.Record.Count;

      if (IsEmpty(local.CoverPersonBenfitNumb.Text12))
      {
        break;
      }

      ++local.ReadCount.Count;
      local.CsePerson.Number =
        Substring(local.CoverPersonBenfitNumb.Text12, 4, 9);
      UseCabZeroFillNumber();
      local.CoveredPersonNumb.Number = local.CsePerson.Number;

      if (!ReadCase())
      {
        // the covered person is not on a open cse case at this time
        continue;
      }

      if (Equal(local.PolicyHolder.LastName, local.PrevPolicyHolder.LastName) &&
        Equal(local.PolicyHolder.FirstName, local.PrevPolicyHolder.FirstName))
      {
        if (Equal(local.PolicyHolder.Ssn, local.PrevPolicyHolder.Ssn))
        {
          if (Equal(local.CarrierName.Text50, local.PrevCarrierName.Text50) && Equal
            (local.CarrierStreetAddr1.Text30,
            local.PrevCarrierStreetAddr1.Text30))
          {
            if (Equal(local.PolicyNumber.Text20, local.PrevPolicyNumber.Text20))
            {
              if (Equal(local.CoveredPerson.LastName,
                local.PrevCoveredPerson.LastName) && Equal
                (local.CoveredPerson.FirstName,
                local.PrevCoveredPerson.FirstName))
              {
                if (Equal(local.CoveredPersonNumb.Number,
                  local.PrevCoveredPersonNumb.Number))
                {
                  if (local.CoverPersonCount.Count >= 5)
                  {
                    continue;
                  }

                  local.CoverCodes.Text5 = TrimEnd(local.CoverCodes.Text5) + local
                    .CoverageCode.Text1;
                  ++local.CoverPersonCount.Count;
                }
                else
                {
                  local.EabReportSend.RptDetail = "   " + local
                    .PrevCoveredPerson.LastName + " " + local
                    .PrevCoveredPerson.FirstName + " " + local
                    .PrevCoveredPerson.Ssn + " " + local
                    .PrevCoverPersonBenfNb.Text12;
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.EabReportSend.RptDetail) + "  COVERAGE CODES  " +
                    local.CoverCodes.Text5;
                  local.EabFileHandling.Action = "WRITE";
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "(01) Error writing policy info to report. Returned status  = " +
                      local.External.TextReturnCode;
                    UseCabErrorReport2();
                    ++local.NumberOfErrors.Count;

                    continue;
                  }

                  local.PrevCoveredPersonNumb.Number =
                    local.CoveredPersonNumb.Number;
                  local.PrevCoverPersonBenfNb.Text12 =
                    local.CoverPersonBenfitNumb.Text12;
                  local.CoverCodes.Text5 = "";
                  local.CoverCodes.Text5 = TrimEnd(local.CoverCodes.Text5) + local
                    .CoverageCode.Text1;
                  local.CoverPersonCount.Count = 1;
                }
              }
              else
              {
                local.EabReportSend.RptDetail = "   " + local
                  .PrevCoveredPerson.LastName + " " + local
                  .PrevCoveredPerson.FirstName + " " + local
                  .PrevCoveredPerson.Ssn + " " + local
                  .PrevCoverPersonBenfNb.Text12;
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "  COVERAGE CODES  " +
                  local.CoverCodes.Text5;
                UseCabBusinessReport01();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "(01) Error writing policy info to report. Returned status  = " +
                    local.External.TextReturnCode;
                  UseCabErrorReport2();
                  ++local.NumberOfErrors.Count;

                  continue;
                }

                local.PrevCoveredPersonNumb.Number =
                  local.CoveredPersonNumb.Number;
                local.PrevCoveredPerson.Assign(local.CoveredPerson);
                local.PrevCoverPersonBenfNb.Text12 =
                  local.CoverPersonBenfitNumb.Text12;
                local.CoverCodes.Text5 = "";
                local.CoverCodes.Text5 = TrimEnd(local.CoverCodes.Text5) + local
                  .CoverageCode.Text1;
                local.CoverPersonCount.Count = 1;
              }
            }
            else
            {
              if (!IsEmpty(local.PrevCoveredPerson.LastName))
              {
                local.EabReportSend.RptDetail = "   " + local
                  .PrevCoveredPerson.LastName + " " + local
                  .PrevCoveredPerson.FirstName + " " + local
                  .PrevCoveredPerson.Ssn + " " + local
                  .PrevCoverPersonBenfNb.Text12;
                local.EabReportSend.RptDetail =
                  TrimEnd(local.EabReportSend.RptDetail) + "  COVERAGE CODES  " +
                  local.CoverCodes.Text5;
                UseCabBusinessReport01();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "(01) Error writing policy info to report. Returned status  = " +
                    local.External.TextReturnCode;
                  UseCabErrorReport2();
                  ++local.NumberOfErrors.Count;

                  continue;
                }
              }

              local.EabReportSend.RptDetail = "      POLICY NUMBER  " + local
                .PolicyNumber.Text20 + "  GROUP NUMBER  " + local
                .GroupNumber.Text20;
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "(01) Error writing policy info to report. Returned status  = " +
                  local.External.TextReturnCode;
                UseCabErrorReport2();
                ++local.NumberOfErrors.Count;

                continue;
              }

              local.EabReportSend.RptDetail =
                "      POLICY START DATE         " + local
                .PolicyStartDate.Text10 + "  POLICY END DATE " + local
                .PolicyEndDate.Text10;
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "(01) Error writing policy info to report. Returned status  = " +
                  local.External.TextReturnCode;
                UseCabErrorReport2();
                ++local.NumberOfErrors.Count;

                continue;
              }

              local.EabReportSend.RptDetail = "   EMPLOYER NAME     " + (
                local.CarrierEmployer.Name ?? "");
              local.PrevCoveredPersonNumb.Number =
                local.CoveredPersonNumb.Number;
              local.PrevCoveredPerson.Assign(local.CoveredPerson);
              local.PrevCoverPersonBenfNb.Text12 =
                local.CoverPersonBenfitNumb.Text12;
              local.PrevPolicyNumber.Text20 = local.PolicyNumber.Text20;
              local.PrevCarrierName.Text50 = local.CarrierName.Text50;
              local.PrevCarrierStreetAddr1.Text30 =
                local.CarrierStreetAddr1.Text30;
              local.CoverCodes.Text5 = "";
              local.CoverCodes.Text5 = TrimEnd(local.CoverCodes.Text5) + local
                .CoverageCode.Text1;
              local.CoverPersonCount.Count = 1;
            }
          }
          else
          {
            if (!IsEmpty(local.PrevCoveredPerson.LastName))
            {
              local.EabReportSend.RptDetail = "   " + local
                .PrevCoveredPerson.LastName + " " + local
                .PrevCoveredPerson.FirstName + " " + local
                .PrevCoveredPerson.Ssn + " " + local
                .PrevCoverPersonBenfNb.Text12;
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "  COVERAGE CODES  " +
                local.CoverCodes.Text5;
              local.EabFileHandling.Action = "WRITE";
              UseCabBusinessReport01();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "(01) Error writing policy info to report. Returned status  = " +
                  local.External.TextReturnCode;
                UseCabErrorReport2();
                ++local.NumberOfErrors.Count;

                continue;
              }
            }

            local.EabReportSend.RptDetail = "   " + local.CarrierName.Text50;
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(01) Error writing policy info to report. Returned status  = " +
                local.External.TextReturnCode;
              UseCabErrorReport2();
              ++local.NumberOfErrors.Count;

              continue;
            }

            local.EabReportSend.RptDetail = "   " + local
              .CarrierStreetAddr1.Text30 + "          " + local
              .CarrierStreetAddr2.Text30;
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(01) Error writing policy info to report. Returned status  = " +
                local.External.TextReturnCode;
              UseCabErrorReport2();
              ++local.NumberOfErrors.Count;

              continue;
            }

            local.EabReportSend.RptDetail = "   " + (
              local.CarrierEmployerAddress.City ?? "") + "   " + local
              .CarrrierState.Text5 + "   " + (
                local.CarrierEmployerAddress.ZipCode ?? "") + "   " + (
                local.CarrierEmployerAddress.Zip4 ?? "");
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(01) Error writing policy info to report. Returned status  = " +
                local.External.TextReturnCode;
              UseCabErrorReport2();
              ++local.NumberOfErrors.Count;

              continue;
            }

            local.EabReportSend.RptDetail = "      POLICY NUMBER  " + local
              .PolicyNumber.Text20 + "  GROUP NUMBER  " + local
              .GroupNumber.Text20;
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(01) Error writing policy info to report. Returned status  = " +
                local.External.TextReturnCode;
              UseCabErrorReport2();
              ++local.NumberOfErrors.Count;

              continue;
            }

            local.EabReportSend.RptDetail =
              "      POLICY START DATE         " + local
              .PolicyStartDate.Text10 + "  POLICY END DATE " + local
              .PolicyEndDate.Text10;
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(01) Error writing policy info to report. Returned status  = " +
                local.External.TextReturnCode;
              UseCabErrorReport2();
              ++local.NumberOfErrors.Count;

              continue;
            }

            local.EabReportSend.RptDetail = "   EMPLOYER NAME     " + (
              local.CarrierEmployer.Name ?? "");
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(01) Error writing policy info to report. Returned status  = " +
                local.External.TextReturnCode;
              UseCabErrorReport2();
              ++local.NumberOfErrors.Count;

              continue;
            }

            local.PrevCoveredPersonNumb.Number = local.CoveredPersonNumb.Number;
            local.PrevCoveredPerson.Assign(local.CoveredPerson);
            local.PrevCoverPersonBenfNb.Text12 =
              local.CoverPersonBenfitNumb.Text12;
            local.PrevPolicyNumber.Text20 = local.PolicyNumber.Text20;
            local.PrevCarrierName.Text50 = local.CarrierName.Text50;
            local.PrevCarrierStreetAddr1.Text30 =
              local.CarrierStreetAddr1.Text30;
            local.CoverCodes.Text5 = "";
            local.CoverCodes.Text5 = TrimEnd(local.CoverCodes.Text5) + local
              .CoverageCode.Text1;
            local.CoverPersonCount.Count = 1;
          }
        }
        else
        {
          if (!IsEmpty(local.PrevCoveredPerson.LastName))
          {
            local.EabReportSend.RptDetail = "   " + local
              .PrevCoveredPerson.LastName + " " + local
              .PrevCoveredPerson.FirstName + " " + local
              .PrevCoveredPerson.Ssn + " " + local
              .PrevCoverPersonBenfNb.Text12;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "  COVERAGE CODES  " + local
              .CoverCodes.Text5;
            local.EabFileHandling.Action = "WRITE";
            UseCabBusinessReport01();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "(01) Error writing policy info to report. Returned status  = " +
                local.External.TextReturnCode;
              UseCabErrorReport2();
              ++local.NumberOfErrors.Count;

              continue;
            }
          }

          local.EabReportSend.RptDetail = local.PolicyHolder.LastName + "  " + local
            .PolicyHolder.FirstName + "          " + local.PolicyHolder.Ssn + "     " +
            local.PolicyHolderBenfitNumb.Text12;
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(01) Error writing policy info to report. Returned status  = " +
              local.External.TextReturnCode;
            UseCabErrorReport2();
            ++local.NumberOfErrors.Count;

            continue;
          }

          local.EabReportSend.RptDetail = "   " + local.CarrierName.Text50;
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(01) Error writing policy info to report. Returned status  = " +
              local.External.TextReturnCode;
            UseCabErrorReport2();
            ++local.NumberOfErrors.Count;

            continue;
          }

          local.EabReportSend.RptDetail = "   " + local
            .CarrierStreetAddr1.Text30 + "        " + local
            .CarrierStreetAddr2.Text30;
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(01) Error writing policy info to report. Returned status  = " +
              local.External.TextReturnCode;
            UseCabErrorReport2();
            ++local.NumberOfErrors.Count;

            continue;
          }

          local.EabReportSend.RptDetail = "   " + (
            local.CarrierEmployerAddress.City ?? "") + "   " + local
            .CarrrierState.Text5 + "   " + (
              local.CarrierEmployerAddress.ZipCode ?? "") + "   " + (
              local.CarrierEmployerAddress.Zip4 ?? "");
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(01) Error writing policy info to report. Returned status  = " +
              local.External.TextReturnCode;
            UseCabErrorReport2();
            ++local.NumberOfErrors.Count;

            continue;
          }

          local.EabReportSend.RptDetail = "      POLICY NUMBER  " + local
            .PolicyNumber.Text20 + "  GROUP NUMBER  " + local
            .GroupNumber.Text20;
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(01) Error writing policy info to report. Returned status  = " +
              local.External.TextReturnCode;
            UseCabErrorReport2();
            ++local.NumberOfErrors.Count;

            continue;
          }

          local.EabReportSend.RptDetail = "      POLICY START DATE         " + local
            .PolicyStartDate.Text10 + "  POLICY END DATE " + local
            .PolicyEndDate.Text10;
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(01) Error writing policy info to report. Returned status  = " +
              local.External.TextReturnCode;
            UseCabErrorReport2();
            ++local.NumberOfErrors.Count;

            continue;
          }

          local.EabReportSend.RptDetail = "   EMPLOYER NAME     " + (
            local.CarrierEmployer.Name ?? "");
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(01) Error writing policy info to report. Returned status  = " +
              local.External.TextReturnCode;
            UseCabErrorReport2();
            ++local.NumberOfErrors.Count;

            continue;
          }

          local.PrevCoveredPersonNumb.Number = local.CoveredPersonNumb.Number;
          local.PrevCoveredPerson.Assign(local.CoveredPerson);
          local.PrevCoverPersonBenfNb.Text12 =
            local.CoverPersonBenfitNumb.Text12;
          local.PrevPolicyNumber.Text20 = local.PolicyNumber.Text20;
          local.PrevCarrierName.Text50 = local.CarrierName.Text50;
          local.PrevPolicyHolder.Assign(local.PolicyHolder);
          local.PrevCarrierStreetAddr1.Text30 = local.CarrierStreetAddr1.Text30;
          local.CoverCodes.Text5 = "";
          local.CoverCodes.Text5 = TrimEnd(local.CoverCodes.Text5) + local
            .CoverageCode.Text1;
          local.CoverPersonCount.Count = 1;
        }
      }
      else
      {
        if (!IsEmpty(local.PrevCoveredPerson.LastName))
        {
          local.EabReportSend.RptDetail = "   " + local
            .PrevCoveredPerson.LastName + " " + local
            .PrevCoveredPerson.FirstName + " " + local.PrevCoveredPerson.Ssn + " " +
            local.PrevCoverPersonBenfNb.Text12;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  COVERAGE CODES  " + local
            .CoverCodes.Text5;
          local.EabFileHandling.Action = "WRITE";
          UseCabBusinessReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "(01) Error writing policy info to report. Returned status  = " +
              local.External.TextReturnCode;
            UseCabErrorReport2();
            ++local.NumberOfErrors.Count;

            continue;
          }
        }

        local.EabReportSend.RptDetail = local.PolicyHolder.LastName + "  " + local
          .PolicyHolder.FirstName + "          " + local.PolicyHolder.Ssn + "     " +
          local.PolicyHolderBenfitNumb.Text12;
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error writing policy info to report. Returned status  = " + local
            .External.TextReturnCode;
          UseCabErrorReport2();
          ++local.NumberOfErrors.Count;

          continue;
        }

        local.EabReportSend.RptDetail = "   " + local.CarrierName.Text50;
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error writing policy info to report. Returned status  = " + local
            .External.TextReturnCode;
          UseCabErrorReport2();
          ++local.NumberOfErrors.Count;

          continue;
        }

        local.EabReportSend.RptDetail = "   " + local
          .CarrierStreetAddr1.Text30 + "        " + local
          .CarrierStreetAddr2.Text30;
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error writing policy info to report. Returned status  = " + local
            .External.TextReturnCode;
          UseCabErrorReport2();
          ++local.NumberOfErrors.Count;

          continue;
        }

        local.EabReportSend.RptDetail = "   " + (
          local.CarrierEmployerAddress.City ?? "") + "  " + local
          .CarrrierState.Text5 + "  " + (
            local.CarrierEmployerAddress.ZipCode ?? "") + "  " + (
            local.CarrierEmployerAddress.Zip4 ?? "");
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error writing policy info to report. Returned status  = " + local
            .External.TextReturnCode;
          UseCabErrorReport2();
          ++local.NumberOfErrors.Count;

          continue;
        }

        local.EabReportSend.RptDetail = "      POLICY NUMBER  " + local
          .PolicyNumber.Text20 + "  GROUP NUMBER  " + local.GroupNumber.Text20;
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error writing policy info to report. Returned status  = " + local
            .External.TextReturnCode;
          UseCabErrorReport2();
          ++local.NumberOfErrors.Count;

          continue;
        }

        local.EabReportSend.RptDetail = "      POLICY START DATE      " + local
          .PolicyStartDate.Text10 + "  POLICY END DATE " + local
          .PolicyEndDate.Text10;
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error writing policy info to report. Returned status  = " + local
            .External.TextReturnCode;
          UseCabErrorReport2();
          ++local.NumberOfErrors.Count;

          continue;
        }

        local.EabReportSend.RptDetail = "   EMPLOYER NAME     " + (
          local.CarrierEmployer.Name ?? "");
        local.EabFileHandling.Action = "WRITE";
        UseCabBusinessReport01();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(01) Error writing policy info to report. Returned status  = " + local
            .External.TextReturnCode;
          UseCabErrorReport2();
          ++local.NumberOfErrors.Count;

          continue;
        }

        local.PrevCoveredPersonNumb.Number = local.CoveredPersonNumb.Number;
        local.PrevCoveredPerson.Assign(local.CoveredPerson);
        local.PrevCoverPersonBenfNb.Text12 = local.CoverPersonBenfitNumb.Text12;
        local.PrevPolicyNumber.Text20 = local.PolicyNumber.Text20;
        local.PrevCarrierName.Text50 = local.CarrierName.Text50;
        local.PrevCarrierStreetAddr1.Text30 = local.CarrierStreetAddr1.Text30;
        local.PrevPolicyHolder.Assign(local.PolicyHolder);
        local.CoverCodes.Text5 = "";
        local.CoverCodes.Text5 = TrimEnd(local.CoverCodes.Text5) + local
          .CoverageCode.Text1;
        local.CoverPersonCount.Count = 1;
      }

      ++local.RecordsProcessed.Count;
    }
    while(Equal(local.Main.TextReturnCode, "00"));

    if (!IsEmpty(local.PrevCoveredPerson.LastName))
    {
      local.EabReportSend.RptDetail = "   " + local
        .PrevCoveredPerson.LastName + " " + local
        .PrevCoveredPerson.FirstName + " " + local.PrevCoveredPerson.Ssn + " " +
        local.PrevCoverPersonBenfNb.Text12;
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "  COVERAGE CODES  " +
        local.CoverCodes.Text5;
      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport01();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(01) Error writing policy info to report. Returned status  = " + local
          .External.TextReturnCode;
        UseCabErrorReport2();
        ++local.NumberOfErrors.Count;
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Incoming Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB682ProcessKhpaPolicyfile2();

    if (!Equal(local.External.TextReturnCode, "00"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Incoming Extract File...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseFnB682Close();

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Outgoing Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Outgoing Extract File...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabControlReport();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabErrorReport3();

    if (!Equal(local.Close.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.WriteEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.WriteEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnB682BatchInitialization()
  {
    var useImport = new FnB682BatchInitialization.Import();
    var useExport = new FnB682BatchInitialization.Export();

    Call(FnB682BatchInitialization.Execute, useImport, useExport);

    local.ProgramProcessingInfo.ProcessDate =
      useExport.ProgramProcessingInfo.ProcessDate;
  }

  private void UseFnB682Close()
  {
    var useImport = new FnB682Close.Import();
    var useExport = new FnB682Close.Export();

    useImport.TotalNumProcessed.Count = local.RecordsProcessed.Count;
    useImport.NumOfRecordsRead.Count = local.ReadCount.Count;
    useImport.NumberOfErrorRecords.Count = local.NumberOfErrors.Count;

    Call(FnB682Close.Execute, useImport, useExport);
  }

  private void UseFnB682ProcessKhpaPolicyfile1()
  {
    var useImport = new FnB682ProcessKhpaPolicyfile.Import();
    var useExport = new FnB682ProcessKhpaPolicyfile.Export();

    useImport.EabFileHandling.Action = local.Read.Action;
    useExport.PolicyHolder.Assign(local.PolicyHolder);
    useExport.PolicyHolderBenfitNumb.Text12 =
      local.PolicyHolderBenfitNumb.Text12;
    useExport.CoveredPerson.Assign(local.CoveredPerson);
    useExport.CoverPersonBenfitNumb.Text12 = local.CoverPersonBenfitNumb.Text12;
    useExport.CarrierName.Text50 = local.CarrierName.Text50;
    useExport.Carrier.Assign(local.CarrierEmployerAddress);
    useExport.CarrierStreetAddr1.Text30 = local.CarrierStreetAddr1.Text30;
    useExport.CarrierStreetAddr2.Text30 = local.CarrierStreetAddr2.Text30;
    useExport.CarrrierState.Text5 = local.CarrrierState.Text5;
    useExport.PolicyNumber.Text20 = local.PolicyNumber.Text20;
    useExport.GroupNumber.Text20 = local.GroupNumber.Text20;
    useExport.CoverageCode.Text1 = local.CoverageCode.Text1;
    useExport.PolicyStartDate.Text10 = local.PolicyStartDate.Text10;
    useExport.PolicyEndDate.Text10 = local.PolicyEndDate.Text10;
    useExport.EdxportCarrier.Name = local.CarrierEmployer.Name;
    useExport.External.Assign(local.Main);

    Call(FnB682ProcessKhpaPolicyfile.Execute, useImport, useExport);

    local.PolicyHolder.Assign(useExport.PolicyHolder);
    local.PolicyHolderBenfitNumb.Text12 =
      useExport.PolicyHolderBenfitNumb.Text12;
    local.CoveredPerson.Assign(useExport.CoveredPerson);
    local.CoverPersonBenfitNumb.Text12 = useExport.CoverPersonBenfitNumb.Text12;
    local.CarrierName.Text50 = useExport.CarrierName.Text50;
    local.CarrierEmployerAddress.Assign(useExport.Carrier);
    local.CarrierStreetAddr1.Text30 = useExport.CarrierStreetAddr1.Text30;
    local.CarrierStreetAddr2.Text30 = useExport.CarrierStreetAddr2.Text30;
    local.CarrrierState.Text5 = useExport.CarrrierState.Text5;
    local.PolicyNumber.Text20 = useExport.PolicyNumber.Text20;
    local.GroupNumber.Text20 = useExport.GroupNumber.Text20;
    local.CoverageCode.Text1 = useExport.CoverageCode.Text1;
    local.PolicyStartDate.Text10 = useExport.PolicyStartDate.Text10;
    local.PolicyEndDate.Text10 = useExport.PolicyEndDate.Text10;
    local.CarrierEmployer.Name = useExport.EdxportCarrier.Name;
    local.Main.Assign(useExport.External);
  }

  private void UseFnB682ProcessKhpaPolicyfile2()
  {
    var useImport = new FnB682ProcessKhpaPolicyfile.Import();
    var useExport = new FnB682ProcessKhpaPolicyfile.Export();

    useImport.EabFileHandling.Action = local.Close.Action;
    useExport.External.Assign(local.External);

    Call(FnB682ProcessKhpaPolicyfile.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CoveredPersonNumb.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
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
    /// A value of RecordsProcessed.
    /// </summary>
    [JsonPropertyName("recordsProcessed")]
    public Common RecordsProcessed
    {
      get => recordsProcessed ??= new();
      set => recordsProcessed = value;
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
    /// A value of PrevCoverPersonBenfNb.
    /// </summary>
    [JsonPropertyName("prevCoverPersonBenfNb")]
    public TextWorkArea PrevCoverPersonBenfNb
    {
      get => prevCoverPersonBenfNb ??= new();
      set => prevCoverPersonBenfNb = value;
    }

    /// <summary>
    /// A value of PrevPolicyHolderBenfNb.
    /// </summary>
    [JsonPropertyName("prevPolicyHolderBenfNb")]
    public TextWorkArea PrevPolicyHolderBenfNb
    {
      get => prevPolicyHolderBenfNb ??= new();
      set => prevPolicyHolderBenfNb = value;
    }

    /// <summary>
    /// A value of Main.
    /// </summary>
    [JsonPropertyName("main")]
    public External Main
    {
      get => main ??= new();
      set => main = value;
    }

    /// <summary>
    /// A value of CoverPersonCount.
    /// </summary>
    [JsonPropertyName("coverPersonCount")]
    public Common CoverPersonCount
    {
      get => coverPersonCount ??= new();
      set => coverPersonCount = value;
    }

    /// <summary>
    /// A value of PrevCarrierStreetAddr1.
    /// </summary>
    [JsonPropertyName("prevCarrierStreetAddr1")]
    public TextWorkArea PrevCarrierStreetAddr1
    {
      get => prevCarrierStreetAddr1 ??= new();
      set => prevCarrierStreetAddr1 = value;
    }

    /// <summary>
    /// A value of CoverCodes.
    /// </summary>
    [JsonPropertyName("coverCodes")]
    public WorkArea CoverCodes
    {
      get => coverCodes ??= new();
      set => coverCodes = value;
    }

    /// <summary>
    /// A value of CoverageCode.
    /// </summary>
    [JsonPropertyName("coverageCode")]
    public WorkArea CoverageCode
    {
      get => coverageCode ??= new();
      set => coverageCode = value;
    }

    /// <summary>
    /// A value of WriteExternal.
    /// </summary>
    [JsonPropertyName("writeExternal")]
    public External WriteExternal
    {
      get => writeExternal ??= new();
      set => writeExternal = value;
    }

    /// <summary>
    /// A value of PrevCoveredPersonNumb.
    /// </summary>
    [JsonPropertyName("prevCoveredPersonNumb")]
    public CsePersonsWorkSet PrevCoveredPersonNumb
    {
      get => prevCoveredPersonNumb ??= new();
      set => prevCoveredPersonNumb = value;
    }

    /// <summary>
    /// A value of ReturnedCoveredPerson.
    /// </summary>
    [JsonPropertyName("returnedCoveredPerson")]
    public CsePersonsWorkSet ReturnedCoveredPerson
    {
      get => returnedCoveredPerson ??= new();
      set => returnedCoveredPerson = value;
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
    /// A value of PolicyHolderNumb.
    /// </summary>
    [JsonPropertyName("policyHolderNumb")]
    public CsePersonsWorkSet PolicyHolderNumb
    {
      get => policyHolderNumb ??= new();
      set => policyHolderNumb = value;
    }

    /// <summary>
    /// A value of CoveredPersonNumb.
    /// </summary>
    [JsonPropertyName("coveredPersonNumb")]
    public CsePersonsWorkSet CoveredPersonNumb
    {
      get => coveredPersonNumb ??= new();
      set => coveredPersonNumb = value;
    }

    /// <summary>
    /// A value of PrevPolicyNumber.
    /// </summary>
    [JsonPropertyName("prevPolicyNumber")]
    public WorkArea PrevPolicyNumber
    {
      get => prevPolicyNumber ??= new();
      set => prevPolicyNumber = value;
    }

    /// <summary>
    /// A value of PrevCarrierName.
    /// </summary>
    [JsonPropertyName("prevCarrierName")]
    public WorkArea PrevCarrierName
    {
      get => prevCarrierName ??= new();
      set => prevCarrierName = value;
    }

    /// <summary>
    /// A value of PrevPolicyHolder.
    /// </summary>
    [JsonPropertyName("prevPolicyHolder")]
    public CsePersonsWorkSet PrevPolicyHolder
    {
      get => prevPolicyHolder ??= new();
      set => prevPolicyHolder = value;
    }

    /// <summary>
    /// A value of PrevCoveredPerson.
    /// </summary>
    [JsonPropertyName("prevCoveredPerson")]
    public CsePersonsWorkSet PrevCoveredPerson
    {
      get => prevCoveredPerson ??= new();
      set => prevCoveredPerson = value;
    }

    /// <summary>
    /// A value of CarrierEmployer.
    /// </summary>
    [JsonPropertyName("carrierEmployer")]
    public Employer CarrierEmployer
    {
      get => carrierEmployer ??= new();
      set => carrierEmployer = value;
    }

    /// <summary>
    /// A value of PolicyEndDate.
    /// </summary>
    [JsonPropertyName("policyEndDate")]
    public WorkArea PolicyEndDate
    {
      get => policyEndDate ??= new();
      set => policyEndDate = value;
    }

    /// <summary>
    /// A value of PolicyStartDate.
    /// </summary>
    [JsonPropertyName("policyStartDate")]
    public WorkArea PolicyStartDate
    {
      get => policyStartDate ??= new();
      set => policyStartDate = value;
    }

    /// <summary>
    /// A value of GroupNumber.
    /// </summary>
    [JsonPropertyName("groupNumber")]
    public WorkArea GroupNumber
    {
      get => groupNumber ??= new();
      set => groupNumber = value;
    }

    /// <summary>
    /// A value of PolicyNumber.
    /// </summary>
    [JsonPropertyName("policyNumber")]
    public WorkArea PolicyNumber
    {
      get => policyNumber ??= new();
      set => policyNumber = value;
    }

    /// <summary>
    /// A value of CarrrierState.
    /// </summary>
    [JsonPropertyName("carrrierState")]
    public WorkArea CarrrierState
    {
      get => carrrierState ??= new();
      set => carrrierState = value;
    }

    /// <summary>
    /// A value of CarrierStreetAddr2.
    /// </summary>
    [JsonPropertyName("carrierStreetAddr2")]
    public TextWorkArea CarrierStreetAddr2
    {
      get => carrierStreetAddr2 ??= new();
      set => carrierStreetAddr2 = value;
    }

    /// <summary>
    /// A value of CarrierStreetAddr1.
    /// </summary>
    [JsonPropertyName("carrierStreetAddr1")]
    public TextWorkArea CarrierStreetAddr1
    {
      get => carrierStreetAddr1 ??= new();
      set => carrierStreetAddr1 = value;
    }

    /// <summary>
    /// A value of CarrierEmployerAddress.
    /// </summary>
    [JsonPropertyName("carrierEmployerAddress")]
    public EmployerAddress CarrierEmployerAddress
    {
      get => carrierEmployerAddress ??= new();
      set => carrierEmployerAddress = value;
    }

    /// <summary>
    /// A value of CarrierName.
    /// </summary>
    [JsonPropertyName("carrierName")]
    public WorkArea CarrierName
    {
      get => carrierName ??= new();
      set => carrierName = value;
    }

    /// <summary>
    /// A value of CoverPersonBenfitNumb.
    /// </summary>
    [JsonPropertyName("coverPersonBenfitNumb")]
    public TextWorkArea CoverPersonBenfitNumb
    {
      get => coverPersonBenfitNumb ??= new();
      set => coverPersonBenfitNumb = value;
    }

    /// <summary>
    /// A value of CoveredPerson.
    /// </summary>
    [JsonPropertyName("coveredPerson")]
    public CsePersonsWorkSet CoveredPerson
    {
      get => coveredPerson ??= new();
      set => coveredPerson = value;
    }

    /// <summary>
    /// A value of PolicyHolderBenfitNumb.
    /// </summary>
    [JsonPropertyName("policyHolderBenfitNumb")]
    public TextWorkArea PolicyHolderBenfitNumb
    {
      get => policyHolderBenfitNumb ??= new();
      set => policyHolderBenfitNumb = value;
    }

    /// <summary>
    /// A value of PolicyHolder.
    /// </summary>
    [JsonPropertyName("policyHolder")]
    public CsePersonsWorkSet PolicyHolder
    {
      get => policyHolder ??= new();
      set => policyHolder = value;
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
    /// A value of WriteEabFileHandling.
    /// </summary>
    [JsonPropertyName("writeEabFileHandling")]
    public EabFileHandling WriteEabFileHandling
    {
      get => writeEabFileHandling ??= new();
      set => writeEabFileHandling = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public EabFileHandling Close
    {
      get => close ??= new();
      set => close = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public EabFileHandling Read
    {
      get => read ??= new();
      set => read = value;
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
    /// A value of NumOfStatementsErrored.
    /// </summary>
    [JsonPropertyName("numOfStatementsErrored")]
    public Common NumOfStatementsErrored
    {
      get => numOfStatementsErrored ??= new();
      set => numOfStatementsErrored = value;
    }

    /// <summary>
    /// A value of NumOfStatementsPrinted.
    /// </summary>
    [JsonPropertyName("numOfStatementsPrinted")]
    public Common NumOfStatementsPrinted
    {
      get => numOfStatementsPrinted ??= new();
      set => numOfStatementsPrinted = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
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
    /// A value of ReadCount.
    /// </summary>
    [JsonPropertyName("readCount")]
    public Common ReadCount
    {
      get => readCount ??= new();
      set => readCount = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of NumberOfErrors.
    /// </summary>
    [JsonPropertyName("numberOfErrors")]
    public Common NumberOfErrors
    {
      get => numberOfErrors ??= new();
      set => numberOfErrors = value;
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
    /// A value of DateMonth.
    /// </summary>
    [JsonPropertyName("dateMonth")]
    public EabReportSend DateMonth
    {
      get => dateMonth ??= new();
      set => dateMonth = value;
    }

    private Common recordsProcessed;
    private CsePerson csePerson;
    private TextWorkArea prevCoverPersonBenfNb;
    private TextWorkArea prevPolicyHolderBenfNb;
    private External main;
    private Common coverPersonCount;
    private TextWorkArea prevCarrierStreetAddr1;
    private WorkArea coverCodes;
    private WorkArea coverageCode;
    private External writeExternal;
    private CsePersonsWorkSet prevCoveredPersonNumb;
    private CsePersonsWorkSet returnedCoveredPerson;
    private Common record;
    private CsePersonsWorkSet policyHolderNumb;
    private CsePersonsWorkSet coveredPersonNumb;
    private WorkArea prevPolicyNumber;
    private WorkArea prevCarrierName;
    private CsePersonsWorkSet prevPolicyHolder;
    private CsePersonsWorkSet prevCoveredPerson;
    private Employer carrierEmployer;
    private WorkArea policyEndDate;
    private WorkArea policyStartDate;
    private WorkArea groupNumber;
    private WorkArea policyNumber;
    private WorkArea carrrierState;
    private TextWorkArea carrierStreetAddr2;
    private TextWorkArea carrierStreetAddr1;
    private EmployerAddress carrierEmployerAddress;
    private WorkArea carrierName;
    private TextWorkArea coverPersonBenfitNumb;
    private CsePersonsWorkSet coveredPerson;
    private TextWorkArea policyHolderBenfitNumb;
    private CsePersonsWorkSet policyHolder;
    private EabReportSend eabReportSend;
    private EabFileHandling writeEabFileHandling;
    private EabFileHandling close;
    private EabFileHandling read;
    private ProgramProcessingInfo programProcessingInfo;
    private Common numOfStatementsErrored;
    private Common numOfStatementsPrinted;
    private Common counter;
    private External external;
    private Common readCount;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private Common numberOfErrors;
    private DateWorkArea end;
    private DateWorkArea start;
    private EabReportSend header;
    private EabReportSend dateMonth;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private CaseUnit caseUnit;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
  }
#endregion
}
