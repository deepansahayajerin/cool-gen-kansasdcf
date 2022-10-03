// Program: FN_B722_OCSE34_KPC_INTERFACE, ID: 371200249, model: 746.
// Short name: SWEB722P
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B722_OCSE34_KPC_INTERFACE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB722Ocse34KpcInterface: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B722_OCSE34_KPC_INTERFACE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB722Ocse34KpcInterface(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB722Ocse34KpcInterface.
  /// </summary>
  public FnB722Ocse34KpcInterface(IContext context, Import import, Export export)
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
    // --------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // ----------------------------------------------
    // 12/02/03  GVandy	WR040134	Initial Development
    // --------------------------------------------------------------------------------------
    // 11/03/2004  cm johnson        pr00222953     previous for held and stale 
    // files
    // 02/15/2005  e.shirk        	WR040796    	OCSE34 held/stale/UI aging 
    // changes.
    // 05/19/2005 cm johnson  pr 244780  ui percentage on aging do not age on 
    // full amount.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Write.Action = "WRITE";

    // --------------------------------------------------------------------------------------------
    // -- General housekeeping and initializations.
    // --------------------------------------------------------------------------------------------
    UseFnB722BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Initialization CAB Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      // Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EndDate.Date = local.Ocse34.ReportingPeriodEndDate;
    local.Local2Date.Date = AddDays(local.EndDate.Date, -1);
    local.Local30Date.Date = AddDays(local.EndDate.Date, -29);
    local.Local180Date.Date = AddDays(local.EndDate.Date, -181);
    local.Local365Date.Date = AddDays(local.EndDate.Date, -364);
    local.Local1095Date.Date = AddDays(local.EndDate.Date, -1094);
    local.Local1825Date.Date = AddDays(local.EndDate.Date, -1824);

    // -------------------------------------------------------------------------------------------
    // -- Process each KPC extract file.
    // -------------------------------------------------------------------------------------------
    // set up flag for stale and held so you know which one to process for 
    // previous
    for(local.FileNumber.Count = local.StartingFileNumber.Count; local
      .FileNumber.Count <= 7; ++local.FileNumber.Count)
    {
      switch(local.FileNumber.Count)
      {
        case 1:
          local.FileName.TextLine80 = "NON-IVD, IWO COLLECTIONS";

          break;
        case 2:
          local.FileName.TextLine80 = "IVD, NON-IWO COLLECTIONS";

          break;
        case 3:
          local.FileName.TextLine80 = "NON IVD, IWO FORWARDED TO PAYEES";

          break;
        case 4:
          local.FileName.TextLine80 = "UNIDENTIFIED PAYMENTS";

          break;
        case 5:
          local.FileName.TextLine80 = "STALE DATED LISTING";

          break;
        case 6:
          local.FileName.TextLine80 = "HELD DISBURSEMENT LISTING";

          break;
        case 7:
          local.FileName.TextLine80 = "LAST DAYS BUSINESS";

          break;
        default:
          break;
      }

      local.Arecord.Count = 0;
      local.Brecord.Count = 0;
      local.Brecord.TotalCurrency = 0;
      local.Jrecord.Count = 0;
      local.Jrecord.TotalCurrency = 0;
      local.Trecord.Count = 0;

      // -------------------------------------------------------------------------------------------
      // -- Open KPC file.
      // -------------------------------------------------------------------------------------------
      local.External.FileInstruction = "OPEN";
      UseFnB721ExtReadFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        local.EabReportSend.RptDetail = "Error opening " + TrimEnd
          (local.FileName.TextLine80) + " file.";
        UseCabErrorReport1();

        // Set Abort exit state and go to next file.
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        continue;
      }

      do
      {
        // -------------------------------------------------------------------------------------------
        // -- Read a record from the KPC file.
        // -------------------------------------------------------------------------------------------
        local.External.FileInstruction = "READ";
        UseFnB721ExtReadFile();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          if (Equal(local.External.TextReturnCode, "EF"))
          {
            continue;
          }

          local.EabReportSend.RptDetail = "Error reading " + TrimEnd
            (local.FileName.TextLine80) + " file.";
          UseCabErrorReport1();

          // Set Abort exit state and go to the next file.
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto Next;
        }

        // -------------------------------------------------------------------------------------------
        // -- Process the information based on the type of record.
        // -------------------------------------------------------------------------------------------
        local.RecordType.SelectChar =
          Substring(local.External.TextLine80, 1, 1);

        switch(AsChar(local.RecordType.SelectChar))
        {
          case 'A':
            // -------------------------------------------------------------------------------------------
            // -- Header Record
            // -------------------------------------------------------------------------------------------
            ++local.Arecord.Count;

            // cmj  5/04/2005  add dates to check for held and stale files - can
            // only process current and previous dates.
            // POPULATE THE CSE DATE WITH THE OCSE34 REPORTING PERIOD END DATE 
            // WITH YEAR AND MONTH
            local.CseReportYear.Text4 =
              NumberToString(Year(local.Ocse34.ReportingPeriodEndDate), 12, 4);
            local.CseReportMo.Text2 =
              NumberToString(Month(local.Ocse34.ReportingPeriodEndDate), 14, 2);
              
            local.CseCompareDate.Text6 = local.CseReportYear.Text4 + local
              .CseReportMo.Text2;

            // POPULATE THE KPC DATE WITH THE KPC  REPORTING PERIOD END DATE 
            // WITH YEAR AND MONTH from the KPC file
            local.HsDateYear.Text4 =
              Substring(local.External.TextLine80, 20, 4);
            local.HsDateMonth.Text2 =
              Substring(local.External.TextLine80, 14, 2);

            // populate the date compare with year and month for comparison 
            // later
            local.HsCompareDate.Text6 = local.HsDateYear.Text4 + local
              .HsDateMonth.Text2;

            // held and stale files only
            if (local.FileNumber.Count == 5 || local.FileNumber.Count == 6)
            {
              // if cse date = hs date is ok to process
              if (Equal(local.CseReportYear.Text4, local.HsDateYear.Text4) && Equal
                (local.CseReportMo.Text2, local.HsDateMonth.Text2))
              {
              }
              else
              {
                // calculate a previous quarter
                local.HsDatePrevious.Date =
                  AddDays(AddMonths(
                    AddDays(local.Ocse34.ReportingPeriodEndDate, 0), -3), 0);

                // move calculated previous into a compare 6 byte field  year 
                // and month
                local.KpcPreviousCompare.Text6 =
                  NumberToString(Year(local.HsDatePrevious.Date), 12, 4) + NumberToString
                  (Month(local.HsDatePrevious.Date), 14, 2);

                // compare hs date previous to the hs date on the file  - if 
                // equal ok to process otherwise it is a reject
                if (!Equal(local.KpcPreviousCompare.Text6,
                  local.HsCompareDate.Text6))
                {
                  local.Ocse34.Period =
                    Year(local.Ocse34.ReportingPeriodEndDate) * 100;
                  local.EabReportSend.RptDetail = "KPC end date " + local
                    .HsCompareDate.Text6 + " does not match CSE end date for h&s" +
                    local.CseCompareDate.Text6 + " in file " + local
                    .FileName.TextLine80;
                  UseCabErrorReport1();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
                }
              }
            }
            else if (!Equal(local.CseCompareDate.Text6,
              local.HsCompareDate.Text6))
            {
              local.Ocse34.Period =
                Year(local.Ocse34.ReportingPeriodEndDate) * 100;
              local.EabReportSend.RptDetail = "KPC end date " + local
                .HsCompareDate.Text6 + " does not match CSE end date" + local
                .CseCompareDate.Text6 + " in file " + local
                .FileName.TextLine80;
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
            }

            break;
          case 'B':
            // -------------------------------------------------------------------------------------------
            // -- Payment Record
            // -------------------------------------------------------------------------------------------
            ++local.Brecord.Count;

            if (local.FileNumber.Count == 4)
            {
              // -- Validate the total UI amount is in the format 
              // 999999999999999.99
              local.AlocalStartingPosition.Count = 3;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 15), "0123456789") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 15, 1), ".") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 16, 2), "0123456789") == 0
                )
              {
                // -- Total UI amount format is valid.  Convert to numeric.
                local.TotalUiAmount.TotalCurrency =
                  StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 15)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count + 16, 2)) / (decimal)100;
              }
              else
              {
                // -- Total UI amount is not in a 99999999999999.99 format.
                local.EabReportSend.RptDetail =
                  "Invalid format for total UI amount " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 18) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              // -- Validate the total non-IVD ,IWO and IVD % is in the format 
              // 999
              local.AlocalStartingPosition.Count = 22;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 3), "0123456789") == 0)
              {
                // -- Format is valid.  Convert to numeric.
                local.Pct.Percentage =
                  (int)StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 3));
                local.PctUiSaved.Percentage = local.Pct.Percentage;
                local.NonIvdIwoAndIvd.Text4 =
                  Substring(local.External.TextLine80,
                  local.AlocalStartingPosition.Count, 3);

                // -- Compute the total non-IVD ,IWO and IVD amount.
                local.Ocse34.KpcUiNonIvdIwoAmt =
                  (int?)Math.Round(
                    local.TotalUiAmount.TotalCurrency *
                  ((decimal)local.Pct.Percentage / 100),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                // -- Percentage is not in a 999 format.
                local.EabReportSend.RptDetail =
                  "Invalid format for TOTAL non-IVD ,IWO and IVD %  " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 3) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              // -- Validate the total UI quarterly amount is in the format 
              // 999999999999999.99
              local.AlocalStartingPosition.Count = 26;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 15), "0123456789") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 15, 1), ".") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 16, 2), "0123456789") == 0
                )
              {
                // -- Total UI quarterly amount format is valid.  Convert to 
                // numeric.
                local.TotalUiQuarterlyAmount.TotalCurrency =
                  StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 15)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count + 16, 2)) / (decimal)100;
              }
              else
              {
                // -- Total UI quarterly amount is not in a 99999999999999.99 
                // format.
                local.EabReportSend.RptDetail =
                  "Invalid format for total UI quarterly amount " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 18) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              // -- Validate the total IVD,IWO and NON-IVD IWO % is in the 
              // format 999
              local.AlocalStartingPosition.Count = 45;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 3), "0123456789") == 0)
              {
                // -- Format is valid.  Convert to numeric.
                local.Pct.Percentage =
                  (int)StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 3));
                local.IvdIwoAndNonIvdIwo.Text4 =
                  Substring(local.External.TextLine80,
                  local.AlocalStartingPosition.Count, 3);

                // -- Compute the total IVD,IWO and NON-IVD IWO amount.
                local.Ocse34.KpcUiIvdNivdIwoAmt =
                  (int?)Math.Round(
                    local.TotalUiQuarterlyAmount.TotalCurrency *
                  ((decimal)local.Pct.Percentage / 100),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                // -- Percentage is not in a 999 format.
                local.EabReportSend.RptDetail =
                  "Invalid format for TOTAL IVD,IWO and NON-IVD IWO % " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 3) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              // -- Validate the total IVD INTERSTATE NON-IWO % is in the format
              // 999
              local.AlocalStartingPosition.Count = 49;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 3), "0123456789") == 0)
              {
                // -- Format is valid.  Convert to numeric.
                local.Pct.Percentage =
                  (int)StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 3));
                local.IvdInterstateNonIwo.Text4 =
                  Substring(local.External.TextLine80,
                  local.AlocalStartingPosition.Count, 3);

                // -- Compute the total IVD INTERSTATE NON-IWO amount.
                local.Ocse34.UiIvdNonIwoIntAmt =
                  (int?)Math.Round(
                    local.TotalUiQuarterlyAmount.TotalCurrency *
                  ((decimal)local.Pct.Percentage / 100),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                // -- Percentage is not in a 999 format.
                local.EabReportSend.RptDetail =
                  "Invalid format for TOTAL IVD INTERSTATE NON-IWO % " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 3) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              // -- Validate the total IVD NON-INTERSTATE, NON IWO % is in the 
              // format 999
              local.AlocalStartingPosition.Count = 53;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 3), "0123456789") == 0)
              {
                // -- Format is valid.  Convert to numeric.
                local.Pct.Percentage =
                  (int)StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 3));
                local.IvdNonInterstateNonIwo.Text4 =
                  Substring(local.External.TextLine80,
                  local.AlocalStartingPosition.Count, 3);

                // -- Compute the total IVD NON-INTERSTATE, NON IWO amount.
                local.Ocse34.KpcUiIvdNonIwoNonIntAmt =
                  (int?)Math.Round(
                    local.TotalUiQuarterlyAmount.TotalCurrency *
                  ((decimal)local.Pct.Percentage / 100),
                  MidpointRounding.AwayFromZero);
              }
              else
              {
                // -- Percentage is not in a 999 format.
                local.EabReportSend.RptDetail =
                  "Invalid format for TOTAL IVD NON-INTERSTATE, NON IWO % " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 3) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              // ***  Verify aging percentage figures are numeric.
              local.AlocalStartingPosition.Count = 57;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 14), "0123456789") == 0)
              {
                local.PercentageSum.TotalInteger =
                  StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  57, 2)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  59, 2)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  61, 2)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  63, 2)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  65, 2)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  67, 2)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  69, 2));

                // ***  Verify aging percentage figures sum 100.
                if (local.PercentageSum.TotalInteger != 100)
                {
                  local.EabReportSend.RptDetail =
                    "UI percentages do not total 100%" + Substring
                    (local.External.TextLine80, External.TextLine80_MaxLength,
                    local.AlocalStartingPosition.Count, 14) + " in file " + local
                    .FileName.TextLine80;
                  UseCabErrorReport1();
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
                }
                else
                {
                  // ***  Calculate aged UI amounts.
                  local.LbdUiAmt.TotalCurrency =
                    local.TotalUiAmount.TotalCurrency * (
                      StringToNumber(Substring(
                      local.External.TextLine80, External.TextLine80_MaxLength,
                    57, 2)) / (decimal)100);
                  local.Gt2UiAmt.TotalCurrency =
                    local.TotalUiAmount.TotalCurrency * (
                      StringToNumber(Substring(
                      local.External.TextLine80, External.TextLine80_MaxLength,
                    59, 2)) / (decimal)100);
                  local.Gt30UiAmt.TotalCurrency =
                    local.TotalUiAmount.TotalCurrency * (
                      StringToNumber(Substring(
                      local.External.TextLine80, External.TextLine80_MaxLength,
                    61, 2)) / (decimal)100);
                  local.Gt180UiAmt.TotalCurrency =
                    local.TotalUiAmount.TotalCurrency * (
                      StringToNumber(Substring(
                      local.External.TextLine80, External.TextLine80_MaxLength,
                    63, 2)) / (decimal)100);
                  local.Gt365UiAmt.TotalCurrency =
                    local.TotalUiAmount.TotalCurrency * (
                      StringToNumber(Substring(
                      local.External.TextLine80, External.TextLine80_MaxLength,
                    65, 2)) / (decimal)100);
                  local.Gt1095UiAmt.TotalCurrency =
                    local.TotalUiAmount.TotalCurrency * (
                      StringToNumber(Substring(
                      local.External.TextLine80, External.TextLine80_MaxLength,
                    67, 2)) / (decimal)100);
                  local.Gt1825UiAmt.TotalCurrency =
                    local.TotalUiAmount.TotalCurrency * (
                      StringToNumber(Substring(
                      local.External.TextLine80, External.TextLine80_MaxLength,
                    69, 2)) / (decimal)100);
                }
              }
              else
              {
                local.EabReportSend.RptDetail =
                  "Invalid format for one of the UI aging percentages." + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 14) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }
            }
            else
            {
              // -- Set the starting position of amount within the B record.
              switch(local.FileNumber.Count)
              {
                case 1:
                  local.AlocalStartingPosition.Count = 18;

                  break;
                case 2:
                  local.AlocalStartingPosition.Count = 18;

                  break;
                case 3:
                  local.AlocalStartingPosition.Count = 18;

                  break;
                case 5:
                  local.AlocalStartingPosition.Count = 29;

                  break;
                case 6:
                  local.AlocalStartingPosition.Count = 29;

                  break;
                case 7:
                  local.AlocalStartingPosition.Count = 18;

                  break;
                default:
                  break;
              }

              // -- Validate the payment amount is in the format 
              // 999999999999999.99
              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 15), "0123456789") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 15, 1), ".") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 16, 2), "0123456789") == 0
                )
              {
                // -- Payment amount format is valid.  Add the payment amount to
                // the running payment total.
                local.Common.TotalCurrency =
                  StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 15)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count + 16, 2)) / (decimal)100;
                local.Brecord.TotalCurrency += local.Common.TotalCurrency;
              }
              else
              {
                // -- Payment amount is not in a 99999999999999.99 format.
                local.EabReportSend.RptDetail =
                  "Invalid format for payment amount " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 18) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              if (local.FileNumber.Count == 5 || local.FileNumber.Count == 6)
              {
                local.HeldStaleDate.Date =
                  StringToDate(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  81, 4) + "-" + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength, 75,
                  2) + "-" + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength, 78,
                  2));

                if (Lt(local.HeldStaleDate.Date, local.Local1825Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt1825StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt1825HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local1095Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt1095StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt1095HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local365Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt365StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt365HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local180Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt180StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt180HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local30Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt30StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt30HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local2Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt2StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt2HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (local.FileNumber.Count == 5)
                {
                  local.LbdStaleAmt.TotalCurrency += local.Common.TotalCurrency;
                }
                else
                {
                  local.LbdHeldAmt.TotalCurrency += local.Common.TotalCurrency;
                }
              }
            }

            break;
          case 'J':
            // -------------------------------------------------------------------------------------------
            // -- Adjustment Record
            // -------------------------------------------------------------------------------------------
            ++local.Jrecord.Count;

            if (local.FileNumber.Count == 4)
            {
              // -- Invalid record type for the UNIDENTIFIED PAYMENTS file.
              local.EabReportSend.RptDetail = "Invalid record type " + local
                .RecordType.SelectChar + " found in file " + TrimEnd
                (local.FileName.TextLine80);
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
            }
            else
            {
              // -- Set the starting position of amount within the J record.
              switch(local.FileNumber.Count)
              {
                case 1:
                  local.AlocalStartingPosition.Count = 18;

                  break;
                case 2:
                  local.AlocalStartingPosition.Count = 18;

                  break;
                case 3:
                  local.AlocalStartingPosition.Count = 18;

                  break;
                case 5:
                  local.AlocalStartingPosition.Count = 29;

                  break;
                case 6:
                  local.AlocalStartingPosition.Count = 29;

                  break;
                case 7:
                  local.AlocalStartingPosition.Count = 18;

                  break;
                default:
                  break;
              }

              // -- Validate the adjustment amount is in the format 
              // 999999999999999.99
              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 15), "0123456789") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 15, 1), ".") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 16, 2), "0123456789") == 0
                )
              {
                // -- Adjustment amount format is valid.  Add the adjustment 
                // amount to the running adjustment total.
                local.Common.TotalCurrency =
                  StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 15)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count + 16, 2)) / (decimal)100;
                local.Jrecord.TotalCurrency += local.Common.TotalCurrency;
              }
              else
              {
                // -- Adjustment amount is not in a 99999999999999.99 format.
                local.EabReportSend.RptDetail =
                  "Invalid format for adjustment amount " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 18) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              local.Common.TotalCurrency = -local.Common.TotalCurrency;

              if (local.FileNumber.Count == 5 || local.FileNumber.Count == 6)
              {
                local.HeldStaleDate.Date =
                  StringToDate(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  81, 4) + "-" + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength, 75,
                  2) + "-" + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength, 78,
                  2));

                if (Lt(local.HeldStaleDate.Date, local.Local1825Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt1825StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt1825HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local1095Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt1095StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt1095HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local365Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt365StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt365HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local180Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt180StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt180HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local30Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt30StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt30HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (Lt(local.HeldStaleDate.Date, local.Local2Date.Date))
                {
                  if (local.FileNumber.Count == 5)
                  {
                    local.Gt2StaleAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }
                  else
                  {
                    local.Gt2HeldAmt.TotalCurrency += local.Common.
                      TotalCurrency;
                  }

                  break;
                }

                if (local.FileNumber.Count == 5)
                {
                  local.LbdStaleAmt.TotalCurrency += local.Common.TotalCurrency;
                }
                else
                {
                  local.LbdHeldAmt.TotalCurrency += local.Common.TotalCurrency;
                }
              }
            }

            break;
          case 'T':
            // -------------------------------------------------------------------------------------------
            // -- Footer Record
            // -------------------------------------------------------------------------------------------
            ++local.Trecord.Count;

            if (local.FileNumber.Count == 4)
            {
              // -- Invalid record type for the UNIDENTIFIED PAYMENTS file.
              local.EabReportSend.RptDetail = "Invalid record type " + local
                .RecordType.SelectChar + " found in file " + TrimEnd
                (local.FileName.TextLine80);
              UseCabErrorReport1();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
            }
            else
            {
              // -- Validate the total detail count is in the format 999999999
              local.AlocalStartingPosition.Count = 11;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 9), "0123456789") == 0)
              {
                // -- Total detail count format is valid.  Convert to numeric.
                local.TrecordDetailCount.Count =
                  (int)StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 9));
              }
              else
              {
                // -- Total detail count is not in a 999999999 format.
                local.EabReportSend.RptDetail =
                  "Invalid format for TOTAL detail count " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 9) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              // -- Validate the total payment amount is in the format 
              // 999999999999999.99
              local.AlocalStartingPosition.Count = 32;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 15), "0123456789") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 15, 1), ".") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 16, 2), "0123456789") == 0
                )
              {
                // -- Total payment amount format is valid.  Convert to numeric.
                local.TrecordPaymentAmount.TotalCurrency =
                  StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 15)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count + 16, 2)) / (decimal)100;
              }
              else
              {
                // -- Total payment amount is not in a 99999999999999.99 format.
                local.EabReportSend.RptDetail =
                  "Invalid format for TOTAL payment amount " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 18) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }

              // -- Validate the total adjustment amount is in the format 
              // 999999999999999.99
              local.AlocalStartingPosition.Count = 62;

              if (Verify(Substring(
                local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count, 15), "0123456789") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 15, 1), ".") == 0 && Verify
                (Substring(
                  local.External.TextLine80, External.TextLine80_MaxLength,
                local.AlocalStartingPosition.Count + 16, 2), "0123456789") == 0
                )
              {
                // -- Total adjustment amount format is valid.  Convert to 
                // numeric.
                local.TrecordAdjustmentAmount.TotalCurrency =
                  StringToNumber(Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 15)) + StringToNumber
                  (Substring(
                    local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count + 16, 2)) / (decimal)100;
              }
              else
              {
                // -- Total adjustment amount is not in a 99999999999999.99 
                // format.
                local.EabReportSend.RptDetail =
                  "Invalid format for TOTAL adjustment amount " + Substring
                  (local.External.TextLine80, External.TextLine80_MaxLength,
                  local.AlocalStartingPosition.Count, 18) + " in file " + local
                  .FileName.TextLine80;
                UseCabErrorReport1();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
              }
            }

            break;
          default:
            // -------------------------------------------------------------------------------------------
            // -- Invalid record type.
            // -------------------------------------------------------------------------------------------
            local.EabReportSend.RptDetail = "Invalid record type " + local
              .RecordType.SelectChar + " found in file " + TrimEnd
              (local.FileName.TextLine80);
            UseCabErrorReport1();

            // Set Abort exit state.
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
        }
      }
      while(!Equal(local.External.TextReturnCode, "EF"));

      // -------------------------------------------------------------------------------------------
      // -- Verify there was one and only one Header record.
      // -------------------------------------------------------------------------------------------
      switch(local.Arecord.Count)
      {
        case 0:
          // -- Error.  No Header record found in the file.
          local.EabReportSend.RptDetail =
            "No Header (A record) found in file " + local.FileName.TextLine80;
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        case 1:
          break;
        default:
          // -- Error.  More than one Header record found in the file.
          local.EabReportSend.RptDetail =
            "More than one Header (A record) found in file " + local
            .FileName.TextLine80;
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
      }

      // -------------------------------------------------------------------------------------------
      // -- Verify there was one and only one Detail record for the UNIDENTIFIED
      // PAYMENTS file.
      // -------------------------------------------------------------------------------------------
      if (local.FileNumber.Count == 4)
      {
        switch(local.Brecord.Count)
        {
          case 0:
            // -- Error.  No Detail record found in the file.
            local.EabReportSend.RptDetail =
              "No Detail (B record) found in file " + local
              .FileName.TextLine80;
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          case 1:
            // -- Continue.
            break;
          default:
            // -- Error.  More than one Detail record found in the file.  The 
            // UNIDENTIFIED PAYMENTS file should contain only one B record.
            local.EabReportSend.RptDetail =
              "More than one Header (B record) found in file " + local
              .FileName.TextLine80;
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
        }
      }

      // -------------------------------------------------------------------------------------------
      // -- Verify there was one and only one Footer record.
      // -------------------------------------------------------------------------------------------
      switch(local.Trecord.Count)
      {
        case 0:
          if (local.FileNumber.Count == 4)
          {
            // -- Continue...  The UNIDENTIFIED PAYMENTS file does not have a 
            // footer record.
          }
          else
          {
            // -- Error.  No Footer record found in the file.
            local.EabReportSend.RptDetail =
              "No Footer (T record) found in file " + local
              .FileName.TextLine80;
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          break;
        case 1:
          // -------------------------------------------------------------------------------------------
          // -- Compare T record totals with calculated totals.
          // -------------------------------------------------------------------------------------------
          // -- Compare total detail count on footer record to calculated 
          // amount.
          if (local.TrecordDetailCount.Count != local.Brecord.Count + local
            .Jrecord.Count)
          {
            local.EabReportSend.RptDetail =
              "Footer record total detail count " + NumberToString
              (local.TrecordDetailCount.Count, 7, 9);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " does not match calculated detail count " +
              NumberToString
              ((long)local.Brecord.Count + local.Jrecord.Count, 7, 9);
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          // -- Compare total payment amount on footer record to calculated 
          // amount.
          if (local.TrecordPaymentAmount.TotalCurrency != local
            .Brecord.TotalCurrency)
          {
            local.EabReportSend.RptDetail =
              "Footer record total payment amount " + NumberToString
              ((long)local.TrecordPaymentAmount.TotalCurrency, 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ".";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              ((long)(local.TrecordPaymentAmount.TotalCurrency * 100), 14, 2);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " does not match calculated payment amount ";
              
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
              ((long)local.Brecord.TotalCurrency, 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ".";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              ((long)(local.Brecord.TotalCurrency * 100), 14, 2);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " in file " + local
              .FileName.TextLine80;
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          // -- Compare total adjustment amount on footer record to calculated 
          // amount.
          if (local.TrecordAdjustmentAmount.TotalCurrency != local
            .Jrecord.TotalCurrency)
          {
            local.EabReportSend.RptDetail =
              "Footer record total adjustment amount " + NumberToString
              ((long)local.TrecordAdjustmentAmount.TotalCurrency, 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ".";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              ((long)(local.TrecordAdjustmentAmount.TotalCurrency * 100), 14, 2);
              
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " does not match calculated adjustment amount ";
              
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " " + NumberToString
              ((long)local.Jrecord.TotalCurrency, 15);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ".";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              ((long)(local.Jrecord.TotalCurrency * 100), 14, 2);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " in file " + local
              .FileName.TextLine80;
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
          }

          break;
        default:
          // -- Error.  More than one Footer record found in the file.
          local.EabReportSend.RptDetail =
            "More than one Footer (T record) found in file " + local
            .FileName.TextLine80;
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
      }

      // -------------------------------------------------------------------------------------------
      // -- Write the amount to the OCSE34 record.
      // -------------------------------------------------------------------------------------------
      if (!ReadOcse34())
      {
        local.BatchTimestampWorkArea.IefTimestamp =
          local.Ocse34.CreatedTimestamp;
        UseLeCabConvertTimestamp();
        local.EabReportSend.RptDetail =
          "Error reading OCSE34 record for created timestamp " + local
          .BatchTimestampWorkArea.TextTimestamp;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        continue;
      }

      // -------------------------------------------------------------------------------------------
      // -- Set the appropriate local OCSE34 attribute...
      // -------------------------------------------------------------------------------------------
      switch(local.FileNumber.Count)
      {
        case 1:
          // NON-IVD, IWO COLLECTIONS
          local.Ocse34.KpcNon4DIwoCollAmt =
            (int?)Math.Round(
              local.Brecord.TotalCurrency -
            local.Jrecord.TotalCurrency, MidpointRounding.AwayFromZero);

          try
          {
            UpdateOcse4();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OCSE34_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OCSE34_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          break;
        case 2:
          // IVD, NON-IWO COLLECTIONS
          // -- Processing in Step 2 of B700 ultimately causes the IVD, Non-IWO 
          // amount to be added to Line 2F and subtracted from Line 2H of the
          // report.
          local.Ocse34.KpcIvdNonIwoCollAmt =
            (int?)Math.Round(
              local.Brecord.TotalCurrency -
            local.Jrecord.TotalCurrency, MidpointRounding.AwayFromZero);

          try
          {
            UpdateOcse2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OCSE34_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OCSE34_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          break;
        case 3:
          // NON IVD, IWO FORWARDED TO PAYEES
          local.Ocse34.KpcNonIvdIwoForwCollAmt =
            (int?)Math.Round(
              local.Brecord.TotalCurrency -
            local.Jrecord.TotalCurrency, MidpointRounding.AwayFromZero);

          try
          {
            UpdateOcse5();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OCSE34_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OCSE34_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          break;
        case 4:
          // UNIDENTIFIED PAYMENTS
          // -- The OCSE34 attributes for this file were set during processing 
          // for the B type record.  Continue.
          // added code to only age on percentage of UI   5/19/2005  pr 244780
          try
          {
            UpdateOcse7();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OCSE34_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OCSE34_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          break;
        case 5:
          // STALE DATED LISTING
          local.Ocse34.KpcStaleDateAmt =
            (int?)Math.Round(
              local.Brecord.TotalCurrency -
            local.Jrecord.TotalCurrency, MidpointRounding.AwayFromZero);

          try
          {
            UpdateOcse6();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OCSE34_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OCSE34_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          break;
        case 6:
          // HELD DISBURSEMENT LISTING
          local.Ocse34.KpcHeldDisbAmt =
            (int?)Math.Round(
              local.Brecord.TotalCurrency -
            local.Jrecord.TotalCurrency, MidpointRounding.AwayFromZero);

          try
          {
            UpdateOcse1();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OCSE34_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OCSE34_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          break;
        case 7:
          // LAST DAYS BUSINESS
          local.Ocse34.KpcNivdIwoLda =
            (int?)Math.Round(
              local.Brecord.TotalCurrency -
            local.Jrecord.TotalCurrency, MidpointRounding.AwayFromZero);

          try
          {
            UpdateOcse3();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OCSE34_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "OCSE34_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          break;
        default:
          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Error updating OCSE34 record..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        continue;
      }

      // -------------------------------------------------------------------------------------------
      // -- Close the KPC extract file.
      // -------------------------------------------------------------------------------------------
      local.External.FileInstruction = "CLOSE";
      UseFnB721ExtReadFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        local.EabReportSend.RptDetail = "Error closing " + TrimEnd
          (local.FileName.TextLine80) + " file.";
        UseCabErrorReport1();

        // Set Abort exit state and go to the next file.
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        continue;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        continue;
      }

      // -------------------------------------------------------------------------------------------
      // -- Log summary info to the control report.
      // -------------------------------------------------------------------------------------------
      for(local.ControlReportLine.Count = 1; local.ControlReportLine.Count <= 9
        ; ++local.ControlReportLine.Count)
      {
        if (local.FileNumber.Count == 4)
        {
          switch(local.ControlReportLine.Count)
          {
            case 1:
              local.EabReportSend.RptDetail = "     " + TrimEnd
                (local.FileName.TextLine80) + " file:";

              break;
            case 2:
              local.EabReportSend.RptDetail =
                "                                           Percent       Amount";
                

              break;
            case 3:
              // Total UI Amount
              local.EabReportSend.RptDetail =
                "          Total UI.........................             " + NumberToString
                ((long)local.TotalUiAmount.TotalCurrency, 15) + "." + NumberToString
                ((long)(local.TotalUiAmount.TotalCurrency * 100), 14, 2);

              break;
            case 4:
              // Total Non-IVD, IWO and IVD %
              local.EabReportSend.RptDetail =
                "          Total Non-IVD, IWO and IVD.......      " + local
                .NonIvdIwoAndIvd.Text4 + "   " + NumberToString
                (local.Ocse34.KpcUiNonIvdIwoAmt.GetValueOrDefault(), 15) + ".00"
                ;

              break;
            case 5:
              // Total UI Qtr Amount
              local.EabReportSend.RptDetail =
                "          Total UI Quarter.................             " + NumberToString
                ((long)local.TotalUiQuarterlyAmount.TotalCurrency, 15) + "." + NumberToString
                ((long)(local.TotalUiQuarterlyAmount.TotalCurrency * 100), 14, 2);
                

              break;
            case 6:
              // IVD, IWO and Non-IVD, IWO %
              local.EabReportSend.RptDetail =
                "          IVD, IWO and Non-IVD, IWO........      " + local
                .IvdIwoAndNonIvdIwo.Text4 + "   " + NumberToString
                (local.Ocse34.KpcUiIvdNivdIwoAmt.GetValueOrDefault(), 15) + ".00"
                ;

              break;
            case 7:
              // IVD, Interstate, Non-IWO %
              local.EabReportSend.RptDetail =
                "          IVD, Interstate, Non-IWO.........      " + local
                .IvdInterstateNonIwo.Text4 + "   " + NumberToString
                (local.Ocse34.UiIvdNonIwoIntAmt.GetValueOrDefault(), 15) + ".00"
                ;

              break;
            case 8:
              // IVD Non-Interstate, Non-IWO %
              local.EabReportSend.RptDetail =
                "          IVD Non-Interstate, Non-IWO......      " + local
                .IvdNonInterstateNonIwo.Text4 + "   " + NumberToString
                (local.Ocse34.KpcUiIvdNonIwoNonIntAmt.GetValueOrDefault(), 15) +
                ".00";

              break;
            case 9:
              local.EabReportSend.RptDetail = "";

              break;
            default:
              continue;
          }
        }
        else
        {
          switch(local.ControlReportLine.Count)
          {
            case 1:
              local.EabReportSend.RptDetail = "     " + TrimEnd
                (local.FileName.TextLine80) + " file:";

              break;
            case 2:
              local.EabReportSend.RptDetail =
                "                                           Count         Amount";
                

              break;
            case 3:
              // Total Payments
              local.EabReportSend.RptDetail =
                "          Total Payments..................." + NumberToString
                (local.Brecord.Count, 7, 9) + "    " + NumberToString
                ((long)local.Brecord.TotalCurrency, 15) + "." + NumberToString
                ((long)(local.Brecord.TotalCurrency * 100), 14, 2);

              break;
            case 4:
              // Total Adjustments
              local.EabReportSend.RptDetail =
                "          Total Adjustments................" + NumberToString
                (local.Jrecord.Count, 7, 9) + "    " + NumberToString
                ((long)local.Jrecord.TotalCurrency, 15) + "." + NumberToString
                ((long)(local.Jrecord.TotalCurrency * 100), 14, 2);

              break;
            case 5:
              // Total Net
              local.EabReportSend.RptDetail =
                "          Total Net........................" + NumberToString
                ((long)local.Brecord.Count + local.Jrecord.Count, 7, 9) + "    " +
                NumberToString((long)(local.Brecord.TotalCurrency - local
                .Jrecord.TotalCurrency), 15) + "." + NumberToString
                ((long)((local.Brecord.TotalCurrency - local
                .Jrecord.TotalCurrency) * 100), 14, 2);

              break;
            case 6:
              local.EabReportSend.RptDetail = "";

              break;
            default:
              continue;
          }
        }

        UseCabControlReport1();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          goto AfterCycle;
        }
      }

      // -------------------------------------------------------------------------------------------
      // -- Take a Checkpoint.
      // -------------------------------------------------------------------------------------------
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      UseReadPgmCheckpointRestart();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      local.ProgramCheckpointRestart.RestartInd = "Y";
      local.ProgramCheckpointRestart.RestartInfo =
        NumberToString((long)local.FileNumber.Count + 1, 14, 2) + " " + Substring
        (local.ProgramCheckpointRestart.RestartInfo, 250, 4, 247);
      UseUpdateCheckpointRstAndCommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

Next:
      ;
    }

AfterCycle:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (!IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Error Message..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport2();
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Take a Final Checkpoint.
    // -------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Error Taking Final Checkpoint..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Close the Control report.
    // -------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      // Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Close the Error report.
    // -------------------------------------------------------------------------------------------
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveExternal(External source, External target)
  {
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveOcse34(Ocse34 source, Ocse34 target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReportingPeriodEndDate = source.ReportingPeriodEndDate;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Write.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Write.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseFnB721ExtReadFile()
  {
    var useImport = new FnB721ExtReadFile.Import();
    var useExport = new FnB721ExtReadFile.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useImport.FileNumber.Count = local.FileNumber.Count;
    MoveExternal(local.External, useExport.External);

    Call(FnB721ExtReadFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseFnB722BatchInitialization()
  {
    var useImport = new FnB722BatchInitialization.Import();
    var useExport = new FnB722BatchInitialization.Export();

    Call(FnB722BatchInitialization.Execute, useImport, useExport);

    local.StartingFileNumber.Count = useExport.RestartFileNumber.Count;
    MoveOcse34(useExport.Ocse34, local.Ocse34);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    local.BatchTimestampWorkArea.Assign(useExport.BatchTimestampWorkArea);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadOcse34()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse34",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.Ocse34.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ocse34.Period = db.GetInt32(reader, 0);
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Ocse34.ReportingPeriodBeginDate =
          db.GetNullableDate(reader, 2);
        entities.Ocse34.ReportingPeriodEndDate = db.GetNullableDate(reader, 3);
        entities.Ocse34.KpcNon4DIwoCollAmt = db.GetNullableInt32(reader, 4);
        entities.Ocse34.KpcIvdNonIwoCollAmt = db.GetNullableInt32(reader, 5);
        entities.Ocse34.KpcNonIvdIwoForwCollAmt =
          db.GetNullableInt32(reader, 6);
        entities.Ocse34.KpcStaleDateAmt = db.GetNullableInt32(reader, 7);
        entities.Ocse34.KpcHeldDisbAmt = db.GetNullableInt32(reader, 8);
        entities.Ocse34.UiIvdNonIwoIntAmt = db.GetNullableInt32(reader, 9);
        entities.Ocse34.KpcUiIvdNonIwoNonIntAmt =
          db.GetNullableInt32(reader, 10);
        entities.Ocse34.KpcUiIvdNivdIwoAmt = db.GetNullableInt32(reader, 11);
        entities.Ocse34.KpcUiNonIvdIwoAmt = db.GetNullableInt32(reader, 12);
        entities.Ocse34.KpcNivdIwoLda = db.GetNullableInt32(reader, 13);
        entities.Ocse34.HeldLda = db.GetNullableInt32(reader, 14);
        entities.Ocse34.HeldGt2 = db.GetNullableInt32(reader, 15);
        entities.Ocse34.HeldGt30 = db.GetNullableInt32(reader, 16);
        entities.Ocse34.HeldGt180 = db.GetNullableInt32(reader, 17);
        entities.Ocse34.HeldGt365 = db.GetNullableInt32(reader, 18);
        entities.Ocse34.HeldGt1095 = db.GetNullableInt32(reader, 19);
        entities.Ocse34.HeldGt1825 = db.GetNullableInt32(reader, 20);
        entities.Ocse34.StaleLda = db.GetNullableInt32(reader, 21);
        entities.Ocse34.StaleGt2 = db.GetNullableInt32(reader, 22);
        entities.Ocse34.StaleGt30 = db.GetNullableInt32(reader, 23);
        entities.Ocse34.StaleGt180 = db.GetNullableInt32(reader, 24);
        entities.Ocse34.StaleGt365 = db.GetNullableInt32(reader, 25);
        entities.Ocse34.StaleGt1095 = db.GetNullableInt32(reader, 26);
        entities.Ocse34.StaleGt1825 = db.GetNullableInt32(reader, 27);
        entities.Ocse34.KpcUiLda = db.GetNullableInt32(reader, 28);
        entities.Ocse34.KpcUiGt2 = db.GetNullableInt32(reader, 29);
        entities.Ocse34.KpcUiGt30 = db.GetNullableInt32(reader, 30);
        entities.Ocse34.KpcUiGt180 = db.GetNullableInt32(reader, 31);
        entities.Ocse34.KpcUiGt365 = db.GetNullableInt32(reader, 32);
        entities.Ocse34.KpcUiGt1095 = db.GetNullableInt32(reader, 33);
        entities.Ocse34.KpcUiGt1825 = db.GetNullableInt32(reader, 34);
        entities.Ocse34.Populated = true;
      });
  }

  private void UpdateOcse1()
  {
    var kpcHeldDisbAmt = local.Ocse34.KpcHeldDisbAmt.GetValueOrDefault();
    var heldLda = (int?)local.LbdHeldAmt.TotalCurrency;
    var heldGt2 = (int?)local.Gt2HeldAmt.TotalCurrency;
    var heldGt30 = (int?)local.Gt30HeldAmt.TotalCurrency;
    var heldGt180 = (int?)local.Gt180HeldAmt.TotalCurrency;
    var heldGt365 = (int?)local.Gt365HeldAmt.TotalCurrency;
    var heldGt1095 = (int?)local.Gt1095HeldAmt.TotalCurrency;
    var heldGt1825 = (int?)local.Gt1825HeldAmt.TotalCurrency;

    entities.Ocse34.Populated = false;
    Update("UpdateOcse1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "kpcHldDsbAmt", kpcHeldDisbAmt);
        db.SetNullableInt32(command, "heldLda", heldLda);
        db.SetNullableInt32(command, "heldGt2", heldGt2);
        db.SetNullableInt32(command, "heldGt30", heldGt30);
        db.SetNullableInt32(command, "heldGt180", heldGt180);
        db.SetNullableInt32(command, "heldGt365", heldGt365);
        db.SetNullableInt32(command, "heldGt1095", heldGt1095);
        db.SetNullableInt32(command, "heldGt1825", heldGt1825);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.KpcHeldDisbAmt = kpcHeldDisbAmt;
    entities.Ocse34.HeldLda = heldLda;
    entities.Ocse34.HeldGt2 = heldGt2;
    entities.Ocse34.HeldGt30 = heldGt30;
    entities.Ocse34.HeldGt180 = heldGt180;
    entities.Ocse34.HeldGt365 = heldGt365;
    entities.Ocse34.HeldGt1095 = heldGt1095;
    entities.Ocse34.HeldGt1825 = heldGt1825;
    entities.Ocse34.Populated = true;
  }

  private void UpdateOcse2()
  {
    var kpcIvdNonIwoCollAmt =
      local.Ocse34.KpcIvdNonIwoCollAmt.GetValueOrDefault();

    entities.Ocse34.Populated = false;
    Update("UpdateOcse2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "kpcIvdNiwoAmt", kpcIvdNonIwoCollAmt);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.KpcIvdNonIwoCollAmt = kpcIvdNonIwoCollAmt;
    entities.Ocse34.Populated = true;
  }

  private void UpdateOcse3()
  {
    var kpcNivdIwoLda = local.Ocse34.KpcNivdIwoLda.GetValueOrDefault();

    entities.Ocse34.Populated = false;
    Update("UpdateOcse3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "kpcNivdIwoLda", kpcNivdIwoLda);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.KpcNivdIwoLda = kpcNivdIwoLda;
    entities.Ocse34.Populated = true;
  }

  private void UpdateOcse4()
  {
    var kpcNon4DIwoCollAmt =
      local.Ocse34.KpcNon4DIwoCollAmt.GetValueOrDefault();

    entities.Ocse34.Populated = false;
    Update("UpdateOcse4",
      (db, command) =>
      {
        db.SetNullableInt32(command, "kpcNivdIwoAmt", kpcNon4DIwoCollAmt);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.KpcNon4DIwoCollAmt = kpcNon4DIwoCollAmt;
    entities.Ocse34.Populated = true;
  }

  private void UpdateOcse5()
  {
    var kpcNonIvdIwoForwCollAmt =
      local.Ocse34.KpcNonIvdIwoForwCollAmt.GetValueOrDefault();

    entities.Ocse34.Populated = false;
    Update("UpdateOcse5",
      (db, command) =>
      {
        db.SetNullableInt32(command, "kpcNivdIwoFamt", kpcNonIvdIwoForwCollAmt);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.KpcNonIvdIwoForwCollAmt = kpcNonIvdIwoForwCollAmt;
    entities.Ocse34.Populated = true;
  }

  private void UpdateOcse6()
  {
    var kpcStaleDateAmt = local.Ocse34.KpcStaleDateAmt.GetValueOrDefault();
    var staleLda = (int?)local.LbdStaleAmt.TotalCurrency;
    var staleGt2 = (int?)local.Gt2StaleAmt.TotalCurrency;
    var staleGt30 = (int?)local.Gt30StaleAmt.TotalCurrency;
    var staleGt180 = (int?)local.Gt180StaleAmt.TotalCurrency;
    var staleGt365 = (int?)local.Gt365StaleAmt.TotalCurrency;
    var staleGt1095 = (int?)local.Gt1095StaleAmt.TotalCurrency;
    var staleGt1825 = (int?)local.Gt1825StaleAmt.TotalCurrency;

    entities.Ocse34.Populated = false;
    Update("UpdateOcse6",
      (db, command) =>
      {
        db.SetNullableInt32(command, "kpcStaleDtAmt", kpcStaleDateAmt);
        db.SetNullableInt32(command, "staleLda", staleLda);
        db.SetNullableInt32(command, "staleGt2", staleGt2);
        db.SetNullableInt32(command, "staleGt30", staleGt30);
        db.SetNullableInt32(command, "staleGt180", staleGt180);
        db.SetNullableInt32(command, "staleGt365", staleGt365);
        db.SetNullableInt32(command, "staleGt1095", staleGt1095);
        db.SetNullableInt32(command, "staleGt1825", staleGt1825);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.KpcStaleDateAmt = kpcStaleDateAmt;
    entities.Ocse34.StaleLda = staleLda;
    entities.Ocse34.StaleGt2 = staleGt2;
    entities.Ocse34.StaleGt30 = staleGt30;
    entities.Ocse34.StaleGt180 = staleGt180;
    entities.Ocse34.StaleGt365 = staleGt365;
    entities.Ocse34.StaleGt1095 = staleGt1095;
    entities.Ocse34.StaleGt1825 = staleGt1825;
    entities.Ocse34.Populated = true;
  }

  private void UpdateOcse7()
  {
    var uiIvdNonIwoIntAmt =
      entities.Ocse34.UiIvdNonIwoIntAmt.GetValueOrDefault() +
      local.Ocse34.UiIvdNonIwoIntAmt.GetValueOrDefault();
    var kpcUiIvdNonIwoNonIntAmt =
      entities.Ocse34.KpcUiIvdNonIwoNonIntAmt.GetValueOrDefault() +
      local.Ocse34.KpcUiIvdNonIwoNonIntAmt.GetValueOrDefault();
    var kpcUiIvdNivdIwoAmt =
      entities.Ocse34.KpcUiIvdNivdIwoAmt.GetValueOrDefault() +
      local.Ocse34.KpcUiIvdNivdIwoAmt.GetValueOrDefault();
    var kpcUiNonIvdIwoAmt =
      entities.Ocse34.KpcUiNonIvdIwoAmt.GetValueOrDefault() +
      local.Ocse34.KpcUiNonIvdIwoAmt.GetValueOrDefault();
    var kpcUiLda = (int)(local.LbdUiAmt.TotalCurrency * (
      (decimal)local.PctUiSaved.Percentage / 100));
    var kpcUiGt2 = (int)(local.Gt2UiAmt.TotalCurrency * (
      (decimal)local.PctUiSaved.Percentage / 100));
    var kpcUiGt30 = (int)(local.Gt30UiAmt.TotalCurrency * (
      (decimal)local.PctUiSaved.Percentage / 100));
    var kpcUiGt180 = (int)(local.Gt180UiAmt.TotalCurrency * (
      (decimal)local.PctUiSaved.Percentage / 100));
    var kpcUiGt365 = (int)(local.Gt365UiAmt.TotalCurrency * (
      (decimal)local.PctUiSaved.Percentage / 100));
    var kpcUiGt1095 = (int)(local.Gt1095UiAmt.TotalCurrency * (
      (decimal)local.PctUiSaved.Percentage / 100));
    var kpcUiGt1825 = (int)(local.Gt1825UiAmt.TotalCurrency * (
      (decimal)local.PctUiSaved.Percentage / 100));

    entities.Ocse34.Populated = false;
    Update("UpdateOcse7",
      (db, command) =>
      {
        db.SetNullableInt32(command, "uiIvdNiwoIntamt", uiIvdNonIwoIntAmt);
        db.SetNullableInt32(command, "uiIvdNiwoNintA", kpcUiIvdNonIwoNonIntAmt);
        db.SetNullableInt32(command, "uiIvdNivdIwoA", kpcUiIvdNivdIwoAmt);
        db.SetNullableInt32(command, "uiNivdIwoAmt", kpcUiNonIvdIwoAmt);
        db.SetNullableInt32(command, "kpcUiLda", kpcUiLda);
        db.SetNullableInt32(command, "kpcUiGt2", kpcUiGt2);
        db.SetNullableInt32(command, "kpcUiGt30", kpcUiGt30);
        db.SetNullableInt32(command, "kpcUiGt180", kpcUiGt180);
        db.SetNullableInt32(command, "kpcUiGt365", kpcUiGt365);
        db.SetNullableInt32(command, "kpcUiGt1095", kpcUiGt1095);
        db.SetNullableInt32(command, "kpcUiGt1825", kpcUiGt1825);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.UiIvdNonIwoIntAmt = uiIvdNonIwoIntAmt;
    entities.Ocse34.KpcUiIvdNonIwoNonIntAmt = kpcUiIvdNonIwoNonIntAmt;
    entities.Ocse34.KpcUiIvdNivdIwoAmt = kpcUiIvdNivdIwoAmt;
    entities.Ocse34.KpcUiNonIvdIwoAmt = kpcUiNonIvdIwoAmt;
    entities.Ocse34.KpcUiLda = kpcUiLda;
    entities.Ocse34.KpcUiGt2 = kpcUiGt2;
    entities.Ocse34.KpcUiGt30 = kpcUiGt30;
    entities.Ocse34.KpcUiGt180 = kpcUiGt180;
    entities.Ocse34.KpcUiGt365 = kpcUiGt365;
    entities.Ocse34.KpcUiGt1095 = kpcUiGt1095;
    entities.Ocse34.KpcUiGt1825 = kpcUiGt1825;
    entities.Ocse34.Populated = true;
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
    /// <summary>
    /// A value of LocalKpcMonthda.
    /// </summary>
    [JsonPropertyName("localKpcMonthda")]
    public TextWorkArea LocalKpcMonthda
    {
      get => localKpcMonthda ??= new();
      set => localKpcMonthda = value;
    }

    private TextWorkArea localKpcMonthda;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PctUiSaved.
    /// </summary>
    [JsonPropertyName("pctUiSaved")]
    public Common PctUiSaved
    {
      get => pctUiSaved ??= new();
      set => pctUiSaved = value;
    }

    /// <summary>
    /// A value of CseCompareDate.
    /// </summary>
    [JsonPropertyName("cseCompareDate")]
    public WorkArea CseCompareDate
    {
      get => cseCompareDate ??= new();
      set => cseCompareDate = value;
    }

    /// <summary>
    /// A value of KpcPreviousCompare.
    /// </summary>
    [JsonPropertyName("kpcPreviousCompare")]
    public WorkArea KpcPreviousCompare
    {
      get => kpcPreviousCompare ??= new();
      set => kpcPreviousCompare = value;
    }

    /// <summary>
    /// A value of HsDatePrevious.
    /// </summary>
    [JsonPropertyName("hsDatePrevious")]
    public DateWorkArea HsDatePrevious
    {
      get => hsDatePrevious ??= new();
      set => hsDatePrevious = value;
    }

    /// <summary>
    /// A value of HsCompareDate.
    /// </summary>
    [JsonPropertyName("hsCompareDate")]
    public WorkArea HsCompareDate
    {
      get => hsCompareDate ??= new();
      set => hsCompareDate = value;
    }

    /// <summary>
    /// A value of CseReportMo.
    /// </summary>
    [JsonPropertyName("cseReportMo")]
    public TextWorkArea CseReportMo
    {
      get => cseReportMo ??= new();
      set => cseReportMo = value;
    }

    /// <summary>
    /// A value of CseReportYear.
    /// </summary>
    [JsonPropertyName("cseReportYear")]
    public TextWorkArea CseReportYear
    {
      get => cseReportYear ??= new();
      set => cseReportYear = value;
    }

    /// <summary>
    /// A value of HsDateYear.
    /// </summary>
    [JsonPropertyName("hsDateYear")]
    public TextWorkArea HsDateYear
    {
      get => hsDateYear ??= new();
      set => hsDateYear = value;
    }

    /// <summary>
    /// A value of HsDateMonth.
    /// </summary>
    [JsonPropertyName("hsDateMonth")]
    public TextWorkArea HsDateMonth
    {
      get => hsDateMonth ??= new();
      set => hsDateMonth = value;
    }

    /// <summary>
    /// A value of PercentageSum.
    /// </summary>
    [JsonPropertyName("percentageSum")]
    public Common PercentageSum
    {
      get => percentageSum ??= new();
      set => percentageSum = value;
    }

    /// <summary>
    /// A value of LbdUiAmt.
    /// </summary>
    [JsonPropertyName("lbdUiAmt")]
    public Common LbdUiAmt
    {
      get => lbdUiAmt ??= new();
      set => lbdUiAmt = value;
    }

    /// <summary>
    /// A value of Gt2UiAmt.
    /// </summary>
    [JsonPropertyName("gt2UiAmt")]
    public Common Gt2UiAmt
    {
      get => gt2UiAmt ??= new();
      set => gt2UiAmt = value;
    }

    /// <summary>
    /// A value of Gt30UiAmt.
    /// </summary>
    [JsonPropertyName("gt30UiAmt")]
    public Common Gt30UiAmt
    {
      get => gt30UiAmt ??= new();
      set => gt30UiAmt = value;
    }

    /// <summary>
    /// A value of Gt180UiAmt.
    /// </summary>
    [JsonPropertyName("gt180UiAmt")]
    public Common Gt180UiAmt
    {
      get => gt180UiAmt ??= new();
      set => gt180UiAmt = value;
    }

    /// <summary>
    /// A value of Gt365UiAmt.
    /// </summary>
    [JsonPropertyName("gt365UiAmt")]
    public Common Gt365UiAmt
    {
      get => gt365UiAmt ??= new();
      set => gt365UiAmt = value;
    }

    /// <summary>
    /// A value of Gt1095UiAmt.
    /// </summary>
    [JsonPropertyName("gt1095UiAmt")]
    public Common Gt1095UiAmt
    {
      get => gt1095UiAmt ??= new();
      set => gt1095UiAmt = value;
    }

    /// <summary>
    /// A value of Gt1825UiAmt.
    /// </summary>
    [JsonPropertyName("gt1825UiAmt")]
    public Common Gt1825UiAmt
    {
      get => gt1825UiAmt ??= new();
      set => gt1825UiAmt = value;
    }

    /// <summary>
    /// A value of LbdStaleAmt.
    /// </summary>
    [JsonPropertyName("lbdStaleAmt")]
    public Common LbdStaleAmt
    {
      get => lbdStaleAmt ??= new();
      set => lbdStaleAmt = value;
    }

    /// <summary>
    /// A value of Gt2StaleAmt.
    /// </summary>
    [JsonPropertyName("gt2StaleAmt")]
    public Common Gt2StaleAmt
    {
      get => gt2StaleAmt ??= new();
      set => gt2StaleAmt = value;
    }

    /// <summary>
    /// A value of Gt30StaleAmt.
    /// </summary>
    [JsonPropertyName("gt30StaleAmt")]
    public Common Gt30StaleAmt
    {
      get => gt30StaleAmt ??= new();
      set => gt30StaleAmt = value;
    }

    /// <summary>
    /// A value of Gt180StaleAmt.
    /// </summary>
    [JsonPropertyName("gt180StaleAmt")]
    public Common Gt180StaleAmt
    {
      get => gt180StaleAmt ??= new();
      set => gt180StaleAmt = value;
    }

    /// <summary>
    /// A value of Gt365StaleAmt.
    /// </summary>
    [JsonPropertyName("gt365StaleAmt")]
    public Common Gt365StaleAmt
    {
      get => gt365StaleAmt ??= new();
      set => gt365StaleAmt = value;
    }

    /// <summary>
    /// A value of Gt1095StaleAmt.
    /// </summary>
    [JsonPropertyName("gt1095StaleAmt")]
    public Common Gt1095StaleAmt
    {
      get => gt1095StaleAmt ??= new();
      set => gt1095StaleAmt = value;
    }

    /// <summary>
    /// A value of Gt1825StaleAmt.
    /// </summary>
    [JsonPropertyName("gt1825StaleAmt")]
    public Common Gt1825StaleAmt
    {
      get => gt1825StaleAmt ??= new();
      set => gt1825StaleAmt = value;
    }

    /// <summary>
    /// A value of LbdHeldAmt.
    /// </summary>
    [JsonPropertyName("lbdHeldAmt")]
    public Common LbdHeldAmt
    {
      get => lbdHeldAmt ??= new();
      set => lbdHeldAmt = value;
    }

    /// <summary>
    /// A value of Gt2HeldAmt.
    /// </summary>
    [JsonPropertyName("gt2HeldAmt")]
    public Common Gt2HeldAmt
    {
      get => gt2HeldAmt ??= new();
      set => gt2HeldAmt = value;
    }

    /// <summary>
    /// A value of Gt30HeldAmt.
    /// </summary>
    [JsonPropertyName("gt30HeldAmt")]
    public Common Gt30HeldAmt
    {
      get => gt30HeldAmt ??= new();
      set => gt30HeldAmt = value;
    }

    /// <summary>
    /// A value of Gt180HeldAmt.
    /// </summary>
    [JsonPropertyName("gt180HeldAmt")]
    public Common Gt180HeldAmt
    {
      get => gt180HeldAmt ??= new();
      set => gt180HeldAmt = value;
    }

    /// <summary>
    /// A value of Gt365HeldAmt.
    /// </summary>
    [JsonPropertyName("gt365HeldAmt")]
    public Common Gt365HeldAmt
    {
      get => gt365HeldAmt ??= new();
      set => gt365HeldAmt = value;
    }

    /// <summary>
    /// A value of Gt1095HeldAmt.
    /// </summary>
    [JsonPropertyName("gt1095HeldAmt")]
    public Common Gt1095HeldAmt
    {
      get => gt1095HeldAmt ??= new();
      set => gt1095HeldAmt = value;
    }

    /// <summary>
    /// A value of Gt1825HeldAmt.
    /// </summary>
    [JsonPropertyName("gt1825HeldAmt")]
    public Common Gt1825HeldAmt
    {
      get => gt1825HeldAmt ??= new();
      set => gt1825HeldAmt = value;
    }

    /// <summary>
    /// A value of HeldStaleDate.
    /// </summary>
    [JsonPropertyName("heldStaleDate")]
    public DateWorkArea HeldStaleDate
    {
      get => heldStaleDate ??= new();
      set => heldStaleDate = value;
    }

    /// <summary>
    /// A value of KpcHsDateOcse34.
    /// </summary>
    [JsonPropertyName("kpcHsDateOcse34")]
    public Ocse34 KpcHsDateOcse34
    {
      get => kpcHsDateOcse34 ??= new();
      set => kpcHsDateOcse34 = value;
    }

    /// <summary>
    /// A value of KpcHsCompare.
    /// </summary>
    [JsonPropertyName("kpcHsCompare")]
    public TextWorkArea KpcHsCompare
    {
      get => kpcHsCompare ??= new();
      set => kpcHsCompare = value;
    }

    /// <summary>
    /// A value of StaleFlag.
    /// </summary>
    [JsonPropertyName("staleFlag")]
    public Common StaleFlag
    {
      get => staleFlag ??= new();
      set => staleFlag = value;
    }

    /// <summary>
    /// A value of HeldFlag.
    /// </summary>
    [JsonPropertyName("heldFlag")]
    public Common HeldFlag
    {
      get => heldFlag ??= new();
      set => heldFlag = value;
    }

    /// <summary>
    /// A value of KpcHsDateTextWorkArea.
    /// </summary>
    [JsonPropertyName("kpcHsDateTextWorkArea")]
    public TextWorkArea KpcHsDateTextWorkArea
    {
      get => kpcHsDateTextWorkArea ??= new();
      set => kpcHsDateTextWorkArea = value;
    }

    /// <summary>
    /// A value of KpcPeriod.
    /// </summary>
    [JsonPropertyName("kpcPeriod")]
    public TextWorkArea KpcPeriod
    {
      get => kpcPeriod ??= new();
      set => kpcPeriod = value;
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
    /// A value of IvdNonInterstateNonIwo.
    /// </summary>
    [JsonPropertyName("ivdNonInterstateNonIwo")]
    public TextWorkArea IvdNonInterstateNonIwo
    {
      get => ivdNonInterstateNonIwo ??= new();
      set => ivdNonInterstateNonIwo = value;
    }

    /// <summary>
    /// A value of IvdInterstateNonIwo.
    /// </summary>
    [JsonPropertyName("ivdInterstateNonIwo")]
    public TextWorkArea IvdInterstateNonIwo
    {
      get => ivdInterstateNonIwo ??= new();
      set => ivdInterstateNonIwo = value;
    }

    /// <summary>
    /// A value of IvdIwoAndNonIvdIwo.
    /// </summary>
    [JsonPropertyName("ivdIwoAndNonIvdIwo")]
    public TextWorkArea IvdIwoAndNonIvdIwo
    {
      get => ivdIwoAndNonIvdIwo ??= new();
      set => ivdIwoAndNonIvdIwo = value;
    }

    /// <summary>
    /// A value of NonIvdIwoAndIvd.
    /// </summary>
    [JsonPropertyName("nonIvdIwoAndIvd")]
    public TextWorkArea NonIvdIwoAndIvd
    {
      get => nonIvdIwoAndIvd ??= new();
      set => nonIvdIwoAndIvd = value;
    }

    /// <summary>
    /// A value of AlocalStartingPosition.
    /// </summary>
    [JsonPropertyName("alocalStartingPosition")]
    public Common AlocalStartingPosition
    {
      get => alocalStartingPosition ??= new();
      set => alocalStartingPosition = value;
    }

    /// <summary>
    /// A value of ControlReportLine.
    /// </summary>
    [JsonPropertyName("controlReportLine")]
    public Common ControlReportLine
    {
      get => controlReportLine ??= new();
      set => controlReportLine = value;
    }

    /// <summary>
    /// A value of StartingFileNumber.
    /// </summary>
    [JsonPropertyName("startingFileNumber")]
    public Common StartingFileNumber
    {
      get => startingFileNumber ??= new();
      set => startingFileNumber = value;
    }

    /// <summary>
    /// A value of Pct.
    /// </summary>
    [JsonPropertyName("pct")]
    public Common Pct
    {
      get => pct ??= new();
      set => pct = value;
    }

    /// <summary>
    /// A value of TotalUiQuarterlyAmount.
    /// </summary>
    [JsonPropertyName("totalUiQuarterlyAmount")]
    public Common TotalUiQuarterlyAmount
    {
      get => totalUiQuarterlyAmount ??= new();
      set => totalUiQuarterlyAmount = value;
    }

    /// <summary>
    /// A value of TotalUiAmount.
    /// </summary>
    [JsonPropertyName("totalUiAmount")]
    public Common TotalUiAmount
    {
      get => totalUiAmount ??= new();
      set => totalUiAmount = value;
    }

    /// <summary>
    /// A value of TrecordDetailCount.
    /// </summary>
    [JsonPropertyName("trecordDetailCount")]
    public Common TrecordDetailCount
    {
      get => trecordDetailCount ??= new();
      set => trecordDetailCount = value;
    }

    /// <summary>
    /// A value of TrecordAdjustmentAmount.
    /// </summary>
    [JsonPropertyName("trecordAdjustmentAmount")]
    public Common TrecordAdjustmentAmount
    {
      get => trecordAdjustmentAmount ??= new();
      set => trecordAdjustmentAmount = value;
    }

    /// <summary>
    /// A value of TrecordPaymentAmount.
    /// </summary>
    [JsonPropertyName("trecordPaymentAmount")]
    public Common TrecordPaymentAmount
    {
      get => trecordPaymentAmount ??= new();
      set => trecordPaymentAmount = value;
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
    /// A value of KpcDate.
    /// </summary>
    [JsonPropertyName("kpcDate")]
    public TextWorkArea KpcDate
    {
      get => kpcDate ??= new();
      set => kpcDate = value;
    }

    /// <summary>
    /// A value of CseDate.
    /// </summary>
    [JsonPropertyName("cseDate")]
    public TextWorkArea CseDate
    {
      get => cseDate ??= new();
      set => cseDate = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    /// <summary>
    /// A value of FileName.
    /// </summary>
    [JsonPropertyName("fileName")]
    public External FileName
    {
      get => fileName ??= new();
      set => fileName = value;
    }

    /// <summary>
    /// A value of Trecord.
    /// </summary>
    [JsonPropertyName("trecord")]
    public Common Trecord
    {
      get => trecord ??= new();
      set => trecord = value;
    }

    /// <summary>
    /// A value of Jrecord.
    /// </summary>
    [JsonPropertyName("jrecord")]
    public Common Jrecord
    {
      get => jrecord ??= new();
      set => jrecord = value;
    }

    /// <summary>
    /// A value of Brecord.
    /// </summary>
    [JsonPropertyName("brecord")]
    public Common Brecord
    {
      get => brecord ??= new();
      set => brecord = value;
    }

    /// <summary>
    /// A value of Arecord.
    /// </summary>
    [JsonPropertyName("arecord")]
    public Common Arecord
    {
      get => arecord ??= new();
      set => arecord = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public Common FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabFileHandling Write
    {
      get => write ??= new();
      set => write = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Local2Date.
    /// </summary>
    [JsonPropertyName("local2Date")]
    public DateWorkArea Local2Date
    {
      get => local2Date ??= new();
      set => local2Date = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of Local30Date.
    /// </summary>
    [JsonPropertyName("local30Date")]
    public DateWorkArea Local30Date
    {
      get => local30Date ??= new();
      set => local30Date = value;
    }

    /// <summary>
    /// A value of Local180Date.
    /// </summary>
    [JsonPropertyName("local180Date")]
    public DateWorkArea Local180Date
    {
      get => local180Date ??= new();
      set => local180Date = value;
    }

    /// <summary>
    /// A value of Local365Date.
    /// </summary>
    [JsonPropertyName("local365Date")]
    public DateWorkArea Local365Date
    {
      get => local365Date ??= new();
      set => local365Date = value;
    }

    /// <summary>
    /// A value of Local1095Date.
    /// </summary>
    [JsonPropertyName("local1095Date")]
    public DateWorkArea Local1095Date
    {
      get => local1095Date ??= new();
      set => local1095Date = value;
    }

    /// <summary>
    /// A value of Local1825Date.
    /// </summary>
    [JsonPropertyName("local1825Date")]
    public DateWorkArea Local1825Date
    {
      get => local1825Date ??= new();
      set => local1825Date = value;
    }

    private Common pctUiSaved;
    private WorkArea cseCompareDate;
    private WorkArea kpcPreviousCompare;
    private DateWorkArea hsDatePrevious;
    private WorkArea hsCompareDate;
    private TextWorkArea cseReportMo;
    private TextWorkArea cseReportYear;
    private TextWorkArea hsDateYear;
    private TextWorkArea hsDateMonth;
    private Common percentageSum;
    private Common lbdUiAmt;
    private Common gt2UiAmt;
    private Common gt30UiAmt;
    private Common gt180UiAmt;
    private Common gt365UiAmt;
    private Common gt1095UiAmt;
    private Common gt1825UiAmt;
    private Common lbdStaleAmt;
    private Common gt2StaleAmt;
    private Common gt30StaleAmt;
    private Common gt180StaleAmt;
    private Common gt365StaleAmt;
    private Common gt1095StaleAmt;
    private Common gt1825StaleAmt;
    private Common lbdHeldAmt;
    private Common gt2HeldAmt;
    private Common gt30HeldAmt;
    private Common gt180HeldAmt;
    private Common gt365HeldAmt;
    private Common gt1095HeldAmt;
    private Common gt1825HeldAmt;
    private DateWorkArea heldStaleDate;
    private Ocse34 kpcHsDateOcse34;
    private TextWorkArea kpcHsCompare;
    private Common staleFlag;
    private Common heldFlag;
    private TextWorkArea kpcHsDateTextWorkArea;
    private TextWorkArea kpcPeriod;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private TextWorkArea ivdNonInterstateNonIwo;
    private TextWorkArea ivdInterstateNonIwo;
    private TextWorkArea ivdIwoAndNonIvdIwo;
    private TextWorkArea nonIvdIwoAndIvd;
    private Common alocalStartingPosition;
    private Common controlReportLine;
    private Common startingFileNumber;
    private Common pct;
    private Common totalUiQuarterlyAmount;
    private Common totalUiAmount;
    private Common trecordDetailCount;
    private Common trecordAdjustmentAmount;
    private Common trecordPaymentAmount;
    private Common common;
    private TextWorkArea kpcDate;
    private TextWorkArea cseDate;
    private Ocse34 ocse34;
    private External fileName;
    private Common trecord;
    private Common jrecord;
    private Common brecord;
    private Common arecord;
    private Common recordType;
    private External external;
    private Common fileNumber;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling write;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private DateWorkArea local2Date;
    private DateWorkArea endDate;
    private DateWorkArea local30Date;
    private DateWorkArea local180Date;
    private DateWorkArea local365Date;
    private DateWorkArea local1095Date;
    private DateWorkArea local1825Date;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private Ocse34 ocse34;
  }
#endregion
}
