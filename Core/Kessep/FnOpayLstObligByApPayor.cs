// Program: FN_OPAY_LST_OBLIG_BY_AP_PAYOR, ID: 371947901, model: 746.
// Short name: SWEOPAYP
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
/// A program: FN_OPAY_LST_OBLIG_BY_AP_PAYOR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOpayLstObligByApPayor: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OPAY_LST_OBLIG_BY_AP_PAYOR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOpayLstObligByApPayor(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOpayLstObligByApPayor.
  /// </summary>
  public FnOpayLstObligByApPayor(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG 
    // ***********************************
    // AUTHOR    	 DATE  	  DESCRIPTION
    // D.M.Nielson  	07/21/95  Initial Code
    // T.O.Redmond	02/19/96  Retrofit
    // H.Kennedy	07/22/96  Made changes from Sign off meeting.
    // R. Welborn    	09/09/96  EAB zero fill.
    // R. Welborn    	09/17/96  Flow to PREL.
    // H. Kennedy    	10/18/96  Data lvl security
    // 		10/24/96  Fix logic to display the periodic amount Fix read to display 
    // worker ID. Fix Active Inactive problem. Fixed Monthly due totals.  The
    // Non accruing was being added to the accruing.  The non accruing should
    // have been displayed as Periodic amount due.
    // : 07/23/1997  Paul R. Egger  Added link to OFEE.
    // : 10/22/1998  E. Parker      Corrected logic so details lines will remain
    // when next tran or show history errors occur; changed Sel line to green;
    // made amount on detail lines and undistributed amount blank when zero;
    // expanded Type to 7 characters; took out LINT logic; made unavailable PF
    // keys invalid; made ENTER an invalid command; added logic to check for Y
    // or N on Show History; added logic to require selection on flow to PREL;
    // set command to DISPLAY on return from ONAC; enabled flow to PAYR w/o
    // select.
    // : 03/30/1999  A Doty         Modified the logic to support the 
    // calculation of summary totals real-time.  Removed the use of the Summary
    // Entities.
    // : 08/03/2000  E Lyman        Fixed Next Tran.
    // : 09/12/2000  Madhu Kumar added PF keys for direct flows to DEBT and 
    // PAYR.
    // : 12/13/00 swsrchf     000238      Return super "NEXT TRAN" to ALRT
    // : 11/07/01 swsrkxd     PR131586    Fix screen help Id problem.
    // ******** MAINTENANCE LOG 
    // ***********************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.SelectFound.Flag = "";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // Move Imports to Exports
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.ShowInactiveFlag.SelectChar = import.ShowInactiveFlag.SelectChar;
    export.Passed.Number = import.CsePersonsWorkSet.Number;
    export.CurrentSupportOwed.TotalCurrency =
      import.CurrentSupportOwed.TotalCurrency;
    export.TotalArrearsOwed.TotalCurrency =
      import.TotalArrearsOwed.TotalCurrency;
    export.TotalInterestOwed.TotalCurrency =
      import.TotalInterestOwed.TotalCurrency;
    export.TotalMonthlyOwed.TotalCurrency =
      import.TotalMonthlyOwed.TotalCurrency;
    export.HiddenDisplayed.Number = import.HiddenDisplayed.Number;
    export.CurrentSupportDue.TotalCurrency =
      import.CurrentSupportDue.TotalCurrency;
    export.PeriodicAmountDue.TotalCurrency =
      import.PeriodicAmountDue.TotalCurrency;
    export.TotalMonthlyDue.TotalCurrency = import.TotalMonthlyDue.TotalCurrency;
    export.Undistributed.TotalCurrency = import.Undistributed.TotalCurrency;
    MoveStandard(import.Pf16FlowTo, export.Pf16FlowTo);

    if (IsEmpty(export.ShowInactiveFlag.SelectChar))
    {
      export.ShowInactiveFlag.SelectChar = "N";
    }

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.ZeroFill.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.CsePersonsWorkSet.Number = local.ZeroFill.Text10;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumber ?? Spaces
        (10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "BYPASS"))
    {
      // *** ONAC is only flow to OPAY that sets command to bypass.  Will set 
      // command to DISPLAY until flow can be corrected. ***
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailMonthlyDue.TotalCurrency =
        import.Import1.Item.DetailMonthlyDue.TotalCurrency;
      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      export.Export1.Update.DetailDebtDetailStatusHistory.Code =
        import.Import1.Item.DetailDebtDetailStatusHistory.Code;
      export.Export1.Update.DetailLegalAction.Assign(
        import.Import1.Item.DetailLegalAction);
      export.Export1.Update.DetailObligation.SystemGeneratedIdentifier =
        import.Import1.Item.DetailObligation.SystemGeneratedIdentifier;
      MoveObligationTransaction(import.Import1.Item.DetailObligationTransaction,
        export.Export1.Update.DetailObligationTransaction);
      export.Export1.Update.DetailObligationType.Assign(
        import.Import1.Item.DetailObligationType);
      export.Export1.Update.DetailServiceProvider.UserId =
        import.Import1.Item.DetailServiceProvider.UserId;
      export.Export1.Update.DetailMultipleSp.SelectChar =
        import.Import1.Item.DetailMultipleSp.SelectChar;
      export.Export1.Update.DetailArrearsOwed.TotalCurrency =
        import.Import1.Item.DetailArrearsOwed.TotalCurrency;
      export.Export1.Update.DetailCurrentOwed.TotalCurrency =
        import.Import1.Item.DetailCurrentOwed.TotalCurrency;
      export.Export1.Update.DetailIntrestOwed.TotalCurrency =
        import.Import1.Item.DetailIntrestOwed.TotalCurrency;
      export.Export1.Update.DetailTotalOwed.TotalCurrency =
        import.Import1.Item.DetailTotalOwed.TotalCurrency;
      export.Export1.Update.DetailHiddenDark.Flag =
        import.Import1.Item.DetailHiddenDark.Flag;
      MoveObligationPaymentSchedule(import.Import1.Item.
        DetailObligationPaymentSchedule,
        export.Export1.Update.DetailObligationPaymentSchedule);
      export.Export1.Update.DetailAcNac.SelectChar =
        import.Import1.Item.DetailAcNac.SelectChar;
      export.Export1.Update.GexportPriSecAndIntrstInd.State =
        import.Import1.Item.GimportPriSecAndIntrstInd.State;
      export.Export1.Update.DetailConcatInds.Text8 =
        import.Import1.Item.DetailConcatInds.Text8;
      export.Export1.Next();
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
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

    if (Equal(global.Command, "RETURN") || Equal(global.Command, "MAINT") || Equal
      (global.Command, "FLOWSTO") || Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "DEBT") || Equal(global.Command, "PAYR") || Equal
      (global.Command, "LCDA") || Equal(global.Command, "COLP"))
    {
      // *** Work request 000238
      // *** 1213/00 swsrchf
      // *** start
      if (Equal(global.Command, "RETURN"))
      {
        if (Equal(export.Hidden.LastTran, "SRPQ"))
        {
          global.NextTran = (export.Hidden.LastTran ?? "") + " XXNEXTXX";

          return;
        }
      }

      // *** end
      // *** 12/13/00 swsrchf
      // *** Work request 000238
      // : Check for selection
      local.SelectFound.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
        {
          case ' ':
            // : Continue processing
            break;
          case 'S':
            ++local.SelectFound.Count;

            if (local.SelectFound.Count > 1)
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              return;
            }

            if (local.SelectFound.Count == 1)
            {
              // : Move only the first occurance found
              local.SelectFound.Flag = "Y";
              export.SelectedObligation.SystemGeneratedIdentifier =
                export.Export1.Item.DetailObligation.SystemGeneratedIdentifier;
              MoveCsePersonsWorkSet(export.CsePersonsWorkSet,
                export.SelectedCsePersonsWorkSet);
              export.Passed.Number = export.CsePersonsWorkSet.Number;
              export.SelCourtOrderNo.CourtOrderNumber =
                export.Export1.Item.DetailLegalAction.StandardNumber ?? "";
              local.Selected.Assign(export.Export1.Item.DetailObligationType);
              export.PassLegalAction.Assign(
                export.Export1.Item.DetailLegalAction);
              export.PassObligationTransaction.Type1 =
                export.Export1.Item.DetailObligationTransaction.Type1;
              MoveObligationType(export.Export1.Item.DetailObligationType,
                export.PassObligationType);
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            return;
        }
      }
    }

    // *****
    // Data Level security added.
    // *****
    if (Equal(global.Command, "RETNAME") || Equal(global.Command, "MAINT") || Equal
      (global.Command, "BYPASS") || Equal(global.Command, "ENTER") || Equal
      (global.Command, "FLOWSTO") || Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "DEBT") || Equal(global.Command, "PAYR") || Equal
      (global.Command, "LCDA") || Equal(global.Command, "COLP"))
    {
      if (Equal(global.Command, "RETNAME"))
      {
        export.CsePersonsWorkSet.Number = import.FromList.Number;
        global.Command = "DISPLAY";
      }

      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
    }
    else
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

    if (Equal(global.Command, "RETCDVL"))
    {
      if (AsChar(export.Pf16FlowTo.PromptField) == 'S')
      {
        export.Pf16FlowTo.PromptField = "";

        if (!IsEmpty(import.DlgflwSelected.Cdvalue))
        {
          export.Pf16FlowTo.NextTransaction = import.DlgflwSelected.Cdvalue;
          global.Command = "FLOWSTO";
        }
      }
    }

    if (AsChar(export.ShowInactiveFlag.SelectChar) != 'Y' && AsChar
      (export.ShowInactiveFlag.SelectChar) != 'N')
    {
      var field = GetField(export.ShowInactiveFlag, "selectChar");

      field.Error = true;

      ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DEBT":
        // : Link to LIST DEBT ACTIVITY BY AP/PAYOR
        ExitState = "ECO_LNK_LST_DBT_ACT_BY_AP_PYR";

        break;
      case "LCDA":
        ExitState = "ECO_LNK_TO_LCDA";

        break;
      case "COLP":
        ExitState = "ECO_LNK_TO_COLP";

        break;
      case "PAYR":
        ExitState = "ECO_LNK_TO_LST_COLL_BY_AP_PYR";

        break;
      case "RETCDVL":
        break;
      case "BYPASS":
        break;
      case "DISPLAY":
        export.Pf16FlowTo.NextTransaction = "";
        export.Pf16FlowTo.PromptField = "";

        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }
        else
        {
          local.ZeroFill.Text10 = export.CsePersonsWorkSet.Number;
          UseEabPadLeftWithZeros();
          export.CsePersonsWorkSet.Number = local.ZeroFill.Text10;
        }

        export.Passed.Number = export.CsePersonsWorkSet.Number;
        UseFnDisplayObligationsByPayor();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
          {
            var field =
              GetField(export.ScreenOwedAmounts, "errorInformationLine");

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;
          }

          if (export.Export1.IsEmpty)
          {
            ExitState = "FN0000_NO_OBLIGATIONS_FOUND";
          }
          else
          {
            export.HiddenDisplayed.Number = export.CsePersonsWorkSet.Number;

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Protected = false;
              field.Focused = true;

              break;
            }

            if (export.Export1.IsFull)
            {
              ExitState = "FN0000_MORE_DATA_EXISTS";
            }
          }
        }
        else
        {
        }

        break;
      case "FLOWSTO":
        switch(TrimEnd(export.Pf16FlowTo.NextTransaction))
        {
          case "CSPM":
            if (local.SelectFound.Count == 0)
            {
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Protected = false;
                field1.Focused = true;

                break;
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            ExitState = "ECO_LNK_MTN_OBLG_CPN_SUPRESON";

            break;
          case "DBWR":
            // : Link to RECORD ACCRUED ARREARAGE ADJUSTMENT-DBWR
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
              {
                if (AsChar(export.Export1.Item.DetailAcNac.SelectChar) != 'A')
                {
                  ExitState = "FN0000_NON_ACCRUAL_OBLIGATION";

                  return;
                }
              }
            }

            if (local.SelectFound.Count == 0)
            {
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Protected = false;
                field1.Focused = true;

                break;
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            ExitState = "ECO_LNK_TO_REC_ACCRUED_ARR_ADJ_2";

            break;
          case "DEBT":
            // : Link to LIST DEBT ACTIVITY BY AP/PAYOR
            ExitState = "ECO_LNK_LST_DBT_ACT_BY_AP_PYR";

            break;
          case "LCDA":
            ExitState = "ECO_LNK_TO_LCDA";

            break;
          case "COLP":
            ExitState = "ECO_LNK_TO_COLP";

            break;
          case "MDIS":
            if (local.SelectFound.Count == 0)
            {
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Protected = false;
                field1.Focused = true;

                break;
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            ExitState = "ECO_LNK_TO_MDIS";

            break;
          case "OCOL":
            // : Link to LIST COLLECTIONS BY OBLIGATION
            if (local.SelectFound.Count == 0)
            {
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Protected = false;
                field1.Focused = true;

                break;
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            ExitState = "ECO_LNK_TO_LST_COLL_BY_OBLIG";

            break;
          case "OSUM":
            // : Link to DISPLAY OBLIGATION SUMMARY
            if (local.SelectFound.Count == 0)
            {
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Protected = false;
                field1.Focused = true;

                break;
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            ExitState = "ECO_LNK_TO_DSPLY_OBLIG_SUM";

            break;
          case "PREL":
            if (local.SelectFound.Count == 0)
            {
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Protected = false;
                field1.Focused = true;

                break;
              }

              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            }

            ExitState = "ECO_LNK_TO_PREL";

            break;
          case "REIP":
            ExitState = "ECO_LNK_TO_REC_IND_PYMNT_HIST";

            break;
          case "PAYR":
            ExitState = "ECO_LNK_TO_LST_COLL_BY_AP_PYR";

            break;
          case "COMN":
            ExitState = "ECO_LNK_TO_COMN";

            break;
          default:
            var field = GetField(export.Pf16FlowTo, "nextTransaction");

            field.Error = true;

            ExitState = "FN0000_INV_SCREEN_ID";

            break;
        }

        break;
      case "MAINT":
        // : Pass control to the appropriate Obligation Maintenance procedure.
        if (local.SelectFound.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Protected = false;
            field.Focused = true;

            break;
          }

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        switch(AsChar(local.Selected.Classification))
        {
          case 'V':
            // : Link to MAINTAIN VOLUNTARY OBLIGATIONS
            ExitState = "ECO_LNK_TO_MTN_VOLUNTARY_OBLIG";

            break;
          case 'R':
            // Link to MAINTAIN RECOVERY OBLIGATIONS
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
              {
                if (Equal(export.Export1.Item.DetailObligationType.Code, "FEE"))
                {
                  ExitState = "ECO_LNK_TO_OFEE";
                }
                else
                {
                  ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";
                }

                return;
              }
            }

            break;
          case 'A':
            // Link to MAINTAIN ACCRUING OBLIGATION
            ExitState = "ECO_LNK_TO_MTN_ACCRUING_OBLIG";

            break;
          default:
            // Link to NON-ACCRUING OBLIGATION
            if (ReadObligationType())
            {
              if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'Y')
              {
                ExitState = "ECO_LNK_TO_MTN_NON_ACCRUING_OBLG";
              }
              else if (Equal(entities.ObligationType.Code, "FEE"))
              {
                ExitState = "ECO_LNK_TO_OFEE";
              }
              else
              {
                ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";
              }
            }
            else
            {
              ExitState = "FN0000_OBLIG_TYPE_NF";
            }

            break;
        }

        break;
      case "LIST":
        if (!IsEmpty(import.PersonSelect.SelectChar) && !
          IsEmpty(export.UndistCollPrompt.SelectChar))
        {
          var field1 = GetField(export.UndistCollPrompt, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PersonSelect, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (!IsEmpty(import.PersonSelect.SelectChar))
        {
          if (AsChar(import.PersonSelect.SelectChar) == 'S')
          {
            // : Link to SI NAME NAME LIST
            ExitState = "ECO_LNK_TO_SELECT_PERSON";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.PersonSelect, "selectChar");

            field.Error = true;
          }

          return;
        }

        if (!IsEmpty(import.UndistCollPrompt.SelectChar))
        {
          if (AsChar(import.UndistCollPrompt.SelectChar) == 'S')
          {
            export.Passed.Number = import.CsePersonsWorkSet.Number;
            ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.UndistCollPrompt, "selectChar");

            field.Error = true;
          }

          return;
        }

        switch(AsChar(export.Pf16FlowTo.PromptField))
        {
          case ' ':
            break;
          case 'S':
            export.DlgflwRequired.CodeName = "FLOW FROM OPAY";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.Pf16FlowTo, "promptField");

            field.Error = true;

            return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "PREV":
        if (!Equal(export.CsePersonsWorkSet.Number,
          export.HiddenDisplayed.Number))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }
        else
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";
        }

        break;
      case "NEXT":
        if (!Equal(export.CsePersonsWorkSet.Number,
          export.HiddenDisplayed.Number))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }
        else
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
        }

        break;
      case "RETURN":
        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
        }
        else
        {
          export.FlowToKdmv.Number = export.HiddenDisplayed.Number;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1(FnDisplayObligationsByPayor.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailMonthlyDue.TotalCurrency =
      source.DetailMonthlyDue.TotalCurrency;
    target.DetailHiddenDark.Flag = source.DetailDark.Flag;
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.DetailLegalAction.Assign(source.DetailLegalAction);
    target.DetailObligationType.Assign(source.DetailObligationType);
    target.DetailAcNac.SelectChar = source.DetailAcNonAc.SelectChar;
    target.DetailDebtDetailStatusHistory.Code =
      source.DetailDebtDetailStatusHistory.Code;
    target.GexportPriSecAndIntrstInd.State =
      source.GexportPriSecAndIntrstInd.State;
    MoveObligationTransaction(source.DetailObligationTransaction,
      target.DetailObligationTransaction);
    target.DetailServiceProvider.UserId = source.DetailServiceProvider.UserId;
    target.DetailMultipleSp.SelectChar = source.DetailMultipleSp.SelectChar;
    target.DetailCurrentOwed.TotalCurrency =
      source.DetailCurrentOwed.TotalCurrency;
    MoveObligationPaymentSchedule(source.DetailObligationPaymentSchedule,
      target.DetailObligationPaymentSchedule);
    target.DetailArrearsOwed.TotalCurrency =
      source.DetailArrearsOwed.TotalCurrency;
    target.DetailIntrestOwed.TotalCurrency =
      source.DetailIntrestDue.TotalCurrency;
    target.DetailTotalOwed.TotalCurrency = source.DetailTotalDue.TotalCurrency;
    target.DetailObligation.SystemGeneratedIdentifier =
      source.DetailObligation.SystemGeneratedIdentifier;
    target.DetailConcatInds.Text8 = source.DetailConcatInds.Text8;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Classification = source.Classification;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PromptField = source.PromptField;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnDisplayObligationsByPayor()
  {
    var useImport = new FnDisplayObligationsByPayor.Import();
    var useExport = new FnDisplayObligationsByPayor.Export();

    useImport.CsePerson.Number = export.Passed.Number;
    useImport.ShowInactive.SelectChar = import.ShowInactiveFlag.SelectChar;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(FnDisplayObligationsByPayor.Execute, useImport, useExport);

    export.ScreenOwedAmounts.ErrorInformationLine =
      useExport.ScreenOwedAmounts.ErrorInformationLine;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source has greater capacity than target.");
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.TotalMonthlyOwed.TotalCurrency =
      useExport.TotalMonthlyOwed.TotalCurrency;
    export.CurrentSupportOwed.TotalCurrency =
      useExport.CurrentSupportOwed.TotalCurrency;
    export.TotalArrearsOwed.TotalCurrency =
      useExport.TotalArrearsOwed.TotalCurrency;
    export.TotalInterestOwed.TotalCurrency =
      useExport.TotalIntrestOwed.TotalCurrency;
    export.CurrentSupportDue.TotalCurrency =
      useExport.CurrentSupportDue.TotalCurrency;
    export.PeriodicAmountDue.TotalCurrency =
      useExport.PeriodicAmountDue.TotalCurrency;
    export.TotalMonthlyDue.TotalCurrency =
      useExport.TotalMonthlyDue.TotalCurrency;
    export.Undistributed.TotalCurrency = useExport.Undistributed.TotalCurrency;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(export.Hidden);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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

    MoveLegalAction(export.PassLegalAction, useImport.LegalAction);
    useImport.CsePerson.Number = export.Passed.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId", local.Selected.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 6);
        entities.ObligationType.Description = db.GetNullableString(reader, 7);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
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
      /// A value of DetailMonthlyDue.
      /// </summary>
      [JsonPropertyName("detailMonthlyDue")]
      public Common DetailMonthlyDue
      {
        get => detailMonthlyDue ??= new();
        set => detailMonthlyDue = value;
      }

      /// <summary>
      /// A value of DetailHiddenDark.
      /// </summary>
      [JsonPropertyName("detailHiddenDark")]
      public Common DetailHiddenDark
      {
        get => detailHiddenDark ??= new();
        set => detailHiddenDark = value;
      }

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
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailAcNac.
      /// </summary>
      [JsonPropertyName("detailAcNac")]
      public Common DetailAcNac
      {
        get => detailAcNac ??= new();
        set => detailAcNac = value;
      }

      /// <summary>
      /// A value of DetailDebtDetailStatusHistory.
      /// </summary>
      [JsonPropertyName("detailDebtDetailStatusHistory")]
      public DebtDetailStatusHistory DetailDebtDetailStatusHistory
      {
        get => detailDebtDetailStatusHistory ??= new();
        set => detailDebtDetailStatusHistory = value;
      }

      /// <summary>
      /// A value of GimportPriSecAndIntrstInd.
      /// </summary>
      [JsonPropertyName("gimportPriSecAndIntrstInd")]
      public Common GimportPriSecAndIntrstInd
      {
        get => gimportPriSecAndIntrstInd ??= new();
        set => gimportPriSecAndIntrstInd = value;
      }

      /// <summary>
      /// A value of DetailObligationTransaction.
      /// </summary>
      [JsonPropertyName("detailObligationTransaction")]
      public ObligationTransaction DetailObligationTransaction
      {
        get => detailObligationTransaction ??= new();
        set => detailObligationTransaction = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailMultipleSp.
      /// </summary>
      [JsonPropertyName("detailMultipleSp")]
      public Common DetailMultipleSp
      {
        get => detailMultipleSp ??= new();
        set => detailMultipleSp = value;
      }

      /// <summary>
      /// A value of DetailCurrentOwed.
      /// </summary>
      [JsonPropertyName("detailCurrentOwed")]
      public Common DetailCurrentOwed
      {
        get => detailCurrentOwed ??= new();
        set => detailCurrentOwed = value;
      }

      /// <summary>
      /// A value of DetailObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("detailObligationPaymentSchedule")]
      public ObligationPaymentSchedule DetailObligationPaymentSchedule
      {
        get => detailObligationPaymentSchedule ??= new();
        set => detailObligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of DetailArrearsOwed.
      /// </summary>
      [JsonPropertyName("detailArrearsOwed")]
      public Common DetailArrearsOwed
      {
        get => detailArrearsOwed ??= new();
        set => detailArrearsOwed = value;
      }

      /// <summary>
      /// A value of DetailIntrestOwed.
      /// </summary>
      [JsonPropertyName("detailIntrestOwed")]
      public Common DetailIntrestOwed
      {
        get => detailIntrestOwed ??= new();
        set => detailIntrestOwed = value;
      }

      /// <summary>
      /// A value of DetailTotalOwed.
      /// </summary>
      [JsonPropertyName("detailTotalOwed")]
      public Common DetailTotalOwed
      {
        get => detailTotalOwed ??= new();
        set => detailTotalOwed = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailConcatInds.
      /// </summary>
      [JsonPropertyName("detailConcatInds")]
      public TextWorkArea DetailConcatInds
      {
        get => detailConcatInds ??= new();
        set => detailConcatInds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common detailMonthlyDue;
      private Common detailHiddenDark;
      private Common detailCommon;
      private LegalAction detailLegalAction;
      private ObligationType detailObligationType;
      private Common detailAcNac;
      private DebtDetailStatusHistory detailDebtDetailStatusHistory;
      private Common gimportPriSecAndIntrstInd;
      private ObligationTransaction detailObligationTransaction;
      private ServiceProvider detailServiceProvider;
      private Common detailMultipleSp;
      private Common detailCurrentOwed;
      private ObligationPaymentSchedule detailObligationPaymentSchedule;
      private Common detailArrearsOwed;
      private Common detailIntrestOwed;
      private Common detailTotalOwed;
      private Obligation detailObligation;
      private TextWorkArea detailConcatInds;
    }

    /// <summary>
    /// A value of UndistCollPrompt.
    /// </summary>
    [JsonPropertyName("undistCollPrompt")]
    public Common UndistCollPrompt
    {
      get => undistCollPrompt ??= new();
      set => undistCollPrompt = value;
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
    /// A value of ShowInactiveFlag.
    /// </summary>
    [JsonPropertyName("showInactiveFlag")]
    public Common ShowInactiveFlag
    {
      get => showInactiveFlag ??= new();
      set => showInactiveFlag = value;
    }

    /// <summary>
    /// A value of Undistributed.
    /// </summary>
    [JsonPropertyName("undistributed")]
    public Common Undistributed
    {
      get => undistributed ??= new();
      set => undistributed = value;
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
    /// A value of CurrentSupportOwed.
    /// </summary>
    [JsonPropertyName("currentSupportOwed")]
    public Common CurrentSupportOwed
    {
      get => currentSupportOwed ??= new();
      set => currentSupportOwed = value;
    }

    /// <summary>
    /// A value of TotalArrearsOwed.
    /// </summary>
    [JsonPropertyName("totalArrearsOwed")]
    public Common TotalArrearsOwed
    {
      get => totalArrearsOwed ??= new();
      set => totalArrearsOwed = value;
    }

    /// <summary>
    /// A value of TotalInterestOwed.
    /// </summary>
    [JsonPropertyName("totalInterestOwed")]
    public Common TotalInterestOwed
    {
      get => totalInterestOwed ??= new();
      set => totalInterestOwed = value;
    }

    /// <summary>
    /// A value of TotalMonthlyOwed.
    /// </summary>
    [JsonPropertyName("totalMonthlyOwed")]
    public Common TotalMonthlyOwed
    {
      get => totalMonthlyOwed ??= new();
      set => totalMonthlyOwed = value;
    }

    /// <summary>
    /// A value of HiddenDisplayed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayed")]
    public CsePersonsWorkSet HiddenDisplayed
    {
      get => hiddenDisplayed ??= new();
      set => hiddenDisplayed = value;
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
    /// A value of PersonSelect.
    /// </summary>
    [JsonPropertyName("personSelect")]
    public Common PersonSelect
    {
      get => personSelect ??= new();
      set => personSelect = value;
    }

    /// <summary>
    /// A value of CurrentSupportDue.
    /// </summary>
    [JsonPropertyName("currentSupportDue")]
    public Common CurrentSupportDue
    {
      get => currentSupportDue ??= new();
      set => currentSupportDue = value;
    }

    /// <summary>
    /// A value of PeriodicAmountDue.
    /// </summary>
    [JsonPropertyName("periodicAmountDue")]
    public Common PeriodicAmountDue
    {
      get => periodicAmountDue ??= new();
      set => periodicAmountDue = value;
    }

    /// <summary>
    /// A value of TotalMonthlyDue.
    /// </summary>
    [JsonPropertyName("totalMonthlyDue")]
    public Common TotalMonthlyDue
    {
      get => totalMonthlyDue ??= new();
      set => totalMonthlyDue = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    /// <summary>
    /// A value of FromList.
    /// </summary>
    [JsonPropertyName("fromList")]
    public CsePerson FromList
    {
      get => fromList ??= new();
      set => fromList = value;
    }

    /// <summary>
    /// A value of Pf16FlowTo.
    /// </summary>
    [JsonPropertyName("pf16FlowTo")]
    public Standard Pf16FlowTo
    {
      get => pf16FlowTo ??= new();
      set => pf16FlowTo = value;
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    private Common undistCollPrompt;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common showInactiveFlag;
    private Common undistributed;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common currentSupportOwed;
    private Common totalArrearsOwed;
    private Common totalInterestOwed;
    private Common totalMonthlyOwed;
    private CsePersonsWorkSet hiddenDisplayed;
    private Array<ImportGroup> import1;
    private Common personSelect;
    private Common currentSupportDue;
    private Common periodicAmountDue;
    private Common totalMonthlyDue;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePerson fromList;
    private Standard pf16FlowTo;
    private CodeValue dlgflwSelected;
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
      /// A value of DetailMonthlyDue.
      /// </summary>
      [JsonPropertyName("detailMonthlyDue")]
      public Common DetailMonthlyDue
      {
        get => detailMonthlyDue ??= new();
        set => detailMonthlyDue = value;
      }

      /// <summary>
      /// A value of DetailHiddenDark.
      /// </summary>
      [JsonPropertyName("detailHiddenDark")]
      public Common DetailHiddenDark
      {
        get => detailHiddenDark ??= new();
        set => detailHiddenDark = value;
      }

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
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailAcNac.
      /// </summary>
      [JsonPropertyName("detailAcNac")]
      public Common DetailAcNac
      {
        get => detailAcNac ??= new();
        set => detailAcNac = value;
      }

      /// <summary>
      /// A value of DetailDebtDetailStatusHistory.
      /// </summary>
      [JsonPropertyName("detailDebtDetailStatusHistory")]
      public DebtDetailStatusHistory DetailDebtDetailStatusHistory
      {
        get => detailDebtDetailStatusHistory ??= new();
        set => detailDebtDetailStatusHistory = value;
      }

      /// <summary>
      /// A value of GexportPriSecAndIntrstInd.
      /// </summary>
      [JsonPropertyName("gexportPriSecAndIntrstInd")]
      public Common GexportPriSecAndIntrstInd
      {
        get => gexportPriSecAndIntrstInd ??= new();
        set => gexportPriSecAndIntrstInd = value;
      }

      /// <summary>
      /// A value of DetailObligationTransaction.
      /// </summary>
      [JsonPropertyName("detailObligationTransaction")]
      public ObligationTransaction DetailObligationTransaction
      {
        get => detailObligationTransaction ??= new();
        set => detailObligationTransaction = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailMultipleSp.
      /// </summary>
      [JsonPropertyName("detailMultipleSp")]
      public Common DetailMultipleSp
      {
        get => detailMultipleSp ??= new();
        set => detailMultipleSp = value;
      }

      /// <summary>
      /// A value of DetailCurrentOwed.
      /// </summary>
      [JsonPropertyName("detailCurrentOwed")]
      public Common DetailCurrentOwed
      {
        get => detailCurrentOwed ??= new();
        set => detailCurrentOwed = value;
      }

      /// <summary>
      /// A value of DetailObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("detailObligationPaymentSchedule")]
      public ObligationPaymentSchedule DetailObligationPaymentSchedule
      {
        get => detailObligationPaymentSchedule ??= new();
        set => detailObligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of DetailArrearsOwed.
      /// </summary>
      [JsonPropertyName("detailArrearsOwed")]
      public Common DetailArrearsOwed
      {
        get => detailArrearsOwed ??= new();
        set => detailArrearsOwed = value;
      }

      /// <summary>
      /// A value of DetailIntrestOwed.
      /// </summary>
      [JsonPropertyName("detailIntrestOwed")]
      public Common DetailIntrestOwed
      {
        get => detailIntrestOwed ??= new();
        set => detailIntrestOwed = value;
      }

      /// <summary>
      /// A value of DetailTotalOwed.
      /// </summary>
      [JsonPropertyName("detailTotalOwed")]
      public Common DetailTotalOwed
      {
        get => detailTotalOwed ??= new();
        set => detailTotalOwed = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailConcatInds.
      /// </summary>
      [JsonPropertyName("detailConcatInds")]
      public TextWorkArea DetailConcatInds
      {
        get => detailConcatInds ??= new();
        set => detailConcatInds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private Common detailMonthlyDue;
      private Common detailHiddenDark;
      private Common detailCommon;
      private LegalAction detailLegalAction;
      private ObligationType detailObligationType;
      private Common detailAcNac;
      private DebtDetailStatusHistory detailDebtDetailStatusHistory;
      private Common gexportPriSecAndIntrstInd;
      private ObligationTransaction detailObligationTransaction;
      private ServiceProvider detailServiceProvider;
      private Common detailMultipleSp;
      private Common detailCurrentOwed;
      private ObligationPaymentSchedule detailObligationPaymentSchedule;
      private Common detailArrearsOwed;
      private Common detailIntrestOwed;
      private Common detailTotalOwed;
      private Obligation detailObligation;
      private TextWorkArea detailConcatInds;
    }

    /// <summary>
    /// A value of SelCourtOrderNo.
    /// </summary>
    [JsonPropertyName("selCourtOrderNo")]
    public CashReceiptDetail SelCourtOrderNo
    {
      get => selCourtOrderNo ??= new();
      set => selCourtOrderNo = value;
    }

    /// <summary>
    /// A value of UndistCollPrompt.
    /// </summary>
    [JsonPropertyName("undistCollPrompt")]
    public Common UndistCollPrompt
    {
      get => undistCollPrompt ??= new();
      set => undistCollPrompt = value;
    }

    /// <summary>
    /// A value of PassObligationType.
    /// </summary>
    [JsonPropertyName("passObligationType")]
    public ObligationType PassObligationType
    {
      get => passObligationType ??= new();
      set => passObligationType = value;
    }

    /// <summary>
    /// A value of PassObligationTransaction.
    /// </summary>
    [JsonPropertyName("passObligationTransaction")]
    public ObligationTransaction PassObligationTransaction
    {
      get => passObligationTransaction ??= new();
      set => passObligationTransaction = value;
    }

    /// <summary>
    /// A value of PassLegalAction.
    /// </summary>
    [JsonPropertyName("passLegalAction")]
    public LegalAction PassLegalAction
    {
      get => passLegalAction ??= new();
      set => passLegalAction = value;
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
    /// A value of ShowInactiveFlag.
    /// </summary>
    [JsonPropertyName("showInactiveFlag")]
    public Common ShowInactiveFlag
    {
      get => showInactiveFlag ??= new();
      set => showInactiveFlag = value;
    }

    /// <summary>
    /// A value of Undistributed.
    /// </summary>
    [JsonPropertyName("undistributed")]
    public Common Undistributed
    {
      get => undistributed ??= new();
      set => undistributed = value;
    }

    /// <summary>
    /// A value of Passed.
    /// </summary>
    [JsonPropertyName("passed")]
    public CsePerson Passed
    {
      get => passed ??= new();
      set => passed = value;
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
    /// A value of CurrentSupportOwed.
    /// </summary>
    [JsonPropertyName("currentSupportOwed")]
    public Common CurrentSupportOwed
    {
      get => currentSupportOwed ??= new();
      set => currentSupportOwed = value;
    }

    /// <summary>
    /// A value of TotalArrearsOwed.
    /// </summary>
    [JsonPropertyName("totalArrearsOwed")]
    public Common TotalArrearsOwed
    {
      get => totalArrearsOwed ??= new();
      set => totalArrearsOwed = value;
    }

    /// <summary>
    /// A value of TotalInterestOwed.
    /// </summary>
    [JsonPropertyName("totalInterestOwed")]
    public Common TotalInterestOwed
    {
      get => totalInterestOwed ??= new();
      set => totalInterestOwed = value;
    }

    /// <summary>
    /// A value of TotalMonthlyOwed.
    /// </summary>
    [JsonPropertyName("totalMonthlyOwed")]
    public Common TotalMonthlyOwed
    {
      get => totalMonthlyOwed ??= new();
      set => totalMonthlyOwed = value;
    }

    /// <summary>
    /// A value of HiddenDisplayed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayed")]
    public CsePersonsWorkSet HiddenDisplayed
    {
      get => hiddenDisplayed ??= new();
      set => hiddenDisplayed = value;
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
    /// A value of PersonSelect.
    /// </summary>
    [JsonPropertyName("personSelect")]
    public Common PersonSelect
    {
      get => personSelect ??= new();
      set => personSelect = value;
    }

    /// <summary>
    /// A value of SelectedObligation.
    /// </summary>
    [JsonPropertyName("selectedObligation")]
    public Obligation SelectedObligation
    {
      get => selectedObligation ??= new();
      set => selectedObligation = value;
    }

    /// <summary>
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CurrentSupportDue.
    /// </summary>
    [JsonPropertyName("currentSupportDue")]
    public Common CurrentSupportDue
    {
      get => currentSupportDue ??= new();
      set => currentSupportDue = value;
    }

    /// <summary>
    /// A value of PeriodicAmountDue.
    /// </summary>
    [JsonPropertyName("periodicAmountDue")]
    public Common PeriodicAmountDue
    {
      get => periodicAmountDue ??= new();
      set => periodicAmountDue = value;
    }

    /// <summary>
    /// A value of TotalMonthlyDue.
    /// </summary>
    [JsonPropertyName("totalMonthlyDue")]
    public Common TotalMonthlyDue
    {
      get => totalMonthlyDue ??= new();
      set => totalMonthlyDue = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    /// <summary>
    /// A value of Pf16FlowTo.
    /// </summary>
    [JsonPropertyName("pf16FlowTo")]
    public Standard Pf16FlowTo
    {
      get => pf16FlowTo ??= new();
      set => pf16FlowTo = value;
    }

    /// <summary>
    /// A value of DlgflwRequired.
    /// </summary>
    [JsonPropertyName("dlgflwRequired")]
    public Code DlgflwRequired
    {
      get => dlgflwRequired ??= new();
      set => dlgflwRequired = value;
    }

    /// <summary>
    /// A value of FlowToKdmv.
    /// </summary>
    [JsonPropertyName("flowToKdmv")]
    public CsePersonsWorkSet FlowToKdmv
    {
      get => flowToKdmv ??= new();
      set => flowToKdmv = value;
    }

    private CashReceiptDetail selCourtOrderNo;
    private Common undistCollPrompt;
    private ObligationType passObligationType;
    private ObligationTransaction passObligationTransaction;
    private LegalAction passLegalAction;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common showInactiveFlag;
    private Common undistributed;
    private CsePerson passed;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common currentSupportOwed;
    private Common totalArrearsOwed;
    private Common totalInterestOwed;
    private Common totalMonthlyOwed;
    private CsePersonsWorkSet hiddenDisplayed;
    private Array<ExportGroup> export1;
    private Common personSelect;
    private Obligation selectedObligation;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private Common currentSupportDue;
    private Common periodicAmountDue;
    private Common totalMonthlyDue;
    private NextTranInfo hidden;
    private Standard standard;
    private Standard pf16FlowTo;
    private Code dlgflwRequired;
    private CsePersonsWorkSet flowToKdmv;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public ObligationType Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of SelectFound.
    /// </summary>
    [JsonPropertyName("selectFound")]
    public Common SelectFound
    {
      get => selectFound ??= new();
      set => selectFound = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public NextTranInfo Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private Obligation obligation;
    private TextWorkArea zeroFill;
    private ObligationType selected;
    private Common selectFound;
    private NextTranInfo zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private ObligationType obligationType;
  }
#endregion
}
