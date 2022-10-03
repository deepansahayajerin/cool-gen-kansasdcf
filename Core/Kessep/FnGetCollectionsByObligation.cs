// Program: FN_GET_COLLECTIONS_BY_OBLIGATION, ID: 371738615, model: 746.
// Short name: SWE00467
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
/// A program: FN_GET_COLLECTIONS_BY_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block retrieves data for screen display.
/// </para>
/// </summary>
[Serializable]
public partial class FnGetCollectionsByObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_COLLECTIONS_BY_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetCollectionsByObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetCollectionsByObligation.
  /// </summary>
  public FnGetCollectionsByObligation(IContext context, Import import,
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
    // : 03/30/1999  A Doty    Modified the logic to support the calculation of 
    // summary totals real-time.  Removed the use of the Summary Entities.
    // ***---  6/22/99 - B Adams  -  Read properties set
    local.Max.Date = new DateTime(2099, 12, 31);

    // **** Get hardcode values
    UseFnHardcodedDebtDistribution();

    if (ReadCsePerson())
    {
      export.Search.Number = entities.CsePerson.Number;

      switch(AsChar(entities.CsePerson.Type1))
      {
        case 'C':
          // **** Client
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          break;
        case 'O':
          // **** Organization
          export.Search.FormattedName = entities.CsePerson.OrganizationName ?? Spaces
            (33);

          break;
        default:
          break;
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.Current.YearMonth = DateToInt(Now().Date) / 100;

    // **** Compute search from/to date
    UseFnDateEdit01From31To();

    if (Lt(export.SearchTo.Date, export.SearchFrom.Date))
    {
      ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

      return;
    }

    local.SearchTo.YearMonth = DateToInt(export.SearchTo.Date) / 100;

    // ***---  b adams - 6/16/99  -  combined 2 Reads
    if (ReadObligationObligationType())
    {
      export.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
      MoveObligationType(entities.ObligationType, export.ObligationType);
    }
    else
    {
      ExitState = "LCOB_OBLIGATION_NF";

      return;
    }

    // ***  Added logic to retrieve frequency -- E. Parker  10/7/98 ***
    if (AsChar(entities.ObligationType.Classification) == 'A')
    {
      if (ReadObligationPaymentSchedule())
      {
        export.ObligationPaymentSchedule.FrequencyCode =
          entities.ObligationPaymentSchedule.FrequencyCode;
      }
      else
      {
        ExitState = "FN0000_OBLIG_PYMNT_SCH_NF";
      }
    }

    // Set the amount based on the obligation type. If Accruing get the total 
    // for the accruing obligations, else get the obligations for the debt type
    // of D type
    // ***  Removed logic calculating Obligation Amount and added logic to use 
    // fn_cab_set_accrual_or_due_amount instead -- E. Parker 10/7/98 ***
    UseFnCabSetAccrualOrDueAmount();

    // **** The following logic may be changed. Each obligation may have more 
    // than one court order number assigned. Method of determining which  court
    // order correspond to this obligation will be decided later(7/19/95)
    if (ReadLegalAction())
    {
      MoveLegalAction(entities.LegalAction, export.LegalAction);
    }
    else
    {
      // **** Valid situation
    }

    // **** Calculate current owed, arrears owed, and interest owed.
    UseFnComputeSummaryTotals();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **** Read each DISTINCT Cash_Receipt_Detail which details the CURRENT 
    // obligation.
    // **** local_collection obligation_transaction type = "DE"
    // ****mfb  Qualified this read on COLLECTION entity.
    // ****mfb  previously it was qualified on Collection Obligation Txn entity.
    // *** Changed logic to bypass adjusted Collections -- E. Parker 11/5/1998 *
    // **
    // ***--- SKM read the status history and status tables
    //        to find the status..
    // ***--- removed Debt_Detail from R/E and referred to SOME; data not needed
    // ***--- but if it's an existence thing, this will provide that.   ba - 6/
    // 16
    // ***--- combined Read of Status with History; selection logic did not 
    // include
    // ***--- Discontinue_Date; put both into this R/E  -  bud adams - 6/16/99
    local.Group.Index = 0;
    local.Group.Clear();

    foreach(var item in ReadCashReceiptDetailCollectionObligationTransaction())
    {
      MoveCashReceiptDetail(entities.CashReceiptDetail,
        local.Group.Update.CashReceiptDetail);
      local.Group.Update.CashReceipt.Assign(entities.CashReceipt);

      if (ReadCashReceiptSourceTypeCashReceiptEventCashReceiptType())
      {
        MoveCashReceiptSourceType(entities.CashReceiptSourceType,
          local.Group.Update.CashReceiptSourceType);
        MoveCashReceiptEvent(entities.CashReceiptEvent,
          local.Group.Update.DetailCashReceiptEvent);
        MoveCashReceiptType(entities.CashReceiptType,
          local.Group.Update.DetailCashReceiptType);
      }
      else
      {
        ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
        local.Group.Next();

        break;
      }

      local.Group.Update.Common.SelectChar = "";
      local.Group.Update.Status.Code = entities.CashReceiptDetailStatus.Code;

      if (AsChar(entities.Collection.AdjustedInd) != 'Y')
      {
        local.Group.Update.DistToOblig.TotalCurrency =
          entities.Collection.Amount;
      }

      local.Group.Next();
    }

    // ---- Combine records with same collection id ----
    UseFnCombineCollectionsByOblig();
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveGroup(Local.GroupGroup source,
    FnCombineCollectionsByOblig.Import.GroupGroup target)
  {
    target.Status.Code = source.Status.Code;
    MoveCashReceiptEvent(source.DetailCashReceiptEvent, target.CashReceiptEvent);
      
    MoveCashReceiptType(source.DetailCashReceiptType, target.CashReceiptType);
    target.Common.SelectChar = source.Common.SelectChar;
    MoveCashReceiptSourceType(source.CashReceiptSourceType,
      target.CashReceiptSourceType);
    target.CashReceipt.Assign(source.CashReceipt);
    target.CashReceiptDetail.Assign(source.CashReceiptDetail);
    target.DistToOblig.TotalCurrency = source.DistToOblig.TotalCurrency;
  }

  private static void MoveGroupToExport1(FnCombineCollectionsByOblig.Export.
    GroupGroup source, Export.ExportGroup target)
  {
    target.Status.Code = source.Status.Code;
    MoveCashReceiptEvent(source.CashReceiptEvent, target.DetailCashReceiptEvent);
      
    MoveCashReceiptType(source.CashReceiptType, target.DetailCashReceiptType);
    target.Common.SelectChar = source.Common.SelectChar;
    MoveCashReceiptSourceType(source.CashReceiptSourceType,
      target.CashReceiptSourceType);
    target.CashReceipt.Assign(source.CashReceipt);
    target.CashReceiptDetail.Assign(source.CashReceiptDetail);
    target.DistToOblig.TotalCurrency = source.DistToOblig.TotalCurrency;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseFnCabSetAccrualOrDueAmount()
  {
    var useImport = new FnCabSetAccrualOrDueAmount.Import();
    var useExport = new FnCabSetAccrualOrDueAmount.Export();

    useImport.Obligation.SystemGeneratedIdentifier =
      import.SearchObligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = import.SearchCsePerson.Number;
    MoveObligationType(export.ObligationType, useImport.ObligationType);

    Call(FnCabSetAccrualOrDueAmount.Execute, useImport, useExport);

    export.AccrualOrDue.Date = useExport.StartDte.Date;
    export.TotalAmountDue.TotalCurrency = useExport.Common.TotalCurrency;
  }

  private void UseFnCombineCollectionsByOblig()
  {
    var useImport = new FnCombineCollectionsByOblig.Import();
    var useExport = new FnCombineCollectionsByOblig.Export();

    local.Group.CopyTo(useImport.Group, MoveGroup);

    Call(FnCombineCollectionsByOblig.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Export1, MoveGroupToExport1);
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Filter.SystemGeneratedIdentifier =
      entities.Obligation.SystemGeneratedIdentifier;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      entities.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligor.Number = entities.CsePerson.Number;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    export.TotalUndistAmt.TotalCurrency = useExport.UndistAmt.TotalCurrency;
  }

  private void UseFnDateEdit01From31To()
  {
    var useImport = new FnDateEdit01From31To.Import();
    var useExport = new FnDateEdit01From31To.Export();

    useImport.SearchFrom.Date = import.SearchFrom.Date;
    useImport.SearchTo.Date = import.SearchTo.Date;

    Call(FnDateEdit01From31To.Execute, useImport, useExport);

    export.SearchFrom.Date = useExport.SearchFrom.Date;
    export.SearchTo.Date = useExport.SearchTo.Date;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeObligation.Type1 = useExport.MosObligation.Type1;
    local.HardcodeDebt.Type1 = useExport.OtrnTDebt.Type1;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Search.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Search.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCollectionObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    return ReadEach("ReadCashReceiptDetailCollectionObligationTransaction",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", export.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", export.SearchTo.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrvId = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 9);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 13);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 14);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 15);
        entities.Collection.ObgId = db.GetInt32(reader, 16);
        entities.PrimaryDebt.ObgGeneratedId = db.GetInt32(reader, 16);
        entities.Collection.CspNumber = db.GetString(reader, 17);
        entities.PrimaryDebt.CspNumber = db.GetString(reader, 17);
        entities.Collection.CpaType = db.GetString(reader, 18);
        entities.PrimaryDebt.CpaType = db.GetString(reader, 18);
        entities.Collection.OtrId = db.GetInt32(reader, 19);
        entities.PrimaryDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 19);
        entities.Collection.OtrType = db.GetString(reader, 20);
        entities.PrimaryDebt.Type1 = db.GetString(reader, 20);
        entities.Collection.OtyId = db.GetInt32(reader, 21);
        entities.PrimaryDebt.OtyType = db.GetInt32(reader, 21);
        entities.Collection.Amount = db.GetDecimal(reader, 22);
        entities.PrimaryDebt.DebtType = db.GetString(reader, 23);
        entities.PrimaryDebt.CspSupNumber = db.GetNullableString(reader, 24);
        entities.PrimaryDebt.CpaSupType = db.GetNullableString(reader, 25);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 26);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 27);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 28);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 29);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 30);
        entities.CashReceipt.ReferenceNumber = db.GetNullableString(reader, 31);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 32);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 32);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 33);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 34);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 35);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 36);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 37);
        entities.PrimaryDebt.Populated = true;
        entities.Collection.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.PrimaryDebt.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.PrimaryDebt.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.PrimaryDebt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.PrimaryDebt.CpaSupType);

        return true;
      });
  }

  private bool ReadCashReceiptSourceTypeCashReceiptEventCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceTypeCashReceiptEventCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.CashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 3);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptType.Code = db.GetString(reader, 5);
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SearchCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.CreatedBy = db.GetString(reader, 3);
        entities.CsePerson.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligationObligationType()
  {
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", import.SearchObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.HardcodeObligor.Type1);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 5);
        entities.Obligation.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 6);
        entities.Obligation.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 7);
        entities.Obligation.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 8);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 9);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 10);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 11);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 12);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 13);
        entities.Obligation.TillDtNadArrColl =
          db.GetNullableDecimal(reader, 14);
        entities.Obligation.TillDtNadIntColl =
          db.GetNullableDecimal(reader, 15);
        entities.Obligation.TillDtAdcArrColl =
          db.GetNullableDecimal(reader, 16);
        entities.Obligation.TillDtAdcIntColl =
          db.GetNullableDecimal(reader, 17);
        entities.Obligation.AsOfDtTotRecColl =
          db.GetNullableDecimal(reader, 18);
        entities.Obligation.AsOfDtTotRecIntColl =
          db.GetNullableDecimal(reader, 19);
        entities.Obligation.AsOfDtTotFeeColl =
          db.GetNullableDecimal(reader, 20);
        entities.Obligation.AsOfDtTotFeeIntColl =
          db.GetNullableDecimal(reader, 21);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 22);
        entities.ObligationType.Code = db.GetString(reader, 23);
        entities.ObligationType.Classification = db.GetString(reader, 24);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 5);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
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
    /// A value of SearchObligation.
    /// </summary>
    [JsonPropertyName("searchObligation")]
    public Obligation SearchObligation
    {
      get => searchObligation ??= new();
      set => searchObligation = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    private Obligation searchObligation;
    private CsePerson searchCsePerson;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptDetailStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptType")]
      public CashReceiptType DetailCashReceiptType
      {
        get => detailCashReceiptType ??= new();
        set => detailCashReceiptType = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of CashReceipt.
      /// </summary>
      [JsonPropertyName("cashReceipt")]
      public CashReceipt CashReceipt
      {
        get => cashReceipt ??= new();
        set => cashReceipt = value;
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
      /// A value of DistToOblig.
      /// </summary>
      [JsonPropertyName("distToOblig")]
      public Common DistToOblig
      {
        get => distToOblig ??= new();
        set => distToOblig = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CashReceiptDetailStatus status;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceiptType detailCashReceiptType;
      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private Common distToOblig;
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
    /// A value of AccrualOrDue.
    /// </summary>
    [JsonPropertyName("accrualOrDue")]
    public DateWorkArea AccrualOrDue
    {
      get => accrualOrDue ??= new();
      set => accrualOrDue = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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
    /// A value of MthOblSummaryNfMsg.
    /// </summary>
    [JsonPropertyName("mthOblSummaryNfMsg")]
    public ErrorMessageText MthOblSummaryNfMsg
    {
      get => mthOblSummaryNfMsg ??= new();
      set => mthOblSummaryNfMsg = value;
    }

    /// <summary>
    /// A value of TotalAmountDue.
    /// </summary>
    [JsonPropertyName("totalAmountDue")]
    public Common TotalAmountDue
    {
      get => totalAmountDue ??= new();
      set => totalAmountDue = value;
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
    /// A value of ObligationPrompt.
    /// </summary>
    [JsonPropertyName("obligationPrompt")]
    public Common ObligationPrompt
    {
      get => obligationPrompt ??= new();
      set => obligationPrompt = value;
    }

    /// <summary>
    /// A value of TotalUndistAmt.
    /// </summary>
    [JsonPropertyName("totalUndistAmt")]
    public Common TotalUndistAmt
    {
      get => totalUndistAmt ??= new();
      set => totalUndistAmt = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Screen.
    /// </summary>
    [JsonPropertyName("screen")]
    public ObligationTransaction Screen
    {
      get => screen ??= new();
      set => screen = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
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

    private Obligation obligation;
    private DateWorkArea accrualOrDue;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Array<ExportGroup> export1;
    private ScreenOwedAmounts screenOwedAmounts;
    private ErrorMessageText mthOblSummaryNfMsg;
    private Common totalAmountDue;
    private LegalAction legalAction;
    private Common obligationPrompt;
    private Common totalUndistAmt;
    private CsePersonsWorkSet search;
    private ObligationTransaction screen;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private ObligationType obligationType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptDetailStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptType")]
      public CashReceiptType DetailCashReceiptType
      {
        get => detailCashReceiptType ??= new();
        set => detailCashReceiptType = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of CashReceipt.
      /// </summary>
      [JsonPropertyName("cashReceipt")]
      public CashReceipt CashReceipt
      {
        get => cashReceipt ??= new();
        set => cashReceipt = value;
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
      /// A value of DistToOblig.
      /// </summary>
      [JsonPropertyName("distToOblig")]
      public Common DistToOblig
      {
        get => distToOblig ??= new();
        set => distToOblig = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CashReceiptDetailStatus status;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceiptType detailCashReceiptType;
      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private Common distToOblig;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
    }

    /// <summary>
    /// A value of HardcodeObligation.
    /// </summary>
    [JsonPropertyName("hardcodeObligation")]
    public MonthlyObligorSummary HardcodeObligation
    {
      get => hardcodeObligation ??= new();
      set => hardcodeObligation = value;
    }

    /// <summary>
    /// A value of HardcodeDebt.
    /// </summary>
    [JsonPropertyName("hardcodeDebt")]
    public ObligationTransaction HardcodeDebt
    {
      get => hardcodeDebt ??= new();
      set => hardcodeDebt = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
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
    /// A value of DueDate.
    /// </summary>
    [JsonPropertyName("dueDate")]
    public DateWorkArea DueDate
    {
      get => dueDate ??= new();
      set => dueDate = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public NullDate NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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

    private Array<GroupGroup> group;
    private CsePersonAccount hardcodeObligor;
    private MonthlyObligorSummary hardcodeObligation;
    private ObligationTransaction hardcodeDebt;
    private Common common;
    private ObligationTransaction obligationTransaction;
    private DateWorkArea searchTo;
    private DateWorkArea current;
    private DateWorkArea dueDate;
    private NullDate nullDate;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PrimaryDebt.
    /// </summary>
    [JsonPropertyName("primaryDebt")]
    public ObligationTransaction PrimaryDebt
    {
      get => primaryDebt ??= new();
      set => primaryDebt = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of ZdelCollection.
    /// </summary>
    [JsonPropertyName("zdelCollection")]
    public Collection ZdelCollection
    {
      get => zdelCollection ??= new();
      set => zdelCollection = value;
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
    /// A value of ZdelObligationTransaction.
    /// </summary>
    [JsonPropertyName("zdelObligationTransaction")]
    public ObligationTransaction ZdelObligationTransaction
    {
      get => zdelObligationTransaction ??= new();
      set => zdelObligationTransaction = value;
    }

    /// <summary>
    /// A value of SecondaryDebt.
    /// </summary>
    [JsonPropertyName("secondaryDebt")]
    public ObligationTransaction SecondaryDebt
    {
      get => secondaryDebt ??= new();
      set => secondaryDebt = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private ObligationTransaction primaryDebt;
    private Collection collection;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptType cashReceiptType;
    private LegalActionDetail legalActionDetail;
    private Collection zdelCollection;
    private Obligation obligation;
    private ObligationTransaction zdelObligationTransaction;
    private ObligationTransaction secondaryDebt;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationType obligationType;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private DebtDetail debtDetail;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private MonthlyObligorSummary monthlyObligorSummary;
    private LegalAction legalAction;
  }
#endregion
}
