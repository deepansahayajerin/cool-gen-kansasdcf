// Program: FN_CAB_REPLICATE_ONE_COLLECTION, ID: 372265518, model: 746.
// Short name: SWE02345
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_REPLICATE_ONE_COLLECTION.
/// </summary>
[Serializable]
public partial class FnCabReplicateOneCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_REPLICATE_ONE_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabReplicateOneCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabReplicateOneCollection.
  /// </summary>
  public FnCabReplicateOneCollection(IContext context, Import import,
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
    // ************************************************************************
    //   Date      Programmer   Problem #   Description
    // ----------  ----------   ---------
    // 
    // ----------------------------------
    // 10/20/1999  E. Lyman     H00074221   Fix view matching to FN DETERMINE
    //                                      
    // PGM FOR DEBT DETAIL.
    // 10/27/1999  E. Lyman     H00A74221   Don't set Disbursement Processing 
    // Need
    //                                      
    // Ind on Collection.
    // 10/28/1999  E. Lyman     H00A74221   Check Obligation Type to see if a
    //                                      
    // Supported Person is required.
    // 12/01/1999  E. Lyman     PR# 81189   Check Type of Change to determine
    //                                      
    // how attributes should be set.
    // 12/20/1999  E. Lyman     PR# 82843   Do not replicate if derived
    //                                      
    // program same as previous program.
    // 04/20/2000  E. Shirk     PRWORA      Added call to FN_UPDATE_URA_AMT when
    //                                      
    // collection successfully adjusted.
    // 11/30/2000  E. Lyman     PR# 108246  New attributes are not being 
    // replicated:
    //                                      
    // dist_pgm_state_appld_to and
    //                                      
    // ocse34_reporting_period.
    // ************************************************************************
    // The purpose of this action block is to replicate the existing
    // collection. This is done to trigger disbursement processing on the
    // collection because of a program change or a case role change. The 
    // existing
    // collection will be then be adjusted.  Because the existing collection
    // (now adjusted) and new collection will offset each other, no financial
    // changes are needed. ***
    // ************************************************************************
    // ************************************************************************************
    // Derive the new COLLECTION identifier using generate 9 digit random
    // number routine.   This is called in the actual create statement.
    // ***********************************************************************************
    if (ReadCashReceiptDetailDebtDebtDetail())
    {
      if (ReadObligationObligationType())
      {
        if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
        {
          if (!ReadClient())
          {
            ExitState = "SUPPORTED_PERSON_NF_RB";

            return;
          }
        }
        else
        {
          return;
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_NF";

        return;
      }

      if (!ReadCashReceiptType())
      {
        ExitState = "FN0000_CASH_RECEIPT_TYPE_NF";

        return;
      }

      local.CollectionDate.Date = import.Pers.CollectionDt;

      // ***  TYPE OF CHANGE (P = Program has changed)
      // ***  TYPE OF CHANGE (R = Case Role has changed)
      // ***  TYPE OF CHANGE (D = Debts have changed)
      // ***  TYPE OF CHANGE (C = Collection has changed)
      if (AsChar(import.TypeOfChange.SelectChar) == 'R')
      {
        local.Retry.Count = 0;

        while(local.Retry.Count < 99)
        {
          try
          {
            CreateCollection();

            if (ReadCollectionAdjustmentReason())
            {
              try
              {
                UpdateCollection();

                if (AsChar(import.ReportNeeded.Flag) == 'Y')
                {
                  local.EabConvertNumeric.SendAmount =
                    NumberToString((long)(entities.Collection.Amount * 100), 15);
                    

                  if (entities.Collection.Amount < 0)
                  {
                    local.EabConvertNumeric.SendSign = "-";
                  }

                  UseEabConvertNumeric1();
                  local.Collection2.ReturnCurrencySigned =
                    local.EabConvertNumeric.ReturnCurrencySigned;
                  local.EabConvertNumeric.Assign(local.Clear);
                  local.FormatDate.Text10 =
                    NumberToString(Month(entities.Collection.CollectionDt), 14,
                    2) + "-" + NumberToString
                    (Day(entities.Collection.CollectionDt), 14, 2) + "-" + NumberToString
                    (Year(entities.Collection.CollectionDt), 12, 4);
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail = "replicated" + "  " + entities
                    .Client.Number + "   " + local.FormatDate.Text10 + "    " +
                    NumberToString
                    (entities.CashReceiptDetail.SequentialIdentifier, 13, 3) + "  " +
                    "            " + " " + " " + Substring
                    (local.Collection2.ReturnCurrencySigned,
                    EabConvertNumeric2.ReturnCurrencySigned_MaxLength, 10, 12) +
                    " " + " " + " " + " " + " ";
                  UseCabBusinessReport01();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  local.EabConvertNumeric.Assign(local.Clear);
                }

                // ****************************************************************************************
                // Transfer any URA amounts for supported presons to the new
                // collection.
                // **************************************************************************************
                if (AsChar(import.Pers.ConcurrentInd) != 'Y')
                {
                  if (Equal(import.Pers.ProgramAppliedTo, "FC") || Equal
                    (import.Pers.ProgramAppliedTo, "AF"))
                  {
                    UseFnTransferUraCollAppl();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      return;
                    }
                  }
                }
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_COLLECTION_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_COLLECTION_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            else
            {
              ExitState = "FN0000_COLL_ADJUST_REASON_NF";
            }

            return;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ++local.Retry.Count;

                continue;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
      else
      {
        ExitState = "ACO_NE0000_INVALID_ACTION";
      }
    }
    else
    {
      ExitState = "FN0000_CASH_RCPT_NUMBER_NF";
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.DisbursementDt = source.DisbursementDt;
    target.AdjustedInd = source.AdjustedInd;
    target.ConcurrentInd = source.ConcurrentInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjProcessDate = source.CollectionAdjProcessDate;
    target.DisbursementAdjProcessDate = source.DisbursementAdjProcessDate;
    target.DisbursementProcessingNeedInd = source.DisbursementProcessingNeedInd;
    target.DistributionMethod = source.DistributionMethod;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.AppliedToOrderTypeCode = source.AppliedToOrderTypeCode;
    target.ManualDistributionReasonText = source.ManualDistributionReasonText;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
    target.CourtNoticeReqInd = source.CourtNoticeReqInd;
    target.CourtNoticeProcessedDate = source.CourtNoticeProcessedDate;
    target.AeNotifiedDate = source.AeNotifiedDate;
    target.Ocse34ReportingPeriod = source.Ocse34ReportingPeriod;
    target.BalForIntCompBefColl = source.BalForIntCompBefColl;
    target.CumIntChargedUptoColl = source.CumIntChargedUptoColl;
    target.CumIntCollAfterThisColl = source.CumIntCollAfterThisColl;
    target.IntBalAftThisColl = source.IntBalAftThisColl;
    target.DisburseToArInd = source.DisburseToArInd;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
    target.AppliedToFuture = source.AppliedToFuture;
    target.CsenetOutboundReqInd = source.CsenetOutboundReqInd;
    target.CsenetOutboundProcDt = source.CsenetOutboundProcDt;
    target.CsenetOutboundAdjProjDt = source.CsenetOutboundAdjProjDt;
    target.CourtNoticeAdjProcessDate = source.CourtNoticeAdjProcessDate;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnCurrencySigned = source.ReturnCurrencySigned;
    target.ReturnCurrencyNegInParens = source.ReturnCurrencyNegInParens;
    target.ReturnAmountNonDecimalSigned = source.ReturnAmountNonDecimalSigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
    target.ReturnOkFlag = source.ReturnOkFlag;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    useImport.EabConvertNumeric.Assign(local.EabConvertNumeric);
    useExport.EabConvertNumeric.Assign(local.EabConvertNumeric);

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric2(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseFnTransferUraCollAppl()
  {
    var useImport = new FnTransferUraCollAppl.Import();
    var useExport = new FnTransferUraCollAppl.Export();

    useImport.PersistentOld.Assign(import.Pers);
    useImport.PersistentNew.Assign(entities.Collection);

    Call(FnTransferUraCollAppl.Execute, useImport, useExport);

    import.Pers.Assign(useImport.PersistentOld);
    MoveCollection(useImport.PersistentNew, entities.Collection);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var appliedToCode = import.Pers.AppliedToCode;
    var collectionDt = import.Pers.CollectionDt;
    var disbursementDt = local.Null1.Date;
    var adjustedInd = "N";
    var concurrentInd = import.Pers.ConcurrentInd;
    var crtType = entities.CashReceiptDetail.CrtIdentifier;
    var cstId = entities.CashReceiptDetail.CstIdentifier;
    var crvId = entities.CashReceiptDetail.CrvIdentifier;
    var crdId = entities.CashReceiptDetail.SequentialIdentifier;
    var obgId = entities.Debt.ObgGeneratedId;
    var cspNumber = entities.Debt.CspNumber;
    var cpaType = entities.Debt.CpaType;
    var otrId = entities.Debt.SystemGeneratedIdentifier;
    var otrType = entities.Debt.Type1;
    var otyId = entities.Debt.OtyType;
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTmst = Now();
    var amount = import.Pers.Amount;
    var disbursementProcessingNeedInd =
      import.Pers.DisbursementProcessingNeedInd;
    var distributionMethod = import.Pers.DistributionMethod;
    var programAppliedTo = import.Pers.ProgramAppliedTo;
    var appliedToOrderTypeCode = import.Pers.AppliedToOrderTypeCode;
    var courtNoticeReqInd = import.Pers.CourtNoticeReqInd;
    var aeNotifiedDate = import.Pers.AeNotifiedDate;
    var ocse34ReportingPeriod = import.Pers.Ocse34ReportingPeriod;
    var balForIntCompBefColl = import.Pers.BalForIntCompBefColl;
    var cumIntChargedUptoColl = import.Pers.CumIntChargedUptoColl;
    var cumIntCollAfterThisColl = import.Pers.CumIntCollAfterThisColl;
    var intBalAftThisColl = import.Pers.IntBalAftThisColl;
    var disburseToArInd = import.Pers.DisburseToArInd;
    var manualDistributionReasonText = import.Pers.ManualDistributionReasonText;
    var collectionAdjustmentReasonTxt =
      import.Pers.CollectionAdjustmentReasonTxt;
    var courtOrderAppliedTo = import.Pers.CourtOrderAppliedTo;
    var appliedToFuture = import.Pers.AppliedToFuture;
    var csenetOutboundReqInd = import.Pers.CsenetOutboundReqInd;
    var distPgmStateAppldTo = import.Pers.DistPgmStateAppldTo;

    CheckValid<Collection>("AppliedToCode", appliedToCode);
    CheckValid<Collection>("AdjustedInd", adjustedInd);
    CheckValid<Collection>("ConcurrentInd", concurrentInd);
    CheckValid<Collection>("CpaType", cpaType);
    CheckValid<Collection>("OtrType", otrType);
    CheckValid<Collection>("DisbursementProcessingNeedInd",
      disbursementProcessingNeedInd);
    CheckValid<Collection>("DistributionMethod", distributionMethod);
    CheckValid<Collection>("ProgramAppliedTo", programAppliedTo);
    CheckValid<Collection>("AppliedToOrderTypeCode", appliedToOrderTypeCode);
    CheckValid<Collection>("AppliedToFuture", appliedToFuture);
    CheckValid<Collection>("CsenetOutboundReqInd", csenetOutboundReqInd);
    entities.Collection.Populated = false;
    Update("CreateCollection",
      (db, command) =>
      {
        db.SetInt32(command, "collId", systemGeneratedIdentifier);
        db.SetString(command, "appliedToCd", appliedToCode);
        db.SetDate(command, "collDt", collectionDt);
        db.SetNullableDate(command, "disbDt", disbursementDt);
        db.SetNullableString(command, "adjInd", adjustedInd);
        db.SetString(command, "concurrentInd", concurrentInd);
        db.SetDate(command, "disbAdjProcDate", disbursementDt);
        db.SetInt32(command, "crtType", crtType);
        db.SetInt32(command, "cstId", cstId);
        db.SetInt32(command, "crvId", crvId);
        db.SetInt32(command, "crdId", crdId);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "otrId", otrId);
        db.SetString(command, "otrType", otrType);
        db.SetNullableDate(command, "prevCollAdjDt", default(DateTime));
        db.SetInt32(command, "otyId", otyId);
        db.SetDate(command, "collAdjDt", disbursementDt);
        db.SetDate(command, "collAdjProcDate", disbursementDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTmst);
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetNullableString(
          command, "disbProcNeedInd", disbursementProcessingNeedInd);
        db.SetString(command, "distMtd", distributionMethod);
        db.SetString(command, "pgmAppldTo", programAppliedTo);
        db.SetString(command, "applToOrdTypCd", appliedToOrderTypeCode);
        db.SetNullableString(command, "crtNoticeReqInd", courtNoticeReqInd);
        db.SetNullableDate(command, "crtNoticeProcDt", disbursementDt);
        db.SetNullableDate(command, "aeNotifiedDt", aeNotifiedDate);
        db.SetNullableDate(command, "ocse34RptPeriod", ocse34ReportingPeriod);
        db.SetNullableDecimal(command, "balBefColl", balForIntCompBefColl);
        db.SetNullableDecimal(command, "cumIntChrgd", cumIntChargedUptoColl);
        db.
          SetNullableDecimal(command, "cumIntCollAft", cumIntCollAfterThisColl);
          
        db.SetNullableDecimal(command, "intBalAftColl", intBalAftThisColl);
        db.SetNullableString(command, "disbToArInd", disburseToArInd);
        db.SetNullableString(
          command, "mnlDistRsnTxt", manualDistributionReasonText);
        db.SetNullableString(
          command, "colAdjRsnTxt", collectionAdjustmentReasonTxt);
        db.SetNullableString(command, "ctOrdAppliedTo", courtOrderAppliedTo);
        db.SetString(command, "appliedToFutInd", appliedToFuture);
        db.SetString(command, "csenetObReqInd", csenetOutboundReqInd);
        db.SetNullableDate(command, "csenetObPDt", disbursementDt);
        db.SetNullableDate(command, "csenetObAdjPDt", disbursementDt);
        db.SetDate(command, "crtNtcAdjPrcDt", disbursementDt);
        db.SetNullableString(command, "pgmStAppldTo", distPgmStateAppldTo);
        db.SetNullableString(command, "arNumber", "");
      });

    entities.Collection.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Collection.AppliedToCode = appliedToCode;
    entities.Collection.CollectionDt = collectionDt;
    entities.Collection.DisbursementDt = disbursementDt;
    entities.Collection.AdjustedInd = adjustedInd;
    entities.Collection.ConcurrentInd = concurrentInd;
    entities.Collection.DisbursementAdjProcessDate = disbursementDt;
    entities.Collection.CrtType = crtType;
    entities.Collection.CstId = cstId;
    entities.Collection.CrvId = crvId;
    entities.Collection.CrdId = crdId;
    entities.Collection.ObgId = obgId;
    entities.Collection.CspNumber = cspNumber;
    entities.Collection.CpaType = cpaType;
    entities.Collection.OtrId = otrId;
    entities.Collection.OtrType = otrType;
    entities.Collection.OtyId = otyId;
    entities.Collection.CollectionAdjustmentDt = disbursementDt;
    entities.Collection.CollectionAdjProcessDate = disbursementDt;
    entities.Collection.CreatedBy = createdBy;
    entities.Collection.CreatedTmst = createdTmst;
    entities.Collection.LastUpdatedBy = createdBy;
    entities.Collection.LastUpdatedTmst = createdTmst;
    entities.Collection.Amount = amount;
    entities.Collection.DisbursementProcessingNeedInd =
      disbursementProcessingNeedInd;
    entities.Collection.DistributionMethod = distributionMethod;
    entities.Collection.ProgramAppliedTo = programAppliedTo;
    entities.Collection.AppliedToOrderTypeCode = appliedToOrderTypeCode;
    entities.Collection.CourtNoticeReqInd = courtNoticeReqInd;
    entities.Collection.CourtNoticeProcessedDate = disbursementDt;
    entities.Collection.AeNotifiedDate = aeNotifiedDate;
    entities.Collection.Ocse34ReportingPeriod = ocse34ReportingPeriod;
    entities.Collection.BalForIntCompBefColl = balForIntCompBefColl;
    entities.Collection.CumIntChargedUptoColl = cumIntChargedUptoColl;
    entities.Collection.CumIntCollAfterThisColl = cumIntCollAfterThisColl;
    entities.Collection.IntBalAftThisColl = intBalAftThisColl;
    entities.Collection.DisburseToArInd = disburseToArInd;
    entities.Collection.ManualDistributionReasonText =
      manualDistributionReasonText;
    entities.Collection.CollectionAdjustmentReasonTxt =
      collectionAdjustmentReasonTxt;
    entities.Collection.CourtOrderAppliedTo = courtOrderAppliedTo;
    entities.Collection.AppliedToFuture = appliedToFuture;
    entities.Collection.CsenetOutboundReqInd = csenetOutboundReqInd;
    entities.Collection.CsenetOutboundProcDt = disbursementDt;
    entities.Collection.CsenetOutboundAdjProjDt = disbursementDt;
    entities.Collection.CourtNoticeAdjProcessDate = disbursementDt;
    entities.Collection.DistPgmStateAppldTo = distPgmStateAppldTo;
    entities.Collection.Populated = true;
  }

  private bool ReadCashReceiptDetailDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(import.Pers.Populated);
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailDebtDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crvId", import.Pers.CrvId);
        db.SetInt32(command, "cstId", import.Pers.CstId);
        db.SetInt32(command, "crtType", import.Pers.CrtType);
        db.SetInt32(command, "crdId", import.Pers.CrdId);
        db.SetInt32(command, "otyType", import.Pers.OtyId);
        db.SetString(command, "obTrnTyp", import.Pers.OtrType);
        db.SetInt32(command, "obTrnId", import.Pers.OtrId);
        db.SetString(command, "cpaType", import.Pers.CpaType);
        db.SetString(command, "cspNumber", import.Pers.CspNumber);
        db.SetInt32(command, "obgGeneratedId", import.Pers.ObgId);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 4);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 4);
        entities.Debt.CspNumber = db.GetString(reader, 5);
        entities.DebtDetail.CspNumber = db.GetString(reader, 5);
        entities.Debt.CpaType = db.GetString(reader, 6);
        entities.DebtDetail.CpaType = db.GetString(reader, 6);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 7);
        entities.Debt.Type1 = db.GetString(reader, 8);
        entities.DebtDetail.OtrType = db.GetString(reader, 8);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 9);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 10);
        entities.Debt.OtyType = db.GetInt32(reader, 11);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 11);
        entities.DebtDetail.DueDt = db.GetDate(reader, 12);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 13);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 14);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 15);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 1);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadClient()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Client.Populated = false;

    return Read("ReadClient",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Client.Number = db.GetString(reader, 0);
        entities.Client.Populated = true;
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          import.CollectionAdjustmentReason.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "dtyGeneratedId", entities.Debt.OtyType);
        db.SetInt32(command, "obId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.ObligationType.Classification = db.GetString(reader, 6);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 7);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(import.Pers.Populated);

    var adjustedInd = "Y";
    var carId = entities.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    var collectionAdjustmentDt = import.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTmst = Now();
    var collectionAdjustmentReasonTxt =
      import.Collection.CollectionAdjustmentReasonTxt ?? "";

    CheckValid<Collection>("AdjustedInd", adjustedInd);
    import.Pers.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "adjInd", adjustedInd);
        db.SetNullableInt32(command, "carId", carId);
        db.SetDate(command, "collAdjDt", collectionAdjustmentDt);
        db.SetDate(command, "collAdjProcDate", collectionAdjustmentDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(
          command, "colAdjRsnTxt", collectionAdjustmentReasonTxt);
        db.SetInt32(command, "collId", import.Pers.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", import.Pers.CrtType);
        db.SetInt32(command, "cstId", import.Pers.CstId);
        db.SetInt32(command, "crvId", import.Pers.CrvId);
        db.SetInt32(command, "crdId", import.Pers.CrdId);
        db.SetInt32(command, "obgId", import.Pers.ObgId);
        db.SetString(command, "cspNumber", import.Pers.CspNumber);
        db.SetString(command, "cpaType", import.Pers.CpaType);
        db.SetInt32(command, "otrId", import.Pers.OtrId);
        db.SetString(command, "otrType", import.Pers.OtrType);
        db.SetInt32(command, "otyId", import.Pers.OtyId);
      });

    import.Pers.AdjustedInd = adjustedInd;
    import.Pers.CarId = carId;
    import.Pers.CollectionAdjustmentDt = collectionAdjustmentDt;
    import.Pers.CollectionAdjProcessDate = collectionAdjustmentDt;
    import.Pers.LastUpdatedBy = lastUpdatedBy;
    import.Pers.LastUpdatedTmst = lastUpdatedTmst;
    import.Pers.CollectionAdjustmentReasonTxt = collectionAdjustmentReasonTxt;
    import.Pers.Populated = true;
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
    /// A value of TypeOfChange.
    /// </summary>
    [JsonPropertyName("typeOfChange")]
    public Common TypeOfChange
    {
      get => typeOfChange ??= new();
      set => typeOfChange = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Pers.
    /// </summary>
    [JsonPropertyName("pers")]
    public Collection Pers
    {
      get => pers ??= new();
      set => pers = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    private Common typeOfChange;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Collection collection;
    private ProgramProcessingInfo programProcessingInfo;
    private Collection pers;
    private Common reportNeeded;
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
    /// A value of CollectionDate.
    /// </summary>
    [JsonPropertyName("collectionDate")]
    public DateWorkArea CollectionDate
    {
      get => collectionDate ??= new();
      set => collectionDate = value;
    }

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
    /// A value of Retry.
    /// </summary>
    [JsonPropertyName("retry")]
    public Common Retry
    {
      get => retry ??= new();
      set => retry = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Collection1.
    /// </summary>
    [JsonPropertyName("collection1")]
    public Collection Collection1
    {
      get => collection1 ??= new();
      set => collection1 = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of Collection2.
    /// </summary>
    [JsonPropertyName("collection2")]
    public EabConvertNumeric2 Collection2
    {
      get => collection2 ??= new();
      set => collection2 = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public EabConvertNumeric2 Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of FormatDate.
    /// </summary>
    [JsonPropertyName("formatDate")]
    public TextWorkArea FormatDate
    {
      get => formatDate ??= new();
      set => formatDate = value;
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

    private DateWorkArea collectionDate;
    private DateWorkArea null1;
    private Common retry;
    private Program program;
    private Collection collection1;
    private EabConvertNumeric2 eabConvertNumeric;
    private EabConvertNumeric2 collection2;
    private EabConvertNumeric2 clear;
    private TextWorkArea formatDate;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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

    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount supported;
    private CsePerson client;
    private DebtDetail debtDetail;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private ObligationTransaction debt;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
  }
#endregion
}
