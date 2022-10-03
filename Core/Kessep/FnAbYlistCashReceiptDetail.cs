// Program: FN_AB_YLIST_CASH_RECEIPT_DETAIL, ID: 371876089, model: 746.
// Short name: SWE02270
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
/// A program: FN_AB_YLIST_CASH_RECEIPT_DETAIL.
/// </para>
/// <para>
/// This action block retrieves list of cash receipt details from REIP receipts.
/// </para>
/// </summary>
[Serializable]
public partial class FnAbYlistCashReceiptDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_YLIST_CASH_RECEIPT_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbYlistCashReceiptDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbYlistCashReceiptDetail.
  /// </summary>
  public FnAbYlistCashReceiptDetail(IContext context, Import import,
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
    // *************************************************************************************************
    // Maintenance Log
    // Date      Author	Description
    // 04/28/97  Ty Hill	Change Current_date
    // 12/01/98  S. Newman	Rewrote cab to remove code causing duplicate 
    // displays.  Revised reads
    // 			to process three filters with If Statements rather than qualified 
    // reads.
    // 			Revised reads to check for only REIP payments.  This action block will
    // only
    // 			be used when the Pay History Indicator on CRDL = 'Y'
    // 11/05/99  PPhinney	H00077902  - Add filter for Collection_Type and 
    // Interface_Tran_ID
    // 03/21/00  PPhinney	H00091214  - Add code to keep blank lines off 
    // scrolling.
    // 09/19/01  P. Phinney	WR010561  Prevent display of CRD's deleted by REIP.
    // 07/17/03  GVandy 	Performance improvements when only source type and 
    // interface trans id are
    // 			entered.
    // 07/21/16  GVandy 	CQ52578 - Performance improvements when no source type 
    // and no cash
    // 			receipt number are entered.
    // *************************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // **** Get hardcode values
    UseFnHardcodedCashReceipting();

    // **** Set maximum discontinue date****
    UseCabSetMaximumDiscontinueDate();

    // *******Source Type is Blank.  Source Filter Is Not Used.
    if (IsEmpty(import.UserCashReceiptSourceType.Code))
    {
      // ****Cash Receipt Detail Is Blank.  Cash Receipt Detail Filter Is Not 
      // Used.****
      if (import.UserCashReceipt.SequentialNumber == 0)
      {
        // 07/21/16  GVandy  CQ52578 - Performance improvements when no source 
        // type and no cash
        // receipt number are entered. Original read each is commented out 
        // below.
        if (Equal(import.UserInputFrom.ReceivedDate, null))
        {
          local.UserInputFrom.ReceivedDate = new DateTime(1, 1, 1);
        }
        else
        {
          local.UserInputFrom.ReceivedDate = import.UserInputFrom.ReceivedDate;
        }

        if (Equal(import.UserInputToCashReceiptEvent.ReceivedDate, null))
        {
          local.UserInputTo.ReceivedDate = new DateTime(2099, 12, 31);
        }
        else
        {
          local.UserInputTo.ReceivedDate =
            import.UserInputToCashReceiptEvent.ReceivedDate;
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory5())
          
        {
          if (!ReadCashReceiptDetailStatus())
          {
            export.Export1.Next();

            continue;
          }

          if (!ReadCashReceiptSourceType())
          {
            export.Export1.Next();

            continue;
          }

          // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
          // deleted by REIP.
          // *****************************************************************
          if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
            "REIPDELETE"))
          {
            export.Export1.Next();

            continue;
          }

          // *****************************************************************
          // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type 
          // and Interface_Tran_ID
          if (!IsEmpty(import.FilterCashReceiptDetail.InterfaceTransId))
          {
            if (!Equal(import.FilterCashReceiptDetail.InterfaceTransId,
              entities.CashReceiptDetail.InterfaceTransId))
            {
              export.Export1.Next();

              continue;
            }
          }

          if (ReadCollectionType())
          {
            if (!IsEmpty(import.FilterCollectionType.Code))
            {
              if (!Equal(import.FilterCollectionType.Code,
                entities.CollectionType.Code))
              {
                export.Export1.Next();

                continue;
              }
            }

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            local.Save.Code = entities.CollectionType.Code;
          }
          else
          {
            if (!IsEmpty(import.FilterCollectionType.Code))
            {
              export.Export1.Next();

              continue;
            }

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            local.Save.Code = "*";
          }

          // * * * * * * * * * * * * H00077902 * * * * * * * * * * * * * * * *
          foreach(var item1 in ReadCashReceiptType2())
          {
          }

          if (AsChar(import.UndistributedOnly.Flag) == 'Y')
          {
            local.Net.CollectionAmount =
              entities.CashReceiptDetail.CollectionAmount;

            if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
            {
              local.UndistAmount.TotalCurrency = 0;
            }
            else
            {
              local.UndistAmount.TotalCurrency = local.Net.CollectionAmount - entities
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
            }

            // ****Court Interface  Adjustments****
            foreach(var item1 in ReadCashReceiptDetail())
            {
              local.UndistAmount.TotalCurrency = 0;
            }

            if (local.UndistAmount.TotalCurrency <= 0)
            {
              export.Export1.Next();

              continue;
            }

            export.Export1.Update.UndistributedAmt.TotalCurrency =
              local.UndistAmount.TotalCurrency;
            export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
            export.Export1.Update.CashReceiptDetail.Assign(
              entities.CashReceiptDetail);
            MoveCashReceiptSourceType(entities.CashReceiptSourceType,
              export.Export1.Update.CashReceiptSourceType);
            MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
              export.Export1.Update.DetailCashReceiptEvent);

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            export.Export1.Update.CollectionType.Code = local.Save.Code;
            export.Export1.Update.CashReceiptDetail.CollectionAmount =
              local.Net.CollectionAmount;
            MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
              export.Export1.Update.CashReceiptDetailStatus);
            UseFnAbConcatCrAndCrd();

            if (ReadCashReceiptType1())
            {
              export.Export1.Update.DetailCashReceiptType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;
            }
          }
          else
          {
            local.Net.CollectionAmount =
              entities.CashReceiptDetail.CollectionAmount;
            export.Export1.Update.CashReceiptDetail.CollectionAmount =
              local.Net.CollectionAmount;

            // *****COLA (Manual Adjustments)*****
            if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
            {
              local.UndistAmount.TotalCurrency = 0;
            }
            else
            {
              local.UndistAmount.TotalCurrency = local.Net.CollectionAmount - entities
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
            }

            // ****Court Interface  Adjustments****
            foreach(var item1 in ReadCashReceiptDetail())
            {
              local.UndistAmount.TotalCurrency = 0;
            }

            export.Export1.Update.UndistributedAmt.TotalCurrency =
              local.UndistAmount.TotalCurrency;
            export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
            export.Export1.Update.CashReceiptDetail.Assign(
              entities.CashReceiptDetail);
            MoveCashReceiptSourceType(entities.CashReceiptSourceType,
              export.Export1.Update.CashReceiptSourceType);
            MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
              export.Export1.Update.DetailCashReceiptEvent);
            export.Export1.Update.CashReceiptDetail.CollectionAmount =
              local.Net.CollectionAmount;

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            export.Export1.Update.CollectionType.Code = local.Save.Code;
            export.Export1.Update.CashReceiptDetail.CollectionAmount =
              local.Net.CollectionAmount;
            MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
              export.Export1.Update.CashReceiptDetailStatus);
            UseFnAbConcatCrAndCrd();

            if (ReadCashReceiptType1())
            {
              export.Export1.Update.DetailCashReceiptType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;
            }
          }

          export.Export1.Next();
        }
      }
      else
      {
        // ****Cash Receipt Detail Number is Filled In****
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory4())
          
        {
          // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
          // deleted by REIP.
          // *****************************************************************
          if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
            "REIPDELETE"))
          {
            export.Export1.Next();

            continue;
          }

          // *****************************************************************
          // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type 
          // and Interface_Tran_ID
          if (!IsEmpty(import.FilterCashReceiptDetail.InterfaceTransId))
          {
            if (!Equal(import.FilterCashReceiptDetail.InterfaceTransId,
              entities.CashReceiptDetail.InterfaceTransId))
            {
              export.Export1.Next();

              continue;
            }
          }

          if (ReadCollectionType())
          {
            if (!IsEmpty(import.FilterCollectionType.Code))
            {
              if (!Equal(import.FilterCollectionType.Code,
                entities.CollectionType.Code))
              {
                export.Export1.Next();

                continue;
              }
            }

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            local.Save.Code = entities.CollectionType.Code;
          }
          else
          {
            if (!IsEmpty(import.FilterCollectionType.Code))
            {
              export.Export1.Next();

              continue;
            }

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            local.Save.Code = "*";
          }

          // * * * * * * * * * * * * H00077902 * * * * * * * * * * * * * * * *
          if (AsChar(import.UndistributedOnly.Flag) == 'Y')
          {
            local.Net.CollectionAmount =
              entities.CashReceiptDetail.CollectionAmount;

            // *****COLA (Manual Adjustments)*****
            if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
            {
              local.UndistAmount.TotalCurrency = 0;
            }
            else
            {
              local.UndistAmount.TotalCurrency = local.Net.CollectionAmount - entities
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
            }

            // ****Court Interface  Adjustments****
            foreach(var item1 in ReadCashReceiptDetail())
            {
              local.UndistAmount.TotalCurrency = 0;
            }

            if (local.UndistAmount.TotalCurrency <= 0)
            {
              export.Export1.Next();

              continue;
            }

            export.Export1.Update.UndistributedAmt.TotalCurrency =
              local.UndistAmount.TotalCurrency;
            export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
            export.Export1.Update.CashReceiptDetail.Assign(
              entities.CashReceiptDetail);
            MoveCashReceiptSourceType(entities.CashReceiptSourceType,
              export.Export1.Update.CashReceiptSourceType);
            MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
              export.Export1.Update.DetailCashReceiptEvent);
            export.Export1.Update.CashReceiptDetail.CollectionAmount =
              local.Net.CollectionAmount;

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            export.Export1.Update.CollectionType.Code = local.Save.Code;
            MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
              export.Export1.Update.CashReceiptDetailStatus);
            UseFnAbConcatCrAndCrd();

            if (ReadCashReceiptType1())
            {
              export.Export1.Update.DetailCashReceiptType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;
            }
          }
          else
          {
            local.Net.CollectionAmount =
              entities.CashReceiptDetail.CollectionAmount;

            // *****COLA (Manual) Adjustments*****
            if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
            {
              local.UndistAmount.TotalCurrency = 0;
            }
            else
            {
              local.UndistAmount.TotalCurrency = local.Net.CollectionAmount - entities
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
            }

            // ****Court Interface  Adjustments****
            foreach(var item1 in ReadCashReceiptDetail())
            {
              local.UndistAmount.TotalCurrency = 0;
            }

            export.Export1.Update.UndistributedAmt.TotalCurrency =
              local.UndistAmount.TotalCurrency;
            export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
            export.Export1.Update.CashReceiptDetail.Assign(
              entities.CashReceiptDetail);
            MoveCashReceiptSourceType(entities.CashReceiptSourceType,
              export.Export1.Update.CashReceiptSourceType);
            MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
              export.Export1.Update.DetailCashReceiptEvent);
            export.Export1.Update.CashReceiptDetail.CollectionAmount =
              local.Net.CollectionAmount;

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            export.Export1.Update.CollectionType.Code = local.Save.Code;
            MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
              export.Export1.Update.CashReceiptDetailStatus);
            UseFnAbConcatCrAndCrd();

            if (ReadCashReceiptType1())
            {
              export.Export1.Update.DetailCashReceiptType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;
            }
          }

          export.Export1.Next();
        }
      }
    }
    else
    {
      // Import User Input Cash Receipt Sequence Number is not equal to zero
      if (import.UserCashReceipt.SequentialNumber == 0)
      {
        if (IsEmpty(import.FilterCashReceiptDetail.InterfaceTransId))
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory3())
            
          {
            // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
            // deleted by REIP.
            // *****************************************************************
            if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
              "REIPDELETE"))
            {
              export.Export1.Next();

              continue;
            }

            // *****************************************************************
            // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type
            // and Interface_Tran_ID
            if (!IsEmpty(import.FilterCashReceiptDetail.InterfaceTransId))
            {
              if (!Equal(import.FilterCashReceiptDetail.InterfaceTransId,
                entities.CashReceiptDetail.InterfaceTransId))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (ReadCollectionType())
            {
              if (!IsEmpty(import.FilterCollectionType.Code))
              {
                if (!Equal(import.FilterCollectionType.Code,
                  entities.CollectionType.Code))
                {
                  export.Export1.Next();

                  continue;
                }
              }

              // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines
              // off scrolling.
              local.Save.Code = entities.CollectionType.Code;
            }
            else
            {
              if (!IsEmpty(import.FilterCollectionType.Code))
              {
                export.Export1.Next();

                continue;
              }

              // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines
              // off scrolling.
              local.Save.Code = "*";
            }

            // * * * * * * * * * * * * H00077902 * * * * * * * * * * * * * * * *
            if (AsChar(import.UndistributedOnly.Flag) == 'Y')
            {
              local.Net.CollectionAmount =
                entities.CashReceiptDetail.CollectionAmount;

              // *****COLA (Manual) Adjustments*****
              if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
              {
                local.UndistAmount.TotalCurrency = 0;
              }
              else
              {
                local.UndistAmount.TotalCurrency =
                  local.Net.CollectionAmount - entities
                  .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                  .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
              }

              // ****Court Interface  Adjustments****
              foreach(var item1 in ReadCashReceiptDetail())
              {
                local.UndistAmount.TotalCurrency = 0;
              }

              if (local.UndistAmount.TotalCurrency <= 0)
              {
                export.Export1.Next();

                continue;
              }

              export.Export1.Update.UndistributedAmt.TotalCurrency =
                local.UndistAmount.TotalCurrency;
              export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
              export.Export1.Update.CashReceiptDetail.Assign(
                entities.CashReceiptDetail);
              MoveCashReceiptSourceType(entities.CashReceiptSourceType,
                export.Export1.Update.CashReceiptSourceType);
              MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
                export.Export1.Update.DetailCashReceiptEvent);
              export.Export1.Update.CashReceiptDetail.CollectionAmount =
                local.Net.CollectionAmount;

              // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines
              // off scrolling.
              export.Export1.Update.CollectionType.Code = local.Save.Code;
              MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
                export.Export1.Update.CashReceiptDetailStatus);
              UseFnAbConcatCrAndCrd();

              if (ReadCashReceiptType1())
              {
                export.Export1.Update.DetailCashReceiptType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptType.SystemGeneratedIdentifier;
              }
            }
            else
            {
              local.Net.CollectionAmount =
                entities.CashReceiptDetail.CollectionAmount;

              // *****COLA (Manual) Adjustments*****
              if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
              {
                local.UndistAmount.TotalCurrency = 0;
              }
              else
              {
                local.UndistAmount.TotalCurrency =
                  local.Net.CollectionAmount - entities
                  .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                  .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
              }

              // ****Court Interface  Adjustments****
              foreach(var item1 in ReadCashReceiptDetail())
              {
                local.UndistAmount.TotalCurrency = 0;
              }

              export.Export1.Update.UndistributedAmt.TotalCurrency =
                local.UndistAmount.TotalCurrency;
              export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
              export.Export1.Update.CashReceiptDetail.Assign(
                entities.CashReceiptDetail);
              MoveCashReceiptSourceType(entities.CashReceiptSourceType,
                export.Export1.Update.CashReceiptSourceType);
              MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
                export.Export1.Update.DetailCashReceiptEvent);
              export.Export1.Update.CashReceiptDetail.CollectionAmount =
                local.Net.CollectionAmount;

              // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines
              // off scrolling.
              export.Export1.Update.CollectionType.Code = local.Save.Code;
              MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
                export.Export1.Update.CashReceiptDetailStatus);
              UseFnAbConcatCrAndCrd();

              if (ReadCashReceiptType1())
              {
                export.Export1.Update.DetailCashReceiptType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptType.SystemGeneratedIdentifier;
              }
            }

            export.Export1.Next();
          }
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory1())
            
          {
            // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
            // deleted by REIP.
            // *****************************************************************
            if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
              "REIPDELETE"))
            {
              export.Export1.Next();

              continue;
            }

            // *****************************************************************
            // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type
            // and Interface_Tran_ID
            if (ReadCollectionType())
            {
              if (!IsEmpty(import.FilterCollectionType.Code))
              {
                if (!Equal(import.FilterCollectionType.Code,
                  entities.CollectionType.Code))
                {
                  export.Export1.Next();

                  continue;
                }
              }

              // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines
              // off scrolling.
              local.Save.Code = entities.CollectionType.Code;
            }
            else
            {
              if (!IsEmpty(import.FilterCollectionType.Code))
              {
                export.Export1.Next();

                continue;
              }

              // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines
              // off scrolling.
              local.Save.Code = "*";
            }

            // * * * * * * * * * * * * H00077902 * * * * * * * * * * * * * * * *
            if (AsChar(import.UndistributedOnly.Flag) == 'Y')
            {
              local.Net.CollectionAmount =
                entities.CashReceiptDetail.CollectionAmount;

              // *****COLA (Manual) Adjustments*****
              if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
              {
                local.UndistAmount.TotalCurrency = 0;
              }
              else
              {
                local.UndistAmount.TotalCurrency =
                  local.Net.CollectionAmount - entities
                  .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                  .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
              }

              // ****Court Interface  Adjustments****
              foreach(var item1 in ReadCashReceiptDetail())
              {
                local.UndistAmount.TotalCurrency = 0;
              }

              if (local.UndistAmount.TotalCurrency <= 0)
              {
                export.Export1.Next();

                continue;
              }

              export.Export1.Update.UndistributedAmt.TotalCurrency =
                local.UndistAmount.TotalCurrency;
              export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
              export.Export1.Update.CashReceiptDetail.Assign(
                entities.CashReceiptDetail);
              MoveCashReceiptSourceType(entities.CashReceiptSourceType,
                export.Export1.Update.CashReceiptSourceType);
              MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
                export.Export1.Update.DetailCashReceiptEvent);
              export.Export1.Update.CashReceiptDetail.CollectionAmount =
                local.Net.CollectionAmount;

              // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines
              // off scrolling.
              export.Export1.Update.CollectionType.Code = local.Save.Code;
              MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
                export.Export1.Update.CashReceiptDetailStatus);
              UseFnAbConcatCrAndCrd();

              if (ReadCashReceiptType1())
              {
                export.Export1.Update.DetailCashReceiptType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptType.SystemGeneratedIdentifier;
              }
            }
            else
            {
              local.Net.CollectionAmount =
                entities.CashReceiptDetail.CollectionAmount;

              // *****COLA (Manual) Adjustments*****
              if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
              {
                local.UndistAmount.TotalCurrency = 0;
              }
              else
              {
                local.UndistAmount.TotalCurrency =
                  local.Net.CollectionAmount - entities
                  .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                  .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
              }

              // ****Court Interface  Adjustments****
              foreach(var item1 in ReadCashReceiptDetail())
              {
                local.UndistAmount.TotalCurrency = 0;
              }

              export.Export1.Update.UndistributedAmt.TotalCurrency =
                local.UndistAmount.TotalCurrency;
              export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
              export.Export1.Update.CashReceiptDetail.Assign(
                entities.CashReceiptDetail);
              MoveCashReceiptSourceType(entities.CashReceiptSourceType,
                export.Export1.Update.CashReceiptSourceType);
              MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
                export.Export1.Update.DetailCashReceiptEvent);
              export.Export1.Update.CashReceiptDetail.CollectionAmount =
                local.Net.CollectionAmount;

              // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines
              // off scrolling.
              export.Export1.Update.CollectionType.Code = local.Save.Code;
              MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
                export.Export1.Update.CashReceiptDetailStatus);
              UseFnAbConcatCrAndCrd();

              if (ReadCashReceiptType1())
              {
                export.Export1.Update.DetailCashReceiptType.
                  SystemGeneratedIdentifier =
                    entities.CashReceiptType.SystemGeneratedIdentifier;
              }
            }

            export.Export1.Next();
          }
        }
      }
      else
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory2())
          
        {
          // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
          // deleted by REIP.
          // *****************************************************************
          if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
            "REIPDELETE"))
          {
            export.Export1.Next();

            continue;
          }

          // *****************************************************************
          // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type 
          // and Interface_Tran_ID
          if (!IsEmpty(import.FilterCashReceiptDetail.InterfaceTransId))
          {
            if (!Equal(import.FilterCashReceiptDetail.InterfaceTransId,
              entities.CashReceiptDetail.InterfaceTransId))
            {
              export.Export1.Next();

              continue;
            }
          }

          if (ReadCollectionType())
          {
            if (!IsEmpty(import.FilterCollectionType.Code))
            {
              if (!Equal(import.FilterCollectionType.Code,
                entities.CollectionType.Code))
              {
                export.Export1.Next();

                continue;
              }
            }

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            local.Save.Code = entities.CollectionType.Code;
          }
          else
          {
            if (!IsEmpty(import.FilterCollectionType.Code))
            {
              export.Export1.Next();

              continue;
            }

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            local.Save.Code = "*";
          }

          // * * * * * * * * * * * * H00077902 * * * * * * * * * * * * * * * *
          if (AsChar(import.UndistributedOnly.Flag) == 'Y')
          {
            local.Net.CollectionAmount =
              entities.CashReceiptDetail.CollectionAmount;

            if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
            {
              local.UndistAmount.TotalCurrency = 0;
            }
            else
            {
              local.UndistAmount.TotalCurrency = local.Net.CollectionAmount - entities
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
            }

            // ****Court Interface  Adjustments****
            foreach(var item1 in ReadCashReceiptDetail())
            {
              local.UndistAmount.TotalCurrency = 0;
            }

            if (local.UndistAmount.TotalCurrency <= 0)
            {
              export.Export1.Next();

              continue;
            }

            export.Export1.Update.UndistributedAmt.TotalCurrency =
              local.UndistAmount.TotalCurrency;
            export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
            export.Export1.Update.CashReceiptDetail.Assign(
              entities.CashReceiptDetail);
            MoveCashReceiptSourceType(entities.CashReceiptSourceType,
              export.Export1.Update.CashReceiptSourceType);
            MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
              export.Export1.Update.DetailCashReceiptEvent);
            export.Export1.Update.CashReceiptDetail.CollectionAmount =
              local.Net.CollectionAmount;

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            export.Export1.Update.CollectionType.Code = local.Save.Code;
            MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
              export.Export1.Update.CashReceiptDetailStatus);
            UseFnAbConcatCrAndCrd();

            if (ReadCashReceiptType1())
            {
              export.Export1.Update.DetailCashReceiptType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;
            }
          }
          else
          {
            local.Net.CollectionAmount =
              entities.CashReceiptDetail.CollectionAmount;

            // *****COLA (Manual) Adjustments*****
            if (Equal(entities.CashReceiptDetailStatus.Code, "ADJ"))
            {
              local.UndistAmount.TotalCurrency = 0;
            }
            else
            {
              local.UndistAmount.TotalCurrency = local.Net.CollectionAmount - entities
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
                .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
            }

            // ****Court Interface  Adjustments****
            foreach(var item1 in ReadCashReceiptDetail())
            {
              local.UndistAmount.TotalCurrency = 0;
            }

            export.Export1.Update.UndistributedAmt.TotalCurrency =
              local.UndistAmount.TotalCurrency;
            export.Export1.Update.CashReceipt.Assign(entities.CashReceipt);
            export.Export1.Update.CashReceiptDetail.Assign(
              entities.CashReceiptDetail);
            MoveCashReceiptSourceType(entities.CashReceiptSourceType,
              export.Export1.Update.CashReceiptSourceType);
            MoveCashReceiptEvent(entities.IdOnlyCashReceiptEvent,
              export.Export1.Update.DetailCashReceiptEvent);
            export.Export1.Update.CashReceiptDetail.CollectionAmount =
              local.Net.CollectionAmount;

            // 03/21/00    H00091214  PPhinney  - Add code to keep blank lines 
            // off scrolling.
            export.Export1.Update.CollectionType.Code = local.Save.Code;
            MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
              export.Export1.Update.CashReceiptDetailStatus);
            UseFnAbConcatCrAndCrd();

            if (ReadCashReceiptType1())
            {
              export.Export1.Update.DetailCashReceiptType.
                SystemGeneratedIdentifier =
                  entities.CashReceiptType.SystemGeneratedIdentifier;
            }
          }

          export.Export1.Next();
        }
      }
    }
  }

  private static void MoveCashReceiptDetailStatus(
    CashReceiptDetailStatus source, CashReceiptDetailStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxDiscontinue.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDiscontinue.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    export.Export1.Update.DetailCrdCrComboNo.CrdCrCombo =
      useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.Suspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.Pended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ExistingDetailAdj.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDetailAdj.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingDetailAdj.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingDetailAdj.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingDetailAdj.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDetailAdj.CollectionAmount = db.GetDecimal(reader, 4);
        entities.ExistingDetailAdj.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory1()
  {
    return ReadEach(
      "ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "interfaceTranId",
          import.FilterCashReceiptDetail.InterfaceTransId ?? "");
        db.SetInt32(
          command, "crdId",
          import.StartingCashReceiptDetail.SequentialIdentifier);
        db.SetString(command, "createdBy", import.UserCashReceipt.CreatedBy);
        db.SetDate(
          command, "receivedDate1",
          import.UserInputFrom.ReceivedDate.GetValueOrDefault());
        db.SetDate(command, "date", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "receivedDate2",
          import.UserInputToCashReceiptEvent.ReceivedDate.GetValueOrDefault());
        db.SetString(command, "code1", import.UserCashReceiptSourceType.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code2", import.CashReceiptDetailStatus.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.IdOnlyCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.IdOnlyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.IdOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 13);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 14);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 15);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 16);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 17);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 21);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 23);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 24);
        entities.IdOnlyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 24);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 25);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 26);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 27);
        entities.IdOnlyCashReceiptEvent.ReceivedDate = db.GetDate(reader, 28);
        entities.IdOnlyCashReceiptType.Code = db.GetString(reader, 29);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.IdOnlyCashReceiptEvent.Populated = true;
        entities.IdOnlyCashReceiptType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory2()
  {
    return ReadEach(
      "ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          import.StartingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", import.UserCashReceipt.SequentialNumber);
        db.SetString(command, "createdBy", import.UserCashReceipt.CreatedBy);
        db.SetDate(
          command, "receivedDate1",
          import.UserInputFrom.ReceivedDate.GetValueOrDefault());
        db.SetDate(command, "date", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "receivedDate2",
          import.UserInputToCashReceiptEvent.ReceivedDate.GetValueOrDefault());
        db.SetString(command, "code1", import.UserCashReceiptSourceType.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code2", import.CashReceiptDetailStatus.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.IdOnlyCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.IdOnlyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.IdOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 13);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 14);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 15);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 16);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 17);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 21);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 23);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 24);
        entities.IdOnlyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 24);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 25);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 26);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 27);
        entities.IdOnlyCashReceiptEvent.ReceivedDate = db.GetDate(reader, 28);
        entities.IdOnlyCashReceiptType.Code = db.GetString(reader, 29);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.IdOnlyCashReceiptEvent.Populated = true;
        entities.IdOnlyCashReceiptType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory3()
  {
    return ReadEach(
      "ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory3",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          import.StartingCashReceiptDetail.SequentialIdentifier);
        db.SetString(command, "createdBy", import.UserCashReceipt.CreatedBy);
        db.SetDate(
          command, "receivedDate1",
          import.UserInputFrom.ReceivedDate.GetValueOrDefault());
        db.SetDate(command, "date", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "receivedDate2",
          import.UserInputToCashReceiptEvent.ReceivedDate.GetValueOrDefault());
        db.SetString(command, "code1", import.UserCashReceiptSourceType.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code2", import.CashReceiptDetailStatus.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.IdOnlyCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.IdOnlyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.IdOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 13);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 14);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 15);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 16);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 17);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 21);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 23);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 24);
        entities.IdOnlyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 24);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 25);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 26);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 27);
        entities.IdOnlyCashReceiptEvent.ReceivedDate = db.GetDate(reader, 28);
        entities.IdOnlyCashReceiptType.Code = db.GetString(reader, 29);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.IdOnlyCashReceiptEvent.Populated = true;
        entities.IdOnlyCashReceiptType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory4()
  {
    return ReadEach(
      "ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory4",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          import.StartingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", import.UserCashReceipt.SequentialNumber);
        db.SetString(command, "createdBy", import.UserCashReceipt.CreatedBy);
        db.SetDate(
          command, "receivedDate1",
          import.UserInputFrom.ReceivedDate.GetValueOrDefault());
        db.SetDate(command, "date", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "receivedDate2",
          import.UserInputToCashReceiptEvent.ReceivedDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code", import.CashReceiptDetailStatus.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.IdOnlyCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.IdOnlyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.IdOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 13);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 14);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 15);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 16);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 17);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 21);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 23);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 24);
        entities.IdOnlyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 24);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 25);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 26);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 27);
        entities.IdOnlyCashReceiptEvent.ReceivedDate = db.GetDate(reader, 28);
        entities.IdOnlyCashReceiptType.Code = db.GetString(reader, 29);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.IdOnlyCashReceiptEvent.Populated = true;
        entities.IdOnlyCashReceiptType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory5()
  {
    return ReadEach(
      "ReadCashReceiptDetailCashReceiptCashReceiptDetailStatHistory5",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          import.StartingCashReceiptDetail.SequentialIdentifier);
        db.SetString(command, "createdBy", import.UserCashReceipt.CreatedBy);
        db.SetDate(
          command, "receivedDate1",
          local.UserInputFrom.ReceivedDate.GetValueOrDefault());
        db.SetDate(
          command, "receivedDate2",
          local.UserInputTo.ReceivedDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.IdOnlyCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.IdOnlyCashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.IdOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 13);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 14);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 15);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 16);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 17);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 21);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 22);
        entities.IdOnlyCashReceiptEvent.ReceivedDate = db.GetDate(reader, 23);
        entities.IdOnlyCashReceiptType.Code = db.GetString(reader, 24);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.IdOnlyCashReceiptEvent.Populated = true;
        entities.IdOnlyCashReceiptType.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptDetailStatHistory.Populated);
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          entities.CashReceiptDetailStatHistory.CdsIdentifier);
        db.SetString(command, "code", import.CashReceiptDetailStatus.Code);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.IdOnlyCashReceiptEvent.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          entities.IdOnlyCashReceiptEvent.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptType1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType1",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptType2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.IdOnlyCashReceiptType.Populated = false;

    return ReadEach("ReadCashReceiptType2",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.IdOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.IdOnlyCashReceiptType.Code = db.GetString(reader, 1);
        entities.IdOnlyCashReceiptType.Populated = true;

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
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
    /// A value of UserInputFrom.
    /// </summary>
    [JsonPropertyName("userInputFrom")]
    public CashReceiptEvent UserInputFrom
    {
      get => userInputFrom ??= new();
      set => userInputFrom = value;
    }

    /// <summary>
    /// A value of UserInputToCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("userInputToCashReceiptEvent")]
    public CashReceiptEvent UserInputToCashReceiptEvent
    {
      get => userInputToCashReceiptEvent ??= new();
      set => userInputToCashReceiptEvent = value;
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
    /// A value of StartingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("startingCashReceiptDetail")]
    public CashReceiptDetail StartingCashReceiptDetail
    {
      get => startingCashReceiptDetail ??= new();
      set => startingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of StartingCashReceipt.
    /// </summary>
    [JsonPropertyName("startingCashReceipt")]
    public CashReceipt StartingCashReceipt
    {
      get => startingCashReceipt ??= new();
      set => startingCashReceipt = value;
    }

    /// <summary>
    /// A value of UndistributedOnly.
    /// </summary>
    [JsonPropertyName("undistributedOnly")]
    public Common UndistributedOnly
    {
      get => undistributedOnly ??= new();
      set => undistributedOnly = value;
    }

    /// <summary>
    /// A value of UserCashReceipt.
    /// </summary>
    [JsonPropertyName("userCashReceipt")]
    public CashReceipt UserCashReceipt
    {
      get => userCashReceipt ??= new();
      set => userCashReceipt = value;
    }

    /// <summary>
    /// A value of UserInputToCashReceipt.
    /// </summary>
    [JsonPropertyName("userInputToCashReceipt")]
    public CashReceipt UserInputToCashReceipt
    {
      get => userInputToCashReceipt ??= new();
      set => userInputToCashReceipt = value;
    }

    /// <summary>
    /// A value of UserCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("userCashReceiptSourceType")]
    public CashReceiptSourceType UserCashReceiptSourceType
    {
      get => userCashReceiptSourceType ??= new();
      set => userCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of FilterCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("filterCashReceiptDetail")]
    public CashReceiptDetail FilterCashReceiptDetail
    {
      get => filterCashReceiptDetail ??= new();
      set => filterCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of FilterCollectionType.
    /// </summary>
    [JsonPropertyName("filterCollectionType")]
    public CollectionType FilterCollectionType
    {
      get => filterCollectionType ??= new();
      set => filterCollectionType = value;
    }

    private CashReceiptEvent userInputFrom;
    private CashReceiptEvent userInputToCashReceiptEvent;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail startingCashReceiptDetail;
    private CashReceipt startingCashReceipt;
    private Common undistributedOnly;
    private CashReceipt userCashReceipt;
    private CashReceipt userInputToCashReceipt;
    private CashReceiptSourceType userCashReceiptSourceType;
    private CashReceiptDetail filterCashReceiptDetail;
    private CollectionType filterCollectionType;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of DetailCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("detailCrdCrComboNo")]
      public CrdCrComboNo DetailCrdCrComboNo
      {
        get => detailCrdCrComboNo ??= new();
        set => detailCrdCrComboNo = value;
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
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
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
      /// A value of UndistributedAmt.
      /// </summary>
      [JsonPropertyName("undistributedAmt")]
      public Common UndistributedAmt
      {
        get => undistributedAmt ??= new();
        set => undistributedAmt = value;
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
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public CollectionType CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private Common common;
      private CrdCrComboNo detailCrdCrComboNo;
      private CashReceiptType detailCashReceiptType;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceipt cashReceipt;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceiptDetail cashReceiptDetail;
      private Common undistributedAmt;
      private CashReceiptDetailStatus cashReceiptDetailStatus;
      private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
      private CollectionType collectionType;
    }

    /// <summary>
    /// A value of UserInputFrom.
    /// </summary>
    [JsonPropertyName("userInputFrom")]
    public CashReceiptEvent UserInputFrom
    {
      get => userInputFrom ??= new();
      set => userInputFrom = value;
    }

    /// <summary>
    /// A value of UserInputTo.
    /// </summary>
    [JsonPropertyName("userInputTo")]
    public CashReceiptEvent UserInputTo
    {
      get => userInputTo ??= new();
      set => userInputTo = value;
    }

    /// <summary>
    /// A value of ReleasedForDistribution.
    /// </summary>
    [JsonPropertyName("releasedForDistribution")]
    public Common ReleasedForDistribution
    {
      get => releasedForDistribution ??= new();
      set => releasedForDistribution = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public CashReceipt To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public CashReceipt From
    {
      get => from ??= new();
      set => from = value;
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
    /// A value of PreviousLastCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("previousLastCashReceiptDetail")]
    public CashReceiptDetail PreviousLastCashReceiptDetail
    {
      get => previousLastCashReceiptDetail ??= new();
      set => previousLastCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PreviousLastCashReceipt.
    /// </summary>
    [JsonPropertyName("previousLastCashReceipt")]
    public CashReceipt PreviousLastCashReceipt
    {
      get => previousLastCashReceipt ??= new();
      set => previousLastCashReceipt = value;
    }

    private CashReceiptEvent userInputFrom;
    private CashReceiptEvent userInputTo;
    private Common releasedForDistribution;
    private CashReceipt to;
    private CashReceipt from;
    private Array<ExportGroup> export1;
    private CashReceiptDetail previousLastCashReceiptDetail;
    private CashReceipt previousLastCashReceipt;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
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
      /// A value of CashReceiptType.
      /// </summary>
      [JsonPropertyName("cashReceiptType")]
      public CashReceiptType CashReceiptType
      {
        get => cashReceiptType ??= new();
        set => cashReceiptType = value;
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
      /// A value of UndistributedAmt.
      /// </summary>
      [JsonPropertyName("undistributedAmt")]
      public Common UndistributedAmt
      {
        get => undistributedAmt ??= new();
        set => undistributedAmt = value;
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
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public CollectionType CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private Common common;
      private CashReceipt cashReceipt;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceiptDetail cashReceiptDetail;
      private CashReceiptType cashReceiptType;
      private CashReceiptEvent cashReceiptEvent;
      private Common undistributedAmt;
      private CashReceiptDetailStatus cashReceiptDetailStatus;
      private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
      private CollectionType collectionType;
    }

    /// <summary>
    /// A value of Net.
    /// </summary>
    [JsonPropertyName("net")]
    public CashReceiptDetail Net
    {
      get => net ??= new();
      set => net = value;
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
    /// A value of UndistAmount.
    /// </summary>
    [JsonPropertyName("undistAmount")]
    public Common UndistAmount
    {
      get => undistAmount ??= new();
      set => undistAmount = value;
    }

    /// <summary>
    /// A value of InputTo.
    /// </summary>
    [JsonPropertyName("inputTo")]
    public CashReceipt InputTo
    {
      get => inputTo ??= new();
      set => inputTo = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

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
    /// A value of DataOkay.
    /// </summary>
    [JsonPropertyName("dataOkay")]
    public Common DataOkay
    {
      get => dataOkay ??= new();
      set => dataOkay = value;
    }

    /// <summary>
    /// A value of FirstimeCr.
    /// </summary>
    [JsonPropertyName("firstimeCr")]
    public Common FirstimeCr
    {
      get => firstimeCr ??= new();
      set => firstimeCr = value;
    }

    /// <summary>
    /// A value of FirstimeCrd.
    /// </summary>
    [JsonPropertyName("firstimeCrd")]
    public Common FirstimeCrd
    {
      get => firstimeCrd ??= new();
      set => firstimeCrd = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
    }

    /// <summary>
    /// A value of Pended.
    /// </summary>
    [JsonPropertyName("pended")]
    public CashReceiptDetailStatus Pended
    {
      get => pended ??= new();
      set => pended = value;
    }

    /// <summary>
    /// A value of Input.
    /// </summary>
    [JsonPropertyName("input")]
    public CashReceipt Input
    {
      get => input ??= new();
      set => input = value;
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
    /// A value of MaxDiscontinue.
    /// </summary>
    [JsonPropertyName("maxDiscontinue")]
    public DateWorkArea MaxDiscontinue
    {
      get => maxDiscontinue ??= new();
      set => maxDiscontinue = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public CollectionType Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of UserInputFrom.
    /// </summary>
    [JsonPropertyName("userInputFrom")]
    public CashReceiptEvent UserInputFrom
    {
      get => userInputFrom ??= new();
      set => userInputFrom = value;
    }

    /// <summary>
    /// A value of UserInputTo.
    /// </summary>
    [JsonPropertyName("userInputTo")]
    public CashReceiptEvent UserInputTo
    {
      get => userInputTo ??= new();
      set => userInputTo = value;
    }

    private CashReceiptDetail net;
    private DateWorkArea current;
    private Common undistAmount;
    private CashReceipt inputTo;
    private Array<LocalGroup> local1;
    private Common dataOkay;
    private Common firstimeCr;
    private Common firstimeCrd;
    private CashReceiptDetailStatus suspended;
    private CashReceiptDetailStatus pended;
    private CashReceipt input;
    private DateWorkArea null1;
    private DateWorkArea maxDiscontinue;
    private CollectionType save;
    private CashReceiptEvent userInputFrom;
    private CashReceiptEvent userInputTo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingDetailAdj.
    /// </summary>
    [JsonPropertyName("existingDetailAdj")]
    public CashReceiptDetail ExistingDetailAdj
    {
      get => existingDetailAdj ??= new();
      set => existingDetailAdj = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of IdOnlyCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("idOnlyCashReceiptEvent")]
    public CashReceiptEvent IdOnlyCashReceiptEvent
    {
      get => idOnlyCashReceiptEvent ??= new();
      set => idOnlyCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of IdOnlyCashReceiptType.
    /// </summary>
    [JsonPropertyName("idOnlyCashReceiptType")]
    public CashReceiptType IdOnlyCashReceiptType
    {
      get => idOnlyCashReceiptType ??= new();
      set => idOnlyCashReceiptType = value;
    }

    /// <summary>
    /// A value of IdOnlyCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("idOnlyCashReceiptSourceType")]
    public CashReceiptSourceType IdOnlyCashReceiptSourceType
    {
      get => idOnlyCashReceiptSourceType ??= new();
      set => idOnlyCashReceiptSourceType = value;
    }

    private CashReceiptDetail existingDetailAdj;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptSourceType cashReceiptSourceType;
    private CollectionType collectionType;
    private CashReceiptEvent idOnlyCashReceiptEvent;
    private CashReceiptType idOnlyCashReceiptType;
    private CashReceiptSourceType idOnlyCashReceiptSourceType;
  }
#endregion
}
