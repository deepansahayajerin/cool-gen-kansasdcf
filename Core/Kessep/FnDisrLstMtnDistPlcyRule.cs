// Program: FN_DISR_LST_MTN_DIST_PLCY_RULE, ID: 371961726, model: 746.
// Short name: SWEDISRP
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
/// A program: FN_DISR_LST_MTN_DIST_PLCY_RULE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This procedure lists Distribution Policy Rules related to a Distribution 
/// Policy.  It also allows update, delete, create and move functions, if the
/// Distribution Policy is not active.  The move function allows the sequencing
/// of the Distribution Policy Rules to be altered.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDisrLstMtnDistPlcyRule: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISR_LST_MTN_DIST_PLCY_RULE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisrLstMtnDistPlcyRule(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisrLstMtnDistPlcyRule.
  /// </summary>
  public FnDisrLstMtnDistPlcyRule(IContext context, Import import, Export export)
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
    // *******************************************************************
    // CHANGE LOG
    // Date           Name          reference     Description
    // -------------------------------------------------------------------
    // 04/18/97    H. Kennedy - MTW               Changed Process
    //                                            
    // Command making it
    //                                            
    // 3 command of ADD
    //                                            
    // Update and delete.
    //                                            
    // Changed prompt fields
    //                                            
    // from ief supplied flag
    //                                            
    // to select char in order
    //                                            
    // to avoid permitted value
    //                                            
    // error message
    // 04/24/97    H. Kennedy - MTW               Changed Add
    //                                            
    // Update and delete logic
    //                                            
    // to process with the new
    //                                            
    // DPR Entities, added to
    //                                            
    // resolve many to many
    //                                            
    // relationships
    // 04/28/97     Ty Hill - MTW                 Change Current_date
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // : Move all IMPORTs to EXPORTs.
    export.SearchCollectionType.Assign(import.SearchCollectionType);
    MoveCollectionType(import.HiddenPreviousCollectionType,
      export.HiddenPreviousCollectionType);
    export.DistPlcyPrompt.SelectChar = import.DistPlcyPrompt.SelectChar;
    export.SearchDistributionPolicy.Assign(import.SearchDistributionPolicy);
    export.HiddenPreviousDistributionPolicy.Assign(
      import.HiddenPreviousDistributionPolicy);
    export.SourceTypePrompt.SelectChar = import.SourceTypePrompt.SelectChar;
    MoveCommon(import.BeforeFlow, export.BeforeFlow);
    export.HiddenSelection.Assign(import.HiddenSelection);
    export.Returned.SupportedPersonReqInd =
      import.Returned.SupportedPersonReqInd;

    if (Equal(global.Command, "LIST") || Equal(global.Command, "RTLIST") || Equal
      (global.Command, "EXIT"))
    {
    }
    else if (Equal(import.SearchDistributionPolicy.DiscontinueDt,
      local.Blank.Date))
    {
      export.SearchDistributionPolicy.DiscontinueDt = local.Max.Date;
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

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      export.Export1.Update.DistributionPolicyRule.Assign(
        import.Import1.Item.DistributionPolicyRule);
      export.Export1.Update.Apply.TextLine10 =
        import.Import1.Item.ApplyTo.TextLine10;
      export.Export1.Update.DebtState.TextLine10 =
        import.Import1.Item.DebtState.TextLine10;
      export.Export1.Update.FunctionType.Text13 =
        import.Import1.Item.FunctionType.Text13;

      switch(AsChar(export.Export1.Item.Common.SelectChar))
      {
        case '*':
          export.Export1.Update.Common.SelectChar = "";

          break;
        case ' ':
          break;
        case 'S':
          break;
        default:
          ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          break;
      }

      // : ** If one of the literals is blank, call AB to set all literals.
      if (IsEmpty(import.Import1.Item.ApplyTo.TextLine10))
      {
        UseFnSetDprFieldLiterals();
      }

      export.Export1.Next();
    }

    export.ReturnedOt.Index = 0;
    export.ReturnedOt.Clear();

    for(import.ReturnedOt.Index = 0; import.ReturnedOt.Index < import
      .ReturnedOt.Count; ++import.ReturnedOt.Index)
    {
      if (export.ReturnedOt.IsFull)
      {
        break;
      }

      MoveObligationType(import.ReturnedOt.Item.Returned,
        export.ReturnedOt.Update.Returned);
      export.ReturnedOt.Next();
    }

    export.ReturnedPgm.Index = 0;
    export.ReturnedPgm.Clear();

    for(import.ReturnedPgm.Index = 0; import.ReturnedPgm.Index < import
      .ReturnedPgm.Count; ++import.ReturnedPgm.Index)
    {
      if (export.ReturnedPgm.IsFull)
      {
        break;
      }

      export.ReturnedPgm.Update.Returned.SystemGeneratedIdentifier =
        import.ReturnedPgm.Item.Returned.SystemGeneratedIdentifier;
      export.ReturnedPgm.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      global.Command = "BYPASS";
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
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
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (Equal(global.Command, "OBRL") || Equal(global.Command, "PGRL") || Equal
      (global.Command, "BYPASS"))
    {
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

    if (!Equal(global.Command, "FIRSTIME") && !
      Equal(global.Command, "LIST") && !Equal(global.Command, "RTLIST") && !
      Equal(global.Command, "EXIT") && !Equal(global.Command, "RETURN"))
    {
      if (Equal(import.SearchDistributionPolicy.EffectiveDt, local.Blank.Date))
      {
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        var field = GetField(export.SearchDistributionPolicy, "effectiveDt");

        field.Error = true;

        return;
      }

      if (IsEmpty(import.SearchCollectionType.Code))
      {
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        var field = GetField(export.SearchCollectionType, "code");

        field.Error = true;

        return;
      }
    }

    // : Check key fields - if changed, validate.
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "FIRSTIME"))
    {
      if ((!Equal(
        export.SearchDistributionPolicy.DiscontinueDt,
        export.HiddenPreviousDistributionPolicy.DiscontinueDt) || !
        Equal(export.SearchDistributionPolicy.EffectiveDt,
        export.HiddenPreviousDistributionPolicy.EffectiveDt) || !
        Equal(import.SearchCollectionType.Code,
        export.HiddenPreviousCollectionType.Code)) && !
        Equal(global.Command, "FIRSTIME"))
      {
        // : If command is add, update or delete, key fields may not be changed.
        switch(TrimEnd(global.Command))
        {
          case "ADD":
            ExitState = "FN0000_NEW_KEY_ON_ADD";

            var field1 = GetField(export.SearchCollectionType, "code");

            field1.Error = true;

            var field2 =
              GetField(export.SearchDistributionPolicy, "discontinueDt");

            field2.Error = true;

            var field3 =
              GetField(export.SearchDistributionPolicy, "effectiveDt");

            field3.Error = true;

            return;
          case "UPDATE":
            ExitState = "ACO_NE0000_NEW_KEY_ON_UPDATE";

            var field4 = GetField(export.SearchCollectionType, "code");

            field4.Error = true;

            var field5 =
              GetField(export.SearchDistributionPolicy, "discontinueDt");

            field5.Error = true;

            var field6 =
              GetField(export.SearchDistributionPolicy, "effectiveDt");

            field6.Error = true;

            return;
          case "DELETE":
            ExitState = "ACO_NE0000_NEW_KEY_ON_DELETE";

            var field7 = GetField(export.SearchCollectionType, "code");

            field7.Error = true;

            var field8 =
              GetField(export.SearchDistributionPolicy, "discontinueDt");

            field8.Error = true;

            var field9 =
              GetField(export.SearchDistributionPolicy, "effectiveDt");

            field9.Error = true;

            return;
          default:
            break;
        }

        if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
          (global.Command, "DELETE"))
        {
        }
        else
        {
          // : Read new Distribution Policy and Collection Type.
          if (!Equal(export.SearchDistributionPolicy.EffectiveDt,
            local.Blank.Date))
          {
            if (ReadDistributionPolicyCollectionType2())
            {
              export.HiddenPreviousDistributionPolicy.Assign(
                entities.DistributionPolicy);
              export.SearchDistributionPolicy.
                Assign(entities.DistributionPolicy);
              export.SearchCollectionType.Assign(entities.CollectionType);
              MoveCollectionType(entities.CollectionType,
                export.HiddenPreviousCollectionType);
            }
            else
            {
              ExitState = "FN0000_DIST_PLCY_NF";

              return;
            }
          }
          else if (ReadDistributionPolicyCollectionType1())
          {
            export.HiddenPreviousDistributionPolicy.Assign(
              entities.DistributionPolicy);
            export.SearchDistributionPolicy.Assign(entities.DistributionPolicy);
            export.SearchCollectionType.Assign(entities.CollectionType);
            MoveCollectionType(entities.CollectionType,
              export.HiddenPreviousCollectionType);
          }
          else
          {
            ExitState = "FN0000_DIST_PLCY_NF";

            return;
          }
        }
      }
      else
      {
        // : No keys have been changed, or this is FIRSTIME in - read using Sys 
        // Gen Id.
        if (ReadDistributionPolicy())
        {
          export.SearchDistributionPolicy.Assign(entities.DistributionPolicy);
          export.HiddenPreviousDistributionPolicy.Assign(
            entities.DistributionPolicy);

          if (ReadCollectionType())
          {
            export.SearchCollectionType.Assign(entities.CollectionType);
            MoveCollectionType(entities.CollectionType,
              export.HiddenPreviousCollectionType);
          }
        }
        else
        {
          ExitState = "FN0000_DIST_PLCY_NF";

          return;
        }
      }

      if (Equal(global.Command, "FIRSTIME"))
      {
        global.Command = "DISPLAY";
      }

      // ** Check to see if the Distribution Policy is in effect.
      if (!Equal(entities.DistributionPolicy.MaximumProcessedDt,
        local.Blank.Date) || Lt
        (entities.DistributionPolicy.EffectiveDt, local.Current.Date))
      {
        local.Active.Flag = "Y";

        if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
          (global.Command, "DELETE"))
        {
          ExitState = "FN0000_DIST_PLCY_ACT_NO_UPDATES";
        }
      }

      if (Equal(export.SearchDistributionPolicy.DiscontinueDt, local.Max.Date))
      {
        export.SearchDistributionPolicy.DiscontinueDt = local.Blank.Date;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      // ** Check to see if the Distribution Policy is in effect.
      if (!Equal(entities.DistributionPolicy.MaximumProcessedDt,
        local.Blank.Date) || Lt
        (entities.DistributionPolicy.EffectiveDt, local.Current.Date))
      {
        local.Active.Flag = "Y";

        if (Equal(global.Command, "PROCESS"))
        {
          ExitState = "FN0000_DIST_PLCY_ACT_NO_UPDATES";

          return;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          return;
        }
      }
    }

    // : VALIDATION CASE OF COMMAND
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case '*':
              break;
            case 'S':
              if (export.Export1.Item.DistributionPolicyRule.
                SystemGeneratedIdentifier != 0)
              {
                ExitState = "FN0000_DPR_CREATE_NON_BLANK_LINE";

                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;

                continue;
              }

              // : If action Add, make sure all mandatory fields have been 
              // entered.
              if (IsEmpty(export.Export1.Item.DistributionPolicyRule.ApplyTo))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                var field1 =
                  GetField(export.Export1.Item.DistributionPolicyRule, "applyTo");
                  

                field1.Error = true;
              }

              // --- Debt Function Type can be spaces. If spaces, it applies to 
              // all (accruing, non accruing, recovery ... debts)
              if (IsEmpty(export.Export1.Item.DistributionPolicyRule.DebtState))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                var field1 =
                  GetField(export.Export1.Item.DistributionPolicyRule,
                  "debtState");

                field1.Error = true;
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                continue;
              }

              break;
            default:
              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        break;
      case "UPDATE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case '*':
              break;
            case 'S':
              if (export.Export1.Item.DistributionPolicyRule.
                SystemGeneratedIdentifier == 0)
              {
                ExitState = "FN0000_ACTION_ON_BLANK_LINE";

                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;

                continue;
              }

              // : If action is "C"reate or "U"pdate, make sure all mandatory 
              // fields have been entered.
              if (IsEmpty(export.Export1.Item.DistributionPolicyRule.ApplyTo))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                var field1 =
                  GetField(export.Export1.Item.DistributionPolicyRule, "applyTo");
                  

                field1.Error = true;
              }

              // --- Debt Function Type can be spaces. If spaces, it applies to 
              // all (accruing, non accruing, recovery ... debts)
              if (IsEmpty(export.Export1.Item.DistributionPolicyRule.DebtState))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                var field1 =
                  GetField(export.Export1.Item.DistributionPolicyRule,
                  "debtState");

                field1.Error = true;
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                continue;
              }

              break;
            default:
              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        break;
      case "DELETE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case '*':
              break;
            case 'S':
              if (export.Export1.Item.DistributionPolicyRule.
                SystemGeneratedIdentifier == 0)
              {
                ExitState = "FN0000_ACTION_ON_BLANK_LINE";

                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;

                continue;
              }

              break;
            default:
              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        break;
      case "OBRL":
        // : Flow to List Obligation Types related to D.P.R.
        local.Select.Count = 0;

        // Check to see if a selection has been made.
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            local.Select.Count = (int)((long)local.Select.Count + 1);

            if (local.Select.Count > 1)
            {
              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              break;
            }
          }

          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              export.HiddenSelection.Assign(
                export.Export1.Item.DistributionPolicyRule);
              export.Export1.Update.Common.SelectChar = "";

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_ACTION";

              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              return;
          }
        }

        if (local.Select.Count == 1)
        {
          ExitState = "ECO_LNK_TO_LST_MTN_OBLG_TYPE_RLN";

          return;
        }
        else if (local.Select.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.Common.SelectChar))
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              return;
            }
          }
        }

        ExitState = "ZD_ACO_NE0000_NO_SELECTN_MADE_2";

        break;
      case "PGRL":
        // : Flow to List Programs related to D.P.R.
        // Check to see if a selection has been made.
        local.Select.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            local.Select.Count = (int)((long)local.Select.Count + 1);

            if (local.Select.Count > 1)
            {
              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              break;
            }
          }

          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              if (local.Select.Count == 1)
              {
                export.HiddenSelection.Assign(
                  export.Export1.Item.DistributionPolicyRule);
                export.Export1.Update.Common.SelectChar = "";
                ExitState = "ECO_LNK_TO_LST_PROGRAM_RLNS";
              }

              break;
            default:
              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              return;
          }
        }

        if (IsExitState("ECO_LNK_TO_LST_PROGRAM_RLNS"))
        {
          return;
        }
        else if (IsExitState("ACO_NE0000_INVALID_MULTIPLE_SEL"))
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!IsEmpty(export.Export1.Item.Common.SelectChar))
            {
              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              return;
            }
          }
        }

        ExitState = "ZD_ACO_NE0000_NO_SELECTN_MADE_2";

        break;
      default:
        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      global.Command = "BYPASS";
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.DistPlcyPrompt.SelectChar = "";
        UseFnDisplayDistPolicyRules();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "LIST":
        // : Prompts
        switch(AsChar(import.DistPlcyPrompt.SelectChar))
        {
          case 'S':
            ExitState = "ECO_LNK_LST_DIST_POLICY";

            return;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.DistPlcyPrompt, "selectChar");

            field.Error = true;

            return;
        }

        switch(AsChar(export.SourceTypePrompt.SelectChar))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_LST_COLLECTION_TYPE";

            return;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.SourceTypePrompt, "selectChar");

            field.Error = true;

            return;
        }

        // :  If logic comes here, none of the prompt fields were selected.
        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        return;
      case "ADD":
        // : Create Distribution Policy Rule.
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              UseFnCreateDistribPolicyRule();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
                export.Export1.Update.Common.SelectChar = "*";
                global.Command = "ADD";
              }
              else if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
              {
                export.Export1.Update.Common.SelectChar = "*";
                global.Command = "ADD";
              }
              else
              {
              }

              break;
            default:
              // Validated in first case of command
              break;
          }
        }

        break;
      case "UPDATE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              UseFnUpdateDistribPolicyRule();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Export1.Update.Common.SelectChar = "*";
                ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
              }
              else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
                export.Export1.Update.Common.SelectChar = "*";
                global.Command = "UPDATE";
              }
              else
              {
                return;
              }

              break;
            default:
              // Validated in first case of command
              break;
          }
        }

        break;
      case "DELETE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              UseFnDeleteDistribPolicyRule();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_2";
                export.Export1.Update.Common.SelectChar = "*";
                export.Export1.Update.Apply.TextLine10 = "**********";
                export.Export1.Update.FunctionType.Text13 = " ***********";
                export.Export1.Update.DebtState.TextLine10 = "DELETED";

                var field1 =
                  GetField(export.Export1.Item.DistributionPolicyRule, "applyTo");
                  

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 =
                  GetField(export.Export1.Item.DistributionPolicyRule,
                  "debtFunctionType");

                field2.Color = "cyan";
                field2.Protected = true;

                var field3 =
                  GetField(export.Export1.Item.DistributionPolicyRule,
                  "debtState");

                field3.Color = "cyan";
                field3.Protected = true;

                export.Export1.Update.DistributionPolicyRule.
                  SystemGeneratedIdentifier = 0;
              }
              else if (IsExitState("ZD_ACO_NI0000_SUCCESSFUL_DEL_2"))
              {
                export.Export1.Update.Common.SelectChar = "*";
                export.Export1.Update.Apply.TextLine10 = "**********";
                export.Export1.Update.FunctionType.Text13 = " ***********";
                export.Export1.Update.DebtState.TextLine10 = "DELETED";

                var field1 =
                  GetField(export.Export1.Item.DistributionPolicyRule, "applyTo");
                  

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 =
                  GetField(export.Export1.Item.DistributionPolicyRule,
                  "debtFunctionType");

                field2.Color = "cyan";
                field2.Protected = true;

                var field3 =
                  GetField(export.Export1.Item.DistributionPolicyRule,
                  "debtState");

                field3.Color = "cyan";
                field3.Protected = true;

                export.Export1.Update.DistributionPolicyRule.
                  SystemGeneratedIdentifier = 0;
              }
              else
              {
                return;
              }

              break;
            default:
              // Validated in first case of command
              break;
          }
        }

        break;
      case "OBRL":
        // : This has already been handled.
        break;
      case "PGRL":
        // : This has already been handled.
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ZD_ACO_NE0000_INVALID_BACKWARD_2";

        break;
      case "BYPASS":
        break;
      case "RTLIST":
        export.SourceTypePrompt.SelectChar = "";

        if (Equal(export.SearchDistributionPolicy.DiscontinueDt, local.Max.Date))
          
        {
          export.SearchDistributionPolicy.DiscontinueDt = local.Blank.Date;
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // : Add any common logic that must occur at
    // the end of every pass.
    if (AsChar(local.Active.Flag) == 'Y')
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        var field1 =
          GetField(export.Export1.Item.DistributionPolicyRule, "applyTo");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Export1.Item.DistributionPolicyRule,
          "debtFunctionType");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Export1.Item.DistributionPolicyRule, "debtState");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Export1.Item.DistributionPolicyRule,
          "distributeToOrderTypeCode");

        field4.Color = "cyan";
        field4.Protected = true;
      }
    }

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      var field1 = GetField(export.Export1.Item.Apply, "textLine10");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Export1.Item.DebtState, "textLine10");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.Export1.Item.FunctionType, "text13");

      field3.Color = "cyan";
      field3.Protected = true;
    }
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Command = source.Command;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveDistributionPolicy(DistributionPolicy source,
    DistributionPolicy target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Name = source.Name;
    target.EffectiveDt = source.EffectiveDt;
    target.DiscontinueDt = source.DiscontinueDt;
    target.MaximumProcessedDt = source.MaximumProcessedDt;
  }

  private static void MoveDistributionPolicyRule(DistributionPolicyRule source,
    DistributionPolicyRule target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DebtFunctionType = source.DebtFunctionType;
    target.DebtState = source.DebtState;
    target.ApplyTo = source.ApplyTo;
    target.DistributeToOrderTypeCode = source.DistributeToOrderTypeCode;
  }

  private static void MoveExport1(FnDisplayDistPolicyRules.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.DistributionPolicyRule.Assign(source.DistributionPolicyRule);
    target.FunctionType.Text13 = source.FunctionType.Text13;
    target.DebtState.TextLine10 = source.DebtState.TextLine10;
    target.Apply.TextLine10 = source.Apply.TextLine10;
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

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveReturnedOt(Import.ReturnedOtGroup source,
    FnCreateDistribPolicyRule.Import.OtGroup target)
  {
    target.ObligationType.SystemGeneratedIdentifier =
      source.Returned.SystemGeneratedIdentifier;
  }

  private static void MoveReturnedPgmToProgram(Import.ReturnedPgmGroup source,
    FnCreateDistribPolicyRule.Import.ProgramGroup target)
  {
    target.Program1.SystemGeneratedIdentifier =
      source.Returned.SystemGeneratedIdentifier;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCreateDistribPolicyRule()
  {
    var useImport = new FnCreateDistribPolicyRule.Import();
    var useExport = new FnCreateDistribPolicyRule.Export();

    useImport.DistributionPolicy.SystemGeneratedIdentifier =
      export.SearchDistributionPolicy.SystemGeneratedIdentifier;
    MoveDistributionPolicyRule(export.Export1.Item.DistributionPolicyRule,
      useImport.DistributionPolicyRule);
    import.ReturnedOt.CopyTo(useImport.Ot, MoveReturnedOt);
    import.ReturnedPgm.CopyTo(useImport.Program, MoveReturnedPgmToProgram);

    Call(FnCreateDistribPolicyRule.Execute, useImport, useExport);

    export.Export1.Update.DistributionPolicyRule.SystemGeneratedIdentifier =
      useExport.DistributionPolicyRule.SystemGeneratedIdentifier;
  }

  private void UseFnDeleteDistribPolicyRule()
  {
    var useImport = new FnDeleteDistribPolicyRule.Import();
    var useExport = new FnDeleteDistribPolicyRule.Export();

    useImport.DistributionPolicy.SystemGeneratedIdentifier =
      export.SearchDistributionPolicy.SystemGeneratedIdentifier;
    useImport.DistributionPolicyRule.SystemGeneratedIdentifier =
      export.Export1.Item.DistributionPolicyRule.SystemGeneratedIdentifier;
    useImport.Persistent.Assign(entities.DistributionPolicy);

    Call(FnDeleteDistribPolicyRule.Execute, useImport, useExport);

    MoveDistributionPolicy(useImport.Persistent, entities.DistributionPolicy);
  }

  private void UseFnDisplayDistPolicyRules()
  {
    var useImport = new FnDisplayDistPolicyRules.Import();
    var useExport = new FnDisplayDistPolicyRules.Export();

    useImport.Persistent.Assign(entities.DistributionPolicy);
    useImport.DistributionPolicy.SystemGeneratedIdentifier =
      export.SearchDistributionPolicy.SystemGeneratedIdentifier;

    Call(FnDisplayDistPolicyRules.Execute, useImport, useExport);

    MoveDistributionPolicy(useImport.Persistent, entities.DistributionPolicy);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseFnSetDprFieldLiterals()
  {
    var useImport = new FnSetDprFieldLiterals.Import();
    var useExport = new FnSetDprFieldLiterals.Export();

    useImport.DistributionPolicyRule.Assign(
      import.Import1.Item.DistributionPolicyRule);

    Call(FnSetDprFieldLiterals.Execute, useImport, useExport);

    export.Export1.Update.FunctionType.Text13 = useExport.Function.Text13;
    export.Export1.Update.DebtState.TextLine10 = useExport.DebtState.TextLine10;
    export.Export1.Update.Apply.TextLine10 = useExport.Apply.TextLine10;
  }

  private void UseFnUpdateDistribPolicyRule()
  {
    var useImport = new FnUpdateDistribPolicyRule.Import();
    var useExport = new FnUpdateDistribPolicyRule.Export();

    useImport.DistributionPolicy.SystemGeneratedIdentifier =
      export.SearchDistributionPolicy.SystemGeneratedIdentifier;
    MoveDistributionPolicyRule(export.Export1.Item.DistributionPolicyRule,
      useImport.DistributionPolicyRule);
    useImport.Persistent.Assign(entities.DistributionPolicy);

    Call(FnUpdateDistribPolicyRule.Execute, useImport, useExport);

    MoveDistributionPolicy(useImport.Persistent, entities.DistributionPolicy);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicy.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.DistributionPolicy.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadDistributionPolicy()
  {
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyId",
          import.SearchDistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.DistributionPolicy.Populated = true;
      });
  }

  private bool ReadDistributionPolicyCollectionType1()
  {
    entities.CollectionType.Populated = false;
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicyCollectionType1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "code", import.SearchCollectionType.Code);
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 5);
        entities.CollectionType.Code = db.GetString(reader, 6);
        entities.CollectionType.Name = db.GetString(reader, 7);
        entities.CollectionType.Populated = true;
        entities.DistributionPolicy.Populated = true;
      });
  }

  private bool ReadDistributionPolicyCollectionType2()
  {
    entities.CollectionType.Populated = false;
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicyCollectionType2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          export.SearchDistributionPolicy.EffectiveDt.GetValueOrDefault());
        db.SetString(command, "code", import.SearchCollectionType.Code);
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 5);
        entities.CollectionType.Code = db.GetString(reader, 6);
        entities.CollectionType.Name = db.GetString(reader, 7);
        entities.CollectionType.Populated = true;
        entities.DistributionPolicy.Populated = true;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of DistributionPolicyRule.
      /// </summary>
      [JsonPropertyName("distributionPolicyRule")]
      public DistributionPolicyRule DistributionPolicyRule
      {
        get => distributionPolicyRule ??= new();
        set => distributionPolicyRule = value;
      }

      /// <summary>
      /// A value of FunctionType.
      /// </summary>
      [JsonPropertyName("functionType")]
      public WorkArea FunctionType
      {
        get => functionType ??= new();
        set => functionType = value;
      }

      /// <summary>
      /// A value of DebtState.
      /// </summary>
      [JsonPropertyName("debtState")]
      public ListScreenWorkArea DebtState
      {
        get => debtState ??= new();
        set => debtState = value;
      }

      /// <summary>
      /// A value of ApplyTo.
      /// </summary>
      [JsonPropertyName("applyTo")]
      public ListScreenWorkArea ApplyTo
      {
        get => applyTo ??= new();
        set => applyTo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private DistributionPolicyRule distributionPolicyRule;
      private WorkArea functionType;
      private ListScreenWorkArea debtState;
      private ListScreenWorkArea applyTo;
    }

    /// <summary>A ReturnedOtGroup group.</summary>
    [Serializable]
    public class ReturnedOtGroup
    {
      /// <summary>
      /// A value of Returned.
      /// </summary>
      [JsonPropertyName("returned")]
      public ObligationType Returned
      {
        get => returned ??= new();
        set => returned = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType returned;
    }

    /// <summary>A ReturnedPgmGroup group.</summary>
    [Serializable]
    public class ReturnedPgmGroup
    {
      /// <summary>
      /// A value of Returned.
      /// </summary>
      [JsonPropertyName("returned")]
      public Program Returned
      {
        get => returned ??= new();
        set => returned = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program returned;
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
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public ObligationType Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of SearchDistributionPolicy.
    /// </summary>
    [JsonPropertyName("searchDistributionPolicy")]
    public DistributionPolicy SearchDistributionPolicy
    {
      get => searchDistributionPolicy ??= new();
      set => searchDistributionPolicy = value;
    }

    /// <summary>
    /// A value of HiddenPreviousDistributionPolicy.
    /// </summary>
    [JsonPropertyName("hiddenPreviousDistributionPolicy")]
    public DistributionPolicy HiddenPreviousDistributionPolicy
    {
      get => hiddenPreviousDistributionPolicy ??= new();
      set => hiddenPreviousDistributionPolicy = value;
    }

    /// <summary>
    /// A value of SearchCollectionType.
    /// </summary>
    [JsonPropertyName("searchCollectionType")]
    public CollectionType SearchCollectionType
    {
      get => searchCollectionType ??= new();
      set => searchCollectionType = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCollectionType.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCollectionType")]
    public CollectionType HiddenPreviousCollectionType
    {
      get => hiddenPreviousCollectionType ??= new();
      set => hiddenPreviousCollectionType = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of SourceTypePrompt.
    /// </summary>
    [JsonPropertyName("sourceTypePrompt")]
    public Common SourceTypePrompt
    {
      get => sourceTypePrompt ??= new();
      set => sourceTypePrompt = value;
    }

    /// <summary>
    /// A value of DistPlcyPrompt.
    /// </summary>
    [JsonPropertyName("distPlcyPrompt")]
    public Common DistPlcyPrompt
    {
      get => distPlcyPrompt ??= new();
      set => distPlcyPrompt = value;
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
    /// Gets a value of ReturnedOt.
    /// </summary>
    [JsonIgnore]
    public Array<ReturnedOtGroup> ReturnedOt => returnedOt ??= new(
      ReturnedOtGroup.Capacity);

    /// <summary>
    /// Gets a value of ReturnedOt for json serialization.
    /// </summary>
    [JsonPropertyName("returnedOt")]
    [Computed]
    public IList<ReturnedOtGroup> ReturnedOt_Json
    {
      get => returnedOt;
      set => ReturnedOt.Assign(value);
    }

    /// <summary>
    /// Gets a value of ReturnedPgm.
    /// </summary>
    [JsonIgnore]
    public Array<ReturnedPgmGroup> ReturnedPgm => returnedPgm ??= new(
      ReturnedPgmGroup.Capacity);

    /// <summary>
    /// Gets a value of ReturnedPgm for json serialization.
    /// </summary>
    [JsonPropertyName("returnedPgm")]
    [Computed]
    public IList<ReturnedPgmGroup> ReturnedPgm_Json
    {
      get => returnedPgm;
      set => ReturnedPgm.Assign(value);
    }

    /// <summary>
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public DistributionPolicyRule HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
    }

    /// <summary>
    /// A value of BeforeFlow.
    /// </summary>
    [JsonPropertyName("beforeFlow")]
    public Common BeforeFlow
    {
      get => beforeFlow ??= new();
      set => beforeFlow = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
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

    private Standard standard;
    private ObligationType returned;
    private DistributionPolicy searchDistributionPolicy;
    private DistributionPolicy hiddenPreviousDistributionPolicy;
    private CollectionType searchCollectionType;
    private CollectionType hiddenPreviousCollectionType;
    private Common nextTransaction;
    private Common sourceTypePrompt;
    private Common distPlcyPrompt;
    private Array<ImportGroup> import1;
    private Array<ReturnedOtGroup> returnedOt;
    private Array<ReturnedPgmGroup> returnedPgm;
    private DistributionPolicyRule hiddenSelection;
    private Common beforeFlow;
    private Common previous;
    private NextTranInfo hidden;
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
      /// A value of DistributionPolicyRule.
      /// </summary>
      [JsonPropertyName("distributionPolicyRule")]
      public DistributionPolicyRule DistributionPolicyRule
      {
        get => distributionPolicyRule ??= new();
        set => distributionPolicyRule = value;
      }

      /// <summary>
      /// A value of FunctionType.
      /// </summary>
      [JsonPropertyName("functionType")]
      public WorkArea FunctionType
      {
        get => functionType ??= new();
        set => functionType = value;
      }

      /// <summary>
      /// A value of DebtState.
      /// </summary>
      [JsonPropertyName("debtState")]
      public ListScreenWorkArea DebtState
      {
        get => debtState ??= new();
        set => debtState = value;
      }

      /// <summary>
      /// A value of Apply.
      /// </summary>
      [JsonPropertyName("apply")]
      public ListScreenWorkArea Apply
      {
        get => apply ??= new();
        set => apply = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private DistributionPolicyRule distributionPolicyRule;
      private WorkArea functionType;
      private ListScreenWorkArea debtState;
      private ListScreenWorkArea apply;
    }

    /// <summary>A ReturnedOtGroup group.</summary>
    [Serializable]
    public class ReturnedOtGroup
    {
      /// <summary>
      /// A value of Returned.
      /// </summary>
      [JsonPropertyName("returned")]
      public ObligationType Returned
      {
        get => returned ??= new();
        set => returned = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType returned;
    }

    /// <summary>A ReturnedPgmGroup group.</summary>
    [Serializable]
    public class ReturnedPgmGroup
    {
      /// <summary>
      /// A value of Returned.
      /// </summary>
      [JsonPropertyName("returned")]
      public Program Returned
      {
        get => returned ??= new();
        set => returned = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program returned;
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
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public ObligationType Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of SearchDistributionPolicy.
    /// </summary>
    [JsonPropertyName("searchDistributionPolicy")]
    public DistributionPolicy SearchDistributionPolicy
    {
      get => searchDistributionPolicy ??= new();
      set => searchDistributionPolicy = value;
    }

    /// <summary>
    /// A value of HiddenPreviousDistributionPolicy.
    /// </summary>
    [JsonPropertyName("hiddenPreviousDistributionPolicy")]
    public DistributionPolicy HiddenPreviousDistributionPolicy
    {
      get => hiddenPreviousDistributionPolicy ??= new();
      set => hiddenPreviousDistributionPolicy = value;
    }

    /// <summary>
    /// A value of SearchCollectionType.
    /// </summary>
    [JsonPropertyName("searchCollectionType")]
    public CollectionType SearchCollectionType
    {
      get => searchCollectionType ??= new();
      set => searchCollectionType = value;
    }

    /// <summary>
    /// A value of HiddenPreviousCollectionType.
    /// </summary>
    [JsonPropertyName("hiddenPreviousCollectionType")]
    public CollectionType HiddenPreviousCollectionType
    {
      get => hiddenPreviousCollectionType ??= new();
      set => hiddenPreviousCollectionType = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of SourceTypePrompt.
    /// </summary>
    [JsonPropertyName("sourceTypePrompt")]
    public Common SourceTypePrompt
    {
      get => sourceTypePrompt ??= new();
      set => sourceTypePrompt = value;
    }

    /// <summary>
    /// A value of DistPlcyPrompt.
    /// </summary>
    [JsonPropertyName("distPlcyPrompt")]
    public Common DistPlcyPrompt
    {
      get => distPlcyPrompt ??= new();
      set => distPlcyPrompt = value;
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
    /// Gets a value of ReturnedOt.
    /// </summary>
    [JsonIgnore]
    public Array<ReturnedOtGroup> ReturnedOt => returnedOt ??= new(
      ReturnedOtGroup.Capacity);

    /// <summary>
    /// Gets a value of ReturnedOt for json serialization.
    /// </summary>
    [JsonPropertyName("returnedOt")]
    [Computed]
    public IList<ReturnedOtGroup> ReturnedOt_Json
    {
      get => returnedOt;
      set => ReturnedOt.Assign(value);
    }

    /// <summary>
    /// Gets a value of ReturnedPgm.
    /// </summary>
    [JsonIgnore]
    public Array<ReturnedPgmGroup> ReturnedPgm => returnedPgm ??= new(
      ReturnedPgmGroup.Capacity);

    /// <summary>
    /// Gets a value of ReturnedPgm for json serialization.
    /// </summary>
    [JsonPropertyName("returnedPgm")]
    [Computed]
    public IList<ReturnedPgmGroup> ReturnedPgm_Json
    {
      get => returnedPgm;
      set => ReturnedPgm.Assign(value);
    }

    /// <summary>
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public DistributionPolicyRule HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of BeforeFlow.
    /// </summary>
    [JsonPropertyName("beforeFlow")]
    public Common BeforeFlow
    {
      get => beforeFlow ??= new();
      set => beforeFlow = value;
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

    private Standard standard;
    private ObligationType returned;
    private DistributionPolicy searchDistributionPolicy;
    private DistributionPolicy hiddenPreviousDistributionPolicy;
    private CollectionType searchCollectionType;
    private CollectionType hiddenPreviousCollectionType;
    private Common nextTransaction;
    private Common sourceTypePrompt;
    private Common distPlcyPrompt;
    private Array<ExportGroup> export1;
    private Array<ReturnedOtGroup> returnedOt;
    private Array<ReturnedPgmGroup> returnedPgm;
    private DistributionPolicyRule hiddenSelection;
    private Common previous;
    private Common beforeFlow;
    private NextTranInfo hidden;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of Asearch.
    /// </summary>
    [JsonPropertyName("asearch")]
    public Common Asearch
    {
      get => asearch ??= new();
      set => asearch = value;
    }

    /// <summary>
    /// A value of Msearch.
    /// </summary>
    [JsonPropertyName("msearch")]
    public Common Msearch
    {
      get => msearch ??= new();
      set => msearch = value;
    }

    private DateWorkArea current;
    private Common select;
    private DateWorkArea max;
    private DateWorkArea blank;
    private Common active;
    private Common asearch;
    private Common msearch;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    private Obligation obligation;
    private CollectionType collectionType;
    private DistributionPolicy distributionPolicy;
    private Program program;
    private ObligationType obligationType;
    private DistributionPolicyRule distributionPolicyRule;
  }
#endregion
}
