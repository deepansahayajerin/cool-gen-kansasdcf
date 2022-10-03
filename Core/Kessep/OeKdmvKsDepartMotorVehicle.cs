// Program: OE_KDMV_KS_DEPART_MOTOR_VEHICLE, ID: 371371970, model: 746.
// Short name: SWEKDMVP
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
/// A program: OE_KDMV_KS_DEPART_MOTOR_VEHICLE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeKdmvKsDepartMotorVehicle: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_KDMV_KS_DEPART_MOTOR_VEHICLE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeKdmvKsDepartMotorVehicle(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeKdmvKsDepartMotorVehicle.
  /// </summary>
  public OeKdmvKsDepartMotorVehicle(IContext context, Import import,
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
    // *******************************************************************
    //   Date		Developer	Request #	Description
    // 09-10-2008        DDupree	 WR280420            Initial development
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      // transfer to ddmm screen (FN_DDMM_DEBT_DSTRBTN_MNGMNT_MENU)
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      // **** begin group F ****
      UseScCabSignoff();

      return;

      // **** end   group F ****
    }

    local.HighlightError.Flag = "Y";
    local.Rollback.Flag = "N";

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    if (Equal(global.Command, "DONE"))
    {
      if (Equal(import.CsePersonsWorkSet.Number,
        import.FlowCsePersonsWorkSet.Number) && !
        IsEmpty(import.CsePersonsWorkSet.Number))
      {
        export.LegalAction.Assign(import.LegalAction);
        export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
        export.KsDriversLicense.Assign(import.KsDriversLicense);
        export.AmountDue.Text9 = import.AmountDue.Text9;
        export.LastUpdate.Date = import.LastUpdate.Date;
      }
      else if (!IsEmpty(import.FlowCsePersonsWorkSet.Number))
      {
        export.CsePersonsWorkSet.Number = import.FlowCsePersonsWorkSet.Number;
        export.KsDriversLicense.SequenceCounter =
          import.FlowKsDriversLicense.SequenceCounter;
      }
      else
      {
        return;
      }

      global.Command = "DISPLAY";
    }
    else
    {
      export.LegalAction.Assign(import.LegalAction);
      export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
      export.KsDriversLicense.Assign(import.KsDriversLicense);
      export.AmountDue.Text9 = import.AmountDue.Text9;
      export.LastUpdate.Date = import.LastUpdate.Date;
    }

    // *********************************************
    // Move Hidden Imports to Hidden Exports.
    // *********************************************
    export.HiddenPrevLegalAction.Assign(import.HiddenPrevLegalAction);
    MoveCsePersonsWorkSet(import.HiddenPrevCsePersonsWorkSet,
      export.HiddenPrevCsePersonsWorkSet);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    export.HiddenKsDriversLicense.SequenceCounter =
      import.HiddenKsDriversLicense.SequenceCounter;

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    local.NullDate.Date = new DateTime(1, 1, 1);
    export.PromptCsePersonNumber.SelectChar =
      import.PromptCsePersonNumber.SelectChar;
    export.PromptAppealDecsionCod.SelectChar =
      import.PromptAppealDecisionCd.SelectChar;
    export.HistoricRecord.Flag = import.HistoricRecord.Flag;
    export.MoreLessScroll.Text3 = import.MoreLessScroll.Text3;
    export.UpdateConfirm.Count = import.UpdateConfirm.Count;

    if (Verify(import.AmountDue.Text9, " 0123456789.") > 0)
    {
      var field = GetField(export.AmountDue, "text9");

      field.Error = true;

      ExitState = "ACO_NON_NUMERIC_DATA_ENTERED";

      return;
    }

    if (!IsEmpty(import.AmountDue.Text9))
    {
      local.Start.Count = 1;
      local.Start.Count = Verify(import.AmountDue.Text9, " ");
      local.Current.Count = 1;
      local.CurrentPosition.Count = local.Start.Count;
      local.AmountDue2.Text9 = import.AmountDue.Text9;

      do
      {
        local.Postion.Text1 =
          Substring(local.AmountDue2.Text9, local.CurrentPosition.Count, 1);

        if (AsChar(local.Postion.Text1) == '.')
        {
          local.WorkArea.Text15 = "";
          local.WorkArea.Text15 =
            Substring(local.AmountDue2.Text9, local.Start.Count,
            local.Current.Count - 1);
          local.Dollars.TotalCurrency = StringToNumber(local.WorkArea.Text15);
          local.Start.Count = local.CurrentPosition.Count + 1;
          local.Current.Count = 0;
        }
        else if (IsEmpty(local.Postion.Text1))
        {
          local.AmountDue3.Text9 =
            Substring(local.AmountDue2.Text9, local.Current.Count, 9 -
            local.Current.Count);

          if (Verify(local.AmountDue3.Text9, " 0123456789.") > 0)
          {
            local.Start.Count = Verify(local.AmountDue3.Text9, " ");
          }

          if (local.Dollars.TotalCurrency <= 0)
          {
            local.WorkArea.Text15 =
              Substring(local.AmountDue2.Text9, local.Start.Count +
              0, local.Current.Count - 1);

            if (Verify(local.AmountDue2.Text9, " 0123456789") > 0)
            {
              // there was a decmial point in here
              local.Cents.TotalCurrency = StringToNumber(local.WorkArea.Text15);
              local.AmountDue3.Text9 =
                Substring(local.AmountDue2.Text9, local.Start.Count, 2);

              if (!IsEmpty(local.AmountDue3.Text9))
              {
                local.AmountDue2.Text9 = "";
                local.WorkArea.Text15 =
                  Substring(local.AmountDue3.Text9, local.Start.Count, 9 -
                  local.Start.Count);
                local.Cents.TotalCurrency =
                  StringToNumber(local.WorkArea.Text15);

                if (local.Current.Count <= 2)
                {
                  local.Cents.TotalCurrency = local.Cents.TotalCurrency * 10;
                }
              }
              else
              {
                local.Cents.TotalCurrency = 0;
              }
            }
            else
            {
              local.Dollars.TotalCurrency =
                StringToNumber(local.WorkArea.Text15);
            }
          }
          else if (local.Current.Count > 1)
          {
            local.WorkArea.Text15 =
              Substring(import.AmountDue.Text9, local.Start.Count +
              0, local.Current.Count - 1);
            local.Cents.TotalCurrency = StringToNumber(local.WorkArea.Text15);

            if (local.Current.Count == 2)
            {
              local.Cents.TotalCurrency = local.Cents.TotalCurrency * 10;
            }
            else
            {
            }
          }
          else
          {
            local.Cents.TotalCurrency = 0;
          }

          if (local.Cents.TotalCurrency > 0)
          {
            local.Cents.TotalCurrency = local.Cents.TotalCurrency / 100;
          }

          break;
        }

        ++local.CurrentPosition.Count;
        ++local.Current.Count;
      }
      while(!Equal(global.Command, "COMMAND"));

      export.AmountDue.Text9 = import.AmountDue.Text9;
      local.AmountDueCommon.TotalCurrency = local.Dollars.TotalCurrency + local
        .Cents.TotalCurrency;
      export.KsDriversLicense.AmountDue = local.AmountDueCommon.TotalCurrency;
    }
    else
    {
      export.AmountDue.Text9 = "";
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

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
      export.CsePersonsWorkSet.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
        (10);

      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
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
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // *** Check security only if CRUD action is being performed.
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
    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    if (!Equal(global.Command, "LIST"))
    {
      export.PromptAppealDecsionCod.SelectChar = "";
      export.PromptCsePersonNumber.SelectChar = "";
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DISPLAY"))
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        if (!Equal(export.CsePersonsWorkSet.Number,
          import.HiddenPrevCsePersonsWorkSet.Number) && !
          IsEmpty(import.HiddenPrevCsePersonsWorkSet.Number))
        {
          export.LegalAction.Assign(local.InitialisedLegalAction);
          export.HiddenPrevLegalAction.Assign(local.InitialisedLegalAction);
          MoveCsePersonsWorkSet(local.InitialisedCsePersonsWorkSet,
            export.HiddenPrevCsePersonsWorkSet);
          export.CsePersonsWorkSet.Assign(local.InitialisedCsePersonsWorkSet);
          export.KsDriversLicense.Assign(local.InitialisedKsDriversLicense);
          export.MoreLessScroll.Text3 = "";
          export.LastUpdate.Date = local.NullDate.Date;
        }

        if (import.FlowKsDriversLicense.SequenceCounter > 0)
        {
          export.KsDriversLicense.SequenceCounter =
            import.FlowKsDriversLicense.SequenceCounter;
        }
      }

      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;

      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        // either the cse person number needs to be entered or when the program 
        // is flowed
        // to from a another program then it needs to bring the cse person 
        // number with it
        if (Equal(global.Command, "UPDATE"))
        {
          ExitState = "FN0000_DISPALY_ACTIVE_RECORD_UPD";

          return;
        }

        // prompt for the cse person number
        ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        return;
      }
      else
      {
        UseSiReadCsePerson();
      }

      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
        ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";

        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        if (Equal(global.Command, "DISPLAY"))
        {
          export.CsePersonsWorkSet.Assign(local.InitialisedCsePersonsWorkSet);
          export.LegalAction.Assign(local.InitialisedLegalAction);
          export.KsDriversLicense.Assign(local.InitialisedKsDriversLicense);
          export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
          export.LegalAction.Identifier = import.LegalAction.Identifier;
          export.KsDriversLicense.SequenceCounter =
            import.KsDriversLicense.SequenceCounter;
          MoveCsePersonsWorkSet(export.CsePersonsWorkSet,
            export.HiddenPrevCsePersonsWorkSet);
          export.HiddenPrevLegalAction.Assign(export.LegalAction);

          // move export initialised  ks driver's license to export hidden ks 
          // driver's license
        }

        return;
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        var field = GetField(export.CsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

        return;
      }

      if (!IsEmpty(export.LegalAction.StandardNumber) || export
        .KsDriversLicense.SequenceCounter > 0 || export
        .LegalAction.Identifier > 0)
      {
        // if the ap's number came in with a either a court order number or a 
        // squence number then it does not matter if there are orther records we
        // only care about the selcted court order
        if (!IsEmpty(export.LegalAction.StandardNumber))
        {
          // we will need to the legal action table by the legal id stored on ks
          // driver's lincense
          //  table, so that we can display the standard number on the screen
          if (ReadLegalAction3())
          {
            export.LegalAction.Assign(entities.ExistingLegalAction);
          }
          else
          {
            ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

            var field = GetField(export.LegalAction, "standardNumber");

            field.Error = true;

            return;
          }

          ReadKsDriversLicense8();

          if (!entities.KsDriversLicense.Populated)
          {
            var field1 = GetField(export.LegalAction, "standardNumber");

            field1.Error = true;

            var field2 = GetField(export.CsePersonsWorkSet, "number");

            field2.Error = true;

            ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";

            return;
          }
        }
        else if (export.LegalAction.Identifier > 0)
        {
          // we will need to the legal action table by the legal id stored on ks
          // driver's lincense
          //  table, so that we can display the standard number on the screen
          if (ReadLegalAction4())
          {
            export.LegalAction.Assign(entities.ExistingLegalAction);
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";

            var field = GetField(export.LegalAction, "standardNumber");

            field.Error = true;

            return;
          }

          ReadKsDriversLicense9();

          if (!entities.KsDriversLicense.Populated)
          {
            var field1 = GetField(export.LegalAction, "standardNumber");

            field1.Error = true;

            var field2 = GetField(export.CsePersonsWorkSet, "number");

            field2.Error = true;

            ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";

            return;
          }
        }
        else
        {
          ReadKsDriversLicense7();

          if (!entities.KsDriversLicense.Populated)
          {
            if (ReadCsePerson())
            {
              ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";
            }
            else
            {
              ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";
            }

            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }
        }
      }
      else
      {
        // since no legal action id was passed in then the program will have to 
        // see if here is
        // more than active court order that is tied to the ap that was passd 
        // in, if there is
        // more than one the process then we will flow back to kdvl screen so 
        // that a
        // worker  can select a court order.
        foreach(var item in ReadKsDriversLicenseLegalAction3())
        {
          if (!Equal(entities.ExistingLegalAction.StandardNumber,
            local.Hold.StandardNumber) && !IsEmpty(local.Hold.StandardNumber))
          {
            global.Command = "DMVL";

            break;
          }

          local.Hold.StandardNumber =
            entities.ExistingLegalAction.StandardNumber;
          ++local.NumberOfCtOrders.Count;
        }

        if (local.NumberOfCtOrders.Count <= 0)
        {
          // if there are no ks driver's license we need to determine why
          if (ReadCsePerson())
          {
            ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";
          }
          else
          {
            ExitState = "FN0000_PERSON_NUMBER_NOT_FOUND";
          }

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        break;
      case "RETURN":
        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
        }
        else
        {
          export.Flow.Number = export.HiddenPrevCsePersonsWorkSet.Number;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DMVL":
        ExitState = "ECO_LNK_DMVL_MOTOR_VECHICLE_LIST";

        // THIS IS A  NEW EXITSTATE TO FLOW TO DMVL SCREEN
        break;
      case "OBLO":
        if (IsEmpty(import.CsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

          return;
        }

        if (!Equal(import.CsePersonsWorkSet.Number,
          import.HiddenPrevCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        ExitState = "ECO_XFR_TO_OBL_ADM_ACT_BY_OBLGR";

        break;
      case "OPAY":
        if (IsEmpty(import.CsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

          return;
        }

        if (!Equal(import.CsePersonsWorkSet.Number,
          import.HiddenPrevCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        ExitState = "ECO_LNK_LST_OPAY_OBLG_BY_AP";

        break;
      case "PAYR":
        if (IsEmpty(import.CsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

          return;
        }

        if (!Equal(import.CsePersonsWorkSet.Number,
          import.HiddenPrevCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (ReadCsePerson())
        {
          export.CsePerson.Number = entities.CsePerson.Number;
        }

        export.CashReceiptDetail.CourtOrderNumber =
          export.LegalAction.StandardNumber ?? "";
        ExitState = "ECO_XFR_TO_PAYR";

        break;
      case "RETCDVL":
        if (IsEmpty(import.CsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

          return;
        }

        if (!Equal(import.CsePersonsWorkSet.Number,
          import.HiddenPrevCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (IsEmpty(import.DlgflwSelected.Cdvalue))
        {
        }
        else if (Equal(import.DlgflwSelected.Cdvalue, "O") || Equal
          (import.DlgflwSelected.Cdvalue, "S") || Equal
          (import.DlgflwSelected.Cdvalue, "W") || Equal
          (import.DlgflwSelected.Cdvalue, "N"))
        {
          export.KsDriversLicense.AppealResolved =
            import.DlgflwSelected.Cdvalue;
          export.PromptAppealDecsionCod.SelectChar = "";
          global.Command = "DISPLAY";
        }

        break;
      case "DISPLAY":
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;

        if (!IsEmpty(import.HiddenPrevCsePersonsWorkSet.Number) || !
          IsEmpty(import.HiddenPrevLegalAction.StandardNumber))
        {
          if (!Equal(export.CsePersonsWorkSet.Number,
            import.HiddenPrevCsePersonsWorkSet.Number) || !
            Equal(export.LegalAction.StandardNumber,
            import.HiddenPrevLegalAction.CourtCaseNumber))
          {
            export.LegalAction.Assign(local.InitialisedLegalAction);
          }
        }

        // This is display logic
        // Read each ks driver's license
        //     sorted desending legal action id
        //     sorted desending last validate date
        //     where cse person number = import cse person number
        //     and legal id = > local legal id
        // need to know if there are any more records for this ap and court 
        // order, if there are then set the more flag to '+'
        if (export.KsDriversLicense.SequenceCounter > 0)
        {
          if (ReadKsDriversLicenseCsePerson())
          {
            if (ReadLegalAction6())
            {
              export.KsDriversLicense.Assign(entities.KsDriversLicense);

              if (ReadKsDriversLicense5())
              {
                export.MoreLessScroll.Text3 = "+";
              }

              if (ReadKsDriversLicense3())
              {
                local.HistoricRec.Flag = "Y";

                if (Equal(export.MoreLessScroll.Text3, "+"))
                {
                  export.MoreLessScroll.Text3 = "+ -";
                }
                else
                {
                  export.MoreLessScroll.Text3 = "  -";
                }
              }
            }
            else
            {
              ExitState = "LEGAL_ACTION_NF";

              var field = GetField(export.LegalAction, "standardNumber");

              field.Error = true;

              return;
            }
          }
          else
          {
            ExitState = "KS_DRIVERS_LICENSE_NF";

            var field15 = GetField(export.CsePersonsWorkSet, "number");

            field15.Error = true;

            var field16 = GetField(export.KsDriversLicense, "ksDvrLicense");

            field16.Error = true;

            return;
          }
        }
        else if (!IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          foreach(var item in ReadKsDriversLicense15())
          {
            ++local.RecordCounter.Count;

            if (local.RecordCounter.Count > 1)
            {
              export.MoreLessScroll.Text3 = "+";

              break;
            }

            export.KsDriversLicense.Assign(entities.KsDriversLicense);
          }

          if (ReadKsDriversLicense4())
          {
            if (Equal(export.MoreLessScroll.Text3, "+"))
            {
              export.MoreLessScroll.Text3 = "+ -";
            }
            else
            {
              export.MoreLessScroll.Text3 = "  -";
            }

            local.HistoricRec.Flag = "Y";
          }
        }
        else if (export.LegalAction.Identifier > 0)
        {
          foreach(var item in ReadKsDriversLicense16())
          {
            ++local.RecordCounter.Count;

            if (local.RecordCounter.Count > 1)
            {
              export.MoreLessScroll.Text3 = "+";

              break;
            }

            export.KsDriversLicense.Assign(entities.KsDriversLicense);
          }

          if (ReadKsDriversLicense2())
          {
            if (Equal(export.MoreLessScroll.Text3, "+"))
            {
              export.MoreLessScroll.Text3 = "+ -";
            }
            else
            {
              export.MoreLessScroll.Text3 = "  -";
            }

            local.HistoricRec.Flag = "Y";
          }
        }
        else
        {
          foreach(var item in ReadKsDriversLicense17())
          {
            if (ReadLegalAction6())
            {
              ++local.RecordCounter.Count;

              if (local.RecordCounter.Count > 1)
              {
                export.MoreLessScroll.Text3 = "+";

                break;
              }

              export.KsDriversLicense.Assign(entities.KsDriversLicense);
            }
            else
            {
              // we do not want to count shell records, that is ks driver's 
              // license records that have not court orders tied to them
            }
          }

          // since there  is no read by court order number, that means there is 
          // only one court
          // order number tied to this ap as far as the driver's license 
          // restriction process is
          // concern
        }

        if (export.KsDriversLicense.SequenceCounter <= 0)
        {
          ExitState = "KS_DRIVERS_LICENSE_NF";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }

        if (ReadLegalAction1())
        {
          export.LegalAction.Assign(entities.ExistingLegalAction);
        }
        else
        {
          ExitState = "NO_DATA_EXISTS_FOR_PERSON_NUMBER";

          return;
        }

        if (export.KsDriversLicense.AmountDue.GetValueOrDefault() > 0)
        {
          local.AmountDueWorkArea.Text15 =
            NumberToString((long)(export.KsDriversLicense.AmountDue.
              GetValueOrDefault() * 100), 15);
          local.Current.Count = 1;
          local.CurrentPosition.Count = 1;

          do
          {
            local.Postion.Text1 =
              Substring(local.AmountDueWorkArea.Text15,
              local.CurrentPosition.Count, 1);

            if (AsChar(local.Postion.Text1) == '0')
            {
              local.Start.Count = local.CurrentPosition.Count + 1;
            }
            else
            {
              local.WorkArea.Text15 = "";
              local.WorkArea.Text15 =
                Substring(local.AmountDueWorkArea.Text15, local.Current.Count,
                16 - local.Current.Count - 2);
              local.WorkArea.Text2 =
                Substring(local.AmountDueWorkArea.Text15, 14, 2);
              local.AmountDueWorkArea.Text15 = "";
              local.AmountDueWorkArea.Text15 =
                TrimEnd(local.WorkArea.Text15) + "." + local.WorkArea.Text2;
              export.AmountDue.Text9 = local.AmountDueWorkArea.Text15;

              goto Test1;
            }

            ++local.CurrentPosition.Count;
            ++local.Current.Count;
          }
          while(!Equal(global.Command, "COMMAND"));
        }
        else
        {
          export.AmountDue.Text9 = "";
        }

Test1:

        if (!IsEmpty(export.KsDriversLicense.RecordClosureReason) || Equal
          (export.KsDriversLicense.RestrictionStatus, "REINSTATED") || Equal
          (export.KsDriversLicense.RestrictionStatus, "DENIED RESTRICTION") || Equal
          (export.KsDriversLicense.RestrictionStatus, "REINSTATE MANUALLY"))
        {
          local.HistoricRec.Flag = "Y";
        }

        // unprotect and protec feilds as needed
        if (Lt(local.NullDate.Date,
          export.KsDriversLicense.Attribute30DayLetterCreatedDate) && AsChar
          (local.HistoricRec.Flag) != 'Y')
        {
          if (!Lt(local.NullDate.Date,
            export.KsDriversLicense.AppealReceivedDate))
          {
            var field15 =
              GetField(export.KsDriversLicense, "appealReceivedDate");

            field15.Color = "";
            field15.Protected = false;

            var field16 = GetField(export.KsDriversLicense, "appealResolved");

            field16.Protected = false;

            var field17 = GetField(export.PromptAppealDecsionCod, "selectChar");

            field17.Protected = false;
          }
          else
          {
            var field = GetField(export.KsDriversLicense, "appealReceivedDate");

            field.Color = "cyan";
            field.Protected = true;

            if (IsEmpty(export.KsDriversLicense.AppealResolved))
            {
              var field15 = GetField(export.KsDriversLicense, "appealResolved");

              field15.Color = "";
              field15.Protected = false;

              var field16 =
                GetField(export.PromptAppealDecsionCod, "selectChar");

              field16.Color = "";
              field16.Protected = false;
            }
            else
            {
              var field15 = GetField(export.KsDriversLicense, "appealResolved");

              field15.Color = "cyan";
              field15.Protected = true;

              var field16 =
                GetField(export.PromptAppealDecsionCod, "selectChar");

              field16.Color = "cyan";
              field16.Protected = true;
            }
          }

          if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N' || IsEmpty
            (export.KsDriversLicense.ServiceCompleteInd))
          {
            // if the service complete flag is 'Y' set the service complete 
            // field  to protected
            // if the service complete flag is 'N' do not set the service 
            // complete field  to protected
            // if the service complete date is completed set it to protected
            var field15 =
              GetField(export.KsDriversLicense, "serviceCompleteInd");

            field15.Color = "";
            field15.Protected = false;

            var field16 =
              GetField(export.KsDriversLicense, "serviceCompleteDate");

            field16.Color = "";
            field16.Protected = false;
          }
          else
          {
            var field15 =
              GetField(export.KsDriversLicense, "serviceCompleteInd");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 =
              GetField(export.KsDriversLicense, "serviceCompleteDate");

            field16.Color = "cyan";
            field16.Protected = true;
          }

          if (!Lt(local.NullDate.Date,
            export.KsDriversLicense.PaymentAgreementDate))
          {
            var field15 =
              GetField(export.KsDriversLicense, "paymentAgreementDate");

            field15.Color = "";
            field15.Protected = false;

            var field16 =
              GetField(export.KsDriversLicense, "firstPaymentDueDate");

            field16.Color = "";
            field16.Protected = false;

            var field17 = GetField(export.AmountDue, "text9");

            field17.Color = "";
            field17.Protected = false;
          }
          else
          {
            // If the payment agreement date is greater than a null date then 
            // protect the payment agreement date.
            var field15 =
              GetField(export.KsDriversLicense, "paymentAgreementDate");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 =
              GetField(export.KsDriversLicense, "firstPaymentDueDate");

            field16.Protected = false;

            var field17 = GetField(export.AmountDue, "text9");

            field17.Protected = false;
          }
        }

        export.HistoricRecord.Flag = "";

        if (AsChar(local.HistoricRec.Flag) == 'Y' || AsChar
          (export.KsDriversLicense.AppealResolved) == 'O')
        {
          export.HistoricRecord.Flag = "Y";

          var field15 = GetField(export.KsDriversLicense, "note1");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.KsDriversLicense, "note2");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.KsDriversLicense, "note3");

          field17.Color = "cyan";
          field17.Protected = true;

          var field18 =
            GetField(export.KsDriversLicense, "paymentAgreementDate");

          field18.Color = "cyan";
          field18.Protected = true;

          var field19 =
            GetField(export.KsDriversLicense, "firstPaymentDueDate");

          field19.Color = "cyan";
          field19.Protected = true;

          var field20 = GetField(export.AmountDue, "text9");

          field20.Color = "cyan";
          field20.Protected = true;

          var field21 = GetField(export.KsDriversLicense, "serviceCompleteInd");

          field21.Color = "cyan";
          field21.Protected = true;

          var field22 =
            GetField(export.KsDriversLicense, "serviceCompleteDate");

          field22.Color = "cyan";
          field22.Protected = true;

          var field23 = GetField(export.KsDriversLicense, "appealReceivedDate");

          field23.Color = "cyan";
          field23.Protected = true;

          var field24 = GetField(export.KsDriversLicense, "appealResolved");

          field24.Color = "cyan";
          field24.Protected = true;

          var field25 = GetField(export.PromptAppealDecsionCod, "selectChar");

          field25.Color = "cyan";
          field25.Protected = true;
        }

        if (!Lt(local.NullDate.Date,
          export.KsDriversLicense.PaymentAgreementDate))
        {
          export.KsDriversLicense.FirstPaymentDueDate =
            local.InitialisedKsDriversLicense.FirstPaymentDueDate;
        }

        export.LastUpdate.Date =
          Date(export.KsDriversLicense.LastUpdatedTstamp);
        export.UpdateConfirm.Count = 0;
        export.HiddenDisplayPerformed.Flag = "Y";
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        // ---------------------------------------------
        // Move all exports to export hidden previous
        // ---------------------------------------------
        export.HiddenPrevLegalAction.Assign(export.LegalAction);
        MoveCsePersonsWorkSet(export.CsePersonsWorkSet,
          export.HiddenPrevCsePersonsWorkSet);
        export.HiddenKsDriversLicense.SequenceCounter =
          export.KsDriversLicense.SequenceCounter;

        // move export ks driver's license  to export hidden  ks driver's 
        // license
        break;
      case "LIST":
        // first check that ''S" used as the selectioon character
        local.PromptCount.Count = 0;

        if (!IsEmpty(export.PromptAppealDecsionCod.SelectChar) && AsChar
          (export.PromptAppealDecsionCod.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptAppealDecsionCod, "selectChar");

          field.Error = true;

          break;
        }

        if (!IsEmpty(export.PromptCsePersonNumber.SelectChar) && AsChar
          (export.PromptCsePersonNumber.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptCsePersonNumber, "selectChar");

          field.Error = true;

          break;
        }

        // prompt for ap person number - send to NAME screen
        if (AsChar(export.PromptCsePersonNumber.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_SELECT_PERSON";
          ++local.PromptCount.Count;
        }

        // prompt appeal decision code - send to code table
        if (AsChar(export.PromptAppealDecsionCod.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
          export.DlgflwRequired.CodeName = "APPEAL DECISION";
          ++local.PromptCount.Count;
        }

        if (local.PromptCount.Count == 1)
        {
          if (AsChar(export.PromptCsePersonNumber.SelectChar) == 'S')
          {
          }
        }
        else if (local.PromptCount.Count > 1)
        {
          if (AsChar(export.PromptAppealDecsionCod.SelectChar) == 'S')
          {
            var field = GetField(export.PromptAppealDecsionCod, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.PromptCsePersonNumber.SelectChar) == 'S')
          {
            var field = GetField(export.PromptCsePersonNumber, "selectChar");

            field.Error = true;
          }

          ExitState = "ZD_ACO_NE0_INVALID_MULT_PROMPT_S";
        }
        else
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        break;
      case "UPDATE":
        local.LastErrorEntryNo.Count = 0;

        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (AsChar(export.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (!Equal(export.CsePersonsWorkSet.Number,
          export.HiddenPrevCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";
          export.UpdateConfirm.Count = 0;

          break;
        }

        if (AsChar(import.HistoricRecord.Flag) == 'Y')
        {
          // can not update a history record, nothing should be enterable to 
          // even update
          ExitState = "CAN_NOT_UPDATE_A_HISTORY_REC";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          return;
        }

        local.ServiceCompletedCheck.Flag = "";
        local.UserAction.Command = global.Command;
        local.CsePerson.Number = export.CsePersonsWorkSet.Number;

        if (ReadKsDriversLicense6())
        {
          if (Lt(local.NullDate.Date,
            entities.KsDriversLicense.Attribute30DayLetterCreatedDate))
          {
            if (IsEmpty(entities.KsDriversLicense.ServiceCompleteInd))
            {
              if (AsChar(import.KsDriversLicense.ServiceCompleteInd) == 'Y')
              {
                if (Lt(local.NullDate.Date,
                  import.KsDriversLicense.ServiceCompleteDate))
                {
                  if (Lt(import.KsDriversLicense.
                    Attribute30DayLetterCreatedDate,
                    import.KsDriversLicense.ServiceCompleteDate) && !
                    Lt(Now().Date, import.KsDriversLicense.ServiceCompleteDate))
                  {
                    // this is ok , as it is should be
                    local.ServiceCompletedCheck.Flag = "Y";
                  }
                  else
                  {
                    var field15 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field15.Error = true;

                    ExitState = "DATE_MUST_BE_LESS_CURRENT_AND_GR";

                    if (!Lt(local.NullDate.Date,
                      export.KsDriversLicense.AppealReceivedDate))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field17.Protected = false;

                      var field18 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field18.Protected = false;

                      var field19 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field19.Protected = false;
                    }
                    else
                    {
                      var field =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field.Color = "cyan";
                      field.Protected = true;

                      if (IsEmpty(export.KsDriversLicense.AppealResolved))
                      {
                        var field17 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field17.Protected = false;

                        var field18 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field18.Protected = false;
                      }
                      else
                      {
                        var field17 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field17.Color = "cyan";
                        field17.Protected = true;

                        var field18 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field18.Color = "cyan";
                        field18.Protected = true;
                      }
                    }

                    var field16 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field16.Protected = false;

                    if (!Lt(local.NullDate.Date,
                      export.KsDriversLicense.PaymentAgreementDate))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "paymentAgreementDate");
                        

                      field17.Protected = false;

                      var field18 =
                        GetField(export.KsDriversLicense, "firstPaymentDueDate");
                        

                      field18.Protected = false;

                      var field19 = GetField(export.AmountDue, "text9");

                      field19.Protected = false;
                    }
                    else
                    {
                      // If the payment agreement date is greater than a null 
                      // date then protect the payment agreement date.
                      var field17 =
                        GetField(export.KsDriversLicense, "paymentAgreementDate");
                        

                      field17.Color = "cyan";
                      field17.Protected = true;

                      var field18 =
                        GetField(export.KsDriversLicense, "firstPaymentDueDate");
                        

                      field18.Protected = false;

                      var field19 = GetField(export.AmountDue, "text9");

                      field19.Protected = false;
                    }

                    return;
                  }
                }
                else
                {
                  var field15 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field15.Error = true;

                  ExitState = "SERVICE_COMPLETE_DATE_MUST_EXIST";

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.AppealReceivedDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field.Color = "cyan";
                    field.Protected = true;

                    if (IsEmpty(export.KsDriversLicense.AppealResolved))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field17.Protected = false;

                      var field18 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field18.Protected = false;
                    }
                    else
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field17.Color = "cyan";
                      field17.Protected = true;

                      var field18 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field18.Color = "cyan";
                      field18.Protected = true;
                    }
                  }

                  var field16 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field16.Protected = false;

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.PaymentAgreementDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field18.Protected = false;

                    var field19 = GetField(export.AmountDue, "text9");

                    field19.Protected = false;
                  }
                  else
                  {
                    // If the payment agreement date is greater than a null date
                    // then protect the payment agreement date.
                    var field17 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field17.Color = "cyan";
                    field17.Protected = true;

                    var field18 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field18.Protected = false;

                    var field19 = GetField(export.AmountDue, "text9");

                    field19.Protected = false;
                  }

                  return;
                }
              }
              else if (IsEmpty(import.KsDriversLicense.ServiceCompleteInd) || AsChar
                (import.KsDriversLicense.ServiceCompleteInd) == 'N')
              {
                if (AsChar(import.KsDriversLicense.ServiceCompleteInd) == 'N')
                {
                  if (Lt(import.KsDriversLicense.
                    Attribute30DayLetterCreatedDate,
                    import.KsDriversLicense.ServiceCompleteDate) && !
                    Lt(Now().Date, import.KsDriversLicense.ServiceCompleteDate))
                  {
                    // this is ok , as it is should be
                    local.ServiceCompletedCheck.Flag = "Y";
                  }
                  else
                  {
                    if (Lt(local.NullDate.Date,
                      import.KsDriversLicense.ServiceCompleteDate))
                    {
                      ExitState = "DATE_MUST_BE_LESS_CURRENT_AND_GR";
                    }
                    else
                    {
                      ExitState = "SERVICE_COMPLETE_DATE_MUST_EXIST";
                    }

                    var field15 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field15.Error = true;

                    if (!Lt(local.NullDate.Date,
                      export.KsDriversLicense.AppealReceivedDate))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field17.Protected = false;

                      var field18 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field18.Protected = false;

                      var field19 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field19.Protected = false;
                    }
                    else
                    {
                      var field =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field.Color = "cyan";
                      field.Protected = true;

                      if (IsEmpty(export.KsDriversLicense.AppealResolved))
                      {
                        var field17 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field17.Protected = false;

                        var field18 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field18.Protected = false;
                      }
                      else
                      {
                        var field17 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field17.Color = "cyan";
                        field17.Protected = true;

                        var field18 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field18.Color = "cyan";
                        field18.Protected = true;
                      }
                    }

                    var field16 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field16.Protected = false;

                    if (!Lt(local.NullDate.Date,
                      export.KsDriversLicense.PaymentAgreementDate))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "paymentAgreementDate");
                        

                      field17.Protected = false;

                      var field18 =
                        GetField(export.KsDriversLicense, "firstPaymentDueDate");
                        

                      field18.Protected = false;

                      var field19 = GetField(export.AmountDue, "text9");

                      field19.Protected = false;
                    }
                    else
                    {
                      // If the payment agreement date is greater than a null 
                      // date then protect the payment agreement date.
                      var field17 =
                        GetField(export.KsDriversLicense, "paymentAgreementDate");
                        

                      field17.Color = "cyan";
                      field17.Protected = true;

                      var field18 =
                        GetField(export.KsDriversLicense, "firstPaymentDueDate");
                        

                      field18.Protected = false;

                      var field19 = GetField(export.AmountDue, "text9");

                      field19.Protected = false;
                    }

                    return;
                  }
                }
                else if (Lt(local.NullDate.Date,
                  import.KsDriversLicense.ServiceCompleteDate))
                {
                  var field15 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field15.Error = true;

                  ExitState = "SERVICE_COMPLETE_IND_MUST_EXIST";

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.AppealReceivedDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field.Color = "cyan";
                    field.Protected = true;

                    if (IsEmpty(export.KsDriversLicense.AppealResolved))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field17.Protected = false;

                      var field18 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field18.Protected = false;
                    }
                    else
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field17.Color = "cyan";
                      field17.Protected = true;

                      var field18 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field18.Color = "cyan";
                      field18.Protected = true;
                    }
                  }

                  var field16 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field16.Protected = false;

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.PaymentAgreementDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field18.Protected = false;

                    var field19 = GetField(export.AmountDue, "text9");

                    field19.Protected = false;
                  }
                  else
                  {
                    // If the payment agreement date is greater than a null date
                    // then protect the payment agreement date.
                    var field17 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field17.Color = "cyan";
                    field17.Protected = true;

                    var field18 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field18.Protected = false;

                    var field19 = GetField(export.AmountDue, "text9");

                    field19.Protected = false;
                  }

                  return;
                }
              }
              else
              {
                var field15 =
                  GetField(export.KsDriversLicense, "serviceCompleteInd");

                field15.Error = true;

                ExitState = "VALID_CHARACTERS_ARE_Y_OR_N";

                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.AppealReceivedDate))
                {
                  var field17 =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field17.Protected = false;

                  var field18 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field19.Protected = false;
                }
                else
                {
                  var field =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field.Color = "cyan";
                  field.Protected = true;

                  if (IsEmpty(export.KsDriversLicense.AppealResolved))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field17.Protected = false;

                    var field18 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field18.Protected = false;
                  }
                  else
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field17.Color = "cyan";
                    field17.Protected = true;

                    var field18 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field18.Color = "cyan";
                    field18.Protected = true;
                  }
                }

                var field16 =
                  GetField(export.KsDriversLicense, "serviceCompleteDate");

                field16.Protected = false;

                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.PaymentAgreementDate))
                {
                  var field17 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field17.Protected = false;

                  var field18 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field18.Protected = false;

                  var field19 = GetField(export.AmountDue, "text9");

                  field19.Protected = false;
                }
                else
                {
                  // If the payment agreement date is greater than a null date 
                  // then protect the payment agreement date.
                  var field17 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field17.Color = "cyan";
                  field17.Protected = true;

                  var field18 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field18.Protected = false;

                  var field19 = GetField(export.AmountDue, "text9");

                  field19.Protected = false;
                }

                return;
              }
            }
            else if (AsChar(import.KsDriversLicense.ServiceCompleteInd) == 'Y')
            {
              if (Lt(local.NullDate.Date,
                import.KsDriversLicense.ServiceCompleteDate))
              {
                if (Lt(import.KsDriversLicense.Attribute30DayLetterCreatedDate,
                  import.KsDriversLicense.ServiceCompleteDate) && !
                  Lt(Now().Date, import.KsDriversLicense.ServiceCompleteDate))
                {
                  // this is ok , as it is should be
                  local.ServiceCompletedCheck.Flag = "Y";
                }
                else
                {
                  var field15 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field15.Error = true;

                  ExitState = "DATE_MUST_BE_LESS_CURRENT_AND_GR";

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.AppealReceivedDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field.Color = "cyan";
                    field.Protected = true;

                    if (IsEmpty(export.KsDriversLicense.AppealResolved))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field17.Protected = false;

                      var field18 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field18.Protected = false;
                    }
                    else
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field17.Color = "cyan";
                      field17.Protected = true;

                      var field18 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field18.Color = "cyan";
                      field18.Protected = true;
                    }
                  }

                  var field16 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field16.Protected = false;

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.PaymentAgreementDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field18.Protected = false;

                    var field19 = GetField(export.AmountDue, "text9");

                    field19.Protected = false;
                  }
                  else
                  {
                    // If the payment agreement date is greater than a null date
                    // then protect the payment agreement date.
                    var field17 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field17.Color = "cyan";
                    field17.Protected = true;

                    var field18 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field18.Protected = false;

                    var field19 = GetField(export.AmountDue, "text9");

                    field19.Protected = false;
                  }

                  return;
                }
              }
              else
              {
                var field15 =
                  GetField(export.KsDriversLicense, "serviceCompleteDate");

                field15.Error = true;

                ExitState = "SERVICE_COMPLETE_DATE_MUST_EXIST";

                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.AppealReceivedDate))
                {
                  var field17 =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field17.Protected = false;

                  var field18 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field19.Protected = false;
                }
                else
                {
                  var field =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field.Color = "cyan";
                  field.Protected = true;

                  if (IsEmpty(export.KsDriversLicense.AppealResolved))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field17.Protected = false;

                    var field18 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field18.Protected = false;
                  }
                  else
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field17.Color = "cyan";
                    field17.Protected = true;

                    var field18 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field18.Color = "cyan";
                    field18.Protected = true;
                  }
                }

                var field16 =
                  GetField(export.KsDriversLicense, "serviceCompleteInd");

                field16.Protected = false;

                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.PaymentAgreementDate))
                {
                  var field17 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field17.Protected = false;

                  var field18 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field18.Protected = false;

                  var field19 = GetField(export.AmountDue, "text9");

                  field19.Protected = false;
                }
                else
                {
                  // If the payment agreement date is greater than a null date 
                  // then protect the payment agreement date.
                  var field17 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field17.Color = "cyan";
                  field17.Protected = true;

                  var field18 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field18.Protected = false;

                  var field19 = GetField(export.AmountDue, "text9");

                  field19.Protected = false;
                }

                return;
              }
            }
            else if (IsEmpty(import.KsDriversLicense.ServiceCompleteInd) || AsChar
              (import.KsDriversLicense.ServiceCompleteInd) == 'N')
            {
              if (AsChar(import.KsDriversLicense.ServiceCompleteInd) == 'N')
              {
                if (Lt(import.KsDriversLicense.Attribute30DayLetterCreatedDate,
                  import.KsDriversLicense.ServiceCompleteDate) && !
                  Lt(Now().Date, import.KsDriversLicense.ServiceCompleteDate))
                {
                  // this is ok , as it is should be
                  local.ServiceCompletedCheck.Flag = "Y";
                }
                else
                {
                  if (Lt(local.NullDate.Date,
                    import.KsDriversLicense.ServiceCompleteDate))
                  {
                    ExitState = "DATE_MUST_BE_LESS_CURRENT_AND_GR";
                  }
                  else
                  {
                    ExitState = "SERVICE_COMPLETE_DATE_MUST_EXIST";
                  }

                  var field15 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field15.Error = true;

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.AppealReceivedDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field.Color = "cyan";
                    field.Protected = true;

                    if (IsEmpty(export.KsDriversLicense.AppealResolved))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field17.Protected = false;

                      var field18 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field18.Protected = false;
                    }
                    else
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field17.Color = "cyan";
                      field17.Protected = true;

                      var field18 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field18.Color = "cyan";
                      field18.Protected = true;
                    }
                  }

                  var field16 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field16.Protected = false;

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.PaymentAgreementDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field18.Protected = false;

                    var field19 = GetField(export.AmountDue, "text9");

                    field19.Protected = false;
                  }
                  else
                  {
                    // If the payment agreement date is greater than a null date
                    // then protect the payment agreement date.
                    var field17 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field17.Color = "cyan";
                    field17.Protected = true;

                    var field18 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field18.Protected = false;

                    var field19 = GetField(export.AmountDue, "text9");

                    field19.Protected = false;
                  }

                  return;
                }
              }
              else if (Lt(local.NullDate.Date,
                import.KsDriversLicense.ServiceCompleteDate))
              {
                var field15 =
                  GetField(export.KsDriversLicense, "serviceCompleteInd");

                field15.Error = true;

                ExitState = "SERVICE_COMPLETE_IND_MUST_EXIST";

                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.AppealReceivedDate))
                {
                  var field17 =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field17.Protected = false;

                  var field18 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field19.Protected = false;
                }
                else
                {
                  var field =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field.Color = "cyan";
                  field.Protected = true;

                  if (IsEmpty(export.KsDriversLicense.AppealResolved))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field17.Protected = false;

                    var field18 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field18.Protected = false;
                  }
                  else
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field17.Color = "cyan";
                    field17.Protected = true;

                    var field18 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field18.Color = "cyan";
                    field18.Protected = true;
                  }
                }

                var field16 =
                  GetField(export.KsDriversLicense, "serviceCompleteDate");

                field16.Protected = false;

                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.PaymentAgreementDate))
                {
                  var field17 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field17.Protected = false;

                  var field18 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field18.Protected = false;

                  var field19 = GetField(export.AmountDue, "text9");

                  field19.Protected = false;
                }
                else
                {
                  // If the payment agreement date is greater than a null date 
                  // then protect the payment agreement date.
                  var field17 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field17.Color = "cyan";
                  field17.Protected = true;

                  var field18 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field18.Protected = false;

                  var field19 = GetField(export.AmountDue, "text9");

                  field19.Protected = false;
                }

                return;
              }
            }
            else
            {
              var field15 =
                GetField(export.KsDriversLicense, "serviceCompleteInd");

              field15.Error = true;

              ExitState = "VALID_CHARACTERS_ARE_Y_OR_N";

              if (!Lt(local.NullDate.Date,
                export.KsDriversLicense.AppealReceivedDate))
              {
                var field17 =
                  GetField(export.KsDriversLicense, "appealReceivedDate");

                field17.Protected = false;

                var field18 =
                  GetField(export.KsDriversLicense, "appealResolved");

                field18.Protected = false;

                var field19 =
                  GetField(export.PromptAppealDecsionCod, "selectChar");

                field19.Protected = false;
              }
              else
              {
                var field =
                  GetField(export.KsDriversLicense, "appealReceivedDate");

                field.Color = "cyan";
                field.Protected = true;

                if (IsEmpty(export.KsDriversLicense.AppealResolved))
                {
                  var field17 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field17.Protected = false;

                  var field18 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field18.Protected = false;
                }
                else
                {
                  var field17 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field17.Color = "cyan";
                  field17.Protected = true;

                  var field18 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field18.Color = "cyan";
                  field18.Protected = true;
                }
              }

              var field16 =
                GetField(export.KsDriversLicense, "serviceCompleteDate");

              field16.Protected = false;

              if (!Lt(local.NullDate.Date,
                export.KsDriversLicense.PaymentAgreementDate))
              {
                var field17 =
                  GetField(export.KsDriversLicense, "paymentAgreementDate");

                field17.Protected = false;

                var field18 =
                  GetField(export.KsDriversLicense, "firstPaymentDueDate");

                field18.Protected = false;

                var field19 = GetField(export.AmountDue, "text9");

                field19.Protected = false;
              }
              else
              {
                // If the payment agreement date is greater than a null date 
                // then protect the payment agreement date.
                var field17 =
                  GetField(export.KsDriversLicense, "paymentAgreementDate");

                field17.Color = "cyan";
                field17.Protected = true;

                var field18 =
                  GetField(export.KsDriversLicense, "firstPaymentDueDate");

                field18.Protected = false;

                var field19 = GetField(export.AmountDue, "text9");

                field19.Protected = false;
              }

              return;
            }

            // If payment agreement date > null date
            //     if 30 day letter create date < = null date
            //        exit state is 30 day letter needs to be sent before an 
            // payment agreement
            //             can be  made
            //         make error payment agreement date
            // <-------escape
            //     else
            //        if payment agreement date < = 30 day letter create date
            //           exit state is 30 day letter needs to be sent before an 
            // appeal can be made
            //            make error payment agreement date
            // <-------escape
            //        else
            //            if first pmt due date < = null date
            //               or amount due <= 0
            //                exit state is all payment info must be filled in
            //                 if first pmt due date < = null date
            //                   make error first pmt due date
            //                 end if
            //                 if amount due <= 0
            //                    make error amount due
            //                 end if
            // <------------escape
            //           end if
            //        end if
            //     end if
            // else
            //      if first pmt due date => null date
            //               or amount due => 0
            //       make error payment agreement date
            //       exit state is all payment info must be filled in
            //          if first pmt due date => null date
            //             make error amount due
            //          end if
            //         if amount due => 0
            //            make error first pmt due date
            //        end if
            // <------------escape
            // end if
            if (Lt(local.NullDate.Date,
              import.KsDriversLicense.PaymentAgreementDate))
            {
              if (Lt(import.KsDriversLicense.Attribute30DayLetterCreatedDate,
                import.KsDriversLicense.PaymentAgreementDate) && !
                Lt(Now().Date, import.KsDriversLicense.PaymentAgreementDate))
              {
                if (Lt(local.NullDate.Date,
                  import.KsDriversLicense.FirstPaymentDueDate) || Lt
                  (local.NullDate.Date,
                  entities.KsDriversLicense.FirstPaymentDueDate) || !
                  IsEmpty(import.AmountDue.Text9))
                {
                  if (!Lt(local.NullDate.Date,
                    import.KsDriversLicense.FirstPaymentDueDate) && !
                    Lt(local.NullDate.Date,
                    entities.KsDriversLicense.FirstPaymentDueDate))
                  {
                    var field15 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field15.Error = true;

                    ExitState = "PAYMENT_AGREEMENT_INFO_MISSING";

                    if (!Lt(local.NullDate.Date,
                      export.KsDriversLicense.AppealReceivedDate))
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field18.Protected = false;

                      var field19 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field19.Protected = false;

                      var field20 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field20.Protected = false;
                    }
                    else
                    {
                      var field =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field.Color = "cyan";
                      field.Protected = true;

                      if (IsEmpty(export.KsDriversLicense.AppealResolved))
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field18.Protected = false;

                        var field19 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field19.Protected = false;
                      }
                      else
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field18.Color = "cyan";
                        field18.Protected = true;

                        var field19 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field19.Color = "cyan";
                        field19.Protected = true;
                      }
                    }

                    if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                      || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                    {
                      // if the service complete flag is 'Y' set the service 
                      // complete field  to protected
                      // if the service complete flag is 'N' do not set the 
                      // service complete field  to protected
                      // if the service complete date is completed set it to 
                      // protected
                      var field18 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field18.Protected = false;

                      var field19 =
                        GetField(export.KsDriversLicense, "serviceCompleteDate");
                        

                      field19.Protected = false;
                    }
                    else
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field18.Color = "cyan";
                      field18.Protected = true;

                      var field19 =
                        GetField(export.KsDriversLicense, "serviceCompleteDate");
                        

                      field19.Protected = false;
                    }

                    var field16 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field16.Protected = false;

                    var field17 = GetField(export.AmountDue, "text9");

                    field17.Color = "";
                    field17.Protected = false;

                    return;
                  }

                  if (IsEmpty(import.AmountDue.Text9))
                  {
                    ExitState = "PAYMENT_AGREEMENT_INFO_MISSING";

                    var field15 = GetField(export.AmountDue, "text9");

                    field15.Error = true;

                    if (!Lt(local.NullDate.Date,
                      export.KsDriversLicense.AppealReceivedDate))
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field18.Protected = false;

                      var field19 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field19.Protected = false;

                      var field20 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field20.Protected = false;
                    }
                    else
                    {
                      var field =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field.Color = "cyan";
                      field.Protected = true;

                      if (IsEmpty(export.KsDriversLicense.AppealResolved))
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field18.Protected = false;

                        var field19 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field19.Protected = false;
                      }
                      else
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field18.Color = "cyan";
                        field18.Protected = true;

                        var field19 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field19.Color = "cyan";
                        field19.Protected = true;
                      }
                    }

                    if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                      || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                    {
                      // if the service complete flag is 'Y' set the service 
                      // complete field  to protected
                      // if the service complete flag is 'N' do not set the 
                      // service complete field  to protected
                      // if the service complete date is completed set it to 
                      // protected
                      var field18 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field18.Protected = false;

                      var field19 =
                        GetField(export.KsDriversLicense, "serviceCompleteDate");
                        

                      field19.Protected = false;
                    }
                    else
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field18.Color = "cyan";
                      field18.Protected = true;

                      var field19 =
                        GetField(export.KsDriversLicense, "serviceCompleteDate");
                        

                      field19.Color = "cyan";
                      field19.Protected = true;
                    }

                    var field16 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field16.Protected = false;

                    var field17 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field17.Protected = false;

                    return;
                  }
                  else if (Equal(import.AmountDue.Text9, "000000000") || Equal
                    (import.AmountDue.Text9, "000000.00") || Equal
                    (import.AmountDue.Text9, "00000.00") || Equal
                    (import.AmountDue.Text9, "0000.00") || Equal
                    (import.AmountDue.Text9, "000.00") || Equal
                    (import.AmountDue.Text9, "00.00") || Equal
                    (import.AmountDue.Text9, "0.00") || Equal
                    (import.AmountDue.Text9, ".00") || Equal
                    (import.AmountDue.Text9, ".0") || Equal
                    (import.AmountDue.Text9, "0") || Equal
                    (import.AmountDue.Text9, "0.0") || Equal
                    (import.AmountDue.Text9, "00.0") || Equal
                    (import.AmountDue.Text9, "000.0") || Equal
                    (import.AmountDue.Text9, "0000.0") || Equal
                    (import.AmountDue.Text9, "00000.0") || Equal
                    (import.AmountDue.Text9, "000000.0") || Equal
                    (import.AmountDue.Text9, "0000000.0") || Equal
                    (import.AmountDue.Text9, "00000000") || Equal
                    (import.AmountDue.Text9, "0000000") || Equal
                    (import.AmountDue.Text9, "000000") || Equal
                    (import.AmountDue.Text9, "00000") || Equal
                    (import.AmountDue.Text9, "0000") || Equal
                    (import.AmountDue.Text9, "000") || Equal
                    (import.AmountDue.Text9, "00"))
                  {
                    if (!Lt(local.NullDate.Date,
                      export.KsDriversLicense.AppealReceivedDate))
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field18.Protected = false;

                      var field19 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field19.Protected = false;

                      var field20 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field20.Protected = false;
                    }
                    else
                    {
                      var field =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field.Color = "cyan";
                      field.Protected = true;

                      if (IsEmpty(export.KsDriversLicense.AppealResolved))
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field18.Protected = false;

                        var field19 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field19.Protected = false;
                      }
                      else
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field18.Color = "cyan";
                        field18.Protected = true;

                        var field19 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field19.Color = "cyan";
                        field19.Protected = true;
                      }
                    }

                    if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                      || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                    {
                      // if the service complete flag is 'Y' set the service 
                      // complete field  to protected
                      // if the service complete flag is 'N' do not set the 
                      // service complete field  to protected
                      // if the service complete date is completed set it to 
                      // protected
                      var field18 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field18.Protected = false;

                      var field19 =
                        GetField(export.KsDriversLicense, "serviceCompleteDate");
                        

                      field19.Protected = false;
                    }
                    else
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field18.Color = "cyan";
                      field18.Protected = true;

                      var field19 =
                        GetField(export.KsDriversLicense, "serviceCompleteDate");
                        

                      field19.Color = "cyan";
                      field19.Protected = true;
                    }

                    ExitState = "AMOUNT_MUST_BE_GREATER_THAN_ZERO";

                    var field15 = GetField(export.AmountDue, "text9");

                    field15.Error = true;

                    var field16 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field16.Protected = false;

                    var field17 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field17.Protected = false;

                    return;
                  }
                  else
                  {
                    // this is ok keep processing
                  }

                  if (Lt(import.KsDriversLicense.FirstPaymentDueDate, Now().Date)
                    && Lt
                    (entities.KsDriversLicense.FirstPaymentDueDate, Now().Date))
                  {
                    var field15 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field15.Error = true;

                    ExitState = "DATE_MUST_GREATER_OR_EQU_CUR_DT";

                    if (!Lt(local.NullDate.Date,
                      import.KsDriversLicense.FirstPaymentDueDate) && Lt
                      (local.NullDate.Date,
                      entities.KsDriversLicense.FirstPaymentDueDate))
                    {
                      export.KsDriversLicense.FirstPaymentDueDate =
                        entities.KsDriversLicense.FirstPaymentDueDate;
                    }

                    if (Lt(local.NullDate.Date,
                      import.KsDriversLicense.FirstPaymentDueDate) && !
                      Lt(local.NullDate.Date,
                      entities.KsDriversLicense.FirstPaymentDueDate))
                    {
                      export.KsDriversLicense.FirstPaymentDueDate =
                        import.KsDriversLicense.FirstPaymentDueDate;
                    }

                    if (!Lt(local.NullDate.Date,
                      export.KsDriversLicense.AppealReceivedDate))
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field18.Protected = false;

                      var field19 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field19.Protected = false;

                      var field20 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field20.Protected = false;
                    }
                    else
                    {
                      var field =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field.Color = "cyan";
                      field.Protected = true;

                      if (IsEmpty(export.KsDriversLicense.AppealResolved))
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field18.Protected = false;

                        var field19 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field19.Protected = false;
                      }
                      else
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field18.Protected = true;

                        var field19 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field19.Color = "cyan";
                        field19.Protected = true;
                      }
                    }

                    if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                      || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                    {
                      // if the service complete flag is 'Y' set the service 
                      // complete field  to protected
                      // if the service complete flag is 'N' do not set the 
                      // service complete field  to protected
                      // if the service complete date is completed set it to 
                      // protected
                      var field18 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field18.Protected = false;

                      var field19 =
                        GetField(export.KsDriversLicense, "serviceCompleteDate");
                        

                      field19.Protected = false;
                    }
                    else
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field18.Color = "cyan";
                      field18.Protected = true;

                      var field19 =
                        GetField(export.KsDriversLicense, "serviceCompleteDate");
                        

                      field19.Color = "cyan";
                      field19.Protected = true;
                    }

                    var field16 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field16.Protected = false;

                    var field17 = GetField(export.AmountDue, "text9");

                    field17.Color = "";
                    field17.Protected = false;

                    return;
                  }
                  else
                  {
                    ++export.UpdateConfirm.Count;

                    if (!Lt(local.NullDate.Date,
                      import.KsDriversLicense.FirstPaymentDueDate) && Lt
                      (local.NullDate.Date,
                      entities.KsDriversLicense.FirstPaymentDueDate))
                    {
                      export.KsDriversLicense.FirstPaymentDueDate =
                        entities.KsDriversLicense.FirstPaymentDueDate;
                      local.PaymentUpadate.FirstPaymentDueDate =
                        entities.KsDriversLicense.FirstPaymentDueDate;
                    }
                    else if (Lt(local.NullDate.Date,
                      import.KsDriversLicense.FirstPaymentDueDate) && Lt
                      (import.KsDriversLicense.FirstPaymentDueDate, Now().Date))
                    {
                      var field15 =
                        GetField(export.KsDriversLicense, "firstPaymentDueDate");
                        

                      field15.Error = true;

                      ExitState = "DATE_MUST_GREATER_OR_EQU_CUR_DT";

                      if (!Lt(local.NullDate.Date,
                        import.KsDriversLicense.FirstPaymentDueDate) && Lt
                        (local.NullDate.Date,
                        entities.KsDriversLicense.FirstPaymentDueDate))
                      {
                        export.KsDriversLicense.FirstPaymentDueDate =
                          entities.KsDriversLicense.FirstPaymentDueDate;
                      }

                      if (Lt(local.NullDate.Date,
                        import.KsDriversLicense.FirstPaymentDueDate) && !
                        Lt(local.NullDate.Date,
                        entities.KsDriversLicense.FirstPaymentDueDate))
                      {
                        export.KsDriversLicense.FirstPaymentDueDate =
                          import.KsDriversLicense.FirstPaymentDueDate;
                      }

                      if (!Lt(local.NullDate.Date,
                        export.KsDriversLicense.AppealReceivedDate))
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "appealReceivedDate");
                          

                        field18.Protected = false;

                        var field19 =
                          GetField(export.KsDriversLicense, "appealResolved");

                        field19.Protected = false;

                        var field20 =
                          GetField(export.PromptAppealDecsionCod, "selectChar");
                          

                        field20.Protected = false;
                      }
                      else
                      {
                        var field =
                          GetField(export.KsDriversLicense, "appealReceivedDate");
                          

                        field.Color = "cyan";
                        field.Protected = true;

                        if (IsEmpty(export.KsDriversLicense.AppealResolved))
                        {
                          var field18 =
                            GetField(export.KsDriversLicense, "appealResolved");
                            

                          field18.Protected = false;

                          var field19 =
                            GetField(export.PromptAppealDecsionCod, "selectChar");
                            

                          field19.Protected = false;
                        }
                        else
                        {
                          var field18 =
                            GetField(export.KsDriversLicense, "appealResolved");
                            

                          field18.Protected = true;

                          var field19 =
                            GetField(export.PromptAppealDecsionCod, "selectChar");
                            

                          field19.Color = "cyan";
                          field19.Protected = true;
                        }
                      }

                      if (AsChar(export.KsDriversLicense.ServiceCompleteInd) ==
                        'N' || IsEmpty
                        (export.KsDriversLicense.ServiceCompleteInd))
                      {
                        // if the service complete flag is 'Y' set the service 
                        // complete field  to protected
                        // if the service complete flag is 'N' do not set the 
                        // service complete field  to protected
                        // if the service complete date is completed set it to 
                        // protected
                        var field18 =
                          GetField(export.KsDriversLicense, "serviceCompleteInd");
                          

                        field18.Protected = false;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "serviceCompleteDate");

                        field19.Protected = false;
                      }
                      else
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "serviceCompleteInd");
                          

                        field18.Color = "cyan";
                        field18.Protected = true;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "serviceCompleteDate");

                        field19.Color = "cyan";
                        field19.Protected = true;
                      }

                      var field16 =
                        GetField(export.KsDriversLicense, "paymentAgreementDate");
                        

                      field16.Protected = false;

                      var field17 = GetField(export.AmountDue, "text9");

                      field17.Protected = false;

                      return;
                    }
                    else
                    {
                      local.PaymentUpadate.FirstPaymentDueDate =
                        import.KsDriversLicense.FirstPaymentDueDate;
                    }

                    foreach(var item in ReadKsDriversLicense10())
                    {
                      if (export.UpdateConfirm.Count < 2)
                      {
                        ExitState = "FIRST_PAYMT_DT_CHGE_FOR_ALL";
                        local.NeedToOkPaymentUpdate.Flag = "Y";

                        return;
                      }
                    }

                    foreach(var item in ReadKsDriversLicense12())
                    {
                      try
                      {
                        UpdateKsDriversLicense2();
                        export.UpdateConfirm.Count = 0;
                      }
                      catch(Exception e)
                      {
                        switch(GetErrorCode(e))
                        {
                          case ErrorCode.AlreadyExists:
                            ExitState = "KS_DRIVERS_LICENSE_NU";

                            var field15 =
                              GetField(export.CsePersonsWorkSet, "number");

                            field15.Error = true;

                            if (!Lt(local.NullDate.Date,
                              export.KsDriversLicense.AppealReceivedDate))
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "appealReceivedDate");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "appealResolved");

                              field18.Protected = false;

                              var field19 =
                                GetField(export.PromptAppealDecsionCod,
                                "selectChar");

                              field19.Protected = false;
                            }
                            else
                            {
                              var field =
                                GetField(export.KsDriversLicense,
                                "appealReceivedDate");

                              field.Color = "cyan";
                              field.Protected = true;

                              if (IsEmpty(export.KsDriversLicense.AppealResolved))
                                
                              {
                                var field17 =
                                  GetField(export.KsDriversLicense,
                                  "appealResolved");

                                field17.Protected = false;

                                var field18 =
                                  GetField(export.PromptAppealDecsionCod,
                                  "selectChar");

                                field18.Protected = false;
                              }
                              else
                              {
                                var field17 =
                                  GetField(export.KsDriversLicense,
                                  "appealResolved");

                                field17.Color = "cyan";
                                field17.Protected = true;

                                var field18 =
                                  GetField(export.PromptAppealDecsionCod,
                                  "selectChar");

                                field18.Color = "cyan";
                                field18.Protected = true;
                              }
                            }

                            if (AsChar(export.KsDriversLicense.
                              ServiceCompleteInd) == 'N' || IsEmpty
                              (export.KsDriversLicense.ServiceCompleteInd))
                            {
                              // if the service complete flag is 'Y' set the 
                              // service complete field  to protected
                              // if the service complete flag is 'N' do not set 
                              // the service complete field  to protected
                              // if the service complete date is completed set 
                              // it to protected
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "serviceCompleteInd");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "serviceCompleteDate");

                              field18.Protected = false;
                            }
                            else
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "serviceCompleteInd");

                              field17.Color = "cyan";
                              field17.Protected = true;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "serviceCompleteDate");

                              field18.Color = "cyan";
                              field18.Protected = true;
                            }

                            if (!Lt(local.NullDate.Date,
                              export.KsDriversLicense.PaymentAgreementDate))
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "paymentAgreementDate");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "firstPaymentDueDate");

                              field18.Protected = false;

                              var field19 = GetField(export.AmountDue, "text9");

                              field19.Protected = false;
                            }
                            else
                            {
                              // If the payment agreement date is greater than a
                              // null date then protect the payment agreement
                              // date.
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "paymentAgreementDate");

                              field17.Color = "cyan";
                              field17.Protected = true;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "firstPaymentDueDate");

                              field18.Protected = false;

                              var field19 = GetField(export.AmountDue, "text9");

                              field19.Protected = false;
                            }

                            return;
                          case ErrorCode.PermittedValueViolation:
                            ExitState = "KS_DRIVERS_LICENSE_PV";

                            var field16 =
                              GetField(export.CsePersonsWorkSet, "number");

                            field16.Error = true;

                            if (!Lt(local.NullDate.Date,
                              export.KsDriversLicense.AppealReceivedDate))
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "appealReceivedDate");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "appealResolved");

                              field18.Protected = false;

                              var field19 =
                                GetField(export.PromptAppealDecsionCod,
                                "selectChar");

                              field19.Protected = false;
                            }
                            else
                            {
                              var field =
                                GetField(export.KsDriversLicense,
                                "appealReceivedDate");

                              field.Color = "cyan";
                              field.Protected = true;

                              if (IsEmpty(export.KsDriversLicense.AppealResolved))
                                
                              {
                                var field17 =
                                  GetField(export.KsDriversLicense,
                                  "appealResolved");

                                field17.Protected = false;

                                var field18 =
                                  GetField(export.PromptAppealDecsionCod,
                                  "selectChar");

                                field18.Protected = false;
                              }
                              else
                              {
                                var field17 =
                                  GetField(export.KsDriversLicense,
                                  "appealResolved");

                                field17.Color = "cyan";
                                field17.Protected = true;

                                var field18 =
                                  GetField(export.PromptAppealDecsionCod,
                                  "selectChar");

                                field18.Color = "cyan";
                                field18.Protected = true;
                              }
                            }

                            if (AsChar(export.KsDriversLicense.
                              ServiceCompleteInd) == 'N' || IsEmpty
                              (export.KsDriversLicense.ServiceCompleteInd))
                            {
                              // if the service complete flag is 'Y' set the 
                              // service complete field  to protected
                              // if the service complete flag is 'N' do not set 
                              // the service complete field  to protected
                              // if the service complete date is completed set 
                              // it to protected
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "serviceCompleteInd");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "serviceCompleteDate");

                              field18.Protected = false;
                            }
                            else
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "serviceCompleteInd");

                              field17.Color = "cyan";
                              field17.Protected = true;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "serviceCompleteDate");

                              field18.Color = "cyan";
                              field18.Protected = true;
                            }

                            if (!Lt(local.NullDate.Date,
                              export.KsDriversLicense.PaymentAgreementDate))
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "paymentAgreementDate");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "firstPaymentDueDate");

                              field18.Protected = false;

                              var field19 = GetField(export.AmountDue, "text9");

                              field19.Protected = false;
                            }
                            else
                            {
                              // If the payment agreement date is greater than a
                              // null date then protect the payment agreement
                              // date.
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "paymentAgreementDate");

                              field17.Color = "cyan";
                              field17.Protected = true;

                              var field18 =
                                GetField(export.KsDriversLicense,
                                "firstPaymentDueDate");

                              field18.Protected = false;

                              var field19 = GetField(export.AmountDue, "text9");

                              field19.Protected = false;
                            }

                            return;
                          case ErrorCode.DatabaseError:
                            break;
                          default:
                            throw;
                        }
                      }
                    }
                  }
                }
                else
                {
                  var field15 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field15.Error = true;

                  var field16 = GetField(export.AmountDue, "text9");

                  field16.Error = true;

                  ExitState = "PAYMENT_AGREEMENT_INFO_MISSING";

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.AppealReceivedDate))
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field19.Protected = false;

                    var field20 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field20.Protected = false;
                  }
                  else
                  {
                    var field =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field.Color = "cyan";
                    field.Protected = true;

                    if (IsEmpty(export.KsDriversLicense.AppealResolved))
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field18.Protected = false;

                      var field19 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field19.Protected = false;
                    }
                    else
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field18.Color = "cyan";
                      field18.Protected = true;

                      var field19 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field19.Color = "cyan";
                      field19.Protected = true;
                    }
                  }

                  if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                    || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                  {
                    // if the service complete flag is 'Y' set the service 
                    // complete field  to protected
                    // if the service complete flag is 'N' do not set the 
                    // service complete field  to protected
                    // if the service complete date is completed set it to 
                    // protected
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Color = "cyan";
                    field18.Protected = true;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Color = "cyan";
                    field19.Protected = true;
                  }

                  var field17 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field17.Protected = false;

                  return;
                }
              }
              else
              {
                var field15 =
                  GetField(export.KsDriversLicense, "paymentAgreementDate");

                field15.Error = true;

                ExitState = "DATE_MUST_BE_LESS_CURRENT_AND_GR";

                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.AppealReceivedDate))
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field19.Protected = false;

                  var field20 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field20.Protected = false;
                }
                else
                {
                  var field =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field.Color = "cyan";
                  field.Protected = true;

                  if (IsEmpty(export.KsDriversLicense.AppealResolved))
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Protected = false;
                  }
                }

                if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                  || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                {
                  // if the service complete flag is 'Y' set the service 
                  // complete field  to protected
                  // if the service complete flag is 'N' do not set the service 
                  // complete field  to protected
                  // if the service complete date is completed set it to 
                  // protected
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Protected = false;
                }
                else
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Color = "cyan";
                  field18.Protected = true;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Color = "cyan";
                  field19.Protected = true;
                }

                // If the payment agreement date is greater than a null date 
                // then protect the payment agreement date.
                var field16 =
                  GetField(export.KsDriversLicense, "firstPaymentDueDate");

                field16.Protected = false;

                var field17 = GetField(export.AmountDue, "text9");

                field17.Protected = false;

                return;
              }
            }
            else if (Lt(local.NullDate.Date,
              entities.KsDriversLicense.FirstPaymentDueDate))
            {
              if (!IsEmpty(import.AmountDue.Text9))
              {
                if (Equal(import.AmountDue.Text9, "000000000") || Equal
                  (import.AmountDue.Text9, "000000.00") || Equal
                  (import.AmountDue.Text9, "00000.00") || Equal
                  (import.AmountDue.Text9, "0000.00") || Equal
                  (import.AmountDue.Text9, "000.00") || Equal
                  (import.AmountDue.Text9, "00.00") || Equal
                  (import.AmountDue.Text9, "0.00") || Equal
                  (import.AmountDue.Text9, ".00") || Equal
                  (import.AmountDue.Text9, ".0") || Equal
                  (import.AmountDue.Text9, "0") || Equal
                  (import.AmountDue.Text9, "0.0") || Equal
                  (import.AmountDue.Text9, "00.0") || Equal
                  (import.AmountDue.Text9, "000.0") || Equal
                  (import.AmountDue.Text9, "0000.0") || Equal
                  (import.AmountDue.Text9, "00000.0") || Equal
                  (import.AmountDue.Text9, "000000.0") || Equal
                  (import.AmountDue.Text9, "0000000.0") || Equal
                  (import.AmountDue.Text9, "00000000") || Equal
                  (import.AmountDue.Text9, "0000000") || Equal
                  (import.AmountDue.Text9, "000000") || Equal
                  (import.AmountDue.Text9, "00000") || Equal
                  (import.AmountDue.Text9, "0000") || Equal
                  (import.AmountDue.Text9, "000") || Equal
                  (import.AmountDue.Text9, "00"))
                {
                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.AppealReceivedDate))
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field19.Protected = false;

                    var field20 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field20.Protected = false;
                  }
                  else
                  {
                    var field =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field.Color = "cyan";
                    field.Protected = true;

                    if (IsEmpty(export.KsDriversLicense.AppealResolved))
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field18.Protected = false;

                      var field19 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field19.Protected = false;
                    }
                    else
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field18.Color = "cyan";
                      field18.Protected = true;

                      var field19 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field19.Color = "cyan";
                      field19.Protected = true;
                    }
                  }

                  if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                    || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                  {
                    // if the service complete flag is 'Y' set the service 
                    // complete field  to protected
                    // if the service complete flag is 'N' do not set the 
                    // service complete field  to protected
                    // if the service complete date is completed set it to 
                    // protected
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Color = "cyan";
                    field18.Protected = true;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Color = "cyan";
                    field19.Protected = true;
                  }

                  ExitState = "AMOUNT_MUST_BE_GREATER_THAN_ZERO";

                  var field15 = GetField(export.AmountDue, "text9");

                  field15.Error = true;

                  var field16 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field16.Protected = false;

                  var field17 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field17.Protected = false;

                  return;
                }
                else
                {
                  var field15 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field15.Error = true;

                  ExitState = "PAYMENT_AGREEMENT_INFO_MISSING";

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.AppealReceivedDate))
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field19.Protected = false;

                    var field20 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field20.Protected = false;
                  }
                  else
                  {
                    var field =
                      GetField(export.KsDriversLicense, "appealReceivedDate");

                    field.Color = "cyan";
                    field.Protected = true;

                    if (IsEmpty(export.KsDriversLicense.AppealResolved))
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field18.Protected = false;

                      var field19 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field19.Protected = false;
                    }
                    else
                    {
                      var field18 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field18.Color = "cyan";
                      field18.Protected = true;

                      var field19 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field19.Color = "cyan";
                      field19.Protected = true;
                    }
                  }

                  if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                    || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                  {
                    // if the service complete flag is 'Y' set the service 
                    // complete field  to protected
                    // if the service complete flag is 'N' do not set the 
                    // service complete field  to protected
                    // if the service complete date is completed set it to 
                    // protected
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Color = "cyan";
                    field18.Protected = true;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Color = "cyan";
                    field19.Protected = true;
                  }

                  var field16 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field16.Protected = false;

                  var field17 = GetField(export.AmountDue, "text9");

                  field17.Protected = false;

                  return;
                }
              }
              else
              {
                // this is ok so keep processing
              }
            }
            else if (Lt(local.NullDate.Date,
              import.KsDriversLicense.FirstPaymentDueDate) || !
              IsEmpty(import.AmountDue.Text9) || import
              .KsDriversLicense.AmountDue.GetValueOrDefault() > 0)
            {
              if (Equal(import.AmountDue.Text9, "000000000") || Equal
                (import.AmountDue.Text9, "000000.00") || Equal
                (import.AmountDue.Text9, "00000.00") || Equal
                (import.AmountDue.Text9, "0000.00") || Equal
                (import.AmountDue.Text9, "000.00") || Equal
                (import.AmountDue.Text9, "00.00") || Equal
                (import.AmountDue.Text9, "0.00") || Equal
                (import.AmountDue.Text9, ".00") || Equal
                (import.AmountDue.Text9, ".0") || Equal
                (import.AmountDue.Text9, "0") || Equal
                (import.AmountDue.Text9, "0.0") || Equal
                (import.AmountDue.Text9, "00.0") || Equal
                (import.AmountDue.Text9, "000.0") || Equal
                (import.AmountDue.Text9, "0000.0") || Equal
                (import.AmountDue.Text9, "00000.0") || Equal
                (import.AmountDue.Text9, "000000.0") || Equal
                (import.AmountDue.Text9, "0000000.0") || Equal
                (import.AmountDue.Text9, "00000000") || Equal
                (import.AmountDue.Text9, "0000000") || Equal
                (import.AmountDue.Text9, "000000") || Equal
                (import.AmountDue.Text9, "00000") || Equal
                (import.AmountDue.Text9, "0000") || Equal
                (import.AmountDue.Text9, "000") || Equal
                (import.AmountDue.Text9, "00"))
              {
                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.AppealReceivedDate))
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field19.Protected = false;

                  var field20 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field20.Protected = false;
                }
                else
                {
                  var field =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field.Color = "cyan";
                  field.Protected = true;

                  if (IsEmpty(export.KsDriversLicense.AppealResolved))
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Color = "cyan";
                    field18.Protected = true;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Color = "cyan";
                    field19.Protected = true;
                  }
                }

                if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                  || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                {
                  // if the service complete flag is 'Y' set the service 
                  // complete field  to protected
                  // if the service complete flag is 'N' do not set the service 
                  // complete field  to protected
                  // if the service complete date is completed set it to 
                  // protected
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Protected = false;
                }
                else
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Color = "cyan";
                  field18.Protected = true;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Color = "cyan";
                  field19.Protected = true;
                }

                ExitState = "AMOUNT_MUST_BE_GREATER_THAN_ZERO";

                var field15 = GetField(export.AmountDue, "text9");

                field15.Error = true;

                var field16 =
                  GetField(export.KsDriversLicense, "paymentAgreementDate");

                field16.Protected = false;

                var field17 =
                  GetField(export.KsDriversLicense, "firstPaymentDueDate");

                field17.Protected = false;

                return;
              }
              else
              {
                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.AppealReceivedDate))
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field19.Protected = false;

                  var field20 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field20.Protected = false;
                }
                else
                {
                  var field =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field.Protected = true;

                  if (IsEmpty(export.KsDriversLicense.AppealResolved))
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "appealResolved");

                    field18.Color = "cyan";
                    field18.Protected = true;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Color = "cyan";
                    field19.Protected = true;
                  }
                }

                if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                  || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                {
                  // if the service complete flag is 'Y' set the service 
                  // complete field  to protected
                  // if the service complete flag is 'N' do not set the service 
                  // complete field  to protected
                  // if the service complete date is completed set it to 
                  // protected
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Protected = false;
                }
                else
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Color = "cyan";
                  field18.Protected = true;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Color = "cyan";
                  field19.Protected = true;
                }

                var field15 = GetField(export.AmountDue, "text9");

                field15.Protected = false;

                var field16 =
                  GetField(export.KsDriversLicense, "firstPaymentDueDate");

                field16.Protected = false;

                if (!Lt(local.NullDate.Date,
                  import.KsDriversLicense.FirstPaymentDueDate) && !
                  Lt(local.NullDate.Date,
                  entities.KsDriversLicense.FirstPaymentDueDate))
                {
                  var field =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field.Error = true;
                }

                if (IsEmpty(import.AmountDue.Text9))
                {
                  var field = GetField(export.AmountDue, "text9");

                  field.Error = true;
                }

                var field17 =
                  GetField(export.KsDriversLicense, "paymentAgreementDate");

                field17.Error = true;

                ExitState = "PAYMENT_AGREEMENT_INFO_MISSING";

                return;
              }
            }

            // if import appeal request date<> hiddend appeal request date
            //  and hidden appeal request date <= null date
            //   If appeal request date > null date
            //     if 30 day letter create date < = null date
            //        exit state is 30 day letter needs to be sent before an 
            // appeal can be
            //            made
            //         make error appeal request date
            // <-------escape
            //     else
            //        if appeal request date < = 30 day letter create date
            //           exit state is 30 day letter needs to be sent before an 
            // appeal can be made
            //            make error appeal request date
            // <-------escape
            //        else
            //           if appeal decision code > spaces
            //             and  appeal decision date <= null date
            //                  if appeal decision code not = 'O'
            //                   or appeal decision code not = 'S'
            //                    make error appeal decision code
            //                    exitstate is invalid appeal deciison code
            //                  else
            //                     set appeal decision date to curent date
            //                     read ks driver's license table
            //                         where cse person number = current person 
            // number
            //                             and  last validatioin date = current 
            // validation date
            //                             and  legal action id not = current 
            // legal action id
            //                             and  appeal decision code = spaces
            //                          when successsful
            //                                    
            // set more appeal records to be
            // opened to 'Y'
            //                         when not successful
            //                  end if
            //             else
            //               set more appeal records to be opened to 'Y'
            //               set local appeal request date to import appeal 
            // request date
            //            end if
            //        end if
            //     end if
            //   end if
            // else if import appeal request date = hiddend appeal date
            //     and hidden appeal request date > null date
            //      if appeal decision code > spaces
            //             and  appeal decision date <= null date
            //                  if appeal decision code not = 'O'
            //                   or appeal decision code not = 'S'
            //                    make error appeal decision code
            //                    exitstate is invalid appeal deciison code
            //                  else
            //                     set appeal decision date to curent date
            //                     read ks driver's license table
            //                         where cse person number = current person 
            // number
            //                             and  last validatioin date = current 
            // validation date
            //                             and  legal action id not = current 
            // legal action id
            //                             and  appeal decision code = spaces
            //                          when successsful
            //                                    
            // set more appeal records to be
            // closed to 'Y'
            //                         when not successful
            //                  end if
            //      endif
            // endif
            if (!Lt(local.NullDate.Date,
              entities.KsDriversLicense.AppealReceivedDate))
            {
              if (Lt(local.NullDate.Date,
                import.KsDriversLicense.AppealReceivedDate))
              {
                if (Lt(import.KsDriversLicense.Attribute30DayLetterCreatedDate,
                  import.KsDriversLicense.AppealReceivedDate) && !
                  Lt(Now().Date, import.KsDriversLicense.AppealReceivedDate))
                {
                  if (!IsEmpty(import.KsDriversLicense.AppealResolved))
                  {
                    if (AsChar(import.KsDriversLicense.AppealResolved) == 'S'
                      || AsChar(import.KsDriversLicense.AppealResolved) == 'O'
                      || AsChar(import.KsDriversLicense.AppealResolved) == 'W'
                      || AsChar(import.KsDriversLicense.AppealResolved) == 'N')
                    {
                      // this is ok , as it is should be
                      export.KsDriversLicense.AppealResolvedDate = Now().Date;

                      if (ReadKsDriversLicense1())
                      {
                        local.MoreAppealRecsToClose.Flag = "Y";
                      }
                    }
                    else
                    {
                      var field15 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field15.Error = true;

                      ExitState = "VALID_CODES_ARE_S_O_W_N";

                      var field16 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field16.Protected = false;

                      var field17 =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field17.Protected = false;

                      if (AsChar(export.KsDriversLicense.ServiceCompleteInd) ==
                        'N' || IsEmpty
                        (export.KsDriversLicense.ServiceCompleteInd))
                      {
                        // if the service complete flag is 'Y' set the service 
                        // complete field  to protected
                        // if the service complete flag is 'N' do not set the 
                        // service complete field  to protected
                        // if the service complete date is completed set it to 
                        // protected
                        var field18 =
                          GetField(export.KsDriversLicense, "serviceCompleteInd");
                          

                        field18.Protected = false;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "serviceCompleteDate");

                        field19.Protected = false;
                      }
                      else
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "serviceCompleteInd");
                          

                        field18.Color = "cyan";
                        field18.Protected = true;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "serviceCompleteDate");

                        field19.Color = "cyan";
                        field19.Protected = true;
                      }

                      if (!Lt(local.NullDate.Date,
                        export.KsDriversLicense.PaymentAgreementDate))
                      {
                        var field18 =
                          GetField(export.KsDriversLicense,
                          "paymentAgreementDate");

                        field18.Protected = false;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "firstPaymentDueDate");

                        field19.Protected = false;

                        var field20 = GetField(export.AmountDue, "text9");

                        field20.Protected = false;
                      }
                      else
                      {
                        // If the payment agreement date is greater than a null 
                        // date then protect the payment agreement date.
                        var field18 =
                          GetField(export.KsDriversLicense,
                          "paymentAgreementDate");

                        field18.Color = "cyan";
                        field18.Protected = true;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "firstPaymentDueDate");

                        field19.Protected = false;

                        var field20 = GetField(export.AmountDue, "text9");

                        field20.Protected = false;
                      }

                      return;
                    }
                  }

                  // this is ok , as it is should be
                  foreach(var item in ReadKsDriversLicense11())
                  {
                    try
                    {
                      UpdateKsDriversLicense1();
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "KS_DRIVERS_LICENSE_NU";

                          var field15 =
                            GetField(export.CsePersonsWorkSet, "number");

                          field15.Error = true;

                          if (!Lt(local.NullDate.Date,
                            export.KsDriversLicense.AppealReceivedDate))
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "appealReceivedDate");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense, "appealResolved");
                              

                            field18.Protected = false;

                            var field19 =
                              GetField(export.PromptAppealDecsionCod,
                              "selectChar");

                            field19.Protected = false;
                          }
                          else
                          {
                            var field =
                              GetField(export.KsDriversLicense,
                              "appealReceivedDate");

                            field.Color = "cyan";
                            field.Protected = true;

                            if (IsEmpty(export.KsDriversLicense.AppealResolved))
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "appealResolved");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.PromptAppealDecsionCod,
                                "selectChar");

                              field18.Protected = false;
                            }
                          }

                          if (AsChar(export.KsDriversLicense.ServiceCompleteInd) ==
                            'N' || IsEmpty
                            (export.KsDriversLicense.ServiceCompleteInd))
                          {
                            // if the service complete flag is 'Y' set the 
                            // service complete field  to protected
                            // if the service complete flag is 'N' do not set 
                            // the service complete field  to protected
                            // if the service complete date is completed set it 
                            // to protected
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteInd");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteDate");

                            field18.Protected = false;
                          }
                          else
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteInd");

                            field17.Color = "cyan";
                            field17.Protected = true;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteDate");

                            field18.Color = "cyan";
                            field18.Protected = true;
                          }

                          if (!Lt(local.NullDate.Date,
                            export.KsDriversLicense.PaymentAgreementDate))
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "paymentAgreementDate");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "firstPaymentDueDate");

                            field18.Protected = false;

                            var field19 = GetField(export.AmountDue, "text9");

                            field19.Protected = false;
                          }
                          else
                          {
                            // If the payment agreement date is greater than a 
                            // null date then protect the payment agreement
                            // date.
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "paymentAgreementDate");

                            field17.Color = "cyan";
                            field17.Protected = true;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "firstPaymentDueDate");

                            field18.Protected = false;

                            var field19 = GetField(export.AmountDue, "text9");

                            field19.Protected = false;
                          }

                          return;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "KS_DRIVERS_LICENSE_PV";

                          var field16 =
                            GetField(export.CsePersonsWorkSet, "number");

                          field16.Error = true;

                          if (!Lt(local.NullDate.Date,
                            export.KsDriversLicense.AppealReceivedDate))
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "appealReceivedDate");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense, "appealResolved");
                              

                            field18.Protected = false;

                            var field19 =
                              GetField(export.PromptAppealDecsionCod,
                              "selectChar");

                            field19.Protected = false;
                          }
                          else
                          {
                            var field =
                              GetField(export.KsDriversLicense,
                              "appealReceivedDate");

                            field.Color = "cyan";
                            field.Protected = true;

                            if (IsEmpty(export.KsDriversLicense.AppealResolved))
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "appealResolved");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.PromptAppealDecsionCod,
                                "selectChar");

                              field18.Protected = false;
                            }
                          }

                          if (AsChar(export.KsDriversLicense.ServiceCompleteInd) ==
                            'N' || IsEmpty
                            (export.KsDriversLicense.ServiceCompleteInd))
                          {
                            // if the service complete flag is 'Y' set the 
                            // service complete field  to protected
                            // if the service complete flag is 'N' do not set 
                            // the service complete field  to protected
                            // if the service complete date is completed set it 
                            // to protected
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteInd");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteDate");

                            field18.Protected = false;
                          }
                          else
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteInd");

                            field17.Color = "cyan";
                            field17.Protected = true;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteDate");

                            field18.Color = "cyan";
                            field18.Protected = true;
                          }

                          if (!Lt(local.NullDate.Date,
                            export.KsDriversLicense.PaymentAgreementDate))
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "paymentAgreementDate");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "firstPaymentDueDate");

                            field18.Protected = false;

                            var field19 = GetField(export.AmountDue, "text9");

                            field19.Protected = false;
                          }
                          else
                          {
                            // If the payment agreement date is greater than a 
                            // null date then protect the payment agreement
                            // date.
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "paymentAgreementDate");

                            field17.Color = "cyan";
                            field17.Protected = true;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "firstPaymentDueDate");

                            field18.Protected = false;

                            var field19 = GetField(export.AmountDue, "text9");

                            field19.Protected = false;
                          }

                          return;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                }
                else
                {
                  var field15 =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field15.Error = true;

                  ExitState = "DATE_MUST_BE_LESS_CURRENT_AND_GR";

                  var field16 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field16.Protected = true;

                  var field17 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field17.Protected = false;

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.PaymentAgreementDate))
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field19.Protected = false;

                    var field20 = GetField(export.AmountDue, "text9");

                    field20.Protected = false;
                  }
                  else
                  {
                    // If the payment agreement date is greater than a null date
                    // then protect the payment agreement date.
                    var field18 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field18.Color = "cyan";
                    field18.Protected = true;

                    var field19 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field19.Protected = false;

                    var field20 = GetField(export.AmountDue, "text9");

                    field20.Protected = false;
                  }

                  if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                    || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                  {
                    // if the service complete flag is 'Y' set the service 
                    // complete field  to protected
                    // if the service complete flag is 'N' do not set the 
                    // service complete field  to protected
                    // if the service complete date is completed set it to 
                    // protected
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Color = "cyan";
                    field18.Protected = true;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Color = "cyan";
                    field19.Protected = true;
                  }

                  return;
                }
              }
              else if (!IsEmpty(import.KsDriversLicense.AppealResolved))
              {
                var field15 =
                  GetField(export.KsDriversLicense, "appealReceivedDate");

                field15.Error = true;

                ExitState = "APPEALS_REQUEST_MUST_EXIST_PRIOR";

                var field16 =
                  GetField(export.PromptAppealDecsionCod, "selectChar");

                field16.Protected = false;

                var field17 =
                  GetField(export.KsDriversLicense, "appealResolved");

                field17.Protected = false;

                if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                  || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                {
                  // if the service complete flag is 'Y' set the service 
                  // complete field  to protected
                  // if the service complete flag is 'N' do not set the service 
                  // complete field  to protected
                  // if the service complete date is completed set it to 
                  // protected
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Protected = false;
                }
                else
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Color = "cyan";
                  field18.Protected = true;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Color = "cyan";
                  field19.Protected = true;
                }

                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.PaymentAgreementDate))
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field19.Protected = false;

                  var field20 = GetField(export.AmountDue, "text9");

                  field20.Protected = false;
                }
                else
                {
                  // If the payment agreement date is greater than a null date 
                  // then protect the payment agreement date.
                  var field18 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field18.Color = "cyan";
                  field18.Protected = true;

                  var field19 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field19.Protected = false;

                  var field20 = GetField(export.AmountDue, "text9");

                  field20.Protected = false;
                }

                return;
              }
            }
            else
            {
              // there already is a appeal recieved date
              if (Lt(local.NullDate.Date,
                import.KsDriversLicense.AppealReceivedDate))
              {
                if (Lt(import.KsDriversLicense.Attribute30DayLetterCreatedDate,
                  import.KsDriversLicense.AppealReceivedDate) && !
                  Lt(Now().Date, import.KsDriversLicense.AppealReceivedDate))
                {
                  if (!IsEmpty(import.KsDriversLicense.AppealResolved))
                  {
                    if (AsChar(import.KsDriversLicense.AppealResolved) == 'S'
                      || AsChar(import.KsDriversLicense.AppealResolved) == 'O'
                      || AsChar(import.KsDriversLicense.AppealResolved) == 'W'
                      || AsChar(import.KsDriversLicense.AppealResolved) == 'N')
                    {
                      // this is ok , as it is should be
                      export.KsDriversLicense.AppealResolvedDate = Now().Date;

                      if (ReadKsDriversLicense1())
                      {
                        local.MoreAppealRecsToClose.Flag = "Y";
                      }
                    }
                    else
                    {
                      var field15 =
                        GetField(export.KsDriversLicense, "appealResolved");

                      field15.Error = true;

                      ExitState = "VALID_CODES_ARE_S_O_W_N";

                      var field16 =
                        GetField(export.PromptAppealDecsionCod, "selectChar");

                      field16.Protected = false;

                      var field17 =
                        GetField(export.KsDriversLicense, "appealReceivedDate");
                        

                      field17.Protected = false;

                      if (AsChar(export.KsDriversLicense.ServiceCompleteInd) ==
                        'N' || IsEmpty
                        (export.KsDriversLicense.ServiceCompleteInd))
                      {
                        // if the service complete flag is 'Y' set the service 
                        // complete field  to protected
                        // if the service complete flag is 'N' do not set the 
                        // service complete field  to protected
                        // if the service complete date is completed set it to 
                        // protected
                        var field18 =
                          GetField(export.KsDriversLicense, "serviceCompleteInd");
                          

                        field18.Protected = false;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "serviceCompleteDate");

                        field19.Protected = false;
                      }
                      else
                      {
                        var field18 =
                          GetField(export.KsDriversLicense, "serviceCompleteInd");
                          

                        field18.Color = "cyan";
                        field18.Protected = true;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "serviceCompleteDate");

                        field19.Color = "cyan";
                        field19.Protected = true;
                      }

                      if (!Lt(local.NullDate.Date,
                        export.KsDriversLicense.PaymentAgreementDate))
                      {
                        var field18 =
                          GetField(export.KsDriversLicense,
                          "paymentAgreementDate");

                        field18.Protected = false;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "firstPaymentDueDate");

                        field19.Protected = false;

                        var field20 = GetField(export.AmountDue, "text9");

                        field20.Protected = false;
                      }
                      else
                      {
                        // If the payment agreement date is greater than a null 
                        // date then protect the payment agreement date.
                        var field18 =
                          GetField(export.KsDriversLicense,
                          "paymentAgreementDate");

                        field18.Color = "cyan";
                        field18.Protected = true;

                        var field19 =
                          GetField(export.KsDriversLicense,
                          "firstPaymentDueDate");

                        field19.Protected = false;

                        var field20 = GetField(export.AmountDue, "text9");

                        field20.Protected = false;
                      }

                      return;
                    }
                  }

                  // this is ok , as it is should be
                  foreach(var item in ReadKsDriversLicense11())
                  {
                    try
                    {
                      UpdateKsDriversLicense1();
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "KS_DRIVERS_LICENSE_NU";

                          var field15 =
                            GetField(export.CsePersonsWorkSet, "number");

                          field15.Error = true;

                          if (!Lt(local.NullDate.Date,
                            export.KsDriversLicense.AppealReceivedDate))
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "appealReceivedDate");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense, "appealResolved");
                              

                            field18.Protected = false;

                            var field19 =
                              GetField(export.PromptAppealDecsionCod,
                              "selectChar");

                            field19.Protected = false;
                          }
                          else
                          {
                            var field =
                              GetField(export.KsDriversLicense,
                              "appealReceivedDate");

                            field.Color = "cyan";
                            field.Protected = true;

                            if (IsEmpty(export.KsDriversLicense.AppealResolved))
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "appealResolved");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.PromptAppealDecsionCod,
                                "selectChar");

                              field18.Protected = false;
                            }
                          }

                          if (AsChar(export.KsDriversLicense.ServiceCompleteInd) ==
                            'N' || IsEmpty
                            (export.KsDriversLicense.ServiceCompleteInd))
                          {
                            // if the service complete flag is 'Y' set the 
                            // service complete field  to protected
                            // if the service complete flag is 'N' do not set 
                            // the service complete field  to protected
                            // if the service complete date is completed set it 
                            // to protected
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteInd");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteDate");

                            field18.Protected = false;
                          }
                          else
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteInd");

                            field17.Color = "cyan";
                            field17.Protected = true;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteDate");

                            field18.Color = "cyan";
                            field18.Protected = true;
                          }

                          if (!Lt(local.NullDate.Date,
                            export.KsDriversLicense.PaymentAgreementDate))
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "paymentAgreementDate");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "firstPaymentDueDate");

                            field18.Protected = false;

                            var field19 = GetField(export.AmountDue, "text9");

                            field19.Protected = false;
                          }
                          else
                          {
                            // If the payment agreement date is greater than a 
                            // null date then protect the payment agreement
                            // date.
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "paymentAgreementDate");

                            field17.Color = "cyan";
                            field17.Protected = true;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "firstPaymentDueDate");

                            field18.Protected = false;

                            var field19 = GetField(export.AmountDue, "text9");

                            field19.Protected = false;
                          }

                          return;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "KS_DRIVERS_LICENSE_PV";

                          var field16 =
                            GetField(export.CsePersonsWorkSet, "number");

                          field16.Error = true;

                          if (!Lt(local.NullDate.Date,
                            export.KsDriversLicense.AppealReceivedDate))
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "appealReceivedDate");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense, "appealResolved");
                              

                            field18.Protected = false;

                            var field19 =
                              GetField(export.PromptAppealDecsionCod,
                              "selectChar");

                            field19.Protected = false;
                          }
                          else
                          {
                            var field =
                              GetField(export.KsDriversLicense,
                              "appealReceivedDate");

                            field.Color = "cyan";
                            field.Protected = true;

                            if (IsEmpty(export.KsDriversLicense.AppealResolved))
                            {
                              var field17 =
                                GetField(export.KsDriversLicense,
                                "appealResolved");

                              field17.Protected = false;

                              var field18 =
                                GetField(export.PromptAppealDecsionCod,
                                "selectChar");

                              field18.Protected = false;
                            }
                          }

                          if (AsChar(export.KsDriversLicense.ServiceCompleteInd) ==
                            'N' || IsEmpty
                            (export.KsDriversLicense.ServiceCompleteInd))
                          {
                            // if the service complete flag is 'Y' set the 
                            // service complete field  to protected
                            // if the service complete flag is 'N' do not set 
                            // the service complete field  to protected
                            // if the service complete date is completed set it 
                            // to protected
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteInd");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteDate");

                            field18.Protected = false;
                          }
                          else
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteInd");

                            field17.Color = "cyan";
                            field17.Protected = true;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "serviceCompleteDate");

                            field18.Color = "cyan";
                            field18.Protected = true;
                          }

                          if (!Lt(local.NullDate.Date,
                            export.KsDriversLicense.PaymentAgreementDate))
                          {
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "paymentAgreementDate");

                            field17.Protected = false;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "firstPaymentDueDate");

                            field18.Protected = false;

                            var field19 = GetField(export.AmountDue, "text9");

                            field19.Protected = false;
                          }
                          else
                          {
                            // If the payment agreement date is greater than a 
                            // null date then protect the payment agreement
                            // date.
                            var field17 =
                              GetField(export.KsDriversLicense,
                              "paymentAgreementDate");

                            field17.Color = "cyan";
                            field17.Protected = true;

                            var field18 =
                              GetField(export.KsDriversLicense,
                              "firstPaymentDueDate");

                            field18.Protected = false;

                            var field19 = GetField(export.AmountDue, "text9");

                            field19.Protected = false;
                          }

                          return;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                }
                else
                {
                  var field15 =
                    GetField(export.KsDriversLicense, "appealReceivedDate");

                  field15.Error = true;

                  ExitState = "DATE_MUST_BE_LESS_CURRENT_AND_GR";

                  var field16 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field16.Protected = true;

                  var field17 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field17.Protected = false;

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.PaymentAgreementDate))
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field19.Protected = false;

                    var field20 = GetField(export.AmountDue, "text9");

                    field20.Protected = false;
                  }
                  else
                  {
                    // If the payment agreement date is greater than a null date
                    // then protect the payment agreement date.
                    var field18 =
                      GetField(export.KsDriversLicense, "paymentAgreementDate");
                      

                    field18.Color = "cyan";
                    field18.Protected = true;

                    var field19 =
                      GetField(export.KsDriversLicense, "firstPaymentDueDate");

                    field19.Protected = false;

                    var field20 = GetField(export.AmountDue, "text9");

                    field20.Protected = false;
                  }

                  if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                    || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                  {
                    // if the service complete flag is 'Y' set the service 
                    // complete field  to protected
                    // if the service complete flag is 'N' do not set the 
                    // service complete field  to protected
                    // if the service complete date is completed set it to 
                    // protected
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Color = "cyan";
                    field18.Protected = true;

                    var field19 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field19.Color = "cyan";
                    field19.Protected = true;
                  }

                  return;
                }
              }
              else if (!IsEmpty(import.KsDriversLicense.AppealResolved))
              {
                var field15 =
                  GetField(export.KsDriversLicense, "appealResolved");

                field15.Error = true;

                ExitState = "APPEALS_REQUEST_MUST_EXIST_PRIOR";

                var field16 =
                  GetField(export.PromptAppealDecsionCod, "selectChar");

                field16.Protected = false;

                var field17 =
                  GetField(export.KsDriversLicense, "appealReceivedDate");

                field17.Color = "";
                field17.Protected = false;

                if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N'
                  || IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                {
                  // if the service complete flag is 'Y' set the service 
                  // complete field  to protected
                  // if the service complete flag is 'N' do not set the service 
                  // complete field  to protected
                  // if the service complete date is completed set it to 
                  // protected
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Protected = false;
                }
                else
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "serviceCompleteInd");

                  field18.Color = "cyan";
                  field18.Protected = true;

                  var field19 =
                    GetField(export.KsDriversLicense, "serviceCompleteDate");

                  field19.Color = "cyan";
                  field19.Protected = true;
                }

                if (!Lt(local.NullDate.Date,
                  export.KsDriversLicense.PaymentAgreementDate))
                {
                  var field18 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field18.Protected = false;

                  var field19 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field19.Protected = false;

                  var field20 = GetField(export.AmountDue, "text9");

                  field20.Protected = false;
                }
                else
                {
                  // If the payment agreement date is greater than a null date 
                  // then protect the payment agreement date.
                  var field18 =
                    GetField(export.KsDriversLicense, "paymentAgreementDate");

                  field18.Color = "cyan";
                  field18.Protected = true;

                  var field19 =
                    GetField(export.KsDriversLicense, "firstPaymentDueDate");

                  field19.Protected = false;

                  var field20 = GetField(export.AmountDue, "text9");

                  field20.Protected = false;
                }

                return;
              }
            }
          }

          // update the ks driver's license table  here
          try
          {
            UpdateKsDriversLicense4();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "KS_DRIVERS_LICENSE_NU";

                var field15 = GetField(export.CsePersonsWorkSet, "number");

                field15.Error = true;

                local.HighlightError.Flag = "Y";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "KS_DRIVERS_LICENSE_PV";

                var field16 = GetField(export.CsePersonsWorkSet, "number");

                field16.Error = true;

                local.HighlightError.Flag = "Y";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(local.Rollback.Flag) == 'Y')
            {
              UseEabRollbackCics();
            }

            break;
          }
        }
        else
        {
          ExitState = "KS_DRIVERS_LICENSE_NF";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ++local.LastErrorEntryNo.Count;

          return;
        }

        // we will now check to see if the AP has any more records that need to 
        // be
        //  updated with the service complete info. we will reread the ks 
        // drivers licesne
        // table by the APs number and the current 30 day letter create date. If
        // there are
        // any records we will check the 'more records to be updated flag" to '
        // Y'
        if (AsChar(local.ServiceCompletedCheck.Flag) == 'Y')
        {
          foreach(var item in ReadKsDriversLicense11())
          {
            try
            {
              UpdateKsDriversLicense3();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "KS_DRIVERS_LICENSE_NU";

                  var field15 = GetField(export.CsePersonsWorkSet, "number");

                  field15.Error = true;

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.ServiceCompleteDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Protected = false;
                  }
                  else
                  {
                    var field =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field.Color = "cyan";
                    field.Protected = true;

                    if (IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field17.Protected = false;
                    }
                  }

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "KS_DRIVERS_LICENSE_PV";

                  var field16 = GetField(export.CsePersonsWorkSet, "number");

                  field16.Error = true;

                  if (!Lt(local.NullDate.Date,
                    export.KsDriversLicense.ServiceCompleteDate))
                  {
                    var field17 =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field17.Protected = false;

                    var field18 =
                      GetField(export.KsDriversLicense, "serviceCompleteInd");

                    field18.Protected = false;

                    var field19 =
                      GetField(export.PromptAppealDecsionCod, "selectChar");

                    field19.Protected = false;
                  }
                  else
                  {
                    var field =
                      GetField(export.KsDriversLicense, "serviceCompleteDate");

                    field.Color = "cyan";
                    field.Protected = true;

                    if (IsEmpty(export.KsDriversLicense.ServiceCompleteInd))
                    {
                      var field17 =
                        GetField(export.KsDriversLicense, "serviceCompleteInd");
                        

                      field17.Protected = false;
                    }
                  }

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        // If appeal request date > null date and appeal resolved flag greater 
        // than spaces
        //    exit state is appeal must be resolved before it can be processed
        // <-------escape
        // end if
        if (local.LastErrorEntryNo.Count > 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        // once the updates have been completed successfully and if the more 
        // apeal flag  = 'Y'
        // then the progam will flow to the dmvl screen so that the worker can 
        // select the
        // next cour order to update the appeal info
        // now we will reread after a successful update
        if (ReadKsDriversLicense6())
        {
          export.KsDriversLicense.Assign(entities.KsDriversLicense);
          export.LastUpdate.Date =
            Date(export.KsDriversLicense.LastUpdatedTstamp);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          if (!Lt(local.NullDate.Date,
            export.KsDriversLicense.PaymentAgreementDate))
          {
            export.KsDriversLicense.FirstPaymentDueDate =
              local.InitialisedKsDriversLicense.FirstPaymentDueDate;
          }

          if (export.KsDriversLicense.AmountDue.GetValueOrDefault() > 0)
          {
            local.AmountDueWorkArea.Text15 =
              NumberToString((long)(export.KsDriversLicense.AmountDue.
                GetValueOrDefault() * 100), 15);
            local.Current.Count = 1;
            local.CurrentPosition.Count = 1;

            do
            {
              local.Postion.Text1 =
                Substring(local.AmountDueWorkArea.Text15,
                local.CurrentPosition.Count, 1);

              if (AsChar(local.Postion.Text1) == '0')
              {
                local.Start.Count = local.CurrentPosition.Count + 1;
              }
              else
              {
                local.WorkArea.Text15 = "";
                local.WorkArea.Text15 =
                  Substring(local.AmountDueWorkArea.Text15, local.Current.Count,
                  16 - local.Current.Count - 2);
                local.WorkArea.Text2 =
                  Substring(local.AmountDueWorkArea.Text15, 14, 2);
                local.AmountDueWorkArea.Text15 = "";
                local.AmountDueWorkArea.Text15 =
                  TrimEnd(local.WorkArea.Text15) + "." + local.WorkArea.Text2;
                export.AmountDue.Text9 = local.AmountDueWorkArea.Text15;

                goto Test2;
              }

              ++local.CurrentPosition.Count;
              ++local.Current.Count;
            }
            while(!Equal(global.Command, "COMMAND"));
          }
          else
          {
            export.AmountDue.Text9 = "";
          }

Test2:

          if (AsChar(export.KsDriversLicense.AppealResolved) == 'O')
          {
            var field15 = GetField(export.KsDriversLicense, "note1");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 = GetField(export.KsDriversLicense, "note2");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 = GetField(export.KsDriversLicense, "note3");

            field17.Color = "cyan";
            field17.Protected = true;

            var field18 =
              GetField(export.KsDriversLicense, "paymentAgreementDate");

            field18.Color = "cyan";
            field18.Protected = true;

            var field19 =
              GetField(export.KsDriversLicense, "firstPaymentDueDate");

            field19.Color = "cyan";
            field19.Protected = true;

            var field20 = GetField(export.AmountDue, "text9");

            field20.Color = "cyan";
            field20.Protected = true;

            var field21 =
              GetField(export.KsDriversLicense, "serviceCompleteInd");

            field21.Color = "cyan";
            field21.Protected = true;

            var field22 =
              GetField(export.KsDriversLicense, "serviceCompleteDate");

            field22.Color = "cyan";
            field22.Protected = true;

            var field23 =
              GetField(export.KsDriversLicense, "appealReceivedDate");

            field23.Color = "cyan";
            field23.Protected = true;

            var field24 = GetField(export.KsDriversLicense, "appealResolved");

            field24.Color = "cyan";
            field24.Protected = true;

            var field25 = GetField(export.PromptAppealDecsionCod, "selectChar");

            field25.Color = "cyan";
            field25.Protected = true;

            break;
          }

          if (!Lt(local.NullDate.Date,
            export.KsDriversLicense.AppealReceivedDate))
          {
            var field15 =
              GetField(export.KsDriversLicense, "appealReceivedDate");

            field15.Protected = false;

            var field16 = GetField(export.KsDriversLicense, "appealResolved");

            field16.Protected = false;

            var field17 = GetField(export.PromptAppealDecsionCod, "selectChar");

            field17.Protected = false;
          }
          else
          {
            var field = GetField(export.KsDriversLicense, "appealReceivedDate");

            field.Color = "cyan";
            field.Protected = true;

            if (IsEmpty(export.KsDriversLicense.AppealResolved))
            {
              var field15 = GetField(export.KsDriversLicense, "appealResolved");

              field15.Protected = false;

              var field16 =
                GetField(export.PromptAppealDecsionCod, "selectChar");

              field16.Protected = false;
            }
            else
            {
              var field15 = GetField(export.KsDriversLicense, "appealResolved");

              field15.Color = "cyan";
              field15.Protected = true;

              var field16 =
                GetField(export.PromptAppealDecsionCod, "selectChar");

              field16.Color = "cyan";
              field16.Protected = true;
            }
          }

          if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N' || IsEmpty
            (export.KsDriversLicense.ServiceCompleteInd))
          {
            // if the service complete flag is 'Y' set the service complete 
            // field  to protected
            // if the service complete flag is 'N' do not set the service 
            // complete field  to protected
            // if the service complete date is completed set it to protected
            var field15 =
              GetField(export.KsDriversLicense, "serviceCompleteInd");

            field15.Protected = false;

            var field16 =
              GetField(export.KsDriversLicense, "serviceCompleteDate");

            field16.Protected = false;
          }
          else
          {
            var field15 =
              GetField(export.KsDriversLicense, "serviceCompleteInd");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 =
              GetField(export.KsDriversLicense, "serviceCompleteDate");

            field16.Color = "cyan";
            field16.Protected = true;
          }

          if (!Lt(local.NullDate.Date,
            export.KsDriversLicense.PaymentAgreementDate))
          {
            var field15 =
              GetField(export.KsDriversLicense, "paymentAgreementDate");

            field15.Protected = false;

            var field16 =
              GetField(export.KsDriversLicense, "firstPaymentDueDate");

            field16.Protected = false;

            var field17 = GetField(export.AmountDue, "text9");

            field17.Protected = false;
          }
          else
          {
            // If the payment agreement date is greater than a null date then 
            // protect the payment agreement date.
            var field15 =
              GetField(export.KsDriversLicense, "paymentAgreementDate");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 =
              GetField(export.KsDriversLicense, "firstPaymentDueDate");

            field16.Protected = false;

            var field17 = GetField(export.AmountDue, "text9");

            field17.Protected = false;
          }
        }
        else
        {
          ExitState = "KS_DRIVERS_LICENSE_NF";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          break;
        }

        // ---------------------------------------------
        // Move all exports to export hidden previous
        // ---------------------------------------------
        export.HiddenPrevLegalAction.Assign(export.LegalAction);
        MoveCsePersonsWorkSet(export.CsePersonsWorkSet,
          export.HiddenPrevCsePersonsWorkSet);
        export.HiddenKsDriversLicense.SequenceCounter =
          export.KsDriversLicense.SequenceCounter;

        // move export ks driver's license  to export hidden  ks driver's 
        // license
        // if the flags for either "more appeal records to be opened" or "more 
        // records to be
        // updated" have been set to 'Y' then we will read ks driver's license 
        // table by the
        // current ap's number and the current 30 day lettter create date. and 
        // update these
        // records with the new aapeal info or the service complete data.
        if (AsChar(local.MoreAppealRecsToClose.Flag) == 'Y')
        {
          ExitState = "ACO_NI0000_ADDITIONAL_CT_OR_NUMB";
        }

        break;
      case "EXMP":
        if (IsEmpty(import.CsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "FN0000_MUST_ENTER_PERSON_NUMBER";

          return;
        }

        if (!Equal(import.CsePersonsWorkSet.Number,
          import.HiddenPrevCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";

          return;
        }

        if (IsEmpty(export.LegalAction.StandardNumber))
        {
          if (import.LegalAction.Identifier > 0)
          {
            if (ReadLegalAction5())
            {
              export.LegalAction.Assign(entities.ExistingLegalAction);
            }
            else
            {
              ExitState = "LEGAL_ACTION_NF";

              var field = GetField(export.LegalAction, "standardNumber");

              field.Error = true;

              return;
            }
          }
          else if (import.KsDriversLicense.SequenceCounter > 0)
          {
            if (ReadLegalAction2())
            {
              export.LegalAction.Assign(entities.ExistingLegalAction);
            }
            else
            {
              ExitState = "LEGAL_ACTION_NF";

              var field = GetField(export.LegalAction, "standardNumber");

              field.Error = true;

              return;
            }
          }
          else if (ReadKsDriversLicenseLegalAction2())
          {
            export.LegalAction.Assign(entities.ExistingLegalAction);
          }
        }
        else if (import.KsDriversLicense.SequenceCounter > 0 && import
          .KsDriversLicense.SequenceCounter == export
          .HiddenKsDriversLicense.SequenceCounter && export
          .HiddenKsDriversLicense.SequenceCounter > 0)
        {
          if (ReadLegalAction2())
          {
            export.LegalAction.Assign(entities.ExistingLegalAction);
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";

            var field = GetField(export.LegalAction, "standardNumber");

            field.Error = true;

            return;
          }
        }
        else if (ReadKsDriversLicenseLegalAction1())
        {
          export.LegalAction.Assign(entities.ExistingLegalAction);
        }

        if (ReadObligationObligationType())
        {
          export.Obligation.SystemGeneratedIdentifier =
            entities.ExistingObligation.SystemGeneratedIdentifier;
          export.ObligationType.SystemGeneratedIdentifier =
            entities.ExistingObligationType.SystemGeneratedIdentifier;
        }

        ExitState = "ECO_LNK_2_EXMP_ADM_ACT_EXEMPTION";
        export.HistoricRecord.Flag = "N";

        break;
      case "NEXT":
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "FN0000_MUST_PERFORM_DISPLAY_1ST";
          export.HiddenDisplayPerformed.Flag = "N";

          return;
        }

        if (!Equal(export.CsePersonsWorkSet.Number,
          export.HiddenPrevCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";
          export.UpdateConfirm.Count = 0;

          return;
        }

        // if this is a history record then the fields except the ap person 
        // number field needs to be protected
        local.RecordCounter.Count = 0;

        foreach(var item in ReadKsDriversLicense14())
        {
          ++local.RecordCounter.Count;

          if (local.RecordCounter.Count > 1)
          {
            break;
          }

          export.KsDriversLicense.Assign(entities.KsDriversLicense);
        }

        if (!Lt(local.NullDate.Date,
          export.KsDriversLicense.PaymentAgreementDate))
        {
          export.KsDriversLicense.FirstPaymentDueDate =
            local.InitialisedKsDriversLicense.FirstPaymentDueDate;
        }

        export.HiddenKsDriversLicense.SequenceCounter =
          export.KsDriversLicense.SequenceCounter;

        if (export.KsDriversLicense.AmountDue.GetValueOrDefault() > 0)
        {
          local.AmountDueWorkArea.Text15 =
            NumberToString((long)(export.KsDriversLicense.AmountDue.
              GetValueOrDefault() * 100), 15);
          local.Current.Count = 1;
          local.CurrentPosition.Count = 1;

          do
          {
            local.Postion.Text1 =
              Substring(local.AmountDueWorkArea.Text15,
              local.CurrentPosition.Count, 1);

            if (AsChar(local.Postion.Text1) == '0')
            {
              local.Start.Count = local.CurrentPosition.Count + 1;
            }
            else
            {
              local.WorkArea.Text15 = "";
              local.WorkArea.Text15 =
                Substring(local.AmountDueWorkArea.Text15, local.Current.Count,
                16 - local.Current.Count - 2);
              local.WorkArea.Text2 =
                Substring(local.AmountDueWorkArea.Text15, 14, 2);
              local.AmountDueWorkArea.Text15 = "";
              local.AmountDueWorkArea.Text15 =
                TrimEnd(local.WorkArea.Text15) + "." + local.WorkArea.Text2;
              export.AmountDue.Text9 = local.AmountDueWorkArea.Text15;

              goto Test3;
            }

            ++local.CurrentPosition.Count;
            ++local.Current.Count;
          }
          while(!Equal(global.Command, "COMMAND"));
        }
        else
        {
          export.AmountDue.Text9 = "";
        }

Test3:

        if (local.RecordCounter.Count > 1)
        {
          // set the more +/- flag to +/-
          export.MoreLessScroll.Text3 = "+ -";
        }
        else if (local.RecordCounter.Count == 1)
        {
          export.MoreLessScroll.Text3 = "  -";
        }
        else
        {
          ExitState = "THERE_NO_MORE_RECORDS_TO_DISPLAY";
        }

        export.LastUpdate.Date =
          Date(export.KsDriversLicense.LastUpdatedTstamp);
        export.HistoricRecord.Flag = "Y";

        var field1 = GetField(export.KsDriversLicense, "note1");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.KsDriversLicense, "note2");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.KsDriversLicense, "note3");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.KsDriversLicense, "note1");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.KsDriversLicense, "note2");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.KsDriversLicense, "note3");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.KsDriversLicense, "paymentAgreementDate");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.KsDriversLicense, "firstPaymentDueDate");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.AmountDue, "text9");

        field9.Color = "cyan";
        field9.Protected = true;

        var field10 = GetField(export.KsDriversLicense, "serviceCompleteInd");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.KsDriversLicense, "serviceCompleteDate");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.KsDriversLicense, "appealReceivedDate");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.KsDriversLicense, "appealResolved");

        field13.Color = "cyan";
        field13.Protected = true;

        var field14 = GetField(export.PromptAppealDecsionCod, "selectChar");

        field14.Color = "cyan";
        field14.Protected = true;

        return;
      case "PREV":
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "FN0000_MUST_PERFORM_DISPLAY_1ST";
          export.HiddenDisplayPerformed.Flag = "N";

          return;
        }

        if (!Equal(export.CsePersonsWorkSet.Number,
          export.HiddenPrevCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "PERSON_NUMBER_CHANGED_REDISPLAY";
          export.UpdateConfirm.Count = 0;

          return;
        }

        local.RecordCounter.Count = 0;

        foreach(var item in ReadKsDriversLicense13())
        {
          ++local.RecordCounter.Count;

          if (local.RecordCounter.Count > 1)
          {
            break;
          }

          export.KsDriversLicense.Assign(entities.KsDriversLicense);
        }

        if (!Lt(local.NullDate.Date,
          export.KsDriversLicense.PaymentAgreementDate))
        {
          export.KsDriversLicense.FirstPaymentDueDate =
            local.InitialisedKsDriversLicense.FirstPaymentDueDate;
        }

        if (export.KsDriversLicense.AmountDue.GetValueOrDefault() > 0)
        {
          local.AmountDueWorkArea.Text15 =
            NumberToString((long)(export.KsDriversLicense.AmountDue.
              GetValueOrDefault() * 100), 15);
          local.Current.Count = 1;
          local.CurrentPosition.Count = 1;

          do
          {
            local.Postion.Text1 =
              Substring(local.AmountDueWorkArea.Text15,
              local.CurrentPosition.Count, 1);

            if (AsChar(local.Postion.Text1) == '0')
            {
              local.Start.Count = local.CurrentPosition.Count + 1;
            }
            else
            {
              local.WorkArea.Text15 = "";
              local.WorkArea.Text15 =
                Substring(local.AmountDueWorkArea.Text15, local.Current.Count,
                16 - local.Current.Count - 2);
              local.WorkArea.Text2 =
                Substring(local.AmountDueWorkArea.Text15, 14, 2);
              local.AmountDueWorkArea.Text15 = "";
              local.AmountDueWorkArea.Text15 =
                TrimEnd(local.WorkArea.Text15) + "." + local.WorkArea.Text2;
              export.AmountDue.Text9 = local.AmountDueWorkArea.Text15;

              goto Test4;
            }

            ++local.CurrentPosition.Count;
            ++local.Current.Count;
          }
          while(!Equal(global.Command, "COMMAND"));
        }
        else
        {
          export.AmountDue.Text9 = "";
        }

Test4:

        export.HiddenKsDriversLicense.SequenceCounter =
          export.KsDriversLicense.SequenceCounter;
        export.LastUpdate.Date =
          Date(export.KsDriversLicense.LastUpdatedTstamp);

        if (local.RecordCounter.Count > 1)
        {
          // set the more +/- flag to +/-
          export.MoreLessScroll.Text3 = "+ -";
          export.HistoricRecord.Flag = "Y";

          var field15 = GetField(export.KsDriversLicense, "note1");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.KsDriversLicense, "note2");

          field16.Color = "cyan";
          field16.Protected = true;

          var field17 = GetField(export.KsDriversLicense, "note3");

          field17.Color = "cyan";
          field17.Protected = true;

          var field18 =
            GetField(export.KsDriversLicense, "paymentAgreementDate");

          field18.Color = "cyan";
          field18.Protected = true;

          var field19 =
            GetField(export.KsDriversLicense, "firstPaymentDueDate");

          field19.Color = "cyan";
          field19.Protected = true;

          var field20 = GetField(export.AmountDue, "text9");

          field20.Color = "cyan";
          field20.Protected = true;

          var field21 = GetField(export.KsDriversLicense, "serviceCompleteInd");

          field21.Color = "cyan";
          field21.Protected = true;

          var field22 =
            GetField(export.KsDriversLicense, "serviceCompleteDate");

          field22.Color = "cyan";
          field22.Protected = true;

          var field23 = GetField(export.KsDriversLicense, "appealReceivedDate");

          field23.Color = "cyan";
          field23.Protected = true;

          var field24 = GetField(export.KsDriversLicense, "appealResolved");

          field24.Color = "cyan";
          field24.Protected = true;

          var field25 = GetField(export.PromptAppealDecsionCod, "selectChar");

          field25.Color = "cyan";
          field25.Protected = true;
        }
        else if (local.RecordCounter.Count == 1)
        {
          export.MoreLessScroll.Text3 = "+";

          if (Equal(export.KsDriversLicense.RestrictionStatus,
            "REINSTATE MANUALLY") || Equal
            (export.KsDriversLicense.RestrictionStatus, "REINSTATED") || Equal
            (export.KsDriversLicense.RestrictionStatus, "DENIED RESTRICTION") ||
            !IsEmpty(export.KsDriversLicense.RecordClosureReason))
          {
            var field15 = GetField(export.KsDriversLicense, "note1");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 = GetField(export.KsDriversLicense, "note2");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 = GetField(export.KsDriversLicense, "note3");

            field17.Color = "cyan";
            field17.Protected = true;

            var field18 =
              GetField(export.KsDriversLicense, "paymentAgreementDate");

            field18.Color = "cyan";
            field18.Protected = true;

            var field19 =
              GetField(export.KsDriversLicense, "firstPaymentDueDate");

            field19.Color = "cyan";
            field19.Protected = true;

            var field20 = GetField(export.AmountDue, "text9");

            field20.Color = "cyan";
            field20.Protected = true;

            var field21 =
              GetField(export.KsDriversLicense, "serviceCompleteInd");

            field21.Color = "cyan";
            field21.Protected = true;

            var field22 =
              GetField(export.KsDriversLicense, "serviceCompleteDate");

            field22.Color = "cyan";
            field22.Protected = true;

            var field23 =
              GetField(export.KsDriversLicense, "appealReceivedDate");

            field23.Color = "cyan";
            field23.Protected = true;

            var field24 = GetField(export.KsDriversLicense, "appealResolved");

            field24.Color = "cyan";
            field24.Protected = true;

            var field25 = GetField(export.PromptAppealDecsionCod, "selectChar");

            field25.Color = "cyan";
            field25.Protected = true;

            break;
          }

          if (AsChar(export.KsDriversLicense.AppealResolved) == 'O')
          {
            var field15 = GetField(export.KsDriversLicense, "note1");

            field15.Color = "cyan";
            field15.Protected = true;

            var field16 = GetField(export.KsDriversLicense, "note2");

            field16.Color = "cyan";
            field16.Protected = true;

            var field17 = GetField(export.KsDriversLicense, "note3");

            field17.Color = "cyan";
            field17.Protected = true;

            var field18 =
              GetField(export.KsDriversLicense, "paymentAgreementDate");

            field18.Color = "cyan";
            field18.Protected = true;

            var field19 =
              GetField(export.KsDriversLicense, "firstPaymentDueDate");

            field19.Color = "cyan";
            field19.Protected = true;

            var field20 = GetField(export.AmountDue, "text9");

            field20.Color = "cyan";
            field20.Protected = true;

            var field21 =
              GetField(export.KsDriversLicense, "serviceCompleteInd");

            field21.Color = "cyan";
            field21.Protected = true;

            var field22 =
              GetField(export.KsDriversLicense, "serviceCompleteDate");

            field22.Color = "cyan";
            field22.Protected = true;

            var field23 =
              GetField(export.KsDriversLicense, "appealReceivedDate");

            field23.Color = "cyan";
            field23.Protected = true;

            var field24 = GetField(export.KsDriversLicense, "appealResolved");

            field24.Color = "cyan";
            field24.Protected = true;

            var field25 = GetField(export.PromptAppealDecsionCod, "selectChar");

            field25.Color = "cyan";
            field25.Protected = true;

            break;
          }

          // unprotect and protect fields as needed
          if (Lt(local.NullDate.Date,
            export.KsDriversLicense.Attribute30DayLetterCreatedDate) && AsChar
            (local.HistoricRec.Flag) != 'Y')
          {
            if (!Lt(local.NullDate.Date,
              export.KsDriversLicense.AppealReceivedDate))
            {
              var field15 =
                GetField(export.KsDriversLicense, "appealReceivedDate");

              field15.Protected = false;

              var field16 = GetField(export.KsDriversLicense, "appealResolved");

              field16.Protected = false;

              var field17 =
                GetField(export.PromptAppealDecsionCod, "selectChar");

              field17.Protected = false;
            }
            else
            {
              var field =
                GetField(export.KsDriversLicense, "appealReceivedDate");

              field.Color = "cyan";
              field.Protected = true;

              if (IsEmpty(export.KsDriversLicense.AppealResolved))
              {
                var field15 =
                  GetField(export.KsDriversLicense, "appealResolved");

                field15.Protected = false;

                var field16 =
                  GetField(export.PromptAppealDecsionCod, "selectChar");

                field16.Protected = false;
              }
              else
              {
                var field15 =
                  GetField(export.KsDriversLicense, "appealResolved");

                field15.Color = "cyan";
                field15.Protected = true;

                var field16 =
                  GetField(export.PromptAppealDecsionCod, "selectChar");

                field16.Color = "cyan";
                field16.Protected = true;
              }
            }

            if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N' || IsEmpty
              (export.KsDriversLicense.ServiceCompleteInd))
            {
              // if the service complete flag is 'Y' set the service complete 
              // field  to protected
              // if the service complete flag is 'N' do not set the service 
              // complete field  to protected
              // if the service complete date is completed set it to protected
              var field15 =
                GetField(export.KsDriversLicense, "serviceCompleteInd");

              field15.Protected = false;

              var field16 =
                GetField(export.KsDriversLicense, "serviceCompleteDate");

              field16.Protected = false;
            }
            else
            {
              var field15 =
                GetField(export.KsDriversLicense, "serviceCompleteInd");

              field15.Color = "cyan";
              field15.Protected = true;

              var field16 =
                GetField(export.KsDriversLicense, "serviceCompleteDate");

              field16.Color = "cyan";
              field16.Protected = true;
            }

            if (!Lt(local.NullDate.Date,
              export.KsDriversLicense.PaymentAgreementDate))
            {
              var field15 =
                GetField(export.KsDriversLicense, "paymentAgreementDate");

              field15.Protected = false;

              var field16 =
                GetField(export.KsDriversLicense, "firstPaymentDueDate");

              field16.Protected = false;

              var field17 = GetField(export.AmountDue, "text9");

              field17.Protected = false;
            }
            else
            {
              // If the payment agreement date is greater than a null date then 
              // protect the payment agreement date.
              var field15 =
                GetField(export.KsDriversLicense, "paymentAgreementDate");

              field15.Color = "cyan";
              field15.Protected = true;

              var field16 =
                GetField(export.KsDriversLicense, "firstPaymentDueDate");

              field16.Protected = false;

              var field17 = GetField(export.AmountDue, "text9");

              field17.Protected = false;
            }
          }
        }
        else
        {
          ExitState = "THERE_NO_PREVIOUS_RECORD_DISPLAY";

          if (!IsEmpty(export.KsDriversLicense.KsDvrLicense))
          {
            if (Equal(export.KsDriversLicense.RestrictionStatus,
              "REINSTATE MANUALLY") || Equal
              (export.KsDriversLicense.RestrictionStatus, "REINSTATED") || Equal
              (export.KsDriversLicense.RestrictionStatus, "DENIED RESTRICTION") ||
              !IsEmpty(export.KsDriversLicense.RecordClosureReason))
            {
              var field15 = GetField(export.KsDriversLicense, "note1");

              field15.Color = "cyan";
              field15.Protected = true;

              var field16 = GetField(export.KsDriversLicense, "note2");

              field16.Color = "cyan";
              field16.Protected = true;

              var field17 = GetField(export.KsDriversLicense, "note3");

              field17.Color = "cyan";
              field17.Protected = true;

              var field18 =
                GetField(export.KsDriversLicense, "paymentAgreementDate");

              field18.Color = "cyan";
              field18.Protected = true;

              var field19 =
                GetField(export.KsDriversLicense, "firstPaymentDueDate");

              field19.Color = "cyan";
              field19.Protected = true;

              var field20 = GetField(export.AmountDue, "text9");

              field20.Color = "cyan";
              field20.Protected = true;

              var field21 =
                GetField(export.KsDriversLicense, "serviceCompleteInd");

              field21.Color = "cyan";
              field21.Protected = true;

              var field22 =
                GetField(export.KsDriversLicense, "serviceCompleteDate");

              field22.Color = "cyan";
              field22.Protected = true;

              var field23 =
                GetField(export.KsDriversLicense, "appealReceivedDate");

              field23.Color = "cyan";
              field23.Protected = true;

              var field24 = GetField(export.KsDriversLicense, "appealResolved");

              field24.Color = "cyan";
              field24.Protected = true;

              var field25 =
                GetField(export.PromptAppealDecsionCod, "selectChar");

              field25.Color = "cyan";
              field25.Protected = true;

              break;
            }

            if (AsChar(export.KsDriversLicense.AppealResolved) == 'O')
            {
              var field15 = GetField(export.KsDriversLicense, "note1");

              field15.Color = "cyan";
              field15.Protected = true;

              var field16 = GetField(export.KsDriversLicense, "note2");

              field16.Color = "cyan";
              field16.Protected = true;

              var field17 = GetField(export.KsDriversLicense, "note3");

              field17.Color = "cyan";
              field17.Protected = true;

              var field18 =
                GetField(export.KsDriversLicense, "paymentAgreementDate");

              field18.Color = "cyan";
              field18.Protected = true;

              var field19 =
                GetField(export.KsDriversLicense, "firstPaymentDueDate");

              field19.Color = "cyan";
              field19.Protected = true;

              var field20 = GetField(export.AmountDue, "text9");

              field20.Color = "cyan";
              field20.Protected = true;

              var field21 =
                GetField(export.KsDriversLicense, "serviceCompleteInd");

              field21.Color = "cyan";
              field21.Protected = true;

              var field22 =
                GetField(export.KsDriversLicense, "serviceCompleteDate");

              field22.Color = "cyan";
              field22.Protected = true;

              var field23 =
                GetField(export.KsDriversLicense, "appealReceivedDate");

              field23.Color = "cyan";
              field23.Protected = true;

              var field24 = GetField(export.KsDriversLicense, "appealResolved");

              field24.Color = "cyan";
              field24.Protected = true;

              var field25 =
                GetField(export.PromptAppealDecsionCod, "selectChar");

              field25.Color = "cyan";
              field25.Protected = true;

              break;
            }

            // unprotect and protect fields as needed
            if (Lt(local.NullDate.Date,
              export.KsDriversLicense.Attribute30DayLetterCreatedDate) && AsChar
              (local.HistoricRec.Flag) != 'Y')
            {
              if (!Lt(local.NullDate.Date,
                export.KsDriversLicense.AppealReceivedDate))
              {
                var field15 =
                  GetField(export.KsDriversLicense, "appealReceivedDate");

                field15.Protected = false;

                var field16 =
                  GetField(export.KsDriversLicense, "appealResolved");

                field16.Protected = false;

                var field17 =
                  GetField(export.PromptAppealDecsionCod, "selectChar");

                field17.Protected = false;
              }
              else
              {
                var field =
                  GetField(export.KsDriversLicense, "appealReceivedDate");

                field.Color = "cyan";
                field.Protected = true;

                if (IsEmpty(export.KsDriversLicense.AppealResolved))
                {
                  var field15 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field15.Protected = false;

                  var field16 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field16.Protected = false;
                }
                else
                {
                  var field15 =
                    GetField(export.KsDriversLicense, "appealResolved");

                  field15.Color = "cyan";
                  field15.Protected = true;

                  var field16 =
                    GetField(export.PromptAppealDecsionCod, "selectChar");

                  field16.Color = "cyan";
                  field16.Protected = true;
                }
              }

              if (AsChar(export.KsDriversLicense.ServiceCompleteInd) == 'N' || IsEmpty
                (export.KsDriversLicense.ServiceCompleteInd))
              {
                // if the service complete flag is 'Y' set the service complete 
                // field  to protected
                // if the service complete flag is 'N' do not set the service 
                // complete field  to protected
                // if the service complete date is completed set it to protected
                var field15 =
                  GetField(export.KsDriversLicense, "serviceCompleteInd");

                field15.Protected = false;

                var field16 =
                  GetField(export.KsDriversLicense, "serviceCompleteDate");

                field16.Protected = false;
              }
              else
              {
                var field15 =
                  GetField(export.KsDriversLicense, "serviceCompleteInd");

                field15.Color = "cyan";
                field15.Protected = true;

                var field16 =
                  GetField(export.KsDriversLicense, "serviceCompleteDate");

                field16.Color = "cyan";
                field16.Protected = true;
              }

              if (!Lt(local.NullDate.Date,
                export.KsDriversLicense.PaymentAgreementDate))
              {
                var field15 =
                  GetField(export.KsDriversLicense, "paymentAgreementDate");

                field15.Protected = false;

                var field16 =
                  GetField(export.KsDriversLicense, "firstPaymentDueDate");

                field16.Protected = false;

                var field17 = GetField(export.AmountDue, "text9");

                field17.Protected = false;
              }
              else
              {
                // If the payment agreement date is greater than a null date 
                // then protect the payment agreement date.
                var field15 =
                  GetField(export.KsDriversLicense, "paymentAgreementDate");

                field15.Color = "cyan";
                field15.Protected = true;

                var field16 =
                  GetField(export.KsDriversLicense, "firstPaymentDueDate");

                field16.Protected = false;

                var field17 = GetField(export.AmountDue, "text9");

                field17.Protected = false;
              }
            }
          }
        }

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    export.HiddenPrevUserAction.Command = global.Command;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadKsDriversLicense1()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadKsDriversLicense1",
      (db, command) =>
      {
        db.SetString(command, "cspNum", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "validationDate",
          import.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "ltr30DayDate",
          import.KsDriversLicense.Attribute30DayLetterCreatedDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter", import.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 4);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 5);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 6);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 7);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 8);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 12);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 13);
        entities.N2dRead.Populated = true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense10()
  {
    entities.N2dRead.Populated = false;

    return ReadEach("ReadKsDriversLicense10",
      (db, command) =>
      {
        db.SetString(command, "cspNum", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "validationDate",
          import.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "ltr30DayDate",
          import.KsDriversLicense.Attribute30DayLetterCreatedDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter", import.KsDriversLicense.SequenceCounter);
        db.SetNullableDate(
          command, "firstPmntDueDt1",
          local.PaymentUpadate.FirstPaymentDueDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "firstPmntDueDt2", local.NullDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 4);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 5);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 6);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 7);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 8);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 12);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 13);
        entities.N2dRead.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense11()
  {
    entities.N2dRead.Populated = false;

    return ReadEach("ReadKsDriversLicense11",
      (db, command) =>
      {
        db.SetString(command, "cspNum", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "validationDate",
          import.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "ltr30DayDate",
          import.KsDriversLicense.Attribute30DayLetterCreatedDate.
            GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter", import.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 4);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 5);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 6);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 7);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 8);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 12);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 13);
        entities.N2dRead.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense12()
  {
    entities.N2dRead.Populated = false;

    return ReadEach("ReadKsDriversLicense12",
      (db, command) =>
      {
        db.SetString(command, "cspNum", import.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "validationDate",
          import.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "ltr30DayDate",
          import.KsDriversLicense.Attribute30DayLetterCreatedDate.
            GetValueOrDefault());
        db.SetNullableDate(
          command, "firstPmntDueDt",
          local.PaymentUpadate.FirstPaymentDueDate.GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter", import.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 4);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 5);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 6);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 7);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 8);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 12);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 13);
        entities.N2dRead.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense13()
  {
    entities.KsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense13",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetNullableDate(
          command, "validationDate",
          import.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter", import.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense14()
  {
    entities.KsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense14",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
        db.SetNullableDate(
          command, "validationDate",
          import.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter", import.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense15()
  {
    entities.KsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense15",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense16()
  {
    entities.KsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense16",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicense17()
  {
    entities.KsDriversLicense.Populated = false;

    return ReadEach("ReadKsDriversLicense17",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;

        return true;
      });
  }

  private bool ReadKsDriversLicense2()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadKsDriversLicense2",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetNullableDate(
          command, "validationDate",
          export.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter", export.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 4);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 5);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 6);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 7);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 8);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 12);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 13);
        entities.N2dRead.Populated = true;
      });
  }

  private bool ReadKsDriversLicense3()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadKsDriversLicense3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.ExistingLegalAction.Identifier);
        db.SetNullableDate(
          command, "validationDate",
          entities.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 4);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 5);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 6);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 7);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 8);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 12);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 13);
        entities.N2dRead.Populated = true;
      });
  }

  private bool ReadKsDriversLicense4()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadKsDriversLicense4",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePersonsWorkSet.Number);
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableDate(
          command, "validationDate",
          export.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter", export.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 4);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 5);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 6);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 7);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 8);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 12);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 13);
        entities.N2dRead.Populated = true;
      });
  }

  private bool ReadKsDriversLicense5()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadKsDriversLicense5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.ExistingLegalAction.Identifier);
        db.SetNullableDate(
          command, "validationDate",
          entities.KsDriversLicense.ValidationDate.GetValueOrDefault());
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.N2dRead.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.N2dRead.CspNum = db.GetString(reader, 1);
        entities.N2dRead.LgaIdentifier = db.GetNullableInt32(reader, 2);
        entities.N2dRead.ValidationDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 4);
        entities.N2dRead.ServiceCompleteInd = db.GetNullableString(reader, 5);
        entities.N2dRead.ServiceCompleteDate = db.GetNullableDate(reader, 6);
        entities.N2dRead.AppealReceivedDate = db.GetNullableDate(reader, 7);
        entities.N2dRead.AppealResolved = db.GetNullableString(reader, 8);
        entities.N2dRead.FirstPaymentDueDate = db.GetNullableDate(reader, 9);
        entities.N2dRead.AppealResolvedDate = db.GetNullableDate(reader, 10);
        entities.N2dRead.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.N2dRead.LastUpdatedTstamp = db.GetNullableDateTime(reader, 12);
        entities.N2dRead.SequenceCounter = db.GetInt32(reader, 13);
        entities.N2dRead.Populated = true;
      });
  }

  private bool ReadKsDriversLicense6()
  {
    entities.KsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense6",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequenceCounter", import.KsDriversLicense.SequenceCounter);
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;
      });
  }

  private bool ReadKsDriversLicense7()
  {
    entities.KsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense7",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequenceCounter", export.KsDriversLicense.SequenceCounter);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;
      });
  }

  private bool ReadKsDriversLicense8()
  {
    entities.KsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense8",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;
      });
  }

  private bool ReadKsDriversLicense9()
  {
    entities.KsDriversLicense.Populated = false;

    return Read("ReadKsDriversLicense9",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;
      });
  }

  private bool ReadKsDriversLicenseCsePerson()
  {
    entities.KsDriversLicense.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadKsDriversLicenseCsePerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequenceCounter", export.KsDriversLicense.SequenceCounter);
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.KsDriversLicense.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadKsDriversLicenseLegalAction1()
  {
    entities.KsDriversLicense.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadKsDriversLicenseLegalAction1",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 28);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 29);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 30);
        entities.KsDriversLicense.Populated = true;
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadKsDriversLicenseLegalAction2()
  {
    entities.KsDriversLicense.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadKsDriversLicenseLegalAction2",
      (db, command) =>
      {
        db.SetString(command, "cspNum", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 28);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 29);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 30);
        entities.KsDriversLicense.Populated = true;
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadKsDriversLicenseLegalAction3()
  {
    entities.KsDriversLicense.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadKsDriversLicenseLegalAction3",
      (db, command) =>
      {
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.KsDriversLicense.CreatedTstamp = db.GetDateTime(reader, 0);
        entities.KsDriversLicense.CspNum = db.GetString(reader, 1);
        entities.KsDriversLicense.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 2);
        entities.KsDriversLicense.KsDvrLicense =
          db.GetNullableString(reader, 3);
        entities.KsDriversLicense.ValidationDate =
          db.GetNullableDate(reader, 4);
        entities.KsDriversLicense.CourtesyLetterSentDate =
          db.GetNullableDate(reader, 5);
        entities.KsDriversLicense.Attribute30DayLetterCreatedDate =
          db.GetNullableDate(reader, 6);
        entities.KsDriversLicense.ServiceCompleteInd =
          db.GetNullableString(reader, 7);
        entities.KsDriversLicense.ServiceCompleteDate =
          db.GetNullableDate(reader, 8);
        entities.KsDriversLicense.RestrictedDate =
          db.GetNullableDate(reader, 9);
        entities.KsDriversLicense.ReinstatedDate =
          db.GetNullableDate(reader, 10);
        entities.KsDriversLicense.AppealReceivedDate =
          db.GetNullableDate(reader, 11);
        entities.KsDriversLicense.AppealResolved =
          db.GetNullableString(reader, 12);
        entities.KsDriversLicense.PaymentAgreementDate =
          db.GetNullableDate(reader, 13);
        entities.KsDriversLicense.FirstPaymentDueDate =
          db.GetNullableDate(reader, 14);
        entities.KsDriversLicense.AppealResolvedDate =
          db.GetNullableDate(reader, 15);
        entities.KsDriversLicense.LicenseRestrictionSentDate =
          db.GetNullableDate(reader, 16);
        entities.KsDriversLicense.RestrictionStatus =
          db.GetNullableString(reader, 17);
        entities.KsDriversLicense.AmountOwed =
          db.GetNullableDecimal(reader, 18);
        entities.KsDriversLicense.AmountDue = db.GetNullableDecimal(reader, 19);
        entities.KsDriversLicense.RecordClosureReason =
          db.GetNullableString(reader, 20);
        entities.KsDriversLicense.RecordClosureDate =
          db.GetNullableDate(reader, 21);
        entities.KsDriversLicense.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.KsDriversLicense.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.KsDriversLicense.Note1 = db.GetNullableString(reader, 24);
        entities.KsDriversLicense.Note2 = db.GetNullableString(reader, 25);
        entities.KsDriversLicense.Note3 = db.GetNullableString(reader, 26);
        entities.KsDriversLicense.SequenceCounter = db.GetInt32(reader, 27);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 28);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 29);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 30);
        entities.KsDriversLicense.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequenceCounter", export.KsDriversLicense.SequenceCounter);
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequenceCounter", import.KsDriversLicense.SequenceCounter);
        db.SetString(command, "cspNum", export.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction4()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction4",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction5()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction5",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction6()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction6",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.KsDriversLicense.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadObligationObligationType()
  {
    entities.ExistingObligation.Populated = false;
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.ExistingObligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 7);
        entities.ExistingObligation.Populated = true;
        entities.ExistingObligationType.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingObligation.PrimarySecondaryCode);
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
      });
  }

  private void UpdateKsDriversLicense1()
  {
    System.Diagnostics.Debug.Assert(entities.N2dRead.Populated);

    var appealReceivedDate = import.KsDriversLicense.AppealReceivedDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.N2dRead.Populated = false;
    Update("UpdateKsDriversLicense1",
      (db, command) =>
      {
        db.SetNullableDate(command, "appealRecDate", appealReceivedDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.N2dRead.CspNum);
        db.
          SetInt32(command, "sequenceCounter", entities.N2dRead.SequenceCounter);
          
      });

    entities.N2dRead.AppealReceivedDate = appealReceivedDate;
    entities.N2dRead.LastUpdatedBy = lastUpdatedBy;
    entities.N2dRead.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.N2dRead.Populated = true;
  }

  private void UpdateKsDriversLicense2()
  {
    System.Diagnostics.Debug.Assert(entities.N2dRead.Populated);

    var firstPaymentDueDate = import.KsDriversLicense.FirstPaymentDueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.N2dRead.Populated = false;
    Update("UpdateKsDriversLicense2",
      (db, command) =>
      {
        db.SetNullableDate(command, "firstPmntDueDt", firstPaymentDueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.N2dRead.CspNum);
        db.
          SetInt32(command, "sequenceCounter", entities.N2dRead.SequenceCounter);
          
      });

    entities.N2dRead.FirstPaymentDueDate = firstPaymentDueDate;
    entities.N2dRead.LastUpdatedBy = lastUpdatedBy;
    entities.N2dRead.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.N2dRead.Populated = true;
  }

  private void UpdateKsDriversLicense3()
  {
    System.Diagnostics.Debug.Assert(entities.N2dRead.Populated);

    var serviceCompleteInd = import.KsDriversLicense.ServiceCompleteInd ?? "";
    var serviceCompleteDate = import.KsDriversLicense.ServiceCompleteDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.N2dRead.Populated = false;
    Update("UpdateKsDriversLicense3",
      (db, command) =>
      {
        db.SetNullableString(command, "servCompleteInd", serviceCompleteInd);
        db.SetNullableDate(command, "servCompleteDt", serviceCompleteDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "cspNum", entities.N2dRead.CspNum);
        db.
          SetInt32(command, "sequenceCounter", entities.N2dRead.SequenceCounter);
          
      });

    entities.N2dRead.ServiceCompleteInd = serviceCompleteInd;
    entities.N2dRead.ServiceCompleteDate = serviceCompleteDate;
    entities.N2dRead.LastUpdatedBy = lastUpdatedBy;
    entities.N2dRead.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.N2dRead.Populated = true;
  }

  private void UpdateKsDriversLicense4()
  {
    System.Diagnostics.Debug.Assert(entities.KsDriversLicense.Populated);

    var serviceCompleteInd = import.KsDriversLicense.ServiceCompleteInd ?? "";
    var serviceCompleteDate = import.KsDriversLicense.ServiceCompleteDate;
    var appealReceivedDate = import.KsDriversLicense.AppealReceivedDate;
    var appealResolved = import.KsDriversLicense.AppealResolved ?? "";
    var paymentAgreementDate = import.KsDriversLicense.PaymentAgreementDate;
    var firstPaymentDueDate = export.KsDriversLicense.FirstPaymentDueDate;
    var appealResolvedDate = export.KsDriversLicense.AppealResolvedDate;
    var amountDue = export.KsDriversLicense.AmountDue.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var note1 = import.KsDriversLicense.Note1 ?? "";
    var note2 = import.KsDriversLicense.Note2 ?? "";
    var note3 = import.KsDriversLicense.Note3 ?? "";

    entities.KsDriversLicense.Populated = false;
    Update("UpdateKsDriversLicense4",
      (db, command) =>
      {
        db.SetNullableString(command, "servCompleteInd", serviceCompleteInd);
        db.SetNullableDate(command, "servCompleteDt", serviceCompleteDate);
        db.SetNullableDate(command, "appealRecDate", appealReceivedDate);
        db.SetNullableString(command, "appealResolved", appealResolved);
        db.SetNullableDate(command, "paymentAgrmntDt", paymentAgreementDate);
        db.SetNullableDate(command, "firstPmntDueDt", firstPaymentDueDate);
        db.SetNullableDate(command, "appealReslvdDt", appealResolvedDate);
        db.SetNullableDecimal(command, "amountDue", amountDue);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "note1", note1);
        db.SetNullableString(command, "note2", note2);
        db.SetNullableString(command, "note3", note3);
        db.SetString(command, "cspNum", entities.KsDriversLicense.CspNum);
        db.SetInt32(
          command, "sequenceCounter",
          entities.KsDriversLicense.SequenceCounter);
      });

    entities.KsDriversLicense.ServiceCompleteInd = serviceCompleteInd;
    entities.KsDriversLicense.ServiceCompleteDate = serviceCompleteDate;
    entities.KsDriversLicense.AppealReceivedDate = appealReceivedDate;
    entities.KsDriversLicense.AppealResolved = appealResolved;
    entities.KsDriversLicense.PaymentAgreementDate = paymentAgreementDate;
    entities.KsDriversLicense.FirstPaymentDueDate = firstPaymentDueDate;
    entities.KsDriversLicense.AppealResolvedDate = appealResolvedDate;
    entities.KsDriversLicense.AmountDue = amountDue;
    entities.KsDriversLicense.LastUpdatedBy = lastUpdatedBy;
    entities.KsDriversLicense.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.KsDriversLicense.Note1 = note1;
    entities.KsDriversLicense.Note2 = note2;
    entities.KsDriversLicense.Note3 = note3;
    entities.KsDriversLicense.Populated = true;
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
    /// <summary>
    /// A value of HiddenKsDriversLicense.
    /// </summary>
    [JsonPropertyName("hiddenKsDriversLicense")]
    public KsDriversLicense HiddenKsDriversLicense
    {
      get => hiddenKsDriversLicense ??= new();
      set => hiddenKsDriversLicense = value;
    }

    /// <summary>
    /// A value of PromptCsePersonNumber.
    /// </summary>
    [JsonPropertyName("promptCsePersonNumber")]
    public Common PromptCsePersonNumber
    {
      get => promptCsePersonNumber ??= new();
      set => promptCsePersonNumber = value;
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
    /// A value of UpdateConfirm.
    /// </summary>
    [JsonPropertyName("updateConfirm")]
    public Common UpdateConfirm
    {
      get => updateConfirm ??= new();
      set => updateConfirm = value;
    }

    /// <summary>
    /// A value of OkUpdatePaymentAgr2.
    /// </summary>
    [JsonPropertyName("okUpdatePaymentAgr2")]
    public Common OkUpdatePaymentAgr2
    {
      get => okUpdatePaymentAgr2 ??= new();
      set => okUpdatePaymentAgr2 = value;
    }

    /// <summary>
    /// A value of MoreLessScroll.
    /// </summary>
    [JsonPropertyName("moreLessScroll")]
    public WorkArea MoreLessScroll
    {
      get => moreLessScroll ??= new();
      set => moreLessScroll = value;
    }

    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
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
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public CodeValue DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
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
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of PromptAppealDecisionCd.
    /// </summary>
    [JsonPropertyName("promptAppealDecisionCd")]
    public Common PromptAppealDecisionCd
    {
      get => promptAppealDecisionCd ??= new();
      set => promptAppealDecisionCd = value;
    }

    /// <summary>
    /// A value of HistoricRecord.
    /// </summary>
    [JsonPropertyName("historicRecord")]
    public Common HistoricRecord
    {
      get => historicRecord ??= new();
      set => historicRecord = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPrevCsePersonsWorkSet
    {
      get => hiddenPrevCsePersonsWorkSet ??= new();
      set => hiddenPrevCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalAction")]
    public LegalAction HiddenPrevLegalAction
    {
      get => hiddenPrevLegalAction ??= new();
      set => hiddenPrevLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
    }

    /// <summary>
    /// A value of FlowCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("flowCsePersonsWorkSet")]
    public CsePersonsWorkSet FlowCsePersonsWorkSet
    {
      get => flowCsePersonsWorkSet ??= new();
      set => flowCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AmountDue.
    /// </summary>
    [JsonPropertyName("amountDue")]
    public WorkArea AmountDue
    {
      get => amountDue ??= new();
      set => amountDue = value;
    }

    /// <summary>
    /// A value of FlowKsDriversLicense.
    /// </summary>
    [JsonPropertyName("flowKsDriversLicense")]
    public KsDriversLicense FlowKsDriversLicense
    {
      get => flowKsDriversLicense ??= new();
      set => flowKsDriversLicense = value;
    }

    private KsDriversLicense hiddenKsDriversLicense;
    private Common promptCsePersonNumber;
    private DateWorkArea lastUpdate;
    private Common updateConfirm;
    private Common okUpdatePaymentAgr2;
    private WorkArea moreLessScroll;
    private KsDriversLicense ksDriversLicense;
    private Standard standard;
    private CodeValue dlgflwSelected;
    private Common hiddenDisplayPerformed;
    private Common hiddenPrevUserAction;
    private Common promptAppealDecisionCd;
    private Common historicRecord;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private LegalAction hiddenPrevLegalAction;
    private Security2 hiddenSecurity;
    private CsePersonsWorkSet flowCsePersonsWorkSet;
    private WorkArea amountDue;
    private KsDriversLicense flowKsDriversLicense;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HiddenKsDriversLicense.
    /// </summary>
    [JsonPropertyName("hiddenKsDriversLicense")]
    public KsDriversLicense HiddenKsDriversLicense
    {
      get => hiddenKsDriversLicense ??= new();
      set => hiddenKsDriversLicense = value;
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
    /// A value of DlgflwRequired.
    /// </summary>
    [JsonPropertyName("dlgflwRequired")]
    public Code DlgflwRequired
    {
      get => dlgflwRequired ??= new();
      set => dlgflwRequired = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of LastUpdate.
    /// </summary>
    [JsonPropertyName("lastUpdate")]
    public DateWorkArea LastUpdate
    {
      get => lastUpdate ??= new();
      set => lastUpdate = value;
    }

    /// <summary>
    /// A value of UpdateConfirm.
    /// </summary>
    [JsonPropertyName("updateConfirm")]
    public Common UpdateConfirm
    {
      get => updateConfirm ??= new();
      set => updateConfirm = value;
    }

    /// <summary>
    /// A value of OkToUpdatePaymentAgre.
    /// </summary>
    [JsonPropertyName("okToUpdatePaymentAgre")]
    public Common OkToUpdatePaymentAgre
    {
      get => okToUpdatePaymentAgre ??= new();
      set => okToUpdatePaymentAgre = value;
    }

    /// <summary>
    /// A value of MoreLessScroll.
    /// </summary>
    [JsonPropertyName("moreLessScroll")]
    public WorkArea MoreLessScroll
    {
      get => moreLessScroll ??= new();
      set => moreLessScroll = value;
    }

    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
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
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public Code Required
    {
      get => required ??= new();
      set => required = value;
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
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of PromptCsePersonNumber.
    /// </summary>
    [JsonPropertyName("promptCsePersonNumber")]
    public Common PromptCsePersonNumber
    {
      get => promptCsePersonNumber ??= new();
      set => promptCsePersonNumber = value;
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
    /// A value of HistoricRecord.
    /// </summary>
    [JsonPropertyName("historicRecord")]
    public Common HistoricRecord
    {
      get => historicRecord ??= new();
      set => historicRecord = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of PromptAppealDecsionCod.
    /// </summary>
    [JsonPropertyName("promptAppealDecsionCod")]
    public Common PromptAppealDecsionCod
    {
      get => promptAppealDecsionCod ??= new();
      set => promptAppealDecsionCod = value;
    }

    /// <summary>
    /// A value of HiddenPrevCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenPrevCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenPrevCsePersonsWorkSet
    {
      get => hiddenPrevCsePersonsWorkSet ??= new();
      set => hiddenPrevCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalAction")]
    public LegalAction HiddenPrevLegalAction
    {
      get => hiddenPrevLegalAction ??= new();
      set => hiddenPrevLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
    }

    /// <summary>
    /// A value of AmountDue.
    /// </summary>
    [JsonPropertyName("amountDue")]
    public WorkArea AmountDue
    {
      get => amountDue ??= new();
      set => amountDue = value;
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public CsePersonsWorkSet Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    private KsDriversLicense hiddenKsDriversLicense;
    private ObligationType obligationType;
    private Code dlgflwRequired;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson csePerson;
    private DateWorkArea lastUpdate;
    private Common updateConfirm;
    private Common okToUpdatePaymentAgre;
    private WorkArea moreLessScroll;
    private KsDriversLicense ksDriversLicense;
    private Standard standard;
    private Code required;
    private Common hiddenDisplayPerformed;
    private Common hiddenPrevUserAction;
    private Common promptCsePersonNumber;
    private Obligation obligation;
    private Common historicRecord;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private AdministrativeAction administrativeAction;
    private Common promptAppealDecsionCod;
    private CsePersonsWorkSet hiddenPrevCsePersonsWorkSet;
    private LegalAction hiddenPrevLegalAction;
    private Security2 hiddenSecurity;
    private WorkArea amountDue;
    private CsePersonsWorkSet flow;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Deatil.
      /// </summary>
      [JsonPropertyName("deatil")]
      public CsePersonsWorkSet Deatil
      {
        get => deatil ??= new();
        set => deatil = value;
      }

      /// <summary>
      /// A value of DetailAlt.
      /// </summary>
      [JsonPropertyName("detailAlt")]
      public Common DetailAlt
      {
        get => detailAlt ??= new();
        set => detailAlt = value;
      }

      /// <summary>
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private CsePersonsWorkSet deatil;
      private Common detailAlt;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
    }

    /// <summary>
    /// A value of ServiceCompletedCheck.
    /// </summary>
    [JsonPropertyName("serviceCompletedCheck")]
    public Common ServiceCompletedCheck
    {
      get => serviceCompletedCheck ??= new();
      set => serviceCompletedCheck = value;
    }

    /// <summary>
    /// A value of PaymentUpadate.
    /// </summary>
    [JsonPropertyName("paymentUpadate")]
    public KsDriversLicense PaymentUpadate
    {
      get => paymentUpadate ??= new();
      set => paymentUpadate = value;
    }

    /// <summary>
    /// A value of AmountDue3.
    /// </summary>
    [JsonPropertyName("amountDue3")]
    public WorkArea AmountDue3
    {
      get => amountDue3 ??= new();
      set => amountDue3 = value;
    }

    /// <summary>
    /// A value of AmountDue2.
    /// </summary>
    [JsonPropertyName("amountDue2")]
    public WorkArea AmountDue2
    {
      get => amountDue2 ??= new();
      set => amountDue2 = value;
    }

    /// <summary>
    /// A value of Dollars.
    /// </summary>
    [JsonPropertyName("dollars")]
    public Common Dollars
    {
      get => dollars ??= new();
      set => dollars = value;
    }

    /// <summary>
    /// A value of Cents.
    /// </summary>
    [JsonPropertyName("cents")]
    public Common Cents
    {
      get => cents ??= new();
      set => cents = value;
    }

    /// <summary>
    /// A value of AmountDueWorkArea.
    /// </summary>
    [JsonPropertyName("amountDueWorkArea")]
    public WorkArea AmountDueWorkArea
    {
      get => amountDueWorkArea ??= new();
      set => amountDueWorkArea = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public LegalAction Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of UpdateError.
    /// </summary>
    [JsonPropertyName("updateError")]
    public Common UpdateError
    {
      get => updateError ??= new();
      set => updateError = value;
    }

    /// <summary>
    /// A value of InitialisedKsDriversLicense.
    /// </summary>
    [JsonPropertyName("initialisedKsDriversLicense")]
    public KsDriversLicense InitialisedKsDriversLicense
    {
      get => initialisedKsDriversLicense ??= new();
      set => initialisedKsDriversLicense = value;
    }

    /// <summary>
    /// A value of NeedToOkPaymentUpdate.
    /// </summary>
    [JsonPropertyName("needToOkPaymentUpdate")]
    public Common NeedToOkPaymentUpdate
    {
      get => needToOkPaymentUpdate ??= new();
      set => needToOkPaymentUpdate = value;
    }

    /// <summary>
    /// A value of RecordCounter.
    /// </summary>
    [JsonPropertyName("recordCounter")]
    public Common RecordCounter
    {
      get => recordCounter ??= new();
      set => recordCounter = value;
    }

    /// <summary>
    /// A value of HistoricRec.
    /// </summary>
    [JsonPropertyName("historicRec")]
    public Common HistoricRec
    {
      get => historicRec ??= new();
      set => historicRec = value;
    }

    /// <summary>
    /// A value of NumberOfCtOrders.
    /// </summary>
    [JsonPropertyName("numberOfCtOrders")]
    public Common NumberOfCtOrders
    {
      get => numberOfCtOrders ??= new();
      set => numberOfCtOrders = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public KsDriversLicense CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of NumberOfRecordsFound.
    /// </summary>
    [JsonPropertyName("numberOfRecordsFound")]
    public Common NumberOfRecordsFound
    {
      get => numberOfRecordsFound ??= new();
      set => numberOfRecordsFound = value;
    }

    /// <summary>
    /// A value of MoreAppealRecsToClose.
    /// </summary>
    [JsonPropertyName("moreAppealRecsToClose")]
    public Common MoreAppealRecsToClose
    {
      get => moreAppealRecsToClose ??= new();
      set => moreAppealRecsToClose = value;
    }

    /// <summary>
    /// A value of InitialisedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("initialisedCsePersonsWorkSet")]
    public CsePersonsWorkSet InitialisedCsePersonsWorkSet
    {
      get => initialisedCsePersonsWorkSet ??= new();
      set => initialisedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of InitialisedLegalAction.
    /// </summary>
    [JsonPropertyName("initialisedLegalAction")]
    public LegalAction InitialisedLegalAction
    {
      get => initialisedLegalAction ??= new();
      set => initialisedLegalAction = value;
    }

    /// <summary>
    /// A value of Rollback.
    /// </summary>
    [JsonPropertyName("rollback")]
    public Common Rollback
    {
      get => rollback ??= new();
      set => rollback = value;
    }

    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
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
    /// A value of InitialisedServiceProvider.
    /// </summary>
    [JsonPropertyName("initialisedServiceProvider")]
    public ServiceProvider InitialisedServiceProvider
    {
      get => initialisedServiceProvider ??= new();
      set => initialisedServiceProvider = value;
    }

    /// <summary>
    /// A value of HighlightError.
    /// </summary>
    [JsonPropertyName("highlightError")]
    public Common HighlightError
    {
      get => highlightError ??= new();
      set => highlightError = value;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of SearchPerson.
    /// </summary>
    [JsonPropertyName("searchPerson")]
    public Common SearchPerson
    {
      get => searchPerson ??= new();
      set => searchPerson = value;
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
    /// A value of DisplayAnyObligation.
    /// </summary>
    [JsonPropertyName("displayAnyObligation")]
    public Common DisplayAnyObligation
    {
      get => displayAnyObligation ??= new();
      set => displayAnyObligation = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of NoOfLegalActionsFound.
    /// </summary>
    [JsonPropertyName("noOfLegalActionsFound")]
    public Common NoOfLegalActionsFound
    {
      get => noOfLegalActionsFound ??= new();
      set => noOfLegalActionsFound = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of ReturnedFromLacn.
    /// </summary>
    [JsonPropertyName("returnedFromLacn")]
    public Common ReturnedFromLacn
    {
      get => returnedFromLacn ??= new();
      set => returnedFromLacn = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public Common Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of FieldNumber.
    /// </summary>
    [JsonPropertyName("fieldNumber")]
    public Common FieldNumber
    {
      get => fieldNumber ??= new();
      set => fieldNumber = value;
    }

    /// <summary>
    /// A value of AmountDueCommon.
    /// </summary>
    [JsonPropertyName("amountDueCommon")]
    public Common AmountDueCommon
    {
      get => amountDueCommon ??= new();
      set => amountDueCommon = value;
    }

    private Common serviceCompletedCheck;
    private KsDriversLicense paymentUpadate;
    private WorkArea amountDue3;
    private WorkArea amountDue2;
    private Common dollars;
    private Common cents;
    private WorkArea amountDueWorkArea;
    private LegalAction hold;
    private Common updateError;
    private KsDriversLicense initialisedKsDriversLicense;
    private Common needToOkPaymentUpdate;
    private Common recordCounter;
    private Common historicRec;
    private Common numberOfCtOrders;
    private KsDriversLicense currentDate;
    private Common numberOfRecordsFound;
    private Common moreAppealRecsToClose;
    private CsePersonsWorkSet initialisedCsePersonsWorkSet;
    private LegalAction initialisedLegalAction;
    private Common rollback;
    private Common promptCount;
    private TextWorkArea textWorkArea;
    private ServiceProvider initialisedServiceProvider;
    private Common highlightError;
    private Common lastErrorEntryNo;
    private Array<LocalGroup> local1;
    private Common searchPerson;
    private CsePerson csePerson;
    private Common displayAnyObligation;
    private Common userAction;
    private Common noOfLegalActionsFound;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private DateWorkArea initialisedToZeros;
    private Common returnedFromLacn;
    private NextTranInfo nextTranInfo;
    private DateWorkArea nullDate;
    private TextWorkArea postion;
    private Common currentPosition;
    private WorkArea workArea;
    private Common current;
    private Common start;
    private Common fieldNumber;
    private Common amountDueCommon;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of N2dRead.
    /// </summary>
    [JsonPropertyName("n2dRead")]
    public KsDriversLicense N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
    }

    /// <summary>
    /// A value of KsDriversLicense.
    /// </summary>
    [JsonPropertyName("ksDriversLicense")]
    public KsDriversLicense KsDriversLicense
    {
      get => ksDriversLicense ??= new();
      set => ksDriversLicense = value;
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
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
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
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnly.
    /// </summary>
    [JsonPropertyName("existingKeyOnly")]
    public CsePersonAccount ExistingKeyOnly
    {
      get => existingKeyOnly ??= new();
      set => existingKeyOnly = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePerson ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    private KsDriversLicense n2dRead;
    private KsDriversLicense ksDriversLicense;
    private CsePerson csePerson;
    private LegalAction existingLegalAction;
    private Obligation existingObligation;
    private ObligationType existingObligationType;
    private CsePersonAccount existingKeyOnly;
    private CsePerson existingObligor;
  }
#endregion
}
