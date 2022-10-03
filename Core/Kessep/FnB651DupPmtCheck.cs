// Program: FN_B651_DUP_PMT_CHECK, ID: 371004909, model: 746.
// Short name: SWE02690
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_DUP_PMT_CHECK.
/// </summary>
[Serializable]
public partial class FnB651DupPmtCheck: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_DUP_PMT_CHECK program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651DupPmtCheck(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651DupPmtCheck.
  /// </summary>
  public FnB651DupPmtCheck(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************
    // 09-25-00  PR 98039  Fangman - New code to get the total amount of 
    // disbursements for a person/reference number.  This total is only needed
    // if there has been at least one adjustment for this reference number.
    // 12-08-00  PR 108996  Fangman - Added code to not include current nights 
    // disbursements to the exp_tot_disb_for_ref_nbr field.
    // 05-02-01  PR 118495  Fangman - Redesign for suppr negative calc of 
    // collection funds availabe to disburse.
    // 10-16-01  PR 118495  Fangman - Add code to calculate the total amount of 
    // disbursable disbursement collections that will be processed in the
    // current run so that the total nightly CR Fee can be calculated.
    // 12/07/01  PR 118495  Fangman - Dup Pmt modification.  Added code to 
    // escape if Dup Check = N.
    // 01-29-01  WR 000235  Fangman - PSUM redesign.
    // 02-16-04  PR 200634  Fangman - Prob w/ ending processing on certain 
    // errors
    // ****************************************************************
    export.CheckForDupPmt.Flag = "N";
    local.EabFileHandling.Action = "WRITE";

    // For the current cash receipt detail (ref #) look for an adjusted 
    // collection with a program that causes money to go out that has
    // disbursements going to the current AR.
    foreach(var item in ReadCollection())
    {
      // The majority of collections will not meet the above conditions but if 
      // one or more do then we must check to see if they resulted in
      // disbursements to the current AR.
      if (ReadDisbCollectionCsePerson())
      {
        export.CheckForDupPmt.Flag = "Y";
        local.DisbursementTransaction.ReferenceNumber =
          entities.Existing1.ReferenceNumber;

        break;
      }
    }

    if (AsChar(export.CheckForDupPmt.Flag) == 'N')
    {
      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "  No adjustments for this AR Ref # so do not check for dups.";
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      return;
    }

    // Determine the net amount of disbursable money that has been distributed 
    // to the AR for the Ref #.
    foreach(var item in ReadDisbCollection2())
    {
      export.CollFundsAvailForDisb.TotalCurrency += entities.Existing1.Amount;
    }

    if (AsChar(import.TestDisplayInd.Flag) == 'Y')
    {
      local.Unformatted.Number112 = export.CollFundsAvailForDisb.TotalCurrency;
      UseCabFormat112AmtFieldTo2();

      if (export.CollFundsAvailForDisb.TotalCurrency < 0)
      {
        local.Sign1.Text1 = "-";
      }
      else
      {
        local.Sign1.Text1 = "+";
      }

      local.EabReportSend.RptDetail =
        "    Dup Pmt Check:  Total non adjusted coll for AR & Ref # = " + local
        .Sign1.Text1 + local.FormattedAmt.Text9;
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        "---------------  DB Disb  --------------   ------------------ Totals -------------------";
        
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail =
        "     ID        Proc Dt  Type         Amt      a date   all dates     suppr       cr fees";
        
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // Total all of the positive processed disbursements (previous to the 
    // current nights processing) for the current AR from the current cash
    // receipt detail (ref #) if those disbursements are associated to program
    // codes that would cause the disbursement to be paid out.
    // Any negatives that processed in the same night as a positive can be used 
    // to net against the positive.
    // Read all of the disbursable debit disbursements for this AR and reference
    // number.
    foreach(var item in ReadDisbursementDisbursementType())
    {
      if (Equal(entities.Db.ProcessDate, local.Initialized.Date))
      {
        // Count all suppressed disbursements.
        local.TotSupprDisb.TotalCurrency += entities.Db.Amount;
      }
      else
      {
        if (!Equal(entities.Db.ProcessDate, local.HoldDateWorkArea.Date))
        {
          // Finish processing for last process date prior to processing for 
          // current process date.
          local.TotForAllProcessDates.Amount += local.TotForAProcessDate.Amount;
          local.TotForAProcessDate.Amount = 0;
          local.HoldDateWorkArea.Date = entities.Db.ProcessDate;
        }

        if (entities.DisbursementType.SystemGeneratedIdentifier == 73)
        {
          // All CR Fees are totalled & should be GE 0.
          local.TotCrFees.TotalCurrency += entities.Db.Amount;
        }
        else if (entities.Db.Amount > 0)
        {
          // All postives go into the total.
          local.TotForAProcessDate.Amount += entities.Db.Amount;
        }
        else if (-entities.Db.Amount <= local.TotForAProcessDate.Amount)
        {
          // The total can be reduced by the negative amt.
          local.TotForAProcessDate.Amount += entities.Db.Amount;
        }
        else
        {
          // The total (for this AR, ref # and process date) can be reduced by 
          // the negative amt down to 0 but not below.
          local.TotForAProcessDate.Amount = 0;
        }
      }

      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.UnformattedDate.Date = entities.Db.ProcessDate;
        UseCabFormatDate();

        if (entities.Db.Amount < 0)
        {
          local.Sign1.Text1 = "-";
        }
        else
        {
          local.Sign1.Text1 = "";
        }

        local.Unformatted.Number112 = entities.Db.Amount;
        UseCabFormat112AmtFieldTo4();

        if (local.TotForAProcessDate.Amount < 0)
        {
          local.Sign2.Text1 = "-";
        }
        else
        {
          local.Sign2.Text1 = "";
        }

        local.Unformatted.Number112 = local.TotForAProcessDate.Amount;
        UseCabFormat112AmtFieldTo5();

        if (local.TotForAllProcessDates.Amount < 0)
        {
          local.Sign3.Text1 = "-";
        }
        else
        {
          local.Sign3.Text1 = "";
        }

        local.Unformatted.Number112 = local.TotForAllProcessDates.Amount;
        UseCabFormat112AmtFieldTo1();

        if (local.TotSupprDisb.TotalCurrency < 0)
        {
          local.Sign4.Text1 = "-";
        }
        else
        {
          local.Sign4.Text1 = "";
        }

        local.Unformatted.Number112 = local.TotSupprDisb.TotalCurrency;
        UseCabFormat112AmtFieldTo6();

        if (local.TotCrFees.TotalCurrency < 0)
        {
          local.Sign6.Text1 = "-";
        }
        else
        {
          local.Sign6.Text1 = "";
        }

        local.Unformatted.Number112 = local.TotCrFees.TotalCurrency;
        UseCabFormat112AmtFieldTo7();
        local.EabReportSend.RptDetail = "  " + NumberToString
          (entities.Db.SystemGeneratedIdentifier, 7, 9) + "  " + local
          .FormattedDate.Text10 + "  " + NumberToString
          (entities.DisbursementType.SystemGeneratedIdentifier, 13, 3) + "  " +
          local.Sign1.Text1 + local.FormattedDisbAmt.Text9 + "  " + local
          .Sign2.Text1 + local.FormattedTotForADate.Text9 + "  " + local
          .Sign3.Text1 + local.FormattedTotForAllDates.Text9 + "  " + local
          .Sign4.Text1 + local.FormattedTotForSuppr.Text9 + "  " + "" + "" + ""
          + local.Sign6.Text1 + local.FormattedTotForCrFees.Text9;
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    local.TotForAllProcessDates.Amount += local.TotForAProcessDate.Amount;
    UseFnB651DupPmtCrFeeCalc();

    // **** PR 200634 - Fix prob in 651 w/ ending processing on certain errors *
    // ***
    if (!Equal(ExitState, "ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.CollFundsAvailForDisb.TotalCurrency =
      export.CollFundsAvailForDisb.TotalCurrency - local
      .TotForAllProcessDates.Amount - local.TotSupprDisb.TotalCurrency - local
      .TotCrFees.TotalCurrency - local.TotFutureCrFees.TotalCurrency;

    if (export.CollFundsAvailForDisb.TotalCurrency < 0)
    {
      if (AsChar(import.TestDisplayInd.Flag) == 'Y')
      {
        local.Unformatted.Number112 =
          export.CollFundsAvailForDisb.TotalCurrency;
        UseCabFormat112AmtFieldTo3();
        local.EabReportSend.RptDetail = "  Total funds avail to disb is -" + local
          .FormattedCollFundsAvail.Text9 + "  so set to 0.";
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      export.CollFundsAvailForDisb.TotalCurrency = 0;
    }

    if (AsChar(import.TestDisplayInd.Flag) == 'Y')
    {
      local.EabFileHandling.Action = "WRITE";

      if (local.TotFutureCrFees.TotalCurrency < 0)
      {
        local.Sign1.Text1 = "-";
      }
      else
      {
        local.Sign1.Text1 = "+";
      }

      local.Unformatted.Number112 = local.TotFutureCrFees.TotalCurrency;
      UseCabFormat112AmtFieldTo9();
      local.Unformatted.Number112 = export.CollFundsAvailForDisb.TotalCurrency;
      UseCabFormat112AmtFieldTo3();
      local.EabReportSend.RptDetail = "  Tot future CR Fees " + local
        .Sign1.Text1 + local.FormattedTotForFutureCr.Text9 + "  Total funds avail to disb " +
        local.FormattedCollFundsAvail.Text9;
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // Total all collections for this AR/Ref #
    ReadDisbCollection1();
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

  private void UseCabFormat112AmtFieldTo1()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.Unformatted.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedTotForAllDates.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo2()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.Unformatted.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedAmt.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo3()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.Unformatted.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedCollFundsAvail.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo4()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.Unformatted.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedDisbAmt.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo5()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.Unformatted.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedTotForADate.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo6()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.Unformatted.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedTotForSuppr.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo7()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.Unformatted.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedTotForCrFees.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo9()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 = local.Unformatted.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.FormattedTotForFutureCr.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.UnformattedDate.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseFnB651DupPmtCrFeeCalc()
  {
    var useImport = new FnB651DupPmtCrFeeCalc.Import();
    var useExport = new FnB651DupPmtCrFeeCalc.Export();

    useImport.PerAr.Number = entities.Obligee.Number;
    useImport.PerObligee.Assign(import.PerObligee);
    useImport.DisbursementTransaction.ReferenceNumber =
      import.PerDisbursementTransaction.ReferenceNumber;
    useImport.TestDisplayInd.Flag = import.TestDisplayInd.Flag;

    Call(FnB651DupPmtCrFeeCalc.Execute, useImport, useExport);

    import.PerObligee.Assign(useImport.PerObligee);
    local.TotFutureCrFees.TotalCurrency =
      useExport.TotArRefNbrCfFees.TotalCurrency;
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(import.PerCashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.PerCashReceiptDetail.SequentialIdentifier);
        db.
          SetInt32(command, "crvId", import.PerCashReceiptDetail.CrvIdentifier);
          
        db.
          SetInt32(command, "cstId", import.PerCashReceiptDetail.CstIdentifier);
          
        db.SetInt32(
          command, "crtType", import.PerCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 12);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private bool ReadDisbCollection1()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);

    return Read("ReadDisbCollection1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.PerObligee.Type1);
        db.SetString(command, "cspNumber", import.PerObligee.CspNumber);
        db.SetNullableString(
          command, "referenceNumber",
          import.PerDisbursementTransaction.ReferenceNumber ?? "");
      },
      (db, reader) =>
      {
        export.PrevTotArRefNbrAmt.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private IEnumerable<bool> ReadDisbCollection2()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);
    System.Diagnostics.Debug.Assert(import.PerCashReceiptDetail.Populated);
    entities.Existing1.Populated = false;

    return ReadEach("ReadDisbCollection2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.PerObligee.Type1);
        db.SetString(command, "cspNumber", import.PerObligee.CspNumber);
        db.SetInt32(
          command, "crdId", import.PerCashReceiptDetail.SequentialIdentifier);
        db.
          SetInt32(command, "crvId", import.PerCashReceiptDetail.CrvIdentifier);
          
        db.
          SetInt32(command, "cstId", import.PerCashReceiptDetail.CstIdentifier);
          
        db.SetInt32(
          command, "crtType", import.PerCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing1.CpaType = db.GetString(reader, 0);
        entities.Existing1.CspNumber = db.GetString(reader, 1);
        entities.Existing1.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Existing1.Amount = db.GetDecimal(reader, 3);
        entities.Existing1.ProcessDate = db.GetNullableDate(reader, 4);
        entities.Existing1.OtyId = db.GetNullableInt32(reader, 5);
        entities.Existing1.OtrTypeDisb = db.GetNullableString(reader, 6);
        entities.Existing1.OtrId = db.GetNullableInt32(reader, 7);
        entities.Existing1.CpaTypeDisb = db.GetNullableString(reader, 8);
        entities.Existing1.CspNumberDisb = db.GetNullableString(reader, 9);
        entities.Existing1.ObgId = db.GetNullableInt32(reader, 10);
        entities.Existing1.CrdId = db.GetNullableInt32(reader, 11);
        entities.Existing1.CrvId = db.GetNullableInt32(reader, 12);
        entities.Existing1.CstId = db.GetNullableInt32(reader, 13);
        entities.Existing1.CrtId = db.GetNullableInt32(reader, 14);
        entities.Existing1.ColId = db.GetNullableInt32(reader, 15);
        entities.Existing1.ReferenceNumber = db.GetNullableString(reader, 16);
        entities.Existing1.Populated = true;

        return true;
      });
  }

  private bool ReadDisbCollectionCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Obligee.Populated = false;
    entities.Existing1.Populated = false;

    return Read("ReadDisbCollectionCsePerson",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.PerObligee.CspNumber);
        db.SetNullableInt32(
          command, "colId", entities.Collection.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "otyId", entities.Collection.OtyId);
        db.SetNullableInt32(command, "obgId", entities.Collection.ObgId);
        db.SetNullableString(
          command, "cspNumberDisb", entities.Collection.CspNumber);
        db.
          SetNullableString(command, "cpaTypeDisb", entities.Collection.CpaType);
          
        db.SetNullableInt32(command, "otrId", entities.Collection.OtrId);
        db.
          SetNullableString(command, "otrTypeDisb", entities.Collection.OtrType);
          
        db.SetNullableInt32(command, "crtId", entities.Collection.CrtType);
        db.SetNullableInt32(command, "cstId", entities.Collection.CstId);
        db.SetNullableInt32(command, "crvId", entities.Collection.CrvId);
        db.SetNullableInt32(command, "crdId", entities.Collection.CrdId);
        db.SetString(command, "cpaType", import.PerObligee.Type1);
      },
      (db, reader) =>
      {
        entities.Existing1.CpaType = db.GetString(reader, 0);
        entities.Existing1.CspNumber = db.GetString(reader, 1);
        entities.Existing1.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Existing1.Amount = db.GetDecimal(reader, 3);
        entities.Existing1.ProcessDate = db.GetNullableDate(reader, 4);
        entities.Existing1.OtyId = db.GetNullableInt32(reader, 5);
        entities.Existing1.OtrTypeDisb = db.GetNullableString(reader, 6);
        entities.Existing1.OtrId = db.GetNullableInt32(reader, 7);
        entities.Existing1.CpaTypeDisb = db.GetNullableString(reader, 8);
        entities.Existing1.CspNumberDisb = db.GetNullableString(reader, 9);
        entities.Existing1.ObgId = db.GetNullableInt32(reader, 10);
        entities.Existing1.CrdId = db.GetNullableInt32(reader, 11);
        entities.Existing1.CrvId = db.GetNullableInt32(reader, 12);
        entities.Existing1.CstId = db.GetNullableInt32(reader, 13);
        entities.Existing1.CrtId = db.GetNullableInt32(reader, 14);
        entities.Existing1.ColId = db.GetNullableInt32(reader, 15);
        entities.Existing1.ReferenceNumber = db.GetNullableString(reader, 16);
        entities.Obligee.Number = db.GetString(reader, 17);
        entities.Obligee.Populated = true;
        entities.Existing1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisbursementDisbursementType()
  {
    System.Diagnostics.Debug.Assert(import.PerObligee.Populated);
    entities.DisbursementType.Populated = false;
    entities.Db.Populated = false;

    return ReadEach("ReadDisbursementDisbursementType",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.PerObligee.Type1);
        db.SetString(command, "cspNumber", import.PerObligee.CspNumber);
        db.SetNullableString(
          command, "referenceNumber",
          local.DisbursementTransaction.ReferenceNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Db.CpaType = db.GetString(reader, 0);
        entities.Db.CspNumber = db.GetString(reader, 1);
        entities.Db.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Db.Type1 = db.GetString(reader, 3);
        entities.Db.Amount = db.GetDecimal(reader, 4);
        entities.Db.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Db.DbtGeneratedId = db.GetNullableInt32(reader, 6);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Db.ReferenceNumber = db.GetNullableString(reader, 7);
        entities.Db.ExcessUraInd = db.GetNullableString(reader, 8);
        entities.DisbursementType.Populated = true;
        entities.Db.Populated = true;

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
    /// <summary>
    /// A value of PerObligee.
    /// </summary>
    [JsonPropertyName("perObligee")]
    public CsePersonAccount PerObligee
    {
      get => perObligee ??= new();
      set => perObligee = value;
    }

    /// <summary>
    /// A value of PerDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("perDisbursementTransaction")]
    public DisbursementTransaction PerDisbursementTransaction
    {
      get => perDisbursementTransaction ??= new();
      set => perDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of PerCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("perCashReceiptDetail")]
    public CashReceiptDetail PerCashReceiptDetail
    {
      get => perCashReceiptDetail ??= new();
      set => perCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PerCollection.
    /// </summary>
    [JsonPropertyName("perCollection")]
    public Collection PerCollection
    {
      get => perCollection ??= new();
      set => perCollection = value;
    }

    /// <summary>
    /// A value of PerCollectionType.
    /// </summary>
    [JsonPropertyName("perCollectionType")]
    public CollectionType PerCollectionType
    {
      get => perCollectionType ??= new();
      set => perCollectionType = value;
    }

    /// <summary>
    /// A value of TestDisplayInd.
    /// </summary>
    [JsonPropertyName("testDisplayInd")]
    public Common TestDisplayInd
    {
      get => testDisplayInd ??= new();
      set => testDisplayInd = value;
    }

    private CsePersonAccount perObligee;
    private DisbursementTransaction perDisbursementTransaction;
    private CashReceiptDetail perCashReceiptDetail;
    private Collection perCollection;
    private CollectionType perCollectionType;
    private Common testDisplayInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CollFundsAvailForDisb.
    /// </summary>
    [JsonPropertyName("collFundsAvailForDisb")]
    public Common CollFundsAvailForDisb
    {
      get => collFundsAvailForDisb ??= new();
      set => collFundsAvailForDisb = value;
    }

    /// <summary>
    /// A value of PrevTotArRefNbrAmt.
    /// </summary>
    [JsonPropertyName("prevTotArRefNbrAmt")]
    public Common PrevTotArRefNbrAmt
    {
      get => prevTotArRefNbrAmt ??= new();
      set => prevTotArRefNbrAmt = value;
    }

    /// <summary>
    /// A value of CheckForDupPmt.
    /// </summary>
    [JsonPropertyName("checkForDupPmt")]
    public Common CheckForDupPmt
    {
      get => checkForDupPmt ??= new();
      set => checkForDupPmt = value;
    }

    private Common collFundsAvailForDisb;
    private Common prevTotArRefNbrAmt;
    private Common checkForDupPmt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of Unformatted.
    /// </summary>
    [JsonPropertyName("unformatted")]
    public NumericWorkSet Unformatted
    {
      get => unformatted ??= new();
      set => unformatted = value;
    }

    /// <summary>
    /// A value of FormattedAmt.
    /// </summary>
    [JsonPropertyName("formattedAmt")]
    public WorkArea FormattedAmt
    {
      get => formattedAmt ??= new();
      set => formattedAmt = value;
    }

    /// <summary>
    /// A value of TotForAProcessDate.
    /// </summary>
    [JsonPropertyName("totForAProcessDate")]
    public DisbursementTransaction TotForAProcessDate
    {
      get => totForAProcessDate ??= new();
      set => totForAProcessDate = value;
    }

    /// <summary>
    /// A value of TotForAllProcessDates.
    /// </summary>
    [JsonPropertyName("totForAllProcessDates")]
    public DisbursementTransaction TotForAllProcessDates
    {
      get => totForAllProcessDates ??= new();
      set => totForAllProcessDates = value;
    }

    /// <summary>
    /// A value of TotSupprDisb.
    /// </summary>
    [JsonPropertyName("totSupprDisb")]
    public Common TotSupprDisb
    {
      get => totSupprDisb ??= new();
      set => totSupprDisb = value;
    }

    /// <summary>
    /// A value of TotCrFees.
    /// </summary>
    [JsonPropertyName("totCrFees")]
    public Common TotCrFees
    {
      get => totCrFees ??= new();
      set => totCrFees = value;
    }

    /// <summary>
    /// A value of TotFutureCrFees.
    /// </summary>
    [JsonPropertyName("totFutureCrFees")]
    public Common TotFutureCrFees
    {
      get => totFutureCrFees ??= new();
      set => totFutureCrFees = value;
    }

    /// <summary>
    /// A value of FormattedCollFundsAvail.
    /// </summary>
    [JsonPropertyName("formattedCollFundsAvail")]
    public WorkArea FormattedCollFundsAvail
    {
      get => formattedCollFundsAvail ??= new();
      set => formattedCollFundsAvail = value;
    }

    /// <summary>
    /// A value of DisbSign.
    /// </summary>
    [JsonPropertyName("disbSign")]
    public TextWorkArea DisbSign
    {
      get => disbSign ??= new();
      set => disbSign = value;
    }

    /// <summary>
    /// A value of Sign1.
    /// </summary>
    [JsonPropertyName("sign1")]
    public TextWorkArea Sign1
    {
      get => sign1 ??= new();
      set => sign1 = value;
    }

    /// <summary>
    /// A value of Sign2.
    /// </summary>
    [JsonPropertyName("sign2")]
    public TextWorkArea Sign2
    {
      get => sign2 ??= new();
      set => sign2 = value;
    }

    /// <summary>
    /// A value of Sign3.
    /// </summary>
    [JsonPropertyName("sign3")]
    public TextWorkArea Sign3
    {
      get => sign3 ??= new();
      set => sign3 = value;
    }

    /// <summary>
    /// A value of Sign4.
    /// </summary>
    [JsonPropertyName("sign4")]
    public TextWorkArea Sign4
    {
      get => sign4 ??= new();
      set => sign4 = value;
    }

    /// <summary>
    /// A value of Sign5.
    /// </summary>
    [JsonPropertyName("sign5")]
    public TextWorkArea Sign5
    {
      get => sign5 ??= new();
      set => sign5 = value;
    }

    /// <summary>
    /// A value of Sign6.
    /// </summary>
    [JsonPropertyName("sign6")]
    public TextWorkArea Sign6
    {
      get => sign6 ??= new();
      set => sign6 = value;
    }

    /// <summary>
    /// A value of FormattedDisbAmt.
    /// </summary>
    [JsonPropertyName("formattedDisbAmt")]
    public WorkArea FormattedDisbAmt
    {
      get => formattedDisbAmt ??= new();
      set => formattedDisbAmt = value;
    }

    /// <summary>
    /// A value of FormattedTotForADate.
    /// </summary>
    [JsonPropertyName("formattedTotForADate")]
    public WorkArea FormattedTotForADate
    {
      get => formattedTotForADate ??= new();
      set => formattedTotForADate = value;
    }

    /// <summary>
    /// A value of FormattedTotForAllDates.
    /// </summary>
    [JsonPropertyName("formattedTotForAllDates")]
    public WorkArea FormattedTotForAllDates
    {
      get => formattedTotForAllDates ??= new();
      set => formattedTotForAllDates = value;
    }

    /// <summary>
    /// A value of FormattedTotForSuppr.
    /// </summary>
    [JsonPropertyName("formattedTotForSuppr")]
    public WorkArea FormattedTotForSuppr
    {
      get => formattedTotForSuppr ??= new();
      set => formattedTotForSuppr = value;
    }

    /// <summary>
    /// A value of FormattedTotForXuraSuppr.
    /// </summary>
    [JsonPropertyName("formattedTotForXuraSuppr")]
    public WorkArea FormattedTotForXuraSuppr
    {
      get => formattedTotForXuraSuppr ??= new();
      set => formattedTotForXuraSuppr = value;
    }

    /// <summary>
    /// A value of FormattedTotForCrFees.
    /// </summary>
    [JsonPropertyName("formattedTotForCrFees")]
    public WorkArea FormattedTotForCrFees
    {
      get => formattedTotForCrFees ??= new();
      set => formattedTotForCrFees = value;
    }

    /// <summary>
    /// A value of FormattedTotForFutureCr.
    /// </summary>
    [JsonPropertyName("formattedTotForFutureCr")]
    public WorkArea FormattedTotForFutureCr
    {
      get => formattedTotForFutureCr ??= new();
      set => formattedTotForFutureCr = value;
    }

    /// <summary>
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    /// <summary>
    /// A value of UnformattedDate.
    /// </summary>
    [JsonPropertyName("unformattedDate")]
    public DateWorkArea UnformattedDate
    {
      get => unformattedDate ??= new();
      set => unformattedDate = value;
    }

    /// <summary>
    /// A value of HoldDateWorkArea.
    /// </summary>
    [JsonPropertyName("holdDateWorkArea")]
    public DateWorkArea HoldDateWorkArea
    {
      get => holdDateWorkArea ??= new();
      set => holdDateWorkArea = value;
    }

    /// <summary>
    /// A value of HoldDisbCollection.
    /// </summary>
    [JsonPropertyName("holdDisbCollection")]
    public DisbursementTransaction HoldDisbCollection
    {
      get => holdDisbCollection ??= new();
      set => holdDisbCollection = value;
    }

    /// <summary>
    /// A value of AdjustedCollections.
    /// </summary>
    [JsonPropertyName("adjustedCollections")]
    public Common AdjustedCollections
    {
      get => adjustedCollections ??= new();
      set => adjustedCollections = value;
    }

    /// <summary>
    /// A value of FormattedCrdAmt.
    /// </summary>
    [JsonPropertyName("formattedCrdAmt")]
    public WorkArea FormattedCrdAmt
    {
      get => formattedCrdAmt ??= new();
      set => formattedCrdAmt = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    private DisbursementTransaction disbursementTransaction;
    private NumericWorkSet unformatted;
    private WorkArea formattedAmt;
    private DisbursementTransaction totForAProcessDate;
    private DisbursementTransaction totForAllProcessDates;
    private Common totSupprDisb;
    private Common totCrFees;
    private Common totFutureCrFees;
    private WorkArea formattedCollFundsAvail;
    private TextWorkArea disbSign;
    private TextWorkArea sign1;
    private TextWorkArea sign2;
    private TextWorkArea sign3;
    private TextWorkArea sign4;
    private TextWorkArea sign5;
    private TextWorkArea sign6;
    private WorkArea formattedDisbAmt;
    private WorkArea formattedTotForADate;
    private WorkArea formattedTotForAllDates;
    private WorkArea formattedTotForSuppr;
    private WorkArea formattedTotForXuraSuppr;
    private WorkArea formattedTotForCrFees;
    private WorkArea formattedTotForFutureCr;
    private WorkArea formattedDate;
    private DateWorkArea unformattedDate;
    private DateWorkArea holdDateWorkArea;
    private DisbursementTransaction holdDisbCollection;
    private Common adjustedCollections;
    private WorkArea formattedCrdAmt;
    private DateWorkArea initialized;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
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
    /// A value of Existing1.
    /// </summary>
    [JsonPropertyName("existing1")]
    public DisbursementTransaction Existing1
    {
      get => existing1 ??= new();
      set => existing1 = value;
    }

    /// <summary>
    /// A value of Existing2.
    /// </summary>
    [JsonPropertyName("existing2")]
    public DisbursementTransaction Existing2
    {
      get => existing2 ??= new();
      set => existing2 = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
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
    /// A value of Db.
    /// </summary>
    [JsonPropertyName("db")]
    public DisbursementTransaction Db
    {
      get => db ??= new();
      set => db = value;
    }

    private CsePerson obligee;
    private Collection collection;
    private DisbursementTransaction existing1;
    private DisbursementTransaction existing2;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementType disbursementType;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction db;
  }
#endregion
}
