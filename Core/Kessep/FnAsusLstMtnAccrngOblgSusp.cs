// Program: FN_ASUS_LST_MTN_ACCRNG_OBLG_SUSP, ID: 372082431, model: 746.
// Short name: SWEASUSP
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
/// A program: FN_ASUS_LST_MTN_ACCRNG_OBLG_SUSP.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnAsusLstMtnAccrngOblgSusp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASUS_LST_MTN_ACCRNG_OBLG_SUSP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAsusLstMtnAccrngOblgSusp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAsusLstMtnAccrngOblgSusp.
  /// </summary>
  public FnAsusLstMtnAccrngOblgSusp(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------
    // MODIFICATION DOCUMENTATION
    // Date      Developer     Description
    // 12/14/95  Beth Burrell  Initial development
    // 03/12/96  Holly Kennedy	Revamp code to meet new specs, and to cause the 
    // prad to be a
    // 			List/Maintain.
    // 08/20/96  Regan Welborn Little Changes
    // 12/11/96  R. Marchman   Add new security and next tran
    // 04/28/97  SHERAZ MALIK	CHANGE CURRENT DATE
    // 06/08/97  A Samuels  	Set exit state on delete
    // 06/09/97  A Samuels	Fix scrolling indicator
    // 08/13/97  Parker/Newman	Fixed Protection on Update Logic
    // 09/23/97	A Samuels	Added Reduction Amt Field
    // 09/24/97	A Samuels	Problem Report 28174
    // 09/24/97	A Samuels	Problem Report 28177
    // 09/24/97	A Samuels	Problem Report 28178
    // 7/22/99    b adams    Attempt to handle joint and several
    //   obligations was never tested and could never work - wrote
    //   a new CAB; fixed Update cab;
    // 05/19/2000          PR# 90070                Vithal Madhira
    //    Fixing explicit scrolling : Explicit scrolling is implemented by 
    // reading the accruals from database into 'Hidden Group View'. Then move
    // the records from 'hidden_group_view' to the export view to display on the
    // screen.
    // 07/11/2000                PR# 99066              Vithal Madhira
    // Fixed the code to prevent ASUS abends.
    // 09/27/2000               PR# 104032             Vithal Madhira
    // ASUS suspend & Resume date must be within or equal to Accrual start and 
    // Discontinue dates for the OACC HISTORY obligation.
    // Jan., 2002 M. Brown Work Order # 010504 Retro Processing
    // Handle situation where an accrual suspension is updated or deleted such 
    // that it will cause new debts to be created, and the obligation is on a
    // court order that has AF, FC, NF or NC collections (XA).  In this
    // situation, we want to ask the worker if prior payments should be
    // protected, and set up collection protection if the worker answers yes.
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.DateWorkArea.Date = local.Current.Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate1();
    UseFnHardcodedDebtDistribution();

    // ---------------------------------------------
    // Move all IMPORTS TO EXPORTS.
    // ---------------------------------------------
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.FormattedName =
      import.CsePersonsWorkSet.FormattedName;
    export.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    MoveObligationTransaction1(import.ObligationTransaction,
      export.ObligationTransaction);
    MoveObligationType(import.ObligationType, export.ObligationType);
    export.AccrualInstructions.Assign(import.AccrualInstructions);
    export.FrequencyWorkSet.Assign(import.FrequencyWorkSet);
    export.SupportedCsePerson.Number = import.SupportedCsePerson.Number;
    export.SupportedCsePersonsWorkSet.FormattedName =
      import.SupportedCsePersonsWorkSet.FormattedName;
    export.ObligationAmt.TotalCurrency = import.ObligationAmt.TotalCurrency;
    export.Hidden.Assign(import.Hidden);
    MoveObligation(import.Obligation, export.Obligation);
    export.ScrollIndicator.Text4 = import.ScrollIndicator.Text4;
    export.Common.SelectChar = import.Common.SelectChar;

    // Jan., 2002 M. Brown Work Order # 010504 Retro Processing
    export.CollProtection.Flag = import.CollProtection.Flag;
    export.HidCurrentRecord.Count = import.HidCurrentRecord.Count;

    // =================================================
    // 11-17-98 - B Adams  -  CLEAR function must retain on the
    //   screen all the data above this point.
    // =================================================
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";
      export.Common.SelectChar = "";

      var field1 = GetField(export.CollProtAnswer, "selectChar");

      field1.Intensity = Intensity.Dark;
      field1.Protected = true;

      return;
    }

    export.AccrualSuspension.Assign(import.AccrualSuspension);
    export.Common.SelectChar = import.Common.SelectChar;
    export.HiddenFirstTime.Flag = import.HiddenFirstTime.Flag;
    export.HiddenDisplay.Flag = import.HiddenDisplay.Flag;
    export.JointAndSeveralCsePerson.Number = import.ConcurrentCsePerson.Number;
    export.JointAndSeveralObligation.SystemGeneratedIdentifier =
      import.ConcurrentObligation.SystemGeneratedIdentifier;
    export.CountOfPages.Count = import.CountOfPages.Count;

    if (IsEmpty(import.ShowHistory.Flag))
    {
      export.ShowHistory.Flag = "N";
    }
    else
    {
      export.ShowHistory.Flag = import.ShowHistory.Flag;
    }

    if (IsEmpty(import.HiddenShowHistory.Flag))
    {
      export.HiddenShowHistory.Flag = "N";
    }
    else
    {
      export.HiddenShowHistory.Flag = import.HiddenShowHistory.Flag;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      if (Equal(global.Command, "RETURN"))
      {
        goto Test1;
      }

      if (Equal(global.Command, "PROCESS"))
      {
        // ----------------------------------------------------------------------
        // If user adding the first record ,  first move the record to group 
        // view.
        //                                                  
        // Vithal ( 06/09/2000)
        // ----------------------------------------------------------------------
        if (export.Group2.IsEmpty && AsChar(export.Common.SelectChar) == 'A'
          && export.AccrualSuspension.SystemGeneratedIdentifier == 0)
        {
          export.Group2.Index = 0;
          export.Group2.CheckSize();

          export.Group2.Update.HiddenGrpDetail2Common.SelectChar =
            export.Common.SelectChar;
          export.Group2.Update.HiddenGrpDetail2AccrualSuspension.Assign(
            export.AccrualSuspension);

          goto Test1;
        }
      }

      for(import.Group2.Index = 0; import.Group2.Index < import.Group2.Count; ++
        import.Group2.Index)
      {
        if (!import.Group2.CheckSize())
        {
          break;
        }

        export.Group2.Index = import.Group2.Index;
        export.Group2.CheckSize();

        export.Group2.Update.HiddenGrpDetail2Common.SelectChar =
          import.Group2.Item.HiddenGrpDetail2Common.SelectChar;
        export.Group2.Update.HiddenGrpDetail2AccrualSuspension.Assign(
          import.Group2.Item.HiddenGrpDetail2AccrualSuspension);
        export.Group2.Update.HiddenPrev2.Assign(import.Group2.Item.HiddenPrev2);
      }

      import.Group2.CheckIndex();

      // ----------------------------------------------------------------
      // We need to set the ' export_hidden_grp_detail2 ' ief_supplied FLAG to 
      // the 'import' ief_supplied FLAG (ie.  previous "action value". )
      // -----------------------------------------------------------------
      if (!IsEmpty(import.Common.SelectChar))
      {
        // ----------------------------------------------------------------
        // PR# 99066        Vithal Madhira      07/11/2000
        //    If this is the first record to be added on the screen, 
        // Export_hidden_current_record  COUNT will be 0. SET it to 1. This will
        // prevent the ASUS abends.
        // -----------------------------------------------------------------
        if (export.HidCurrentRecord.Count == 0)
        {
          export.HidCurrentRecord.Count = 1;
        }

        export.Group2.Index = export.HidCurrentRecord.Count - 1;
        export.Group2.CheckSize();

        export.Group2.Update.HiddenGrpDetail2Common.SelectChar =
          import.Common.SelectChar;
        export.Group2.Update.HiddenGrpDetail2AccrualSuspension.Assign(
          import.AccrualSuspension);

        // --------------------------------------------------------------------------
        // Clear the 'export' ief_supplied FLAG. So, the previous flag will not 
        // display again on the screen.
        // -------------------------------------------------------------------------
      }
    }

Test1:

    // ---------------------------------------------
    // Next Tran/Security
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      for(export.Group2.Index = 0; export.Group2.Index < export.Group2.Count; ++
        export.Group2.Index)
      {
        if (!export.Group2.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Group2.Item.HiddenGrpDetail2Common.SelectChar))
        {
          var field1 = GetField(export.Standard, "nextTransaction");

          field1.Error = true;

          ExitState = "ACO_NE0000_PROCESS_SCREEN_FIRST";
        }
      }

      export.Group2.CheckIndex();
      export.Hidden.CourtCaseNumber = export.LegalAction.StandardNumber ?? "";
      export.Hidden.CsePersonNumberAp = export.CsePerson.Number;
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      export.Hidden.StandardCrtOrdNumber =
        export.LegalAction.StandardNumber ?? "";
      export.Hidden.ObligationId = export.Obligation.SystemGeneratedIdentifier;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;
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
      if (!IsEmpty(export.Hidden.CsePersonNumberAp))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumberAp ?? Spaces(10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }

      if (!IsEmpty(export.Hidden.StandardCrtOrdNumber))
      {
        export.LegalAction.StandardNumber =
          export.Hidden.StandardCrtOrdNumber ?? "";
      }
      else
      {
        export.LegalAction.StandardNumber = export.Hidden.CourtOrderNumber ?? ""
          ;
      }

      export.Obligation.SystemGeneratedIdentifier =
        export.Hidden.ObligationId.GetValueOrDefault();
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // ---------------------------------------------
    // Initialize show history flag to "N" first time
    // thru or on REFRESH
    // ---------------------------------------------
    if (IsEmpty(export.ShowHistory.Flag))
    {
      export.ShowHistory.Flag = "N";
    }

    if (IsEmpty(export.HiddenShowHistory.Flag))
    {
      export.HiddenShowHistory.Flag = "N";
    }

    if (AsChar(export.HiddenShowHistory.Flag) == 'N' && AsChar
      (export.ShowHistory.Flag) == 'Y')
    {
      // ---------------------------------------------------------------------------------
      // User wants to display the HISTORY records. SET the 'hidden_first_time' 
      // FLAG to ' Y '  so that records will be READ from the database again.
      // This will REFRESH the screen.
      // --------------------------------------------------------------------------------
      export.HiddenShowHistory.Flag = export.ShowHistory.Flag;
      export.HiddenFirstTime.Flag = "Y";
    }

    // ---------------------------------------------
    // Check to see if a selection has been made.
    // Assume no selection first time thru.
    // ---------------------------------------------
    local.SelectChar.Count = 0;
    local.SelectChar.Flag = "";

    if (Equal(global.Command, "PROCESS"))
    {
      // ---------------------------------------------
      // Validate entry in the show history field.
      // ---------------------------------------------
      if (AsChar(export.ShowHistory.Flag) == 'Y' || AsChar
        (export.ShowHistory.Flag) == 'N')
      {
        // *** valid value entered
      }
      else
      {
        var field1 = GetField(export.ShowHistory, "flag");

        field1.Error = true;

        ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

        return;
      }

      for(export.Group2.Index = 0; export.Group2.Index < export.Group2.Count; ++
        export.Group2.Index)
      {
        if (!export.Group2.CheckSize())
        {
          break;
        }

        switch(AsChar(export.Group2.Item.HiddenGrpDetail2Common.SelectChar))
        {
          case ' ':
            break;
          case 'A':
            if (local.DeleteCount.Count > 0 || local.UpdateCount.Count > 0)
            {
              ExitState = "ADD_NOT_ALLOWED_WITH_DEL_OR_UPD";
            }

            if (Equal(export.AccrualSuspension.SuspendDt,
              export.Group2.Item.HiddenPrev2.SuspendDt) && Equal
              (export.AccrualSuspension.ResumeDt,
              export.Group2.Item.HiddenPrev2.ResumeDt) && export
              .AccrualSuspension.ReductionPercentage.GetValueOrDefault() == export
              .Group2.Item.HiddenPrev2.ReductionPercentage.
                GetValueOrDefault() && export
              .AccrualSuspension.ReductionAmount.GetValueOrDefault() == export
              .Group2.Item.HiddenPrev2.ReductionAmount.GetValueOrDefault() && Equal
              (export.AccrualSuspension.ReasonTxt,
              export.Group2.Item.HiddenPrev2.ReasonTxt))
            {
              export.Common.SelectChar =
                export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
              ExitState = "SP0000_DATA_NOT_CHANGED";

              var field2 = GetField(export.Common, "selectChar");

              field2.Error = true;

              goto Test2;
            }

            ++local.AddCount.Count;

            break;
          case 'C':
            if (local.DeleteCount.Count > 0 || local.AddCount.Count > 0)
            {
              ExitState = "UPD_NOT_ALLOWED_WITH_ADD_OR_DEL";
            }

            if (Equal(export.AccrualSuspension.SuspendDt,
              export.Group2.Item.HiddenPrev2.SuspendDt) && Equal
              (export.AccrualSuspension.ResumeDt,
              export.Group2.Item.HiddenPrev2.ResumeDt) && export
              .AccrualSuspension.ReductionPercentage.GetValueOrDefault() == export
              .Group2.Item.HiddenPrev2.ReductionPercentage.
                GetValueOrDefault() && export
              .AccrualSuspension.ReductionAmount.GetValueOrDefault() == export
              .Group2.Item.HiddenPrev2.ReductionAmount.GetValueOrDefault() && Equal
              (export.AccrualSuspension.ReasonTxt,
              export.Group2.Item.HiddenPrev2.ReasonTxt))
            {
              export.Common.SelectChar =
                export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
              ExitState = "FN0000_NO_CHANGE_TO_UPDATE";

              var field2 = GetField(export.Common, "selectChar");

              field2.Error = true;

              goto Test2;
            }

            if (Equal(export.AccrualSuspension.SuspendDt,
              export.Group2.Item.HiddenPrev2.SuspendDt) && Equal
              (export.AccrualSuspension.ResumeDt,
              export.Group2.Item.HiddenPrev2.ResumeDt))
            {
              local.DatesChanged.Flag = "N";
            }

            ++local.UpdateCount.Count;

            break;
          case 'D':
            if (local.UpdateCount.Count > 0 || local.AddCount.Count > 0)
            {
              ExitState = "DEL_NOT_ALLOWED_WITH_ADD_OR_UPD";
            }

            ++local.DeleteCount.Count;

            break;
          default:
            ExitState = "FN0000_INVALID_ACTION_CODE";
            export.AccrualSuspension.Assign(
              export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
            export.Common.SelectChar =
              export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

            var field1 = GetField(export.Common, "selectChar");

            field1.Error = true;

            break;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          goto Test2;
        }
      }

      export.Group2.CheckIndex();

      if (export.Group2.IsEmpty)
      {
        ExitState = "FN0000_INVALID_ACTION_CODE";

        goto Test2;
      }

      // ================================================
      // 1/4/99  -  B Adams  -  If all of them are spaces and PROCESS
      //   was pressed, then go no further.
      // =================================================
      if (local.UpdateCount.Count == 0 && local.AddCount.Count == 0 && local
        .DeleteCount.Count == 0 && Equal(global.Command, "PROCESS"))
      {
        ExitState = "FN0000_PROCESS_NO_FUNCTION";
      }
    }

Test2:

    if (!Equal(global.Command, "COLP"))
    {
      UseScCabTestSecurity();
    }

    // ---------------------------------------------
    //            Main CASE of COMMAND
    // ---------------------------------------------
    local.Error.Flag = "N";
    export.HiddenFirstTime.Flag = "Y";

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      switch(TrimEnd(global.Command))
      {
        case "DISPLAY":
          break;
        case "RETURN":
          ExitState = "ACO_NE0000_RETURN";

          break;
        case "PREV":
          if (export.HidCurrentRecord.Count <= 1)
          {
            ExitState = "ACO_NI0000_TOP_OF_LIST";
          }
          else
          {
            // ------------------------------------------------------------------------
            // Each record needs to be processed before scrolling to previous 
            // record. If the record has not processed, Clear the 'Select Char'.
            // --------------------------------------------------------------------------
            export.Group2.Index = export.HidCurrentRecord.Count - 1;
            export.Group2.CheckSize();

            if (!IsEmpty(export.Group2.Item.HiddenGrpDetail2Common.SelectChar))
            {
              // ------------------------------------------------------------------------
              // Current record is not processed. User cleared the 'Select Char'
              // and hit PF7. So populate the record from 'Hid_Prev' views to '
              // Hid' view and clear the 'Hid' Select Char field.
              // --------------------------------------------------------------------------
              export.Group2.Update.HiddenGrpDetail2Common.SelectChar = "";
              MoveAccrualSuspension3(export.Group2.Item.HiddenPrev2,
                export.Group2.Update.HiddenGrpDetail2AccrualSuspension);
            }

            export.HidCurrentRecord.Count =
              (int)((long)export.HidCurrentRecord.Count - 1);

            export.Group2.Index = export.HidCurrentRecord.Count - 1;
            export.Group2.CheckSize();

            export.AccrualSuspension.Assign(
              export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
            export.Common.SelectChar =
              export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
          }

          break;
        case "NEXT":
          if (export.HidCurrentRecord.Count == export.Group2.Count)
          {
            ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
          }
          else
          {
            // ------------------------------------------------------------------------
            // Each record needs to be processed before scrolling to Next 
            // record. If the record has not processed, Clear the 'Select Char'.
            // --------------------------------------------------------------------------
            export.Group2.Index = export.HidCurrentRecord.Count - 1;
            export.Group2.CheckSize();

            if (!IsEmpty(export.Group2.Item.HiddenGrpDetail2Common.SelectChar))
            {
              // ------------------------------------------------------------------------
              // Current record is not processed. User cleared the 'Select Char'
              // and hit PF8. So populate the record from 'Hid_Prev' views to '
              // Hid' view and clear the 'Hid' Select Char field.
              // --------------------------------------------------------------------------
              export.Group2.Update.HiddenGrpDetail2Common.SelectChar = "";
              MoveAccrualSuspension3(export.Group2.Item.HiddenPrev2,
                export.Group2.Update.HiddenGrpDetail2AccrualSuspension);
            }

            export.HidCurrentRecord.Count =
              (int)((long)export.HidCurrentRecord.Count + 1);

            export.Group2.Index = export.HidCurrentRecord.Count - 1;
            export.Group2.CheckSize();

            export.AccrualSuspension.Assign(
              export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
            export.Common.SelectChar =
              export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
          }

          break;
        case "SIGNOFF":
          UseScCabSignoff();

          break;
        case "PROCESS":
          local.FirstProcessRecord.Count = 0;
          export.Group2.Index = 0;

          for(var limit = export.Group2.Count; export.Group2.Index < limit; ++
            export.Group2.Index)
          {
            if (!export.Group2.CheckSize())
            {
              break;
            }

            export.HidCurrentRecord.Count = export.Group2.Index + 1;

            // ***--- b adams  -  this counter is used to determine which row, 
            // if any, has the first error on it.
            ++local.GroupRecord.Count;

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // =================================================
              // 12/29/98 - B Adams  -  Data has already been moved earlier
              //   in the flow.  Also, an escape prior to this will leave the
              //   export group view empty
              // =================================================
              break;
            }

            if (AsChar(export.HiddenDisplay.Flag) != 'Y' && (
              AsChar(import.Group2.Item.HiddenGrpDetail2Common.SelectChar) == 'C'
              || AsChar
              (import.Group2.Item.HiddenGrpDetail2Common.SelectChar) == 'D'))
            {
              export.AccrualSuspension.Assign(
                export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
              export.Common.SelectChar =
                export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
              ExitState = "FN0000_DISPLAY_BEFORE_CHG_DEL";

              break;
            }

            if (export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
              SystemGeneratedIdentifier == 0 && (
                AsChar(export.Group2.Item.HiddenGrpDetail2Common.SelectChar) ==
                'C' || AsChar
              (export.Group2.Item.HiddenGrpDetail2Common.SelectChar) == 'D'))
            {
              export.AccrualSuspension.Assign(
                export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
              export.Common.SelectChar =
                export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
              ExitState = "ACO_NE0000_ADD_THE_RECORD_FIRST";

              var field1 = GetField(export.Common, "selectChar");

              field1.Error = true;

              break;
            }

            if (AsChar(export.Group2.Item.HiddenGrpDetail2Common.SelectChar) ==
              'A' || AsChar
              (export.Group2.Item.HiddenGrpDetail2Common.SelectChar) == 'C')
            {
              // **** Edit checks ****
              if (Equal(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                SuspendDt, local.Null1.SuspendDt))
              {
                local.Error.Flag = "Y";
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 = GetField(export.AccrualSuspension, "suspendDt");

                field1.Error = true;

                ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
              }

              if (Equal(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                SuspendDt,
                export.Group2.Item.HiddenGrpDetail2AccrualSuspension.ResumeDt))
              {
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 = GetField(export.AccrualSuspension, "suspendDt");

                field1.Error = true;

                var field2 = GetField(export.AccrualSuspension, "resumeDt");

                field2.Error = true;

                ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

                break;
              }

              if (IsEmpty(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                ReasonTxt))
              {
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 = GetField(export.AccrualSuspension, "reasonTxt");

                field1.Error = true;

                ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
              }

              if (Lt(export.AccrualInstructions.DiscontinueDt,
                export.Group2.Item.HiddenGrpDetail2AccrualSuspension.ResumeDt))
              {
                var field1 = GetField(export.AccrualSuspension, "resumeDt");

                field1.Error = true;

                ExitState = "FN0000_RESUME_DT_AFTER_DISCNT_DT";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }

              if (export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                ReductionPercentage.GetValueOrDefault() == 0 && export
                .Group2.Item.HiddenGrpDetail2AccrualSuspension.ReductionAmount.
                  GetValueOrDefault() == 0)
              {
                local.Error.Flag = "Y";
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 =
                  GetField(export.AccrualSuspension, "reductionAmount");

                field1.Error = true;

                var field2 =
                  GetField(export.AccrualSuspension, "reductionPercentage");

                field2.Error = true;

                ExitState = "FN0000_REDUCTION_PERCENT_OR_AMT";

                break;
              }

              if (export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                ReductionPercentage.GetValueOrDefault() > 0 && export
                .Group2.Item.HiddenGrpDetail2AccrualSuspension.ReductionAmount.
                  GetValueOrDefault() > 0)
              {
                local.Error.Flag = "Y";
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 =
                  GetField(export.AccrualSuspension, "reductionAmount");

                field1.Error = true;

                var field2 =
                  GetField(export.AccrualSuspension, "reductionPercentage");

                field2.Error = true;

                ExitState = "FN0000_REDUCTN_PERCNT_OR_AMT";

                break;
              }

              if (export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                ReductionPercentage.GetValueOrDefault() > 100)
              {
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 =
                  GetField(export.AccrualSuspension, "reductionPercentage");

                field1.Error = true;

                ExitState = "FN0000_TOTAL_PERCENTAGE_GT_100";

                break;
              }

              if (export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                ReductionAmount.GetValueOrDefault() > export
                .ObligationTransaction.Amount)
              {
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 =
                  GetField(export.AccrualSuspension, "reductionAmount");

                field1.Error = true;

                ExitState = "FN0000_ADJ_AMT_GT_ACCRL_AMT";

                break;
              }

              if (AsChar(export.Group2.Item.HiddenGrpDetail2Common.SelectChar) ==
                'A')
              {
                export.AccrualSuspension.CreatedTmst = local.Current.Timestamp;
              }

              // =================================================
              // 4/20/99 - b adams  -  This situation is never OK
              // =================================================
              if (Lt(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                SuspendDt, export.AccrualInstructions.AsOfDt))
              {
                ExitState = "FN0000_SUSPEND_DT_TOO_EARLY";
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 = GetField(export.AccrualSuspension, "suspendDt");

                field1.Error = true;

                break;
              }

              // =================================================
              // 4/20/99 - bud adams  -  IF tests was not allowing historical
              //   accrual suspensions to be entered for back-dated debts.
              // =================================================
              if (Equal(export.AccrualInstructions.LastAccrualDt,
                local.Zero.Date))
              {
              }
              else
              {
                if (!Lt(export.AccrualInstructions.LastAccrualDt,
                  export.AccrualSuspension.SuspendDt) && Equal
                  (export.AccrualSuspension.ResumeDt, local.Zero.Date) && AsChar
                  (export.Common.SelectChar) == 'A')
                {
                  local.Error.Flag = "Y";
                  export.AccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                  export.Common.SelectChar =
                    export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                  var field1 = GetField(export.AccrualSuspension, "suspendDt");

                  field1.Error = true;

                  ExitState = "FN0000_SUSPEND_DT_B4_LST_ACCRUAL";

                  break;
                }

                if (!Equal(export.AccrualSuspension.ResumeDt,
                  export.Group2.Item.HiddenPrev2.ResumeDt))
                {
                  // ------------------------------------------------------------------------------
                  // This must be executed only if the RESUME DATE is updated.
                  //                                                   
                  // --- Vithal Madhira.
                  // ------------------------------------------------------------------------------
                  if (!Lt(export.AccrualInstructions.LastAccrualDt,
                    export.AccrualSuspension.ResumeDt) && AsChar
                    (export.Group2.Item.HiddenGrpDetail2Common.SelectChar) == 'C'
                    && Lt(local.Zero.Date, export.AccrualSuspension.ResumeDt))
                  {
                    local.Error.Flag = "Y";
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field1 = GetField(export.AccrualSuspension, "resumeDt");

                    field1.Error = true;

                    export.Common.SelectChar = "C";
                    ExitState = "FN0000_RESUME_DT_B4_LST_ACCRUAL";

                    break;
                  }
                }
              }

              if (Lt(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                ResumeDt,
                export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                  SuspendDt) && !
                Equal(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                  ResumeDt, local.Null1.SuspendDt))
              {
                local.Error.Flag = "Y";
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 = GetField(export.AccrualSuspension, "resumeDt");

                field1.Error = true;

                var field2 = GetField(export.AccrualSuspension, "suspendDt");

                field2.Error = true;

                ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

                break;
              }
            }

            if (!IsEmpty(export.Group2.Item.HiddenGrpDetail2Common.SelectChar) &&
              local.FirstProcessRecord.Count == 0)
            {
              // --------------------------------------------------------------------------
              // Store the subscript of the first record to be processed. If the
              // processing is successful  display the first processed record
              // on the screen.
              // -------------------------------------------------------------------------------
              local.FirstProcessRecord.Count = export.Group2.Index + 1;
            }

            if (AsChar(export.Group2.Item.HiddenGrpDetail2Common.SelectChar) ==
              'A' || AsChar
              (export.Group2.Item.HiddenGrpDetail2Common.SelectChar) == 'C')
            {
              // ----------------------------------------------------------------------------------
              // 09/27/2000               PR# 104032             Vithal Madhira
              // ASUS suspend & Resume date must be within or equal to Accrual 
              // start and Discontinue dates for the OACC HISTORY obligation.
              // -------------------------------------------------------------------------------------
              if (ReadObligationPaymentScheduleObligation())
              {
                if (AsChar(entities.Obligation.HistoryInd) == 'Y')
                {
                  if (Lt(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                    SuspendDt, entities.ObligationPaymentSchedule.StartDt) || Lt
                    (entities.ObligationPaymentSchedule.EndDt,
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                      SuspendDt))
                  {
                    var field1 =
                      GetField(export.AccrualSuspension, "suspendDt");

                    field1.Error = true;

                    var field2 = GetField(export.AccrualSuspension, "resumeDt");

                    field2.Error = true;

                    ExitState = "FN0000_ASUS_DATE_ERROR";

                    break;
                  }

                  if (Lt(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                    ResumeDt, entities.ObligationPaymentSchedule.StartDt) || Lt
                    (AddDays(entities.ObligationPaymentSchedule.EndDt, 1),
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                      ResumeDt))
                  {
                    var field1 =
                      GetField(export.AccrualSuspension, "suspendDt");

                    field1.Error = true;

                    var field2 = GetField(export.AccrualSuspension, "resumeDt");

                    field2.Error = true;

                    ExitState = "FN0000_ASUS_DATE_ERROR";

                    break;
                  }
                }
              }
              else
              {
                ExitState = "FN0000_OBLIGATION_NF";

                break;
              }
            }

            switch(AsChar(export.Group2.Item.HiddenGrpDetail2Common.SelectChar))
            {
              case ' ':
                break;
              case 'A':
                // =================================================
                // 11/17/98 - B Adams  -  If the resume date has not been 
                // entered
                //   then the default value is max date.  It is not an error.
                // =================================================
                if (Equal(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                  ResumeDt, local.Null1.SuspendDt))
                {
                  export.Group2.Update.HiddenGrpDetail2AccrualSuspension.
                    ResumeDt = local.Max.Date;
                }

                export.Group2.Update.HiddenGrpDetail2AccrualSuspension.
                  CreatedTmst = local.Current.Timestamp;
                export.Group2.Update.CreateDate2.Date = local.Current.Date;

                // *****
                // Add the Suspension
                // *****
                UseFnAddAccrualSuspension2();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  local.Error.Flag = "Y";

                  if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
                  {
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field2 = GetField(export.AccrualSuspension, "resumeDt");

                    field2.Error = true;

                    var field3 =
                      GetField(export.AccrualSuspension, "suspendDt");

                    field3.Error = true;
                  }

                  if (IsExitState("FN0000_ACCRUAL_SUS_DATE_LESS_LST"))
                  {
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field2 =
                      GetField(export.AccrualSuspension, "suspendDt");

                    field2.Error = true;
                  }

                  if (IsExitState("FN0000_SUSP_DT_AFTER_DISCNT_DATE"))
                  {
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field2 =
                      GetField(export.AccrualSuspension, "suspendDt");

                    field2.Error = true;
                  }

                  continue;
                }

                // *****
                // If Concurrent obligation exists perform same processing to 
                // it.
                // *****
                if (AsChar(export.Obligation.PrimarySecondaryCode) == AsChar
                  (local.HcObligJointAndSeveral.PrimarySecondaryCode))
                {
                  UseFnReadConcurrentObligation();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field2 = GetField(export.Common, "selectChar");

                    field2.Error = true;

                    local.Error.Flag = "Y";
                    local.RollbackRequired.Flag = "Y";

                    continue;
                  }

                  UseFnAddAccrualSuspension1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field2 = GetField(export.Common, "selectChar");

                    field2.Error = true;

                    local.Error.Flag = "Y";
                    local.RollbackRequired.Flag = "Y";

                    continue;
                  }
                }

                local.ConditionalMaximum.Date =
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension.ResumeDt;
                  
                export.Group2.Update.HiddenGrpDetail2Common.SelectChar = "";
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);

                if (Equal(export.AccrualSuspension.ResumeDt, new DateTime(2099,
                  12, 31)))
                {
                  export.AccrualSuspension.ResumeDt = local.Null1.ResumeDt;
                }

                export.Group2.Update.HiddenPrev2.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
                export.HiddenDisplay.Flag = "Y";

                // =================================================
                // 12/10/98 - B Adams  -  Users want to see blanks if the date
                //   is 'max' date (i.e. indefinite)
                // =================================================
                if (Equal(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                  ResumeDt, local.Max.Date))
                {
                  export.Group2.Update.HiddenGrpDetail2AccrualSuspension.
                    ResumeDt = local.Zero.Date;
                  export.Group2.Update.HiddenPrev2.ResumeDt = local.Zero.Date;
                  export.AccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                }

                global.Command = "ADD";

                break;
              case 'C':
                // *****
                // Update the Suspension
                // *****
                if (AsChar(export.HiddenDisplay.Flag) != 'Y')
                {
                  ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
                  export.AccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                  export.Common.SelectChar =
                    export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                  var field2 = GetField(export.Common, "selectChar");

                  field2.Error = true;

                  continue;
                }

                if (Equal(export.AccrualSuspension.SuspendDt,
                  export.Group2.Item.HiddenPrev2.SuspendDt) && Equal
                  (export.AccrualSuspension.ResumeDt,
                  export.Group2.Item.HiddenPrev2.ResumeDt))
                {
                  local.DatesChanged.Flag = "N";
                }
                else
                {
                  local.DatesChanged.Flag = "Y";
                }

                if (Equal(export.AccrualSuspension.ResumeDt,
                  local.Null1.SuspendDt))
                {
                  export.AccrualSuspension.ResumeDt = local.Max.Date;
                }

                UseFnUpdateAccrualSuspension2();

                if (Equal(export.AccrualSuspension.ResumeDt, local.Max.Date))
                {
                  export.AccrualSuspension.ResumeDt = local.Null1.ResumeDt;
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // Jan., 2002 M. Brown Work Order # 010504 Retro Processing
                  //  Set export coll protect question literal and answer char 
                  // fields to space.
                  //  If the exitstate is to check to see if collections should 
                  // be protected,
                  //  these fields are set again below.
                  export.ProtectQuestionLiteral.Text80 = "";
                  export.CollProtAnswer.SelectChar = "";

                  if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
                  {
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);

                    if (Equal(export.AccrualSuspension.ResumeDt, local.Max.Date))
                      
                    {
                      export.AccrualSuspension.ResumeDt = local.Null1.ResumeDt;
                    }

                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field3 = GetField(export.AccrualSuspension, "resumeDt");

                    field3.Error = true;

                    var field4 =
                      GetField(export.AccrualSuspension, "suspendDt");

                    field4.Error = true;
                  }

                  if (IsExitState("FN0000_SUSP_DT_AFTER_DISCNT_DATE"))
                  {
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field3 =
                      GetField(export.AccrualSuspension, "suspendDt");

                    field3.Error = true;
                  }

                  // Jan., 2002 M. Brown Work Order # 010504 Retro Processing
                  if (IsExitState("FN0000_Y_OR_N_THEN_PF15"))
                  {
                    var field3 = GetField(export.CollProtAnswer, "selectChar");

                    field3.Highlighting = Highlighting.Underscore;
                    field3.Protected = false;
                    field3.Focused = true;

                    export.ProtectQuestionLiteral.Text80 =
                      "State retained collections exist: protect prior payments?";
                      
                    export.Common.SelectChar = import.Common.SelectChar;

                    var field4 = GetField(export.AccrualSuspension, "resumeDt");

                    field4.Color = "cyan";
                    field4.Protected = true;

                    var field5 =
                      GetField(export.AccrualSuspension, "suspendDt");

                    field5.Color = "cyan";
                    field5.Protected = true;

                    var field6 =
                      GetField(export.AccrualSuspension, "reductionPercentage");
                      

                    field6.Color = "cyan";
                    field6.Protected = true;

                    var field7 =
                      GetField(export.AccrualSuspension, "reductionAmount");

                    field7.Color = "cyan";
                    field7.Protected = true;

                    var field8 = GetField(export.Common, "selectChar");

                    field8.Color = "cyan";
                    field8.Protected = true;

                    return;
                  }

                  export.AccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);

                  if (Equal(export.AccrualSuspension.ResumeDt, local.Max.Date))
                  {
                    export.AccrualSuspension.ResumeDt = local.Null1.ResumeDt;
                  }

                  export.Common.SelectChar =
                    export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                  var field2 = GetField(export.Common, "selectChar");

                  field2.Error = true;

                  local.Error.Flag = "Y";

                  continue;
                }

                // *****
                // If Concurrent obligation exists perform same processing to 
                // it.
                // *****
                if (AsChar(export.Obligation.PrimarySecondaryCode) == AsChar
                  (local.HcObligJointAndSeveral.PrimarySecondaryCode))
                {
                  UseFnReadConcurrentObligation();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field2 = GetField(export.Common, "selectChar");

                    field2.Error = true;

                    local.Error.Flag = "Y";
                    local.RollbackRequired.Flag = "Y";

                    continue;
                  }

                  local.JointAndSeveralAccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                  local.ConditionalMaximum.Date =
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                      ResumeDt;
                  local.JointAndSeveralAccrualSuspension.ResumeDt =
                    UseCabSetMaximumDiscontinueDate2();

                  if (ReadAccrualSuspension2())
                  {
                    local.JointAndSeveralAccrualSuspension.
                      SystemGeneratedIdentifier =
                        entities.JointAndSeveralAccrualSuspension.
                        SystemGeneratedIdentifier;
                  }
                  else
                  {
                    ExitState = "FN0000_CONCURRNT_ACCRUAL_SUSP_NF";
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field2 = GetField(export.Common, "selectChar");

                    field2.Error = true;

                    local.Error.Flag = "Y";
                    local.RollbackRequired.Flag = "Y";

                    continue;
                  }

                  UseFnUpdateAccrualSuspension1();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field2 = GetField(export.Common, "selectChar");

                    field2.Error = true;

                    local.Error.Flag = "Y";
                    local.RollbackRequired.Flag = "Y";

                    continue;
                  }
                }

                local.ConditionalMaximum.Date =
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension.ResumeDt;
                  
                export.Group2.Update.HiddenGrpDetail2AccrualSuspension.
                  ResumeDt = UseCabSetMaximumDiscontinueDate2();
                export.Group2.Update.HiddenGrpDetail2Common.SelectChar = "";
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);

                if (Equal(export.AccrualSuspension.ResumeDt, new DateTime(2099,
                  12, 31)))
                {
                  export.AccrualSuspension.ResumeDt = local.Null1.ResumeDt;
                }

                export.Group2.Update.HiddenPrev2.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
                export.HiddenDisplay.Flag = "Y";

                // Jan., 2002 M. Brown Work Order # 010504 Retro Processing
                // : Set the protection confirm message and answer fields to 
                // spaces.
                export.ProtectQuestionLiteral.Text80 = "";
                export.CollProtAnswer.SelectChar = "";
                global.Command = "UPDATE";

                break;
              case 'D':
                // *****
                // Delete the Suspension
                // *****
                if (AsChar(export.HiddenDisplay.Flag) != 'Y')
                {
                  ExitState = "FN0000_DISPLAY_ACS_BEFORE_DELETE";

                  continue;
                }

                if (!Lt(export.AccrualInstructions.LastAccrualDt,
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
                    SuspendDt))
                {
                  export.AccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                  export.Common.SelectChar =
                    export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                  var field2 = GetField(export.Common, "selectChar");

                  field2.Error = true;

                  ExitState = "FN0000_SUSP_DATE_LT_LAST_ACCR_DT";

                  continue;
                }

                if (!ReadObligationCsePersonCsePersonAccount())
                {
                  ExitState = "FN0000_OBLIGATION_NF";
                  export.AccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                  export.Common.SelectChar =
                    export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                  var field2 = GetField(export.Common, "selectChar");

                  field2.Error = true;

                  continue;
                }

                if (!ReadAccrualInstructionsObligationTransaction())
                {
                  ExitState = "OBLIGATION_TRANSACTION_NF";
                  export.AccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                  export.Common.SelectChar =
                    export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                  var field2 = GetField(export.Common, "selectChar");

                  field2.Error = true;

                  continue;
                }

                if (ReadAccrualSuspension1())
                {
                  if (Lt(entities.AccrualInstructions.LastAccrualDt,
                    entities.AccrualSuspension.SuspendDt))
                  {
                  }
                  else
                  {
                    ExitState = "FN0000_CANT_DEL_SUSP_DT_ACRUL_DT";
                    export.AccrualSuspension.Assign(
                      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                    export.Common.SelectChar =
                      export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                    var field2 =
                      GetField(export.AccrualSuspension, "suspendDt");

                    field2.Error = true;

                    continue;
                  }

                  local.Deleted.SuspendDt =
                    entities.AccrualSuspension.SuspendDt;

                  // : Jan, 2002, M. Brown, Retro Processing, WO#010504.
                  //   Check to see if this is an 'XA' (arrears-only obligation,
                  // with collections
                  //   applied to AF, NF, FC or NC).  If so we will check with 
                  // the user to see if they
                  //   want collections protected.
                  if (IsEmpty(import.CollProtAnswer.SelectChar))
                  {
                    if (Lt(import.AccrualSuspension.SuspendDt, Now().Date))
                    {
                      if (ReadCollection())
                      {
                        var field2 =
                          GetField(export.CollProtAnswer, "selectChar");

                        field2.Highlighting = Highlighting.Underscore;
                        field2.Protected = false;
                        field2.Focused = true;

                        export.ProtectQuestionLiteral.Text80 =
                          "State retained collections exist: protect prior payments?";
                          
                        export.Common.SelectChar = import.Common.SelectChar;
                        ExitState = "FN0000_Y_OR_N_THEN_PF15";

                        return;
                      }
                      else
                      {
                        // : OK
                      }
                    }
                  }
                  else if (AsChar(import.CollProtAnswer.SelectChar) == 'Y' || AsChar
                    (import.CollProtAnswer.SelectChar) == 'N')
                  {
                    // : OK, continue
                  }
                  else
                  {
                    // A character other than 'Y' or 'N' was entered.
                    var field2 = GetField(export.CollProtAnswer, "selectChar");

                    field2.Highlighting = Highlighting.Underscore;
                    field2.Protected = false;
                    field2.Focused = true;

                    export.ProtectQuestionLiteral.Text80 =
                      "State retained collections exist: protect prior payments?";
                      
                    ExitState = "FN0000_Y_OR_N_THEN_PF15";

                    return;
                  }

                  // *****
                  // If Suspension is active delete it.
                  // *****
                  DeleteAccrualSuspension1();

                  if (AsChar(import.CollProtAnswer.SelectChar) == 'Y')
                  {
                    // : Jan, 2002, M. Brown, Retro Processing, WO#010504.
                    //    The user has confirmed that collections should be 
                    // protected.
                    local.ObligCollProtectionHist.ReasonText =
                      "Accrual Suspension deleted.";
                    local.ObligCollProtectionHist.CvrdCollStrtDt =
                      import.AccrualSuspension.SuspendDt;
                    local.ObligCollProtectionHist.CvrdCollEndDt =
                      local.Current.Date;

                    foreach(var item in ReadObligation2())
                    {
                      UseFnProtectCollectionsForOblig();

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        return;
                      }
                    }
                  }

                  if (AsChar(export.Obligation.PrimarySecondaryCode) == AsChar
                    (local.HcObligJointAndSeveral.PrimarySecondaryCode))
                  {
                    UseFnReadConcurrentObligation();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      export.AccrualSuspension.Assign(
                        export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                      export.Common.SelectChar =
                        export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                      var field2 = GetField(export.Common, "selectChar");

                      field2.Error = true;

                      local.RollbackRequired.Flag = "Y";

                      continue;
                    }

                    if (!ReadAccrualInstructions2())
                    {
                      ExitState = "FN0000_CONCURRENT_OBLIG_TRANS_NF";
                      export.AccrualSuspension.Assign(
                        export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                      export.Common.SelectChar =
                        export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                      var field2 = GetField(export.Common, "selectChar");

                      field2.Error = true;

                      local.RollbackRequired.Flag = "Y";

                      continue;
                    }

                    if (ReadAccrualSuspension3())
                    {
                      DeleteAccrualSuspension2();
                    }
                    else
                    {
                      export.AccrualSuspension.Assign(
                        export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                      export.Common.SelectChar =
                        export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                      var field2 = GetField(export.Common, "selectChar");

                      field2.Error = true;

                      ExitState = "FN0000_CONCURRNT_ACCRUAL_SUSP_NF";
                      local.Error.Flag = "Y";
                      local.RollbackRequired.Flag = "Y";

                      continue;
                    }
                  }

                  // =================================================
                  // 12/29/98 - b adams  -  If one is deleted, keep the "D" but 
                  // get
                  //   rid of the data.
                  // =================================================
                  MoveAccrualSuspension4(local.InitializeAccrualSuspension,
                    export.Group2.Update.HiddenPrev2);
                  MoveAccrualSuspension2(local.InitializeAccrualSuspension,
                    export.Group2.Update.HiddenGrpDetail2AccrualSuspension);
                  export.AccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                }
                else
                {
                  ExitState = "FN0000_ACCRUAL_SUSPENSION_NF";
                  export.AccrualSuspension.Assign(
                    export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                  export.Common.SelectChar =
                    export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                  var field2 = GetField(export.Common, "selectChar");

                  field2.Error = true;

                  local.Error.Flag = "Y";

                  continue;
                }

                export.Group2.Update.HiddenGrpDetail2Common.SelectChar = "";
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
                global.Command = "REDISP";

                break;
              default:
                export.AccrualSuspension.Assign(
                  export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
                export.Common.SelectChar =
                  export.Group2.Item.HiddenGrpDetail2Common.SelectChar;

                var field1 = GetField(export.Common, "selectChar");

                field1.Error = true;

                ExitState = "FN0000_INVALID_ACTION_CODE";
                local.Error.Flag = "Y";

                continue;
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.HiddenFirstTime.Flag = "Y";
            }

            // ***--- b adams  -  establish the record to display to the user if
            // there's an error.
            if (!IsExitState("ACO_NN0000_ALL_OK") && local
              .GroupRecord.Count == 0)
            {
              local.Error.Count = local.GroupRecord.Count;
            }
          }

          export.Group2.CheckIndex();

          break;
        case "COLP":
          ExitState = "ECO_LNK_TO_COLP";

          return;
        default:
          ExitState = "ACO_NE0000_INVALID_COMMAND";

          break;
      }

      if (Equal(global.Command, "REDISP") || Equal
        (global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
        (global.Command, "UPDATE"))
      {
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test4;
        }

        if (Equal(global.Command, "REDISP"))
        {
          // **** Clean up the export group view ****
          for(export.Group2.Index = 0; export.Group2.Index < export
            .Group2.Count; ++export.Group2.Index)
          {
            if (!export.Group2.CheckSize())
            {
              break;
            }

            MoveAccrualSuspension2(local.InitializeAccrualSuspension,
              export.Group2.Update.HiddenGrpDetail2AccrualSuspension);
            MoveAccrualSuspension4(local.InitializeAccrualSuspension,
              export.Group2.Update.HiddenPrev2);
            export.Group2.Update.HiddenGrpDetail2Common.SelectChar =
              local.InitializeCommon.SelectChar;
          }

          export.Group2.CheckIndex();
        }

        // **** display the group  *****
        // ------------------------------------------------------------
        // Populate screen header obligation information
        // ------------------------------------------------------------
        if (ReadObligation1())
        {
          MoveObligation(entities.Obligation, export.Obligation);
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF";

          goto Test4;
        }

        if (ReadObligationTransaction1())
        {
          MoveObligationTransaction1(entities.ObligationTransaction,
            export.ObligationTransaction);
        }
        else
        {
          ExitState = "FN0000_OBLIG_TRANS_NF";

          goto Test4;
        }

        if (ReadAccrualInstructions1())
        {
          export.AccrualInstructions.Assign(entities.AccrualInstructions);
        }
        else
        {
          ExitState = "CO0000_ACCRUAL_INSTRUCTN_NF";

          goto Test4;
        }

        // ---------------------------------------------
        //       Get Obligation Type for screen
        // ---------------------------------------------
        if (ReadObligationType())
        {
          MoveObligationType(entities.ObligationType, export.ObligationType);
        }
        else
        {
          ExitState = "FN0000_OBLIG_TYPE_NF";

          goto Test4;
        }

        // : Jan., 2002 M. Brown Work Order # 010504 Retro Processing
        if (ReadObligCollProtectionHist())
        {
          export.CollProtection.Flag = "Y";
        }
        else
        {
          export.CollProtection.Flag = "N";
        }

        // ---------------------------------------------
        //          Calculate Amounts Owed
        // ---------------------------------------------
        local.DateWorkArea.Date = local.Current.Date;
        UseFnComputeSummaryTotals();

        if (Equal(export.ScreenOwedAmounts.ErrorInformationLine,
          "SUMMARY INFO IS UNAVAILABLE"))
        {
          var field1 = GetField(export.ScreenOwedAmounts, "currentAmountOwed");

          field1.Intensity = Intensity.Dark;
          field1.Protected = true;

          var field2 = GetField(export.ScreenOwedAmounts, "arrearsAmountOwed");

          field2.Intensity = Intensity.Dark;
          field2.Protected = true;

          var field3 = GetField(export.ScreenOwedAmounts, "interestAmountOwed");

          field3.Intensity = Intensity.Dark;
          field3.Protected = true;

          var field4 = GetField(export.ScreenOwedAmounts, "totalAmountOwed");

          field4.Intensity = Intensity.Dark;
          field4.Protected = true;
        }

        if (Equal(export.ScreenOwedAmounts.ErrorInformationLine,
          "UNPROCESSED TRANSACTIONS EXIST"))
        {
          var field1 =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field1.Color = "yellow";
          field1.Intensity = Intensity.High;
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = true;
        }

        // ------------------------------------------------------------
        // Sum up active accrual instructions to get total accrual
        // amount for the current obligation
        // ------------------------------------------------------------
        export.ObligationAmt.TotalCurrency = 0;

        foreach(var item in ReadObligationTransaction2())
        {
          export.ObligationAmt.TotalCurrency += entities.Cummulative.Amount;
        }

        // ---------------------------------------------
        //            Get accrual frequency
        // ---------------------------------------------
        if (ReadObligationPaymentSchedule())
        {
          // *****
          // If day of week has a value, assume frequency is weekly and show day
          // of week in first position on screen which maps to day_of_month_1,
          // which is display only.  Frequency code is not used so if codes
          // change logic won't need to be changed.
          // *****
          export.FrequencyWorkSet.FrequencyCode =
            entities.ObligationPaymentSchedule.FrequencyCode;

          if (Lt(0, entities.ObligationPaymentSchedule.DayOfWeek))
          {
            export.FrequencyWorkSet.Day1 =
              entities.ObligationPaymentSchedule.DayOfWeek.GetValueOrDefault();
          }
          else
          {
            export.FrequencyWorkSet.Day1 =
              entities.ObligationPaymentSchedule.DayOfMonth1.
                GetValueOrDefault();
          }

          if (!Equal(entities.ObligationPaymentSchedule.DayOfMonth2, 0))
          {
            export.FrequencyWorkSet.Day2 =
              entities.ObligationPaymentSchedule.DayOfMonth2.
                GetValueOrDefault();
          }

          UseFnSetFrequencyTextField();
        }
        else
        {
          ExitState = "FN0000_OBLIG_PYMNT_SCH_NF";

          goto Test4;
        }

        // ---------------------------------------------
        //              Get payor's name
        // ---------------------------------------------
        local.CsePersonsWorkSet.Number = export.CsePerson.Number;
        UseSiReadCsePerson1();

        if (!IsExitState("ACO_NN0000_ALL_OK") && !
          IsExitState("FN0000_SUCCESSFUL_PROCESSING"))
        {
          if (IsExitState("CSE_PERSON_NF"))
          {
            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;
          }

          goto Test4;
        }

        // ---------------------------------------------
        //          Get supported person's name
        // ---------------------------------------------
        local.CsePersonsWorkSet.Number = export.SupportedCsePerson.Number;
        UseSiReadCsePerson2();

        if (!IsExitState("ACO_NN0000_ALL_OK") && !
          IsExitState("FN0000_SUCCESSFUL_PROCESSING"))
        {
          if (IsExitState("CSE_PERSON_NF"))
          {
            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;
          }

          goto Test4;
        }

        // ---------------------------------------------
        //             List Suspensions
        // ---------------------------------------------
        if (AsChar(export.ShowHistory.Flag) == 'Y')
        {
          if (AsChar(export.HiddenFirstTime.Flag) == 'N')
          {
            goto Test3;
          }

          export.Group2.Index = -1;

          foreach(var item in ReadAccrualSuspension5())
          {
            ++export.Group2.Index;
            export.Group2.CheckSize();

            MoveAccrualSuspension1(entities.AccrualSuspension,
              export.Group2.Update.HiddenGrpDetail2AccrualSuspension);
            MoveAccrualSuspension3(entities.AccrualSuspension,
              export.Group2.Update.HiddenPrev2);
            export.Group2.Update.CreateDate2.Date =
              Date(entities.AccrualSuspension.CreatedTmst);

            if (Equal(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
              ResumeDt, local.Max.Date))
            {
              export.Group2.Update.HiddenGrpDetail2AccrualSuspension.ResumeDt =
                local.Zero.Date;
              export.Group2.Update.HiddenPrev2.ResumeDt = local.Zero.Date;
            }

            if (export.Group2.Index + 1 >= Export.Group2Group.Capacity)
            {
              export.Group2.Index = 0;
              export.Group2.CheckSize();

              export.AccrualSuspension.Assign(
                export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
              ExitState = "OE0000_MAX_NUMBER_OF_ENTRIES_EX";

              return;
            }
          }
        }
        else
        {
          // =================================================
          // 12/29/98 - b adams  -  Changed sort order from Descending
          //   to Ascending
          // =================================================
          if (AsChar(export.HiddenFirstTime.Flag) == 'N')
          {
            goto Test3;
          }

          export.Group2.Index = -1;

          foreach(var item in ReadAccrualSuspension4())
          {
            ++export.Group2.Index;
            export.Group2.CheckSize();

            MoveAccrualSuspension1(entities.AccrualSuspension,
              export.Group2.Update.HiddenGrpDetail2AccrualSuspension);
            MoveAccrualSuspension3(entities.AccrualSuspension,
              export.Group2.Update.HiddenPrev2);
            export.Group2.Update.CreateDate2.Date =
              Date(entities.AccrualSuspension.CreatedTmst);

            if (Equal(export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
              ResumeDt, local.Max.Date))
            {
              export.Group2.Update.HiddenGrpDetail2AccrualSuspension.ResumeDt =
                local.Zero.Date;
              export.Group2.Update.HiddenPrev2.ResumeDt = local.Zero.Date;
            }

            if (export.Group2.Index + 1 >= Export.Group2Group.Capacity)
            {
              export.Group2.Index = 0;
              export.Group2.CheckSize();

              export.AccrualSuspension.Assign(
                export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
              ExitState = "OE0000_MAX_NUMBER_OF_ENTRIES_EX";

              return;
            }
          }
        }

Test3:

        export.HiddenDisplay.Flag = "Y";

        if (!export.Group2.IsEmpty && AsChar(export.HiddenFirstTime.Flag) == 'Y'
          )
        {
          export.HiddenFirstTime.Flag = "N";
          export.HidCurrentRecord.Count = 1;

          // -----------------------------------------------------------------------------------
          // After reading all the records successfully, display the first 
          // record in groupview on the screen.
          // -----------------------------------------------------------------------------------
          export.Group2.Index = 0;
          export.Group2.CheckSize();

          export.Common.SelectChar =
            export.Group2.Item.HiddenGrpDetail2Common.SelectChar;
          export.AccrualSuspension.Assign(
            export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
        }

        if (export.Group2.IsEmpty && !Equal(global.Command, "REDISP"))
        {
          MoveAccrualSuspension2(local.InitializeAccrualSuspension,
            export.AccrualSuspension);
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          goto Test4;
        }

        if (export.Group2.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";
        }
      }

Test4:

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (Equal(global.Command, "DISPLAY") || Equal
          (global.Command, "NEXT") || Equal(global.Command, "PREV"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else
        {
          export.Common.SelectChar = "";
          ExitState = "FN0000_SUCCESSFUL_PROCESSING";

          // ------------------------------------------------------------------------
          // The following first SET statement is needed to SET the SCROLL 
          // values.
          // -------------------------------------------------------------------------
          export.HidCurrentRecord.Count = local.FirstProcessRecord.Count;

          export.Group2.Index = local.FirstProcessRecord.Count - 1;
          export.Group2.CheckSize();

          export.AccrualSuspension.Assign(
            export.Group2.Item.HiddenGrpDetail2AccrualSuspension);
        }
      }
    }

    if (export.Group2.Count <= 1)
    {
      export.ScrollIndicator.Text4 = "";
    }
    else if (export.Group2.Count > 1 && export.HidCurrentRecord.Count == 1)
    {
      export.ScrollIndicator.Text4 = "   +";
    }
    else if (export.Group2.Count > 2 && export.HidCurrentRecord.Count > 1 && export
      .HidCurrentRecord.Count < export.Group2.Count)
    {
      export.ScrollIndicator.Text4 = " -/+";
    }
    else if (export.Group2.Count > 1 && export.HidCurrentRecord.Count == export
      .Group2.Count)
    {
      export.ScrollIndicator.Text4 = "-";
    }

    // **** protect appropriate fields ****
    // =================================================
    // 1/4/99 - B Adams  -  For ADD transactions, none of those
    //   fields should be protected.  Also, if the data was just created
    //   today (meaning Accruals haven't been run yet) none should
    //   be protectec either.
    // 06/19/2000  V.MADHIRA    The above business rule is not valid. We must 
    // not protect the fields based on  ACCRUAL ran (ie. Last Accrual Dt is
    // populated) date.
    //  Check the  'Last_Accrual Date ' to protect the fields. I changed the 
    // CURRENT_DATE to LAST_ACCRUAL_DATE' in the below code.
    // =================================================
    if (export.AccrualSuspension.SystemGeneratedIdentifier > 0 && !
      Lt(export.AccrualInstructions.LastAccrualDt,
      export.Group2.Item.CreateDate2.Date))
    {
      if (!Lt(export.AccrualInstructions.LastAccrualDt,
        export.AccrualSuspension.SuspendDt) && Lt
        (export.Group2.Item.CreateDate2.Date,
        export.AccrualInstructions.LastAccrualDt) && Lt
        (local.Zero.Date, export.AccrualSuspension.SuspendDt))
      {
        var field1 = GetField(export.AccrualSuspension, "reductionPercentage");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.AccrualSuspension, "reductionAmount");

        field2.Color = "cyan";
        field2.Protected = true;

        if (!IsExitState("FN0000_SUSPEND_DT_B4_LST_ACCRUAL"))
        {
          var field3 = GetField(export.AccrualSuspension, "suspendDt");

          field3.Color = "cyan";
          field3.Protected = true;
        }
      }

      if (!Lt(export.AccrualInstructions.LastAccrualDt,
        export.AccrualSuspension.SuspendDt) && Lt
        (local.Zero.Date, export.AccrualSuspension.SuspendDt))
      {
        var field1 = GetField(export.AccrualSuspension, "reductionPercentage");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.AccrualSuspension, "reductionAmount");

        field2.Color = "cyan";
        field2.Protected = true;

        if (!IsExitState("FN0000_SUSPEND_DT_B4_LST_ACCRUAL"))
        {
          var field3 = GetField(export.AccrualSuspension, "suspendDt");

          field3.Color = "cyan";
          field3.Protected = true;
        }
      }

      // +++++++++++++++++++++++++++++++++++++++++++++++++
      // 12/29/98 - B Adams  -  The parenthesis below used to have
      //   "OR resume_date <= current_date"; this is not applicable
      // +++++++++++++++++++++++++++++++++++++++++++++++++
      if (!Lt(export.AccrualInstructions.LastAccrualDt,
        export.AccrualSuspension.ResumeDt) && Lt
        (local.Zero.Date, export.AccrualSuspension.ResumeDt))
      {
        if (!IsExitState("FN0000_RESUME_DT_B4_LST_ACCRUAL"))
        {
          var field1 = GetField(export.AccrualSuspension, "resumeDt");

          field1.Color = "cyan";
          field1.Protected = true;
        }
      }
    }

    // Jan., 2002 M. Brown Work Order # 010504 Retro Processing
    var field = GetField(export.CollProtAnswer, "selectChar");

    field.Intensity = Intensity.Dark;
    field.Highlighting = Highlighting.Normal;
    field.Protected = true;
    field.Focused = false;

    if (AsChar(local.RollbackRequired.Flag) == 'Y')
    {
      UseEabRollbackCics();
    }
  }

  private static void MoveAccrualSuspension1(AccrualSuspension source,
    AccrualSuspension target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SuspendDt = source.SuspendDt;
    target.ResumeDt = source.ResumeDt;
    target.ReductionPercentage = source.ReductionPercentage;
    target.ReasonTxt = source.ReasonTxt;
    target.CreatedTmst = source.CreatedTmst;
    target.ReductionAmount = source.ReductionAmount;
  }

  private static void MoveAccrualSuspension2(AccrualSuspension source,
    AccrualSuspension target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SuspendDt = source.SuspendDt;
    target.ResumeDt = source.ResumeDt;
    target.ReductionPercentage = source.ReductionPercentage;
    target.ReasonTxt = source.ReasonTxt;
    target.ReductionAmount = source.ReductionAmount;
  }

  private static void MoveAccrualSuspension3(AccrualSuspension source,
    AccrualSuspension target)
  {
    target.SuspendDt = source.SuspendDt;
    target.ResumeDt = source.ResumeDt;
    target.ReductionPercentage = source.ReductionPercentage;
    target.ReasonTxt = source.ReasonTxt;
    target.CreatedTmst = source.CreatedTmst;
    target.ReductionAmount = source.ReductionAmount;
  }

  private static void MoveAccrualSuspension4(AccrualSuspension source,
    AccrualSuspension target)
  {
    target.SuspendDt = source.SuspendDt;
    target.ResumeDt = source.ResumeDt;
    target.ReductionPercentage = source.ReductionPercentage;
    target.ReasonTxt = source.ReasonTxt;
    target.ReductionAmount = source.ReductionAmount;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
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
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
  }

  private static void MoveObligationTransaction1(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.ConditionalMaximum.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnAddAccrualSuspension1()
  {
    var useImport = new FnAddAccrualSuspension.Import();
    var useExport = new FnAddAccrualSuspension.Export();

    useImport.ZdelImportCurrent.Timestamp = local.Current.Timestamp;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      local.JointAndSeveralObligationTransaction.SystemGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = local.HardcodedObligor.Type1;
    useImport.CsePerson.Number = export.JointAndSeveralCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.JointAndSeveralObligation.SystemGeneratedIdentifier;
    useImport.AccrualSuspension.Assign(
      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);

    Call(FnAddAccrualSuspension.Execute, useImport, useExport);
  }

  private void UseFnAddAccrualSuspension2()
  {
    var useImport = new FnAddAccrualSuspension.Import();
    var useExport = new FnAddAccrualSuspension.Export();

    useImport.ZdelImportCurrent.Timestamp = local.Current.Timestamp;
    useImport.HcCpaObligor.Type1 = local.HardcodedObligor.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.ObligationTransaction.SystemGeneratedIdentifier;
    useImport.AccrualSuspension.Assign(
      export.Group2.Item.HiddenGrpDetail2AccrualSuspension);

    Call(FnAddAccrualSuspension.Execute, useImport, useExport);
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Obligor.Number = export.CsePerson.Number;
    useImport.Filter.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    MoveObligationTransaction2(useExport.OtrnDtAccrualInstructions,
      local.HcOtrnDtAccrualInstruc);
    local.HardcodedObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodedSupported.Type1 = useExport.CpaSupportedPerson.Type1;
    local.HardcodedDebt.Type1 = useExport.OtrnTDebt.Type1;
    local.HcObligJointAndSeveral.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
  }

  private void UseFnProtectCollectionsForOblig()
  {
    var useImport = new FnProtectCollectionsForOblig.Import();
    var useExport = new FnProtectCollectionsForOblig.Export();

    useImport.Persistent.Assign(entities.OtherView);
    useImport.ObligCollProtectionHist.Assign(local.ObligCollProtectionHist);
    useImport.CreateObCollProtHist.Flag = local.Common.Flag;

    Call(FnProtectCollectionsForOblig.Execute, useImport, useExport);

    entities.OtherView.Assign(useImport.Persistent);
    local.ObCollProtHistCreated.Flag = useExport.ObCollProtHistCreated.Flag;
    local.CollsFndToProtect.Flag = useExport.CollsFndToProtect.Flag;
  }

  private void UseFnReadConcurrentObligation()
  {
    var useImport = new FnReadConcurrentObligation.Import();
    var useExport = new FnReadConcurrentObligation.Export();

    MoveObligationTransaction2(local.HcOtrnDtAccrualInstruc,
      useImport.HcOtrnDtAccrlInstrctn);
    useImport.HcCpaObligor.Type1 = local.HardcodedObligor.Type1;
    useImport.HcCpaSupported.Type1 = local.HardcodedSupported.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodedDebt.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveObligation(export.Obligation, useImport.Obligation);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Supported.Number = export.SupportedCsePerson.Number;

    Call(FnReadConcurrentObligation.Execute, useImport, useExport);

    local.JointAndSeveralObligationTransaction.SystemGeneratedIdentifier =
      useExport.JointAndSeveralObligationTransaction.SystemGeneratedIdentifier;
    export.JointAndSeveralCsePerson.Number =
      useExport.JointAndSeveralCsePerson.Number;
    export.JointAndSeveralObligation.SystemGeneratedIdentifier =
      useExport.JointAndSeveralObligation.SystemGeneratedIdentifier;
  }

  private void UseFnSetFrequencyTextField()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    MoveObligationPaymentSchedule(entities.ObligationPaymentSchedule,
      useImport.ObligationPaymentSchedule);

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    export.FrequencyWorkSet.Assign(useExport.FrequencyWorkSet);
  }

  private void UseFnUpdateAccrualSuspension1()
  {
    var useImport = new FnUpdateAccrualSuspension.Import();
    var useExport = new FnUpdateAccrualSuspension.Export();

    useImport.CollProtAnswer.SelectChar = import.CollProtAnswer.SelectChar;
    useImport.ScreenOwedAmounts.Assign(export.ScreenOwedAmounts);
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      local.JointAndSeveralObligationTransaction.SystemGeneratedIdentifier;
    MoveAccrualSuspension2(local.JointAndSeveralAccrualSuspension,
      useImport.AccrualSuspension);
    useImport.HcCpaObligor.Type1 = local.HardcodedObligor.Type1;
    useImport.Obligor.Number = export.JointAndSeveralCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.JointAndSeveralObligation.SystemGeneratedIdentifier;

    Call(FnUpdateAccrualSuspension.Execute, useImport, useExport);
  }

  private void UseFnUpdateAccrualSuspension2()
  {
    var useImport = new FnUpdateAccrualSuspension.Import();
    var useExport = new FnUpdateAccrualSuspension.Export();

    useImport.CollProtAnswer.SelectChar = import.CollProtAnswer.SelectChar;
    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    useImport.ScreenOwedAmounts.Assign(export.ScreenOwedAmounts);
    useImport.AccrualSuspension.Assign(export.AccrualSuspension);
    useImport.DatesChanged.Flag = local.DatesChanged.Flag;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HcCpaObligor.Type1 = local.HardcodedObligor.Type1;
    useImport.Obligor.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.ObligationTransaction.SystemGeneratedIdentifier;

    Call(FnUpdateAccrualSuspension.Execute, useImport, useExport);

    MoveAccrualSuspension2(useExport.AccrualSuspension,
      export.Group2.Update.HiddenGrpDetail2AccrualSuspension);
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

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.LegalAction.StandardNumber = import.LegalAction.StandardNumber;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Type1 = useExport.AbendData.Type1;
    export.CsePerson.Number = useExport.CsePerson.Number;
    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.AbendData.Type1 = useExport.AbendData.Type1;
    export.SupportedCsePerson.Number = useExport.CsePerson.Number;
    export.SupportedCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void DeleteAccrualSuspension1()
  {
    Update("DeleteAccrualSuspension1",
      (db, command) =>
      {
        db.SetInt32(
          command, "frqSuspId",
          entities.AccrualSuspension.SystemGeneratedIdentifier);
        db.SetString(command, "otrType", entities.AccrualSuspension.OtrType);
        db.SetInt32(command, "otyId", entities.AccrualSuspension.OtyId);
        db.SetInt32(command, "obgId", entities.AccrualSuspension.ObgId);
        db.
          SetString(command, "cspNumber", entities.AccrualSuspension.CspNumber);
          
        db.SetString(command, "cpaType", entities.AccrualSuspension.CpaType);
        db.SetInt32(command, "otrId", entities.AccrualSuspension.OtrId);
      });
  }

  private void DeleteAccrualSuspension2()
  {
    Update("DeleteAccrualSuspension2",
      (db, command) =>
      {
        db.SetInt32(
          command, "frqSuspId",
          entities.JointAndSeveralAccrualSuspension.SystemGeneratedIdentifier);
        db.SetString(
          command, "otrType",
          entities.JointAndSeveralAccrualSuspension.OtrType);
        db.SetInt32(
          command, "otyId", entities.JointAndSeveralAccrualSuspension.OtyId);
        db.SetInt32(
          command, "obgId", entities.JointAndSeveralAccrualSuspension.ObgId);
        db.SetString(
          command, "cspNumber",
          entities.JointAndSeveralAccrualSuspension.CspNumber);
        db.SetString(
          command, "cpaType",
          entities.JointAndSeveralAccrualSuspension.CpaType);
        db.SetInt32(
          command, "otrId", entities.JointAndSeveralAccrualSuspension.OtrId);
      });
  }

  private bool ReadAccrualInstructions1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions1",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadAccrualInstructions2()
  {
    entities.JointAndSeveralAccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrGeneratedId",
          local.JointAndSeveralObligationTransaction.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "obgGeneratedId",
          export.JointAndSeveralObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.HardcodedObligor.Type1);
        db.SetString(
          command, "cspNumber", export.JointAndSeveralCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.JointAndSeveralAccrualInstructions.OtrType =
          db.GetString(reader, 0);
        entities.JointAndSeveralAccrualInstructions.OtyId =
          db.GetInt32(reader, 1);
        entities.JointAndSeveralAccrualInstructions.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralAccrualInstructions.CspNumber =
          db.GetString(reader, 3);
        entities.JointAndSeveralAccrualInstructions.CpaType =
          db.GetString(reader, 4);
        entities.JointAndSeveralAccrualInstructions.OtrGeneratedId =
          db.GetInt32(reader, 5);
        entities.JointAndSeveralAccrualInstructions.AsOfDt =
          db.GetDate(reader, 6);
        entities.JointAndSeveralAccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.JointAndSeveralAccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.JointAndSeveralAccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.JointAndSeveralAccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.JointAndSeveralAccrualInstructions.CpaType);
      });
  }

  private bool ReadAccrualInstructionsObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.AccrualInstructions.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return Read("ReadAccrualInstructionsObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "obTrnId",
          export.ObligationTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 9);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 10);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 11);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 12);
        entities.AccrualInstructions.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private bool ReadAccrualSuspension1()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.AccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension1",
      (db, command) =>
      {
        db.SetInt32(
          command, "frqSuspId",
          export.Group2.Item.HiddenGrpDetail2AccrualSuspension.
            SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otrId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgId", entities.AccrualInstructions.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.ReductionPercentage =
          db.GetNullableDecimal(reader, 3);
        entities.AccrualSuspension.CreatedTmst = db.GetDateTime(reader, 4);
        entities.AccrualSuspension.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.AccrualSuspension.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 7);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 8);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 9);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 10);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 11);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 12);
        entities.AccrualSuspension.ReductionAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AccrualSuspension.ReasonTxt = db.GetNullableString(reader, 14);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);
      });
  }

  private bool ReadAccrualSuspension2()
  {
    entities.JointAndSeveralAccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId",
          local.JointAndSeveralObligationTransaction.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "obgId",
          export.JointAndSeveralObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.HardcodedObligor.Type1);
        db.SetString(
          command, "cspNumber", export.JointAndSeveralCsePerson.Number);
        db.SetDate(
          command, "suspendDt",
          local.JointAndSeveralAccrualSuspension.SuspendDt.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.JointAndSeveralAccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.JointAndSeveralAccrualSuspension.SuspendDt =
          db.GetDate(reader, 1);
        entities.JointAndSeveralAccrualSuspension.OtrType =
          db.GetString(reader, 2);
        entities.JointAndSeveralAccrualSuspension.OtyId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralAccrualSuspension.ObgId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralAccrualSuspension.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralAccrualSuspension.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralAccrualSuspension.OtrId =
          db.GetInt32(reader, 7);
        entities.JointAndSeveralAccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.JointAndSeveralAccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.JointAndSeveralAccrualSuspension.CpaType);
      });
  }

  private bool ReadAccrualSuspension3()
  {
    System.Diagnostics.Debug.Assert(
      entities.JointAndSeveralAccrualInstructions.Populated);
    entities.JointAndSeveralAccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension3",
      (db, command) =>
      {
        db.SetDate(
          command, "suspendDt", local.Deleted.SuspendDt.GetValueOrDefault());
        db.SetInt32(
          command, "otrId",
          entities.JointAndSeveralAccrualInstructions.OtrGeneratedId);
        db.SetString(
          command, "cpaType",
          entities.JointAndSeveralAccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.JointAndSeveralAccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgId",
          entities.JointAndSeveralAccrualInstructions.ObgGeneratedId);
        db.SetInt32(
          command, "otyId", entities.JointAndSeveralAccrualInstructions.OtyId);
        db.SetString(
          command, "otrType",
          entities.JointAndSeveralAccrualInstructions.OtrType);
      },
      (db, reader) =>
      {
        entities.JointAndSeveralAccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.JointAndSeveralAccrualSuspension.SuspendDt =
          db.GetDate(reader, 1);
        entities.JointAndSeveralAccrualSuspension.OtrType =
          db.GetString(reader, 2);
        entities.JointAndSeveralAccrualSuspension.OtyId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralAccrualSuspension.ObgId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralAccrualSuspension.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralAccrualSuspension.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralAccrualSuspension.OtrId =
          db.GetInt32(reader, 7);
        entities.JointAndSeveralAccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.JointAndSeveralAccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.JointAndSeveralAccrualSuspension.CpaType);
      });
  }

  private IEnumerable<bool> ReadAccrualSuspension4()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.AccrualSuspension.Populated = false;

    return ReadEach("ReadAccrualSuspension4",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgId", entities.AccrualInstructions.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.SetDate(
          command, "suspendDt",
          entities.AccrualInstructions.LastAccrualDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.ReductionPercentage =
          db.GetNullableDecimal(reader, 3);
        entities.AccrualSuspension.CreatedTmst = db.GetDateTime(reader, 4);
        entities.AccrualSuspension.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.AccrualSuspension.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 7);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 8);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 9);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 10);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 11);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 12);
        entities.AccrualSuspension.ReductionAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AccrualSuspension.ReasonTxt = db.GetNullableString(reader, 14);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadAccrualSuspension5()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.AccrualSuspension.Populated = false;

    return ReadEach("ReadAccrualSuspension5",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgId", entities.AccrualInstructions.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.ReductionPercentage =
          db.GetNullableDecimal(reader, 3);
        entities.AccrualSuspension.CreatedTmst = db.GetDateTime(reader, 4);
        entities.AccrualSuspension.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.AccrualSuspension.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 7);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 8);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 9);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 10);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 11);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 12);
        entities.AccrualSuspension.ReductionAmount =
          db.GetNullableDecimal(reader, 13);
        entities.AccrualSuspension.ReasonTxt = db.GetNullableString(reader, 14);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);

        return true;
      });
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetDate(
          command, "suspendDt",
          entities.AccrualSuspension.SuspendDt.GetValueOrDefault());
        db.SetNullableDate(
          command, "resumeDt",
          entities.AccrualSuspension.ResumeDt.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 16);
        entities.Collection.DistributionMethod = db.GetString(reader, 17);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 18);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
      });
  }

  private bool ReadObligCollProtectionHist()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          entities.Obligation.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "otyIdentifier", entities.Obligation.DtyGeneratedId);
          
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetDate(
          command, "deactivationDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 4);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 5);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 6);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 7);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
      });
  }

  private bool ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
      });
  }

  private IEnumerable<bool> ReadObligation2()
  {
    entities.OtherView.Populated = false;

    return ReadEach("ReadObligation2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.OtherView.CpaType = db.GetString(reader, 0);
        entities.OtherView.CspNumber = db.GetString(reader, 1);
        entities.OtherView.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.OtherView.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.OtherView.LgaId = db.GetNullableInt32(reader, 4);
        entities.OtherView.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.OtherView.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.OtherView.LastUpdateTmst = db.GetNullableDateTime(reader, 7);
        entities.OtherView.LastObligationEvent =
          db.GetNullableString(reader, 8);
        entities.OtherView.Populated = true;
        CheckValid<Obligation>("CpaType", entities.OtherView.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.OtherView.PrimarySecondaryCode);

        return true;
      });
  }

  private bool ReadObligationCsePersonCsePersonAccount()
  {
    entities.CsePerson.Populated = false;
    entities.CsePersonAccount.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadObligationCsePersonCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.CsePerson.Populated = true;
        entities.CsePersonAccount.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
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
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
      });
  }

  private bool ReadObligationPaymentScheduleObligation()
  {
    entities.Obligation.Populated = false;
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentScheduleObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.Obligation.CspNumber = db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.Obligation.CpaType = db.GetString(reader, 3);
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
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 10);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 11);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 12);
        entities.Obligation.Populated = true;
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnId",
          import.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", local.HardcodedDebt.Type1);
        db.SetString(command, "debtTyp", local.HcOtrnDtAccrualInstruc.DebtType);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 6);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 9);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private IEnumerable<bool> ReadObligationTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Cummulative.Populated = false;

    return ReadEach("ReadObligationTransaction2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "debtTyp", local.HcOtrnDtAccrualInstruc.DebtType);
        db.SetString(command, "obTrnTyp", local.HardcodedDebt.Type1);
      },
      (db, reader) =>
      {
        entities.Cummulative.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Cummulative.CspNumber = db.GetString(reader, 1);
        entities.Cummulative.CpaType = db.GetString(reader, 2);
        entities.Cummulative.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Cummulative.Type1 = db.GetString(reader, 4);
        entities.Cummulative.Amount = db.GetDecimal(reader, 5);
        entities.Cummulative.DebtType = db.GetString(reader, 6);
        entities.Cummulative.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Cummulative.CpaSupType = db.GetNullableString(reader, 8);
        entities.Cummulative.OtyType = db.GetInt32(reader, 9);
        entities.Cummulative.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.Cummulative.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Cummulative.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.Cummulative.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.Cummulative.CpaSupType);

        return true;
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
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A Group2Group group.</summary>
    [Serializable]
    public class Group2Group
    {
      /// <summary>
      /// A value of HiddenGrpDetail2Common.
      /// </summary>
      [JsonPropertyName("hiddenGrpDetail2Common")]
      public Common HiddenGrpDetail2Common
      {
        get => hiddenGrpDetail2Common ??= new();
        set => hiddenGrpDetail2Common = value;
      }

      /// <summary>
      /// A value of HiddenGrpDetail2AccrualSuspension.
      /// </summary>
      [JsonPropertyName("hiddenGrpDetail2AccrualSuspension")]
      public AccrualSuspension HiddenGrpDetail2AccrualSuspension
      {
        get => hiddenGrpDetail2AccrualSuspension ??= new();
        set => hiddenGrpDetail2AccrualSuspension = value;
      }

      /// <summary>
      /// A value of HiddenPrev2.
      /// </summary>
      [JsonPropertyName("hiddenPrev2")]
      public AccrualSuspension HiddenPrev2
      {
        get => hiddenPrev2 ??= new();
        set => hiddenPrev2 = value;
      }

      /// <summary>
      /// A value of CreateDate2.
      /// </summary>
      [JsonPropertyName("createDate2")]
      public DateWorkArea CreateDate2
      {
        get => createDate2 ??= new();
        set => createDate2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common hiddenGrpDetail2Common;
      private AccrualSuspension hiddenGrpDetail2AccrualSuspension;
      private AccrualSuspension hiddenPrev2;
      private DateWorkArea createDate2;
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
    /// A value of CountOfPages.
    /// </summary>
    [JsonPropertyName("countOfPages")]
    public Common CountOfPages
    {
      get => countOfPages ??= new();
      set => countOfPages = value;
    }

    /// <summary>
    /// A value of CountOfRecords.
    /// </summary>
    [JsonPropertyName("countOfRecords")]
    public Common CountOfRecords
    {
      get => countOfRecords ??= new();
      set => countOfRecords = value;
    }

    /// <summary>
    /// A value of ConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("concurrentCsePerson")]
    public CsePerson ConcurrentCsePerson
    {
      get => concurrentCsePerson ??= new();
      set => concurrentCsePerson = value;
    }

    /// <summary>
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
    }

    /// <summary>
    /// A value of HiddenDisplay.
    /// </summary>
    [JsonPropertyName("hiddenDisplay")]
    public Common HiddenDisplay
    {
      get => hiddenDisplay ??= new();
      set => hiddenDisplay = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of HiddenFirstTime.
    /// </summary>
    [JsonPropertyName("hiddenFirstTime")]
    public Common HiddenFirstTime
    {
      get => hiddenFirstTime ??= new();
      set => hiddenFirstTime = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
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
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("supportedCsePersonsWorkSet")]
    public CsePersonsWorkSet SupportedCsePersonsWorkSet
    {
      get => supportedCsePersonsWorkSet ??= new();
      set => supportedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ObligationAmt.
    /// </summary>
    [JsonPropertyName("obligationAmt")]
    public Common ObligationAmt
    {
      get => obligationAmt ??= new();
      set => obligationAmt = value;
    }

    /// <summary>
    /// A value of Redisplay.
    /// </summary>
    [JsonPropertyName("redisplay")]
    public AccrualSuspension Redisplay
    {
      get => redisplay ??= new();
      set => redisplay = value;
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
    /// A value of HidCurrentRecord.
    /// </summary>
    [JsonPropertyName("hidCurrentRecord")]
    public Common HidCurrentRecord
    {
      get => hidCurrentRecord ??= new();
      set => hidCurrentRecord = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
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
    /// A value of HiddenShowHistory.
    /// </summary>
    [JsonPropertyName("hiddenShowHistory")]
    public Common HiddenShowHistory
    {
      get => hiddenShowHistory ??= new();
      set => hiddenShowHistory = value;
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public TextWorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// A value of CollProtection.
    /// </summary>
    [JsonPropertyName("collProtection")]
    public Common CollProtection
    {
      get => collProtection ??= new();
      set => collProtection = value;
    }

    /// <summary>
    /// A value of ProtectQuestionLiteral.
    /// </summary>
    [JsonPropertyName("protectQuestionLiteral")]
    public SpTextWorkArea ProtectQuestionLiteral
    {
      get => protectQuestionLiteral ??= new();
      set => protectQuestionLiteral = value;
    }

    /// <summary>
    /// A value of CollProtAnswer.
    /// </summary>
    [JsonPropertyName("collProtAnswer")]
    public Common CollProtAnswer
    {
      get => collProtAnswer ??= new();
      set => collProtAnswer = value;
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

    private Array<Group2Group> group2;
    private Common countOfPages;
    private Common countOfRecords;
    private CsePerson concurrentCsePerson;
    private Obligation concurrentObligation;
    private Common hiddenDisplay;
    private AccrualInstructions accrualInstructions;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common hiddenFirstTime;
    private FrequencyWorkSet frequencyWorkSet;
    private LegalAction legalAction;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common showHistory;
    private CsePerson supportedCsePerson;
    private CsePersonsWorkSet supportedCsePersonsWorkSet;
    private Common obligationAmt;
    private AccrualSuspension redisplay;
    private NextTranInfo hidden;
    private Standard standard;
    private Common hidCurrentRecord;
    private AccrualSuspension accrualSuspension;
    private Common common;
    private Common hiddenShowHistory;
    private TextWorkArea scrollIndicator;
    private Common collProtection;
    private SpTextWorkArea protectQuestionLiteral;
    private Common collProtAnswer;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A Group2Group group.</summary>
    [Serializable]
    public class Group2Group
    {
      /// <summary>
      /// A value of HiddenGrpDetail2Common.
      /// </summary>
      [JsonPropertyName("hiddenGrpDetail2Common")]
      public Common HiddenGrpDetail2Common
      {
        get => hiddenGrpDetail2Common ??= new();
        set => hiddenGrpDetail2Common = value;
      }

      /// <summary>
      /// A value of HiddenGrpDetail2AccrualSuspension.
      /// </summary>
      [JsonPropertyName("hiddenGrpDetail2AccrualSuspension")]
      public AccrualSuspension HiddenGrpDetail2AccrualSuspension
      {
        get => hiddenGrpDetail2AccrualSuspension ??= new();
        set => hiddenGrpDetail2AccrualSuspension = value;
      }

      /// <summary>
      /// A value of HiddenPrev2.
      /// </summary>
      [JsonPropertyName("hiddenPrev2")]
      public AccrualSuspension HiddenPrev2
      {
        get => hiddenPrev2 ??= new();
        set => hiddenPrev2 = value;
      }

      /// <summary>
      /// A value of CreateDate2.
      /// </summary>
      [JsonPropertyName("createDate2")]
      public DateWorkArea CreateDate2
      {
        get => createDate2 ??= new();
        set => createDate2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private Common hiddenGrpDetail2Common;
      private AccrualSuspension hiddenGrpDetail2AccrualSuspension;
      private AccrualSuspension hiddenPrev2;
      private DateWorkArea createDate2;
    }

    /// <summary>
    /// A value of CountOfPages.
    /// </summary>
    [JsonPropertyName("countOfPages")]
    public Common CountOfPages
    {
      get => countOfPages ??= new();
      set => countOfPages = value;
    }

    /// <summary>
    /// A value of CountOfRecords.
    /// </summary>
    [JsonPropertyName("countOfRecords")]
    public Common CountOfRecords
    {
      get => countOfRecords ??= new();
      set => countOfRecords = value;
    }

    /// <summary>
    /// A value of JointAndSeveralCsePerson.
    /// </summary>
    [JsonPropertyName("jointAndSeveralCsePerson")]
    public CsePerson JointAndSeveralCsePerson
    {
      get => jointAndSeveralCsePerson ??= new();
      set => jointAndSeveralCsePerson = value;
    }

    /// <summary>
    /// A value of JointAndSeveralObligation.
    /// </summary>
    [JsonPropertyName("jointAndSeveralObligation")]
    public Obligation JointAndSeveralObligation
    {
      get => jointAndSeveralObligation ??= new();
      set => jointAndSeveralObligation = value;
    }

    /// <summary>
    /// A value of HiddenDisplay.
    /// </summary>
    [JsonPropertyName("hiddenDisplay")]
    public Common HiddenDisplay
    {
      get => hiddenDisplay ??= new();
      set => hiddenDisplay = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of HiddenFirstTime.
    /// </summary>
    [JsonPropertyName("hiddenFirstTime")]
    public Common HiddenFirstTime
    {
      get => hiddenFirstTime ??= new();
      set => hiddenFirstTime = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
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
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("supportedCsePersonsWorkSet")]
    public CsePersonsWorkSet SupportedCsePersonsWorkSet
    {
      get => supportedCsePersonsWorkSet ??= new();
      set => supportedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ObligationAmt.
    /// </summary>
    [JsonPropertyName("obligationAmt")]
    public Common ObligationAmt
    {
      get => obligationAmt ??= new();
      set => obligationAmt = value;
    }

    /// <summary>
    /// A value of Redisplay.
    /// </summary>
    [JsonPropertyName("redisplay")]
    public AccrualSuspension Redisplay
    {
      get => redisplay ??= new();
      set => redisplay = value;
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
    /// A value of HidCurrentRecord.
    /// </summary>
    [JsonPropertyName("hidCurrentRecord")]
    public Common HidCurrentRecord
    {
      get => hidCurrentRecord ??= new();
      set => hidCurrentRecord = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
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
    /// A value of HiddenShowHistory.
    /// </summary>
    [JsonPropertyName("hiddenShowHistory")]
    public Common HiddenShowHistory
    {
      get => hiddenShowHistory ??= new();
      set => hiddenShowHistory = value;
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public TextWorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// A value of CollProtection.
    /// </summary>
    [JsonPropertyName("collProtection")]
    public Common CollProtection
    {
      get => collProtection ??= new();
      set => collProtection = value;
    }

    /// <summary>
    /// A value of ProtectQuestionLiteral.
    /// </summary>
    [JsonPropertyName("protectQuestionLiteral")]
    public SpTextWorkArea ProtectQuestionLiteral
    {
      get => protectQuestionLiteral ??= new();
      set => protectQuestionLiteral = value;
    }

    /// <summary>
    /// A value of CollProtAnswer.
    /// </summary>
    [JsonPropertyName("collProtAnswer")]
    public Common CollProtAnswer
    {
      get => collProtAnswer ??= new();
      set => collProtAnswer = value;
    }

    private Common countOfPages;
    private Common countOfRecords;
    private CsePerson jointAndSeveralCsePerson;
    private Obligation jointAndSeveralObligation;
    private Common hiddenDisplay;
    private AccrualInstructions accrualInstructions;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common hiddenFirstTime;
    private FrequencyWorkSet frequencyWorkSet;
    private LegalAction legalAction;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common showHistory;
    private CsePerson supportedCsePerson;
    private CsePersonsWorkSet supportedCsePersonsWorkSet;
    private Common obligationAmt;
    private AccrualSuspension redisplay;
    private Array<Group2Group> group2;
    private NextTranInfo hidden;
    private Standard standard;
    private Common hidCurrentRecord;
    private AccrualSuspension accrualSuspension;
    private Common common;
    private Common hiddenShowHistory;
    private TextWorkArea scrollIndicator;
    private Common collProtection;
    private SpTextWorkArea protectQuestionLiteral;
    private Common collProtAnswer;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of Deleted.
    /// </summary>
    [JsonPropertyName("deleted")]
    public AccrualSuspension Deleted
    {
      get => deleted ??= new();
      set => deleted = value;
    }

    /// <summary>
    /// A value of RollbackRequired.
    /// </summary>
    [JsonPropertyName("rollbackRequired")]
    public Common RollbackRequired
    {
      get => rollbackRequired ??= new();
      set => rollbackRequired = value;
    }

    /// <summary>
    /// A value of HcObligJointAndSeveral.
    /// </summary>
    [JsonPropertyName("hcObligJointAndSeveral")]
    public Obligation HcObligJointAndSeveral
    {
      get => hcObligJointAndSeveral ??= new();
      set => hcObligJointAndSeveral = value;
    }

    /// <summary>
    /// A value of GroupRecord.
    /// </summary>
    [JsonPropertyName("groupRecord")]
    public Common GroupRecord
    {
      get => groupRecord ??= new();
      set => groupRecord = value;
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
    /// A value of HcOtrnDtAccrualInstruc.
    /// </summary>
    [JsonPropertyName("hcOtrnDtAccrualInstruc")]
    public ObligationTransaction HcOtrnDtAccrualInstruc
    {
      get => hcOtrnDtAccrualInstruc ??= new();
      set => hcOtrnDtAccrualInstruc = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of InitializeCommon.
    /// </summary>
    [JsonPropertyName("initializeCommon")]
    public Common InitializeCommon
    {
      get => initializeCommon ??= new();
      set => initializeCommon = value;
    }

    /// <summary>
    /// A value of InitializeAccrualSuspension.
    /// </summary>
    [JsonPropertyName("initializeAccrualSuspension")]
    public AccrualSuspension InitializeAccrualSuspension
    {
      get => initializeAccrualSuspension ??= new();
      set => initializeAccrualSuspension = value;
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
    /// A value of DeleteCount.
    /// </summary>
    [JsonPropertyName("deleteCount")]
    public Common DeleteCount
    {
      get => deleteCount ??= new();
      set => deleteCount = value;
    }

    /// <summary>
    /// A value of UpdateCount.
    /// </summary>
    [JsonPropertyName("updateCount")]
    public Common UpdateCount
    {
      get => updateCount ??= new();
      set => updateCount = value;
    }

    /// <summary>
    /// A value of AddCount.
    /// </summary>
    [JsonPropertyName("addCount")]
    public Common AddCount
    {
      get => addCount ??= new();
      set => addCount = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of ConditionalMaximum.
    /// </summary>
    [JsonPropertyName("conditionalMaximum")]
    public DateWorkArea ConditionalMaximum
    {
      get => conditionalMaximum ??= new();
      set => conditionalMaximum = value;
    }

    /// <summary>
    /// A value of JointAndSeveralObligationTransaction.
    /// </summary>
    [JsonPropertyName("jointAndSeveralObligationTransaction")]
    public ObligationTransaction JointAndSeveralObligationTransaction
    {
      get => jointAndSeveralObligationTransaction ??= new();
      set => jointAndSeveralObligationTransaction = value;
    }

    /// <summary>
    /// A value of Concurrent.
    /// </summary>
    [JsonPropertyName("concurrent")]
    public AccrualInstructions Concurrent
    {
      get => concurrent ??= new();
      set => concurrent = value;
    }

    /// <summary>
    /// A value of JointAndSeveralAccrualSuspension.
    /// </summary>
    [JsonPropertyName("jointAndSeveralAccrualSuspension")]
    public AccrualSuspension JointAndSeveralAccrualSuspension
    {
      get => jointAndSeveralAccrualSuspension ??= new();
      set => jointAndSeveralAccrualSuspension = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public AccrualSuspension Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of HardcodedObligor.
    /// </summary>
    [JsonPropertyName("hardcodedObligor")]
    public CsePersonAccount HardcodedObligor
    {
      get => hardcodedObligor ??= new();
      set => hardcodedObligor = value;
    }

    /// <summary>
    /// A value of HardcodedSupported.
    /// </summary>
    [JsonPropertyName("hardcodedSupported")]
    public CsePersonAccount HardcodedSupported
    {
      get => hardcodedSupported ??= new();
      set => hardcodedSupported = value;
    }

    /// <summary>
    /// A value of HardcodedDebt.
    /// </summary>
    [JsonPropertyName("hardcodedDebt")]
    public ObligationTransaction HardcodedDebt
    {
      get => hardcodedDebt ??= new();
      set => hardcodedDebt = value;
    }

    /// <summary>
    /// A value of SelectChar.
    /// </summary>
    [JsonPropertyName("selectChar")]
    public Common SelectChar
    {
      get => selectChar ??= new();
      set => selectChar = value;
    }

    /// <summary>
    /// A value of FirstProcessRecord.
    /// </summary>
    [JsonPropertyName("firstProcessRecord")]
    public Common FirstProcessRecord
    {
      get => firstProcessRecord ??= new();
      set => firstProcessRecord = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of ObCollProtHistCreated.
    /// </summary>
    [JsonPropertyName("obCollProtHistCreated")]
    public Common ObCollProtHistCreated
    {
      get => obCollProtHistCreated ??= new();
      set => obCollProtHistCreated = value;
    }

    /// <summary>
    /// A value of CollsFndToProtect.
    /// </summary>
    [JsonPropertyName("collsFndToProtect")]
    public Common CollsFndToProtect
    {
      get => collsFndToProtect ??= new();
      set => collsFndToProtect = value;
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
    /// A value of DatesChanged.
    /// </summary>
    [JsonPropertyName("datesChanged")]
    public Common DatesChanged
    {
      get => datesChanged ??= new();
      set => datesChanged = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      deleted = null;
      rollbackRequired = null;
      hcObligJointAndSeveral = null;
      groupRecord = null;
      max = null;
      zero = null;
      initializeCommon = null;
      initializeAccrualSuspension = null;
      deleteCount = null;
      updateCount = null;
      addCount = null;
      error = null;
      conditionalMaximum = null;
      jointAndSeveralObligationTransaction = null;
      concurrent = null;
      jointAndSeveralAccrualSuspension = null;
      null1 = null;
      abendData = null;
      csePersonsWorkSet = null;
      dateWorkArea = null;
      selectChar = null;
      firstProcessRecord = null;
      obligCollProtectionHist = null;
      obCollProtHistCreated = null;
      collsFndToProtect = null;
      common = null;
      datesChanged = null;
    }

    private AccrualSuspension deleted;
    private Common rollbackRequired;
    private Obligation hcObligJointAndSeveral;
    private Common groupRecord;
    private DateWorkArea max;
    private ObligationTransaction hcOtrnDtAccrualInstruc;
    private DateWorkArea zero;
    private Common initializeCommon;
    private AccrualSuspension initializeAccrualSuspension;
    private DateWorkArea current;
    private Common deleteCount;
    private Common updateCount;
    private Common addCount;
    private Common error;
    private DateWorkArea conditionalMaximum;
    private ObligationTransaction jointAndSeveralObligationTransaction;
    private AccrualInstructions concurrent;
    private AccrualSuspension jointAndSeveralAccrualSuspension;
    private AccrualSuspension null1;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea dateWorkArea;
    private CsePersonAccount hardcodedObligor;
    private CsePersonAccount hardcodedSupported;
    private ObligationTransaction hardcodedDebt;
    private Common selectChar;
    private Common firstProcessRecord;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Common obCollProtHistCreated;
    private Common collsFndToProtect;
    private Common common;
    private Common datesChanged;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of JointAndSeveralObligationTransaction.
    /// </summary>
    [JsonPropertyName("jointAndSeveralObligationTransaction")]
    public ObligationTransaction JointAndSeveralObligationTransaction
    {
      get => jointAndSeveralObligationTransaction ??= new();
      set => jointAndSeveralObligationTransaction = value;
    }

    /// <summary>
    /// A value of JointAndSeveralAccrualInstructions.
    /// </summary>
    [JsonPropertyName("jointAndSeveralAccrualInstructions")]
    public AccrualInstructions JointAndSeveralAccrualInstructions
    {
      get => jointAndSeveralAccrualInstructions ??= new();
      set => jointAndSeveralAccrualInstructions = value;
    }

    /// <summary>
    /// A value of Cummulative.
    /// </summary>
    [JsonPropertyName("cummulative")]
    public ObligationTransaction Cummulative
    {
      get => cummulative ??= new();
      set => cummulative = value;
    }

    /// <summary>
    /// A value of JointAndSeveralAccrualSuspension.
    /// </summary>
    [JsonPropertyName("jointAndSeveralAccrualSuspension")]
    public AccrualSuspension JointAndSeveralAccrualSuspension
    {
      get => jointAndSeveralAccrualSuspension ??= new();
      set => jointAndSeveralAccrualSuspension = value;
    }

    /// <summary>
    /// A value of JointAndSeveralObligation.
    /// </summary>
    [JsonPropertyName("jointAndSeveralObligation")]
    public Obligation JointAndSeveralObligation
    {
      get => jointAndSeveralObligation ??= new();
      set => jointAndSeveralObligation = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of OtherView.
    /// </summary>
    [JsonPropertyName("otherView")]
    public Obligation OtherView
    {
      get => otherView ??= new();
      set => otherView = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    private ObligationTransaction jointAndSeveralObligationTransaction;
    private AccrualInstructions jointAndSeveralAccrualInstructions;
    private ObligationTransaction cummulative;
    private AccrualSuspension jointAndSeveralAccrualSuspension;
    private Obligation jointAndSeveralObligation;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private AccrualInstructions accrualInstructions;
    private AccrualSuspension accrualSuspension;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Obligation otherView;
    private LegalAction legalAction;
    private Collection collection;
    private ObligationTransaction debt;
  }
#endregion
}
