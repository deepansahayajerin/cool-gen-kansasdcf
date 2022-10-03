// Program: FN_MDIS_LST_MTN_MANUAL_DIST_INST, ID: 372039598, model: 746.
// Short name: SWEMDISP
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
/// A program: FN_MDIS_LST_MTN_MANUAL_DIST_INST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnMdisLstMtnManualDistInst: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_MDIS_LST_MTN_MANUAL_DIST_INST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnMdisLstMtnManualDistInst(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnMdisLstMtnManualDistInst.
  /// </summary>
  public FnMdisLstMtnManualDistInst(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // Date     Developer Name   Request#      Description
    // 02/16/96  Rick Delgado                  New Development
    // 12/11/96  R. Marchman		        Add new security and next tran
    // 07/23/97  A Samuels MTW		        Restricted Disc Date to >= current date
    // 07/23/97  A Samuels MTW		        Initialize group view prior to display
    // 07/24/97  A Samuels MTW			Correction to display of 'Obligation Amount'
    // 10-27-98  G Sharp          Phase 2      1. Clean up of exit state's that 
    // are zdelete
    //                                         
    // 2. Added Frequency to screen.
    //                                         
    // 3. Added Interstate Ind. to screen.
    //                                         
    // 4. Added Oblig Id to screen.
    //                                         
    // 5. Remove logic calculating Obligation
    // Amount and added logic to use of AB
    // fn_cab_set_accrual_or_due_amount
    //                                         
    // 6. Added logic on delete. When effective
    // < current date, then check if
    // discontinue date < current date. If so
    // difference error message.
    //                                         
    // 7. Added logic on Add and Update.
    // discontine date must be greater than
    // effective date.
    //                                         
    // 8. added logic to carrier ces_person
    // number in next tran.
    // 08-12-02  K Doshi         PR149011     Fix screen Help Id.
    // 05-15-09  Arun Mathias    CQ#9804     Allow to delete record with future 
    // effective date, or a record that was added on the same day. Also, give
    //                                       
    // appropriate message when deleting a
    // blank record.
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    // ---------------------------------------------
    // The following details should not be cleared on CLEAR key
    // ---------------------------------------------
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonAccount.Type1 = import.CsePersonAccount.Type1;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    MoveObligation(import.Obligation, export.Obligation);
    export.ObligationType.Assign(import.ObligationType);
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    MoveFrequencyWorkSet(import.FrequencyWorkSet, export.FrequencyWorkSet);
    export.AccrualOrDue.Date = import.AccrualOrDue.Date;
    export.TotalAmountDue.TotalCurrency = import.TotalAmountDue.TotalCurrency;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.ShowHistory.Flag = import.ShowHistory.Flag;
    export.Starting.EffectiveDt = import.Starting.EffectiveDt;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.Group.Index = -1;

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Update.ManualDistributionAudit.Assign(
          import.Group.Item.ManualDistributionAudit);
        export.Group.Update.Hidden.DiscontinueDt =
          import.Group.Item.Hidden.DiscontinueDt;

        if (!IsEmpty(export.Group.Item.ManualDistributionAudit.CreatedBy))
        {
          var field =
            GetField(export.Group.Item.ManualDistributionAudit, "effectiveDt");

          field.Color = "cyan";
          field.Protected = true;
        }

        // *******  Protect the discontnue date when it has expired.  *******
        if (Lt(local.InitialisedToZeros.Date,
          export.Group.Item.ManualDistributionAudit.DiscontinueDt))
        {
          if (Lt(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
            local.Current.Date))
          {
            var field =
              GetField(export.Group.Item.ManualDistributionAudit,
              "discontinueDt");

            field.Color = "cyan";
            field.Protected = true;
          }
        }
      }
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // <<< Set the NEXTTRAN info for the destination procedure .... RBM 03/17/
      // 97 >>>
      export.Hidden.CsePersonNumberObligor = export.CsePerson.Number;

      // *** Added next tran cse_person nunber. GSharp 12/15/98. ***
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      export.Hidden.ObligationId = export.Obligation.SystemGeneratedIdentifier;
      export.Hidden.LegalActionIdentifier = export.LegalAction.Identifier;
      UseScCabNextTranPut();

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
      export.CsePerson.Number = export.Hidden.CsePersonNumberObligor ?? Spaces
        (10);
      export.LegalAction.Identifier =
        export.Hidden.LegalActionIdentifier.GetValueOrDefault();

      // *** Added next tran legal_action standard_number. GSharp 12/15/98. ***
      export.LegalAction.StandardNumber =
        export.Hidden.StandardCrtOrdNumber ?? "";
      export.Obligation.SystemGeneratedIdentifier =
        export.Hidden.ObligationId.GetValueOrDefault();
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **** end   group C ****
    UseFnHardcodedDebtDistribution();

    switch(TrimEnd(global.Command))
    {
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "ADD":
        local.TotalRecsSelectedFProc.Count = 0;

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
              continue;
            case 'S':
              ++local.TotalRecsSelectedFProc.Count;

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              goto AfterCycle1;
          }

          if (!Lt(local.InitialisedToZeros.Date,
            export.Group.Item.ManualDistributionAudit.EffectiveDt))
          {
            export.Group.Update.ManualDistributionAudit.EffectiveDt =
              local.Current.Date;
          }

          if (!Lt(local.InitialisedToZeros.Date,
            export.Group.Item.ManualDistributionAudit.DiscontinueDt))
          {
            export.Group.Update.ManualDistributionAudit.DiscontinueDt =
              local.MaxDate.Date;
          }
          else if (Lt(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
            local.Current.Date))
          {
            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            var field =
              GetField(export.Group.Item.ManualDistributionAudit,
              "discontinueDt");

            field.Error = true;

            break;
          }

          if (!Lt(export.Group.Item.ManualDistributionAudit.EffectiveDt,
            export.Group.Item.ManualDistributionAudit.DiscontinueDt))
          {
            ExitState = "FN0000_DISC_DT_GREATER_THAN_EFF";

            var field =
              GetField(export.Group.Item.ManualDistributionAudit,
              "discontinueDt");

            field.Error = true;

            break;
          }

          if (Lt(export.Group.Item.ManualDistributionAudit.EffectiveDt,
            local.Current.Date))
          {
            var field =
              GetField(export.Group.Item.ManualDistributionAudit, "effectiveDt");
              

            field.Error = true;

            ExitState = "FN0000_EFF_DT_LT_CURR_DT";

            break;
          }

          if (IsEmpty(export.Group.Item.ManualDistributionAudit.Instructions))
          {
            ExitState = "FN0000_MAN_DIST_INST_REQD";

            var field =
              GetField(export.Group.Item.ManualDistributionAudit, "instructions");
              

            field.Error = true;

            break;
          }

          export.Group.Update.ManualDistributionAudit.CreatedBy = global.UserId;
          UseFnCreateManualDistAudit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          export.Group.Update.Hidden.DiscontinueDt =
            export.Group.Item.ManualDistributionAudit.DiscontinueDt;
        }

AfterCycle1:

        export.Group.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        if (local.TotalRecsSelectedFProc.Count == 0)
        {
          ExitState = "FN0000_MUST_SEL_REC_FOR_PROC";

          export.Group.Index = 0;
          export.Group.CheckSize();

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          break;
        }

        // --- If the processing is successful, clear the select chars.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            export.Group.Update.Common.SelectChar = "";
          }
        }

        export.Group.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        local.TotalRecsSelectedFProc.Count = 0;

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
              continue;
            case 'S':
              ++local.TotalRecsSelectedFProc.Count;

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              goto AfterCycle2;
          }

          if (!Lt(local.InitialisedToZeros.Date,
            export.Group.Item.ManualDistributionAudit.EffectiveDt))
          {
            export.Group.Update.ManualDistributionAudit.EffectiveDt =
              local.Current.Date;
          }

          if (!Lt(local.InitialisedToZeros.Date,
            export.Group.Item.ManualDistributionAudit.DiscontinueDt))
          {
            export.Group.Update.ManualDistributionAudit.DiscontinueDt =
              local.MaxDate.Date;
          }

          if (!Lt(export.Group.Item.ManualDistributionAudit.EffectiveDt,
            export.Group.Item.ManualDistributionAudit.DiscontinueDt))
          {
            ExitState = "FN0000_DISC_DT_GREATER_THAN_EFF";

            var field =
              GetField(export.Group.Item.ManualDistributionAudit,
              "discontinueDt");

            field.Error = true;

            break;
          }

          if (Equal(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
            export.Group.Item.Hidden.DiscontinueDt))
          {
          }
          else if (Lt(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
            local.Current.Date))
          {
            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            var field =
              GetField(export.Group.Item.ManualDistributionAudit,
              "discontinueDt");

            field.Error = true;

            break;
          }

          if (IsEmpty(export.Group.Item.ManualDistributionAudit.Instructions))
          {
            ExitState = "FN0000_MAN_DIST_INST_REQD";

            var field =
              GetField(export.Group.Item.ManualDistributionAudit, "instructions");
              

            field.Error = true;

            break;
          }

          UseFnUpdateManualDistAudit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          export.Group.Update.Hidden.DiscontinueDt =
            export.Group.Item.ManualDistributionAudit.DiscontinueDt;
        }

AfterCycle2:

        export.Group.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        if (local.TotalRecsSelectedFProc.Count == 0)
        {
          ExitState = "FN0000_MUST_SEL_REC_FOR_PROC";

          export.Group.Index = 0;
          export.Group.CheckSize();

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          break;
        }

        // --- If the processing is successful, clear the select chars.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            export.Group.Update.Common.SelectChar = "";
          }
        }

        export.Group.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DELETE":
        local.TotalRecsSelectedFProc.Count = 0;

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
              continue;
            case 'S':
              ++local.TotalRecsSelectedFProc.Count;

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              goto AfterCycle3;
          }

          // ***** CQ#9804 Changes Begins Here *****
          if (Equal(export.Group.Item.ManualDistributionAudit.EffectiveDt,
            local.InitialisedToZeros.Date))
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_COMMAND";

            break;
          }

          // ***** CQ#9804 Changes Ends   Here *****
          if (Lt(export.Group.Item.ManualDistributionAudit.EffectiveDt,
            local.Current.Date))
          {
            if (Equal(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
              local.InitialisedToZeros.Date))
            {
              // ** CQ#9804 commented the below ERROR display
              ExitState = "FN0000_CANT_DEL_ACT_MAN_DIST";
            }
            else if (Lt(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
              local.Current.Date))
            {
              // ** CQ#9804 commented the below ERROR display
              ExitState = "FN0000_CANT_DEL_HIST_MAN_DIST";
            }
            else
            {
              // ** CQ#9804 commented the below ERROR display
              ExitState = "FN0000_CANT_DEL_ACT_MAN_DIST";
            }

            // ***** CQ#9804 Changes Begins Here *****
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;
            }

            // ***** CQ#9804 Changes Ends   Here *****
            break;
          }

          // ***** CQ#9804 Changes Begins Here *****
          if (Equal(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
            local.InitialisedToZeros.Date))
          {
            export.Group.Update.ManualDistributionAudit.DiscontinueDt =
              local.MaxDate.Date;
          }

          // ***** CQ#9804 Changes Ends   Here *****
          UseFnDeleteManualDistribAudit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

AfterCycle3:

        export.Group.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        if (local.TotalRecsSelectedFProc.Count == 0)
        {
          ExitState = "FN0000_MUST_SEL_REC_FOR_PROC";

          export.Group.Index = 0;
          export.Group.CheckSize();

          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Protected = false;
          field.Focused = true;

          break;
        }

        // --- If the processing is successful, clear the select chars.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Group.Item.Common.SelectChar))
          {
            export.Group.Update.Common.SelectChar = "";

            // --- Clear the deleted record's fields.
            export.Group.Update.ManualDistributionAudit.CreatedBy = "";
            export.Group.Update.ManualDistributionAudit.Instructions =
              Spaces(ManualDistributionAudit.Instructions_MaxLength);
            export.Group.Update.ManualDistributionAudit.EffectiveDt =
              local.InitialisedToZeros.Date;
            export.Group.Update.ManualDistributionAudit.DiscontinueDt =
              local.InitialisedToZeros.Date;
          }
        }

        export.Group.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "DISPLAY":
        // *** Display. Note added by G Sharp ***
        if (IsEmpty(export.ShowHistory.Flag))
        {
          export.ShowHistory.Flag = "N";
        }

        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (export.Obligation.SystemGeneratedIdentifier == 0 || export
          .ObligationType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "FN0000_MUST_SEL_OBLG";

          break;
        }

        local.OmitCrdInd.Flag = "N";
        UseFnComputeSummaryTotals();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        UseFnCabSetAccrualOrDueAmount();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // *** Get frequency info.  ***
        if (ReadObligationPaymentSchedule())
        {
          UseFnSetFrequencyTextField();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        // =================================================
        // 5/19/99 - bud adams  -  When flowing from OPAY,
        //   order_type_code was not making the trip.
        // =================================================
        if (IsEmpty(export.Obligation.OrderTypeCode))
        {
          if (ReadObligation())
          {
            export.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
          }
          else
          {
            ExitState = "FN0000_OBLIGATION_NF";

            break;
          }
        }

        UseFnGetEachManualDistAudit();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // --- Reset the max dates back to zeros
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.Group.Item.ManualDistributionAudit, "effectiveDt");
              

            field.Color = "cyan";
            field.Protected = true;

            if (Equal(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
              local.MaxDate.Date))
            {
              export.Group.Update.ManualDistributionAudit.DiscontinueDt =
                local.InitialisedToZeros.Date;
            }
            else if (Lt(local.InitialisedToZeros.Date,
              export.Group.Item.ManualDistributionAudit.DiscontinueDt))
            {
              if (Lt(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
                local.Current.Date))
              {
                var field1 =
                  GetField(export.Group.Item.ManualDistributionAudit,
                  "discontinueDt");

                field1.Color = "cyan";
                field1.Protected = true;
              }
            }
          }

          export.Group.CheckIndex();

          if (export.Group.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else if (export.Group.IsEmpty)
          {
            ExitState = "FN0000_MANUAL_DIST_INSTRN_NF";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // --- Reset the max dates back to zeros
    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      if (Equal(export.Group.Item.ManualDistributionAudit.DiscontinueDt,
        local.MaxDate.Date))
      {
        export.Group.Update.ManualDistributionAudit.DiscontinueDt =
          local.InitialisedToZeros.Date;
      }
    }

    export.Group.CheckIndex();

    if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
    {
      var field = GetField(export.ScreenOwedAmounts, "errorInformationLine");

      field.Color = "red";
      field.Intensity = Intensity.Normal;
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = true;
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1ToGroup(FnGetEachManualDistAudit.Export.
    ExportGroup source, Export.GroupGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move fit weakly.");
    target.Common.SelectChar = source.Common.SelectChar;
    target.ManualDistributionAudit.Assign(source.ManualDistributionAudit);
  }

  private static void MoveFrequencyWorkSet(FrequencyWorkSet source,
    FrequencyWorkSet target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.FrequencyDescription = source.FrequencyDescription;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveManualDistributionAudit(
    ManualDistributionAudit source, ManualDistributionAudit target)
  {
    target.EffectiveDt = source.EffectiveDt;
    target.DiscontinueDt = source.DiscontinueDt;
    target.Instructions = source.Instructions;
    target.CreatedBy = source.CreatedBy;
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

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnCabSetAccrualOrDueAmount()
  {
    var useImport = new FnCabSetAccrualOrDueAmount.Import();
    var useExport = new FnCabSetAccrualOrDueAmount.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    MoveObligationType(export.ObligationType, useImport.ObligationType);

    Call(FnCabSetAccrualOrDueAmount.Execute, useImport, useExport);

    export.AccrualOrDue.Date = useExport.StartDte.Date;
    export.TotalAmountDue.TotalCurrency = useExport.Common.TotalCurrency;
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.OmitCrdInd.Flag = local.OmitCrdInd.Flag;
    useImport.Obligor.Number = export.CsePerson.Number;
    useImport.Filter.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnCreateManualDistAudit()
  {
    var useImport = new FnCreateManualDistAudit.Import();
    var useExport = new FnCreateManualDistAudit.Export();

    MoveManualDistributionAudit(export.Group.Item.ManualDistributionAudit,
      useImport.ManualDistributionAudit);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.Current.Timestamp = local.Current.Timestamp;

    Call(FnCreateManualDistAudit.Execute, useImport, useExport);
  }

  private void UseFnDeleteManualDistribAudit()
  {
    var useImport = new FnDeleteManualDistribAudit.Import();
    var useExport = new FnDeleteManualDistribAudit.Export();

    MoveManualDistributionAudit(export.Group.Item.ManualDistributionAudit,
      useImport.ManualDistributionAudit);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnDeleteManualDistribAudit.Execute, useImport, useExport);
  }

  private void UseFnGetEachManualDistAudit()
  {
    var useImport = new FnGetEachManualDistAudit.Import();
    var useExport = new FnGetEachManualDistAudit.Export();

    useImport.Starting.EffectiveDt = import.Starting.EffectiveDt;
    useImport.ShowHistory.Flag = export.ShowHistory.Flag;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.Current.Date = local.Current.Date;

    Call(FnGetEachManualDistAudit.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Group, MoveExport1ToGroup);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
  }

  private void UseFnSetFrequencyTextField()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    useImport.ObligationPaymentSchedule.Assign(
      entities.ObligationPaymentSchedule);

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    MoveFrequencyWorkSet(useExport.FrequencyWorkSet, export.FrequencyWorkSet);
  }

  private void UseFnUpdateManualDistAudit()
  {
    var useImport = new FnUpdateManualDistAudit.Import();
    var useExport = new FnUpdateManualDistAudit.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    MoveObligationType(export.ObligationType, useImport.ObligationType);
    useImport.ManualDistributionAudit.Assign(
      export.Group.Item.ManualDistributionAudit);
    useImport.Current.Timestamp = local.Current.Timestamp;

    Call(FnUpdateManualDistAudit.Execute, useImport, useExport);
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

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    MoveLegalAction(import.LegalAction, useImport.LegalAction);
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, export.CsePerson);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          export.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetString(command, "obgCspNumber", export.CsePerson.Number);
        db.SetInt32(
          command, "obgGeneratedId",
          export.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", export.ObligationType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 5);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 6);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 7);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 8);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 9);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 10);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of ManualDistributionAudit.
      /// </summary>
      [JsonPropertyName("manualDistributionAudit")]
      public ManualDistributionAudit ManualDistributionAudit
      {
        get => manualDistributionAudit ??= new();
        set => manualDistributionAudit = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ManualDistributionAudit Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private ManualDistributionAudit manualDistributionAudit;
      private ManualDistributionAudit hidden;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of Day2.
    /// </summary>
    [JsonPropertyName("day2")]
    public Common Day2
    {
      get => day2 ??= new();
      set => day2 = value;
    }

    /// <summary>
    /// A value of Day1.
    /// </summary>
    [JsonPropertyName("day1")]
    public Common Day1
    {
      get => day1 ??= new();
      set => day1 = value;
    }

    /// <summary>
    /// A value of ZdelImportObligationAmt.
    /// </summary>
    [JsonPropertyName("zdelImportObligationAmt")]
    public Common ZdelImportObligationAmt
    {
      get => zdelImportObligationAmt ??= new();
      set => zdelImportObligationAmt = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ManualDistributionAudit Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of AccrualOrDue.
    /// </summary>
    [JsonPropertyName("accrualOrDue")]
    public DateWorkArea AccrualOrDue
    {
      get => accrualOrDue ??= new();
      set => accrualOrDue = value;
    }

    /// <summary>
    /// A value of TotalAmountDue.
    /// </summary>
    [JsonPropertyName("totalAmountDue")]
    public Common TotalAmountDue
    {
      get => totalAmountDue ??= new();
      set => totalAmountDue = value;
    }

    private Common showHistory;
    private Common day2;
    private Common day1;
    private Common zdelImportObligationAmt;
    private Common ae;
    private LegalActionDetail legalActionDetail;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private Common common;
    private ScreenOwedAmounts screenOwedAmounts;
    private CsePersonAccount csePersonAccount;
    private Array<GroupGroup> group;
    private CsePerson csePerson;
    private Obligation obligation;
    private NextTranInfo hidden;
    private ManualDistributionAudit starting;
    private FrequencyWorkSet frequencyWorkSet;
    private DateWorkArea accrualOrDue;
    private Common totalAmountDue;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of ManualDistributionAudit.
      /// </summary>
      [JsonPropertyName("manualDistributionAudit")]
      public ManualDistributionAudit ManualDistributionAudit
      {
        get => manualDistributionAudit ??= new();
        set => manualDistributionAudit = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public ManualDistributionAudit Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common common;
      private ManualDistributionAudit manualDistributionAudit;
      private ManualDistributionAudit hidden;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Day2.
    /// </summary>
    [JsonPropertyName("day2")]
    public Common Day2
    {
      get => day2 ??= new();
      set => day2 = value;
    }

    /// <summary>
    /// A value of Day1.
    /// </summary>
    [JsonPropertyName("day1")]
    public Common Day1
    {
      get => day1 ??= new();
      set => day1 = value;
    }

    /// <summary>
    /// A value of ZdelExportObligationAmt.
    /// </summary>
    [JsonPropertyName("zdelExportObligationAmt")]
    public Common ZdelExportObligationAmt
    {
      get => zdelExportObligationAmt ??= new();
      set => zdelExportObligationAmt = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ManualDistributionAudit Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of AccrualOrDue.
    /// </summary>
    [JsonPropertyName("accrualOrDue")]
    public DateWorkArea AccrualOrDue
    {
      get => accrualOrDue ??= new();
      set => accrualOrDue = value;
    }

    /// <summary>
    /// A value of TotalAmountDue.
    /// </summary>
    [JsonPropertyName("totalAmountDue")]
    public Common TotalAmountDue
    {
      get => totalAmountDue ??= new();
      set => totalAmountDue = value;
    }

    private Common showHistory;
    private Common ae;
    private LegalActionDetail legalActionDetail;
    private CsePersonAccount csePersonAccount;
    private Standard standard;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Obligation obligation;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private Common day2;
    private Common day1;
    private Common zdelExportObligationAmt;
    private ScreenOwedAmounts screenOwedAmounts;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private ManualDistributionAudit starting;
    private FrequencyWorkSet frequencyWorkSet;
    private DateWorkArea accrualOrDue;
    private Common totalAmountDue;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OmitCrdInd.
    /// </summary>
    [JsonPropertyName("omitCrdInd")]
    public Common OmitCrdInd
    {
      get => omitCrdInd ??= new();
      set => omitCrdInd = value;
    }

    /// <summary>
    /// A value of TotalRecsSelectedFProc.
    /// </summary>
    [JsonPropertyName("totalRecsSelectedFProc")]
    public Common TotalRecsSelectedFProc
    {
      get => totalRecsSelectedFProc ??= new();
      set => totalRecsSelectedFProc = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
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
    /// A value of ExpireEffectiveDateAttributes.
    /// </summary>
    [JsonPropertyName("expireEffectiveDateAttributes")]
    public ExpireEffectiveDateAttributes ExpireEffectiveDateAttributes
    {
      get => expireEffectiveDateAttributes ??= new();
      set => expireEffectiveDateAttributes = value;
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

    private Common omitCrdInd;
    private Common totalRecsSelectedFProc;
    private DateWorkArea maxDate;
    private DateWorkArea initialisedToZeros;
    private DateWorkArea current;
    private ExpireEffectiveDateAttributes expireEffectiveDateAttributes;
    private CsePersonAccount hardcodeObligor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    private CsePersonAccount csePersonAccount;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
  }
#endregion
}
