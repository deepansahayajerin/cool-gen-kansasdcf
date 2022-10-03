// Program: FN_B615_CASH_FLOW_CAB, ID: 373027975, model: 746.
// Short name: SWE01000
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
/// A program: FN_B615_CASH_FLOW_CAB.
/// </para>
/// <para>
/// RESP: FINANCE
/// This AB will build a group view of formatted text lines and key values 
/// representing all Debts for a AP/Payor
/// and, optionally, all activity associated with each Debt (i.e. Collections, 
/// Debt Adjustments).
/// </para>
/// </summary>
[Serializable]
public partial class FnB615CashFlowCab: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B615_CASH_FLOW_CAB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB615CashFlowCab(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB615CashFlowCab.
  /// </summary>
  public FnB615CashFlowCab(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------------
    // 07/12/2012 GVandy Add support for new KSDLUI (Kansas Dept. Of Labor 
    // Unemployment Insurance, ID=325)
    //            cash receipt source type
    // 08/12/2014 GVandy Add support for new CSSI (Child Support Savings 
    // Initiative, ID=327)
    //            cash receipt source type
    // ------------------------------------------------------------------------------------
    if (Equal(import.EnvCd.Text10, "ONLINE") || Equal
      (import.EnvCd.Text10, "BATCH"))
    {
    }
    else
    {
      ExitState = "ACO_NE0000_INVALID_ACTION";

      return;
    }

    local.Oct12000.Date = new DateTime(2000, 10, 1);
    local.Max.Date = new DateTime(2099, 12, 31);
    export.Online.Index = -1;

    if (Lt(import.SearchTo.Date, local.Oct12000.Date))
    {
      local.ProcessPre10012000.Flag = "Y";
      local.SearchFromLt10012000.Date = import.SearchFrom.Date;
      local.SearchToLt10012000.Date = import.SearchTo.Date;
    }
    else if (!Lt(import.SearchFrom.Date, local.Oct12000.Date))
    {
      local.ProcessPost10012000.Flag = "Y";
      local.SearchFromGe10012000.Date = import.SearchFrom.Date;
      local.SearchToGe10012000.Date = import.SearchTo.Date;
    }
    else
    {
      local.ProcessPre10012000.Flag = "Y";
      local.ProcessPost10012000.Flag = "Y";
      local.SearchFromLt10012000.Date = import.SearchFrom.Date;
      local.SearchToLt10012000.Date = AddDays(local.Oct12000.Date, -1);
      local.SearchFromGe10012000.Date = local.Oct12000.Date;
      local.SearchToGe10012000.Date = import.SearchTo.Date;
    }

    if (AsChar(import.ProcessCashInd.Flag) != 'N')
    {
      // : COURT INTERFACE/KPC - Pre 10/01/2000
      if (AsChar(local.ProcessPre10012000.Flag) == 'Y')
      {
        foreach(var item in ReadCashReceipt2())
        {
          local.CashKpc.TotalCurrency += entities.ExistingCashReceipt.
            ReceiptAmount;

          foreach(var item1 in ReadCashReceiptBalanceAdjustment2())
          {
            local.CashKpcCrAdj.TotalCurrency += entities.
              ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
          }

          foreach(var item1 in ReadCashReceiptBalanceAdjustment1())
          {
            local.CashKpcCrAdj.TotalCurrency += -
              entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
          }

          foreach(var item1 in ReadCashReceiptDetail1())
          {
            local.CashKpcColl.TotalCurrency += entities.
              ExistingCashReceiptDetail.CollectionAmount;
            local.CashKpcDist.TotalCurrency += entities.
              ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault();
            local.CashKpcRfnd.TotalCurrency += entities.
              ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
            local.CashKpcUndist.TotalCurrency += entities.
              ExistingCashReceiptDetail.CollectionAmount - (
                entities.ExistingCashReceiptDetail.DistributedAmount.
                GetValueOrDefault() + entities
              .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
          }

          foreach(var item1 in ReadCashReceiptDetail2())
          {
            local.CashKpcCrdAdj.TotalCurrency += -
              entities.ExistingCashReceiptDetail.CollectionAmount;
          }
        }
      }

      // : COURT INTERFACE/KPC - Post 10/01/2000
      if (AsChar(local.ProcessPost10012000.Flag) == 'Y')
      {
        foreach(var item in ReadCashReceipt3())
        {
          local.CashKpc.TotalCurrency += entities.ExistingCashReceipt.
            TotalCashTransactionAmount.GetValueOrDefault();

          foreach(var item1 in ReadCashReceiptBalanceAdjustment2())
          {
            local.CashKpcCrAdj.TotalCurrency += entities.
              ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
          }

          foreach(var item1 in ReadCashReceiptBalanceAdjustment1())
          {
            local.CashKpcCrAdj.TotalCurrency += -
              entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
          }

          foreach(var item1 in ReadCashReceiptDetail1())
          {
            local.CashKpcColl.TotalCurrency += entities.
              ExistingCashReceiptDetail.CollectionAmount;
            local.CashKpcDist.TotalCurrency += entities.
              ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault();
            local.CashKpcRfnd.TotalCurrency += entities.
              ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
            local.CashKpcUndist.TotalCurrency += entities.
              ExistingCashReceiptDetail.CollectionAmount - (
                entities.ExistingCashReceiptDetail.DistributedAmount.
                GetValueOrDefault() + entities
              .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
          }

          foreach(var item1 in ReadCashReceiptDetail2())
          {
            local.CashKpcCrdAdj.TotalCurrency += -
              entities.ExistingCashReceiptDetail.CollectionAmount;
          }
        }
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "KPC-CASH";
      export.Online.Update.Online1.TotalCurrency = local.CashKpc.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "KPC-CRAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKpcCrAdj.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "KPC-COLL";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKpcColl.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "KPC-DIST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKpcDist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "KPC-RFND";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKpcRfnd.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "KPC-UNDST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKpcUndist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "KPC-CRDAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKpcCrdAdj.TotalCurrency;

      // : FDSO
      foreach(var item in ReadCashReceipt5())
      {
        local.CashFdso.TotalCurrency += entities.ExistingCashReceipt.
          TotalCashTransactionAmount.GetValueOrDefault();

        foreach(var item1 in ReadCashReceiptBalanceAdjustment2())
        {
          local.CashFdsoCrAdj.TotalCurrency += entities.
            ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptBalanceAdjustment1())
        {
          local.CashFdsoCrAdj.TotalCurrency += -
            entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptDetail1())
        {
          local.CashFdsoColl.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount;
          local.CashFdsoDist.TotalCurrency += entities.
            ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault();
          local.CashFdsoRfnd.TotalCurrency += entities.
            ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
          local.CashFdsoUndist.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - (
              entities.ExistingCashReceiptDetail.DistributedAmount.
              GetValueOrDefault() + entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        }

        foreach(var item1 in ReadCashReceiptDetail2())
        {
          local.CashFdsoCrdAdj.TotalCurrency += -
            entities.ExistingCashReceiptDetail.CollectionAmount;
        }
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "FDSO-CASH";
      export.Online.Update.Online1.TotalCurrency = local.CashFdso.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "FDSO-CRAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashFdsoCrAdj.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "FDSO-COLL";
      export.Online.Update.Online1.TotalCurrency =
        local.CashFdsoColl.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "FDSO-DIST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashFdsoDist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "FDSO-RFND";
      export.Online.Update.Online1.TotalCurrency =
        local.CashFdsoRfnd.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "FDSO-UNDST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashFdsoUndist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "FDSO-CRDAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashFdsoCrdAdj.TotalCurrency;
      local.CashFdso.TotalCurrency -= local.CashFdsoCrdAdj.TotalCurrency;

      // : SDSO
      foreach(var item in ReadCashReceipt6())
      {
        local.CashSdso.TotalCurrency += entities.ExistingCashReceipt.
          TotalCashTransactionAmount.GetValueOrDefault();

        foreach(var item1 in ReadCashReceiptBalanceAdjustment2())
        {
          local.CashSdsoCrAdj.TotalCurrency += entities.
            ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptBalanceAdjustment1())
        {
          local.CashSdsoCrAdj.TotalCurrency += -
            entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptDetail1())
        {
          local.CashSdsoColl.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount;
          local.CashSdsoDist.TotalCurrency += entities.
            ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault();
          local.CashSdsoRfnd.TotalCurrency += entities.
            ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
          local.CashSdsoUndist.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - (
              entities.ExistingCashReceiptDetail.DistributedAmount.
              GetValueOrDefault() + entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        }

        foreach(var item1 in ReadCashReceiptDetail2())
        {
          local.CashSdsoCrdAdj.TotalCurrency += -
            entities.ExistingCashReceiptDetail.CollectionAmount;
        }
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "SDSO-CASH";
      export.Online.Update.Online1.TotalCurrency = local.CashSdso.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "SDSO-CRAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashSdsoCrAdj.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "SDSO-COLL";
      export.Online.Update.Online1.TotalCurrency =
        local.CashSdsoColl.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "SDSO-DIST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashSdsoDist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "SDSO-RFND";
      export.Online.Update.Online1.TotalCurrency =
        local.CashSdsoRfnd.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "SDSO-UNDST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashSdsoUndist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "SDSO-CRDAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashSdsoCrdAdj.TotalCurrency;

      // 07/12/2012 GVandy Add support for new KSDLUI (Kansas Dept. Of Labor 
      // Unemployment Insurance, ID=325)
      //            cash receipt source type
      // : KSDLUI
      foreach(var item in ReadCashReceipt7())
      {
        local.CashKsdlui.TotalCurrency += entities.ExistingCashReceipt.
          TotalCashTransactionAmount.GetValueOrDefault();

        foreach(var item1 in ReadCashReceiptBalanceAdjustment2())
        {
          local.CashKsdluiCrAdj.TotalCurrency += entities.
            ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptBalanceAdjustment1())
        {
          local.CashKsdluiCrAdj.TotalCurrency += -
            entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptDetail1())
        {
          local.CashKsdluiColl.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount;
          local.CashKsdluiDist.TotalCurrency += entities.
            ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault();
          local.CashKsdluiRfnd.TotalCurrency += entities.
            ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
          local.CashKsdluiUndist.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - (
              entities.ExistingCashReceiptDetail.DistributedAmount.
              GetValueOrDefault() + entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        }

        foreach(var item1 in ReadCashReceiptDetail2())
        {
          local.CashKsdluiCrdAdj.TotalCurrency += -
            entities.ExistingCashReceiptDetail.CollectionAmount;
        }
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DLUI-CASH";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKsdlui.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DLUI-CRAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKsdluiCrAdj.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DLUI-COLL";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKsdluiColl.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DLUI-DIST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKsdluiDist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DLUI-RFND";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKsdluiRfnd.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DLUI-UNDST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKsdluiUndist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DLUI-CRDAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKsdluiCrdAdj.TotalCurrency;

      // 08/12/2014 GVandy Add support for new CSSI (Child Support Savings 
      // Initiative, ID=327)
      //            cash receipt source type
      // : CSSI
      foreach(var item in ReadCashReceipt8())
      {
        local.CashCssi.TotalCurrency += entities.ExistingCashReceipt.
          ReceiptAmount;

        foreach(var item1 in ReadCashReceiptBalanceAdjustment2())
        {
          local.CashCssiCrAdj.TotalCurrency += entities.
            ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptBalanceAdjustment1())
        {
          local.CashCssiCrAdj.TotalCurrency += -
            entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptDetail1())
        {
          local.CashCssiColl.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount;
          local.CashCssiDist.TotalCurrency += entities.
            ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault();
          local.CashCssiRfnd.TotalCurrency += entities.
            ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
          local.CashCssiUndist.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - (
              entities.ExistingCashReceiptDetail.DistributedAmount.
              GetValueOrDefault() + entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        }

        foreach(var item1 in ReadCashReceiptDetail2())
        {
          local.CashCssiCrdAdj.TotalCurrency += -
            entities.ExistingCashReceiptDetail.CollectionAmount;
        }
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "CSSI-CASH";
      export.Online.Update.Online1.TotalCurrency = local.CashCssi.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "CSSI-CRAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashCssiCrAdj.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "CSSI-COLL";
      export.Online.Update.Online1.TotalCurrency =
        local.CashCssiColl.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "CSSI-DIST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashCssiDist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "CSSI-RFND";
      export.Online.Update.Online1.TotalCurrency =
        local.CashCssiRfnd.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "CSSI-UNDST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashCssiUndist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "CSSI-CRDAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashCssiCrdAdj.TotalCurrency;

      // : MISC CASH
      foreach(var item in ReadCashReceipt4())
      {
        local.CashMisc.TotalCurrency += entities.ExistingCashReceipt.
          ReceiptAmount;

        foreach(var item1 in ReadCashReceiptBalanceAdjustment2())
        {
          local.CashMiscCrAdj.TotalCurrency += entities.
            ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptBalanceAdjustment1())
        {
          local.CashMiscCrAdj.TotalCurrency += -
            entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptDetail1())
        {
          local.CashMiscColl.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount;
          local.CashMiscDist.TotalCurrency += entities.
            ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault();
          local.CashMiscRfnd.TotalCurrency += entities.
            ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
          local.CashMiscUndist.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - (
              entities.ExistingCashReceiptDetail.DistributedAmount.
              GetValueOrDefault() + entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        }

        foreach(var item1 in ReadCashReceiptDetail2())
        {
          local.CashMiscCrdAdj.TotalCurrency += -
            entities.ExistingCashReceiptDetail.CollectionAmount;
        }
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "MISC-CASH";
      export.Online.Update.Online1.TotalCurrency = local.CashMisc.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "MISC-CRAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashMiscCrAdj.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "MISC-COLL";
      export.Online.Update.Online1.TotalCurrency =
        local.CashMiscColl.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "MISC-DIST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashMiscDist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "MISC-RFND";
      export.Online.Update.Online1.TotalCurrency =
        local.CashMiscRfnd.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "MISC-UNDST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashKpcUndist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "MISC-CRDAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashMiscCrdAdj.TotalCurrency;

      // : OTHER CASH
      foreach(var item in ReadCashReceipt1())
      {
        local.CashOther.TotalCurrency += entities.ExistingCashReceipt.
          TotalCashTransactionAmount.GetValueOrDefault();

        foreach(var item1 in ReadCashReceiptBalanceAdjustment2())
        {
          local.CashOtherCrAdj.TotalCurrency += entities.
            ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptBalanceAdjustment1())
        {
          local.CashOtherCrAdj.TotalCurrency = local.CashOther.TotalCurrency + -
            entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount;
        }

        foreach(var item1 in ReadCashReceiptDetail1())
        {
          local.CashOtherColl.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount;
          local.CashOtherDist.TotalCurrency += entities.
            ExistingCashReceiptDetail.DistributedAmount.GetValueOrDefault();
          local.CashOtherRfnd.TotalCurrency += entities.
            ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault();
          local.CashOtherUndist.TotalCurrency += entities.
            ExistingCashReceiptDetail.CollectionAmount - (
              entities.ExistingCashReceiptDetail.DistributedAmount.
              GetValueOrDefault() + entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        }

        foreach(var item1 in ReadCashReceiptDetail2())
        {
          local.CashOtherCrdAdj.TotalCurrency += -
            entities.ExistingCashReceiptDetail.CollectionAmount;
        }
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "OTHR-CASH";
      export.Online.Update.Online1.TotalCurrency =
        local.CashOther.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "OTHR-CRAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashOtherCrAdj.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "OTHR-COLL";
      export.Online.Update.Online1.TotalCurrency =
        local.CashOtherColl.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "OTHR-DIST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashOtherDist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "OTHR-RFND";
      export.Online.Update.Online1.TotalCurrency =
        local.CashOtherRfnd.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "OTHR-UNDST";
      export.Online.Update.Online1.TotalCurrency =
        local.CashOtherUndist.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "OTHR-CRDAD";
      export.Online.Update.Online1.TotalCurrency =
        local.CashOtherCrdAdj.TotalCurrency;
    }

    // *** DISTRIBUTION ***
    if (AsChar(import.ProcessDistInd.Flag) != 'N')
    {
      // : TAF
      foreach(var item in ReadCollection5())
      {
        if (AsChar(entities.ExistingCollection.AdjustedInd) == 'Y' && !
          Lt(import.SearchTo.Date,
          entities.ExistingCollection.CollectionAdjustmentDt))
        {
          continue;
        }

        local.DistTaf.TotalCurrency += entities.ExistingCollection.Amount;
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DIST-TAF";
      export.Online.Update.Online1.TotalCurrency = local.DistTaf.TotalCurrency;

      // : Non TAF
      foreach(var item in ReadCollection7())
      {
        if (AsChar(entities.ExistingCollection.AdjustedInd) == 'Y' && !
          Lt(import.SearchTo.Date,
          entities.ExistingCollection.CollectionAdjustmentDt))
        {
          continue;
        }

        local.DistNonTaf.TotalCurrency += entities.ExistingCollection.Amount;
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DIST-NTAF";
      export.Online.Update.Online1.TotalCurrency =
        local.DistNonTaf.TotalCurrency;

      // : FC
      foreach(var item in ReadCollection6())
      {
        if (AsChar(entities.ExistingCollection.AdjustedInd) == 'Y' && !
          Lt(import.SearchTo.Date,
          entities.ExistingCollection.CollectionAdjustmentDt))
        {
          continue;
        }

        local.DistFc.TotalCurrency += entities.ExistingCollection.Amount;
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DIST-FC";
      export.Online.Update.Online1.TotalCurrency = local.DistFc.TotalCurrency;

      // : GA/FC
      foreach(var item in ReadCollection8())
      {
        if (AsChar(entities.ExistingCollection.AdjustedInd) == 'Y' && !
          Lt(import.SearchTo.Date,
          entities.ExistingCollection.CollectionAdjustmentDt))
        {
          continue;
        }

        local.DistGaFc.TotalCurrency += entities.ExistingCollection.Amount;
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DIST-GA/FC";
      export.Online.Update.Online1.TotalCurrency = local.DistGaFc.TotalCurrency;

      // : TAF Adjustments
      foreach(var item in ReadCollection1())
      {
        local.DistTafAdj.TotalCurrency += entities.ExistingCollection.Amount;
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DADJ-TAF";
      export.Online.Update.Online1.TotalCurrency =
        local.DistTafAdj.TotalCurrency;

      // : Non-TAF Adjustments
      foreach(var item in ReadCollection3())
      {
        local.DistNonTafAdj.TotalCurrency += entities.ExistingCollection.Amount;
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DADJ-NTAF";
      export.Online.Update.Online1.TotalCurrency =
        local.DistNonTafAdj.TotalCurrency;

      // : FC Adjustments
      foreach(var item in ReadCollection2())
      {
        local.DistFcAdj.TotalCurrency += entities.ExistingCollection.Amount;
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DADJ-FC";
      export.Online.Update.Online1.TotalCurrency =
        local.DistFcAdj.TotalCurrency;

      // : GA/FC Adjustments
      foreach(var item in ReadCollection4())
      {
        local.DistGaFcAdj.TotalCurrency += entities.ExistingCollection.Amount;
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "DADJ-GA/FC";
      export.Online.Update.Online1.TotalCurrency =
        local.DistGaFcAdj.TotalCurrency;
    }

    // *** DISBURSEMENTS
    if (AsChar(import.ProcessDisbInd.Flag) != 'N')
    {
      // : Passthru warrants
      foreach(var item in ReadDisbursementTransactionPaymentRequest3())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.WarPt.TotalCurrency += entities.ExistingDisbursementTransaction.
          Amount;
      }

      // : Passthru EFT
      foreach(var item in ReadDisbursementTransactionPaymentRequest1())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.EftPt.TotalCurrency += entities.ExistingDisbursementTransaction.
          Amount;
      }

      // : Passthru Potential Recovery
      foreach(var item in ReadDisbursementTransactionPaymentRequest2())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.RcvPt.TotalCurrency += entities.ExistingDisbursementTransaction.
          Amount;
      }

      // : NA, NAI, AFI & FCI Warrants
      foreach(var item in ReadPaymentRequest8())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.WarNonTaf.TotalCurrency += entities.ExistingPaymentRequest.Amount;
      }

      local.WarNonTaf.TotalCurrency -= local.WarPt.TotalCurrency;

      // : NA, NAI, AFI & FCI EFT's
      foreach(var item in ReadPaymentRequest2())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.EftNonTaf.TotalCurrency += entities.ExistingPaymentRequest.Amount;
      }

      local.EftNonTaf.TotalCurrency -= local.EftPt.TotalCurrency;

      // : NA, NAI, AFI & FCI Recovery
      foreach(var item in ReadPaymentRequest5())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.RcvNonTaf.TotalCurrency += entities.ExistingPaymentRequest.Amount;
      }

      local.RcvNonTaf.TotalCurrency -= local.RcvPt.TotalCurrency;

      // : Refunds and Advancements
      foreach(var item in ReadPaymentRequest7())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.WarRef.TotalCurrency += entities.ExistingPaymentRequest.Amount;
      }

      foreach(var item in ReadPaymentRequest6())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.WarAdv.TotalCurrency += entities.ExistingPaymentRequest.Amount;
      }

      // : Refunds and Advancements
      foreach(var item in ReadPaymentRequest4())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.RcvRef.TotalCurrency += entities.ExistingPaymentRequest.Amount;
      }

      foreach(var item in ReadPaymentRequest3())
      {
        if (ReadPaymentRequest1())
        {
          continue;
        }
        else
        {
          // : Continue Processing.
        }

        local.RcvAdv.TotalCurrency += entities.ExistingPaymentRequest.Amount;
      }

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "WAR-PT";
      export.Online.Update.Online1.TotalCurrency = local.WarPt.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "EFT-PT";
      export.Online.Update.Online1.TotalCurrency = local.EftPt.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "RCV-PT";
      export.Online.Update.Online1.TotalCurrency = local.RcvPt.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "WAR-NA & I";
      export.Online.Update.Online1.TotalCurrency =
        local.WarNonTaf.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "EFT-NA & I";
      export.Online.Update.Online1.TotalCurrency =
        local.EftNonTaf.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "RCV-NA & I";
      export.Online.Update.Online1.TotalCurrency =
        local.RcvNonTaf.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "WAR-REF";
      export.Online.Update.Online1.TotalCurrency = local.WarRef.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "RCV-REF";
      export.Online.Update.Online1.TotalCurrency = local.RcvRef.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "WAR-ADV";
      export.Online.Update.Online1.TotalCurrency = local.WarAdv.TotalCurrency;

      ++export.Online.Index;
      export.Online.CheckSize();

      export.Online.Update.LabelOnline.Text10 = "RCV-ADV";
      export.Online.Update.Online1.TotalCurrency = local.RcvAdv.TotalCurrency;
    }

    if (Equal(import.EnvCd.Text10, "BATCH"))
    {
      UseFnB615EabFormatDetailLines();

      // : Don't delete the following.  For some reason the group view passed 
      // back from the eab thinks it is empty.  The for reinitializes the
      // subscript of the group view.
      for(export.Batch.Index = 0; export.Batch.Index < Export
        .BatchGroup.Capacity; ++export.Batch.Index)
      {
        if (!export.Batch.CheckSize())
        {
          break;
        }
      }

      export.Batch.CheckIndex();
    }
  }

  private static void MoveBatchToGroup(Export.BatchGroup source,
    FnB615EabFormatDetailLines.Export.GroupGroup target)
  {
    target.ReportData.LineText = source.Batch1.LineText;
  }

  private static void MoveGroupToBatch(FnB615EabFormatDetailLines.Export.
    GroupGroup source, Export.BatchGroup target)
  {
    target.Batch1.LineText = source.ReportData.LineText;
  }

  private void UseFnB615EabFormatDetailLines()
  {
    var useImport = new FnB615EabFormatDetailLines.Import();
    var useExport = new FnB615EabFormatDetailLines.Export();

    useImport.From.Date = import.SearchFrom.Date;
    useImport.To.Date = import.SearchTo.Date;
    useImport.ReportPeriod.Text10 = import.ReportPeriod.Text10;
    useImport.CashKpc.TotalCurrency = local.CashKpc.TotalCurrency;
    useImport.CashFdso.TotalCurrency = local.CashFdso.TotalCurrency;
    useImport.CashSdso.TotalCurrency = local.CashSdso.TotalCurrency;
    useImport.CashMisc.TotalCurrency = local.CashMisc.TotalCurrency;
    useImport.CashOther.TotalCurrency = local.CashOther.TotalCurrency;
    useImport.DistTaf.TotalCurrency = local.DistTaf.TotalCurrency;
    useImport.DistNonTaf.TotalCurrency = local.DistNonTaf.TotalCurrency;
    useImport.DistTafFc.TotalCurrency = local.DistFc.TotalCurrency;
    useImport.DistGaFc.TotalCurrency = local.DistGaFc.TotalCurrency;
    useImport.DistTafAdj.TotalCurrency = local.DistTafAdj.TotalCurrency;
    useImport.DistNonTafAdj.TotalCurrency = local.DistNonTafAdj.TotalCurrency;
    useImport.DistTafFcAdj.TotalCurrency = local.DistFcAdj.TotalCurrency;
    useImport.DistGaFcAdj.TotalCurrency = local.DistGaFcAdj.TotalCurrency;
    useImport.DisbPtWar.TotalCurrency = local.WarPt.TotalCurrency;
    useImport.DisbPtEft.TotalCurrency = local.EftPt.TotalCurrency;
    useImport.DisbNtafWar.TotalCurrency = local.WarNonTaf.TotalCurrency;
    useImport.DisbNtafEft.TotalCurrency = local.EftNonTaf.TotalCurrency;
    useImport.RefundAdvancement.TotalCurrency = local.WarAdv.TotalCurrency;
    useImport.CashKsdlui.TotalCurrency = local.CashKsdlui.TotalCurrency;
    useImport.CashCssi.TotalCurrency = local.CashCssi.TotalCurrency;
    export.Batch.CopyTo(useExport.Group, MoveBatchToGroup);

    Call(FnB615EabFormatDetailLines.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Batch, MoveGroupToBatch);
  }

  private IEnumerable<bool> ReadCashReceipt1()
  {
    entities.ExistingCashReceipt.Populated = false;

    return ReadEach("ReadCashReceipt1",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.ExistingCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 7);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingCashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceipt2()
  {
    entities.ExistingCashReceipt.Populated = false;

    return ReadEach("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetDate(
          command, "date1",
          local.SearchFromLt10012000.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.SearchToLt10012000.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.ExistingCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 7);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingCashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceipt3()
  {
    entities.ExistingCashReceipt.Populated = false;

    return ReadEach("ReadCashReceipt3",
      (db, command) =>
      {
        db.SetDate(
          command, "date1",
          local.SearchFromGe10012000.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.SearchToGe10012000.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.ExistingCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 7);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingCashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceipt4()
  {
    entities.ExistingCashReceipt.Populated = false;

    return ReadEach("ReadCashReceipt4",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.ExistingCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 7);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingCashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceipt5()
  {
    entities.ExistingCashReceipt.Populated = false;

    return ReadEach("ReadCashReceipt5",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.ExistingCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 7);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingCashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceipt6()
  {
    entities.ExistingCashReceipt.Populated = false;

    return ReadEach("ReadCashReceipt6",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.ExistingCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 7);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingCashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceipt7()
  {
    entities.ExistingCashReceipt.Populated = false;

    return ReadEach("ReadCashReceipt7",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.ExistingCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 7);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingCashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceipt8()
  {
    entities.ExistingCashReceipt.Populated = false;

    return ReadEach("ReadCashReceipt8",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.ExistingCashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 7);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingCashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptBalanceAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptBalanceAdjustment.Populated = false;

    return ReadEach("ReadCashReceiptBalanceAdjustment1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptBalanceAdjustment.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptBalanceAdjustment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptBalanceAdjustment.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptBalanceAdjustment.CrtIIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptBalanceAdjustment.CstIIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptBalanceAdjustment.CrvIIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptBalanceAdjustment.CrrIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptBalanceAdjustment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptBalanceAdjustment2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptBalanceAdjustment.Populated = false;

    return ReadEach("ReadCashReceiptBalanceAdjustment2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIIdentifier",
          entities.ExistingCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIIdentifier",
          entities.ExistingCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIIdentifier",
          entities.ExistingCashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptBalanceAdjustment.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptBalanceAdjustment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptBalanceAdjustment.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptBalanceAdjustment.CrtIIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptBalanceAdjustment.CstIIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptBalanceAdjustment.CrvIIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptBalanceAdjustment.CrrIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingCashReceiptBalanceAdjustment.AdjustmentAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptBalanceAdjustment.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 5);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 6);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 5);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 6);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection1()
  {
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 12);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 13);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 15);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 12);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 13);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 15);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection3()
  {
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection3",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 12);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 13);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 15);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection4()
  {
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection4",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 12);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 13);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 15);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection5()
  {
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection5",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 12);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 13);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 15);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection6()
  {
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection6",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 12);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 13);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 15);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection7()
  {
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection7",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 12);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 13);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 15);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection8()
  {
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection8",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 12);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 13);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 15);
        entities.ExistingCollection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.ExistingCollection.ProgramAppliedTo);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionPaymentRequest1()
  {
    entities.ExistingPaymentRequest.Populated = false;
    entities.ExistingDisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransactionPaymentRequest1",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDisbursementTransaction.CpaType =
          db.GetString(reader, 0);
        entities.ExistingDisbursementTransaction.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingDisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDisbursementTransaction.Amount =
          db.GetDecimal(reader, 3);
        entities.ExistingDisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingDisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 6);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 7);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 8);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 10);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.ExistingPaymentRequest.Populated = true;
        entities.ExistingDisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.ExistingDisbursementTransaction.CpaType);
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionPaymentRequest2()
  {
    entities.ExistingPaymentRequest.Populated = false;
    entities.ExistingDisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransactionPaymentRequest2",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDisbursementTransaction.CpaType =
          db.GetString(reader, 0);
        entities.ExistingDisbursementTransaction.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingDisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDisbursementTransaction.Amount =
          db.GetDecimal(reader, 3);
        entities.ExistingDisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingDisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 6);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 7);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 8);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 10);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.ExistingPaymentRequest.Populated = true;
        entities.ExistingDisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.ExistingDisbursementTransaction.CpaType);
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionPaymentRequest3()
  {
    entities.ExistingPaymentRequest.Populated = false;
    entities.ExistingDisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransactionPaymentRequest3",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDisbursementTransaction.CpaType =
          db.GetString(reader, 0);
        entities.ExistingDisbursementTransaction.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingDisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingDisbursementTransaction.Amount =
          db.GetDecimal(reader, 3);
        entities.ExistingDisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingDisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 6);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 7);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 8);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 10);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.ExistingPaymentRequest.Populated = true;
        entities.ExistingDisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.ExistingDisbursementTransaction.CpaType);
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

        return true;
      });
  }

  private bool ReadPaymentRequest1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingPaymentRequest.Populated);
    entities.Reissued.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.ExistingPaymentRequest.PrqRGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Reissued.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Reissued.PrqRGeneratedId = db.GetNullableInt32(reader, 1);
        entities.Reissued.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest2()
  {
    entities.ExistingPaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest2",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 3);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 5);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest3()
  {
    entities.ExistingPaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest3",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 3);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 5);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest4()
  {
    entities.ExistingPaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest4",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 3);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 5);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest5()
  {
    entities.ExistingPaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest5",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 3);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 5);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest6()
  {
    entities.ExistingPaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest6",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 3);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 5);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest7()
  {
    entities.ExistingPaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest7",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 3);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 5);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest8()
  {
    entities.ExistingPaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest8",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.SearchFrom.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.SearchTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.ExistingPaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.ExistingPaymentRequest.Classification =
          db.GetString(reader, 3);
        entities.ExistingPaymentRequest.PrintDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingPaymentRequest.Type1 = db.GetString(reader, 5);
        entities.ExistingPaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingPaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1",
          entities.ExistingPaymentRequest.Type1);

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
    /// A value of ProcessCashInd.
    /// </summary>
    [JsonPropertyName("processCashInd")]
    public Common ProcessCashInd
    {
      get => processCashInd ??= new();
      set => processCashInd = value;
    }

    /// <summary>
    /// A value of ProcessDistInd.
    /// </summary>
    [JsonPropertyName("processDistInd")]
    public Common ProcessDistInd
    {
      get => processDistInd ??= new();
      set => processDistInd = value;
    }

    /// <summary>
    /// A value of ProcessDisbInd.
    /// </summary>
    [JsonPropertyName("processDisbInd")]
    public Common ProcessDisbInd
    {
      get => processDisbInd ??= new();
      set => processDisbInd = value;
    }

    /// <summary>
    /// A value of ReportPeriod.
    /// </summary>
    [JsonPropertyName("reportPeriod")]
    public WorkArea ReportPeriod
    {
      get => reportPeriod ??= new();
      set => reportPeriod = value;
    }

    /// <summary>
    /// A value of CurrentDelMe.
    /// </summary>
    [JsonPropertyName("currentDelMe")]
    public DateWorkArea CurrentDelMe
    {
      get => currentDelMe ??= new();
      set => currentDelMe = value;
    }

    /// <summary>
    /// A value of EnvCd.
    /// </summary>
    [JsonPropertyName("envCd")]
    public WorkArea EnvCd
    {
      get => envCd ??= new();
      set => envCd = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public JobRun DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    /// <summary>
    /// A value of MaxDelMe.
    /// </summary>
    [JsonPropertyName("maxDelMe")]
    public DateWorkArea MaxDelMe
    {
      get => maxDelMe ??= new();
      set => maxDelMe = value;
    }

    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private Common processCashInd;
    private Common processDistInd;
    private Common processDisbInd;
    private WorkArea reportPeriod;
    private DateWorkArea currentDelMe;
    private WorkArea envCd;
    private JobRun delMe;
    private DateWorkArea maxDelMe;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A BatchGroup group.</summary>
    [Serializable]
    public class BatchGroup
    {
      /// <summary>
      /// A value of Batch1.
      /// </summary>
      [JsonPropertyName("batch1")]
      public ReportData Batch1
      {
        get => batch1 ??= new();
        set => batch1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ReportData batch1;
    }

    /// <summary>A OnlineGroup group.</summary>
    [Serializable]
    public class OnlineGroup
    {
      /// <summary>
      /// A value of LabelOnline.
      /// </summary>
      [JsonPropertyName("labelOnline")]
      public TextWorkArea LabelOnline
      {
        get => labelOnline ??= new();
        set => labelOnline = value;
      }

      /// <summary>
      /// A value of Online1.
      /// </summary>
      [JsonPropertyName("online1")]
      public Common Online1
      {
        get => online1 ??= new();
        set => online1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private TextWorkArea labelOnline;
      private Common online1;
    }

    /// <summary>
    /// Gets a value of Batch.
    /// </summary>
    [JsonIgnore]
    public Array<BatchGroup> Batch => batch ??= new(BatchGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Batch for json serialization.
    /// </summary>
    [JsonPropertyName("batch")]
    [Computed]
    public IList<BatchGroup> Batch_Json
    {
      get => batch;
      set => Batch.Assign(value);
    }

    /// <summary>
    /// Gets a value of Online.
    /// </summary>
    [JsonIgnore]
    public Array<OnlineGroup> Online => online ??= new(OnlineGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Online for json serialization.
    /// </summary>
    [JsonPropertyName("online")]
    [Computed]
    public IList<OnlineGroup> Online_Json
    {
      get => online;
      set => Online.Assign(value);
    }

    private Array<BatchGroup> batch;
    private Array<OnlineGroup> online;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CashCssi.
    /// </summary>
    [JsonPropertyName("cashCssi")]
    public Common CashCssi
    {
      get => cashCssi ??= new();
      set => cashCssi = value;
    }

    /// <summary>
    /// A value of CashCssiCrAdj.
    /// </summary>
    [JsonPropertyName("cashCssiCrAdj")]
    public Common CashCssiCrAdj
    {
      get => cashCssiCrAdj ??= new();
      set => cashCssiCrAdj = value;
    }

    /// <summary>
    /// A value of CashCssiColl.
    /// </summary>
    [JsonPropertyName("cashCssiColl")]
    public Common CashCssiColl
    {
      get => cashCssiColl ??= new();
      set => cashCssiColl = value;
    }

    /// <summary>
    /// A value of CashCssiDist.
    /// </summary>
    [JsonPropertyName("cashCssiDist")]
    public Common CashCssiDist
    {
      get => cashCssiDist ??= new();
      set => cashCssiDist = value;
    }

    /// <summary>
    /// A value of CashCssiRfnd.
    /// </summary>
    [JsonPropertyName("cashCssiRfnd")]
    public Common CashCssiRfnd
    {
      get => cashCssiRfnd ??= new();
      set => cashCssiRfnd = value;
    }

    /// <summary>
    /// A value of CashCssiUndist.
    /// </summary>
    [JsonPropertyName("cashCssiUndist")]
    public Common CashCssiUndist
    {
      get => cashCssiUndist ??= new();
      set => cashCssiUndist = value;
    }

    /// <summary>
    /// A value of CashCssiCrdAdj.
    /// </summary>
    [JsonPropertyName("cashCssiCrdAdj")]
    public Common CashCssiCrdAdj
    {
      get => cashCssiCrdAdj ??= new();
      set => cashCssiCrdAdj = value;
    }

    /// <summary>
    /// A value of CashKsdlui.
    /// </summary>
    [JsonPropertyName("cashKsdlui")]
    public Common CashKsdlui
    {
      get => cashKsdlui ??= new();
      set => cashKsdlui = value;
    }

    /// <summary>
    /// A value of CashKsdluiCrAdj.
    /// </summary>
    [JsonPropertyName("cashKsdluiCrAdj")]
    public Common CashKsdluiCrAdj
    {
      get => cashKsdluiCrAdj ??= new();
      set => cashKsdluiCrAdj = value;
    }

    /// <summary>
    /// A value of CashKsdluiColl.
    /// </summary>
    [JsonPropertyName("cashKsdluiColl")]
    public Common CashKsdluiColl
    {
      get => cashKsdluiColl ??= new();
      set => cashKsdluiColl = value;
    }

    /// <summary>
    /// A value of CashKsdluiDist.
    /// </summary>
    [JsonPropertyName("cashKsdluiDist")]
    public Common CashKsdluiDist
    {
      get => cashKsdluiDist ??= new();
      set => cashKsdluiDist = value;
    }

    /// <summary>
    /// A value of CashKsdluiRfnd.
    /// </summary>
    [JsonPropertyName("cashKsdluiRfnd")]
    public Common CashKsdluiRfnd
    {
      get => cashKsdluiRfnd ??= new();
      set => cashKsdluiRfnd = value;
    }

    /// <summary>
    /// A value of CashKsdluiUndist.
    /// </summary>
    [JsonPropertyName("cashKsdluiUndist")]
    public Common CashKsdluiUndist
    {
      get => cashKsdluiUndist ??= new();
      set => cashKsdluiUndist = value;
    }

    /// <summary>
    /// A value of CashKsdluiCrdAdj.
    /// </summary>
    [JsonPropertyName("cashKsdluiCrdAdj")]
    public Common CashKsdluiCrdAdj
    {
      get => cashKsdluiCrdAdj ??= new();
      set => cashKsdluiCrdAdj = value;
    }

    /// <summary>
    /// A value of Oct12000.
    /// </summary>
    [JsonPropertyName("oct12000")]
    public DateWorkArea Oct12000
    {
      get => oct12000 ??= new();
      set => oct12000 = value;
    }

    /// <summary>
    /// A value of TempNonKpc.
    /// </summary>
    [JsonPropertyName("tempNonKpc")]
    public Common TempNonKpc
    {
      get => tempNonKpc ??= new();
      set => tempNonKpc = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of MaxReports.
    /// </summary>
    [JsonPropertyName("maxReports")]
    public Common MaxReports
    {
      get => maxReports ??= new();
      set => maxReports = value;
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

    /// <summary>
    /// A value of ProcessPre10012000.
    /// </summary>
    [JsonPropertyName("processPre10012000")]
    public Common ProcessPre10012000
    {
      get => processPre10012000 ??= new();
      set => processPre10012000 = value;
    }

    /// <summary>
    /// A value of SearchFromLt10012000.
    /// </summary>
    [JsonPropertyName("searchFromLt10012000")]
    public DateWorkArea SearchFromLt10012000
    {
      get => searchFromLt10012000 ??= new();
      set => searchFromLt10012000 = value;
    }

    /// <summary>
    /// A value of SearchToLt10012000.
    /// </summary>
    [JsonPropertyName("searchToLt10012000")]
    public DateWorkArea SearchToLt10012000
    {
      get => searchToLt10012000 ??= new();
      set => searchToLt10012000 = value;
    }

    /// <summary>
    /// A value of ProcessPost10012000.
    /// </summary>
    [JsonPropertyName("processPost10012000")]
    public Common ProcessPost10012000
    {
      get => processPost10012000 ??= new();
      set => processPost10012000 = value;
    }

    /// <summary>
    /// A value of SearchFromGe10012000.
    /// </summary>
    [JsonPropertyName("searchFromGe10012000")]
    public DateWorkArea SearchFromGe10012000
    {
      get => searchFromGe10012000 ??= new();
      set => searchFromGe10012000 = value;
    }

    /// <summary>
    /// A value of SearchToGe10012000.
    /// </summary>
    [JsonPropertyName("searchToGe10012000")]
    public DateWorkArea SearchToGe10012000
    {
      get => searchToGe10012000 ??= new();
      set => searchToGe10012000 = value;
    }

    /// <summary>
    /// A value of CashKpc.
    /// </summary>
    [JsonPropertyName("cashKpc")]
    public Common CashKpc
    {
      get => cashKpc ??= new();
      set => cashKpc = value;
    }

    /// <summary>
    /// A value of CashKpcCrAdj.
    /// </summary>
    [JsonPropertyName("cashKpcCrAdj")]
    public Common CashKpcCrAdj
    {
      get => cashKpcCrAdj ??= new();
      set => cashKpcCrAdj = value;
    }

    /// <summary>
    /// A value of CashKpcColl.
    /// </summary>
    [JsonPropertyName("cashKpcColl")]
    public Common CashKpcColl
    {
      get => cashKpcColl ??= new();
      set => cashKpcColl = value;
    }

    /// <summary>
    /// A value of CashKpcDist.
    /// </summary>
    [JsonPropertyName("cashKpcDist")]
    public Common CashKpcDist
    {
      get => cashKpcDist ??= new();
      set => cashKpcDist = value;
    }

    /// <summary>
    /// A value of CashKpcRfnd.
    /// </summary>
    [JsonPropertyName("cashKpcRfnd")]
    public Common CashKpcRfnd
    {
      get => cashKpcRfnd ??= new();
      set => cashKpcRfnd = value;
    }

    /// <summary>
    /// A value of CashKpcUndist.
    /// </summary>
    [JsonPropertyName("cashKpcUndist")]
    public Common CashKpcUndist
    {
      get => cashKpcUndist ??= new();
      set => cashKpcUndist = value;
    }

    /// <summary>
    /// A value of CashKpcCrdAdj.
    /// </summary>
    [JsonPropertyName("cashKpcCrdAdj")]
    public Common CashKpcCrdAdj
    {
      get => cashKpcCrdAdj ??= new();
      set => cashKpcCrdAdj = value;
    }

    /// <summary>
    /// A value of CashFdso.
    /// </summary>
    [JsonPropertyName("cashFdso")]
    public Common CashFdso
    {
      get => cashFdso ??= new();
      set => cashFdso = value;
    }

    /// <summary>
    /// A value of CashFdsoCrAdj.
    /// </summary>
    [JsonPropertyName("cashFdsoCrAdj")]
    public Common CashFdsoCrAdj
    {
      get => cashFdsoCrAdj ??= new();
      set => cashFdsoCrAdj = value;
    }

    /// <summary>
    /// A value of CashFdsoColl.
    /// </summary>
    [JsonPropertyName("cashFdsoColl")]
    public Common CashFdsoColl
    {
      get => cashFdsoColl ??= new();
      set => cashFdsoColl = value;
    }

    /// <summary>
    /// A value of CashFdsoDist.
    /// </summary>
    [JsonPropertyName("cashFdsoDist")]
    public Common CashFdsoDist
    {
      get => cashFdsoDist ??= new();
      set => cashFdsoDist = value;
    }

    /// <summary>
    /// A value of CashFdsoRfnd.
    /// </summary>
    [JsonPropertyName("cashFdsoRfnd")]
    public Common CashFdsoRfnd
    {
      get => cashFdsoRfnd ??= new();
      set => cashFdsoRfnd = value;
    }

    /// <summary>
    /// A value of CashFdsoUndist.
    /// </summary>
    [JsonPropertyName("cashFdsoUndist")]
    public Common CashFdsoUndist
    {
      get => cashFdsoUndist ??= new();
      set => cashFdsoUndist = value;
    }

    /// <summary>
    /// A value of CashFdsoCrdAdj.
    /// </summary>
    [JsonPropertyName("cashFdsoCrdAdj")]
    public Common CashFdsoCrdAdj
    {
      get => cashFdsoCrdAdj ??= new();
      set => cashFdsoCrdAdj = value;
    }

    /// <summary>
    /// A value of CashSdso.
    /// </summary>
    [JsonPropertyName("cashSdso")]
    public Common CashSdso
    {
      get => cashSdso ??= new();
      set => cashSdso = value;
    }

    /// <summary>
    /// A value of CashSdsoCrAdj.
    /// </summary>
    [JsonPropertyName("cashSdsoCrAdj")]
    public Common CashSdsoCrAdj
    {
      get => cashSdsoCrAdj ??= new();
      set => cashSdsoCrAdj = value;
    }

    /// <summary>
    /// A value of CashSdsoColl.
    /// </summary>
    [JsonPropertyName("cashSdsoColl")]
    public Common CashSdsoColl
    {
      get => cashSdsoColl ??= new();
      set => cashSdsoColl = value;
    }

    /// <summary>
    /// A value of CashSdsoDist.
    /// </summary>
    [JsonPropertyName("cashSdsoDist")]
    public Common CashSdsoDist
    {
      get => cashSdsoDist ??= new();
      set => cashSdsoDist = value;
    }

    /// <summary>
    /// A value of CashSdsoRfnd.
    /// </summary>
    [JsonPropertyName("cashSdsoRfnd")]
    public Common CashSdsoRfnd
    {
      get => cashSdsoRfnd ??= new();
      set => cashSdsoRfnd = value;
    }

    /// <summary>
    /// A value of CashSdsoUndist.
    /// </summary>
    [JsonPropertyName("cashSdsoUndist")]
    public Common CashSdsoUndist
    {
      get => cashSdsoUndist ??= new();
      set => cashSdsoUndist = value;
    }

    /// <summary>
    /// A value of CashSdsoCrdAdj.
    /// </summary>
    [JsonPropertyName("cashSdsoCrdAdj")]
    public Common CashSdsoCrdAdj
    {
      get => cashSdsoCrdAdj ??= new();
      set => cashSdsoCrdAdj = value;
    }

    /// <summary>
    /// A value of CashMisc.
    /// </summary>
    [JsonPropertyName("cashMisc")]
    public Common CashMisc
    {
      get => cashMisc ??= new();
      set => cashMisc = value;
    }

    /// <summary>
    /// A value of CashMiscCrAdj.
    /// </summary>
    [JsonPropertyName("cashMiscCrAdj")]
    public Common CashMiscCrAdj
    {
      get => cashMiscCrAdj ??= new();
      set => cashMiscCrAdj = value;
    }

    /// <summary>
    /// A value of CashMiscColl.
    /// </summary>
    [JsonPropertyName("cashMiscColl")]
    public Common CashMiscColl
    {
      get => cashMiscColl ??= new();
      set => cashMiscColl = value;
    }

    /// <summary>
    /// A value of CashMiscDist.
    /// </summary>
    [JsonPropertyName("cashMiscDist")]
    public Common CashMiscDist
    {
      get => cashMiscDist ??= new();
      set => cashMiscDist = value;
    }

    /// <summary>
    /// A value of CashMiscRfnd.
    /// </summary>
    [JsonPropertyName("cashMiscRfnd")]
    public Common CashMiscRfnd
    {
      get => cashMiscRfnd ??= new();
      set => cashMiscRfnd = value;
    }

    /// <summary>
    /// A value of CashMiscUndist.
    /// </summary>
    [JsonPropertyName("cashMiscUndist")]
    public Common CashMiscUndist
    {
      get => cashMiscUndist ??= new();
      set => cashMiscUndist = value;
    }

    /// <summary>
    /// A value of CashMiscCrdAdj.
    /// </summary>
    [JsonPropertyName("cashMiscCrdAdj")]
    public Common CashMiscCrdAdj
    {
      get => cashMiscCrdAdj ??= new();
      set => cashMiscCrdAdj = value;
    }

    /// <summary>
    /// A value of CashOther.
    /// </summary>
    [JsonPropertyName("cashOther")]
    public Common CashOther
    {
      get => cashOther ??= new();
      set => cashOther = value;
    }

    /// <summary>
    /// A value of CashOtherCrAdj.
    /// </summary>
    [JsonPropertyName("cashOtherCrAdj")]
    public Common CashOtherCrAdj
    {
      get => cashOtherCrAdj ??= new();
      set => cashOtherCrAdj = value;
    }

    /// <summary>
    /// A value of CashOtherColl.
    /// </summary>
    [JsonPropertyName("cashOtherColl")]
    public Common CashOtherColl
    {
      get => cashOtherColl ??= new();
      set => cashOtherColl = value;
    }

    /// <summary>
    /// A value of CashOtherDist.
    /// </summary>
    [JsonPropertyName("cashOtherDist")]
    public Common CashOtherDist
    {
      get => cashOtherDist ??= new();
      set => cashOtherDist = value;
    }

    /// <summary>
    /// A value of CashOtherRfnd.
    /// </summary>
    [JsonPropertyName("cashOtherRfnd")]
    public Common CashOtherRfnd
    {
      get => cashOtherRfnd ??= new();
      set => cashOtherRfnd = value;
    }

    /// <summary>
    /// A value of CashOtherUndist.
    /// </summary>
    [JsonPropertyName("cashOtherUndist")]
    public Common CashOtherUndist
    {
      get => cashOtherUndist ??= new();
      set => cashOtherUndist = value;
    }

    /// <summary>
    /// A value of CashOtherCrdAdj.
    /// </summary>
    [JsonPropertyName("cashOtherCrdAdj")]
    public Common CashOtherCrdAdj
    {
      get => cashOtherCrdAdj ??= new();
      set => cashOtherCrdAdj = value;
    }

    /// <summary>
    /// A value of DistTaf.
    /// </summary>
    [JsonPropertyName("distTaf")]
    public Common DistTaf
    {
      get => distTaf ??= new();
      set => distTaf = value;
    }

    /// <summary>
    /// A value of DistNonTaf.
    /// </summary>
    [JsonPropertyName("distNonTaf")]
    public Common DistNonTaf
    {
      get => distNonTaf ??= new();
      set => distNonTaf = value;
    }

    /// <summary>
    /// A value of DistFc.
    /// </summary>
    [JsonPropertyName("distFc")]
    public Common DistFc
    {
      get => distFc ??= new();
      set => distFc = value;
    }

    /// <summary>
    /// A value of DistGaFc.
    /// </summary>
    [JsonPropertyName("distGaFc")]
    public Common DistGaFc
    {
      get => distGaFc ??= new();
      set => distGaFc = value;
    }

    /// <summary>
    /// A value of DistTafAdj.
    /// </summary>
    [JsonPropertyName("distTafAdj")]
    public Common DistTafAdj
    {
      get => distTafAdj ??= new();
      set => distTafAdj = value;
    }

    /// <summary>
    /// A value of DistNonTafAdj.
    /// </summary>
    [JsonPropertyName("distNonTafAdj")]
    public Common DistNonTafAdj
    {
      get => distNonTafAdj ??= new();
      set => distNonTafAdj = value;
    }

    /// <summary>
    /// A value of DistFcAdj.
    /// </summary>
    [JsonPropertyName("distFcAdj")]
    public Common DistFcAdj
    {
      get => distFcAdj ??= new();
      set => distFcAdj = value;
    }

    /// <summary>
    /// A value of DistGaFcAdj.
    /// </summary>
    [JsonPropertyName("distGaFcAdj")]
    public Common DistGaFcAdj
    {
      get => distGaFcAdj ??= new();
      set => distGaFcAdj = value;
    }

    /// <summary>
    /// A value of WarPt.
    /// </summary>
    [JsonPropertyName("warPt")]
    public Common WarPt
    {
      get => warPt ??= new();
      set => warPt = value;
    }

    /// <summary>
    /// A value of EftPt.
    /// </summary>
    [JsonPropertyName("eftPt")]
    public Common EftPt
    {
      get => eftPt ??= new();
      set => eftPt = value;
    }

    /// <summary>
    /// A value of RcvPt.
    /// </summary>
    [JsonPropertyName("rcvPt")]
    public Common RcvPt
    {
      get => rcvPt ??= new();
      set => rcvPt = value;
    }

    /// <summary>
    /// A value of WarNonTaf.
    /// </summary>
    [JsonPropertyName("warNonTaf")]
    public Common WarNonTaf
    {
      get => warNonTaf ??= new();
      set => warNonTaf = value;
    }

    /// <summary>
    /// A value of EftNonTaf.
    /// </summary>
    [JsonPropertyName("eftNonTaf")]
    public Common EftNonTaf
    {
      get => eftNonTaf ??= new();
      set => eftNonTaf = value;
    }

    /// <summary>
    /// A value of RcvNonTaf.
    /// </summary>
    [JsonPropertyName("rcvNonTaf")]
    public Common RcvNonTaf
    {
      get => rcvNonTaf ??= new();
      set => rcvNonTaf = value;
    }

    /// <summary>
    /// A value of WarRef.
    /// </summary>
    [JsonPropertyName("warRef")]
    public Common WarRef
    {
      get => warRef ??= new();
      set => warRef = value;
    }

    /// <summary>
    /// A value of WarAdv.
    /// </summary>
    [JsonPropertyName("warAdv")]
    public Common WarAdv
    {
      get => warAdv ??= new();
      set => warAdv = value;
    }

    /// <summary>
    /// A value of RcvRef.
    /// </summary>
    [JsonPropertyName("rcvRef")]
    public Common RcvRef
    {
      get => rcvRef ??= new();
      set => rcvRef = value;
    }

    /// <summary>
    /// A value of RcvAdv.
    /// </summary>
    [JsonPropertyName("rcvAdv")]
    public Common RcvAdv
    {
      get => rcvAdv ??= new();
      set => rcvAdv = value;
    }

    private Common cashCssi;
    private Common cashCssiCrAdj;
    private Common cashCssiColl;
    private Common cashCssiDist;
    private Common cashCssiRfnd;
    private Common cashCssiUndist;
    private Common cashCssiCrdAdj;
    private Common cashKsdlui;
    private Common cashKsdluiCrAdj;
    private Common cashKsdluiColl;
    private Common cashKsdluiDist;
    private Common cashKsdluiRfnd;
    private Common cashKsdluiUndist;
    private Common cashKsdluiCrdAdj;
    private DateWorkArea oct12000;
    private Common tempNonKpc;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private Common maxReports;
    private DateWorkArea max;
    private Common processPre10012000;
    private DateWorkArea searchFromLt10012000;
    private DateWorkArea searchToLt10012000;
    private Common processPost10012000;
    private DateWorkArea searchFromGe10012000;
    private DateWorkArea searchToGe10012000;
    private Common cashKpc;
    private Common cashKpcCrAdj;
    private Common cashKpcColl;
    private Common cashKpcDist;
    private Common cashKpcRfnd;
    private Common cashKpcUndist;
    private Common cashKpcCrdAdj;
    private Common cashFdso;
    private Common cashFdsoCrAdj;
    private Common cashFdsoColl;
    private Common cashFdsoDist;
    private Common cashFdsoRfnd;
    private Common cashFdsoUndist;
    private Common cashFdsoCrdAdj;
    private Common cashSdso;
    private Common cashSdsoCrAdj;
    private Common cashSdsoColl;
    private Common cashSdsoDist;
    private Common cashSdsoRfnd;
    private Common cashSdsoUndist;
    private Common cashSdsoCrdAdj;
    private Common cashMisc;
    private Common cashMiscCrAdj;
    private Common cashMiscColl;
    private Common cashMiscDist;
    private Common cashMiscRfnd;
    private Common cashMiscUndist;
    private Common cashMiscCrdAdj;
    private Common cashOther;
    private Common cashOtherCrAdj;
    private Common cashOtherColl;
    private Common cashOtherDist;
    private Common cashOtherRfnd;
    private Common cashOtherUndist;
    private Common cashOtherCrdAdj;
    private Common distTaf;
    private Common distNonTaf;
    private Common distFc;
    private Common distGaFc;
    private Common distTafAdj;
    private Common distNonTafAdj;
    private Common distFcAdj;
    private Common distGaFcAdj;
    private Common warPt;
    private Common eftPt;
    private Common rcvPt;
    private Common warNonTaf;
    private Common eftNonTaf;
    private Common rcvNonTaf;
    private Common warRef;
    private Common warAdv;
    private Common rcvRef;
    private Common rcvAdv;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KeyOnlyCashReceipt.
    /// </summary>
    [JsonPropertyName("keyOnlyCashReceipt")]
    public CashReceipt KeyOnlyCashReceipt
    {
      get => keyOnlyCashReceipt ??= new();
      set => keyOnlyCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("existingCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment ExistingCashReceiptBalanceAdjustment
    {
      get => existingCashReceiptBalanceAdjustment ??= new();
      set => existingCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of KeyOnlyCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("keyOnlyCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory KeyOnlyCashReceiptDetailStatHistory
    {
      get => keyOnlyCashReceiptDetailStatHistory ??= new();
      set => keyOnlyCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of KeyOnlyCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("keyOnlyCashReceiptDetailStatus")]
    public CashReceiptDetailStatus KeyOnlyCashReceiptDetailStatus
    {
      get => keyOnlyCashReceiptDetailStatus ??= new();
      set => keyOnlyCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of KeyOnlyCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("keyOnlyCashReceiptEvent")]
    public CashReceiptEvent KeyOnlyCashReceiptEvent
    {
      get => keyOnlyCashReceiptEvent ??= new();
      set => keyOnlyCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of KeyOnlyCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("keyOnlyCashReceiptSourceType")]
    public CashReceiptSourceType KeyOnlyCashReceiptSourceType
    {
      get => keyOnlyCashReceiptSourceType ??= new();
      set => keyOnlyCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of KeyOnlyCashReceiptType.
    /// </summary>
    [JsonPropertyName("keyOnlyCashReceiptType")]
    public CashReceiptType KeyOnlyCashReceiptType
    {
      get => keyOnlyCashReceiptType ??= new();
      set => keyOnlyCashReceiptType = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    /// <summary>
    /// A value of Reissued.
    /// </summary>
    [JsonPropertyName("reissued")]
    public PaymentRequest Reissued
    {
      get => reissued ??= new();
      set => reissued = value;
    }

    /// <summary>
    /// A value of ExistingPaymentRequest.
    /// </summary>
    [JsonPropertyName("existingPaymentRequest")]
    public PaymentRequest ExistingPaymentRequest
    {
      get => existingPaymentRequest ??= new();
      set => existingPaymentRequest = value;
    }

    /// <summary>
    /// A value of ExistingDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("existingDisbursementTransaction")]
    public DisbursementTransaction ExistingDisbursementTransaction
    {
      get => existingDisbursementTransaction ??= new();
      set => existingDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnly.
    /// </summary>
    [JsonPropertyName("existingKeyOnly")]
    public DisbursementType ExistingKeyOnly
    {
      get => existingKeyOnly ??= new();
      set => existingKeyOnly = value;
    }

    private CashReceipt keyOnlyCashReceipt;
    private CashReceipt existingCashReceipt;
    private CashReceiptBalanceAdjustment existingCashReceiptBalanceAdjustment;
    private CashReceiptDetail existingCashReceiptDetail;
    private CashReceiptDetailStatHistory keyOnlyCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus keyOnlyCashReceiptDetailStatus;
    private CashReceiptEvent keyOnlyCashReceiptEvent;
    private CashReceiptSourceType keyOnlyCashReceiptSourceType;
    private CashReceiptType keyOnlyCashReceiptType;
    private Collection existingCollection;
    private PaymentRequest reissued;
    private PaymentRequest existingPaymentRequest;
    private DisbursementTransaction existingDisbursementTransaction;
    private DisbursementType existingKeyOnly;
  }
#endregion
}
