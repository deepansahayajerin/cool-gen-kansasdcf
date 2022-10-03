// Program: FN_OREL_LST_POTENTIAL_RECOV_OBLG, ID: 372046318, model: 746.
// Short name: SWEORELP
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
/// <para>
/// A program: FN_OREL_LST_POTENTIAL_RECOV_OBLG.
/// </para>
/// <para>
/// Resp: Finance	
/// The purpose for this PRODEDURE is to list all Potential recovery Obligations
/// that are available by Obligee.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOrelLstPotentialRecovOblg: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OREL_LST_POTENTIAL_RECOV_OBLG program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOrelLstPotentialRecovOblg(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOrelLstPotentialRecovOblg.
  /// </summary>
  public FnOrelLstPotentialRecovOblg(IContext context, Import import,
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
    // Date      Developer Name  Request#    Description
    // 02/16/96  Rick Delgado                New Development
    // 12/17/96  R. Marchman                 Add new security/next tran
    // 09/30/97  A Samuels       IDCR 347
    // 10/15/98  G Sharp         Phase2      Changes
    // 8/21/99    B Adams    PR# 230     (note below)
    // 12/20/1999  B Adams   PR# 82602 (note below)
    // 12/21/1999  K Doshi   PR#82734 Increase cardinality of group view  to 
    // 120.
    // 01/11/2000  K. Doshi   PR# 84315. If Person # has been entered do NOT 
    // apply status filter. i.e. Set search status flag to SPACES.
    // 01/12/2000  K. Doshi   PR# 84307. When NEXTing to OREL and Person # was 
    // not entered, do not perform a display.
    // 08/05/2002  K. Doshi   PR# 149011. Fix screen help Id.
    // ----------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // 12/22/2000              Vithal Madhira                WR# 000262
    //  New business rules implemented. User can enter reason for denial of 
    // services.
    // 1. Records in RCVPOT status will display a one line Note that will be 
    // enterable prior to denial of the potential recovery.
    // 2.	When the PF6 Deny function is used, the data entered on the Note line 
    // will always display with the potential recovery that was denied and will
    // be protected.
    // 3.	The Note will not display or be retained when a potential recovery is 
    // in RCVCRE status.
    // 4.	The user must have the ability to filter the OREL screen by a specific
    // recovery status; (P)otential, (C)reated or (D)enied.
    // 5.	The user must not be able to create a recovery obligation from an 
    // already denied record.
    // --------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // ************************************************
    // *Move Imports to Exports.                      *
    // ************************************************
    if (!IsEmpty(import.FromFlow.Number))
    {
      MoveCsePersonsWorkSet(import.FromFlow,
        export.SearchObligeeCsePersonsWorkSet);
      export.SearchObligeeCsePerson.Number = import.FromFlow.Number;

      var field = GetField(export.SearchObligeeCsePerson, "number");

      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;
    }
    else
    {
      if (!IsEmpty(import.SearchObligeeCsePerson.Number))
      {
        local.CsePersonsWorkSet.Number = import.SearchObligeeCsePerson.Number;
        UseSiReadCsePerson();
        MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
          export.SearchObligeeCsePersonsWorkSet);
        export.SearchObligeeCsePerson.Number = local.CsePersonsWorkSet.Number;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        for(export.Group2.Index = 0; export.Group2.Index < export.Group2.Count; ++
          export.Group2.Index)
        {
          if (!export.Group2.CheckSize())
          {
            break;
          }

          export.Group2.Update.Gv2Common.SelectChar = "";

          var field1 = GetField(export.Group2.Item.Gv2Common, "selectChar");

          field1.Intensity = Intensity.Dark;
          field1.Protected = true;

          export.Group2.Update.Gv2NewWorkSet.Text76 = "";

          var field2 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

          field2.Intensity = Intensity.Dark;
          field2.Protected = true;
        }

        export.Group2.CheckIndex();

        return;
      }
    }

    export.SearchTo.ProcessDate = import.SearchTo.ProcessDate;
    export.SearchFrom.ProcessDate = import.SearchFrom.ProcessDate;
    export.SearchStatusPaymentStatus.Code =
      import.SearchStatusPaymentStatus.Code;
    MoveCommon(import.SearchStatusCommon, export.SearchStatusCommon);
    export.HiddenFirstTime.Flag = import.HiddenFirstTime.Flag;

    // *** The 1st time thru, if NO P, C, or D. then default to P. After that 
    // can chage to P, C, D, or space. Space will display all records. G Sharp 1
    // -20-99 ***
    if (Equal(global.Command, "RETLINK"))
    {
      var field = GetField(export.SearchStatusPaymentStatus, "code");

      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;
    }

    export.Search.CreatedBy = import.Search.CreatedBy;
    export.CsePerson.PromptField = import.CsePerson.PromptField;
    export.Status.PromptField = import.Status.PromptField;
    export.History.Flag = import.History.Flag;

    for(import.Hidden1.Index = 0; import.Hidden1.Index < import.Hidden1.Count; ++
      import.Hidden1.Index)
    {
      if (!import.Hidden1.CheckSize())
      {
        break;
      }

      export.Hidden1.Index = import.Hidden1.Index;
      export.Hidden1.CheckSize();

      MoveCsePersonsWorkSet(import.Hidden1.Item.HidGv1CsePersonsWorkSet,
        export.Hidden1.Update.HidGv1CsePersonsWorkSet);
      export.Hidden1.Update.HidGv1PaymentRequest.Assign(
        import.Hidden1.Item.HidGv1PaymentRequest);
      export.Hidden1.Update.HidGv1PaymentStatus.Code =
        import.Hidden1.Item.HidGv1PaymentStatus.Code;
      MovePaymentStatusHistory(import.Hidden1.Item.HidGv1PaymentStatusHistory,
        export.Hidden1.Update.HidGv1PaymentStatusHistory);
      export.Hidden1.Update.HidGv1ReasonText.Text76 =
        import.Hidden1.Item.HidGv1ReasonText.Text76;
    }

    import.Hidden1.CheckIndex();

    for(import.Group2.Index = 0; import.Group2.Index < import.Group2.Count; ++
      import.Group2.Index)
    {
      if (!import.Group2.CheckSize())
      {
        break;
      }

      export.Group2.Index = import.Group2.Index;
      export.Group2.CheckSize();

      export.Group2.Update.Gv2Common.SelectChar =
        import.Group2.Item.Gv2Common.SelectChar;
      export.Group2.Update.Gv2NewWorkSet.Text76 =
        import.Group2.Item.Gv2NewWorkSet.Text76;
      export.Group2.Update.HiddenGv2PaymentRequest.Assign(
        import.Group2.Item.HiddenGv2PaymentRequest);
      export.Group2.Update.HiddenGv2PaymentStatus.Code =
        import.Group2.Item.HiddenGv2PaymentStatus.Code;
      MoveCsePersonsWorkSet(import.Group2.Item.HiddenGv2CsePersonsWorkSet,
        export.Group2.Update.HiddenGv2CsePersonsWorkSet);
    }

    import.Group2.CheckIndex();

    for(import.Group3.Index = 0; import.Group3.Index < import.Group3.Count; ++
      import.Group3.Index)
    {
      if (!import.Group3.CheckSize())
      {
        break;
      }

      export.Group3.Index = import.Group3.Index;
      export.Group3.CheckSize();

      export.Group3.Update.HidPage.StartIndex =
        import.Group3.Item.HidPage.StartIndex;
    }

    import.Group3.CheckIndex();
    export.HiddenPresent.Assign(import.HiddenPresent);
    export.HiddenPrevious.Assign(import.HiddenPrevious);
    export.HiddenGv1Subscript.Subscript = import.HiddenGv1Subscript.Subscript;
    export.HidDisplayReasonText.Flag = import.HidDisplayReasonText.Flag;

    if (!Equal(global.Command, "DISPLAY"))
    {
      for(export.Group2.Index = 0; export.Group2.Index < export.Group2.Count; ++
        export.Group2.Index)
      {
        if (!export.Group2.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Group2.Item.Gv2Common.SelectChar))
        {
          if (AsChar(export.Group2.Item.Gv2Common.SelectChar) == 'S')
          {
            ++local.Common.Count;
            local.Check.Code = export.Group2.Item.HiddenGv2PaymentStatus.Code;
            export.ForFlowPaymentRequest.Assign(
              export.Group2.Item.HiddenGv2PaymentRequest);
            MoveCsePersonsWorkSet(export.Group2.Item.HiddenGv2CsePersonsWorkSet,
              export.ForFlowCsePersonsWorkSet);
            export.SeltectedFlowOn.Number =
              export.ForFlowCsePersonsWorkSet.Number;
            export.FlowObligationTransaction.Amount =
              -export.Group2.Item.HiddenGv2PaymentRequest.Amount;
            export.FlowDebtDetail.DueDt =
              Date(export.Group2.Item.HiddenGv2PaymentRequest.CreatedTimestamp);
              
          }
          else
          {
            var field = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            global.Command = "BYPASS";
          }
        }
      }

      export.Group2.CheckIndex();
    }

    // *****
    // G Sharp: Get valid hardcode values for local entity attributes.
    // *****
    if (Equal(global.Command, "RETLINK") || Equal(global.Command, "RETNAME"))
    {
      global.Command = "DISPLAY";
    }

    // ************************************************
    // *If History is not being Requested Default to N*
    // ************************************************
    if (AsChar(import.History.Flag) != 'Y')
    {
      export.History.Flag = "N";
    }

    // *** If Status is not being requested default to P on 1st time. (if space 
    // deault to P). After that space is ok. If space will display ALL Status. G
    // Sharp Change2 01-20-99. ***
    if (AsChar(import.HiddenFirstTime.Flag) == 'N')
    {
      // ** OK ***
    }
    else
    {
      switch(AsChar(export.SearchStatusCommon.SelectChar))
      {
        case 'P':
          break;
        case 'C':
          break;
        case 'D':
          break;
        default:
          export.SearchStatusCommon.SelectChar = "P";

          break;
      }
    }

    export.HiddenFirstTime.Flag = "N";

    // ************************************************
    // *Find out if Next Tran output is required      *
    // ************************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;

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

      global.Command = "BYPASS";
    }

    // ************************************************
    // *Enter Code here for Outgoing Next Tran        *
    // ************************************************
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      // ----------------------------------------------------------------
      // 1/12/2000 K. Doshi PR# 84307. When NEXTing to OREL and Person # was not
      // entered, do not perform a display.
      // ----------------------------------------------------------------
      if (!IsEmpty(export.Hidden.CsePersonNumberObligee))
      {
        global.Command = "DISPLAY";
        export.SearchObligeeCsePerson.Number =
          export.Hidden.CsePersonNumberObligee ?? Spaces(10);
        local.CsePersonsWorkSet.Number = export.SearchObligeeCsePerson.Number;
        UseSiReadCsePerson();
        MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
          export.SearchObligeeCsePersonsWorkSet);
      }
      else
      {
        return;
      }
    }

    // ************************************************
    // *Validate action level security.               *
    // ************************************************
    if (Equal(global.Command, "PACC") || Equal(global.Command, "CRRL") || Equal
      (global.Command, "CRAL") || Equal(global.Command, "WDTL") || Equal
      (global.Command, "OREC") || Equal(global.Command, "NEXT1"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "XXFMMENU"))
      {
        global.Command = "DISPLAY";
      }
    }
    else
    {
      global.Command = "BYPASS";
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ***************************************************************
        // Display logic located at bottom of PrAD.
        // ***************************************************************
        break;
      case "LIST":
        if (!IsEmpty(export.CsePerson.PromptField))
        {
          switch(AsChar(export.CsePerson.PromptField))
          {
            case 'S':
              ++local.PromptsSelected.Count;
              ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

              break;
            case '+':
              break;
            default:
              var field = GetField(export.CsePerson, "promptField");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
              global.Command = "BYPASS";

              break;
          }
        }

        if (!IsEmpty(export.Status.PromptField))
        {
          switch(AsChar(export.Status.PromptField))
          {
            case 'S':
              ++local.PromptsSelected.Count;
              ExitState = "ECO_LNK_TO_LST_PAYMENT_STATUSES";

              break;
            case '+':
              break;
            default:
              var field = GetField(export.Status, "promptField");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
              global.Command = "BYPASS";

              break;
          }
        }

        switch(local.PromptsSelected.Count)
        {
          case 0:
            ExitState = "ZD_ACO_NE00_MUST_SELECT_4_PROMPT";
            global.Command = "BYPASS";

            break;
          case 1:
            if (AsChar(export.CsePerson.PromptField) == 'S')
            {
              export.CsePerson.PromptField = "";
            }

            if (AsChar(export.Status.PromptField) == 'S')
            {
              export.Status.PromptField = "";
            }

            return;
          default:
            if (AsChar(export.CsePerson.PromptField) == 'S')
            {
              var field = GetField(export.CsePerson, "promptField");

              field.Error = true;
            }

            if (AsChar(export.Status.PromptField) == 'S')
            {
              var field = GetField(export.Status, "promptField");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            global.Command = "BYPASS";

            break;
        }

        break;
      case "NEXT1":
        // ----------------------------------------------------------------------
        // For some reason command 'NEXT'  stopped working after changing 
        // scrolling from implicit to explicit. So I am using 'NEXT1'.
        //                                             
        // Per WR# 262 ( Vithal Madhira)
        // ------------------------------------------------------------------------
        if (export.HiddenPresent.PageNumber == 0)
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        // ----------------------------------------------------------------------
        // The following code is added to implement  explicit scrolling.
        //                                             
        // Per WR# 262 ( Vithal Madhira)
        // ------------------------------------------------------------------------
        if (export.HiddenGv1Subscript.Subscript < export.Hidden1.Count)
        {
          export.HiddenPrevious.PageNumber = export.HiddenPresent.PageNumber;
          export.HiddenPrevious.StartIndex = export.HiddenPresent.StartIndex;
          export.HiddenPrevious.EndIndex = export.HiddenPresent.EndIndex;

          for(export.Group2.Index = 0; export.Group2.Index < export
            .Group2.Count; ++export.Group2.Index)
          {
            if (!export.Group2.CheckSize())
            {
              break;
            }

            export.Group2.Update.Gv2Common.SelectChar = "";

            var field1 = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field1.Intensity = Intensity.Dark;
            field1.Protected = true;

            export.Group2.Update.Gv2NewWorkSet.Text76 = "";

            var field2 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

            field2.Intensity = Intensity.Dark;
            field2.Protected = true;

            export.Group2.Update.HiddenGv2PaymentRequest.Assign(
              local.BlankPaymentRequest);
            export.Group2.Update.HiddenGv2PaymentStatus.Code =
              local.BlankPaymentStatus.Code;
            MoveCsePersonsWorkSet(local.BlankCsePersonsWorkSet,
              export.Group2.Update.HiddenGv2CsePersonsWorkSet);
          }

          export.Group2.CheckIndex();
          export.Group2.Index = -1;
          export.Group2.Count = 0;
          local.GroupViewIndex.Count = export.HiddenGv1Subscript.Subscript + 1;
          export.Hidden1.Index = local.GroupViewIndex.Count - 1;

          for(var limit = export.Hidden1.Count; export.Hidden1.Index < limit; ++
            export.Hidden1.Index)
          {
            if (!export.Hidden1.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.HidDisplayReasonText.Flag))
            {
              ++export.Group2.Index;
              export.Group2.CheckSize();

              if (AsChar(export.HidDisplayReasonText.Flag) == 'P')
              {
                var field3 =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field3.Intensity = Intensity.Dark;
                field3.Protected = true;

                var field4 =
                  GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

                field4.Color = "green";
                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = false;

                export.Hidden1.Index = export.HiddenGv1Subscript.Subscript - 1;
                export.Hidden1.CheckSize();

                export.Group2.Update.Gv2NewWorkSet.Text76 =
                  export.Hidden1.Item.HidGv1ReasonText.Text76;
                export.HidDisplayReasonText.Flag = "";

                export.Hidden1.Index = local.GroupViewIndex.Count - 1;
                export.Hidden1.CheckSize();
              }
              else if (AsChar(export.HidDisplayReasonText.Flag) == 'D')
              {
                var field3 =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field3.Intensity = Intensity.Dark;
                field3.Protected = true;

                var field4 =
                  GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

                field4.Color = "cyan";
                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = true;

                export.Hidden1.Index = export.HiddenGv1Subscript.Subscript - 1;
                export.Hidden1.CheckSize();

                export.Group2.Update.Gv2NewWorkSet.Text76 =
                  export.Hidden1.Item.HidGv1ReasonText.Text76;
                export.HidDisplayReasonText.Flag = "";

                export.Hidden1.Index = local.GroupViewIndex.Count - 1;
                export.Hidden1.CheckSize();
              }
            }

            ++export.Group2.Index;
            export.Group2.CheckSize();

            local.RecoveryDate.TextDate =
              NumberToString(DateToInt(
                Date(export.Hidden1.Item.HidGv1PaymentRequest.CreatedTimestamp)),
              8, 8);
            local.RecoveryDate.TextDate =
              Substring(local.RecoveryDate.TextDate,
              DateWorkArea.TextDate_MaxLength, 5, 2) + Substring
              (local.RecoveryDate.TextDate, DateWorkArea.TextDate_MaxLength, 7,
              2) + Substring
              (local.RecoveryDate.TextDate, DateWorkArea.TextDate_MaxLength, 1,
              4);
            local.ProcessDate.TextDate =
              NumberToString(DateToInt(
                export.Hidden1.Item.HidGv1PaymentStatusHistory.EffectiveDate),
              8, 8);
            local.ProcessDate.TextDate =
              Substring(local.ProcessDate.TextDate,
              DateWorkArea.TextDate_MaxLength, 5, 2) + Substring
              (local.ProcessDate.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2)
              + Substring
              (local.ProcessDate.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);
              
            local.PmntRequestAmount.Text9 =
              NumberToString((long)export.Hidden1.Item.HidGv1PaymentRequest.
                Amount, 10, 6) + TrimEnd(".") + NumberToString
              ((long)(export.Hidden1.Item.HidGv1PaymentRequest.Amount * 100),
              14, 2);
            local.Local1Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 1, 1);
            local.Local2Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 2, 1);
            local.Local3Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 3, 1);
            local.Local4Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 4, 1);
            local.Local5Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 5, 1);

            if (AsChar(local.Local1Letter.Flag) == '0')
            {
              local.Local1Letter.Flag = "";
              local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + Substring
                (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength, 2, 8);
                

              if (AsChar(local.Local2Letter.Flag) == '0')
              {
                local.Local2Letter.Flag = "";
                local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                  .Local2Letter.Flag + Substring
                  (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength, 3,
                  7);

                if (AsChar(local.Local3Letter.Flag) == '0')
                {
                  local.Local3Letter.Flag = "";
                  local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                    .Local2Letter.Flag + local.Local3Letter.Flag + Substring
                    (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength,
                    4, 6);

                  if (AsChar(local.Local4Letter.Flag) == '0')
                  {
                    local.Local4Letter.Flag = "";
                    local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                      .Local2Letter.Flag + local.Local3Letter.Flag + local
                      .Local4Letter.Flag + Substring
                      (local.PmntRequestAmount.Text9,
                      NewWorkSet.Text9_MaxLength, 5, 5);

                    if (AsChar(local.Local5Letter.Flag) == '0')
                    {
                      local.Local5Letter.Flag = "";
                      local.PmntRequestAmount.Text9 =
                        local.Local1Letter.Flag + local.Local2Letter.Flag + local
                        .Local3Letter.Flag + local.Local4Letter.Flag + local
                        .Local5Letter.Flag + Substring
                        (local.PmntRequestAmount.Text9,
                        NewWorkSet.Text9_MaxLength, 6, 4);
                    }
                  }
                }
              }
            }

            export.Group2.Update.Gv2NewWorkSet.Text76 =
              (export.Hidden1.Item.HidGv1PaymentRequest.CsePersonNumber ?? "") +
              local.NewWorkSet.FillerText1 + Substring
              (export.Hidden1.Item.HidGv1CsePersonsWorkSet.FormattedName,
              CsePersonsWorkSet.FormattedName_MaxLength, 1, 15) + local
              .NewWorkSet.FillerText1 + local.RecoveryDate.TextDate + local
              .NewWorkSet.FillerText1 + local.PmntRequestAmount.Text9 + local
              .NewWorkSet.FillerText1 + export
              .Hidden1.Item.HidGv1PaymentStatus.Code + local
              .NewWorkSet.FillerText1 + local.ProcessDate.TextDate + local
              .NewWorkSet.FillerText1 + export
              .Hidden1.Item.HidGv1PaymentStatusHistory.CreatedBy;
            export.Group2.Update.HiddenGv2PaymentRequest.Assign(
              export.Hidden1.Item.HidGv1PaymentRequest);
            export.Group2.Update.HiddenGv2PaymentStatus.Code =
              export.Hidden1.Item.HidGv1PaymentStatus.Code;
            MoveCsePersonsWorkSet(export.Hidden1.Item.HidGv1CsePersonsWorkSet,
              export.Group2.Update.HiddenGv2CsePersonsWorkSet);

            var field1 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field2.Protected = false;

            if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code, "RCVPOT"))
            {
              local.PaymentStatusCode.Flag = "P";
              local.PayStatusHistoryReason.Text76 =
                export.Hidden1.Item.HidGv1ReasonText.Text76;
            }
            else if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code,
              "RCVDEN"))
            {
              local.PaymentStatusCode.Flag = "D";
              local.PayStatusHistoryReason.Text76 =
                export.Hidden1.Item.HidGv1ReasonText.Text76;
            }

            if (export.Group2.Index + 1 >= Export.Group2Group.Capacity)
            {
              // -----------------------------------------------------------------------
              // If the Reason text can not be displayed on the present screen, 
              // display it on next screen by checking the FLAG.
              // --------------------------------------------------------------------------
              if (!IsEmpty(local.PaymentStatusCode.Flag))
              {
                export.HidDisplayReasonText.Flag = local.PaymentStatusCode.Flag;
              }

              ++export.HiddenPresent.PageNumber;
              export.HiddenPresent.StartIndex =
                export.HiddenGv1Subscript.Subscript + 1;
              export.HiddenGv1Subscript.Subscript = export.Hidden1.Index + 1;
              export.HiddenPresent.EndIndex =
                export.HiddenGv1Subscript.Subscript;

              export.Group3.Index = export.HiddenPresent.PageNumber - 1;
              export.Group3.CheckSize();

              export.Group3.Update.HidPage.StartIndex =
                export.HiddenPresent.StartIndex;

              break;
            }

            if (AsChar(local.PaymentStatusCode.Flag) == 'P')
            {
              local.PaymentStatusCode.Flag = "";

              ++export.Group2.Index;
              export.Group2.CheckSize();

              var field3 = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field3.Intensity = Intensity.Dark;
              field3.Protected = true;

              var field4 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

              field4.Color = "green";
              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = false;

              export.Group2.Update.Gv2NewWorkSet.Text76 =
                local.PayStatusHistoryReason.Text76;
            }
            else if (AsChar(local.PaymentStatusCode.Flag) == 'D')
            {
              local.PaymentStatusCode.Flag = "";

              ++export.Group2.Index;
              export.Group2.CheckSize();

              export.Group2.Update.Gv2NewWorkSet.Text76 =
                local.PayStatusHistoryReason.Text76;

              var field3 = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field3.Intensity = Intensity.Dark;
              field3.Protected = true;

              var field4 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

              field4.Color = "cyan";
              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = true;
            }

            if (export.Group2.Index + 1 >= Export.Group2Group.Capacity)
            {
              ++export.HiddenPresent.PageNumber;
              export.HiddenPresent.StartIndex =
                export.HiddenGv1Subscript.Subscript + 1;
              export.HiddenGv1Subscript.Subscript = export.Hidden1.Index + 1;
              export.HiddenPresent.EndIndex =
                export.HiddenGv1Subscript.Subscript;

              export.Group3.Index = export.HiddenPresent.PageNumber - 1;
              export.Group3.CheckSize();

              export.Group3.Update.HidPage.StartIndex =
                export.HiddenPresent.StartIndex;
              ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

              goto Test;
            }
          }

          export.Hidden1.CheckIndex();

          if (export.Hidden1.Index + 1 >= export.Hidden1.Count && export
            .Group2.Index < Export.Group2Group.Capacity)
          {
            ++export.HiddenPresent.PageNumber;
            export.HiddenPresent.StartIndex =
              export.HiddenGv1Subscript.Subscript + 1;
            export.HiddenGv1Subscript.Subscript = export.Hidden1.Count;
            export.HiddenPresent.EndIndex = export.HiddenGv1Subscript.Subscript;

            export.Group3.Index = export.HiddenPresent.PageNumber - 1;
            export.Group3.CheckSize();

            export.Group3.Update.HidPage.StartIndex =
              export.HiddenPresent.StartIndex;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
          export.Group2.Index = 0;

          for(var limit = export.Group2.Count; export.Group2.Index < limit; ++
            export.Group2.Index)
          {
            if (!export.Group2.CheckSize())
            {
              break;
            }

            var field1 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field2.Protected = false;

            if (IsEmpty(export.Group2.Item.HiddenGv2PaymentStatus.Code))
            {
              export.Hidden1.Index = export.HiddenPresent.StartIndex - 2;
              export.Hidden1.CheckSize();

              if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code, "RCVDEN"))
              {
                var field3 =
                  GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

                field3.Color = "cyan";
                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = true;

                var field4 =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field4.Intensity = Intensity.Dark;
                field4.Protected = true;
              }
              else if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code,
                "RCVPOT"))
              {
                var field3 =
                  GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = false;

                var field4 =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field4.Intensity = Intensity.Dark;
                field4.Protected = true;
              }

              export.Hidden1.Index = export.Hidden1.Count - 1;
              export.Hidden1.CheckSize();
            }

            if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code, "RCVDEN"))
            {
              ++export.Group2.Index;
              export.Group2.CheckSize();

              var field3 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

              field3.Color = "cyan";
              field3.Highlighting = Highlighting.Underscore;
              field3.Protected = true;

              var field4 = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field4.Intensity = Intensity.Dark;
              field4.Protected = true;
            }

            if (export.Group2.Index + 1 >= export.Group2.Count)
            {
              break;
            }

            if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code, "RCVPOT"))
            {
              ++export.Group2.Index;
              export.Group2.CheckSize();

              var field3 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

              field3.Highlighting = Highlighting.Underscore;
              field3.Protected = false;

              var field4 = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field4.Intensity = Intensity.Dark;
              field4.Protected = true;
            }

            if (export.Group2.Index + 1 >= export.Group2.Count)
            {
              break;
            }
          }

          export.Group2.CheckIndex();
        }

        break;
      case "PREV":
        if (export.HiddenPresent.PageNumber == 0)
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        // ----------------------------------------------------------------------
        // The following code is added to implement explicit scrolling.
        //                                             
        // Per WR# 262 ( Vithal Madhira)
        // ------------------------------------------------------------------------
        if (export.HiddenPresent.PageNumber == 1)
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";
          local.CheckGv1.Subscript = export.Hidden1.Count;
          export.Group2.Index = 0;

          for(var limit = export.Group2.Count; export.Group2.Index < limit; ++
            export.Group2.Index)
          {
            if (!export.Group2.CheckSize())
            {
              break;
            }

            var field1 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field2.Protected = false;

            if (export.Group2.Index + 1 >= export.Group2.Count)
            {
              break;
            }

            if (IsEmpty(export.Group2.Item.HiddenGv2PaymentStatus.Code))
            {
              export.Hidden1.Index = export.HiddenPresent.StartIndex - 2;
              export.Hidden1.CheckSize();

              if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code, "RCVDEN"))
              {
                var field3 =
                  GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

                field3.Color = "cyan";
                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = true;

                var field4 =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field4.Intensity = Intensity.Dark;
                field4.Protected = true;
              }
              else if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code,
                "RCVPOT"))
              {
                var field3 =
                  GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = false;

                var field4 =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field4.Intensity = Intensity.Dark;
                field4.Protected = true;
              }

              export.Hidden1.Index = export.Hidden1.Count - 1;
              export.Hidden1.CheckSize();
            }

            if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code, "RCVDEN"))
            {
              ++export.Group2.Index;
              export.Group2.CheckSize();

              var field3 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

              field3.Color = "cyan";
              field3.Highlighting = Highlighting.Underscore;
              field3.Protected = true;

              var field4 = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field4.Intensity = Intensity.Dark;
              field4.Protected = true;
            }

            if (export.Group2.Index + 1 >= export.Group2.Count)
            {
              break;
            }

            if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code, "RCVPOT"))
            {
              ++export.Group2.Index;
              export.Group2.CheckSize();

              var field3 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

              field3.Highlighting = Highlighting.Underscore;
              field3.Protected = false;

              var field4 = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field4.Intensity = Intensity.Dark;
              field4.Protected = true;
            }

            if (export.Group2.Index + 1 >= export.Group2.Count)
            {
              break;
            }
          }

          export.Group2.CheckIndex();

          // ----------------------------------------------------------------------------------
          // For some reason the subscript of Export_Group_Hidden_1 is not set 
          // to last of subscript. So reset to LAST.
          // ----------------------------------------------------------------------------------
          export.Hidden1.Count = local.CheckGv1.Subscript;
        }
        else
        {
          export.Group2.Index = -1;
          export.Group2.Count = 0;

          // -------------------------------------------------------------------------
          // Find the start index for this page from  Export_Group_3.
          // --------------------------------------------------------------------------
          export.Group3.Index = export.HiddenPresent.PageNumber - 2;
          export.Group3.CheckSize();

          local.GroupViewIndex.Count = export.Group3.Item.HidPage.StartIndex;
          export.Hidden1.Index = local.GroupViewIndex.Count - 1;

          for(var limit = export.Hidden1.Count; export.Hidden1.Index < limit; ++
            export.Hidden1.Index)
          {
            if (!export.Hidden1.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.HidDisplayReasonText.Flag))
            {
              ++export.Group2.Index;
              export.Group2.CheckSize();

              if (AsChar(export.HidDisplayReasonText.Flag) == 'P')
              {
                var field3 =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field3.Intensity = Intensity.Dark;
                field3.Protected = true;

                var field4 =
                  GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

                field4.Color = "green";
                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = false;

                export.Hidden1.Index = export.HiddenGv1Subscript.Subscript - 1;
                export.Hidden1.CheckSize();

                export.Group2.Update.Gv2NewWorkSet.Text76 =
                  export.Hidden1.Item.HidGv1ReasonText.Text76;
                export.HidDisplayReasonText.Flag = "";

                export.Hidden1.Index = local.GroupViewIndex.Count - 1;
                export.Hidden1.CheckSize();
              }
              else if (AsChar(export.HidDisplayReasonText.Flag) == 'D')
              {
                var field3 =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field3.Intensity = Intensity.Dark;
                field3.Protected = true;

                var field4 =
                  GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

                field4.Color = "cyan";
                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = true;

                export.Hidden1.Index = export.HiddenGv1Subscript.Subscript - 1;
                export.Hidden1.CheckSize();

                export.Group2.Update.Gv2NewWorkSet.Text76 =
                  export.Hidden1.Item.HidGv1ReasonText.Text76;
                export.HidDisplayReasonText.Flag = "";

                export.Hidden1.Index = local.GroupViewIndex.Count - 1;
                export.Hidden1.CheckSize();
              }
            }

            ++export.Group2.Index;
            export.Group2.CheckSize();

            local.RecoveryDate.TextDate =
              NumberToString(DateToInt(
                Date(export.Hidden1.Item.HidGv1PaymentRequest.CreatedTimestamp)),
              8, 8);
            local.RecoveryDate.TextDate =
              Substring(local.RecoveryDate.TextDate,
              DateWorkArea.TextDate_MaxLength, 5, 2) + Substring
              (local.RecoveryDate.TextDate, DateWorkArea.TextDate_MaxLength, 7,
              2) + Substring
              (local.RecoveryDate.TextDate, DateWorkArea.TextDate_MaxLength, 1,
              4);
            local.ProcessDate.TextDate =
              NumberToString(DateToInt(
                export.Hidden1.Item.HidGv1PaymentStatusHistory.EffectiveDate),
              8, 8);
            local.ProcessDate.TextDate =
              Substring(local.ProcessDate.TextDate,
              DateWorkArea.TextDate_MaxLength, 5, 2) + Substring
              (local.ProcessDate.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2)
              + Substring
              (local.ProcessDate.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);
              
            local.PmntRequestAmount.Text9 =
              NumberToString((long)export.Hidden1.Item.HidGv1PaymentRequest.
                Amount, 10, 6) + TrimEnd(".") + NumberToString
              ((long)(export.Hidden1.Item.HidGv1PaymentRequest.Amount * 100),
              14, 2);
            local.Local1Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 1, 1);
            local.Local2Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 2, 1);
            local.Local3Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 3, 1);
            local.Local4Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 4, 1);
            local.Local5Letter.Flag =
              Substring(local.PmntRequestAmount.Text9, 5, 1);

            if (AsChar(local.Local1Letter.Flag) == '0')
            {
              local.Local1Letter.Flag = "";
              local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + Substring
                (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength, 2, 8);
                

              if (AsChar(local.Local2Letter.Flag) == '0')
              {
                local.Local2Letter.Flag = "";
                local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                  .Local2Letter.Flag + Substring
                  (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength, 3,
                  7);

                if (AsChar(local.Local3Letter.Flag) == '0')
                {
                  local.Local3Letter.Flag = "";
                  local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                    .Local2Letter.Flag + local.Local3Letter.Flag + Substring
                    (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength,
                    4, 6);

                  if (AsChar(local.Local4Letter.Flag) == '0')
                  {
                    local.Local4Letter.Flag = "";
                    local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                      .Local2Letter.Flag + local.Local3Letter.Flag + local
                      .Local4Letter.Flag + Substring
                      (local.PmntRequestAmount.Text9,
                      NewWorkSet.Text9_MaxLength, 5, 5);

                    if (AsChar(local.Local5Letter.Flag) == '0')
                    {
                      local.Local5Letter.Flag = "";
                      local.PmntRequestAmount.Text9 =
                        local.Local1Letter.Flag + local.Local2Letter.Flag + local
                        .Local3Letter.Flag + local.Local4Letter.Flag + local
                        .Local5Letter.Flag + Substring
                        (local.PmntRequestAmount.Text9,
                        NewWorkSet.Text9_MaxLength, 6, 4);
                    }
                  }
                }
              }
            }

            export.Group2.Update.Gv2NewWorkSet.Text76 =
              (export.Hidden1.Item.HidGv1PaymentRequest.CsePersonNumber ?? "") +
              local.NewWorkSet.FillerText1 + Substring
              (export.Hidden1.Item.HidGv1CsePersonsWorkSet.FormattedName,
              CsePersonsWorkSet.FormattedName_MaxLength, 1, 15) + local
              .NewWorkSet.FillerText1 + local.RecoveryDate.TextDate + local
              .NewWorkSet.FillerText1 + local.PmntRequestAmount.Text9 + local
              .NewWorkSet.FillerText1 + export
              .Hidden1.Item.HidGv1PaymentStatus.Code + local
              .NewWorkSet.FillerText1 + local.ProcessDate.TextDate + local
              .NewWorkSet.FillerText1 + export
              .Hidden1.Item.HidGv1PaymentStatusHistory.CreatedBy;
            export.Group2.Update.HiddenGv2PaymentRequest.Assign(
              export.Hidden1.Item.HidGv1PaymentRequest);
            export.Group2.Update.HiddenGv2PaymentStatus.Code =
              export.Hidden1.Item.HidGv1PaymentStatus.Code;
            MoveCsePersonsWorkSet(export.Hidden1.Item.HidGv1CsePersonsWorkSet,
              export.Group2.Update.HiddenGv2CsePersonsWorkSet);

            var field1 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field2.Protected = false;

            if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code, "RCVPOT"))
            {
              local.PaymentStatusCode.Flag = "P";
              local.PayStatusHistoryReason.Text76 =
                export.Hidden1.Item.HidGv1ReasonText.Text76;
            }
            else if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code,
              "RCVDEN"))
            {
              local.PaymentStatusCode.Flag = "D";
              local.PayStatusHistoryReason.Text76 =
                export.Hidden1.Item.HidGv1ReasonText.Text76;
            }

            if (export.Group2.Index + 1 >= Export.Group2Group.Capacity)
            {
              // -----------------------------------------------------------------------
              // If the Reason text can not be displayed on the present screen, 
              // display it on next screen by checking the FLAG.
              // --------------------------------------------------------------------------
              if (!IsEmpty(local.PaymentStatusCode.Flag))
              {
                export.HidDisplayReasonText.Flag = local.PaymentStatusCode.Flag;
              }

              --export.HiddenPresent.PageNumber;
              export.HiddenPresent.StartIndex = local.GroupViewIndex.Count;
              export.HiddenGv1Subscript.Subscript = export.Hidden1.Index + 1;
              export.HiddenPresent.EndIndex =
                export.HiddenGv1Subscript.Subscript;

              break;
            }

            if (AsChar(local.PaymentStatusCode.Flag) == 'P')
            {
              local.PaymentStatusCode.Flag = "";

              ++export.Group2.Index;
              export.Group2.CheckSize();

              var field3 = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field3.Intensity = Intensity.Dark;
              field3.Protected = true;

              var field4 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

              field4.Color = "green";
              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = false;

              export.Group2.Update.Gv2NewWorkSet.Text76 =
                local.PayStatusHistoryReason.Text76;
            }
            else if (AsChar(local.PaymentStatusCode.Flag) == 'D')
            {
              local.PaymentStatusCode.Flag = "";

              ++export.Group2.Index;
              export.Group2.CheckSize();

              export.Group2.Update.Gv2NewWorkSet.Text76 =
                local.PayStatusHistoryReason.Text76;

              var field3 = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field3.Intensity = Intensity.Dark;
              field3.Protected = true;

              var field4 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

              field4.Color = "cyan";
              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = true;
            }

            if (export.Group2.Index + 1 >= Export.Group2Group.Capacity)
            {
              --export.HiddenPresent.PageNumber;
              export.HiddenPresent.StartIndex = local.GroupViewIndex.Count;
              export.HiddenGv1Subscript.Subscript = export.Hidden1.Index + 1;
              export.HiddenPresent.EndIndex =
                export.HiddenGv1Subscript.Subscript;

              break;
            }
          }

          export.Hidden1.CheckIndex();
          export.HiddenPrevious.PageNumber = export.HiddenPresent.PageNumber - 1
            ;
          export.HiddenPrevious.StartIndex = 0;
          export.HiddenPrevious.EndIndex = 0;
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";
        global.Command = "BYPASS";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ADD":
        // ***************************************************************
        // One and only one record should be selected.
        // ***************************************************************
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            global.Command = "BYPASS";

            break;
          case 1:
            for(export.Group2.Index = 0; export.Group2.Index < export
              .Group2.Count; ++export.Group2.Index)
            {
              if (!export.Group2.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group2.Item.Gv2Common.SelectChar) == 'S')
              {
                if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code,
                  "RCVDEN"))
                {
                  var field =
                    GetField(export.Group2.Item.Gv2Common, "selectChar");

                  field.Error = true;

                  ExitState = "FN0000_CANNOT_CREATE_RECOV_OBLG";
                  global.Command = "BYPASS";

                  goto Test;
                }
                else if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code,
                  "RCVCRE"))
                {
                  ExitState = "FN0000_RECOVERY_STATUS_CREATED";

                  var field =
                    GetField(export.Group2.Item.Gv2Common, "selectChar");

                  field.Error = true;

                  global.Command = "BYPASS";

                  goto Test;
                }
                else if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code,
                  "RCVPOT"))
                {
                  if (ReadPaymentStatusPaymentStatusHistory())
                  {
                    if (Equal(entities.WarrantPaymentStatus.Code, "RCVCRE"))
                    {
                      ExitState = "FN0000_RECOVERY_STATUS_CREATED";

                      var field =
                        GetField(export.Group2.Item.Gv2Common, "selectChar");

                      field.Error = true;

                      global.Command = "BYPASS";

                      goto Test;
                    }
                  }

                  export.FlowDebtDetail.DueDt =
                    Date(export.ForFlowPaymentRequest.CreatedTimestamp);
                  export.FlowObligationTransaction.Amount =
                    -export.ForFlowPaymentRequest.Amount;
                  export.FlowFromObligationType.Code = "IVD RC";
                  ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";
                  global.Command = "ADDOREL";
                }
              }
            }

            export.Group2.CheckIndex();

            return;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
            global.Command = "BYPASS";

            break;
        }

        break;
      case "UPDATE":
        // ***************************************************************
        // Deny all selected rows.
        // ***************************************************************
        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          global.Command = "BYPASS";

          break;
        }

        local.PassToDenyPaymentStatus.Code = "RCVDEN";
        export.Group2.Index = 0;

        for(var limit = export.Group2.Count; export.Group2.Index < limit; ++
          export.Group2.Index)
        {
          if (!export.Group2.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group2.Item.Gv2Common.SelectChar) == 'S')
          {
            if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code, "RCVDEN"))
            {
              ExitState = "FN0000_RECOVERY_ALREADY_DENIED";

              var field = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field.Error = true;

              global.Command = "BYPASS";

              goto Test;
            }
            else if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code,
              "RCVCRE"))
            {
              ExitState = "FN0000_RECOVERY_ALREADY_CREATED";

              var field = GetField(export.Group2.Item.Gv2Common, "selectChar");

              field.Error = true;

              global.Command = "BYPASS";

              goto Test;
            }
            else if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code,
              "RCVPOT"))
            {
              // ----------------------------------------------------------------------------
              // Check if this is already denied/created.
              // -------------------------------------------------------------------
              if (ReadPaymentStatusPaymentStatusHistory())
              {
                if (Equal(entities.WarrantPaymentStatus.Code, "RCVDEN"))
                {
                  ExitState = "FN0000_RECOVERY_ALREADY_DENIED";

                  var field =
                    GetField(export.Group2.Item.Gv2Common, "selectChar");

                  field.Error = true;

                  global.Command = "BYPASS";

                  goto Test;
                }
                else if (Equal(entities.WarrantPaymentStatus.Code, "RCVCRE"))
                {
                  ExitState = "FN0000_RECOVERY_ALREADY_CREATED";

                  var field =
                    GetField(export.Group2.Item.Gv2Common, "selectChar");

                  field.Error = true;

                  global.Command = "BYPASS";

                  goto Test;
                }
              }
            }

            local.PassToDenyPaymentRequest.SystemGeneratedIdentifier =
              export.Group2.Item.HiddenGv2PaymentRequest.
                SystemGeneratedIdentifier;

            ++export.Group2.Index;
            export.Group2.CheckSize();

            UseFnCabDenyPotentialRecovery();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              global.Command = "BYPASS";

              goto Test;
            }
          }
        }

        export.Group2.CheckIndex();

        for(export.Group2.Index = 0; export.Group2.Index < export.Group2.Count; ++
          export.Group2.Index)
        {
          if (!export.Group2.CheckSize())
          {
            break;
          }

          export.Group2.Update.Gv2Common.SelectChar = "";
        }

        export.Group2.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        global.Command = "DISPLAY";

        break;
      case "PACC":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            global.Command = "BYPASS";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_LST_PAYEE_ACCT";

            return;
          default:
            for(export.Group2.Index = 0; export.Group2.Index < export
              .Group2.Count; ++export.Group2.Index)
            {
              if (!export.Group2.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Group2.Item.Gv2Common.SelectChar))
              {
                var field =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field.Error = true;
              }
            }

            export.Group2.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
            global.Command = "BYPASS";

            break;
        }

        break;
      case "CRRL":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            global.Command = "BYPASS";

            break;
          case 1:
            export.ForFlowReceiptRefund.PayeeName =
              export.ForFlowCsePersonsWorkSet.FormattedName;
            ExitState = "FN0000_FLOW_TO_CRRL_SEL_REFUND";

            return;
          default:
            for(export.Group2.Index = 0; export.Group2.Index < export
              .Group2.Count; ++export.Group2.Index)
            {
              if (!export.Group2.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Group2.Item.Gv2Common.SelectChar))
              {
                var field =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field.Error = true;
              }
            }

            export.Group2.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
            global.Command = "BYPASS";

            break;
        }

        break;
      case "CRAL":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            global.Command = "BYPASS";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_LST_OFFSET_ADVANCEMNT";

            return;
          default:
            for(export.Group2.Index = 0; export.Group2.Index < export
              .Group2.Count; ++export.Group2.Index)
            {
              if (!export.Group2.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Group2.Item.Gv2Common.SelectChar))
              {
                var field =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field.Error = true;
              }
            }

            export.Group2.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
            global.Command = "BYPASS";

            break;
        }

        break;
      case "OREC":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            global.Command = "BYPASS";

            goto Test;
          case 1:
            // -------------------------------------------------------------------------------------
            // The code written below will be executed.
            // -------------------------------------------------------------------------
            break;
          default:
            for(export.Group2.Index = 0; export.Group2.Index < export
              .Group2.Count; ++export.Group2.Index)
            {
              if (!export.Group2.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Group2.Item.Gv2Common.SelectChar))
              {
                var field =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field.Error = true;
              }
            }

            export.Group2.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
            global.Command = "BYPASS";

            goto Test;
        }

        export.FlowDebtDetail.DueDt = local.InitializedDate.Date;
        export.FlowObligationTransaction.Amount = 0;

        if (ReadObligationObligationType())
        {
          // =================================================
          // 12/20/99 - bud adams  -  PR# 82602: Changed COMMAND
          //   from DISPLAY.  Some way is required to know the flow is
          //   from OREL so OREC handles it properly.
          // =================================================
          export.FlowFromObligation.SystemGeneratedIdentifier =
            entities.Obligation.SystemGeneratedIdentifier;
          export.FlowFromObligationType.Assign(entities.ObligationType);
          ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";
          global.Command = "RDISPLAY";

          return;
        }
        else
        {
          ExitState = "FN0000_INVALID_CREATE_RECOVERY";
          global.Command = "BYPASS";
        }

        break;
      case "WDTL":
        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            global.Command = "BYPASS";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_LST_WARRANTS";

            return;
          default:
            for(export.Group2.Index = 0; export.Group2.Index < export
              .Group2.Count; ++export.Group2.Index)
            {
              if (!export.Group2.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Group2.Item.Gv2Common.SelectChar))
              {
                var field =
                  GetField(export.Group2.Item.Gv2Common, "selectChar");

                field.Error = true;
              }
            }

            export.Group2.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
            global.Command = "BYPASS";

            break;
        }

        break;
      case "BYPASS":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";
        global.Command = "BYPASS";

        break;
    }

Test:

    // ------------------------------------------------------------------
    // This code will be executed if an error (Command will be set to 'bypass' 
    // if an error occur on screen)  occured.
    //                         Vithal Madhira (12/28/2000)
    // -------------------------------------------------------------------
    if (Equal(global.Command, "BYPASS") && !IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.CheckGv1.Subscript = export.Hidden1.Count;
      export.Group2.Index = 0;

      for(var limit = export.Group2.Count; export.Group2.Index < limit; ++
        export.Group2.Index)
      {
        if (!export.Group2.CheckSize())
        {
          break;
        }

        var field1 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Group2.Item.Gv2Common, "selectChar");

        field2.Protected = false;

        if (AsChar(export.Group2.Item.Gv2Common.SelectChar) == 'S')
        {
          var field = GetField(export.Group2.Item.Gv2Common, "selectChar");

          field.Error = true;
        }

        if (export.Group2.Index + 1 >= export.Group2.Count)
        {
          break;
        }

        if (IsEmpty(export.Group2.Item.HiddenGv2PaymentStatus.Code))
        {
          export.Hidden1.Index = export.HiddenPresent.StartIndex - 2;
          export.Hidden1.CheckSize();

          if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code, "RCVDEN"))
          {
            var field3 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

            field3.Color = "cyan";
            field3.Highlighting = Highlighting.Underscore;
            field3.Protected = true;

            var field4 = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field4.Intensity = Intensity.Dark;
            field4.Protected = true;
          }
          else if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code, "RCVPOT"))
            
          {
            var field3 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

            field3.Highlighting = Highlighting.Underscore;
            field3.Protected = false;

            var field4 = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field4.Intensity = Intensity.Dark;
            field4.Protected = true;
          }

          export.Hidden1.Index = export.Hidden1.Count - 1;
          export.Hidden1.CheckSize();
        }

        if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code, "RCVDEN"))
        {
          ++export.Group2.Index;
          export.Group2.CheckSize();

          var field3 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

          field3.Color = "cyan";
          field3.Highlighting = Highlighting.Underscore;
          field3.Protected = true;

          var field4 = GetField(export.Group2.Item.Gv2Common, "selectChar");

          field4.Intensity = Intensity.Dark;
          field4.Protected = true;
        }

        if (export.Group2.Index + 1 >= export.Group2.Count)
        {
          break;
        }

        if (Equal(export.Group2.Item.HiddenGv2PaymentStatus.Code, "RCVPOT"))
        {
          ++export.Group2.Index;
          export.Group2.CheckSize();

          var field3 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

          field3.Highlighting = Highlighting.Underscore;
          field3.Protected = false;

          var field4 = GetField(export.Group2.Item.Gv2Common, "selectChar");

          field4.Intensity = Intensity.Dark;
          field4.Protected = true;
        }

        if (export.Group2.Index + 1 >= export.Group2.Count)
        {
          break;
        }
      }

      export.Group2.CheckIndex();

      // ----------------------------------------------------------------------------------
      // For some reason the subscript of Export_Group_Hidden_1 is not set to 
      // last of subscript. So reset to LAST.
      // ----------------------------------------------------------------------------------
      export.Hidden1.Count = local.CheckGv1.Subscript;
    }

    // -------------------------------------------------------------
    // WR# 000262 - SWSRVXM  12/27/2000
    // Per the new business rules , the following code  is modified.
    // -------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      for(export.Group2.Index = 0; export.Group2.Index < export.Group2.Count; ++
        export.Group2.Index)
      {
        if (!export.Group2.CheckSize())
        {
          break;
        }

        export.Group2.Update.Gv2Common.SelectChar = "";

        var field1 = GetField(export.Group2.Item.Gv2Common, "selectChar");

        field1.Intensity = Intensity.Dark;
        field1.Protected = true;

        export.Group2.Update.Gv2NewWorkSet.Text76 = "";

        var field2 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

        field2.Intensity = Intensity.Dark;
        field2.Protected = true;

        export.Group2.Update.HiddenGv2PaymentRequest.Assign(
          local.BlankPaymentRequest);
        export.Group2.Update.HiddenGv2PaymentStatus.Code =
          local.BlankPaymentStatus.Code;
        MoveCsePersonsWorkSet(local.BlankCsePersonsWorkSet,
          export.Group2.Update.HiddenGv2CsePersonsWorkSet);
      }

      export.Group2.CheckIndex();
      export.Group2.Count = 0;

      for(export.Group3.Index = 0; export.Group3.Index < export.Group3.Count; ++
        export.Group3.Index)
      {
        if (!export.Group3.CheckSize())
        {
          break;
        }

        export.Group3.Update.HidPage.StartIndex = 0;
      }

      export.Group3.CheckIndex();
      export.Group3.Count = 0;

      if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
      {
        local.ExitStateUpdate.Flag = "Y";
        ExitState = "ACO_NN0000_ALL_OK";
      }

      // -------------------------------------------------------------
      // WR# 000262 - SWSRVXM  12/27/2000
      // Per the new business rules , the status filter is needed.
      // -------------------------------------------------------------
      // *** Added by G Sharp 12-13-1998 ***
      switch(AsChar(export.SearchStatusCommon.SelectChar))
      {
        case 'P':
          export.SearchStatusPaymentStatus.Code = "RCVPOT";

          break;
        case 'C':
          export.SearchStatusPaymentStatus.Code = "RCVCRE";

          break;
        case 'D':
          export.SearchStatusPaymentStatus.Code = "RCVDEN";

          break;
        case ' ':
          export.SearchStatusPaymentStatus.Code = "";

          break;
        default:
          var field = GetField(export.SearchStatusCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_CODE";

          break;
      }

      UseFnGetAllPaymentReqRecovery();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        for(export.Group2.Index = 0; export.Group2.Index < export.Group2.Count; ++
          export.Group2.Index)
        {
          if (!export.Group2.CheckSize())
          {
            break;
          }

          export.Group2.Update.Gv2Common.SelectChar = "";

          var field1 = GetField(export.Group2.Item.Gv2Common, "selectChar");

          field1.Intensity = Intensity.Dark;
          field1.Protected = true;

          export.Group2.Update.Gv2NewWorkSet.Text76 = "";

          var field2 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

          field2.Intensity = Intensity.Dark;
          field2.Protected = true;
        }

        export.Group2.CheckIndex();

        for(export.Hidden1.Index = 0; export.Hidden1.Index < export
          .Hidden1.Count; ++export.Hidden1.Index)
        {
          if (!export.Hidden1.CheckSize())
          {
            break;
          }

          MoveCsePersonsWorkSet(local.BlankCsePersonsWorkSet,
            export.Hidden1.Update.HidGv1CsePersonsWorkSet);
          export.Hidden1.Update.HidGv1ReasonText.Text76 =
            local.BlankNewWorkSet.Text76;
          export.Hidden1.Update.HidGv1PaymentRequest.Assign(
            local.BlankPaymentRequest);
          export.Hidden1.Update.HidGv1PaymentStatus.Code =
            local.BlankPaymentStatus.Code;
          MovePaymentStatusHistory(local.BlankPaymentStatusHistory,
            export.Hidden1.Update.HidGv1PaymentStatusHistory);
        }

        export.Hidden1.CheckIndex();
        export.ScrollIndicator.Text3 = "";
        export.Hidden1.Count = 0;
        export.Group2.Count = 0;

        return;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Group2.Index = -1;
        local.PaymentStatusCode.Flag = "";
        local.GroupViewIndex.Count = 1;

        if (export.HiddenPresent.StartIndex > 0)
        {
          local.GroupViewIndex.Count = export.HiddenPresent.StartIndex;
        }

        for(export.Hidden1.Index = local.GroupViewIndex.Count - 1; export
          .Hidden1.Index < export.Hidden1.Count; ++export.Hidden1.Index)
        {
          if (!export.Hidden1.CheckSize())
          {
            break;
          }

          ++export.Group2.Index;
          export.Group2.CheckSize();

          local.RecoveryDate.TextDate =
            NumberToString(DateToInt(
              Date(export.Hidden1.Item.HidGv1PaymentRequest.CreatedTimestamp)),
            8, 8);
          local.RecoveryDate.TextDate =
            Substring(local.RecoveryDate.TextDate,
            DateWorkArea.TextDate_MaxLength, 5, 2) + Substring
            (local.RecoveryDate.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) +
            Substring
            (local.RecoveryDate.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);
            
          local.ProcessDate.TextDate =
            NumberToString(DateToInt(
              export.Hidden1.Item.HidGv1PaymentStatusHistory.EffectiveDate), 8,
            8);
          local.ProcessDate.TextDate =
            Substring(local.ProcessDate.TextDate,
            DateWorkArea.TextDate_MaxLength, 5, 2) + Substring
            (local.ProcessDate.TextDate, DateWorkArea.TextDate_MaxLength, 7, 2) +
            Substring
            (local.ProcessDate.TextDate, DateWorkArea.TextDate_MaxLength, 1, 4);
            
          local.PmntRequestAmount.Text9 =
            NumberToString((long)export.Hidden1.Item.HidGv1PaymentRequest.
              Amount, 10, 6) + TrimEnd(".") + NumberToString
            ((long)(export.Hidden1.Item.HidGv1PaymentRequest.Amount * 100), 14,
            2);
          local.Local1Letter.Flag =
            Substring(local.PmntRequestAmount.Text9, 1, 1);
          local.Local2Letter.Flag =
            Substring(local.PmntRequestAmount.Text9, 2, 1);
          local.Local3Letter.Flag =
            Substring(local.PmntRequestAmount.Text9, 3, 1);
          local.Local4Letter.Flag =
            Substring(local.PmntRequestAmount.Text9, 4, 1);
          local.Local5Letter.Flag =
            Substring(local.PmntRequestAmount.Text9, 5, 1);

          if (AsChar(local.Local1Letter.Flag) == '0')
          {
            local.Local1Letter.Flag = "";
            local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + Substring
              (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength, 2, 8);
              

            if (AsChar(local.Local2Letter.Flag) == '0')
            {
              local.Local2Letter.Flag = "";
              local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                .Local2Letter.Flag + Substring
                (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength, 3, 7);
                

              if (AsChar(local.Local3Letter.Flag) == '0')
              {
                local.Local3Letter.Flag = "";
                local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                  .Local2Letter.Flag + local.Local3Letter.Flag + Substring
                  (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength, 4,
                  6);

                if (AsChar(local.Local4Letter.Flag) == '0')
                {
                  local.Local4Letter.Flag = "";
                  local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                    .Local2Letter.Flag + local.Local3Letter.Flag + local
                    .Local4Letter.Flag + Substring
                    (local.PmntRequestAmount.Text9, NewWorkSet.Text9_MaxLength,
                    5, 5);

                  if (AsChar(local.Local5Letter.Flag) == '0')
                  {
                    local.Local5Letter.Flag = "";
                    local.PmntRequestAmount.Text9 = local.Local1Letter.Flag + local
                      .Local2Letter.Flag + local.Local3Letter.Flag + local
                      .Local4Letter.Flag + local.Local5Letter.Flag + Substring
                      (local.PmntRequestAmount.Text9,
                      NewWorkSet.Text9_MaxLength, 6, 4);
                  }
                }
              }
            }
          }

          export.Group2.Update.Gv2NewWorkSet.Text76 =
            (export.Hidden1.Item.HidGv1PaymentRequest.CsePersonNumber ?? "") + local
            .NewWorkSet.FillerText1 + Substring
            (export.Hidden1.Item.HidGv1CsePersonsWorkSet.FormattedName,
            CsePersonsWorkSet.FormattedName_MaxLength, 1, 15) + local
            .NewWorkSet.FillerText1 + local.RecoveryDate.TextDate + local
            .NewWorkSet.FillerText1 + local.PmntRequestAmount.Text9 + local
            .NewWorkSet.FillerText1 + export
            .Hidden1.Item.HidGv1PaymentStatus.Code + local
            .NewWorkSet.FillerText1 + local.ProcessDate.TextDate + local
            .NewWorkSet.FillerText1 + export
            .Hidden1.Item.HidGv1PaymentStatusHistory.CreatedBy;
          export.Group2.Update.HiddenGv2PaymentRequest.Assign(
            export.Hidden1.Item.HidGv1PaymentRequest);
          export.Group2.Update.HiddenGv2PaymentStatus.Code =
            export.Hidden1.Item.HidGv1PaymentStatus.Code;
          MoveCsePersonsWorkSet(export.Hidden1.Item.HidGv1CsePersonsWorkSet,
            export.Group2.Update.HiddenGv2CsePersonsWorkSet);

          var field1 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Group2.Item.Gv2Common, "selectChar");

          field2.Protected = false;

          if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code, "RCVPOT"))
          {
            local.PaymentStatusCode.Flag = "P";
            local.PayStatusHistoryReason.Text76 =
              export.Hidden1.Item.HidGv1ReasonText.Text76;
          }
          else if (Equal(export.Hidden1.Item.HidGv1PaymentStatus.Code, "RCVDEN"))
            
          {
            local.PaymentStatusCode.Flag = "D";
            local.PayStatusHistoryReason.Text76 =
              export.Hidden1.Item.HidGv1ReasonText.Text76;
          }

          if (export.Group2.Index + 1 >= Export.Group2Group.Capacity)
          {
            // -----------------------------------------------------------------------
            // If the Reason text can not be displayed on the present screen, 
            // display it on next screen by checking the FLAG.
            // --------------------------------------------------------------------------
            if (!IsEmpty(local.PaymentStatusCode.Flag))
            {
              export.HidDisplayReasonText.Flag = local.PaymentStatusCode.Flag;
            }

            export.HiddenGv1Subscript.Subscript = export.Hidden1.Index + 1;
            export.HiddenPresent.PageNumber = 1;
            export.HiddenPresent.StartIndex = 1;
            export.HiddenPresent.EndIndex = export.HiddenGv1Subscript.Subscript;
            export.HiddenPrevious.PageNumber = 0;
            export.HiddenPrevious.StartIndex = 0;
            export.HiddenPrevious.EndIndex = 0;

            export.Group3.Index = export.HiddenPresent.PageNumber - 1;
            export.Group3.CheckSize();

            export.Group3.Update.HidPage.StartIndex =
              export.HiddenPresent.StartIndex;

            break;
          }

          if (AsChar(local.PaymentStatusCode.Flag) == 'P')
          {
            local.PaymentStatusCode.Flag = "";

            ++export.Group2.Index;
            export.Group2.CheckSize();

            var field3 = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field3.Intensity = Intensity.Dark;
            field3.Protected = true;

            var field4 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

            field4.Color = "green";
            field4.Highlighting = Highlighting.Underscore;
            field4.Protected = false;

            export.Group2.Update.Gv2NewWorkSet.Text76 =
              local.PayStatusHistoryReason.Text76;
          }
          else if (AsChar(local.PaymentStatusCode.Flag) == 'D')
          {
            local.PaymentStatusCode.Flag = "";

            ++export.Group2.Index;
            export.Group2.CheckSize();

            export.Group2.Update.Gv2NewWorkSet.Text76 =
              local.PayStatusHistoryReason.Text76;

            var field3 = GetField(export.Group2.Item.Gv2Common, "selectChar");

            field3.Intensity = Intensity.Dark;
            field3.Protected = true;

            var field4 = GetField(export.Group2.Item.Gv2NewWorkSet, "text76");

            field4.Color = "cyan";
            field4.Highlighting = Highlighting.Underscore;
            field4.Protected = true;
          }

          if (export.Group2.Index + 1 >= Export.Group2Group.Capacity)
          {
            export.HiddenGv1Subscript.Subscript = export.Hidden1.Index + 1;
            export.HiddenPresent.PageNumber = 1;
            export.HiddenPresent.StartIndex = 1;
            export.HiddenPresent.EndIndex = export.HiddenGv1Subscript.Subscript;
            export.HiddenPrevious.PageNumber = 0;
            export.HiddenPrevious.StartIndex = 0;
            export.HiddenPrevious.EndIndex = 0;

            export.Group3.Index = export.HiddenPresent.PageNumber - 1;
            export.Group3.CheckSize();

            export.Group3.Update.HidPage.StartIndex =
              export.HiddenPresent.StartIndex;

            break;
          }
        }

        export.Hidden1.CheckIndex();

        if (export.Group2.Index < Export.Group2Group.Capacity)
        {
          // ---------------------------------------------------------------------------
          // This is the case where all the records are displayed on one screen 
          // and there are no records to display on the next screen.
          // -------------------------------------------------------------------------------
          export.HiddenGv1Subscript.Subscript = export.Hidden1.Index + 1;
          export.HiddenPresent.PageNumber = 1;
          export.HiddenPresent.StartIndex = 1;
          export.HiddenPresent.EndIndex = export.HiddenGv1Subscript.Subscript;
          export.HiddenPrevious.PageNumber = 0;
          export.HiddenPrevious.StartIndex = 0;
          export.HiddenPrevious.EndIndex = 0;

          export.Group3.Index = export.HiddenPresent.PageNumber - 1;
          export.Group3.CheckSize();

          export.Group3.Update.HidPage.StartIndex =
            export.HiddenPresent.StartIndex;
        }

        if (!export.Group2.IsEmpty)
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else if (export.Group2.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        if (AsChar(local.ExitStateUpdate.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
      }
    }

    // -------------------------------------------------------------
    // WR# 000262 - SWSRVXM  12/27/2000
    // The following code  is needed to set the SCROLL indicator value for 
    // explicit scrolling.
    // -------------------------------------------------------------
    if (export.HiddenGv1Subscript.Subscript < export.Hidden1.Count && export
      .HiddenPresent.PageNumber > 1)
    {
      export.ScrollIndicator.Text3 = "- +";
    }
    else if (export.HiddenGv1Subscript.Subscript == export.Hidden1.Count && export
      .HiddenPresent.PageNumber == 1)
    {
      export.ScrollIndicator.Text3 = "";
    }
    else if (export.HiddenGv1Subscript.Subscript < export.Hidden1.Count)
    {
      export.ScrollIndicator.Text3 = "  +";
    }
    else if (export.HiddenGv1Subscript.Subscript == export.Hidden1.Count && export
      .HiddenPresent.PageNumber > 1)
    {
      export.ScrollIndicator.Text3 = "-";

      if (export.Hidden1.Count == Export.Hidden1Group.Capacity)
      {
        ExitState = "ACO_NI0000_LIST_IS_FULL";
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveGroupToHidden1(FnGetAllPaymentReqRecovery.Export.
    GroupGroup source, Export.Hidden1Group target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
    target.HidGv1ReasonText.Text76 = source.PayHistReasonText.Text76;
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

  private static void MovePaymentStatusHistory(PaymentStatusHistory source,
    PaymentStatusHistory target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.CreatedBy = source.CreatedBy;
  }

  private void UseFnCabDenyPotentialRecovery()
  {
    var useImport = new FnCabDenyPotentialRecovery.Import();
    var useExport = new FnCabDenyPotentialRecovery.Export();

    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.PaymentStatus.Code = local.PassToDenyPaymentStatus.Code;
    useImport.PaymentRequest.SystemGeneratedIdentifier =
      local.PassToDenyPaymentRequest.SystemGeneratedIdentifier;
    useImport.PmntStatHistReasonTxt.Text76 =
      export.Group2.Item.Gv2NewWorkSet.Text76;

    Call(FnCabDenyPotentialRecovery.Execute, useImport, useExport);
  }

  private void UseFnGetAllPaymentReqRecovery()
  {
    var useImport = new FnGetAllPaymentReqRecovery.Import();
    var useExport = new FnGetAllPaymentReqRecovery.Export();

    useImport.SearchTo.ProcessDate = export.SearchTo.ProcessDate;
    useImport.SearchFrom.ProcessDate = export.SearchFrom.ProcessDate;
    useImport.PaymentStatusHistory.CreatedBy = import.Search.CreatedBy;
    useImport.History.Flag = export.History.Flag;
    useImport.SearchObligee.Number = export.SearchObligeeCsePerson.Number;
    useImport.Search.Code = export.SearchStatusPaymentStatus.Code;
    useImport.Current.Date = local.Current.Date;

    Call(FnGetAllPaymentReqRecovery.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Hidden1, MoveGroupToHidden1);
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

    useImport.CsePerson.Number = import.SearchObligeeCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private bool ReadObligationObligationType()
  {
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqId",
          export.ForFlowPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.Obligation.PrqId = db.GetNullableInt32(reader, 4);
        entities.ObligationType.Code = db.GetString(reader, 5);
        entities.ObligationType.Classification = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadPaymentStatusPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;
    entities.WarrantPaymentStatus.Populated = false;

    return Read("ReadPaymentStatusPaymentStatusHistory",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prqGeneratedId",
          export.Group2.Item.HiddenGv2PaymentRequest.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.WarrantPaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.WarrantPaymentStatus.Code = db.GetString(reader, 1);
        entities.WarrantPaymentStatus.EffectiveDate = db.GetDate(reader, 2);
        entities.WarrantPaymentStatus.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 4);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 6);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.PaymentStatusHistory.Populated = true;
        entities.WarrantPaymentStatus.Populated = true;
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
      /// A value of PaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("paymentStatusHistory")]
      public PaymentStatusHistory PaymentStatusHistory
      {
        get => paymentStatusHistory ??= new();
        set => paymentStatusHistory = value;
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
      /// A value of PaymentRequest.
      /// </summary>
      [JsonPropertyName("paymentRequest")]
      public PaymentRequest PaymentRequest
      {
        get => paymentRequest ??= new();
        set => paymentRequest = value;
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
      /// A value of PaymentStatus.
      /// </summary>
      [JsonPropertyName("paymentStatus")]
      public PaymentStatus PaymentStatus
      {
        get => paymentStatus ??= new();
        set => paymentStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private PaymentStatusHistory paymentStatusHistory;
      private Common common;
      private PaymentRequest paymentRequest;
      private CsePersonsWorkSet csePersonsWorkSet;
      private PaymentStatus paymentStatus;
    }

    /// <summary>A Group2Group group.</summary>
    [Serializable]
    public class Group2Group
    {
      /// <summary>
      /// A value of Gv2NewWorkSet.
      /// </summary>
      [JsonPropertyName("gv2NewWorkSet")]
      public NewWorkSet Gv2NewWorkSet
      {
        get => gv2NewWorkSet ??= new();
        set => gv2NewWorkSet = value;
      }

      /// <summary>
      /// A value of Gv2Common.
      /// </summary>
      [JsonPropertyName("gv2Common")]
      public Common Gv2Common
      {
        get => gv2Common ??= new();
        set => gv2Common = value;
      }

      /// <summary>
      /// A value of HiddenGv2PaymentRequest.
      /// </summary>
      [JsonPropertyName("hiddenGv2PaymentRequest")]
      public PaymentRequest HiddenGv2PaymentRequest
      {
        get => hiddenGv2PaymentRequest ??= new();
        set => hiddenGv2PaymentRequest = value;
      }

      /// <summary>
      /// A value of HiddenGv2PaymentStatus.
      /// </summary>
      [JsonPropertyName("hiddenGv2PaymentStatus")]
      public PaymentStatus HiddenGv2PaymentStatus
      {
        get => hiddenGv2PaymentStatus ??= new();
        set => hiddenGv2PaymentStatus = value;
      }

      /// <summary>
      /// A value of HiddenGv2CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGv2CsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGv2CsePersonsWorkSet
      {
        get => hiddenGv2CsePersonsWorkSet ??= new();
        set => hiddenGv2CsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private NewWorkSet gv2NewWorkSet;
      private Common gv2Common;
      private PaymentRequest hiddenGv2PaymentRequest;
      private PaymentStatus hiddenGv2PaymentStatus;
      private CsePersonsWorkSet hiddenGv2CsePersonsWorkSet;
    }

    /// <summary>A Hidden1Group group.</summary>
    [Serializable]
    public class Hidden1Group
    {
      /// <summary>
      /// A value of HidGv1PaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("hidGv1PaymentStatusHistory")]
      public PaymentStatusHistory HidGv1PaymentStatusHistory
      {
        get => hidGv1PaymentStatusHistory ??= new();
        set => hidGv1PaymentStatusHistory = value;
      }

      /// <summary>
      /// A value of HidGv1PaymentRequest.
      /// </summary>
      [JsonPropertyName("hidGv1PaymentRequest")]
      public PaymentRequest HidGv1PaymentRequest
      {
        get => hidGv1PaymentRequest ??= new();
        set => hidGv1PaymentRequest = value;
      }

      /// <summary>
      /// A value of HidGv1CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hidGv1CsePersonsWorkSet")]
      public CsePersonsWorkSet HidGv1CsePersonsWorkSet
      {
        get => hidGv1CsePersonsWorkSet ??= new();
        set => hidGv1CsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of HidGv1PaymentStatus.
      /// </summary>
      [JsonPropertyName("hidGv1PaymentStatus")]
      public PaymentStatus HidGv1PaymentStatus
      {
        get => hidGv1PaymentStatus ??= new();
        set => hidGv1PaymentStatus = value;
      }

      /// <summary>
      /// A value of HidGv1ReasonText.
      /// </summary>
      [JsonPropertyName("hidGv1ReasonText")]
      public NewWorkSet HidGv1ReasonText
      {
        get => hidGv1ReasonText ??= new();
        set => hidGv1ReasonText = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 84;

      private PaymentStatusHistory hidGv1PaymentStatusHistory;
      private PaymentRequest hidGv1PaymentRequest;
      private CsePersonsWorkSet hidGv1CsePersonsWorkSet;
      private PaymentStatus hidGv1PaymentStatus;
      private NewWorkSet hidGv1ReasonText;
    }

    /// <summary>A Group3Group group.</summary>
    [Serializable]
    public class Group3Group
    {
      /// <summary>
      /// A value of HidPage.
      /// </summary>
      [JsonPropertyName("hidPage")]
      public NewWorkSet HidPage
      {
        get => hidPage ??= new();
        set => hidPage = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private NewWorkSet hidPage;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public PaymentRequest SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of FromFlow.
    /// </summary>
    [JsonPropertyName("fromFlow")]
    public CsePersonsWorkSet FromFlow
    {
      get => fromFlow ??= new();
      set => fromFlow = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public PaymentStatusHistory Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of SearchStatusCommon.
    /// </summary>
    [JsonPropertyName("searchStatusCommon")]
    public Common SearchStatusCommon
    {
      get => searchStatusCommon ??= new();
      set => searchStatusCommon = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public Standard Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public PaymentRequest SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchStatusPaymentStatus.
    /// </summary>
    [JsonPropertyName("searchStatusPaymentStatus")]
    public PaymentStatus SearchStatusPaymentStatus
    {
      get => searchStatusPaymentStatus ??= new();
      set => searchStatusPaymentStatus = value;
    }

    /// <summary>
    /// A value of StartingDte.
    /// </summary>
    [JsonPropertyName("startingDte")]
    public DateWorkArea StartingDte
    {
      get => startingDte ??= new();
      set => startingDte = value;
    }

    /// <summary>
    /// A value of SearchObligeeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchObligeeCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchObligeeCsePersonsWorkSet
    {
      get => searchObligeeCsePersonsWorkSet ??= new();
      set => searchObligeeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of History.
    /// </summary>
    [JsonPropertyName("history")]
    public Common History
    {
      get => history ??= new();
      set => history = value;
    }

    /// <summary>
    /// A value of SearchObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("searchObligeeCsePerson")]
    public CsePerson SearchObligeeCsePerson
    {
      get => searchObligeeCsePerson ??= new();
      set => searchObligeeCsePerson = value;
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
    /// A value of HiddenFirstTime.
    /// </summary>
    [JsonPropertyName("hiddenFirstTime")]
    public Common HiddenFirstTime
    {
      get => hiddenFirstTime ??= new();
      set => hiddenFirstTime = value;
    }

    /// <summary>
    /// Gets a value of Group2.
    /// </summary>
    [JsonIgnore]
    public Array<Group2Group> Group2 => group2 ??= new(Group2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Group2 for json serialization.
    /// </summary>
    [JsonPropertyName("group2")]
    [Computed]
    public IList<Group2Group> Group2_Json
    {
      get => group2;
      set => Group2.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hidden1.
    /// </summary>
    [JsonIgnore]
    public Array<Hidden1Group> Hidden1 => hidden1 ??= new(
      Hidden1Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Hidden1 for json serialization.
    /// </summary>
    [JsonPropertyName("hidden1")]
    [Computed]
    public IList<Hidden1Group> Hidden1_Json
    {
      get => hidden1;
      set => Hidden1.Assign(value);
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public NewWorkSet ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// A value of HiddenGv1Subscript.
    /// </summary>
    [JsonPropertyName("hiddenGv1Subscript")]
    public Common HiddenGv1Subscript
    {
      get => hiddenGv1Subscript ??= new();
      set => hiddenGv1Subscript = value;
    }

    /// <summary>
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public NewWorkSet HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    /// <summary>
    /// A value of HiddenPresent.
    /// </summary>
    [JsonPropertyName("hiddenPresent")]
    public NewWorkSet HiddenPresent
    {
      get => hiddenPresent ??= new();
      set => hiddenPresent = value;
    }

    /// <summary>
    /// A value of HiddenGv2.
    /// </summary>
    [JsonPropertyName("hiddenGv2")]
    public Common HiddenGv2
    {
      get => hiddenGv2 ??= new();
      set => hiddenGv2 = value;
    }

    /// <summary>
    /// A value of HidDisplayReasonText.
    /// </summary>
    [JsonPropertyName("hidDisplayReasonText")]
    public Common HidDisplayReasonText
    {
      get => hidDisplayReasonText ??= new();
      set => hidDisplayReasonText = value;
    }

    /// <summary>
    /// Gets a value of Group3.
    /// </summary>
    [JsonIgnore]
    public Array<Group3Group> Group3 => group3 ??= new(Group3Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Group3 for json serialization.
    /// </summary>
    [JsonPropertyName("group3")]
    [Computed]
    public IList<Group3Group> Group3_Json
    {
      get => group3;
      set => Group3.Assign(value);
    }

    private PaymentRequest searchTo;
    private CsePersonsWorkSet fromFlow;
    private PaymentStatusHistory search;
    private Common searchStatusCommon;
    private Array<GroupGroup> group;
    private Standard status;
    private Standard csePerson;
    private PaymentRequest searchFrom;
    private PaymentStatus searchStatusPaymentStatus;
    private DateWorkArea startingDte;
    private CsePersonsWorkSet searchObligeeCsePersonsWorkSet;
    private Common history;
    private CsePerson searchObligeeCsePerson;
    private NextTranInfo hidden;
    private Standard standard;
    private Common hiddenFirstTime;
    private Array<Group2Group> group2;
    private Array<Hidden1Group> hidden1;
    private NewWorkSet scrollIndicator;
    private Common hiddenGv1Subscript;
    private NewWorkSet hiddenPrevious;
    private NewWorkSet hiddenPresent;
    private Common hiddenGv2;
    private Common hidDisplayReasonText;
    private Array<Group3Group> group3;
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
      /// A value of PaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("paymentStatusHistory")]
      public PaymentStatusHistory PaymentStatusHistory
      {
        get => paymentStatusHistory ??= new();
        set => paymentStatusHistory = value;
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
      /// A value of PaymentRequest.
      /// </summary>
      [JsonPropertyName("paymentRequest")]
      public PaymentRequest PaymentRequest
      {
        get => paymentRequest ??= new();
        set => paymentRequest = value;
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
      /// A value of PaymentStatus.
      /// </summary>
      [JsonPropertyName("paymentStatus")]
      public PaymentStatus PaymentStatus
      {
        get => paymentStatus ??= new();
        set => paymentStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private PaymentStatusHistory paymentStatusHistory;
      private Common common;
      private PaymentRequest paymentRequest;
      private CsePersonsWorkSet csePersonsWorkSet;
      private PaymentStatus paymentStatus;
    }

    /// <summary>A Group2Group group.</summary>
    [Serializable]
    public class Group2Group
    {
      /// <summary>
      /// A value of Gv2NewWorkSet.
      /// </summary>
      [JsonPropertyName("gv2NewWorkSet")]
      public NewWorkSet Gv2NewWorkSet
      {
        get => gv2NewWorkSet ??= new();
        set => gv2NewWorkSet = value;
      }

      /// <summary>
      /// A value of Gv2Common.
      /// </summary>
      [JsonPropertyName("gv2Common")]
      public Common Gv2Common
      {
        get => gv2Common ??= new();
        set => gv2Common = value;
      }

      /// <summary>
      /// A value of HiddenGv2PaymentRequest.
      /// </summary>
      [JsonPropertyName("hiddenGv2PaymentRequest")]
      public PaymentRequest HiddenGv2PaymentRequest
      {
        get => hiddenGv2PaymentRequest ??= new();
        set => hiddenGv2PaymentRequest = value;
      }

      /// <summary>
      /// A value of HiddenGv2PaymentStatus.
      /// </summary>
      [JsonPropertyName("hiddenGv2PaymentStatus")]
      public PaymentStatus HiddenGv2PaymentStatus
      {
        get => hiddenGv2PaymentStatus ??= new();
        set => hiddenGv2PaymentStatus = value;
      }

      /// <summary>
      /// A value of HiddenGv2CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hiddenGv2CsePersonsWorkSet")]
      public CsePersonsWorkSet HiddenGv2CsePersonsWorkSet
      {
        get => hiddenGv2CsePersonsWorkSet ??= new();
        set => hiddenGv2CsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private NewWorkSet gv2NewWorkSet;
      private Common gv2Common;
      private PaymentRequest hiddenGv2PaymentRequest;
      private PaymentStatus hiddenGv2PaymentStatus;
      private CsePersonsWorkSet hiddenGv2CsePersonsWorkSet;
    }

    /// <summary>A Hidden1Group group.</summary>
    [Serializable]
    public class Hidden1Group
    {
      /// <summary>
      /// A value of HidGv1PaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("hidGv1PaymentStatusHistory")]
      public PaymentStatusHistory HidGv1PaymentStatusHistory
      {
        get => hidGv1PaymentStatusHistory ??= new();
        set => hidGv1PaymentStatusHistory = value;
      }

      /// <summary>
      /// A value of HidGv1PaymentRequest.
      /// </summary>
      [JsonPropertyName("hidGv1PaymentRequest")]
      public PaymentRequest HidGv1PaymentRequest
      {
        get => hidGv1PaymentRequest ??= new();
        set => hidGv1PaymentRequest = value;
      }

      /// <summary>
      /// A value of HidGv1CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("hidGv1CsePersonsWorkSet")]
      public CsePersonsWorkSet HidGv1CsePersonsWorkSet
      {
        get => hidGv1CsePersonsWorkSet ??= new();
        set => hidGv1CsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of HidGv1PaymentStatus.
      /// </summary>
      [JsonPropertyName("hidGv1PaymentStatus")]
      public PaymentStatus HidGv1PaymentStatus
      {
        get => hidGv1PaymentStatus ??= new();
        set => hidGv1PaymentStatus = value;
      }

      /// <summary>
      /// A value of HidGv1ReasonText.
      /// </summary>
      [JsonPropertyName("hidGv1ReasonText")]
      public NewWorkSet HidGv1ReasonText
      {
        get => hidGv1ReasonText ??= new();
        set => hidGv1ReasonText = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 84;

      private PaymentStatusHistory hidGv1PaymentStatusHistory;
      private PaymentRequest hidGv1PaymentRequest;
      private CsePersonsWorkSet hidGv1CsePersonsWorkSet;
      private PaymentStatus hidGv1PaymentStatus;
      private NewWorkSet hidGv1ReasonText;
    }

    /// <summary>A Group3Group group.</summary>
    [Serializable]
    public class Group3Group
    {
      /// <summary>
      /// A value of HidPage.
      /// </summary>
      [JsonPropertyName("hidPage")]
      public NewWorkSet HidPage
      {
        get => hidPage ??= new();
        set => hidPage = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private NewWorkSet hidPage;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public PaymentRequest SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of SeltectedFlowOn.
    /// </summary>
    [JsonPropertyName("seltectedFlowOn")]
    public CsePerson SeltectedFlowOn
    {
      get => seltectedFlowOn ??= new();
      set => seltectedFlowOn = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public PaymentStatusHistory Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of SearchStatusCommon.
    /// </summary>
    [JsonPropertyName("searchStatusCommon")]
    public Common SearchStatusCommon
    {
      get => searchStatusCommon ??= new();
      set => searchStatusCommon = value;
    }

    /// <summary>
    /// A value of ForFlowCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("forFlowCsePersonsWorkSet")]
    public CsePersonsWorkSet ForFlowCsePersonsWorkSet
    {
      get => forFlowCsePersonsWorkSet ??= new();
      set => forFlowCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ForFlowPaymentRequest.
    /// </summary>
    [JsonPropertyName("forFlowPaymentRequest")]
    public PaymentRequest ForFlowPaymentRequest
    {
      get => forFlowPaymentRequest ??= new();
      set => forFlowPaymentRequest = value;
    }

    /// <summary>
    /// A value of ForFlowReceiptRefund.
    /// </summary>
    [JsonPropertyName("forFlowReceiptRefund")]
    public ReceiptRefund ForFlowReceiptRefund
    {
      get => forFlowReceiptRefund ??= new();
      set => forFlowReceiptRefund = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public Standard Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public PaymentRequest SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchStatusPaymentStatus.
    /// </summary>
    [JsonPropertyName("searchStatusPaymentStatus")]
    public PaymentStatus SearchStatusPaymentStatus
    {
      get => searchStatusPaymentStatus ??= new();
      set => searchStatusPaymentStatus = value;
    }

    /// <summary>
    /// A value of StartingDte.
    /// </summary>
    [JsonPropertyName("startingDte")]
    public DateWorkArea StartingDte
    {
      get => startingDte ??= new();
      set => startingDte = value;
    }

    /// <summary>
    /// A value of SearchObligeeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchObligeeCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchObligeeCsePersonsWorkSet
    {
      get => searchObligeeCsePersonsWorkSet ??= new();
      set => searchObligeeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of History.
    /// </summary>
    [JsonPropertyName("history")]
    public Common History
    {
      get => history ??= new();
      set => history = value;
    }

    /// <summary>
    /// A value of SearchObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("searchObligeeCsePerson")]
    public CsePerson SearchObligeeCsePerson
    {
      get => searchObligeeCsePerson ??= new();
      set => searchObligeeCsePerson = value;
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
    /// A value of FlowSpTextWorkArea.
    /// </summary>
    [JsonPropertyName("flowSpTextWorkArea")]
    public SpTextWorkArea FlowSpTextWorkArea
    {
      get => flowSpTextWorkArea ??= new();
      set => flowSpTextWorkArea = value;
    }

    /// <summary>
    /// A value of FlowObligationTransaction.
    /// </summary>
    [JsonPropertyName("flowObligationTransaction")]
    public ObligationTransaction FlowObligationTransaction
    {
      get => flowObligationTransaction ??= new();
      set => flowObligationTransaction = value;
    }

    /// <summary>
    /// A value of FlowDebtDetail.
    /// </summary>
    [JsonPropertyName("flowDebtDetail")]
    public DebtDetail FlowDebtDetail
    {
      get => flowDebtDetail ??= new();
      set => flowDebtDetail = value;
    }

    /// <summary>
    /// A value of FlowFromObligation.
    /// </summary>
    [JsonPropertyName("flowFromObligation")]
    public Obligation FlowFromObligation
    {
      get => flowFromObligation ??= new();
      set => flowFromObligation = value;
    }

    /// <summary>
    /// A value of FlowFromObligationType.
    /// </summary>
    [JsonPropertyName("flowFromObligationType")]
    public ObligationType FlowFromObligationType
    {
      get => flowFromObligationType ??= new();
      set => flowFromObligationType = value;
    }

    /// <summary>
    /// A value of HiddenFirstTime.
    /// </summary>
    [JsonPropertyName("hiddenFirstTime")]
    public Common HiddenFirstTime
    {
      get => hiddenFirstTime ??= new();
      set => hiddenFirstTime = value;
    }

    /// <summary>
    /// Gets a value of Group2.
    /// </summary>
    [JsonIgnore]
    public Array<Group2Group> Group2 => group2 ??= new(Group2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Group2 for json serialization.
    /// </summary>
    [JsonPropertyName("group2")]
    [Computed]
    public IList<Group2Group> Group2_Json
    {
      get => group2;
      set => Group2.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hidden1.
    /// </summary>
    [JsonIgnore]
    public Array<Hidden1Group> Hidden1 => hidden1 ??= new(
      Hidden1Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Hidden1 for json serialization.
    /// </summary>
    [JsonPropertyName("hidden1")]
    [Computed]
    public IList<Hidden1Group> Hidden1_Json
    {
      get => hidden1;
      set => Hidden1.Assign(value);
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public NewWorkSet ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// A value of HiddenGv1Subscript.
    /// </summary>
    [JsonPropertyName("hiddenGv1Subscript")]
    public Common HiddenGv1Subscript
    {
      get => hiddenGv1Subscript ??= new();
      set => hiddenGv1Subscript = value;
    }

    /// <summary>
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public NewWorkSet HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    /// <summary>
    /// A value of HiddenPresent.
    /// </summary>
    [JsonPropertyName("hiddenPresent")]
    public NewWorkSet HiddenPresent
    {
      get => hiddenPresent ??= new();
      set => hiddenPresent = value;
    }

    /// <summary>
    /// A value of HiddenGv2.
    /// </summary>
    [JsonPropertyName("hiddenGv2")]
    public Common HiddenGv2
    {
      get => hiddenGv2 ??= new();
      set => hiddenGv2 = value;
    }

    /// <summary>
    /// A value of HidDisplayReasonText.
    /// </summary>
    [JsonPropertyName("hidDisplayReasonText")]
    public Common HidDisplayReasonText
    {
      get => hidDisplayReasonText ??= new();
      set => hidDisplayReasonText = value;
    }

    /// <summary>
    /// Gets a value of Group3.
    /// </summary>
    [JsonIgnore]
    public Array<Group3Group> Group3 => group3 ??= new(Group3Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Group3 for json serialization.
    /// </summary>
    [JsonPropertyName("group3")]
    [Computed]
    public IList<Group3Group> Group3_Json
    {
      get => group3;
      set => Group3.Assign(value);
    }

    private PaymentRequest searchTo;
    private CsePerson seltectedFlowOn;
    private PaymentStatusHistory search;
    private Common searchStatusCommon;
    private CsePersonsWorkSet forFlowCsePersonsWorkSet;
    private PaymentRequest forFlowPaymentRequest;
    private ReceiptRefund forFlowReceiptRefund;
    private Array<GroupGroup> group;
    private Standard status;
    private Standard csePerson;
    private PaymentRequest searchFrom;
    private PaymentStatus searchStatusPaymentStatus;
    private DateWorkArea startingDte;
    private CsePersonsWorkSet searchObligeeCsePersonsWorkSet;
    private Common history;
    private CsePerson searchObligeeCsePerson;
    private NextTranInfo hidden;
    private Standard standard;
    private SpTextWorkArea flowSpTextWorkArea;
    private ObligationTransaction flowObligationTransaction;
    private DebtDetail flowDebtDetail;
    private Obligation flowFromObligation;
    private ObligationType flowFromObligationType;
    private Common hiddenFirstTime;
    private Array<Group2Group> group2;
    private Array<Hidden1Group> hidden1;
    private NewWorkSet scrollIndicator;
    private Common hiddenGv1Subscript;
    private NewWorkSet hiddenPrevious;
    private NewWorkSet hiddenPresent;
    private Common hiddenGv2;
    private Common hidDisplayReasonText;
    private Array<Group3Group> group3;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public PaymentStatus Check
    {
      get => check ??= new();
      set => check = value;
    }

    /// <summary>
    /// A value of HardcodeOtIvdRecovery.
    /// </summary>
    [JsonPropertyName("hardcodeOtIvdRecovery")]
    public ObligationType HardcodeOtIvdRecovery
    {
      get => hardcodeOtIvdRecovery ??= new();
      set => hardcodeOtIvdRecovery = value;
    }

    /// <summary>
    /// A value of ExitStateUpdate.
    /// </summary>
    [JsonPropertyName("exitStateUpdate")]
    public Common ExitStateUpdate
    {
      get => exitStateUpdate ??= new();
      set => exitStateUpdate = value;
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
    /// A value of PassToDenyPaymentStatus.
    /// </summary>
    [JsonPropertyName("passToDenyPaymentStatus")]
    public PaymentStatus PassToDenyPaymentStatus
    {
      get => passToDenyPaymentStatus ??= new();
      set => passToDenyPaymentStatus = value;
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
    /// A value of PromptsSelected.
    /// </summary>
    [JsonPropertyName("promptsSelected")]
    public Common PromptsSelected
    {
      get => promptsSelected ??= new();
      set => promptsSelected = value;
    }

    /// <summary>
    /// A value of FilterCounter.
    /// </summary>
    [JsonPropertyName("filterCounter")]
    public Common FilterCounter
    {
      get => filterCounter ??= new();
      set => filterCounter = value;
    }

    /// <summary>
    /// A value of InitializedDate.
    /// </summary>
    [JsonPropertyName("initializedDate")]
    public DateWorkArea InitializedDate
    {
      get => initializedDate ??= new();
      set => initializedDate = value;
    }

    /// <summary>
    /// A value of RecoveryDate.
    /// </summary>
    [JsonPropertyName("recoveryDate")]
    public DateWorkArea RecoveryDate
    {
      get => recoveryDate ??= new();
      set => recoveryDate = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of PmntRequestAmount.
    /// </summary>
    [JsonPropertyName("pmntRequestAmount")]
    public NewWorkSet PmntRequestAmount
    {
      get => pmntRequestAmount ??= new();
      set => pmntRequestAmount = value;
    }

    /// <summary>
    /// A value of NewWorkSet.
    /// </summary>
    [JsonPropertyName("newWorkSet")]
    public NewWorkSet NewWorkSet
    {
      get => newWorkSet ??= new();
      set => newWorkSet = value;
    }

    /// <summary>
    /// A value of PaymentStatusCode.
    /// </summary>
    [JsonPropertyName("paymentStatusCode")]
    public Common PaymentStatusCode
    {
      get => paymentStatusCode ??= new();
      set => paymentStatusCode = value;
    }

    /// <summary>
    /// A value of PayStatusHistoryReason.
    /// </summary>
    [JsonPropertyName("payStatusHistoryReason")]
    public NewWorkSet PayStatusHistoryReason
    {
      get => payStatusHistoryReason ??= new();
      set => payStatusHistoryReason = value;
    }

    /// <summary>
    /// A value of BlankCommon.
    /// </summary>
    [JsonPropertyName("blankCommon")]
    public Common BlankCommon
    {
      get => blankCommon ??= new();
      set => blankCommon = value;
    }

    /// <summary>
    /// A value of ProtectFields.
    /// </summary>
    [JsonPropertyName("protectFields")]
    public Common ProtectFields
    {
      get => protectFields ??= new();
      set => protectFields = value;
    }

    /// <summary>
    /// A value of PassToDenyNewWorkSet.
    /// </summary>
    [JsonPropertyName("passToDenyNewWorkSet")]
    public NewWorkSet PassToDenyNewWorkSet
    {
      get => passToDenyNewWorkSet ??= new();
      set => passToDenyNewWorkSet = value;
    }

    /// <summary>
    /// A value of PassToDenyPaymentRequest.
    /// </summary>
    [JsonPropertyName("passToDenyPaymentRequest")]
    public PaymentRequest PassToDenyPaymentRequest
    {
      get => passToDenyPaymentRequest ??= new();
      set => passToDenyPaymentRequest = value;
    }

    /// <summary>
    /// A value of Local1Letter.
    /// </summary>
    [JsonPropertyName("local1Letter")]
    public Common Local1Letter
    {
      get => local1Letter ??= new();
      set => local1Letter = value;
    }

    /// <summary>
    /// A value of Local2Letter.
    /// </summary>
    [JsonPropertyName("local2Letter")]
    public Common Local2Letter
    {
      get => local2Letter ??= new();
      set => local2Letter = value;
    }

    /// <summary>
    /// A value of Local3Letter.
    /// </summary>
    [JsonPropertyName("local3Letter")]
    public Common Local3Letter
    {
      get => local3Letter ??= new();
      set => local3Letter = value;
    }

    /// <summary>
    /// A value of Local4Letter.
    /// </summary>
    [JsonPropertyName("local4Letter")]
    public Common Local4Letter
    {
      get => local4Letter ??= new();
      set => local4Letter = value;
    }

    /// <summary>
    /// A value of Local5Letter.
    /// </summary>
    [JsonPropertyName("local5Letter")]
    public Common Local5Letter
    {
      get => local5Letter ??= new();
      set => local5Letter = value;
    }

    /// <summary>
    /// A value of GroupViewIndex.
    /// </summary>
    [JsonPropertyName("groupViewIndex")]
    public Common GroupViewIndex
    {
      get => groupViewIndex ??= new();
      set => groupViewIndex = value;
    }

    /// <summary>
    /// A value of BlankPaymentRequest.
    /// </summary>
    [JsonPropertyName("blankPaymentRequest")]
    public PaymentRequest BlankPaymentRequest
    {
      get => blankPaymentRequest ??= new();
      set => blankPaymentRequest = value;
    }

    /// <summary>
    /// A value of BlankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("blankCsePersonsWorkSet")]
    public CsePersonsWorkSet BlankCsePersonsWorkSet
    {
      get => blankCsePersonsWorkSet ??= new();
      set => blankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of BlankPaymentStatus.
    /// </summary>
    [JsonPropertyName("blankPaymentStatus")]
    public PaymentStatus BlankPaymentStatus
    {
      get => blankPaymentStatus ??= new();
      set => blankPaymentStatus = value;
    }

    /// <summary>
    /// A value of CheckGv2.
    /// </summary>
    [JsonPropertyName("checkGv2")]
    public Common CheckGv2
    {
      get => checkGv2 ??= new();
      set => checkGv2 = value;
    }

    /// <summary>
    /// A value of CheckGv1.
    /// </summary>
    [JsonPropertyName("checkGv1")]
    public Common CheckGv1
    {
      get => checkGv1 ??= new();
      set => checkGv1 = value;
    }

    /// <summary>
    /// A value of BlankPaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("blankPaymentStatusHistory")]
    public PaymentStatusHistory BlankPaymentStatusHistory
    {
      get => blankPaymentStatusHistory ??= new();
      set => blankPaymentStatusHistory = value;
    }

    /// <summary>
    /// A value of BlankNewWorkSet.
    /// </summary>
    [JsonPropertyName("blankNewWorkSet")]
    public NewWorkSet BlankNewWorkSet
    {
      get => blankNewWorkSet ??= new();
      set => blankNewWorkSet = value;
    }

    private DateWorkArea current;
    private PaymentStatus check;
    private ObligationType hardcodeOtIvdRecovery;
    private Common exitStateUpdate;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PaymentStatus passToDenyPaymentStatus;
    private Common common;
    private Common promptsSelected;
    private Common filterCounter;
    private DateWorkArea initializedDate;
    private DateWorkArea recoveryDate;
    private DateWorkArea processDate;
    private NewWorkSet pmntRequestAmount;
    private NewWorkSet newWorkSet;
    private Common paymentStatusCode;
    private NewWorkSet payStatusHistoryReason;
    private Common blankCommon;
    private Common protectFields;
    private NewWorkSet passToDenyNewWorkSet;
    private PaymentRequest passToDenyPaymentRequest;
    private Common local1Letter;
    private Common local2Letter;
    private Common local3Letter;
    private Common local4Letter;
    private Common local5Letter;
    private Common groupViewIndex;
    private PaymentRequest blankPaymentRequest;
    private CsePersonsWorkSet blankCsePersonsWorkSet;
    private PaymentStatus blankPaymentStatus;
    private Common checkGv2;
    private Common checkGv1;
    private PaymentStatusHistory blankPaymentStatusHistory;
    private NewWorkSet blankNewWorkSet;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of WarrantPaymentStatus.
    /// </summary>
    [JsonPropertyName("warrantPaymentStatus")]
    public PaymentStatus WarrantPaymentStatus
    {
      get => warrantPaymentStatus ??= new();
      set => warrantPaymentStatus = value;
    }

    /// <summary>
    /// A value of PotentialRecoveryPaymentStatus.
    /// </summary>
    [JsonPropertyName("potentialRecoveryPaymentStatus")]
    public PaymentStatus PotentialRecoveryPaymentStatus
    {
      get => potentialRecoveryPaymentStatus ??= new();
      set => potentialRecoveryPaymentStatus = value;
    }

    /// <summary>
    /// A value of WarrantPaymentRequest.
    /// </summary>
    [JsonPropertyName("warrantPaymentRequest")]
    public PaymentRequest WarrantPaymentRequest
    {
      get => warrantPaymentRequest ??= new();
      set => warrantPaymentRequest = value;
    }

    /// <summary>
    /// A value of PotentialRecoveryPaymentRequest.
    /// </summary>
    [JsonPropertyName("potentialRecoveryPaymentRequest")]
    public PaymentRequest PotentialRecoveryPaymentRequest
    {
      get => potentialRecoveryPaymentRequest ??= new();
      set => potentialRecoveryPaymentRequest = value;
    }

    private ObligationType obligationType;
    private Obligation obligation;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus warrantPaymentStatus;
    private PaymentStatus potentialRecoveryPaymentStatus;
    private PaymentRequest warrantPaymentRequest;
    private PaymentRequest potentialRecoveryPaymentRequest;
  }
#endregion
}
