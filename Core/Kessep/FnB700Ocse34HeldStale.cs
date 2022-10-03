// Program: FN_B700_OCSE34_HELD_STALE, ID: 371222332, model: 746.
// Short name: SWE02020
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
/// A program: FN_B700_OCSE34_HELD_STALE.
/// </para>
/// <para>
/// This action block inputs and allocates held, stale, and reissues payments 
/// sent from the KPC.
/// </para>
/// </summary>
[Serializable]
public partial class FnB700Ocse34HeldStale: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_OCSE34_HELD_STALE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700Ocse34HeldStale(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700Ocse34HeldStale.
  /// </summary>
  public FnB700Ocse34HeldStale(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************************
    // **                 M A I N T E N A N C E   L O G
    // ****************************************************************************
    // ** Date		WR/PR	Developer	Description
    // ****************************************************************************
    // ** 10/20/2004	040134	E.Shirk		Initial development.
    // ** 12/03/2007	CQ295	GVandy		Federally mandated changes to OCSE34 report.
    // ** 01/07/2010	CQ14811	GVandy
    // 1) Remove the edit that a payment request ID is required for Non-IVD 
    // checks.
    // 2) The decimal point on the control report totals needs to be shifted 2 
    // places to the left.
    // 3) Non-IVD indicator is located in position 106 of the Re-issued file, 
    // not position 86.
    // ** 10/14/12  		GVandy		Emergency fix to expand foreign group view size
    // ***************************************************************************
    // **************************************************************************
    // ***   Initialize process variables.
    // **************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ForCreate.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreate.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);

    // **************************************************************************
    // ***   Open Files
    // **************************************************************************
    local.External.FileInstruction = "OPEN";
    UseFnB700ExtReadFile1();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "***  Bad open of external flat files.";
      UseCabErrorReport();
      export.AbortInd.Flag = "Y";
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // **************************************************************************
    // ***   Check for restart
    // **************************************************************************
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 2, "05"))
    {
      // @@@
      if (Verify(Substring(
        import.ProgramCheckpointRestart.RestartInfo, 250, 4, 1),
        " 0123456789") == 0)
      {
        if (IsEmpty(Substring(import.ProgramCheckpointRestart.RestartInfo, 4, 1)))
          
        {
          local.StartingFileNbr.Count = 1;
        }
        else
        {
          local.StartingFileNbr.Count =
            (int)StringToNumber(Substring(
              import.ProgramCheckpointRestart.RestartInfo, 250, 4, 1));
        }

        // @@@ END
      }
      else
      {
        local.EabReportSend.RptDetail = "Invalid file number " + Substring
          (import.ProgramCheckpointRestart.RestartInfo, 250, 4, 1) + " at position 4 of restart information.";
          
        UseCabErrorReport();
        export.AbortInd.Flag = "Y";
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
      {
        if (local.Common.Count == 1)
        {
          local.EabReportSend.RptDetail =
            "Process restarting in Step 5, Part 2 on file " + Substring
            (import.ProgramCheckpointRestart.RestartInfo, 250, 4, 1);
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          export.AbortInd.Flag = "Y";
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          return;
        }
      }

      UseFnB700BuildGvForRestart();
    }
    else
    {
      local.StartingFileNbr.Count = 1;

      if (import.Group.IsEmpty)
      {
        // -- The import group is empty if the PPI record is set to begin 
        // processing at this step.
        // --  We need to load the values calculated in the previous steps so 
        // that they are not overridden with zeros.
        UseFnB700BuildGvForRestart();
      }
    }

    // **************************************************************************
    // ***   Write Control Report Header Info
    // **************************************************************************
    for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "*********  Control Report for held, stale, and reissue file processing.   ******";
            

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "FILE          COUNT          PAYMENT AMOUNT      ADJUSTMENT AMOUNT";
            

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "-------  ---------------   ------------------   ------------------";
            

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        export.AbortInd.Flag = "Y";
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // **************************************************************************
    // ***   Process Held, Stale, and Reissue Files
    // **************************************************************************
    for(local.FileNumber.Count = local.StartingFileNbr.Count; local
      .FileNumber.Count <= 3; ++local.FileNumber.Count)
    {
      local.NumberOfARecordsFound.Count = 0;
      local.NumberOfBRecordsFound.Count = 0;
      local.NumberOfTRecordsFound.Count = 0;
      local.TotalDetailRecPayAmt.TotalCurrency = 0;
      local.TotalDetailRecAdjAmt.TotalCurrency = 0;
      local.FtrRecTransaction.Count = 0;
      local.FtrRecPayAmt.TotalCurrency = 0;
      local.FtrRecAdjAmt.TotalCurrency = 0;

      // -- Set file name to be used in output messages and/or control reports.
      switch(local.FileNumber.Count)
      {
        case 1:
          local.External.TextLine8 = "Held";

          break;
        case 2:
          local.External.TextLine8 = "Stale";

          break;
        case 3:
          local.External.TextLine8 = "Reissue";

          break;
        default:
          break;
      }

      // **************************************************************************
      // ***   Read External File
      // **************************************************************************
      do
      {
        local.External.FileInstruction = "READ";
        UseFnB700ExtReadFile2();

        switch(TrimEnd(local.External.TextReturnCode))
        {
          case "OK":
            break;
          case "EF":
            continue;
          default:
            local.EabReportSend.RptDetail = "*** Error reading " + TrimEnd
              (local.External.TextLine8) + " file. Return Code = " + local
              .External.TextReturnCode;
            UseCabErrorReport();
            export.AbortInd.Flag = "Y";
            ExitState = "OE0000_ERROR_READING_EXT_FILE";

            return;
        }

        // **************************************************************************
        // ***   Validate and process the record based on the record type in 
        // position 1.
        // **************************************************************************
        switch(TrimEnd(Substring(local.External.TextLine130, 1, 1)))
        {
          case "A":
            // -- Header record processing.
            ++local.NumberOfARecordsFound.Count;

            break;
          case "B":
            // -- Detail record processing.
            ++local.NumberOfBRecordsFound.Count;

            // -- Determine if the payment is IV-D or not.  Non IV-D payments do
            // not have payment request IDs in the files.
            if (local.FileNumber.Count == 3)
            {
              if (CharAt(local.External.TextLine130, 106) == 'Y')
              {
                local.IvdPayment.Flag = "Y";
              }
              else
              {
                local.IvdPayment.Flag = "N";
              }
            }
            else if (CharAt(local.External.TextLine130, 86) == 'Y')
            {
              local.IvdPayment.Flag = "Y";
            }
            else
            {
              local.IvdPayment.Flag = "N";
            }

            if (AsChar(local.IvdPayment.Flag) == 'Y')
            {
              // -- Payment request ids are only required on IV-D payments.
              if (Verify(Substring(
                local.External.TextLine130, External.TextLine130_MaxLength, 3,
                9), "0123456789") > 0)
              {
                // -- Payment request ID is not numeric.
                local.EabReportSend.RptDetail =
                  TrimEnd(local.External.TextLine8) + " file payment request id is invalid.  Payment request id = " +
                  Substring
                  (local.External.TextLine130, External.TextLine130_MaxLength,
                  3, 9) + "";
                UseCabErrorReport();
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }
            }

            if (Verify(Substring(
              local.External.TextLine130, External.TextLine130_MaxLength, 29,
              15), "0123456789") > 0 || Verify
              (Substring(
                local.External.TextLine130, External.TextLine130_MaxLength, 45,
              2), "0123456789") > 0)
            {
              // -- Payment amount is not numeric.
              local.EabReportSend.RptDetail =
                TrimEnd(local.External.TextLine8) + " file payment amount is invalid.  Payment amount = " +
                Substring
                (local.External.TextLine130, External.TextLine130_MaxLength, 29,
                18) + "";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
            else
            {
              // -- Keep a running total of the B record payment amounts to 
              // compare to the footer record.
              // @@@
              local.WorkAmt.TotalCurrency =
                StringToNumber(Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength,
                29, 15)) + StringToNumber
                (Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength,
                45, 2)) / (decimal)100;
              local.TotalDetailRecPayAmt.TotalCurrency += local.WorkAmt.
                TotalCurrency;
            }

            if (AsChar(local.IvdPayment.Flag) == 'N')
            {
              // --   Non-IVD payment.
              // @@@
              local.WorkAmt.TotalCurrency =
                StringToNumber(Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength,
                29, 15)) + StringToNumber
                (Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength,
                45, 2)) / (decimal)100;

              if (local.FileNumber.Count == 1 || local.FileNumber.Count == 2)
              {
                local.WorkAmt.TotalCurrency = -local.WorkAmt.TotalCurrency;
              }

              import.Group.Index = 10;
              import.Group.CheckSize();

              import.Group.Update.Common.TotalCurrency =
                import.Group.Item.Common.TotalCurrency + local
                .WorkAmt.TotalCurrency;
            }
            else
            {
              // --   IVD payment.
              // @@@ Added this statement
              local.Compare.SystemGeneratedIdentifier =
                (int)StringToNumber(Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength, 3,
                9));

              if (!ReadPaymentRequest())
              {
                // --  Payment request ID provided by the KPC was not found.
                local.EabReportSend.RptDetail =
                  TrimEnd(local.External.TextLine8) + " file Payment id not found on payment request. ID=" +
                  Substring
                  (local.External.TextLine130, External.TextLine130_MaxLength,
                  3, 9);
                UseCabErrorReport();
                export.AbortInd.Flag = "Y";
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              if (ReadDisbursementTransaction2())
              {
                foreach(var item in ReadDisbursementTransactionRln())
                {
                  if (!ReadDisbursementTransaction1())
                  {
                    // --  Credit disbursement transaction associated to the 
                    // payment request ID was not found.
                    local.EabReportSend.RptDetail =
                      TrimEnd(local.External.TextLine8) + " file.  Credit disb tran not found for payment request. Pay Req ID=" +
                      Substring
                      (local.External.TextLine130,
                      External.TextLine130_MaxLength, 3, 9);
                    UseCabErrorReport();
                    export.AbortInd.Flag = "Y";
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  if (!ReadCollection())
                  {
                    // --  Collection associated to the payment request ID was 
                    // not found.
                    local.EabReportSend.RptDetail =
                      TrimEnd(local.External.TextLine8) + " file.  Collection not found for payment request. Pay Req ID=" +
                      Substring
                      (local.External.TextLine130,
                      External.TextLine130_MaxLength, 3, 9);
                    UseCabErrorReport();
                    export.AbortInd.Flag = "Y";
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }

                  if (!ReadCsePersonObligationObligationType())
                  {
                    // --  Obligation associated to the payment request ID was 
                    // not found.
                    local.EabReportSend.RptDetail =
                      TrimEnd(local.External.TextLine8) + " file.  Obligation not found for payment request. Pay Req ID=" +
                      Substring
                      (local.External.TextLine130,
                      External.TextLine130_MaxLength, 3, 9);
                    UseCabErrorReport();
                    export.AbortInd.Flag = "Y";
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }
                }

                // @@@
                local.WorkAmt.TotalCurrency =
                  StringToNumber(Substring(
                    local.External.TextLine130, External.TextLine130_MaxLength,
                  29, 15)) + StringToNumber
                  (Substring(
                    local.External.TextLine130, External.TextLine130_MaxLength,
                  45, 2)) / (decimal)100;

                if (local.FileNumber.Count == 1 || local.FileNumber.Count == 2)
                {
                  local.WorkAmt.TotalCurrency = -local.WorkAmt.TotalCurrency;
                }

                // **************************************************************************
                // ***   Allocate disbursement amount to lines 4/7
                // **************************************************************************
                UseFnB700Ocse34MaintainLine47();

                // **************************************************************************
                // ***   Stamp audit detail.
                // **************************************************************************
                if (!IsEmpty(local.ForCreate.LineNumber))
                {
                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  // @@@
                  // @@@ Trace back to the CRD ???
                  // @@@
                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  if (AsChar(import.WriteAuditDtl.Flag) == 'Y')
                  {
                    local.ForCreate.SuppPersonNumber = entities.Supp.Number;
                    local.ForCreate.CourtOrderNumber =
                      entities.Collection.CourtOrderAppliedTo;
                    local.ForCreate.ObligorPersonNbr =
                      entities.PaymentRequest.CsePersonNumber;
                    local.ForCreate.CollectionSgi =
                      entities.Collection.SystemGeneratedIdentifier;
                    local.ForCreate.CollCreatedDte =
                      Date(entities.Collection.CreatedTmst);
                    local.ForCreate.CollApplToCode =
                      entities.Collection.AppliedToCode;
                    local.ForCreate.CollectionDte =
                      entities.Collection.CollectionDt;
                    local.ForCreate.CollectionAmount =
                      local.WorkAmt.TotalCurrency;
                    UseFnCreateOcse157Verification();
                  }
                }
                else
                {
                  local.EabReportSend.RptDetail =
                    TrimEnd(local.External.TextLine8) + " file.  Unable to allocate to lines 4/7. Pay Req ID=" +
                    Substring
                    (local.External.TextLine130, External.TextLine130_MaxLength,
                    3, 9);
                  UseCabErrorReport();
                  export.AbortInd.Flag = "Y";
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }
              }
            }

            break;
          case "T":
            // -- Footer record processing.
            ++local.NumberOfTRecordsFound.Count;

            if (Verify(Substring(
              local.External.TextLine130, External.TextLine130_MaxLength, 11,
              9), "0123456789") > 0)
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.External.TextLine8) + " file footer row detail count " +
                Substring
                (local.External.TextLine130, External.TextLine130_MaxLength, 11,
                9) + " is invalid.";
              UseCabErrorReport();
              export.AbortInd.Flag = "Y";
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
            else
            {
              local.FtrRecTransaction.Count =
                (int)StringToNumber(Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength,
                11, 9));
            }

            if (Verify(Substring(
              local.External.TextLine130, External.TextLine130_MaxLength, 32,
              15), "0123456789") > 0 && Verify
              (Substring(
                local.External.TextLine130, External.TextLine130_MaxLength, 48,
              2), "0123456789") == 0)
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.External.TextLine8) + " file footer row detail amount " +
                Substring
                (local.External.TextLine130, External.TextLine130_MaxLength, 32,
                18) + " is invalid.";
              UseCabErrorReport();
              export.AbortInd.Flag = "Y";
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
            else
            {
              local.FtrRecPayAmt.TotalCurrency =
                StringToNumber(Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength,
                32, 15)) + StringToNumber
                (Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength,
                48, 2)) / (decimal)100;
            }

            if (Verify(Substring(
              local.External.TextLine130, External.TextLine130_MaxLength, 62,
              15), "0123456789") > 0 && Verify
              (Substring(
                local.External.TextLine130, External.TextLine130_MaxLength, 78,
              2), "0123456789") == 0)
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.External.TextLine8) + " file footer row adjustment amount " +
                Substring
                (local.External.TextLine130, External.TextLine130_MaxLength, 62,
                18) + " is invalid.";
              UseCabErrorReport();
              export.AbortInd.Flag = "Y";
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
            else
            {
              local.FtrRecAdjAmt.TotalCurrency =
                StringToNumber(Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength,
                62, 15)) + StringToNumber
                (Substring(
                  local.External.TextLine130, External.TextLine130_MaxLength,
                78, 2)) / (decimal)100;
            }

            break;
          default:
            // -- Invalid record type.
            local.EabReportSend.RptDetail = "*** Invalid record type " + Substring
              (local.External.TextLine130, External.TextLine130_MaxLength, 1, 1) +
              " detected in position 1 of " + TrimEnd
              (local.External.TextLine8) + " file.";
            UseCabErrorReport();
            export.AbortInd.Flag = "Y";
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
        }
      }
      while(!Equal(local.External.TextReturnCode, "EF"));

      // **************************************************************************
      // ***   Verify that the file contains the appropriate records and
      // ***   that the footer record totals match the data within the file.
      // **************************************************************************
      if (local.NumberOfARecordsFound.Count != 1)
      {
        // -- There should be one and only one header record in each file.
        local.EabReportSend.RptDetail =
          "Invalid number of header records found in " + TrimEnd
          (local.External.TextLine8) + " file.  Number of header records = " + NumberToString
          (local.NumberOfARecordsFound.Count, 15);
        UseCabErrorReport();
        export.AbortInd.Flag = "Y";
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      if (local.NumberOfTRecordsFound.Count == 1)
      {
        if (local.NumberOfBRecordsFound.Count != local.FtrRecTransaction.Count)
        {
          // -- The number of detail records should match the detail count on 
          // the footer record.
          local.EabReportSend.RptDetail =
            "Number of B records does not match footer in " + TrimEnd
            (local.External.TextLine8) + " file.  # of B records = " + NumberToString
            (local.NumberOfBRecordsFound.Count, 15) + ".  Footer record count = " +
            NumberToString(local.FtrRecTransaction.Count, 15);
          UseCabErrorReport();
          export.AbortInd.Flag = "Y";
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }

        if (local.TotalDetailRecPayAmt.TotalCurrency != local
          .FtrRecPayAmt.TotalCurrency)
        {
          // -- The dollar amount of the detail records should match the dollar 
          // amount on the footer record.
          local.EabReportSend.RptDetail =
            "B record payment amounts do not match footer in " + TrimEnd
            (local.External.TextLine8) + " file.  B record amt = " + NumberToString
            ((long)(local.TotalDetailRecPayAmt.TotalCurrency * 100), 15) + ".  Footer record amt = " +
            NumberToString((long)(local.FtrRecPayAmt.TotalCurrency * 100), 15);
          UseCabErrorReport();
          export.AbortInd.Flag = "Y";
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }

        if (local.TotalDetailRecAdjAmt.TotalCurrency != local
          .FtrRecAdjAmt.TotalCurrency)
        {
          // -- We don't expect any adjustment records in these files.  But this
          // check will insure that there were none.
          // -- The dollar amount of the detail adjustment records should match 
          // the dollar amount on the footer record.
          local.EabReportSend.RptDetail =
            "Adjustment amounts do not match footer in " + TrimEnd
            (local.External.TextLine8) + " file.  Adj amt = " + NumberToString
            ((long)(local.TotalDetailRecAdjAmt.TotalCurrency * 100), 15) + ".  Footer record amt = " +
            NumberToString((long)(local.FtrRecAdjAmt.TotalCurrency * 100), 15);
          UseCabErrorReport();
          export.AbortInd.Flag = "Y";
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
      }
      else
      {
        // -- There should be one and only one footer record in each file.
        local.EabReportSend.RptDetail =
          "Invalid number of footer records found in " + TrimEnd
          (local.External.TextLine8) + " file.  Number of footer records = " + NumberToString
          (local.NumberOfTRecordsFound.Count, 15);
        UseCabErrorReport();
        export.AbortInd.Flag = "Y";
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      if (IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
      {
        return;
      }

      // **************************************************************************
      // ***   Write Counts and Totals to the Control Report
      // **************************************************************************
      local.EabReportSend.RptDetail = local.External.TextLine8;
      local.EabReportSend.RptDetail =
        Substring(local.EabReportSend.RptDetail,
        EabReportSend.RptDetail_MaxLength, 1, 9) + NumberToString
        (local.FtrRecTransaction.Count, 15);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "   " +
        NumberToString((long)local.FtrRecPayAmt.TotalCurrency, 15);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "."
        + NumberToString
        ((long)(local.FtrRecPayAmt.TotalCurrency * 100), 14, 2);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "   " +
        NumberToString((long)local.FtrRecAdjAmt.TotalCurrency, 15);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "."
        + NumberToString
        ((long)(local.FtrRecAdjAmt.TotalCurrency * 100), 14, 2);
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        export.AbortInd.Flag = "Y";
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }

      UseFnB700ApplyUpdates();
      local.ProgramCheckpointRestart.RestartInd = "Y";

      if (local.FileNumber.Count == 3)
      {
        local.ProgramCheckpointRestart.RestartInfo = "06";
      }
      else
      {
        local.ProgramCheckpointRestart.RestartInfo = "05 " + NumberToString
          ((long)local.FileNumber.Count + 1, 15, 1) + " " + "PART 2";
      }

      UseUpdateCheckpointRstAndCommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ForError.LineNumber = "05";
        UseOcse157WriteError();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.AbortInd.Flag = "Y";

          return;
        }
      }
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.CollectionDt = source.CollectionDt;
    target.AdjustedInd = source.AdjustedInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CreatedTmst = source.CreatedTmst;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine130 = source.TextLine130;
  }

  private static void MoveGroup1(Import.GroupGroup source,
    FnB700ApplyUpdates.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup2(Import.GroupGroup source,
    FnB700Ocse34MaintainLine47.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup3(FnB700BuildGvForRestart.Export.
    GroupGroup source, Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup4(FnB700Ocse34MaintainLine47.Import.
    GroupGroup source, Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveIncomingForeign(Import.IncomingForeignGroup source,
    FnB700Ocse34MaintainLine47.Import.IncomingForeignGroup target)
  {
    target.GimportIncomingForeign.StandardNumber =
      source.GimportIncomingForeign.StandardNumber;
  }

  private static void MoveOcse157Verification1(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseWorkerName = source.CaseWorkerName;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
  }

  private static void MoveOcse34(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB700ApplyUpdates()
  {
    var useImport = new FnB700ApplyUpdates.Import();
    var useExport = new FnB700ApplyUpdates.Export();

    import.Group.CopyTo(useImport.Group, MoveGroup1);
    MoveOcse34(import.Ocse34, useImport.Ocse34);

    Call(FnB700ApplyUpdates.Execute, useImport, useExport);
  }

  private void UseFnB700BuildGvForRestart()
  {
    var useImport = new FnB700BuildGvForRestart.Import();
    var useExport = new FnB700BuildGvForRestart.Export();

    MoveOcse34(import.Ocse34, useImport.Ocse34);

    Call(FnB700BuildGvForRestart.Execute, useImport, useExport);

    useExport.Group.CopyTo(import.Group, MoveGroup3);
  }

  private void UseFnB700ExtReadFile1()
  {
    var useImport = new FnB700ExtReadFile.Import();
    var useExport = new FnB700ExtReadFile.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    useExport.External.Assign(local.External);

    Call(FnB700ExtReadFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseFnB700ExtReadFile2()
  {
    var useImport = new FnB700ExtReadFile.Import();
    var useExport = new FnB700ExtReadFile.Export();

    useImport.FileNumber.Count = local.FileNumber.Count;
    useImport.External.FileInstruction = local.External.FileInstruction;
    useExport.External.Assign(local.External);

    Call(FnB700ExtReadFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseFnB700Ocse34MaintainLine47()
  {
    var useImport = new FnB700Ocse34MaintainLine47.Import();
    var useExport = new FnB700Ocse34MaintainLine47.Export();

    import.Group.CopyTo(useImport.Group, MoveGroup2);
    MoveCollection(entities.Collection, useImport.Collection);
    useImport.Supp.Number = entities.Supp.Number;
    import.IncomingForeign.
      CopyTo(useImport.IncomingForeign, MoveIncomingForeign);
    useImport.ObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Common.TotalCurrency = local.WorkAmt.TotalCurrency;

    Call(FnB700Ocse34MaintainLine47.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup4);
    local.WorkAmt.TotalCurrency = useImport.Common.TotalCurrency;
    local.ForCreate.LineNumber = useExport.Ocse157Verification.LineNumber;
  }

  private void UseFnCreateOcse157Verification()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification1(local.ForCreate, useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    MoveOcse157Verification2(local.ForError, useImport.Ocse157Verification);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.
          SetInt32(command, "collId", entities.Credit.ColId.GetValueOrDefault());
          
        db.
          SetInt32(command, "otyId", entities.Credit.OtyId.GetValueOrDefault());
          
        db.
          SetInt32(command, "obgId", entities.Credit.ObgId.GetValueOrDefault());
          
        db.SetString(command, "cspNumber", entities.Credit.CspNumberDisb ?? "");
        db.SetString(command, "cpaType", entities.Credit.CpaTypeDisb ?? "");
        db.
          SetInt32(command, "otrId", entities.Credit.OtrId.GetValueOrDefault());
          
        db.SetString(command, "otrType", entities.Credit.OtrTypeDisb ?? "");
        db.SetInt32(
          command, "crtType", entities.Credit.CrtId.GetValueOrDefault());
        db.
          SetInt32(command, "cstId", entities.Credit.CstId.GetValueOrDefault());
          
        db.
          SetInt32(command, "crvId", entities.Credit.CrvId.GetValueOrDefault());
          
        db.
          SetInt32(command, "crdId", entities.Credit.CrdId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
      });
  }

  private bool ReadCsePersonObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supp.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadCsePersonObligationObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Obligation.CpaType = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 2);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 4);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Supp.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbursementTransactionRln.Populated);
    entities.Credit.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransactionRln.DtrPGeneratedId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransactionRln.CpaPType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransactionRln.CspPNumber);
          
      },
      (db, reader) =>
      {
        entities.Credit.CpaType = db.GetString(reader, 0);
        entities.Credit.CspNumber = db.GetString(reader, 1);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Credit.Type1 = db.GetString(reader, 3);
        entities.Credit.Amount = db.GetDecimal(reader, 4);
        entities.Credit.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Credit.OtyId = db.GetNullableInt32(reader, 6);
        entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 7);
        entities.Credit.OtrId = db.GetNullableInt32(reader, 8);
        entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 9);
        entities.Credit.CspNumberDisb = db.GetNullableString(reader, 10);
        entities.Credit.ObgId = db.GetNullableInt32(reader, 11);
        entities.Credit.CrdId = db.GetNullableInt32(reader, 12);
        entities.Credit.CrvId = db.GetNullableInt32(reader, 13);
        entities.Credit.CstId = db.GetNullableInt32(reader, 14);
        entities.Credit.CrtId = db.GetNullableInt32(reader, 15);
        entities.Credit.ColId = db.GetNullableInt32(reader, 16);
        entities.Credit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Credit.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.Credit.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.Credit.CpaTypeDisb);
      });
  }

  private bool ReadDisbursementTransaction2()
  {
    entities.Debit.Populated = false;

    return Read("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Debit.CpaType = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debit.Type1 = db.GetString(reader, 3);
        entities.Debit.PrqGeneratedId = db.GetNullableInt32(reader, 4);
        entities.Debit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Debit.Type1);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.DisbursementTransactionRln.Populated = false;

    return ReadEach("ReadDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId", entities.Debit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debit.CpaType);
        db.SetString(command, "cspNumber", entities.Debit.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementTransactionRln.CspNumber = db.GetString(reader, 2);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 3);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 4);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 5);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 6);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 7);
        entities.DisbursementTransactionRln.Populated = true;
        CheckValid<DisbursementTransactionRln>("CpaType",
          entities.DisbursementTransactionRln.CpaType);
        CheckValid<DisbursementTransactionRln>("CpaPType",
          entities.DisbursementTransactionRln.CpaPType);

        return true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId", local.Compare.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 1);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.PaymentRequest.Classification = db.GetString(reader, 3);
        entities.PaymentRequest.Type1 = db.GetString(reader, 4);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 74;

      private Common common;
    }

    /// <summary>A IncomingForeignGroup group.</summary>
    [Serializable]
    public class IncomingForeignGroup
    {
      /// <summary>
      /// A value of GimportIncomingForeign.
      /// </summary>
      [JsonPropertyName("gimportIncomingForeign")]
      public LegalAction GimportIncomingForeign
      {
        get => gimportIncomingForeign ??= new();
        set => gimportIncomingForeign = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private LegalAction gimportIncomingForeign;
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
    /// A value of RptPrdEnd.
    /// </summary>
    [JsonPropertyName("rptPrdEnd")]
    public DateWorkArea RptPrdEnd
    {
      get => rptPrdEnd ??= new();
      set => rptPrdEnd = value;
    }

    /// <summary>
    /// A value of RptPrdBegin.
    /// </summary>
    [JsonPropertyName("rptPrdBegin")]
    public DateWorkArea RptPrdBegin
    {
      get => rptPrdBegin ??= new();
      set => rptPrdBegin = value;
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
    /// A value of WriteAuditDtl.
    /// </summary>
    [JsonPropertyName("writeAuditDtl")]
    public Common WriteAuditDtl
    {
      get => writeAuditDtl ??= new();
      set => writeAuditDtl = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// Gets a value of IncomingForeign.
    /// </summary>
    [JsonIgnore]
    public Array<IncomingForeignGroup> IncomingForeign =>
      incomingForeign ??= new(IncomingForeignGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IncomingForeign for json serialization.
    /// </summary>
    [JsonPropertyName("incomingForeign")]
    [Computed]
    public IList<IncomingForeignGroup> IncomingForeign_Json
    {
      get => incomingForeign;
      set => IncomingForeign.Assign(value);
    }

    private Array<GroupGroup> group;
    private DateWorkArea rptPrdEnd;
    private DateWorkArea rptPrdBegin;
    private Ocse34 ocse34;
    private Common writeAuditDtl;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private Ocse157Verification ocse157Verification;
    private Array<IncomingForeignGroup> incomingForeign;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AbortInd.
    /// </summary>
    [JsonPropertyName("abortInd")]
    public Common AbortInd
    {
      get => abortInd ??= new();
      set => abortInd = value;
    }

    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abortInd;
    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TotalDetailRecAdjAmt.
    /// </summary>
    [JsonPropertyName("totalDetailRecAdjAmt")]
    public Common TotalDetailRecAdjAmt
    {
      get => totalDetailRecAdjAmt ??= new();
      set => totalDetailRecAdjAmt = value;
    }

    /// <summary>
    /// A value of TotalDetailRecPayAmt.
    /// </summary>
    [JsonPropertyName("totalDetailRecPayAmt")]
    public Common TotalDetailRecPayAmt
    {
      get => totalDetailRecPayAmt ??= new();
      set => totalDetailRecPayAmt = value;
    }

    /// <summary>
    /// A value of IvdPayment.
    /// </summary>
    [JsonPropertyName("ivdPayment")]
    public Common IvdPayment
    {
      get => ivdPayment ??= new();
      set => ivdPayment = value;
    }

    /// <summary>
    /// A value of FtrRecTransaction.
    /// </summary>
    [JsonPropertyName("ftrRecTransaction")]
    public Common FtrRecTransaction
    {
      get => ftrRecTransaction ??= new();
      set => ftrRecTransaction = value;
    }

    /// <summary>
    /// A value of NumberOfTRecordsFound.
    /// </summary>
    [JsonPropertyName("numberOfTRecordsFound")]
    public Common NumberOfTRecordsFound
    {
      get => numberOfTRecordsFound ??= new();
      set => numberOfTRecordsFound = value;
    }

    /// <summary>
    /// A value of NumberOfBRecordsFound.
    /// </summary>
    [JsonPropertyName("numberOfBRecordsFound")]
    public Common NumberOfBRecordsFound
    {
      get => numberOfBRecordsFound ??= new();
      set => numberOfBRecordsFound = value;
    }

    /// <summary>
    /// A value of NumberOfARecordsFound.
    /// </summary>
    [JsonPropertyName("numberOfARecordsFound")]
    public Common NumberOfARecordsFound
    {
      get => numberOfARecordsFound ??= new();
      set => numberOfARecordsFound = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of FtrRecAdjAmt.
    /// </summary>
    [JsonPropertyName("ftrRecAdjAmt")]
    public Common FtrRecAdjAmt
    {
      get => ftrRecAdjAmt ??= new();
      set => ftrRecAdjAmt = value;
    }

    /// <summary>
    /// A value of FtrRecPayAmt.
    /// </summary>
    [JsonPropertyName("ftrRecPayAmt")]
    public Common FtrRecPayAmt
    {
      get => ftrRecPayAmt ??= new();
      set => ftrRecPayAmt = value;
    }

    /// <summary>
    /// A value of WorkAmt.
    /// </summary>
    [JsonPropertyName("workAmt")]
    public Common WorkAmt
    {
      get => workAmt ??= new();
      set => workAmt = value;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public PaymentRequest Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of StartingFileNbr.
    /// </summary>
    [JsonPropertyName("startingFileNbr")]
    public Common StartingFileNbr
    {
      get => startingFileNbr ??= new();
      set => startingFileNbr = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public Ocse157Verification ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    private Common totalDetailRecAdjAmt;
    private Common totalDetailRecPayAmt;
    private Common ivdPayment;
    private Common ftrRecTransaction;
    private Common numberOfTRecordsFound;
    private Common numberOfBRecordsFound;
    private Common numberOfARecordsFound;
    private Common fileNumber;
    private Common common;
    private Common ftrRecAdjAmt;
    private Common ftrRecPayAmt;
    private Common workAmt;
    private PaymentRequest compare;
    private Common startingFileNbr;
    private Ocse157Verification forCreate;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External external;
    private Ocse157Verification forError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private PaymentRequest paymentRequest;
    private DisbursementTransaction debit;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction credit;
    private CsePerson supp;
    private ObligationTransaction debt;
    private CsePersonAccount supported;
    private Obligation obligation;
    private ObligationType obligationType;
    private Collection collection;
    private CsePerson ap;
  }
#endregion
}
