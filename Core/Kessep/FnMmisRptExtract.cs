// Program: FN_MMIS_RPT_EXTRACT, ID: 372814550, model: 746.
// Short name: SWE00230
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_MMIS_RPT_EXTRACT.
/// </summary>
[Serializable]
public partial class FnMmisRptExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_MMIS_RPT_EXTRACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnMmisRptExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnMmisRptExtract.
  /// </summary>
  public FnMmisRptExtract(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***  Initial code completed July, 1999 swsrmxk
    // ***
    // MAINTENANCE LOG
    // ===============================================
    // Jan 25, 2000, M. Brown, PR# 80970: Added 'NF' to program applied to
    // qualifiers in the read of collections.
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.LnfErrorMessageCrDetail.Text50 =
      "CASH RECEIPT DETAIL NOT FOUND FOR COLLECTION";
    local.LnfErrorMessageSupported.Text50 =
      "SUPPORTED PERSON NOT FOUND FOR COLLECTION";
    local.LnfErrorMessageInvalidCode.Text50 = "INVALID CODE FOR COLLECTION";
    local.LnfErrorMessageXsErrors.Text50 = "MORE ERRORS OCCURED THAN REPORTED";

    // ***   Set DATE OF PROGRAM EXECUTION to CURRENT DATE
    // ***
    local.LdateOfReportRun.Date = Now().Date;

    // ***   Calculate REPORT PERIOD START DATE
    // ***             REPORT PERIOD STOP DATE
    // ***        and  START DATE OF FOLLOWING MONTH.
    // ***
    // ***  PROGRAM PROCESSING DATE MUST BE
    // ***  REPORT PERIOD START DATE.
    // ***
    // ***   Validate date exists and is 1st of month
    // ***
    if (import.RunMonthStart.Date == null)
    {
      export.RcErrorMessage.RptDetail = "Processing Date is NULL.";
      ExitState = "ACO_NI0000_INVALID_DATE";

      return;
    }

    if (Day(import.RunMonthStart.Date) != 1)
    {
      export.RcErrorMessage.RptDetail =
        "Processing Date is not first day of month.";
      ExitState = "ACO_NI0000_INVALID_DATE";

      return;
    }

    local.LdateMonthStart.Date = import.RunMonthStart.Date;
    local.LdateMonthNextStart.Date = AddMonths(import.RunMonthStart.Date, 1);
    local.LdateMonthStop.Date = AddDays(local.LdateMonthNextStart.Date, -1);

    // ***   Calculate PROCESSING MONTH START DATETIMESTAMP
    // ***
    UseCabDate2TextWithHyphens1();
    local.LdateMonthStart.Timestamp = Timestamp(local.L.Text10);

    // ***   Calculate PROCESSING MONTH NEXT START DATETIMESTAMP
    // ***
    UseCabDate2TextWithHyphens2();
    local.LdateMonthNextStart.Timestamp = Timestamp(local.L.Text10);

    // *** PreRead Obligation Type SYSID for codes MC, MJ, MS.
    // ***
    foreach(var item in ReadObligationType())
    {
      switch(TrimEnd(entities.EobligationType.Code))
      {
        case "MC":
          MoveObligationType(entities.EobligationType, local.LmcCode);

          break;
        case "MJ":
          MoveObligationType(entities.EobligationType, local.LmjCode);

          break;
        case "MS":
          MoveObligationType(entities.EobligationType, local.LmsCode);

          break;
        default:
          break;
      }
    }

    // ***  Call RC4 EAB to INITIALIZE output file.
    // ***
    local.LreportComposerSend.Parm1 = "OF";
    local.LreportComposerSend.Parm2 = "";
    UseEabMmisRptRc1();

    if (!IsEmpty(local.LreportComposerReturn.Parm1))
    {
      // ******************************
      // FATAL ERROR CONDITION
      // FILE NOT OPENED
      // ******************************
      UseCabRc4ErrorCodeTranslation();

      return;
    }

    local.LreportComposerSend.Parm1 = "GR";
    local.LreportComposerSend.Parm2 = "";

    // *** Set control break and initialize error count.
    // ***
    local.LreportControlBreak.Text33 = "           COLLECTIONS";
    export.NfError1.Subscript = 0;

    // *** Read MC, MJ, MS Unadjusted Collections for the processing month.
    // ***
    foreach(var item in ReadCollectionObligationTransactionObligationObligationType1())
      
    {
      // *** If the APPLIED TO FUTURE FLAG is Y the Collection is processed if 
      // distributed in the processing month.
      // ***
      // *** BLANK REPORT LINE INFORMATION
      // ***
      local.LreportLineCashReceiptDetail.Assign(local.LblankCashReceiptDetail);
      local.LreportLineCollection.Assign(local.LblankCollection);
      local.LreportLineSupported.Number = local.LblankCsePerson.Number;
      local.LreportLineProcessedDate.Date = local.LblankDateWorkArea.Date;
      local.LreportLineObligationType.Code = local.LblankObligationType.Code;
      local.LerrorFlag.Flag = "";

      // *** PROCESS UNADJUSTED COLLECTION
      // ***
      // *****************************************************************
      // *** READ C R DETAIL for Payor Information.
      // ***
      if (ReadCashReceiptDetail())
      {
        // *** FALL THRU and process.
        // ***
      }
      else
      {
        // ******************************
        // ERROR CONDITION TO HANDLE
        // ******************************
        // ***CHECK IF VIEW IS FULL
        // ***
        if (export.NfError.Index + 1 < Export.NfErrorGroup.Capacity)
        {
          ++export.NfError1.Subscript;

          export.NfError.Index = export.NfError1.Subscript - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            entities.CashReceiptDetail.SequentialIdentifier;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageCrDetail.Text50 + NumberToString
            (export.NfError.Item.GrpmbrNfCollection.SystemGeneratedIdentifier,
            7, 15);
        }
        else
        {
          // ***SET REPORT MESSAGE TO INDICATE VIEW IS FULL
          // ***CONTINUE PROCESSING
          // ***
          // ***THIS IS VERY UNLIKELY
          // ***
          export.NfError.Index = Export.NfErrorGroup.Capacity - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            0;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageXsErrors.Text50;
        }

        ++local.LsumErrors.Number120;
        local.LsumErrors.Currency132 += entities.Collection.Amount;
        local.LerrorFlag.Flag = "Y";
      }

      // *** READ to Determine Supported Person Number.
      // ***
      if (ReadCsePersonCsePersonAccount())
      {
        // *** FALL THRU and process.
        // ***
      }
      else
      {
        // ******************************
        // ERROR CONDITION TO HANDLE
        // ******************************
        // ***CHECK IF VIEW IS FULL
        // ***
        if (export.NfError.Index + 1 < Export.NfErrorGroup.Capacity)
        {
          ++export.NfError1.Subscript;

          export.NfError.Index = export.NfError1.Subscript - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            entities.CashReceiptDetail.SequentialIdentifier;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageSupported.Text50 + NumberToString
            (export.NfError.Item.GrpmbrNfCollection.SystemGeneratedIdentifier,
            7, 15);
        }
        else
        {
          // ***SET REPORT MESSAGE TO INDICATE VIEW IS FULL
          // ***CONTINUE PROCESSING
          // ***
          // ***THIS IS VERY UNLIKELY
          // ***
          export.NfError.Index = Export.NfErrorGroup.Capacity - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            0;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageXsErrors.Text50;
        }

        if (IsEmpty(local.LerrorFlag.Flag))
        {
          ++local.LsumErrors.Number120;
          local.LsumErrors.Currency132 += entities.Collection.Amount;
          local.LerrorFlag.Flag = "Y";
        }
      }

      // ***  NF ERROR
      // ***  DO NOT PROCESS FURTHER
      // ***
      if (!IsEmpty(local.LerrorFlag.Flag))
      {
        continue;
      }

      // ***  Set up Report Record Line.
      // ***
      local.LreportLineProcessedDate.Date =
        Date(entities.Collection.CreatedTmst);
      MoveCollection(entities.Collection, local.LreportLineCollection);
      local.LreportLineSupported.Number = entities.Supported.Number;
      local.LreportLineCashReceiptDetail.Assign(entities.CashReceiptDetail);
      local.LreportLineCsePersonsWorkSet.FirstName =
        local.LreportLineCashReceiptDetail.ObligorFirstName ?? Spaces(12);
      local.LreportLineCsePersonsWorkSet.MiddleInitial =
        local.LreportLineCashReceiptDetail.ObligorMiddleName ?? Spaces(1);
      local.LreportLineCsePersonsWorkSet.LastName =
        local.LreportLineCashReceiptDetail.ObligorLastName ?? Spaces(17);

      if (IsEmpty(local.LreportLineCsePersonsWorkSet.FirstName) && IsEmpty
        (local.LreportLineCsePersonsWorkSet.LastName) && IsEmpty
        (local.LreportLineCsePersonsWorkSet.MiddleInitial))
      {
        local.LreportLineCsePersonsWorkSet.FormattedName = "";
      }
      else
      {
        local.LreportLineCsePersonsWorkSet.FormattedName =
          TrimEnd(local.LreportLineCsePersonsWorkSet.LastName) + "," + TrimEnd
          (local.LreportLineCsePersonsWorkSet.FirstName) + " " + local
          .LreportLineCsePersonsWorkSet.MiddleInitial;
      }

      if (entities.ObligationType.SystemGeneratedIdentifier == local
        .LmcCode.SystemGeneratedIdentifier)
      {
        local.LreportLineObligationType.Code = "MC";
        ++local.LsumNonadjMc.Number120;
        local.LsumNonadjMc.Currency132 += entities.Collection.Amount;
      }
      else if (entities.ObligationType.SystemGeneratedIdentifier == local
        .LmjCode.SystemGeneratedIdentifier)
      {
        local.LreportLineObligationType.Code = "MJ";
        ++local.LsumNonadjMj.Number120;
        local.LsumNonadjMj.Currency132 += entities.Collection.Amount;
      }
      else if (entities.ObligationType.SystemGeneratedIdentifier == local
        .LmsCode.SystemGeneratedIdentifier)
      {
        local.LreportLineObligationType.Code = "MS";
        ++local.LsumNonadjMs.Number120;
        local.LsumNonadjMs.Currency132 += entities.Collection.Amount;
      }
      else
      {
        // ******************************
        // ERROR INVALID OBLIGATION TYPE
        // ******************************
        // ***CHECK IF VIEW IS FULL
        // ***
        if (export.NfError.Index + 1 < Export.NfErrorGroup.Capacity)
        {
          ++export.NfError1.Subscript;

          export.NfError.Index = export.NfError1.Subscript - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            entities.CashReceiptDetail.SequentialIdentifier;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageInvalidCode.Text50 + NumberToString
            (export.NfError.Item.GrpmbrNfCollection.SystemGeneratedIdentifier,
            7, 15);
        }
        else
        {
          // ***SET REPORT MESSAGE TO INDICATE VIEW IS FULL
          // ***CONTINUE PROCESSING
          // ***
          // ***THIS IS VERY UNLIKELY
          // ***
          export.NfError.Index = Export.NfErrorGroup.Capacity - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            0;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageXsErrors.Text50;
        }

        ++local.LsumErrors.Number120;
        local.LsumErrors.Currency132 += entities.Collection.Amount;

        continue;
      }

      // ***  Calculate Summary Data
      // ***
      ++local.LsumNonadjOverall.Number120;
      local.LsumNonadjOverall.Currency132 += entities.Collection.Amount;

      // *** CALCULATE OVERALL AND NET TOTALS
      // ***
      local.LsumOverallMc.Number120 = local.LsumNonadjMc.Number120 + local
        .LsumAdjMc.Number120;
      local.LsumOverallMj.Number120 = local.LsumNonadjMj.Number120 + local
        .LsumAdjMj.Number120;
      local.LsumOverallMs.Number120 = local.LsumNonadjMs.Number120 + local
        .LsumAdjMs.Number120;
      local.LsumOverallNet.Number120 = local.LsumOverallMc.Number120 + local
        .LsumOverallMj.Number120 + local.LsumOverallMs.Number120;
      local.LsumOverallMc.Currency132 = local.LsumNonadjMc.Currency132 - local
        .LsumAdjMc.Currency132;
      local.LsumOverallMj.Currency132 = local.LsumNonadjMj.Currency132 - local
        .LsumAdjMj.Currency132;
      local.LsumOverallMs.Currency132 = local.LsumNonadjMs.Currency132 - local
        .LsumAdjMs.Currency132;
      local.LsumOverallNet.Currency132 = local.LsumOverallMc.Currency132 + local
        .LsumOverallMj.Currency132 + local.LsumOverallMs.Currency132;

      // ***  Call RC4 EAB to write record.
      // ***
      UseEabMmisRptRc3();

      if (!IsEmpty(local.LreportComposerReturn.Parm1))
      {
        // ******************************************
        // ERROR CONDITION WRITING RECORD
        // ******************************************
        UseCabRc4ErrorCodeTranslation();

        return;
      }
    }

    // *** PROCESS ADJUSTED COLLECTIONS
    // ***
    // *****************************************************************
    // *** Read MC, MJ, MS Adjusted Collections for the processing month.
    // ***
    // *** Set control break and initialize error count.
    // ***
    local.LreportControlBreak.Text33 = "           ADJUSTMENTS";

    foreach(var item in ReadCollectionObligationTransactionObligationObligationType2())
      
    {
      // *** BLANK REPORT LINE INFORMATION
      // ***
      local.LreportLineCashReceiptDetail.Assign(local.LblankCashReceiptDetail);
      local.LreportLineCollection.Assign(local.LblankCollection);
      local.LreportLineSupported.Number = local.LblankCsePerson.Number;
      local.LreportLineProcessedDate.Date = local.LblankDateWorkArea.Date;
      local.LreportLineObligationType.Code = local.LblankObligationType.Code;
      local.LerrorFlag.Flag = "";

      // ***  If collection was distributed and adjusted in same month the net 
      // effect is 0.  Therefore, do not process or report these records.
      if (Month(entities.Collection.CreatedTmst) == Month
        (entities.Collection.CollectionAdjustmentDt) && Year
        (entities.Collection.CreatedTmst) == Year
        (entities.Collection.CollectionAdjustmentDt))
      {
        continue;
      }

      // *** PROCESS ADJUSTED COLLECTION
      // ***
      // *** READ C R DETAIL for Payor Information.
      // ***
      if (ReadCashReceiptDetail())
      {
        // *** FALL THRU and process.
      }
      else
      {
        // ******************************
        // ERROR CONDITION
        // ******************************
        // ***CHECK IF VIEW IS FULL
        // ***
        if (export.NfError.Index + 1 < Export.NfErrorGroup.Capacity)
        {
          ++export.NfError1.Subscript;

          export.NfError.Index = export.NfError1.Subscript - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            entities.CashReceiptDetail.SequentialIdentifier;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageCrDetail.Text50 + NumberToString
            (export.NfError.Item.GrpmbrNfCollection.SystemGeneratedIdentifier,
            7, 15);
        }
        else
        {
          // ***SET REPORT MESSAGE TO INDICATE VIEW IS FULL
          // ***CONTINUE PROCESSING
          // ***
          // ***THIS IS VERY UNLIKELY
          // ***
          export.NfError.Index = Export.NfErrorGroup.Capacity - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            0;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageXsErrors.Text50;
        }

        ++local.LsumErrors.Number120;
        local.LsumErrors.Currency132 += entities.Collection.Amount;
        local.LerrorFlag.Flag = "Y";
      }

      // *** READ to Determine Supported Person Number.
      // ***
      if (ReadCsePersonCsePersonAccount())
      {
        // *** FALL THRU and process.
      }
      else
      {
        // ******************************
        // ERROR CONDITION
        // ******************************
        // ***CHECK IF VIEW IS FULL
        // ***
        if (export.NfError.Index + 1 < Export.NfErrorGroup.Capacity)
        {
          ++export.NfError1.Subscript;

          export.NfError.Index = export.NfError1.Subscript - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            entities.CashReceiptDetail.SequentialIdentifier;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageSupported.Text50 + NumberToString
            (export.NfError.Item.GrpmbrNfCollection.SystemGeneratedIdentifier,
            7, 15);
        }
        else
        {
          // ***SET REPORT MESSAGE TO INDICATE VIEW IS FULL
          // ***CONTINUE PROCESSING
          // ***
          // ***THIS IS VERY UNLIKELY
          // ***
          export.NfError.Index = Export.NfErrorGroup.Capacity - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            0;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageXsErrors.Text50;
        }

        if (IsEmpty(local.LerrorFlag.Flag))
        {
          ++local.LsumErrors.Number120;
          local.LsumErrors.Currency132 += entities.Collection.Amount;
          local.LerrorFlag.Flag = "Y";
        }
      }

      // ***  NF ERROR
      // ***  DO NOT PROCESS FURTHER
      // ***
      if (!IsEmpty(local.LerrorFlag.Flag))
      {
        continue;
      }

      // ***  Set up Report Record Line.
      // ***
      local.LreportLineProcessedDate.Date =
        entities.Collection.CollectionAdjustmentDt;
      MoveCollection(entities.Collection, local.LreportLineCollection);
      local.LreportLineSupported.Number = entities.Supported.Number;
      local.LreportLineCashReceiptDetail.Assign(entities.CashReceiptDetail);
      local.LreportLineCsePersonsWorkSet.FirstName =
        local.LreportLineCashReceiptDetail.ObligorFirstName ?? Spaces(12);
      local.LreportLineCsePersonsWorkSet.MiddleInitial =
        local.LreportLineCashReceiptDetail.ObligorMiddleName ?? Spaces(1);
      local.LreportLineCsePersonsWorkSet.LastName =
        local.LreportLineCashReceiptDetail.ObligorLastName ?? Spaces(17);

      if (IsEmpty(local.LreportLineCsePersonsWorkSet.FirstName) && IsEmpty
        (local.LreportLineCsePersonsWorkSet.LastName) && IsEmpty
        (local.LreportLineCsePersonsWorkSet.MiddleInitial))
      {
        local.LreportLineCsePersonsWorkSet.FormattedName = "";
      }
      else
      {
        local.LreportLineCsePersonsWorkSet.FormattedName =
          TrimEnd(local.LreportLineCsePersonsWorkSet.LastName) + "," + TrimEnd
          (local.LreportLineCsePersonsWorkSet.FirstName) + " " + local
          .LreportLineCsePersonsWorkSet.MiddleInitial;
      }

      if (entities.ObligationType.SystemGeneratedIdentifier == local
        .LmcCode.SystemGeneratedIdentifier)
      {
        local.LreportLineObligationType.Code = "MC";
        ++local.LsumAdjMc.Number120;
        local.LsumAdjMc.Currency132 += entities.Collection.Amount;
      }
      else if (entities.ObligationType.SystemGeneratedIdentifier == local
        .LmjCode.SystemGeneratedIdentifier)
      {
        local.LreportLineObligationType.Code = "MJ";
        ++local.LsumAdjMj.Number120;
        local.LsumAdjMj.Currency132 += entities.Collection.Amount;
      }
      else if (entities.ObligationType.SystemGeneratedIdentifier == local
        .LmsCode.SystemGeneratedIdentifier)
      {
        local.LreportLineObligationType.Code = "MS";
        ++local.LsumAdjMs.Number120;
        local.LsumAdjMs.Currency132 += entities.Collection.Amount;
      }
      else
      {
        // ******************************
        // ERROR INVALID OBLIGATION TYPE
        // ******************************
        // ***CHECK IF VIEW IS FULL
        // ***
        if (export.NfError.Index + 1 < Export.NfErrorGroup.Capacity)
        {
          ++export.NfError1.Subscript;

          export.NfError.Index = export.NfError1.Subscript - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            entities.CashReceiptDetail.SequentialIdentifier;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageInvalidCode.Text50 + NumberToString
            (export.NfError.Item.GrpmbrNfCollection.SystemGeneratedIdentifier,
            7, 15);
        }
        else
        {
          // ***SET REPORT MESSAGE TO INDICATE VIEW IS FULL
          // ***CONTINUE PROCESSING
          // ***
          // ***THIS IS VERY UNLIKELY
          // ***
          export.NfError.Index = Export.NfErrorGroup.Capacity - 1;
          export.NfError.CheckSize();

          export.NfError.Update.GrpmbrNfCollection.SystemGeneratedIdentifier =
            0;
          export.NfError.Update.GrpmbrNfEabReportSend.RptDetail =
            local.LnfErrorMessageXsErrors.Text50;
        }

        ++local.LsumErrors.Number120;
        local.LsumErrors.Currency132 += entities.Collection.Amount;

        continue;
      }

      // ***  Calculate Summary Data
      // ***
      ++local.LsumAdjOverall.Number120;
      local.LsumAdjOverall.Currency132 += entities.Collection.Amount;

      // *** CALCULATE OVERALL AND NET TOTALS
      // ***
      local.LsumOverallMc.Number120 = local.LsumNonadjMc.Number120 + local
        .LsumAdjMc.Number120;
      local.LsumOverallMj.Number120 = local.LsumNonadjMj.Number120 + local
        .LsumAdjMj.Number120;
      local.LsumOverallMs.Number120 = local.LsumNonadjMs.Number120 + local
        .LsumAdjMs.Number120;
      local.LsumOverallNet.Number120 = local.LsumOverallMc.Number120 + local
        .LsumOverallMj.Number120 + local.LsumOverallMs.Number120;
      local.LsumOverallMc.Currency132 = local.LsumNonadjMc.Currency132 - local
        .LsumAdjMc.Currency132;
      local.LsumOverallMj.Currency132 = local.LsumNonadjMj.Currency132 - local
        .LsumAdjMj.Currency132;
      local.LsumOverallMs.Currency132 = local.LsumNonadjMs.Currency132 - local
        .LsumAdjMs.Currency132;
      local.LsumOverallNet.Currency132 = local.LsumOverallMc.Currency132 + local
        .LsumOverallMj.Currency132 + local.LsumOverallMs.Currency132;

      // ***  Call RC4 EAB to write record.
      // ***
      UseEabMmisRptRc2();

      if (!IsEmpty(local.LreportComposerReturn.Parm1))
      {
        // ******************************************
        // ERROR CONDITION WRITING RECORD
        // ******************************************
        UseCabRc4ErrorCodeTranslation();

        return;
      }
    }

    // ***  Call RC4 EAB to close output file.
    // ***
    local.LreportComposerSend.Parm1 = "CF";
    local.LreportComposerSend.Parm2 = "";
    UseEabMmisRptRc1();

    if (!IsEmpty(local.LreportComposerReturn.Parm1))
    {
      // ******************************
      // FATAL ERROR CONDITION
      // FILE NOT CLOSED
      // ******************************
      UseCabRc4ErrorCodeTranslation();
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.CollectionDt = source.CollectionDt;
    target.DisbursementDt = source.DisbursementDt;
    target.AdjustedInd = source.AdjustedInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveReportTotals(ReportTotals source, ReportTotals target)
  {
    target.Number120 = source.Number120;
    target.Currency132 = source.Currency132;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.LdateMonthStart.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.L.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.LdateMonthNextStart.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.L.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabRc4ErrorCodeTranslation()
  {
    var useImport = new CabRc4ErrorCodeTranslation.Import();
    var useExport = new CabRc4ErrorCodeTranslation.Export();

    useImport.Report1Composer.Assign(local.LreportComposerReturn);

    Call(CabRc4ErrorCodeTranslation.Execute, useImport, useExport);

    export.RcErrorMessage.RptDetail = useExport.RcErrorMessage.RptDetail;
  }

  private void UseEabMmisRptRc1()
  {
    var useImport = new EabMmisRptRc4.Import();
    var useExport = new EabMmisRptRc4.Export();

    useImport.ReportParms.Assign(local.LreportComposerSend);
    useImport.DateRptPeriodStart.Date = local.LdateMonthStart.Date;
    useImport.DateRptPeriodEnd.Date = local.LdateMonthStop.Date;
    useImport.DateOfReportRun.Date = local.LdateOfReportRun.Date;
    useExport.ReportParms.Assign(local.LreportComposerReturn);

    Call(EabMmisRptRc4.Execute, useImport, useExport);

    local.LreportComposerReturn.Assign(useExport.ReportParms);
  }

  private void UseEabMmisRptRc2()
  {
    var useImport = new EabMmisRptRc4.Import();
    var useExport = new EabMmisRptRc4.Export();

    useImport.ReportParms.Assign(local.LreportComposerSend);
    useImport.DateRptPeriodStart.Date = local.LdateMonthStart.Date;
    useImport.DateRptPeriodEnd.Date = local.LdateMonthStop.Date;
    useImport.DateOfReportRun.Date = local.LdateOfReportRun.Date;
    useImport.ControlBreak.Text33 = local.LreportControlBreak.Text33;
    useImport.CsePersonsWorkSet.FormattedName =
      local.LreportLineCsePersonsWorkSet.FormattedName;
    useImport.ReportLineCashReceiptDetail.ObligorPersonNumber =
      local.LreportLineCashReceiptDetail.ObligorPersonNumber;
    useImport.ReportLineCollection.Assign(local.LreportLineCollection);
    useImport.ReportLineDistributeDt.Date = local.LreportLineProcessedDate.Date;
    useImport.ReportLineSupported.Number = local.LreportLineSupported.Number;
    useImport.ReportLineObligationType.Code =
      local.LreportLineObligationType.Code;
    MoveReportTotals(local.LsumAdjMc, useImport.SumMc);
    MoveReportTotals(local.LsumAdjMj, useImport.SumMj);
    MoveReportTotals(local.LsumAdjMs, useImport.SumMs);
    MoveReportTotals(local.LsumAdjOverall, useImport.Total);
    MoveReportTotals(local.LsumOverallMc, useImport.FtotalMc);
    MoveReportTotals(local.LsumOverallMj, useImport.FtotalMj);
    MoveReportTotals(local.LsumOverallMs, useImport.FtotalMs);
    MoveReportTotals(local.LsumOverallNet, useImport.FoverallTotal);
    MoveReportTotals(local.LsumErrors, useImport.Errors);
    useExport.ReportParms.Assign(local.LreportComposerReturn);

    Call(EabMmisRptRc4.Execute, useImport, useExport);

    local.LreportComposerReturn.Assign(useExport.ReportParms);
  }

  private void UseEabMmisRptRc3()
  {
    var useImport = new EabMmisRptRc4.Import();
    var useExport = new EabMmisRptRc4.Export();

    useImport.ReportParms.Assign(local.LreportComposerSend);
    useImport.DateRptPeriodStart.Date = local.LdateMonthStart.Date;
    useImport.DateRptPeriodEnd.Date = local.LdateMonthStop.Date;
    useImport.DateOfReportRun.Date = local.LdateOfReportRun.Date;
    useImport.ControlBreak.Text33 = local.LreportControlBreak.Text33;
    useImport.CsePersonsWorkSet.FormattedName =
      local.LreportLineCsePersonsWorkSet.FormattedName;
    useImport.ReportLineCashReceiptDetail.ObligorPersonNumber =
      local.LreportLineCashReceiptDetail.ObligorPersonNumber;
    useImport.ReportLineCollection.Assign(local.LreportLineCollection);
    useImport.ReportLineDistributeDt.Date = local.LreportLineProcessedDate.Date;
    useImport.ReportLineSupported.Number = local.LreportLineSupported.Number;
    useImport.ReportLineObligationType.Code =
      local.LreportLineObligationType.Code;
    MoveReportTotals(local.LsumNonadjMc, useImport.SumMc);
    MoveReportTotals(local.LsumNonadjMj, useImport.SumMj);
    MoveReportTotals(local.LsumNonadjMs, useImport.SumMs);
    MoveReportTotals(local.LsumNonadjOverall, useImport.Total);
    MoveReportTotals(local.LsumOverallMc, useImport.FtotalMc);
    MoveReportTotals(local.LsumOverallMj, useImport.FtotalMj);
    MoveReportTotals(local.LsumOverallMs, useImport.FtotalMs);
    MoveReportTotals(local.LsumOverallNet, useImport.FoverallTotal);
    MoveReportTotals(local.LsumErrors, useImport.Errors);
    useExport.ReportParms.Assign(local.LreportComposerReturn);

    Call(EabMmisRptRc4.Execute, useImport, useExport);

    local.LreportComposerReturn.Assign(useExport.ReportParms);
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionObligationTransactionObligationObligationType1()
  {
    entities.Collection.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach(
      "ReadCollectionObligationTransactionObligationObligationType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.LmcCode.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.LmjCode.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.LmsCode.SystemGeneratedIdentifier);
        db.SetDateTime(
          command, "createdTmst1",
          local.LdateMonthStart.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTmst2",
          local.LdateMonthNextStart.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.CrtType = db.GetInt32(reader, 6);
        entities.Collection.CstId = db.GetInt32(reader, 7);
        entities.Collection.CrvId = db.GetInt32(reader, 8);
        entities.Collection.CrdId = db.GetInt32(reader, 9);
        entities.Collection.ObgId = db.GetInt32(reader, 10);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 10);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.Collection.CspNumber = db.GetString(reader, 11);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 11);
        entities.Obligation.CspNumber = db.GetString(reader, 11);
        entities.Obligation.CspNumber = db.GetString(reader, 11);
        entities.Collection.CpaType = db.GetString(reader, 12);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 12);
        entities.Obligation.CpaType = db.GetString(reader, 12);
        entities.Obligation.CpaType = db.GetString(reader, 12);
        entities.Collection.OtrId = db.GetInt32(reader, 13);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.Collection.OtrType = db.GetString(reader, 14);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 14);
        entities.Collection.OtyId = db.GetInt32(reader, 15);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 15);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 15);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 15);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 16);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 17);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 18);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 19);
        entities.Collection.Amount = db.GetDecimal(reader, 20);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 21);
        entities.Collection.AppliedToFuture = db.GetString(reader, 22);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 23);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 24);
        entities.Collection.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionObligationTransactionObligationObligationType2()
  {
    entities.Collection.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.ObligationType.Populated = false;

    return ReadEach(
      "ReadCollectionObligationTransactionObligationObligationType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.LmcCode.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.LmjCode.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.LmsCode.SystemGeneratedIdentifier);
        db.SetDate(
          command, "collAdjDt1",
          local.LdateMonthStart.Date.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt2",
          local.LdateMonthNextStart.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.CrtType = db.GetInt32(reader, 6);
        entities.Collection.CstId = db.GetInt32(reader, 7);
        entities.Collection.CrvId = db.GetInt32(reader, 8);
        entities.Collection.CrdId = db.GetInt32(reader, 9);
        entities.Collection.ObgId = db.GetInt32(reader, 10);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 10);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.Collection.CspNumber = db.GetString(reader, 11);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 11);
        entities.Obligation.CspNumber = db.GetString(reader, 11);
        entities.Obligation.CspNumber = db.GetString(reader, 11);
        entities.Collection.CpaType = db.GetString(reader, 12);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 12);
        entities.Obligation.CpaType = db.GetString(reader, 12);
        entities.Obligation.CpaType = db.GetString(reader, 12);
        entities.Collection.OtrId = db.GetInt32(reader, 13);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.Collection.OtrType = db.GetString(reader, 14);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 14);
        entities.Collection.OtyId = db.GetInt32(reader, 15);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 15);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 15);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 15);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 16);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 17);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 18);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 19);
        entities.Collection.Amount = db.GetDecimal(reader, 20);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 21);
        entities.Collection.AppliedToFuture = db.GetString(reader, 22);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 23);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 24);
        entities.Collection.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadCsePersonCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.CsePersonAccount.Populated = false;
    entities.Supported.Populated = false;

    return Read("ReadCsePersonCsePersonAccount",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspSupNumber",
          entities.ObligationTransaction.CspSupNumber ?? "");
        db.SetNullableString(
          command, "cpaSupType", entities.ObligationTransaction.CpaSupType ?? ""
          );
      },
      (db, reader) =>
      {
        entities.Supported.Number = db.GetString(reader, 0);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 2);
        entities.CsePersonAccount.Populated = true;
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool> ReadObligationType()
  {
    entities.EobligationType.Populated = false;

    return ReadEach("ReadObligationType",
      null,
      (db, reader) =>
      {
        entities.EobligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.EobligationType.Code = db.GetString(reader, 1);
        entities.EobligationType.Populated = true;

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
    /// A value of RunMonthStart.
    /// </summary>
    [JsonPropertyName("runMonthStart")]
    public DateWorkArea RunMonthStart
    {
      get => runMonthStart ??= new();
      set => runMonthStart = value;
    }

    private DateWorkArea runMonthStart;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A NfErrorGroup group.</summary>
    [Serializable]
    public class NfErrorGroup
    {
      /// <summary>
      /// A value of GrpmbrNfCollection.
      /// </summary>
      [JsonPropertyName("grpmbrNfCollection")]
      public Collection GrpmbrNfCollection
      {
        get => grpmbrNfCollection ??= new();
        set => grpmbrNfCollection = value;
      }

      /// <summary>
      /// A value of GrpmbrNfEabReportSend.
      /// </summary>
      [JsonPropertyName("grpmbrNfEabReportSend")]
      public EabReportSend GrpmbrNfEabReportSend
      {
        get => grpmbrNfEabReportSend ??= new();
        set => grpmbrNfEabReportSend = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Collection grpmbrNfCollection;
      private EabReportSend grpmbrNfEabReportSend;
    }

    /// <summary>
    /// A value of RcErrorMessage.
    /// </summary>
    [JsonPropertyName("rcErrorMessage")]
    public EabReportSend RcErrorMessage
    {
      get => rcErrorMessage ??= new();
      set => rcErrorMessage = value;
    }

    /// <summary>
    /// A value of NfError1.
    /// </summary>
    [JsonPropertyName("nfError1")]
    public Common NfError1
    {
      get => nfError1 ??= new();
      set => nfError1 = value;
    }

    /// <summary>
    /// Gets a value of NfError.
    /// </summary>
    [JsonIgnore]
    public Array<NfErrorGroup> NfError => nfError ??= new(
      NfErrorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of NfError for json serialization.
    /// </summary>
    [JsonPropertyName("nfError")]
    [Computed]
    public IList<NfErrorGroup> NfError_Json
    {
      get => nfError;
      set => NfError.Assign(value);
    }

    private EabReportSend rcErrorMessage;
    private Common nfError1;
    private Array<NfErrorGroup> nfError;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LblankCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("lblankCashReceiptDetail")]
    public CashReceiptDetail LblankCashReceiptDetail
    {
      get => lblankCashReceiptDetail ??= new();
      set => lblankCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of LblankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("lblankCsePersonsWorkSet")]
    public CsePersonsWorkSet LblankCsePersonsWorkSet
    {
      get => lblankCsePersonsWorkSet ??= new();
      set => lblankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LblankCollection.
    /// </summary>
    [JsonPropertyName("lblankCollection")]
    public Collection LblankCollection
    {
      get => lblankCollection ??= new();
      set => lblankCollection = value;
    }

    /// <summary>
    /// A value of LblankCsePerson.
    /// </summary>
    [JsonPropertyName("lblankCsePerson")]
    public CsePerson LblankCsePerson
    {
      get => lblankCsePerson ??= new();
      set => lblankCsePerson = value;
    }

    /// <summary>
    /// A value of LblankObligationType.
    /// </summary>
    [JsonPropertyName("lblankObligationType")]
    public ObligationType LblankObligationType
    {
      get => lblankObligationType ??= new();
      set => lblankObligationType = value;
    }

    /// <summary>
    /// A value of LblankDateWorkArea.
    /// </summary>
    [JsonPropertyName("lblankDateWorkArea")]
    public DateWorkArea LblankDateWorkArea
    {
      get => lblankDateWorkArea ??= new();
      set => lblankDateWorkArea = value;
    }

    /// <summary>
    /// A value of LdateMonthStart.
    /// </summary>
    [JsonPropertyName("ldateMonthStart")]
    public DateWorkArea LdateMonthStart
    {
      get => ldateMonthStart ??= new();
      set => ldateMonthStart = value;
    }

    /// <summary>
    /// A value of LdateMonthStop.
    /// </summary>
    [JsonPropertyName("ldateMonthStop")]
    public DateWorkArea LdateMonthStop
    {
      get => ldateMonthStop ??= new();
      set => ldateMonthStop = value;
    }

    /// <summary>
    /// A value of LdateMonthNextStart.
    /// </summary>
    [JsonPropertyName("ldateMonthNextStart")]
    public DateWorkArea LdateMonthNextStart
    {
      get => ldateMonthNextStart ??= new();
      set => ldateMonthNextStart = value;
    }

    /// <summary>
    /// A value of LdateOfReportRun.
    /// </summary>
    [JsonPropertyName("ldateOfReportRun")]
    public DateWorkArea LdateOfReportRun
    {
      get => ldateOfReportRun ??= new();
      set => ldateOfReportRun = value;
    }

    /// <summary>
    /// A value of LreportComposerSend.
    /// </summary>
    [JsonPropertyName("lreportComposerSend")]
    public ReportParms LreportComposerSend
    {
      get => lreportComposerSend ??= new();
      set => lreportComposerSend = value;
    }

    /// <summary>
    /// A value of LreportComposerReturn.
    /// </summary>
    [JsonPropertyName("lreportComposerReturn")]
    public ReportParms LreportComposerReturn
    {
      get => lreportComposerReturn ??= new();
      set => lreportComposerReturn = value;
    }

    /// <summary>
    /// A value of LmcCode.
    /// </summary>
    [JsonPropertyName("lmcCode")]
    public ObligationType LmcCode
    {
      get => lmcCode ??= new();
      set => lmcCode = value;
    }

    /// <summary>
    /// A value of LmjCode.
    /// </summary>
    [JsonPropertyName("lmjCode")]
    public ObligationType LmjCode
    {
      get => lmjCode ??= new();
      set => lmjCode = value;
    }

    /// <summary>
    /// A value of LmsCode.
    /// </summary>
    [JsonPropertyName("lmsCode")]
    public ObligationType LmsCode
    {
      get => lmsCode ??= new();
      set => lmsCode = value;
    }

    /// <summary>
    /// A value of LreportControlBreak.
    /// </summary>
    [JsonPropertyName("lreportControlBreak")]
    public ReportText LreportControlBreak
    {
      get => lreportControlBreak ??= new();
      set => lreportControlBreak = value;
    }

    /// <summary>
    /// A value of LreportLineCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("lreportLineCashReceiptDetail")]
    public CashReceiptDetail LreportLineCashReceiptDetail
    {
      get => lreportLineCashReceiptDetail ??= new();
      set => lreportLineCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of LreportLineCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("lreportLineCsePersonsWorkSet")]
    public CsePersonsWorkSet LreportLineCsePersonsWorkSet
    {
      get => lreportLineCsePersonsWorkSet ??= new();
      set => lreportLineCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LreportLineCollection.
    /// </summary>
    [JsonPropertyName("lreportLineCollection")]
    public Collection LreportLineCollection
    {
      get => lreportLineCollection ??= new();
      set => lreportLineCollection = value;
    }

    /// <summary>
    /// A value of LreportLineSupported.
    /// </summary>
    [JsonPropertyName("lreportLineSupported")]
    public CsePerson LreportLineSupported
    {
      get => lreportLineSupported ??= new();
      set => lreportLineSupported = value;
    }

    /// <summary>
    /// A value of LreportLineObligationType.
    /// </summary>
    [JsonPropertyName("lreportLineObligationType")]
    public ObligationType LreportLineObligationType
    {
      get => lreportLineObligationType ??= new();
      set => lreportLineObligationType = value;
    }

    /// <summary>
    /// A value of LreportLineProcessedDate.
    /// </summary>
    [JsonPropertyName("lreportLineProcessedDate")]
    public DateWorkArea LreportLineProcessedDate
    {
      get => lreportLineProcessedDate ??= new();
      set => lreportLineProcessedDate = value;
    }

    /// <summary>
    /// A value of LsumAdjMc.
    /// </summary>
    [JsonPropertyName("lsumAdjMc")]
    public ReportTotals LsumAdjMc
    {
      get => lsumAdjMc ??= new();
      set => lsumAdjMc = value;
    }

    /// <summary>
    /// A value of LsumAdjMj.
    /// </summary>
    [JsonPropertyName("lsumAdjMj")]
    public ReportTotals LsumAdjMj
    {
      get => lsumAdjMj ??= new();
      set => lsumAdjMj = value;
    }

    /// <summary>
    /// A value of LsumAdjMs.
    /// </summary>
    [JsonPropertyName("lsumAdjMs")]
    public ReportTotals LsumAdjMs
    {
      get => lsumAdjMs ??= new();
      set => lsumAdjMs = value;
    }

    /// <summary>
    /// A value of LsumAdjOverall.
    /// </summary>
    [JsonPropertyName("lsumAdjOverall")]
    public ReportTotals LsumAdjOverall
    {
      get => lsumAdjOverall ??= new();
      set => lsumAdjOverall = value;
    }

    /// <summary>
    /// A value of LsumNonadjMc.
    /// </summary>
    [JsonPropertyName("lsumNonadjMc")]
    public ReportTotals LsumNonadjMc
    {
      get => lsumNonadjMc ??= new();
      set => lsumNonadjMc = value;
    }

    /// <summary>
    /// A value of LsumNonadjMj.
    /// </summary>
    [JsonPropertyName("lsumNonadjMj")]
    public ReportTotals LsumNonadjMj
    {
      get => lsumNonadjMj ??= new();
      set => lsumNonadjMj = value;
    }

    /// <summary>
    /// A value of LsumNonadjMs.
    /// </summary>
    [JsonPropertyName("lsumNonadjMs")]
    public ReportTotals LsumNonadjMs
    {
      get => lsumNonadjMs ??= new();
      set => lsumNonadjMs = value;
    }

    /// <summary>
    /// A value of LsumNonadjOverall.
    /// </summary>
    [JsonPropertyName("lsumNonadjOverall")]
    public ReportTotals LsumNonadjOverall
    {
      get => lsumNonadjOverall ??= new();
      set => lsumNonadjOverall = value;
    }

    /// <summary>
    /// A value of LsumOverallMc.
    /// </summary>
    [JsonPropertyName("lsumOverallMc")]
    public ReportTotals LsumOverallMc
    {
      get => lsumOverallMc ??= new();
      set => lsumOverallMc = value;
    }

    /// <summary>
    /// A value of LsumOverallMj.
    /// </summary>
    [JsonPropertyName("lsumOverallMj")]
    public ReportTotals LsumOverallMj
    {
      get => lsumOverallMj ??= new();
      set => lsumOverallMj = value;
    }

    /// <summary>
    /// A value of LsumOverallMs.
    /// </summary>
    [JsonPropertyName("lsumOverallMs")]
    public ReportTotals LsumOverallMs
    {
      get => lsumOverallMs ??= new();
      set => lsumOverallMs = value;
    }

    /// <summary>
    /// A value of LsumOverallNet.
    /// </summary>
    [JsonPropertyName("lsumOverallNet")]
    public ReportTotals LsumOverallNet
    {
      get => lsumOverallNet ??= new();
      set => lsumOverallNet = value;
    }

    /// <summary>
    /// A value of LsumErrors.
    /// </summary>
    [JsonPropertyName("lsumErrors")]
    public ReportTotals LsumErrors
    {
      get => lsumErrors ??= new();
      set => lsumErrors = value;
    }

    /// <summary>
    /// A value of L.
    /// </summary>
    [JsonPropertyName("l")]
    public TextWorkArea L
    {
      get => l ??= new();
      set => l = value;
    }

    /// <summary>
    /// A value of LnfErrorMessageCrDetail.
    /// </summary>
    [JsonPropertyName("lnfErrorMessageCrDetail")]
    public WorkArea LnfErrorMessageCrDetail
    {
      get => lnfErrorMessageCrDetail ??= new();
      set => lnfErrorMessageCrDetail = value;
    }

    /// <summary>
    /// A value of LnfErrorMessageSupported.
    /// </summary>
    [JsonPropertyName("lnfErrorMessageSupported")]
    public WorkArea LnfErrorMessageSupported
    {
      get => lnfErrorMessageSupported ??= new();
      set => lnfErrorMessageSupported = value;
    }

    /// <summary>
    /// A value of LnfErrorMessageInvalidCode.
    /// </summary>
    [JsonPropertyName("lnfErrorMessageInvalidCode")]
    public WorkArea LnfErrorMessageInvalidCode
    {
      get => lnfErrorMessageInvalidCode ??= new();
      set => lnfErrorMessageInvalidCode = value;
    }

    /// <summary>
    /// A value of LnfErrorMessageXsErrors.
    /// </summary>
    [JsonPropertyName("lnfErrorMessageXsErrors")]
    public WorkArea LnfErrorMessageXsErrors
    {
      get => lnfErrorMessageXsErrors ??= new();
      set => lnfErrorMessageXsErrors = value;
    }

    /// <summary>
    /// A value of LerrorFlag.
    /// </summary>
    [JsonPropertyName("lerrorFlag")]
    public Common LerrorFlag
    {
      get => lerrorFlag ??= new();
      set => lerrorFlag = value;
    }

    private CashReceiptDetail lblankCashReceiptDetail;
    private CsePersonsWorkSet lblankCsePersonsWorkSet;
    private Collection lblankCollection;
    private CsePerson lblankCsePerson;
    private ObligationType lblankObligationType;
    private DateWorkArea lblankDateWorkArea;
    private DateWorkArea ldateMonthStart;
    private DateWorkArea ldateMonthStop;
    private DateWorkArea ldateMonthNextStart;
    private DateWorkArea ldateOfReportRun;
    private ReportParms lreportComposerSend;
    private ReportParms lreportComposerReturn;
    private ObligationType lmcCode;
    private ObligationType lmjCode;
    private ObligationType lmsCode;
    private ReportText lreportControlBreak;
    private CashReceiptDetail lreportLineCashReceiptDetail;
    private CsePersonsWorkSet lreportLineCsePersonsWorkSet;
    private Collection lreportLineCollection;
    private CsePerson lreportLineSupported;
    private ObligationType lreportLineObligationType;
    private DateWorkArea lreportLineProcessedDate;
    private ReportTotals lsumAdjMc;
    private ReportTotals lsumAdjMj;
    private ReportTotals lsumAdjMs;
    private ReportTotals lsumAdjOverall;
    private ReportTotals lsumNonadjMc;
    private ReportTotals lsumNonadjMj;
    private ReportTotals lsumNonadjMs;
    private ReportTotals lsumNonadjOverall;
    private ReportTotals lsumOverallMc;
    private ReportTotals lsumOverallMj;
    private ReportTotals lsumOverallMs;
    private ReportTotals lsumOverallNet;
    private ReportTotals lsumErrors;
    private TextWorkArea l;
    private WorkArea lnfErrorMessageCrDetail;
    private WorkArea lnfErrorMessageSupported;
    private WorkArea lnfErrorMessageInvalidCode;
    private WorkArea lnfErrorMessageXsErrors;
    private Common lerrorFlag;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of EdebtDetail.
    /// </summary>
    [JsonPropertyName("edebtDetail")]
    public DebtDetail EdebtDetail
    {
      get => edebtDetail ??= new();
      set => edebtDetail = value;
    }

    /// <summary>
    /// A value of EobligationType.
    /// </summary>
    [JsonPropertyName("eobligationType")]
    public ObligationType EobligationType
    {
      get => eobligationType ??= new();
      set => eobligationType = value;
    }

    /// <summary>
    /// A value of Ekey.
    /// </summary>
    [JsonPropertyName("ekey")]
    public CashReceipt Ekey
    {
      get => ekey ??= new();
      set => ekey = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private DebtDetail edebtDetail;
    private ObligationType eobligationType;
    private CashReceipt ekey;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private CsePerson supported;
  }
#endregion
}
