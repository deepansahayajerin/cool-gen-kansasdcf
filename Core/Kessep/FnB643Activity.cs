// Program: FN_B643_ACTIVITY, ID: 372683789, model: 746.
// Short name: SWE02392
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_ACTIVITY.
/// </summary>
[Serializable]
public partial class FnB643Activity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_ACTIVITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643Activity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643Activity.
  /// </summary>
  public FnB643Activity(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************************************************
    // DATE      PR#   PROGRAMMER  DESCRIPTION
    // ----------  -----  ----------  
    // -------------------------------
    // 02/03/2000  87256  Ed Lyman    Don't print payments that are
    //                                
    // adjusted in the same month they
    //                                
    // are created or adjustments due
    //                                
    // to program or case role change.
    // 11/03/2000  KPC    Ed Lyman    Consolidate debt adjustments.
    // **************************************************************
    export.VendorFileRecordCount.Count = import.VendorFileRecordCount.Count;
    export.SortSequenceNumber.Count = import.SortSequenceNumber.Count;
    export.SuppressCouponsByDebt.Flag = import.SuppressCouponsByDebt.Flag;
    export.CpnsSuppressedByDebt.Count = import.CpnsSuppressedByDebt.Count;

    // **************************************************************
    // Get ending monthly obligor summary for this obligation.
    // **************************************************************
    if (ReadMonthlyObligorSummary2())
    {
      local.EndBalanceAmt.AverageCurrency =
        entities.MonthlyObligorSummary.ForMthCurrBal.GetValueOrDefault();
    }
    else
    {
      ExitState = "FN0000_MTH_OBLIGOR_SUM_NF";

      return;
    }

    // **************************************************************
    // Get beginning monthly obligor summary for this obligation.
    // **************************************************************
    if (ReadMonthlyObligorSummary1())
    {
      local.BeginBalanceAmt.AverageCurrency =
        entities.MonthlyObligorSummary.ForMthCurrBal.GetValueOrDefault();
    }
    else
    {
      ExitState = "FN0000_MTH_OBLIGOR_SUM_NF";

      return;
    }

    local.RecordType.ActionEntry = "07";
    local.EabFileHandling.Action = "WRITE";
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;
    local.VariableLine1.RptDetail = "Beginning Balance Due";
    UseEabCreateVendorFile1();

    // **************************************************************
    // Obligation Transaction Types:
    // DE = Debt
    // DA = Debt Adjustment
    // Obligation Transaction Debt Types:
    // D = Debt
    // ? = Accrual Instructions
    // **************************************************************
    ++export.SortSequenceNumber.Count;

    // **************************************************************
    // Get payments due during the statement period.
    // **************************************************************
    local.AmountDue.AverageCurrency = 0;
    local.AmountReceived.AverageCurrency = 0;
    local.Previous.DueDt = local.Null1.Date;

    foreach(var item in ReadDebtDetailDebt())
    {
      if (Lt(local.Null1.Date, local.Previous.DueDt) && !
        Equal(entities.DebtDetail.DueDt, local.Previous.DueDt) && local
        .AmountDue.AverageCurrency != 0)
      {
        local.VariableLine1.RptDetail = "Payment Due";
        local.ActivityDate.Text10 =
          NumberToString(Month(local.Previous.DueDt), 14, 2) + "/" + NumberToString
          (Day(local.Previous.DueDt), 14, 2) + "/" + NumberToString
          (Year(local.Previous.DueDt), 12, 4);

        // **************************************************************
        // WRITE STATEMENT DETAILS (REC TYPE = 08)
        // **************************************************************
        local.RecordType.ActionEntry = "08";
        local.EabFileHandling.Action = "WRITE";
        ++export.VendorFileRecordCount.Count;
        UseEabCreateVendorFile3();
        local.AmountDue.AverageCurrency = 0;
      }

      local.Previous.DueDt = entities.DebtDetail.DueDt;
      local.AmountDue.AverageCurrency += entities.Debt.Amount;
    }

    if (local.AmountDue.AverageCurrency != 0)
    {
      local.VariableLine1.RptDetail = "Payment Due";
      local.ActivityDate.Text10 =
        NumberToString(Month(entities.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
        (Day(entities.DebtDetail.DueDt), 14, 2) + "/" + NumberToString
        (Year(entities.DebtDetail.DueDt), 12, 4);

      // **************************************************************
      // WRITE STATEMENT DETAILS (REC TYPE = 08)
      // **************************************************************
      local.RecordType.ActionEntry = "08";
      local.EabFileHandling.Action = "WRITE";
      ++export.VendorFileRecordCount.Count;
      UseEabCreateVendorFile3();
    }

    // **************************************************************
    // Get payments received during the statement period.
    // Summarize collections coming from the same cash receipt detail.
    // **************************************************************
    local.AmountDue.AverageCurrency = 0;

    foreach(var item in ReadCashReceiptDetailCollectionType2())
    {
      local.CollectionFound.Flag = "N";
      local.AmountReceived.AverageCurrency = 0;

      foreach(var item1 in ReadCollection4())
      {
        if (AsChar(entities.Collection.AdjustedInd) == 'Y' && !
          Lt(entities.Collection.CollectionAdjustmentDt,
          import.StmtBeginDateWorkArea.Date) && !
          Lt(import.StmtEndDateWorkArea.Date,
          entities.Collection.CollectionAdjustmentDt))
        {
          if (ReadCollectionAdjustmentReason())
          {
            switch(TrimEnd(entities.CollectionAdjustmentReason.Code))
            {
              case "WR ACCT":
                goto Test1;
              case "BAD CK":
                goto Test1;
              case "ST PMT":
                goto Test1;
              case "WR AMT":
                goto Test1;
              case "RETPCHG":
                break;
              case "RETCROL":
                break;
              default:
                break;
            }
          }
          else
          {
            goto Test1;
          }

          if (ReadCollection2())
          {
            // **************************************************************
            // * Collection was created, adjusted and then reapplied to the
            // * same obligation (not necessarily the same debt detail) in
            // * the same activity period, so don't report the earlier
            // * collection or the collection adjustment.  Simply report
            // * the later collection.
            // **************************************************************
            continue;
          }
        }
        else if (ReadCollection1())
        {
          // **************************************************************
          // * Collection was first created in a prior period.  During this
          // * period it was adjusted and reapplied to the same obligation
          // * (not necessarily the same debt detail).  Don't report it.
          // **************************************************************
          continue;
        }

Test1:

        local.CollectionFound.Flag = "Y";

        // ***********************************************************************
        // * Concurrent Ind = Y is a payment by other obligor on a joint debt
        // ***********************************************************************
        local.AmountReceived.AverageCurrency += entities.Collection.Amount;
      }

      if (AsChar(local.CollectionFound.Flag) == 'Y')
      {
        // **************************************************************
        // WRITE STATEMENT DETAILS (REC TYPE = 08)
        // **************************************************************
        switch(TrimEnd(entities.CollectionType.Code))
        {
          case "C":
            local.VariableLine1.RptDetail = "Payment -" + "Regular Collection";

            break;
          case "D":
            local.VariableLine1.RptDetail = "Payment -" + "Recovery Payment";

            break;
          case "F":
            local.VariableLine1.RptDetail = "Payment";

            break;
          case "S":
            local.VariableLine1.RptDetail = "Payment";

            break;
          case "U":
            local.VariableLine1.RptDetail = "Payment";

            break;
          case "I":
            local.VariableLine1.RptDetail = "Payment -" + "Income Withholding";
            export.SuppressCouponsByDebt.Flag = "Y";
            ++export.CpnsSuppressedByDebt.Count;

            break;
          case "P":
            local.VariableLine1.RptDetail = "Payment -" + "Fee";

            break;
          case "K":
            local.VariableLine1.RptDetail = "Payment";

            break;
          case "R":
            local.VariableLine1.RptDetail = "Payment";

            break;
          case "4":
            local.VariableLine1.RptDetail = "Direct Payment";

            break;
          case "A":
            local.VariableLine1.RptDetail = "Payment -" + "Collection Agency";

            break;
          case "B":
            local.VariableLine1.RptDetail = "Bad Check Recovery";

            break;
          case "M":
            local.VariableLine1.RptDetail = "Misdirected";

            break;
          case "N":
            local.VariableLine1.RptDetail = "Payment";

            break;
          case "T":
            local.VariableLine1.RptDetail = "Payment";

            break;
          case "5":
            local.VariableLine1.RptDetail = "Direct Payment";

            break;
          case "6":
            local.VariableLine1.RptDetail = "Direct Payment";

            break;
          case "V":
            local.VariableLine1.RptDetail = "Payment -" + "Voluntary";

            break;
          case "Y":
            local.VariableLine1.RptDetail = "Payment";

            break;
          case "Z":
            local.VariableLine1.RptDetail = "Would not use on statement";

            break;
          case "7":
            local.VariableLine1.RptDetail = "Payment" + " (on outgoing cases)";

            break;
          case "8":
            local.VariableLine1.RptDetail = "Payment" + " (on outgoing cases)";

            break;
          case "9":
            local.VariableLine1.RptDetail = "Payment" + " (on outgoing cases)";

            break;
          default:
            local.VariableLine1.RptDetail = "Payment - " + entities
              .CollectionType.PrintName;

            break;
        }

        local.ActivityDate.Text10 =
          NumberToString(Month(entities.CashReceiptDetail.CollectionDate), 14, 2)
          + "/" + NumberToString
          (Day(entities.CashReceiptDetail.CollectionDate), 14, 2) + "/" + NumberToString
          (Year(entities.CashReceiptDetail.CollectionDate), 12, 4);
        local.RecordType.ActionEntry = "08";
        local.EabFileHandling.Action = "WRITE";
        ++export.VendorFileRecordCount.Count;
        UseEabCreateVendorFile3();
      }
    }

    // **************************************************************
    // Get debt adjustments made during the statement period.
    // **************************************************************
    // *** NEW CODE FOLLOWS, NOW SUMMARIZING DEBT ADJUSTMENTS ***
    local.AmountReceived.AverageCurrency = 0;
    local.AmountDue.AverageCurrency = 0;

    foreach(var item in ReadDebtAdjustment())
    {
      local.VariableLine1.RptDetail = "Debt Adjustment";
      local.ActivityDate.Text10 =
        NumberToString(Month(entities.DebtAdjustment.DebtAdjustmentDt), 14, 2) +
        "/" + NumberToString
        (Day(entities.DebtAdjustment.DebtAdjustmentDt), 14, 2) + "/" + NumberToString
        (Year(entities.DebtAdjustment.DebtAdjustmentDt), 12, 4);

      // **************************************************************
      // Determine if Adjustment Increases or Decreases Balance
      // **************************************************************
      if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == 'I')
      {
        local.AmountDue.AverageCurrency = entities.DebtAdjustment.Amount + local
          .AmountDue.AverageCurrency;
      }
      else
      {
        local.AmountReceived.AverageCurrency =
          entities.DebtAdjustment.Amount + local
          .AmountReceived.AverageCurrency;
      }
    }

    if (local.AmountDue.AverageCurrency > 0 || local
      .AmountReceived.AverageCurrency > 0)
    {
      if (local.AmountDue.AverageCurrency > 0 && local
        .AmountReceived.AverageCurrency > 0)
      {
        if (local.AmountDue.AverageCurrency == local
          .AmountReceived.AverageCurrency)
        {
          // *** It is a wash ***
          goto Test2;
        }

        if (local.AmountDue.AverageCurrency > local
          .AmountReceived.AverageCurrency)
        {
          local.AmountDue.AverageCurrency -= local.AmountReceived.
            AverageCurrency;
          local.AmountReceived.AverageCurrency = 0;
        }
        else
        {
          local.AmountReceived.AverageCurrency -= local.AmountDue.
            AverageCurrency;
          local.AmountDue.AverageCurrency = 0;
        }
      }

      // **************************************************************
      // WRITE STATEMENT DETAILS (REC TYPE = 08)
      // **************************************************************
      local.RecordType.ActionEntry = "08";
      local.EabFileHandling.Action = "WRITE";
      ++export.VendorFileRecordCount.Count;
      UseEabCreateVendorFile3();
    }

Test2:

    // *** OLD CODE FOLLOWS ***
    // **************************************************************
    // Get collection adjustments made during the statement period.
    // **************************************************************
    foreach(var item in ReadCashReceiptDetailCollectionType1())
    {
      local.CollectionFound.Flag = "N";
      local.AmountDue.AverageCurrency = 0;
      local.AmountReceived.AverageCurrency = 0;

      foreach(var item1 in ReadCollection3())
      {
        if (ReadCollection2())
        {
          // **************************************************************
          // * Collection was created, adjusted and then reapplied to the
          // * same obligation (not necessarily the same debt detail) in
          // * the same activity period, so don't report the earlier
          // * collection or the collection adjustment.  Simply report
          // * the later collection.
          // **************************************************************
          continue;
        }

        if (ReadCollectionAdjustmentReason())
        {
          switch(TrimEnd(entities.CollectionAdjustmentReason.Code))
          {
            case "WR ACCT":
              local.VariableLine1.RptDetail = "Collection Adj: " + entities
                .CollectionAdjustmentReason.Name;

              break;
            case "BAD CK":
              local.VariableLine1.RptDetail = "Collection Adj: " + entities
                .CollectionAdjustmentReason.Name;

              break;
            case "ST PMT":
              local.VariableLine1.RptDetail = "Collection Adj: " + entities
                .CollectionAdjustmentReason.Name;

              break;
            case "WR AMT":
              local.VariableLine1.RptDetail = "Collection Adj: " + entities
                .CollectionAdjustmentReason.Name;

              break;
            case "RETPCHG":
              local.VariableLine1.RptDetail = "Collection Adjustment";

              break;
            case "RETCROL":
              local.VariableLine1.RptDetail = "Collection Adjustment";

              break;
            default:
              local.VariableLine1.RptDetail = "Collection Adjustment";

              break;
          }
        }
        else
        {
          local.VariableLine1.RptDetail = "Collection Adjustment";
        }

        // **************************************************************
        // Collection Adjustments always Increase the Balance
        // **************************************************************
        local.CollectionFound.Flag = "Y";
        local.AmountDue.AverageCurrency += entities.Collection.Amount;
      }

      if (AsChar(local.CollectionFound.Flag) == 'Y')
      {
        local.ActivityDate.Text10 =
          NumberToString(Month(entities.Collection.CollectionAdjustmentDt), 14,
          2) + "/" + NumberToString
          (Day(entities.Collection.CollectionAdjustmentDt), 14, 2) + "/" + NumberToString
          (Year(entities.Collection.CollectionAdjustmentDt), 12, 4);

        // **************************************************************
        // WRITE STATEMENT DETAILS (REC TYPE = 08)
        // **************************************************************
        local.RecordType.ActionEntry = "08";
        local.EabFileHandling.Action = "WRITE";
        ++export.VendorFileRecordCount.Count;
        UseEabCreateVendorFile3();
      }
    }

    local.RecordType.ActionEntry = "09";
    local.EabFileHandling.Action = "WRITE";
    ++export.VendorFileRecordCount.Count;
    ++export.SortSequenceNumber.Count;
    local.VariableLine1.RptDetail = "Ending Balance Due";
    UseEabCreateVendorFile2();
  }

  private void UseEabCreateVendorFile1()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.AmountOne.AverageCurrency = local.BeginBalanceAmt.AverageCurrency;
    useImport.DateOne.Text10 = import.StmtBeginTextWorkArea.Text10;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile2()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.AmountOne.AverageCurrency = local.EndBalanceAmt.AverageCurrency;
    useImport.DateOne.Text10 = import.StmtEndTextWorkArea.Text10;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCreateVendorFile3()
  {
    var useImport = new EabCreateVendorFile.Import();
    var useExport = new EabCreateVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.StmtNumber.Count = import.StmtNumber.Count;
    useImport.RecordNumber.Count = export.VendorFileRecordCount.Count;
    useImport.RecordType.ActionEntry = local.RecordType.ActionEntry;
    useImport.VariableLine1.RptDetail = local.VariableLine1.RptDetail;
    useImport.AmountOne.AverageCurrency = local.AmountDue.AverageCurrency;
    useImport.AmountTwo.AverageCurrency = local.AmountReceived.AverageCurrency;
    useImport.DateOne.Text10 = local.ActivityDate.Text10;
    useImport.SortSequenceNumber.Count = export.SortSequenceNumber.Count;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCreateVendorFile.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCashReceiptDetailCollectionType1()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return ReadEach("ReadCashReceiptDetailCollectionType1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDate(
          command, "date1",
          import.StmtBeginDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2",
          import.StmtEndDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 5);
        entities.CollectionType.PrintName = db.GetNullableString(reader, 6);
        entities.CollectionType.Code = db.GetString(reader, 7);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCollectionType2()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return ReadEach("ReadCashReceiptDetailCollectionType2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBeginDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.StmtEndDateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 5);
        entities.CollectionType.PrintName = db.GetNullableString(reader, 6);
        entities.CollectionType.Code = db.GetString(reader, 7);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;

        return true;
      });
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Other.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetDecimal(command, "obTrnAmt", entities.Collection.Amount);
        db.SetDateTime(
          command, "createdTmst",
          import.StmtBeginDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "date1",
          import.StmtBeginDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2",
          import.StmtEndDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Other.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Other.CrtType = db.GetInt32(reader, 2);
        entities.Other.CstId = db.GetInt32(reader, 3);
        entities.Other.CrvId = db.GetInt32(reader, 4);
        entities.Other.CrdId = db.GetInt32(reader, 5);
        entities.Other.ObgId = db.GetInt32(reader, 6);
        entities.Other.CspNumber = db.GetString(reader, 7);
        entities.Other.CpaType = db.GetString(reader, 8);
        entities.Other.OtrId = db.GetInt32(reader, 9);
        entities.Other.OtrType = db.GetString(reader, 10);
        entities.Other.OtyId = db.GetInt32(reader, 11);
        entities.Other.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Other.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Other.Amount = db.GetDecimal(reader, 14);
        entities.Other.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Other.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Other.CpaType);
        CheckValid<Collection>("OtrType", entities.Other.OtrType);
      });
  }

  private bool ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Other.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetDecimal(command, "obTrnAmt", entities.Collection.Amount);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBeginDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.StmtEndDateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Other.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Other.CrtType = db.GetInt32(reader, 2);
        entities.Other.CstId = db.GetInt32(reader, 3);
        entities.Other.CrvId = db.GetInt32(reader, 4);
        entities.Other.CrdId = db.GetInt32(reader, 5);
        entities.Other.ObgId = db.GetInt32(reader, 6);
        entities.Other.CspNumber = db.GetString(reader, 7);
        entities.Other.CpaType = db.GetString(reader, 8);
        entities.Other.OtrId = db.GetInt32(reader, 9);
        entities.Other.OtrType = db.GetString(reader, 10);
        entities.Other.OtyId = db.GetInt32(reader, 11);
        entities.Other.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Other.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Other.Amount = db.GetDecimal(reader, 14);
        entities.Other.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Other.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Other.CpaType);
        CheckValid<Collection>("OtrType", entities.Other.OtrType);
      });
  }

  private IEnumerable<bool> ReadCollection3()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection3",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetDate(
          command, "date1",
          import.StmtBeginDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2",
          import.StmtEndDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.CarId = db.GetNullableInt32(reader, 16);
        entities.Collection.OtyId = db.GetInt32(reader, 17);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 18);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 19);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 21);
        entities.Collection.Amount = db.GetDecimal(reader, 22);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 23);
        entities.Collection.DistributionMethod = db.GetString(reader, 24);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 25);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 26);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 27);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 28);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 29);
        entities.Collection.BalForIntCompBefColl =
          db.GetNullableDecimal(reader, 30);
        entities.Collection.CumIntChargedUptoColl =
          db.GetNullableDecimal(reader, 31);
        entities.Collection.CumIntCollAfterThisColl =
          db.GetNullableDecimal(reader, 32);
        entities.Collection.IntBalAftThisColl =
          db.GetNullableDecimal(reader, 33);
        entities.Collection.DisburseToArInd = db.GetNullableString(reader, 34);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 35);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 36);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection4()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection4",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBeginDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.StmtEndDateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.CarId = db.GetNullableInt32(reader, 16);
        entities.Collection.OtyId = db.GetInt32(reader, 17);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 18);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 19);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 21);
        entities.Collection.Amount = db.GetDecimal(reader, 22);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 23);
        entities.Collection.DistributionMethod = db.GetString(reader, 24);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 25);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 26);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 27);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 28);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 29);
        entities.Collection.BalForIntCompBefColl =
          db.GetNullableDecimal(reader, 30);
        entities.Collection.CumIntChargedUptoColl =
          db.GetNullableDecimal(reader, 31);
        entities.Collection.CumIntCollAfterThisColl =
          db.GetNullableDecimal(reader, 32);
        entities.Collection.IntBalAftThisColl =
          db.GetNullableDecimal(reader, 33);
        entities.Collection.DisburseToArInd = db.GetNullableString(reader, 34);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 35);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 36);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);

        return true;
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          entities.Collection.CarId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Name = db.GetString(reader, 2);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBeginDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.StmtEndDateWorkArea.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 12);
        entities.DebtAdjustment.DebtAdjCollAdjProcDt =
          db.GetNullableDate(reader, 13);
        entities.DebtAdjustment.ReasonCode = db.GetString(reader, 14);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailDebt()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;

    return ReadEach("ReadDebtDetailDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBeginDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.StmtEndDateWorkArea.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "dueDt",
          import.StmtEndDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(
          command, "date",
          import.StmtBeginDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.Debt.Amount = db.GetDecimal(reader, 9);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 10);
        entities.Debt.CreatedTmst = db.GetDateTime(reader, 11);
        entities.Debt.DebtType = db.GetString(reader, 12);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 13);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 14);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.Debt.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadMonthlyObligorSummary1()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.MonthlyObligorSummary.Populated = false;

    return Read("ReadMonthlyObligorSummary1",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "otyType", import.Obligation.DtyGeneratedId);
          
        db.SetNullableInt32(
          command, "obgSGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.
          SetNullableString(command, "cspSNumber", import.Obligation.CspNumber);
          
        db.SetNullableString(command, "cpaSType", import.Obligation.CpaType);
        db.SetInt32(command, "fnclMsumYrMth", import.Beginning.YearMonth);
      },
      (db, reader) =>
      {
        entities.MonthlyObligorSummary.Type1 = db.GetString(reader, 0);
        entities.MonthlyObligorSummary.YearMonth = db.GetInt32(reader, 1);
        entities.MonthlyObligorSummary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.MonthlyObligorSummary.CpaSType =
          db.GetNullableString(reader, 3);
        entities.MonthlyObligorSummary.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.MonthlyObligorSummary.ObgSGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.MonthlyObligorSummary.OtyType = db.GetNullableInt32(reader, 6);
        entities.MonthlyObligorSummary.ForMthCurrBal =
          db.GetNullableDecimal(reader, 7);
        entities.MonthlyObligorSummary.Populated = true;
        CheckValid<MonthlyObligorSummary>("Type1",
          entities.MonthlyObligorSummary.Type1);
        CheckValid<MonthlyObligorSummary>("CpaSType",
          entities.MonthlyObligorSummary.CpaSType);
      });
  }

  private bool ReadMonthlyObligorSummary2()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.MonthlyObligorSummary.Populated = false;

    return Read("ReadMonthlyObligorSummary2",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "otyType", import.Obligation.DtyGeneratedId);
          
        db.SetNullableInt32(
          command, "obgSGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.
          SetNullableString(command, "cspSNumber", import.Obligation.CspNumber);
          
        db.SetNullableString(command, "cpaSType", import.Obligation.CpaType);
        db.SetInt32(command, "fnclMsumYrMth", import.Ending.YearMonth);
      },
      (db, reader) =>
      {
        entities.MonthlyObligorSummary.Type1 = db.GetString(reader, 0);
        entities.MonthlyObligorSummary.YearMonth = db.GetInt32(reader, 1);
        entities.MonthlyObligorSummary.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.MonthlyObligorSummary.CpaSType =
          db.GetNullableString(reader, 3);
        entities.MonthlyObligorSummary.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.MonthlyObligorSummary.ObgSGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.MonthlyObligorSummary.OtyType = db.GetNullableInt32(reader, 6);
        entities.MonthlyObligorSummary.ForMthCurrBal =
          db.GetNullableDecimal(reader, 7);
        entities.MonthlyObligorSummary.Populated = true;
        CheckValid<MonthlyObligorSummary>("Type1",
          entities.MonthlyObligorSummary.Type1);
        CheckValid<MonthlyObligorSummary>("CpaSType",
          entities.MonthlyObligorSummary.CpaSType);
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
    /// A value of SuppressCouponsByDebt.
    /// </summary>
    [JsonPropertyName("suppressCouponsByDebt")]
    public Common SuppressCouponsByDebt
    {
      get => suppressCouponsByDebt ??= new();
      set => suppressCouponsByDebt = value;
    }

    /// <summary>
    /// A value of CpnsSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("cpnsSuppressedByDebt")]
    public Common CpnsSuppressedByDebt
    {
      get => cpnsSuppressedByDebt ??= new();
      set => cpnsSuppressedByDebt = value;
    }

    /// <summary>
    /// A value of Beginning.
    /// </summary>
    [JsonPropertyName("beginning")]
    public MonthlyObligorSummary Beginning
    {
      get => beginning ??= new();
      set => beginning = value;
    }

    /// <summary>
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public MonthlyObligorSummary Ending
    {
      get => ending ??= new();
      set => ending = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of StmtBeginTextWorkArea.
    /// </summary>
    [JsonPropertyName("stmtBeginTextWorkArea")]
    public TextWorkArea StmtBeginTextWorkArea
    {
      get => stmtBeginTextWorkArea ??= new();
      set => stmtBeginTextWorkArea = value;
    }

    /// <summary>
    /// A value of StmtBeginDateWorkArea.
    /// </summary>
    [JsonPropertyName("stmtBeginDateWorkArea")]
    public DateWorkArea StmtBeginDateWorkArea
    {
      get => stmtBeginDateWorkArea ??= new();
      set => stmtBeginDateWorkArea = value;
    }

    /// <summary>
    /// A value of StmtEndTextWorkArea.
    /// </summary>
    [JsonPropertyName("stmtEndTextWorkArea")]
    public TextWorkArea StmtEndTextWorkArea
    {
      get => stmtEndTextWorkArea ??= new();
      set => stmtEndTextWorkArea = value;
    }

    /// <summary>
    /// A value of StmtEndDateWorkArea.
    /// </summary>
    [JsonPropertyName("stmtEndDateWorkArea")]
    public DateWorkArea StmtEndDateWorkArea
    {
      get => stmtEndDateWorkArea ??= new();
      set => stmtEndDateWorkArea = value;
    }

    /// <summary>
    /// A value of StmtNumber.
    /// </summary>
    [JsonPropertyName("stmtNumber")]
    public Common StmtNumber
    {
      get => stmtNumber ??= new();
      set => stmtNumber = value;
    }

    /// <summary>
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private Common suppressCouponsByDebt;
    private Common cpnsSuppressedByDebt;
    private MonthlyObligorSummary beginning;
    private MonthlyObligorSummary ending;
    private CsePersonAccount obligor;
    private TextWorkArea stmtBeginTextWorkArea;
    private DateWorkArea stmtBeginDateWorkArea;
    private TextWorkArea stmtEndTextWorkArea;
    private DateWorkArea stmtEndDateWorkArea;
    private Common stmtNumber;
    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
    private DateWorkArea processDate;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of VendorFileRecordCount.
    /// </summary>
    [JsonPropertyName("vendorFileRecordCount")]
    public Common VendorFileRecordCount
    {
      get => vendorFileRecordCount ??= new();
      set => vendorFileRecordCount = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    /// <summary>
    /// A value of SuppressCouponsByDebt.
    /// </summary>
    [JsonPropertyName("suppressCouponsByDebt")]
    public Common SuppressCouponsByDebt
    {
      get => suppressCouponsByDebt ??= new();
      set => suppressCouponsByDebt = value;
    }

    /// <summary>
    /// A value of CpnsSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("cpnsSuppressedByDebt")]
    public Common CpnsSuppressedByDebt
    {
      get => cpnsSuppressedByDebt ??= new();
      set => cpnsSuppressedByDebt = value;
    }

    private EabFileHandling eabFileHandling;
    private Common vendorFileRecordCount;
    private Common sortSequenceNumber;
    private Common suppressCouponsByDebt;
    private Common cpnsSuppressedByDebt;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public DebtDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Beginning.
    /// </summary>
    [JsonPropertyName("beginning")]
    public MonthlyObligorSummary Beginning
    {
      get => beginning ??= new();
      set => beginning = value;
    }

    /// <summary>
    /// A value of CollectionFound.
    /// </summary>
    [JsonPropertyName("collectionFound")]
    public Common CollectionFound
    {
      get => collectionFound ??= new();
      set => collectionFound = value;
    }

    /// <summary>
    /// A value of AmountReceived.
    /// </summary>
    [JsonPropertyName("amountReceived")]
    public Common AmountReceived
    {
      get => amountReceived ??= new();
      set => amountReceived = value;
    }

    /// <summary>
    /// A value of AmountDue.
    /// </summary>
    [JsonPropertyName("amountDue")]
    public Common AmountDue
    {
      get => amountDue ??= new();
      set => amountDue = value;
    }

    /// <summary>
    /// A value of ActivityDate.
    /// </summary>
    [JsonPropertyName("activityDate")]
    public TextWorkArea ActivityDate
    {
      get => activityDate ??= new();
      set => activityDate = value;
    }

    /// <summary>
    /// A value of BeginBalanceAmt.
    /// </summary>
    [JsonPropertyName("beginBalanceAmt")]
    public Common BeginBalanceAmt
    {
      get => beginBalanceAmt ??= new();
      set => beginBalanceAmt = value;
    }

    /// <summary>
    /// A value of EndBalanceAmt.
    /// </summary>
    [JsonPropertyName("endBalanceAmt")]
    public Common EndBalanceAmt
    {
      get => endBalanceAmt ??= new();
      set => endBalanceAmt = value;
    }

    /// <summary>
    /// A value of VariableLine1.
    /// </summary>
    [JsonPropertyName("variableLine1")]
    public EabReportSend VariableLine1
    {
      get => variableLine1 ??= new();
      set => variableLine1 = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private DateWorkArea null1;
    private DebtDetail previous;
    private MonthlyObligorSummary beginning;
    private Common collectionFound;
    private Common amountReceived;
    private Common amountDue;
    private TextWorkArea activityDate;
    private Common beginBalanceAmt;
    private Common endBalanceAmt;
    private EabReportSend variableLine1;
    private Common recordType;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public Collection Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of MonthlyObligorSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligorSummary")]
    public MonthlyObligorSummary MonthlyObligorSummary
    {
      get => monthlyObligorSummary ??= new();
      set => monthlyObligorSummary = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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

    private Collection other;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransaction debtAdjustment;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Collection collection;
    private MonthlyObligorSummary monthlyObligorSummary;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
  }
#endregion
}
