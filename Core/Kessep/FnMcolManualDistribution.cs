// Program: FN_MCOL_MANUAL_DISTRIBUTION, ID: 372287853, model: 746.
// Short name: SWEMCOLP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_MCOL_MANUAL_DISTRIBUTION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnMcolManualDistribution: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_MCOL_MANUAL_DISTRIBUTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnMcolManualDistribution(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnMcolManualDistribution.
  /// </summary>
  public FnMcolManualDistribution(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // ---------------------------------------------
    // ----------------------------------------------------------------
    // Date     Developer Name   Request#      Description
    // 02/16/96  Rick Delgado                New Development
    // 06/26/96  Holly Kennedy-MTW	      Added screen changes required
    // 				      for signoff.
    // 07/16/96  Holly Kennedy-MTW	      Screen not working properly
    // 				      Fixed Reads and functionality
    // 				      Added exitstates in the not found
    // 				      Condition of many reads.
    // 				      Caused proper data to pass to CABs.
    // 				      Etc.
    // 11/25/96   Holly Kennedy-MTW	      Retrofitted Data Level security
    // 				      Implemented full Distribution
    // 				      process
    // 09/09/97   Govind		Set Recompute Balance From Date in CSE PERSON ACCOUNT
    // ----------------------------------------------------------------
    // 03/05/97	A.Kinney	Removed RETURN logic at top of Tran.
    // 				It was preventing any data from
    // 				being sent back on a Link Return.
    // 				In addition, corrected a view problem in PAYR
    // 				so that CSH RCPT DETAIL Seq Id would be Sent
    // 				from PAYER.  Omission of this attribute in an
    // 				Entity view was preventing a read of any
    // 				CASH DETAILS in this tran MCOL.
    // 				
    // -----------------------------------------------------------------------
    // 03/06/97	A.Kinney	AP/Payor field needs to be protected.						Prompt for 
    // payor removed.
    // ---------------------------------------------------------
    // 04/11/1997     Sumanta - MTW
    //   - Made the following changes :-
    //      - changed PF5 to 'Release'
    //      - deleted reference type
    //      - added rcpt/ref #
    //      - ADDED flow to CRUC for look up ...
    // --------------------------------------------------------------
    // --------------------------------------------------------------
    // 01/09/1998   Gary McGirr   Added PF key to go to DEBT
    // --------------------------------------------------------------
    // ---------------------------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // 021398	govind		Fixed to get the SSN and pass it to the subseq acblks
    // 030598	govind		Fixed to handle primary-secondary debts properly
    // ---------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // 092398	A. Doty		Corrected problems following problems;
    // 1) Invalid exit state displaying on return from DEBT.
    // 2) Removed prompt field for Amt Undist.
    // 3) Forced selection before linking to MDIS.
    // 4) Resolved invalid exit state when the Enter key is pressed with no Next
    // Tran info entered.
    // 5) Removed the ERROR make statement on Bal Owed field.
    // 6) Removed the ERROR make statement on Int Owed field.
    // 7) Corrected problems with exit states regarding the Court Order field.
    // 8) Set the Receipt Number field to protected.
    // 9) Corrected problems with the summary bucket updates.
    // 10) Force cursor to the B/I field with it is in error.
    // 11) Rewrote the way in which the Prad's & A/B's were handling the Primary
    // /Secondary & Joint and Several obligations.
    // In general, resturctured the procedure and the action blocks to be more 
    // readable and easier to understand.
    // 08/05/2002  K. Doshi   PR# 149011. Fix screen help Id.
    // ---------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------------
    // 04/26/2019   R Mathews   CQ65311   Add ability to display and filter by 
    // program
    // ------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // : Initialization - Set up local date views.
    local.HardcodedFType.SequentialIdentifier = 3;
    local.HardcodedSType.SequentialIdentifier = 4;
    local.HardcodedNaDprProgram.ProgramState = "NA";
    local.HardcodedPa.ProgramState = "PA";
    local.HardcodedTa.ProgramState = "TA";
    local.HardcodedCa.ProgramState = "CA";
    local.HardcodedUd.ProgramState = "UD";
    local.HardcodedUp.ProgramState = "UP";
    local.HardcodedUk.ProgramState = "UK";
    UseFnHardcodedCashReceipting();
    UseFnHardcodedDebtDistribution();
    local.PrworaDateOfConversion.Date = new DateTime(2000, 10, 1);
    local.MaxDiscontinue.Date = UseCabSetMaximumDiscontinueDate();
    local.UserId.Text8 = global.UserId;

    // : Initialization - Move Imports to Exports.
    export.Hidden.Assign(import.Hidden);
    export.ObligorCsePerson.Number = import.ObligorCsePerson.Number;
    MoveCsePersonsWorkSet(import.ObligorCsePersonsWorkSet,
      export.ObligorCsePersonsWorkSet);
    export.CashReceiptSourceType.Assign(import.CashReceiptSourceType);
    export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
    export.CashReceiptDetail.Assign(import.CashReceiptDetail);
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    export.CashReceiptType.Assign(import.CashReceiptType);
    export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      import.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    MoveCollectionType(import.CollectionType, export.CollectionType);
    export.TotalCollToDist.TotalCurrency = import.TotalCollToDist.TotalCurrency;
    export.Collection.ManualDistributionReasonText =
      import.Collection.ManualDistributionReasonText;
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    export.TraceInd.Flag = import.TraceInd.Flag;

    // : If the PF11 - Clear key has been pressed, clear all use enterable 
    // fields, but keep the protected fields intact.
    if (Equal(global.Command, "CLEAR"))
    {
      export.DisplayAllObligInd.Flag = "N";

      return;
    }

    if (IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
    {
      var field = GetField(export.ScreenOwedAmounts, "errorInformationLine");

      field.Color = "";
      field.Intensity = Intensity.Dark;
      field.Highlighting = Highlighting.Normal;
      field.Protected = true;
    }
    else
    {
      var field = GetField(export.ScreenOwedAmounts, "errorInformationLine");

      field.Color = "red";
      field.Intensity = Intensity.High;
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = true;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.CrRefNumber.CrdCrCombo = import.CrRefNumber.CrdCrCombo;
    export.FilterLegalAction.Assign(import.FilterLegalAction);
    export.PromptLegalAction.Text1 = import.PromptLegalAction.Text1;
    export.FromDateFilter.DueDt = import.FromDateFilter.DueDt;
    export.ToDateFilter.DueDt = import.ToDateFilter.DueDt;
    MoveObligationType(import.FilterObligationType, export.FilterObligationType);
      
    export.PromptObligationType.Text1 = import.PromptObligationType.Text1;
    export.PromptPgm.SelectChar = import.PromptPgm.SelectChar;
    export.Pgm.Text3 = import.Pgm.Text3;
    export.DisplayAllObligInd.Flag = import.DisplayAllObligInd.Flag;

    if (IsEmpty(export.DisplayAllObligInd.Flag))
    {
      export.DisplayAllObligInd.Flag = "N";
    }

    export.Group.Index = -1;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.LegalAction.StandardNumber =
        import.Group.Item.LegalAction.StandardNumber;
      export.Group.Update.ObligationType.
        Assign(import.Group.Item.ObligationType);
      export.Group.Update.Obligation.Assign(import.Group.Item.Obligation);
      MoveObligationTransaction(import.Group.Item.Debt, export.Group.Update.Debt);
        
      export.Group.Update.DebtDetail.Assign(import.Group.Item.DebtDetail);
      MoveCollection(import.Group.Item.Collection,
        export.Group.Update.Collection);
      export.Group.Update.ApplyToCode.Text1 =
        import.Group.Item.ApplyToCode.Text1;
      export.Group.Update.SuppPrsnCsePerson.Number =
        import.Group.Item.SuppPrsnCsePerson.Number;
      MoveCsePersonsWorkSet(import.Group.Item.SuppPrsnCsePersonsWorkSet,
        export.Group.Update.SuppPrsnCsePersonsWorkSet);
      export.Group.Update.Pgm.Text3 = import.Group.Item.Pgm.Text3;

      var field1 = GetField(export.Group.Item.Collection, "amount");

      field1.Color = "green";
      field1.Intensity = Intensity.High;
      field1.Highlighting = Highlighting.Underscore;
      field1.Protected = false;
      field1.Focused = false;

      var field2 = GetField(export.Group.Item.ApplyToCode, "text1");

      field2.Color = "green";
      field2.Intensity = Intensity.High;
      field2.Highlighting = Highlighting.Underscore;
      field2.Protected = false;
      field2.Focused = false;
    }

    // : If the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (Equal(global.Command, "ENTER"))
    {
      if (IsEmpty(import.Standard.NextTransaction))
      {
        // : No Next Tran entered - Continue Processing.
      }
      else
      {
        export.Hidden.CsePersonNumberAp = export.ObligorCsePerson.Number;
        export.Hidden.StandardCrtOrdNumber =
          export.FilterLegalAction.StandardNumber ?? "";
        UseScCabNextTranPut();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;

          return;
        }
      }
    }

    // : Next Tran to this procedure is prohibited.
    if (Equal(global.Command, "XXNEXTXX"))
    {
      ExitState = "FN0000_NO_NEXT_TRAN_TO_MCOL";

      return;
    }

    // : Validate action level security - Added Data level security 11/25/96.
    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "RELEASE") || Equal(global.Command, "NOMANDIS"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // : Command Processing for Flow Control (Flow into and out of this 
    // procedure).
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        if (AsChar(export.PromptLegalAction.Text1) == 'S')
        {
          if (AsChar(export.PromptObligationType.Text1) == 'S')
          {
            if (AsChar(export.PromptPgm.SelectChar) == 'S')
            {
              var field6 = GetField(export.PromptLegalAction, "text1");

              field6.Error = true;

              var field7 = GetField(export.PromptObligationType, "text1");

              field7.Error = true;

              var field8 = GetField(export.PromptPgm, "selectChar");

              field8.Error = true;

              ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S2";

              return;
            }
            else
            {
              var field6 = GetField(export.PromptLegalAction, "text1");

              field6.Error = true;

              var field7 = GetField(export.PromptObligationType, "text1");

              field7.Error = true;

              ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S2";

              return;
            }
          }
          else if (AsChar(export.PromptPgm.SelectChar) == 'S')
          {
            var field6 = GetField(export.PromptLegalAction, "text1");

            field6.Error = true;

            var field7 = GetField(export.PromptPgm, "selectChar");

            field7.Error = true;

            ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S2";

            return;
          }
        }
        else if (AsChar(export.PromptObligationType.Text1) == 'S')
        {
          if (AsChar(export.PromptPgm.SelectChar) == 'S')
          {
            var field6 = GetField(export.PromptObligationType, "text1");

            field6.Error = true;

            var field7 = GetField(export.PromptPgm, "selectChar");

            field7.Error = true;

            ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S2";

            return;
          }
        }

        switch(AsChar(export.PromptLegalAction.Text1))
        {
          case ' ':
            // : This field not selected for prompt - Continue Processing.
            break;
          case 'S':
            ExitState = "ECO_LNK_LST_LEG_ACT_BY_CSE_PERSN";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.PromptLegalAction, "text1");

            field.Error = true;

            return;
        }

        switch(AsChar(export.PromptObligationType.Text1))
        {
          case ' ':
            // : This field not selected for prompt - Continue Processing.
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_LST_OBLIGATION_TYPE";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.PromptObligationType, "text1");

            field.Error = true;

            return;
        }

        switch(AsChar(export.PromptPgm.SelectChar))
        {
          case ' ':
            // : This field not selected for prompt - Continue Processing.
            break;
          case 'S':
            export.ToCdvl.CodeName = "CASE LEVEL PROGRAM";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.PromptPgm, "selectChar");

            field.Error = true;

            return;
        }

        // : The PF4 - List key was pressed, but no prompt field was selected.
        var field1 = GetField(export.PromptLegalAction, "text1");

        field1.Error = true;

        var field2 = GetField(export.PromptObligationType, "text1");

        field2.Error = true;

        var field3 = GetField(export.PromptPgm, "selectChar");

        field3.Error = true;

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        return;
      case "MDIS":
        local.TotalSelected.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.Common.SelectChar))
          {
            case ' ':
              // : Continue Processing.
              break;
            case 'S':
              ++local.TotalSelected.Count;

              if (local.TotalSelected.Count > 1)
              {
                var field6 = GetField(export.Group.Item.Common, "selectChar");

                field6.Error = true;

                ExitState = "ZD_ACO_NE00_INVALID_MULTIPLE_SEL";

                return;
              }

              export.DlgflwRequiredObligation.SystemGeneratedIdentifier =
                export.Group.Item.Obligation.SystemGeneratedIdentifier;
              MoveObligationType(export.Group.Item.ObligationType,
                export.DlgflwRequiredObligationType);
              export.DlgflwRequiredLegalAction.StandardNumber =
                export.Group.Item.LegalAction.StandardNumber;

              break;
            default:
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }
        }

        export.Group.CheckIndex();

        if (local.TotalSelected.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        ExitState = "ECO_LNK_TO_MDIS";

        return;
      case "DEBT":
        local.TotalSelected.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.Common.SelectChar))
          {
            case ' ':
              // : Continue Processing.
              break;
            case 'S':
              ++local.TotalSelected.Count;

              if (local.TotalSelected.Count > 1)
              {
                var field6 = GetField(export.Group.Item.Common, "selectChar");

                field6.Error = true;

                ExitState = "ZD_ACO_NE00_INVALID_MULTIPLE_SEL";

                return;
              }

              export.DlgflwRequiredObligation.SystemGeneratedIdentifier =
                export.Group.Item.Obligation.SystemGeneratedIdentifier;
              MoveObligationType(export.Group.Item.ObligationType,
                export.DlgflwRequiredObligationType);
              export.DlgflwRequiredLegalAction.StandardNumber =
                export.Group.Item.LegalAction.StandardNumber;

              break;
            default:
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }
        }

        export.Group.CheckIndex();
        ExitState = "ECO_LNK_LST_DBT_ACT_BY_AP_PYR";

        return;
      case "RETLAPS":
        export.PromptLegalAction.Text1 = "";

        var field4 = GetField(export.FilterObligationType, "code");

        field4.Protected = false;
        field4.Focused = false;

        global.Command = "DISPLAY";

        break;
      case "RETOBTL":
        export.PromptObligationType.Text1 = "";

        var field5 = GetField(export.Group.Item.Common, "selectChar");

        field5.Protected = false;
        field5.Focused = false;

        global.Command = "DISPLAY";

        break;
      case "RETMDIS":
        return;
      case "RETDEBT":
        return;
      case "RETCDVL":
        export.PromptPgm.SelectChar = "";

        if (IsEmpty(import.FromCdvl.Cdvalue))
        {
        }
        else
        {
          export.Pgm.Text3 = import.FromCdvl.Cdvalue;
        }

        global.Command = "DISPLAY";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "SIGNOFF":
        UseScEabSignoff();

        return;
      default:
        // : No Flow Control command to process.
        break;
    }

    // : Processing Time - Handle the Display, Release and No Manual 
    // Distribution options.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (Equal(export.ToDateFilter.DueDt, local.Null1.Date))
        {
          export.ToDateFilter.DueDt =
            Now().Date.AddDays(-(Now().Date.Day - 1)).AddMonths(1).AddDays(-1);
        }
        else if (Lt(export.ToDateFilter.DueDt, export.FromDateFilter.DueDt))
        {
          ExitState = "ACO_NE0000_END_LESS_THAN_START";

          return;
        }

        // : The Cash Receipt information is expected to be passed to this 
        // procedure.
        if (export.CashReceiptSourceType.SystemGeneratedIdentifier == 0 || export
          .CashReceiptEvent.SystemGeneratedIdentifier == 0 || export
          .CashReceiptType.SystemGeneratedIdentifier == 0 || export
          .CashReceipt.SequentialNumber == 0 || export
          .CashReceiptDetail.SequentialIdentifier == 0)
        {
          ExitState = "FN0000_MISSING_DATA_FOR_MCOL";

          return;
        }

        if (ReadCashReceiptDetailCashReceiptTypeCashReceiptSourceType())
        {
          export.CashReceiptDetail.Assign(entities.ExistingCashReceiptDetail);
          export.CashReceiptType.Assign(entities.ExistingCashReceiptType);
          export.CashReceiptSourceType.Assign(
            entities.ExistingCashReceiptSourceType);

          if (ReadCashReceiptDetailStatus())
          {
            export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
              entities.ExistingCashReceiptDetailStatus.
                SystemGeneratedIdentifier;
          }
          else
          {
            ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

            return;
          }

          foreach(var item in ReadCashReceiptDetail2())
          {
            export.CashReceiptDetail.CollectionAmount -= entities.
              ExistingAdjusted.CollectionAmount;

            if (export.CashReceiptDetail.CollectionAmount <= 0)
            {
              ExitState = "NO_COLL_TO_DIST";

              return;
            }
          }

          if (ReadCollectionType())
          {
            MoveCollectionType(entities.ExistingCollectionType,
              export.CollectionType);
          }
          else
          {
            ExitState = "FN0000_COLLECTION_TYPE_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0052_CASH_RCPT_DTL_NF";

          return;
        }

        // : Deriving the Receipt/Ref# for display.
        UseFnAbConcatCrAndCrd();

        // : Compute the undistributed amount for display.
        export.TotalCollToDist.TotalCurrency =
          export.CashReceiptDetail.CollectionAmount - (
            export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + export
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault());

        // : Verify that the status of the Cash Receipt Detail is in Recorded or
        // Pended.
        // : Get the Formatted Name for the CSE Person.
        export.ObligorCsePersonsWorkSet.Number =
          export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);

        if (AsChar(export.TraceInd.Flag) == 'Y')
        {
          export.ObligorCsePerson.Number =
            export.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
          export.ObligorCsePersonsWorkSet.FormattedName =
            "Tracing - No Name Returned.";
        }
        else
        {
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        if (export.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .HardcodedPended.SystemGeneratedIdentifier || export
          .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .HardcodedRecorded.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_CANNOT_DIST_IN_THIS_STAT";

          return;
        }

        // : Verify that the court order entered is a valid court order, that it
        // is valid for the AP/Payor  and that there are valid obligations
        // associated to the court order.
        if (!IsEmpty(export.FilterLegalAction.StandardNumber))
        {
          UseFnVerifyCourtOrderFilter();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.FilterLegalAction, "standardNumber");

            field.Error = true;

            return;
          }
        }

        if (!IsEmpty(export.FilterObligationType.Code))
        {
          if (ReadObligationType())
          {
            MoveObligationType(entities.ExistingObligationType,
              export.FilterObligationType);
          }
          else
          {
            var field = GetField(export.FilterObligationType, "code");

            field.Error = true;

            ExitState = "FN0000_OBLIGATION_TYPE_NF";

            return;
          }
        }
        else
        {
          MoveObligationType(local.Initialized, export.FilterObligationType);
        }

        // : This will display a list of debts which are available for Manual 
        // Distribution.
        UseFnRetrieveDebtsForMnlDist();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (export.Group.IsEmpty)
        {
          if (AsChar(export.DisplayAllObligInd.Flag) == 'N')
          {
            ExitState = "FN0000_NO_OBLG_FOUND_F_MDIST_RDS";
          }
          else
          {
            ExitState = "FN0000_NO_OBLG_FOUND_F_MDIST";
          }

          return;
        }
        else if (export.Group.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";

          return;
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        if (IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "";
          field.Intensity = Intensity.Dark;
          field.Highlighting = Highlighting.Normal;
          field.Protected = true;
        }
        else
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        break;
      case "NOMANDIS":
        // : Update an indicator on Cash Receipt Detail to override the Manual 
        // Distribution Instructions.
        UseFnBypassMnlDistInstr();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "RELEASE":
        // : If there are no debts, then there can be no distributions.
        if (export.Group.IsEmpty)
        {
          ExitState = "FN0000_NO_OBLG_FOUND_F_MDIST";

          return;
        }

        if (IsEmpty(export.Collection.ManualDistributionReasonText))
        {
          var field =
            GetField(export.Collection, "manualDistributionReasonText");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        // : Edit the distribution amounts prior to creating collection.
        local.Coll.Date = export.CashReceiptDetail.CollectionDate;
        UseCabFirstAndLastDateOfMonth1();
        local.TotalDistAmt.Amount = 0;
        local.TotalDistAmtToSecOb.Amount = 0;
        local.TotalSelected.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (IsEmpty(export.Group.Item.Common.SelectChar))
          {
            continue;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) != 'S')
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
          }

          // --- Select char is "S"
          // : Check for an amount being entered to ensure that it is not 
          // negative and that it is greater than zero.
          if (export.Group.Item.Collection.Amount <= 0)
          {
            var field = GetField(export.Group.Item.Collection, "amount");

            field.Error = true;

            ExitState = "FN0000_AMT_CANT_BE_ZERO_OR_NEG";

            return;
          }

          ++local.TotalSelected.Count;

          // : Track the distributed amount by Primary & Secondary separately.
          if (AsChar(export.Group.Item.Obligation.PrimarySecondaryCode) == 'S')
          {
            local.TotalDistAmtToSecOb.Amount += export.Group.Item.Collection.
              Amount;

            if (local.TotalDistAmtToSecOb.Amount > export
              .TotalCollToDist.TotalCurrency)
            {
              var field = GetField(export.Group.Item.Collection, "amount");

              field.Error = true;

              ExitState = "FN0000_EXCEEDS_AMT_AVAIL_TO_DIST";

              return;
            }
          }
          else
          {
            local.TotalDistAmt.Amount += export.Group.Item.Collection.Amount;

            if (local.TotalDistAmt.Amount > export
              .TotalCollToDist.TotalCurrency)
            {
              var field = GetField(export.Group.Item.Collection, "amount");

              field.Error = true;

              ExitState = "FN0000_EXCEEDS_AMT_AVAIL_TO_DIST";

              return;
            }
          }

          switch(AsChar(export.Group.Item.ApplyToCode.Text1))
          {
            case 'B':
              // : The system will apply to Balance Owed on the Debt Detail. The
              // system will derive whether it is current, arrears or gift
              // based on the collection date and due date.
              if (AsChar(export.Group.Item.ObligationType.Classification) == AsChar
                (local.HardcodedVoluntaryClass.Classification))
              {
                export.Group.Update.Collection.AppliedToCode = "C";

                break;
              }

              if (AsChar(export.Group.Item.ObligationType.Classification) == AsChar
                (local.HardcodedAccruingClass.Classification))
              {
                if (!Lt(export.Group.Item.DebtDetail.DueDt,
                  local.CollFirstOfMth.Date))
                {
                  export.Group.Update.Collection.AppliedToCode = "C";
                }
                else
                {
                  export.Group.Update.Collection.AppliedToCode = "A";
                }
              }
              else
              {
                export.Group.Update.Collection.AppliedToCode = "A";
              }

              if (export.Group.Item.Collection.Amount > export
                .Group.Item.DebtDetail.BalanceDueAmt)
              {
                var field1 = GetField(export.Group.Item.Collection, "amount");

                field1.Error = true;

                ExitState = "FN0000_AMT_EXCEEDS_BAL_AVAIL";

                return;
              }

              break;
            case 'I':
              export.Group.Update.Collection.AppliedToCode =
                export.Group.Item.ApplyToCode.Text1;

              if (export.Group.Item.Collection.Amount > export
                .Group.Item.DebtDetail.InterestBalanceDueAmt.
                  GetValueOrDefault())
              {
                var field1 = GetField(export.Group.Item.Collection, "amount");

                field1.Error = true;

                ExitState = "FN0000_AMT_EXCEEDS_BAL_AVAIL";

                return;
              }

              break;
            default:
              var field = GetField(export.Group.Item.ApplyToCode, "text1");

              field.Error = true;

              ExitState = "FN0000_INV_MCOL_APPLY_TO_CODE";

              return;
          }
        }

        export.Group.CheckIndex();

        if (local.TotalSelected.Count == 0)
        {
          export.Group.Index = 0;
          export.Group.CheckSize();

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        if (local.TotalDistAmtToSecOb.Amount > local.TotalDistAmt.Amount)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.Common.SelectChar) == 'S' && AsChar
              (export.Group.Item.Obligation.PrimarySecondaryCode) == AsChar
              (local.HardcodedSecondary.PrimarySecondaryCode))
            {
              var field = GetField(export.Group.Item.Collection, "amount");

              field.Error = true;
            }
          }

          export.Group.CheckIndex();
          ExitState = "FN0000_CANT_DIST_MORE_TO_S_THN_P";

          return;
        }

        if (local.TotalDistAmt.Amount == 0)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.Common.SelectChar) == 'S' && export
              .Group.Item.Collection.Amount == 0)
            {
              var field = GetField(export.Group.Item.Collection, "amount");

              field.Error = true;
            }
          }

          export.Group.CheckIndex();
          ExitState = "FN0000_AMT_CANT_BE_ZERO_OR_NEG";

          return;
        }

        local.Manual.DistributionMethod = "M";
        local.Debts.Index = -1;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) != 'S')
          {
            continue;
          }

          if (local.Debts.Index + 1 >= Local.DebtsGroup.Capacity)
          {
            break;
          }

          ++local.Debts.Index;
          local.Debts.CheckSize();

          local.Debts.Update.DebtsObligationType.Assign(
            export.Group.Item.ObligationType);
          local.Debts.Update.DebtsObligation.
            Assign(export.Group.Item.Obligation);
          local.Debts.Update.DebtsDebt.SystemGeneratedIdentifier =
            export.Group.Item.Debt.SystemGeneratedIdentifier;
          MoveCollection(export.Group.Item.Collection,
            local.Debts.Update.DebtsCollection);
          local.Debts.Update.DebtsSuppPrsn.Number =
            export.Group.Item.SuppPrsnCsePerson.Number;
        }

        export.Group.CheckIndex();

        // : No debts have been selected.  This should never happen because a 
        // check has already been made for this condition above.
        if (local.Debts.IsEmpty)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        if (ReadCashReceiptDetail1())
        {
          // : Continue Processing.
        }
        else
        {
          ExitState = "FN0052_CASH_RCPT_DTL_NF";

          return;
        }

        if (ReadObligor())
        {
          // : Continue Processing.
        }
        else
        {
          ExitState = "FN0000_OBLIGOR_NF";

          return;
        }

        // : Build a list of program values to be used in determining the 
        // program for a debt detail.
        UseFnBuildProgramValues();

        // : Build a list of program values.
        local.OfPgms.Index = 0;
        local.OfPgms.Clear();

        foreach(var item in ReadProgram())
        {
          local.OfPgms.Update.OfPgms1.Assign(entities.ExistingProgram);
          local.OfPgms.Next();
        }

        UseFnBuildProgramHistoryTable();
        UseFnBuildHouseholdHistoryTable();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.HhHistProc.Index = 0;
        local.HhHistProc.Clear();

        for(local.HhHist.Index = 0; local.HhHist.Index < local.HhHist.Count; ++
          local.HhHist.Index)
        {
          if (local.HhHistProc.IsFull)
          {
            break;
          }

          local.HhHistProc.Update.HhHistSuppPrsnProc.Number =
            local.HhHist.Item.HhHistSuppPrsn.Number;

          local.HhHistProc.Item.HhHistDtlProc.Index = 0;
          local.HhHistProc.Item.HhHistDtlProc.Clear();

          for(local.HhHist.Item.HhHistDtl.Index = 0; local
            .HhHist.Item.HhHistDtl.Index < local.HhHist.Item.HhHistDtl.Count; ++
            local.HhHist.Item.HhHistDtl.Index)
          {
            if (local.HhHistProc.Item.HhHistDtlProc.IsFull)
            {
              break;
            }

            local.HhHistProc.Update.HhHistDtlProc.Update.
              HhHistDtlProcImHousehold.AeCaseNo =
                local.HhHist.Item.HhHistDtl.Item.HhHistDtlImHousehold.AeCaseNo;
            local.HhHistProc.Update.HhHistDtlProc.Update.
              HhHistDtlProcImHouseholdMbrMnthlySum.Assign(
                local.HhHist.Item.HhHistDtl.Item.
                HhHistDtlImHouseholdMbrMnthlySum);
            local.HhHistProc.Item.HhHistDtlProc.Next();
          }

          local.HhHistProc.Next();
        }

        UseFnDeterminePgmForColl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.Manual.DistributionMethod = "M";
        local.Current.Date = Now().Date;
        local.Current.Timestamp = Now();
        UseCabFirstAndLastDateOfMonth2();
        local.Current.YearMonth = UseCabGetYearMonthFromDate();
        UseFnProcessDistributionRequest();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.CashReceiptDetail.Assign(entities.ExistingCashReceiptDetail);

        // : Compute the undistributed amount
        export.TotalCollToDist.TotalCurrency =
          export.CashReceiptDetail.CollectionAmount - (
            export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + export
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault());

        // : This will display a list of debts which are available for Manual 
        // Distribution.
        UseFnRetrieveDebtsForMnlDist();

        if (IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "";
          field.Intensity = Intensity.Dark;
          field.Highlighting = Highlighting.Normal;
          field.Protected = true;
        }
        else
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        ExitState = "FN0000_MANUAL_DIST_SUCCESSFUL";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
    target.OverrideManualDistInd = source.OverrideManualDistInd;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.OverrideManualDistInd = source.OverrideManualDistInd;
  }

  private static void MoveCashReceiptDetail3(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionDate = source.CollectionDate;
  }

  private static void MoveCashReceiptSourceType1(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptSourceType2(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CourtInd = source.CourtInd;
  }

  private static void MoveCashReceiptType1(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptType2(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CategoryIndicator = source.CategoryIndicator;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCsePersonAccount(CsePersonAccount source,
    CsePersonAccount target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.LastManualDistributionDate = source.LastManualDistributionDate;
    target.LastCollAmt = source.LastCollAmt;
    target.LastCollDt = source.LastCollDt;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDebts1(Local.DebtsGroup source,
    FnDeterminePgmForColl.Import.DebtsGroup target)
  {
    target.DebtsObligationType.Assign(source.DebtsObligationType);
    target.DebtsObligation.Assign(source.DebtsObligation);
    target.DebtsDebt.SystemGeneratedIdentifier =
      source.DebtsDebt.SystemGeneratedIdentifier;
    MoveCollection(source.DebtsCollection, target.DebtsCollection);
    target.DebtsSuppPrsn.Number = source.DebtsSuppPrsn.Number;
    target.DebtsProgram.Assign(source.DebtsProgram);
    target.DebtsDprProgram.ProgramState = source.DebtsDprProgram.ProgramState;
  }

  private static void MoveDebts2(FnDeterminePgmForColl.Import.DebtsGroup source,
    Local.DebtsGroup target)
  {
    target.DebtsObligationType.Assign(source.DebtsObligationType);
    target.DebtsObligation.Assign(source.DebtsObligation);
    target.DebtsDebt.SystemGeneratedIdentifier =
      source.DebtsDebt.SystemGeneratedIdentifier;
    MoveCollection(source.DebtsCollection, target.DebtsCollection);
    target.DebtsSuppPrsn.Number = source.DebtsSuppPrsn.Number;
    target.DebtsProgram.Assign(source.DebtsProgram);
    target.DebtsDprProgram.ProgramState = source.DebtsDprProgram.ProgramState;
  }

  private static void MoveDebtsToGroup(Local.DebtsGroup source,
    FnProcessDistributionRequest.Import.GroupGroup target)
  {
    target.ObligationType.Assign(source.DebtsObligationType);
    target.Obligation.Assign(source.DebtsObligation);
    target.Debt.SystemGeneratedIdentifier =
      source.DebtsDebt.SystemGeneratedIdentifier;
    MoveCollection(source.DebtsCollection, target.Collection);
    target.SuppPrsn.Number = source.DebtsSuppPrsn.Number;
    target.Program.Assign(source.DebtsProgram);
    target.DprProgram.ProgramState = source.DebtsDprProgram.ProgramState;
  }

  private static void MoveGroup(FnRetrieveDebtsForMnlDist.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    MoveObligationTransaction(source.Debt, target.Debt);
    target.DebtDetail.Assign(source.DebtDetail);
    target.LegalAction.StandardNumber = source.LegalAction.StandardNumber;
    MoveCollection(source.Collection, target.Collection);
    target.ApplyToCode.Text1 = source.ApplyToCod.Text1;
    target.SuppPrsnCsePerson.Number = source.SuppPrsnCsePerson.Number;
    MoveCsePersonsWorkSet(source.SuppPrsnCsePersonsWorkSet,
      target.SuppPrsnCsePersonsWorkSet);
    target.Pgm.Text3 = source.Pgm.Text3;
    target.Common.SelectChar = source.Common.SelectChar;
  }

  private static void MoveHhHist1(FnBuildHouseholdHistoryTable.Export.
    HhHistGroup source, Local.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl1);
  }

  private static void MoveHhHist2(FnProcessDistributionRequest.Import.
    HhHistGroup source, Local.HhHistProcGroup target)
  {
    target.HhHistSuppPrsnProc.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtlProc, MoveHhHistDtl2);
  }

  private static void MoveHhHist3(FnDeterminePgmForColl.Import.
    HhHistGroup source, Local.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl3);
  }

  private static void MoveHhHist4(Local.HhHistGroup source,
    FnDeterminePgmForColl.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsn.Number;
    source.HhHistDtl.CopyTo(target.HhHistDtl, MoveHhHistDtl4);
  }

  private static void MoveHhHistDtl1(FnBuildHouseholdHistoryTable.Export.
    HhHistDtlGroup source, Local.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl2(FnProcessDistributionRequest.Import.
    HhHistDtlGroup source, Local.HhHistDtlProcGroup target)
  {
    target.HhHistDtlProcImHousehold.AeCaseNo =
      source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlProcImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl3(FnDeterminePgmForColl.Import.
    HhHistDtlGroup source, Local.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtl4(Local.HhHistDtlGroup source,
    FnDeterminePgmForColl.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo = source.HhHistDtlImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistDtlProc(Local.HhHistDtlProcGroup source,
    FnProcessDistributionRequest.Import.HhHistDtlGroup target)
  {
    target.HhHistDtlImHousehold.AeCaseNo =
      source.HhHistDtlProcImHousehold.AeCaseNo;
    target.HhHistDtlImHouseholdMbrMnthlySum.Assign(
      source.HhHistDtlProcImHouseholdMbrMnthlySum);
  }

  private static void MoveHhHistProc(Local.HhHistProcGroup source,
    FnProcessDistributionRequest.Import.HhHistGroup target)
  {
    target.HhHistSuppPrsn.Number = source.HhHistSuppPrsnProc.Number;
    source.HhHistDtlProc.CopyTo(target.HhHistDtl, MoveHhHistDtlProc);
  }

  private static void MoveLegal1(FnBuildHouseholdHistoryTable.Export.
    LegalGroup source, Local.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl1);
  }

  private static void MoveLegal2(Local.LegalGroup source,
    FnProcessDistributionRequest.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl2);
  }

  private static void MoveLegal3(Local.LegalGroup source,
    FnDeterminePgmForColl.Import.LegalGroup target)
  {
    target.LegalSuppPrsn.Number = source.LegalSuppPrsn.Number;
    source.LegalDtl.CopyTo(target.LegalDtl, MoveLegalDtl3);
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalDtl1(FnBuildHouseholdHistoryTable.Export.
    LegalDtlGroup source, Local.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl2(Local.LegalDtlGroup source,
    FnProcessDistributionRequest.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
  }

  private static void MoveLegalDtl3(Local.LegalDtlGroup source,
    FnDeterminePgmForColl.Import.LegalDtlGroup target)
  {
    target.LegalDtl1.StandardNumber = source.LegalDtl1.StandardNumber;
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

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveOfPgms(Local.OfPgmsGroup source,
    FnBuildProgramHistoryTable.Import.OfPgmsGroup target)
  {
    target.OfPgms1.Assign(source.OfPgms1);
  }

  private static void MovePersonProgram(PersonProgram source,
    PersonProgram target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MovePgmHist1(FnBuildProgramHistoryTable.Export.
    PgmHistGroup source, Local.PgmHistGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl1);
  }

  private static void MovePgmHist2(Local.PgmHistGroup source,
    FnDeterminePgmForColl.Import.PgmHistGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    source.PgmHistDtl.CopyTo(target.PgmHistDtl, MovePgmHistDtl2);
    target.PgmHistSuppPrsn.Number = source.PgmHistSuppPrsn.Number;
  }

  private static void MovePgmHistDtl1(FnBuildProgramHistoryTable.Export.
    PgmHistDtlGroup source, Local.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private static void MovePgmHistDtl2(Local.PgmHistDtlGroup source,
    FnDeterminePgmForColl.Import.PgmHistDtlGroup target)
  {
    target.PgmHistDtlProgram.Assign(source.PgmHistDtlProgram);
    MovePersonProgram(source.PgmHistDtlPersonProgram,
      target.PgmHistDtlPersonProgram);
  }

  private void UseCabFirstAndLastDateOfMonth1()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Coll.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.CollLastOfMth.Date = useExport.Last.Date;
    local.CollFirstOfMth.Date = useExport.First.Date;
  }

  private void UseCabFirstAndLastDateOfMonth2()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.CollLastOfMth.Date = useExport.Last.Date;
    local.CollFirstOfMth.Date = useExport.First.Date;
  }

  private int UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    export.CrRefNumber.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseFnBuildHouseholdHistoryTable()
  {
    var useImport = new FnBuildHouseholdHistoryTable.Import();
    var useExport = new FnBuildHouseholdHistoryTable.Export();

    MoveCashReceiptDetail3(entities.ExistingCashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.Obligor.Number = export.ObligorCsePerson.Number;

    Call(FnBuildHouseholdHistoryTable.Execute, useImport, useExport);

    useExport.Legal.CopyTo(local.Legal, MoveLegal1);
    useExport.HhHist.CopyTo(local.HhHist, MoveHhHist1);
  }

  private void UseFnBuildProgramHistoryTable()
  {
    var useImport = new FnBuildProgramHistoryTable.Import();
    var useExport = new FnBuildProgramHistoryTable.Export();

    useImport.MaximumDiscontinue.Date = local.MaxDiscontinue.Date;
    local.OfPgms.CopyTo(useImport.OfPgms, MoveOfPgms);
    useImport.Obligor.Number = export.ObligorCsePerson.Number;

    Call(FnBuildProgramHistoryTable.Execute, useImport, useExport);

    useExport.PgmHist.CopyTo(local.PgmHist, MovePgmHist1);
  }

  private void UseFnBuildProgramValues()
  {
    var useImport = new FnBuildProgramValues.Import();
    var useExport = new FnBuildProgramValues.Export();

    Call(FnBuildProgramValues.Execute, useImport, useExport);

    local.HardcodedAf.Assign(useExport.HardcodedAf);
    local.HardcodedAfi.Assign(useExport.HardcodedAfi);
    local.HardcodedFc.Assign(useExport.HardcodedFc);
    local.HardcodedFci.Assign(useExport.HardcodedFci);
    local.HardcodedNaProgram.Assign(useExport.HardcodedNa);
    local.HardcodedNai.Assign(useExport.HardcodedNai);
    local.HardcodedNc.Assign(useExport.HardcodedNc);
    local.HardcodedNf.Assign(useExport.HardcodedNf);
    local.HardcodedMai.Assign(useExport.HardcodedMai);
  }

  private void UseFnBypassMnlDistInstr()
  {
    var useImport = new FnBypassMnlDistInstr.Import();
    var useExport = new FnBypassMnlDistInstr.Export();

    MoveCashReceiptSourceType1(export.CashReceiptSourceType,
      useImport.CashReceiptSourceType);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptType1(export.CashReceiptType, useImport.CashReceiptType);
    MoveCashReceiptDetail2(export.CashReceiptDetail, useImport.CashReceiptDetail);
      

    Call(FnBypassMnlDistInstr.Execute, useImport, useExport);
  }

  private void UseFnDeterminePgmForColl()
  {
    var useImport = new FnDeterminePgmForColl.Import();
    var useExport = new FnDeterminePgmForColl.Export();

    useImport.HardcodedMai.Assign(local.HardcodedMai);
    useImport.PersistantObligor.Assign(entities.ExistingObligor);
    useImport.PersistantCashReceiptDetail.Assign(
      entities.ExistingCashReceiptDetail);
    local.PgmHist.CopyTo(useImport.PgmHist, MovePgmHist2);
    local.Legal.CopyTo(useImport.Legal, MoveLegal3);
    local.HhHist.CopyTo(useImport.HhHist, MoveHhHist4);
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      local.Hardcoded718B.SystemGeneratedIdentifier;
    useImport.Coll.Date = local.Coll.Date;
    local.Debts.CopyTo(useImport.Debts, MoveDebts1);
    useImport.HardcodedFType.SequentialIdentifier =
      local.HardcodedFType.SequentialIdentifier;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      local.HardcodedSecondary.PrimarySecondaryCode;
    useImport.HardcodedAccruingClass.Classification =
      local.HardcodedAccruingClass.Classification;
    useImport.HardcodedAf.Assign(local.HardcodedAf);
    useImport.HardcodedAfi.Assign(local.HardcodedAfi);
    useImport.HardcodedFc.Assign(local.HardcodedFc);
    useImport.HardcodedFci.Assign(local.HardcodedFci);
    useImport.HardcodedNaProgram.Assign(local.HardcodedNaProgram);
    useImport.HardcodedNai.Assign(local.HardcodedNai);
    useImport.HardcodedNc.Assign(local.HardcodedNc);
    useImport.HardcodedNf.Assign(local.HardcodedNf);
    useImport.HardcodedNaDprProgram.ProgramState =
      local.HardcodedNaDprProgram.ProgramState;
    useImport.HardcodedPa.ProgramState = local.HardcodedPa.ProgramState;
    useImport.HardcodedTa.ProgramState = local.HardcodedTa.ProgramState;
    useImport.HardcodedCa.ProgramState = local.HardcodedCa.ProgramState;
    useImport.HardcodedUd.ProgramState = local.HardcodedUd.ProgramState;
    useImport.HardcodedUp.ProgramState = local.HardcodedUp.ProgramState;
    useImport.HardcodedUk.ProgramState = local.HardcodedUk.ProgramState;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      local.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      local.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      local.HardcodedMcType.SystemGeneratedIdentifier;
    useImport.PrworaDateOfConversion.Date = local.PrworaDateOfConversion.Date;
    MoveCollectionType(export.CollectionType, useImport.CollectionType);
    useImport.Obligor.Number = export.ObligorCsePerson.Number;

    Call(FnDeterminePgmForColl.Execute, useImport, useExport);

    MoveCsePersonAccount(useImport.PersistantObligor, entities.ExistingObligor);
    MoveCashReceiptDetail1(useImport.PersistantCashReceiptDetail,
      entities.ExistingCashReceiptDetail);
    useImport.HhHist.CopyTo(local.HhHist, MoveHhHist3);
    useImport.Debts.CopyTo(local.Debts, MoveDebts2);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedFdirPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFdirPmt.SystemGeneratedIdentifier;
    local.HardcodedFcourtPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFcrtRec.SystemGeneratedIdentifier;
    local.HardcodedDistributed.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdDistributed.SystemGeneratedIdentifier;
    local.HardcodedCashType.CategoryIndicator =
      useExport.CrtCategory.CrtCatCash.CategoryIndicator;
    local.HardcodedPended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
    local.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedArrears.AppliedToCode = useExport.CollArrears.AppliedToCode;
    local.HardcodedCurrent.AppliedToCode = useExport.CollCurrent.AppliedToCode;
    local.HardcodedDeactivedStat.Code = useExport.DdshDeactivedStatus.Code;
    local.HardcodedJointSeveralObligationRlnRsn.SequentialGeneratedIdentifier =
      useExport.OrrJointSeveral.SequentialGeneratedIdentifier;
    local.HardcodedJointSeveralObligation.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
    local.HardcodedPrimary.PrimarySecondaryCode =
      useExport.ObligPrimaryConcurrent.PrimarySecondaryCode;
    local.HardcodedSecondary.PrimarySecondaryCode =
      useExport.ObligSecondaryConcurrent.PrimarySecondaryCode;
    local.HardcodedVoluntaryClass.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    local.HardcodedAccruingClass.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.Hardcoded718B.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.HardcodedMsType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupport.SystemGeneratedIdentifier;
    local.HardcodedMjType.SystemGeneratedIdentifier =
      useExport.OtMedicalJudgement.SystemGeneratedIdentifier;
    local.HardcodedMcType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupportForCash.SystemGeneratedIdentifier;
  }

  private void UseFnProcessDistributionRequest()
  {
    var useImport = new FnProcessDistributionRequest.Import();
    var useExport = new FnProcessDistributionRequest.Export();

    useImport.PersistantObligor.Assign(entities.ExistingObligor);
    useImport.PersistantCashReceiptDetail.Assign(
      entities.ExistingCashReceiptDetail);
    useImport.HardcodedFdirPmt.SystemGeneratedIdentifier =
      local.HardcodedFdirPmt.SystemGeneratedIdentifier;
    useImport.HardcodedFcourtPmt.SystemGeneratedIdentifier =
      local.HardcodedFcourtPmt.SystemGeneratedIdentifier;
    useImport.UserId.Text8 = local.UserId.Text8;
    local.Debts.CopyTo(useImport.Group, MoveDebtsToGroup);
    useImport.Current.Assign(local.Current);
    useImport.AutoOrManual.DistributionMethod = local.Manual.DistributionMethod;
    useImport.HardcodedArrears.AppliedToCode =
      local.HardcodedArrears.AppliedToCode;
    useImport.HardcodedCurrent.AppliedToCode =
      local.HardcodedCurrent.AppliedToCode;
    useImport.HardcodedDeactivedStat.Code = local.HardcodedDeactivedStat.Code;
    useImport.HardcodedDistributed.SystemGeneratedIdentifier =
      local.HardcodedDistributed.SystemGeneratedIdentifier;
    useImport.HardcodedJointSeveralObligationRlnRsn.
      SequentialGeneratedIdentifier =
        local.HardcodedJointSeveralObligationRlnRsn.
        SequentialGeneratedIdentifier;
    useImport.HardcodedCashType.CategoryIndicator =
      local.HardcodedCashType.CategoryIndicator;
    useImport.HardcodedJointSeveralObligation.PrimarySecondaryCode =
      local.HardcodedJointSeveralObligation.PrimarySecondaryCode;
    useImport.HardcodedPrimary.PrimarySecondaryCode =
      local.HardcodedPrimary.PrimarySecondaryCode;
    useImport.HardcodedSecondary.PrimarySecondaryCode =
      local.HardcodedSecondary.PrimarySecondaryCode;
    useImport.HardcodedAccruingClass.Classification =
      local.HardcodedAccruingClass.Classification;
    MoveCashReceiptType2(export.CashReceiptType, useImport.CashReceiptType);
    useImport.Adjusted.CollectionAmount =
      export.CashReceiptDetail.CollectionAmount;
    useImport.CollectionType.SequentialIdentifier =
      export.CollectionType.SequentialIdentifier;
    useImport.ForMnlDistOnly.ManualDistributionReasonText =
      export.Collection.ManualDistributionReasonText;
    useImport.HardcodedSType.SequentialIdentifier =
      local.HardcodedSType.SequentialIdentifier;
    useImport.HardcodedFType.SequentialIdentifier =
      local.HardcodedFType.SequentialIdentifier;
    MoveCashReceiptSourceType2(export.CashReceiptSourceType,
      useImport.CashReceiptSourceType);
    useImport.HardcodedVolClass.Classification =
      local.HardcodedVoluntaryClass.Classification;
    local.Legal.CopyTo(useImport.Legal, MoveLegal2);
    local.HhHistProc.CopyTo(useImport.HhHist, MoveHhHistProc);
    useImport.Hardcoded718B.SystemGeneratedIdentifier =
      local.Hardcoded718B.SystemGeneratedIdentifier;
    useImport.HardcodedAfType.SystemGeneratedIdentifier =
      local.HardcodedAf.SystemGeneratedIdentifier;
    useImport.HardcodedFcType.SystemGeneratedIdentifier =
      local.HardcodedFc.SystemGeneratedIdentifier;
    useImport.HardcodedNcType.SystemGeneratedIdentifier =
      local.HardcodedNc.SystemGeneratedIdentifier;
    useImport.HardcodedNfType.SystemGeneratedIdentifier =
      local.HardcodedNf.SystemGeneratedIdentifier;
    useImport.HardcodedUk.ProgramState = local.HardcodedUk.ProgramState;
    useImport.HardcodedMsType.SystemGeneratedIdentifier =
      local.HardcodedMsType.SystemGeneratedIdentifier;
    useImport.HardcodedMjType.SystemGeneratedIdentifier =
      local.HardcodedMjType.SystemGeneratedIdentifier;
    useImport.HardcodedMcType.SystemGeneratedIdentifier =
      local.HardcodedMcType.SystemGeneratedIdentifier;

    Call(FnProcessDistributionRequest.Execute, useImport, useExport);

    MoveCsePersonAccount(useImport.PersistantObligor, entities.ExistingObligor);
    MoveCashReceiptDetail1(useImport.PersistantCashReceiptDetail,
      entities.ExistingCashReceiptDetail);
    useImport.HhHist.CopyTo(local.HhHistProc, MoveHhHist2);
  }

  private void UseFnRetrieveDebtsForMnlDist()
  {
    var useImport = new FnRetrieveDebtsForMnlDist.Import();
    var useExport = new FnRetrieveDebtsForMnlDist.Export();

    useImport.CashReceiptDetail.CollectionDate =
      export.CashReceiptDetail.CollectionDate;
    MoveCollectionType(export.CollectionType, useImport.CollectionType);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.FilterLegalAction.StandardNumber =
      export.FilterLegalAction.StandardNumber;
    useImport.FromFilter.DueDt = export.FromDateFilter.DueDt;
    useImport.ToFilter.DueDt = export.ToDateFilter.DueDt;
    useImport.FilterObligationType.SystemGeneratedIdentifier =
      export.FilterObligationType.SystemGeneratedIdentifier;
    useImport.DisplayAllObligInd.Flag = export.DisplayAllObligInd.Flag;
    useImport.TraceInd.Flag = export.TraceInd.Flag;
    useImport.Pgm.Text3 = export.Pgm.Text3;

    Call(FnRetrieveDebtsForMnlDist.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnVerifyCourtOrderFilter()
  {
    var useImport = new FnVerifyCourtOrderFilter.Import();
    var useExport = new FnVerifyCourtOrderFilter.Export();

    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.LegalAction.StandardNumber =
      export.FilterLegalAction.StandardNumber;

    Call(FnVerifyCourtOrderFilter.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    MoveLegalAction(export.FilterLegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScEabSignoff()
  {
    var useImport = new ScEabSignoff.Import();
    var useExport = new ScEabSignoff.Export();

    Call(ScEabSignoff.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.ObligorCsePerson.Number = useExport.CsePerson.Number;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.ObligorCsePersonsWorkSet);
  }

  private bool ReadCashReceiptDetail1()
  {
    entities.ExistingCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", export.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          export.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          export.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          export.CashReceiptType.SystemGeneratedIdentifier);
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
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CaseNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.OffsetTaxid =
          db.GetNullableInt32(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 11);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 18);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 19);
        entities.ExistingCashReceiptDetail.OverrideManualDistInd =
          db.GetNullableString(reader, 20);
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.ExistingCashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingAdjusted.Populated = false;

    return ReadEach("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingAdjusted.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingAdjusted.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingAdjusted.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingAdjusted.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.ExistingAdjusted.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.ExistingAdjusted.CollectionAmount = db.GetDecimal(reader, 5);
        entities.ExistingAdjusted.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailCashReceiptTypeCashReceiptSourceType()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;
    entities.ExistingCashReceiptType.Populated = false;
    entities.ExistingCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptTypeCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", export.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          export.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crSrceTypeId",
          export.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtypeId",
          export.CashReceiptType.SystemGeneratedIdentifier);
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
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CaseNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.OffsetTaxid =
          db.GetNullableInt32(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 11);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 18);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 19);
        entities.ExistingCashReceiptDetail.OverrideManualDistInd =
          db.GetNullableString(reader, 20);
        entities.ExistingCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 21);
        entities.ExistingCashReceiptType.Code = db.GetString(reader, 22);
        entities.ExistingCashReceiptType.CategoryIndicator =
          db.GetString(reader, 23);
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 24);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 25);
        entities.ExistingCashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 26);
        entities.ExistingCashReceiptSourceType.Populated = true;
        entities.ExistingCashReceiptType.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.ExistingCashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.ExistingCashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaxDiscontinue.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.ExistingCashReceiptDetail.CltIdentifier.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollectionType.Code = db.GetString(reader, 1);
        entities.ExistingCollectionType.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", export.FilterObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Populated = true;
      });
  }

  private bool ReadObligor()
  {
    entities.ExistingObligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          entities.ExistingCashReceiptDetail.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingObligor.CspNumber = db.GetString(reader, 0);
        entities.ExistingObligor.Type1 = db.GetString(reader, 1);
        entities.ExistingObligor.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingObligor.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.ExistingObligor.LastManualDistributionDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingObligor.LastCollAmt = db.GetNullableDecimal(reader, 5);
        entities.ExistingObligor.LastCollDt = db.GetNullableDate(reader, 6);
        entities.ExistingObligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.ExistingObligor.Type1);
      });
  }

  private IEnumerable<bool> ReadProgram()
  {
    return ReadEach("ReadProgram",
      null,
      (db, reader) =>
      {
        if (local.OfPgms.IsFull)
        {
          return false;
        }

        entities.ExistingProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingProgram.Code = db.GetString(reader, 1);
        entities.ExistingProgram.InterstateIndicator = db.GetString(reader, 2);
        entities.ExistingProgram.Populated = true;

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
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
      /// A value of ApplyToCode.
      /// </summary>
      [JsonPropertyName("applyToCode")]
      public TextWorkArea ApplyToCode
      {
        get => applyToCode ??= new();
        set => applyToCode = value;
      }

      /// <summary>
      /// A value of SuppPrsnCsePerson.
      /// </summary>
      [JsonPropertyName("suppPrsnCsePerson")]
      public CsePerson SuppPrsnCsePerson
      {
        get => suppPrsnCsePerson ??= new();
        set => suppPrsnCsePerson = value;
      }

      /// <summary>
      /// A value of SuppPrsnCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("suppPrsnCsePersonsWorkSet")]
      public CsePersonsWorkSet SuppPrsnCsePersonsWorkSet
      {
        get => suppPrsnCsePersonsWorkSet ??= new();
        set => suppPrsnCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Pgm.
      /// </summary>
      [JsonPropertyName("pgm")]
      public WorkArea Pgm
      {
        get => pgm ??= new();
        set => pgm = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 85;

      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction debt;
      private DebtDetail debtDetail;
      private LegalAction legalAction;
      private Collection collection;
      private TextWorkArea applyToCode;
      private CsePerson suppPrsnCsePerson;
      private CsePersonsWorkSet suppPrsnCsePersonsWorkSet;
      private WorkArea pgm;
      private Common common;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CrRefNumber.
    /// </summary>
    [JsonPropertyName("crRefNumber")]
    public CrdCrComboNo CrRefNumber
    {
      get => crRefNumber ??= new();
      set => crRefNumber = value;
    }

    /// <summary>
    /// A value of PromptLegalAction.
    /// </summary>
    [JsonPropertyName("promptLegalAction")]
    public TextWorkArea PromptLegalAction
    {
      get => promptLegalAction ??= new();
      set => promptLegalAction = value;
    }

    /// <summary>
    /// A value of FilterLegalAction.
    /// </summary>
    [JsonPropertyName("filterLegalAction")]
    public LegalAction FilterLegalAction
    {
      get => filterLegalAction ??= new();
      set => filterLegalAction = value;
    }

    /// <summary>
    /// A value of FromDateFilter.
    /// </summary>
    [JsonPropertyName("fromDateFilter")]
    public DebtDetail FromDateFilter
    {
      get => fromDateFilter ??= new();
      set => fromDateFilter = value;
    }

    /// <summary>
    /// A value of ToDateFilter.
    /// </summary>
    [JsonPropertyName("toDateFilter")]
    public DebtDetail ToDateFilter
    {
      get => toDateFilter ??= new();
      set => toDateFilter = value;
    }

    /// <summary>
    /// A value of PromptObligationType.
    /// </summary>
    [JsonPropertyName("promptObligationType")]
    public TextWorkArea PromptObligationType
    {
      get => promptObligationType ??= new();
      set => promptObligationType = value;
    }

    /// <summary>
    /// A value of FilterObligationType.
    /// </summary>
    [JsonPropertyName("filterObligationType")]
    public ObligationType FilterObligationType
    {
      get => filterObligationType ??= new();
      set => filterObligationType = value;
    }

    /// <summary>
    /// A value of TotalCollToDist.
    /// </summary>
    [JsonPropertyName("totalCollToDist")]
    public Common TotalCollToDist
    {
      get => totalCollToDist ??= new();
      set => totalCollToDist = value;
    }

    /// <summary>
    /// A value of DisplayAllObligInd.
    /// </summary>
    [JsonPropertyName("displayAllObligInd")]
    public Common DisplayAllObligInd
    {
      get => displayAllObligInd ??= new();
      set => displayAllObligInd = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of TraceInd.
    /// </summary>
    [JsonPropertyName("traceInd")]
    public Common TraceInd
    {
      get => traceInd ??= new();
      set => traceInd = value;
    }

    /// <summary>
    /// A value of Pgm.
    /// </summary>
    [JsonPropertyName("pgm")]
    public WorkArea Pgm
    {
      get => pgm ??= new();
      set => pgm = value;
    }

    /// <summary>
    /// A value of FromCdvl.
    /// </summary>
    [JsonPropertyName("fromCdvl")]
    public CodeValue FromCdvl
    {
      get => fromCdvl ??= new();
      set => fromCdvl = value;
    }

    /// <summary>
    /// A value of PromptPgm.
    /// </summary>
    [JsonPropertyName("promptPgm")]
    public Common PromptPgm
    {
      get => promptPgm ??= new();
      set => promptPgm = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private Standard standard;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private Array<GroupGroup> group;
    private Collection collection;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CrdCrComboNo crRefNumber;
    private TextWorkArea promptLegalAction;
    private LegalAction filterLegalAction;
    private DebtDetail fromDateFilter;
    private DebtDetail toDateFilter;
    private TextWorkArea promptObligationType;
    private ObligationType filterObligationType;
    private Common totalCollToDist;
    private Common displayAllObligInd;
    private ScreenOwedAmounts screenOwedAmounts;
    private NextTranInfo hidden;
    private Common traceInd;
    private WorkArea pgm;
    private CodeValue fromCdvl;
    private Common promptPgm;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
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
      /// A value of ApplyToCode.
      /// </summary>
      [JsonPropertyName("applyToCode")]
      public TextWorkArea ApplyToCode
      {
        get => applyToCode ??= new();
        set => applyToCode = value;
      }

      /// <summary>
      /// A value of SuppPrsnCsePerson.
      /// </summary>
      [JsonPropertyName("suppPrsnCsePerson")]
      public CsePerson SuppPrsnCsePerson
      {
        get => suppPrsnCsePerson ??= new();
        set => suppPrsnCsePerson = value;
      }

      /// <summary>
      /// A value of SuppPrsnCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("suppPrsnCsePersonsWorkSet")]
      public CsePersonsWorkSet SuppPrsnCsePersonsWorkSet
      {
        get => suppPrsnCsePersonsWorkSet ??= new();
        set => suppPrsnCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Pgm.
      /// </summary>
      [JsonPropertyName("pgm")]
      public WorkArea Pgm
      {
        get => pgm ??= new();
        set => pgm = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 85;

      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction debt;
      private DebtDetail debtDetail;
      private LegalAction legalAction;
      private Collection collection;
      private TextWorkArea applyToCode;
      private CsePerson suppPrsnCsePerson;
      private CsePersonsWorkSet suppPrsnCsePersonsWorkSet;
      private WorkArea pgm;
      private Common common;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CrRefNumber.
    /// </summary>
    [JsonPropertyName("crRefNumber")]
    public CrdCrComboNo CrRefNumber
    {
      get => crRefNumber ??= new();
      set => crRefNumber = value;
    }

    /// <summary>
    /// A value of PromptLegalAction.
    /// </summary>
    [JsonPropertyName("promptLegalAction")]
    public TextWorkArea PromptLegalAction
    {
      get => promptLegalAction ??= new();
      set => promptLegalAction = value;
    }

    /// <summary>
    /// A value of FilterLegalAction.
    /// </summary>
    [JsonPropertyName("filterLegalAction")]
    public LegalAction FilterLegalAction
    {
      get => filterLegalAction ??= new();
      set => filterLegalAction = value;
    }

    /// <summary>
    /// A value of FromDateFilter.
    /// </summary>
    [JsonPropertyName("fromDateFilter")]
    public DebtDetail FromDateFilter
    {
      get => fromDateFilter ??= new();
      set => fromDateFilter = value;
    }

    /// <summary>
    /// A value of ToDateFilter.
    /// </summary>
    [JsonPropertyName("toDateFilter")]
    public DebtDetail ToDateFilter
    {
      get => toDateFilter ??= new();
      set => toDateFilter = value;
    }

    /// <summary>
    /// A value of PromptObligationType.
    /// </summary>
    [JsonPropertyName("promptObligationType")]
    public TextWorkArea PromptObligationType
    {
      get => promptObligationType ??= new();
      set => promptObligationType = value;
    }

    /// <summary>
    /// A value of FilterObligationType.
    /// </summary>
    [JsonPropertyName("filterObligationType")]
    public ObligationType FilterObligationType
    {
      get => filterObligationType ??= new();
      set => filterObligationType = value;
    }

    /// <summary>
    /// A value of TotalCollToDist.
    /// </summary>
    [JsonPropertyName("totalCollToDist")]
    public Common TotalCollToDist
    {
      get => totalCollToDist ??= new();
      set => totalCollToDist = value;
    }

    /// <summary>
    /// A value of DisplayAllObligInd.
    /// </summary>
    [JsonPropertyName("displayAllObligInd")]
    public Common DisplayAllObligInd
    {
      get => displayAllObligInd ??= new();
      set => displayAllObligInd = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of DlgflwRequiredLegalAction.
    /// </summary>
    [JsonPropertyName("dlgflwRequiredLegalAction")]
    public LegalAction DlgflwRequiredLegalAction
    {
      get => dlgflwRequiredLegalAction ??= new();
      set => dlgflwRequiredLegalAction = value;
    }

    /// <summary>
    /// A value of DlgflwRequiredObligation.
    /// </summary>
    [JsonPropertyName("dlgflwRequiredObligation")]
    public Obligation DlgflwRequiredObligation
    {
      get => dlgflwRequiredObligation ??= new();
      set => dlgflwRequiredObligation = value;
    }

    /// <summary>
    /// A value of DlgflwRequiredObligationType.
    /// </summary>
    [JsonPropertyName("dlgflwRequiredObligationType")]
    public ObligationType DlgflwRequiredObligationType
    {
      get => dlgflwRequiredObligationType ??= new();
      set => dlgflwRequiredObligationType = value;
    }

    /// <summary>
    /// A value of TraceInd.
    /// </summary>
    [JsonPropertyName("traceInd")]
    public Common TraceInd
    {
      get => traceInd ??= new();
      set => traceInd = value;
    }

    /// <summary>
    /// A value of ZdelExportObligor.
    /// </summary>
    [JsonPropertyName("zdelExportObligor")]
    public CsePersonsWorkSet ZdelExportObligor
    {
      get => zdelExportObligor ??= new();
      set => zdelExportObligor = value;
    }

    /// <summary>
    /// A value of Pgm.
    /// </summary>
    [JsonPropertyName("pgm")]
    public WorkArea Pgm
    {
      get => pgm ??= new();
      set => pgm = value;
    }

    /// <summary>
    /// A value of ToCdvl.
    /// </summary>
    [JsonPropertyName("toCdvl")]
    public Code ToCdvl
    {
      get => toCdvl ??= new();
      set => toCdvl = value;
    }

    /// <summary>
    /// A value of PromptPgm.
    /// </summary>
    [JsonPropertyName("promptPgm")]
    public Common PromptPgm
    {
      get => promptPgm ??= new();
      set => promptPgm = value;
    }

    private Standard standard;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private Array<GroupGroup> group;
    private Collection collection;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CrdCrComboNo crRefNumber;
    private TextWorkArea promptLegalAction;
    private LegalAction filterLegalAction;
    private DebtDetail fromDateFilter;
    private DebtDetail toDateFilter;
    private TextWorkArea promptObligationType;
    private ObligationType filterObligationType;
    private Common totalCollToDist;
    private Common displayAllObligInd;
    private ScreenOwedAmounts screenOwedAmounts;
    private NextTranInfo hidden;
    private LegalAction dlgflwRequiredLegalAction;
    private Obligation dlgflwRequiredObligation;
    private ObligationType dlgflwRequiredObligationType;
    private Common traceInd;
    private CsePersonsWorkSet zdelExportObligor;
    private WorkArea pgm;
    private Code toCdvl;
    private Common promptPgm;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A PgmHistGroup group.</summary>
    [Serializable]
    public class PgmHistGroup
    {
      /// <summary>
      /// A value of PgmHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("pgmHistSuppPrsn")]
      public CsePerson PgmHistSuppPrsn
      {
        get => pgmHistSuppPrsn ??= new();
        set => pgmHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of PgmHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<PgmHistDtlGroup> PgmHistDtl => pgmHistDtl ??= new(
        PgmHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of PgmHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("pgmHistDtl")]
      [Computed]
      public IList<PgmHistDtlGroup> PgmHistDtl_Json
      {
        get => pgmHistDtl;
        set => PgmHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson pgmHistSuppPrsn;
      private Array<PgmHistDtlGroup> pgmHistDtl;
    }

    /// <summary>A PgmHistDtlGroup group.</summary>
    [Serializable]
    public class PgmHistDtlGroup
    {
      /// <summary>
      /// A value of PgmHistDtlProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlProgram")]
      public Program PgmHistDtlProgram
      {
        get => pgmHistDtlProgram ??= new();
        set => pgmHistDtlProgram = value;
      }

      /// <summary>
      /// A value of PgmHistDtlPersonProgram.
      /// </summary>
      [JsonPropertyName("pgmHistDtlPersonProgram")]
      public PersonProgram PgmHistDtlPersonProgram
      {
        get => pgmHistDtlPersonProgram ??= new();
        set => pgmHistDtlPersonProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private Program pgmHistDtlProgram;
      private PersonProgram pgmHistDtlPersonProgram;
    }

    /// <summary>A LegalGroup group.</summary>
    [Serializable]
    public class LegalGroup
    {
      /// <summary>
      /// A value of LegalSuppPrsn.
      /// </summary>
      [JsonPropertyName("legalSuppPrsn")]
      public CsePerson LegalSuppPrsn
      {
        get => legalSuppPrsn ??= new();
        set => legalSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of LegalDtl.
      /// </summary>
      [JsonIgnore]
      public Array<LegalDtlGroup> LegalDtl => legalDtl ??= new(
        LegalDtlGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of LegalDtl for json serialization.
      /// </summary>
      [JsonPropertyName("legalDtl")]
      [Computed]
      public IList<LegalDtlGroup> LegalDtl_Json
      {
        get => legalDtl;
        set => LegalDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson legalSuppPrsn;
      private Array<LegalDtlGroup> legalDtl;
    }

    /// <summary>A LegalDtlGroup group.</summary>
    [Serializable]
    public class LegalDtlGroup
    {
      /// <summary>
      /// A value of LegalDtl1.
      /// </summary>
      [JsonPropertyName("legalDtl1")]
      public LegalAction LegalDtl1
      {
        get => legalDtl1 ??= new();
        set => legalDtl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private LegalAction legalDtl1;
    }

    /// <summary>A HhHistGroup group.</summary>
    [Serializable]
    public class HhHistGroup
    {
      /// <summary>
      /// A value of HhHistSuppPrsn.
      /// </summary>
      [JsonPropertyName("hhHistSuppPrsn")]
      public CsePerson HhHistSuppPrsn
      {
        get => hhHistSuppPrsn ??= new();
        set => hhHistSuppPrsn = value;
      }

      /// <summary>
      /// Gets a value of HhHistDtl.
      /// </summary>
      [JsonIgnore]
      public Array<HhHistDtlGroup> HhHistDtl => hhHistDtl ??= new(
        HhHistDtlGroup.Capacity);

      /// <summary>
      /// Gets a value of HhHistDtl for json serialization.
      /// </summary>
      [JsonPropertyName("hhHistDtl")]
      [Computed]
      public IList<HhHistDtlGroup> HhHistDtl_Json
      {
        get => hhHistDtl;
        set => HhHistDtl.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hhHistSuppPrsn;
      private Array<HhHistDtlGroup> hhHistDtl;
    }

    /// <summary>A HhHistDtlGroup group.</summary>
    [Serializable]
    public class HhHistDtlGroup
    {
      /// <summary>
      /// A value of HhHistDtlImHousehold.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHousehold")]
      public ImHousehold HhHistDtlImHousehold
      {
        get => hhHistDtlImHousehold ??= new();
        set => hhHistDtlImHousehold = value;
      }

      /// <summary>
      /// A value of HhHistDtlImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("hhHistDtlImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum HhHistDtlImHouseholdMbrMnthlySum
      {
        get => hhHistDtlImHouseholdMbrMnthlySum ??= new();
        set => hhHistDtlImHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private ImHousehold hhHistDtlImHousehold;
      private ImHouseholdMbrMnthlySum hhHistDtlImHouseholdMbrMnthlySum;
    }

    /// <summary>A HhHistProcGroup group.</summary>
    [Serializable]
    public class HhHistProcGroup
    {
      /// <summary>
      /// A value of HhHistSuppPrsnProc.
      /// </summary>
      [JsonPropertyName("hhHistSuppPrsnProc")]
      public CsePerson HhHistSuppPrsnProc
      {
        get => hhHistSuppPrsnProc ??= new();
        set => hhHistSuppPrsnProc = value;
      }

      /// <summary>
      /// Gets a value of HhHistDtlProc.
      /// </summary>
      [JsonIgnore]
      public Array<HhHistDtlProcGroup> HhHistDtlProc => hhHistDtlProc ??= new(
        HhHistDtlProcGroup.Capacity);

      /// <summary>
      /// Gets a value of HhHistDtlProc for json serialization.
      /// </summary>
      [JsonPropertyName("hhHistDtlProc")]
      [Computed]
      public IList<HhHistDtlProcGroup> HhHistDtlProc_Json
      {
        get => hhHistDtlProc;
        set => HhHistDtlProc.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson hhHistSuppPrsnProc;
      private Array<HhHistDtlProcGroup> hhHistDtlProc;
    }

    /// <summary>A HhHistDtlProcGroup group.</summary>
    [Serializable]
    public class HhHistDtlProcGroup
    {
      /// <summary>
      /// A value of HhHistDtlProcImHousehold.
      /// </summary>
      [JsonPropertyName("hhHistDtlProcImHousehold")]
      public ImHousehold HhHistDtlProcImHousehold
      {
        get => hhHistDtlProcImHousehold ??= new();
        set => hhHistDtlProcImHousehold = value;
      }

      /// <summary>
      /// A value of HhHistDtlProcImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("hhHistDtlProcImHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum HhHistDtlProcImHouseholdMbrMnthlySum
      {
        get => hhHistDtlProcImHouseholdMbrMnthlySum ??= new();
        set => hhHistDtlProcImHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private ImHousehold hhHistDtlProcImHousehold;
      private ImHouseholdMbrMnthlySum hhHistDtlProcImHouseholdMbrMnthlySum;
    }

    /// <summary>A DebtsGroup group.</summary>
    [Serializable]
    public class DebtsGroup
    {
      /// <summary>
      /// A value of DebtsObligationType.
      /// </summary>
      [JsonPropertyName("debtsObligationType")]
      public ObligationType DebtsObligationType
      {
        get => debtsObligationType ??= new();
        set => debtsObligationType = value;
      }

      /// <summary>
      /// A value of DebtsObligation.
      /// </summary>
      [JsonPropertyName("debtsObligation")]
      public Obligation DebtsObligation
      {
        get => debtsObligation ??= new();
        set => debtsObligation = value;
      }

      /// <summary>
      /// A value of DebtsDebt.
      /// </summary>
      [JsonPropertyName("debtsDebt")]
      public ObligationTransaction DebtsDebt
      {
        get => debtsDebt ??= new();
        set => debtsDebt = value;
      }

      /// <summary>
      /// A value of DebtsCollection.
      /// </summary>
      [JsonPropertyName("debtsCollection")]
      public Collection DebtsCollection
      {
        get => debtsCollection ??= new();
        set => debtsCollection = value;
      }

      /// <summary>
      /// A value of DebtsSuppPrsn.
      /// </summary>
      [JsonPropertyName("debtsSuppPrsn")]
      public CsePerson DebtsSuppPrsn
      {
        get => debtsSuppPrsn ??= new();
        set => debtsSuppPrsn = value;
      }

      /// <summary>
      /// A value of DebtsProgram.
      /// </summary>
      [JsonPropertyName("debtsProgram")]
      public Program DebtsProgram
      {
        get => debtsProgram ??= new();
        set => debtsProgram = value;
      }

      /// <summary>
      /// A value of DebtsDprProgram.
      /// </summary>
      [JsonPropertyName("debtsDprProgram")]
      public DprProgram DebtsDprProgram
      {
        get => debtsDprProgram ??= new();
        set => debtsDprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1500;

      private ObligationType debtsObligationType;
      private Obligation debtsObligation;
      private ObligationTransaction debtsDebt;
      private Collection debtsCollection;
      private CsePerson debtsSuppPrsn;
      private Program debtsProgram;
      private DprProgram debtsDprProgram;
    }

    /// <summary>A OfPgmsGroup group.</summary>
    [Serializable]
    public class OfPgmsGroup
    {
      /// <summary>
      /// A value of OfPgms1.
      /// </summary>
      [JsonPropertyName("ofPgms1")]
      public Program OfPgms1
      {
        get => ofPgms1 ??= new();
        set => ofPgms1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Program ofPgms1;
    }

    /// <summary>
    /// Gets a value of PgmHist.
    /// </summary>
    [JsonIgnore]
    public Array<PgmHistGroup> PgmHist =>
      pgmHist ??= new(PgmHistGroup.Capacity);

    /// <summary>
    /// Gets a value of PgmHist for json serialization.
    /// </summary>
    [JsonPropertyName("pgmHist")]
    [Computed]
    public IList<PgmHistGroup> PgmHist_Json
    {
      get => pgmHist;
      set => PgmHist.Assign(value);
    }

    /// <summary>
    /// Gets a value of Legal.
    /// </summary>
    [JsonIgnore]
    public Array<LegalGroup> Legal => legal ??= new(LegalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Legal for json serialization.
    /// </summary>
    [JsonPropertyName("legal")]
    [Computed]
    public IList<LegalGroup> Legal_Json
    {
      get => legal;
      set => Legal.Assign(value);
    }

    /// <summary>
    /// Gets a value of HhHist.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistGroup> HhHist => hhHist ??= new(HhHistGroup.Capacity);

    /// <summary>
    /// Gets a value of HhHist for json serialization.
    /// </summary>
    [JsonPropertyName("hhHist")]
    [Computed]
    public IList<HhHistGroup> HhHist_Json
    {
      get => hhHist;
      set => HhHist.Assign(value);
    }

    /// <summary>
    /// Gets a value of HhHistProc.
    /// </summary>
    [JsonIgnore]
    public Array<HhHistProcGroup> HhHistProc => hhHistProc ??= new(
      HhHistProcGroup.Capacity);

    /// <summary>
    /// Gets a value of HhHistProc for json serialization.
    /// </summary>
    [JsonPropertyName("hhHistProc")]
    [Computed]
    public IList<HhHistProcGroup> HhHistProc_Json
    {
      get => hhHistProc;
      set => HhHistProc.Assign(value);
    }

    /// <summary>
    /// A value of Hardcoded718B.
    /// </summary>
    [JsonPropertyName("hardcoded718B")]
    public ObligationType Hardcoded718B
    {
      get => hardcoded718B ??= new();
      set => hardcoded718B = value;
    }

    /// <summary>
    /// A value of UserId.
    /// </summary>
    [JsonPropertyName("userId")]
    public TextWorkArea UserId
    {
      get => userId ??= new();
      set => userId = value;
    }

    /// <summary>
    /// A value of HardcodedVType.
    /// </summary>
    [JsonPropertyName("hardcodedVType")]
    public CollectionType HardcodedVType
    {
      get => hardcodedVType ??= new();
      set => hardcodedVType = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public ObligationType Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Coll.
    /// </summary>
    [JsonPropertyName("coll")]
    public DateWorkArea Coll
    {
      get => coll ??= new();
      set => coll = value;
    }

    /// <summary>
    /// A value of CollLastOfMth.
    /// </summary>
    [JsonPropertyName("collLastOfMth")]
    public DateWorkArea CollLastOfMth
    {
      get => collLastOfMth ??= new();
      set => collLastOfMth = value;
    }

    /// <summary>
    /// A value of CollFirstOfMth.
    /// </summary>
    [JsonPropertyName("collFirstOfMth")]
    public DateWorkArea CollFirstOfMth
    {
      get => collFirstOfMth ??= new();
      set => collFirstOfMth = value;
    }

    /// <summary>
    /// Gets a value of Debts.
    /// </summary>
    [JsonIgnore]
    public Array<DebtsGroup> Debts => debts ??= new(DebtsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Debts for json serialization.
    /// </summary>
    [JsonPropertyName("debts")]
    [Computed]
    public IList<DebtsGroup> Debts_Json
    {
      get => debts;
      set => Debts.Assign(value);
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
    /// A value of TotalSelected.
    /// </summary>
    [JsonPropertyName("totalSelected")]
    public Common TotalSelected
    {
      get => totalSelected ??= new();
      set => totalSelected = value;
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
    /// A value of Manual.
    /// </summary>
    [JsonPropertyName("manual")]
    public Collection Manual
    {
      get => manual ??= new();
      set => manual = value;
    }

    /// <summary>
    /// A value of TotalDistAmt.
    /// </summary>
    [JsonPropertyName("totalDistAmt")]
    public Collection TotalDistAmt
    {
      get => totalDistAmt ??= new();
      set => totalDistAmt = value;
    }

    /// <summary>
    /// A value of TotalDistAmtToSecOb.
    /// </summary>
    [JsonPropertyName("totalDistAmtToSecOb")]
    public Collection TotalDistAmtToSecOb
    {
      get => totalDistAmtToSecOb ??= new();
      set => totalDistAmtToSecOb = value;
    }

    /// <summary>
    /// A value of HardcodedSType.
    /// </summary>
    [JsonPropertyName("hardcodedSType")]
    public CollectionType HardcodedSType
    {
      get => hardcodedSType ??= new();
      set => hardcodedSType = value;
    }

    /// <summary>
    /// A value of HardcodedFType.
    /// </summary>
    [JsonPropertyName("hardcodedFType")]
    public CollectionType HardcodedFType
    {
      get => hardcodedFType ??= new();
      set => hardcodedFType = value;
    }

    /// <summary>
    /// A value of HardcodedFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFdirPmt")]
    public CashReceiptType HardcodedFdirPmt
    {
      get => hardcodedFdirPmt ??= new();
      set => hardcodedFdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedFcourtPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFcourtPmt")]
    public CashReceiptType HardcodedFcourtPmt
    {
      get => hardcodedFcourtPmt ??= new();
      set => hardcodedFcourtPmt = value;
    }

    /// <summary>
    /// A value of HardcodedArrears.
    /// </summary>
    [JsonPropertyName("hardcodedArrears")]
    public Collection HardcodedArrears
    {
      get => hardcodedArrears ??= new();
      set => hardcodedArrears = value;
    }

    /// <summary>
    /// A value of HardcodedCurrent.
    /// </summary>
    [JsonPropertyName("hardcodedCurrent")]
    public Collection HardcodedCurrent
    {
      get => hardcodedCurrent ??= new();
      set => hardcodedCurrent = value;
    }

    /// <summary>
    /// A value of HardcodedDeactivedStat.
    /// </summary>
    [JsonPropertyName("hardcodedDeactivedStat")]
    public DebtDetailStatusHistory HardcodedDeactivedStat
    {
      get => hardcodedDeactivedStat ??= new();
      set => hardcodedDeactivedStat = value;
    }

    /// <summary>
    /// A value of HardcodedDistributed.
    /// </summary>
    [JsonPropertyName("hardcodedDistributed")]
    public CashReceiptDetailStatus HardcodedDistributed
    {
      get => hardcodedDistributed ??= new();
      set => hardcodedDistributed = value;
    }

    /// <summary>
    /// A value of HardcodedJointSeveralObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("hardcodedJointSeveralObligationRlnRsn")]
    public ObligationRlnRsn HardcodedJointSeveralObligationRlnRsn
    {
      get => hardcodedJointSeveralObligationRlnRsn ??= new();
      set => hardcodedJointSeveralObligationRlnRsn = value;
    }

    /// <summary>
    /// A value of HardcodedCashType.
    /// </summary>
    [JsonPropertyName("hardcodedCashType")]
    public CashReceiptType HardcodedCashType
    {
      get => hardcodedCashType ??= new();
      set => hardcodedCashType = value;
    }

    /// <summary>
    /// A value of HardcodedJointSeveralObligation.
    /// </summary>
    [JsonPropertyName("hardcodedJointSeveralObligation")]
    public Obligation HardcodedJointSeveralObligation
    {
      get => hardcodedJointSeveralObligation ??= new();
      set => hardcodedJointSeveralObligation = value;
    }

    /// <summary>
    /// A value of HardcodedPrimary.
    /// </summary>
    [JsonPropertyName("hardcodedPrimary")]
    public Obligation HardcodedPrimary
    {
      get => hardcodedPrimary ??= new();
      set => hardcodedPrimary = value;
    }

    /// <summary>
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
    }

    /// <summary>
    /// A value of HardcodedPended.
    /// </summary>
    [JsonPropertyName("hardcodedPended")]
    public CashReceiptDetailStatus HardcodedPended
    {
      get => hardcodedPended ??= new();
      set => hardcodedPended = value;
    }

    /// <summary>
    /// A value of HardcodedRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedRecorded")]
    public CashReceiptDetailStatus HardcodedRecorded
    {
      get => hardcodedRecorded ??= new();
      set => hardcodedRecorded = value;
    }

    /// <summary>
    /// A value of HardcodedVoluntaryClass.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntaryClass")]
    public ObligationType HardcodedVoluntaryClass
    {
      get => hardcodedVoluntaryClass ??= new();
      set => hardcodedVoluntaryClass = value;
    }

    /// <summary>
    /// A value of HardcodedAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodedAccruingClass")]
    public ObligationType HardcodedAccruingClass
    {
      get => hardcodedAccruingClass ??= new();
      set => hardcodedAccruingClass = value;
    }

    /// <summary>
    /// A value of HardcodedAf.
    /// </summary>
    [JsonPropertyName("hardcodedAf")]
    public Program HardcodedAf
    {
      get => hardcodedAf ??= new();
      set => hardcodedAf = value;
    }

    /// <summary>
    /// A value of HardcodedAfi.
    /// </summary>
    [JsonPropertyName("hardcodedAfi")]
    public Program HardcodedAfi
    {
      get => hardcodedAfi ??= new();
      set => hardcodedAfi = value;
    }

    /// <summary>
    /// A value of HardcodedFc.
    /// </summary>
    [JsonPropertyName("hardcodedFc")]
    public Program HardcodedFc
    {
      get => hardcodedFc ??= new();
      set => hardcodedFc = value;
    }

    /// <summary>
    /// A value of HardcodedFci.
    /// </summary>
    [JsonPropertyName("hardcodedFci")]
    public Program HardcodedFci
    {
      get => hardcodedFci ??= new();
      set => hardcodedFci = value;
    }

    /// <summary>
    /// A value of HardcodedNaProgram.
    /// </summary>
    [JsonPropertyName("hardcodedNaProgram")]
    public Program HardcodedNaProgram
    {
      get => hardcodedNaProgram ??= new();
      set => hardcodedNaProgram = value;
    }

    /// <summary>
    /// A value of HardcodedNai.
    /// </summary>
    [JsonPropertyName("hardcodedNai")]
    public Program HardcodedNai
    {
      get => hardcodedNai ??= new();
      set => hardcodedNai = value;
    }

    /// <summary>
    /// A value of HardcodedNc.
    /// </summary>
    [JsonPropertyName("hardcodedNc")]
    public Program HardcodedNc
    {
      get => hardcodedNc ??= new();
      set => hardcodedNc = value;
    }

    /// <summary>
    /// A value of HardcodedNf.
    /// </summary>
    [JsonPropertyName("hardcodedNf")]
    public Program HardcodedNf
    {
      get => hardcodedNf ??= new();
      set => hardcodedNf = value;
    }

    /// <summary>
    /// A value of HardcodedMai.
    /// </summary>
    [JsonPropertyName("hardcodedMai")]
    public Program HardcodedMai
    {
      get => hardcodedMai ??= new();
      set => hardcodedMai = value;
    }

    /// <summary>
    /// A value of HardcodedNaDprProgram.
    /// </summary>
    [JsonPropertyName("hardcodedNaDprProgram")]
    public DprProgram HardcodedNaDprProgram
    {
      get => hardcodedNaDprProgram ??= new();
      set => hardcodedNaDprProgram = value;
    }

    /// <summary>
    /// A value of HardcodedPa.
    /// </summary>
    [JsonPropertyName("hardcodedPa")]
    public DprProgram HardcodedPa
    {
      get => hardcodedPa ??= new();
      set => hardcodedPa = value;
    }

    /// <summary>
    /// A value of HardcodedTa.
    /// </summary>
    [JsonPropertyName("hardcodedTa")]
    public DprProgram HardcodedTa
    {
      get => hardcodedTa ??= new();
      set => hardcodedTa = value;
    }

    /// <summary>
    /// A value of HardcodedCa.
    /// </summary>
    [JsonPropertyName("hardcodedCa")]
    public DprProgram HardcodedCa
    {
      get => hardcodedCa ??= new();
      set => hardcodedCa = value;
    }

    /// <summary>
    /// A value of HardcodedUd.
    /// </summary>
    [JsonPropertyName("hardcodedUd")]
    public DprProgram HardcodedUd
    {
      get => hardcodedUd ??= new();
      set => hardcodedUd = value;
    }

    /// <summary>
    /// A value of HardcodedUp.
    /// </summary>
    [JsonPropertyName("hardcodedUp")]
    public DprProgram HardcodedUp
    {
      get => hardcodedUp ??= new();
      set => hardcodedUp = value;
    }

    /// <summary>
    /// A value of HardcodedUk.
    /// </summary>
    [JsonPropertyName("hardcodedUk")]
    public DprProgram HardcodedUk
    {
      get => hardcodedUk ??= new();
      set => hardcodedUk = value;
    }

    /// <summary>
    /// A value of HardcodedMsType.
    /// </summary>
    [JsonPropertyName("hardcodedMsType")]
    public ObligationType HardcodedMsType
    {
      get => hardcodedMsType ??= new();
      set => hardcodedMsType = value;
    }

    /// <summary>
    /// A value of HardcodedMjType.
    /// </summary>
    [JsonPropertyName("hardcodedMjType")]
    public ObligationType HardcodedMjType
    {
      get => hardcodedMjType ??= new();
      set => hardcodedMjType = value;
    }

    /// <summary>
    /// A value of HardcodedMcType.
    /// </summary>
    [JsonPropertyName("hardcodedMcType")]
    public ObligationType HardcodedMcType
    {
      get => hardcodedMcType ??= new();
      set => hardcodedMcType = value;
    }

    /// <summary>
    /// A value of PrworaDateOfConversion.
    /// </summary>
    [JsonPropertyName("prworaDateOfConversion")]
    public DateWorkArea PrworaDateOfConversion
    {
      get => prworaDateOfConversion ??= new();
      set => prworaDateOfConversion = value;
    }

    /// <summary>
    /// Gets a value of OfPgms.
    /// </summary>
    [JsonIgnore]
    public Array<OfPgmsGroup> OfPgms => ofPgms ??= new(OfPgmsGroup.Capacity);

    /// <summary>
    /// Gets a value of OfPgms for json serialization.
    /// </summary>
    [JsonPropertyName("ofPgms")]
    [Computed]
    public IList<OfPgmsGroup> OfPgms_Json
    {
      get => ofPgms;
      set => OfPgms.Assign(value);
    }

    private Array<PgmHistGroup> pgmHist;
    private Array<LegalGroup> legal;
    private Array<HhHistGroup> hhHist;
    private Array<HhHistProcGroup> hhHistProc;
    private ObligationType hardcoded718B;
    private TextWorkArea userId;
    private CollectionType hardcodedVType;
    private DateWorkArea null1;
    private ObligationType initialized;
    private DateWorkArea coll;
    private DateWorkArea collLastOfMth;
    private DateWorkArea collFirstOfMth;
    private Array<DebtsGroup> debts;
    private DateWorkArea maxDiscontinue;
    private Common totalSelected;
    private DateWorkArea current;
    private Collection manual;
    private Collection totalDistAmt;
    private Collection totalDistAmtToSecOb;
    private CollectionType hardcodedSType;
    private CollectionType hardcodedFType;
    private CashReceiptType hardcodedFdirPmt;
    private CashReceiptType hardcodedFcourtPmt;
    private Collection hardcodedArrears;
    private Collection hardcodedCurrent;
    private DebtDetailStatusHistory hardcodedDeactivedStat;
    private CashReceiptDetailStatus hardcodedDistributed;
    private ObligationRlnRsn hardcodedJointSeveralObligationRlnRsn;
    private CashReceiptType hardcodedCashType;
    private Obligation hardcodedJointSeveralObligation;
    private Obligation hardcodedPrimary;
    private Obligation hardcodedSecondary;
    private CashReceiptDetailStatus hardcodedPended;
    private CashReceiptDetailStatus hardcodedRecorded;
    private ObligationType hardcodedVoluntaryClass;
    private ObligationType hardcodedAccruingClass;
    private Program hardcodedAf;
    private Program hardcodedAfi;
    private Program hardcodedFc;
    private Program hardcodedFci;
    private Program hardcodedNaProgram;
    private Program hardcodedNai;
    private Program hardcodedNc;
    private Program hardcodedNf;
    private Program hardcodedMai;
    private DprProgram hardcodedNaDprProgram;
    private DprProgram hardcodedPa;
    private DprProgram hardcodedTa;
    private DprProgram hardcodedCa;
    private DprProgram hardcodedUd;
    private DprProgram hardcodedUp;
    private DprProgram hardcodedUk;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMjType;
    private ObligationType hardcodedMcType;
    private DateWorkArea prworaDateOfConversion;
    private Array<OfPgmsGroup> ofPgms;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnly.
    /// </summary>
    [JsonPropertyName("existingKeyOnly")]
    public CsePerson ExistingKeyOnly
    {
      get => existingKeyOnly ??= new();
      set => existingKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
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
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExistingAdjusted.
    /// </summary>
    [JsonPropertyName("existingAdjusted")]
    public CashReceiptDetail ExistingAdjusted
    {
      get => existingAdjusted ??= new();
      set => existingAdjusted = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj ExistingCashReceiptDetailBalanceAdj
    {
      get => existingCashReceiptDetailBalanceAdj ??= new();
      set => existingCashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of ExistingCollectionType.
    /// </summary>
    [JsonPropertyName("existingCollectionType")]
    public CollectionType ExistingCollectionType
    {
      get => existingCollectionType ??= new();
      set => existingCollectionType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ExistingCashReceiptDetailStatus
    {
      get => existingCashReceiptDetailStatus ??= new();
      set => existingCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligor.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligor")]
    public CsePerson ExistingKeyOnlyObligor
    {
      get => existingKeyOnlyObligor ??= new();
      set => existingKeyOnlyObligor = value;
    }

    /// <summary>
    /// A value of ExistingProgram.
    /// </summary>
    [JsonPropertyName("existingProgram")]
    public Program ExistingProgram
    {
      get => existingProgram ??= new();
      set => existingProgram = value;
    }

    private DebtDetail existingDebtDetail;
    private CsePerson existingKeyOnly;
    private CsePersonAccount existingObligor;
    private ObligationType existingObligationType;
    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceiptType existingCashReceiptType;
    private CashReceipt existingCashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
    private CashReceiptDetail existingAdjusted;
    private CashReceiptDetailBalanceAdj existingCashReceiptDetailBalanceAdj;
    private CollectionType existingCollectionType;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus existingCashReceiptDetailStatus;
    private CsePerson existingKeyOnlyObligor;
    private Program existingProgram;
  }
#endregion
}
