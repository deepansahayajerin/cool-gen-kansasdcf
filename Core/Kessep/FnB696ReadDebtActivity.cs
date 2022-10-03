// Program: FN_B696_READ_DEBT_ACTIVITY, ID: 371126959, model: 746.
// Short name: SWE04100
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
/// A program: FN_B696_READ_DEBT_ACTIVITY.
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
public partial class FnB696ReadDebtActivity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B696_READ_DEBT_ACTIVITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB696ReadDebtActivity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB696ReadDebtActivity.
  /// </summary>
  public FnB696ReadDebtActivity(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Current.Date = import.Current.Date;
    local.ProgramProcessingInfo.ProcessDate = local.Current.Date;
    local.Current.YearMonth = UseCabGetYearMonthFromDate2();
    local.HardcodeObligor.Type1 = "R";
    local.HardcodeSupported.Type1 = "S";

    // ***** MAIN-LINE AREA *****
    if (ReadCsePerson1())
    {
      if (AsChar(entities.ObligorCsePerson.Type1) == 'C')
      {
        export.CsePersonsWorkSet.Number = entities.ObligorCsePerson.Number;
        UseSiReadCsePersonBatch2();
      }
      else
      {
        export.CsePersonsWorkSet.FormattedName =
          entities.ObligorCsePerson.OrganizationName ?? Spaces(33);
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ***---  select only
    if (ReadCsePersonAccount())
    {
      // : Continue Processing . . .
    }
    else
    {
      ExitState = "CSE_PERSON_NOT_AN_OBLIGOR";

      return;
    }

    if (import.SearchObligation.SystemGeneratedIdentifier != 0)
    {
      local.LowObligation.SystemGeneratedIdentifier =
        import.SearchObligation.SystemGeneratedIdentifier;
      local.HighObligation.SystemGeneratedIdentifier =
        import.SearchObligation.SystemGeneratedIdentifier;
    }
    else
    {
      local.LowObligation.SystemGeneratedIdentifier = 0;
      local.HighObligation.SystemGeneratedIdentifier = 999;
    }

    if (import.SearchObligationType.SystemGeneratedIdentifier != 0)
    {
      local.LowObligationType.SystemGeneratedIdentifier =
        import.SearchObligationType.SystemGeneratedIdentifier;
      local.HighObligationType.SystemGeneratedIdentifier =
        import.SearchObligationType.SystemGeneratedIdentifier;
    }
    else
    {
      local.LowObligationType.SystemGeneratedIdentifier = 0;
      local.HighObligationType.SystemGeneratedIdentifier = 999;
    }

    local.Group.Index = -1;

    foreach(var item in ReadObligationObligationTransactionDebtDetailObligationType())
      
    {
      if (AsChar(import.ListDebtsWithAmtOwed.SelectChar) == 'Y')
      {
        if (Lt(local.Zero.Date, entities.DebtDetail.RetiredDt))
        {
          continue;
        }
      }

      // =================================================
      // 2/19/00 - Bud Adams  -  IF this is the same obligation as the
      //   last time through, no need to do this.
      // =================================================
      if (local.Previous.SystemGeneratedIdentifier == entities
        .Obligation.SystemGeneratedIdentifier)
      {
      }
      else
      {
        local.Previous.SystemGeneratedIdentifier =
          entities.Obligation.SystemGeneratedIdentifier;

        if (!IsEmpty(import.SearchLegalAction.StandardNumber))
        {
          // ***---  select only
          if (ReadLegalAction1())
          {
            local.LegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
          }
          else
          {
            local.LegalAction.StandardNumber =
              local.InitializedLegalAction.StandardNumber;
            local.Previous.SystemGeneratedIdentifier = 0;

            continue;
          }
        }
        else
        {
          // ***---  select only
          if (ReadLegalAction2())
          {
            local.LegalAction.StandardNumber =
              entities.LegalAction.StandardNumber;
          }
          else
          {
            local.LegalAction.StandardNumber =
              local.InitializedLegalAction.StandardNumber;
          }
        }
      }

      if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
      {
        // ***---  select only
        if (ReadCsePerson2())
        {
          local.Supported.Number = entities.SupportedPerson.Number;
          UseSiReadCsePersonBatch1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.Supported.FirstName = "ZZZZZZZZZZZZ";
            local.Supported.MiddleInitial = "Z";
            local.Supported.LastName = "ZZZZZZZZZZZZZZZZZ";
            local.Supported.FormattedName = "** ADABAS UNAVAILABLE **";
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else if (IsEmpty(local.Supported.FormattedName))
          {
            if (Lt(local.Supported.LastName, "AAAAAAAAAAAAAAAAA") || Lt
              ("ZZZZZZZZZZZZZZZZZ", local.Supported.LastName))
            {
              local.Supported.FormattedName = local.Supported.LastName;
            }
          }

          // =================================================
          // 7/27/99 - bud adams  -  for obligations that exist due to legal
          //   actions, the Case and Worker are determined differently.
          //   Also, for Case information, it doesn't matter if the supported
          //   person is currently active on that case or not.
          //   Added the IF, the READ, and the fn_read_case_and_wrkr_
          //   from_legal.
          // 10/21/99 - E. Parker - Made changes to read Case No and Worker Id 
          // similar to Disbursements.  Implemented new Action Block.
          // =================================================
          UseFnDetCaseNoAndWrkrForDbt();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          UseFnDeterminePgmForDebtDetail();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
            local.ProgramScreenAttributes.ProgramTypeInd = "##";
          }
          else
          {
            local.ProgramScreenAttributes.ProgramTypeInd =
              local.DistributionPgm.Code;
          }
        }
        else
        {
          local.Supported.FormattedName = "SUPPORTED PERSON NF";
        }

        local.DateWorkArea.Date = entities.DebtDetail.DueDt;

        if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
        {
          // : Go fill up the export group.  That logic will handle the overflow
          // msg.
          break;
        }

        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.Common.Count = local.Group.Index + 1;
        MoveObligation(entities.Obligation, local.Group.Update.Obligation);
        MoveObligationType1(entities.ObligationType,
          local.Group.Update.ObligationType);
        local.Group.Update.LegalAction.StandardNumber =
          local.LegalAction.StandardNumber;
        local.Group.Update.ObligationTransaction.Assign(
          entities.ObligationTransaction);
        MoveDebtDetail2(entities.DebtDetail, local.Group.Update.DebtDetail);
        MoveCsePerson(entities.SupportedPerson,
          local.Group.Update.SupportedCsePerson);
        local.Group.Update.SupportedCsePersonsWorkSet.Assign(local.Supported);
        local.Group.Update.ProgramScreenAttributes.ProgramTypeInd =
          local.ProgramScreenAttributes.ProgramTypeInd;
        local.Group.Update.DprProgram.ProgramState =
          local.DprProgram.ProgramState;
        local.Group.Update.Case1.Number = local.Case1.Number;
        MoveServiceProvider(local.ServiceProvider,
          local.Group.Update.ServiceProvider);
      }
      else
      {
        // : No supported person.
        if (local.Group.Index + 1 >= Local.GroupGroup.Capacity)
        {
          // : Go fill up the export group.  That logic will handle the overflow
          // msg.
          break;
        }

        ++local.Group.Index;
        local.Group.CheckSize();

        local.Group.Update.Common.Count = local.Group.Index + 1;
        MoveObligation(entities.Obligation, local.Group.Update.Obligation);
        MoveObligationType1(entities.ObligationType,
          local.Group.Update.ObligationType);
        local.Group.Update.LegalAction.StandardNumber =
          local.LegalAction.StandardNumber;
        local.Group.Update.ObligationTransaction.Assign(
          entities.ObligationTransaction);
        MoveDebtDetail2(entities.DebtDetail, local.Group.Update.DebtDetail);

        if (ReadServiceProvider())
        {
          MoveServiceProvider(entities.ServiceProvider,
            local.Group.Update.ServiceProvider);
        }
        else
        {
          local.ServiceProvider.UserId = "N/A";
        }
      }
    }

    if (!local.Group.IsEmpty)
    {
      export.Debt.Index = -1;
      local.Group.Index = 0;

      for(var limit = local.Group.Count; local.Group.Index < limit; ++
        local.Group.Index)
      {
        if (!local.Group.CheckSize())
        {
          break;
        }

        // *** Populate the values used in the Format Debt Line 1 cab ***
        local.Obligation.InterestAmountOwed =
          local.Group.Item.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
          

        if (AsChar(local.Group.Item.ObligationType.Classification) == 'A')
        {
          // --- Accruing obligations
          local.DebtDue.Date = local.Group.Item.DebtDetail.DueDt;
          local.DebtDue.YearMonth = UseCabGetYearMonthFromDate1();

          if (local.DebtDue.YearMonth < local.Current.YearMonth)
          {
            local.Obligation.ArrearsAmountOwed =
              local.Group.Item.DebtDetail.BalanceDueAmt;
            local.Obligation.CurrentAmountOwed = 0;
          }
          else
          {
            local.Obligation.CurrentAmountOwed =
              local.Group.Item.DebtDetail.BalanceDueAmt;
            local.Obligation.ArrearsAmountOwed = 0;
          }
        }
        else
        {
          // --- non accruing, recoveries, fees etc
          local.DebtDue.Date = local.Group.Item.DebtDetail.DueDt;
          local.Obligation.ArrearsAmountOwed =
            local.Group.Item.DebtDetail.BalanceDueAmt;
          local.Obligation.CurrentAmountOwed = 0;
        }

        local.Obligation.TotalAmountOwed =
          local.Obligation.ArrearsAmountOwed + local
          .Obligation.CurrentAmountOwed + local.Obligation.InterestAmountOwed;

        // ---------------------------------------------
        // The EAB moves debt_detail balance due amt to screen field Amt-Due. 
        // But we want the screen field to show the original obligation
        // transaction amount. Until the EAB is fixed use a local view to get
        // around the problem. The EAB does not use any field other than this
        // field.
        // ---------------------------------------------
        MoveDebtDetail1(local.Group.Item.DebtDetail, local.DebtDetail);
        local.DebtDetail.BalanceDueAmt =
          local.Group.Item.ObligationTransaction.Amount;

        // : Format debt type report line.
        if (Equal(local.Group.Item.ObligationType.Code, "VOL"))
        {
          local.DebtDtlDueDate.Text10 = "";
        }
        else
        {
          // : Format text date for the debt detail due date.
          local.TextMm.Text2 =
            NumberToString(DateToInt(local.DebtDetail.DueDt), 12, 2);
          local.TextDd.Text2 =
            NumberToString(DateToInt(local.DebtDetail.DueDt), 14, 2);
          local.TextYyyy.Text4 =
            NumberToString(DateToInt(local.DebtDue.Date), 8, 4);
          local.DebtDtlDueDate.Text10 = local.TextMm.Text2 + "/" + local
            .TextDd.Text2 + "/" + local.TextYyyy.Text4;
        }

        // : See previous note re use of ob trn amount for this field (we want 
        // to
        //   use original ob trn amount, not amount due).
        local.CurrencyToText.TotalCurrency = local.DebtDetail.BalanceDueAmt;
        UseFnCabCurrencyToText5();
        local.CurrencyToText.TotalCurrency = local.Obligation.CurrentAmountOwed;
        UseFnCabCurrencyToText3();
        local.CurrencyToText.TotalCurrency = local.Obligation.ArrearsAmountOwed;
        UseFnCabCurrencyToText4();
        local.CurrencyToText.TotalCurrency =
          local.Obligation.InterestAmountOwed;
        UseFnCabCurrencyToText1();
        local.CurrencyToText.TotalCurrency = local.Obligation.TotalAmountOwed;
        UseFnCabCurrencyToText2();
        local.TextObligationId.Text3 =
          NumberToString(local.Group.Item.Obligation.SystemGeneratedIdentifier,
          13, 3);

        if (!IsEmpty(local.Group.Item.ProgramScreenAttributes.ProgramTypeInd))
        {
          local.ProgramLiteral.Text5 =
            TrimEnd(local.Group.Item.ProgramScreenAttributes.ProgramTypeInd) + "-"
            + local.Group.Item.DprProgram.ProgramState;
        }

        if (export.Debt.Index + 1 >= Export.DebtGroup.Capacity)
        {
          export.Debt.Index = Export.DebtGroup.Capacity - 1;
          export.Debt.CheckSize();

          export.Debt.Update.Debt1.LineText =
            "**********  MORE DATA EXISTS THAN IS ON THE REPORT - USE FILTERS. ***********";
            
          ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

          return;
        }

        ++export.Debt.Index;
        export.Debt.CheckSize();

        export.Debt.Update.Debt1.LineText =
          local.Group.Item.ObligationTransaction.Type1 + "     " + local
          .TextObligationId.Text3 + "  " + local
          .Group.Item.ObligationType.Code + "   " + local
          .DebtDtlDueDate.Text10 + " " + TrimEnd(" ") + local
          .ProgramLiteral.Text5 + "   " + " " + local
          .Group.Item.Obligation.OrderTypeCode + "        " + Substring
          (local.Group.Item.LegalAction.StandardNumber, 20, 1, 12) + "   " + local
          .Group.Item.ServiceProvider.UserId + " " + local.AmtDue.Text10 + "  " +
          local.CurrOwed.Text10 + " " + local.ArrsOwed.Text10 + " " + local
          .IntOwed.Text10 + "" + local.TotOwed.Text10 + " ";

        if (!IsEmpty(local.Group.Item.SupportedCsePerson.Number))
        {
          if (export.Debt.Index + 1 == Export.DebtGroup.Capacity)
          {
            export.Debt.Index = Export.DebtGroup.Capacity - 1;
            export.Debt.CheckSize();

            export.Debt.Update.Debt1.LineText =
              "**********  MORE DATA EXISTS THAN IS ON THE REPORT - USE FILTERS. ***********";
              
            ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

            return;
          }

          // M. Brown, pr#133140, Dec 5, 2001 - fixed supported person number - 
          // it was truncating the last character on the report.
          ++export.Debt.Index;
          export.Debt.CheckSize();

          export.Debt.Update.Debt1.LineText = "Child/Supported Person: " + local
            .Group.Item.SupportedCsePerson.Number + "  " + local
            .Group.Item.SupportedCsePersonsWorkSet.FormattedName;
        }

        // ***************************************************************************
        // Logic to populate Collection, Collection Adj and Debt Adj lines (if 
        // any)
        // ***************************************************************************
        // *** Creating the Collection_Adjustment Line ***
        foreach(var item in ReadCollection())
        {
          // ===============================================
          // Collection Identifier is not sequential.  Once the first set of
          //   collections are retrieved for a specific ob-tran, this number
          //   must be reset to zero.  -  bud adams  -  5/22/00
          //   By the way, this is my last day of work on KESSEP.
          // ===============================================
          // =============================================================
          // RBM 08/26/97  Do not show Collection Adjustments if
          //               the show-colladj = 'N'
          // =============================================================
          if (AsChar(import.SearchShowCollAdj.Text1) == 'N' && AsChar
            (entities.Collection.AdjustedInd) == 'Y')
          {
            continue;
          }

          // ***---  select only
          ReadCashReceiptDetail();

          // ***---  select only
          if (ReadCashReceipt())
          {
            if (!ReadCashReceiptSourceType())
            {
              local.CashReceiptSourceType.Code = "";
            }

            ReadCashReceiptEvent();
            ReadCashReceiptType();
          }

          // : Format text date for the collection date.
          local.TextMm.Text2 =
            NumberToString(DateToInt(entities.Collection.CollectionDt), 12, 2);
          local.TextDd.Text2 =
            NumberToString(DateToInt(entities.Collection.CollectionDt), 14, 2);
          local.TextYyyy.Text4 =
            NumberToString(DateToInt(entities.Collection.CollectionDt), 8, 4);
          local.CollectionDateWorkArea.Text10 = local.TextMm.Text2 + "/" + local
            .TextDd.Text2 + "/" + local.TextYyyy.Text4;
          local.EffectiveDate.Date = Date(entities.Collection.CreatedTmst);
          local.TextMm.Text2 =
            NumberToString(DateToInt(local.EffectiveDate.Date), 12, 2);
          local.TextDd.Text2 =
            NumberToString(DateToInt(local.EffectiveDate.Date), 14, 2);
          local.TextYyyy.Text4 =
            NumberToString(DateToInt(local.EffectiveDate.Date), 8, 4);
          local.ProcessDate.Text10 = local.TextMm.Text2 + "/" + local
            .TextDd.Text2 + "/" + local.TextYyyy.Text4;
          local.CurrencyToText.TotalCurrency = entities.Collection.Amount;
          UseFnCabCurrencyToText7();

          if (!IsEmpty(entities.Collection.ProgramAppliedTo))
          {
            local.ProgramLiteral.Text5 =
              TrimEnd(entities.Collection.ProgramAppliedTo) + "-" + entities
              .Collection.DistPgmStateAppldTo;
          }

          switch(AsChar(entities.Collection.DistributionMethod))
          {
            case 'A':
              local.DistMethodLiteral.Text9 = "Automatic";

              break;
            case 'M':
              local.DistMethodLiteral.Text9 = "Manual";

              break;
            case 'W':
              local.DistMethodLiteral.Text9 = "Write-off";

              break;
            default:
              break;
          }

          if (AsChar(entities.Collection.AppliedToCode) == 'C')
          {
            local.AppliedToLiteral.Text7 = "Current";
          }
          else if (AsChar(entities.Collection.AppliedToCode) == 'A')
          {
            local.AppliedToLiteral.Text7 = "Arrears";
          }

          if (export.Debt.Index + 1 >= Export.DebtGroup.Capacity)
          {
            export.Debt.Index = Export.DebtGroup.Capacity - 1;
            export.Debt.CheckSize();

            export.Debt.Update.Debt1.LineText =
              "**********  MORE DATA EXISTS THAN IS ON THE REPORT - USE FILTERS. ***********";
              
            ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

            return;
          }

          ++export.Debt.Index;
          export.Debt.CheckSize();

          export.Debt.Update.Debt1.LineText = " CO" + "  " + local
            .CollectionDateWorkArea.Text10 + "   " + entities
            .CashReceiptSourceType.Code + "  " + local.ProcessDate.Text10 + ""
            + local.CollectionAmt.Text10 + "    " + local
            .AppliedToLiteral.Text7 + "    " + local.ProgramLiteral.Text5 + " " +
            "" + " " + "" + "                              " + local
            .DistMethodLiteral.Text9;

          if (AsChar(entities.Collection.AdjustedInd) == 'Y' && AsChar
            (import.SearchShowCollAdj.Text1) == 'Y')
          {
            if (ReadCollectionAdjustmentReason())
            {
              local.AdjReasonCode.TextLine10 =
                entities.CollectionAdjustmentReason.Code;
            }
            else
            {
              ExitState = "FN0000_COLL_ADJUST_REASON_NF";

              return;
            }

            // : Format text date for the collection date.
            local.TextMm.Text2 =
              NumberToString(DateToInt(
                entities.Collection.CollectionAdjustmentDt), 12, 2);
            local.TextDd.Text2 =
              NumberToString(DateToInt(
                entities.Collection.CollectionAdjustmentDt), 14, 2);
            local.TextYyyy.Text4 =
              NumberToString(DateToInt(
                entities.Collection.CollectionAdjustmentDt), 8, 4);
            local.CollAdjDate.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;

            // : Format text date for the collection process date.
            local.TextMm.Text2 =
              NumberToString(DateToInt(entities.Collection.CollectionDt), 12, 2);
              
            local.TextDd.Text2 =
              NumberToString(DateToInt(entities.Collection.CollectionDt), 14, 2);
              
            local.TextYyyy.Text4 =
              NumberToString(DateToInt(local.DebtDue.Date), 8, 4);
            local.Date.Text10 = local.TextMm.Text2 + "/" + local
              .TextDd.Text2 + "/" + local.TextYyyy.Text4;

            if (export.Debt.Index + 1 >= Export.DebtGroup.Capacity)
            {
              export.Debt.Index = Export.DebtGroup.Capacity - 1;
              export.Debt.CheckSize();

              export.Debt.Update.Debt1.LineText =
                "**********  MORE DATA EXISTS THAN IS ON THE REPORT - USE FILTERS. ***********";
                
              ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

              return;
            }

            ++export.Debt.Index;
            export.Debt.CheckSize();

            export.Debt.Update.Debt1.LineText = " CA" + "  " + local
              .CollAdjDate.Text10 + "   " + entities
              .CashReceiptSourceType.Code + "  " + local.Date.Text10 + "" + local
              .CollectionAmt.Text10 + "    " + local.AppliedToLiteral.Text7 + "    " +
              local.ProgramLiteral.Text5 + " " + "" + "             " + local
              .AdjReasonCode.TextLine10 + "        " + local
              .DistMethodLiteral.Text9;

            continue;
          }
        }

        // *** There should not be any Adjustments against Voluntary Obligations
        // ***
        if (AsChar(local.Group.Item.ObligationType.Classification) == 'V')
        {
          continue;
        }

        if (AsChar(import.SearchShowDebtAdj.Text1) == 'Y')
        {
          // *** Check for Debt Adjustment Obligation Transactions. ***
          foreach(var item in ReadObligationTransactionRln())
          {
            if (ReadObligationTransaction())
            {
              if (Equal(entities.NonDebt.Type1, "DA"))
              {
                if (ReadObligationTransactionRlnRsn())
                {
                  local.AdjReasonCode.TextLine10 =
                    entities.ObligationTransactionRlnRsn.Code;
                }
                else
                {
                  ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF";

                  return;
                }

                // : Format text date for the debt adjustment date.
                local.TextMm.Text2 =
                  NumberToString(DateToInt(entities.NonDebt.DebtAdjustmentDt),
                  12, 2);
                local.TextDd.Text2 =
                  NumberToString(DateToInt(entities.NonDebt.DebtAdjustmentDt),
                  14, 2);
                local.TextYyyy.Text4 =
                  NumberToString(DateToInt(entities.NonDebt.DebtAdjustmentDt),
                  8, 4);
                local.Date.Text10 = local.TextMm.Text2 + "/" + local
                  .TextDd.Text2 + "/" + local.TextYyyy.Text4;

                // : Format text date for the debt adjustment process date.
                local.TextMm.Text2 =
                  NumberToString(DateToInt(
                    entities.NonDebt.DebtAdjustmentProcessDate), 12, 2);
                local.TextDd.Text2 =
                  NumberToString(DateToInt(
                    entities.NonDebt.DebtAdjustmentProcessDate), 14, 2);
                local.TextYyyy.Text4 =
                  NumberToString(DateToInt(
                    entities.NonDebt.DebtAdjustmentProcessDate), 8, 4);
                local.ProcessDate.Text10 = local.TextMm.Text2 + "/" + local
                  .TextDd.Text2 + "/" + local.TextYyyy.Text4;
                local.CurrencyToText.TotalCurrency = entities.NonDebt.Amount;
                UseFnCabCurrencyToText6();

                // **** Determine increase or decrease adjustment
                if (AsChar(entities.NonDebt.DebtAdjustmentType) == 'I')
                {
                  local.MinusSign.Flag = "";
                }
                else
                {
                  local.MinusSign.Flag = "-";
                }

                if (export.Debt.Index + 1 >= Export.DebtGroup.Capacity)
                {
                  export.Debt.Index = Export.DebtGroup.Capacity - 1;
                  export.Debt.CheckSize();

                  export.Debt.Update.Debt1.LineText =
                    "**********  MORE DATA EXISTS THAN IS ON THE REPORT - USE FILTERS. ***********";
                    
                  ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

                  return;
                }

                ++export.Debt.Index;
                export.Debt.CheckSize();

                export.Debt.Update.Debt1.LineText = " " + entities
                  .NonDebt.Type1 + "  " + local.Date.Text10 + "               " +
                  local.ProcessDate.Text10 + "" + local.TextCurrency.Text10 + local
                  .MinusSign.Flag + "                                 " + entities
                  .ObligationTransactionRlnRsn.Code + " ";
              }
              else
              {
              }
            }
          }
        }

        local.SuppressDebtHeading.Flag = "N";
      }

      local.Group.CheckIndex();
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDebtDetail1(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
  }

  private static void MoveDebtDetail2(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.RetiredDt = source.RetiredDt;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationType1(ObligationType source,
    ObligationType target)
  {
    target.SupportedPersonReqInd = source.SupportedPersonReqInd;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Classification = source.Classification;
  }

  private static void MoveObligationType2(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.InterstateIndicator = source.InterstateIndicator;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private int UseCabGetYearMonthFromDate1()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.DebtDue.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private int UseCabGetYearMonthFromDate2()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private void UseFnCabCurrencyToText1()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CurrencyToText.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.IntOwed.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText2()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CurrencyToText.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.TotOwed.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText3()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CurrencyToText.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.CurrOwed.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText4()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CurrencyToText.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.ArrsOwed.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText5()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CurrencyToText.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.AmtDue.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText6()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CurrencyToText.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.TextCurrency.Text10 = useExport.WorkArea.Text10;
    local.CollectionAmt.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabCurrencyToText7()
  {
    var useImport = new FnCabCurrencyToText.Import();
    var useExport = new FnCabCurrencyToText.Export();

    useImport.Common.TotalCurrency = local.CurrencyToText.TotalCurrency;

    Call(FnCabCurrencyToText.Execute, useImport, useExport);

    local.CollectionAmt.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnDetCaseNoAndWrkrForDbt()
  {
    var useImport = new FnDetCaseNoAndWrkrForDbt.Import();
    var useExport = new FnDetCaseNoAndWrkrForDbt.Export();

    useImport.Obligor.Number = entities.ObligorCsePerson.Number;
    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;
    useImport.Supported.Number = entities.SupportedPerson.Number;

    Call(FnDetCaseNoAndWrkrForDbt.Execute, useImport, useExport);

    local.Case1.Number = useExport.Case1.Number;
    MoveServiceProvider(useExport.ServiceProvider, local.ServiceProvider);
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    MoveObligationType2(entities.ObligationType, useImport.ObligationType);
    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;
    useImport.SupportedPerson.Number = entities.SupportedPerson.Number;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    MoveProgram(useExport.Program, local.DistributionPgm);
    local.DprProgram.ProgramState = useExport.DprProgram.ProgramState;
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Supported.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Supported.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private bool ReadCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 4);
        entities.CashReceipt.ReferenceNumber = db.GetNullableString(reader, 5);
        entities.CashReceipt.Populated = true;
      });
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
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.CashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.
          SetInt32(command, "crSrceTypeId", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
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

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId",
          local.Group.Item.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "otrType", local.Group.Item.ObligationTransaction.Type1);
        db.SetInt32(
          command, "obgId",
          local.Group.Item.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId",
          local.Group.Item.ObligationType.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligorCsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.CarId = db.GetNullableInt32(reader, 14);
        entities.Collection.OtyId = db.GetInt32(reader, 15);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 16);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 17);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 18);
        entities.Collection.Amount = db.GetDecimal(reader, 19);
        entities.Collection.DistributionMethod = db.GetString(reader, 20);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 21);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 22);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 23);
        entities.Collection.Populated = true;

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
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ObligorCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SearchCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ObligorCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.SupportedPerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.ObligationTransaction.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SupportedPerson.Number = db.GetString(reader, 0);
        entities.SupportedPerson.Type1 = db.GetString(reader, 1);
        entities.SupportedPerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.ObligorCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "type", local.HardcodeObligor.Type1);
        db.SetString(command, "cspNumber", entities.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligorCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.ObligorCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePersonAccount.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.SearchLegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadObligationObligationTransactionDebtDetailObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.Obligation.Populated = false;
    entities.ObligationType.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach(
      "ReadObligationObligationTransactionDebtDetailObligationType",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligorCsePersonAccount.CspNumber);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.LowObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HighObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.LowObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HighObligationType.SystemGeneratedIdentifier);
        db.SetDate(command, "dueDt1", import.SearchTo.Date.GetValueOrDefault());
        db.
          SetDate(command, "dueDt2", import.SearchFrom.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 0);
        entities.DebtDetail.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 6);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 7);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 8);
        entities.DebtDetail.OtrType = db.GetString(reader, 8);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 9);
        entities.ObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 10);
        entities.ObligationTransaction.CreatedTmst = db.GetDateTime(reader, 11);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 12);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.DebtDetail.DueDt = db.GetDate(reader, 15);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 16);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 17);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 18);
        entities.ObligationType.Code = db.GetString(reader, 19);
        entities.ObligationType.Classification = db.GetString(reader, 20);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 21);
        entities.Obligation.Populated = true;
        entities.ObligationType.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.DebtDetail.Populated = true;

        return true;
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.
      Assert(entities.ObligationTransactionRln.Populated);
    entities.NonDebt.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ObligationTransactionRln.OtyTypeSecondary);
        db.SetString(
          command, "obTrnTyp", entities.ObligationTransactionRln.OtrType);
        db.SetInt32(
          command, "obTrnId", entities.ObligationTransactionRln.OtrGeneratedId);
          
        db.SetString(
          command, "cpaType", entities.ObligationTransactionRln.CpaType);
        db.SetString(
          command, "cspNumber", entities.ObligationTransactionRln.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransactionRln.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.NonDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.NonDebt.CspNumber = db.GetString(reader, 1);
        entities.NonDebt.CpaType = db.GetString(reader, 2);
        entities.NonDebt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.NonDebt.Type1 = db.GetString(reader, 4);
        entities.NonDebt.Amount = db.GetDecimal(reader, 5);
        entities.NonDebt.DebtAdjustmentType = db.GetString(reader, 6);
        entities.NonDebt.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.NonDebt.CreatedTmst = db.GetDateTime(reader, 8);
        entities.NonDebt.CspSupNumber = db.GetNullableString(reader, 9);
        entities.NonDebt.CpaSupType = db.GetNullableString(reader, 10);
        entities.NonDebt.OtyType = db.GetInt32(reader, 11);
        entities.NonDebt.DebtAdjustmentProcessDate = db.GetDate(reader, 12);
        entities.NonDebt.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.ObligationTransactionRln.Populated = false;

    return ReadEach("ReadObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrPGeneratedId",
          local.Group.Item.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "otrPType", local.Group.Item.ObligationTransaction.Type1);
        db.SetInt32(
          command, "obgPGeneratedId",
          local.Group.Item.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyTypePrimary",
          local.Group.Item.ObligationType.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaPType", entities.ObligorCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspPNumber", entities.ObligorCsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 1);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 3);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 4);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 5);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 6);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 7);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 8);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 9);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 10);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationTransactionRln.CreatedTmst =
          db.GetDateTime(reader, 12);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 13);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 14);
        entities.ObligationTransactionRln.Populated = true;

        return true;
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    System.Diagnostics.Debug.
      Assert(entities.ObligationTransactionRln.Populated);
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          entities.ObligationTransactionRln.OnrGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Code = db.GetString(reader, 1);
        entities.ObligationTransactionRlnRsn.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNo", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
    }

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
    /// A value of SearchObligationType.
    /// </summary>
    [JsonPropertyName("searchObligationType")]
    public ObligationType SearchObligationType
    {
      get => searchObligationType ??= new();
      set => searchObligationType = value;
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
    /// A value of SearchShowDebtAdj.
    /// </summary>
    [JsonPropertyName("searchShowDebtAdj")]
    public TextWorkArea SearchShowDebtAdj
    {
      get => searchShowDebtAdj ??= new();
      set => searchShowDebtAdj = value;
    }

    /// <summary>
    /// A value of SearchShowCollAdj.
    /// </summary>
    [JsonPropertyName("searchShowCollAdj")]
    public TextWorkArea SearchShowCollAdj
    {
      get => searchShowCollAdj ??= new();
      set => searchShowCollAdj = value;
    }

    /// <summary>
    /// A value of ListDebtsWithAmtOwed.
    /// </summary>
    [JsonPropertyName("listDebtsWithAmtOwed")]
    public Common ListDebtsWithAmtOwed
    {
      get => listDebtsWithAmtOwed ??= new();
      set => listDebtsWithAmtOwed = value;
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

    private CsePerson searchCsePerson;
    private LegalAction searchLegalAction;
    private Obligation searchObligation;
    private ObligationType searchObligationType;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private TextWorkArea searchShowDebtAdj;
    private TextWorkArea searchShowCollAdj;
    private Common listDebtsWithAmtOwed;
    private DateWorkArea current;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DebtGroup group.</summary>
    [Serializable]
    public class DebtGroup
    {
      /// <summary>
      /// A value of Debt1.
      /// </summary>
      [JsonPropertyName("debt1")]
      public ReportData Debt1
      {
        get => debt1 ??= new();
        set => debt1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private ReportData debt1;
    }

    /// <summary>
    /// Gets a value of Debt.
    /// </summary>
    [JsonIgnore]
    public Array<DebtGroup> Debt => debt ??= new(DebtGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Debt for json serialization.
    /// </summary>
    [JsonPropertyName("debt")]
    [Computed]
    public IList<DebtGroup> Debt_Json
    {
      get => debt;
      set => Debt.Assign(value);
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    private Array<DebtGroup> debt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ScreenOwedAmounts screenOwedAmounts;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
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
      /// A value of SupportedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("supportedCsePersonsWorkSet")]
      public CsePersonsWorkSet SupportedCsePersonsWorkSet
      {
        get => supportedCsePersonsWorkSet ??= new();
        set => supportedCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ProgramScreenAttributes.
      /// </summary>
      [JsonPropertyName("programScreenAttributes")]
      public ProgramScreenAttributes ProgramScreenAttributes
      {
        get => programScreenAttributes ??= new();
        set => programScreenAttributes = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>
      /// A value of DprProgram.
      /// </summary>
      [JsonPropertyName("dprProgram")]
      public DprProgram DprProgram
      {
        get => dprProgram ??= new();
        set => dprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2000;

      private Common common;
      private Obligation obligation;
      private ObligationType obligationType;
      private ObligationTransaction obligationTransaction;
      private DebtDetail debtDetail;
      private LegalAction legalAction;
      private CsePerson supportedCsePerson;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private ProgramScreenAttributes programScreenAttributes;
      private Case1 case1;
      private ServiceProvider serviceProvider;
      private DprProgram dprProgram;
    }

    /// <summary>
    /// A value of HighObligationType.
    /// </summary>
    [JsonPropertyName("highObligationType")]
    public ObligationType HighObligationType
    {
      get => highObligationType ??= new();
      set => highObligationType = value;
    }

    /// <summary>
    /// A value of LowObligationType.
    /// </summary>
    [JsonPropertyName("lowObligationType")]
    public ObligationType LowObligationType
    {
      get => lowObligationType ??= new();
      set => lowObligationType = value;
    }

    /// <summary>
    /// A value of HighObligation.
    /// </summary>
    [JsonPropertyName("highObligation")]
    public Obligation HighObligation
    {
      get => highObligation ??= new();
      set => highObligation = value;
    }

    /// <summary>
    /// A value of LowObligation.
    /// </summary>
    [JsonPropertyName("lowObligation")]
    public Obligation LowObligation
    {
      get => lowObligation ??= new();
      set => lowObligation = value;
    }

    /// <summary>
    /// A value of AppliedToLiteral.
    /// </summary>
    [JsonPropertyName("appliedToLiteral")]
    public WorkArea AppliedToLiteral
    {
      get => appliedToLiteral ??= new();
      set => appliedToLiteral = value;
    }

    /// <summary>
    /// A value of DistMethodLiteral.
    /// </summary>
    [JsonPropertyName("distMethodLiteral")]
    public WorkArea DistMethodLiteral
    {
      get => distMethodLiteral ??= new();
      set => distMethodLiteral = value;
    }

    /// <summary>
    /// A value of ProgramLiteral.
    /// </summary>
    [JsonPropertyName("programLiteral")]
    public WorkArea ProgramLiteral
    {
      get => programLiteral ??= new();
      set => programLiteral = value;
    }

    /// <summary>
    /// A value of TextObligationId.
    /// </summary>
    [JsonPropertyName("textObligationId")]
    public WorkArea TextObligationId
    {
      get => textObligationId ??= new();
      set => textObligationId = value;
    }

    /// <summary>
    /// A value of DebtDtlDueDate.
    /// </summary>
    [JsonPropertyName("debtDtlDueDate")]
    public WorkArea DebtDtlDueDate
    {
      get => debtDtlDueDate ??= new();
      set => debtDtlDueDate = value;
    }

    /// <summary>
    /// A value of IntOwed.
    /// </summary>
    [JsonPropertyName("intOwed")]
    public TextWorkArea IntOwed
    {
      get => intOwed ??= new();
      set => intOwed = value;
    }

    /// <summary>
    /// A value of TotOwed.
    /// </summary>
    [JsonPropertyName("totOwed")]
    public TextWorkArea TotOwed
    {
      get => totOwed ??= new();
      set => totOwed = value;
    }

    /// <summary>
    /// A value of CurrOwed.
    /// </summary>
    [JsonPropertyName("currOwed")]
    public TextWorkArea CurrOwed
    {
      get => currOwed ??= new();
      set => currOwed = value;
    }

    /// <summary>
    /// A value of ArrsOwed.
    /// </summary>
    [JsonPropertyName("arrsOwed")]
    public TextWorkArea ArrsOwed
    {
      get => arrsOwed ??= new();
      set => arrsOwed = value;
    }

    /// <summary>
    /// A value of AmtDue.
    /// </summary>
    [JsonPropertyName("amtDue")]
    public TextWorkArea AmtDue
    {
      get => amtDue ??= new();
      set => amtDue = value;
    }

    /// <summary>
    /// A value of CollectionAmt.
    /// </summary>
    [JsonPropertyName("collectionAmt")]
    public TextWorkArea CollectionAmt
    {
      get => collectionAmt ??= new();
      set => collectionAmt = value;
    }

    /// <summary>
    /// A value of TextCurrency.
    /// </summary>
    [JsonPropertyName("textCurrency")]
    public WorkArea TextCurrency
    {
      get => textCurrency ??= new();
      set => textCurrency = value;
    }

    /// <summary>
    /// A value of CurrencyToText.
    /// </summary>
    [JsonPropertyName("currencyToText")]
    public Common CurrencyToText
    {
      get => currencyToText ??= new();
      set => currencyToText = value;
    }

    /// <summary>
    /// A value of SuppressDebtHeading.
    /// </summary>
    [JsonPropertyName("suppressDebtHeading")]
    public Common SuppressDebtHeading
    {
      get => suppressDebtHeading ??= new();
      set => suppressDebtHeading = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Obligation Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of DistributionPgm.
    /// </summary>
    [JsonPropertyName("distributionPgm")]
    public Program DistributionPgm
    {
      get => distributionPgm ??= new();
      set => distributionPgm = value;
    }

    /// <summary>
    /// A value of InitializedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("initializedCsePersonsWorkSet")]
    public CsePersonsWorkSet InitializedCsePersonsWorkSet
    {
      get => initializedCsePersonsWorkSet ??= new();
      set => initializedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of InitializedLegalAction.
    /// </summary>
    [JsonPropertyName("initializedLegalAction")]
    public LegalAction InitializedLegalAction
    {
      get => initializedLegalAction ??= new();
      set => initializedLegalAction = value;
    }

    /// <summary>
    /// A value of ProgramScreenAttributes.
    /// </summary>
    [JsonPropertyName("programScreenAttributes")]
    public ProgramScreenAttributes ProgramScreenAttributes
    {
      get => programScreenAttributes ??= new();
      set => programScreenAttributes = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public ObligationType Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public ScreenOwedAmounts Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of AdjReasonCode.
    /// </summary>
    [JsonPropertyName("adjReasonCode")]
    public ListScreenWorkArea AdjReasonCode
    {
      get => adjReasonCode ??= new();
      set => adjReasonCode = value;
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
    /// A value of CollectionDateDateWorkArea.
    /// </summary>
    [JsonPropertyName("collectionDateDateWorkArea")]
    public DateWorkArea CollectionDateDateWorkArea
    {
      get => collectionDateDateWorkArea ??= new();
      set => collectionDateDateWorkArea = value;
    }

    /// <summary>
    /// A value of EffectiveDate.
    /// </summary>
    [JsonPropertyName("effectiveDate")]
    public DateWorkArea EffectiveDate
    {
      get => effectiveDate ??= new();
      set => effectiveDate = value;
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
    /// A value of MinusSign.
    /// </summary>
    [JsonPropertyName("minusSign")]
    public Common MinusSign
    {
      get => minusSign ??= new();
      set => minusSign = value;
    }

    /// <summary>
    /// A value of DebtDue.
    /// </summary>
    [JsonPropertyName("debtDue")]
    public DateWorkArea DebtDue
    {
      get => debtDue ??= new();
      set => debtDue = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
    }

    /// <summary>
    /// A value of HardcodeSupported.
    /// </summary>
    [JsonPropertyName("hardcodeSupported")]
    public CsePersonAccount HardcodeSupported
    {
      get => hardcodeSupported ??= new();
      set => hardcodeSupported = value;
    }

    /// <summary>
    /// A value of HardcodeAccount.
    /// </summary>
    [JsonPropertyName("hardcodeAccount")]
    public MonthlyObligorSummary HardcodeAccount
    {
      get => hardcodeAccount ??= new();
      set => hardcodeAccount = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    /// <summary>
    /// A value of TextMm.
    /// </summary>
    [JsonPropertyName("textMm")]
    public TextWorkArea TextMm
    {
      get => textMm ??= new();
      set => textMm = value;
    }

    /// <summary>
    /// A value of TextDd.
    /// </summary>
    [JsonPropertyName("textDd")]
    public TextWorkArea TextDd
    {
      get => textDd ??= new();
      set => textDd = value;
    }

    /// <summary>
    /// A value of TextYyyy.
    /// </summary>
    [JsonPropertyName("textYyyy")]
    public TextWorkArea TextYyyy
    {
      get => textYyyy ??= new();
      set => textYyyy = value;
    }

    /// <summary>
    /// A value of CollectionDateWorkArea.
    /// </summary>
    [JsonPropertyName("collectionDateWorkArea")]
    public WorkArea CollectionDateWorkArea
    {
      get => collectionDateWorkArea ??= new();
      set => collectionDateWorkArea = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public WorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of CollAdjDate.
    /// </summary>
    [JsonPropertyName("collAdjDate")]
    public WorkArea CollAdjDate
    {
      get => collAdjDate ??= new();
      set => collAdjDate = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public WorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    private ObligationType highObligationType;
    private ObligationType lowObligationType;
    private Obligation highObligation;
    private Obligation lowObligation;
    private WorkArea appliedToLiteral;
    private WorkArea distMethodLiteral;
    private WorkArea programLiteral;
    private WorkArea textObligationId;
    private WorkArea debtDtlDueDate;
    private TextWorkArea intOwed;
    private TextWorkArea totOwed;
    private TextWorkArea currOwed;
    private TextWorkArea arrsOwed;
    private TextWorkArea amtDue;
    private TextWorkArea collectionAmt;
    private WorkArea textCurrency;
    private Common currencyToText;
    private Common suppressDebtHeading;
    private Obligation previous;
    private ProgramProcessingInfo programProcessingInfo;
    private Program distributionPgm;
    private CsePersonsWorkSet initializedCsePersonsWorkSet;
    private LegalAction initializedLegalAction;
    private ProgramScreenAttributes programScreenAttributes;
    private DateWorkArea dateWorkArea;
    private DebtDetail debtDetail;
    private DateWorkArea zero;
    private ObligationType required;
    private ScreenOwedAmounts obligation;
    private LegalAction legalAction;
    private ListScreenWorkArea adjReasonCode;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private DateWorkArea collectionDateDateWorkArea;
    private DateWorkArea effectiveDate;
    private CashReceiptSourceType cashReceiptSourceType;
    private Common minusSign;
    private DateWorkArea debtDue;
    private CsePersonsWorkSet supported;
    private Case1 case1;
    private ServiceProvider serviceProvider;
    private DateWorkArea current;
    private CsePersonAccount hardcodeObligor;
    private CsePersonAccount hardcodeSupported;
    private MonthlyObligorSummary hardcodeAccount;
    private Array<GroupGroup> group;
    private DprProgram dprProgram;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
    private WorkArea collectionDateWorkArea;
    private WorkArea processDate;
    private WorkArea collAdjDate;
    private WorkArea date;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of NonDebt.
    /// </summary>
    [JsonPropertyName("nonDebt")]
    public ObligationTransaction NonDebt
    {
      get => nonDebt ??= new();
      set => nonDebt = value;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of SupportedPerson.
    /// </summary>
    [JsonPropertyName("supportedPerson")]
    public CsePerson SupportedPerson
    {
      get => supportedPerson ??= new();
      set => supportedPerson = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
    }

    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private CsePersonAccount supported;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CsePerson obligorCsePerson;
    private CsePersonAccount obligorCsePersonAccount;
    private Obligation obligation;
    private ObligationType obligationType;
    private Collection collection;
    private ObligationTransaction obligationTransaction;
    private ObligationTransaction nonDebt;
    private DebtDetail debtDetail;
    private ObligationTransactionRln obligationTransactionRln;
    private LegalAction legalAction;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CsePerson supportedPerson;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private ObligationAssignment obligationAssignment;
  }
#endregion
}
