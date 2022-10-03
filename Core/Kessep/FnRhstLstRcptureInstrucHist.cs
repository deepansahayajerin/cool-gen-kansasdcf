// Program: FN_RHST_LST_RCPTURE_INSTRUC_HIST, ID: 372128409, model: 746.
// Short name: SWERHSTP
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
/// A program: FN_RHST_LST_RCPTURE_INSTRUC_HIST.
/// </para>
/// <para>
/// This procedure will list all of the recapture rules that have ever been set 
/// up for an Obligor. The list will be in chronological order by effective
/// date. Any one of these recapture rules can be selected and carried forward
/// to another screen.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRhstLstRcptureInstrucHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RHST_LST_RCPTURE_INSTRUC_HIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRhstLstRcptureInstrucHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRhstLstRcptureInstrucHist.
  /// </summary>
  public FnRhstLstRcptureInstrucHist(IContext context, Import import,
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
    // : Generate RCAPRPAY cover letter and repayment agreement when negotiated 
    // date is updated.
    // ---------------------------------------------
    // List Recapture Rules History
    // Date Created    Created by
    // 07/24/1995      Terry W. Cooley - MTW
    // Date Modified   Modified by
    // 03/19/1996      R.B.Mohapatra - MTW
    // 12/16/96	R. Marchman	Add new security/next tran
    // 01/19/98	R. Marchman	Added add and update capability for Recapture Rules.
    // ---------------------------------------------
    // Maureen Brown, November, 1998: Modified program to allow list processing 
    // of delete and update functions.  Added edits for delete and update
    // functions.  Added field protection based on effective/discontinue date.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    export.Hidden.Assign(import.Hidden);
    local.Active.Flag = "N";
    local.Current.Date = Now().Date;
    local.HighDate.Date = UseCabSetMaximumDiscontinueDate();
    local.LowDate.Date = new DateTime(1, 1, 1);
    MoveCsePerson(import.CsePerson, export.CsePerson);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.PromptCsePerson.SelectChar = import.PromptCsePerson.SelectChar;
    MoveCsePerson(import.Prev, export.Prev);

    // ***Check if it is a RETURN for PROMPT from the CSE_PERSON List
    if (Equal(global.Command, "RETCSENO"))
    {
      export.PromptCsePerson.SelectChar = "";

      if (!IsEmpty(import.CsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = import.CsePersonsWorkSet.Number;
      }

      global.Command = "DISPLAY";
    }

    local.LeftPadding.Text10 = export.CsePerson.Number;
    UseEabPadLeftWithZeros();
    export.CsePerson.Number = local.LeftPadding.Text10;

    if (!Equal(global.Command, "LIST"))
    {
      export.PromptCsePerson.SelectChar = "";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // ---------------------------------------------
      // If User selects PF2 (Display) there is no
      // need to move the import group views to the
      // output group views.
      // 08/08/95     tw cooley.
      // ---------------------------------------------
      // 		
    }
    else
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.ObligorRule.Assign(import.Group.Item.ObligorRule);
        export.Group.Update.Negotiated.Flag = import.Group.Item.Negotiated.Flag;
        MoveCommon(import.Group.Item.Select, export.Group.Update.Select);
        export.Group.Next();
      }
    }

    // : Validate next tran info if it is not spaces.
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumberObligor = export.CsePerson.Number;
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        // ***  Protect all fields except expire date if the rule is active (
        // start date less or equal current date).
        local.AfterTheFirst.Flag = "N";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          // *** The first entry must always be unprotected, but for any 
          // occurences after this, all fields except discontinue date are
          // protected if the start date is less or equal to current.
          if (Lt(export.Group.Item.ObligorRule.EffectiveDate, Now().Date) && AsChar
            (local.AfterTheFirst.Flag) == 'Y')
          {
            var field1 =
              GetField(export.Group.Item.ObligorRule, "effectiveDate");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Group.Item.ObligorRule, "negotiatedDate");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Group.Item.ObligorRule, "nonAdcArrearsAmount");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Group.Item.ObligorRule, "nonAdcArrearsMaxAmount");
              

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Group.Item.ObligorRule, "nonAdcArrearsPercentage");
              

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Group.Item.ObligorRule, "nonAdcCurrentAmount");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Group.Item.ObligorRule, "nonAdcCurrentMaxAmount");
              

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Group.Item.ObligorRule, "nonAdcCurrentPercentage");
              

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 =
              GetField(export.Group.Item.ObligorRule, "passthruAmount");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.Group.Item.ObligorRule, "passthruMaxAmount");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.Group.Item.ObligorRule, "passthruPercentage");

            field11.Color = "cyan";
            field11.Protected = true;

            // *** Also protect discontinue date and select character if 
            // discontinue date is less than current date.
            if (Lt(export.Group.Item.ObligorRule.DiscontinueDate, Now().Date) &&
              !
              Equal(export.Group.Item.ObligorRule.DiscontinueDate,
              local.InitializedToZero.Date))
            {
              var field12 = GetField(export.Group.Item.Select, "selectChar");

              field12.Color = "cyan";
              field12.Protected = true;

              var field13 =
                GetField(export.Group.Item.ObligorRule, "discontinueDate");

              field13.Color = "cyan";
              field13.Protected = true;
            }
          }

          local.AfterTheFirst.Flag = "Y";
        }
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

      if (IsEmpty(export.Hidden.CsePersonNumberObligor))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }
      else
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumberObligor ?? Spaces
          (10);
      }

      // *** If CSE Person number is spaces, escape.  Otherwise, format person 
      // number, and set command to display.
      if (IsEmpty(export.CsePerson.Number))
      {
        return;
      }
      else
      {
        local.LeftPadding.Text10 = export.CsePerson.Number;
        UseEabPadLeftWithZeros();
        export.CsePerson.Number = local.LeftPadding.Text10;
        global.Command = "DISPLAY";
      }
    }

    // *** End NEXT TRAN Validation
    // ***  Validate action level security
    if (Equal(global.Command, "RCAP") || Equal(global.Command, "ROHL") || Equal
      (global.Command, "ENTER"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // **** end   security ****
    // *** Check for person number of spaces, or a change in person number.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "ADD"))
    {
      if (IsEmpty(export.CsePerson.Number))
      {
        ExitState = "FN0000_OBLIGOR_NUMBER_REQUIRED";

        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        return;
      }
      else if (!Equal(export.CsePerson.Number, export.Prev.Number))
      {
        ExitState = "FN0000_CSE_NUMBER_CHANGED";

        return;
      }
    }

    // *** Add is only allowed on the first lines.  If there is a selection 
    // anywhere else, set a message.
    if (Equal(global.Command, "ADD") && IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.AddEdit.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        ++local.AddEdit.Count;

        // *** Check to see if "S" not entered on line 1
        if (AsChar(export.Group.Item.Select.SelectChar) != 'S' && local
          .AddEdit.Count == 1)
        {
          var field = GetField(export.Group.Item.Select, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          goto Test1;
        }

        // *** Check to see if "S" entered on lines beyond line 1
        if (!IsEmpty(export.Group.Item.Select.SelectChar) && local
          .AddEdit.Count > 1)
        {
          var field = GetField(export.Group.Item.Select, "selectChar");

          field.Error = true;

          ExitState = "FN0000_ADD_ON_FIRST_LINE_ONLY";
        }
      }
    }

Test1:

    // *** Validate selection character(s) for update and delete actions.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      IsExitState("ACO_NN0000_ALL_OK"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (IsEmpty(export.Group.Item.Select.SelectChar))
        {
        }
        else if (AsChar(export.Group.Item.Select.SelectChar) == 'S')
        {
          local.SelectFound.Flag = "Y";
        }
        else
        {
          var field = GetField(export.Group.Item.Select, "selectChar");

          field.Error = true;

          local.SelectFound.Flag = "Y";
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }
      }

      // *** Escape if no selection was made.
      if (AsChar(local.SelectFound.Flag) != 'Y')
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";
      }
    }

    // *** Perform update, add and delete edits for mandatory fields and attempt
    // to update active or discontinued fields on the first (unprotected)
    // entry.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      IsExitState("ACO_NN0000_ALL_OK"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.Select.SelectChar) == 'S')
        {
          if (ReadObligorRule())
          {
            // ** Continue **
          }
          else
          {
            var field = GetField(export.Group.Item.Select, "selectChar");

            field.Error = true;

            ExitState = "FN0000_OBLIGOR_RULE_NF";

            goto Test2;
          }

          if (Equal(global.Command, "DELETE"))
          {
            // *** A delete cannot be done if the rule is active.
            if (Lt(entities.Existing.EffectiveDate, Now().Date))
            {
              var field = GetField(export.Group.Item.Select, "selectChar");

              field.Error = true;

              ExitState = "FN0000_CANNOT_DEL_ACTIVE_RECAP";
            }

            continue;
          }

          // *** No updates are allowed for a discontinued recapture rule.
          if (Lt(entities.Existing.DiscontinueDate, Now().Date))
          {
            var field = GetField(export.Group.Item.Select, "selectChar");

            field.Error = true;

            export.Group.Update.ObligorRule.Assign(entities.Existing);
            ExitState = "FN0000_RECORD_IS_DISCONTINUED";

            continue;
          }

          if (Equal(export.Group.Item.ObligorRule.DiscontinueDate,
            local.InitializedToZero.Date))
          {
            export.Group.Update.ObligorRule.DiscontinueDate =
              local.HighDate.Date;
          }

          // *** Check to see if no updates have been made.
          if (Equal(export.Group.Item.ObligorRule.EffectiveDate,
            entities.Existing.EffectiveDate) && Equal
            (export.Group.Item.ObligorRule.NegotiatedDate,
            entities.Existing.NegotiatedDate) && Equal
            (export.Group.Item.ObligorRule.NonAdcArrearsAmount.
              GetValueOrDefault(), entities.Existing.NonAdcArrearsAmount) && Equal
            (export.Group.Item.ObligorRule.NonAdcArrearsMaxAmount.
              GetValueOrDefault(), entities.Existing.NonAdcArrearsMaxAmount) &&
            Equal
            (export.Group.Item.ObligorRule.NonAdcArrearsPercentage.
              GetValueOrDefault(), entities.Existing.NonAdcArrearsPercentage) &&
            Equal
            (export.Group.Item.ObligorRule.NonAdcCurrentAmount.
              GetValueOrDefault(), entities.Existing.NonAdcCurrentAmount) && Equal
            (export.Group.Item.ObligorRule.NonAdcCurrentMaxAmount.
              GetValueOrDefault(), entities.Existing.NonAdcCurrentMaxAmount) &&
            Equal
            (export.Group.Item.ObligorRule.NonAdcCurrentPercentage.
              GetValueOrDefault(), entities.Existing.NonAdcCurrentPercentage) &&
            Equal
            (export.Group.Item.ObligorRule.PassthruPercentage.
              GetValueOrDefault(), entities.Existing.PassthruPercentage) && Equal
            (export.Group.Item.ObligorRule.PassthruAmount.GetValueOrDefault(),
            entities.Existing.PassthruAmount) && Equal
            (export.Group.Item.ObligorRule.PassthruMaxAmount.
              GetValueOrDefault(), entities.Existing.PassthruMaxAmount))
          {
            local.FieldsChanged.Flag = "N";

            // : If nothing was changed on update, set a flag so that
            //  the RCAPRPAY letter is not generated.
            export.Group.Update.PrintRcaprpay.Flag = "N";
          }
          else
          {
            local.FieldsChanged.Flag = "Y";

            // : If anything other than discontinue date was changed,
            //   set a flag so that the RCAPRPAY letter is generated.
            export.Group.Update.PrintRcaprpay.Flag = "Y";
          }

          // *** Set separate flags to indicate whether or not discontinue date
          //     has changed.
          if (Equal(export.Group.Item.ObligorRule.DiscontinueDate,
            entities.Existing.DiscontinueDate) || Equal
            (export.Group.Item.ObligorRule.DiscontinueDate,
            local.InitializedToZero.Date) && Equal
            (entities.Existing.DiscontinueDate, local.HighDate.Date))
          {
            local.ExpDateChanged.Flag = "N";
          }
          else
          {
            local.ExpDateChanged.Flag = "Y";
          }

          // *** Escape if no changes have been entered.
          if (AsChar(local.FieldsChanged.Flag) == 'N' && AsChar
            (local.ExpDateChanged.Flag) == 'N')
          {
            ExitState = "FN0000_NO_UPDATES_MADE";

            var field = GetField(export.Group.Item.Select, "selectChar");

            field.Error = true;

            continue;
          }

          // *** If the start date is in the past, only the discontinue date may
          // be updated.
          if (Lt(entities.Existing.EffectiveDate, Now().Date))
          {
            local.Active.Flag = "Y";

            if (AsChar(local.FieldsChanged.Flag) == 'Y')
            {
              // *** Something other than the discontinue date has been changed.
              // Set exit state and restore fields other than expiry date to
              // what they were.
              local.Temp.DiscontinueDate =
                export.Group.Item.ObligorRule.DiscontinueDate;
              export.Group.Update.ObligorRule.Assign(entities.Existing);
              export.Group.Update.ObligorRule.DiscontinueDate =
                local.Temp.DiscontinueDate;

              var field = GetField(export.Group.Item.Select, "selectChar");

              field.Error = true;

              ExitState = "FN0000_CAN_ONLY_UPDATE_EXP_DATE";

              goto Test2;
            }
          }
        }
      }
    }

Test2:

    // *** At this point, preliminary edits have been completed.  We do not 
    // escape if errors were found, because we need to perform field protection
    // logic at the end of the prad.   Now, for ADD and UPDATE,  pass thru the
    // group view a second time to edit dates and amounts, if exitstate is ok.
    if ((Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE")) && IsExitState
      ("ACO_NN0000_ALL_OK"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.Select.SelectChar) == 'S')
        {
          // *** Effective Date must be entered ***
          if (Equal(export.Group.Item.ObligorRule.EffectiveDate,
            local.InitializedToZero.Date))
          {
            ++local.ErrorCount.Count;
            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            var field =
              GetField(export.Group.Item.ObligorRule, "effectiveDate");

            field.Error = true;

            continue;
          }

          // *** Negotiated Date cannot be updated to spaces if anything besides
          // discontinue date has been changed.  If command is add, this field
          // is mandatory always. ***
          if (Equal(export.Group.Item.ObligorRule.NegotiatedDate,
            local.InitializedToZero.Date) || Equal
            (export.Group.Item.ObligorRule.NegotiatedDate, local.LowDate.Date))
          {
            if (AsChar(local.FieldsChanged.Flag) == 'Y' || Equal
              (global.Command, "ADD"))
            {
              var field =
                GetField(export.Group.Item.ObligorRule, "negotiatedDate");

              field.Error = true;

              ++local.ErrorCount.Count;
              ExitState = "FN0000_NEGOTIATED_DATE_ZERO";
            }
          }

          // *** Negotiated Date cannot be greater than current date ***
          if (Lt(local.Current.Date,
            export.Group.Item.ObligorRule.NegotiatedDate))
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "negotiatedDate");

            field.Error = true;

            ++local.ErrorCount.Count;
            ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
          }

          if (Equal(export.Group.Item.ObligorRule.DiscontinueDate,
            local.InitializedToZero.Date))
          {
            export.Group.Update.ObligorRule.DiscontinueDate =
              local.HighDate.Date;
          }

          UseFnCompareRecaptureDates();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(local.EffectiveDateError.Flag) == 'Y')
            {
              var field =
                GetField(export.Group.Item.ObligorRule, "effectiveDate");

              field.Error = true;
            }

            if (AsChar(local.ExpireDateError.Flag) == 'Y')
            {
              var field =
                GetField(export.Group.Item.ObligorRule, "discontinueDate");

              field.Error = true;
            }
          }

          // *** Check percent / max amount / amount entries for each type.  If 
          // max is entered, percent must also be entered.  If amount is
          // entered, percent and max must not be entered.
          // *** If the rule is active, don't bother doing the amount edits, as 
          // none of these can be updated  for an active record.
          if (Lt(export.Group.Item.ObligorRule.EffectiveDate, Now().Date))
          {
            continue;
          }

          UseFnCabCheckRecapInstrucAmts();

          // *** Check error flags and set screen field attributes accordingly.
          if (AsChar(local.NadcArrearsAmtErr.Flag) == 'Y')
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "nonAdcArrearsAmount");

            field.Error = true;
          }

          if (AsChar(local.NadcArrearsMaxErr.Flag) == 'Y')
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "nonAdcArrearsMaxAmount");
              

            field.Error = true;
          }

          if (AsChar(local.NadcArrearsPerErr.Flag) == 'Y')
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "nonAdcArrearsPercentage");
              

            field.Error = true;
          }

          if (AsChar(local.NadcCurrAmtErr.Flag) == 'Y')
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "nonAdcCurrentAmount");

            field.Error = true;
          }

          if (AsChar(local.NadcCurrMaxErr.Flag) == 'Y')
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "nonAdcCurrentMaxAmount");
              

            field.Error = true;
          }

          if (AsChar(local.NadcCurrPerErr.Flag) == 'Y')
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "nonAdcCurrentPercentage");
              

            field.Error = true;
          }

          if (AsChar(local.PassthruAmtErr.Flag) == 'Y')
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "passthruAmount");

            field.Error = true;
          }

          if (AsChar(local.PassthruMaxErr.Flag) == 'Y')
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "passthruMaxAmount");

            field.Error = true;
          }

          if (AsChar(local.PassthruPerErr.Flag) == 'Y')
          {
            var field =
              GetField(export.Group.Item.ObligorRule, "passthruPercentage");

            field.Error = true;
          }
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else if (IsExitState("FN0000_MULTIPLE_ERRORS_FOUND"))
      {
      }
      else
      {
        if (local.ErrorCount.Count > 1)
        {
          ExitState = "FN0000_MULTIPLE_ERRORS_FOUND";
        }
      }

      // *** If command is ADD, edit only the first recapture rule fields.
      if (Equal(global.Command, "ADD"))
      {
      }
    }

    // *** Exit state not ok condition is checked within the case of command 
    // statement below, as we want to perform field protection logic at the end
    // of the prad.
    // *** Perform command processing
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        // **** If edit errors were found, escape to bottom of prad for field 
        // protection logic.
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (Equal(export.Group.Item.ObligorRule.DiscontinueDate,
            local.InitializedToZero.Date))
          {
            export.Group.Update.ObligorRule.DiscontinueDate =
              local.HighDate.Date;
          }

          UseFnCreateObligorRule();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test3;
          }

          // *************** Set RCAPRPAY document trigger ******************
          // mjr
          // ------------------------------------------------
          // 02/18/1999
          // Use this action block (find_outgoing_doc) if you need to check
          // for another document for some reason.
          // ------------------------------------------------------------
          local.Document.Name = "RCAPRPAY";
          local.Infrastructure.ReferenceDate = local.Current.Date;
          local.SpDocKey.KeyRecaptureRule =
            local.RecaptureRule.SystemGeneratedIdentifier;

          // mjr
          // ------------------------------------------------
          // 02/18/1999
          // Use this action block (create_document_infrastruct) to create a 
          // document trigger
          // ------------------------------------------------------------
          local.Infrastructure.SystemGeneratedIdentifier = 0;
          UseSpCreateDocumentInfrastruct();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            goto Test3;
          }

          // *************** End Set RCAPRPAY document trigger 
          // ******************
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseFnGrpReadObligorRecaptRules();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
          }
          else
          {
            UseEabRollbackCics();

            goto Test3;
          }

          // *** Create is done on first occurence only.
          goto Test3;
        }

        break;
      case "UPDATE":
        // **** IF edit errors were found, escape to bottom of prad for field 
        // protection logic.
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Select.SelectChar) == 'S')
          {
            UseFnUpdateRecaptureRuleObligor();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              goto Test3;
            }

            if (AsChar(export.Group.Item.PrintRcaprpay.Flag) == 'Y')
            {
              // *************** Set RCAPRPAY document trigger 
              // ******************
              // mjr
              // ------------------------------------------------
              // 02/18/1999
              // Use this action block (find_outgoing_doc) if you need to check
              // for another document for some reason.
              // ------------------------------------------------------------
              local.Document.Name = "RCAPRPAY";
              local.Infrastructure.ReferenceDate = local.Current.Date;
              local.SpDocKey.KeyRecaptureRule =
                export.Group.Item.ObligorRule.SystemGeneratedIdentifier;
              UseSpDocFindOutgoingDocument();

              if (local.Infrastructure.SystemGeneratedIdentifier > 0)
              {
                // mjr---> A document already exists.  Check outgoing_doc 
                // print_succesfful_ind to determine action.
                switch(AsChar(local.OutgoingDocument.PrintSucessfulIndicator))
                {
                  case 'Y':
                    // mjr---> Printed successfully
                    local.Infrastructure.SystemGeneratedIdentifier = 0;

                    break;
                  case 'N':
                    // mjr---> NOT Printed successfully
                    break;
                  case 'G':
                    // mjr---> Awaiting generation
                    break;
                  case 'B':
                    // mjr---> Awaiting Natural batch print
                    UseSpDocCancelOutgoingDoc();
                    local.Infrastructure.SystemGeneratedIdentifier = 0;

                    break;
                  case 'C':
                    // mjr---> Printing canceled
                    break;
                  default:
                    break;
                }
              }

              // mjr
              // ------------------------------------------------
              // 02/18/1999
              // Use this action block (create_document_infrastruct) to create a
              // document trigger
              // ------------------------------------------------------------
              UseSpCreateDocumentInfrastruct();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else
              {
                goto Test3;
              }

              // *************** End RCAPRPAY document trigger 
              // ******************
            }
          }
        }

        UseFnGrpReadObligorRecaptRules();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "SIGNOFF":
        UseScEabSignoff();

        break;
      case "DISPLAY":
        if (!IsEmpty(export.CsePerson.Number))
        {
          export.CsePersonsWorkSet.Number = export.CsePerson.Number;
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;

            return;
          }

          UseFnGrpReadObligorRecaptRules();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (export.Group.IsEmpty)
            {
              ExitState = "FN0000_NO_RECORDS_FOUND";
            }
            else
            {
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }

            export.Prev.Number = export.CsePersonsWorkSet.Number;
          }
          else
          {
            return;
          }

          global.Command = "";
        }
        else
        {
          ExitState = "FN0000_OBLIGOR_NUMBER_REQUIRED";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        break;
      case "EXIT":
        break;
      case "LIST":
        if (AsChar(import.PromptCsePerson.SelectChar) == 'S')
        {
          // ---------------------------------------------
          // Go to NAME screen for name search
          // ---------------------------------------------
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

          return;
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          var field = GetField(export.PromptCsePerson, "selectChar");

          field.Error = true;
        }

        break;
      case "DELETE":
        // **** IF edit errors were found, escape to bottom of prad for field 
        // protection logic.
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // *** Delete is allowed if start date < current date.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Select.SelectChar) == 'S')
          {
            if (ReadObligorRule())
            {
              DeleteObligorRule();
            }
            else
            {
              ExitState = "FN0000_OBLIGOR_RULE_NF";
              UseEabRollbackCics();

              goto Test3;
            }

            // : IF a document has been generated for this recapture rule, 
            // cancel it.
            local.Document.Name = "RCAPRPAY";
            local.Infrastructure.ReferenceDate = local.Current.Date;
            local.SpDocKey.KeyRecaptureRule =
              export.Group.Item.ObligorRule.SystemGeneratedIdentifier;
            UseSpDocFindOutgoingDocument();

            if (local.Infrastructure.SystemGeneratedIdentifier > 0)
            {
              // mjr---> A document already exists.  Check outgoing_doc 
              // print_succesfful_ind to determine action.
              switch(AsChar(local.OutgoingDocument.PrintSucessfulIndicator))
              {
                case 'Y':
                  // mjr---> Printed successfully
                  local.Infrastructure.SystemGeneratedIdentifier = 0;

                  break;
                case 'N':
                  // mjr---> NOT Printed successfully
                  UseSpDocCancelOutgoingDoc();

                  break;
                case 'G':
                  // mjr---> Awaiting generation
                  UseSpDocCancelOutgoingDoc();

                  break;
                case 'B':
                  // mjr---> Awaiting Natural batch print
                  UseSpDocCancelOutgoingDoc();

                  break;
                case 'C':
                  // mjr---> Printing canceled
                  break;
                default:
                  break;
              }
            }
          }
        }

        UseFnGrpReadObligorRecaptRules();
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "RCAP":
        // Pass the Obligor cse_person number and Name
        ExitState = "FN0000_LNK_RCAP_LST_RECOV_OBLIGS";

        return;
      case "ROHL":
        // Pass the Obligor cse_person number and Name
        ExitState = "FN0000_LNK_ROHL_LST_OB_RC_HIST";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

Test3:

    // ***  Protect all fields except expire date if the rule is active (start 
    // date less or equal current date).
    local.AfterTheFirst.Flag = "N";

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (Equal(export.Group.Item.ObligorRule.DiscontinueDate,
        local.HighDate.Date))
      {
        export.Group.Update.ObligorRule.DiscontinueDate =
          local.InitializedToZero.Date;
      }

      // *** The first entry must always be unprotected, but for any occurences 
      // after this, all fields except discontinue date are protected if the
      // start date is less or equal to current (export _grp_select ief_supplied
      // flag was set in the read CAB)
      if (AsChar(local.AfterTheFirst.Flag) == 'Y' && (
        AsChar(export.Group.Item.Select.Flag) == 'Y' || AsChar
        (export.Group.Item.Select.Flag) == 'X'))
      {
        var field1 = GetField(export.Group.Item.ObligorRule, "effectiveDate");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Group.Item.ObligorRule, "negotiatedDate");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Group.Item.ObligorRule, "nonAdcArrearsAmount");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Group.Item.ObligorRule, "nonAdcArrearsMaxAmount");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 =
          GetField(export.Group.Item.ObligorRule, "nonAdcArrearsPercentage");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 =
          GetField(export.Group.Item.ObligorRule, "nonAdcCurrentAmount");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 =
          GetField(export.Group.Item.ObligorRule, "nonAdcCurrentMaxAmount");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 =
          GetField(export.Group.Item.ObligorRule, "nonAdcCurrentPercentage");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.Group.Item.ObligorRule, "passthruAmount");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 =
          GetField(export.Group.Item.ObligorRule, "passthruMaxAmount");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 =
          GetField(export.Group.Item.ObligorRule, "passthruPercentage");

        field11.Color = "cyan";
        field11.Protected = true;

        // *** Also protect discontinue date and select character if discontinue
        // date is less than current date.
        if (AsChar(export.Group.Item.Select.Flag) == 'X')
        {
          var field12 = GetField(export.Group.Item.Select, "selectChar");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 =
            GetField(export.Group.Item.ObligorRule, "discontinueDate");

          field13.Color = "cyan";
          field13.Protected = true;
        }
      }

      local.AfterTheFirst.Flag = "Y";
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveGroup(FnGrpReadObligorRecaptRules.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    MoveCommon(source.Select, target.Select);
    target.ObligorRule.Assign(source.Detail);
    target.Negotiated.Flag = source.DetailNeg.Flag;
    target.PrintRcaprpay.Flag = source.PrintRcaprpay.Flag;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
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

  private static void MoveRecaptureRule1(RecaptureRule source,
    RecaptureRule target)
  {
    target.NegotiatedDate = source.NegotiatedDate;
    target.NonAdcArrearsMaxAmount = source.NonAdcArrearsMaxAmount;
    target.NonAdcArrearsAmount = source.NonAdcArrearsAmount;
    target.NonAdcArrearsPercentage = source.NonAdcArrearsPercentage;
    target.NonAdcCurrentMaxAmount = source.NonAdcCurrentMaxAmount;
    target.NonAdcCurrentAmount = source.NonAdcCurrentAmount;
    target.NonAdcCurrentPercentage = source.NonAdcCurrentPercentage;
    target.Type1 = source.Type1;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PassthruPercentage = source.PassthruPercentage;
    target.PassthruAmount = source.PassthruAmount;
    target.PassthruMaxAmount = source.PassthruMaxAmount;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveRecaptureRule2(RecaptureRule source,
    RecaptureRule target)
  {
    target.NegotiatedDate = source.NegotiatedDate;
    target.NonAdcArrearsMaxAmount = source.NonAdcArrearsMaxAmount;
    target.NonAdcArrearsAmount = source.NonAdcArrearsAmount;
    target.NonAdcArrearsPercentage = source.NonAdcArrearsPercentage;
    target.NonAdcCurrentMaxAmount = source.NonAdcCurrentMaxAmount;
    target.NonAdcCurrentAmount = source.NonAdcCurrentAmount;
    target.NonAdcCurrentPercentage = source.NonAdcCurrentPercentage;
    target.Type1 = source.Type1;
    target.PassthruPercentage = source.PassthruPercentage;
    target.PassthruAmount = source.PassthruAmount;
    target.PassthruMaxAmount = source.PassthruMaxAmount;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveRecaptureRule3(RecaptureRule source,
    RecaptureRule target)
  {
    target.NonAdcArrearsMaxAmount = source.NonAdcArrearsMaxAmount;
    target.NonAdcArrearsAmount = source.NonAdcArrearsAmount;
    target.NonAdcArrearsPercentage = source.NonAdcArrearsPercentage;
    target.NonAdcCurrentMaxAmount = source.NonAdcCurrentMaxAmount;
    target.NonAdcCurrentAmount = source.NonAdcCurrentAmount;
    target.NonAdcCurrentPercentage = source.NonAdcCurrentPercentage;
    target.PassthruPercentage = source.PassthruPercentage;
    target.PassthruAmount = source.PassthruAmount;
    target.PassthruMaxAmount = source.PassthruMaxAmount;
  }

  private static void MoveRecaptureRule4(RecaptureRule source,
    RecaptureRule target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyRecaptureRule = source.KeyRecaptureRule;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.LeftPadding.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPadding.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPadding.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnCabCheckRecapInstrucAmts()
  {
    var useImport = new FnCabCheckRecapInstrucAmts.Import();
    var useExport = new FnCabCheckRecapInstrucAmts.Export();

    useImport.ErrorCount.Count = local.ErrorCount.Count;
    MoveRecaptureRule3(export.Group.Item.ObligorRule, useImport.ObligorRule);

    Call(FnCabCheckRecapInstrucAmts.Execute, useImport, useExport);

    local.ErrorCount.Count = useExport.ErrorCount.Count;
    local.PassthruAmtErr.Flag = useExport.PassthruAmtErr.Flag;
    local.PassthruPerErr.Flag = useExport.PassthruPerErr.Flag;
    local.PassthruMaxErr.Flag = useExport.PassthruMaxErr.Flag;
    local.NadcCurrAmtErr.Flag = useExport.NadcCurrAmtErr.Flag;
    local.NadcCurrPerErr.Flag = useExport.NadcCurrPerErr.Flag;
    local.NadcCurrMaxErr.Flag = useExport.NadcCurrMaxErr.Flag;
    local.NadcArrearsAmtErr.Flag = useExport.NadcArrearsAmtErr.Flag;
    local.NadcArrearsPerErr.Flag = useExport.NadcArrearsPerErr.Flag;
    local.NadcArrearsMaxErr.Flag = useExport.NadcArrearsMaxErr.Flag;
  }

  private void UseFnCompareRecaptureDates()
  {
    var useImport = new FnCompareRecaptureDates.Import();
    var useExport = new FnCompareRecaptureDates.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveRecaptureRule4(export.Group.Item.ObligorRule, useImport.New1);
    useImport.Error.Count = local.ErrorCount.Count;
    useImport.Active.Flag = local.Active.Flag;

    Call(FnCompareRecaptureDates.Execute, useImport, useExport);

    local.ErrorCount.Count = useExport.Error.Count;
    local.ExpireDateError.Flag = useExport.ExpireError.Flag;
    local.EffectiveDateError.Flag = useExport.EffectiveError.Flag;
  }

  private void UseFnCreateObligorRule()
  {
    var useImport = new FnCreateObligorRule.Import();
    var useExport = new FnCreateObligorRule.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveRecaptureRule2(export.Group.Item.ObligorRule, useImport.ObligorRule);

    Call(FnCreateObligorRule.Execute, useImport, useExport);

    local.RecaptureRule.SystemGeneratedIdentifier =
      useExport.RecaptureRule.SystemGeneratedIdentifier;
  }

  private void UseFnGrpReadObligorRecaptRules()
  {
    var useImport = new FnGrpReadObligorRecaptRules.Import();
    var useExport = new FnGrpReadObligorRecaptRules.Export();

    MoveCsePerson(export.CsePerson, useImport.CsePerson);

    Call(FnGrpReadObligorRecaptRules.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseFnUpdateRecaptureRuleObligor()
  {
    var useImport = new FnUpdateRecaptureRuleObligor.Import();
    var useExport = new FnUpdateRecaptureRuleObligor.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveRecaptureRule1(export.Group.Item.ObligorRule, useImport.ObligorRule);

    Call(FnUpdateRecaptureRuleObligor.Execute, useImport, useExport);
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

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

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void UseSpDocCancelOutgoingDoc()
  {
    var useImport = new SpDocCancelOutgoingDoc.Import();
    var useExport = new SpDocCancelOutgoingDoc.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;

    Call(SpDocCancelOutgoingDoc.Execute, useImport, useExport);
  }

  private void UseSpDocFindOutgoingDocument()
  {
    var useImport = new SpDocFindOutgoingDocument.Import();
    var useExport = new SpDocFindOutgoingDocument.Export();

    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpDocFindOutgoingDocument.Execute, useImport, useExport);

    local.OutgoingDocument.PrintSucessfulIndicator =
      useExport.OutgoingDocument.PrintSucessfulIndicator;
    local.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
  }

  private void DeleteObligorRule()
  {
    Update("DeleteObligorRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "recaptureRuleId",
          entities.Existing.SystemGeneratedIdentifier);
      });
  }

  private bool ReadObligorRule()
  {
    entities.Existing.Populated = false;

    return Read("ReadObligorRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "recaptureRuleId",
          export.Group.Item.ObligorRule.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.EffectiveDate = db.GetDate(reader, 1);
        entities.Existing.NegotiatedDate = db.GetNullableDate(reader, 2);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.Existing.NonAdcArrearsMaxAmount =
          db.GetNullableDecimal(reader, 4);
        entities.Existing.NonAdcArrearsAmount =
          db.GetNullableDecimal(reader, 5);
        entities.Existing.NonAdcArrearsPercentage =
          db.GetNullableInt32(reader, 6);
        entities.Existing.NonAdcCurrentMaxAmount =
          db.GetNullableDecimal(reader, 7);
        entities.Existing.NonAdcCurrentAmount =
          db.GetNullableDecimal(reader, 8);
        entities.Existing.NonAdcCurrentPercentage =
          db.GetNullableInt32(reader, 9);
        entities.Existing.PassthruPercentage = db.GetNullableInt32(reader, 10);
        entities.Existing.PassthruAmount = db.GetNullableDecimal(reader, 11);
        entities.Existing.PassthruMaxAmount = db.GetNullableDecimal(reader, 12);
        entities.Existing.CreatedBy = db.GetString(reader, 13);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.Existing.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Existing.LastUpdatedTmst = db.GetNullableDateTime(reader, 16);
        entities.Existing.Type1 = db.GetString(reader, 17);
        entities.Existing.RepaymentLetterPrintDate =
          db.GetNullableDate(reader, 18);
        entities.Existing.Populated = true;
        CheckValid<RecaptureRule>("Type1", entities.Existing.Type1);
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
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of ObligorRule.
      /// </summary>
      [JsonPropertyName("obligorRule")]
      public RecaptureRule ObligorRule
      {
        get => obligorRule ??= new();
        set => obligorRule = value;
      }

      /// <summary>
      /// A value of Negotiated.
      /// </summary>
      [JsonPropertyName("negotiated")]
      public Common Negotiated
      {
        get => negotiated ??= new();
        set => negotiated = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common select;
      private RecaptureRule obligorRule;
      private Common negotiated;
    }

    /// <summary>
    /// A value of ValidCodeAttribute.
    /// </summary>
    [JsonPropertyName("validCodeAttribute")]
    public Common ValidCodeAttribute
    {
      get => validCodeAttribute ??= new();
      set => validCodeAttribute = value;
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
    /// A value of PromptCsePerson.
    /// </summary>
    [JsonPropertyName("promptCsePerson")]
    public Common PromptCsePerson
    {
      get => promptCsePerson ??= new();
      set => promptCsePerson = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of AlreadyDisplayed.
    /// </summary>
    [JsonPropertyName("alreadyDisplayed")]
    public Common AlreadyDisplayed
    {
      get => alreadyDisplayed ??= new();
      set => alreadyDisplayed = value;
    }

    /// <summary>
    /// A value of Zdel1.
    /// </summary>
    [JsonPropertyName("zdel1")]
    public RecaptureRule Zdel1
    {
      get => zdel1 ??= new();
      set => zdel1 = value;
    }

    /// <summary>
    /// A value of ZdelPreviousObligorRule.
    /// </summary>
    [JsonPropertyName("zdelPreviousObligorRule")]
    public RecaptureRule ZdelPreviousObligorRule
    {
      get => zdelPreviousObligorRule ??= new();
      set => zdelPreviousObligorRule = value;
    }

    private Common validCodeAttribute;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common promptCsePerson;
    private CsePerson csePerson;
    private CsePerson prev;
    private Array<GroupGroup> group;
    private Standard standard;
    private NextTranInfo hidden;
    private Common alreadyDisplayed;
    private RecaptureRule zdel1;
    private RecaptureRule zdelPreviousObligorRule;
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
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of ObligorRule.
      /// </summary>
      [JsonPropertyName("obligorRule")]
      public RecaptureRule ObligorRule
      {
        get => obligorRule ??= new();
        set => obligorRule = value;
      }

      /// <summary>
      /// A value of Negotiated.
      /// </summary>
      [JsonPropertyName("negotiated")]
      public Common Negotiated
      {
        get => negotiated ??= new();
        set => negotiated = value;
      }

      /// <summary>
      /// A value of PrintRcaprpay.
      /// </summary>
      [JsonPropertyName("printRcaprpay")]
      public Common PrintRcaprpay
      {
        get => printRcaprpay ??= new();
        set => printRcaprpay = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common select;
      private RecaptureRule obligorRule;
      private Common negotiated;
      private Common printRcaprpay;
    }

    /// <summary>
    /// A value of ValidCodeAttribute.
    /// </summary>
    [JsonPropertyName("validCodeAttribute")]
    public Common ValidCodeAttribute
    {
      get => validCodeAttribute ??= new();
      set => validCodeAttribute = value;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public WorkArea Command
    {
      get => command ??= new();
      set => command = value;
    }

    /// <summary>
    /// A value of PromptedFrom.
    /// </summary>
    [JsonPropertyName("promptedFrom")]
    public Common PromptedFrom
    {
      get => promptedFrom ??= new();
      set => promptedFrom = value;
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
    /// A value of PromptCsePerson.
    /// </summary>
    [JsonPropertyName("promptCsePerson")]
    public Common PromptCsePerson
    {
      get => promptCsePerson ??= new();
      set => promptCsePerson = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of LastUpdate.
    /// </summary>
    [JsonPropertyName("lastUpdate")]
    public DateWorkArea LastUpdate
    {
      get => lastUpdate ??= new();
      set => lastUpdate = value;
    }

    /// <summary>
    /// A value of Zdel1.
    /// </summary>
    [JsonPropertyName("zdel1")]
    public RecaptureRule Zdel1
    {
      get => zdel1 ??= new();
      set => zdel1 = value;
    }

    /// <summary>
    /// A value of ZdelPortPreviousObligorRule.
    /// </summary>
    [JsonPropertyName("zdelPortPreviousObligorRule")]
    public RecaptureRule ZdelPortPreviousObligorRule
    {
      get => zdelPortPreviousObligorRule ??= new();
      set => zdelPortPreviousObligorRule = value;
    }

    /// <summary>
    /// A value of ZdelExportPrevious.
    /// </summary>
    [JsonPropertyName("zdelExportPrevious")]
    public CsePerson ZdelExportPrevious
    {
      get => zdelExportPrevious ??= new();
      set => zdelExportPrevious = value;
    }

    private Common validCodeAttribute;
    private WorkArea command;
    private Common promptedFrom;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common promptCsePerson;
    private CsePerson csePerson;
    private CsePerson prev;
    private Array<GroupGroup> group;
    private Standard standard;
    private NextTranInfo hidden;
    private DateWorkArea lastUpdate;
    private RecaptureRule zdel1;
    private RecaptureRule zdelPortPreviousObligorRule;
    private CsePerson zdelExportPrevious;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ZdelGroup group.</summary>
    [Serializable]
    public class ZdelGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private Common common;
    }

    /// <summary>
    /// A value of RecaptureRule.
    /// </summary>
    [JsonPropertyName("recaptureRule")]
    public RecaptureRule RecaptureRule
    {
      get => recaptureRule ??= new();
      set => recaptureRule = value;
    }

    /// <summary>
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Common Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public RecaptureRule Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of HighDate.
    /// </summary>
    [JsonPropertyName("highDate")]
    public DateWorkArea HighDate
    {
      get => highDate ??= new();
      set => highDate = value;
    }

    /// <summary>
    /// A value of AddEdit.
    /// </summary>
    [JsonPropertyName("addEdit")]
    public Common AddEdit
    {
      get => addEdit ??= new();
      set => addEdit = value;
    }

    /// <summary>
    /// A value of ErrorCount.
    /// </summary>
    [JsonPropertyName("errorCount")]
    public Common ErrorCount
    {
      get => errorCount ??= new();
      set => errorCount = value;
    }

    /// <summary>
    /// A value of NadcArrearsMaxErr.
    /// </summary>
    [JsonPropertyName("nadcArrearsMaxErr")]
    public Common NadcArrearsMaxErr
    {
      get => nadcArrearsMaxErr ??= new();
      set => nadcArrearsMaxErr = value;
    }

    /// <summary>
    /// A value of NadcArrearsPerErr.
    /// </summary>
    [JsonPropertyName("nadcArrearsPerErr")]
    public Common NadcArrearsPerErr
    {
      get => nadcArrearsPerErr ??= new();
      set => nadcArrearsPerErr = value;
    }

    /// <summary>
    /// A value of NadcArrearsAmtErr.
    /// </summary>
    [JsonPropertyName("nadcArrearsAmtErr")]
    public Common NadcArrearsAmtErr
    {
      get => nadcArrearsAmtErr ??= new();
      set => nadcArrearsAmtErr = value;
    }

    /// <summary>
    /// A value of NadcCurrMaxErr.
    /// </summary>
    [JsonPropertyName("nadcCurrMaxErr")]
    public Common NadcCurrMaxErr
    {
      get => nadcCurrMaxErr ??= new();
      set => nadcCurrMaxErr = value;
    }

    /// <summary>
    /// A value of NadcCurrPerErr.
    /// </summary>
    [JsonPropertyName("nadcCurrPerErr")]
    public Common NadcCurrPerErr
    {
      get => nadcCurrPerErr ??= new();
      set => nadcCurrPerErr = value;
    }

    /// <summary>
    /// A value of NadcCurrAmtErr.
    /// </summary>
    [JsonPropertyName("nadcCurrAmtErr")]
    public Common NadcCurrAmtErr
    {
      get => nadcCurrAmtErr ??= new();
      set => nadcCurrAmtErr = value;
    }

    /// <summary>
    /// A value of PassthruMaxErr.
    /// </summary>
    [JsonPropertyName("passthruMaxErr")]
    public Common PassthruMaxErr
    {
      get => passthruMaxErr ??= new();
      set => passthruMaxErr = value;
    }

    /// <summary>
    /// A value of PassthruPerErr.
    /// </summary>
    [JsonPropertyName("passthruPerErr")]
    public Common PassthruPerErr
    {
      get => passthruPerErr ??= new();
      set => passthruPerErr = value;
    }

    /// <summary>
    /// A value of PassthruAmtErr.
    /// </summary>
    [JsonPropertyName("passthruAmtErr")]
    public Common PassthruAmtErr
    {
      get => passthruAmtErr ??= new();
      set => passthruAmtErr = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public RecaptureRule New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public RecaptureRule Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public RecaptureRule Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of ExpDateChanged.
    /// </summary>
    [JsonPropertyName("expDateChanged")]
    public Common ExpDateChanged
    {
      get => expDateChanged ??= new();
      set => expDateChanged = value;
    }

    /// <summary>
    /// A value of FieldsChanged.
    /// </summary>
    [JsonPropertyName("fieldsChanged")]
    public Common FieldsChanged
    {
      get => fieldsChanged ??= new();
      set => fieldsChanged = value;
    }

    /// <summary>
    /// A value of AfterTheFirst.
    /// </summary>
    [JsonPropertyName("afterTheFirst")]
    public Common AfterTheFirst
    {
      get => afterTheFirst ??= new();
      set => afterTheFirst = value;
    }

    /// <summary>
    /// A value of EffectiveDateError.
    /// </summary>
    [JsonPropertyName("effectiveDateError")]
    public Common EffectiveDateError
    {
      get => effectiveDateError ??= new();
      set => effectiveDateError = value;
    }

    /// <summary>
    /// A value of ExpireDateError.
    /// </summary>
    [JsonPropertyName("expireDateError")]
    public Common ExpireDateError
    {
      get => expireDateError ??= new();
      set => expireDateError = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public RecaptureRule Selected
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
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
    }

    /// <summary>
    /// A value of RecapModified.
    /// </summary>
    [JsonPropertyName("recapModified")]
    public Common RecapModified
    {
      get => recapModified ??= new();
      set => recapModified = value;
    }

    /// <summary>
    /// A value of InitializedToZero.
    /// </summary>
    [JsonPropertyName("initializedToZero")]
    public DateWorkArea InitializedToZero
    {
      get => initializedToZero ??= new();
      set => initializedToZero = value;
    }

    /// <summary>
    /// A value of LowDate.
    /// </summary>
    [JsonPropertyName("lowDate")]
    public DateWorkArea LowDate
    {
      get => lowDate ??= new();
      set => lowDate = value;
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
    /// A value of ZdelLocalAlreadyDisplayed.
    /// </summary>
    [JsonPropertyName("zdelLocalAlreadyDisplayed")]
    public Common ZdelLocalAlreadyDisplayed
    {
      get => zdelLocalAlreadyDisplayed ??= new();
      set => zdelLocalAlreadyDisplayed = value;
    }

    /// <summary>
    /// A value of ZdelLocalNew.
    /// </summary>
    [JsonPropertyName("zdelLocalNew")]
    public ExpireEffectiveDateAttributes ZdelLocalNew
    {
      get => zdelLocalNew ??= new();
      set => zdelLocalNew = value;
    }

    /// <summary>
    /// A value of ZdelLocalPrevious.
    /// </summary>
    [JsonPropertyName("zdelLocalPrevious")]
    public ExpireEffectiveDateAttributes ZdelLocalPrevious
    {
      get => zdelLocalPrevious ??= new();
      set => zdelLocalPrevious = value;
    }

    /// <summary>
    /// A value of ZdelLocalNext.
    /// </summary>
    [JsonPropertyName("zdelLocalNext")]
    public ExpireEffectiveDateAttributes ZdelLocalNext
    {
      get => zdelLocalNext ??= new();
      set => zdelLocalNext = value;
    }

    /// <summary>
    /// Gets a value of Zdel.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroup> Zdel => zdel ??= new(ZdelGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Zdel for json serialization.
    /// </summary>
    [JsonPropertyName("zdel")]
    [Computed]
    public IList<ZdelGroup> Zdel_Json
    {
      get => zdel;
      set => Zdel.Assign(value);
    }

    private RecaptureRule recaptureRule;
    private Common print;
    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
    private Document document;
    private SpDocKey spDocKey;
    private RecaptureRule temp;
    private Common active;
    private DateWorkArea highDate;
    private Common addEdit;
    private Common errorCount;
    private Common nadcArrearsMaxErr;
    private Common nadcArrearsPerErr;
    private Common nadcArrearsAmtErr;
    private Common nadcCurrMaxErr;
    private Common nadcCurrPerErr;
    private Common nadcCurrAmtErr;
    private Common passthruMaxErr;
    private Common passthruPerErr;
    private Common passthruAmtErr;
    private RecaptureRule new1;
    private RecaptureRule prev;
    private RecaptureRule next;
    private Common expDateChanged;
    private Common fieldsChanged;
    private Common afterTheFirst;
    private Common effectiveDateError;
    private Common expireDateError;
    private CsePerson csePerson;
    private RecaptureRule selected;
    private Common selectFound;
    private Common work;
    private TextWorkArea leftPadding;
    private Common recapModified;
    private DateWorkArea initializedToZero;
    private DateWorkArea lowDate;
    private DateWorkArea current;
    private Common zdelLocalAlreadyDisplayed;
    private ExpireEffectiveDateAttributes zdelLocalNew;
    private ExpireEffectiveDateAttributes zdelLocalPrevious;
    private ExpireEffectiveDateAttributes zdelLocalNext;
    private Array<ZdelGroup> zdel;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public RecaptureRule Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private CsePerson csePerson;
    private RecaptureRule existing;
    private CsePersonAccount obligor;
  }
#endregion
}
