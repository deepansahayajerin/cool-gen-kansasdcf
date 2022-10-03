// Program: FN_CAB_LIST_CASH_RECEIPT, ID: 371722704, model: 746.
// Short name: SWE00297
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
/// A program: FN_CAB_LIST_CASH_RECEIPT.
/// </para>
/// <para>
/// RESP: FINANCE
/// This common action block is used to display lists of CASH_RECEIPTs with 
/// varying selection criteria.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabListCashReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_LIST_CASH_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabListCashReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabListCashReceipt.
  /// </summary>
  public FnCabListCashReceipt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******
    // common action for list cash receipts
    // Developed by MTW Consulting for KESSEP
    //      D.M. Nilsen 06/27/95
    // ******
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (!IsEmpty(import.CashReceiptStatus.Code))
    {
      if (!ReadCashReceiptStatus())
      {
        ExitState = "FN0108_CASH_RCPT_STAT_NF";

        return;
      }
    }

    if (!IsEmpty(import.CashReceiptSourceType.Code))
    {
      if (!ReadCashReceiptSourceType1())
      {
        ExitState = "FN0000_CASH_RCPT_SOURCE_TYPE_NF";

        return;
      }
    }

    if (Equal(import.From.Date, null))
    {
      local.FromDateWorkArea.Date = new DateTime(1, 1, 1);
    }
    else
    {
      local.FromDateWorkArea.Date = import.From.Date;
    }

    if (Equal(import.Thru.Date, null))
    {
      export.Thru.Date = Now().Date;
    }
    else
    {
      export.Thru.Date = import.Thru.Date;
    }

    local.Current.Date = Now().Date;

    if (IsEmpty(import.CashReceiptSourceType.Code))
    {
      if (IsEmpty(import.CashReceipt.CreatedBy))
      {
        if (IsEmpty(import.CashReceiptStatus.Code))
        {
          // ******
          // No criteria has been entered in From thru date and time
          // ******
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptCashReceiptEventCashReceiptType4())
          {
            UseFnReadCashRcptStatus();

            // *****************************************************************
            // Calculate Screen total and pass it back to CREL
            // Set cash receipt detail satus code flag and pass it back to CREL
            // *****************************************************************
            local.Detail.Flag = "Y";
            local.Common.Count = 0;

            foreach(var item1 in ReadCashReceiptDetailStatus())
            {
              ++local.Common.Count;

              if (Equal(entities.CashReceiptDetailStatus.Code, "BAL") || Equal
                (entities.CashReceiptDetailStatus.Code, "REC") || Equal
                (entities.CashReceiptDetailStatus.Code, "DEP"))
              {
                local.Detail.Flag = "N";

                break;
              }
            }

            if (local.Common.Count == 0)
            {
              local.Detail.Flag = "*";
            }

            ReadCashReceiptSourceType2();

            // ********************************************************************
            // The following set of code will check the Cash Receipt Status code
            // to
            // see if it is a Deposit.  If it is a Deposit, the code will read 
            // for
            // the current Fund Transaction Status History and check to see if 
            // the
            // current Status is opened or closed.
            // ********************************************************************
            local.CashReceiptStatus.Code =
              entities.ExistingCashReceiptStatus.Code;

            if (Equal(entities.ExistingCashReceiptStatus.Code, "DEP"))
            {
              if (ReadFundTransactionStatusHistoryFundTransactionStatus())
              {
                if (Equal(entities.FundTransactionStatus.Code, "OPEN"))
                {
                  local.CashReceiptStatus.Code = "DEP O";
                }
                else if (Equal(entities.FundTransactionStatus.Code, "CLOSED"))
                {
                  local.CashReceiptStatus.Code = "DEP C";
                }
                else
                {
                  ExitState = "ACO_NE0000_INVALID_CODE";
                  export.Export1.Next();

                  return;
                }
              }

              if (!Equal(local.CashReceiptStatus.Code, "DEP O") && !
                Equal(local.CashReceiptStatus.Code, "DEP C"))
              {
                ExitState = "FN0000_FUND_TRANS_STAT_HIS_NF";
                export.Export1.Next();

                return;
              }
            }

            MoveCommon(local.Detail, export.Export1.Update.DetailCommon);
            MoveCashReceipt(entities.ExistingCashReceipt,
              export.Export1.Update.DetailCashReceipt);
            export.Export1.Update.DetailHiddenCashReceiptEvent.
              SystemGeneratedIdentifier =
                entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
            MoveCashReceiptType(entities.ExistingCashReceiptType,
              export.Export1.Update.DetailHiddenCashReceiptType);
            MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
              export.Export1.Update.DetailCashReceiptSourceType);
            export.Export1.Update.DetailCashReceiptStatus.Code =
              local.CashReceiptStatus.Code;
            export.TotalScreenAmount.TotalCurrency += export.Export1.Item.
              DetailCashReceipt.ReceiptAmount;
            export.Export1.Next();
          }
        }
        else
        {
          // ******
          // Cash receipt status and or thru date and time entered
          // ******
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptCashReceiptEventCashReceiptType4())
          {
            if (!ReadCashReceiptSourceType2())
            {
              ExitState = "FN0000_CASH_RCPT_SOURCE_TYPE_NF";
              export.Export1.Next();

              return;
            }

            // *****************************************************************
            // Calculate Screen total and pass it back to CREL
            // Set cash receipt detail satus code flag and pass it back to CREL
            // *****************************************************************
            local.Detail.Flag = "Y";
            local.Common.Count = 0;

            foreach(var item1 in ReadCashReceiptDetailStatus())
            {
              ++local.Common.Count;

              if (Equal(entities.CashReceiptDetailStatus.Code, "BAL") || Equal
                (entities.CashReceiptDetailStatus.Code, "REC") || Equal
                (entities.CashReceiptDetailStatus.Code, "DEP"))
              {
                local.Detail.Flag = "N";

                break;
              }
            }

            if (local.Common.Count == 0)
            {
              local.Detail.Flag = "*";
            }

            // ******
            // now read cash receipt status history for current status and 
            // current cash receipt
            // ******
            if (ReadCashReceiptStatusHistory2())
            {
              // ********************************************************************
              // The following set of code will check the Cash Receipt Status 
              // code to
              // see if it is a Deposit.  If it is a Deposit, the code will read
              // for
              // the current Fund Transaction Status History and check to see if
              // the
              // current Status is opened or closed.
              // ********************************************************************
              local.CashReceiptStatus.Code =
                entities.ExistingCashReceiptStatus.Code;

              if (Equal(entities.ExistingCashReceiptStatus.Code, "DEP"))
              {
                if (ReadFundTransactionStatusHistoryFundTransactionStatus())
                {
                  if (Equal(entities.FundTransactionStatus.Code, "OPEN"))
                  {
                    local.CashReceiptStatus.Code = "DEP O";
                  }
                  else if (Equal(entities.FundTransactionStatus.Code, "CLOSED"))
                  {
                    local.CashReceiptStatus.Code = "DEP C";
                  }
                  else
                  {
                    ExitState = "ACO_NE0000_INVALID_CODE";
                    export.Export1.Next();

                    return;
                  }
                }

                if (!Equal(local.CashReceiptStatus.Code, "DEP O") && !
                  Equal(local.CashReceiptStatus.Code, "DEP C"))
                {
                  ExitState = "FN0000_FUND_TRANS_STAT_HIS_NF";
                  export.Export1.Next();

                  return;
                }
              }

              MoveCashReceipt(entities.ExistingCashReceipt,
                export.Export1.Update.DetailCashReceipt);
              MoveCommon(local.Detail, export.Export1.Update.DetailCommon);
              export.Export1.Update.DetailHiddenCashReceiptEvent.
                SystemGeneratedIdentifier =
                  entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
              MoveCashReceiptType(entities.ExistingCashReceiptType,
                export.Export1.Update.DetailHiddenCashReceiptType);
              MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
                export.Export1.Update.DetailCashReceiptSourceType);
              export.Export1.Update.DetailCashReceiptStatus.Code =
                local.CashReceiptStatus.Code;
              export.TotalScreenAmount.TotalCurrency += export.Export1.Item.
                DetailCashReceipt.ReceiptAmount;
            }
            else
            {
              // *****
              // Nothing to store for this receipt
              // *****
            }

            export.Export1.Next();
          }
        }
      }
      else
      {
        // ******
        // Created by not = spaces
        // ******
        if (IsEmpty(import.CashReceiptStatus.Code))
        {
          // ******
          // Created by only entered
          // ******
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptCashReceiptEventCashReceiptType2())
          {
            UseFnReadCashRcptStatus();

            // *****************************************************************
            // Calculate Screen total and pass it back to CREL
            // Set cash receipt detail satus code flag and pass it back to CREL
            // *****************************************************************
            local.Detail.Flag = "Y";
            local.Common.Count = 0;

            foreach(var item1 in ReadCashReceiptDetailStatus())
            {
              ++local.Common.Count;

              if (Equal(entities.CashReceiptDetailStatus.Code, "BAL") || Equal
                (entities.CashReceiptDetailStatus.Code, "REC") || Equal
                (entities.CashReceiptDetailStatus.Code, "DEP"))
              {
                local.Detail.Flag = "N";

                break;
              }
            }

            if (local.Common.Count == 0)
            {
              local.Detail.Flag = "*";
            }

            ReadCashReceiptSourceType2();

            // ********************************************************************
            // The following set of code will check the Cash Receipt Status code
            // to
            // see if it is a Deposit.  If it is a Deposit, the code will read 
            // for
            // the current Fund Transaction Status History and check to see if 
            // the
            // current Status is opened or closed.
            // ********************************************************************
            local.CashReceiptStatus.Code =
              entities.ExistingCashReceiptStatus.Code;

            if (Equal(entities.ExistingCashReceiptStatus.Code, "DEP"))
            {
              if (ReadFundTransactionStatusHistoryFundTransactionStatus())
              {
                if (Equal(entities.FundTransactionStatus.Code, "OPEN"))
                {
                  local.CashReceiptStatus.Code = "DEP O";
                }
                else if (Equal(entities.FundTransactionStatus.Code, "CLOSED"))
                {
                  local.CashReceiptStatus.Code = "DEP C";
                }
                else
                {
                  ExitState = "ACO_NE0000_INVALID_CODE";
                  export.Export1.Next();

                  return;
                }
              }

              if (!Equal(local.CashReceiptStatus.Code, "DEP O") && !
                Equal(local.CashReceiptStatus.Code, "DEP C"))
              {
                ExitState = "FN0000_FUND_TRANS_STAT_HIS_NF";
                export.Export1.Next();

                return;
              }
            }

            MoveCommon(local.Detail, export.Export1.Update.DetailCommon);
            MoveCashReceipt(entities.ExistingCashReceipt,
              export.Export1.Update.DetailCashReceipt);
            export.Export1.Update.DetailHiddenCashReceiptEvent.
              SystemGeneratedIdentifier =
                entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
            MoveCashReceiptType(entities.ExistingCashReceiptType,
              export.Export1.Update.DetailHiddenCashReceiptType);
            MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
              export.Export1.Update.DetailCashReceiptSourceType);
            export.Export1.Update.DetailCashReceiptStatus.Code =
              local.CashReceiptStatus.Code;
            export.TotalScreenAmount.TotalCurrency += export.Export1.Item.
              DetailCashReceipt.ReceiptAmount;
            export.Export1.Next();
          }
        }
        else
        {
          // ******
          // cash receipt status and
          // created by and
          // from and thru date and time have been entered
          // ******
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptCashReceiptEventCashReceiptType2())
          {
            ReadCashReceiptSourceType2();

            // *****************************************************************
            // Calculate Screen total and pass it back to CREL
            // Set cash receipt detail satus code flag and pass it back to CREL
            // *****************************************************************
            local.Detail.Flag = "Y";
            local.Common.Count = 0;

            foreach(var item1 in ReadCashReceiptDetailStatus())
            {
              ++local.Common.Count;

              if (Equal(entities.CashReceiptDetailStatus.Code, "BAL") || Equal
                (entities.CashReceiptDetailStatus.Code, "REC") || Equal
                (entities.CashReceiptDetailStatus.Code, "DEP"))
              {
                local.Detail.Flag = "N";

                break;
              }
            }

            if (local.Common.Count == 0)
            {
              local.Detail.Flag = "*";
            }

            // ******
            // now read cash receipt status history for current status and 
            // current cash receipt
            // ******
            if (ReadCashReceiptStatusHistory2())
            {
              // ********************************************************************
              // The following set of code will check the Cash Receipt Status 
              // code to
              // see if it is a Deposit.  If it is a Deposit, the code will read
              // for
              // the current Fund Transaction Status History and check to see if
              // the
              // current Status is opened or closed.
              // ********************************************************************
              local.CashReceiptStatus.Code =
                entities.ExistingCashReceiptStatus.Code;

              if (Equal(entities.ExistingCashReceiptStatus.Code, "DEP"))
              {
                if (ReadFundTransactionStatusHistoryFundTransactionStatus())
                {
                  if (Equal(entities.FundTransactionStatus.Code, "OPEN"))
                  {
                    local.CashReceiptStatus.Code = "DEP O";
                  }
                  else if (Equal(entities.FundTransactionStatus.Code, "CLOSED"))
                  {
                    local.CashReceiptStatus.Code = "DEP C";
                  }
                  else
                  {
                    ExitState = "ACO_NE0000_INVALID_CODE";
                    export.Export1.Next();

                    return;
                  }
                }

                if (!Equal(local.CashReceiptStatus.Code, "DEP O") && !
                  Equal(local.CashReceiptStatus.Code, "DEP C"))
                {
                  ExitState = "FN0000_FUND_TRANS_STAT_HIS_NF";
                  export.Export1.Next();

                  return;
                }
              }

              MoveCommon(local.Detail, export.Export1.Update.DetailCommon);
              MoveCashReceipt(entities.ExistingCashReceipt,
                export.Export1.Update.DetailCashReceipt);
              export.Export1.Update.DetailHiddenCashReceiptEvent.
                SystemGeneratedIdentifier =
                  entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
              MoveCashReceiptType(entities.ExistingCashReceiptType,
                export.Export1.Update.DetailHiddenCashReceiptType);
              MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
                export.Export1.Update.DetailCashReceiptSourceType);
              export.Export1.Update.DetailCashReceiptStatus.Code =
                local.CashReceiptStatus.Code;
              export.TotalScreenAmount.TotalCurrency += export.Export1.Item.
                DetailCashReceipt.ReceiptAmount;
            }
            else
            {
              // *****
              // Nothing to store for this receipt
              // *****
            }

            export.Export1.Next();
          }
        }
      }
    }
    else
    {
      // ******
      // Cash receipt source type not equal spaces
      // ******
      if (IsEmpty(import.CashReceipt.CreatedBy))
      {
        if (IsEmpty(import.CashReceiptStatus.Code))
        {
          // ******
          // Cash receipt source type entered only
          // ******
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptCashReceiptEventCashReceiptType3())
          {
            UseFnReadCashRcptStatus();

            // *****************************************************************
            // Calculate Screen total and pass it back to CREL
            // Set cash receipt detail satus code flag and pass it back to CREL
            // *****************************************************************
            local.Detail.Flag = "Y";
            local.Common.Count = 0;

            foreach(var item1 in ReadCashReceiptDetailStatus())
            {
              ++local.Common.Count;

              if (Equal(entities.CashReceiptDetailStatus.Code, "BAL") || Equal
                (entities.CashReceiptDetailStatus.Code, "REC") || Equal
                (entities.CashReceiptDetailStatus.Code, "DEP"))
              {
                local.Detail.Flag = "N";

                break;
              }
            }

            if (local.Common.Count == 0)
            {
              local.Detail.Flag = "*";
            }

            // ********************************************************************
            // The following set of code will check the Cash Receipt Status code
            // to
            // see if it is a Deposit.  If it is a Deposit, the code will read 
            // for
            // the current Fund Transaction Status History and check to see if 
            // the
            // current Status is opened or closed.
            // ********************************************************************
            local.CashReceiptStatus.Code =
              entities.ExistingCashReceiptStatus.Code;

            if (Equal(entities.ExistingCashReceiptStatus.Code, "DEP"))
            {
              if (ReadFundTransactionStatusHistoryFundTransactionStatus())
              {
                if (Equal(entities.FundTransactionStatus.Code, "OPEN"))
                {
                  local.CashReceiptStatus.Code = "DEP O";
                }
                else if (Equal(entities.FundTransactionStatus.Code, "CLOSED"))
                {
                  local.CashReceiptStatus.Code = "DEP C";
                }
                else
                {
                  ExitState = "ACO_NE0000_INVALID_CODE";
                  export.Export1.Next();

                  return;
                }
              }

              if (!Equal(local.CashReceiptStatus.Code, "DEP O") && !
                Equal(local.CashReceiptStatus.Code, "DEP C"))
              {
                ExitState = "FN0000_FUND_TRANS_STAT_HIS_NF";
                export.Export1.Next();

                return;
              }
            }

            MoveCommon(local.Detail, export.Export1.Update.DetailCommon);
            MoveCashReceipt(entities.ExistingCashReceipt,
              export.Export1.Update.DetailCashReceipt);
            export.Export1.Update.DetailHiddenCashReceiptEvent.
              SystemGeneratedIdentifier =
                entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
            MoveCashReceiptType(entities.ExistingCashReceiptType,
              export.Export1.Update.DetailHiddenCashReceiptType);
            MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
              export.Export1.Update.DetailCashReceiptSourceType);
            export.Export1.Update.DetailCashReceiptStatus.Code =
              local.CashReceiptStatus.Code;
            export.TotalScreenAmount.TotalCurrency += export.Export1.Item.
              DetailCashReceipt.ReceiptAmount;
            export.Export1.Next();
          }
        }
        else
        {
          // ******
          // cash receipt source type code and
          // cash receipt status code entered
          // ******
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadCashReceiptCashReceiptEventCashReceiptType3())
          {
            // *****************************************************************
            // Calculate Screen total and pass it back to CREL
            // Set cash receipt detail satus code flag and pass it back to CREL
            // *****************************************************************
            local.Detail.Flag = "Y";
            local.Common.Count = 0;

            foreach(var item1 in ReadCashReceiptDetailStatus())
            {
              ++local.Common.Count;

              if (Equal(entities.CashReceiptDetailStatus.Code, "BAL") || Equal
                (entities.CashReceiptDetailStatus.Code, "REC") || Equal
                (entities.CashReceiptDetailStatus.Code, "DEP"))
              {
                local.Detail.Flag = "N";

                break;
              }
            }

            if (local.Common.Count == 0)
            {
              local.Detail.Flag = "*";
            }

            if (ReadCashReceiptStatusHistory1())
            {
              // ********************************************************************
              // The following set of code will check the Cash Receipt Status 
              // code to
              // see if it is a Deposit.  If it is a Deposit, the code will read
              // for
              // the current Fund Transaction Status History and check to see if
              // the
              // current Status is opened or closed.
              // ********************************************************************
              local.CashReceiptStatus.Code =
                entities.ExistingCashReceiptStatus.Code;

              if (Equal(entities.ExistingCashReceiptStatus.Code, "DEP"))
              {
                if (ReadFundTransactionStatusHistoryFundTransactionStatus())
                {
                  if (Equal(entities.FundTransactionStatus.Code, "OPEN"))
                  {
                    local.CashReceiptStatus.Code = "DEP O";
                  }
                  else if (Equal(entities.FundTransactionStatus.Code, "CLOSED"))
                  {
                    local.CashReceiptStatus.Code = "DEP C";
                  }
                  else
                  {
                    ExitState = "ACO_NE0000_INVALID_CODE";
                    export.Export1.Next();

                    return;
                  }
                }

                if (!Equal(local.CashReceiptStatus.Code, "DEP O") && !
                  Equal(local.CashReceiptStatus.Code, "DEP C"))
                {
                  ExitState = "FN0000_FUND_TRANS_STAT_HIS_NF";
                  export.Export1.Next();

                  return;
                }
              }

              MoveCommon(local.Detail, export.Export1.Update.DetailCommon);
              MoveCashReceipt(entities.ExistingCashReceipt,
                export.Export1.Update.DetailCashReceipt);
              export.Export1.Update.DetailHiddenCashReceiptEvent.
                SystemGeneratedIdentifier =
                  entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
              MoveCashReceiptType(entities.ExistingCashReceiptType,
                export.Export1.Update.DetailHiddenCashReceiptType);
              MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
                export.Export1.Update.DetailCashReceiptSourceType);
              export.Export1.Update.DetailCashReceiptStatus.Code =
                local.CashReceiptStatus.Code;
              export.TotalScreenAmount.TotalCurrency += export.Export1.Item.
                DetailCashReceipt.ReceiptAmount;
            }
            else
            {
              // ******
              // nothing to store here
              // ******
            }

            export.Export1.Next();
          }
        }
      }
      else if (IsEmpty(import.CashReceiptStatus.Code))
      {
        // ******
        // cash receipt source type code and
        // cash receipt created by entered
        // ******
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCashReceiptCashReceiptEventCashReceiptType1())
        {
          UseFnReadCashRcptStatus();

          // *****************************************************************
          // Calculate Screen total and pass it back to CREL
          // Set cash receipt detail satus code flag and pass it back to CREL
          // *****************************************************************
          local.Detail.Flag = "Y";
          local.Common.Count = 0;

          foreach(var item1 in ReadCashReceiptDetailStatus())
          {
            ++local.Common.Count;

            if (Equal(entities.CashReceiptDetailStatus.Code, "BAL") || Equal
              (entities.CashReceiptDetailStatus.Code, "REC") || Equal
              (entities.CashReceiptDetailStatus.Code, "DEP"))
            {
              local.Detail.Flag = "N";

              break;
            }
          }

          if (local.Common.Count == 0)
          {
            local.Detail.Flag = "*";
          }

          // ********************************************************************
          // The following set of code will check the Cash Receipt Status code 
          // to
          // see if it is a Deposit.  If it is a Deposit, the code will read for
          // the current Fund Transaction Status History and check to see if the
          // current Status is opened or closed.
          // ********************************************************************
          local.CashReceiptStatus.Code =
            entities.ExistingCashReceiptStatus.Code;

          if (Equal(entities.ExistingCashReceiptStatus.Code, "DEP"))
          {
            if (ReadFundTransactionStatusHistoryFundTransactionStatus())
            {
              if (Equal(entities.FundTransactionStatus.Code, "OPEN"))
              {
                local.CashReceiptStatus.Code = "DEP O";
              }
              else if (Equal(entities.FundTransactionStatus.Code, "CLOSED"))
              {
                local.CashReceiptStatus.Code = "DEP C";
              }
              else
              {
                ExitState = "ACO_NE0000_INVALID_CODE";
                export.Export1.Next();

                return;
              }
            }

            if (!Equal(local.CashReceiptStatus.Code, "DEP O") && !
              Equal(local.CashReceiptStatus.Code, "DEP C"))
            {
              ExitState = "FN0000_FUND_TRANS_STAT_HIS_NF";
              export.Export1.Next();

              return;
            }
          }

          MoveCashReceipt(entities.ExistingCashReceipt,
            export.Export1.Update.DetailCashReceipt);
          MoveCommon(local.Detail, export.Export1.Update.DetailCommon);
          export.Export1.Update.DetailHiddenCashReceiptEvent.
            SystemGeneratedIdentifier =
              entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
          MoveCashReceiptType(entities.ExistingCashReceiptType,
            export.Export1.Update.DetailHiddenCashReceiptType);
          MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
            export.Export1.Update.DetailCashReceiptSourceType);
          export.Export1.Update.DetailCashReceiptStatus.Code =
            local.CashReceiptStatus.Code;
          export.TotalScreenAmount.TotalCurrency += export.Export1.Item.
            DetailCashReceipt.ReceiptAmount;
          export.Export1.Next();
        }
      }
      else
      {
        // ******
        // cash receipt created by and
        // cash receipt source type code and
        // cash receipt status code entered
        // ******
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCashReceiptCashReceiptEventCashReceiptType1())
        {
          // *****************************************************************
          // Calculate Screen total and pass it back to CREL
          // Set cash receipt detail satus code flag and pass it back to CREL
          // *****************************************************************
          local.Detail.Flag = "Y";
          local.Common.Count = 0;

          foreach(var item1 in ReadCashReceiptDetailStatus())
          {
            ++local.Common.Count;

            if (Equal(entities.CashReceiptDetailStatus.Code, "BAL") || Equal
              (entities.CashReceiptDetailStatus.Code, "REC") || Equal
              (entities.CashReceiptDetailStatus.Code, "DEP"))
            {
              local.Detail.Flag = "N";

              break;
            }
          }

          if (local.Common.Count == 0)
          {
            local.Detail.Flag = "*";
          }

          if (ReadCashReceiptStatusHistory1())
          {
            // ********************************************************************
            // The following set of code will check the Cash Receipt Status code
            // to
            // see if it is a Deposit.  If it is a Deposit, the code will read 
            // for
            // the current Fund Transaction Status History and check to see if 
            // the
            // current Status is opened or closed.
            // ********************************************************************
            local.CashReceiptStatus.Code =
              entities.ExistingCashReceiptStatus.Code;

            if (Equal(entities.ExistingCashReceiptStatus.Code, "DEP"))
            {
              if (ReadFundTransactionStatusHistoryFundTransactionStatus())
              {
                if (Equal(entities.FundTransactionStatus.Code, "OPEN"))
                {
                  local.CashReceiptStatus.Code = "DEP O";
                }
                else if (Equal(entities.FundTransactionStatus.Code, "CLOSED"))
                {
                  local.CashReceiptStatus.Code = "DEP C";
                }
                else
                {
                  ExitState = "ACO_NE0000_INVALID_CODE";
                  export.Export1.Next();

                  return;
                }
              }

              if (!Equal(local.CashReceiptStatus.Code, "DEP O") && !
                Equal(local.CashReceiptStatus.Code, "DEP C"))
              {
                ExitState = "FN0000_FUND_TRANS_STAT_HIS_NF";
                export.Export1.Next();

                return;
              }
            }

            MoveCommon(local.Detail, export.Export1.Update.DetailCommon);
            MoveCashReceipt(entities.ExistingCashReceipt,
              export.Export1.Update.DetailCashReceipt);
            export.Export1.Update.DetailHiddenCashReceiptEvent.
              SystemGeneratedIdentifier =
                entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier;
            MoveCashReceiptType(entities.ExistingCashReceiptType,
              export.Export1.Update.DetailHiddenCashReceiptType);
            MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
              export.Export1.Update.DetailCashReceiptSourceType);
            export.Export1.Update.DetailCashReceiptStatus.Code =
              local.CashReceiptStatus.Code;
            export.TotalScreenAmount.TotalCurrency += export.Export1.Item.
              DetailCashReceipt.ReceiptAmount;
          }
          else
          {
            // ******
            // nothing to store here
            // ******
          }

          export.Export1.Next();
        }
      }
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
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

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnReadCashRcptStatus()
  {
    var useImport = new FnReadCashRcptStatus.Import();
    var useExport = new FnReadCashRcptStatus.Export();

    useImport.CashReceipt.Assign(entities.ExistingCashReceipt);

    Call(FnReadCashRcptStatus.Execute, useImport, useExport);

    MoveCashReceiptStatus(useExport.CashReceiptStatus,
      entities.ExistingCashReceiptStatus);
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptEventCashReceiptType1()
  {
    return ReadEach("ReadCashReceiptCashReceiptEventCashReceiptType1",
      (db, command) =>
      {
        db.SetString(command, "createdBy", import.CashReceipt.CreatedBy);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "date1", local.FromDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.Thru.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "checkType", import.CashReceipt.CheckType ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.ExistingCashReceipt.CheckType =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceipt.CheckNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 8);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingCashReceipt.FttIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingCashReceipt.PcaCode = db.GetNullableString(reader, 11);
        entities.ExistingCashReceipt.PcaEffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCashReceipt.FunIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.ExistingCashReceipt.FtrIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.ExistingCashReceiptType.Code = db.GetString(reader, 15);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptEvent.Populated = true;
        entities.ExistingCashReceiptType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptEventCashReceiptType2()
  {
    return ReadEach("ReadCashReceiptCashReceiptEventCashReceiptType2",
      (db, command) =>
      {
        db.SetString(command, "createdBy", import.CashReceipt.CreatedBy);
        db.SetDate(
          command, "date1", local.FromDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.Thru.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "checkType", import.CashReceipt.CheckType ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.ExistingCashReceipt.CheckType =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceipt.CheckNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 8);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingCashReceipt.FttIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingCashReceipt.PcaCode = db.GetNullableString(reader, 11);
        entities.ExistingCashReceipt.PcaEffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCashReceipt.FunIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.ExistingCashReceipt.FtrIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.ExistingCashReceiptType.Code = db.GetString(reader, 15);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptEvent.Populated = true;
        entities.ExistingCashReceiptType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptEventCashReceiptType3()
  {
    return ReadEach("ReadCashReceiptCashReceiptEventCashReceiptType3",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "date1", local.FromDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.Thru.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "checkType", import.CashReceipt.CheckType ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.ExistingCashReceipt.CheckType =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceipt.CheckNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 8);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingCashReceipt.FttIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingCashReceipt.PcaCode = db.GetNullableString(reader, 11);
        entities.ExistingCashReceipt.PcaEffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCashReceipt.FunIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.ExistingCashReceipt.FtrIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.ExistingCashReceiptType.Code = db.GetString(reader, 15);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptEvent.Populated = true;
        entities.ExistingCashReceiptType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptEventCashReceiptType4()
  {
    return ReadEach("ReadCashReceiptCashReceiptEventCashReceiptType4",
      (db, command) =>
      {
        db.SetDate(
          command, "date1", local.FromDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.Thru.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "checkType", import.CashReceipt.CheckType ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.ExistingCashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.ExistingCashReceipt.CheckType =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceipt.CheckNumber =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceipt.CreatedBy = db.GetString(reader, 8);
        entities.ExistingCashReceipt.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingCashReceipt.FttIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingCashReceipt.PcaCode = db.GetNullableString(reader, 11);
        entities.ExistingCashReceipt.PcaEffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCashReceipt.FunIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.ExistingCashReceipt.FtrIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.ExistingCashReceiptType.Code = db.GetString(reader, 15);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptEvent.Populated = true;
        entities.ExistingCashReceiptType.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.CashReceiptDetailStatus.Populated = false;

    return ReadEach("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CashReceiptDetailStatus.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptSourceType1()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptSourceType.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptEvent.Populated);
    entities.ExistingCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          entities.ExistingCashReceiptEvent.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.ExistingCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptStatus.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptStatus.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.ExistingCashReceipt.CrtIdentifier);
          
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetInt32(
          command, "crvIdentifier", entities.ExistingCashReceipt.CrvIdentifier);
          
        db.SetInt32(
          command, "crsIdentifier",
          entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceiptStatusHistory.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crsIdentifier",
          entities.ExistingCashReceiptStatus.SystemGeneratedIdentifier);
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
        entities.ExistingCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ExistingCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingCashReceiptStatusHistory.Populated = true;
      });
  }

  private bool ReadFundTransactionStatusHistoryFundTransactionStatus()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.FundTransactionStatus.Populated = false;
    entities.FundTransactionStatusHistory.Populated = false;

    return Read("ReadFundTransactionStatusHistoryFundTransactionStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "ftrIdentifier",
          entities.ExistingCashReceipt.FtrIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "funIdentifier",
          entities.ExistingCashReceipt.FunIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "fttIdentifier",
          entities.ExistingCashReceipt.FttIdentifier.GetValueOrDefault());
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.ExistingCashReceipt.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "pcaCode", entities.ExistingCashReceipt.PcaCode ?? "");
      },
      (db, reader) =>
      {
        entities.FundTransactionStatusHistory.FtrIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatusHistory.FunIdentifier =
          db.GetInt32(reader, 1);
        entities.FundTransactionStatusHistory.PcaEffectiveDate =
          db.GetDate(reader, 2);
        entities.FundTransactionStatusHistory.PcaCode = db.GetString(reader, 3);
        entities.FundTransactionStatusHistory.FttIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransactionStatusHistory.FtsIdentifier =
          db.GetInt32(reader, 5);
        entities.FundTransactionStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.FundTransactionStatusHistory.EffectiveTmst =
          db.GetDateTime(reader, 6);
        entities.FundTransactionStatus.Code = db.GetString(reader, 7);
        entities.FundTransactionStatus.Populated = true;
        entities.FundTransactionStatusHistory.Populated = true;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CashReceipt Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
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
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of Thru.
    /// </summary>
    [JsonPropertyName("thru")]
    public DateWorkArea Thru
    {
      get => thru ??= new();
      set => thru = value;
    }

    private CashReceipt starting;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptSourceType cashReceiptSourceType;
    private DateWorkArea from;
    private DateWorkArea thru;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptSourceType")]
      public CashReceiptSourceType DetailCashReceiptSourceType
      {
        get => detailCashReceiptSourceType ??= new();
        set => detailCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of DetailCashReceipt.
      /// </summary>
      [JsonPropertyName("detailCashReceipt")]
      public CashReceipt DetailCashReceipt
      {
        get => detailCashReceipt ??= new();
        set => detailCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptStatus.
      /// </summary>
      [JsonPropertyName("detailCashReceiptStatus")]
      public CashReceiptStatus DetailCashReceiptStatus
      {
        get => detailCashReceiptStatus ??= new();
        set => detailCashReceiptStatus = value;
      }

      /// <summary>
      /// A value of DetailHiddenCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailHiddenCashReceiptEvent")]
      public CashReceiptEvent DetailHiddenCashReceiptEvent
      {
        get => detailHiddenCashReceiptEvent ??= new();
        set => detailHiddenCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailHiddenCashReceiptType.
      /// </summary>
      [JsonPropertyName("detailHiddenCashReceiptType")]
      public CashReceiptType DetailHiddenCashReceiptType
      {
        get => detailHiddenCashReceiptType ??= new();
        set => detailHiddenCashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private Common detailCommon;
      private CashReceiptSourceType detailCashReceiptSourceType;
      private CashReceipt detailCashReceipt;
      private CashReceiptStatus detailCashReceiptStatus;
      private CashReceiptEvent detailHiddenCashReceiptEvent;
      private CashReceiptType detailHiddenCashReceiptType;
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
    /// A value of Thru.
    /// </summary>
    [JsonPropertyName("thru")]
    public DateWorkArea Thru
    {
      get => thru ??= new();
      set => thru = value;
    }

    /// <summary>
    /// A value of TotalScreenAmount.
    /// </summary>
    [JsonPropertyName("totalScreenAmount")]
    public Common TotalScreenAmount
    {
      get => totalScreenAmount ??= new();
      set => totalScreenAmount = value;
    }

    private Array<ExportGroup> export1;
    private DateWorkArea thru;
    private Common totalScreenAmount;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Detail.
    /// </summary>
    [JsonPropertyName("detail")]
    public Common Detail
    {
      get => detail ??= new();
      set => detail = value;
    }

    /// <summary>
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of FromDateWorkArea.
    /// </summary>
    [JsonPropertyName("fromDateWorkArea")]
    public DateWorkArea FromDateWorkArea
    {
      get => fromDateWorkArea ??= new();
      set => fromDateWorkArea = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of TextDateTime.
    /// </summary>
    [JsonPropertyName("textDateTime")]
    public TimestampWorkArea TextDateTime
    {
      get => textDateTime ??= new();
      set => textDateTime = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public CashReceiptStatus Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Thru.
    /// </summary>
    [JsonPropertyName("thru")]
    public CashReceipt Thru
    {
      get => thru ??= new();
      set => thru = value;
    }

    /// <summary>
    /// A value of FromCashReceipt.
    /// </summary>
    [JsonPropertyName("fromCashReceipt")]
    public CashReceipt FromCashReceipt
    {
      get => fromCashReceipt ??= new();
      set => fromCashReceipt = value;
    }

    private Common detail;
    private CashReceiptStatus cashReceiptStatus;
    private Common common;
    private DateWorkArea null1;
    private DateWorkArea fromDateWorkArea;
    private DateWorkArea current;
    private DateWorkArea max;
    private TimestampWorkArea textDateTime;
    private CashReceiptStatus maximum;
    private CashReceipt thru;
    private CashReceipt fromCashReceipt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FundTransactionStatus.
    /// </summary>
    [JsonPropertyName("fundTransactionStatus")]
    public FundTransactionStatus FundTransactionStatus
    {
      get => fundTransactionStatus ??= new();
      set => fundTransactionStatus = value;
    }

    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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
    /// A value of ExistingCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ExistingCashReceiptStatusHistory
    {
      get => existingCashReceiptStatusHistory ??= new();
      set => existingCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
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
    /// A value of ExistingCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptStatus")]
    public CashReceiptStatus ExistingCashReceiptStatus
    {
      get => existingCashReceiptStatus ??= new();
      set => existingCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptType")]
    public CashReceiptType ExistingCashReceiptType
    {
      get => existingCashReceiptType ??= new();
      set => existingCashReceiptType = value;
    }

    private FundTransactionStatus fundTransactionStatus;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransaction fundTransaction;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptStatusHistory existingCashReceiptStatusHistory;
    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceipt existingCashReceipt;
    private CashReceiptStatus existingCashReceiptStatus;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceiptType existingCashReceiptType;
  }
#endregion
}
