// Program: FN_CSPM_LST_MTN_OBLIG_S_C_SUPP, ID: 371737089, model: 746.
// Short name: SWECSPMP
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
/// A program: FN_CSPM_LST_MTN_OBLIG_S_C_SUPP.
/// </para>
/// <para>
/// Resp: Finance
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCspmLstMtnObligSCSupp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CSPM_LST_MTN_OBLIG_S_C_SUPP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCspmLstMtnObligSCSupp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCspmLstMtnObligSCSupp.
  /// </summary>
  public FnCspmLstMtnObligSCSupp(IContext context, Import import, Export export):
    
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
    // Date	   Programmer		Description
    // 07/23/96   R. Welborn - MTW	Initial code.
    // 06/14/97   T.O.Redmond  MTW     Make Reason Text Mandatory
    // 				Make Discontinue Date Mandatory
    // 				Add Success Exit_States
    // 09/26/97	Siraj Konkader
    // ADD SUCC exit state was being set before call to CREATE CAB.
    // Moved it to common edit area for A/C/D's.
    // 8/28/98    E. Parker - SRS	Changed sort to descending.  Changed disc date
    // to default to max if blank.  Added edits to doc type and reason under '
    // C' process.
    // 8/31/98    E. Parker - SRS	Added edit to prevent disc date less than 
    // current date.
    // 6/19/1999  E. Parker	Changed logic to use fn_compute_summary_totals.  
    // Changed logic to ensure that a suppression record cannot get past edits
    // on add when user enters data over existing record.
    // 7/1/99 - B. Adams  -  Set Read properties.
    // ------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    UseFnHardcodedDebtDistribution();
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    export.DueDte.Date = import.DueDte.Date;
    export.ObligAmt.TotalCurrency = import.ObligAmt.TotalCurrency;
    export.Obligation.Assign(import.Obligation);
    MoveObligationType(import.ObligationType, export.ObligationType);
    export.ObligationPaymentSchedule.FrequencyCode =
      import.ObligationPaymentSchedule.FrequencyCode;
    export.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    export.HiddenSuccessfulDisplay.Flag = import.HiddenSuccessfulDisplay.Flag;

    // ***************************************************
    // Move Imports to Exports.
    // ***************************************************
    export.HiddenPrev.Assign(import.HiddenPrev);
    export.ShowHistory.Flag = export.HiddenPrevHistory.Flag;

    if (!IsEmpty(import.ShowHistory.Flag))
    {
      export.ShowHistory.Flag = import.ShowHistory.Flag;
    }
    else
    {
      export.ShowHistory.Flag = "N";
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ************************************************
    // *Flowing from Next Tran to Here.               *
    // ************************************************
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
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

    if (!Equal(global.Command, "PROCESS"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (Equal(export.Export1.Item.DetStmtCouponSuppStatusHist.
          DiscontinueDate, new DateTime(2099, 12, 31)))
        {
          export.Export1.Update.DetStmtCouponSuppStatusHist.DiscontinueDate =
            null;
        }
      }
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      local.NextTranInfo.ObligationId =
        export.Obligation.SystemGeneratedIdentifier;
      local.NextTranInfo.CsePersonNumber = export.CsePerson.Number;
      local.NextTranInfo.StandardCrtOrdNumber =
        export.LegalAction.StandardNumber ?? "";
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "RETURN"))
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

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        local.CsePersonsWorkSet.Number = import.CsePerson.Number;
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        if (ReadObligation())
        {
          export.Obligation.Assign(entities.Obligation);
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF";

          return;
        }

        if (ReadObligationType())
        {
          if (AsChar(entities.ObligationType.Classification) == 'A')
          {
            if (ReadObligationPaymentSchedule())
            {
              export.ObligationPaymentSchedule.FrequencyCode =
                entities.ObligationPaymentSchedule.FrequencyCode;
            }
            else
            {
              ExitState = "FN0000_OBLIG_PYMNT_SCH_NF";

              return;
            }
          }
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_TYPE_NF";

          return;
        }

        UseFnComputeSummaryTotals();

        if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseFnCabSetAccrualOrDueAmount();

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadStmtCouponSuppStatusHist())
        {
          if (AsChar(export.ShowHistory.Flag) != 'Y')
          {
            if (Lt(entities.StmtCouponSuppStatusHist.DiscontinueDate,
              local.Current.Date))
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.Common.ActionEntry = "";
          export.Export1.Update.DetStmtCouponSuppStatusHist.Assign(
            entities.StmtCouponSuppStatusHist);
          MoveStmtCouponSuppStatusHist3(entities.StmtCouponSuppStatusHist,
            export.Export1.Update.DetPrev);

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

          return;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        export.HiddenSuccessfulDisplay.Flag = "Y";

        break;
      case "PROCESS":
        // : VALIDATION CASE OF COMMAND
        if (export.Export1.IsEmpty)
        {
          ExitState = "SP0000_ACTION_INDICATOR_NOT_VLD";

          return;
        }
        else
        {
          local.Common.Count = 0;
        }

        if (import.Obligation.SystemGeneratedIdentifier != 0)
        {
          if (ReadObligation())
          {
            export.Obligation.Assign(entities.Obligation);
          }
          else
          {
            ExitState = "FN0000_OBLIGATION_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0000_SELECT_OBLIGATION_BEFORE";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.Common.ActionEntry, "A") || Equal
            (export.Export1.Item.Common.ActionEntry, "C") || Equal
            (export.Export1.Item.Common.ActionEntry, "D"))
          {
            ++local.Common.Count;

            // Make sure all mandatory fields have been entered.
            if (Equal(export.Export1.Item.Common.ActionEntry, "D"))
            {
              continue;
            }

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

            if (IsEmpty(export.Export1.Item.DetStmtCouponSuppStatusHist.
              ReasonText))
            {
              var field =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "reasonText");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (!Lt(local.Blank.Date,
              export.Export1.Item.DetStmtCouponSuppStatusHist.EffectiveDate))
            {
              var field =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "effectiveDate");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }
          else
          {
            switch(TrimEnd(export.Export1.Item.Common.ActionEntry))
            {
              case "":
                break;
              case "S":
                break;
              case "*":
                export.Export1.Update.Common.ActionEntry = "";

                break;
              default:
                ExitState = "FN0000_INVALID_ACTION_CODE";

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
        else if (local.Common.Count < 1)
        {
          ExitState = "ACO_NE0000_INVALID_ACTION";

          return;
        }

        // PROCESS ACTIONS - ADD, CHANGE, DELETE.
        // ************************************************
        // *Delete Statement/Coupon Processing            *
        // ************************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.Common.ActionEntry, "D"))
          {
            if (Lt(export.Export1.Item.DetStmtCouponSuppStatusHist.
              EffectiveDate, local.Current.Date))
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

              ExitState = "FN0000_STMT_CPN_SUPP_HIST_ACTIVE";

              return;
            }

            UseFnDeleteObligStmtCpnHist();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Common.ActionEntry = "*";
              ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
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
            if (Lt(export.Export1.Item.DetPrev.DiscontinueDate,
              local.Current.Date))
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

              ExitState = "FN0000_CANT_UPDATE_DISCONTINUED";

              return;
            }

            if (!Equal(export.Export1.Item.DetStmtCouponSuppStatusHist.
              EffectiveDate, export.Export1.Item.DetPrev.EffectiveDate) && Lt
              (export.Export1.Item.DetPrev.EffectiveDate, local.Current.Date))
            {
              var field1 =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "effectiveDate");

              field1.Error = true;

              var field2 = GetField(export.Export1.Item.Common, "actionEntry");

              field2.Error = true;

              ExitState = "FN0000_CANNOT_CHG_EFF_DT";

              return;
            }
            else if (!Equal(export.Export1.Item.DetStmtCouponSuppStatusHist.
              EffectiveDate, export.Export1.Item.DetPrev.EffectiveDate) && Lt
              (export.Export1.Item.DetStmtCouponSuppStatusHist.EffectiveDate,
              local.Current.Date))
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

              ExitState = "FN0000_EFF_DATE_NOT_IN_FUTURE";

              return;
            }

            if (AsChar(export.Export1.Item.DetStmtCouponSuppStatusHist.
              DocTypeToSuppress) != AsChar
              (export.Export1.Item.DetPrev.DocTypeToSuppress))
            {
              if (Lt(export.Export1.Item.DetPrev.EffectiveDate,
                local.Current.Date))
              {
                var field =
                  GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                  "docTypeToSuppress");

                field.Error = true;

                ExitState = "FN0000_CANNOT_CHG_DOC_TYPE";

                return;
              }
            }

            if (!Equal(export.Export1.Item.DetStmtCouponSuppStatusHist.
              ReasonText, export.Export1.Item.DetPrev.ReasonText))
            {
              if (Lt(export.Export1.Item.DetPrev.EffectiveDate,
                local.Current.Date))
              {
                var field =
                  GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                  "reasonText");

                field.Error = true;

                ExitState = "FN0000_CANNOT_CHG_REASON_TXT";

                return;
              }
            }

            if (!Lt(local.Blank.Date,
              export.Export1.Item.DetStmtCouponSuppStatusHist.DiscontinueDate))
            {
              export.Export1.Update.DetStmtCouponSuppStatusHist.
                DiscontinueDate = local.Max.Date;
            }

            if (!Equal(export.Export1.Item.DetPrev.DiscontinueDate,
              export.Export1.Item.DetStmtCouponSuppStatusHist.DiscontinueDate))
            {
              if (Lt(export.Export1.Item.DetStmtCouponSuppStatusHist.
                DiscontinueDate, local.Current.Date))
              {
                var field =
                  GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                  "discontinueDate");

                field.Error = true;

                ExitState = "ACO_NE0000_END_CANT_B_LESS_CRNT";

                return;
              }
            }

            UseFnValidateObligStmtCpnDates();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field1 = GetField(export.Export1.Item.Common, "actionEntry");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "effectiveDate");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "discontinueDate");

              field3.Error = true;

              return;
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

            UseFnUpdateObligStmtCpnHist();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Common.ActionEntry = "*";
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
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
              EffectiveDate, local.Current.Date))
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

            if (IsEmpty(export.Export1.Item.DetStmtCouponSuppStatusHist.
              ReasonText))
            {
              ExitState = "SP0000_MANDATORY_FIELD_NOT_ENTRD";

              var field =
                GetField(export.Export1.Item.DetStmtCouponSuppStatusHist,
                "reasonText");

              field.Error = true;

              return;
            }

            if (!Lt(local.Blank.Date,
              export.Export1.Item.DetStmtCouponSuppStatusHist.DiscontinueDate))
            {
              export.Export1.Update.DetStmtCouponSuppStatusHist.
                DiscontinueDate = local.Max.Date;
            }

            UseFnValidateObligStmtCpnDates();

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

            UseFnCreateObligStmtCpnHist();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Common.ActionEntry = "*";
              export.Export1.Update.DetPrev.Assign(
                export.Export1.Item.DetStmtCouponSuppStatusHist);
              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
            else
            {
              return;
            }
          }
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

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

  private static void MoveStmtCouponSuppStatusHist1(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.DocTypeToSuppress = source.DocTypeToSuppress;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveStmtCouponSuppStatusHist2(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.DocTypeToSuppress = source.DocTypeToSuppress;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.ReasonText = source.ReasonText;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveStmtCouponSuppStatusHist3(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.DocTypeToSuppress = source.DocTypeToSuppress;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.ReasonText = source.ReasonText;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCabSetAccrualOrDueAmount()
  {
    var useImport = new FnCabSetAccrualOrDueAmount.Import();
    var useExport = new FnCabSetAccrualOrDueAmount.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    MoveObligationType(import.ObligationType, useImport.ObligationType);
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;

    Call(FnCabSetAccrualOrDueAmount.Execute, useImport, useExport);

    export.ObligAmt.TotalCurrency = useExport.Common.TotalCurrency;
    export.DueDte.Date = useExport.StartDte.Date;
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = import.CsePerson.Number;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.Filter.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnCreateObligStmtCpnHist()
  {
    var useImport = new FnCreateObligStmtCpnHist.Import();
    var useExport = new FnCreateObligStmtCpnHist.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    useImport.StmtCouponSuppStatusHist.Assign(
      export.Export1.Item.DetStmtCouponSuppStatusHist);
    useImport.Current.Timestamp = local.Current.Timestamp;

    Call(FnCreateObligStmtCpnHist.Execute, useImport, useExport);

    export.Export1.Update.DetStmtCouponSuppStatusHist.Assign(
      useExport.StmtCouponSuppStatusHist);
  }

  private void UseFnDeleteObligStmtCpnHist()
  {
    var useImport = new FnDeleteObligStmtCpnHist.Import();
    var useExport = new FnDeleteObligStmtCpnHist.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    useImport.StmtCouponSuppStatusHist.Assign(
      export.Export1.Item.DetStmtCouponSuppStatusHist);

    Call(FnDeleteObligStmtCpnHist.Execute, useImport, useExport);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
  }

  private void UseFnUpdateObligStmtCpnHist()
  {
    var useImport = new FnUpdateObligStmtCpnHist.Import();
    var useExport = new FnUpdateObligStmtCpnHist.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    useImport.StmtCouponSuppStatusHist.Assign(
      export.Export1.Item.DetStmtCouponSuppStatusHist);
    useImport.Current.Timestamp = local.Current.Timestamp;

    Call(FnUpdateObligStmtCpnHist.Execute, useImport, useExport);

    MoveStmtCouponSuppStatusHist2(useExport.StmtCouponSuppStatusHist,
      export.Export1.Update.DetStmtCouponSuppStatusHist);
  }

  private void UseFnValidateObligStmtCpnDates()
  {
    var useImport = new FnValidateObligStmtCpnDates.Import();
    var useExport = new FnValidateObligStmtCpnDates.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    MoveStmtCouponSuppStatusHist1(export.Export1.Item.
      DetStmtCouponSuppStatusHist, useImport.StmtCouponSuppStatusHist);
    useImport.Common.ActionEntry = export.Export1.Item.Common.ActionEntry;

    Call(FnValidateObligStmtCpnDates.Execute, useImport, useExport);
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

    MoveNextTranInfo(local.NextTranInfo, useImport.NextTranInfo);
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

    useImport.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

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

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", local.HardcodeObligor.Type1);
        db.SetString(command, "cspNumber", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
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
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 5);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private IEnumerable<bool> ReadStmtCouponSuppStatusHist()
  {
    return ReadEach("ReadStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId", import.ObligationType.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.
          SetNullableString(command, "cspNumberOblig", import.CsePerson.Number);
          
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
        entities.StmtCouponSuppStatusHist.OtyId =
          db.GetNullableInt32(reader, 9);
        entities.StmtCouponSuppStatusHist.CpaTypeOblig =
          db.GetNullableString(reader, 10);
        entities.StmtCouponSuppStatusHist.CspNumberOblig =
          db.GetNullableString(reader, 11);
        entities.StmtCouponSuppStatusHist.ObgId =
          db.GetNullableInt32(reader, 12);
        entities.StmtCouponSuppStatusHist.DocTypeToSuppress =
          db.GetString(reader, 13);
        entities.StmtCouponSuppStatusHist.Populated = true;
        CheckValid<StmtCouponSuppStatusHist>("CpaType",
          entities.StmtCouponSuppStatusHist.CpaType);
        CheckValid<StmtCouponSuppStatusHist>("Type1",
          entities.StmtCouponSuppStatusHist.Type1);
        CheckValid<StmtCouponSuppStatusHist>("CpaTypeOblig",
          entities.StmtCouponSuppStatusHist.CpaTypeOblig);
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of HiddenSuccessfulDisplay.
    /// </summary>
    [JsonPropertyName("hiddenSuccessfulDisplay")]
    public Common HiddenSuccessfulDisplay
    {
      get => hiddenSuccessfulDisplay ??= new();
      set => hiddenSuccessfulDisplay = value;
    }

    /// <summary>
    /// A value of ObligAmt.
    /// </summary>
    [JsonPropertyName("obligAmt")]
    public Common ObligAmt
    {
      get => obligAmt ??= new();
      set => obligAmt = value;
    }

    /// <summary>
    /// A value of DueDte.
    /// </summary>
    [JsonPropertyName("dueDte")]
    public DateWorkArea DueDte
    {
      get => dueDte ??= new();
      set => dueDte = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public StmtCouponSuppStatusHist HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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

    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Common hiddenSuccessfulDisplay;
    private Common obligAmt;
    private DateWorkArea dueDte;
    private ScreenOwedAmounts screenOwedAmounts;
    private LegalAction legalAction;
    private Standard standard;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ObligationType obligationType;
    private Obligation obligation;
    private Common showHistory;
    private Standard personPrompt;
    private StmtCouponSuppStatusHist hiddenPrev;
    private Array<ImportGroup> import1;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of HiddenSuccessfulDisplay.
    /// </summary>
    [JsonPropertyName("hiddenSuccessfulDisplay")]
    public Common HiddenSuccessfulDisplay
    {
      get => hiddenSuccessfulDisplay ??= new();
      set => hiddenSuccessfulDisplay = value;
    }

    /// <summary>
    /// A value of ObligAmt.
    /// </summary>
    [JsonPropertyName("obligAmt")]
    public Common ObligAmt
    {
      get => obligAmt ??= new();
      set => obligAmt = value;
    }

    /// <summary>
    /// A value of DueDte.
    /// </summary>
    [JsonPropertyName("dueDte")]
    public DateWorkArea DueDte
    {
      get => dueDte ??= new();
      set => dueDte = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public StmtCouponSuppStatusHist HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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

    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Common hiddenSuccessfulDisplay;
    private Common obligAmt;
    private DateWorkArea dueDte;
    private ScreenOwedAmounts screenOwedAmounts;
    private LegalAction legalAction;
    private Standard standard;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ObligationType obligationType;
    private Obligation obligation;
    private Common showHistory;
    private Standard personPrompt;
    private StmtCouponSuppStatusHist hiddenPrev;
    private Common hiddenPrevHistory;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private CsePersonAccount hardcodeObligor;
    private Common common;
    private NextTranInfo nextTranInfo;
    private DateWorkArea max;
    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

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

    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationType obligationType;
    private Obligation obligation;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
