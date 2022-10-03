// Program: LE_B578_PRORATE_UI_2_COURT_ORDER, ID: 945096064, model: 746.
// Short name: SWE03071
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B578_PRORATE_UI_2_COURT_ORDER.
/// </summary>
[Serializable]
public partial class LeB578ProrateUi2CourtOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B578_PRORATE_UI_2_COURT_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB578ProrateUi2CourtOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB578ProrateUi2CourtOrder.
  /// </summary>
  public LeB578ProrateUi2CourtOrder(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    // Date      Developer         Request #  Description
    // --------  ----------------  ---------  ------------------------
    // 05/14/12  GVandy            CQ33628    Initial Development
    // 09/28/12  GVandy            CQ36405    Correct a rounding problem when 
    // the final
    // 					court order has either a $0.00 WC amount or
    // 					$0.00 WA amount.
    // -------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------
    // -- This CAB prorates UI withholding across the court orders certified for
    // -- offset of unemployment compensation with Department of Labor.
    // --
    // -- The withholding amount is first prorated based on the percentage
    // -- of the total WC (current support) amount that is attributable to each 
    // court
    // -- order.  Any remaining amount is then prorated based on the percentage
    // -- of the total WA (arrears) amount that is attributable to each court 
    // order.
    // ------------------------------------------------------------------------------
    local.Local1.Index = -1;

    // ------------------------------------------------------------------------------
    // -- Read court order information for the NCP from the DOL certification 
    // table.
    // ------------------------------------------------------------------------------
    foreach(var item in ReadDolUiWithholding())
    {
      ++local.Local1.Index;
      local.Local1.CheckSize();

      MoveCashReceiptDetail(import.CashReceiptDetail,
        local.Local1.Update.GcashReceiptDetail);
      local.Local1.Update.GcashReceiptDetail.CollectionAmount = 0;
      local.Local1.Update.GcashReceiptDetail.CourtOrderNumber =
        entities.DolUiWithholding.StandardNumber;
      MoveDolUiWithholding(entities.DolUiWithholding,
        local.Local1.Update.GdolUiWithholding);

      // -- Calculate the total amount of WC and WA ordered across all court 
      // orders.
      local.TotalWc.TotalCurrency += entities.DolUiWithholding.WcAmount.
        GetValueOrDefault();
      local.TotalWa.TotalCurrency += entities.DolUiWithholding.WaAmount.
        GetValueOrDefault();

      // -- Determine the subscript of the final court order with a non zero WC 
      // certified amount.
      if (Lt(0, entities.DolUiWithholding.WcAmount))
      {
        local.FinalWcCourtOrder.Count = local.Local1.Index + 1;
      }

      // -- Determine the subscript of the final court order with a non zero WA 
      // certified amount.
      if (Lt(0, entities.DolUiWithholding.WaAmount))
      {
        local.FinalWaCourtOrder.Count = local.Local1.Index + 1;
      }
    }

    // ------------------------------------------------------------------------------
    // -- Log to the error report if we didn't find a certification record for 
    // this
    // -- person.  We'll still receipt the money without a court order but we
    // -- shouldn't receive money for people we didn't certify.
    // ------------------------------------------------------------------------------
    if (local.Local1.Count == 0)
    {
      local.ForConversion.TotalCurrency =
        import.CashReceiptDetail.CollectionAmount;
      UseFnCabCurrencyToText();
      local.BatchConvertNumToText.TextNumber9 =
        Substring(local.TextWorkArea.Text10, 2, 9);
      local.DateWorkArea.Date =
        import.DolUiWithholding.WithholdingCertificationDate;
      UseCabDate2TextWithHyphens3();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "KDOL SSN=" + (
        import.CashReceiptDetail.ObligorSocialSecurityNumber ?? "") + ", CSP Number=" +
        (import.CashReceiptDetail.ObligorPersonNumber ?? "") + ", Certification Date=" +
        local.TextWorkArea.Text10 + " not found.  $" + local
        .BatchConvertNumToText.TextNumber9 + " UI will be receipted w/o court order number.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered writing non ceritified person info to the error report.  Status = " +
          local.EabFileHandling.Status;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -- The amount available to allocate to current support is the total 
    // amount of the withholding.
    local.TotalAmountAvailable.TotalCurrency =
      import.CashReceiptDetail.CollectionAmount;

    // -- Keep a running total of the amount left to allocate.
    local.AmountStillToProrate.TotalCurrency =
      import.CashReceiptDetail.CollectionAmount;

    // ----------------------------------------------------------------------------
    // -- Allocate to Current Support first.
    // ----------------------------------------------------------------------------
    // -- 09/28/12 GVandy CQ36405 Correct a rounding problem when the final 
    // court order
    //    has either a $0.00 WC amount or $0.00 WA amount.  Original logic 
    // commented out above.
    local.Local1.Index = 0;

    for(var limit = local.FinalWcCourtOrder.Count; local.Local1.Index < limit; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      if (local.TotalAmountAvailable.TotalCurrency < local
        .TotalWc.TotalCurrency)
      {
        // -- There isn't enough money to satisfy the WC amounts for all the NCP
        // court orders.
        //    The withholding amount will be prorated based on the percentage of
        // the total
        //    WC (current support) amount that is attributable to this court 
        // order.
        local.Local1.Update.GlocalActualWcAllocationAmt.TotalCurrency =
          Math.Round(
            local.TotalAmountAvailable.TotalCurrency * local
          .Local1.Item.GdolUiWithholding.WcAmount.GetValueOrDefault() /
          local.TotalWc.TotalCurrency, 2, MidpointRounding.AwayFromZero);

        if (local.Local1.Index + 1 == local.FinalWcCourtOrder.Count)
        {
          // -- Adjust the amount for the final court order to compensate for 
          // rounding.
          local.Local1.Update.GlocalActualWcAllocationAmt.TotalCurrency =
            local.Local1.Item.GlocalActualWcAllocationAmt.TotalCurrency + (
              local.AmountStillToProrate.TotalCurrency - local
            .Local1.Item.GlocalActualWcAllocationAmt.TotalCurrency);
        }
      }
      else
      {
        // -- There is enough money to satisfy all the WC amounts for the NCP.
        //    Allocate the full WC amount for this court order.
        local.Local1.Update.GlocalActualWcAllocationAmt.TotalCurrency =
          local.Local1.Item.GdolUiWithholding.WcAmount.GetValueOrDefault();
      }

      // -- Update the running total of the amount left to allocate.
      local.AmountStillToProrate.TotalCurrency -= local.Local1.Item.
        GlocalActualWcAllocationAmt.TotalCurrency;
    }

    local.Local1.CheckIndex();

    // ----------------------------------------------------------------------------
    // -- Allocate next to Arrears.
    // ----------------------------------------------------------------------------
    if (local.AmountStillToProrate.TotalCurrency > 0)
    {
      // -- The amount available to allocate to arrears is the amount remaining 
      // after current was allocated.
      local.TotalAmountAvailable.TotalCurrency =
        local.AmountStillToProrate.TotalCurrency;

      // -- 09/28/12 GVandy CQ36405 Correct a rounding problem when the final 
      // court order
      //    has either a $0.00 WC amount or $0.00 WA amount.  Original logic 
      // commented out above.
      local.Local1.Index = 0;

      for(var limit = local.FinalWaCourtOrder.Count; local.Local1.Index < limit
        ; ++local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (local.TotalAmountAvailable.TotalCurrency < local
          .TotalWa.TotalCurrency)
        {
          // -- There isn't enough money to satisfy the WA amounts for all the 
          // NCP court orders.
          //    The withholding amount will be prorated based on the percentage 
          // of the total
          //    WA (arrears) amount that is attributable to this court order.
          local.Local1.Update.GlocalActualWaAllocationAmt.TotalCurrency =
            Math.Round(
              local.TotalAmountAvailable.TotalCurrency * local
            .Local1.Item.GdolUiWithholding.WaAmount.GetValueOrDefault() /
            local.TotalWa.TotalCurrency, 2, MidpointRounding.AwayFromZero);

          if (local.Local1.Index + 1 == local.FinalWaCourtOrder.Count)
          {
            // -- Adjust the amount for the final court order to compensate for 
            // rounding.
            local.Local1.Update.GlocalActualWaAllocationAmt.TotalCurrency =
              local.Local1.Item.GlocalActualWaAllocationAmt.TotalCurrency + (
                local.AmountStillToProrate.TotalCurrency - local
              .Local1.Item.GlocalActualWaAllocationAmt.TotalCurrency);
          }
        }
        else
        {
          // -- There is enough money to satisfy all the WA amounts for the NCP.
          //    Allocate the full WA amount for this court order.
          local.Local1.Update.GlocalActualWaAllocationAmt.TotalCurrency =
            local.Local1.Item.GdolUiWithholding.WaAmount.GetValueOrDefault();
        }

        // -- Update the running total of the amount left to allocate.
        local.AmountStillToProrate.TotalCurrency -= local.Local1.Item.
          GlocalActualWaAllocationAmt.TotalCurrency;
      }

      local.Local1.CheckIndex();
    }

    // ----------------------------------------------------------------------------
    // -- If there's still money remaining then allocate the excess.  This means
    // -- DOL withheld more than we certified.
    // ----------------------------------------------------------------------------
    // -- 09/28/12 GVandy CQ36405 Correct a rounding problem when the final 
    // court order
    //    has either a $0.00 WC amount or $0.00 WA amount.  Original logic 
    // commented out above.
    if (local.AmountStillToProrate.TotalCurrency > 0)
    {
      // -- The excess amount available to allocate to arrears is the amount 
      // remaining after current was allocated.
      local.TotalAmountAvailable.TotalCurrency =
        local.AmountStillToProrate.TotalCurrency;

      if (local.TotalWa.TotalCurrency > 0)
      {
        // -- If the person has WA ordered then prorate the excess amount based 
        // on the
        //    percentage of the total WA (arrears) amount that is attributable 
        // to this
        //    court order.
        local.Local1.Index = 0;

        for(var limit = local.FinalWaCourtOrder.Count; local.Local1.Index < limit
          ; ++local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          local.Local1.Update.GlocalActualExcessAmt.TotalCurrency =
            Math.Round(
              local.TotalAmountAvailable.TotalCurrency * local
            .Local1.Item.GdolUiWithholding.WaAmount.GetValueOrDefault() /
            local.TotalWa.TotalCurrency, 2, MidpointRounding.AwayFromZero);

          if (local.Local1.Index + 1 == local.FinalWaCourtOrder.Count)
          {
            // -- Adjust the amount for the final court order to compensate for 
            // rounding.
            local.Local1.Update.GlocalActualExcessAmt.TotalCurrency =
              local.Local1.Item.GlocalActualExcessAmt.TotalCurrency + (
                local.AmountStillToProrate.TotalCurrency - local
              .Local1.Item.GlocalActualExcessAmt.TotalCurrency);
          }

          // -- Update the running total of the amount left to allocate.
          local.AmountStillToProrate.TotalCurrency -= local.Local1.Item.
            GlocalActualExcessAmt.TotalCurrency;
        }

        local.Local1.CheckIndex();
      }
      else
      {
        // -- If the person does not have WA ordered then prorate the excess 
        // amount based on the
        //    percentage of the total WC (current) amount that is attributable 
        // to this
        //    court order.
        local.Local1.Index = 0;

        for(var limit = local.FinalWcCourtOrder.Count; local.Local1.Index < limit
          ; ++local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          local.Local1.Update.GlocalActualExcessAmt.TotalCurrency =
            Math.Round(
              local.TotalAmountAvailable.TotalCurrency * local
            .Local1.Item.GdolUiWithholding.WcAmount.GetValueOrDefault() /
            local.TotalWc.TotalCurrency, 2, MidpointRounding.AwayFromZero);

          if (local.Local1.Index + 1 == local.FinalWcCourtOrder.Count)
          {
            // -- Adjust the amount for the final court order to compensate for 
            // rounding.
            local.Local1.Update.GlocalActualExcessAmt.TotalCurrency =
              local.Local1.Item.GlocalActualExcessAmt.TotalCurrency + (
                local.AmountStillToProrate.TotalCurrency - local
              .Local1.Item.GlocalActualExcessAmt.TotalCurrency);
          }

          // -- Update the running total of the amount left to allocate.
          local.AmountStillToProrate.TotalCurrency -= local.Local1.Item.
            GlocalActualExcessAmt.TotalCurrency;
        }

        local.Local1.CheckIndex();
      }
    }

    // ----------------------------------------------------------------------------
    // -- If we didn't find any certification records it means DOL sent us money
    // -- for someone we didn't certify.  Receipt the full amount withheld.
    // -- No court order will be attributed to this money.  This is no different
    // -- than if an employer erroneously forwards withholding for someone whom
    // -- they shouldn't.
    // ----------------------------------------------------------------------------
    if (local.Local1.Count == 0)
    {
      local.Local1.Index = 0;
      local.Local1.CheckSize();

      MoveCashReceiptDetail(import.CashReceiptDetail,
        local.Local1.Update.GcashReceiptDetail);
      local.Local1.Update.GlocalActualExcessAmt.TotalCurrency =
        import.CashReceiptDetail.CollectionAmount;
    }

    // ----------------------------------------------------------------------------
    // -- Move all court orders to which we allocated money to the export group.
    // ----------------------------------------------------------------------------
    export.Export1.Index = -1;

    for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
      local.Local1.Index)
    {
      if (!local.Local1.CheckSize())
      {
        break;
      }

      if (local.Local1.Item.GlocalActualWcAllocationAmt.TotalCurrency > 0 || local
        .Local1.Item.GlocalActualWaAllocationAmt.TotalCurrency > 0 || local
        .Local1.Item.GlocalActualExcessAmt.TotalCurrency > 0)
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.G.Assign(local.Local1.Item.GcashReceiptDetail);
        export.Export1.Update.G.CollectionAmount =
          local.Local1.Item.GlocalActualWcAllocationAmt.TotalCurrency + local
          .Local1.Item.GlocalActualWaAllocationAmt.TotalCurrency + local
          .Local1.Item.GlocalActualExcessAmt.TotalCurrency;
      }
    }

    local.Local1.CheckIndex();

    // ----------------------------------------------------------------------------
    // -- Log how the money was prorated to the control report.
    // ----------------------------------------------------------------------------
    if (local.Local1.Count > 0)
    {
      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = "";

            break;
          case 2:
            // -- Write payment header to control report.
            local.EabReportSend.RptDetail =
              "PERSON #     DOL DATE   DOL AMOUNT   CERT DATE  COURT ORDER    WC AMOUNT    WA AMOUNT   EXCESS AMT  WC CERTIFIED WA CERTIFIED";
              

            break;
          default:
            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing prorated amounts to the control report.  Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      local.Local1.Index = 0;

      for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        for(local.Common.Count = 1; local.Common.Count <= 5; ++
          local.Common.Count)
        {
          switch(local.Common.Count)
          {
            case 1:
              local.EabReportSend.RptDetail = "";
              local.ForConversion.TotalCurrency =
                local.Local1.Item.GdolUiWithholding.WaAmount.
                  GetValueOrDefault();

              break;
            case 2:
              local.ForConversion.TotalCurrency =
                local.Local1.Item.GdolUiWithholding.WcAmount.
                  GetValueOrDefault();

              break;
            case 3:
              local.ForConversion.TotalCurrency =
                local.Local1.Item.GlocalActualExcessAmt.TotalCurrency;

              break;
            case 4:
              local.ForConversion.TotalCurrency =
                local.Local1.Item.GlocalActualWaAllocationAmt.TotalCurrency;

              break;
            case 5:
              local.ForConversion.TotalCurrency =
                local.Local1.Item.GlocalActualWcAllocationAmt.TotalCurrency;

              break;
            default:
              break;
          }

          UseFnCabCurrencyToText();
          local.EabReportSend.RptDetail = "   " + local.TextWorkArea.Text10 + local
            .EabReportSend.RptDetail;
        }

        local.EabReportSend.RptDetail = "" + Substring
          (local.Local1.Item.GcashReceiptDetail.CourtOrderNumber, 20, 1, 12) + local
          .EabReportSend.RptDetail;

        if (local.Local1.Index == 0)
        {
          local.DateWorkArea.Date = import.CashReceiptDetail.CollectionDate;
          UseCabDate2TextWithHyphens2();
          local.DateWorkArea.Date =
            import.DolUiWithholding.WithholdingCertificationDate;
          UseCabDate2TextWithHyphens1();
          local.ForConversion.TotalCurrency =
            import.CashReceiptDetail.CollectionAmount;
          UseFnCabCurrencyToText();
          local.EabReportSend.RptDetail =
            (import.CashReceiptDetail.ObligorPersonNumber ?? "") + "  " + local
            .Date.Text10 + "   " + local.TextWorkArea.Text10 + " " + local
            .Date2.Text10 + " " + local.EabReportSend.RptDetail;
        }
        else
        {
          local.EabReportSend.RptDetail =
            "                                               " + local
            .EabReportSend.RptDetail;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered writing prorated amounts to the control report.  Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      local.Local1.CheckIndex();
    }

    // ------------------------------------------------------------------------------
    // -- Find the Department of Labor employer record.  (EIN=621444754)
    // ------------------------------------------------------------------------------
    if (!ReadEmployer())
    {
      ExitState = "LE0000_DOL_EMPLOYER_NOT_FOUND";

      return;
    }

    // ----------------------------------------------------------------------------
    // -- If we have attributed money to a court order for which DOL is not 
    // defined
    // -- as an IWGL income source for an ORDIWO2 legal action then create an 
    // auto
    // -- IWO, triggering the ORDIWO2B document, for that court order only.
    // ----------------------------------------------------------------------------
    export.Export1.Index = 0;

    for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (IsEmpty(export.Export1.Item.G.CourtOrderNumber))
      {
        continue;
      }

      local.LegalAction.StandardNumber =
        export.Export1.Item.G.CourtOrderNumber ?? "";

      foreach(var item in ReadLegalAction())
      {
        goto Next;
      }

      local.CsePerson.Number = import.CashReceiptDetail.ObligorPersonNumber ?? Spaces
        (10);
      UseLeCreateUiEmpIncomeSource();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Unable to create Income Source for person: " + local
          .CsePerson.Number + " Error: " + local.ExitStateWorkArea.Message;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

Next:
      ;
    }

    export.Export1.CheckIndex();
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
  }

  private static void MoveDolUiWithholding(DolUiWithholding source,
    DolUiWithholding target)
  {
    target.WaAmount = source.WaAmount;
    target.WcAmount = source.WcAmount;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.Date2.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.Date.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens3()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
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

  private void UseFnCabCurrencyToText()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.ForConversion.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseLeCreateUiEmpIncomeSource()
  {
    var useImport = new LeCreateUiEmpIncomeSource.Import();
    var useExport = new LeCreateUiEmpIncomeSource.Export();

    useImport.Employer.Identifier = entities.Employer.Identifier;
    useImport.CsePerson.Number = local.CsePerson.Number;
    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Filter.StandardNumber = local.LegalAction.StandardNumber;
    useImport.InterstateCaseloadOfficeServiceProvider.RoleCode =
      import.InterstateCaseloadOfficeServiceProvider.RoleCode;
    useImport.InterstateCaseloadServiceProvider.SystemGeneratedId =
      import.InterstateCaseloadServiceProvider.SystemGeneratedId;
    useImport.InterstateCaseloadOffice.SystemGeneratedId =
      import.InterstateCaseloadOffice.SystemGeneratedId;
    useImport.CentralOffDefaultAttyOfficeServiceProvider.RoleCode =
      import.CentralOffDefaultAttyOfficeServiceProvider.RoleCode;
    useImport.CentralOffDefaultAttyOffice.SystemGeneratedId =
      import.CentralOffDefaultAttyOffice.SystemGeneratedId;
    useImport.CentralOffDefaultAttyServiceProvider.SystemGeneratedId =
      import.CentralOffDefaultAttyServiceProvider.SystemGeneratedId;

    Call(LeCreateUiEmpIncomeSource.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadDolUiWithholding()
  {
    entities.DolUiWithholding.Populated = false;

    return ReadEach("ReadDolUiWithholding",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          import.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetString(
          command, "ssn",
          import.CashReceiptDetail.ObligorSocialSecurityNumber ?? "");
        db.SetDate(
          command, "iwoCertDate",
          import.DolUiWithholding.WithholdingCertificationDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DolUiWithholding.CsePersonNumber = db.GetString(reader, 0);
        entities.DolUiWithholding.WithholdingCertificationDate =
          db.GetDate(reader, 1);
        entities.DolUiWithholding.StandardNumber = db.GetString(reader, 2);
        entities.DolUiWithholding.SocialSecurityNumber =
          db.GetString(reader, 3);
        entities.DolUiWithholding.WaAmount = db.GetNullableDecimal(reader, 4);
        entities.DolUiWithholding.WcAmount = db.GetNullableDecimal(reader, 5);
        entities.DolUiWithholding.MaxWithholdingPercent =
          db.GetNullableInt32(reader, 6);
        entities.DolUiWithholding.FirstName = db.GetNullableString(reader, 7);
        entities.DolUiWithholding.LastName = db.GetNullableString(reader, 8);
        entities.DolUiWithholding.MiddleInitial =
          db.GetNullableString(reader, 9);
        entities.DolUiWithholding.Populated = true;

        return true;
      });
  }

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      null,
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", local.LegalAction.StandardNumber ?? "");
        db.SetNullableInt32(command, "empId", entities.Employer.Identifier);
        db.SetString(
          command, "cspINumber", export.Export1.Item.G.ObligorPersonNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.ActionTaken = db.GetString(reader, 1);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.Populated = true;

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
    /// A value of InterstateCaseloadOffice.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOffice")]
    public Office InterstateCaseloadOffice
    {
      get => interstateCaseloadOffice ??= new();
      set => interstateCaseloadOffice = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadServiceProvider")]
    public ServiceProvider InterstateCaseloadServiceProvider
    {
      get => interstateCaseloadServiceProvider ??= new();
      set => interstateCaseloadServiceProvider = value;
    }

    /// <summary>
    /// A value of InterstateCaseloadOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("interstateCaseloadOfficeServiceProvider")]
    public OfficeServiceProvider InterstateCaseloadOfficeServiceProvider
    {
      get => interstateCaseloadOfficeServiceProvider ??= new();
      set => interstateCaseloadOfficeServiceProvider = value;
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
    /// A value of DolUiWithholding.
    /// </summary>
    [JsonPropertyName("dolUiWithholding")]
    public DolUiWithholding DolUiWithholding
    {
      get => dolUiWithholding ??= new();
      set => dolUiWithholding = value;
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
    /// A value of CentralOffDefaultAttyServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyServiceProvider")]
    public ServiceProvider CentralOffDefaultAttyServiceProvider
    {
      get => centralOffDefaultAttyServiceProvider ??= new();
      set => centralOffDefaultAttyServiceProvider = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOffice.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOffice")]
    public Office CentralOffDefaultAttyOffice
    {
      get => centralOffDefaultAttyOffice ??= new();
      set => centralOffDefaultAttyOffice = value;
    }

    /// <summary>
    /// A value of CentralOffDefaultAttyOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("centralOffDefaultAttyOfficeServiceProvider")]
    public OfficeServiceProvider CentralOffDefaultAttyOfficeServiceProvider
    {
      get => centralOffDefaultAttyOfficeServiceProvider ??= new();
      set => centralOffDefaultAttyOfficeServiceProvider = value;
    }

    private Office interstateCaseloadOffice;
    private ServiceProvider interstateCaseloadServiceProvider;
    private OfficeServiceProvider interstateCaseloadOfficeServiceProvider;
    private ProgramProcessingInfo programProcessingInfo;
    private DolUiWithholding dolUiWithholding;
    private CashReceiptDetail cashReceiptDetail;
    private ServiceProvider centralOffDefaultAttyServiceProvider;
    private Office centralOffDefaultAttyOffice;
    private OfficeServiceProvider centralOffDefaultAttyOfficeServiceProvider;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CashReceiptDetail G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CashReceiptDetail g;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of GcashReceiptDetail.
      /// </summary>
      [JsonPropertyName("gcashReceiptDetail")]
      public CashReceiptDetail GcashReceiptDetail
      {
        get => gcashReceiptDetail ??= new();
        set => gcashReceiptDetail = value;
      }

      /// <summary>
      /// A value of GdolUiWithholding.
      /// </summary>
      [JsonPropertyName("gdolUiWithholding")]
      public DolUiWithholding GdolUiWithholding
      {
        get => gdolUiWithholding ??= new();
        set => gdolUiWithholding = value;
      }

      /// <summary>
      /// A value of GlocalActualWcAllocationAmt.
      /// </summary>
      [JsonPropertyName("glocalActualWcAllocationAmt")]
      public Common GlocalActualWcAllocationAmt
      {
        get => glocalActualWcAllocationAmt ??= new();
        set => glocalActualWcAllocationAmt = value;
      }

      /// <summary>
      /// A value of GlocalActualWaAllocationAmt.
      /// </summary>
      [JsonPropertyName("glocalActualWaAllocationAmt")]
      public Common GlocalActualWaAllocationAmt
      {
        get => glocalActualWaAllocationAmt ??= new();
        set => glocalActualWaAllocationAmt = value;
      }

      /// <summary>
      /// A value of GlocalActualExcessAmt.
      /// </summary>
      [JsonPropertyName("glocalActualExcessAmt")]
      public Common GlocalActualExcessAmt
      {
        get => glocalActualExcessAmt ??= new();
        set => glocalActualExcessAmt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CashReceiptDetail gcashReceiptDetail;
      private DolUiWithholding gdolUiWithholding;
      private Common glocalActualWcAllocationAmt;
      private Common glocalActualWaAllocationAmt;
      private Common glocalActualExcessAmt;
    }

    /// <summary>
    /// A value of FinalWaCourtOrder.
    /// </summary>
    [JsonPropertyName("finalWaCourtOrder")]
    public Common FinalWaCourtOrder
    {
      get => finalWaCourtOrder ??= new();
      set => finalWaCourtOrder = value;
    }

    /// <summary>
    /// A value of FinalWcCourtOrder.
    /// </summary>
    [JsonPropertyName("finalWcCourtOrder")]
    public Common FinalWcCourtOrder
    {
      get => finalWcCourtOrder ??= new();
      set => finalWcCourtOrder = value;
    }

    /// <summary>
    /// A value of Date2.
    /// </summary>
    [JsonPropertyName("date2")]
    public TextWorkArea Date2
    {
      get => date2 ??= new();
      set => date2 = value;
    }

    /// <summary>
    /// A value of ForConversion.
    /// </summary>
    [JsonPropertyName("forConversion")]
    public Common ForConversion
    {
      get => forConversion ??= new();
      set => forConversion = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
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
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of TotalAmountAvailable.
    /// </summary>
    [JsonPropertyName("totalAmountAvailable")]
    public Common TotalAmountAvailable
    {
      get => totalAmountAvailable ??= new();
      set => totalAmountAvailable = value;
    }

    /// <summary>
    /// A value of AmountToApply.
    /// </summary>
    [JsonPropertyName("amountToApply")]
    public Common AmountToApply
    {
      get => amountToApply ??= new();
      set => amountToApply = value;
    }

    /// <summary>
    /// A value of AmountStillToProrate.
    /// </summary>
    [JsonPropertyName("amountStillToProrate")]
    public Common AmountStillToProrate
    {
      get => amountStillToProrate ??= new();
      set => amountStillToProrate = value;
    }

    /// <summary>
    /// A value of TotalWc.
    /// </summary>
    [JsonPropertyName("totalWc")]
    public Common TotalWc
    {
      get => totalWc ??= new();
      set => totalWc = value;
    }

    /// <summary>
    /// A value of TotalWa.
    /// </summary>
    [JsonPropertyName("totalWa")]
    public Common TotalWa
    {
      get => totalWa ??= new();
      set => totalWa = value;
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

    private Common finalWaCourtOrder;
    private Common finalWcCourtOrder;
    private TextWorkArea date2;
    private Common forConversion;
    private TextWorkArea date;
    private Common common;
    private BatchConvertNumToText batchConvertNumToText;
    private TextWorkArea textWorkArea;
    private DateWorkArea dateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private Array<LocalGroup> local1;
    private Common totalAmountAvailable;
    private Common amountToApply;
    private Common amountStillToProrate;
    private Common totalWc;
    private Common totalWa;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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
    /// A value of DolUiWithholding.
    /// </summary>
    [JsonPropertyName("dolUiWithholding")]
    public DolUiWithholding DolUiWithholding
    {
      get => dolUiWithholding ??= new();
      set => dolUiWithholding = value;
    }

    private CsePerson csePerson;
    private Employer employer;
    private IncomeSource incomeSource;
    private LegalActionIncomeSource legalActionIncomeSource;
    private LegalAction legalAction;
    private DolUiWithholding dolUiWithholding;
  }
#endregion
}
