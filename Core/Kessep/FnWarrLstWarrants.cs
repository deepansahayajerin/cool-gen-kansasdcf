// Program: FN_WARR_LST_WARRANTS, ID: 371864796, model: 746.
// Short name: SWEWARRP
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
/// A program: FN_WARR_LST_WARRANTS.
/// </para>
/// <para>
/// RESP:FINCLMNGMNT
/// This screen will be used to list warrants for a payee, warrant in a 
/// particular status, or warrants starting on a requested date.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnWarrLstWarrants: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_WARR_LST_WARRANTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnWarrLstWarrants(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnWarrLstWarrants.
  /// </summary>
  public FnWarrLstWarrants(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------------
    // Date		Developer	Description
    // 04/22/1997	C. Dasgupta
    // 1)Modified logic to display same CSE_PERSON Number and name which was 
    // there in the screen before flowing to name list if nothing is selected in
    // name list screen.
    // 2)Changed code to display proper message in case of an invalid prompt 
    // selection
    // 06/19/97	T.O.Redmond	Correct Prompt Logic
    // 11/23/98 K. Doshi Numerous phase 2 changes as requested by SMEs
    // 7/15/99  K. Doshi Reduce date range for search.
    // 10/28/99 K. Doshi PR#77957. Performace change to improve DB2 selects.
    // 10/28/99 K. Doshi PR#78639. Change date defaults to one month when payee/
    // DP has been entered else set both from and to dates to 'current date - 1
    // day'.
    // 11/23/99 K. Doshi PR#80747. Sort by Desc process date when starting 
    // warrant number is NOT entered. When entered, sort by Asc Warrant number.
    // 04/20/00 K. Doshi PR#93707. Fix READ EACHes to improve response time.
    // 11/21/00 Fangman  WR 000234.  Added Interstate Indicator to the screen.
    // 02/05/01 K. Doshi PR#109149. Inflate GVs. Change read properties to 
    // Uncommitted/Browse.
    // 04/12/01 Fangman  PR#112439. Added msg for PF2 Display.
    // 05/22/01 K. Doshi  WR# 285. Cater for duplicate warrants.
    // 11/28/01 K. Doshi  WR# 020147 - KPC Recoupment. Add KPC RCP flag to list
    // ..
    // 2/3/2004 H. Mace - PR191778 - display null valued payment date records.
    // 5/14/2019 GVandy - CQ65809 - Display warrants in REQCANCEL status.
    // ------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();
      ExitState = "ECO_XFR_TO_SIGNOFF_PROCEDURE";

      return;
    }

    if (Equal(global.Command, "RETLINK"))
    {
      return;
    }

    export.Hidden.Assign(import.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Pass.Number) && AsChar
      (import.PromptDpCsePerson.Text1) == 'S')
    {
      export.Dp.Number = import.Pass.Number;
    }
    else
    {
      export.Dp.Number = import.Dp.Number;
    }

    if (!IsEmpty(import.Pass.Number) && AsChar
      (import.PromptCsePersonTextWorkArea.Text1) == 'S')
    {
      export.CsePersonsWorkSet.Number = import.Pass.Number;
    }
    else
    {
      export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    }

    export.PaymentStatus.Code = import.PaymentStatus.Code;
    export.From.Date = import.From.Date;
    export.To.Date = import.To.Date;
    MovePaymentRequest2(import.StartingWarrant, export.StartingWarrant);

    // ----------------------------------------------------
    // 05/22/01 K. Doshi  WR# 285.
    // Cater for duplicate warrants.
    // ---------------------------------------------------
    export.HiddenDuplicateWarrants.Flag = import.HiddenDuplicateWarrants.Flag;
    local.Common.Count = 0;

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.Dp.Number))
    {
      local.TextWorkArea.Text10 = export.Dp.Number;
      UseEabPadLeftWithZeros();
      export.Dp.Number = local.TextWorkArea.Text10;
    }

    // --------------------------------------------------------
    // KD - 11/23/98
    // Pad zeroes to the left of warrant number
    // --------------------------------------------------------
    if (!IsEmpty(export.StartingWarrant.Number))
    {
      local.TextWorkArea.Text10 = export.StartingWarrant.Number ?? Spaces(10);
      UseEabPadLeftWithZeros();
      export.StartingWarrant.Number =
        Substring(local.TextWorkArea.Text10, 2, 9);
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      // ***** Since the command is equal to display we want to limit what we 
      // move from imports to exports.
    }
    else
    {
      // --------------------------------------------------------
      // KD - 12/29/98
      // Move payment_status from import to export
      // --------------------------------------------------------
      export.PaymentStatus.Assign(import.PaymentStatus);

      // ---------------------------------------------
      // MTW - Chayan 4/22/97 Change Start
      // Added the IF statement
      // ---------------------------------------------
      if (!IsEmpty(import.Pass.FormattedName) && AsChar
        (import.PromptCsePersonTextWorkArea.Text1) == 'S')
      {
        export.CsePersonsWorkSet.FormattedName = import.Pass.FormattedName;
      }
      else
      {
        export.CsePersonsWorkSet.FormattedName =
          import.CsePersonsWorkSet.FormattedName;
      }

      if (!IsEmpty(import.Pass.FormattedName) && AsChar
        (import.PromptDpCsePerson.Text1) == 'S')
      {
        export.Dp.FormattedName = import.Pass.FormattedName;
      }
      else
      {
        export.Dp.FormattedName = import.Dp.FormattedName;
      }

      // ---------------------------------------------
      // MTW - Chayan 4/22/97 Change End
      // ---------------------------------------------
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailPaymentStatus.Code =
          import.Import1.Item.DetailPaymentStatus.Code;
        export.Export1.Update.DetailPaymentRequest.Assign(
          import.Import1.Item.DetailPaymentRequest);
        MoveCsePersonsWorkSet(import.Import1.Item.DetailCsePersonsWorkSet,
          export.Export1.Update.DetailCsePersonsWorkSet);

        if (Equal(global.Command, "AP_ACC") || Equal
          (global.Command, "OBTL") || Equal(global.Command, "PACC") || Equal
          (global.Command, "WDTL") || Equal(global.Command, "WAST") || Equal
          (global.Command, "WHST") || Equal(global.Command, "WARA") || Equal
          (global.Command, "RETURN"))
        {
          if (!IsEmpty(import.Import1.Item.DetailCommon.SelectChar))
          {
            ++local.Common.Count;

            if (local.Common.Count > 1)
            {
              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
              export.Export1.Next();

              return;
            }

            if (AsChar(import.Import1.Item.DetailCommon.SelectChar) == 'S')
            {
              MovePaymentRequest1(export.Export1.Item.DetailPaymentRequest,
                export.PassThruFlowPaymentRequest);
              export.PassThruFlowCsePerson.Number =
                export.Export1.Item.DetailCsePersonsWorkSet.Number;
            }
            else
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
            }
          }
        }

        export.Export1.Next();
      }
    }

    if (IsExitState("ACO_NE0000_INVALID_SELECT_CODE1"))
    {
      return;
    }

    if (Equal(global.Command, "AP_ACC") || Equal(global.Command, "OBTL") || Equal
      (global.Command, "PACC") || Equal(global.Command, "WDTL") || Equal
      (global.Command, "WAST") || Equal(global.Command, "WHST") || Equal
      (global.Command, "WARA") || Equal(global.Command, "RETURN"))
    {
      if (local.Common.Count > 1)
      {
        ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTION";

        return;
      }
    }

    // *** If it is a case of return from the cse person list or Payment Status 
    // list, then display the screen with these selected values
    if (Equal(global.Command, "RETCDVL") || Equal(global.Command, "RETCSENO"))
    {
      if (Equal(global.Command, "RETCDVL"))
      {
        var field = GetField(export.PaymentStatus, "code");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;
      }

      // ---------------------------------------------
      // KD - PR# 77957
      // 10/26/99 - Place cursor on DP number if user prompted on
      // DP.
      // ---------------------------------------------
      if (Equal(global.Command, "RETCSENO") && AsChar
        (import.PromptDpCsePerson.Text1) == 'S')
      {
        var field = GetField(export.Dp, "number");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;
      }

      return;
    }

    UseCabSetMaximumDiscontinueDate();

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber = import.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumber ?? Spaces
        (10);

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "PACC") || Equal(global.Command, "WAST") || Equal
      (global.Command, "WHST") || Equal(global.Command, "WDTL") || Equal
      (global.Command, "WARA") || Equal(global.Command, "PACC"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------------------
    //                   Main CASE OF COMMAND.
    // ---------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        // ***** Will link to CSE Person or Payment Status for pick lists
        // ---------------------------------------------
        // MTW - Chayan 4/22/97 Change Start
        // ---------------------------------------------
        export.PromptCsePersonTextWorkArea.Text1 =
          import.PromptCsePersonTextWorkArea.Text1;
        export.PromptStatusTextWorkArea.Text1 =
          import.PromptStatusTextWorkArea.Text1;

        // ---------------------------------------------
        // KD - PR#77957
        // Add prompt for DP field. Incorporate new code to cross validate 
        // fields.
        // ---------------------------------------------
        export.PromptDpCsePerson.Text1 = import.PromptDpCsePerson.Text1;

        if (AsChar(import.PromptCsePersonTextWorkArea.Text1) == 'S')
        {
          if (AsChar(import.PromptDpCsePerson.Text1) == 'S')
          {
            ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S";

            var field4 = GetField(export.PromptDpCsePerson, "text1");

            field4.Error = true;

            var field5 = GetField(export.PromptCsePersonTextWorkArea, "text1");

            field5.Error = true;

            return;
          }

          if (AsChar(import.PromptStatusTextWorkArea.Text1) == 'S')
          {
            ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S";

            var field4 = GetField(export.PromptStatusTextWorkArea, "text1");

            field4.Error = true;

            var field5 = GetField(export.PromptCsePersonTextWorkArea, "text1");

            field5.Error = true;

            return;
          }
        }

        if (AsChar(import.PromptDpCsePerson.Text1) == 'S')
        {
          if (AsChar(import.PromptStatusTextWorkArea.Text1) == 'S')
          {
            ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S";

            var field4 = GetField(export.PromptDpCsePerson, "text1");

            field4.Error = true;

            var field5 = GetField(export.PromptStatusTextWorkArea, "text1");

            field5.Error = true;

            return;
          }
        }

        switch(AsChar(export.PromptCsePersonTextWorkArea.Text1))
        {
          case ' ':
            break;
          case 'S':
            // --------------------------------------------------------
            // KD - 12/28/98
            // Set Phonetic flag and % when calling NAME
            // --------------------------------------------------------
            export.Phonetic.Flag = "Y";
            export.Phonetic.Percentage = 35;
            ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";

            return;
          default:
            var field = GetField(export.PromptCsePersonTextWorkArea, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        // ---------------------------------------------
        // KD - PR#77957
        // Add code for new DP field. Prompt on DP field will also flow
        // to NAME to retrieve cse_person # and name like Payee
        // field. On return from NAME prompt selection fields will be
        // checked to see which one of the 2 fields user prompted for.
        // ---------------------------------------------
        switch(AsChar(export.PromptDpCsePerson.Text1))
        {
          case ' ':
            break;
          case 'S':
            export.Phonetic.Flag = "Y";
            export.Phonetic.Percentage = 35;
            ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";

            return;
          default:
            var field = GetField(export.PromptDpCsePerson, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.PromptStatusTextWorkArea.Text1))
        {
          case ' ':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_LST_PAYMENT_STATUSES";

            return;
          default:
            var field = GetField(export.PromptStatusTextWorkArea, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        // --------------------------------------------------------
        // KD - 11/23/98
        // Atleast one promptable field must be selected
        // --------------------------------------------------------
        var field1 = GetField(export.PromptStatusTextWorkArea, "text1");

        field1.Error = true;

        var field2 = GetField(export.PromptCsePersonTextWorkArea, "text1");

        field2.Error = true;

        var field3 = GetField(export.PromptDpCsePerson, "text1");

        field3.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        break;
      case "DISPLAY":
        if (!IsEmpty(import.PromptCsePersonTextWorkArea.Text1))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          export.PromptCsePersonTextWorkArea.Text1 =
            import.PromptCsePersonTextWorkArea.Text1;

          var field = GetField(export.PromptCsePersonTextWorkArea, "text1");

          field.Error = true;

          return;
        }

        // -----------------------
        // KD - 10/26/99
        // New DP field.
        // -----------------------
        if (!IsEmpty(import.PromptDpCsePerson.Text1))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          export.PromptDpCsePerson.Text1 = import.PromptDpCsePerson.Text1;

          var field = GetField(export.PromptDpCsePerson, "text1");

          field.Error = true;

          return;
        }

        if (!IsEmpty(import.PromptStatusTextWorkArea.Text1))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          export.PromptStatusTextWorkArea.Text1 =
            import.PromptStatusTextWorkArea.Text1;

          var field = GetField(export.PromptStatusTextWorkArea, "text1");

          field.Error = true;

          return;
        }

        local.CsePersonNf.Flag = "";

        // ***** DETERMINE HOW THE LIST WILL BE SELECTED *****
        // ---------------------------------------
        // SWSRKXD
        // 10/26/99 - Add DP field to IF.
        // 11/23/99 - Add select by warrant number
        // --------------------------------------
        if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          // ***** Select by Payee ******
          local.SelectBy.Flag = "P";
        }
        else if (!IsEmpty(export.Dp.Number))
        {
          // ***** Select by DP ******
          local.SelectBy.Flag = "E";
        }
        else if (!IsEmpty(export.StartingWarrant.Number))
        {
          // ***** Select by Warrant Number ******
          local.SelectBy.Flag = "N";
        }
        else if (!IsEmpty(export.PaymentStatus.Code))
        {
          // ***** Select by Status
          local.SelectBy.Flag = "S";
        }

        // --------------------------------
        // KD - 11/23/98
        // Validate entered warrant number
        // --------------------------------
        if (!IsEmpty(export.StartingWarrant.Number))
        {
          if (!ReadPaymentRequest1())
          {
            ExitState = "FN0000_WARRANT_NUMBER_INVALID";

            var field = GetField(export.StartingWarrant, "number");

            field.Error = true;

            // ------------------------------------------------------------
            // KD - 12/29/98
            // Don't escape yet. Trap all errors at once.
            // --------------------------------------------------------------
          }
        }

        // ***** Default dates when left blank *****
        if (Equal(import.From.Date, null))
        {
          if (Equal(import.PaymentStatus.Code, "REQ") || Equal
            (import.PaymentStatus.Code, "REQCANCEL"))
          {
            // 5/14/2019 GVandy - CQ65809 - Display warrants in REQCANCEL 
            // status.
            export.From.Date = new DateTime(1, 1, 1);
          }
          else
          {
            // -------------------------------------------
            // KD - 10/28/99
            // PR# 78639 - Default search range to 1 month, if Payee or
            // DP has been entered. If both payee and DP are blank then
            // set from date to yesterday.
            // -------------------------------------------
            if (IsEmpty(export.CsePersonsWorkSet.Number) && IsEmpty
              (export.Dp.Number))
            {
              export.From.Date = Now().Date.AddDays(-1);
            }
            else
            {
              export.From.Date = Now().Date.AddMonths(-1);
            }
          }
        }

        if (Equal(import.To.Date, null))
        {
          if (Equal(import.PaymentStatus.Code, "REQ") || Equal
            (import.PaymentStatus.Code, "REQCANCEL"))
          {
            // 5/14/2019 GVandy - CQ65809 - Display warrants in REQCANCEL 
            // status.
            export.To.Date = Now().Date;
          }
          else
          {
            // -------------------------------------------
            // KD - 10/28/99
            // PR# 78639 - If both Payee and DP are blank, then set To
            // Date to yesterday, else today.
            // -------------------------------------------
            if (IsEmpty(export.CsePersonsWorkSet.Number) && IsEmpty
              (export.Dp.Number))
            {
              export.To.Date = Now().Date.AddDays(-1);
            }
            else
            {
              export.To.Date = Now().Date;
            }
          }
        }

        // -------------------------------------------
        // KD - 11/24/98
        // Incorporate date validation
        // From Date must be <= To date
        // Both dates must be <= Current Date
        // -------------------------------------------
        if (Lt(export.To.Date, export.From.Date))
        {
          ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";

          var field4 = GetField(export.From, "date");

          field4.Error = true;

          var field5 = GetField(export.To, "date");

          field5.Error = true;
        }

        if (Lt(Now().Date, export.From.Date))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          var field = GetField(export.From, "date");

          field.Error = true;
        }

        if (Lt(Now().Date, export.To.Date))
        {
          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          var field = GetField(export.To, "date");

          field.Error = true;
        }

        // ------------------------------------------------------------
        // KD - 12/29/98
        // Don't escape yet. Trap all errors at once.
        // --------------------------------------------------------------
        if (!IsEmpty(import.PaymentStatus.Code))
        {
          if (ReadPaymentStatus1())
          {
            // ***** We now have currency to qualify the future reads on.
            // --------------------------------------------------------
            // KD - 11/23/98
            // Need to display payment status name on the
            // screen.  Add the attribute view to the views and also the
            // move statement below
            // --------------------------------------------------------
            export.PaymentStatus.Assign(entities.SelectionCriteria);
          }
          else
          {
            var field = GetField(export.PaymentStatus, "code");

            field.Error = true;

            ExitState = "FN0000_INVALID_PAYMENT_STATUS";
          }
        }

        if (!IsEmpty(export.Dp.Number))
        {
          UseSiReadCsePerson3();

          if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.Dp, "number");

            field.Error = true;
          }
        }

        if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseSiReadCsePerson1();

          if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;
          }
        }

        // ---------------------------------------------
        // KD - 10/26/99
        // Only one of the 2 fields Payee or DP may be entered, never both!
        // ---------------------------------------------
        if (!IsEmpty(export.CsePersonsWorkSet.Number) && !
          IsEmpty(export.Dp.Number))
        {
          ExitState = "FN0000_ENTER_EITHER_PAYEE_OR_DP";

          var field4 = GetField(export.CsePersonsWorkSet, "number");

          field4.Error = true;

          var field5 = GetField(export.Dp, "number");

          field5.Error = true;
        }

        // ---------------------------
        // KD - 12/29/98
        // Add error handling
        // ---------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ----------------------------------------------------------
        // When neither Payee nor DP is entered, override 'To date'
        // and set it to 'From date + 1 week'. This is done to reduce
        // the number of retrieved warrants.
        // KD - 10/27
        // Add DP field in check.
        // ------------------------------------------------------------
        if (IsEmpty(export.CsePersonsWorkSet.Number) && IsEmpty
          (export.Dp.Number) && IsEmpty(export.StartingWarrant.Number) && Lt
          (export.From.Date, AddDays(export.To.Date, -7)))
        {
          if (Equal(import.PaymentStatus.Code, "REQ") || Equal
            (import.PaymentStatus.Code, "REQCANCEL"))
          {
            goto Test1;
          }

          // --------------------------------------------------------
          // KD - 12/29/98
          // Set flag, so a message can be displayed on screen
          // indicating that the date range has been limited.
          // --------------------------------------------------------
          local.DisplayDateMessage.Flag = "Y";
          export.To.Date = AddDays(export.From.Date, 7);
        }

Test1:

        // ------------------------------------------------------------
        // KD - 11/23/98
        //  When starting warrant number has been entered do not
        // apply the date filters - All warrants greater than and equal
        // to entered warrant # will be displayed. Set from and to dates
        // to min and max date respectively to avoid coding seperate
        // READ EACH statements.
        // -------------------------------------------------------------
        if (!IsEmpty(export.StartingWarrant.Number))
        {
          export.To.Date = new DateTime(9999, 12, 31);
          export.From.Date = new DateTime(1901, 1, 1);
        }

        switch(AsChar(local.SelectBy.Flag))
        {
          case 'N':
            // -------------------------------------------------
            // SWSRKXD PR#80747 11/23/99
            // When starting # has been entered, sort by Asc warrant #.
            // PR#93707 04/20/00
            // Remove Payee # and DP# qualification.
            // ------------------------------------------------
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadPaymentRequest4())
            {
              // -----------------------------------------------------------
              // Cater for Duplicate warrants. Display matching warrants only.
              // -----------------------------------------------------------
              if (AsChar(import.HiddenDuplicateWarrants.Flag) == 'Y' && Lt
                (export.StartingWarrant.Number, entities.PaymentRequest.Number))
              {
                export.Export1.Next();

                goto Test2;
              }

              // ***** If status was entered, filter by status ******
              if (!IsEmpty(export.PaymentStatus.Code))
              {
                if (!ReadPaymentStatusHistoryPaymentStatus())
                {
                  ExitState = "FN0000_PYMNT_STAT_HIST_NF";
                  export.Export1.Next();

                  return;
                }

                if (entities.ReadForId.SystemGeneratedIdentifier != entities
                  .SelectionCriteria.SystemGeneratedIdentifier)
                {
                  export.Export1.Next();

                  continue;
                }

                export.Export1.Update.DetailPaymentStatus.Code =
                  entities.SelectionCriteria.Code;
              }
              else
              {
                // -----------------------------------------------------
                // Status filter turned off. Retrieve current status of
                // warrant to populate display list.
                // ----------------------------------------------------
                if (!ReadPaymentStatus2())
                {
                  ExitState = "FN0000_PYMNT_STAT_NF";
                  export.Export1.Next();

                  return;
                }

                export.Export1.Update.DetailPaymentStatus.Code =
                  entities.ReadForCode.Code;
              }

              export.Export1.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);

              if (AsChar(entities.PaymentRequest.InterstateInd) != 'Y')
              {
                export.Export1.Update.DetailPaymentRequest.InterstateInd = "N";
              }

              if (ReadPaymentRequest3())
              {
                if (AsChar(entities.ReissuedFrom.InterstateInd) == 'Y')
                {
                  // The warrant was reissued from a previous warrant AND the 
                  // previous warrant was an interstate warrant so display the
                  // interstate indicator as 'Y'.
                  export.Export1.Update.DetailPaymentRequest.InterstateInd =
                    "Y";
                }
              }
              else
              {
                // The warrant was not reissued from a previous warrant so 
                // display the interstate indicator on the warrant.
              }

              if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
              {
                export.Export1.Update.DetailCsePersonsWorkSet.Number =
                  entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces
                  (10);
              }
              else
              {
                export.Export1.Update.DetailCsePersonsWorkSet.Number =
                  entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
              }

              UseSiReadCsePerson2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                  

                field.Color = "red";
                field.Protected = false;

                local.CsePersonNf.Flag = "Y";
                ExitState = "ACO_NN0000_ALL_OK";
              }

              export.Export1.Next();
            }

            break;
          case 'P':
            // **** This section handles search by Payee ****
            // --------------------------------------------------------
            // KD - 10/27/99
            // Performance change -  split read each.
            // --------------------------------------------------------
            // -------------------------------------------------
            // SWSRKXD PR#80747 11/23/99
            // Change sort order
            // PR#93707 04/20/00
            // Remove process_date and starting_warrant_number
            // qualification from READ EACH.
            // ------------------------------------------------
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadPaymentRequest7())
            {
              if (Lt(entities.PaymentRequest.Number,
                export.StartingWarrant.Number))
              {
                export.Export1.Next();

                continue;
              }

              // HLM - PR#191778 - 2/4/2004
              // Logic for checking for process_date between 'from' and 'to' 
              // values was located here,
              // prior to moving below logic checking for payment_status.
              // ***** Before loading the group view we will get the payment 
              // status of this payment request in case they are selecting
              // payments by status as well as by person.
              if (!ReadPaymentStatusHistory())
              {
                ExitState = "FN0000_PYMNT_STAT_HIST_NF";
                export.Export1.Next();

                return;
              }

              if (!ReadPaymentStatus3())
              {
                ExitState = "FN0000_PYMNT_STAT_NF";
                export.Export1.Next();

                return;
              }

              if (!IsEmpty(entities.SelectionCriteria.Code))
              {
                if (!Equal(entities.ReadForCode.Code,
                  entities.SelectionCriteria.Code))
                {
                  export.Export1.Next();

                  continue;
                }
              }

              // HLM - PR# 191778 - 2/4/2004
              // Put check for process_date here with additional qualifier for 
              // checking the payment_status
              //  for REQ (value 1). If REQ, then let "in range" process_date 
              // record be displayed (even null),
              //  but if not REQ ensure process_date is between the 'from' and '
              // to' dates entered
              //  (or the values that have been modified by prior range setting 
              // display logic).
              // 5/14/2019 GVandy - CQ65809 - Display warrants in REQCANCEL 
              // status.
              if (Lt(entities.PaymentRequest.ProcessDate, export.From.Date) || Lt
                (export.To.Date, entities.PaymentRequest.ProcessDate))
              {
                if (Equal(entities.ReadForCode.Code, "REQ") || Equal
                  (entities.ReadForCode.Code, "REQCANCEL"))
                {
                  if (!Equal(entities.PaymentRequest.ProcessDate,
                    local.NullDate.Date))
                  {
                    export.Export1.Next();

                    continue;
                  }
                }
                else
                {
                  export.Export1.Next();

                  continue;
                }
              }

              export.Export1.Update.DetailPaymentStatus.Code =
                entities.ReadForCode.Code;
              export.Export1.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);

              if (AsChar(entities.PaymentRequest.InterstateInd) != 'Y')
              {
                export.Export1.Update.DetailPaymentRequest.InterstateInd = "N";
              }

              if (ReadPaymentRequest3())
              {
                if (AsChar(entities.ReissuedFrom.InterstateInd) == 'Y')
                {
                  // The warrant was reissued from a previous warrant AND the 
                  // previous warrant was an interstate warrant so display the
                  // interstate indicator as 'Y'.
                  export.Export1.Update.DetailPaymentRequest.InterstateInd =
                    "Y";
                }
              }
              else
              {
                // The warrant was not reissued from a previous warrant so 
                // display the interstate indicator on the warrant.
              }

              if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
              {
                export.Export1.Update.DetailCsePersonsWorkSet.Number =
                  entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces
                  (10);
                UseSiReadCsePerson2();
              }
              else
              {
                export.Export1.Update.DetailCsePersonsWorkSet.Number =
                  entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
                export.Export1.Update.DetailCsePersonsWorkSet.FormattedName =
                  export.CsePersonsWorkSet.FormattedName;
              }

              export.Export1.Next();
            }

            break;
          case 'E':
            // **** This section handles search by DP ****
            // -------------------------------------------------
            // SWSRKXD PR#80747 11/23/99
            // Change sort order
            // PR#93707 04/20/00
            // Remove process_date and starting_warrant_number
            // qualification from READ EACH
            // ------------------------------------------------
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadPaymentRequest8())
            {
              if (Lt(entities.PaymentRequest.Number,
                export.StartingWarrant.Number))
              {
                export.Export1.Next();

                continue;
              }

              // HLM - PR#191778 - 2/4/2004
              // Logic for checking for process_date between 'from' and 'to' 
              // values was located here,
              // prior to moving below logic checking for payment_status.
              // ***** Before loading the group view we will get the payment 
              // status of this payment request in case they are selecting
              // payments by status as well as by person.
              if (!ReadPaymentStatusHistory())
              {
                ExitState = "FN0000_PYMNT_STAT_HIST_NF";
                export.Export1.Next();

                return;
              }

              if (!ReadPaymentStatus3())
              {
                ExitState = "FN0000_PYMNT_STAT_NF";
                export.Export1.Next();

                return;
              }

              if (!IsEmpty(entities.SelectionCriteria.Code))
              {
                if (!Equal(entities.ReadForCode.Code,
                  entities.SelectionCriteria.Code))
                {
                  export.Export1.Next();

                  continue;
                }
              }

              // HLM - PR# 191778 - 2/4/2004
              // Put check for process_date here with additional qualifier for 
              // checking the payment_status
              //  for REQ (value 1). If REQ, then let "in range" process_date 
              // record be displayed (even null),
              //  but if not REQ ensure process_date is between the 'from' and '
              // to' dates entered
              //  (or the values that have been modified by prior range setting 
              // display logic).
              // 5/14/2019 GVandy - CQ65809 - Display warrants in REQCANCEL 
              // status.
              if (Lt(entities.PaymentRequest.ProcessDate, export.From.Date) || Lt
                (export.To.Date, entities.PaymentRequest.ProcessDate))
              {
                if (Equal(entities.ReadForCode.Code, "REQ") || Equal
                  (entities.ReadForCode.Code, "REQCANCEL"))
                {
                  if (!Equal(entities.PaymentRequest.ProcessDate,
                    local.NullDate.Date))
                  {
                    export.Export1.Next();

                    continue;
                  }
                }
                else
                {
                  export.Export1.Next();

                  continue;
                }
              }

              export.Export1.Update.DetailPaymentStatus.Code =
                entities.ReadForCode.Code;
              export.Export1.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);

              if (AsChar(entities.PaymentRequest.InterstateInd) != 'Y')
              {
                export.Export1.Update.DetailPaymentRequest.InterstateInd = "N";
              }

              if (ReadPaymentRequest3())
              {
                if (AsChar(entities.ReissuedFrom.InterstateInd) == 'Y')
                {
                  // The warrant was reissued from a previous warrant AND the 
                  // previous warrant was an interstate warrant so display the
                  // interstate indicator as 'Y'.
                  export.Export1.Update.DetailPaymentRequest.InterstateInd =
                    "Y";
                }
              }
              else
              {
                // The warrant was not reissued from a previous warrant so 
                // display the interstate indicator on the warrant.
              }

              export.Export1.Update.DetailCsePersonsWorkSet.Number =
                entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces
                (10);
              export.Export1.Update.DetailCsePersonsWorkSet.FormattedName =
                export.Dp.FormattedName;
              export.Export1.Next();
            }

            break;
          case 'S':
            // 5/14/2019 GVandy - CQ65809 - Display warrants in REQCANCEL 
            // status.
            // ***** This section handles the request of warrants by Status.
            // -------------------------------------------------
            // SWSRKXD PR#80747 11/23/99
            // Change sort order
            // PR#93707 04/20/00
            // Remove starting_warrant_number qualification from READ
            // EACH
            // ------------------------------------------------
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadPaymentRequest5())
            {
              export.Export1.Update.DetailPaymentStatus.Code =
                entities.SelectionCriteria.Code;
              export.Export1.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);

              if (AsChar(entities.PaymentRequest.InterstateInd) != 'Y')
              {
                export.Export1.Update.DetailPaymentRequest.InterstateInd = "N";
              }

              if (ReadPaymentRequest3())
              {
                if (AsChar(entities.ReissuedFrom.InterstateInd) == 'Y')
                {
                  // The warrant was reissued from a previous warrant AND the 
                  // previous warrant was an interstate warrant so display the
                  // interstate indicator as 'Y'.
                  export.Export1.Update.DetailPaymentRequest.InterstateInd =
                    "Y";
                }
              }
              else
              {
                // The warrant was not reissued from a previous warrant so 
                // display the interstate indicator on the warrant.
              }

              if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
              {
                export.Export1.Update.DetailCsePersonsWorkSet.Number =
                  entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces
                  (10);
              }
              else
              {
                export.Export1.Update.DetailCsePersonsWorkSet.Number =
                  entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
              }

              UseSiReadCsePerson2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                  

                field.Color = "red";
                field.Protected = false;

                local.CsePersonNf.Flag = "Y";
                ExitState = "ACO_NN0000_ALL_OK";
              }

              export.Export1.Next();
            }

            if (AsChar(local.CsePersonNf.Flag) == 'Y')
            {
              ExitState = "FN0000_DISP_SUCC_DATA_ERROR";
            }

            break;
          default:
            // ***** This section handles the request of warrants by Date.
            // -------------------------------------------------
            // SWSRKXD PR#80747 11/23/99
            // Change sort order
            // PR#93707 04/20/00
            // Remove starting_warrant_number qualification from READ
            // EACH
            // ------------------------------------------------
            export.Export1.Index = 0;
            export.Export1.Clear();

            foreach(var item in ReadPaymentRequest6())
            {
              if (ReadPaymentStatusHistoryPaymentStatus())
              {
                if (AsChar(local.SelectBy.Flag) == 'S')
                {
                  // ***** This section handles the request of warrants by 
                  // Status.
                  if (entities.ReadForId.SystemGeneratedIdentifier == entities
                    .SelectionCriteria.SystemGeneratedIdentifier)
                  {
                    export.Export1.Update.DetailPaymentStatus.Code =
                      entities.SelectionCriteria.Code;
                  }
                  else
                  {
                    export.Export1.Next();

                    continue;
                  }
                }
                else
                {
                  // ***** This section handles the request of warrants by Date.
                  if (ReadPaymentStatus3())
                  {
                    export.Export1.Update.DetailPaymentStatus.Code =
                      entities.ReadForCode.Code;
                  }
                  else
                  {
                    ExitState = "FN0000_PYMNT_STAT_NF";
                    export.Export1.Next();

                    return;
                  }
                }

                export.Export1.Update.DetailPaymentRequest.Assign(
                  entities.PaymentRequest);

                if (AsChar(entities.PaymentRequest.InterstateInd) != 'Y')
                {
                  export.Export1.Update.DetailPaymentRequest.InterstateInd =
                    "N";
                }

                if (ReadPaymentRequest3())
                {
                  if (AsChar(entities.ReissuedFrom.InterstateInd) == 'Y')
                  {
                    // The warrant was reissued from a previous warrant AND the 
                    // previous warrant was an interstate warrant so display the
                    // interstate indicator as 'Y'.
                    export.Export1.Update.DetailPaymentRequest.InterstateInd =
                      "Y";
                  }
                }
                else
                {
                  // The warrant was not reissued from a previous warrant so 
                  // display the interstate indicator on the warrant.
                }
              }
              else
              {
                ExitState = "FN0000_PYMNT_STAT_HIST_NF";
                export.Export1.Next();

                return;
              }

              if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
              {
                export.Export1.Update.DetailCsePersonsWorkSet.Number =
                  entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces
                  (10);
              }
              else
              {
                export.Export1.Update.DetailCsePersonsWorkSet.Number =
                  entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
              }

              UseSiReadCsePerson2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonsWorkSet, "number");
                  

                field.Color = "red";
                field.Protected = false;

                local.CsePersonNf.Flag = "Y";
                ExitState = "ACO_NN0000_ALL_OK";
              }

              export.Export1.Next();
            }

            if (AsChar(local.CsePersonNf.Flag) == 'Y')
            {
              ExitState = "FN0000_DISP_SUCC_DATA_ERROR";
            }

            break;
        }

Test2:

        // 5/14/2019 GVandy - CQ65809 - Display warrants in REQCANCEL status.
        if (Equal(export.From.Date, new DateTime(1, 1, 1)))
        {
          export.From.Date = null;
        }

        if (Equal(export.To.Date, new DateTime(1, 1, 1)))
        {
          export.To.Date = null;
        }

        // -----------------------------------------------------------
        // KD - 11/23/98
        // Ignore date filters when starting warrant number has been entered
        // ------------------------------------------------------------
        if (!IsEmpty(export.StartingWarrant.Number))
        {
          export.From.Date = null;
          export.To.Date = null;
        }

        // -----------------------------------------------------------
        // KD - 11/23/98
        // Display message when array is empty
        // ------------------------------------------------------------
        if (export.Export1.IsEmpty)
        {
          if (AsChar(local.DisplayDateMessage.Flag) == 'Y')
          {
            ExitState = "FN0000_WARR_NF_DATE_RANGE_RED";
          }
          else
          {
            ExitState = "NO_WARRANTS_FOUND";
          }

          return;
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";

          return;
        }

        // -----------------------------------------------------------
        // KD - 12/29/98
        // If Date range has been limited to 2 months display message
        // ------------------------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK") && AsChar
          (local.DisplayDateMessage.Flag) == 'Y')
        {
          ExitState = "FN0000_SEARCH_DATE_RANGE_REDUCED";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(import.HiddenDuplicateWarrants.Flag) == 'Y')
          {
            ExitState = "FN0000_DUPLICATE_WARR_SELECT_ONE";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }

          // -----------------------------------------------------------
          // After successful display, place cursor on the first row.
          // ------------------------------------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;

            return;
          }
        }

        break;
      case "AP_ACC":
        ExitState = "ECO_LNK_TO_LST_APACC";

        break;
      case "OBTL":
        ExitState = "ECO_LNK_TO_LST_OBLIGATION_DETAIL";

        break;
      case "PACC":
        // --------------------------------------------------------
        // KD - 12/30/98
        // When flowing to PACC and a DP is setup for the selected
        // warrant, display message indicating that user is required to
        // flow to WDTL. From there user can then PFKey to PACC.
        // This is a temp fix until DP field can be added to the list on
        // the screen.
        // -----------------------------------------------------------
        if (export.PassThruFlowPaymentRequest.SystemGeneratedIdentifier != 0)
        {
          if (!ReadPaymentRequest2())
          {
            ExitState = "FN0000_PAYMENT_REQUEST_NF";

            return;
          }

          if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
          {
            ExitState = "FN0000_FLOW_TO_WDTL_TO_GOTO_PACC";

            return;
          }
        }

        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      case "WDTL":
        ExitState = "ECO_LNK_TO_WDTL";

        break;
      case "WAST":
        ExitState = "ECO_LNK_TO_WAST";

        break;
      case "WHST":
        ExitState = "ECO_LNK_TO_WHST";

        break;
      case "WARA":
        ExitState = "ECO_LNK_TO_WARA";

        break;
      case "RETURN":
        if (local.Common.Count > 1)
        {
          ExitState = "ZD_ACO_NE0_INVALID_MULTIPLE_SEL1";

          return;
        }

        // ----------------------------------------------------
        // 05/22/01 K. Doshi  WR# 285.
        // Cater for duplicate warrants.
        // ---------------------------------------------------
        if (local.Common.Count == 0)
        {
          if (AsChar(import.HiddenDuplicateWarrants.Flag) == 'Y' && !
            export.Export1.IsEmpty)
          {
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              return;
            }
          }

          // <<< RBM  12/11/1997  If nothing was selected, then pass the 
          // Starting Warrant number back >>>
          export.PassThruFlowPaymentRequest.Number =
            export.StartingWarrant.Number ?? "";
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY_3";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MovePaymentRequest1(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.CsePersonNumber = source.CsePersonNumber;
    target.Number = source.Number;
  }

  private static void MovePaymentRequest2(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaximumDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaximumDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Export1.Item.DetailCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.Export1.Update.DetailCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Dp.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Dp);
  }

  private bool ReadPaymentRequest1()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", export.StartingWarrant.Number ?? "");
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest2()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          export.PassThruFlowPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest3()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentRequest.Populated);
    entities.ReissuedFrom.Populated = false;

    return Read("ReadPaymentRequest3",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.PrqRGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReissuedFrom.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReissuedFrom.PrqRGeneratedId = db.GetNullableInt32(reader, 1);
        entities.ReissuedFrom.InterstateInd = db.GetNullableString(reader, 2);
        entities.ReissuedFrom.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest4()
  {
    return ReadEach("ReadPaymentRequest4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", export.StartingWarrant.Number ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest5()
  {
    return ReadEach("ReadPaymentRequest5",
      (db, command) =>
      {
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.To.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDate.Date.GetValueOrDefault());
        db.SetInt32(
          command, "pstGeneratedId",
          entities.SelectionCriteria.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest6()
  {
    return ReadEach("ReadPaymentRequest6",
      (db, command) =>
      {
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.To.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest7()
  {
    return ReadEach("ReadPaymentRequest7",
      (db, command) =>
      {
        db.SetNullableString(
          command, "csePersonNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest8()
  {
    return ReadEach("ReadPaymentRequest8",
      (db, command) =>
      {
        db.SetNullableString(command, "dpCsePerNum", export.Dp.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private bool ReadPaymentStatus1()
  {
    entities.SelectionCriteria.Populated = false;

    return Read("ReadPaymentStatus1",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
      },
      (db, reader) =>
      {
        entities.SelectionCriteria.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.SelectionCriteria.Code = db.GetString(reader, 1);
        entities.SelectionCriteria.Name = db.GetString(reader, 2);
        entities.SelectionCriteria.Description =
          db.GetNullableString(reader, 3);
        entities.SelectionCriteria.Populated = true;
      });
  }

  private bool ReadPaymentStatus2()
  {
    entities.ReadForCode.Populated = false;

    return Read("ReadPaymentStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReadForCode.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ReadForCode.Code = db.GetString(reader, 1);
        entities.ReadForCode.Populated = true;
      });
  }

  private bool ReadPaymentStatus3()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);
    entities.ReadForCode.Populated = false;

    return Read("ReadPaymentStatus3",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          entities.PaymentStatusHistory.PstGeneratedId);
      },
      (db, reader) =>
      {
        entities.ReadForCode.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ReadForCode.Code = db.GetString(reader, 1);
        entities.ReadForCode.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDate.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistoryPaymentStatus()
  {
    entities.ReadForId.Populated = false;
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistoryPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaximumDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.ReadForId.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ReadForId.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailPaymentRequest.
      /// </summary>
      [JsonPropertyName("detailPaymentRequest")]
      public PaymentRequest DetailPaymentRequest
      {
        get => detailPaymentRequest ??= new();
        set => detailPaymentRequest = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("detailPaymentStatusHistory")]
      public PaymentStatusHistory DetailPaymentStatusHistory
      {
        get => detailPaymentStatusHistory ??= new();
        set => detailPaymentStatusHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 115;

      private Common detailCommon;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private PaymentRequest detailPaymentRequest;
      private PaymentStatus detailPaymentStatus;
      private PaymentStatusHistory detailPaymentStatusHistory;
    }

    /// <summary>
    /// A value of PromptDpCsePerson.
    /// </summary>
    [JsonPropertyName("promptDpCsePerson")]
    public TextWorkArea PromptDpCsePerson
    {
      get => promptDpCsePerson ??= new();
      set => promptDpCsePerson = value;
    }

    /// <summary>
    /// A value of Dp.
    /// </summary>
    [JsonPropertyName("dp")]
    public CsePersonsWorkSet Dp
    {
      get => dp ??= new();
      set => dp = value;
    }

    /// <summary>
    /// A value of PromptStatusTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptStatusTextWorkArea")]
    public TextWorkArea PromptStatusTextWorkArea
    {
      get => promptStatusTextWorkArea ??= new();
      set => promptStatusTextWorkArea = value;
    }

    /// <summary>
    /// A value of PromptCsePersonTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptCsePersonTextWorkArea")]
    public TextWorkArea PromptCsePersonTextWorkArea
    {
      get => promptCsePersonTextWorkArea ??= new();
      set => promptCsePersonTextWorkArea = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CsePersonsWorkSet Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of StartingWarrant.
    /// </summary>
    [JsonPropertyName("startingWarrant")]
    public PaymentRequest StartingWarrant
    {
      get => startingWarrant ??= new();
      set => startingWarrant = value;
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
    /// A value of PromptStatusCommon.
    /// </summary>
    [JsonPropertyName("promptStatusCommon")]
    public Common PromptStatusCommon
    {
      get => promptStatusCommon ??= new();
      set => promptStatusCommon = value;
    }

    /// <summary>
    /// A value of PromptCsePersonCommon.
    /// </summary>
    [JsonPropertyName("promptCsePersonCommon")]
    public Common PromptCsePersonCommon
    {
      get => promptCsePersonCommon ??= new();
      set => promptCsePersonCommon = value;
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
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of HiddenDuplicateWarrants.
    /// </summary>
    [JsonPropertyName("hiddenDuplicateWarrants")]
    public Common HiddenDuplicateWarrants
    {
      get => hiddenDuplicateWarrants ??= new();
      set => hiddenDuplicateWarrants = value;
    }

    private TextWorkArea promptDpCsePerson;
    private CsePersonsWorkSet dp;
    private TextWorkArea promptStatusTextWorkArea;
    private TextWorkArea promptCsePersonTextWorkArea;
    private CsePersonsWorkSet pass;
    private PaymentRequest startingWarrant;
    private PaymentRequest paymentRequest;
    private Common promptStatusCommon;
    private Common promptCsePersonCommon;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea from;
    private DateWorkArea to;
    private PaymentStatus paymentStatus;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePerson csePerson;
    private Common hiddenDuplicateWarrants;
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
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailPaymentRequest.
      /// </summary>
      [JsonPropertyName("detailPaymentRequest")]
      public PaymentRequest DetailPaymentRequest
      {
        get => detailPaymentRequest ??= new();
        set => detailPaymentRequest = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("detailPaymentStatusHistory")]
      public PaymentStatusHistory DetailPaymentStatusHistory
      {
        get => detailPaymentStatusHistory ??= new();
        set => detailPaymentStatusHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 115;

      private Common detailCommon;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private PaymentRequest detailPaymentRequest;
      private PaymentStatus detailPaymentStatus;
      private PaymentStatusHistory detailPaymentStatusHistory;
    }

    /// <summary>
    /// A value of PromptDpCsePerson.
    /// </summary>
    [JsonPropertyName("promptDpCsePerson")]
    public TextWorkArea PromptDpCsePerson
    {
      get => promptDpCsePerson ??= new();
      set => promptDpCsePerson = value;
    }

    /// <summary>
    /// A value of Dp.
    /// </summary>
    [JsonPropertyName("dp")]
    public CsePersonsWorkSet Dp
    {
      get => dp ??= new();
      set => dp = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of PromptStatusTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptStatusTextWorkArea")]
    public TextWorkArea PromptStatusTextWorkArea
    {
      get => promptStatusTextWorkArea ??= new();
      set => promptStatusTextWorkArea = value;
    }

    /// <summary>
    /// A value of PromptCsePersonTextWorkArea.
    /// </summary>
    [JsonPropertyName("promptCsePersonTextWorkArea")]
    public TextWorkArea PromptCsePersonTextWorkArea
    {
      get => promptCsePersonTextWorkArea ??= new();
      set => promptCsePersonTextWorkArea = value;
    }

    /// <summary>
    /// A value of PassThruFlowCsePerson.
    /// </summary>
    [JsonPropertyName("passThruFlowCsePerson")]
    public CsePerson PassThruFlowCsePerson
    {
      get => passThruFlowCsePerson ??= new();
      set => passThruFlowCsePerson = value;
    }

    /// <summary>
    /// A value of PassThruFlowPaymentRequest.
    /// </summary>
    [JsonPropertyName("passThruFlowPaymentRequest")]
    public PaymentRequest PassThruFlowPaymentRequest
    {
      get => passThruFlowPaymentRequest ??= new();
      set => passThruFlowPaymentRequest = value;
    }

    /// <summary>
    /// A value of StartingWarrant.
    /// </summary>
    [JsonPropertyName("startingWarrant")]
    public PaymentRequest StartingWarrant
    {
      get => startingWarrant ??= new();
      set => startingWarrant = value;
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
    /// A value of PromptStatusCommon.
    /// </summary>
    [JsonPropertyName("promptStatusCommon")]
    public Common PromptStatusCommon
    {
      get => promptStatusCommon ??= new();
      set => promptStatusCommon = value;
    }

    /// <summary>
    /// A value of PromptCsePersonCommon.
    /// </summary>
    [JsonPropertyName("promptCsePersonCommon")]
    public Common PromptCsePersonCommon
    {
      get => promptCsePersonCommon ??= new();
      set => promptCsePersonCommon = value;
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
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of HiddenDuplicateWarrants.
    /// </summary>
    [JsonPropertyName("hiddenDuplicateWarrants")]
    public Common HiddenDuplicateWarrants
    {
      get => hiddenDuplicateWarrants ??= new();
      set => hiddenDuplicateWarrants = value;
    }

    private TextWorkArea promptDpCsePerson;
    private CsePersonsWorkSet dp;
    private Common phonetic;
    private TextWorkArea promptStatusTextWorkArea;
    private TextWorkArea promptCsePersonTextWorkArea;
    private CsePerson passThruFlowCsePerson;
    private PaymentRequest passThruFlowPaymentRequest;
    private PaymentRequest startingWarrant;
    private PaymentRequest paymentRequest;
    private Common promptStatusCommon;
    private Common promptCsePersonCommon;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea from;
    private DateWorkArea to;
    private PaymentStatus paymentStatus;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePerson csePerson;
    private Common hiddenDuplicateWarrants;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of DisplayDateMessage.
    /// </summary>
    [JsonPropertyName("displayDateMessage")]
    public Common DisplayDateMessage
    {
      get => displayDateMessage ??= new();
      set => displayDateMessage = value;
    }

    /// <summary>
    /// A value of CsePersonNf.
    /// </summary>
    [JsonPropertyName("csePersonNf")]
    public Common CsePersonNf
    {
      get => csePersonNf ??= new();
      set => csePersonNf = value;
    }

    /// <summary>
    /// A value of SelectBy.
    /// </summary>
    [JsonPropertyName("selectBy")]
    public Common SelectBy
    {
      get => selectBy ??= new();
      set => selectBy = value;
    }

    /// <summary>
    /// A value of MaximumDate.
    /// </summary>
    [JsonPropertyName("maximumDate")]
    public DateWorkArea MaximumDate
    {
      get => maximumDate ??= new();
      set => maximumDate = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private DateWorkArea nullDate;
    private Common displayDateMessage;
    private Common csePersonNf;
    private Common selectBy;
    private DateWorkArea maximumDate;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea dateWorkArea;
    private Common common;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReissuedFrom.
    /// </summary>
    [JsonPropertyName("reissuedFrom")]
    public PaymentRequest ReissuedFrom
    {
      get => reissuedFrom ??= new();
      set => reissuedFrom = value;
    }

    /// <summary>
    /// A value of ReadForId.
    /// </summary>
    [JsonPropertyName("readForId")]
    public PaymentStatus ReadForId
    {
      get => readForId ??= new();
      set => readForId = value;
    }

    /// <summary>
    /// A value of SelectionCriteria.
    /// </summary>
    [JsonPropertyName("selectionCriteria")]
    public PaymentStatus SelectionCriteria
    {
      get => selectionCriteria ??= new();
      set => selectionCriteria = value;
    }

    /// <summary>
    /// A value of Asdf.
    /// </summary>
    [JsonPropertyName("asdf")]
    public CsePerson Asdf
    {
      get => asdf ??= new();
      set => asdf = value;
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
    /// A value of ReadForCode.
    /// </summary>
    [JsonPropertyName("readForCode")]
    public PaymentStatus ReadForCode
    {
      get => readForCode ??= new();
      set => readForCode = value;
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

    private PaymentRequest reissuedFrom;
    private PaymentStatus readForId;
    private PaymentStatus selectionCriteria;
    private CsePerson asdf;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus readForCode;
    private PaymentRequest paymentRequest;
  }
#endregion
}
