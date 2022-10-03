// Program: SI_NCOP_NCP_NON_COOPERATION, ID: 1902537274, model: 746.
// Short name: SWENCOPP
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
/// A program: SI_NCOP_NCP_NON_COOPERATION.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiNcopNcpNonCooperation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_NCOP_NCP_NON_COOPERATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiNcopNcpNonCooperation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiNcopNcpNonCooperation.
  /// </summary>
  public SiNcopNcpNonCooperation(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	    Developer		Description
    // 03-16-2016  D Dupree		Initial Development
    ExitState = "ACO_NN0000_ALL_OK";
    MoveNextTranInfo(import.HiddenNextTranInfo, export.HiddenNextTranInfo);
    local.Current.Date = Now().Date;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    UseCabSetMaximumDiscontinueDate();

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Next.Number = import.Next.Number;
    export.Case1.Number = import.Case1.Number;
    export.HiddenCase.Number = import.HiddenCase.Number;
    export.Ap.Assign(import.Ap);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    export.HiddenRedisplay.Flag = import.HiddenRedisplay.Flag;
    export.DesignatedPayeeFnd.Flag = import.DesignatedPayeeFnd.Flag;
    export.ApPrompt.SelectChar = import.ApPrompt.SelectChar;
    MoveOffice(import.Office, export.Office);
    export.HeaderLine.Text35 = import.HeaderLine.Text35;
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.New1.Assign(import.New1);
    MoveNcpNonCooperation3(import.Last, export.Last);
    export.NewAp.Number = import.NewAp.Number;
    export.NewSelect.SelectChar = import.NewSelect.SelectChar;
    export.NewPersonPrompt.SelectChar = import.NewPersonPrompt.SelectChar;
    export.NewLetter1CdPrmt.SelectChar = import.NewLetter1CdPrmt.SelectChar;
    export.NewLetter2CdPrmt.SelectChar = import.NewLetter2CdPrmt.SelectChar;
    export.NewPhone1CdPrmt.SelectChar = import.NewPhone1CdPrmt.SelectChar;
    export.NewPhone2CdPrmt.SelectChar = import.NewPhone2CdPrmt.SelectChar;
    export.SuccessfullyDisplayed.Flag = import.SuccessfullyDisplayed.Flag;

    if (!IsEmpty(export.HiddenRedisplay.Flag))
    {
      if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "EXIT") || Equal
        (global.Command, "SIGNOFF") || Equal(global.Command, "ENTER") || Equal
        (global.Command, "ADD"))
      {
      }
      else
      {
        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }
    }

    // initialize Non-Coop Group  export view
    for(import.Nc.Index = 0; import.Nc.Index < import.Nc.Count; ++
      import.Nc.Index)
    {
      if (!import.Nc.CheckSize())
      {
        break;
      }

      export.Nc.Index = import.Nc.Index;
      export.Nc.CheckSize();

      export.Nc.Update.Select.SelectChar = import.Nc.Item.NcSelect.SelectChar;
      export.Nc.Update.NcAp.Number = import.Nc.Item.Ap.Number;
      export.Nc.Update.NcRsnPrompt.SelectChar =
        import.Nc.Item.ReasonPrompt.SelectChar;
      export.Nc.Update.NcpNonCooperation.
        Assign(import.Nc.Item.NcpNonCooperation);
      export.Nc.Update.EffDate.Date = import.Nc.Item.EffDate.Date;
      export.Nc.Update.EndDate.Date = import.Nc.Item.EndDate.Date;
      export.Nc.Update.Ltr1Promt.SelectChar =
        import.Nc.Item.Ltr1Prompt.SelectChar;
      export.Nc.Update.Ltr2Prompt.SelectChar =
        import.Nc.Item.Ltr2Prompt.SelectChar;
      export.Nc.Update.Phone1Prompt.SelectChar =
        import.Nc.Item.Phone1Prompt.SelectChar;
      export.Nc.Update.Phone2Prompt.SelectChar =
        import.Nc.Item.Phone2Prompt.SelectChar;
      export.Nc.Update.EndReasonPrompt.SelectChar =
        import.Nc.Item.EndReasonPrompt.SelectChar;
    }

    import.Nc.CheckIndex();
    export.NcMinus.OneChar = import.NcMinus.OneChar;
    export.NcPlus.OneChar = import.NcPlus.OneChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!import.Pagenum.IsEmpty)
    {
      for(import.Pagenum.Index = 0; import.Pagenum.Index < import
        .Pagenum.Count; ++import.Pagenum.Index)
      {
        if (!import.Pagenum.CheckSize())
        {
          break;
        }

        export.Pagenum.Index = import.Pagenum.Index;
        export.Pagenum.CheckSize();

        MoveNcpNonCooperation3(import.Pagenum.Item.Pagenum1,
          export.Pagenum.Update.Pagenum1);
      }

      import.Pagenum.CheckIndex();
    }

    // ---------------------------------------------
    //         	Move hidden views
    // ---------------------------------------------
    export.CurrItmNoNc.Count = import.CurrItmNoNc.Count;
    export.HidNoItemsFndNc.Count = import.HidNoItemsFndNc.Count;
    export.HiddenPrev.Number = import.HiddenPrev.Number;
    export.Prev.Number = import.Prev.Number;
    export.NoItemsNc.Count = import.NoItemsNc.Count;
    export.MaxPagesNc.Count = import.MaxPagesNc.Count;
    export.Nc1.PageNumber = import.Nc1.PageNumber;

    if (Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.ApPrompt.SelectChar) == 'S')
      {
        export.ApPrompt.SelectChar = "";
      }
    }
    else if (!import.HiddenGrpNc.IsEmpty)
    {
      for(import.HiddenGrpNc.Index = 0; import.HiddenGrpNc.Index < import
        .HiddenGrpNc.Count; ++import.HiddenGrpNc.Index)
      {
        if (!import.HiddenGrpNc.CheckSize())
        {
          break;
        }

        export.HiddenGrpNc.Index = import.HiddenGrpNc.Index;
        export.HiddenGrpNc.CheckSize();

        export.HiddenGrpNc.Update.HiddenGrpNcAp.Number =
          import.HiddenGrpNc.Item.HiddenGrpNcDetAp.Number;
        export.HiddenGrpNc.Update.Hidden.Assign(import.HiddenGrpNc.Item.Hidden);
        export.HiddenGrpNc.Update.HiddenEffDate.Date =
          import.HiddenGrpNc.Item.HiddenEffDate.Date;
        export.HiddenGrpNc.Update.HiddenEndDate.Date =
          import.HiddenGrpNc.Item.HiddenEndDate.Date;
      }

      import.HiddenGrpNc.CheckIndex();
    }

    // ============================================================
    // NEXTTRAN AND SECURITY  LOGIC
    // ============================================================
    // If the next tran info is not equal to spaces, this implies
    // the user requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Case1.Number;
      export.HiddenNextTranInfo.CsePersonNumber = export.Ap.Number;

      // >>
      // 05/22/02 TB start
      export.HiddenNextTranInfo.CsePersonNumberAp = export.Ap.Number;

      // >>
      // 05/22/02 TB end
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // this is where you set your export value to the export hidden
      // next tran values if the user is comming into this procedure
      // on a next tran action.
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          local.LastTran.SystemGeneratedIdentifier =
            export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
          UseOeCabReadInfrastructure();
          export.Ap.Number = local.LastTran.CsePersonNumber ?? Spaces(10);
          export.Next.Number = local.LastTran.CaseNumber ?? Spaces(10);
        }
        else
        {
          export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
            (10);
          export.Case1.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
            (10);
        }

        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // IF SELCTION IS MADE NO SCROLLING WILL BE ALLOWED
    if (local.GrpNcCnt.Count > 0)
    {
      switch(TrimEnd(global.Command))
      {
        case "NEXT":
          break;
        case "PREV":
          break;
        default:
          goto Test1;
      }

      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";
    }

Test1:

    if (Equal(global.Command, "RETCOMP"))
    {
      if (AsChar(export.ApPrompt.SelectChar) == 'S')
      {
        export.ApPrompt.SelectChar = "";

        if (!IsEmpty(import.SelectedAp.Number))
        {
          MoveCsePersonsWorkSet(import.SelectedAp, export.Ap);
        }

        global.Command = "DISPLAY";
      }

      if (AsChar(export.NewPersonPrompt.SelectChar) == 'S')
      {
        export.NewPersonPrompt.SelectChar = "";

        if (!IsEmpty(import.SelectedAp.Number))
        {
          export.NewAp.Number = import.SelectedAp.Number;
        }

        return;
      }
    }

    // -----------------------------------------------------
    // Changed security to check on CRUD actions only.
    // ------------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // mjr---> Changed this escape to exit the PrAD.
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "RTLIST":
        // ----------------------------------------------------------
        // On return from list(code table etc), populate the retreive
        // data to field on the screen.
        // ----------------------------------------------------------
        if (AsChar(export.DuplicateIndPromptDel.SelectChar) == 'S')
        {
          // -------------------------------------------------------------------
          // 07/18/00 W.Campbell - Added the following
          // code to replaced the above code with different logic.
          // Work done on PR#98232.
          // -------------------------------------------------------------------
          if (!IsEmpty(import.Selected.Description))
          {
            export.Case1.DuplicateCaseIndicator = import.Selected.Cdvalue;

            if (AsChar(export.Case1.DuplicateCaseIndicator) == 'S')
            {
              // -------------------------------------------------------------------
              // 07/18/00 W.Campbell - 'S' for 'S'PACE
              // from the code table.
              // -------------------------------------------------------------------
              export.Case1.DuplicateCaseIndicator = "";
            }
          }

          // -------------------------------------------------------------------
          // 07/18/00 W.Campbell - End of Added code
          // to replaced the above code with different logic.
          // Work done on PR#98232.
          // -------------------------------------------------------------------
          export.DuplicateIndPromptDel.SelectChar = "";
        }

        if (AsChar(export.NewLetter1CdPrmt.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.New1.Letter1Code = import.Selected.Cdvalue;
          }

          export.NewLetter1CdPrmt.SelectChar = "";

          var field = GetField(export.NewLetter1CdPrmt, "selectChar");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;
        }

        if (AsChar(export.NewLetter2CdPrmt.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.New1.Letter2Code = import.Selected.Cdvalue;
          }

          export.NewLetter2CdPrmt.SelectChar = "";

          var field = GetField(export.NewLetter2CdPrmt, "selectChar");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;
        }

        if (AsChar(export.NewPhone1CdPrmt.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.New1.Phone1Code = import.Selected.Cdvalue;
          }

          export.NewPhone1CdPrmt.SelectChar = "";

          var field = GetField(export.NewPhone1CdPrmt, "selectChar");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;
        }

        if (AsChar(export.NewPhone2CdPrmt.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.New1.Phone2Code = import.Selected.Cdvalue;
          }

          export.NewPhone2CdPrmt.SelectChar = "";

          var field = GetField(export.NewPhone2CdPrmt, "selectChar");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;
        }

        if (!export.Nc.IsEmpty)
        {
          // Validate Non-Coop Prompt
          for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
            export.Nc.Index)
          {
            if (!export.Nc.CheckSize())
            {
              break;
            }

            if (AsChar(export.Nc.Item.NcRsnPrompt.SelectChar) == 'S')
            {
              if (!IsEmpty(import.Selected.Cdvalue))
              {
                export.Nc.Update.NcpNonCooperation.ReasonCode =
                  import.Selected.Cdvalue;
              }

              export.Nc.Update.NcRsnPrompt.SelectChar = "";

              var field = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }

            if (AsChar(export.Nc.Item.EndReasonPrompt.SelectChar) == 'S')
            {
              if (!IsEmpty(import.Selected.Cdvalue))
              {
                export.Nc.Update.NcpNonCooperation.EndStatusCode =
                  import.Selected.Cdvalue;
              }

              export.Nc.Update.EndReasonPrompt.SelectChar = "";

              var field =
                GetField(export.Nc.Item.EndReasonPrompt, "selectChar");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }

            if (AsChar(export.Nc.Item.Ltr1Promt.SelectChar) == 'S')
            {
              if (!IsEmpty(import.Selected.Cdvalue))
              {
                export.Nc.Update.NcpNonCooperation.Letter1Code =
                  import.Selected.Cdvalue;
              }

              export.Nc.Update.Ltr1Promt.SelectChar = "";

              var field = GetField(export.Nc.Item.Ltr1Promt, "selectChar");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }

            if (AsChar(export.Nc.Item.Ltr2Prompt.SelectChar) == 'S')
            {
              if (!IsEmpty(import.Selected.Cdvalue))
              {
                export.Nc.Update.NcpNonCooperation.Letter2Code =
                  import.Selected.Cdvalue;
              }

              export.Nc.Update.Ltr2Prompt.SelectChar = "";

              var field = GetField(export.Nc.Item.Ltr2Prompt, "selectChar");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }

            if (AsChar(export.Nc.Item.Phone1Prompt.SelectChar) == 'S')
            {
              if (!IsEmpty(import.Selected.Cdvalue))
              {
                export.Nc.Update.NcpNonCooperation.Phone1Code =
                  import.Selected.Cdvalue;
              }

              export.Nc.Update.Phone1Prompt.SelectChar = "";

              var field = GetField(export.Nc.Item.Phone1Prompt, "selectChar");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }

            if (AsChar(export.Nc.Item.Phone2Prompt.SelectChar) == 'S')
            {
              if (!IsEmpty(import.Selected.Cdvalue))
              {
                export.Nc.Update.NcpNonCooperation.Phone2Code =
                  import.Selected.Cdvalue;
              }

              export.Nc.Update.Phone2Prompt.SelectChar = "";

              var field = GetField(export.Nc.Item.Phone2Prompt, "selectChar");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }

            if (Equal(export.Nc.Item.NcpNonCooperation.EffectiveDate,
              local.Max.Date))
            {
            }
            else
            {
              var field1 = GetField(export.Nc.Item.Ltr1Promt, "selectChar");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.Nc.Item.Ltr2Prompt, "selectChar");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Nc.Item.Phone1Prompt, "selectChar");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.Nc.Item.Phone2Prompt, "selectChar");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 =
                GetField(export.Nc.Item.NcpNonCooperation, "letter1Date");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 =
                GetField(export.Nc.Item.NcpNonCooperation, "letter1Code");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 =
                GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.Nc.Item.NcpNonCooperation, "letter2Code");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.Nc.Item.NcpNonCooperation, "phone1Code");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Nc.Item.NcpNonCooperation, "phone1Date");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 =
                GetField(export.Nc.Item.NcpNonCooperation, "phone2Code");

              field11.Color = "cyan";
              field11.Protected = true;

              var field12 =
                GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

              field12.Color = "cyan";
              field12.Protected = true;

              var field13 = GetField(export.Nc.Item.EffDate, "date");

              field13.Color = "cyan";
              field13.Protected = true;

              var field14 =
                GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

              field14.Color = "cyan";
              field14.Protected = true;

              var field15 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

              field15.Color = "cyan";
              field15.Protected = true;
            }

            if (Equal(export.Nc.Item.NcpNonCooperation.EndDate, local.Max.Date))
            {
            }
            else
            {
              var field1 = GetField(export.Nc.Item.EndDate, "date");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.Nc.Item.EffDate, "date");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Nc.Item.EndReasonPrompt, "selectChar");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.Nc.Item.Select, "selectChar");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 = GetField(export.Nc.Item.NcpNonCooperation, "note");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 =
                GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 =
                GetField(export.Nc.Item.NcpNonCooperation, "endStatusCode");

              field8.Color = "cyan";
              field8.Protected = true;
            }

            if (!Lt(local.Blank.Date,
              export.Nc.Item.NcpNonCooperation.Phone2Date))
            {
              var field1 = GetField(export.Nc.Item.EffDate, "date");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.Nc.Item.NcAp, "number");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }

          export.Nc.CheckIndex();
        }

        break;
      case "ADD":
        if (AsChar(export.SuccessfullyDisplayed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_ADD";

          break;
        }

        local.Row1AddFuncNc.Flag = "";

        // ---------------------------------------------------------
        // Added the IF statements in the
        // case of SPACES to set the error and
        // place the cursor.
        // --------------------------------------------------------------
        for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
          export.Nc.Index)
        {
          if (!export.Nc.CheckSize())
          {
            break;
          }

          if (AsChar(export.Nc.Item.Select.SelectChar) == 'S')
          {
            var field1 = GetField(export.Nc.Item.NcAp, "number");

            field1.Error = true;

            var field2 = GetField(export.Nc.Item.Select, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_ACTION";

            return;
          }
          else
          {
          }
        }

        export.Nc.CheckIndex();

        if (IsEmpty(export.NewSelect.SelectChar))
        {
          var field = GetField(export.NewSelect, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // Add ap non-cooperation record
        if (AsChar(export.NewSelect.SelectChar) == 'S')
        {
          if (IsEmpty(export.NewAp.Number))
          {
            ExitState = "CSE_PERSON_NO_REQUIRED";

            var field = GetField(export.NewAp, "number");

            field.Error = true;

            return;
          }
          else
          {
            UseCabZeroFillNumber1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (IsExitState("PERSON_NUMBER_NOT_NUMERIC"))
              {
              }
            }
          }

          if (Lt(local.Blank.Date, export.New1.Letter1Date))
          {
            if (Lt(local.Current.Date, export.New1.Letter1Date))
            {
              var field = GetField(export.New1, "letter1Date");

              field.Error = true;

              ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

              return;
            }
          }
          else
          {
            var field = GetField(export.New1, "letter1Date");

            field.Error = true;

            ExitState = "MUST_HAVE_1ST_LETTER_DATE";

            return;
          }

          if (Lt(local.Blank.Date, export.New1.Phone1Date))
          {
            if (!Lt(export.New1.Phone1Date, AddDays(export.New1.Letter1Date, 14)))
              
            {
              if (Lt(local.Current.Date, export.New1.Phone1Date))
              {
                var field = GetField(export.New1, "phone1Date");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                return;
              }

              // this is fine
            }
            else
            {
              var field = GetField(export.New1, "phone1Date");

              field.Error = true;

              ExitState = "PHONE_CALL_14_DAYS_AFTER_LTR";

              return;
            }
          }

          if (Lt(local.Blank.Date, export.New1.Letter2Date))
          {
            if (!Lt(export.New1.Letter2Date,
              AddDays(export.New1.Letter1Date, 14)))
            {
              if (Lt(local.Current.Date, export.New1.Letter2Date))
              {
                var field = GetField(export.New1, "letter2Date");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                return;
              }

              // this is fine
            }
            else
            {
              var field = GetField(export.New1, "phone1Date");

              field.Error = true;

              ExitState = "2ND_LTR_14_DAYS_AFTER_1ST_LTR";

              return;
            }
          }

          if (Lt(local.Blank.Date, export.New1.Phone2Date))
          {
            if (!Lt(local.Blank.Date, export.New1.Letter2Date))
            {
              var field1 = GetField(export.New1, "phone2Date");

              field1.Error = true;

              var field2 = GetField(export.New1, "letter2Date");

              field2.Error = true;

              ExitState = "MUST_HAVE_A_2ND_LETTER_DT_FIRST";

              return;
            }

            if (!Lt(export.New1.Phone2Date, AddDays(export.New1.Letter2Date, 14)))
              
            {
              if (Lt(local.Current.Date, export.New1.Phone2Date))
              {
                var field = GetField(export.New1, "phone2Date");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                return;
              }

              // this is fine
            }
            else
            {
              var field = GetField(export.New1, "phone2Date");

              field.Error = true;

              ExitState = "PHONE_CALL_14_DAYS_AFTER_2_LTR";

              return;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          // VALIDATE non coop CODES
          if (!IsEmpty(export.New1.Letter1Code))
          {
            if (!Lt(local.Blank.Date, export.New1.Letter1Date))
            {
              var field = GetField(export.New1, "letter1Date");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }

            local.Code.CodeName = "REASON FOR NCP NON-COOP LETTER";
            local.CodeValue.Cdvalue = export.New1.Letter1Code ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field = GetField(export.New1, "letter1Code");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              return;
            }
          }

          if (!IsEmpty(export.New1.Phone1Code))
          {
            if (!Lt(local.Blank.Date, export.New1.Phone1Date))
            {
              var field = GetField(export.New1, "phone1Date");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }

            local.Code.CodeName = "REASON FOR NCP NC PHONE CALL";
            local.CodeValue.Cdvalue = export.New1.Phone1Code ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field = GetField(export.New1, "phone1Code");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              return;
            }
          }

          if (!IsEmpty(export.New1.Letter2Code))
          {
            if (!Lt(local.Blank.Date, export.New1.Letter2Date))
            {
              var field = GetField(export.New1, "letter2Date");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }

            local.Code.CodeName = "REASON FOR NCP NON-COOP LETTER";
            local.CodeValue.Cdvalue = export.New1.Letter2Code ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field = GetField(export.New1, "letter2Code");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              return;
            }
          }

          if (!IsEmpty(export.New1.Phone2Code))
          {
            if (!Lt(local.Blank.Date, export.New1.Phone2Date))
            {
              var field = GetField(export.New1, "phone2Date");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

              return;
            }

            local.Code.CodeName = "REASON FOR NCP NC PHONE CALL";
            local.CodeValue.Cdvalue = export.New1.Phone2Code ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.Common.Flag) == 'N')
            {
              var field = GetField(export.New1, "phone2Code");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE";

              return;
            }
          }

          MoveNcpNonCooperation2(local.ClearNcpNonCooperation, local.TestNew);

          if (ReadNcpNonCooperation())
          {
            MoveNcpNonCooperation2(entities.NcpNonCooperation, local.TestNew);
          }

          if (Equal(local.TestNew.EndDate, local.Max.Date))
          {
            // this is a live record of some sort so we can not add one
            if (Equal(local.TestNew.EffectiveDate, local.Max.Date))
            {
              var field = GetField(export.NewAp, "number");

              field.Error = true;

              ExitState = "NON_COOP_ALREADY_STARTED";
            }
            else
            {
              var field = GetField(export.NewAp, "number");

              field.Error = true;

              ExitState = "ALREADY_HAVE_AN_ACTIVE_REC";
            }

            return;
          }

          UseSiAddNcpNonCoop();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.NewSelect.SelectChar = "";
            export.New1.Assign(local.ClearNcpNonCooperation);
            export.NewAp.Number = "";
            export.NewLetter1CdPrmt.SelectChar = "";
            export.NewLetter2CdPrmt.SelectChar = "";
            export.NewPersonPrompt.SelectChar = "";
            export.NewPhone1CdPrmt.SelectChar = "";
            export.NewPhone2CdPrmt.SelectChar = "";
          }
          else
          {
            UseEabRollbackCics();

            return;
          }
        }
        else
        {
          var field = GetField(export.NewSelect, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (AsChar(local.Row1AddFuncNc.Flag) == 'Y')
        {
          export.Nc.Index = 0;
          export.Nc.CheckSize();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenRedisplay.Flag = "A";
          global.Command = "DISPLAY";
        }

        break;
      case "DISPLAY":
        // Display statements are in a seperate case at the end of the prad.
        break;
      case "PREV":
        if (IsEmpty(export.NcMinus.OneChar))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.Nc1.PageNumber;

        export.Pagenum.Index = export.Nc1.PageNumber - 1;
        export.Pagenum.CheckSize();

        MoveNcpNonCooperation3(export.Pagenum.Item.Pagenum1, export.Last);
        export.Nc.Index = -1;
        export.Nc.Count = 0;
        export.HiddenGrpNc.Index = -1;
        export.HiddenGrpNc.Count = 0;
        export.NoItemsNc.Count = 0;
        export.CurrItmNoNc.Count = 0;

        foreach(var item in ReadNcpNonCooperationCsePerson1())
        {
          ++export.Nc.Index;
          export.Nc.CheckSize();

          MoveNcpNonCooperation1(entities.NcpNonCooperation,
            export.Nc.Update.NcpNonCooperation);
          export.Nc.Update.NcAp.Number = entities.ApCsePerson.Number;

          if (Equal(export.Nc.Item.NcpNonCooperation.EffectiveDate,
            local.Max.Date))
          {
            export.Nc.Update.EffDate.Date = local.Blank.Date;
          }
          else
          {
            export.Nc.Update.EffDate.Date =
              export.Nc.Item.NcpNonCooperation.EffectiveDate;

            var field1 = GetField(export.Nc.Item.Ltr1Promt, "selectChar");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.Ltr2Prompt, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Nc.Item.Phone1Prompt, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.Phone2Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter1Date");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter1Code");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter2Code");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone1Code");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone1Date");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone2Code");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 = GetField(export.Nc.Item.EffDate, "date");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field15.Color = "cyan";
            field15.Protected = true;
          }

          if (Equal(export.Nc.Item.NcpNonCooperation.EndDate, local.Max.Date))
          {
            export.Nc.Update.EndDate.Date = local.Blank.Date;
          }
          else
          {
            export.Nc.Update.EndDate.Date =
              export.Nc.Item.NcpNonCooperation.EndDate;

            var field1 = GetField(export.Nc.Item.EndDate, "date");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.EffDate, "date");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Nc.Item.EndReasonPrompt, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Nc.Item.Select, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.Nc.Item.NcpNonCooperation, "note");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Nc.Item.NcpNonCooperation, "endStatusCode");

            field8.Color = "cyan";
            field8.Protected = true;
          }

          if (!Lt(local.Blank.Date, export.Nc.Item.NcpNonCooperation.Phone2Date))
            
          {
            var field1 = GetField(export.Nc.Item.EffDate, "date");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.NcAp, "number");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          export.HiddenGrpNc.Index = export.Nc.Index;
          export.HiddenGrpNc.CheckSize();

          export.HiddenGrpNc.Update.Hidden.Assign(
            export.Nc.Item.NcpNonCooperation);
          export.HiddenGrpNc.Update.HiddenEffDate.Date =
            export.Nc.Item.EffDate.Date;
          export.HiddenGrpNc.Update.HiddenEndDate.Date =
            export.Nc.Item.EndDate.Date;
          export.HiddenGrpNc.Update.HiddenGrpNcAp.Number =
            export.Nc.Item.NcAp.Number;

          if (export.Nc.IsFull)
          {
            break;
          }
        }

        if (export.Nc1.PageNumber > 1)
        {
          export.NcMinus.OneChar = "-";
        }
        else
        {
          export.NcMinus.OneChar = "";
        }

        export.NcPlus.OneChar = "+";

        break;
      case "NEXT":
        if (IsEmpty(export.NcPlus.OneChar))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        ++export.Nc1.PageNumber;

        export.Pagenum.Index = export.Nc1.PageNumber - 1;
        export.Pagenum.CheckSize();

        MoveNcpNonCooperation3(export.Pagenum.Item.Pagenum1, export.Last);
        export.Nc.Index = -1;
        export.Nc.Count = 0;
        export.HiddenGrpNc.Index = -1;
        export.HiddenGrpNc.Count = 0;
        export.NoItemsNc.Count = 0;
        export.CurrItmNoNc.Count = 0;

        foreach(var item in ReadNcpNonCooperationCsePerson1())
        {
          ++export.Nc.Index;
          export.Nc.CheckSize();

          MoveNcpNonCooperation1(entities.NcpNonCooperation,
            export.Nc.Update.NcpNonCooperation);
          export.Nc.Update.NcAp.Number = entities.ApCsePerson.Number;

          if (Equal(export.Nc.Item.NcpNonCooperation.EffectiveDate,
            local.Max.Date))
          {
            export.Nc.Update.EffDate.Date = local.Blank.Date;
          }
          else
          {
            export.Nc.Update.EffDate.Date =
              export.Nc.Item.NcpNonCooperation.EffectiveDate;

            var field1 = GetField(export.Nc.Item.Ltr1Promt, "selectChar");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.Ltr2Prompt, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Nc.Item.Phone1Prompt, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.Phone2Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter1Date");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter1Code");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter2Code");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone1Code");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone1Date");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone2Code");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 = GetField(export.Nc.Item.EffDate, "date");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field15.Color = "cyan";
            field15.Protected = true;
          }

          if (Equal(export.Nc.Item.NcpNonCooperation.EndDate, local.Max.Date))
          {
            export.Nc.Update.EndDate.Date = local.Blank.Date;
          }
          else
          {
            export.Nc.Update.EndDate.Date =
              export.Nc.Item.NcpNonCooperation.EndDate;

            var field1 = GetField(export.Nc.Item.EndDate, "date");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.EffDate, "date");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Nc.Item.EndReasonPrompt, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Nc.Item.Select, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.Nc.Item.NcpNonCooperation, "note");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Nc.Item.NcpNonCooperation, "endStatusCode");

            field8.Color = "cyan";
            field8.Protected = true;
          }

          if (!Lt(local.Blank.Date, export.Nc.Item.NcpNonCooperation.Phone2Date))
            
          {
            var field1 = GetField(export.Nc.Item.EffDate, "date");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.NcAp, "number");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          export.HiddenGrpNc.Index = export.Nc.Index;
          export.HiddenGrpNc.CheckSize();

          export.HiddenGrpNc.Update.Hidden.Assign(
            export.Nc.Item.NcpNonCooperation);
          export.HiddenGrpNc.Update.HiddenEffDate.Date =
            export.Nc.Item.EffDate.Date;
          export.HiddenGrpNc.Update.HiddenEndDate.Date =
            export.Nc.Item.EndDate.Date;
          export.HiddenGrpNc.Update.HiddenGrpNcAp.Number =
            export.Nc.Item.NcAp.Number;

          if (export.Nc.IsFull)
          {
            break;
          }
        }

        if (export.Nc.IsFull)
        {
          export.NcPlus.OneChar = "+";

          ++export.Pagenum.Index;
          export.Pagenum.CheckSize();

          export.Nc.Index = Export.NcGroup.Capacity - 2;
          export.Nc.CheckSize();

          export.Pagenum.Update.Pagenum1.EffectiveDate =
            export.Nc.Item.NcpNonCooperation.EffectiveDate;
          export.Pagenum.Update.Pagenum1.LastUpdatedTimestamp =
            export.Nc.Item.NcpNonCooperation.LastUpdatedTimestamp;
        }
        else
        {
          export.NcPlus.OneChar = "";
        }

        export.NcMinus.OneChar = "-";

        break;
      case "UPDATE":
        if (AsChar(export.SuccessfullyDisplayed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          break;
        }

        local.DataChanged.Flag = "N";
        local.NoRecordSelected.Flag = "N";
        local.SuccessfullyUpdated.Flag = "N";
        local.UpdatedRecord.CreatedTimestamp =
          local.ClearNcpNonCooperation.CreatedTimestamp;

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        export.Nc.Index = 0;
        export.Nc.CheckSize();

        local.GrpNcCnt.Count = 0;
        export.Nc.Index = 0;

        for(var limit = export.Nc.Count; export.Nc.Index < limit; ++
          export.Nc.Index)
        {
          if (!export.Nc.CheckSize())
          {
            break;
          }

          export.HiddenGrpNc.Index = export.Nc.Index;
          export.HiddenGrpNc.CheckSize();

          if (AsChar(export.Nc.Item.Select.SelectChar) == 'S')
          {
            if (!IsEmpty(export.HiddenGrpNc.Item.Hidden.EndStatusCode))
            {
              var field = GetField(export.Nc.Item.Select, "selectChar");

              field.Error = true;

              ExitState = "CAN_NOT_UPDATE_A_HISTORY_REC";

              return;
            }

            if (!Equal(export.Nc.Item.NcpNonCooperation.Letter1Date,
              export.HiddenGrpNc.Item.Hidden.Letter1Date) && !
              Lt(local.Blank.Date, export.Nc.Item.EffDate.Date))
            {
              if (Lt(local.Current.Date,
                export.Nc.Item.NcpNonCooperation.Letter1Date))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "letter1Date");

                field.Error = true;

                ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                return;
              }

              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.Letter1Date))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "letter1Date");

                field.Error = true;

                ExitState = "MUST_HAVE_1ST_LETTER_DATE";

                return;
              }

              if (Lt(local.Blank.Date,
                export.HiddenGrpNc.Item.Hidden.Letter1Date))
              {
                // ok the letter 1 date has been changed and there was no 
                // effective date, that means
                // this record is being started over, therefore clear everything
                // outthat was already
                // there but if new thinds were added then let them also be 
                // added
                if (Lt(local.Blank.Date,
                  export.HiddenGrpNc.Item.Hidden.Phone1Date))
                {
                  export.Nc.Update.NcpNonCooperation.Phone1Date =
                    local.Blank.Date;
                  export.Nc.Update.NcpNonCooperation.Phone1Code = "";
                }

                if (Lt(local.Blank.Date,
                  export.HiddenGrpNc.Item.Hidden.Letter2Date))
                {
                  export.Nc.Update.NcpNonCooperation.Letter2Date =
                    local.Blank.Date;
                  export.Nc.Update.NcpNonCooperation.Letter2Code = "";
                }

                if (Lt(local.Blank.Date,
                  export.HiddenGrpNc.Item.Hidden.Phone2Date))
                {
                  export.Nc.Update.NcpNonCooperation.Phone2Date =
                    local.Blank.Date;
                  export.Nc.Update.NcpNonCooperation.Phone2Code = "";
                }
              }

              ++local.GrpNcCnt.Count;
            }
            else
            {
              if (Lt(local.Blank.Date, export.Nc.Item.EffDate.Date) && !
                Equal(export.Nc.Item.NcpNonCooperation.Letter1Date,
                export.HiddenGrpNc.Item.Hidden.Letter1Date))
              {
                var field = GetField(export.New1, "letter1Date");

                field.Error = true;

                ExitState = "CANT_CHANGE_LETTER_1_DATE";

                return;
              }

              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.Letter1Date))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "letter1Date");

                field.Error = true;

                ExitState = "MUST_HAVE_1ST_LETTER_DATE";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            if (Lt(local.Blank.Date, export.Nc.Item.NcpNonCooperation.Phone1Date)
              && !
              Equal(export.Nc.Item.NcpNonCooperation.Phone1Date,
              export.HiddenGrpNc.Item.Hidden.Phone1Date))
            {
              if (!Lt(export.Nc.Item.NcpNonCooperation.Phone1Date,
                AddDays(export.Nc.Item.NcpNonCooperation.Letter1Date, 14)))
              {
                if (Lt(local.Current.Date,
                  export.Nc.Item.NcpNonCooperation.Phone1Date))
                {
                  var field =
                    GetField(export.Nc.Item.NcpNonCooperation, "phone1Date");

                  field.Error = true;

                  ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                  return;
                }

                // this is fine
              }
              else
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "phone1Date");

                field.Error = true;

                ExitState = "PHONE_CALL_14_DAYS_AFTER_LTR";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            if (Lt(local.Blank.Date,
              export.Nc.Item.NcpNonCooperation.Letter2Date) && !
              Equal(export.Nc.Item.NcpNonCooperation.Letter2Date,
              export.HiddenGrpNc.Item.Hidden.Letter2Date))
            {
              if (!Lt(export.Nc.Item.NcpNonCooperation.Letter2Date,
                AddDays(export.Nc.Item.NcpNonCooperation.Letter1Date, 14)))
              {
                if (Lt(local.Current.Date,
                  export.Nc.Item.NcpNonCooperation.Letter2Date))
                {
                  var field =
                    GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

                  field.Error = true;

                  ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                  return;
                }

                // this is fine
              }
              else
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

                field.Error = true;

                ExitState = "2ND_LTR_14_DAYS_AFTER_1ST_LTR";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            if (Lt(local.Blank.Date, export.Nc.Item.NcpNonCooperation.Phone2Date)
              && !
              Equal(export.Nc.Item.NcpNonCooperation.Phone2Date,
              export.HiddenGrpNc.Item.Hidden.Phone2Date))
            {
              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.Letter2Date))
              {
                var field1 =
                  GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

                field1.Error = true;

                var field2 =
                  GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

                field2.Error = true;

                ExitState = "MUST_HAVE_A_2ND_LETTER_DT_FIRST";

                return;
              }

              if (!Lt(export.Nc.Item.NcpNonCooperation.Phone2Date,
                AddDays(export.Nc.Item.NcpNonCooperation.Letter2Date, 14)))
              {
                if (Lt(local.Current.Date,
                  export.Nc.Item.NcpNonCooperation.Phone2Date))
                {
                  var field =
                    GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

                  field.Error = true;

                  ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

                  return;
                }

                // this is fine
              }
              else
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

                field.Error = true;

                ExitState = "PHONE_CALL_14_DAYS_AFTER_2_LTR";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            if (!IsEmpty(export.Nc.Item.NcpNonCooperation.Letter1Code) && !
              Equal(export.Nc.Item.NcpNonCooperation.Letter1Code,
              export.HiddenGrpNc.Item.Hidden.Letter1Code))
            {
              local.Code.CodeName = "REASON FOR NCP NON-COOP LETTER";
              local.CodeValue.Cdvalue =
                export.Nc.Item.NcpNonCooperation.Letter1Code ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.Common.Flag) == 'N')
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "letter1Code");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            if (!IsEmpty(export.Nc.Item.NcpNonCooperation.Phone1Code) && !
              Equal(export.Nc.Item.NcpNonCooperation.Phone1Code,
              export.HiddenGrpNc.Item.Hidden.Phone1Code))
            {
              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.Phone1Date))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "phone1Date");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              local.Code.CodeName = "REASON FOR NCP NC PHONE CALL";
              local.CodeValue.Cdvalue =
                export.Nc.Item.NcpNonCooperation.Phone1Code ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.Common.Flag) == 'N')
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "phone1Code");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            if (!IsEmpty(export.Nc.Item.NcpNonCooperation.Letter2Code) && !
              Equal(export.Nc.Item.NcpNonCooperation.Letter2Code,
              export.HiddenGrpNc.Item.Hidden.Letter2Code))
            {
              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.Letter2Date))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              local.Code.CodeName = "REASON FOR NCP NON-COOP LETTER";
              local.CodeValue.Cdvalue =
                export.Nc.Item.NcpNonCooperation.Letter2Code ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.Common.Flag) == 'N')
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "letter2Code");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            if (!IsEmpty(export.Nc.Item.NcpNonCooperation.Phone2Code) && !
              Equal(export.Nc.Item.NcpNonCooperation.Phone2Code,
              export.HiddenGrpNc.Item.Hidden.Phone2Code))
            {
              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.Phone2Date))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              local.Code.CodeName = "REASON FOR NCP NC PHONE CALL";
              local.CodeValue.Cdvalue =
                export.Nc.Item.NcpNonCooperation.Phone2Code ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.Common.Flag) == 'N')
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "phone2Code");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            if (Lt(local.Blank.Date, export.Nc.Item.EffDate.Date) && !
              Equal(export.Nc.Item.EffDate.Date,
              export.HiddenGrpNc.Item.Hidden.EffectiveDate))
            {
              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.Phone1Date))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "phone1Date");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.Letter2Date))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.Phone2Date))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              if (Lt(export.Nc.Item.EffDate.Date,
                export.Nc.Item.NcpNonCooperation.Phone2Date))
              {
                var field = GetField(export.Nc.Item.EffDate, "date");

                field.Error = true;

                ExitState = "EFFECTIVE_DATE_BEFORE_PHONE_2_DT";

                return;
              }

              if (IsEmpty(export.Nc.Item.NcpNonCooperation.ReasonCode))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              export.Nc.Update.NcpNonCooperation.EffectiveDate =
                export.Nc.Item.EffDate.Date;
              ++local.GrpNcCnt.Count;
            }

            if (Lt(local.Blank.Date, export.Nc.Item.EndDate.Date) && !
              Equal(export.Nc.Item.EndDate.Date,
              export.HiddenGrpNc.Item.Hidden.EffectiveDate))
            {
              if (Lt(export.Nc.Item.EndDate.Date, export.Nc.Item.EffDate.Date))
              {
                var field = GetField(export.Nc.Item.EndDate, "date");

                field.Error = true;

                ExitState = "ACO_NE0000_END_LESS_THAN_EFF";

                return;
              }

              if (IsEmpty(export.Nc.Item.NcpNonCooperation.EndStatusCode))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "endStatusCode");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              export.Nc.Update.NcpNonCooperation.EndDate =
                export.Nc.Item.EndDate.Date;
              export.Nc.Update.NcpNonCooperation.EffectiveDate =
                export.Nc.Item.EndDate.Date;
              ++local.GrpNcCnt.Count;
            }

            // VALIDATE non coop CODES
            if (!IsEmpty(export.Nc.Item.NcpNonCooperation.ReasonCode) && !
              Equal(export.Nc.Item.NcpNonCooperation.ReasonCode,
              export.HiddenGrpNc.Item.Hidden.ReasonCode))
            {
              if (!Lt(local.Blank.Date,
                export.Nc.Item.NcpNonCooperation.EffectiveDate))
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "effectiveDate");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              local.Code.CodeName = "NCP NON-COOP EFF DATE REASON";
              local.CodeValue.Cdvalue =
                export.Nc.Item.NcpNonCooperation.ReasonCode ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.Common.Flag) == 'N')
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            if (!IsEmpty(export.Nc.Item.NcpNonCooperation.EndStatusCode) && !
              Equal(export.Nc.Item.NcpNonCooperation.EndStatusCode,
              export.HiddenGrpNc.Item.Hidden.EndStatusCode))
            {
              if (!Lt(local.Blank.Date, export.Nc.Item.NcpNonCooperation.EndDate))
                
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "endDate");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              local.Code.CodeName = "NCP NON-COOP END DATE REASON";
              local.CodeValue.Cdvalue =
                export.Nc.Item.NcpNonCooperation.EndStatusCode ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.Common.Flag) == 'N')
              {
                var field =
                  GetField(export.Nc.Item.NcpNonCooperation, "endStatusCode");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                return;
              }

              ++local.GrpNcCnt.Count;
            }

            UseSiUpdateNcpNonCoop();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field = GetField(export.Nc.Item.Select, "selectChar");

              field.Error = true;

              UseEabRollbackCics();

              return;
            }

            local.NoRecordSelected.Flag = "Y";
            export.Nc.Update.Select.SelectChar = "*";
            local.UpdatedRecord.CreatedTimestamp =
              export.Nc.Item.NcpNonCooperation.CreatedTimestamp;
          }
          else
          {
          }
        }

        export.Nc.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(local.NoRecordSelected.Flag) == 'N')
        {
          ExitState = "ACO_NE0000_NO_SELECTION_UPDATE";

          return;
        }

        if (AsChar(local.DataChanged.Flag) == 'N' && local.GrpGcCnt.Count <= 0
          && local.GrpNcCnt.Count <= 0)
        {
          ExitState = "ACO_NI0000_DATA_WAS_NOT_CHANGED";

          break;
        }

        local.SuccessfullyUpdated.Flag = "Y";
        global.Command = "DISPLAY";

        break;
      case "LIST":
        switch(AsChar(export.ApPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field = GetField(export.ApPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(export.NewPersonPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            break;
          default:
            var field = GetField(export.NewPersonPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(export.NewLetter1CdPrmt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.Prompt.CodeName = "REASON FOR NCP NON-COOP LETTER";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.NewLetter1CdPrmt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(export.NewLetter2CdPrmt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.Prompt.CodeName = "REASON FOR NCP NON-COOP LETTER";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.NewLetter2CdPrmt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(export.NewPhone1CdPrmt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.Prompt.CodeName = "REASON FOR NCP NC PHONE CALL";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.NewPhone1CdPrmt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            ++local.Invalid.Count;

            break;
        }

        switch(AsChar(export.NewPhone2CdPrmt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Invalid.Count;
            export.Prompt.CodeName = "REASON FOR NCP NC PHONE CALL";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            var field = GetField(export.NewPhone2CdPrmt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            ++local.Invalid.Count;

            break;
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            switch(AsChar(export.ApPrompt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.ApPrompt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            switch(AsChar(export.NewPersonPrompt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewPersonPrompt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            switch(AsChar(export.NewLetter1CdPrmt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewLetter1CdPrmt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            switch(AsChar(export.NewLetter2CdPrmt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewLetter2CdPrmt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            switch(AsChar(export.NewPhone1CdPrmt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewPhone1CdPrmt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            switch(AsChar(export.NewPhone2CdPrmt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewPhone2CdPrmt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            return;
        }

        // Non-Coop Prompt
        if (!export.Nc.IsEmpty)
        {
          for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
            export.Nc.Index)
          {
            if (!export.Nc.CheckSize())
            {
              break;
            }

            switch(AsChar(export.Nc.Item.NcRsnPrompt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ++local.Invalid.Count;
                export.Prompt.CodeName = "NCP NON-COOP EFF DATE REASON";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                break;
              default:
                var field = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                ++local.Invalid.Count;

                break;
            }

            switch(AsChar(export.Nc.Item.EndReasonPrompt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ++local.Invalid.Count;
                export.Prompt.CodeName = "NCP NON-COOP END DATE REASON";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                break;
              default:
                var field =
                  GetField(export.Nc.Item.EndReasonPrompt, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                ++local.Invalid.Count;

                break;
            }

            switch(AsChar(export.Nc.Item.Ltr1Promt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ++local.Invalid.Count;
                export.Prompt.CodeName = "REASON FOR NCP NON-COOP LETTER";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                break;
              default:
                var field = GetField(export.Nc.Item.Ltr1Promt, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                ++local.Invalid.Count;

                break;
            }

            switch(AsChar(export.Nc.Item.Ltr2Prompt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ++local.Invalid.Count;
                export.Prompt.CodeName = "REASON FOR NCP NON-COOP LETTER";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                break;
              default:
                var field = GetField(export.Nc.Item.Ltr2Prompt, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                ++local.Invalid.Count;

                break;
            }

            switch(AsChar(export.Nc.Item.Phone1Prompt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ++local.Invalid.Count;
                export.Prompt.CodeName = "REASON FOR NCP NC PHONE CALL";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                break;
              default:
                var field = GetField(export.Nc.Item.Phone1Prompt, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                ++local.Invalid.Count;

                break;
            }

            switch(AsChar(export.Nc.Item.Phone2Prompt.SelectChar))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ++local.Invalid.Count;
                export.Prompt.CodeName = "REASON FOR NCP NC PHONE CALL";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                break;
              default:
                var field = GetField(export.Nc.Item.Phone2Prompt, "selectChar");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
                ++local.Invalid.Count;

                break;
            }
          }

          export.Nc.CheckIndex();
        }

        switch(local.Invalid.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            if (AsChar(export.ApPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.ApPrompt, "selectChar");

              field.Error = true;
            }

            switch(AsChar(export.NewPersonPrompt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewPersonPrompt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            switch(AsChar(export.NewLetter1CdPrmt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewLetter1CdPrmt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            switch(AsChar(export.NewLetter2CdPrmt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewLetter2CdPrmt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            switch(AsChar(export.NewPhone1CdPrmt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewPhone1CdPrmt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            switch(AsChar(export.NewPhone2CdPrmt.SelectChar))
            {
              case ' ':
                break;
              case 'S':
                var field = GetField(export.NewPhone2CdPrmt, "selectChar");

                field.Error = true;

                break;
              default:
                break;
            }

            if (!export.Nc.IsEmpty)
            {
              for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
                export.Nc.Index)
              {
                if (!export.Nc.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Nc.Item.NcRsnPrompt.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

                  field.Error = true;
                }

                if (AsChar(export.Nc.Item.EndReasonPrompt.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Nc.Item.EndReasonPrompt, "selectChar");

                  field.Error = true;
                }

                if (AsChar(export.Nc.Item.Ltr1Promt.SelectChar) == 'S')
                {
                  var field = GetField(export.Nc.Item.Ltr1Promt, "selectChar");

                  field.Error = true;
                }

                if (AsChar(export.Nc.Item.Ltr2Prompt.SelectChar) == 'S')
                {
                  var field = GetField(export.Nc.Item.Ltr2Prompt, "selectChar");

                  field.Error = true;
                }

                if (AsChar(export.Nc.Item.Phone1Prompt.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Nc.Item.Phone1Prompt, "selectChar");

                  field.Error = true;
                }

                if (AsChar(export.Nc.Item.Phone2Prompt.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Nc.Item.Phone2Prompt, "selectChar");

                  field.Error = true;
                }
              }

              export.Nc.CheckIndex();
            }

            break;
        }

        break;
      case "HELP":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
            ;
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "PEPR":
        ExitState = "ECO_LNK_TO_PEPR";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
          export.Nc.Index)
        {
          if (!export.Nc.CheckSize())
          {
            break;
          }

          if (Equal(export.Nc.Item.NcpNonCooperation.EffectiveDate,
            local.Max.Date))
          {
            export.Nc.Update.EffDate.Date = local.Blank.Date;
          }
          else
          {
            export.Nc.Update.EffDate.Date =
              export.Nc.Item.NcpNonCooperation.EffectiveDate;

            var field1 = GetField(export.Nc.Item.Ltr1Promt, "selectChar");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.Ltr2Prompt, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Nc.Item.Phone1Prompt, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.Phone2Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter1Date");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter1Code");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter2Code");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone1Code");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone1Date");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone2Code");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 = GetField(export.Nc.Item.EffDate, "date");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field15.Color = "cyan";
            field15.Protected = true;
          }

          if (Equal(export.Nc.Item.NcpNonCooperation.EndDate, local.Max.Date))
          {
            export.Nc.Update.EndDate.Date = local.Blank.Date;
          }
          else
          {
            export.Nc.Update.EndDate.Date =
              export.Nc.Item.NcpNonCooperation.EndDate;

            var field1 = GetField(export.Nc.Item.EndDate, "date");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.EffDate, "date");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Nc.Item.EndReasonPrompt, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Nc.Item.Select, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.Nc.Item.NcpNonCooperation, "note");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Nc.Item.NcpNonCooperation, "endStatusCode");

            field8.Color = "cyan";
            field8.Protected = true;
          }

          if (!Lt(local.Blank.Date, export.Nc.Item.NcpNonCooperation.Phone2Date))
            
          {
            var field1 = GetField(export.Nc.Item.EffDate, "date");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.NcAp, "number");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }

        export.Nc.CheckIndex();

        break;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      export.New1.Assign(local.ClearNcpNonCooperation);
      export.NewPersonPrompt.SelectChar = "";
      export.NewLetter2CdPrmt.SelectChar = "";
      export.NewPhone1CdPrmt.SelectChar = "";
      export.NewPhone2CdPrmt.SelectChar = "";
      export.NewLetter1CdPrmt.SelectChar = "";
      export.NewLetter1CdPrmt.SelectChar = "";
      export.NewAp.Number = "";
      export.NewSelect.SelectChar = "";

      // ------------------------------------------------------------
      // Clear export group views
      // ------------------------------------------------------------
      for(export.Nc.Index = 0; export.Nc.Index < Export.NcGroup.Capacity; ++
        export.Nc.Index)
      {
        if (!export.Nc.CheckSize())
        {
          break;
        }

        export.Nc.Update.NcpNonCooperation.Assign(local.ClearNcpNonCooperation);
        export.Nc.Update.EffDate.Date = local.Blank.Date;
        export.Nc.Update.NcAp.Number = "";
        export.Nc.Update.Select.SelectChar = "";
        export.Nc.Update.NcRsnPrompt.SelectChar = "";
        export.Nc.Update.Ltr1Promt.SelectChar = "";
        export.Nc.Update.Ltr2Prompt.SelectChar = "";
        export.Nc.Update.Phone1Prompt.SelectChar = "";
        export.Nc.Update.Phone2Prompt.SelectChar = "";
        export.Nc.Update.EndReasonPrompt.SelectChar = "";
      }

      export.Nc.CheckIndex();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.SuccessfullyDisplayed.Flag = "Y";

        if (!IsEmpty(export.Next.Number))
        {
          UseCabZeroFillNumber2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (IsExitState("CASE_NUMBER_NOT_NUMERIC"))
            {
              var field = GetField(export.Next, "number");

              field.Error = true;

              export.SuccessfullyDisplayed.Flag = "N";

              return;
            }
          }
        }
        else
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "CASE_NUMBER_REQUIRED";
          export.SuccessfullyDisplayed.Flag = "N";

          goto Test3;
        }

        if (IsEmpty(export.Case1.Number))
        {
          export.Case1.Number = export.Next.Number;
        }

        if (!Equal(export.Next.Number, export.Case1.Number))
        {
          // Check AP number and Person number to determine if they are
          // valid and associated to the new case number.
          UseSiCheckCaseToApAndChild();
          export.Case1.Number = export.Next.Number;
        }

        export.NcPlus.OneChar = "";
        export.NcMinus.OneChar = "";

        // ---------------------------------------------
        // Call the action block that reads
        // the data required for this screen.
        // --------------------------------------------
        UseSiReadCaseHeaderInformation();

        if (!IsEmpty(local.AbendData.Type1))
        {
          // If EXIT STATE is OK for Abend of Natural then changed exit state to
          // error.
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_ADABAS_UNAVAILABLE";
          }

          export.SuccessfullyDisplayed.Flag = "N";

          goto Test3;
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y')
        {
          // ------------------------------------------
          // Code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
          // -----------------------------------------------
          export.HiddenNextTranInfo.MiscText1 = "SELECTION NEEDED";
          export.ApPrompt.SelectChar = "S";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          goto Test3;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("NO_APS_ON_A_CASE"))
          {
            local.NoAps.Flag = "Y";
            ExitState = "ACO_NN0000_ALL_OK";

            goto Test2;
          }

          goto Test3;
        }

Test2:

        UseSiReadOfficeOspHeader();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Prev.Number = export.Case1.Number;
        }
        else
        {
          if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF") || IsExitState
            ("CASE_NF"))
          {
            var field = GetField(export.Next, "number");

            field.Error = true;
          }
          else if (IsExitState("CASE_ROLE_AR_NF"))
          {
            var field = GetField(export.Ar, "number");

            field.Error = true;
          }

          export.SuccessfullyDisplayed.Flag = "N";

          goto Test3;
        }

        export.CurrItmNoNc.Count = 0;
        export.Nc.Index = -1;
        export.Nc.Count = 0;
        export.HiddenGrpNc.Index = -1;
        export.HiddenGrpNc.Count = 0;
        export.NoItemsNc.Count = 0;
        export.CurrItmNoNc.Count = 0;

        foreach(var item in ReadNcpNonCooperationCsePerson2())
        {
          ++export.Nc.Index;
          export.Nc.CheckSize();

          MoveNcpNonCooperation1(entities.NcpNonCooperation,
            export.Nc.Update.NcpNonCooperation);
          export.Nc.Update.NcAp.Number = entities.ApCsePerson.Number;

          if (Equal(export.Nc.Item.NcpNonCooperation.EffectiveDate,
            local.Max.Date))
          {
            export.Nc.Update.EffDate.Date = local.Blank.Date;
          }
          else
          {
            export.Nc.Update.EffDate.Date =
              export.Nc.Item.NcpNonCooperation.EffectiveDate;

            var field1 = GetField(export.Nc.Item.Ltr1Promt, "selectChar");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.Ltr2Prompt, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Nc.Item.Phone1Prompt, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.Phone2Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter1Date");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter1Code");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter2Date");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Nc.Item.NcpNonCooperation, "letter2Code");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone1Code");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone1Date");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone2Code");

            field11.Color = "cyan";
            field11.Protected = true;

            var field12 =
              GetField(export.Nc.Item.NcpNonCooperation, "phone2Date");

            field12.Color = "cyan";
            field12.Protected = true;

            var field13 = GetField(export.Nc.Item.EffDate, "date");

            field13.Color = "cyan";
            field13.Protected = true;

            var field14 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field14.Color = "cyan";
            field14.Protected = true;

            var field15 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field15.Color = "cyan";
            field15.Protected = true;
          }

          if (Equal(export.Nc.Item.NcpNonCooperation.EndDate, local.Max.Date))
          {
            export.Nc.Update.EndDate.Date = local.Blank.Date;
          }
          else
          {
            export.Nc.Update.EndDate.Date =
              export.Nc.Item.NcpNonCooperation.EndDate;

            var field1 = GetField(export.Nc.Item.EndDate, "date");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.EffDate, "date");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Nc.Item.EndReasonPrompt, "selectChar");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Nc.Item.Select, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.Nc.Item.NcpNonCooperation, "note");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 =
              GetField(export.Nc.Item.NcpNonCooperation, "endStatusCode");

            field8.Color = "cyan";
            field8.Protected = true;
          }

          if (!Lt(local.Blank.Date, export.Nc.Item.NcpNonCooperation.Phone2Date))
            
          {
            var field1 = GetField(export.Nc.Item.EffDate, "date");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Nc.Item.NcAp, "number");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Nc.Item.NcpNonCooperation, "reasonCode");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Nc.Item.NcRsnPrompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          export.HiddenGrpNc.Index = export.Nc.Index;
          export.HiddenGrpNc.CheckSize();

          export.HiddenGrpNc.Update.Hidden.Assign(
            export.Nc.Item.NcpNonCooperation);
          export.HiddenGrpNc.Update.HiddenEffDate.Date =
            export.Nc.Item.EffDate.Date;
          export.HiddenGrpNc.Update.HiddenEndDate.Date =
            export.Nc.Item.EndDate.Date;
          export.HiddenGrpNc.Update.HiddenGrpNcAp.Number =
            export.Nc.Item.NcAp.Number;

          if (export.Nc.IsFull)
          {
            break;
          }
        }

        if (export.Nc.IsFull)
        {
          export.NcPlus.OneChar = "+";

          export.Pagenum.Index = 0;
          export.Pagenum.CheckSize();

          export.Nc.Index = 0;
          export.Nc.CheckSize();

          export.Pagenum.Update.Pagenum1.EffectiveDate =
            export.Nc.Item.NcpNonCooperation.EffectiveDate;
          export.Pagenum.Update.Pagenum1.LastUpdatedTimestamp =
            Now().AddDays(1);

          ++export.Pagenum.Index;
          export.Pagenum.CheckSize();

          export.Nc.Index = Export.NcGroup.Capacity - 2;
          export.Nc.CheckSize();

          export.Pagenum.Update.Pagenum1.LastUpdatedTimestamp =
            export.Nc.Item.NcpNonCooperation.LastUpdatedTimestamp;
          export.Pagenum.Update.Pagenum1.EffectiveDate =
            export.Nc.Item.NcpNonCooperation.EffectiveDate;
        }
        else
        {
          export.NcPlus.OneChar = "";
        }

        export.Nc1.PageNumber = 1;
        export.NcMinus.OneChar = "";

        if (local.Count.AverageReal > export.MaxPagesNc.Count)
        {
          ++export.MaxPagesNc.Count;
        }
      }

Test3:

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Prev.Number = export.Case1.Number;
        export.HiddenPrev.Number = export.Ap.Number;

        if (AsChar(export.HiddenRedisplay.Flag) == 'A')
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        export.HiddenRedisplay.Flag = "";

        var field = GetField(export.Next, "number");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(local.SuccessfullyUpdated.Flag) == 'Y')
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
          export.Nc.Index)
        {
          if (!export.Nc.CheckSize())
          {
            break;
          }

          if (Equal(export.Nc.Item.NcpNonCooperation.CreatedTimestamp,
            local.UpdatedRecord.CreatedTimestamp))
          {
            export.Nc.Update.Select.SelectChar = "*";

            return;
          }
        }

        export.Nc.CheckIndex();
      }
      else
      {
        for(export.Nc.Index = 0; export.Nc.Index < export.Nc.Count; ++
          export.Nc.Index)
        {
          if (!export.Nc.CheckSize())
          {
            break;
          }

          if (Equal(export.Nc.Item.NcpNonCooperation.CreatedTimestamp,
            local.UpdatedRecord.CreatedTimestamp))
          {
            export.Nc.Update.Select.SelectChar = "";

            return;
          }
        }

        export.Nc.CheckIndex();
      }
    }
    else
    {
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveNcpNonCooperation1(NcpNonCooperation source,
    NcpNonCooperation target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.ReasonCode = source.ReasonCode;
    target.Letter1Date = source.Letter1Date;
    target.Letter1Code = source.Letter1Code;
    target.Letter2Date = source.Letter2Date;
    target.Letter2Code = source.Letter2Code;
    target.Phone1Date = source.Phone1Date;
    target.Phone1Code = source.Phone1Code;
    target.Phone2Date = source.Phone2Date;
    target.Phone2Code = source.Phone2Code;
    target.EndDate = source.EndDate;
    target.EndStatusCode = source.EndStatusCode;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.Note = source.Note;
  }

  private static void MoveNcpNonCooperation2(NcpNonCooperation source,
    NcpNonCooperation target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.ReasonCode = source.ReasonCode;
    target.Letter1Date = source.Letter1Date;
    target.Letter2Date = source.Letter2Date;
    target.Phone1Date = source.Phone1Date;
    target.Phone2Date = source.Phone2Date;
    target.EndDate = source.EndDate;
    target.EndStatusCode = source.EndStatusCode;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveNcpNonCooperation3(NcpNonCooperation source,
    NcpNonCooperation target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LastTran = source.LastTran;
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
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number = export.NewAp.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.NewAp.Number = useImport.CsePersonsWorkSet.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabReadInfrastructure()
  {
    var useImport = new OeCabReadInfrastructure.Import();
    var useExport = new OeCabReadInfrastructure.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.LastTran.SystemGeneratedIdentifier;

    Call(OeCabReadInfrastructure.Execute, useImport, useExport);

    local.LastTran.Assign(useExport.Infrastructure);
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
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiAddNcpNonCoop()
  {
    var useImport = new SiAddNcpNonCoop.Import();
    var useExport = new SiAddNcpNonCoop.Export();

    useImport.NcpNonCooperation.Assign(export.New1);
    useImport.Ap.Number = export.NewAp.Number;
    useImport.Case1.Number = import.Case1.Number;

    Call(SiAddNcpNonCoop.Execute, useImport, useExport);
  }

  private void UseSiCheckCaseToApAndChild()
  {
    var useImport = new SiCheckCaseToApAndChild.Import();
    var useExport = new SiCheckCaseToApAndChild.Export();

    useImport.Ap.Number = export.Ap.Number;
    useImport.HiddenAp.Number = export.HiddenPrev.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(SiCheckCaseToApAndChild.Execute, useImport, useExport);

    export.Ap.Number = useExport.Ap.Number;
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Ap.Number = export.Ap.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    local.AbendData.Assign(useExport.AbendData);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    export.Ap.Assign(useExport.Ap);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
  }

  private void UseSiUpdateNcpNonCoop()
  {
    var useImport = new SiUpdateNcpNonCoop.Import();
    var useExport = new SiUpdateNcpNonCoop.Export();

    useImport.NcpNonCooperation.Assign(export.Nc.Item.NcpNonCooperation);
    useImport.Ap.Number = export.Nc.Item.NcAp.Number;
    useImport.Case1.Number = import.Case1.Number;

    Call(SiUpdateNcpNonCoop.Execute, useImport, useExport);
  }

  private bool ReadNcpNonCooperation()
  {
    entities.NcpNonCooperation.Populated = false;

    return Read("ReadNcpNonCooperation",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.NewAp.Number);
      },
      (db, reader) =>
      {
        entities.NcpNonCooperation.EffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.NcpNonCooperation.ReasonCode = db.GetNullableString(reader, 1);
        entities.NcpNonCooperation.Letter1Date = db.GetNullableDate(reader, 2);
        entities.NcpNonCooperation.Letter1Code =
          db.GetNullableString(reader, 3);
        entities.NcpNonCooperation.Letter2Date = db.GetNullableDate(reader, 4);
        entities.NcpNonCooperation.Letter2Code =
          db.GetNullableString(reader, 5);
        entities.NcpNonCooperation.Phone1Date = db.GetNullableDate(reader, 6);
        entities.NcpNonCooperation.Phone1Code = db.GetNullableString(reader, 7);
        entities.NcpNonCooperation.Phone2Date = db.GetNullableDate(reader, 8);
        entities.NcpNonCooperation.Phone2Code = db.GetNullableString(reader, 9);
        entities.NcpNonCooperation.EndDate = db.GetNullableDate(reader, 10);
        entities.NcpNonCooperation.EndStatusCode =
          db.GetNullableString(reader, 11);
        entities.NcpNonCooperation.CreatedBy = db.GetString(reader, 12);
        entities.NcpNonCooperation.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.NcpNonCooperation.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.NcpNonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.NcpNonCooperation.CasNumber = db.GetString(reader, 16);
        entities.NcpNonCooperation.CspNumber = db.GetString(reader, 17);
        entities.NcpNonCooperation.Note = db.GetNullableString(reader, 18);
        entities.NcpNonCooperation.Populated = true;
      });
  }

  private IEnumerable<bool> ReadNcpNonCooperationCsePerson1()
  {
    entities.ApCsePerson.Populated = false;
    entities.NcpNonCooperation.Populated = false;

    return ReadEach("ReadNcpNonCooperationCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.SetNullableDate(
          command, "effectiveDt",
          export.Last.EffectiveDate.GetValueOrDefault());
        db.SetNullableDateTime(
          command, "lastUpdatedTmst",
          export.Last.LastUpdatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NcpNonCooperation.EffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.NcpNonCooperation.ReasonCode = db.GetNullableString(reader, 1);
        entities.NcpNonCooperation.Letter1Date = db.GetNullableDate(reader, 2);
        entities.NcpNonCooperation.Letter1Code =
          db.GetNullableString(reader, 3);
        entities.NcpNonCooperation.Letter2Date = db.GetNullableDate(reader, 4);
        entities.NcpNonCooperation.Letter2Code =
          db.GetNullableString(reader, 5);
        entities.NcpNonCooperation.Phone1Date = db.GetNullableDate(reader, 6);
        entities.NcpNonCooperation.Phone1Code = db.GetNullableString(reader, 7);
        entities.NcpNonCooperation.Phone2Date = db.GetNullableDate(reader, 8);
        entities.NcpNonCooperation.Phone2Code = db.GetNullableString(reader, 9);
        entities.NcpNonCooperation.EndDate = db.GetNullableDate(reader, 10);
        entities.NcpNonCooperation.EndStatusCode =
          db.GetNullableString(reader, 11);
        entities.NcpNonCooperation.CreatedBy = db.GetString(reader, 12);
        entities.NcpNonCooperation.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.NcpNonCooperation.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.NcpNonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.NcpNonCooperation.CasNumber = db.GetString(reader, 16);
        entities.NcpNonCooperation.CspNumber = db.GetString(reader, 17);
        entities.ApCsePerson.Number = db.GetString(reader, 17);
        entities.NcpNonCooperation.Note = db.GetNullableString(reader, 18);
        entities.ApCsePerson.Populated = true;
        entities.NcpNonCooperation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNcpNonCooperationCsePerson2()
  {
    entities.ApCsePerson.Populated = false;
    entities.NcpNonCooperation.Populated = false;

    return ReadEach("ReadNcpNonCooperationCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.NcpNonCooperation.EffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.NcpNonCooperation.ReasonCode = db.GetNullableString(reader, 1);
        entities.NcpNonCooperation.Letter1Date = db.GetNullableDate(reader, 2);
        entities.NcpNonCooperation.Letter1Code =
          db.GetNullableString(reader, 3);
        entities.NcpNonCooperation.Letter2Date = db.GetNullableDate(reader, 4);
        entities.NcpNonCooperation.Letter2Code =
          db.GetNullableString(reader, 5);
        entities.NcpNonCooperation.Phone1Date = db.GetNullableDate(reader, 6);
        entities.NcpNonCooperation.Phone1Code = db.GetNullableString(reader, 7);
        entities.NcpNonCooperation.Phone2Date = db.GetNullableDate(reader, 8);
        entities.NcpNonCooperation.Phone2Code = db.GetNullableString(reader, 9);
        entities.NcpNonCooperation.EndDate = db.GetNullableDate(reader, 10);
        entities.NcpNonCooperation.EndStatusCode =
          db.GetNullableString(reader, 11);
        entities.NcpNonCooperation.CreatedBy = db.GetString(reader, 12);
        entities.NcpNonCooperation.CreatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.NcpNonCooperation.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.NcpNonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 15);
        entities.NcpNonCooperation.CasNumber = db.GetString(reader, 16);
        entities.NcpNonCooperation.CspNumber = db.GetString(reader, 17);
        entities.ApCsePerson.Number = db.GetString(reader, 17);
        entities.NcpNonCooperation.Note = db.GetNullableString(reader, 18);
        entities.ApCsePerson.Populated = true;
        entities.NcpNonCooperation.Populated = true;

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
    /// <summary>A PagenumGroup group.</summary>
    [Serializable]
    public class PagenumGroup
    {
      /// <summary>
      /// A value of Pagenum1.
      /// </summary>
      [JsonPropertyName("pagenum1")]
      public NcpNonCooperation Pagenum1
      {
        get => pagenum1 ??= new();
        set => pagenum1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private NcpNonCooperation pagenum1;
    }

    /// <summary>A HiddenGrpNcGroup group.</summary>
    [Serializable]
    public class HiddenGrpNcGroup
    {
      /// <summary>
      /// A value of HiddenEndDate.
      /// </summary>
      [JsonPropertyName("hiddenEndDate")]
      public DateWorkArea HiddenEndDate
      {
        get => hiddenEndDate ??= new();
        set => hiddenEndDate = value;
      }

      /// <summary>
      /// A value of HiddenEffDate.
      /// </summary>
      [JsonPropertyName("hiddenEffDate")]
      public DateWorkArea HiddenEffDate
      {
        get => hiddenEffDate ??= new();
        set => hiddenEffDate = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public NcpNonCooperation Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of HiddenGrpNcDetAp.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcDetAp")]
      public CsePersonsWorkSet HiddenGrpNcDetAp
      {
        get => hiddenGrpNcDetAp ??= new();
        set => hiddenGrpNcDetAp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private DateWorkArea hiddenEndDate;
      private DateWorkArea hiddenEffDate;
      private NcpNonCooperation hidden;
      private CsePersonsWorkSet hiddenGrpNcDetAp;
    }

    /// <summary>A NcGroup group.</summary>
    [Serializable]
    public class NcGroup
    {
      /// <summary>
      /// A value of NcSelect.
      /// </summary>
      [JsonPropertyName("ncSelect")]
      public Common NcSelect
      {
        get => ncSelect ??= new();
        set => ncSelect = value;
      }

      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CsePersonsWorkSet Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>
      /// A value of NcpNonCooperation.
      /// </summary>
      [JsonPropertyName("ncpNonCooperation")]
      public NcpNonCooperation NcpNonCooperation
      {
        get => ncpNonCooperation ??= new();
        set => ncpNonCooperation = value;
      }

      /// <summary>
      /// A value of EffDate.
      /// </summary>
      [JsonPropertyName("effDate")]
      public DateWorkArea EffDate
      {
        get => effDate ??= new();
        set => effDate = value;
      }

      /// <summary>
      /// A value of EndDate.
      /// </summary>
      [JsonPropertyName("endDate")]
      public DateWorkArea EndDate
      {
        get => endDate ??= new();
        set => endDate = value;
      }

      /// <summary>
      /// A value of EndReasonPrompt.
      /// </summary>
      [JsonPropertyName("endReasonPrompt")]
      public Common EndReasonPrompt
      {
        get => endReasonPrompt ??= new();
        set => endReasonPrompt = value;
      }

      /// <summary>
      /// A value of Phone2Prompt.
      /// </summary>
      [JsonPropertyName("phone2Prompt")]
      public Common Phone2Prompt
      {
        get => phone2Prompt ??= new();
        set => phone2Prompt = value;
      }

      /// <summary>
      /// A value of Phone1Prompt.
      /// </summary>
      [JsonPropertyName("phone1Prompt")]
      public Common Phone1Prompt
      {
        get => phone1Prompt ??= new();
        set => phone1Prompt = value;
      }

      /// <summary>
      /// A value of Ltr2Prompt.
      /// </summary>
      [JsonPropertyName("ltr2Prompt")]
      public Common Ltr2Prompt
      {
        get => ltr2Prompt ??= new();
        set => ltr2Prompt = value;
      }

      /// <summary>
      /// A value of Ltr1Prompt.
      /// </summary>
      [JsonPropertyName("ltr1Prompt")]
      public Common Ltr1Prompt
      {
        get => ltr1Prompt ??= new();
        set => ltr1Prompt = value;
      }

      /// <summary>
      /// A value of ReasonPrompt.
      /// </summary>
      [JsonPropertyName("reasonPrompt")]
      public Common ReasonPrompt
      {
        get => reasonPrompt ??= new();
        set => reasonPrompt = value;
      }

      /// <summary>
      /// A value of NcCodePrompt.
      /// </summary>
      [JsonPropertyName("ncCodePrompt")]
      public Common NcCodePrompt
      {
        get => ncCodePrompt ??= new();
        set => ncCodePrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common ncSelect;
      private CsePersonsWorkSet ap;
      private NcpNonCooperation ncpNonCooperation;
      private DateWorkArea effDate;
      private DateWorkArea endDate;
      private Common endReasonPrompt;
      private Common phone2Prompt;
      private Common phone1Prompt;
      private Common ltr2Prompt;
      private Common ltr1Prompt;
      private Common reasonPrompt;
      private Common ncCodePrompt;
    }

    /// <summary>
    /// A value of NewSelect.
    /// </summary>
    [JsonPropertyName("newSelect")]
    public Common NewSelect
    {
      get => newSelect ??= new();
      set => newSelect = value;
    }

    /// <summary>
    /// A value of NewAp.
    /// </summary>
    [JsonPropertyName("newAp")]
    public CsePersonsWorkSet NewAp
    {
      get => newAp ??= new();
      set => newAp = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public NcpNonCooperation Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// Gets a value of Pagenum.
    /// </summary>
    [JsonIgnore]
    public Array<PagenumGroup> Pagenum => pagenum ??= new(
      PagenumGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Pagenum for json serialization.
    /// </summary>
    [JsonPropertyName("pagenum")]
    [Computed]
    public IList<PagenumGroup> Pagenum_Json
    {
      get => pagenum;
      set => Pagenum.Assign(value);
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public NcpNonCooperation New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of NewPersonPrompt.
    /// </summary>
    [JsonPropertyName("newPersonPrompt")]
    public Common NewPersonPrompt
    {
      get => newPersonPrompt ??= new();
      set => newPersonPrompt = value;
    }

    /// <summary>
    /// A value of NewEndReasponCdPrompt.
    /// </summary>
    [JsonPropertyName("newEndReasponCdPrompt")]
    public Common NewEndReasponCdPrompt
    {
      get => newEndReasponCdPrompt ??= new();
      set => newEndReasponCdPrompt = value;
    }

    /// <summary>
    /// A value of NewPhone2CdPrmt.
    /// </summary>
    [JsonPropertyName("newPhone2CdPrmt")]
    public Common NewPhone2CdPrmt
    {
      get => newPhone2CdPrmt ??= new();
      set => newPhone2CdPrmt = value;
    }

    /// <summary>
    /// A value of NewPhone1CdPrmt.
    /// </summary>
    [JsonPropertyName("newPhone1CdPrmt")]
    public Common NewPhone1CdPrmt
    {
      get => newPhone1CdPrmt ??= new();
      set => newPhone1CdPrmt = value;
    }

    /// <summary>
    /// A value of NewLetter2CdPrmt.
    /// </summary>
    [JsonPropertyName("newLetter2CdPrmt")]
    public Common NewLetter2CdPrmt
    {
      get => newLetter2CdPrmt ??= new();
      set => newLetter2CdPrmt = value;
    }

    /// <summary>
    /// A value of NewLetter1CdPrmt.
    /// </summary>
    [JsonPropertyName("newLetter1CdPrmt")]
    public Common NewLetter1CdPrmt
    {
      get => newLetter1CdPrmt ??= new();
      set => newLetter1CdPrmt = value;
    }

    /// <summary>
    /// A value of SuccessfullyDisplayed.
    /// </summary>
    [JsonPropertyName("successfullyDisplayed")]
    public Common SuccessfullyDisplayed
    {
      get => successfullyDisplayed ??= new();
      set => successfullyDisplayed = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of HiddenRedisplay.
    /// </summary>
    [JsonPropertyName("hiddenRedisplay")]
    public Common HiddenRedisplay
    {
      get => hiddenRedisplay ??= new();
      set => hiddenRedisplay = value;
    }

    /// <summary>
    /// A value of DesignatedPayeeFnd.
    /// </summary>
    [JsonPropertyName("designatedPayeeFnd")]
    public Common DesignatedPayeeFnd
    {
      get => designatedPayeeFnd ??= new();
      set => designatedPayeeFnd = value;
    }

    /// <summary>
    /// A value of Nc1.
    /// </summary>
    [JsonPropertyName("nc1")]
    public Standard Nc1
    {
      get => nc1 ??= new();
      set => nc1 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of NoItemsNc.
    /// </summary>
    [JsonPropertyName("noItemsNc")]
    public Common NoItemsNc
    {
      get => noItemsNc ??= new();
      set => noItemsNc = value;
    }

    /// <summary>
    /// A value of CurrItmNoNc.
    /// </summary>
    [JsonPropertyName("currItmNoNc")]
    public Common CurrItmNoNc
    {
      get => currItmNoNc ??= new();
      set => currItmNoNc = value;
    }

    /// <summary>
    /// A value of HidNoItemsFndNc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndNc")]
    public Common HidNoItemsFndNc
    {
      get => hidNoItemsFndNc ??= new();
      set => hidNoItemsFndNc = value;
    }

    /// <summary>
    /// A value of MaxPagesNc.
    /// </summary>
    [JsonPropertyName("maxPagesNc")]
    public Common MaxPagesNc
    {
      get => maxPagesNc ??= new();
      set => maxPagesNc = value;
    }

    /// <summary>
    /// A value of NcMinus.
    /// </summary>
    [JsonPropertyName("ncMinus")]
    public Standard NcMinus
    {
      get => ncMinus ??= new();
      set => ncMinus = value;
    }

    /// <summary>
    /// A value of NcPlus.
    /// </summary>
    [JsonPropertyName("ncPlus")]
    public Standard NcPlus
    {
      get => ncPlus ??= new();
      set => ncPlus = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of SelectedAp.
    /// </summary>
    [JsonPropertyName("selectedAp")]
    public CsePersonsWorkSet SelectedAp
    {
      get => selectedAp ??= new();
      set => selectedAp = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// Gets a value of HiddenGrpNc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpNcGroup> HiddenGrpNc => hiddenGrpNc ??= new(
      HiddenGrpNcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpNc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpNc")]
    [Computed]
    public IList<HiddenGrpNcGroup> HiddenGrpNc_Json
    {
      get => hiddenGrpNc;
      set => HiddenGrpNc.Assign(value);
    }

    /// <summary>
    /// Gets a value of Nc.
    /// </summary>
    [JsonIgnore]
    public Array<NcGroup> Nc => nc ??= new(NcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Nc for json serialization.
    /// </summary>
    [JsonPropertyName("nc")]
    [Computed]
    public IList<NcGroup> Nc_Json
    {
      get => nc;
      set => Nc.Assign(value);
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
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private Common newSelect;
    private CsePersonsWorkSet newAp;
    private NcpNonCooperation last;
    private Array<PagenumGroup> pagenum;
    private NcpNonCooperation new1;
    private Common newPersonPrompt;
    private Common newEndReasponCdPrompt;
    private Common newPhone2CdPrmt;
    private Common newPhone1CdPrmt;
    private Common newLetter2CdPrmt;
    private Common newLetter1CdPrmt;
    private Common successfullyDisplayed;
    private Common caseOpen;
    private Common hiddenRedisplay;
    private Common designatedPayeeFnd;
    private Standard nc1;
    private Case1 prev;
    private Common noItemsNc;
    private Common currItmNoNc;
    private Common hidNoItemsFndNc;
    private Common maxPagesNc;
    private Standard ncMinus;
    private Standard ncPlus;
    private CodeValue selected;
    private Common apPrompt;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Case1 case1;
    private CsePersonsWorkSet hiddenPrev;
    private Case1 next;
    private CsePersonsWorkSet selectedAp;
    private Standard standard;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<HiddenGrpNcGroup> hiddenGrpNc;
    private Array<NcGroup> nc;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 hiddenCase;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PagenumGroup group.</summary>
    [Serializable]
    public class PagenumGroup
    {
      /// <summary>
      /// A value of Pagenum1.
      /// </summary>
      [JsonPropertyName("pagenum1")]
      public NcpNonCooperation Pagenum1
      {
        get => pagenum1 ??= new();
        set => pagenum1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private NcpNonCooperation pagenum1;
    }

    /// <summary>A HiddenGrpNcGroup group.</summary>
    [Serializable]
    public class HiddenGrpNcGroup
    {
      /// <summary>
      /// A value of HiddenGrpNcAp.
      /// </summary>
      [JsonPropertyName("hiddenGrpNcAp")]
      public CsePersonsWorkSet HiddenGrpNcAp
      {
        get => hiddenGrpNcAp ??= new();
        set => hiddenGrpNcAp = value;
      }

      /// <summary>
      /// A value of HiddenEffDate.
      /// </summary>
      [JsonPropertyName("hiddenEffDate")]
      public DateWorkArea HiddenEffDate
      {
        get => hiddenEffDate ??= new();
        set => hiddenEffDate = value;
      }

      /// <summary>
      /// A value of HiddenEndDate.
      /// </summary>
      [JsonPropertyName("hiddenEndDate")]
      public DateWorkArea HiddenEndDate
      {
        get => hiddenEndDate ??= new();
        set => hiddenEndDate = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public NcpNonCooperation Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePersonsWorkSet hiddenGrpNcAp;
      private DateWorkArea hiddenEffDate;
      private DateWorkArea hiddenEndDate;
      private NcpNonCooperation hidden;
    }

    /// <summary>A NcGroup group.</summary>
    [Serializable]
    public class NcGroup
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
      /// A value of NcAp.
      /// </summary>
      [JsonPropertyName("ncAp")]
      public CsePersonsWorkSet NcAp
      {
        get => ncAp ??= new();
        set => ncAp = value;
      }

      /// <summary>
      /// A value of NcpNonCooperation.
      /// </summary>
      [JsonPropertyName("ncpNonCooperation")]
      public NcpNonCooperation NcpNonCooperation
      {
        get => ncpNonCooperation ??= new();
        set => ncpNonCooperation = value;
      }

      /// <summary>
      /// A value of EffDate.
      /// </summary>
      [JsonPropertyName("effDate")]
      public DateWorkArea EffDate
      {
        get => effDate ??= new();
        set => effDate = value;
      }

      /// <summary>
      /// A value of EndDate.
      /// </summary>
      [JsonPropertyName("endDate")]
      public DateWorkArea EndDate
      {
        get => endDate ??= new();
        set => endDate = value;
      }

      /// <summary>
      /// A value of EndReasonPrompt.
      /// </summary>
      [JsonPropertyName("endReasonPrompt")]
      public Common EndReasonPrompt
      {
        get => endReasonPrompt ??= new();
        set => endReasonPrompt = value;
      }

      /// <summary>
      /// A value of Phone2Prompt.
      /// </summary>
      [JsonPropertyName("phone2Prompt")]
      public Common Phone2Prompt
      {
        get => phone2Prompt ??= new();
        set => phone2Prompt = value;
      }

      /// <summary>
      /// A value of Phone1Prompt.
      /// </summary>
      [JsonPropertyName("phone1Prompt")]
      public Common Phone1Prompt
      {
        get => phone1Prompt ??= new();
        set => phone1Prompt = value;
      }

      /// <summary>
      /// A value of Ltr2Prompt.
      /// </summary>
      [JsonPropertyName("ltr2Prompt")]
      public Common Ltr2Prompt
      {
        get => ltr2Prompt ??= new();
        set => ltr2Prompt = value;
      }

      /// <summary>
      /// A value of Ltr1Promt.
      /// </summary>
      [JsonPropertyName("ltr1Promt")]
      public Common Ltr1Promt
      {
        get => ltr1Promt ??= new();
        set => ltr1Promt = value;
      }

      /// <summary>
      /// A value of NcRsnPrompt.
      /// </summary>
      [JsonPropertyName("ncRsnPrompt")]
      public Common NcRsnPrompt
      {
        get => ncRsnPrompt ??= new();
        set => ncRsnPrompt = value;
      }

      /// <summary>
      /// A value of NcCodePrmpt.
      /// </summary>
      [JsonPropertyName("ncCodePrmpt")]
      public Common NcCodePrmpt
      {
        get => ncCodePrmpt ??= new();
        set => ncCodePrmpt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Common select;
      private CsePersonsWorkSet ncAp;
      private NcpNonCooperation ncpNonCooperation;
      private DateWorkArea effDate;
      private DateWorkArea endDate;
      private Common endReasonPrompt;
      private Common phone2Prompt;
      private Common phone1Prompt;
      private Common ltr2Prompt;
      private Common ltr1Promt;
      private Common ncRsnPrompt;
      private Common ncCodePrmpt;
    }

    /// <summary>
    /// A value of NewSelect.
    /// </summary>
    [JsonPropertyName("newSelect")]
    public Common NewSelect
    {
      get => newSelect ??= new();
      set => newSelect = value;
    }

    /// <summary>
    /// A value of NewAp.
    /// </summary>
    [JsonPropertyName("newAp")]
    public CsePersonsWorkSet NewAp
    {
      get => newAp ??= new();
      set => newAp = value;
    }

    /// <summary>
    /// A value of NewLetter1CdPrmt.
    /// </summary>
    [JsonPropertyName("newLetter1CdPrmt")]
    public Common NewLetter1CdPrmt
    {
      get => newLetter1CdPrmt ??= new();
      set => newLetter1CdPrmt = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public NcpNonCooperation Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// Gets a value of Pagenum.
    /// </summary>
    [JsonIgnore]
    public Array<PagenumGroup> Pagenum => pagenum ??= new(
      PagenumGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Pagenum for json serialization.
    /// </summary>
    [JsonPropertyName("pagenum")]
    [Computed]
    public IList<PagenumGroup> Pagenum_Json
    {
      get => pagenum;
      set => Pagenum.Assign(value);
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public NcpNonCooperation New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of NewPersonPrompt.
    /// </summary>
    [JsonPropertyName("newPersonPrompt")]
    public Common NewPersonPrompt
    {
      get => newPersonPrompt ??= new();
      set => newPersonPrompt = value;
    }

    /// <summary>
    /// A value of NewEndReasonCdPromt.
    /// </summary>
    [JsonPropertyName("newEndReasonCdPromt")]
    public Common NewEndReasonCdPromt
    {
      get => newEndReasonCdPromt ??= new();
      set => newEndReasonCdPromt = value;
    }

    /// <summary>
    /// A value of NewPhone2CdPrmt.
    /// </summary>
    [JsonPropertyName("newPhone2CdPrmt")]
    public Common NewPhone2CdPrmt
    {
      get => newPhone2CdPrmt ??= new();
      set => newPhone2CdPrmt = value;
    }

    /// <summary>
    /// A value of NewPhone1CdPrmt.
    /// </summary>
    [JsonPropertyName("newPhone1CdPrmt")]
    public Common NewPhone1CdPrmt
    {
      get => newPhone1CdPrmt ??= new();
      set => newPhone1CdPrmt = value;
    }

    /// <summary>
    /// A value of NewLetter2CdPrmt.
    /// </summary>
    [JsonPropertyName("newLetter2CdPrmt")]
    public Common NewLetter2CdPrmt
    {
      get => newLetter2CdPrmt ??= new();
      set => newLetter2CdPrmt = value;
    }

    /// <summary>
    /// A value of SuccessfullyDisplayed.
    /// </summary>
    [JsonPropertyName("successfullyDisplayed")]
    public Common SuccessfullyDisplayed
    {
      get => successfullyDisplayed ??= new();
      set => successfullyDisplayed = value;
    }

    /// <summary>
    /// A value of DuplicateIndPromptDel.
    /// </summary>
    [JsonPropertyName("duplicateIndPromptDel")]
    public Common DuplicateIndPromptDel
    {
      get => duplicateIndPromptDel ??= new();
      set => duplicateIndPromptDel = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// A value of HiddenRedisplay.
    /// </summary>
    [JsonPropertyName("hiddenRedisplay")]
    public Common HiddenRedisplay
    {
      get => hiddenRedisplay ??= new();
      set => hiddenRedisplay = value;
    }

    /// <summary>
    /// A value of PaMedCdPromptDel.
    /// </summary>
    [JsonPropertyName("paMedCdPromptDel")]
    public Common PaMedCdPromptDel
    {
      get => paMedCdPromptDel ??= new();
      set => paMedCdPromptDel = value;
    }

    /// <summary>
    /// A value of DesignatedPayeeFnd.
    /// </summary>
    [JsonPropertyName("designatedPayeeFnd")]
    public Common DesignatedPayeeFnd
    {
      get => designatedPayeeFnd ??= new();
      set => designatedPayeeFnd = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Case1 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of NoItemsNc.
    /// </summary>
    [JsonPropertyName("noItemsNc")]
    public Common NoItemsNc
    {
      get => noItemsNc ??= new();
      set => noItemsNc = value;
    }

    /// <summary>
    /// A value of CurrItmNoNc.
    /// </summary>
    [JsonPropertyName("currItmNoNc")]
    public Common CurrItmNoNc
    {
      get => currItmNoNc ??= new();
      set => currItmNoNc = value;
    }

    /// <summary>
    /// A value of HidNoItemsFndNc.
    /// </summary>
    [JsonPropertyName("hidNoItemsFndNc")]
    public Common HidNoItemsFndNc
    {
      get => hidNoItemsFndNc ??= new();
      set => hidNoItemsFndNc = value;
    }

    /// <summary>
    /// A value of MaxPagesNc.
    /// </summary>
    [JsonPropertyName("maxPagesNc")]
    public Common MaxPagesNc
    {
      get => maxPagesNc ??= new();
      set => maxPagesNc = value;
    }

    /// <summary>
    /// A value of Nc1.
    /// </summary>
    [JsonPropertyName("nc1")]
    public Standard Nc1
    {
      get => nc1 ??= new();
      set => nc1 = value;
    }

    /// <summary>
    /// A value of SaveNc.
    /// </summary>
    [JsonPropertyName("saveNc")]
    public CsePerson SaveNc
    {
      get => saveNc ??= new();
      set => saveNc = value;
    }

    /// <summary>
    /// A value of NcMinus.
    /// </summary>
    [JsonPropertyName("ncMinus")]
    public Standard NcMinus
    {
      get => ncMinus ??= new();
      set => ncMinus = value;
    }

    /// <summary>
    /// A value of NcPlus.
    /// </summary>
    [JsonPropertyName("ncPlus")]
    public Standard NcPlus
    {
      get => ncPlus ??= new();
      set => ncPlus = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Code Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public Common ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePersonsWorkSet HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of SelectedAp.
    /// </summary>
    [JsonPropertyName("selectedAp")]
    public CsePersonsWorkSet SelectedAp
    {
      get => selectedAp ??= new();
      set => selectedAp = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// Gets a value of HiddenGrpNc.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGrpNcGroup> HiddenGrpNc => hiddenGrpNc ??= new(
      HiddenGrpNcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenGrpNc for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenGrpNc")]
    [Computed]
    public IList<HiddenGrpNcGroup> HiddenGrpNc_Json
    {
      get => hiddenGrpNc;
      set => HiddenGrpNc.Assign(value);
    }

    /// <summary>
    /// Gets a value of Nc.
    /// </summary>
    [JsonIgnore]
    public Array<NcGroup> Nc => nc ??= new(NcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Nc for json serialization.
    /// </summary>
    [JsonPropertyName("nc")]
    [Computed]
    public IList<NcGroup> Nc_Json
    {
      get => nc;
      set => Nc.Assign(value);
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
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private Common newSelect;
    private CsePersonsWorkSet newAp;
    private Common newLetter1CdPrmt;
    private NcpNonCooperation last;
    private Array<PagenumGroup> pagenum;
    private NcpNonCooperation new1;
    private Common newPersonPrompt;
    private Common newEndReasonCdPromt;
    private Common newPhone2CdPrmt;
    private Common newPhone1CdPrmt;
    private Common newLetter2CdPrmt;
    private Common successfullyDisplayed;
    private Common duplicateIndPromptDel;
    private Common caseOpen;
    private Common hiddenRedisplay;
    private Common paMedCdPromptDel;
    private Common designatedPayeeFnd;
    private Case1 prev;
    private Common noItemsNc;
    private Common currItmNoNc;
    private Common hidNoItemsFndNc;
    private Common maxPagesNc;
    private Standard nc1;
    private CsePerson saveNc;
    private Standard ncMinus;
    private Standard ncPlus;
    private Code prompt;
    private Common apPrompt;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Case1 case1;
    private CsePersonsWorkSet hiddenPrev;
    private Case1 next;
    private CsePersonsWorkSet selectedAp;
    private Standard standard;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<HiddenGrpNcGroup> hiddenGrpNc;
    private Array<NcGroup> nc;
    private NextTranInfo hiddenNextTranInfo;
    private Case1 hiddenCase;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CsePerson Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private CsePerson ap;
    }

    /// <summary>
    /// A value of UpdatedRecord.
    /// </summary>
    [JsonPropertyName("updatedRecord")]
    public NcpNonCooperation UpdatedRecord
    {
      get => updatedRecord ??= new();
      set => updatedRecord = value;
    }

    /// <summary>
    /// A value of SuccessfullyUpdated.
    /// </summary>
    [JsonPropertyName("successfullyUpdated")]
    public Common SuccessfullyUpdated
    {
      get => successfullyUpdated ??= new();
      set => successfullyUpdated = value;
    }

    /// <summary>
    /// A value of NoRecordSelected.
    /// </summary>
    [JsonPropertyName("noRecordSelected")]
    public Common NoRecordSelected
    {
      get => noRecordSelected ??= new();
      set => noRecordSelected = value;
    }

    /// <summary>
    /// A value of TestNew.
    /// </summary>
    [JsonPropertyName("testNew")]
    public NcpNonCooperation TestNew
    {
      get => testNew ??= new();
      set => testNew = value;
    }

    /// <summary>
    /// A value of ClearNcpNonCooperation.
    /// </summary>
    [JsonPropertyName("clearNcpNonCooperation")]
    public NcpNonCooperation ClearNcpNonCooperation
    {
      get => clearNcpNonCooperation ??= new();
      set => clearNcpNonCooperation = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
    }

    /// <summary>
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of NoAps.
    /// </summary>
    [JsonPropertyName("noAps")]
    public Common NoAps
    {
      get => noAps ??= new();
      set => noAps = value;
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
    /// A value of ClearCommon.
    /// </summary>
    [JsonPropertyName("clearCommon")]
    public Common ClearCommon
    {
      get => clearCommon ??= new();
      set => clearCommon = value;
    }

    /// <summary>
    /// A value of ClearCaseRole.
    /// </summary>
    [JsonPropertyName("clearCaseRole")]
    public CaseRole ClearCaseRole
    {
      get => clearCaseRole ??= new();
      set => clearCaseRole = value;
    }

    /// <summary>
    /// A value of ClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("clearCsePersonsWorkSet")]
    public CsePersonsWorkSet ClearCsePersonsWorkSet
    {
      get => clearCsePersonsWorkSet ??= new();
      set => clearCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Row1AddFuncNc.
    /// </summary>
    [JsonPropertyName("row1AddFuncNc")]
    public Common Row1AddFuncNc
    {
      get => row1AddFuncNc ??= new();
      set => row1AddFuncNc = value;
    }

    /// <summary>
    /// A value of Row1AddFuncGc.
    /// </summary>
    [JsonPropertyName("row1AddFuncGc")]
    public Common Row1AddFuncGc
    {
      get => row1AddFuncGc ??= new();
      set => row1AddFuncGc = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of DataChanged.
    /// </summary>
    [JsonPropertyName("dataChanged")]
    public Common DataChanged
    {
      get => dataChanged ??= new();
      set => dataChanged = value;
    }

    /// <summary>
    /// A value of ItemsFndNc.
    /// </summary>
    [JsonPropertyName("itemsFndNc")]
    public Common ItemsFndNc
    {
      get => itemsFndNc ??= new();
      set => itemsFndNc = value;
    }

    /// <summary>
    /// A value of GrpGcCnt.
    /// </summary>
    [JsonPropertyName("grpGcCnt")]
    public Common GrpGcCnt
    {
      get => grpGcCnt ??= new();
      set => grpGcCnt = value;
    }

    /// <summary>
    /// A value of GrpNcCnt.
    /// </summary>
    [JsonPropertyName("grpNcCnt")]
    public Common GrpNcCnt
    {
      get => grpNcCnt ??= new();
      set => grpNcCnt = value;
    }

    /// <summary>
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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
    /// A value of Invalid.
    /// </summary>
    [JsonPropertyName("invalid")]
    public Common Invalid
    {
      get => invalid ??= new();
      set => invalid = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of SendClosureLtrReqd.
    /// </summary>
    [JsonPropertyName("sendClosureLtrReqd")]
    public Common SendClosureLtrReqd
    {
      get => sendClosureLtrReqd ??= new();
      set => sendClosureLtrReqd = value;
    }

    /// <summary>
    /// A value of Lt60Days.
    /// </summary>
    [JsonPropertyName("lt60Days")]
    public Common Lt60Days
    {
      get => lt60Days ??= new();
      set => lt60Days = value;
    }

    /// <summary>
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
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

    private NcpNonCooperation updatedRecord;
    private Common successfullyUpdated;
    private Common noRecordSelected;
    private NcpNonCooperation testNew;
    private NcpNonCooperation clearNcpNonCooperation;
    private Common work;
    private Infrastructure infrastructure;
    private Case1 case1;
    private Common screenIdentification;
    private Array<GrpGroup> grp;
    private Common position;
    private Common noAps;
    private DateWorkArea current;
    private Common clearCommon;
    private CaseRole clearCaseRole;
    private CsePersonsWorkSet clearCsePersonsWorkSet;
    private Common row1AddFuncNc;
    private Common row1AddFuncGc;
    private Common count;
    private Common dataChanged;
    private Common itemsFndNc;
    private Common grpGcCnt;
    private Common grpNcCnt;
    private Common multipleAps;
    private AbendData abendData;
    private Code code;
    private CodeValue codeValue;
    private Common common;
    private Common invalid;
    private DateWorkArea blank;
    private DateWorkArea dateWorkArea;
    private Common sendClosureLtrReqd;
    private Common lt60Days;
    private Infrastructure lastTran;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of NcpNonCooperation.
    /// </summary>
    [JsonPropertyName("ncpNonCooperation")]
    public NcpNonCooperation NcpNonCooperation
    {
      get => ncpNonCooperation ??= new();
      set => ncpNonCooperation = value;
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
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public CsePerson KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
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

    private CsePerson apCsePerson;
    private NcpNonCooperation ncpNonCooperation;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount csePersonAccount;
    private CaseUnit caseUnit;
    private PersonProgram personProgram;
    private Program program;
    private CaseRole apCaseRole;
    private CsePerson keyOnly;
    private Case1 case1;
  }
#endregion
}
