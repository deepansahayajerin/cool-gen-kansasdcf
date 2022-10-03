// Program: FN_OPMT_LST_MTN_OVRPYMT_INTENT, ID: 372045072, model: 746.
// Short name: SWEOPMTP
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
/// A program: FN_OPMT_LST_MTN_OVRPYMT_INTENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOpmtLstMtnOvrpymtIntent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OPMT_LST_MTN_OVRPYMT_INTENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOpmtLstMtnOvrpymtIntent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOpmtLstMtnOvrpymtIntent.
  /// </summary>
  public FnOpmtLstMtnOvrpymtIntent(IContext context, Import import,
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
    // ------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 07/29/96   G. Lofton - MTW			Initial code
    // 10/07/98   G. Sharp -SCB        Phase 2         changes: clean up exit 
    // state's that have zdelete.
    // ------------------------------------------------------------------
    // ------------------------------------------------------------------
    // 09/28/99, M. Brown, Problem report #73652: Added a permitted
    // value of 'C'ancel to enable discontinue of an overpayment intent.
    // Added logic to display the literal for the new value, as
    // well as an edit to ensure that this value is not entered
    // when no other overpayment records exist.
    // ------------------------------------------------------------------
    // ------------------------------------------------------------------
    // 09/26/00, E. Shirk   PR#103718    Disable the 'F'uture overpayment intent
    // logic.   The 'F' types will remain on the database, therefore the 'F'
    // type formatting will remain in the DISPLAY logic section, but no new 'F'
    // types will be added.
    // 08/12/02  K Doshi         PR149011     Fix screen Help Id.
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.DateWorkArea.Date = Now().Date;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ***************************************************
    // Move Imports to Exports.
    // ***************************************************
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

    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.Common.ActionEntry =
          import.Import1.Item.Common.ActionEntry;
        export.Export1.Update.DetOverpaymentHistory.Assign(
          import.Import1.Item.DetOverpaymentHistory);
        export.Export1.Update.DetTextWorkArea.Text10 =
          import.Import1.Item.DetTextWorkArea.Text10;
        export.Export1.Next();
      }
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.CsePerson.Number = local.NextTranInfo.CsePersonNumber ?? Spaces
        (10);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      local.NextTranInfo.CsePersonNumber = export.CsePerson.Number;
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

          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

          return;
        }

        export.HiddenPrev.Number = export.CsePerson.Number;
        export.HiddenPrevHistory.Flag = export.History.Flag;

        if (!ReadCsePersonAccount())
        {
          ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";

          return;
        }

        local.EndRead.Flag = "N";

        if (AsChar(export.History.Flag) == 'N')
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadOverpaymentHistory2())
          {
            if (!Lt(Now().Date, entities.OverpaymentHistory.EffectiveDt))
            {
              local.EndRead.Flag = "Y";
            }

            export.Export1.Update.Common.ActionEntry = "";
            export.Export1.Update.DetOverpaymentHistory.Assign(
              entities.OverpaymentHistory);

            if (AsChar(export.Export1.Item.DetOverpaymentHistory.OverpaymentInd) ==
              'F')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "FUTURE";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'G')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "GIFT";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'R')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "REFUND";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'N')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "NOTICE";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'C')
            {
              // 09/28/99, M. Brown, problem report #73652
              export.Export1.Update.DetTextWorkArea.Text10 = "CANCEL";
            }
            else
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "";
            }

            if (AsChar(local.EndRead.Flag) == 'Y')
            {
              export.Export1.Next();

              break;
            }

            export.Export1.Next();
          }
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadOverpaymentHistory1())
          {
            export.Export1.Update.Common.ActionEntry = "";
            export.Export1.Update.DetOverpaymentHistory.Assign(
              entities.OverpaymentHistory);

            if (AsChar(export.Export1.Item.DetOverpaymentHistory.OverpaymentInd) ==
              'F')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "FUTURE";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'G')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "GIFT";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'R')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "REFUND";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'N')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "NOTICE";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'C')
            {
              // 09/28/99, M. Brown, problem report #73652
              export.Export1.Update.DetTextWorkArea.Text10 = "CANCEL";
            }
            else
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "";
            }

            export.Export1.Next();
          }
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

          return;
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "FN0000_NO_OVERPAYMENT_INTENT_FND";
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

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        if (AsChar(export.History.Flag) != AsChar
          (export.HiddenPrevHistory.Flag))
        {
          ExitState = "ACO_NE0000_DISPLAY_REQD_NEW_SRCH";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          return;
        }

        // : VALIDATION CASE OF COMMAND
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.Common.ActionEntry, "A") || Equal
            (export.Export1.Item.Common.ActionEntry, "D"))
          {
            // *****  Make sure all mandatory fields have been entered. *****
            // *****  'F'uture type overpayment ind is valid only on a Delete 
            // action.. *****
            if (AsChar(export.Export1.Item.DetOverpaymentHistory.OverpaymentInd) ==
              'G' || AsChar
              (export.Export1.Item.DetOverpaymentHistory.OverpaymentInd) == 'R'
              || AsChar
              (export.Export1.Item.DetOverpaymentHistory.OverpaymentInd) == 'C'
              || AsChar
              (export.Export1.Item.DetOverpaymentHistory.OverpaymentInd) == 'F'
              && Equal(export.Export1.Item.Common.ActionEntry, "D"))
            {
            }
            else if (IsEmpty(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd))
            {
              var field =
                GetField(export.Export1.Item.DetOverpaymentHistory,
                "overpaymentInd");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              return;
            }
            else
            {
              // ***** 'N'otice Sent are for display purposes.   They should not
              // be added or deleted.  *****
              if (AsChar(export.Export1.Item.DetOverpaymentHistory.
                OverpaymentInd) == 'N')
              {
                if (Equal(export.Export1.Item.Common.ActionEntry, "A"))
                {
                  var field =
                    GetField(export.Export1.Item.DetOverpaymentHistory,
                    "overpaymentInd");

                  field.Error = true;

                  ExitState = "FN0000_INVALID_OVERPAYMNT_INTENT";

                  return;
                }

                if (Equal(export.Export1.Item.Common.ActionEntry, "D"))
                {
                  var field =
                    GetField(export.Export1.Item.Common, "actionEntry");

                  field.Error = true;

                  ExitState = "FN0000_OVERPAYMENT_INTENT_ACTIVE";

                  return;
                }
              }
              else
              {
                var field =
                  GetField(export.Export1.Item.DetOverpaymentHistory,
                  "overpaymentInd");

                field.Error = true;

                ExitState = "FN0000_INVALID_OVERPAYMNT_INTENT";

                return;
              }
            }

            if (!Lt(local.Blank.Date,
              export.Export1.Item.DetOverpaymentHistory.EffectiveDt))
            {
              var field =
                GetField(export.Export1.Item.DetOverpaymentHistory,
                "effectiveDt");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              return;
            }

            local.SelectionMade.Flag = "Y";
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
                ExitState = "INVALID_ACTION_ENTER_A_OR_D";

                var field = GetField(export.Export1.Item.Common, "actionEntry");

                field.Error = true;

                return;
            }
          }
        }

        if (AsChar(local.SelectionMade.Flag) != 'Y')
        {
          ExitState = "FN0000_ACTION_OF_A_OR_D_REQUIRED";

          return;
        }

        // PROCESS ACTIONS - ADD OR DELETE.
        // ** Delete Statement/Coupon Processing **
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (Equal(export.Export1.Item.Common.ActionEntry, "D"))
          {
            if (Lt(export.Export1.Item.DetOverpaymentHistory.EffectiveDt,
              local.DateWorkArea.Date))
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

              ExitState = "FN0000_OVERPAYMENT_INTENT_ACTIVE";

              return;
            }

            UseFnDeleteOverpaymentHistory();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Common.ActionEntry = "*";
            }
            else
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

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
            if (AsChar(export.Export1.Item.DetOverpaymentHistory.OverpaymentInd) ==
              'G')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "GIFT";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'R')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "REFUND";
            }
            else if (AsChar(export.Export1.Item.DetOverpaymentHistory.
              OverpaymentInd) == 'C')
            {
              export.Export1.Update.DetTextWorkArea.Text10 = "CANCEL";
            }

            if (Lt(export.Export1.Item.DetOverpaymentHistory.EffectiveDt,
              local.DateWorkArea.Date))
            {
              var field1 = GetField(export.Export1.Item.Common, "actionEntry");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetOverpaymentHistory,
                "effectiveDt");

              field2.Error = true;

              ExitState = "EFF_DTE_MUST_BE_EQ_GT_CURRENT_DT";

              return;
            }

            UseFnValidateOverpaymentHistDts();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else if (IsExitState("FN0000_CANNOT_CANCEL_NO_OVERPYMT"))
            {
              // 09/28/99, M. Brown, problem report #73652
              var field =
                GetField(export.Export1.Item.DetOverpaymentHistory,
                "overpaymentInd");

              field.Error = true;

              return;
            }
            else
            {
              var field = GetField(export.Export1.Item.Common, "actionEntry");

              field.Error = true;

              return;
            }

            UseFnCreateOverpaymentHistory();

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
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

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
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveOverpaymentHistory(OverpaymentHistory source,
    OverpaymentHistory target)
  {
    target.OverpaymentInd = source.OverpaymentInd;
    target.EffectiveDt = source.EffectiveDt;
  }

  private void UseFnCreateOverpaymentHistory()
  {
    var useImport = new FnCreateOverpaymentHistory.Import();
    var useExport = new FnCreateOverpaymentHistory.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveOverpaymentHistory(export.Export1.Item.DetOverpaymentHistory,
      useImport.OverpaymentHistory);

    Call(FnCreateOverpaymentHistory.Execute, useImport, useExport);

    export.Export1.Update.DetOverpaymentHistory.Assign(
      useExport.OverpaymentHistory);
  }

  private void UseFnDeleteOverpaymentHistory()
  {
    var useImport = new FnDeleteOverpaymentHistory.Import();
    var useExport = new FnDeleteOverpaymentHistory.Export();

    MoveOverpaymentHistory(export.Export1.Item.DetOverpaymentHistory,
      useImport.OverpaymentHistory);
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(FnDeleteOverpaymentHistory.Execute, useImport, useExport);
  }

  private void UseFnValidateOverpaymentHistDts()
  {
    var useImport = new FnValidateOverpaymentHistDts.Import();
    var useExport = new FnValidateOverpaymentHistDts.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveOverpaymentHistory(export.Export1.Item.DetOverpaymentHistory,
      useImport.OverpaymentHistory);

    Call(FnValidateOverpaymentHistDts.Execute, useImport, useExport);
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

  private IEnumerable<bool> ReadOverpaymentHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    return ReadEach("ReadOverpaymentHistory1",
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

        entities.OverpaymentHistory.CpaType = db.GetString(reader, 0);
        entities.OverpaymentHistory.CspNumber = db.GetString(reader, 1);
        entities.OverpaymentHistory.EffectiveDt = db.GetDate(reader, 2);
        entities.OverpaymentHistory.OverpaymentInd = db.GetString(reader, 3);
        entities.OverpaymentHistory.CreatedBy = db.GetString(reader, 4);
        entities.OverpaymentHistory.CreatedTmst = db.GetDateTime(reader, 5);
        entities.OverpaymentHistory.Populated = true;
        CheckValid<OverpaymentHistory>("CpaType",
          entities.OverpaymentHistory.CpaType);
        CheckValid<OverpaymentHistory>("OverpaymentInd",
          entities.OverpaymentHistory.OverpaymentInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadOverpaymentHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    return ReadEach("ReadOverpaymentHistory2",
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

        entities.OverpaymentHistory.CpaType = db.GetString(reader, 0);
        entities.OverpaymentHistory.CspNumber = db.GetString(reader, 1);
        entities.OverpaymentHistory.EffectiveDt = db.GetDate(reader, 2);
        entities.OverpaymentHistory.OverpaymentInd = db.GetString(reader, 3);
        entities.OverpaymentHistory.CreatedBy = db.GetString(reader, 4);
        entities.OverpaymentHistory.CreatedTmst = db.GetDateTime(reader, 5);
        entities.OverpaymentHistory.Populated = true;
        CheckValid<OverpaymentHistory>("CpaType",
          entities.OverpaymentHistory.CpaType);
        CheckValid<OverpaymentHistory>("OverpaymentInd",
          entities.OverpaymentHistory.OverpaymentInd);

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
      /// A value of DetOverpaymentHistory.
      /// </summary>
      [JsonPropertyName("detOverpaymentHistory")]
      public OverpaymentHistory DetOverpaymentHistory
      {
        get => detOverpaymentHistory ??= new();
        set => detOverpaymentHistory = value;
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
      public const int Capacity = 200;

      private Common common;
      private OverpaymentHistory detOverpaymentHistory;
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
      /// A value of DetOverpaymentHistory.
      /// </summary>
      [JsonPropertyName("detOverpaymentHistory")]
      public OverpaymentHistory DetOverpaymentHistory
      {
        get => detOverpaymentHistory ??= new();
        set => detOverpaymentHistory = value;
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
      public const int Capacity = 200;

      private Common common;
      private OverpaymentHistory detOverpaymentHistory;
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

    private Standard standard;
    private CsePerson csePerson;
    private CsePerson hiddenPrev;
    private Standard personPrompt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common history;
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
    /// A value of SelectionMade.
    /// </summary>
    [JsonPropertyName("selectionMade")]
    public Common SelectionMade
    {
      get => selectionMade ??= new();
      set => selectionMade = value;
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
    /// A value of EndRead.
    /// </summary>
    [JsonPropertyName("endRead")]
    public Common EndRead
    {
      get => endRead ??= new();
      set => endRead = value;
    }

    private Common selectionMade;
    private NextTranInfo nextTranInfo;
    private DateWorkArea dateWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea blank;
    private Common endRead;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OverpaymentHistory.
    /// </summary>
    [JsonPropertyName("overpaymentHistory")]
    public OverpaymentHistory OverpaymentHistory
    {
      get => overpaymentHistory ??= new();
      set => overpaymentHistory = value;
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

    private OverpaymentHistory overpaymentHistory;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
