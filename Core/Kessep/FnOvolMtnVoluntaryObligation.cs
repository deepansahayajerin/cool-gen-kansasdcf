// Program: FN_OVOL_MTN_VOLUNTARY_OBLIGATION, ID: 372100240, model: 746.
// Short name: SWEOVOLP
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
/// A program: FN_OVOL_MTN_VOLUNTARY_OBLIGATION.
/// </para>
/// <para>
/// This procedure adds, updates, and displays Voluntary Obligations, and 
/// related details. Updates and deletes are only allowed on Obligations with no
/// activity.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOvolMtnVoluntaryObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OVOL_MTN_VOLUNTARY_OBLIGATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOvolMtnVoluntaryObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOvolMtnVoluntaryObligation.
  /// </summary>
  public FnOvolMtnVoluntaryObligation(IContext context, Import import,
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
    // #################################################
    // #################################################
    // ##
    // ##    PLEASE NOTE:
    // ##
    // ##  When putting ESCAPES in the main CASE OF COMMAND
    // ##  constuct, do NOT escape all the way out of the procedure
    // ##  because this will prevent the logic at the bottom of this
    // ##  from being processed - and leave some screen fields
    // ##  unprotected.  Just look at how all the other escapes are
    // ##  placed and do likewise.
    // ##
    // #################################################
    // #################################################
    // *******************************************************************
    // 01/04/97	R. Marchman	Add new security/next tran.
    // 01/13/97	HOOKS		raise events
    // 01/23/97        HOOKS	              ADD LOGIC FOR HIST/MONA
    // 
    // AUTOMATIC NEXTTRAN
    // 07/09/97        Paul R. Egger   Fixes to help desk issues.
    // 09/29/97	A Samuels	Problem Report 26135	
    // 12/01/97	Venkatesh Kamaraj
    // 		PR # 32283 - Added logic to bring back supported person
    //                              from NAME.
    // 		PR # 28255 - Protected Effective date once it has been
    //                              added.
    // 		PR # 28451 - Added logic to delete a supported
    //                              person or discontinue an obligation if the 
    // last
    //                              supported person is selected for delete.
    // 12/29/97	Venkatesh Kamaraj   Changed logic to set situation # to 0
    //                                     
    // because of infrastructure changes
    // 03/26/98	Siraj Konkader	ZDEL cleanup - partial
    // *******************************************************************
    // =================================================
    // 12/22/98 - b adams  -  Program code was being arrived at
    //   incorrectly; was not using the FN_... cab.
    //   User could not add an additional supported person.
    //   Added Obligation identifier to the screen.
    // =================================================
    // =================================================
    // 12/23/98 - B Adams  -  Can add additional supported persons,
    //   but only on the same day the obligation was created.  After
    //   that, the obligation will have to be discontinued and a new
    //   one added.
    //   Cannot delete an obligation after its creation date.
    //   Must Clear before Add.  No reason to copy a voluntary from
    //   an existing one.
    //   Effective date will ALWAYS be the creation date.
    // =================================================
    // 09-21-99, M. Brown, problem report 74567: Allow effective date to be 
    // backdated.
    // Unprotected Effective Date on the screen, and added necessary edits.
    // Also default effective date to current date only if effective date not 
    // entered.
    // **********************************************************************
    // ===================================================================
    // 10/20/99, pr#s 76960, 77622, 77437: Removed case and worker from this 
    // screen.
    // 11/16/99 - PR#s 78975, 78976: % allocation not being adjusted on Add and
    //   Delete functions and should have been.  Incorrect error message.
    // ===================================================================
    // ===================================================================
    // 01/18/00     K. Price    PR 84045 - Allow Discontinue Date to be
    //                          backdated
    // ===================================================================
    // =================================================
    // Oct, 2000 M. Brown, pr# 106234 - NEXT TRAN updates.
    // =================================================
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      // ***--- Must Clear before Add; reset flag  -  12/23/98 - b adams
      export.HiddenPrev.Command = "";

      return;
    }

    // : Set hardcoded values
    UseFnHardcodedDebtDistribution();
    UseFnHardcodeLegal();
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    // : Move all IMPORTs to EXPORTs.
    export.HiddenEffDate.Date = import.HiddenEffDate.Date;
    export.CsePerson.Number = import.CsePerson.Number;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.HiddenDiscontinueDate.Date = import.HiddenDiscontinueDate.Date;
    MoveObligation5(import.Obligation, export.Obligation);
    export.ManuallyDistribute.Flag = import.ManuallyDistribute.Flag;
    export.HiddenPrev.Command = import.HiddenPrev.Command;
    export.ObligationActive.Flag = import.ObligationActive.Flag;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Discontinued.Date = import.Discontinued.Date;
    export.Effective.Date = import.Effective.Date;
    MoveCommon2(import.ObligorPrompt, export.ObligorPrompt);
    export.ObligationType.Assign(import.ObligationType);
    MoveDebtDetail(import.DebtDetail, export.DebtDetail);
    export.ObligationCreated.Date = import.ObligationCreated.Date;
    export.HsupportedPersons.Count = import.HsupportedPersons.Count;
    export.ObCollProtAct.Flag = import.ObCollProtAct.Flag;

    if (IsEmpty(export.ObCollProtAct.Flag))
    {
      export.ObCollProtAct.Flag = "N";
    }

    local.ZeroFill.Text10 = export.CsePerson.Number;
    UseEabPadLeftWithZeros();
    export.CsePerson.Number = local.ZeroFill.Text10;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      MoveCommon2(import.Group.Item.SpNamePrompt,
        export.Group.Update.SpNamePrompt);
      MoveObligationTransaction1(import.Group.Item.ObligationTransaction,
        export.Group.Update.ObligationTransaction);
      export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
      export.Group.Update.SupportedCsePerson.Number =
        import.Group.Item.SupportedCsePerson.Number;
      MoveCsePersonsWorkSet(import.Group.Item.SupportedCsePersonsWorkSet,
        export.Group.Update.SupportedCsePersonsWorkSet);
      export.Group.Update.ServiceProvider.UserId =
        import.Group.Item.ServiceProvider.UserId;
      ++local.Temp.Count;
      local.ZeroFill.Text10 = export.Group.Item.SupportedCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.Group.Update.SupportedCsePerson.Number = local.ZeroFill.Text10;

      if (Equal(global.Command, "PRMPTRET"))
      {
        if (AsChar(import.Group.Item.SpNamePrompt.SelectChar) == 'S')
        {
          export.Group.Update.SpNamePrompt.SelectChar = "";
          export.Group.Update.SupportedCsePerson.Number = import.Passed.Number;
          export.Group.Update.SupportedCsePersonsWorkSet.FormattedName =
            import.Passed.FormattedName;
          local.CseApplied.Flag = "Y";
          global.Command = "BYPASS";
        }
      }

      export.Group.Next();
    }

    if (AsChar(local.CseApplied.Flag) == 'Y')
    {
      return;
    }
    else if (Equal(global.Command, "PRMPTRET"))
    {
      if (AsChar(import.ObligorPrompt.SelectChar) == 'S')
      {
        export.CsePerson.Number = import.Passed.Number;
        export.CsePersonsWorkSet.FormattedName = import.Passed.FormattedName;
        export.ObligorPrompt.SelectChar = "";
      }

      global.Command = "DISPLAY";
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
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
      export.HiddenNextTranInfo.Assign(export.HiddenNextTranInfo);
      export.CsePerson.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
        (10);
      export.Obligation.SystemGeneratedIdentifier =
        export.HiddenNextTranInfo.ObligationId.GetValueOrDefault();

      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        local.FromHistMonaNxttran.Flag = "Y";
      }
      else
      {
        local.FromHistMonaNxttran.Flag = "N";
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "OPAY") || Equal(global.Command, "MDIS") || Equal
      (global.Command, "COLP") || Equal(global.Command, "BYPASS"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        if (Equal(global.Command, "XXFMMENU"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }

        return;
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (export.Obligation.SystemGeneratedIdentifier == 0)
    {
      if (Equal(global.Command, "UPDATE"))
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
      }
      else if (Equal(global.Command, "DELETE"))
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
      }
    }

    // If the key field is blank, certain commands are not allowed.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "NEXT") || Equal(global.Command, "PREV"))
    {
      // The above list of commands must be reviewed if any new commands are 
      // made relevant to this  procedure.
      if (IsEmpty(export.CsePerson.Number))
      {
        ExitState = "KEY_FIELD_IS_BLANK";

        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        local.Temp.Command = global.Command;
        global.Command = "BYPASS";
      }
    }

    // The logic assumes that a record cannot be UPDATEd or DELETEd without 
    // first being displayed.
    // Therefore, a key change with either command is invalid.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY"))
    {
      // The above list of commands must be reviewed if
      // any new commands are made relevant to this
      //  procedure.
      if (!Equal(import.CsePerson.Number, import.HiddenCsePerson.Number) && !
        IsEmpty(import.HiddenCsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        if (Equal(global.Command, "DISPLAY"))
        {
          ExitState = "OACM_PF19_FOR_ANOTHER_OBLIGATION";
        }
        else
        {
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        return;
      }
    }

    // Edit errors were detected.  Set command to BYPASS to skip processing
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Temp.Command = global.Command;
      global.Command = "BYPASS";
    }

    // If obligation is active,  UPDATE not allowed
    // All edits are coded  in CASE UPDATE.
    // Validation common to CREATE and UPDATE. If an error is found, EXIT STATE 
    // should be set.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY"))
    {
      local.CsePersonsWorkSet.Number = export.CsePerson.Number;
      UseSiReadCsePerson1();

      if (!IsEmpty(local.Eab.Type1))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
        local.Temp.Command = global.Command;
        global.Command = "BYPASS";

        goto Test1;
      }

      export.CsePerson.Number = local.CsePersonsWorkSet.Number;
      MoveCsePersonsWorkSet(local.CsePersonsWorkSet, export.CsePersonsWorkSet);

      if (Equal(global.Command, "DISPLAY"))
      {
        goto Test1;
      }

      if (IsEmpty(import.Obligation.Description))
      {
        var field = GetField(export.Obligation, "description");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
      }

      if (Equal(export.Effective.Date, local.Blank.Date))
      {
        export.Effective.Date = local.Current.Date;
      }

      // =================================================
      // 12/23/98 - b adams  -  The 'hidden' discontinue date is used
      //   to keep that field from becoming unprotected on the screen
      //   when it should not be.  If the original disc date was not a
      //   future date, then it cannot be changed and has to remain
      //   protected.
      //   It is SET after a successful display, update, or add.
      // =================================================
      // =================================================
      // 12/23/98 - b adams  -  Test betw. discontinue-date and
      //   effective-date was not correct.
      // =================================================
      // =================================================
      // 0921/99 - mfb, problem report #74567: Effective Date no longer
      // protected, so include it in the edit as a potential field in error.
      // =================================================
      if (!Lt(export.Effective.Date, export.Discontinued.Date) && !
        Equal(export.Discontinued.Date, local.Blank.Date))
      {
        if (!Equal(export.Discontinued.Date, export.HiddenDiscontinueDate.Date))
        {
          var field1 = GetField(export.Effective, "date");

          field1.Error = true;

          var field2 = GetField(export.Discontinued, "date");

          field2.Error = true;
        }
        else
        {
          var field = GetField(export.Effective, "date");

          field.Error = true;
        }

        ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";
        local.Temp.Command = global.Command;
        global.Command = "BYPASS";

        goto Test1;
      }

      // =================================================
      // 01/18/00 - K. Price  -  PR 84045 - allow discontinue date to be back 
      // dated.
      // This is the only place/reason the discontinue date is protected.  So 
      // code will be disabled.
      // =================================================
      // If there are errors on the header portion of the screen,
      // bypass list screen edits
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.Temp.Command = global.Command;
        global.Command = "BYPASS";

        goto Test1;
      }

      // : EDIT LIST PORTION OF SCREEN
      // *** Flag will be used to indicate that at least one
      // supported person has been entered ***
      local.Child.Flag = "N";

      // Check for duplicate Supported Persons
      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (IsEmpty(import.Group.Item.SupportedCsePerson.Number))
        {
          continue;
        }

        local.DuplicateSpCheck.Flag = "N";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          local.ZeroFill.Text10 = import.Group.Item.SupportedCsePerson.Number;
          UseEabPadLeftWithZeros();

          if (Equal(local.ZeroFill.Text10,
            export.Group.Item.SupportedCsePerson.Number))
          {
            if (AsChar(local.DuplicateSpCheck.Flag) == 'Y')
            {
              ExitState = "DUPLICATE_SUPPORTED_PERSONS";

              var field =
                GetField(export.Group.Item.SupportedCsePerson, "number");

              field.Error = true;
            }
            else
            {
              local.DuplicateSpCheck.Flag = "Y";
            }
          }

          // ---------------------------------------------------------------------------------------
          // RBM  05/19/1997  The OBLIGOR can not be the Supported Person in his
          // own Obligations
          // ---------------------------------------------------------------------------------------
          if (Equal(export.CsePerson.Number,
            export.Group.Item.SupportedCsePerson.Number))
          {
            ExitState = "FN0000_OBLGR_CAN_NOT_BE_SUP_PER";

            var field =
              GetField(export.Group.Item.SupportedCsePerson, "number");

            field.Error = true;
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test1;
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        // Validate action character
        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            if (IsEmpty(export.Group.Item.SupportedCsePerson.Number))
            {
              if (Equal(global.Command, "UPDATE"))
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Color = "red";
                field1.Highlighting = Highlighting.ReverseVideo;
                field1.Protected = false;
                field1.Focused = true;

                ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";

                continue;
              }
            }

            break;
          case ' ':
            if (Equal(global.Command, "UPDATE"))
            {
              continue;
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            goto Test1;
        }

        // *** Supported person number is mandatory and must exist ***
        if (IsEmpty(export.Group.Item.SupportedCsePerson.Number))
        {
          var field = GetField(export.Group.Item.SupportedCsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
        else
        {
          local.Child.Flag = "Y";
          local.CsePersonsWorkSet.Number =
            export.Group.Item.SupportedCsePerson.Number;

          if (ReadCsePersonSupported())
          {
            local.CsePersonsWorkSet.Number = entities.SupportedCsePerson.Number;
          }
          else if (ReadCsePerson())
          {
            if (ReadSupported())
            {
              // **  OK  **
            }
            else
            {
              try
              {
                CreateCsePersonAccount();

                // **  OK  **
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    var field1 =
                      GetField(export.Group.Item.SupportedCsePerson, "number");

                    field1.Error = true;

                    ExitState = "FN0000_CSE_PERSON_ACCOUNT_AE";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    var field2 =
                      GetField(export.Group.Item.SupportedCsePerson, "number");

                    field2.Error = true;

                    ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

                    break;
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
            var field =
              GetField(export.Group.Item.SupportedCsePerson, "number");

            field.Error = true;

            ExitState = "CSE_PERSON_NF";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (!ReadCaseUnit())
          {
            var field =
              GetField(export.Group.Item.SupportedCsePerson, "number");

            field.Error = true;

            ExitState = "INVALID_CASE_ROLE";

            return;
          }

          UseSiReadCsePerson3();

          // 10/20/99, pr#s 76960, 77622, 77437: Removing case and worker
          // from screens, so commenting out this logic.
        }

        // If edit errors found on current line, exit FOR EACH loop
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test1;
        }
      }

      // For add function, at least one Supported Person must have been entered
      if (AsChar(local.Child.Flag) == 'N' && Equal(global.Command, "ADD"))
      {
        ExitState = "OACM_AT_LEAST_ONE_CHILD_IS_REQD";

        // **--- escape
      }
    }

Test1:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      global.Command = "BYPASS";
      local.Temp.Command = global.Command;
    }

    // ** Mainline **
    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "LIST":
        local.Select.Count = 0;

        switch(AsChar(export.ObligorPrompt.SelectChar))
        {
          case 'S':
            ++local.Select.Count;
            ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";

            break;
          case '+':
            break;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.ObligorPrompt, "selectChar");

            field.Error = true;

            goto Test2;
        }

        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.Common.SelectChar =
            import.Group.Item.Common.SelectChar;
          MoveCommon2(import.Group.Item.SpNamePrompt,
            export.Group.Update.SpNamePrompt);
          MoveObligationTransaction1(import.Group.Item.ObligationTransaction,
            export.Group.Update.ObligationTransaction);
          export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
          export.Group.Update.SupportedCsePerson.Number =
            import.Group.Item.SupportedCsePerson.Number;
          MoveCsePersonsWorkSet(import.Group.Item.SupportedCsePersonsWorkSet,
            export.Group.Update.SupportedCsePersonsWorkSet);
          export.Group.Update.ServiceProvider.UserId =
            import.Group.Item.ServiceProvider.UserId;

          switch(AsChar(import.Group.Item.SpNamePrompt.SelectChar))
          {
            case 'S':
              ++local.Select.Count;

              if (local.Select.Count > 1)
              {
                var field1 =
                  GetField(export.Group.Item.SpNamePrompt, "selectChar");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
              }

              break;
            case '+':
              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              var field =
                GetField(export.Group.Item.SpNamePrompt, "selectChar");

              field.Color = "red";
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = false;
              field.Focused = true;

              break;
          }

          export.Group.Next();
        }

        if (local.Select.Count == 0 && IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";

          return;
        }

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

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        local.CsePersonsWorkSet.Number = export.CsePerson.Number;
        UseSiReadCsePerson2();
        export.CsePerson.Number = local.CsePersonsWorkSet.Number;
        export.CsePersonsWorkSet.FormattedName =
          local.CsePersonsWorkSet.FormattedName;
        export.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

        if (Equal(export.CsePerson.Number, export.CsePersonsWorkSet.Number) && Equal
          (export.CsePersonsWorkSet.Number, export.HiddenCsePerson.Number))
        {
          // Do Nothing
        }
        else
        {
          export.Obligation.SystemGeneratedIdentifier = 0;
        }

        if (AsChar(local.FromHistMonaNxttran.Flag) == 'Y')
        {
          UseFnGetOblFromHistMonaNxtran();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        // *** Read voluntary obligation ***
        UseFnReadVoluntaryObligation();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Effective.Date = export.DebtDetail.CoveredPrdStartDt;
          export.Discontinued.Date = export.DebtDetail.CoveredPrdEndDt;

          if (Equal(export.Discontinued.Date, local.Maximum.Date))
          {
            export.Discontinued.Date = local.Blank.Date;
          }

          if (!IsEmpty(export.CsePerson.Number))
          {
            export.CsePersonsWorkSet.Number = export.CsePerson.Number;
            UseSiReadCsePerson4();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }

          // ***---  PR# 78975: keep count of existing supported persons
          export.HsupportedPersons.Count = local.Temp.Count;
          export.ObligationCreated.Date = Date(export.Obligation.CreatedTmst);
          export.HiddenDiscontinueDate.Date = export.Discontinued.Date;
          export.HiddenPrev.Command = "NO ADD";
          export.HiddenEffDate.Date = export.Effective.Date;

          // =================================================
          // 09/21/99 - Maureen Brown  - Effective date is only
          // unprotected for add command. (part of PR 74567)
          // =================================================
          if (Lt(export.ObligationCreated.Date, local.Current.Date))
          {
            var field = GetField(export.Effective, "date");

            field.Color = "cyan";
            field.Highlighting = Highlighting.Normal;
            field.Protected = true;
          }

          if (ReadObligCollProtectionHist())
          {
            export.ObCollProtAct.Flag = "Y";
          }
          else
          {
            export.ObCollProtAct.Flag = "N";
          }

          ExitState = "ACO_NI0000_DISPLAY_OK";
        }
        else
        {
          if (IsEmpty(export.CsePersonsWorkSet.FormattedName) && !
            IsEmpty(local.CsePersonsWorkSet.FormattedName))
          {
            export.CsePersonsWorkSet.FormattedName =
              local.CsePersonsWorkSet.FormattedName;
          }
        }

        break;
      case "ADD":
        // 09-21-99, M. Brown, problem report 74567: Removed check to see if a
        // voluntary obligation already exists for the AP.  This has been 
        // replaced
        // by an edit to not allow the voluntary obligation add if an obligation
        // exists for the same ap and child, with an overlapping timeframe.
        if (Equal(export.HiddenPrev.Command, "NO ADD"))
        {
          // ***--- Must CLEAR before Add - 12/23/98 - b adams
          ExitState = "FN0000_CLEAR_BEFORE_ADD_2";

          break;
        }
        else
        {
          // ***--- Effective date defaults to current-date  -  11/24/98  -  b 
          // adams
          // 09-21-99, M. Brown, problem report 74567: Allow effective date to 
          // be backdated.
          if (Equal(export.Effective.Date, local.Blank.Date))
          {
            export.Effective.Date = local.Current.Date;
          }

          // *** Check to see if the obligation being added is potentially a 
          // duplicate. ***
          // =================================================
          // Oct 14, 1999, pr# 74567, mbrown - Edit to prevent create of 
          // overlapping obligations.
          // ---
          // 11/17/99 - b adams - restructured read to avoid accessing base 
          // tables
          //   using SOME/THAT
          // =================================================
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            foreach(var item in ReadDebtDetail())
            {
              // =================================================
              // PR# 78974: 11/19/99 - b adams  -  IF did not account for
              //   discontinue date being 'null' (e.g. max)
              // =================================================
              if (!Lt(export.Effective.Date,
                entities.DebtDetail.CoveredPrdStartDt) && Lt
                (export.Effective.Date, entities.DebtDetail.CoveredPrdEndDt) ||
                Lt
                (export.Effective.Date, entities.DebtDetail.CoveredPrdStartDt) &&
                (Equal(export.Discontinued.Date, local.Blank.Date) || Lt
                (entities.DebtDetail.CoveredPrdStartDt, export.Discontinued.Date)))
                
              {
                ExitState = "ACO_NE0000_DATE_OVERLAP";

                var field1 = GetField(export.Discontinued, "date");

                field1.Color = "yellow";
                field1.Highlighting = Highlighting.ReverseVideo;
                field1.Protected = false;

                var field2 = GetField(export.Effective, "date");

                field2.Color = "yellow";
                field2.Highlighting = Highlighting.ReverseVideo;
                field2.Protected = false;

                var field3 = GetField(export.Group.Item.Common, "selectChar");

                field3.Error = true;

                global.Command = "BYPASS";

                goto Test2;
              }
            }
          }
        }

        // *** Default value for order_type_code (K=Kansas) ***
        export.Obligation.OrderTypeCode = "K";
        export.DebtDetail.CoveredPrdEndDt = export.Discontinued.Date;
        export.DebtDetail.CoveredPrdStartDt = export.Effective.Date;
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.ReferenceDate = local.Current.Date;
        export.Obligation.OtherStateAbbr = "KS";
        UseFnProcessVoluntaryObligAdd();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        ExitState = "ACO_NI0000_CREATE_OK";
        export.ObligationCreated.Date = local.Current.Date;
        export.HiddenDiscontinueDate.Date = export.Discontinued.Date;
        export.HiddenEffDate.Date = export.Effective.Date;
        export.HiddenPrev.Command = "NO ADD";
        export.HsupportedPersons.Count = local.Temp.Count;

        break;
      case "UPDATE":
        export.DebtDetail.CoveredPrdStartDt = export.Effective.Date;

        if (Equal(export.Discontinued.Date, local.Blank.Date))
        {
          export.DebtDetail.CoveredPrdEndDt = local.Maximum.Date;
        }
        else
        {
          export.DebtDetail.CoveredPrdEndDt = export.Discontinued.Date;
        }

        if (AsChar(local.Child.Flag) == 'Y' || !
          Equal(export.Effective.Date, export.HiddenEffDate.Date))
        {
          if (Lt(export.ObligationCreated.Date, local.Current.Date) && !
            Equal(export.ObligationCreated.Date, local.Blank.Date))
          {
            ExitState = "FN0000_CANT_UPD_AFTER_CREATE_DAT";

            break;
          }

          if (AsChar(export.ObligationActive.Flag) == 'Y')
          {
            ExitState = "FN0000_OBLIG_ACT_UPD_NOT_ALLOWED";

            break;
          }
        }

        // Oct 26, 1999, pr#74567, M. Brown: If a supported person is being 
        // added, check to see if that person is already on another obligation
        // with an overlapping timeframe.
        if (AsChar(local.Child.Flag) == 'Y' || !
          Equal(export.Discontinued.Date, export.HiddenDiscontinueDate.Date) ||
          !Equal(export.Effective.Date, export.HiddenEffDate.Date))
        {
          // =================================================
          // Oct 14, 1999, pr# 74567, mbrown - Edit to prevent create of 
          // overlapping obligations.
          // ---
          // 11/17/99 - b adams - restructured read to avoid accessing base 
          // tables
          //   using SOME/THAT
          // =================================================
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            foreach(var item in ReadDebtDetailObligation())
            {
              // =================================================
              // PR# 78974: 11/19/99 - b adams  -  IF did not account for
              //   discontinue date being 'null' (e.g. max)
              // =================================================
              if (!Lt(export.Effective.Date,
                entities.DebtDetail.CoveredPrdStartDt) && Lt
                (export.Effective.Date, entities.DebtDetail.CoveredPrdEndDt) ||
                !
                Lt(entities.DebtDetail.CoveredPrdStartDt, export.Effective.Date) &&
                (Equal(export.Discontinued.Date, local.Blank.Date) || Lt
                (entities.DebtDetail.CoveredPrdStartDt, export.Discontinued.Date)))
                
              {
                // =================================================
                // PR# 123055 : 07/02/2001 -  Vithal Madhira -  During 
                // conversion, some obligations are created with same '
                // covered_prd_start_dt'. This edit will let the USER to end_dt
                // some of the obligations created with overlapping timeframes
                // during conversion..
                // =================================================
                if (Equal(entities.Obligation.CreatedBy, "CONVERSN"))
                {
                }
                else
                {
                  ExitState = "ACO_NE0000_DATE_OVERLAP";

                  var field1 = GetField(export.Discontinued, "date");

                  field1.Color = "yellow";
                  field1.Highlighting = Highlighting.ReverseVideo;
                  field1.Protected = false;

                  var field2 = GetField(export.Effective, "date");

                  field2.Color = "yellow";
                  field2.Highlighting = Highlighting.ReverseVideo;
                  field2.Protected = false;

                  var field3 = GetField(export.Group.Item.Common, "selectChar");

                  field3.Error = true;

                  global.Command = "BYPASS";

                  goto Test2;
                }
              }
            }
          }
        }

        // =================================================
        // PR# 78975: 11/16/99 - bud adams  -  When Update function
        //   adds or deletes supported persons, the allocation percent
        //   for each SP was not recalculated and should have been.
        // =================================================
        if (local.Temp.Count != export.HsupportedPersons.Count)
        {
          local.NumberOfSupportedPersns.Count = local.Temp.Count;
          local.NumberOfSupportedPersns.Percentage = 100 / local
            .NumberOfSupportedPersns.Count;
        }

        UseFnUpdateVoluntaryObligation1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsEmpty(export.Group.Item.Common.SelectChar))
          {
            continue;
          }

          local.HardcodeVoluntary.Classification =
            local.HardcodeOtCVoluntaryClassifi.Classification;

          // =================================================
          // PR# 78975: 11/16/99 - b adams  -  allocation percent was not
          //   being changed if the number of supported persons had
          //   changed and it should be.
          //   These values are exported from fn-update-voluntary-oblig.
          // =================================================
          if (local.PercentageAllocated.Count > 0)
          {
            local.PercentageAllocated.Percentage =
              local.NumberOfSupportedPersns.Percentage + 1;
            --local.PercentageAllocated.Count;
          }
          else
          {
            local.PercentageAllocated.Percentage =
              local.NumberOfSupportedPersns.Percentage;
          }

          UseFnCreateObligationTransaction();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test2;
          }

          export.Group.Update.Common.SelectChar = "";
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        export.HsupportedPersons.Count = local.Temp.Count;
        export.HiddenDiscontinueDate.Date = export.Discontinued.Date;
        export.HiddenEffDate.Date = export.Effective.Date;
        export.HiddenPrev.Command = "NO ADD";

        break;
      case "BYPASS":
        // If an error was found before the CASE OF
        // COMMAND, the command was set to BYPASS
        // so that the subsequent logic was not
        // performed.
        // This allows the addition of common actions
        // (for example, protection logic) that must
        // occur at the end of every pass through
        // the logic.
        // The command is reset here in case it is
        // displayed on the screen.
        global.Command = local.Temp.Command;

        break;
      case "PROMPT":
        if (AsChar(import.ObligorPrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";
        }
        else
        {
          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (AsChar(import.Group.Item.SpNamePrompt.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
            }
          }
        }

        break;
      case "MDIS":
        export.CsePersonsWorkSet.Number = export.CsePerson.Number;

        if (ReadObligationType())
        {
          export.ObligationType.SystemGeneratedIdentifier =
            entities.ObligationType.SystemGeneratedIdentifier;
          export.ObligationType.Code = entities.ObligationType.Code;
        }
        else
        {
          ExitState = "OBLIGATION_TYPE_NF";

          break;
        }

        ExitState = "ECO_LNK_TO_MTN_MANUAL_DIST_INST";

        break;
      case "COLP":
        export.CsePersonsWorkSet.Number = export.CsePerson.Number;

        if (ReadObligationType())
        {
          export.ObligationType.SystemGeneratedIdentifier =
            entities.ObligationType.SystemGeneratedIdentifier;
          export.ObligationType.Code = entities.ObligationType.Code;
        }
        else
        {
          ExitState = "OBLIGATION_TYPE_NF";

          break;
        }

        ExitState = "ECO_LNK_TO_COLP";

        break;
      case "OPAY":
        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        ExitState = "ECO_LNK_LST_OBLIG_BY_AP_PAYOR";

        break;
      case "DELETE":
        // Oct 14, 1999, pr# 76806, mbrown - Changed so that if a supported
        // person is not selected, the entire obligation is deleted.
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          switch(AsChar(export.Group.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              if (IsEmpty(export.CsePerson.Number))
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Color = "red";
                field1.Highlighting = Highlighting.ReverseVideo;
                field1.Protected = false;
                field1.Focused = true;

                ExitState = "CO0000_SELECT_ON_BLANK_DETAIL";

                continue;
              }

              ++local.SelCount.Count;

              break;
            default:
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
              }

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        export.HiddenDiscontinueDate.Date = export.Discontinued.Date;

        if (Lt(export.ObligationCreated.Date, local.Current.Date))
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "FN0000_CANT_DELETE_SEL_PERSON";
            }
          }

          if (!IsExitState("FN0000_CANT_DELETE_SEL_PERSON"))
          {
            ExitState = "FN0000_CANT_DEL_AFTER_CREAT_DATE";
          }

          break;
        }

        // Oct 14, 1999, pr# 76806, mbrown: If no supported person was selected,
        // we are
        // deleting the entire obligation.
        if (local.SelCount.Count == 0)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            export.Group.Update.Common.SelectChar = "S";
          }
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            UseFnDeleteSuppPersonForVolOb();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else if (IsExitState("FN0000_DELETE_SUCCESSFUL"))
            {
              // : The last supported person was deleted, so the obligation was 
              // deleted also.
              export.ObligationCreated.Date = local.Blank.Date;
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else if (IsExitState("FN0000_SUPP_PERSON_HAS_COLL"))
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "FN0000_OBLIG_ACT_DEL_NOT_ALLOWED";

              goto Test2;
            }
            else
            {
              ExitState = "FN0000_DELETE_UNSUCCESSFUL_RB";

              goto Test2;
            }
          }
        }

        // =================================================
        // PR# 78975: 11/16/99 - bud adams  -  When Delete function
        //   deletes supported persons, the allocation percent for each
        //   SP was not recalculated and should have been.  This will
        //   recalc those values and update the ob-tran records.
        // =================================================
        if (local.SelCount.Count < local.Temp.Count && local.SelCount.Count > 0)
        {
          local.NumberOfSupportedPersns.Count = local.Temp.Count - local
            .SelCount.Count;
          local.NumberOfSupportedPersns.Percentage = 100 / local
            .NumberOfSupportedPersns.Count;
          UseFnUpdateVoluntaryObligation2();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenPrev.Command = "NO ADD";
          ExitState = "DELETE_SUCESSFUL_REDISPLAY";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test2:

    // : This is common logic that must occur at the end of every pass.
    if (Equal(export.Discontinued.Date, local.Blank.Date))
    {
      var field = GetField(export.CsePersonsWorkSet, "formattedName");

      field.Color = "cyan";
      field.Protected = true;
    }

    // =================================================
    // 01/18/00 - K. Price  -  PR 84045 - allow discontinue date to be back 
    // dated.
    // This is the only place/reason the discontinue date is protected.  So code
    // will be disabled.
    // =================================================
    // =================================================
    // 3/31/99 - b adams  -  Discontinue date must be updatable
    // =================================================
    if (Equal(global.Command, "DISPLAY") && (
      Lt(local.Current.Date, export.Discontinued.Date) || Equal
      (export.Discontinued.Date, local.Blank.Date)))
    {
      var field = GetField(export.Discontinued, "date");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
    }

    // =================================================
    // 12/23/98 - B Adams  -  Protect supported person fields when
    //   the Obligation was created before today.
    // =================================================
    if (AsChar(export.ObligationActive.Flag) == 'Y' || Lt
      (export.ObligationCreated.Date, local.Current.Date) && !
      Equal(export.ObligationCreated.Date, local.Blank.Date))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!IsEmpty(export.Group.Item.SupportedCsePerson.Number))
        {
          var field1 = GetField(export.Group.Item.SupportedCsePerson, "number");

          field1.Color = "cyan";
          field1.Highlighting = Highlighting.Normal;
          field1.Protected = true;

          var field2 = GetField(export.Group.Item.SpNamePrompt, "selectChar");

          field2.Color = "cyan";
          field2.Protected = true;

          export.Group.Update.SpNamePrompt.SelectChar = "";
        }
      }
    }

    // =================================================
    // 09/21/99, mfb, pr#74567 - Protect effective date if
    // any command other than add, or hidden effective date is
    // not blank (which means first time into the screen).
    // =================================================
    if (!Equal(export.HiddenEffDate.Date, local.Blank.Date) && !
      Equal(global.Command, "ADD") && Lt
      (export.ObligationCreated.Date, local.Current.Date))
    {
      var field = GetField(export.Effective, "date");

      field.Color = "cyan";
      field.Protected = true;
    }
  }

  private static void MoveCommon1(Common source, Common target)
  {
    target.Count = source.Count;
    target.Percentage = source.Percentage;
  }

  private static void MoveCommon2(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveGroup1(Export.GroupGroup source,
    FnProcessVoluntaryObligAdd.Import.GroupGroup target)
  {
    MoveCommon2(source.SpNamePrompt, target.Prompt);
    target.Common.SelectChar = source.Common.SelectChar;
    target.SupportedCsePerson.Number = source.SupportedCsePerson.Number;
    MoveCsePersonsWorkSet(source.SupportedCsePersonsWorkSet,
      target.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Case1.Number;
    MoveObligationTransaction1(source.ObligationTransaction,
      target.ObligationTransaction);
    target.ServiceProvider.UserId = source.ServiceProvider.UserId;
  }

  private static void MoveGroup2(FnReadVoluntaryObligation.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    MoveCommon2(source.Prompt, target.SpNamePrompt);
    target.Common.SelectChar = source.Common.SelectChar;
    target.SupportedCsePerson.Number = source.SupportedCsePerson.Number;
    MoveCsePersonsWorkSet(source.SupportedCsePersonsWorkSet,
      target.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Case1.Number;
    MoveObligationTransaction1(source.ObligationTransaction,
      target.ObligationTransaction);
    target.ServiceProvider.UserId = source.ServiceProvider.UserId;
  }

  private static void MoveGroup3(FnProcessVoluntaryObligAdd.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    MoveCommon2(source.Prompt, target.SpNamePrompt);
    target.Common.SelectChar = source.Common.SelectChar;
    target.SupportedCsePerson.Number = source.SupportedCsePerson.Number;
    MoveCsePersonsWorkSet(source.SupportedCsePersonsWorkSet,
      target.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Case1.Number;
    MoveObligationTransaction1(source.ObligationTransaction,
      target.ObligationTransaction);
    target.ServiceProvider.UserId = source.ServiceProvider.UserId;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ReferenceDate = source.ReferenceDate;
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
    target.InfrastructureId = source.InfrastructureId;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
  }

  private static void MoveObligation1(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.Description = source.Description;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation2(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private static void MoveObligation3(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation4(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
  }

  private static void MoveObligation5(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.OrderTypeCode = source.OrderTypeCode;
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
    target.Classification = source.Classification;
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

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCreateObligationTransaction()
  {
    var useImport = new FnCreateObligationTransaction.Import();
    var useExport = new FnCreateObligationTransaction.Export();

    useImport.HardcodeObligorLap.AccountType =
      local.HardcodedObligorLap.AccountType;
    MoveObligationTransaction2(local.HardcodeOtrnDtAccrual,
      useImport.HcOtrnDtAccrual);
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodeOtCRecoverClassifica.Classification;
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    MoveDateWorkArea(local.Current, useImport.Current);
    MoveObligationType(local.HardcodeVoluntary, useImport.ObligationType);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebtType.Type1;
    MoveObligationTransaction2(local.HardcodeOtVoluntary, useImport.Hardcoded);
    MoveObligationTransaction2(local.HardcodeOtVoluntary,
      useImport.HcOtrnDtVoluntary);
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.Max.Date = local.Maximum.Date;
    useImport.HcCpaSupportedPerson.Type1 = local.HardcodeSupported.Type1;
    MoveDebtDetail(export.DebtDetail, useImport.DebtDetail);
    useImport.Obligor.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.Supported.Number = export.Group.Item.SupportedCsePerson.Number;
    useImport.ObligationTransaction.Amount =
      export.Group.Item.ObligationTransaction.Amount;
    useImport.NumberOfSupportedPrsns.Percentage =
      local.PercentageAllocated.Percentage;

    Call(FnCreateObligationTransaction.Execute, useImport, useExport);

    export.Group.Update.ObligationTransaction.SystemGeneratedIdentifier =
      useExport.ObligationTransaction.SystemGeneratedIdentifier;
  }

  private void UseFnDeleteSuppPersonForVolOb()
  {
    var useImport = new FnDeleteSuppPersonForVolOb.Import();
    var useExport = new FnDeleteSuppPersonForVolOb.Export();

    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HcOtVoluntary.SystemGeneratedIdentifier =
      local.HardcodeVoluntary.SystemGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebtType.Type1;
    MoveObligationTransaction2(local.HardcodeOtVoluntary,
      useImport.HcOtrnTVoluntary);
    useImport.Obligor.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.Supported.Number = export.Group.Item.SupportedCsePerson.Number;

    Call(FnDeleteSuppPersonForVolOb.Execute, useImport, useExport);
  }

  private void UseFnGetOblFromHistMonaNxtran()
  {
    var useImport = new FnGetOblFromHistMonaNxtran.Import();
    var useExport = new FnGetOblFromHistMonaNxtran.Export();

    useImport.NextTranInfo.InfrastructureId =
      export.HiddenNextTranInfo.InfrastructureId;

    Call(FnGetOblFromHistMonaNxtran.Execute, useImport, useExport);

    export.Obligation.SystemGeneratedIdentifier =
      useExport.Obligation.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodeLegal()
  {
    var useImport = new FnHardcodeLegal.Import();
    var useExport = new FnHardcodeLegal.Export();

    Call(FnHardcodeLegal.Execute, useImport, useExport);

    local.HardcodedObligorLap.AccountType = useExport.Obligor.AccountType;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    MoveObligationTransaction2(useExport.OtrnDtAccrualInstructions,
      local.HardcodeOtrnDtAccrual);
    local.HardcodeDdshActiveStatus.Code = useExport.DdshActiveStatus.Code;
    local.HardcodeOtCFeesClassificatio.Classification =
      useExport.OtCFeesClassification.Classification;
    local.HardcodeOtCRecoverClassifica.Classification =
      useExport.OtCRecoverClassification.Classification;
    local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.HardcodeOtCVoluntaryClassifi.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier =
      useExport.OtrrConcurrentObligation.SystemGeneratedIdentifier;
    local.HardcodeVoluntary.SystemGeneratedIdentifier =
      useExport.OtVoluntary.SystemGeneratedIdentifier;
    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeDebtType.Type1 = useExport.OtrnTDebt.Type1;
    MoveObligationTransaction2(useExport.OtrnDtVoluntary,
      local.HardcodeOtVoluntary);
    MoveObligationTransaction2(useExport.OtrnDtDebtDetail,
      local.HardcodeOtrnDtDebtDetail);
    local.HardcodeSupported.Type1 = useExport.CpaSupportedPerson.Type1;
    local.HardcodeOtCAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
  }

  private void UseFnProcessVoluntaryObligAdd()
  {
    var useImport = new FnProcessVoluntaryObligAdd.Import();
    var useExport = new FnProcessVoluntaryObligAdd.Export();

    useImport.Obligor.Number = export.CsePerson.Number;
    useImport.HardcodedObligorLap.AccountType =
      local.HardcodedObligorLap.AccountType;
    useImport.Max.Date = local.Maximum.Date;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodeOtCRecoverClassifica.Classification;
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    useImport.HcCpaSupportedPerson.Type1 = local.HardcodeSupported.Type1;
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    MoveObligationTransaction2(local.HardcodeOtrnDtAccrual,
      useImport.HcOtrnDtAccrual);
    MoveObligationTransaction2(local.HardcodeOtVoluntary,
      useImport.HcOtrnDtVoluntary);
    useImport.HcOtVoluntary.SystemGeneratedIdentifier =
      local.HardcodeVoluntary.SystemGeneratedIdentifier;
    useImport.HcOtrnDebtType.Type1 = local.HardcodeDebtType.Type1;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    MoveDateWorkArea(local.Current, useImport.Current);
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.Effective.Date = export.Effective.Date;
    MoveDebtDetail(export.DebtDetail, useImport.DebtDetail);
    MoveObligation1(export.Obligation, useImport.Obligation);
    useImport.Discontinue.Date = local.Maximum.Date;
    export.Group.CopyTo(useImport.Group, MoveGroup1);

    Call(FnProcessVoluntaryObligAdd.Execute, useImport, useExport);

    MoveObligation4(useExport.Obligation, export.Obligation);
    useExport.Group.CopyTo(export.Group, MoveGroup3);
  }

  private void UseFnReadVoluntaryObligation()
  {
    var useImport = new FnReadVoluntaryObligation.Import();
    var useExport = new FnReadVoluntaryObligation.Export();

    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.Obligor.Number = export.CsePerson.Number;
    MoveObligationTransaction2(local.HardcodeOtVoluntary,
      useImport.HcOtrnVoluntary);
    useImport.HcOtrnDebtType.Type1 = local.HardcodeDebtType.Type1;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.Current.Date = local.Current.Date;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.HcOtCAccruing.Classification =
      local.HardcodeOtCAccruing.Classification;
    useImport.HcOtCVoluntary.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    useImport.HcVolSysGenId.SystemGeneratedIdentifier =
      local.HardcodeVoluntary.SystemGeneratedIdentifier;

    Call(FnReadVoluntaryObligation.Execute, useImport, useExport);

    MoveObligation3(useExport.Obligation, export.Obligation);
    MoveDebtDetail(useExport.DebtDetail, export.DebtDetail);
    export.Discontinued.Date = useExport.Disc.Date;
    export.Effective.Date = useExport.Effective.Date;
    MoveCsePersonsWorkSet(useExport.Obligor, export.CsePersonsWorkSet);
    export.ManuallyDistribute.Flag = useExport.ManualDistributionInd.Flag;
    export.ObligationActive.Flag = useExport.ObligationInEffectInd.Flag;
    useExport.Group.CopyTo(export.Group, MoveGroup2);
  }

  private void UseFnUpdateVoluntaryObligation1()
  {
    var useImport = new FnUpdateVoluntaryObligation.Import();
    var useExport = new FnUpdateVoluntaryObligation.Export();

    useImport.HardcodedObligorLap.AccountType =
      local.HardcodedObligorLap.AccountType;
    useImport.Max.Date = local.Maximum.Date;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodeOtCRecoverClassifica.Classification;
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    useImport.HcCpaSupportedPerson.Type1 = local.HardcodeSupported.Type1;
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    MoveObligationTransaction2(local.HardcodeOtrnDtAccrual,
      useImport.HcOtrnDtAccrual);
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebtType.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.HcOtrnDtDebtDetail.Type1 = local.HardcodeOtrnDtDebtDetail.Type1;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    MoveObligationTransaction2(local.HardcodeOtVoluntary,
      useImport.HcOtrnDtVoluntary);
    useImport.HcOtVoluntary.SystemGeneratedIdentifier =
      local.HardcodeVoluntary.SystemGeneratedIdentifier;
    useImport.Current.Timestamp = local.Current.Timestamp;
    MoveDebtDetail(export.DebtDetail, useImport.DebtDetail);
    MoveObligation2(import.Obligation, useImport.Obligation);
    useImport.ActiveObligation.Flag = import.ObligationActive.Flag;
    useImport.NumberOfSupportedPrsns.Count =
      local.NumberOfSupportedPersns.Count;

    Call(FnUpdateVoluntaryObligation.Execute, useImport, useExport);

    export.ObligationActive.Flag = useExport.ActiveObligation.Flag;
    MoveCommon1(useExport.PercentageAllocated, local.PercentageAllocated);
    local.NumberOfSupportedPersns.Percentage =
      useExport.NumberOfSupportedPrsns.Percentage;
  }

  private void UseFnUpdateVoluntaryObligation2()
  {
    var useImport = new FnUpdateVoluntaryObligation.Import();
    var useExport = new FnUpdateVoluntaryObligation.Export();

    useImport.ActiveObligation.Flag = import.ObligationActive.Flag;
    useImport.NumberOfSupportedPrsns.Count =
      local.NumberOfSupportedPersns.Count;
    useImport.HardcodedObligorLap.AccountType =
      local.HardcodedObligorLap.AccountType;
    MoveObligationTransaction2(local.HardcodeOtrnDtAccrual,
      useImport.HcOtrnDtAccrual);
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodeOtCRecoverClassifica.Classification;
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.HcOtVoluntary.SystemGeneratedIdentifier =
      local.HardcodeVoluntary.SystemGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebtType.Type1;
    MoveObligationTransaction2(local.HardcodeOtVoluntary,
      useImport.HcOtrnDtVoluntary);
    useImport.HcOtrnDtDebtDetail.Type1 = local.HardcodeOtrnDtDebtDetail.Type1;
    useImport.Max.Date = local.Maximum.Date;
    useImport.HcCpaSupportedPerson.Type1 = local.HardcodeSupported.Type1;
    MoveDebtDetail(export.DebtDetail, useImport.DebtDetail);
    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveObligation2(export.Obligation, useImport.Obligation);

    Call(FnUpdateVoluntaryObligation.Execute, useImport, useExport);

    MoveCommon1(useExport.PercentageAllocated, local.PercentageAllocated);
    local.NumberOfSupportedPersns.Percentage =
      useExport.NumberOfSupportedPrsns.Percentage;
    export.ObligationActive.Flag = useExport.ActiveObligation.Flag;
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
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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
    useImport.Case1.Number = import.Group.Item.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.Group.Update.SupportedCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson4()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private void CreateCsePersonAccount()
  {
    var cspNumber = entities.SupportedCsePerson.Number;
    var type1 = local.HardcodeSupported.Type1;
    var createdBy = global.UserId;
    var createdTmst = local.Current.Timestamp;

    CheckValid<CsePersonAccount>("Type1", type1);
    entities.SupportedCsePersonAccount.Populated = false;
    Update("CreateCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDate(command, "recompBalFromDt", default(DateTime));
        db.SetNullableDecimal(command, "stdTotGiftColl", 0M);
        db.SetNullableString(command, "triggerType", "");
      });

    entities.SupportedCsePersonAccount.CspNumber = cspNumber;
    entities.SupportedCsePersonAccount.Type1 = type1;
    entities.SupportedCsePersonAccount.CreatedBy = createdBy;
    entities.SupportedCsePersonAccount.CreatedTmst = createdTmst;
    entities.SupportedCsePersonAccount.Populated = true;
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", export.CsePerson.Number);
        db.SetNullableString(
          command, "cspNoChild", entities.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.Group.Item.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonSupported()
  {
    entities.Supported.Populated = false;
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePersonSupported",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", export.Group.Item.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.Populated = true;
        entities.SupportedCsePerson.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          local.HardcodeVoluntary.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.SetNullableString(
          command, "cspSupNumber", export.Group.Item.SupportedCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 6);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.CreatedBy = db.GetString(reader, 8);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailObligation()
  {
    entities.DebtDetail.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadDebtDetailObligation",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspSupNumber", export.Group.Item.SupportedCsePerson.Number);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          local.HardcodeVoluntary.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 6);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 7);
        entities.DebtDetail.CreatedBy = db.GetString(reader, 8);
        entities.Obligation.CpaType = db.GetString(reader, 9);
        entities.Obligation.CspNumber = db.GetString(reader, 10);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 12);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 13);
        entities.Obligation.Description = db.GetNullableString(reader, 14);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 15);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 16);
        entities.Obligation.CreatedBy = db.GetString(reader, 17);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 18);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 20);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 21);
        entities.DebtDetail.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);

        return true;
      });
  }

  private bool ReadObligCollProtectionHist()
  {
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgIdentifier",
          export.Obligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "debtTypClass",
          local.HardcodeOtCVoluntaryClassifi.Classification);
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.SetDate(
          command, "deactivationDate", local.Blank.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 1);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 2);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 3);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 4);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 5);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          local.HardcodeVoluntary.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadSupported()
  {
    entities.Supported.Populated = false;

    return Read("ReadSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of SpNamePrompt.
      /// </summary>
      [JsonPropertyName("spNamePrompt")]
      public Common SpNamePrompt
      {
        get => spNamePrompt ??= new();
        set => spNamePrompt = value;
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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common spNamePrompt;
      private Common common;
      private CsePerson supportedCsePerson;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private ObligationTransaction obligationTransaction;
      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of HsupportedPersons.
    /// </summary>
    [JsonPropertyName("hsupportedPersons")]
    public Common HsupportedPersons
    {
      get => hsupportedPersons ??= new();
      set => hsupportedPersons = value;
    }

    /// <summary>
    /// A value of ObligationCreated.
    /// </summary>
    [JsonPropertyName("obligationCreated")]
    public DateWorkArea ObligationCreated
    {
      get => obligationCreated ??= new();
      set => obligationCreated = value;
    }

    /// <summary>
    /// A value of HiddenDiscontinueDate.
    /// </summary>
    [JsonPropertyName("hiddenDiscontinueDate")]
    public DateWorkArea HiddenDiscontinueDate
    {
      get => hiddenDiscontinueDate ??= new();
      set => hiddenDiscontinueDate = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Passed.
    /// </summary>
    [JsonPropertyName("passed")]
    public CsePersonsWorkSet Passed
    {
      get => passed ??= new();
      set => passed = value;
    }

    /// <summary>
    /// A value of Discontinued.
    /// </summary>
    [JsonPropertyName("discontinued")]
    public DateWorkArea Discontinued
    {
      get => discontinued ??= new();
      set => discontinued = value;
    }

    /// <summary>
    /// A value of Effective.
    /// </summary>
    [JsonPropertyName("effective")]
    public DateWorkArea Effective
    {
      get => effective ??= new();
      set => effective = value;
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
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
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
    /// A value of ManuallyDistribute.
    /// </summary>
    [JsonPropertyName("manuallyDistribute")]
    public Common ManuallyDistribute
    {
      get => manuallyDistribute ??= new();
      set => manuallyDistribute = value;
    }

    /// <summary>
    /// A value of ObligationActive.
    /// </summary>
    [JsonPropertyName("obligationActive")]
    public Common ObligationActive
    {
      get => obligationActive ??= new();
      set => obligationActive = value;
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
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Common HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of ObCollProtAct.
    /// </summary>
    [JsonPropertyName("obCollProtAct")]
    public Common ObCollProtAct
    {
      get => obCollProtAct ??= new();
      set => obCollProtAct = value;
    }

    private Common hsupportedPersons;
    private DateWorkArea obligationCreated;
    private DateWorkArea hiddenDiscontinueDate;
    private DateWorkArea hiddenEffDate;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private CsePersonsWorkSet passed;
    private DateWorkArea discontinued;
    private DateWorkArea effective;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson hiddenCsePerson;
    private Obligation obligation;
    private Common manuallyDistribute;
    private Common obligationActive;
    private Standard standard;
    private Common obligorPrompt;
    private Common hiddenPrev;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Common obCollProtAct;
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
      /// A value of SpNamePrompt.
      /// </summary>
      [JsonPropertyName("spNamePrompt")]
      public Common SpNamePrompt
      {
        get => spNamePrompt ??= new();
        set => spNamePrompt = value;
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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
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
      /// A value of ServiceProvider.
      /// </summary>
      [JsonPropertyName("serviceProvider")]
      public ServiceProvider ServiceProvider
      {
        get => serviceProvider ??= new();
        set => serviceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common spNamePrompt;
      private Common common;
      private CsePerson supportedCsePerson;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private ObligationTransaction obligationTransaction;
      private ServiceProvider serviceProvider;
    }

    /// <summary>
    /// A value of HsupportedPersons.
    /// </summary>
    [JsonPropertyName("hsupportedPersons")]
    public Common HsupportedPersons
    {
      get => hsupportedPersons ??= new();
      set => hsupportedPersons = value;
    }

    /// <summary>
    /// A value of ObligationCreated.
    /// </summary>
    [JsonPropertyName("obligationCreated")]
    public DateWorkArea ObligationCreated
    {
      get => obligationCreated ??= new();
      set => obligationCreated = value;
    }

    /// <summary>
    /// A value of HiddenDiscontinueDate.
    /// </summary>
    [JsonPropertyName("hiddenDiscontinueDate")]
    public DateWorkArea HiddenDiscontinueDate
    {
      get => hiddenDiscontinueDate ??= new();
      set => hiddenDiscontinueDate = value;
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
    /// A value of FlowObligationTransaction.
    /// </summary>
    [JsonPropertyName("flowObligationTransaction")]
    public ObligationTransaction FlowObligationTransaction
    {
      get => flowObligationTransaction ??= new();
      set => flowObligationTransaction = value;
    }

    /// <summary>
    /// A value of FlowObligationType.
    /// </summary>
    [JsonPropertyName("flowObligationType")]
    public ObligationType FlowObligationType
    {
      get => flowObligationType ??= new();
      set => flowObligationType = value;
    }

    /// <summary>
    /// A value of SuppPersonFlow.
    /// </summary>
    [JsonPropertyName("suppPersonFlow")]
    public CsePersonsWorkSet SuppPersonFlow
    {
      get => suppPersonFlow ??= new();
      set => suppPersonFlow = value;
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
    /// A value of Discontinued.
    /// </summary>
    [JsonPropertyName("discontinued")]
    public DateWorkArea Discontinued
    {
      get => discontinued ??= new();
      set => discontinued = value;
    }

    /// <summary>
    /// A value of Effective.
    /// </summary>
    [JsonPropertyName("effective")]
    public DateWorkArea Effective
    {
      get => effective ??= new();
      set => effective = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ManuallyDistribute.
    /// </summary>
    [JsonPropertyName("manuallyDistribute")]
    public Common ManuallyDistribute
    {
      get => manuallyDistribute ??= new();
      set => manuallyDistribute = value;
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
    /// A value of ObligationActive.
    /// </summary>
    [JsonPropertyName("obligationActive")]
    public Common ObligationActive
    {
      get => obligationActive ??= new();
      set => obligationActive = value;
    }

    /// <summary>
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of HiddenPrev.
    /// </summary>
    [JsonPropertyName("hiddenPrev")]
    public Common HiddenPrev
    {
      get => hiddenPrev ??= new();
      set => hiddenPrev = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of FlowSpTextWorkArea.
    /// </summary>
    [JsonPropertyName("flowSpTextWorkArea")]
    public SpTextWorkArea FlowSpTextWorkArea
    {
      get => flowSpTextWorkArea ??= new();
      set => flowSpTextWorkArea = value;
    }

    /// <summary>
    /// A value of FlowCsePersonAccount.
    /// </summary>
    [JsonPropertyName("flowCsePersonAccount")]
    public CsePersonAccount FlowCsePersonAccount
    {
      get => flowCsePersonAccount ??= new();
      set => flowCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ObCollProtAct.
    /// </summary>
    [JsonPropertyName("obCollProtAct")]
    public Common ObCollProtAct
    {
      get => obCollProtAct ??= new();
      set => obCollProtAct = value;
    }

    private Common hsupportedPersons;
    private DateWorkArea obligationCreated;
    private DateWorkArea hiddenDiscontinueDate;
    private DateWorkArea hiddenEffDate;
    private ObligationTransaction flowObligationTransaction;
    private ObligationType flowObligationType;
    private CsePersonsWorkSet suppPersonFlow;
    private DebtDetail debtDetail;
    private DateWorkArea discontinued;
    private DateWorkArea effective;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Obligation obligation;
    private Common manuallyDistribute;
    private Standard standard;
    private Common obligationActive;
    private Common obligorPrompt;
    private Common hiddenPrev;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private SpTextWorkArea flowSpTextWorkArea;
    private CsePersonAccount flowCsePersonAccount;
    private Common obCollProtAct;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of PercentageAllocated.
    /// </summary>
    [JsonPropertyName("percentageAllocated")]
    public Common PercentageAllocated
    {
      get => percentageAllocated ??= new();
      set => percentageAllocated = value;
    }

    /// <summary>
    /// A value of NumberOfSupportedPersns.
    /// </summary>
    [JsonPropertyName("numberOfSupportedPersns")]
    public Common NumberOfSupportedPersns
    {
      get => numberOfSupportedPersns ??= new();
      set => numberOfSupportedPersns = value;
    }

    /// <summary>
    /// A value of HardcodeOtCAccruing.
    /// </summary>
    [JsonPropertyName("hardcodeOtCAccruing")]
    public ObligationType HardcodeOtCAccruing
    {
      get => hardcodeOtCAccruing ??= new();
      set => hardcodeOtCAccruing = value;
    }

    /// <summary>
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public AbendData Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    /// <summary>
    /// A value of HardcodedObligorLap.
    /// </summary>
    [JsonPropertyName("hardcodedObligorLap")]
    public LegalActionPerson HardcodedObligorLap
    {
      get => hardcodedObligorLap ??= new();
      set => hardcodedObligorLap = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDtAccrual.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtAccrual")]
    public ObligationTransaction HardcodeOtrnDtAccrual
    {
      get => hardcodeOtrnDtAccrual ??= new();
      set => hardcodeOtrnDtAccrual = value;
    }

    /// <summary>
    /// A value of HardcodeDdshActiveStatus.
    /// </summary>
    [JsonPropertyName("hardcodeDdshActiveStatus")]
    public DebtDetailStatusHistory HardcodeDdshActiveStatus
    {
      get => hardcodeDdshActiveStatus ??= new();
      set => hardcodeDdshActiveStatus = value;
    }

    /// <summary>
    /// A value of HardcodeOtCFeesClassificatio.
    /// </summary>
    [JsonPropertyName("hardcodeOtCFeesClassificatio")]
    public ObligationType HardcodeOtCFeesClassificatio
    {
      get => hardcodeOtCFeesClassificatio ??= new();
      set => hardcodeOtCFeesClassificatio = value;
    }

    /// <summary>
    /// A value of HardcodeOtCRecoverClassifica.
    /// </summary>
    [JsonPropertyName("hardcodeOtCRecoverClassifica")]
    public ObligationType HardcodeOtCRecoverClassifica
    {
      get => hardcodeOtCRecoverClassifica ??= new();
      set => hardcodeOtCRecoverClassifica = value;
    }

    /// <summary>
    /// A value of HardcodeOt718BUraJudgement.
    /// </summary>
    [JsonPropertyName("hardcodeOt718BUraJudgement")]
    public ObligationType HardcodeOt718BUraJudgement
    {
      get => hardcodeOt718BUraJudgement ??= new();
      set => hardcodeOt718BUraJudgement = value;
    }

    /// <summary>
    /// A value of HardcodeOtCVoluntaryClassifi.
    /// </summary>
    [JsonPropertyName("hardcodeOtCVoluntaryClassifi")]
    public ObligationType HardcodeOtCVoluntaryClassifi
    {
      get => hardcodeOtCVoluntaryClassifi ??= new();
      set => hardcodeOtCVoluntaryClassifi = value;
    }

    /// <summary>
    /// A value of HardcodeOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hardcodeOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HardcodeOtrrConcurrentObliga
    {
      get => hardcodeOtrrConcurrentObliga ??= new();
      set => hardcodeOtrrConcurrentObliga = value;
    }

    /// <summary>
    /// A value of SelectedToBeDeleted.
    /// </summary>
    [JsonPropertyName("selectedToBeDeleted")]
    public CsePerson SelectedToBeDeleted
    {
      get => selectedToBeDeleted ??= new();
      set => selectedToBeDeleted = value;
    }

    /// <summary>
    /// A value of SelCount.
    /// </summary>
    [JsonPropertyName("selCount")]
    public Common SelCount
    {
      get => selCount ??= new();
      set => selCount = value;
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of CseApplied.
    /// </summary>
    [JsonPropertyName("cseApplied")]
    public Common CseApplied
    {
      get => cseApplied ??= new();
      set => cseApplied = value;
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
    /// A value of DuplicateSpCheck.
    /// </summary>
    [JsonPropertyName("duplicateSpCheck")]
    public Common DuplicateSpCheck
    {
      get => duplicateSpCheck ??= new();
      set => duplicateSpCheck = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public Common Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of HardcodeVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodeVoluntary")]
    public ObligationType HardcodeVoluntary
    {
      get => hardcodeVoluntary ??= new();
      set => hardcodeVoluntary = value;
    }

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
    /// A value of HardcodeDebtType.
    /// </summary>
    [JsonPropertyName("hardcodeDebtType")]
    public ObligationTransaction HardcodeDebtType
    {
      get => hardcodeDebtType ??= new();
      set => hardcodeDebtType = value;
    }

    /// <summary>
    /// A value of HardcodeOtVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodeOtVoluntary")]
    public ObligationTransaction HardcodeOtVoluntary
    {
      get => hardcodeOtVoluntary ??= new();
      set => hardcodeOtVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtDebtDetail")]
    public ObligationTransaction HardcodeOtrnDtDebtDetail
    {
      get => hardcodeOtrnDtDebtDetail ??= new();
      set => hardcodeOtrnDtDebtDetail = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of FromHistMonaNxttran.
    /// </summary>
    [JsonPropertyName("fromHistMonaNxttran")]
    public Common FromHistMonaNxttran
    {
      get => fromHistMonaNxttran ??= new();
      set => fromHistMonaNxttran = value;
    }

    /// <summary>
    /// A value of HardcodeSupported.
    /// </summary>
    [JsonPropertyName("hardcodeSupported")]
    public CsePersonAccount HardcodeSupported
    {
      get => hardcodeSupported ??= new();
      set => hardcodeSupported = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      percentageAllocated = null;
      numberOfSupportedPersns = null;
      hardcodeOtCAccruing = null;
      eab = null;
      hardcodedObligorLap = null;
      hardcodeOtrnDtAccrual = null;
      hardcodeDdshActiveStatus = null;
      hardcodeOtCFeesClassificatio = null;
      hardcodeOtCRecoverClassifica = null;
      hardcodeOt718BUraJudgement = null;
      hardcodeOtCVoluntaryClassifi = null;
      selectedToBeDeleted = null;
      selCount = null;
      cseApplied = null;
      select = null;
      duplicateSpCheck = null;
      csePersonsWorkSet = null;
      child = null;
      temp = null;
      blank = null;
      infrastructure = null;
      fromHistMonaNxttran = null;
    }

    private Common percentageAllocated;
    private Common numberOfSupportedPersns;
    private ObligationType hardcodeOtCAccruing;
    private AbendData eab;
    private LegalActionPerson hardcodedObligorLap;
    private ObligationTransaction hardcodeOtrnDtAccrual;
    private DebtDetailStatusHistory hardcodeDdshActiveStatus;
    private ObligationType hardcodeOtCFeesClassificatio;
    private ObligationType hardcodeOtCRecoverClassifica;
    private ObligationType hardcodeOt718BUraJudgement;
    private ObligationType hardcodeOtCVoluntaryClassifi;
    private ObligationTransactionRlnRsn hardcodeOtrrConcurrentObliga;
    private CsePerson selectedToBeDeleted;
    private Common selCount;
    private DateWorkArea current;
    private TextWorkArea zeroFill;
    private Common cseApplied;
    private Common select;
    private Common duplicateSpCheck;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common child;
    private ObligationType hardcodeVoluntary;
    private CsePersonAccount hardcodeObligor;
    private ObligationTransaction hardcodeDebtType;
    private ObligationTransaction hardcodeOtVoluntary;
    private ObligationTransaction hardcodeOtrnDtDebtDetail;
    private Common temp;
    private DateWorkArea maximum;
    private DateWorkArea blank;
    private Infrastructure infrastructure;
    private Common fromHistMonaNxttran;
    private CsePersonAccount hardcodeSupported;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
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
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
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
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private ObligationType obligationType;
    private CaseUnit caseUnit;
    private CsePersonAccount supported;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private CsePerson obligorCsePerson;
    private CsePerson supportedCsePerson;
    private CsePersonAccount supportedCsePersonAccount;
    private ObligCollProtectionHist obligCollProtectionHist;
    private CsePersonAccount obligorCsePersonAccount;
  }
#endregion
}
