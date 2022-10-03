// Program: FN_DISB_DISBURSE_COLLECTION, ID: 373462724, model: 746.
// Short name: SWEDISBP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DISB_DISBURSE_COLLECTION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDisbDisburseCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISB_DISBURSE_COLLECTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisbDisburseCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisbDisburseCollection.
  /// </summary>
  public FnDisbDisburseCollection(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------------
    // 11/06/02  Fangman  WR 020323
    //      New screen to process collections that have been erroring out of 650
    // due to not being able to determine the AR.  This screen will allow a
    // worker to enter an AR for a collection or group of collections for an AP.
    // The collections will then be disbursed to the AR entered (a
    // disbursement collection will be created) and then 651 will process the
    // disbursement collections in the next run.
    // -----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Search.SystemGeneratedIdentifier =
      import.Search.SystemGeneratedIdentifier;
    export.Hidden.SystemGeneratedIdentifier =
      import.Hidden.SystemGeneratedIdentifier;
    export.ShowAll.Flag = import.ShowAll.Flag;

    if (AsChar(export.ShowAll.Flag) != 'Y')
    {
      export.ShowAll.Flag = "N";
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.Hidden.SystemGeneratedIdentifier =
        import.Search.SystemGeneratedIdentifier;
      export.Collection.Assign(import.Collection);
      export.ApCsePerson.Number = import.ApCsePerson.Number;
      export.ApWorkArea.Text33 = import.ApWorkArea.Text33;
      export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
      export.CashReceiptDetail.SequentialIdentifier =
        import.CashReceiptDetail.SequentialIdentifier;
      export.SupportedCsePerson.Number = import.SupportedCsePerson.Number;
      export.SupportedWorkArea.Text33 = import.SupportedWorkArea.Text33;
      export.Case1.Number = import.Case1.Number;
      export.DebtDetail.DueDt = import.DebtDetail.DueDt;

      export.Grp.Index = 0;
      export.Grp.Clear();

      for(import.Grp.Index = 0; import.Grp.Index < import.Grp.Count; ++
        import.Grp.Index)
      {
        if (export.Grp.IsFull)
        {
          break;
        }

        export.Grp.Update.DtlSel.SelectChar = import.Grp.Item.DtlSel.SelectChar;
        export.Grp.Update.DtlCase.Number = import.Grp.Item.DtlCase.Number;
        export.Grp.Update.DtlCaseRoleCsePerson.Number =
          import.Grp.Item.DtlCaseRoleCsePerson.Number;
        export.Grp.Update.DtlCaseRoleWorkArea.Text33 =
          import.Grp.Item.DtlCaseRoleWorkArea.Text33;
        export.Grp.Update.Dtl.Assign(import.Grp.Item.Dtl);
        export.Grp.Next();
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "DISBURSE") || Equal(global.Command, "PROCESS") || Equal
      (global.Command, "NEXTCOLL"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "DISBURSE") || Equal(global.Command, "PROCESS"))
    {
      for(export.Grp.Index = 0; export.Grp.Index < export.Grp.Count; ++
        export.Grp.Index)
      {
        if (AsChar(export.Grp.Item.DtlSel.SelectChar) == 'S')
        {
          ++local.NbrOfArsSelected.Count;

          if (local.NbrOfArsSelected.Count > 1)
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            return;
          }

          if (!Equal(export.Grp.Item.Dtl.Type1, "AR"))
          {
            var field = GetField(export.Grp.Item.DtlSel, "selectChar");

            field.Error = true;

            ExitState = "FN0000_MUST_SELECT_AN_AR";

            return;
          }

          local.SelectedAr.Number = export.Grp.Item.DtlCaseRoleCsePerson.Number;
        }
        else if (IsEmpty(export.Grp.Item.DtlSel.SelectChar))
        {
          // Continue
        }
        else
        {
          var field = GetField(export.Grp.Item.DtlSel, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
        }
      }

      if (local.NbrOfArsSelected.Count == 0)
      {
        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

        return;
      }
    }
    else if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "NEXTCOLL"))
    {
      if (import.Search.SystemGeneratedIdentifier <= 0)
      {
        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

        var field = GetField(export.Search, "systemGeneratedIdentifier");

        field.Error = true;

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        UseFnDisplayCollection();

        break;
      case "PROCESS":
        if (ReadCollectionDebtObligationCashReceiptDetailCashReceipt1())
        {
          local.ProgramProcessingInfo.ProcessDate = Now().Date;

          if (AsChar(entities.ReadForUpdate.AdjustedInd) == 'Y')
          {
            local.ForUpdate.DisbursementDt =
              entities.ReadForUpdate.DisbursementDt;
            local.ForUpdate.DisbursementAdjProcessDate = Now().Date;
          }
          else
          {
            local.ForUpdate.DisbursementDt = Now().Date;
            local.ForUpdate.DisbursementAdjProcessDate = local.Initialized.Date;
          }

          UseFnProcessADistCollOnline();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
          }
        }
        else
        {
          ExitState = "FN0000_COLLECTION_NF";
        }

        break;
      case "DISBURSE":
        if (ReadCsePersonAccountCsePersonAccountCsePerson())
        {
          // Continue
        }
        else
        {
          ExitState = "FN0000_NO_RECORDS_FOUND";

          return;
        }

        foreach(var item in ReadCollectionDebtObligationCashReceiptDetailCashReceipt2())
          
        {
          // Skip Not Fully Applied errors
          if (AsChar(entities.CashReceiptDetail.CollectionAmtFullyAppliedInd) ==
            'Y' || AsChar(entities.CashReceiptType.CategoryIndicator) == 'N')
          {
          }
          else if (AsChar(entities.ReadForUpdate.AdjustedInd) == 'Y')
          {
            if (Lt(local.Initialized.Date, entities.ReadForUpdate.DisbursementDt))
              
            {
              continue;
            }
          }
          else
          {
            foreach(var item1 in ReadCollection())
            {
              goto ReadEach1;
            }
          }

          local.ProgramProcessingInfo.ProcessDate = Now().Date;

          if (AsChar(entities.ReadForUpdate.AdjustedInd) == 'Y')
          {
            local.ForUpdate.DisbursementDt =
              entities.ReadForUpdate.DisbursementDt;
            local.ForUpdate.DisbursementAdjProcessDate = Now().Date;
          }
          else
          {
            local.ForUpdate.DisbursementDt = Now().Date;
            local.ForUpdate.DisbursementAdjProcessDate = local.Initialized.Date;
          }

          UseFnProcessADistCollOnline();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          ++local.NbrOfRecsProcessed.Count;

ReadEach1:
          ;
        }

        if (local.NbrOfRecsProcessed.Count > 0)
        {
          ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
        }
        else
        {
          ExitState = "FN0000_NO_RECORDS_FOUND";
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "NEXTCOLL":
        if (IsEmpty(export.ApCsePerson.Number))
        {
          ExitState = "FN0000_DISPLAY_BEFORE_NEXT_COLL";

          return;
        }

        local.ApRangeNumeric.Number10 =
          StringToNumber(export.ApCsePerson.Number) + 10000;
        local.ApRangeTxt.Number =
          NumberToString(local.ApRangeNumeric.Number10, 6, 10);
        local.NextCollectionFoundInd.Flag = "N";

        foreach(var item in ReadCollectionCsePersonCashReceiptCashReceiptDetail())
          
        {
          if (Equal(entities.ApCsePerson.Number, export.ApCsePerson.Number))
          {
            if (entities.CashReceipt.SequentialNumber < export
              .CashReceipt.SequentialNumber)
            {
              continue;
            }
            else if (entities.CashReceipt.SequentialNumber == export
              .CashReceipt.SequentialNumber)
            {
              if (entities.ReadForNextCashReceiptDetail.SequentialIdentifier < export
                .CashReceiptDetail.SequentialIdentifier)
              {
                continue;
              }
              else if (entities.ReadForNextCashReceiptDetail.
                SequentialIdentifier == export
                .CashReceiptDetail.SequentialIdentifier)
              {
                if (entities.ReadForNextCollection.SystemGeneratedIdentifier <=
                  export.Search.SystemGeneratedIdentifier)
                {
                  continue;
                }
              }
            }
          }

          // Skip Not Fully Applied errors
          if (AsChar(entities.ReadForNextCashReceiptDetail.
            CollectionAmtFullyAppliedInd) == 'Y' || AsChar
            (entities.CashReceiptType.CategoryIndicator) == 'N')
          {
            local.NextCollectionFoundInd.Flag = "Y";
          }
          else if (AsChar(entities.ReadForNextCollection.AdjustedInd) == 'Y')
          {
            if (Lt(local.Initialized.Date,
              entities.ReadForNextCollection.DisbursementDt))
            {
              continue;
            }

            local.NextCollectionFoundInd.Flag = "Y";
          }
          else
          {
            foreach(var item1 in ReadCollection())
            {
              goto ReadEach2;
            }

            local.NextCollectionFoundInd.Flag = "Y";
          }

          break;

ReadEach2:
          ;
        }

        if (AsChar(local.NextCollectionFoundInd.Flag) == 'N')
        {
          ExitState = "FN0000_NEXT_COLL_NOT_FOUND";

          return;
        }

        export.Search.SystemGeneratedIdentifier =
          entities.ReadForNextCollection.SystemGeneratedIdentifier;
        UseFnDisplayCollection();

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCollection1(Collection source, Collection target)
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
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.AppliedToOrderTypeCode = source.AppliedToOrderTypeCode;
    target.AppliedToFuture = source.AppliedToFuture;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
    target.ArNumber = source.ArNumber;
  }

  private static void MoveCollection2(Collection source, Collection target)
  {
    target.DisbursementDt = source.DisbursementDt;
    target.DisbursementAdjProcessDate = source.DisbursementAdjProcessDate;
  }

  private static void MoveGrp(FnDisplayCollection.Export.GrpGroup source,
    Export.GrpGroup target)
  {
    target.DtlSel.SelectChar = source.DtlSel.SelectChar;
    target.DtlCase.Number = source.DtlCase.Number;
    target.DtlCaseRoleCsePerson.Number = source.DtlCaseRoleCsePerson.Number;
    target.DtlCaseRoleWorkArea.Text33 = source.DtlCaseRoleWorkArea.Text33;
    target.Dtl.Assign(source.Dtl);
  }

  private void UseFnDisplayCollection()
  {
    var useImport = new FnDisplayCollection.Import();
    var useExport = new FnDisplayCollection.Export();

    useImport.Search.SystemGeneratedIdentifier =
      export.Search.SystemGeneratedIdentifier;
    useImport.ShowAll.Flag = export.ShowAll.Flag;
    useImport.TraceMode.Flag = local.TraceMode.Flag;

    Call(FnDisplayCollection.Execute, useImport, useExport);

    export.Collection.Assign(useExport.Collection);
    export.CashReceipt.SequentialNumber =
      useExport.CashReceipt.SequentialNumber;
    export.CashReceiptDetail.SequentialIdentifier =
      useExport.CashReceiptDetail.SequentialIdentifier;
    export.ApCsePerson.Number = useExport.ApCsePerson.Number;
    export.ApWorkArea.Text33 = useExport.ApWorkArea.Text33;
    export.SupportedCsePerson.Number = useExport.SupportedCsePerson.Number;
    export.SupportedWorkArea.Text33 = useExport.SupportedWorkArea.Text33;
    export.Case1.Number = useExport.Case1.Number;
    export.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    useExport.Grp.CopyTo(export.Grp, MoveGrp);
  }

  private void UseFnProcessADistCollOnline()
  {
    var useImport = new FnProcessADistCollOnline.Import();
    var useExport = new FnProcessADistCollOnline.Export();

    useImport.PerObligation.Assign(entities.Obligation);
    useImport.PerCollection.Assign(entities.ReadForUpdate);
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    MoveCollection2(local.ForUpdate, useImport.CollectionUpdateValues);
    useImport.PerDebt.Assign(entities.Debt);
    useImport.Obligee.Number = local.SelectedAr.Number;
    useImport.Supported.Number = entities.SupportedCsePerson.Number;
    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;

    Call(FnProcessADistCollOnline.Execute, useImport, useExport);

    entities.Obligation.SystemGeneratedIdentifier =
      useImport.PerObligation.SystemGeneratedIdentifier;
    MoveCollection1(useImport.PerCollection, entities.ReadForUpdate);
    entities.Debt.SystemGeneratedIdentifier =
      useImport.PerDebt.SystemGeneratedIdentifier;
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(
      entities.ReadForNextCashReceiptDetail.Populated);
    entities.ReadForAdjustments.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.ReadForNextCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvId",
          entities.ReadForNextCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstId",
          entities.ReadForNextCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtType",
          entities.ReadForNextCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ReadForAdjustments.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReadForAdjustments.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ReadForAdjustments.CrtType = db.GetInt32(reader, 2);
        entities.ReadForAdjustments.CstId = db.GetInt32(reader, 3);
        entities.ReadForAdjustments.CrvId = db.GetInt32(reader, 4);
        entities.ReadForAdjustments.CrdId = db.GetInt32(reader, 5);
        entities.ReadForAdjustments.ObgId = db.GetInt32(reader, 6);
        entities.ReadForAdjustments.CspNumber = db.GetString(reader, 7);
        entities.ReadForAdjustments.CpaType = db.GetString(reader, 8);
        entities.ReadForAdjustments.OtrId = db.GetInt32(reader, 9);
        entities.ReadForAdjustments.OtrType = db.GetString(reader, 10);
        entities.ReadForAdjustments.OtyId = db.GetInt32(reader, 11);
        entities.ReadForAdjustments.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionCsePersonCashReceiptCashReceiptDetail()
  {
    entities.CashReceiptType.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ReadForNextCollection.Populated = false;
    entities.ReadForNextCashReceiptDetail.Populated = false;

    return ReadEach("ReadCollectionCsePersonCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", export.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", local.ApRangeTxt.Number);
        db.SetDate(
          command, "disbAdjProcDate",
          local.Initialized.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReadForNextCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReadForNextCollection.DisbursementDt =
          db.GetNullableDate(reader, 1);
        entities.ReadForNextCollection.AdjustedInd =
          db.GetNullableString(reader, 2);
        entities.ReadForNextCollection.DisbursementAdjProcessDate =
          db.GetDate(reader, 3);
        entities.ReadForNextCollection.CrtType = db.GetInt32(reader, 4);
        entities.ReadForNextCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 4);
        entities.ReadForNextCollection.CstId = db.GetInt32(reader, 5);
        entities.ReadForNextCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 5);
        entities.ReadForNextCollection.CrvId = db.GetInt32(reader, 6);
        entities.ReadForNextCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 6);
        entities.ReadForNextCollection.CrdId = db.GetInt32(reader, 7);
        entities.ReadForNextCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.ReadForNextCollection.ObgId = db.GetInt32(reader, 8);
        entities.ReadForNextCollection.CspNumber = db.GetString(reader, 9);
        entities.ApCsePerson.Number = db.GetString(reader, 9);
        entities.ReadForNextCollection.CpaType = db.GetString(reader, 10);
        entities.ReadForNextCollection.OtrId = db.GetInt32(reader, 11);
        entities.ReadForNextCollection.OtrType = db.GetString(reader, 12);
        entities.ReadForNextCollection.OtyId = db.GetInt32(reader, 13);
        entities.ReadForNextCollection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 14);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 15);
        entities.ReadForNextCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 15);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 16);
        entities.ReadForNextCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 16);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 17);
        entities.ReadForNextCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 17);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 18);
        entities.ReadForNextCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 19);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 20);
        entities.CashReceiptType.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ReadForNextCollection.Populated = true;
        entities.ReadForNextCashReceiptDetail.Populated = true;

        return true;
      });
  }

  private bool ReadCollectionDebtObligationCashReceiptDetailCashReceipt1()
  {
    entities.ReadForUpdate.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.SupportedCsePerson.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadCollectionDebtObligationCashReceiptDetailCashReceipt1",
      (db, command) =>
      {
        db.SetInt32(command, "collId", import.Search.SystemGeneratedIdentifier);
        db.SetDate(
          command, "disbAdjProcDate",
          local.Initialized.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReadForUpdate.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReadForUpdate.AppliedToCode = db.GetString(reader, 1);
        entities.ReadForUpdate.CollectionDt = db.GetDate(reader, 2);
        entities.ReadForUpdate.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.ReadForUpdate.AdjustedInd = db.GetNullableString(reader, 4);
        entities.ReadForUpdate.ConcurrentInd = db.GetString(reader, 5);
        entities.ReadForUpdate.DisbursementAdjProcessDate =
          db.GetDate(reader, 6);
        entities.ReadForUpdate.CrtType = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 7);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 7);
        entities.ReadForUpdate.CstId = db.GetInt32(reader, 8);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 8);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 8);
        entities.ReadForUpdate.CrvId = db.GetInt32(reader, 9);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 9);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 9);
        entities.ReadForUpdate.CrdId = db.GetInt32(reader, 10);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 10);
        entities.ReadForUpdate.ObgId = db.GetInt32(reader, 11);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.ReadForUpdate.CspNumber = db.GetString(reader, 12);
        entities.Debt.CspNumber = db.GetString(reader, 12);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.DebtDetail.CspNumber = db.GetString(reader, 12);
        entities.DebtDetail.CspNumber = db.GetString(reader, 12);
        entities.ReadForUpdate.CpaType = db.GetString(reader, 13);
        entities.Debt.CpaType = db.GetString(reader, 13);
        entities.Obligation.CpaType = db.GetString(reader, 13);
        entities.Obligation.CpaType = db.GetString(reader, 13);
        entities.DebtDetail.CpaType = db.GetString(reader, 13);
        entities.DebtDetail.CpaType = db.GetString(reader, 13);
        entities.ReadForUpdate.OtrId = db.GetInt32(reader, 14);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 14);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 14);
        entities.ReadForUpdate.OtrType = db.GetString(reader, 15);
        entities.Debt.Type1 = db.GetString(reader, 15);
        entities.DebtDetail.OtrType = db.GetString(reader, 15);
        entities.ReadForUpdate.OtyId = db.GetInt32(reader, 16);
        entities.Debt.OtyType = db.GetInt32(reader, 16);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 16);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 16);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 16);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 16);
        entities.ReadForUpdate.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.ReadForUpdate.CollectionAdjProcessDate =
          db.GetDate(reader, 18);
        entities.ReadForUpdate.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.ReadForUpdate.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 20);
        entities.ReadForUpdate.Amount = db.GetDecimal(reader, 21);
        entities.ReadForUpdate.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 22);
        entities.ReadForUpdate.ProgramAppliedTo = db.GetString(reader, 23);
        entities.ReadForUpdate.AppliedToOrderTypeCode =
          db.GetString(reader, 24);
        entities.ReadForUpdate.AppliedToFuture = db.GetString(reader, 25);
        entities.ReadForUpdate.DistPgmStateAppldTo =
          db.GetNullableString(reader, 26);
        entities.ReadForUpdate.ArNumber = db.GetNullableString(reader, 27);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 28);
        entities.SupportedCsePerson.Number = db.GetString(reader, 28);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 30);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 31);
        entities.DebtDetail.DueDt = db.GetDate(reader, 32);
        entities.ReadForUpdate.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.SupportedCsePerson.Populated = true;
        entities.Obligation.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadCollectionDebtObligationCashReceiptDetailCashReceipt2()
  {
    System.Diagnostics.Debug.Assert(entities.ApCsePersonAccount.Populated);
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);
    entities.CashReceiptType.Populated = false;
    entities.ReadForUpdate.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadCollectionDebtObligationCashReceiptDetailCashReceipt2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.ApCsePersonAccount.Type1);
        db.
          SetString(command, "cspNumber", entities.ApCsePersonAccount.CspNumber);
          
        db.SetNullableString(
          command, "cpaSupType", entities.SupportedCsePersonAccount.Type1);
        db.SetNullableString(
          command, "cspSupNumber",
          entities.SupportedCsePersonAccount.CspNumber);
        db.SetDate(
          command, "disbAdjProcDate",
          local.Initialized.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReadForUpdate.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReadForUpdate.AppliedToCode = db.GetString(reader, 1);
        entities.ReadForUpdate.CollectionDt = db.GetDate(reader, 2);
        entities.ReadForUpdate.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.ReadForUpdate.AdjustedInd = db.GetNullableString(reader, 4);
        entities.ReadForUpdate.ConcurrentInd = db.GetString(reader, 5);
        entities.ReadForUpdate.DisbursementAdjProcessDate =
          db.GetDate(reader, 6);
        entities.ReadForUpdate.CrtType = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 7);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 7);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ReadForUpdate.CstId = db.GetInt32(reader, 8);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 8);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 8);
        entities.ReadForUpdate.CrvId = db.GetInt32(reader, 9);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 9);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 9);
        entities.ReadForUpdate.CrdId = db.GetInt32(reader, 10);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 10);
        entities.ReadForUpdate.ObgId = db.GetInt32(reader, 11);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.ReadForUpdate.CspNumber = db.GetString(reader, 12);
        entities.Debt.CspNumber = db.GetString(reader, 12);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.Obligation.CspNumber = db.GetString(reader, 12);
        entities.DebtDetail.CspNumber = db.GetString(reader, 12);
        entities.DebtDetail.CspNumber = db.GetString(reader, 12);
        entities.ReadForUpdate.CpaType = db.GetString(reader, 13);
        entities.Debt.CpaType = db.GetString(reader, 13);
        entities.Obligation.CpaType = db.GetString(reader, 13);
        entities.Obligation.CpaType = db.GetString(reader, 13);
        entities.DebtDetail.CpaType = db.GetString(reader, 13);
        entities.DebtDetail.CpaType = db.GetString(reader, 13);
        entities.ReadForUpdate.OtrId = db.GetInt32(reader, 14);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 14);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 14);
        entities.ReadForUpdate.OtrType = db.GetString(reader, 15);
        entities.Debt.Type1 = db.GetString(reader, 15);
        entities.DebtDetail.OtrType = db.GetString(reader, 15);
        entities.ReadForUpdate.OtyId = db.GetInt32(reader, 16);
        entities.Debt.OtyType = db.GetInt32(reader, 16);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 16);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 16);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 16);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 16);
        entities.ReadForUpdate.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.ReadForUpdate.CollectionAdjProcessDate =
          db.GetDate(reader, 18);
        entities.ReadForUpdate.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.ReadForUpdate.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 20);
        entities.ReadForUpdate.Amount = db.GetDecimal(reader, 21);
        entities.ReadForUpdate.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 22);
        entities.ReadForUpdate.ProgramAppliedTo = db.GetString(reader, 23);
        entities.ReadForUpdate.AppliedToOrderTypeCode =
          db.GetString(reader, 24);
        entities.ReadForUpdate.AppliedToFuture = db.GetString(reader, 25);
        entities.ReadForUpdate.DistPgmStateAppldTo =
          db.GetNullableString(reader, 26);
        entities.ReadForUpdate.ArNumber = db.GetNullableString(reader, 27);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 28);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 30);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 31);
        entities.DebtDetail.DueDt = db.GetDate(reader, 32);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 33);
        entities.CashReceiptType.Populated = true;
        entities.ReadForUpdate.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.Obligation.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonAccountCsePersonAccountCsePerson()
  {
    entities.ApCsePerson.Populated = false;
    entities.ApCsePersonAccount.Populated = false;
    entities.SupportedCsePerson.Populated = false;
    entities.SupportedCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccountCsePersonAccountCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "collId", export.Search.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ApCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.SupportedCsePersonAccount.CspNumber = db.GetString(reader, 2);
        entities.SupportedCsePerson.Number = db.GetString(reader, 2);
        entities.SupportedCsePersonAccount.Type1 = db.GetString(reader, 3);
        entities.ApCsePerson.Populated = true;
        entities.ApCsePersonAccount.Populated = true;
        entities.SupportedCsePerson.Populated = true;
        entities.SupportedCsePersonAccount.Populated = true;
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
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of DtlSel.
      /// </summary>
      [JsonPropertyName("dtlSel")]
      public Common DtlSel
      {
        get => dtlSel ??= new();
        set => dtlSel = value;
      }

      /// <summary>
      /// A value of DtlCase.
      /// </summary>
      [JsonPropertyName("dtlCase")]
      public Case1 DtlCase
      {
        get => dtlCase ??= new();
        set => dtlCase = value;
      }

      /// <summary>
      /// A value of DtlCaseRoleCsePerson.
      /// </summary>
      [JsonPropertyName("dtlCaseRoleCsePerson")]
      public CsePerson DtlCaseRoleCsePerson
      {
        get => dtlCaseRoleCsePerson ??= new();
        set => dtlCaseRoleCsePerson = value;
      }

      /// <summary>
      /// A value of DtlCaseRoleWorkArea.
      /// </summary>
      [JsonPropertyName("dtlCaseRoleWorkArea")]
      public WorkArea DtlCaseRoleWorkArea
      {
        get => dtlCaseRoleWorkArea ??= new();
        set => dtlCaseRoleWorkArea = value;
      }

      /// <summary>
      /// A value of Dtl.
      /// </summary>
      [JsonPropertyName("dtl")]
      public CaseRole Dtl
      {
        get => dtl ??= new();
        set => dtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common dtlSel;
      private Case1 dtlCase;
      private CsePerson dtlCaseRoleCsePerson;
      private WorkArea dtlCaseRoleWorkArea;
      private CaseRole dtl;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Collection Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApWorkArea.
    /// </summary>
    [JsonPropertyName("apWorkArea")]
    public WorkArea ApWorkArea
    {
      get => apWorkArea ??= new();
      set => apWorkArea = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedWorkArea.
    /// </summary>
    [JsonPropertyName("supportedWorkArea")]
    public WorkArea SupportedWorkArea
    {
      get => supportedWorkArea ??= new();
      set => supportedWorkArea = value;
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
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public Collection Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Collection search;
    private Common showAll;
    private Collection collection;
    private CsePerson apCsePerson;
    private WorkArea apWorkArea;
    private CsePerson supportedCsePerson;
    private WorkArea supportedWorkArea;
    private Case1 case1;
    private DebtDetail debtDetail;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Array<GrpGroup> grp;
    private Collection hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of DtlSel.
      /// </summary>
      [JsonPropertyName("dtlSel")]
      public Common DtlSel
      {
        get => dtlSel ??= new();
        set => dtlSel = value;
      }

      /// <summary>
      /// A value of DtlCase.
      /// </summary>
      [JsonPropertyName("dtlCase")]
      public Case1 DtlCase
      {
        get => dtlCase ??= new();
        set => dtlCase = value;
      }

      /// <summary>
      /// A value of DtlCaseRoleCsePerson.
      /// </summary>
      [JsonPropertyName("dtlCaseRoleCsePerson")]
      public CsePerson DtlCaseRoleCsePerson
      {
        get => dtlCaseRoleCsePerson ??= new();
        set => dtlCaseRoleCsePerson = value;
      }

      /// <summary>
      /// A value of DtlCaseRoleWorkArea.
      /// </summary>
      [JsonPropertyName("dtlCaseRoleWorkArea")]
      public WorkArea DtlCaseRoleWorkArea
      {
        get => dtlCaseRoleWorkArea ??= new();
        set => dtlCaseRoleWorkArea = value;
      }

      /// <summary>
      /// A value of Dtl.
      /// </summary>
      [JsonPropertyName("dtl")]
      public CaseRole Dtl
      {
        get => dtl ??= new();
        set => dtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common dtlSel;
      private Case1 dtlCase;
      private CsePerson dtlCaseRoleCsePerson;
      private WorkArea dtlCaseRoleWorkArea;
      private CaseRole dtl;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Collection Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApWorkArea.
    /// </summary>
    [JsonPropertyName("apWorkArea")]
    public WorkArea ApWorkArea
    {
      get => apWorkArea ??= new();
      set => apWorkArea = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedWorkArea.
    /// </summary>
    [JsonPropertyName("supportedWorkArea")]
    public WorkArea SupportedWorkArea
    {
      get => supportedWorkArea ??= new();
      set => supportedWorkArea = value;
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
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public Collection Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    private Collection search;
    private Common showAll;
    private Collection collection;
    private CsePerson apCsePerson;
    private WorkArea apWorkArea;
    private CsePerson supportedCsePerson;
    private WorkArea supportedWorkArea;
    private Case1 case1;
    private DebtDetail debtDetail;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Array<GrpGroup> grp;
    private Collection hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TraceMode.
    /// </summary>
    [JsonPropertyName("traceMode")]
    public Common TraceMode
    {
      get => traceMode ??= new();
      set => traceMode = value;
    }

    /// <summary>
    /// A value of NextCollectionFoundInd.
    /// </summary>
    [JsonPropertyName("nextCollectionFoundInd")]
    public Common NextCollectionFoundInd
    {
      get => nextCollectionFoundInd ??= new();
      set => nextCollectionFoundInd = value;
    }

    /// <summary>
    /// A value of SelectedAr.
    /// </summary>
    [JsonPropertyName("selectedAr")]
    public CsePerson SelectedAr
    {
      get => selectedAr ??= new();
      set => selectedAr = value;
    }

    /// <summary>
    /// A value of ApRangeTxt.
    /// </summary>
    [JsonPropertyName("apRangeTxt")]
    public CsePerson ApRangeTxt
    {
      get => apRangeTxt ??= new();
      set => apRangeTxt = value;
    }

    /// <summary>
    /// A value of ApRangeNumeric.
    /// </summary>
    [JsonPropertyName("apRangeNumeric")]
    public NumericWorkSet ApRangeNumeric
    {
      get => apRangeNumeric ??= new();
      set => apRangeNumeric = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public Collection ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
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
    /// A value of NbrOfArsSelected.
    /// </summary>
    [JsonPropertyName("nbrOfArsSelected")]
    public Common NbrOfArsSelected
    {
      get => nbrOfArsSelected ??= new();
      set => nbrOfArsSelected = value;
    }

    /// <summary>
    /// A value of NbrOfRecsProcessed.
    /// </summary>
    [JsonPropertyName("nbrOfRecsProcessed")]
    public Common NbrOfRecsProcessed
    {
      get => nbrOfRecsProcessed ??= new();
      set => nbrOfRecsProcessed = value;
    }

    private Common traceMode;
    private Common nextCollectionFoundInd;
    private CsePerson selectedAr;
    private CsePerson apRangeTxt;
    private NumericWorkSet apRangeNumeric;
    private Collection forUpdate;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea initialized;
    private TextWorkArea leftPadding;
    private Common ae;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common nbrOfArsSelected;
    private Common nbrOfRecsProcessed;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ReadForAdjustments.
    /// </summary>
    [JsonPropertyName("readForAdjustments")]
    public Collection ReadForAdjustments
    {
      get => readForAdjustments ??= new();
      set => readForAdjustments = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ReadForUpdate.
    /// </summary>
    [JsonPropertyName("readForUpdate")]
    public Collection ReadForUpdate
    {
      get => readForUpdate ??= new();
      set => readForUpdate = value;
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
    /// A value of ReadForDisplay.
    /// </summary>
    [JsonPropertyName("readForDisplay")]
    public Collection ReadForDisplay
    {
      get => readForDisplay ??= new();
      set => readForDisplay = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePersonAccount.
    /// </summary>
    [JsonPropertyName("apCsePersonAccount")]
    public CsePersonAccount ApCsePersonAccount
    {
      get => apCsePersonAccount ??= new();
      set => apCsePersonAccount = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of SupportedCaseRole.
    /// </summary>
    [JsonPropertyName("supportedCaseRole")]
    public CaseRole SupportedCaseRole
    {
      get => supportedCaseRole ??= new();
      set => supportedCaseRole = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ReadForNextCollection.
    /// </summary>
    [JsonPropertyName("readForNextCollection")]
    public Collection ReadForNextCollection
    {
      get => readForNextCollection ??= new();
      set => readForNextCollection = value;
    }

    /// <summary>
    /// A value of ReadForNextCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("readForNextCashReceiptDetail")]
    public CashReceiptDetail ReadForNextCashReceiptDetail
    {
      get => readForNextCashReceiptDetail ??= new();
      set => readForNextCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ArCsePersonAccount.
    /// </summary>
    [JsonPropertyName("arCsePersonAccount")]
    public CsePersonAccount ArCsePersonAccount
    {
      get => arCsePersonAccount ??= new();
      set => arCsePersonAccount = value;
    }

    private Collection collection;
    private Collection readForAdjustments;
    private CashReceiptType cashReceiptType;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Collection readForUpdate;
    private Case1 case1;
    private Collection readForDisplay;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson apCsePerson;
    private CsePersonAccount apCsePersonAccount;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private ObligationType obligationType;
    private CsePerson supportedCsePerson;
    private CsePersonAccount supportedCsePersonAccount;
    private Obligation obligation;
    private CaseRole apCaseRole;
    private CaseRole supportedCaseRole;
    private CaseRole arCaseRole;
    private Collection readForNextCollection;
    private CashReceiptDetail readForNextCashReceiptDetail;
    private CsePerson arCsePerson;
    private CsePersonAccount arCsePersonAccount;
  }
#endregion
}
