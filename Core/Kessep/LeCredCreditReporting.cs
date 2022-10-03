// Program: LE_CRED_CREDIT_REPORTING, ID: 372586237, model: 746.
// Short name: SWECREDP
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
/// A program: LE_CRED_CREDIT_REPORTING.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeCredCreditReporting: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CRED_CREDIT_REPORTING program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCredCreditReporting(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCredCreditReporting.
  /// </summary>
  public LeCredCreditReporting(IContext context, Import import, Export export):
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
    // Date		Developer	Request #	Description
    // 02/19/1996	govind				Initial Coding
    // 04/24/96        Bruce Moore                     Retrofit
    // 10/09/98     P. Sharp     Made changes per 
    // Phase 2 assessment and problem report 25734.
    // Exit states not making fields error after
    // display action block, also not erroring in
    // correct order. Removed call to action block
    // si_format_cse_person_name. This was being
    // performed in the display action block.
    // 
    // 07/14/99        Anand     Modifications to
    // disallow past dates in Date Stayed and Date
    // Stay Released fields upon UPDATE.
    // 11/09/99        R. Jean     PR79604 - Test the 15 day rule for stayed vs 
    // reported date only if stayed date has changed.
    // 04/04/00        C. Scroggins Added view matching for security for family 
    // violence.
    // 12/13/2001	E.Shirk	PR122269	altered message that is displayed when screen
    // group view is full after a display action.
    // 12/05/17	GVandy		CQ56369		Add Good Cause and Exemption Indicators.
    // -------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.End.CraTransDate = import.End.CraTransDate;
    export.Ap.Assign(import.Ap);

    if (!IsEmpty(export.Ap.Number))
    {
      local.TextWorkArea.Text10 = export.Ap.Number;
      UseEabPadLeftWithZeros();
      export.Ap.Number = local.TextWorkArea.Text10;
    }

    export.CreditReportingAction.CseActionCode =
      import.CreditReportingAction.CseActionCode;
    export.ListCseAction.PromptField = import.ListCseAction.PromptField;
    export.CreditReporting.Assign(import.CreditReporting);
    export.NotifiedBy.Assign(import.NotifiedBy);
    export.MoreCseCasesInd.OneChar = import.MoreCseCasesInd.OneChar;
    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (export.SsnWorkArea.SsnNumPart1 > 0 || export.SsnWorkArea.SsnNumPart2 > 0
      || export.SsnWorkArea.SsnNumPart3 > 0)
    {
      export.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.Ap.Ssn = export.SsnWorkArea.SsnText9;
    }

    MoveCase1(import.Import1St, export.Export1St);
    MoveCase1(import.Import2Nd, export.Export2Nd);
    MoveCase1(import.Import3Rd, export.Export3Rd);
    MoveCase1(import.Import4Th, export.Export4Th);
    MoveCase1(import.Import5Th, export.Export5Th);

    var field1 = GetField(export.Export1St, "number");

    field1.Protected = true;

    var field2 = GetField(export.Export2Nd, "number");

    field2.Protected = true;

    var field3 = GetField(export.Export3Rd, "number");

    field3.Protected = true;

    var field4 = GetField(export.Export4Th, "number");

    field4.Protected = true;

    var field5 = GetField(export.Export5Th, "number");

    field5.Protected = true;

    // ****************************************************************
    // * Highlight closed cases
    // ****************************************************************
    if (AsChar(export.Export1St.Status) == 'C')
    {
      var field = GetField(export.Export1St, "number");

      field.Intensity = Intensity.High;
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = true;
    }

    if (AsChar(export.Export2Nd.Status) == 'C')
    {
      var field = GetField(export.Export2Nd, "number");

      field.Intensity = Intensity.High;
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = true;
    }

    if (AsChar(export.Export3Rd.Status) == 'C')
    {
      var field = GetField(export.Export3Rd, "number");

      field.Intensity = Intensity.High;
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = true;
    }

    if (AsChar(export.Export4Th.Status) == 'C')
    {
      var field = GetField(export.Export4Th, "number");

      field.Intensity = Intensity.High;
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = true;
    }

    if (AsChar(export.Export5Th.Status) == 'C')
    {
      var field = GetField(export.Export5Th, "number");

      field.Intensity = Intensity.High;
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = true;
    }

    MoveStandard(import.Standard, export.Standard);
    export.AdminExmpExists.Flag = import.AdminExmpExists.Flag;
    export.GoodCauseExists.Flag = import.GoodCauseExists.Flag;

    if (!import.Cra.IsEmpty)
    {
      export.Cra.Index = 0;
      export.Cra.Clear();

      for(import.Cra.Index = 0; import.Cra.Index < import.Cra.Count; ++
        import.Cra.Index)
      {
        if (export.Cra.IsFull)
        {
          break;
        }

        export.Cra.Update.DetailCra.Assign(import.Cra.Item.DetailCra);
        export.Cra.Next();
      }
    }

    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenPrev.Number = import.HiddenPrev.Number;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    export.H.Assign(import.H);
    local.Current.Date = Now().Date;
    local.Datenum.Date = null;

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    MoveStandard(import.Standard, export.Standard);

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.Ap.Number = local.NextTranInfo.CsePersonNumber ?? Spaces(10);

      if (!IsEmpty(export.Ap.Number))
      {
        local.TextWorkArea.Text10 = export.Ap.Number;
        UseEabPadLeftWithZeros();
        export.Ap.Number = local.TextWorkArea.Text10;
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.Ap.Number;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // Edit checks :
    // =============
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.Ap.Number) && IsEmpty(export.Ap.Ssn))
      {
        var field6 = GetField(export.Ap, "number");

        field6.Error = true;

        var field7 = GetField(export.SsnWorkArea, "ssnNumPart1");

        field7.Error = true;

        var field8 = GetField(export.SsnWorkArea, "ssnNumPart2");

        field8.Error = true;

        var field9 = GetField(export.SsnWorkArea, "ssnNumPart3");

        field9.Error = true;

        ExitState = "LE0000_CSE_NO_OR_SSN_REQD";

        return;
      }
    }

    if (Equal(global.Command, "UPDATE"))
    {
      // ****************************************************************
      // * Process Response date changes
      // ****************************************************************
      if (!Equal(import.CreditReporting.ApRespReceivedData,
        export.H.ApRespReceivedData))
      {
        if (Lt(local.Current.Date, import.CreditReporting.ApRespReceivedData))
        {
          ExitState = "LE0000_AP_RESP_DATE_GREATER_CURR";

          var field = GetField(export.CreditReporting, "apRespReceivedData");

          field.Error = true;

          return;
        }

        if (!IsEmpty(export.CreditReportingAction.CseActionCode))
        {
          ExitState = "LE0000_ACTION_CODE_NOT_ALLOWED";

          var field = GetField(export.CreditReportingAction, "cseActionCode");

          field.Error = true;

          return;
        }

        for(export.Cra.Index = 0; export.Cra.Index < export.Cra.Count; ++
          export.Cra.Index)
        {
          // *****************************************************************
          // * 11/09/99        R. Jean     PR79604 - Test against the first ISS 
          // transaction.
          // *****************************************************************
          if (!Equal(export.Cra.Item.DetailCra.CseActionCode, "ISS"))
          {
            continue;
          }

          if (Lt(import.CreditReporting.ApRespReceivedData,
            export.Cra.Item.DetailCra.CraTransDate))
          {
            ExitState = "LE0000_RESP_DT_LT_ISSUE_DT";

            var field = GetField(export.CreditReporting, "apRespReceivedData");

            field.Error = true;

            return;
          }

          // *****************************************************************
          // * 11/09/99        R. Jean     PR79604 - Test the 15 day rule for
          // * for receiving the response.
          // *****************************************************************
          if (Lt(AddDays(export.Cra.Item.DetailCra.CraTransDate, 14),
            import.CreditReporting.ApRespReceivedData))
          {
            ExitState = "LE0000_RESP_DT_GT_ISS_DT_PLUS_14";

            var field = GetField(export.CreditReporting, "apRespReceivedData");

            field.Error = true;

            return;
          }

          // *****************************************************************
          // * 11/09/99        R. Jean     PR79604 - Get out after testing the 
          // latest and greatest ISS transaction
          // *****************************************************************
          break;
        }
      }

      // *** Process Changed Stay Dates only'     - Anand 7/15/1999
      if (!Equal(import.H.DateStayed, import.CreditReporting.DateStayed))
      {
        // *** Condition Changed from > to 'Not ='     - Anand 7/15/1999
        if (!Equal(import.CreditReporting.DateStayed, local.Current.Date))
        {
          ExitState = "LE0000_STAY_DATE_NE_CURRENT_DATE";

          var field = GetField(export.CreditReporting, "dateStayed");

          field.Error = true;

          return;
        }

        if (!IsEmpty(export.CreditReportingAction.CseActionCode))
        {
          ExitState = "LE0000_ACTION_CODE_NOT_ALLOWED";

          var field = GetField(export.CreditReportingAction, "cseActionCode");

          field.Error = true;

          return;
        }

        // *****************************************************************
        // * 11/09/99        R. Jean     PR79604 - Force the stay released date 
        // to be blanked out if stay date changes.
        // *****************************************************************
        if (!Equal(import.CreditReporting.DateStayReleased, local.Datenum.Date))
        {
          ExitState = "LE0000_STAY_REL_MUST_BE_BLANK";

          var field = GetField(export.CreditReporting, "dateStayReleased");

          field.Error = true;

          return;
        }

        for(export.Cra.Index = 0; export.Cra.Index < export.Cra.Count; ++
          export.Cra.Index)
        {
          // *****************************************************************
          // * 11/09/99        R. Jean     PR79604 - Test against the first ISS 
          // transaction.
          // *****************************************************************
          if (!Equal(export.Cra.Item.DetailCra.CseActionCode, "ISS"))
          {
            continue;
          }

          if (Lt(import.CreditReporting.DateStayed,
            export.Cra.Item.DetailCra.CraTransDate))
          {
            ExitState = "LE0000_STAY_DT_LT_ISSUE_DT";

            var field = GetField(export.CreditReporting, "dateStayed");

            field.Error = true;

            return;
          }

          // *****************************************************************
          // * 11/09/99        R. Jean     PR79604 - Test the 15 day rule for
          // * stayed vs reported date only if stayed date has changed.
          // *****************************************************************
          if (Lt(AddDays(export.Cra.Item.DetailCra.CraTransDate, 14),
            import.CreditReporting.DateStayed))
          {
            ExitState = "LE0000_STAY_DT_GT_ISS_DT_PLUS_14";

            var field = GetField(export.CreditReporting, "dateStayed");

            field.Error = true;

            return;
          }

          // *****************************************************************
          // * 11/09/99        R. Jean     PR79604 - Get out after testing the 
          // latest and greatest ISS transaction
          // *****************************************************************
          break;
        }
      }

      // *** Process Changed Date Stay Released only'     - Anand 7/15/1999
      if (!Equal(import.H.DateStayReleased,
        import.CreditReporting.DateStayReleased))
      {
        // *** Condition Changed from > to 'Not ='     - Anand 7/15/1999
        if (!Equal(import.CreditReporting.DateStayReleased, local.Current.Date) &&
          !Equal(import.CreditReporting.DateStayReleased, local.Datenum.Date))
        {
          ExitState = "LE0000_STAY_REL_NEQ_CUR_OR_BLANK";

          var field = GetField(export.CreditReporting, "dateStayReleased");

          field.Error = true;

          return;
        }

        if (!IsEmpty(export.CreditReportingAction.CseActionCode))
        {
          ExitState = "LE0000_ACTION_CODE_NOT_ALLOWED";

          var field = GetField(export.CreditReportingAction, "cseActionCode");

          field.Error = true;

          return;
        }

        if (!Equal(import.CreditReporting.DateStayReleased, local.Datenum.Date))
        {
          if (Equal(import.CreditReporting.DateStayed, local.Datenum.Date))
          {
            ExitState = "LE0000_STAYED_DATE_REQUIRED";

            var field = GetField(export.CreditReporting, "dateStayed");

            field.Error = true;

            return;
          }

          if (Lt(import.CreditReporting.DateStayReleased,
            import.CreditReporting.DateStayed))
          {
            ExitState = "LE0000_STAYED_DT_GRTR_REL_DT";

            var field = GetField(export.CreditReporting, "dateStayed");

            field.Error = true;

            return;
          }
        }
      }

      // *** Condition Changed from > to 'Not ='     - Anand 7/15/1999
      if (!Equal(import.CreditReporting.DateStayed, local.Current.Date))
      {
        if (Equal(import.H.DateStayReleased,
          import.CreditReporting.DateStayReleased))
        {
          goto Test;
        }

        for(export.Cra.Index = 0; export.Cra.Index < export.Cra.Count; ++
          export.Cra.Index)
        {
          // *****************************************************************
          // * 11/09/99        R. Jean     PR79604 - Test against the first ISS 
          // transaction.
          // *****************************************************************
          if (!Equal(export.Cra.Item.DetailCra.CseActionCode, "ISS"))
          {
            continue;
          }

          if (Lt(import.CreditReporting.DateStayed,
            export.Cra.Item.DetailCra.CraTransDate))
          {
            ExitState = "LE0000_STAY_DT_LT_ISSUE_DT";

            var field = GetField(export.CreditReporting, "dateStayed");

            field.Error = true;

            return;
          }

          // *****************************************************************
          // * 11/09/99        R. Jean     PR79604 - Test the 15 day rule for
          // * stayed vs reported date only if stayed date has changed.
          // *****************************************************************
          if (Lt(AddDays(export.Cra.Item.DetailCra.CraTransDate, 14),
            import.CreditReporting.DateStayed) && !
            Equal(import.CreditReporting.DateStayed, import.H.DateStayed))
          {
            ExitState = "LE0000_STAY_DT_GT_ISS_DT_PLUS_14";

            var field = GetField(export.CreditReporting, "dateStayed");

            field.Error = true;

            return;
          }

          goto Test;
        }
      }

Test:

      if (!IsEmpty(export.CreditReportingAction.CseActionCode))
      {
        local.Code.CodeName = "CRED CSE ACTION";
        local.CodeValue.Cdvalue =
          export.CreditReportingAction.CseActionCode ?? Spaces(10);
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count > 0)
        {
          var field = GetField(export.CreditReportingAction, "cseActionCode");

          field.Error = true;

          ExitState = "LE0000_INVALID_CRED_REP_ACTION";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // PF KEY PROCESSING
    // =================
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL"))
    {
      export.ListCseAction.PromptField = "";
    }

    export.HiddenPrevUserAction.Command = global.Command;

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETEXMP":
        break;
      case "RETOBLO":
        break;
      case "RETPART":
        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Clear the display fields before populating new values
        // ---------------------------------------------
        export.HiddenDisplayPerformed.Flag = "";
        export.Ap.FormattedName = "";
        export.Export1St.Number = local.InitialisedToSpaces.Number;
        export.Export2Nd.Number = local.InitialisedToSpaces.Number;
        export.Export3Rd.Number = local.InitialisedToSpaces.Number;
        export.Export4Th.Number = local.InitialisedToSpaces.Number;
        export.Export5Th.Number = local.InitialisedToSpaces.Number;

        var field6 = GetField(export.Export1St, "number");

        field6.Intensity = Intensity.Normal;
        field6.Highlighting = Highlighting.Normal;
        field6.Protected = true;

        var field7 = GetField(export.Export2Nd, "number");

        field7.Intensity = Intensity.Normal;
        field7.Highlighting = Highlighting.Normal;
        field7.Protected = true;

        var field8 = GetField(export.Export3Rd, "number");

        field8.Intensity = Intensity.Normal;
        field8.Highlighting = Highlighting.Normal;
        field8.Protected = true;

        var field9 = GetField(export.Export4Th, "number");

        field9.Intensity = Intensity.Normal;
        field9.Highlighting = Highlighting.Normal;
        field9.Protected = true;

        var field10 = GetField(export.Export5Th, "number");

        field10.Intensity = Intensity.Normal;
        field10.Highlighting = Highlighting.Normal;
        field10.Protected = true;

        UseLeCredDisplayCraActions1();

        if (IsExitState("LE0000_NO_CRED_ADMIN_ACTION_TAKN"))
        {
          if (!IsEmpty(export.Ap.Ssn))
          {
            export.SsnWorkArea.SsnText9 = export.Ap.Ssn;
            UseCabSsnConvertTextToNum();
          }

          return;
        }
        else if (IsExitState("LE0000_CSE_PERSON_NF_FOR_SSN"))
        {
          var field11 = GetField(export.Ap, "number");

          field11.Error = true;

          var field12 = GetField(export.SsnWorkArea, "ssnNumPart1");

          field12.Error = true;

          var field13 = GetField(export.SsnWorkArea, "ssnNumPart2");

          field13.Error = true;

          var field14 = GetField(export.SsnWorkArea, "ssnNumPart3");

          field14.Error = true;

          return;
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.Ap, "number");

          field.Error = true;

          export.SsnWorkArea.SsnNumPart1 = 0;
          export.SsnWorkArea.SsnNumPart2 = 0;
          export.SsnWorkArea.SsnNumPart3 = 0;

          return;
        }
        else if (IsExitState("LE0000_CSE_NO_OR_SSN_REQD"))
        {
          var field11 = GetField(export.Ap, "number");

          field11.Error = true;

          var field12 = GetField(export.SsnWorkArea, "ssnNumPart1");

          field12.Error = true;

          var field13 = GetField(export.SsnWorkArea, "ssnNumPart2");

          field13.Error = true;

          var field14 = GetField(export.SsnWorkArea, "ssnNumPart3");

          field14.Error = true;

          return;
        }
        else
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (export.Cra.IsEmpty)
            {
              ExitState = "LE0000_NO_CRED_RPT_ACTIONS";
            }
            else if (export.Cra.IsFull)
            {
              ExitState = "LE0000_CRED_LIST_IS_FULL";
            }
            else
            {
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
              export.HiddenDisplayPerformed.Flag = "Y";
              export.HiddenPrev.Number = export.Ap.Number;

              // *** Save the Retrieved Stayed Date -  Anand 07/15/1999
              export.H.Assign(export.CreditReporting);
            }

            // ****************************************************************
            // * Highlight closed cases
            // ****************************************************************
            if (AsChar(export.Export1St.Status) == 'C')
            {
              var field = GetField(export.Export1St, "number");

              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }

            if (AsChar(export.Export2Nd.Status) == 'C')
            {
              var field = GetField(export.Export2Nd, "number");

              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }

            if (AsChar(export.Export3Rd.Status) == 'C')
            {
              var field = GetField(export.Export3Rd, "number");

              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }

            if (AsChar(export.Export4Th.Status) == 'C')
            {
              var field = GetField(export.Export4Th, "number");

              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }

            if (AsChar(export.Export5Th.Status) == 'C')
            {
              var field = GetField(export.Export5Th, "number");

              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }
          }
        }

        if (!IsEmpty(export.Ap.Ssn))
        {
          export.SsnWorkArea.SsnText9 = export.Ap.Ssn;
          UseCabSsnConvertTextToNum();
        }

        break;
      case "LIST":
        if (!IsEmpty(export.ListCseAction.PromptField) && AsChar
          (export.ListCseAction.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListCseAction, "promptField");

          field.Error = true;

          return;
        }

        if (AsChar(export.ListCseAction.PromptField) == 'S')
        {
          export.ListCseAction.PromptField = "";
          export.DlgflwDesired.CodeName = "CRED CSE ACTION";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RETCDVL":
        // ---------------------------------------------
        // Returned from Code Value List (CDVL). Move the selected value to 
        // export
        // ---------------------------------------------
        if (!IsEmpty(import.DlgflwSelected.Cdvalue))
        {
          export.CreditReportingAction.CseActionCode =
            import.DlgflwSelected.Cdvalue;
          export.ListCseAction.PromptField = "";

          var field = GetField(export.CreditReporting, "dateStayed");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          var field = GetField(export.CreditReportingAction, "cseActionCode");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "UPDATE":
        if (AsChar(import.HiddenDisplayPerformed.Flag) != 'Y' || !
          Equal(import.HiddenPrev.Number, export.Ap.Number))
        {
          // ---------------------------------------------
          // Either not displayed or person # changed
          // ---------------------------------------------
          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        UseLeCredUpdateCra();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("CREDIT_REPORTING_ACTION_NU"))
          {
            UseEabRollbackCics();
          }

          return;
        }

        // ---------------------------------------------
        // Clear the display fields before populating new values
        // ---------------------------------------------
        export.Ap.FormattedName = "";
        export.Export1St.Number = local.InitialisedToSpaces.Number;
        export.Export2Nd.Number = local.InitialisedToSpaces.Number;
        export.Export3Rd.Number = local.InitialisedToSpaces.Number;
        export.Export4Th.Number = local.InitialisedToSpaces.Number;
        export.Export5Th.Number = local.InitialisedToSpaces.Number;

        export.Cra.Index = 0;
        export.Cra.Clear();

        for(import.Cra.Index = 0; import.Cra.Index < import.Cra.Count; ++
          import.Cra.Index)
        {
          if (export.Cra.IsFull)
          {
            break;
          }

          export.Cra.Next();

          break;

          export.Cra.Next();
        }

        UseLeCredDisplayCraActions2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          export.HiddenDisplayPerformed.Flag = "Y";

          // *** Save the Changed Credit Reporting Dates -  Anand 07/15/1999
          export.H.Assign(export.CreditReporting);
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        if (export.Cra.IsFull)
        {
          ExitState = "LE0000_CRED_LIST_IS_FULL";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXMP":
        export.Dlgflw.Type1 = "CRED";
        export.DlgflwAllObligation.Flag = "Y";
        ExitState = "ECO_LNK_TO_EXMP_ADM_ACT_EXEMPTN";

        break;
      case "OBLO":
        export.Dlgflw.Type1 = "CRED";
        ExitState = "ECO_LNK_OBLO_ADM_ACTS_BY_OBLIGOR";

        break;
      case "PART":
        ExitState = "ECO_LNK_TO_CASE_PARTICIPATION";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCredsToCra(LeCredDisplayCraActions.Export.
    CredsGroup source, Export.CraGroup target)
  {
    target.DetailCra.Assign(source.CredsDetail);
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.MenuOption = source.MenuOption;
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(export.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = export.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeCredDisplayCraActions1()
  {
    var useImport = new LeCredDisplayCraActions.Import();
    var useExport = new LeCredDisplayCraActions.Export();

    useImport.End.CraTransDate = export.End.CraTransDate;
    MoveCsePersonsWorkSet(export.Ap, useImport.Obligor);

    Call(LeCredDisplayCraActions.Execute, useImport, useExport);

    useExport.Creds.CopyTo(export.Cra, MoveCredsToCra);
    MoveCase1(useExport.Export1St, export.Export1St);
    MoveCase1(useExport.Export2Nd, export.Export2Nd);
    MoveCase1(useExport.Export3Rd, export.Export3Rd);
    MoveCase1(useExport.Export4Th, export.Export4Th);
    MoveCase1(useExport.Export5Th, export.Export5Th);
    export.Ap.Assign(useExport.Obligor);
    export.CreditReporting.Assign(useExport.CreditReporting);
    export.MoreCseCasesInd.OneChar = useExport.MoreCseCasesInd.OneChar;
    export.AdminExmpExists.Flag = useExport.AdminExemptExists.Flag;
    export.GoodCauseExists.Flag = useExport.GoodCauseExists.Flag;
  }

  private void UseLeCredDisplayCraActions2()
  {
    var useImport = new LeCredDisplayCraActions.Import();
    var useExport = new LeCredDisplayCraActions.Export();

    MoveCsePersonsWorkSet(export.Ap, useImport.Obligor);

    Call(LeCredDisplayCraActions.Execute, useImport, useExport);

    useExport.Creds.CopyTo(export.Cra, MoveCredsToCra);
    MoveCase1(useExport.Export1St, export.Export1St);
    MoveCase1(useExport.Export2Nd, export.Export2Nd);
    MoveCase1(useExport.Export3Rd, export.Export3Rd);
    MoveCase1(useExport.Export4Th, export.Export4Th);
    MoveCase1(useExport.Export5Th, export.Export5Th);
    export.Ap.Assign(useExport.Obligor);
    export.CreditReporting.Assign(useExport.CreditReporting);
    export.MoreCseCasesInd.OneChar = useExport.MoreCseCasesInd.OneChar;
  }

  private void UseLeCredUpdateCra()
  {
    var useImport = new LeCredUpdateCra.Import();
    var useExport = new LeCredUpdateCra.Export();

    useImport.CreditReporting.Assign(export.CreditReporting);
    useImport.UserAction.CseActionCode =
      export.CreditReportingAction.CseActionCode;
    MoveCsePersonsWorkSet(export.Ap, useImport.Obligor);

    Call(LeCredUpdateCra.Execute, useImport, useExport);

    export.CreditReporting.Assign(useExport.CreditReporting);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
    /// <summary>A CraGroup group.</summary>
    [Serializable]
    public class CraGroup
    {
      /// <summary>
      /// A value of DetailCra.
      /// </summary>
      [JsonPropertyName("detailCra")]
      public CreditReportingAction DetailCra
      {
        get => detailCra ??= new();
        set => detailCra = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private CreditReportingAction detailCra;
    }

    /// <summary>
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public AdministrativeActCertification H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public CreditReportingAction End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Import5Th.
    /// </summary>
    [JsonPropertyName("import5Th")]
    public Case1 Import5Th
    {
      get => import5Th ??= new();
      set => import5Th = value;
    }

    /// <summary>
    /// A value of Import4Th.
    /// </summary>
    [JsonPropertyName("import4Th")]
    public Case1 Import4Th
    {
      get => import4Th ??= new();
      set => import4Th = value;
    }

    /// <summary>
    /// A value of Import3Rd.
    /// </summary>
    [JsonPropertyName("import3Rd")]
    public Case1 Import3Rd
    {
      get => import3Rd ??= new();
      set => import3Rd = value;
    }

    /// <summary>
    /// A value of Import2Nd.
    /// </summary>
    [JsonPropertyName("import2Nd")]
    public Case1 Import2Nd
    {
      get => import2Nd ??= new();
      set => import2Nd = value;
    }

    /// <summary>
    /// A value of Import1St.
    /// </summary>
    [JsonPropertyName("import1St")]
    public Case1 Import1St
    {
      get => import1St ??= new();
      set => import1St = value;
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
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of ListCseAction.
    /// </summary>
    [JsonPropertyName("listCseAction")]
    public Standard ListCseAction
    {
      get => listCseAction ??= new();
      set => listCseAction = value;
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
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
    }

    /// <summary>
    /// A value of CreditReporting.
    /// </summary>
    [JsonPropertyName("creditReporting")]
    public AdministrativeActCertification CreditReporting
    {
      get => creditReporting ??= new();
      set => creditReporting = value;
    }

    /// <summary>
    /// A value of NotifiedBy.
    /// </summary>
    [JsonPropertyName("notifiedBy")]
    public ServiceProvider NotifiedBy
    {
      get => notifiedBy ??= new();
      set => notifiedBy = value;
    }

    /// <summary>
    /// A value of MoreCseCasesInd.
    /// </summary>
    [JsonPropertyName("moreCseCasesInd")]
    public Standard MoreCseCasesInd
    {
      get => moreCseCasesInd ??= new();
      set => moreCseCasesInd = value;
    }

    /// <summary>
    /// Gets a value of Cra.
    /// </summary>
    [JsonIgnore]
    public Array<CraGroup> Cra => cra ??= new(CraGroup.Capacity);

    /// <summary>
    /// Gets a value of Cra for json serialization.
    /// </summary>
    [JsonPropertyName("cra")]
    [Computed]
    public IList<CraGroup> Cra_Json
    {
      get => cra;
      set => Cra.Assign(value);
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
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of AdminExmpExists.
    /// </summary>
    [JsonPropertyName("adminExmpExists")]
    public Common AdminExmpExists
    {
      get => adminExmpExists ??= new();
      set => adminExmpExists = value;
    }

    /// <summary>
    /// A value of GoodCauseExists.
    /// </summary>
    [JsonPropertyName("goodCauseExists")]
    public Common GoodCauseExists
    {
      get => goodCauseExists ??= new();
      set => goodCauseExists = value;
    }

    private AdministrativeActCertification h;
    private CreditReportingAction end;
    private Case1 import5Th;
    private Case1 import4Th;
    private Case1 import3Rd;
    private Case1 import2Nd;
    private Case1 import1St;
    private CodeValue dlgflwSelected;
    private CsePersonsWorkSet hiddenPrev;
    private Common hiddenPrevUserAction;
    private Common hiddenDisplayPerformed;
    private Standard listCseAction;
    private CsePersonsWorkSet ap;
    private CreditReportingAction creditReportingAction;
    private AdministrativeActCertification creditReporting;
    private ServiceProvider notifiedBy;
    private Standard moreCseCasesInd;
    private Array<CraGroup> cra;
    private Standard standard;
    private NextTranInfo hidden;
    private SsnWorkArea ssnWorkArea;
    private Common adminExmpExists;
    private Common goodCauseExists;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CraGroup group.</summary>
    [Serializable]
    public class CraGroup
    {
      /// <summary>
      /// A value of DetailCra.
      /// </summary>
      [JsonPropertyName("detailCra")]
      public CreditReportingAction DetailCra
      {
        get => detailCra ??= new();
        set => detailCra = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private CreditReportingAction detailCra;
    }

    /// <summary>
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public AdministrativeActCertification H
    {
      get => h ??= new();
      set => h = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public CreditReportingAction End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of Dlgflw.
    /// </summary>
    [JsonPropertyName("dlgflw")]
    public AdministrativeAction Dlgflw
    {
      get => dlgflw ??= new();
      set => dlgflw = value;
    }

    /// <summary>
    /// A value of DlgflwAllObligation.
    /// </summary>
    [JsonPropertyName("dlgflwAllObligation")]
    public Common DlgflwAllObligation
    {
      get => dlgflwAllObligation ??= new();
      set => dlgflwAllObligation = value;
    }

    /// <summary>
    /// A value of Export5Th.
    /// </summary>
    [JsonPropertyName("export5Th")]
    public Case1 Export5Th
    {
      get => export5Th ??= new();
      set => export5Th = value;
    }

    /// <summary>
    /// A value of Export4Th.
    /// </summary>
    [JsonPropertyName("export4Th")]
    public Case1 Export4Th
    {
      get => export4Th ??= new();
      set => export4Th = value;
    }

    /// <summary>
    /// A value of Export3Rd.
    /// </summary>
    [JsonPropertyName("export3Rd")]
    public Case1 Export3Rd
    {
      get => export3Rd ??= new();
      set => export3Rd = value;
    }

    /// <summary>
    /// A value of Export2Nd.
    /// </summary>
    [JsonPropertyName("export2Nd")]
    public Case1 Export2Nd
    {
      get => export2Nd ??= new();
      set => export2Nd = value;
    }

    /// <summary>
    /// A value of Export1St.
    /// </summary>
    [JsonPropertyName("export1St")]
    public Case1 Export1St
    {
      get => export1St ??= new();
      set => export1St = value;
    }

    /// <summary>
    /// A value of DlgflwDesired.
    /// </summary>
    [JsonPropertyName("dlgflwDesired")]
    public Code DlgflwDesired
    {
      get => dlgflwDesired ??= new();
      set => dlgflwDesired = value;
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
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of ListCseAction.
    /// </summary>
    [JsonPropertyName("listCseAction")]
    public Standard ListCseAction
    {
      get => listCseAction ??= new();
      set => listCseAction = value;
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
    /// A value of CreditReportingAction.
    /// </summary>
    [JsonPropertyName("creditReportingAction")]
    public CreditReportingAction CreditReportingAction
    {
      get => creditReportingAction ??= new();
      set => creditReportingAction = value;
    }

    /// <summary>
    /// A value of CreditReporting.
    /// </summary>
    [JsonPropertyName("creditReporting")]
    public AdministrativeActCertification CreditReporting
    {
      get => creditReporting ??= new();
      set => creditReporting = value;
    }

    /// <summary>
    /// A value of NotifiedBy.
    /// </summary>
    [JsonPropertyName("notifiedBy")]
    public ServiceProvider NotifiedBy
    {
      get => notifiedBy ??= new();
      set => notifiedBy = value;
    }

    /// <summary>
    /// A value of MoreCseCasesInd.
    /// </summary>
    [JsonPropertyName("moreCseCasesInd")]
    public Standard MoreCseCasesInd
    {
      get => moreCseCasesInd ??= new();
      set => moreCseCasesInd = value;
    }

    /// <summary>
    /// Gets a value of Cra.
    /// </summary>
    [JsonIgnore]
    public Array<CraGroup> Cra => cra ??= new(CraGroup.Capacity);

    /// <summary>
    /// Gets a value of Cra for json serialization.
    /// </summary>
    [JsonPropertyName("cra")]
    [Computed]
    public IList<CraGroup> Cra_Json
    {
      get => cra;
      set => Cra.Assign(value);
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
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of AdminExmpExists.
    /// </summary>
    [JsonPropertyName("adminExmpExists")]
    public Common AdminExmpExists
    {
      get => adminExmpExists ??= new();
      set => adminExmpExists = value;
    }

    /// <summary>
    /// A value of GoodCauseExists.
    /// </summary>
    [JsonPropertyName("goodCauseExists")]
    public Common GoodCauseExists
    {
      get => goodCauseExists ??= new();
      set => goodCauseExists = value;
    }

    private AdministrativeActCertification h;
    private CreditReportingAction end;
    private AdministrativeAction dlgflw;
    private Common dlgflwAllObligation;
    private Case1 export5Th;
    private Case1 export4Th;
    private Case1 export3Rd;
    private Case1 export2Nd;
    private Case1 export1St;
    private Code dlgflwDesired;
    private CsePersonsWorkSet hiddenPrev;
    private Common hiddenPrevUserAction;
    private Common hiddenDisplayPerformed;
    private Standard listCseAction;
    private CsePersonsWorkSet ap;
    private CreditReportingAction creditReportingAction;
    private AdministrativeActCertification creditReporting;
    private ServiceProvider notifiedBy;
    private Standard moreCseCasesInd;
    private Array<CraGroup> cra;
    private Standard standard;
    private NextTranInfo hidden;
    private SsnWorkArea ssnWorkArea;
    private Common adminExmpExists;
    private Common goodCauseExists;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A MatchCsePersonsGroup group.</summary>
    [Serializable]
    public class MatchCsePersonsGroup
    {
      /// <summary>
      /// A value of DetailMatchCsePer.
      /// </summary>
      [JsonPropertyName("detailMatchCsePer")]
      public CsePersonsWorkSet DetailMatchCsePer
      {
        get => detailMatchCsePer ??= new();
        set => detailMatchCsePer = value;
      }

      /// <summary>
      /// A value of DetailMatchAlt.
      /// </summary>
      [JsonPropertyName("detailMatchAlt")]
      public Common DetailMatchAlt
      {
        get => detailMatchAlt ??= new();
        set => detailMatchAlt = value;
      }

      /// <summary>
      /// A value of MatchAe.
      /// </summary>
      [JsonPropertyName("matchAe")]
      public Common MatchAe
      {
        get => matchAe ??= new();
        set => matchAe = value;
      }

      /// <summary>
      /// A value of MatchCse.
      /// </summary>
      [JsonPropertyName("matchCse")]
      public Common MatchCse
      {
        get => matchCse ??= new();
        set => matchCse = value;
      }

      /// <summary>
      /// A value of MatchKanpay.
      /// </summary>
      [JsonPropertyName("matchKanpay")]
      public Common MatchKanpay
      {
        get => matchKanpay ??= new();
        set => matchKanpay = value;
      }

      /// <summary>
      /// A value of MatchKscares.
      /// </summary>
      [JsonPropertyName("matchKscares")]
      public Common MatchKscares
      {
        get => matchKscares ??= new();
        set => matchKscares = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet detailMatchCsePer;
      private Common detailMatchAlt;
      private Common matchAe;
      private Common matchCse;
      private Common matchKanpay;
      private Common matchKscares;
    }

    /// <summary>
    /// A value of Datenum.
    /// </summary>
    [JsonPropertyName("datenum")]
    public DateWorkArea Datenum
    {
      get => datenum ??= new();
      set => datenum = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public Case1 InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of SearchOption.
    /// </summary>
    [JsonPropertyName("searchOption")]
    public Common SearchOption
    {
      get => searchOption ??= new();
      set => searchOption = value;
    }

    /// <summary>
    /// Gets a value of MatchCsePersons.
    /// </summary>
    [JsonIgnore]
    public Array<MatchCsePersonsGroup> MatchCsePersons =>
      matchCsePersons ??= new(MatchCsePersonsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MatchCsePersons for json serialization.
    /// </summary>
    [JsonPropertyName("matchCsePersons")]
    [Computed]
    public IList<MatchCsePersonsGroup> MatchCsePersons_Json
    {
      get => matchCsePersons;
      set => MatchCsePersons.Assign(value);
    }

    private DateWorkArea datenum;
    private DateWorkArea current;
    private Common returnCode;
    private CodeValue codeValue;
    private Code code;
    private TextWorkArea textWorkArea;
    private Case1 initialisedToSpaces;
    private NextTranInfo nextTranInfo;
    private Common searchOption;
    private Array<MatchCsePersonsGroup> matchCsePersons;
  }
#endregion
}
