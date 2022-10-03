// Program: FN_FEES_REC_CASH_RCPT_COLL_FEES, ID: 371774267, model: 746.
// Short name: SWEFEESP
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
/// A program: FN_FEES_REC_CASH_RCPT_COLL_FEES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnFeesRecCashRcptCollFees: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_FEES_REC_CASH_RCPT_COLL_FEES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnFeesRecCashRcptCollFees(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnFeesRecCashRcptCollFees.
  /// </summary>
  public FnFeesRecCashRcptCollFees(IContext context, Import import,
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
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ----------------------------------------------------------------------------------------
    // Date 	  	Developer Name		Request #	Description
    // 02/19/96	Holly Kennedy-MTW			Retrofits
    // 02/19/96	Holly Kennedy-MTW			Prompting logic for Code was not not 
    // working.  Added view to group in order to bring the selected value back
    // to the proper occurrence of the group.
    // 04/11/96	Holly Kennedy				When adding a fee the identifier (Timestamp) 
    // was not being populated and passed to the Action Block.  This was causing
    // only one fee type to be allowed per Cash Receipt.
    // 01/03/97	R. Marchman				Add new security/next tran
    // 03/26/98	Siraj Konkader		ZDEL cleanup
    // 10/9/98	Sunya Sharp					Make changes per screen assessment signed 9/18/
    // 98.  Change edit pattern on some fields on screen.  Change exitstates per
    // user request.  Order validation logic.  Add delete pf key and logic for
    // this process.
    // 06/8/99	Sunya Sharp					When user types over an existing fee during the 
    // add process the totals is not calculated correctly.  It allows the user
    // to add more fees then the total.						
    // ----------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // MOVE IMPORTS TO EXPORTS
    export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
    export.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    export.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
      import.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    export.HiddenCashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    export.HiddenCashReceiptSourceType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptSourceType.SystemGeneratedIdentifier;
    export.TotalFee.TotalCurrency = import.TotalFee.TotalCurrency;
    export.HiddenDisplayOk.Flag = import.HiddenDisplayOk.Flag;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.Selection1.SelectChar =
          import.Import1.Item.Selection1.SelectChar;
        MoveCashReceiptDetailFee(import.Import1.Item.CashReceiptDetailFee,
          export.Export1.Update.CashReceiptDetailFee);
        MoveCashReceiptDetailFeeType(import.Import1.Item.
          CashReceiptDetailFeeType,
          export.Export1.Update.CashReceiptDetailFeeType);
        export.Export1.Update.Detail.SelectChar =
          import.Import1.Item.Detail.SelectChar;

        if (Equal(global.Command, "PRMPTRET") && !
          IsEmpty(export.Export1.Item.Detail.SelectChar))
        {
          // *** Check to see if a value was passed back from FETL.  Only move 
          // value to export is one is returned.  Sunya Sharp 10/9/98 ***
          if (!IsEmpty(import.ReturnFromList.Code))
          {
            export.Export1.Update.CashReceiptDetailFeeType.Code =
              import.ReturnFromList.Code;
          }

          export.Export1.Update.Detail.SelectChar = "";
        }

        export.Export1.Next();
      }
    }

    if (Equal(global.Command, "PRMPTRET"))
    {
      return;
    }

    // *****
    // Next Tran/Security logic
    // *****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    if (!Equal(global.Command, "PRMPTRET"))
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // MAIN CASE CONSTRUCT
    switch(TrimEnd(global.Command))
    {
      case "UPDATE":
        if (AsChar(export.HiddenDisplayOk.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        if (ReadCashReceiptDetail())
        {
          local.CashReceiptDetail.Assign(entities.ForPersistent);
        }
        else
        {
          ExitState = "FN0052_CASH_RCPT_DTL_NF";

          return;
        }

        local.SuppliedOnInput.TotalCurrency = 0;
        local.SelectionCount.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          local.SuppliedOnInput.TotalCurrency += export.Export1.Item.
            CashReceiptDetailFee.Amount;

          if (AsChar(export.Export1.Item.Selection1.SelectChar) == 'S')
          {
            ++local.SelectionCount.Count;

            if (IsEmpty(export.Export1.Item.CashReceiptDetailFeeType.Code))
            {
              ExitState = "FN0000_MANDATORY_FIELDS";

              var field =
                GetField(export.Export1.Item.CashReceiptDetailFeeType, "code");

              field.Error = true;

              return;
            }

            if (ReadCashReceiptDetailFeeType1())
            {
              MoveCashReceiptDetailFeeType(entities.CashReceiptDetailFeeType,
                export.Export1.Update.CashReceiptDetailFeeType);
            }
            else
            {
              var field =
                GetField(export.Export1.Item.CashReceiptDetailFeeType, "code");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

              return;
            }

            if (export.Export1.Item.CashReceiptDetailFee.Amount == 0)
            {
              ExitState = "FN0000_MANDATORY_FIELDS";

              var field =
                GetField(export.Export1.Item.CashReceiptDetailFee, "amount");

              field.Error = true;

              return;
            }
          }
        }

        if (local.SelectionCount.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        if (local.SuppliedOnInput.TotalCurrency > export.TotalFee.TotalCurrency)
        {
          // *** Added logic to make entered group view amount fields error.  
          // Sunya Sharp 10/9/98 ***
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Selection1.SelectChar) == 'S')
            {
              var field =
                GetField(export.Export1.Item.CashReceiptDetailFee, "amount");

              field.Error = true;
            }
          }

          ExitState = "SUPPLIED_FEES_OUTOFBALANCE";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Selection1.SelectChar) == 'S')
          {
            UseFnUpdateCollectionFees();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
              export.Export1.Update.Selection1.SelectChar = "";
            }
            else
            {
              return;
            }
          }
        }

        // *** Added logic to check and see if the total amount is less than the
        // fee amount.  If it is then return message.   Sunya Sharp 10/9/98 ***
        if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          if (local.SuppliedOnInput.TotalCurrency < export
            .TotalFee.TotalCurrency)
          {
            ExitState = "FN0000_FEE_LESS_THAN_TOTAL_UPDT";

            return;
          }
        }

        break;
      case "":
        break;
      case "DISPLAY":
        // *** Display logic located at the bottom of the procedure. ***
        // *** Moved the display logic to the bottom of the procedure to be able
        // to access this logic after a successful delete.  Sunya Sharp 10/13/
        // 1998 ***
        break;
      case "ADD":
        if (ReadCashReceiptDetail())
        {
          local.CashReceiptDetail.Assign(entities.ForPersistent);
          export.TotalFee.TotalCurrency =
            entities.ForPersistent.CollectionAmount - entities
            .ForPersistent.ReceivedAmount;

          if (export.TotalFee.TotalCurrency == 0)
          {
            ExitState = "NO_FEES_ALLOWED_FOR_COLLECTION";

            return;
          }
          else
          {
            if (export.Export1.IsEmpty)
            {
              ExitState = "ENTER_REQUIRED_FEES";

              return;
            }

            local.SuppliedOnInput.TotalCurrency = 0;
            local.SelCount.Count = 0;

            // *** Add logic to prevent the user from getting more fees then the
            // total required.  When the user types over an existing row and
            // presses PF5 it is not calculating the totals correctly.  Sunya
            // Sharp 6/8/1999 ***
            foreach(var item in ReadCashReceiptDetailFee1())
            {
              local.SuppliedOnInput.TotalCurrency =
                entities.CashReceiptDetailFee.Amount + local
                .SuppliedOnInput.TotalCurrency;
            }

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Selection1.SelectChar) == 'S')
              {
                local.SuppliedOnInput.TotalCurrency =
                  export.Export1.Item.CashReceiptDetailFee.Amount + local
                  .SuppliedOnInput.TotalCurrency;
                local.SelCount.Count = local.SelectionCount.Count + 1;

                if (IsEmpty(export.Export1.Item.CashReceiptDetailFeeType.Code))
                {
                  ExitState = "FN0000_MANDATORY_FIELDS";

                  var field =
                    GetField(export.Export1.Item.CashReceiptDetailFeeType,
                    "code");

                  field.Error = true;

                  return;
                }

                if (ReadCashReceiptDetailFeeType1())
                {
                  MoveCashReceiptDetailFeeType(entities.
                    CashReceiptDetailFeeType,
                    export.Export1.Update.CashReceiptDetailFeeType);
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.CashReceiptDetailFeeType,
                    "code");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

                  return;
                }

                if (export.Export1.Item.CashReceiptDetailFee.Amount == 0)
                {
                  ExitState = "FN0000_MANDATORY_FIELDS";

                  var field =
                    GetField(export.Export1.Item.CashReceiptDetailFee, "amount");
                    

                  field.Error = true;

                  return;
                }
              }
            }

            if (local.SelCount.Count == 0)
            {
              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            if (local.SuppliedOnInput.TotalCurrency > export
              .TotalFee.TotalCurrency)
            {
              // *** Added logic to make entered group view amount fields error.
              // Sunya Sharp 10/9/98 ***
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                if (AsChar(export.Export1.Item.Selection1.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Export1.Item.CashReceiptDetailFee, "amount");
                    

                  field.Error = true;
                }
              }

              ExitState = "SUPPLIED_FEES_OUTOFBALANCE";

              return;
            }

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Selection1.SelectChar) == 'S')
              {
                export.Export1.Update.CashReceiptDetailFee.
                  SystemGeneratedIdentifier = Now();
                UseCreateCashReceiptDetFee();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  export.Export1.Update.Selection1.SelectChar = "";
                }
                else
                {
                  // issue rollback exit state here.  cannot only add partial 
                  // fees.
                  UseEabRollbackCics();

                  return;
                }
              }
            }

            // *** Added logic to check and see if the total amount is less than
            // the fee amount.  If it is then return error message.  Sunya
            // Sharp 10/9/98 ***
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.HiddenDisplayOk.Flag = "Y";

              if (local.SuppliedOnInput.TotalCurrency < export
                .TotalFee.TotalCurrency)
              {
                ExitState = "FN0000_FEE_LESS_THAN_TOTAL_ADD";

                return;
              }
            }

            ExitState = "ACO_NE0000_RETURN";
          }
        }
        else
        {
          ExitState = "FN0052_CASH_RCPT_DTL_NF";

          return;
        }

        break;
      case "DELETE":
        // *** Added logic to delete a fee.  Made the delete process work the 
        // same way as the add and update for less confusion.  Sunya Sharp 10/13
        // /1998 ***
        if (AsChar(export.HiddenDisplayOk.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        if (!ReadCashReceiptDetail())
        {
          ExitState = "FN0052_CASH_RCPT_DTL_NF";

          return;
        }

        local.SelectionCount.Count = 0;
        local.Delete.Flag = "";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Selection1.SelectChar) == 'S')
          {
            ++local.SelectionCount.Count;
            UseFnDeleteCollectionFees();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
              export.Export1.Update.Selection1.SelectChar = "";
            }
            else
            {
              return;
            }
          }
        }

        if (local.SelectionCount.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        local.Delete.Flag = "Y";
        global.Command = "DISPLAY";

        break;
      case "LIST":
        // ****
        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        // ****
        // *** Set statements included rounding the selection count.  This is 
        // not required and generates unnecessary code.  Removed ROUNDED from
        // statements.  Sunya Sharp 10/9/98 ***
        local.SelectionCount.Count = 0;
        local.SelectedPrompt.SelectChar = "";

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          MoveCashReceiptDetailFee(import.Import1.Item.CashReceiptDetailFee,
            export.Export1.Update.CashReceiptDetailFee);
          MoveCashReceiptDetailFeeType(import.Import1.Item.
            CashReceiptDetailFeeType,
            export.Export1.Update.CashReceiptDetailFeeType);
          export.Export1.Update.Detail.SelectChar =
            import.Import1.Item.Detail.SelectChar;

          switch(AsChar(export.Export1.Item.Detail.SelectChar))
          {
            case ' ':
              break;
            case '+':
              break;
            case 'S':
              ++local.SelectionCount.Count;
              local.SelectedPrompt.SelectChar = "S";

              break;
            default:
              ++local.SelectionCount.Count;

              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;

              break;
          }

          export.Export1.Next();
        }

        if (local.SelectionCount.Count == 1)
        {
          if (AsChar(local.SelectedPrompt.SelectChar) == 'S')
          {
            ExitState = "ECO_LNK_LST_FEE_TYPES";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }
        else if (local.SelectionCount.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }
        else if (AsChar(local.SelectedPrompt.SelectChar) == 'S')
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.Detail.SelectChar) == 'S')
            {
              var field = GetField(export.Export1.Item.Detail, "selectChar");

              field.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          local.TotalFees.TotalCurrency += export.Export1.Item.
            CashReceiptDetailFee.Amount;
        }

        if (local.TotalFees.TotalCurrency != export.TotalFee.TotalCurrency)
        {
          ExitState = "FN0000_FEES_LESS_THAN_TOTAL_FEES";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "HELP":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (ReadCashReceiptDetail())
      {
        export.CashReceiptDetail.SequentialIdentifier =
          entities.ForPersistent.SequentialIdentifier;
        export.TotalFee.TotalCurrency =
          entities.ForPersistent.CollectionAmount - entities
          .ForPersistent.ReceivedAmount;

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCashReceiptDetailFee2())
        {
          MoveCashReceiptDetailFee(entities.CashReceiptDetailFee,
            export.Export1.Update.CashReceiptDetailFee);

          if (ReadCashReceiptDetailFeeType2())
          {
            MoveCashReceiptDetailFeeType(entities.CashReceiptDetailFeeType,
              export.Export1.Update.CashReceiptDetailFeeType);
          }
          else
          {
            ExitState = "FN0048_CASH_RCPT_DTL_FEE_TYP_NF";
            export.Export1.Next();

            return;
          }

          export.Export1.Next();
        }
      }
      else
      {
        ExitState = "FN0052_CASH_RCPT_DTL_NF";

        return;
      }

      if (export.Export1.IsEmpty)
      {
        if (entities.ForPersistent.CollectionAmount > entities
          .ForPersistent.ReceivedAmount)
        {
          ExitState = "ENTER_REQUIRED_FEES";
        }
        else
        {
          ExitState = "NO_FEES_ALLOWED_FOR_COLLECTION";
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Update.CashReceiptDetailFee.Amount = 0;
          export.Export1.Update.CashReceiptDetailFeeType.Code = "";
          export.Export1.Next();
        }
      }
      else
      {
        // *** Added logic to handle if displaying after a successful delete.  
        // Sunya Sharp 10/13/98 ***
        if (AsChar(local.Delete.Flag) == 'Y')
        {
          local.SuppliedOnInput.TotalCurrency = 0;

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            local.SuppliedOnInput.TotalCurrency += export.Export1.Item.
              CashReceiptDetailFee.Amount;
          }

          if (local.SuppliedOnInput.TotalCurrency != export
            .TotalFee.TotalCurrency)
          {
            ExitState = "FN0000_FEES_LESS_THAN_TOTAL_DEL";
          }
          else
          {
            export.HiddenDisplayOk.Flag = "Y";
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          }
        }
        else
        {
          export.HiddenDisplayOk.Flag = "Y";
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
      }
    }
    else
    {
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
  }

  private static void MoveCashReceiptDetailFee(CashReceiptDetailFee source,
    CashReceiptDetailFee target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveCashReceiptDetailFeeType(
    CashReceiptDetailFeeType source, CashReceiptDetailFeeType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private void UseCreateCashReceiptDetFee()
  {
    var useImport = new CreateCashReceiptDetFee.Import();
    var useExport = new CreateCashReceiptDetFee.Export();

    useImport.CashReceiptDetail.Assign(entities.ForPersistent);
    useImport.CashReceiptDetailFeeType.Code =
      export.Export1.Item.CashReceiptDetailFeeType.Code;
    MoveCashReceiptDetailFee(export.Export1.Item.CashReceiptDetailFee,
      useImport.CashReceiptDetailFee);

    Call(CreateCashReceiptDetFee.Execute, useImport, useExport);

    MoveCashReceiptDetail(useImport.CashReceiptDetail, entities.ForPersistent);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnDeleteCollectionFees()
  {
    var useImport = new FnDeleteCollectionFees.Import();
    var useExport = new FnDeleteCollectionFees.Export();

    useImport.P.Assign(entities.ForPersistent);
    useImport.CashReceiptDetailFee.SystemGeneratedIdentifier =
      export.Export1.Item.CashReceiptDetailFee.SystemGeneratedIdentifier;

    Call(FnDeleteCollectionFees.Execute, useImport, useExport);

    MoveCashReceiptDetail(useImport.P, entities.ForPersistent);
  }

  private void UseFnUpdateCollectionFees()
  {
    var useImport = new FnUpdateCollectionFees.Import();
    var useExport = new FnUpdateCollectionFees.Export();

    useImport.P.Assign(entities.ForPersistent);
    MoveCashReceiptDetailFeeType(export.Export1.Item.CashReceiptDetailFeeType,
      useImport.CashReceiptDetailFeeType);
    MoveCashReceiptDetailFee(export.Export1.Item.CashReceiptDetailFee,
      useImport.CashReceiptDetailFee);

    Call(FnUpdateCollectionFees.Execute, useImport, useExport);

    MoveCashReceiptDetail(useImport.P, entities.ForPersistent);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
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

  private bool ReadCashReceiptDetail()
  {
    entities.ForPersistent.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          import.HiddenCashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.HiddenCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.HiddenCashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ForPersistent.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ForPersistent.CstIdentifier = db.GetInt32(reader, 1);
        entities.ForPersistent.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ForPersistent.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.ForPersistent.ReceivedAmount = db.GetDecimal(reader, 4);
        entities.ForPersistent.CollectionAmount = db.GetDecimal(reader, 5);
        entities.ForPersistent.CollectionDate = db.GetDate(reader, 6);
        entities.ForPersistent.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailFee1()
  {
    System.Diagnostics.Debug.Assert(entities.ForPersistent.Populated);
    entities.CashReceiptDetailFee.Populated = false;

    return ReadEach("ReadCashReceiptDetailFee1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ForPersistent.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.ForPersistent.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.ForPersistent.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.ForPersistent.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailFee.CrdIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailFee.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailFee.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailFee.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetailFee.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 4);
        entities.CashReceiptDetailFee.Amount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetailFee.CdtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.CashReceiptDetailFee.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailFee2()
  {
    System.Diagnostics.Debug.Assert(entities.ForPersistent.Populated);

    return ReadEach("ReadCashReceiptDetailFee2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ForPersistent.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.ForPersistent.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.ForPersistent.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.ForPersistent.CrtIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CashReceiptDetailFee.CrdIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailFee.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailFee.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailFee.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetailFee.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 4);
        entities.CashReceiptDetailFee.Amount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetailFee.CdtIdentifier =
          db.GetNullableInt32(reader, 6);
        entities.CashReceiptDetailFee.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailFeeType1()
  {
    entities.CashReceiptDetailFeeType.Populated = false;

    return Read("ReadCashReceiptDetailFeeType1",
      (db, command) =>
      {
        db.SetString(
          command, "code", export.Export1.Item.CashReceiptDetailFeeType.Code);
        db.SetDate(
          command, "effectiveDate",
          local.CashReceiptDetail.CollectionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailFeeType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailFeeType.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailFeeType.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptDetailFeeType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptDetailFeeType.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailFeeType2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetailFee.Populated);
    entities.CashReceiptDetailFeeType.Populated = false;

    return Read("ReadCashReceiptDetailFeeType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdtlFeeTypeId",
          entities.CashReceiptDetailFee.CdtIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailFeeType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailFeeType.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailFeeType.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptDetailFeeType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptDetailFeeType.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Selection1.
      /// </summary>
      [JsonPropertyName("selection1")]
      public Common Selection1
      {
        get => selection1 ??= new();
        set => selection1 = value;
      }

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
      /// A value of CashReceiptDetailFeeType.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailFeeType")]
      public CashReceiptDetailFeeType CashReceiptDetailFeeType
      {
        get => cashReceiptDetailFeeType ??= new();
        set => cashReceiptDetailFeeType = value;
      }

      /// <summary>
      /// A value of CashReceiptDetailFee.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailFee")]
      public CashReceiptDetailFee CashReceiptDetailFee
      {
        get => cashReceiptDetailFee ??= new();
        set => cashReceiptDetailFee = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common selection1;
      private Common detail;
      private CashReceiptDetailFeeType cashReceiptDetailFeeType;
      private CashReceiptDetailFee cashReceiptDetailFee;
    }

    /// <summary>
    /// A value of HiddenDisplayOk.
    /// </summary>
    [JsonPropertyName("hiddenDisplayOk")]
    public Common HiddenDisplayOk
    {
      get => hiddenDisplayOk ??= new();
      set => hiddenDisplayOk = value;
    }

    /// <summary>
    /// A value of ReturnFromList.
    /// </summary>
    [JsonPropertyName("returnFromList")]
    public CashReceiptDetailFeeType ReturnFromList
    {
      get => returnFromList ??= new();
      set => returnFromList = value;
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
    /// A value of HiddenCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptEvent")]
    public CashReceiptEvent HiddenCashReceiptEvent
    {
      get => hiddenCashReceiptEvent ??= new();
      set => hiddenCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptType")]
    public CashReceiptType HiddenCashReceiptType
    {
      get => hiddenCashReceiptType ??= new();
      set => hiddenCashReceiptType = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// A value of TotalFee.
    /// </summary>
    [JsonPropertyName("totalFee")]
    public Common TotalFee
    {
      get => totalFee ??= new();
      set => totalFee = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common hiddenDisplayOk;
    private CashReceiptDetailFeeType returnFromList;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptType hiddenCashReceiptType;
    private Array<ImportGroup> import1;
    private Common totalFee;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
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
      /// A value of Selection1.
      /// </summary>
      [JsonPropertyName("selection1")]
      public Common Selection1
      {
        get => selection1 ??= new();
        set => selection1 = value;
      }

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
      /// A value of CashReceiptDetailFeeType.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailFeeType")]
      public CashReceiptDetailFeeType CashReceiptDetailFeeType
      {
        get => cashReceiptDetailFeeType ??= new();
        set => cashReceiptDetailFeeType = value;
      }

      /// <summary>
      /// A value of CashReceiptDetailFee.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailFee")]
      public CashReceiptDetailFee CashReceiptDetailFee
      {
        get => cashReceiptDetailFee ??= new();
        set => cashReceiptDetailFee = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common selection1;
      private Common detail;
      private CashReceiptDetailFeeType cashReceiptDetailFeeType;
      private CashReceiptDetailFee cashReceiptDetailFee;
    }

    /// <summary>
    /// A value of HiddenDisplayOk.
    /// </summary>
    [JsonPropertyName("hiddenDisplayOk")]
    public Common HiddenDisplayOk
    {
      get => hiddenDisplayOk ??= new();
      set => hiddenDisplayOk = value;
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
    /// A value of HiddenCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptEvent")]
    public CashReceiptEvent HiddenCashReceiptEvent
    {
      get => hiddenCashReceiptEvent ??= new();
      set => hiddenCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptType")]
    public CashReceiptType HiddenCashReceiptType
    {
      get => hiddenCashReceiptType ??= new();
      set => hiddenCashReceiptType = value;
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
    /// A value of TotalFee.
    /// </summary>
    [JsonPropertyName("totalFee")]
    public Common TotalFee
    {
      get => totalFee ??= new();
      set => totalFee = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common hiddenDisplayOk;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptType hiddenCashReceiptType;
    private Array<ExportGroup> export1;
    private Common totalFee;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Delete.
    /// </summary>
    [JsonPropertyName("delete")]
    public Common Delete
    {
      get => delete ??= new();
      set => delete = value;
    }

    /// <summary>
    /// A value of TotalFees.
    /// </summary>
    [JsonPropertyName("totalFees")]
    public Common TotalFees
    {
      get => totalFees ??= new();
      set => totalFees = value;
    }

    /// <summary>
    /// A value of SelCount.
    /// </summary>
    [JsonPropertyName("selCount")]
    public Common SelCount
    {
      get => selCount ??= new();
      set => selCount = value;
    }

    /// <summary>
    /// A value of SelectedPrompt.
    /// </summary>
    [JsonPropertyName("selectedPrompt")]
    public Common SelectedPrompt
    {
      get => selectedPrompt ??= new();
      set => selectedPrompt = value;
    }

    /// <summary>
    /// A value of SelectionCount.
    /// </summary>
    [JsonPropertyName("selectionCount")]
    public Common SelectionCount
    {
      get => selectionCount ??= new();
      set => selectionCount = value;
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
    /// A value of SuppliedOnInput.
    /// </summary>
    [JsonPropertyName("suppliedOnInput")]
    public Common SuppliedOnInput
    {
      get => suppliedOnInput ??= new();
      set => suppliedOnInput = value;
    }

    private Common delete;
    private Common totalFees;
    private Common selCount;
    private Common selectedPrompt;
    private Common selectionCount;
    private CashReceiptDetail cashReceiptDetail;
    private Common suppliedOnInput;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ForPersistent.
    /// </summary>
    [JsonPropertyName("forPersistent")]
    public CashReceiptDetail ForPersistent
    {
      get => forPersistent ??= new();
      set => forPersistent = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailFeeType.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFeeType")]
    public CashReceiptDetailFeeType CashReceiptDetailFeeType
    {
      get => cashReceiptDetailFeeType ??= new();
      set => cashReceiptDetailFeeType = value;
    }

    private CashReceiptDetail forPersistent;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceiptDetailFeeType cashReceiptDetailFeeType;
  }
#endregion
}
