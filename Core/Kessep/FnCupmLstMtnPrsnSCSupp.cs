// Program: FN_CUPM_LST_MTN_PRSN_S_C_SUPP, ID: 371738112, model: 746.
// Short name: SWECUPMP
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
/// A program: FN_CUPM_LST_MTN_PRSN_S_C_SUPP.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCupmLstMtnPrsnSCSupp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CUPM_LST_MTN_PRSN_S_C_SUPP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCupmLstMtnPrsnSCSupp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCupmLstMtnPrsnSCSupp.
  /// </summary>
  public FnCupmLstMtnPrsnSCSupp(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 07/17/96   G. Lofton - MTW			Initial code.
    // 6/9/97	   A Samuels - MTW			Disallow overlapping dates.
    // 8/26/98    E. Parker - SRS			Removed edit on discontinue date under case 
    // of process.  Added edits to prevent update of doc type or reason text on
    // active record.
    // 8/27/98    E. Parker - SRS			Changed logic to prevent date overlap of 
    // diff doc types.
    // 8/28/98    E. Parker - SRS			Enabled scrolling.
    // 9/1/98     E. Parker - SRS			Changed CURRENT_DATE comparisons to local 
    // date_work_area date.
    // 9/2/98     E. Parker - SRS			Changed logic to allowed disc date = current
    // date.
    // Oct, 2000 M. Brown  PR# 106234 Updated NEXT TRAN.
    // ------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ***************************************************
    // Move Imports to Exports.
    // ***************************************************
    export.NextTranInfo.Assign(import.NextTranInfo);
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.PersonPrompt.PromptField = import.PersonPrompt.PromptField;
    export.HiddenPrev.Number = import.HiddenPrev.Number;
    export.HiddenPrevHistory.Flag = import.HiddenPrevHistory.Flag;

    if (!IsEmpty(import.History.Flag))
    {
      export.History.Flag = import.History.Flag;
    }
    else
    {
      export.History.Flag = "N";
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

      export.Export1.Update.Common.ActionEntry =
        import.Import1.Item.Common.ActionEntry;
      export.Export1.Update.DetStmtCouponSuppStatusHist.Assign(
        import.Import1.Item.DetStmtCouponSuppStatusHist);
      export.Export1.Update.DetTextWorkArea.Text10 =
        import.Import1.Item.DetTextWorkArea.Text10;
      export.Export1.Update.DetPrev.Assign(import.Import1.Item.DetPrev);
      export.Export1.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // : Mbrown, Oct 2000, pr# 106234 - put nexttran info into export instead of
    // local view.
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsEmpty(export.NextTranInfo.CsePersonNumber))
      {
        export.CsePerson.Number = export.NextTranInfo.CsePersonNumber ?? Spaces
          (10);
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

    // : Mbrown, Oct 2000, pr# 106234 - used export instead of local view
    //   to send next tran info to cab .
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      if (Equal(export.CsePerson.Number, export.NextTranInfo.CsePersonNumber))
      {
        // : do nothing if the person number has not been changed.
      }
      else if (IsEmpty(export.CsePerson.Number))
      {
        // : do nothing if the person number has been spaced out.
      }
      else
      {
        export.NextTranInfo.CsePersonNumber = export.CsePerson.Number;
        export.NextTranInfo.CsePersonNumberAp = export.CsePerson.Number;
        export.NextTranInfo.CsePersonNumberObligor = export.CsePerson.Number;
        export.NextTranInfo.CsePersonNumberObligee = "";
        export.NextTranInfo.CaseNumber = "";
        export.NextTranInfo.CourtOrderNumber = "";
        export.NextTranInfo.StandardCrtOrdNumber = "";
        export.NextTranInfo.ObligationId = 0;
        export.NextTranInfo.MiscNum1 = 0;
        export.NextTranInfo.MiscNum2 = 0;
        export.NextTranInfo.LegalActionIdentifier = 0;
      }

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

    if (Equal(global.Command, "RETNAME"))
    {
      export.PersonPrompt.PromptField = "";

      if (!IsEmpty(import.FromListCsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = import.FromListCsePersonsWorkSet.Number;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "LIST"))
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

    local.DateWorkArea.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (IsEmpty(export.CsePerson.Number))
        {
          ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        local.CsePersonsWorkSet.Number = export.CsePerson.Number;
        export.PersonPrompt.PromptField = "";
        UseSiReadCsePerson();
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        if (AsChar(export.History.Flag) != 'Y' && AsChar
          (export.History.Flag) != 'N')
        {
          var field = GetField(export.History, "flag");

          field.Error = true;

          ExitState = "ZD_ACO_NE00_INVALID_SELECTION_YN";

          return;
        }

        export.HiddenPrev.Number = export.CsePerson.Number;
        export.HiddenPrevHistory.Flag = export.History.Flag;

        if (!ReadCsePersonAccount())
        {
          ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

          return;
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadStmtCouponSuppStatusHist())
        {
          if (AsChar(export.History.Flag) != 'Y')
          {
            if (Lt(entities.StmtCouponSuppStatusHist.DiscontinueDate,
              local.DateWorkArea.Date))
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.Common.ActionEntry = "";
          export.Export1.Update.DetStmtCouponSuppStatusHist.Assign(
            entities.StmtCouponSuppStatusHist);
          export.Export1.Update.DetPrev.
            Assign(entities.StmtCouponSuppStatusHist);

          if (Equal(export.Export1.Item.DetStmtCouponSuppStatusHist.
            DiscontinueDate, local.Max.Date))
          {
            export.Export1.Update.DetStmtCouponSuppStatusHist.DiscontinueDate =
              local.Blank.Date;
          }

          if (AsChar(export.Export1.Item.DetStmtCouponSuppStatusHist.
            DocTypeToSuppress) == 'C')
          {
            export.Export1.Update.DetTextWorkArea.Text10 = "COUPN ONLY";
          }
          else if (AsChar(export.Export1.Item.DetStmtCouponSuppStatusHist.
            DocTypeToSuppress) == 'S')
          {
            export.Export1.Update.DetTextWorkArea.Text10 = "STMT&COUPN";
          }
          else
          {
            export.Export1.Update.DetTextWorkArea.Text10 = "";
          }

          export.Export1.Next();
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

          return;
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "FN0000_STMT_CPN_SUPP_HIST_NF";
        }

        break;
      case "LIST":
        // : Prompts
        switch(AsChar(export.PersonPrompt.PromptField))
        {
          case 'S':
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.PersonPrompt, "promptField");

            field.Error = true;

            return;
        }

        // :  If logic comes here, none of the prompt fields were selected.
        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "PROCESS":
        if (!Equal(export.CsePerson.Number, export.HiddenPrev.Number))
        {
          ExitState = "PERSON_HAS_CHANGED_MUST_DISPLAY";

          return;
        }

        if (AsChar(export.History.Flag) != AsChar
          (export.HiddenPrevHistory.Flag))
        {
          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

          return;
        }

        // : VALIDATION CASE OF COMMAND
        local.Common.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.Common.ActionEntry, "A") || Equal
            (export.Export1.Item.Common.ActionEntry, "C") || Equal
            (export.Export1.Item.Common.ActionEntry, "D"))
          {
            // Make sure all mandatory fields have been entered.
            ++local.Common.Count;

            if (AsChar(export.Export1.Item.DetStmtCouponSuppStatusHist.
              DocTypeToSuppress) == 'C' || AsChar
              (export.Export1.Item.DetStmtCouponSuppStatusHist.DocTypeToSuppress)
              == 'S')
            {
            }
            else if (IsEmpty(export.Export1.Item.DetStmtCouponSuppStatusHist.
              DocTypeToSuppress))
            {
              var field =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "docTypeToSuppress");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
            else
            {
              var field =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "docTypeToSuppress");

              field.Error = true;

              ExitState = "FN0000_INVALID_STMT_CPN_DOC_TYPE";
            }

            if (!Lt(local.Blank.Date,
              export.Export1.Item.DetStmtCouponSuppStatusHist.EffectiveDate))
            {
              var field =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "effectiveDate");

              field.Error = true;

              ExitState = "EFFECTIVE_DATE_REQUIRED";
            }

            if (Equal(export.Export1.Item.Common.ActionEntry, "A") || Equal
              (export.Export1.Item.Common.ActionEntry, "C"))
            {
              if (IsEmpty(export.Export1.Item.DetStmtCouponSuppStatusHist.
                ReasonText))
              {
                var field =
                  GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                  "reasonText");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }

              // *********************************************************************
              // Do not allow overlapping suppressions of the same type to 
              // occur.
              // *********************************************************************
              for(import.Import1.Index = 0; import.Import1.Index < import
                .Import1.Count; ++import.Import1.Index)
              {
                // *********************************************************************
                // Only compare suppressions of same type.
                // 8/27/98 Changed logic to compare both types.
                // Ignore rows being deleted.
                // Don't compare row being processed to itself.
                // *********************************************************************
                if (!Equal(import.Import1.Item.Common.ActionEntry, "D") && import
                  .Import1.Item.DetStmtCouponSuppStatusHist.
                    SystemGeneratedIdentifier != export
                  .Export1.Item.DetStmtCouponSuppStatusHist.
                    SystemGeneratedIdentifier)
                {
                  if (!Lt(export.Export1.Item.DetStmtCouponSuppStatusHist.
                    EffectiveDate,
                    import.Import1.Item.DetStmtCouponSuppStatusHist.
                      EffectiveDate) && Lt
                    (export.Export1.Item.DetStmtCouponSuppStatusHist.
                      EffectiveDate,
                    import.Import1.Item.DetStmtCouponSuppStatusHist.
                      DiscontinueDate) || !
                    Lt(export.Export1.Item.DetStmtCouponSuppStatusHist.
                      DiscontinueDate,
                    import.Import1.Item.DetStmtCouponSuppStatusHist.
                      EffectiveDate) && !
                    Lt(import.Import1.Item.DetStmtCouponSuppStatusHist.
                      DiscontinueDate,
                    export.Export1.Item.DetStmtCouponSuppStatusHist.
                      DiscontinueDate) || Lt
                    (export.Export1.Item.DetStmtCouponSuppStatusHist.
                      EffectiveDate,
                    import.Import1.Item.DetStmtCouponSuppStatusHist.
                      EffectiveDate) && Lt
                    (import.Import1.Item.DetStmtCouponSuppStatusHist.
                      DiscontinueDate,
                    export.Export1.Item.DetStmtCouponSuppStatusHist.
                      DiscontinueDate))
                  {
                    var field =
                      GetField(export.Export1.Item.Common, "actionEntry");

                    field.Error = true;

                    ExitState = "FN0000_DATES_OVERLAP";

                    break;
                  }
                }
              }
            }
          }
          else
          {
            switch(TrimEnd(export.Export1.Item.Common.ActionEntry))
            {
              case "":
                break;
              case "*":
                export.Export1.Update.Common.ActionEntry = "";

                break;
              default:
                ExitState = "INVALID_ACTION_ENTER_A_C_OR_D";

                var field = GetField(export.Export1.Item.Common, "actionEntry");

                field.Error = true;

                break;
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NI0000_NO_ACTION_REQUESTED";

          return;
        }

        // PROCESS ACTIONS - ADD, CHANGE, DELETE.
        // ** Delete Statement/Coupon Processing **
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.Common.ActionEntry, "D"))
          {
            if (Lt(export.Export1.Item.DetStmtCouponSuppStatusHist.
              EffectiveDate, local.DateWorkArea.Date))
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

              ExitState = "FN0000_STMT_CPN_SUPP_HIST_ACTIVE";

              return;
            }

            UseFnDeleteApPyrStmtCpnHist();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Common.ActionEntry = "*";
            }
            else
            {
              return;
            }
          }
        }

        // ** Change Statement/Coupon Processing **
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.Common.ActionEntry, "C"))
          {
            if (AsChar(export.Export1.Item.DetStmtCouponSuppStatusHist.
              DocTypeToSuppress) == 'C')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "COUPN ONLY";
            }
            else if (AsChar(export.Export1.Item.DetStmtCouponSuppStatusHist.
              DocTypeToSuppress) == 'S')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "STMT&COUPN";
            }

            if (AsChar(export.Export1.Item.DetPrev.DocTypeToSuppress) != AsChar
              (export.Export1.Item.DetStmtCouponSuppStatusHist.DocTypeToSuppress))
              
            {
              if (Lt(export.Export1.Item.DetPrev.EffectiveDate,
                local.DateWorkArea.Date))
              {
                var field =
                  GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                  "docTypeToSuppress");

                field.Error = true;

                ExitState = "FN0000_CANNOT_CHG_DOC_TYPE";

                return;
              }
            }

            if (!Equal(export.Export1.Item.DetPrev.ReasonText,
              export.Export1.Item.DetStmtCouponSuppStatusHist.ReasonText))
            {
              if (Lt(export.Export1.Item.DetPrev.EffectiveDate,
                local.DateWorkArea.Date))
              {
                var field =
                  GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                  "reasonText");

                field.Error = true;

                ExitState = "FN0000_CANNOT_CHG_REASON_TXT";

                return;
              }
            }

            if (Lt(export.Export1.Item.DetPrev.DiscontinueDate,
              local.DateWorkArea.Date))
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

              ExitState = "FN0000_CANT_UPDATE_DISCONTINUED";

              return;
            }

            if (!Equal(export.Export1.Item.DetStmtCouponSuppStatusHist.
              EffectiveDate, export.Export1.Item.DetPrev.EffectiveDate))
            {
              if (Lt(export.Export1.Item.DetPrev.EffectiveDate,
                local.DateWorkArea.Date))
              {
                var field1 =
                  GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                  "effectiveDate");

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.Common, "actionEntry");

                field2.Error = true;

                ExitState = "FN0000_CANNOT_CHG_EFF_DT";

                return;
              }

              if (Lt(export.Export1.Item.DetStmtCouponSuppStatusHist.
                EffectiveDate, local.DateWorkArea.Date))
              {
                var field1 =
                  GetField(export.Export1.Item.Common, "actionEntry");

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                  "effectiveDate");

                field2.Error = true;

                ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

                return;
              }
            }

            if (!Lt(local.Blank.Date,
              export.Export1.Item.DetStmtCouponSuppStatusHist.DiscontinueDate))
            {
              export.Export1.Update.DetStmtCouponSuppStatusHist.
                DiscontinueDate = local.Max.Date;
            }

            if (AsChar(export.Export1.Item.DetStmtCouponSuppStatusHist.
              DocTypeToSuppress) == AsChar
              (export.Export1.Item.DetPrev.DocTypeToSuppress) && Equal
              (export.Export1.Item.DetStmtCouponSuppStatusHist.EffectiveDate,
              export.Export1.Item.DetPrev.EffectiveDate) && Equal
              (export.Export1.Item.DetStmtCouponSuppStatusHist.DiscontinueDate,
              export.Export1.Item.DetPrev.DiscontinueDate) && Equal
              (export.Export1.Item.DetStmtCouponSuppStatusHist.ReasonText,
              export.Export1.Item.DetPrev.ReasonText))
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

              ExitState = "FN0000_NO_UPDATES_MADE";

              goto Test;
            }

            UseFnValidateApPyrStmtCpnDts();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field1 = GetField(export.Export1.Item.Common, "actionEntry");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "discontinueDate");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "effectiveDate");

              field3.Error = true;

              goto Test;
            }

            UseFnUpdateApPyrStmtCpnHist();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.DetPrev.Assign(
                export.Export1.Item.DetStmtCouponSuppStatusHist);
            }
            else
            {
            }
          }

Test:

          if (Equal(export.Export1.Item.Common.ActionEntry, "C"))
          {
            if (Equal(export.Export1.Item.DetStmtCouponSuppStatusHist.
              DiscontinueDate, local.Max.Date))
            {
              export.Export1.Update.DetStmtCouponSuppStatusHist.
                DiscontinueDate = local.Blank.Date;
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Common.ActionEntry = "*";
            }
            else
            {
              return;
            }
          }
        }

        // ** Add Statement/Coupon Processing **
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.Common.ActionEntry, "A"))
          {
            if (AsChar(export.Export1.Item.DetStmtCouponSuppStatusHist.
              DocTypeToSuppress) == 'C')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "COUPN ONLY";
            }
            else if (AsChar(export.Export1.Item.DetStmtCouponSuppStatusHist.
              DocTypeToSuppress) == 'S')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "STMT&COUPN";
            }

            if (Lt(export.Export1.Item.DetStmtCouponSuppStatusHist.
              EffectiveDate, local.DateWorkArea.Date))
            {
              var field1 = GetField(export.Export1.Item.Common, "actionEntry");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "effectiveDate");

              field2.Error = true;

              ExitState = "FN0000_EFF_DATE_NOT_IN_FUTURE";

              return;
            }

            if (!Lt(local.Blank.Date,
              export.Export1.Item.DetStmtCouponSuppStatusHist.DiscontinueDate))
            {
              export.Export1.Update.DetStmtCouponSuppStatusHist.
                DiscontinueDate = local.Max.Date;
            }

            UseFnValidateApPyrStmtCpnDts();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field1 = GetField(export.Export1.Item.Common, "actionEntry");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "discontinueDate");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "effectiveDate");

              field3.Error = true;

              return;
            }

            // ***--- 05/19/1997  - Sumanta - MTW
            // ***--- Currently if action 'A' is entered on an existing row,
            //        a duplicate row is getting added. The following logic
            //        will stop that.
            // ***---
            if (export.Export1.Item.DetStmtCouponSuppStatusHist.
              SystemGeneratedIdentifier > 0)
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              return;
            }

            UseFnCreateApPyrStmtCpnHist();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Common.ActionEntry = "*";
              export.Export1.Update.DetPrev.Assign(
                export.Export1.Item.DetStmtCouponSuppStatusHist);

              if (Equal(export.Export1.Item.DetStmtCouponSuppStatusHist.
                DiscontinueDate, local.Max.Date))
              {
                export.Export1.Update.DetStmtCouponSuppStatusHist.
                  DiscontinueDate = local.Blank.Date;
              }
            }
            else
            {
              return;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
        }
        else
        {
        }

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
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
  }

  private static void MoveStmtCouponSuppStatusHist1(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.DocTypeToSuppress = source.DocTypeToSuppress;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private static void MoveStmtCouponSuppStatusHist2(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCreateApPyrStmtCpnHist()
  {
    var useImport = new FnCreateApPyrStmtCpnHist.Import();
    var useExport = new FnCreateApPyrStmtCpnHist.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.StmtCouponSuppStatusHist.Assign(
      export.Export1.Item.DetStmtCouponSuppStatusHist);

    Call(FnCreateApPyrStmtCpnHist.Execute, useImport, useExport);

    MoveStmtCouponSuppStatusHist1(useExport.StmtCouponSuppStatusHist,
      export.Export1.Update.DetStmtCouponSuppStatusHist);
  }

  private void UseFnDeleteApPyrStmtCpnHist()
  {
    var useImport = new FnDeleteApPyrStmtCpnHist.Import();
    var useExport = new FnDeleteApPyrStmtCpnHist.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.StmtCouponSuppStatusHist.Assign(
      export.Export1.Item.DetStmtCouponSuppStatusHist);

    Call(FnDeleteApPyrStmtCpnHist.Execute, useImport, useExport);
  }

  private void UseFnUpdateApPyrStmtCpnHist()
  {
    var useImport = new FnUpdateApPyrStmtCpnHist.Import();
    var useExport = new FnUpdateApPyrStmtCpnHist.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.StmtCouponSuppStatusHist.Assign(
      export.Export1.Item.DetStmtCouponSuppStatusHist);

    Call(FnUpdateApPyrStmtCpnHist.Execute, useImport, useExport);

    MoveStmtCouponSuppStatusHist1(useExport.StmtCouponSuppStatusHist,
      export.Export1.Update.DetStmtCouponSuppStatusHist);
  }

  private void UseFnValidateApPyrStmtCpnDts()
  {
    var useImport = new FnValidateApPyrStmtCpnDts.Import();
    var useExport = new FnValidateApPyrStmtCpnDts.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveStmtCouponSuppStatusHist2(export.Export1.Item.
      DetStmtCouponSuppStatusHist, useImport.StmtCouponSuppStatusHist);
    useImport.Common.ActionEntry = export.Export1.Item.Common.ActionEntry;

    Call(FnValidateApPyrStmtCpnDts.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveNextTranInfo(export.NextTranInfo, useImport.NextTranInfo);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private IEnumerable<bool> ReadStmtCouponSuppStatusHist()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    return ReadEach("ReadStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.StmtCouponSuppStatusHist.CpaType = db.GetString(reader, 0);
        entities.StmtCouponSuppStatusHist.CspNumber = db.GetString(reader, 1);
        entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.StmtCouponSuppStatusHist.Type1 = db.GetString(reader, 3);
        entities.StmtCouponSuppStatusHist.EffectiveDate = db.GetDate(reader, 4);
        entities.StmtCouponSuppStatusHist.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.StmtCouponSuppStatusHist.ReasonText =
          db.GetNullableString(reader, 6);
        entities.StmtCouponSuppStatusHist.CreatedBy = db.GetString(reader, 7);
        entities.StmtCouponSuppStatusHist.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.StmtCouponSuppStatusHist.DocTypeToSuppress =
          db.GetString(reader, 9);
        entities.StmtCouponSuppStatusHist.Populated = true;
        CheckValid<StmtCouponSuppStatusHist>("CpaType",
          entities.StmtCouponSuppStatusHist.CpaType);
        CheckValid<StmtCouponSuppStatusHist>("Type1",
          entities.StmtCouponSuppStatusHist.Type1);
        CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress",
          entities.StmtCouponSuppStatusHist.DocTypeToSuppress);

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
      /// A value of DetStmtCouponSuppStatusHist.
      /// </summary>
      [JsonPropertyName("detStmtCouponSuppStatusHist")]
      public StmtCouponSuppStatusHist DetStmtCouponSuppStatusHist
      {
        get => detStmtCouponSuppStatusHist ??= new();
        set => detStmtCouponSuppStatusHist = value;
      }

      /// <summary>
      /// A value of DetPrev.
      /// </summary>
      [JsonPropertyName("detPrev")]
      public StmtCouponSuppStatusHist DetPrev
      {
        get => detPrev ??= new();
        set => detPrev = value;
      }

      /// <summary>
      /// A value of DetTextWorkArea.
      /// </summary>
      [JsonPropertyName("detTextWorkArea")]
      public TextWorkArea DetTextWorkArea
      {
        get => detTextWorkArea ??= new();
        set => detTextWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common common;
      private StmtCouponSuppStatusHist detStmtCouponSuppStatusHist;
      private StmtCouponSuppStatusHist detPrev;
      private TextWorkArea detTextWorkArea;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePerson HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of PersonPrompt.
    /// </summary>
    [JsonPropertyName("personPrompt")]
    public Standard PersonPrompt
    {
      get => personPrompt ??= new();
      set => personPrompt = value;
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
    /// A value of FromListCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fromListCsePersonsWorkSet")]
    public CsePersonsWorkSet FromListCsePersonsWorkSet
    {
      get => fromListCsePersonsWorkSet ??= new();
      set => fromListCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FromListCsePerson.
    /// </summary>
    [JsonPropertyName("fromListCsePerson")]
    public CsePerson FromListCsePerson
    {
      get => fromListCsePerson ??= new();
      set => fromListCsePerson = value;
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
    /// A value of HiddenPrevHistory.
    /// </summary>
    [JsonPropertyName("hiddenPrevHistory")]
    public Common HiddenPrevHistory
    {
      get => hiddenPrevHistory ??= new();
      set => hiddenPrevHistory = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private Standard standard;
    private CsePerson csePerson;
    private CsePerson hiddenPrev;
    private Standard personPrompt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet fromListCsePersonsWorkSet;
    private CsePerson fromListCsePerson;
    private Common history;
    private Common hiddenPrevHistory;
    private Array<ImportGroup> import1;
    private NextTranInfo nextTranInfo;
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
      /// A value of DetStmtCouponSuppStatusHist.
      /// </summary>
      [JsonPropertyName("detStmtCouponSuppStatusHist")]
      public StmtCouponSuppStatusHist DetStmtCouponSuppStatusHist
      {
        get => detStmtCouponSuppStatusHist ??= new();
        set => detStmtCouponSuppStatusHist = value;
      }

      /// <summary>
      /// A value of DetPrev.
      /// </summary>
      [JsonPropertyName("detPrev")]
      public StmtCouponSuppStatusHist DetPrev
      {
        get => detPrev ??= new();
        set => detPrev = value;
      }

      /// <summary>
      /// A value of DetTextWorkArea.
      /// </summary>
      [JsonPropertyName("detTextWorkArea")]
      public TextWorkArea DetTextWorkArea
      {
        get => detTextWorkArea ??= new();
        set => detTextWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common common;
      private StmtCouponSuppStatusHist detStmtCouponSuppStatusHist;
      private StmtCouponSuppStatusHist detPrev;
      private TextWorkArea detTextWorkArea;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public CsePerson HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
    }

    /// <summary>
    /// A value of PersonPrompt.
    /// </summary>
    [JsonPropertyName("personPrompt")]
    public Standard PersonPrompt
    {
      get => personPrompt ??= new();
      set => personPrompt = value;
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
    /// A value of History.
    /// </summary>
    [JsonPropertyName("history")]
    public Common History
    {
      get => history ??= new();
      set => history = value;
    }

    /// <summary>
    /// A value of HiddenPrevHistory.
    /// </summary>
    [JsonPropertyName("hiddenPrevHistory")]
    public Common HiddenPrevHistory
    {
      get => hiddenPrevHistory ??= new();
      set => hiddenPrevHistory = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private Standard standard;
    private CsePerson csePerson;
    private CsePerson hiddenPrev;
    private Standard personPrompt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common history;
    private Common hiddenPrevHistory;
    private Array<ExportGroup> export1;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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

    private DateWorkArea max;
    private DateWorkArea dateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea blank;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
