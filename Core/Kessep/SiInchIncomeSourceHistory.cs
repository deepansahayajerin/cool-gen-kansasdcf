// Program: SI_INCH_INCOME_SOURCE_HISTORY, ID: 371766268, model: 746.
// Short name: SWEINCHP
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
/// A program: SI_INCH_INCOME_SOURCE_HISTORY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiInchIncomeSourceHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INCH_INCOME_SOURCE_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiInchIncomeSourceHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiInchIncomeSourceHistory.
  /// </summary>
  public SiInchIncomeSourceHistory(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date	  Author	Reason
    // 03/12/96  Lewis		Initial Development
    // 05/2/96	  Rao		Changes to Import Views & Menu Transfer
    // 11/03/96  G. Lofton	Add new security and removed old.
    // 12/24/96  Raju		Event insertion
    // 04/22/97  Sid		Modify event insertion logic.
    // 07/02/97  Sid		Cleanup.
    // 06/04/99  M. Lachowicz  Check if screen dates are not
    //                         greater then max date
    // 05/25/99 W.Campbell     Replaced zd exit states.
    // 10/28/02  K.Doshi       Fix screen Help.
    // --------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    MoveStandard1(import.Standard, export.Standard);
    export.Next.Number = import.Next.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.IncomeSource.Assign(import.IncomeSource);
    export.PersonPrompt.SelectChar = import.PersonPrompt.SelectChar;

    for(import.Cases.Index = 0; import.Cases.Index < import.Cases.Count; ++
      import.Cases.Index)
    {
      if (!import.Cases.CheckSize())
      {
        break;
      }

      export.Cases.Index = import.Cases.Index;
      export.Cases.CheckSize();

      export.Cases.Update.Case1.Number = import.Cases.Item.Case1.Number;
    }

    import.Cases.CheckIndex();

    for(import.IncomeHistory.Index = 0; import.IncomeHistory.Index < import
      .IncomeHistory.Count; ++import.IncomeHistory.Index)
    {
      if (!import.IncomeHistory.CheckSize())
      {
        break;
      }

      export.IncomeHistory.Index = import.IncomeHistory.Index;
      export.IncomeHistory.CheckSize();

      export.IncomeHistory.Update.Common.SelectChar =
        import.IncomeHistory.Item.Common.SelectChar;
      export.IncomeHistory.Update.FreqPrompt.SelectChar =
        import.IncomeHistory.Item.FreqPrompt.SelectChar;
      export.IncomeHistory.Update.PersonIncomeHistory.Assign(
        import.IncomeHistory.Item.PersonIncomeHistory);

      if (AsChar(export.IncomeHistory.Item.Common.SelectChar) == '*')
      {
        export.IncomeHistory.Update.Common.SelectChar = "";
      }
    }

    import.IncomeHistory.CheckIndex();

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenCsePersonsWorkSet.Number =
      import.HiddenCsePersonsWorkSet.Number;
    export.Cases1.PageNumber = import.Cases1.PageNumber;
    MoveStandard2(import.IncHist, export.IncHist);

    for(import.PageKeysCases.Index = 0; import.PageKeysCases.Index < import
      .PageKeysCases.Count; ++import.PageKeysCases.Index)
    {
      if (!import.PageKeysCases.CheckSize())
      {
        break;
      }

      export.PageKeysCases.Index = import.PageKeysCases.Index;
      export.PageKeysCases.CheckSize();

      export.PageKeysCases.Update.PageKey.Number =
        import.PageKeysCases.Item.PageKey.Number;
    }

    import.PageKeysCases.CheckIndex();

    for(import.PageKeysIncHist.Index = 0; import.PageKeysIncHist.Index < import
      .PageKeysIncHist.Count; ++import.PageKeysIncHist.Index)
    {
      if (!import.PageKeysIncHist.CheckSize())
      {
        break;
      }

      export.PageKeysIncHist.Index = import.PageKeysIncHist.Index;
      export.PageKeysIncHist.CheckSize();

      MovePersonIncomeHistory1(import.PageKeysIncHist.Item.PageKey,
        export.PageKeysIncHist.Update.PageKey);
    }

    import.PageKeysIncHist.CheckIndex();

    for(import.HiddenIncomeHist.Index = 0; import.HiddenIncomeHist.Index < import
      .HiddenIncomeHist.Count; ++import.HiddenIncomeHist.Index)
    {
      if (!import.HiddenIncomeHist.CheckSize())
      {
        break;
      }

      export.HiddenIncomeHist.Index = import.HiddenIncomeHist.Index;
      export.HiddenIncomeHist.CheckSize();

      export.HiddenIncomeHist.Update.Hidden.VerifiedDt =
        import.HiddenIncomeHist.Item.Hidden.VerifiedDt;

      // ---------------------------------------------
      // Start of code - Raju 12/24/1996:0630 hrs CST 
      // ---------------------------------------------
      export.HiddenIncomeHist.Update.LastReadTotal.TotalCurrency =
        import.HiddenIncomeHist.Item.LastReadTotal.TotalCurrency;

      // ---------------------------------------------
      // Start of code
      // ---------------------------------------------
    }

    import.HiddenIncomeHist.CheckIndex();

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      export.HiddenNextTranInfo.CsePersonNumber =
        export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
      export.CsePersonsWorkSet.Number =
        export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      UseCabZeroFillNumber1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      // ---------------------------------------------
      // Start of Code (Raju 01/20/97:1035 hrs CST)
      // ---------------------------------------------
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        local.LastTran.SystemGeneratedIdentifier =
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.Next.Number = local.LastTran.CaseNumber ?? Spaces(10);
        export.CsePersonsWorkSet.Number = local.LastTran.CsePersonNumber ?? Spaces
          (10);
        export.IncomeSource.Identifier = local.LastTran.DenormTimestamp;
      }

      // ---------------------------------------------
      // End  of Code
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      export.CsePersonsWorkSet.Number = import.FromMenu.Number;
      global.Command = "DISPLAY";
    }

    UseCabZeroFillNumber2();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "INCS") || Equal(global.Command, "EMPL") || Equal
      (global.Command, "INCL") || Equal(global.Command, "IHNX") || Equal
      (global.Command, "IHPV") || Equal(global.Command, "MCNX") || Equal
      (global.Command, "MCPV"))
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

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    // ------------------------------------------
    // If control is returned from a list screen,
    // populate the appropriate fields.
    // ------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(export.PersonPrompt.SelectChar) == 'S')
      {
        export.PersonPrompt.SelectChar = "";

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Protected = false;
        field.Focused = true;

        goto Test;
      }

      for(export.IncomeHistory.Index = 0; export.IncomeHistory.Index < export
        .IncomeHistory.Count; ++export.IncomeHistory.Index)
      {
        if (!export.IncomeHistory.CheckSize())
        {
          break;
        }

        if (AsChar(export.IncomeHistory.Item.FreqPrompt.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.IncomeHistory.Update.PersonIncomeHistory.Freq =
              import.Selected.Cdvalue;
          }

          export.IncomeHistory.Update.FreqPrompt.SelectChar = "";

          var field =
            GetField(export.IncomeHistory.Item.PersonIncomeHistory, "freq");

          field.Protected = false;
          field.Focused = true;

          return;
        }
      }

      export.IncomeHistory.CheckIndex();
    }

Test:

    if (!Equal(export.CsePersonsWorkSet.Number,
      export.HiddenCsePersonsWorkSet.Number))
    {
      if (!Equal(global.Command, "DISPLAY"))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_DISPLAY_FIRST";

        return;
      }
      else
      {
        export.PageKeysCases.Count = 0;
        export.PageKeysIncHist.Count = 0;
        export.Cases1.PageNumber = 1;
        export.IncHist.PageNumber = 1;

        export.PageKeysIncHist.Index = export.IncHist.PageNumber - 1;
        export.PageKeysIncHist.CheckSize();

        export.PageKeysIncHist.Update.PageKey.IncomeEffDt =
          UseCabSetMaximumDiscontinueDate2();

        if (!IsEmpty(export.HiddenCsePersonsWorkSet.Number))
        {
          export.IncomeSource.Identifier = local.BlankIncomeSource.Identifier;
        }
      }
    }

    local.Select.Count = 0;

    for(export.IncomeHistory.Index = 0; export.IncomeHistory.Index < export
      .IncomeHistory.Count; ++export.IncomeHistory.Index)
    {
      if (!export.IncomeHistory.CheckSize())
      {
        break;
      }

      switch(AsChar(export.IncomeHistory.Item.Common.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.Select.Count;

          break;
        default:
          var field = GetField(export.IncomeHistory.Item.Common, "selectChar");

          field.Error = true;

          // ---------------------------------------------
          // 05/25/99 W.Campbell - Replaced zd exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
      }
    }

    export.IncomeHistory.CheckIndex();

    if (local.Select.Count > 0 && (Equal(global.Command, "IHPV") || Equal
      (global.Command, "IHNX")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

      return;
    }

    if (local.Select.Count == 0 && (Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE")))
    {
      ExitState = "ACO_NE0000_NO_SELECTION_MADE";

      return;
    }

    // ----------------
    // Field Validation
    // ----------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // 04/06/99 M. Lachowicz Start
      UseCabSetMaximumDiscontinueDate1();

      if (Lt(local.MaxDate.Date, export.IncomeSource.EndDt))
      {
        var field = GetField(export.IncomeSource, "startDt");

        field.Error = true;

        ExitState = "ACO_NI0000_INVALID_DATE";

        return;
      }

      if (Lt(local.MaxDate.Date, export.IncomeSource.StartDt))
      {
        var field = GetField(export.IncomeSource, "endDt");

        field.Error = true;

        ExitState = "ACO_NI0000_INVALID_DATE";

        return;
      }

      // 04/06/99 M. Lachowicz End
      export.IncomeHistory.Index = 0;

      for(var limit = export.IncomeHistory.Count; export.IncomeHistory.Index < limit
        ; ++export.IncomeHistory.Index)
      {
        if (!export.IncomeHistory.CheckSize())
        {
          break;
        }

        if (AsChar(export.IncomeHistory.Item.Common.SelectChar) == 'S')
        {
          if (Equal(global.Command, "ADD"))
          {
            if (!Equal(export.IncomeHistory.Item.PersonIncomeHistory.Identifier,
              local.BlankPersonIncomeHistory.Identifier))
            {
              var field =
                GetField(export.IncomeHistory.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              return;
            }
          }
          else if (Equal(global.Command, "UPDATE"))
          {
            if (Equal(export.IncomeHistory.Item.PersonIncomeHistory.Identifier,
              local.BlankPersonIncomeHistory.Identifier))
            {
              var field =
                GetField(export.IncomeHistory.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              return;
            }
          }

          if (Equal(export.IncomeHistory.Item.PersonIncomeHistory.IncomeEffDt,
            local.BlankPersonIncomeHistory.IncomeEffDt))
          {
            var field =
              GetField(export.IncomeHistory.Item.PersonIncomeHistory,
              "incomeEffDt");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }

          // ----------------------------
          // Effective Date >= Start Date
          //                <= End Date
          // ----------------------------
          local.DateWorkArea.Date = export.IncomeSource.EndDt;
          local.IncomeSource.EndDt = UseCabSetMaximumDiscontinueDate3();

          if (Lt(export.IncomeHistory.Item.PersonIncomeHistory.IncomeEffDt,
            export.IncomeSource.StartDt) || Lt
            (local.IncomeSource.EndDt,
            export.IncomeHistory.Item.PersonIncomeHistory.IncomeEffDt))
          {
            var field =
              GetField(export.IncomeHistory.Item.PersonIncomeHistory,
              "incomeEffDt");

            field.Error = true;

            ExitState = "SI0000_INVALID_EFFECTIVE_DATE";

            return;
          }

          if (export.IncomeHistory.Item.PersonIncomeHistory.IncomeAmt.
            GetValueOrDefault() == 0)
          {
            var field =
              GetField(export.IncomeHistory.Item.PersonIncomeHistory,
              "incomeAmt");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }

          if (!IsEmpty(export.IncomeHistory.Item.PersonIncomeHistory.Freq))
          {
            local.Code.CodeName = "SI FREQUENCY";
            local.CodeValue.Cdvalue =
              export.IncomeHistory.Item.PersonIncomeHistory.Freq ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.ValidValue.Flag) == 'N')
            {
              var field =
                GetField(export.IncomeHistory.Item.PersonIncomeHistory, "freq");
                

              field.Error = true;

              ExitState = "SI0000_INVALID_INCOME_FREQUENCY";

              return;
            }
          }
          else
          {
            var field =
              GetField(export.IncomeHistory.Item.PersonIncomeHistory, "freq");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }

          // ---------------------------
          // BAQ Allotoment only valid
          // for Military Income Source.
          // ---------------------------
          if (AsChar(export.IncomeSource.Type1) != 'M' && export
            .IncomeHistory.Item.PersonIncomeHistory.MilitaryBaqAllotment.
              GetValueOrDefault() > 0)
          {
            var field =
              GetField(export.IncomeHistory.Item.PersonIncomeHistory,
              "militaryBaqAllotment");

            field.Error = true;

            ExitState = "SI0000_BAQ_NOT_ALLOWED";

            return;
          }

          if (Equal(export.IncomeHistory.Item.PersonIncomeHistory.VerifiedDt,
            local.BlankPersonIncomeHistory.VerifiedDt))
          {
            var field =
              GetField(export.IncomeHistory.Item.PersonIncomeHistory,
              "verifiedDt");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }
          else if (Lt(export.IncomeHistory.Item.PersonIncomeHistory.VerifiedDt,
            export.IncomeHistory.Item.PersonIncomeHistory.IncomeEffDt) || Lt
            (Now().Date,
            export.IncomeHistory.Item.PersonIncomeHistory.VerifiedDt))
          {
            var field =
              GetField(export.IncomeHistory.Item.PersonIncomeHistory,
              "verifiedDt");

            field.Error = true;

            ExitState = "OE0000_INVALID_VERIFIED_DATE";

            return;
          }
        }
      }

      export.IncomeHistory.CheckIndex();
    }

    switch(TrimEnd(global.Command))
    {
      case "HELP":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
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

        break;
      case "LIST":
        switch(AsChar(export.PersonPrompt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.InvalidSel.Count;
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            break;
          default:
            ++local.InvalidSel.Count;

            var field = GetField(export.PersonPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        for(export.IncomeHistory.Index = 0; export.IncomeHistory.Index < export
          .IncomeHistory.Count; ++export.IncomeHistory.Index)
        {
          if (!export.IncomeHistory.CheckSize())
          {
            break;
          }

          switch(AsChar(export.IncomeHistory.Item.FreqPrompt.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              export.Prompt.CodeName = "SI FREQUENCY";
              ++local.InvalidSel.Count;
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              break;
            default:
              ++local.InvalidSel.Count;

              var field = GetField(export.PersonPrompt, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              break;
          }
        }

        export.IncomeHistory.CheckIndex();

        switch(local.InvalidSel.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            if (AsChar(export.PersonPrompt.SelectChar) == 'S')
            {
              var field = GetField(export.PersonPrompt, "selectChar");

              field.Error = true;
            }

            break;
        }

        break;
      case "ADD":
        // ---------------------------------------------
        // Start of code : Raju Dec 25, 1996 1225hrs CST
        // 
        // ---------------------------------------------
        for(export.HiddenIncomeHist.Index = 0; export.HiddenIncomeHist.Index < export
          .HiddenIncomeHist.Count; ++export.HiddenIncomeHist.Index)
        {
          if (!export.HiddenIncomeHist.CheckSize())
          {
            break;
          }

          export.HiddenIncomeHist.Update.LastReadTotal.TotalCurrency = 0;
        }

        export.HiddenIncomeHist.CheckIndex();

        // ---------------------------------------------
        // End   of code
        // ---------------------------------------------
        export.IncomeHistory.Index = 0;

        for(var limit = export.IncomeHistory.Count; export
          .IncomeHistory.Index < limit; ++export.IncomeHistory.Index)
        {
          if (!export.IncomeHistory.CheckSize())
          {
            break;
          }

          if (AsChar(export.IncomeHistory.Item.Common.SelectChar) == 'S')
          {
            UseSiCreatePersonIncomeHistory();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field =
                GetField(export.IncomeHistory.Item.Common, "selectChar");

              field.Error = true;

              return;
            }

            export.IncomeHistory.Update.Common.SelectChar = "*";
          }
        }

        export.IncomeHistory.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        export.IncomeHistory.Index = 0;

        for(var limit = export.IncomeHistory.Count; export
          .IncomeHistory.Index < limit; ++export.IncomeHistory.Index)
        {
          if (!export.IncomeHistory.CheckSize())
          {
            break;
          }

          if (AsChar(export.IncomeHistory.Item.Common.SelectChar) == 'S')
          {
            UseSiUpdatePersonIncomeHistory();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field =
                GetField(export.IncomeHistory.Item.Common, "selectChar");

              field.Error = true;

              return;
            }

            export.IncomeHistory.Update.Common.SelectChar = "*";
          }
        }

        export.IncomeHistory.CheckIndex();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "IHPV":
        if (export.IncHist.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        break;
      case "IHNX":
        if (export.IncHist.PageNumber == Export.PageKeysIncHistGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        export.PageKeysIncHist.Index = export.IncHist.PageNumber;
        export.PageKeysIncHist.CheckSize();

        if (Equal(export.PageKeysIncHist.Item.PageKey.IncomeEffDt, null))
        {
          // ---------------------------------------------
          // 05/25/99 W.Campbell - Replaced zd exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.IncHist.PageNumber;

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "INCS":
        ExitState = "ECO_XFR_TO_INCOME_SOURCE_DETAIL";

        return;
      case "INCL":
        ExitState = "ECO_XFR_TO_INCOME_SOURCE_LIST";

        return;
      case "MCPV":
        if (export.Cases1.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        break;
      case "MCNX":
        if (export.Cases1.PageNumber == Export.PageKeysCasesGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        export.PageKeysCases.Index = export.Cases1.PageNumber;
        export.PageKeysCases.CheckSize();

        if (IsEmpty(export.PageKeysCases.Item.PageKey.Number))
        {
          // ---------------------------------------------
          // 05/25/99 W.Campbell - Replaced zd exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.Cases1.PageNumber;

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      default:
        break;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "MCPV") || Equal
      (global.Command, "MCNX"))
    {
      // --------------------------------------
      // Retrieve Person Details and associated
      // Cases.
      // --------------------------------------
      export.Cases.Count = 0;

      export.PageKeysCases.Index = export.Cases1.PageNumber - 1;
      export.PageKeysCases.CheckSize();

      UseSiReadCasesByPerson();

      if (!IsEmpty(local.AbendData.Type1))
      {
        return;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        return;
      }

      if (Equal(global.Command, "DISPLAY"))
      {
        if (Equal(export.IncomeSource.Identifier,
          local.BlankIncomeSource.Identifier))
        {
          UseSiIncsCountIncomeSourceRecs();

          switch(TrimEnd(local.IncSourceRecordResult.Command))
          {
            case "NO RECORDS":
              ExitState = "SI0000_NO_INCOME_SOURCES_EXIST";

              return;
            case "ONE RECORD":
              break;
            case "MORE THAN ONE RECORD":
              ExitState = "ECO_XFR_TO_INCOME_SOURCE_LIST";

              return;
            default:
              break;
          }
        }

        UseSiReadIncomeSourceDetails();
      }

      export.HiddenCsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
      export.ToMenu.Number = export.CsePersonsWorkSet.Number;
    }

    // ---------------------------------------------
    // Start of code : Raju Dec 25, 1996 1225hrs CST
    // 
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") && IsExitState("ACO_NN0000_ALL_OK"))
    {
      for(export.HiddenIncomeHist.Index = 0; export.HiddenIncomeHist.Index < export
        .HiddenIncomeHist.Count; ++export.HiddenIncomeHist.Index)
      {
        if (!export.HiddenIncomeHist.CheckSize())
        {
          break;
        }

        export.HiddenIncomeHist.Update.LastReadTotal.TotalCurrency = 0;
      }

      export.HiddenIncomeHist.CheckIndex();
    }

    // ---------------------------------------------
    // End   of code
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "IHPV") || Equal
      (global.Command, "IHNX"))
    {
      export.IncomeHistory.Count = 0;
      export.HiddenIncomeHist.Count = 0;

      export.PageKeysIncHist.Index = export.IncHist.PageNumber - 1;
      export.PageKeysIncHist.CheckSize();

      UseSiReadIncomeSourceHistory();

      if (export.IncomeHistory.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }
      else
      {
        export.PageKeysIncHist.Index = export.IncHist.PageNumber;
        export.PageKeysIncHist.CheckSize();

        MovePersonIncomeHistory1(local.NextPage,
          export.PageKeysIncHist.Update.PageKey);

        for(export.IncomeHistory.Index = 0; export.IncomeHistory.Index < export
          .IncomeHistory.Count; ++export.IncomeHistory.Index)
        {
          if (!export.IncomeHistory.CheckSize())
          {
            break;
          }

          export.HiddenIncomeHist.Index = export.IncomeHistory.Index;
          export.HiddenIncomeHist.CheckSize();

          export.HiddenIncomeHist.Update.Hidden.VerifiedDt =
            export.IncomeHistory.Item.PersonIncomeHistory.VerifiedDt;

          // ---------------------------------------------
          // Start of code : Raju Dec 25, 1996 1240hrs CST
          // 
          // ---------------------------------------------
          UseCabComputeAvgMonthlyIncome();

          // ---------------------------------------------
          // End   of code
          // ---------------------------------------------
        }

        export.IncomeHistory.CheckIndex();
      }
    }

    // ---------------------------------------------
    // Code added by Raju  Dec 24, 1996:0545 hrs CST
    // The oe cab raise event will be called from
    // here case of update.
    // Modification (Sid) : The event has to be raised
    // for an 10% increase in total income.
    // ---------------------------------------------
    // ---------------------------------------------
    // Start of code
    // ---------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      local.Infrastructure.UserId = "INCH";
      local.Infrastructure.BusinessObjectCd = "ICS";
      local.Infrastructure.EventId = 46;
      local.Infrastructure.ReasonCode = "AP$CHNG";
      local.Ap.Number = export.CsePersonsWorkSet.Number;
      local.Infrastructure.DenormTimestamp = export.IncomeSource.Identifier;
      local.Infrastructure.SituationNumber = 0;
      export.IncomeHistory.Index = 0;

      for(var limit = export.IncomeHistory.Count; export.IncomeHistory.Index < limit
        ; ++export.IncomeHistory.Index)
      {
        if (!export.IncomeHistory.CheckSize())
        {
          break;
        }

        export.HiddenIncomeHist.Index = export.IncomeHistory.Index;
        export.HiddenIncomeHist.CheckSize();

        if (AsChar(export.IncomeHistory.Item.Common.SelectChar) == '*')
        {
          local.Infrastructure.ReferenceDate =
            export.IncomeHistory.Item.PersonIncomeHistory.IncomeEffDt;
          UseSiCabDetermineIncomeIncrease();

          if ((local.NewMonthlyTotal.TotalCurrency - export
            .HiddenIncomeHist.Item.LastReadTotal.TotalCurrency) * 11 >= local
            .TotalMonthlySum.TotalCurrency)
          {
            // ---------------------------------------------
            // Case : ( new increase / total ) > 10 %
            //     of old total
            // where total = income amount + BAQ
            // The event is to be raised.
            // ---------------------------------------------
            // ---------------------------------------------
            // Begin forming infrastructure detail line
            // ---------------------------------------------
            local.DetailText30.Text30 = "AP's New Total Income :";
            local.Text16.Text16 = UseSiCabAmount2Text();
            local.Infrastructure.Detail = TrimEnd(local.DetailText30.Text30) + TrimEnd
              (local.Text16.Text16);
            local.DetailText30.Text30 = " , Effective :";
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText30.Text30;
            local.DateWorkArea.Date =
              export.IncomeHistory.Item.PersonIncomeHistory.IncomeEffDt;
            local.DetailText10.Text10 = UseCabConvertDate2String();
            local.Infrastructure.Detail =
              TrimEnd(local.Infrastructure.Detail) + local.DetailText10.Text10;

            // ---------------------------------------------
            // End   forming infrastructure detail line
            // ---------------------------------------------
            UseOeCabRaiseEvent();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              UseEabRollbackCics();

              return;
            }
          }
        }

        export.HiddenIncomeHist.Update.LastReadTotal.TotalCurrency =
          local.NewMonthlyTotal.TotalCurrency;
      }

      export.IncomeHistory.CheckIndex();
    }

    // ---------------------------------------------
    // End   of code
    // ---------------------------------------------
  }

  private static void MoveExport1ToCases(SiReadCasesByPerson.Export.
    ExportGroup source, Export.CasesGroup target)
  {
    target.Case1.Number = source.Detail.Number;
  }

  private static void MoveIncomeHistory(SiReadIncomeSourceHistory.Export.
    IncomeHistoryGroup source, Export.IncomeHistoryGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.PersonIncomeHistory.Assign(source.PersonIncomeHistory);
    target.FreqPrompt.SelectChar = source.FreqPrompt.SelectChar;
  }

  private static void MoveIncomeSource1(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveIncomeSource2(IncomeSource source, IncomeSource target)
    
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.Name = source.Name;
    target.StartDt = source.StartDt;
    target.EndDt = source.EndDt;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormTimestamp = source.DenormTimestamp;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MovePersonIncomeHistory1(PersonIncomeHistory source,
    PersonIncomeHistory target)
  {
    target.Identifier = source.Identifier;
    target.IncomeEffDt = source.IncomeEffDt;
  }

  private static void MovePersonIncomeHistory2(PersonIncomeHistory source,
    PersonIncomeHistory target)
  {
    target.Identifier = source.Identifier;
    target.IncomeEffDt = source.IncomeEffDt;
    target.IncomeAmt = source.IncomeAmt;
    target.Freq = source.Freq;
    target.WorkerId = source.WorkerId;
    target.VerifiedDt = source.VerifiedDt;
    target.MilitaryBaqAllotment = source.MilitaryBaqAllotment;
  }

  private static void MoveStandard1(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.ScrollingMessage = source.ScrollingMessage;
  }

  private static void MoveStandard2(Standard source, Standard target)
  {
    target.ScrollingMessage = source.ScrollingMessage;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabComputeAvgMonthlyIncome()
  {
    var useImport = new CabComputeAvgMonthlyIncome.Import();
    var useExport = new CabComputeAvgMonthlyIncome.Export();

    useImport.New1.Assign(export.IncomeHistory.Item.PersonIncomeHistory);

    Call(CabComputeAvgMonthlyIncome.Execute, useImport, useExport);

    export.HiddenIncomeHist.Update.LastReadTotal.TotalCurrency =
      useExport.Common.TotalCurrency;
  }

  private string UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    return useExport.TextWorkArea.Text8;
  }

  private void UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate3()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidValue.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
    export.CsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = local.Ap.Number;

    Call(OeCabRaiseEvent.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
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

    useImport.Case1.Number = export.Next.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private string UseSiCabAmount2Text()
  {
    var useImport = new SiCabAmount2Text.Import();
    var useExport = new SiCabAmount2Text.Export();

    useImport.Currency.TotalCurrency = local.TotalMonthlySum.TotalCurrency;

    Call(SiCabAmount2Text.Execute, useImport, useExport);

    return useExport.Text.Text16;
  }

  private void UseSiCabDetermineIncomeIncrease()
  {
    var useImport = new SiCabDetermineIncomeIncrease.Import();
    var useExport = new SiCabDetermineIncomeIncrease.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.PersonIncomeHistory.Assign(
      export.IncomeHistory.Item.PersonIncomeHistory);
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;

    Call(SiCabDetermineIncomeIncrease.Execute, useImport, useExport);

    local.NewMonthlyTotal.TotalCurrency = useExport.New1.TotalCurrency;
    local.TotalMonthlySum.TotalCurrency = useExport.Total.TotalCurrency;
  }

  private void UseSiCreatePersonIncomeHistory()
  {
    var useImport = new SiCreatePersonIncomeHistory.Import();
    var useExport = new SiCreatePersonIncomeHistory.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    MovePersonIncomeHistory2(export.IncomeHistory.Item.PersonIncomeHistory,
      useImport.PersonIncomeHistory);

    Call(SiCreatePersonIncomeHistory.Execute, useImport, useExport);

    export.IncomeHistory.Update.PersonIncomeHistory.Assign(
      useImport.PersonIncomeHistory);
  }

  private void UseSiIncsCountIncomeSourceRecs()
  {
    var useImport = new SiIncsCountIncomeSourceRecs.Import();
    var useExport = new SiIncsCountIncomeSourceRecs.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiIncsCountIncomeSourceRecs.Execute, useImport, useExport);

    local.IncSourceRecordResult.Command = useExport.Result.Command;
    MoveIncomeSource1(useExport.IncomeSource, export.IncomeSource);
  }

  private void UseSiReadCasesByPerson()
  {
    var useImport = new SiReadCasesByPerson.Import();
    var useExport = new SiReadCasesByPerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Standard.PageNumber = export.Cases1.PageNumber;
    useImport.Page.Number = export.PageKeysCases.Item.PageKey.Number;

    Call(SiReadCasesByPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.Page.Number = useExport.Page.Number;
    useExport.Export1.CopyTo(export.Cases, MoveExport1ToCases);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadIncomeSourceDetails()
  {
    var useImport = new SiReadIncomeSourceDetails.Import();
    var useExport = new SiReadIncomeSourceDetails.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;

    Call(SiReadIncomeSourceDetails.Execute, useImport, useExport);

    MoveIncomeSource2(useExport.IncomeSource, export.IncomeSource);
  }

  private void UseSiReadIncomeSourceHistory()
  {
    var useImport = new SiReadIncomeSourceHistory.Import();
    var useExport = new SiReadIncomeSourceHistory.Export();

    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    useImport.Standard.PageNumber = export.IncHist.PageNumber;
    MovePersonIncomeHistory1(export.PageKeysIncHist.Item.PageKey,
      useImport.PageKey);

    Call(SiReadIncomeSourceHistory.Execute, useImport, useExport);

    MovePersonIncomeHistory1(useExport.NextPageKey, local.NextPage);
    useExport.IncomeHistory.CopyTo(export.IncomeHistory, MoveIncomeHistory);
    export.IncHist.ScrollingMessage = useExport.Standard.ScrollingMessage;
  }

  private void UseSiUpdatePersonIncomeHistory()
  {
    var useImport = new SiUpdatePersonIncomeHistory.Import();
    var useExport = new SiUpdatePersonIncomeHistory.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.IncomeSource.Identifier = export.IncomeSource.Identifier;
    MovePersonIncomeHistory2(export.IncomeHistory.Item.PersonIncomeHistory,
      useImport.PersonIncomeHistory);

    Call(SiUpdatePersonIncomeHistory.Execute, useImport, useExport);

    export.IncomeHistory.Update.PersonIncomeHistory.Assign(
      useImport.PersonIncomeHistory);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A CasesGroup group.</summary>
    [Serializable]
    public class CasesGroup
    {
      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Case1 case1;
    }

    /// <summary>A IncomeHistoryGroup group.</summary>
    [Serializable]
    public class IncomeHistoryGroup
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
      /// A value of PersonIncomeHistory.
      /// </summary>
      [JsonPropertyName("personIncomeHistory")]
      public PersonIncomeHistory PersonIncomeHistory
      {
        get => personIncomeHistory ??= new();
        set => personIncomeHistory = value;
      }

      /// <summary>
      /// A value of FreqPrompt.
      /// </summary>
      [JsonPropertyName("freqPrompt")]
      public Common FreqPrompt
      {
        get => freqPrompt ??= new();
        set => freqPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private PersonIncomeHistory personIncomeHistory;
      private Common freqPrompt;
    }

    /// <summary>A HiddenIncomeHistGroup group.</summary>
    [Serializable]
    public class HiddenIncomeHistGroup
    {
      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public PersonIncomeHistory Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of LastReadTotal.
      /// </summary>
      [JsonPropertyName("lastReadTotal")]
      public Common LastReadTotal
      {
        get => lastReadTotal ??= new();
        set => lastReadTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private PersonIncomeHistory hidden;
      private Common lastReadTotal;
    }

    /// <summary>A PageKeysCasesGroup group.</summary>
    [Serializable]
    public class PageKeysCasesGroup
    {
      /// <summary>
      /// A value of PageKey.
      /// </summary>
      [JsonPropertyName("pageKey")]
      public Case1 PageKey
      {
        get => pageKey ??= new();
        set => pageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 pageKey;
    }

    /// <summary>A PageKeysIncHistGroup group.</summary>
    [Serializable]
    public class PageKeysIncHistGroup
    {
      /// <summary>
      /// A value of PageKey.
      /// </summary>
      [JsonPropertyName("pageKey")]
      public PersonIncomeHistory PageKey
      {
        get => pageKey ??= new();
        set => pageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private PersonIncomeHistory pageKey;
    }

    /// <summary>
    /// A value of FromMenu.
    /// </summary>
    [JsonPropertyName("fromMenu")]
    public CsePerson FromMenu
    {
      get => fromMenu ??= new();
      set => fromMenu = value;
    }

    /// <summary>
    /// Gets a value of Cases.
    /// </summary>
    [JsonIgnore]
    public Array<CasesGroup> Cases => cases ??= new(CasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Cases for json serialization.
    /// </summary>
    [JsonPropertyName("cases")]
    [Computed]
    public IList<CasesGroup> Cases_Json
    {
      get => cases;
      set => Cases.Assign(value);
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of PersonPrompt.
    /// </summary>
    [JsonPropertyName("personPrompt")]
    public Common PersonPrompt
    {
      get => personPrompt ??= new();
      set => personPrompt = value;
    }

    /// <summary>
    /// Gets a value of IncomeHistory.
    /// </summary>
    [JsonIgnore]
    public Array<IncomeHistoryGroup> IncomeHistory => incomeHistory ??= new(
      IncomeHistoryGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IncomeHistory for json serialization.
    /// </summary>
    [JsonPropertyName("incomeHistory")]
    [Computed]
    public IList<IncomeHistoryGroup> IncomeHistory_Json
    {
      get => incomeHistory;
      set => IncomeHistory.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenIncomeHist.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenIncomeHistGroup> HiddenIncomeHist =>
      hiddenIncomeHist ??= new(HiddenIncomeHistGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenIncomeHist for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenIncomeHist")]
    [Computed]
    public IList<HiddenIncomeHistGroup> HiddenIncomeHist_Json
    {
      get => hiddenIncomeHist;
      set => HiddenIncomeHist.Assign(value);
    }

    /// <summary>
    /// A value of IncHist.
    /// </summary>
    [JsonPropertyName("incHist")]
    public Standard IncHist
    {
      get => incHist ??= new();
      set => incHist = value;
    }

    /// <summary>
    /// Gets a value of PageKeysCases.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysCasesGroup> PageKeysCases => pageKeysCases ??= new(
      PageKeysCasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysCases for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysCases")]
    [Computed]
    public IList<PageKeysCasesGroup> PageKeysCases_Json
    {
      get => pageKeysCases;
      set => PageKeysCases.Assign(value);
    }

    /// <summary>
    /// Gets a value of PageKeysIncHist.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysIncHistGroup> PageKeysIncHist =>
      pageKeysIncHist ??= new(PageKeysIncHistGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysIncHist for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysIncHist")]
    [Computed]
    public IList<PageKeysIncHistGroup> PageKeysIncHist_Json
    {
      get => pageKeysIncHist;
      set => PageKeysIncHist.Assign(value);
    }

    /// <summary>
    /// A value of Cases1.
    /// </summary>
    [JsonPropertyName("cases1")]
    public Standard Cases1
    {
      get => cases1 ??= new();
      set => cases1 = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private CsePerson fromMenu;
    private Array<CasesGroup> cases;
    private Standard standard;
    private Case1 next;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private IncomeSource incomeSource;
    private Common personPrompt;
    private Array<IncomeHistoryGroup> incomeHistory;
    private Array<HiddenIncomeHistGroup> hiddenIncomeHist;
    private Standard incHist;
    private Array<PageKeysCasesGroup> pageKeysCases;
    private Array<PageKeysIncHistGroup> pageKeysIncHist;
    private Standard cases1;
    private CodeValue selected;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CasesGroup group.</summary>
    [Serializable]
    public class CasesGroup
    {
      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Case1 case1;
    }

    /// <summary>A IncomeHistoryGroup group.</summary>
    [Serializable]
    public class IncomeHistoryGroup
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
      /// A value of PersonIncomeHistory.
      /// </summary>
      [JsonPropertyName("personIncomeHistory")]
      public PersonIncomeHistory PersonIncomeHistory
      {
        get => personIncomeHistory ??= new();
        set => personIncomeHistory = value;
      }

      /// <summary>
      /// A value of FreqPrompt.
      /// </summary>
      [JsonPropertyName("freqPrompt")]
      public Common FreqPrompt
      {
        get => freqPrompt ??= new();
        set => freqPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private PersonIncomeHistory personIncomeHistory;
      private Common freqPrompt;
    }

    /// <summary>A HiddenIncomeHistGroup group.</summary>
    [Serializable]
    public class HiddenIncomeHistGroup
    {
      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public PersonIncomeHistory Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of LastReadTotal.
      /// </summary>
      [JsonPropertyName("lastReadTotal")]
      public Common LastReadTotal
      {
        get => lastReadTotal ??= new();
        set => lastReadTotal = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private PersonIncomeHistory hidden;
      private Common lastReadTotal;
    }

    /// <summary>A PageKeysCasesGroup group.</summary>
    [Serializable]
    public class PageKeysCasesGroup
    {
      /// <summary>
      /// A value of PageKey.
      /// </summary>
      [JsonPropertyName("pageKey")]
      public Case1 PageKey
      {
        get => pageKey ??= new();
        set => pageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 pageKey;
    }

    /// <summary>A PageKeysIncHistGroup group.</summary>
    [Serializable]
    public class PageKeysIncHistGroup
    {
      /// <summary>
      /// A value of PageKey.
      /// </summary>
      [JsonPropertyName("pageKey")]
      public PersonIncomeHistory PageKey
      {
        get => pageKey ??= new();
        set => pageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private PersonIncomeHistory pageKey;
    }

    /// <summary>
    /// A value of ToMenu.
    /// </summary>
    [JsonPropertyName("toMenu")]
    public CsePerson ToMenu
    {
      get => toMenu ??= new();
      set => toMenu = value;
    }

    /// <summary>
    /// Gets a value of Cases.
    /// </summary>
    [JsonIgnore]
    public Array<CasesGroup> Cases => cases ??= new(CasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Cases for json serialization.
    /// </summary>
    [JsonPropertyName("cases")]
    [Computed]
    public IList<CasesGroup> Cases_Json
    {
      get => cases;
      set => Cases.Assign(value);
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of PersonPrompt.
    /// </summary>
    [JsonPropertyName("personPrompt")]
    public Common PersonPrompt
    {
      get => personPrompt ??= new();
      set => personPrompt = value;
    }

    /// <summary>
    /// Gets a value of IncomeHistory.
    /// </summary>
    [JsonIgnore]
    public Array<IncomeHistoryGroup> IncomeHistory => incomeHistory ??= new(
      IncomeHistoryGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IncomeHistory for json serialization.
    /// </summary>
    [JsonPropertyName("incomeHistory")]
    [Computed]
    public IList<IncomeHistoryGroup> IncomeHistory_Json
    {
      get => incomeHistory;
      set => IncomeHistory.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenIncomeHist.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenIncomeHistGroup> HiddenIncomeHist =>
      hiddenIncomeHist ??= new(HiddenIncomeHistGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenIncomeHist for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenIncomeHist")]
    [Computed]
    public IList<HiddenIncomeHistGroup> HiddenIncomeHist_Json
    {
      get => hiddenIncomeHist;
      set => HiddenIncomeHist.Assign(value);
    }

    /// <summary>
    /// A value of IncHist.
    /// </summary>
    [JsonPropertyName("incHist")]
    public Standard IncHist
    {
      get => incHist ??= new();
      set => incHist = value;
    }

    /// <summary>
    /// Gets a value of PageKeysCases.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysCasesGroup> PageKeysCases => pageKeysCases ??= new(
      PageKeysCasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysCases for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysCases")]
    [Computed]
    public IList<PageKeysCasesGroup> PageKeysCases_Json
    {
      get => pageKeysCases;
      set => PageKeysCases.Assign(value);
    }

    /// <summary>
    /// Gets a value of PageKeysIncHist.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysIncHistGroup> PageKeysIncHist =>
      pageKeysIncHist ??= new(PageKeysIncHistGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysIncHist for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysIncHist")]
    [Computed]
    public IList<PageKeysIncHistGroup> PageKeysIncHist_Json
    {
      get => pageKeysIncHist;
      set => PageKeysIncHist.Assign(value);
    }

    /// <summary>
    /// A value of Cases1.
    /// </summary>
    [JsonPropertyName("cases1")]
    public Standard Cases1
    {
      get => cases1 ??= new();
      set => cases1 = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Code Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    private CsePerson toMenu;
    private Array<CasesGroup> cases;
    private Standard standard;
    private Case1 next;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private IncomeSource incomeSource;
    private Common personPrompt;
    private Array<IncomeHistoryGroup> incomeHistory;
    private Array<HiddenIncomeHistGroup> hiddenIncomeHist;
    private Standard incHist;
    private Array<PageKeysCasesGroup> pageKeysCases;
    private Array<PageKeysIncHistGroup> pageKeysIncHist;
    private Standard cases1;
    private NextTranInfo hiddenNextTranInfo;
    private Code prompt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of BlankIncomeSource.
    /// </summary>
    [JsonPropertyName("blankIncomeSource")]
    public IncomeSource BlankIncomeSource
    {
      get => blankIncomeSource ??= new();
      set => blankIncomeSource = value;
    }

    /// <summary>
    /// A value of ValidValue.
    /// </summary>
    [JsonPropertyName("validValue")]
    public Common ValidValue
    {
      get => validValue ??= new();
      set => validValue = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of BlankPersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("blankPersonIncomeHistory")]
    public PersonIncomeHistory BlankPersonIncomeHistory
    {
      get => blankPersonIncomeHistory ??= new();
      set => blankPersonIncomeHistory = value;
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
    /// A value of NextPage.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public PersonIncomeHistory NextPage
    {
      get => nextPage ??= new();
      set => nextPage = value;
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
    /// A value of Page.
    /// </summary>
    [JsonPropertyName("page")]
    public Case1 Page
    {
      get => page ??= new();
      set => page = value;
    }

    /// <summary>
    /// A value of InvalidSel.
    /// </summary>
    [JsonPropertyName("invalidSel")]
    public Common InvalidSel
    {
      get => invalidSel ??= new();
      set => invalidSel = value;
    }

    /// <summary>
    /// A value of IncSourceRecordResult.
    /// </summary>
    [JsonPropertyName("incSourceRecordResult")]
    public Common IncSourceRecordResult
    {
      get => incSourceRecordResult ??= new();
      set => incSourceRecordResult = value;
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
    /// A value of DetailText30.
    /// </summary>
    [JsonPropertyName("detailText30")]
    public TextWorkArea DetailText30
    {
      get => detailText30 ??= new();
      set => detailText30 = value;
    }

    /// <summary>
    /// A value of Text16.
    /// </summary>
    [JsonPropertyName("text16")]
    public WorkArea Text16
    {
      get => text16 ??= new();
      set => text16 = value;
    }

    /// <summary>
    /// A value of NewMonthlyTotal.
    /// </summary>
    [JsonPropertyName("newMonthlyTotal")]
    public Common NewMonthlyTotal
    {
      get => newMonthlyTotal ??= new();
      set => newMonthlyTotal = value;
    }

    /// <summary>
    /// A value of TotalMonthlySum.
    /// </summary>
    [JsonPropertyName("totalMonthlySum")]
    public Common TotalMonthlySum
    {
      get => totalMonthlySum ??= new();
      set => totalMonthlySum = value;
    }

    /// <summary>
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private DateWorkArea maxDate;
    private IncomeSource incomeSource;
    private IncomeSource blankIncomeSource;
    private Common validValue;
    private CodeValue codeValue;
    private Code code;
    private PersonIncomeHistory blankPersonIncomeHistory;
    private Common select;
    private PersonIncomeHistory nextPage;
    private AbendData abendData;
    private Case1 page;
    private Common invalidSel;
    private Common incSourceRecordResult;
    private Infrastructure infrastructure;
    private TextWorkArea detailText30;
    private WorkArea text16;
    private Common newMonthlyTotal;
    private Common totalMonthlySum;
    private TextWorkArea detailText10;
    private DateWorkArea dateWorkArea;
    private CsePerson ap;
    private Infrastructure lastTran;
  }
#endregion
}
